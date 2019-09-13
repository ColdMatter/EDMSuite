# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def mapLoop(numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")

	list1 = [line.strip() for line in open("rfCentreTimes.txt")]
	list2 = [line.strip() for line in open("topAmpPwrs.txt")]

	# start looping
	for i in range(len(list1)):
		print  "pg:rf2CentreTime -> " + str(list1[i])
		print  "pg:rf2BlankingCentreTime -> " + str(list1[i])
		print  "out:scanOnAmplitude -> " + str(list2[i])
		sm.AdjustProfileParameter("pg", "rf2CentreTime", list1[i], False)
		sm.AdjustProfileParameter("pg", "rf2BlankingCentreTime", list1[i], False)
		sm.AdjustProfileParameter("out", "scanOnAmplitude", list2[i], False)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveData(scanPath)

def run_script():
	print "Use mapLoop(numScans)"

