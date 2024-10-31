from System import *
from Data.Scans import *
from DAQ.Environment import *
from DAQ.Analyze import *
import time
from uedmfuncs import prompt#, SelectProfile
import numpy as np 
from scipy.optimize import curve_fit 
import matplotlib.pyplot as mpl
from probeScanTest import getFile

# Let's create a function to model and create data 
def func(x, a, x0, sigma, c): 
    return (a*np.exp(-(x-x0)**2/(2*sigma**2))+c) 

def CSharpToPython2Darray(input_array, array_depth=(4, 4)):
    """Convert a C# Array to a N-List."""
    a = []
    for i in range(array_depth[0]):
        for j in range(array_depth[1]):
            a[i].append(input_array.GetValue(i, j))
    return a

def CSharpToPython1Darray(input_array, array_depth=4):
    """Convert a C# Array to a N-List."""
    a = []
    for i in range(array_depth):
        a.append(input_array.GetValue(i))
    return a

scanSerializer = ScanSerializer()
#file = getFile("..\\..\\Example_scan_01.zip")
file = r"C:\\Users\UEDM\\Imperial College London\\Team ultracold - PH - Documents\\Data\\2024\\2024-07\\080724\\LOG\\probescan\\scan_01.zip"

scan = scanSerializer.DeserializeScanFromZippedXML(str(file),"average.xml")

voltage = scan.ScanParameterArray
#signal = emptyArray(voltage)

intStart=1000
intEnd=4000
bgStart=1
bgEnd=600

integral = 22.5*np.array(scan.GetTOFOnIntegralArray(0, intStart, intEnd))
bg = 22.5*np.array(scan.GetTOFOnIntegralArray(0, bgStart, bgEnd))

#print(integral)
signal = integral-bg
#print(signal)
# Generating clean data 
x = voltage
y = signal



first_try = [max(signal)-min(signal), (max(voltage)+min(voltage))/2, (max(voltage)-min(voltage))/5, min(signal)]

# Adding noise to the data 
yn = y# + 0.2 * np.random.normal(size=len(x)) 
  
# Plot out the current state of the data and model 
fig = mpl.figure() 
ax = fig.add_subplot(111) 
#ax.plot(x, y, c='k', label='Function') 
ax.scatter(x, yn)
ax.set_xlabel("TCL Setpoint / V")
ax.set_ylabel("Photons") 
  
# Executing curve_fit on noisy data 
popt, pcov = curve_fit(func, x, yn, p0=first_try) 
  
#popt returns the best fit values for parameters of the given model (func) 
print ("New setpoint is %3.3f" % popt[1]) 
  
ym = func(x, popt[0], popt[1], popt[2], popt[3]) 
ax.plot(x, ym, c='r', label='Best fit') 
ax.legend() 
mpl.show()