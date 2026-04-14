# -*- coding: utf-8 -*-
"""
Created on Thu Aug 28 14:59:31 2025

This is for analysing slowing data in batches and generating deceleration plots
for selected data

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

from matplotlib.lines import Line2D
markers = list(Line2D.markers.keys())

from scipy.optimize import curve_fit

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Set data path
datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
selection="Slowing data to publish\\Slowing\\Fibered\\"#"White light\\"#"August2025"
datapath = datadrive + "\\" + selection
typepaths = [f.path for f in os.scandir(datapath) if f.is_dir()] #This gives the full paths
#print(typepaths)

types = []
for t in typepaths:
    types.append(re.split(r'[\\]', t)[-1])
print(types)

#% drop folder(s) not to be analysed
start = 0
end = len(types) -1
typesele = types[start:end]
print(typesele)

#typesele = ["15_6.5", "70_20"]  #by-pass
#print(typesele)
#%
#date="20Jan2025\\NoMW"
#subfolders = ["V0", "V0V1", "V0V1V2", "V0V1V2V3"]
#blockdrive=datadrive+"\\BlockData\\"

#%% This cell tries to be as generic as possible
#Only using setpoint for now
SigStart = 23 #in ms. 
SigStartSave = 23 # This is the default range for <=12ms slowing
SigEnd = 38
BkgStart = 60
BkgEnd = 70

wavelength = 552 #in nm

showTOF = True
shot_for_TOF = 35

showRef = True
showCali = False
showGatedTOF = False

forceTCL = True  #If True, forcing the code to use TCL setpoints to calculate 
#                 velocity. For debugging and comparison.

TCLconvSave = -175.518  #A calibration from the day, in case missing WM data for 
TCLconverrSave = 0.6  #both rest frame scan and the angled scan. 

TCLconvRefSave = -175.518
TCLconvReferrSave = 0.6

distance = {'Downstream':1.5, 'MOT':2.0} # m
angle = {'Downstream':60, 'MOT':45}

Subfolders = {}
Locs = {}
Dates = {}

AllfileLabels = {}
AllRefs = {}
AllPeakVDiffs = {}
AllPeakVDiffmeans = {}
AllPeakVDifferrs = {}
AllPeakFits = {}
AllSlowTimes = {}

for t in typesele:
    print("In selection -- " + t)
    Date = []
    loc = []
    subfolders = []
    subpaths = [f.path for f in os.scandir(datapath + t) if f.is_dir()]
    for d in subpaths:
        Date.append(re.split(r'[ ]', re.split(r'[\\]', d)[-1])[0])
        loc.append(re.split(r'[ ]', re.split(r'[\\]', d)[-1])[1])
        subfolders.append(re.split(r'[\\]', d)[-1])
    #drive = datapath + typesele + "\\" + date + "\\"
    
    #Save info, in case they're needed again
    Subfolders[t] = subfolders
    Dates[t] = Date
    Locs[t] = loc
    
    print("subfolders: ", subfolders)
    print("dates: ", Date)
    print("locations :", loc)

    print("\n")
    
    print("Start analysis in ", t)
    
    Refs = {}
    PeakVDiffs = {}
    PeakVDiffmeans = {}
    PeakVDifferrs = {}
    PeakFits = {}
    sfileLabels = {}
    SlowTimes = {}
    
    for s in subfolders:
        drive = datapath + t + "\\" + s + "\\"
        print(drive)
        
        location = re.split(r'[ ]', s)[-1]
        date = re.split(r'[ ]', s)[0]
        
        """
        Get zero-velocity frequency for V0. Must have
        """
        print("\n Getting zero-velocity frequency for V0. \n")

        pattern="*_probe*.zip"
        files = glob.glob(f'{drive}{pattern}', recursive=True)
        print("Matching files: ", [os.path.basename(f) for f in files], "\n")
    
        if len(files) > 0:
            DataRef = EDM.ReadAverageScanInZippedXML(files[0])
            SettingsRef = EDM.GetScanSettings(DataRef)
            ScanParamsRef = EDM.GetScanParameterArray(DataRef)
            fileLabelRef = re.split(r'[\\]', files[0])[-1][0:3]
            
            f_iniTHzRef, f_relMHzRef = EDM.GetScanFreqArrayMHz(DataRef)
            
            FittedGatedTOFRef, fit_resultsRef, FittedGatedTOFWMRef, fit_resultsWMRef, HasWMRef, summaryRef= \
                EDM.ResonanceFreq(DataRef, SigStart, SigEnd, BkgStart, BkgEnd,\
                                  fileLabel = fileLabelRef, showPlot=showRef)
            
            Refs[s] = summaryRef
            
            RestSP = summaryRef[2] #fit_resultsRef['best fit'][0]
            RestSPerr = summaryRef[3] #fit_resultsRef['error'][0]
            RestWM = summaryRef[0]*1e-6+f_iniTHzRef  #fit_resultsWMRef['best fit'][0]
            RestWMerr = summaryRef[1] #fit_resultsWMRef['error'][0]
            
            TCLconvRef = summaryRef[4]
            TCLconverrRef = summaryRef[5]
            
            if HasWMRef:
                print("Rest frame frequency: %.10g (THz) +- %.3g (MHz)"%(RestWM, RestWMerr))
                print("Rest frame setpoint: %.4g +- %.3g (V)"%(RestSP, RestSPerr))
            else:
                TCLconvRef = 0
                TCLconvReferr = 0
                print("Rest frame setpoint: %.4g +- %.3g (V)"%(RestSP, RestSPerr))
                print("\n")
            
            
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
                        TCLconverrRef = 0
                        print("Probe setpint (V): ", RestSP)
            else:
                print("Nothing was saved. Stop here")
            print("\n")
       
        '''Get peak velocity reduction from slowing data'''
        print("Analysing slowing data. \n")
        
        pattern="*angled*.zip"
        files = glob.glob(f'{drive}{pattern}', recursive=True)
        print("Matching files: ", [os.path.basename(f) for f in files])
        
        #Data = {}  #No need to save raw data
        fileLabels = []
        PeakVDiff = []
        PeakVDiffmean = []
        PeakVDifferr = []
        PeakFit = {}
        SlowTime = []
        
        if TCLconvRef == 0:
            TCLconvRef = TCLconvRefSave
            TCLconvReferr = TCLconvReferrSave
        
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
                str(SigStart) + "ms to " + str(SigEnd) + "ms gate, %gms slowing, \n file "%slowing_time +\
                    fileLabel + ", " + t + ", " + s
            
            if HasWM:
                print("place holder")
            else:
                SPerr = np.ones(shape=len(ScanParams)) * RestSPerr
                
                if TCLconv == 0:
                    TCLconv = TCLconvSave
                    TCLconverr = TCLconverrSave
                        
                v, err = EDM.VelocityfromSetpoint(ScanParams, SPerr, RestSP, RestSPerr, \
                                         TCLconv, TCLconverr, wavelength, angle[location])

                print("For molecules in gate " +str(SigStart) + "ms to " + str(SigEnd) + "ms \n")
                    
                peakOn = v[np.where(OnBkgSub == np.max(OnBkgSub))[0][0]]
                fitOn, covOn = curve_fit(tools.SkewedGaussian, v, OnBkgSub, p0=[peakOn, 10., 0.5, 0.5, 1.])
                errOn = np.sqrt(np.diag(covOn))
                print("Central ON velocity = %.4g +- %.3g m/s"%(fitOn[0], errOn[0]) +\
                      "\n with FWHM = %.4g +- %.3g m/s"%(fitOn[1]*2*np.sqrt(2*np.log(2)),\
                                                         errOn[1]*2*np.sqrt(2*np.log(2))))

                peakOff = v[np.where(OffBkgSub == np.max(OffBkgSub))[0][0]]
                fitOff, covOff = curve_fit(tools.SkewedGaussian, v, OffBkgSub, p0=[peakOff, 10., 1., 0.5, 1.])
                errOff = np.sqrt(np.diag(covOff))
                print("Central OFF velocity = %.4g +- %.3g m/s"%(fitOff[0], errOff[0]) +\
                      "\n with FWHM = %.4g +- %.3g m/s"%(fitOff[1]*2*np.sqrt(2*np.log(2)),\
                                                         errOff[1]*2*np.sqrt(2*np.log(2))))

                vspan = np.arange(np.min(v), np.max(v), step = 1)
                
                plt.plot(v, OnBkgSub, '.', label='On', color=colors[0])
                plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOn), color=colors[0])
                plt.plot(v, OffBkgSub, '.', label='Off', color=colors[1])
                plt.plot(vspan, tools.SkewedGaussian(vspan, *fitOff), color=colors[1])
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
                    PeakVDifferr.append(peakDifferr)
                    PeakFit[fileLabel] = [fitOn, errOn, fitOff, errOff]
                    SlowTime.append(slowing_time)
            
            # Reset slowing time to default, if it changed.
            if slowing_time > 12:
                SigStart = SigStartSave
            
            print("\n")
        
        PeakVDiffs[s] = np.array(PeakVDiff)
        PeakVDiffmeans[s] = np.array(PeakVDiffmean)
        PeakVDifferrs[s] = np.array(PeakVDifferr)
        PeakFits[s] = PeakFit
        sfileLabels[s] = fileLabels
        SlowTimes[s] = np.array(SlowTime)
        print("\n")
    
    AllfileLabels[t] = sfileLabels
    AllRefs[t] = Refs
    AllPeakVDiffs[t] = PeakVDiffs
    AllPeakVDiffmeans[t] = PeakVDiffmeans
    AllPeakVDifferrs[t] = PeakVDifferrs
    AllPeakFits[t] = PeakFits
    AllSlowTimes[t] = SlowTimes
    print("\n")
    
#%% Combine plots
colorID = 0
markerID = 0

for t in typesele:
    allslowtime = []
    allpeakVdiff = []
    allpeakVdifferr = []
    allpeakVdiffmean = []
    
    allslowtime2 = []
    allpeakVdiff2 = []
    allpeakVdifferr2 = []
    
    for s in Subfolders[t]:
        allslowtime.append(AllSlowTimes[t][s])
        allpeakVdiff.append(AllPeakVDiffs[t][s])
        allpeakVdifferr.append(AllPeakVDifferrs[t][s])
        allpeakVdiffmean.append(AllPeakVDiffmeans[t][s])
        
        for i in range(0, len(AllSlowTimes[t][s])):
            if AllSlowTimes[t][s][i] < 12:
                allslowtime2.append(AllSlowTimes[t][s][i])
                allpeakVdiff2.append(AllPeakVDiffs[t][s][i])
                allpeakVdifferr2.append(AllPeakVDifferrs[t][s][i])
        
        #If plot by dates under each category:
        #plt.plot(AllSlowTimes[t][s], AllPeakVDiffs[t][s], markers[markerID], markersize=10,\
        #         color=colors[colorID])#, label=t+", "+s)
        #plt.errorbar(AllSlowTimes[t][s], AllPeakVDiffs[t][s],\
        #             yerr=AllPeakVDifferrs[t][s], fmt=' ', capsize=5,\
        #                 color=colors[colorID])
    
    plt.plot(tools.flattenList(allslowtime), tools.flattenList(allpeakVdiff),\
             markers[markerID], markersize=14,\
             color=colors[colorID], label="With "+t)
    plt.errorbar(tools.flattenList(allslowtime), tools.flattenList(allpeakVdiff),\
                 yerr=tools.flattenList(allpeakVdifferr), fmt=' ', capsize=5,\
                     color=colors[colorID])
            
        #markerID += 1
        #if markerID == 1:
        #    markerID += 1
    
    allslowtime = np.array(tools.flattenList(allslowtime))
    allpeakVdiff = np.array(tools.flattenList(allpeakVdiff))
    allpeakVdifferr = np.array(tools.flattenList(allpeakVdifferr))
    allpeakVdiffmean = np.array(tools.flattenList(allpeakVdiffmean))
    
    
    fit, cov = curve_fit(tools.Line, allslowtime, allpeakVdiff, p0=[-0.5, 0.],\
                         bounds=([-np.inf, -0.01], [np.inf, 0.01]))#, \
                            # sigma=allpeakVdifferr, absolute_sigma=True)
    err = np.sqrt(np.diag(cov))
    
    if t == 'MWV0': #'70_20 MWV0V1V2':
        stspan = np.arange(0., 18., 0.1)
        plt.plot(stspan, tools.Line(stspan, *fit), color=colors[colorID])
        #print(fit, err)
    print("With " + t + ", slowing rate is %.3g +- %.2g m/s per ms \n"%(fit[0], err[0]))
    
    if t == "70_20 MWV0V1V2":
        allslowtime2 = np.array(allslowtime2)
        allpeakVdiff2 = np.array(allpeakVdiff2)
        allpeakVdifferr2 = np.array(allpeakVdifferr2)
        
       # fit2, cov2 = curve_fit(tools.Line, allslowtime2, allpeakVdiff2, p0=[-0.5, 0.],\
       #                      bounds=([-np.inf, -0.01], [np.inf, 0.01]), \
       #                          sigma=allpeakVdifferr2, absolute_sigma=True)
       # err2 = np.sqrt(np.diag(cov2))
       # plt.plot(stspan, tools.Line(stspan, *fit2), '-.', color=colors[colorID+1], \
       #          label="With "+t + ", <12ms")
        #print(fit, err)
       # print("With " + t + ", <12ms slowing rate is %.3g +- %.2g m/s per ms \n"%(fit2[0], err2[0]))
        
       # colorID +=1
        
    colorID +=1

plt.plot(stspan, tools.Line(stspan, -1., 0.), '-.', color="red", label="-1000 m/s^2")

plt.xlabel("Slowing time (ms)")
plt.ylabel("Change in peak velocity (m/s)")
plt.title("Slowing with microwave repumps")
plt.ylim(-20, 1.5)
plt.legend()#loc="upper right")#, bbox_to_anchor=(1.45, 1.))
plt.show()

#%% Exp decay fit
colorID = 0
markerID = 2

for t in typesele:
    allslowtime = []
    allpeakVdiff = []
    allpeakVdifferr = []
    allpeakVdiffmean = []
    
    allslowtime2 = []
    allpeakVdiff2 = []
    allpeakVdifferr2 = []
    
    for s in Subfolders[t]:
        allslowtime.append(AllSlowTimes[t][s])
        allpeakVdiff.append(AllPeakVDiffs[t][s])
        allpeakVdifferr.append(AllPeakVDifferrs[t][s])
        allpeakVdiffmean.append(AllPeakVDiffmeans[t][s])
        
        for i in range(0, len(AllSlowTimes[t][s])):
            if AllSlowTimes[t][s][i] < 10:
                allslowtime2.append(AllSlowTimes[t][s][i])
                allpeakVdiff2.append(AllPeakVDiffs[t][s][i])
                allpeakVdifferr2.append(AllPeakVDifferrs[t][s][i])
        
        plt.plot(AllSlowTimes[t][s], AllPeakVDiffs[t][s], markers[markerID], markersize=10,\
                 color=colors[colorID])#, label=t+", "+s)
        plt.errorbar(AllSlowTimes[t][s], AllPeakVDiffs[t][s],\
                     yerr=AllPeakVDifferrs[t][s], fmt=' ', capsize=5,\
                         color=colors[colorID])
            
        markerID += 1
        if markerID == 1:
            markerID += 1
    
    allslowtime = np.array(tools.flattenList(allslowtime))
    allpeakVdiff = np.array(tools.flattenList(allpeakVdiff))
    allpeakVdifferr = np.array(tools.flattenList(allpeakVdifferr))
    allpeakVdiffmean = np.array(tools.flattenList(allpeakVdiffmean))
    
    fit, cov = curve_fit(tools.exp_decay, allslowtime, allpeakVdiff, p0=[5., 10., -10.],\
                         sigma=allpeakVdifferr, absolute_sigma=True)
    err = np.sqrt(np.diag(cov))
    
    stspan = np.arange(0., 18., 0.1)
    plt.plot(stspan, tools.exp_decay(stspan, *fit), color=colors[colorID], \
             label="With "+t)
    #print(fit, err)
    print("With " + t + ", maximum reduction in velocity is\
          %.3g +- %.2g m/s per ms \n"%(fit[2], err[2]))
    
    # allslowtime2 = np.array(allslowtime2)
    # allpeakVdiff2 = np.array(allpeakVdiff2)
    # allpeakVdifferr2 = np.array(allpeakVdifferr2)
    
    # fit2, cov2 = curve_fit(tools.Line, allslowtime2, allpeakVdiff2, p0=[-0.5, 0.],\
    #                      bounds=([-np.inf, -0.01], [np.inf, 0.01]))#, \
    #                        #  sigma=allpeakVdifferr2, absolute_sigma=True)
    # err2 = np.sqrt(np.diag(cov2))
    # plt.plot(stspan, tools.Line(stspan, *fit2), '-.', color=colors[colorID+1], \
    #          label="With "+t + ", <10ms")
    # #print(fit, err)
    # print("With " + t + ", <10ms slowing rate is %.3g +- %.2g m/s per ms \n"%(fit2[0], err2[0]))
    
    colorID +=1

plt.plot(stspan, tools.Line(stspan, -1., 0.), '-.', color="red", label="1000 m/s^2")

plt.xlabel("Slowing time (ms)")
plt.ylabel("Change in peak velocity (m/s)")
plt.title("White light slowing")
plt.ylim(-20, 1.5)
plt.legend(loc="upper right")#, bbox_to_anchor=(1.3, 1.))
plt.show()
