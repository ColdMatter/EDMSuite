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
from time import sleep

def run_script():
	return 0

def ScanExpansionTime(values=[50, 650, 150, 750, 550, 350, 250, 450, 800]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTRampIntensity.cs")
	while(count < endcount):
		dic["ExpansionTime"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0
	
def ScanExpansionTimeHot(values=[50, 260, 180, 140, 300, 220, 100]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTRampIntensity.cs")
	while(count < endcount):
		dic["ExpansionTime"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0

	
def ScanOscillationTime(values=[ 0, 1200, 500, 900, 200, 1000, 300, 700, 400, 100, 600, 1100, 800, 1300]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTOscillation.cs")
	while(count < endcount):
		dic["OscillationTime"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0
	
def ScanSlowingAOMOff(values=[1500, 900, 1300, 1100]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTBasicExperimental.cs")
	while(count < endcount):
		dic["slowingAOMOffStart"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0
	
def ScanFrame0Trigger(values=[250, 850, 1450, 1050, 450, 1250, 650]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTBasic2.cs")
	while(count < endcount):
		dic["Frame0Trigger"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0

def ScanPokeDetuning(values=[ -1.4,-1.35,-1.38,-1.31,-1.36,-1.33,-1.37,-1.32,-1.34,-1.39]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTOscillation.cs")
	while(count < endcount):
		dic["PokeDetuningValue"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0	

def ScanMolassesField(values=[0.05, -0.05, 0.25, 0.15, 0.35]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTBlueMolassesShimSwitch.cs")
	while(count < endcount):
		dic["MOTBOPCoilsCurrentMolassesValue"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0

	
def ScanRedMolassesTime(values=[8200, 8300, 8400, 8500, 8600, 8800, 9200]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTBlueMolassesRedMolasses.cs")
	while(count < endcount):
		dic["Frame0Trigger"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0
	
def ScanDetuningv0(values=[0.0,5.0,10.0,15.0,20.0,25.0,30.0,35.0]):
	count = 0
	
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTBlueMolassesShimSwitch.cs")
	while(count < endcount):
		dic["v0FrequencyNewValue"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0

def MagTrapLifetime(values=[ 2000, 3000, 4000, 5000]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTMagTrapLifetime.cs")
	while(count < endcount):
		dic["magTrapLifetime"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0
	
def ChirpAmplitudeScan(values=[ -1.9, -1.95, -2.0, -2.05, -2.1]):
	count = 0
	list = values
	endcount = len(list)
	dic = Dictionary[String, Object]()
	mm.SetScriptPath("C:\\Control Programs\\EDMSuite\\MoleculeMOTMasterScripts\\MOTBasic2.cs")
	while(count < endcount):
		dic["SlowingChirpEndValue"] = list[count]
		mm.Go(dic)
		count=count + 1
	return 0	
	