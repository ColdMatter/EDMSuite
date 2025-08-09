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
import pandas as pd
import matplotlib.pyplot as plt
import sys
import os
import glob
import copy

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

# Functions to fit
def Line(x, a, b):
    return a*x + b

def FitLine(Figure, xdata, ydata, p0, xstep=0.01, display=True):
    fit, cov = curve_fit(Line, xdata, ydata, p0=p0)
    
    newFig = copy.deepcopy(plt.figure(Figure)) 
    #This ensures the original figure is not altered

    xspan = np.arange(np.min(xdata), np.max(xdata), step=xstep)
    plt.plot(xspan, Line(xspan, *fit))
    if display:
        plt.show()
    
    err = np.sqrt(np.diag(cov))
    fit_results = {"Variables":["slope", "shift"],
                   "best fit":fit, "error":err}
    
    print(fit_results)
    
    return newFig, fit_results

def Gaussian(x, mean, std, amp, shift):
    fac = -(x - mean)**2 / (2*std**2)
    return amp * np.exp(fac) + shift

def FitGaussian(Figure, xdata, ydata, p0, xstep=0.01, display=True):
    fit, cov = curve_fit(Gaussian, xdata, ydata, p0=p0)

    newFig = copy.deepcopy(plt.figure(Figure)) 
    #This ensures the original figure is not altered

    xspan = np.arange(np.min(xdata), np.max(xdata), step=xstep)
    plt.plot(xspan, Gaussian(xspan, *fit))
    if display:
        plt.show()
    
    err = np.sqrt(np.diag(cov))
    fit_results = {"Variables":["mean", "std", "amplitude", "shift"],
                   "best fit":fit, "error":err}
    
    print(fit_results)
    
    return newFig, fit_results

def Gaussian_FWHM(w, A, w0, dw, shift):
    """ Implements a generalised gaussian profile sampled on w. """
    return A*(2 * np.sqrt(np.log(2) / np.pi) / dw ) * np. exp(- 4 * np.log(2) * (w - w0)**2 / dw**2)+shift

def Gaussian_FWHM_norm(w, *args):
    """ Implements a unit-area gaussian profile sampled on w. """
    w0, dw = args # centre frequency, FWHM
    return (2 * np.sqrt(np.log(2) / np.pi) / dw ) * np. exp(- 4 * np.log(2) * (w - w0)**2 / dw**2)

def Gaussian_norm(x, mean, std):
    fac = -(x - mean)**2 / (2*std**2)
    amp = 1 / (std * np.sqrt(2 * np.pi))
    return amp * np.exp(fac)

def lorentzian_norm(w, w0, dw):    #Y3 lasers coding sheet 1
    """ Implement a unit-area lorentzian function sampled on w. """
    # centre frequency, FWHM
    return (2 / np.pi / dw) * dw**2 / (dw**2 + 4 * (w - w0)**2) 

def lorentzian(w, A, w0, dw, shift):    #Y3 lasers coding sheet 1
    """ Implement a unit-area lorentzian function sampled on w. """
     # area, centre frequency, FWHM
    return A * (2 / np.pi / dw) * dw**2 / (dw**2 + 4 * (w - w0)**2) + shift

def exp_decay(x, A, B, C):
    return A * (np.exp(x * B) - 1) + C