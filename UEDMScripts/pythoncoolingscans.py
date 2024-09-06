from System import *

from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import *

currentSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")

print("Current setpoint is: " + str(currentSetpoint)+"\n")
print("Scanning 0.25 around setpoint\n")

[filepath,file] = getNextFile()

print("Saving as " + file + "_*.zip")
print("")

SelectProfile("TCL Setpoint Scan Probe")

sm.AdjustProfileParameter("switch","switchActive", str(False), False)
sm.AdjustProfileParameter("shot","gateLength", str(5000), False)
sm.AdjustProfileParameter("out", "start", str(round(currentSetpoint-0.15,2)), False)
sm.AdjustProfileParameter("out", "end", str(round(currentSetpoint+0.15,2)), False)
sm.AdjustProfileParameter("out", "scanMode", "updown", False)
sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)

print("\nScanning!\n")

sm.AcquireAndWait(1)
scanFile = file + "_01" + ".zip"
scanPath = filepath + "_01" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
print("\nSaving scan as "+scanFile)

sm.SaveAverageData(scanPath)

System.Threading.Thread.CurrentThread.Join(5000)

newSetPoint = round(getSetPoint(scanFile),6)

print("\nSetting new probe setpoint at " + str(newSetPoint))

tcl.SetLaserSetpoint("VISCavity", "probelaser", newSetPoint)

print("plotting...")

plotfit(scanPath)
