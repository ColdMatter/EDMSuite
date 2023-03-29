using System;
using System.Linq;
using System.Collections.Generic;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace DigitalTransferCavityLock
{
    public class Controller
    {
        private UInt32 referencePeakCount = UInt32.MaxValue;
        private UInt32 slavePeakCount = UInt32.MaxValue;
        public Cavity reference;
        public Laser slave;
        private CounterSingleChannelReader refReader;
        private CounterSingleChannelReader refReader2;
        private AnalogSingleChannelWriter rampWriter;
        private NationalInstruments.AnalogWaveform<double> rampWaveform;
        public ControlWindow window;

        private Task syncTask;
        private Task rampTask;
        private Task referenceTask;
        private Task referenceTask2;

        public UInt32 NormalisedReferencePeakCount
        {
            get
            {
                return referencePeakCount;
            }
        }

        public UInt32 NormalisedSlavePeakCount
        {
            get
            {
                return slavePeakCount;
            }
        }

        public double referencePeakMS
        {
            get
            {
                return (double)NormalisedReferencePeakCount / 5000;
            }
        }
        public double referencePeakV
        {
            get
            {
                return (double)NormalisedReferencePeakCount * (double)4 / 2500;
            }
        }

        public double slavePeakMS
        {
            get
            {
                return (double)NormalisedSlavePeakCount / 5000;
            }
        }
        public double slavePeakV
        {
            get
            {
                return (double)NormalisedSlavePeakCount * (double)4 / 25000;
            }
        }

        public Controller()
        {
            reference = new Cavity(() => { return referencePeakV; }, "tclCavityLengthVoltage");
            slave = new Laser(() => { return slavePeakV; }, "tclOut", reference);
        }

        public void CreateCounterTasks()
        {
            referenceTask = new Task("Reference Task");

            CIChannel MainCounter = referenceTask.CIChannels.CreateCountEdgesChannel("/PXI1Slot5/ctr1", "counter", CICountEdgesActiveEdge.Rising, 0, CICountEdgesCountDirection.Up);
            MainCounter.CountEdgesTerminal = "/PXI1Slot5/10MHzRefClock";
            MainCounter.CountEdgesCountResetEnable = true;
            MainCounter.CountEdgesCountResetTerminal = "/PXI1Slot5/ctr2InternalOutput";
            MainCounter.CountEdgesCountResetActiveEdge = CICountEdgesCountResetActiveEdge.Rising;
            MainCounter.CountEdgesCountResetResetCount = 0;
            MainCounter.CountEdgesCountResetDigitalSynchronizationEnable = false;

            referenceTask.Timing.ReferenceClockRate = 10e6;
            referenceTask.Timing.ConfigureSampleClock("/PXI1Slot5/PFI1", 10000000, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, 1);
            referenceTask.Stream.Buffer.InputBufferSize = 2000;
            referenceTask.Timing.SampleClockTimebaseSource = "/PXI1Slot5/20MHzTimebase";
            referenceTask.Triggers.ArmStartTrigger.ConfigureDigitalEdgeTrigger("/PXI1Slot5/ctr2InternalOutput", DigitalEdgeArmStartTriggerEdge.Rising);
            referenceTask.Stream.Timeout = 5;

            referenceTask.Control(TaskAction.Verify);
            referenceTask.Control(TaskAction.Commit);

            refReader = new CounterSingleChannelReader(referenceTask.Stream);
            //refReader.SynchronizeCallbacks = false;
            //refReader.BeginReadMultiSampleInt32(1, new AsyncCallback(CounterCallback), null);


            referenceTask2 = new Task("Reference Task2");

            CIChannel MainCounter2 = referenceTask2.CIChannels.CreateCountEdgesChannel("/PXI1Slot5/ctr3", "counter2", CICountEdgesActiveEdge.Rising, 0, CICountEdgesCountDirection.Up);
            MainCounter2.CountEdgesTerminal = "/PXI1Slot5/10MHzRefClock";
            MainCounter2.CountEdgesCountResetEnable = true;
            MainCounter2.CountEdgesCountResetTerminal = "/PXI1Slot5/ctr2InternalOutput";
            MainCounter2.CountEdgesCountResetActiveEdge = CICountEdgesCountResetActiveEdge.Rising;
            MainCounter2.CountEdgesCountResetResetCount = 0;
            MainCounter2.CountEdgesCountResetDigitalSynchronizationEnable = false;

            /*
            CIChannel EdgeSepCounter = referenceTask2.CIChannels.CreateTwoEdgeSeparationChannel("/PXI1Slot5/ctr3", "edgeSep", 50e-9, 1e-3, CITwoEdgeSeparationFirstEdge.Rising, CITwoEdgeSeparationSecondEdge.Rising, CITwoEdgeSeparationUnits.Seconds);
            EdgeSepCounter.TwoEdgeSeparationFirstTerminal = "/PXI1Slot5/PFI1";
            EdgeSepCounter.TwoEdgeSeparationSecondTerminal = "/PXI1Slot5/PFI2";
            */
            referenceTask2.Timing.ReferenceClockRate = 10e6;
            referenceTask2.Timing.ConfigureSampleClock("/PXI1Slot5/PFI15", 10000000, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, 1);
            referenceTask2.Stream.Buffer.InputBufferSize = 2000;
            referenceTask2.Timing.SampleClockTimebaseSource = "/PXI1Slot5/20MHzTimebase";
            referenceTask2.Triggers.ArmStartTrigger.ConfigureDigitalEdgeTrigger("/PXI1Slot5/ctr2InternalOutput", DigitalEdgeArmStartTriggerEdge.Rising);
            referenceTask2.Stream.Timeout = 5;

            referenceTask2.Control(TaskAction.Verify);
            referenceTask2.Control(TaskAction.Commit);

            refReader2 = new CounterSingleChannelReader(referenceTask2.Stream);
            //refReader2.SynchronizeCallbacks = false;
            //refReader2.BeginReadMultiSampleInt32(1, new AsyncCallback(CounterCallback2), null);

            syncTask = new Task("helper task");
            COChannel freqChannel = syncTask.COChannels.CreatePulseChannelFrequency("/PXI1Slot5/ctr2", "ref", COPulseFrequencyUnits.Hertz, COPulseIdleState.High, 0, 200, 0.5);
            freqChannel.CounterTimebaseSource = "/PXI1Slot5/20MHzTimebase";
            syncTask.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples);
            syncTask.Triggers.StartTrigger.ConfigureNone();
            syncTask.Timing.SampleQuantityMode = SampleQuantityMode.ContinuousSamples;
            syncTask.Control(TaskAction.Verify);
            syncTask.Control(TaskAction.Commit);
            syncTask.Start();
        }

        public int count1 = 0;
        public int count2 = 0;
        private bool counted1 = true;
        private bool counted2 = true;
        private bool fail1 = false;
        private bool fail2 = false;
        public void CounterCallback(IAsyncResult res)
        {
            switch ((int)res.AsyncState)
            {
                case 0:
                    fail1 = false;
                    try
                    {
                        referencePeakCount = refReader.EndReadSingleSampleUInt32(res);
                    }
                    catch (DaqException)
                    {
                        fail1 = true;
                    }
                    counted1 = true;
                    break;
                case 1:
                    fail2 = false;
                    try
                    {
                        slavePeakCount = refReader.EndReadSingleSampleUInt32(res);
                    }
                    catch (DaqException)
                    {
                        fail2 = true;
                    }
                    counted2 = true;
                    break;
            }

        }

        public void CounterCallback2(IAsyncResult res)
        {
            Int32[] data;
            data = refReader2.EndReadMultiSampleInt32(res);

            //Console.Write("a");
            //Console.WriteLine(data[0] % 10000);
            //Console.WriteLine();
            count2 = data[0] % 10000;
            Console.Write(count1);
            Console.Write(" ");
            Console.WriteLine(count2 - count1);
            refReader2.BeginReadMultiSampleInt32(1, new AsyncCallback(CounterCallback2), null);
        }

        private int a = 0;

        public void ReadSamples()
        {
            a++;
            counted1 = false;
            counted2 = false;
            refReader.BeginReadSingleSampleUInt32(CounterCallback,0);
            refReader2.BeginReadSingleSampleUInt32(CounterCallback,1);
            while((!counted1 || !counted2))
            {
                Thread.Sleep(0);
            }
            if (a % 100 == 0)
            {
                if (!fail1)
                {
                    window.SetTextField(window.refLocMS, referencePeakMS.ToString());
                    window.SetTextField(window.refLocV, referencePeakV.ToString());
                }
                else
                {
                    window.SetTextField(window.refLocMS, "No Data");
                    window.SetTextField(window.refLocV, "No Data");
                }
                if (!fail2)
                {
                    window.SetTextField(window.slaveLocMS, slavePeakMS.ToString());
                    window.SetTextField(window.slaveLocV, slavePeakV.ToString());
                }
                else
                {
                    window.SetTextField(window.slaveLocMS, "No Data");
                    window.SetTextField(window.slaveLocV, "No Data");
                }
            }
            //rampTask.Stop();
            //referenceTask.Stop();
            //referenceTask2.Stop();
            //syncTask.Stop();
            //rampTask.Start();
            //referenceTask.Start();
            //referenceTask2.Start();
            //syncTask.Start();
        }

        public void CreateRampTask()
        {
            rampTask = new Task("Ramp task");
            AOChannel outChannel = rampTask.AOChannels.CreateVoltageChannel("/PXI1Slot5/AO0", "RampOut", 0, 4, AOVoltageUnits.Volts);
            rampTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger("/PXI1Slot5/ctr2InternalOutput",DigitalEdgeStartTriggerEdge.Rising);
            List<double> rampPattern = new List<double>();
            double val = 0;
            for(int i = 0; i < 2500; ++i)
            {
                rampPattern.Add(val);
                val = val + (double)4 / 2500;
            }
            for (int i = 0; i < 2500; ++i)
            {
                rampPattern.Add(val);
                val = val - (double)4 / 2500;
            }
            rampWaveform = new NationalInstruments.AnalogWaveform<double>(1000);
            rampWaveform.Append(rampPattern.ToArray());
            rampTask.Timing.ReferenceClockRate = 10e6;
            rampTask.Timing.ConfigureSampleClock(string.Empty,1000000,SampleClockActiveEdge.Rising,SampleQuantityMode.ContinuousSamples);
            rampTask.Timing.SampleClockTimebaseSource = "/PXI1Slot5/20MHzTimebase";
            //rampTask.Stream.Buffer.OutputBufferSize = 1000;
            //rampTask.Stream.Buffer.OutputOnBoardBufferSize = 1000;
            rampTask.Control(TaskAction.Verify);
            rampTask.Control(TaskAction.Commit);
            rampWriter = new AnalogSingleChannelWriter(rampTask.Stream);
            rampWriter.BeginWriteMultiSample(true, rampPattern.ToArray(), new AsyncCallback(RampCallback), null);
            
        }

        public void RampCallback(IAsyncResult result)
        {
            Console.WriteLine("out");
            //rampWriter.BeginWriteMultiSample(true, rampWaveform, new AsyncCallback(RampCallback), null);
        }

        List<double> sElapsed = new List<double> { };
        public void UpdateLockRate(Stopwatch watch)
        {
            TimeSpan elapsed = watch.Elapsed;
            sElapsed.Add(elapsed.TotalSeconds);
            if (sElapsed.Count > 100)
                sElapsed.RemoveAt(0);
            window.SetTextField(window.LockRate, Convert.ToString(sElapsed.Count/(sElapsed.Sum())));
        }

        public Stopwatch watch = new Stopwatch();
        public void Update()
        {
            for(; ; )
            {
                watch.Start();
                ReadSamples();
                if (!fail1)
                    reference.UpdateLock();
                if (!fail2)
                    slave.UpdateLock();
                if(window.LockReference.Checked)
                    window.SetTextField(window.RefVoltageFeedback, reference.CurrentVoltage.ToString());
                if(window.LockSlave.Checked)
                    window.SetTextField(window.SlaveVoltageFeedback, slave.CurrentVoltage.ToString());
                watch.Stop();
                UpdateLockRate(watch);
                watch.Reset();
            }
        }

    }
}
