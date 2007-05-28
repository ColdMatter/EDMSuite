# edm_init.py - sets up the IronPython environment ready for scripting
# the edm control software.

import clr
import sys
from System.IO import Path

# Import the edm control software assemblies into IronPython
sys.path.append(Path.GetFullPath("..\\ScanMaster\\bin\\Debug\\"))
clr.AddReferenceToFile("ScanMaster.exe")
sys.path.append(Path.GetFullPath("..\\BlockHead\\bin\\Debug\\"))
clr.AddReferenceToFile("BlockHead.exe")
sys.path.append(Path.GetFullPath("..\\EDMHardwareControl\\bin\\Debug\\"))
clr.AddReferenceToFile("EDMHardwareControl.exe")
clr.AddReferenceToFile("DAQ.dll")
clr.AddReferenceToFile("SharedCode.dll")
clr.AddReferenceToFile("Wolfram.NETLink.dll")

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
import BlockHead
import EDMHardwareControl

sm = typedproxy(System.Activator.GetObject(ScanMaster.Controller, 'tcp://localhost:1170/controller.rem'), ScanMaster.Controller)
bh = typedproxy(System.Activator.GetObject(BlockHead.Controller, 'tcp://localhost:1171/controller.rem'), BlockHead.Controller)
hc = typedproxy(System.Activator.GetObject(EDMHardwareControl.Controller, 'tcp://localhost:1172/controller.rem'), EDMHardwareControl.Controller)

# usage message
print("EDM interactive scripting control")
print('''
The variables sm, bh, and hc are pre-assigned to the ScanMaster, BlockHead
and EDMHardwareControl Controller objects respectively. You can call any of
these objects methods, for example: sm.AcquireAndWait(5). Look at the c#
code to see which remote methods are available. You can use any Python code
you like to script these calls.

Scripts in the directory EDMScripts are automatically loaded.
''')

# autoload scripts
import nt
pp = Path.GetFullPath("..\\EDMScripts")
files = nt.listdir(pp)
scriptsToLoad = [e[0:-3] for e in files if e.EndsWith(".py") and e != "edm_init.py"]
print(scriptsToLoad)
