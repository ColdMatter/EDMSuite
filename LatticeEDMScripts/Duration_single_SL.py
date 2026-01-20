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
month="September2025"
date="29"
subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolder
print(drive)

pattern="*015*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%%
Data = EDM.ReadAverageScanInZippedXML(files[0])

#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 25
SigEnd = 27
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
plt.ylim(0, 1)
plt.show()


#%%
fig, fit_results = tools.Fitexp_decay(0, ScanParams, Ratio,\
                    p0=[1., 5000., 0.], xstep=Settings["end"]*1e-3, \
            plot=True, display=True, Toprint=True)

#Figs[fileLabels[i]] = fig
#Fits[fileLabels[i]] = fit_results

#%% for off-dupoint
fig, fit_results = tools.Fitinverse_exp_decay(0, ScanParams, Ratio,\
                    p0=[0.3, 500., 0.4], xstep=Settings["end"]*1e-3, \
            plot=True, display=True, Toprint=True, \
                title = "V3 pump-in", xlabel='V3 duration (μs)',\
                    ylabel='Population remaining in X, v=0, N=1')

#%
a = fit_results['best fit'][0]
b = 1 - fit_results['best fit'][2]

da = fit_results['error'][0]
db = fit_results['error'][2]

err = np.sqrt((da/b)**2 + (db * a/b**2)**2)

print(a/b)
print(err)

#%% 
a = np.array([0.292, 0.236, 0.365, 0.220, 0.434, 0.328, 0.225, 0.424])
da = np.array([0.026, 0.039, 0.048, 0.022, 0.097, 0.015, 0.036, 0.331])

bg = np.array([0.10, 0.15, 0.29, 0.17, 0.41, 0.35, 0.13, 0.15])
dbg = np.array([0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.01, 0.02])

print(a)
print(1-bg-a)

bRa = (1-a-bg)/a
print(bRa)

dbRa = np.sqrt((da*(1-bg)/a**2)**2 + (dbg/a)**2)
print(dbRa)

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
shot_for_TOF = 6
BkgOn = np.average(DataOnSPP[0][shot_for_TOF][BkgStartIndex:BkgEndIndex])
BkgOff = np.average(DataOffSPP[0][shot_for_TOF][BkgStartIndex:BkgEndIndex])
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
window_size = 100

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
    plt.xlim(20, 80)
    plt.ylim(-0.05, 0.5)
    plt.legend()
    plt.show()

#%% Difference
TOFshots = [25, 27, 31]
window_size = 100
for i in TOFshots:
    BkgOn = np.average(DataOnSPP[0][i][BkgStartIndex:BkgEndIndex])
    BkgOff = np.average(DataOffSPP[0][i][BkgStartIndex:BkgEndIndex])
    final_list_On = tools.MovingAverage(window_size, \
                                np.average(DataOnSPP[0][i], axis=1))
    final_list_Off = tools.MovingAverage(window_size, \
                                np.average(DataOffSPP[0][i], axis=1))
    final_list_diff = (final_list_On-BkgOn) - (final_list_Off-BkgOff)
    final_MAtimeOnlist = tools.MovingAverage(window_size,TimeOnSPP*1000)
    final_MAtimeOfflist = tools.MovingAverage(window_size,TimeOffSPP*1000)

    plt.plot(final_MAtimeOnlist, final_list_diff,\
             label='duration = %g us'%ScanParams[i])
    #plt.plot(final_MAtimeOfflist, final_list_Off\
     #        -BkgOff, label="Off")
plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
           ymin=np.min(DataOnSPP[0][i])-BkgOn, \
           ymax=np.max(DataOnSPP[0][i])-BkgOn,\
               linestyles="dashed", colors="black")
plt.hlines(0, xmin=25, xmax=80, color='red', linestyles='dashed')
plt.title("TOF differences, averaged for all shots \
          \n background subtracted, moving average of %g"%\
          window_size)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.xlim(28, 50)
plt.ylim(-0.04, 0.08)
plt.legend()
plt.show()
    

#%% Get gated TOF against scanned param with bkg sub
SigStart = 31
SigEnd = 44

OnBkgSub, OffBkgSub = EDM.GatedAvgCountsOnOff(Data,\
                        DataOnSPP[0],DataOffSPP[0],TimeOnSPP,TimeOffSPP,\
                SigStart,SigEnd,BkgStart,BkgEnd)

#% Plot gated TOF
GatedTOF = EDM.PlotGatedAvgCountsOnOff(Data,DataOnSPP[0],DataOffSPP[0],\
                                       TimeOnSPP,TimeOffSPP,\
                SigStart,SigEnd,BkgStart,BkgEnd,error=False, display=True,\
                    extraTitle=" ")

#%% Plot Difference
plt.plot(ScanParams, OnBkgSub-OffBkgSub, '.')
plt.xlabel("V0 slowing duration (us)")
plt.ylabel("Gated LIF (ms.V)")
plt.title("Gated TOF difference, over " +\
          str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
              str(SigStart) + "ms to " + str(SigEnd) + "ms gate")
plt.show()
    
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