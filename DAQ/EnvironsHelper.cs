using System;
using System.Collections;

using DAQ.HAL;

namespace DAQ.Environment
{
    /// <summary>
    /// The EnvironsHelper class helps to initialize the Enivirons class for different computers.
    /// It also allows access to the methods from other computers. 
    /// </summary>
    public class EnvironsHelper
    {
        /// <summary>
        /// The Hardware that is present in this Environs, presented through a
        /// standardised interface: Hardware.
        /// </summary>
        public Hardware Hardware;

        /// <summary>
        /// This is where details of the file system specific to a computer belong.
        /// </summary>
        public FileSystem FileSystem;

        /// <summary>
        /// This is where calibrations for specific experiments go.
        /// </summary>
        //public static HardwareCalibrationLibrary HardwareCalibrationLibrary;

        /// <summary>
        /// This hashtable stores information about the system (as Strings). You can put
        /// pretty much anything in here that's specific to a particular computer.
        /// </summary>
        public Hashtable Info = new Hashtable();

        /// <summary>
        /// The global debug flag. If this flag is set to true, no hardware calls will be made
        /// (if you're writing a hardware class it's your responsibility to observe this flag)
        /// and data will be synthesized.
        /// </summary>
        public bool Debug;
        private string computer;

        /// <summary>
        /// Computer name for wavemeter lock server.
        /// This is the name of the computer on which your wavemeter server is running.
        /// Add this to the computer config on which you run the wavemeter server and wavemeter lock.
        /// </summary>
        public string serverComputerName;

        /// <summary>
        /// TCP channel for wavemeter lock server.
        /// This is the channel that the wavemeter server brodcast the measured frequency.
        /// Add this to the computer config on which you run the wavemeter server and wavemeter lock.
        /// </summary>
        public int serverTCPChannel;

        /// <summary>
        /// TCP channel for wavemeter lock.
        /// This is the channel shared between wavemeter lock and other programmes.
        /// Add this to the computer config on which you run the wavemeter lock.
        /// </summary>
        public int wavemeterLockTCPChannel;

        /// <summary>
        /// Experiment type is for code that needs to know what experiment it's running on.
        /// </summary>
        //public static String ExperimentType;

        /// <summary>
        /// Initialise the environment. This code switches on computer name and
        /// sets up the environment accordingly.
        /// </summary>
        /// 
        public EnvironsHelper()
        {
            String computerName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
            InitializeEnvirons(computerName);

        }

        public EnvironsHelper(string computer)
        {
            this.computer = computer;
            InitializeEnvirons(computer);
        }


        void InitializeEnvirons(string computerName)
        {

            switch (computerName)
            {

                case "PH-NI-LAB":
                    Hardware = new GobelinHardware();
                    FileSystem = new FileSystem();
                    Debug = true;
                    //ExperimentType = "edm";
                    break;

                case "Rhys-XPS":
                    Hardware = new BufferClassicHardware();
                    FileSystem = new RhysFileSystem();
                    Debug = true;
                    break;


                case "PH-ULTRAEDM":
                    Hardware = new PHULTRAEDMHardware();
                    FileSystem = new PHULTRAEDMFileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;

                case "Centaur":
                    Hardware = new CentaurEDMHardware();
                    FileSystem = new CentaurEDMFileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;

                case "PH-NFITCH-2":
                    Hardware = new ZeemanSisyphusHardware();
                    FileSystem = new PHNFITCH2FileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;

                case "PH-DK902":
                    Hardware = new EDMHardware();
                    FileSystem = new PhkaraFileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;

                case "CRASH1":
                    Hardware = new MoleculeMOTHardware();
                    FileSystem = new CrashFileSystem();
                    Debug = false;
                    //ExperimentType = "decelerator";
                    break;

                case "SCHNAPS":
                    Hardware = new MoleculeMOTHardware();
                    FileSystem = new SchnapsFileSystem();
                    //ExperimentType = "decelerator";
                    Info.Add("SwitchSequenceCode", "SwitchSequenceV1`");
                    Debug = false;
                    break;

                case "SUNSHINE":
                    Hardware = new MoleculeMOTHardware();
                    FileSystem = new SunshineFileSystem();
                    //ExperimentType = "decelerator";
                    Info.Add("SwitchSequenceCode", "SwitchSequenceV1`");
                    Debug = false;
                    break;

#if CaF || ZS
                case "PH-BONESAW":
                    Hardware = new MoleculeMOTHardware();
                    FileSystem = new PHBonesawFileSystem();
                    serverComputerName = "IC-CZC136CFDJ";
                    serverTCPChannel = 1984;
                    wavemeterLockTCPChannel = 6666;
                    Info.Add("SwitchSequenceCode", "SwitchSequenceV1`");
                    Debug = false;
                    break;
#endif

                case "CLAM":
                    Hardware = new SympatheticHardware();
                    FileSystem = new ClamFileSystem();
                    Debug = true;
                    Info.Add("SwitchSequenceCode", "WFSwitchSequenceV1`");
                    //ExperimentType = "decelerator";
                    break;

                case "CHROME1":
                    Hardware = new EDMHardware();
                    FileSystem = new ChromeFileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;

                case "IC-CZC202DMH1":
                    Hardware = new CaFBECHardware();
                    FileSystem = new CaFBECFileSystem();
                    Debug = false;
                    serverComputerName = "IC-CZC136CFDJ";
                    serverTCPChannel = 1984;
                    wavemeterLockTCPChannel = 1234;
                    //ExperimentType = "edm";
                    break;

                #if EDM
                case "PIXIE":
                    Hardware = new PXIEDMHardware();
                    FileSystem = new PixieFileSystem();
                    Debug = false;
                    serverComputerName = "IC-CZC136CFDJ";
                    wavemeterLockTCPChannel = 1012;
                    serverTCPChannel = 1984;
                    //ExperimentType = "edm";
                    break;
#endif

                //PC running TCL for EDM
                case "GREMLIN":
                    Hardware = new TCLEDMHardware();
                    FileSystem = new TCLEDMFileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;

                case "PH-CJH211":
                    Hardware = new EDMTestCrateHardware();
                    FileSystem = new PixieFileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;


                case "PH-JKITE":
                    Hardware = new EDMHardware();
                    FileSystem = new PHJKiteFileSystem();
                    Debug = true;
                    //ExperimentType = "edm";
                    break;

                case "TURTLETAMER":
                    Hardware = new EDMHardware();
                    FileSystem = new SealClubberFileSystem();
                    Debug = true;
                    //ExperimentType = "edm";
                    break;

                //case "SEALCLUBBER":
                //    Hardware = new MoleculeMOTHardware();
                //    FileSystem = new SealClubberFileSystem();
                //    Debug = true;
                //    ExperimentType = "decelerator";
                //    break;

                case "GANYMEDE0":
                    Hardware = new SympatheticHardware();
                    FileSystem = new GanymedeFileSystem();
                    Debug = false;
                    //ExperimentType = "lih";
                    Info.Add("SwitchSequenceCode", "WFSwitchSequenceV1`");
                    break;

                case "CARMELITE":
                    Hardware = new BufferGasHardware();
                    FileSystem = new CarmeliteFileSystem();
                    Debug = false;
                    //ExperimentType = "buffer";
                    break;

                case "YBF":
                    Hardware = new EDMHardware();
                    FileSystem = new YBFFileSystem();
                    Debug = true;
                    //ExperimentType = "edm";
                    break;

                case "PH-CDSLAP":
                    Hardware = new BufferGasHardware();
                    FileSystem = new PHCDSLapFileSystem();
                    Debug = true;
                    //ExperimentType = "edm";
                    break;

                case "RAINBOW":
                    Hardware = new RainbowHardware();
                    FileSystem = new RainbowFileSystem();
                    Debug = false;
                    break;


                //13 Jan 2021 (Chris): I've taken this computer to replace
                //the computer running TCL for the EDM experiment.
                //case "PH-REQUIEM":
                //    Hardware = new PXISympatheticHardware();
                //    FileSystem = new RequiemFileSystem();
                //    Debug = false;
                //    break;

                case "PH-REQUIEM":
                    Hardware = new TCLEDMHardware();
                    FileSystem = new TCLEDMFileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;

                case "PH-RAGNAROK":
                    Hardware = new SympatheticHardware();
                    FileSystem = new RagnarokFileSystem();
                    Debug = false;
                    break;

                case "PH-RHENDRIC0":
                    Hardware = new BufferGasHardware();
                    FileSystem = new PHRHENDRIC0FileSystem();
                    Debug = false;
                    break;

                case "SSWARB":
                    Hardware = new BufferClassicHardware();
                    // FileSystem = new SSWARBFileSystem();
                    Debug = true;
                    break;

                case "CENTAUR":
                    Hardware = new BufferClassicHardware();
                    FileSystem = new CENTAURFileSystem();
                    Debug = false;
                    break;

                case "PH-RHENDRIC-02":
                    Hardware = new BufferClassicHardware();
                    FileSystem = new PHRHENDRIC02FileSystem();
                    Debug = false;
                    break;

                case "PH-LAB10PC":
                    Hardware = new SympatheticHardware();
                    FileSystem = new Lab10PCFileSystem();
                    Debug = false;
                    break;

                case "PH-ST1809":
                    Hardware = new EDMHardware();
                    FileSystem = new FileSystem();
                    Debug = false;
                    break;

                case "PH-LAB10A":
                    Hardware = new EDMHardware();
                    FileSystem = new FileSystem();
                    Debug = false;
                    break;

                case "ALF":
                    Hardware = new AlFHardware();
                    FileSystem = new AlFFileSystem();
                    Debug = false;
                    break;

                case "IC-CZC136CFDJ":
                    Hardware = new WMLServerHardware();
                    FileSystem = new WMLServerFileSystem();
                    Debug = false;
                    serverTCPChannel = 1984;
                    wavemeterLockTCPChannel = 6666;
                    break;

                case "IC-CZC225B85M":
                    Hardware = new AlFHardware();
                    FileSystem = new AlFFileSystem();
                    serverTCPChannel = 1984;
                    Debug = false;
                    break;

                case "IC-CZC222C0F4":
                    Hardware = new WMLServerHuxleyHardware();
                    FileSystem = new WMLServerHuxleyFileSystem();
                    Debug = false;
                    break;

                default:
                    Hardware = new EDMHardware();
                    FileSystem = new FileSystem();
                    Debug = true;
                    //ExperimentType = "edm";
                    break;
            }
        }

    }
}
