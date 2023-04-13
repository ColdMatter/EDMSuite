from __future__ import print_function
import numpy as np
import sys
import os
import re
import glob
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


def atoi(text):
    return int(text) if text.isdigit() else text

def natural_keys(text):
    return [atoi(c) for c in re.split(r'(\d+)', text) ]

def get_images(cache):
    dst_filepath = os.path.join(cache.dirpath_timestamp, '{}.zip'.format(cache.successful_run))
    zipobj = ZipFile(dst_filepath, 'w')
    filenames = glob.glob(os.path.join(cache.dirpath, "*.tif"))
    filenames.sort(key=natural_keys)
    assert len(filenames) % 3 == 0
    images = []
    for filename in filenames:
        image = np.array(Image.open(filename), dtype=float)
        images.append(image)
        zipobj.write(filename, os.path.basename(filename))
        os.remove(filename)    
    zipobj.close()
    return np.array(images)


def image_processor(cache):
    clouds = cache.n_image[0::3, :, :]
    probes = cache.n_image[1::3, :, :]
    bgs = cache.n_image[2::3, :, :]
    probes=probes-bgs
    clouds=clouds-bgs
    clouds[clouds<=0]=1.0
    od=np.log(probes/clouds)
    od[np.isnan(od)] = 0.0
    od[od == -np.inf] = 0.0
    od[od == np.inf] = 0.0
    od = np.mean(od, axis=0)
    n = np.sum(od)
    return -n


def diffev_function_motmaster(w, *args):
    cache = get_cache(args)
    cache.w = typecast_parameter_values(w, cache)

    phi, costheta = cache.w
    theta = np.arccos(costheta)

    split = 1.4e6                # 1.4 MHz/Gauss is rate of splitting of the lines
    Gx = 0.7e3                   # kHz/mA
    Gy = 1.14e3                  # kHz/mA
    Gz = 2.93e3                  # kHz/mA
    B = 400e-3                   # Gauss
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

    if not multi_param_single_shot(
            cache.diffev_script_name,
            ["xShimImagingCurrent", "yShimImagingCurrent", "zShimImagingCurrent"],
            [float(Bx), float(By), float(Bz)]):
        print('Error occured during mot master process in evolution')
        sys.exit(0)
    cache.n_image = get_images(cache)
    cache.successful_run += 1
    print('No of run', cache.successful_run)
    
    cache.output = image_processor(cache)
    cache = track_and_update_metric(cache)
    return cache.output



def main():
    cache = Cache()

    # parameter names and their corresponding bounds
    parameter_names_and_bounds = {}
    parameter_names_and_bounds["costheta"] = [(-1, 1), 0, 'double']
    parameter_names_and_bounds["phi"] = [(0, 6.28), 3.14, 'double']

    # attach dictionary to cache
    cache.parameter_names_and_bounds = parameter_names_and_bounds

    # MOTMaster execution parameters
    cache.diffev_script_name = "DualMQTN1"

    # scipy differential evolution parameters
    cache.diffev_popsize = 15
    cache.diffev_tol = 1e3
    cache.diffev_atol = 1e3
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
    cache.dirpath = "C:\Users\cafmot\Desktop\probe_optimization"
    cache.filename = 'cache'

    # evaluation reset parameters
    cache.reset_interval = 10
    cache.checkpoint_interval = 10

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

    return cache


if __name__ == '__main__':
    main()
