from __future__ import print_function
import clr
import sys
from System.IO import Path
import time


sys.path.append(Path.GetFullPath("C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\"))
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\MOTMaster.exe")

sys.path.append(Path.GetFullPath("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\"))
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\MoleculeMOTHardwareControl.exe")
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\DAQ.dll")
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\SharedCode.dll")

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
hc = System.Activator.GetObject(MoleculeMOTHardwareControl.Controller, 'tcp://localhost:1172/controller.rem')
mm = System.Activator.GetObject(MOTMaster.Controller, 'tcp://localhost:1187/controller.rem')


# some generic stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *
from System.Collections.Generic import Dictionary
import time
import itertools
from random import shuffle

# specific EDMSuite stuff
from DAQ.Environment import *
from DAQ import *
from MOTMaster import *


def single_param_single_shot(script_name, parameter_name, value):
    dict_instance = Dictionary[String, Object]()
    script_path = 'C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts\\{}.cs'.format(script_name)
    mm.SetScriptPath(script_path)
    dict_instance[parameter_name] = value
    mm.Go(dict_instance)
    return True


def multi_param_single_shot(script_name, parameter_names, values):
    dict_instance = Dictionary[String, Object]()
    script_path = 'C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts\\{}.cs'.format(script_name)
    mm.SetScriptPath(script_path)
    for parameter_name, value in zip(parameter_names, values):
        dict_instance[parameter_name] = value
    mm.Go(dict_instance)
    return True
