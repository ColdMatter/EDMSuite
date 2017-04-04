using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;


namespace MOTMaster.SnippetLibrary
{
    public class Imaging : MOTMasterScriptSnippet
    {

         public Imaging(HSDIOPatternBuilder hs, Dictionary<String,Object> parameters)
         {
             AddDigitalSnippet(hs,parameters);
         }

         public Imaging(AnalogPatternBuilder p, Dictionary<String,Object> parameters)
         {
             AddAnalogSnippet(p,parameters);
         }

         public Imaging(MuquansBuilder mu, Dictionary<String, Object> parameters)
         {
             AddMuquansCommands(mu,parameters);
         }
        public void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
            //The Image time is defined as the length of time we wait after switching off the MOT B field
            int switchOffTime = (int)parameters["BfieldSwitchOffTime"] * (int)parameters["ScaleFactor"];
            int imagetime = ((int)parameters["BfieldSwitchOffTime"] + (int)parameters["ImageTime"]) * (int)parameters["ScaleFactor"];
            int backgroundtime = (int)parameters["BackgroundDwellTime"] * (int)parameters["ScaleFactor"];
            int exposuretime = (int)parameters["ExposureTime"] * (int)parameters["ScaleFactor"];
            int delaytime = (int)parameters["BfieldDelayTime"] * (int)parameters["ScaleFactor"];

            
            //Switch off light during the expansiontime
            if ((int)parameters["ImageTime"]!=0)
                hs.DownPulse(switchOffTime, 0, (int)parameters["ImageTime"] * (int)parameters["ScaleFactor"]+delaytime, "motTTL");

            //Trigger laser jump
            hs.Pulse(imagetime - 200, delaytime, 200, "slaveDDSTrig");
           
            //Image the atoms
            hs.Pulse(imagetime, delaytime, exposuretime, "cameraTTL");

            
            //hs.AddEdge("mphiTTL", imagetime + exposuretime, false);
            hs.Pulse(imagetime + exposuretime + backgroundtime, delaytime, exposuretime, "cameraTTL");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {

            p.AddAnalogValue("mot3DCoil", (int)parameters["BfieldSwitchOffTime"], 0.0);
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Shifts the light to resonance with the 2->3 transition - note the extra 1.5MHz comes from a frequency shift with the AOM
           // mu.SetFrequency("slave0",1.5);
        }
    }
}
