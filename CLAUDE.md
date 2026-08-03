# General
This project includes the control code for the molecular beam based experiments at the Centre for Cold Matter, Imperial College London. 
It is currently used to drive experiments including the YbF electron EDM measurement, laser cooling of CaF and SrF, buffer gas cooling of YbF, Li/LiH sympathetic cooling, and precsion measurement of CH transitions to test the stability of fundamental constants.

For historical reasons this project also includes the low-level analysis code for the electron EDM experiment.

The code is not terribly pretty, but it does the job. There is minimal documentation, and what is there should be treated with suspicion!

# Basic MOTMaster Experiment structure

MOTMaster experiments use NI digital and analogue pattern generators to control experiment hardware. 
The patterns are created in .cs scripts e.g. MoleculeMOTMasterScripts\0. DDSBlueMOT.cs. 
The DDS board fires first and triggers the analogue pattern and any other devices.  
MOTMaster experiments are fired by the MOTMaster Controller.
Patterns live inside a sequence.

# Current proeject
The current branch is for building a new driver for the spectrum DDS.  The manual can be found broken up by chapter in spectrum_dds_chapters. this is a pdf-to-markdown conversion, tables may be mangled and figures are not present.

## MoleculeMOT

This update is only for the MoleculeMOT experiment (also known as cafmot). Other experiments e.g. Lattice and AlF use this suite, ignore any folders related to them. I expect you only need to look at DAQ, MOTMaster, MoleculeMOTHardwareControl, MoleculeMOTMasterScripts. You will find references to NeanderthalDDS; this is the old controller. I want the new driver built entirely from scratch, ignore anything you find about the previous driver so it doesn't influence you. The exception to this is how the DDS pattern is defined in experiments

## Python testing
There is a poetry environment defined in pyproject.toml which has what you should need to test things in python without having to build C#. 