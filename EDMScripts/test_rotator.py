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

r = Random()

def EDMGo():
	# loop and take data
	blockIndex = 0
	maxBlockIndex = 100000
	while blockIndex < maxBlockIndex:
		print("Acquiring block " + str(blockIndex) + " ...")
		# randomise polarization
		polAngle = 360.0 * r.NextDouble()
		hc.SetPolarizerAngle(polAngle)
		hc.SwitchEAndWait()
		blockIndex = blockIndex + 1

def run_script():
	EDMGo()

