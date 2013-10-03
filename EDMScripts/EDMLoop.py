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

def measureParametersAndMakeBC(cluster, eState, bState, rfState, scramblerV, measProbePwr, measPumpPwr):
	fileSystem = Environs.FileSystem
	print("Measuring parameters ...")
	bh.StopPattern()
	hc.UpdateRFPowerMonitor()
	hc.UpdateRFFrequencyMonitor()
	bh.StartPattern()
	hc.UpdateBCurrentMonitor()
	hc.UpdateVMonitor()
	hc.UpdateI2AOMFreqMonitor()
	hc.UpdatePumpAOMFreqMonitor()
	hc.UpdateVCOFraction()
	print("Measuring polarizer angle")
	hc.UpdateProbePolAngleMonitor()
	hc.UpdatePumpPolAngleMonitor()
	pumpPolAngle = hc.pumpPolAngle
	probePolAngle = hc.probePolAngle
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
	bc.Settings["rfState"] = rfState
	bc.Settings["phaseScramblerV"] = scramblerV
	bc.Settings["probePolarizerAngle"] = probePolAngle
	bc.Settings["pumpPolarizerAngle"] = pumpPolAngle
	bc.Settings["ePlus"] = hc.CPlusMonitorVoltage * hc.CPlusMonitorScale
	bc.Settings["eMinus"] = hc.CMinusMonitorVoltage * hc.CMinusMonitorScale
	bc.Settings["pumpAOMFreq"] = hc.PumpAOMFrequencyCentre 
	bc.Settings["bBiasV"] = hc.SteppingBiasVoltage
	bc.Settings["greenDCFM"] = hc.GreenSynthDCFM
	bc.Settings["greenAmp"] = hc.GreenSynthOnAmplitude
	bc.Settings["greenFreq"] = hc.GreenSynthOnFrequency
	bc.Settings["measStartProbePwr"] = measProbePwr
	bc.Settings["measStartPumpPwr"] = measPumpPwr
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
	bc.GetModulationByName("LF2").Centre = hc.PumpAOMVoltage
	bc.GetModulationByName("LF2").Centre = hc.PumpAOMStep
	bc.GetModulationByName("LF2").PhysicalCentre = hc.PumpAOMFrequencyCentre
	bc.GetModulationByName("LF2").PhysicalStep = hc.PumpAOMFrequencyStep

	# generate the waveform codes
	print("Generating waveform codes ...")
	eWave = bc.GetModulationByName("E").Waveform
	eWave.Name = "E"
	lf1Wave = bc.GetModulationByName("LF1").Waveform
	lf1Wave.Name = "LF1"
	ws = WaveformSetGenerator.GenerateWaveforms( (eWave, lf1Wave), ("B","DB","PI","RF1A","RF2A","RF1F","RF2F","LF2") )
	bc.GetModulationByName("B").Waveform = ws["B"]
	bc.GetModulationByName("DB").Waveform = ws["DB"]
	bc.GetModulationByName("PI").Waveform = ws["PI"]
	bc.GetModulationByName("RF1A").Waveform = ws["RF1A"]
	bc.GetModulationByName("RF2A").Waveform = ws["RF2A"]
	bc.GetModulationByName("RF1F").Waveform = ws["RF1F"]
	bc.GetModulationByName("RF2F").Waveform = ws["RF2F"]
	bc.GetModulationByName("LF2").Waveform = ws["LF2"]
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
	# printWaveformCode(bc, "LF2")
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
	# store the E switch asymmetry in the block
	bc.Settings["E0PlusBoost"] = hc.E0PlusBoost
	return bc

# lock gains
# microamps of current per volt of control input
kSteppingBiasCurrentPerVolt = 2453.06
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
	sigValue = pmtChannelValues.GetValue(("SIG",))
	bValue = pmtChannelValues.GetValue(("B",))
	dbValue = pmtChannelValues.GetValue(("DB",))
	rf1aValue = pmtChannelValues.GetValue(("RF1A","DB"))
	rf2aValue = pmtChannelValues.GetValue(("RF2A","DB"))
	rf1fValue = pmtChannelValues.GetValue(("RF1F","DB"))
	rf2fValue = pmtChannelValues.GetValue(("RF2F","DB"))
	lf1Value = pmtChannelValues.GetValue(("LF1",))
	lf1dbValue = pmtChannelValues.GetValue(("LF1","DB"))
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
	deltaBias = - (1.0/10.0) * feedbackSign * (hc.CalStepCurrent * (bValue / dbValue)) / kSteppingBiasCurrentPerVolt
	deltaBias = windowValue(deltaBias, -kBMaxChange, kBMaxChange)
	print "Attempting to change stepping B bias by " + str(deltaBias) + " V."
	newBiasVoltage = windowValue( hc.SteppingBiasVoltage - deltaBias, -5, 5)
	hc.SetSteppingBBiasVoltage( newBiasVoltage )
	# RFA  locks
	deltaRF1A = - (6.0/3.0) * (rf1aValue / dbValue) * kRFAVoltsPerCal
	deltaRF1A = windowValue(deltaRF1A, -kRFAMaxChange, kRFAMaxChange)
	print "Attempting to change RF1A by " + str(deltaRF1A) + " V."
	newRF1A = windowValue( hc.RF1AttCentre - deltaRF1A, hc.RF1AttStep, 5 - hc.RF1AttStep)
	hc.SetRF1AttCentre( newRF1A )
	#
	deltaRF2A = - (6.0/3.0) * (rf2aValue / dbValue) * kRFAVoltsPerCal
	deltaRF2A = windowValue(deltaRF2A, -kRFAMaxChange, kRFAMaxChange)
	print "Attempting to change RF2A by " + str(deltaRF2A) + " V."
	newRF2A = windowValue( hc.RF2AttCentre - deltaRF2A, hc.RF2AttStep, 5 - hc.RF2AttStep )
	hc.SetRF2AttCentre( newRF2A )
	# RFF  locks
	deltaRF1F = - (10.0/4.0) * (rf1fValue / dbValue) * kRFFVoltsPerCal
	deltaRF1F = windowValue(deltaRF1F, -kRFFMaxChange, kRFFMaxChange)
	print "Attempting to change RF1F by " + str(deltaRF1F) + " V."
	newRF1F = windowValue( hc.RF1FMCentre - deltaRF1F, hc.RF1FMStep, 5 - hc.RF1FMStep)
	hc.SetRF1FMCentre( newRF1F )
	#
	deltaRF2F = - (10.0/4.0) * (rf2fValue / dbValue) * kRFFVoltsPerCal
	deltaRF2F = windowValue(deltaRF2F, -kRFFMaxChange, kRFFMaxChange)
	print "Attempting to change RF2F by " + str(deltaRF2F) + " V."
	newRF2F = windowValue( hc.RF2FMCentre - deltaRF2F, hc.RF2FMStep, 5 - hc.RF2FMStep )
	hc.SetRF2FMCentre( newRF2F )
	deltaLF1 = -1.25 * (lf1Value / dbValue)
	deltaLF1 = windowValue(deltaLF1, -0.1, 0.1)
	print "Attempting to change LF1 by " + str(deltaLF1) + " V."
	newLF1 = windowValue( hc.FLPZTVoltage - deltaLF1, hc.FLPZTStep, 5 - hc.FLPZTStep )
	hc.SetFLPZTVoltage( newLF1 )

def updateLocksNL(bState):
	pmtChannelValues = bh.DBlock.ChannelValues[0]
	normedpmtChannelValues = bh.DBlock.ChannelValues[8]
	rf1ampReftChannelValues = bh.DBlock.ChannelValues[6]
	rf2ampReftChannelValues = bh.DBlock.ChannelValues[7]
	# note the weird python syntax for a one element list
	sigValue = pmtChannelValues.GetValue(("SIG",))
	bValue = pmtChannelValues.GetValue(("B",))
	dbValue = pmtChannelValues.GetValue(("DB",))
	bDBValue = normedpmtChannelValues.GetSpecialValue("BDB")
	rf1aValue = pmtChannelValues.GetValue(("RF1A",))
	rf1adbdbValue = normedpmtChannelValues.GetSpecialValue("RF1ADBDB")
	rf2aValue = pmtChannelValues.GetValue(("RF2A",))
	rf2adbdbValue = normedpmtChannelValues.GetSpecialValue("RF2ADBDB")
	rf1fValue = pmtChannelValues.GetValue(("RF1F",))
	rf1fdbdbValue = normedpmtChannelValues.GetSpecialValue("RF1FDBDB")
	rf2fValue = pmtChannelValues.GetValue(("RF2F",))
	rf2fdbdbValue = normedpmtChannelValues.GetSpecialValue("RF2FDBDB")
	lf1Value = pmtChannelValues.GetValue(("LF1",))
	lf1dbdbValue = normedpmtChannelValues.GetSpecialValue("LF1DBDB")
	lf1dbValue = normedpmtChannelValues.GetSpecialValue("LF1DB")
	lf2Value = pmtChannelValues.GetValue(("LF2",))
	lf2dbdbValue = pmtChannelValues.GetSpecialValue("LF2DBDB")
	rf1ampRefSig = rf1ampReftChannelValues.GetValue(("SIG",))
	rf2ampRefSig = rf2ampReftChannelValues.GetValue(("SIG",))
	rf1ampRefE = rf1ampReftChannelValues.GetValue(("E",))
	rf2ampRefE = rf2ampReftChannelValues.GetValue(("E",))
	rf1ampRefEErr = rf1ampReftChannelValues.GetError(("E",))
	rf2ampRefEErr = rf2ampReftChannelValues.GetError(("E",))

	print "SIG: " + str(sigValue)
	print "B: " + str(bValue) + " DB: " + str(dbValue)
	print "B/DB" + str(bDBValue)
	print "RF1A: " + str(rf1aValue) + " RF2A: " + str(rf2aValue)
	print "RF1A.DB/DB: " + str(rf1adbdbValue) + " RF2A.DB/DB: " + str(rf2adbdbValue)
	print "RF1F: " + str(rf1fValue) + " RF2F: " + str(rf2fValue)
	print "LF1: " + str(lf1Value) + " LF1.DB/DB: " + str(lf1dbdbValue)
	print "LF2: " + str(lf2Value) + " LF2.DB/DB: " + str(lf2dbdbValue)
	print "RF1 Reflected: " + str(rf1ampRefSig) +  " RF2 Reflected: " + str(rf2ampRefSig) 
	print "{E}_RF1 Reflected: {" + str(rf1ampRefE) + " , " + str(rf1ampRefEErr) + " }"
	print "{E}_RF2 Reflected: {" + str(rf2ampRefE) + " , " + str(rf2ampRefEErr) + " }"
	
	# B bias lock
	# the sign of the feedback depends on the b-state
	if bState: 
		feedbackSign = 1
	else: 
		feedbackSign = -1
	deltaBias = - (1.0/10.0) * feedbackSign * (hc.CalStepCurrent * bDBValue) / kSteppingBiasCurrentPerVolt 
	deltaBias = windowValue(deltaBias, -kBMaxChange, kBMaxChange)
	#deltaBias = 0
	print "Attempting to change stepping B bias by " + str(deltaBias) + " V."
	newBiasVoltage = windowValue( hc.SteppingBiasVoltage - deltaBias, -5, 5)
	hc.SetSteppingBBiasVoltage( newBiasVoltage )
	# RFA  locks
	deltaRF1A = - (1.0/2.0) * rf1adbdbValue * kRFAVoltsPerCal
	deltaRF1A = windowValue(deltaRF1A, -kRFAMaxChange, kRFAMaxChange)
	#deltaRF1A = 0
	newRF1A = windowValue( hc.RF1AttCentre - deltaRF1A, hc.RF1AttStep, 5 - hc.RF1AttStep)
	if (newRF1A == 4.9):
		newSynthAmp =  hc.GreenSynthOnAmplitude + 1
		print "RF1A pinned, increasing synth to  " + str(newSynthAmp) + " dBm."
		print "Setting RF1A to 4.5 V."
		newRF1A = 4.5
		hc.SetRF1AttCentre( newRF1A )
		hc.SetGreenSynthAmp(newSynthAmp)
	else:
		print "Attempting to change RF1A by " + str(deltaRF1A) + " V."
		hc.SetRF1AttCentre( newRF1A )
	#
	deltaRF2A = - (1.0/2.0) * rf2adbdbValue * kRFAVoltsPerCal
	deltaRF2A = windowValue(deltaRF2A, -kRFAMaxChange, kRFAMaxChange)
	#deltaRF2A = 0
	newRF2A = windowValue( hc.RF2AttCentre - deltaRF2A, hc.RF2AttStep, 5 - hc.RF2AttStep )
	if (newRF2A == 4.9):
		newSynthAmp =  hc.GreenSynthOnAmplitude + 1
		print "RF2A pinned, increasing synth to  " + str(newSynthAmp) + " dBm."
		print "Setting RF2A to 4.5 V."
		newRF2A = 4.5
		hc.SetRF2AttCentre( newRF2A )
		hc.SetGreenSynthAmp(newSynthAmp)
	else:
		print "Attempting to change RF2A by " + str(deltaRF2A) + " V."
		hc.SetRF2AttCentre( newRF2A )

	# RFF  locks
	deltaRF1F = - (1.0/2.0) * rf1fdbdbValue * kRFFVoltsPerCal
	deltaRF1F = windowValue(deltaRF1F, -kRFFMaxChange, kRFFMaxChange)
	#deltaRF1F = 0
	print "Attempting to change RF1F by " + str(deltaRF1F) + " V."
	newRF1F = windowValue( hc.RF1FMCentre - deltaRF1F, hc.RF1FMStep, 1.1 - hc.RF1FMStep)
	hc.SetRF1FMCentre( newRF1F )
	#
	deltaRF2F = - (1.0/2.0) * rf2fdbdbValue * kRFFVoltsPerCal
	deltaRF2F = windowValue(deltaRF2F, -kRFFMaxChange, kRFFMaxChange)
	#deltaRF2F = 0
	print "Attempting to change RF2F by " + str(deltaRF2F) + " V."
	newRF2F = windowValue( hc.RF2FMCentre - deltaRF2F, hc.RF2FMStep, 1.1 - hc.RF2FMStep )
	hc.SetRF2FMCentre( newRF2F )

	# Laser frequency lock (-ve multiplier in f0 mode and +ve in f1)
	deltaLF1 = -2.5* ( lf1dbdbValue)
	#deltaLF1 = 2.5 * ( lf1dbValue) (for Diode laser)
	deltaLF1 = windowValue(deltaLF1, -0.1, 0.1)
	#deltaLF1 = 0
	print "Attempting to change LF1 by " + str(deltaLF1) + " V."
	newLF1 = windowValue( hc.FLPZTVoltage - deltaLF1, hc.FLPZTStep, 10 - hc.FLPZTStep )
	hc.SetFLPZTVoltage( newLF1 )
	
	# Laser frequency lock (-ve multiplier in f0 mode and +ve in f1)
	# first cancel the overal movement of the laser
	deltaLF2 = hc.VCOConvFrac * deltaLF1 - 2.5 * lf2dbdbValue
	deltaLF2 = hc.VCOConvFrac * deltaLF1
	deltaLF2 = windowValue(deltaLF2, -0.1, 0.1)
	#deltaLF2 = 0
	print "Attempting to change LF2 by " + str(deltaLF2) + " V."
	newLF2 = windowValue( hc.PumpAOMVoltage - deltaLF2, hc.PumpAOMStep, 10 - hc.PumpAOMStep )
	hc.SetPumpAOMVoltage( newLF2 )

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
	measProbePwr = prompt("Measured probe power (mV_3): ")
	measPumpPwr = prompt("Measured pump power (mV_3): ")
	nightBool = prompt("Night run (Y/N)? ") 
	eState = hc.EManualState
	print("E-state: " + str(eState))
	bState = hc.BManualState
	print("B-state: " + str(bState))
	rfState = hc.RFManualState
	print("rf-state: " + str(rfState))

	# this is to make sure the B current monitor is in a sensible state
	hc.UpdateBCurrentMonitor()
	# randomise Ramsey phase 
	scramblerV = 0.97156 * r.NextDouble()
	hc.SetScramblerVoltage(scramblerV)
	# randomise polarizations
	hc.SetRandomProbePosition()
	hc.SetRandomPumpPosition()

	# calibrate leakage monitors
	print("calibrating leakage monitors..")
	print("E-field off")
	hc.EnableGreenSynth( False )
	hc.EnableEField( False )
	System.Threading.Thread.Sleep(10000)
	hc.EnableBleed( True )
	System.Threading.Thread.Sleep(5000)
	hc.CalibrateIMonitors()
	hc.EnableBleed( False )
	System.Threading.Thread.Sleep(500)
	print("E-field on")
	hc.EnableEField( True )
	hc.EnableGreenSynth( True )
	print("leakage monitors calibrated")
	#print("Waiting For Polarizers (maybe)")

	bc = measureParametersAndMakeBC(cluster, eState, bState, rfState, scramblerV, measProbePwr, measPumpPwr)

	# loop and take data
	blockIndex = 0
	maxBlockIndex = 10000
	dbValueList = []
	Emag1List =[]
	Emini1List=[]
	Emini2List=[]
	Emini3List=[]
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
		updateLocksNL(bState)
		# randomise Ramsey phase
		scramblerV = 0.97156 * r.NextDouble()
		hc.SetScramblerVoltage(scramblerV)
		# randomise polarizations
		hc.SetRandomProbePosition()
		hc.SetRandomPumpPosition()

		bc = measureParametersAndMakeBC(cluster, eState, bState, rfState, scramblerV, measProbePwr, measPumpPwr)
		pmtChannelValues = bh.DBlock.ChannelValues[0]
		magChannelValues = bh.DBlock.ChannelValues[2]
		mini1ChannelValues = bh.DBlock.ChannelValues[9]
		mini2ChannelValues = bh.DBlock.ChannelValues[10]
		mini3ChannelValues = bh.DBlock.ChannelValues[11]
		dbValue = pmtChannelValues.GetValue(("DB",))
		magEValue = magChannelValues.GetValue(("E",))
		mini1EValue = mini1ChannelValues.GetValue(("E",))
		mini2EValue = mini2ChannelValues.GetValue(("E",))
		mini3EValue = mini3ChannelValues.GetValue(("E",))
		

		# some code to stop EDMLoop if the laser unlocks. 
		# This averages the last 3 db values and stops the loop if the average is below 1
		
		dbValueList.append(dbValue)
		if (len(dbValueList) == 4):
			del dbValueList[0]
		print "DB values for last 3 blocks " + str(dbValueList).strip('[]')
		runningdbMean =float(sum(dbValueList)) / len(dbValueList)
		if ( runningdbMean < 1 and nightBool is "Y" ):	
			hc.EnableEField( False )
			hc.SetArgonShutter( True )
			break

		Emag1List.append(magEValue)
		if (len(Emag1List) == 11):
			del Emag1List[0]
		print "E_{Mag} for the last 10 blocks " + str(Emag1List).strip('[]')
		runningEmag1Mean =float(sum(Emag1List)) / len(Emag1List)
		print "Average E_{Mag} for the last 10 blocks " + str(runningEmag1Mean)


		if (dbValue < 19):
			print("Dodgy spot target rotation.")
			for i in range(4):
				hc.StepTarget(20)
				System.Threading.Thread.Sleep(500)
		if ((blockIndex % kReZeroLeakageMonitorsPeriod) == 0):
			print("Recalibrating leakage monitors.")
			# calibrate leakage monitors
			print("calibrating leakage monitors..")
			print("E-field off")
			hc.EnableEField( False )
			System.Threading.Thread.Sleep(10000)
			hc.EnableBleed( True )
			System.Threading.Thread.Sleep(5000)
			hc.CalibrateIMonitors()
			hc.EnableBleed( False )
			System.Threading.Thread.Sleep(500)
			print("E-field on")
			hc.EnableEField( True )
			print("leakage monitors calibrated")

	bh.StopPattern()


def run_script():
	EDMGo()

