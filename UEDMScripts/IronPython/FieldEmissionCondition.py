from System.Threading import *
import math

def condition(waitTime, stepSize, polarity):
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
	
def run_script():
	print("Use condition(waitTime in s, stepSize in kV, polarity in True/False)")


