using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class RequiemFileSystem : DAQ.Environment.FileSystem
    {
        public RequiemFileSystem()
        {
            Paths.Add("mathPath", "c:\\Program Files\\Wolfram Research\\Mathematica\\7.0\\mathkernel.exe");
            Paths.Add("settingsPath", "c:\\Data\\Settings\\");
            Paths.Add("scanMasterDataPath", "c:\\Data\\ScanMasterData\\");
            Paths.Add("dataPath", "c:\\Data\\");
            Paths.Add("fakeData", "c:\\Data\\Examples\\");

            Paths.Add("MOTMasterDataPath", "c:\\Data\\MOTMasterData\\");
            Paths.Add("scriptListPath", "C:\\Experiment Control\\EDMSuite\\SympatheticMOTMasterScripts\\");
            Paths.Add("daqDLLPath", "C:\\Experiment Control\\EDMSuite\\DAQ\\bin\\Sympathetic");
            Paths.Add("MOTMasterExePath",
                "C:\\Experiment Control\\EDMSuite\\MOTMaster\\bin\\Sympathetic");

            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = false;
        }
    }
}
