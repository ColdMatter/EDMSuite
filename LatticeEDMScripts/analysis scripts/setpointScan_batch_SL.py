# -*- coding: utf-8 -*-
"""
Created on Wed Aug  6 14:15:48 2025

To analyse setpoint scan measurements. Typically with On-Off shots (only On)

@author: sl5119
"""

#%% Import libraries
import sys
import os
import re

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
from scipy.optimize import curve_fit


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
datadrive = EDM.get_datadrive()  # set per device in data_locations.json
month = "Oct 2026"
date = "23"
#blockdrive=datadrive+"\\BlockData\\"

drive = os.path.join(datadrive, month, date, "")# + subfolder
print(drive)

pattern="*setpoint*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%% Selection
sele = ["013"] # leave empty, sele = [], to load all matching files

#%%
LoadPasses = True

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
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[0]
        Laser = re.split(r'[.]', re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[-2])[0]
       
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

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 27.5
SigEnd = 40
BkgStart = 70
BkgEnd = 78

showTOF = False
shot_for_TOF = 17

showSET = True   #To show setpoint decided on the day as a vertical line
SET = -0.119     #TCL or WM setpoint
SETfreq = 288.734637

fTHz = 288

TCLcal = -235.1  #in MHz/V
TCLcalerr = 0.83

#%% Extract from XML & initial processing
OnBkgSubs = {}
OffBkgSubs = {}
f_iniTHzs = {}
X_f_relMHzs = {} # X axis of the scan. Will be setpoint or WM (if present)
X_f_THz = {}
#GoodData = [] #Append index here if fitted tau error is larger than tau

for i in range(0, len(fileLabels)):  #for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    print('For file ' + re.split(r'[\\]', fileLabels[i])[-1])   #files[i]
    Settings = EDM.GetScanSettings(Scan)
    
    ScanParams = EDM.GetScanParameterArray(Scan)
    print(Settings)
    print("\n")

    BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
    BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

    f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Scan)

    if int(f_iniTHz) == fTHz:
        TCL_WM_cali = EDM.TCL_WM_Calibration(Scan, plot=True, Toprint=False)
        HasWM = True
        TCLconv = TCL_WM_cali['best fit'][0]
        TCLconverr = TCL_WM_cali['error'][0]
        print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
        
        if TCLconv < 800:
            print("Probably not recording the correct WM channel, " +
                  "and it's using Wavemeter Lock")
            HasWM = False
            Scan_F_offset = Settings["Freq offset THz"]
            if Scan_F_offset is None:
                WMLock = False
                print("Scan doesn't use WM lock")
            else:
                WMLock = True
                print("Scan uses WM lock.")
                print("Scan frequency offset (THz): %.10g"%Scan_F_offset)
    else:
        HasWM = False
        Scan_F_offset = Settings["Freq offset THz"]
        if Scan_F_offset is None:
            WMLock = False
            print("Scan doesn't use WM lock")
            TCLconv = TCLcal
            TCLconverr = TCLcalerr
            print("Wrong fibre in WM and not in WM lock. Use manual TCL calibration inputs.")
        else:
            WMLock = True
            print("Scan uses WM lock.")
            print("Scan frequency offset (THz): %.10g"%Scan_F_offset)
        

    
    #% Get TOFs, averaged per point
    TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Scan)
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
    
    #% Bkg-sub TOF
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
        #plt.ylim(-0.3, 3.5)
        plt.legend()
        plt.show()
        
    # Get gated TOF against scanned param with bkg sub
    OnBkgSub, OffBkgSub = EDM.GatedAvgCountsOnOff(Scan,DataOnSPP[0],DataOffSPP[0],\
                                                  TimeOnSPP,TimeOffSPP,\
                    SigStart,SigEnd,BkgStart,BkgEnd)
    
    # Sort by the scanned parameter: up-down scans run alternate passes in
    # decreasing order, so everything below sees increasing scan points.
    # ScanParams, WM readings and gated counts are all per point in acquisition
    # order, so one ordering applies to all of them
    if not (len(ScanParams) == len(f_relMHz) == len(OnBkgSub) == len(OffBkgSub)):
        raise ValueError("Scan arrays of " + fileLabels[i] + " have different lengths")
    order = np.argsort(ScanParams, kind='stable')
    ScanParams = ScanParams[order]
    f_relMHz = f_relMHz[order]
    OnBkgSub = OnBkgSub[order]
    OffBkgSub = OffBkgSub[order]
    # f_relMHz was relative to the first point taken (the top end on a down
    # pass); re-reference it to the first point after sorting
    f_iniTHz = f_iniTHz + f_relMHz[0]*1e-6
    f_relMHz = f_relMHz - f_relMHz[0]
    
    if HasWM:
        f_iniTHzs[fileLabels[i]] = f_iniTHz
        X_f_relMHzs[fileLabels[i]] = f_relMHz
        X_f_THz[fileLabels[i]] = f_relMHz * 1e-6 + f_iniTHz
    else:
        f_iniTHzs[fileLabels[i]] = 0
        X_f_relMHzs[fileLabels[i]] = ScanParams
        X_f_THz[fileLabels[i]] = ScanParams
    
    OnBkgSubs[fileLabels[i]] = OnBkgSub
    OffBkgSubs[fileLabels[i]] = OffBkgSub
        
    title="Gated TOF over " + Settings["param"] + " with " +\
        str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
        str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
            fileLabels[i]

    if HasWM:
        plt.plot(f_relMHz, OnBkgSub, '.', label='On')
        plt.plot(f_relMHz, OffBkgSub, '.', label='Off')
        plt.xlabel("Relative frequency (MHz) to %g THz"%f_iniTHz)
    else:
        plt.plot(ScanParams, OnBkgSub, '.', label='On')
        plt.plot(ScanParams, OffBkgSub, '.', label='Off')
        plt.xlabel('Setpoint (V)')
    plt.ylabel("Gated LIF (ms.V)")
    plt.title(title)
    plt.legend()
    plt.show()
    
    print("\n")
    
#%% Ratio (by passes/files)
if HasWM:
    f_offset = 0 #270.259491 #f_iniTHzs[fileLabels[0]]
if WMLock:
    f_offset = Scan_F_offset
else:
    print("No WM or WM lock. Everything in setpoints (V)")
    
MA = 3

Ratios = {}
for i in range(0, len(fileLabels)):
    Ratio = OnBkgSubs[fileLabels[i]]/OffBkgSubs[fileLabels[i]]
    Ratios[fileLabels[i]] = Ratio
    
    X = X_f_THz[fileLabels[i]]
    
    MoveAvg_R = tools.MovingAverage(MA, Ratio)
    MoveAvg_f = tools.MovingAverage(MA, X)
    
    #peakR = f_relMHz[np.where(Ratio == np.min(Ratio))[0][0]]
    
    #fitR, covR = curve_fit(tools.Gaussian, f_relMHz, Ratio, p0=[peakR, 30., -0.5, 1.])
    #errR = np.sqrt(np.diag(covR))
    
    #fspan = np.arange(np.min(f_relMHz), np.max(f_relMHz), step = 1.)
    
    title="Gated TOF over " + Settings["param"] + " with " +\
        str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
        str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
            fileLabels[i]
    
    if HasWM:
        plt.plot((X-f_offset)*1e6, Ratio, label='On/Off', color=colors[0])
        plt.plot((MoveAvg_f-f_offset)*1e6, MoveAvg_R, label="With moving average of %g"%MA, color=colors[1])
        
        plt.xlabel("Relative frequency (MHz) to %.10g THz"%f_offset)
    
    if WMLock:
        plt.plot(X*1e3, Ratio, label='On/Off', color=colors[0])
        plt.plot(MoveAvg_f*1e3, MoveAvg_R, label="With moving average of %g"%MA, color=colors[1])
        
        plt.xlabel("Relative frequency (MHz) to WM lock setpoint %.10g"%f_offset)
    
    else:
        plt.plot(X, Ratio, label='On/Off', color=colors[0])
        plt.plot(MoveAvg_f, MoveAvg_R, label="With moving average of %g"%MA, color=colors[1])
        
        if showSET:
            plt.axvline(x=SET, color='r', linestyle='--', linewidth=1, label='SET')
        
        plt.xlabel("TCL setpoint (V)")
    
    plt.ylabel("Slowing On/Off Ratio")
    plt.title(title)
    plt.legend()
    plt.show()       

#print("Gaussian fit params [mean, std, amp, shift]: ", fitR)
#print("Errors of fit [mean, std, amp, shift]: ", errR)
#%% Ratio -- averaged over all
'''
Point-by-point mean of the On/Off ratio over all passes (scan point k of every
pass averaged together), with the SEM over passes as the error band.
Needs every pass to have the same scan points.
'''
AvgSele = [] # file IDs to include, e.g. ["010", "011"]; leave empty for all loaded files

XAxis = "native" # "native": setpoint (V) / WM frequency, as in the cells above
                  # "frequency": MHz relative to SETfreq
                  # "velocity": molecule velocity (m/s), needs RestFreq
RestFreq = 542.809032 - 198e-6 # THz, the v = 0 (rest frame) resonance. Only needed for "velocity"
wavelength = 299792458/(SETfreq*1e12) # slowing light wavelength (m)
def ToFreqTHz(X):
    """Absolute laser frequency (THz) of scan x values"""
    if HasWM:
        return X
    elif WMLock:
        return Scan_F_offset + X*1e-3
    else:
        # No WM: SET <-> SETfreq, then the TCL calibration (MHz/V)
        return SETfreq + (X - SET)*TCLconv*1e-6

def ToVelocity(X):
    """Velocity (m/s) addressed by a counter-propagating slowing beam.
    Red detuning from RestFreq -> molecules moving towards the beam (positive v)"""
    return -(ToFreqTHz(X) - RestFreq)*1e12*wavelength

AvgLabels = [f for f in fileLabels if len(AvgSele) == 0 or f.split("_")[0] in AvgSele]
nPoints = [len(Ratios[f]) for f in AvgLabels]
if len(set(nPoints)) > 1:
    raise ValueError("Passes have different numbers of scan points: " +
                     str(dict(zip(AvgLabels, nPoints))))

R_all = np.array([Ratios[f] for f in AvgLabels])    # shape (passes, points)
X_all = np.array([X_f_THz[f] for f in AvgLabels])
nPasses = len(AvgLabels)

R_avg = np.mean(R_all, axis=0)
R_sem = np.std(R_all, axis=0, ddof=1)/np.sqrt(nPasses) if nPasses > 1 \
    else np.zeros_like(R_avg)
X_avg = np.mean(X_all, axis=0) # identical for setpoints; WM readings differ slightly

if XAxis == "velocity" and RestFreq is None:
    raise ValueError('XAxis = "velocity" needs RestFreq (THz) set')

def PlotX(X):
    if XAxis == "velocity":
        return ToVelocity(X)
    elif XAxis == "frequency":
        return (ToFreqTHz(X) - SETfreq)*1e6
    elif HasWM:
        return (X - f_offset)*1e6
    elif WMLock:
        return X*1e3
    else:
        return X

if XAxis == "velocity":
    Xlabel = "Velocity (m/s), rest frame at %.10g THz"%RestFreq
elif XAxis == "frequency":
    Xlabel = "Relative frequency (MHz) to SET %.10g THz"%SETfreq
    if not (HasWM or WMLock):
        Xlabel += "\n(from setpoint, TCL cal. %.4g MHz/V)"%TCLconv
elif HasWM:
    Xlabel = "Relative frequency (MHz) to %.10g THz"%f_offset
elif WMLock:
    Xlabel = "Relative frequency (MHz) to WM lock setpoint %.10g"%f_offset
else:
    Xlabel = "TCL setpoint (V)"

for f in AvgLabels:
    plt.plot(PlotX(X_f_THz[f]), Ratios[f], color='grey', alpha=0.3, linewidth=0.8)
plt.plot(PlotX(X_avg), R_avg, '.-', color=colors[0], label="Mean of %g passes"%nPasses)
plt.fill_between(PlotX(X_avg), R_avg-R_sem, R_avg+R_sem,
                 color=colors[0], alpha=0.3, linewidth=0, label="SEM")
if showSET:
    if XAxis == "frequency":
        plt.axvline(x=0, color='r', linestyle='--', linewidth=1, label='SET')
    elif XAxis == "velocity":
        plt.axvline(x=-(SETfreq - RestFreq)*1e12*wavelength,
                    color='r', linestyle='--', linewidth=1, label='SET')
    elif WMLock:
        plt.axvline(x=SET*1e3, color='r', linestyle='--', linewidth=1, label='SET')
    
    elif not (HasWM or WMLock):
        plt.axvline(x=SET, color='r', linestyle='--', linewidth=1, label='SET')

plt.xlabel(Xlabel)
plt.ylabel("Slowing On/Off Ratio")
plt.title("Gated TOF over " + Settings["param"] + " from " + str(SigStart) +
          "ms to " + str(SigEnd) + "ms gate,\n averaged over passes of files " +
          str(sorted(set(f.split("_")[0] for f in AvgLabels))))
plt.legend()
plt.show()

#%% Stack
'''
It makes more sense to use this with HasWM = True, because for points taken
at the same setpoint their WM readings can be slightly different
'''
ExcludeLabels = [] # bad data point in the first pass of file 002

Rstack = 0 # Just to create a variable
Xstack = 0

for i in range(0, len(fileLabels)):
    if fileLabels[i] in ExcludeLabels:
        print("Excluding " + fileLabels[i] + " from stack")
        continue
    if isinstance(Rstack, int):
        Rstack = Ratios[fileLabels[i]]
        Xstack = X_f_THz[fileLabels[i]]
    else:
        Rstack = np.concatenate((Rstack, Ratios[fileLabels[i]]))
        Xstack = np.concatenate((Xstack, X_f_THz[fileLabels[i]]))

#%% Sort
# Get the indices that would sort Combi_ScanParams
sort_indices = np.argsort(Xstack)

# Reorder both arrays using those indices
Xstack_sorted = Xstack[sort_indices]
Rstack_sorted = Rstack[sort_indices]

#%%
offset = f_offset #284.4646
MA = 1
MoveAvg_R = tools.MovingAverage(MA, Rstack_sorted)
MoveAvg_f = tools.MovingAverage(MA, Xstack_sorted)

title2="Gated TOF over " + Settings["param"] + " with " +\
    str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate, files " +\
        str(sele)

if HasWM:
    #plt.plot((Xstack_sorted-offset)*1e6, Rstack_sorted, '.', label='Stacked raw')
    plt.plot((MoveAvg_f-offset)*1e6, MoveAvg_R, label="moving average of %g"%MA)
    plt.xlabel("Relative frequency (MHz) to %.10g THz"%offset)
    
else:
    #plt.plot((Xstack_sorted-offset)*1e3, Rstack_sorted, '.', label='Stacked raw')
    plt.plot(MoveAvg_f*1e3, MoveAvg_R, label="moving average of %g"%MA)
    plt.xlabel("Relative frequency (MHz) to WM lock setpoint %.10g"%f_offset)
    
plt.ylabel("Slowing On/Off Ratio")
#plt.xlim(-600, 400)
#plt.ylim(0.4, 1.0)
plt.title(title2+"\n4f v1 R line scan in Lattice")
plt.legend()
plt.show()   

#%% Bin
from scipy.stats import binned_statistic, sem
# 2. Define your bins (boundaries along the X-axis)
fstep = 5
bin_edges = np.arange(np.min(Xstack_sorted), np.max(Xstack_sorted), step=fstep* 1e-6)

# 2. Compute binned averages, standard error of mean and number of points per bin
bin_means, _, _ = binned_statistic(
    x=Xstack_sorted, values=Rstack_sorted, statistic='mean', bins=bin_edges
)
bin_sems, _, _ = binned_statistic(
    x=Xstack_sorted, values=Rstack_sorted, statistic=sem, bins=bin_edges
)
bin_counts, _, _ = binned_statistic(
    x=Xstack_sorted, values=Rstack_sorted, statistic='count', bins=bin_edges
)

# 3. Convert bin edges to bin centers
bin_centers = (bin_edges[:-1] + bin_edges[1:]) / 2


# 4. Filter out NaNs (empty bins) so they don't break the plot
valid_mask = ~np.isnan(bin_means)
BinnedX = bin_centers[valid_mask]
BinnedY = bin_means[valid_mask]
BinnedYerr = bin_sems[valid_mask]       # SEM of each bin height
BinnedN = bin_counts[valid_mask].astype(int)

# Bins with 1 point (or zero spread) have no usable SEM - give them the mean SEM of the other bins
no_sem = ~(np.isfinite(BinnedYerr) & (BinnedYerr > 0))
if np.any(no_sem) and np.any(~no_sem):
    BinnedYerr[no_sem] = np.mean(BinnedYerr[~no_sem])
    print("%d of %d bins have < 2 points (or zero spread) - SEM set to mean of the others = %.3g"
          %(np.sum(no_sem), len(BinnedY), BinnedYerr[no_sem][0]))

# Bins with a finite, non-zero error - use these for weighted fitting (sigma=BinnedYerr[sem_mask])
sem_mask = np.isfinite(BinnedYerr) & (BinnedYerr > 0)

if HasWM:
    #plt.plot((Xstack_sorted-f_offset)*1e6, Rstack_sorted, '.', label="combined raw")
    plt.plot((BinnedX-f_offset)*1e6, BinnedY, '.-', color=colors[0], label = "Freq bin: %g MHz"%fstep)
    plt.fill_between((BinnedX-f_offset)*1e6, BinnedY-BinnedYerr, BinnedY+BinnedYerr,
                     color=colors[0], alpha=0.3, linewidth=0, label="SEM")
    plt.xlabel("Relative frequency (MHz) to %g THz"%f_iniTHz)
else:
    #plt.plot((Xstack_sorted-f_offset)*1e6, Rstack_sorted, '.', label="combined raw")
    plt.plot(BinnedX*1e3, BinnedY, '.-', color=colors[0], label = "Freq bin: %g MHz"%fstep)
    plt.fill_between(BinnedX*1e3, BinnedY-BinnedYerr, BinnedY+BinnedYerr,
                     color=colors[0], alpha=0.3, linewidth=0, label="SEM")
    plt.xlabel("Relative frequency (MHz) to WM lock setpoint %.10g"%f_offset)
plt.ylabel("Ratio")
plt.title(title2)
plt.legend()
plt.show()   

#%% Fitting
# "MoveAvg": Gaussian fit to the moving-averaged ratio (unweighted)
# "Binned":  Gaussian fit to the binned ratio, weighted by the SEM of each bin (from the Bin cell)
# "OnOff":   skewed Gaussian fits to On/Off vs setpoint (original behaviour)
FitMode = "Binned"

if FitMode in ("MoveAvg", "Binned"):
    # Same x-axes as the moving-average / binned plots above
    if FitMode == "MoveAvg":
        FitXraw, FitY, FitYerr = MoveAvg_f, MoveAvg_R, None
        if HasWM:
            FitX = (FitXraw-offset)*1e6
            FitXlabel = "Relative frequency (MHz) to %.10g THz"%offset
        else:
            FitX = FitXraw*1e3
            FitXlabel = "Relative frequency (MHz) to WM lock setpoint %.10g"%f_offset
    else:
        FitXraw, FitY, FitYerr = BinnedX[sem_mask], BinnedY[sem_mask], BinnedYerr[sem_mask]
        if HasWM:
            FitX = (FitXraw-f_offset)*1e6
            FitXlabel = "Relative frequency (MHz) to %g THz"%f_iniTHz
        else:
            FitX = FitXraw*1e3
            FitXlabel = "Relative frequency (MHz) to WM lock setpoint %.10g"%f_offset

    # Initial guess: peak or dip, whichever deviates most from the baseline
    baseline = np.median(FitY)
    iPeak = np.argmax(np.abs(FitY - baseline))
    p0 = [FitX[iPeak], (np.max(FitX)-np.min(FitX))/10, FitY[iPeak]-baseline, baseline]
    fitR, covR = curve_fit(tools.Gaussian, FitX, FitY, p0=p0,
                           sigma=FitYerr, absolute_sigma=FitYerr is not None)
    errR = np.sqrt(np.diag(covR))
    FWHMfac = 2*np.sqrt(2*np.log(2))
    print("Centre = %.6g +- %.3g MHz"%(fitR[0], errR[0]) +\
          "\n with FWHM = %.4g +- %.3g MHz"%(abs(fitR[1])*FWHMfac, errR[1]*FWHMfac) +\
          "\n amplitude = %.4g +- %.3g, offset = %.4g +- %.3g"%(fitR[2], errR[2], fitR[3], errR[3]))
    if FitYerr is not None:
        chi2_red = np.sum(((FitY - tools.Gaussian(FitX, *fitR))/FitYerr)**2) / (len(FitY) - len(fitR))
        print(" reduced chi^2 = %.3g"%chi2_red)

    xspan = np.linspace(np.min(FitX), np.max(FitX), 1000)
    if FitMode == "MoveAvg":
        plt.plot(FitX, FitY, '-.', label="moving average of %g"%MA, color=colors[0])
    else:
        plt.plot(FitX, FitY, '.-', label="Freq bin: %g MHz"%fstep, color=colors[0])
        plt.fill_between(FitX, FitY-FitYerr, FitY+FitYerr,
                         color=colors[0], alpha=0.3, linewidth=0, label="SEM")
    plt.plot(xspan, tools.Gaussian(xspan, *fitR), color=colors[1],
             label="Gaussian fit")#": FWHM = %.3g MHz"%(abs(fitR[1])*FWHMfac))
    plt.xlabel(FitXlabel)
    plt.ylabel("Slowing On/Off Ratio")
    plt.title(title2)
    plt.legend()
    plt.show()

elif FitMode == "OnOff":
    peakOn = ScanParams[np.where(OnBkgSub == np.min(OnBkgSub))[0][0]]
    fitOn, covOn = curve_fit(tools.SkewedGaussian, ScanParams, OnBkgSub, p0=[peakOn, 1., -1., 0.5, 1.])
    errOn = np.sqrt(np.diag(covOn))
    print("Central ON setpoint = %.4g +- %.3g V"%(fitOn[0], errOn[0]) +\
          "\n with FWHM = %.4g +- %.3g V"%(fitOn[1]*2*np.sqrt(2*np.log(2)),\
                                             errOn[1]*2*np.sqrt(2*np.log(2))))

    peakOff = ScanParams[np.where(OffBkgSub == np.max(OffBkgSub))[0][0]]
    fitOff, covOff = curve_fit(tools.SkewedGaussian, ScanParams, OffBkgSub, p0=[peakOff, 1., -1., 0.5, 1.])
    errOff = np.sqrt(np.diag(covOff))
    print("Central OFF setpoint = %.4g +- %.3g V"%(fitOff[0], errOff[0]) +\
          "\n with FWHM = %.4g +- %.3g V"%(fitOff[1]*2*np.sqrt(2*np.log(2)),\
                                             errOff[1]*2*np.sqrt(2*np.log(2))))

    vspan = np.arange(np.min(ScanParams), np.max(ScanParams), step = 0.01)

    plt.plot(ScanParams, OnBkgSub, '.', label='On', color=colors[0])
    plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOn), color=colors[0])
    plt.plot(ScanParams, OffBkgSub, '.', label='Off', color=colors[1])
    plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOff), color=colors[1])
    plt.xlabel('Setpoint (V)')
    plt.ylabel("Gated TOF (ms.V)")
    plt.title(title)
    plt.legend()
    plt.show()

    if HasWM:
        peakOn = f_relMHz[np.where(OnBkgSub == np.max(OnBkgSub))[0][0]]
        peakOff = f_relMHz[np.where(OffBkgSub == np.max(OffBkgSub))[0][0]]

#%% Export
import pandas as pd

ToSave = pd.DataFrame.from_dict({"Frequency (THz)":Xstack_sorted, "Ratio":Rstack_sorted})

Save_csv = tools.save_dataframe_interactive(ToSave)

#%% Selectively stack
from scipy.stats import binned_statistic, sem
# 2. Define your bins (boundaries along the X-axis)
fstep = 10
offset = 284.4646

types = {"1.7W":['005', '006'],
         "3.5W":['007'],
         "5.7W":['009']}

keys = list(types.keys())

R_stacked = {}
Freq = {}
Sterr = {}  #Standard error of mean

R_binned = {}
Freq_binned = {}


for i in range(0, len(keys)):
    keyList = types[keys[i]]
    toStackKeys = []
    toStackX = []
    toStackY = []
    
    for k in keyList: 
        for f in fileLabels: 
            if f[:3] == k:
                toStackKeys.append(f)
                toStackX.append(X_f_THz[f])
                toStackY.append(Ratios[f])
    
    toStackX_flat = np.array(tools.flattenAnyList(toStackX))
    toStackY_flat = np.array(tools.flattenAnyList(toStackY))
    
    # Get the indices that would sort Combi_ScanParams
    sort_indices = np.argsort(toStackX_flat)

    # Reorder both arrays using those indices
    X_sorted = toStackX_flat[sort_indices]
    R_sorted = toStackY_flat[sort_indices]
    
    R_stacked[keys[i]] = R_sorted
    Freq[keys[i]] = X_sorted
    
    bin_edges = np.arange(np.min(X_sorted), np.max(X_sorted), step=fstep* 1e-6)

    # 2. Compute binned averages and standard error of mean
    bin_means, _, _ = binned_statistic(
        x=X_sorted, values=R_sorted, statistic='mean', bins=bin_edges
    )
    bin_sems, _, _ = binned_statistic(x=X_sorted, values=R_sorted, 
                                      statistic=sem, bins=bin_edges)

    # 3. Convert bin edges to bin centers
    bin_centers = (bin_edges[:-1] + bin_edges[1:]) / 2


    # 4. Filter out NaNs (empty bins) so they don't break the plot
    valid_mask = ~np.isnan(bin_means)
    BinnedX = bin_centers[valid_mask]
    BinnedY = bin_means[valid_mask]
    
    R_binned[keys[i]] = BinnedY
    Freq_binned[keys[i]] = BinnedX
    Sterr[keys[i]] = bin_sems
    
    
    plt.plot((BinnedX-offset)*1e6, BinnedY, '.', color=colors[i], label=keys[i])
    plt.fill_between((BinnedX-offset)*1e6, y1=BinnedY-bin_sems, 
                     y2=BinnedY+bin_sems, alpha=0.3)

title2="Gated TOF over " + Settings["param"] + " with " +\
    str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
    str(SigStart) + "ms to " + str(SigEnd) + "ms gate, files " +\
        str(sele)

plt.xlabel("Relative frequency (MHz) to %g THz"%f_iniTHz)
plt.ylabel("On/Off ratio")
plt.title(title2+"\n4f v1 R line scan in Lattice by power")
plt.legend()
plt.show()  
    