using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SonOfSirCachealot
{
    class FileUtils
    {
        public static string GetDirectoryForCluster(string cName) 
        {
            Dictionary<string, string> monthMap = new Dictionary<string, string> { 
                {"Jan","January"},
                {"Feb","February"},
                {"Mar","March"},
                {"Apr","April"},
                {"May","May"},
                {"Jun","June"},
                {"Jul","July"},
                {"Aug","August"},
                {"Sep","September"},
                {"Oct","October"},
                {"Nov","November"},
                {"Dec","December"}};

            string cMonth = cName.Substring(2,3);
            string cYear = "20" + cName.Substring(5,2);

            string filePath = @"\sedm\v3\" + cYear + @"\" + monthMap[cMonth] + cYear;
            return filePath;
        }

        public static string[] GetFilesForCluster(string cName)
        {
            string filePath = GetDirectoryForCluster(cName);
            return System.IO.Directory.GetFiles(filePath, cName + "*.zip");
        }

    }
}
