using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Threading;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

using Accord.Math.Optimization;
using Accord.Math.Convergence;

namespace ConfocalControl
{

    // Uses delegate multicasting to compose and invoke event manager methods in series 
    public delegate void CounterOptimizationDataEventHandler(Point3D[] ptns);
    public delegate void CounterOptimizationFinishedEventHandler();
    public delegate void OptExceptionEventHandler(DaqException e);

    class CounterOptimizationPlugin
    {
        #region Class members

        // Bound event managers to class
        public event CounterOptimizationDataEventHandler Data;
        public event CounterOptimizationFinishedEventHandler OptFinished;
        public event OptExceptionEventHandler DaqProblem;

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

        // Define Opt state
        private enum OptimizationState { stopped, running, stopping };
        private OptimizationState backendState = OptimizationState.stopped;

        // Constants relating to sample acquisition
        private int MINNUMBEROFSAMPLES = 10;
        private double TRUESAMPLERATE = 1000;
        private int pointsPerExposure;
        private double sampleRate;

        // Keep track of optimizer
        private double[] startParams;
        private NelderMead nonLinearOptimizer;
        private CancellationTokenSource tokenSource;

        // Keep track of tasks
        private List<Point3D> pointHistory;
        private Task triggerTask;
        private DigitalSingleChannelWriter triggerWriter;
        private Task freqOutTask;
        private string counterChannel;
        private Task counterTask;
        private CounterSingleChannelReader counterReader;

        #endregion

        public CounterOptimizationPlugin()
        {
            startParams = null;
            nonLinearOptimizer = null;
            tokenSource = null;

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
            if (IsRunning() || SingleCounterPlugin.GetController().IsRunning() || FastMultiChannelRasterScan.GetController().IsRunning())
            {
                throw new DaqException("Counter already running");
            }

            backendState = OptimizationState.running;

            counterChannel = countChannel;

            pointHistory = new List<Point3D>();

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

        private double EvaluateAt(double[] _optParams)
        {
            nonLinearOptimizer.Token.ThrowIfCancellationRequested();

            double[] optParams = ParameterInverseTransform(_optParams);

            double Xpos = optParams[0]; double Ypos = optParams[1];

            GalvoPairPlugin.GetController().SetGalvoXSetpoint(Xpos);
            GalvoPairPlugin.GetController().SetGalvoYSetpoint(Ypos);

            // Start trigger task
            triggerWriter.WriteSingleSampleSingleLine(true, true);

            double[] counterLatestData = counterReader.ReadMultiSampleDouble(pointsPerExposure + 1);

            // re-init the trigger 
            triggerWriter.WriteSingleSampleSingleLine(true, false);

            double result = counterLatestData[counterLatestData.Length - 1] - counterLatestData[0];

            Point3D outPoint = new Point3D(Xpos, Ypos, result);
            pointHistory.Add(outPoint);
            OnData(pointHistory.ToArray());

            return result;
        }

        private double[] ParameterTransform(double[] untransformed)
        {
            double hrange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"] - (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"];
            double vrange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"] - (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"];

            double xtransformed = (untransformed[0] - startParams[0]) / 5000;
            double ytransformed = (untransformed[1] - startParams[1]) / 5000;

            return new double[] { xtransformed, ytransformed };
        }

        private double[] ParameterInverseTransform(double[] transformed)
        {
            double hrange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXEnd"] - (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoXStart"];
            double vrange = (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYEnd"] - (double)FastMultiChannelRasterScan.GetController().scanSettings["GalvoYStart"];

            double xuntransformed = transformed[0] * 5000 + startParams[0];
            double yuntransformed = transformed[1] * 5000 + startParams[1];

            return new double[] { xuntransformed, yuntransformed };
        }

        public void FindOptimum(double cursorX, double cursorY, double cursorZ)
        {
            startParams = new double[] { cursorX, cursorY };

            tokenSource = new CancellationTokenSource();

            Func<double[], double> objFunction = (double[] x) => EvaluateAt(x);

            GeneralConvergence convergence = new GeneralConvergence(2);
            convergence.MaximumEvaluations = 1000;
            convergence.RelativeParameterTolerance = 0.0001;

            nonLinearOptimizer = new NelderMead(2, objFunction);
            nonLinearOptimizer.Convergence = convergence;
            nonLinearOptimizer.Token = tokenSource.Token;

            try
            {
                nonLinearOptimizer.Maximize(ParameterTransform(startParams));
            }
            
            catch(DaqException e1)
            {
                if (DaqProblem != null) DaqProblem(e1);
            }
            
            catch (OperationCanceledException) { }

            finally { OptimizationEnding(); }
        }

        private void OptimizationEnding()
        {
            startParams = null;
            nonLinearOptimizer = null;
            tokenSource = null;

            GalvoPairPlugin.GetController().MoveOnlyAcquisitionFinished();

            triggerTask.Dispose();
            freqOutTask.Dispose();
            counterTask.Dispose();

            triggerTask = null;
            freqOutTask = null;
            counterTask = null;

            triggerWriter = null;
            counterReader = null;

            backendState = OptimizationState.stopped;

            OnOptFinished();
        }

        public void StopOptimizing()
        {
            tokenSource.Cancel();
            backendState = OptimizationState.stopping;
        }

        private void OnData(Point3D[] ps)
        {
            if (Data != null) Data(ps);
        }

        private void OnOptFinished()
        {
            if (OptFinished != null) OptFinished();
        }

        public bool IsRunning()
        {
            return backendState == OptimizationState.running;
        }
    }
}
