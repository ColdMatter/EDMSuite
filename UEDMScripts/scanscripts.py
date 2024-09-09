from System import *
from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import *

VOLT_PER_GAMMA = 0.0122 # V / Gamma

def probescan():
    currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
    print("Current setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.25 around setpoint\n")
    [filepath,file] = getNextFile()
    print("Saving as " + file + "_*.zip")
    print("")
    SelectProfile("TCL Setpoint Scan Probe")
    sm.AdjustProfileParameter("switch","switchActive", str(False), False)
    sm.AdjustProfileParameter("shot","gateLength", str(5000), False)
    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.25,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.25,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)
    print("\nScanning!\n")
    sm.AcquireAndWait(1)
    scanFile = file + "_probescan" + ".zip"
    scanPath = filepath + "_probescan" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
    print("\nSaving scan as "+scanFile)
    sm.SaveAverageData(scanPath)
    System.Threading.Thread.CurrentThread.Join(5000)
    newSetPoint = round(getSetPoint(scanFile),6)
    print("\nSetting new probe setpoint at " + str(newSetPoint))
    tcl.SetLaserSetpoint("VISCavity", "probelaser", newSetPoint)

def v1scan():
    # Set the V0 setpoint and take old v1 setpoint
    v0Setpoint = tcl.GetLaserSetpoint("VISCavity", "v0laser")
    probeSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
    tcl.SetLaserSetpoint("VISCavity","v0laser",probeSetpoint)
    currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "v1laser")

    # Scan around rough setpoint
    print("Current v1 setpoint is: " + str(currentSetpoint)+"\n")
    print("Scanning 0.4 around setpoint\n")
    [filepath,file] = getNextFile()
    print("Saving as " + file + "_*.zip")
    print("")
    SelectProfile("TCL Setpoint Scan V1")
    sm.AdjustProfileParameter("switch","switchActive", str(True), False)
    sm.AdjustProfileParameter("shot","gateLength", str(5000), False)
    sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.4,2)), False)
    sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.4,2)), False)
    sm.AdjustProfileParameter("out", "scanMode", "updown", False)
    sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)
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

def main():
    print("To scan the lasers on TCL, put them roughly where we expect using the wavemeter and lock them on TCL.")
    print("Then use these functions to complete scans of the appropriate laser, each one assume the previous has been completed:")
    print("\n probescan() \n v1scan()")
    pass

if __name__ == "__main__":
    main()