import numpy as np

from core import Cache
from core import scipy_diffev
from core import notification
from core import preset
from core import track_and_update_metric
from visualization import display_iterations


def diffev_function_test(w, *args):
    cache = args[0]
    cache.w = w
    residual = 0
    for x, y in zip(cache.test_x, cache.test_y):
        poly = cache.w[0] + cache.w[1]*x + cache.w[2]*x**2 + cache.w[3]*x**3
        residual += (poly - y) ** 2
    cache.output = np.sqrt(residual/len(cache.test_x))
    cache = track_and_update_metric(cache)
    return cache.output


def main():
    cache = Cache()

    # scipy differential evolution parameters
    cache.diffev_function = diffev_function_test
    cache.diffev_bounds = [(-2, 2), (-2, 2), (-2, 2), (-2, 2)]
    cache.diffev_args = ([cache])
    cache.diffev_strategy = 'best1bin'
    cache.diffev_maxiter = 1000
    cache.diffev_popsize = 15
    cache.diffev_tol = 1e-16
    cache.diffev_atol = 1e-16
    cache.diffev_mutation = (0.5, 1)
    cache.diffev_recombination = 0.9
    cache.diffev_seed = None
    cache.diffev_callback = None
    cache.diffev_disp = False
    cache.diffev_polish = True
    cache.diffev_init = 'latinhypercube'
    cache.diffev_updating = 'immediate'
    cache.diffev_workers = 1

    # fileio parameters
    cache.dirname = 'test'
    cache.filename = 'cache.pkl'

    # test parameter
    w = [-1.2, 0.4, -0.6, -0.8]
    x = np.linspace(0.1, 2.0, 100)
    y = w[0] + w[1] * x + w[2] * x ** 2 + w[3] * x ** 3
    cache.test_x = x
    cache.test_y = y
    cache.parameter_names = ['w0', 'w1', 'w2', 'w3']

    # evaluation reset parameters
    cache.iteration = 0
    cache.reset_interval = 10

    # initialization of tracking and fileio variables
    cache = preset(cache)

    # actual differential evoltion step
    cache = scipy_diffev(cache)
    
    # notification step
    cache = notification(cache)

    # fileio step
    cache.save()

    # instant visualization step
    cache = display_iterations(cache)
    return cache


if __name__ == '__main__':
    main()
