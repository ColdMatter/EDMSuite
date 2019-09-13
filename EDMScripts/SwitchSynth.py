# edm_init.py - sets up the IronPython environment ready for scripting
# the edm control software.

import clr
import sys
import time
from System.IO import Path

# Import the edm control software assemblies into IronPython
sys.path.append(Path.GetFullPath("..\\EDMHardwareControl\\bin\\EDM\\"))
clr.AddReferenceToFile("EDMHardwareControl.exe")
clr.AddReferenceToFile("DAQ.dll")

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
import EDMHardwareControl

hc = typedproxy(System.Activator.GetObject(EDMHardwareControl.Controller, 'tcp://localhost:1172/controller.rem'), EDMHardwareControl.Controller)

def run_script():
    reps = prompt('No. of times to switch synth on and off: ')
    for i in range(int(reps)):
        print('Rep: %d') % i
        hc.EnableGreenSynth(True)
        time.sleep(0.1)
        hc.EnableGreenSynth(False)
        time.sleep(0.1)

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()



