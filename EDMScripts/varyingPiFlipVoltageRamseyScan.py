# Asks ScanMaster to make a series of B scans while changing the scrambler voltage

from DAQ.Environment import *
from System import *

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def acquire(numScans):
	# setup
	piV_input = prompt("Enter pi voltages: ")
	r = piV_input.split(",")

	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")

	# start looping
	for i in range(len(r)):
		hc.SetPiFlipVoltage(float(r[i]))
		hc.SetPiFlipVoltage()
		print("pi flip voltage set to " + str(r[i]))
		sm.AdjustProfileParameter("out", "externalParameters", str(r[i]), False)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveData(scanPath)

def run_script():
	print "Use acquire(numScans)"

