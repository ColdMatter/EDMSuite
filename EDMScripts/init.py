# EDM init script - sets up the boo interpreter environment for running
# edm scripts. Necessary assemblies will have already been loaded by
# pre_init.boo.

import System
import System.IO
import System.Drawing from System.Drawing
import System.Runtime.Remoting
import System.Threading
import System.Windows.Forms from System.Windows.Forms
import System.Xml.Serialization from System.Xml

import Wolfram.NETLink from Wolfram.NETLink
import Wolfram.NETLink.UI from Wolfram.NETLink

import EDMRemotingHelper from EDMRemotingHelper
import DAQ.Environment from DAQ
import DAQ.Mathematica from DAQ
import EDMConfig from SharedCode

# define functions that might be generally useful here

# loading and saving configurations
def saveBlockConfig(path as string, config as BlockConfig):
	fs = FileStream(path, FileMode.Create)
	s = XmlSerializer(BlockConfig)
	s.Serialize(fs,config)
	fs.Close()

def loadBlockConfig(path as string) as BlockConfig:
	fs = FileStream(path, FileMode.Open)
	s = XmlSerializer(BlockConfig)
	bc = s.Deserialize(fs)
	fs.Close()
	return bc

# initialisation code that will run when the interpreter starts up
remote = RemotingHelper()
scriptBasePath = Environs.FileSystem.Paths["edmScriptPath"]
print("EDM scripting control")

# load other scripts
#executeScript("SimpleEDMLoop.boo")
#executeScript("SimpleCondition.boo")


