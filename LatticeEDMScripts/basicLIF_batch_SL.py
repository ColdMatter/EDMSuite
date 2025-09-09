# -*- coding: utf-8 -*-
"""
Created on Wed Aug  6 14:15:48 2025

To analyse basic LIF measurements. Typically no On-Off shots (only On)

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

#%% Load data
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month=datadrive+"\\August2025\\"
date=month+"\\17\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

pattern="*_ProbeSetpointScan*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])


if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    Data = {}
    fileLabels = []
    locations = []
    for i in range(0, len(files)):
        location = re.split(r'[.]', re.split(r'[_]',\
                                    re.split(r'[\\]', files[i])[-1])[-1])[0]
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[0]
        Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
        print("loaded file " + files[i])
        fileLabels.append(fileLabel)
        locations.append(location)

else:
    print("No matching files.")

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 25
SigEnd = 27
BkgStart = 60
BkgEnd = 70

showTOF = False
shot_for_TOF = 20
    
#%%
FittedGatedTOFs = {}
Fit_results = {}
FittedGatedTOFsWM = {}
Fit_resultsWM = {}
HasWMs = {}

for i in range(0, len(files)):
    Data = EDM.ReadAverageScanInZippedXML(files[0])
    FittedGatedTOF, fit_results, FittedGatedTOFWM, fit_resultsWM, HasWM = \
        EDM.ResonanceFreq(Data, SigStart, SigEnd, BkgStart, BkgEnd,\
                      showTOF=False, showPlot=True, fileLabel=fileLabels[i],\
                          location=locations[i],\
                          shot_for_TOF=0, freqTHz=542)
    
    FittedGatedTOFs[fileLabels[i]] = FittedGatedTOF
    Fit_results[fileLabels[i]] = fit_results
    FittedGatedTOFsWM[fileLabels[i]] = FittedGatedTOFWM
    Fit_resultsWM[fileLabels[i]] = fit_resultsWM
    HasWMs[fileLabels[i]] = HasWM

#%%
TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=True)

#%% angled probe, for velocity distribution calculation
pattern="*_angledProbeSetpointScan*.zip"
files_angled = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files_angled])


if len(files_angled) > 0:
    print("%g matching files found. Loading"%len(files_angled))
    Data_angled = {}
    fileLabels_angled = []
    locations_angled = []
    for i in range(0, len(files_angled)):
        location = re.split(r'[.]', re.split(r'[_]',\
                                    re.split(r'[\\]', files_angled[i])[-1])[-1])[0]
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files_angled[i])[-1])[0]
        Data_angled[fileLabel] = EDM.ReadAverageScanInZippedXML(files_angled[i])
        print("loaded file " + files[i])
        fileLabels_angled.append(fileLabel)
        locations_angled.append(location)

else:
    print("No matching files.")

#%% Rolling gate for angled probe, to get resonance freq per gate

