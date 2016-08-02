# Sets up python control of the Nav controller

import clr
import sys
from System.IO import Path

# Sets references to files

sys.path.append(Path.GetFullPath("..\\NavigatorHardwareControl\\bin\\Debug"))
sys.path.append(Path.GetFullPath("..\\MOTMaster\\bin\\Debug"))
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
#from NavigatorHardwareControl import Controller as hc
#from MOTMaster import Controller as mm
#import NavAnalysis 	
hc = typedproxy(System.Activator.GetObject(NavigatorHardwareControl.Controller, 'tcp://localhost:1172/controller.rem'), NavigatorHardwareControl.Controller)
mm = typedproxy(System.Activator.GetObject(MOTMaster.Controller, 'tcp://localhost:1187/controller.rem'), MOTMaster.Controller)
#anal = typedproxy(System.Activator.GetObject(NavAnalysis.Controller, 'tcp://localhost:1188/controller.rem'), NavAnalysis.Controller)


print "hc object now exists"
print "mm object now exists"
print "anal object now exists"

execfile("useful_shit.py")