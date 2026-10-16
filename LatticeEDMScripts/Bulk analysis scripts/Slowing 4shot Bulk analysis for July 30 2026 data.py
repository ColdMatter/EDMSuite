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
# file = EDM.get_scan()[0] #getting a list of [file_path, file_name, file_date]
# print("Selected: ", file[1], " in ", file[2])
# #%
# files = [file[0]]  #Just so it still works with the old code
# fileLabels = []
# Data = {}

# fileLabel = file[1][0:3]
# Data[fileLabel] = EDM.ReadAverageScanInZippedXML(file[0])
# print("loaded file " + file[1])
# fileLabels.append(fileLabel)

# print(fileLabel)

#%% Load data
datadrive = r"C:\Users\sl5119\Box\LatticeEDM\data"
month = "July 2026"
date = "30"
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\"# + subfolder
print(drive)

pattern="*slowing*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%% Selection
V04fRQ = ["002", "013", "009", "014"]
MWand4f = ["007", "012", "008", "016"]
OnlyMW = ["006", "011", "010", "017"]

V04fRQ2 = ["015", "018", "009"]
V0V14fR = ["019", "020"]

sele = V04fRQ + MWand4f + OnlyMW + V04fRQ2 + V0V14fR
print("Selected files: ", sele)

#%%
if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    Data = {}
    fileLabels = []
    Lasers = []
    for i in range(0, len(files)):
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[0]
        Laser = re.split(r'[.]', re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[-2])[0]
       
       #Use this part if have selections
        for j in range(0, len(sele)):
            if fileLabel == sele[j]:
                print("File "+fileLabel+" selected")
       ###
                Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
                print("loaded file " + files[i])
                fileLabels.append(fileLabel)
                Lasers.append(Laser)

else:
    print("No matching files.")

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 29 # in ms
SigEnd = 40

#Use YAG-OFF shots for background subtraction

angle = 45 # Angle of probe to slowing
Resonance = 542.809124 #in THz, rest frame probe frequency

showTOF = False # If true, plots an example TOF before and after bkg subtraction
showIntermediate = True # If true, show unnormalised data with frequency/setpoint axis
showProcessed = False # If true, show final processed plot with fit

fTHz = 542

point = 1 # This is for TOF check

#%%
FitResNormON = {}
FitResNormOFF = {}
PeakShifts = {}
SlowTimes = {}

#%% Process all
for f in range(0, len(fileLabels)):
    Scan = Data[fileLabels[f]]
    
    #% Print out all params
    Settings = EDM.GetScanSettings(Scan)
    ScanParams = EDM.GetScanParameterArray(Scan)
    print("for file " + fileLabels[f])
    print(Settings)
    
    tSlow = Settings["slowing time"]/1000 # in ms
    SlowTimes[fileLabels[f]] = tSlow

# Get data, individual shots
    TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFs(Scan)

# For checking
    OnTOFbyShot = []
    OffTOFbyShot = []
    
    for i in range(0, len(DataOn)):
        OnTOFbyShot.append(DataOn[i][point])
        OffTOFbyShot.append(DataOff[i][point])
    
    if showTOF: 
        title = "TOF at point %g for shot #"%point
        
        for i in range(0, len(OnTOFbyShot)):
            plt.plot(TimeOn*1000, OnTOFbyShot[i], label='On')
            plt.plot(TimeOff*1000, OffTOFbyShot[i], label='Off')
            plt.title(title + str(i))
            plt.xlabel("time (ms)")
            plt.ylabel("Signal (V)")
            plt.legend()
            plt.show()
    
# Grouping by ON/OFF and detector
    PMTOnYAGOns = DataOn[0]
    PMTOnYAGOffs = DataOn[1]
    
    PMTOffYAGOns = DataOff[0]
    PMTOffYAGOffs = DataOff[1]
    
    #% YAG On-Off subtractions
    SPP = Settings['shotsPerPoint']
    
    PMTOns = PMTOnYAGOns - PMTOnYAGOffs
    PMTOffs = PMTOffYAGOns - PMTOffYAGOffs

# check 2 
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

# Get gated TOFs
    OnMeanCounts = EDM.GetGatedAvgCounts4Shot(Scan,\
                                        PMTOns,TimeOn,SigStart,SigEnd)
    OffMeanCounts = EDM.GetGatedAvgCounts4Shot(Scan,\
                                        PMTOffs,TimeOff,SigStart,SigEnd)

# Get x axis (setpoint or WM reading)
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

# Plot
    if showIntermediate: 
        title="Gated TOF over " + Settings["param"] + " with " +\
            str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
            str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
                fileLabels[f]
        
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

# For slowing measurements, convert X-axis to velocity
    f_relRes = (f_relMHz/1e6 + f_iniTHz - Resonance) * 1e6
    v = tools.VelocityfromFshift(f_relRes, Resonance, angle)
    
    if showIntermediate:
        plt.plot(v, OffMeanCounts, label="Slowing Off")
        plt.plot(v, OnMeanCounts, label="Slowing On")
            
        plt.xlabel("Velocity (m/s)")
        plt.ylabel("Gated TOF (ms.V)")
        plt.title(title)
        plt.legend()
        plt.show()

# Normalise & fit
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
                             p0=[0.2, peakOn, 20., 0.])
    errOnNorm = np.sqrt(np.diag(covOnNorm))
    print("Central ON velocity = %.4g +- %.3g m/s"%(fitOnNorm[1], errOnNorm[1]) +\
          "\n with FWHM = %.4g +- %.3g m/s"%(fitOnNorm[2], errOnNorm[2]))
    
    fitOffNorm, covOffNorm = curve_fit(tools.Gaussian_FWHM, v, NormOff, \
                             p0=[0.2, peakOff, 20., 0.])
    errOffNorm = np.sqrt(np.diag(covOffNorm))
    print("Central OFF velocity = %.4g +- %.3g m/s"%(fitOffNorm[1], errOffNorm[1]) +\
          "\n with FWHM = %.4g +- %.3g m/s"%(fitOffNorm[2], errOffNorm[2]))
    
    vspan = np.arange(np.min(v), np.max(v), 0.1)    
    
    peakShift = fitOnNorm[1] - fitOffNorm[1]
    peakShifterr = np.abs(peakShift) * (np.abs(errOnNorm[1])/fitOnNorm[1] +\
                                np.abs(errOffNorm[1])/fitOffNorm[1])
    
    title2 = str(tSlow) + \
        " ms slowing with Gated TOF normalised over off-shot fit amplitude,\n " +\
        str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
            fileLabels[f] + ", %.3g +- %.2g m/s peak shift"%(peakShift, peakShifterr)
    
    if showProcessed:
        plt.plot(v, NormOff, '.', color=colors[0], label="Slowing Off")
        plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitOffNorm), color=colors[0])
        plt.plot(v, NormOn, '.', color=colors[1], label="Slowing On")
        plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitOnNorm), color=colors[1])
        
        plt.xlabel("Velocity (m/s)")
        plt.ylabel("Normalised Gated TOF")
        plt.title(title2)
        plt.legend()
        plt.show()
    
    fit_results_ON = {"Variables":["amplitude", "mean", "std", "shift"],
                   "best fit":fitOnNorm, "error":errOnNorm}
    fit_results_OFF = {"Variables":["amplitude", "mean", "std", "shift"],
                   "best fit":fitOffNorm, "error":errOffNorm}
    
    FitResNormON[fileLabels[f]] = fit_results_ON
    FitResNormOFF[fileLabels[f]] = fit_results_OFF
    PeakShifts[fileLabels[f]] = [peakShift, peakShifterr]

#%% Summary -- slowing time vs peak shift
V04fRQ_X = []
V04fRQ_Y = []
V04fRQ_Yerr = []

MWand4f_X = []
MWand4f_Y = []
MWand4f_Yerr = []

OnlyMW_X = []
OnlyMW_Y = []
OnlyMW_Yerr = []

for s in V04fRQ:
    V04fRQ_X.append(SlowTimes[s])
    V04fRQ_Y.append(PeakShifts[s][0])
    V04fRQ_Yerr.append(PeakShifts[s][1])

for s in MWand4f:
    MWand4f_X.append(SlowTimes[s])
    MWand4f_Y.append(PeakShifts[s][0])
    MWand4f_Yerr.append(PeakShifts[s][1])

for s in OnlyMW:
    OnlyMW_X.append(SlowTimes[s])
    OnlyMW_Y.append(PeakShifts[s][0])
    OnlyMW_Yerr.append(PeakShifts[s][1])

V04fRQ_X = np.array(V04fRQ_X)
V04fRQ_Y = np.array(V04fRQ_Y)
V04fRQ_Yerr = np.array(V04fRQ_Yerr)

MWand4f_X = np.array(MWand4f_X)
MWand4f_Y = np.array(MWand4f_Y)
MWand4f_Yerr = np.array(MWand4f_Yerr)

OnlyMW_X = np.array(OnlyMW_X)
OnlyMW_Y = np.array(OnlyMW_Y)
OnlyMW_Yerr = np.array(OnlyMW_Yerr)

#%
sSpan = np.arange(0., 10., 0.1)

fit1, cov1 = curve_fit(tools.Line, V04fRQ_X, V04fRQ_Y, p0=[-1., 0.])
err1 = np.sqrt(np.diag(cov1))
plt.plot(V04fRQ_X, V04fRQ_Y, '.', color=colors[0], label="4f v0 R&Q, a=%.3g+-%.2g m/s^2"%(fit1[0], err1[0]))
plt.errorbar(V04fRQ_X, V04fRQ_Y, yerr=np.abs(V04fRQ_Yerr), color=colors[0],\
             fmt=" ", capsize=5)
plt.plot(sSpan, tools.Line(sSpan, *fit1), color=colors[0])

fit2, cov2 = curve_fit(tools.Line, MWand4f_X, MWand4f_Y, p0=[-1., 0.])
err2 = np.sqrt(np.diag(cov2))
plt.plot(MWand4f_X, MWand4f_Y, '.', color=colors[1], label="MW + 4f v0 R&Q, a=%.3g+-%.2g m/s^2"%(fit2[0], err2[0]))
plt.errorbar(MWand4f_X, MWand4f_Y, yerr=np.abs(MWand4f_Yerr), color=colors[1],\
             fmt=" ", capsize=5)
plt.plot(sSpan, tools.Line(sSpan, *fit2), color=colors[1])

fit3, cov3 = curve_fit(tools.Line, OnlyMW_X, OnlyMW_Y, p0=[-1., 0.])
err3 = np.sqrt(np.diag(cov3))
plt.plot(OnlyMW_X, OnlyMW_Y, '.', color=colors[2], label="MW only, a=%.3g+-%.2g m/s^2"%(fit3[0], err3[0]))
plt.errorbar(OnlyMW_X, OnlyMW_Y, yerr=OnlyMW_Yerr, color=colors[2],\
             fmt=" ", capsize=5)
plt.plot(sSpan, tools.Line(sSpan, *fit3), color=colors[2])

plt.xlabel("Slowing duration (ms)")
plt.ylabel("Velocity distribution peak shift (m/s)")
plt.title("July 30 slowing summary with 4f repumps and MW, \n Gate: "+\
          str(SigStart) + "ms to " + str(SigEnd) + "ms")
plt.legend()
plt.show()

#%% Pick one slowing duration and compare normalised gated TOFs:
# 10ms duration
sele10 = ["008", "009", "010"]
labels = ["MW + 4f v0 R&Q", "4f v0 R&Q", "MW only"]

#Normalise again by the off-shot's amplitudes to correct for source fluctuation
for i in range(0, len(sele10)):
    s10 = sele10[i]
    fitON = FitResNormON[s10]["best fit"]
    fitOFF = FitResNormOFF[s10]["best fit"]
    
    plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitON), color=colors[i], \
             label = labels[i])
    plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitOFF),\
             '-.', color='black')

plt.xlabel("Velocity (m/s)")
plt.ylabel("Normalised gated TOF")
plt.title("Normalised gated TOFs of slowing with different repumps and \n" + \
          "10ms slowing duration, accounting for source fluctuation.")
plt.legend()
plt.show()

#%% Compare fixed slowing start time and end time, and with 4fv0v1 R
V04fRQ2_X = []
V04fRQ2_Y = []
V04fRQ2_Yerr = []

V0V14fR_X = []
V0V14fR_Y = []
V0V14fR_Yerr = []

for s in V04fRQ2:
    V04fRQ2_X.append(SlowTimes[s])
    V04fRQ2_Y.append(PeakShifts[s][0])
    V04fRQ2_Yerr.append(PeakShifts[s][1])

for s in V0V14fR:
    V0V14fR_X.append(SlowTimes[s])
    V0V14fR_Y.append(PeakShifts[s][0])
    V0V14fR_Yerr.append(PeakShifts[s][1])
    
V04fRQ2_X = np.array(V04fRQ2_X)
V04fRQ2_Y = np.array(V04fRQ2_Y)
V04fRQ2_Yerr = np.array(V04fRQ2_Yerr)

V0V14fR_X = np.array(V0V14fR_X)
V0V14fR_Y = np.array(V0V14fR_Y)
V0V14fR_Yerr = np.array(V0V14fR_Yerr)

fit1, cov1 = curve_fit(tools.Line, V04fRQ_X, V04fRQ_Y, p0=[-1., 0.])
err1 = np.sqrt(np.diag(cov1))
plt.plot(V04fRQ_X, V04fRQ_Y, '.', color=colors[0], label="4f v0 R&Q, fixed start\na=%.3g+-%.2g m/s^2"%(fit1[0], err1[0]))
plt.errorbar(V04fRQ_X, V04fRQ_Y, yerr=np.abs(V04fRQ_Yerr), color=colors[0],\
             fmt=" ", capsize=5)
plt.plot(sSpan, tools.Line(sSpan, *fit1), color=colors[0])

fit2, cov2 = curve_fit(tools.Line, V04fRQ2_X, V04fRQ2_Y, p0=[-1., 0.])
err2 = np.sqrt(np.diag(cov2))
plt.plot(V04fRQ2_X, V04fRQ2_Y, '.', color=colors[1], label="4f v0 R&Q, fixed end\na=%.3g+-%.2g m/s^2"%(fit2[0], err2[0]))
plt.errorbar(V04fRQ2_X, V04fRQ2_Y, yerr=np.abs(V04fRQ2_Yerr), color=colors[1],\
             fmt=" ", capsize=5)
plt.plot(sSpan, tools.Line(sSpan, *fit2), color=colors[1])

fit3, cov3 = curve_fit(tools.Line, V0V14fR_X, V0V14fR_Y, p0=[-1., 0.])
err3 = np.sqrt(np.diag(cov3))
plt.plot(V0V14fR_X, V0V14fR_Y, '.', color=colors[2], label="4f v0 v1 R, fixed end\na=%.3g+-%.2g m/s^2"%(fit3[0], err3[0]))
plt.errorbar(V0V14fR_X, V0V14fR_Y, yerr=V0V14fR_Yerr, color=colors[2],\
             fmt=" ", capsize=5)
plt.plot(sSpan, tools.Line(sSpan, *fit3), color=colors[2])

plt.xlabel("Slowing duration (ms)")
plt.ylabel("Velocity distribution peak shift (m/s)")
plt.title("July 30 slowing summary with 4f repumps, fixed start or end slowing time, \n Gate: "+\
          str(SigStart) + "ms to " + str(SigEnd) + "ms")
plt.legend()
plt.show()

#%% Pick one slowing duration and compare normalised gated TOFs:
# 5ms duration
sele5 = ["002", "018", "019"]
labels = ["4f v0 R&Q, fixed start", "4f v0 R&Q, fixed end", "4f v0v1 R, fixed end"]

#Normalise again by the off-shot's amplitudes to correct for source fluctuation
for i in range(0, len(sele5)):
    s5 = sele5[i]
    fitON = FitResNormON[s5]["best fit"]
    fitOFF = FitResNormOFF[s5]["best fit"]
    
    plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitON), color=colors[i], \
             label = labels[i])
    plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitOFF),\
             '-.', color='black')

plt.xlabel("Velocity (m/s)")
plt.ylabel("Normalised gated TOF")
plt.title("Normalised gated TOFs of slowing with different repumps and \n" + \
          "5ms slowing duration, accounting for source fluctuation.")
plt.legend()
plt.show()

#%% Pick one slowing duration and compare normalised gated TOFs:
# 3ms duration
sele3 = ["014", "015", "020"]
labels = ["4f v0 R&Q, fixed start", "4f v0 R&Q, fixed end", "4f v0v1 R, fixed end"]

#Normalise again by the off-shot's amplitudes to correct for source fluctuation
for i in range(0, len(sele3)):
    s3 = sele3[i]
    fitON = FitResNormON[s3]["best fit"]
    fitOFF = FitResNormOFF[s3]["best fit"]
    
    plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitON), color=colors[i], \
             label = labels[i])
    plt.plot(vspan, tools.Gaussian_FWHM(vspan, *fitOFF),\
             '-.', color='black')

plt.xlabel("Velocity (m/s)")
plt.ylabel("Normalised gated TOF")
plt.title("Normalised gated TOFs of slowing with different repumps and \n" + \
          "3ms slowing duration, accounting for source fluctuation.")
plt.legend()
plt.show()