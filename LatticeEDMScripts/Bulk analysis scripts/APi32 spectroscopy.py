# -*- coding: utf-8 -*-
"""
Created on Sun Aug  2 21:09:57 2026

For A Pi-3/2 spectroscopy

@author: sl5119
"""

#%% Import libraries
import sys
import os
import re

OneDriveFolder = os.environ['onedrive']
sys.path.append(OneDriveFolder + r"\Desktop\EDMSuite\LatticeEDMScripts")
import LatticeEDM_analysis_library as EDM
import YbF_spectroscopy_library as YbF

import numpy as np

import glob
import matplotlib.pyplot as plt
from scipy.optimize import curve_fit

import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Load data (interactive)
#% Select folder and find all NPZ files
selected_directory = tools.select_folder()

pattern="*.npz"
files = glob.glob(f'{selected_directory}/{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%%
offset = 568.681 #in THz
Data = {}

showPlot = False

for f in files:
    fileLabel = re.split(r'[\\]', f)[-1]
    with np.load(f) as data:
        # See all array keys stored inside the file
        #print("Available keys:", data.files)
        
        Data[f] = data['arr_0']
        
        if showPlot:
            plt.plot((data['arr_0'][0] - offset)*1e3, data['arr_0'][1])
            plt.xlabel("Frequency offset (GHz) w.r.t %g"%offset)
            plt.ylabel("PMT signal normalised to X-A (a.u.)")
            plt.title(fileLabel)
            plt.show()

#%% Join data by file and Normalise with X-A signal
X = []
Y = []

for i in range(0, len(files)):
    f = files[i]
    x = Data[f][0]
    y = Data[f][1]
    yXA = Data[f][2]
    X.append(list(x))
    Y.append(list(y/yXA))

X = np.array(tools.flattenAnyList(X))
Y = np.array(tools.flattenAnyList(Y))

#%% Sort & bin
# Get the indices that would sort Combi_ScanParams
sort_indices = np.argsort(X)

# Reorder both arrays using those indices
X_sorted = X[sort_indices]
Y_sorted = Y[sort_indices]
#%%
plt.figure(figsize=(30,10), dpi=300)

plt.plot((X_sorted-offset)*1e3, Y_sorted)
plt.xlabel("Frequency offset (GHz) w.r.t %g"%offset)
plt.ylabel("PMT signal normalised to X-A (a.u.)")
plt.title("Sorted & combined A Pi 3/2 spectrum, all branches, no freq bin")
plt.xlim(-75, 46)
plt.show()

#%% Bin
from scipy.stats import binned_statistic
# 2. Define your bins (boundaries along the X-axis)
fstep = 14*1e-6
bin_edges = np.arange(np.min(X_sorted), np.max(X_sorted), step=fstep)

# 2. Compute binned averages
bin_means, _, _ = binned_statistic(
    x=X_sorted, values=Y_sorted, statistic='mean', bins=bin_edges
)

# 3. Convert bin edges to bin centers
bin_centers = (bin_edges[:-1] + bin_edges[1:]) / 2


#%% 4. Filter out NaNs (empty bins) so they don't break the plot
valid_mask = ~np.isnan(bin_means)
BinnedX = bin_centers[valid_mask]
BinnedY = bin_means[valid_mask]

#%% Change aspect ratio & xlim to zoom into different branches
plt.figure(figsize=(30,10), dpi=300)

plt.plot((bin_centers-offset)*1e3, bin_means)  #With NaN there won't be lines going
                                               #through empty spaces
#plt.plot((BinnedX-offset)*1e3, BinnedY)
plt.xlabel("Frequency offset (GHz) w.r.t %g"%offset)
plt.ylabel("PMT signal normalised to X-A (a.u.)")
plt.title("Sorted & combined A Pi 3/2 spectrum, all branches, freq bin: %g MHz"%(fstep*1e6))
plt.xlim(-75, 46)
plt.show()


#%% Moving average
MA = 5
MoveAvg_Y = tools.MovingAverage(MA, Y_sorted)
MoveAvg_X = tools.MovingAverage(MA, X_sorted)

#%
plt.figure(figsize=(30,10), dpi=300)

plt.plot((MoveAvg_X-offset)*1e3, MoveAvg_Y)
plt.xlabel("Frequency offset (GHz) w.r.t %g"%offset)
plt.ylabel("PMT signal normalised to X-A (a.u.)")
plt.title("Sorted & combined A Pi 3/2 spectrum, P branch, moving average of %g"%MA)
plt.xlim(-50, -20)
plt.show()

#%% Get prediction lines
Xv1 = YbF.X_State["v1"]       # includes isotopes 174, 172, 176
APi32v0 = YbF.A_Pi_32["v0"]    # includes isotopes 174, 172, 176
#%%
Lines = {}
StartN = 0
EndN = 8
T = 4    #Temperature in K

isotopes = [174, 172, 176]

for i in isotopes:  #iterate through isotopes
    spectra = YbF.GetSpectra_N(StartN, EndN, YbF.x_APi32_branches, APi32v0[i], \
                     Xv1[i], T)
    Lines[i] = spectra
    
# =============================================================================
#     fig, ax = YbF.plot_ybf_spectrum(spectra, center_freq_thz=offset, 
#                                     xlim=None,
#                                     use_wavenumber=False,
#                                     measured_x=bin_centers,         # Optional 1D array of measured x-data
#                                     measured_y=bin_means,         # Optional 1D array of measured y-data
#                                     measured_in_thz=True,   # If True, converts measured_x from THz to GHz offset
#                                     scale_sticks_to_measured=True,  # Scales theoretical stick heights to match measured peak height
#                                     min_stick_fraction=0.1,  # Minimum stick height as a fraction of peak height (e.g., 0.02 = 2%)
#                                     stick_offset_ghz=-3.15,  # Global offset in GHz applied to theoretical sticks
#                                     figsize=(30, 10))
# =============================================================================
#%%
fig, ax = YbF.plot_ybf_spectrum_multi_isotope(
    Lines,  # Dict: {174: df_174, 172: df_172, 176: df_176}
    center_freq_thz=offset,
    xlim=(-75,46),  # e.g., (-50, 50) in GHz
    use_wavenumber=False,
    measured_x=bin_centers,    # Optional 1D array of measured x-data
    measured_y=bin_means,      # Optional 1D array of measured y-data
    measured_in_thz=True,      # If True, converts measured_x from THz to GHz offset
    scale_sticks_to_measured=True,    # Scales theoretical stick heights to match measured peak height
    min_stick_fraction=0.1,    # Minimum stick height as a fraction of peak height (e.g., 0.02 = 2%)
    stick_offset_ghz=-3.15,    # # Global offset in GHz applied to theoretical sticks
    figsize=(30, 10),
    ytitle="PMT signal normalised to X-A (a.u.)"
)