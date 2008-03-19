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
    /// A plugin to capture time of flight data by sampling an analog input
    /// on an E-series board.
    /// </summary>
    [Serializable]
    public class AnalogCameraShotGathererPlugin : ShotGathererPlugin
    {

        [NonSerialized]
        private Task inputTask;
        [NonSerialized]
        private AnalogMultiChannelReader reader;
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
            inputTask = new Task("analog gatherer " /*+ (string)settings["channel"]*/);

            // new analog channel, range -10 to 10 volts
            //			if (!Environs.Debug)
            //			{
            string channelList = (string)settings["channel"];
            string[] channels = channelList.Split(new char[] { ',' });

            foreach (string channel in channels)
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                    inputTask,
                    (double)settings["inputRangeLow"],
                    (double)settings["inputRangeHigh"]
                    );

            // internal clock, finite acquisition
            inputTask.Timing.ConfigureSampleClock(
                "",
                (int)settings["sampleRate"],
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                (int)settings["gateLength"]);

            // trigger off PFI0 (with the standard routing, that's the same as trig1)
            inputTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("analogTrigger0"),
                DigitalEdgeStartTriggerEdge.Rising);

            inputTask.Control(TaskAction.Verify);
            //			}
            reader = new AnalogMultiChannelReader(inputTask.Stream);

            cameraControl = new CameraHardwareControl.Controller();

            cameraControl.primeScan();

           
           
           
        }

        public override void ScanStarting()
        {

         cameraControl.primeAquisition((int)config.outputPlugin.Settings["pointsPerScan"]);
           
        }

        public override void ScanFinished()
        {

            cameraControl.scanFinished();
        }

        public override void AcquisitionFinished()
        {
            // release the analog input
            inputTask.Dispose();

            cameraControl.CloseCamera();

        }

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (!Environs.Debug)
                {

                   // cameraControl.RecordScanParameter(ToString[config.outputPlugin.ScanParameter));

                    cameraControl.CameraArmAndWait(config.outputPlugin.ScanParameter);

                    inputTask.Start();
                    latestData = reader.ReadMultiSample((int)settings["gateLength"]);
                    inputTask.Stop();
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
                        for (int i = 0; i < inputTask.AIChannels.Count; i++)
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
