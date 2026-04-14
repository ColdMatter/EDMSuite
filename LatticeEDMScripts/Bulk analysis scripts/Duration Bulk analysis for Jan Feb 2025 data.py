# -*- coding: utf-8 -*-
"""
Created on Sun Aug 24 14:34:55 2025

For bulk processing duration scans.

Jan: Microwave effects. Fiber coupled slowing
Need Mathematica to read individual scans and average them to export a txt first.

Feb: power vs scattering rate for all lasers. Free space slowing

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

#%%
def ScatRate(tau, tauerr, BR):
    Scat = -1 / (tau * np.log(BR))
    
    Scaterr = -tauerr / (tau * np.log(BR)) / (tau**2 * np.log(BR))
    
    return Scat, Scaterr

b0 = 0.93
b1 = 0.066
b2 = 0.003
b3 = 0.0005

SigStart = 22
SigEnd = 24
BkgStart = 60
BkgEnd = 70

#%% Load data (Stacked Duration for MW, Jan 2025)
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month="Slowing data to publish\\Durations\\Fiber coupled slowing\\"#"August2025"
date="20Jan2025\\NoMW"
subfolders = ["V0", "V0V1", "V0V1V2", "V0V1V2V3"]
#blockdrive=datadrive+"\\BlockData\\"

resfolder = "result\\resultTXT\\"

#% Use pre-processed data from Mathematica output

#Fit is weighted by data error. Results are consistent with Mathmatica results.

FinalResultsScanSep = {}
Tau = []
Tauerr = []
Base = []
Baseerr = []
SR = {}
Leak = {}

ScatV0V1 = 0.449
ScatV0V1err = 0

tspan = np.arange(0., 9000., step = 1.)

fig = plt.figure()

for V in range(0, len(subfolders)):
    drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolders[V] + "\\"\
        + resfolder
    print(drive)
    
    patternTXT="*Duration*.txt"
    files = glob.glob(f'{drive}{patternTXT}', recursive=True)
    print("Matching files: ", [os.path.basename(f) for f in files])
    
    if subfolders[V] == 'V0':
        BR = b0
    if subfolders[V] == 'V0V1':
        BR = b0 + b1
    if subfolders[V] == 'V0V1V2':
        BR = b0 + b1 + b2
    if subfolders[V] == 'V0V1V2V3':
        BR = b0 + b1 + b2 + b3
    
    DataTXT = tools.loadCSV(files[0], path=drive, skiprow = 0, deli = '\t', dtype=float)
    
    columns = DataTXT.columns.tolist() #[V0 duration, Ratio, Ratio err]
    
    DurationAvg = np.array(DataTXT[columns[0]])
    RatioAvg = np.array(DataTXT[columns[1]])
    
    if len(columns) > 2:
        RatioErr = np.array(DataTXT[columns[2]])
        
        fit, cov = curve_fit(tools.exp_decay, DurationAvg, RatioAvg, \
                             p0=[1., DurationAvg[-1]/3, 0.], \
                                 sigma=RatioErr, absolute_sigma=True)
    else:
        fit, cov = curve_fit(tools.exp_decay, DurationAvg, RatioAvg, \
                             p0=[1., DurationAvg[-1]/3, 0.])#,\
                       # bounds=([0.99, 1.01], [0., np.inf], [-1, 1]))
    fiterr = np.sqrt(np.diag(cov))
    
    FinalResultsScanSep[subfolders[V]] = [fit, fiterr]
    Tau.append(fit[1])
    Tauerr.append(fiterr[1])
    Base.append(fit[2])
    Baseerr.append(fiterr[2])
    
    Scat = -1 / (fit[1] * np.log(BR))
    Scaterr = -fiterr[1] / (fit[1] * np.log(BR)) / (fit[1]**2 * np.log(BR))
    SR[subfolders[V]] = [Scat, np.abs(Scaterr)]
    
    if subfolders[V] == 'V0':
        leak = 1 - np.exp(-1/(Scat*fit[1]))
        leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[1])/fit[1])
        Leak[subfolders[V]] = [leak, leakerr]
    if subfolders[V] == 'V0V1':
        leak = 1 - np.exp(-1/(Scat*fit[1]))
        ScatV0V1 = Scat
        ScatV0V1err = np.abs(Scaterr)
        leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[1])/fit[1])
        Leak[subfolders[V]] = [leak, leakerr]
    if subfolders[V] == 'V0V1V2':
        leak = 1 - np.exp(-1/(ScatV0V1*fit[1]))
        scatV0V1errPerc = ScatV0V1err/ScatV0V1
        leakerr = leak*(np.abs(fiterr[1])/fit[1] + scatV0V1errPerc)
        Leak[subfolders[V]] = [leak, leakerr]
    if subfolders[V] == 'V0V1V2V3':
        leak = 1 - np.exp(-1/(ScatV0V1*fit[1]))
        scatV0V1errPerc = ScatV0V1err/ScatV0V1
        leakerr = leak*(np.abs(fiterr[1])/fit[1] + scatV0V1errPerc)
        Leak[subfolders[V]] = [leak, leakerr]
    
    #ScatRate, ScatRateerr = ScatRate(fit[1], fiterr[1], BR)
    #SR[subfolders[V]] = [ScatRate, ScatRateerr]
    
    plt.plot(DurationAvg, RatioAvg, '.', label=subfolders[V], color = colors[V])
    plt.plot(tspan, tools.exp_decay(tspan, *fit), color = colors[V])
    
    if len(columns) > 2:
        plt.fill_between(DurationAvg, RatioAvg+RatioErr, RatioAvg-RatioErr, \
                     color=colors[V], alpha=0.3)
        
plt.title("Population remaining in X, N=1, v=0 over time."+\
          "\n Gated TOF ratio (On/Off), " + re.split(r'[\\]', date)[0] +\
              ", " + re.split(r'[\\]', date)[1])
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("ratio")
plt.legend(loc='upper right')
plt.ylim(-0.1, 1.1)
plt.show()

plt.close()

print("\n Fit results: \n", FinalResultsScanSep)
print("\n Scattering rate (MHz):\n", SR)
print("\n Leak:\n", Leak)

#%% Add set to above plot
date="20Jan2025\\MWV2"
subfolders = ["V0V1V2", "V0V1V2V3"]
#blockdrive=datadrive+"\\BlockData\\"

resfolder = "result\\resultTXT\\"

plt.figure(fig)

for V in range(0, len(subfolders)):
    drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolders[V] + "\\"\
        + resfolder
    print(drive)
    
    patternTXT="*Duration*.txt"
    files = glob.glob(f'{drive}{patternTXT}', recursive=True)
    print("Matching files: ", [os.path.basename(f) for f in files])
    
    if subfolders[V] == 'V0':
        BR = b0
    if subfolders[V] == 'V0V1':
        BR = b0 + b1
    if subfolders[V] == 'V0V1V2':
        BR = b0 + b1 + b2
    if subfolders[V] == 'V0V1V2V3':
        BR = b0 + b1 + b2 + b3
    
    DataTXT = tools.loadCSV(files[0], path=drive, skiprow = 0, deli = '\t', dtype=float)
    
    columns = DataTXT.columns.tolist() #[V0 duration, Ratio, Ratio err]
    
    DurationAvg = np.array(DataTXT[columns[0]])
    RatioAvg = np.array(DataTXT[columns[1]])
    
    if len(columns) > 2:
        RatioErr = np.array(DataTXT[columns[2]])
        
        fit, cov = curve_fit(tools.exp_decay2, DurationAvg, RatioAvg, \
                             p0=[DurationAvg[-1]/3, 0.], \
                                 sigma=RatioErr, absolute_sigma=True)
    else:
        fit, cov = curve_fit(tools.exp_decay2, DurationAvg, RatioAvg, \
                             p0=[DurationAvg[-1]/3, 0.])
    fiterr = np.sqrt(np.diag(cov))
    
    FinalResultsScanSep[subfolders[V]+", MWV2"] = [fit, fiterr]
    Tau.append(fit[0])
    Tauerr.append(fiterr[0])
    Base.append(fit[1])
    Baseerr.append(fiterr[1])
    
    Scat = -1 / (fit[0] * np.log(BR))
    Scaterr = -fiterr[0] / (fit[0] * np.log(BR)) / (fit[0]**2 * np.log(BR))
    SR[subfolders[V]+", MWV2"] = [Scat, np.abs(Scaterr)]
    
    #ScatRate, ScatRateerr = ScatRate(fit[1], fiterr[1], BR)
    #SR[subfolders[V]] = [ScatRate, ScatRateerr]
    
    if subfolders[V] == 'V0':
        leak = 1 - np.exp(-1/(Scat*fit[0]))
        leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[0])/fit[0])
        Leak[subfolders[V]] = [leak, leakerr]
    if subfolders[V] == 'V0V1':
        leak = 1 - np.exp(-1/(Scat*fit[0]))
        ScatV0V1 = Scat
        ScatV0V1err = Scaterr
        leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[0])/fit[0])
        Leak[subfolders[V]] = [leak, leakerr]
    if subfolders[V] == 'V0V1V2':
        leak = 1 - np.exp(-1/(ScatV0V1*fit[0]))
        scatV0V1errPerc = ScatV0V1err/ScatV0V1
        leakerr = leak*(np.abs(fiterr[0])/fit[0] + scatV0V1errPerc)
        Leak[subfolders[V]] = [leak, leakerr]
    if subfolders[V] == 'V0V1V2V3':
        leak = 1 - np.exp(-1/(ScatV0V1*fit[0]))
        scatV0V1errPerc = ScatV0V1err/ScatV0V1
        leakerr = leak*(np.abs(fiterr[0])/fit[0] + scatV0V1errPerc)
        Leak[subfolders[V]+", MWV2"] = [leak, leakerr]
    
    plt.plot(DurationAvg, RatioAvg, '.', label=subfolders[V]+", MWV2", color = colors[V+4])
    plt.plot(tspan, tools.exp_decay2(tspan, *fit), color = colors[V+4])
    
    if len(columns) > 2:
        plt.fill_between(DurationAvg, RatioAvg+RatioErr, RatioAvg-RatioErr, \
                     color=colors[V+4], alpha=0.3)
        
plt.title("Population remaining in X, N=1, v=0 over time."+\
          "\n Gated TOF ratio (On/Off), " + re.split(r'[\\]', date)[0])
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("ratio")
plt.legend(loc='upper right', bbox_to_anchor=(1.4, 1.))
plt.ylim(-0.1, 1.1)
plt.show()

plt.close()

print("\n Fit results: best fit, errors of {amplitude, tau, base} \n", FinalResultsScanSep)
print("\n Scattering rate (MHz): {value, error}\n", SR)
print("\n Leak: {value, error}\n", Leak)

#%% Add set to above plot
date="15Jan2025\\MWV0"
subfolders = ["V0V1V2V3"]
#blockdrive=datadrive+"\\BlockData\\"

resfolder = "result\\resultTXT\\"

plt.figure(fig)
V=0
drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolders[V] + "\\"\
    + resfolder
print(drive)

patternTXT="*Duration*.txt"
files = glob.glob(f'{drive}{patternTXT}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

if subfolders[V] == 'V0':
    BR = b0
if subfolders[V] == 'V0V1':
    BR = b0 + b1
if subfolders[V] == 'V0V1V2':
    BR = b0 + b1 + b2
if subfolders[V] == 'V0V1V2V3':
    BR = b0 + b1 + b2 + b3

DataTXT = tools.loadCSV(files[0], path=drive, skiprow = 0, deli = '\t', dtype=float)

columns = DataTXT.columns.tolist() #[V0 duration, Ratio, Ratio err]

DurationAvg = np.array(DataTXT[columns[0]])
RatioAvg = np.array(DataTXT[columns[1]])

if len(columns) > 2:
    RatioErr = np.array(DataTXT[columns[2]])
    
    fit, cov = curve_fit(tools.exp_decay2, DurationAvg, RatioAvg, \
                         p0=[DurationAvg[-1]/3, 0.], \
                             sigma=RatioErr, absolute_sigma=True)
else:
    fit, cov = curve_fit(tools.exp_decay2, DurationAvg, RatioAvg, \
                         p0=[DurationAvg[-1]/3, 0.])
fiterr = np.sqrt(np.diag(cov))

FinalResultsScanSep[subfolders[V]+", MWV0"] = [fit, fiterr]
Tau.append(fit[0])
Tauerr.append(fiterr[0])
Base.append(fit[1])
Baseerr.append(fiterr[1])

Scat = -1 / (fit[0] * np.log(BR))
Scaterr = -fiterr[0] / (fit[0] * np.log(BR)) / (fit[0]**2 * np.log(BR))
SR[subfolders[V]+", MWV0"] = [Scat, np.abs(Scaterr)]

#ScatRate, ScatRateerr = ScatRate(fit[1], fiterr[1], BR)
#SR[subfolders[V]] = [ScatRate, ScatRateerr]

if subfolders[V] == 'V0':
    leak = 1 - np.exp(-1/(Scat*fit[0]))
    leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[0])/fit[0])
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1':
    leak = 1 - np.exp(-1/(Scat*fit[0]))
    ScatV0V1 = Scat
    ScatV0V1err = Scaterr
    leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[0])/fit[0])
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1V2':
    leak = 1 - np.exp(-1/(ScatV0V1*fit[0]))
    scatV0V1errPerc = ScatV0V1err/ScatV0V1
    leakerr = leak*(np.abs(fiterr[0])/fit[0] + scatV0V1errPerc)
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1V2V3':
    leak = 1 - np.exp(-1/(ScatV0V1*fit[0]))
    scatV0V1errPerc = ScatV0V1err/ScatV0V1
    leakerr = leak*(np.abs(fiterr[0])/fit[0] + scatV0V1errPerc)
    Leak[subfolders[V]+", MWV0"] = [leak, leakerr]

plt.plot(DurationAvg, RatioAvg, '.', label=subfolders[V]+", MWV0", color = colors[4])
plt.plot(tspan, tools.exp_decay2(tspan, *fit), color = colors[4])

if len(columns) > 2:
    plt.fill_between(DurationAvg, RatioAvg+RatioErr, RatioAvg-RatioErr, \
                 color=colors[5], alpha=0.3)
        
plt.title("Population remaining in X, N=1, v=0 over time."+\
          "\n Gated TOF ratio (On/Off), "+ re.split(r'[\\]', date)[0])
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("ratio")
plt.legend(loc='upper right', bbox_to_anchor=(1.4, 1.))
plt.ylim(-0.1, 1.1)
plt.show()

plt.close()

print("\n Fit results: \n", FinalResultsScanSep)
print("\n Scattering rate (MHz):\n", SR)
print("\n Leak:\n", Leak)

#%% Add set to above plot
date="15Jan2025\\MWV1"
subfolders = ["V0V1V2V3"]
#blockdrive=datadrive+"\\BlockData\\"

resfolder = "result\\resultTXT\\"

plt.figure(fig)
V=0
drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolders[V] + "\\"\
    + resfolder
print(drive)

patternTXT="*Duration*.txt"
files = glob.glob(f'{drive}{patternTXT}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

if subfolders[V] == 'V0':
    BR = b0
if subfolders[V] == 'V0V1':
    BR = b0 + b1
if subfolders[V] == 'V0V1V2':
    BR = b0 + b1 + b2
if subfolders[V] == 'V0V1V2V3':
    BR = b0 + b1 + b2 + b3

DataTXT = tools.loadCSV(files[0], path=drive, skiprow = 0, deli = '\t', dtype=float)

columns = DataTXT.columns.tolist() #[V0 duration, Ratio, Ratio err]

DurationAvg = np.array(DataTXT[columns[0]])
RatioAvg = np.array(DataTXT[columns[1]])

if len(columns) > 2:
    RatioErr = np.array(DataTXT[columns[2]])
    
    fit, cov = curve_fit(tools.exp_decay2, DurationAvg, RatioAvg, \
                         p0=[DurationAvg[-1]/3, 0.], \
                             sigma=RatioErr, absolute_sigma=True)
else:
    fit, cov = curve_fit(tools.exp_decay2, DurationAvg, RatioAvg, \
                         p0=[DurationAvg[-1]/3, 0.])
fiterr = np.sqrt(np.diag(cov))

FinalResultsScanSep[subfolders[V]+", MWV1"] = [fit, fiterr]
Tau.append(fit[0])
Tauerr.append(fiterr[0])
Base.append(fit[1])
Baseerr.append(fiterr[1])

Scat = -1 / (fit[0] * np.log(BR))
Scaterr = -fiterr[0] / (fit[0] * np.log(BR)) / (fit[0]**2 * np.log(BR))
SR[subfolders[V]+", MWV1"] = [Scat, np.abs(Scaterr)]

#ScatRate, ScatRateerr = ScatRate(fit[1], fiterr[1], BR)
#SR[subfolders[V]] = [ScatRate, ScatRateerr]

if subfolders[V] == 'V0':
    leak = 1 - np.exp(-1/(Scat*fit[0]))
    leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[0])/fit[0])
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1':
    leak = 1 - np.exp(-1/(Scat*fit[0]))
    ScatV0V1 = Scat
    ScatV0V1err = Scaterr
    leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[0])/fit[0])
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1V2':
    leak = 1 - np.exp(-1/(ScatV0V1*fit[0]))
    scatV0V1errPerc = ScatV0V1err/ScatV0V1
    leakerr = leak*(np.abs(fiterr[0])/fit[0] + scatV0V1errPerc)
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1V2V3':
    leak = 1 - np.exp(-1/(ScatV0V1*fit[0]))
    scatV0V1errPerc = ScatV0V1err/ScatV0V1
    leakerr = leak*(np.abs(fiterr[0])/fit[0] + scatV0V1errPerc)
    Leak[subfolders[V]+", MWV1"] = [leak, leakerr]

plt.plot(DurationAvg, RatioAvg, '.', label=subfolders[V]+", MWV1", color = colors[5])
plt.plot(tspan, tools.exp_decay2(tspan, *fit), color = colors[5])

if len(columns) > 2:
    plt.fill_between(DurationAvg, RatioAvg+RatioErr, RatioAvg-RatioErr, \
                 color=colors[5], alpha=0.3)
        
plt.title("Population remaining in X, N=1, v=0 over time."+\
          "\n Gated TOF ratio (On/Off), "+ re.split(r'[\\]', date)[0])
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("ratio")
plt.legend(loc='upper right', bbox_to_anchor=(1.4, 1.))
plt.ylim(-0.1, 1.1)
plt.show()

plt.close()

print("\n Fit results: \n", FinalResultsScanSep)
print("\n Scattering rate (MHz):\n", SR)
print("\n Leak:\n", Leak)

#%% Add set to above plot
date="15Jan2025\\MWV0V1"
subfolders = ["V0V1V2V3"]
#blockdrive=datadrive+"\\BlockData\\"

resfolder = "result\\resultTXT\\"

plt.figure(fig)
V=0
drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolders[V] + "\\"\
    + resfolder
print(drive)

patternTXT="*Duration*.txt"
files = glob.glob(f'{drive}{patternTXT}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

if subfolders[V] == 'V0':
    BR = b0
if subfolders[V] == 'V0V1':
    BR = b0 + b1
if subfolders[V] == 'V0V1V2':
    BR = b0 + b1 + b2
if subfolders[V] == 'V0V1V2V3':
    BR = b0 + b1 + b2 + b3

DataTXT = tools.loadCSV(files[0], path=drive, skiprow = 0, deli = '\t', dtype=float)

columns = DataTXT.columns.tolist() #[V0 duration, Ratio, Ratio err]

DurationAvg = np.array(DataTXT[columns[0]])
RatioAvg = np.array(DataTXT[columns[1]])

if len(columns) > 2:
    RatioErr = np.array(DataTXT[columns[2]])
    
    fit, cov = curve_fit(tools.exp_decay2, DurationAvg, RatioAvg, \
                         p0=[DurationAvg[-1]/3, 0.], \
                             sigma=RatioErr, absolute_sigma=True)
else:
    fit, cov = curve_fit(tools.exp_decay2, DurationAvg, RatioAvg, \
                         p0=[DurationAvg[-1]/3, 0.])
fiterr = np.sqrt(np.diag(cov))

FinalResultsScanSep[subfolders[V]+", MWV0V1"] = [fit, fiterr]
Tau.append(fit[0])
Tauerr.append(fiterr[0])
Base.append(fit[1])
Baseerr.append(fiterr[1])

Scat = -1 / (fit[0] * np.log(BR))
Scaterr = -fiterr[0] / (fit[0] * np.log(BR)) / (fit[0]**2 * np.log(BR))
SR[subfolders[V]+", MWV0V1"] = [Scat, np.abs(Scaterr)]

#ScatRate, ScatRateerr = ScatRate(fit[1], fiterr[1], BR)
#SR[subfolders[V]] = [ScatRate, ScatRateerr]

if subfolders[V] == 'V0':
    leak = 1 - np.exp(-1/(Scat*fit[0]))
    leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[0])/fit[0])
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1':
    leak = 1 - np.exp(-1/(Scat*fit[0]))
    ScatV0V1 = Scat
    ScatV0V1err = Scaterr
    leakerr = leak * (np.abs(Scaterr)/Scat + np.abs(fiterr[0])/fit[0])
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1V2':
    leak = 1 - np.exp(-1/(ScatV0V1*fit[0]))
    scatV0V1errPerc = ScatV0V1err/ScatV0V1
    leakerr = leak*(np.abs(fiterr[0])/fit[0] + scatV0V1errPerc)
    Leak[subfolders[V]] = [leak, leakerr]
if subfolders[V] == 'V0V1V2V3':
    leak = 1 - np.exp(-1/(ScatV0V1*fit[0]))
    scatV0V1errPerc = ScatV0V1err/ScatV0V1
    leakerr = leak*(np.abs(fiterr[0])/fit[0] + scatV0V1errPerc)
    Leak[subfolders[V]+", MWV0V1"] = [leak, leakerr]

plt.plot(DurationAvg, RatioAvg, '.', label=subfolders[V]+", MWV0V1", color = colors[6])
plt.plot(tspan, tools.exp_decay2(tspan, *fit), color = colors[6])

if len(columns) > 2:
    plt.fill_between(DurationAvg, RatioAvg+RatioErr, RatioAvg-RatioErr, \
                 color=colors[6], alpha=0.3)
        
plt.title("Population remaining in X, N=1, v=0 over time."+\
          "\n Gated TOF ratio (On/Off), "+ re.split(r'[\\]', date)[0])
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("ratio")
plt.legend(loc='upper right', bbox_to_anchor=(1.4, 1.))
plt.ylim(-0.1, 1.1)
plt.show()

plt.close()

print("\n Fit results: \n", FinalResultsScanSep)
print("\n Scattering rate (MHz):\n", SR)
print("\n Leak:\n", Leak)

#%%
ScatRate, ScatRateerr = ScatRate(Tau[1], Tauerr[1], subfolders[1])
print(ScatRate)
print(ScatRateerr)
SR[subfolders[1]] = [ScatRate, ScatRateerr]

#%% From zip files directly, but use the average.xml (For saturation curve)
'''For power saturation curve

V0, V1 powers should be plotted against scattering rate, as branching ratio is
well defined.

V2, V3 powers should be plotted against base line, to show the population that
remains in the cycle after certain slowing time.

'''

datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month="Slowing data to publish\\Durations\\Free space slowing\\Saturation tests\\"#"August2025"
lasers = ["V0", "V1"] #["V0", "V1", "V2(MWV0)", "V3(MWV0)"]
types={"V0":["NoMW", "V1MWV0"], "V1":["NoMW", "MWV0"]}
#subfolders = ["V0", "V0V1", "V0V1V2", "V0V1V2V3"]
#blockdrive=datadrive+"\\BlockData\\"

resfolder = "result\\resultTXT\\"

SigStart = 22
SigEnd = 24
BkgStart = 60
BkgEnd = 70

Powers = {"V0, NoMW":{"013":440, "014":120, "015":220, "016":340, \
                           "017":82, "018":42, "019":440},\
          "V0, V1MWV0":{"035":325, "036":200, "037":56, "038":125, \
                        "039":250, "040":448, "041":320, "042":440}, \
          "V1, NoMW":{"021":372, "022":372, "023":135, "024":70, \
                      "025":215, "026":300, "027":23, "028":375}, \
          "V1, MWV0":{"029":375, "030":57, "031":22, "032":170, "033":278, \
                      "034":375}}
PowersToPlot = {}
SRs = {}
SRerrs = {}
FitResults = {}
Psats = {}

Pspan = np.arange(0., 500., step=1.)

figV0V1 = plt.figure()

for L in range(0, len(lasers)):
    for T in range(0, len(types)):
        drive = datadrive + "\\" + month + "\\" + lasers[L] + "\\" + types[lasers[L]][T] + "\\"
        print(drive)
    
        pattern="*duration*.zip"
        files = glob.glob(f'{drive}{pattern}', recursive=True)
        print("Matching files: ", [os.path.basename(f) for f in files])
        
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
        
        #% No averaging
        Tau = {}
        Tauerr = {}
        Base = {}
        Baseerr = {}
        SR=[]
        SRerr = []
        
        if lasers[L]+", "+types[lasers[L]][T] == 'V0, NoMW':
            BR = b0
        if lasers[L]+", "+types[lasers[L]][T]  == 'V0, V1MWV0':
            BR = b0 + b1
        if lasers[L]+", "+types[lasers[L]][T] == 'V1, NoMW':
            BR = b0 + b1
        if lasers[L]+", "+types[lasers[L]][T]  == 'V1, MWV0':
            BR = b0 + b1
        
        for i in range(0, len(files)):
            Scan = Data[fileLabels[i]]
            print('For file ' + re.split(r'[\\]', files[i])[-1])
            Settings = EDM.GetScanSettings(Scan)
            fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabels[i],\
                                                    SigStart, SigEnd, BkgStart, BkgEnd,\
                                                    plotFit=False)
            
            tau = fit_results['best fit'][1]
            tauerr = fit_results['error'][1]
            base = fit_results['best fit'][2]
            baseerr = fit_results['error'][2]
            
            Tau[fileLabels[i]] = tau
            Tauerr[fileLabels[i]] = tauerr
            Base[fileLabels[i]] = base
            Baseerr[fileLabels[i]] = baseerr
            
            Scat = -1 / (tau * np.log(BR))
            Scaterr = -tauerr / (tau * np.log(BR)) / (tau**2 * np.log(BR))
            SR.append(Scat)
            SRerr.append(np.abs(Scaterr))
            
            print('\n')
        
        #match power and scattering rate
        P = []
        
        for f in fileLabels:
            P.append(Powers[lasers[L]+", "+types[lasers[L]][T]][f])
            
        SR = np.array(SR)
        SRerr = np.array(SRerr)
        P = np.array(P)
        
        PowersToPlot[lasers[L]+", "+types[lasers[L]][T]] = P
        SRs[lasers[L]+", "+types[lasers[L]][T]] = SR
        SRerrs[lasers[L]+", "+types[lasers[L]][T]] = SRerr
        
        fit, cov = curve_fit(tools.inverse_exp_decay, P, SR, p0=[1., 100., 0.],\
                             sigma=SRerr, absolute_sigma=True, \
                                 bounds=([-np.inf, -np.inf, -0.01],[np.inf, np.inf, 0.01]))
        err = np.sqrt(np.diag(cov))
        Psat = [fit[1], err[1]]
        
        FitResults[lasers[L]+", "+types[lasers[L]][T]] = [fit, err]
        Psats[lasers[L]+", "+types[lasers[L]][T]] = Psat
        
        plt.plot(P, SR, '.', label = lasers[L]+", "+types[lasers[L]][T])
        plt.errorbar(P, SR, yerr = SRerr, fmt = ' ', capsize=5, color=colors[T+len(lasers)*L])
        plt.plot(Pspan, tools.inverse_exp_decay(Pspan, *fit), color=colors[T+len(lasers)*L])
            
plt.xlabel("Laser power (mW)")
plt.ylabel("Scattering rate (x10^6 photons/s)")
plt.title("V0, V1 slowing saturation")
plt.legend(loc="upper right", bbox_to_anchor=(1.3, 1.))
plt.show()

plt.close()

#%%
print("Saturation powers (1/e): {value, error} \n", Psats)
print("\n Fit results: {amp, tau, bkg} & errs\n", FitResults)

#%% Plot separate
columns = ["V0, NoMW", "V0, V1MWV0", "V1, NoMW", "V1, MWV0"]

colorID = 0
Pspan = np.arange(0., 550., step=1.)

for c in columns:
    if c[0:2] == "V0":
        plt.plot(PowersToPlot[c], SRs[c], '.', label="With"+c[3::], \
                 color=colors[colorID])
        plt.errorbar(PowersToPlot[c], SRs[c], SRerrs[c], fmt=' ', capsize=5, \
                     color=colors[colorID])
        plt.plot(Pspan, tools.inverse_exp_decay(Pspan, *FitResults[c][0]),\
                 color=colors[colorID])
        
        print("\n", c, "Saturation powers (1/e): {value, error} \n", Psats[c])
        print("\n", c, "Fit results: {amp, tau, bkg} & errs\n", FitResults[c])
            
        colorID += 1

plt.vlines([170., 210.], ymin=0., ymax=2., color="black", linestyles="-.")
plt.vlines([440., 550.], ymin=0., ymax=2., color="red", linestyles="-.")

plt.xlabel("V0 power (mW)")
plt.ylabel("Photons scattered per second (x10^6)")
plt.title("V0 slowing saturation")
plt.legend(loc="upper right", bbox_to_anchor=(1.32, 1.))
plt.show()

plt.close()

#%%
columns = ["V0, NoMW", "V0, V1MWV0", "V1, NoMW", "V1, MWV0"]

colorID = 0
Pspan = np.arange(0., 400., step=1.)

for c in columns:
    if c[0:2] == "V1":
        plt.plot(PowersToPlot[c], SRs[c], '.', label="With"+c[3::], \
                 color=colors[colorID])
        plt.errorbar(PowersToPlot[c], SRs[c], SRerrs[c], fmt=' ', capsize=5, \
                     color=colors[colorID])
        plt.plot(Pspan, tools.inverse_exp_decay(Pspan, *FitResults[c][0]),\
                 color=colors[colorID])
        
        print("\n", c, "Saturation powers (1/e): {value, error} \n", Psats[c])
        print("\n", c, "Fit results: {amp, tau, bkg} & errs\n", FitResults[c])
            
        colorID += 1

plt.vlines([90., 110.], ymin=0., ymax=2., color="black", linestyles="-.")
plt.vlines([350., 380.], ymin=0., ymax=2., color="red", linestyles="-.")

plt.xlabel("V1 power (mW)")
plt.ylabel("Photons scattered per second (x10^6)")
plt.title("V1 slowing saturation")
plt.legend(loc="upper right", bbox_to_anchor=(1.28, 1.))
plt.show()

plt.close()


#%%
'''For V2 and V3'''
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month="Slowing data to publish\\Durations\\Free space slowing\\Saturation tests\\"#"August2025"

PowersV2 = {"018":90, "019":70, "021":4, "022":10, \
                           "023":6, "024":50}
PowersV3 = {"013":14.7, "014":6.1, "015":11.8, "016":8.4, \
                        "017":7.1}
lasersV2 = "V2(MWV0)"
lasersV3 = "V3(MWV0)"

drive = datadrive + "\\" + month + "\\" + lasersV2 + "\\"
print(drive)

pattern="*duration*.zip"
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

#% No averaging
Tau = {}
Tauerr = {}
BaseV2 = []
BaseerrV2 = []
Frac9ms = []

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    print('For file ' + re.split(r'[\\]', files[i])[-1])
    Settings = EDM.GetScanSettings(Scan)
    fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabels[i],\
                                            SigStart, SigEnd, BkgStart, BkgEnd,\
                                            plotFit=True)
    
    tau = fit_results['best fit'][1]
    tauerr = fit_results['error'][1]
    base = fit_results['best fit'][2]
    baseerr = fit_results['error'][2]
    
    Tau[fileLabels[i]] = tau
    Tauerr[fileLabels[i]] = tauerr
    BaseV2.append(base)
    BaseerrV2.append(baseerr)
    
    Frac9ms.append(tools.exp_decay(9000., *fit_results["best fit"]))
    
    print('\n')

#%match power and scattering rate
P = []

for f in fileLabels:
    P.append(PowersV2[f])
    
P = np.array(P)

fit, cov = curve_fit(tools.inverse_exp_decay, P, Frac9ms, p0=[0.5, 10., 0.],\
                    bounds=([-np.inf, -np.inf, -0.01],[np.inf, np.inf, 0.01]))
err = np.sqrt(np.diag(cov))
Psat = [fit[1], err[1]]

Pspan = np.arange(0., 100., 0.1)

plt.plot(P, Frac9ms, '.', color = colors[0])#, label = lasers[L]+", "+types[lasers[L]][T])
#plt.errorbar(P, Frac9ms, yerr = BaseerrV2, fmt = ' ', capsize=5)#, color=colors[T+len(lasers)*L])
plt.plot(Pspan, tools.inverse_exp_decay(Pspan, *fit), color = colors[0])#, color=colors[T+len(lasers)*L])

plt.vlines([15., 30.], ymin=0., ymax=0.6, color="black", linestyles="-.")
plt.vlines([40., 60.], ymin=0., ymax=0.6, color="red", linestyles="-.")

plt.xlabel("V2 power (mW)")
plt.ylabel("Fraction of population remaining\n after 9ms slowing")
plt.title("V2 slowing saturation")
#plt.legend(loc="upper right", bbox_to_anchor=(1.28, 1.))
plt.show()

plt.close()

print("\n", lasersV2, "Saturation powers (1/e): {value, error} \n", Psat)
print("\n", lasersV2, "Fit results: {amp, tau, bkg} & errs\n", fit, err)

#%%
drive = datadrive + "\\" + month + "\\" + lasersV3 + "\\"
print(drive)

pattern="*duration*.zip"
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

#% No averaging
Tau = {}
Tauerr = {}
BaseV3 = []
BaseerrV3 = []
Frac9ms = []

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    print('For file ' + re.split(r'[\\]', files[i])[-1])
    Settings = EDM.GetScanSettings(Scan)
    fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabels[i],\
                                            SigStart, SigEnd, BkgStart, BkgEnd,\
                                            plotFit=True)
    
    tau = fit_results['best fit'][1]
    tauerr = fit_results['error'][1]
    base = fit_results['best fit'][2]
    baseerr = fit_results['error'][2]
    
    Tau[fileLabels[i]] = tau
    Tauerr[fileLabels[i]] = tauerr
    BaseV3.append(base)
    BaseerrV3.append(baseerr)
    
    Frac9ms.append(tools.exp_decay(9000., *fit_results["best fit"]))
    
    print('\n')

#%%match power and scattering rate
P = []

for f in fileLabels:
    P.append(PowersV3[f])
    
P = np.array(P)

fit, cov = curve_fit(tools.inverse_exp_decay, P, Frac9ms, p0=[0.5, 10., 0.],\
                    bounds=([-np.inf, -np.inf, -0.01],[np.inf, np.inf, 0.01]))
err = np.sqrt(np.diag(cov))
Psat = [fit[1], err[1]]

Pspan = np.arange(0., 20., 0.1)

plt.plot(P, Frac9ms, '.', color = colors[0])#, label = lasers[L]+", "+types[lasers[L]][T])
#plt.errorbar(P, Frac9ms, yerr = BaseerrV2, fmt = ' ', capsize=5)#, color=colors[T+len(lasers)*L])
plt.plot(Pspan, tools.inverse_exp_decay(Pspan, *fit), '-.', color = colors[0])#, color=colors[T+len(lasers)*L])

plt.vlines([4., 7.], ymin=0., ymax=0.6, color="black", linestyles="-.")
plt.vlines([10., 15.], ymin=0., ymax=0.6, color="red", linestyles="-.")

plt.xlabel("V3 power (mW)")
plt.ylabel("Fraction of population remaining\n after 9ms slowing")
plt.title("V3 slowing saturation")
#plt.legend(loc="upper right", bbox_to_anchor=(1.28, 1.))
plt.show()

plt.close()

print("\n", lasersV3, "Saturation powers (1/e): {value, error} \n", Psat)
print("\n", lasersV3, "Fit results: {amp, tau, bkg} & errs\n", fit, err)
