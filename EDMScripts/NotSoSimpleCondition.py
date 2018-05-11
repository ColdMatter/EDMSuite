from System.Threading import *
import math

def condition(switchTime, pollTime):
	switchCount = 1
	numSamples = int(switchTime * 1000.0 / pollTime)
	nCurrentSamples = []
	sCurrentSamples = []
	#loop forever
	while switchCount > 0:
		hc.SwitchEAndWait()
		print "Switch count "+str(switchCount)
		for x in range(1, numSamples):
			System.Threading.Thread.Sleep(pollTime)
			nCurrentSamples.append(hc.LastNorthCurrent)
			sCurrentSamples.append(hc.LastSouthCurrent)
		nAvCurrent = sum(nCurrentSamples) / float(len(nCurrentSamples))
		sAvCurrent = sum(sCurrentSamples) / float(len(sCurrentSamples))
		print "Average leakage current in N plate is " + str(nAvCurrent) + "nA."
		print "Average leakage current in S plate is " + str(sAvCurrent) + "nA."
		if math.fabs(nAvCurrent) > 20:
			print "Average leakage current in N plate is too high! Aborting conditioning..."
			break
		elif math.fabs(sAvCurrent) > 20:
			print "Average leakage current in S plate is too high! Aborting conditioning..."
			break
		switchCount = switchCount + 1
		del nCurrentSamples[:]
		del sCurrentSamples[:]
	hc.EnableEField(False)

	
def run_script():
	print "Use condition(switchTime in seconds, pollTime in ms)"


