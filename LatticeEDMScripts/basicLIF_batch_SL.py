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

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Load data
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month=datadrive+"\\August2025\\"
date=month+"\\17\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

pattern="*_ProbeSetpointScan*.zip"
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

showTOF = False
shot_for_TOF = 9
    
#%%
FittedGatedTOFs = {}
Fit_results = {}
FittedGatedTOFsWM = {}
Fit_resultsWM = {}
HasWMs = {}

for i in range(0, len(files)):
    print(re.split(r'[\\]', files[i])[-1])
    
    Scan = Data[fileLabels[i]]
    FittedGatedTOF, fit_results, FittedGatedTOFWM, fit_resultsWM, HasWM, summary = \
        EDM.ResonanceFreq(Scan, SigStart, SigEnd, BkgStart, BkgEnd,\
                      showTOF=showTOF, showPlot=True, fileLabel=fileLabels[i],\
                          location=locations[i],\
                          shot_for_TOF=0, freqTHz=542)
    
    FittedGatedTOFs[fileLabels[i]] = FittedGatedTOF
    Fit_results[fileLabels[i]] = fit_results
    FittedGatedTOFsWM[fileLabels[i]] = FittedGatedTOFWM
    Fit_resultsWM[fileLabels[i]] = fit_resultsWM
    HasWMs[fileLabels[i]] = HasWM
    
    print("\n  \n")

#%%
TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=True)

#%% angled probe, for velocity distribution calculation
'''The MOT one didn't scan far enough'''
pattern="*_angledProbeSetpointScan*.zip"
files_angled = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files_angled])


if len(files_angled) > 0:
    print("%g matching files found. Loading"%len(files_angled))
    Data_angled = {}
    fileLabels_angled = []
    locations_angled = []
    for i in range(0, len(files_angled)):
        location = re.split(r'[.]', re.split(r'[_]',\
                                    re.split(r'[\\]', files_angled[i])[-1])[-1])[0]
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files_angled[i])[-1])[0]
        Data_angled[fileLabel] = EDM.ReadAverageScanInZippedXML(files_angled[i])
        print("loaded file " + files[i])
        fileLabels_angled.append(fileLabel)
        locations_angled.append(location)

else:
    print("No matching files.")

#%% Rolling gate for angled probe, to get resonance freq per gate

'''Not the best way, do not use'''

for i in range(0, len(files_angled)):
    if locations_angled[i] == 'Downstream':
        SigStart = 10
        SigEnd = 40
        gateLength = 1.
        gateGap = 1.
        
    if locations_angled[i] == 'MOT':
        SigStart = 15
        SigEnd = 39
        gateLength = 3.
        gateGap = 1.
    #else:
    #    print("What is this file??")
    #    print("Analysis stopped")
    #    break
    BkgStart = 60
    BkgEnd = 70     
  
    gatesStart = np.arange(SigStart, SigEnd-gateLength+gateGap, step=gateGap)
    gatesEnd = np.arange(SigStart+gateLength, SigEnd+gateGap, step=gateGap)

    gateC = (gatesStart + gatesEnd)/2
    
    Scan = Data_angled[fileLabels_angled[i]]
    
    f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Scan)
    
    if int(f_iniTHz) == 542:
        TCL_WM_cali = EDM.TCL_WM_Calibration(Scan, plot=False, Toprint=False)
        HasWM = True
        TCLconv = TCL_WM_cali['best fit'][0]
        TCLconverr = TCL_WM_cali['error'][0]
        print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
    else:
        HasWM = False
        print("Wrong fibre in WM.")
    
    Resonances = []
    Resonanceerrs = []
    
    ResonanceSPs = []
    ResonanceSPerrs = []
    
    for A in range(0, len(gatesStart)):
        FittedGatedTOF, fit_results, FittedGatedTOFWM, fit_resultsWM, HasWM, summary = \
            EDM.ResonanceFreq(Scan, gatesStart[A], gatesEnd[A], BkgStart, BkgEnd,\
                          showTOF=False, showPlot=False, fileLabel=fileLabels_angled[i],\
                              location=locations_angled[i],\
                              shot_for_TOF=0, freqTHz=542)
        
        Resonances.append(summary[0])  #Relative freq, in MHz
        Resonanceerrs.append(summary[1])
        ResonanceSPs.append(summary[2])
        ResonanceSPerrs.append(summary[3])
    
    Resonances = np.array(Resonances)
    Resonanceerrs = np.array(Resonanceerrs)
    
    ResonanceSPs = np.array(ResonanceSPs)
    ResonanceSPerrs = np.array(ResonanceSPerrs)
    
    if HasWM:
        plt.plot(gateC, Resonances, label=locations_angled[i])
        plt.fill_between(gateC, Resonances+Resonanceerrs, Resonances-Resonanceerrs, alpha=0.3)
        plt.ylabel("Relative frequency (MHz) \n from %.8g THz"%f_iniTHz)
    else:
        plt.plot(gateC, ResonanceSPs, label=locations_angled[i])
        plt.fill_between(gateC, ResonanceSPs+ResonanceSPerrs, ResonanceSPs-ResonanceSPerrs, alpha=0.3)
        plt.ylabel("Setpoint (V)")
    
plt.title("Velocity distribution mapping")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()

#%% One by one
i = 0
SigStart = 10
SigEnd = 40
BkgStart = 60
BkgEnd = 70

        
gateLength = 1.
gateGap = 1.
gatesStart = np.arange(SigStart, SigEnd-gateLength+gateGap, step=gateGap)
gatesEnd = np.arange(SigStart+gateLength, SigEnd+gateGap, step=gateGap)

gateC = (gatesStart + gatesEnd)/2

Scan = Data_angled[fileLabels_angled[i]]

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Scan)

if int(f_iniTHz) == 542:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Scan, plot=False, Toprint=False)
    HasWM = True
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
else:
    HasWM = False
    print("Wrong fibre in WM.")

Resonances = []
Resonanceerrs = []

ResonanceSPs = []
ResonanceSPerrs = []

Spread = []
SpreadSP = []

for A in range(0, len(gatesStart)):
    FittedGatedTOF, fit_results, FittedGatedTOFWM, fit_resultsWM, HasWM, summary = \
        EDM.ResonanceFreq(Scan, gatesStart[A], gatesEnd[A], BkgStart, BkgEnd,\
                      showTOF=False, showPlot=False, fileLabel=fileLabels_angled[i],\
                          location=locations_angled[i],\
                          shot_for_TOF=0, freqTHz=542)
    
    Resonances.append(summary[0])  #Relative freq, in MHz
    Resonanceerrs.append(summary[1])
    ResonanceSPs.append(summary[2])
    ResonanceSPerrs.append(summary[3])
    
    Spread.append(fit_resultsWM['best fit'][1])
    SpreadSP.append(fit_results['best fit'][1])

Resonances = np.array(Resonances)
Resonanceerrs = np.array(Resonanceerrs)

ResonanceSPs = np.array(ResonanceSPs)
ResonanceSPerrs = np.array(ResonanceSPerrs)

Spread = np.array(Spread)
SpreadSP = np.array(SpreadSP)

if HasWM:
    plt.plot(gateC, Resonances, label=locations_angled[i])
    plt.fill_between(gateC, Resonances+Resonanceerrs, Resonances-Resonanceerrs, alpha=0.3)
    plt.fill_between(gateC, Resonances+Spread, Resonances-Spread, alpha=0.3, color="orange")
    plt.ylabel("Relative frequency (MHz) \n from %.8g THz"%f_iniTHz)
else:
    plt.plot(gateC, ResonanceSPs, label=locations_angled[i])
    plt.fill_between(gateC, ResonanceSPs+ResonanceSPerrs, ResonanceSPs-ResonanceSPerrs, alpha=0.3)
    plt.ylabel("Setpoint (V)")
    
plt.title("Velocity distribution mapping")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()

#%% To velocity
ResonancesDS = np.array(Resonances)
ResonanceerrsDS = np.array(Resonanceerrs)

ResonanceSPsDS = np.array(ResonanceSPs)
ResonanceSPerrsDS = np.array(ResonanceSPerrs)

#%%
def VelocityfromFshift(dF, F0, angle, dFerr):
    angle_rad = angle * np.pi / 180
    v = -dF*1e6 * 299792458 / (F0*1e12 * np.cos(angle_rad))
    dv = dFerr/dF * v
    return v, dv

#vDS, dvDS = VelocityfromFshift(ResonancesDS, 542.8091807917087-56.05068161*1e-6, 60, ResonanceerrsDS)

vDS, dvDS = EDM.VelocityfromFreq(ResonancesDS, ResonanceerrsDS,\
                        542.8091807917087-56.05068161*1e-6, 0.64214063, \
                         f_iniTHz, 552, 60)
vDS, dvDSspread = VelocityfromFshift(Resonances, \
                            542.8091471996183-26.48974844*1e-6, 60, Spread)

if HasWM:
    plt.plot(gateC, vDS, label="Downstream")#locations_angled[i])
    plt.fill_between(gateC, vDS+dvDS, vDS-dvDS, alpha=0.3)
    plt.fill_between(gateC, vDS+dvDSspread, vDS-dvDSspread, alpha=0.3, color='orange')
    plt.ylabel("Velocity (m/s)")
else:
    plt.plot(gateC, ResonanceSPs, label="Downstream")#locations_angled[i])
    plt.fill_between(gateC, ResonanceSPs+ResonanceSPerrs, ResonanceSPs-ResonanceSPerrs, alpha=0.3)
    plt.ylabel("Velocity (m/s)")
    
plt.title("Velocity VS time")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()

#%% fit trend
dDS = 1.5
def timeTOvelocity(v, A, tau_v, d=dDS):
    return d/v + A*np.exp(-v/tau_v)

from scipy.optimize import curve_fit
fitT, covT = curve_fit(timeTOvelocity, vDS, gateC, p0=[1., 10.])
print(str(fitT))

plt.plot(gateC, vDS, label="Downstream")#locations_angled[i])
plt.fill_between(gateC, vDS+dvDS, vDS-dvDS, alpha=0.3)
plt.fill_between(gateC, vDS+dvDSspread, vDS-dvDSspread, alpha=0.3, color='orange')

v = np.arange(65, 170, step=0.1)

plt.plot(timeTOvelocity(v, *fitT), v, '-.', label='fit', color='black')

plt.ylabel("Velocity (m/s)")
plt.title("Velocity - time mapping")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()

#%% sanity check with setpoint
vDS, dvDS = EDM.VelocityfromSetpoint(ResonanceSPsDS, ResonanceSPerrsDS, \
                                    2.32450924, 0.00317625, \
                         -203.4, 0.7, 552, 60)
vDS, dvDSspread = EDM.VelocityfromSetpoint(ResonanceSPsDS, SpreadSP, \
                                    2.32450924, 0.00317625, \
                         -203.4, 0.7, 552, 60)

plt.plot(gateC, vDS, label="Downstream")#locations_angled[i])
plt.fill_between(gateC, vDS+dvDS, vDS-dvDS, alpha=0.3)
plt.fill_between(gateC, vDS+dvDSspread, vDS-dvDSspread, alpha=0.3, color='orange')

plt.ylabel("Velocity (m/s)")
    
plt.title("Velocity VS time, from setpoint")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()

#%%
vMOT, dvMOT = VelocityfromFshift(Resonances, 542.8091471996183-26.48974844*1e-6, 45, Resonanceerrs)
vMOT, dvMOTspread = VelocityfromFshift(Resonances, 542.8091471996183-26.48974844*1e-6, 45, Spread)
#vDS, dvDS = EDM.VelocityfromFreq(ResonancesDS, ResonanceerrsDS,\
#                        542.8091807917087-56.05068161*1e-6, 0.64214063, \
#                         f_iniTHz, 552, 60)

if HasWM:
    plt.plot(gateC, vMOT, label="MOT")#locations_angled[i])
    plt.fill_between(gateC, vMOT+dvMOT, vMOT-dvMOT, alpha=0.3)
    plt.fill_between(gateC, vMOT+dvMOTspread, vMOT-dvMOTspread, alpha=0.3, color='orange')
    plt.ylabel("Velocity (m/s)")
else:
    plt.plot(gateC, ResonanceSPs, label="MOT")#locations_angled[i])
    plt.fill_between(gateC, ResonanceSPs+ResonanceSPerrs, ResonanceSPs-ResonanceSPerrs, alpha=0.3)
    plt.ylabel("Velocity (m/s)")
    
plt.title("Velocity VS time")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()

#%%
vMOT, dvMOT = EDM.VelocityfromSetpoint(ResonanceSPsDS, ResonanceSPerrsDS, \
                                    -1.77723969, 0.00328696, \
                         -127.8, 0.6, 552, 45)

plt.plot(gateC, ResonanceSPs, label="MOT")#locations_angled[i])
plt.fill_between(gateC, ResonanceSPs+ResonanceSPerrs, ResonanceSPs-ResonanceSPerrs, alpha=0.3)
plt.ylabel("Velocity (m/s)")
    
plt.title("Velocity VS time")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()