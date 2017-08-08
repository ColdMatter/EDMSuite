# make division work like you'd expect
from __future__ import division

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
		dic[parameter_name] = value
		mm.Go(dic)
		end = time.time()
		print '{0} : {1} seconds'.format(value, int(round(end-start)))
	print 'Finished'
	return 0
	
def ScanMultipleParameters(script_name, parameter_names, values):
	"""
	Generic function to run a MOTMaster script (script_name - note you don't need the path or .cs suffix) 
	repeatedly, whilst scanning a list of parameters (parameter_names) over a list of lists of values. The number
	lists of values must match the number of parameters. The parameters will be looped over in order of appearance.
	Can be used directly or with one of convenience functions defined below.
	"""
	dic = Dictionary[String, Object]()
	mm.SetScriptPath('C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
	if not all(isinstance(item, list) for item in values):
		raise ValueError('Values must be a list of lists (even if only single valued!).')
	num_params = len(parameter_names)
	num_values_lists = len(values)
	if not num_params == num_values_lists:
		raise ValueError('The number of lists of values must match the number of parameters')
	row_format = '{:<3} {:<8}' + ' {:<20}' * num_params
	print row_format.format('N', 'Time', *parameter_names)
	i=0
	start = time.time()
	for combination in itertools.product(*values):
		iter_start = time.time()
		for j in range(0,num_params):
			dic[parameter_names[j]] = combination[j]
		mm.Go(dic)
		iter_end = time.time()
		i+=1
		print row_format.format(i, str(int(round(iter_end-iter_start))) + ' s', *combination)
	end = time.time()
	print 'Finished, total time was {} s.'.format(int(round(end-start)))
	
def ScanSingleParameter2(script_name, parameter_name, values):
	ScanMultipleParameters(script_name, [parameter_name], [values])
	
def ScanMicrowaveFrequency(script_name, centre_freq, num_steps, freq_range):
	lowest_freq = centre_freq - freq_range/2
	spacing = freq_range/(num_steps - 1)
	values = [lowest_freq + spacing * x for x in range(0, num_steps)]
	shuffle(values)
	mm.SetScriptPath('C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
	for value in values:
		start = time.time()
		hc.tabs["Windfreak Synthesizer"].SetFrequency(value, False)
		mm.Go()
		end = time.time()
		print '{0} : {1} seconds'.format(value, int(round(end-start)))
	print 'Finished'

def ScanExpansionTime(values=[50, 650, 150, 750, 550, 350, 250, 450, 800]):
	script_name = 'MOTBlueMolassesShimSwitch'
	parameter_name = 'ExpansionTime'
	return ScanSingleParameter(script_name, parameter_name, values)
def ScanExpansionTimeShort(values=[50, 650, 150, 750, 550, 350, 250, 450, 800]):
	script_name = 'MOTBlueMolassesShimSwitchShort'
	parameter_name = 'ExpansionTime'
	return ScanSingleParameter(script_name, parameter_name, values)	
def ScanExpansionTimeHot(values=[50, 260, 180, 140, 300, 220, 100]):
	script_name = 'MOTRampIntensity'
	parameter_name = 'ExpansionTime'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def ScanOscillationTime(values=[0, 1200, 500, 900, 200, 1000, 300, 700, 400, 100, 600, 1100, 800, 1300]):
	script_name = 'MOTOscillation'
	parameter_name = 'OscillationTime'
	return ScanSingleParameter(script_name, parameter_name, values)
	
def PokeVelocityMeasurement(values=[0,20,30,10]):
	script_name = 'PokeNoRecapture'
	parameter_name = 'FreeFlightTime'
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