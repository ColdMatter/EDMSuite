from __future__ import print_function
import numpy as np
import errno
import os
import glob
import sys
import datetime
import time
from PIL import Image
from zipfile import ZipFile
from scipy.optimize import differential_evolution
import matplotlib.pyplot as plt
import seaborn as sns
sns.set_style('darkgrid')

try:
    import cPickle as pickle
except ModuleNotFoundError:
    import pickle

try:
    from motmaster_wrapper import single_param_single_shot
    from motmaster_wrapper import multi_param_single_shot
except ModuleNotFoundError:
    print('Not compatible with motmaster execution')


class Cache():
    def __init__(self):
        self.dirpath = '.'
        self.timestamp = datetime.datetime.now().strftime('%Y_%m_%d__%H_%M_%S')
        self.filename = 'cache'
        self.iteration = 0

    def __setattr__(self, key, value):
        self.__dict__[key] = value

    def save(self, use_iteration=False):
        if use_iteration:
            filepath = \
                os.path.join(self.dirpath,
                            self.timestamp,
                            '{}_{}.pkl'.format(self.filename, self.iteration))
        else:
            filepath = \
                os.path.join(self.dirpath,
                            self.timestamp,
                            '{}.pkl'.format(self.filename))
        with open(filepath, 'wb') as f:
            pickle.dump(self, f)

    def load(self):
        filepath = \
            os.path.join(self.dirpath,
                         self.timestamp,
                         self.filename)
        with open(filepath, 'rb') as f:
            data = pickle.load(f)
        self.__dict__.update(data.__dict__)


def create_parameter_list_bounds_and_init(cache):
    cache.parameter_names = []
    cache.diffev_bounds = []
    cache.diffev_bounds_type = []
    randpop_init = np.zeros((cache.diffev_popsize,1))
    for parameter_name, listitem in cache.parameter_names_and_bounds.items():
        cache.parameter_names.append(parameter_name)
        cache.diffev_bounds.append(listitem[0])
        cache.diffev_bounds_type.append(listitem[2])
        randpop = np.random.uniform(listitem[0][0], listitem[0][1], cache.diffev_popsize)
        randpop[0] = listitem[1]
        randpop_init = np.append(randpop_init, np.reshape(randpop,(cache.diffev_popsize,1)), axis=1)
    cache.diffev_init = randpop_init[:, 1:]
    return cache


def preset(cache):
    cache.dirpath_timestamp = os.path.join(cache.dirpath, cache.timestamp)
    try:
        os.makedirs(cache.dirpath_timestamp)
    except OSError as e:
        if e.errno != errno.EEXIST:
            raise
    cache.parameter_values_list = np.zeros((1, len(cache.parameter_names)))
    cache.output_list = np.array([0])
    cache.iteration = 0
    cache.successful_run = 0
    return cache


def scipy_diffev(cache):
    cache.result = differential_evolution(
        cache.diffev_function,
        bounds=cache.diffev_bounds,
        args=cache.diffev_args,
        strategy=cache.diffev_strategy,
        maxiter=cache.diffev_maxiter,
        popsize=cache.diffev_popsize,
        tol=cache.diffev_tol,
        mutation=cache.diffev_mutation,
        recombination=cache.diffev_recombination,
        seed=cache.diffev_seed,
        callback=cache.diffev_callback,
        disp=cache.diffev_disp,
        polish=cache.diffev_polish,
        init=cache.diffev_init,
        atol=cache.diffev_atol,
        updating='immediate')
    return cache


def image_processor(cache):
    bg_sub_n_image = cache.n_image - cache.bg_image
    bg_sub_n0_image = cache.n0_image - cache.bg_image
    n = np.sum(bg_sub_n_image)
    n0 = np.sum(bg_sub_n0_image)
    return (1.0-(n/n0))


def track_and_update_metric(cache):
    cache.parameter_values_list = np.append(cache.parameter_values_list,
                                            np.reshape(cache.w,
                                                       (1, len(cache.parameter_names))),
                                            axis=0)
    cache.output_list = np.append(cache.output_list,
                                  cache.output)
    cache.iteration += 1
    if (cache.iteration % cache.checkpoint_interval) == 0:
        cache.save(use_iteration=True)
        cache = status_of_convergence(cache)
    return cache


def get_images(cache):
    images = []
    dst_filepath = os.path.join(cache.dirpath_timestamp, '{}.zip'.format(cache.successful_run))
    zipobj = ZipFile(dst_filepath, 'w')
    for filename in os.listdir(cache.dirpath):
        if '.tif' in filename:
            src_filepath = os.path.join(cache.dirpath, filename)
            image = np.array(Image.open(src_filepath), dtype=float)
            images.append(image)
            zipobj.write(src_filepath, os.path.basename(src_filepath))
            os.remove(src_filepath)
    zipobj.close()
    return np.array(images)


def get_cache(args):
    if type(args[0]) in [list, tuple]:
        return args[0][0]
    else:
        return args[0]


def typecast_parameter_values(w, cache):
    w_typed = []
    for value, typecast in zip(w, cache.diffev_bounds_type):
        if typecast is 'int':
            w_typed.append(int(value))
        if typecast is 'double':
            w_typed.append(float(value))
    return w_typed


def diffev_function_motmaster(w, *args):
    cache = get_cache(args)
    cache.w = typecast_parameter_values(w, cache)

    if (cache.iteration % cache.reset_interval) == 0:
        if not single_param_single_shot(
                cache.standard_script_name,
                cache.coil_current_parameter,
                float(cache.coil_current_offvalue)):
            print('Error occured during mot master process in bg')
            sys.exit(0)
        cache.bg_image = np.mean(get_images(cache), axis=0)
        cache.successful_run += 1

        if not single_param_single_shot(
                cache.standard_script_name,
                cache.coil_current_parameter,
                float(cache.coil_current_onvalue)):
            print('Error occured during mot master process in n0')
            sys.exit(0)
        cache.n0_image = np.mean(get_images(cache), axis=0)
        cache.successful_run += 1

    if not multi_param_single_shot(
            cache.diffev_script_name,
            cache.parameter_names,
            cache.w):
        print('Error occured during mot master process in evolution')
        sys.exit(0)
    cache.n_image = np.mean(get_images(cache), axis=0)
    cache.successful_run += 1
    
    cache.output = image_processor(cache)
    cache = track_and_update_metric(cache)
    return cache.output


def notification(cache):
    print('Optimized result: \n', cache.result)
    return cache


def status_of_convergence(cache):
    if len(cache.output_list) > 1:
        n = len(cache.parameter_names) + 1
        _, ax = plt.subplots(n, 1, figsize=(10, 25), sharex=True)
        iterations = np.arange(0, cache.iteration+1)
        min_val_loc = np.argmin(cache.output_list[1:])
        for i in range(n):
            if i == 0:
                ax[i].plot(iterations[1:],
                        cache.output_list[1:], '-ok',
                        label='output@{0:.2f}'.format(cache.output_list[min_val_loc]))
            else:
                ax[i].plot(iterations[1:],
                        cache.parameter_values_list[1:, i-1], '-o',
                        label='{0}@{1:.3f}'.format(
                            cache.parameter_names[i-1],
                            cache.parameter_values_list[:, i-1][min_val_loc]))
                ax[i].set_ylim(cache.diffev_bounds[i-1])
            ax[i].legend()
        ax[n-1].set_xlabel('iterations')
        imgpath = os.path.join(cache.dirpath_timestamp, '{}.png'.format(cache.iteration))
        plt.savefig(imgpath, bbox_inches='tight')
        plt.close()
    return cache
