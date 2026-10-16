Mode DDS20 (20-tone DDS)
This card mode requires the SPCM_FEAT_EXTFW_DDS20 (formally named SPCM_FEAT_EXTFW_DDS) feature to
be installed and a DDS20 capable firmware image to be active. For details see “Feature and firmware matrix
of AWG and DDS products” paragraph in the “Introduction” chapter.
General Information
DDS – Direct Digital Synthesis – is a method for generating arbitrary periodic waves from a single, fixed-frequency reference clock and is
widely used in signal generation applications. The DDS functionality implemented on Spectrum Instrumentation’s AWGs is based on the prin-
ciple of adding multiple “DDS cores” to generate a multi-carrier (multi-tone) signal, with each carrier having its own well-defined frequency,
amplitude and phase. In addition to these static parameters, there are also built-in dynamic parameters like frequency and amplitude slope
to allow for intrinsic linear changes for multiple cores.
In the simplest case, the user writes the commands frequency and amplitude for a specific DDS core to the card. The card will then output a
single periodic sine wave continuously until the user writes a change to the card. These changes are written to the card in the form of com-
mands (see below tables for a list of the available commands) that are added to a First-In-First-Out (FIFO) buffer. These commands are then
executed in the order in which they were written to the card.
Feature Overview
The DDS firmware gives access to a number of DDS cores that each generate a sine wave, see register SPC_DDS_NUM_CORES for the
maximum number of cores for your current configuration. The following parameters can be used to parametrise each of the sine waves:
Static Parameters:
• Frequency (e.g., SPC_DDS_CORE0_FREQ)
• Amplitude (e.g., SPC_DDS_CORE0_AMP)
• Phase (e.g., SPC_DDS_CORE0_PHASE)
Dynamic Parameters:
• Frequency slope (e.g., SPC_DDS_CORE0_FREQ_SLOPE)
changes the active frequency of the DDS core with a linear slope
• Amplitude slope (e.g., SPC_DDS_CORE0_AMP_SLOPE)
changes the active amplitude of the DDS core with a linear slope.
These cores can be routed to a specific channel as described in the section about the Core connections.
Please be aware that each firm- and hardware configuration comes with its own specific number of cores
and not all the devices and options contain the same number of cores.
Controlling the DDS functionality
The whole card needs to be switched to DDS mode to allow programming of the DDS functionality. The below table only shows the DDS
mode. All other modes are shown in the “Generation modes” chapter of this manual:
Table 147: Spectrum API: card mode and read out of available card mode software registers
Register Value Direction Description
SPC_CARDMODE 9500 read/write Defines the used operating mode, a read command will return the currently used mode.
SPC_AVAILCARDMODES 9501 read Returns a bitmap with all available modes on your card. Please see the general chapter “Generation
modes” for a list of all supported modes.
SPC_REP_STD_DDS 4000000h DDS replay mode functionality.
(c) Spectrum Instrumentation GmbH 155

Mode DDS20 (20-tone DDS) DDS Command buffer fill size, overrun and underrun
Command FIFO
The DDS functionality is con-
trolled through commands that
are written to a driver-internal
list and then written to the card
when the command
SPCM_DDS_CMD_WRITE_TO_-
CARD is sent. These lists of com-
mands are put onto a First-In-
First-Out (command queue) buff-
er and executed one after the
other.
The right hand command queue
overview gives an idea how
commands are used to generate
the different output states of a
single DDS core. Commands in
Image 77: command queue, trigger and timer interaction
the command queue are execut-
ed from top to bottom.
The settings are first written to a set of “shadow registers” that are a separate set of registers in parallel to the active DDS configuration reg-
isters. One command after the other manipulates the shadow registers until the command SPCM_DDS_CMD_EXEC_AT_TRIG is received, then
writing from the FIFO to the shadow registers is stopped and the card starts waiting for the next internal trigger. After a trigger is received
the shadow registers are transferred to the active registers.
Hence, if two commands writing to the same register are sent before an
SPCM_DDS_CMD_EXEC_AT_TRG command was sent, then the second command overwrites the first.
DDS Command buffer fill size, overrun and underrun
There are two modes for transferring commands to the
card, Single and DMA mode. In Single mode, com-
mands are sent to the card one after another, while in
DMA mode commands are sent in chunks, which is use-
ful for sending a lot of commands. In both modes, com-
mands are executed by the DDS in the order that the user
enters them. This section assumes that Single mode is be-
ing used, for more information on DMA see the section
“Defining the data transfer mode”.
The DDS data pipeline contains multiple FIFOs through
which commands pass through. On the user application
side, there is a software FIFO of dynamic size (automat-
ically adjusted by the driver as needed), that contains all
commands the user enters until they are written to the
card, using the SPCM_DDS_CMD_WRITE_TO_CARD
command.
In Single mode, commands are then sent directly to the
“DDS command FIFO”, the size of which can be read out
by SPC_DDS_QUEUE_CMD_MAX. The current number
of commands in this buffer can be read out with the SP-
C_DDS_QUEUE_CMD_COUNT register. Note that in
DMA mode, this FIFO does not contain all commands, so
the returned value of SPC_DDS_QUEUE_CMD_COUNT Image 78: The different FIFO buffers in DDS mode
may not be correct. In Single mode, if more commands
are sent than fit inside this buffer, an overrun error is
flagged.
All the commands in the “DDS command FIFO” are written one-by-one into the “Shadow Register” until a SPCM_DDS_CMD_EXEC_AT_TRG
command is received. The “Shadow Register” is essentially a large dictionary, mapping the values the user has entered to each individual
register of the DDS (e.g. 100 KHz to SPC_DDS_CORE0_FREQ, External trigger to SPC_DDS_TRG_SOURCE etc.). After an SPCM_DDS_C-
MD_EXEC_AT_TRG is received, the shadow register remains unchanged until a valid trigger or timer event is received. When this occurs, the
entire content of the “Shadow Register” is written to the DDS registers simultaneously, and the output of the DDS will change accordingly. At
the same time, the shadow register will again read new commands from the “DDS command FIFO” until the next SPCM_DDS_CMD_EX-
EC_AT_TRG command, and the process repeats.
(c) Spectrum Instrumentation GmbH 156

Mode DDS20 (20-tone DDS) DDS Command buffer fill size, overrun and underrun
If the hardware receives a trigger or timer event before the “Shadow Register” is halted, this is handled as an error and the SPC-
M_DDS_STAT_QUEUE_UNDERRUN flag in the SPC_DDS_STATUS register is set.
Table 148: Spectrum API: DDS information on the command queue
Register Value Direction Description
SPC_DDS_QUEUE_CMD_MAX 608005l read Length of the DDS command queue.
SPC_DDS_QUEUE_CMD_COUNT 608006l read Current number of entries in the DDS command queue.
SPC_DDS_NUM_QUEUED_CMD_IN_SW 608011l read Returns the number of commands that are currently in the command queue in the library and not yet
written to the card. Writing to the card is done with the SPCM_DDS_CMD_WRITE_TO_CARD com-
mand.
DDS Command
Table 149: Spectrum API: DDS command register
Register Value Direction Description
SPC_DDS_CMD 608003 write Register for DDS-specific commands.
SPCM_DDS_CMD_RESET 1h Resets all DDS-specific firmware registers to default state.
SPCM_DDS_CMD_EXEC_AT_TRG 2h Apply the current changes at DDS trigger. DDS trigger can be card trigger or DDS timer.
SPCM_DDS_CMD_EXEC_NOW 4h Apply the current changes immediately.
SPCM_DDS_CMD_WRITE_TO_CARD 8h The driver accumulates all DDS internally settings. This command transfers them to the card.
SPCM_DDS_CMD_NO_NOP_FILL 10h Special flag for DMA mode, please see separate chapter.
Before starting the DDS output, the channels need to be enabled and output level needs to be defined and finally the card itself needs to be
started using the start and enable trigger command. This is necessary to write all general settings to the hardware and to initialize trigger
and timing engine. Without the card start command, timing is unpredictable.
The first settings written to the DDS engine are written directly to the shadow register, and should contain the most important settings (espe-
cially the DDS trigger source), see below for an example starting sequence.
spcm_dwSetParam_i32 (hCard, SPC_CHENABLE, CHANNEL0); // enable channel 0
spcm_dwSetParam_i32 (hCard, SPC_ENABLEOUT0, 1); // output enabled (default)
spcm_dwSetParam_i32 (hCard, SPC_AMP0, 1000); // output amplitude 1000 mV
spcm_dwSetParam_i32 (hCard, SPC_FILTER0, 0); // full bandwidth, no filter
spcm_dwSetParam_i32 (hCard, SPC_CARDMODE, SPC_REP_STD_DDS); // DDS mode
spcm_dwSetParam_i32 (hCard, SPC_CLOCKMODE, SPC_CM_INTPLL); // clock mode internal PLL
// card start, writing of all paramters
spcm_dwSetParam_i32 (hCard, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER);
// write DDS setup for first sine signal
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP, 1); // 100% amplitude
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(100)); // 100 MHz sine signal
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG); // execute at trigger
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD);// write all dds settings to card
// start the output by initiating a force trigger
spcm_dwSetParam_i32 (hCard, SPC_M2CMD, M2CMD_CARD_FORCETRIGGER);
Internal trigger sources
There are three different internal trigger sources, that can be set with the register entry SPC_DDS_TRG_SRC. The DDS can be triggered by
an internal timer or use the card's trigger event.:
Table 150: Spectrum API: DDS trigger sources
Register Value Direction Description
SPC_DDS_TRG_SRC 608000l write Register for DDS-specific trigger condition.
SPCM_DDS_TRG_SRC_NONE 0h No special DDS trigger is used. To change a setting SPCM_DDS_CMD_EXEC_NOW needs to be used (see above).
SPCM_DDS_TRG_SRC_TIMER 1h The firmware itself generates a repeating trigger event. The triggers are generated on a timed grid with a period that
can be set by the register SPC_DDS_TRG_TIMER.
SPCM_DDS_TRG_SRC_CARD 2h The trigger source uses the cards internal trigger logic. For more information, see the product's user manual on how to
set up the different trigger conditions. In the DDS-mode, multiple triggers are processed, with each trigger received
advancing to the next command step.
The selection of a trigger source is also one of the commands that is executed on receipt of a trigger. That means, changing the trigger source
happens only when a SPCM_DDS_CMD_EXEC_AT_TRG command was sent and a next trigger/timer event was received.
(c) Spectrum Instrumentation GmbH 157

Mode DDS20 (20-tone DDS) DDS Command buffer fill size, overrun and underrun
DDS status register
The status register reports whether the DDS command queue is waiting for a trigger or has missed a trigger:
Table 151: Spectrum API: DDS trigger status register
Register Value Direction Description
SPC_DDS_STATUS 608004l read Register for reading out the DDS status.
SPCM_DDS_STAT_WAITING_FOR_TRG 1h DDS is waiting for a trigger event.
SPCM_DDS_STAT_QUEUE_UNDERRUN 2h The DDS detected a trigger or timer event prior to receiving the EXEC_AT_TRG command. The trigger source was set
to SRC_TIMER or SRC_CARD and a trigger or timer event was detected but the finalizing command EXEC_AT_TRG
was not sent in time.
SPCM_DDS_STAT_QUEUE_OVERRUN 4h User tried to add an additional command to the DDS command queue, although the queue is already filled.
General DDS information register
These registers can be used to generalize software. Using these information registers, one can read out the different available settings. This
could be helpful to utilize software to different hardware versions or firmware release versions:
Table 152: Spectrum API: DDS information registers
Register Value Direction Description
SPC_DDS_QUEUE_CMD_MAX 608005l read Length of the DDS command queue. Read-only.
SPC_DDS_QUEUE_CMD_COUNT 608006l read Current number of entries in the DDS command queue. Read-only.
SPC_DDS_NUM_CORES 608007l read Number of available DDS cores. Depends on the specific card. Read-only.
SPC_DDS_NUM_QUEUED_CMD_IN_SW 608011l read Returns the number of commands that are currently in the command queue in the library and not yet
written to the card. Writing to the card is done with the SPCM_DDS_CMD_WRITE_TO_CARD com-
mand.
SPC_DDS_TRG_COUNT 608013l read Returns the number of trigger events that have been received since the last SPCM_DDS_CMD_RESET.
A trigger event is any event that executes a DDS command, including external triggers, the timer or
manual events (SPCM_DDS_CMD_EXEC_NOW).
Trigger timer
The internal trigger timer defines an interval after which the DDS section automatically generates a trigger signal to advance to the next se-
quence of DDS settings. Please note that this interval needs to be long enough to allow to execute all DDS settings.
Only used if SPC_DDS_TRG_SRC is set to SPCM_DDS_TRG_SRC_TIMER.
Table 153: Spectrum API: DDS trigger timer
Register Value Direction Description
SPC_DDS_TRG_TIMER 608001l read/write The interval of the timer can be set in seconds or fraction of seconds as double value using the
spcm_dwSetParam_d64 function. The minimum value, maximum value and resolution of the timer
depends on an internal system clock which is a fraction of the output clock.
Each timer interval corresponds to a number of output clocks depending on the platform and type of product used. The driver finds the nearest
matching timer interval. It is therefore recommended to read-back the timer to determine the exact programmed timer value. The exact timer
intervals for a specific card, can be read out or found in the technical data section.
As an example, for a timer with a timer resolution of 6.4ns: writing a 0.00001s (10us) time to the SPC_DDS_TRG_TIMER register, the driver
then sets the DDS timer to precisely 0.0000100032s (10.0032us or 1563x6.4ns). The following example can be used to get the exact
timer:
double dTimer_s = 0;
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.00001);
spcm_dwGetParam_d64 (hCard, SPC_DDS_TRG_TIMER, &dTimer_s);
printf ("Programmed Trigger Timer: %lf s\n", dTimer_s);
(c) Spectrum Instrumentation GmbH 158

Mode DDS20 (20-tone DDS) DDS Command buffer fill size, overrun and underrun
DDS20 core connections
The DDS_CORE_ON_CHx registers defines the setup of
the different multiplexers that form the connections of the
different DDS cores on hardware channels. If a hard-
ware channel is not present on your specific hardware,
programming this connection does not have any func-
tion. In that case, dedicated DDS cores that are routed
to a fixed hardware channel (like Core 22 being solely
connected to hardware channel 3) cannot be used.
Each block represents a single DDS core or a group of
DDS cores where the following parameters can be indi-
vidually programmed:
• Amplitude
• Frequency
• Phase
• Amplitude Slope
• Frequency Slope
The cores on channel registers combine the setup of all
shown multiplexers. Each core is represented by a sin-
gle bit. All cores that should be routed to the dedicated
channel needs to be combined by a logical OR. Only
settings that match the shown possible connections in
the right-hand drawing can be selected:
Image 79: block diagram of the DDS20 option showing the DDS cores and connections options.
Table 154: Spectrum API: DDS20 connection register
| Register             | Value Direction                      | Description                                |
| -------------------- | ------------------------------------ | ------------------------------------------ |
| SPC_DDS_CORES_ON_CH0 | 608040l read/write                   | Setup of the DDS core routed to channel 0. |
| SPC_DDS_CORES_ON_CH1 | 608041l read/write                   | Setup of the DDS core routed to channel 1. |
| SPC_DDS_CORES_ON_CH2 | 608042l read/write                   | Setup of the DDS core routed to channel 2. |
| SPC_DDS_CORES_ON_CH3 | 608043l read/write                   | Setup of the DDS core routed to channel 3. |
| SPCM_DDS_CORE_NONE   | 00000h no core connected to channel  |                                            |
| SPCM_DDS_CORE0       | 000001h core 0 connected to channel  |                                            |
| SPCM_DDS_CORE1       | 000002h core 1 connected to channel  |                                            |
| SPCM_DDS_CORE2       | 000004h core 2 connected to channel  |                                            |
| SPCM_DDS_CORE3       | 000008h core 3 connected to channel  |                                            |
| SPCM_DDS_CORE4       | 000010h core 4 connected to channel  |                                            |
| ...                  | ... ...                              |                                            |
| SPCM_DDS_CORE19      | 080000h core 19 connected to channel |                                            |
| SPCM_DDS_CORE20      | 100000h core 20 connected to channel |                                            |
| SPCM_DDS_CORE21      | 200000h core 21 connected to channel |                                            |
| SPCM_DDS_CORE22      | 400000h core 22 connected to channel |                                            |
The following example for a 4-channel product connects the maximum number of cores to channel 0 and the minimum number of cores to
channel 1, 2 and 3. Please note that we mix numbers and constants here to keep the example readable.
This setup is also the default setup after reset:
// connect the maximum number of 20 cores to channel 0 and the minimum to channel 1, 2 and 3
spcm_dwSetParam_i64 (hCard, SPC_DDS_CORES_ON_CH0, 0x0FFFFF);
spcm_dwSetParam_i64 (hCard, SPC_DDS_CORES_ON_CH1, SPCM_DDS_CORE20);
spcm_dwSetParam_i64 (hCard, SPC_DDS_CORES_ON_CH2, SPCM_DDS_CORE21);
spcm_dwSetParam_i64 (hCard, SPC_DDS_CORES_ON_CH3, SPCM_DDS_CORE22);

(c) Spectrum Instrumentation GmbH 159

Mode DDS20 (20-tone DDS) Programming the DDS cores
Programming the DDS cores
The DDS cores are individually pro-
grammed. The phase and frequency
settings of each DDS core is determin-
ing which position of a programmed
memory look-up-table (LUT) is used for
this DDS output.
The amplitude value is used to attenu-
ate the output of the DDS core before
addition of multiple cores takes place.
All below mentioned settings can be
programmed in parallel on each core.
There is no specific “mode” register for
the core. It is recommend to use the API
functions writing and reading “dou-
ble” values to program these settings:
Image 81: Details of the DDS core
uint32 _stdcall spcm_dwSetParam_d64 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be modified
double dValue); // the value to be set
uint32 _stdcall spcm_dwGetParam_d64 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be read
double* dValue); // pointer to the return value
Frequency
The frequency settings are done in Hertz. The driver re-calculates the given frequency to the frequency best matching by taking the output
rate and the restrictions of the DDS-core internal settings. It is advised to read the register after writing to determine which exact frequency
has been calculated. Please also take into account the “Output Filters” of the front-end section. While large frequencies can be programmed
for the DDS core, the output filter may fully filter them off the signal if they’re above the filter frequency.
Table 155: Spectrum API: DDS core frequency settings
Register Value Direction Type Description
SPC_DDS_CORE0_FREQ 603000l read/write double Set up the frequency of DDS core 0 in Hertz. Read back to get the exact frequency set-
tings. The setting can be programmed in the range from 0 Hz to [output rate] where fre-
quencies above [output rate/2] are mirrored at the [output rate/2] and 180° phase
shifted.
SPC_DDS_CORE1_FREQ 603001l ... double ... DDS core 1...
... ... ... ... ...
The available programmable frequency minimum, maximum and step size can be read from the library:
Table 156: Spectrum API: DDS programmable frequency range
Register Value Direction Type Description
SPC_DDS_AVAIL_FREQ_MIN 608500l read double Minimum programmable frequency in Hz.
SPC_DDS_AVAIL_FREQ_MAX 608501l read double Maximum programmable frequency in Hz.
SPC_DDS_AVAIL_FREQ_STEP 608502l read double Step size of frequency programming in Hz.
Amplitude
The amplitude of the DDS core is programmed as a fraction of the output level. Please check the chapter “Setting up the outputs” in the
“Analog Outputs” section of the manual to see the different settings of the output amplifier. Writing a “1” in this register uses the full pro-
grammed output level of the output amplifier. Using multiple DDS cores that are accumulated, it may be a good idea to attenuate each DDS
core to avoid overranging the used DAC. If the sum of all DDS core outputs exceeds the “1” at any time, the DAC will go into overflow and
the output signal is clipped. This leads to heavy distortion in the signal.
Table 157: Spectrum API: DDS core amplitude settings
Register Value Direction Type Description
SPC_DDS_CORE0_AMP 605000l read/write double Set up the relative amplitude of DDS core 0 on a scale between -1.0 and +1.0. Some
examples:
0.0: no output at all.
1.0: full amplitude output.
-0.5: inverted output with half of maximum.
(c) Spectrum Instrumentation GmbH 160

Mode DDS20 (20-tone DDS) Programming the DDS cores
Table 157: Spectrum API: DDS core amplitude settings
Register Value Direction Type Description
SPC_DDS_CORE1_AMP 605001l ... double ... DDS core 1...
... ... ... ... ...
The available programmable amplitude minimum, maximum and step size can be read from the library:
Table 158: Spectrum API: DDS programmable amplitude range
Register Value Direction Type Description
SPC_DDS_AVAIL_AMP_MIN 608506l read double Minimum programmable amplitude.
SPC_DDS_AVAIL_AMP_MAX 608507l read double Maximum programmable amplitude.
SPC_DDS_AVAIL_AMP_STEP 608508l read double Step size of programmable amplitude.
Using multiple DDS cores that are accumulated and/or using amplitude ramping, it is advised to ensure that
the sum all DDS core outputs routed to one hardware channel does not exceed the valid range (-1.0...+1.0)
at any time (including amplitude ramping) to prevent going into DAC overflow resulting in heavy distortions.
Phase
The phase for each DDS core in relation to other DDS cores is set in degree. Valid values range from 0° to +360° (other values are allowed
and are mapped to this range). See the technical data sheet for phase resolution of your specific device.
Table 159: Spectrum API: DDS core phase settings
Register Value Direction Type Description
SPC_DDS_CORE0_PHASE 607000l read/write double Set up the phase of the DDS core 0 in degrees.
SPC_DDS_CORE1_PHASE 607001l ... double ... DDS core 1...
... ... ... ... ...
The available programmable phase minimum, maximum and step size can be read from the library:
Table 160: Spectrum API: DDS programmable phase range
Register Value Direction Type Description
SPC_DDS_AVAIL_PHASE_MIN 608512l read double Minimum programmable phase in degrees.
SPC_DDS_AVAIL_PHASE_MAX 608513l read double Maximum programmable phase in degrees.
SPC_DDS_AVAIL_PHASE_STEP 608514l read double Step size of programmable phase in degrees.
Simple example for fixed frequency output
The following example programs core 0 for a 50 MHz output, with full amplitude and 90° phase shift. The setup is written immediately without
waiting for a trigger or a timer:
// ... card initialisation and output amplifier settings
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP, 1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_PHASE, 90);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(50));
// read back the exact signal frequency
double dFreq_Hz = 0;
spcm_dwGetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, &dFreq_Hz);
printf ("Generated signal frequency: %lf Hz\n", dFreq_Hz);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_NOW);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD);
(c) Spectrum Instrumentation GmbH 161

Mode DDS20 (20-tone DDS) Programming the DDS cores
Slope update rate
Slopes are programmed with the below mentioned registers. The slope functionality automatically adds a calculated increment or decrement
to either amplitude or frequency or both. The time resolution for the increment/decrement is the same as the DDS update rate. That means
that the slope functionality smoothly changes amplitude and/or frequency every DDS update interval. There are no jumps in the slope func-
tionality.
Frequency Slope
The frequency slope settings are done in Hertz per second, meaning how much change of frequency should be done within one second. The
figure can be entered as positive (increasing frequency value) or negative (decreasing frequency value) value. The driver re-calculates the
given frequency slope to the frequency slope best matching by taking the output rate, ramp step size and the restrictions of the DDS-core
internal settings into account. It is advised to read the register after writing to determine which exact frequency slope has been calculated.
Table 161: Spectrum API: DDS core frequency slope settings
Register Value Direction Type Description
SPC_DDS_CORE0_FREQ_SLOPE 604000l read/write double Set up the frequency slope of DDS core 0 in Hertz per second.
SPC_DDS_CORE1_FREQ_SLOPE 604001l ... double ... DDS core 1...
... ... ... ... ...
The available programmable frequency slope minimum, maximum and step size can be read from the library:
Table 162: Spectrum API: DDS programmable frequency slope range
Register Value Direction Type Description
SPC_DDS_AVAIL_FREQ_SLOPE_MIN 608503l read double Minimum programmable frequency slope in Hertz per second.
SPC_DDS_AVAIL_FREQ_SLOPE_MAX 608504l read double Maximum programmable frequency slope in Hertz per second.
SPC_DDS_AVAIL_FREQ_SLOPE_STEP 608505l read double Step size of programmable frequency slope in Hertz per second.
Example for Frequency Slope
The following example should generate a 110 MHz signal for 100 ms, do a ramp from 110 MHz to 120 MHz within the next 100 ms and
then keep the 120 MHz until stopped.
// ... card initialisation and output amplifier settings
// set 100 ms timer and execute steps on timer
spcm_dwSetParam_i32 (hCard, SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_TIMER);
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.1);
// Initial 110 MHz frequency
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP, 1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_PHASE, 0);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(110));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// slope from 110 MHz to 120 MHz (10 MHz change in 100 ms = 100 MHz change in 1 second)
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, MEGA(100));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// Final 120 MHz frequency
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(120));
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, 0);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD);
Amplitude Slope
The amplitude slope settings are done in level of output amplitude per second, meaning how much change of amplitude should be done
within one second. The figure can be entered as positive (increasing amplitude) or negative (decreasing amplitude) value. The driver re-cal-
culates the given amplitude slope to the amplitude slope best matching by taking the output rate, ramp step size and the restrictions of the
DDS-core internal settings into account. It is advised to read the register after writing to determine which exact amplitude slope has been
calculated. The amplitude slope is only available for dedicated DDS cores.
Table 163: Spectrum API: DDS core amplitude slope settings
Register Value Direction Type Description
SPC_DDS_CORE0_AMP_SLOPE 606000l read/write double Set up the amplitude slope of DDS core 0 in level of amplitude per second
SPC_DDS_CORE1_AMP_SLOPE 606001l ... double ... DDS core 1...
... ... ... ... ...
(c) Spectrum Instrumentation GmbH 162

Mode DDS20 (20-tone DDS) Programming the DDS cores
The available programmable amplitude slope minimum, maximum and step size can be read from the library:
Table 164: Spectrum API: DDS programmable amplitude slope range
Register Value Direction Type Description
SPC_DDS_AVAIL_AMP_SLOPE_MIN 608509l read double Minimum programmable amplitude slope in level of amplitude per second.
SPC_DDS_AVAIL_AMP_SLOPE_MAX 608510l read double Maximum programmable amplitude slope in level of amplitude per second.
SPC_DDS_AVAIL_AMP_SLOPE_STEP 608511l read double Step size of programmable amplitude slope in level of amplitude per second.
Using multiple DDS cores that are accumulated and/or use amplitude ramping, it is advised to ensure that the sum all DDS core
outputs routed to one hardware channel does not exceed the valid range (1.0...+1.0) at any time (including amplitude ramping)
to prevent going into DAC overflow resulting in heavy distortions.
Example for Amplitude Slope
The following example generates a 120 MHz signal which is ramped up for 100 ms, run at full amplitude for 500 ms and ramped down
again within 100 ms.
// ... card initialisation and output amplifier settings
// execute steps on timer
spcm_dwSetParam_i32 (hCard, SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_TIMER);
// slope from zero to full amplitude in 100 ms (+1.0 change in 100 ms = +10.0 change in 1 second)
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP, 0);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_PHASE, 0);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(120));
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP_SLOPE, +10.0);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// keep amplitude for 500 ms
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.5);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP, 1.0);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP_SLOPE, 0.0);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// slope from full amplitude to zero in 100 ms (-1.0 change in 100 ms = -10.0 change in 1 second)
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP_SLOPE, -10.0);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD);
Modifying step size of slope
For very slow slopes of frequency or amplitude, the resolution of the internal firmware registers may not be large enough. For these cases,
there are two additional global step size registers which allow to work with even smaller slopes:
Table 165: Spectrum API: DDS core slope step size settings
Register Value Direction Type Description
SPC_DDS_FREQ_RAMP_STEPSIZE 608008l read/write double Valid range: 1 to 4096: The step size setting increases the number of clock cycles
between two changes and thus allows smaller frequency changes during ramps.
SPC_DDS_AMP_RAMP_STEPSIZE 608009l read/write double Valid range: 1 to 4096: The step size setting increases the number of clock cycles
between two changes and thus allows smaller amplitude changes during ramps.
(c) Spectrum Instrumentation GmbH 163

Mode DDS20 (20-tone DDS) Defining the Phase Behaviour
Defining the Phase Behaviour
The phase behaviour defines how the sine phase behaves with the next trigger event. There are two different modes available that define the
behaviour for all cores:
Table 166: Spectrum API: DDS Phase Behaviour
Register Value Direction Description
SPC_DDS_PHASE_BEHAVIOUR 608002l read/write Setup of the phase behaviour. One of the below settings is available.
SPCM_DDS_PHASE_JUMP 0 phase jumps to the next phase value at the next trigger.
SPCM_DDS_PHASE_SHIFT 1 phase value is added to the phase counter output.
Mode Phase Jump
The phase jumps to the defined phase value at the next trig-
ger or timer. The phase counter is reset and the prior phase
is irrelevant. This can be used for phase coherent frequency
multiplexing.
The example on the right shows a phase jump to 90°phase
when the 2nd trigger signal arrives. Below are the com-
mands that are written to the library to receive this behav-
iour.
Command List
// 1. Sequence
SPC_DDS_PHASE_BEHAVIOUR: SPCM_DDS_PHASE_JUMP
SPC_DDS_CORE0_PHASE: 0°
SPC_DDS_CORE0_AMP: 1.0
SPC_DDS_CORE0_FREQ: 1 Hz
SPC_DDS_CMD: SPCM_DDS_CMD_EXEC_AT_TRG
// 2. Sequence
SPC_DDS_CORE0_PHASE: 90°
SPC_DDS_CMD: SPCM_DDS_CMD_EXEC_AT_TRG
Image 82: Phase Jumps to 90° instantly when the 2nd trigger arrives, regardless of prior state.
Mode Phase Shift
The phase shifts to a new phase by adding the phase value
to the current phase counter at the moment of the trigger
event. Advantage here is that the phase relations are guar-
anteed even if the trigger is not 100% time synchronized.
This mode can be used for phase modulation and phase
shifts between hardware channels during runtime.
The example in the right shows a phase shift. The signal
phase of the first signal part is 180°. At the 2nd trigger the
signal shifts to a signal phase of 270°. This phase is in rela-
tion to the original phase, thus the shift of phase is 270°-180°
= 90°. At the position of the 2nd trigger the signal therefore
moves from 270° phase to 270° + 90° = 0° phase. The dot-
ted blue signal shows the phase counter of the second signal
part how it would have been if started with that phase imme-
diately at the beginning of the output.
Command List
// 1. Sequence
SPC_DDS_PHASE_BEHAVIOUR: SPCM_DDS_PHASE_SHIFT
SPC_DDS_CORE0_PHASE: 180°
SPC_DDS_CORE0_AMP: 1.0
SPC_DDS_CORE0_FREQ: 1 Hz
SPC_DDS_CMD: SPCM_DDS_CMD_EXEC_AT_TRG
// 2. Sequence
Image 83: Phase shifts forward by 90° relative to current state at 2nd Trigger
SPC_DDS_CORE0_PHASE: 270°
SPC_DDS_CMD: SPCM_DDS_CMD_EXEC_AT_TRG
(c) Spectrum Instrumentation GmbH 164

Mode DDS20 (20-tone DDS) Additional functions in DDS core
Additional functions in DDS core
Using the multi-purpose I/O lines within the DDS core command structure allows to manipulate external equipment within the exact timing of
the DDS functionality. Please note that the general SPCM_Xx_MODE registers need to be set to SPCM_MODE_DDS first to be under control
of the DDS functionality.
Table 167: Spectrum API: multi-purpose I/O lines registers and available register settings
Register Value Direction Description
SPCM_X0_AVAILMODES 47210 read Bit mask with all bits of the below mentioned modes showing the available modes for (X0).
SPCM_X1_AVAILMODES 47211 read Bit mask with all bits of the below mentioned modes showing the available modes for (X1).
SPCM_X2_AVAILMODES 47212 read Bit mask with all bits of the below mentioned modes showing the available modes for (X2).
SPCM_X0_MODE 47200 read/write Defines the mode for (X0). Only one mode selection is possible to be set at a time.
SPCM_X1_MODE 47201 read/write Defines the mode for (X1). Only one mode selection is possible to be set at a time.
SPCM_X2_MODE 47202 read/write Defines the mode for (X2). Only one mode selection is possible to be set at a time.
SPCM_XMODE_DDS 00000004h Switch the output to DDS mode.
Once switched to DDS mode, you can use an additional DDS-MODE register, specialized to DDS functionality:
Table 168: Spectrum API: DDS multi-purpose I/O additional registers
Register Value Direction Description
SPC_DDS_X0_MODE 608011 read/write After setting SPCM_X0_MODE to SPCM_XMODE_DDS this register defines the DDS-specific usage of
the multi purpose line X0.
SPC_DDS_X1_MODE 608012 read/write After setting SPCM_X1_MODE to SPCM_XMODE_DDS this register defines the DDS-specific usage of
the multi purpose line X1.
SPC_DDS_X2_MODE 608013 read/write After setting SPCM_X2_MODE to SPCM_XMODE_DDS this register defines the DDS-specific usage of
the multi purpose line X2.
SPCM_DDS_XMODE_MANUAL 1 Set the XIO-line to High or low with a software command SPC_DDS_X_MANUAL_OUTPUT. This command is part of
the DDS command queue and executed following the DDS trigger and timer settings in parallel to changes of the DDS
core.
SPCM_DDS_XMODE_WAITING_FOR_TRG 2 Generates a high signal on the multi-purpose output as soon as the DDS engine is waiting for the next trigger event
(DDS engine is armed for trigger). This mode can be used to interact with external hardware and issue changes and
the next trigger event.
SPCM_DDS_XMODE_EXEC 3 Generates a high pulse on the multi-purpose output with the length of DDS timer resolution when a trigger or timer
event occurs. This output shows the loading of a new set of DDS parameters from shadow registers to DDS registers.
The additional register below defines the manual output if the DDS_XMODE of that specific output is set to manual:
Table 169: Spectrum API: DDS multi-purpose I/O manual output register
Register Value Direction Description
SPC_DDS_X_MANUAL_OUTPUT 608014 write If SPCM_DDS_XMODE_MANUAL is used this register sets the XIO line to high if the corresponding
bit is set.
SPCM_DDS_X0 1h Sets output X0 to high.
SPCM_DDS_X1 2h Sets output X1 to high.
SPCM_DDS_X2 4h Sets output X2 to high.
Example for multi-purpose DDS output
The following example should generate constantly a 110 MHz signal. After 100 ms, it switches X1 to high for another 200 ms and switches
back to 0.
// ... card initialisation and X-mode settings
// set XIO 0 to DDS, manual output mode and execute steps on timer
spcm_dwSetParam_i32 (hCard, SPC_DDS_TRG_SRC, SPCM_DDS_TRG_SRC_TIMER);
spcm_dwSetParam_i32 (hCard, SPCM_X0_MODE, SPCM_XMODE_DDS);
spcm_dwSetParam_i32 (hCard, SPC_DDS_X0_MODE, SPCM_DDS_XMODE_MANUAL);
// Initial 110 MHz frequency for 100 ms
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP, 1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(110));
spcm_dwSetParam_i32 (hCard, SPC_DDS_X_MANUAL, 0);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// set X0 to high for 200 ms
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.2);
spcm_dwSetParam_i32 (hCard, SPC_DDS_X_MANUAL, SPCM_DDS_X1);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// and back to zero
spcm_dwSetParam_i32 (hCard, SPC_DDS_X_MANUAL, 0);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD);
(c) Spectrum Instrumentation GmbH 165

Mode DDS20 (20-tone DDS) Improving DDS throughput by writing commands as parameter lists
Improving DDS throughput by writing commands as parameter lists
All above examples have show how to program parameters with single commands. This is an easy to understand approach which can be
extended at any time by new commands. However, this approach executes one library call for each single command. Each library call has
some internal overhead for the call itself, for handle routing, for error checking and for miscellaneous others steps that are necessarily done
internally. That overhead adds up to some latency for every call, limiting the throughput when sending commands to the library. This is espe-
cially limiting, when a lot of commands should be send in a short time with the intention to get a fast update rate.
To enhance the command throughput, one can alternatively send a number of commands as a parameter array with a single library call using
the dwSetParam_ptr function (introduced with library V7.x). The following register can be used to send the array of structs to the driver:
Table 170: Spectrum API: register lists register
Register Value Direction Type Description
SPC_REGISTER_LIST 120 write ptr provides an interface for sending multiple register write calls through one call of the driver
API, hence reducing the number of calls to the API and reducing the time it takes to send
many register calls.
For simplifying the handling the driver provides the definition of a data structure, that must be used for providing commands as a list:
// --- define structure for accessing the driver registers by means of
// a list of values via the dwSetParam_ptr() function.
#define TYPE_INT64 0
#define TYPE_DOUBLE 1
typedef struct
{
int32 lReg; // driver register as defined in regs.h
int32 lType; // as defined above: TYPE_INT64 or TYPE_DOUBLE
// one single 64bit value, that will either be interpreted as INT64 or DOUBLE
union
{
double dValue; // MUST be used in conjunction with TYPE_DOUBLE
int64 llValue; // MUST be used in conjunction with TYPE_INT64
};
} ST_LIST_PARAM;
The structure consists of three entries:
• int32 lReg: a 32bit value containing the driver register name.
• int32 lType: this is a type specifier, so that the driver will know how to interpret the third parameter.
• The value to be written to the register specified by lReg. This value is a 64bit value that will be interpreted as either an 64bit integer value
or as a 64bit double value depending on the lType specifier and must be accessed when creating the list accordingly:
• lType = TYPE_DOUBLE: the content must be written to dValue.
• lType = TYPE_INT64: the content must be written to llValue.
Below are two examples showing the two different approaches when writing some DDS commands with either method.
Single Commands Example (6 library calls)
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP, 0);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_PHASE, 0);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(120));
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP_SLOPE, +10.0);
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
Parameter Array Example (1 library call)
// setting up the list
ST_LIST_PARAM astList[6];
astList[0].lReg = SPC_DDS_TRG_TIMER; astList[0].lType = TYPE_DOUBLE; astList[0].dValue = 0.1;
astList[1].lReg = SPC_DDS_CORE0_AMP; astList[1].lType = TYPE_DOUBLE; astList[1].dValue = 0.0;
astList[2].lReg = SPC_DDS_CORE0_PHASE; astList[2].lType = TYPE_DOUBLE; astList[2].dValue = 0.0;
astList[3].lReg = SPC_DDS_CORE0_FREQ; astList[3].lType = TYPE_DOUBLE; astList[3].dValue = MEGA(120);
astList[4].lReg = SPC_DDS_CORE0_AMP_SLOPE; astList[4].lType = TYPE_DOUBLE; astList[4].dValue = +10.0;
astList[5].lReg = SPC_DDS_CMD; astList[5].lType = TYPE_INT64; astList[5].llValue =
SPCM_DDS_CMD_EXEC_AT_TRG;
// and transferring the list with one single library call
spcm_dwSetParam_ptr (hCard, SPC_REGISTER_LIST, astList, 6 * sizeof(ST_LIST_PARAM));
(c) Spectrum Instrumentation GmbH 166

Mode DDS20 (20-tone DDS) Defining the data transfer mode
Defining the data transfer mode
By the default, the DDS firmware uses the data transfer mode Single, which is described in detail in the section "DDS Command buffer fill
size, overrun and underrun" earlier in this chapter.
There are, however, two different modes to transfer data from software buffer to hardware buffer. In the following table, the register to switch
between these two modes and their register values are listed:
Table 171: Spectrum API: data transfer mode definition
Register Value Direction Description
SPC_DDS_DATA_TRANSFER_MODE 608012l read/write defining the data transfer mode that is performed when executing the SPCM_DDS_CM-
D_WRITE_TO_CARD command.
SPCM_DDS_DTM_SINGLE 0 commands are transferred by single calls from library buffer to card.
SPCM_DDS_DTM_DMA 1 commands are transferred by DMA from library buffer to card.
The other data transfer mode is the DMA transfer mode. This transfer mode uses the on-board memory of the card as an additional FIFO
buffer between the software FIFO and the “DDS command FIFO” buffer.
As mentioned before, all transfers start as soon as a SPCM_DDS_CMD_WRITE_TO_CARD command is issued. If DTM_SINGLE mode is se-
lected, the driver transfers single commands from the software buffer to the “DDS command FIFO” buffer. If DTM_DMA is selected, the com-
mands are transferred using DMA into the “HW Data FIFO Buffer” command queue that is set up on the on-board card memory. This on-
board FIFO queue automatically fills the “DDS command FIFO” command buffer.
When the card is running in DDS mode, all the DDS com-
mands that are executed by the user are added to the
Software FIFO. When the command
SPCM_DDS_CMD_WRITE_TO_CARD is written to the
register SPC_DDS_CMD, the Software FIFO is trans-
ferred to either the DDS command FIFO
(in SPC_DDS_DTM_SINGLE mode) or the “HW Data
FIFO Buffer” through DMA (in SPC_DDS_DTM_DMA
mode).
In DMA mode, the commands in the “HW Data FIFO
Buffer” are automatically transferred to the DDS com-
mand FIFO.
In both modes, the commands in the DDS command FIFO
are transferred into the DDS shadow registers until a
SPC_DDS_CMD_EXEC_AT_TRG command is received.
The transfer is put on hold until a card- or timer trigger is
received.
Using DMA transfer, transferred data requires a mini-
mum size of the Notifysize. If the “Software FIFO” con-
tains less data than the programmed Notifysize and the
SPCM_DDS_CMD_WRITE_TO_CARD is issued, the driv-
er will automatically insert NOP (no operation = dummy) Image 84: The different FIFO buffers in DDS mode
commands to allow the DMA transfer to start. Note that
these NOP commands are filtered out by the DDS, which
may produce a delay when the subsequent DDS commands reach the "DDS command FIFO". For this reason, it is not recommended to use
DMA mode if you intend to regularly write a small number of commands to the card. DMA should be used when writing a large number of
commands or when you only intend to send all needed commands once.
This automatic insertion of NOP commands can be suppressed by combining the write to card command with the no-nop flag:
(SPCM_DDS_CMD_WRITE_TO_CARD | SPCM_DDS_CMD_NO_NOP_FILL), with the result that the DMA transfer is delayed, until at least
NotifySize commands are available within the “Software FIFO”.
When a card-trigger, timer-trigger or SPC_DDS_CMD_EXEC_NOW command is received, all parameters in the shadow register are trans-
ferred to the DDS registers and the card will start to output the signal generated by the DDS cores with the new parameters.
Table 172: Comparison of the different DDS FIFOs and their fillsize registers
# FIFO buffer Get the fillsize of the FIFO via Max. number of commands Where used
1 Software FIFO SPC_DDS_NUM_QUEUED_CMD_IN_SW (dynamic) Single and DMA mode
2 HW Data FIFO SPC_FILLSIZEPROMILLE (unavailable) DMA mode only
3 DDS command FIFO SPC_DDS_QUEUE_CMD_COUNT SPC_DDS_QUEUE_CMD_MAX Single and DMA mode
(c) Spectrum Instrumentation GmbH 167

Mode DDS20 (20-tone DDS) Execute Now, Timer and Trigger timing behaviour
Below is a comparison of the two different modes in terms of advantages and disadvantages:
Table 173: Comparison of single and DMA transfer mode
DTM_SINGLE DTM_DMA
Min. user software to analog output latency 10 us 20 us
Max continuous command rate 400 kHz 10 MHz
On-board command buffer 4Ki commands 512Mi commands
CPU load High Low
Combining the parameter array function with the DMA transfer mode allows to transfer DDS commands with a very high speed, making
advanced waveforms available. Using this mode, it is easy to perform different kinds of modulation or different slope shapes like s-form shape.
Please see the examples below.
Example of S-shape frequency slope
The built-in slope function is a linear ramp and is performed by a single command. Other slopes can be performed using multiple linear ramps.
Below is an example of a s-shape slope using 7 single linear ramps:
// basic setup of starting frequncy 100 MHz, full amplitude with a timing of 100 ms
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_SOURCE, SPCM_DDS_TRG_SRC_TIMER);
spcm_dwSetParam_d64 (hCard, SPC_DDS_TRG_TIMER, 0.1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_AMP, 1);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_PHASE, 0);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(100));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// s-form slope in 7 steps (700 ms)
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, +MEGA(10));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, +MEGA(20));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, +MEGA(40));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, +MEGA(80));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, +MEGA(40));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, +MEGA(20));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, +MEGA(10));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// final frequency 122 MHz
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ_SLOPE, 0);
spcm_dwSetParam_d64 (hCard, SPC_DDS_CORE0_FREQ, MEGA(122));
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_EXEC_AT_TRG);
// writing the whole setup to the card
spcm_dwSetParam_i32 (hCard, SPC_DDS_CMD, SPCM_DDS_CMD_WRITE_TO_CARD);
Execute Now, Timer and Trigger timing behaviour
This chapter explains the timing behaviour of the three different command to change the output. It is possible to mix Execute Now
(SPCM_DDS_CMD_EXEC_NOW) with trigger (SPCM_DDS_CMD_EXEC_AT_TRG) and also with timer but depending on the combination
there are some additional time delays:
• If an SPCM_DDS_CMD_EXEC_NOW command is queued directly after a SPCM_DDS_CMD_EXEC_AT_TRG, there is a time delay of r8
times the DDS resolution of between the trigger event and the execute of the command(s). With n example DDS resolution of 6.4 ns this
equals roughly 51 ns.
• If SPCM_DDS_CMD_EXEC_NOW commands are queued directly after each other, the delay between the command execution is one
time interval of the DDS resolution.
• If the SPCM_DDS_CMD_EXEC_NOW command is sent from software together with a SPCM_DDS_CMD_WRITE_TO_CARD command
with the command FIFO being empty, there is a highly operating system depending delay until the output is changed. The delay is in the
region of a few micro seconds up to milliseconds.
• Using a timer together with a SPCM_DDS_CMD_EXEC_NOW resets the timer to zero at the execution time of the command.
(c) Spectrum Instrumentation GmbH 168

Mode DDS50 (50-tone DDS) General Information
