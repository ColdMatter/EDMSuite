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
###When we were using OneDrive:
#datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
#month="September2025"
#date="29"
#subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

###When we are using Box:
#datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
datadrive = r"C:\Users\sl5119\Box\LatticeEDM\data"
month = "July 2026"
date = "30"
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\"# + subfolder
print(drive)

pattern="*duration*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%% Selection
sele = ["001", "005", "021"]

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
SigStart = 24
SigEnd = 27
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

for i in range(0, len(sele)):  #for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    print('For file ' + re.split(r'[\\]', fileLabels[i])[-1])   #files[i]
    Settings = EDM.GetScanSettings(Scan)
    fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabels[i],\
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

types = {'001':'001 4f v0 R&Q', '005':'005 MW only', '021':'021 4f v0v1 R'}
PlotFit = {'001':True, '005':True, '021':True}
MovAvg = {'001':False, '005':False, '021':False}

BR = b0 + b1 + b2 + b3

tspan = np.arange(0., 8000, 0.1)

for i in range(0, len(sele)):
    Scan = Data[fileLabels[i]]
    print('For file ' + re.split(r'[\\]', fileLabels[i])[-1]) #files[i]
    Settings = EDM.GetScanSettings(Scan)
    fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabel,\
                                            SigStart, SigEnd, BkgStart, BkgEnd,\
                                            plotFit=False)
    
    plt.plot(ScanParams, Ratio, '.', label=types[fileLabels[i]], color=colors[i])
    
    if PlotFit[fileLabels[i]]:
        fit = fit_results['best fit']
        fiterr = fit_results['error']
        
        plt.plot(tspan, tools.exp_decay(tspan, *fit_results['best fit']), color=colors[i],\
                 label="Decay time (μs): %.4g +- %.3g"%(fit[1], fiterr[1]))
        print("\n File " + fileLabels[i] + ", " + types[fileLabels[i]], ": ", fit_results)
        
        Scat = -1 / (fit[1] * np.log(BR))
        Scaterr = -fiterr[1] / (fit[1] * np.log(BR)) / (fit[1]**2 * np.log(BR))
        print("\n Scatterint rate (MHz): %.4g +- %.3g"%(Scat, Scaterr))
        print("\n Decay time (μs): %.4g +- %.3g"%(fit[1], fiterr[1]))
        
        print("\n")

plt.title("MW and 4f repump effect on pumping, July 30th 2026")
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("Population remaining in optical cycle")
plt.legend(bbox_to_anchor=(1.6, 1.1))
plt.show()    

#%% Combine multiple dataset into one
fig, fit_results, ScanParams1, Ratio1 = EDM.DurationRunSingle(Data["005"], fileLabel,\
                                        SigStart, SigEnd, BkgStart, BkgEnd,\
                                        plotFit=False)
    
fig, fit_results, ScanParams2, Ratio2 = EDM.DurationRunSingle(Data["006"], fileLabel,\
                                        SigStart, SigEnd, BkgStart, BkgEnd,\
                                        plotFit=False)
    
Combi_ScanParams = np.concatenate((ScanParams1, ScanParams2))
Combi_Ratio = np.concatenate((Ratio1, Ratio2))
#%%
plt.plot(Combi_ScanParams, Combi_Ratio, '.', label="before sorting")

# Get the indices that would sort Combi_ScanParams
sort_indices = np.argsort(Combi_ScanParams)

# Reorder both arrays using those indices
Combi_ScanParams_sorted = Combi_ScanParams[sort_indices]
Combi_Ratio_sorted      = Combi_Ratio[sort_indices]

plt.plot(Combi_ScanParams_sorted, Combi_Ratio_sorted, '.', label="after sorting")
plt.title("Soritng data based on X axis. All points should overlap")
plt.legend()
plt.show()
#%%
MA = 20
MoveAvg_SP = tools.MovingAverage(MA, Combi_ScanParams_sorted)
MoveAvg_R = tools.MovingAverage(MA, Combi_Ratio_sorted)

#Compare set
fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Data["004"], fileLabel,\
                                        SigStart, SigEnd, BkgStart, BkgEnd,\
                                        plotFit=False)

#plot
plt.plot(ScanParams, Ratio, '.', label="No MW")
plt.plot(MoveAvg_SP, MoveAvg_R, '.', color=colors[1], label="With MW, Move_avg=%g"%MA)

tspan = np.arange(0., 2000, 0.1)
fit, cov = curve_fit(tools.exp_decay, MoveAvg_SP, MoveAvg_R, p0=[1., 1000., 0.5])
err = np.sqrt(np.diag(cov))
print("Variables: ['amplitude', 'lifetime', 'shift']")
print("fit: ", fit)
print("err: ", err)
plt.plot(tspan, tools.exp_decay(tspan, *fit), color=colors[1], \
         label="Decay time (μs): %.4g +- %.3g"%(fit[1], err[1]))

plt.title("MW effect on pumping with Q(0) probe, July 24th 2026")
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("Population remaining in optical cycle")
plt.legend()

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
    
