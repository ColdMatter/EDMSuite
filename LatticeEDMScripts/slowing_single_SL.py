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
month=datadrive+"\\August2025\\"
date=month+"\\07\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

pattern="*_slowing_*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%%
Data = EDM.ReadAverageScanInZippedXML(files[1])
print("loaded file " + files[1])

#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

slowing_time = Settings["slowing time"]/1000
print("Slowing duration is " + str(slowing_time) + " ms")

#%% Analysis settings
SigStart = 20 #in ms
SigEnd = 40
BkgStart = 60
BkgEnd = 70

showTOF = True
shot_for_TOF = 20

BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

#%% Get TOFs, averaged per point
TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1), label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1), label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd], ymin=np.min(DataOnSPP[0][shot_for_TOF]), \
               ymax=np.max(DataOffSPP[0][shot_for_TOF]), linestyles="dashed", colors="black")
    plt.title("TOF of the 20th scan point, averaged for all shots")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.legend()
    plt.show()

#%% Bkg-sub TOF
BkgOn = np.average(DataOnSPP[0][0][BkgStartIndex:BkgEndIndex])
BkgOff = np.average(DataOffSPP[0][0][BkgStartIndex:BkgEndIndex])

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1)-BkgOn, label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1)-BkgOff, label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd], ymin=np.min(DataOnSPP[0][shot_for_TOF])-BkgOn, \
               ymax=np.max(DataOffSPP[0][shot_for_TOF])-BkgOff, linestyles="dashed", colors="black")
    plt.title("TOF of the 20th scan point, averaged for all shots,\n \
              background subtracted")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.ylim(-0.3, 3.5)
    plt.legend()
    plt.show()

#%% Get gated TOF against scanned param with bkg sub
OnMeanCounts, OnStderrCounts, OnTimeWindow = EDM.GetGatedAvgCounts(Data,DataOnSPP[0],TimeOnSPP,SigStart,SigEnd)

OnBkgMeanCounts, OnBkgStderrCounts, OnBkgTimeWindow = EDM.GetGatedAvgCounts(Data,DataOnSPP[0],TimeOnSPP,BkgStart,BkgEnd)
OnBkgSub = OnMeanCounts - OnBkgMeanCounts * OnTimeWindow/OnBkgTimeWindow

OffMeanCounts, OffStderrCounts, OffTimeWindow = EDM.GetGatedAvgCounts(Data,DataOffSPP[0],TimeOffSPP,SigStart,SigEnd)

OffBkgMeanCounts, OffBkgStderrCounts, OffBkgTimeWindow = EDM.GetGatedAvgCounts(Data,DataOffSPP[0],TimeOffSPP,BkgStart,BkgEnd)
OffBkgSub = OffMeanCounts - OffBkgMeanCounts * OffTimeWindow/OffBkgTimeWindow

#% Plot gated TOF
if showTOF:
    GatedOnOFFTOF = EDM.PlotGatedAvgCountsOnOff(Data,DataOnSPP[0],DataOffSPP[0],\
                        TimeOnSPP,TimeOffSPP,SigStart,SigEnd,BkgStart,BkgEnd,error=False)
#%% Fitting

FittedGatedTOF, fit_results = tools.FitGaussian(GatedTOF, ScanParams, BkgSub, p0=[np.mean(ScanParams), 5., 10., 1.])