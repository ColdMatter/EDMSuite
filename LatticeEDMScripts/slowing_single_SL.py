# -*- coding: utf-8 -*-
"""
Created on Wed Aug  6 14:15:48 2025

@author: sl5119
"""

#%% Import libraries
import sys
import os
import re

OneDriveFolder = os.environ['onedrive']
sys.path.append(OneDriveFolder + r"\Desktop\EDMSuite\LatticeEDMScripts")
import LatticeEDM_analysis_library as EDM

import numpy as np

import glob
import matplotlib.pyplot as plt

import tools as tools

tools.set_plots()

#%% Set data path
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month=datadrive+"\\June2025\\"
date=month+"\\25\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

#%% Load and analyse perpendicular probe data, for calibration
"""Get zero-velocity frequency for V0. Must have
"""
pattern="*_ProbeSetpointScan.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])


if len(files) > 0:
    DataRef = EDM.ReadAverageScanInZippedXML(files[0])
    SettingsRef = EDM.GetScanSettings(DataRef)
    ScanParamsRef = EDM.GetScanParameterArray(DataRef)
    
    f_iniTHzRef, f_relMHzRef = EDM.GetScanFreqArrayMHz(DataRef)
    if int(f_iniTHzRef) == 542:
        TCL_WM_cali = EDM.TCL_WM_Calibration(DataRef, plot=True)
        HasWM = True
        TCLconv = TCL_WM_cali['best fit'][0]
        TCLconverr = TCL_WM_cali['error'][0]
        print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
    else:
        HasWM = False
        print("Wrong fibre in WM.")
    
else:
    print("No rest frame data available on this day.")

#%% Load slowing data
pattern="*_ProbeScan_angled_*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

Data = EDM.ReadAverageScanInZippedXML(files[1])
print("loaded file " + files[1])

#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

slowing_time = Settings["slowing time"]/1000
print("Slowing duration is " + str(slowing_time) + " ms")

fileLabel = re.split(r'[\\]', files[1])[-1][0:3]

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 25 #in ms
SigEnd = 30
BkgStart = 60
BkgEnd = 70

wavelength = 552 #in nm

showTOF = True
shot_for_TOF = 20

BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Data)
if int(f_iniTHz) == 542:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=True)
    HasWM = True
else:
    HasWM = False
    print("Wrong fibre in WM.")

distance = 1.3 # m
MOTdistance = 1.8 

gateLength = 1.
gatesStart = np.arange(SigStart, SigEnd, step=gateLength)
gatesEnd = np.arange(SigStart+gateLength, SigEnd+gateLength, step=gateLength)
gateC = (gatesStart + gatesEnd)/2

v_exp = EDM.ExpectedVelocity(distance, gateC) #in m/s, gate in ms
v_expMOT = EDM.ExpectedVelocity(MOTdistance, gateC)

#%% Get rest frame setpoint
print("Get rest frame velocity of the gate range:")
RestSP, RestSPerr, RestGatedTOFSP, RestWM, RestWMerr, RestGatedTOFWM =\
    EDM.FitGatedTOFOn(DataRef, SigStart, SigEnd, BkgStart, BkgEnd, distance)

print("Zero velocity setpoint = %.3g +- %.2g V"%\
      (RestSP, RestSPerr))

if RestWM != 0:
    print("zero velocity frequency = %.10g THz +- %.2g MHz"%\
           (RestWM, RestWMerr))

#%% Get TOFs, averaged per point
"""  Must run before proceeding  """
TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1),\
             label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1),\
             label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF]), \
               ymax=np.max(DataOffSPP[0][shot_for_TOF]),\
                   linestyles="dashed", colors="black")
    plt.title("TOF of the 20th scan point, averaged for all shots")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.legend()
    plt.show()

#% Bkg-sub TOF
BkgOn = np.average(DataOnSPP[0][0][BkgStartIndex:BkgEndIndex])
BkgOff = np.average(DataOffSPP[0][0][BkgStartIndex:BkgEndIndex])

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF],\
                                        axis=1)-BkgOn, label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF],\
                                         axis=1)-BkgOff, label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF])-BkgOn, \
               ymax=np.max(DataOffSPP[0][shot_for_TOF])-BkgOff,\
                   linestyles="dashed", colors="black")
    plt.title("TOF of the 20th scan point, averaged for all shots,\n \
              background subtracted")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.ylim(-0.3, 3.5)
    plt.legend()
    plt.show()

#%% Gated TOF with rolling gates
FitResultsOn, FitResultsOff, FittedGatedTOFs,\
    FitResultsOnWM, FitResultsOffWM, FittedGatedTOFsWM = EDM.FitGatedTOFperGate(Data,\
                                            DataOnSPP[0], TimeOnSPP,\
                                            DataOffSPP[0], TimeOffSPP,\
                       gatesStart, gatesEnd, gateC, BkgStart, BkgEnd,\
                       HasWM, distance, display=False)

#To show the plots, put it in console directly. Otherwise it will display all 
#plots (raw data, with one fit, and with both fits). Can export figures later.
#%% Convert to velocity distribution
velocityFig, meanVOn, meanVOnerr, meanVOff, meanVOfferr = \
    EDM.VelocityOnOff(FitResultsOn, FitResultsOff, RestSP, RestSPerr, v_exp,\
                      TCLconv, TCLconverr, wavelength, slowing_time, gateC, gateLength,\
                          file=fileLabel)
        
#%%
velocityFigWM, meanVOnWM, meanVOnerrWM, meanVOffWM, meanVOfferrWM = \
    EDM.VelocityOnOffWM(FitResultsOnWM, FitResultsOffWM, RestWM, RestWMerr, v_exp,\
                      f_iniTHz, wavelength, slowing_time, gateC, gateLength,\
                          file=fileLabel)

#%% Compare
diffV = meanVOn - meanVOff
diffVerr = np.sqrt(meanVOnerr**2 + meanVOfferr**2)
diffVWM = meanVOnWM - meanVOffWM
diffVerrWM = np.sqrt(meanVOnerrWM**2 + meanVOfferrWM**2)

plt.plot(gateC, diffV, '.', markersize=15, label="from setpoint")
plt.errorbar(gateC, diffV, yerr=diffVerr, capsize=3, fmt=' ')
plt.plot(gateC, diffVWM, '.', markersize=15, label="from WM")
plt.errorbar(gateC, diffVWM, yerr=diffVerrWM, capsize=3, fmt=' ')
plt.title("Velocity change, comparison of two calculations")
plt.xlabel("Gate center (ms)")
plt.ylabel("Velocity change (m/s)")
plt.legend()
plt.show()