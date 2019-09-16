# Switch between four states (INT PHASE, MW DET B)
# INT PHASE can be 0 (TRUE) or pi/2 (FALSE)
# MW DET B can be on (TRUE) or off (FALSE)

from DAQ.Environment import *

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def go(intPhaseZeroVoltage, intPhasePiBy2Voltage, mwDetBOnLength, numRepeat):
	# prompt to enter det A microwave powers
	mwpowers_input = prompt("Enter range of microwave mixer voltages for det A in V: ")
	mwpowers = mwpowers_input.split(",")

	for j in range(len(mwpowers)):
		# setup det A microwaves
		print "Microwaves in det A set to mixer voltage of " + mwpowers[j] + "V."
		hc.UpdateBottomProbeMicrowaveMixerV(float(mwpowers[j]))
		
		# setup file saving
		fileSystem = Environs.FileSystem
		file = \
			fileSystem.GetDataDirectory(\
						fileSystem.Paths["scanMasterDataPath"])\
			+ fileSystem.GenerateNextDataFileName()
		print("Saving as " + file + "_*.zip")
		print("")

		for k in range(numRepeat):
			print "Iteration " + str(k+1) + " of " + str(numRepeat) + "."
			# choose state, then scan
			for i in range(4):
				[intPhaseState, mwDetBState] = [bool(int(x)) for x in list("{0:02b}".format(i))]
				setState(intPhaseState, mwDetBState, intPhaseZeroVoltage, intPhasePiBy2Voltage, mwDetBOnLength, 0)
				sm.AcquireAndWait(2)
				scanPath = file + "_" + str(k) + "_" + str(i) + ".zip"
				sm.SaveAverageData(scanPath)

def setState(intPhaseState, mwDetBState, intPhaseTrue, intPhaseFalse, mwDetBTrue, mwDetBFalse):
	# set hardware to reflect state
	if intPhaseState:
		hc.SetScanningBVoltage(intPhaseTrue)
		print "Setting voltage on scanning B box to " + str(intPhaseTrue) + " V."
	else:
		hc.SetScanningBVoltage(intPhaseFalse)
		print "Setting voltage on scanning B box to " + str(intPhaseFalse) + " V."

	if mwDetBState:
		sm.AdjustProfileParameter("pg", "topProbemwLength", str(mwDetBTrue), False)
		print "Setting microwave pulse length in top detector to " + str(mwDetBTrue) + " us."
	else:
		sm.AdjustProfileParameter("pg", "topProbemwLength", str(mwDetBFalse), False)
		print "Setting microwave pulse length in top detector to " + str(mwDetBFalse) + " us."

def run_script():
	print "go(intPhaseZeroVoltage, intPhasePiBy2Voltage, mwDetBOnLength, numRepeat)"

