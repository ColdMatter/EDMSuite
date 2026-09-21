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
import scipy as sp

import glob
import matplotlib.pyplot as plt


import tools as tools

tools.set_plots()

#%% Load data
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month="August2025"
date="17"
subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolder
print(drive)

pattern="*TOF*.zip"
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
SigStart = 10
SigEnd = 40
BkgStart = 60
BkgEnd = 70

showTOF = True

#%% Get averaged TOFs over all shots
Figs = {}
AvgTOFOns = {}
AvgTOFOnerrs = {}

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    
    #% Print out all params
    Settings = EDM.GetScanSettings(Scan)
    ScanParams = EDM.GetScanParameterArray(Scan)
    print(Settings)
    
    BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
    BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))
    
    TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFs(Scan)
    
    BkgOn = []
    BkgOnstd = []
    
    BkgSubOn = []
    
    for j in range(0, len(DataOn[0])):
        AvgBkgOn = np.average(DataOn[0][j][BkgStartIndex:BkgEndIndex])
        BkgSubOn.append(DataOn[0][j] - AvgBkgOn)
        
        BkgOn.append(AvgBkgOn)
        BkgOnstd.append(np.std(DataOn[0][j][BkgStartIndex:BkgEndIndex]))
    
    BkgOn = np.average(BkgOn)
    BkgOnstdavg = np.average(BkgOnstd)
        
    AvgTOFOn = np.average(BkgSubOn, axis=0)
    AvgTOFOnerr = np.sqrt(np.std(BkgSubOn, axis=0)**2 + \
            (BkgOnstdavg/np.sqrt(BkgStartIndex-BkgEndIndex))**2) / \
        np.sqrt(Settings["pointsPerScan"] * Settings["shotsPerPoint"])
    
    fig = plt.figure()
    plt.plot(TimeOn*1000, AvgTOFOn,\
             label="On")
    plt.fill_between(TimeOn*1000, AvgTOFOn+AvgTOFOnerr, AvgTOFOn-AvgTOFOnerr, \
                     alpha=0.3)
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(AvgTOFOn), \
               ymax=np.max(AvgTOFOn),\
                   linestyles="dashed", colors="black")
    plt.title("Averaged TOF over %g shots, bkg sub, file "%\
              (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) + fileLabels[i])
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.legend()
    if showTOF:
        plt.show()
    plt.close()
    
    Figs[fileLabels[i]] = fig
    AvgTOFOns[fileLabels[i]] = AvgTOFOn
    AvgTOFOnerrs[fileLabels[i]] = AvgTOFOnerr

#%% Combine plot
comb = plt.figure()
for i in range(0, len(files)):
    plt.plot(TimeOn*1000, AvgTOFOns[fileLabels[i]], label=locations[i])
    plt.fill_between(TimeOn*1000, AvgTOFOns[fileLabels[i]]+AvgTOFOnerrs[fileLabels[i]],\
                     AvgTOFOns[fileLabels[i]]-AvgTOFOnerrs[fileLabels[i]], \
                     alpha=0.3)
plt.title("Averaged TOF over %g shots, bkg sub, on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.legend(loc="right")
plt.show()
plt.close()

#%% Convert to photon counts
# Calibrations in counts/V/s
DownstreamPMT = 7e6
MOTPMT = 5e9
MOTPMT2 = 1e7

DownstreamDetectEff = 0.12
MOTDetectEff = 0.06

PowerRatio = 10/35
BeamRatio = (3/6.4)**2

cycling = 5

# Need counts/V/ms

Timebin = 1e-5 # 10us time step

#Plot in counts/time step:
Molecules = {}
for i in range(0, len(files)):
    if locations[i] == "Downstream":
        Molecule = AvgTOFOns[fileLabels[i]] * DownstreamPMT * Timebin / cycling\
           /DownstreamDetectEff
        Molecules[fileLabels[i]] = Molecule
        plt.plot(TimeOn*1000, Molecule, label=locations[i])
        
    if locations[i] == "MOT":
        Molecule = AvgTOFOns[fileLabels[i]] * MOTPMT2 * Timebin / cycling\
            /MOTDetectEff * BeamRatio
        Molecules[fileLabels[i]] = Molecule
        plt.plot(TimeOn*1000, Molecule, label=locations[i])

plt.title("Averaged TOF over %g shots, bkg sub, on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month + \
                "\n scaled for detection efficiency, PMT calibrations" + \
                    "\n power and beam size ratios")
#plt.title("Averaged TOF over 100 shots with background subtraction")
plt.xlabel("time (ms)")
plt.ylabel("Molecules per time bin (10μs)")
plt.legend(loc="upper right")
plt.show()
plt.close()

#%% To velocity distribution
dDS = 1.5 #For Downstream, pre Aug 18th 2025. After is the MOT distance.
def timeFromVelocity(v, A, tau_v, d=dDS):
    return d/v + A*np.exp(-v/tau_v)

def VelocityFromTime(v, A, tau_v, t, d):
    return d/v + A*np.exp(-v/tau_v) - t
#This should equate to 0. Use this to find root --> find v

#From Aug 17th 2025 Downstream angled probe measurement:
A, tau_v = 102.70390736, 72.18821354

from scipy.optimize import fsolve
VelocityOn = []
for t in TimeOn:
    #arg = [A, tau_v, t*1000]
    v = fsolve(VelocityFromTime, x0=100., args=(A, tau_v, t*1000, dDS))[0]
    VelocityOn.append(v)

for i in range(0, len(files)):
    plt.plot(VelocityOn, Molecules[fileLabels[i]], label=locations[i])

plt.title("Velocity distribution reconstructed from averaged TOF, \n on "+\
              date + " " + month)
#plt.title("Averaged TOF over 100 shots with background subtraction")
plt.xlabel("Velocity (m/s)")
plt.ylabel("Molecules per time bin (10μs)")
plt.legend(loc="right")
plt.show()
plt.close()

#%% Check
v = np.arange(65, 170, step=0.1)
#compareV = dDS/TimeOn
plt.plot(TimeOn*1000, VelocityOn)
plt.plot(timeFromVelocity(v, A, tau_v), v)
#plt.plot(TimeOn*1000, compareV)
plt.show()

#%% Normalise
def SkewedGaussian(x, mean, std, amp, shift, gamma):
    skewness = 1 + sp.special.erf(gamma * (x-mean) / (np.sqrt(2) * std))
    exponent = -(x-mean)**2 / (2 * std**2)
    return amp * skewness * np.exp(exponent) + shift

from scipy.optimize import curve_fit

DS_peak_mean = 0
Norm = 0
NormMol = {}
for i in range(0, len(files)):
    fit, cov = curve_fit(SkewedGaussian, TimeOn*1000, Molecules[fileLabels[i]],\
                         p0=[20., 10., 100., 0., 1.])
    print(locations[i] + " Fit params (mean, std, amp, shift, gamma):")
    print(fit)
    
    if locations[i] == "Downstream":
        DS_peak_mean = fit[0]
        NormDSmol = np.max(SkewedGaussian(TimeOn*1000, *fit))
        Norm = NormDSmol
        NormMol[fileLabels[i]] = Molecules[fileLabels[i]] / NormDSmol
            
        plt.plot(TimeOn*1000, Molecules[fileLabels[i]] / NormDSmol, label=locations[i])
        plt.plot(TimeOn*1000, SkewedGaussian(TimeOn*1000, *fit) / NormDSmol, \
                 label = locations[i] + " fit")
    
    if locations[i] == "MOT":
        MOT_peak_height = np.max(SkewedGaussian(TimeOn*1000, *fit))
        MOT_peak_mean = fit[0]
        Shift = MOT_peak_mean - DS_peak_mean
        NormMol[fileLabels[i]] = Molecules[fileLabels[i]] / MOT_peak_height
            
        plt.plot(TimeOn*1000 - Shift, Molecules[fileLabels[i]] / MOT_peak_height, label=locations[i])
        plt.plot(TimeOn*1000 - Shift, SkewedGaussian(TimeOn*1000, *fit) / MOT_peak_height, \
                 label = locations[i] + " fit")
    
    

#plt.title("Normalised and peak-matched averaged TOF over %g shots,\n bkg sub, on "%\
#          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
#              date + " " + month + \
#                "\n scaled for detection efficiency, PMT calibrations" + \
#                    "\n power and beam size ratios")
plt.title("Normalised and peak-matched averaged TOF")
plt.xlabel("time (ms)")
plt.ylabel("Molecules per time bin (10μs) \n normalised to Downstream TOF")
plt.xlim(0, 70)
plt.legend(loc="upper right")
plt.show()
plt.close()

#%% In velocity distribution
plt.plot(VelocityOn, NormMol[fileLabels[0]], label=locations[0])

#get peak velocity
fitV, covV = curve_fit(SkewedGaussian, VelocityOn, NormMol[fileLabels[0]],\
                     p0=[130., 10., 100., 0., 1.])
errV = np.sqrt(np.diag(covV))
print("Peak velocity is: %g +- %g m/s"%(fitV[0], errV[0]))
print("Peak FWHM width is: %g +- %g m/s"%(fitV[1]*2*np.sqrt(2*np.log(2)), \
                                          errV[1]*2*np.sqrt(2*np.log(2))))

plt.plot(VelocityOn, SkewedGaussian(VelocityOn, *fitV),\
         label=locations[0]+" fit, \n peak at %.3g m/s"%fitV[0])
    
VelocityOnMOT = []
startInd = np.where(TimeOn*1000 > Shift)[0][0]
for t in TimeOn[startInd::]:
    #arg = [A, tau_v, t*1000]
    v = fsolve(VelocityFromTime, x0=100., args=(A, tau_v, t*1000 - Shift, dDS))[0]
    VelocityOnMOT.append(v)

#plt.plot(VelocityOnMOT, NormMol[fileLabels[1]][startInd::], label=locations[1])

#get peak velocity
fitV2, covV2 = curve_fit(SkewedGaussian, VelocityOnMOT, NormMol[fileLabels[1]][startInd::],\
                     p0=[130., 10., 100., 0., 1.])
errV2 = np.sqrt(np.diag(covV2))
print("Peak velocity is: %g +- %g m/s"%(fitV2[0], errV2[0]))
print("Peak FWHM width is: %g +- %g m/s"%(fitV2[1]*2*np.sqrt(2*np.log(2)),\
                                     errV2[1]*2*np.sqrt(2*np.log(2))))
plt.plot(VelocityOnMOT, SkewedGaussian(VelocityOnMOT, *fitV2), '-.', color='black',\
         label=locations[1]+" projection, \n peak at %.3g m/s"%fitV2[0])

plt.title("Normalised velocity distribution reconstructed from averaged TOF")#, \n on "+\
              #date + " " + month)
#plt.title("Averaged TOF over 100 shots with background subtraction")
plt.xlabel("Velocity (m/s)")
plt.ylabel("Molecules per velocity bin")
plt.xlim(0, 300)
plt.legend(loc="upper right")
plt.show()
plt.close()