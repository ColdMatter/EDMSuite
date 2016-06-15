# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def aquireWithEFlipPattern(numScans, scansPerSwitch):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	
	pattern = 
	print(pattern)

	# start looping
	r = range(numScans)
	for i in range(len(r)):
		print  "Scan Number: " + str(r[i])
		sm.AcquireAndWait(1)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)
		if pattern[i] == 0:
			print("No E Switch")
		else:
			print("Switching E")
			hc.SwitchEAndWait()
		#raw_input("unplug the rf cable")

def run_script():
	print "Use aquireWithEFlipPattern(numScans, scansPerSwitch)"

