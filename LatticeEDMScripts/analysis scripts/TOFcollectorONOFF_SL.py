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
from scipy.optimize import curve_fit
import pandas as pd

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
month = "Sept 2026"
date = "22"
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\"# + subfolder
print(drive)

pattern="*TOF*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])
#%% Selection
sele = []#["005", "006", "007", "009"]

#%%
LoadPasses = True

if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    Data = {}
    fileLabels = []
    Lasers = []
    for i in range(0, len(files)):
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[0]
        Laser = re.split(r'[.]', re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[-2])[0]
       
        if len(sele) > 0:
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
#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 22
SigEnd = 24
BkgStart = 70
BkgEnd = 78

showTOF = False
showDiff = False

#%% Get averaged TOFs over all shots
Figs = {}
FigDiffs = {}
AvgTOFOns = {}
AvgTOFOnerrs = {}
AvgTOFOffs = {}
AvgTOFOfferrs = {}
DiffTOFs = {}
DiffTOFerrs = {}

#Gated
OnBkgSubs = {}  #Not averaged
OffBkgSubs = {}  #Not averaged
Ratios = {}  #Ratios of averaged gated TOFs
Ratioerrs = {}  #Standard error of mean, which is std/sqrt(N)

for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    
    #% Print out all params
    Settings = EDM.GetScanSettings(Scan)
    ScanParams = EDM.GetScanParameterArray(Scan)
    print("for file " + fileLabels[i])
    print(Settings)
    
    BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
    BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))
    
    TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Scan)
    
    #TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFs(Scan)
    
    BkgOn = []
    BkgOnstd = []
    BkgOff = []
    BkgOffstd = []
    
    BkgSubOn = []
    BkgSubOff = []
    
    for j in range(0, len(DataOnSPP[0])):
        AvgBkgOn = np.average(DataOnSPP[0][j][BkgStartIndex:BkgEndIndex][0])
        BkgSubOn.append(DataOnSPP[0][j][::].flatten() - AvgBkgOn)
        AvgBkgOff = np.average(DataOffSPP[0][j][BkgStartIndex:BkgEndIndex][0])
        BkgSubOff.append(DataOffSPP[0][j][::].flatten() - AvgBkgOff)
        
        BkgOn.append(AvgBkgOn)
        BkgOnstd.append(np.std(DataOnSPP[0][j][BkgStartIndex:BkgEndIndex]))
        BkgOff = np.average(AvgBkgOff)
        BkgOffstd.append(np.std(DataOffSPP[0][j][BkgStartIndex:BkgEndIndex]))
        
        # Get gated TOF against scanned param with bkg sub
        
    
    BkgOn = np.average(BkgOn)
    BkgOnstdavg = np.average(BkgOnstd)
    BkgOff = np.average(BkgOff)
    BkgOffstdavg = np.average(BkgOffstd)
        
    AvgTOFOn = np.average(BkgSubOn, axis=0)
    AvgTOFOnerr = np.sqrt(np.std(BkgSubOn, axis=0)**2 + \
            (BkgOnstdavg/np.sqrt(BkgEndIndex-BkgStartIndex))**2) / \
        np.sqrt(Settings["pointsPerScan"] * Settings["shotsPerPoint"])
    AvgTOFOff = np.average(BkgSubOff, axis=0)
    AvgTOFOfferr = np.sqrt(np.std(BkgSubOff, axis=0)**2 + \
            (BkgOnstdavg/np.sqrt(BkgEndIndex-BkgStartIndex))**2) / \
        np.sqrt(Settings["pointsPerScan"] * Settings["shotsPerPoint"])
   
    OnBkgSub, OffBkgSub = EDM.GatedAvgCountsOnOff(Scan,DataOnSPP[0],DataOffSPP[0],\
                                                  TimeOnSPP,TimeOffSPP,\
                    SigStart,SigEnd,BkgStart,BkgEnd)
    
    R = OnBkgSub/OffBkgSub
    Ravg = np.average(R)
    Rerr = np.std(R)/Settings["pointsPerScan"]
    
    fig = plt.figure()
    plt.plot(TimeOnSPP*1000, AvgTOFOn,\
             label="On")
    plt.fill_between(TimeOnSPP*1000, AvgTOFOn+AvgTOFOnerr, AvgTOFOn-AvgTOFOnerr, \
                     alpha=0.3)
    plt.plot(TimeOffSPP*1000, AvgTOFOff,\
             label="Off")
    plt.fill_between(TimeOffSPP*1000, AvgTOFOff+AvgTOFOfferr, AvgTOFOff-AvgTOFOfferr, \
                     alpha=0.3)
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(AvgTOFOn), \
               ymax=np.max(AvgTOFOn),\
                   linestyles="dashed", colors="black")
    plt.vlines([10],\
               ymin=np.min(AvgTOFOn), \
               ymax=np.max(AvgTOFOn),\
                   linestyles="dashed", colors="red")
    plt.title("Averaged TOF over %g shots, bkg sub, file "%\
              (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) + fileLabels[i])
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    #plt.ylim(np.min(AvgTOFOff)-0.01, np.max(AvgTOFOff)+0.01)
    #plt.ylim(-0.1, 0.5)
    plt.legend()
    if showTOF:
        plt.show()
    plt.close()
    
    figdiff = plt.figure()
    diff = AvgTOFOn-AvgTOFOff
    plt.plot(TimeOnSPP*1000, diff)
    differr = np.sqrt(AvgTOFOn**2+AvgTOFOnerr**2)
    plt.fill_between(TimeOnSPP*1000, diff+differr,\
                     diff-differr, \
                     alpha=0.3)
    plt.title("Averaged TOF difference (On-Off) over %g shots, bkg sub, \n on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month + ", " + fileLabels[i])
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    #plt.xlim(65, 80)
    #plt.ylim(-0.1, 0.5)
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
    
    OnBkgSubs[fileLabels[i]] = OnBkgSub
    OffBkgSubs[fileLabels[i]] = OffBkgSub
    Ratios[fileLabels[i]] = Ravg
    Ratioerrs[fileLabels[i]] = Rerr

#%% Plot average TOF of the first file
plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0], axis=0),\
         label="On")
#plt.fill_between(TimeOn*1000, AvgTOFOn+AvgTOFOnerr, AvgTOFOn-AvgTOFOnerr, \
#                 alpha=0.3)
plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0], axis=0),\
         label="Off")
#plt.fill_between(TimeOff*1000, AvgTOFOff+AvgTOFOfferr, AvgTOFOff-AvgTOFOfferr, \
#                 alpha=0.3)
plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
           ymin=-0.1, \
           ymax=0.5,\
               linestyles="dashed", colors="black")
#plt.vlines([10],\
#           ymin=np.min(AvgTOFOn), \
#           ymax=np.max(AvgTOFOn),\
#               linestyles="dashed", colors="red")
plt.title("Averaged TOF over %g shots, file "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) + fileLabels[i])
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
#plt.ylim(np.min(AvgTOFOff)-0.01, np.max(AvgTOFOff)+0.01)
plt.ylim(0, 1.)
plt.legend()
if showTOF:
    plt.show()
plt.close()

#%% Combine plot
compare = ["004", "008", "009", "010"]
comb = plt.figure()
for i in compare:
    plt.plot(TimeOnSPP*1000, AvgTOFOns[i], label=i+"On")
    plt.fill_between(TimeOnSPP*1000, AvgTOFOns[i]+AvgTOFOnerrs[i],\
                     AvgTOFOns[i]-AvgTOFOnerrs[i], \
                     alpha=0.3)
    plt.plot(TimeOffSPP*1000, AvgTOFOffs[i],\
             label=i+"Off")
    plt.fill_between(TimeOffSPP*1000, AvgTOFOffs[i]+AvgTOFOfferrs[i],\
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

#%% Combine for ratios for saturation measurements
#This is for Sept 8th 2026 4fv0 saturation
V3_file = ''  #For benchmark

P_Settings = {'007': 100,
            '008': 40,
            '009': 200,
            '010': 250,
            '011': 150,
            '012': 15,
            '014': 165,
            '015': 270,
            '016': 125,
            '017': 42.5,
            '018': 75}

#If forgot to take a bentchmark measurement, just put the values here
#Otherwise initialise as 0
benchmark = 0.524
benchmarkerr = 0.005

#Initialise with a (0, 0) point
R_sub = [0]
R_suberr = [1e-3]
P = [0]

for f in fileLabels:
    if f==V3_file or f[:3]==V3_file:
        benchmark = Ratios[f]
        benchmarkerr = Ratioerrs[f]
    else:
        continue

keys = list(P_Settings.keys())
for f in fileLabels:
    for k in keys:
        if f==k or f[:3]==k:
            R_sub.append(Ratios[f] - benchmark)
            R_suberr.append(np.sqrt(Ratioerrs[f]**2 + benchmarkerr**2))
            P.append(P_Settings[k])

# --- NEW: Group by duplicate P values and compute mean & propagated error ---
df = pd.DataFrame({'P': P, 'R_sub': R_sub, 'R_suberr': R_suberr})

# Custom aggregation function for propagating error: sqrt(sum(err^2)) / N
def prop_err(errs):
    return np.sqrt(np.sum(errs**2)) / len(errs)

df_avg = df.groupby('P', as_index=False).agg({
    'R_sub': 'mean',
    'R_suberr': prop_err
}).sort_values('P')

# Extract averaged arrays for plotting and fitting
P_proc = df_avg['P'].to_numpy()
R_sub_proc = df_avg['R_sub'].to_numpy()
R_suberr_proc = df_avg['R_suberr'].to_numpy()

# --- Plotting & Fitting using averaged data ---
plt.plot(P_proc, R_sub_proc, '.', color=colors[0])
plt.errorbar(P_proc, R_sub_proc, R_suberr_proc, fmt=' ', capsize=5, color=colors[0])

#fit, cov = curve_fit(tools.exp_decay, P_proc, R_sub_proc, p0=[-0.1, 3000., 0.15],
#                     absolute_sigma=True, sigma=R_suberr_proc)
#err = np.sqrt(np.diag(cov))

fit2, cov2 = curve_fit(tools.Saturation, P_proc, R_sub_proc, p0=[0.1, 400.],
                     absolute_sigma=True, sigma=R_suberr_proc)
err2 = np.sqrt(np.diag(cov2))

P_span = np.arange(0, 300, 1)
#plt.plot(P_span, tools.exp_decay(P_span, *fit), '-.', color='black',
#         label=r'exp fit: $I_{sat}=%.4g \pm %.2gmW$'%(fit[1], err[1]))
plt.plot(P_span, tools.Saturation(P_span, *fit2), '-.', color='red',
         label=r'Saturation fit: $I_{sat}=%.4g \pm %.2gmW$'%(fit2[1], err2[1]))

plt.xlabel("4fv0 power (mW)")
plt.ylabel("Relative on/off ratio")
plt.title("4f v1 repump saturation, with 5ms slowing \n"+
          "with 1.6W 4fv0 for 1:1 R & Q lines")
plt.legend()
plt.show()

# 3. Compute weighted residuals
#residuals = R_sub_proc - tools.exp_decay(P_proc, *fit)
#chi_squared = np.sum((residuals / R_suberr_proc) ** 2)

residuals2 = R_sub_proc - tools.Saturation(P_proc, *fit2)
chi_squared2 = np.sum((residuals2 / R_suberr_proc) ** 2)

# 4. Calculate degrees of freedom (N points - k parameters)
n_data = len(R_sub_proc)
#n_params = len(fit)
n_params2 = len(fit2)
#dof = n_data - n_params
dof2 = n_data - n_params2

# 5. Reduced chi-squared
#red_chi_sq = chi_squared / dof
red_chi_sq2 = chi_squared2 / dof2

#print("Exponential fit results: ", fit)
#print("Errors: ", err)
#print("With reduced chi-squared = %g"%red_chi_sq)
print("Saturation fit results: ", fit2)
print("Errors: ", err2)
print("With reduced chi-squared = %g"%red_chi_sq2)
#%% Difference
compare = ["007", "008"]
for i in compare:
    diff = AvgTOFOns[i]-AvgTOFOffs[i]
    plt.plot(TimeOnSPP*1000, diff, label=i+"On-Off")
    differr = np.sqrt(AvgTOFOns[i]**2+AvgTOFOnerrs[i]**2)
    plt.fill_between(TimeOnSPP*1000, diff+differr,\
                     diff-differr, \
                     alpha=0.3)
plt.title("Averaged TOF over %g shots, bkg sub, \n on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.xlim(25, 80)
plt.ylim(-0.05, 0.1)
#plt.ylim(np.min(AvgTOFOffs[i])-0.01, np.max(AvgTOFOffs[i])+0.01)
plt.legend(loc="upper right", bbox_to_anchor=(1.3, 1.05))
plt.show()
plt.close()

#%% with moving average
compare = ["007", "008"]
#compare = ["004", "008", "009", "010", "011", "012", "013", "014", "015", "016"]
MA = 100
for i in compare:
    diffMA = tools.MovingAverage(MA, DiffTOFs[i])
    MAtime = tools.MovingAverage(MA, TimeOnSPP*1000)
    
    plt.plot(MAtime, diffMA, label=i+"On-Off")
    
plt.title("Averaged TOF over %g shots, bkg sub, \n on "%\
          (Settings["pointsPerScan"] * Settings["shotsPerPoint"]) +\
              date + " " + month + " with moving average of %g"%MA)
plt.xlabel("time (ms)")
plt.ylabel("PMT signal (V)")
plt.xlim(25, 80)
plt.ylim(-0.05, 0.1)
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
