using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Analysis;
using Analysis.EDM;
using Data;
using Data.EDM;

using SirCachealot.Database;

namespace SirCachealot
{
    public class Controller : MarshalByRefObject
    {
        // UI
        internal MainWindow mainWindow;
        System.Threading.Timer statusMonitorTimer;
 
        // Database
        private MySqlDBlockStore blockStore;

        // Demodulation

        // Threading
        int totalAnalysed;
        int queueLength;
        int analysisThreadCount;
        int currentAnalysisTotal;
        object counterLock = new object();
        DateTime currentAnalysisStart = DateTime.Now;

        #region Setup and UI

        // This method is called before the main form is created.
        // Don't do any UI stuff here!
        internal void Initialise()
        {
            //set up sql database
            blockStore = new MySqlDBlockStore();
            blockStore.Start();
            InitialiseThreading();
        }

        // This method is called by the GUI thread once the form has
        // loaded and the UI is ready.
        internal void UIInitialise()
        {
            //start the thread pool monitor
            statusMonitorTimer = new System.Threading.Timer(new TimerCallback(UpdateStatusMonitor), null, 500, 500);
        }

        // this method gets called by the main window menu exit item, and when
        // the form's close button is pressed.
        internal void Exit()
        {
            blockStore.Stop();
            // not sure whether this is needed, or even helpful.
            statusMonitorTimer.Dispose();
        }

        internal void UpdateStatusMonitor(object unused)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(GetThreadStats());
            b.AppendLine("");
            b.AppendLine(GetDatabaseStats());
            mainWindow.SetStatsText(b.ToString());
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

        #endregion

        #region Database methods

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

        // Use this to add blocks to SirCachealot's analysis queue.
        public void AddBlockToQueue(string path, string[] demodulationConfigs)
        {
            blockAddParams bap = new blockAddParams();
            bap.path = path;
            bap.demodulationConfigs = demodulationConfigs;
            AddToQueue(AddBlockThreadWrapper, bap);
        }

        // this method and the following struct are wrappers so that we can add a block
        // with a single parameter, as required by the threadpool.
        private void AddBlockThreadWrapper(object parametersIn)
        {
            QueueItemWrapper(delegate(object parms)
            {
                blockAddParams parameters = (blockAddParams)parms;
                AddBlock(parameters.path, parameters.demodulationConfigs);
            },
            parametersIn
            );
        }
        private struct blockAddParams
        {
            public string path;
            public string[] demodulationConfigs;
            // this struct has a ToString method defined for error reporting porpoises.
            public override string ToString()
            {
                return path;
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

        private string GetDatabaseStats()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Database queries: " + blockStore.QueryCount);
            b.AppendLine("DBlocks served: " + blockStore.DBlockCount);
            return b.ToString();
        }

        #endregion

        #region Parallel analysis methods

        private void InitialiseThreading()
        {
            ThreadPool.SetMaxThreads(64, 64);
        }

        // this function adds an item to the queue, and takes care of updating the counters.
        // All functions added to the queue should be wrapped in this wrapper.
        private void QueueItemWrapper(WaitCallback workFunction, object parametersIn)
        {
            lock (counterLock)
            {
                queueLength--;
                analysisThreadCount++;
            }
            try
            {
                workFunction(parametersIn);
            }
            catch (Exception)
            {
                // if there's an exception thrown while adding a block then we're
                // pretty much stuck. The best we can do is log it and eat it to
                // stop it killing the rest of the program.
                log("Exception thrown analysing " + parametersIn.ToString());
                return;
            }
            finally
            {
                lock (counterLock) analysisThreadCount--;
            }
            lock (counterLock)
            {
                totalAnalysed++;
                currentAnalysisTotal++;
            }
        }

        private void AddToQueue(WaitCallback func, object parameters)
        {
            ThreadPool.QueueUserWorkItem(func, parameters);
            lock (counterLock) queueLength++;
        }

        // This method resets SirCachealot's analysis stats. Call it before an analysis run.
        public void ClearAnalysisRunStats()
        {
            currentAnalysisTotal = 0;
            currentAnalysisStart = DateTime.Now;
        }

        private string GetThreadStats()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Analysis threads: " + analysisThreadCount);
            b.AppendLine("Queued: " + queueLength);
            b.AppendLine("Analysed this run: " + currentAnalysisTotal);
            b.AppendLine("Run time: " + (DateTime.Now.Subtract(currentAnalysisStart)));
            b.AppendLine("Estimated time to go: " + EstimateFinishTime());
            b.AppendLine("Total analysed: " + totalAnalysed);
            return b.ToString();
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

        #endregion

        #region Testing

        public TOFChannelSetGroup Test1()
        {
            BlockSerializer bs = new BlockSerializer();
            BlockTOFDemodulator btd = new BlockTOFDemodulator();
            TOFChannelSetGroupAccumulator tcsga = new TOFChannelSetGroupAccumulator();
            Block b;
            string blockFile;
            TOFChannelSet tcs;

            blockFile = "c:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\03Oct0900_1.zip";
            b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");
            tcs = btd.TOFDemodulateBlock(b);
            tcsga.Add(tcs);
            blockFile = "c:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\03Oct0900_2.zip";
            b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");
            tcs = btd.TOFDemodulateBlock(b);
            tcsga.Add(tcs);
            blockFile = "c:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\02Oct0900_1.zip";
            b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");
            tcs = btd.TOFDemodulateBlock(b);
            tcsga.Add(tcs);
            blockFile = "c:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\02Oct0900_2.zip";
            b = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml");
            tcs = btd.TOFDemodulateBlock(b);
            tcsga.Add(tcs);

            TOFChannelSetGroup tcsg = tcsga.GetResult();

            Stream fileStream = new FileStream("c:\\Users\\jony\\Desktop\\tcsg.bin", FileMode.Create);
            (new BinaryFormatter()).Serialize(fileStream, tcsg);
            fileStream.Close();

            TOFChannelSet tcs2 = tcsg.AverageChannelSetSignedByMachineState(true, false, false);
            return tcsg;
        }

        #endregion
    }
}
