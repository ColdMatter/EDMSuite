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
	currentSetpoint = tclProbe.GetLaserSetpoint("ProbeCavity", "TopticaSHGPZT")
	count=0
	print("Current setpoint is: " + str(currentSetpoint))
	print("")
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping
	x = [0.002*i for i in range(-10,10)]
	for j in range(5):
		for i in range(len(x)):
			tclProbe.SetLaserSetpoint("ProbeCavity", "TopticaSHGPZT",currentSetpoint+x[i])
			for k in range(0,2):
				print("Current detuning: " + str(x[i]) + ", MW state: " + str(hc.MwSwitchState))
				count=count+1
				sm.AcquireAndWait(2)
				scanPath = file + "_" + str(count) + "_" + str(round(currentSetpoint+x[i],3)) + ".zip"
				sm.SaveAverageData(scanPath)
				hc.SwitchMwAndWait()
	tclProbe.SetLaserSetpoint("ProbeCavity", "TopticaSHGPZT",currentSetpoint)
		
	

		
	


def run_script():
	SMGo()
	SelectProfile("Scan B")
	Acquire()