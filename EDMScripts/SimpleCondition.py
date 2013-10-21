from System.Threading import *

def condition(switchTime):
	switchCount = 1
	#loop forever
	while switchCount > 0:
		hc.SwitchEAndWait()
		print "Switch count "+str(switchCount)
		System.Threading.Thread.Sleep(1000 * switchTime)
		switchCount = switchCount + 1

	
def run_script():
	print "Use condition(switchTime)"


