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
month="August2025"
date="11"
subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolder
print(drive)

pattern="*Duration*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%%
if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    Data = {}
    fileLabels = []
    Lasers = []
    for i in range(0, len(files)):
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[0]
        Laser = re.split(r'[.]', re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[-1])[0]
        Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
        print("loaded file " + files[i])
        fileLabels.append(fileLabel)
        Lasers.append(Laser)

else:
    print("No matching files.")


#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 26
SigEnd = 27
BkgStart = 60
BkgEnd = 70

showTOF = False
shot_for_TOF = 20

showPlots = True

#%% Gated TOFs
Ons = {}
Offs = {}
Ratios = {}
Figs = {}
Fits = {}

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    
    #% Print out all params
    Settings = EDM.GetScanSettings(Scan)
    ScanParams = EDM.GetScanParameterArray(Scan)
    print(Settings)
    
    BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
    BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))
    
    TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Scan)
    
    OnBkgSub, OffBkgSub = EDM.GatedAvgCountsOnOff(Scan,\
                            DataOnSPP,DataOffSPP,TimeOnSPP,TimeOffSPP,\
                    SigStart,SigEnd,BkgStart,BkgEnd)
    
    Ratio = OnBkgSub/OffBkgSub
    
    Ons[fileLabels[i]] = OnBkgSub
    Offs[fileLabels[i]] = OffBkgSub
    Ratios[fileLabels[i]] = Ratio
    
    
    
    fig, fit_results = tools.Fitexp_decay(0, ScanParams, Ratio,\
                        p0=[1., 1000., 0.], xstep=Settings["end"]*1e-3, \
                plot=True, display=True, Toprint=False)
    
    Figs[fileLabels[i]] = fig
    Fits[fileLabels[i]] = fit_results
    


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
    plt.title("TOF of the 20th scan point, averaged for all shots")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.legend()
    plt.show()

#%% Bkg-sub TOF
BkgOn = np.average(DataOnSPP[0][0][BkgStartIndex:BkgEndIndex])
#BkgOff = np.average(DataOffSPP[0][0][BkgStartIndex:BkgEndIndex])

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1)\
             -BkgOn, label="On")
    #plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1)\
    #         -BkgOff, label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF])-BkgOn, \
               ymax=np.max(DataOnSPP[0][shot_for_TOF])-BkgOn,\
                   linestyles="dashed", colors="black")
    plt.title("TOF of the 20th scan point, averaged for all shots,\n \
              background subtracted")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.ylim(-0.3, 3.5)
    plt.legend()
    plt.show()

#%% Get gated TOF against scanned param with bkg sub
MeanCounts, StderrCounts, TimeWindow = EDM.GetGatedAvgCounts(Data,DataOnSPP[0],\
                                                    TimeOnSPP,SigStart,SigEnd)

BkgMeanCounts, BkgStderrCounts, BkgTimeWindow = EDM.GetGatedAvgCounts(Data,\
                                        DataOnSPP[0],TimeOnSPP,BkgStart,BkgEnd)
BkgSub = MeanCounts - BkgMeanCounts * TimeWindow/BkgTimeWindow

#% Plot gated TOF
GatedTOF = EDM.PlotGatedAvgCounts(Data,DataOnSPP[0],TimeOnSPP,SigStart,\
                                  SigEnd,BkgStart,BkgEnd)
#% Fitting

FittedGatedTOF, fit_results = tools.FitGaussian(GatedTOF, ScanParams,\
                                BkgSub, p0=[np.mean(ScanParams), 5., 20., 10.])
    
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