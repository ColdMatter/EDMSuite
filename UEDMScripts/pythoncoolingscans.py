from System import *

from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import *

probeSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
v1Setpoint = tcl.GetLaserSetpoint("VISCavity", "v1laser")
v0Setpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")-(6*0.0122)

print("V0 set to probe setpoint: " + str(probeSetpoint)+"\n")

tcl.SetLaserSetpoint("VISCavity", "v0laser", probeSetpoint)

print("Scanning 0.35 around V1 setpoint\n")
print("Make sure the V0 switch is ON\n")

[filepath,file] = getNextFile()

print("Saving as " + file + "_*.zip")
print("")

SelectProfile("TCL Setpoint Scan V1")

sm.AdjustProfileParameter("switch","switchActive", str(True), False)
sm.AdjustProfileParameter("shot","gateLength", str(5000), False)
sm.AdjustProfileParameter("out", "start", str(round(v1Setpoint-0.35,2)), False)
sm.AdjustProfileParameter("out", "end", str(round(v1Setpoint+0.35,2)), False)
sm.AdjustProfileParameter("out", "scanMode", "updown", False)
sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)

print("\nScanning!\n")

sm.AcquireAndWait(1)
scanFile = file + "_01" + ".zip"
scanPath = filepath + "_01" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
print("\nSaving scan as "+scanFile)

sm.SaveAverageData(scanPath)

System.Threading.Thread.CurrentThread.Join(5000)

newSetPoint = round(getSetPoint(scanFile,'OnOffRatio'),6)

print("\nSetting new probe setpoint at " + str(newSetPoint))

tcl.SetLaserSetpoint("VISCavity", "v1laser", newSetPoint)

print("plotting...")

plotfit(scanPath)
