using System;
using System.IO;

namespace DAQ.Environment
{
    public class NavigatorFileSystem : DAQ.Environment.FileSystem
    {
     public NavigatorFileSystem()
        {
            Paths.Add("settingsPath", System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Settings"));
            Paths.Add("navDataPath", "%USERPROFILE%\\Data");
            Paths.Add("mathPath", "C:\\Program Files\\Wolfram Research\\Mathematica\\10.4\\mathkernel.exe");
            Paths.Add("navServerPath", "\\155.198.206.40\\Navigator_Data\\Data");
            Paths.Add("scriptListPath", "C:\\Users\\Navigator\\Software\\EDMSuite\\NavigatorMaster");
            Paths.Add("MOTMasterEXEPath", "C:\\Users\\Navigator\\Software\\EDMSuite\\MOTMaster\\bin\\Nav");
         
            Paths.Add("cameraAttributesPath", "C:\\Users\\Public\\Documents\\National Instruments\\NI-IMAQdx\\Data\\cam0.icd");
            Paths.Add("CameraAttributesPath", "C:\\Users\\Public\\Documents\\National Instruments\\NI-IMAQdx\\Data\\cam0_remote.icd");
            Paths.Add("daqDLLPath", "C:\\Users\\Navigator\\Software\\EDMSuite\\MOTMaster\\bin\\Nav\\DAQ.dll");
          
                string sYear = DateTime.Today.Year.ToString();
                string sMonth = DateTime.Today.Month.ToString().PadLeft(2, '0');
                string sDay = DateTime.Today.Day.ToString().PadLeft(2,'0');
                Paths.Add("DataPath", "Z:\\Data\\" + sYear + sMonth + sDay);
           

            if (!Directory.Exists((string)Paths["DataPath"]))
            {
                try
                { Directory.CreateDirectory((string)Paths["DataPath"]); }
                catch
                {
                    Console.WriteLine("Could not connect to the server. Check it is connected.");
                }
            }
            Paths.Add("MOTMasterDataPath", (string)Paths["DataPath"]+"\\");
            Paths.Add("MuquansExePath", "Z:\\Software\\ukus_dds_comm_gw");
            Paths.Add("scriptSnippetPath", (string)Paths["scriptListPath"] + "\\bin\\Nav\\NavigatorMaster.dll");
            Paths.Add("HardwareClassPath", "C:\\Users\\Navigator\\Software\\EDMSuite\\DAQ\\NavigatorHardware.cs");
            DataSearchPaths.Add(Paths["navDataPath"]);
            DataSearchPaths.Add(Paths["navServerPath"]);
        }
    }
}
