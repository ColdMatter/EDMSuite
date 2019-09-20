using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class Lab10PCFileSystem : DAQ.Environment.FileSystem
    {
        public Lab10PCFileSystem()
        {
            //Paths.Add("mathPath", "c:\\Program Files\\Wolfram Research\\Mathematica\\7.0\\mathkernel.exe");
            Paths.Add("settingsPath", "d:\\Data\\Settings\\");
            Paths.Add("scanMasterDataPath", "d:\\Data\\CH");
            Paths.Add("fakeData", "d:\\Data\\Examples\\");

            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = false;
        }
    }
}

