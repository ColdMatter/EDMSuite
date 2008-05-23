import time

def run_script():
	hc.SetLeakageMonitorMeasurementTime( 0.005 )
	hc.ReconfigureIMonitors()
	t1 = time.clock()
	for i in range(0,100):
		hc.UpdateIMonitor()
	dt = (time.clock() - t1)
	print "Time for 10^2 measurements"
	print dt
