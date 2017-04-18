using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;



namespace MOTMaster2.SnippetLibrary
{
    public class Initialize : SequenceStep
    {
        public Initialize()
        {
            Console.WriteLine("No Parameter");
        }
        public Initialize(HSDIOPatternBuilder hs, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(hs, parameters);
        }

        public Initialize(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }

        public Initialize(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            AddMuquansCommands(mu, parameters);
        }
        public override void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {
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

        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
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
            p.AddAnalogValue("motCTRL", 0, (double)parameters["MotPower"]);
            p.AddAnalogValue("mphiCTRL", 0, (double)parameters["RepumpPower"]);
            p.AddAnalogValue("mot3DCoil", 0, (double)parameters["3DBfield"]);
            p.AddAnalogValue("mot2DCoil", 0, (double)parameters["2DBfield"]);
            p.AddAnalogValue("xbiasCoil2D", 0, (double)parameters["XBias2D"]);
            p.AddAnalogValue("ybiasCoil2D", 0, (double)parameters["YBias2D"]);

            p.AddAnalogValue("horizPiezo", 0, 9.0);
            //Attenuate the MOT beams to balance the powers
            p.AddAnalogValue("xaomAtten", 0, (double)parameters["XAtten"]);
            p.AddAnalogValue("yaomAtten", 0, (double)parameters["YAtten"]);
            p.AddAnalogValue("zpaomAtten", 0, (double)parameters["ZPAtten"]);
            p.AddAnalogValue("zmaomAtten", 0, (double)parameters["ZMAtten"]);
            p.AddAnalogValue("pushaomAtten", 0, (double)parameters["PushAtten"]);
            p.AddAnalogValue("2DaomAtten", 0, (double)parameters["2DMotAtten"]);

            p.AddAnalogValue("xaomFreq", 0, (double)parameters["XFreq"]);
            p.AddAnalogValue("yaomFreq", 0, (double)parameters["YFreq"]);
            p.AddAnalogValue("zpaomFreq", 0, (double)parameters["ZPFreq"]);
            p.AddAnalogValue("zmaomFreq", 0, (double)parameters["ZMFreq"]);
            p.AddAnalogValue("pushaomFreq", 0, (double)parameters["PushFreq"]);
            p.AddAnalogValue("2DaomFreq", 0, (double)parameters["2DMotFreq"]);
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            mu.SetFrequency("slave0", (double)parameters["MOTdetuning"]);
            mu.SetFrequency("mphi", 0.0);
        }
    }
}
