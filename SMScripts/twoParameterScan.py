# Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *

from DAQ.Environment import *
import time

def SMGo():
	fileSystem = Environs.FileSystem
	dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])

def SelectProfile(profileName):
	sm.SelectProfile(profileName)

def StartPattern():
	sm.OutputPattern()

def StopPattern():
	sm.StopPatternOutput()

def run_script():
	SMGo()
	SelectProfile("Scan B")
	StartPattern()
	time.sleep(5)
	StopPattern()
	hc.UpdateProbeAOMV(8.5)
	StartPattern()
	time.sleep(5)
	StopPattern()