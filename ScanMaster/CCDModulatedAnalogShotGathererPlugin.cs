using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;
using System.Net.Sockets;

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
        private NationalInstruments.DAQmx.Task counterTask1;
        [NonSerialized]
        private AnalogMultiChannelReader reader1;
        [NonSerialized]
        private AnalogMultiChannelReader reader2;
        [NonSerialized]
        private CounterSingleChannelWriter counter1;
        //private System.Threading.Tasks.Task ccdTask;

        [NonSerialized]
        private double[,] latestData;

        private string nameCCD1;
        private string nameCCD2;
        private string computerCCD1 = "ULTRACOLDEDM";
        private string computerCCD2 = "PH-NI-LAB";

        // add ccd function
        [NonSerialized]
        private csAcq4.CCDController ccd1controller;
        [NonSerialized]
        private csAcq4.CCDController ccd2controller;


        protected override void InitialiseBaseSettings()
        {
            settings["gateStartTime"] = 600;
            settings["gateLength"] = 12000;
            settings["cameraEnabled"] = false;
            settings["ccdEnableStartTime"] = 600;
            settings["ccdEnableLength"] = 10000;
            settings["ccdTriggerMode"] = 2;
            settings["ccdNBurstFrames"] = 20;
            settings["ccdExposureTime"] = 0.04;
            settings["ccd1Gain"] = 100;
            settings["clockPeriod"] = 1;
            settings["sampleRate"] = 100000;
            settings["channel"] = "detectorA,detectorB";
            settings["cameraChannel"] = "cameraEnabler";
            settings["inputRangeLow"] = -1.0;
            settings["inputRangeHigh"] = 10.0;
        }
        protected override void InitialiseSettings()
        {
        }

        public override void AcquisitionStarting()
        {
            if ((bool)settings["cameraEnabled"])
            {
                //Set Up TCP CCD1 - ULTRAEDM
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

                //Set Up TCP CCD2 - gobelin ("PH-NI-LAB")
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

                //set the number of ccd frames 
                if (ccdTriggerMode == 2) //external edge  mode. Number of shots equal to the pmt pointsperscan * shotsperpoint * 2 (for the background shots)
                {
                    if (config.switchPlugin.State == true)
                    {
                        int CCDsnaps = 2 * (int)config.outputPlugin.Settings["pointsPerScan"] * (int)config.outputPlugin.Settings["shotsPerPoint"];
                        ccd1controller.UpdateNumSnaps(CCDsnaps);
                        ccd2controller.UpdateNumSnaps(CCDsnaps);
                    }
                    else
                    {
                        int CCDsnaps = 2 * (int)config.outputPlugin.Settings["pointsPerScan"] * (int)config.outputPlugin.Settings["shotsPerPoint"];
                        ccd1controller.UpdateNumSnaps(CCDsnaps);
                        ccd2controller.UpdateNumSnaps(CCDsnaps);
                    }
                }
                else if (ccdTriggerMode == 1) //external burst mode. number of shots equal to the pmt pointsperscan * shotsperpoint. Also update the number of frames ber burst
                {
                    int CCDsnaps = (int)config.outputPlugin.Settings["pointsPerScan"] * (int)config.outputPlugin.Settings["shotsPerPoint"];
                    ccd1controller.UpdateNumSnaps(CCDsnaps);
                    ccd2controller.UpdateNumSnaps(CCDsnaps);
                    int CCDBurstframes = (int)settings["ccdNBurstFrames"];
                    ccd1controller.UpdateFrameCount(CCDBurstframes);
                    ccd2controller.UpdateFrameCount(CCDBurstframes);
                }

                //set the CCD exposure Time
                double CCDExposureTime = (double)settings["ccdExposureTime"];
                ccd1controller.UpdateExposureTime(CCDExposureTime);
                //set the number of ccd frames 
                if (ccdTriggerMode == 2) //external edge  mode. Number of shots equal to the pmt pointsperscan * shotsperpoint * 2 (for the background shots)
                {
                    ccd2controller.UpdateExposureTime(CCDExposureTime);
                }
                else if (ccdTriggerMode == 1) //external burst mode. number of shots equal to the pmt pointsperscan * shotsperpoint. Also update the number of frames ber burst
                {
                    ccd2controller.UpdateExposureTime(CCDExposureTime * 1.1);
                }

                //set the CCD gain 
                int ccdGain = (int)settings["ccd1Gain"];
                ccd1controller.UpdateCCDGain(ccdGain);
                ccd2controller.UpdateCCDGain(ccdGain);

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
            counterTask1 = new NationalInstruments.DAQmx.Task("CCD enable Task Counter");

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

                CounterChannel pulseChannel = ((CounterChannel)Environs.Hardware.CounterChannels[camChannel]);
                // CCD enable TTL pulse
                counterTask1.COChannels.CreatePulseChannelTicks(
                    pulseChannel.PhysicalChannel,
                    pulseChannel.Name,
                    "20MHzTimebase",
                    COPulseIdleState.Low,
                    0,
                    100,
                    (20000000 / (int)settings["sampleRate"]) * (int)settings["ccdEnableLength"]
                    );

                //counterTask1.COChannels[0].PulseTerminal = "/DAQ_PXIe_6363/PFI12";



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


                counterTask1.Timing.ConfigureImplicit(SampleQuantityMode.FiniteSamples, 1);


                // trigger off PFI0 (with the standard routing, that's the same as trig1)
                inputTask1.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger0"),
                    DigitalEdgeStartTriggerEdge.Rising);
                // trigger off PFI1 (with the standard routing, that's the same as trig2)
                inputTask2.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger1"),
                    DigitalEdgeStartTriggerEdge.Rising);
                // trigger off PFI2 (with the standard routing, that's the same as trig1)
                counterTask1.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger2"),
                    DigitalEdgeStartTriggerEdge.Rising);


                inputTask1.Control(TaskAction.Verify);
                inputTask2.Control(TaskAction.Verify);
                counterTask1.Control(TaskAction.Verify);
            }


            reader1 = new AnalogMultiChannelReader(inputTask1.Stream);
            reader2 = new AnalogMultiChannelReader(inputTask2.Stream);
            counter1 = new CounterSingleChannelWriter(counterTask1.Stream);
        }


        public override void ScanStarting()
        {
        }

        public override void ScanFinished()
        {
        }

        public override void AcquisitionFinished()
        {
            // release the analog input
            inputTask1.Dispose();
            inputTask2.Dispose();
            counterTask1.Dispose();
            //config.outputPlugin.ScanParameter
            if ((bool)settings["cameraEnabled"] && ccd1controller != null && ccd2controller != null)
            {
                int CCDsnaps = 2 * (int)config.outputPlugin.Settings["pointsPerScan"] * (int)config.outputPlugin.Settings["shotsPerPoint"];
                //int ccd1Framecount = ccd1controller.GetFrameCount();
                //int ccd2Framecount = ccd2controller.GetFrameCount();
                if ((int)settings["ccdTriggerMode"] == 1)
                {
                    try
                    {
                        ccd1controller.StopBurstAcquisition();
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
                        while (ccd1controller.GetFrameCount() < CCDsnaps || ccd2controller.GetFrameCount() < CCDsnaps)
                        {
                            ccd1controller.StopAcquisition();
                            ccd2controller.StopAcquisition();
                        }
                        ccd1controller.RemoteBufRelease();
                        ccd2controller.RemoteBufRelease();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error during stopping acquisition: " + ex.ToString());
                    }

                }
                // Disconnect the remote object (if using .NET Remoting)
                // RemotingServices.Disconnect(ccd1controller);
                //catch (Exception ex)
                //{
                //    Console.WriteLine("Error during stopping acquisition: " + ex.ToString());
                //}
            }
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
                if (!Environs.Debug)
                {

                    //Console.WriteLine("Waiting for camera to start capturing...");
                    //cameraStarted.Wait();  // Wait until the camera signals frame capture start

                    if (config.switchPlugin.State == true)
                    {

                        //Thread.Sleep(50);
                        //System.Threading.Tasks.Task.Run(() =>
                        //{
                        //    try
                        //    {
                        //        ccd1controller.RemoteSnap();

                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Console.WriteLine("CCD aquisition error", ex);
                        //    }
                        //});
                        //Thread.Sleep(50);
                        //bool success = System.Threading.Tasks.Task.Run(() => ccd1controller.SnapAsync()).Result;

                        //bool success = System.Threading.Tasks.Task.Run(() => ccd1controller.SnapAsync()).Result;

                        //if (!success)
                        //{
                        //	Console.WriteLine("Failed to start Snap.");
                        //	return; // Exit if Snap did not start successfully
                        //}
                        //Console.WriteLine("Snap started successfully.");
                        counterTask1.Start(); // Start camera trigger
                        inputTask1.Start();

                        latestData = reader1.ReadMultiSample((int)settings["gateLength"]);
                        inputTask1.Stop();
                        counterTask1.Stop();
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
                    else
                    {
                        //System.Threading.Tasks.Task.Run(() =>
                        //{
                        //    try
                        //    {
                        //        ccd1controller.RemoteSnap();

                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Console.WriteLine("CCD aquisition error", ex);
                        //    }
                        //});
                        //Thread.Sleep(50);
                        counterTask1.Start(); // Start camera trigger
                        inputTask2.Start();
                        latestData = reader2.ReadMultiSample((int)settings["gateLength"]);
                        inputTask2.Stop();
                        counterTask1.Stop();
                        //Thread.Sleep(200);
                        //ccd1controller.RemoteBufRelease(); //save ccd data and rearm to external start
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
        }
    }
}
