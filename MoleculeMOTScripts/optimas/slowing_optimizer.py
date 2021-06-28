import numpy as np
from core import Cache
from core import scipy_diffev
from core import diffev_function_motmaster as diffev_function
from core import notification
from core import preset
from core import status_of_convergence
from core import create_parameter_list_bounds_and_init


def main():
    cache = Cache()

    # parameter names and their corresponding bounds
    parameter_names_and_bounds = {}
    parameter_names_and_bounds["SlowingChirpStartTime"] = [(0, 800), 300, 'int']
    parameter_names_and_bounds["SlowingChirpDuration"] = [(500, 1500), 1160, 'int']
    parameter_names_and_bounds["SlowingChirpStartValue"] = [(-0.05, 0.05), 0.0, 'double']
    parameter_names_and_bounds["SlowingChirpEndValue"] = [(-1.5, -1.0), -1.25, 'double']
    parameter_names_and_bounds["slowingAOMOnStart"] = [(0, 500), 240, 'int']

    # attach dictionary to cache
    cache.parameter_names_and_bounds = parameter_names_and_bounds

    # MOTMaster execution parameters
    cache.standard_script_name = "AMOTBasic"
    cache.diffev_script_name = "TestPolynomialRamp"
    cache.coil_current_parameter = "MOTCoilsCurrentValue"
    cache.coil_current_offvalue = 0.0
    cache.coil_current_onvalue = 1.0

    # scipy differential evolution parameters
    cache.diffev_popsize = 10
    cache.diffev_tol = 1e-2
    cache.diffev_atol = 1e-2
    cache.diffev_mutation = 0.9
    cache.diffev_recombination = 0.5
    cache.diffev_seed = None
    cache.diffev_callback = None
    cache.diffev_disp = False
    cache.diffev_polish = True
    cache.diffev_function = diffev_function
    cache.diffev_args = ([cache])
    cache.diffev_strategy = 'best1bin'
    cache.diffev_maxiter = 10
    
    # fileio parameters
    cache.dirpath = "C:\Users\cafmot\Desktop\slowing_chirp_optimization"
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
