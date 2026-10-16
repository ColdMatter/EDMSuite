# Safety
**I never run a MOTMaster script or fire the experiment, or interact with experiment hardware.** Running a pattern at the wrong moment
risks human harm and equipment damage. Anything that drives the NI pattern generators, fires a
sequence, or triggers the experiment is done **by the user, at a time of their choosing**.

# General

This code is used by multiple users on multiple experiments. 
In addition to SharedCode, much of the codebase is used by different experiments in different ways. 
If you are making a change, this may either be a feature change for all users, or for one experiment. When adding new features, is important to keep existing functionality and behaviour the same unless explicitly told otherwise, so other users are not disrupted.

The code is not terribly pretty, but it does the job. There is minimal documentation, and what is there should be treated with suspicion!

# Basic MOTMaster Experiment structure

MOTMaster experiments use NI digital and analogue pattern generators to control experiment hardware. 
The patterns are created in .cs scripts e.g. MoleculeMOTMasterScripts\0. DDSBlueMOT.cs. 
The DDS board fires first and triggers the analogue pattern and any other devices.  
MOTMaster experiments are fired by the MOTMaster Controller.
Patterns live inside a sequence.


## Python testing
There is a poetry environment defined in pyproject.toml which has what you should need to test things in python without having to build C#. 