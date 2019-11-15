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
	x = [172.6+0.02*i for i in range(31)]
	for j in range(3):
		for i in range(len(x)):
			hc.SetGreenSynthFrequency(round(x[i],2))
			sm.AdjustProfileParameter("out", "externalParameters", str(round(x[i],2)), False)
			for k in range(4):
				#print("MW state: " + str(hc.MwSwitchState) + ", Pi flip: " + str(hc.phaseFlip1State))
				print("Synth Freq: " + str(x[i]) + "V, MW state: " + str(hc.MwSwitchState))
				count=count+1
				sm.AcquireAndWait(2)
				scanPath = file + "_" + str(count) + ".zip"
				sm.SaveAverageData(scanPath)
				hc.SwitchMwAndWait()
				if (k<2):
					hc.SetPhaseFlip1(True)
				else:
					hc.SetPhaseFlip1(False)
sm.AdjustProfileParameter("out", "externalParameters", "SidIsGreat", False)
		
	

		
	


def run_script():
	SMGo()
	SelectProfile("Scan B")
	Acquire()