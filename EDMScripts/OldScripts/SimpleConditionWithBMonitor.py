from System.Threading import *

def condition(switchTime):
	switchCount = 1
	#loop forever
	f = open('27Aug1300.txt', 'a')
	f.write("eState, ePlus, eMinus, DB0db0, DB0db1, DB1db0, DB1db1\n")
	f.close()
	while switchCount > 0:
		hc.SwitchEAndWait()
		hc.UpdateBCurrentMonitor()
		hc.UpdateVMonitor()
		eState = hc.EFieldPolarity
		ePlus = hc.CPlusMonitorVoltage * hc.CPlusMonitorScale
		eMinus = hc.CMinusMonitorVoltage * hc.CMinusMonitorScale		
		bCurrent00 = hc.BCurrent00
		bCurrent01 = hc.BCurrent01
		bCurrent10 = hc.BCurrent10
		bCurrent11 = hc.BCurrent11 
		s = str(eState) + ", " + str(ePlus) + ", " + str(eMinus) + ", " + str(bCurrent00) + ", " + str(bCurrent01) + ", " + str(bCurrent10) + ", " + str(bCurrent11) + "\n"
		print s
		f = open('27Aug1300.txt', 'a')
		f.write(s)
		f.close()
		System.Threading.Thread.Sleep(1000 * switchTime)
		switchCount = switchCount + 1

	
def run_script():
	print "Use condition(switchTime)"


