# -*- coding: utf-8 -*-
"""
Created on Mon Oct 26 11:34:51 2026

Simple linear fit for TCL calibration

@author: sl5119
"""

#%% Packages
import numpy as np
import matplotlib.pyplot as plt
from scipy.optimize import curve_fit

import sys
import os

# On macOS, pythonnet needs pointing at Mono before anything imports clr
if sys.platform == "darwin":
    from pythonnet import load
    load("mono", libmono="/Library/Frameworks/Mono.framework/Versions/Current/lib/libmonosgen-2.0.dylib")

# Add LatticeEDMScripts (parent of "analysis scripts") to the path so the
# library imports work whatever the working directory or OS
try:
    scriptDir = os.path.dirname(os.path.abspath(__file__))
except NameError:  # __file__ isn't defined when running cells interactively
    scriptDir = os.getcwd()
sys.path.append(os.path.dirname(scriptDir))

import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Oct 26th 2026
V0set = [2.623, 2.673, 2.773, 2.923, 3.123, 2.573, 2.473, 2.323, 2.123, 1.873]
V0freq = [542.808651, 542.808640, 542.808616, 542.808583, 542.808540, 542.808664,
          542.808689, 542.808725, 542.808775, 542.808918]

V0set2  = [1.5235, 1.5735, 1.6235, 1.6735, 1.7235, 1.7735, 1.8234, 1.8735, 1.9235, 1.9735,
          2.0235, 2.0735, 2.1235, 2.1735, 2.2235, 2.2735, 2.3235, 2.3735, 2.4235, 2.4735,
          2.5235, 2.5735, 2.6235, 2.6735]

V0freq2 = [542.808941, 542.808928, 542.808916, 542.808903, 542.808891, 542.808878, 542.808865,
          542.808851, 542.808839, 542.808826, 542.808813, 542.808802, 542.808789, 542.808775,
          542.808764, 542.808750, 542.808738, 542.808726, 542.808714, 542.808702, 542.808688,
          542.808677, 542.808664, 542.808653]

V1set = [-0.394, -0.344, -0.244, -0.094, 0.105, -0.444, -0.494, -0.594, -0.744, -0.994]
V1freq = [527.754907, 527.754892, 527.754870, 527.754833, 527.754784,
          527.754921, 527.754932, 527.754956, 527.754993, 527.755040]

V2set = [2.874, 2.824, 2.724, 2.574, 2.374, 2.924, 3.024, 3.174, 3.374, 3.574]
V2freq = [530.794566, 530.794576, 530.794589, 530.794616, 530.794652,
          530.794558, 530.794538, 530.794515, 530.794472, 530.794435]

V3set = [-0.449, -0.499, -0.599, -0.749, -0.949, -0.399, -0.299, -0.149, 0.050]
V3freq = [512.277141, 512.277154, 512.277176, 512.277212, 512.277260,
          512.277129, 512.277106, 512.277071, 512.277025]

TCLs = {"v0": [V0set, V0freq],
        "v0 retake": [V0set2, V0freq2],
        "v1": [V1set, V1freq],
        "v2": [V2set, V2freq],
        "v3": [V3set, V3freq]}

#%% Plot & fit
Cal = {}
i = 0
keys = list(TCLs.keys())

for k in keys:
    Set = np.array(TCLs[k][0])
    Freq = np.array(TCLs[k][1])
    
    plt.plot(Set, (Freq-Freq[0])*1e6, '.', label=k, color = colors[i])
    
    fit, cov = curve_fit(tools.Line, Set, Freq*1e6, p0=[-120., Freq[0]*1e6])
    err = np.sqrt(np.diag(cov))
    
    print(k, fit, err)
    print("TCL calibration = %.4g +- %.2g MHz/V"%(fit[0], err[0]))
    
    Sspan = np.arange(Set[0] - 1.5, Set[0] + 1.5, 0.01)
    
    plt.plot(Sspan, (tools.Line(Sspan, *fit)*1e-6-Freq[0])*1e6, color = colors[i])
    
    i += 1
    
    Cal[k] = [fit, err]
    
plt.ylabel("Relative Frequency (MHz) \n from Slowing setpoint")
plt.xlabel("TCL setpoint (V)")
plt.title("TCL calibration")
plt.legend()
plt.show()

#%% Export
import pandas as pd

CalDF = pd.DataFrame(Cal)

Save = tools.save_dataframe_interactive(CalDF, default_name="TCL calibration.csv")