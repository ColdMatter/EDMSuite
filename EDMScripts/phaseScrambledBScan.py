# Asks ScanMaster to make a series of B scans while changing the scrambler voltage

from DAQ.Environment import *
from System import *

def acquire(numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	r = Random()
	# start looping
	for i in range(numScans):
		# randomise Ramsey phase
		scramblerV = 0.7248 * r.NextDouble()
		hc.SetScramblerVoltage(scramblerV)
		print("scrambler voltage set to " + str(scramblerV))
		sm.AdjustProfileParameter("out", "externalParameters", str(scramblerV), False)
		sm.AcquireAndWait(1)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def run_script():
	print "Use acquire(numScans)"

