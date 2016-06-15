# This loop monitors the rf Discharges for a particular amplitude, then repeats for other amplitudes
# n

from DAQ.Environment import *

def RampVoltages(start, stop, step, sleep):

	# start looping
	r = range(int(10*start), int(10*stop), int(10*step))
	for i in range(len(r)):
		print  "E fields at +/- " + str(float(r[i])/10) + " KV"
		hc.SetCPlusVoltage(float(r[i])/10)
		hc.SetCMinusVoltage(float(r[i])/10)
		print "waiting for " +  str(sleep) + " milliseconds" 
		System.Threading.Thread.Sleep(sleep)

	raw_input("Measure at your leisure")
	r.reverse()
	
	for i in range(len(r)):
		print  "E fields at +/- " + str(float(r[i])/10) + " KV"
		hc.SetCPlusVoltage(float(r[i])/10)
		hc.SetCMinusVoltage(float(r[i])/10)
		print "waiting for " +  str(sleep) + " milliseconds" 
		System.Threading.Thread.Sleep(sleep)

	print "done"

def run_script():
	print "Use RampVoltages(start, stop, step, sleep ). Voltages in kV, sleep in ms"

