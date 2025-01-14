#%%
from uedm_analysis import *
import csv
import numpy as np
import os
import glob
import matplotlib.pyplot as plt
import scipy.signal as scisig
import pandas as pd

datadrive=str(os.environ["Onedrive"]+"\\UltracoldData\\Data")
scriptdrive=datadrive+"\\ScriptData\\"
quspindrive=datadrive+"\\2024\\Quspin\\"
blockdrive=datadrive+"\\BlockData\\"

drive = blockdrive
print(drive)

pattern="/**/27Nov2400_*.zip"
files = glob.glob(f'{drive}{pattern}', recursive=True)
print("Matching files: ", [os.path.basename(f) for f in files])
#%%
from Data.EDM import *
bs=BlockSerializer()

block1=bs.DeserializeBlockFromZippedXML(files[0],"block.xml")
#%%
quspins=np.zeros((21,256))
for i in range(21):
    # print(i)

    quspins[i]=(1000/2.7)*np.array(block1.GetTOFMeanArray(i,100,150000))
# print(hpy)

x=np.linspace(1,256,256)
plt.subplots()
plt.plot(np.transpose(quspins))
plt.show()
#%%
def boolSign(x:bool):
    if (x):
        return 1
    else:
        return -1
    
def extractWaveform(block,modulation:str):
    wav=list(block.Config.GetModulationByName(modulation).Waveform.Bits)
    return np.array(list(map(boolSign,wav)))

print(extractWaveform(block1,"B"))
print(extractWaveform(block1,"B")*extractWaveform(block1,"E"))
#%%
def extractChannel(block,switches,detIndex,calib,gateLow,gateHigh):
    if switches:
        channelWaveform = np.prod(np.array([extractWaveform(block,switch) for switch in switches]),axis=0)
    else:
        channelWaveform = 1
    return np.mean((1/calib)*np.array(block.GetTOFMeanArray(detIndex,gateLow,gateHigh))*channelWaveform)

def extractSPChannel(block,switches,detName):
    if switches:
        channelWaveform = np.prod(np.array([extractWaveform(block,switch) for switch in switches]),axis=0)
    else:
        channelWaveform = 1
    return np.mean(np.array(block.GetSPData(detName).ToArray())*channelWaveform)
    
def extractChannels(block,switches,calib,gateLow,gateHigh):
    return dict(
        fvy=extractChannel(block,switches,0,-calib,gateLow,gateHigh),
        hty=extractChannel(block,switches,1,-calib,gateLow,gateHigh),
        hry=extractChannel(block,switches,2,calib,gateLow,gateHigh),
        hsy=extractChannel(block,switches,3,calib,gateLow,gateHigh),
        hqy=extractChannel(block,switches,4,-calib,gateLow,gateHigh),
        hpy=extractChannel(block,switches,5,-calib,gateLow,gateHigh),
        hoy=extractChannel(block,switches,6,-calib,gateLow,gateHigh),
        hmy=extractChannel(block,switches,7,-calib,gateLow,gateHigh)
    )

def extractNoise(block,detIndex,calib,gateLow,gateHigh):
    return np.std((1/calib)*np.array(block.GetTOFMeanArray(detIndex,gateLow,gateHigh)))

def extractSPNoise(block,detName):
    return np.std(np.array(block.GetSPData(detName)))

def extractNoises(block,calib,gateLow,gateHigh):
    return dict(
        fvy=extractNoise(block,0,-calib,gateLow,gateHigh),
        hty=extractNoise(block,1,-calib,gateLow,gateHigh),
        hry=extractNoise(block,2,calib,gateLow,gateHigh),
        hsy=extractNoise(block,3,calib,gateLow,gateHigh),
        hqy=extractNoise(block,4,-calib,gateLow,gateHigh),
        hpy=extractNoise(block,5,-calib,gateLow,gateHigh),
        hoy=extractNoise(block,6,-calib,gateLow,gateHigh),
        hmy=extractNoise(block,7,-calib,gateLow,gateHigh)
    )

print(extractChannels(block1,["DB"],2.7,100,150000))
print(extractNoises(block1,2.7,100,150000))
#%%
extractMagDataFromBlock[block_,calib_,gateLow_,gateHigh_]:={
"eChan"->extractChannels[block,{"E"},calib,gateLow,gateHigh],
"dbChan"->extractChannels[block,{"DB"},calib,gateLow,gateHigh],
"bChan"->extractChannels[block,{"B"},calib,gateLow,gateHigh],
"sigChan"->extractChannels[block,{},calib,gateLow,gateHigh],
"edbChan"->extractChannels[block,{"E","DB"},calib,gateLow,gateHigh],
"noise"->extractNoise[block,calib,gateLow,gateHigh],
"westLeakageEChan"->{extractSPChannel[block,{"E"},"WestCurrent"],extractSPNoise[block,"WestCurrent"]},
"eastLeakageEChan"->{extractSPChannel[block,{"E"},"EastCurrent"],extractSPNoise[block,"EastCurrent"]},
"westLeakageChan"->{extractSPChannel[block,{},"WestCurrent"],extractSPNoise[block,"WestCurrent"]},
"eastLeakageChan"->{extractSPChannel[block,{},"EastCurrent"],extractSPNoise[block,"EastCurrent"]},
"miniFlux1EChan"->{extractSPChannel[block,{"E"},"MiniFlux1"],extractSPNoise[block,"MiniFlux1"]},
"miniFlux1Chan"->{extractSPChannel[block,{},"MiniFlux1"],extractSPNoise[block,"MiniFlux1"]},
"timestamp"->timeStampToDateList[block@TimeStamp@Ticks],
"ePlus"->Round[block@Config@Settings["ePlus"],500]
}
#%%
import datetime
print(datetime.datetime(1,1,1))
#%%
def timestampToDateTime(ticks):
    return datetime.datetime(1,1,1)+datetime.timedelta(microseconds=ticks/10)
def extractMagDataFromBlock(block,calib,gateLow,gateHigh):
    return dict(
        eChan=extractChannels(block,["E"],calib,gateLow,gateHigh),
        dbChan=extractChannels(block,["DB"],calib,gateLow,gateHigh),
        bChan=extractChannels(block,["B"],calib,gateLow,gateHigh),
        sigChan=extractChannels(block,[],calib,gateLow,gateHigh),
        edbChan=extractChannels(block,["E","DB"],calib,gateLow,gateHigh),
        noise=extractNoises(block,calib,gateLow,gateHigh),
        westLeakageEChan=[extractSPChannel(block,["E"],"WestCurrent"),extractSPNoise(block,"WestCurrent")],
        eastLeakageEChan=[extractSPChannel(block,["E"],"EastCurrent"),extractSPNoise(block,"EastCurrent")],
        westLeakageChan=[extractSPChannel(block,[],"WestCurrent"),extractSPNoise(block,"WestCurrent")],
        eastLeakageChan=[extractSPChannel(block,[],"EastCurrent"),extractSPNoise(block,"EastCurrent")],
        miniFlux1EChan=[extractSPChannel(block,["E"],"MiniFlux1"),extractSPNoise(block,"MiniFlux1")],
        miniFlux1Chan=[extractSPChannel(block,[],"MiniFlux1"),extractSPNoise(block,"MiniFlux1")],
        timestamp=timestampToDateTime(block.TimeStamp.Ticks),
        ePlus=(np.round(block1.Config.Settings["ePlus"]/500)*500)
    )

print(extractMagDataFromBlock(block1,2.7,100,150000))
#%%
def extractShotToShotData(block,calib,gateLow,gateHigh):
    return dict(
        hpy=(1/calib)*np.array(block.GetTOFMeanArray(1,gateLow,gateHigh)),
         wL=np.array(block.GetSPData("WestCurrent").ToArray()),
         eL=np.array(block.GetSPData("EastCurrent").ToArray()),
         miniFlux1=np.array(block.GetSPData("MiniFlux1").ToArray()),
         eWaveformInverted=list(block.Config.DigitalModulations)[0].Waveform.Inverted,
         phaseLockFreq=np.array(block.GetSPData("PhaseLockFrequency").ToArray()),
         phaseLockError=np.array(block.GetSPData("PhaseLockError").ToArray()))
    
print(extractShotToShotData(block1,2.7,100,150000)['phaseLockError'])
#%%
analysisCount=0
def loadBlocksAndExtractMagData(blockFile,calib,gateLow,gateHigh):
    block=bs.DeserializeBlockFromZippedXML(blockFile,"block.xml")
    return extractMagDataFromBlock(block,calib,gateLow,gateHigh)

blockData=np.array([])
for file in files[:10]:
    blockData=np.append(blockData,loadBlocksAndExtractMagData(file,2.7,100,150000))
    analysisCount+=1
# %%
for block in blockData:
    print(block['timestamp'])