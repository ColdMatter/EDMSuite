using System;
using System.Threading;
//using System.Threading.Tasks;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;
using System.Net.Sockets;
using System.IO;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin to capture time of flight data by sampling an analog input
    /// on an E-series board. Deals with the case where shots are to be gathered
    /// synchronously with something that switches. This is done by making use
    /// of both detector trigger inputs on the board. The pattern that modulates 
    /// the switched channel, should also modulate the trigger channel so that this
    /// shot gatherer has the right behaviour.
    /// </summary>
    [Serializable]
    public class CCDModulatedAnalogShotGathererPlugin : ShotGathererPlugin
    {

        [NonSerialized]
        private NationalInstruments.DAQmx.Task inputTask1;
        [NonSerialized]
        private NationalInstruments.DAQmx.Task inputTask2;
        [NonSerialized]
        private NationalInstruments.DAQmx.Task counterTaskCCDOnShot;
        [NonSerialized]
        private NationalInstruments.DAQmx.Task counterTaskCCDOffShot;
        [NonSerialized]
        private AnalogMultiChannelReader reader1;
        [NonSerialized]
        private AnalogMultiChannelReader reader2;
        [NonSerialized]
        private CounterSingleChannelWriter counter1;
        [NonSerialized]
        private NationalInstruments.DAQmx.Task CCDReadyStatusTask; 
        [NonSerialized]
        private DigitalMultiChannelReader CCDReadyStatusReader; 
        [NonSerialized]
        private NationalInstruments.DAQmx.Task CCDAcquireStatusTaskOnShot;
        [NonSerialized]
        private NationalInstruments.DAQmx.Task CCDAcquireStatusTaskOffShot;
        [NonSerialized]
        private DigitalMultiChannelReader CCDAcquireStatusReaderOnShot;
        [NonSerialized]
        private DigitalMultiChannelReader CCDAcquireStatusReaderOffShot;
        [NonSerialized]
        private NationalInstruments.DAQmx.Task TaskCompleteTaskOnShot;
        [NonSerialized]
        private DigitalSingleChannelWriter TaskCompleteWriterOnShot;
        [NonSerialized]
        private NationalInstruments.DAQmx.Task TaskCompleteTaskOffShot;
        [NonSerialized]
        private DigitalSingleChannelWriter TaskCompleteWriterOffShot;
        [NonSerialized]
        private DigitalSingleChannelReader reader3; //rhys add 08/07
        [NonSerialized]
        private double[,] latestData;
        [NonSerialized]
        private double[,] analogData;
        [NonSerialized]
        private bool ccdAStatus;
        [NonSerialized]
        public bool shotSuccessful;
        [NonSerialized]
        //public volatile bool cancelRequested = false; //rhys add 24/11
        public volatile bool cancelRequested;

        private string nameCCD1;
        private string nameCCD2;
        //private string computerCCD1 = "ULTRACOLDEDM"; 
        private string computerCCD1 = "PH-NI-LAB"; // CCD A PC (Gobelin)
        private string computerCCD2 = "ic-czc5347lb5"; // CCD B PC
        private int CCDsnaps;

        // add ccd function
        [NonSerialized]
        private csAcq4.CCDController ccd1controller;
        [NonSerialized]
        private csAcq4.CCDController ccd2controller;


        protected override void InitialiseBaseSettings()
        {
            settings["gateStartTime"] = 60; //pmt gate start
            settings["gateLength"] = 1200; //pmt gate length
            settings["cameraEnabled"] = false; 
            settings["ccdEnableStartTime"] = 60;
            settings["ccdEnableLength"] = 100; //ccd Enable gate length. Can be changed between 100 and 1000 for burst and edge mode respectively
            settings["ccdTriggerMode"] = 1; //burst mode = 1, edge mode = 2
            settings["ccdNBurstFrames"] = 20; //number of frames per shot in burst mode
            settings["ccdAExposureTime"] = 0.00432; //burst mode exposure time
            settings["ccdBExposureTime"] = 0.004828; //burst mode exposure time
            settings["ccd1Gain"] = 600;  // default CCDA at 600
            settings["ccd2Gain"] = 1200; // default CCDB at 1200
            settings["clockPeriod"] = 1;
            settings["sampleRate"] = 10000;
            settings["channel"] = "detectorA,detectorB";
            settings["cameraChannel"] = "cameraEnabler";
            settings["inputRangeLow"] = -1.0;
            settings["inputRangeHigh"] = 10.0;
        }

        // Shirley adds on 20/01/2026 to serialise the CCD parameter and save the xml file
        [Serializable]
        public class CCDSettings
        {
            public DateTime TimeStamp = DateTime.Now;
            public int GateStartTime;
            public int GateLength;
            public int CCDEnableStartTime;
            public int CCDEnableLength;
            public int CCDTriggerMode;
            public int CCDNBurstFrames;
            public int CCDNShots;
            public int pointsPerScan;
            public int shotsPerPoint;
            public double CCDAExposureTime;
            public double CCDBExposureTime;
            public int CCDAGain;
            public int CCDBGain;
            public int SampleRate;
        }

        protected override void InitialiseSettings()
        {
        }

        public override void AcquisitionStarting()
        {
            cancelRequested = false;

            if ((bool)settings["cameraEnabled"])
            {
                //Set Up TCP CCD A - gobelin ("PH-NI-LAB")
                IPHostEntry hostInfo = Dns.GetHostEntry(computerCCD1);

                foreach (var addr in Dns.GetHostEntry(computerCCD1).AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                        nameCCD1 = addr.ToString();

                    Console.WriteLine(nameCCD1);
                }
                EnvironsHelper eHelper1 = new EnvironsHelper(computerCCD1);
                int ccd1Port = eHelper1.emccdTCPChannel;
                Console.WriteLine(ccd1Port.ToString());
                ccd1controller = (csAcq4.CCDController)(Activator.GetObject(typeof(csAcq4.CCDController), "tcp://" + nameCCD1 + ":" + ccd1Port.ToString() + "/controller.rem"));

                //Set Up TCP CCD B - "ic-czc5347lb5"
                IPHostEntry hostInfoCCD2 = Dns.GetHostEntry(computerCCD2);

                foreach (var addr in Dns.GetHostEntry(computerCCD2).AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                        nameCCD2 = addr.ToString();
                    Console.WriteLine(nameCCD2);
                }
                EnvironsHelper eHelper2 = new EnvironsHelper(computerCCD2);
                int ccd2Port = eHelper2.emccdTCPChannel;
                Console.WriteLine(ccd2Port.ToString());
                ccd2controller = (csAcq4.CCDController)(Activator.GetObject(typeof(csAcq4.CCDController), "tcp://" + nameCCD2 + ":" + ccd2Port.ToString() + "/controller.rem"));


                //set Trigger Mode 0 = internal, 1 = burst mode, 2 = external edge
                int ccdTriggerMode = (int)settings["ccdTriggerMode"];
                ccd1controller.ApplySelectedTriggerSource(ccdTriggerMode);
                ccd2controller.ApplySelectedTriggerSource(ccdTriggerMode);

                int shotsPerPoint = (int)config.outputPlugin.Settings["shotsPerPoint"];
                int pointsPerScan = (int)config.outputPlugin.Settings["pointsPerScan"];

                //RJ add 20/11 - start
                bool switchStatus = (bool)config.switchPlugin.Settings["switchActive"];

                if (switchStatus) // uncomment line 160 and replace this line with if (switchStatus) 
                {
                    CCDsnaps = 2 * pointsPerScan * shotsPerPoint; //accomodate twice the shots due to on/off shots
                    ccd1controller.UpdateNumSnaps(CCDsnaps);
                    ccd2controller.UpdateNumSnaps(CCDsnaps);
                }
                else
                {
                    CCDsnaps = pointsPerScan * shotsPerPoint;
                    ccd1controller.UpdateNumSnaps(CCDsnaps);
                    ccd2controller.UpdateNumSnaps(CCDsnaps);
                }
                int CCDBurstframes = (int)settings["ccdNBurstFrames"];
                ccd1controller.UpdateFrameCount(CCDBurstframes);
                ccd2controller.UpdateFrameCount(CCDBurstframes);
                //RJ add 21/11 - end

                ////set the number of ccd frames 
                //if (ccdTriggerMode == 2) //external edge  mode. Number of shots equal to the pmt pointsperscan * shotsperpoint * 2 (for the background shots)
                //{
                //    if (config.switchPlugin.State == true) //uncomment line 160 and replace with if switchStatus
                //    {
                //        CCDsnaps = 2 * pointsPerScan * shotsPerPoint; //Changed from 2* to 4* 05/06/2025 by Freddie to accomodate extra switch shots
                //        ccd1controller.UpdateNumSnaps(CCDsnaps);
                //        ccd2controller.UpdateNumSnaps(CCDsnaps);
                //        Console.WriteLine("Taking {0} snaps this scan", CCDsnaps.ToString());
                //    }
                //    else
                //    {
                //        CCDsnaps = 2 * pointsPerScan * shotsPerPoint;
                //        ccd1controller.UpdateNumSnaps(CCDsnaps);
                //        ccd2controller.UpdateNumSnaps(CCDsnaps);
                //    }
                //}
                //else if (ccdTriggerMode == 1) //external burst mode. number of shots equal to the pmt pointsperscan * shotsperpoint. Also update the number of frames per burst
                //{
                //    int CCDsnaps = pointsPerScan * shotsPerPoint;
                //    ccd1controller.UpdateNumSnaps(CCDsnaps);
                //    ccd2controller.UpdateNumSnaps(CCDsnaps);
                //    int CCDBurstframes = (int)settings["ccdNBurstFrames"];
                //    ccd1controller.UpdateFrameCount(CCDBurstframes);
                //    ccd2controller.UpdateFrameCount(CCDBurstframes);
                //}
                //rj 20/11
                //Add logic to incorporate if the switch. For switch true this would be int CCDsnaps=pointsPerScan * shotsPerPoint * 2. Else CCDsnaps=pointsPerScan * shotsPerPoint
                //Uncomment the lines below. Comment out lines 181-186 above

                //Note: I dont think (config.switchPlugin.State == true) below is correct. This just means On shot. We want the switchActive boolean. 
                //I think we need to import the switchActive boolean within switch of scanmaster. Uncomment line 160 above
                //replace the if (config.switchPlugin.State == true) with if (switchStatus) below

                //RJ add 20/11 - start
                //if (config.switchPlugin.State == true) // uncomment line 160 and replace this line with if (switchStatus) 
                //{
                //    CCDsnaps = 2 * pointsPerScan * shotsPerPoint; //accomodate twice the shots due to on/off shots
                //    ccd1controller.UpdateNumSnaps(CCDsnaps);
                //    ccd2controller.UpdateNumSnaps(CCDsnaps);
                //}
                //else
                //{
                //    CCDsnaps = pointsPerScan * shotsPerPoint;
                //    ccd1controller.UpdateNumSnaps(CCDsnaps);
                //    ccd2controller.UpdateNumSnaps(CCDsnaps);
                //}
                //int CCDBurstframes = (int)settings["ccdNBurstFrames"];
                //ccd1controller.UpdateFrameCount(CCDBurstframes);
                //ccd2controller.UpdateFrameCount(CCDBurstframes);
                //RJ add 21/11 - end


                //set the CCD exposure Time
                double CCDAExposureTime = (double)settings["ccdAExposureTime"];
                double CCDBExposureTime = (double)settings["ccdBExposureTime"];
                if (ccdTriggerMode == 2) //external edge  mode. Number of shots equal to the pmt pointsperscan * shotsperpoint * 2 (for the background shots)
                {
                    ccd1controller.UpdateExposureTime(CCDAExposureTime);
                    ccd2controller.UpdateExposureTime(CCDBExposureTime);
                }
                else if (ccdTriggerMode == 1) //external burst mode. number of shots equal to the pmt pointsperscan * shotsperpoint. Also update the number of frames ber burst
                {
                    ccd1controller.UpdateExposureTime(CCDAExposureTime);
                    ccd2controller.UpdateExposureTime(CCDBExposureTime); // ** shirley modifies on 08/04, based on
                    // t_ex,2 = 1.11*t_ex,1 (exposure time of ccd1) + 0.11*t_d (dead time, 0.3ms) **
                }

                //set the CCD gain 
                int ccd1Gain = (int)settings["ccd1Gain"];
                int ccd2Gain = (int)settings["ccd2Gain"]; // shirley adds on 21/11/25 to separate the gain control 
                ccd1controller.UpdateCCDGain(ccd1Gain);
                ccd2controller.UpdateCCDGain(ccd2Gain); // shirley adds on 21/11/25 to separate the gain control 


                if (ccdTriggerMode == 2)
                {

                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            ccd1controller.RemoteSnap();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("CCD acquisition error", ex);
                        }
                    });

                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            ccd2controller.RemoteSnap();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("CCD acquisition error", ex);
                        }
                    });
                }
                else if (ccdTriggerMode == 1)
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            ccd1controller.StartBurstAcquisition();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("CCD acquisition error", ex);
                        }
                    });


                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            ccd2controller.StartBurstAcquisition();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("CCD acquisition error", ex);
                        }
                    });
                }

            }



            // configure the analog input
            inputTask1 = new NationalInstruments.DAQmx.Task("analog gatherer 1 -" /*+ (string)settings["channel"]*/);
            inputTask2 = new NationalInstruments.DAQmx.Task("analog gatherer 2 -" /*+ (string)settings["channel"]*/);
            counterTaskCCDOnShot = new NationalInstruments.DAQmx.Task("ccd Enable Gate Output ON SHOT");
            counterTaskCCDOffShot = new NationalInstruments.DAQmx.Task("ccd Enable Gate Output OFF SHOT");
            CCDAcquireStatusTaskOnShot = new NationalInstruments.DAQmx.Task("ccdReadyStatusInput ON SHOT");
            CCDAcquireStatusTaskOffShot = new NationalInstruments.DAQmx.Task("ccdReadyStatusInput OFF SHOT");
            CCDReadyStatusTask = new NationalInstruments.DAQmx.Task("ccdAcquireStatusInput");
            TaskCompleteTaskOnShot = new NationalInstruments.DAQmx.Task("onShotTasksCompleteOutput");
            TaskCompleteTaskOffShot = new NationalInstruments.DAQmx.Task("offShotTasksCompleteOutput");

            // new analog channel, range -10 to 10 volts
            if (!Environs.Debug)
            {

                string channelList = (string)settings["channel"];
                string[] channels = channelList.Split(new char[] { ',' });

                string camChannel = (string)settings["cameraChannel"];

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

                // trigger off PFI0 (with the standard routing, that's the same as trig1)
                inputTask1.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger0"),
                    DigitalEdgeStartTriggerEdge.Rising);
                // trigger off PFI1 (with the standard routing, that's the same as trig2)
                inputTask2.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger1"),
                    DigitalEdgeStartTriggerEdge.Rising);

                //set up the CCD enable TTL counter channel for ON SHOT and OFF SHOT
                CounterChannel pulseChannel = ((CounterChannel)Environs.Hardware.CounterChannels[camChannel]);
                // CCD enable TTL pulse config - ON SHOT
                counterTaskCCDOnShot.COChannels.CreatePulseChannelTicks(
                    pulseChannel.PhysicalChannel,
                    pulseChannel.Name,
                    "20MHzTimebase",
                    COPulseIdleState.Low,
                    0,
                    100,
                    (20000000 / (int)settings["sampleRate"]) * (int)settings["ccdEnableLength"]
                    );

                counterTaskCCDOnShot.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, 1);

                // CCD counter task ON SHOT - trigger off PFI0 
                counterTaskCCDOnShot.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger0"),
                    DigitalEdgeStartTriggerEdge.Rising
                );

                // CCD enable TTL pulse config - OFF SHOT
                counterTaskCCDOffShot.COChannels.CreatePulseChannelTicks(
                    pulseChannel.PhysicalChannel,
                    pulseChannel.Name,
                    "20MHzTimebase",
                    COPulseIdleState.Low,
                    0,
                    100,
                    (20000000 / (int)settings["sampleRate"]) * (int)settings["ccdEnableLength"]
                    );

                counterTaskCCDOffShot.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, 1);

                //CCD counter task OFF SHOT - trigger off PFI1 
                counterTaskCCDOffShot.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger1"),
                    DigitalEdgeStartTriggerEdge.Rising
                );

                // Set up CCD Ready Status Channel (read in the TTL status from the CCDs) 
                string CCDTTLChannel = (string)Environs.Hardware.GetInfo("ccdDigitalIn");
                CCDReadyStatusTask.DIChannels.CreateChannel(
                    CCDTTLChannel,
                    "ccdStatusTTL",
                    ChannelLineGrouping.OneChannelForEachLine
                );

                // Set up the CCD Acquire Status Channel for ON SHOT 
                CCDAcquireStatusTaskOnShot.DIChannels.CreateChannel(
                   CCDTTLChannel,
                   "ccdStatusTTLOnShot",
                   ChannelLineGrouping.OneChannelForEachLine
                   );

                CCDAcquireStatusTaskOnShot.Timing.ConfigureSampleClock(
                    "",
                    (int)settings["sampleRate"],
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.FiniteSamples,
                    (int)settings["gateLength"] / 2 //shirley noted on 19/11, used to be (int)settings["gateLength"] / 2
                    );

                CCDAcquireStatusTaskOnShot.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                   (string)Environs.Hardware.GetInfo("analogTrigger0"),
                   DigitalEdgeStartTriggerEdge.Rising
                   );

                // Set up the CCD Acquire Status Channel for OFF SHOT 
                CCDAcquireStatusTaskOffShot.DIChannels.CreateChannel(
                   CCDTTLChannel,
                   "ccdStatusTTLOffShot",
                   ChannelLineGrouping.OneChannelForEachLine
                   );

                CCDAcquireStatusTaskOffShot.Timing.ConfigureSampleClock(
                    "",
                    (int)settings["sampleRate"],
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.FiniteSamples,
                    (int)settings["gateLength"] / 2 //shirley noted on 19/11, used to be (int)settings["gateLength"] / 2
                    );

                CCDAcquireStatusTaskOffShot.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                   (string)Environs.Hardware.GetInfo("analogTrigger1"),
                   DigitalEdgeStartTriggerEdge.Rising
                   );

                // Set up digital output task for the Task Complete 
                //ON SHOT
                string digitalOutputONSHOTsyncChannel = (string)Environs.Hardware.GetInfo("pfiTrigger3");
                TaskCompleteTaskOnShot.DOChannels.CreateChannel(
                    digitalOutputONSHOTsyncChannel,
                    "DigitalOutLineONSHOTsync",
                    ChannelLineGrouping.OneChannelForAllLines);

                //OFF SHOT
                string digitalOutputOFFSHOTsyncChannel = (string)Environs.Hardware.GetInfo("pfiTrigger4");
                TaskCompleteTaskOffShot.DOChannels.CreateChannel(
                    digitalOutputOFFSHOTsyncChannel,
                    "DigitalOutLineOFFSHOTsync",
                    ChannelLineGrouping.OneChannelForAllLines);


                // Verify the Tasks now before calling them in the main code. This can give more meaniful error messages
                inputTask1.Control(TaskAction.Verify);
                inputTask2.Control(TaskAction.Verify);
                counterTaskCCDOnShot.Control(TaskAction.Verify);
                counterTaskCCDOffShot.Control(TaskAction.Verify);
                CCDReadyStatusTask.Control(TaskAction.Verify);
                CCDAcquireStatusTaskOnShot.Control(TaskAction.Verify);
                CCDAcquireStatusTaskOffShot.Control(TaskAction.Verify);
                TaskCompleteTaskOnShot.Control(TaskAction.Verify);
                TaskCompleteTaskOffShot.Control(TaskAction.Verify);
            }


            // Set up readers for the relevant Tasks 
            reader1 = new AnalogMultiChannelReader(inputTask1.Stream);
            reader2 = new AnalogMultiChannelReader(inputTask2.Stream);
            CCDReadyStatusReader = new DigitalMultiChannelReader(CCDReadyStatusTask.Stream);//shirley changes from DigitalSingleChannelReader to DigitalMultiChannelReader on 11/11.
            CCDAcquireStatusReaderOnShot = new DigitalMultiChannelReader(CCDAcquireStatusTaskOnShot.Stream);
            CCDAcquireStatusReaderOffShot = new DigitalMultiChannelReader(CCDAcquireStatusTaskOffShot.Stream);
            TaskCompleteWriterOnShot = new DigitalSingleChannelWriter(TaskCompleteTaskOnShot.Stream);
            TaskCompleteWriterOffShot = new DigitalSingleChannelWriter(TaskCompleteTaskOffShot.Stream);
        }


        public override void ScanStarting()

        {
        }

        public override void ScanFinished()
        {
        }

        private void SafeStop(NationalInstruments.DAQmx.Task task) //rhys add 24/11/25
        {
            if (task == null) return;

            try
            {
                task.Stop();
            }
            catch { /* ignore DAQmx errors during forced stop */ }
        }

        // Shirley adds on 21/01/2026. This is to resolve the base path difference between the Gobelin pc and the Centaur pc
        private string RemapCCDPathtoScanMaster(string ccdFullPath)
        {
            const string dataAnchor = @"\Data\";

            int anchorIndex = ccdFullPath.IndexOf(dataAnchor, StringComparison.OrdinalIgnoreCase);
            if (anchorIndex < 0)
                throw new InvalidOperationException("CCD path does not contain \\Data\\ anchor.");

            // this preserves everything before \Data\
            string relativePath = ccdFullPath.Substring(anchorIndex + dataAnchor.Length);

            // scanmaster local data root
            string DataRoot = Environs.FileSystem.Paths["CCDxmlDataPath"].ToString();

            return Path.Combine(DataRoot, relativePath);
        }

        public override void AcquisitionFinished()
        {

            //rhys add 24/11
            // ---- Signal all ArmAndWait loops to exit ----
            cancelRequested = true;
            Console.WriteLine("Acquisition stop requested — signalling cancellation.");
            // ---- SAFELY STOP all DAQmx tasks BEFORE disposing ----
            SafeStop(inputTask1);
            SafeStop(inputTask2);
            SafeStop(counterTaskCCDOnShot);
            SafeStop(counterTaskCCDOffShot);
            SafeStop(CCDAcquireStatusTaskOnShot);
            SafeStop(CCDAcquireStatusTaskOffShot);
            SafeStop(CCDReadyStatusTask);
            SafeStop(TaskCompleteTaskOnShot);
            SafeStop(TaskCompleteTaskOffShot);

            // ---- Now dispose tasks safely ----
            try { inputTask1?.Dispose(); } catch { }
            try { inputTask2?.Dispose(); } catch { }
            try { counterTaskCCDOnShot?.Dispose(); } catch { }
            try { counterTaskCCDOffShot?.Dispose(); } catch { }
            try { CCDAcquireStatusTaskOnShot?.Dispose(); } catch { }
            try { CCDAcquireStatusTaskOffShot?.Dispose(); } catch { }
            try { CCDReadyStatusTask?.Dispose(); } catch { }
            try { TaskCompleteTaskOnShot?.Dispose(); } catch { }
            try { TaskCompleteTaskOffShot?.Dispose(); } catch { }

            Console.WriteLine("DAQ tasks safely stopped and disposed.");
            // Rhys finish adding 24/11

            // release the analog input
            //inputTask1.Dispose();
            //inputTask2.Dispose();
            //counterTaskCCDOnShot.Dispose();
            //counterTaskCCDOffShot.Dispose();
            //CCDAcquireStatusTaskOnShot.Dispose();
            //CCDAcquireStatusTaskOffShot.Dispose();
            //CCDReadyStatusTask.Dispose();
            //TaskCompleteTaskOnShot.Dispose();
            //TaskCompleteTaskOffShot.Dispose();

            int shotsPerPoint = (int)config.outputPlugin.Settings["shotsPerPoint"];
            int pointsPerScan = (int)config.outputPlugin.Settings["pointsPerScan"];

            // Shirley adds the following part for serialising the CCD parameters into xml file on 20/01/2026
            if ((bool)settings["cameraEnabled"])
            {
                try
                {
                    int shots = (int)config.outputPlugin.Settings["shotsPerPoint"]; 
                    int points = (int)config.outputPlugin.Settings["pointsPerScan"];
                    CCDSettings logData = new CCDSettings
                    {
                        TimeStamp = DateTime.Now,
                        GateStartTime = (int)settings["gateStartTime"],
                        GateLength = (int)settings["gateLength"],
                        CCDEnableStartTime = (int)settings["ccdEnableStartTime"],
                        CCDEnableLength = (int)settings["ccdEnableLength"],
                        CCDTriggerMode = (int)settings["ccdTriggerMode"],
                        CCDNBurstFrames = (int)settings["ccdNBurstFrames"],
                        shotsPerPoint = shots,
                        pointsPerScan = points,
                        CCDNShots = this.CCDsnaps,
                        CCDAExposureTime = (double)settings["ccdAExposureTime"],
                        CCDBExposureTime = (double)settings["ccdBExposureTime"],
                        CCDAGain = (int)settings["ccd1Gain"],
                        CCDBGain = (int)settings["ccd2Gain"],
                        SampleRate = (int)settings["sampleRate"]
                    };

                    // set up saving directory
                    string ccdSavePath = ccd2controller.GetSaveFullPath();
                    int ccdFileIndex = ccd2controller.GetSyncFileName();

                    if (string.IsNullOrEmpty(ccdSavePath) || ccdFileIndex < 0)
                    {
                        throw new InvalidOperationException("CCD save state is not available from CCD A (gobelin) PC.");
                    }

                    string smTiffPath = RemapCCDPathtoScanMaster(ccdSavePath);

                    string logDirectory = Path.GetDirectoryName(smTiffPath);

                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }

                    string xmlFileName = $"CCD_Config_{ccdFileIndex:D5}.xml";
                    string fullPath = Path.Combine(logDirectory, xmlFileName);


                    //// set up saving directory
                    //string basePath = Environs.FileSystem.Paths["CCDxmlDataPath"].ToString();

                    //// date-based folders
                    //DateTime now = DateTime.Now;
                    //string yearFolder = now.Year.ToString();
                    //string monthFolder = now.ToString("yyyy-MM");
                    //string dayFolder = now.ToString("yyyyMMdd");

                    //string logDirectory = Path.Combine(basePath, yearFolder, monthFolder, dayFolder);
                    //if (!Directory.Exists(logDirectory))
                    //{
                    //    Directory.CreateDirectory(logDirectory);
                    //}

                    //// define next available file index
                    //int index = 1;
                    //string fileName;
                    //string fullPath;

                    //do
                    //{
                    //    fileName = $"CCD_Config_{index:D2}.xml";
                    //    fullPath = Path.Combine(logDirectory, fileName);
                    //    index++;
                    //}
                    //while (File.Exists(fullPath));

                    // serialise the parameters
                    XmlSerializer writer = new XmlSerializer(typeof(CCDSettings));
                    using (FileStream file = File.Create(fullPath))
                    {
                        writer.Serialize(file, logData);
                    }

                    Console.WriteLine("CCD Configuration file logged to: " + fullPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not save CCD settings xml:" + ex.Message);
                }
            }


            //bool SwitchStatus = (bool)config.switchPlugin.Settings["switchActive"];


            //if (config.switchPlugin.State == true)
            //{
            //    CCDsnaps = 2 * pointsPerScan * shotsPerPoint; //Changed from 2* to 4* 05/06/2025 by Freddie to accomodate extra switch shots
            //}
            //else
            //{
            //    CCDsnaps = pointsPerScan * shotsPerPoint; // shirley removes the 2 * factor on 21/11/25
            //}

            //CCD1 disconnect
            if ((bool)settings["cameraEnabled"] && ccd1controller != null)
            {
                if ((int)settings["ccdTriggerMode"] == 1)
                {
                    try
                    {
                        Thread.Sleep(100); // Shirley adds on 05/06 to test missing frames issue
                        ccd1controller.StopBurstAcquisition();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error during stopping acquisition: " + ex.ToString());
                    }
                }
                else if ((int)settings["ccdTriggerMode"] == 2)
                {
                    try
                    {
                        if (ccd1controller.GetFrameCount() < CCDsnaps)
                        {
                            Console.WriteLine("Stopping acquisition...");
                            ccd1controller.StopAcquisition();
                        }
                        else
                        {
                            Console.WriteLine("Releasing remote buffer...");
                            ccd1controller.RemoteBufRelease();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error during stopping acquisition: " + ex.ToString());
                    }
                }
                try
                {
                    Thread.Sleep(500); // set wait time before disconnect
                    Console.WriteLine("Disconnecting CCD1...");
                    //RemotingServices.Disconnect(ccd1controller);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error disconnecting CCD1: " + ex.ToString());
                }

            }

            //CCD2 disconnect
            if ((bool)settings["cameraEnabled"] && ccd2controller != null)
            {
                //int CCDsnaps = 2 * (int)config.outputPlugin.Settings["pointsPerScan"] * (int)config.outputPlugin.Settings["shotsPerPoint"];
                if ((int)settings["ccdTriggerMode"] == 1)
                {
                    try
                    {
                        Thread.Sleep(100); // Shirley adds on 05/06 to test missing frames issue
                        ccd2controller.StopBurstAcquisition();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error during stopping acquisition: " + ex.ToString());
                    }
                }
                else if ((int)settings["ccdTriggerMode"] == 2)
                {
                    try
                    {
                        if (ccd2controller.GetFrameCount() < CCDsnaps)
                        {
                            ccd2controller.StopAcquisition();
                        }
                        else
                        {
                            ccd2controller.RemoteBufRelease();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error during stopping acquisition: " + ex.ToString());
                    }

                }
                try
                {
                    Thread.Sleep(500); // give CCD time to settle
                    //RemotingServices.Disconnect(ccd2controller);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error disconnecting CCD2: " + ex.ToString());
                }

            }

            Console.WriteLine("AcquisitionFinished() completed safely.");
        }
        //if ((bool)settings["cameraEnabled"] && ccd2controller != null)
        //{

        //    try
        //    {
        //        if ((int)settings["ccdTriggerMode"] == 1)
        //        {
        //            ccd2controller.StopBurstAcquisition();
        //        }
        //        if ((int)settings["ccdTriggerMode"] == 2)
        //        {
        //            ccd2controller.RemoteBufRelease();
        //        }

        //        // Disconnect the remote object (if using .NET Remoting)
        //        RemotingServices.Disconnect(ccd2controller);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error during disconnection: " + ex.ToString());
        //    }
        //}

        //public override void ArmAndWait()
        //{
        //	lock (this)
        //	{
        //		if (!Environs.Debug) 
        //		{
        //			if (config.switchPlugin.State == true)
        //			{
        //				counterTask1.Start();
        //				inputTask1.Start();
        //				latestData = reader1.ReadMultiSample((int)settings["gateLength"]);

        //				inputTask1.Stop();
        //				counterTask1.Stop();

        //			}
        //			else
        //			{
        //				counterTask1.Start();
        //				inputTask2.Start();
        //				latestData = reader2.ReadMultiSample((int)settings["gateLength"]);
        //				inputTask2.Stop();
        //				counterTask1.Stop();
        //			}
        //		}
        //	}
        //}

        //private ManualResetEventSlim cameraStarted = new ManualResetEventSlim(false);
        //private ManualResetEventSlim cameraFinished = new ManualResetEventSlim(false);

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (cancelRequested)
                {
                    Console.WriteLine("ArmAndWait aborted before start.");
                    return;
                }

                if (!Environs.Debug)
                {

                    //Console.WriteLine("Waiting for camera to start capturing...");
                    //cameraStarted.Wait();  // Wait until the camera signals frame capture start

                    if (config.switchPlugin.State == true) //ON SHOT
                    {

                        if ((bool)settings["cameraEnabled"])
                        {
                            bool shotSuccessful = false;

                            while (!shotSuccessful && !cancelRequested) //RJ add cancel requested 24/11 && !cancelRequested
                            {
                                //code for CCDA and CCDB
                                CCDReadyStatusTask.Start();
                                bool isCCDAready = false;
                                bool isCCDBready = false; // Shirley adds 06/11 to implement the retake logic for two CCDs
                                //while (!isCCDAready || !isCCDBready && !cancelRequested)
                                while (!isCCDAready || !isCCDBready && !cancelRequested)//while (!isCCDAready)  Shirley adds 06/11 to implement the retake logic for two CCDs, commented out for now
                                {
                                    bool[,] ttlState = CCDReadyStatusReader.ReadSingleSampleMultiLine();
                                    isCCDAready = ttlState[0, 0];
                                    isCCDBready = ttlState[1, 0]; // Shirley adds 06/11 to implement the retake logic for two CCDs

                                    //if (!isCCDAready) Thread.Sleep(1);
                                    if (!isCCDAready || !isCCDBready) Thread.Sleep(1); // Shirley adds 06/11 to implement the retake logic for two CCDs, commented out for now
                                }
                                //Console.WriteLine($"CCD Ready Status CCDA={isCCDAready}.");
                                CCDReadyStatusTask.Stop();

                                if (cancelRequested)
                                {
                                    Console.WriteLine("Cancellation detected after CCD Ready wait.");
                                    return;
                                }

                                Console.WriteLine($"CCD Ready Status CCDA={isCCDAready}, CCDB={isCCDBready}"); // Shirley adds 06/11 to implement the retake logic for two CCDs

                                //start tasks
                                inputTask1.Start();
                                counterTaskCCDOnShot.Start(); // Start camera trigger
                                CCDAcquireStatusTaskOnShot.Start(); //ccd status. Digital in
                                TaskCompleteTaskOnShot.Start(); //task sync. Digital out
                                TaskCompleteWriterOnShot.WriteSingleSampleSingleLine(false, true);


                                // Run in parallel the pmt read task and the CCD status read tasks.
                                //set the default bool status'
                                bool ccdAStatus = false;
                                bool ccdBStatus = false; // Shirley adds 06/11 to implement the retake logic for two CCDs

                                //First run the PMT task. Task.Run(()=>...) means that the task is called and the code moves on immediately.
                                //We make sure to wait for the pmt to have finised reading, after the ccd check. see below double[,] analogData = analogReadTask.Result;
                                //System.Threading.Tasks.Task<double[,]> analogReadTask = System.Threading.Tasks.Task.Run(() => reader1.ReadMultiSample((int)settings["gateLength"]));

                                //latestData = reader1.ReadMultiSample((int)settings["gateLength"]);
                                var analogReadTask = System.Threading.Tasks.Task.Run(() =>
                                {
                                    try
                                    {
                                        return reader1.ReadMultiSample((int)settings["gateLength"]);
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                });

                                System.Threading.Tasks.Task.Run(() =>
                                {
                                    if (cancelRequested) return;
                                    try
                                    {
                                        uint[,] ccdStartStatusSample = CCDAcquireStatusReaderOnShot.ReadMultiSamplePortUInt32((int)settings["gateLength"] / 2); // Sample once
                                        if (cancelRequested) return;
                                        int samplepoint = (int)settings["gateLength"] / 2;
                                        ccdAStatus = (ccdStartStatusSample[0, samplepoint - 1] & (1 << 0)) != 0; //sample P0.0 at samplepoint-1
                                        ccdBStatus = (ccdStartStatusSample[1, samplepoint - 1] & (1 << 1)) != 0; //sample P0.1 at samplepoint-1 // Shirley adds 06/11 to implement the retake logic for two CCDs
                                        //Console.WriteLine($"Status: CCDA={(ccdAStatus ? "HIGH" : "LOW")}");
                                        // Shirley adds 06/11 to implement the retake logic for two CCDs
                                        Console.WriteLine($"Status: CCDA={(ccdAStatus ? "HIGH" : "LOW")}, CCDB={(ccdBStatus ? "HIGH" : "LOW")}");

                                        if (!ccdAStatus && !ccdBStatus) //if(!ccdAStatus)
                                        {
                                            if (cancelRequested) return;
                                            var timertcp = Stopwatch.StartNew();
                                            ccd1controller.SetShotStatus(2); // shot success sent to ccd1 // Shirley adds 06/11 to implement the retake logic for two CCDs
                                            ccd2controller.SetShotStatus(2); // shot success sent to ccd2
                                            timertcp.Stop();
                                            Console.WriteLine($"TCP command took {timertcp.ElapsedMilliseconds} ms");
                                        }
                                        else
                                        {
                                            if (cancelRequested) return;
                                            ccd1controller.SetShotStatus(1); // shot not successful sent to ccd1 // Shirley adds 06/11 to implement the retake logic for two CCDs
                                            ccd2controller.SetShotStatus(1); // shot not successful sent to ccd2
                                        }
                                    }
                                    catch {/* ignore during cancellation */}

                                });


                                // Wait for PMT read to finish (this remains blocking)
                                latestData = analogReadTask.Result; // this line blocks until the pmt read task is complete
                                                                    //double[,] latestData = analogReadTask.Result; // this line blocks until the pmt read task is complete
                                //if (cancelRequested)
                                //{
                                //    Console.WriteLine("Cancellation detected during PMT read.");
                                //    return;
                                //}
                                Console.WriteLine("PMT data acquisition complete.");

                                TaskCompleteWriterOnShot.WriteSingleSampleSingleLine(false, false); //Tasks stopping. Set the digital out to a TTL Low

                                //stop all of the tasks
                                CCDAcquireStatusTaskOnShot.Stop();
                                inputTask1.Stop();
                                counterTaskCCDOnShot.Stop();
                                TaskCompleteTaskOnShot.Stop();

                                // shirley adds on 21/11/25, as we think this might solve the pg pattern hanging issue when click "stop"
                                //RemotingServices.Disconnect(ccd1controller);
                                //RemotingServices.Disconnect(ccd2controller);

                                //ccd1controller.RemoteBufRelease(); //save ccd data and rearm to external start

                                if (!ccdAStatus && !ccdBStatus) //if (!ccdAStatus)  LOW TTL means CCD captured the shot
                                {
                                    // Mark shot as successful so we can exit the while loop
                                    shotSuccessful = true;
                                    Console.WriteLine("CCD shot successful.");
                                }
                                else
                                {
                                    if (cancelRequested)
                                    {
                                        Console.WriteLine("Cancellation detected during retry.");
                                        return;
                                    }
                                    Console.WriteLine($"CCD missed the shot. Retrying the same point.");
                                    Thread.Sleep(50); // wait some short time before retry
                                }
                            }
                        }
                        else // camera Enable set to False
                        {
                            counterTaskCCDOnShot.Start(); // Start camera trigger
                            inputTask1.Start(); // Start pmt stream trigger
                            TaskCompleteTaskOnShot.Start(); //task sync. Digital out
                            TaskCompleteWriterOnShot.WriteSingleSampleSingleLine(false, true);
                            latestData = reader1.ReadMultiSample((int)settings["gateLength"]);
                            TaskCompleteWriterOnShot.WriteSingleSampleSingleLine(false, false);
                            inputTask1.Stop();
                            counterTaskCCDOnShot.Stop();
                            TaskCompleteTaskOnShot.Stop();
                        }


                        //ccd2controller.ContinuousSnapAndSave();

                        //bool result = await ccd1controller.RemoteBufReleaseAsync();
                        //if (result)
                        //{
                        //    Console.WriteLine("BufRelease completed successfully.");
                        //}
                        //else
                        //{
                        //    Console.WriteLine("BufRelease failed.");
                        //}

                        //bool result = System.Threading.Tasks.Task.Run(() => ccd1controller.RemoteBufReleaseAsync()).Result;
                        //bool result = ccd1controller.RemoteBufReleaseAsync().GetAwaiter().GetResult();
                        //if (result)
                        //{
                        //    Console.WriteLine("BufRelease completed successfully.");
                        //}
                        //else
                        //{
                        //    Console.WriteLine("BufRelease failed.");
                        //}

                        //Thread.Sleep(200);
                        //ccd1controller.RemoteBufRelease(); //save ccd data and rearm to external start

                    }
                    else //OFF SHOT
                    {
                        if ((bool)settings["cameraEnabled"])
                        {
                            bool shotSuccessful = false;

                            while (!shotSuccessful && !cancelRequested)
                            {
                                //code for CCDA and CCDB
                                CCDReadyStatusTask.Start();
                                bool isCCDAready = false;
                                bool isCCDBready = false; // Shirley adds 06/11 to implement the retake logic for two CCDs
                                //while (!isCCDAready)
                                while(!isCCDAready || !isCCDBready && !cancelRequested) //while (!isCCDAready)  // Shirley adds 06/11 to implement the retake logic for two CCDs, commented out for now
                                {
                                    bool[,] ttlState = CCDReadyStatusReader.ReadSingleSampleMultiLine();
                                    isCCDAready = ttlState[0, 0];
                                    isCCDBready = ttlState[1, 0]; // Shirley adds 06/11 to implement the retake logic for two CCDs
                                                                  //if (!isCCDAready) Thread.Sleep(1); 
                                    //if (!isCCDAready) Thread.Sleep(1);
                                    if (!isCCDAready || !isCCDBready) Thread.Sleep(1); // Shirley adds 06/11 to implement the retake logic for two CCDs, commented out for now
                                }

                                CCDReadyStatusTask.Stop();

                                if (cancelRequested)
                                {
                                    Console.WriteLine("Cancellation detected after CCD Ready wait.");
                                    return;
                                }

                                Console.WriteLine($"CCD Ready Status CCDA={isCCDAready}"); // Shirley adds 06/11 to implement the retake logic for two CCDs

                                //start tasks
                                inputTask2.Start();
                                counterTaskCCDOffShot.Start(); // Start camera trigger
                                CCDAcquireStatusTaskOffShot.Start(); //ccd status. Digital in
                                TaskCompleteTaskOffShot.Start(); //task sync. Digital out
                                TaskCompleteWriterOffShot.WriteSingleSampleSingleLine(false, true);


                                // Run in parallel the pmt read task and the CCD status read tasks.
                                //set the default bool status'
                                bool ccdAStatus = false;
                                bool ccdBStatus = false; // Shirley adds 06/11 to implement the retake logic for two CCDs

                                //First run the PMT task. Task.Run(()=>...) means that the task is called and the code moves on immediately.
                                //We make sure to wait for the pmt to have finised reading, after the ccd check. see below double[,] analogData = analogReadTask.Result;
                                System.Threading.Tasks.Task<double[,]> analogReadTask = System.Threading.Tasks.Task.Run(() => reader2.ReadMultiSample((int)settings["gateLength"]));
                                //latestData = reader2.ReadMultiSample((int)settings["gateLength"]);

                                System.Threading.Tasks.Task.Run(() =>
                                {
                                if (cancelRequested) return;
                                    try
                                    {
                                        uint[,] ccdStartStatusSample = CCDAcquireStatusReaderOffShot.ReadMultiSamplePortUInt32((int)settings["gateLength"] / 2); // Sample once
                                        int samplepoint = (int)settings["gateLength"] / 2;
                                        ccdAStatus = (ccdStartStatusSample[0, samplepoint - 1] & (1 << 0)) != 0; //sample P0.0 at samplepoint-1
                                        ccdBStatus = (ccdStartStatusSample[1, samplepoint - 1] & (1 << 1)) != 0; //sample P0.1 at samplepoint-1 // Shirley adds 06/11 to implement the retake logic for two CCDs

                                        Console.WriteLine($"Status: CCDA={(ccdAStatus ? "HIGH" : "LOW")}");

                                        // Shirley adds 06/11 to implement the retake logic for two CCDs
                                        Console.WriteLine($"Status: CCDA={(ccdAStatus ? "HIGH" : "LOW")}"); 

                                        //if (!ccdAStatus) 
                                        if (!ccdAStatus && !ccdBStatus)
                                        {
                                            if(cancelRequested) return;
                                            var timertcp = Stopwatch.StartNew();
                                            ccd1controller.SetShotStatus(2); // shot success sent to ccd1 // Shirley adds 06/11 to implement the retake logic for two CCDs
                                            ccd2controller.SetShotStatus(2); // shot success sent to ccd2
                                            timertcp.Stop();
                                            Console.WriteLine($"TCP command took {timertcp.ElapsedMilliseconds} ms");
                                        }
                                        else
                                        {
                                            if(cancelRequested) return;
                                            ccd1controller.SetShotStatus(1); // shot unsuccess sent to ccd1 // Shirley adds 06/11 to implement the retake logic for two CCDs
                                            ccd2controller.SetShotStatus(1); // shot unsuccess sent to ccd2
                                        }
                                    }
                                    catch {/* ignore during cancellation */}

                                });


                                // Wait for PMT read to finish (this remains blocking)
                                latestData = analogReadTask.Result; // this line blocks until the pmt read task is complete
                                                                    //double[,] analogData = analogReadTask.Result; // this line blocks until the pmt read task is complete

                                Console.WriteLine("PMT data acquisition complete.");

                                TaskCompleteWriterOffShot.WriteSingleSampleSingleLine(false, false); //Tasks stopping. Set the digital out to a TTL Low

                                //stop all of the tasks
                                CCDAcquireStatusTaskOffShot.Stop();
                                inputTask2.Stop();
                                counterTaskCCDOffShot.Stop();
                                TaskCompleteTaskOffShot.Stop();
                                //ccd1controller.RemoteBufRelease(); //save ccd data and rearm to external start

                                //if (!ccdAStatus) 
                                if (!ccdAStatus && !ccdBStatus) // // LOW TTL means CCD captured the shot
                                {
                                    // Mark shot as successful so we can exit the while loop
                                    shotSuccessful = true;
                                    Console.WriteLine("CCD shot successful.");
                                }
                                else
                                {
                                    if (cancelRequested)
                                    {
                                        Console.WriteLine("Cancellation detected during retry.");
                                        return;
                                    }
                                    //Console.WriteLine($"CCD point {point} missed the shot. Retrying the same point.");
                                    Thread.Sleep(50); // wait some short time before retry
                                    Console.WriteLine($"CCD shot not successful. Retake the shot");
                                }
                            }
                        }
                        else
                        {
                            counterTaskCCDOffShot.Start(); // Start camera trigger
                            inputTask2.Start();
                            TaskCompleteTaskOffShot.Start(); //task sync. Digital out
                            TaskCompleteWriterOffShot.WriteSingleSampleSingleLine(false, true);
                            latestData = reader2.ReadMultiSample((int)settings["gateLength"]);
                            TaskCompleteWriterOffShot.WriteSingleSampleSingleLine(false, false);
                            inputTask2.Stop();
                            counterTaskCCDOffShot.Stop();
                            TaskCompleteTaskOffShot.Stop();
                        }
                    }

                    //Console.WriteLine("Waiting for camera to finish capturing...");
                    //cameraFinished.Wait();  // Wait for the camera to confirm frame completion


                    //cameraStarted.Reset();
                    //cameraFinished.Reset();
                    //Console.WriteLine("Camera handshake complete, system rearmed.");
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
                    s.SetTimeStamp();
                    if (!Environs.Debug)
                    {
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

            //get
            //{
            //    lock (this)
            //    {

            //        if (!ccdAStatus) // LOW TTL means CCD captured the shot
            //        {
            //            //Console.WriteLine($"CCDA confirmed shot {point}. Proceeding.");
            //            Shot s = new Shot();
            //            s.SetTimeStamp();
            //            if (!Environs.Debug)
            //            {
            //                for (int i = 0; i < inputTask1.AIChannels.Count; i++)
            //                {
            //                    TOF t = new TOF();
            //                    t.ClockPeriod = (int)settings["clockPeriod"];
            //                    t.GateStartTime = (int)settings["gateStartTime"];
            //                    double[] tmp = new double[(int)settings["gateLength"]];
            //                    for (int j = 0; j < (int)settings["gateLength"]; j++)
            //                        tmp[j] = analogData[i, j];
            //                    t.Data = tmp;
            //                    s.TOFs.Add(t);
            //                }
            //                return s;

            //                // Mark shot as successful so we can exit the while loop
            //                shotSuccessful = true;
            //            }
            //            else
            //            {
            //                Thread.Sleep(50);
            //                return DataFaker.GetFakeShot((int)settings["gateStartTime"], (int)settings["gateLength"],
            //                    (int)settings["clockPeriod"], 1, 1);
            //            }
            //        }
            //        else
            //        {
            //            //Console.WriteLine($"CCD point {point} missed the shot. Retrying the same point.");
            //            Thread.Sleep(50); // wait some short time before retry
            //            return null; //i think this is okay to do?

            //        }
            //    }
            //}
        }
    }
}
