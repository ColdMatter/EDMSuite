# MapThreadLoop - asks ScanMaster to make a series of scans with two of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def mapThreadLoop(plugin1, param1, file1Loc, pugin2, param2, file2Loc, numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	
	list1 = [line.strip() for line in open(file1Loc)]
	list2 = [line.strip() for line in open(file2Loc)]
	
	# assume both files are of equal length (they should be!!!)
	# start looping
	for i in range(len(list1)):
		print plugin1 + ":" + param1 + " -> " + list1[i]
		print plugin2 + ":" + param2 + " -> " + list2[i]
		sm.AdjustProfileParameter(plugin1, param1, list1[i], False)
		sm.AdjustProfileParameter(plugin2, param2, list2[i], False)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def run_script():
	print "Use mapThreadLoop(plugin1, param1, file1Loc, pugin2, param2, file2Loc, numScans):"
	print "\"plugin\", \"param\" must be in double quotes!"
	print ""
	print "Two apropriate txt files must be present which contain the two lists which will be mapped over"

