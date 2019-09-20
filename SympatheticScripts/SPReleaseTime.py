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

def ScanReleaseTime(initial,final,interval):
	count = 0
	endcount = (final-initial)/interval
	dic = Dictionary[String,Object]()
	mm.SetScriptPath("C:\\Experiment Control\\EDMSuiteTrunk\\SympatheticMOTMasterScripts\\ReleaseRecapture.cs")
	while(count < endcount+1):
		dic["ReleaseTime"] = (count*interval)+initial
		dic["Frame1Trigger"] = 100001 + (count*interval)+initial
		mm.Run(dic)
		count = count + 1
	return 0

def RepeatScansRT(initial, final, interval, numberofrepeats):
	j = 0
	while(j < numberofrepeats):
		ScanReleaseTime(initial,final,interval)
		j = j+1
	return 0
