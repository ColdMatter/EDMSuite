# MOTMaster User Guide
This is a guide for using the latest version of MOTMaster

## Requirements
To run MOTMaster, you need to create instances of these classes for your experiment. Examples are located in the `DAQ` folder in `EDMSuite`
* `HardwareClass`:
  * `AnalogOutputChannels`
  * `DigitalOuptutChannels`
  * `AnalogInputChannels`
  * `RS232Instrument` added to `Instruments`
  * `MMConfig`
* `MMConfig`
  * `UseAI` - turns acquisition on/off
  * `UseMuquans` - turns timed serial communication for muquans laser on/off
  * `UseHSDIO` - turns digital pattern generation for NI-HSDIO cards on/off

* `FileSystem`
  *  
## Defining a Sequence
In previous versions of MOTMaster, a `MOTMasterSequence` was compiled from a `MOTMasterScript`. 
### Parameters

### Analog Channels

### Digital Channels

### Serial Channels

## Saving and Loading Sequences

### Converting from Cicero

## Recording Analog Input Data

## Running an Experiment

## Remote Connections

### Axel-Hub

### Mathematica

