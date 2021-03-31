import numpy as np
import matplotlib.pyplot as plt
import os

try:
    import cPickle as pickle
except ModuleNotFoundError:
    import pickle

from core import Cache
from core import notification
from visualization import display_iterations

'''
cache = Cache()
cache.timestamp = '2020_07_04__13_17_24'
cache.dirname = 'test'
cache.filename = 'cache.pkl'
cache.load()

cache = notification(cache)
cache = display_iterations(cache)
'''

w0 = -1.2
w1 = 0.4
w2 = 0.6
w3 = -0.8

x = np.linspace(0.1, 2.0, 100)
y = w0+w1*x+w2*x**2+w3*x**3

_, ax = plt.subplots()
ax.plot(x, y, '-ok')
plt.show()
