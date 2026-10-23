# -*- coding: utf-8 -*-
"""
Created on Mon Aug 11 10:19:10 2025

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

import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Set data path
###When we were using OneDrive:
#datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
#month="August2025"
#date="12"

###When we are using Box:
datadrive = EDM.get_datadrive()  # set per device in data_locations.json
month = "Oct 2026"
date = "20"
#blockdrive=datadrive+"\\BlockData\\"

drive = os.path.join(datadrive, month, date, "")
print(drive)

#%% Basic analysis params
SigStart = 20 #in ms
SigEnd = 22.5
BkgStart = 70
BkgEnd = 80

distance = 1.2 # m
MOTdistance = 1.2

gateLength = 1.
gatesStart = np.arange(SigStart, SigEnd, step=gateLength)
gatesEnd = np.arange(SigStart+gateLength, SigEnd+gateLength, step=gateLength)
gateC = (gatesStart + gatesEnd)/2

v_exp = EDM.ExpectedVelocity(distance, gateC) #in m/s, gate in ms

wavelength = 552

#%% Load and analyse perpendicular probe data
"""Get zero-velocity frequency for V0. Must have
"""
RefPattern = "*probeVertical*.zip"
Reffiles = glob.glob(f'{drive}{RefPattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in Reffiles])

#%%
# Defaults, overwritten below if there is a reference scan
RestSP = None
RestSPerr = None
RefTCLconv = None
RefTCLconverr = None

if len(Reffiles) > 0:
    Ref = True
    DataRef = EDM.ReadAverageScanInZippedXML(Reffiles[0])
    SettingsRef = EDM.GetScanSettings(DataRef)
    ScanParamsRef = EDM.GetScanParameterArray(DataRef)

    f_iniTHzRef, f_relMHzRef = EDM.GetScanFreqArrayMHz(DataRef)
    if int(f_iniTHzRef) == 542:
        TCL_WM_cali = EDM.TCL_WM_Calibration(DataRef, plot=True)
        HasWM = True
        RefTCLconv = TCL_WM_cali['best fit'][0]
        RefTCLconverr = TCL_WM_cali['error'][0]
        print("TCL calibration = %.4g +- %.2g MHz"%(RefTCLconv, RefTCLconverr))
    else:
        HasWM = False
        print("Wrong fibre in WM.")

    print("Get rest frame velocity of the gate range:")
    RestSP, RestSPerr, RestGatedTOFSP, RestWM, RestWMerr, RestGatedTOFWM =\
        EDM.FitGatedTOFOn(DataRef, SigStart, SigEnd, BkgStart, BkgEnd, distance)

    print("Zero velocity setpoint = %.3g +- %.2g V"%\
          (RestSP, RestSPerr))

    if RestWM != 0:
        print("zero velocity frequency = %.10g THz +- %.2g MHz"%\
               (RestWM, RestWMerr))

else:
    Ref = False
    RestWM = 542.8091201 #THz, from June 25 2025
    RestWMerr = 0.43 #MHz
    print("No rest frame data available on this day.")
    print("Use default rest frame frequency, 25-30ms gate: %.10g THz +- %.2g MHz"%\
           (RestWM, RestWMerr))
    print("Velocities can't be calculated without the reference scan - only gated TOF fits will be done.")

#%% Load data
pattern="*V0setpointScan*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%% Selection
sele = ["009", "010"] # leave empty, sele = [], to load all matching files

#%%
LoadPasses = False

if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    if len(sele) == 0:
        # No selection: select every matching file by its ID
        sele = sorted(set(re.split(r'[_]', os.path.basename(f))[0] for f in files))
        print("No selection - loading all matching files: ", sele)
    Data = {}
    fileLabels = []
    Lasers = []
    for i in range(0, len(files)):
        fileLabel = re.split(r'[_]', os.path.basename(files[i]))[0]
        Laser = re.split(r'[.]', re.split(r'[_]', os.path.basename(files[i]))[-2])[0]

       #Use this part if have selections
        for j in range(0, len(sele)):
            if fileLabel == sele[j]:
                print("File "+fileLabel+" selected")
       ###
                if LoadPasses:
                    Scans = EDM.ReadAllScansInZippedXML(files[i])
                    for k in range(0, len(Scans)):
                        Data[fileLabel+"_%g"%k] = Scans[k]
                        print("loaded file " + files[i] + ", scan %g"%k)
                        fileLabels.append(fileLabel+"_%g"%k)
                        Lasers.append(Laser)
                else:
                    Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
                    print("loaded file " + files[i])
                    fileLabels.append(fileLabel)
                    Lasers.append(Laser)

else:
    print("No matching files.")

#%% Check settings saved by ScanMaster (TOF gates, slowing duration, plugins)
for f in fileLabels:
    Settings = EDM.GetScanSettings(Data[f])
    print(f + ": slowing %s us from %s us, ScanMaster gate %s-%s ms (bkg %s-%s ms), %s"%\
          (Settings["slowing time"], Settings["slowing start"],\
           Settings["gate SigStart"], Settings["gate SigEnd"],\
           Settings["gate BkgStart"], Settings["gate BkgEnd"],\
           Settings["pattern plugin"]))

#%% Get peak velocities
'''This is the old way with rolling gates'''
PeakVOn = {}
PeakVOnerr = {}
PeakVOff = {}
PeakVOfferr = {}

velocityFigs = {}
AllFittedGatedTOFs = {}
slowing_times = {}

for i in range(0, len(fileLabels)):
    Scan = Data[fileLabels[i]]
    velocityFig, meanVOn, meanVOnerr, meanVOff, meanVOfferr, \
        FittedGatedTOFs, FittedGatedTOFsWM, slowing_time =\
        EDM.Slowing(Scan, fileLabels[i], SigStart, SigEnd, BkgStart, BkgEnd,\
                gatesStart, gatesEnd, gateC, gateLength, distance,\
                RestSP, RestSPerr, v_exp, wavelength, Ref,\
                RefTCLconv, RefTCLconverr,\
                showTOF=False, TOFshot=20, detector=0, showFitTOF=True, \
                    plotFitTOF=True, showVelocity=True)

    PeakVOn[fileLabels[i]] = meanVOn
    PeakVOnerr[fileLabels[i]] = meanVOnerr
    PeakVOff[fileLabels[i]] = meanVOff
    PeakVOfferr[fileLabels[i]] = meanVOfferr

    velocityFigs[fileLabels[i]] = velocityFig
    AllFittedGatedTOFs[fileLabels[i]] = FittedGatedTOFs
    slowing_times[fileLabels[i]] = slowing_time


#%% Get change in peak velocities
if not Ref:
    print("No reference scan, so no velocities to compare.")
else:
    for i in range(0, len(fileLabels)):
        diffV = PeakVOn[fileLabels[i]] - PeakVOff[fileLabels[i]]
        diffVerr = np.sqrt(PeakVOnerr[fileLabels[i]]**2 + PeakVOfferr[fileLabels[i]]**2)

        plt.figure()
        plt.plot(gateC, diffV, '.', markersize=15, label="from setpoint")
        plt.errorbar(gateC, diffV, yerr=diffVerr, capsize=3, fmt=' ')
        plt.title("Velocity change of file " + fileLabels[i] +\
                  " with %g ms slowing"%slowing_times[fileLabels[i]])
        plt.xlabel("Gate center (ms)")
        plt.ylabel("Velocity change (m/s)")
        plt.legend()
        plt.show()
        plt.close()
