using System;

namespace DAQ.Environment
{
    public class NavigatorFileSystem : DAQ.Environment.FileSystem
    {
     public NavigatorFileSystem()
        {
            Paths.Add("settingspath", "%USERPROFILE%\\Settings");
            Paths.Add("navDataPath", "%USERPROFILE%\\Data");
            Paths.Add("mathPath", "C:\\Program Files\\Wolfram Research\\Mathematica\\10.4\\mathkernel.exe");
            Paths.Add("navServerPath", "\\155.198.206.40\\Navigator_Data\\Data");
            Paths.Add("scriptListPath", "C:\\Users\\Navigator\\Software\\Scripts");
            Paths.Add("MOTMasterEXEPath", "C:\\Users\\Navigator\\Software\\EDMSuite\\MOTMaster\\bin\\Nav");
            Paths.Add("cameraAttributesPath", "C:\\Users\\Public\\Documents\\National Instruments\\NI-IMAQdx\\Data\\Pike.icd");
            Paths.Add("daqDLLPath", "C:\\Users\\Navigator\\Software\\EDMSuite\\MOTMaster\\bin\\Nav\\DAQ.dll");

            DataSearchPaths.Add(Paths["navDataPath"]);
            DataSearchPaths.Add(Paths["navServerPath"]);
        }
    }
}
