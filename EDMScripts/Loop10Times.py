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
	count = 0
	while(count < 10):
		mm.CompileAndRun()
		count = count + 1



