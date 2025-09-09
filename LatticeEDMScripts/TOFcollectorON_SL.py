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
MOTPMT2 = 3e7

DownstreamDetectEff = 0.12
MOTDetectEff = 0.06

PowerRatio = 10/35
BeamRatio = (3/10)**2

# Need counts/V/ms

Timebin = 1e-5 # 10us time step

#Plot in counts/time step:
Molecules = {}
for i in range(0, len(files)):
    if locations[i] == "Downstream":
        Molecule = AvgTOFOns[fileLabels[i]] * DownstreamPMT * Timebin
           # DownstreamDetectEff * Timebin
        Molecules[fileLabels[i]] = Molecule
        plt.plot(TimeOn*1000, Molecule, label=locations[i])
        
    if locations[i] == "MOT":
        Molecule = AvgTOFOns[fileLabels[i]] * MOTPMT2 * Timebin
            #MOTDetectEff  * PowerRatio# * BeamRatio
        Molecules[fileLabels[i]] = Molecule
        plt.plot(TimeOn*1000, Molecule, label=locations[i])

plt.title("Averaged TOF observed over %g shots, bkg sub, on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month )#+ \
              #  "\n scaled for detection efficiency, PMT calibrations" + \
               #     "\n power and beam size ratios")
plt.xlabel("time (ms)")
plt.ylabel("Molecules at location")
plt.legend(loc="right")
plt.show()
plt.close()