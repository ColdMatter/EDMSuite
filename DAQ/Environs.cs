using System;
using System.Collections;

using DAQ.HAL;

namespace DAQ.Environment
{
	/// <summary>
	/// The Environs class insulates the control programs from the computer they are running
	/// on. The Environs can be used to access hardware, access files, use Mathematica and
	/// control debugging in a computer independent way.
	/// </summary>
	public class Environs
	{
		/// <summary>
		/// The Hardware that is present in this Environs, presented through a
		/// standardised interface: Hardware.
		/// </summary>
		public static Hardware Hardware;

		/// <summary>
		/// This is where details of the file system specific to a computer belong.
		/// </summary>
		public static FileSystem FileSystem;

        /// <summary>
        /// This is where calibrations for specific experiments go.
        /// </summary>
        //public static HardwareCalibrationLibrary HardwareCalibrationLibrary;

		/// <summary>
		/// This hashtable stores information about the system (as Strings). You can put
		/// pretty much anything in here that's specific to a particular computer.
		/// </summary>
		public static Hashtable Info = new Hashtable();

		/// <summary>
		/// The global debug flag. If this flag is set to true, no hardware calls will be made
		/// (if you're writing a hardware class it's your responsibility to observe this flag)
		/// and data will be synthesized.
		/// </summary>
		public static bool Debug;

       	/// <summary>
		/// Experiment type is for code that needs to know what experiment it's running on.
		/// </summary>
		//public static String ExperimentType;

		/// <summary>
		/// Initialise the environment. This code switches on computer name and
		/// sets up the environment accordingly.
		/// </summary>
		static Environs()
		{
			String computerName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
			switch (computerName)
			{
                case "PH-DK902":
                    Hardware = new EDMHardware();
                    FileSystem = new PhkaraFileSystem();
                    Debug = false;
                    //ExperimentType = "edm";
                    break;

                case "CRASH1":
					Hardware = new DecelerationHardware();
					FileSystem = new CrashFileSystem();
					Debug = false;
                    //ExperimentType = "decelerator";
					break;

				case "SCHNAPS":
					Hardware = new DecelerationHardware();
					FileSystem = new SchnapsFileSystem();
					//ExperimentType = "decelerator";
                    Info.Add("SwitchSequenceCode", "SwitchSequenceV1`");
					Debug = false;
                    break;

                case "SUNSHINE":
                    Hardware = new DecelerationHardware();
                    FileSystem = new SunshineFileSystem();
                    //ExperimentType = "decelerator";
                    Info.Add("SwitchSequenceCode", "SwitchSequenceV1`");
                    Debug = false;
                    break;

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

                case "PIXIE":
                    Hardware = new PXIEDMHardware();
                    FileSystem = new PixieFileSystem();
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
                //    Hardware = new DecelerationHardware();
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

                case "PH-REQUIEM":
                    Hardware = new PXISympatheticHardware();
                    FileSystem = new RequiemFileSystem();
                    Debug = false;
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

                case "NAVIGATOR-ANAL":
                    Hardware = new NavigatorHardware();
                    FileSystem = new NavigatorFileSystem();
                    Debug = false;
                    break;

                case "PH-LAB-015":
                    Hardware = new NavigatorHardware();
                    FileSystem = new NavigatorFileSystem();
                    Debug = true;
                    break;
                case "JIMMY-SURFACE":
                    Hardware = new NavigatorHardware();
                    FileSystem = new NavigatorFileSystem();
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
