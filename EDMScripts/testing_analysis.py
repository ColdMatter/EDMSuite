from Analysis import *
from Analysis.EDM import *
from Data.EDM import *
from System.Collections.Generic import *
from System.IO import *
from System.Runtime.Serialization.Formatters.Binary import *
from BeIT.MemCached import *
import time

bs = BlockSerializer()
block = bs.DeserializeBlockFromZippedXML("C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2008\\August2008\\07Aug0802_2.zip", "block.xml")

dc = DemodulationConfig.GetStandardDemodulationConfig("fwhm", block)

bd = BlockDemodulator()

db = bd.DemodulateBlock(block, dc)




def run_script():
	print "Done!"
