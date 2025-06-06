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
	BlockConfig_type = Type.GetType("EDMConfig.BlockConfig, SharedCode") # Pythonnet requires explicit typing of the xmlserializer where IronPython figured it out
	s = XmlSerializer(BlockConfig_type)
	s.Serialize(fs,config)
	fs.Close()

def loadBlockConfig(path):
	fs = FileStream(path, FileMode.Open)
	BlockConfig_type = Type.GetType("EDMConfig.BlockConfig, SharedCode") # Pythonnet requires explicit typing of the xmlserializer where IronPython figured it out
	s = XmlSerializer(BlockConfig_type)
	bc = s.Deserialize(fs)
	fs.Close()
	return bc

def writeLatestBlockNotificationFile(cluster, blockIndex):
	fs = FileStream(Environs.FileSystem.Paths["settingsPath"] + "\\BlockHead\\latestBlock.txt", FileMode.Create)
	sw = StreamWriter(fs)
	sw.WriteLine(cluster + "\t" + str(blockIndex))
	sw.Close()
	fs.Close()

# def checkYAGAndFix():
# 	interlockFailed = hc.YAGInterlockFailed
# 	if (interlockFailed):
# 		bh.StopPattern()
# 		bh.StartPattern()

def checkPhaseLock():
	while (abs(pl.PhaseError) > 5):
		prompt("Check Phase Lock is running (hit enter if you are happy.)")
	print("Phase Lock checked.")

def printWaveformCode(bc, name):
	print(name + ": " + str(bc.GetModulationByName(name).Waveform.Code) + " -- " + str(bc.GetModulationByName(name).Waveform.Inverted))

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def measureParametersAndMakeBC(cluster, eState, bState, mwState): #used to have, rfState, mwState, scramblerV):
	fileSystem = Environs.FileSystem
	#fl.StopFieldLock()
	#print("B field lock OFF")
	print("Measuring parameters ...")
	#bh.StopPattern()
	# hc.UpdateRFPowerMonitor()
	print("Waiting 0s for YAG laser head to cool down..")
	#System.Threading.Thread.CurrentThread.Join(90000)
	#hc.UpdateRFFrequencyMonitor()
	#bh.StartPattern()
	#print("Waiting for YAG to start ...")
	#System.Threading.Thread.CurrentThread.Join(5000)
	#fl.LockField()
	#print("B field lock ON")
	hc.UpdateBCurrentMonitor()
	hc.PollVMonitor()
	# hc.UpdateProbeAOMV()
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
	#bc.Settings["rfState"] = rfState
	bc.Settings["mwState"] = mwState
	# bc.Settings["phaseScramblerV"] = scramblerV
	bc.Settings["ePlus"] = hc.CPlusMonitorVoltage * hc.CPlusMonitorScale
	bc.Settings["eMinus"] = hc.CMinusMonitorVoltage * hc.CMinusMonitorScale
	bc.Settings["bBiasV"] = hc.SteppingBiasVoltage
	bc.Settings["greenDCFM"] = 0.0#hc.GreenSynthDCFM
	bc.Settings["greenAmp"] = hc.GreenSynthOnAmplitude
	bc.Settings["greenFreq"] = hc.GreenSynthOnFrequency
	#bc.Settings["rf1CentreTime"] = 1400#sm.GetPGSetting("rf1CentreTime")
	#bc.Settings["rf2CentreTime"] = 1400#sm.GetPGSetting("rf2CentreTime")
	bc.GetModulationByName("B").Centre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").Step = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").Step = abs(hc.CalStepCurrent)/1000
	# these next 3, seemingly redundant, lines are to preserve backward compatibility
	bc.GetModulationByName("B").PhysicalCentre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").PhysicalStep = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").PhysicalStep = abs(hc.CalStepCurrent)/1000
	# bc.GetModulationByName("RF1A").Centre = 0#hc.RF1AttCentre
	# bc.GetModulationByName("RF1A").Step = 0#hc.RF1AttStep
	# bc.GetModulationByName("RF1A").PhysicalCentre = 0#hc.RF1PowerCentre
	# bc.GetModulationByName("RF1A").PhysicalStep = 0#hc.RF1PowerStep
	# bc.GetModulationByName("RF2A").Centre = 0#hc.RF2AttCentre
	# bc.GetModulationByName("RF2A").Step = 0#hc.RF2AttStep
	# bc.GetModulationByName("RF2A").PhysicalCentre = 0#hc.RF2PowerCentre
	# bc.GetModulationByName("RF2A").PhysicalStep = 0#hc.RF2PowerStep
	# bc.GetModulationByName("RF1F").Centre = 0#hc.RF1FMCentre
	# bc.GetModulationByName("RF1F").Step = 0#hc.RF1FMStep
	# bc.GetModulationByName("RF1F").PhysicalCentre = 0#hc.RF1FrequencyCentre
	# bc.GetModulationByName("RF1F").PhysicalStep = 0#hc.RF1FrequencyStep
	# bc.GetModulationByName("RF2F").Centre = 0#hc.RF2FMCentre
	# bc.GetModulationByName("RF2F").Step = 0#hc.RF2FMStep
	# bc.GetModulationByName("RF2F").PhysicalCentre = 0#hc.RF2FrequencyCentre
	# bc.GetModulationByName("RF2F").PhysicalStep = 0#hc.RF2FrequencyStep
	# laser frequency stuff goes here
	# bc.GetModulationByName("LF1").PhysicalCentre = 0#hc.ProbeAOMFrequencyCentre
	# bc.GetModulationByName("LF1").PhysicalStep = 0#hc.ProbeAOMFrequencyStep
	# generate the waveform codes
	print("Generating waveform codes ...")
	eWave = bc.GetModulationByName("E").Waveform
	eWave.Name = "E"
	##lf1Wave = bc.GetModulationByName("LF1").Waveform
	##lf1Wave.Name = "LF1"
	##mwWave = bc.GetModulationByName("MW").Waveform
	##mwWave.Name = "MW"
	ws = WaveformSetGenerator.GenerateWaveforms( (eWave,), ("B","DB"))#,"PI","RF1A","RF2A","RF1F","RF2F","LF1") )
	bc.GetModulationByName("B").Waveform = ws["B"]
	bc.GetModulationByName("DB").Waveform = ws["DB"]
	#bc.GetModulationByName("PI").Waveform = ws["PI"]
	#bc.GetModulationByName("RF1A").Waveform = ws["RF1A"]
	#bc.GetModulationByName("RF2A").Waveform = ws["RF2A"]
	#bc.GetModulationByName("RF1F").Waveform = ws["RF1F"]
	#bc.GetModulationByName("RF2F").Waveform = ws["RF2F"]
	#bc.GetModulationByName("LF1").Waveform = ws["LF1"]	
	# change the inversions of the static codes E
	bc.GetModulationByName("E").Waveform.Inverted = WaveformSetGenerator.RandomBool()
	# Do the same for the microwave channel
	# bc.GetModulationByName("MW").Waveform.Inverted = WaveformSetGenerator.RandomBool()
	# print the waveform codes
	printWaveformCode(bc, "E")
	printWaveformCode(bc, "B")
	printWaveformCode(bc, "DB")
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
	bc.Settings["maximumNumberOfTimesToStepTarget"] = 100
	# minimum signal in the first detector, in Vus
	bc.Settings["minimumSignalToRun"] = 100.0
	bc.Settings["targetStepperGateStartTime"] = 2050.0
	bc.Settings["targetStepperGateEndTime"] = 2850.0
	bc.Settings["liveAnalysisGateLow"] = 10000.0
	bc.Settings["liveAnalysisGateHigh"] = 45000.0
	return bc

# lock gains
# microamps of current per volt of control input
kSteppingBiasCurrentPerVolt = 350.79
# max change in the b-bias voltage per block
kBMaxChange = 0.2
# volts of rf*a input required per cal's worth of offset
# kRFAVoltsPerCal = 3.2
# kRFAMaxChange = 0.05
# volts of rf*f input required per cal's worth of offset
# kRFFVoltsPerCal = 8
# kRFFMaxChange = 0.05
	
def updateLocks(bState, mwState):
	sigValue = bh.AnalysedDBlock.SIGValAndErr[0]
	bValue = bh.AnalysedDBlock.BValAndErr[0]
	dbValue = bh.AnalysedDBlock.DBValAndErr[0]
	#bDBValue = bh.AnalysedDBlock.BDBValAndErr[0]
	# rf1aValue = bh.AnalysedDBlock.rf1AmpAndErr[0]
	# rf1adbdbValue = bh.AnalysedDBlock.RF1ADBDB[0]
	# rf2aValue = bh.AnalysedDBlock.rf2AmpAndErr[0]
	# rf2adbdbValue = bh.AnalysedDBlock.RF2ADBDB[0]
	# rf1fValue = bh.AnalysedDBlock.rf1FreqAndErr[0]
	# rf1fdbdbValue = bh.AnalysedDBlock.RF1FDBDB[0]
	# rf2fValue = bh.AnalysedDBlock.rf2FreqAndErr[0]
	# rf2fdbdbValue = bh.AnalysedDBlock.RF2FDBDB[0]
	# lf1Value = bh.AnalysedDBlock.LF1ValAndErr[0]
	# lf1dbdbValue = bh.AnalysedDBlock.LF1DBDB[0]
	# lf1dbValue = bh.AnalysedDBlock.LF1DB[0]
	#lf2Value = pmtChannelValues.GetValue(("LF2",))
	#lf2dbdbValue = pmtChannelValues.GetSpecialValue("LF2DBDB")
	#rf1ampRefSig = rf1ampReftChannelValues.GetValue(("SIG",))
	#rf2ampRefSig = rf2ampReftChannelValues.GetValue(("SIG",))
	#rf1ampRefE = rf1ampReftChannelValues.GetValue(("E",))
	#rf2ampRefE = rf2ampReftChannelValues.GetValue(("E",))
	#rf1ampRefEErr = rf1ampReftChannelValues.GetError(("E",))
	#rf2ampRefEErr = rf2ampReftChannelValues.GetError(("E",))

	print "SIG: " + str(sigValue)
	print "B: " + str(bValue) + " DB: " + str(dbValue)
	#print "B/DB" + str(bDBValue)
	# print "RF1A: " + str(rf1aValue) + " RF2A: " + str(rf2aValue)
	# print "RF1A.DB/DB: " + str(rf1adbdbValue) + " RF2A.DB/DB: " + str(rf2adbdbValue)
	# print "RF1F: " + str(rf1fValue) + " RF2F: " + str(rf2fValue)
	# print "LF1: " + str(lf1Value) + " LF1.DB/DB: " + str(lf1dbdbValue)
	#print "LF2: " + str(lf2Value) + " LF2.DB/DB: " + str(lf2dbdbValue)
	#print "RF1 Reflected: " + str(rf1ampRefSig) +  " RF2 Reflected: " + str(rf2ampRefSig)
	#print "{E}_RF1 Reflected: {" + str(rf1ampRefE) + " , " + str(rf1ampRefEErr) + " }"
	#print "{E}_RF2 Reflected: {" + str(rf2ampRefE) + " , " + str(rf2ampRefEErr) + " }"

	# B bias lock
	# the sign of the feedback depends only on the b-state
	# when mw manual state is false, {DB} flips sign as well
	if bState:
		feedbackSign = 1
	else:
		feedbackSign = -1
	
	# if mwState:
	# 	rfFeedbackSign = 1
	# else:
	# 	rfFeedbackSign = -1
		
	deltaBias = - (1.0/10.0) * feedbackSign * (hc.CalStepCurrent * (bValue / dbValue)) / kSteppingBiasCurrentPerVolt
	deltaBias = windowValue(deltaBias, -kBMaxChange, kBMaxChange)
	print "Attempting to change stepping B bias by " + str(deltaBias) + " V."
	
	newBiasVoltage = windowValue( hc.SteppingBiasVoltage - deltaBias, -5, 5)
	hc.SetSteppingBBiasVoltage( newBiasVoltage )

	# RFA  locks
	# deltaRF1A = - (3.0/3.0) * rf1adbdbValue * kRFAVoltsPerCal
	# deltaRF1A = windowValue(deltaRF1A, -kRFAMaxChange, kRFAMaxChange)
	# print "Attempting to change RF1A by " + str(deltaRF1A) + " V."
	# newRF1A = windowValue( hc.RF1AttCentre - deltaRF1A, hc.RF1AttStep, 5 - hc.RF1AttStep)
	# hc.SetRF1AttCentre( newRF1A )
	# #
	# deltaRF2A = - (3.0/3.0) * rf2adbdbValue * kRFAVoltsPerCal
	# deltaRF2A = windowValue(deltaRF2A, -kRFAMaxChange, kRFAMaxChange)
	# print "Attempting to change RF2A by " + str(deltaRF2A) + " V."
	# newRF2A = windowValue( hc.RF2AttCentre - deltaRF2A, hc.RF2AttStep, 5 - hc.RF2AttStep )
	# hc.SetRF2AttCentre( newRF2A )
	# # RFF  locks
	# deltaRF1F = - (1.0/4.0) * rf1fdbdbValue * kRFFVoltsPerCal
	# deltaRF1F = windowValue(deltaRF1F, -kRFFMaxChange, kRFFMaxChange)
	# print "Attempting to change RF1F by " + str(deltaRF1F) + " V."
	# newRF1F = windowValue( hc.RF1FMCentre - deltaRF1F, hc.RF1FMStep, 5 - hc.RF1FMStep)
	# hc.SetRF1FMCentre( newRF1F )
	# #
	# deltaRF2F = - (1.0/4.0) * rf2fdbdbValue * kRFFVoltsPerCal
	# deltaRF2F = windowValue(deltaRF2F, -kRFFMaxChange, kRFFMaxChange)
	# print "Attempting to change RF2F by " + str(deltaRF2F) + " V."
	# newRF2F = windowValue( hc.RF2FMCentre - deltaRF2F, hc.RF2FMStep, 5 - hc.RF2FMStep )
	# hc.SetRF2FMCentre( newRF2F )

	#Laser frequency lock using TCL
	#deltaLF1setpoint = 0.25 * (lf1dbdbValue)
	# maxLF1SetPointChange = 0.005
	# deltaLF1setpoint = windowValue( 0.25 * (lf1dbdbValue), -maxLF1SetPointChange, maxLF1SetPointChange )
	# print "Attempting to change tclProbe setpoint by " + str(deltaLF1setpoint) + " V."
	# tclProbe.SetLaserSetpoint("ProbeCavity", "TopticaSHGPZT",tclProbe.GetLaserSetpoint("ProbeCavity", "TopticaSHGPZT") +deltaLF1setpoint)

	# Laser frequency lock (-ve multiplier in f0 mode and +ve in f1)
	#deltaLF2 =  - 2.5 * lf2dbdbValue
	#deltaLF2 = windowValue(deltaLF2, -0.1, 0.1)
	#deltaLF2 = 0
	#print "Attempting to change LF2 by " + str(deltaLF2) + " V."
	#newLF2 = windowValue( hc.PumpAOMVoltage - deltaLF2, hc.PumpAOMStep, 10 - hc.PumpAOMStep )
	#hc.SetPumpAOMVoltage( newLF2 )

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
	checkPhaseLock()
	eState = hc.EManualState
	eCurrentState = hc.EFieldPolarity
	cPlusV = 3*(hc.CPlusVoltage)
	cMinusV = 3*(hc.CMinusVoltage)
	print("E-state: " + str(eState))
	bState = hc.BManualState
	print("B-state: " + str(bState))
	# rfState = True#hc.RFManualState
	# print("rf-state: " + str(rfState))
	mwState = True#hc.MWManualState
	print("mw-state: " + str(mwState))

	# this is to make sure the B current monitor is in a sensible state
	hc.UpdateBCurrentMonitor()

	# randomise Ramsey phase
	# scramblerV = 0.97156 * r.NextDouble()
	# hc.SetScramblerVoltage(scramblerV)
	# print("scrambler voltage set to " + str(scramblerV))

	# calibrate leakage monitors
	print("calibrating leakage monitors..")
	print("E-field off")
	hc.FieldsOff()
	# hc.EnableGreenSynth( False )
	# hc.EnableEField( False )
	System.Threading.Thread.CurrentThread.Join(60000)
	# hc.EnableBleed( True )
	# System.Threading.Thread.CurrentThread.Join(5000)
	hc.CalibrateIMonitors()
	# hc.EnableBleed( False )
	# System.Threading.Thread.CurrentThread.Join(500)
	# hc.SetCPlusVoltage(cPlusV)
	# hc.SetCMinusVoltage(cMinusV)
	print("E Params refreshed")
	System.Threading.Thread.CurrentThread.Join(5000)
	print("E-field on")
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(20000)
	print("leakage monitors calibrated")

	bc = measureParametersAndMakeBC(cluster, eState, bState, mwState)#, rfState, mwState, scramblerV)

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
		System.Threading.Thread.CurrentThread.Join(500)
		print("Loading temp config.")
		hc.EnableGreenSynth( False )
		bh.LoadConfig(tempConfigFile)
		# take the block and save it
		# print("Running Target Stepper ...")
		#fl.StopFieldLock()
		# bh.StartTargetStepperAndWait()
		# print("Target Stepper finished")
		hc.EnableGreenSynth( True )
		# if bh.TargetHealthy == False:
		# 	print("Unable to find acceptable spot")
		# 	print("Stopping Cluster")
		# 	hc.EnableEField( False )
		# 	#fl.StopFieldLock()
		# 	bh.StopPattern()
		# 	break
		#fl.LockField()
		print("Running Block ...")
		bh.StartPattern()
		System.Threading.Thread.CurrentThread.Join(2000)

		bh.AcquireAndWait()
		print("Done.")
		bh.StopPattern()
		blockPath = '%(p)s%(c)s_%(i)s.zip' % {'p': dataPath, 'c': cluster, 'i': blockIndex}
		bh.SaveBlock(blockPath)
		print("Saved block "+ str(blockIndex) + ".")
		# give mma a chance to analyse the block
		# print("Notifying Mathematica and waiting ...")
		writeLatestBlockNotificationFile(cluster, blockIndex)
		System.Threading.Thread.CurrentThread.Join(5000)
		print("Done.")

		#Step target
		print("New target position.")
		bh.StartPattern()
		System.Threading.Thread.CurrentThread.Join(2000)

		for step in range(4):
			hc.StepTarget(3)
			System.Threading.Thread.CurrentThread.Join(500)

		System.Threading.Thread.CurrentThread.Join(2000)
		bh.StopPattern()
		# increment and loop
		File.Delete(tempConfigFile)
		# checkYAGAndFix()
		blockIndex = blockIndex + 1
		
		updateLocks(bState, mwState)
		# updateLocksNL(bState, mwState)
		# randomise Ramsey phase
		# scramblerV = 0.97156 * r.NextDouble()
		# hc.SetScramblerVoltage(scramblerV)
		#print("setting green synth amp to: " + str(3.8 + blockIndex % 4))
		#hc.SetGreenSynthAmp(3.8 + blockIndex % 4)

		bc = measureParametersAndMakeBC(cluster, eState, bState, mwState)#, rfState, mwState, scramblerV)
		#pmtChannelValues = bh.DBlock.ChannelValues[0]
		#magChannelValues = bh.DBlock.ChannelValues[2]
		#mini1ChannelValues = bh.DBlock.ChannelValues[9]
		#mini2ChannelValues = bh.DBlock.ChannelValues[10]
		#mini3ChannelValues = bh.DBlock.ChannelValues[11]
		#dbValue = pmtChannelValues.GetValue(("DB",))
		#magEValue = magChannelValues.GetValue(("E",))
		#mini1EValue = mini1ChannelValues.GetValue(("E",))
		#mini2EValue = mini2ChannelValues.GetValue(("E",))
		#mini3EValue = mini3ChannelValues.GetValue(("E",))

		# hc.EnableAnapicoListSweep( True )
		# print("ListSweep for microwaves enabled")

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
		#		System.Threading.Thread.CurrentThread.Join(500)
		if ((blockIndex % kReZeroLeakageMonitorsPeriod) == 0):
			print("Recalibrating leakage monitors.")
			# calibrate leakage monitors

			eCurrentState = hc.EFieldPolarity
			cPlusV = 3*(hc.CPlusVoltage)
			cMinusV = 3*(hc.CMinusVoltage)

			print("E-field off")
			hc.FieldsOff()

			System.Threading.Thread.CurrentThread.Join(60000)
			hc.CalibrateIMonitors()
			# hc.EnableBleed( False )
			# System.Threading.Thread.CurrentThread.Join(500)
			print("E-field on")
			hc.EnableEField(True)
			# hc.SetCPlusVoltage(cPlusV)
			# hc.SetCMinusVoltage(cMinusV)
			# hc.SwitchEAndWait(eCurrentState)
			# print("E Switch Finished")
			System.Threading.Thread.CurrentThread.Join(20000)

			print("leakage monitors calibrated")

		# if ((blockIndex % kReZeroLeakageMonitorsPeriod) == 0):
		# 	print("Recalibrating leakage monitors.")
		# 	# calibrate leakage monitors
		# 	print("calibrating leakage monitors..")
		# 	print("E-field off")
		# 	hc.EnableEField( False )
		# 	System.Threading.Thread.CurrentThread.Join(10000)
		# 	hc.EnableBleed( True )
		# 	System.Threading.Thread.CurrentThread.Join(5000)
		# 	hc.CalibrateIMonitors()
		# 	hc.EnableBleed( False )
		# 	System.Threading.Thread.CurrentThread.Join(500)
		# 	print("E-field on")
		# 	hc.EnableEField( True )
		# 	print("leakage monitors calibrated")

	bh.StopPattern()


def run_script():
	EDMGo()
