# loading and saving configurations
def saveBlockConfig(path as string, config as BlockConfig):
	fs = FileStream(path, FileMode.Create)
	s = XmlSerializer(BlockConfig)
	s.Serialize(fs,config)
	fs.Close()

def loadBlockConfig(path as string) as BlockConfig:
	fs = FileStream(path, FileMode.Open)
	s = XmlSerializer(BlockConfig)
	bc = s.Deserialize(fs)
	fs.Close()
	return bc

def plotChannelGraph(clusterToPlot as String, kernel) as Form:
	form = Form()
	x = 600
	y = 600
	form.Size = Size(x, y)
	form.Text = "Live analysis"
	channelGraph = MathPictureBox(kernel);
	channelGraph.Size = Size(x, y - 30)
	channelGraph.UseFrontEnd = false
	channelGraph.PictureType = "Automatic"
	channelGraph.MathCommand = "plotLiveDiagnostics[\"${clusterToPlot}\"]"
	form.Controls.Add(channelGraph)
	Thread({Application.Run(form)}).Start()
	return form

def updateChannelGraph(form as Form):
	if form.Controls.Count > 0:
		mpb as MathPictureBox = form.Controls[0]
		mpb.Recompute()

def analyseBlock(path as string, kernel):
	mmedPath = path.Replace("\\","\\\\")
	kernel.Evaluate("addFileToDatabase[\"${mmedPath}\",extractFunc,viewSpecs]")
	kernel.WaitAndDiscardAnswer();
	#kernel.Evaluate("saveDatabase[\"running_temp\"]")
	#kernel.WaitAndDiscardAnswer();

def checkYAGAndFix():
	interlockFailed = remote.HardwareControl.YAGInterlockFailed;
	if (interlockFailed):
		remote.BlockHead.StopPattern();
		remote.BlockHead.StartPattern();	

def initialiseMathematica(kernel):
	#MathematicaService.LoadPackage("SEDM2`Database`", false)
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
	kernel.Evaluate("gates = {{0, 10^6}, {0, 10^6}};rf1KeepLength=0.02;rf2KeepLength=0.03;offset=0;extractFunc={integrateTOF[#,0,First[generatePulsedRFGates[#,rf1KeepLength,rf2KeepLength,offset]],Last[generatePulsedRFGates[#,rf1KeepLength,rf2KeepLength,offset]],True,\"pmt\"](*,integrateTOF[#,1,gates\\[LeftDoubleBracket]2\\[RightDoubleBracket]\\[LeftDoubleBracket]1\\[RightDoubleBracket],gates\\[LeftDoubleBracket]2\\[RightDoubleBracket]\\[LeftDoubleBracket]2\\[RightDoubleBracket],False,\"mag1\"]*)}&;")
	kernel.WaitAndDiscardAnswer();
	kernel.Evaluate("viewSpecs = {};")
	kernel.WaitAndDiscardAnswer();

def EDMGoReal(nullRun):
	# Setup
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
	cast(AnalogModulation,bc.GetModulationByName("B")).Centre = biasCurrent
	prompt("Have you closed the last Mathematica ? [return]")

	# establish a Mathematica kernel link
	kernel = MathematicaService.GetKernel()
	initialiseMathematica(kernel)
	f as Form

	# scan Raman lineshapes ?
#	raman = prompt("Scan Raman lineshapes [y/n] : ")
#	if raman == "y":
#		# upper Raman
#		remote.ScanMaster.SelectProfile("Scan green frequency UR")
#		remote.ScanMaster.AcquireAndWait(1)
#		remote.ScanMaster.SaveAverageData("${dataPath}${cluster}_UR.zip")
#		# lower Raman
#		remote.ScanMaster.SelectProfile("Scan green frequency LR")
#		remote.ScanMaster.AcquireAndWait(1)
#		remote.ScanMaster.SaveAverageData("${dataPath}${cluster}_LR.zip")

	# loop and take data
	remote.BlockHead.StartPattern()
	blockIndex = 0
	if nullRun:
		maxBlockIndex = 10000
	else:
		maxBlockIndex = 60
	while blockIndex < maxBlockIndex:
		print("Acquiring block ${blockIndex} ...")
		# save the block config and load into blockhead
		print("Saving temp config.")
		bc.Settings["clusterIndex"] = blockIndex
		tempConfigFile as string = "${settingsPath}temp${cluster}_${blockIndex}.xml"
		saveBlockConfig(tempConfigFile, bc)
		System.Threading.Thread.Sleep(500)
		print("Loading temp config.")
		remote.BlockHead.LoadConfig(tempConfigFile)
		# take the block and save it
		print("Running ...")
		remote.BlockHead.AcquireAndWait()
		print("Done.")
		blockPath as string = "${dataPath}${cluster}_${blockIndex}.zip"
		remote.BlockHead.SaveBlock(blockPath)
		print("Saved block ${blockIndex}.")
		# hand the block off to mathematica for analysis
		print("Analysing block ${blockIndex}")
		analyseBlock(blockPath, kernel)
		print("Done.")
		# display some diagnostic information
		if f == null:
			f = plotChannelGraph(cluster, kernel)
		else:
			updateChannelGraph(f)
		# increment and loop
		File.Delete(tempConfigFile)
		# if not nullRun:
		checkYAGAndFix()
		++blockIndex
	remote.BlockHead.StopPattern()


def EDMGoNull():
	EDMGoReal(true)

def EDMGo():
	EDMGoReal(false)

