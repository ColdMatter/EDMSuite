# This script is called when starting the Python interpreter.
# It's used to set up a comfortable environment for writing
# experiment control scripts.

import System
import System.IO
import clr
import sys

# Load some assemblies for access to the daq apps and mathematica

p = System.IO.Path.GetFullPath("..\\EDMRemotingHelper\\bin\\Debug\\")
clr.Path.Add(p)

clr.AddReferenceToFile("EDMRemotingHelper.dll")
clr.AddReferenceToFile("DAQ.dll")
clr.AddReferenceToFile("SharedCode.dll")
clr.AddReferenceToFile("ScanMaster.exe")
clr.AddReferenceToFile("Wolfram.NETLink.dll")

# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")

# add the EDMScripts directory to the module search path

pp = System.IO.Path.GetFullPath("..\\EDMScripts")
sys.path.append(pp)

print("EDM scripting control")