# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def aquireWithEFlip(numScans, start, stop, step):
	# setup
	fileSystem = Environs.FileSystem
	# loop over voltages
	l = range(int(10*start), int(10*stop), int(10*step))
	for j in range(len(l)):
		print  "E fields at +/- " + str(float(l[j])/10) + " KV"
		hc.SetCPlusVoltage(float(l[j])/10)
		hc.SetCMinusVoltage(float(l[j])/10)
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
			sm.AcquireAndWait(5)
			scanPath = file + "_" + str(i) + ".zip"
			sm.SaveAverageData(scanPath)
			print  "Switching E fields"
			hc.SwitchEAndWait()

def run_script():
	print "Use aquireWithEFlip(numScans, start, stop, step). voltages in kV, no inc less than 0.l"

