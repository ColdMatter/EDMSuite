# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def mapLoop(start, end, step, numScans, indivFreqs):
	r = range(start, end, step)

	if indivFreqs:
		freqs1_input = prompt("Enter frequencies in MHz: ")
		freqs1 = freqs1_input.split(",")
	else :
		freqs1_input = prompt("Enter frequency for every scan in MHz: ")
		freqs1 = [freqs1_input] * len(r)

	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping
	
	for i in range(len(r)):
		print  "pg:rf1CentreTime -> " + str(r[i])
		print  "pg:rf1BlankingCentreTime -> " + str(r[i])
		sm.AdjustProfileParameter("pg", "rf1CentreTime", str(r[i]), False)
		sm.AdjustProfileParameter("pg", "rf1BlankingCentreTime", str(r[i]), False)
		sm.AdjustProfileParameter("out", "externalParameters", str(r[i]), False)
		print "out:onFrequency -> " + str(freqs1[i])
		sm.AdjustProfileParameter("out", "onFrequency", str(freqs1[i]), False)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

	
def run_script():
	print "Use mapLoop(start, end, step, numScans, indivFreqs?)"

