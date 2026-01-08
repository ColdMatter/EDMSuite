# -*- coding: utf-8 -*-
"""
Created on Wed Aug  6 14:15:48 2025

To analyse setpoint scan measurements. Typically with On-Off shots (only On)

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
from scipy.optimize import curve_fit


import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Load data
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month=datadrive+"\\October2025\\"
date=month+"\\23\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

pattern="002*.zip"
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

showTOF = True
shot_for_TOF = 17

#%%
Data = EDM.ReadAverageScanInZippedXML(files[0])

#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Data)

if int(f_iniTHz) == 542:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=True, Toprint=False)
    HasWM = True
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
else:
    HasWM = False
    print("Wrong fibre in WM.")

#%% Get TOFs, averaged per point
TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1),\
             label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1),\
             label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF]), \
               ymax=np.max(DataOnSPP[0][shot_for_TOF]),\
                   linestyles="dashed", colors="black")
    plt.title("TOF of scan point %g, averaged for all shots"%shot_for_TOF)
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.legend()
    plt.show()

#%% Bkg-sub TOF
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
    plt.title("TOF of the 20th scan point, averaged for all shots,\n \
              background subtracted")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.ylim(-0.3, 3.5)
    plt.legend()
    plt.show()

#%% Get gated TOF against scanned param with bkg sub
OnBkgSub, OffBkgSub = EDM.GatedAvgCountsOnOff(Data,DataOnSPP[0],DataOffSPP[0],\
                                              TimeOnSPP,TimeOffSPP,\
                SigStart,SigEnd,BkgStart,BkgEnd)

title="Gated TOF over " + Settings["param"] + " with " +\
    str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
        fileLabel
    
plt.plot(ScanParams, OnBkgSub, '.', label='On')
plt.plot(ScanParams, OffBkgSub, '.', label='Off')
plt.xlabel('Setpoint (V)')
plt.ylabel("Gated LIF (ms.V)")
plt.title(title)
plt.legend()
plt.show()    

#%% Fitting
peakOn = ScanParams[np.where(OnBkgSub == np.min(OnBkgSub))[0][0]]
fitOn, covOn = curve_fit(tools.SkewedGaussian, ScanParams, OnBkgSub, p0=[peakOn, 1., -1., 0.5, 1.])
errOn = np.sqrt(np.diag(covOn))
print("Central ON setpoint = %.4g +- %.3g V"%(fitOn[0], errOn[0]) +\
      "\n with FWHM = %.4g +- %.3g V"%(fitOn[1]*2*np.sqrt(2*np.log(2)),\
                                         errOn[1]*2*np.sqrt(2*np.log(2))))

peakOff = ScanParams[np.where(OffBkgSub == np.max(OffBkgSub))[0][0]]
fitOff, covOff = curve_fit(tools.SkewedGaussian, ScanParams, OffBkgSub, p0=[peakOff, 1., -1., 0.5, 1.])
errOff = np.sqrt(np.diag(covOff))
print("Central OFF setpoint = %.4g +- %.3g V"%(fitOff[0], errOff[0]) +\
      "\n with FWHM = %.4g +- %.3g V"%(fitOff[1]*2*np.sqrt(2*np.log(2)),\
                                         errOff[1]*2*np.sqrt(2*np.log(2))))

vspan = np.arange(np.min(ScanParams), np.max(ScanParams), step = 0.01)

plt.plot(ScanParams, OnBkgSub, '.', label='On', color=colors[0])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOn), color=colors[0])
plt.plot(ScanParams, OffBkgSub, '.', label='Off', color=colors[1])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOff), color=colors[1])
plt.xlabel('Setpoint (V)')
plt.ylabel("Gated TOF (ms.V)")
plt.title(title)
plt.legend()
plt.show()

if HasWM:
    peakOn = f_relMHz[np.where(OnBkgSub == np.max(OnBkgSub))[0][0]]
    peakOff = f_relMHz[np.where(OffBkgSub == np.max(OffBkgSub))[0][0]]
    
    
#%% Ratio
Ratio = OnBkgSub/OffBkgSub

peakR = f_relMHz[np.where(Ratio == np.min(Ratio))[0][0]]

fitR, covR = curve_fit(tools.Gaussian, f_relMHz, Ratio, p0=[peakR, 30., -0.5, 1.])
errR = np.sqrt(np.diag(covR))

fspan = np.arange(np.min(f_relMHz), np.max(f_relMHz), step = 1.)

plt.plot(f_relMHz, Ratio, '.', label='On/Off', color=colors[0])
plt.plot(fspan, tools.Gaussian(fspan, *fitR))
#plt.plot(ScanParams, OffBkgSub, '.', label='Off', color=colors[1])
#plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOff), color=colors[1])
plt.xlabel("Relative frequency (MHz) to %g THz"%f_iniTHz)
plt.ylabel("Ratio")
plt.title(title)
plt.legend()
plt.show()       

print("Gaussian fit params [mean, std, amp, shift]: ", fitR)
print("Errors of fit [mean, std, amp, shift]: ", errR)