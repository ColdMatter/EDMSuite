from System import *

from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import *
from datetime import datetime
import os
import numpy as np


AOMStart = 171.2 # Define Startpoint of STIRAP AOM scan
AOMEnd =  171.4 # Define Endpoint of STIRAP AOM scan
AOMNPoints = 30 # Define Nr of Points of STIRAP AOM scan

datafolder = r'C:\Users\UEDM\Imperial College London\Team ultracold - PH - Documents\Data\2025\2025-04\20250409\StirapESwitchAOMScan'

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
        sm.AdjustProfileParameter("pg", "TTL1Repetitions", str(1), False)
        sm.AdjustProfileParameter("pg", "TTL1Durations", str(int(i)), False)

        #sm.AdjustProfileParameter("switch","switchActive", str(True), False)
        #sm.AdjustProfileParameter("pg","TTL1StartTimes", str(int(i)), False)
        #sm.AdjustProfileParameter("pg", "TTL1Repetitions", str(1), False)
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

