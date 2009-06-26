# Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *

from Analysis.EDM import *
from DAQ.Environment import *
from EDMConfig import *

def saveBlockConfig(path, config):
	fs = FileStream(path, FileMode.Create)
	s = XmlSerializer(BlockConfig)
	s.Serialize(fs,config)
	fs.Close()

def loadBlockConfig(path):
	fs = FileStream(path, FileMode.Open)
	s = XmlSerializer(BlockConfig)
	bc = s.Deserialize(fs)
	fs.Close()
	return bc

def writeLatestBlockNotificationFile(cluster, blockIndex):
	fs = FileStream(Environs.FileSystem.Paths["settingsPath"] + "\\BlockHead\\latestBlock.txt", FileMode.Create)
	sw = StreamWriter(fs)
	sw.WriteLine(cluster + "\t" + str(blockIndex))
	sw.Close()
	fs.Close()

def checkYAGAndFix():
	interlockFailed = hc.YAGInterlockFailed;
	if (interlockFailed):
		bh.StopPattern();
		bh.StartPattern();

def printWaveformCode(bc, name):
	print(name + ": " + str(bc.GetModulationByName(name).Waveform.Code) + " -- " + str(bc.GetModulationByName(name).Waveform.Inverted))

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def measureParametersAndMakeBC(cluster, eState, bState, scramblerV, polAngle):
	fileSystem = Environs.FileSystem
	print("Measuring parameters ...")
	bh.StopPattern()
	hc.UpdateRFPowerMonitor()
	hc.UpdateRFFrequencyMonitor()
	bh.StartPattern()
	hc.UpdateBCurrentMonitor()
	hc.UpdateVMonitor()
	hc.UpdateI2AOMFreqMonitor()
	print("V plus: " + str(hc.CPlusMonitorVoltage * hc.CPlusMonitorScale))
	print("V minus: " + str(hc.CMinusMonitorVoltage * hc.CMinusMonitorScale))
	print("Bias: " + str(hc.BiasCurrent))
	print("B step: " + str(abs(hc.FlipStepCurrent)))
	print("DB step: " + str(abs(hc.CalStepCurrent)))
	# load a default BlockConfig and customise it appropriately
	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	bc = loadBlockConfig(settingsPath + "default.xml")
	bc.Settings["cluster"] = cluster
	bc.Settings["eState"] = eState
	bc.Settings["bState"] = bState
	bc.Settings["phaseScramblerV"] = scramblerV
	bc.Settings["probePolarizerAngle"] = polAngle
	bc.Settings["ePlus"] = hc.CPlusMonitorVoltage * hc.CPlusMonitorScale
	bc.Settings["eMinus"] = hc.CMinusMonitorVoltage * hc.CMinusMonitorScale
	bc.GetModulationByName("B").Centre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").Step = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").Step = abs(hc.CalStepCurrent)/1000
	# these next 3, seemingly redundant, lines are to preserve backward compatibility
	bc.GetModulationByName("B").PhysicalCentre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").PhysicalStep = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").PhysicalStep = abs(hc.CalStepCurrent)/1000
	bc.GetModulationByName("RF1A").Centre = hc.RF1AttCentre
	bc.GetModulationByName("RF1A").Step = hc.RF1AttStep
	bc.GetModulationByName("RF1A").PhysicalCentre = hc.RF1PowerCentre
	bc.GetModulationByName("RF1A").PhysicalStep = hc.RF1PowerStep
	bc.GetModulationByName("RF2A").Centre = hc.RF2AttCentre
	bc.GetModulationByName("RF2A").Step = hc.RF2AttStep
	bc.GetModulationByName("RF2A").PhysicalCentre = hc.RF2PowerCentre
	bc.GetModulationByName("RF2A").PhysicalStep = hc.RF2PowerStep
	bc.GetModulationByName("RF1F").Centre = hc.RF1FMCentre
	bc.GetModulationByName("RF1F").Step = hc.RF1FMStep
	bc.GetModulationByName("RF1F").PhysicalCentre = hc.RF1FrequencyCentre
	bc.GetModulationByName("RF1F").PhysicalStep = hc.RF1FrequencyStep
	bc.GetModulationByName("RF2F").Centre = hc.RF2FMCentre
	bc.GetModulationByName("RF2F").Step = hc.RF2FMStep
	bc.GetModulationByName("RF2F").PhysicalCentre = hc.RF2FrequencyCentre
	bc.GetModulationByName("RF2F").PhysicalStep = hc.RF2FrequencyStep
	bc.GetModulationByName("LF1").Centre = hc.FLPZTVoltage
	bc.GetModulationByName("LF1").Step = hc.FLPZTStep
	bc.GetModulationByName("LF1").PhysicalCentre = hc.I2LockAOMFrequencyCentre
	bc.GetModulationByName("LF1").PhysicalStep = hc.I2LockAOMFrequencyStep
	# generate the waveform codes
	print("Generating waveform codes ...")
	eWave = bc.GetModulationByName("E").Waveform
	eWave.Name = "E"
	lf1Wave = bc.GetModulationByName("LF1").Waveform
	lf1Wave.Name = "LF1"
	ws = WaveformSetGenerator.GenerateWaveforms( (eWave, lf1Wave), ("B","DB","PI","RF1A","RF2A","RF1F","RF2F") )
	bc.GetModulationByName("B").Waveform = ws["B"]
	bc.GetModulationByName("DB").Waveform = ws["DB"]
	bc.GetModulationByName("PI").Waveform = ws["PI"]
	bc.GetModulationByName("RF1A").Waveform = ws["RF1A"]
	bc.GetModulationByName("RF2A").Waveform = ws["RF2A"]
	bc.GetModulationByName("RF1F").Waveform = ws["RF1F"]
	bc.GetModulationByName("RF2F").Waveform = ws["RF2F"]
	# change the inversions of the static codes E and LF1
	bc.GetModulationByName("E").Waveform.Inverted = WaveformSetGenerator.RandomBool()
	bc.GetModulationByName("LF1").Waveform.Inverted = WaveformSetGenerator.RandomBool()
	# print the waveform codes
	# printWaveformCode(bc, "E")
	# printWaveformCode(bc, "B")
	# printWaveformCode(bc, "DB")
	# printWaveformCode(bc, "PI")
	# printWaveformCode(bc, "RF1A")
	# printWaveformCode(bc, "RF2A")
	# printWaveformCode(bc, "RF1F")
	# printWaveformCode(bc, "RF2F")
	# printWaveformCode(bc, "LF1")
	# store e-switch info in block config
	print("Storing E switch parameters ...")
	bc.Settings["eRampDownTime"] = hc.ERampDownTime
	bc.Settings["eRampDownDelay"] = hc.ERampDownDelay
	bc.Settings["eBleedTime"] = hc.EBleedTime
	bc.Settings["eSwitchTime"] = hc.ESwitchTime
	bc.Settings["eRampUpTime"] = hc.ERampUpTime
	bc.Settings["eRampUpDelay"] = hc.ERampUpDelay
	# this is for legacy analysis compatibility
	bc.Settings["eDischargeTime"] = hc.ERampDownTime + hc.ERampDownDelay
	bc.Settings["eChargeTime"] = hc.ERampUpTime + hc.ERampUpDelay
	return bc

# lock gains
# microamps of current per volt of control input
kSteppingBiasCurrentPerVolt = 1000.0
# max change in the b-bias voltage per block
kBMaxChange = 0.05
# volts of rf*a input required per cal's worth of offset
kRFAVoltsPerCal = 3.2
kRFAMaxChange = 0.1
# volts of rf*f input required per cal's worth of offset
kRFFVoltsPerCal = 8
kRFFMaxChange = 0.1

def updateLocks(bState):
	pmtChannelValues = bh.DBlock.ChannelValues[0]
	# note the weird python syntax for a one element list
	sigIndex = pmtChannelValues.GetChannelIndex(("SIG",))
	sigValue = pmtChannelValues.GetValue(sigIndex)
	bIndex = pmtChannelValues.GetChannelIndex(("B",))
	bValue = pmtChannelValues.GetValue(bIndex)
	#bError = pmtChannelValues.GetError(bIndex)
	dbIndex = pmtChannelValues.GetChannelIndex(("DB",))
	dbValue = pmtChannelValues.GetValue(dbIndex)
	#dbError = pmtChannelValues.GetError(dbIndex)
	rf1aIndex = pmtChannelValues.GetChannelIndex(("RF1A","DB"))
	rf1aValue = pmtChannelValues.GetValue(rf1aIndex)
	#rf1aError = pmtChannelValues.GetError(rf1aIndex)
	rf2aIndex = pmtChannelValues.GetChannelIndex(("RF2A","DB"))
	rf2aValue = pmtChannelValues.GetValue(rf2aIndex)
	#rf2aError = pmtChannelValues.GetError(rf2aIndex)
	rf1fIndex = pmtChannelValues.GetChannelIndex(("RF1F","DB"))
	rf1fValue = pmtChannelValues.GetValue(rf1fIndex)
	#rf1fError = pmtChannelValues.GetError(rf1fIndex)
	rf2fIndex = pmtChannelValues.GetChannelIndex(("RF2F","DB"))
	rf2fValue = pmtChannelValues.GetValue(rf2fIndex)
	#rf2fError = pmtChannelValues.GetError(rf2fIndex)
	lf1Index = pmtChannelValues.GetChannelIndex(("LF1",))
	lf1Value = pmtChannelValues.GetValue(lf1Index)
	#lf1Error = pmtChannelValues.GetError(lf1Index)
	lf1dbIndex = pmtChannelValues.GetChannelIndex(("LF1","DB"))
	lf1dbValue = pmtChannelValues.GetValue(lf1dbIndex)
	print "SIG: " + str(sigValue)
	print "B: " + str(bValue) + " DB: " + str(dbValue)
	print "RF1A: " + str(rf1aValue) + " RF2A: " + str(rf2aValue)
	print "RF1F: " + str(rf1fValue) + " RF2F: " + str(rf2fValue)
	print "LF1: " + str(lf1Value) + " LF1.DB: " + str(lf1dbValue)
	# B bias lock
	# the sign of the feedback depends on the b-state
	if bState: 
		feedbackSign = 1
	else: 
		feedbackSign = -1
	deltaBias = - (1.0/3.0) * feedbackSign * (hc.CalStepCurrent * (bValue / dbValue)) / kSteppingBiasCurrentPerVolt
	deltaBias = windowValue(deltaBias, -kBMaxChange, kBMaxChange)
	print "Attempting to change stepping B bias by " + str(deltaBias) + " V."
	newBiasVoltage = windowValue( hc.SteppingBiasVoltage - deltaBias, 0, 5)
	hc.SetSteppingBBiasVoltage( newBiasVoltage )
	# RFA  locks
	deltaRF1A = - (1.0/3.0) * (rf1aValue / dbValue) * kRFAVoltsPerCal
	deltaRF1A = windowValue(deltaRF1A, -kRFAMaxChange, kRFAMaxChange)
	print "Attempting to change RF1A by " + str(deltaRF1A) + " V."
	newRF1A = windowValue( hc.RF1AttCentre - deltaRF1A, hc.RF1AttStep, 5 - hc.RF1AttStep)
	hc.SetRF1AttCentre( newRF1A )
	#
	deltaRF2A = - (1.0/3.0) * (rf2aValue / dbValue) * kRFAVoltsPerCal
	deltaRF2A = windowValue(deltaRF2A, -kRFAMaxChange, kRFAMaxChange)
	print "Attempting to change RF2A by " + str(deltaRF2A) + " V."
	newRF2A = windowValue( hc.RF2AttCentre - deltaRF2A, hc.RF2AttStep, 5 - hc.RF2AttStep )
	hc.SetRF2AttCentre( newRF2A )
	# RFF  locks
	deltaRF1F = - (1.0/4.0) * (rf1fValue / dbValue) * kRFFVoltsPerCal
	deltaRF1F = windowValue(deltaRF1F, -kRFFMaxChange, kRFFMaxChange)
	print "Attempting to change RF1F by " + str(deltaRF1F) + " V."
	newRF1F = windowValue( hc.RF1FMCentre - deltaRF1F, hc.RF1FMStep, 5 - hc.RF1FMStep)
	hc.SetRF1FMCentre( newRF1F )
	#
	deltaRF2F = - (1.0/4.0) * (rf2fValue / dbValue) * kRFFVoltsPerCal
	deltaRF2F = windowValue(deltaRF2F, -kRFFMaxChange, kRFFMaxChange)
	print "Attempting to change RF2F by " + str(deltaRF2F) + " V."
	newRF2F = windowValue( hc.RF2FMCentre - deltaRF2F, hc.RF2FMStep, 5 - hc.RF2FMStep )
	hc.SetRF2FMCentre( newRF2F )
	# Laser frequency lock
	deltaLF1 = -1.25 * (lf1Value / dbValue)
	deltaLF1 = windowValue(deltaLF1, -0.1, 0.1)
	print "Attempting to change LF1 by " + str(deltaLF1) + " V."
	newLF1 = windowValue( hc.FLPZTVoltage - deltaLF1, 0, 5 )
	hc.SetFLPZTVoltage( newLF1 )


def windowValue(value, minValue, maxValue):
	if ( (value < maxValue) & (value > minValue) ):
		return value
	else:
		if (value < minValue):
			return minValue
		else:
			return maxValue

kTargetRotationPeriod = 10
kReZeroLeakageMonitorsPeriod = 10
r = Random()

def EDMGo():
	# Setup
	f = None
	fileSystem = Environs.FileSystem
	dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	print("Data directory is : " + dataPath)
	print("")
	suggestedClusterName = fileSystem.GenerateNextDataFileName()
	sm.SelectProfile("Scan B")

	# User inputs data
	cluster = prompt("Cluster name [" + suggestedClusterName +"]: ")
	if cluster == "":
		cluster = suggestedClusterName
		print("Using cluster " + suggestedClusterName)
	eState = Boolean.Parse(prompt("E-state: "))
	bState = Boolean.Parse(prompt("B-state: "))

	# this is to make sure the B current monitor is in a sensible state
	hc.UpdateBCurrentMonitor()
	# randomise Ramsey phase
	scramblerV = 0.724774 * r.NextDouble()
	hc.SetScramblerVoltage(scramblerV)
	# randomise polarization
	polAngle = 360.0 * r.NextDouble()
	hc.SetPolarizerAngle(polAngle)
	bc = measureParametersAndMakeBC(cluster, eState, bState, scramblerV, polAngle)

	# loop and take data
	blockIndex = 0
	maxBlockIndex = 10000
	while blockIndex < maxBlockIndex:
		print("Acquiring block " + str(blockIndex) + " ...")
		# save the block config and load into blockhead
		print("Saving temp config.")
		bc.Settings["clusterIndex"] = blockIndex
		tempConfigFile ='%(p)stemp%(c)s_%(i)s.xml' % {'p': settingsPath, 'c': cluster, 'i': blockIndex}
		saveBlockConfig(tempConfigFile, bc)
		System.Threading.Thread.Sleep(500)
		print("Loading temp config.")
		bh.LoadConfig(tempConfigFile)
		# take the block and save it
		print("Running ...")
		bh.AcquireAndWait()
		print("Done.")
		blockPath = '%(p)s%(c)s_%(i)s.zip' % {'p': dataPath, 'c': cluster, 'i': blockIndex}
		bh.SaveBlock(blockPath)
		print("Saved block "+ str(blockIndex) + ".")
		# give mma a chance to analyse the block
		print("Notifying Mathematica and waiting ...")
		writeLatestBlockNotificationFile(cluster, blockIndex)
		System.Threading.Thread.Sleep(5000)
		print("Done.")
		# increment and loop
		File.Delete(tempConfigFile)
		checkYAGAndFix()
		blockIndex = blockIndex + 1
		updateLocks(bState)
		# randomise Ramsey phase
		scramblerV = 0.724774 * r.NextDouble()
		hc.SetScramblerVoltage(scramblerV)
		# randomise polarization
		polAngle = 360.0 * r.NextDouble()
		hc.SetPolarizerAngle(polAngle)
		bc = measureParametersAndMakeBC(cluster, eState, bState, scramblerV, polAngle)	
		# do things that need periodically doing
	#	if ((blockIndex % kTargetRotationPeriod) == 0):
		#	print("Rotating target.")
		#	hc.StepTarget(15)
		pmtChannelValues = bh.DBlock.ChannelValues[0]
		dbIndex = pmtChannelValues.GetChannelIndex(("DB",))
		dbValue = pmtChannelValues.GetValue(dbIndex)
		if (dbValue < 5.5):
			print("Dodgy spot target rotation.")
			hc.StepTarget(1)
		if ((blockIndex % kReZeroLeakageMonitorsPeriod) == 0):
			print("Recalibrating leakage monitors.")
			hc.EnableEField( False )
			System.Threading.Thread.Sleep(10000)
			hc.EnableBleed( True )
			System.Threading.Thread.Sleep(1000)
			hc.EnableBleed( False )
			System.Threading.Thread.Sleep(5000)
			hc.CalibrateIMonitors()
			hc.EnableEField( True )

	bh.StopPattern()


def run_script():
	EDMGo()


