from System.Threading import *
import math
import datetime

def RecordBreakdown():
	count = 0
	while True:
		System.Threading.Thread.CurrentThread.Join(100)
		if (hc.LastEastCurrent < -300):
			count = count + 1
			print("Breakdown detected at " + str(datetime.datetime.now()))
			print("Count: " + str(count))
			System.Threading.Thread.CurrentThread.Join(500)
	
def run_script():
	print("Start: " + str(datetime.datetime.now()))
	RecordBreakdown()


