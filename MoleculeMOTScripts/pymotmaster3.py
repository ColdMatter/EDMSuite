# -*- coding: utf-8 -*-
"""
Created on Mon Apr 29 14:24:42 2019

@author: CaFMOT
"""

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
import MOTMaster.bin.CaF.MOTMaster
import MoleculeMOTHardwareControl

#sm = typedproxy(System.Activator.GetObject(ScanMaster.Controller, 'tcp://localhost:1170/controller.rem'), #ScanMaster.Controller)
#hc = System.Activator.GetObject(MoleculeMOTHardwareControl.Controller, 'tcp://localhost:1172/controller.rem')
mm = System.Activator.GetObject(MOTMaster.Controller, 'tcp://localhost:1187/controller.rem')

# usage message
print('''
MoleculeMOT interactive scripting control initialised\n

The variables mm, and hc are pre-assigned to the MOTMaster and MoleculeMOTHardwareControl Controller objects respectively.
You can call any of these objects methods, for example: mm.Go(). 
Look at the c# code to see which remote methods are available. 
You can use any Python code you like to script these calls.
1. ScanSingleParameter(script_name, parameter_name, values)
2. ScanMultipleParametersList(script_name, parameter_names, value_tuples)
3. ScanMultipleParameters(script_name, parameter_names, values)
4. ScanMicrowaveFrequency(script_name, centre_freq, num_steps, freq_range, channel)
Use functionName.__doc__ for the individual doc strings.
''')

