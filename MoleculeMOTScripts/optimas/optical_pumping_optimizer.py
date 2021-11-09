from __future__ import print_function
import numpy as np
import sys
import os
import re
import glob
import time
from zipfile import ZipFile
from PIL import Image
from core import Cache
from core import scipy_diffev
#from core import diffev_function_motmaster as diffev_function
from core import notification
from core import preset
from core import status_of_convergence
from core import create_parameter_list_bounds_and_init
from core import typecast_parameter_values
from core import get_cache
from core import track_and_update_metric

try:
    from motmaster_wrapper import single_param_single_shot
    from motmaster_wrapper import multi_param_single_shot
except ModuleNotFoundError:
    print('Not compatible with motmaster execution')


import py_thorlabs_ctrl.kinesis
py_thorlabs_ctrl.kinesis.init(r'C:\Program Files\Thorlabs\Kinesis')
from py_thorlabs_ctrl.kinesis.motor import TCubeDCServo


def atoi(text):
    return int(text) if text.isdigit() else text


def natural_keys(text):
    return [atoi(c) for c in re.split(r'(\d+)', text) ]


def get_images(cache):
    dst_filepath = os.path.join(cache.dirpath_timestamp, '{}.zip'.format(cache.successful_run))
    zipobj = ZipFile(dst_filepath, 'w')
    filenames = glob.glob(os.path.join(cache.dirpath, "*.tif"))
    filenames.sort(key=natural_keys)
    assert len(filenames) % 2 == 0
    images = []
    for filename in filenames:
        image = np.array(Image.open(filename), dtype=float)
        images.append(image)
        zipobj.write(filename, os.path.basename(filename))
        os.remove(filename)    
    zipobj.close()
    return np.array(images, dtype=np.float64)


def image_processor(cache):
    bg_sub_images = cache.images - cache.bgs
    n = np.sum(bg_sub_images[1::2])/np.sum(bg_sub_images[0::2])
    return 1.0-n

def get_shim_currents(theta, phi):
    split = 1.4e6                # 1.4 MHz/Gauss is rate of splitting of the lines
    Gx = 0.7e3                   # kHz/mA
    Gy = 1.14e3                  # kHz/mA
    Gz = 2.93e3                  # kHz/mA
    B = 300e-3                   # Gauss
    volt_to_mA = 100             # mA/V

    Bx0Volt = -1.35                          # Volt  
    By0Volt = -1.92                          # Volt 
    Bz0Volt = -0.22                          # Volt

    Bx0 = Bx0Volt*volt_to_mA*Gx/split        # Gauss
    By0 = By0Volt*volt_to_mA*Gy/split        # Gauss 
    Bz0 = Bz0Volt*volt_to_mA*Gz/split        # Gauss
    Bxp = B*np.sin(theta)*np.cos(phi)        # Gauss
    Byp = B*np.sin(theta)*np.sin(phi)        # Gauss
    Bzp = B*np.cos(theta)                    # Gauss

    Bx = ((Bx0 + Bxp)*split/Gx)/volt_to_mA   # Volt
    By = ((By0 + Byp)*split/Gy)/volt_to_mA   # Volt
    Bz = ((Bz0 + Bzp)*split/Gz)/volt_to_mA   # Volt
    return Bx, By, Bz


def diffev_function_motmaster(w, *args):
    cache = get_cache(args)
    cache.w = typecast_parameter_values(w, cache)

    # unpack the cache values and pre process for 
    # field and polarization variation
    # phi, angle_servo_0, angle_servo_1, costheta = cache.w
    # theta = np.arccos(costheta)
    # final_positions = [angle_servo_0, angle_servo_1]

    # unpack the cache values and pre process for 
    # polarization variation only
    angle_servo_0, angle_servo_1 = cache.w
    final_positions = [angle_servo_0, angle_servo_1]

    #Bx, By, Bz = get_shim_currents(theta, phi)
    Bx, By, Bz = -1.35, -1.92, -0.22

    # set the rotation drivers
    for servo, final_position in zip(cache.servos, final_positions):
        servo.move_absolute(final_position)

    # check if the servos reached the final positions
    for servo, final_position in zip(cache.servos, final_positions):
        current_pos = servo.get_position()
        while np.abs(current_pos - final_position) > 0.1:
            time.sleep(1)
            current_pos = servo.get_position()
    
    if (cache.iteration % cache.reset_interval) == 0:
        if not single_param_single_shot(
                cache.script_name,
                cache.coil_current_parameter,
                float(cache.coil_current_offvalue)):
            print('Error occured during mot master process in bg')
            sys.exit(0)
        cache.bgs = get_images(cache)
        cache.successful_run += 1

    if not multi_param_single_shot(
            cache.script_name,
            ["xShimLoadCurrent", "yShimLoadCurrent", "zShimLoadCurrent"],
            [float(Bx), float(By), float(Bz)]):
        print('Error occured during mot master process in evolution')
        sys.exit(0)
    cache.images = get_images(cache)
    cache.successful_run += 1
    print('No of run', cache.successful_run)
    
    cache.output = image_processor(cache)
    cache = track_and_update_metric(cache)
    return cache.output


def main():
    cache = Cache()

    # parameter names and their corresponding bounds
    parameter_names_and_bounds = {}
    #parameter_names_and_bounds["costheta"] = [(-1, 1), 0.25, 'double']
    #parameter_names_and_bounds["phi"] = [(0, 6.28), 3.14, 'double']
    parameter_names_and_bounds["angle_servo_0"] = [(0, 90), 0, 'int']
    parameter_names_and_bounds["angle_servo_1"] = [(0, 90), 0, 'int']

    cache.servos = [TCubeDCServo(83817788), TCubeDCServo(83825463)]
    for servo in cache.servos:
        servo.create()
        servo.enable()
        servo.set_velocity(max_velocity = 25, acceleration = 25)
    
    # attach dictionary to cache
    cache.parameter_names_and_bounds = parameter_names_and_bounds

    # MOTMaster execution parameters
    cache.script_name = "OptPumpDiffEvol"
    cache.coil_current_parameter = "MOTBField"
    cache.coil_current_offvalue = 0.0

    # scipy differential evolution parameters
    cache.diffev_popsize = 15
    cache.diffev_tol = 1e-3
    cache.diffev_atol = 1e-3
    cache.diffev_mutation = 0.9
    cache.diffev_recombination = 0.5
    cache.diffev_seed = None
    cache.diffev_callback = None
    cache.diffev_disp = False
    cache.diffev_polish = True
    cache.diffev_function = diffev_function_motmaster
    cache.diffev_args = ([cache])
    cache.diffev_strategy = 'best1bin'
    cache.diffev_maxiter = 10
    
    # fileio parameters
    cache.dirpath = "C:\Users\cafmot\Desktop\slowing_chirp_optimization"
    cache.filename = 'cache'

    # evaluation reset parameters
    cache.reset_interval = 2000
    cache.checkpoint_interval = 1

    # create parameters, bounds and initial vectors from the 
    # parameter_names_and_bounds variable
    cache = create_parameter_list_bounds_and_init(cache)

    # initialization of tracking and fileio variables
    cache = preset(cache)

    # actual differential evoltion step
    cache = scipy_diffev(cache)

    # notification step
    cache = notification(cache)
    
    # fileio step
    cache.save()

    for servo in cache.servos:
        servo.disable()
        servo.disconnect()

    return cache


if __name__ == '__main__':
    main()
