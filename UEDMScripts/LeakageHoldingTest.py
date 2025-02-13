# This loop monitors the leakage current and saves in a csv for a 
# ramp up of the potential up to a given value

from DAQ.Environment import *
from uedmfuncs import *

def CapacitorTest(plateVoltage, EPol, holdstart, holdend, holdstep, holdgap):
	'''CapacitorTest(plateVoltage, EPol, holdstart, holdend, holdstep, holdgap):
	times in seconds, plate voltage in kV
	'''
	#Logging stopped
	hc.StopIMonitorPoll()
	
	#Fields Off and plate voltage prepped
	hc.FieldsOff()
	hc.SetCPlusVoltage(float(plateVoltage))
	hc.SetCMinusVoltage(float(plateVoltage))
	hc.UpdateVoltages()
	hc.ConnectEField(True)
	currentEpolarity = hc.EFieldPolarity
	
	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(100)

	[filepath,file] = getNextFile()

	hc.leakageFileSave = filepath+'_EFieldDisconnectTest.csv'

	if (hc.leakageFileSave==''):
		loggingcheck=input("Have you started logging? (y/n)\n")
		if loggingcheck!="y":
			print("Start the logging first")
			return
	else:
		hc.StartIMonitorPoll()
	
	print("Brief wait for initialisation")
	System.Threading.Thread.CurrentThread.Join(10000)
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(30000)

	if not(currentEpolarity==EPol):
		hc.SwitchEAndWait()
	
	System.Threading.Thread.CurrentThread.Join(20000)

	# r = range(int(holdstart*1),int((holdstart+holdend)*1),int(holdstep*1))
	#r=[105, 135, 285, 45, 165, 225, 345, 255, 315, 15, 195, 75]
	#r=[225, 315, 255, 75, 285, 45, 15, 165, 105, 135, 195, 345]
	r=np.arange(holdstart,holdend,holdstep)
	np.random.shuffle(r)
	print(r)
	for i in r:
		System.Threading.Thread.CurrentThread.Join(holdgap*1000)
		print("Disconnecting for "+str(i)+" seconds")
		hc.ConnectEField(False)
		System.Threading.Thread.CurrentThread.Join(int(i)*1000)
		hc.ConnectEField(True)
		print("Waiting for recharging currents")

	print("About to discharge")
	System.Threading.Thread.CurrentThread.Join(20*1000)
	hc.EnableEField(False)

	print("Waiting at 0 potential for a minute")
	System.Threading.Thread.CurrentThread.Join(60*1000)
	hc.StopIMonitorPoll()

	print("Finished test")
	print("Updating Testlist with pattern")

	with open(filepath[:-2]+'00_EFieldTestList.txt','a') as patternfile:
		s=str(r)[0]+' '+str(r)[1:-1]+' '+str(r)[-1]
		line=file+'_EFieldTest'+'\t'+s+'\n'
		patternfile.write(line)

	return


def RampTest(start, stop, step, stepuptime, holduptime, stepdowntime):
	'''
	RampStart is how to run the leakage ramp test.
	'''
	#Logging stopped
	hc.StopIMonitorPoll()
	
	#Fields Off and plate voltage prepped
	hc.FieldsOff()
	hc.SetCPlusVoltage(float(start))
	hc.SetCMinusVoltage(float(start))
	hc.UpdateVoltages()
	hc.ConnectEField(True)
	currentEpolarity = hc.EFieldPolarity
	
	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(100)

	[filepath,file] = getNextFile()

	hc.leakageFileSave = filepath+'_EFieldRampTest.csv'

	if (hc.leakageFileSave==''):
		loggingcheck=input("Have you started logging? (y/n)\n")
		if loggingcheck!="y":
			print("Start the logging first")
			return
	else:
		hc.StartIMonitorPoll()
	
	print("Brief wait for initialisation")
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(10*1000)

	r = np.arange(float(start), float(stop+step), float(step))
	for i in r:
		print("E fields at +/- " + str(i) + " kV")
		hc.SetCPlusVoltage(float(i))
		hc.SetCMinusVoltage(float(i))
		hc.UpdateVoltages()
		print("waiting for " +  str(stepuptime) + " seconds")
		System.Threading.Thread.CurrentThread.Join(stepuptime*1000)
		

	print("Holding at full potential for "+str(holduptime)+" seconds")
	System.Threading.Thread.CurrentThread.Join(holduptime*1000)

	a = reversed(r)
	
	for i in a:
		print("E fields at +/- " + str(float(i)) + " kV")
		hc.SetCPlusVoltage(float(i))
		hc.SetCMinusVoltage(float(i))
		hc.UpdateVoltages()
		print("waiting for " +  str(stepdowntime) + " seconds")
		System.Threading.Thread.CurrentThread.Join(stepdowntime*1000)

	print("Waiting at 0 potential for a minute")
	hc.EnableEField(False)
	System.Threading.Thread.CurrentThread.Join(60*1000)
	hc.StopIMonitorPoll()

	# with open(filepath[:-2]+'00_EFieldTestList.txt','a') as patternfile:
	# 	s=str(r)[0]+' '+str(r)[1:-1]+' '+str(r)[-1]
	# 	line=file+'_EFieldTest'+'\t'+s+'\n'
	# 	patternfile.write(line)

	print("Finished leakage test")



def RampSwitchTest(start, stop, step, stepuptime, holduptime, stepdowntime):
	'''
	RampStart is how to run the leakage test file
	'''
	#Logging stopped
	hc.StopIMonitorPoll()
	
	#Fields Off and plate voltage prepped
	hc.FieldsOff()
	hc.SetCPlusVoltage(float(start))
	hc.SetCMinusVoltage(float(start))
	hc.UpdateVoltages()
	hc.ConnectEField(True)
	currentEpolarity = hc.EFieldPolarity
	
	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(100)

	[filepath,file] = getNextFile()

	hc.leakageFileSave = filepath+'_EFieldRampSwitchTest.csv'

	if (hc.leakageFileSave==''):
		loggingcheck=input("Have you started logging? (y/n)\n")
		if loggingcheck!="y":
			print("Start the logging first")
			return
	else:
		hc.StartIMonitorPoll()
	
	print("Brief wait for initialisation")
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(1000)

	r = np.arange(float(start), float(stop+step), float(step))
	for i in r:
		print("E fields at +/- " + str(i) + " kV")
		hc.SetCPlusVoltage(float(i))
		hc.SetCMinusVoltage(float(i))
		hc.UpdateVoltages()
		print("waiting for " +  str(stepuptime) + " seconds")
		System.Threading.Thread.CurrentThread.Join(stepuptime*1000)
		

	print("Holding at full potential for "+str(holduptime)+" seconds")
	System.Threading.Thread.CurrentThread.Join(holduptime*1000)

	print("Testing disconnections and RC time to discharge and charge:")
	print("Disconnect -> Bleed 15 s -> Switch EPol -> Reconnect -> wait 15s")
	System.Threading.Thread.CurrentThread.Join(1000)

	for j in range(5):
		currentEpolarity = hc.EFieldPolarity
		hc.ConnectEField(False)
		System.Threading.Thread.CurrentThread.Join(1000)
		hc.EnableBleed(True)
		System.Threading.Thread.CurrentThread.Join(15000)
		hc.EnableBleed(False)
		System.Threading.Thread.CurrentThread.Join(1000)
		hc.EFieldPolarity = not(currentEpolarity)
		System.Threading.Thread.CurrentThread.Join(1000)
		hc.ConnectEField(True)
		System.Threading.Thread.CurrentThread.Join(15000)
	
	print("Holding for another 20 seconds")
	System.Threading.Thread.CurrentThread.Join(20000)
	
	a = reversed(r)
	
	for i in a:
		print("E fields at +/- " + str(float(i)) + " kV")
		hc.SetCPlusVoltage(float(i))
		hc.SetCMinusVoltage(float(i))
		hc.UpdateVoltages()
		print("waiting for " +  str(stepdowntime) + " seconds")
		System.Threading.Thread.CurrentThread.Join(stepdowntime*1000)

	print("Waiting at 0 potential for a minute")
	System.Threading.Thread.CurrentThread.Join(60*1000)
	hc.EnableEField(False)
	hc.StopIMonitorPoll()

	print("Finished leakage test")

def main():
	# Ensure the data will be logged + set sensible poll period
	#print("Ticking Log Checkbox + setting sensible poll period. Please start polling leakage monitors")
	#hc.SetLeakageCurrentLogCheck(True)
	#hc.SetiMonitorPollPeriod(100)
	#print("Hi new stuff")
	print("Use CapacitorTest(plateVoltage, EPol, holdstart, holdend, holdstep, holdgap)")
	print("or try RampTest(start, stop, step, stepuptime, holduptime, stepdowntime)")
	print("or use RampSwitchTest(start, stop, step, stepuptime, holduptime, stepdowntime)")
	print("Voltages in kV, EPol in True/False, waiting times in seconds")
	pass

if __name__=="__main__":
	main()