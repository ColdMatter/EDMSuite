import sys

def prompt(text):
	sys.stdout.write(text)
	return sys.stdin.readline().strip()
    
def frange(start, stop=None, step=None):
	start = float(start)
	if stop == None:
		stop = start + 0.0
		start = 0.0
	if step == None:
		step = 1.0

	count = 0
	while True:
		temp = float(start + count * step)
		if step > 0 and temp >= stop:
			break
		elif step < 0 and temp <= stop:
			break
		yield temp
		count += 1

def SelectProfile(profileName):
    sm.SelectProfile(profileName)

def StartPattern():
    sm.OutputPattern()

def StopPattern():
    sm.StopPatternOutput()