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

def SwitchCoils(maxCurrent,minCurrent):
	count = 0
	endcount = 2
	dic = Dictionary[String,Object]()
	mm.SetScriptPath("C:\\Experiment Control\\EDMSuiteTrunk\\SympatheticMOTMasterScripts\\MagTrapAbsImage.cs")
	while(count < endcount):
		if count == 0:
			dic["MOTCoilsCurrent"] = maxCurrent
			mm.Run(dic)
			count = count + 1
		elif count == 1:
			dic["MOTCoilsCurrent"] = minCurrent
			mm.Run(dic)
			count = count + 1
	
	return 0

def RepeatScansSWC(maxCurrent,minCurrent,numberofrepeats):
	j = 0
	while(j < numberofrepeats):
		SwitchCoils(maxCurrent,minCurrent)
		j = j+1
	return 0