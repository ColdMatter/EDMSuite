# -*- coding: utf-8 -*-
"""
Created on Wed Aug  6 14:15:48 2025

TOF collector analysis, with slowing ON-OFF shots (or B-field up/down)
For 4-shot patterns.
Assuming 2 detectors -- PMT & Photodiode (PD)
- Data saved as 4 "detectors". Each have their own ON-OFF shots
- Detector 1: PMT, YAG ON
- Detector 2: PMT, YAG OFF
- Detector 3: PD, YAG ON
- Detector 4: PD, YAG OFF
Single-file analysis


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
#%% Load data (interactive)
file = EDM.get_scan()[0] #getting a list of [file_path, file_name, file_date]
print("Selected: ", file[1], " in ", file[2])
#%
files = [file[0]]  #Just so it still works with the old code
fileLabels = []
Data = {}

fileLabel = file[1][0:3]
Data[fileLabel] = EDM.ReadAverageScanInZippedXML(file[0])
print("loaded file " + file[1])
fileLabels.append(fileLabel)

print(fileLabel)

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 26
SigEnd = 40

#Use YAG-OFF shots for background subtraction

angle = 45
Resonance = 542.809124 #in THz, rest frame probe frequency

showTOF = True
showDiff = True

fTHz = 542
#%% Grouping data
Scan = Data[fileLabels[0]]

#% Print out all params
Settings = EDM.GetScanSettings(Scan)
ScanParams = EDM.GetScanParameterArray(Scan)
print("for file " + fileLabels[0])
print(Settings)

#%%
TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFs(Scan)

#%% For checking
point = 1

OnTOFbyShot = []
OffTOFbyShot = []

for i in range(0, len(DataOn)):
    OnTOFbyShot.append(DataOn[i][point])
    OffTOFbyShot.append(DataOff[i][point])

title = "TOF at point %g for shot #"%point

for i in range(0, len(OnTOFbyShot)):
    plt.plot(TimeOn*1000, OnTOFbyShot[i], label='On')
    plt.plot(TimeOff*1000, OffTOFbyShot[i], label='Off')
    plt.title(title + str(i))
    plt.xlabel("time (ms)")
    plt.ylabel("Signal (V)")
    plt.legend()
    plt.show()
    
#%% Grouping by ON/OFF and detector
PMTOnYAGOns = DataOn[0]
PMTOnYAGOffs = DataOn[1]

PMTOffYAGOns = DataOff[0]
PMTOffYAGOffs = DataOff[1]

#% YAG On-Off subtractions
SPP = Settings['shotsPerPoint']

PMTOns = PMTOnYAGOns - PMTOnYAGOffs
PMTOffs = PMTOffYAGOns - PMTOffYAGOffs

#%% check 2 -- passed
# =============================================================================
if showTOF:
    plt.plot(TimeOn*1000, PMTOnYAGOns[point]-PMTOnYAGOffs[point], label='On')
    plt.plot(TimeOff*1000, PMTOffYAGOns[point]-PMTOffYAGOffs[point], label='Off')
    plt.title(title + "\n YAG ON-OFF, PMT")
    plt.xlabel("time (ms)")
    plt.ylabel("Signal (V)")
    plt.legend()
    plt.show()

    plt.plot(TimeOn*1000, PMTOns[point], label='On')
    plt.plot(TimeOff*1000, PMTOffs[point], label='Off')
    plt.title(title + "\n YAG ON-OFF, PMT")
    plt.xlabel("time (ms)")
    plt.ylabel("Signal (V)")
    plt.legend()
    plt.show()

#%% Get gated TOFs
OnMeanCounts = EDM.GetGatedAvgCounts4Shot(Scan,\
                                    PMTOns,TimeOn,SigStart,SigEnd)
OffMeanCounts = EDM.GetGatedAvgCounts4Shot(Scan,\
                                    PMTOffs,TimeOff,SigStart,SigEnd)

#%% Get x axis (setpoint or WM reading)
ScanParams = EDM.GetScanParameterArray(Scan)
print(Settings)

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Scan)

if int(f_iniTHz) == fTHz:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Scan, plot=True, Toprint=False)
    HasWM = True
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
else:
    HasWM = False
    print("Wrong fibre in WM.")

#%% Plot
title="Gated TOF over " + Settings["param"] + " with " +\
    str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
        fileLabels[0]

if HasWM:
    plt.plot(f_relMHz, OffMeanCounts, label="Slowing Off")
    plt.plot(f_relMHz, OnMeanCounts, label="Slowing On")
    
    plt.xlabel("Relative frequency (MHz) to %g THz"%f_iniTHz)

else:
    plt.plot(ScanParams, OffMeanCounts, label="Slowing Off")
    plt.plot(ScanParams, OnMeanCounts, label="Slowing On")
    
    plt.xlabel("Setpoint (V)")
    
plt.ylabel("Gated TOF (ms.V)")
plt.title(title)
plt.legend()
plt.show()

#%% If it's a slowing measurement, convert X-axis to velocity
f_relRes = (f_relMHz/1e6 + f_iniTHz - Resonance) * 1e6
v = tools.VelocityfromFshift(f_relRes, Resonance, angle)

plt.plot(v, OffMeanCounts, label="Slowing Off")
plt.plot(v, OnMeanCounts, label="Slowing On")
    
plt.xlabel("Velocity (m/s)")
plt.ylabel("Gated TOF (ms.V)")
plt.title(title)
plt.legend()
plt.show()

#%% Normalise & fit
peakOn = v[np.where(OnMeanCounts == np.max(OnMeanCounts))[0][0]]
peakOff = v[np.where(OffMeanCounts == np.max(OffMeanCounts))[0][0]]

fitOn, covOn = curve_fit(tools.Gaussian_FWHM, v, OnMeanCounts, \
                         p0=[200., peakOn, 20., 100.])
errOn = np.sqrt(np.diag(covOn))

fitOff, covOff = curve_fit(tools.Gaussian_FWHM, v, OffMeanCounts, \
                         p0=[200., peakOff, 20., 100.])
errOff = np.sqrt(np.diag(covOff))

# Normalise to amplitude of Off fit & correct shift to 0
NormOff = (OffMeanCounts - fitOff[3])/fitOff[0]
NormOn = (OnMeanCounts - fitOn[3])/fitOff[0]

fitOnNorm, covOnNorm = curve_fit(tools.Gaussian_FWHM, v, NormOn, \
                         p0=[200., peakOn, 20., 100.])
errOnNorm = np.sqrt(np.diag(covOnNorm))
print("Central ON velocity = %.4g +- %.3g m/s"%(fitOnNorm[1], errOnNorm[1]) +\
      "\n with FWHM = %.4g +- %.3g m/s"%(fitOnNorm[2], errOnNorm[2]))

fitOffNorm, covOffNorm = curve_fit(tools.Gaussian_FWHM, v, NormOff, \
                         p0=[200., peakOff, 20., 100.])
errOffNorm = np.sqrt(np.diag(covOffNorm))
print("Central OFF velocity = %.4g +- %.3g m/s"%(fitOffNorm[1], errOffNorm[1]) +\
      "\n with FWHM = %.4g +- %.3g m/s"%(fitOffNorm[2], errOffNorm[2]))

vspan = np.arange(np.min(v), np.max(v), 0.1)    

peakShift = fitOnNorm[1] - fitOffNorm[1]
peakShifterr = peakShift * (errOnNorm[1]/fitOnNorm[1] + errOffNorm[1]/fitOffNorm[1])

title2 = str(Settings["slowing time"]/1000) + \
    " ms slowing with Gated TOF normalised over off-shot fit amplitude,\n " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
        fileLabels[0] + ", %.3g +- %.2g m/s peak shift"%(peakShift, peakShifterr)

plt.plot(v, NormOff, '.', color=colors[0], label="Slowing Off")
plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitOffNorm), color=colors[0])
plt.plot(v, NormOn, '.', color=colors[1], label="Slowing On")
plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitOnNorm), color=colors[1])

plt.xlabel("Velocity (m/s)")
plt.ylabel("Normalised Gated TOF")
plt.title(title2)
plt.legend()
plt.show()