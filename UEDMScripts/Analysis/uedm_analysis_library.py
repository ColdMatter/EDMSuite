#%% uedm_analysis_libary.py sets up the connection to the SharedCode library and 
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

repo = git.Repo(os.path.dirname(os.path.abspath(__file__)), search_parent_directories=True)
RootFolder = repo.working_tree_dir

# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")


# Import the SharedCode DLLs, assumes you are executing this function from within
# the EDMSuite Git repository
clr.AddReference(Path.GetFullPath(RootFolder + r"\SEDM4\Libraries\SharedCode.dll"))
import System
import Data


#%% Functions related to Scan objects
def GetScanParameterArray(Scan):
    return np.array(Scan.ScanParameterArray)

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
    DataOn = np.empty((TimeOn.shape[0], NrTOFs, NrDetectors))*np.nan
    Index = 0
    for indPoint in range(NrScanPoints):
        for indOn in range(NrOnShotsPerPoint):
            for indDet in range(NrDetectors):
                Data = np.array(Scan.Points[indPoint].OnShots[indOn].TOFs[indDet].Data)
                DataOn[:,Index, indDet] = Data
            Index = Index+1

    if NrOffShotsPerPoint == 0:
        TimeOff = []
        DataOff = []
    else:
        Index = 0
        TimeOff = np.array(Scan.Points[0].OffShots[0].TOFs[0].Times)/SampleRate
        DataOff = np.empty((TimeOff.shape[0], NrTOFs, NrDetectors))*np.nan
        for indPoint in range(NrScanPoints):
            for indOn in range(NrOffShotsPerPoint):
                for indDet in range(NrDetectors):
                    Data = np.array(Scan.Points[indPoint].OffShots[indOn].TOFs[indDet].Data)
                    DataOff[:,Index, indDet] = Data
                Index = Index+1

    return TimeOn, DataOn, TimeOff, DataOff

def DownsampleTOFs(Scan, DownSampleRate):
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


#%% Functions for the TOF
def GetCounts(Data,Time,Start,Stop):
    Indi = (Time*1000>Start) & (Time*1000 <Stop)
    IndiArray = np.where(Indi)[0]
    Counts = np.sum(Data[Indi,:,:], axis=0)
    TimeWindow = Time[IndiArray[-1]]-Time[IndiArray[0]]
    return Counts, TimeWindow

def GetSignalwithBackgroundSubtraction(Data,Time,StartSig,StopSig,StartBg,StopBg):
    [Background, BgTimeWindow] = GetCounts(Data,Time,StartBg,StopBg)
    [SignalAndBg, SignalTimeWindow] = GetCounts(Data,Time,StartSig,StopSig)
    BackgroundScaled = Background*SignalTimeWindow/BgTimeWindow
    Signal = SignalAndBg - BackgroundScaled
    return [Signal, BackgroundScaled]


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

def GetBlockConfigurationList(Block):
    return list(Block.Config.Settings.StringKeyList)

def GetAppliedBiasCurrent(Block):
    """Extract the total current applied per point in the block from the applied 
    bias current and the B and dB step. Value is returned in .
    """
    NrShots = len(Block.Points)
    Itotal = np.full((NrShots),np.nan)

    # Read the bias current always applied
    Ibias = Block.Config.Settings["bBiasV"]
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
