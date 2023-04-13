# MapLoop - asks ScanMaster to make a series of B scans with the interferometer times incremented along the machine
# The pi-pulse amplitude and RF frequencies can be read in from the file to automate running through the machine.
# The required format is rf1Time, interferometer length (us), synth amplitude, synth frequency

from DAQ.Environment import *
import csv

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def mapLoop(numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	sm.SelectProfile("Scan B")
	RFdata = importRFdata('\RFdata_05Mar2020')
	print RFdata
	hc.PrepareForBScan()
	sm.AdjustProfileParameter("pg", "attLength", str(100), False)
	sm.AdjustProfileParameter("pg", "fmLength", str(100), False)
	# start looping
	for i in range(len(RFdata)):
		print  "pg:rf1CentreTime -> " + str(RFdata[i][0])
		print  "pg:rf1BlankingCentreTime -> " + str(RFdata[i][0])
		print  "pg:rf2CentreTime -> " + str(int(RFdata[i][0]) + int(RFdata[i][1]))
		print	"synthAmplitude -> " + str(RFdata[i][2])
		print	"synthFrequency -> " + str(RFdata[i][3])
		sm.AdjustProfileParameter("pg", "rf1CentreTime", str(RFdata[i][0]), False)
		sm.AdjustProfileParameter("pg", "attCentreTime", str(RFdata[i][0]), False)
		sm.AdjustProfileParameter("pg", "fmCentreTime", str(RFdata[i][0]), False)
		sm.AdjustProfileParameter("pg", "rf1BlankingCentreTime", str(RFdata[i][0]), False)
		sm.AdjustProfileParameter("pg", "rf2CentreTime", str(int(RFdata[i][0]) +int(RFdata[i][1])), False)
		sm.AdjustProfileParameter("pg", "rf2BlankingCentreTime", str(int(RFdata[i][0]) + int(RFdata[i][1])), False)
		hc.SetGreenSynthAmp(float(RFdata[i][2]))
		hc.SetGreenSynthFrequency(float(RFdata[i][3]))
		hc.EnableGreenSynth(True)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)
		#hc.StepTarget()


def importRFdata(filename):
	data = []
	with open('D:\Box Sync\EDM\Data\Settings\ScanMaster' + filename +'.csv', 'rb') as csvfile:
		spamreader = csv.reader(csvfile, delimiter=',')
		for row in spamreader:
			data.append(row)
		return data
	

def run_script():
	print "Use mapLoop(numScans)"

