# This loop monitors the leakage current and saves in a csv for a 
# ramp up of the potential up to a given value. If breakdown is detected
# it reduces the e field voltage waits again. If nothing is detected and 
# the leakage is low it ups the voltage and continues

from DAQ.Environment import *
from uedmfuncs import *

def switchcondition(switchTime, pollTime):
	switchCount = 1
	numSamples = int(switchTime * 1000.0 / pollTime)
	nCurrentSamples = []
	sCurrentSamples = []
	countList = []
	#loop forever
	while switchCount > 0:
		hc.SwitchEAndWait()
		print("Switch count "+str(switchCount))
		count = 0
		for x in range(1, numSamples):
			System.Threading.Thread.Sleep(pollTime)
			nCurrentSamples.append(hc.LastNorthCurrent)
			sCurrentSamples.append(hc.LastSouthCurrent)
			if ((math.fabs(hc.LastNorthCurrent) > 20) or (math.fabs(hc.LastSouthCurrent) > 20)):
				count = count + 1
		nAvCurrent = sum(nCurrentSamples) / float(len(nCurrentSamples))
		sAvCurrent = sum(sCurrentSamples) / float(len(sCurrentSamples))
		print("Average leakage current in N plate is " + str(nAvCurrent) + "nA.")
		print("Average leakage current in S plate is " + str(sAvCurrent) + "nA.")
		print("Number of spikes (above 20nA): " + str(count))
		countList.append(count)
		if (len(countList) == 21):
			del countList[0]
		print("Spike counts for the last 20 switches " + str(countList).strip('[]'))
		if math.fabs(nAvCurrent) > 20:
			print("Average leakage current in N plate is too high! Aborting conditioning...")
			break
		elif math.fabs(sAvCurrent) > 20:
			print("Average leakage current in S plate is too high! Aborting conditioning...")
			break
		switchCount = switchCount + 1
		del nCurrentSamples[:]
		del sCurrentSamples[:]
	hc.EnableEField(False)
	return

def fieldcondition(waitTime, stepSize, polarity):
	currentVoltage = 0.0
	hc.EFieldPolarity = polarity
	hc.EnableEField(True)
	print("E-field is switched on in polarity " + str(polarity))
	System.Threading.Thread.Sleep(10000)
	while currentVoltage < 10:
		currentVoltage = currentVoltage + stepSize
		print("Incrementing voltage to " + str(currentVoltage) + " kV.")
		hc.SetCPlusVoltage(currentVoltage)
		hc.SetCMinusVoltage(currentVoltage)
		System.Threading.Thread.Sleep(waitTime * 1000)

def smartCondition(waitTime, pollTime, startV, stepsize, maxV, currLimit = 20, spikeLim=10):
	hc.FieldsOff()
	plateV=startV
	numSamples = 10
	samplesPerWait = int(np.floor(waitTime/((pollTime*numSamples)/1000)))
	westCurrSamples = np.empty(numSamples)
	eastCurrSamples = np.empty(numSamples)
	print(f"Starting plate voltage at +- {startV} kV")
	print(f"If there are more than {spikeLim} spikes, or the current averages \n above {currLimit} nA, the program will lower the voltage.")
	hc.SetCPlusVoltage(plateV)
	hc.SetCMinusVoltage(plateV)
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(5000)
	print(f"Now ramping plate voltage up in step sizes of {stepsize} kV")
	while plateV < maxV:
		count = 0
		for sampling in range(samplesPerWait):
			upvolt = True
			for x in range(numSamples):
				System.Threading.Thread.CurrentThread.Join(pollTime)
				westCurrSamples[x] = hc.LastWestCurrent
				eastCurrSamples[x] = hc.LastEastCurrent
				if ((np.abs(westCurrSamples[x])>currLimit) or (np.abs(eastCurrSamples[x])>currLimit)):
					count+=1
			print(f"spikes so far at this voltage: {count}",end='\r')
			if ((np.abs(westCurrSamples.mean())>currLimit) or (np.abs(eastCurrSamples.mean())>currLimit) or (count > spikeLim)):
				plateV-=1.0
				if plateV<=0:
					plateV=0.0
				print(f"spikes so far at this voltage: {count}",end='\n')
				print(f"Current too high, dropping the voltage to {plateV} kV")
				upvolt = False
				hc.SetCPlusVoltage(plateV)
				hc.SetCMinusVoltage(plateV)
				System.Threading.Thread.CurrentThread.Join(5000)
				break
		if upvolt:
			plateV+=stepsize
			plateV=np.round(plateV,2)
			print(f"spikes so far at this voltage: {count}",end='\n')
			print(f"Upping the voltage to {plateV} kV.")
			System.Threading.Thread.CurrentThread.Join(5000)
	hc.FieldsOff()
	plateV=startV
	numSamples = 10
	samplesPerWait = np.floor(waitTime/((pollTime*numSamples)/1000))
	westCurrSamples = np.empty(numSamples)
	eastCurrSamples = np.empty(numSamples)
	print(f"Starting plate voltage at +- {startV} kV")
	hc.SetCPlusVoltage(plateV)
	hc.SetCMinusVoltage(plateV)
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(20000)
	print(f"Now ramping plate voltage up in step sizes of {stepsize} kV")
	while plateV < maxV:
		count = 0
		for sampling in range(samplesPerWait):
			for x in range(numSamples):
				System.Threading.Thread.CurrentThread.Join(pollTime)
				westCurrSamples[x] = hc.LastWestCurrent
				eastCurrSamples[x] = hc.LastEastCurrent
				if ((np.abs(westCurrSamples[x])>currLimit) or (np.abs(eastCurrSamples[x])>currLimit)):
					count+=1
			print(f"spikes so far at this voltage: {count}")
			if ((np.abs(westCurrSamples.mean())>currLimit) or (np.abs(eastCurrSamples.mean())>currLimit)):
				plateV-=1.0
				if plateV<=0:
					plateV=0.0
				print(f"Current too high, dropping the voltage to {plateV} kV")
				hc.SetCPlusVoltage(plateV)
				hc.SetCMinusVoltage(plateV)
				
				break

	
def main():
	print("Use fieldcondition(waitTime in s, stepSize in kV, polarity in True/False)")
	print("or use switchcondition(switchTime in seconds, pollTime in ms)")
	print("or use smartCondition(waitTime in s, pollTime in ms, startV in kV, stepsize in kV, maxV in kV, currLimit=20 in nA, spikeLim=10)")
	pass

if __name__=="__main__":
	main()


