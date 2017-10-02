# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def aquireWithEFlip(numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")

	# start looping
	r = range(numScans)
	for i in range(len(r)):
		print  "Scan Number: " + str(r[i])
		sm.AcquireAndWait(1)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)
		hc.SwitchEAndWait()
		#raw_input("unplug the rf cable")

def run_script():
	print "Use aquireWithEFlip(numScans)"

