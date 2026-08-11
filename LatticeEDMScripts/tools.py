#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Created on Wed Feb  8 22:20:14 2023

The grand tool kit

@author: Simeng Li
"""

# common packages
import numpy as np
from scipy.optimize import curve_fit
import scipy as sp
import pandas as pd
import matplotlib.pyplot as plt
import sys
import os
import glob
import copy
from itertools import chain

from tkinter import Tk     # from tkinter import Tk for Python 3.x
from tkinter.filedialog import askopenfilename,askopenfilenames
import tkinter as tk

#% Some system settings for convenience

# Create a permanent configuration path in your user folder
CONFIG_FILE = os.path.join(os.path.expanduser("~"), ".spyder_folder_picker.txt")

# settings
def set_plots():
    params = {
        'figure.figsize': [12, 6],
        'axes.grid': True,
        'font.size': 18,
        'lines.markersize': 10
        }
    plt.rcParams.update(params)

def set_directory(path):
    os.chdir(path)
    
def add_path(path):
    if path not in sys.path: sys.path.append(path)
    
def checkpath(path):  #Create folder/directory if it doesn't exist.
    isExist = os.path.exists(path)
    if isExist == False:
       os.makedirs(path)

# loading data
def filenames(datatype, title = '', keyword = ''):
    """ Search files in directory.
    Options:
        - title = '': find all files of the same format, eg .csv
        - title = 'some title': find all files of the same format and starting title
        - keyword: to narrow down selection further
    
    Hence a general file name: title_maybe-some-extra-details_keyword.datatype
    
    Return: list of file names"""
    if title == '':
        if keyword == '':
            names = glob.glob("*."+datatype)
        else:
            names = glob.glob("*_"+keyword+"."+datatype)
    else:
        if keyword == '':
            names = glob.glob(title+"_*."+datatype)
        else:
            names = glob.glob(title+"_"+keyword+"."+datatype)
    return names

def loadCSV(name, path='', skiprow = 0, deli = '', dtype=float):
    data = pd.read_csv(name, skiprows = skiprow, delimiter=deli, low_memory=False,\
                       dtype=dtype)
    return data

def get_file_(Filetypes):
    """
    This code allows importing the file with a GUI window and also fetches relevant.
    """
    root = Tk()
    root.withdraw()
    root.call('wm', 'attributes', '.', '-topmost', True)
    file_paths = askopenfilenames(filetypes=Filetypes)
    # file_names = file_path.split("/")[-1]
    # file_dates = " ".join(file_path.split("/")[-3:-1])
    #file_scan_type = file_name.split("_")[1]
    #file_beams_used = file_name.split("_")[2]

    scans = []
    for file_path in file_paths:
        file_name = file_path.split("/")[-1]
        file_date = " ".join(file_path.split("/")[-3:-1])
        scans.append([file_path, file_name, file_date])

    return scans

def get_last_directory():
    """Reads the saved path from the hidden file, falls back to Home folder if missing."""
    if os.path.exists(CONFIG_FILE):
        with open(CONFIG_FILE, "r", encoding="utf-8") as f:
            saved_path = f.read().strip()
            if os.path.exists(saved_path):  # Verify the folder still exists
                return saved_path
    return os.path.expanduser("~")  # Fallback default

def save_last_directory(path):
    """Saves the selected path permanently to the configuration file."""
    with open(CONFIG_FILE, "w", encoding="utf-8") as f:
        f.write(path)


def select_folder():
    # 1. Fetch the permanently stored starting location
    start_dir = get_last_directory()

    root = tk.Tk()
    root.withdraw()
    root.attributes("-topmost", True)

    # 2. Open the dialogue box pointing to our saved path
    folder_path = tk.filedialog.askdirectory(
        title="Select a Project Folder", initialdir=start_dir
    )

    root.destroy()  # Clean up memory

    if folder_path:
        # 3. Permanently write the new path to disk
        save_last_directory(folder_path)
        print(f"Directory Saved & Opened: {folder_path}")
        return folder_path
    else:
        print("Selection cancelled. Using last saved location next time.")
        return None

def MovingAverage(window_size, data):
    Data_series = pd.Series(data)
   
    # Get the window of series
    # of observations of specified window size
    windows = Data_series.rolling(window_size)
 
    # Create a series of moving
    # averages of each window
    moving_averages = windows.mean()

    # Convert pandas series back to list
    moving_averages_list = moving_averages.tolist()

    # Remove null entries from the list
    final_list = moving_averages_list[window_size - 1:]
    
    return np.array(final_list)

def flattenList(xss):
    return [x for xs in xss for x in xs]

def flattenAnyList(list_of_lists):
    return [*chain(*list_of_lists)]

def VelocityfromFshift(dF, F0, angle):
    """
    Parameters
    ----------
    dF : TYPE
        Relative frequency in MHz.
    F0 : TYPE
        Reference frequency in THz.
    angle : TYPE
        Angle of detection in degrees.

    Returns
    -------
    v : TYPE
        DESCRIPTION.
    dv : TYPE
        DESCRIPTION.

    """
    angle_rad = angle * np.pi / 180
    v = -dF*1e6 * 299792458 / (F0*1e12 * np.cos(angle_rad))
    #dv = dFerr/dF * v
    return v

# Functions to fit
def Line(x, a, b):
    return a*x + b

def FitLine(Figure, xdata, ydata, p0, xstep=0.01, display=True, Toprint=True):
    fit, cov = curve_fit(Line, xdata, ydata, p0=p0)
    
    newFig = copy.deepcopy(plt.figure(Figure)) 
    #This ensures the original figure is not altered

    xspan = np.arange(np.min(xdata), np.max(xdata), step=xstep)
    plt.plot(xspan, Line(xspan, *fit))
    if display:
        plt.show()
    plt.close()
    
    err = np.sqrt(np.diag(cov))
    fit_results = {"Variables":["slope", "shift"],
                   "best fit":fit, "error":err}
    
    if Toprint:
        print(fit_results)
    
    return newFig, fit_results

def Gaussian(x, mean, std, amp, shift):
    fac = -(x - mean)**2 / (2*std**2)
    return amp * np.exp(fac) + shift

def FitGaussian(Figure, xdata, ydata, p0, xstep=0.01, \
                plot=True, display=True, Toprint=True,\
                    xlabel='', ylabel='', title='',\
    bounds=([-np.inf, -np.inf, -np.inf, -np.inf], [np.inf, np.inf, np.inf, np.inf])):
    fit, cov = curve_fit(Gaussian, xdata, ydata, p0=p0, bounds=bounds)
    
    if plot:
        if Figure == 0:
            newFig = plt.figure()
            plt.plot(xdata, ydata, '.')
            plt.xlabel(xlabel)
            plt.ylabel(ylabel)
            plt.title(title)
        else:
            newFig = copy.deepcopy(plt.figure(Figure)) 
            #This ensures the original figure is not altered
    
        xspan = np.arange(np.min(xdata), np.max(xdata), step=xstep)
        plt.plot(xspan, Gaussian(xspan, *fit))
        if display:
            plt.show()
        plt.close()
        
    else:
        newFig = 0
    
    err = np.sqrt(np.diag(cov))
    fit_results = {"Variables":["mean", "std", "amplitude", "shift"],
                   "best fit":fit, "error":err}
    
    if Toprint:
        print(fit_results)
    
    return newFig, fit_results

def Gaussian_FWHM(w, A, w0, dw, shift):
    """ Implements a generalised gaussian profile sampled on w. """
    return A * np. exp(- 4 * np.log(2) * (w - w0)**2 / dw**2)+shift #A*(2 * np.sqrt(np.log(2) / np.pi) / dw )

def double_Gaussian_FWHM(w, A1, w01, dw1, A2, w02, dw2, shift):
    return Gaussian_FWHM(w, A1, w01, dw1, shift) + Gaussian_FWHM(w, A2, w02, dw2, shift)

def Gaussian_FWHM_norm(w, *args):
    """ Implements a unit-area gaussian profile sampled on w. """
    w0, dw = args # centre frequency, FWHM
    return (2 * np.sqrt(np.log(2) / np.pi) / dw ) * np. exp(- 4 * np.log(2) * (w - w0)**2 / dw**2)

def Gaussian_norm(x, mean, std):
    fac = -(x - mean)**2 / (2*std**2)
    amp = 1 / (std * np.sqrt(2 * np.pi))
    return amp * np.exp(fac)

def SkewedGaussian(x, mean, std, amp, shift, gamma):
    skewness = 1 + sp.special.erf(gamma * (x-mean) / (np.sqrt(2) * std))
    exponent = -(x-mean)**2 / (2 * std**2)
    return amp * skewness * np.exp(exponent) + shift

def lorentzian_norm(w, w0, dw):    #Y3 lasers coding sheet 1
    """ Implement a unit-area lorentzian function sampled on w. """
    # centre frequency, FWHM
    return (2 / np.pi / dw) * dw**2 / (dw**2 + 4 * (w - w0)**2) 

def lorentzian(w, A, w0, dw, shift):    #Y3 lasers coding sheet 1
    """ Implement a unit-area lorentzian function sampled on w. """
     # area, centre frequency, FWHM
    return A * (2 / np.pi / dw) * dw**2 / (dw**2 + 4 * (w - w0)**2) + shift

def exp_decay(x, A, B, C):
    return A * np.exp(-x / B)  + C

def exp_decay2(x, B, C):
    return np.exp(-x / B)  + C

def exp_decay_FixAmpBkgto1(x, A, B):
    C = 1-A
    return A * np.exp(-x / B)  + C

def Fitexp_decay(Figure, xdata, ydata, p0, xstep=0.01, \
                plot=True, display=True, Toprint=True,\
                    xlabel='', ylabel='', title='',\
                plotErr=False, errY=[]):
    fit, cov = curve_fit(exp_decay, xdata, ydata, p0=p0)
    
    if plot:
        if Figure == 0:
            newFig = plt.figure()
            plt.plot(xdata, ydata, '.')
            if plotErr:
                plt.fill_between(xdata, ydata+errY, ydata-errY, alpha=0.3)
            plt.xlabel(xlabel)
            plt.ylabel(ylabel)
            plt.title(title)
        else:
            newFig = copy.deepcopy(plt.figure(Figure)) 
            #This ensures the original figure is not altered
    
        xspan = np.arange(np.min(xdata), np.max(xdata), step=xstep)
        plt.plot(xspan, exp_decay(xspan, *fit))
        if display:
            plt.show()
        plt.close()
        
    else:
        newFig = 0
    
    err = np.sqrt(np.diag(cov))
    fit_results = {"Variables":["amplitude", "lifetime", "shift"],
                   "best fit":fit, "error":err}
    
    if Toprint:
        print(fit_results)
    
    return newFig, fit_results

def inverse_exp_decay(x, A, B, C):
    return A * (1 - np.exp(-x / B))  + C

def Fitinverse_exp_decay(Figure, xdata, ydata, p0, xstep=0.01, \
                plot=True, display=True, Toprint=True,\
                    xlabel='', ylabel='', title='',\
                plotErr=False, errY=[]):
    fit, cov = curve_fit(inverse_exp_decay, xdata, ydata, p0=p0)
    
    if plot:
        if Figure == 0:
            newFig = plt.figure()
            plt.plot(xdata, ydata, '.')
            if plotErr:
                plt.fill_between(xdata, ydata+errY, ydata-errY, alpha=0.3)
            plt.xlabel(xlabel)
            plt.ylabel(ylabel)
            plt.title(title)
        else:
            newFig = copy.deepcopy(plt.figure(Figure)) 
            #This ensures the original figure is not altered
    
        xspan = np.arange(np.min(xdata), np.max(xdata), step=xstep)
        plt.plot(xspan, inverse_exp_decay(xspan, *fit))
        if display:
            plt.show()
        plt.close()
        
    else:
        newFig = 0
    
    err = np.sqrt(np.diag(cov))
    fit_results = {"Variables":["amplitude", "lifetime", "shift"],
                   "best fit":fit, "error":err}
    
    if Toprint:
        print(fit_results)
    
    return newFig, fit_results