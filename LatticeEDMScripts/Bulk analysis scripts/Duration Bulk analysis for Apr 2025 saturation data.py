# -*- coding: utf-8 -*-
"""
Created on Sun Aug 24 14:34:55 2025

For bulk processing duration scans.

Apr 8th 2025: V2 saturation test, pump-in & pump-out

This uses Guanchen's 3-level model to calculate scattering rate.

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
def Three_level_SC(R, tau_in, tau_out):
    return R/(1 + tau_in/tau_out)

def Three_level_SC_err(R, tau_in, tau_out, dR, dtau_in, dtau_out):
    facR = 1/(1 + tau_in/tau_out)

b0 = 0.93
b1 = 0.066
b2 = 0.003
b3 = 0.0005

#%% From zip files directly, but use the average.xml (For saturation curve)
'''For power saturation curve

'''

datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month="Slowing data to publish\\Durations\\Free space slowing\\Saturation tests\\"#"August2025"
lasers = "V2 (new scheme)" #["V0", "V1", "V2(MWV0)", "V3(MWV0)"]
types=["Pump-in", "Pump-out"]#{"V0":["NoMW", "V1MWV0"], "V1":["NoMW", "MWV0"]}
#subfolders = ["V0", "V0V1", "V0V1V2", "V0V1V2V3"]

SigStart = 26
SigEnd = 28
BkgStart = 60
BkgEnd = 70

plotDuration = False

BR = b0 + b1

Powers = {"Pump-in":{"010":101, "012":20, "014":80, "016":5, \
                           "018":40, "020":90, "022":10, "024":60},\
          "Pump-out":{"009":101, "011":20, "013":80, "015":5, \
                                     "017":40, "019":90, "021":10, "023":60}}
PowersToPlot = {}
TausToPlot = {}
TauserrToPlot = {}

Pspan = np.arange(0., 110., step=1.)
xlabel = "V2 duration (μs)"

figV2 = plt.figure()


for T in types:
    drive = datadrive + "\\" + month + "\\" + lasers + "\\" + T + "\\"
    print(drive)   
    
    pattern="*scan*.zip"
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
    
    for i in range(0, len(files)):
        Scan = Data[fileLabels[i]]
        print('For file ' + re.split(r'[\\]', files[i])[-1])
        Settings = EDM.GetScanSettings(Scan)
        
        if T == "Pump-out":
            fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabels[i],\
                                                SigStart, SigEnd, BkgStart, BkgEnd,\
                                                plotFit=plotDuration)
        if T == "Pump-in":
            fig, fit_results, ScanParams, Ratio = EDM.Pump_inRunSingle(Scan, fileLabels[i],\
                                                SigStart, SigEnd, BkgStart, BkgEnd,\
                                                xlabel=xlabel, plotFit=plotDuration)
        
        tau = fit_results['best fit'][1]
        tauerr = fit_results['error'][1]
        base = fit_results['best fit'][2]
        baseerr = fit_results['error'][2]
        
        Tau[fileLabels[i]] = tau
        Tauerr[fileLabels[i]] = tauerr
        Base[fileLabels[i]] = base
        Baseerr[fileLabels[i]] = baseerr
        
        print('\n')
    
    if T == "Pump-out":
        Tau_out = Tau
        Tau_outerr = Tauerr
    if T == "Pump-in":
        Tau_in = Tau
        Tau_inerr = Tauerr
    
    #match power and scattering rate
    P = []
    tau = []
    tauerr = []
    
    for f in fileLabels:
        if np.abs(Tauerr[f]) < Tau[f]:
            P.append(Powers[T][f])
            tau.append(Tau[f])
            tauerr.append(Tauerr[f])
        
    P = np.array(P)
    tau = np.array(tau)
    tauerr = np.array(tauerr)
    
    PowersToPlot[T] = P
    TausToPlot[T] = tau
    TauserrToPlot[T] = tauerr
    
#%%
FitResults = {}
Psats = {}

colorID = 0

fig, ax1 = plt.subplots()
ax2 = ax1.twinx()  # instantiate a second Axes that shares the same x-axis
ax2.tick_params(axis='y', labelcolor=colors[1])

Axes = {types[0]:ax1, types[1]:ax2}

for T in types:
    tau = TausToPlot[T]
    tauerr = TauserrToPlot[T]
    P = PowersToPlot[T]
    
    fit, cov = curve_fit(tools.exp_decay, P, tau, p0=[1., 10., 500.],\
                         sigma=tauerr, absolute_sigma=True)
    err = np.sqrt(np.diag(cov))
    Psat = [fit[1], err[1]]
    
    FitResults[T] = [fit, err]
    Psats[T] = Psat
    
    Axes[T].plot(P, tau, '.', label = T, color=colors[colorID])
    Axes[T].errorbar(P, tau, yerr = tauerr, fmt = ' ', capsize=5, color=colors[colorID])
    Axes[T].plot(Pspan, tools.exp_decay(Pspan, *fit), color=colors[colorID])
    
    colorID += 1
        
plt.xlabel("V2 laser power (mW)")
ax1.set_ylabel("Pump-in time (μs)")
ax2.set_ylabel("Pump-out time (μs)", color=colors[1])
plt.title("V2 slowing saturation")
ax1.set_ylim(0, 2500)
ax2.set_ylim(0, 250)
plt.show()

plt.close()

#%%
print("Saturation powers (1/e): {value, error} \n", Psats)
print("\n Fit results: {amp, tau, bkg} & errs\n", FitResults)

#%%
SRs = []
SRerrs = []

BR = b0 + b1

P_in = PowersToPlot["Pump-in"]
P_out = PowersToPlot["Pump-out"]

tau_in = TausToPlot["Pump-in"]
tau_inerr = TauserrToPlot["Pump-in"]

tau_out = TausToPlot["Pump-out"]
tau_outerr = TauserrToPlot["Pump-out"]

R = -1 / (tau_out * np.log(BR))

RV2 = []
RV2err = []

for i in range(0, len(P_in)):
    for j in range(0, len(P_out)):
        if P_out[j] == P_in[i]:
            RV2.append(R[j]/(1 + tau_in[i]/tau_out[j]))
            RV2err.append(R[j]*(tau_inerr[i]/tau_in[i] + tau_outerr[j]/tau_out[j])/2)
        
#print(RV2)

plt.plot(P_in, RV2, '.')
plt.errorbar(P_in, RV2, RV2err, fmt=' ', capsize=5, color=colors[0])

fit, cov = curve_fit(tools.inverse_exp_decay, P_in, RV2, p0=[0.5, 20., 0.],\
                     bounds=([-np.inf, -np.inf, -0.01], [np.inf, np.inf, 0.01]),\
                         sigma=RV2err, absolute_sigma=True)

pspan = np.arange(0., 100., 1.)
plt.plot(pspan, tools.inverse_exp_decay(pspan, *fit))

plt.xlabel("V2 laser power (mW)")
plt.ylabel("Scattering rate (MHz)")
plt.title("V2 slowing saturation")
plt.show()
#%%
print(fit, np.sqrt(np.diag(cov)))
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
plt.ylabel("Scattering rate (MHz)")
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
plt.ylabel("Scattering rate (MHz)")
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
