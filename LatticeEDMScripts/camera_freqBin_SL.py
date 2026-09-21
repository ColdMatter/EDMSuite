# -*- coding: utf-8 -*-
"""
Created on Fri Mar 20 14:48:10 2026

Post processing for MOT search camera data, using wavemeter recordings
from PMT data.

The aim is to bin images in frequency. (Not realised yet)
- For now just got the averaged frequency and error per file, since taking 4
  images takes 4 seconds, and frequency would have drifted already.

The camera image files must have a corresponding PMT data file with the same
ID number.

@author: sl5119 (Simeng)
"""

#%% Import libraries
import sys
import os
import re

OneDriveFolder = os.environ['onedrive']
sys.path.append(OneDriveFolder + r"\Desktop\EDMSuite\LatticeEDMScripts")
import LatticeEDM_analysis_library as EDM

import numpy as np

import glob
import matplotlib.pyplot as plt
from scipy.optimize import curve_fit

import csv

import tools as tools

tools.set_plots()

prop_cycle = plt.rcParams['axes.prop_cycle']
colors = prop_cycle.by_key()['color']

import tifffile as tiff
from matplotlib.colors import TwoSlopeNorm
import matplotlib.patches as patches

from scipy.ndimage import gaussian_filter

import io
import zipfile
import xml.etree.ElementTree as ET
from tkinter import Tk     # from tkinter import Tk for Python 3.x
from tkinter.filedialog import askopenfilename,askopenfilenames

import xmltodict

import pandas as pd
from tqdm import tqdm 

from matplotlib.widgets import EllipseSelector

def get_tiff():
    """
    Open a GUI to select a TIFF image stack and extract useful metadata.
    """
    root = Tk()
    root.withdraw()
    root.call('wm', 'attributes', '.', '-topmost', True)

    file_path = askopenfilename(
        title="Select a TIFF image stack",
        filetypes=[("TIFF files", "*.tif *.tiff")]
    )

    if not file_path:
        return None, None, None

    file_name = os.path.basename(file_path)
    file_date = " ".join(file_path.split(os.sep)[-3:-1])

    return file_path, file_name, file_date


from tkinter import filedialog

def get_tiff_all(keyword, extension, selection=[]):
    """
    Open a GUI to select a TIFF image stack and extract useful metadata.
    """
    root = Tk()
    root.withdraw()
    root.call('wm', 'attributes', '.', '-topmost', True)

#    file_path = askopenfilename(
#        title="Select a TIFF image stack",
#        filetypes=[("TIFF files", "*.tif *.tiff")]
#    )
    
    folder_path = filedialog.askdirectory()
    
    wanted_extensions = ('.tif')  # add more if needed

    filtered_file_paths = []
    for root_dir, dirs, files in os.walk(folder_path):
        for file in files:
            if (
                    file.lower().endswith(wanted_extensions)
                    and keyword.lower() in file.lower()
                    ):
                filtered_file_paths.append(os.path.join(root_dir, file))
            
    #print(filtered_files)

    #print(all_files)
    
    if not filtered_file_paths:
        return None, None, None

    file_names = []
    for file in filtered_file_paths:
        file_names.append(os.path.basename(file))
#    file_dates = " ".join(filtered_file_paths.split(os.sep)[-3:-1])

    if len(selection) > 0:
        selected_file_paths = []
        selected_file_names = []
        for sele in selection:
            for i in range(0, len(file_names)):
                if file_names[i][:3] == sele:
                    selected_file_paths.append(filtered_file_paths[i])
                    selected_file_names.append(file_names[i])
        return selected_file_paths, selected_file_names, folder_path
    else:
        return filtered_file_paths, file_names, folder_path#, file_dates
    
def bin_2d(arr, bin_h, bin_w, mode="mean"):
    h, w = arr.shape
    
    # Trim array so it's divisible
    arr = arr[:h - h % bin_h, :w - w % bin_w]
    
    # Reshape into blocks
    reshaped = arr.reshape(
        arr.shape[0] // bin_h, bin_h,
        arr.shape[1] // bin_w, bin_w
    )
    
    if mode == "mean":
        return reshaped.mean(axis=(1, 3))
    elif mode == "sum":
        return reshaped.sum(axis=(1, 3))
    else:
        raise ValueError("mode must be 'mean' or 'sum'")

#%% Selection
#sele = ["003", "004", "005", "006", "007", "008", "009", "010", "011", "012"]
sele = ["002", "004", "005", "006"]

#%% Set camera parameters
#Set Gaussian filtering parameters
Sig1 = 5
Sig2 = 5
SigDiff = 5
SigAdd = 5

#Use the following for pre 18 Mar 2026, 016 file:
#Sig1 = 10
#Sig2 = 10
#SigDiff = 10
#SigAdd = 10


k1 = 1
k2 = 1
kDiff = 1
kAdd = 1

#Set color bar scale
Max = 100
Min = -100

Max2 = 60
Min2 = -60

#%% Get camera images
imagePaths, imageNames, folderPath = get_tiff_all("TOF", ".tif", selection=sele)

if imagePaths is None:
    raise ValueError("Aucun fichier TIFF sélectionné.")

#%% Load PMT data
#datadrive=str(os.environ["Onedrive"]+"\\Desktop\\Lattice EDM\\data")
#month="Mar 2026" #"Slowing data to publish\\Durations\\Free space slowing\\New V2 scheme"#
#date="19"
#subfolder = ""
#blockdrive=datadrive+"\\BlockData\\"

#drive = datadrive + "\\" + month + "\\" + date + "\\" + subfolder
drive = folderPath + r"/"
print(drive)
date = re.split(r'[/]', drive)[-2]
month = re.split(r'[/]', drive)[-3]

pattern="*TOF*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])

#%%
if len(files) > 0:
    print("%g matching files found. Loading"%len(files))
    Data = {}
    fileLabels = []
    Lasers = []
    for i in range(0, len(files)):
        fileLabel = re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[0]
        Laser = re.split(r'[.]', re.split(r'[_]', re.split(r'[\\]', files[i])[-1])[-2])[0]
       
       #Use this part if have selections
        if len(sele) > 0:
            for j in range(0, len(sele)):
                if fileLabel == sele[j]:
                    print("File "+fileLabel+" selected")
           ###
                    Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
                    print("loaded file " + files[i])
                    fileLabels.append(fileLabel)
                    Lasers.append(Laser)
        else:
            print("No further selection applied.")
            Data[fileLabel] = EDM.ReadAverageScanInZippedXML(files[i])
            print("loaded file " + files[i])
            fileLabels.append(fileLabel)
            Lasers.append(Laser)

else:
    print("No matching files.")

#%% Extract wavemeter readings
WMTHz = {}
WMMHz = {}
WMMHzAvg = {}  # in format of [average, standard deviation, standard error of mean]
WMTHzAll = []
trim = 1
RestFreq = 542.809124 #in THz

for i in range(0, len(sele)):  #for i in range(0, len(files)):
    Scan = Data[fileLabels[i]]
    print('Extract WM reading (THz) for file ' + re.split(r'[\\]', fileLabels[i])[-1])   #files[i]
    Settings = EDM.GetScanSettings(Scan)
    WMTHz[fileLabels[i]] = EDM.GetScanFreqArrayTHz(Scan)[trim:]  #This is in THz
    WMMHz[fileLabels[i]] = (EDM.GetScanFreqArrayTHz(Scan)[trim:] - RestFreq) * 1e6 # Relative freq in MHz
    WMTHzAll.append(WMTHz[fileLabels[i]])
    WMMHzAvg[fileLabels[i]] = [np.average(WMMHz[fileLabels[i]]),\
                               np.std(WMMHz[fileLabels[i]]),\
                               np.std(WMMHz[fileLabels[i]])/np.sqrt(len(WMMHz[fileLabels[i]]))]

#%
#WMTHzAll = tools.flattenList(WMTHzAll)

#%% Load all camera images
# 1, 2 are B-field configurations
# A, B are YAG On-Off
PrintTrue = False
PlotTrue = True
Filter = False
Mask = False
Binning = True

#ROI
xStart=20;
xEnd=100;
yStart=40;
yEnd=100;

#mask threshold
M = 17000

#bin size in 1 direction
binsize = 4

#Raw images, sorted
A1s = {}
B1s = {}
A2s = {}
B2s = {}

#Averaged images, no filter
Signal_1s = {}
Signal_2s = {}

#Averaged images, can be binned or filtered if options are selected above
Filtered_sig_1s = {}
Filtered_sig_2s = {}
Filtered_diffs = {}

Filtered_ROIs = {}
AvgROIcounts = {}


#At this point the loaded images and PMT files should have matching IDs
for i in range(0, len(imagePaths)):
    file_path = imagePaths[i]
    fileID = imageNames[i][:3]

    # Séparation des configurations
    stack_path1 = file_path
    stack1 = tiff.imread(stack_path1)
    
    #If the first indices are 4, 5, 6, 7, it means the first set of images
    # are discarded. Guanchen believes they should be thrown away.
    config_A1 = stack1[4::4] #[4::4]
    config_B1 = stack1[5::4] #[5::4]
    
    config_A2 = stack1[6::4] #[6::4]
    config_B2 = stack1[7::4] #[7::4]
    
    # Conversion en float pour éviter le wrap-around (removed wrapping for now)
    config_A1 = config_A1.astype(np.float64)
    config_B1 = config_B1.astype(np.float64)
    
    config_A2 = config_A2.astype(np.float64) 
    config_B2 = config_B2.astype(np.float64)
    
    A1s[fileID] = config_A1
    B1s[fileID] = config_B1
    A2s[fileID] = config_A2
    B2s[fileID] = config_B2
    
    # Calcul du signal -- YAG On-Off background removal
    signal_bfield1 = np.mean(config_A1 - config_B1, axis=0)
    signal_bfield2 = np.mean(config_A2 - config_B2, axis=0)
    
    if Mask:
        # Mask off high scatter region
        mask = config_B1[1]<M
        signal_bfield2[~mask] = 0
        signal_bfield1[~mask] = 0
    #signal_bfield1 = np.mean(config_A1, axis=0)#[50:170,20:170]
    #signal_bfield2 = np.mean(config_A2, axis=0)#[50:170,20:170]
    
    #Rotate to the correct orientation
    signal_bfield1_rot90=np.rot90(signal_bfield1,k=k1)
    signal_bfield2_rot90=np.rot90(signal_bfield2,k=k2)
    
    Signal_1s[fileID] = signal_bfield1_rot90
    Signal_2s[fileID] = signal_bfield2_rot90

    if Filter:
        #apply Gaussian filter
        filtered_image_array_config1_rot90= gaussian_filter(signal_bfield1_rot90, sigma=Sig1)
        filtered_image_array_config2_rot90 = gaussian_filter(signal_bfield2_rot90, sigma=Sig2)
    
        diff = filtered_image_array_config1_rot90 - filtered_image_array_config2_rot90
        diff_rot90 = diff
    
        filtered_image_array_diff = diff_rot90
        filtered_image_array_diff_rot90 = diff_rot90
        
        Filtered_sig_1s[fileID] = filtered_image_array_config1_rot90
        Filtered_sig_2s[fileID] = filtered_image_array_config2_rot90
        Filtered_diffs[fileID] = filtered_image_array_diff_rot90
        
        add = (signal_bfield1+signal_bfield2)/2
        filtered_image_array_add = gaussian_filter(add, sigma=SigAdd)
        add_rot90 = np.rot90(add,k=1)
        filtered_image_array_add_rot90 = np.rot90(filtered_image_array_add,k=kAdd)
    
    if Binning:
        filtered_image_array_config1_rot90= bin_2d(signal_bfield1_rot90, binsize, binsize, mode="sum")
        filtered_image_array_config2_rot90 = bin_2d(signal_bfield2_rot90, binsize, binsize, mode="sum")
        
        diff = filtered_image_array_config1_rot90 - filtered_image_array_config2_rot90
        diff_rot90 = diff
    
        filtered_image_array_diff = diff_rot90
        filtered_image_array_diff_rot90 = diff_rot90
        
        Filtered_sig_1s[fileID] = filtered_image_array_config1_rot90
        Filtered_sig_2s[fileID] = filtered_image_array_config2_rot90
        Filtered_diffs[fileID] = filtered_image_array_diff_rot90
        
        add = (signal_bfield1+signal_bfield2)/2
        filtered_image_array_add = bin_2d(add, binsize, binsize, mode="sum")
        add_rot90 = np.rot90(add,k=1)
        filtered_image_array_add_rot90 = np.rot90(filtered_image_array_add,k=kAdd)
    
    else:
        #Just rename
        filtered_image_array_config1_rot90= signal_bfield1_rot90
        filtered_image_array_config2_rot90 = signal_bfield2_rot90
    
        diff = filtered_image_array_config1_rot90 - filtered_image_array_config2_rot90
        diff_rot90 = diff
    
        filtered_image_array_diff = diff_rot90
        filtered_image_array_diff_rot90 = diff_rot90
        
        add = (signal_bfield1+signal_bfield2)/2
        filtered_image_array_add = add
        add_rot90 = np.rot90(add,k=1)
        filtered_image_array_add_rot90 = np.rot90(filtered_image_array_add,k=kAdd)
        
    if PlotTrue:
        fig, axes = plt.subplots(2, 2,figsize=(10,10))
    
        if PrintTrue:
            print(f"average counts per superPixel in the filtered config.2 image: \
                  {np.mean(filtered_image_array_config2_rot90):.3f}")
            print(f"average counts per superPixel in the filtered diff image: \
                {np.mean(filtered_image_array_diff):.3f}")
            print(f"std counts per superPixel in the filtered diff image: \
                  {np.std(filtered_image_array_diff):.2f}")
        
        if PrintTrue:
            print(f"average counts per superPixel in the filtered add image: \
                  {np.mean(filtered_image_array_add):.2f}")  
            print(f"std counts per superPixel in the filtered add image: \
                  {np.std(filtered_image_array_add):.2f}")
    
        norm1 = TwoSlopeNorm(vmin=Min, vcenter=0, vmax=Max)
        im=axes[0,0].imshow(filtered_image_array_config1_rot90, cmap='seismic',norm= norm1, origin='lower')
        axes[0,0].set_title("Filtered image: \n Bfield config.1 \n(ygOn-ygOff)", fontsize=16)
        plt.colorbar(im)
        im=axes[0,1].imshow(filtered_image_array_config2_rot90, cmap='seismic',norm= norm1, origin='lower' )
        axes[0,1].set_title("Filtered image: \n Bfield config.2 \n(ygOn-ygOff)", fontsize=16)
        plt.colorbar(im)
    
        im=axes[1,0].imshow(filtered_image_array_add_rot90, cmap='seismic',norm= norm1, origin='lower')
        axes[1,0].set_title("Filtered image: \n Bfield (config.1 + config.2)/2", fontsize=16)
        plt.colorbar(im)
    
        im=axes[1,1].imshow(filtered_image_array_diff_rot90, cmap='seismic',norm= norm1, origin='lower')
        axes[1,1].set_title("Filtered image: \n Bfield (config.1 - config.2)", fontsize=16)
        plt.colorbar(im)
    
        fig.suptitle(fileID+"\n"+"rotation corrected image", fontsize=18)
        plt.show()
       
        if PrintTrue:
            print(f"the ratio of the mean counts per superPixel \
                  (filtered diff image / filtered config.2): \
                      {np.mean(filtered_image_array_diff)/np.mean(filtered_image_array_config2_rot90):.2f}")
    
            print("Total counts (diff) = %g"%np.sum(filtered_image_array_diff_rot90))
            print("Total photons (diff) = %g"%(np.sum(filtered_image_array_diff_rot90)/5))
            print("Total molecules (diff) = %g"%(np.sum(filtered_image_array_diff_rot90)/5/0.1764))
            print("*using V0 photon scatter only to estimate")
            
    imageOfInterest = filtered_image_array_diff_rot90
    normDiff = TwoSlopeNorm(vmin=Min, vcenter=0, vmax=Max)
    
    if Binning:
        xStartbin = int(xStart/binsize)
        xEndbin = int(xEnd/binsize)
        yStartbin = int(yStart/binsize)
        yEndbin = int(yEnd/binsize)
        
        xROI,yROI,wROI,hROI=xStartbin,yStartbin,xEndbin-xStartbin,yEndbin-yStartbin
        regionOfInterest = imageOfInterest[yStartbin:yEndbin,xStartbin:xEndbin]
    
    else:
        xROI,yROI,wROI,hROI=xStart,yStart,xEnd-xStart,yEnd-yStart
        regionOfInterest = imageOfInterest[yStart:yEnd,xStart:xEnd]
        
    rect = patches.Rectangle(
        (xROI, yROI), wROI, hROI,
        linewidth=2,
        edgecolor='red',
        facecolor='none'
    )

    
    avgCount = regionOfInterest.mean()
    #avgCountstd = regionOfInterest.std()
    
    Diff_array_mean = []
    for d in range(0, len(config_A1)):
        sig1 = np.rot90(config_A1[d] - config_B1[d])
        sig2 = np.rot90(config_A2[d] - config_B2[d])
        if Mask:
            sig1[~mask] = 0
            sig2[~mask] = 0
        
        if Filter:
            sig1_filtered = gaussian_filter(sig1, sigma=Sig1)
            sig2_filtered = gaussian_filter(sig2, sigma=Sig2)
            Diff_array_rot = sig1_filtered - sig2_filtered
            
            Diff_array_rot_cropped = Diff_array_rot[yStart:yEnd,xStart:xEnd]
            
        if Binning:
            sig1_binned = bin_2d(sig1, binsize, binsize, mode="sum")
            sig2_binned = bin_2d(sig2, binsize, binsize, mode="sum")
            Diff_array_rot = sig1_binned - sig2_binned
            
            xStartbin = int(xStart/binsize)
            xEndbin = int(xEnd/binsize)
            yStartbin = int(yStart/binsize)
            yEndbin = int(yEnd/binsize)
            
            Diff_array_rot_cropped = Diff_array_rot[yStartbin:yEndbin,xStartbin:xEndbin]
            
        else:
            Diff_array_rot = sig1 - sig2
            Diff_array_rot_cropped = Diff_array_rot[yStart:yEnd,xStart:xEnd]
            
        
        Diff_array_mean.append(Diff_array_rot_cropped.mean())
    
    avgCount = np.mean(Diff_array_mean)
    avgCountstd = np.std(Diff_array_mean)
    avgCountSEM = avgCountstd/np.sqrt(len(Diff_array_mean))
    
    Filtered_ROIs[fileID] = regionOfInterest
    AvgROIcounts[fileID] = [avgCount, avgCountstd, avgCountSEM]
    #in format [average, std, standard error of mean]
    
    row_mean=regionOfInterest.mean(axis=1)
    col_mean=regionOfInterest.mean(axis=0)
    
    if PlotTrue:
        fig, axes = plt.subplots(1, 2,figsize=(10,5.5))
        im=axes[0].imshow(imageOfInterest, cmap='seismic',norm= normDiff, origin='lower')
        axes[0].set_title("Image of interest\n(config 1 - 2)", fontsize=16)
        axes[0].add_patch(rect)
    
        im=axes[1].imshow(regionOfInterest, cmap='seismic',norm= normDiff, origin='lower')
        axes[1].set_title("averaged counts = %.3g,\n+-%.3g"%(avgCount, avgCountSEM),\
                          fontsize=16)
        plt.colorbar(im)
        if Binning:
            fig.suptitle("Region of interest: (%g - %g, %g - %g)"%(xStartbin, xEndbin, \
                                                               yStartbin, yEndbin), fontsize=18)
        else:
            fig.suptitle("Region of interest: (%g - %g, %g - %g)"%(xStart, xEnd, \
                                                               yStart, yEnd), fontsize=18)
        plt.show()

        fig = plt.figure(figsize=(6, 7.5))

        # Layout grid
        gs = fig.add_gridspec(
            2, 2,
            width_ratios=[1, 1.5],
            height_ratios=[1, 4],
            hspace=0.35,
            wspace=0.35,
        )

        ax_img  = fig.add_subplot(gs[1, 1])
        ax_col  = fig.add_subplot(gs[0, 1], sharex=ax_img)
        ax_row  = fig.add_subplot(gs[1, 0], sharey=ax_img)

        # --- Plot ROI ---
        ax_img.imshow(regionOfInterest, cmap='seismic',norm= normDiff, origin='lower')
        ax_img.set_title("ROI", fontsize=16)
        #ax_img.axis("off")

        # --- Column sum (right top) ---
        ax_col.plot(col_mean)
        ax_col.set_ylabel("Average")
        ax_col.tick_params(axis="x")
        ax_col.set_title("row projection", fontsize=16)

        # --- Row sum (top left) ---
        ax_row.plot(row_mean, range(len(row_mean)))
        ax_row.invert_xaxis()
        ax_row.tick_params(axis="y")
        ax_row.set_xlabel("Average")
        ax_row.set_title("col projection", fontsize=16)
        
        plt.colorbar(im)
        if Binning:
            fig.suptitle("File "+fileID+", ROI (%g - %g, %g - %g) projection "%(xStartbin, xEndbin, \
                                                               yStartbin, yEndbin), fontsize=18)
        else:
            fig.suptitle("File "+fileID+", ROI (%g - %g, %g - %g) projection "%(xStart, xEnd, \
                                                               yStart, yEnd), fontsize=18)
        
        plt.show()

    if PrintTrue:
        print(f"average counts per superPixel in the filtered config.2 image: {np.mean(filtered_image_array_config2_rot90[yStart:yEnd,xStart:xEnd]):.3f}")
        print(f"average counts per superPixel in the filtered diff image: {np.mean(filtered_image_array_diff[yStart:yEnd,xStart:xEnd]):.3f}")
        print(f"the ratio of the mean counts per superPixel (ROI_filtered_diff_image / ROI_filtered_config.2): {np.mean(filtered_image_array_diff[yStart:yEnd,xStart:xEnd])/np.mean(filtered_image_array_config2_rot90[yStart:yEnd,xStart:xEnd]):.2f}")
        
        print(f"average counts per superPixel in the filtered diff ROI image: {np.mean(regionOfInterest):.2f}")

#%%
binsize = 2



binned_diff = bin_2d(filtered_image_array_diff_rot90, binsize, binsize, mode="sum")

norm2 = TwoSlopeNorm(vmin=-1000, vcenter=0, vmax=1000)
plt.imshow(binned_diff, cmap='seismic',norm= norm2, origin='lower')
plt.colorbar()
plt.title("Binned image -- %g x %g pixles for one bin, summed\n file 039"%(binsize, binsize))
plt.show()

#%% Plot Average Counts Against Average Frequncy
freq = []
freqerr = []
AvgCount = []
AvgCounterr = []

for i in range(0, len(imagePaths)):
    fileID = imageNames[i][:3]
    freq.append(WMMHzAvg[fileID][0])
    freqerr.append(WMMHzAvg[fileID][2])
    AvgCount.append(AvgROIcounts[fileID][0])
    AvgCounterr.append(AvgROIcounts[fileID][2])
    
plt.plot(freq, AvgCount, '.')
plt.errorbar(freq, AvgCount, xerr=freqerr, yerr=AvgCounterr, fmt=' ', color=colors[0],\
             capsize = 5)
plt.xlabel('Frequency (MHz) from rest frame freqeuncy 542.809124THz')
plt.ylabel('Average photon counts\n per super pixel in ROI')
plt.title('Average photon counts VS MOT detuning')

plt.show()
    
#%% Save multi-day
Freqs = {}
Freqerrs = {}
AvgCounts = {}
AvgCounterrs = {}
Days = []
#%%
Date = date + ' ' + month + " 20ms"
#
Days.append(Date)
#%
Freqs[Date] = freq
Freqerrs[Date] = freqerr
AvgCounts[Date] = AvgCount
AvgCounterrs[Date] = AvgCounterr

#%% Show stacked
for i in range(0, len(Days)):
    d = Days[i]
    plt.plot(Freqs[d], AvgCounts[d], '.', color=colors[i], label=d)
    plt.errorbar(Freqs[d], AvgCounts[d], xerr=Freqerrs[d], yerr=AvgCounterrs[d],\
                 fmt=' ', color=colors[i],\
                 capsize = 5)

plt.xlabel('Frequency (MHz) from rest frame freqeuncy 542.809124THz')
plt.ylabel('Average photon counts\n per super pixel in ROI')
plt.title('Average photon counts VS MOT detuning')
plt.legend(bbox_to_anchor=(1, 1.05))
plt.show()

#%% Truncate by experimental setting
#columns = ["35ms end", "34ms end", "33ms end", "no B-field"]
columns = ["10ms", "15ms", "18ms", "20ms", "23m", "26ms"]
Save_path = "C:\\Users\\sl5119\\Box\\LatticeEDM\\data\\MOT search\\Table 1\\"
Save_columns = ["Frequency (MHz)", "Freq err (MHz)", "Average Counts", "Avg Counts err"]

ToSave = True

for j in range(0, len(columns)):
    f = []
    ferr = []
    C = []
    Cerr = []
    c = columns[j]
    
    for i in range(0, len(Days)):
        d = Days[i]
        if d[12:] == c:
            f.append(Freqs[d])
            ferr.append(Freqerrs[d])
            C.append(AvgCounts[d])
            Cerr.append(AvgCounterrs[d])
            
    f = tools.flattenList(f)
    ferr = tools.flattenList(ferr)
    C = tools.flattenList(C)
    Cerr = tools.flattenList(Cerr)
        
    plt.plot(f, C, '.', color=colors[j], label=c)
    plt.errorbar(f, C, xerr=ferr, yerr=Cerr,\
                 fmt=' ', color=colors[j],\
                 capsize = 5)
    #Fill-between doesn't work when x values aren't in ascending order
    #plt.fill_between(f, np.array(C)+np.array(Cerr),\
    #                 np.array(C)-np.array(Cerr), color=colors[j], alpha=0.5)
    
    if ToSave:
        Save_dict = {}
        Save_dict[Save_columns[0]] = f
        Save_dict[Save_columns[1]] = ferr
        Save_dict[Save_columns[2]] = C
        Save_dict[Save_columns[3]] = Cerr
        
        df = pd.DataFrame(Save_dict)
        df.to_csv(Save_path+c+".csv", index=False, sep=',')

        
plt.xlabel('Frequency (MHz) from rest frame freqeuncy 542.809124THz')
plt.ylabel('Average photon counts\n per super pixel in ROI')
plt.title('Average photon counts VS MOT detuning\n -- scan slowing time and starts at 12.5ms')
plt.legend(bbox_to_anchor=(1, 1.05))
plt.show()

#%% Further averaging, over different dates for the same setting
#For Table 2/3, 18ms slowing, 35ms end time, 542.809120THz
Mar19sele = ["003", "021", "022", "023"]

ToSum = np.zeros(shape = np.shape(regionOfInterest), dtype=float)

for i in range(0, len(sele)):
    s = sele[i]
    for j in Mar19sele:
        if j == s:
            ToSum += Filtered_ROIs[j]

plt.imshow(ToSum/len(Mar19sele), cmap='seismic',norm= normDiff, origin='lower')
plt.colorbar()
plt.title("Further averaged image, Mar 19th 2026\n -- 18ms slowing, 35ms end, 542.809120THz")
plt.show()

#%% Add Mar 20th 2026 data as well
Mar20sele = ["003", "004", "011", "012"]
ToSum2 = np.zeros(shape = np.shape(regionOfInterest), dtype=float)
for i in range(0, len(sele)):
    s = sele[i]
    for j in Mar19sele:
        if j == s:
            ToSum += Filtered_ROIs[j]
            ToSum2 += Filtered_ROIs[j]

plt.imshow(ToSum/(len(Mar19sele)+len(Mar20sele)), cmap='seismic',norm= normDiff, origin='lower')
plt.colorbar()
plt.title("Further averaged image, Mar 19-20th 2026\n -- 18ms slowing, 35ms end, 542.809120THz")
plt.show()

#%%
plt.imshow(ToSum2/len(Mar20sele), cmap='seismic',norm= normDiff, origin='lower')
plt.colorbar()
plt.title("Further averaged image, Mar 20th 2026\n -- 18ms slowing, 35ms end, 542.809120THz")
plt.show()

#%% Add Mar 23rd

#%% Show averaged raw images
Max2 = 1e4
Min2 = -10

for s in sele:
    print(s)
    A1 = np.mean(A1s[s], axis=0)
    B1 = np.mean(B1s[s], axis=0)
    A2 = np.mean(A2s[s], axis=0)
    B2 = np.mean(B2s[s], axis=0)
    
    fig, axes = plt.subplots(2, 2,figsize=(10,10))
    norm1 = TwoSlopeNorm(vmin=Min2, vcenter=0, vmax=Max2)
    im=axes[0,0].imshow(np.rot90(A1), cmap='seismic',norm= norm1, origin='lower')
    axes[0,0].set_title("Unfiltered image: \n Bfield config.1, ygOn", fontsize=16)
    plt.colorbar(im)
    im=axes[0,1].imshow(np.rot90(A2), cmap='seismic',norm= norm1, origin='lower' )
    axes[0,1].set_title("Unfiltered image: \n Bfield config.2, ygOn", fontsize=16)
    plt.colorbar(im)

    im=axes[1,0].imshow(np.rot90(B1), cmap='seismic',norm= norm1, origin='lower')
    axes[1,0].set_title("Unfiltered image: \n Bfield config.1, ygOff", fontsize=16)
    plt.colorbar(im)

    im=axes[1,1].imshow(np.rot90(B2), cmap='seismic',norm= norm1, origin='lower')
    axes[1,1].set_title("Unfiltered image: \n Bfield config.2, ygOff", fontsize=16)
    plt.colorbar(im)

    fig.suptitle(s+"\n"+"unfiltered rotation corrected image", fontsize=18)
    plt.show()
    
    fig, axes = plt.subplots(1, 2,figsize=(10,5))
    norm1 = TwoSlopeNorm(vmin=Min, vcenter=0, vmax=Max)
    im=axes[0].imshow(np.rot90(A1-B1), cmap='seismic',norm= norm1, origin='lower')
    axes[0].set_title("Unfiltered image: \n Bfield config.1, ygOn-ygOff", fontsize=16)
    plt.colorbar(im)
    im=axes[1].imshow(np.rot90(A2-B2), cmap='seismic',norm= norm1, origin='lower' )
    axes[1].set_title("Unfiltered image: \n Bfield config.2, ygOn-ygOff", fontsize=16)
    plt.colorbar(im)
    fig.suptitle(s+"\n"+"unfiltered rotation corrected image, YAG sub", fontsize=18)
    plt.show()
