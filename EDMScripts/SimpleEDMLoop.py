# Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *

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

def EDMGoReal(nullRun):
	# Setup
	f = None
	fileSystem = Environs.FileSystem
	dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
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
	gVoltage = Double.Parse(prompt("G-voltage (kV): "))
	plusCVoltage = Double.Parse(prompt("+ve C-voltage (kV): "))
	minusCVoltage = Double.Parse(prompt("-ve C-voltage (kV, -ve number !): "))
	biasCurrent = Double.Parse(prompt("Bias current (uA): "))

	# load a default BlockConfig and customise it appropriately
	settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
	bc = loadBlockConfig(settingsPath + "default.xml")
	bc.Settings["cluster"] = cluster
	bc.Settings["eState"] = eState
	bc.Settings["bState"] = bState
	bc.Settings["ePlus"] = plusCVoltage
	bc.Settings["eMinus"] = minusCVoltage
	bc.Settings["gtPlus"] = gVoltage
	bc.Settings["gbPlus"] = gVoltage
	bc.Settings["gtMinus"] = -gVoltage
	bc.Settings["gbMinus"] = -gVoltage
	bc.GetModulationByName("B").Centre = biasCurrent

	# loop and take data
	bh.StartPattern()
	blockIndex = 0
	if nullRun:
		maxBlockIndex = 10000
	else:
		maxBlockIndex = 60
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
		# if not nullRun:
		checkYAGAndFix()
		blockIndex = blockIndex + 1
	bh.StopPattern()


def EDMGoNull():
	EDMGoReal(True)

def EDMGo():
	EDMGoReal(False)

def run_script():
	EDMGo()

