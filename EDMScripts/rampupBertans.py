# This loop monitors the rf Discharges for a particular amplitude, then repeats for other amplitudes
# n

from DAQ.Environment import *

def RampVoltages(start, stop, step, sleep):

	# start looping
	r = range(int(100*start), int(100*stop), int(100*step))
	for i in range(len(r)):
		print  "E fields at +/- " + str((3*float(r[i]))/100) + " KV"
		hc.SetCPlusVoltage(-float(r[i])/100)
		hc.SetCMinusVoltage(float(r[i])/100)
		print "waiting for " +  str(sleep) + " milliseconds" 
		System.Threading.Thread.Sleep(sleep)

	raw_input("Measure at your leisure")
	r.reverse()
	
	for i in range(len(r)):
		print  "E fields at +/- " + str((3*float(r[i]))/100) + " KV"
		hc.SetCPlusVoltage(-float(r[i])/100)
		hc.SetCMinusVoltage(float(r[i])/100)
		print "waiting for " +  str(sleep) + " milliseconds" 
		System.Threading.Thread.Sleep(sleep)

	print "done"

def run_script():
	print "Use RampVoltages(start, stop, step, sleep). Voltages control voltages (use positive numbers, -sign taken care of in code for +ve supply), sleep in ms"

