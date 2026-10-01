# -*- coding: utf-8 -*-
"""
Created on Tue Aug 26 15:05:26 2025

For single slowing data (angled probe setpoint scan ON-OFF) analysis

No rolling gate

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

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Set data path
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month="November2025"
date="24"
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\"
print(drive)
#%% Load and analyse perpendicular probe data, for calibration
"""Get zero-velocity frequency for V0. Must have
"""
pattern="probe*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files], "\n")


if len(files) > 0:
    DataRef = EDM.ReadAverageScanInZippedXML(files[0])
    SettingsRef = EDM.GetScanSettings(DataRef)
    ScanParamsRef = EDM.GetScanParameterArray(DataRef)
    fileLabelRef = re.split(r'[\\]', files[0])[-1][0:3]
    
    SigStart = 25 #in ms
    SigEnd = 27
    BkgStart = 60
    BkgEnd = 70
    
    f_iniTHzRef, f_relMHzRef = EDM.GetScanFreqArrayMHz(DataRef)
    if int(f_iniTHzRef) == 542:
        TCL_WM_cali = EDM.TCL_WM_Calibration(DataRef, plot=True)
        HasWM = True
        TCLconvRef = TCL_WM_cali['best fit'][0]
        TCLconverrRef = TCL_WM_cali['error'][0]
        if np.abs(TCLconvRef) > 1000:
            skipWM = True
        print("\n TCL calibration = %.4g +- %.2g MHz \n"%(TCLconvRef, TCLconverrRef))
    else:
        HasWM = False
        print("\n Wrong fibre in WM. \n")
    
    FittedGatedTOFRef, fit_resultsRef, FittedGatedTOFWMRef, fit_resultsWMRef, HasWMRef, summaryRef= \
        EDM.ResonanceFreq(DataRef, 28, 38, BkgStart, BkgEnd, fileLabel = fileLabelRef)
    
    RestSP = fit_resultsRef['best fit'][0]
    RestSPerr = fit_resultsRef['error'][0]
    RestWM = fit_resultsWMRef['best fit'][0]*1e-6+f_iniTHzRef
    RestWMerr = fit_resultsWMRef['error'][0]
    
    if HasWM:
        print("Rest frame frequency: %.10g (THz) +- %.3g (MHz)"%(RestWM, RestWMerr))
        print("Rest frame setpoint: %.4g +- %.3g (V)"%(RestSP, RestSPerr))
    else:
        print("Rest frame setpoint: %.4g +- %.3g (V)"%(RestSP, RestSPerr))
    
    
else:
    print("\n No rest frame data available on this day. Try loading saved setpoint:")
    patternTXT="*probe*.txt"
    filesTXT = glob.glob(f'{drive}{patternTXT}', recursive=True)
    print("Matching files: ", [os.path.basename(f) for f in filesTXT], "\n")

    if len(filesTXT) != 0:    
        print("Found saved probe setpoint. Reading...")
        with open(filesTXT[0]) as f:
                RestSP = float(f.read())
                RestSPerr = 0.
                print("Probe setpint (V): ", RestSP)
    else:
        print("Nothing was saved. Stop here")

#%% Load slowing data
pattern="*008*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])
#%%
Data = EDM.ReadAverageScanInZippedXML(files[0])
print("\n loaded file " + files[0] + "\n")

#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

slowing_time = Settings["slowing time"]/1000
print("\n Slowing duration is " + str(slowing_time) + " ms")

fileLabel = re.split(r'[\\]', files[0])[-1][0:3]

#%%
#RestSP = 0.393
#RestSPerr = 0
#TCLconvRef = 0.393  #only run if missing
#TCLconverrRef = 0

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 38 #in ms
SigEnd = 50
BkgStart = 70
BkgEnd = 90

wavelength = 552 #in nm

showTOF = True
shot_for_TOF = 35

BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Data)
if int(f_iniTHz) == 542:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=True)
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    HasWM = True
    print(TCLconv)
else:
    HasWM = False
    print("\n Wrong fibre in WM. \n")

distance = 1.5 # m
MOTdistance = 2.0 
angle = 45

gateLength = 1
gatesStart = np.arange(SigStart, SigEnd, step=gateLength)
gatesEnd = np.arange(SigStart+gateLength, SigEnd+gateLength, step=gateLength)
gateC = (gatesStart + gatesEnd)/2

v_exp = EDM.ExpectedVelocity(distance, gateC) #in m/s, gate in ms
v_expMOT = EDM.ExpectedVelocity(MOTdistance, gateC)

#% Get gated TOF
TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

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

#% Convert to velocity distribution, and fit Gaussians
angle = 45
#RestSP = 2.47 #Change here if actual rest frame setpoint is different, or if
#               it has changed during the day
#RestSPerr = TCLconverr
#TCLconv2 = -156.884

#Just use rest frame setpoint fit error for general setpoint errors.
SPerr = np.ones(shape=len(ScanParams)) * RestSPerr
        
v, err = EDM.VelocityfromSetpoint(ScanParams, SPerr, RestSP, RestSPerr, \
                         TCLconv, TCLconverr, wavelength, angle)

print("For molecules in gate " +str(SigStart) + "ms to " + str(SigEnd) + "ms \n")
    
from scipy.optimize import curve_fit
peakOn = v[np.where(OnBkgSub == np.max(OnBkgSub))[0][0]]
fitOn, covOn = curve_fit(tools.SkewedGaussian, v, OnBkgSub, p0=[peakOn, 10., 1., 0.5, 1.])
errOn = np.sqrt(np.diag(covOn))
print("Central ON velocity = %.4g +- %.3g m/s"%(fitOn[0], errOn[0]) +\
      "\n with FWHM = %.4g +- %.3g m/s"%(fitOn[1]*2*np.sqrt(2*np.log(2)),\
                                         errOn[1]*2*np.sqrt(2*np.log(2))))

peakOff = v[np.where(OnBkgSub == np.max(OnBkgSub))[0][0]]
fitOff, covOff = curve_fit(tools.SkewedGaussian, v, OffBkgSub, p0=[peakOff, 10., 0.5, 0.5, 1.])
errOff = np.sqrt(np.diag(covOff))
print("Central OFF velocity = %.4g +- %.3g m/s"%(fitOff[0], errOff[0]) +\
      "\n with FWHM = %.4g +- %.3g m/s"%(fitOff[1]*2*np.sqrt(2*np.log(2)),\
                                         errOff[1]*2*np.sqrt(2*np.log(2))))

vspan = np.arange(np.min(v), np.max(v), step = 1)

plt.plot(v, OnBkgSub, '.', label='On', color=colors[0])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOn), color=colors[0])
plt.plot(v, OffBkgSub, '.', label='Off', color=colors[1])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOff), color=colors[1])
plt.xlabel('Velocity (m/s)')
plt.ylabel("Gated LIF (ms.V)")
plt.title(title)
plt.legend()
plt.show()

#%% Save to plot together
ON_005_3050 = OnBkgSub
ON_005_fit = fitOn
OFF_005_3050 = OffBkgSub
OFF_005_fit = fitOff

#%%
ON_006_3050 = OnBkgSub
ON_006_fit = fitOn
OFF_006_3050 = OffBkgSub
OFF_006_fit = fitOff

#%%
ON_007_3050 = OnBkgSub
ON_007_fit = fitOn
OFF_007_3050 = OffBkgSub
OFF_007_fit = fitOff

#%%
plt.plot(v, ON_005_3050, '.', label='005, chirp 1')
plt.plot(vspan, tools.SkewedGaussian(vspan, *ON_005_fit), color=colors[0])
plt.plot(v, ON_006_3050, '.', label='006, chirp 1')
plt.plot(vspan, tools.SkewedGaussian(vspan, *ON_006_fit), color=colors[1])
plt.plot(v, ON_007_3050, '.', label='007, chirp 2')
plt.plot(vspan, tools.SkewedGaussian(vspan, *ON_007_fit), color=colors[2])

title2 = "Velocity distribution of 18ms slowing with different chirp profiles"
plt.xlabel('Velocity (m/s)')
plt.ylabel("Gated LIF (ms.V)")
plt.title(title2)
plt.legend()
plt.show()

#%%
ON_005006_3050 = (ON_005_3050 + ON_006_3050)/2
peakOn2 = v[np.where(ON_005006_3050 == np.max(ON_005006_3050))[0][0]]
ON_005006_fit, cov2 = curve_fit(tools.SkewedGaussian, v, ON_005006_3050,\
                                p0=[peakOn2, 10., 1., 0.5, 1.])

Norm1 = np.max(tools.SkewedGaussian(vspan, *ON_005006_fit))
Norm2 = np.max(tools.SkewedGaussian(vspan, *ON_007_fit))

plt.plot(v, ON_005006_3050/Norm1, '.', label='005 & 006 avg, chirp 1')
plt.plot(vspan, tools.SkewedGaussian(vspan, *ON_005006_fit)/Norm1, color=colors[0])
plt.plot(v, ON_007_3050/Norm2, '.', label='007, chirp 2')
plt.plot(vspan, tools.SkewedGaussian(vspan, *ON_007_fit)/Norm2, color=colors[1])

title2 = "Normalised velocity distribution of 18ms slowing \n \
    with different chirp profiles"
plt.xlabel('Velocity (m/s)')
plt.ylabel("Gated LIF (ms.V)")
plt.title(title2)
plt.legend()
plt.show()

#%%
OFF_005006_3050 = (OFF_005_3050 + OFF_006_3050)/2
peakOff2 = v[np.where(OFF_005006_3050 == np.max(OFF_005006_3050))[0][0]]
OFF_005006_fit, cov3 = curve_fit(tools.SkewedGaussian, v, OFF_005006_3050,\
                                p0=[peakOff2, 10., 1., 0.5, 1.])

    
Diff_005006 = ON_005006_3050 - OFF_005006_3050
Diff_007 = ON_007_3050 - OFF_007_3050 

plt.plot(v, Diff_005006, '.-', label='005 & 006 avg, chirp 1')
plt.plot(v, Diff_007, '.-', label='007, chirp 2')

title3 = "Difference in velocity distribution of 18ms slowing \n \
    between On Off shots with different chirp profiles"
plt.xlabel('Velocity (m/s)')
plt.ylabel("Gated LIF (ms.V)")
plt.title(title3)
plt.legend()
plt.show()
