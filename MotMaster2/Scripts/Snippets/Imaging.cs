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
    public class Imaging : MOTMasterScriptSnippet
    {
        public Imaging()
        {
            Console.WriteLine("No parameter");
        }
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
            int clock = (int)parameters["HSClockFrequency"];
            int switchOffTime = ConvertToSampleTime((double)parameters["BfieldSwitchOffTime"],clock);
            int imagetime = ConvertToSampleTime((double)parameters["BfieldSwitchOffTime"] + (double)parameters["ImageTime"], clock);
            int backgroundtime = ConvertToSampleTime((double)parameters["BackgroundDwellTime"],clock);
            int exposuretime = ConvertToSampleTime((double)parameters["ExposureTime"],clock);
            int delaytime = ConvertToSampleTime((double)parameters["BfieldDelayTime"], clock);

            
            //Switch off light during the expansiontime
            //if ((int)parameters["ImageTime"]!=0)
            //    hs.DownPulse(switchOffTime, 0, (int)parameters["ImageTime"] * (int)parameters["ScaleFactor"]+delaytime, "motTTL");
            hs.Pulse(imagetime - 40000, delaytime, 200, "serialPreTrigger");
            hs.Pulse(imagetime - 40000, delaytime, 200, "digTest");
           
            //Trigger laser jump
            hs.Pulse(imagetime - 200, delaytime, 200, "slaveDDSTrig");
            hs.Pulse(imagetime - 200, delaytime, 200, "digTest");
           
            //Image the atoms
            hs.Pulse(imagetime, delaytime, exposuretime, "cameraTTL");

            
            //hs.AddEdge("mphiTTL", imagetime + exposuretime, false);
            hs.Pulse(imagetime + exposuretime + backgroundtime, delaytime, exposuretime, "cameraTTL");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {

           
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Shifts the light to resonance with the 2->3 transition - note the extra 1.5MHz comes from a frequency shift with the AOM
           //mu.SetFrequency("slave0",1.5);
            mu.SweepFrequency("slave0", 100, 100);
        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency);
        }
    }
}
