from Data.Scans import *

def windowValue(value,lowerlim,upperlim):
    output=value
    if value<lowerlim:
        output=lowerlim
    if value>upperlim:
        output=upperlim
    return output

def processScanType(scan:Scan, scantype='On', detector=0, intst=0, intend=50, bgst=55, bgend=120,pmtCalib=559):
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
        
        signal = pmtCalib*(integral - bg)
    elif scantype=='Off':
        integral = np.array(scan.GetTOFOffIntegralArray(detector, intStart, intEnd))
        bg = np.array(scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd))
        
        signal = pmtCalib*(integral - bg)
    elif scantype=='OnOff':
        int1 = np.array(scan.GetTOFOnIntegralArray(detector, intStart, intEnd))
        int2 = np.array(scan.GetTOFOffIntegralArray(detector, intStart, intEnd))
        bg1 = np.array(scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd))
        bg2 = np.array(scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd))
        sig1 = int1 - bg1
        sig2 = int2 - bg2
        signal = pmtCalib*(sig1 - sig2)
    elif scantype=='OffOn':
        int1 = np.array(scan.GetTOFOnIntegralArray(detector, intStart, intEnd))
        int2 = np.array(scan.GetTOFOffIntegralArray(detector, intStart, intEnd))
        bg1 = np.array(scan.GetTOFOnIntegralArray(detector, bgStart, bgEnd))
        bg2 = np.array(scan.GetTOFOffIntegralArray(detector, bgStart, bgEnd))
        sig1 =  int1 - bg1
        sig2 =  int2 - bg2
        signal =  pmtCalib*(sig2 - sig1)
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

def fitGaussian(voltage,signal):
    first_try = [max(signal)-min(signal), voltage[np.argmax(signal)], (max(voltage)-min(voltage))/5, ((max(signal)-min(signal))/5)+min(signal)]
    popt, pcov = curve_fit(gaussian, voltage, signal, p0=first_try)
    return [popt,pcov]

def gaussian(x, a, x0, sigma, c): 
    return (a*np.exp(-(x-x0)**2/(2*sigma**2))+c)

def doublegaussian(x, a1, center1, sigma1, a2, center2, sigma2, offset):
    return (a1*np.exp(-(x-center1)**2/(2*sigma1**2))+a2*np.exp(-(x-center2)**2/(2*sigma2**2))+offset) 

def fitGaussian(voltage,signal):
    first_try = [max(signal)-min(signal), voltage[np.argmax(signal)], (max(voltage)-min(voltage))/5, ((max(signal)-min(signal))/5)+min(signal)]
    popt, pcov = curve_fit(gaussian, voltage, signal, p0=first_try)
    return [popt,pcov]

def fitDoubleGaussian(voltage,signal):
    first_try = [max(signal)-min(signal), voltage[np.argmax(signal)], (max(voltage)-min(voltage))/5, (max(signal)-min(signal))/2, voltage[np.argmax(signal)]-0.1, (max(voltage)-min(voltage))/5, ((max(signal)-min(signal))/5)+min(signal)]
    popt, pcov = curve_fit(doublegaussian, voltage, signal, p0=first_try)
    return [popt,pcov]

def getfit(scan, scantype='On', fitfunc='gaussian', detector=0, intStart=10, intEnd=40, bgStart=41, bgEnd=90):
    # Process the scan into arrays
    [voltage,signal] = processScanType(scan, scantype, detector, intStart, intEnd, bgStart, bgEnd)

    x = voltage
    y = signal

    try:
        # Executing curve_fit on data 
        if fitfunc=='doublegaussian':
            popt, pcov = fitDoubleGaussian(x,y)
            ym = doublegaussian(x, *popt) 
        else:
            popt, pcov = fitGaussian(x,y) 
            ym = gaussian(x, *popt) 
    except:
        print("Couldn't find fit")
        return [x,y]
    #popt returns the best fit values for parameters of the given model (func) 
    return [x,y,ym,popt,pcov]

def plotfit(scan:Scan, scantype='On', fitfunc='gaussian', detector=0, intStart=4, intEnd=50, bgStart=50, bgEnd=120):

    [x,y,ym,popt,pcov] = getfit(scan, scantype, fitfunc, detector, intStart, intEnd, bgStart, bgEnd)

    # Plot out the current state of the data and model 
    fig,ax = plt.subplots() 
    ax.plot(x,y,c='k')
    ax.set_xlabel("TCL Setpoint / V")
    ax.set_ylabel("Photons")

    #popt returns the best fit values for parameters of the given model (func) 
    ax.plot(x, ym, c='r', label=f'Best fit: setpoint is {popt[1]:3.6f}') 
    ax.legend() 
    plt.show()

def scanSettings(scan:Scan,pattern:str):
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