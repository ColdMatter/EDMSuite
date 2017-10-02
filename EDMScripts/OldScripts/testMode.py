# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def mapLoop(start, end, step, numScans):
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
	modes = ["up","down"]
	for i in range(len(r)):
		scanMode = i%2
		print  "Scan Mode -> " + str(modes[scanMode])

def run_script():
	print "Use mapLoop(start, end, step, numScans)"

