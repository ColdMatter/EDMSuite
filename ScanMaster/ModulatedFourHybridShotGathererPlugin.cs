using System;
using System.Threading;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    [Serializable]
    public class ModulatedFourHybridShotGathererPlugin : ShotGathererPlugin
    {
        // 
        // ANALOG (Detector 1)
        // 
        [NonSerialized] private Task analogTaskA;
        [NonSerialized] private Task analogTaskB;
        [NonSerialized] private AnalogMultiChannelReader readerA;
        [NonSerialized] private AnalogMultiChannelReader readerB;

        [NonSerialized] private double[,] analogDataA;
        [NonSerialized] private double[,] analogDataB;

        // 
        // PHOTON COUNTER (Detector 2)
        // 
        [NonSerialized] private Task countingTask;
        [NonSerialized] private Task clockTask;
        [NonSerialized] private CounterReader countReader;

        [NonSerialized] private double[] photonDataC;
        [NonSerialized] private double[] photonDataD;

        protected override void InitialiseSettings()
        {
            settings["sampleRate"] = 100000;
            settings["gateLength"] = 200;
            settings["channel"] = "pmt";
            settings["inputRangeLow"] = -1.0;
            settings["inputRangeHigh"] = 10.0;

            settings["CounterSourceChannel"] =
                (string)Environs.Hardware.Boards["daq"] + "/PFI9";
        }

        public override void AcquisitionStarting()
        {
            // 
            // ANALOG TASKS
            // 
            analogTaskA = new Task("Analog A");
            analogTaskB = new Task("Analog B");

            string channel = (string)settings["channel"];

            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel])
                .AddToTask(analogTaskA,
                    (double)settings["inputRangeLow"],
                    (double)settings["inputRangeHigh"]);

            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel])
                .AddToTask(analogTaskB,
                    (double)settings["inputRangeLow"],
                    (double)settings["inputRangeHigh"]);

            analogTaskA.Timing.ConfigureSampleClock(
                "",
                (int)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                (int)settings["gateLength"]);

            analogTaskB.Timing.ConfigureSampleClock(
                "",
                (int)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                (int)settings["gateLength"]);

            // Triggers
            analogTaskA.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("analogTrigger0"),
                DigitalEdgeStartTriggerEdge.Rising);

            analogTaskB.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("analogTrigger1"),
                DigitalEdgeStartTriggerEdge.Rising);

            analogTaskA.Control(TaskAction.Verify);
            analogTaskB.Control(TaskAction.Verify);

            readerA = new AnalogMultiChannelReader(analogTaskA.Stream);
            readerB = new AnalogMultiChannelReader(analogTaskB.Stream);

            // 
            // PHOTON COUNTER TASK
            // 
            countingTask = new Task("Counter");

            countingTask.CIChannels.CreateCountEdgesChannel(
                ((CounterChannel)Environs.Hardware.CounterChannels[channel]).PhysicalChannel,
                "",
                CICountEdgesActiveEdge.Rising,
                0,
                CICountEdgesCountDirection.Up);

            countingTask.Timing.ConfigureSampleClock(
                (string)settings["CounterSourceChannel"],
                (int)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                (int)settings["gateLength"] + 1);

            countReader = new CounterReader(countingTask.Stream);

            // Clock generator
            clockTask = new Task("Clock");

            clockTask.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels["sample clock"]).PhysicalChannel,
                "",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                (int)settings["sampleRate"],
                0.5);

            clockTask.Timing.ConfigureImplicit(
                SampleQuantityMode.ContinuousSamples,
                1000);

            clockTask.Control(TaskAction.Verify);
        }
        public override void ScanStarting()
        {
        }

        public override void ScanFinished()
        {
        }

        public override void AcquisitionFinished()
        {
            analogTaskA.Dispose();
            analogTaskB.Dispose();

            countingTask.Dispose();

            try { clockTask.Stop(); } catch { }
            clockTask.Dispose();
        }

        public override void ArmAndWait()
        {
            lock (this)
            {
                // 
                // ANALOG SHOT A
                // 
                analogTaskA.Start();
                analogDataA = readerA.ReadMultiSample((int)settings["gateLength"]);
                analogTaskA.Stop();

                // 
                // ANALOG SHOT B
                // 
                analogTaskB.Start();
                analogDataB = readerB.ReadMultiSample((int)settings["gateLength"]);
                analogTaskB.Stop();

                // 
                // PHOTON SHOT C (PFI3)
                // 
                RunPhotonShot(
                    out photonDataC,
                    (string)Environs.Hardware.Boards["analogInNew"] + "/PFI3");

                // 
                // PHOTON SHOT D (PFI4)
                // 
                RunPhotonShot(
                    out photonDataD,
                    (string)Environs.Hardware.Boards["analogInNew"] + "/PFI4");
            }
        }

        private void RunPhotonShot(out double[] result, string triggerSource)
        {
            clockTask.Control(TaskAction.Unreserve);

            clockTask.Triggers.StartTrigger.Type = StartTriggerType.DigitalEdge;
            clockTask.Triggers.StartTrigger.DigitalEdge.Edge =
                DigitalEdgeStartTriggerEdge.Rising;

            clockTask.Triggers.StartTrigger.DigitalEdge.Source = triggerSource;

            clockTask.Control(TaskAction.Commit);

            countingTask.Start();
            clockTask.Start();

            double[] temp = countReader.ReadMultiSampleDouble(-1);

            countingTask.Stop();
            clockTask.Stop();

            result = new double[temp.Length - 1];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] =
                    (temp[i + 1] - temp[i]) *
                    (int)settings["sampleRate"] / 1000.0;
            }
        }

        public override Shot Shot
        {
            get
            {
                lock (this)
                {
                    Shot s = new Shot();

                    AddAnalogTOF(s, analogDataA);
                    AddAnalogTOF(s, analogDataB);
                    AddPhotonTOF(s, photonDataC);
                    AddPhotonTOF(s, photonDataD);

                    return s;
                }
            }
        }

        private void AddAnalogTOF(Shot s, double[,] data)
        {
            TOF t = new TOF();

            double[] tmp = new double[(int)settings["gateLength"]];
            for (int i = 0; i < tmp.Length; i++)
                tmp[i] = data[0, i];

            t.Data = tmp;
            s.TOFs.Add(t);
        }

        private void AddPhotonTOF(Shot s, double[] data)
        {
            TOF t = new TOF();
            t.Data = data;
            s.TOFs.Add(t);
        }
    }
}