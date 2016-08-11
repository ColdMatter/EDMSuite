
# Useful stuff for python controlof the Navigator experiment
# IB2015
#
clr.AddReference("System.Core")
import System
# clr.ImportExtensions(System.Linq)
from System import *
from System.Collections.Generic import Dictionary, List
from settings import *
import os
import glob
import csv
from time import sleep

script_path = "..\\..\\..\\NavigatorMotMasterScripts\\MOTMasterScripts\\"
settings_path = "C:\\Data\\Settings\\NavigatorHardwareController\\"
run_path = "C:\\Data\\Nav\\Analysis\\"


#
# Useful functions
# Add things here that will be accessed from the console often. If
# it takes more than a few lines put it in it's own file.
#
# ToDo:
# run a script multiple times, varying a parameter
#


def testRemote(dic):
    '''Tests remote stuff. Send it a dictionary and the console
    should print it out in nav hardwave control'''
    sendable = Dictionary[String, Object]()
    for k, v in dic.items():
        sendable.Add(k, v)
        print str(k) + ", " + str(v)
    returnString = hc.RemoteTest(sendable)


def SetChannel(channelName, value):
    '''Sets the channel with this name to value'''
    returnString = hc.SetValue(channelName, value)
    if returnString:
        print returnString


def GetChannels():
    channels = hc.RemoteGetChannels()
    return [c for c in channels]


def GetScripts():
    return [os.path.basename(x) for x in glob.glob(script_path + "*.cs")]


def GetSavedParameters():
    return glob.glob(settings_path + "*.json")


def LoadParameters(file):
    returnString = hc.RemoteLoadParameters(file)
    if returnString != "":
        print returnString


def RunScript(scriptName, p={}):
    paramDict = Dictionary[String, Object]()
    for k, v in p.items():
        paramDict.Add(k, v)
    returnDic = dict(mm.RemoteRun(script_path + scriptName, paramDict))

    print returnDic["returnMessage"]

    if(analyse):
        AnalyseAbsImage(returnDic["Path"], returnDic["EID"])

    return returnDic


def AnalyseAbsImage(path, EID):
    anal.ComputeAbsImage(path + EID + ".zip", EID)


def close():
    try:
        hc.CloseIt()
    except:
        pass

    try:
        mm.CloseIt()
    except:
        pass

    try:
        anal.CloseIt()
    except:
        pass

    exit()


def findDelayTimes(openMax, closeMax, imDelay):
    for openTime in range(0, openMax, 5):
        for closeTime in range(0, closeMax, 5):
            RunScript("AbsImagingDelay.cs", {
                      "imageDelay": imDelay, "openDelay": openTime, "closeDelay": closeTime}, True)


def aomSweep(startFreq, endFreq):
    freqVal = startFreq
    while(1):
        if (freqVal < endFreq):
            SetChannel("aom1freq", freqVal)
            freqVal += 1.0
            sleep(0.2)
        else:
            freqVal = startFreq
            sleep(0.2)


def coilSweep(startCurrent, endCurrent, step=0.1, wait=0.2):
    coilVal = startCurrent
    while(1):
        if (coilVal > endCurrent):
            SetChannel("motCoil", coilVal)
            coilVal -= step
            sleep(wait)
        else:
            coilVal = startCurrent
            sleep(wait)


def TempRun(startTime, endTime, noImages, p={}, save=True, analyse=False):
    t = datetime.datetime.today()
    dir = run_path + t.strftime('%Y\\%m\\%d\\')
    if not os.path.exists(dir):
        os.makedirs(dir)
    outfile = open(dir + 'TempRun' + t.strftime('%H%M%S') + '.txt', 'w')
    runlist = []
    for time in range(startTime * noImages, endTime * noImages, noImages):
        p["imageDelay"] = time / noImages
        run = RunScript("AbsImagingDelay.cs", p, save, analyse)
        runlist.append(run["Path"] + run["EID"] + ".zip")
    for item in runlist:
        outfile.write("%s\n" % item)
    outfile.close()
