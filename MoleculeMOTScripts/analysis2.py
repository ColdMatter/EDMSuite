#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Created on Wed Jun 19 07:43:49 2019

@author: arijit
"""

import numpy as np
import os,zipfile
from PIL import Image
from scipy.optimize import curve_fit
import matplotlib.pyplot as plt
from matplotlib.ticker import AutoMinorLocator
import scipy.constants as cn
import re

def atoi(text):
    return int(text) if text.isdigit() else text

def natural_keys(text):
    return [atoi(c) for c in re.split(r'(\d+)', text) ]

def injector(fileNoStart,fileNoStop,NoImages,
             fileNameString="CaF18Jul1900",
             remotePath="//PH-TEW105/Users/rfmot/Desktop/AbsImages/",
             dirPath="C:/Users/cafmot/Box Sync/CaF MOT/MOTData/MOTMasterData/"):
    imgs=os.listdir(remotePath)
    imgs.sort(key=natural_keys)
    if len(imgs)==(fileNoStop-fileNoStart+1)*NoImages:
        print 'Inserting images to the zip files...'
        l=0
        for fileNo in range(fileNoStart,fileNoStop+1):
            filepath=os.path.join(dirPath,fileNameString+'_'+str(fileNo).zfill(3)+'.zip')
            with zipfile.ZipFile(filepath, 'a') as archive:
                files=archive.namelist()
                for _ in range(NoImages):
                    if imgs[l] not in files:
                        archive.write(os.path.join(remotePath,imgs[l]),imgs[l])
                        l+=1
        for img in imgs:
            os.remove(os.path.join(remotePath,img))
    elif len(imgs)==0:
        print 'No Image to insert'
    elif len(imgs)<(fileNoStart-fileNoStop+1)*NoImages:
        print 'There seems to be less number of images than required!'
    elif len(imgs)>(fileNoStart-fileNoStop+1)*NoImages:
        print 'There are more images than expected!'

def gaussianFit(x,y):
    f= lambda x,a,c,s,o: a*np.exp(-(x-c)**2/(2*s**2))+o
    loc_trial=np.argmax(y)
    a_trial=y[loc_trial]
    c_trial=x[loc_trial]
    o_trial=np.min(y)
    s_trial=np.sqrt(np.abs(((x[int(loc_trial+4)]-c_trial)**2-\
                            (x[int(loc_trial)]-c_trial)**2)/\
                (2*np.log(np.abs(y[int(loc_trial+4)]/y[int(loc_trial)])))))
    popt,_=curve_fit(f,x,y,p0=[a_trial,c_trial,s_trial,o_trial])
    return f,popt[0],popt[1],popt[2],popt[3]

def gaussianFitX(x,y):
    f= lambda x,a,c,s,o: a*np.exp(-(x-c)**2/(2*s**2))+o
    loc_trial=np.argmax(y)
    a_trial=y[loc_trial]
    c_trial=x[loc_trial]
    o_trial=np.min(y)
    d = np.abs((y-o_trial)-(a_trial-o_trial)/2.0)<(a_trial-o_trial)/10.0
    indexes = np.where(d > 0)
    s_trial=(x[indexes[0][-1]]-x[indexes[0][0]])/2
    popt,_=curve_fit(f,x,y,p0=[a_trial,c_trial,s_trial,o_trial])
    return f,popt[0],popt[1],popt[2],popt[3]

def Absorption(fileNoStart,fileNoStop,param,detuningInVolt,crop,
               centre,width,height,fileNameString,showPlot=True,showOd=False,
               dirPath="C:/Users/cafmot/Box Sync/CaF MOT/MOTData/MOTMasterData/"):
    analysis=defaultCaF()
    analysis.dirPath=dirPath
    analysis.fileNameString=fileNameString
    N_mean_list=[]
    N_std_list=[]
    paramVals=[]
    for fileNo in range(fileNoStart,fileNoStop+1):
        images,paramsDict=analysis.readFromZip(fileNo,dstr='R')
        paramVals.append(paramsDict[param])
        clouds=images[0::3,:,:]
        probes=images[1::3,:,:]
        bgs=images[2::3,:,:]
        od=np.log((probes-bgs)/(clouds-bgs))
        od[np.isnan(od)] = 0.0
        od[od == -np.inf] = 0.0
        od[od == np.inf] = 0.0
        if crop:
            od=od[:,centre[0]-height/2:centre[0]+height/2,
                    centre[1]-width/2:centre[1]+width/2]
        N=(1+4*(detuningInVolt*14.7)**2/36)*(2.4*4*6.4e-6)**2*np.sum(od,axis=(1,2))/(3*780e-9**2/(2*np.pi))
        N_mean_list.append(np.mean(N))
        N_std_list.append(np.std(N)/np.sqrt(len(N)))
        if showOd:
            fig,ax=plt.subplots()
            im=ax.imshow(np.mean(od,axis=0))
            fig.colorbar(im)
    if showPlot:
        fig,ax=plt.subplots()
        ax.errorbar(np.array(paramVals),np.array(N_mean_list),
                    yerr=np.array(N_std_list),
                    fmt='ok')
        ax.set_ylabel('MOT Number')
        ax.set_xlabel(param)
    return np.array(N_mean_list),\
        np.array(N_std_list),\
        np.array(paramVals)

def AbsorptionDensity(fileNoStart,fileNoStop,param,detuningInVolt,crop,
               centre,width,height,fileNameString,showPlot=True,showOd=False,showFits=False,
               dirPath="C:/Users/cafmot/Box Sync/CaF MOT/MOTData/MOTMasterData/"):
    analysis=defaultCaF()
    analysis.dirPath=dirPath
    analysis.fileNameString=fileNameString
    radialSigmas=[]
    axialSigmas=[]
    paramVals=[]
    densities_mean_list=[]
    densities_std_list=[]
    pixelSize=6.4e-6
    binSize=4
    mag=0.416
    for fileNo in range(fileNoStart,fileNoStop+1):
        images,paramsDict=analysis.readFromZip(fileNo,dstr='R')
        paramVals.append(paramsDict[param])
        clouds=images[0::3,:,:]
        probes=images[1::3,:,:]
        bgs=images[2::3,:,:]
        l,m,p=np.shape(probes)
        binProbes=probes#.reshape((l,m/2,2,p/2,2)).sum(2).sum(3)
        binClouds=clouds#.reshape((l,m/2,2,p/2,2)).sum(2).sum(3)
        binBgs=bgs#.reshape((l,m/2,2,p/2,2)).sum(2).sum(3)
        od=np.log((binProbes-binBgs)/(binClouds-binBgs))
        od[np.isnan(od)] = 0.0
        od[od == -np.inf] = 0.0
        od[od == np.inf] = 0.0
        if crop:
            od=od[:,centre[0]-height/2:centre[0]+height/2,
                    centre[1]-width/2:centre[1]+width/2]
        od_mean=np.mean(od,axis=0)
        N=(1+4*(detuningInVolt*14.7)**2/36)*(2.4*4*6.4e-6)**2*np.sum(od,axis=(1,2))/(3*780e-9**2/(2*np.pi))
        radialY=np.sum(od_mean,axis=0)
        axialY=np.sum(od_mean,axis=1)
        radialYLength=len(radialY)
        axialYLength=len(axialY)
        radialX=pixelSize*(binSize/mag)*np.arange(0,radialYLength)
        axialX=pixelSize*(binSize/mag)*np.arange(0,axialYLength)        
        radialGaussian,radialA,radialC,radialSigma,radiaOffset=gaussianFit(radialX,radialY)
        axialGaussian,axialA,axialC,axialSigma,axialOffset=gaussianFit(axialX,axialY)
        radialSigmas.append(np.abs(radialSigma))
        axialSigmas.append(np.abs(axialSigma))
        density=(1e-6*N/((2*np.pi)**(1.5)*np.abs(axialSigma)*radialSigma**2))
        densities_mean_list.append(np.mean(density))
        densities_std_list.append(np.std(density)/np.sqrt(len(density)))
        if showOd:
            fig,ax=plt.subplots()
            im=ax.imshow(od_mean)
            fig.colorbar(im)
            print ''
        if showFits:
            fig, ax = plt.subplots(1,1)
            ax.plot(radialX,radialY,'ob')
            ax.plot(axialX,axialY,'og')
            ax.plot(radialX,radialGaussian(radialX,radialA,radialC,radialSigma,radiaOffset),'-r')
            ax.plot(axialX,axialGaussian(axialX,axialA,axialC,axialSigma,axialOffset),'-k')
            plt.show()
    densities_mean_list=np.array(densities_mean_list)
    densities_std_list=np.array(densities_std_list)
    paramVals=np.array(paramVals)

    fig, ax = plt.subplots()
    ax.errorbar(paramVals,densities_mean_list,yerr=densities_std_list, fmt='ok')
    return np.array(radialSigmas), np.array(axialSigmas)

def AbsorptionTemperature(fileNoStart,fileNoStop,param,detuningInVolt,crop,
               centre,width,height,fileNameString,showPlot=True,showOd=False,showFits=False,
               dirPath="C:/Users/cafmot/Box Sync/CaF MOT/MOTData/MOTMasterData/"):
    analysis=defaultCaF()
    analysis.dirPath=dirPath
    analysis.fileNameString=fileNameString
    radialSigmas=[]
    axialSigmas=[]
    paramVals=[]
    pixelSize=6.4e-6
    binSize=4
    mag=0.39
    for fileNo in range(fileNoStart,fileNoStop+1):
        images,paramsDict=analysis.readFromZip(fileNo,dstr='R')
        paramVals.append(paramsDict[param])
        clouds=images[0::3,:,:]
        probes=images[1::3,:,:]
        bgs=images[2::3,:,:]
        l,m,p=np.shape(probes)
        binProbes=probes#.reshape((l,m/2,2,p/2,2)).sum(2).sum(3)
        binClouds=clouds#.reshape((l,m/2,2,p/2,2)).sum(2).sum(3)
        binBgs=bgs#.reshape((l,m/2,2,p/2,2)).sum(2).sum(3)
        od=np.log((binProbes-binBgs)/(binClouds-binBgs))
        od[np.isnan(od)] = 0.0
        od[od == -np.inf] = 0.0
        od[od == np.inf] = 0.0
        if crop:
            od=od[:,centre[0]-height/2:centre[0]+height/2,
                    centre[1]-width/2:centre[1]+width/2]
        od_mean=np.mean(od,axis=0)
        radialY=np.sum(od_mean,axis=0)
        axialY=np.sum(od_mean,axis=1)
        radialYLength=len(radialY)
        axialYLength=len(axialY)
        radialX=pixelSize*(binSize/mag)*np.arange(0,radialYLength)
        axialX=pixelSize*(binSize/mag)*np.arange(0,axialYLength)        
        radialGaussian,radialA,radialC,radialSigma,radiaOffset=gaussianFit(radialX,radialY)
        axialGaussian,axialA,axialC,axialSigma,axialOffset=gaussianFit(axialX,axialY)
        radialSigmas.append(radialSigma)
        axialSigmas.append(axialSigma)
        if showOd:
            fig,ax=plt.subplots()
            im=ax.imshow(od_mean)
            fig.colorbar(im)
        if showFits:
            fig, ax = plt.subplots(1,1)
            ax.plot(radialX,radialY,'ob')
            ax.plot(axialX,axialY,'og')
            ax.plot(radialX,radialGaussian(radialX,radialA,radialC,radialSigma,radiaOffset),'-r')
            ax.plot(axialX,axialGaussian(axialX,axialA,axialC,axialSigma,axialOffset),'-k')
            plt.show()
    axialSigmas=np.array(axialSigmas)
    radialSigmas=np.array(radialSigmas)
    paramVals=np.array(paramVals)*1e-5
    axialLin,axialM,axialC=analysis.linearFit(paramVals**2,axialSigmas**2)
    radialLin,radialM,radialC=analysis.linearFit(paramVals**2,radialSigmas**2)
    axialTemp=axialM*(86.9*cn.u/cn.k)*1e3
    radialTemp=radialM*(86.9*cn.u/cn.k)*1e3
    timeValsInterpolated=np.linspace(np.min(paramVals),
                                         np.max(paramVals),
                                         100)

    fig, ax = plt.subplots(1,2)
    ax[0].plot(paramVals**2*1e6,radialSigmas**2*1e6,'ok')
    ax[1].plot(paramVals**2*1e6,axialSigmas**2*1e6,'ok')
    ax[0].plot(timeValsInterpolated**2*1e6,
                radialLin(timeValsInterpolated**2,radialM,
                                    radialC)*1e6,'-r')
    ax[1].plot(timeValsInterpolated**2*1e6,
                axialLin(timeValsInterpolated**2,axialM,axialC)*1e6,'-r')
    ax[0].set_title('Tr: {0:2.4f} [mK]'.format(radialTemp))
    ax[1].set_title('Ta: {0:2.4f} [mK]'.format(axialTemp))
    ax[1].yaxis.tick_right()
    ax[1].yaxis.set_label_position("right")
    for axis in ax:
        axis.xaxis.set_minor_locator(AutoMinorLocator())
        axis.yaxis.set_minor_locator(AutoMinorLocator())
        axis.set_xlabel('time^2 [ms^2]')
        axis.set_ylabel('size^2 [mm^2]')

def gaussianFit2D((x, y), amplitude, xo, yo, sigma_x, sigma_y, theta, offset):
    xo = float(xo)
    yo = float(yo)    
    a = (np.cos(theta)**2)/(2*sigma_x**2) + (np.sin(theta)**2)/(2*sigma_y**2)
    b = -(np.sin(2*theta))/(4*sigma_x**2) + (np.sin(2*theta))/(4*sigma_y**2)
    c = (np.sin(theta)**2)/(2*sigma_x**2) + (np.cos(theta)**2)/(2*sigma_y**2)
    g = offset + amplitude*np.exp( - (a*((x-xo)**2) + 2*b*(x-xo)*(y-yo) + c*((y-yo)**2)))
    return g.ravel()

def getInitialGuesses(od,pixelSize,binSize,magFactor):
    amplitude=np.max(od)
    offset=np.min(od)
    x=np.sum(od,axis=0)
    xd=np.arange(0,len(x))*pixelSize*(binSize/magFactor)
    y=np.sum(od,axis=1)
    yd=np.arange(0,len(y))*pixelSize*(binSize/magFactor)
    xo=np.argmax(x)
    yo=np.argmax(y)
    return (amplitude,xo,yo,20*pixelSize*(binSize/magFactor),
            10*pixelSize*(binSize/magFactor),0,offset)

def getOdFitted(od,pixelSize,binSize,magFactor):
    l,m,n=np.shape(od)
    odFitted=np.zeros_like(od)
    f=pixelSize*(binSize/magFactor)
    x = np.arange(0, n)*f
    y = np.arange(0, m)*f
    x, y = np.meshgrid(x, y)   
    popts=[]
    for i in range(l):
        p0 = (1,30*f,40*f,20*f,10*f,0,0)
        popt, _ = curve_fit(gaussianFit2D, (x, y), od[i,:,:].reshape((m*n)),p0=p0)
        odFitted[i,:,:] = gaussianFit2D((x, y), *popt).reshape((m,n))
        popts.append(popt)
    return odFitted,np.array(popts)

def getOdCleaned(probes,clouds,bgs):
    probes=probes-bgs
    clouds=clouds-bgs
    clouds[clouds<=0]=1.0
    od=np.log(probes/clouds)
    od[np.isnan(od)] = 0.0
    od[od == -np.inf] = 0.0
    od[od == np.inf] = 0.0
    return od

def imshowArray(ifarray,array):
    if ifarray:
        fig,ax=plt.subplots()
        im=ax.imshow(np.mean(array,axis=0))
        fig.colorbar(im)
        plt.show()


def AbsorptionAnalysis(fileNoStart,fileNoStop,param,detuningInVolt,crop,
               centre,width,height,fileNameString,
               numberByFit=True,
               showFits=True,
               showOd=False,
               pixelSize=6.4e-6,
               binSize=4,
               magFactor=0.41,
               dirPath="C:/Users/cafmot/Box Sync/CaF MOT/MOTData/MOTMasterData/"):
    returnDict={}
    analysis=defaultCaF()
    analysis.dirPath=dirPath
    analysis.fileNameString=fileNameString
    s0=(1+4*(detuningInVolt*14.7)**2/36)/(3*780e-9**2/(2*np.pi))
    paramVals=[]; amplitudesMean=[]; sigmas_xMean=[]; sigmas_yMean=[]
    xosMean=[]; yosMean=[]; numbersMean=[]; numbersStd=[]; amplitudesStd=[]
    sigmas_xStd=[]; sigmas_yStd=[]; xosStd=[]; yosStd=[];ods=[]
    for fileNo in range(fileNoStart,fileNoStop+1):
        images,paramsDict=analysis.readFromZip(fileNo,dstr='R')
        paramVals.append(paramsDict[param])
        clouds=images[0::3,:,:]
        probes=images[1::3,:,:]
        bgs=images[2::3,:,:]
        od=getOdCleaned(probes,clouds,bgs)
        if crop:
            od=od[:,centre[0]-height/2:centre[0]+height/2,
                    centre[1]-width/2:centre[1]+width/2]
        imshowArray(showOd,od)
        ods.append(np.mean(od,axis=0))
        if numberByFit:
            odFitted,popt=getOdFitted(od,pixelSize,binSize,magFactor)
            imshowArray(showFits,odFitted)
            l=np.sqrt(len(popt[:,0]))
            amplitude=popt[:,0]
            xo=popt[:,1]
            yo=popt[:,2]
            sigma_x=popt[:,3]
            sigma_y=popt[:,4]
            N=(2*np.pi)*amplitude*np.abs(sigma_x)*np.abs(sigma_y)*s0
            amplitudesMean.append(np.mean(amplitude))
            xosMean.append(np.mean(xo))
            yosMean.append(np.mean(yo))
            sigmas_xMean.append(np.mean(sigma_x))
            sigmas_yMean.append(np.mean(sigma_y))
            amplitudesStd.append(np.std(amplitude)/l)
            xosStd.append(np.std(xo)/l)
            yosStd.append(np.std(yo)/l)
            sigmas_xStd.append(np.std(sigma_x)/l)
            sigmas_yStd.append(np.std(sigma_y)/l)
        else:
            N=(pixelSize*(binSize/magFactor))**2*np.sum(od,axis=(1,2))*s0
        numbersMean.append(np.mean(N))
        numbersStd.append(np.std(N)/np.sqrt(len(N)))

    returnDict['N_mean']=np.array(numbersMean)
    returnDict['N_std']=np.array(numbersStd)
    returnDict['paramVals']=np.array(paramVals)
    returnDict['ods']=np.array(ods)
    if numberByFit:
        returnDict['fitSigmas_xMean']=np.array(sigmas_xMean)
        returnDict['fitSigmas_xStd']=np.array(sigmas_xStd)
        returnDict['fitSigmas_yMean']=np.array(sigmas_yMean)
        returnDict['fitSigmas_yStd']=np.array(sigmas_yStd)
        returnDict['fitAmplitudesMean']=np.array(amplitudesMean)
        returnDict['fitAmplitudesStd']=np.array(amplitudesStd)
        returnDict['fitXosMean']=np.array(xosMean)
        returnDict['fitXosStd']=np.array(xosStd)
        returnDict['fitYosMean']=np.array(yosMean)
        returnDict['fitYosStd']=np.array(yosStd)
    return returnDict





def expFit(x,y):
    f= lambda x,a,c,s: a*np.exp(-(x-c)/s)
    a_trial=np.max(y)
    c_trial=0#x[0]
    s_trial=100
    popt,_=curve_fit(f,x,y,p0=[a_trial,c_trial,s_trial])
    return f,popt[0],popt[1],popt[2]







class Analysis:
    """
    This is the analysis object for CaF and Rb MOT \n
    Input \n
    fileNoStart=starting No of the files to be analysed \n,
    fileNoStop=ending No of the files to be analysed \n,
    fileNoBG=file No of the file with background \n,
    requirement=allows a switch to select from \n
        'Image' : To get the images of all the files \n
        'Number': To get the number variation of all the files\n
        'Temperature' : To get the temperature from the expansion set \n
        'Lifetime': to get the lifetime from the dataset\n
    param=parameter of the variation\n
    fit=True to fit the data points\n
    fitType=type of fitting if fit is true, choose from\n
        'exp': for exponential fit [y=a*exp(-(x-c)/s)]\n
        'lin': for linear fit [y=m*x+c]\n
        'gauss': for gaussian fit [y=a*exp(-(x-c)**2/(2*s**2))]
    trigType=choose from\n
        'single': for single trigger images\n
        'double': for double trigger normalizations\n
    N_interpolate=integer for number of points in the fitted curve\n
    fmt=plotting format, default is 'ok'\n,
    showFits=True if want to have the gaussian fit to the cloud data\n
    imageCols=integer for number of coumns for 'Image' or showFits\n
    """
    def __init__(self,args={}):
        for key in args:
            self.__dict__[key]=args[key]
        
    def __setattr__(self,name,value):
        self.__dict__[name]=value

    def getFilepath(self,fileNo):
        """This method create the full filepath from the fileNo input
        """
        return os.path.join(self.dirPath,
                            self.fileNameString+'_'+str(fileNo).zfill(3)+'.zip')
        
    def convertRawToCount(self,raw):
        return (2**self.bitDepth-1)*raw
    
    def convertCountsToPhotons(self,counts): 
        return counts*(np.float(self.fullWellCapacity)/(2**self.bitsPerChannel-1))/self.etaQ
    
    def convertPhotonsToNumber(self,photonCount):
        return photonCount/(self.exposureTime*self.gamma*self.collectionSolidAngle)
    
    def readFromZip(self,fileNo,dstr=''):
        archive=zipfile.ZipFile(self.getFilepath(fileNo),'r')
        imgs=[]
        files=archive.namelist()
        files.sort(key=natural_keys)
        for f in files:
            if f[-3:]=='tif':
                if dstr=='':
                    with archive.open(f) as filename:
                        imgs.append(np.array(Image.open(filename),dtype=float))
                if dstr=='R':
                    if f[0]=='R':
                        with archive.open(f) as filename:
                            imgs.append(np.array(Image.open(filename),dtype=float))
                if dstr=='C':
                    if f[0]=='C':
                        with archive.open(f) as filename:
                            imgs.append(np.array(Image.open(filename),dtype=float))
            if f[-14:]=='parameters.txt':
                with archive.open(f) as filename:
                    scriptParams=filename.readlines()
            if f[-18:]=='hardwareReport.txt':
                with archive.open(f) as filename:
                    hardwareParams=filename.readlines()
        tempDict={}
        for param in scriptParams:
            paramSplit=param.split(b'\t')
            tempDict[paramSplit[0]]=np.float(paramSplit[1])
        for param in hardwareParams:
            paramSplit=param.split(b'\t')
            tempDict[paramSplit[0]]=np.float(paramSplit[1]) if \
                                paramSplit[1].isdigit() else paramSplit[1]
        paramDict={}
        for key in tempDict:
            paramDict[key.decode("utf-8")]=tempDict[key]
        return np.array(imgs),paramDict

    def getImagesFromTwoTriggerData(self,fileNo):
        imgs,paramsDict=self.readFromZip(fileNo,dstr='C')
        return imgs[::2,:,:],imgs[1::2,:,:],paramsDict
    
    def getAvgImageFromTwoTriggerData(self,fileNo):
        normImages,measImages,_=self.getImagesFromTwoTriggerData(fileNo)
        return np.mean(normImages,axis=0),np.mean(measImages,axis=0)
    
    def getImagesFromOneTriggerData(self,fileNo):
        imgs,paramsDict=self.readFromZip(fileNo,dstr='C')
        return imgs,paramsDict
    
    def getAvgImageFromOneTriggerData(self,fileNo):
        imgs,_=self.getImagesFromOneTriggerData(fileNo)
        return np.mean(imgs,axis=0)
    
    def cropImages(self,imageArray):
        h_top=int(self.cropCentre[0]-self.cropHeight/2)
        h_bottom=int(self.cropCentre[0]+self.cropHeight/2)
        w_left=int(self.cropCentre[1]-self.cropWidth/2)
        w_right=int(self.cropCentre[1]+self.cropWidth/2)
        return imageArray[:,h_top:h_bottom,w_left:w_right]
    
    def cropSingleImages(self,imageArray):
        h_top=self.cropCentre[1]-self.cropHeight/2
        h_bottom=self.cropCentre[1]+self.cropHeight/2
        w_left=self.cropCentre[0]-self.cropWidth/2
        w_right=self.cropCentre[0]+self.cropWidth/2
        return imageArray[w_left:w_right,h_top:h_bottom]

    def getMOTNumber(self,imageArray):
        totalCount=np.sum(self.cropImages(imageArray),axis=(1,2))
        totalMolecules=self.convertPhotonsToNumber(
                       self.convertCountsToPhotons(totalCount))
        return totalMolecules
    
    def singleImageNumberWithBG(self,fileNo,fileNoBG,
                                     param='Frame0Trigger'):
        imagesBG,_=self.readFromZip(fileNoBG,dstr='C')
        images,paramsDict=self.readFromZip(fileNo,dstr='C')
        imageSubBG=images-imagesBG
        imageCropped=imageSubBG#self.cropImages(imageSubBG)
        numbers=self.getMOTNumber(imageCropped)
        return np.mean(numbers),\
               np.std(numbers)/np.sqrt(len(numbers)),\
               paramsDict[param]

    def singleImageNumberRange(self,fileNoStart,fileNoStop,fileNoBG,
                               param='Frame0Trigger'):
        meanNoList=[]
        stdNoList=[]
        paramsValList=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            meanNo,stdNo,paramsVal=\
            self.singleImageNumberWithBG(fileNo,fileNoBG,param)
            meanNoList.append(meanNo)
            stdNoList.append(stdNo)
            paramsValList.append(paramsVal)
        paramsValListSorted=np.sort(paramsValList)
        paramsValListSortIndex=np.argsort(paramsValList)
        meanNoListSorted=np.array(meanNoList)[paramsValListSortIndex]
        stdNoListSorted=np.array(stdNoList)[paramsValListSortIndex]
        return meanNoListSorted,stdNoListSorted,paramsValListSorted

    def twoImageNormalisedNumberWithBG(self,fileNo,fileNoBG,
                                       param='Frame0Trigger'):
        avgNormImageBG,avgMeasImageBG=self.getAvgImageFromTwoTriggerData(fileNoBG)
        normImages,measImages,paramsDict=self.getImagesFromTwoTriggerData(fileNo)
        normImagesSubBG=normImages-avgNormImageBG
        measImagesSubBG=measImages-avgMeasImageBG
        normNums=self.getMOTNumber(normImagesSubBG[1:])
        measNums=self.getMOTNumber(measImagesSubBG[1:])
        propsTrapped=measNums/normNums
        return np.mean(propsTrapped),\
               np.std(propsTrapped)/np.sqrt(len(propsTrapped)),\
               paramsDict[param]
    
    def twoImageNormalisedNumberRange(self,fileNoStart,fileNoStop,fileNoBG,
                                      param='Frame0Trigger'):
        meanNoList=[]
        stdNoList=[]
        paramsValList=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            meanNo,stdNo,paramsVal=self.twoImageNormalisedNumberWithBG(fileNo,
                                                fileNoBG,param)
            meanNoList.append(meanNo)
            stdNoList.append(stdNo)
            paramsValList.append(paramsVal)
        paramsValListSorted=np.sort(paramsValList)
        paramsValListSortIndex=np.argsort(paramsValList)
        paramsValListSorted=np.array(paramsValList)
        meanNoListSorted=np.array(meanNoList)#[paramsValListSortIndex]
        stdNoListSorted=np.array(stdNoList)#[paramsValListSortIndex]
        return meanNoListSorted,stdNoListSorted,paramsValListSorted

    def linearFit(self,x,y):
        f= lambda x,m,c: m*x+c
        m_trial=(y[-1]-y[0])/(x[-1]-x[0])
        c_trial=np.max(y) if m_trial<0 else np.min(y)
        popt,_=curve_fit(f,x,y,p0=[m_trial,c_trial])
        return f,popt[0],popt[1]
    
    def expFit(self,x,y):
        f= lambda x,a,c,s: a*np.exp(-(x-c)/s)
        a_trial=np.max(y)
        c_trial=x[np.argmax(y)]
        s_trial=np.abs((x[-1]-x[0])/np.log(np.abs(y[-1]/y[0])))
        popt,_=curve_fit(f,x,y,p0=[a_trial,c_trial,s_trial])
        return f,popt[0],popt[1],popt[2]

    def expFitOffset(self,x,y):
        f= lambda x,a,c,s,o: a*np.exp(-(x-c)/s)+o
        a_trial=np.max(y)
        o_trial=np.min(y)
        c_trial=x[np.argmax(y)]
        s_trial=np.abs((x[-1]-x[0])/np.log(np.abs(y[-1]/y[0])))
        popt,_=curve_fit(f,x,y,p0=[a_trial,c_trial,s_trial,o_trial])
        return f,popt[0],popt[1],popt[2],popt[3]

    def gaussianFit(self,x,y):
        f= lambda x,a,c,s: a*np.exp(-(x-c)**2/(2*s**2))
        loc_trial=np.argmax(y)
        a_trial=y[loc_trial]
        c_trial=x[loc_trial]
        s_trial=np.sqrt(np.abs(((x[int(loc_trial+4)]-c_trial)**2-\
                                (x[int(loc_trial)]-c_trial)**2)/\
                    (2*np.log(np.abs(y[int(loc_trial+4)]/y[int(loc_trial)])))))
        popt,_=curve_fit(f,x,y,p0=[a_trial,c_trial,s_trial])
        return f,popt[0],popt[1],popt[2]

    def numberFit(self,meanNos,paramVals,fitType,N_interpolate):
        valdict={}
        valdict['paramValsInterpolated']=np.linspace(np.min(paramVals),
                                               np.max(paramVals),N_interpolate)
        if fitType=='lin':
            valdict['numberLin'],valdict['m'],valdict['c']=\
            self.linearFit(paramVals,meanNos)
            valdict['meanNosInterpolated']=\
            valdict['numberLin'](valdict['paramValsInterpolated'],
                   valdict['m'],valdict['c'])
        elif fitType=='exp':
            valdict['numberExp'],valdict['a'],valdict['c'],valdict['s']=\
            self.expFit(paramVals,meanNos)
            valdict['meanNosInterpolated']=\
            valdict['numberExp'](valdict['paramValsInterpolated'],
                   valdict['a'],valdict['c'],valdict['s'])
        elif fitType=='gauss':
            valdict['numberGauss'],valdict['a'],valdict['c'],valdict['s']=\
            self.gaussianFit(paramVals,meanNos)
            valdict['meanNosInterpolated']=\
            valdict['numberGauss'](valdict['paramValsInterpolated'],
                   valdict['a'],valdict['c'],valdict['s'])
        return valdict

    def number(self,fileNoStart,fileNoStop,fileNoBG,param,trigType,
                 fit,fmt,fitType,N_interpolate,extParam,extParamVals):
        valdict={}
        if trigType=='single':
            meanNos,stdNos,paramVals=\
            self.singleImageNumberRange(fileNoStart,fileNoStop,fileNoBG,param)
        else:
            meanNos,stdNos,paramVals=\
            self.twoImageNormalisedNumberRange(fileNoStart,fileNoStop,
                                           fileNoBG,param)
        '''fig, ax = plt.subplots()
        if len(extParamVals):
            paramVals=np.array(extParamVals)
            param=extParam
        ax.errorbar(paramVals,meanNos,yerr=stdNos,fmt=fmt)
        if fit:
            valdictFit=self.numberFit(meanNos,paramVals,fitType,
                                        N_interpolate)
            valdict.update(valdictFit)
            ax.plot(valdictFit['paramValsInterpolated'],
                    valdictFit['meanNosInterpolated'],'-r')
        ax.xaxis.set_minor_locator(AutoMinorLocator())
        ax.yaxis.set_minor_locator(AutoMinorLocator())
        ax.set_xlabel(param)
        ax.set_ylabel('MOT number')
        plt.show()'''
        valdict['meanNos']=meanNos
        valdict['paramVals']=paramVals
        valdict['stdNos']=stdNos
        return valdict

    def gaussianFitToCloud(self,imageArray):
        valdict={}
        peakPos=np.unravel_index(np.argmax(imageArray, axis=None), 
                                 imageArray.shape)
        radialXLength=len(imageArray[peakPos[0],:])
        axialXLength=len(imageArray[:,peakPos[1]])
        radialX=self.pixelSize*1e6*np.arange(-radialXLength/2.0,
                                        radialXLength/2.0)
        axialX=self.pixelSize*1e6*np.arange(-axialXLength/2.0,
                                        axialXLength/2.0)
        radialY=imageArray[peakPos[0],:]
        axialY=imageArray[:,peakPos[1]]
        valdict['radialGaussian'],valdict['radialA'],valdict['radialC'],\
        valdict['radialSigma']=self.gaussianFit(radialX,radialY)
        valdict['axialGaussian'],valdict['axialA'],valdict['axialC'],\
        valdict['axialSigma']=self.gaussianFit(axialX,axialY)
        valdict['radialX']=radialX
        valdict['radialY']=radialY
        valdict['axialX']=axialX
        valdict['axialY']=axialY
        return valdict

    def gaussianFitToCloud2(self,imageArray):
        valdict={}
        radialY=np.sum(imageArray,axis=0)
        axialY=np.sum(imageArray,axis=1)
        radialYLength=len(radialY)
        axialYLength=len(axialY)
        radialX=self.pixelSize*(self.binSize/self.magFactor)*np.arange(0,radialYLength)
        axialX=self.pixelSize*(self.binSize/self.magFactor)*np.arange(0,axialYLength)
        valdict['radialGaussian'],valdict['radialA'],valdict['radialC'],\
        valdict['radialSigma']=self.gaussianFit(radialX,radialY)
        valdict['axialGaussian'],valdict['axialA'],valdict['axialC'],\
        valdict['axialSigma']=self.gaussianFit(axialX,axialY)
        valdict['radialX']=radialX
        valdict['radialY']=radialY
        valdict['axialX']=axialX
        valdict['axialY']=axialY
        return valdict

    def getTemperature(self,fileNoStart,fileNoStop,fileNoBG,param):
        valdict={}
        timeVals=[]
        radialSigmas=[]
        axialSigmas=[]
        bg,_=self.readFromZip(fileNoBG,dstr='C')
        valdictFits=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            imgs,paramsDict=self.readFromZip(fileNo,dstr='C')
            imgsSubBG=imgs-bg
            avgImage=np.mean(self.cropImages(imgsSubBG),axis=0)
            valdictFit=self.gaussianFitToCloud2(avgImage)
            timeVals.append(paramsDict[param])
            radialSigmas.append(valdictFit['radialSigma'])
            axialSigmas.append(valdictFit['axialSigma'])
            valdictFits.append(valdictFit)
        valdict['valdictFits']=valdictFits
        valdict['axialSigmas']=np.array(axialSigmas)
        valdict['radialSigmas']=np.array(radialSigmas)
        valdict['timeVals']=np.array(timeVals)*1e-5
        valdict['axialLin'],valdict['axialM'],valdict['axialC']=\
        self.linearFit(valdict['timeVals']**2,valdict['axialSigmas']**2)
        valdict['radialLin'],valdict['radialM'],valdict['radialC']=\
        self.linearFit(valdict['timeVals']**2,valdict['radialSigmas']**2)
        return valdict

    def temperature(self,fileNoStart,fileNoStop,fileNoBG,N_interpolate,
                    param,showFits=True,cols=4):
        valdict=self.getTemperature(fileNoStart,fileNoStop,fileNoBG,param)
        valdict['axialTemp']=valdict['axialM']*(59*cn.u/cn.k)*1e3
        valdict['radialTemp']=valdict['radialM']*(59*cn.u/cn.k)*1e3
        timeValsInterpolated=np.linspace(np.min(valdict['timeVals']),
                                         np.max(valdict['timeVals']),
                                         N_interpolate)
        fig, ax = plt.subplots(1,2)
        ax[0].plot(valdict['timeVals']**2*1e6,valdict['radialSigmas']**2*1e6,'ok')
        ax[1].plot(valdict['timeVals']**2*1e6,valdict['axialSigmas']**2*1e6,'ok')
        ax[0].plot(timeValsInterpolated**2*1e6,
                  valdict['radialLin'](timeValsInterpolated**2,
                                      valdict['radialM'],
                                      valdict['radialC'])*1e6,'-r')
        ax[1].plot(timeValsInterpolated**2*1e6,
                  valdict['axialLin'](timeValsInterpolated**2,
                                      valdict['axialM'],
                                      valdict['axialC'])*1e6,'-r')
        ax[0].set_title('Tr: {0:2.4f} [mK]'.format(valdict['radialTemp']))
        ax[1].set_title('Ta: {0:2.4f} [mK]'.format(valdict['axialTemp']))
        ax[1].yaxis.tick_right()
        ax[1].yaxis.set_label_position("right")
        for axis in ax:
            axis.xaxis.set_minor_locator(AutoMinorLocator())
            axis.yaxis.set_minor_locator(AutoMinorLocator())
            axis.set_xlabel('time^2 [ms^2]')
            axis.set_ylabel('size^2 [mm^2]')
        valdict['timeValsInterpolated']=timeValsInterpolated
        if showFits:
            l=len(valdict['valdictFits'])
            for k in range(l):
                fig, ax = plt.subplots(1,1)
                valdictK=valdict['valdictFits'][k]
                ax.plot(valdictK['radialX'],valdictK['radialY'],'ob')
                ax.plot(valdictK['axialX'],valdictK['axialY'],'og')
                ax.plot(valdictK['radialX'],
                                valdictK['radialGaussian'](valdictK['radialX'],
                                                            valdictK['radialA'],
                                                            valdictK['radialC'],
                                                            valdictK['radialSigma']),'-r')
                ax.plot(valdictK['axialX'],
                                valdictK['axialGaussian'](valdictK['axialX'],
                                                            valdictK['axialA'],
                                                            valdictK['axialC'],
                                                            valdictK['axialSigma']),'-k')
                plt.show()
        return valdict

    def getSize(self,fileNoStart,fileNoStop,fileNoBG,param):
        valdict={}
        paramVals=[]
        radialSigmas=[]
        axialSigmas=[]
        bg,_=self.readFromZip(fileNoBG,dstr='C')
        valdictFits=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            imgs,paramsDict=self.readFromZip(fileNo,dstr='C')
            imgsSubBG=imgs-bg
            avgImage=np.mean(self.cropImages(imgsSubBG),axis=0)
            valdictFit=self.gaussianFitToCloud2(avgImage)
            paramVals.append(paramsDict[param])
            radialSigmas.append(valdictFit['radialSigma'])
            axialSigmas.append(valdictFit['axialSigma'])
            valdictFits.append(valdictFit)
        valdict['valdictFits']=valdictFits
        valdict['axialSigmas']=np.array(axialSigmas)
        valdict['radialSigmas']=np.array(radialSigmas)
        valdict['paramVals']=np.array(paramVals)#-np.min(paramVals))*1e-5
        return valdict

    def size(self,fileNoStart,fileNoStop,fileNoBG,N_interpolate,
                    param,showFits=True,cols=4):
        valdict=self.getSize(fileNoStart,fileNoStop,fileNoBG,param)
        fig, ax = plt.subplots(1,2)
        ax[0].plot(valdict['paramVals'],valdict['radialSigmas'],'ok')
        ax[1].plot(valdict['paramVals'],valdict['axialSigmas'],'ok')
        return valdict

    def lifetime(self,fileNoStart,fileNoStop,fileNoBG,
                 param,trigType,N_interpolate,fmt):
        valdict={}
        if trigType=='single':
            #param='Frame0Trigger'
            meanNos,stdNos,paramVals=\
            self.singleImageNumberRange(fileNoStart,fileNoStop,fileNoBG,param)
        else:
            #param='Frame1Trigger'
            meanNos,stdNos,paramVals=\
            self.twoImageNormalisedNumberRange(fileNoStart,fileNoStop,
                                           fileNoBG,param)
        offset=np.min(paramVals)/100.0
        paramVals=paramVals/100.0-offset
        fig, ax = plt.subplots()
        ax.errorbar(paramVals,meanNos,yerr=stdNos,fmt=fmt)
        valdictFit=self.numberFit(meanNos,paramVals,fitType='exp',
                                  N_interpolate=200)
        ax.plot(valdictFit['paramValsInterpolated'],
                valdictFit['meanNosInterpolated'],'-r')
        ax.xaxis.set_minor_locator(AutoMinorLocator())
        ax.yaxis.set_minor_locator(AutoMinorLocator())
        ax.set_xlabel(param+' [ms] [offset: {}]'.format(offset))
        ax.set_ylabel('MOT number')
        ax.set_title('Lifetime: {0:.2f} ms'.format(valdictFit['s']))
        plt.show()
        valdict['meanNos']=meanNos
        valdict['paramVals']=paramVals
        valdict.update(valdictFit)
        return valdict

    def viewImages(self,fileNoStart,fileNoStop,fileNoBG,cols=4):
        l=(fileNoStop+1)-fileNoStart
        rows=np.int(np.ceil(l/float(cols)))
        fig, ax = plt.subplots(rows,cols)
        avgImages=[]
        bg,_=self.readFromZip(fileNoBG,dstr='C')
        for fileNo in range(fileNoStart,fileNoStop+1):
            imgs,_=self.readFromZip(fileNo,dstr='C')
            imgsSubBG=imgs-bg
            avgImage=np.mean(self.cropImages(imgsSubBG),axis=0)
            avgImages.append(avgImage)
        
        if rows>1 and cols>1:
            k=0
            while k<l:
                ax[np.int(k/cols),np.mod(k,cols)].imshow(avgImages[k])
                k+=1
            for row in range(rows):
                for col in range(cols):
                    ax[row,col].axis('off')
        else:
            k=0
            while k<l:
                ax[np.mod(k,cols)].imshow(avgImages[k])
                k+=1
            longer=np.max([rows,cols])
            for axis in range(longer):
                ax[axis].axis('off')
        plt.show()

    def singleImageLifetime(self,fileNo,fileNoBg,shotsPerImage,t0,dt):
        images,_=self.readFromZip(fileNo,dstr='C')
        bg,_=self.readFromZip(fileNoBg,dstr='C')
        t=np.array([t0+i*dt for i in range(shotsPerImage)])*1e-2
        images=images-bg
        noShots=len(images)/shotsPerImage
        k=0
        N_list=[]
        for i in range(noShots):
            imageArray=images[k:k+shotsPerImage,:,:]
            k+=shotsPerImage
            N=self.getMOTNumber(imageArray)
            N=N/N[0]
            N_list.append(N)
        N_list=np.array(N_list)
        N_mean=np.mean(N_list,axis=0)
        N_std=np.std(N_list,axis=0)/np.sqrt(noShots)
        f,a,c,s=expFit(t,N_mean)
        return N_mean,N_std,t,f,a,c,s
               
        

    def plotDualImage(self,fileNoStart,fileNoStop,fileNoBG):
        bg1,bg2=self.getAvgImageFromTwoTriggerData(fileNoBG)
        for fileNo in range(fileNoStart,fileNoStop+1):
            imgs1,imgs2=self.getAvgImageFromTwoTriggerData(fileNo)
            imgs1-=bg1
            imgs2-=bg2
            imgs1=self.cropSingleImages(imgs1)
            imgs2=self.cropSingleImages(imgs2)
            _,ax=plt.subplots(1,2)
            ax[0].imshow(imgs1)
            ax[1].imshow(imgs2)
            


    def __call__(self,fileNoStart,fileNoStop,fileNoBG,
                     requirement='Number',
                     param='Frame0Trigger',
                     trigType='single',
                     fit=False,fitType='lin',
                     N_interpolate=200,
                     extParam='give a name',
                     extParamVals=[],
                     fmt='ok',
                     showFits=False,
                     preferredUnits=['um','mK','ms'],
                     imageCols=4):
        if requirement=='Number':
            return self.number(fileNoStart,fileNoStop,fileNoBG,param,trigType,
                          fit,fmt,fitType,N_interpolate,extParam,extParamVals)
        elif requirement=='Temperature':
            return self.temperature(fileNoStart,fileNoStop,fileNoBG,
                                    N_interpolate,param,showFits,imageCols)
        elif requirement=='Lifetime':
            return self.lifetime(fileNoStart,fileNoStop,fileNoBG,param,
              trigType,N_interpolate,fmt)
        elif requirement=='Image':
            self.viewImages(fileNoStart,fileNoStop,fileNoBG,imageCols)

def defaultCaF():
    """
    Default settings for CaF analysis\n
    return : Analysis object with settings, \n
    analysis.bitDepth=16 \n
    analysis.fullWellCapacity=18000 \n
    analysis.collectionSolidAngle=0.023 \n
    analysis.pixelSize=6.45e-6 \n
    analysis.binSize=8 \n
    analysis.bitsPerChannel=12 \n
    analysis.gamma=1.5e6 \n
    analysis.etaQ=0.65 \n
    analysis.exposureTime=10e-3 \n
    analysis.cropCentre=(74,64) \n
    analysis.cropHeight=100 \n
    analysis.cropWidth=110 \n  
    Change any of the values in the object instance using \n
    instanceName.propertyName=propertyValue \n
    Add also,\n
    analysis.dirPath=path to the data directory \n
    analysis.fileNameString=starting name of the files before underscore \n
    Example:\n
        analysis=defaultCaF() \n
        analysis.exposureTime=10e-3 \n
        analysis.dirPath='../../data/MOTMasterData' \n
        analysis.fileNameString='CaF16Jan1900' \n
    """
    analysis=Analysis()
    analysis.bitDepth=16
    analysis.fullWellCapacity=18000
    analysis.collectionSolidAngle=0.023
    analysis.pixelSize=6.45e-6
    analysis.binSize=8
    analysis.magFactor=0.5
    analysis.bitsPerChannel=12
    analysis.gamma=1.5e6
    analysis.etaQ=0.65
    analysis.exposureTime=10e-3
    analysis.cropCentre=(74,64)
    analysis.cropHeight=100
    analysis.cropWidth=110
    return analysis


if __name__=='__main__':
    analysis=defaultCaF()
    analysis.dirPath='../../data/temperature'
    analysis.fileNameString='CaF16Jan1900'
    a=analysis(fileNoStart=25,
               fileNoStop=30,
               fileNoBG=31,
               requirement='Number',
               param='ExpansionTime',
               fit=True,fitType='exp',
               trigType='single',
               N_interpolate=200,
               extParam='Test',
               extParamVals=[],
               fmt='ok',
               showFits=True,
               imageCols=4)
    

    


