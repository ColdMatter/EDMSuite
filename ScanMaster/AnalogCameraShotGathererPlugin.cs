using System;
using System.Threading;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
using ScanMaster.Acquire.Plugin;
//using CameraHardwareControl;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin to capture camera pictures from a Marlin or other fish (as well as analog shots from an E series board)
    /// </summary>
    [Serializable]
    public class AnalogCameraShotGathererPlugin : ShotGathererPlugin
    {

        [NonSerialized]
        private Task inputTask1;
        [NonSerialized]
        private Task inputTask2;
        [NonSerialized]
        private AnalogMultiChannelReader reader1;
        [NonSerialized]
        private AnalogMultiChannelReader reader2;
        [NonSerialized]
        private double[,] latestData;

        [NonSerialized]
        private CameraHardwareControl.Controller cameraControl;

        protected override void InitialiseSettings()
        {
        }

        public override void AcquisitionStarting()
        {

           
            // configure the analog input
            inputTask1 = new Task("analog gatherer 1 -" /*+ (string)settings["channel"]*/);
            inputTask2 = new Task("analog gatherer 2 -" /*+ (string)settings["channel"]*/);

            // new analog channel, range -10 to 10 volts
            //			if (!Environs.Debug)
            //			{
            string channelList = (string)settings["channel"];
            string[] channels = channelList.Split(new char[] { ',' });

            foreach (string channel in channels)
            {
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                    inputTask1,
                    (double)settings["inputRangeLow"],
                    (double)settings["inputRangeHigh"]
                    );
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                    inputTask2,
                    (double)settings["inputRangeLow"],
                    (double)settings["inputRangeHigh"]
                    );
            }

            // internal clock, finite acquisition
            inputTask1.Timing.ConfigureSampleClock(
                "",
                (int)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                (int)settings["gateLength"]);
            inputTask2.Timing.ConfigureSampleClock(
                "",
                (int)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                (int)settings["gateLength"]);


            // trigger off PFI0 (with the standard routing, that's the same as trig1, pin 11)
            inputTask1.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("analogTrigger0"),
                DigitalEdgeStartTriggerEdge.Rising);
            // trigger off PFI1 (with the standard routing, that's the same as trig2, pin 10)
            inputTask2.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("analogTrigger1"),
                DigitalEdgeStartTriggerEdge.Rising);

            inputTask1.Control(TaskAction.Verify);
            inputTask2.Control(TaskAction.Verify);
            //			}
            reader1 = new AnalogMultiChannelReader(inputTask1.Stream);
            reader2 = new AnalogMultiChannelReader(inputTask2.Stream);

            cameraControl = new CameraHardwareControl.Controller();

            cameraControl.primeScan();

           
           
           
        }

        public override void ScanStarting()
        {
            if ((bool)config.switchPlugin.Settings["switchActive"])
            {
                cameraControl.primeAquisition(2 * (int)config.outputPlugin.Settings["pointsPerScan"]);
            }
            else
            {
                cameraControl.primeAquisition((int)config.outputPlugin.Settings["pointsPerScan"]);
            }
           
        }

        public override void ScanFinished()
        {

            cameraControl.scanFinished();
        }

        public override void AcquisitionFinished()
        {
            // release the analog inputs
            inputTask1.Dispose();
            inputTask2.Dispose();

            cameraControl.CloseCamera();

        }

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (!Environs.Debug)
                {
                    if (config.switchPlugin.State == true)
                    {
                        inputTask1.Start();
                        latestData = reader1.ReadMultiSample((int)settings["gateLength"]);
                        inputTask1.Stop();
                    }
                    else
                    {
                        inputTask2.Start();
                        latestData = reader2.ReadMultiSample((int)settings["gateLength"]);
                        inputTask2.Stop();
                    }
                    cameraControl.CameraArmAndWait(config.outputPlugin.ScanParameter);
                }
            }
        }

        public override Shot Shot
        {
            get
            {
                lock (this)
                {
                    if (!Environs.Debug)
                    {
                        Shot s = new Shot();
                        for (int i = 0; i < inputTask1.AIChannels.Count; i++)
                        {
                            TOF t = new TOF();
                            t.ClockPeriod = (int)settings["clockPeriod"];
                            t.GateStartTime = (int)settings["gateStartTime"];
                            double[] tmp = new double[(int)settings["gateLength"]];
                            for (int j = 0; j < (int)settings["gateLength"]; j++)
                                tmp[j] = latestData[i, j];
                            t.Data = tmp;
                            s.TOFs.Add(t);
                        }
                        return s;
                    }
                    else
                    {
                        Thread.Sleep(50);
                        return DataFaker.GetFakeShot((int)settings["gateStartTime"], (int)settings["gateLength"],
                            (int)settings["clockPeriod"], 1, 1);
                    }
                }
            }
        }

    }
}
