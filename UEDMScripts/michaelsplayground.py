from System import *

from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import *
from datetime import datetime
import os
import numpy as np

# [filepath,file] = getNextFile()

# print("Saving as " + file + "_*.zip")
# print("")

# SelectProfile("TCL Setpoint Scan V1")

# sm.AdjustProfileParameter("switch","switchActive", str(True), False)
# sm.AdjustProfileParameter("shot","gateLength", str(5000), False)
# sm.AdjustProfileParameter("out", "start", str(round(v1Setpoint-0.35,2)), False)
# sm.AdjustProfileParameter("out", "end", str(round(v1Setpoint+0.35,2)), False)
# sm.AdjustProfileParameter("out", "scanMode", "updown", False)
# sm.AdjustProfileParameter("out", "pointsPerScan", "100", False)

# print("\nScanning!\n")

# sm.AcquireAndWait(1)
# scanFile = file + "_01" + ".zip"
# scanPath = filepath + "_01" + ".zip" # 'C:\\Users\\UEDM\\OneDrive - Imperial College London\\UltracoldEDM\\Data\\ScriptData\\2023\\December2023\\19Dec2300_01.zip'
# print("\nSaving scan as "+scanFile)

# sm.SaveAverageData(scanPath)

# System.Threading.Thread.CurrentThread.Join(5000)

# newSetPoint = round(getSetPoint(scanFile,'OnOffRatio'),6)

# print("\nSetting new probe setpoint at " + str(newSetPoint))

# tcl.SetLaserSetpoint("VISCavity", "v1laser", newSetPoint)

# print("plotting...")

# plotfit(scanPath)
STIRAPSetpoint = tcl.GetLaserSetpoint("IRCavity", "STIRAP") #Save SP set currently for future reference
SPLaserStart = -1.9 # Define Startpoint of STIRAP laser scan
SPLaserEnd = -1.3 # Define Endpoint of STIRAP laser scan
PointsLaser = 8 # Define Nr of Points of STIRAP laser scan
nrtriggers = 18 # Define how often the Stage is supposed to be triggered
stepsize = 0.05
startposition = 10.60
datafolder = r'C:\Users\UEDM\Imperial College London\Team ultracold - PH - Documents\Data\2025\2025-02\20250211\STIRAP\Low Power Test Around Resonance\\'

for j in range(nrtriggers):
    
#This here is a bit of a work around. Thorlabs K-Cubes have two inputs which can be used to give it triggers. I will use one of our digital outputs to send basically a trigger. It sets it high, waits a second, sets it low, waits another second and starts the measurement. The idea it to add omething similar at the bottom for the other trigger which lets it move all the way back. 
    
    print("\n Sent one Trigger to Translational Stage")
    hc.SetDigitalOutput("Port00",True)
    time.sleep(1)
    hc.SetDigitalOutput("Port00",False)
    time.sleep(1)

    #Start Loop to record the STIRAP scans. It will take an AOM scan per Laser SP set
    for i in np.round(np.linspace(SPLaserStart,SPLaserEnd,PointsLaser),3):

        #Set STIRAP Laser SP
        tcl.SetLaserSetpoint("IRCavity", "STIRAP", i)
        print("\n STIRAP Laser set to"+str(i))
        time.sleep(2)

        #Select STIRAP AOM scan and set parameters
        SelectProfile("Scan B")
        # sm.AdjustProfileParameter("switch","switchActive", str(False), False)
        # sm.AdjustProfileParameter("shot","gateLength", str(1100), False)
        # sm.AdjustProfileParameter("out", "start", "171.25", False)
        # sm.AdjustProfileParameter("out", "end", "171.37", False)
        # sm.AdjustProfileParameter("out", "scanMode", "up", False)
        # sm.AdjustProfileParameter("out", "pointsPerScan", "50", False)
        # sm.AdjustProfileParameter("out", "shotsPerPoint", "3", False)
        # print("\nScanning!\n")

        # Create Timestamp, Scan
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        sm.AcquireAndWait(1)

        # Save File
        position = (j+1)*stepsize + startposition
        scanFile = "scan_"+timestamp+"_STIRAPSP_" +f"{i:.2f}"+"_STAGEPOS_" +f"{position:.2f}"+".zip"
        scanPath = datafolder + scanFile  
        print("\nSaving scan as "+ scanFile)
        sm.SaveData(scanPath)

#Reset STIRAP Laser SP
tcl.SetLaserSetpoint("IRCavity", "STIRAP", STIRAPSetpoint) #Set again the initial setpoint before the scan happened  

#Reset Translational Stage
# for i in range(10):
#     hc.SetDigitalOutput("Port00",True)
#     time.sleep(1)
#     hc.SetDigitalOutput("Port00",False)
#     time.sleep(1)

