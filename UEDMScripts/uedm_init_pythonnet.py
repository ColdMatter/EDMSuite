# uedm_init.py - sets up the IronPython environment ready for scripting
# the edm control software.

import pythonnet
import clr
import sys
from System.IO import Path

# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")

# Import the edm control software assemblies into IronPython
clr.AddReference(Path.GetFullPath("..\\DAQ\\bin\\ultracoldEDM\\DAQ"))
clr.AddReference(Path.GetFullPath("..\\SharedCode\\bin\\ultracoldEDM\\SharedCode"))
clr.AddReference(Path.GetFullPath("..\\UEDMHardwareControl\\bin\\ultracoldEDM\\UEDMHardwareControl"))
clr.AddReference(Path.GetFullPath("..\\EDMBlockHead\\bin\\ultracoldEDM\\EDMBlockHead.exe"))
clr.AddReference(Path.GetFullPath("..\\EDMPhaseLock\\bin\\ultracoldEDM\\EDMPhaseLock.exe"))
clr.AddReference(Path.GetFullPath("..\\EDMFieldLock\\bin\\ultracoldEDM\\EDMFieldLock.exe"))
clr.AddReference(Path.GetFullPath("..\\ScanMaster\\bin\\ultracoldEDM\\ScanMaster"))
clr.AddReference(Path.GetFullPath("..\\TransferCavityLock2012\\bin\\ultracoldEDM\\TransferCavityLock.exe"))

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
    def __setattribute__(self, attr):
        proxyType = object.__setattribute__(self, 'proxyType')
        obj = object.__setattribute__(self, 'obj')
        return setattr(proxyType, attr).__set__(obj, proxyType)

# create connections to the control programs
import System
import ScanMaster
import EDMBlockHead
import UEDMHardwareControl
import TransferCavityLock2012
import EDMPhaseLock
import EDMFieldLock

sm = typedproxy(System.Activator.GetObject(ScanMaster.Controller, 'tcp://localhost:1191/controller.rem'), ScanMaster.Controller)
bh = typedproxy(System.Activator.GetObject(EDMBlockHead.Controller, 'tcp://localhost:1181/controller.rem'), EDMBlockHead.Controller)
hc = typedproxy(System.Activator.GetObject(UEDMHardwareControl.UEDMController, 'tcp://localhost:1172/UEDMController.rem'), UEDMHardwareControl.UEDMController)
tcl = typedproxy(System.Activator.GetObject(TransferCavityLock2012.Controller, 'tcp://localhost:1190/controller.rem'), TransferCavityLock2012.Controller)
pl = typedproxy(System.Activator.GetObject(EDMPhaseLock.MainForm, 'tcp://localhost:1175/controller.rem'), EDMPhaseLock.MainForm)
fl = typedproxy(System.Activator.GetObject(EDMFieldLock.MainForm, 'tcp://localhost:1176/controller.rem'), EDMFieldLock.MainForm)

# usage message
print('UEDM interactive scripting control')
print('''
The variables sm, tclProbe and hc are pre-assigned to the ScanMaster, TransferCavityLock 
and EDMHardwareControl Controller objects respectively. You can call any of
these objects methods, for example: sm.AcquireAndWait(5). Look at the c#
code to see which remote methods are available. You can use any Python code
you like to script these calls.

You can run scripts in the UEDMScripts directory with the command run(i),
where i is the script's index number (below). For this to work the script
must have a run_script() function defined somewhere. You'd be unwise to
try and run more than one script in a session with this method!

Available scripts:''')

# script shortcuts
import nt
pp = Path.GetFullPath("..\\UEDMScripts")
files = nt.listdir(pp)
scriptsToLoad = [e for e in files if e.EndsWith(".py") and e != "uedm_init.py" and e != "winforms.py" and e != "uedmfuncs.py" and e != "winforms.py" and e != "uedm_init_pythonnet.py"]
for i in range(len(scriptsToLoad)):
            print(str(i) + ": " + scriptsToLoad[i])
print("")

def run(i):
	global run_script
	execfile(scriptsToLoad[i], globals())
	run_script()
