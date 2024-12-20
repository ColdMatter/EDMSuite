# Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *
from System import Type

from Analysis.EDM import *
from DAQ.Environment import *
from EDMConfig import *
from uedmfuncs import *

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

def checkPhaseLock():
	while (abs(pl.PhaseError) > 5):
		input("Check Phase Lock is running (hit enter if you are happy.)")
	print("Phase Lock checked.")

def printWaveformCode(bc, name):
	print(name + ": " + str(list(bc.GetModulationByName(name).Waveform.Code)) + " -- " + str(bc.GetModulationByName(name).Waveform.Inverted))

def measureParametersAndMakeBC(cluster, eState, bState, mwState):
	fileSystem = Environs.FileSystem
	print("Measuring parameters ...")
	bh.StopPattern()
	# hc.UpdateBCurrentMonitor()
	hc.PollVMonitor()
	bh.StartPattern()

	print("V plus: " + str(hc.CPlusMonitorVoltage * hc.CPlusMonitorScale))
	print("V minus: " + str(hc.CMinusMonitorVoltage * hc.CMinusMonitorScale))
	print("Bias: " + str(hc.BiasCurrent))
	print("B step: " + str(abs(hc.FlipStepCurrent)))
	print("DB step: " + str(abs(hc.CalStepCurrent)))
	print("Phase Lock Error (deg): "+ str(pl.PhaseError))

	# load a default BlockConfig and customise it appropriately
	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	bc = loadBlockConfig(settingsPath + "default.xml")

	bc.Settings["cluster"] = str(cluster)
	bc.Settings["eState"] = eState
	bc.Settings["bState"] = bState
	bc.Settings["mwState"] = mwState
	
	bc.Settings["ePlus"] = hc.CPlusMonitorVoltage * hc.CPlusMonitorScale
	bc.Settings["eMinus"] = hc.CMinusMonitorVoltage * hc.CMinusMonitorScale
	bc.Settings["bBiasV"] = hc.SteppingBiasVoltage
	bc.Settings["greenDCFM"] = 0.0#hc.GreenSynthDCFM
	bc.Settings["greenAmp"] = hc.GreenSynthOnAmplitude
	bc.Settings["greenFreq"] = hc.GreenSynthOnFrequency
	
	bc.GetModulationByName("B").Centre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").Step = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").Step = abs(hc.CalStepCurrent)/1000
	# these next 3, seemingly redundant, lines are to preserve backward compatibility
	bc.GetModulationByName("B").PhysicalCentre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").PhysicalStep = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").PhysicalStep = abs(hc.CalStepCurrent)/1000
	
	# generate the waveform codes
	print("Generating waveform codes ...")
	eWave = bc.GetModulationByName("E").Waveform
	eWave.Name = str("E")
	ws = WaveformSetGenerator.GenerateWaveforms( (eWave,), ("B","DB"))#,"PI","RF1A","RF2A","RF1F","RF2F","LF1") )
	bc.GetModulationByName("B").Waveform = ws["B"]
	bc.GetModulationByName("DB").Waveform = ws["DB"]
	
	# change the inversions of the static codes E
	bc.GetModulationByName("E").Waveform.Inverted = WaveformSetGenerator.RandomBool()

	printWaveformCode(bc, "E")
	printWaveformCode(bc, "B")
	printWaveformCode(bc, "DB")
	
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
	bc.Settings["maximumNumberOfTimesToStepTarget"] = System.Int32(100)
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
#r = Random()

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
	cluster = input("Cluster name [" + suggestedClusterName +"]: \n")
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
	# rfState = True #hc.RFManualState
	# print("rf-state: " + str(rfState))
	mwState = True #hc.MWManualState
	print("mw-state: " + str(mwState))
	# this is to make sure the B current monitor is in a sensible state
	# hc.UpdateBCurrentMonitor()

	# randomise Ramsey phase 
	# scramblerV = 0.97156 * r.NextDouble()
	#hc.SetScramblerVoltage(scramblerV)

	# calibrate leakage monitors
	print("calibrating leakage monitors..")
	print("Is E-field off yet?")
	# hc.EnableGreenSynth( False )
	hc.FieldsOff()
	hc.PollVMonitor()
	if(hc.CPlusMonitorVoltage * hc.CPlusMonitorScale)>100.0:
		print("Waiting")
		System.Threading.Thread.CurrentThread.Join(60000)
	else:
		print("E-Field Off")
	# hc.EnableBleed( True )
	# System.Threading.Thread.CurrentThread.Join(5000)
	hc.CalibrateIMonitors()
	# hc.EnableBleed( False )
	# System.Threading.Thread.CurrentThread.Join(500)
	hc.SetCPlusVoltage(cPlusV)
	hc.SetCMinusVoltage(cMinusV)
	print("E Params refreshed")
	System.Threading.Thread.CurrentThread.Join(5000)
	print("E-field on")
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(10000)
	# hc.EnableEField( True )
	# hc.EnableGreenSynth( True )
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

		print("Acquiring MAGNETIC FIELD block " + str(blockIndex) + " ...")
		# save the block config and load into blockhead
		print("Saving temp config.")
		bc.Settings["clusterIndex"] = System.Int32(blockIndex)
		tempConfigFile =str('%(p)stemp%(c)s_%(i)s.xml' % {'p': settingsPath, 'c': cluster, 'i': blockIndex})
		saveBlockConfig(tempConfigFile, bc)
		System.Threading.Thread.CurrentThread.Join(500)
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
		# print("Notifying Mathematica and waiting ...")
		writeLatestBlockNotificationFile(cluster, blockIndex)
		System.Threading.Thread.CurrentThread.Join(5000)
		print("Done.")
		# increment and loop
		File.Delete(tempConfigFile)
		
		blockIndex = blockIndex + 1
		#updateLocks(bState)
		# randomise Ramsey phase
		#scramblerV = 0.97156 * r.NextDouble()
		# hc.SetScramblerVoltage(scramblerV)

		bc = measureParametersAndMakeBC(cluster, eState, bState, mwState)#rfState, mwState, scramblerV)

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
			hc.SetCPlusVoltage(cPlusV)
			hc.SetCMinusVoltage(cMinusV)
			hc.SwitchEAndWait(eCurrentState)
			print("E Switch Finished")
			System.Threading.Thread.CurrentThread.Join(10000)

			print("leakage monitors calibrated")

	bh.StopPattern()


def main():
	QuSpinGo()
	pass

if __name__=="__main__":
	main()
