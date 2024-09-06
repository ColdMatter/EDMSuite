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

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def measureParametersAndMakeBC(cluster, eState, bState, rfState, mwState, scramblerV, eFastFlag):
	fileSystem = Environs.FileSystem
	print("Measuring parameters ...")
	bh.StopPattern()
	# hc.UpdateRFPowerMonitor()
	# hc.UpdateRFFrequencyMonitor()
	bh.StartPattern()
	hc.PollVMonitor()
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
	bc.Settings["greenDCFM"] = 0.0#hc.GreenSynthDCFM
	bc.Settings["greenAmp"] = 0.0#hc.GreenSynthOnAmplitude
	bc.Settings["greenFreq"] = 172.0#hc.GreenSynthOnFrequency
	bc.Settings["rf1CentreTime"] = 1400#sm.GetPGSetting("rf1CentreTime")
	bc.Settings["rf2CentreTime"] = 1400#sm.GetPGSetting("rf2CentreTime")
	bc.GetModulationByName("B").Centre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").Step = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").Step = abs(hc.CalStepCurrent)/1000
	# these next 3, seemingly redundant, lines are to preserve backward compatibility
	bc.GetModulationByName("B").PhysicalCentre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").PhysicalStep = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").PhysicalStep = abs(hc.CalStepCurrent)/1000
	bc.GetModulationByName("RF1A").Centre = 0#hc.RF1AttCentre
	bc.GetModulationByName("RF1A").Step = 0#hc.RF1AttStep
	bc.GetModulationByName("RF1A").PhysicalCentre = 0#hc.RF1PowerCentre
	bc.GetModulationByName("RF1A").PhysicalStep = 0#hc.RF1PowerStep
	bc.GetModulationByName("RF2A").Centre = 0#hc.RF2AttCentre
	bc.GetModulationByName("RF2A").Step = 0#hc.RF2AttStep
	bc.GetModulationByName("RF2A").PhysicalCentre = 0#hc.RF2PowerCentre
	bc.GetModulationByName("RF2A").PhysicalStep = 0#hc.RF2PowerStep
	bc.GetModulationByName("RF1F").Centre = 0#hc.RF1FMCentre
	bc.GetModulationByName("RF1F").Step = 0#hc.RF1FMStep
	bc.GetModulationByName("RF1F").PhysicalCentre = 0#hc.RF1FrequencyCentre
	bc.GetModulationByName("RF1F").PhysicalStep = 0#hc.RF1FrequencyStep
	bc.GetModulationByName("RF2F").Centre = 0#hc.RF2FMCentre
	bc.GetModulationByName("RF2F").Step = 0#hc.RF2FMStep
	bc.GetModulationByName("RF2F").PhysicalCentre = 0#hc.RF2FrequencyCentre
	bc.GetModulationByName("RF2F").PhysicalStep = 0#hc.RF2FrequencyStep
	# laser frequency stuff goes here
	bc.GetModulationByName("LF1").PhysicalCentre = 0#hc.ProbeAOMFrequencyCentre
	bc.GetModulationByName("LF1").PhysicalStep = 0#hc.ProbeAOMFrequencyStep
	# generate the waveform codes
	print("Generating waveform codes ...")
	eWave = bc.GetModulationByName("E").Waveform
	eWave.Name = "E"
	##lf1Wave = bc.GetModulationByName("LF1").Waveform
	##lf1Wave.Name = "LF1"
	##mwWave = bc.GetModulationByName("MW").Waveform
	##mwWave.Name = "MW"
	ws = WaveformSetGenerator.GenerateWaveforms( (eWave,), ("B","DB","PI","RF1A","RF2A","RF1F","RF2F","LF1") )
	bc.GetModulationByName("E").Waveform = eWave
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
	eFieldVoltagesInput = prompt("E-field voltages in kV: ")
	eFieldVoltages = eFieldVoltagesInput.split(",")
	cluster = prompt("Cluster name [" + suggestedClusterName +"]: ")
	if cluster == "":
		cluster = suggestedClusterName
		print("Using cluster " + suggestedClusterName)
	eState = hc.EManualState
	eCurrentState = hc.EFieldPolarity
	cPlusV = 3*(hc.CPlusVoltage)
	cMinusV = 3*(hc.CMinusVoltage)
	print("E-state: " + str(eState))
	bState = True #hc.BManualState
	print("B-state: " + str(bState))
	rfState = True #hc.RFManualState
	print("rf-state: " + str(rfState))
	mwState = True #hc.MWManualState
	print("mw-state: " + str(mwState))
	# this is to make sure the B current monitor is in a sensible state
	#hc.UpdateBCurrentMonitor()
	# randomise Ramsey phase 
	scramblerV = 0.97156 * r.NextDouble()
	# hc.SetScramblerVoltage(scramblerV)

	# calibrate leakage monitors
	print("calibrating leakage monitors..")
	print("E-field off")
	# hc.EnableGreenSynth( False )
	hc.FieldsOff()
	# hc.EnableEField( False )
	System.Threading.Thread.CurrentThread.Join(60000)
	# hc.EnableBleed( True )
	# System.Threading.Thread.Sleep(5000)
	hc.CalibrateIMonitors()
	# hc.EnableBleed( False )
	# System.Threading.Thread.Sleep(500)
	hc.SetCPlusVoltage(cPlusV)
	hc.SetCMinusVoltage(cMinusV)
	print("E Params refreshed")
	System.Threading.Thread.CurrentThread.Join(5000)
	print("E-field on")
	hc.SwitchEAndWait(eCurrentState)
	print("E Switch Finished")
	System.Threading.Thread.CurrentThread.Join(5000)
	print("E Switch wait finished")
	# hc.EnableEField( True )
	# hc.EnableGreenSynth( True )
	print("leakage monitors calibrated")

	bc = measureParametersAndMakeBC(cluster, eState, bState, rfState, mwState, scramblerV, False)

	# loop and take data
	blockIndex = 0
	maxBlockIndex = 10000
	dbValueList = []
	Emag1List =[]
	Emini1List=[]
	Emini2List=[]
	Emini3List=[]
	while blockIndex < maxBlockIndex:
		for i in range(len(eFieldVoltages)):
			#SLOW E SWITCH
			eCurrentState = hc.EFieldPolarity
			hc.SetCPlusVoltage(float(eFieldVoltages[i]))
			hc.SetCMinusVoltage(float(eFieldVoltages[i]))
			System.Threading.Thread.CurrentThread.Join(1000)
			hc.SwitchEAndWait(eCurrentState)
			print("Acquiring MAGNETIC FIELD block " + str(blockIndex) + " with E-field voltages set to " + str(eFieldVoltages[i]) + "kV")
			# save the block config and load into blockhead
			print("Saving temp config.")
			bc.Settings["clusterIndex"] = blockIndex
			tempConfigFile ='%(p)stemp%(c)s_%(i)s.xml' % {'p': settingsPath, 'c': cluster, 'i': blockIndex}
			saveBlockConfig(tempConfigFile, bc)
			System.Threading.Thread.CurrentThread.Join(500)
			print("Loading temp config.")
			bh.LoadConfig(tempConfigFile)
			# take the block and save it
			print("Running magnetic field data acquisition ...")
			bh.StartMagDataAcquisitionAndWait()
			print("Done.")
			blockPath = '%(p)s%(c)s_%(v)s_%(i)s.zip' % {'p': dataPath, 'c': cluster, 'v': str(eFieldVoltages[i]) + "kV", 'i': blockIndex}
			bh.SaveBlock(blockPath)
			print("Saved block "+ str(blockIndex) + ".")
			# give mma a chance to analyse the block
			# print("Notifying Mathematica and waiting ...")
			writeLatestBlockNotificationFile(cluster, blockIndex)
			System.Threading.Thread.CurrentThread.Join(5000)
			print("Done.")
			# increment and loop
			File.Delete(tempConfigFile)

			blockIndex = blockIndex + 1
			# randomise Ramsey phase
			scramblerV = 0.97156 * r.NextDouble()
			# hc.SetScramblerVoltage(scramblerV)

			#bc = measureParametersAndMakeBC(cluster, eState, bState, rfState, mwState, scramblerV, True)

			# if ((blockIndex % kReZeroLeakageMonitorsPeriod) == 0):
			# 	print("Recalibrating leakage monitors.")
			# 	# calibrate leakage monitors
			# 	print("calibrating leakage monitors..")
			# 	print("E-field off")
			# 	hc.EnableEField( False )
			# 	System.Threading.Thread.Sleep(10000)
			# 	hc.EnableBleed( True )
			# 	System.Threading.Thread.Sleep(5000)
			# 	hc.CalibrateIMonitors()
			# 	hc.EnableBleed( False )
			# 	System.Threading.Thread.Sleep(500)
			# 	print("E-field on")
			# 	hc.EnableEField( True )
			# 	print("leakage monitors calibrated")

			# # FAST E SWITCH
			# hc.SetEFieldVoltages(float(eFieldVoltages[i]))
			# print("Acquiring MAGNETIC FIELD block " + str(blockIndex) + " with E-field voltages set to " + str(eFieldVoltages[i]) + "kV")
			# # save the block config and load into blockhead
			# print("Saving temp config.")
			# bc.Settings["clusterIndex"] = blockIndex
			# tempConfigFile ='%(p)stemp%(c)s_%(i)s.xml' % {'p': settingsPath, 'c': cluster, 'i': blockIndex}
			# saveBlockConfig(tempConfigFile, bc)
			# System.Threading.Thread.Sleep(500)
			# print("Loading temp config.")
			# bh.LoadConfig(tempConfigFile)
			# # take the block and save it
			# print("Running magnetic field data acquisition ...")
			# bh.StartMagDataAcquisitionAndWait()
			# print("Done.")
			# blockPath = '%(p)s%(c)s_%(v)s_%(i)s.zip' % {'p': dataPath, 'c': cluster, 'v': str(eFieldVoltages[i]) + "kV", 'i': blockIndex}
			# bh.SaveBlock(blockPath)
			# print("Saved block "+ str(blockIndex) + ".")
			# # give mma a chance to analyse the block
			# # print("Notifying Mathematica and waiting ...")
			# writeLatestBlockNotificationFile(cluster, blockIndex)
			# System.Threading.Thread.Sleep(5000)
			# print("Done.")
			# # increment and loop
			# File.Delete(tempConfigFile)
			# # checkYAGAndFix()
			# blockIndex = blockIndex + 1
			# # randomise Ramsey phase
			# scramblerV = 0.97156 * r.NextDouble()
			# hc.SetScramblerVoltage(scramblerV)

			bc = measureParametersAndMakeBC(cluster, eState, bState, rfState, mwState, scramblerV, False)

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
				hc.SetCPlusVoltage(cPlusV)
				hc.SetCMinusVoltage(cMinusV)
				hc.SwitchEAndWait(eCurrentState)
				print("E Switch Finished")
				System.Threading.Thread.CurrentThread.Join(10000)

				print("leakage monitors calibrated")

	bh.StopPattern()


def run_script():
	QuSpinGo()

