#MapLoop - asks ScanMaster to make a series of scans with AOM rf freq changed

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

def mapAOMLoop(start, end, step, numScans, savingfolder = "C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\"):
	# setup
	fileSystem = savingfolder
	file = fileSystem + "scan"
	print("Saving as " + file + "_*.zip")
	print("")

	# start looping
	count = 0
	for i in frange(start, end, step):
		print "AOM freq -> " + str(i) + " MHz"

		# tclProbe.SetLaserSetpoint("ProbeCavity", "TopticaSHGPZT", i)
		#Let's try and change the Stirap RF frequency
		hc.SetGreenSynthFrequency(float(i))

		
		System.Threading.Thread.Sleep(300)
		sm.AdjustProfileParameter("out", "externalParameters",str(i), False)
		sm.AcquireAndWait(numScans)
		if len(str(count)) == 2:
			filecount = str(count)
		else:
			filecount = "0"+str(count)

		scanPath = file + "_" + filecount + ".zip"
		sm.SaveAverageData(scanPath)

		count += 1

	
def run_script():
	print "Use mapAOMLoop(start, end, step, numScans, savingfolder)"
	print "Make sure we are in a'STIRAP MWF1F0 Plus Switch' profile"

