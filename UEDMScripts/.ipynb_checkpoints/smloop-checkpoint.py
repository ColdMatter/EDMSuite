# MapLoop - asks ScanMaster to make a series of scans with one of the pg
# parameters incremented scan to scan

from DAQ.Environment import *
from uedmfuncs import *

def megaMapLoop(plugin1, param1, start1, stop1, step1, plugin2, param2, start2, stop2, step2, numScans):
    # setup
    fileSystem = Environs.FileSystem
    file = \
        fileSystem.GetDataDirectory(\
                    fileSystem.Paths["scanMasterDataPath"])\
        + fileSystem.GenerateNextDataFileName()
    print("Saving as " + file + "_*.zip")
    print("")
    map1 = np.arange(start1,stop1,step1)
    map2 = np.arange(start2,stop2,step2)
    np.random.shuffle(map1)
    np.random.shuffle(map2)
    for i in map1:
        print(plugin1 + ":" + param1 + " -> " + str(i))
        sm.AdjustProfileParameter(plugin1, param1, str(i), False)
        for j in map2:
            print(plugin2 + ":" + param2 + " -> " + str(j))
            sm.AdjustProfileParameter(plugin2, param2, str(j), False)
            sm.AcquireAndWait(numScans)
            scanPath = file + "_" + plugin1 + ":" + param1 + "_" + str(i) + "_" + plugin2 + ":" + param2 + "_" + str(j) + ".zip"
            sm.SaveAverageData(scanPath)

def mapLoopAny(plugin, param, start, end, step, numScans):
    # setup
    fileSystem = Environs.FileSystem
    file = \
        fileSystem.GetDataDirectory(\
                    fileSystem.Paths["scanMasterDataPath"])\
        + fileSystem.GenerateNextDataFileName()
    print("Saving as " + file + "_*.zip")
    print("")
    # start looping
    r = np.arange(start, end, step)
    #shuffling to a random order
    np.random.shuffle(r)
    for i in r:
        print(plugin + ":" + param + " -> " + str(i))
        sm.AdjustProfileParameter(plugin, param, str(i), False)
        sm.AcquireAndWait(numScans)
        scanPath = file + "_" + plugin + ":" + param + "_" + str(i) + ".zip"
        sm.SaveAverageData(scanPath)

def mapLoopRepRate(start, end, step, numScans):
    # setup
    fileSystem = Environs.FileSystem
    file = \
        fileSystem.GetDataDirectory(\
                    fileSystem.Paths["scanMasterDataPath"])\
        + fileSystem.GenerateNextDataFileName()
    print("Saving as " + file + "_*.zip")
    print("")
    # start looping
    r = np.arange(start, end, step)
    #shuffling to a random order
    np.random.shuffle(r)
    for i in r:
        print("pg:flashlampPulseInterval -> " + str(int(1000000/i)))
        sm.AdjustProfileParameter("pg","flashlampPulseInterval", str(int(1000000/i)), False)
        sm.AcquireAndWait(numScans)
        scanPath = file + "_repRateHz_" + str(i) + ".zip"
        sm.SaveAverageData(scanPath)

# def mapLoop(start, end, step, numScans):
# 	powers_input = prompt("Enter attenuator volts for rf2: ")
# 	powers = powers_input.split(",")
# 	# setup
# 	fileSystem = Environs.FileSystem
# 	file = \
# 		fileSystem.GetDataDirectory(\
# 					fileSystem.Paths["scanMasterDataPath"])\
# 		+ fileSystem.GenerateNextDataFileName()
# 	print("Saving as " + file + "_*.zip")
# 	print("")
# 	# start looping
# 	r = range(start, end, step)
# 	for i in range(len(r)):
# 		print("pg:rf2CentreTime -> " + str(r[i])
# 		print("pg:rf2BlankingCentreTime -> " + str(r[i])
# 		print("rf2 attenuator voltage -> " + powers[i]
# 		sm.AdjustProfileParameter("pg", "rf2CentreTime", str(r[i]), False)
# 		sm.AdjustProfileParameter("pg", "rf2BlankingCentreTime", str(r[i]), False)
# 		sm.AdjustProfileParameter("out", "externalParameters", powers[i], False)
# 		hc.SetRF2AttCentre(float(powers[i]))
# 		sm.AcquireAndWait(numScans)
# 		scanPath = file + "_" + str(i) + ".zip"
# 		sm.SaveData(scanPath)

if __name__=="__main__":
    print("Use mapLoopRepRate(start, end, step, numScans) for RepRate scan")
    print("Use mapLoopAny(plugin, param, start, end, step, numScans) for generic scan")
    print("Use megaMapLoop(plugin1, param1, start1, stop1, step1, plugin2, param2, start2, stop2, step2, numScans)  for megamapping scan")
    print("\"plugin\", \"param\" must be in double quotes!")