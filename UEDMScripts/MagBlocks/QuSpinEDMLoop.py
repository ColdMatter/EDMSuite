﻿# Import a whole load of stuff
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

def measureParametersAndMakeBC(cluster, eState, bState, rfState, mwState, scramblerV):
	fileSystem = Environs.FileSystem
	print("Measuring parameters ...")
	bh.StopPattern()
	#hc.UpdateRFPowerMonitor()
	#hc.UpdateRFFrequencyMonitor()
	bh.StartPattern()
	#hc.UpdateBCurrentMonitor()
	hc.UpdateVMonitor()
	#hc.UpdateProbeAOMV()
	#hc.UpdatePumpAOMFreqMonitor()
	#hc.CheckPiMonitor()
	#print("Measuring polarizer angle")
	#hc.UpdateProbePolAngleMonitor()
	#hc.UpdatePumpPolAngleMonitor()
	#pumpPolAngle = hc.pumpPolAngle
	#probePolAngle = hc.probePolAngle
	print("V plus: " + str(hc.CPlusMonitorVoltage * hc.CPlusMonitorScale))
	print("V minus: " + str(hc.CMinusMonitorVoltage * hc.CMinusMonitorScale))
	print("Bias: " + str(hc.BiasCurrent))
	print("B step: " + str(abs(hc.FlipStepCurrent)))
	print("DB step: " + str(abs(hc.CalStepCurrent)))
	print("Phase Lock Error (deg): "+ str(pl.PhaseError))
	# load a default BlockConfig and customise it appropriately
	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	bc = loadBlockConfig(settingsPath + "default.xml")
	bc.Settings["cluster"] = cluster
	bc.Settings["eState"] = eState
	bc.Settings["bState"] = bState
	bc.Settings["rfState"] = rfState
	bc.Settings["mwState"] = mwState
	bc.Settings["phaseScramblerV"] = scramblerV
	bc.Settings["ePlus"] = hc.CPlusMonitorVoltage * hc.CPlusMonitorScale
	bc.Settings["eMinus"] = hc.CMinusMonitorVoltage * hc.CMinusMonitorScale
	bc.Settings["bBiasV"] = hc.SteppingBiasVoltage
	bc.Settings["greenDCFM"] = hc.GreenSynthDCFM
	bc.Settings["greenAmp"] = hc.GreenSynthOnAmplitude
	bc.Settings["greenFreq"] = hc.GreenSynthOnFrequency
	bc.Settings["rf1CentreTime"] = sm.GetPGSetting("rf1CentreTime")
	bc.Settings["rf2CentreTime"] = sm.GetPGSetting("rf2CentreTime")
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
	# laser frequency stuff goes here
	bc.GetModulationByName("LF1").PhysicalCentre = hc.ProbeAOMFrequencyCentre
	bc.GetModulationByName("LF1").PhysicalStep = hc.ProbeAOMFrequencyStep
	# generate the waveform codes
	print("Generating waveform codes ...")
	eWave = bc.GetModulationByName("E").Waveform
	eWave.Name = "E"
	##lf1Wave = bc.GetModulationByName("LF1").Waveform
	##lf1Wave.Name = "LF1"
	##mwWave = bc.GetModulationByName("MW").Waveform
	##mwWave.Name = "MW"
	ws = WaveformSetGenerator.GenerateWaveforms( (eWave,), ("B","DB","PI","RF1A","RF2A","RF1F","RF2F","LF1") )
	bc.GetModulationByName("B").Waveform = ws["B"]
	bc.GetModulationByName("DB").Waveform = ws["DB"]
	bc.GetModulationByName("PI").Waveform = ws["PI"]
	bc.GetModulationByName("RF1A").Waveform = ws["RF1A"]
	bc.GetModulationByName("RF2A").Waveform = ws["RF2A"]
	bc.GetModulationByName("RF1F").Waveform = ws["RF1F"]
	bc.GetModulationByName("RF2F").Waveform = ws["RF2F"]
	bc.GetModulationByName("LF1").Waveform = ws["LF1"]	
	# change the inversions of the static codes E
	bc.GetModulationByName("E").Waveform.Inverted = WaveformSetGenerator.RandomBool()
	# Do the same for the microwave channel
	# bc.GetModulationByName("MW").Waveform.Inverted = WaveformSetGenerator.RandomBool()
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
	bc.Settings["eOvershootFactor"] = hc.EOvershootFactor
	# store the E switch asymmetry in the block
	bc.Settings["E0PlusBoost"] = hc.E0PlusBoost
	# number of times to step the target looking for a good target spot, step size is 2 (coded in Acquisitor)
	bc.Settings["maximumNumberOfTimesToStepTarget"] = 2000
	# minimum signal in the first detector, in Vus
	bc.Settings["minimumSignalToRun"] = 100.0
	bc.Settings["targetStepperGateStartTime"] = 2380.0
	bc.Settings["targetStepperGateEndTime"] = 2580.0
	return bc

# lock gains
# microamps of current per volt of control input
kSteppingBiasCurrentPerVolt = 2453.06
# max change in the b-bias voltage per block
kBMaxChange = 0.05
# volts of rf*a input required per cal's worth of offset
kRFAVoltsPerCal = 3.2
kRFAMaxChange = 0.05
# volts of rf*f input required per cal's worth of offset
kRFFVoltsPerCal = 8
kRFFMaxChange = 0.05

def updateLocks(bState):
	bottomProbeChannelValues = bh.DBlock.ChannelValues[0]
	# note the weird python syntax for a one element list
	sigValue = bottomProbeChannelValues.GetValue(("SIG",))
	bValue = bottomProbeChannelValues.GetValue(("B","MW"))
	dbValue = bottomProbeChannelValues.GetValue(("DB","MW"))
	rf1aValue = bottomProbeChannelValues.GetValue(("RF1A","DB","MW"))
	rf2aValue = bottomProbeChannelValues.GetValue(("RF2A","DB","MW"))
	rf1fValue = bottomProbeChannelValues.GetValue(("RF1F","DB","MW"))
	rf2fValue = bottomProbeChannelValues.GetValue(("RF2F","DB","MW"))
	print "SIG: " + str(sigValue)
	print "B: " + str(bValue) + " DB: " + str(dbValue)
	print "RF1A: " + str(rf1aValue) + " RF2A: " + str(rf2aValue)
	print "RF1F: " + str(rf1fValue) + " RF2F: " + str(rf2fValue)
	if dbValue < 4:
		print "DB value too low, not applying feedback"
	else:
		print "feeding back to Bias and rf parameters"
		# B bias lock
		# the sign of the feedback depends on the b-state and the microwave state 
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
	# the sign of the feedback depends on the b-state and the microwave state
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
	deltaLF1 = windowValue(deltaLF1, -0.1, 0.1)
	#deltaLF1 = 0
	print "Attempting to change LF1 by " + str(deltaLF1) + " V."
	newLF1 = windowValue( hc.probeAOMVoltage - deltaLF1, hc.probeAOMStep, 10 - hc.probeAOMStep )
	hc.SetprobeAOMVoltage( newLF1 )
	
	# Laser frequency lock (-ve multiplier in f0 mode and +ve in f1)
	deltaLF2 =  - 2.5 * lf2dbdbValue
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

def QuSpinGo():
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
	eState = hc.EManualState
	print("E-state: " + str(eState))
	bState = hc.BManualState
	print("B-state: " + str(bState))
	rfState = hc.RFManualState
	print("rf-state: " + str(rfState))
	mwState = hc.MWManualState
	print("mw-state: " + str(mwState))
	# this is to make sure the B current monitor is in a sensible state
	#hc.UpdateBCurrentMonitor()
	# randomise Ramsey phase 
	scramblerV = 0.97156 * r.NextDouble()
	hc.SetScramblerVoltage(scramblerV)

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

	bc = measureParametersAndMakeBC(cluster, eState, bState, rfState, mwState, scramblerV)

	# loop and take data
	blockIndex = 0
	maxBlockIndex = 10000
	dbValueList = []
	Emag1List =[]
	Emini1List=[]
	Emini2List=[]
	Emini3List=[]
	while blockIndex < maxBlockIndex:
		print("Acquiring MAGNETIC FIELD block " + str(blockIndex) + " ...")
		# save the block config and load into blockhead
		print("Saving temp config.")
		bc.Settings["clusterIndex"] = blockIndex
		tempConfigFile ='%(p)stemp%(c)s_%(i)s.xml' % {'p': settingsPath, 'c': cluster, 'i': blockIndex}
		saveBlockConfig(tempConfigFile, bc)
		System.Threading.Thread.Sleep(500)
		print("Loading temp config.")
		bh.LoadConfig(tempConfigFile)
		# take the block and save it
		print("Running magnetic field data acquisition ...")
		bh.StartMagDataAcquisitionAndWait()
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
		# checkYAGAndFix()
		blockIndex = blockIndex + 1
		#updateLocks(bState)
		# randomise Ramsey phase
		scramblerV = 0.97156 * r.NextDouble()
		hc.SetScramblerVoltage(scramblerV)

		bc = measureParametersAndMakeBC(cluster, eState, bState, rfState, mwState, scramblerV)

		#hc.EnableAnapicoListSweep( True )
		#print("ListSweep for microwaves enabled")

		# some code to stop EDMLoop if the laser unlocks. 
		# This averages the last 3 db values and stops the loop if the average is below 1
		
		#bValueList.append(dbValue)
		#if (len(dbValueList) == 4):
		#	del dbValueList[0]
		#print "DB values for last 3 blocks " + str(dbValueList).strip('[]')
		#runningdbMean =float(sum(dbValueList)) / len(dbValueList)
		#if ( runningdbMean < 1 and nightBool is "Y" ):	
		#	hc.EnableEField( False )
		#	hc.SetArgonShutter( True )
		#	break

		#Emag1List.append(magEValue)
		#if (len(Emag1List) == 11):
		#	del Emag1List[0]
		#print "E_{Mag} for the last 10 blocks " + str(Emag1List).strip('[]')
		#runningEmag1Mean =float(sum(Emag1List)) / len(Emag1List)
		#print "Average E_{Mag} for the last 10 blocks " + str(runningEmag1Mean)


		#if (dbValue < 8):
		#	print("Dodgy spot target rotation.")
		#	for i in range(3):
		#		hc.StepTarget(2)
		#		System.Threading.Thread.Sleep(500)
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
	QuSpinGo()

