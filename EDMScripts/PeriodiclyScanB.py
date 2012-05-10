# asks ScanMaster to make a series of B scans

from DAQ.Environment import *
from System.Threading import *

def Run(interval, numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("Selecting profile Scan B")
	sm.SelectProfile("Scan B")
	print("running...")
	# start looping
	r = list
	for i in range(numScans):
		print str(i)+"th scan of B"
		sm.AcquireAndWait(4)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveData(scanPath)
		System.Threading.Thread.Sleep(interval*1000)	

def run_script():
	print "Use Run(interval (s), numScans)"


