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
	mm.SetScriptPath("C:\\Experiment Control\\EDMSuite\\SympatheticMOTMasterScripts\\MOTPattern.cs")
	while(count < 14):
		dic["MOTLoadTime"] = 1000 + 500 * count
		dic["PatternLength"] = 1001 + 500 * count
		mm.Run(dic)
		count = count + 1

