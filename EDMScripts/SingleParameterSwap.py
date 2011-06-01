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
	SwapMOTLoadTime()

def SwapMOTLoadTime():
	dic = Dictionary[String,Object]()
	dic["MOTLoadTime"] = 6
	dic["PatternLength"] = 2000
	mm.Run(dic)



