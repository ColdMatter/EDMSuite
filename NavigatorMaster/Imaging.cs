using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;


namespace NavigatorMaster
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
            int imagetime = (int)parameters["ImageTime"] * (int)parameters["ScaleFactor"];
            int backgroundtime = (int)parameters["BackgroundDwellTime"] * (int)parameters["ScaleFactor"];
            int exposuretime = (int)parameters["ExposureTime"] * (int)parameters["ScaleFactor"];
            //Trigger laser jump
            hs.Pulse(imagetime - 200, 0, 200, "slaveDDSTrig");
            //Image the atoms
            hs.Pulse(imagetime, 0, exposuretime, "cameraTTL");

            //Switch off repump before taking background
            //hs.AddEdge("mphiTTL", imagetime + exposuretime, false);
            hs.Pulse(imagetime + exposuretime + backgroundtime, 0, exposuretime, "cameraTTL");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
           
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Shifts the light to resonance with the 2->3 transition
            mu.SetFrequency("slave0",0.0);
        }
    }
}
