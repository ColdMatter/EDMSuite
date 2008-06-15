using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Analysis.EDM;
using Data.EDM;

namespace SirCachealot
{
    public class Controller
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

        // This method is called before the main form is created.
        // Don't do any UI stuff here!
        internal void Initialise()
        {
            //set up memcached
            cache = new Cache();
            cache.Start();

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
            cache.Stop();
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
                    blockStore.Connect(db);
                    log("Selected db: " + db);
                }
            }
        }

        internal void Test1()
        {
            BlockSerializer bs = new BlockSerializer();
            string blockFile = "c:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2008\\June2008\\02Jun0805_8.zip";
            Block b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");

            DemodulationConfig dc = new DemodulationConfig();
            DetectorExtractSpec dg0 = DetectorExtractSpec.MakeGateFWHM(b, 0, 0, 1);
            dg0.Name = "top";
            DetectorExtractSpec dg1 = DetectorExtractSpec.MakeGateFWHM(b, 1, 0, 1);
            dg1.Name = "norm";
            DetectorExtractSpec dg2 = DetectorExtractSpec.MakeWideGate(2);
            dg2.Name = "mag1";
            dg2.Integrate = false;

            dc.DetectorExtractSpecs.Add(dg0);
            dc.DetectorExtractSpecs.Add(dg1);
            dc.DetectorExtractSpecs.Add(dg2);
            BlockDemodulator blockDemodulator = new BlockDemodulator();
            DemodulatedBlock dBlock = blockDemodulator.DemodulateBlock(b, dc);
            for (int i = 1; i < 100000; i++)
            {
                blockStore.AddDBlock(dBlock);
            }
        }

        private void log(string txt)
        {
            mainWindow.AppendToLog(txt);
        }
    }
}
