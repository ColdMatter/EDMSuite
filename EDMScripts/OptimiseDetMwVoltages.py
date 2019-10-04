# Switch between four states (INT PHASE, MW DET B)
# INT PHASE can be 0 (TRUE) or pi/2 (FALSE)
# MW DET B can be on (TRUE) or off (FALSE)

from DAQ.Environment import *

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def go(intPhaseZeroVoltage, intPhasePiBy2Voltage):
	# prompt to enter det A microwave powers
	mwpowers_input = prompt("Enter range of microwave mixer voltages in V: ")
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
		print("Saving as " + file + "_*_*.zip")
		print("")

		for k in range(len(mwpowers)):
			# setup det B microwaves
			print "Microwaves in det B set to mixer voltage of " + mwpowers[k] + "V."
			hc.UpdateTopProbeMicrowaveMixerV(float(mwpowers[k]))
			# choose state, then scan
			for i in range(4):
				[intPhaseState, mwDetState] = [bool(int(x)) for x in list("{0:02b}".format(i))]
				setState(intPhaseState, mwDetState, intPhaseZeroVoltage, intPhasePiBy2Voltage)
				sm.AcquireAndWait(1)
				scanPath = file + "_" + str(k) + "_" + str(i) + ".zip"
				sm.SaveAverageData(scanPath)

def setState(intPhaseState, mwDetState, intPhaseTrue, intPhaseFalse):
	# set hardware to reflect state
	if intPhaseState:
		hc.SetScanningBVoltage(intPhaseTrue)
		print "Setting voltage on scanning B box to " + str(intPhaseTrue) + " V."
	else:
		hc.SetScanningBVoltage(intPhaseFalse)
		print "Setting voltage on scanning B box to " + str(intPhaseFalse) + " V."

	if mwDetState:
		hc.SwitchMwAndWait(True)
		print "Setting microwaves to F=0/F=1 in bottom/top detector."
	else:
		hc.SwitchMwAndWait(False)
		print "Setting microwaves to F=1/F=0 in bottom/top detector."

def run_script():
	print "go(intPhaseZeroVoltage, intPhasePiBy2Voltage)"

