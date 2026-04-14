# -*- coding: utf-8 -*-
"""
Created on Thu Oct 23 14:30:56 2025

Glass calculator

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


import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Functions

def Mech_stress_rect(a, b, t, p):
    '''
    Equation for maximum allowable mechanical stress of BOROFLOAT33 glass 
    from Schott. Representable for other Borosilicate glass. Taken from 
    Schott's leaflet, eqn 4.

    Parameters
    ----------
    a : float, mm
        Width of the unsupported glass (long side).
    b : float, mm
        Depth of the unsupported glass (short side).
    t : float, mm
        Thickness of glass.
    p : float, MPa
        pressure.

    Returns
    -------
    sig : float, MPa
        Maximum allowable mechanical stress.

    '''
    sig = 0.75 * p * b**2 / (t**2 * (1 + 1.61 * b**3/a**3))
    return sig

def Pressure_limit_rect(a, b, t, sig_mech):
    p = sig_mech * t**2 * (1 + 1.61 * b**3/a**3) / (0.75 * b**2)
    return p

#%%
a = 56
b = 50
t = np.arange(0.01, 10, step=0.01)
p_atm = 0.101325

sig_Boro33 = 6

plt.plot(t, Pressure_limit_rect(a, b, t, sig_Boro33), label="BOROFLOAT33")
plt.plot(t, np.zeros(len(t))+p_atm, color="red", label="Atm pressure")
plt.xlabel("Glass thickness (mm)")
plt.ylabel("Maximum allowable pressure (MPa)")
plt.title("Maximum alloable pressure VS glass thickness")
plt.legend()
plt.show()