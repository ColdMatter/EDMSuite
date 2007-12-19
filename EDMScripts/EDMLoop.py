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

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def measureParametersAndMakeBC(cluster, eState, bState):
	fileSystem = Environs.FileSystem
	hc.UpdateBCurrentMonitor()
	hc.UpdateBCurrentMonitor()
	hc.UpdateVMonitor()
	# load a default BlockConfig and customise it appropriately
	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	bc = loadBlockConfig(settingsPath + "default.xml")
	bc.Settings["cluster"] = cluster
	bc.Settings["eState"] = eState
	bc.Settings["bState"] = bState
	bc.Settings["ePlus"] = hc.CPlusMonitorVoltage * hc.CPlusMonitorScale
	bc.Settings["eMinus"] = hc.CMinusMonitorVoltage * hc.CMinusMonitorScale
	bc.GetModulationByName("B").Centre = (hc.BiasCurrent)/1000
	bc.GetModulationByName("B").Step = abs(hc.FlipStepCurrent)/1000
	bc.GetModulationByName("DB").Step = abs(hc.CalStepCurrent)/1000
	print("V plus: " + str(hc.CPlusMonitorVoltage * hc.CPlusMonitorScale))
	print("V minus: " + str(hc.CMinusMonitorVoltage * hc.CMinusMonitorScale))
	print("Bias: " + str(hc.BiasCurrent))
	print("B step: " + str(abs(hc.FlipStepCurrent)))
	print("DB step: " + str(abs(hc.CalStepCurrent)))
	print("Setting rf parameters ...")
	bc.GetModulationByName("RF1A").Centre = hc.RF1AttCentre
	bc.GetModulationByName("RF1A").Step = hc.RF1AttStep
	bc.GetModulationByName("RF2A").Centre = hc.RF2AttCentre
	bc.GetModulationByName("RF2A").Step = hc.RF2AttStep
	bc.GetModulationByName("RF1F").Centre = hc.RF1FMCentre
	bc.GetModulationByName("RF1F").Step = hc.RF1FMStep
	bc.GetModulationByName("RF2F").Centre = hc.RF2FMCentre
	bc.GetModulationByName("RF2F").Step = hc.RF2FMStep
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
kSteppingBiasCurrentPerVolt = 100.0
# max change in the b-bias voltage per block
kBMaxChange = 0.1
# volts of rf*a input required per cal's worth of offset
kRFAVoltsPerCal = 0.3
kRFAMaxChange = 0.05

def updateLocks(bState):
	pmtChannelValues = bh.DBlock.ChannelValues[0]
	# note the weird python syntax for a one element list
	bIndex = pmtChannelValues.GetChannelIndex(("B",))
	bValue = pmtChannelValues.GetValue(bIndex)
	bError = pmtChannelValues.GetError(bIndex)
	dbIndex = pmtChannelValues.GetChannelIndex(("DB",))
	dbValue = pmtChannelValues.GetValue(dbIndex)
	dbError = pmtChannelValues.GetError(dbIndex)
	rf1aIndex = pmtChannelValues.GetChannelIndex(("RF1A",))
	rf1aValue = pmtChannelValues.GetValue(rf1aIndex)
	rf1aError = pmtChannelValues.GetError(rf1aIndex)
	rf2aIndex = pmtChannelValues.GetChannelIndex(("RF2A",))
	rf2aValue = pmtChannelValues.GetValue(rf2aIndex)
	rf2aError = pmtChannelValues.GetError(rf2aIndex)
	rf1fIndex = pmtChannelValues.GetChannelIndex(("RF1F",))
	rf1fValue = pmtChannelValues.GetValue(rf1fIndex)
	rf1fError = pmtChannelValues.GetError(rf1fIndex)
	rf2fIndex = pmtChannelValues.GetChannelIndex(("RF2F",))
	rf2fValue = pmtChannelValues.GetValue(rf2fIndex)
	rf2fError = pmtChannelValues.GetError(rf2fIndex)
	print "B: " + str(bValue) + " DB: " + str(dbValue)
	print "RF1A: " + str(rf1aValue) + " RF2A: " + str(rf2aValue)
	print "RF1F: " + str(rf1fValue) + " RF2F: " + str(rf2fValue)
	# B bias lock
	# feedback only 1/5 of what we think we should - very loose
	# the sign of the feedback depends on the b-state
	if bState: 
		feedbackSign = 1
	else: 
		feedbackSign = -1
	deltaBias = (1.0/5.0) * (hc.CalStepCurrent * (bValue / dbValue)) / kSteppingBiasCurrentPerVolt
	deltaBias = windowValue(deltaBias, -kBMaxChange, kBMaxChange)
	print "Attempting to change stepping B bias by " + str(deltaBias) + " V."
	newBiasVoltage = windowValue( hc.SteppingBiasVoltage - deltaBias, 0, 5)
	hc.SetSteppingBBiasBVoltage( newBiasVoltage )
	# RFA  locks
	deltaRF1A = (1.0/5.0) * (rf1aValue / dbValue) * kRFAVoltsPerCal
	deltaRF1A = windowValue(deltaBias, -kRFAMaxChange, kRFAMaxChange)
	print "Attempting to change RF1A by " + str(deltaRF1A) + " V."
	newRF1A = windowValue( hc.RF1AttCentre - deltaRF1A, 0, 5)
	hc.SetRF1AttCentre( newRF1A )
	#
	deltaRF2A = (1.0/5.0) * (rf2aValue / dbValue) * kRFAVoltsPerCal
	deltaRF2A = windowValue(deltaBias, -kRFAMaxChange, kRFAMaxChange)
	print "Attempting to change RF2A by " + str(deltaRF2A) + " V."
	newRF2A = windowValue( hc.RF2AttCentre - deltaRF2A, 0, 5)
	hc.SetRF2AttCentre( newRF2A )


def windowValue(value, minValue, maxValue):
	if ( (value < maxValue) & (value > minValue) ):
		return value
	else:
		if (value < maxValue):
			return maxValue
		else:
			return minValue

def EDMGoReal(nullRun):
	# Setup
	f = None
	fileSystem = Environs.FileSystem
	dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	print("Data directory is : " + dataPath)
	print("")
	suggestedClusterName = fileSystem.GenerateNextDataFileName()

	# User inputs data
	cluster = prompt("Cluster name [" + suggestedClusterName +"]: ")
	if cluster == "":
		cluster = suggestedClusterName
		print("Using cluster " + suggestedClusterName)
	eState = Boolean.Parse(prompt("E-state: "))
	bState = Boolean.Parse(prompt("B-state: "))

	bc = measureParametersAndMakeBC(cluster, eState, bState)

	# loop and take data
	bh.StartPattern()
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
		bc = measureParametersAndMakeBC(cluster, eState, bState)
	bh.StopPattern()


def EDMGoNull():
	EDMGoReal(True)

def EDMGo():
	EDMGoReal(False)

def run_script():
	EDMGo()

