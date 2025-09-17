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
date="13"
subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolder
print(drive)

pattern="*TOF*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])
#%%
if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    Data = {}
    fileLabels = []
    for i in range(0, len(files)):
        fileLabel = re.split(r'[\\]', files[i])[-1][0:3]
        Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
        print("loaded file " + files[i])
        fileLabels.append(fileLabel)

else:
    print("No matching files.")

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 35
SigEnd = 50
BkgStart = 60
BkgEnd = 80

showTOF = True
showDiff = True

#%% Get averaged TOFs over all shots
Figs = {}
FigDiffs = {}
AvgTOFOns = {}
AvgTOFOnerrs = {}
AvgTOFOffs = {}
AvgTOFOfferrs = {}
DiffTOFs = {}
DiffTOFerrs = {}

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    
    #% Print out all params
    Settings = EDM.GetScanSettings(Scan)
    ScanParams = EDM.GetScanParameterArray(Scan)
    print("for file " + fileLabels[i])
    print(Settings)
    
    BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
    BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))
    
    TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFs(Scan)
    
    BkgOn = []
    BkgOnstd = []
    BkgOff = []
    BkgOffstd = []
    
    BkgSubOn = []
    BkgSubOff = []
    
    for j in range(0, len(DataOn[0])):
        AvgBkgOn = np.average(DataOn[0][j][BkgStartIndex:BkgEndIndex])
        BkgSubOn.append(DataOn[0][j] - AvgBkgOn)
        AvgBkgOff = np.average(DataOff[0][j][BkgStartIndex:BkgEndIndex])
        BkgSubOff.append(DataOff[0][j] - AvgBkgOff)
        
        BkgOn.append(AvgBkgOn)
        BkgOnstd.append(np.std(DataOn[0][j][BkgStartIndex:BkgEndIndex]))
        BkgOff = np.average(AvgBkgOff)
        BkgOffstd.append(np.std(DataOff[0][j][BkgStartIndex:BkgEndIndex]))
    
    BkgOn = np.average(BkgOn)
    BkgOnstdavg = np.average(BkgOnstd)
    BkgOff = np.average(BkgOff)
    BkgOffstdavg = np.average(BkgOffstd)
        
    AvgTOFOn = np.average(BkgSubOn, axis=0)
    AvgTOFOnerr = np.sqrt(np.std(BkgSubOn, axis=0)**2 + \
            (BkgOnstdavg/np.sqrt(BkgStartIndex-BkgEndIndex))**2) / \
        np.sqrt(Settings["pointsPerScan"] * Settings["shotsPerPoint"])
    AvgTOFOff = np.average(BkgSubOff, axis=0)
    AvgTOFOfferr = np.sqrt(np.std(BkgSubOff, axis=0)**2 + \
            (BkgOnstdavg/np.sqrt(BkgStartIndex-BkgEndIndex))**2) / \
        np.sqrt(Settings["pointsPerScan"] * Settings["shotsPerPoint"])
    
    fig = plt.figure()
    plt.plot(TimeOn*1000, AvgTOFOn,\
             label="On")
    plt.fill_between(TimeOn*1000, AvgTOFOn+AvgTOFOnerr, AvgTOFOn-AvgTOFOnerr, \
                     alpha=0.3)
    plt.plot(TimeOff*1000, AvgTOFOff,\
             label="Off")
    plt.fill_between(TimeOff*1000, AvgTOFOff+AvgTOFOfferr, AvgTOFOff-AvgTOFOfferr, \
                     alpha=0.3)
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(AvgTOFOff), \
               ymax=np.max(AvgTOFOff),\
                   linestyles="dashed", colors="black")
    plt.vlines([10],\
               ymin=np.min(AvgTOFOff), \
               ymax=np.max(AvgTOFOff),\
                   linestyles="dashed", colors="red")
    plt.title("Averaged TOF over %g shots, bkg sub, file "%\
              (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) + fileLabels[i])
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    #plt.ylim(np.min(AvgTOFOff)-0.01, np.max(AvgTOFOff)+0.01)
    plt.ylim(-0.01, 0.05)
    plt.legend()
    if showTOF:
        plt.show()
    plt.close()
    
    figdiff = plt.figure()
    diff = AvgTOFOn-AvgTOFOff
    plt.plot(TimeOn*1000, diff)
    differr = np.sqrt(AvgTOFOn**2+AvgTOFOnerr**2)
    plt.fill_between(TimeOn*1000, diff+differr,\
                     diff-differr, \
                     alpha=0.3)
    plt.title("Averaged TOF difference (On-Off) over %g shots, bkg sub, \n on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month + ", " + fileLabels[i])
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    #plt.xlim(65, 80)
    plt.ylim(-0.01, 0.03)
    #plt.ylim(np.min(AvgTOFOffs[i])-0.01, np.max(AvgTOFOffs[i])+0.01)
    #plt.legend(loc="upper right", bbox_to_anchor=(1.3, 1.05))
    if showDiff:
        plt.show()
    plt.close()
    
    
    Figs[fileLabels[i]] = fig
    FigDiffs[fileLabels[i]] = figdiff
    AvgTOFOns[fileLabels[i]] = AvgTOFOn
    AvgTOFOnerrs[fileLabels[i]] = AvgTOFOnerr
    AvgTOFOffs[fileLabels[i]] = AvgTOFOff
    AvgTOFOfferrs[fileLabels[i]] = AvgTOFOfferr
    DiffTOFs[fileLabels[i]] = diff
    DiffTOFerrs[fileLabels[i]] = differr

#%% Combine plot
compare = ["004", "008", "009", "010"]
comb = plt.figure()
for i in compare:
    plt.plot(TimeOn*1000, AvgTOFOns[i], label=i+"On")
    plt.fill_between(TimeOn*1000, AvgTOFOns[i]+AvgTOFOnerrs[i],\
                     AvgTOFOns[i]-AvgTOFOnerrs[i], \
                     alpha=0.3)
    plt.plot(TimeOff*1000, AvgTOFOffs[i],\
             label=i+"Off")
    plt.fill_between(TimeOff*1000, AvgTOFOffs[i]+AvgTOFOfferrs[i],\
                     AvgTOFOffs[i]-AvgTOFOfferrs[i], \
                     alpha=0.3)
plt.title("Averaged TOF over %g shots, bkg sub, \n on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
#plt.xlim(65, 80)
plt.ylim(-0.01, 0.05)
#plt.ylim(np.min(AvgTOFOffs[i])-0.01, np.max(AvgTOFOffs[i])+0.01)
plt.legend(loc="upper right", bbox_to_anchor=(1.3, 1.05))
plt.show()
plt.close()

#%% Difference
compare = ["004", "008", "009", "010"]
for i in compare:
    diff = AvgTOFOns[i]-AvgTOFOffs[i]
    plt.plot(TimeOn*1000, diff, label=i+"On-Off")
    differr = np.sqrt(AvgTOFOns[i]**2+AvgTOFOnerrs[i]**2)
    plt.fill_between(TimeOn*1000, diff+differr,\
                     diff-differr, \
                     alpha=0.3)
plt.title("Averaged TOF over %g shots, bkg sub, \n on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.xlim(25, 80)
plt.ylim(-0.001, 0.01)
#plt.ylim(np.min(AvgTOFOffs[i])-0.01, np.max(AvgTOFOffs[i])+0.01)
plt.legend(loc="upper right", bbox_to_anchor=(1.3, 1.05))
plt.show()
plt.close()

#%% with moving average
compare = ["004", "008", "009", "010"]
#compare = ["004", "008", "009", "010", "011", "012", "013", "014", "015", "016"]
MA = 100
for i in compare:
    diffMA = tools.MovingAverage(MA, DiffTOFs[i])
    MAtime = tools.MovingAverage(MA, TimeOn*1000)
    
    plt.plot(MAtime, diffMA, label=i+"On-Off")
    
plt.title("Averaged TOF over %g shots, bkg sub, \n on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month + " with moving average of %g"%MA)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.xlim(25, 80)
plt.ylim(-0.001, 0.01)
#plt.ylim(np.min(AvgTOFOffs[i])-0.01, np.max(AvgTOFOffs[i])+0.01)
plt.legend(loc="upper right", bbox_to_anchor=(1.3, 1.05))
plt.show()
plt.close()

#%%
"""Average part of the scan, in case the signal got averaged out"""
AvgLength = 300
Nshots = Settings["pointsPerScan"] * Settings["shotsPerPoint"]
AvgStarts = np.arange(0, Nshots, AvgLength)
AvgEnds = AvgStarts + AvgLength

Figs = {}
AvgTOFOns = {}
AvgTOFOnerrs = {}
AvgTOFOffs = {}
AvgTOFOfferrs = {}

for c in compare:
    Scan = Data[c]
    
    #% Print out all params
    Settings = EDM.GetScanSettings(Scan)
    ScanParams = EDM.GetScanParameterArray(Scan)
    print(Settings)
    
    BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
    BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))
    
    TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFs(Scan)
    
    fig = plt.figure()
    
    for A in AvgStarts:
        BkgOn = []
        BkgOnstd = []
        BkgOff = []
        BkgOffstd = []
        
        BkgSubOn = []
        BkgSubOff = []
        
        for j in range(0+A, AvgLength+A):
            AvgBkgOn = np.average(DataOn[0][j][BkgStartIndex:BkgEndIndex])
            BkgSubOn.append(DataOn[0][j] - AvgBkgOn)
            AvgBkgOff = np.average(DataOff[0][j][BkgStartIndex:BkgEndIndex])
            BkgSubOff.append(DataOff[0][j] - AvgBkgOff)
            
            BkgOn.append(np.average(DataOn[0][j][BkgStartIndex:BkgEndIndex]))
            BkgOnstd.append(np.std(DataOn[0][j][BkgStartIndex:BkgEndIndex]))
            BkgOff = np.average(DataOff[0][j][BkgStartIndex:BkgEndIndex])
            BkgOffstd.append(np.std(DataOff[0][j][BkgStartIndex:BkgEndIndex]))
        
        BkgOn = np.average(BkgOn)
        BkgOnstdavg = np.average(BkgOnstd)
        BkgOff = np.average(BkgOff)
        BkgOffstdavg = np.average(BkgOffstd)
            
        AvgTOFOn = np.average(BkgSubOn, axis=0)
        AvgTOFOnerr = np.sqrt(np.std(BkgSubOn, axis=0)**2\
                              + BkgOnstdavg**2)/np.sqrt(AvgLength)
        AvgTOFOff = np.average(BkgSubOff, axis=0)
        AvgTOFOfferr = np.sqrt(np.std(BkgSubOff, axis=0)**2\
                               + BkgOffstdavg**2)/np.sqrt(AvgLength)
            
        AvgTOFOns[c+str(A)] = AvgTOFOn
        AvgTOFOnerrs[c+str(A)] = AvgTOFOnerr
        AvgTOFOffs[c+str(A)] = AvgTOFOff
        AvgTOFOfferrs[c+str(A)] = AvgTOFOfferr
    
        plt.plot(TimeOn*1000, AvgTOFOn,\
                 label="On, shots %g to %g"%(A, AvgLength+A))
        plt.fill_between(TimeOn*1000, AvgTOFOn+AvgTOFOnerr,\
                         AvgTOFOn-AvgTOFOnerr, \
                         alpha=0.3)
        plt.plot(TimeOff*1000, AvgTOFOff,\
                 label="Off, shots %g to %g"%(A, AvgLength+A))
        plt.fill_between(TimeOff*1000, AvgTOFOff+AvgTOFOfferr,\
                         AvgTOFOff-AvgTOFOfferr, \
                         alpha=0.3)
            
        plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
                   ymin=np.min(AvgTOFOff), \
                   ymax=np.max(AvgTOFOff),\
                       linestyles="dashed", colors="black")
        plt.title("Averaged TOF over %g shots, bkg sub, file "%\
                  (AvgLength) + c)
        plt.xlabel("time (ms)")
        plt.ylabel("PMT signal (V)")
        plt.ylim(-0.01, 0.05)
        plt.xlim(30, 50)
        #plt.ylim(np.min(AvgTOFOff)-0.01, np.max(AvgTOFOff)+0.01)
        plt.legend()
        if showTOF:
            plt.show()
        plt.close()
        
        Figs[c+str(A)] = fig

#%%
cmap = plt.get_cmap('jet')
colors = cmap(np.linspace(0, 1.0, len(compare)*len(AvgStarts)*2))

count = 0

for i in compare:
    for A in AvgStarts:
        #norm = np.average(AvgTOFOns[i+A][0:500])
        #normOff = np.average(AvgTOFOffs[i+A][0:500])
        plt.plot(TimeOn*1000, AvgTOFOns[i+str(A)], label=i+"On"+str(A), \
                 color = colors[count])
        plt.fill_between(TimeOn*1000, AvgTOFOns[i+str(A)]+AvgTOFOnerrs[i+str(A)],\
                         AvgTOFOns[i+str(A)]-AvgTOFOnerrs[i+str(A)], \
                         alpha=0.3)
        plt.plot(TimeOff*1000, AvgTOFOffs[i+str(A)],\
                 label=i+"Off"+str(A), color = colors[count+1])
        plt.fill_between(TimeOff*1000, AvgTOFOffs[i+str(A)]+AvgTOFOfferrs[i+str(A)],\
                         AvgTOFOffs[i+str(A)]-AvgTOFOfferrs[i+str(A)], \
                         alpha=0.3)
        count = count+2
        
plt.title("Averaged TOF over %g shots, bkg sub, normalised to 0-5ms \n on "%\
          (AvgLength) +\
              date + " " + month)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.xlim(65, 80)
plt.ylim(-0.01, 0.01)
#plt.ylim(np.min(AvgTOFOffs[i])-0.01-normOff, np.max(AvgTOFOffs[i])+0.01-normOff)
plt.legend(loc="upper right", bbox_to_anchor=(1.3, 1.05))
plt.show()
plt.close()
