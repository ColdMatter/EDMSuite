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
month=datadrive+"\\August 2024\\"
date=month+"\\13\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

pattern="003*.zip"
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
SigStart = 15
SigEnd = 25
BkgStart = 35
BkgEnd = 40

showTOF = True
shot_for_TOF = 17

#%%
Data = EDM.ReadAverageScanInZippedXML(files[0])

#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Data)

if int(f_iniTHz) == 542:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=True, Toprint=False)
    HasWM = True
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
else:
    HasWM = False
    print("Wrong fibre in WM.")

#%% Get TOFs, averaged per point
TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1),\
             label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1),\
             label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF]), \
               ymax=np.max(DataOnSPP[0][shot_for_TOF]),\
                   linestyles="dashed", colors="black")
    plt.title("TOF of scan point %g, averaged for all shots"%shot_for_TOF)
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.legend()
    plt.show()

#%% Bkg-sub TOF
BkgOn = np.average(DataOnSPP[0][shot_for_TOF][BkgStartIndex:BkgEndIndex])
BkgOff = np.average(DataOffSPP[0][shot_for_TOF][BkgStartIndex:BkgEndIndex])

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1)\
             -BkgOn, label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1)\
             -BkgOff, label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF])-BkgOn, \
               ymax=np.max(DataOnSPP[0][shot_for_TOF])-BkgOn,\
                   linestyles="dashed", colors="black")
    plt.title("TOF of the 20th scan point, averaged for all shots,\n \
              background subtracted")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.ylim(-0.3, 3.5)
    plt.legend()
    plt.show()

#%% Get gated TOF against scanned param with bkg sub
MeanCounts, StderrCounts, TimeWindow = EDM.GetGatedAvgCounts(Data,DataOnSPP[0],\
                                                    TimeOnSPP,SigStart,SigEnd)

BkgMeanCounts, BkgStderrCounts, BkgTimeWindow = EDM.GetGatedAvgCounts(Data,\
                                        DataOnSPP[0],TimeOnSPP,BkgStart,BkgEnd)
BkgSub = MeanCounts - BkgMeanCounts * TimeWindow/BkgTimeWindow

#% Plot gated TOF
GatedTOF = EDM.PlotGatedAvgCounts(Data,DataOnSPP[0],TimeOnSPP,SigStart,\
                                  SigEnd,BkgStart,BkgEnd)
#% Fitting

FittedGatedTOF, fit_results = tools.FitGaussian(GatedTOF, ScanParams,\
                                BkgSub, p0=[np.mean(ScanParams), 0.5, 20., 10.])

if HasWM:
    peakOn = f_relMHz[np.where(BkgSub == np.max(BkgSub))[0][0]]
    FittedGatedTOFWM, fit_resultsWM = tools.FitGaussian(0, f_relMHz, \
                                    BkgSub, p0=[peakOn, 10., 1., 1.],
                    xlabel="Relative frequency (MHz) from %.8g THz"%f_iniTHz,\
                    ylabel="Gated LIF (ms.V)", \
                    title="Gated TOF over " + Settings["param"] + " with " +\
                        str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
                        str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
                            fileLabel+location, Toprint=True, plot=True,\
      bounds=([-np.inf, 0, -np.inf, -np.inf], [np.inf, 100., np.inf, np.inf]))
#%% To velocity axis
wavelength = 552
angle = 60

FerrEsti = np.ones(len(f_relMHz))*TCLconverr

v, err = EDM.VelocityfromFreq(f_relMHz, FerrEsti, \
                          542.8091807917087-56.05068161*1e-6, 0.64214063, \
                         f_iniTHz, wavelength, angle)
    
peakOn = v[np.where(BkgSub == np.max(BkgSub))[0][0]]

from scipy.optimize import curve_fit
fitv, covv = curve_fit(tools.SkewedGaussian, v, BkgSub, p0=[peakOn, 30., 10., 1., 1.])
errv = np.sqrt(np.diag(covv))
vspan = np.arange(0., 250., 1.)

plt.plot(v, BkgSub, '.', label='Downstream')
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitv))

plt.xlabel("Velocity (m/s)")
plt.ylabel("Gated LIF (ms.V)")
plt.title("Gated TOF over " + Settings["param"] + " with " +\
    str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate")
plt.xlim(0, 250)
plt.legend()
plt.show()


print("Peak velocity: %g +- %g m/s"%(fitv[0], errv[0]))
print("FWHM: %g +- %g m/s"%(fitv[1]*2*np.sqrt(2*np.log(2)), errv[1]*2*np.sqrt(2*np.log(2))))