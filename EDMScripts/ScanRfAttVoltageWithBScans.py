# MapLoop - asks ScanMaster to make a series of scans with rf attenuator voltage changed

from DAQ.Environment import *

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def frange(start, stop=None, step=None):
	start = float(start)
	if stop == None:
		stop = start + 0.0
		start = 0.0
	if step == None:
		step = 1.0

	count = 0
	while True:
		temp = float(start + count * step)
		if step > 0 and temp >= stop:
			break
		elif step < 0 and temp <= stop:
			break
		yield temp
		count += 1

def mapLoop(start, end, step, numScans, rfSelect):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")

	# start looping
	count = 0
	rfPower = 0.0
	hc.EnableGreenSynth(True)
	for i in frange(start, end, step):
		if rfSelect:
			print "rf1 attenuator voltage -> " + str(i) + " V"
			hc.SetRF1AttCentre(i)
			hc.SetAttenutatorVoltages()
			rfPower = hc.RF1Power
			print "rf1 power -> " + str(rfPower) + " dBm"
		else:
			print "rf2 attenuator voltage -> " + str(i) + " V"
			hc.SetRF2AttCentre(i)
			hc.SetAttenutatorVoltages()
			rfPower = hc.RF2Power
			print "rf2 power -> " + str(rfPower) + " dBm"
		
		sm.AdjustProfileParameter("out", "externalParameters",str(rfPower), False)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(count) + ".zip"
		sm.SaveAverageData(scanPath)

		count += 1

	
def run_script():
	print "Use mapLoop(start, end, step, numScans, rfSelect)"
	print "rfSelect is True for rf1, False for rf2"
	print "Make sure we are in 'Scan B' profile"

