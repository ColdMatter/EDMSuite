#Modified from uedm_analysis_library.py

#% uedm_analysis_libary.py sets up the connection to the SharedCode library and 
# provides a set of analysis functions for data from the ultracold EDM experiment

# import pythonnet
import clr
import sys
import os
from System.IO import Path
import numpy as np
import git
from matplotlib import pyplot as plt
import pandas as pd
import matplotlib.dates as mdates
import time
import glob
from matplotlib.pyplot import cm
from scipy.special import erfcinv
import datetime

repo = git.Repo(os.path.dirname(os.path.abspath(__file__)), search_parent_directories=True)
RootFolder = repo.working_tree_dir

# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")


# Import the SharedCode DLLs, assumes you are executing this function from within
# the EDMSuite Git repository
clr.AddReference(Path.GetFullPath(RootFolder + r"\SEDM4\Libraries\SharedCode.dll"))
sys.path.append(RootFolder + r"\LatticeEDMScripts")
import tools as tools
import System
import Data

import copy
from scipy.optimize import curve_fit


#%% Functions related to Scan objects
def GetScanParameterArray(Scan):
    return np.array(Scan.ScanParameterArray)

def GetScanFreqArrayTHz(Scan):
    pointsPerScan = Scan.ScanSettings["out:pointsPerScan"]
    offset = Scan.ScanSettings["analog:offset"]
    f=[]
    
    for i in range(0, pointsPerScan):
        f.append(Scan.Points[i].Analogs[0])
        
    return np.array(f)+offset

def GetScanFreqArrayMHz(Scan):
    fTHz = GetScanFreqArrayTHz(Scan)
    
    f_ini = fTHz[0]
    f_relMHz = (fTHz - f_ini) * 1e6
    
    return f_ini, f_relMHz

def GetScanSettings(Scan):
    """Extract scan settings"""
    pointsPerScan = Scan.ScanSettings["out:pointsPerScan"]
    ScanStart = Scan.ScanSettings["out:start"]
    ScanEnd = Scan.ScanSettings["out:end"]
    channel = Scan.ScanSettings["out:channel"]
    param = Scan.ScanSettings["out:scannedParameter"]
    clockFreq = Scan.ScanSettings["pg:clockFrequency"]
    sampleRate = Scan.ScanSettings["shot:sampleRate"]
    shotsPerPoint = Scan.ScanSettings["out:shotsPerPoint"]
    V0time = Scan.ScanSettings["pg:DurationV0"]
    detectors = Scan.ScanSettings["shot:channel"]
    
    Tconv = 1/sampleRate * 1000 #convert to ms
    
    Settings = {"pointsPerScan":pointsPerScan, "start":ScanStart, \
                             "end":ScanEnd, "channel":channel, "param":param, \
                            "clockFreq":clockFreq, "sampleRate":sampleRate, \
                            "Tconv":Tconv, "shotsPerPoint":shotsPerPoint, \
                            "slowing time":V0time, "detectors":detectors}
    
    return Settings

def ReadAverageScanInZippedXML(Filename):
    ss = Data.Scans.ScanSerializer()
    Scan = ss.DeserializeScanFromZippedXML(Filename,"average.xml")
    # TODO: Adjust for zip-files with multiple passes
    return Scan

def GetTOFs(Scan):
    """Returns the TOFs of a scan. The datasets are On/Off shots and for each 
    detector (TOFs in one shot)"""
    SampleRate = GetTOFSampleRate(Scan)
    NrScanPoints = len(Scan.Points)
    NrOnShotsPerPoint = len(Scan.Points[0].OnShots)
    NrOffShotsPerPoint = len(Scan.Points[0].OffShots)
    NrTOFs = NrOnShotsPerPoint*NrScanPoints
    NrDetectors = len(Scan.Points[0].OnShots[0].TOFs)
    TimeOn = np.array(Scan.Points[0].OnShots[0].TOFs[0].Times)/SampleRate
    DataOn = np.empty((NrDetectors, NrTOFs, TimeOn.shape[0]))*np.nan
    Index = 0
    for indPoint in range(NrScanPoints):
        for indOn in range(NrOnShotsPerPoint):
            for indDet in range(NrDetectors):
                Data = np.array(Scan.Points[indPoint].OnShots[indOn].TOFs[indDet].Data)
                DataOn[indDet, Index, :] = Data
            Index = Index+1

    if NrOffShotsPerPoint == 0:
        TimeOff = []
        DataOff = []
    else:
        Index = 0
        TimeOff = np.array(Scan.Points[0].OffShots[0].TOFs[0].Times)/SampleRate
        DataOff = np.empty((NrDetectors, NrTOFs, TimeOff.shape[0]))*np.nan
        for indPoint in range(NrScanPoints):
            for indOn in range(NrOffShotsPerPoint):
                for indDet in range(NrDetectors):
                    Data = np.array(Scan.Points[indPoint].OffShots[indOn].TOFs[indDet].Data)
                    DataOff[indDet, Index, :] = Data
                Index = Index+1

    return TimeOn, DataOn, TimeOff, DataOff

def GetTOFsSPP(Scan):
    """Returns the TOFs of a scan. The datasets are On/Off shots and for each 
    detector (TOFs in one shot). Grouped shots of the same scan points together."""
    SampleRate = GetTOFSampleRate(Scan)
    NrScanPoints = len(Scan.Points)
    NrOnShotsPerPoint = len(Scan.Points[0].OnShots)
    NrOffShotsPerPoint = len(Scan.Points[0].OffShots)
    NrTOFs = NrOnShotsPerPoint*NrScanPoints
    NrDetectors = len(Scan.Points[0].OnShots[0].TOFs)
    TimeOn = np.array(Scan.Points[0].OnShots[0].TOFs[0].Times)/SampleRate
    DataOn = np.empty((NrDetectors, NrScanPoints, TimeOn.shape[0], NrOnShotsPerPoint))*np.nan
    Index = 0
    for indPoint in range(NrScanPoints):
        for indOn in range(NrOnShotsPerPoint):
            for indDet in range(NrDetectors):
                Data = np.array(Scan.Points[indPoint].OnShots[indOn].TOFs[indDet].Data)
                DataOn[indDet, indPoint, :, indOn] = Data
            Index = Index+1

    if NrOffShotsPerPoint == 0:
        TimeOff = []
        DataOff = []
        print("This scan has no Off shots")
    else:
        Index = 0
        TimeOff = np.array(Scan.Points[0].OffShots[0].TOFs[0].Times)/SampleRate
        DataOff = np.empty((NrDetectors, NrScanPoints, TimeOn.shape[0], NrOnShotsPerPoint))*np.nan
        for indPoint in range(NrScanPoints):
            for indOn in range(NrOffShotsPerPoint):
                for indDet in range(NrDetectors):
                    Data = np.array(Scan.Points[indPoint].OffShots[indOn].TOFs[indDet].Data)
                    DataOff[indDet, indPoint, :, indOn] = Data
                Index = Index+1

    return TimeOn, DataOn, TimeOff, DataOff

def DownsampleTOFsInScan(Scan, DownSampleRate):
    """Returns the TOFs of a scan, but downsampled to reduce the memory usage.
    The datasets are On/Off shots and for each detector (TOFs in one shot)"""
    OriginalRate = GetTOFSampleRate(Scan)
    NrSamples = int(np.floor(OriginalRate/DownSampleRate))
    DownSampleRateReal = OriginalRate/NrSamples
    NrScanPoints = len(Scan.Points)
    NrOnShotsPerPoint = len(Scan.Points[0].OnShots)
    NrOffShotsPerPoint = len(Scan.Points[0].OffShots)
    NrTOFs = NrOnShotsPerPoint*NrScanPoints
    NrDetectors = len(Scan.Points[0].OnShots[0].TOFs)
    TimeOnOriginal = np.array(Scan.Points[0].OnShots[0].TOFs[0].Times)/OriginalRate
    TimeOn = TimeOnOriginal[int(NrSamples/2)::NrSamples]
    DataOn = np.empty((TimeOn.shape[0], NrTOFs, NrDetectors))*np.nan
    Index = 0
    for indScan in range(NrScanPoints):
        for indOn in range(NrOnShotsPerPoint):
            for indDet in range(NrDetectors):
                OriginalData = np.array(Scan.Points[indScan].OnShots[indOn].TOFs[indDet].Data)
                NrRows = int(np.shape(OriginalData)[0]/NrSamples)
                NrCols = NrSamples
                DownsampledData = OriginalData.reshape((NrRows,NrCols))
                DataOn[:,Index, indDet] = np.sum(DownsampledData, axis=1)
            Index = Index+1

    if NrOffShotsPerPoint == 0:
        TimeOff = []
        DataOff = []
    else:
        Index = 0
        TimeOffOriginal = np.array(Scan.Points[0].OffShots[0].TOFs[0].Times)/OriginalRate
        TimeOff = TimeOffOriginal[int(NrSamples/2)::NrSamples]
        DataOff = np.empty((TimeOff.shape[0], NrTOFs, NrDetectors))*np.nan
        for indScan in range(NrScanPoints):
            for indOn in range(NrOffShotsPerPoint):
                for indDet in range(NrDetectors):
                    OriginalData = np.array(Scan.Points[indScan].OffShots[indOn].TOFs[indDet].Data)
                    NrRows = int(np.shape(OriginalData)[0]/NrSamples)
                    NrCols = NrSamples
                    DownsampledData = OriginalData.reshape((NrRows,NrCols))
                    DataOff[:,Index, indDet] = np.sum(DownsampledData, axis=1)
                Index = Index+1

    return TimeOn, DataOn, TimeOff, DataOff

def GetScanParameterList(Scan):
    return list(Scan.ScanSettings.StringKeyList)

def GetTOFtimes(Scan):
    StartTime = Scan.GetSetting("shot","gateStartTime")/Scan.GetSetting("shot","sampleRate")
    Duration = Scan.GetSetting("shot","gateLength")/Scan.GetSetting("shot","sampleRate")
    EndTime = StartTime + Duration
    Scan.Points[0].OnShots[0].TOFs[0].Times


def GetTOFSampleRate(Scan):
    """GetTOFSampleRate(Scan) returns the sample rate used for the TOF spectrum.
    Scan is a Scan object from the SharedCode.dll, the sample rate is an integer
    with unit Hz.
    """
    return Scan.GetSetting("shot","sampleRate")

def GetShotTimestamps(Scan):
    NrShots = len(Scan.Points)
    TimeStamps = np.full((NrShots),np.nan)
    for i in range(NrShots):
        T = Scan.Points[i].OnShots[0].TimeStamp
        TimeStamps[i] = datetime.datetime.strptime(T.ToString("dd/MM/yyyy HH:mm:ss.ffffff+00:00"), '%d/%m/%Y %H:%M:%S.%f%z').timestamp()
    return TimeStamps
#%% Functions for the TOF
def BgSubTOF(Data,Time,StartBg,StopBg):
    Indi= (Time*1000>StartBg) & (Time*1000 < StopBg)
    BgMean=np.mean(Data[Indi,:,:,:],axis=0)
    return (Data-BgMean)

def GetCounts(Data,Time,Start,Stop):
    Indi = (Time*1000>Start) & (Time*1000 <Stop)
    IndiArray = np.where(Indi)[0]
    Counts = np.sum(Data[Indi,:,:], axis=0)
    TimeWindow = Time[IndiArray[-1]]-Time[IndiArray[0]]
    return Counts, TimeWindow

def GetCountsSPP(Data,Time,Start,Stop):
    Indi = (Time*1000>Start) & (Time*1000 <Stop)
    IndiArray = np.where(Indi)[0]
    RawCounts = np.sum(Data[Indi,:,:,:], axis=0)
    
    MeanCounts = np.mean(RawCounts,axis=1)
    StderrCounts = np.std(RawCounts,axis=1)/np.sqrt(RawCounts.shape[1])
    TimeWindow = Time[IndiArray[-1]]-Time[IndiArray[0]]
    return MeanCounts, StderrCounts, TimeWindow

def GetGatedAvgCounts(Scan, TOFData,Time,Start,Stop):
    "Make things simpler by just looking at one detector. "
    Indi = (Time*1000>Start) & (Time*1000 <Stop)
    IndiArray = np.where(Indi)[0]
    RawCounts = np.sum(TOFData[:,Indi,:], axis=1)
    
    Settings = GetScanSettings(Scan)
    Tconv = Settings["Tconv"]
    
    MeanCounts = np.mean(RawCounts,axis=1) * Tconv # convert to ms.V
    StderrCounts = np.std(RawCounts,axis=1)/np.sqrt(RawCounts.shape[1])
    TimeWindow = Time[IndiArray[-1]]-Time[IndiArray[0]]
    return MeanCounts, StderrCounts, TimeWindow

def PlotGatedAvgCounts(Scan,TOFData,Time,SigStart,SigStop,BkgStart,BkgStop,error=False, display=True):
    "Plot gated counts against scanned parameter, with background subtraction"
    MeanCounts, StderrCounts, TimeWindow = GetGatedAvgCounts(Scan,TOFData,Time,SigStart,SigStop)
    BkgMeanCounts, BkgStderrCounts, BkgTimeWindow = GetGatedAvgCounts(Scan,TOFData,Time,BkgStart,BkgStop)
    BkgSub = MeanCounts - BkgMeanCounts * TimeWindow/BkgTimeWindow
    
    #Scan params for plotting
    ScannedParam = np.array(Scan.ScanParameterArray)
    Settings = GetScanSettings(Scan)
    
    title = "Gated TOF over " + Settings["param"] + " with " + str(Settings["shotsPerPoint"]) \
        + " shots per point \n from " + str(SigStart) + "ms to " + str(SigStop) + "ms gate"
    if Settings["param"] == 'setpoint':
        Xunit = " (V)"
    else:
        Xunit = ""
    xlabel = Settings["channel"] + " " + Settings["param"] + Xunit
    ylabel = "Gated LIF (ms.V)"
    
    fig = plt.figure()
    plt.plot(ScannedParam, BkgSub, '.')
    if error==True:
        plt.errorbar(ScannedParam, BkgSub, yerr = StderrCounts, fmt=" ")
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    if display:
        plt.show()
    
    return fig

def PlotGatedAvgCountsOnOff(Scan,TOFDataOn,TOFDataOff,TimeOn,TimeOff,\
                SigStart,SigStop,BkgStart,BkgStop,error=False, display=True):
    "Plot gated counts against scanned parameter, with background subtraction"
    OnMeanCounts, OnStderrCounts, OnTimeWindow = GetGatedAvgCounts(Scan,\
                                        TOFDataOn,TimeOn,SigStart,SigStop)
    OnBkgMeanCounts, OnBkgStderrCounts, OnBkgTimeWindow = GetGatedAvgCounts(Scan,\
                                        TOFDataOn,TimeOn,BkgStart,BkgStop)
    OnBkgSub = OnMeanCounts - OnBkgMeanCounts * OnTimeWindow/OnBkgTimeWindow

    OffMeanCounts, OffStderrCounts, OffTimeWindow = GetGatedAvgCounts(Scan,\
                                        TOFDataOff,TimeOff,SigStart,SigStop)
    OffBkgMeanCounts, OffBkgStderrCounts, OffBkgTimeWindow = GetGatedAvgCounts(Scan,\
                                        TOFDataOff,TimeOff,BkgStart,BkgStop)
    OffBkgSub = OffMeanCounts - OffBkgMeanCounts * OffTimeWindow/OffBkgTimeWindow
    
    #Scan params for plotting
    ScannedParam = np.array(Scan.ScanParameterArray)
    Settings = GetScanSettings(Scan)
    
    title = "Gated TOF over " + Settings["param"] + " with " + str(Settings["shotsPerPoint"]) \
        + " shots per point \n from " + str(SigStart) + "ms to " + str(SigStop) + "ms gate"
    if Settings["param"] == 'setpoint':
        Xunit = " (V)"
    else:
        Xunit = ""
    xlabel = Settings["channel"] + " " + Settings["param"] + Xunit
    ylabel = "Gated LIF (ms.V)"
    
    fig = plt.figure()
    plt.plot(ScannedParam, OnBkgSub, '.', label="On")
    plt.plot(ScannedParam, OffBkgSub, '.', label="Off")
    if error==True:
        plt.errorbar(ScannedParam, OnBkgSub, yerr = OnStderrCounts, fmt=" ")
        plt.errorbar(ScannedParam, OffBkgSub, yerr = OffStderrCounts, fmt=" ")
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.legend()
    if display:
        plt.show()
    
    return fig

def PlotGatedAvgCountsOnOffWM(Scan,TOFDataOn,TOFDataOff,TimeOn,TimeOff,\
                SigStart,SigStop,BkgStart,BkgStop,error=False, display=True):
    "Plot gated counts against scanned parameter, with background subtraction"
    OnMeanCounts, OnStderrCounts, OnTimeWindow = GetGatedAvgCounts(Scan,\
                                        TOFDataOn,TimeOn,SigStart,SigStop)
    OnBkgMeanCounts, OnBkgStderrCounts, OnBkgTimeWindow = GetGatedAvgCounts(Scan,\
                                        TOFDataOn,TimeOn,BkgStart,BkgStop)
    OnBkgSub = OnMeanCounts - OnBkgMeanCounts * OnTimeWindow/OnBkgTimeWindow

    OffMeanCounts, OffStderrCounts, OffTimeWindow = GetGatedAvgCounts(Scan,\
                                        TOFDataOff,TimeOff,SigStart,SigStop)
    OffBkgMeanCounts, OffBkgStderrCounts, OffBkgTimeWindow = GetGatedAvgCounts(Scan,\
                                        TOFDataOff,TimeOff,BkgStart,BkgStop)
    OffBkgSub = OffMeanCounts - OffBkgMeanCounts * OffTimeWindow/OffBkgTimeWindow
    
    #Scan params for plotting
    f_iniTHz, f_relMHz = GetScanFreqArrayMHz(Scan)
    Settings = GetScanSettings(Scan)
    
    title = "Gated TOF over " + Settings["param"] + " with " + str(Settings["shotsPerPoint"]) \
        + " shots per point \n from " + str(SigStart) + "ms to " + str(SigStop) + "ms gate"
    
    xlabel = "Relative frequency from %.8g THz"%f_iniTHz
    ylabel = "Gated LIF (ms.V)"
    
    fig = plt.figure()
    plt.plot(f_relMHz, OnBkgSub, '.', label="On")
    plt.plot(f_relMHz, OffBkgSub, '.', label="Off")
    if error==True:
        plt.errorbar(f_relMHz, OnBkgSub, yerr = OnStderrCounts, fmt=" ")
        plt.errorbar(f_relMHz, OffBkgSub, yerr = OffStderrCounts, fmt=" ")
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.legend()
    if display:
        plt.show()
    
    return fig

def TCL_WM_Calibration(Scan, step=0.01, plot=False):
    ScanParams = GetScanParameterArray(Scan)
    Settings = GetScanSettings(Scan)
    f_iniTHz, f_relMHz = GetScanFreqArrayMHz(Scan)
    
    fit, cov = curve_fit(tools.Line, ScanParams, f_relMHz, p0=[-150., 0.])

    xspan = np.arange(np.min(ScanParams), np.max(ScanParams), step=step)
    
    err = np.sqrt(np.diag(cov))
    fit_results = {"Variables":["slope", "shift"],
                   "best fit":fit, "error":err}
    
    print(fit_results)
    
    
    if plot:
        if Settings["param"] == 'setpoint':
             Xunit = " (V)"
        else:
             Xunit = ""
        xlabel = Settings["channel"] + " " + Settings["param"] + Xunit
        ylabel = "Relative frequency from %.8g THz"%f_iniTHz
        
        plt.plot(ScanParams, f_relMHz, '.')
        plt.plot(xspan, tools.Line(xspan, *fit))
        plt.title("TCL-WM calibration: %.4g MHz/V"%fit[0])
        plt.xlabel(xlabel)
        plt.ylabel(ylabel)
        plt.show()
    
    return fit_results

def GetSignalwithBackgroundSubtraction(Data,Time,StartSig,StopSig,StartBg,StopBg):
    [Background, BgTimeWindow] = GetCounts(Data,Time,StartBg,StopBg)
    [SignalAndBg, SignalTimeWindow] = GetCounts(Data,Time,StartSig,StopSig)
    BackgroundScaled = Background*SignalTimeWindow/BgTimeWindow
    Signal = SignalAndBg - BackgroundScaled
    return [Signal, BackgroundScaled]

def GetSignalwithBackgroundSubtractionSPP(Data,Time,StartSig,StopSig,StartBg,StopBg):
    [MeanBackground, StderrBackground, BgTimeWindow] = GetCountsSPP(Data,Time,StartBg,StopBg)
    [MeanSignalAndBg, StderrSignalAndBg, SignalTimeWindow] = GetCountsSPP(Data,Time,StartSig,StopSig)
    BackgroundScaled = MeanBackground*SignalTimeWindow/BgTimeWindow
    ErrBackgroundScaled = StderrBackground*SignalTimeWindow/BgTimeWindow
    MeanSignal = MeanSignalAndBg - BackgroundScaled
    StderrSignal = np.sqrt(StderrSignalAndBg**2 + ErrBackgroundScaled**2)
    return [MeanSignal, StderrSignal, BackgroundScaled]

def DownsampleTOF(Data, Time, NrSamples):
    NrRows = int(len(Data)/NrSamples)
    NrCols = NrSamples
    DownsampledDataRect = Data.reshape((NrRows,NrCols))                
    DownsampledData = np.sum(DownsampledDataRect, axis=1)
    DownsampledTime = Time[int(NrSamples/2)::NrSamples]
    return DownsampledTime, DownsampledData


#%% Functions for Blocks
def ReadBlockInZippedXML(Filename):
    ss = Data.EDM.BlockSerializer()
    Block = ss.DeserializeBlockFromZippedXML(Filename,"block.xml")
    # TODO: Adjust for zip-files with multiple passes
    return Block

def ExtractMagneticFields(Block, TOFnumber, Gain):
    NrShots = len(Block.Points)
    TOFlength = len(Block.Points[0].Shot.TOFs[TOFnumber].Data)
    Data = np.full((NrShots, TOFlength), np.nan)
    Names = GetDetectorNames(Block)
    Factor = 1
    Offset = 0
    if Names[TOFnumber][0:6] == 'quSpin':
        if Gain ==3:
            Factor = 1/8.1*1000 # Converts to pT
        elif Gain==1:
            Factor = 1/2.7*1000
        elif Gain == 0.33:
            Factor = 1/0.9*1000
        else:
            Factor = 1
    elif Names[TOFnumber][0:6] == 'bartin':
        Factor = 10000000 # 1V = 10uT
    elif Names[TOFnumber][0:6] == 'MiniFl':
        Offset = 2.5
        Factor = 50e6 # (OUT+ - 2.5)*50 to convert from V to uT
    for PointNumber in range(NrShots):
        Data[PointNumber,:] = (np.array(Block.Points[PointNumber].Shot.TOFs[TOFnumber].Data)-Offset)*Factor
    return Data

def ExtractAverageMagneticFields(Block, TOFnumber, Gain):
    NrShots = len(Block.Points)
    Data = np.full((NrShots), np.nan)
    DataStd = np.full((NrShots), np.nan)
    DataNr = np.full((NrShots), np.nan)
    Names = GetDetectorNames(Block)
    Factor = 1
    Offset = 0
    if Names[TOFnumber][0:6] == 'quSpin':
        if Gain ==3:
            Factor = 1/8.1*1000 # Converts to pT
        elif Gain==1:
            Factor = 1/2.7*1000
        elif Gain == 0.33:
            Factor = 1/0.9*1000
        else:
            Factor = 1
    elif Names[TOFnumber][0:6] == 'bartin':
        Factor = 10000000 # 1V = 10uT
    elif Names[TOFnumber][0:6] == 'MiniFl':
        Offset = 2.5
        Factor = 50e6 # (OUT+ - 2.5)*50 to convert from V to uT
    for PointNumber in range(NrShots):
        Dataset = (np.array(Block.Points[PointNumber].Shot.TOFs[TOFnumber].Data)-Offset)*Factor
        Data[PointNumber] = np.mean(Dataset)
        DataStd[PointNumber] = np.std(Dataset)
        DataNr[PointNumber] = len(Dataset)
    return Data, DataStd, DataNr

def ExtractAverageMagneticFieldSegments(Block, TOFnumber, Gain, NrSegments):
    NrShots = len(Block.Points)
    Data = np.full((NrShots,NrSegments), np.nan)
    DataStd = np.full((NrShots,NrSegments), np.nan)
    DataNr = np.full((NrShots,NrSegments), np.nan)
    Names = GetDetectorNames(Block)
    Factor = 1
    Offset = 0
    if Names[TOFnumber][0:6] == 'quSpin':
        if Gain ==3:
            Factor = 1/8.1*1000 # Converts to pT
        elif Gain==1:
            Factor = 1/2.7*1000
        elif Gain == 0.33:
            Factor = 1/0.9*1000
        else:
            Factor = 1
    elif Names[TOFnumber][0:6] == 'bartin':
        Factor = 10000000 # 1V = 10uT
    elif Names[TOFnumber][0:6] == 'MiniFl':
        Offset = 2.5
        Factor = 50e6 # (OUT+ - 2.5)*50 to convert from V to uT
    for PointNumber in range(NrShots):
        Dataset = (np.array(Block.Points[PointNumber].Shot.TOFs[TOFnumber].Data)-Offset)*Factor
        for Seg in range(NrSegments):
            Datalength = len(Dataset)
            Seglength = int(Datalength/NrSegments)
            Data[PointNumber,Seg] = np.mean(Dataset[Seglength*Seg:Seglength*(Seg+1)])
            DataStd[PointNumber,Seg] = np.std(Dataset[Seglength*Seg:Seglength*(Seg+1)])
            DataNr[PointNumber,Seg] = len(Dataset[Seglength*Seg:Seglength*(Seg+1)])
    return Data, DataStd, DataNr

def ExtractAverageMagneticFieldTimeseries(Block, TOFnumber, Gain):
    Datalength = len(Block.Points[0].Shot.TOFs[TOFnumber].Data)
    Data = np.full((Datalength,4), np.nan)
    DataStd = np.full((Datalength,4), np.nan)
    DataNr = np.full((Datalength,4), np.nan)
    Labels = ['E- switch before', 'E- no switch', 'E+ no switch', 'E+ switch before']
    Names = GetDetectorNames(Block)
    
    Factor = 1
    Offset = 0
    if Names[TOFnumber][0:6] == 'quSpin':
        if Gain ==3:
            Factor = 1/8.1*1000 # Converts to pT
        elif Gain==1:
            Factor = 1/2.7*1000
        elif Gain == 0.33:
            Factor = 1/0.9*1000
        else:
            Factor = 1
    elif Names[TOFnumber][0:6] == 'bartin':
        Factor = 10000000 # 1V = 10uT
    elif Names[TOFnumber][0:6] == 'MiniFl':
        Offset = 2.5
        Factor = 50e6 # (OUT+ - 2.5)*50 to convert from V to uT

    # Get the Efield pattern
    EfieldPattern = ExtractEfieldWaveformFromBlock(Block, 1)
    Switches = np.diff(EfieldPattern)
    Switches = np.insert(Switches,0,0)
    States = Switches + EfieldPattern
    Values = [-3, -1, 1, 3]
    for i in range(4):
        Indi = np.where(States == Values[i])[0]
        Dataset = np.full((len(Indi),Datalength), np.nan)
        for j in range(len(Indi)):
            Dataset[j,:] = (np.array(Block.Points[int(Indi[j])].Shot.TOFs[TOFnumber].Data)-Offset)*Factor
        for k in range(Datalength):
            Data[k,i] = np.mean(Dataset[:,k])
            DataStd[k,i] = np.std(Dataset[:,k])
            DataNr[k,i] = len(Dataset[:,k])
    return Data, DataStd, DataNr, Labels


def ExtractAverageMagneticFieldsAnd50HzPhase(Block, TOFnumber, Gain):
    NrShots = len(Block.Points)
    Data = np.full((NrShots), np.nan)
    DataStd = np.full((NrShots), np.nan)
    DataNr = np.full((NrShots), np.nan)
    Phases = np.full((NrShots), np.nan)
    Names = GetDetectorNames(Block)
    Factor = 1
    Offset = 0
    if Names[TOFnumber][0:6] == 'quSpin':
        if Gain ==3:
            Factor = 1/8.1*1000 # Converts to pT
        elif Gain==1:
            Factor = 1/2.7*1000
        elif Gain == 0.33:
            Factor = 1/0.9*1000
        else:
            Factor = 1
    elif Names[TOFnumber][0:6] == 'bartin':
        Factor = 10000000 # 1V = 10uT
    elif Names[TOFnumber][0:6] == 'MiniFl':
        Offset = 2.5
        Factor = 50e6 # (OUT+ - 2.5)*50 to convert from V to uT
    for PointNumber in range(NrShots):
        Dataset = (np.array(Block.Points[PointNumber].Shot.TOFs[TOFnumber].Data)-Offset)*Factor
        Time = np.array(Block.Points[PointNumber].Shot.TOFs[TOFnumber].Times)
        Data[PointNumber] = np.mean(Dataset)
        DataStd[PointNumber] = np.std(Dataset)
        DataNr[PointNumber] = len(Dataset)
        Phases[PointNumber] = Dataset*np.sin(2*np.pi*50*Time)
    return Data, DataStd, DataNr


def ExtractEfieldWaveform(Block, Emagnitude):
    NrBits = len(Block.Config.GetModulationByName('E').Waveform.Code)
    BitString = ''
    for ind in range(NrBits):
        if Block.Config.GetModulationByName('E').Waveform.Code[ind]:
            BitString = BitString + '1'
        else:
            BitString = BitString + '0'
    NrShots = len(Block.Points)
    Shots = [x for x in range(NrShots)]
    EfieldBinary = int(BitString,2)
    Pattern = [None]*len(Shots)
    for i in range(len(Shots)):
        BitMultiplication = Shots[i]&EfieldBinary
        State = format(BitMultiplication,'08b')
        Pattern[i] = 1-2*np.mod(State.count('1'),2)
    if Block.Config.GetModulationByName('E').Waveform.Inverted:
        EfieldPattern = np.array(Pattern)*Emagnitude*(-1)
    else:
        EfieldPattern = np.array(Pattern)*Emagnitude
    return EfieldPattern

def ExtractEfieldWaveformFromBlock(Block, Emagnitude):
    Bits = Block.Config.GetModulationByName('E').Waveform.Bits
    EfieldPattern = np.full((len(Bits)),np.nan)
    for i in range(len(Bits)):
        if Bits[i] == True:
            EfieldPattern[i] = Emagnitude
        else:
            EfieldPattern[i] = -Emagnitude
    return EfieldPattern

def ExtractBfieldWaveformFromBlock(Block):
    Bits = Block.Config.GetModulationByName('B').Waveform.Bits
    Bmagnitude = Block.Config.GetModulationByName('B').PhysicalStep
    BfieldPattern = np.full((len(Bits)),np.nan)
    for i in range(len(Bits)):
        if Bits[i] == True:
            BfieldPattern[i] = Bmagnitude
        else:
            BfieldPattern[i] = -Bmagnitude
    return BfieldPattern

def ExtractdBfieldWaveformFromBlock(Block):
    Bits = Block.Config.GetModulationByName('DB').Waveform.Bits
    dBmagnitude = Block.Config.GetModulationByName('DB').PhysicalStep
    BfieldPattern = np.full((len(Bits)),np.nan)
    for i in range(len(Bits)):
        if Bits[i] == True:
            BfieldPattern[i] = dBmagnitude
        else:
            BfieldPattern[i] = -dBmagnitude
    return BfieldPattern


def GetDetectorNames(Block):
    DetectorNamesList = []
    for name in Block.detectors:
        DetectorNamesList.append(name)
    return DetectorNamesList

def ExtractTOFsfromBlock(Block, TOFnumber):
    NrShots = len(Block.Points)
    TOFlength = len(Block.Points[0].Shot.TOFs[TOFnumber].Data)
    Data = np.full((NrShots, TOFlength), np.nan)
    Factor = 225
    for PointNumber in range(NrShots):
        Data[PointNumber,:] = np.array(Block.Points[PointNumber].Shot.TOFs[TOFnumber].Data)*Factor
    return Data

def GetTOFsFromBlock(Block, TOFnumber):
    """Returns the TOFs of a block. The datasets are On/Off shots and for each 
    detector (TOFs in one shot)"""
    SampleRate = Block.Config.Settings["clockFrequency"]
    NrShots = len(Block.Points)
    TOFlength = len(Block.Points[0].Shot.TOFs[0].Data)
    Time = np.array(Block.Points[0].Shot.TOFs[0].Times)/SampleRate
    Data = np.empty((TOFlength, NrShots, 1))*np.nan
    for indPoint in range(NrShots):
        DataTemp = np.array(Block.Points[indPoint].Shot.TOFs[TOFnumber].Data)
        Data[:,indPoint,0] = DataTemp
    return Time, Data

def GetShotTimestampsFromBlock(Block):
    NrShots = len(Block.Points)
    TimeStamps = np.full((NrShots),np.nan)
    for i in range(NrShots):
        T = Block.Points[i].Shot.TimeStamp
        TimeStamps[i] = datetime.datetime.strptime(T.ToString("dd/MM/yyyy HH:mm:ss.ffffff+00:00"), '%d/%m/%Y %H:%M:%S.%f%z').timestamp()
    return TimeStamps

def GetBlockConfigurationList(Block):
    return list(Block.Config.Settings.StringKeyList)

def GetAppliedBiasCurrent(Block):
    """Extract the total current applied per point in the block from the applied 
    bias current and the B and dB step. Value is returned in .
    """
    NrShots = len(Block.Points)
    Itotal = np.full((NrShots),np.nan)

    # Read the bias current always applied
    # Ibias = Block.Config.Settings["bBiasV"]
    Ibias = Block.Config.GetModulationByName('B').PhysicalCentre
    B = ExtractBfieldWaveformFromBlock(Block)
    dB = ExtractdBfieldWaveformFromBlock(Block)
    Itotal = Ibias - B + np.sign(B)*dB
    return Itotal

def GetBSwitchState(Block):
    BitsB = Block.Config.GetModulationByName('B').Waveform.Bits
    BitsdB = Block.Config.GetModulationByName('DB').Waveform.Bits
    SwitchState = np.full((len(BitsB)),np.nan)
    for i in range(len(BitsB)):
        if (BitsB[i] == False) & (BitsdB[i] == False):
            SwitchState[i] = 0
        elif (BitsB[i] == False) & (BitsdB[i] == True):
            SwitchState[i] = 1
        elif (BitsB[i] == True) & (BitsdB[i] == False):
            SwitchState[i] = 2
        elif (BitsB[i] == True) & (BitsdB[i] == True):
            SwitchState[i] = 3
    return SwitchState

def GetSettleTime(Block):
    SettleTime = Block.Config.Settings["eRampUpDelay"]
    return SettleTime
    

#%% Function for analysis

def ScaledMAD(x):
    """
    Calculate the scaled median absolute deviation (MAD), should correspond to the 
    standard deviation for a dataset without outliers

    Parameters
    ----------
    x : array of floats
        Array of which the scaled MAD should be determined.

    Returns
    -------
    float
        The scaled MAD of the array x.

    """
    c = -1/(np.sqrt(2)*erfcinv(3/2))
    return c*np.median(np.abs(x-np.median(x)))

def DetectOutliersOfDistributionMAD(x, MADlevel):
    Median = np.median(x)
    Sigma = ScaledMAD(x)
    Indi = np.abs(x-Median) > MADlevel*Sigma
    return Indi

def MovingAverage(data,N):
    ma = np.full((len(data)), np.nan)
    for ind in range(len(data)):
        if (ind < N/2-1) | (ind > len(data)-N/2):
            continue
        else:
            datacut = data[ind-int(N/2):ind+int(N/2)]
            ma[ind] = np.mean(datacut[~np.isnan(datacut)])
    return ma

