# -*- coding: utf-8 -*-
"""
Created on Mon Aug 11 10:19:10 2025

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
month=datadrive+"\\August2025\\"
date=month+"\\12\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

#%% Basic analysis params
SigStart = 25 #in ms
SigEnd = 32
BkgStart = 60
BkgEnd = 80

distance = 1.3 # m
MOTdistance = 1.8 

gateLength = 1.
gatesStart = np.arange(SigStart, SigEnd, step=gateLength)
gatesEnd = np.arange(SigStart+gateLength, SigEnd+gateLength, step=gateLength)
gateC = (gatesStart + gatesEnd)/2

v_exp = EDM.ExpectedVelocity(distance, gateC) #in m/s, gate in ms

wavelength = 552

#%% Load and analyse perpendicular probe data
"""Get zero-velocity frequency for V0. Must have
"""
pattern="022*.zip"
Reffiles = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in Reffiles])


if len(Reffiles) > 0:
    Ref = True
    DataRef = EDM.ReadAverageScanInZippedXML(Reffiles[0])
    SettingsRef = EDM.GetScanSettings(DataRef)
    ScanParamsRef = EDM.GetScanParameterArray(DataRef)
    
    f_iniTHzRef, f_relMHzRef = EDM.GetScanFreqArrayMHz(DataRef)
    if int(f_iniTHzRef) == 542:
        TCL_WM_cali = EDM.TCL_WM_Calibration(DataRef, plot=True)
        HasWM = True
        RefTCLconv = TCL_WM_cali['best fit'][0]
        RefTCLconverr = TCL_WM_cali['error'][0]
        print("TCL calibration = %.4g +- %.2g MHz"%(RefTCLconv, RefTCLconverr))
    else:
        HasWM = False
        print("Wrong fibre in WM.")
        
    print("Get rest frame velocity of the gate range:")
    RestSP, RestSPerr, RestGatedTOFSP, RestWM, RestWMerr, RestGatedTOFWM =\
        EDM.FitGatedTOFOn(DataRef, SigStart, SigEnd, BkgStart, BkgEnd, distance)

    print("Zero velocity setpoint = %.3g +- %.2g V"%\
          (RestSP, RestSPerr))

    if RestWM != 0:
        print("zero velocity frequency = %.10g THz +- %.2g MHz"%\
               (RestWM, RestWMerr))
    
else:
    Ref = False
    RestWM = 542.8091201 #THz, from June 25 2025
    RestWMerr = 0.43 #MHz
    print("No rest frame data available on this day.")
    print("Use default rest frame frequency, 25-30ms gate: %.10g THz +- %.2g MHz"%\
           (RestWM, RestWMerr))
        
#%% Load all data
pattern="*V0Probe_setpointscan_V0V1V2V3MWP2v1v2*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

Data = {}
fileLabels = []
for i in range(0, len(files)):
    fileLabel = re.split(r'[\\]', files[i])[-1][0:3]
    Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
    print("loaded file " + files[i])
    fileLabels.append(fileLabel)
    
#%% Get peak velocities
'''This is the old way with rolling gates'''
PeakVOn = {}
PeakVOnerr = {}
PeakVOff = {}
PeakVOfferr = {}

velocityFigs = {}
AllFittedGatedTOFs = {}
slowing_times = {}

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    velocityFig, meanVOn, meanVOnerr, meanVOff, meanVOfferr, \
        FittedGatedTOFs, FittedGatedTOFsWM, slowing_time =\
        EDM.Slowing(Scan, fileLabels[i], SigStart, SigEnd, BkgStart, BkgEnd,\
                gatesStart, gatesEnd, gateC, gateLength, distance,\
                RestSP, RestSPerr, v_exp, wavelength, Ref,\
                RefTCLconv, RefTCLconverr,\
                showTOF=False, TOFshot=20, detector=0, showFitTOF=True, \
                    plotFitTOF=True, showVelocity=True)
    
    PeakVOn[fileLabels[i]] = meanVOn
    PeakVOnerr[fileLabels[i]] = meanVOnerr
    PeakVOff[fileLabels[i]] = meanVOff
    PeakVOfferr[fileLabels[i]] = meanVOfferr
    
    velocityFigs[fileLabels[i]] = velocityFig
    AllFittedGatedTOFs[fileLabels[i]] = FittedGatedTOFs
    slowing_times[fileLabels[i]] = slowing_time
    

#%% Get change in peak velocities
for i in range(0, len(files)):
    diffV = PeakVOn[fileLabels[i]] - PeakVOff[fileLabels[i]]
    diffVerr = np.sqrt(PeakVOnerr[fileLabels[i]]**2 + PeakVOfferr[fileLabels[i]]**2)
    
    plt.figure()
    plt.plot(gateC, diffV, '.', markersize=15, label="from setpoint")
    plt.errorbar(gateC, diffV, yerr=diffVerr, capsize=3, fmt=' ')
    plt.title("Velocity change of file " + fileLabels[i] +\
              " with %g ms slowing"%slowing_times[fileLabels[i]])
    plt.xlabel("Gate center (ms)")
    plt.ylabel("Velocity change (m/s)")
    plt.legend()
    plt.show()
    plt.close()