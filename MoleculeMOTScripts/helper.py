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
        print 'There are less number of images than required!'
    elif len(imgs)>(fileNoStart-fileNoStop+1)*NoImages:
        print 'There are more images than expected!'