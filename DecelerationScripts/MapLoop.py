# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def mapLoop(plugin, param, start, end, step, numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping
	r = range(start, end, step)
	for i in range(len(r)):
		print plugin + ":" + param + " -> " + str(r[i])
		sm.AdjustProfileParameter(plugin, param, str(r[i]), False)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def run_script():
	print "Use mapLoop(plugin, param, start, end, step, numScans)"


