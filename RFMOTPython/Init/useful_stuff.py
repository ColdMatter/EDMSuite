from System.Collections.Generic import List, Dictionary
from System import String,Object

path_stem = 'C:\\Users\\rfmot\\EDMSuite\\RFMOTMOTMasterScripts\\'

exec("myDic = Dictionary[String,Object]()")

import time

class controller:
    def __init__(self, MMObject, HCObject):
        self.mm = MMObject
        self.hc = HCObject
        self.parametersDictionary = parametersDictionaryWrapper()
        self.camTrigIntervals = range(0,75,5) 
        
    def tempMeasurement(self,triggerIntervals = None):
        if triggerIntervals == None:
            for i in self.camTrigIntervals:
                self.parametersDictionary["CamTrig1Time"] = 7500 + i
                self.mm.RemoteRun(path_stem+'FreeExpansionImage.cs',self.parametersDictionary,True)
        else:
            assert isinstance(triggerIntervals,list), "Trigger interval specification must be a list of values"
            for trigTime in triggerIntervals:
                self.parametersDictionary["CamTrig1Time"] = trigTime
                self.mm.RemoteRun(path_stem+'FreeExpansionImage.cs',self.parametersDictionary,True)
                    
    def setValueOnHCAndWait(self, chanName, value):
        self.hc.SetValue(chanName,value)
        time.sleep(1);
        
    def temperatureSequence(self, chanName, paramToVaryValues):
        for paramValue in paramToVaryValues:
            self.setValueOnHCAndWait(chanName,paramValue)
            self.tempMeasurement()
            
    def clearParametersDictionary(self):
        self.parametersDictionary.clear()

    def  setSaveDirectory(self):
        currentDirectory =  self.mm.getSaveDirectory()
        print ("Current directory = " + currentDirectory)
        newDirectory = raw_input("\n New Directory : ")
        self.mm.SetSaveDirectory(newDirectory)
        currentDirectory =  self.mm.getSaveDirectory()
        print ("Current directory = " + currentDirectory)
   
class parametersDictionaryWrapper:
    '''
    A wrapper for use with commonly changed experimental parameters
    '''
    def __init__(self):
        self.__dictionary = Dictionary[String,Object]()
    def clear(self):
        parametersDictionaryWrapper.__init__()
    def setMolassesDuration(self,value):
        self.__dictionary["MolassesDuration"] = float(value)
    def setMolassesLaserFreq(self,value):
        self.__dictionary["CoolingLaserMolassesValue"]
    def add_parameter(self,chanName,value):
        self.__dictionary[chanName] = value
    def get_dictionary(self):
        return self.__dictionary

def createValuesList(listMin,listMax,listStep):
    numPoints = (listMax - listMin)/listStep + 1
    toReturn = []
    for i in range(int(numPoints)):
        toReturn.append(listMin + listStep*i)
    return toReturn

