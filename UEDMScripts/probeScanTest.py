# Import a whole load of stuff
# from System.IO import *
# from System.Drawing import *
# from System.Runtime.Remoting import *
# from System.Threading import *
# from System.Windows.Forms import *
# from System.Xml.Serialization import *
from System import *

from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import prompt#, SelectProfile

def SelectProfile(profileName):
    sm.SelectProfile(profileName)

def getFile(filepath):
    fileSystem = Environs.FileSystem
    dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
    file = \
        fileSystem.GetDataDirectory(\
                    fileSystem.Paths["scanMasterDataPath"])\
        + filepath
    return file

def test():
    currentSetpoint = 0.324#tcl.GetLaserSetpoint("VISCavity", "probelaser")
    print("Current setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.15 around setpoint\n")

    file = getFile('scan')
    print("Saving as " + file + "_*.zip")
    print("")

    SelectProfile("TCL Setpoint Scan Probe")

    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.15,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.15,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)

    reply=prompt("Ready to acquire? Gas on? (y/n)\n")
    if reply!="y":
        return
    print("\nScanning!\n")

    # sm.AcquireAndWait(2)
    scanPath = file + "_01" + ".zip"
    print("\nSaving scan as "+scanPath)
    # sm.SaveAverageData(scanPath)

    newSetPoint = round(getSetPoint(),5)
    print("\nSetting new setpoint at " + str(newSetPoint))
    # tcl.SetLaserSetpoint("VISCavity", "probelaser", newSetpoint)
    

def getSetPoint(filePath='Example_scan_01.zip', intStart=750, intEnd=4000, bgStart=600, bgEnd=750):
    scanSerializer = ScanSerializer()
    gfitter = GaussianFitter()

    file = getFile(filePath)

    scan = scanSerializer.DeserializeScanFromZippedXML(str(file),"average.xml")

    integral = scan.GetTOFOnIntegralArray(0,intStart,intEnd)
    bg = scan.GetTOFOnIntegralArray(0,bgStart,bgEnd)
    voltage = scan.ScanParameterArray

    templist=[]
    for i in range(len(bg)):
        templist.append(integral[i]-bg[i])
    signal = Array[float](templist)

    bestGuess=gfitter.SuggestParameters(voltage,signal,0,1)
    # print(bestGuess)

    gfitter.Fit(voltage,signal,bestGuess)
    coeffs=gfitter.ParameterReport
    # print(coeffs)
    center=gfitter.returncenter()
    return center




    

# def Acquire():
#     currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
#     count=0
#     print("Current setpoint is: " + str(currentSetpoint))
#     print("")
#     fileSystem = Environs.FileSystem
#     file = \
#         fileSystem.GetDataDirectory(\
#                     fileSystem.Paths["scanMasterDataPath"])\
#         + fileSystem.GenerateNextDataFileName()
#     print("Saving as " + file + "_*.zip")
#     print("")
#     # start looping
#     x = [0.002*i for i in range(-10,10)]
#     for j in range(5):
#         for i in range(len(x)):
#             sm.AdjustProfileParameter("outputPlugin", "dummy", x[i], False)
#             tcl.SetLaserSetpoint("VISCavity", "probelaser",currentSetpoint+x[i])
#             for k in range(0,2):
#                 print("Current detuning: " + str(x[i]) + ", MW state: " + str(hc.MwSwitchState))
#                 count=count+1
#                 sm.AcquireAndWait(2)
#                 scanPath = file + "_" + str(count) + "_" + str(round(currentSetpoint+x[i],3)) + ".zip"
#                 sm.SaveAverageData(scanPath)
#                 hc.SwitchMwAndWait()
#     tcl.SetLaserSetpoint("VISCavity", "probelaser",currentSetpoint)

def run_script():
	# SMGo()
	# SelectProfile("Scan B")
	# Acquire()
    test()
