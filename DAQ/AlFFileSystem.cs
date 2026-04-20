using System;
using System.Collections.Generic;

namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	class AlFFileSystem : DAQ.Environment.FileSystem
	{
		public AlFFileSystem()
		{
			Paths.Add("MOTMasterDataPath", "C:\\Data\\mot_master_data\\");
			Paths.Add("scriptListPath", "C:\\EDMSuite\\AlFMOTMasterScripts");
			Paths.Add("daqDLLPath", "C:\\EDMSuite\\DAQ\\bin\\AlF\\daq.dll");
			Paths.Add("MOTMasterExePath", "C:\\EDMSuite\\bin\\AlF\\");
			Paths.Add("HardwareClassPath", "C:\\EDMSuite\\DAQ\\AlFHardware.cs");
			Paths.Add("fakeData", "C:\\Data\\Examples\\");
			Paths.Add("scanMasterDataPath", "C:\\Data\\");
			Paths.Add("ToFFilesPath", "C:\\Users\\alfultra\\Documents\\ToF_Data\\");
			Paths.Add("ExternalFilesPath", "C:\\Users\\alfultra\\Documents\\Camera_images");
			Paths.Add("MMStuffTemp", "C:\\Users\\alfultra\\Documents\\MMStuff_temp");
			Paths.Add("LineData", @"C:\Users\alfultra\OneDrive - Imperial College London\Desktop\LineData.xml");
			List<string> MMAssemblies = new List<string> { };
			//foreach (string dll in System.IO.Directory.GetFiles(@"C:\EDMSuite\WavemeterLock\bin\AlF\", "*.dll"))
   //         {
			//	if (dll.Contains("DAQ.dll")) continue;
			//	MMAssemblies.Add(dll);
   //         }
			MMAssemblies.Add(@"C:\EDMSuite\WavemeterLock\bin\AlF\WavemeterLock.exe");
			MMAssemblies.Add(@"C:\EDMSuite\WavemeterLock\bin\AlF\Wavemeter Lock Server.exe");
			Paths.Add("AdditionalMOTMasterAssemblies", MMAssemblies);

			DataSearchPaths.Add(Paths["scanMasterDataPath"]);
			DataSearchPaths.Add(Paths["fakeData"]);

			SortDataByDate = false;
		}
	}
}
