from Analysis import *
from Analysis.EDM import *
from Data.EDM import *
from System.Collections.Generic import *
from System.IO import *
from System.Runtime.Serialization.Formatters.Binary import *
from Newtonsoft.Json import *
import time

#bs = BlockSerializer()
#blockFile="C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\June2009\\26Jun0902_2.zip"
#block = bs.DeserializeBlockFromZippedXML(blockFile, "block.xml")

#db = BlockDemodulator()

<<<<<<< .mine
#serializer = JsonSerializer()
#sw = StreamWriter("C:\\Users\\jony\\Desktop\\test.json")
#writer = JsonTextWriter(sw)
=======
bf = BinaryFormatter()
fs=FileStream("C:\\Users\\jony\\Desktop\\tcsg.bin", FileMode.Open)
tcsg = bf.Deserialize(fs)

serializer = JsonSerializer()
sw = StreamWriter("C:\\Users\\jony\\Desktop\\test.json")
writer = JsonTextWriter(sw)
>>>>>>> .r562

eWave = (True,True,False,False)
bWave = (True,False,True,False)
db.


def run_script():
	print "Done!"
