# edm_init.py - sets up the IronPython environment ready for scripting
# the edm control software.

import clr
import sys
from System.IO import Path

# Import the edm control software assemblies into IronPython
sys.path.append(Path.GetFullPath("..\\ScanMaster\\bin\\EDM\\"))
clr.AddReferenceToFile("ScanMaster.exe")
sys.path.append(Path.GetFullPath("..\\EDMBlockHead\\bin\\EDM\\"))
clr.AddReferenceToFile("EDMBlockHead.exe")
sys.path.append(Path.GetFullPath("..\\EDMHardwareControl\\bin\\EDM\\"))
clr.AddReferenceToFile("EDMHardwareControl.exe")
clr.AddReferenceToFile("DAQ.dll")
clr.AddReferenceToFile("SharedCode.dll")
sys.path.append(Path.GetFullPath("..\\SirCachealot\\bin\\EDM\\"))
clr.AddReferenceToFile("SirCachealot.exe")
sys.path.append(Path.GetFullPath("..\\TransferCavityLock2012\\bin\\EDM\\"))
clr.AddReferenceToFile("TransferCavityLock.exe")
sys.path.append(Path.GetFullPath("..\\EDMPhaseLock\\bin\\EDM\\"))
clr.AddReferenceToFile("EDMPhaseLock.exe")
sys.path.append(Path.GetFullPath("..\\EDMFieldLock\\bin\\EDM\\"))
clr.AddReferenceToFile("EDMFieldLock.exe")

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
import EDMBlockHead
import EDMHardwareControl
import SirCachealot
import TransferCavityLock2012
import EDMPhaseLock
import EDMFieldLock

sm = typedproxy(System.Activator.GetObject(ScanMaster.Controller, 'tcp://localhost:1170/controller.rem'), ScanMaster.Controller)
bh = typedproxy(System.Activator.GetObject(EDMBlockHead.Controller, 'tcp://localhost:1181/controller.rem'), EDMBlockHead.Controller)
hc = typedproxy(System.Activator.GetObject(EDMHardwareControl.Controller, 'tcp://localhost:1172/controller.rem'), EDMHardwareControl.Controller)
sc = typedproxy(System.Activator.GetObject(SirCachealot.Controller, 'tcp://localhost:1180/controller.rem'), SirCachealot.Controller)
tclProbe = typedproxy(System.Activator.GetObject(TransferCavityLock2012.Controller, 'tcp://155.198.206.242:1190/controller.rem'), TransferCavityLock2012.Controller)
pl = typedproxy(System.Activator.GetObject(EDMPhaseLock.MainForm, 'tcp://localhost:1175/controller.rem'), EDMPhaseLock.MainForm)
fl = typedproxy(System.Activator.GetObject(EDMFieldLock.MainForm, 'tcp://localhost:1176/controller.rem'), EDMFieldLock.MainForm)

# usage message
print('EDM interactive scripting control')
print('''
The variables sm, bh, pl, fl and hc are pre-assigned to the ScanMaster, BlockHead, EDMPhaseLock, EDMFieldLock
and EDMHardwareControl Controller objects respectively. You can call any of
these objects methods, for example: sm.AcquireAndWait(5). Look at the c#
code to see which remote methods are available. You can use any Python code
you like to script these calls.

You can run scripts in the EDMScripts directory with the command run(i),
where i is the script's index number (below). For this to work the script
must have a run_script() function defined somewhere. You'd be unwise to
try and run more than one script in a session with this method!

Available scripts:''')

# script shortcuts
import nt
pp = Path.GetFullPath("..\\EDMScripts")
files = nt.listdir(pp)
scriptsToLoad = [e for e in files if e.EndsWith(".py") and e != "edm_init.py" and e != "winforms.py"]
for i in range(len(scriptsToLoad)):
            print str(i) + ": " + scriptsToLoad[i]
print ""

def run(i):
	global run_script
	execfile(scriptsToLoad[i], globals())
	run_script()
