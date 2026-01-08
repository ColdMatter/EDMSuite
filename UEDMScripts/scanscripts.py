from System import *
from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import *

VOLT_PER_GAMMA = 0.0102 # V / Gamma (used to be 0.0122 before cavity change Nov24)

def probescan():
    currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
    print("Current setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.25 around setpoint\n")
    [filepath,file] = getNextFile()
    print("Saving as " + file + "_*.zip")
    print("")
    SelectProfile("TCL Setpoint Scan Probe")
    sm.AdjustProfileParameter("switch","switchActive", str(False), False)
    sm.AdjustProfileParameter("shot","sampleRate", str(10000), False)
    sm.AdjustProfileParameter("shot","gateStartTime", str(40), False)
    sm.AdjustProfileParameter("shot","gateLength", str(900), False)
    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.25,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.25,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)
    print("\nScanning!\n")
    sm.AcquireAndWait(1) #this sets amount of "passes" per scan and will start scanning immediately
    scanFile = file + "_probescan" + ".zip"
    scanPath = filepath + "_probescan" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath) # 
    System.Threading.Thread.CurrentThread.Join(5000)
    newSetPoint = round(getSetPoint(scanFile, 'On', 0, 6, 50, 55, 90),6)
    print("\nSetting new probe setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("VISCavity", "probelaser", newSetPoint)

def v1scan():
    # Set the V0 setpoint and take old v1 setpoint
    v0Setpoint = tcl.GetLaserSetpoint("VISCavity", "v0laser")
    probeSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
    newSetV0 = probeSetpoint - 6*VOLT_PER_GAMMA
    tcl.SetLaserSetpoint("VISCavity","v0laser",newSetV0)
    currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "v1laser")

    # Scan around rough setpoint
    print("Current v1 setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.35 around setpoint\n")
    [filepath,file] = getNextFile()
    print("Saving as " + file + "_*.zip")
    print("")
    SelectProfile("TCL Setpoint Scan V1")
    sm.AdjustProfileParameter("switch","switchActive", str(True), False)
    sm.AdjustProfileParameter("shot","sampleRate", str(10000), False)
    sm.AdjustProfileParameter("shot","gateStartTime", str(40), False)
    sm.AdjustProfileParameter("shot","gateLength", str(900), False)
    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.35,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.35,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "150", False)
    print("\nScanning!\n")
    sm.AcquireAndWait(1)
    scanFile = file + "_v1scan" + ".zip"
    scanPath = filepath + "_v1scan" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)
    newSetPoint = round(getSetPoint(scanFile,'OnOffRatio', 0, 6, 50, 55, 120),6)
    print("\nSetting new v1 setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("VISCavity", "v1laser", newSetPoint)

def v2scan():
    # Set the V0 setpoint and take old v1 setpoint
    v0Setpoint = tcl.GetLaserSetpoint("VISCavity", "v0laser")
    probeSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
    tcl.SetLaserSetpoint("VISCavity","v0laser",probeSetpoint)
    currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "v1laser")

    # Scan around rough setpoint
    print("Current v1 setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.35 around setpoint\n")
    [filepath,file] = getNextFile()
    print("Saving as " + file + "_*.zip")
    print("")
    SelectProfile("TCL Setpoint Scan V2Cav")
    sm.AdjustProfileParameter("switch","switchActive", str(True), False)
    sm.AdjustProfileParameter("shot","gateLength", str(6000), False)
    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.35,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.35,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "200", False)
    print("\nScanning!\n")
    sm.AcquireAndWait(1)
    scanFile = file + "_v1scan" + ".zip"
    scanPath = filepath + "_v1scan" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)
    newSetPoint = round(getSetPoint(scanFile,'OnOffRatio'),6)
    print("\nSetting new probe setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("VISCavity", "probelaser", newSetPoint)

def coolingTOF():
    # Not scanning anything but switching the cooling on and off
    [filepath,file] = getNextFile()
    scanFile = file + "_coolingTOF" + ".zip"
    scanPath = filepath + "_coolingTOF" + ".zip"
    print("Saving as " + file + "_*.zip")
    print("")
    SelectProfile("DownstreamTOF")
    sm.AdjustProfileParameter("switch","switchActive", str(True), False)
    sm.AdjustProfileParameter("shot","sampleRate", str(10000), False)
    sm.AdjustProfileParameter("shot","gateStartTime", str(40), False)
    sm.AdjustProfileParameter("shot","gateLength", str(1200), False)
    sm.AdjustProfileParameter("out", "start", str(0), False)
    sm.AdjustProfileParameter("out", "end", str(1), False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "256", False)
    print("\nScanning!\n")
    sm.AcquireAndWait(1)
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)
    coolingRatio=getCoolingRatio(scanFile)
    print("\nCooling ratio is " + str(coolingRatio))

def p12scan():
    # Check the setpoint
    currentSetpoint = tcl.GetLaserSetpoint("OPCavity", "P12")

    # Scan around rough setpoint
    print("Current p12 setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.35 around setpoint\n")
    [filepath,file] = getNextFile()
    scanFile = file + "_p12scan" + ".zip"
    scanPath = filepath + "_p12scan" + ".zip"
    print("Saving as " + file + "_*.zip")
    print("")
    SelectProfile("TCL Setpoint Scan P12")
    sm.AdjustProfileParameter("switch","switchActive", str(True), False)
    sm.AdjustProfileParameter("shot","sampleRate", str(10000), False)
    sm.AdjustProfileParameter("shot","gateStartTime", str(40), False)
    sm.AdjustProfileParameter("shot","gateLength", str(900), False)
    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.35,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.35,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "150", False)
    print("\nScanning!\n")
    sm.AcquireAndWait(1)
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)
    newSetPoint = round(getSetPoint(scanFile,'OnOffRatio', 0, 6, 50, 55, 120),6)
    print("\nSetting new p12 setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("OPCavity", "P12", newSetPoint)

def q0scan():
    # Check the setpoint
    currentSetpoint = tcl.GetLaserSetpoint("OPCavity", "Q0")

    # Scan around rough setpoint
    print("Current q0 setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.6 below and 0.2 above the setpoint\n")
    [filepath,file] = getNextFile()
    scanFile = file + "_q0scan" + ".zip"
    scanPath = filepath + "_q0scan" + ".zip"
    print("Saving as " + file + "_*.zip")
    print("")
    SelectProfile("TCL Setpoint Scan P12")
    sm.AdjustProfileParameter("switch","switchActive", str(True), False)
    sm.AdjustProfileParameter("shot","sampleRate", str(10000), False)
    sm.AdjustProfileParameter("shot","gateStartTime", str(40), False)
    sm.AdjustProfileParameter("shot","gateLength", str(900), False)
    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.6,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.2,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "150", False)
    print("\nScanning!\n")
    sm.AcquireAndWait(1)
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)
    newSetPoint = round(getSetPoint(scanFile,'OnOffRatio', 0, 6, 50, 55, 120),6)
    print("\nSetting new q0 setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("OPCavity", "Q0", newSetPoint)


def main():
    print("To scan the lasers on TCL, put them roughly where we expect using the wavemeter and lock them on TCL.")
    print("Then use these functions to complete scans of the appropriate laser, each one assume the previous has been completed:")
    print("\n probescan(time) \n v1scan() \n coolingTOF() \n p12scan() \n q0scan()")
    pass

if __name__ == "__main__":
    main()