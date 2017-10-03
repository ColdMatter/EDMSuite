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
        /// 
        static Environs()
        {
            String computerName = (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"];
            EnvironsHelper eHelper = new EnvironsHelper(computerName);
            Hardware = eHelper.Hardware;
            FileSystem = eHelper.FileSystem;
            Debug = eHelper.Debug;
        }
	}
}
