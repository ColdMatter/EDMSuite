import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
sns.set_style('darkgrid')


def display_iterations(cache):
    _, ax = plt.subplots(2, 1, figsize=(8, 8), sharex=True)
    iterations = np.arange(0, cache.iteration+1)
    ax[0].semilogy(iterations,
                   cache.output_list, '-k')
    lines = ax[1].plot(iterations, cache.parameter_values_list, '-')
    ax[1].legend(lines,
                ['{j}={k:.3f}'.format(j=j, k=k) 
                for j, k in zip(cache.parameter_names, cache.result.x)])
    ax[1].set_xlabel('iterations')
    ax[1].set_ylabel('evolution of parameters')
    ax[0].set_ylabel('min[f(w,..)]')
    plt.show()
    return cache