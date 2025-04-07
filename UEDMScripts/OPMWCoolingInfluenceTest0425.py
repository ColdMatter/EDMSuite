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
#STIRAPSetpoint = tcl.GetLaserSetpoint("IRCavity", "STIRAP") #Save SP set currently for future reference

TTL1DurationStart = 4000 # Define Startpoint of STIRAP laser scan
TTL1DurationEnd =  10000# Define Endpoint of STIRAP laser scan
PointsTTL1Duration = 7 # Define Nr of Points of STIRAP laser scan

#TTL1StartStart = 2000 # Define Startpoint of STIRAP laser scan
#TTL1StartEnd =  8000# Define Endpoint of STIRAP laser scan
#PointsTTL1Duration = 13 # Define Nr of Points of STIRAP laser scan

datafolder = r'C:\Users\UEDM\Imperial College London\Team ultracold - PH - Documents\Data\2025\2025-04\20250407\OPCoolingInterference\Gate Type 2\\'

#for j in range(nrtriggers):
    
#This here is a bit of a work around. Thorlabs K-Cubes have two inputs which can be used to give it triggers. I will use one of our digital outputs to send basically a trigger. It sets it high, waits a second, sets it low, waits another second and starts the measurement. The idea it to add omething similar at the bottom for the other trigger which lets it move all the way back. 
    
    #print("\n Sent one Trigger to Translational Stage")
    #hc.SetDigitalOutput("Port00",True)
    #time.sleep(1)
    #hc.SetDigitalOutput("Port00",False)
    #time.sleep(1)

    #Start Loop to record the STIRAP scans. It will take an AOM scan per Laser SP set
for i in np.linspace(TTL1DurationStart,TTL1DurationEnd,PointsTTL1Duration):

        #Set STIRAP Laser SP
        #tcl.SetLaserSetpoint("IRCavity", "STIRAP", i)
        print("\n "+str(int(i)))
        time.sleep(2)

        #Select STIRAP AOM scan and set parameters
        SelectProfile("OPMWamplitudescan")
        sm.AdjustProfileParameter("switch","switchActive", str(True), False)
        sm.AdjustProfileParameter("pg","TTL1StartTimes", str(-2000), False)
        sm.AdjustProfileParameter("pg", "TTL1Repetitions", str(2), False)
        sm.AdjustProfileParameter("pg", "TTL1Durations", str(int(i)), False)

        #sm.AdjustProfileParameter("switch","switchActive", str(True), False)
        #sm.AdjustProfileParameter("pg","TTL1StartTimes", str(int(i)), False)
        #sm.AdjustProfileParameter("pg", "TTL1Repetitions", str(2), False)
        #sm.AdjustProfileParameter("pg", "TTL1Durations", str(500), False)

        #sm.AdjustProfileParameter("pg", "end", "171.37", False)
        # sm.AdjustProfileParameter("out", "scanMode", "up", False)
        # sm.AdjustProfileParameter("out", "pointsPerScan", "50", False)
        # sm.AdjustProfileParameter("out", "shotsPerPoint", "3", False)
        # print("\nScanning!\n")

        # Create Timestamp, Scan
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        sm.AcquireAndWait(1)

        # Save File
        #position = (j+1)*stepsize + startposition
        scanFile = "scan_"+"_TTL1StartTime"+str(i)+".zip"
        scanPath = datafolder + scanFile  
        print("\nSaving scan as "+ scanFile)
        sm.SaveData(scanPath)

#Reset STIRAP Laser SP
#tcl.SetLaserSetpoint("IRCavity", "STIRAP", STIRAPSetpoint) #Set again the initial setpoint before the scan happened  

#Reset Translational Stage
# for i in range(10):
#     hc.SetDigitalOutput("Port00",True)
#     time.sleep(1)
#     hc.SetDigitalOutput("Port00",False)
#     time.sleep(1)

