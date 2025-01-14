# uedm_init.py - sets up the Python environment ready for scripting
# the edm control software.

import pythonnet
import clr
import sys
from System.IO import Path

# Load some system assemblies that we'll need
clr.AddReference("System.Drawing")
clr.AddReference("System.Windows.Forms")
clr.AddReference("System.Xml")

# Import the SharedCode DLLs
<<<<<<< HEAD
clr.AddReference(Path.GetFullPath("..\\..\\SEDM4\\Libraries\\SharedCode"))
=======
clr.AddReference(Path.GetFullPath("..\\..\\SEDM4\\Libraries\\SharedCode.dll"))
>>>>>>> 8ea0648ffb1b7766cb65908c7bef969ae9f1587d

# create connections to the control programs
import System

# usage message
print('UEDM python analysis package')
print('''This should import the SharedCode dll and create a scan serialiser to import scans and blocks easily
''')

# script shortcuts
import nt
pp = Path.GetFullPath("..\\..\\UEDMScripts\\Analysis")
files = nt.listdir(pp)
scriptsToLoad = [e for e in files if e.endswith(".py") and e != "uedm_init.py" and e != "winforms.py" and e != "uedmfuncs.py" and e != "winforms.py" and e != "uedm_init_pythonnet.py" and e != "uedm_analysis.py"]
<<<<<<< HEAD

=======
>>>>>>> 8ea0648ffb1b7766cb65908c7bef969ae9f1587d
for i in range(len(scriptsToLoad)):
            print(str(i+1) + ": " + scriptsToLoad[i])
print("")

def run(i):
	execfile(scriptsToLoad[i-1], globals())

def main():
    pass

if __name__=="__main__":
        main()