# This loop monitors the leakage current and saves in a csv for a 
# ramp up of the potential up to a given value

from DAQ.Environment import *
from uedmfuncs import *

def RampStart(start, stop, step, stepuptime, holduptime, stepdowntime):
	'''
	RampStart is how to run the leakage test file
	'''
	# Logging started 
	hc.FieldsOff()
	hc.SetCPlusOffVoltage(0.0)
	hc.SetCMinusOffVoltage(0.0)
	# hc.EnableEField(True)

	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(200)

	loggingcheck=input("Have you started logging? (y/n)\n")
	if loggingcheck!="y":
		print("Start the logging first")
		return

	print("waiting for initialisation")
	System.Threading.Thread.CurrentThread.Join(1000)

	r = range(int(start*10), int((stop+step)*10), int(step*10))
	for i in r:
		print("E fields at +/- " + str(float(i)/10) + " kV")
		hc.SetCPlusOffVoltage(float(i)/10)
		hc.SetCMinusOffVoltage(float(i)/10)
		hc.UpdateVoltages()
		print("waiting for " +  str(stepuptime) + " seconds")
		System.Threading.Thread.CurrentThread.Join(stepuptime*1000)
		

	print("Holding at full potential for "+str(holduptime)+" seconds")
	System.Threading.Thread.CurrentThread.Join(holduptime*1000)

	a = reversed(r)
	
	for j in a:
		print("E fields at +/- " + str(float(j)/10) + " kV")
		hc.SetCPlusOffVoltage(float(j)/10)
		hc.SetCMinusOffVoltage(float(j)/10)
		hc.UpdateVoltages()
		print("waiting for " +  str(stepdowntime) + " seconds")
		System.Threading.Thread.CurrentThread.Join(stepdowntime*1000)

	print("Waiting at 0 potential for a minute")
	System.Threading.Thread.CurrentThread.Join(60*1000)
	hc.StopIMonitorPoll()

	print("Finished leakage test")

def run_script():
	# Ensure the data will be logged + set sensible poll period
	print("Ticking Log Checkbox + setting sensible poll period. Please start polling leakage monitors")
	hc.SetLeakageCurrentLogCheck(True)
	hc.SetiMonitorPollPeriod(200)

	print("Use RampStart(start, stop, step, stepuptime, holduptime, stepdowntime)")
	print("Voltages in kV, waiting times in seconds")

