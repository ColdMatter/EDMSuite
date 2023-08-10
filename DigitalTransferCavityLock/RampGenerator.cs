using System;
using System.Collections.Generic;
using System.Linq;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace DigitalTransferCavityLock
{
    public class RampGenerator
    {
        public Controller controller;

        public string syncCounter;
        public string rampOut;
        public string sharedTimebase;
        public int timebaseFrequency;
        public string resetOutput = "";

        public CounterChannel ResetOutput
        {
            get
            {
                if (resetOutput == "") return null;
                return (CounterChannel)Environs.Hardware.CounterChannels[resetOutput];
            }
        }

        public AnalogOutputChannel RampOut
        {
            get
            {
                return (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[rampOut];
            }
        }

        public CounterChannel SyncCounter
        {
            get
            {
                return (CounterChannel)Environs.Hardware.CounterChannels[syncCounter];
            }
        }

        public RampGenerator(string _rampOut, string _syncCounter, string timebase, int timebaseFreq, Controller cont)
        {
            controller = cont;
            rampOut = _rampOut;
            syncCounter = _syncCounter;
            sharedTimebase = timebase;
            timebaseFrequency = timebaseFreq;
            if (timebaseFreq < 1e6)
                throw new Exception("Timebase must be faster than sampling clock! set it above 1e6");
        }

        public RampGenerator(string _rampOut, string _syncCounter, string timebase, int timebaseFreq, Controller cont, string resetOut)
            : this(_rampOut, _syncCounter, timebase, timebaseFreq, cont)
        {
            resetOutput = resetOut;
        }


        private Task rampTask;
        private Task syncTask;
        private AnalogSingleChannelWriter rampWriter;

        public int GetSamplesPerHalfPeriod(double frequency)
        {
            return (int)Math.Floor(500000 / frequency);
        }


        public int samplesPerHalfPeriod;
        public double Amplitude;
        public double Offset;

        public double periodMS
        {
            get
            {
                return samplesPerHalfPeriod / 500;
            }
        }

        List<double> rampPattern = new List<double>();
        List<double> rampX = new List<double>();
        public int SetUpTasks(double amplitude, double frequency, double offset) // Returns samples per half period (aka half period in units of us)
        {
            samplesPerHalfPeriod = GetSamplesPerHalfPeriod(frequency);
            Amplitude = amplitude;
            Offset = offset;

            rampTask = new Task("Ramp task");
            if (offset < RampOut.RangeLow || offset + amplitude > RampOut.RangeHigh || offset > RampOut.RangeHigh || offset + amplitude < RampOut.RangeLow)
                throw new Exception("Ramp cannot exceed output channel limits");
            AOChannel outChannel = rampTask.AOChannels.CreateVoltageChannel(RampOut.PhysicalChannel, RampOut.Name, Math.Min(offset, offset + amplitude), Math.Max(offset, offset + amplitude), AOVoltageUnits.Volts);
            rampTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(SyncCounter.PhysicalChannel + "InternalOutput", DigitalEdgeStartTriggerEdge.Rising);
            rampPattern.Clear();
            rampX.Clear();
            double val = offset;
            for (int i = 0; i < samplesPerHalfPeriod; ++i)
            {
                rampPattern.Add(val);
                val = val + (double)amplitude / samplesPerHalfPeriod;
                rampX.Add((double)i / 1000);
            }
            for (int i = 0; i < samplesPerHalfPeriod; ++i)
            {
                rampPattern.Add(val);
                val = val - (double)amplitude / samplesPerHalfPeriod;
                rampX.Add(((double)samplesPerHalfPeriod + i - 1) / 1000);
            }
            //rampTask.Timing.ReferenceClockRate = 10e6;
            rampTask.Timing.ConfigureSampleClock(string.Empty, 1000000, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples);
            rampTask.Timing.SampleClockTimebaseSource = sharedTimebase;
            //rampTask.Stream.Buffer.OutputBufferSize = 1000;
            //rampTask.Stream.Buffer.OutputOnBoardBufferSize = 1000;
            rampTask.Control(TaskAction.Verify);
            rampTask.Control(TaskAction.Commit);
            rampWriter = new AnalogSingleChannelWriter(rampTask.Stream);
            rampWriter.WriteMultiSample(true, rampPattern.ToArray());

            syncTask = new Task("Sync task");
            //COChannel freqChannel = syncTask.COChannels.CreatePulseChannelFrequency("/PXI1Slot5/ctr2", "ref", COPulseFrequencyUnits.Hertz, COPulseIdleState.High, 0, 200, 0.5);
            COChannel freqChannel = syncTask.COChannels.CreatePulseChannelTicks(SyncCounter.PhysicalChannel, SyncCounter.Name, "", COPulseIdleState.Low, 0, (timebaseFrequency / 1000000) * samplesPerHalfPeriod, (timebaseFrequency / 1000000) * samplesPerHalfPeriod);
            freqChannel.CounterTimebaseSource = sharedTimebase;
            if (ResetOutput != null)
                freqChannel.PulseTerminal = ResetOutput.PhysicalChannel;
            syncTask.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);
            syncTask.Triggers.StartTrigger.ConfigureNone();
            syncTask.Timing.SampleQuantityMode = SampleQuantityMode.ContinuousSamples;

            syncTask.CounterOutput += (object ob, CounterOutputEventArgs e) => { controller.updateReady = true; ++controller.loopCount; };
            //syncTask.EveryNSamplesWrittenEventInterval = 2;
            //syncTask.EveryNSamplesWritten += (object ob, EveryNSamplesWrittenEventArgs e) => { controller.Update(); };

            syncTask.Control(TaskAction.Verify);
            syncTask.Control(TaskAction.Commit);
            syncTask.Start();

            return samplesPerHalfPeriod;
        }

        public double ConvertToVoltage(double timeMS) // Fix bug
        {
            timeMS = timeMS % (samplesPerHalfPeriod / 500);
            if (timeMS < samplesPerHalfPeriod / 1000)
                return (timeMS * 1000 / samplesPerHalfPeriod) * Amplitude + Offset;
            return (2 - timeMS * 1000 / samplesPerHalfPeriod) * Amplitude + Offset;
        }

        public void StopTasks()
        {
            rampTask.Dispose();
            syncTask.Dispose();
        }

        public void UpdateRampPlot(ControlWindow win, bool active)
        {
            if (!active)
            {
                rampPattern.Clear();
                for (int i = 0; i < samplesPerHalfPeriod; ++i)
                {
                    rampPattern.Add(0);
                }
                for (int i = 0; i < samplesPerHalfPeriod; ++i)
                {
                    rampPattern.Add(0);
                }
            }
            win.UpdateRenderedObject<NationalInstruments.UI.WindowsForms.ScatterGraph>(win.PeakPlot, (NationalInstruments.UI.WindowsForms.ScatterGraph g) => { g.Plots[0].PlotXY(rampX.ToArray(), rampPattern.ToArray()); });
        }

    }
}
