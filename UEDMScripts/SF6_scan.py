from System import *

from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import *
from datetime import datetime
import os
import numpy as np

SF6_flow_start=0.001
SF6_flow_end=0.007
SF6_points=7
SF6_baseline=0.005

SF6_points=np.array((0.001,0.002,0.005,0.01,0.02,0.04,0.08))
datafolder = r'C:\Users\UEDM\Box\Ultracold eEDM\Data\2026\2026-09\20260915\SF6 scans'
#for j in range(nrtriggers):
    
#This here is a bit of a work around. Thorlabs K-Cubes have two inputs which can be used to give it triggers. I will use one of our digital outputs to send basically a trigger. It sets it high, waits a second, sets it low, waits another second and starts the measurement. The idea it to add omething similar at the bottom for the other trigger which lets it move all the way back. 
    
    #print("\n Sent one Trigger to Translational Stage")
    #hc.SetDigitalOutput("Port00",True)
    #time.sleep(1)
    #hc.SetDigitalOutput("Port00",False)
    #time.sleep(1)

    #Start Loop to record the STIRAP scans. It will take an AOM scan per Laser SP set
for i in np.random.permutation(SF6_points):

        #Set STIRAP Laser SP
        #tcl.SetLaserSetpoint("IRCavity", "STIRAP", i)
        print("\n "+str(round(i,4)))
        hc.SetRemoteSF6Flow(i)
        time.sleep(2)

        #Select STIRAP AOM scan and set parameters
        SelectProfile("Downstream TOF")
        sm.AdjustProfileParameter("switch","switchActive", str(False), False)
        sm.AdjustProfileParameter("out", "pointsPerScan", "500", False)
        sm.AdjustProfileParameter("out", "shotsPerPoint", "1", False)
        

        # Create Timestamp, Scan
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        print("scan starting")
        sm.AcquireAndWait(1)
        print("scan done")

        # Save File
        #position = (j+1)*stepsize + startposition
        scanFile = "\scan_"+"_SF6Flow"+str(round(i,4))+".zip"
        scanPath = datafolder + scanFile  
        print("\nSaving scan as "+ scanFile)
        sm.SaveAverageData(scanPath)

        hc.SetRemoteSF6Flow(SF6_baseline)
        time.sleep(2)
        print("scan starting ref")
        sm.AcquireAndWait(1)
        print("scan done")
        scanFile = "\scan_REF_"+"_SF6Flow"+str(round(i,4))+".zip"
        scanPath = datafolder + scanFile  
        print("\nSaving scan as "+ scanFile)
        sm.SaveAverageData(scanPath)
        
hc.SetRemoteSF6Flow(0)

#Reset STIRAP Laser SP
#tcl.SetLaserSetpoint("IRCavity", "STIRAP", STIRAPSetpoint) #Set again the initial setpoint before the scan happened  

#Reset Translational Stage
# for i in range(10):
#     hc.SetDigitalOutput("Port00",True)
#     time.sleep(1)
#     hc.SetDigitalOutput("Port00",False)
#     time.sleep(1)

