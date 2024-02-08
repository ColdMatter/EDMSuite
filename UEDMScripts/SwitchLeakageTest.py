# This loop monitors the leakage current and saves in a csv for a 
# ramp up of the potential up to a given value

from DAQ.Environment import *

def SwitchLeakTest(kV, loops, holduptime):
	'''
	RampStart is how to run the leakage test file
	'''
	# Logging started 
	hc.FieldsOff()

	hc.CalibrateIMonitors()

	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(100)

	ePolarity = hc.EFieldPolarity
	waittime = hc.ERampUpTime

	loggingcheck=raw_input("Have you started logging? (y/n)\n")
	if loggingcheck!="y":
		print("Start the logging first")
		return

	print("waiting for initialisation")
	System.Threading.Thread.CurrentThread.Join(1000)

	hc.SetCMinusVoltage(kV)
	hc.SetCPlusVoltage(kV)

	hc.SwitchEAndWait(ePolarity)
	print("waiting for " +  str(holduptime) + " seconds")
	System.Threading.Thread.CurrentThread.Join((holduptime)*1000)

	for i in range(loops):
		print("Switching E field")
		hc.SwitchEAndWait()
		print("Waiting for " +  str(holduptime) + " seconds")
		System.Threading.Thread.CurrentThread.Join((holduptime)*1000)
		
	print("Ramping down")

	hc.FieldsOff()

	print("Waiting at 0 potential for two minutes")
	System.Threading.Thread.CurrentThread.Join(120*1000)
	hc.StopIMonitorPoll()
	
	# r = range(int(start*10), int((stop+step)*10), int(step*10))
	# for i in range(len(r)):
	# 	print("E fields at +/- " + str(float(r[i])/10) + " kV")
	# 	hc.SetCPlusOffVoltage(float(r[i])/30)
	# 	hc.SetCMinusOffVoltage(float(r[i])/30)
	# 	hc.UpdateVoltages()
	# 	print("waiting for " +  str(stepuptime) + " seconds")
	# 	System.Threading.Thread.CurrentThread.Join(stepuptime*1000)
		

	# print("Holding at full potential for "+str(holduptime)+" seconds")
	# System.Threading.Thread.CurrentThread.Join(holduptime*1000)

	# r.reverse()
	
	# for i in range(len(r)):
	# 	print("E fields at +/- " + str(float(r[i])/10) + " kV")
	# 	hc.SetCPlusOffVoltage(float(r[i])/30)
	# 	hc.SetCMinusOffVoltage(float(r[i])/30)
	# 	hc.UpdateVoltages()
	# 	print("waiting for " +  str(stepdowntime) + " seconds")
	# 	System.Threading.Thread.CurrentThread.Join(stepdowntime*1000)

	# print("Waiting at 0 potential for a minute")
	# System.Threading.Thread.CurrentThread.Join(60*1000)
	# hc.StopIMonitorPoll()

	print("Finished leakage test")

def run_script():
	# Ensure the data will be logged + set sensible poll period
	print("Ticking Log Checkbox + setting sensible poll period. Please start polling leakage monitors")
	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(100)

	print("Use SwitchLeakTest(kV, loops, holduptime)")
	print("Voltages in kV, waiting times in seconds, number of loops in integers")