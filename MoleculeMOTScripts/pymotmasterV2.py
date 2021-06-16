from __future__ import print_function
import clr
import sys
from System.IO import Path
import os

sys.path.append(Path.GetFullPath("C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\"))
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MOTMaster\\bin\\CaF\\MOTMaster.exe")
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\TransferCavityLock2012\\bin\\CaF\\TransferCavityLock.exe")

sys.path.append(Path.GetFullPath("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\"))
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\MoleculeMOTHardwareControl.exe")
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\DAQ.dll")
clr.AddReference("C:\\ControlPrograms\\EDMSuite\\MoleculeMOTHardwareControl\\bin\\CaF\\SharedCode.dll")

sys.path.append(Path.GetFullPath("C:\\Users\\cafmot\\Documents\\Visual Studio 2013\\Projects\\WavePlateControl\\WavePlateControl\\bin\\Debug\\"))
clr.AddReference("C:\\Users\\cafmot\\Documents\\Visual Studio 2013\\Projects\\WavePlateControl\\WavePlateControl\\bin\\Debug\\WavePlateControl.exe")


# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")

# create connections to the control programs
import System
import MOTMaster
import MoleculeMOTHardwareControl
import TransferCavityLock2012
import WavePlateControl

hc = System.Activator.GetObject(MoleculeMOTHardwareControl.Controller, 'tcp://localhost:1172/controller.rem')
mm = System.Activator.GetObject(MOTMaster.Controller, 'tcp://localhost:1187/controller.rem')
tcl = System.Activator.GetObject(TransferCavityLock2012.Controller, 'tcp://localhost:1190/controller.rem')
wpmotor = System.Activator.GetObject(WavePlateControl.Controller, 'tcp://localhost:1192/WPmotor.rem')

from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *
from System.Collections.Generic import Dictionary
import time
import itertools
from random import shuffle

# specific EDMSuite stuff
from DAQ.Environment import *
from DAQ import *
from MOTMaster import *

# Python specific libraries
import time
import collections
from PIL import Image as PILImage
from tqdm import tqdm
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
import gc
import random

sns.set()


def tcl_auto_tuner(
			script_name="AMOTBasic",
			imgs_dirpath='C:\Users\cafmot\Documents\Temp Camera Images',
            field_parameter="MOTCoilsCurrentValue",
            bg_field_value=0.0,
            mot_field_value=0.65,
        	v00_scan=False,
			bx_scan=False,
			v10_scan=False,
			v21_scan=False,
			v32_scan=False,
			bxBeast_scan=False,
        	v00_range=[],
			v10_range=[],
			v21_range=[],
			v32_range=[],
			bx_range=[],
			bxBeast_range=[],
            display=True):
    """
    @param
	script_name : String, Name of the script in mot_master to run,
	              i.e. "AMOTBasic"
	imgs_dirpath : String, dirctory in which the images are being saved,
	               i.e. "C:\Users\cafmot\Documents\Temp Camera Images"
    field_parameter: String, name of the parameter that controls the magnetic field
    bg_field_value : Float, voltage value that keeps the magnetic field switched off
                     for background image
    mot_field_value : Float, voltage value that keeps the magnetic field on to make a mot
	*_scan : Boolean, True for engaging the tuner to scan and set the set_point automatically,
	         or False to left unaltered
	*_range : list/range of values to check for the set_point
	@out
	domain: Dictionary, with laser names as keys, and each element being another 
	        Dictionary with information 
	        range : List, range of values to scan
	        laser: String, name of the laser
	        cavity, String, name of the cavity it is locked
	        scan: Boolean, laser should be scanned or not
	        set_point: float, optimized set point
	        n: Numpy array, number of molecules obtained during the optimization process for plotting
    """
    domain = collections.OrderedDict()
    domain["v00"] = {"range": v00_range, "cavity": "Hamish",
                    "laser": "v00Lock", "scan": v00_scan,
                    "set_point": 0.0, "n": [], "ne": []}
    domain["bX"] = {"range": bx_range, "cavity": "Hamish",
                    "laser": "bXLock", "scan": bx_scan,
                    "set_point": 0.0, "n": [], "ne": []}
    domain["v10"] = {"range": v10_range, "cavity": "Hamish",
                    "laser": "v10Lock", "scan": v10_scan,
                    "set_point": 0.0, "n": [], "ne": []}
    domain["v21"] = {"range": v21_range, "cavity": "Carlos",
                    "laser": "v21Lock", "scan": v21_scan,
                    "set_point": 0.0, "n": [], "ne": []}
    domain["v32"] = {"range": v32_range, "cavity": "Carlos",
                    "laser": "v32Lock", "scan": v32_scan,
                    "set_point": 0.0, "n": [], "ne": []}
    domain["bXBeast"] = {"range": bxBeast_range, "cavity": "Carlos",
                        "laser": "bXBeastLock", "scan": bxBeast_scan,
                        "set_point": 0.0, "n": [], "ne": []}
    
    for laser in domain:
        print("currently scanning {}".format(laser))
        osp, n, ne = _tcl_scan_laser(script_name, imgs_dirpath, 
                                     field_parameter, bg_field_value, mot_field_value,
                                     domain[laser])
        domain[laser]["set_point"] = osp
        domain[laser]["n"] = n
        domain[laser]["ne"] = ne
    
    if display:
        fig, ax = plt.subplots(2, 3, figsize=(15, 8), sharey=True)
        fig.subplots_adjust(left=0.125,
                            bottom=0.1, 
                            right=0.9, 
                            top=0.9, 
                            wspace=0.05, 
                            hspace=0.35)
        named_ax = {"v00": ax[0, 0], "bX": ax[0, 1], "v10": ax[0, 2],
                    "v21": ax[1, 0], "v32": ax[1, 1], "bXBeast": ax[1, 2]}
        named_ax["v00"].set_ylabel("Norm. Number")
        named_ax["v21"].set_ylabel("Norm. Number")
        for laser, prop in domain.items():
            if prop["scan"]:
                named_ax[laser].errorbar(prop["range"], prop["n"] / np.max(prop["n"]),
                                         yerr=prop["ne"] / np.max(prop["n"]), fmt='ok')
                named_ax[laser].arrow(prop["set_point"], 1.15, 0, -0.1,
                                      head_width=0.002, head_length=0.03, width=0.0005,
                                      fc='r', ec='r')
                named_ax[laser].set_title("{} set @ {}".format(laser, prop["set_point"]))
            else:
                named_ax[laser].set_title("{} not scanned".format(laser))
            named_ax[laser].set_xlabel("TCL Volatge [V]")
            named_ax[laser].set_ylim((0,1.2))
    return domain


def _tcl_scan_laser(
            script_name,
            imgs_dirpath,
            field_parameter,
            bg_field_value,
            mot_field_value,
            args):
    """
    Internal call function for tcl_auto_tuner for scanning the laser set point
    """
    dictionary = Dictionary[String, Object]()
    mm.SetScriptPath('C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
    set_point = args["set_point"]
    n, ne = np.array([]), np.array([])
    if args["scan"]:
        set_point = args["range"][0]
        _tcl_set_point_ramp(set_point, args)
        dictionary[field_parameter] = bg_field_value
        images_bg = _motmaster_single_run_images(dictionary, imgs_dirpath)
        for value in tqdm(args["range"]):
            tcl.SetLaserSetpoint(args["cavity"], args["laser"], value)
            dictionary[field_parameter] = mot_field_value
            images_mot = _motmaster_single_run_images(dictionary, imgs_dirpath)
            images_mot -= images_bg
            _n = np.sum(images_mot, axis=(1, 2))
            n = np.append(n, np.mean(_n))
            ne = np.append(ne, np.std(_n)/np.sqrt(len(_n)))
        set_point = args["range"][np.argmax(n)]
        _tcl_set_point_ramp(set_point, args)
    return set_point, n, ne


def _tcl_set_point_ramp(set_point, args):
    """
    Internal call function for ramping the set point of tcl to a desired
    new value slowly, rather than jumping
    """
    current_set_point = tcl.GetLaserSetpoint(args["cavity"], args["laser"])
    while current_set_point > set_point:
        current_set_point -= 0.001
        tcl.SetLaserSetpoint(args["cavity"], args["laser"], current_set_point)
        time.sleep(0.05)
    while current_set_point < set_point:
        current_set_point += 0.001
        tcl.SetLaserSetpoint(args["cavity"], args["laser"], current_set_point)
        time.sleep(0.05)
    return None


def _motmaster_single_run_images(dictionary, imgs_dirpath):
    """
    Internal call function for running the motmaster with a 
    specific set of values supplied by the .net dictionary
    and then returns an array of images from the imgs_dirpath
    """
    for filename in os.listdir(imgs_dirpath):
        if '.tif' in filename:
            os.remove(os.path.join(imgs_dirpath, filename))
    time.sleep(0.01)
    mm.Go(dictionary)
    time.sleep(0.1)
    images, filepaths = [], []
    for filename in os.listdir(imgs_dirpath):
        if '.tif' in filename:
            filepaths.append(os.path.join(imgs_dirpath, filename))
    for filepath in filepaths:
        with PILImage.open(filepath, 'r') as imagefile:
		    image = np.array(_get_image_from_file(filepath), dtype=float)
        images.append(image)
    gc.collect()
    for filepath in filepaths:
        os.remove(filepath)
    return np.array(images)


def _get_image_from_file(filepath):
    """
    Internal call wrapper function to force python to release the 
    image file. 
    #bugfix to python not realeasing file resource for deleting
    """
    with PILImage.open(filepath, 'r') as imagefile:
		image = np.array(imagefile, dtype=float)
    return image


def tcl_get_laser_parameters():
    """
    """
    lasers = {"v00Lock": "Hamish",
              "bXLock": "Hamish",
              "v10Lock": "Hamish",
              "v21Lock": "Carlos",
              "v32Lock": "Carlos",
              "bXBeastLock": "Carlos"}
    for laser, cavity in lasers.items():
        voltage = tcl.GetLaserVoltage(cavity, laser)
        set_point = tcl.GetLaserSetpoint(cavity, laser)
        print("{0} laser is set @ {1:.3f} V with laser voltage {2:.3f} V".format(laser, set_point, voltage))
    return None


def mm_scan_single_parameter(script_name, parameter_name, values, randomize=True):
    """
    A single parameter can be scanned with multiple values
    @param
    script_name : String, Name of the script in mot_master to run,
	              i.e. "AMOTBasic"
    parameter_name : String, name of the parameter in the script that will be 
                     scanned, i.e. "Frame0Trigger"
    values : List of appropriate typedef, series of values of the parameter to run with.
    randomize : Boolean, If True, it will randomly order the values to run
                the experiment with
    """
    dictionary = Dictionary[String, Object]()
    mm.SetScriptPath('C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
    random.shuffle(values)
    for value in tqdm(values):
        dictionary[parameter_name] = value
        mm.Go(dictionary)
        time.sleep(0.05)
    print("finished with execution order: {}".format(values))
    return None


def mm_scan_multi_parameter(script_name, parameter_dict, randomize=True):
    """
    Multiple parameters can be scanned with each set of parameters drawn from
    a list of set of parameters
    @param
    script_name : String, Name of the script in mot_master to run,
	              i.e. "AMOTBasic"
    parameter_dict : Dictionary, containing (M) parameters as key and 
                     list of (N_0, N_1, ..., N_M) values to scan
                     will generate (N_0xN_1x...xN_M) individual experimental runs.
                     i.e. {"SlowingChirpEndValue" : [5.6, 3.2, 1.9],
                           "SlowingChirpStartTime": [180, 250, 360],
                           "SlowingChirpDuration": [1160, 1200]}
                     wiil generate a set of runs with 
                     ("SlowingChirpEndValue", "SlowingChirpStartTime", "SlowingChirpDuration") ==>
                     [(5.6, 180, 1160), (5.6, 250, 1160), (5.6, 360, 1160),
                      (3.2, 180, 1160), (3.2, 250, 1160), (3.2, 360, 1160),
                      (1.9, 180, 1160), (1.9, 250, 1160), (1.9, 360, 1160),
                      (5.6, 180, 1200), (5.6, 250, 1200), (5.6, 360, 1200),
                      (3.2, 180, 1200), (3.2, 250, 1200), (3.2, 360, 1200),
                      (1.9, 180, 1200), (1.9, 250, 1200), (1.9, 360, 1200)]
    randomize : Boolean, If True, it will randomly order the values to run
                the experiment with
    """
    dictionary = Dictionary[String, Object]()
    mm.SetScriptPath('C:\\ControlPrograms\\EDMSuite\\MoleculeMOTMasterScripts\\' + script_name + '.cs')
    parameter_names = list(parameter_dict.keys())
    value_tuples = []
    _dict = {}
    random.shuffle(value_tuples)
    for value_tuple in tqdm(value_tuples):
        for parameter_name, value in zip(parameter_names, value_tuple):
            dictionary[parameter_name] = value
        mm.Go()
        time.sleep(0.05)
    print("finished with execution order: {}".format(values))
    return NotImplementedError
