 #Get ScanMaster to take B scans while I scan microwave power in both detectors

from DAQ.Environment import *

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def scanMicrowaves(numScans):
	mwpowers_input = prompt("Enter range of microwave mixer voltages in V: ")
	mwpowers = mwpowers_input.split(",")

	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")

	for i in range(len(mwpowers)):
		print  "HC: bottomProbeMwPower -> " + mwpowers[i]
		print  "HC: topProbeMwPower -> " + mwpowers[i]
		hc.UpdateBottomProbeMicrowaveMixerV(float(mwpowers[i]))
		hc.UpdateTopProbeMicrowaveMixerV(float(mwpowers[i]))
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def run_script():
	print "scanMicrowaves(numScans)"

