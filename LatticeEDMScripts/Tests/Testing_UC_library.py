# -*- coding: utf-8 -*-
"""
Created on Wed Aug  6 14:15:48 2025

@author: sl5119
"""

#%% Import libraries
import sys
import os

OneDriveFolder = os.environ['onedrive']
sys.path.append(OneDriveFolder + r"\Desktop\EDMSuite\LatticeEDMScripts")
import LatticeEDM_analysis_library as EDM

import csv
import numpy as np

import glob
import matplotlib.pyplot as plt
import scipy.signal as scisig
import pandas as pd

import tools as tools

tools.set_plots()

#%% Load data
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month=datadrive+"\\June2025\\"
date=month+"\\24\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

pattern="*_ProbeSetpointScan.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])


Data = EDM.ReadAverageScanInZippedXML(files[0])

#%% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

#%% Analysis settings
SigStart = 10
SigEnd = 40
BkgStart = 60
BkgEnd = 70

#%% Get TOFs, averaged per point
TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

plt.plot(np.average(DataOnSPP[0][20], axis=1))
plt.show()

#%% Get Gated TOF against scanned param with bkg sub
MeanCounts, StderrCounts, TimeWindow = EDM.GetGatedAvgCounts(Data,DataOnSPP[0],TimeOnSPP,SigStart,SigEnd)

BkgMeanCounts, BkgStderrCounts, BkgTimeWindow = EDM.GetGatedAvgCounts(Data,DataOnSPP[0],TimeOnSPP,BkgStart,BkgEnd)
BkgSub = MeanCounts - BkgMeanCounts * TimeWindow/BkgTimeWindow

#%%
EDM.PlotGatedAvgCounts(Data,DataOnSPP[0],TimeOnSPP,SigStart,SigEnd,BkgStart,BkgEnd)
#%%

