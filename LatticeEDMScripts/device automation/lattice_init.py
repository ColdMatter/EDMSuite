# lattice_init.py - sets up the IronPython environment ready for scripting
# the latticeEDM control software.

import clr
import sys
from System.IO import Path

# Import the edm control software assemblies into IronPython
sys.path.append(Path.GetFullPath("..\\ScanMaster\\bin\\LatticeEDM\\")) #Need to change the address here and make sure it's the correct one
clr.AddReferenceToFile("ScanMaster.exe") 
#sys.path.append(Path.GetFullPath("..\\EDMBlockHead\\bin\\EDM\\"))
#clr.AddReferenceToFile("EDMBlockHead.exe")
sys.path.append(Path.GetFullPath("..\\LatticeEDMtest\\bin\\LatticeEDM\\")) #This is the lattice control program
clr.AddReferenceToFile("LatticeHardwareControl.exe")
clr.AddReferenceToFile("DAQ.dll")
clr.AddReferenceToFile("SharedCode.dll")
#sys.path.append(Path.GetFullPath("..\\SirCachealot\\bin\\Lattice\\"))
#clr.AddReferenceToFile("SirCachealot.exe")
sys.path.append(Path.GetFullPath("..\\TransferCavityLock2012\\bin\\LatticeEDM\\"))
clr.AddReferenceToFile("TransferCavityLock.exe")
#sys.path.append(Path.GetFullPath("..\\EDMPhaseLock\\bin\\EDM\\"))
#clr.AddReferenceToFile("EDMPhaseLock.exe")

# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")

# code for IronPython remoting problem workaround
class typedproxy(object):
    __slots__ = ['obj', 'proxyType']
    def __init__(self, obj, proxyType):
        self.obj = obj
        self.proxyType = proxyType
    def __getattribute__(self, attr):
        proxyType = object.__getattribute__(self, 'proxyType')
        obj = object.__getattribute__(self, 'obj')
        return getattr(proxyType, attr).__get__(obj, proxyType)


# create connections to the control programs
import System
import ScanMaster
#import EDMBlockHead
import LatticeHardwareControl
#import SirCachealot
import TransferCavityLock2012
#import EDMPhaseLock

hc = typedproxy(System.Activator.GetObject(LatticeHardwareControl.Program, 'tcp://localhost:1197/controller.rem'), LatticeHardwareControl.Program) #Need to change the hardware controller name and address here
sm = typedproxy(System.Activator.GetObject(ScanMaster.Controller, 'tcp://localhost:1191/controller.rem'), ScanMaster.Controller) #Need to change the address here
#bh = typedproxy(System.Activator.GetObject(EDMBlockHead.Controller, 'tcp://localhost:1181/controller.rem'), EDMBlockHead.Controller)
#sc = typedproxy(System.Activator.GetObject(SirCachealot.Controller, 'tcp://localhost:1180/controller.rem'), SirCachealot.Controller)
#tclProbe = typedproxy(System.Activator.GetObject(TransferCavityLock2012.Controller, 'tcp://155.198.206.103:1190/controller.rem'), TransferCavityLock2012.Controller)
#pl = typedproxy(System.Activator.GetObject(EDMPhaseLock.MainForm, 'tcp://localhost:1175/controller.rem'), EDMPhaseLock.MainForm)

# usage message
print('Lattice EDM interactive scripting control')
print('''
The variables sm, and hc are pre-assigned to the ScanMaster
and EDMHardwareControl Controller objects respectively. You can call any of
these objects methods, for example: sm.AcquireAndWait(5). Look at the c#
code to see which remote methods are available. You can use any Python code
you like to script these calls.

You can run scripts in the LatticeEDMScripts directory with the command run(i),
where i is the script's index number (below). For this to work the script
must have a run_script() function defined somewhere. You'd be unwise to
try and run more than one script in a session with this method!

Available scripts:''')

# script shortcuts
import nt
pp = Path.GetFullPath("..\\LatticeEDMScripts")
files = nt.listdir(pp)
scriptsToLoad = [e for e in files if e.EndsWith(".py") and e != "lattice_init.py" and e != "winforms.py"]
for i in range(len(scriptsToLoad)):
            print str(i) + ": " + scriptsToLoad[i]
print ""

def run(i):
	global run_script
	execfile(scriptsToLoad[i], globals())
	run_script()
