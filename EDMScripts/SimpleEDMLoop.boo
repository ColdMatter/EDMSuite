# function definitions
def plotChannelGraph(clusterToPlot as String, kernel) as Form:
	form = Form()
	x = 600
	y = 690
	form.Size = Size(x, y)
	form.Text = "Live analysis"
	channelGraph = MathPictureBox(kernel);
	channelGraph.Size = Size(x, y - 30)
	channelGraph.UseFrontEnd = false
	channelGraph.PictureType = "Automatic"
	channelGraph.MathCommand = "plotDiagnostics[\"${clusterToPlot}\"]"
	form.Controls.Add(channelGraph)
	Thread({Application.Run(form)}).Start()
	return form

def updateChannelGraph(form as Form):
	if form.Controls.Count > 0:
		mpb as MathPictureBox = form.Controls[0]
		mpb.Recompute()

def analyseBlock(path as string, kernel):
	mmedPath = path.Replace("\\","\\\\")
	kernel.Evaluate("addBlockToDB[\"${mmedPath}\"]")
	kernel.WaitAndDiscardAnswer();

def checkYAGAndFix():
	interlockFailed = remote.HardwareControl.YAGInterlockFailed;
	if (interlockFailed):
		remote.BlockHead.StopPattern();
		remote.BlockHead.StartPattern();	

def EDMGo():
	# Setup
	fileSystem = Environs.FileSystem
	dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
	print("Data directory is : " + dataPath)
	print("")

	# User inputs data
	cluster = prompt("Cluster name: ")
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
	MathematicaService.LoadPackage("SEDM`Database`", false)
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
	while blockIndex < 60:
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
		checkYAGAndFix()
		++blockIndex

	remote.BlockHead.StopPattern()
