# This is a pretty brittle way to get the assembly reference
sys.path.append("C:\\Program Files\\National Instruments\\MeasurementStudioVS2005\\DotNET\\Assemblies\\Current")
clr.AddReference("NationalInstruments.DAQmx.dll")

from NationalInstruments.DAQmx import *
from DAQ.Environment import *
from math import *

def readInput(t, sampleRate, numOfSamples):
	t.Timing.ConfigureSampleClock("", sampleRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, numOfSamples);
	reader = AnalogSingleChannelReader(t.Stream);
	valArray = reader.ReadMultiSample(numOfSamples);
	t.Control(TaskAction.Unreserve);
	return sum(valArray) / len(valArray)

def sniff(channel, numOfSamples, sampleRate, displayEvery):
	# set up the magnetomter input
	t = Task("SnifferInput")
	ch = Environs.Hardware.AnalogInputChannels[channel]
	ch.AddToTask(t,-10,10)
	t.Control(TaskAction.Verify)
	vals = []
	i = 0
	hc.SwitchEAndWait(False)

	while True:
		v1 = readInput(t, sampleRate, numOfSamples)
		hc.SwitchEAndWait()
		v2 = readInput(t, sampleRate, numOfSamples)
		vals.Append(v1 - v2)
		i = i + 1
		if ((i % displayEvery) == 0):
			mn = sum(vals) / len(vals)
			se = sqrt(sum((x - mn)**2 for x in vals)) / len(vals)
			print "i: " + str(i) + "\tMean: " + str(mn) + "\tS.E: " + str(se)

	t.Dispose()
	return va

def run_script():
	print """Field sniffer (tm). Measures an analog input a number of times, throws the
e-switch and repeats. Collects statistics on the difference in one e-state to
the other. The e-switch parameters are taken from EDMHardwareController. So,
if, for instance, you just want to test the relays with the HV off, you can
set most of the delays in hardware controller to zero to speed things up. You
can easily get above 1Hz this way.

usage: sniff(channel, numOfSamples, sampleRate, displayEvery)

- channel is the name of an analog input in the Hardware class (i.e.
  "magnetomter" or "miniFlux1" - case sensitive!).
- numOfSamples is the number of samples taken between each e-switch.
- sampleRate is the rate at which these samples are taken.
- displayEvery governs after how many e-switch pairs the statistics
  are updated

So, for example, sniff("magnetometer", 1000, 1000, 5) will measure for one
second at one kHz, reverse the field and repeat. It will display updated
statistics every five seconds.
	"""


