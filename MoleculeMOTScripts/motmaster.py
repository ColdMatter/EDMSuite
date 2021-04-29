# -*- coding: utf-8 -*-
"""
Created on Mon Apr 29 14:24:42 2019

@author: CaFMOT
"""
from __future__ import print_function
import clr
import sys
from System.IO import Path
import time
import os
from tqdm import tqdm

sys.path.append(Path.GetFullPath("C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\"))
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\MOTMaster.exe")
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\TransferCavityLock2012\\bin\\CaF\\TransferCavityLock.exe")

sys.path.append(Path.GetFullPath("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\"))
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\MoleculeMOTHardwareControl.exe")
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\DAQ.dll")
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\SharedCode.dll")

sys.path.append(Path.GetFullPath("C:\\Users\\cafmot\\Documents\\Visual Studio 2013\\Projects\\WavePlateControl\\WavePlateControl\\bin\\Debug\\"))
clr.AddReference("C:\\Users\\cafmot\\Documents\\Visual Studio 2013\\Projects\\WavePlateControl\\WavePlateControl\\bin\\Debug\\WavePlateControl.exe")


# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")

# create connections to the control programs
import System
#import ScanMaster
import MOTMaster
import MoleculeMOTHardwareControl
import TransferCavityLock2012
import WavePlateControl

#sm = typedproxy(System.Activator.GetObject(ScanMaster.Controller, 'tcp://localhost:1170/controller.rem'), #ScanMaster.Controller)
hc = System.Activator.GetObject(MoleculeMOTHardwareControl.Controller, 'tcp://localhost:1172/controller.rem')
mm = System.Activator.GetObject(MOTMaster.Controller, 'tcp://localhost:1187/controller.rem')
tcl = System.Activator.GetObject(TransferCavityLock2012.Controller, 'tcp://localhost:1190/controller.rem')
wpmotor = System.Activator.GetObject(WavePlateControl.Controller, 'tcp://localhost:1192/WPmotor.rem')

# usage message
print('''
MoleculeMOT interactive scripting control initialised\n

The variables mm, hc and tcl are pre-assigned to the MOTMaster and MoleculeMOTHardwareControl Controller,
and transfer cavity lock  objects respectively.
You can call any of these objects methods, for example: mm.Go(). 
Look at the c# code to see which remote methods are available. 
You can use any Python code you like to script these calls.
1. ScanSingleParameter(script_name, parameter_name, values)
2. ScanMultipleParametersList(script_name, parameter_names, value_tuples)
3. ScanMultipleParameters(script_name, parameter_names, values)
4. ScanMicrowaveFrequency(script_name, centre_freq, num_steps, freq_range, channel)
Use functionName.__doc__ for the individual doc strings.
''')

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


def ScanSingleParameter(
		script_name,
		parameter_name,
		parameter_values,
		):
	"""
	Function to scan a list of tcl laser set points with another parameter alternating
	between two given values
	input:
	script_name : name of the script in the motmaster to run
	parameter_name : name of the parameter that takes different values in different execution steps
	parameter_values : list with values from which the parameter_name will take value from
	return: True if no error occurs
	"""
	dictionary = Dictionary[String, Object]()
	mm.SetScriptPath('C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
	for value in tqdm(parameter_values):
		dictionary[parameter_name] = value
		mm.Go(dictionary)
	print("Finished...")
	return True


def ScanMultipleParameter(
		script_name,
		list_of_parameter_names,
		list_of_tuples_with_parameter_values,
		):
	"""
	Function to scan a list of tcl laser set points with another parameter alternating
	between two given values
	input:
	script_name : name of the script in the motmaster to run
	parameter_names : list parameters that takes different set of values in different execution steps
	list_of_parameter_values : list of lists with values from which the parameter_names will take value from
	return: True if no error occurs
	"""
	dictionary = Dictionary[String, Object]()
	mm.SetScriptPath('C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
	for tuple_value in tqdm(list_of_tuples_with_parameter_values):
		dictionary[parameter_name] = value
		mm.Go(dictionary)
	print("Finished...")
	return True


def ScanTCLWithAlternateParams(
		script_name,
		cavity_name,
		laser_name,
		alternate_parameter_name,
		alternate_parameter_values,
		tcl_scan_values_list
		):
	"""
	Function to scan a list of tcl laser set points with another parameter alternating
	between two given values
	input:
	script_name : name of the script in the motmaster to run
	cavity_name : name of the cavity in which the scanning laser is listed, e.g. Hamish / Carlos
	laser_name  : name of the laser to be scanned
	alternate_parameter_name : parameter with two different values to be alternated between two
	                           execution steps
	alternate_parameter_values : list with two values between which the alternate_parameter_name
	                             will take values from
	tcl_scan_values_list : list of laser set point values to set in the tcl selected with 
	                       the parameters laser_name and cavity_name
	return: True if no error occurs
	"""
	dictionary = Dictionary[String, Object]()
	mm.SetScriptPath('C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
	for value in tqdm(tcl_scan_values_list):
		tcl.SetLaserSetpoint(cavity_name, laser_name, value)
		dictionary[alternate_parameter_name] = alternate_parameter_values[0]
		mm.Go(dictionary)
		time.sleep(0.1)
		dictionary[alternate_parameter_name] = alternate_parameter_values[1]
		mm.Go(dictionary)
		time.sleep(0.1)
	print("Finished...")
	return True