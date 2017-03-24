using DAQ.Environment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DAQ
{
    class CCMQNFileSystem : DAQ.Environment.FileSystem
    {
        public CCMQNFileSystem()
        {
            //Paths.Add("mathPath", "c:\\Program Files\\Wolfram Research\\Mathematica\\7.0\\mathkernel.exe");
            //Paths.Add("UNCPath", "\\\\store.ic.ac.uk\\ic\\fons\\physics\\CCMQN\\data\\");
            Paths.Add("settingsPath", "c:\\Program Files\\ScanMaster\\ScanMaster\\bin\\Microcavity\\");
            Paths.Add("scanMasterDataPath", "\\\\store.ic.ac.uk\\ic\\fons\\physics\\CCMQN\\data\\");
            Paths.Add("fakeData", "\\\\store.ic.ac.uk\\ic\\fons\\physics\\CCMQN\\data\\\\examples\\");

            DataSearchPaths.Add(Paths["scanMasterDataPath"]);

            SortDataByDate = true;
        }

        // A helper function to assemble the correct dataPath, creating directories on the
        // way down if need be for CCMQN should override the method in FileSystem
        override public String GetDataDirectory(String baseDir)
        {
            if ((bool)Environs.FileSystem.SortDataByDate)
            {
                String yearshort = DateTime.Now.ToString("yy", DateTimeFormatInfo.InvariantInfo);
                String monthshort = DateTime.Now.ToString("MM", DateTimeFormatInfo.InvariantInfo);
                String day = DateTime.Now.ToString("dd", DateTimeFormatInfo.InvariantInfo);
                String directoryCCMQN = baseDir + yearshort + "-" + monthshort + "-" + day + "\\Microcavity";
                if (!Directory.Exists(directoryCCMQN)) Directory.CreateDirectory(directoryCCMQN);
                return directoryCCMQN;
            }
            else
            {
                if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
                return baseDir;
            }
        }

        // 
        override public String GenerateNextDataFileName()
        {
            if (!(bool)Environs.FileSystem.SortDataByDate) return "";
            // iterate through the data search paths and find the latest data file
            int highestIndex = -1;
            // assemble the stub for today's files
            String day = DateTime.Now.ToString("dd", DateTimeFormatInfo.InvariantInfo);
            String month = DateTime.Now.ToString("MMM", DateTimeFormatInfo.InvariantInfo);
            String year = DateTime.Now.ToString("yy", DateTimeFormatInfo.InvariantInfo);
            String fileStub = day + month + year;

            foreach (String dataPath in DataSearchPaths)
            {
                // find all of the data files for today
                String dataDir = GetDataDirectory(dataPath);
                string[] files = Directory.GetFiles(dataDir, fileStub + "*");
                // iterate over them and keep track of the highest index
                foreach (string filename in files)
                {
                    // lop off the file extension (and the block number, if present) and extract the index number
                    String firstBit = (filename.Split(new char[] { '_', '.' }))[filename.Split(new char[] { '_', '.' }).Length - 2];
                    String numString = firstBit.Substring(firstBit.Length - 2, 2);
                    int number = (int)Double.Parse(numString);
                    if (number > highestIndex) highestIndex = number;
                }
            }
            // make sure the number has two digits (there's probably a way to do this with number
            // format, but I couldn't figure it out.
            String indexString = (highestIndex + 1).ToString();
            if (indexString.Length == 1) indexString = "0" + indexString;
            return fileStub + indexString;
        }
    }
}

