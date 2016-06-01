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

            DataSearchPaths.Add(Paths["navDataPath"]);
        }
    }
}
