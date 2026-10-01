# -*- coding: utf-8 -*-
"""
Created on Tue Sep  2 11:49:53 2025

Sideband plotter

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
from scipy import optimize

import glob
import matplotlib.pyplot as plt

import tools as tools

from matplotlib.lines import Line2D
markers = list(Line2D.markers.keys())

from scipy.optimize import curve_fit

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Bessel functions -- EOM mode amplitude
from scipy.special import jv

x = np.arange(0., 5., 0.01)

Bessel0 = jv(0, x) **2
Bessel1 = jv(1, x) **2
Bessel2 = jv(2, x) **2

EOM1stMax = x[np.where(Bessel1 == np.max(Bessel1))[0][0]]
EOM2ndMax = x[np.where(Bessel2 == np.max(Bessel2))[0][0]]

def Bessel0_1(x):
    return jv(0, x) **2 - jv(1, x) **2

def Bessel1_2(x):
    return jv(1, x) **2 - jv(2, x) **2

EOM0_1 = optimize.root(Bessel0_1, 1.3).x[0]
EOM1_2 = optimize.root(Bessel1_2, 2.3).x[0]

plt.plot(x, Bessel0, label = '0th')
plt.plot(x, Bessel1, label = '1st')
plt.plot(x, Bessel2, label = '2nd')

plt.vlines(EOM1stMax, 0, 1, linestyles='-.', color=colors[1], label="%.4g"%EOM1stMax)
plt.vlines(EOM2ndMax, 0, 1, linestyles='-.', color=colors[2], label="%.4g"%EOM2ndMax)

plt.vlines(EOM0_1, 0, 1, linestyles='-.', color=colors[0], label="%.4g"%EOM0_1)
plt.vlines(EOM1_2, 0, 1, linestyles='-.', label="%.4g"%EOM1_2)

plt.legend()
plt.show()

#%%
'''   V2 spectrum    '''

A = np.array([0.65, 0.7, 0.45, 1.0, 0.3, 0.8])
lwStd = 12/2.355 #From FWHM to standard deviation
lwFWHM = 12 #MHz
#Line separation in MHz with respect to line #6
ls6 = np.array([-255.42, -222.45, -198.46, -144.20, -32.98, 0.])

ls6Shifted = ls6 + 111.2

V2F = np.arange(-200., 200., 0.1)

V2spectra = np.zeros(len(V2F))
for i in range(0, len(A)):
    V2spectra += A[i] * tools.Gaussian_FWHM_norm(V2F, ls6Shifted[i], lwFWHM)

plt.plot(V2F, V2spectra)
plt.xlabel("Relative frequency (MHz)")
plt.ylabel("Arb. amplitude")
plt.title("V2 spectrum")
plt.show()

#%% Old V2 sidebands, 50MHz overdriven
Aold = np.array([jv(1, EOM2ndMax), jv(2, EOM2ndMax), jv(1, EOM2ndMax),\
                 jv(0, EOM2ndMax), jv(1, EOM2ndMax), jv(2, EOM2ndMax), jv(1, EOM2ndMax)])

laserFWHM = 2.

lsold = np.array([-150., -100., -50., 0., 50., 100., 150.])

SBold = np.zeros(len(V2F))
for i in range(0, len(Aold)):
    SBold += Aold[i]**2 * tools.Gaussian_FWHM_norm(V2F-10, lsold[i], laserFWHM)
    
plt.plot(V2F, V2spectra, label='V2 spectrum')
plt.plot(V2F, SBold, label='Old sidebands')
plt.xlabel("Relative frequency (MHz)")
plt.ylabel("Arb. amplitude")
plt.title("V2 spectrum")
plt.legend(loc='upper right')
plt.show()

#%% New V2 sidebands, 111.2MHz & 33MHz + some broadening (~6MHz)
A1 = jv(1, EOM1stMax) * jv(1, EOM0_1) * jv(1, EOM0_1) 
A2 = jv(0, EOM1stMax) * jv(1, EOM0_1) * jv(1, EOM0_1) 

Anew = np.zeros(27)

for i in range(0, 3):
    if i == 1:
        Aadd = A2
    else:
        Aadd = A1
    for j in range(0, 9):
        Anew[j+9*i] = Aadd
        
#print(Anew)
EOM1 = 111.2
EOM2 = 33.
EOM3 = 6.5

lsnew = []

lsnew_1 = [-EOM1, 0., EOM1]
lsnew_2 = [-EOM2, 0., EOM2]
lsnew_3 = [-EOM3, 0., EOM3]

for i in range(0, 3):
    for j in range(0, 3):
        lsnew.append(list(np.array(lsnew_3)+lsnew_2[j]+lsnew_1[i]))
            
#print(lsnew)
lsnew = np.array(tools.flattenList(lsnew))

SBnew = np.zeros(len(V2F))

for i in range(0, len(Anew)):
    SBnew += Anew[i]**2 * tools.Gaussian_FWHM_norm(V2F, lsnew[i], laserFWHM)
    
plt.plot(V2F, V2spectra, label='V2 spectrum', color=colors[2])
plt.plot(V2F, SBold, label='Old sidebands', color=colors[1])
plt.plot(V2F, SBnew*2, label='New sidebands', color=colors[0])
plt.xlabel("Relative frequency (MHz)")
plt.ylabel("Arb. amplitude")
plt.title("V2 spectrum and sidebands")
plt.legend(loc='upper right', bbox_to_anchor=(1.32, 1.))
plt.show()