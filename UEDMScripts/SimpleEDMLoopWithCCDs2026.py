## SimpleEDMLoopWithCCDs2026.py
## Block loop with E-field switching (Behlke) and the two EMCCDs (CCDA, CCDB).
##
## Based on SimpleEDMLoopWithoutCCDs2026.py (E-field voltage list, Behlke switching,
## leakage monitor recalibration) with the EMCCD handling from SimpleEDMLoopTestShirley.py:
##   - hc.SyncCCDBlockName(cluster, blockIndex) before every block, so the CCD PCs name
##     their files <cluster>_<blockIndex:D5>_CCDA.tif / _CCDB.tif
##   - bh.AcquireAndWait() instead of bh.AcquireWithoutCCDsAndWait(). This runs the CCD
##     acquisition path in BlockHead (shot count sent to the CCDs, ready handshake,
##     burst start, per-shot CCD status check / retake, CCD config xml saved at the end).
##
## Note: block zips are named <cluster>_<blockIndex:04d>_<voltage>.zip, while the CCD
## files are named <cluster>_<blockIndex:05d>. blockIndex increases across all voltages,
## so each CCD file maps to exactly one block zip through blockIndex.
##
## The CCD acquisition software must be running on both CCD PCs before starting:
## SyncCCDBlockName and AcquireAndWait both wait (with no timeout) until CCDA and CCDB
## report ready.

## Import a whole load of stuff
from System.IO import *
from System.Drawing import *
from System.Runtime.Remoting import *
from System.Threading import *
from System.Windows.Forms import *
from System.Xml.Serialization import *
from System import *
from System import Type

from Analysis.EDM import *
from DAQ.Environment import *
from EDMConfig import *
from uedmfuncs import *

def saveBlockConfig(path, config):
    fs = FileStream(path, FileMode.Create)
    BlockConfig_type = Type.GetType("EDMConfig.BlockConfig, SharedCode") # Pythonnet requires explicit typing of the xmlserializer where IronPython figured it out
    s = XmlSerializer(BlockConfig_type)
    s.Serialize(fs,config)
    fs.Close()

def loadBlockConfig(path):
    fs = FileStream(path, FileMode.Open)
    BlockConfig_type = Type.GetType("EDMConfig.BlockConfig, SharedCode") # Pythonnet requires explicit typing of the xmlserializer where IronPython figured it out
    s = XmlSerializer(BlockConfig_type)
    bc = s.Deserialize(fs)
    fs.Close()
    return bc

def writeLatestBlockNotificationFile(cluster, blockIndex):
    fs = FileStream(Environs.FileSystem.Paths["settingsPath"] + "\\BlockHead\\latestBlock.txt", FileMode.Create)
    sw = StreamWriter(fs)
    sw.WriteLine(cluster + "\t" + str(blockIndex))
    sw.Close()
    fs.Close()

def checkPhaseLock():
    while (abs(pl.PhaseError) > 5):
        input("Check Phase Lock is running (hit enter if you are happy.)")
    print("Phase Lock checked.")

def printWaveformCode(bc, name):
    print(name + ": " + str(list(bc.GetModulationByName(name).Waveform.Code)) + " -- " + str(bc.GetModulationByName(name).Waveform.Inverted))

def measureParametersAndMakeBC(cluster, eState, bState, mwState):
    fileSystem = Environs.FileSystem
    print("Measuring parameters ...")
    bh.StopPattern()
    hc.UpdateBCurrentMonitor()
    hc.PollVMonitor()
    bh.StartPattern()

    print("V plus: " + str(hc.CPlusMonitorVoltage * hc.CPlusMonitorScale))
    print("V minus: " + str(hc.CMinusMonitorVoltage * hc.CMinusMonitorScale))
    print("Bias: " + str(hc.BiasCurrent))
    print("B step: " + str(abs(hc.FlipStepCurrent)))
    print("DB step: " + str(abs(hc.CalStepCurrent)))
    print("Phase Lock Error (deg): "+ str(pl.PhaseError))

    # load a default BlockConfig and customise it appropriately
    settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
    # bc = loadBlockConfig(settingsPath + "calibrateBfield.xml")
    # bc = loadBlockConfig(settingsPath + "default_EfieldBlocks.xml")
    # bc = loadBlockConfig(settingsPath + "default_EfieldBlocks_Unit8_STIRAPAOM01.xml")
    bc = loadBlockConfig(settingsPath + "default_EfieldBlocks_2026.xml")

    bc.Settings["cluster"] = str(cluster)
    bc.Settings["eState"] = eState
    bc.Settings["bState"] = bState
    bc.Settings["mwState"] = mwState

    bc.Settings["ePlus"] = hc.CPlusMonitorVoltage * hc.CPlusMonitorScale
    bc.Settings["eMinus"] = hc.CMinusMonitorVoltage * hc.CMinusMonitorScale
    bc.Settings["bBiasV"] = hc.SteppingBiasVoltage
    bc.Settings["greenDCFM"] = 0.0#hc.GreenSynthDCFM
    bc.Settings["greenAmp"] = hc.GreenSynthOnAmplitude
    bc.Settings["greenFreq"] = hc.GreenSynthOnFrequency
    bc.Settings["StirapRFfreqTrue"] = hc.StirapRFfrequencyTrueValue
    bc.Settings["StirapRFfreqFalse"] = hc.StirapRFfrequencyFalseValue

    bc.GetModulationByName("B").Centre = (hc.BiasCurrent)/1000
    bc.GetModulationByName("B").Step = abs(hc.FlipStepCurrent)/1000
    bc.GetModulationByName("DB").Step = abs(hc.CalStepCurrent)/1000
    # these next 3, seemingly redundant, lines are to preserve backward compatibility
    bc.GetModulationByName("B").PhysicalCentre = (hc.BiasCurrent)/1000
    bc.GetModulationByName("B").PhysicalStep = abs(hc.FlipStepCurrent)/1000
    bc.GetModulationByName("DB").PhysicalStep = abs(hc.CalStepCurrent)/1000

    # generate the waveform codes
    print("Generating waveform codes ...")
    eWave = bc.GetModulationByName("E").Waveform
    eWave.Name = str("E")
    ws = WaveformSetGenerator.GenerateWaveforms( (eWave,), ("B","DB"))#,"PI","RF1A","RF2A","RF1F","RF2F","LF1") )
    bc.GetModulationByName("B").Waveform = ws["B"]
    bc.GetModulationByName("DB").Waveform = ws["DB"]

    # change the inversions of the static codes E
    bc.GetModulationByName("E").Waveform.Inverted = WaveformSetGenerator.RandomBool()

    printWaveformCode(bc, "E")
    printWaveformCode(bc, "B")
    printWaveformCode(bc, "DB")

    # store e-switch info in block config
    print("Storing E switch parameters ...")
    bc.Settings["BehlkeBleedTime"] = hc.BehlkeBleedTime
    bc.Settings["BehlkeSwitchTime"] = hc.BehlkeSwitchTime
    bc.Settings["BehlkeSettleTime"] = hc.BehlkeSettleTime
    # store the E switch asymmetry in the block
    bc.Settings["E0PlusBoost"] = hc.E0PlusBoost
    # number of times to step the target looking for a good target spot, step size is 2 (coded in Acquisitor)
    bc.Settings["maximumNumberOfTimesToStepTarget"] = System.Int32(100)
    # minimum signal in the first detector, in Vus
    bc.Settings["minimumSignalToRun"] = 100.0
    bc.Settings["targetStepperGateStartTime"] = 2380.0
    bc.Settings["targetStepperGateEndTime"] = 2580.0

    # ccd config
    #gains = hc.QueryCCDGain()
    #bc.Settings["CCDAGain"] = gains[0]
    #bc.Settings["CCDBGain"] = gains[1]
    return bc

def windowValue(value, minValue, maxValue):
    if ( (value < maxValue) & (value > minValue) ):
        return value
    else:
        if (value < minValue):
            return minValue
        else:
            return maxValue

kTargetRotationPeriod = 100
kReZeroLeakageMonitorsPeriod = 100

## Target back-and-forth sweep.
## Each block moves the target by N steps (N = "target move steps" box in the hardware
## controller, only if "Serial move N before each block" is ticked). The script tracks
## the net position relative to where the sweep started and reverses direction whenever
## the next move would take it outside [0, kTargetTravelRange]. So the target walks
## 0 -> kTargetTravelRange -> 0 -> ... and never goes further than kTargetTravelRange
## steps from the start position.
## The position and direction are saved to a file after every successful move so that
## restarting the script continues the same sweep instead of starting a new window from
## wherever the target happens to be.
kTargetTravelRange = 1000      # max net steps away from the start position
kTargetStartDirection = 1     # +1 or -1: direction of the first leg of a fresh sweep

def targetSweepFile(settingsPath):
    return settingsPath + "targetSweepPosition.txt"

def loadTargetSweepState(settingsPath):
    # returns (position, direction); a fresh sweep if the file is missing or unreadable
    try:
        parts = File.ReadAllText(targetSweepFile(settingsPath)).split()
        position = int(parts[0])
        direction = 1 if int(parts[1]) >= 0 else -1
        return position, direction
    except Exception:
        return 0, kTargetStartDirection

def saveTargetSweepState(settingsPath, position, direction):
    try:
        File.WriteAllText(targetSweepFile(settingsPath), str(position) + " " + str(direction))
    except Exception as e:
        print("Warning: could not save target sweep position: " + str(e))

def moveTargetSweep(position, direction):
    # Moves the target one step of the sweep. Returns (stepsMoved, position, direction);
    # stepsMoved is signed and 0 if moving is disabled or the move failed.
    if not hc.moveTargetBeforeBlock:
        return 0, position, direction
    try:
        n = abs(int(hc.targetMoveSteps))
    except Exception:
        print("Target move skipped: steps box is not a whole number.")
        return 0, position, direction
    if n == 0:
        return 0, position, direction
    if n > kTargetTravelRange:
        print("Target move skipped: N = " + str(n) + " is larger than the travel range " + str(kTargetTravelRange) + ".")
        return 0, position, direction
    # reverse if this move would leave the window
    if not (0 <= position + direction * n <= kTargetTravelRange):
        direction = -direction
        print("Target sweep reversing direction.")
    steps = direction * n
    if hc.MoveTarget(steps):
        return steps, position + steps, direction
    print("Target move of " + str(steps) + " steps FAILED.")
    return 0, position, direction
#r = Random()

def EDMGo():
    # Setup E field voltages
    eFieldVoltagesInput = input("E-field voltages in kV: ")
    eFieldVoltages = [v.strip() for v in eFieldVoltagesInput.split(",") if v.strip() != ""]

    # Setup file
    f = None
    fileSystem = Environs.FileSystem
    dataPath = fileSystem.GetDataDirectory(fileSystem.Paths["edmDataPath"])
    settingsPath = fileSystem.Paths["settingsPath"] + "\\BlockHead\\"
    print("Data directory is : " + dataPath)
    print("")
    suggestedClusterName = fileSystem.GenerateNextDataFileName()
    sm.SelectProfile("Scan B")

    # User inputs data
    cluster = input("Cluster name [" + suggestedClusterName +"]: \n")
    if cluster == "":
        cluster = suggestedClusterName
        print("Using cluster " + suggestedClusterName)
    checkPhaseLock()
    eState = hc.EManualState
    eCurrentState = hc.EFieldPolarityBehlke
    cPlusV = 3*(hc.CPlusVoltage)
    cMinusV = 3*(hc.CMinusVoltage)
    print("E-state: " + str(eState))
    bState = hc.BManualState
    print("B-state: " + str(bState))
    mwState = True #hc.MWManualState
    print("mw-state: " + str(mwState))

    hc.SetCPlusVoltage(cPlusV)
    hc.SetCMinusVoltage(cMinusV)
    hc.UpdateVoltages()
    print("E Params refreshed")
    print("E-field on")
    hc.EnableEField(True)
    print("leakage monitors calibrated")

    # target sweep state (carried over from the last run unless reset)
    targetPosition, targetDirection = loadTargetSweepState(settingsPath)
    print("Target sweep: position " + str(targetPosition) + " of 0.." + str(kTargetTravelRange)
          + " steps, direction " + ("+" if targetDirection > 0 else "-")
          + ". Before-block move is " + ("ON" if hc.moveTargetBeforeBlock else "OFF") + ".")
    resetInput = input("Press Enter to continue this sweep, or type r to start a new sweep from the current target position: ")
    if resetInput.strip().lower() == "r":
        targetPosition, targetDirection = 0, kTargetStartDirection
        saveTargetSweepState(settingsPath, targetPosition, targetDirection)
        print("Target sweep reset.")

    # loop and take data
    blockIndex = 0
    maxBlockIndex = 10000
    while blockIndex < maxBlockIndex:
        for i in eFieldVoltages:
            eCurrentState = hc.EFieldPolarityBehlke
            hc.SetCPlusVoltage(float(i))
            hc.SetCMinusVoltage(float(i))
            hc.UpdateVoltages()
            hc.SwitchEBehlkeAndWait(eCurrentState)
            if (float(i)==0.0):
                System.Threading.Thread.CurrentThread.Join(20000)
            else:
                System.Threading.Thread.CurrentThread.Join(5000)
            # Make new block config with correct E Field
            bc = measureParametersAndMakeBC(cluster, eState, bState, mwState)

            # tell the CCD PCs the name of this block (waits until both CCDs are ready)
            print("Waiting for CCDs to be ready and syncing CCD block name ...")
            hc.SyncCCDBlockName(cluster, blockIndex)

            # move the target to a fresh spot at the start of the block, sweeping back and
            # forth within kTargetTravelRange (see moveTargetSweep). Only moves if
            # "Serial move N before each block" is ticked in the hardware controller.
            targetStepsMoved, targetPosition, targetDirection = moveTargetSweep(targetPosition, targetDirection)
            if targetStepsMoved != 0:
                saveTargetSweepState(settingsPath, targetPosition, targetDirection)
                print("Target moved " + str(targetStepsMoved) + " steps, sweep position " + str(targetPosition) + ".")

            print("Acquiring EDM block " + str(blockIndex) + " at " + str(i) + " kV ...")
            # save the block config and load into blockhead
            print("Saving temp config.")
            bc.Settings["clusterIndex"] = System.Int32(blockIndex)
            bc.Settings["targetStepsMoved"] = System.Int32(targetStepsMoved)
            bc.Settings["targetSweepPosition"] = System.Int32(targetPosition)
            tempConfigFile =str('%(p)stemp%(c)s_%(i)s.xml' % {'p': settingsPath, 'c': cluster, 'i': blockIndex})
            saveBlockConfig(tempConfigFile, bc)
            System.Threading.Thread.CurrentThread.Join(500)
            print("Loading temp config.")
            bh.LoadConfig(tempConfigFile)

            # take the block (PMT + EMCCDs) and save it
            print("Running molecule data acquisition with CCDs ...")
            bh.AcquireAndWait()
            print("Done.")
            blockPath = '%(p)s%(c)s_%(i)04d_%(v)s.zip' % {'p': dataPath, 'c': cluster, 'i': blockIndex, 'v': i}
            bh.SaveBlock(blockPath)
            print("Saved block "+ str(blockIndex) + ".")
            writeLatestBlockNotificationFile(cluster, blockIndex)
            System.Threading.Thread.CurrentThread.Join(5000)
            print("Done.")
            # increment and loop
            File.Delete(tempConfigFile)

            blockIndex = blockIndex + 1

            if ((blockIndex % kReZeroLeakageMonitorsPeriod) == 0):
                print("Recalibrating leakage monitors.")
                # calibrate leakage monitors

                eCurrentState = hc.EFieldPolarityBehlke
                cPlusV = 3*(hc.CPlusVoltage)
                cMinusV = 3*(hc.CMinusVoltage)

                print("E-field off")
                hc.FieldsOff()

                System.Threading.Thread.CurrentThread.Join(60000)
                hc.CalibrateIMonitors()
                print("E-field on")
                hc.SetCPlusVoltage(cPlusV)
                hc.SetCMinusVoltage(cMinusV)
                hc.UpdateVoltages()
                hc.SwitchEBehlkeAndWait(eCurrentState)
                print("E Switch Finished")
                System.Threading.Thread.CurrentThread.Join(10000)

                print("leakage monitors calibrated")

    bh.StopPattern()


def main():
    EDMGo()
    pass

if __name__=="__main__":
    main()
