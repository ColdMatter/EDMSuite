# MapRFxF - asks ScanMaster to make a series of scans with rf2f scanned

from DAQ.Environment import *

def mapRF1F(list, numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping
	r = list
	for i in range(len(r)):
		print "RF1F -> " + str(r[i])
		hc.SetRF1FMCentre( r[i] )
		hc.UpdateRFPowerMonitor()
		hc.UpdateRFFrequencyMonitor()
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def mapRF2F(list, numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping
	r = list
	for i in range(len(r)):
		print "RF2F -> " + str(r[i])
		hc.SetRF2FMCentre( r[i] )
		hc.UpdateRFPowerMonitor()
		hc.UpdateRFFrequencyMonitor()
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def mapRF1A(list, numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping
	r = list
	for i in range(len(r)):
		print "RF1A -> " + str(r[i])
		hc.SetRF1AttCentre( r[i] )
		hc.UpdateRFPowerMonitor()
		hc.UpdateRFFrequencyMonitor()
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def mapRF2A(list, numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")
	# start looping
	r = list
	for i in range(len(r)):
		print "RF2A -> " + str(r[i])
		hc.SetRF2AttCentre( r[i] )
		hc.UpdateRFPowerMonitor()
		hc.UpdateRFFrequencyMonitor()
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def run_script():
	print "Use mapRFxx(list, numScans)"

