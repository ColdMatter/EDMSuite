# Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *
from System.Collections.Generic import Dictionary

from DAQ.Environment import *
from DAQ import *
from MOTMaster import *
import time

def run_script():
	return 0
	
def ScanSingleParameter(script_name, parameter_name, values):
	"""
	Generic function to run a MOTMaster script (script_name - note you don't need the path or .cs suffix) 
	repeatedly, whilst scanning a single parameter (parameter_name) over a list of values. Can be used 
	directly or with one of convenience functions defined below.
	"""
	dic = Dictionary[String, Object]()
	mm.SetScriptPath('C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
	for value in values:
		start = time.time()
		dic["ExpansionTime"] = value
		mm.Go(dic)
		end = time.time()
		print '{0} : {1} seconds'.format(value, int(round(end-start))
	print 'Finished'
	return 0

def ScanExpansionTime(values=[50, 650, 150, 750, 550, 350, 250, 450, 800]):
	script_name = 'MOTRampIntenisty'
	parameter_name = 'ExpansionTime'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def ScanExpansionTimeHot(values=[50, 260, 180, 140, 300, 220, 100]):
	script_name = 'MOTRampIntenisty'
	parameter_name = 'ExpansionTime'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def ScanOscillationTime(values=[ 0, 1200, 500, 900, 200, 1000, 300, 700, 400, 100, 600, 1100, 800, 1300]):
	script_name = 'MOTOscillation'
	parameter_name = 'OscillationTime'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def ScanSlowingAOMOff(values=[1500, 900, 1300, 1100]):
	script_name = 'MOTBasicExperimental'
	parameter_name = 'SlowingAOMOffStart'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def ScanFrame0Trigger(values=[250, 850, 1450, 1050, 450, 1250, 650]):
	script_name = 'MOTBasic'
	parameter_name = 'Frame0Trigger'
	return ScanSingleParameter(script_name, parameter_name, values)

def ScanPokeDetuning(values=[ -1.4,-1.35,-1.38,-1.31,-1.36,-1.33,-1.37,-1.32,-1.34,-1.39]):
	script_name = 'MOTOscillation'
	parameter_name = 'PokeDetuningValue'
	return ScanSingleParameter(script_name, parameter_name, values)

def ScanMolassesField(values=[0.05, -0.05, 0.25, 0.15, 0.35]):
	script_name = 'MOTBlueMolassesShimSwitch'
	parameter_name = 'MOTBOPCoilsCurrentMolassesValue'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def ScanRedMolassesTime(values=[8200, 8300, 8400, 8500, 8600, 8800, 9200]):
	script_name = 'MOTBlueMolassesRedMolasses'
	parameter_name = 'Frame0Trigger'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def ScanDetuningv0(values=[0.0,5.0,10.0,15.0,20.0,25.0,30.0,35.0]):
	script_name = 'MOTBlueMolassesShimSwitch'
	parameter_name = 'v0FrequencyNewValue'
	return ScanSingleParameter(script_name, parameter_name, values)

def MagTrapLifetime(values=[ 2000, 3000, 4000, 5000]):
	script_name = 'MOTMagTrapLifetime'
	parameter_name = 'magTrapLifetime'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def ChirpAmplitudeScan(values=[ -1.9, -1.95, -2.0, -2.05, -2.1]):
	script_name = 'MOTBasic'
	parameter_name = 'SlowingChirpEndValue'
	return ScanSingleParameter(script_name, parameter_name, values)	