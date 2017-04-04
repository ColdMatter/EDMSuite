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
            int imagetime = 0;
            int switchOffTime = 0;
            if (parameters.ContainsKey("ImageStartTime"))
            {
                imagetime = ConvertToSampleTime((double)parameters["ImageStartTime"] + (double)parameters["ImageTime"], clock);
                switchOffTime = ConvertToSampleTime((double)parameters["ImageStartTime"], clock);
            }
            else
            {
                switchOffTime = ConvertToSampleTime((double)parameters["BfieldSwitchOffTime"], clock);
                imagetime = ConvertToSampleTime((double)parameters["BfieldSwitchOffTime"] + (double)parameters["BfieldDelayTime"] + (double)parameters["ImageTime"], clock);
            }

            int backgroundtime = ConvertToSampleTime((double)parameters["BackgroundDwellTime"],clock);
            int exposuretime = ConvertToSampleTime((double)parameters["ExposureTime"],clock);
            int delaytime = ConvertToSampleTime((double)parameters["BfieldDelayTime"], clock);

            
            //Switch off light during the expansiontime
            if ((double)parameters["ImageTime"] != 0.0)
                hs.DownPulse(switchOffTime, 0, imagetime - switchOffTime , "motTTL");
            hs.Pulse(imagetime - 100000, 0, 200, "serialPreTrigger");
           
           
            //Trigger laser jump
            hs.Pulse(imagetime - 200, 0, 200, "slaveDDSTrig");
        
           
            //Image the atoms
            hs.Pulse(imagetime, 0, exposuretime, "cameraTTL");

            
            //hs.AddEdge("mphiTTL", imagetime + exposuretime, false);
            //Image background
            hs.Pulse(imagetime + exposuretime + backgroundtime, 0, exposuretime, "cameraTTL");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int clock = (int)parameters["AnalogClockFrequency"];
            int imagetime = 0;
            int switchOffTime = 0;
            int backgroundtime = ConvertToSampleTime((double)parameters["BackgroundDwellTime"], clock);
            int exposuretime = ConvertToSampleTime((double)parameters["ExposureTime"], clock);
            if (parameters.ContainsKey("ImageStartTime"))
            {
                imagetime = ConvertToSampleTime((double)parameters["ImageStartTime"] + (double)parameters["ImageTime"], clock);
                switchOffTime = ConvertToSampleTime((double)parameters["ImageStartTime"], clock);
            }
            else
            {
                switchOffTime = ConvertToSampleTime((double)parameters["BfieldSwitchOffTime"], clock);
                imagetime = ConvertToSampleTime((double)parameters["BfieldSwitchOffTime"] + (double)parameters["BfieldDelayTime"] + (double)parameters["ImageTime"], clock);
            }
            p.AddAnalogValue("motCTRL", imagetime, 2.0);
            p.AddAnalogValue("motCTRL", imagetime + exposuretime + backgroundtime + exposuretime, 0.0);
           
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            //Shifts the light to resonance with the 2->3 transition - note the extra 1.5MHz comes from a frequency shift with the AOM
            mu.SetFrequency("slave0",1.5);
            mu.SetFrequency("mphi", 0.0);
         
        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency/1000);
        }
    }
}
