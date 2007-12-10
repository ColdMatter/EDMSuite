from Analysis import *
from Analysis.EDM import *
from Data.EDM import *
from System.IO import *
from System.Runtime.Serialization.Formatters.Binary import *

bs = BlockSerializer()
block = bs.DeserializeBlockFromZippedXML("C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2007\\November2007\\14Nov0702_2.zip", "block.xml")
#tf = TOFFitter()
#rs = tf.FitTOF(block.GetAverageTOF(0))

bd = BlockDemodulator()
dc = DemodulationConfig()
dg0 = DetectorExtractSpec.MakeGateFWHM(block, 0, 0, 1)
#dg0 = DetectorExtractSpec()
#dg0.Index = 0
#dg0.GateLow = 2155
#dg0.GateHigh = 2339
dg1 = DetectorExtractSpec.MakeGateFWHM(block, 1, 0, 1)
#dg1 = DetectorExtractSpec()
#dg1.Index = 1
#dg1.GateLow = 563
#dg1.GateHigh = 611
#dg1.BackgroundSubtract = False
dg2 = DetectorExtractSpec.MakeWideGate(2)
dg2.Integrate = False
dc.DetectorExtractSpecs.Add(dg0)
dc.DetectorExtractSpecs.Add(dg1)
dc.DetectorExtractSpecs.Add(dg2)

db = bd.DemodulateBlock(block, dc)

fs = FileStream("c:\\Users\\jony\\Desktop\\db.bin",FileMode.Create)
bf = BinaryFormatter()
bf.Serialize(fs, db)
fs.Close()

def run_script():
	print("done!")
