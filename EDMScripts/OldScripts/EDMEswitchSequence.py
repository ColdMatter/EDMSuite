from System.Threading import *
from System import *

def switchEandWait(timeinS):
	hc.FlipDB()
	hc.SwitchEAndWait()
        while (hc.SwitchingEfields==True):
		pass
	System.Threading.Thread.Sleep(1000*timeinS)

def EswitchSequence():
	switchCount = 1
	#loop forever
	polarity = True
	r = Random()
	while switchCount > 0:
		polarity = bool(r.Next(0,2))
		if (hc.EFieldPolarity==polarity):
			print "Bonus Switch"
			hc.SwitchEAndWait()
			System.Threading.Thread.Sleep(100)
		hc.SetTargetStepperHigh()
		switchEandWait(10)
		switchEandWait(20)
		switchEandWait(10)
		switchEandWait(10)
		switchEandWait(20)
		switchEandWait(20)
		switchEandWait(20)
		switchEandWait(10)
		switchEandWait(10)
		switchEandWait(20)
		switchEandWait(10)
		hc.FlipDB()
        	hc.SwitchEAndWait()
        	switchCount=switchCount+1
        	print "Switch Count " + str(switchCount)
		System.Threading.Thread.Sleep(30000)
		hc.SetTargetStepperLow()
def run_script():
	print "Use EswitchSequence()"


