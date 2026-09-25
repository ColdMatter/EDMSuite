using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class RbTweezerFileSystem : DAQ.Environment.FileSystem
    {
        public RbTweezerFileSystem()
        {
			//Paths.Add("mathPath", "c:/program files/wolfram research/mathematica/10.4/mathkernel.exe");
			Paths.Add("RbTweezerData", "E: \\Data");
			Paths.Add("fakeData", "C: \\ControlPrograms\\Examples");

			Paths.Add("MOTMasterDataPath", "C:\\Users\\CaFMOT\\OneDrive - Imperial College London\\caftweezers\\mot_master_data\\");
			Paths.Add("daqDLLPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\bin\\x86\\RbTweezer\\daq.dll");
			Paths.Add("MOTMasterExePath", "C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\RbTweezer\\");
			Paths.Add("ExternalFilesPath", "C:\\Users\\cafmot\\Documents\\Temp_camera_images");
			Paths.Add("HardwareClassPath", "C:\\ControlPrograms\\EDMSuite\\DAQ\\RbTweezerHardware.cs");
			Paths.Add("scriptListPath", "C:\\ControlPrograms\\EDMSuite\\RbTweezerMasterScripts\\");
			Paths.Add("ToFFilesPath", "C:\\Users\\cafmot\\Documents\\ToF_Data\\");
			Paths.Add("wavemeterLockData", "C:\\Users\\CaFMOT\\Documents\\WML_DataLog\\");

			DataSearchPaths.Add(Paths["RbTweezerData"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = true;
        }
    }
}
