using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Data.EDM;

using EDMConfig;
using Analysis.EDM;

namespace EDMAnalysisTests
{
    class Program
    {
        static void Main(string[] args)
        {
 //           testBSonSerializer();
 //           testDB4O();
            testSQLServer();
        }

        private static void testSQLServer()
        {
            BlockSerializer bls = new BlockSerializer();
            Block b = bls.DeserializeBlockFromZippedXML(
                "C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\01Oct0900_0.zip", "block.xml");
            byte[] bts = serializeAsByteArray(b.Config);

            BlockTOFDemodulator demod = new BlockTOFDemodulator();
            TOFChannelSet tcsD = demod.TOFDemodulateBlock(b, 0, true);
            byte[] tcsBytes = serializeAsByteArray(tcsD);

            for (int i = 0; i < 10; i++)
            {
                using (EDMDatabaseDataContext dc2 = new EDMDatabaseDataContext())
                {
                    DBBlock dbb = new DBBlock();
                    dbb.cluster = "test3";
                    dbb.clusterIndex = i;
                    dbb.configBytes = bts;
                    DBTOFChannelSet t = new DBTOFChannelSet();
                    t.tcsData = tcsBytes;
                    t.detector = "test";
                    t.FileID = Guid.NewGuid();
                    dbb.DBTOFChannelSets.Add(t);
                    DBTOFChannelSet t2 = new DBTOFChannelSet();
                    t2.tcsData = tcsBytes;
                    t2.detector = "test2";
                    t2.FileID = Guid.NewGuid();
                    dbb.DBTOFChannelSets.Add(t2);
                    dc2.DBBlocks.InsertOnSubmit(dbb);
                    dc2.SubmitChanges();
                }
            }

            EDMDatabaseDataContext dc = new EDMDatabaseDataContext();

            IEnumerable<string> blockNames = from DBTOFChannelSet tcs in dc.DBTOFChannelSets
                                             where tcs.DBBlock.cluster == "test3"
                                             where tcs.detector == "test"
                                             select tcs.DBBlock.cluster;

            string[] bn = blockNames.ToArray<string>();
            Console.WriteLine(bn.Count());

        }

        //private static void testDB4O()
        //{

        //    IEmbeddedConfiguration config2 = Db4oEmbedded.NewConfiguration();
        //    config2.Common.MessageLevel = 1;
        //    config2.Common.ObjectClass(typeof(BlockDBEntry)).CascadeOnUpdate(true);
        //    config2.Common.ObjectClass(typeof(BlockDBEntry)).CascadeOnDelete(true);
        //    config2.Common.ObjectClass(typeof(BlockDBEntry)).CascadeOnActivate(true);
        //    config2.Common.ObjectClass(typeof(BlockDBEntry)).ObjectField("EState").Indexed(true);

        //    IObjectContainer db2 = Db4oEmbedded.OpenFile("C:\\Users\\jony\\Desktop\\test2.yap");
        //    //IObjectServer server = Db4oClientServer.OpenServer("C:\\Users\\jony\\Desktop\\test2.yap", 1234565);

        //    //BlockSerializer bs = new BlockSerializer();
        //    //string[] blockFiles = Directory.GetFiles(
        //    //    "C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2010\\", "*.zip", SearchOption.AllDirectories);
        //    //int i = 0;
        //    //foreach (string blockFile in blockFiles)
        //    //{
        //    //    try
        //    //    {
        //    //        Block b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");
        //    //        BlockDBEntry bdb = new BlockDBEntry(b.Config);
        //    //        db2.Store(bdb);
        //    //        db2.Commit();
        //    //        Console.WriteLine(i++);
        //    //    }
        //    //    catch (Exception e)
        //    //    {
        //    //        Console.WriteLine(e.StackTrace);
        //    //    }
        //    //}

        //    //IEnumerable<string> names = from BlockConfig b in db
        //    //                            where ((bool)b.Settings["eState"] == true)
        //    //                            select (string)b.Settings["cluster"];
        //    //string[] nameArray = names.ToArray<string>();
        //    //foreach (string n in nameArray) Console.WriteLine(n);

        //    //IEnumerable<BlockDBEntry> bcs = from BlockConfig b in db
        //    //                                select new BlockDBEntry(b);



        //    //foreach (BlockDBEntry b in bcs) db2.Store(b);

        //    IEnumerable<string> bcs = from BlockDBEntry b in db2
        //                              where b.EState == true
        //                              where b.Tags.Contains("include")
        //                              select b.Cluster;

        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    List<string> names = new List<string>();
        //    foreach (string s in bcs) names.Add(s);
        //    sw.Stop();
        //    Console.WriteLine("time: " + sw.Elapsed);
        //    Console.WriteLine(names.Count);

        //    //db.Close();
        //    db2.Close();
        //}

        private static byte[] serializeAsByteArray(object bc)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, bc);
            byte[] buffer = new Byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(buffer, 0, (int)ms.Length);
            ms.Close();
            return buffer;
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

//            //DBTOFChannelSet tcs = new DBTOFChannelSet();
//            //tcs.AddChannel(new string[] { "E", "B" }, tc);

//            BlockSerializer bs = new BlockSerializer();
//            Block b = bs.DeserializeBlockFromZippedXML(
//                "C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\01Oct0900_0.zip", "block.xml");

//            BlockTOFDemodulator demod = new BlockTOFDemodulator();
//            DBTOFChannelSet tcs = demod.TOFDemodulateBlock(b, 0, true);

////            string tcsString = tcs.ToJson<DBTOFChannelSet>();

//            byte[] tcsBSON = tcs.ToBson<DBTOFChannelSet>();

//            DBTOFChannelSet tcs2 = BsonSerializer.Deserialize<DBTOFChannelSet>(tcsBSON);
//            Console.WriteLine(tcs2.Channels);

//            String sss = tcs2.ToJson<DBTOFChannelSet>();
//            sss.StartsWith("{");

//            MongoServer server = MongoServer.Create("mongodb://localhost");
//            MongoDatabase db = server.GetDatabase("test");
//            MongoCollection<DBTOFChannelSet> coll = db.GetCollection<DBTOFChannelSet>("foo");

//            tcs.Cluster = "01Jan0002";
//            for (int i = 0; i < 1000; i++)
//            {
//                tcs.ClusterIndex = i;
//                coll.Insert<DBTOFChannelSet>(tcs);
//            }
//            tcs.Cluster = "01Jan0003";
//            for (int i = 0; i < 2000; i++)
//            {
//                tcs.ClusterIndex = i;
//                coll.Insert<DBTOFChannelSet>(tcs);
//            }
//        }

    }
}
