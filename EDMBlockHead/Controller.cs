using System;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

using EDMBlockHead.Acquire;
using EDMBlockHead.GUI;

using Analysis;
using Analysis.EDM;
using Data;
using Data.EDM;

using DAQ.Environment;
using DAQ.HAL;
using EDMConfig;


namespace EDMBlockHead
{
    /// <summary>
    /// This class is in control of BlockHead. It has a window to interact with the user
    /// and an acquisitor to take the data.
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        private const int UPDATE_EVERY = 5;

        #region Class members

        private MainWindow mainWindow;
        private LiveViewer liveViewer;
        private Acquisitor acquisitor;
        public Block Block;
        private BlockConfig config;
        private XmlSerializer serializer = new XmlSerializer(typeof(BlockConfig));
        BlockSerializer blockSerializer = new BlockSerializer();
        BlockDemodulator blockDemodulator = new BlockDemodulator();
        public DemodulatedBlock DBlock;
        private bool haveBlock = false;

        // State information
        public enum AppState { stopped, running, remote };
        public AppState appState = AppState.stopped;

        // it's a singleton
        private static Controller controllerInstance;

        //DK LiveViewer
        int blockCount=1;
        double clusterVariance=0;
        double clusterVarianceNormed=0;
        double blocksPerDay = 240;
        double plateSpacing = 1.1;
        double electronCharge = 1.6022 * Math.Pow(10, -19);
        double bohrMagneton = 9.274 * Math.Pow(10, -24);
        double saturatedEffectiveField = 26 * Math.Pow(10, 9);

        #endregion

        #region Initialisation and access

        // This is the right way to get a reference to the controller. You shouldn't create a
        // controller yourself.
        public static Controller GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new Controller();
            }
            return controllerInstance;
        }

        public Controller() { }

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void StartApplication()
        {
            // stuff the config with a default config, makes it easy to save out
            // a blank config.
            MakeDefaultBlockConfig();

            // ask the remoting system for access to the EDMHardwareController
            RemotingConfiguration.RegisterWellKnownClientType(
                Type.GetType("EDMHardwareControl.Controller, EDMHardwareControl"),
                "tcp://localhost:1172/controller.rem"
                );

            // ask the remoting system for access to ScanMaster 
            RemotingConfiguration.RegisterWellKnownClientType(
                Type.GetType("ScanMaster.Controller, ScanMaster"),
                "tcp://localhost:1170/controller.rem"
                );

            // ask the remoting system for access to PhaseLock
            RemotingConfiguration.RegisterWellKnownClientType(
                Type.GetType("EDMPhaseLock.MainForm, EDMPhaseLock"),
                "tcp://localhost:1175/controller.rem"
                );

            mainWindow = new MainWindow(this);
            acquisitor = new Acquisitor();
            mainWindow.textArea.Text = "BlockHead!" + Environment.NewLine;

            liveViewer = new LiveViewer(this);
            liveViewer.Show();
            liveViewer.UpdateStatusText("EDMErr\t" + "normedErr\t" + "B\t" + "DB\t" + "DB/SIG" + "\t" + Environment.NewLine);

            Application.Run(mainWindow);
        }

        // this function creates a default BlockConfig that is sufficient to
        // get BlockHead running. The recommended way of making a new config
        // is to use BlockHead to save this config and then modify it.
        private void MakeDefaultBlockConfig()
        {
            const int CODE_LENGTH = 12;
            config = new BlockConfig();

            DigitalModulation dm = new DigitalModulation();
            dm.Name = "E";
            dm.Waveform = new Waveform("E Modulation", CODE_LENGTH);
            dm.Waveform.Code = new bool[] { true, true, false, false, false, false, false, false, false, false, false, false };
            config.DigitalModulations.Add(dm);

            DigitalModulation pi = new DigitalModulation();
            pi.Name = "PI";
            pi.Waveform = new Waveform("Pi Modulation", CODE_LENGTH);
            pi.Waveform.Code = new bool[] { false, false, false, false, false, false, false, false, false, false, false, true };
            config.DigitalModulations.Add(pi);

            AnalogModulation b = new AnalogModulation();
            b.Name = "B";
            b.Waveform = new Waveform("B Modulation", CODE_LENGTH);
            b.Waveform.Code = new bool[] { false, false, false, false, false, false, false, false, false, false, false, true };
            b.DelayAfterSwitch = 5;
            b.Centre = 0;
            b.Step = 0.46;
            config.AnalogModulations.Add(b);

            AnalogModulation db = new AnalogModulation();
            db.Name = "DB";
            db.Waveform = new Waveform("DB Modulation", CODE_LENGTH);
            db.Waveform.Code = new bool[] { false, false, false, false, false, false, false, false, false, false, true, false };
            db.DelayAfterSwitch = 5;
            db.Centre = 0;
            db.Step = 0.1;
            config.AnalogModulations.Add(db);

            AnalogModulation rf1A = new AnalogModulation();
            rf1A.Name = "RF1A";
            rf1A.Waveform = new Waveform("rf1 Amplitude modulation", CODE_LENGTH);
            rf1A.Waveform.Code = new bool[] { false, false, false, false, false, false, false, false, false, false, true, false };
            rf1A.DelayAfterSwitch = 0;
            rf1A.Centre = 1.5;
            rf1A.Step = 0.1;
            config.AnalogModulations.Add(rf1A);

            AnalogModulation rf2A = new AnalogModulation();
            rf2A.Name = "RF2A";
            rf2A.Waveform = new Waveform("rf2 Amplitude modulation", CODE_LENGTH);
            rf2A.Waveform.Code = new bool[] { false, false, false, false, false, false, false, false, false, false, true, false };
            rf2A.DelayAfterSwitch = 0;
            rf2A.Centre = 2.5;
            rf2A.Step = 0.1;
            config.AnalogModulations.Add(rf2A);

            AnalogModulation rf1F = new AnalogModulation();
            rf1F.Name = "RF1F";
            rf1F.Waveform = new Waveform("rf1 frequency modulation", CODE_LENGTH);
            rf1F.Waveform.Code = new bool[] { false, false, false, false, false, false, false, false, false, false, true, false };
            rf1F.DelayAfterSwitch = 0;
            rf1F.Centre = 2.5;
            rf1F.Step = 0.1;
            config.AnalogModulations.Add(rf1F);

            AnalogModulation rf2F = new AnalogModulation();
            rf2F.Name = "RF2F";
            rf2F.Waveform = new Waveform("rf2 frequency modulation", CODE_LENGTH);
            rf2F.Waveform.Code = new bool[] { false, false, false, false, false, false, false, false, false, false, true, false };
            rf2F.DelayAfterSwitch = 0;
            rf2F.Centre = 2.5;
            rf2F.Step = 0.1;
            config.AnalogModulations.Add(rf2F);

            AnalogModulation lf1 = new AnalogModulation();
            lf1.Name = "LF1";
            lf1.Waveform = new Waveform("laser frequency 1 modulation", CODE_LENGTH);
            lf1.Waveform.Code = new bool[] { false, true, false, true, false, false, false, false, false, false, false, false };
            lf1.DelayAfterSwitch = 0;
            lf1.Centre = 2.5;
            lf1.Step = 0.05;
            config.AnalogModulations.Add(lf1);

            config.Settings["codeLength"] = CODE_LENGTH;
            config.Settings["numberOfPoints"] = 4096;
            config.Settings["pgClockFrequency"] = 1000000;
            config.Settings["eDischargeTime"] = 1000;
            config.Settings["eBleedTime"] = 1000;
            config.Settings["eSwitchTime"] = 500;
            config.Settings["eChargeTime"] = 1000;
            config.Settings["magnetCalibration"] = 16.5;

			config.Settings["eState"] = true;
			config.Settings["bState"] = true;
            config.Settings["rfState"] = true;
			config.Settings["ePlus"] = 8.0;
			config.Settings["eMinus"] = -8.0;
			config.Settings["gtPlus"] = 1.6;
			config.Settings["gtMinus"] = -1.6;
			config.Settings["gbPlus"] = 1.6;
			config.Settings["gbMinus"] = -1.6;

        }

        public void StopApplication()
        {
            if (appState == AppState.running) StopAcquisition();
        }

        #endregion

        #region Remote methods

        public void CaptureRemote()
        {
            mainWindow.EnableMenus(false);
        }

        public void ReleaseRemote()
        {
            mainWindow.EnableMenus(true);
        }

        public void StartAcquisition()
        {
            if (appState != AppState.running)
            {
                Status = "Acquiring ...";
                acquisitor.Start(config);
                appState = AppState.running;
                mainWindow.AppendToTextArea("Starting acquisition ...");
            }
        }

        public void StopAcquisition()
        {
            if (appState == AppState.running)
            {
                acquisitor.Stop();
                appState = AppState.stopped;
                Status = "Ready.";
                mainWindow.AppendToTextArea("Acquisition stopping ...");
            }
        }

        public void StartPattern()
        {
            ScanMaster.Controller scanMaster = new ScanMaster.Controller();
            scanMaster.SelectProfile("Scan B");
            scanMaster.OutputPattern();
        }

        public void StopPattern()
        {
            ScanMaster.Controller scanMaster = new ScanMaster.Controller();
            scanMaster.StopPatternOutput();
        }

        public void AcquireAndWait()
        {
            Monitor.Enter(acquisitor.MonitorLockObject);
            StartAcquisition();
            Monitor.Wait(acquisitor.MonitorLockObject);
            Monitor.Exit(acquisitor.MonitorLockObject);
        }

        public void LoadConfig(String path)
        {
            FileStream fs = File.Open(path, FileMode.Open);
            LoadConfig(fs);
            fs.Close();
            mainWindow.AppendToTextArea("Loaded config " + path);
        }

        public void SaveBlock(String path)
        {
            if (haveBlock)
            {
                SaveBlock(File.Create(path));
                mainWindow.AppendToTextArea("Saved block to " + path);
            }
        }

        public BlockConfig Config
        {
            set
            {
                config = value;
            }
            get
            {
                return config;
            }
        }

        #endregion

        #region Local methods

        public void AcquisitionFinished(Block b)
        {
            this.Block = b;
            mainWindow.AppendToTextArea("Demodulating block.");
            DemodulationConfig dc = new DemodulationConfig();
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
            DBlock = blockDemodulator.DemodulateBlock(b, dc);
            
            //edm factor calculation
            double dbStep = ((AnalogModulation)Config.GetModulationByName("DB")).Step; 
            double magCal = (double)Config.Settings["magnetCalibration"];
            double eField = cField((double)Config.Settings["ePlus"], (double)Config.Settings["eMinus"]);//arguments are in volts not kV
            double edmFactor = (bohrMagneton * dbStep * magCal * Math.Pow(10, -9)) /
                        (electronCharge * saturatedEffectiveField * polarisationFactor(eField));

            //Get relevant channel values and errors
            double[] sigValandErr = DBlock.GetChannelValueAndError(new string[] { "SIG" }, "top");
            double[] BValandErr = DBlock.GetChannelValueAndError(new string[] { "B" }, "top");
            double[] dbValandErr = DBlock.GetChannelValueAndError(new string[] { "DB" }, "top");
            double[] EValandErr = DBlock.GetChannelValueAndError(new string[] { "E" }, "top");
            double[] EdotBValandErr = DBlock.GetChannelValueAndError(new string[] { "E", "B" }, "top");
           
            //edm error calculation
            double rawEDM = edmFactor *(EdotBValandErr[0] / dbValandErr[0]);
            double rawEDMErr = Math.Abs(rawEDM) * Math.Sqrt(Math.Pow(EdotBValandErr[1] / EdotBValandErr[0], 2)
                               + Math.Pow(dbValandErr[1] /dbValandErr[0],2));

            //same again for normed data.
            double[] sigValandErrNormed = DBlock.GetChannelValueAndError(new string[] { "SIG" }, "topNormed");
            double[] BValandErrNormed = DBlock.GetChannelValueAndError(new string[] { "B" }, "topNormed");
            double[] dbValandErrNormed = DBlock.GetChannelValueAndError(new string[] { "DB" }, "topNormed");
            double[] EValandErrNormed = DBlock.GetChannelValueAndError(new string[] { "E" }, "topNormed");
            double[] EdotBValandErrNormed = DBlock.GetChannelValueAndError(new string[] { "E", "B" }, "topNormed");

            //normed edm error
            double rawEDMNormed = edmFactor * (EdotBValandErrNormed[0] / dbValandErrNormed[0]);
            double rawEDMErrNormed = Math.Abs(rawEDMNormed) * Math.Sqrt(Math.Pow(EdotBValandErrNormed[1] / EdotBValandErrNormed[0], 2)
                                     + Math.Pow(dbValandErrNormed[1] / dbValandErrNormed[0], 2));
            
            //Append LiveViewer text with edm errors, B, DB & DB/SIG
            liveViewer.AppendStatusText((Math.Pow(10, 26) * rawEDMErr).ToString("G3") + "\t" + (Math.Pow(10, 26) * rawEDMErrNormed).ToString("G3")
                + "\t\t" + (BValandErr[0]).ToString("N2") + "\t" + (dbValandErr[0]).ToString("N2")
                + "\t" + (dbValandErr[0] / sigValandErr[0]).ToString("N3") + Environment.NewLine);
        
            // Rollings values of edm error
            clusterVariance = ((clusterVariance*(blockCount - 1)) + rawEDMErr*rawEDMErr) / blockCount;
            double edmPerDay = Math.Sqrt(clusterVariance / blocksPerDay);
            clusterVarianceNormed = ((clusterVarianceNormed * (blockCount - 1)) + rawEDMErrNormed * rawEDMErrNormed) / blockCount;
            double edmPerDayNormed = Math.Sqrt(clusterVarianceNormed / blocksPerDay);

            liveViewer.UpdateClusterStatusText("errorPerDay: " + edmPerDay.ToString("E3") + "\terrorPerDayNormed: " + edmPerDayNormed.ToString("E3")
                + Environment.NewLine + "block count: " + blockCount);

            //Update Plots
            liveViewer.AppendToSigScatter(new double[] { blockCount }, new double[] { sigValandErr[0] });
            liveViewer.AppendTobScatter(new double[] { blockCount }, new double[] { BValandErr[0] });
            liveViewer.AppendTodbScatter(new double[] { blockCount }, new double[] { dbValandErr[0] });
            liveViewer.AppendToedmScatter(new double[] { blockCount }, new double[] { Math.Pow(10, 26) * rawEDMErr });
            liveViewer.AppendToedmNormedScatter(new double[] { blockCount }, new double[] { Math.Pow(10, 26) * rawEDMErrNormed });
            
            liveViewer.AppendSigmaTosigScatter(new double[] { blockCount }, new double[] { sigValandErr[0] + sigValandErr[1] },
                new double[] { sigValandErr[0] - sigValandErr[1] });
            liveViewer.AppendSigmaTobScatter(new double[] { blockCount }, new double[] { BValandErr[0] + BValandErr[1] },
                new double[] { BValandErr[0] - BValandErr[1] });
            liveViewer.AppendSigmaTodbScatter(new double[] { blockCount }, new double[] { dbValandErr[0] + dbValandErr[1] },
                new double[] { dbValandErr[0] - dbValandErr[1] });
            
            blockCount = blockCount + 1;
            
            //config.g
            haveBlock = true;
            appState = AppState.stopped;
            mainWindow.AppendToTextArea("Acquisition finished");
            SetStatusReady();
        }
        
        public double polarisationFactor(double electricField/*in kV/cm*/)
        {
            double polFactor = 0.00001386974812686904
                + 0.09020507207607358 * electricField
                + 0.0008134261949342792 * Math.Pow(electricField, 2)
                - 0.001365930559363722 * Math.Pow(electricField, 3)
                + 0.00016467328012807663 * Math.Pow(electricField, 4)
                - 9.53482 * Math.Pow(10, -6) * Math.Pow(electricField, 5)
                + 2.804041112473679 * Math.Pow(10, -7) * Math.Pow(electricField, 6)
                - 3.352604355536456 * Math.Pow(10, -9) * Math.Pow(electricField, 7);

            return polFactor;
        }
        
        public double cField(double ePlus,double eMinus)//voltage in volts returns kV/cm
        {
            double efield = (ePlus - eMinus) / plateSpacing;
            return efield / 1000;
        }

        public void resetEdmErrRunningMeans()
        {
            blockCount = 1;
            clusterVariance = 0;
            clusterVarianceNormed = 0;
            liveViewer.UpdateClusterStatusText("errorPerDay: " + 0 + "\terrorPerDayNormed: " + 0
                + Environment.NewLine + "block count: " + 0);
            liveViewer.UpdateStatusText("EDMErr\t" + "normedErr\t" + "B\t" + "DB\t" + "DB/SIG" + "\t" + Environment.NewLine);
            liveViewer.ClearSigScatter();
            liveViewer.ClearbScatter();
            liveViewer.CleardbScatter();
            liveViewer.ClearedmErrScatter();            
        }

        double[] northLeakages = new double[UPDATE_EVERY];
        double[] southLeakages = new double[UPDATE_EVERY];
        int leakageIndex = 0;
        public void GotPoint(int point, EDMPoint p)
        {
            // store the leakage measurements ready for the graph update
            northLeakages[leakageIndex] = (double)p.SinglePointData["NorthCurrent"];
            southLeakages[leakageIndex] = (double)p.SinglePointData["SouthCurrent"];
            leakageIndex++;

            if ((point % UPDATE_EVERY) == 0)
            {
                Shot data = p.Shot;
                mainWindow.TankLevel = point;

                TOF tof = (TOF)data.TOFs[0];
                mainWindow.PlotTOF(0, tof.Data, tof.GateStartTime, tof.ClockPeriod);
                tof = (TOF)data.TOFs[1];
                mainWindow.PlotTOF(1, tof.Data, tof.GateStartTime, tof.ClockPeriod);
                tof = (TOF)data.TOFs[2];
                mainWindow.PlotTOF(2, tof.Data, tof.GateStartTime, tof.ClockPeriod);

                // update the leakage graphs
                mainWindow.AppendLeakageMeasurement(new double[]{northLeakages[0]}, new double[]{southLeakages[0]});
                leakageIndex = 0;
            }
        }

        public void LoadConfig()
        {
            mainWindow.StatusText = "Loading config ...";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "xml block config|*.xml";
            dialog.Title = "Open block config file";
            dialog.InitialDirectory = Environs.FileSystem.Paths["settingsPath"] + "BlockHead";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                System.IO.FileStream fs =
                    (System.IO.FileStream)dialog.OpenFile();
                LoadConfig(fs);
                fs.Close();
                SetStatusReady();
                mainWindow.AppendToTextArea("Loaded config.");
                //				mainWindow.progressTank.Range= new NationalInstruments.UI.Range(0, (int)config.Settings["NumberOfPoints"]);
            }
        }

        private void LoadConfig(FileStream fs)
        {
            config = (BlockConfig)serializer.Deserialize(fs);
        }

        public void SaveConfig()
        {
            mainWindow.StatusText = "Saving config ...";
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "xml block config|*.xml";
            dialog.Title = "Save block config file";
            dialog.InitialDirectory = Environs.FileSystem.Paths["settingsPath"] + "BlockHead";
            dialog.ShowDialog();
            if (dialog.FileName != "")
            {
                System.IO.FileStream fs =
                    (System.IO.FileStream)dialog.OpenFile();
                serializer.Serialize(fs, config);
                fs.Close();
            }
            mainWindow.AppendToTextArea("Saved config.");
            SetStatusReady();
        }

        public String Status
        {
            set
            {
                mainWindow.StatusText = value;
            }
        }

        public void SaveBlock()
        {
            if (haveBlock)
            {
                mainWindow.StatusText = "Saving block ...";
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "zipped xml block|*.zip";
                dialog.Title = "Save block";
                dialog.ShowDialog();
                if (dialog.FileName != "")
                {
                    System.IO.FileStream fs =
                        (System.IO.FileStream)dialog.OpenFile();
                    SaveBlock(fs);
                    fs.Close();
                }
                mainWindow.AppendToTextArea("Saved block.");
                SetStatusReady();
            }
        }

        private void SaveBlock(FileStream fs)
        {
            blockSerializer.SerializeBlockAsZippedXML(fs, Block);
        }

        private void SetStatusReady()
        {
            if (haveBlock) Status = "Ready. Block in memory.";
            else Status = "Ready.";
        }

        internal void ShowLiveViewer()
        {
            liveViewer.Show();
        }

        #endregion


        internal void TestLiveAnalysis()
        {
            new Thread(new ThreadStart(ActuallyTestLiveAnalysis)).Start();
        }

        internal void ActuallyTestLiveAnalysis()
        {
            for (int i = 1; i <= 20; i++)
            {
                string fileRoot = Environs.FileSystem.Paths["edmDataPath"] + "\\2009\\April2009\\30apr0900_";
                BlockSerializer bs = new BlockSerializer();
                Block b = bs.DeserializeBlockFromZippedXML(fileRoot + (i.ToString()) + ".zip", "block.xml");
                AcquisitionFinished(b);
            }
        }
    }
}
