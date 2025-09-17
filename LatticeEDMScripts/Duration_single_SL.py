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
import pandas as pd

import glob
import matplotlib.pyplot as plt


import tools as tools

tools.set_plots()

#%% Load data
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month="August2025"
date="12"
subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolder
print(drive)

pattern="*Duration_*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%%
Data = EDM.ReadAverageScanInZippedXML(files[-1])

#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 40
SigEnd = 50
BkgStart = 70
BkgEnd = 80

showTOF = True
shot_for_TOF = 25

BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

#f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Data)

#%%
#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

OnBkgSub, OffBkgSub = EDM.GatedAvgCountsOnOff(Data,\
                        DataOnSPP[0],DataOffSPP[0],TimeOnSPP,TimeOffSPP,\
                SigStart,SigEnd,BkgStart,BkgEnd)

Ratio = OnBkgSub/OffBkgSub

#Ons[fileLabels[i]] = OnBkgSub
#Offs[fileLabels[i]] = OffBkgSub
#Ratios[fileLabels[i]] = Ratio

plt.plot(ScanParams, Ratio, '.')
plt.xlabel("V0 slowing duration (us)")
plt.ylabel("ratio")
plt.title("Gated TOF over " +\
   str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
   str(SigStart) + "ms to " + str(SigEnd) + "ms gate")
#plt.xlim(0, 9800)
plt.ylim(0, 1.5)
plt.show()


#%%
fig, fit_results = tools.Fitexp_decay(0, ScanParams, Ratio,\
                    p0=[1., 5000., 0.], xstep=Settings["end"]*1e-3, \
            plot=True, display=True, Toprint=True)

#Figs[fileLabels[i]] = fig
#Fits[fileLabels[i]] = fit_results

#%% See TOF at certain V0 duration range

#%% Get TOFs, averaged per point
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
    plt.title("TOF of scan point %g, duration = %g us, averaged for all shots"%\
              (shot_for_TOF, ScanParams[shot_for_TOF]))
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.legend()
    plt.show()

#%% Bkg-sub TOF
BkgOn = np.average(DataOnSPP[0][0][BkgStartIndex:BkgEndIndex])
BkgOff = np.average(DataOffSPP[0][0][BkgStartIndex:BkgEndIndex])

shot_for_TOF = 2

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1)\
             -BkgOn, label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1)\
             -BkgOff, label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF])-BkgOn, \
               ymax=np.max(DataOnSPP[0][shot_for_TOF])-BkgOn,\
                   linestyles="dashed", colors="black")
    plt.title("TOF of scan point %g, duration = %g us, averaged for all shots \
              \n background subtracted"%\
              (shot_for_TOF, ScanParams[shot_for_TOF]))
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.ylim(-0.1, 0.1)
    plt.legend()
    plt.show()

#% Moving average
window_size = 10

final_list_On = tools.MovingAverage(window_size, \
                            np.average(DataOnSPP[0][shot_for_TOF], axis=1))
final_list_Off = tools.MovingAverage(window_size, \
                            np.average(DataOffSPP[0][shot_for_TOF], axis=1))

final_MAtimeOnlist = tools.MovingAverage(window_size,TimeOnSPP*1000)
final_MAtimeOfflist = tools.MovingAverage(window_size,TimeOffSPP*1000)

if showTOF:
    plt.plot(final_MAtimeOnlist, final_list_On\
             -BkgOn, label="On")
    plt.plot(final_MAtimeOfflist, final_list_Off\
             -BkgOff, label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF])-BkgOn, \
               ymax=np.max(DataOnSPP[0][shot_for_TOF])-BkgOn,\
                   linestyles="dashed", colors="black")
    plt.title("TOF of scan point %g, duration = %g us, averaged for all shots \
              \n background subtracted, moving average of %g"%\
              (shot_for_TOF, ScanParams[shot_for_TOF], window_size))
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.ylim(-0.025, 0.1)
    plt.legend()
    plt.show()

#%% Get gated TOF against scanned param with bkg sub
OnBkgSub, OffBkgSub = EDM.GatedAvgCountsOnOff(Data,\
                        DataOnSPP[0],DataOffSPP[0],TimeOnSPP,TimeOffSPP,\
                SigStart,SigEnd,BkgStart,BkgEnd)

#% Plot gated TOF
GatedTOF = EDM.PlotGatedAvgCountsOnOff(Data,DataOnSPP[0],DataOffSPP[0],\
                                       TimeOnSPP,TimeOffSPP,\
                SigStart,SigEnd,BkgStart,BkgEnd,error=False, display=True,\
                    extraTitle=" ")

#%% Plot ratio
    
#%% Fitting

FittedGatedTOF, fit_results = tools.FitGaussian(0, ScanParams,\
                                OnBkgSub/OffBkgSub, \
                                    p0=[np.mean(ScanParams), 500., 1., 0.],\
                        xlabel="V0 slowing duration (us)", ylabel="ratio",\
        title="Gated TOF over " +\
        str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
        str(SigStart) + "ms to " + str(SigEnd) + "ms gate")
    
#%% TCL-WM calibration
if Settings["param"] == 'setpoint':
     Xunit = " (V)"
else:
     Xunit = ""
xlabel = Settings["channel"] + " " + Settings["param"] + Xunit
ylabel = "Relative frequency from %.8g THz"%f_iniTHz

plt.plot(ScanParams, f_relMHz, '.')
plt.title("TCL-WM calibration")
plt.xlabel(xlabel)
plt.ylabel(ylabel)
plt.show()

#%%
TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=True)