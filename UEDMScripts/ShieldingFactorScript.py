# This loop monitors the leakage current and saves in a csv for a 
# ramp up of the potential up to a given value

from DAQ.Environment import *
from uedmfuncs import *

def shieldingPoint(amplitude,frequency,waittime,time):

	wavGen = Environs.Hardware.Instruments["rigolWavGen"]
	wavGen.Connect()
	wavGen.Enabled = False
	wavGen.SquareWave = False
	wavGen.Amplitude = amplitude
	wavGen.Offset = 0.0
	wavGen.Frequency = (0.000001*frequency)

	System.Threading.Thread.CurrentThread.Join(waittime*1000)

	wavGen.Enabled = True

	System.Threading.Thread.CurrentThread.Join(time*1000)

	wavGen.Enabled= False

def FrequencyScan(start,stop,step,amp=1,waiting=5,holding=60):
	'''
	RampStart is how to run the leakage test file
	'''
	# Logging started
	loggingcheck=input("Have you started logging? (y/n)\n")
	if loggingcheck!="y":
		print("Start the logging first")
		return


	print("waiting for initialisation")
	System.Threading.Thread.CurrentThread.Join(1000)

	r = np.arange(float(start), float(stop+step), float(step))
	for i in r:
		print("External B fields at " + str(i) + " Hz")
		shieldingPoint(amplitude=amp,frequency=i,waittime=waiting,time=holding)
		print("waiting for " +  str(holding) + " seconds")
		System.Threading.Thread.CurrentThread.Join(waiting*100)

def AmplitudeScan(start,stop,step,freq=1,waiting=5,holding=60):
	'''
	RampStart is how to run the leakage test file
	'''
	# Logging started
	loggingcheck=input("Have you started logging? (y/n)\n")
	if loggingcheck!="y":
		print("Start the logging first")
		return


	print("waiting for initialisation")
	System.Threading.Thread.CurrentThread.Join(1000)

	r = np.arange(float(start), float(stop+step), float(step))
	for i in r:
		print("External B fields at " + str(i) + " V")
		shieldingPoint(amplitude=i,frequency=freq,waittime=waiting,time=holding)
		print("waiting for " +  str(holding) + " seconds")
		System.Threading.Thread.CurrentThread.Join(waiting*100)

def main():
	print("Shielding Factor measurements, run either an amplitude scan or a frequency scan while measuring the inputs on signal express")

if __name__=="__main__":
	main()
	pass

