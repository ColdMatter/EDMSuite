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

def mapLaserSetPointLoop(start, end, step, numScans):
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
	for i in frange(start, end, step):
		print "laser set point -> " + str(i) + " V"
		tclProbe.SetLaserSetpoint("ProbeCavity", "TopticaSHGPZT", i)
		System.Threading.Thread.Sleep(300)
		sm.AdjustProfileParameter("out", "externalParameters",str(i), False)
		sm.AcquireAndWait(numScans)
		scanPath = file + "_" + str(count) + ".zip"
		sm.SaveAverageData(scanPath)

		count += 1

	
def run_script():
	print "Use mapLaserSetPointLoop(start, end, step, numScans)"
	print "Make sure we are in 'Scan B' profile"

