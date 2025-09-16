from System.Xml.Serialization import XmlSerializer
from EDMConfig import BlockConfig
from System import Type
from System.IO import *

def bctest():
    fileSystem = Environs.FileSystem
    settingsPath = fileSystem.Paths["settingsPath"] + "BlockHead\\"
    fs = FileStream(settingsPath+"default.xml", FileMode.Open)
    BlockConfig_type = Type.GetType("EDMConfig.BlockConfig, SharedCode")
    s = XmlSerializer(BlockConfig_type)
    bc = s.Deserialize(fs)
    fs.Close()
    eWave = bc.GetModulationByName("E").Waveform
    print(str(list(eWave.Code)))
    print(str(eWave.Inverted))
    eWave.Name = "E"
    print(eWave.Name)
    ws = WaveformSetGenerator.GenerateWaveforms( (eWave,), ("B","DB"))
    bc.GetModulationByName("E").Waveform.Inverted = WaveformSetGenerator.RandomBool()
    print(eWave.Inverted)
    print(bc.Settings)
    pass

def blockconfigtest():
    from QuSpinEDMLoop import *
    fileSystem = Environs.FileSystem
    dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
    settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
    cluster = fileSystem.GenerateNextDataFileName()
    blockIndex=System.Int32(99)
    bc = loadBlockConfig(settingsPath + "temp18Oct2404_0.xml")

    print(bc.Settings)
    eState = hc.EManualState
    bState = hc.BManualState
    mwState = True
    newbc = measureParametersAndMakeBC(cluster, eState, bState, mwState)
    newbc.Settings["clusterIndex"] = blockIndex
    print(newbc.Settings)
    tempConfigFile ='%(p)stemp%(c)s_%(i)s.xml' % {'p': settingsPath, 'c': cluster, 'i': blockIndex}
    saveBlockConfig(tempConfigFile, bc)
    System.Threading.Thread.CurrentThread.Join(500)
    print("Loading temp config.")
    bh.LoadConfig(tempConfigFile)
    pass