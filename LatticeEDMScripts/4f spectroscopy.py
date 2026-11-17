# -*- coding: utf-8 -*-
"""
Created on Mon Aug 24 10:58:15 2026

For some 4f spectroscopy analysis

@author: sl5119
"""

#%% Packages & set directory
import sys
import os
import re

# On macOS, pythonnet needs pointing at Mono before anything imports clr
if sys.platform == "darwin":
    from pythonnet import load
    load("mono", libmono="/Library/Frameworks/Mono.framework/Versions/Current/lib/libmonosgen-2.0.dylib")

# Add LatticeEDMScripts (the folder holding YbF_spectroscopy_library.py) to the
# path so the library imports work whatever the working directory, OS, or
# which subfolder this script lives in
try:
    scriptDir = os.path.dirname(os.path.abspath(__file__))
except NameError:  # __file__ isn't defined when running cells interactively
    scriptDir = os.getcwd()
libDir = scriptDir
while not os.path.isfile(os.path.join(libDir, "YbF_spectroscopy_library.py")):
    parent = os.path.dirname(libDir)
    if parent == libDir:
        raise ImportError("Couldn't find YbF_spectroscopy_library.py above " + scriptDir)
    libDir = parent
if libDir not in sys.path:
    sys.path.insert(0, libDir)

import YbF_spectroscopy_library as YbF
import numpy as np
#%% Get prediction lines
State_4fv0_174 = YbF.State_4f_7212["v0"][174]    
State_4fv1_174 = YbF.State_4f_7212["v1"][174]    
APi12v0_174 = YbF.A_Pi_12["v0"][174]    
APi32v0_174 = YbF.A_Pi_32_new["v0"][174]
State561 = YbF.State_561["v0"][174]
State557 = YbF.State_557["v0"][174]
State557_Popa = YbF.rule_1858
X_v2 = YbF.X_State["v2"][174]

State574 = np.array([19398]) # in cm^-1
State578 = np.array([19523]) # in cm^-1

#%% Quick transition calculations 
#diff = YbF.Ue(0.5, **State561) - YbF.X_hyperfine(1, **YbF.X_State["v1"][174])
diff = State578 - YbF.Uf4f(0.5, **State_4fv1_174)
print(1e7 / diff) #Converted to wavelength in nm

#%%
StartJ = 0.5
EndJ = 14.5
T = 4    #Temperature in K

StartN = 0
EndN = 10

spectra = YbF.GetSpectra_J(StartJ, EndJ, YbF.branches_4f, State557_Popa, \
                 State_4fv1_174, T)
    
#%% Reshape the dataframe so Branches are columns and J'' is the index
# Strip hidden whitespace from column names just in case
spectra.columns = spectra.columns.str.strip()

# Target column by position (Column index 1 is J'' in your screenshot)
j_col_name = spectra.columns[1] 

spectra_display = spectra.pivot(
    index=j_col_name, 
    columns="Branch", 
    values="Frequency_THz"
).sort_index()
# Optional: Ensure rows are sorted by J''
spectra_display = spectra_display.sort_index()

# Clean up index name so it displays cleanly like your example
spectra_display.index.name = "J"

print(spectra_display)


#%% Level diagram
# Excited states
APi12v0_174_J12 = [YbF.Ue(0.5, **APi12v0_174), YbF.Uf(0.5, **APi12v0_174)]
APi12v0_174_J32 = [YbF.Ue(1.5, **APi12v0_174), YbF.Uf(1.5, **APi12v0_174)]

APi32v0_174_J32 = [YbF.Ue3JCP(1.5, **APi32v0_174), YbF.Uf3JCP(1.5, **APi32v0_174)]

State561_174_J12 = [YbF.Ue(0.5, **State561), YbF.Uf(0.5, **State561)]
State561_174_J32 = [YbF.Ue(1.5, **State561), YbF.Uf(1.5, **State561)]

State557_174_J12 = [YbF.Ue(0.5, **State557), YbF.Uf(0.5, **State557)]
State557_174_J32 = [YbF.Ue(1.5, **State557), YbF.Uf(1.5, **State557)]

#%% 4f ground states
State4f_174_v0_J12 = [YbF.Ue4f(0.5, **State_4fv0_174), YbF.Uf4f(0.5, **State_4fv0_174)]
State4f_174_v0_J32 = [YbF.Ue4f(1.5, **State_4fv0_174), YbF.Uf4f(1.5, **State_4fv0_174)]

State4f_174_v1_J12 = [YbF.Ue4f(0.5, **State_4fv1_174), YbF.Uf4f(0.5, **State_4fv1_174)]
State4f_174_v1_J32 = [YbF.Ue4f(1.5, **State_4fv1_174), YbF.Uf4f(1.5, **State_4fv1_174)]

#%% X ground states
# YbF.X_hyperfine(N, ...) returns 4 hyperfine levels in cm^-1, ordered
# [F=N+1, F=N- (lower F=N), F=N+ (upper F=N), F=N-1]
# N=0 only has F=1 and F=0, so the last two entries are NaN
X_v0_174 = YbF.X_State["v0"][174]
X_v1_174 = YbF.X_State["v1"][174]
X_v2_174 = YbF.X_State["v2"][174]
X_v3_174 = YbF.X_State["v3"][174]

X_174_v0_N0 = YbF.X_hyperfine(0, **X_v0_174)
X_174_v0_N1 = YbF.X_hyperfine(1, **X_v0_174)
X_174_v0_N2 = YbF.X_hyperfine(2, **X_v0_174)
X_174_v0_N3 = YbF.X_hyperfine(3, **X_v0_174)

X_174_v1_N0 = YbF.X_hyperfine(0, **X_v1_174)
X_174_v1_N1 = YbF.X_hyperfine(1, **X_v1_174)
X_174_v1_N2 = YbF.X_hyperfine(2, **X_v1_174)
X_174_v1_N3 = YbF.X_hyperfine(3, **X_v1_174)

X_174_v2_N0 = YbF.X_hyperfine(0, **X_v2_174)
X_174_v2_N1 = YbF.X_hyperfine(1, **X_v2_174)
X_174_v2_N2 = YbF.X_hyperfine(2, **X_v2_174)
X_174_v2_N3 = YbF.X_hyperfine(3, **X_v2_174)

X_174_v3_N0 = YbF.X_hyperfine(0, **X_v3_174)
X_174_v3_N1 = YbF.X_hyperfine(1, **X_v3_174)
X_174_v3_N2 = YbF.X_hyperfine(2, **X_v3_174)
X_174_v3_N3 = YbF.X_hyperfine(3, **X_v3_174)

# Grouped by vibrational level then N, for plotting
X_ground_states = {
    "v0": {0: X_174_v0_N0, 1: X_174_v0_N1, 2: X_174_v0_N2, 3: X_174_v0_N3},
    "v1": {0: X_174_v1_N0, 1: X_174_v1_N1, 2: X_174_v1_N2, 3: X_174_v1_N3},
    "v2": {0: X_174_v2_N0, 1: X_174_v2_N1, 2: X_174_v2_N2, 3: X_174_v2_N3},
    "v3": {0: X_174_v3_N0, 1: X_174_v3_N1, 2: X_174_v3_N2, 3: X_174_v3_N3},
}


#%%
import matplotlib.pyplot as plt

parity_map = {0: "e", 1: "f"}

electronic_states = {
    r"$A\,^2\Pi_{1/2}$": [
        ("J=1/2", APi12v0_174_J12),
        ("J=3/2", APi12v0_174_J32),
    ],
    r"$A\,^2\Pi_{3/2}$": [
        ("J=3/2", APi32v0_174_J32),
    ],
    r"$4f_{7/2, 1/2}$ v0": [
        ("J=1/2", State4f_174_v0_J12),
        ("J=3/2", State4f_174_v0_J32),
    ],
    "[561]": [
        ("J=1/2", State561_174_J12),
        ("J=3/2", State561_174_J32),
    ],
    r"$4f_{7/2, 1/2}$ v1": [
        ("J=1/2", State4f_174_v1_J12),
        ("J=3/2", State4f_174_v1_J32),
    ],
    "[557]": [
        ("J=1/2", State557_174_J12),
        ("J=3/2", State557_174_J32),
    ],
    "[574]": [
        ("(Chi, 2022)", State574),
    ],
    "[578]": [
        ("(Chi, 2022)", State578),
    ],
}

#%% For overall structures
# 1. Define y-limits for both regions
top_limits = (18000, 19600)    # Span = 1600
bottom_limits = (8000, 9500)   # Span = 1500

top_span = top_limits[1] - top_limits[0]
bottom_span = bottom_limits[1] - bottom_limits[0]

# 2. Match subplot heights proportionally to the energy spans
fig, (ax_top, ax_bottom) = plt.subplots(
    2, 1, sharex=True, figsize=(10, 8), 
    gridspec_kw={'height_ratios': [top_span, bottom_span], 'hspace': 0.08}
)

# 3. Apply limits
ax_top.set_ylim(top_limits)
ax_bottom.set_ylim(bottom_limits)

zoom_threshold = 15.0  # cm^-1 window size threshold for text labels
line_width = 0.5

# Hide touching spines
ax_top.spines['bottom'].set_visible(False)
ax_bottom.spines['top'].set_visible(False)
ax_top.xaxis.tick_top()
ax_top.tick_params(labeltop=False, top=False)
ax_bottom.xaxis.tick_bottom()

# 3. Plot energy levels across both subplots
for ax in (ax_top, ax_bottom):
    y_min, y_max = ax.get_ylim()
    show_labels = (y_max - y_min) <= zoom_threshold

    for x_idx, (state_name, j_groups) in enumerate(electronic_states.items()):
        for j_label, state_list in j_groups:
            is_doublet = (len(state_list) == 2)
            
            for idx, state in enumerate(state_list):
                energy = getattr(state, "energy", state) if not isinstance(state, (int, float)) else state
                
                # Only plot levels that fit in the current axis panel
                if y_min <= energy <= y_max:
                    x_start = x_idx - line_width / 2
                    x_end = x_idx + line_width / 2
                    ax.hlines(y=energy, xmin=x_start, xmax=x_end, color="black", linewidth=1.5)
                    
                    if show_labels:
                        if is_doublet:
                            parity = parity_map.get(idx, "")
                            label_text = f"{j_label} ({parity})"
                        else:
                            label_text = j_label

                        ax.text(
                            x_end + 0.05, energy, label_text, 
                            va="center", fontsize=9, color="darkblue",
                            clip_on=True
                        )

    ax.grid(axis="y", linestyle="--", alpha=0.5)

# 4. Add diagonal break lines on the y-axis
d = 0.015  # Size of cut lines
kwargs = dict(transform=ax_top.transAxes, color='k', clip_on=False)
ax_top.plot((-d, +d), (-d, +d), **kwargs)        # Top-left diagonal
ax_top.plot((1 - d, 1 + d), (-d, +d), **kwargs)  # Top-right diagonal

kwargs.update(transform=ax_bottom.transAxes)
ax_bottom.plot((-d, +d), (1 - d, 1 + d), **kwargs)        # Bottom-left diagonal
ax_bottom.plot((1 - d, 1 + d), (1 - d, 1 + d), **kwargs)  # Bottom-right diagonal

# Formatting
ax_bottom.set_xticks(range(len(electronic_states)))
ax_bottom.set_xticklabels(electronic_states.keys(), fontsize=12)
ax_bottom.set_xlim(-0.6, len(electronic_states) - 0.2)

# Set the label on the top axis
ax_top.set_ylabel(r"Energy ($\text{cm}^{-1}$)", fontsize=12)

# Position the label: 
ax_top.yaxis.set_label_coords(-0.15, 0.0)

ax_top.set_title("YbF Energy Level Diagram", fontsize=14, fontweight="bold")

plt.show()

#%% To show rotational features
fig, ax = plt.subplots(figsize=(10, 6))
line_width = 0.5

# 1. Define y-limits first (or comment out set_ylim to view all states)
y_min, y_max = 8470, 8480  # Zoomed-in example
# y_min, y_max = 18000, 19600  # Zoomed-out example

ax.set_ylim(y_min, y_max)

# 2. Set threshold for label visibility (in cm^-1)
# Labels only plot if the total visible energy span is <= 15 cm^-1
zoom_threshold = 15.0
show_labels = (y_max - y_min) <= zoom_threshold

for x_idx, (state_name, j_groups) in enumerate(electronic_states.items()):
    for j_label, state_list in j_groups:
        is_doublet = (len(state_list) == 2)
        
        for idx, state in enumerate(state_list):
            energy = getattr(state, "energy", state) if not isinstance(state, (int, float)) else state
            
            # Plot energy level line
            x_start = x_idx - line_width / 2
            x_end = x_idx + line_width / 2
            ax.hlines(y=energy, xmin=x_start, xmax=x_end, color="black", linewidth=1.5)
            
            # Only draw text if zoomed in close enough
            if show_labels:
                if is_doublet:
                    parity = parity_map.get(idx, "")
                    label_text = f"{j_label} ({parity})"
                else:
                    label_text = j_label

                ax.text(
                    x_end + 0.05, energy, label_text, 
                    va="center", fontsize=9, color="darkblue",
                    clip_on=True
                )

# Formatting
ax.set_xticks(range(len(electronic_states)))
ax.set_xticklabels(electronic_states.keys(), fontsize=12)
ax.set_xlim(-0.6, len(electronic_states) - 0.2)
ax.set_ylabel(r"Energy ($\text{cm}^{-1}$)", fontsize=12)
ax.set_title("YbF Excited States Energy Levels", fontsize=14, fontweight="bold")
ax.grid(axis="y", linestyle="--", alpha=0.5)

plt.show()
#%% X ground state structure
# Choose what to plot and in which units
X_plot_v = ["v0"]#, "v1", "v2", "v3"]  # vibrational levels (columns)
X_plot_N = [0, 1, 2, 3]              # rotational levels
X_unit = "GHz"                       # "cm-1", "THz", "GHz" or "MHz"
X_relative = False   # True: plot each v relative to its own N=0 F=1 level so
                    # rotational/hyperfine structure is visible side by side
X_ylim = None       # e.g. (-0.5, 0.5) to zoom in on N=0 in X_unit; None = auto

# Conversion from cm^-1 (YbF.c is c in cm*GHz)
X_unit_factor = {"cm-1": 1, "THz": YbF.c * 1e-3, "GHz": YbF.c, "MHz": YbF.c * 1e3}
X_unit_label = {"cm-1": r"$\text{cm}^{-1}$", "THz": "THz", "GHz": "GHz", "MHz": "MHz"}

def X_level_label(N, idx):
    """F of each X_hyperfine output, in the order it returns them. The two
    F=N levels are named as in the library: N- is the lower, N+ the upper."""
    if N == 0:
        return ["F=1", "F=0"][idx]
    return [f"F={N + 1}", f"F={N}−", f"F={N}+", f"F={N - 1}"][idx]

fig, ax = plt.subplots(figsize=(10, 8))
line_width = 0.5

# Collect (x, energy, N, F label) for every level in the selection
X_levels = []
for x_idx, v in enumerate(X_plot_v):
    reference = X_ground_states[v][0][0] if X_relative else 0
    for N in X_plot_N:
        for idx, energy in enumerate(X_ground_states[v][N]):
            if np.isnan(energy):
                continue
            X_levels.append((x_idx, (energy - reference) * X_unit_factor[X_unit],
                             N, X_level_label(N, idx)))

for x_idx, energy, N, label in X_levels:
    ax.hlines(y=energy, xmin=x_idx - line_width / 2, xmax=x_idx + line_width / 2,
              color="black", linewidth=1.5)

if X_ylim is not None:
    ax.set_ylim(X_ylim)
y_min, y_max = ax.get_ylim()

# Lines closer than min_gap are drawn as one cluster: each cluster gets an N
# label on its left (a range, e.g. N=0-3, if several N have merged), and an F
# label on its right only if it is a single resolved line
min_gap = 0.02 * (y_max - y_min)

for x_idx in range(len(X_plot_v)):
    column = sorted((e, N, l) for x, e, N, l in X_levels
                    if x == x_idx and y_min <= e <= y_max)
    clusters = []
    for level in column:
        if clusters and level[0] - clusters[-1][-1][0] <= min_gap:
            clusters[-1].append(level)
        else:
            clusters.append([level])

    for cluster in clusters:
        energy = np.mean([e for e, N, l in cluster])
        Ns = sorted({N for e, N, l in cluster})
        N_text = f"N={Ns[0]}" if len(Ns) == 1 else f"N={Ns[0]}–{Ns[-1]}"
        ax.text(x_idx - line_width / 2 - 0.03, energy, N_text, ha="right",
                va="center", fontsize=9, color="darkred", clip_on=True)
        if len(cluster) == 1:
            ax.text(x_idx + line_width / 2 + 0.03, energy, cluster[0][2],
                    va="center", fontsize=9, color="darkblue", clip_on=True)

ax.set_xticks(range(len(X_plot_v)))
ax.set_xticklabels([rf"$X\,^2\Sigma^+$ {v}" for v in X_plot_v], fontsize=12)
ax.set_xlim(-0.6, len(X_plot_v) + 0.1)  # room for labels on the last column
if X_relative:
    ax.set_ylabel(f"Energy relative to N=0, F=1 ({X_unit_label[X_unit]})", fontsize=12)
else:
    ax.set_ylabel(f"Energy ({X_unit_label[X_unit]})", fontsize=12)
ax.set_title(r"$^{174}$YbF X State Energy Levels", fontsize=14, fontweight="bold")
ax.grid(axis="y", linestyle="--", alpha=0.5)

plt.show()
