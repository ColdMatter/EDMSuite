# -*- coding: utf-8 -*-
"""
Created on Tue Oct 20 2026
Based on Bulk analysis scripts/Slowing 4shot Bulk analysis for July 30 2026 data.py

TOF collector analysis, with slowing ON-OFF shots (or B-field up/down)
For 4-shot patterns.
Assuming 2 detectors -- PMT & Photodiode (PD)
- Data saved as 4 "detectors". Each have their own ON-OFF shots
- Detector 1: PMT, YAG ON
- Detector 2: PMT, YAG OFF
- Detector 3: PD, YAG ON
- Detector 4: PD, YAG OFF
Batch analysis: all selected files in a folder are processed in turn.

X axis is the WM frequency when the WM recorded the probe/V0 (fTHz),
otherwise (or with UseSetpoint = True) the TCL setpoint in V. Setpoints can be
converted to velocity with CalTCLconv and CalRestSP once the calibration is known.

@author: sl5119
"""

#%% Import libraries
import sys
import os
import re

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
import LatticeEDM_analysis_library as EDM

import numpy as np

import glob
import matplotlib.pyplot as plt
from scipy.optimize import curve_fit

import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

#%% Load data
###When we were using OneDrive:
#datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
#month="September2025"
#date="29"
#subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

###When we are using Box:
if sys.platform == "win32":
    datadrive = r"C:\Users\sl5119\Box\LatticeEDM\data"
else:
    datadrive = os.path.expanduser("~/Library/CloudStorage/Box-Box/LatticeEDM/data")  # TODO: set Mac path
month = "Oct 2026"
date = "20"
#blockdrive=datadrive+"\\BlockData\\"

drive = os.path.join(datadrive, month, date, "")# + subfolder
print(drive)

pattern="*V0setpointScan*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%% Selection
sele = [] # leave empty, sele = [], to load all matching files

#%%
LoadPasses = True

if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    if len(sele) == 0:
        # No selection: select every matching file by its ID
        sele = sorted(set(re.split(r'[_]', os.path.basename(f))[0] for f in files))
        print("No selection - loading all matching files: ", sele)
    Data = {}
    fileLabels = []
    Lasers = []
    for i in range(0, len(files)):
        fileLabel = re.split(r'[_]', os.path.basename(files[i]))[0]
        Laser = re.split(r'[.]', re.split(r'[_]', os.path.basename(files[i]))[-2])[0]

       #Use this part if have selections
        for j in range(0, len(sele)):
            if fileLabel == sele[j]:
                print("File "+fileLabel+" selected")
       ###
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

#%% Analysis settings
"""Can also read from scan settings (optional, for later)"""
SigStart = 30 # in ms
SigEnd = 42

#Use YAG-OFF shots for background subtraction

angle = 45 # Angle of probe to slowing
Resonance = 542.809124 #in THz, rest frame probe frequency

showTOF = False # If true, plots an example TOF before and after bkg subtraction
showIntermediate = True # If true, show unnormalised data with frequency/setpoint axis
showProcessed = True # If true, show final processed plot with fit

WeightedFit = True # If true, Gaussian fits of averaged scans are weighted by their standard error
                    # (only when more than 2 scans are averaged; others are fitted unweighted)

fTHz = 542

point = 1 # This is for TOF check

# X axis: WM frequency (default) or the TCL setpoint (ScanParameter, in V)
UseSetpoint = True  # True: ignore the WM and process everything on TCL setpoints

# Optional setpoint calibration, used only for files without a valid WM reading
# (UseSetpoint = True or wrong fibre). Leave as None to keep the results in volts.
# CalTCLconv alone converts the peak shifts to m/s (x axis stays in V);
# with CalRestSP as well, the x axis is converted to velocity too.
CalTCLconv = -133.9  # TCL conversion, MHz/V
CalTCLconverr = 6.5   # MHz/V
CalRestSP = -0.338     # zero-velocity setpoint, V
wavelength = 552     # nm

#%% Fitting functions, used for single scans and for averaged groups of scans
def XAxisInfo(InVolts):
    '''Unit, name, axis label and initial FWHM guess of the x axis'''
    if InVolts:
        return "V", "setpoint", "TCL setpoint (V)", 0.03 # V
    else:
        return "m/s", "velocity", "Velocity (m/s)", 20. # m/s

def FitSlowing(x, OnMeanCounts, OffMeanCounts, InVolts, tSlow, name, show=True,\
               FromSetpoint=False, OnErr=None, OffErr=None, AllowWeighted=True):
    '''Fit the slowing On and Off gated TOFs with Gaussians, normalise to the
       amplitude of the Off fit, fit again and get the peak shift (On-Off).
       FromSetpoint: x is velocity converted from the setpoint with CalTCLconv,
       so the calibration error is added to the peak shift error.
       OnErr, OffErr: optional errors of the gated TOFs at each point (e.g. the
       standard error of averaged scans), shown as shaded regions on the plot.
       AllowWeighted: False forces an unweighted fit even if WeightedFit is True.
       Returns a dict of results, or None if a fit fails.'''
    xunit, xname, xlabel, width0 = XAxisInfo(InVolts)

    peakOn = x[np.where(OnMeanCounts == np.max(OnMeanCounts))[0][0]]
    peakOff = x[np.where(OffMeanCounts == np.max(OffMeanCounts))[0][0]]

    # Weighted fit only if switched on and there are usable (all non-zero) errors
    Weighted = WeightedFit and AllowWeighted and OnErr is not None and OffErr is not None
    if Weighted and (np.any(np.asarray(OnErr) <= 0) or np.any(np.asarray(OffErr) <= 0)):
        print(name + ": some errors are zero - fitted unweighted")
        Weighted = False
    if Weighted:
        print(name + ": weighted fit")

    try:
        fitOn, covOn = curve_fit(tools.Gaussian_FWHM, x, OnMeanCounts, \
                                 p0=[200., peakOn, width0, 100.],\
                                 sigma=OnErr if Weighted else None, absolute_sigma=Weighted)
        errOn = np.sqrt(np.diag(covOn))

        fitOff, covOff = curve_fit(tools.Gaussian_FWHM, x, OffMeanCounts, \
                                 p0=[200., peakOff, width0, 100.],\
                                 sigma=OffErr if Weighted else None, absolute_sigma=Weighted)
        errOff = np.sqrt(np.diag(covOff))

        # Normalise to amplitude of Off fit & correct shift to 0
        NormOff = (OffMeanCounts - fitOff[3])/fitOff[0]
        NormOn = (OnMeanCounts - fitOn[3])/fitOff[0]

        fitOnNorm, covOnNorm = curve_fit(tools.Gaussian_FWHM, x, NormOn, \
                                 p0=[0.2, peakOn, width0, 0.],\
                                 sigma=np.abs(OnErr/fitOff[0]) if Weighted else None,\
                                 absolute_sigma=Weighted)
        errOnNorm = np.sqrt(np.diag(covOnNorm))

        fitOffNorm, covOffNorm = curve_fit(tools.Gaussian_FWHM, x, NormOff, \
                                 p0=[0.2, peakOff, width0, 0.],\
                                 sigma=np.abs(OffErr/fitOff[0]) if Weighted else None,\
                                 absolute_sigma=Weighted)
        errOffNorm = np.sqrt(np.diag(covOffNorm))
    except RuntimeError as e:   # fit did not converge
        print(name + ": fit failed - " + str(e))
        return None

    print("Central ON %s = %.4g +- %.3g %s"%(xname, fitOnNorm[1], errOnNorm[1], xunit) +\
          "\n with FWHM = %.4g +- %.3g %s"%(fitOnNorm[2], errOnNorm[2], xunit))
    print("Central OFF %s = %.4g +- %.3g %s"%(xname, fitOffNorm[1], errOffNorm[1], xunit) +\
          "\n with FWHM = %.4g +- %.3g %s"%(fitOffNorm[2], errOffNorm[2], xunit))

    if InVolts:
        xspan = np.linspace(np.min(x), np.max(x), 1000)
    else:
        xspan = np.arange(np.min(x), np.max(x), 0.1)

    peakShift = fitOnNorm[1] - fitOffNorm[1]
    # Fit errors of the two peak centres in quadrature
    peakShifterr = np.sqrt(errOnNorm[1]**2 + errOffNorm[1]**2)
    if FromSetpoint and CalTCLconv is not None:
        # The TCL calibration scales the whole shift
        peakShifterr = np.sqrt(peakShifterr**2 +\
                               (peakShift * CalTCLconverr / CalTCLconv)**2)
    print("Peak shift (On-Off) = %.4g +- %.3g %s"%(peakShift, peakShifterr, xunit))

    # A shift only needs the TCL conversion, not the zero-velocity setpoint
    shiftUnit = xunit
    if InVolts and CalTCLconv is not None:
        fac = -1 / np.cos(angle*np.pi/180) * wavelength * 1e-3 # (m/s)/MHz, same as EDM.VelocityfromSetpoint
        peakShiftV, peakShiftVerr = peakShift, peakShifterr
        peakShift = peakShiftV * CalTCLconv * fac
        peakShifterr = np.sqrt((peakShiftVerr * CalTCLconv * fac)**2 +\
                               (peakShiftV * CalTCLconverr * fac)**2)
        shiftUnit = "m/s"
        print("Velocity peak shift = %.4g +- %.3g m/s (%.4g MHz/V)"%(peakShift, peakShifterr, CalTCLconv))

    title2 = str(tSlow) + \
        " ms slowing with Gated TOF normalised over off-shot fit amplitude,\n " +\
        str(SigStart) + "ms to " + str(SigEnd) + "ms gate,\n " +\
            name + ", %.3g +- %.2g %s peak shift"%(peakShift, peakShifterr, shiftUnit)

    if show:
        # Errors are normalised the same way as the data (by the Off fit amplitude)
        if OffErr is not None:
            plt.fill_between(x, NormOff - OffErr/fitOff[0], NormOff + OffErr/fitOff[0],\
                             color=colors[0], alpha=0.3)
        if OnErr is not None:
            plt.fill_between(x, NormOn - OnErr/fitOff[0], NormOn + OnErr/fitOff[0],\
                             color=colors[1], alpha=0.3)
        plt.plot(x, NormOff, '.', color=colors[0], label="Slowing Off")
        plt.plot(xspan, tools.Gaussian_FWHM(xspan, *fitOffNorm), color=colors[0])
        plt.plot(x, NormOn, '.', color=colors[1], label="Slowing On")
        plt.plot(xspan, tools.Gaussian_FWHM(xspan, *fitOnNorm), color=colors[1])

        plt.xlabel(xlabel)
        plt.ylabel("Normalised Gated TOF")
        plt.title(title2)
        plt.legend()
        plt.show()

    fit_results_ON = {"Variables":["amplitude", "mean", "FWHM", "shift"],
                   "best fit":fitOnNorm, "error":errOnNorm}
    fit_results_OFF = {"Variables":["amplitude", "mean", "FWHM", "shift"],
                   "best fit":fitOffNorm, "error":errOffNorm}

    return {"fit ON":fit_results_ON, "fit OFF":fit_results_OFF,
            "peak shift":[peakShift, peakShifterr],
            "x unit":xunit, "shift unit":shiftUnit}

#%%
FitResNormON = {}
FitResNormOFF = {}
PeakShifts = {}
SlowTimes = {}
XAxis = {}      # x values of each file (velocity in m/s, or setpoint in V)
XUnits = {}     # "m/s" or "V" for each file
ShiftUnits = {} # unit of PeakShifts for each file ("m/s" if converted with CalTCLconv)
FailedFits = []

# Gated TOFs of each scan with their x axis, kept so scans can be averaged before fitting.
# GatedTOFs[label] = {"On": slowing-on gated TOF, "Off": slowing-off gated TOF,
#                     "x": x axis (velocity in m/s, or setpoint in V), "x unit": "m/s" or "V",
#                     "setpoint": TCL setpoint (ScanParameter, V), "slowing time": ms}
GatedTOFs = {}

#%% Process all
for f in range(0, len(fileLabels)):
    Scan = Data[fileLabels[f]]

    #% Print out all params
    Settings = EDM.GetScanSettings(Scan)
    ScanParams = EDM.GetScanParameterArray(Scan)
    print("\nfor file " + fileLabels[f])

    if Settings["slowing time"] is None:
        tSlow = np.nan
        print("WARNING: no slowing duration saved for this file (" +\
              str(Settings["pattern plugin"]) + ")")
    else:
        tSlow = Settings["slowing time"]/1000 # in ms
    SlowTimes[fileLabels[f]] = tSlow
    print("Slowing duration %g ms"%tSlow)

# Get data, individual shots
    TimeOn, DataOn, TimeOff, DataOff = EDM.GetTOFs(Scan)
    if len(DataOff) == 0 or len(DataOn) < 2:
        print("Skipped: not 4-shot ON/OFF data (" + str(Settings["pattern plugin"]) + ")")
        FailedFits.append(fileLabels[f])
        continue

# For checking
    title = "TOF at point %g for shot #"%point

    if showTOF:
        OnTOFbyShot = []
        OffTOFbyShot = []

        for i in range(0, len(DataOn)):
            OnTOFbyShot.append(DataOn[i][point])
            OffTOFbyShot.append(DataOff[i][point])

        for i in range(0, len(OnTOFbyShot)):
            plt.plot(TimeOn*1000, OnTOFbyShot[i], label='On')
            plt.plot(TimeOff*1000, OffTOFbyShot[i], label='Off')
            plt.title(title + str(i) + ", file " + fileLabels[f])
            plt.xlabel("time (ms)")
            plt.ylabel("Signal (V)")
            plt.legend()
            plt.show()

# Grouping by ON/OFF and detector
    PMTOnYAGOns = DataOn[0]
    PMTOnYAGOffs = DataOn[1]

    PMTOffYAGOns = DataOff[0]
    PMTOffYAGOffs = DataOff[1]

    #% YAG On-Off subtractions
    SPP = Settings['shotsPerPoint']

    PMTOns = PMTOnYAGOns - PMTOnYAGOffs
    PMTOffs = PMTOffYAGOns - PMTOffYAGOffs

# check 2
    if showTOF:
        plt.plot(TimeOn*1000, PMTOnYAGOns[point]-PMTOnYAGOffs[point], label='On')
        plt.plot(TimeOff*1000, PMTOffYAGOns[point]-PMTOffYAGOffs[point], label='Off')
        plt.title(title + "\n YAG ON-OFF, PMT, file " + fileLabels[f])
        plt.xlabel("time (ms)")
        plt.ylabel("Signal (V)")
        plt.legend()
        plt.show()

# Get gated TOFs
    OnMeanCounts = EDM.GetGatedAvgCounts4Shot(Scan,\
                                        PMTOns,TimeOn,SigStart,SigEnd)
    OffMeanCounts = EDM.GetGatedAvgCounts4Shot(Scan,\
                                        PMTOffs,TimeOff,SigStart,SigEnd)

# Get x axis (setpoint or WM reading)
    f_iniTHz, f_relMHz = EDM.GetScanFreqArrayMHz(Scan)

    if UseSetpoint:
        HasWM = False
        print("Using TCL setpoints (V) as the x axis, WM ignored.")
    elif int(f_iniTHz) == fTHz:
        TCL_WM_cali = EDM.TCL_WM_Calibration(Scan, plot=showIntermediate, Toprint=False)
        HasWM = True
        TCLconv = TCL_WM_cali['best fit'][0]
        TCLconverr = TCL_WM_cali['error'][0]
        print("TCL calibration = %.4g +- %.2g MHz"%(TCLconv, TCLconverr))
    else:
        HasWM = False
        print("Wrong fibre in WM (%.7g THz). Using TCL setpoints (V) as the x axis."%f_iniTHz)

# Plot
    title="Gated TOF over " + str(Settings["param"]) + " with " +\
        str(Settings["shotsPerPoint"]) + " shots per point \n from " +\
        str(SigStart) + "ms to " + str(SigEnd) + "ms gate, file " +\
            fileLabels[f]

    if showIntermediate:
        if HasWM:
            plt.plot(f_relMHz, OffMeanCounts, label="Slowing Off")
            plt.plot(f_relMHz, OnMeanCounts, label="Slowing On")

            plt.xlabel("Relative frequency (MHz) to %g THz"%f_iniTHz)

        else:
            plt.plot(ScanParams, OffMeanCounts, label="Slowing Off")
            plt.plot(ScanParams, OnMeanCounts, label="Slowing On")

            plt.xlabel("Setpoint (V)")

        plt.ylabel("Gated TOF (ms.V)")
        plt.title(title)
        plt.legend()
        plt.show()

# For slowing measurements, convert X-axis to velocity
    if HasWM:
        f_relRes = (f_relMHz/1e6 + f_iniTHz - Resonance) * 1e6
        x = tools.VelocityfromFshift(f_relRes, Resonance, angle)
        InVolts = False
        FromSetpoint = False
    elif CalTCLconv is not None and CalRestSP is not None:
        x, _ = EDM.VelocityfromSetpoint(ScanParams, 0, CalRestSP, 0,\
                                        CalTCLconv, 0, wavelength, angle)
        InVolts = False
        FromSetpoint = True
    else:
        x = ScanParams
        InVolts = True
        FromSetpoint = False

    xunit, xname, xlabel, width0 = XAxisInfo(InVolts)

    if showIntermediate and not InVolts:
        plt.plot(x, OffMeanCounts, label="Slowing Off")
        plt.plot(x, OnMeanCounts, label="Slowing On")

        plt.xlabel(xlabel)
        plt.ylabel("Gated TOF (ms.V)")
        plt.title(title)
        plt.legend()
        plt.show()

# Keep the gated TOFs so scans can be averaged before fitting later
    GatedTOFs[fileLabels[f]] = {"On":OnMeanCounts, "Off":OffMeanCounts,
                                "x":x, "x unit":xunit, "setpoint":ScanParams,
                                "slowing time":tSlow, "from setpoint":FromSetpoint}
    XAxis[fileLabels[f]] = x
    XUnits[fileLabels[f]] = xunit

# Normalise & fit
    res = FitSlowing(x, OnMeanCounts, OffMeanCounts, InVolts, tSlow,\
                     "file " + fileLabels[f], show=showProcessed,\
                     FromSetpoint=FromSetpoint)
    if res is None:
        FailedFits.append(fileLabels[f])
        continue

    FitResNormON[fileLabels[f]] = res["fit ON"]
    FitResNormOFF[fileLabels[f]] = res["fit OFF"]
    PeakShifts[fileLabels[f]] = res["peak shift"]
    ShiftUnits[fileLabels[f]] = res["shift unit"]

if len(FailedFits) > 0:
    print("\nFits failed for: ", FailedFits)

#%% Summary table
print("File   slowing (ms)   peak shift")
for f in PeakShifts.keys():
    print("%-6s %8.3g       %.4g +- %.3g %s"%(f, SlowTimes[f], PeakShifts[f][0],\
                                          PeakShifts[f][1], ShiftUnits[f]))

#%% Summary -- slowing time vs peak shift
'''Group files by condition. Scans of a file are matched by the first
   3 characters of the label, so with LoadPasses every pass is included.
   File IDs in the nested list means they have the same slowing duration.'''
types = {
         "MW + 4fv0": [["004", "005"]],
         "MW + 4fv0v1": [["008", "010"], ["012"], ["013"], ["014"]], #012, 013 are >10ms
         "4fv0v1 only": [["009"], ["011"]],
         "MW only": [["015"], ["016"], ["001", "002"], ["018"], ["020"], ["021"]] #018, 021 are >10ms
        }

FitExcludeTypes = ["MW repumps only", "MW + 4fv0"]

ExcludeLabels = ["010_1"] # bad scans to discard manually, e.g. ["009_0"] for the first pass of file 009

FitLine = True
WeightedLineFit = True # If true, line fits are weighted by the peak shift error bars
FixOrigin = True # If true, line fits go through 0 (no peak shift at 0 ms slowing); only the slope is fitted
sSpan = np.arange(0., 18, 0.1)

keys = list(types.keys())
units = set()

MarkerSize = 14
ErrorbarWidth = 2

GroupFits = {} # fit results of the averaged groups, keyed by type and file IDs
SummaryPoints = {} # plotted points (and line fit) of each type

def ScansOf(fileIDs, typeName):
    '''Labels of all scans (incl. passes) of the given file IDs that have
       gated TOFs, minus ExcludeLabels'''
    scans = []
    for k in fileIDs:
        for f in GatedTOFs.keys():
            if f[:3] == k:
                if f in ExcludeLabels:
                    print("Excluding " + f + " from " + typeName)
                    continue
                scans.append(f)
    return scans

for i in range(0, len(keys)):
    X = []
    Y = []
    Yerr = []
    for k in types[keys[i]]:
        if isinstance(k, list):
            # Nested list: files with the same slowing duration. Average their
            # gated TOFs point by point, then fit the averaged curves.
            scans = ScansOf(k, keys[i])
            if len(scans) == 0:
                continue
            times = np.array([GatedTOFs[f]["slowing time"] for f in scans])
            if np.ptp(times) > 0:
                print("WARNING: " + keys[i] + ", " + str(k) +\
                      " have different slowing durations: " + str(times))
            # Sort every scan by setpoint: in "updown" scan mode every other pass is reversed
            Sorted = {}
            for f in scans:
                order = np.argsort(GatedTOFs[f]["setpoint"])
                Sorted[f] = {key:np.asarray(GatedTOFs[f][key])[order] for key in\
                             ["On", "Off", "x", "setpoint"]}
                Sorted[f]["x unit"] = GatedTOFs[f]["x unit"]

            sp0 = Sorted[scans[0]]["setpoint"]
            if any(len(Sorted[f]["setpoint"]) != len(sp0) or\
                   not np.allclose(Sorted[f]["setpoint"], sp0) for f in scans):
                print("WARNING: " + keys[i] + ", " + str(scans) +\
                      " were not scanned over the same setpoints - not averaged, skipped")
                continue
            if len(set(GatedTOFs[f]["x unit"] for f in scans)) > 1:
                print("WARNING: " + keys[i] + ", " + str(scans) +\
                      " mix velocity and setpoint x axes - not averaged, skipped")
                continue

            avgOn = np.mean([Sorted[f]["On"] for f in scans], axis=0)
            avgOff = np.mean([Sorted[f]["Off"] for f in scans], axis=0)
            avgX = np.mean([Sorted[f]["x"] for f in scans], axis=0)
            InVolts = GatedTOFs[scans[0]]["x unit"] == "V"
            # Standard error of the averaged scans at each point, for the shaded regions
            if len(scans) > 1:
                errOnAvg = np.std([Sorted[f]["On"] for f in scans], axis=0, ddof=1) / np.sqrt(len(scans))
                errOffAvg = np.std([Sorted[f]["Off"] for f in scans], axis=0, ddof=1) / np.sqrt(len(scans))
            else:
                errOnAvg = None
                errOffAvg = None
            # The standard error of 2 or fewer scans is too uncertain to weight a fit with
            AllowWeighted = len(scans) > 2
            if WeightedFit and not AllowWeighted:
                print(keys[i] + ", " + str(scans) + ": only %g scans averaged - fitted unweighted"%len(scans))

            name = keys[i] + ", average of scans in" + str(k)
            print("\n" + name)
            res = FitSlowing(avgX, avgOn, avgOff, InVolts, np.mean(times),\
                             name, show=showProcessed,\
                             FromSetpoint=GatedTOFs[scans[0]]["from setpoint"],\
                             OnErr=errOnAvg, OffErr=errOffAvg,\
                             AllowWeighted=AllowWeighted)
            if res is None:
                continue
            GroupFits[keys[i] + " " + "+".join(k)] = res

            X.append(np.mean(times))
            Y.append(res["peak shift"][0])
            Yerr.append(res["peak shift"][1])
            units.add(res["shift unit"])
            groupUnit = res["shift unit"]
        else:
            scans = [f for f in ScansOf([k], keys[i]) if f in PeakShifts]  # individual fit must have worked
            for f in scans:
                X.append(SlowTimes[f])
                Y.append(PeakShifts[f][0])
                Yerr.append(PeakShifts[f][1])
                units.add(ShiftUnits[f])
                groupUnit = ShiftUnits[f]

    if len(X) == 0:
        print(keys[i] + ": no fitted scans - skipped")
        continue

    SummaryPoints[keys[i]] = {"X":np.array(X), "Y":np.array(Y),\
                              "Yerr":np.abs(np.array(Yerr)), "unit":groupUnit}

# Plot only after all types are processed: the group fits above show their
# own figures, which would otherwise close the summary figure part-way
for i in range(0, len(keys)):
    if keys[i] not in SummaryPoints:
        continue
    X = SummaryPoints[keys[i]]["X"]
    Y = SummaryPoints[keys[i]]["Y"]
    Yerr = SummaryPoints[keys[i]]["Yerr"]

    label = keys[i]
    # Through the origin, one non-zero slowing duration is enough to fit a slope
    EnoughPoints = np.any(X != 0) if FixOrigin else len(np.unique(X)) > 1
    if FitLine and keys[i] not in FitExcludeTypes and EnoughPoints:
        if FixOrigin:
            LineFunc = lambda x, a: a*x
            p0Line = [-1.]
        else:
            LineFunc = tools.Line
            p0Line = [-1., 0.]
        if WeightedLineFit and np.all(Yerr > 0):
            fit, cov = curve_fit(LineFunc, X, Y, p0=p0Line,\
                                 sigma=Yerr, absolute_sigma=True)
        else:
            if WeightedLineFit:
                print(keys[i] + ": some error bars are zero - line fitted unweighted")
            fit, cov = curve_fit(LineFunc, X, Y, p0=p0Line)
        err = np.sqrt(np.diag(cov))
        if FixOrigin:
            # Store as [slope, intercept] like the free fit, with the intercept fixed at 0
            fit = np.array([fit[0], 0.])
            err = np.array([err[0], 0.])
        label = label + ", slope=%.3g+-%.2g %s/ms"%(fit[0], err[0],\
                                                    SummaryPoints[keys[i]]["unit"])
        SummaryPoints[keys[i]]["line fit"] = {"best fit":fit, "error":err}
        plt.plot(sSpan, tools.Line(sSpan, *fit), color=colors[i])

    plt.plot(X, Y, '.', color=colors[i], markersize=MarkerSize, label=label)
    plt.errorbar(X, Y, yerr=Yerr, color=colors[i], fmt=" ", capsize=5,\
                 elinewidth=ErrorbarWidth, capthick=ErrorbarWidth)

if len(units) > 1:
    print("WARNING: groups mix velocity (m/s) and setpoint (V) results.")

plt.xlabel("Slowing duration (ms)")
plt.ylabel("Peak shift (%s)"%"/".join(sorted(units)))
plt.ylim(-30, 0)
plt.title(month + " " + date + " slowing summary, \n Gate: "+\
          str(SigStart) + "ms to " + str(SigEnd) + "ms")
plt.legend(bbox_to_anchor=(1, 1))
plt.show()

#%% Compare normalised gated TOFs of chosen files
'''Normalised by the off-shot's amplitudes to correct for source fluctuation.
   Off-shot fits are dashed black lines.'''
compare = {"009": "009", "010": "010"}   # file label : legend label

for i, s in enumerate(compare.keys()):
    if s not in FitResNormON:
        print(s + ": no fit results - skipped")
        continue
    fitON = FitResNormON[s]["best fit"]
    fitOFF = FitResNormOFF[s]["best fit"]
    xspan = np.linspace(np.min(XAxis[s]), np.max(XAxis[s]), 1000)

    plt.plot(xspan, tools.Gaussian_FWHM(xspan, *fitON), color=colors[i], \
             label = compare[s] + ", %g ms"%SlowTimes[s])
    plt.plot(xspan, tools.Gaussian_FWHM(xspan, *fitOFF),\
             '-.', color='black')

plt.xlabel("Velocity (m/s)" if XUnits.get(s) == "m/s" else "TCL setpoint (V)")
plt.ylabel("Normalised gated TOF")
plt.title("Normalised gated TOFs, accounting for source fluctuation.")
plt.legend()
plt.show()
