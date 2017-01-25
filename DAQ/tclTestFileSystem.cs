using System;

namespace DAQ.Environment
{
	class tclTestFileSystem : DAQ.Environment.FileSystem
	{
		public tclTestFileSystem()
		{
			Paths.Add("fakeData", "c:\\Users\\nfitch\\Desktop\\fakeData\\");
		}

	}
}
