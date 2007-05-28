# Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *

from Wolfram.NETLink.UI import *

from DAQ.Environment import *
from DAQ.Mathematica import *
from EDMConfig import *


# function definitions
# define functions that might be generally useful here

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

def plotChannelGraph(clusterToPlot, kernel):
	form = Form()
	x = 1000
	y = 1000
	form.Size = Size(x, y)
	form.Text = "Live analysis"
	channelGraph = MathPictureBox(kernel);
	channelGraph.Size = Size(x, y - 30)
	channelGraph.UseFrontEnd = False
	channelGraph.PictureType = "Automatic"
	channelGraph.MathCommand = "plotLiveDiagnostics[\"%(c)s\"]" % {"c": clusterToPlot}
	form.Controls.Add(channelGraph)
	Thread(ThreadStart(Application.Run(form))).Start()
	return form

def updateChannelGraph(form):
	if form.Controls.Count > 0:
		mpb = form.Controls[0]
		mpb.Recompute()

def analyseBlock(path, kernel):
	mmedPath = path.Replace("\\","\\\\")
	kernel.Evaluate("addFileToDatabase[\"%(m)s\",extractFunc,viewSpecs]" % {"m": mmedPath})
	kernel.WaitAndDiscardAnswer();
	#kernel.Evaluate("saveDatabase[\"running_temp\"]")
	#kernel.WaitAndDiscardAnswer();

def checkYAGAndFix():
	interlockFailed = hc.YAGInterlockFailed;
	if (interlockFailed):
		bh.StopPattern();
		bh.StartPattern();	

def initialiseMathematica(kernel):
	#MathematicaService.LoadPackage("SEDM2`Database`", False)
	kernel.Evaluate("Needs[\"SEDM2`Database`\"];Needs[\"SEDM2`SharedCode`\"];Needs[\"SEDM2`Graphics`\"]")
	kernel.WaitAndDiscardAnswer();
	kernel.Evaluate("initialiseSharedCode[]")
	kernel.WaitAndDiscardAnswer();
	kernel.Evaluate("createBlockSerializer[]")
	kernel.WaitAndDiscardAnswer();
	kernel.Evaluate("$allowDBReplace = False;")
	kernel.WaitAndDiscardAnswer();
	kernel.Evaluate("dbName = \"running_temp\";loadDatabase[dbName];")
	kernel.WaitAndDiscardAnswer();
	kernel.Evaluate("gates={{0,10^6},{0,10^6},{0,10^6}};extractFunc = {integrateTOF[#, 0, gates[[1]][[1]], gates[[1]][[2]], True, \"pmt\"], integrateTOF[#, 1, gates[[2]][[1]], gates[[2]][[2]], False, \"mag1\"]} &;")
	kernel.WaitAndDiscardAnswer();
	kernel.Evaluate("viewSpecs = {};")
	kernel.WaitAndDiscardAnswer();

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
	prompt("Have you closed the last Mathematica ? [return]")

	# establish a Mathematica kernel link
	kernel = MathematicaService.GetKernel()
	initialiseMathematica(kernel)

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
		# hand the block off to mathematica for analysis
		print("Analysing block "+ str(blockIndex) + "...")
		analyseBlock(blockPath, kernel)
		print("Done.")
		# display some diagnostic information
		if f == None:
			f = plotChannelGraph(cluster, kernel)
		else:
			updateChannelGraph(f)
		# increment and loop
		File.Delete(tempConfigFile)
		# if not nullRun:
		checkYAGAndFix()
		++blockIndex
	bh.StopPattern()


def EDMGoNull():
	EDMGoReal(True)

def EDMGo():
	EDMGoReal(False)

