# MOTMaster User Guide
This is a guide for developing the latest version of MOTMaster

## Requirements
Like most projects within `EDMSuite` instances of the following classes for your experiment. Examples are located in the `DAQ` project
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
  * `UseMSquared` - turns communication to an MSquared laser on/off
* `FileSystem`
  *  
## Defining a Sequence
In previous versions of MOTMaster, a `MOTMasterSequence` was compiled from a `MOTMasterScript`. Scripts are now compiled from the `SequenceBuilder` which is represented as a grid of timesteps in the UI. Each timestep contains the following properties:
  * Name
  * Description
  * Duration (in units given by multiplier)
  * Time multiplier (us, ms or s)

As well as properties which define the state of each analogue and digital channel over that timestep. These are  explained in more detail below.
### Parameters
Parameters act as variables which can modify any numerical value in the sequence. Typically, these are used to represent key timestep durations or analogue voltages. To use a parameter in a sequence, simply write the name of the parameter in the corresponding textbox. When the `SequenceBuilder` converts the sequence into output patterns, the parameter names are converted into their values. Additionally, Parameters have the following properties
* IsHidden - if true, this parameter will not appear in a list of ones to scan
* SequenceVariable - if false, this parameter will be varied without modifying the underlying DAQ card patterns. This can cause unexpected behaviour if incorrectly used in a sequence.
### Analog Channels
Analog channels are named according to the `AnalogOutputChannel` objects defined in the Hardware class. For each timestep, the channels will have a combo box which selects the `AnalogArg` of the channel. Selecting one other than `Continue` will open an editor to edit the `AnalogArg` properties
#### AnalogArg Properties
* Continue - keep the last used value
* Single Value
  * Start time - time relative to the start of the timestep to set the value. Defaults to 0
  * Value - voltage level to set for the channel
* Linear Ramp - ramps the voltage linearly in time
  * Start time - as above
  * Duration - time to ramp over
  * Ramp value - final value for the ramp
* Pulse - pulse the voltage and resets it to the original value
  * Start time - as above
  * Duration - time after which the state returns to its inital value
  * Pulse value - value for the pulse
* XY Pairs - a list of times and voltages
  * Interpolation type - Specifies how to interpolate the voltage between each co-ordinate. Either `Piecewise linear` or `Step`
* Function
  * Start time - as above
  * Duration - as above
  * Function - a mathematical function. The time can be specified by `t` and standard trigonometric functions can be used, as well as `Log`, `Ln`, `e` and $\pi$
### Digital Channels
Similar to the analogue channels, digital channels are defined as `DigitalOutputChannels` in the `HardwareClass`. These are either enabled or disabled for each timestep
### Serial Channels
Serial channels are defined as `RS232Instruments` (or a derived class) in the `HardwareClass`. Each timestep contains an `RS232Commands` property which can be used to add a serial command to write to each channel at this timestep. 
#### Timing serial output
It is not possible to output serial commands with the same precision as the analog and digital voltages written by the DAQ hardware. However, MOTMaster is able to output serial commands with a timing accuracy of around 1ms by using a digital channel to trigger the output, using the `MuquansController` class in `DAQ`. This creates a `CounterTask` on a channel named `Counter` in the `CounterChannels` collection in `HardwareClass` and counts edges on the `pfi1` channel of the `analogOut` board. Every time an edge is counted, the next set of serial commands is output. Presently, this class is designed to work for the Muquans laser system, but can in principle be extened to work with any number of `RS232Instuments` The main caveat is that all serial channels output their commands sequentially and there must be the same number of commands on each channel. If not all channels are intended to output a command for a particualar timestep, the others must have a dummy command, for instance `""`. The counter channel must be wired to a channel named `serialPreTrigger` which does not directly appear in the `SequenceDataGrid`. The `serialPreTrigger` fires a fixed time before the intended output of the serial command, which is defined in the `SequenceBuilder` class. This also means that the first serial command must be sent at least this amount of time after the start of the sequence. For precise timing, it is recommend to use hardware which will execute the intended function of this command after sensing an edge on another digital channel.
## Saving and Loading Sequences
Sequences are saved to file as a JSON object with the extension `.sm2` which can be read in a text editor or by other programming languages which support JSON, such as Mathematica or Python. Each sequence contains a list of `SequenceStep` objects which completely define each timestep and a list of `Parameters`. Due to the naming conventions of the hardware channels, there may be unexpected behaviour if an older sequence is loaded which does not contain a hardware channel that was later added. The sequence contains no reference from a channel name to the physical channel. If a physical channel is renamed, it is recommended to find and replace all references to this name in the sequence file.

There is currently a bug in MOTMaster where the UI is not updated if a new sequence is loaded, although the underlying `Sequence` object is. Relaunching the application will fix this.
### Converting from Cicero
There is some functionality available to load a sequence from Cicero, although this is experimental and is mainly intended as a one-time use. In order to convert a Cicero sequence, a Cicero settings file must be loaded before the sequence file. Every cicero timestep is converted into a `SequenceStep` object in the `SequenceData` and the data for each Cicero channel in this step is converted into data for the `HardwareChannel` which has the same name. Digital channels simply take the state during that timestep (it cannot convert a digital pulse defined within) a step). Serial channels take the raw string output of that channel. If the string contains a newline "\n" at the end, this must be removed in the UI. Finally, analog channels are converted to the closest equivalent property in MOTMaster. Single values and functions are converted to their equivalents and analogue waveforms are converted into lists of XY pairs, with their corresponding interpolation type. It is recommended to check the sequence before trying to run it.
## Recording Analog Input Data
At the time of writing, the option to set acquisition of analogue input data must be set in the `MMConfig` file that exists within `NavigatorHardware`. `MOTMasterSequence` contains a `MMAIWrapper` which defines properties for the analogue input channels, such as sample rate, number of samples to acquire and the names of the channels in the `AnalogInputChannel` hashtable in `HardwareClass`. This must be configured within the `SequenceBuilder` class as these are not likely to be modified frequently. In the Navigator experiment, the start of this acquisition is triggered on an input to the `pfi0` channel on the PXI-4462 card. The accelerometer input is on `ai0` and the photodiode is on `ai1`. The `acquisitionTrigger` channel is used to trigger the start of the acquisition and a sampling rate of 200 kHz requires 64 pre-trigger samples to be acquired in order for the timing of the analogue input to be synchronised with the output channels. This is handled in the `ExperimentData` class which calculates the number of samples required and groups the acquired data by timestep. To do this, the acqusition trigger channel must be kept high from the start of acqusition until the end. If input data should not be saved from any timestep between these, the description for that step should contain **"DNS"**. 
### Accelerometer acqusition
The accelerometer signal is acquired on a different channel and during a different sequence timestep. To make this as simple as possible from the UI, the timestep for which the accelerometer should be acquired must contain the word **Interferometer** within the description. From this voltage data, the mean and standard deviation of the acceleration are calculated, along with the equivalent Interfrometer phase using the Transfer function. This method of acqusition configuration is very specific to the Navigator experiment and may change in a future version.
## Running an Experiment
### Configuring a Sequence
Before MOTMaster can write a sequence to the DAQ cards, it needs to know the desired sample rate of them. It is assumed that there is only one digital output card and one analogue output card - their sample rates are defined by the parameters `AnalogClockFrequency` and `PGClockFrequency`/`HSClockFrequency` depending on the type of digital card used - either one which uses NI-DAQmx or NI-HSDIO. The parameter `AnalogLength` is also required to define the number of samples for the analogue pattern. Once the sequence is built, it will update this value.

Each experiment can be given a name to label the saved data and parameters. This will append a timestamp to the name so that multiple experiments with the same name are still distinguishable. If no name is given, the defualt value of just a timestamp will be used
### Running a sequence
### Repeat mode
The Run tab is used to repeatedly run the same sequence. If a positive number is entered, the sequence runs for that many times. Otherwise, the sequence can run continuously by entering a value of -1. In this mode, the sequence will not be re-written to the cards before each cycle, which reduces the time between shots.

### Scan mode
The Scan tab can be used to iterate through a parameter value. The drop-down menu shows all available parameters. Once a parameter is selected, this runs the sequence, modifying the value of the chosen parameter using the given values of `From`, `By` and `To`. After completing or aborting this loop, the parameter returns to its initial value.

### Update mode
The Update tab can be used to maually update a single parameter. This can also be done using `Edit >> Sequence Parameters` but is included here for convenience
### Data Storage
Each run is saved inside the `Data` folder which exists in the root directory of MOTMaster. This contains further folders organised by year, month and day. The defualt location can be modified using the `MOTMasterDataPath` inside the `FileSystem` class.

Once a run or a scan is started, MOTMaster creates two files - `*_data.ahf` contains acquired data and `*_parameters.ahf` contains the parameters for that experiment. Each of these files are in a JSON format and place all data inside an `MMBatch` object. This contains either one or many `MMExec` objects which contain data about each shot.
#### Parameters
Under the current design of MOTMaster, at most one parameter varies during an experiment. Therefore, the parameters file contains a single `MMExec`, which lists the value of all the parameters within a `prm` object. The scan parameter is contained in `scanParam`, along with the values it iterates to and from.
#### Data
The Data file contains an `MMExec` for each shot. The voltage measured by the photodiode in each timestep is stored as an array inside `prms` with a name corresponding to the timestep. If data is recorded from the accelerometer, the mean, standard deviation and phase from this is also recorded.

### Controlling the MSquared laser
The Laser Pulses tab is used to control parameters for the MSquared laser. The power, duration and phase of each pulse can be changed, as well as the time between each Interferometer pulse, the frequency of the PLL and a Chirp Rate/ Duration. Each pulse can be switched on/off as well as the chirp. If any parameter is outside the range accepted by the MSquared laser, MOTMaster will inform you before trying to send data to the laser. All the numeric values here are linked to `Parameters` and can also be iterated over. Currently, the DCS ICE-Bloc module takes some time to interpret and update the FPGA sequence once modified, which reduces the duty cycle of the experiment.
## Remote Connections
The JSON structure of the data sent by MOTMaster allows other programs to remotely control it or accept data. This is mainly used to send data back and forth to the Axel-hub program, but any software which can send and receive messages using TCP can also control MOTMaster. Currently, this is less developed and MOTMaster does not send any data to the other program.
### Axel-Hub
After selecting Axel-hub, MOTMaster attempts to connect to a running Axel-hub application, or launches one if it cannot find it. Either program can then initiate a scan or a run. Axel-hub will plot the data acquired by MOTMaster, as well as save the data into csv format, using the mean and standard deviation of the voltages measured by the photodiode. 
### Mathematica

