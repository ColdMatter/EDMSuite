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
import datetime
import zipfile

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

def ProcessAllScansInZippedXML(Filename):
    ss = Data.Scans.ScanSerializer()
    with zipfile.ZipFile(Filename, 'r') as ZipFile:
        FileList = ZipFile.namelist()
        FileIndi = [FileList.index(l) for l in FileList if l.startswith('scan')]
    index = 0
    for i in FileIndi:
        ScanName = FileList[i]
        Scan = ss.DeserializeScanFromZippedXML(Filename,ScanName)
        if index == 0:
            TimeOn, DataOn, TimeOff, DataOff, ScanParameters, TimestampsOn, TimestampsOff = \
                ProcessScan(Scan)
        else:
            _, DataOnTemp, _, DataOffTemp, ScanParametersTemp, TimestampsOnTemp, TimestampsOffTemp = \
                ProcessScan(Scan)
            DataOn = np.concatenate([DataOn, DataOnTemp], axis=1)
            if DataOff:
                DataOff = np.concatenate([DataOff, DataOffTemp], axis=1)
                TimestampsOff = np.concatenate([TimestampsOff, TimestampsOffTemp], axis=0)
            ScanParameters = np.concatenate([ScanParameters, ScanParametersTemp], axis=0)
            TimestampsOn = np.concatenate([TimestampsOn, TimestampsOnTemp], axis=0)
        index = index + 1
    return TimeOn, DataOn, TimeOff, DataOff, ScanParameters, TimestampsOn, TimestampsOff
    

def ProcessScan(Scan):
    """Processes a scan object and returns the TOFs, the scan parameters and the 
    timestamps of the shots."""
    TimeOn, DataOn, TimeOff, DataOff = GetTOFs(Scan)
    ScanParameter = GetScanParameterArray(Scan)
    ShotsPerPoint = Scan.GetSetting("out","shotsPerPoint")
    ScanParameters = np.repeat(ScanParameter, ShotsPerPoint, axis=0)
    TimestampsOn, TimestampsOff = GetShotTimestamps(Scan)
    return TimeOn, DataOn, TimeOff, DataOff, ScanParameters, TimestampsOn, TimestampsOff

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
    NrPoints = len(Scan.Points)
    NrOnShotsPerPoint = len(Scan.Points[0].OnShots)
    NrOnShots = NrPoints*NrOnShotsPerPoint
    NrOffShotsPerPoint = len(Scan.Points[0].OffShots)
    NrOffShots = NrPoints*NrOffShotsPerPoint
    TimeStampsOn = np.full((NrOnShots),np.nan)
    TimeStampsOff = np.full((NrOffShots),np.nan)
    IndexOn = 0
    IndexOff = 0
    for i in range(NrPoints):
        for j in range(NrOnShotsPerPoint):
            T = Scan.Points[i].OnShots[j].TimeStamp
            TimeStampsOn[IndexOn] = datetime.datetime.strptime(T.ToString("dd/MM/yyyy HH:mm:ss.ffffff+00:00"), '%d/%m/%Y %H:%M:%S.%f%z').timestamp()
            IndexOn = IndexOn + 1
        for j in range(NrOffShotsPerPoint):
            T = Scan.Points[i].OffShots[j].TimeStamp
            TimeStampsOff[IndexOff] = datetime.datetime.strptime(T.ToString("dd/MM/yyyy HH:mm:ss.ffffff+00:00"), '%d/%m/%Y %H:%M:%S.%f%z').timestamp()
            IndexOff = IndexOff + 1 
    return TimeStampsOn, TimeStampsOff


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

def GetBinnedSignal(Data,Time,BinDuration,StartSig,StopSig,StartBg,StopBg):
    [Background, BgTimeWindow] = GetCounts(Data,Time,StartBg,StopBg)
    NrBins = int(np.floor((StopSig-StartSig)/BinDuration))
    BinStarts = StartSig + np.arange(NrBins)*BinDuration
    BinStops = BinStarts + BinDuration
    Signal = np.full((NrBins, Data.shape[1], Data.shape[2]), np.nan)
    for i in range(NrBins):
        Start = BinStarts[i]
        Stop = BinStops[i]
        [SignalAndBg, SignalTimeWindow] = GetCounts(Data,Time,Start,Stop)
        BackgroundScaled = Background*SignalTimeWindow/BgTimeWindow
        Signal[i,:,:] = SignalAndBg - BackgroundScaled
    return [Signal, BackgroundScaled,BinStarts, BinStops]

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


def WeightedMean(x, xerr):
    """
    Calculates the weighted mean of x with 1sigma errors xerr.

    Parameters
    ----------
    x : array of floats
        The values to be averaged.
    xerr : array of floats
        The uncertainties of the values to be averaged.

    Returns
    -------
    avgx : float
        The weighted average.
    avgerr : float
        The uncertainty of the weighted average, not increased if the reduced 
        chi squared value is larger than 1 .
    RChi2 : float
        The reduced chi squared value of the weighted mean.

    """
    Indi = np.isnan(x) | np.isnan(xerr)
    x = x[~Indi]
    xerr = xerr[~Indi]
    if len(x) ==1:
        avgx = x
        avgerr = xerr
        RChi2 = 1
    else:
        weight = 1/xerr**2
        avgx = sum(weight*x)/sum(weight)
        avgerr = np.sqrt(1/sum(weight))
        RChi2 = 1/(len(x)-1)*sum((x-avgx)**2 * weight)
    
    return avgx, avgerr, RChi2
