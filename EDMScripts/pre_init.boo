# This script is called when starting Booish.
# Any assemblies that you want to be available can be loaded
# here. It's a quirk of the compiler that nothing else can happen
# in the script after the assemblies have been loaded, so this
# script calls another script which does any other init work.

load("..\\EDMRemotingHelper\\bin\\Debug\\EDMRemotingHelper.dll")
load("..\\EDMRemotingHelper\\bin\\Debug\\DAQ.dll")
load("..\\EDMRemotingHelper\\bin\\Debug\\SharedCode.dll")
load("..\\EDMRemotingHelper\\bin\\Debug\\Wolfram.NETLink.dll")

