using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;
using MOTMaster2.SnippetLibrary;


namespace MOTMaster2.SnippetLibrary
{
    public class Molasses : SequenceStep
    {
        Dictionary<String, Object> parameters;

        int molassesIntensityRampStartTime;

        public Molasses(HSDIOPatternBuilder hs, Dictionary<String,Object> parameters, double startTime):base(hs,parameters,startTime){}
        public Molasses(AnalogPatternBuilder p, Dictionary<String, Object> parameters, double startTime) : base(p, parameters, startTime) { }
        public Molasses(MuquansBuilder mu, Dictionary<String, Object> parameters) : base(mu, parameters) { }

        public override void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            //Switch off the magnetic field and wait some time
            int clock = (int)parameters["HSClockFrequency"];
            int switchOffTime = ConvertToSampleTime(this.SequenceStartTime,clock);
            int serialWait = ConvertToSampleTime(2.0,clock);
          
        
            int delaytime = ConvertToSampleTime((double)parameters["BfieldDelayTime"], clock);


            //Ramp the frequency of the Light to -150 MHz
            hs.Pulse((int)switchOffTime-40000, 0, 200, "serialPreTrigger");

            hs.Pulse((int)switchOffTime , serialWait, 200, "slaveDDSTrig");
            hs.Pulse((int)switchOffTime, serialWait, 200, "aomDDSTrig");

            SetSequenceEndTime(hs.Layout.LastEventTime, clock);


           
        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            this.parameters = parameters;
            int clock = (int)parameters["AnalogClockFrequency"];
            int switchOffTime = ConvertToSampleTime(this.SequenceStartTime, clock);

            int delaytime = ConvertToSampleTime((double)parameters["BfieldDelayTime"], clock);
            int intensityRampDuration = ConvertToSampleTime((double)parameters["IntensityRampTime"], clock);
            //This time ensures that the DDS has finished the frequency ramp before ramping the intensity
            int waitTime = ConvertToSampleTime(1.5, clock);
            molassesIntensityRampStartTime = switchOffTime + delaytime+waitTime;
            //This is the time from the start of the sequence when the cloud is free to expand - i.e. after molasses

            p.AddAnalogValue("xbiasCoil2D", switchOffTime, 0.0);
            p.AddAnalogValue("ybiasCoil2D", switchOffTime, 0.0);
            p.AddAnalogValue("mot2DCoil", switchOffTime, (double)parameters["2DBfield"]);
            p.AddLinearRamp("mot3DCoil", switchOffTime, (int)(0.9*1e-3*clock),-8.0);
            p.AddAnalogValue("mot3DCoil", switchOffTime + (int)(0.9 * 1e-3 * clock), 0.0);

            p.AddAnalogValue("mphiCTRL", switchOffTime, 0.15);
            //TODO Make the time depend on a parameter
            p.AddFunction("motCTRL",molassesIntensityRampStartTime,molassesIntensityRampStartTime+intensityRampDuration, LinearMolassesRamp);
            SetSequenceEndTime(p.GetLastEventTime(), clock); 
            
        }

        public override void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Shifts the light to resonance with the 2->3 transition - note the extra 1.5MHz comes from a frequency shift with the AOM
            mu.SweepFrequency("Slave0", (double)parameters["Molassesdetuning"], (double)parameters["MolassesFreqDuration"]);
            mu.SweepFrequency("mphi", (double)parameters["Molassesdetuning"], (double)parameters["MolassesFreqDuration"]);

        }

    
        //Helper function to give a linear intensity ramp using a control voltage to an AOM. Returns control voltage as a function of time
        public double LinearMolassesRamp(int currentTime)
        {
            //TODO Check this is actually linearly ramping down the intensity
            double startTime = this.molassesIntensityRampStartTime*1e3/(int)parameters["AnalogClockFrequency"];
            double endTime = currentTime*1e3 / (int)parameters["AnalogClockFrequency"];
            double a = 0.461751;
            double b = 0.405836;
            double c = 0.346444;
            double d = 0.742407- 0.02148; //slight correction to the inital control voltage
            //double e = 4.47747;
            double e = 5.5;
            //Rescales the time so that the intensity ramps over the total ramp time.
            double time_scale = (e*(endTime-startTime) /((double)parameters["IntensityRampTime"]));
          
            return (a / Math.Tan(b * time_scale + c) + d);
        }

      
    }
}
