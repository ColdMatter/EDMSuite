using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using Analysis.EDM;
using Data;
using Data.EDM;

namespace SirCachealot
{
    public class Controller : MarshalByRefObject
    {
        internal MainWindow mainWindow;
        private MySqlDBlockStore blockStore;
        System.Threading.Timer threadMonitorTimer;
        int totalAnalysed;
        int queueLength;
        object queueLengthLock = new object();
        int currentAnalysisTotal;
        DateTime currentAnalysisStart = DateTime.Now;


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
         * configurations. This method is thread-safe.
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

        // This method is thread-safe.
        public void AddBlock(string path, string[] demodulationConfigs)
        {
            string[] splitPath = path.Split('\\');
            log("Loading block " + splitPath[splitPath.Length - 1]);
            BlockSerializer bs = new BlockSerializer();
            Block b = bs.DeserializeBlockFromZippedXML(path, "block.xml");
            AddBlock(b, demodulationConfigs);
        }

        // this method and the following struct are wrappers so that we can add a block
        // with a single parameter, as required by the threadpool.
        private void AddBlockThreadWrapper(object parametersIn)
        {
            lock (queueLengthLock) queueLength--;
            blockAddParams parameters = (blockAddParams)parametersIn;
            try
            {
                AddBlock(parameters.path, parameters.demodulationConfigs);
            }
            catch (Exception)
            {
                // if there's an exception thrown while adding a block then we're
                // pretty much stuck. The best we can do is log it and eat it to
                // stop it killing the rest of the program.
                log("Exception thrown analysing " + parameters.path);
                return;
            }
            lock (queueLengthLock)
            {
                totalAnalysed++;
                currentAnalysisTotal++;
            }
        }
        private struct blockAddParams
        {
            public string path;
            public string[] demodulationConfigs;
        }

        // Use this to add blocks to SirCachealot's analysis queue.
        public void AddBlockToQueue(string path, string[] demodulationConfigs)
        {
            blockAddParams bap = new blockAddParams();
            bap.path = path;
            bap.demodulationConfigs = demodulationConfigs;
            ThreadPool.QueueUserWorkItem(new WaitCallback(AddBlockThreadWrapper), bap);
            lock (queueLengthLock) queueLength++;
        }

        // This method resets SirCachealot's analysis stats. Call it before an analysis run.
        public void ClearAnalysisRunStats()
        {
            currentAnalysisTotal = 0;
            currentAnalysisStart = DateTime.Now;
        }

        // This method is called before the main form is created.
        // Don't do any UI stuff here!
        internal void Initialise()
        {
            //set up sql database
            blockStore = new MySqlDBlockStore();
            blockStore.Start();
            ThreadPool.SetMaxThreads(64,64);
        }

        // This method is called by the GUI thread once the form has
        // loaded and the UI is ready.
        internal void UIInitialise()
        {
            //start the thread pool monitor
            threadMonitorTimer = new System.Threading.Timer(new TimerCallback(UpdateThreadMonitor), null, 500, 500);
        }

        // this method gets called by the main window menu exit item, and when
        // the form's close button is pressed.
        internal void Exit()
        {
            blockStore.Stop();
            // not sure whether this is needed, or even helpful.
            threadMonitorTimer.Dispose();
        }

        internal void UpdateThreadMonitor(object unused)
        {
            StringBuilder b = new StringBuilder();
            int maxWorkers, maxIO;
            ThreadPool.GetMaxThreads(out maxWorkers, out maxIO);
            int availableWorkers, availableIO;
            ThreadPool.GetAvailableThreads(out availableWorkers, out availableIO);
            b.AppendLine("Max threads: " + maxWorkers);
            b.AppendLine("Available threads: " + availableWorkers);
            b.AppendLine("Analysis threads (estimate): " + (maxWorkers - availableWorkers - 1));
            b.AppendLine("Queued: " + queueLength);
            b.AppendLine("Analysed this run: " + currentAnalysisTotal);
            b.AppendLine("Run time: " + (DateTime.Now.Subtract(currentAnalysisStart)));
            b.AppendLine("Estimated time to go: " + EstimateFinishTime() );
            b.AppendLine("Total analysed: " + totalAnalysed);
            b.AppendLine("");
            b.AppendLine("Database queries: " + blockStore.QueryCount);
            b.AppendLine("DBlocks served: " + blockStore.DBlockCount);
            mainWindow.SetStatsText(b.ToString());
        }

        internal TimeSpan EstimateFinishTime()
        {
            if (queueLength == 0) return TimeSpan.FromSeconds(0);
            else
            {
                if (currentAnalysisTotal == 0) return TimeSpan.FromSeconds(0);
                else
                {
                    long ticksGone = DateTime.Now.Ticks - currentAnalysisStart.Ticks;
                    long ticksPerBlock = ticksGone / currentAnalysisTotal;
                    return TimeSpan.FromTicks(ticksPerBlock * queueLength);
                }
            }
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
            string blockFile = "c:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\November2009\\01Nov0900_38.zip";
            Block b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");

            BlockTOFDemodulator btd = new BlockTOFDemodulator();
            ChannelSet<TOFWithError> tcs = btd.TOFDemodulateBlock(b);
 
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
