# Sets up python control of the Nav controller

import clr
clr.AddReference("IronPython")
clr.AddReference("IronPython.Modules")
import sys
from System.IO import Path
from settings import *
# Sets references to files
#sys.path.append("C:\\Program Files (x86)\\IronPython 2.7\\Lib\\")
sys.path.append(Path.GetFullPath("..\\NavigatorHardwareControl\\bin\\Nav"))
sys.path.append(Path.GetFullPath("..\\MOTMaster\\bin\\Nav"))
path  = "C:\\Users\\Navigator\\Software\\EDMSuite\\"
#sys.path.append(Path.GetFullPath("NavAnalysis\\bin\\Debug"))
clr.AddReferenceToFile("NavigatorHardwareControl.exe")
clr.AddReferenceToFile("MOTMaster.exe")
clr.AddReferenceToFile("DAQ.dll")
clr.AddReferenceToFile("SharedCode.dll")
#clr.AddReferenceToFile("NavAnalysis.exe")

# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")
clr.AddReference("NationalInstruments.VisaNS")

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
import NavigatorHardwareControl
import MOTMaster
import NationalInstruments.VisaNS as visa
try:
    if sys.argv[1]:
        print "using control_ip"
        ip = control_ip
    else:
        print "using localhost"
        ip = "localhost"
except:
    # a hacky way of defaulting to localhost if no extra arguments are passed
    ip = "localhost"
    
hc = typedproxy(System.Activator.GetObject(NavigatorHardwareControl.Controller, 'tcp://'+ip+':1172/controller.rem'), NavigatorHardwareControl.Controller)
mm = typedproxy(System.Activator.GetObject(MOTMaster.Controller, 'tcp://'+ip+':1187/controller.rem'), MOTMaster.Controller)


print "hc object now exists"
print "mm object now exists"
print "anal object now exists"

execfile("useful_shit.py")