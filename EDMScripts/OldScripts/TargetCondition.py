from System.Threading import *

def condition(switchTime):
	switchCount = 1
	#loop forever
	while switchCount > 0:
		print("Stepping target")
		hc.StepTarget(2)
		System.Threading.Thread.Sleep(1000 * switchTime)
		switchCount = switchCount + 1

	
def run_script():
	print "Use condition(switchTime)"


