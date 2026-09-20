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
from peak_tools import launch_peak_assigner

import numpy as np

import glob
import matplotlib.pyplot as plt
from scipy.optimize import curve_fit

import tools as tools

import pandas as pd

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
    spectra = YbF.GetSpectra_N_new(StartN, EndN, YbF.x_APi32_branches_new, APi32v0[i], \
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
#%% Plot line predictions over data
fig, ax = YbF.plot_ybf_spectrum_multi_isotope(
    Lines,  # Dict: {174: df_174, 172: df_172, 176: df_176}
    center_freq_thz=offset,
    xlim=(-5, 5),  # e.g., (-50, 50) in GHz
    use_wavenumber=False,
    measured_x=bin_centers,    # Optional 1D array of measured x-data
    measured_y=bin_means,      # Optional 1D array of measured y-data
    measured_in_thz=True,      # If True, converts measured_x from THz to GHz offset
    scale_sticks_to_measured=True,    # Scales theoretical stick heights to match measured peak height
    min_stick_fraction=0.1,    # Minimum stick height as a fraction of peak height (e.g., 0.02 = 2%)
    stick_offset_ghz={174: -3.15, 172: -2.6, 176: -2.2},    # Global offset in GHz applied to theoretical 
                                                            # sticks. Accepts float/int OR Dict: {174: -3.15, 172: -1.8, 176: -2.0}
    figsize=(20, 10),
    ytitle="PMT signal normalised to X-A (a.u.)"
)

#%% Assign lines (skip if using interactive tool)
Lines_assigned = YbF.assign_spectral_lines(
    dfs_by_isotope=Lines,
    measured_x=bin_centers,
    measured_y=bin_means,
    center_freq_thz=offset,
    stick_offset_ghz={174: -3.15, 172: -2.6, 176: -2.2},
    tolerance_ghz=0.1,
    peak_prominence=0.05,  # Adjust higher if picking up baseline noise peaks
)

# Preview the top assignments
print(Lines_assigned.head(15))

#%%
# Run the residual plot
Residual, Raxes = YbF.plot_residuals_vs_n(Lines_assigned,
                                   tolerance_ghz=0.1,
    branch_colors={
        "O": "#1f77b4",
        "P": "#ff7f0e",
        "Q": "#2ca02c",
        "R": "#d62728",
    },
    iso_markers={"172YbF": "o", "174YbF": "s", "176YbF": "^"},
    hf_labels={
        1: "Hyperfine State: F=N+1",
        2: "Hyperfine State: F=N-",
        3: "Hyperfine State: F=N+",
        4: "Hyperfine State: F=N-1",
    }
    )

#%%
fig_assigned, ax_assigned = YbF.plot_spectrum_with_assignments(
    df_assignments=Lines_assigned,
    dfs_by_isotope=Lines,
    measured_x=bin_centers,
    measured_y=bin_means,
    center_freq_thz=offset,
    xlim=(-5, 50),
    figsize=(110, 10),
    label_fontsize=8,
    show_prediction_sticks=True,
    scale_sticks_to_measured=True,
    min_stick_fraction=0.2,
    stick_offset_ghz={174: -3.15, 172: -2.6, 176: -2.2},
    use_wavenumber=False,
)

#%% Export to PDF (interactive)
YbF.save_fig_to_pdf_slices(
    fig=fig_assigned,
    ax=ax_assigned,
    total_range_ghz=(-5, 50),
    window_size_ghz=10.0,
    overlap_ghz=2.0
)

#%% Generate list of peaks
#peak_ind is the index of frequency axis that gives the peak_pos
peak_indices, peak_positions = YbF.detect_spectral_peaks(
    bin_centers, 
    bin_means, 
    height=0.08, 
    prominence=0.05, 
    distance=1, 
    smooth=False,
    window_length=15,
    polyorder=3
)


#%% Launch the interactive GUI
#Or load the line assignments inside the GUI
Lines_assigned_fixed = launch_peak_assigner(
    frequencies=bin_centers,          # Measured X (THz)
    intensities=bin_means,        # Measured Y
    peak_indices=peak_indices,
    peak_positions=peak_positions,
    center_freq_thz=offset,            # Center frequency in THz
    dfs_by_isotope=Lines,      # Overlays theoretical stick predictions
    stick_offset_ghz={174: -3.15, 172: -2.6, 176: -2.2}, # Global prediction offset (GHz)
    zoom_window_ghz=10.0
)

#%% Re-fit constants

# Fit Gaussians to peaks
# Need to use NaN free arrays for data
# 1. Fit with automatically determined global baseline (5th percentile)
df_fitted, full_fit_y, popt_dict, bg_val = YbF.fit_all_assigned_lines_enhanced(
    x_data=(BinnedX - offset) * 1e3,
    y_data=BinnedY,
    lines_df=Lines_assigned_fixed,
    freq_col="Frequency_Offset_GHz",
    global_baseline=None,  # Or set manually e.g. global_baseline=0.08
    min_fwhm_ghz=0.01,
    max_fwhm_ghz=0.10,
)

#%% 2. Plot results smoothly without baseline steps
YbF.plot_fitted_spectrum(
    x_data=(BinnedX - offset) * 1e3,
    y_data=BinnedY,
    full_fit_y=full_fit_y,
    df_fitted=df_fitted,
    global_baseline=bg_val,
    zoom_range=(0.0, 50.0),
    figsize=(50, 5),
    show_individual_gaussians=True,
    annotate_labels=True,
)

#%% 3. Save if all correct
save_peak_fit = YbF.save_dataframe_interactive(df_fitted)

#%% 4. Plot a Fortrait diagram by branch and F
# Plot for the main isotope (174YbF)
YbF.plot_fortrat_diagrams(df_fitted, isotope="174YbF")

#%% Or plot without isotope filtering
YbF.plot_fortrat_diagrams(df_fitted, isotope=None)

#%% Re-fit constants
df_fitted_by_isotopes = YbF.prepare_fitting_data(df_fitted, 
                                                 center_freq_thz=offset)
#%%
# Refit each isotope dataset separately
fit_174_df, res_174 = YbF.run_isotope_refit(
    df_fitted_by_isotopes["174YbF"], iso_num=174, vib_state="v1",
    vib_state_upper="v0", center_freq_thz=offset,
    fixed_params={"ad": YbF.A_Pi_32["v0"][174]["ad"],
                  "d": YbF.A_Pi_32["v0"][174]["d"]}
)
fit_172_df, res_172 = YbF.run_isotope_refit(
    df_fitted_by_isotopes["172YbF"], iso_num=172, vib_state="v1",
    vib_state_upper="v0", center_freq_thz=offset,
    fixed_params={"ad": YbF.A_Pi_32["v0"][172]["ad"],
                  "d": YbF.A_Pi_32["v0"][172]["d"]}

)
fit_176_df, res_176 = YbF.run_isotope_refit(
    df_fitted_by_isotopes["176YbF"], iso_num=176, vib_state="v1",
    vib_state_upper="v0", center_freq_thz=offset,
    fixed_params={"ad": YbF.A_Pi_32["v0"][176]["ad"],
                  "d": YbF.A_Pi_32["v0"][176]["d"]}
)

#%% Plot residuals again
# 1. Format the fitting dictionaries to form the new dictionary of constants
APi32v0_new = {
    172: YbF.clean_fit_params(fit_172_df),
    174: YbF.clean_fit_params(fit_174_df),
    176: YbF.clean_fit_params(fit_176_df)
    }

#%%
# 2. Generate new line predictions
Lines_new = {}
for i in isotopes:  #iterate through isotopes
    spectra = YbF.GetSpectra_N_new(StartN, EndN, YbF.x_APi32_branches_new, APi32v0_new[i], \
                     Xv1[i], T)
    Lines_new[i] = spectra

#%%
# 3. Calculate residuals against identified peaks
# 3.1 Update residuals on your assigned lines using the new constants dict
Lines_assigned_updated = YbF.update_assigned_residuals(
    df_assigned=Lines_assigned_fixed,
    APi32_new_dict=APi32v0_new,
    vib_state="v1",
    center_freq_thz=offset,
)
#%%
# 2. Run your original residual plotting function directly!
Residual_new_by_isotopes = YbF.plot_residuals_by_isotope(
    Lines_assigned_updated,
    tolerance_ghz=0.01,  # Sets red dotted boundaries (e.g., 10 MHz)
    branch_colors={
        "O": "#1f77b4",
        "P": "#ff7f0e",
        "Q": "#2ca02c",
        "R": "#d62728",
    }
)

#%% Remove certain point if sth is wrong
# 1. Locate the exact misassigned line entry
outlier = Lines_assigned_updated[
    (Lines_assigned_updated["Isotope"].astype(str).str.contains("176"))
    & (Lines_assigned_updated["Branch"] == "Q")
    & (Lines_assigned_updated["N"] == 0)
    & (Lines_assigned_updated["Hyperfine_Comp"] == "F=N-")
]
print("Outlier details:")
print(outlier[["Isotope", "Branch", "N", "Residual_GHz"]])

# 2. Filter out extreme outliers (> 0.15 GHz) before final plotting/fitting
Lines_cleaned = Lines_assigned_updated[
    Lines_assigned_updated["Residual_GHz"].abs() < 0.15
].copy()

Residual_new_by_isotopes = YbF.plot_residuals_by_isotope(
    Lines_cleaned,
    tolerance_ghz=0.01,  # Sets red dotted boundaries (e.g., 10 MHz)
    branch_colors={
        "O": "#1f77b4",
        "P": "#ff7f0e",
        "Q": "#2ca02c",
        "R": "#d62728",
    }
)

#%% New prediction over spectrum
fig, ax = YbF.plot_ybf_spectrum_multi_isotope(
    Lines_new,  # Dict: {174: df_174, 172: df_172, 176: df_176}
    center_freq_thz=offset,
    xlim=(-75, -65),  # e.g., (-50, 50) in GHz
    use_wavenumber=False,
    measured_x=bin_centers,    # Optional 1D array of measured x-data
    measured_y=bin_means,      # Optional 1D array of measured y-data
    measured_in_thz=True,      # If True, converts measured_x from THz to GHz offset
    scale_sticks_to_measured=True,    # Scales theoretical stick heights to match measured peak height
    min_stick_fraction=0.1,    # Minimum stick height as a fraction of peak height (e.g., 0.02 = 2%)
    stick_offset_ghz={174: 0, 172: 0, 176: 0},    # Global offset in GHz applied to theoretical 
                                                            # sticks. Accepts float/int OR Dict: {174: -3.15, 172: -1.8, 176: -2.0}
    figsize=(20, 10),
    ytitle="PMT signal normalised to X-A (a.u.)"
)