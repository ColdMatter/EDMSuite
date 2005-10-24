using System;
using System.Collections;
using System.Globalization;
using System.IO;


namespace DAQ.Environment
{
	/// <summary>
	/// 
	/// </summary>
	public class FileSystem
	{
		public Hashtable Paths = new Hashtable();
		public bool SortDataByDate = false;

		// A helper function to assemble the correct dataPath, creating directories on the
		// way down if need be
		public String GetDataDirectory(String baseDir)
		{
			if ((bool)Environs.FileSystem.SortDataByDate) 
			{
				String year = DateTime.Now.ToString("yyyy",DateTimeFormatInfo.InvariantInfo);
				String month = DateTime.Now.ToString("MMMM",DateTimeFormatInfo.InvariantInfo);
				String directory = baseDir + year + "\\" + month + year + "\\";
				if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
				return directory;
			}
			else
			{
				if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
				return baseDir;
			}
		}

	}
}
