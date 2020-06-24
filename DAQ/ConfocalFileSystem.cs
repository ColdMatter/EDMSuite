using DAQ.Environment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DAQ
{
    /// <summary>
    /// Confocal Microscope specific file system. Inherits from the base class FileSystem. 
    /// Copied largely from CCMQNFileSystem. 
    /// </summary>
    class ConfocalFileSystem : DAQ.Environment.FileSystem
    {
        private String _experimentName;

        public ConfocalFileSystem(String experimentName)
        {
            _experimentName = experimentName;
            Paths.Add("settingsPath", "C:\\Users\\ccmqn\\Documents\\ConfocalMicroscope\\bin\\");
            // Paths.Add("scanMasterDataPath", "\\\\store.ic.ac.uk\\ic\\fons\\physics\\CCMQN\\data\\");
            // I have changed this because the network is 'full' even though it isn't (see triplet onenote 13/06/20)
            Paths.Add("scanMasterDataPath", "C:\\Users\\ccmqn\\Desktop\\TripletScanTemp\\");

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
                String directoryCCMQN = baseDir + yearshort + "-" + monthshort + "-" + day + "\\" + _experimentName + "\\";
                if (!Directory.Exists(directoryCCMQN)) Directory.CreateDirectory(directoryCCMQN);
                return directoryCCMQN;
            }
            else
            {
                if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
                return baseDir;
            }
        }
    }
}
