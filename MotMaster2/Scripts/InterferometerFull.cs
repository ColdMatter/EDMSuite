using MOTMaster2;
using MOTMaster2.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

namespace MOTMaster2.Interferometer
{
    public class Patterns : MOTMasterScript
    {
        private int molassesIntensityRampStartTime;
        public Patterns()
        {
            Parameters = new Dictionary<string, object>();
            Parameters["HSClockFrequency"] = 20000000;
            Parameters["AnalogClockFrequency"] = 100000;
            //This is the legnth of the digital pattern which is written to the HSDIO card, clocked at 20MHz
            Parameters["PatternLength"] = 46000000;
            //This is the length of the analogue pattern, clocked at 100 kHz
            Parameters["AnalogLength"] = 100000;
            Parameters["NumberOfFrames"] = 2;

            Parameters["XBias"] = 0.8;
            Parameters["YBias"] = -0.3;
            Parameters["ZBias"] = -0.47;

            Parameters["XBias2D"] = 1.7;
            Parameters["YBias2D"] = -1.55;

            //All times are in milliseconds
            Parameters["2DLoadTime"] = 500.0;
            Parameters["3DLoadTime"] = 150.0;
            Parameters["BfieldSwitchOffTime"] = (double)Parameters["2DLoadTime"] + (double)Parameters["3DLoadTime"];
            Parameters["BfieldDelayTime"] = 3.0;

            //Duration of the molasses ramp in milliseconds
            Parameters["MolassesFreqDuration"] = 1.4;
            //By default the Intensity is ramped 5ms after the molasses is switched off
            Parameters["MolassesIntDuration"] = 1.6;

           
            //This is the time to image the atoms AFTER the Bfield is switched off
            Parameters["ImageStartTime"] = (double)Parameters["BfieldSwitchOffTime"] + (double)Parameters["BfieldDelayTime"] + (double)Parameters["MolassesFreqDuration"] + (double)Parameters["MolassesIntDuration"];
            Parameters["ImageTime"] = 10.0;
            Parameters["ExposureTime"] = 0.1;
            Parameters["BackgroundDwellTime"] = 500.0;

            Parameters["MotPower"] = 2.0;
            Parameters["RepumpPower"] = 0.22;
            Parameters["2DBfield"] = 9.0;
            Parameters["3DBfield"] = 2.8;

            //Frequencies and attenuator voltages for the fibre AOMs
            Parameters["XAtten"] = 2.8;
            Parameters["YAtten"] = 6.5;
            Parameters["ZPAtten"] = 3.9;
            Parameters["ZMAtten"] = 4.2;


            Parameters["XFreq"] = 6.828;
            Parameters["YFreq"] = 6.927;
            Parameters["ZPFreq"] = 6.931;
            Parameters["ZMFreq"] = 7.076;

            Parameters["PushAtten"] = 5.70;
            Parameters["PushFreq"] = 7.41;
            Parameters["2DMotFreq"] = 7.35;
            Parameters["2DMotAtten"] = 5.7;

            Parameters["MOTdetuning"] = 89.3229;
            Parameters["MPhidetuning"] = 105.6170;
            Parameters["MolassesSlavedetuning"] = 98.7500;
            Parameters["MolassesMPhidetuning"] = 70.1500;
            Parameters["StatePrepSlavedetuning"] = 105.2400;
            Parameters["StatePrepMPhidetuning"] = 99.0;
            Parameters["DetectionSlavedetuning"] = 88.6354;
            Parameters["DetectionMPhidetuning"] = 106.9920;

            Parameters["PrepRepumpDuration"] = 5.0;
            Parameters["22PumpTime"] = 2e-3;

            Parameters["BRamanFieldSwitchTime"] =2.0 ;
            Parameters["MicrowaveDuration"] = 0.15;

            Parameters["Pump|1,0>"] = false;

            Parameters["InterferometerDuration"] = 12.5;
            Parameters["DetectionScan"] = 0.0;
        }

        public override HSDIOPatternBuilder GetHSDIOPattern()
        {
            int clock = (int)Parameters["HSClockFrequency"];
            int loadtime2D = ConvertToSampleTime((double)Parameters["2DLoadTime"], clock);
            int loadtime3D = ConvertToSampleTime((double)Parameters["3DLoadTime"], clock);
            int serialWait = ConvertToSampleTime(2.0, clock);
            int molassesStartTime = loadtime2D + loadtime3D;
            int delaytime = ConvertToSampleTime((double)Parameters["BfieldDelayTime"], clock);
            //This time ensures that the DDS has finished the frequency ramp before ramping the intensity
            int intensityWaitTime = ConvertToSampleTime(3.5, clock);
            int frequencyRampDuration = ConvertToSampleTime((double)Parameters["MolassesFreqDuration"], clock);
            int molassesIntensityRampStartTime = molassesStartTime + delaytime + frequencyRampDuration+intensityWaitTime;
            int intensityRampDuration = ConvertToSampleTime((double)Parameters["MolassesIntDuration"], clock);
            int stateprepStartTime = molassesIntensityRampStartTime + intensityRampDuration;
            int repumpTime = ConvertToSampleTime((double)Parameters["PrepRepumpDuration"], clock);
            int pumptime22 = ConvertToSampleTime((double)Parameters["22PumpTime"], clock);
            int statePrepPresetTime = ConvertToSampleTime(1.0, clock);
            int fieldSwitchTime = ConvertToSampleTime((double)Parameters["BRamanFieldSwitchTime"], clock);
            int mWaveDuration = ConvertToSampleTime((double)Parameters["MicrowaveDuration"], clock);
            int mWaveStartTime = stateprepStartTime + repumpTime + serialWait + pumptime22 + statePrepPresetTime + ConvertToSampleTime(0.01, clock) + fieldSwitchTime;
            int dwellTime = ConvertToSampleTime(0.25, clock);
            int msWait = ConvertToSampleTime(1.0, clock);
            int interferometerStartTime = mWaveStartTime + mWaveDuration;
            int interferometerDuration = ConvertToSampleTime((double)Parameters["InterferometerDuration"], clock);
            int detectionStartTime = interferometerStartTime + interferometerDuration + ConvertToSampleTime((double)Parameters["DetectionScan"], clock);
            int serialPreTrigTime = 40000;
            HSDIOPatternBuilder hs = new HSDIOPatternBuilder();
            #region Initialise
            hs.AddEdge("motTTL", 0, true);
            hs.AddEdge("mphiTTL", 0, true);
            //Note the aom TTLs have an opposite sense
            hs.AddEdge("xaomTTL", 0, false);
            hs.AddEdge("yaomTTL", 0, false);
            hs.AddEdge("zpaomTTL", 0, false);
            hs.AddEdge("zmaomTTL", 0, false);
            hs.AddEdge("pushaomTTL", 0, true);
            hs.AddEdge("2DaomTTL", 0, false);

            //These pulses trigger the start of the DDS
            hs.Pulse(4, 0, 500, "aomDDSTrig");
            hs.Pulse(4, 0, 500, "slaveDDSTrig");

            #endregion

            #region Load MOT
            //Pulse push beam for the duration of the 2D mot loading time
            hs.DownPulse(4, 0, loadtime2D, "pushaomTTL");
            //Adds a very short pulse just to set the sequence end after the 3D MOT loads
            hs.Pulse(loadtime2D + loadtime3D - 4, 0, 4, "shutter");
            #endregion

            #region Molasses
            //Switch off the magnetic field and wait some time
            

            //Ramp the frequency of the Light to -150 MHz
            hs.Pulse((int)molassesStartTime - serialPreTrigTime, 0, 200, "serialPreTrigger");

            hs.Pulse(molassesStartTime, delaytime, 200, "slaveDDSTrig");
            hs.Pulse(molassesStartTime, delaytime, 200, "aomDDSTrig");

            #endregion
            
            #region State Preparation

            hs.Pulse(stateprepStartTime - serialPreTrigTime, 0, 200, "serialPreTrigger");

          
            //Triggers the muquans to set the laser frequencies for state preparation
            hs.Pulse(stateprepStartTime + repumpTime, serialWait, 200, "slaveDDSTrig");
            hs.Pulse(stateprepStartTime + repumpTime, serialWait, 200, "aomDDSTrig");

            //Switches off all the MOT beams
            hs.AddEdge("xaomTTL", stateprepStartTime + statePrepPresetTime + repumpTime, true);
            hs.AddEdge("yaomTTL", stateprepStartTime + statePrepPresetTime + repumpTime, true);
            hs.AddEdge("zpaomTTL", stateprepStartTime + statePrepPresetTime + repumpTime, true);
            hs.AddEdge("zmaomTTL", stateprepStartTime + statePrepPresetTime + repumpTime, true);
            
            int aomOnTime;
            if ((bool)Parameters["Pump|1,0>"]) aomOnTime = pumptime22 + ConvertToSampleTime((double)Parameters["10PumpTime"], clock);
            else aomOnTime = pumptime22;

            //Pulses the z mot beams to pump the atoms
            hs.DownPulse(stateprepStartTime + repumpTime + serialWait + statePrepPresetTime, 0, aomOnTime, "zpaomTTL");
            hs.DownPulse(stateprepStartTime + repumpTime + serialWait + statePrepPresetTime, 0, aomOnTime, "zmaomTTL");


            //Switches off repump after pumping all atoms to F=2
            hs.AddEdge("mphiTTL", stateprepStartTime + repumpTime + serialWait, false);

          
            //Adds a 10us pulse to drive 1->0
            hs.Pulse(stateprepStartTime + repumpTime + serialWait + pumptime22 + statePrepPresetTime, 0, ConvertToSampleTime(0.01, clock), "mphiTTL");

            //Pulses the microwave horn after waiting for a time to switch the magnetic field along the Raman axis (2ms)
            hs.Pulse(mWaveStartTime, 0, mWaveDuration, "microwaveTrigger");

            #endregion
            
            #region Interferometer

            hs.Pulse(detectionStartTime - serialPreTrigTime, 0, 200, "serialPreTrigger");

            //Triggers the muquans to set the laser frequencies for state preparation
            hs.Pulse(detectionStartTime, serialWait, 200, "slaveDDSTrig");
            hs.Pulse(detectionStartTime, serialWait, 200, "aomDDSTrig");
            #endregion

            #region Detection
            //Image atoms in N2 and Ntot
            hs.Pulse(detectionStartTime, 0, dwellTime, "acquisitionTrigger");


            hs.DownPulse(detectionStartTime + dwellTime, 0, msWait, "zpaomTTL");
            hs.DownPulse(detectionStartTime + dwellTime, 0, msWait, "zmaomTTL");
            hs.Pulse(detectionStartTime + dwellTime + msWait / 2, 0, msWait / 2, "mphiTTL");

            //Blow away for background
            hs.DownPulse(detectionStartTime + dwellTime + msWait + dwellTime, 0, 3 * msWait, "zpaomTTL");
            detectionStartTime += dwellTime + msWait + dwellTime + 3 * msWait;

            hs.DownPulse(detectionStartTime + dwellTime, 0, msWait, "zpaomTTL");
            hs.DownPulse(detectionStartTime + dwellTime, 0, msWait, "zmaomTTL");
            hs.Pulse(detectionStartTime + dwellTime + msWait / 2, 0, msWait / 2, "mphiTTL");

            detectionStartTime += dwellTime + msWait + dwellTime;
            //Take reference level
            hs.DownPulse(detectionStartTime + dwellTime, 0, msWait / 2, "zpaomTTL");
            hs.DownPulse(detectionStartTime + dwellTime, 0, msWait / 2, "zmaomTTL");

            #endregion
            return hs;
        }

        public override AnalogPatternBuilder GetAnalogPattern()
        {
            AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["AnalogLength"]);
            int clock = (int)Parameters["AnalogClockFrequency"];
            int loadtime2D = ConvertToSampleTime((double)Parameters["2DLoadTime"], clock);
            int loadtime3D = ConvertToSampleTime((double)Parameters["3DLoadTime"], clock);
            int serialWait = ConvertToSampleTime(2.0, clock);
            int molassesStartTime = loadtime2D + loadtime3D;
            int delaytime = ConvertToSampleTime((double)Parameters["BfieldDelayTime"], clock);
            //This time ensures that the DDS has finished the frequency ramp before ramping the intensity
            int intensityWaitTime = ConvertToSampleTime(3.5, clock);
            int frequencyRampDuration = ConvertToSampleTime((double)Parameters["MolassesFreqDuration"], clock);
            int molassesIntensityRampStartTime = molassesStartTime + delaytime + frequencyRampDuration + intensityWaitTime;
            int intensityRampDuration = ConvertToSampleTime((double)Parameters["MolassesIntDuration"], clock);
            this.molassesIntensityRampStartTime = molassesIntensityRampStartTime;
            int stateprepStartTime = molassesIntensityRampStartTime + intensityRampDuration;
            
            int repumpTime = ConvertToSampleTime((double)Parameters["PrepRepumpDuration"], clock);
            int pumptime22 = ConvertToSampleTime((double)Parameters["22PumpTime"], clock);
            int statePrepPresetTime = ConvertToSampleTime(1.0, clock);
            int fieldSwitchTime = ConvertToSampleTime((double)Parameters["BRamanFieldSwitchTime"], clock);
            int mWaveDuration = ConvertToSampleTime((double)Parameters["MicrowaveDuration"], clock);
            int mWaveStartTime = stateprepStartTime + repumpTime + serialWait + pumptime22 + statePrepPresetTime + ConvertToSampleTime(0.01, clock) + fieldSwitchTime;
            int bSwitchTime1 = stateprepStartTime + repumpTime + serialWait + pumptime22 + statePrepPresetTime + ConvertToSampleTime(0.01, clock);
            int bSwitchTime2 = stateprepStartTime + repumpTime + serialWait + pumptime22 + statePrepPresetTime + ConvertToSampleTime(0.01, clock) + fieldSwitchTime / 2;

            int interferometerStartTime = mWaveStartTime + mWaveDuration;
            int interferometerDuration = ConvertToSampleTime((double)Parameters["InterferometerDuration"], clock);
            int detectionStartTime = interferometerStartTime + interferometerDuration + ConvertToSampleTime((double)Parameters["DetectionScan"], clock);

            #region Initialise
            p.AddChannel("motCTRL");
            p.AddChannel("ramanCTRL");
            p.AddChannel("mphiCTRL");
            p.AddChannel("mot3DCoil");
            p.AddChannel("mot2DCoil");
            p.AddChannel("xbiasCoil");
            p.AddChannel("ybiasCoil");
            p.AddChannel("zbiasCoil");
            p.AddChannel("xbiasCoil2D");
            p.AddChannel("ybiasCoil2D");
            p.AddChannel("xaomAtten");
            p.AddChannel("yaomAtten");
            p.AddChannel("zpaomAtten");
            p.AddChannel("zmaomAtten");
            p.AddChannel("2DaomAtten");
            p.AddChannel("pushaomAtten");
            p.AddChannel("xaomFreq");
            p.AddChannel("yaomFreq");
            p.AddChannel("zpaomFreq");
            p.AddChannel("zmaomFreq");
            p.AddChannel("2DaomFreq");
            p.AddChannel("pushaomFreq");
            p.AddChannel("horizPiezo");

            //Switch on the light and magnetic fields
            p.AddAnalogValue("motCTRL", 0, (double)Parameters["MotPower"]);
            p.AddAnalogValue("mphiCTRL", 0, (double)Parameters["RepumpPower"]);
            p.AddAnalogValue("mot3DCoil", 0, (double)Parameters["3DBfield"]);
            p.AddAnalogValue("mot2DCoil", 0, (double)Parameters["2DBfield"]);
            p.AddAnalogValue("xbiasCoil2D", 0, (double)Parameters["XBias2D"]);
            p.AddAnalogValue("ybiasCoil2D", 0, (double)Parameters["YBias2D"]);

            p.AddAnalogValue("horizPiezo", 0, 9.0);
            //Attenuate the MOT beams to balance the powers
            p.AddAnalogValue("xaomAtten", 0, (double)Parameters["XAtten"]);
            p.AddAnalogValue("yaomAtten", 0, (double)Parameters["YAtten"]);
            p.AddAnalogValue("zpaomAtten", 0, (double)Parameters["ZPAtten"]);
            p.AddAnalogValue("zmaomAtten", 0, (double)Parameters["ZMAtten"]);
            p.AddAnalogValue("pushaomAtten", 0, (double)Parameters["PushAtten"]);
            p.AddAnalogValue("2DaomAtten", 0, (double)Parameters["2DMotAtten"]);

            p.AddAnalogValue("xaomFreq", 0, (double)Parameters["XFreq"]);
            p.AddAnalogValue("yaomFreq", 0, (double)Parameters["YFreq"]);
            p.AddAnalogValue("zpaomFreq", 0, (double)Parameters["ZPFreq"]);
            p.AddAnalogValue("zmaomFreq", 0, (double)Parameters["ZMFreq"]);
            p.AddAnalogValue("pushaomFreq", 0, (double)Parameters["PushFreq"]);
            p.AddAnalogValue("2DaomFreq", 0, (double)Parameters["2DMotFreq"]);
            #endregion

            #region Molasses


           
            //This is the time from the start of the sequence when the cloud is free to expand - i.e. after molasses

            p.AddAnalogValue("xbiasCoil2D", molassesStartTime, 0.0);
            p.AddAnalogValue("ybiasCoil2D", molassesStartTime, 0.0);
            p.AddAnalogValue("mot2DCoil", molassesStartTime, 0.0);
            p.AddLinearRamp("mot3DCoil", molassesStartTime, (int)(0.9 * 1e-3 * clock), -8.0);
            p.AddAnalogValue("mot3DCoil", molassesStartTime + (int)(0.9 * 1e-3 * clock), 0.0);

            p.AddLinearRamp("mphiCTRL", molassesStartTime+delaytime,frequencyRampDuration, 0.13);
            //TODO Make the time depend on a parameter
            p.AddFunction("motCTRL", molassesIntensityRampStartTime, molassesIntensityRampStartTime + intensityRampDuration, LinearMolassesRamp);

            #endregion
            
            #region State Preparation
            //Maximises the amount of power in the repump sideband to pump all atoms to F=2
            p.AddAnalogPulse("mphiCTRL", stateprepStartTime, repumpTime, 0.38, 0.32);
            p.AddAnalogValue("motCTRL", stateprepStartTime, (double)Parameters["MotPower"]);

            //Sets the z bias coil to maximum (this is for state selection)
            p.AddAnalogValue("zbiasCoil", stateprepStartTime + repumpTime, 9.0);

            //After pumping and before microwave pulse, the magnetic field is varied so that it points along the Raman axis (the yBias coils)
           
            p.AddAnalogValue("ybiasCoil", bSwitchTime1, 9.0);
            p.AddAnalogValue("zbiasCoil", bSwitchTime2, (double)Parameters["ZBias"]);
            #endregion
            
            #region Interferometer
            #endregion
          
            #region Detection
            p.AddAnalogValue("motCTRL", detectionStartTime, 0.45);
            p.AddAnalogValue("mphiCTRL", detectionStartTime, (double)Parameters["RepumpPower"]);
            #endregion

            return p;

        }

        public override SerialBuilder GetMuquansCommands()
        {
            SerialBuilder mu = new SerialBuilder();

            return mu;

        }

        public override MMAIConfiguration GetAIConfiguration()
        {
            return null;
        }
        public override PatternBuilder32 GetDigitalPattern()
        {
            throw new NotImplementedException();
        }


        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency / 1000);
        }

        //Helper function to give a linear intensity ramp using a control voltage to an AOM. Returns control voltage as a function of time
        public double LinearMolassesRamp(int currentTime)
        {
            //TODO Check this is actually linearly ramping down the intensity
            double startTime = this.molassesIntensityRampStartTime * 1e3 / (int)Parameters["AnalogClockFrequency"];
            double endTime = currentTime * 1e3 / (int)Parameters["AnalogClockFrequency"];
            double a = 0.461751;
            double b = 0.405836;
            double c = 0.346444;
            double d = 0.742407 - 0.02148; //slight correction to the inital control voltage
            //double e = 4.47747;
            double e = 5.5;
            //Rescales the time so that the intensity ramps over the total ramp time.
            double time_scale = (e * (endTime - startTime) / ((double)Parameters["MolassesIntDuration"]));

            return (a / Math.Tan(b * time_scale + c) + d);
        }
    }

}
