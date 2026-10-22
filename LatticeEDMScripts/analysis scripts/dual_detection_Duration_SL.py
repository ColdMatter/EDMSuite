# -*- coding: utf-8 -*-
"""
Created on Fri Mar 20 14:48:10 2026

Dual detection: compares camera images and PMT data taken during the same
V0 slowing Duration scan, to check whether the camera-based ROI signal
tracks the PMT-based population measurement over the scan.

Expected data (per run):
    - A signal camera file: a .tif stack of alternating ON/OFF slowing shots
      (25 shots x 2 = 50 frames), e.g. "006_pumpingCurvesCheck.tif".
    - A background camera file: a .tif stack with no signal, averaged and
      subtracted from the signal file, e.g. "007_pumpingCurvesCheck.tif".
    - A PMT Duration scan zip for the same run, e.g.
      "001_DurationV0scan_v0v1.zip" (loaded per pass, as in
      Duration_batch_SL.py).

Several signal/background pairs can be analysed together: pair them in
`signalToBackground` and map each signal file to its PMT zip in
`cameraToPMTID`. Results are kept in dicts keyed by file ID, and collected
per shot in the `cameraDF` and `combinedDF` DataFrames.

Known caveats -- check/update these each time before trusting the output:
    - PMT zip IDs don't reliably match camera image IDs, so they're mapped
      by hand in `cameraToPMTID` (see that cell for how the mapping was
      guessed from file timestamps for this dataset).
    - The camera OFF shots were unreliable for the Sept 2026, 24 run (fixed
      in the sequence since). Both ON and OFF shots are now background
      -subtracted with the separate background file.
    - `onFirst`, the camera ROI (`xStart`/`xEnd`/`yStart`/`yEnd`, `kRot`),
      and the PMT gate settings (`SigStart`/`SigEnd`/`BkgStart`/`BkgEnd`)
      are all specific to a given dataset and were tuned for Sept 2026, 24
      -- re-check them for other dates/setups.
    - Camera shots and PMT scan points are paired purely by matching index
      order (both step through the same duration sweep simultaneously),
      not by any shared timestamp/key.

@author: sl5119 (Simeng) with Claude Code
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

import csv

import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

import tifffile as tiff
from matplotlib.colors import TwoSlopeNorm
import matplotlib.patches as patches

from scipy.ndimage import gaussian_filter

import io
import zipfile
import xml.etree.ElementTree as ET

import xmltodict

import pandas as pd
from tqdm import tqdm

from matplotlib.widgets import EllipseSelector

# get_tiff(), get_tiff_all(), and bin_2d() now live in tools.py, shared
# across camera analysis scripts -- call as tools.get_tiff_all(...) etc.

#%% Selection
#sele = ["003", "004", "005", "006", "007", "008", "009", "010", "011", "012"]
sele = ["039", "040", "041", "042", "043", "044"]

#%% Set camera parameters
#Set Gaussian filtering parameters (not used)
#Sig1 = 5
#Sig2 = 5
#SigDiff = 5
#SigAdd = 5

#Use the following for pre 18 Mar 2026, 016 file:
#Sig1 = 10
#Sig2 = 10
#SigDiff = 10
#SigAdd = 10

Bkg_sanityCheck = False

k1 = 1
k2 = 1
kDiff = 1
kAdd = 1

#Set color bar scale
Max = 100
Min = -100

Max2 = 60
Min2 = -60

#%% Get camera images
# No keyword filter: dual-detection camera filenames vary by experiment
# (e.g. "..._pumpingCurvesCheck.tif"), so files are matched by the 3-digit
# ID prefix via `selection` instead of a fixed keyword like "TOF".
imagePaths, imageNames, folderPath = tools.get_tiff_all("", ".tif", selection=sele)

if imagePaths is None:
    raise ValueError("Aucun fichier TIFF sélectionné.")

#%% Camera file pairing: signal file ID -> background file ID
# One entry per signal/background pair -- check against the run log.
# Everything below is stored in dicts keyed by the signal (or background)
# file ID, so the file identity is kept through the analysis.
signalToBackground = {
    "039": "040",
    "041": "042",
    "043": "044",
}

signalIDs = list(signalToBackground.keys())
backgroundIDs = list(dict.fromkeys(signalToBackground.values()))  # unique, order preserved

idToPath = {name[:3]: path for name, path in zip(imageNames, imagePaths)}

missing = [ID for ID in signalIDs + backgroundIDs if ID not in idToPath]
if missing:
    raise ValueError(f"Camera files not found for IDs {missing} -- check `sele` and `signalToBackground`")

#%% Background subtraction: average of each background file, 
#   subtracted from its signal file
backgroundStacks = {}   # background ID -> raw frame stack
backgrounds = {}        # background ID -> mean background image
signalStacks = {}       # signal ID -> raw frame stack
bgSubtracted = {}       # signal ID -> background-subtracted frame stack

for bgID in backgroundIDs:
    backgroundStacks[bgID] = tiff.imread(idToPath[bgID]).astype(np.float64)
    backgrounds[bgID] = backgroundStacks[bgID].mean(axis=0)
    print(f"Background: mean of {backgroundStacks[bgID].shape[0]} frames from file {bgID}")

for sigID in signalIDs:
    bgID = signalToBackground[sigID]
    signalStacks[sigID] = tiff.imread(idToPath[sigID]).astype(np.float64)
    bgSubtracted[sigID] = signalStacks[sigID] - backgrounds[bgID]
    print(f"Subtracted background {bgID} from {bgSubtracted[sigID].shape[0]} individual images in file {sigID}")

#%% Plot background subtraction result
PlotID = signalIDs[0]   # signal file to show in the example-image plots below
Frame = 1

PlotBgID = signalToBackground[PlotID]

fig, axes = plt.subplots(1, 3, figsize=(15, 5))
im0 = axes[0].imshow(backgrounds[PlotBgID], cmap='gray', origin='lower')
axes[0].set_title(f"Background\n(mean of file {PlotBgID})")
plt.colorbar(im0, ax=axes[0])

im1 = axes[1].imshow(signalStacks[PlotID][Frame], cmap='gray', origin='lower')
axes[1].set_title(f"File {PlotID}, frame {Frame}\n(raw)")
plt.colorbar(im1, ax=axes[1])

norm3 = TwoSlopeNorm(vmin=Min, vcenter=0, vmax=Max)
im2 = axes[2].imshow(bgSubtracted[PlotID][Frame], cmap='seismic', norm=norm3, origin='lower')
axes[2].set_title(f"File {PlotID}, frame {Frame}\n(background subtracted)")
plt.colorbar(im2, ax=axes[2])
plt.show()

#meanBgSubtracted = bgSubtracted[PlotID].mean(axis=0)
#plt.imshow(meanBgSubtracted, cmap='seismic', norm=norm3, origin='lower')
#plt.colorbar()
#plt.title(f"Mean of background-subtracted image, file {PlotID}\n(background from file {PlotBgID})")
#plt.show()

#%% Split each signal file into slowing ON-OFF shot pairs, subtract OFF from ON
# 25 shots x (slowing ON, slowing OFF) = 50 images. Assumes ON is recorded
# first in each pair (index 0,2,4,... = ON, 1,3,5,... = OFF) -- flip
# onFirst below if the order turns out to be OFF-ON.
onFirst = True

onFrames = {}    # signal ID -> ON frames
offFrames = {}   # signal ID -> OFF frames
onOffDiff = {}   # signal ID -> ON-OFF difference images
nPairs = {}      # signal ID -> number of ON-OFF pairs

for sigID in signalIDs:
    stack = signalStacks[sigID]
    if onFirst:
        on, off = stack[0::2], stack[1::2]
    else:
        on, off = stack[1::2], stack[0::2]

    n = min(len(on), len(off))
    onFrames[sigID] = on[:n]
    offFrames[sigID] = off[:n]
    onOffDiff[sigID] = onFrames[sigID] - offFrames[sigID]
    nPairs[sigID] = n

    print(f"File {sigID}: {stack.shape[0]} images -> {n} ON-OFF pairs")

#%% Plot ON, OFF, and ON-OFF difference for an example shot pair
Pair = 0

fig, axes = plt.subplots(1, 3, figsize=(15, 5))
im0 = axes[0].imshow(onFrames[PlotID][Pair], cmap='gray', origin='lower')
axes[0].set_title(f"File {PlotID}, pair {Pair}\nON")
plt.colorbar(im0, ax=axes[0])

im1 = axes[1].imshow(offFrames[PlotID][Pair], cmap='gray', origin='lower')
axes[1].set_title(f"File {PlotID}, pair {Pair}\nOFF")
plt.colorbar(im1, ax=axes[1])

norm4 = TwoSlopeNorm(vmin=Min, vcenter=0, vmax=Max)
im2 = axes[2].imshow(onOffDiff[PlotID][Pair], cmap='seismic', norm=norm4, origin='lower')
axes[2].set_title(f"File {PlotID}, pair {Pair}\nON - OFF")
plt.colorbar(im2, ax=axes[2])
plt.show()

meanOnOffDiff = onOffDiff[PlotID].mean(axis=0)
plt.imshow(meanOnOffDiff, cmap='seismic', norm=norm4, origin='lower')
plt.colorbar()
plt.title(f"Mean ON-OFF difference over {nPairs[PlotID]} shot pairs, file {PlotID}")
plt.show()

#%% Background-subtract the ON and OFF shots of each signal file
onBgSubtracted = {}    # signal ID -> background-subtracted ON images
offBgSubtracted = {}   # signal ID -> background-subtracted OFF images

for sigID in signalIDs:
    bgID = signalToBackground[sigID]
    onBgSubtracted[sigID] = onFrames[sigID] - backgrounds[bgID]
    offBgSubtracted[sigID] = offFrames[sigID] - backgrounds[bgID]
    print(f"File {sigID}: subtracted background (file {bgID}) from {nPairs[sigID]} ON and {nPairs[sigID]} OFF shots")

#%% Plot background-subtracted ON and OFF shot example
Shot = 0

norm5 = TwoSlopeNorm(vmin=Min, vcenter=0, vmax=Max)
fig, axes = plt.subplots(2, 2, figsize=(10, 10))
for row, (label, raw, sub) in enumerate([("ON", onFrames, onBgSubtracted),
                                         ("OFF", offFrames, offBgSubtracted)]):
    im0 = axes[row, 0].imshow(raw[PlotID][Shot], cmap='gray', origin='lower')
    axes[row, 0].set_title(f"File {PlotID}, shot {Shot}\n{label} (raw)")
    plt.colorbar(im0, ax=axes[row, 0])

    im1 = axes[row, 1].imshow(sub[PlotID][Shot], cmap='seismic', norm=norm5, origin='lower')
    axes[row, 1].set_title(f"File {PlotID}, shot {Shot}\n{label} - background")
    plt.colorbar(im1, ax=axes[row, 1])
plt.tight_layout()
plt.show()

#meanOnBgSubtracted = onBgSubtracted[PlotID].mean(axis=0)
#plt.imshow(meanOnBgSubtracted, cmap='seismic', norm=norm5, origin='lower')
#plt.colorbar()
#plt.title(f"Mean of background-subtracted ON shots ({nPairs[PlotID]} shots), file {PlotID}\n(background from file {PlotBgID})")
#plt.show()

#%% Rotate to lab-frame orientation and define a ROI (as in the MOT-search
# analysis below, "Load all camera images" -- binning is not used here)
kRot = 1

# ROI -- copied from the MOT-search defaults as a starting point; check
# against the plot below and adjust for this data (different ROI/signal
# location than the MOT-search setup). Same ROI is used for all files.
xStart = 54
xEnd = 74
yStart = 54
yEnd = 74

onBgSubtracted_rot = {sigID: np.array([np.rot90(img, k=kRot) for img in onBgSubtracted[sigID]])
                      for sigID in signalIDs}

rect = patches.Rectangle((xStart, yStart), xEnd - xStart, yEnd - yStart,
                          linewidth=2, edgecolor='black', facecolor='none')

fig, axes = plt.subplots(1, 2, figsize=(12, 5))
norm6 = TwoSlopeNorm(vmin=Min, vcenter=0, vmax=Max)
im0 = axes[0].imshow(onBgSubtracted_rot[PlotID][Shot], cmap='seismic', norm=norm6, origin='lower')
axes[0].add_patch(rect)
axes[0].set_title(f"File {PlotID}, shot {Shot}\n(rotated, background subtracted)")
plt.colorbar(im0, ax=axes[0])

regionOfInterest_example = onBgSubtracted_rot[PlotID][Shot][yStart:yEnd, xStart:xEnd]
im1 = axes[1].imshow(regionOfInterest_example, cmap='seismic', norm=norm6, origin='lower')
axes[1].set_title(f"ROI: x[{xStart}:{xEnd}], y[{yStart}:{yEnd}]")
plt.colorbar(im1, ax=axes[1])
plt.show()

#%% Integrate signal inside ROI for each shot -> y value for PMT comparison
cameraROISignal = {}   # signal ID -> integrated ROI signal per ON shot

for sigID in signalIDs:
    cameraROISignal[sigID] = np.array([
        img[yStart:yEnd, xStart:xEnd].sum() for img in onBgSubtracted_rot[sigID]
    ])
    print(f"Integrated ROI signal for {len(cameraROISignal[sigID])} shots in file {sigID}")
    print(cameraROISignal[sigID])

#%% Integrate the background-subtracted OFF shots inside the ROI
offBgSubtracted_rot = {}
cameraROISignalOff = {}   # signal ID -> integrated ROI signal per OFF shot

for sigID in signalIDs:
    offBgSubtracted_rot[sigID] = np.array([np.rot90(img, k=kRot) for img in offBgSubtracted[sigID]])
    cameraROISignalOff[sigID] = np.array([
        img[yStart:yEnd, xStart:xEnd].sum() for img in offBgSubtracted_rot[sigID]
    ])
    print(f"Integrated ROI signal for {len(cameraROISignalOff[sigID])} OFF shots in file {sigID}")
    print(cameraROISignalOff[sigID])

#%% Camera ON/OFF ratio (same idea as the PMT Ratio = OnBkgSub/OffBkgSub)
cameraOnOffRatio = {sigID: cameraROISignal[sigID] / cameraROISignalOff[sigID]
                    for sigID in signalIDs}

# Per-shot camera results for all files in one table
cameraDF = pd.concat([
    pd.DataFrame({"signalID": sigID,
                  "backgroundID": signalToBackground[sigID],
                  "shot": np.arange(nPairs[sigID]),
                  "ROI_on": cameraROISignal[sigID],
                  "ROI_off": cameraROISignalOff[sigID],
                  "camera_ratio": cameraOnOffRatio[sigID]})
    for sigID in signalIDs], ignore_index=True)

plt.figure(figsize=(10, 6))
for i, sigID in enumerate(signalIDs):
    c = colors[i % len(colors)]
    plt.plot(cameraROISignal[sigID], '.-', color=c, label=f'{sigID} ON')
    plt.plot(cameraROISignalOff[sigID], 'x--', color=c, label=f'{sigID} OFF')
plt.xlabel('Shot index')
plt.ylabel('Camera ROI signal (counts)')
plt.title("ON vs OFF ROI signal")
plt.legend()
plt.show()

plt.figure(figsize=(10, 6))
for i, sigID in enumerate(signalIDs):
    plt.plot(cameraOnOffRatio[sigID], '.-', color=colors[i % len(colors)], label=sigID)
plt.xlabel('Shot index')
plt.ylabel('Camera ON/OFF ratio')
plt.title("Camera ON/OFF ratio")
plt.legend()
plt.show()

#% Integrate ROI signal for each frame in each background file
# Raw, not background-subtracted (these are themselves the backgrounds) --
# checks each background file's own stability/structure inside the ROI.
if Bkg_sanityCheck:
    backgroundStack_rot = {}
    backgroundROISignal = {}   # background ID -> integrated ROI signal per frame
    
    for i, bgID in enumerate(backgroundIDs):
        backgroundStack_rot[bgID] = np.array([np.rot90(img, k=kRot) for img in backgroundStacks[bgID]])
        backgroundROISignal[bgID] = np.array([
            img[yStart:yEnd, xStart:xEnd].sum() for img in backgroundStack_rot[bgID]
        ])
        print(f"Integrated ROI signal for {len(backgroundROISignal[bgID])} frames in file {bgID}")
        #print(backgroundROISignal[bgID])
    
        plt.plot(backgroundROISignal[bgID], '.-', color=colors[i % len(colors)], label=bgID)
    
    plt.xlabel('Frame index')
    plt.ylabel('Background ROI signal (counts)')
    plt.title("Background files: integrated ROI signal per frame\n(raw, not background-subtracted)")
    plt.legend()
    plt.show()

#%% Manual camera-to-PMT file mapping
# PMT zip IDs don't always match camera image IDs, so map them by hand.
# Only signal camera files need a PMT file. Check/update against the run log.
# Example from Sept 2026, 24 (guessed from file timestamps):
#   006_pumpingCurvesCheck.tif  6:16:21 PM -> 001_DurationV0scan_v0v1.zip  6:16:55 PM
cameraToPMTID = {
    "039": "039",   # TODO: fill in PMT zip IDs
    "041": "041",
    "043": "043",
}

unmapped = [sigID for sigID in signalIDs if not cameraToPMTID.get(sigID)]
if unmapped:
    print(f"No PMT file mapped for camera signal files {unmapped} -- they will be skipped in the comparison")

pmtSele = list(dict.fromkeys(ID for ID in cameraToPMTID.values() if ID))  # unique PMT IDs to load, order preserved

#%% Load PMT data
'''
               Load PMT data
'''
drive = folderPath + r"/"
print(drive)
date = re.split(r'[/]', drive)[-2]
month = re.split(r'[/]', drive)[-3]

pattern="*v3PumpBack*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%% Count passes in each PMT zip
# One scan_N.xml per pass (average.xml excluded, same filter as
# EDM.ReadAllScansInZippedXML). Only reads the zip's file list, not the data.
nPasses = {}   # PMT ID -> number of passes

for f in files:
    pmtID = os.path.basename(f).split('_')[0]
    if pmtID not in pmtSele:
        continue
    with zipfile.ZipFile(f, 'r') as z:
        nPasses[pmtID] = len([name for name in z.namelist()
                              if name.lower().endswith('.xml') and name != "average.xml"])
    print(f"PMT file {pmtID}: {nPasses[pmtID]} passes")

# Camera records continuously over all passes, so each camera signal file
# should hold nPasses x (shots per pass) ON-OFF pairs
nPassesCamera = {}   # camera signal ID -> number of passes of its PMT file

for sigID in signalIDs:
    pmtID = cameraToPMTID.get(sigID)
    if pmtID not in nPasses:
        print(f"Camera file {sigID}: PMT file '{pmtID}' not found -- pass count unknown")
        continue
    nPassesCamera[sigID] = nPasses[pmtID]
    shotsPerPass, leftover = divmod(nPairs[sigID], nPasses[pmtID])
    print(f"Camera file {sigID}: {nPairs[sigID]} ON-OFF pairs / {nPasses[pmtID]} passes "
          f"= {shotsPerPass} shots per pass" + (f" (+{leftover} left over!)" if leftover else ""))

#%%
LoadPasses = True

Data = {}
fileLabels = []
Lasers = []

if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    for i in range(0, len(files)):
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[0]
        Laser = re.split(r'[.]', re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[-2])[0]

        #Use this part if have selections
        for j in range(0, len(pmtSele)):
            if fileLabel == pmtSele[j]:
                print("File "+fileLabel+" selected")
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

#%% Extract Duration scan data (ScanParams, Ratio) per pass
# Replaces the old wavemeter-frequency extraction below, which was for the
# MOT-search frequency scans and doesn't apply to these Duration scans.
# Gate settings copied from Duration_batch_SL.py for this same dataset
# (Sept 2026, 24) -- check these against the scan settings before trusting them.
SigStart = 26.4
SigEnd = 29.3
BkgStart = 30
BkgEnd = 35

Tau = {}
Tauerr = {}
Base = {}
Baseerr = {}
IndivFit = {}   # full best-fit [A, tau, C] of each pass, used as initial guesses later
GoodData = []   # passes whose fitted tau error is smaller than tau

AllRatios = {}
AllDurations = {}

for i in range(0, len(fileLabels)):
    Scan = Data[fileLabels[i]]
    print('For file ' + fileLabels[i])
    Settings = EDM.GetScanSettings(Scan)
    fig, fit_results, ScanParams, Ratio = EDM.DurationRunSingle(Scan, fileLabels[i],
                                            SigStart, SigEnd, BkgStart, BkgEnd,
                                            plotFit=True)

    AllRatios[fileLabels[i]] = Ratio
    AllDurations[fileLabels[i]] = ScanParams

    tau = fit_results['best fit'][1]
    tauerr = fit_results['error'][1]
    base = fit_results['best fit'][2]
    baseerr = fit_results['error'][2]

    Tau[fileLabels[i]] = tau
    Tauerr[fileLabels[i]] = tauerr
    Base[fileLabels[i]] = base
    Baseerr[fileLabels[i]] = baseerr
    IndivFit[fileLabels[i]] = np.array(fit_results['best fit'])

    if tauerr < tau:
        GoodData.append(fileLabels[i])
    else:
        print("file " + fileLabels[i] + " fit is bad.")

    print('\n')

#%% Average passes within file and fit

types = {"1500 mW": ["039"],
         "7570 mW": ["041"],
         "584 mW": ["043"],
         }

Weighted = True
UseIndivP0 = True   # True: initial guess = mean of the individual-scan fits (cell above); False: fixed p0_default
p0_default = [-0.3, 500., 0.6]

keys = list(types.keys())

Avg = {}
Dur = {}
StandardErr = {}
Fit = {}
Err = {}
Cov = {}   # full fit covariance matrix, for error propagation
BadFit = {}

tspan = np.arange(0, 4000, 1)

for i in range(0, len(keys)):
    keyList = types[keys[i]]
    toAvg = []
    toAvgKeys = []
    
    for k in keyList:    
        for f in fileLabels: 
            if f[:3] == k:
                toAvgKeys.append(f)
                toAvg.append(AllRatios[f])
    
    avg = np.average(np.array(toAvg), axis=0)
    dur = AllDurations[toAvgKeys[0]]
    
    Avg[keys[i]] = avg
    Dur[keys[i]] = dur
    
    sterr = np.std(np.array(toAvg), axis=0)/np.sqrt(len(toAvgKeys))   # zero for a single pass
    StandardErr[keys[i]] = sterr

    # Initial guess from the individual fits of the scans being averaged (good fits only, if any)
    p0 = p0_default
    if UseIndivP0:
        p0Keys = [f for f in toAvgKeys if f in GoodData and f in IndivFit]
        if len(p0Keys) == 0:
            p0Keys = [f for f in toAvgKeys if f in IndivFit]
            if len(p0Keys) > 0:
                print(keys[i] + ": no good individual fits - initial guess from all scans")
        if len(p0Keys) > 0:
            p0 = np.mean([IndivFit[f] for f in p0Keys], axis=0)
        else:
            print(keys[i] + ": no individual fits found - using p0_default")
    print(keys[i] + " initial guess [A, tau, C]: ", p0)

    try:
        multi = len(toAvgKeys) > 1   # weight by SEM only when there are passes to average
        fit, cov = curve_fit(tools.exp_decay, dur, avg, p0=p0,
                             sigma=sterr if multi else None,
                             absolute_sigma=Weighted and multi)
        err = np.sqrt(np.diag(cov))
    except RuntimeError as e:   # fit did not converge
        print(keys[i] + ": fit failed - " + str(e))
        fit = np.full(3, np.nan)
        cov = np.full((3, 3), np.nan)
        err = np.full(3, np.nan)

    # Bad fit: failed, non-finite errors, or tau error >= tau (same criterion as the individual fits)
    badFit = not (np.all(np.isfinite(err)) and err[1] < abs(fit[1]))
    if badFit:
        print(keys[i] + ": bad fit, tau = %.5g +- %.4g us - fit line not plotted"%(fit[1], err[1]))

    Fit[keys[i]] = fit
    Err[keys[i]] = err
    Cov[keys[i]] = cov
    BadFit[keys[i]] = badFit

    plt.plot(dur, avg, '.', color=colors[i],
             label=keys[i] + (" (bad fit)" if badFit else ""))
    if len(toAvgKeys) > 1:
        plt.fill_between(dur, y1=avg-sterr, y2=avg+sterr, alpha=0.3,
                         color=colors[i])

    if not badFit:
        plt.plot(tspan, tools.exp_decay(tspan, *fit), color=colors[i],
                 label=r'$\tau=$%.5g+-%.4g $\mu$s'%(fit[1], err[1]))

plt.xlabel(r'V0 slowing duration ($\mu$s)')
plt.ylabel('On/Off ratio')
plt.title('Averaged pumping curves with weighted fit')
#plt.ylim(0.3, 1.1)
plt.legend(bbox_to_anchor=(1.0, 1.0))
plt.show()

#%% Extract pump-back effect
# Fit y = A exp(-t/B) + C: start = y(0), end = y(T_end), diff = end - start.
# Errors use the full fit covariance, sigma^2 = g . Cov . g with g the
# gradient w.r.t. (A, B, C) -- A and C are strongly correlated.
T_end = 4000   # us

def PumpBackPoints(fit, cov, T_end):
    """start, end, diff and their errors from an exp_decay fit [A, B, C] and its covariance"""
    A, B, C = fit
    expT = np.exp(-T_end / B)
    grads = {"start": np.array([1., 0., 1.]),
             "end": np.array([expT, A * expT * T_end / B**2, 1.])}
    grads["diff"] = grads["end"] - grads["start"]

    row = {"start": A + C, "end": A * expT + C}
    row["diff"] = row["end"] - row["start"]
    for q, g in grads.items():
        row[q + "_err"] = np.sqrt(g @ cov @ g)
        #Claude uses the full covariance matrix to calculate the error
    return row

rows = []
for k in keys:
    if BadFit[k]:
        print("With " + k + " 4fv0: bad fit - skipped")
        continue

    row = {"power": int(k[:-2]), **PumpBackPoints(Fit[k], Cov[k], T_end)}
    rows.append(row)

    print("With " + k + " 4fv0, the curve starts at %.4g +- %.2g, ends at %.4g +- %.2g at %gms,"
          %(row["start"], row["start_err"], row["end"], row["end_err"], T_end/1000))
    print("and the difference is %.4g +- %.2g"%(row["diff"], row["diff_err"]))

PumpBack = pd.DataFrame(rows)   # one row per power: start, end, diff and their errors

for q, label in [("start", "V3 start"), ("end", "V3 end (%gms)"%(T_end/1000)), ("diff", "V3 diff")]:
    plt.errorbar(PumpBack["power"], PumpBack[q], yerr=PumpBack[q + "_err"],
                 fmt='.', capsize=4, label=label)

plt.xlabel("4fv0 power (mW)")
plt.ylabel("On/Off ratio")
plt.title("V3 pump-back with different 4fv0 powers")
plt.legend()
plt.show()

#%%
'''
          Combine camera & PMT
'''
#%% Split camera shots into passes and average over passes
# The camera records continuously over all passes of a PMT scan. Each pass
# has Settings['pointsPerScan'] shots, so the long run of ON-OFF pairs is
# cut into nPasses[pmtID] chunks (pass count from the PMT zip, cell above).
# Camera pass p is matched to PMT pass label f"{pmtID}_{p}".
cameraPassDFs = []   # one row per (file, pass, scan point)
cameraAvgDFs = []    # one row per (file, scan point), averaged over passes

for sigID in signalIDs:
    pmtID = cameraToPMTID.get(sigID)
    if pmtID not in nPasses or f"{pmtID}_0" not in Data:
        print(f"Camera file {sigID}: no PMT data for '{pmtID}' -- cannot split into passes, skipped")
        continue

    nP = nPasses[pmtID]
    pointsPerScan = int(EDM.GetScanSettings(Data[f"{pmtID}_0"])['pointsPerScan'])

    # Sanity check: camera should have nPasses x pointsPerScan ON-OFF pairs
    expected = nP * pointsPerScan
    nUse = min(nP, nPairs[sigID] // pointsPerScan)   # complete passes available in the camera file
    if nPairs[sigID] != expected:
        print(f"Camera file {sigID}: WARNING {nPairs[sigID]} ON-OFF pairs, expected "
              f"{nP} passes x {pointsPerScan} points = {expected}. Using the first {nUse} passes "
              f"({nUse * pointsPerScan} pairs)")
    else:
        print(f"Camera file {sigID}: {nPairs[sigID]} ON-OFF pairs = {nP} passes x {pointsPerScan} points")

    # (passes, points) arrays
    on = cameraROISignal[sigID][:nUse * pointsPerScan].reshape(nUse, pointsPerScan)
    off = cameraROISignalOff[sigID][:nUse * pointsPerScan].reshape(nUse, pointsPerScan)
    ratio = on / off

    for p in range(nUse):
        cameraPassDFs.append(pd.DataFrame({
            "signalID": sigID, "pmtID": pmtID, "pass": p, "pmtLabel": f"{pmtID}_{p}",
            "point": np.arange(pointsPerScan),
            "ROI_on": on[p], "ROI_off": off[p], "camera_ratio": ratio[p]}))

    # Mean and standard error of mean over passes (ratio averaged per pass, as for the PMT)
    sem = lambda a: np.std(a, axis=0) / np.sqrt(nUse)
    cameraAvgDFs.append(pd.DataFrame({
        "signalID": sigID, "backgroundID": signalToBackground[sigID], "pmtID": pmtID,
        "nPasses": nUse, "point": np.arange(pointsPerScan),
        "ROI_on": on.mean(axis=0), "ROI_on_err": sem(on),
        "ROI_off": off.mean(axis=0), "ROI_off_err": sem(off),
        "camera_ratio": ratio.mean(axis=0), "camera_ratio_err": sem(ratio)}))

cameraPassDF = pd.concat(cameraPassDFs, ignore_index=True) if cameraPassDFs else pd.DataFrame()
cameraAvgDF = pd.concat(cameraAvgDFs, ignore_index=True) if cameraAvgDFs else pd.DataFrame()

#%% Combine pass-averaged camera data with the pass-averaged PMT ratio
# PMT mean and SEM over passes (Dur, Avg, StandardErr) come from the
# "Average passes within file and fit" cell, keyed by the `types` names.
# Camera and PMT points are matched by point index within a pass.
pmtToType = {ID: k for k in keys for ID in types[k]}   # PMT ID -> types key

comparedIDs = []   # signal IDs with both camera and PMT data
combinedDFs = []

for sigID, camDF in cameraAvgDF.groupby("signalID", sort=False):
    k = pmtToType.get(camDF["pmtID"].iloc[0])
    if k is None:
        print(f"Camera file {sigID}: PMT file {camDF['pmtID'].iloc[0]} not in `types` -- skipped")
        continue

    df = camDF.copy()
    df["type"] = k
    df["duration"] = Dur[k]
    df["pmt_ratio"] = Avg[k]
    df["pmt_ratio_err"] = StandardErr[k]
    combinedDFs.append(df)
    comparedIDs.append(sigID)

# One row per scan point: signalID, backgroundID, pmtID, nPasses, point,
# ROI_on(_err), ROI_off(_err), camera_ratio(_err), type, duration, pmt_ratio(_err)
combinedDF = pd.concat(combinedDFs, ignore_index=True) if combinedDFs else pd.DataFrame()

#%% PMT vs camera ON/OFF ratio with exponential fits, one figure per file
# (dots/solid = PMT data/fit, crosses/dashed = camera data/fit, shaded = SEM)
# Camera fit: weighted by the camera SEM over passes, initial guess from the
# PMT fit of the same `types` entry (p0_default if that PMT fit was bad),
# unless a manual guess is given for that camera file in CamP0Manual.
CamP0Manual = {"039": [-0.3, 800., 0.9]}   # camera signal ID -> manual initial guess [A, tau, C], e.g. {"039": [-0.3, 800., 0.9]}

CamFit = {}
CamErr = {}
CamCov = {}
CamBadFit = {}

for i, sigID in enumerate(comparedIDs):
    df = combinedDF[combinedDF["signalID"] == sigID]
    k = df["type"].iloc[0]
    c = colors[i % len(colors)]
    dur = df["duration"].to_numpy()
    y = df["camera_ratio"].to_numpy()
    yerr = df["camera_ratio_err"].to_numpy()

    if sigID in CamP0Manual:
        p0 = CamP0Manual[sigID]
        print(f"Camera file {sigID}: manual initial guess {p0}")
    else:
        p0 = p0_default if BadFit[k] else Fit[k]
    multi = df["nPasses"].iloc[0] > 1   # weight by SEM only when there are passes to average
    try:
        fit, cov = curve_fit(tools.exp_decay, dur, y, p0=p0,
                             sigma=yerr if multi else None,
                             absolute_sigma=Weighted and multi)
        err = np.sqrt(np.diag(cov))
    except RuntimeError as e:   # fit did not converge
        print(f"Camera file {sigID}: fit failed - {e}")
        fit = np.full(3, np.nan)
        cov = np.full((3, 3), np.nan)
        err = np.full(3, np.nan)

    badFit = not (np.all(np.isfinite(err)) and err[1] < abs(fit[1]))
    CamFit[sigID], CamErr[sigID], CamCov[sigID], CamBadFit[sigID] = fit, err, cov, badFit
    print(f"Camera file {sigID}: tau = {fit[1]:.5g} +- {err[1]:.4g} us" + (" (bad fit)" if badFit else "") +
          f" | PMT {k}: tau = {Fit[k][1]:.5g} +- {Err[k][1]:.4g} us" + (" (bad fit)" if BadFit[k] else ""))

    plt.plot(dur, df["pmt_ratio"], '.', color=c,
             label=f"PMT {df['pmtID'].iloc[0]}" + (" (bad fit)" if BadFit[k] else ""))
    plt.fill_between(dur, df["pmt_ratio"] - df["pmt_ratio_err"], df["pmt_ratio"] + df["pmt_ratio_err"],
                     color=c, alpha=0.3, linewidth=0)
    if not BadFit[k]:
        plt.plot(tspan, tools.exp_decay(tspan, *Fit[k]), '-', color=c,
                 label=r'PMT fit: $\tau=$%.5g+-%.4g $\mu$s'%(Fit[k][1], Err[k][1]))

    plt.plot(dur, y, 'x', color=c,
             label=f"CCD {sigID} in ROI" + (" (bad fit)" if badFit else ""))
    plt.fill_between(dur, y - yerr, y + yerr, color=c, alpha=0.15, linewidth=0)
    if not badFit:
        plt.plot(tspan, tools.exp_decay(tspan, *fit), '--', color=c,
                 label=r'CCD fit: $\tau=$%.5g+-%.4g $\mu$s'%(fit[1], err[1]))

    plt.xlabel("V0 slowing duration (μs)")
    plt.ylabel("On/Off ratio")
    plt.title(f"Camera ROI ratio vs PMT ratio, {k} (mean over passes)")
    plt.legend(bbox_to_anchor=(1.0, 1.0))
    plt.show()

#%% Extract pump-back effect from the camera fits and compare with the PMT
# Same start/end/diff extraction as "Extract pump-back effect" (PumpBackPoints),
# applied to the camera fits. Filled markers = PMT, open markers = camera.
camRows = []
for sigID in comparedIDs:
    k = combinedDF.loc[combinedDF["signalID"] == sigID, "type"].iloc[0]
    if CamBadFit[sigID]:
        print(f"Camera file {sigID} ({k}): bad fit - skipped")
        continue

    row = {"signalID": sigID, "type": k, "power": int(k[:-2]),
           **PumpBackPoints(CamFit[sigID], CamCov[sigID], T_end)}
    camRows.append(row)

    print(f"Camera file {sigID} ({k}): starts at {row['start']:.4g} +- {row['start_err']:.2g}, "
          f"ends at {row['end']:.4g} +- {row['end_err']:.2g} at {T_end/1000:g}ms,")
    print(f"and the difference is {row['diff']:.4g} +- {row['diff_err']:.2g}")

CamPumpBack = pd.DataFrame(camRows)   # one row per camera file: start, end, diff and their errors

for j, (q, label) in enumerate([("start", "V3 start"), ("end", "V3 end (%gms)"%(T_end/1000)), ("diff", "V3 diff")]):
    plt.errorbar(PumpBack["power"], PumpBack[q], yerr=PumpBack[q + "_err"],
                 fmt='o', capsize=4, color=colors[j], label="PMT " + label)
    if len(CamPumpBack):
        plt.errorbar(CamPumpBack["power"], CamPumpBack[q], yerr=CamPumpBack[q + "_err"],
                     fmt='s', mfc='none', capsize=4, color=colors[j], label="CCD " + label)

plt.xlabel("4fv0 power (mW)")
plt.ylabel("On/Off ratio")
plt.title("V3 pump-back with different 4fv0 powers, PMT vs camera")
plt.legend(bbox_to_anchor=(1.0, 1.0))
plt.show()

#%% Compare the averaged background images with each other
# For each pair of background files: difference of the mean images (top row)
# and the same difference divided by its expected noise, z = diff / sqrt(SEM1^2 + SEM2^2)
# (bottom row). If the backgrounds only differ by noise, z is ~N(0, 1): RMS(z) ~ 1
# and the ROI-integrated difference is consistent with zero.
# Images rotated by kRot to match the ROI plots; black box = ROI.
from itertools import combinations

bgPairs = list(combinations(backgroundIDs, 2))

if len(bgPairs) == 0:
    print("Only one background file -- nothing to compare")
else:
    # SEM image of each mean background (per-pixel std over frames / sqrt(N))
    bgSEM = {bgID: backgroundStacks[bgID].std(axis=0) / np.sqrt(backgroundStacks[bgID].shape[0])
             for bgID in backgroundIDs}

    diffs = {}
    zMaps = {}
    for b1, b2 in bgPairs:
        diffs[(b1, b2)] = np.rot90(backgrounds[b1] - backgrounds[b2], k=kRot)
        zMaps[(b1, b2)] = diffs[(b1, b2)] / np.rot90(np.sqrt(bgSEM[b1]**2 + bgSEM[b2]**2), k=kRot)

        d, z = diffs[(b1, b2)], zMaps[(b1, b2)]
        roiDiff = backgroundROISignal[b1].mean() - backgroundROISignal[b2].mean()
        roiDiffErr = np.sqrt(sum(backgroundROISignal[b].var() / len(backgroundROISignal[b]) for b in (b1, b2)))
        print(f"{b1} - {b2}: mean diff = {d.mean():.3g} counts/pixel, RMS diff = {np.sqrt((d**2).mean()):.3g}, "
              f"RMS(z) = {np.sqrt((z**2).mean()):.3g} (~1 if noise only)")
        print(f"    ROI-integrated diff = {roiDiff:.4g} +- {roiDiffErr:.3g} counts ({roiDiff/roiDiffErr:.2g} sigma)")

    # Shared, symmetric colour scales across pairs
    dMax = max(np.percentile(np.abs(d), 99.5) for d in diffs.values())
    zMax = 5

    fig, axes = plt.subplots(2, len(bgPairs), figsize=(5 * len(bgPairs), 9), squeeze=False)
    for j, pair in enumerate(bgPairs):
        im0 = axes[0, j].imshow(diffs[pair], cmap='seismic', origin='lower',
                                norm=TwoSlopeNorm(vmin=-dMax, vcenter=0, vmax=dMax))
        axes[0, j].set_title(f"Background {pair[0]} - {pair[1]}\n(counts)")
        plt.colorbar(im0, ax=axes[0, j])

        im1 = axes[1, j].imshow(zMaps[pair], cmap='seismic', origin='lower',
                                norm=TwoSlopeNorm(vmin=-zMax, vcenter=0, vmax=zMax))
        axes[1, j].set_title(f"Background {pair[0]} - {pair[1]}\ndifference / expected noise (z)")
        plt.colorbar(im1, ax=axes[1, j])

        for ax in axes[:, j]:
            ax.add_patch(patches.Rectangle((xStart, yStart), xEnd - xStart, yEnd - yStart,
                                           linewidth=1.5, edgecolor='black', facecolor='none'))
    plt.tight_layout()
    plt.show()
