using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Reflection;
using System.Threading;

using Analysis;
using Analysis.EDM;
using Data;
using Data.EDM;

using SirCachealot.Database;
using SirCachealot.Parallel;

namespace SirCachealot
{
    public class Controller : MarshalByRefObject
    {
        // UI
        internal MainWindow mainWindow;
        System.Threading.Timer statusMonitorTimer;

        // Database
        private MySqlDBlockStore blockStore;
        //private MySqlTOFDBlockStore blockStore;

        // Gate set used for demodulation
        private GatedDemodulationConfigSet gateSet;

        // TOF Demodulation
        //private Dictionary<string, TOFChannelSetAccumulator> tcsaDictionary;
        //private object accumulatorLock = new object();

        // Threading
        private ThreadManager threadManager = new ThreadManager();

        #region Setup and UI

        // This method is called before the main form is created.
        // Don't do any UI stuff here!
        internal void Initialise()
        {
            //set up sql database
            blockStore = new MySqlDBlockStore();
            //blockStore = new MySqlTOFDBlockStore();
            gateSet = new GatedDemodulationConfigSet();
            blockStore.Start();
            threadManager.InitialiseThreading(this);
        }

        // This method is called by the GUI thread once the form has
        // loaded and the UI is ready.
        internal void UIInitialise()
        {
            // put the version number in the title bar to avoid confusion!
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            mainWindow.Text += "  " + version.ToString();
            // This will load the shared code assembly so that we can get its
            // version number and display that as well.
            TOF t = new TOF();
            Version sharedCodeVersion = Assembly.GetAssembly(t.GetType()).GetName().Version;
            mainWindow.Text += " (" + sharedCodeVersion.ToString() + ")";
            // start the status monitor
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
            b.AppendLine(threadManager.GetThreadStats());
            b.AppendLine("");
            b.AppendLine(GetDatabaseStats());
            mainWindow.SetStatsText(b.ToString());
        }

        internal void log(string txt)
        {
            mainWindow.AppendToLog(txt);
        }

        internal void errorLog(string txt)
        {
            mainWindow.AppendToErrorLog(txt);
        }

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region Threading methods

        public void ClearAnalysisRunStats()
        {
            threadManager.ClearAnalysisRunStats();
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
        //public void AddBlock(Block b, string[] demodulationConfigs)
        //{
        //    log("Adding block " + b.Config.Settings["cluster"] + " - " + b.Config.Settings["clusterIndex"]);
        //    BlockDemodulator blockDemodulator = new BlockDemodulator();
        //    foreach (string dcName in demodulationConfigs)
        //    {
        //        DemodulationConfig dc = DemodulationConfig.GetStandardDemodulationConfig(dcName, b);
        //        //DemodulatedBlock dBlock = blockDemodulator.DemodulateBlockNL(b, dc);
        //        DemodulatedBlock dBlock = blockDemodulator.DemodulateBlock(b, dc);
        //        blockStore.AddDBlock(dBlock);
        //    }
        //}

        public void AddBlock(Block b)
        {
            TOFBlockDemodulator blockDemodulator = new TOFBlockDemodulator();
            log("Adding block " + b.Config.Settings["cluster"] + " - " + b.Config.Settings["clusterIndex"]);
            TOFDemodulatedBlock tdBlock = blockDemodulator.TOFDemodulateBlock(b);
            blockStore.AddDBlock(tdBlock);
        }

        public void AddGatedBlock(Block b, GatedDemodulationConfig gateConfig)
        {
            GatedBlockDemodulator blockDemodulator = new GatedBlockDemodulator();
            log("Adding block " + b.Config.Settings["cluster"] + " - " + b.Config.Settings["clusterIndex"] +
                " with gate: " + gateConfig.Name
                );
            GatedDemodulatedBlock gdBlock = blockDemodulator.GateThenDemodulateBlock(b, gateConfig);
            blockStore.AddDBlock(gdBlock);
        }


        // This method is thread-safe.
        //public void AddBlock(string path, string[] demodulationConfigs)
        //{
        //    string[] splitPath = path.Split('\\');
        //    log("Loading block " + splitPath[splitPath.Length - 1]);
        //    BlockSerializer bs = new BlockSerializer();
        //    Block b = bs.DeserializeBlockFromZippedXML(path, "block.xml");
        //    AddBlock(b, demodulationConfigs);
        //}

        public Block LoadBlockFromFile(string path)
        {
            string[] splitPath = path.Split('\\');
            log("Loading block " + splitPath[splitPath.Length - 1]);
            BlockSerializer bs = new BlockSerializer();
            Block b = bs.DeserializeBlockFromZippedXML(path, "block.xml");
            return b;
        }

        public void AddBlock(string path)
        {
            Block b = LoadBlockFromFile(path);
            AddBlock(b);
        }

        public void AddGatedBlock(string path, GatedDemodulationConfig gateConfig)
        {
            Block b = LoadBlockFromFile(path);
            AddGatedBlock(b, gateConfig);
        }

        // Use this to add blocks to SirCachealot's analysis queue.
        //public void AddBlockToQueue(string path, string[] demodulationConfigs)
        //{
        //    blockAddParams bap = new blockAddParams();
        //    bap.path = path;
        //    bap.demodulationConfigs = demodulationConfigs;
        //    threadManager.AddToQueue(AddBlockThreadWrapper, bap);
        //}

        public void GateTOFDemodulatedBlock(DemodulatedBlock dblock, GatedDemodulationConfig gateConfig)
        {
            TOFDemodulatedBlock tdblock = dblock as TOFDemodulatedBlock;
            GatedDemodulatedBlock gdblock = new GatedDemodulatedBlock();

            if (tdblock == null)
            {
                log("Error: Not a TOF demodulated block.");
                return;
            }

            GatedBlockDemodulator gatedDemodulator = new GatedBlockDemodulator();
            gdblock = gatedDemodulator.GateTOFDemodulatedBlock(tdblock, gateConfig);

            blockStore.AddDBlock(gdblock);
        }

        public void AddBlockToQueue(string path)
        {
            blockAddParams bap = new blockAddParams();
            bap.path = path;
            threadManager.AddToQueue(AddBlockThreadWrapper, bap);
        }

        // Use this to add blocks to SirCachealot's analysis queue.
        //public void AddBlocksToQueue(string[] paths, string[] demodulationConfigs)
        //{
        //    foreach (string path in paths)
        //    {
        //        blockAddParams bap = new blockAddParams();
        //        bap.path = path;
        //        bap.demodulationConfigs = demodulationConfigs;
        //        threadManager.AddToQueue(AddBlockThreadWrapper, bap);
        //    }
        //}

        public void AddBlocksToQueue(string[] paths)
        {
            foreach (string path in paths)
            {
                blockAddParams bap = new blockAddParams();
                bap.path = path;
                threadManager.AddToQueue(AddBlockThreadWrapper, bap);
            }
        }

        // this method and the following struct are wrappers so that we can add a block
        // with a single parameter, as required by the threadpool.
        //private void AddBlockThreadWrapper(object parametersIn)
        //{
        //    threadManager.QueueItemWrapper(delegate(object parms)
        //    {
        //        blockAddParams parameters = (blockAddParams)parms;
        //        AddBlock(parameters.path, parameters.demodulationConfigs);
        //    },
        //    parametersIn
        //    );
        //}

        private void AddBlockThreadWrapper(object parametersIn)
        {
            threadManager.QueueItemWrapper(delegate (object parms)
            {
                blockAddParams parameters = (blockAddParams)parms;
                AddBlock(parameters.path);
            },
            parametersIn
            );
        }

        //private struct blockAddParams
        //{
        //    public string path;
        //    public string[] demodulationConfigs;
        //    // this struct has a ToString method defined for error reporting porpoises.
        //    public override string ToString()
        //    {
        //        return path;
        //    }
        //}

        private struct blockAddParams
        {
            public string path;
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

        public void LoadGateSet(string path)
        {
            GateManager gateManager = new GateManager();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "xml gate set|*.xml";
            dialog.Title = "Open gate set";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                System.IO.FileStream fs =
                    (System.IO.FileStream)dialog.OpenFile();
                gateManager.LoadGateSetFromXml(fs);
                fs.Close();
            }
            gateSet = gateManager.GateSet;
        }

        private string GetDatabaseStats()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Database queries: " + blockStore.QueryCount);
            b.AppendLine("DBlocks served: " + blockStore.DBlockCount);
            return b.ToString();
        }

        #endregion

        #region TOFDemodulation

        //public void TOFDemodulateBlocks(string[] blockFiles, string savePath, string[] detectorNames)
        //{
        //    // first of all test that the save location exists to avoid later disappointment.

        //    if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        //    {
        //        log("Save path does not exist!!");
        //        return;
        //    }

        //    // initialise the TOF accumulator dictionary
        //    tcsaDictionary = new Dictionary<string, TOFChannelSetAccumulator>();
        //    foreach (string detectorName in detectorNames)
        //    {
        //        tcsaDictionary.Add(detectorName, new TOFChannelSetAccumulator());
        //    }

        //    // queue the blocks - the last block analysed will take care of saving the results.
        //    foreach (string blockFile in blockFiles)
        //    {
        //        TofDemodulateParams tdp = new TofDemodulateParams();
        //        tdp.blockPath = blockFile;
        //        tdp.savePath = savePath;
        //        tdp.detectorNames = detectorNames;
        //        threadManager.AddToQueue(TOFDemodulateThreadWrapper, tdp);
        //    }
        //}

        //private void TOFDemodulateBlock(string blockPath, string savePath, string[] detectorNames)
        //{
        //    BlockSerializer bs = new BlockSerializer();
        //    string[] splitPath = blockPath.Split('\\');
        //    log("Loading block " + splitPath[splitPath.Length - 1]);
        //    Block b = bs.DeserializeBlockFromZippedXML(blockPath, "block.xml");
        //    TOFBlockDemodulator bd = new TOFBlockDemodulator();

        //    log("TOF Demodulating block " + b.Config.Settings["cluster"] + " - " + b.Config.Settings["clusterIndex"]);
        //    foreach (string detectorName in detectorNames)
        //    {
        //        TOFDemodulatedBlock tdb = bd.TOFDemodulateBlock(b);

        //        string savePathTDB = savePath + detectorName + b.Config.Settings["cluster"] + "-" + b.Config.Settings["clusterIndex"] + ".bin";
        //        log("Saving TOF Channel Set for " + detectorName + " - " + b.Config.Settings["cluster"] + " - " + b.Config.Settings["clusterIndex"]);
        //        Stream fs = new FileStream(savePathTDB, FileMode.Create);
        //        (new BinaryFormatter()).Serialize(fs, tdb);
        //        fs.Close();
        //    }
        //}

        //private void TOFDemodulateThreadWrapper(object parametersIn)
        //{
        //    threadManager.QueueItemWrapper(delegate(object parms)
        //    {
        //        TofDemodulateParams parameters = (TofDemodulateParams)parms;
        //        TOFDemodulateBlock(parameters.blockPath, parameters.savePath, parameters.detectorNames);
        //    },
        //    parametersIn
        //    );
        //}
        //private struct TofDemodulateParams
        //{
        //    public string blockPath;
        //    public string savePath;
        //    public string[] detectorNames;
        //    // this struct has a ToString method defined for error reporting porpoises.
        //    public override string ToString()
        //    {
        //        return blockPath;
        //    }
        //}

        #endregion

        #region Testing

        // Somewhere for SirCachealot to store test results that's accessible by Mathematica.
        // Makes debugging easier and is needed as a workaround for the constant Mathematica
        // NET/Link errors.
//        public TOFChannelSetGroup ChanSetGroup;
        // workarounds for NET/Link bugs
        //public TOFChannelSet GetAveragedChannelSet(bool eSign, bool bSign, bool rfSign)
        //{
        //    return ChanSetGroup.AverageChannelSetSignedByMachineState(eSign, bSign, rfSign);
        //}

        //public void LoadChannelSetGroup(string path)
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    FileStream fs = new FileStream(path, FileMode.Open);
        //    ChanSetGroup = (TOFChannelSetGroup)bf.Deserialize(fs);
        //    fs.Close();
        //}


        public void Test1()
        {
            
        }

        public void Test2()
        {
            //string[] blockPath = new string[1];
            //blockPath[0] = "C:\\Users\\cjh211\\Box\\EDM (Electron EDM)\\Data\\sedm\\v3\\2019\\September2019\\09Sep1928_1.zip";
            //string savePath = @"C:\Users\cjh211\Desktop\scTest\";
            //TOFDemodulateBlocks(blockPath, savePath, new string[3] {"asymmetry","topProbeNoBackground","NorthCurrent"});
        }

        #endregion
    }
}
