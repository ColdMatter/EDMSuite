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

# add the EDMScripts directory to the module search path
#pp = System.IO.Path.GetFullPath("..\\EDMScripts")
#sys.path.append(pp)

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
print("")
