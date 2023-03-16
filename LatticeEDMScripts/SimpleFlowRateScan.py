# This is the first script I wrote. All it does is record TOF profiles
# for different Helium flow rates.
# Import a whole load of stuff not all of these are used
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
	dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["scanMasterDataPath"]) # Gets the directory in which the data is stored. This needs to be changed to what it is for us
    	return 0

def SelectProfile(profileName):
	sm.SelectProfile(profileName) # This loads in the profile you want, need to make a profile in ScanMaster first that decides the plugin you use etc
    	return 0

def Acquire():
	currentSetpoint = hc.GetFlowHelium() # This needs to be the function in the hardware controller that does this/ also, is it already a string?
	print(currentSetpoint)
    	count=0
    	flowrates = [1.0,2.0,3.0,4.0,5.0,6.0,7.0,8.0]
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping over the flowrates
	for i in range(len(flowrates)):
        	print(flowrates[i])
		hc.SetHeliumFlow(flowrates[i]) # Check whether this takes in a number or string as an argument...
		time.sleep(4)
                count=count+1
		sm.AcquireAndWait(1) # What does acquire and wait do? What am I actually recording here? I assume this really runs the acquire function in ScanMaster
		scanPath = file + "_" + "HeFlowScan2" + "_" + str(flowrates[i]) + ".zip"
		sm.SaveAverageData(scanPath)
	return 0

def run_script():
	SMGo()
	Acquire()