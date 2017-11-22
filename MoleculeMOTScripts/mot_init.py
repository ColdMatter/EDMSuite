import clr
import sys
from System.IO import Path
import time

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

# Import the edm control software assemblies into IronPython

#sys.path.append(Path.GetFullPath("C:\\Control Programs\\EDMSuite\\ScanMaster\\bin\\Decelerator\\"))
#clr.AddReferenceToFile("ScanMaster.exe")

sys.path.append(Path.GetFullPath("C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\"))
clr.AddReferenceToFile("MOTMaster.exe")

sys.path.append(Path.GetFullPath("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\"))
clr.AddReferenceToFile("MoleculeMOTHardwareControl.exe")
clr.AddReferenceToFile("DAQ.dll")
clr.AddReferenceToFile("SharedCode.dll")

# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")

# create connections to the control programs
import System
#import ScanMaster
import MOTMaster
import MoleculeMOTHardwareControl

#sm = typedproxy(System.Activator.GetObject(ScanMaster.Controller, 'tcp://localhost:1170/controller.rem'), #ScanMaster.Controller)
hc = typedproxy(System.Activator.GetObject(MoleculeMOTHardwareControl.Controller, 'tcp://localhost:1172/controller.rem'), MoleculeMOTHardwareControl.Controller)
mm = typedproxy(System.Activator.GetObject(MOTMaster.Controller, 'tcp://localhost:1187/controller.rem'), MOTMaster.Controller)

# usage message
print('MoleculeMOT interactive scripting control')
print('''
The variables mm, and hc are pre-assigned to the MOTMaster and MoleculeMOTHardwareControl Controller objects respectively. You can call any of these objects methods, for example: mm.Go(). Look at the c# code to see which remote methods are available. You can use any Python code you like to script these calls.

All the functions from scripts.py have been preloaded. If you want to reload the functions (e.g. if you changed them), you can run: reload_script().
''')

# code to reload script
def reload_script():
	execfile('scripts.py', globals())

reload_script()