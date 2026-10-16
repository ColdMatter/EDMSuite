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
    /// <summary>
    /// A plugin to capture time-resolved data by buffered event counting.
    ///  - Each element of the buffer will contain the number of edges present at the source pin of the counter
    /// in the time interval between two successive edges at the gate pin of the same counter. The counts are 
    /// gated into the buffer by a sample clock. The number of samples collected is set by the gateLength setting.
    ///  - The sample clock is also set up in this class. The sample clock will appear on the out pin of a
    ///  second counter. This clock signal should be routed to the gate pin of the first counter. The frequency
    ///  of this sample clock is determined by the sampleRate setting.
    ///  - This plugin can be used either in triggered or un-triggered mode. In untriggered mode, the 
    ///  data collection is not synchronized to anything. In triggered mode, data collection is synchronized
    ///  to a trigger which should appear on the gate pin of the sample clock.
    ///  - The count data is converted to a frequency (kHz).
    ///  
    ///  Signal Connections
    ///  -----------------
    ///  Route the signal to be counted to the source pin of a counter (I'll call it counter A)
    ///  Route the output of the second counter (counter B, the sample clock) to the gate pin of counter A
    ///  To trigger the data acquisition, route a trigger signal to the analog input standard trigger (PFI0 - for compatibility with analog shot gathering)
    ///  To take modulated (on/off) data, you should have two separate triggers from the PG - one for the off shots and the other for the
    ///  on shots. Route the second trigger to PFI1.
    ///  Beware of cross-talk between the source and gate pins - this will upset everything!
    /// </summary>
    [Serializable]
    public class BufferedEventCountingTwoShotGathererPlugin : ShotGathererPlugin
    {
        [NonSerialized]
        private Task countingTask;

        [NonSerialized]
        private Task freqOutTask;

        [NonSerialized]
        private CounterReader countReader;

        [NonSerialized]
        private double[] latestData;

        protected override void InitialiseSettings()
        {
            settings["triggerActive"] = true;
            settings["CounterSourceChannel"] =
                (string)Environs.Hardware.Boards["daq"] + "/PFI9";
        }

        public override void AcquisitionStarting()
        {
            // 
            // COUNTING TASK
            // 
            countingTask = new Task("buffered edge counter gatherer " +
                (string)settings["channel"]);

            countingTask.CIChannels.CreateCountEdgesChannel(
                ((CounterChannel)Environs.Hardware.CounterChannels[
                    (string)settings["channel"]]).PhysicalChannel,
                "edge counter",
                CICountEdgesActiveEdge.Rising,
                0,
                CICountEdgesCountDirection.Up);

            countingTask.Timing.ConfigureSampleClock(
                (string)settings["CounterSourceChannel"],
                (int)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                (int)settings["gateLength"] + 1);

            countingTask.Control(TaskAction.Verify);

            // 
            // CLOCK GENERATION TASK
            // 
            freqOutTask = new Task("buffered event counter clock generation");

            freqOutTask.COChannels.CreatePulseChannelFrequency(
                ((CounterChannel)Environs.Hardware.CounterChannels[
                    "sample clock"]).PhysicalChannel,
                "photon counter clocking signal",
                COPulseFrequencyUnits.Hertz,
                COPulseIdleState.Low,
                0,
                (int)settings["sampleRate"],
                0.5);

            freqOutTask.Timing.ConfigureImplicit(
                SampleQuantityMode.ContinuousSamples,
                1000);

            countReader = new CounterReader(countingTask.Stream);

            // Verify once (do not commit yet, we will reconfigure per shot)
            freqOutTask.Control(TaskAction.Verify);
        }

        public override void ScanStarting() { }

        public override void ScanFinished() { }

        public override void AcquisitionFinished()
        {
            countingTask.Dispose();

            try
            {
                freqOutTask.Stop();
            }
            catch { }

            freqOutTask.Dispose();
        }

        public override void ArmAndWait()
        {
            lock (this)
            {
                // 
                // ALLOW RECONFIGURATION
                // 
                freqOutTask.Control(TaskAction.Unreserve);

                // 
                // SET TRIGGER PER SHOT
                // 
                if ((bool)settings["triggerActive"])
                {
                    freqOutTask.Triggers.StartTrigger.Type =
                        StartTriggerType.DigitalEdge;

                    freqOutTask.Triggers.StartTrigger.DigitalEdge.Edge =
                        DigitalEdgeStartTriggerEdge.Rising;

                    string triggerSource;

                    if (config.switchPlugin.State)
                    {
                        triggerSource =
                            (string)Environs.Hardware.Boards["analogInNew"] + "/PFI4";
                    }
                    else
                    {
                        triggerSource =
                            (string)Environs.Hardware.Boards["analogInNew"] + "/PFI0";
                    }

                    freqOutTask.Triggers.StartTrigger.DigitalEdge.Source =
                        triggerSource;
                }

                // 
                // RE-COMMIT TASK
                // 
                freqOutTask.Control(TaskAction.Commit);

                // 
                // START TASKS
                // 
                countingTask.Start();
                freqOutTask.Start();

                // 
                // READ DATA
                // 
                double[] tempdata =
                    countReader.ReadMultiSampleDouble(-1);

                // 
                // STOP TASKS
                // 
                countingTask.Stop();
                freqOutTask.Stop();

                // 
                // PROCESS DATA
                // 
                latestData = new double[tempdata.Length - 1];

                for (int k = 0; k < latestData.Length; k++)
                {
                    latestData[k] =
                        (tempdata[k + 1] - tempdata[k]) *
                        (int)settings["sampleRate"] / 1000.0;
                }
            }
        }

        public override Shot Shot
        {
            get
            {
                lock (this)
                {
                    Shot s = new Shot();
                    TOF t = new TOF();

                    t.ClockPeriod = (int)settings["clockPeriod"];
                    t.GateStartTime = (int)settings["gateStartTime"];

                    if (!Environs.Debug)
                    {
                        t.Data = latestData;
                        s.TOFs.Add(t);
                        return s;
                    }
                    else
                    {
                        Thread.Sleep(50);

                        return DataFaker.GetFakeShot(
                            (int)settings["gateStartTime"],
                            (int)settings["gateLength"],
                            (int)settings["clockPeriod"],
                            1,
                            1);
                    }
                }
            }
        }
    }
}