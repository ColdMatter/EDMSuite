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
import time

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
	interlockFailed = hc.YAGInterlockFailed
	if (interlockFailed):
		bh.StopPattern()
		bh.StartPattern()

def printWaveformCode(bc, name):
	print(name + ": " + str(bc.GetModulationByName(name).Waveform.Code) + " -- " + str(bc.GetModulationByName(name).Waveform.Inverted))

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def measureParametersAndMakeBC(cluster, eState, bState, rfState, mwState, scramblerV):
	fileSystem = Environs.FileSystem
	
	bh.StopPattern()
	hc.UpdateRFPowerMonitor()
	hc.UpdateRFFrequencyMonitor()
	bh.StartPattern()
	hc.UpdateBCurrentMonitor()
	hc.UpdateVMonitor()
	hc.UpdateProbeAOMV()
	#hc.UpdatePumpAOMFreqMonitor()
	#hc.CheckPiMonitor()
	#print("Measuring polarizer angle")
	#hc.UpdateProbePolAngleMonitor()
	#hc.UpdatePumpPolAngleMonitor()
	#pumpPolAngle = hc.pumpPolAngle
	#probePolAngle = hc.probePolAngle

	# load a default BlockConfig and customise it appropriately
	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	bc = loadBlockConfig(settingsPath + "lfFreqStepTest05Nov19_try2.xml")
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
	bc.GetModulationByName("LF1").PhysicalCentre = hc.ProbeAOMFrequencyCentre;
	bc.GetModulationByName("LF1").PhysicalStep = hc.ProbeAOMFrequencyStep;
	# generate the waveform codes
	
	eWave = bc.GetModulationByName("E").Waveform
	eWave.Name = "E"
	##lf1Wave = bc.GetModulationByName("LF1").Waveform
	##lf1Wave.Name = "LF1"
	##mwWave = bc.GetModulationByName("MW").Waveform
	##mwWave.Name = "MW"
	ws = WaveformSetGenerator.GenerateWaveforms( (eWave,), ("B","DB","PI","RF1A","RF2A","RF1F","RF2F", "LF1") )
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
	
	bc.Settings["eRampDownTime"] = hc.ERampDownTime
	bc.Settings["eRampDownDelay"] = hc.ERampDownDelay
	bc.Settings["eBleedTime"] = hc.EBleedTime
	bc.Settings["eSwitchTime"] = hc.ESwitchTime
	bc.Settings["eRampUpTime"] = hc.ERampUpTime
	bc.Settings["eRampUpDelay"] = hc.ERampUpDelay
	# store the E switch asymmetry in the block
	bc.Settings["E0PlusBoost"] = hc.E0PlusBoost
	# number of times to step the target looking for a good target spot, step size is 2 (coded in Acquisitor)
	bc.Settings["maximumNumberOfTimesToStepTarget"] = 4000
	# minimum signal in the first detector, in Vus
	bc.Settings["minimumSignalToRun"] = 350.0
	bc.Settings["targetStepperGateStartTime"] = 2370.0
	bc.Settings["targetStepperGateEndTime"] = 2570.0
	return bc


def SMGo():
	fileSystem = Environs.FileSystem
	dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])

def SelectProfile(profileName):
	sm.SelectProfile(profileName)

def StartPattern():
	sm.OutputPattern()

def StopPattern():
	sm.StopPatternOutput()

def Acquire():
	count=0
	fileSystem = Environs.FileSystem
	file = \
		fileSystem.GetDataDirectory(\
					fileSystem.Paths["scanMasterDataPath"])\
		+ fileSystem.GenerateNextDataFileName()
	print("Saving as " + file + "_*.zip")
	print("")

	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	bc = loadBlockConfig(settingsPath + "bottomMixerV.xml")

	# start looping
	x = [0.1*i for i in range(20)]
	for j in range(4):
		for i in range(len(x)):
			sm.AdjustProfileParameter("out", "externalParameters", str(x[i]), False)
			for k in range(2):
				hc.UpdateBottomProbeMicrowaveMixerV(0.7)
				hc.SwitchMwAndWait(True)
				bh.StartPattern()
				print("Running Target Stepper ...")
				bh.StartTargetStepperAndWait()
				bh.StopPattern()
				print("Target Stepper finished")
				if bh.TargetHealthy == False:
					print("Unable to find acceptable spot")
					print("Stopping Cluster")
					hc.EnableEField( False )
					bh.StopPattern()
					break
				print("Running Block ...")
				hc.UpdateBottomProbeMicrowaveMixerV(x[i])
				hc.UpdateTopProbeMicrowaveMixerV(x[i])
				if (k<1):
					hc.SwitchMwAndWait(True)
				else:
					hc.SwitchMwAndWait(False)
				#print("MW state: " + str(hc.MwSwitchState) + ", Pi flip: " + str(hc.phaseFlip1State))
				print("bottom and top MixerV: " + str(x[i]) + "V, MW state: " + str(hc.MwSwitchState))
				count=count+1
				sm.AcquireAndWait(2)
				scanPath = file + "_" + str(count) + ".zip"
				sm.SaveAverageData(scanPath)
sm.AdjustProfileParameter("out", "externalParameters", "SidIsGreat", False)
		
	

		
	


def run_script():
	SMGo()
	SelectProfile("Scan B")
	Acquire()