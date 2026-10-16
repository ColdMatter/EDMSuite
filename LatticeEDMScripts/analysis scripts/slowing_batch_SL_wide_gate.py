# -*- coding: utf-8 -*-
"""
Created on Fri Aug 29 17:29:28 2025

For multiple slowing data (angled probe setpoint scan ON-OFF) analysis

No rolling gate

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

#%% Set data path
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
month= "October2025"#"Slowing data to publish\\Slowing\\Free space\\White light\\70_20 MWV0V1V2"
date= "27"#"20June2025 Downstream"#
#blockdrive=datadrive+"\\BlockData\\"

drive = datadrive + "\\" + month + "\\" + date + "\\"
print(drive)

#%% Load and analyse perpendicular probe data, for calibration
"""Get zero-velocity frequency for V0. Must have
"""
pattern="*_probe*.zip"
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
        TCLconvRef = TCL_WM_cali['best fit'][0]
        TCLconverrRef = TCL_WM_cali['error'][0]
        if np.abs(TCLconvRef) > 1000:
            skipWM = True
        print("\n TCL calibration = %.4g +- %.2g MHz \n"%(TCLconvRef, TCLconverrRef))
    else:
        HasWM = False
        TCLconvRef = 0
        TCLconvReferr = 0
        print("\n Wrong fibre in WM. \n")
    
    FittedGatedTOFRef, fit_resultsRef, FittedGatedTOFWMRef, fit_resultsWMRef, HasWMRef, summaryRef= \
        EDM.ResonanceFreq(DataRef, 28, 38, BkgStart, BkgEnd, fileLabel = fileLabelRef)
    
    RestSP = fit_resultsRef['best fit'][0]
    RestSPerr = fit_resultsRef['error'][0]

    
    if HasWM:
        RestWM = fit_resultsWMRef['best fit'][0]*1e-6+f_iniTHzRef
        RestWMerr = fit_resultsWMRef['error'][0]
        print("Rest frame frequency: %.10g (THz) +- %.3g (MHz)"%(RestWM, RestWMerr))
        print("Rest frame setpoint: %.4g +- %.3g (V)"%(RestSP, RestSPerr))
    else:
        RestWM = 0
        RestWMerr = 0
        print("Rest frame setpoint: %.4g +- %.3g (V)"%(RestSP, RestSPerr))
    
    
else:
    print("\n No rest frame data available on this day. Try loading saved setpoint:")
    patternTXT="*probe*.txt"
    filesTXT = glob.glob(f'{drive}{patternTXT}', recursive=True)
    print("Matching files: ", [os.path.basename(f) for f in filesTXT], "\n")

    if len(filesTXT) != 0:    
        print("Found saved probe setpoint. Reading...")
        with open(filesTXT[0]) as f:
                RestSP = float(f.read())
                RestSPerr = 0.6
                TCLconvRef = 0
                TCLconvReferr = 0
                print("Probe setpint (V): ", RestSP)
    else:
        print("Nothing was saved. Stop here")

#%% Load slowing data
pattern="*angled*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])
N = np.zeros(len(files)) + 4

#%%Only using setpoint for now
files=[files[2]]
print(files)
#%%
SigStart = 23 #in ms. 
SigStartSave = 23 # This is the default range for <=12ms slowing
SigEnd = 50
BkgStart = 60
BkgEnd = 70

wavelength = 552 #in nm

showTOF = True
shot_for_TOF = 20

showRef = True
showCali = False
showGatedTOF = False

forceTCL = True  #If True, forcing the code to use TCL setpoints to calculate 
#                 velocity. For debugging and comparison.

distance = 1.5#{'Downstream':1.5, 'MOT':2.0} # m
angle = 45# {'Downstream':60, 'MOT':45}

#RestSP = 0.867 #Active if fitted setpoint is not the same as setpoint in notes
TCLconvRefSave = -175.518
TCLconvReferrSave = 0.6

fileLabels = []
PeakVDiff = []
PeakVDiffmean = []
PeakVDifferr = []
PeakFit = {}
SlowTime = []

for i in range(0, len(files)):
    fileLabel = re.split(r'[\\]', files[i])[-1][0:3]
    Data = EDM.ReadAverageScanInZippedXML(files[i])
    print("loaded file " + files[i])
    fileLabels.append(fileLabel)
    
    #% Print out all params
    Settings = EDM.GetScanSettings(Data)
    ScanParams = EDM.GetScanParameterArray(Data)
    print(Settings)
    slowing_time = Settings["slowing time"]/1000
    print("\n Slowing duration is " + str(slowing_time) + " ms")
    
    # Some measurements in the MOT has much longer slowing times
    if slowing_time > 12:
        SigStart = slowing_time + Settings['shutterslowDelay']/1000 + 1

    BkgStartIndex = int(BkgStart * (Settings["sampleRate"]/1000))
    BkgEndIndex = int(BkgEnd * (Settings["sampleRate"]/1000))
    
    if TCLconvRef == 0:
        TCLconvRef = TCLconvRefSave
        TCLconvReferr = TCLconvReferrSave
    
    print(TCLconvRef)
    
    f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Data)
    if int(f_iniTHz) == 542:
        TCL_WM_cali = EDM.TCL_WM_Calibration(Data, plot=showCali)
        TCLconv = TCL_WM_cali['best fit'][0]
        TCLconverr = TCL_WM_cali['error'][0]
        HasWM = True
        if np.abs(TCLconv) > 1000:
            HasWM = False
            print("WM data unusable")
            TCLconv = TCLconvRef
            TCLconverr = TCLconverrRef
    else:
        HasWM = False
        TCLconv = TCLconvRef
        TCLconverr = TCLconverrRef
        print("\n Wrong fibre in WM. Use rest frame TCL conversion \n")
    
    
    #Get gated TOF
    TimeOnSPP, DataOnSPP, TimeOffSPP, DataOffSPP = EDM.GetTOFsSPP(Data)

    OnBkgSub, OffBkgSub = EDM.GatedAvgCountsOnOff(Data,DataOnSPP[0],DataOffSPP[0],\
                                                  TimeOnSPP,TimeOffSPP,\
                    SigStart,SigEnd,BkgStart,BkgEnd)
    
    if showGatedTOF:
        title="Gated TOF over " + Settings["param"] + " with " +\
            str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
            str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
                fileLabel
            
        plt.plot(ScanParams, OnBkgSub, '.', label='On')
        plt.plot(ScanParams, OffBkgSub, '.', label='Off')
        plt.xlabel('Setpoint (V)')
        plt.ylabel("Gated LIF (ms.V)")
        plt.title(title)
        plt.legend()
        plt.show()
    
    if forceTCL:
        HasWM = False
    
    title="Gated TOF over velocity with " +\
        str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
        str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
            fileLabel + "for %gms slowing"%slowing_time
    
    if HasWM:
        print("place holder")
    else:
        SPerr = np.ones(shape=len(ScanParams)) * RestSPerr
                
        v, err = EDM.VelocityfromSetpoint(ScanParams, SPerr, RestSP, RestSPerr, \
                                 TCLconv, TCLconverr, wavelength, angle)

        print("For molecules in gate " +str(SigStart) + "ms to " + str(SigEnd) + "ms \n")
            
        peakOn = v[np.where(OnBkgSub == np.max(OnBkgSub))[0][0]]
        #print(peakOn)
        fitOn, covOn = curve_fit(tools.SkewedGaussian, v, OnBkgSub, p0=[peakOn, 15., 0.5, 0.2, 1.])
        errOn = np.sqrt(np.diag(covOn))
        print("Central ON velocity = %.4g +- %.3g m/s"%(fitOn[0], errOn[0]) +\
              "\n with FWHM = %.4g +- %.3g m/s"%(fitOn[1]*2*np.sqrt(2*np.log(2)),\
                                                 errOn[1]*2*np.sqrt(2*np.log(2))))

        peakOff = v[np.where(OffBkgSub == np.max(OffBkgSub))[0][0]]
        #print(peakOff)
        fitOff, covOff = curve_fit(tools.SkewedGaussian, v, OffBkgSub, p0=[peakOff, 15., 0.5, 0.2, 1.])
        errOff = np.sqrt(np.diag(covOff))
        print("Central OFF velocity = %.4g +- %.3g m/s"%(fitOff[0], errOff[0]) +\
              "\n with FWHM = %.4g +- %.3g m/s"%(fitOff[1]*2*np.sqrt(2*np.log(2)),\
                                                 errOff[1]*2*np.sqrt(2*np.log(2))))

        vspan = np.arange(np.min(v), np.max(v), step = 1)
        
        plt.plot(v, OnBkgSub, '.', label='On', color=colors[0])
        plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOn), color=colors[0])
        plt.plot(v, OffBkgSub, '.', label='Off', color=colors[1])
        plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOff), color=colors[1])
        plt.plot(v, OnBkgSub-OffBkgSub, '.-', label='Diff', color="red")
        plt.xlabel('Velocity (m/s)')
        plt.ylabel("Gated TOF (ms.V)")
        plt.title(title)
        plt.legend()
        plt.show()
        
        peakDiffmean = fitOn[0] - fitOff[0]
        #Depending on the skewness, the mean is pulled away from the peak (mode)
        peakDiff = vspan[np.where(tools.SkewedGaussian(vspan, *fitOn) \
                                  == np.max(tools.SkewedGaussian(vspan, *fitOn)))]\
            - vspan[np.where(tools.SkewedGaussian(vspan, *fitOff) \
                           == np.max(tools.SkewedGaussian(vspan, *fitOff)))]
        peakDifferr = np.abs((errOn[0]/fitOn[0] + errOff[0]/fitOff[0])*peakDiff[0])
        
        if peakDifferr < np.abs(peakDiff[0]):
            PeakVDiff.append(peakDiff[0])
            PeakVDiffmean.append(peakDiffmean)
            PeakVDifferr.append(peakDifferr/np.sqrt(N[i]))
            PeakFit[fileLabel] = [fitOn, errOn, fitOff, errOff]
            SlowTime.append(slowing_time)
    
    # Reset slowing time to default, if it changed.
    if slowing_time > 12:
        SigStart = SigStartSave
    
    print("\n")

#%%
PeakVDiff = np.array(PeakVDiff)
PeakVDifferr = np.array(PeakVDifferr)
SlowTime = np.array(SlowTime)

plt.plot(SlowTime, PeakVDiff, '.')
plt.errorbar(SlowTime, PeakVDiff, PeakVDifferr, fmt=' ', color=colors[0], capsize=5)

fit, cov = curve_fit(tools.Line, SlowTime, PeakVDiff, p0=[-1., 0.])#, \
#                     bounds=([-np.inf, -0.01], [np.inf, 0.01]), \
 #                        sigma=PeakVDifferr, absolute_sigma=True)
err = np.sqrt(np.diagonal(cov))
print("Slowing rate: ", fit[0], err[0])

tspan = np.arange(0., 10., 0.01)

plt.plot(tspan, tools.Line(tspan, *fit))

plt.xlabel("Slowing time (ms)")
plt.ylabel("Change in peak velocity (m/s)")
plt.ylim(-15, 2)
plt.show()

#%% On-Off difference
