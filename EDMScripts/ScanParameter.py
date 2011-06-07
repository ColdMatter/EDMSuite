# Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *
from System.Collections.Generic import Dictionary

from DAQ.Environment import *
from DAQ import *
from MOTMaster import*

def run_script():
	return 0

def ScanMOTLoadTime():
	count = 0
	dic = Dictionary[String,Object]()
	dic["PatternLength"] = 2000
	mm.SetPatternPath("C:\\Experiment Control\\EDMSuite\\SympatheticMOTMasterScripts\\ExamplePattern.cs")
	while(count < 10):
		dic["MOTLoadTime"] = 5 + count
		mm.Run(dic)
		count = count + 1

