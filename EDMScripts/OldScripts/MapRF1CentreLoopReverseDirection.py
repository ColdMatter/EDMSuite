# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *

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
	modes = ["up","down"]
	amps =["10.2873", "7.45664", "4.28028", "1.51244", "-0.672201", "-2.33717", "-3.81608", "-4.82957", "-5.73749", "-6.36283", "-6.88332", "-7.30834", "-7.67567", "-7.81202", "-7.89538", "-7.80834", "-7.63106", "-7.40025", "-7.00398", "-6.50694", "-5.82152", "-4.96511", "-3.89361", "-2.47886", "-0.668273", "1.72046", "4.54244", "8.75399", "10.2873", "7.45664", "4.28028", "1.51244", "-0.672201", "-2.33717", "-3.81608", "-4.82957", "-5.73749", "-6.36283", "-6.88332", "-7.30834", "-7.67567", "-7.81202", "-7.89538", "-7.80834", "-7.63106", "-7.40025", "-7.00398", "-6.50694", "-5.82152", "-4.96511", "-3.89361", "-2.47886", "-0.668273", "1.72046", "4.54244", "8.75399"]
	amps =["0.527805", "-2.16108", "-5.21073", "-7.11279", "-8.5493", "-9.75751", "-10.7639", "-11.5174", "-12.0689", "-12.4872", "-12.7709", "-12.9314", "-13.095", "-13.1304", "-13.0236", "-12.7866", "-12.3696", "-12.0129", "-11.3589", "-10.535", "-9.66376", "-8.55365", "-6.93337", "-4.66466", "-2.26597", "1.14551", "4.58811", "0.527805", "-2.16108", "-5.21073", "-7.11279", "-8.5493", "-9.75751", "-10.7639", "-11.5174", "-12.0689", "-12.4872", "-12.7709", "-12.9314", "-13.095", "-13.1304", "-13.0236", "-12.7866", "-12.3696", "-12.0129", "-11.3589", "-10.535", "-9.66376", "-8.55365", "-6.93337", "-4.66466", "-2.26597", "1.14551", "4.58811"]
	for i in range(len(r)):
		scanMode = i%2
		mode = modes[scanMode]
		print  "pg:rf1CentreTime -> " + str(r[i])
		print  "pg:rf1BlankingCentreTime -> " + str(r[i])
		print  "out:scanOnAmplitude -> " + str(amps[i])
		sm.AdjustProfileParameter("out", "scanMode", mode, False)
		sm.AdjustProfileParameter("out", "scanOnAmplitude", amps[i], False)
		sm.AdjustProfileParameter("pg", "rf1CentreTime", str(r[i]), False)
		sm.AdjustProfileParameter("pg", "rf1BlankingCentreTime", str(r[i]), False)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + ".zip"
		sm.SaveAverageData(scanPath)

def run_script():
	print "Use mapLoop(start, end, step, numScans)"

