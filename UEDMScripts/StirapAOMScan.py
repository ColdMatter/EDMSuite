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

<<<<<<< HEAD
def mapAOMLoop(start, end, step, numScans):
	# setup
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
=======
def mapAOMLoop(start, end, step, numScans, savingfolder = "C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\"):
	# setup
	fileSystem = savingfolder
	file = fileSystem + "scan"
>>>>>>> 4b8c910f6d93d93a61290473bb73a11e76a3312c
	print("Saving as " + file + "_*.zip")
	print("")

	# start looping
	count = 0
	for i in frange(start, end, step):
		print "AOM freq -> " + str(i) + " MHz"

		# tclProbe.SetLaserSetpoint("ProbeCavity", "TopticaSHGPZT", i)
<<<<<<< HEAD
=======
		#Let's try and change the Stirap RF frequency
		hc.SetGreenSynthFrequency(float(i))
>>>>>>> 4b8c910f6d93d93a61290473bb73a11e76a3312c

		
		System.Threading.Thread.Sleep(300)
		sm.AdjustProfileParameter("out", "externalParameters",str(i), False)
		sm.AcquireAndWait(numScans)
<<<<<<< HEAD
		scanPath = file + "_" + str(count) + ".zip"
=======
		if len(str(count)) == 2:
			filecount = str(count)
		else:
			filecount = "0"+str(count)

		scanPath = file + "_" + filecount + ".zip"
>>>>>>> 4b8c910f6d93d93a61290473bb73a11e76a3312c
		sm.SaveAverageData(scanPath)

		count += 1

	
def run_script():
<<<<<<< HEAD
	print "Use mapAOMLoop(start, end, step, numScans)"
	print "Make sure we are in 'STIRAP MWF1F0 Switch Scan' profile"
=======
	print "Use mapAOMLoop(start, end, step, numScans, savingfolder)"
	print "Make sure we are in a'STIRAP MWF1F0 Plus Switch' profile"
>>>>>>> 4b8c910f6d93d93a61290473bb73a11e76a3312c

