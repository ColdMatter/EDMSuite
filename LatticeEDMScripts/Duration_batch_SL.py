# -*- coding: utf-8 -*-
"""
Created on Wed Aug  6 14:15:48 2025

To analyse V0 duration measurements.

Use different cells to process for different purposes:
    - Stacked
    - Averaged over multiple files

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
month="November2025" #"Slowing data to publish\\Durations\\Free space slowing\\New V2 scheme"#
date="24"
subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolder
print(drive)

pattern="*V0DurationScan*.zip"
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
        Laser = re.split(r'[.]', re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[-2])[0]
        Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
        print("loaded file " + files[i])
        fileLabels.append(fileLabel)
        Lasers.append(Laser)

else:
    print("No matching files.")


#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 28
SigEnd = 30
BkgStart = 70
BkgEnd = 80

showTOF = False
shot_for_TOF = 20

showPlots = True

b0 = 0.93178
b1 = 0.06474
b2 = 0.00284
b3 = 0.00010

#%% Plot and fit each scan file individually
Tau = {}
Tauerr = {}
Base = {}
Baseerr = {}

Durations = []
Ratios = []
GoodData = [] #Append index here if fitted tau error is larger than tau

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    print('For file ' + re.split(r'[\\]', files[i])[-1])
    Settings = EDM.GetScanSettings(Scan)
    fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabel,\
                                            SigStart, SigEnd, BkgStart, BkgEnd,\
                                            plotFit=True)
    
    tau = fit_results['best fit'][1]
    tauerr = fit_results['error'][1]
    base = fit_results['best fit'][2]
    baseerr = fit_results['error'][2]
    
    if tauerr < tau:
        GoodData.append(fileLabels[i])
        
        Durations.append(ScanParams)
        Ratios.append(Ratio)
    else:
        print("file " + fileLabels[i] + " fit is bad. Excluded for averaging.")
    
    Tau[fileLabels[i]] = tau
    Tauerr[fileLabels[i]] = tauerr
    Base[fileLabels[i]] = base
    Baseerr[fileLabels[i]] = baseerr
    
    print('\n')
    
#%% Average of the same scan type
BR = b0

DurationAvg = np.average(Durations, axis=0)
RatioAvg = np.average(Ratios, axis=0)
RatioErr = np.std(Ratios, axis=0) / np.sqrt(len(Ratios))

title = "Gated TOF ratio (On/Off) over " +\
   str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
   str(SigStart) + "ms to " + str(SigEnd) + "ms gate " + fileLabel
   
title2 = "Population remaining in X, N=1, v=0 over time. Gated TOF ratio (On/Off)"

fig, fit_results = tools.Fitexp_decay(0, DurationAvg, RatioAvg,\
                    p0=[1., Settings["end"]/3, 0.], xstep=Settings["end"]*1e-3, \
            plot=True, display=True, Toprint=True,\
            title=title2, xlabel="V0 slowing duration (us)", ylabel="ratio",\
                plotErr=True, errY=RatioErr)

fit = fit_results['best fit']
fiterr = fit_results['error']
    
Scat = -1 / (fit[1] * np.log(BR))
Scaterr = -fiterr[1] / (fit[1] * np.log(BR)) / (fit[1]**2 * np.log(BR))
print("\n Scatterint rate (MHz): %.4g +- %.3g"%(Scat, Scaterr))

#%% Stacked plots
'''Plotting multiple decay curves on the same plot.

   Change types dictionary for legends.

'''

types = {'012':'V0', '013':'V0-V3', '014':'V0-V3 MWN0', '015':'V0-V3 MWN0 P2v0v1',\
         '016':'V0-V3 MWN0N2 P2v0v1'}

BR = b0 + b1

tspan = np.arange(0., 15000., 0.1)

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    print('For file ' + re.split(r'[\\]', files[i])[-1])
    Settings = EDM.GetScanSettings(Scan)
    fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabel,\
                                            SigStart, SigEnd, BkgStart, BkgEnd,\
                                            plotFit=False)
    
    plt.plot(ScanParams, Ratio, '.', label=types[fileLabels[i]], color=colors[i])
    plt.plot(tspan, tools.exp_decay(tspan, *fit_results['best fit']), color=colors[i])
    print("\n File " + fileLabels[i] + ", " + types[fileLabels[i]], ": ", fit_results)
    
    fit = fit_results['best fit']
    fiterr = fit_results['error']
    
    Scat = -1 / (fit[1] * np.log(BR))
    Scaterr = -fiterr[1] / (fit[1] * np.log(BR)) / (fit[1]**2 * np.log(BR))
    print("\n Scatterint rate (MHz): %.4g +- %.3g"%(Scat, Scaterr))
    
    print("\n")

plt.title("V0 Duration scan with P(2) repumps and N=2 MW")
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("Population remaining in optical cycle")
plt.legend(loc="upper right")
plt.show()    
    
#%% Stacked plots
'''Plotting multiple decay curves on the same plot.

   Change types dictionary for legends.

'''

types = {'019':'With N2 MW', '020':'Without N2 MW'}

BR = b0 + b1

tspan = np.arange(0., 15000., 0.1)

sele = ['019', '020']
#%%
for i in range(0, len(sele)):
    Scan = Data[sele[i]]
    print('For file ' + re.split(r'[\\]', sele[i])[-1])
    Settings = EDM.GetScanSettings(Scan)
    fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, sele[i],\
                                            SigStart, SigEnd, BkgStart, BkgEnd,\
                                            plotFit=False)
    
    plt.plot(ScanParams, Ratio, '.', label=types[sele[i]], color=colors[i])
    plt.plot(tspan, tools.exp_decay(tspan, *fit_results['best fit']), color=colors[i])
    print("\n File " + sele[i] + ", " + types[sele[i]], ": ", fit_results)
    
    fit = fit_results['best fit']
    fiterr = fit_results['error']
    
    Scat = -1 / (fit[1] * np.log(BR))
    Scaterr = -fiterr[1] / (fit[1] * np.log(BR)) / (fit[1]**2 * np.log(BR))
    print("\n Scatterint rate (MHz): %.4g +- %.3g"%(Scat, Scaterr))
    
    print("\n")

plt.title("V0 Duration scan with P(2) repumps and N=2 MW")
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("Population remaining in optical cycle")
plt.legend(loc="upper right")
plt.show()    
    
