## Import a whole load of stuff
from DAQ.Environment import *
from uedmfuncs import *


def RampAndSwitch(waitTime, startV, stepsize, maxV, NumberOfSwitches):
	#hc.FieldsOff()
	plateV=startV
	print(f"Starting plate voltage at +- {startV} kV")
	hc.SetCPlusVoltage(plateV)
	hc.SetCMinusVoltage(plateV)
	hc.UpdateVoltages()
	hc.EnableEField(True)
	System.Threading.Thread.CurrentThread.Join(5000)
	print(f"Switching the Efield {NumberOfSwitches} times and then ramping the voltage in step sizes of {stepsize} kV to maximum {maxV} kV.")
	while plateV < maxV:
		print(f"Setting the voltage to {plateV} kV.")
		hc.SetCPlusVoltage(plateV)
		hc.SetCMinusVoltage(plateV)
		hc.UpdateVoltages()
		System.Threading.Thread.CurrentThread.Join(waitTime)
		
		for ind in range(NumberOfSwitches):
			hc.SwitchEBehlkeAndWait()
			print("Switch count "+str(ind))
			System.Threading.Thread.CurrentThread.Join(waitTime)
		
		plateV+=stepsize
		plateV=np.round(plateV,2)
		
	hc.SetCPlusVoltage(0)
	hc.SetCMinusVoltage(0)
	hc.UpdateVoltages()
	hc.FieldsOff()
	

def main():
	print("RampAndSwitch(waitTime in ms, startV in kV, stepsize in kV, maxV in kV, NumberOfSwitches)")
	RampAndSwitch(30000,14.0,1.0,19.1,5)
	pass

if __name__=="__main__":
	main()
