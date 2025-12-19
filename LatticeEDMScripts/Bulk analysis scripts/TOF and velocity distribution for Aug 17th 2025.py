# -*- coding: utf-8 -*-
"""
Created on Wed Aug 27 10:34:39 2025

For TOF and velocity distribution analysis for Aug 17th data

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

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Load data
'''Rest frame setpoints'''
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
SigStart = 10
SigEnd = 50
BkgStart = 60
BkgEnd = 70

wavelength = 552
angle = 60

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
'''TOF collector ON'''
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
    plt.vlines([BkgStart, BkgEnd],\
               ymin=np.min(AvgTOFOn), \
               ymax=np.max(AvgTOFOn),\
                   linestyles="dashed", colors="black")
    plt.vlines([SigStart, SigEnd],\
               ymin=np.min(AvgTOFOn), \
               ymax=np.max(AvgTOFOn),\
                   linestyles="dashed", colors="red")
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
plt.vlines([BkgStart, BkgEnd],\
           ymin=np.min(AvgTOFOn), \
           ymax=np.max(AvgTOFOns[fileLabels[0]]),\
               linestyles="dashed", colors="black")
plt.vlines([SigStart, SigEnd],\
           ymin=np.min(AvgTOFOn), \
           ymax=np.max(AvgTOFOns[fileLabels[0]]),\
               linestyles="dashed", colors="red")
plt.title("Averaged TOF over %g shots with background subtraction")
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.legend(loc="upper right")
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

plt.vlines([BkgStart, BkgEnd],\
           ymin=np.min(Molecule), \
           ymax=np.max(Molecules[fileLabels[0]]),\
               linestyles="dashed", colors="black")
plt.vlines([SigStart, SigEnd],\
           ymin=np.min(Molecule), \
           ymax=np.max(Molecules[fileLabels[0]]),\
               linestyles="dashed", colors="red")

plt.title("Averaged TOF over 100 shots with background subtraction")
plt.xlabel("time (ms)")
plt.ylabel("Molecules per time bin (10μs)")
plt.legend(loc="upper right")
plt.show()
plt.close()

#%% Get total number of molecules
SigStartIndex = int(SigStart * (Settings["sampleRate"]/1000))
SigEndIndex = int(SigEnd * (Settings["sampleRate"]/1000))

DSmol = np.sum(Molecules['002'][SigStartIndex:SigEndIndex])
print("Molecules in Downstream, %g to %g ms gate: "%(SigStart, SigEnd) + str(DSmol))

MOTmol = np.sum(Molecules['021'][SigStartIndex:SigEndIndex])
print("Molecules in MOT, %g to %g ms gate: "%(SigStart, SigEnd) + str(MOTmol))

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
            
        plt.plot(TimeOn*1000, Molecules[fileLabels[i]] / NormDSmol,\
                 color=colors[0], alpha=0.5)
        plt.plot(TimeOn*1000, SkewedGaussian(TimeOn*1000, *fit) / NormDSmol, \
                 color=colors[0], label=locations[i])
    
    if locations[i] == "MOT":
        MOT_peak_height = np.max(SkewedGaussian(TimeOn*1000, *fit))
        MOT_peak_mean = fit[0]
        #Shift = MOT_peak_mean - DS_peak_mean
        Shift = 0
        NormMol[fileLabels[i]] = Molecules[fileLabels[i]] / MOT_peak_height
            
        plt.plot(TimeOn*1000 - Shift, Molecules[fileLabels[i]] / MOT_peak_height,\
                 color=colors[1], alpha=0.5)
        plt.plot(TimeOn*1000 - Shift, SkewedGaussian(TimeOn*1000, *fit) / MOT_peak_height, \
                 color=colors[1], label=locations[i])
    
plt.vlines([BkgStart, BkgEnd],\
           ymin=np.min(NormMol[fileLabels[0]]), \
           ymax=np.max(NormMol[fileLabels[0]]),\
               linestyles="dashed", colors="black")
plt.vlines([SigStart, SigEnd],\
           ymin=np.min(NormMol[fileLabels[0]]), \
           ymax=np.max(NormMol[fileLabels[0]]),\
               linestyles="dashed", colors="red")

#plt.title("Normalised and peak-matched averaged TOF over %g shots,\n bkg sub, on "%\
#          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
#              date + " " + month + \
#                "\n scaled for detection efficiency, PMT calibrations" + \
#                    "\n power and beam size ratios")
#plt.title("Normalised and peak-matched averaged TOF")
plt.title("Normalised averaged TOF")
plt.xlabel("time (ms)")
plt.ylabel("Molecules per time bin (10μs) \n normalised to Downstream TOF")
#plt.xlim(0, 70)
plt.legend(loc="upper right")
plt.show()
plt.close()

#%% angled probe, for velocity distribution calculation
'''Angled probe. Note the MOT one wasn't scanned far enough so don't use'''
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month=datadrive+"\\August2025\\"
date=month+"\\17\\"

drive=date

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

#%%
i=0
angle=60

#SigStart = 30
#SigEnd = 50

print(re.split(r'[\\]', files[i])[-1])

Scan = Data_angled[fileLabels_angled[i]]
Settings = EDM.GetScanSettings(Scan)
ScanParams = EDM.GetScanParameterArray(Scan)
print(Settings)

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Scan)

if int(f_iniTHz) == 542:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Scan, plot=True, Toprint=False)
    HasWM = True
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
else:
    HasWM = False
    print("Wrong fibre in WM.")

TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Scan)
MeanCounts, StderrCounts, TimeWindow = EDM.GetGatedAvgCounts(Scan,DataOnSPP[0],\
                                                    TimeOnSPP,SigStart,SigEnd)
BkgMeanCounts, BkgStderrCounts, BkgTimeWindow = EDM.GetGatedAvgCounts(Scan,\
                                        DataOnSPP[0],TimeOnSPP,BkgStart,BkgEnd)
BkgSub = MeanCounts - BkgMeanCounts * TimeWindow/BkgTimeWindow

FerrEsti = np.ones(len(f_relMHz))*TCLconverr

v, err = EDM.VelocityfromFreq(f_relMHz, FerrEsti, \
                          542.8091807917087-56.05068161*1e-6, 0.64214063, \
                         f_iniTHz, wavelength, angle)
    
peakOn = v[np.where(BkgSub == np.max(BkgSub))[0][0]]

from scipy.optimize import curve_fit
fitv, covv = curve_fit(tools.SkewedGaussian, v, BkgSub, p0=[peakOn, 30., 10., 1., 1.])
errv = np.sqrt(np.diag(covv))
vspan = np.arange(0., 250., 1.)

plt.plot(v, BkgSub, '.', label='Downstream', color=colors[i])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitv), color=colors[i])

plt.xlabel("Velocity (m/s)")
plt.ylabel("Gated LIF (ms.V)")
plt.title("Gated TOF over velocity with " +\
    str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate")
plt.xlim(0, 250)
plt.legend()
plt.show()


print("Peak velocity: %g +- %g m/s"%(fitv[0], errv[0]))
print("FWHM: %g +- %g m/s"%(fitv[1]*2*np.sqrt(2*np.log(2)), errv[1]*2*np.sqrt(2*np.log(2))))

#%%
'''Use the Off-shot of slowing data on Aug 7th for MOT velcity distribution'''
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month=datadrive+"\\August2025\\"
date=month+"\\07\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = date
print(drive)

pattern="*slowing*.zip"
files_angled = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files_angled])

i=3
location = re.split(r'[.]', re.split(r'[_]',\
                            re.split(r'[\\]', files_angled[i])[-1])[-1])[0]
fileLabel = re.split(r'[_]', re.split(r'[\\]', files_angled[i])[-1])[0]
Data_angled[fileLabel] = EDM.ReadAverageScanInZippedXML(files_angled[i])
print("loaded file " + files_angled[i])

#%
angle = 45
print(re.split(r'[\\]', files_angled[i])[-1])

Scan = Data_angled[fileLabel]
Settings = EDM.GetScanSettings(Scan)
ScanParams = EDM.GetScanParameterArray(Scan)
print(Settings)

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Scan)

if int(f_iniTHz) == 542:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Scan, plot=True, Toprint=False)
    HasWM = True
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
else:
    HasWM = False
    print("Wrong fibre in WM.")

#%%
SigStart = 10
SigEnd = 50
TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Scan)
MeanCounts, StderrCounts, TimeWindow = EDM.GetGatedAvgCounts(Scan,DataOffSPP[0],\
                                                    TimeOffSPP,SigStart,SigEnd)
BkgMeanCounts, BkgStderrCounts, BkgTimeWindow = EDM.GetGatedAvgCounts(Scan,\
                                        DataOffSPP[0],TimeOffSPP,BkgStart,BkgEnd)
BkgSubMOT = MeanCounts - BkgMeanCounts * TimeWindow/BkgTimeWindow

FerrEsti = np.ones(len(f_relMHz))*TCLconverr

'''It doesn't have a perpendicular probe scan on the day, just use the same one for now'''
vMOT, err = EDM.VelocityfromFreq(f_relMHz, FerrEsti, \
                          542.8091807917087-56.05068161*1e-6, 0.64214063, \
                         f_iniTHz, wavelength, angle)
    
peakOnMOT = vMOT[np.where(BkgSub == np.max(BkgSub))[0][0]]

from scipy.optimize import curve_fit
fitvMOT, covvMOT = curve_fit(tools.SkewedGaussian, vMOT, BkgSubMOT, \
                             p0=[peakOnMOT, 30., 10., 1., 1.])
errvMOT = np.sqrt(np.diag(covvMOT))
vspan = np.arange(0., 250., 1.)

plt.plot(vMOT, BkgSubMOT, '.', label='MOT', color=colors[i])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitvMOT), color=colors[i])

plt.xlabel("Velocity (m/s)")
plt.ylabel("Gated LIF (ms.V)")
plt.title("Gated TOF over velocity with " +\
    str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate")
plt.xlim(0, 250)
plt.legend()
plt.show()

print("Peak velocity: %g +- %g m/s"%(fitvMOT[0], errvMOT[0]))
print("FWHM: %g +- %g m/s"%(fitvMOT[1]*2*np.sqrt(2*np.log(2)), \
                            errvMOT[1]*2*np.sqrt(2*np.log(2))))

#%% Combine plots
base = fitv[3]
baseMOT = fitvMOT[3]

plt.plot(v, BkgSub-base, '.', label='Downstream', color=colors[0])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitv)-base, color=colors[0])

plt.plot(vMOT, BkgSubMOT-baseMOT, '.', label='MOT', color=colors[1])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitvMOT)-baseMOT, color=colors[1])

plt.xlabel("Velocity (m/s)")
plt.ylabel("Gated TOF (ms.V)")
plt.title("Gated TOF over velocity from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate")
plt.xlim(0, 250)
plt.legend()
plt.show()

print("Peak Downstream velocity: %g +- %g m/s"%(fitv[0], errv[0]))
print("Downstream FWHM: %g +- %g m/s"%(fitv[1]*2*np.sqrt(2*np.log(2)),\
                                       errv[1]*2*np.sqrt(2*np.log(2))))

print("Peak MOT velocity: %g +- %g m/s"%(fitvMOT[0], errvMOT[0]))
print("MOT FWHM: %g +- %g m/s"%(fitvMOT[1]*2*np.sqrt(2*np.log(2)), \
                            errvMOT[1]*2*np.sqrt(2*np.log(2))))

#%% Normalise
peak = np.max(tools.SkewedGaussian(vspan, *fitv)) - base
peakMOT = np.max(tools.SkewedGaussian(vspan, *fitvMOT)) - baseMOT

plt.plot(v, (BkgSub-base)/peak, '.', label='Downstream', color=colors[0])
plt.plot(vspan, (tools.SkewedGaussian(vspan, *fitv)-base)/peak, color=colors[0])

plt.plot(vMOT, (BkgSubMOT-baseMOT)/peakMOT, '.', label='MOT', color=colors[1])
plt.plot(vspan, (tools.SkewedGaussian(vspan, *fitvMOT)-baseMOT)/peakMOT, color=colors[1])

plt.xlabel("Velocity (m/s)")
plt.ylabel("Gated TOF normalised to Downstream")
plt.title("Normalised gated TOF over velocity from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms")
plt.xlim(0, 250)
plt.legend()
plt.show()


    
#%% To molecules
# Calibrations in counts/V/s
'''MOT PMT calibration is not certain, but close enough to that on Aug 17th'''
DownstreamPMT = 7e6
MOTPMT = 5e9
MOTPMT2 = 1e7

DownstreamDetectEff = 0.12
MOTDetectEff = 0.06

PowerRatio = 10/35
BeamRatio = (3/6.4)**2

cycling = 5

# Need counts/V/ms

Timebin = 1e-5 *(SigEnd-SigStart)*100 # 40ms time step (but don't use for gated TOF)

#Plot in counts/time step:
Molecule = (BkgSub-base) * DownstreamPMT/1000 / cycling\
   /DownstreamDetectEff
fitvmol, covvmol = curve_fit(tools.SkewedGaussian, v, Molecule, \
                                   p0=[peakOn, 30., 1e6, 1., 1.])
errvmol = np.sqrt(np.diag(covvmol))
plt.plot(v, Molecule, '.', label="Downstream", color=colors[0])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitvmol), color=colors[0])
        
MoleculeMOT = (BkgSubMOT-baseMOT) * MOTPMT2/1000 / cycling\
    /MOTDetectEff * BeamRatio
fitvMOTmol, covvMOTmol = curve_fit(tools.SkewedGaussian, vMOT, MoleculeMOT, \
                                   p0=[peakOnMOT, 30., 1e5, 1., 1.])
errvMOTmol = np.sqrt(np.diag(covvMOTmol))
plt.plot(vMOT, MoleculeMOT, '.', label="MOT", color=colors[1])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitvMOTmol), color=colors[1])

#plt.vlines([BkgStart, BkgEnd],\
#           ymin=np.min(Molecule), \
#           ymax=np.max(Molecule),\
#               linestyles="dashed", colors="black")
#plt.vlines([SigStart, SigEnd],\
#           ymin=np.min(Molecule), \
#           ymax=np.max(Molecule),\
#               linestyles="dashed", colors="red")

plt.title("Gated TOF over velocity from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms")
plt.xlabel("Velocity (m/s)")
plt.ylabel("Molecules in time bin (40ms)")
plt.legend(loc="upper right")
plt.ticklabel_format(axis='y', style='sci', scilimits=(4,3))
plt.xlim(0, 250)
plt.show()
plt.close()

#%% Plot as the ratio to total molecules in the time gate
plt.plot(v, Molecule/DSmol, '.', label="Downstream", color=colors[0])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitvmol)/DSmol, color=colors[0])

plt.plot(vMOT, MoleculeMOT/MOTmol, '.', label="MOT", color=colors[1])
plt.plot(vspan, tools.SkewedGaussian(vspan, *fitvMOTmol)/MOTmol, color=colors[1])

plt.title("Gated TOF over velocity from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms \n as a ratio to total molecules in the pulse")
plt.xlabel("Velocity (m/s)")
plt.ylabel("ratio to total molecules")
plt.legend(loc="upper right")
#plt.ticklabel_format(axis='y', style='sci', scilimits=(4,3))
plt.xlim(0, 250)
plt.show()
plt.close()