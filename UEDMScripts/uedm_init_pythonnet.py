# uedm_init.py - sets up the Python environment ready for scripting
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
clr.AddReference(Path.GetFullPath("..\\DAQ\\bin\\x64\\ultracoldEDM\\DAQ"))
clr.AddReference(Path.GetFullPath("..\\SharedCode\\bin\\x64\\ultracoldEDM\\SharedCode"))
clr.AddReference(Path.GetFullPath("..\\UEDMHardwareControl\\bin\\x64\\ultracoldEDM\\UEDMHardwareControl"))
clr.AddReference(Path.GetFullPath("..\\EDMBlockHead\\bin\\x64\\ultracoldEDM\\EDMBlockHead.exe"))
clr.AddReference(Path.GetFullPath("..\\EDMPhaseLock\\bin\\x64\\ultracoldEDM\\EDMPhaseLock.exe"))
clr.AddReference(Path.GetFullPath("..\\EDMFieldLock\\bin\\x64\\ultracoldEDM\\EDMFieldLock.exe"))
clr.AddReference(Path.GetFullPath("..\\ScanMaster\\bin\\x64\\ultracoldEDM\\ScanMaster"))
clr.AddReference(Path.GetFullPath("..\\TransferCavityLock2012\\bin\\x64\\ultracoldEDM\\TransferCavityLock.exe"))

# create connections to the control programs
import System
import ScanMaster
import EDMBlockHead
import UEDMHardwareControl
import TransferCavityLock2012
import EDMPhaseLock
import EDMFieldLock

sm = System.Activator.GetObject(ScanMaster.Controller, 'tcp://localhost:1191/controller.rem')
bh = System.Activator.GetObject(EDMBlockHead.Controller, 'tcp://localhost:1181/controller.rem')
hc = System.Activator.GetObject(UEDMHardwareControl.UEDMController, 'tcp://localhost:1172/UEDMController.rem')
tcl = System.Activator.GetObject(TransferCavityLock2012.Controller, 'tcp://localhost:1190/controller.rem')
pl = System.Activator.GetObject(EDMPhaseLock.MainForm, 'tcp://localhost:1175/controller.rem')
fl = System.Activator.GetObject(EDMFieldLock.MainForm, 'tcp://localhost:1176/controller.rem')

# usage message
print('UEDM interactive scripting control')
print('''
The variables sm, tcl and hc are pre-assigned to the ScanMaster, TransferCavityLock 
and UEDMHardwareControl Controller objects respectively. You can call any of
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
scriptsToLoad = [e for e in files if e.endswith(".py") and e != "uedm_init.py" and e != "winforms.py" and e != "uedmfuncs.py" and e != "winforms.py" and e != "uedm_init_pythonnet.py"]
for i in range(len(scriptsToLoad)):
            print(str(i+1) + ": " + scriptsToLoad[i])
print("")

def run(i):
	execfile(scriptsToLoad[i-1], globals())