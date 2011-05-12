using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Analysis.EDM;
using Data;
using Data.EDM;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;


namespace EDMAnalysisTests
{
    class Program
    {
        static void Main(string[] args)
        {
            testBSonSerializer();
        }

        private static void testBSonSerializer()
        {
            //TOF t = new TOF();
            //double[] d = new double[20];
            //for (int i = 0; i < 20; i++) d[i] = i;
            //t.Data = d;
            //t.GateStartTime = 100;
            //t.ClockPeriod = 10;

            //TOF t2 = new TOF();
            //double[] d2 = new double[20];
            //for (int i = 0; i < 20; i++) d2[i] = 2 * i;
            //t2.Data = d2;
            //t2.GateStartTime = 100;
            //t2.ClockPeriod = 10;

            //TOFChannel tc = new TOFChannel();
            //tc.On = t;
            //tc.Off = t2;
            //tc.Difference = t - t2;

            //TOFChannelSet tcs = new TOFChannelSet();
            //tcs.AddChannel(new string[] { "E", "B" }, tc);

            BlockSerializer bs = new BlockSerializer();
            Block b = bs.DeserializeBlockFromZippedXML(
                "C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\01Oct0900_0.zip", "block.xml");

            BlockTOFDemodulator demod = new BlockTOFDemodulator();
            TOFChannelSet tcs = demod.TOFDemodulateBlock(b, 0, true);

//            string tcsString = tcs.ToJson<TOFChannelSet>();

            byte[] tcsBSON = tcs.ToBson<TOFChannelSet>();

            TOFChannelSet tcs2 = BsonSerializer.Deserialize<TOFChannelSet>(tcsBSON);
            Console.WriteLine(tcs2.Channels);

        }

    }
}
