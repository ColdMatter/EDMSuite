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

# On macOS, pythonnet needs pointing at Mono before anything imports clr
if sys.platform == "darwin":
    from pythonnet import load
    load("mono", libmono="/Library/Frameworks/Mono.framework/Versions/Current/lib/libmonosgen-2.0.dylib")

# Add LatticeEDMScripts (parent of "analysis scripts") to the path so the
# library imports work whatever the working directory or OS
try:
    scriptDir = os.path.dirname(os.path.abspath(__file__))
except NameError:  # __file__ isn't defined when running cells interactively
    scriptDir = os.getcwd()
sys.path.append(os.path.dirname(scriptDir))
import LatticeEDM_analysis_library as EDM

import numpy as np

import glob
import matplotlib.pyplot as plt
from scipy.optimize import curve_fit

import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']
#%% Choose file
Interactive = True   # True: pick the file in a dialog; False: use the path settings below

if Interactive:
    file = EDM.get_scan()[0] #getting a list of [file_path, file_name, file_date]
    print("Selected: ", file[1], " in ", file[2])
    filePath = file[0]
else:
    if sys.platform == "win32":
        datadrive = r"C:\Users\sl5119\Box\LatticeEDM\data"
    else:
        datadrive = os.path.expanduser("~/Library/CloudStorage/Box-Box/LatticeEDM/data")  # TODO: set Mac path
    month = "Oct 2026"
    date = "20"
    fileNumber = "009"
    drive = os.path.join(datadrive, month, date, "")
    matches = glob.glob(f'{drive}{fileNumber}_*.zip')
    print("Matching files: ", [os.path.basename(f) for f in matches])
    filePath = matches[0]

#%% Load data
Pass = None   # None: averaged scan (average.xml); 0, 1, ...: a single pass (scan_1.xml, scan_2.xml, ...)

files = [filePath]  #Just so it still works with the old code
fileLabels = []
Data = {}

fileLabel = re.split(r'[_]', os.path.basename(filePath))[0]
if Pass is None:
    Data[fileLabel] = EDM.ReadAverageScanInZippedXML(filePath)
    print("loaded file " + os.path.basename(filePath))
else:
    Scans = EDM.ReadAllScansInZippedXML(filePath)
    print("%g passes in file"%len(Scans))
    fileLabel = fileLabel + "_%g"%Pass
    Data[fileLabel] = Scans[Pass]
    print("loaded file " + os.path.basename(filePath) + ", scan %g"%Pass)
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

# X axis: WM frequency (default) or the TCL setpoint (ScanParameter, in V)
UseSetpoint = False  # True: ignore the WM and process everything on TCL setpoints

# Optional setpoint calibration, used only when there is no valid WM reading
# (UseSetpoint = True or wrong fibre). Leave as None to keep the results in volts.
CalTCLconv = None    # TCL conversion, MHz/V
CalRestSP = None     # zero-velocity setpoint, V
wavelength = 552     # nm
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

if UseSetpoint:
    HasWM = False
    print("Using TCL setpoints (V) as the x axis, WM ignored.")
elif int(f_iniTHz) == fTHz:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Scan, plot=True, Toprint=False)
    HasWM = True
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
else:
    HasWM = False
    print("Wrong fibre in WM (%.7g THz). Using TCL setpoints (V) as the x axis."%f_iniTHz)

#%% Plot
title="Gated TOF over " + str(Settings["param"]) + " with " +\
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
'''x is velocity (m/s) from the WM, or from the setpoint if CalTCLconv and
   CalRestSP are set. Otherwise x stays as the TCL setpoint (V).'''
if HasWM:
    f_relRes = (f_relMHz/1e6 + f_iniTHz - Resonance) * 1e6
    x = tools.VelocityfromFshift(f_relRes, Resonance, angle)
    InVolts = False
elif CalTCLconv is not None and CalRestSP is not None:
    x, _ = EDM.VelocityfromSetpoint(ScanParams, 0, CalRestSP, 0,\
                                    CalTCLconv, 0, wavelength, angle)
    InVolts = False
    print("Velocity from setpoint: %.4g MHz/V, zero velocity at %.4g V"%(CalTCLconv, CalRestSP))
else:
    x = ScanParams
    InVolts = True
    print("No setpoint calibration given - results stay in volts.")

if InVolts:
    xunit = "V"
    xname = "setpoint"
    xlabel = "TCL setpoint (V)"
    width0 = 0.03 # V, initial guess of FWHM
    xspan = np.linspace(np.min(x), np.max(x), 1000)
else:
    xunit = "m/s"
    xname = "velocity"
    xlabel = "Velocity (m/s)"
    width0 = 20. # m/s
    xspan = np.arange(np.min(x), np.max(x), 0.1)

if not InVolts:
    plt.plot(x, OffMeanCounts, label="Slowing Off")
    plt.plot(x, OnMeanCounts, label="Slowing On")

    plt.xlabel(xlabel)
    plt.ylabel("Gated TOF (ms.V)")
    plt.title(title)
    plt.legend()
    plt.show()

#%% Normalise & fit
peakOn = x[np.where(OnMeanCounts == np.max(OnMeanCounts))[0][0]]
peakOff = x[np.where(OffMeanCounts == np.max(OffMeanCounts))[0][0]]

fitOn, covOn = curve_fit(tools.Gaussian_FWHM, x, OnMeanCounts, \
                         p0=[200., peakOn, width0, 100.])
errOn = np.sqrt(np.diag(covOn))

fitOff, covOff = curve_fit(tools.Gaussian_FWHM, x, OffMeanCounts, \
                         p0=[200., peakOff, width0, 100.])
errOff = np.sqrt(np.diag(covOff))

# Normalise to amplitude of Off fit & correct shift to 0
NormOff = (OffMeanCounts - fitOff[3])/fitOff[0]
NormOn = (OnMeanCounts - fitOn[3])/fitOff[0]

fitOnNorm, covOnNorm = curve_fit(tools.Gaussian_FWHM, x, NormOn, \
                         p0=[200., peakOn, width0, 100.])
errOnNorm = np.sqrt(np.diag(covOnNorm))
print("Central ON %s = %.4g +- %.3g %s"%(xname, fitOnNorm[1], errOnNorm[1], xunit) +\
      "\n with FWHM = %.4g +- %.3g %s"%(fitOnNorm[2], errOnNorm[2], xunit))

fitOffNorm, covOffNorm = curve_fit(tools.Gaussian_FWHM, x, NormOff, \
                         p0=[200., peakOff, width0, 100.])
errOffNorm = np.sqrt(np.diag(covOffNorm))
print("Central OFF %s = %.4g +- %.3g %s"%(xname, fitOffNorm[1], errOffNorm[1], xunit) +\
      "\n with FWHM = %.4g +- %.3g %s"%(fitOffNorm[2], errOffNorm[2], xunit))

peakShift = fitOnNorm[1] - fitOffNorm[1]
if InVolts:
    # setpoints can sit near 0 V, so add the errors in quadrature rather than as relative errors
    peakShifterr = np.sqrt(errOnNorm[1]**2 + errOffNorm[1]**2)
else:
    peakShifterr = peakShift * (errOnNorm[1]/fitOnNorm[1] + errOffNorm[1]/fitOffNorm[1])
print("Peak shift (On-Off) = %.4g +- %.3g %s"%(peakShift, peakShifterr, xunit))

title2 = str(Settings["slowing time"]/1000) + \
    " ms slowing with Gated TOF normalised over off-shot fit amplitude,\n " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
        fileLabels[0] + ", %.3g +- %.2g %s peak shift"%(peakShift, peakShifterr, xunit)

plt.plot(x, NormOff, '.', color=colors[0], label="Slowing Off")
plt.plot(xspan, tools.Gaussian_FWHM(xspan, *fitOffNorm), color=colors[0])
plt.plot(x, NormOn, '.', color=colors[1], label="Slowing On")
plt.plot(xspan, tools.Gaussian_FWHM(xspan, *fitOnNorm), color=colors[1])

plt.xlabel(xlabel)
plt.ylabel("Normalised Gated TOF")
plt.title(title2)
plt.legend()
plt.show()
