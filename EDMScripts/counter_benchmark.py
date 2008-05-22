import time

def run_script():
	hc.SetLeakageMonitorMeasurementTime( 0.001 )
	hc.ReconfigureIMonitors()
	t1 = time.clock()
	for i in range(0,1000):
		hc.UpdateIMonitor()
	dt = (time.clock() - t1)
	print "Time for 10^3 measurements"
	print dt
