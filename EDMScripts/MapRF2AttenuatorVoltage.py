﻿# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

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
	for i in range(len(r)):
		print  "pg:rf2CentreTime -> " + str(r[i])
		print  "pg:rf2BlankingCentreTime -> " + str(r[i])
		sm.AdjustProfileParameter("pg", "rf2CentreTime", str(r[i]), False)
		sm.AdjustProfileParameter("pg", "rf2BlankingCentreTime", str(r[i]), False)

		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def run_script():
	print "Use mapLoop(start, end, step, numScans)"

