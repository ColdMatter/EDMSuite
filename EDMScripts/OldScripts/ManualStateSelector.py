from System import Random

rand = Random()

def randomBool():
	if rand.Next(0,2)  < 1:
		return "True"
	else:
		return "False"

def run_script():
	print("(" + randomBool() + ", " + randomBool() + ", " + randomBool() + ")" )


