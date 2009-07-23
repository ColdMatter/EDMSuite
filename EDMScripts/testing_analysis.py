from Analysis import *
from Analysis.EDM import *
from Data.EDM import *
from System.Collections.Generic import *
from System.IO import *
from System.Runtime.Serialization.Formatters.Binary import *
from BeIT.MemCached import *
import time

bs = BlockSerializer()
blockFile="C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\June2009\\26Jun0902_2.zip"
block = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml")

dc = DemodulationConfig.GetStandardDemodulationConfig("fwhm", block)
dcfast = DemodulationConfig.GetStandardDemodulationConfig("fast", block)
dcslow = DemodulationConfig.GetStandardDemodulationConfig("slow", block)
dcvfast = DemodulationConfig.GetStandardDemodulationConfig("vfast", block)
dcvslow = DemodulationConfig.GetStandardDemodulationConfig("vslow", block)

bd = BlockDemodulator()

#db = bd.DemodulateBlock(block, dc)

#sc.AddBlock(blockFile,("fwhm","slow","fast"))


def run_script():
	print "Done!"
