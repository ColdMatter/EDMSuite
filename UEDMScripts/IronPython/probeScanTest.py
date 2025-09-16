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

def bgSubtract(intArray,bgArray):
    templist=[]
    for i in range(len(bgArray)):
        templist.append(intArray[i]-bgArray[i])
    signal = Array[float](templist)
    return signal

def sigRatio(sig1Array,sig2Array):
    templist=[]
    for i in range(len(sig2Array)):
        if sig2Array[i]==0:
            sig2Array[i]=0.0001
        templist.append(sig1Array[i]/sig2Array[i])
    signal = Array[float](templist)
    return signal

def emptyArray(sig1Array):
    templist=[]
    for i in range(len(sig1Array)):
        templist.append(0.)
    signal = Array[float](templist)
    return signal

def getFile(filepath):
    fileSystem = Environs.FileSystem
    dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
    file = \
        fileSystem.GetDataDirectory(\
                    fileSystem.Paths["scanMasterDataPath"])\
        + filepath
    return file

def getNextFile():
    fileSystem = Environs.FileSystem
    dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
    file=fileSystem.GenerateNextDataFileName()
    filepath = fileSystem.GetDataDirectory(fileSystem.Paths["scanMasterDataPath"]) + file
    return [filepath,file]

def getSetPoint(filePath='Example_scan_01.zip', scantype='On', detector=0,intStart=750, intEnd=4000, bgStart=600, bgEnd=750):
    scanSerializer = ScanSerializer()
    gfitter = GaussianFitter()

    file = getFile(filePath)

    scan = scanSerializer.DeserializeScanFromZippedXML(str(file),"average.xml")

    voltage = scan.ScanParameterArray
    signal = emptyArray(voltage)

    if scantype=='On':
        integral = scan.GetTOFOnIntegralArray(detector, intStart, intEnd)
        bg = scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd)
        
        signal = bgSubtract(integral, bg)

    elif scantype=='OnOff':
        int1 = scan.GetTOFOnIntegralArray(detector, intStart, intEnd)
        int2 = scan.GetTOFOffIntegralArray(detector, intStart, intEnd)
        bg1 = scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd)
        bg2 = scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd)
        sig1 = bgSubtract(int1,bg1)
        sig2 = bgSubtract(int2,bg2)
        signal = bgSubtract(sig1,sig2)

    elif scantype=='OffOn':
        int1 = scan.GetTOFOnIntegralArray(detector, intStart, intEnd)
        int2 = scan.GetTOFOffIntegralArray(detector, intStart, intEnd)
        bg1 = scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd)
        bg2 = scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd)
        sig1 = bgSubtract(int1,bg1)
        sig2 = bgSubtract(int2,bg2)
        signal = bgSubtract(sig2,sig1)

    elif scantype=='OnOffRatio':
        int1 = scan.GetTOFOnIntegralArray(detector, intStart, intEnd)
        int2 = scan.GetTOFOffIntegralArray(detector, intStart, intEnd)
        bg1 = scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd)
        bg2 = scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd)
        sig1 = bgSubtract(int1,bg1)
        sig2 = bgSubtract(int2,bg2)
        signal = sigRatio(sig1,sig2)

    bestGuess=gfitter.SuggestParameters(voltage,signal,0,1)
    # print(bestGuess)

    gfitter.Fit(voltage,signal,bestGuess)
    coeffs=gfitter.ParameterReport
    # print(coeffs)
    center=round(gfitter.returncenter(),6)
    return center

def probeScan():
    currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
    print("Current setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.25 around setpoint\n")

    [filepath,file] = getNextFile()
    print("Saving as " + file + "_*.zip")
    print("")

    SelectProfile("TCL Setpoint Scan Probe")

    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.25,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.25,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)

    print("\nScanning!\n")

    sm.AcquireAndWait(1)
    scanFile = file + "_01" + ".zip"
    scanPath = filepath + "_01" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)

    newSetPoint = round(getSetPoint(scanFile),5)
    print("\nSetting new probe setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("VISCavity", "probelaser", newSetPoint)
    # v0Set(newSetPoint)


def v0Set(setpoint):
    print("\nSetting new v0 setpoint at " + str(setpoint))
    tcl.SetLaserSetpoint("VISCavity", "v0laser", setpoint)

def v1Scan():
    currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "v1laser")
    print("Current setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.30 around setpoint\n")

    [a,b] = getNextFile()
    filepath = str(a)
    file = str(b)
    print("Saving as " + str(file) + "_*.zip")
    print("")

    SelectProfile("TCL Setpoint Scan V1")

    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.3,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.3,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)

    reply=prompt("Ready to acquire? Gas on? (y/n)\n")
    if reply!="y":
        return
    print("\nScanning!\n")

    sm.AcquireAndWait(1)
    scanFile = file + "_01" + ".zip"
    scanPath = filepath + "_01" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)

    newSetPoint = round(getSetPoint(scanFile,scantype='OnOffRatio'),5)
    print("\nSetting new setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("VISCavity", "v1laser", newSetPoint)

def p12Scan():
    currentSetpoint = tcl.GetLaserSetpoint("OPCavity", "P12")
    print("Current setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.30 around setpoint\n")

    file = getNextFile()
    print("Saving as " + file + "_*.zip")
    print("")

    SelectProfile("TCL Setpoint Scan P12")

    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.3,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.3,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)

    reply=prompt("Ready to acquire? Gas on? (y/n)\n")
    if reply!="y":
        return
    print("\nScanning!\n")

    sm.AcquireAndWait(1)
    scanPath = file + "_01" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
    print("\nSaving scan as "+scanPath)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)

    newSetPoint = round(getSetPoint(scanPath),5)
    print("\nSetting new setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("OPCavity", "P12", newSetPoint)

def q0Scan():
    currentSetpoint = tcl.GetLaserSetpoint("OPCavity", "Q0")
    print("Current setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.30 around setpoint\n")

    file = getNextFile()
    print("Saving as " + file + "_*.zip")
    print("")

    SelectProfile("TCL Setpoint Scan Q0")

    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.3,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.3,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "150", False)

    reply=prompt("Ready to acquire? Gas on? (y/n)\n")
    if reply!="y":
        return
    print("\nScanning!\n")

    sm.AcquireAndWait(1)
    scanPath = file + "_01" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
    print("\nSaving scan as "+scanPath)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)

    newSetPoint = round(getSetPoint(scanPath),5)
    print("\nSetting new setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("OPCavity", "Q0", newSetPoint)


def run_script():
    print('Use probeScan() to automate scanning the probe')
