using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Analysis.EDM;
using Data.EDM;

namespace SirCachealot
{
    public class Controller : MarshalByRefObject
    {

        internal MainWindow mainWindow;
        private MySqlDBlockStore blockStore;
        private Cache cache;


        /* This is the interface that SirCachealot provides to the d-block store. The actual d-block
         * store class is an instance of the DBlock store interface. That object is available as a
         * private member, for internal use. External users need to get the block store through this
         * getter, which only exposes the block store's generic functions.
         */
        public DBlockStore DBlockStore
        {
            get
            {
                return blockStore;
            }
        }

        /* This is a convenient way to add a block, if you're using standard demodulation
         * configurations.
         */
        public void AddBlock(Block b, string[] demodulationConfigs)
        {
            log("Adding block " + b.Config.Settings["cluster"] + " - " + b.Config.Settings["clusterIndex"]);
            BlockDemodulator blockDemodulator = new BlockDemodulator();
            foreach (string dcName in demodulationConfigs)
            {
                DemodulationConfig dc = DemodulationConfig.GetStandardDemodulationConfig(dcName, b);
                DemodulatedBlock dBlock = blockDemodulator.DemodulateBlock(b, dc);
                blockStore.AddDBlock(dBlock);
            }
        }

        public void AddBlock(string path, string[] demodulationConfigs)
        {
            BlockSerializer bs = new BlockSerializer();
            Block b = bs.DeserializeBlockFromZippedXML(path, "block.xml");

            AddBlock(b, demodulationConfigs);
        }

        // This method is called before the main form is created.
        // Don't do any UI stuff here!
        internal void Initialise()
        {
            //set up memcached
            cache = new Cache();
//            cache.Start();

            //set up sql database
            blockStore = new MySqlDBlockStore();
            blockStore.cache = cache;
            blockStore.Start();
        }

        // This method is called by the GUI thread, once the form has
        // loaded and the UI is ready.
        internal void UIInitialise()
        {
        }

        // this method gets called by the main window menu exit item, and when
        // the form's close button is pressed.
        internal void Exit()
        {
            blockStore.Stop();
//            cache.Stop();
        }

        internal void CreateDB()
        {
            CreateDBDialog dialog = new CreateDBDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                blockStore.CreateDatabase(dialog.GetName());
                log("Created db: " + dialog.GetName());
            }

        }

        internal void SelectDB()
        {
            List<string> databases = blockStore.GetDatabaseList();
            ListSelectionDialog dialog = new ListSelectionDialog();
            dialog.Populate(databases);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string db = dialog.SelectedItem();
                if (db != "")
                {
                    SelectDB(db);
                }
            }
        }

        public void SelectDB(string db)
        {
            blockStore.Connect(db);
            log("Selected db: " + db);
        }

        internal void Test1()
        {
            BlockSerializer bs = new BlockSerializer();
            string blockFile = "c:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\June2009\\26Jun0802_0.zip";
            Block b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");

            DemodulationConfig dc = new DemodulationConfig();
            dc.AnalysisTag = "findme";
            GatedDetectorExtractSpec dg0 = GatedDetectorExtractSpec.MakeGateFWHM(b, 0, 0, 1);
            dg0.Name = "top";
            GatedDetectorExtractSpec dg1 = GatedDetectorExtractSpec.MakeGateFWHM(b, 1, 0, 1);
            dg1.Name = "norm";
            GatedDetectorExtractSpec dg2 = GatedDetectorExtractSpec.MakeWideGate(2);
            dg2.Name = "mag1";
            dg2.Integrate = false;

            dc.GatedDetectorExtractSpecs.Add(dg0.Name, dg0);
            dc.GatedDetectorExtractSpecs.Add(dg1.Name, dg1);
            dc.GatedDetectorExtractSpecs.Add(dg2.Name, dg2);
            BlockDemodulator blockDemodulator = new BlockDemodulator();
            DemodulatedBlock dBlock = blockDemodulator.DemodulateBlock(b, dc);
            DateTime start, end;
            TimeSpan ts;

            //log("Adding blocks");
            //start = DateTime.Now;
            //for (int i = 1; i < 10000; i++)
            //{
            //    blockStore.AddDBlock(dBlock);
            //}
            //end = DateTime.Now;
            //ts = end.Subtract(start);
            //log("Time to add 10000: " + ts.Minutes + "m" + ts.Seconds + "s.");

            log("Retrieving blocks, pass 1");
            start = DateTime.Now;
            for (UInt32 i = 2000; i < 3000; i++)
            {
                DemodulatedBlock dbb = blockStore.GetDBlock(i);
            }
            end = DateTime.Now;
            ts = end.Subtract(start);
            log("Time to retrieve 1000 blocks: " + ts.Minutes + "m" + ts.Seconds + "s.");

            log("Retrieving blocks, pass 2");
            start = DateTime.Now;
            for (UInt32 i = 2000; i < 3000; i++)
            {
                DemodulatedBlock dbb = blockStore.GetDBlock(i);
            }
            end = DateTime.Now;
            ts = end.Subtract(start);
            log("Time to retrieve 1000 blocks: " + ts.Minutes + "m" + ts.Seconds + "s.");

            //log("Deleting blocks");
            //start = DateTime.Now;
            //for (UInt32 i = 500; i < 1000; i++)
            //{
            //    blockStore.RemoveDBlock(i);
            //}
            //end = DateTime.Now;
            //ts = end.Subtract(start);
            //log("Time to delete 500 blocks: " + ts.Minutes + "m" + ts.Seconds + "s.");

            log("Selecting blocks by cluster");
            start = DateTime.Now;
            for (UInt32 i = 0; i < 1000; i++)
            {
                blockStore.GetUIDsByCluster("02Jun0805", new UInt32[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
            }
            end = DateTime.Now;
            ts = end.Subtract(start);
            log("Time to select blocks x 1000: " + ts.Minutes + "m" + ts.Seconds + "s.");

            log("Selecting blocks by tag");
            start = DateTime.Now;
            for (UInt32 i = 0; i < 1000; i++)
            {
                UInt32[] fm = blockStore.GetUIDsByTag("testing");
            }
            end = DateTime.Now;
            ts = end.Subtract(start);
            log("Time to select blocks x 1000: " + ts.Minutes + "m" + ts.Seconds + "s.");

            log("Selecting blocks by predicate");
            start = DateTime.Now;
            UInt32[] fmm = blockStore.GetUIDsByPredicate(
                delegate(DemodulatedBlock db)
                {
                    return (db.ChannelValues[0].GetValue(5) > db.ChannelValues[0].GetValue(7));
                },
                blockStore.GetAllUIDs()
                );
            end = DateTime.Now;
            ts = end.Subtract(start);
            log("Time to check all blocks: " + ts.Minutes + "m" + ts.Seconds + "s.");
 
        }

        private void log(string txt)
        {
            mainWindow.AppendToLog(txt);
        }

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

    }
}
