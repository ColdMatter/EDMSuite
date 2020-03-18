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

def Acquire():
	count=0
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping
	x = [0.55, 0.62, 0.85]
	for j in range(3):
		for i in range(len(x)):
			hc.UpdateProbeAOMamp(x[i])
			sm.AdjustProfileParameter("out", "externalParameters", str(x[i]), False)
			for k in range(4):
				#print("MW state: " + str(hc.MwSwitchState) + ", Pi flip: " + str(hc.phaseFlip1State))
				print("AOM: " + str(x[i]) + "V, MW state: " + str(hc.MwSwitchState))
				count=count+1
				sm.AcquireAndWait(2)
				scanPath = file + "_" + str(count) + ".zip"
				sm.SaveAverageData(scanPath)
				hc.SwitchMwAndWait()
				if (k<2):
					hc.SetPhaseFlip2(True)
				else:
					hc.SetPhaseFlip2(False)
sm.AdjustProfileParameter("out", "externalParameters", "SidIsGreat", False)
		
	

		
	


def run_script():
	SMGo()
	SelectProfile("Scan B")
	Acquire()