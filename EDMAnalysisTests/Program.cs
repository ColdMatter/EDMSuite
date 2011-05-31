using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Analysis.EDM;
using Analysis.EDM.Database;
using Data;
using Data.EDM;
//using MongoDB.Bson;
//using MongoDB.Bson.IO;
//using MongoDB.Bson.Serialization;
//using MongoDB.Driver;

using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Linq;
using EDMConfig;

namespace EDMAnalysisTests
{
    class Program
    {
        static void Main(string[] args)
        {
 //           testBSonSerializer();
            testDB4O();
        }

        private static void testDB4O()
        {
            IEmbeddedConfiguration config = Db4oEmbedded.NewConfiguration();
            config.Common.MessageLevel = 1;
            config.Common.ObjectClass(typeof(BlockConfig)).CascadeOnUpdate(true);
            config.Common.ObjectClass(typeof(BlockConfig)).CascadeOnDelete(true);
            config.Common.ObjectClass(typeof(BlockConfig)).CascadeOnActivate(true);
 //           config.Common.ObjectClass(typeof(BlockConfig)).ObjectField("Settings").Indexed(true);
            IObjectContainer db = Db4oEmbedded.OpenFile("C:\\Users\\jony\\Desktop\\test.yap");

            //BlockSerializer bs = new BlockSerializer();
            //string[] blockFiles = Directory.GetFiles(
            //    "C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2010\\March2010", "*.zip");
            //int i = 0;
            //foreach (string blockFile in blockFiles)
            //{
            //    try
            //    {
            //        Block b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");
            //        BlockDBEntry bdb = new BlockDBEntry();
            //        bdb.Config = b.Config;
            //        db.Store(bdb);
            //        Console.WriteLine(i++);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.StackTrace);
            //    }
            //}

            IEnumerable<string> names = from BlockConfig b in db
                                       where ((bool)b.Settings["eState"] == true)
                                       select (string)b.Settings["cluster"];
            string[] nameArray = names.ToArray<string>();
            foreach (string n in nameArray) Console.WriteLine(n);
            db.Close();


        }

//        private static void testBSonSerializer()
//        {
//            //TOF t = new TOF();
//            //double[] d = new double[20];
//            //for (int i = 0; i < 20; i++) d[i] = i;
//            //t.Data = d;
//            //t.GateStartTime = 100;
//            //t.ClockPeriod = 10;

//            //TOF t2 = new TOF();
//            //double[] d2 = new double[20];
//            //for (int i = 0; i < 20; i++) d2[i] = 2 * i;
//            //t2.Data = d2;
//            //t2.GateStartTime = 100;
//            //t2.ClockPeriod = 10;

//            //TOFChannel tc = new TOFChannel();
//            //tc.On = t;
//            //tc.Off = t2;
//            //tc.Difference = t - t2;

//            //TOFChannelSet tcs = new TOFChannelSet();
//            //tcs.AddChannel(new string[] { "E", "B" }, tc);

//            BlockSerializer bs = new BlockSerializer();
//            Block b = bs.DeserializeBlockFromZippedXML(
//                "C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\01Oct0900_0.zip", "block.xml");

//            BlockTOFDemodulator demod = new BlockTOFDemodulator();
//            TOFChannelSet tcs = demod.TOFDemodulateBlock(b, 0, true);

////            string tcsString = tcs.ToJson<TOFChannelSet>();

//            byte[] tcsBSON = tcs.ToBson<TOFChannelSet>();

//            TOFChannelSet tcs2 = BsonSerializer.Deserialize<TOFChannelSet>(tcsBSON);
//            Console.WriteLine(tcs2.Channels);

//            String sss = tcs2.ToJson<TOFChannelSet>();
//            sss.StartsWith("{");

//            MongoServer server = MongoServer.Create("mongodb://localhost");
//            MongoDatabase db = server.GetDatabase("test");
//            MongoCollection<TOFChannelSet> coll = db.GetCollection<TOFChannelSet>("foo");

//            tcs.Cluster = "01Jan0002";
//            for (int i = 0; i < 1000; i++)
//            {
//                tcs.ClusterIndex = i;
//                coll.Insert<TOFChannelSet>(tcs);
//            }
//            tcs.Cluster = "01Jan0003";
//            for (int i = 0; i < 2000; i++)
//            {
//                tcs.ClusterIndex = i;
//                coll.Insert<TOFChannelSet>(tcs);
//            }
//        }

    }
}
