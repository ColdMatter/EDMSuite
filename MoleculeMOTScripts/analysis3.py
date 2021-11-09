#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Created on Tue Oct 15 06:30:01 2019

@author: arijit
"""

from __future__ import print_function
import numpy as np
import os,zipfile
from PIL import Image
from scipy.optimize import curve_fit
from scipy import optimize
from scipy.signal import savgol_filter
import matplotlib.pyplot as plt
import scipy.constants as cn
import re
import seaborn as sns
sns.set()
import warnings
warnings.filterwarnings("ignore", category=RuntimeWarning)

linear=lambda x,m,c: m*x+c
exponential=lambda x,s: np.exp(-(x)/s)
exponentialOffset=lambda x,a,c,s: a*np.exp(-(x-c)/s)
exponentialAmp=lambda x,a,s: a*np.exp(-x/s)
exponentialAmpDelay=lambda x,a,c,s: a*np.exp(-(x-c)/s)
exponentialAmpDelayOffset=lambda x,a,c,s,o: a*np.exp(-(x-c)/s)+o
gaussian=lambda x,a,c,s: a*np.exp(-(x-c)**2/(2*s**2))
gaussianOffset=lambda x,a,c,s,o: np.abs(a)*np.exp(-(x-c)**2/(2*s**2))+o
invSinc=lambda x,a,b,c,d: a-np.abs(b)*np.sinc((x-c)*d)
sinc=lambda x,a,b,c,d: a+np.abs(b)*np.sinc((x-c)*d)

def atoi(text):
    return int(text) if text.isdigit() else text

def natural_keys(text):
    return [atoi(c) for c in re.split(r'(\d+)', text) ]

def linearFit(x,y,sigma=None): 
    m_trial=(y[-1]-y[0])/(x[-1]-x[0])
    c_trial=np.max(y) if m_trial<0 else np.min(y)
    p0=[m_trial,c_trial]
    try:
        popt,pcov=curve_fit(linear,x,y,sigma=sigma,p0=p0)
        isFit=True
    except:
        popt=np.array(p0)
        pcov=np.zeros((2,2))
        isFit=False
    return popt,np.diag(pcov),isFit

def expFit(x,y,sigma=None):
    a_trial=np.max(y)
    c_trial=x[np.argmax(y)]
    s_trial=np.abs((x[-1]-x[0])/np.log(np.abs(y[-1]/y[0])))
    p0=[s_trial]
    try:
        popt,pcov=curve_fit(exponential,x,y,sigma=sigma,p0=p0)
        isFit=True
    except:
        popt=np.array(p0)
        pcov=np.zeros((3,3))
        isFit=False
    return popt,np.diag(pcov),isFit

def expFitOffset(x,y,sigma=None):
    a_trial=np.max(y)
    o_trial=np.min(y)
    c_trial=x[np.argmax(y)]
    s_trial=np.abs((x[-1]-x[0])/np.log(np.abs(y[-1]/y[0])))
    p0=[a_trial,c_trial,s_trial]
    try:
        popt,pcov=curve_fit(exponentialOffset,x,y,sigma=sigma,p0=p0,absolute_sigma=True)
        isFit=True
    except:
        popt=np.array(p0)
        pcov=np.zeros((4,4))
        isFit=False
    return popt,np.diag(pcov),isFit

def expAmpFit(x,y,sigma=None):
    a_trial=np.max(y)
    s_trial=100000#np.abs((x[-1]-x[0])/np.log(np.abs(y[-1]/y[0])))
    p0=[a_trial,s_trial]
    try:
        popt,pcov=curve_fit(exponentialAmp,x,y,sigma=sigma,p0=p0,absolute_sigma=True)
        isFit=True
    except:
        popt=np.array(p0)
        pcov=np.zeros((4,4))
        isFit=False
    return popt,np.diag(pcov),isFit

def gaussianFit(x,y,sigma=None):
    loc_trial=np.argmax(y)
    halfmax_y = np.max(y)/2.0
    a_trial=y[loc_trial]
    c_trial=x[loc_trial]
    s_trial=np.abs(x[0]-x[1])*len(y[y>halfmax_y])/2.0
    p0=[a_trial,c_trial,s_trial]
    try:
        popt,pcov=curve_fit(gaussian,x,y,sigma=sigma,p0=p0)
        isFit=True
    except:
        popt=np.array(p0)
        pcov=np.zeros((3,3))
        isFit=False
    return popt,np.diag(pcov),isFit

def gaussianFitOffset(x,y,sigma=None):
    loc_trial=np.argmax(y)
    halfmax_y = np.max(y)/2.0
    o_trial=np.min(y)
    a_trial=y[loc_trial]
    c_trial=x[loc_trial]
    s_trial=np.abs(x[0]-x[1])*len(y[y>halfmax_y])/2.0
    p0=[a_trial,c_trial,s_trial,o_trial]
    try:
        popt,pcov=curve_fit(gaussianOffset,x,y,sigma=sigma,p0=p0)
        isFit=True
    except:
        popt=np.array(p0)
        pcov=np.zeros((4,4))
        isFit=False
    return popt,np.diag(pcov),isFit

def invSincFit(x,y,sigma=None):
    p0=[np.max(y),np.min(y),x[np.argmin(y)],200]
    try:
        popt,pcov=curve_fit(invSinc,x,y,sigma=sigma,p0=p0)
        isFit=True
    except:
        popt=np.array(p0)
        pcov=np.zeros((4,4))
        isFit=False
    return popt,np.diag(pcov),isFit

def sincFit(x,y,sigma=None):
    p0=[np.min(y),np.max(y),x[np.argmax(y)],200]
    try:
        popt,pcov=curve_fit(sinc,x,y,sigma=sigma,p0=p0)
        isFit=True
    except:
        popt=np.array(p0)
        pcov=np.zeros((4,4))
        isFit=False
    return popt,np.diag(pcov),isFit

def injector(fileNoStart,fileNoStop,NoImages,
             fileNameString,
             fileSkip=1,
             remotePath="//PH-TEW105/Users/rfmot/Desktop/AbsImages/",
             dirPath="C:/Users/cafmot/Box Sync/CaF MOT/MOTData/MOTMasterData/"):
    imgs=os.listdir(remotePath)
    imgs.sort(key=natural_keys)
    if len(imgs)==(fileNoStop-fileNoStart+1)*NoImages/fileSkip:
        print('Inserting images to the zip files...')
        l=0
        for fileNo in range(fileNoStart,fileNoStop+1, fileSkip):
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
        print('No Image to insert')
    elif len(imgs)<(fileNoStart-fileNoStop+1)*NoImages:
        print('There seems to be less number of images than required!')
    elif len(imgs)>(fileNoStart-fileNoStop+1)*NoImages:
        print('There are more images than expected!')

class Analysis():
    def __init__(self,args={}):
        for key in args:
            self.__dict__[key]=args[key]
        self.diffStr=''
        
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
        return counts*(np.float(self.fullWellCapacity)/\
                       (2**self.bitsPerChannel-1))/self.etaQ
    
    def convertPhotonsToNumber(self,photonCount):
        return photonCount/(self.exposureTime*self.gamma*self.collectionSolidAngle)

    def readFromZip(self,fileNo):
        archive=zipfile.ZipFile(self.getFilepath(fileNo),'r')
        imgs=[]
        files=archive.namelist()
        files.sort(key=natural_keys)
        for f in files:
            if f[-3:]=='tif':
                if self.diffStr=='':
                    with archive.open(f) as filename:
                        imgs.append(np.array(Image.open(filename),dtype=float))
                elif f[0]==self.diffStr:
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

    def readFromZipCaFRb(self,fileNo, prefix):
        archive=zipfile.ZipFile(self.getFilepath(fileNo),'r')
        imgs=[]
        files=archive.namelist()
        files.sort(key=natural_keys)
        for f in files:
            if ((f[-3:]=='tif') and (f[0]==prefix)):
                if self.diffStr=='':
                    with archive.open(f) as filename:
                        imgs.append(np.array(Image.open(filename),dtype=float))
                elif f[0]==self.diffStr:
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

    def getImagesFromOneTriggerData(self,fileNo):
        imgs,paramsDict=self.readFromZip(fileNo)
        return imgs[1:],paramsDict
    
    def getImagesFromTwoTriggerData(self,fileNo):
        imgs,paramsDict=self.readFromZip(fileNo)
        return imgs[2::2,:,:],imgs[3::2,:,:],paramsDict

    def getImagesFromThreeTriggerData(self,fileNo):
        imgs,paramsDict=self.readFromZip(fileNo)
        return imgs[0::3,:,:],imgs[1::3,:,:],imgs[2::3,:,:],paramsDict

    def getImagesFromFourTriggerData(self,fileNo):
        imgs,paramsDict=self.readFromZip(fileNo)
        return imgs[0::4,:,:],imgs[1::4,:,:],imgs[2::4,:,:],imgs[3::4,:,:],paramsDict

    def getAvgImageFromOneTriggerData(self,fileNo):
        imgs,_=self.getImagesFromOneTriggerData(fileNo)
        return np.mean(imgs,axis=0)
    
    def getAvgImageFromTwoTriggerData(self,fileNo):
        firstImages,secondImages,_=self.getImagesFromTwoTriggerData(fileNo)
        return np.mean(firstImages,axis=0),\
               np.mean(secondImages,axis=0)
    
    def getAvgImageFromThreeTriggerData(self,fileNo):
        firstImages,secondImages,thirdImages=self.getImagesFromThreeTriggerData(fileNo)
        return np.mean(firstImages,axis=0),\
               np.mean(secondImages,axis=0),\
               np.mean(thirdImages,axis=0)

    def cropImages(self,imageArray):
        if self.crop:
            h_top=int(self.cropCentre[0]-self.cropHeight/2)
            h_bottom=int(self.cropCentre[0]+self.cropHeight/2)
            w_left=int(self.cropCentre[1]-self.cropWidth/2)
            w_right=int(self.cropCentre[1]+self.cropWidth/2)
            return imageArray[:,h_top:h_bottom,w_left:w_right]
        else:
            return imageArray
    
    def cropSingleImage(self,imageArray):
        if self.crop:
            h_top=self.cropCentre[1]-self.cropHeight/2
            h_bottom=self.cropCentre[1]+self.cropHeight/2
            w_left=self.cropCentre[0]-self.cropWidth/2
            w_right=self.cropCentre[0]+self.cropWidth/2
            return imageArray[h_top:h_bottom,w_left:w_right]
        else:
            return imageArray

    def getImageNumber(self,imageArray):
        totalCount=np.sum(imageArray,axis=(1,2))
        totalMolecules=self.convertPhotonsToNumber(
                       self.convertCountsToPhotons(totalCount))
        return totalMolecules

    def singleImageCloudSize(self,imageArray):
        radialY=np.sum(imageArray,axis=0)
        axialY=np.sum(imageArray,axis=1)
        radialYLength=len(radialY)
        axialYLength=len(axialY)
        radialX=self.pixelSize*(self.binSize/self.magFactor)*np.arange(0,radialYLength)
        axialX=self.pixelSize*(self.binSize/self.magFactor)*np.arange(0,axialYLength)
        smoothRadialY=radialY#savgol_filter(radialY,self.smoothingWindow,3)
        smoothAxialY=axialY#savgol_filter(axialY,self.smoothingWindow,3)
        radialpopt,radialpcov,radialIsFit=gaussianFitOffset(radialX,radialY)
        axialpopt,axialpcov,axialIsFit=gaussianFitOffset(axialX,axialY)
        return radialX,radialY,\
               radialpopt,radialIsFit,\
               axialX,axialY,\
               axialpopt,axialIsFit
    
    def getImageSizes(self,imageArray):
        n=np.shape(imageArray)[0]
        radialX=[]
        radialY=[]
        axialX=[]
        axialY=[]
        radialpopts=[]
        radialisfits=[]
        axialpopts=[]
        axialisfits=[]
        for i in range(n):
            radialXI,radialYI,radialpoptsI,radialisfitsI,axialXI,axialYI,\
            axialpoptsI,axialisfitsI=self.singleImageCloudSize(imageArray[i,:,:])
            radialX.append(radialXI)
            radialY.append(radialYI)
            radialpopts.append(radialpoptsI)
            radialisfits.append(radialisfitsI)
            axialX.append(axialXI)
            axialY.append(axialYI)
            axialpopts.append(axialpoptsI)
            axialisfits.append(axialisfitsI)
        return np.array(radialX),np.array(radialY),\
               np.array(radialpopts),\
               np.array(radialisfits),\
               np.array(axialX),np.array(axialY),\
               np.array(axialpopts),\
               np.array(axialisfits)

    def singleImageProcessing(self,fileNo,fileNoBG,param):
        images,paramsDict=self.getImagesFromOneTriggerData(fileNo)
        if fileNoBG is not None:
            imagesBG=self.getAvgImageFromOneTriggerData(fileNoBG)
            images=images-imagesBG
        if self.crop:
            images=self.cropImages(images)
        return images,paramsDict[param]

    def doubleImageProcessing(self,fileNo,fileNoBG,param):
        firstImages,secondImages,paramsDict=self.getImagesFromTwoTriggerData(fileNo)
        if fileNoBG is not None:
            firstImageAvgBG,secondImageAvgBG=self.getAvgImageFromTwoTriggerData(fileNoBG)
            firstImages=firstImages-firstImageAvgBG
            secondImages=secondImages-secondImageAvgBG
        if self.crop:
            firstImages=self.cropImages(firstImages)
            secondImages=self.cropImages(secondImages)
        return firstImages, secondImages, paramsDict[param]

    def trippleImageProcessing(self,fileNo,param):
        clouds,probes,bgs,paramsDict=self.getImagesFromThreeTriggerData(fileNo)
        probes=probes-bgs
        clouds=clouds-bgs
        clouds[clouds<=0]=1.0
        od=np.log(probes/clouds)
        od[np.isnan(od)] = 0.0
        od[od == -np.inf] = 0.0
        od[od == np.inf] = 0.0
        if self.crop:
            od=self.cropImages(od)
        if self.od_correction:
            od_s = 8
            od = od+np.log((1-np.exp(-od_s))/(1-np.exp(od-od_s)))
        return od,paramsDict[param]

    def singleImageNumberRange(self,fileNoStart,fileNoStop,fileNoBG,param):
        paramsValList=[]
        numbersList=[]
        images=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            if fileNo not in self.fileNoExclude:
                imageSubBG,paramsVal=self.singleImageProcessing(fileNo,fileNoBG,param)
                images.append(imageSubBG)
                numbers=self.getImageNumber(imageSubBG)
                numbersList.append(numbers)
                paramsValList.append(paramsVal)
        self.firstImage=np.array(images)
        self.firstImageNumbers=np.array(numbersList)
        self.paramVals=np.array(paramsValList,dtype=float)
        
    def singleImageSizeRange(self,fileNoStart,fileNoStop,fileNoBG,param):
        images=[]
        radialXList=[]
        radialYList=[]
        axialXList=[]
        axialYList=[]
        radialpoptsList=[]
        radialisfitsList=[]
        axialpoptsList=[]
        axialisfitsList=[]
        paramsValList=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            if fileNo not in self.fileNoExclude:
                imageSubBG,paramsVal=self.singleImageProcessing(fileNo,fileNoBG,param)
                radialX,radialY,radialpopts,radialisfits,\
                axialX,axialY,axialpopts,axialisfits=self.getImageSizes(imageSubBG)
                images.append(imageSubBG)
                radialXList.append(radialX)
                radialYList.append(radialY)
                radialpoptsList.append(radialpopts)
                radialisfitsList.append(radialisfits)
                axialXList.append(axialX)
                axialYList.append(axialY)
                axialpoptsList.append(axialpopts)
                radialisfitsList.append(radialisfits)
                paramsValList.append(paramsVal)
        self.firstImage=np.array(images)
        self.secondImage=np.zeros_like(self.firstImage)
        self.firstImageRadialX=np.array(radialXList)
        self.firstImageRadialY=np.array(radialYList)
        self.firstImageAxialX=np.array(axialXList)
        self.firstImageAxialY=np.array(axialYList)
        self.firstImageRadialFitParams=np.array(radialpoptsList)
        self.firstImageAxialFitParams=np.array(axialpoptsList)
        self.firstImageRadialIsFit=np.array(radialisfitsList)
        self.firstImageAxialIsFit=np.array(axialisfitsList)
        self.paramVals=np.array(paramsValList,dtype=float)
        
    def doubleImageNumberRange(self,fileNoStart,fileNoStop,fileNoBG,param):
        paramsValList=[]
        numbersFirst=[]
        numbersSecond=[]
        firstImages=[]
        secondImages=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            if fileNo not in self.fileNoExclude:
                firstImagesSubBG,secondImagesSubBG,paramsVal=\
                self.doubleImageProcessing(fileNo,fileNoBG,param)
                firstImageNumbers=self.getImageNumber(firstImagesSubBG)
                secondImageNumbers=self.getImageNumber(secondImagesSubBG)
                firstImages.append(firstImagesSubBG)
                secondImages.append(secondImagesSubBG)
                numbersFirst.append(firstImageNumbers)
                numbersSecond.append(secondImageNumbers)
                paramsValList.append(paramsVal)
        self.firstImage=np.array(firstImages)
        self.secondImage=np.array(secondImages)
        self.firstImageNumbers=np.array(numbersFirst)
        self.secondImageNumbers=np.array(numbersSecond)
        self.paramVals=np.array(paramsValList,dtype=float)
    
    def doubleImageSizeRange(self,fileNoStart,fileNoStop,fileNoBG,param):
        radialpoptsFirstList=[]
        radialisfitsFirstList=[]
        axialpoptsFirstList=[]
        axialisfitsFirstList=[]
        radialpoptsSecondList=[]
        radialisfitsSecondList=[]
        axialpoptsSecondList=[]
        axialisfitsSecondList=[]
        paramsValList=[]
        firstImages=[]
        secondImages=[]
        radialXFirstList=[]
        radialYFirstList=[]
        axialXFirstList=[]
        axialYFirstList=[]
        radialXSecondList=[]
        radialYSecondList=[]
        axialXSecondList=[]
        axialYSecondList=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            if fileNo not in self.fileNoExclude:
                firstImagesSubBG,secondImagesSubBG,paramsVal=\
                            self.doubleImageProcessing(fileNo,fileNoBG,param)
                radialXFirst,radialYFirst,radialpoptsFirst,radialisfitsFirst,\
                axialXFirst,axialYFirst,axialpoptsFirst,axialisfitsFirst=\
                                        self.getImageSizes(firstImagesSubBG)
                radialXSecond,radialYSecond,radialpoptsSecond,radialisfitsSecond,\
                axialXSecond,axialYSecond,axialpoptsSecond,axialisfitsSecond=\
                                        self.getImageSizes(secondImagesSubBG)
                firstImages.append(firstImagesSubBG)
                secondImages.append(secondImagesSubBG)
                radialXFirstList.append(radialXFirst)
                radialYFirstList.append(radialYFirst)
                axialXFirstList.append(axialXFirst)
                axialYFirstList.append(axialYFirst)
                radialXSecondList.append(radialXSecond)
                radialYSecondList.append(radialYSecond)
                axialXSecondList.append(axialXSecond)
                axialYSecondList.append(axialYSecond)
                radialpoptsFirstList.append(radialpoptsFirst)
                radialisfitsFirstList.append(radialisfitsFirst)
                axialpoptsFirstList.append(axialpoptsFirst)
                axialisfitsFirstList.append(axialisfitsFirst)
                radialpoptsSecondList.append(radialpoptsSecond)
                radialisfitsSecondList.append(radialisfitsSecond)
                axialpoptsSecondList.append(axialpoptsSecond)
                axialisfitsSecondList.append(axialisfitsSecond)
                paramsValList.append(paramsVal)
        self.firstImage=np.array(firstImages)
        self.secondImage=np.array(secondImages)
        self.firstImageRadialX=np.array(radialXFirstList)
        self.firstImageRadialY=np.array(radialYFirstList)
        self.firstImageRadialFitParams=np.array(radialpoptsFirstList)
        self.firstImageRadialIsFit=np.array(radialisfitsFirstList)
        self.firstImageAxialX=np.array(axialXFirstList)
        self.firstImageAxialY=np.array(axialYFirstList)
        self.firstImageAxialFitParams=np.array(axialpoptsFirstList)
        self.firstImageAxialIsFit=np.array(axialisfitsFirstList)
        self.secondImageRadialX=np.array(radialXSecondList)
        self.secondImageRadialY=np.array(radialYSecondList)
        self.secondImageAxialX=np.array(axialXSecondList)
        self.secondImageAxialY=np.array(axialYSecondList)
        self.secondImageRadialFitParams=np.array(radialpoptsSecondList)
        self.secondImageRadialIsFit=np.array(radialisfitsSecondList)
        self.secondImageAxialFitParams=np.array(axialpoptsSecondList)
        self.secondImageAxialIsFit=np.array(axialisfitsSecondList)
        self.paramVals=np.array(paramsValList,dtype=float)
        
    def trippleImageNumberRange(self,fileNoStart,fileNoStop,fileNoBG,param):
        numbersList=[]
        paramsValList=[]
        images=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            if fileNo not in self.fileNoExclude:
                od,paramsVal=self.trippleImageProcessing(fileNo,param)
                numbers=(self.pixelSize*(self.binSize/self.magFactor))**2*\
                        np.sum(od,axis=(1,2))*self.s0
                images.append(od)
                numbersList.append(numbers)
                paramsValList.append(paramsVal)
        self.firstImage=np.array(images)
        self.firstImageNumbers=np.array(numbersList)
        self.paramVals=np.array(paramsValList,dtype=float)

    def trippleImageSizeRange(self,fileNoStart,fileNoStop,fileNoBG,param):
        radialpoptsList=[]
        radialisfitsList=[]
        axialpoptsList=[]
        axialisfitsList=[]
        paramsValList=[]
        images=[]
        radialXList=[]
        radialYList=[]
        axialXList=[]
        axialYList=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            if fileNo not in self.fileNoExclude:
                od,paramsVal=self.trippleImageProcessing(fileNo,param)
                radialX,radialY,radialpopts,radialisfits,\
                axialX,axialY,axialpopts,axialisfits=\
                                            self.getImageSizes(od)
                images.append(od)
                radialpoptsList.append(radialpopts)
                radialisfitsList.append(radialisfits)
                axialpoptsList.append(axialpopts)
                radialisfitsList.append(radialisfits)
                axialisfitsList.append(axialisfits)
                radialXList.append(radialX)
                radialYList.append(radialY)
                axialXList.append(axialX)
                axialYList.append(axialY)
                paramsValList.append(paramsVal)
        self.firstImage=np.array(images)
        self.firstImageRadialX=np.array(radialXList)
        self.firstImageRadialY=np.array(radialYList)
        self.firstImageAxialX=np.array(axialXList)
        self.firstImageAxialY=np.array(axialYList)
        self.firstImageRadialFitParams=np.array(radialpoptsList)
        self.firstImageAxialFitParams=np.array(axialpoptsList)
        self.firstImageRadialIsFit=np.array(radialisfitsList)
        self.firstImageAxialIsFit=np.array(axialisfitsList)
        self.paramVals=np.array(paramsValList,dtype=float)
    
    def getNumber(self,fileNoStart,fileNoStop,fileNoBG,param):
        if self.imagingType=='Fluoresence':
            if self.trigType=='single':
                self.singleImageNumberRange(fileNoStart,fileNoStop,
                                                   fileNoBG,param)
                self.firstImageMeanNumbers=np.mean(self.firstImageNumbers,axis=1)
                self.firstImageStdErrorNumbers=np.std(self.firstImageNumbers,axis=1)\
                                    /np.sqrt(np.shape(self.firstImageNumbers)[1])

                if self.fit:
                    self.fitVariations(self.paramVals,
                                      self.firstImageMeanNumbers,
                                      self.firstImageStdErrorNumbers,
                                      self.fitType)
                elif self.isLifetime:
                    self.lifetime(self.paramVals,
                                  self.firstImageMeanNumbers,
                                  self.firstImageStdErrorNumbers)
                else:
                    self.displayImageNumbersVariation(self.paramVals,
                                      self.firstImageMeanNumbers,
                                      self.firstImageStdErrorNumbers)
                if self.showFirstImage:
                    self.displayImage('First Image',self.firstImage)
                               
            elif self.trigType=='double':
                self.doubleImageNumberRange(fileNoStart,fileNoStop,
                                                   fileNoBG,param)
                ratio=self.secondImageNumbers/self.firstImageNumbers
                self.ratioImageMeanNumbers=np.mean(ratio,axis=1)
                self.ratioImageStdErrorNumbers=np.std(ratio,axis=1)\
                                    /np.sqrt(np.shape(ratio)[1])
                if self.fit:
                    self.fitVariations(self.paramVals,
                                      self.ratioImageMeanNumbers,
                                      self.ratioImageStdErrorNumbers,
                                      self.fitType)
                elif self.isLifetime:
                    self.lifetime(self.paramVals,
                                  self.ratioImageMeanNumbers,
                                  self.ratioImageStdErrorNumbers)
                else:
                    self.displayImageNumbersVariation(self.paramVals,
                                      self.ratioImageMeanNumbers,
                                      self.ratioImageStdErrorNumbers)
                if self.showFirstImage:
                    self.displayImage('First Image',self.firstImage)
                if self.showSecondImage:
                    self.displayImage('Second Image',self.secondImage)
                
        elif self.imagingType=='Absorption':
            self.trippleImageNumberRange(fileNoStart,fileNoStop,
                                                   fileNoBG,param)
            self.firstImageMeanNumbers=np.mean(self.firstImageNumbers,axis=1)
            self.firstImageStdErrorNumbers=np.std(self.firstImageNumbers,axis=1)\
                                /np.sqrt(np.shape(self.firstImageNumbers)[1])
            if self.fit:
                self.fitVariations(self.paramVals,
                                      self.firstImageMeanNumbers,
                                      self.firstImageStdErrorNumbers,
                                      self.fitType)
            elif self.isLifetime:
                    self.lifetime(self.paramVals,
                                  self.firstImageMeanNumbers,
                                  self.firstImageStdErrorNumbers)
            else:
                self.displayImageNumbersVariation(self.paramVals,
                                      self.firstImageMeanNumbers,
                                      self.firstImageStdErrorNumbers)
            if self.showFirstImage:
                self.displayImage('First Image',self.firstImage)

    def getSize(self,fileNoStart,fileNoStop,fileNoBG,param):
        if self.imagingType=='Fluoresence':
            if self.trigType=='single':
                self.singleImageSizeRange(fileNoStart,fileNoStop,
                                                     fileNoBG,param)
                radialSizes=np.abs(self.firstImageRadialFitParams[:,:,2])
                axialSizes=np.abs(self.firstImageAxialFitParams[:,:,2])
                self.firstImageMeanRadialSizes=np.mean(radialSizes,axis=1)
                self.firstImageMeanAxialSizes=np.mean(axialSizes,axis=1)
                self.firstImageStdErrorRadialSizes=np.std(radialSizes,axis=1)\
                                            /np.sqrt(np.shape(radialSizes)[1])
                self.firstImageStdErrorAxialSizes=np.std(axialSizes,axis=1)\
                                            /np.sqrt(np.shape(axialSizes)[1])
                if self.fit:
                    self.fitVariations(self.paramVals,
                                       self.firstImageMeanRadialSizes,
                                       self.firstImageStdErrorRadialSizes,
                                       self.fitType)
                    self.fitVariations(self.paramVals,
                                       self.firstImageMeanAxialSizes,
                                       self.firstImageStdErrorAxialSizes,
                                       self.fitType)
                elif self.isTemperature:
                    self.temperature(self.paramVals,
                                     self.firstImageMeanRadialSizes,
                                     self.firstImageStdErrorRadialSizes,
                                     self.firstImageMeanAxialSizes,
                                     self.firstImageStdErrorAxialSizes)
                else:
                    self.displaySingleImageSizeVariation()
                    
                if self.showFirstImage:
                    self.displayImage('First Image',self.firstImage)
                if self.showSizeFitsFirstImage:
                    self.displaySizeFits('First Image',
                                         self.firstImageRadialX,
                                         self.firstImageRadialY,
                                         self.firstImageAxialX,
                                         self.firstImageAxialY,
                                         self.firstImageRadialFitParams,
                                         self.firstImageAxialFitParams)
                
                
            elif self.trigType=='double':
                self.doubleImageSizeRange(fileNoStart,fileNoStop,
                                                     fileNoBG,param)
                radialSizes=np.abs(self.firstImageRadialFitParams[:,:,2])
                axialSizes=np.abs(self.firstImageAxialFitParams[:,:,2])
                self.firstImageMeanRadialSizes=np.mean(radialSizes,axis=1)
                self.firstImageMeanAxialSizes=np.mean(axialSizes,axis=1)
                self.firstImageStdErrorRadialSizes=np.std(radialSizes,axis=1)\
                                            /np.sqrt(np.shape(radialSizes)[1])
                self.firstImageStdErrorAxialSizes=np.std(axialSizes,axis=1)\
                                            /np.sqrt(np.shape(axialSizes)[1])
                                            
                radialSizes=np.abs(self.secondImageRadialFitParams[:,:,2])
                axialSizes=np.abs(self.secondImageAxialFitParams[:,:,2])
                self.secondImageMeanRadialSizes=np.mean(radialSizes,axis=1)
                self.secondImageMeanAxialSizes=np.mean(axialSizes,axis=1)
                self.secondImageStdErrorRadialSizes=np.std(radialSizes,axis=1)\
                                            /np.sqrt(np.shape(radialSizes)[1])
                self.secondImageStdErrorAxialSizes=np.std(axialSizes,axis=1)\
                                            /np.sqrt(np.shape(axialSizes)[1])
                if self.fit:
                    self.fitVariations(self.paramVals,
                                       self.secondImageMeanRadialSizes,
                                       self.secondImageStdErrorRadialSizes,
                                       self.fitType)
                    self.fitVariations(self.paramVals,
                                       self.secondImageMeanAxialSizes,
                                       self.secondImageStdErrorAxialSizes,
                                       self.fitType)
                elif self.isTemperature:
                    self.temperature(self.paramVals,
                                     self.secondImageMeanRadialSizes,
                                     self.secondImageStdErrorRadialSizes,
                                     self.secondImageMeanAxialSizes,
                                     self.secondImageStdErrorAxialSizes)
                else:
                    self.displayDoubleImageSizeVariation()
                    
                if self.showFirstImage:
                    self.displayImage('First Image',self.firstImage)
                    
                if self.showSecondImage:
                    self.displayImage('Second Image',self.secondImage)
                    
                if self.showSizeFitsFirstImage:
                    self.displaySizeFits('First Image',
                                         self.firstImageRadialX,
                                         self.firstImageRadialY,
                                         self.firstImageAxialX,
                                         self.firstImageAxialY,
                                         self.firstImageRadialFitParams,
                                         self.firstImageAxialFitParams)
                if self.showSizeFitsSecondImage:
                    self.displaySizeFits('Second Image',
                                         self.secondImageRadialX,
                                         self.secondImageRadialY,
                                         self.secondImageAxialX,
                                         self.secondImageAxialY,
                                         self.secondImageRadialFitParams,
                                         self.secondImageAxialFitParams)
                
        elif self.imagingType=='Absorption':
            self.trippleImageSizeRange(fileNoStart,fileNoStop,
                                                     fileNoBG,param)
            radialSizes=np.abs(self.firstImageRadialFitParams[:,:,2])
            axialSizes=np.abs(self.firstImageAxialFitParams[:,:,2])
            self.firstImageMeanRadialSizes=np.mean(radialSizes,axis=1)
            self.firstImageMeanAxialSizes=np.mean(axialSizes,axis=1)
            self.firstImageStdErrorRadialSizes=np.std(radialSizes,axis=1)\
                                        /np.sqrt(np.shape(radialSizes)[1])
            self.firstImageStdErrorAxialSizes=np.std(axialSizes,axis=1)\
                                        /np.sqrt(np.shape(axialSizes)[1])
            if self.fit:
                self.fitVariations(self.paramVals,
                                   self.firstImageMeanRadialSizes,
                                   self.firstImageStdErrorRadialSizes,
                                   self.fitType)
                self.fitVariations(self.paramVals,
                                   self.firstImageMeanAxialSizes,
                                   self.firstImageStdErrorAxialSizes,
                                   self.fitType)
            elif self.isTemperature:
                self.temperature(self.paramVals,
                                 self.firstImageMeanRadialSizes,
                                 self.firstImageStdErrorRadialSizes,
                                 self.firstImageMeanAxialSizes,
                                 self.firstImageStdErrorAxialSizes)
            else:
                self.displaySingleImageSizeVariation()
                
            if self.showFirstImage:
                self.displayImage('First Image',self.firstImage)
                
            if self.showSizeFitsFirstImage:
                self.displaySizeFits('First Image',
                                     self.firstImageRadialX,
                                     self.firstImageRadialY,
                                     self.firstImageAxialX,
                                     self.firstImageAxialY,
                                     self.firstImageRadialFitParams,
                                     self.firstImageAxialFitParams)

    def fitVariations(self,paramVals,numbers,stdErrorNumbers,fitType):
        paramValsFine=np.linspace(np.min(paramVals),np.max(paramVals),100)
        fig,ax=plt.subplots()
        if fitType=='lin':
            popt,diagpcov,isFit=linearFit(paramVals,numbers,stdErrorNumbers)
            yFit=linear(paramValsFine,*popt)
            self.fitParams=popt
            funcText='Fit Func:\ny(x,m,c)=m*x+c\n'
            m='m: {0:.3f}\n'.format(popt[0]/(self.yScale/self.xScale))
            c='c: {0:.3f}\n'.format(popt[1]/self.yScale)
            ax.text(1.05,0.3,funcText+m+c,transform=ax.transAxes,wrap=True)
        if fitType=='exp':
            popt,diagpcov,isFit=expFit(paramVals,numbers,stdErrorNumbers)
            yFit=exponential(paramValsFine,*popt)
            self.fitParams=popt
            funcText='Fit Func:\ny(x,a,c,s)=a*exp(-(x-c)/s)\n'
            a='a: {0:.3f}\n'.format(popt[0]/self.yScale)
            c='c: {0:.3f}\n'.format(popt[1]/self.xScale)
            s='s: {0:.3f}\n'.format(popt[2]/self.xScale)
            ax.text(1.05,0.3,funcText+a+c+s,transform=ax.transAxes,wrap=True)
        if fitType=='expOffset':
            popt,diagpcov,isFit=expFitOffset(paramVals,numbers,stdErrorNumbers)
            yFit=exponentialOffset(paramValsFine,*popt)
            self.fitParams=popt
            funcText='Fit Func:\ny(x,a,c,s,o)=a*exp(-(x-c)/s)+o\n'
            a='a: {0:.3f}\n'.format(popt[0]/self.yScale)
            c='c: {0:.3f}\n'.format(popt[1]/self.xScale)
            s='s: {0:.3f}\n'.format(popt[2]/self.xScale)
            ax.text(1.05,0.3,funcText+a+c+s,transform=ax.transAxes,wrap=True)
        if fitType=='gaussian':
            popt,diagpcov,isFit=gaussianFit(paramVals,numbers,stdErrorNumbers)
            yFit=gaussian(paramValsFine,*popt)
            self.fitParams=popt
            funcText='Fit Func:\ny(x,a,c,s)=a*exp(-(x-c)**2/(2*s**2))\n'
            a='a: {0:.3f}\n'.format(popt[0]/self.yScale)
            c='c: {0:.3f}\n'.format(popt[1]/self.xScale)
            s='s: {0:.3f}\n'.format(popt[2]/self.xScale)
            ax.text(1.05,0.3,funcText+a+c+s,transform=ax.transAxes,wrap=True)
        if fitType=='gaussianOffset':
            popt,diagpcov,isFit=gaussianFitOffset(paramVals,numbers,stdErrorNumbers)
            yFit=gaussianOffset(paramValsFine,*popt)
            self.fitParams=popt
            funcText='Fit Func:\ny(x,a,c,s,o)=a*exp(-(x-c)**2/(2*s**2))+o\n'
            a='a: {0:.3f}\n'.format(popt[0]/self.yScale)
            c='c: {0:.3f}\n'.format(popt[1]/self.xScale)
            s='s: {0:.3f}\n'.format(popt[2]/self.xScale)
            o='o: {0:.3f}\n'.format(popt[3]/self.yScale)
            ax.text(1.05,0.3,funcText+a+c+s+o,transform=ax.transAxes,wrap=True)
        if fitType=='invSinc':
            popt,diagpcov,isFit=invSincFit(paramVals,numbers,stdErrorNumbers)
            yFit=invSinc(paramValsFine,*popt)
            self.fitParams=popt
            funcText='Fit Func:\ny(x,a,b,c,d)=a-b*sinc((x-c)*d)\n'
            a='a: {0:.3f}\n'.format(popt[0]/self.yScale)
            b='b: {0:.3f}\n'.format(popt[1]/self.yScale)
            c='c: {0:.3f}\n'.format(popt[2]/self.xScale)
            d='d: {0:.3f}\n'.format(popt[3]/self.xScale)
            ax.text(1.05,0.3,funcText+a+b+c+d,transform=ax.transAxes,wrap=True)
        if fitType=='sinc':
            popt,diagpcov,isFit=sincFit(paramVals,numbers,stdErrorNumbers)
            yFit=sinc(paramValsFine,*popt)
            self.fitParams=popt
            funcText='Fit Func:\ny(x,b,c,d)=b*sinc((x-c)*d)\n'
            a='a: {0:.3f}\n'.format(popt[0]/self.yScale)
            b='b: {0:.3f}\n'.format(popt[1]/self.yScale)
            c='c: {0:.3f}\n'.format(popt[2]/self.xScale)
            d='d: {0:.3f}\n'.format(popt[3]/self.xScale)
            ax.text(1.05,0.3,funcText+a+b+c+d,transform=ax.transAxes,wrap=True)
        if fitType=='expAmp':
            popt,diagpcov,isFit=expAmpFit(paramVals,numbers,stdErrorNumbers)
            yFit=exponentialAmp(paramValsFine,*popt)
            self.fitParams=popt
            funcText='Fit Func:\ny(x,a,s)=a*exp(-x/s)\n'
            a='a: {0:.3f}+-{1:.3f}\n'.format(popt[0]/self.yScale,np.sqrt(diagpcov[0])/self.yScale)
            s='s: {0:.3f}+-{1:.3f}\n'.format(popt[1]/self.xScale,np.sqrt(diagpcov[1])/self.xScale)
            ax.text(1.05,0.3,funcText+a+s,transform=ax.transAxes,wrap=True)
        # TODO: add lorengian and inverted gaussian
        ax.errorbar(paramVals/self.xScale,
                    numbers/self.yScale,
                    yerr=stdErrorNumbers/self.yScale,
                    fmt=self.fmtP)
        ax.plot(paramValsFine/self.xScale,yFit/self.yScale,'-g')
        ax.legend(['Fit','Experimental'])
        ax.set_xlabel(self.xLabel)
        ax.set_ylabel(self.yLabel)

    def temperature(self,paramVals,meanRadialSizes,stdErrorRadialSizes,
                    meanAxialSizes,stdErrorAxialSizes):
        tSq=(paramVals*1e-5)**2
        radialSizeSq=meanRadialSizes**2
        radialErrorSq=stdErrorRadialSizes**2
        axialSizeSq=meanAxialSizes**2
        axialErrorSq=stdErrorAxialSizes**2
        poptR,diagpcovR,isFitR=linearFit(tSq,radialSizeSq,stdErrorRadialSizes)
        poptA,diagpcovA,isFitA=linearFit(tSq,axialSizeSq,stdErrorAxialSizes)
        tSqFine=np.linspace(np.min(tSq),np.max(tSq),100)
        radialSizeSqFine=linear(tSqFine,*poptR)
        axialSizeSqFine=linear(tSqFine,*poptA)
        self.radialT=poptR[0]*(self.massInAMU*cn.u/cn.k)
        self.axialT=poptA[0]*(self.massInAMU*cn.u/cn.k)
        self.radialTConfIntv=np.sqrt(diagpcovR[0])*(self.massInAMU*cn.u/cn.k)
        self.axialTConfIntv=np.sqrt(diagpcovA[0])*(self.massInAMU*cn.u/cn.k)
        
        bound_upperR = linear(tSqFine, *(poptR + np.sqrt(diagpcovR)))
        bound_lowerR = linear(tSqFine, *(poptR - np.sqrt(diagpcovR)))
        bound_upperA = linear(tSqFine, *(poptA + np.sqrt(diagpcovA)))
        bound_lowerA = linear(tSqFine, *(poptA - np.sqrt(diagpcovA)))
        
        fig,ax=plt.subplots(1,2,sharex=True,figsize=self.figSizePlot)
        fig.subplots_adjust(hspace=0.01,wspace=0.01)
        ax[1].yaxis.tick_right()
        ax[1].yaxis.set_label_position("right")
        
        ax[0].errorbar(tSq*1e6,radialSizeSq*1e6,yerr=radialErrorSq*1e6,fmt='ok')
        ax[0].plot(tSqFine*1e6,radialSizeSqFine*1e6,'-r')
        ax[1].errorbar(tSq*1e6,axialSizeSq*1e6,yerr=axialErrorSq*1e6,fmt='ok')
        ax[1].plot(tSqFine*1e6,axialSizeSqFine*1e6,'-r')
        ax[0].fill_between(tSqFine*1e6,bound_lowerR*1e6,
                              bound_upperR*1e6,color='r',alpha=0.15)
        ax[1].fill_between(tSqFine*1e6,bound_lowerA*1e6,
                              bound_upperA*1e6,color='r',alpha=0.15)
        ax[0].set_xlabel('time^2 [ms^2]')
        ax[0].set_ylabel('size^2 [mm^2]')
        ax[1].set_xlabel('time^2 [ms^2]')
        ax[1].set_ylabel('size^2 [mm^2]')
        tr = "Tr:{0:.3f}".format(self.radialT*1e6)+\
                   u"\u00B1"+"{0:.3f} uK".format(self.radialTConfIntv*1e6)
        ta = "Ta:{0:.3f}".format(self.axialT*1e6)+\
                   u"\u00B1"+"{0:.3f} uK".format(self.axialTConfIntv*1e6)
        ax[0].set_title(tr)
        ax[1].set_title(ta)
    
    
    def density(self,fileNoStart,fileNoStop,fileNoBG,param):
        if self.imagingType=='Fluoresence':
            if self.trigType=='single':
                self.singleImageSizeRange(fileNoStart,fileNoStop,
                                                     fileNoBG,param)
                radialSizes=np.abs(self.firstImageRadialFitParams[:,:,2])
                axialSizes=np.abs(self.firstImageAxialFitParams[:,:,2])
                self.singleImageNumberRange(fileNoStart,fileNoStop,
                                                   fileNoBG,param)
                numbers=self.firstImageNumbers
            elif self.trigType=='double':
                self.doubleImageSizeRange(fileNoStart,fileNoStop,
                                                     fileNoBG,param)
                radialSizes=np.abs(self.secondImageRadialFitParams[:,:,2])
                axialSizes=np.abs(self.secondImageAxialFitParams[:,:,2])
                self.doubleImageNumberRange(fileNoStart,fileNoStop,
                                                   fileNoBG,param)
                numbers=self.secondImageNumbers
        elif self.imagingType=='Absorption':
            self.trippleImageSizeRange(fileNoStart,fileNoStop,
                                                     fileNoBG,param)
            radialSizes=np.abs(self.firstImageRadialFitParams[:,:,2])
            axialSizes=np.abs(self.firstImageAxialFitParams[:,:,2])
            self.trippleImageNumberRange(fileNoStart,fileNoStop,
                                                   fileNoBG,param)
            numbers=self.firstImageNumbers
        vol=(2*np.pi)**(1.5)*axialSizes*radialSizes**2
        self.imageNumberDensity=numbers/vol
        self.meanNumberDensity=\
        np.mean(self.imageNumberDensity,axis=1)
        self.stdErrorNumberDensity=\
        np.std(self.imageNumberDensity,axis=1)\
        /np.sqrt(np.shape(self.firstImageNumbers)[1])
            
        self.displayImageNumbersVariation(self.paramVals,
                                          self.meanNumberDensity,
                                          self.stdErrorNumberDensity)
        if self.showFirstImage:
            self.displayImage('First Image',self.firstImage)
            
        if self.showSizeFitsFirstImage:
            self.displaySizeFits('First Image',
                                    self.firstImageRadialX,
                                    self.firstImageRadialY,
                                    self.firstImageAxialX,
                                    self.firstImageAxialY,
                                    self.firstImageRadialFitParams,
                                    self.firstImageAxialFitParams)

    def singleImageLifetimes(self,fileNoStart,fileNoStop,
                             fileNoBG,param,shotsPerImage,t0,dt):
        bg,_=self.readFromZip(fileNoBG)
        noShots=int(int(np.shape(bg)[0])/shotsPerImage)
        bg=np.mean(bg,axis=0)
        t=np.array([t0+i*dt for i in range(0,shotsPerImage-1)])
        tI=np.linspace(np.min(t),np.max(t),100)
        paramsValList=[]
        lifetimesList=[]
        errorList=[]
        for fileNo in range(fileNoStart,fileNoStop+1):
            images,paramsDict=self.readFromZip(fileNo)
            paramsValList.append(paramsDict[param])
            images-=bg
            k=0
            N_list=[]
            for i in range(noShots):
                imageArray=images[k:k+shotsPerImage,:,:]
                N=self.getImageNumber(self.cropImages(imageArray))
                N=N[1:]/N[1]
                N_list.append(N)
                k+=shotsPerImage
            N_mean=np.mean(N_list,axis=0)
            N_std=np.std(N_list,axis=0)/np.sqrt(noShots)
            popt,diagpcov,isFit=expFit(t,N_mean)
            lifetimesList.append(popt[0])
            errorList.append(np.sqrt(diagpcov[0]))
        self.singleImageLifetimesList=np.array(lifetimesList)
        self.singleImageLifetimesError=np.array(errorList)
        self.paramVals=np.array(paramsValList)
        if fileNoStart==fileNoStop:
            fig,ax=plt.subplots(1,1,figsize=self.figSizePlot)
            ax.errorbar(t/self.xScale,N_mean,yerr=N_std,fmt='og')
            ax.plot(tI/self.xScale,exponential(tI,*popt),'-k')
            ax.legend(['lifetime :{0:.3}'.format(popt[0]/self.xScale)+u"\u00B1"+"{0:.3f} [s.u]".format(np.sqrt(diagpcov[0])/self.xScale),'numbers'])
            ax.set_xlabel(self.xLabel)
            ax.set_ylabel(self.yLabel)

    def lifetime(self,paramVals,meanNumbers,stdErrorNumbers):
        paramValsFine=np.linspace(np.min(paramVals),np.max(paramVals),100)
        popt,diagpcov,isFit=expFitOffset(paramVals,meanNumbers,stdErrorNumbers)
        bound_upper = exponentialOffset(paramValsFine, *(popt + np.sqrt(diagpcov)))
        bound_lower = exponentialOffset(paramValsFine, *(popt - np.sqrt(diagpcov)))
        yFit=exponentialOffset(paramValsFine,*popt)
        self.fitParams=popt
        fig,ax=plt.subplots(figsize=self.figSizePlot)
        ax.errorbar(paramVals/self.xScale,meanNumbers/self.yScale,
                    yerr=stdErrorNumbers/self.yScale,fmt=self.fmtP)
        ax.plot(paramValsFine/self.xScale,yFit/self.yScale,'-r')
        ax.fill_between(paramValsFine/self.xScale,
                        bound_lower/self.yScale,
                        bound_upper/self.yScale,
                        color='r',alpha=0.15)
        ax.set_xlabel(self.xLabel)
        ax.set_ylabel(self.yLabel)
        l = "Lifetime: {0:.3f}".format(popt[2]/self.xScale)+\
            u"\u00B1"+"{0:.3f} [s.u]".format(np.sqrt(diagpcov[2])/self.xScale)
        ax.set_title(l)
        

    def displayImageNumbersVariation(self,paramVals,numbers,stdErrorNumbers):
        if self.display:
            fig,ax=plt.subplots(figsize=self.figSizePlot)
            ax.errorbar(paramVals/self.xScale,
                        numbers/self.yScale,
                        yerr=stdErrorNumbers/self.yScale,
                        fmt=self.fmtP)
            ax.set_xlabel(self.xLabel)
            ax.set_ylabel(self.yLabel)

    def displaySingleImageSizeVariation(self):
        if self.display:
            fig,ax=plt.subplots(1,1,figsize=self.figSizePlot)
            fig.subplots_adjust(hspace=0.01,wspace=0.01)
            
            ax.errorbar(self.paramVals/self.xScale,
                        self.firstImageMeanRadialSizes/self.yScale,
                        yerr=self.firstImageStdErrorRadialSizes/self.yScale,
                        fmt=self.fmtP)
            ax.errorbar(self.paramVals/self.xScale,
                        self.firstImageMeanAxialSizes/self.yScale,
                        yerr=self.firstImageStdErrorAxialSizes/self.yScale,
                        fmt=self.fmtS)
            ax.legend(['Radial','Axial'])
            ax.set_xlabel(self.xLabel)
            ax.set_ylabel(self.yLabel)

    def displayDoubleImageSizeVariation(self):
        if self.display:
            fig,ax=plt.subplots(1,2,figsize=self.figSizePlot)
            fig.subplots_adjust(hspace=0.01,wspace=0.01)
            ax[1].yaxis.tick_right()
            ax[1].yaxis.set_label_position("right")
            
            ax[0].set_title('First Image')
            ax[0].errorbar(self.paramVals/self.xScale,
                        self.firstImageMeanRadialSizes/self.yScale,
                        yerr=self.firstImageStdErrorRadialSizes/self.yScale,
                        fmt=self.fmtP)
            ax[0].errorbar(self.paramVals/self.xScale,
                        self.firstImageMeanAxialSizes/self.yScale,
                        yerr=self.firstImageStdErrorAxialSizes/self.yScale,
                        fmt=self.fmtS)
            
            ax[1].set_title('Second Image')
            ax[1].errorbar(self.paramVals/self.xScale,
                        self.secondImageMeanRadialSizes/self.yScale,
                        yerr=self.secondImageStdErrorRadialSizes/self.yScale,
                        fmt=self.fmtP)
            ax[1].errorbar(self.paramVals/self.xScale,
                        self.secondImageMeanAxialSizes/self.yScale,
                        yerr=self.secondImageStdErrorAxialSizes/self.yScale,
                        fmt=self.fmtS)
            
            ax[0].legend(['Radial','Axial'])
            ax[0].set_xlabel(self.xLabel)
            ax[0].set_ylabel(self.yLabel)
            ax[1].legend(['Radial','Axial'])
            ax[1].set_xlabel(self.xLabel)
            ax[1].set_ylabel(self.yLabel)

    def displayImage(self,title,images):
        if self.display:
            l,m,_,_=np.shape(images)
            fig,ax=plt.subplots(l,m,figsize=self.figSizeImage,
                                sharex=True,sharey=True)
            fig.tight_layout(rect=[0, 0.01, 1, 0.95])
            fig.suptitle(title)
            fig.subplots_adjust(hspace=0.01,wspace=0)
            minn=np.min(images)
            maxx=np.max(images)
            for i in range(l):
                for j in range(m):
                    im=ax[i,j].imshow(images[i,j,:,:],cmap='jet',
                                    interpolation='nearest',vmin=minn,vmax=maxx)
                    ax[i,j].axis('off')
            fig.colorbar(im, ax=ax.ravel().tolist(),orientation='horizontal')

    def displaySizeFits(self,title,radialX,radialY,axialX,axialY,
                        radialFitParams,axialFitParams):
        if self.display:
            l,m,_=np.shape(radialFitParams)        
            fig,ax=plt.subplots(l,m,figsize=self.figSizeImage,sharex=True)
            fig.tight_layout(rect=[0, 0.01, 1, 0.95])
            fig.suptitle(title)
            fig.subplots_adjust(hspace=0.01,wspace=0.01)
            for i in range(l):
                for j in range(m):
                    radialYFits=gaussianOffset(radialX[i,j,:],
                                            *radialFitParams[i,j,:])
                    axialYFits=gaussianOffset(axialX[i,j,:],
                                            *axialFitParams[i,j,:])
                    ax[i,j].plot(radialX[i,j,:]*1e3,
                                radialY[i,j,:],'--k')
                    ax[i,j].plot(axialX[i,j,:]*1e3,
                                axialY[i,j,:],'--r')
                    ax[i,j].plot(radialX[i,j,:]*1e3,
                                radialYFits,'-k')
                    ax[i,j].plot(axialX[i,j,:]*1e3,
                                axialYFits,'-r')
                    ax[i,j].set_yticks([])
                    if i==l-1:
                        ax[i,j].set_xlabel('distance [mm]')
                    if i==0 and j==0:
                        ax[i,j].legend(['Radial','Axial','RadialFit','AxialFit'])
    
    
    def __call__(self,
                 fileNoStart,
                 fileNoStop,
                 fileNoBg,
                 param,
                 imagingType,
                 requirement='Number',
                 trigType='single',
                 fit=False,
                 fitType='lin',
                 showFirstImage=False,
                 showSecondImage=False,
                 showSizeFitsFirstImage=False,
                 showSizeFitsSecondImage=False,
                 diffStr='',
                 extParam='give a name',
                 extParamVals=[],
                 fileNoExclude=[],
                 figSizeImage=(15,20),
                 figSizePlot=(8,12),
                 fmtP='ok',
                 fmtS='or',
                 xLabel='X',
                 yLabel='Y',
                 xScale=1e2,
                 yScale=1,
                 smoothingWindow=11,
                 od_correction=False,
                 display=True,
                 **kwargs):
        self.imagingType=imagingType
        self.trigType=trigType
        self.fit=fit
        self.fitType=fitType
        self.diffStr=diffStr
        self.showFirstImage=showFirstImage
        self.showSecondImage=showSecondImage
        self.showSizeFitsFirstImage=showSizeFitsFirstImage
        self.showSizeFitsSecondImage=showSizeFitsSecondImage
        self.figSizeImage=figSizeImage
        self.figSizePlot=figSizePlot
        self.fmtP=fmtP
        self.fmtS=fmtS
        self.xScale=xScale
        self.yScale=yScale
        self.xLabel=xLabel
        self.yLabel=yLabel
        self.isLifetime=False
        self.isTemperature=False
        self.fileNoExclude=fileNoExclude
        self.smoothingWindow=smoothingWindow
        self.kwargs=kwargs
        self.od_correction = od_correction
        self.display = display
        self.extParamVals = np.array(extParamVals)
        if hasattr(self, 'detuningInVolt'):
            if self.detuningInVolt != 'None':
                self.s0=(1+4*(self.detuningInVolt*self.detuningFrequencyScaling)**2/\
                    self.gamma**2)/(3*self.lamda**2/(2*np.pi))
        if requirement=='Number':
            self.getNumber(fileNoStart,fileNoStop,fileNoBg,param)
        elif requirement=='Size':
            self.getSize(fileNoStart,fileNoStop,fileNoBg,param)
        elif requirement=='Temperature':
            self.isTemperature=True
            self.fit=False
            self.getSize(fileNoStart,fileNoStop,fileNoBg,param)
        elif requirement=='Lifetime':
            self.isLifetime=True
            self.fit=False
            self.getNumber(fileNoStart,fileNoStop,fileNoBg,param)
        elif requirement=='Density':
            self.density(fileNoStart,fileNoStop,fileNoBg,param)
        elif requirement=='MOTLifetimes':
            self.singleImageLifetimes(fileNoStart,fileNoStop,fileNoBg,
                                      param,self.kwargs['shotsPerImage'],
                                      self.kwargs['t0'],self.kwargs['dt'])
        else:
            print('Unknown requirement',requirement)
        return self


def analysisWithDefaultCaFSettings():
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
    analysis.crop=False
    analysis.cropCentre=(65,65)
    analysis.cropHeight=50
    analysis.cropWidth=50
    analysis.massInAMU=59
    analysis.diffStr='C'
    analysis.smoothingWindow=11
    return analysis

def analysisWithDefaultRbSettings():
    analysis=Analysis()
    analysis.pixelSize=6.45e-6
    analysis.binSize=2
    analysis.magFactor=0.41
    analysis.crop=False
    analysis.cropCentre=(220,320)
    analysis.cropHeight=120
    analysis.cropWidth=120
    analysis.detuningInVolt=0
    analysis.detuningFrequencyScaling=14.7e6
    analysis.gamma=6e6
    analysis.lamda=780e-9
    analysis.massInAMU=86.9
    analysis.diffStr='R'
    analysis.smoothingWindow=11
    return analysis

if __name__=='__main__':
    analysis=analysisWithDefaultCaFSettings()
    analysis.dirPath='./trialData'
    analysis.fileNameString='CaF04Oct1900'
    a=analysis(requirement='Number',
                 fileNoStart=45,
                 fileNoStop=63,
                 fileNoBg=23,
                 param="Gigatronics Synthesizer 1 - Frequency (MHz)",
                 imagingType='Fluoresence',
                 trigType='double',
                 fit=True,
                 fitType='invSinc',
                 showFirstImage=False,
                 showSecondImage=False,
                 showSizeFitsFirstImage=False,
                 showSizeFitsSecondImage=False,
                 diffStr='C',
                 extParam='give a name',
                 extParamVals=[],
                 fileNoExclude=[],
                 figSizeImage=(15,20),
                 figSizePlot=(8,5),
                 xLabel='expansion time [ms]',
                 yLabel='Normalised No',
                 xScale=1,
                 yScale=1)





















