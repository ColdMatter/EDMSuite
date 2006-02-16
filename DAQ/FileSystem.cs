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
		public ArrayList DataSearchPaths = new ArrayList();

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

		// 
		public String GenerateNextDataFileName()
		{
			if (!(bool)Environs.FileSystem.SortDataByDate) return "";
			// iterate through the data search paths and find the latest data file
			int highestIndex = -1;
			// assemble the stub for today's files
			String day = DateTime.Now.ToString("dd",DateTimeFormatInfo.InvariantInfo);
			String month = DateTime.Now.ToString("MMM",DateTimeFormatInfo.InvariantInfo);
			String year = DateTime.Now.ToString("yy",DateTimeFormatInfo.InvariantInfo);
			String fileStub = day + month + year;

			foreach(String dataPath in DataSearchPaths)
			{
				// find all of the data files for today
				String dataDir = GetDataDirectory(dataPath);
				string[] files = Directory.GetFiles(dataDir, fileStub + "*");
				// iterate over them and keep track of the highest index
				foreach(string filename in files)
				{
					// lop off the file extension (and the block number, if present) and extract the index number
					String firstBit = (filename.Split(new char[] {'_','.'}))[0];
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
