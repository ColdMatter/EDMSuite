import sys
import numpy as np 
from scipy.optimize import curve_fit 
import matplotlib.pyplot as plt
from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time, os, fnmatch
from uedm_init_pythonnet import *

#Generic functions for fits
def gaussian(x, a, x0, sigma, c): 
    return (a*np.exp(-(x-x0)**2/(2*sigma**2))+c) 

def doublegaussian(x, a1, center1, sigma1, a2, center2, sigma2, offset):
    return (a1*np.exp(-(x-center1)**2/(2*sigma1**2))+a2*np.exp(-(x-center2)**2/(2*sigma2**2))+offset) 

#Useful functions for user interface
def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()
    
def frange(start, stop=None, step=None):
	start = float(start)
	if stop == None:
		stop = start + 0.0
		start = 0.0
	if step == None:
		step = 1.0

	count = 0
	while True:
		temp = float(start + count * step)
		if step > 0 and temp >= stop:
			break
		elif step < 0 and temp <= stop:
			break
		yield temp
		count += 1
          
def windowValue(value,lowerlim,upperlim):
    output=value
    if value<lowerlim:
        output=lowerlim
    if value>upperlim:
        output=upperlim
    return output

def scanSettings(scan:Scan,pattern):
    scansettings=list(scan.ScanSettings.StringKeyList)
    keylist=[i for i in scansettings if pattern in i]
    valuelist=list()
    for i,param in enumerate(keylist):
        valuelist.append(scan.ScanSettings[param])
    output=np.vstack((keylist,valuelist))
    return output.transpose()

def scanSettingsFromFile(filename:str,pattern:str):
    ss=ScanSerializer()
    scan=ss.DeserializeScanFromZippedXML(filename,"average.xml")
    scansettings=list(scan.ScanSettings.StringKeyList)
    keylist=[i for i in scansettings if pattern in i]
    valuelist=list()
    for i,param in enumerate(keylist):
        valuelist.append(scan.ScanSettings[param])
    output=np.vstack((keylist,valuelist))
    return output.transpose()

def getFiles(pattern):
    fileSystem = Environs.FileSystem
    blockPath = fileSystem.Paths["edmDataPath"]
    scanPath = fileSystem.Paths["scanMasterDataPath"]
    #file = fileSystem.GetDataDirectory(fileSystem.Paths["scanMasterDataPath"]) + filepath
    result = []
    for root, dirs, files in os.walk(scanPath):
        for name in files:
            if fnmatch.fnmatch(name, pattern):
                result.append(os.path.join(root, name))
    return result

def getFile(file):
    return getFiles(file)[0]

def getScans(files):
    ss=ScanSerializer()
    scans=[]
    for file in files:
        scans.append(ss.DeserializeScanFromZippedXML(str(file),"average.xml"))
    return scans

def getScan(file):
    ss=ScanSerializer()
    scan=ss.DeserializeScanFromZippedXML(str(file),"average.xml")
    return scan
               
def getNextFile():
    fileSystem = Environs.FileSystem
    dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
    file=fileSystem.GenerateNextDataFileName()
    filepath = fileSystem.GetDataDirectory(fileSystem.Paths["scanMasterDataPath"]) + file
    return [filepath,file]

#Processing and fitting
def fitGaussian(voltage,signal):
    first_try = [max(signal)-min(signal), voltage[np.argmax(signal)], (max(voltage)-min(voltage))/5, ((max(signal)-min(signal))/5)+min(signal)]
    popt, pcov = curve_fit(gaussian, voltage, signal, p0=first_try)
    return [popt,pcov]

def fitDoubleGaussian(voltage,signal):
    first_try = [max(signal)-min(signal), voltage[np.argmax(signal)], (max(voltage)-min(voltage))/5, (max(signal)-min(signal))/2, voltage[np.argmax(signal)]-0.1, (max(voltage)-min(voltage))/5, ((max(signal)-min(signal))/5)+min(signal)]
    popt, pcov = curve_fit(doublegaussian, voltage, signal, p0=first_try)
    return [popt,pcov]

def processScanType(scan:Scan, scantype='On', detector=0, intst=0, intend=50, bgst=55, bgend=120):
    '''A function to process a scan into a list of voltage and signal.
    Inputs:
        - a scan
        - a scantype ('On','Off','OnOff' (minus),'OnOffRatio' (ratio),'OffOn' or 'OffOnRatio')
        - detector (0,1)
        - intst, intend, bgst, bgend as start and end times in ms
    '''
    sr=float(scan.ScanSettings["shot:sampleRate"])
    gateStart=int(scan.ScanSettings["shot:gateStartTime"])
    gateLength=int(scan.ScanSettings["shot:gateLength"])
    intStart=int(windowValue(((sr/1000)*intst),gateStart,(gateStart+gateLength)))
    intEnd=int(windowValue(((sr/1000)*intend),gateStart,(gateStart+gateLength)))
    bgStart=int(windowValue(((sr/1000)*bgst),gateStart,(gateStart+gateLength)))
    bgEnd=int(windowValue(((sr/1000)*bgend),gateStart,(gateStart+gateLength)))

    voltage = np.array(scan.ScanParameterArray)

    if scantype=='On':
        integral = np.array(scan.GetTOFOnIntegralArray(detector, intStart, intEnd))
        bg = np.array(scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd))
        
        signal = 22.5*(integral - bg)
    elif scantype=='Off':
        integral = np.array(scan.GetTOFOffIntegralArray(detector, intStart, intEnd))
        bg = np.array(scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd))
        
        signal = 22.5*(integral - bg)
    elif scantype=='OnOff':
        int1 = np.array(scan.GetTOFOnIntegralArray(detector, intStart, intEnd))
        int2 = np.array(scan.GetTOFOffIntegralArray(detector, intStart, intEnd))
        bg1 = np.array(scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd))
        bg2 = np.array(scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd))
        sig1 = int1 - bg1
        sig2 = int2 - bg2
        signal = 22.5*(sig1 - sig2)
    elif scantype=='OffOn':
        int1 = np.array(scan.GetTOFOnIntegralArray(detector, intStart, intEnd))
        int2 = np.array(scan.GetTOFOffIntegralArray(detector, intStart, intEnd))
        bg1 = np.array(scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd))
        bg2 = np.array(scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd))
        sig1 =  int1 - bg1
        sig2 =  int2 - bg2
        signal =  22.5*(sig2 - sig1)
    elif scantype=='OnOffRatio':
        int1 = np.array(scan.GetTOFOnIntegralArray(detector, intStart, intEnd))
        int2 = np.array(scan.GetTOFOffIntegralArray(detector, intStart, intEnd))
        bg1 = np.array(scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd))
        bg2 = np.array(scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd))
        sig1 =  int1 - bg1
        sig2 =  int2 - bg2
        signal = sig1 / sig2
    elif scantype=='OffOnRatio':
        int1 = np.array(scan.GetTOFOnIntegralArray(detector, intStart, intEnd))
        int2 = np.array(scan.GetTOFOffIntegralArray(detector, intStart, intEnd))
        bg1 = np.array(scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd))
        bg2 = np.array(scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd))
        sig1 =  int1 - bg1
        sig2 =  int2 - bg2
        signal = sig2 / sig1
    else:
        print("Incorrect Scantype, try again")
        return
    return [voltage,signal] 

def getSetPoint(filePath='..\\..\\Example_scan_01.zip', scantype='On', detector=0,intStart=60, intEnd=500, bgStart=510, bgEnd=650):
    scanSerializer = ScanSerializer()
    #gfitter = GaussianFitter()

    file = getFile(filePath)

    scan = scanSerializer.DeserializeScanFromZippedXML(str(file),"average.xml")

    [voltage,signal] = processScanType(scan, scantype, detector, intStart, intEnd, bgStart, bgEnd)

    coeffs, pcov = fitGaussian(voltage,signal)
    
    #print(coeffs)
    center=round(coeffs[1],6)
    return center

def getSetPoint2(filePath='..\\..\\Example_scan_01.zip', scantype='On', detector=0,intStart=750, intEnd=4000, bgStart=600, bgEnd=750):
    scanSerializer = ScanSerializer()
    gfitter = GaussianFitter()

    file = getFile(filePath)

    scan = scanSerializer.DeserializeScanFromZippedXML(str(file),"average.xml")

    [voltage,signal] = processScanType(scan, scantype, detector, intStart, intEnd, bgStart, bgEnd)

    bestGuess=gfitter.SuggestParameters(voltage,signal,0,1)

    gfitter.Fit(voltage,signal,bestGuess)
    coeffs=gfitter.ParameterReport

    center=round(gfitter.returncenter(),6)
    return center

def getCoolingRatio(filePath='..\\..\\Example_scan_01.zip', scantype='OnOffRatio', detector=0,intStart=60, intEnd=500, bgStart=510, bgEnd=650):
    scanSerializer = ScanSerializer()
    #gfitter = GaussianFitter()

    file = getFile(filePath)

    scan = scanSerializer.DeserializeScanFromZippedXML(str(file),"average.xml")

    [voltage,signal] = processScanType(scan, scantype, detector, intStart, intEnd, bgStart, bgEnd)
    mean=np.mean(signal)
    std=np.std(signal)
    if mean<1:
        mean=1/mean
    return [mean,std]

def getfit(scan, scantype='On', fitfunc='gaussian', detector=0, intStart=10, intEnd=40, bgStart=41, bgEnd=90):
    # Process the scan into arrays
    [voltage,signal] = processScanType(scan, scantype, detector, intStart, intEnd, bgStart, bgEnd)

    x = voltage
    y = signal

    # Executing curve_fit on data 
    if fitfunc=='doublegaussian':
        popt, pcov = fitDoubleGaussian(x,y)
        ym = doublegaussian(x, *popt) 
    else:
        popt, pcov = fitGaussian(x,y) 
        ym = gaussian(x, *popt) 

    #popt returns the best fit values for parameters of the given model (func) 
    return [x,y,ym,popt,pcov]

# Plotting
def plotfit(scan, scantype='On', fitfunc='gaussian', detector=0, intStart=1000, intEnd=4000, bgStart=4100, bgEnd=5000):
    # Process the scan into arrays
    [voltage,signal] = processScanType(scan, scantype, detector, intStart, intEnd, bgStart, bgEnd)

    x = voltage
    y = signal

    # Plot out the current state of the data and model 
    fig = plt.figure() 
    ax = fig.add_subplot(111) 
    #ax.plot(x, y, c='k', label='Function') 
    ax.scatter(x, y)
    ax.set_xlabel("TCL Setpoint / V")
    ax.set_ylabel("Photons")

    # Executing curve_fit on data 
    if fitfunc=='doublegaussian':
        popt, pcov = fitDoubleGaussian(x,y)
        ym = doublegaussian(x, popt[0], popt[1], popt[2], popt[3], popt[4], popt[5], popt[6]) 
    else:
        popt, pcov = fitGaussian(x,y) 
        ym = gaussian(x, popt[0], popt[1], popt[2], popt[3]) 

    #popt returns the best fit values for parameters of the given model (func) 
    print ("New setpoint is %3.6f" % popt[1]) 
    ax.plot(x, ym, c='r', label=f'Best fit: mu={popt[1]:3.6f}') 
    ax.legend() 
    plt.show()

def plotTOF(scan, scanst=-1000, scanend=1000, bgst=41, bgend=120):
    # Window inputs for sensible results
    sr=float(scan.ScanSettings["shot:sampleRate"])
    gateStart=int(scan.ScanSettings["shot:gateStartTime"])
    gateLength=int(scan.ScanSettings["shot:gateLength"])
    bgStart=int(windowValue(((sr/1000)*bgst),gateStart,(gateStart+gateLength)))
    bgEnd=int(windowValue(((sr/1000)*bgend),gateStart,(gateStart+gateLength)))
    scanStart=windowValue(scanst,min(scan.ScanParameterArray),max(scan.ScanParameterArray))
    scanEnd=windowValue(scanend,min(scan.ScanParameterArray),max(scan.ScanParameterArray))

    # Make arrays for timebase and signal and background subtract the laser scatter
    times=np.array(scan.GetGatedAverageOnShot(scanStart,scanEnd).TOFs[0].Times)
    data=np.array(scan.GetGatedAverageOnShot(scanStart,scanEnd).TOFs[0].Data)
    bg=np.mean(data[bgStart:bgEnd])
    signal=data-bg

    #plot the TOF
    fig = plt.figure() 
    ax = fig.add_subplot(111) 
    ax.plot(times, signal)
    ax.set_xlabel("Time / ms")
    ax.set_ylabel("PMT Voltage")
    plt.show()

#Scanmaster control
def SelectProfile(profileName):
    sm.SelectProfile(profileName)

def StartPattern():
    sm.OutputPattern()

def StopPattern():
    sm.StopPatternOutput()

#TCL control
def lineformat(cavity,laser,setpoint):
    outstr=cavity+"\t"+laser+"\t\t"+str(round(setpoint,5))+"\n"
    return outstr

def saveTCLSetPoints():
    probeSetpoint = tcl.GetLaserSetpoint("VISCavity", "probelaser")
    v0Setpoint = tcl.GetLaserSetpoint("VISCavity", "v0laser")
    v1Setpoint = tcl.GetLaserSetpoint("VISCavity", "v1laser")
    q0Setpoint = tcl.GetLaserSetpoint("OPCavity", "Q0")
    p12Setpoint = tcl.GetLaserSetpoint("OPCavity", "P12")
    stirapSetpoint = tcl.GetLaserSetpoint("IRCavity", "STIRAP")
    [filepath,file] = getNextFile()
    probeline=lineformat("VISCavity","probelaser",probeSetpoint)
    v0line=lineformat("VISCavity","v0laser",v0Setpoint)
    v1line=lineformat("VISCavity","v1laser",v1Setpoint)
    q0line=lineformat("OPCavity","Q0",q0Setpoint)
    p12line=lineformat("OPCavity","P12",p12Setpoint)
    stirapline=lineformat("IRCavity","STIRAP",stirapSetpoint)
    linelist=[probeline,v0line,v1line,q0line,p12line,stirapline]
    with open(filepath[:-2]+'_TCLSetpoints.txt','w') as patternfile:
            for line in linelist:
                patternfile.write(line)


