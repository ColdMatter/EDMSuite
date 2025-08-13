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
month="July2025"
date="23"
subfolder = "Circular masks\\"
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolder
print(drive)

pattern="*PMTmaskTest*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    Data = {}
    fileLabels = []
    for i in range(0, len(files)):
        fileLabel = re.split(r'[\\]', files[i])[-1][-7:-4]
        Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
        print("loaded file " + files[i])
        fileLabels.append(fileLabel)

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
Figs = []
AvgTOFOns = []
AvgTOFOnerrs = []

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
    
    for j in range(0, len(DataOn[0])):
        BkgOn.append(np.average(DataOn[0][i][BkgStartIndex:BkgEndIndex]))
        BkgOnstd.append(np.std(DataOn[0][i][BkgStartIndex:BkgEndIndex]))
        #BkgOff = np.average(DataOff[0][0][BkgStartIndex:BkgEndIndex])
    
    BkgOn = np.average(BkgOn)
    BkgOnstdavg = np.average(BkgOnstd)
        
    AvgTOFOn = np.average(DataOn[0], axis=0)-BkgOn
    AvgTOFOnerr = np.sqrt(np.std(DataOn[0], axis=0)**2 + BkgOnstdavg**2) / \
        np.sqrt(Settings["pointsPerScan"] * Settings["shotsPerPoint"])
    #AvgTOFOff = np.average(DataOff[0], axis=0)
    
    fig = plt.figure()
    plt.plot(TimeOn*1000, AvgTOFOn,\
             label="On")
    plt.fill_between(TimeOn*1000, AvgTOFOn+AvgTOFOnerr, AvgTOFOn-AvgTOFOnerr, \
                     alpha=0.3)
    #plt.plot(TimeOff*1000, DataOff[0][shot_for_TOF],\
    #         label="Off")
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
    
    Figs.append(fig)
    AvgTOFOns.append(AvgTOFOn)
    AvgTOFOnerrs.append(AvgTOFOnerr)

#%% Combine plot
comb = plt.figure()
for i in range(0, len(files)):
    plt.plot(TimeOn*1000, AvgTOFOns[i], label=fileLabels[i])
    plt.fill_between(TimeOn*1000, AvgTOFOns[i]+AvgTOFOnerrs[i],\
                     AvgTOFOns[i]-AvgTOFOnerrs[i], \
                     alpha=0.3)
plt.title("Averaged TOF over %g shots, bkg sub, on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.legend(loc="right")
plt.show()
plt.close()
