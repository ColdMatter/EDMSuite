# This loop monitors the leakage current and saves in a csv for a 
# ramp up of the potential up to a given value



from DAQ.Environment import *

def CapacitorTest(plateVoltage, EPol, holdstart, holdend, holdstep, holdgap):
	'''CapacitorTest(plateVoltage, EPol, holdstart, holdend, holdstep, holdgap):
	times in seconds, plate voltage in kV
	'''
	#Logging started
	hc.FieldsOff()
	hc.SetCPlusVoltage(float(plateVoltage))
	hc.SetCMinusVoltage(float(plateVoltage))
	hc.UpdateVoltages()
	hc.ConnectEField(True)
	currentEpolarity = hc.EFieldPolarity
	
	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(200)

	loggingcheck=raw_input("Have you started logging? (y/n)\n")
	if loggingcheck!="y":
		print("Start the logging first")
		return

	print("Brief wait for initialisation")
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(30000)

	if not(currentEpolarity==EPol):
		hc.SwitchEAndWait()
	
	System.Threading.Thread.CurrentThread.Join(20000)

	# r = range(int(holdstart*1),int((holdstart+holdend)*1),int(holdstep*1))
	#r=[105, 135, 285, 45, 165, 225, 345, 255, 315, 15, 195, 75]
	r=[225, 315, 255, 75, 285, 45, 15, 165, 105, 135, 195, 345]
	for i in range(len(r)):
		System.Threading.Thread.CurrentThread.Join(holdgap*1000)
		print("Disconnecting for "+str(r[i])+" seconds")
		hc.ConnectEField(False)
		System.Threading.Thread.CurrentThread.Join(float(r[i])*1000)
		hc.ConnectEField(True)
		print("Waiting for recharging currents")

	print("About to discharge")
	System.Threading.Thread.CurrentThread.Join(20*1000)
	hc.EnableEField(False)

	print("Waiting at 0 potential for a minute")
	System.Threading.Thread.CurrentThread.Join(120*1000)
	hc.StopIMonitorPoll()

	print("Finished test")

		

def RampStart(start, stop, step, stepuptime, holduptime, stepdowntime):
	'''
	RampStart is how to run the leakage test file
	'''
	# Logging started 
	hc.FieldsOff()

	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(200)

	loggingcheck=raw_input("Have you started logging? (y/n)\n")
	if loggingcheck!="y":
		print("Start the logging first")
		return

	print("waiting for initialisation")
	System.Threading.Thread.CurrentThread.Join(1000)

	r = range(int(start*10), int((stop+step)*10), int(step*10))
	for i in range(len(r)):
		print("E fields at +/- " + str(float(r[i])/10) + " kV")
		hc.SetCPlusOffVoltage(float(r[i])/30)
		hc.SetCMinusOffVoltage(float(r[i])/30)
		hc.UpdateVoltages()
		print("waiting for " +  str(stepuptime) + " seconds")
		System.Threading.Thread.CurrentThread.Join(stepuptime*1000)
		

	print("Holding at full potential for "+str(holduptime)+" seconds")
	System.Threading.Thread.CurrentThread.Join(holduptime*1000)

	r.reverse()
	
	for i in range(len(r)):
		print("E fields at +/- " + str(float(r[i])/10) + " kV")
		hc.SetCPlusOffVoltage(float(r[i])/30)
		hc.SetCMinusOffVoltage(float(r[i])/30)
		hc.UpdateVoltages()
		print("waiting for " +  str(stepdowntime) + " seconds")
		System.Threading.Thread.CurrentThread.Join(stepdowntime*1000)

	print("Waiting at 0 potential for a minute")
	System.Threading.Thread.CurrentThread.Join(60*1000)
	hc.StopIMonitorPoll()

	print("Finished leakage test")

def run_script():
	# Ensure the data will be logged + set sensible poll period
	# print("Ticking Log Checkbox + setting sensible poll period. Please start polling leakage monitors")
	# hc.SetLeakageCurrentLogCheck(True)
	# hc.SetiMonitorPollPeriod(200)
	print("Hi new stuff")
	# print("Use RampStart(start, stop, step, stepuptime, holduptime, stepdowntime)")
	# print("Voltages in kV, waiting times in seconds")