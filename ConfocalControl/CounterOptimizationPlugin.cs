using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

using MathNet.Numerics.Optimization;

namespace ConfocalControl
{

    // Uses delegate multicasting to compose and invoke event manager methods in series 
    public delegate void CounterOptimizationDataEventHandler(MultiChannelData ps);
    public delegate void CounterOptimizationFinishedEventHandler();

    class CounterOptimizationPlugin
    {
        // Dependencies should refer to this instance only 
        private static CounterOptimizationPlugin controllerInstance;
        public static CounterOptimizationPlugin GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new CounterOptimizationPlugin();
            }
            return controllerInstance;
        }

        // Constants relating to sample acquisition
        private int MINNUMBEROFSAMPLES = 10;
        private double TRUESAMPLERATE = 1000;
        private int pointsPerExposure;
        private double sampleRate;

        // Keep track of tasks
        private Task triggerTask;
        private DigitalSingleChannelWriter triggerWriter;
        private Task freqOutTask;
        private string counterChannel;
        private Task counterTask;
        private CounterSingleChannelReader counterReader;

        public CounterOptimizationPlugin()
        {
            triggerTask = null;
            freqOutTask = null;
            counterTask = null;

            triggerWriter = null;
            counterReader = null;
        }

        private void CalculateParameters()
        {
            double _sampleRate = (double)SingleCounterPlugin.GetController().Settings["sampleRate"];
            if (_sampleRate * MINNUMBEROFSAMPLES >= TRUESAMPLERATE)
            {
                pointsPerExposure = MINNUMBEROFSAMPLES;
                sampleRate = _sampleRate * pointsPerExposure;
            }
            else
            {
                pointsPerExposure = Convert.ToInt32(TRUESAMPLERATE / _sampleRate);
                sampleRate = _sampleRate * pointsPerExposure;
            }
        }

        public void OptimizationStarting(string countChannel)
        {
            counterChannel = countChannel;

            // Define sample rate 
            CalculateParameters();

            // Set up trigger task
            triggerTask = new Task("pause trigger task");

            // Digital output
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["StartTrigger"]).AddToTask(triggerTask);

            triggerTask.Control(TaskAction.Verify);

            DaqStream triggerStream = triggerTask.Stream;
            triggerWriter = new DigitalSingleChannelWriter(triggerStream);

            triggerWriter.WriteSingleSampleSingleLine(true, false);

            // Set up clock task
            freqOutTask = new Task("sample clock task");

            // Finite pulse train
            freqOutTask.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["SampleClock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                sampleRate,
                0.5);

            freqOutTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("StartTriggerReader"),
                DigitalEdgeStartTriggerEdge.Rising);

            freqOutTask.Triggers.StartTrigger.Retriggerable = true;


            freqOutTask.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, pointsPerExposure + 1);

            freqOutTask.Control(TaskAction.Verify);

            freqOutTask.Start();

            // Set up edge-counting tasks
            counterTask = new Task("buffered edge counters " + counterChannel);

            // Count upwards on rising edges starting from zero
            counterTask.CIChannels.CreateCountEdgesChannel(
                ((CounterChannel)Environs.Hardware.CounterChannels[counterChannel]).PhysicalChannel,
                "edge counter " + counterChannel,
                CICountEdgesActiveEdge.Rising,
                0,
                CICountEdgesCountDirection.Up);

            // Take one sample within a window determined by sample rate using clock task
            counterTask.Timing.ConfigureSampleClock(
                (string)Environs.Hardware.GetInfo("SampleClockReader"),
                sampleRate,
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.ContinuousSamples);

            counterTask.Control(TaskAction.Verify);

            DaqStream counterStream = counterTask.Stream;
            counterReader = new CounterSingleChannelReader(counterStream);

            // Start tasks
            counterTask.Start();

            // Start Galvos
            GalvoPairPlugin.GetController().MoveOnlyAcquisitionStarting();
        }

        public double EvaluateAt(double Xpos, double Ypos)
        {
            GalvoPairPlugin.GetController().SetGalvoXSetpoint(Xpos);
            GalvoPairPlugin.GetController().SetGalvoYSetpoint(Ypos);

            // Start trigger task
            triggerWriter.WriteSingleSampleSingleLine(true, true);

            double[] counterLatestData = counterReader.ReadMultiSampleDouble(pointsPerExposure + 1);

            // re-init the trigger 
            triggerWriter.WriteSingleSampleSingleLine(true, false);

            return counterLatestData[counterLatestData.Length - 1] - counterLatestData[0];
        }

        public void FindOptimum()
        {
            return;
        }

        public void OptimizationEnding()
        {
            GalvoPairPlugin.GetController().MoveOnlyAcquisitionFinished();

            triggerTask.Dispose();
            freqOutTask.Dispose();
            counterTask.Dispose();

            triggerTask = null;
            freqOutTask = null;
            counterTask = null;

            triggerWriter = null;
            counterReader = null;
        }
    }




    class CounterObjectiveFunction : IObjectiveFunction
    {
        public CounterObjectiveFunction()
        {

        }

        void IObjectiveFunction.EvaluateAt(MathNet.Numerics.LinearAlgebra.Vector<double> point)
        {
            currentPoint = point;

            double currentGalvoXpoint = point[0];
            double currentGalvoYpoint = point[1];

            currentValue = CounterOptimizationPlugin.GetController().EvaluateAt(currentGalvoXpoint, currentGalvoYpoint);
        }

        IObjectiveFunction IObjectiveFunction.Fork()
        {
            throw new NotImplementedException();
        }

        IObjectiveFunction IObjectiveFunctionEvaluation.CreateNew()
        {
            throw new NotImplementedException();
        }

        MathNet.Numerics.LinearAlgebra.Vector<double> IObjectiveFunctionEvaluation.Gradient
        {
            get { throw new NotImplementedException(); }
        }

        MathNet.Numerics.LinearAlgebra.Matrix<double> IObjectiveFunctionEvaluation.Hessian
        {
            get { throw new NotImplementedException(); }
        }

        private bool noGradient = false;
        bool IObjectiveFunctionEvaluation.IsGradientSupported
        {
            get { return noGradient; }
        }

        private bool noHessian = false;
        bool IObjectiveFunctionEvaluation.IsHessianSupported
        {
            get { return noHessian; }
        }

        private MathNet.Numerics.LinearAlgebra.Vector<double> currentPoint;
        MathNet.Numerics.LinearAlgebra.Vector<double> IObjectiveFunctionEvaluation.Point
        {
            get { return currentPoint; }
        }

        private double currentValue;
        double IObjectiveFunctionEvaluation.Value
        {
            get { return currentValue; }
        }
    }
}
