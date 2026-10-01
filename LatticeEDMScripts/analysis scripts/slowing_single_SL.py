# -*- coding: utf-8 -*-
"""
Created on Wed Aug  6 14:15:48 2025

For single slowing data (angled probe setpoint scan ON-OFF) analysis

The second half of the script is for converting TOFs to velocity distributions

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

import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Set data path
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month="August2025"
date="20"
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\"
print(drive)

#%% Load and analyse perpendicular probe data, for calibration
"""Get zero-velocity frequency for V0. Must have
"""
pattern="*_ProbeSetpointScan*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files], "\n")


if len(files) > 0:
    DataRef = EDM.ReadAverageScanInZippedXML(files[0])
    SettingsRef = EDM.GetScanSettings(DataRef)
    ScanParamsRef = EDM.GetScanParameterArray(DataRef)
    fileLabelRef = re.split(r'[\\]', files[0])[-1][0:3]
    
    SigStart = 25 #in ms
    SigEnd = 27
    BkgStart = 60
    BkgEnd = 70
    
    f_iniTHzRef, f_relMHzRef = EDM.GetScanFreqArrayMHz(DataRef)
    if int(f_iniTHzRef) == 542:
        TCL_WM_cali = EDM.TCL_WM_Calibration(DataRef, plot=True)
        HasWM = True
        TCLconv = TCL_WM_cali['best fit'][0]
        TCLconverr = TCL_WM_cali['error'][0]
        if np.abs(TCLconv) > 1000:
            skipWM = True
        print("\n TCL calibration = %.4g +- %.2g MHz \n"%(TCLconv, TCLconverr))
    else:
        HasWM = False
        print("\n Wrong fibre in WM. \n")
    
    FittedGatedTOFRef, fit_resultsRef, FittedGatedTOFWMRef, fit_resultsWMRef, HasWMRef, summaryRef= \
        EDM.ResonanceFreq(DataRef, 25, 27, BkgStart, BkgEnd, fileLabel = fileLabelRef)
    
    RestSP = fit_resultsRef['best fit'][0]
    RestSPerr = fit_resultsRef['error'][0]
    RestWM = fit_resultsWMRef['best fit'][0]*1e-6+f_iniTHzRef
    RestWMerr = fit_resultsWMRef['error'][0]
    
    if HasWM:
        print("Rest frame frequency: %.10g (THz) +- %.3g (MHz)"%(RestWM, RestWMerr))
        print("Rest frame setpoint: %.4g +- %.3g (V)"%(RestSP, RestSPerr))
    else:
        print("Rest frame setpoint: %.4g +- %.3g (V)"%(RestSP, RestSPerr))
    
    
else:
    print("\n No rest frame data available on this day.")

#%% Load slowing data
pattern="*angled*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])
#%%
Data = EDM.ReadAverageScanInZippedXML(files[0])
print("\n loaded file " + files[0] + "\n")

#% Print out all params
Settings = EDM.GetScanSettings(Data)
ScanParams = EDM.GetScanParameterArray(Data)
print(Settings)

slowing_time = Settings["slowing time"]/1000
print("\n Slowing duration is " + str(slowing_time) + " ms")

fileLabel = re.split(r'[\\]', files[0])[-1][0:3]

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 17 #in ms
SigEnd = 31
BkgStart = 60
BkgEnd = 70

wavelength = 552 #in nm

showTOF = True
shot_for_TOF = 35

BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))

f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Data)
if int(f_iniTHz) == 542:
    TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=True)
    TCLconv = TCL_WM_cali['best fit'][0]
    TCLconverr = TCL_WM_cali['error'][0]
    HasWM = True
else:
    HasWM = False
    print("\n Wrong fibre in WM. \n")

distance = 1.5 # m
MOTdistance = 1.8 
angle = 45

gateLength = 1
gatesStart = np.arange(SigStart, SigEnd, step=gateLength)
gatesEnd = np.arange(SigStart+gateLength, SigEnd+gateLength, step=gateLength)
gateC = (gatesStart + gatesEnd)/2

v_exp = EDM.ExpectedVelocity(distance, gateC) #in m/s, gate in ms
v_expMOT = EDM.ExpectedVelocity(MOTdistance, gateC)

#% Get rest frame setpoint (not in use)
#print("Get rest frame velocity of the gate range:")
#RestSP, RestSPerr, RestGatedTOFSP, RestWM, RestWMerr, RestGatedTOFWM =\
#    EDM.FitGatedTOFOn(DataRef, SigStart, SigEnd, BkgStart, BkgEnd, distance)

#print("Zero velocity setpoint = %.3g +- %.2g V"%\
#      (RestSP, RestSPerr))

#if RestWM != 0:
#    print("zero velocity frequency = %.10g THz +- %.2g MHz"%\
#           (RestWM, RestWMerr))


#%% Get TOFs, averaged per point
"""  Must run before proceeding  """
TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF], axis=1),\
             label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF], axis=1),\
             label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF]), \
               ymax=np.max(DataOffSPP[0][shot_for_TOF]),\
                   linestyles="dashed", colors="black")
    plt.title("TOF of the 20th scan point, averaged for all shots")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.legend()
    plt.show()

#% Bkg-sub TOF
BkgOn = np.average(DataOnSPP[0][0][BkgStartIndex:BkgEndIndex])
BkgOff = np.average(DataOffSPP[0][0][BkgStartIndex:BkgEndIndex])

if showTOF:
    plt.plot(TimeOnSPP*1000, np.average(DataOnSPP[0][shot_for_TOF],\
                                        axis=1)-BkgOn, label="On")
    plt.plot(TimeOffSPP*1000, np.average(DataOffSPP[0][shot_for_TOF],\
                                         axis=1)-BkgOff, label="Off")
    plt.vlines([SigStart, SigEnd, BkgStart, BkgEnd],\
               ymin=np.min(DataOnSPP[0][shot_for_TOF])-BkgOn, \
               ymax=np.max(DataOffSPP[0][shot_for_TOF])-BkgOff,\
                   linestyles="dashed", colors="black")
    plt.title("TOF of the 20th scan point, averaged for all shots,\n \
              background subtracted")
    plt.xlabel("time (ms)")
    plt.ylabel("PMT signal (V)")
    plt.ylim(-0.3, 3.5)
    plt.legend()
    plt.show()

#%% Gated TOF with rolling gates
FitResultsOn, FitResultsOff, FittedGatedTOFs,\
    FitResultsOnWM, FitResultsOffWM, FittedGatedTOFsWM = EDM.FitGatedTOFperGate(Data,\
                                            DataOnSPP[0], TimeOnSPP,\
                                            DataOffSPP[0], TimeOffSPP,\
                       gatesStart, gatesEnd, gateC, BkgStart, BkgEnd,\
                       HasWM, distance, display=True)

#To show the plots, put it in console directly. Otherwise it will display all 
#plots (raw data, with one fit, and with both fits). Can export figures later.
#%% Convert to velocity distribution
'''Aug 26th 2025 meeting: this is not the best way to analyse slowing

   Use slowing_single_SL_wide_gate instead.
'''
angle = 45
RestSP = -1.69
#RestSPerr = TCLconverr
#TCLconv2 = -156.884
velocityFig, meanVOn, meanVOnerr, meanVOff, meanVOfferr = \
    EDM.VelocityOnOff(FitResultsOn, FitResultsOff, RestSP, RestSPerr, v_exp,\
                      TCLconv, TCLconverr, wavelength, angle, slowing_time, gateC, gateLength,\
                          file=fileLabel)
        
#%%
velocityFigWM, meanVOnWM, meanVOnerrWM, meanVOffWM, meanVOfferrWM = \
    EDM.VelocityOnOffWM(FitResultsOnWM, FitResultsOffWM, RestWM, RestWMerr, v_exp,\
                      f_iniTHz, wavelength, slowing_time, gateC, gateLength,\
                          file=fileLabel)

#%% Compare
diffV = meanVOn - meanVOff
diffVerr = np.sqrt(meanVOnerr**2 + meanVOfferr**2)
#diffVWM = meanVOnWM - meanVOffWM
#diffVerrWM = np.sqrt(meanVOnerrWM**2 + meanVOfferrWM**2)

plt.plot(gateC, diffV, '.', markersize=15, label="from setpoint")
plt.errorbar(gateC, diffV, yerr=diffVerr, capsize=3, fmt=' ')
#plt.plot(gateC, diffVWM, '.', markersize=15, label="from WM")
#plt.errorbar(gateC, diffVWM, yerr=diffVerrWM, capsize=3, fmt=' ')
plt.title("Velocity change")#", comparison of two calculations")
plt.xlabel("Gate center (ms)")
plt.ylabel("Velocity change (m/s)")
plt.legend()
plt.show()

#%% Convert TOF to velocity distribution
'''Not yet adapted for using WM readings'''
#% To velocity
meanSPOn = []
meanSPOnerr = []
meanSPOnSpread = []
meanSPOff = []
meanSPOffSpread = []
meanSPOfferr = []
for i in range(0, len(FitResultsOn)):
    meanSPOn.append(FitResultsOn[i]["best fit"][0])
    meanSPOnSpread.append(FitResultsOn[i]["best fit"][1])
    meanSPOnerr.append(FitResultsOn[i]["error"][0])
    meanSPOff.append(FitResultsOff[i]["best fit"][0])
    meanSPOffSpread.append(FitResultsOff[i]["best fit"][1])
    meanSPOfferr.append(FitResultsOff[i]["error"][0])

#For WM
#ResonancesDS = np.array(Resonances)
#ResonanceerrsDS = np.array(Resonanceerrs)

meanSPOn = np.array(meanSPOn)
meanSPOnerr = np.array(meanSPOnerr)
meanSPOnSpread = np.array(meanSPOnSpread)
meanSPOff = np.array(meanSPOff)
meanSPOffSpread = np.array(meanSPOffSpread)
meanSPOfferr = np.array(meanSPOfferr)

#%%
def VelocityfromFshift(dF, F0, angle, dFerr):
    angle_rad = angle * np.pi / 180
    v = -dF*1e6 * 299792458 / (F0*1e12 * np.cos(angle_rad))
    dv = dFerr/dF * v
    return v, dv

#vDS, dvDS = VelocityfromFshift(ResonancesDS, 542.8091807917087-56.05068161*1e-6, 60, ResonanceerrsDS)

#vDSOn, dvDSOn = EDM.VelocityfromFreq(ResonancesDS, ResonanceerrsDS,\
#                        542.8091807917087-56.05068161*1e-6, 0.64214063, \
#                         f_iniTHz, 552, 60)
#vDS, dvDSspread = VelocityfromFshift(Resonances, \
#                            542.8091471996183-26.48974844*1e-6, 60, Spread)
    
vDSOn, dvDSOn = EDM.VelocityfromSetpoint(meanSPOn, meanSPOnerr, \
                                    RestSP, RestSPerr, \
                         TCLconv, TCLconverr, wavelength, angle)
vDSOn, dvDSspreadOn = EDM.VelocityfromSetpoint(meanSPOn, meanSPOnSpread, \
                                    RestSP, RestSPerr, \
                         TCLconv, TCLconverr, wavelength, angle)
vDSOff, dvDSOff = EDM.VelocityfromSetpoint(meanSPOff, meanSPOfferr, \
                                    RestSP, RestSPerr, \
                         TCLconv, TCLconverr, wavelength, angle)
vDSOff, dvDSspreadOff = EDM.VelocityfromSetpoint(meanSPOff, meanSPOffSpread, \
                                    RestSP, RestSPerr, \
                         TCLconv, TCLconverr, wavelength, angle)

#if HasWM:
#    plt.plot(gateC, vDSOn, label="On")#locations_angled[i])
#    plt.fill_between(gateC, vDSOn+dvDSOn, vDSOn-dvDSOn, alpha=0.3)
#    plt.fill_between(gateC, vDSOn+dvDSspreadOn, vDSOn-dvDSspreadOn, alpha=0.3, color=colors[0])
#    plt.ylabel("Velocity (m/s)")
#if HasWM:
plt.plot(gateC, vDSOn, label="On")#locations_angled[i])
plt.fill_between(gateC, vDSOn+dvDSOn, vDSOn-dvDSOn, alpha=0.3)
plt.fill_between(gateC, vDSOn+dvDSspreadOn, vDSOn-dvDSspreadOn, alpha=0.3, color=colors[0])

plt.plot(gateC, vDSOff, label="Off")#locations_angled[i])
plt.fill_between(gateC, vDSOff+dvDSOff, vDSOff-dvDSOff, alpha=0.3)
plt.fill_between(gateC, vDSOff+dvDSspreadOff, vDSOn-dvDSspreadOff, alpha=0.3, color=colors[1])

plt.ylabel("Velocity (m/s)")
    
plt.title("Velocity VS time")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()

#%% fit trend
dDS = distance
def timeTOvelocity(v, A, tau_v, d=dDS):
    return d/v + A*np.exp(-v/tau_v)

def dtdv(v, A, tau_v, d=dDS):
    return -A/tau_v * np.exp(-v/tau_v) - d/v**2

from scipy.optimize import curve_fit
fitTOn, covTOn = curve_fit(timeTOvelocity, vDSOn, gateC, p0=[1., 10.])
print(str(fitTOn))

fitTOff, covTOff = curve_fit(timeTOvelocity, vDSOff, gateC, p0=[1., 10.])
print(str(fitTOff))

plt.plot(gateC, vDSOn, label="On")
plt.fill_between(gateC, vDSOn+dvDSOn, vDSOn-dvDSOn, alpha=0.3)
plt.fill_between(gateC, vDSOn+dvDSspreadOn, vDSOn-dvDSspreadOn, alpha=0.3, color=colors[0])

plt.plot(gateC, vDSOff, label="Off")
plt.fill_between(gateC, vDSOff+dvDSOff, vDSOff-dvDSOff, alpha=0.3)
plt.fill_between(gateC, vDSOff+dvDSspreadOff, vDSOff-dvDSspreadOff, alpha=0.3, color=colors[1])

v = np.arange(40, 170, step=0.1)

plt.plot(timeTOvelocity(v, *fitTOn), v, '-.', label='fit On', color=colors[0])
plt.plot(timeTOvelocity(v, *fitTOff), v, '-.', label='fit Off', color=colors[1])

plt.ylabel("Velocity (m/s)")
plt.title("Velocity - time mapping")
plt.xlabel("Time (ms)")
plt.legend()
plt.show()

plt.close()

#%%
dDS = distance #For Downstream, pre Aug 18th 2025. After is the MOT distance.

def VelocityFromTime(v, A, tau_v, t, d):
    return d/v + A*np.exp(-v/tau_v) - t
#This should equate to 0. Use this to find root --> find v

#From Aug 17th 2025 Downstream angled probe measurement:
#A, tau_v = 102.70390736, 72.18821354

from scipy.optimize import fsolve
VelocityOn = []
VelocityOff = []
for t in TimeOnSPP[1::]:
    #arg = [A, tau_v, t*1000]
    v = fsolve(VelocityFromTime, x0=100., args=(fitTOn[0], fitTOn[1], t*1000, dDS))[0]
    VelocityOn.append(v)
    v = fsolve(VelocityFromTime, x0=100., args=(fitTOff[0], fitTOff[1], t*1000, dDS))[0]
    VelocityOff.append(v)
    
VelocityOn = np.array(VelocityOn)
VelocityOff = np.array(VelocityOff)

#%%Plot a single TOF. In a slowing measurement, just choose a point?
#Can try different TOFs
shot_for_TOF = 35

TOFOn_t = np.average(DataOnSPP[0][shot_for_TOF], axis=1)-BkgOn
TOFOff_t = np.average(DataOffSPP[0][shot_for_TOF], axis=1)-BkgOff

TOFOn_v = TOFOn_t[1::] * (-1) * dtdv(VelocityOn, fitTOn[0], fitTOn[1])
TOFOff_v = TOFOff_t[1::] * (-1) * dtdv(VelocityOff, fitTOff[0], fitTOff[1])


plt.plot(VelocityOn, TOFOn_v, label="On")
plt.plot(VelocityOff, TOFOff_v, label="Off")

plt.title("Velocity distribution reconstructed from TOF %g, \n on "%shot_for_TOF+\
              date + " " + month + ". Setpoint %g V"%ScanParams[shot_for_TOF] +\
                  ", %gms slowing"%slowing_time)
#plt.title("Averaged TOF over 100 shots with background subtraction")
plt.xlabel("Velocity (m/s)")
plt.ylabel("PMT signal (V)")
plt.xlim(0, 200)
plt.ylim(-0.1, 0.2)
plt.legend(loc="upper right")
plt.show()
plt.close()

#%%
v = np.arange(1, 200, step=0.1)
plt.plot(VelocityOn, dtdv(VelocityOn, fitTOn[0], fitTOn[1]), label='On')
plt.plot(v, dtdv(v, fitTOff[0], fitTOff[1]), label='Off')
plt.legend()
plt.show()

#%%
plt.plot(VelocityOn, TimeOnSPP[1::]*1000)
plt.show()