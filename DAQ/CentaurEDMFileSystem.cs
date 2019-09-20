using System;
using System.Collections.Generic;
using System.Text;

namespace DAQ
{
    class CentaurEDMFileSystem : DAQ.Environment.FileSystem
	{
        public CentaurEDMFileSystem()
		{
			Paths.Add("mathPath", "C:\\Program Files\\Wolfram Research\\Mathematica\\11.3\\MathKernel.exe");
			
			SortDataByDate = true;
		}
	}
}
