# This loop monitors the rf Discharges for a particular amplitude, then repeats for other amplitudes
# n

from DAQ.Environment import *

def scanRF(LowestAmp, HighestAmp, step, numScans):
	# setup
	AmpList = []
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_" + "MeasuredRF1Amp" + "*.zip")
	print("")
	# start looping
	r = range(int(10*LowestAmp), int(10*HighestAmp), int(10*step))
	for i in range(len(r)):
		print  "hc:rf1 Amplitude -> " + str(float(r[i])/10)
		hc.SetGreenSynthAmp(float(r[i])/10) 
		# hc.GreenSynthOnAmplitude = double(r[i]/10)
		hc.EnableGreenSynth( False )
		hc.EnableGreenSynth( True )
		hc.UpdateRFPowerMonitor()
		rfAmpMeasured = hc.RF1PowerCentre
		hc.StepTarget(2)
		System.Threading.Thread.Sleep(500)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(i) + "_" + str(rfAmpMeasured) + ".zip"
		sm.SaveData(scanPath)
		AmpList.append(str(rfAmpMeasured))
	print	 "List of Measured Amplitudes =" + str(AmpList).strip('[]')

def run_script():
	print "Use scanRF(LowestAmp, HighestAmp, step, numScans)"

