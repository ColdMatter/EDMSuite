# function definitions

def Conditioner():
	# Setup (none)
	# User inputs data
	switchTime = 1000*Double.Parse(prompt("Switch time (s): "))
	# loop and switch
	blockIndex = 1
		#loop forever
	while blockIndex > 0:
		print("Switching E  ${blockIndex} ")
		remote.HardwareControl.SwitchE()
		System.Threading.Thread.Sleep(switchTime)
		++blockIndex

	
