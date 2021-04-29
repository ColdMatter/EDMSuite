from System.Threading import *
import math

def condition(switchTime, pollTime):
	switchCount = 1
	numSamples = int(switchTime * 1000.0 / pollTime)
	nCurrentSamples = []
	sCurrentSamples = []
	countList = []
	#loop forever
	while switchCount > 0:
		hc.SwitchEAndWait()
		print "Switch count "+str(switchCount)
		count = 0
		for x in range(1, numSamples):
			System.Threading.Thread.Sleep(pollTime)
			nCurrentSamples.append(hc.LastNorthCurrent)
			sCurrentSamples.append(hc.LastSouthCurrent)
			if math.fabs(hc.LastNorthCurrent) > 20 or math.fabs(hc.LastSouthCurrent) > 20:
				count = count + 1
		nAvCurrent = sum(nCurrentSamples) / float(len(nCurrentSamples))
		sAvCurrent = sum(sCurrentSamples) / float(len(sCurrentSamples))
		print "Average leakage current in N plate is " + str(nAvCurrent) + "nA."
		print "Average leakage current in S plate is " + str(sAvCurrent) + "nA."
		print "Number of spikes (above 20nA): " + str(count)
		countList.append(count)
		if (len(countList) == 21):
			del countList[0]
		print "Spike counts for the last 20 switches " + str(countList).strip('[]')
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
	hc.EnableGreenSynth(False)
	return

	
def run_script():
	print "Use condition(switchTime in seconds, pollTime in ms)"


