<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;
using MOTMaster2.SnippetLibrary;


namespace MOTMaster2.SnippetLibrary
{
    public class Molasses : MOTMasterScriptSnippet
    {
        public Molasses()
        {
            Console.WriteLine("No parameter");
        }
        public Molasses(HSDIOPatternBuilder hs, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(hs, parameters);
        }

        public Molasses(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }

        public Molasses(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            AddMuquansCommands(mu, parameters);
        }
        public void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            //Switch off the magnetic field and wait some time
            int clock = (int)parameters["HSClockFrequency"];
            int switchOffTime = ConvertToSampleTime((double)parameters["BfieldSwitchOffTime"], clock);
           
          
        
            int delaytime = ConvertToSampleTime((double)parameters["BfieldDelayTime"], clock);


            //Ramp the frequency of the Light to -150 MHz
            hs.Pulse((int)switchOffTime, 0, 200, "serialPreTrigger");

            hs.Pulse((int)switchOffTime + 10 * (int)parameters["ScaleFactor"], 0, 200, "slaveDDSTrig");
            hs.Pulse((int)switchOffTime + 10 * (int)parameters["ScaleFactor"], 0, 200, "aomDDSTrig");
          //  hs.Pulse(imagetime - 10 * (int)parameters["ScaleFactor"], 0, 200, "serialPreTrigger");


           
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["AnalogClockFrequency"];
            int switchOffTime = ConvertToSampleTime((double)parameters["BfieldSwitchOffTime"], clock);
            int delaytime = ConvertToSampleTime((double)parameters["BfieldDelayTime"], clock);

            p.AddAnalogValue("mot3DCoil", (int)parameters["BfieldSwitchOffTime"], 0.0);

            //TODO Make the time depend on a parameter
            p.AddLinearRamp("motCTRL", (int)parameters["BfieldSwitchOffTime"] + 100, 1000, 0.0);
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Shifts the light to resonance with the 2->3 transition - note the extra 1.5MHz comes from a frequency shift with the AOM
            mu.SweepFrequency("Slave0", (double)parameters["Molassesdetuning"], 5.0);
            mu.SweepFrequency("mphi", (double)parameters["Molassesdetuning"], 5.0);

        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency);
        }
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;
using MOTMaster2.SnippetLibrary;


namespace MOTMaster2.SnippetLibrary
{
    public class Molasses : MOTMasterScriptSnippet
    {
        Dictionary<String, Object> parameters;
        public Molasses()
        {
            Console.WriteLine("No parameter");
        }
        public Molasses(HSDIOPatternBuilder hs, Dictionary<String, Object> parameters)
        {
            this.parameters = parameters;
            AddDigitalSnippet(hs, parameters);
        }

        public Molasses(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            this.parameters = parameters;
            AddAnalogSnippet(p, parameters);
        }

        public Molasses(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            this.parameters = parameters;
            AddMuquansCommands(mu, parameters);
        }
        public void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            //Switch off the magnetic field and wait some time
            int switchOffTime = (int)parameters["BfieldSwitchOffTime"] * (int)parameters["ScaleFactor"];

            int delaytime = (int)parameters["BfieldDelayTime"] * (int)parameters["ScaleFactor"];

            //Ramp the frequency of the Light to -150 MHz
            hs.Pulse((int)switchOffTime, 0, 200, "serialPreTrigger");

            hs.Pulse((int)switchOffTime + 10 * (int)parameters["ScaleFactor"], 0, 200, "slaveDDSTrig");
            hs.Pulse((int)switchOffTime + 10 * (int)parameters["ScaleFactor"], 0, 200, "aomDDSTrig");
          //  hs.Pulse(imagetime - 10 * (int)parameters["ScaleFactor"], 0, 200, "serialPreTrigger");           
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {

            p.AddAnalogValue("mot3DCoil", (int)parameters["BfieldSwitchOffTime"], 0.0);

            //TODO Make the time depend on a parameter
            p.AddLinearRamp("motCTRL", (int)parameters["BfieldSwitchOffTime"] + 100, 1000, 0.0);
            p.AddFunction("motCTRL", (int)parameters["IntensityRampTime"], (int)parameters["IntensityRampTime"]+(int)((double)parameters["IntensityRampDuration"]*100000), LinearMolassesRamp);
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Shifts the light to resonance with the 2->3 transition - note the extra 1.5MHz comes from a frequency shift with the AOM
            mu.SweepFrequency("Slave0", (double)parameters["Molassesdetuning"], 5.0);
            mu.SweepFrequency("mphi", (double)parameters["Molassesdetuning"], 5.0);

        }

        //Helper function to give a linear intensity ramp using a control voltage to an AOM. Returns control voltage as a function of time
        public double LinearMolassesRamp(int currentTime)
        {
            int startTime = (int)parameters["IntensityRampStartTime"];
            double a = 0.461751;
            double b = 0.405836;
            double c = 0.346444;
            double d = 0.742407;
            double power_scale = (double)parameters["MotPower"];
            //Convert time to seconds based on 100 kHz clock
            double time_scale = (5.5/(double)parameters["IntensityRampDuration"])*((double)currentTime-(double)startTime)/100000;
            return (a/Math.Tan(b*time_scale+c)+d)*power_scale;
        }
    }
}
>>>>>>> origin/navigator_v0.x
