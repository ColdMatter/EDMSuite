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

Known caveats -- check/update these each time before trusting the output:
    - PMT zip IDs don't reliably match camera image IDs, so they're mapped
      by hand in `cameraToPMTID` (see that cell for how the mapping was
      guessed from file timestamps for this dataset).
    - The camera OFF shots were found to be unreliable for this run (the
      camera ON/OFF ratio doesn't track the PMT ratio -- see the sanity
      check cell), so the main analysis uses ON shots only, background
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

OneDriveFolder = os.environ['onedrive']
sys.path.append(OneDriveFolder + r"\Desktop\EDMSuite\LatticeEDMScripts")
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
sele = ["006", "007"]

#%% Set camera parameters
#Set Gaussian filtering parameters
Sig1 = 5
Sig2 = 5
SigDiff = 5
SigAdd = 5

#Use the following for pre 18 Mar 2026, 016 file:
#Sig1 = 10
#Sig2 = 10
#SigDiff = 10
#SigAdd = 10


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

#%% Background subtraction: average of file 007 as background for file 006
backgroundID = "007"
signalID = "006"

idToPath = {name[:3]: path for name, path in zip(imageNames, imagePaths)}

backgroundStack = tiff.imread(idToPath[backgroundID]).astype(np.float64)
signalStack = tiff.imread(idToPath[signalID]).astype(np.float64)

background = backgroundStack.mean(axis=0)
bgSubtracted = signalStack - background  # one background-subtracted image per frame in file 006

print(f"Background: mean of {backgroundStack.shape[0]} frames from file {backgroundID}")
print(f"Subtracted from {bgSubtracted.shape[0]} individual images in file {signalID}")

#%% Plot background subtraction result
Max3 = 100
Min3 = -100

Frame = 1

fig, axes = plt.subplots(1, 3, figsize=(15, 5))
im0 = axes[0].imshow(background, cmap='gray', origin='lower')
axes[0].set_title(f"Background\n(mean of file {backgroundID})")
plt.colorbar(im0, ax=axes[0])

im1 = axes[1].imshow(signalStack[Frame], cmap='gray', origin='lower')
axes[1].set_title(f"File {signalID}, frame {Frame}\n(raw)")
plt.colorbar(im1, ax=axes[1])

norm3 = TwoSlopeNorm(vmin=Min3, vcenter=0, vmax=Max3)
im2 = axes[2].imshow(bgSubtracted[Frame], cmap='seismic', norm=norm3, origin='lower')
axes[2].set_title(f"File {signalID}, frame {Frame}\n(background subtracted)")
plt.colorbar(im2, ax=axes[2])
plt.show()

#meanBgSubtracted = bgSubtracted.mean(axis=0)
#plt.imshow(meanBgSubtracted, cmap='seismic', norm=norm3, origin='lower')
#plt.colorbar()
#plt.title(f"Mean of background-subtracted image, file {signalID}\n(background from file {backgroundID})")
#plt.show()

#%% Split camera file 006 into slowing ON-OFF shot pairs, subtract OFF from ON
# 25 shots x (slowing ON, slowing OFF) = 50 images. Assumes ON is recorded
# first in each pair (index 0,2,4,... = ON, 1,3,5,... = OFF) -- flip
# onFirst below if the order turns out to be OFF-ON.
onFirst = True

if onFirst:
    onFrames = signalStack[0::2]
    offFrames = signalStack[1::2]
else:
    onFrames = signalStack[1::2]
    offFrames = signalStack[0::2]

nPairs = min(len(onFrames), len(offFrames))
onFrames = onFrames[:nPairs]
offFrames = offFrames[:nPairs]

onOffDiff = onFrames - offFrames  # one ON-OFF difference image per shot pair

print(f"File {signalID}: {signalStack.shape[0]} images -> {nPairs} ON-OFF pairs")

#%% Plot ON, OFF, and ON-OFF difference for an example shot pair
Pair = 0

fig, axes = plt.subplots(1, 3, figsize=(15, 5))
im0 = axes[0].imshow(onFrames[Pair], cmap='gray', origin='lower')
axes[0].set_title(f"File {signalID}, pair {Pair}\nON")
plt.colorbar(im0, ax=axes[0])

im1 = axes[1].imshow(offFrames[Pair], cmap='gray', origin='lower')
axes[1].set_title(f"File {signalID}, pair {Pair}\nOFF")
plt.colorbar(im1, ax=axes[1])

norm4 = TwoSlopeNorm(vmin=Min3, vcenter=0, vmax=Max3)
im2 = axes[2].imshow(onOffDiff[Pair], cmap='seismic', norm=norm4, origin='lower')
axes[2].set_title(f"File {signalID}, pair {Pair}\nON - OFF")
plt.colorbar(im2, ax=axes[2])
plt.show()

meanOnOffDiff = onOffDiff.mean(axis=0)
plt.imshow(meanOnOffDiff, cmap='seismic', norm=norm4, origin='lower')
plt.colorbar()
plt.title(f"Mean ON-OFF difference over {nPairs} shot pairs, file {signalID}")
plt.show()

#%% Background-subtract only the ON shots from file 006 (OFF shots invalid this run)
onBgSubtracted = onFrames - background  # one background-subtracted ON image per shot

print(f"File {signalID}: subtracted background (file {backgroundID}) from {nPairs} ON shots")

#%% Plot background-subtracted ON shot example and mean
Shot = 0

fig, axes = plt.subplots(1, 2, figsize=(10, 5))
im0 = axes[0].imshow(onFrames[Shot], cmap='gray', origin='lower')
axes[0].set_title(f"File {signalID}, shot {Shot}\nON (raw)")
plt.colorbar(im0, ax=axes[0])

norm5 = TwoSlopeNorm(vmin=Min3, vcenter=0, vmax=Max3)
im1 = axes[1].imshow(onBgSubtracted[Shot], cmap='seismic', norm=norm5, origin='lower')
axes[1].set_title(f"File {signalID}, shot {Shot}\nON - background")
plt.colorbar(im1, ax=axes[1])
plt.show()

#meanOnBgSubtracted = onBgSubtracted.mean(axis=0)
#plt.imshow(meanOnBgSubtracted, cmap='seismic', norm=norm5, origin='lower')
#plt.colorbar()
#plt.title(f"Mean of background-subtracted ON shots ({nPairs} shots), file {signalID}\n(background from file {backgroundID})")
#plt.show()

#%% Rotate to lab-frame orientation and define a ROI (as in the MOT-search
# analysis below, "Load all camera images" -- binning is not used here)
kRot = 1

# ROI -- copied from the MOT-search defaults as a starting point; check
# against the plot below and adjust for this data (different ROI/signal
# location than the MOT-search setup)
xStart = 54
xEnd = 74
yStart = 54
yEnd = 74

onBgSubtracted_rot = np.array([np.rot90(img, k=kRot) for img in onBgSubtracted])

rect = patches.Rectangle((xStart, yStart), xEnd - xStart, yEnd - yStart,
                          linewidth=2, edgecolor='black', facecolor='none')

fig, axes = plt.subplots(1, 2, figsize=(12, 5))
norm6 = TwoSlopeNorm(vmin=Min3, vcenter=0, vmax=Max3)
im0 = axes[0].imshow(onBgSubtracted_rot[Shot], cmap='seismic', norm=norm6, origin='lower')
axes[0].add_patch(rect)
axes[0].set_title(f"File {signalID}, shot {Shot}\n(rotated, background subtracted)")
plt.colorbar(im0, ax=axes[0])

regionOfInterest_example = onBgSubtracted_rot[Shot][yStart:yEnd, xStart:xEnd]
im1 = axes[1].imshow(regionOfInterest_example, cmap='seismic', norm=norm6, origin='lower')
axes[1].set_title(f"ROI: x[{xStart}:{xEnd}], y[{yStart}:{yEnd}]")
plt.colorbar(im1, ax=axes[1])
plt.show()

#%% Integrate signal inside ROI for each shot -> y value for PMT comparison
cameraROISignal = np.array([
    img[yStart:yEnd, xStart:xEnd].sum() for img in onBgSubtracted_rot
])

print(f"Integrated ROI signal for {len(cameraROISignal)} shots in file {signalID}")
print(cameraROISignal)

#%% Background-subtract and integrate the OFF shots too (checking whether
# they're still usable despite the acquisition issue noted earlier)
offBgSubtracted = offFrames - background
offBgSubtracted_rot = np.array([np.rot90(img, k=kRot) for img in offBgSubtracted])

cameraROISignalOff = np.array([
    img[yStart:yEnd, xStart:xEnd].sum() for img in offBgSubtracted_rot
])

print(f"Integrated ROI signal for {len(cameraROISignalOff)} OFF shots in file {signalID}")
print(cameraROISignalOff)

#%% Camera ON/OFF ratio (same idea as the PMT Ratio = OnBkgSub/OffBkgSub)
cameraOnOffRatio = cameraROISignal / cameraROISignalOff

fig, axes = plt.subplots(1, 2, figsize=(12, 5))
axes[0].plot(cameraROISignal, '.-', label='ON')
axes[0].plot(cameraROISignalOff, '.-', label='OFF')
axes[0].set_xlabel('Shot index')
axes[0].set_ylabel('Camera ROI signal (counts)')
axes[0].legend()
axes[0].set_title(f"File {signalID}: ON vs OFF ROI signal")

axes[1].plot(cameraOnOffRatio, '.-', color=colors[2])
axes[1].set_xlabel('Shot index')
axes[1].set_ylabel('Camera ON/OFF ratio')
axes[1].set_title("Camera ON/OFF ratio\n(sanity check: are OFF shots still usable?)")
plt.tight_layout()
plt.show()

#%% Integrate ROI signal for each frame in the background file 007
# Raw, not background-subtracted (007 is itself the background) -- checks
# the background file's own stability/structure inside the ROI.
backgroundStack_rot = np.array([np.rot90(img, k=kRot) for img in backgroundStack])

backgroundROISignal = np.array([
    img[yStart:yEnd, xStart:xEnd].sum() for img in backgroundStack_rot
])

print(f"Integrated ROI signal for {len(backgroundROISignal)} frames in file {backgroundID}")
print(backgroundROISignal)

plt.plot(backgroundROISignal, '.-', color=colors[3])
plt.xlabel('Frame index')
plt.ylabel('Background ROI signal (counts)')
plt.title(f"File {backgroundID}: integrated ROI signal per frame\n(raw, not background-subtracted)")
plt.show()

#%% Manual camera-to-PMT file mapping
# PMT zip IDs don't always match camera image IDs, so map them by hand for
# now. Guessed here from file timestamps (camera file just before its
# corresponding PMT zip) -- check/update against the run log:
#   006_pumpingCurvesCheck.tif  6:16:21 PM -> 001_DurationV0scan_v0v1.zip  6:16:55 PM
#   007_pumpingCurvesCheck.tif  6:23:12 PM -> 002_DurationV0scan_v0v1.zip  6:23:32 PM
cameraToPMTID = {
    "006": "001",
    "007": "002",
}

pmtSele = list(dict.fromkeys(cameraToPMTID.values()))  # unique PMT IDs to load, order preserved

#%% Load PMT data
'''
               Load PMT data
'''
drive = folderPath + r"/"
print(drive)
date = re.split(r'[/]', drive)[-2]
month = re.split(r'[/]', drive)[-3]

pattern="*Duration*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

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
SigStart = 22
SigEnd = 24
BkgStart = 70
BkgEnd = 78

Tau = {}
Tauerr = {}
Base = {}
Baseerr = {}

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

    print('\n')

#%% Combine camera ROI signal and PMT ratio, sharing an x-axis
# x-axis (V0 slowing duration) comes from the PMT scan; cameraROISignal and
# pmtRatio are assumed to be in matching shot order (same 25-point duration
# scan run simultaneously) -- not matched by any other key.
pmtID = cameraToPMTID[signalID]
pmtPassLabels = [label for label in fileLabels if label.split('_')[0] == pmtID]

if len(pmtPassLabels) > 1:
    print(f"Multiple passes found for PMT file {pmtID}: {pmtPassLabels}. Using {pmtPassLabels[0]}.")

pmtLabel = pmtPassLabels[0]
pmtDuration = AllDurations[pmtLabel]
pmtRatio = AllRatios[pmtLabel]

print(f"Camera shots: {len(cameraROISignal)}, PMT scan points: {len(pmtDuration)}")

#%% Plot camera ROI signal and PMT ratio vs V0 slowing duration
fig, ax1 = plt.subplots(figsize=(10, 5))

color1 = colors[0]
ax1.plot(pmtDuration, cameraROISignal, '.-', color=color1)
ax1.set_xlabel("V0 slowing duration (μs)")
ax1.set_ylabel("Camera ROI signal (counts)", color=color1)
ax1.tick_params(axis='y', labelcolor=color1)

ax2 = ax1.twinx()
color2 = colors[1]
ax2.plot(pmtDuration, pmtRatio, '.-', color=color2)
ax2.set_ylabel("PMT ratio (On/Off)", color=color2)
ax2.tick_params(axis='y', labelcolor=color2)

fig.suptitle(f"Camera file {signalID} ROI signal vs PMT file {pmtID} ratio")
fig.tight_layout()
plt.show()

#%%
plt.plot(pmtDuration, pmtRatio, '.-', color=color1, label = "PMT")
plt.plot(pmtDuration, cameraOnOffRatio, '.-', color=color2, label = "CCD in ROI")
plt.xlabel("V0 slowing duration (μs)")
plt.ylabel("On/Off ratio")
plt.title(f"Camera file {signalID} ROI signal vs PMT file {pmtID} ratio")
plt.legend()
plt.show()