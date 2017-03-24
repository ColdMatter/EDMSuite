using MOTMaster2;
using MOTMaster2.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


namespace MOTMaster2.Testing
{
    public class Patterns : MOTMasterScript
    {
        public Patterns()
        {
            
            Parameters = new Dictionary<string, object>();
            //Digital clock frequency divided by analogue clock frequency
            Parameters["ScaleFactor"] = 20000000 / 100000;
            //This is the legnth of the digital pattern which is written to the HSDIO card, clocked at 20MHz
            Parameters["PatternLength"] = 46000000;
            //This is the length of the analogue pattern, clocked at 100 kHz
            Parameters["AnalogLength"] = 100000;
            Parameters["NumberOfFrames"] = 2;
           
            Parameters["XBias"] = 0.0;
            Parameters["YBias"] = 0.0;
            Parameters["ZBias"] = 0.0;
            //All times are in units of cycles of the 100 kHz clock. Times for digital channels are rescaled
            Parameters["2DLoadTime"] = 10000;
            Parameters["PushTime"] = 100;
            Parameters["AtomMoveTime"] = 1000;
            Parameters["3DLoadTime"] = 1000;

            Parameters["ExposureTime"] = 500;
            Parameters["BackgroundDwellTime"] = 20000;

            Parameters["MotPower"] = 2.0;
            Parameters["RepumpPower"] = 0.26;
            Parameters["2DBfield"] = 0.0;
            Parameters["3DBfield"] = 3.7;

            //Frequencies and attenuator voltages for the fibre AOMs
            Parameters["XAtten"] = 5.2;
            Parameters["YAtten"] = 5.6;
            Parameters["ZPAtten"] = 4.25;
            Parameters["ZMAtten"] = 3.78;

            Parameters["XFreq"] = 6.84;
            Parameters["YFreq"] = 6.95;
            Parameters["ZPFreq"] = 6.95;
            Parameters["ZMFreq"] = 3.78;

            Parameters["PushAtten"] = 5.0;
            Parameters["PushFreq"] = 7.42;
            Parameters["2DMotFreq"] = 7.36;
            Parameters["2DMotAtten"] = 5.7;

            Parameters["MOTdetuning"] = -13.5;
            Parameters["Molassesdetuning"] = -163.5;
               

        }

        public override HSDIOPatternBuilder GetHSDIOPattern()
            {
      
            int loadtime2D = (int)Parameters["2DLoadTime"] * (int)Parameters["ScaleFactor"];
            int pushtime =  (int)Parameters["PushTime"] * (int)Parameters["ScaleFactor"];
            int movetime = (int)Parameters["AtomMoveTime"]*(int)Parameters["ScaleFactor"];
            int loadtime3D =(int)Parameters["3DLoadTime"] * (int)Parameters["ScaleFactor"];
            int backgroundtime=(int)Parameters["BackgroundDwellTime"]*(int)Parameters["ScaleFactor"];
            int exposuretime = (int)Parameters["ExposureTime"] * (int)Parameters["ScaleFactor"];
            HSDIOPatternBuilder hs = new HSDIOPatternBuilder();
            
            hs.AddEdge("motTTL", 0,true);
            hs.AddEdge("mphiTTL", 0, true);
            //Note the aom TTLs have an opposite sense
            hs.AddEdge("xaomTTL", 0, false);
            hs.AddEdge("yaomTTL", 0, false);
            hs.AddEdge("zpaomTTL", 0, false);
            hs.AddEdge("zmaomTTL", 0, false);
            hs.AddEdge("pushaomTTL", 0, true);
            hs.AddEdge("2DaomTTL", 0, false);

            //These pulses trigger the start of the DDS
            hs.Pulse(4,0,200,"aomDDSTrig");
            hs.Pulse(4, 0, 200, "slaveDDSTrig");
            //Pulse push beam
            hs.DownPulse(loadtime2D,0,pushtime, "pushaomTTL");

            //Image the 3D mot after waiting for it to load
            hs.Pulse(loadtime3D + loadtime2D + movetime, 0, exposuretime, "cameraTTL");

            //Switch off repump before taking background
            hs.AddEdge("mphiTTL", loadtime3D+ loadtime2D + movetime+exposuretime, false);
            hs.Pulse(loadtime2D+ + movetime +loadtime3D+ exposuretime + backgroundtime, 0, exposuretime, "cameraTTL");
            return hs;
        }

        public override AnalogPatternBuilder GetAnalogPattern()
        {
            AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["AnalogLength"]);

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

            //Switch on the light and magnetic fields
            p.AddAnalogValue("motCTRL", 0, (double)Parameters["MotPower"]);
            p.AddAnalogValue("mphiCTRL", 0, (double)Parameters["RepumpPower"]);
            p.AddAnalogValue("mot3DCoil", 0, (double)Parameters["3DBfield"]);
            p.AddAnalogValue("mot2DCoil", 0, (double)Parameters["2DBfield"]);

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

            p.AddLinearRamp("motCTRL",(int)Parameters["2DLoadTime"]+(int)Parameters["3DLoadTime"]+(int)Parameters["AtomMoveTime"],1000,0);

            return p;
        }
        
        public override MuquansBuilder GetMuquansCommands()
        {
            MuquansBuilder mu = new MuquansBuilder();

            mu.SetFrequency("slave0", -13.5);
            mu.SetFrequency("mphi", 0.0);

            return mu;
            
        }
        public override PatternBuilder32 GetDigitalPattern()
        {
            throw new NotImplementedException();
        }

        public override MMAIConfiguration GetAIConfiguration()
        {
            return null;
        }
    }
}
