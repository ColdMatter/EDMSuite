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
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Shifts the light to resonance with the 2->3 transition - note the extra 1.5MHz comes from a frequency shift with the AOM
            mu.SweepFrequency("Slave0", (double)parameters["Molassesdetuning"], 5.0);
            mu.SweepFrequency("mphi", (double)parameters["Molassesdetuning"], 5.0);

        }
    }
}
