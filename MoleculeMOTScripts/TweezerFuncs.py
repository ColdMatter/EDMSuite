

import numpy as np
import matplotlib.pyplot as plt
from scipy.optimize import curve_fit
from scipy import optimize



def gaussian(height, center_x, center_y, width_x, width_y):
    """Returns a gaussian function with the given parameters"""
    width_x = float(width_x)
    width_y = float(width_y)
    return lambda x,y: height*np.exp(-(((center_x-x)/width_x)**2+((center_y-y)/width_y)**2)/2)

def gaussianBG(height, center_x, center_y, width_x, width_y, BG):
    """Returns a gaussian function with the given parameters"""
    width_x = float(width_x)
    width_y = float(width_y)
    return lambda x,y: BG+height*np.exp(-(((center_x-x)/width_x)**2+((center_y-y)/width_y)**2)/2)


def fitgaussian(data, guess):
    """Returns (height, x, y, width_x, width_y)
    the gaussian parameters of a 2D distribution found by a fit"""
    errorfunction = lambda p: np.ravel(gaussian(*p)(*np.indices(data.shape)) -
                                 data)
    p, success = optimize.leastsq(errorfunction, guess)
    return p

def fitgaussianBG(data, guess):
    """Returns (height, x, y, width_x, width_y)
    the gaussian parameters of a 2D distribution found by a fit"""
    errorfunction = lambda p: np.ravel(gaussianBG(*p)(*np.indices(data.shape)) -
                                 data)
    p, success = optimize.leastsq(errorfunction, guess)
    return p

def moments(data):
    """Returns (height, x, y, width_x, width_y)
    the gaussian parameters of a 2D distribution by calculating its
    moments """
    total = data.sum()
    X, Y = np.indices(data.shape)
    x = (X*data).sum()/total
    y = (Y*data).sum()/total
    col = data[:, int(y)]
    width_x = np.sqrt(np.abs((np.arange(col.size)-y)**2*col).sum()/col.sum())
    row = data[int(x), :]
    width_y = np.sqrt(np.abs((np.arange(row.size)-x)**2*row).sum()/row.sum())
    height = data.max()
    return height, x, y, width_x, width_y
    
def linfunc(x, a, c):
    return a*x+c


def cart2pol(x, y):
    rho = np.sqrt(x**2 + y**2)
    phi = np.arctan2(y, x)
    return(rho, phi)

def pol2cart(rho, phi):
    x = rho * np.cos(phi)
    y = rho * np.sin(phi)
    return(x, y)


'''
def GetTemperature(imagename, seqno,numtrial, numavg,times1, imagecentre, cropsizeX, cropsizeY):
    #path="C:\\Users\\cafmot\\Box Sync\\CaF MOT\\MOTData\\MOTMasterData\\CaF19Dec1900\\"
    imgsizex=512
    imgsizey=672

    #image crop:
    xmin = int(imagecentre[1]-cropsizeY/2)
    xmax = int(imagecentre[1]+cropsizeY/2)
    ymin = int(imagecentre[0]-cropsizeX/2)
    ymax = int(imagecentre[0]+cropsizeX/2)

    lstclouds=[];lstprobes=[];lstbgs=[];lstods=[];lstodavg=[];totodlst=[];errtotodlst=[];
    for i in range(1,numtrial+1):
        odavg=np.zeros((imgsizex,imgsizey));totod=[];
        for j in range(((i-1)*3*numavg)+1,i*3*numavg,3):
            clouds = plt.imread(path+imagename + str(seqno) + "_" + str(j) + ".tif")
            probes = plt.imread(path+imagename + str(seqno) + "_" + str(j+1) + ".tif")
            bgs = plt.imread(path+imagename + str(seqno) + "_" + str(j+2) + ".tif")
            lstclouds.append(clouds);
            lstprobes.append(probes);
            lstbgs.append(bgs);
            od=np.log((probes.astype(float)-bgs.astype(float))/(clouds.astype(float)-bgs.astype(float)))
            od[np.isnan(od)] = 0.0
            od[od == -np.inf] = 0.0
            od[od == np.inf] = 0.0
            lstods.append(od);
            odavg=odavg+od
            totod.append(sum(sum(od[xmin:xmax,ymin:ymax])))
        odavg=odavg/numavg
        lstodavg.append(odavg)
        totodlst.append(np.mean(totod))
        errtotodlst.append(np.std(totod))
    
    #run for image size
    #imagedimensions = lstodavg[1].shape



    guessedParams = np.array([3,imagecentre[0],imagecentre[1], 50, 50])

    fig, axs = plt.subplots(3,numtrial,figsize=(12,4))



    lstXwidths = []
    lstYwidths = []
    lstXPos = []
    lstYPos = []

    for k in range(0,numtrial):
        TmpFig = axs[0,k]
        imageData = lstodavg[k][xmin:xmax,ymin:ymax]
        params = fitgaussian(imageData, guessedParams)
        TmpFig.imshow(imageData, cmap='rainbow')
       # TmpFig.contour(fit(*np.indices(imageData.shape)), cmap=plt.cm.copper)
        TmpFig.plot(params[2],params[1], '+')
   #     TmpFig.set_title(str(times[k]*1e3)+'ms')
        TmpFig = axs[1,k]
        TmpFig.plot(range(1,len(imageData[:,int(params[2])])+1),imageData[:,int(params[2])], 'r.')
        TmpFig.plot(np.linspace(0,ymax-ymin,100), gaussian(*params)(np.linspace(0,ymax-ymin,100),params[2]))
    #print('params[2] is '+str(params[2]))
        TmpFig = axs[2,k]
        TmpFig.plot(range(1,len(imageData[int(params[1]),:])+1),imageData[int(params[1]),:], 'r.')
        TmpFig.plot(np.linspace(0,xmax-xmin,100), gaussian(*params)(params[1],np.linspace(0,xmax-xmin,100)))
        lstXwidths.append(params[3])
        lstYwidths.append(params[4])
        lstXPos.append(params[1])
        lstYPos.append(params[2])

    np.array(lstXwidths)


    plt.figure()

    #converting widths from px to m
    SigX = 2*6.5e-6*(1/0.4)*np.array(lstXwidths)
    SigY = 2*6.5e-6*(1/0.4)*np.array(lstYwidths)
    #fit along x
    popt, pcov = curve_fit(linfunc, times1**2, (SigX)**2)
    print('Temperature along x is '+str(int(((87*1.66e-27/(1.38e-23))*popt[0])/(1e-6)))+' muK')
    #plot fit along x
    plt.plot(times1**2/(1e-6), (SigX)**2/(1e-6), 'r.' , label = 'x widths')
    plt.plot(np.linspace(0,max(times1**2))/(1e-6), linfunc(np.linspace(0,max(times1**2)),*popt)/(1e-6), 'r-', label = 'fit to x widths')
    #fit along y
    popt, pcov = curve_fit(linfunc, times1**2, (SigY)**2)
    print('Temperature along y is '+str(int(((87*1.66e-27/(1.38e-23))*popt[0])/(1e-6)))+' muK')
    #plot fit along y 
    plt.plot(times1**2/(1e-6), (SigY)**2/(1e-6), 'b.', label = 'y widths')
    plt.plot(np.linspace(0,max(times1**2))/(1e-6), linfunc(np.linspace(0,max(times1**2)),*popt)/(1e-6), 'b-', label = 'fit to y widths')
    plt.xlabel('squared expansion time in ms^2')
    plt.ylabel('squared gaussian RMS width mm^2')
    plt.legend()
    plt.show()
    
    print('x positions be like '+str(lstXPos))
    print('y positions be like '+str(lstYPos))
    
    GetTemperature.lstXPos = lstXPos
    GetTemperature.lstYPos = lstYPos
    GetTemperature.totodlst = totodlst
'''
def GetOD(path,imagename, seqno,numtrial, numavg,times1, imagecentre, cropsizeX, cropsizeY, crop):
    #path="C:\\Users\\cafmot\\Box Sync\\CaF MOT\\MOTData\\MOTMasterData\\CaF19Dec1900\\"
    imgsizex=512
    imgsizey=672

    #image crop:
    xmin = int(imagecentre[1]-cropsizeY/2)
    xmax = int(imagecentre[1]+cropsizeY/2)
    ymin = int(imagecentre[0]-cropsizeX/2)
    ymax = int(imagecentre[0]+cropsizeX/2)

    lstclouds=[];lstprobes=[];lstbgs=[];lstods=[];lstodavg=[];totodlst=[];errtotodlst=[];
    for i in range(1,numtrial+1):
        odavg=np.zeros((imgsizex,imgsizey));totod=[];
        for j in range(((i-1)*3*numavg)+1,i*3*numavg,3):
            clouds = plt.imread(path+imagename + str(seqno) + "_" + str(j) + ".tif")
            probes = plt.imread(path+imagename + str(seqno) + "_" + str(j+1) + ".tif")
            bgs = plt.imread(path+imagename + str(seqno) + "_" + str(j+2) + ".tif")
            lstclouds.append(clouds);
            lstprobes.append(probes);
            lstbgs.append(bgs);
            od=np.log((probes.astype(float)-bgs.astype(float))/(clouds.astype(float)-bgs.astype(float)))
            od[np.isnan(od)] = 0.0
            od[od == -np.inf] = 0.0
            od[od == np.inf] = 0.0
            lstods.append(od);
            odavg=odavg+od
            totod.append(sum(sum(od[xmin:xmax,ymin:ymax])))
        odavg=odavg/numavg
        lstodavg.append(odavg)
        totodlst.append(np.mean(totod))
        errtotodlst.append(np.std(totod))
    
    plt.figure()
    plt.plot(times1, totodlst ,'--o')


def GetTemperature(path,imagename, seqno,numtrial, numavg,times1, imagecentre, cropsizeX, cropsizeY, crop):
    #path="C:\\Users\\cafmot\\Box Sync\\CaF MOT\\MOTData\\MOTMasterData\\CaF19Dec1900\\"
    imgsizex=512
    imgsizey=672

    #image crop:
    xmin = int(imagecentre[1]-cropsizeY/2)
    xmax = int(imagecentre[1]+cropsizeY/2)
    ymin = int(imagecentre[0]-cropsizeX/2)
    ymax = int(imagecentre[0]+cropsizeX/2)

    lstclouds=[];lstprobes=[];lstbgs=[];lstods=[];lstodavg=[];totodlst=[];errtotodlst=[];
    for i in range(1,numtrial+1):
        odavg=np.zeros((imgsizex,imgsizey));totod=[];
        for j in range(((i-1)*3*numavg)+1,i*3*numavg,3):
            clouds = plt.imread(path+imagename + str(seqno) + "_" + str(j) + ".tif")
            probes = plt.imread(path+imagename + str(seqno) + "_" + str(j+1) + ".tif")
            bgs = plt.imread(path+imagename + str(seqno) + "_" + str(j+2) + ".tif")
            lstclouds.append(clouds);
            lstprobes.append(probes);
            lstbgs.append(bgs);
            od=np.log((probes.astype(float)-bgs.astype(float))/(clouds.astype(float)-bgs.astype(float)))
            od[np.isnan(od)] = 0.0
            od[od == -np.inf] = 0.0
            od[od == np.inf] = 0.0
            lstods.append(od);
            odavg=odavg+od
            totod.append(sum(sum(od[xmin:xmax,ymin:ymax])))
        odavg=odavg/numavg
        lstodavg.append(odavg)
        totodlst.append(np.mean(totod))
        errtotodlst.append(np.std(totod))
    
    #run for image size
    #imagedimensions = lstodavg[1].shape



 



    lstXwidths = []
    lstYwidths = []
    lstXPos = []
    lstYPos = []
    

        
    fig, axs = plt.subplots(3,numtrial,figsize=(12,6))


    for k in range(0,numtrial):
        TmpFig = axs[0,k]
        
        if crop == True:
            imagedata = lstodavg[k][xmin:xmax,ymin:ymax]
        if crop == False:
            imagedata = lstodavg[k]
            
        imagearray = np.array(imagedata)
        guessedParams = np.array([np.amax(imagearray),np.where(imagearray == np.amax(imagearray))[0][0],np.where(imagearray == np.amax(imagearray))[1][0], 50, 50, 0])
        params = fitgaussianBG(imagedata, guessedParams)
        TmpFig.imshow(imagedata, cmap='rainbow')
        TmpFig.plot(params[2],params[1], '+')
        TmpFig.plot(guessedParams[2],guessedParams[1], 'r+')
        TmpFig.set_title(str(times1[k]*1e3)+'ms')
        TmpFig = axs[1,k]
        TmpFig.plot(imagedata[:,int(params[2])], 'r.')
        TmpFig.plot(np.linspace(0,xmax-xmin,100), gaussianBG(*params)(np.linspace(0,xmax-xmin,100),params[2]))
        TmpFig = axs[2,k]
        TmpFig.plot(imagedata[int(params[1]),:], 'r.')
        TmpFig.plot(np.linspace(0,ymax-ymin,100), gaussianBG(*params)(params[1],np.linspace(0,ymax-ymin,100)))
        lstXwidths.append(params[3])
        lstYwidths.append(params[4])
        lstXPos.append(params[1])
        lstYPos.append(params[2])

    np.array(lstXwidths)
    

    plt.figure()

    #converting widths from px to m
    SigX = 2*6.5e-6*(1/0.4)*np.array(lstXwidths)[1:]
    SigY = 2*6.5e-6*(1/0.4)*np.array(lstYwidths)[1:]
    times1 = times1[1:]
    #fit along x
    popt, pcov = curve_fit(linfunc, times1**2, (SigX)**2)
    print('Temperature along x is '+str(int(((87*1.66e-27/(1.38e-23))*popt[0])/(1e-6)))+' muK')
    #plot fit along x
    plt.plot(times1**2/(1e-6), (SigX)**2/(1e-6), 'r.' , label = 'x widths')
    plt.plot(np.linspace(0,max(times1**2))/(1e-6), linfunc(np.linspace(0,max(times1**2)),*popt)/(1e-6), 'r-', label = 'fit to x widths')
    #fit along y
    popt, pcov = curve_fit(linfunc, times1**2, (SigY)**2)
    print('Temperature along y is '+str(int(((87*1.66e-27/(1.38e-23))*popt[0])/(1e-6)))+' muK')
    #plot fit along y 
    plt.plot(times1**2/(1e-6), (SigY)**2/(1e-6), 'b.', label = 'y widths')
    plt.plot(np.linspace(0,max(times1**2))/(1e-6), linfunc(np.linspace(0,max(times1**2)),*popt)/(1e-6), 'b-', label = 'fit to y widths')
    plt.xlabel('squared expansion time in ms^2')
    plt.ylabel('squared gaussian RMS width mm^2')
    plt.legend()
    plt.show()
    
    print('x positions be like '+str(lstXPos))
    print('y positions be like '+str(lstYPos))
    
    GetTemperature.lstXPos = lstXPos
    GetTemperature.lstYPos = lstYPos
    GetTemperature.totodlst = totodlst
    
def GetTemperatureNew(path,imagename, seqno,numtrial, numavg,times1, imagecentre, cropsizeX, cropsizeY, crop):
    #path="C:\\Users\\cafmot\\Box Sync\\CaF MOT\\MOTData\\MOTMasterData\\CaF19Dec1900\\"
    imgsizex=512
    imgsizey=672

    #image crop:
    xmin = int(imagecentre[1]-cropsizeY/2)
    xmax = int(imagecentre[1]+cropsizeY/2)
    ymin = int(imagecentre[0]-cropsizeX/2)
    ymax = int(imagecentre[0]+cropsizeX/2)

    lstclouds=[];lstprobes=[];lstbgs=[];lstods=[];lstodavg=[];totodlst=[];errtotodlst=[];
    for i in range(1,numtrial+1):
        odavg=np.zeros((imgsizex,imgsizey));totod=[];
        for j in range(((i-1)*3*numavg)+1,i*3*numavg,3):
            clouds = plt.imread(path+imagename + str(seqno) + "_" + str(j) + ".tif")
            probes = plt.imread(path+imagename + str(seqno) + "_" + str(j+1) + ".tif")
            bgs = plt.imread(path+imagename + str(seqno) + "_" + str(j+2) + ".tif")
            lstclouds.append(clouds);
            lstprobes.append(probes);
            lstbgs.append(bgs);
            od=np.log((probes.astype(float)-bgs.astype(float))/(clouds.astype(float)-bgs.astype(float)))
            od[np.isnan(od)] = 0.0
            od[od == -np.inf] = 0.0
            od[od == np.inf] = 0.0
            lstods.append(od);
            odavg=odavg+od
            totod.append(sum(sum(od[xmin:xmax,ymin:ymax])))
        odavg=odavg/numavg
        lstodavg.append(odavg)
        totodlst.append(np.mean(totod))
        errtotodlst.append(np.std(totod))
    
    #run for image size
    #imagedimensions = lstodavg[1].shape



 



    lstXwidths = []
    lstYwidths = []
    lstXPos = []
    lstYPos = []
    

        
    fig, axs = plt.subplots(3,numtrial,figsize=(12,6))


    for k in range(0,numtrial):
        TmpFig = axs[0,k]
        
        if crop == True:
            imagedata = lstodavg[k][xmin:xmax,ymin:ymax]
        if crop == False:
            imagedata = lstodavg[k]
        
#        imagearray = np.array(imagedata)
#        guessedParams = np.array([np.amax(imagearray),np.where(imagearray == np.amax(imagearray))[0][0],np.where(imagearray == np.amax(imagearray))[1][0], 50, 50, 0])
        guessedParams = moments(imagedata)
        params = fitgaussian(imagedata, guessedParams)
        TmpFig.imshow(imagedata, cmap='rainbow')
        TmpFig.plot(params[2],params[1], '+')
        TmpFig.plot(guessedParams[2],guessedParams[1], 'r+')
        TmpFig.set_title(str(times1[k])+'s')
        TmpFig = axs[1,k]
        TmpFig.plot(imagedata[:,int(params[2])], 'r.')
        TmpFig.plot(np.linspace(0,xmax-xmin,100), gaussian(*params)(np.linspace(0,xmax-xmin,100),params[2]))
        TmpFig = axs[2,k]
        TmpFig.plot(imagedata[int(params[1]),:], 'r.')
        TmpFig.plot(np.linspace(0,ymax-ymin,100), gaussian(*params)(params[1],np.linspace(0,ymax-ymin,100)))
        lstXwidths.append(params[3])
        lstYwidths.append(params[4])
        lstXPos.append(params[1])
        lstYPos.append(params[2])
    

    plt.figure()

    #converting widths from px to m
    SigX = 2*6.5e-6*(1/0.4)*np.array(lstXwidths)[1:4]
    SigY = 2*6.5e-6*(1/0.4)*np.array(lstYwidths)[1:4]
    times1 = times1[1:4]
    #fit along x
    popt, pcov = curve_fit(linfunc, times1**2, (SigX)**2)
    print('Temperature along axial direction is '+str(int(((87*1.66e-27/(1.38e-23))*popt[0])/(1e-6)))+' muK')
    #plot fit along x
    plt.plot(times1**2/(1e-6), (SigX)**2/(1e-6), 'r.' , label = 'widths totally')
    plt.plot(np.linspace(0,max(times1**2))/(1e-6), linfunc(np.linspace(0,max(times1**2)),*popt)/(1e-6), 'r-', label = 'fit to axial widths')
    #fit along y
    popt, pcov = curve_fit(linfunc, times1**2, (SigY)**2)
    print('Temperature along radial direction is '+str(int(((87*1.66e-27/(1.38e-23))*popt[0])/(1e-6)))+' muK')
    #plot fit along y 
    plt.plot(times1**2/(1e-6), (SigY)**2/(1e-6), 'b.', label = 'widths radially')
    plt.plot(np.linspace(0,max(times1**2))/(1e-6), linfunc(np.linspace(0,max(times1**2)),*popt)/(1e-6), 'b-', label = 'fit to radial widths')
    plt.xlabel('squared expansion time in ms^2')
    plt.ylabel('squared gaussian RMS width mm^2')
    plt.legend()
    plt.show()
    
    print('x positions be like '+str(lstXPos))
    print('y positions be like '+str(lstYPos))
    
    GetTemperature.lstXPos = lstXPos
    GetTemperature.lstYPos = lstYPos
    GetTemperature.totodlst = totodlst
    
def GetPositions(path,imagename, seqno,numtrial, numavg,times1, imagecentre, cropsizeX, cropsizeY, crop):
    #path="C:\\Users\\cafmot\\Box Sync\\CaF MOT\\MOTData\\MOTMasterData\\CaF19Dec1900\\"
    imgsizex=512
    imgsizey=672

    #image crop:
    xmin = int(imagecentre[1]-cropsizeY/2)
    xmax = int(imagecentre[1]+cropsizeY/2)
    ymin = int(imagecentre[0]-cropsizeX/2)
    ymax = int(imagecentre[0]+cropsizeX/2)

    lstclouds=[];lstprobes=[];lstbgs=[];lstods=[];lstodavg=[];totodlst=[];errtotodlst=[];
    for i in range(1,numtrial+1):
        odavg=np.zeros((imgsizex,imgsizey));totod=[];
        for j in range(((i-1)*3*numavg)+1,i*3*numavg,3):
            clouds = plt.imread(path+imagename + str(seqno) + "_" + str(j) + ".tif")
            probes = plt.imread(path+imagename + str(seqno) + "_" + str(j+1) + ".tif")
            bgs = plt.imread(path+imagename + str(seqno) + "_" + str(j+2) + ".tif")
            lstclouds.append(clouds);
            lstprobes.append(probes);
            lstbgs.append(bgs);
            od=np.log((probes.astype(float)-bgs.astype(float))/(clouds.astype(float)-bgs.astype(float)))
            od[np.isnan(od)] = 0.0
            od[od == -np.inf] = 0.0
            od[od == np.inf] = 0.0
            lstods.append(od);
            odavg=odavg+od
            totod.append(sum(sum(od[xmin:xmax,ymin:ymax])))
        odavg=odavg/numavg
        lstodavg.append(odavg)
        totodlst.append(np.mean(totod))
        errtotodlst.append(np.std(totod))
    
    #run for image size
    #imagedimensions = lstodavg[1].shape



 



    lstXwidths = []
    lstYwidths = []
    lstXPos = []
    lstYPos = []
    

        
    fig, axs = plt.subplots(3,numtrial,figsize=(12,6))
    fig.suptitle(imagename +str(seqno), fontsize=14)


    for k in range(0,numtrial):
        TmpFig = axs[0,k]
        
        if crop == True:
            imagedata = lstodavg[k][xmin:xmax,ymin:ymax]
        if crop == False:
            imagedata = lstodavg[k]
        
#        imagearray = np.array(imagedata)
#        guessedParams = np.array([np.amax(imagearray),np.where(imagearray == np.amax(imagearray))[0][0],np.where(imagearray == np.amax(imagearray))[1][0], 50, 50, 0])
        guessedParams = moments(imagedata)
        params = fitgaussian(imagedata, guessedParams)
        TmpFig.imshow(imagedata, cmap='rainbow')
        TmpFig.plot(params[2],params[1], '+')
        TmpFig.plot(guessedParams[2],guessedParams[1], 'r+')
        TmpFig.set_title(str(times1[k])+'ms after ramp end')
        TmpFig = axs[1,k]
        TmpFig.plot(imagedata[:,int(params[2])], 'r.')
        TmpFig.plot(np.linspace(0,xmax-xmin,100), gaussian(*params)(np.linspace(0,xmax-xmin,100),params[2]))
        TmpFig = axs[2,k]
        TmpFig.plot(imagedata[int(params[1]),:], 'r.')
        TmpFig.plot(np.linspace(0,ymax-ymin,100), gaussian(*params)(params[1],np.linspace(0,ymax-ymin,100)))
        lstXwidths.append(params[3])
        lstYwidths.append(params[4])
        lstXPos.append(params[1])
        lstYPos.append(params[2])

    np.array(lstXwidths)
    

#    plt.figure()
#
#    plt.plot(times1, np.array(lstXPos)*2*6.5e-3/0.4, '--ro', label = ' max position axially')
#    plt.plot(times1, np.array(lstYPos)*2*6.5e-3/0.4, '--bo', label = ' max position radially')
#   
#    plt.xlabel('track position in mm')
#    plt.ylabel('position of maximum in mm')
#    plt.legend()
#    plt.show()
    print('axial position '+str(np.array(lstXPos)[0]*2*6.5e-3/0.4))
    print('radial position '+str(np.array(lstYPos)[0]*2*6.5e-3/0.4))
    
    fig, axs = plt.subplots(2,1,figsize=(6,6))
    fig.suptitle(imagename +str(seqno), fontsize=14)
    TmpFig = axs[0]
    Xpositions = np.array(lstXPos)*2*6.5e-3/0.4
    TmpFig.plot(times1, Xpositions, '--ro', label = ' max position axially')
    Ypositions = np.array(lstYPos)*2*6.5e-3/0.4
    TmpFig.plot(times1,Ypositions, '--bo', label = ' max position radially')
#    TmpFig.xticks(np.arange(-47, 383, 20))
    TmpFig.set_ylabel('cloud position in mm')
    

    TmpFig.legend()
    TmpFig = axs[1]
    TmpFig.plot(times1, np.array(lstXwidths)*2*6.5e-3/0.4, '--ro', label = ' width axially')
    TmpFig.plot(times1, np.array(lstYwidths)*2*6.5e-3/0.4, '--bo', label = ' width radially')
#    TmpFig.xticks(np.arange(-47, 383, 20))
    TmpFig.set_xlabel('wait time in ms')
    TmpFig.set_ylabel('cloud width in mm')

    TmpFig.legend()
    return  [Xpositions,Ypositions]

    
    
def PlotAbsImage(path, imagename, seqno,numtrial, numavg,times1, imagecentre, cropsizeX, cropsizeY, crop):
    #path="C:\\Users\\cafmot\\Box Sync\\CaF MOT\\MOTData\\MOTMasterData\\CaF19Dec1900\\"
    imgsizex=512
    imgsizey=672

    #image crop:
    xmin = int(imagecentre[1]-cropsizeY/2)
    xmax = int(imagecentre[1]+cropsizeY/2)
    ymin = int(imagecentre[0]-cropsizeX/2)
    ymax = int(imagecentre[0]+cropsizeX/2)

    lstclouds=[];lstprobes=[];lstbgs=[];lstods=[];lstodavg=[];totodlst=[];errtotodlst=[];
    for i in range(1,numtrial+1):
        odavg=np.zeros((imgsizex,imgsizey));totod=[];
        for j in range(((i-1)*3*numavg)+1,i*3*numavg,3):
            clouds = plt.imread(path+imagename + str(seqno) + "_" + str(j) + ".tif")
            probes = plt.imread(path+imagename + str(seqno) + "_" + str(j+1) + ".tif")
            bgs = plt.imread(path+imagename + str(seqno) + "_" + str(j+2) + ".tif")
            lstclouds.append(clouds);
            lstprobes.append(probes);
            lstbgs.append(bgs);
            od=np.log((probes.astype(float)-bgs.astype(float))/(clouds.astype(float)-bgs.astype(float)))
            od[np.isnan(od)] = 0.0
            od[od == -np.inf] = 0.0
            od[od == np.inf] = 0.0
            lstods.append(od);
            odavg=odavg+od
            totod.append(sum(sum(od[xmin:xmax,ymin:ymax])))
        odavg=odavg/numavg
        lstodavg.append(odavg)
        totodlst.append(np.mean(totod))
        errtotodlst.append(np.std(totod))
        
    
    
    #run for image size
    #imagedimensions = lstodavg[1].shape
#        plt.figure()
#        if crop == False:
#            imageData = lstodavg
#        if crop == True:
#            imageData = lstodavg[xmin:xmax,ymin:ymax]
#        plt.imshow(imageData, cmap='rainbow')
#        plt.title.set_text(str(times1)+' ms')
        

    fig, axs = plt.subplots(1,numtrial,figsize=(12,4))
    fig.suptitle(imagename +str(seqno), fontsize=14)
    for k in range(0,numtrial):
        TmpFig = axs[k]
        if crop == False:
            imageData = lstodavg[k]
        if crop == True:
            imageData = lstodavg[k][xmin:xmax,ymin:ymax]
        TmpFig.imshow(imageData, cmap='rainbow')
        TmpFig.title.set_text(str(times1[k])+' ms')
        
#        TmpFig = axs[1,k]
#        TmpFig.plot(range(1,len(imageData[:,int(0.5*(xmax-xmin))])+1),imageData[:,int(0.5*(xmax-xmin))], 'r.')
#        TmpFig = axs[2,k]
#        TmpFig.plot(range(1,len(imageData[int(0.5*(ymax-ymin)),:])+1),imageData[int(0.5*(ymax-ymin)),:], 'r.')
    
#    fig, axs = plt.subplots(3,1,figsize=(12,6))
#    fig.suptitle(imagename +str(seqno), fontsize=16)
#    TmpFig = axs[0]
#    imageData = lstodavg[0][xmin:xmax,ymin:ymax]
#    TmpFig.imshow(imageData, cmap='rainbow')
#    TmpFig = axs[1]
#    TmpFig.plot(range(1,len(imageData[:,int(0.5*(xmax-xmin))])+1),imageData[:,int(0.5*(xmax-xmin))], 'r.')
#    TmpFig = axs[2]
#    TmpFig.plot(range(1,len(imageData[int(0.5*(ymax-ymin)),:])+1),imageData[int(0.5*(ymax-ymin)),:], 'r.')
        

def PlotFluoImage(path,imagename, seqno,numimage, imagecentre, cropsizeX, cropsizeY, crop):

  
    xmin = int(imagecentre[1]-cropsizeY/2)
    xmax = int(imagecentre[1]+cropsizeY/2)
    ymin = int(imagecentre[0]-cropsizeX/2)
    ymax = int(imagecentre[0]+cropsizeX/2)
    clouds = plt.imread(path+imagename + str(seqno) + "_" + str(numimage) + ".tif")
    if crop == True:
        imageData = clouds[xmin:xmax,ymin:ymax]
    if crop == False:
        imageData = clouds

    plt.figure()
    plt.imshow(imageData, cmap='rainbow')
    plt.show()