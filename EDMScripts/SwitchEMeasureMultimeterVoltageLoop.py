# Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *

from Analysis.EDM import *
from DAQ.Environment import *
from EDMConfig import *



def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()

def EDMGo():
	# loop and take data
	blockIndex = 0
	maxBlockIndex = 2000
	polarity = True
	voltage = 0
	f = open('C:/Users/edm/Desktop/test.txt', 'a')


	while blockIndex < maxBlockIndex:
		hc.ChangePolarity( polarity )
		print("Measurement Number " + str(blockIndex) + " ...")
		print("Polarity is " + str(polarity) )
		System.Threading.Thread.Sleep(1000)
		hc.UpdateBVoltage()
		voltage = hc.HPVoltage
		print("Voltage is " + str(voltage))
		f.write(str(polarity) + " " + str(voltage) + "\n")
		polarity = not polarity 
		blockIndex = blockIndex +1
	
	f.close()

def run_script():
	EDMGo()

