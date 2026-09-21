Pulse Generator (Firmware Option)
General Information
The pulse generator module provides a versatile timing synchroniza-
tion interface between the acquisition/replay functionality of the card
and external equipment.
The module consists of four pulse generators, where each generator
allows for (in)dependent generation of individual pulses, pulse trains
or a continuous stream of pulses that can be output on a Multi-Pur-
pose I/O Line, greatly enhancing the versatility of the XIO lines.
The versatile trigger capabilities allow for external or internal trigger-
ing. Moreover, the pulse generators can trigger each other, hence al-
lowing for cascading of up to four pulse repetition time scales.
The outputs of the pulse generators are intrinsically synchronized to
the card acquisition/replay functionality and its sampling clock,
Image 93: overview block diagram of multi-purpose I/O lines and pulse generators
hence allowing for reproducible enabling or switching of external
signals (e.g., for signal actuating). Other use cases might be pulse
broadening, pulse delaying, or just pulse generation.
The generation of the pulse trains and timing signals is performed inside the FPGA of the card and is working in parallel to any other func-
tionality of the card (such as data acquisition or replay), and hence not reducing the performance.
Feature Overview
• Four pulse generators are available
• Single-shot, multiple repetitions or continuous/infinite repetition of pulses
• Individual control of pulse length/duty cycle
• External or internal triggering/starting individually for each pulse generator
• Individual trigger delay per pulse generator allowing for phase shifting
• Internal cascading of pulse generators possible allowing up to four repetition time scales.
The “standard” modes of the multi purpose I/O lines are still available, as described in the “Multi Purpose I/O Lines” section. This chapter
focuses on the additional functionality, available with the pulse generator firmware option installed.
The multi purpose I/O lines are available on the front plate and labelled with X0 (line 0), X1 (line 1), and X2 (line 2). As default these lines
are switched off.
As default (power-on and after reset command) the I/O capable lines are switched off and hence are not
actively driven. Hence the on-board 10 kOhm pull-up resistors are pulling these lines to logic HIGH. If a logic
LOW is required, external lower-value (1 kOhm) pull-down resistors might be used.
Please be careful when programming these lines as an output whilst maybe still being connected with an
external signal source, as that may damage components either on the external equipment or on the card
itself.
(c) Spectrum Instrumentation GmbH 183

Pulse Generator (Firmware Option) Principle of Operation
Principle of Operation
Image 94: overview block diagram of the pulse generator
All of the four available pulse generator units are identical in their feature set and individually programmable.
As shown above, each unit consists of:
• A dedicated trigger setup consisting of two multiplexers MUX1 and MUX2 combining various signals
• A programmable inverter on the output of each multiplexer
• A static logic AND gate combining the outputs of both multiplexers to form a trigger/gate for the pulse generating unit
• The pulse generating unit itself with its trigger signal driven by the AND gate
• A final programmable output inverter
The pulse generator unit is clocked with an FPGA internal clock, which is a divided version derived from the acquisition or generation sam-
pling rate. Since the division ratio is depending on the used card type, the number of active channels and the sampling rate, an dedicated
read only register allows to read out the frequency value by the following register:
Table 201: Spectrum API: pulse generator clock frequency read register
Register Value Direction Description
SPC_XIO_PULSEGEN_CLOCK 602000 read Returns the clock driving the pulse generator in Hz.
The following short excerpt shows which parameters need to be defined first and how to read out the clock rate at which the pulse generator
units then are clocked:
...
// first set up the parameters, that influence the pulse generator’s clock rate
spcm_dwSetParam_i32 (hCard, SPC_CHENABLE, CHANNEL0); // channel enable
spcm_dwSetParam_i64 (hCard, SPC_SAMPLERATE, MEGA(1)); // desired acquisition/generation sampling rate
...
// afterwards read out the divided clock rate, clocking the pulse generator units
int64 llPulseGenClock_Hz = 0;
spcm_dwGetParam_i64 (hCard, SPC_XIO_PULSEGEN_CLOCK, &llPulseGenClock_Hz);
See the end of this chapter for a more complete example setup of a pulse generator unit.
Changing the card settings while pulse generators are active will cause a stop and restart of the pulse gen-
erators automatically issued by the driver to the pulse generators.
(c) Spectrum Instrumentation GmbH 184

Pulse Generator (Firmware Option) Setting up the Pulse Generator
Setting up the Pulse Generator
Enabling, disabling and resetting a pulse generator
Each pulse generator unit can be enabled and disabled separately:
Table 202: Spectrum API: pulse generator enable registers
Register Value Direction Description
SPC_XIO_PULSEGEN_ENABLE 601500 read/write Bitmask to enable any combination of the four different pulse generators.
SPCM_PULSEGEN_ENABLE0 1h Enable pulse generator 0. When disabled, the output (prior to the output inverter) is set to logic LOW.
SPCM_PULSEGEN_ENABLE1 2h Enable pulse generator 1. When disabled, the output (prior to the output inverter) is set to logic LOW.
SPCM_PULSEGEN_ENABLE2 4h Enable pulse generator 2. When disabled, the output (prior to the output inverter) is set to logic LOW.
SPCM_PULSEGEN_ENABLE3 8h Enable pulse generator 3. When disabled, the output (prior to the output inverter) is set to logic LOW.
Disabling a unit will act as a reset dedicated to this single unit. A disabled pulse generator will output a logic LOW prior to the programmable
output inverter, hence with an active output inverter the final output of a disabled pulse generator will be logically HIGH.
Defining the basic pulse parameters
The two basic properties for generating a (repetitive) pulsed output is to define the length (or period) and define how much of the waveform
should the output be HIGH:
Image 95: timing diagram illustrating the basic pulse parameters
The pulse generator will upon start (trigger) first set the output HIGH for the programmed amount of time. Afterwards it will set the waveform
LOW for the remaining time until the programmed length (period) has been reached. As a result, the number of clock cycles during which
the output is LOW calculates to: LOW = LEN - HIGH. In the example above with LEN = 7 and HIGH = 4, the signal will be LOW for the
remaining 3 clock cycles.
The following table shows the registers required to set the total length of the pulse to be generated. The length is defined in clock cycles:
Table 203: Spectrum API: pulse generator length/period register
Register Value Direction Description
SPC_XIO_PULSEGEN_AVAILLEN_MIN 602001 read Returns the minimum length (period) of the pulse generator’s output pulses in clock cycles.
SPC_XIO_PULSEGEN_AVAILLEN_MAX 602002 read Returns the maximum length (period) of the pulse generator’s output pulses in clock cycles.
SPC_XIO_PULSEGEN_AVAILLEN_STEP 602003 read Returns the step size the pulse generator’s output pulses in clock cycles.
SPC_XIO_PULSEGEN0_LEN 601001 read/write Define the length of the pulse period generated by pulse generator 0 in clock cycles.
SPC_XIO_PULSEGEN1_LEN 601101 read/write Define the length of the pulse period generated by pulse generator 1 in clock cycles.
SPC_XIO_PULSEGEN2_LEN 601201 read/write Define the length of the pulse period generated by pulse generator 2 in clock cycles.
SPC_XIO_PULSEGEN3_LEN 601301 read/write Define the length of the pulse period generated by pulse generator 3 in clock cycles.
The second parameter that needs to be defined is the amount of clock pulses that force the output to a logic HIGH. The following table shows
the registers required to set the total length of the pulse to be generated:
Table 204: Spectrum API: pulse generator HIGH time registers
Register Value Direction Description
SPC_XIO_PULSEGEN_AVAILHIGH_MIN 602004 read Returns the minimum HIGH time of the pulse generator’s output pulses in clock cycles.
SPC_XIO_PULSEGEN_AVAILHIGH_MAX 602005 read Returns the maximum HIGH time of the pulse generator’s output pulses in clock cycles.
SPC_XIO_PULSEGEN_AVAILHIGH_STEP 602006 read Returns the step size the pulse generator’s HIGH time in clock cycles.
SPC_XIO_PULSEGEN0_HIGH 601002 read/write Define the HIGH time for the pulse generated by pulse generator 0 in clock cycles.
SPC_XIO_PULSEGEN1_HIGH 601102 read/write Define the HIGH time for the pulse generated by pulse generator 1 in clock cycles.
SPC_XIO_PULSEGEN2_HIGH 601202 read/write Define the HIGH time for the pulse generated by pulse generator 2 in clock cycles.
SPC_XIO_PULSEGEN3_HIGH 601302 read/write Define the HIGH time for the pulse generated by pulse generator 3 in clock cycles.
These two settings alone allow for the creation of periodic signals with the freely programmable duty cycle. Setting the HIGH time to half the
LEN will result is a clock-like signal with half the time being HIGH and half the time being LOW, hence having a 50% duty-cycle signal.
Since the output of the pulse generator can only change with every edge of its clock input, the speed of this clock ultimately defines the gran-
ularity at which the pulses can be configured. The lower the period of the generated pulse signal the finer this granularity becomes with
regards to the output signal frequency.
For example, when creating an output with the maximum output frequency of Clk/2 (with LEN = 2 and HIGH = 1), the only possible remaining
configuration is a duty-cycle of 50%. And with a output at frequency with Clk/3 (with LEN=3 and HIGH either 1 or 2) the duty-cycle is either
33% or 66%, but cannot be 50%.
(c) Spectrum Instrumentation GmbH 185

Pulse Generator (Firmware Option) Setting up the Pulse Generator
In addition to defining the length/period of a single pulse, one can also define how often a pulse should be replayed repeatedly. The choice
can be made between repeating the pulses infinitely (until being explicitly stopped) or to pre-define a number of repetitions:
Table 205: Spectrum API: pulse generator loops/pulse repetition registers
Register Value Direction Description
SPC_XIO_PULSEGEN_AVAILLOOPS_MIN 602010 read Returns the minimum number of times, the output of a pulse generator can be repeated.
SPC_XIO_PULSEGEN_AVAILLOOPS_MAX 602011 read Returns the maximum number of times, the output of a pulse generator can be repeated.
SPC_XIO_PULSEGEN_AVAILLOOPS_STEP 602012 read Returns the step size when defining the repetition of pulse generator’s output.
SPC_XIO_PULSEGEN0_LOOPS 601004 read/write Define the number of repetitions of the output period when triggered for pulse generator 0.
SPC_XIO_PULSEGEN1_LOOPS 601104 read/write Define the number of repetitions of the output period when triggered for pulse generator 1.
SPC_XIO_PULSEGEN2_LOOPS 601204 read/write Define the number of repetitions of the output period when triggered for pulse generator 2.
SPC_XIO_PULSEGEN3_LOOPS 601304 read/write Define the number of repetitions of the output period when triggered for pulse generator 3.
0 Upon a trigger event the output of the pulse generator will run infinitely until being disabled or reset.
1 ... [4Gi - 2] Upon a trigger event the output period will replayed the defined number of times.
Delaying (phase shifting) the Outputs
As mentioned above the pulse generator will always start with the first portion of the period to be HIGH and then will set the output LOW for
the remaining number of cycles within the chosen length.
When using the delay, it is possible to delay the initial HIGH portion of the pulse generator(s) by a defined amount of clock cycles. This in
combination with a common starting point (start/trigger) allows for the generation of phase shifted signals as shown below for two of the
pulse generators. Both are set up with identical LEN and HIGH parameters, but the additional delay for pulse generator 0 (PGen0) is kept at
the default of zero clock cycles, whilst PGen1is delayed by 5 clock cycles:
Image 96: timing diagram illustrating delaying a pulse generator output
The amount of additional delay can be set individually for each pulse generator, by using the following registers:
Table 206: Spectrum API: pulse generator delay/phase shift registers
Register Value Direction Description
SPC_XIO_PULSEGEN_AVAILDELAY_MIN 602007 read Returns the minimum delay of the pulse generator’s output in clock cycles.
SPC_XIO_PULSEGEN_AVAILDELAY_MAX 602008 read Returns the maximum delay of the pulse generator’s output in clock cycles.
SPC_XIO_PULSEGEN_AVAILDELAY_STEP 602009 read Returns the step size of the pulse generator’s output delay in clock cycles.
SPC_XIO_PULSEGEN0_DELAY 601003 read/write Define how much the output of pulse generator 0 is delayed after trigger in clock cycles.
SPC_XIO_PULSEGEN1_DELAY 601103 read/write Define how much the output of pulse generator 1 is delayed after trigger in clock cycles.
SPC_XIO_PULSEGEN2_DELAY 601203 read/write Define how much the output of pulse generator 2 is delayed after trigger in clock cycles.
SPC_XIO_PULSEGEN3_DELAY 601303 read/write Define how much the output of pulse generator 3 is delayed after trigger in clock cycles.
Defining the trigger behavior
Each pulse generator can be set up to react on its trigger input in three different ways, depending on the application’s need:
Table 207: Spectrum API: pulse generator mode registers with their available settings
Register Value Direction Description
SPC_XIO_PULSEGEN0_MODE 601000 read/write Defines the behavior of pulse generator 0 on how to react on its trigger event.
SPC_XIO_PULSEGEN1_MODE 601100 read/write Defines the behavior of pulse generator 1 on how to react on its trigger event.
SPC_XIO_PULSEGEN2_MODE 601200 read/write Defines the behavior of pulse generator 2 on how to react on its trigger event.
SPC_XIO_PULSEGEN3_MODE 601300 read/write Defines the behavior of pulse generator 3 on how to react on its trigger event.
SPCM_PULSEGEN_MODE_GATED 1 Pulse generator will start if the trigger condition or “gate” is met and will stop, if either the gate becomes inactive or
the defined number of LOOPS have been generated. Will reset its loop counter, when the gate becomes LOW.
SPCM_PULSEGEN_MODE_TRIGGERED 2 The pulse generator will start if the trigger condition is met and will replay the defined number of loops before re-arm-
ing itself and waiting for another trigger event. Changes in the trigger signal while replaying will be ignored.
SPCM_PULSEGEN_MODE_SINGLESHOT 3 The pulse generator will start if the trigger condition is met and will replay the defined number of loops once.
For simplicity, the waveforms below will show the modes principle, without any additionally programmed delay, and also omitting the intrinsic
pipeline delay from the trigger event to the output’s reaction.
(c) Spectrum Instrumentation GmbH 186

Pulse Generator (Firmware Option) Setting up the Pulse Generator
Continuously triggered output
After enabling the pulse generator, it will detect trigger events. Upon each trigger, the programmed number of pulses are generated, as
defined by the LEN, HIGH, DELAY and LOOPS parameters explained above. After finishing the programmed number of triggers, it will au-
tomatically arm itself again and wait for the next trigger.
In contrast to the Gated mode (see below), once a trigger has been detected the trigger input is ignored and the pulse train will finish inde-
pendent from any activity on the trigger input. Only when is has finished the current generation, a new trigger will be detected:
Image 97: timing diagram illustrating the pulse generator triggered output mode
Single Shot triggering
This mode is similar to the triggered mode, but after enabling the pulse generator it will only detect one single trigger. Upon that trigger, the
programmed number of pulses are generated, as defined by the LEN, HIGH, DELAY and LOOPS parameters explained above:
Image 98: timing diagram illustrating the pulse generator single-shot triggered output mode
Afterwards the pulse generator will not detect any further triggers, until being reset by re-enabling:
Continuously gated Output
After enabling the pulse generator, it will detect trigger events. Upon each trigger, the programmed number of pulses are generated, as
defined by the LEN, HIGH, DELAY and LOOPS parameters explained above and as long as the trigger condition or gate is still valid (HIGH).
If the gate ends, this will stop the output and reset all internal counters back to start. So, each time the gate turns HIGH, the sequence (number
of pulses as defined by the LEN, HIGH, DELAY and LOOPS) starts again from its beginning:
Image 99: timing diagram illustrating the pulse generator gated output mode
Configuring the pulse generator’s trigger source
The various possible signals that can logically be combined to form a trigger event for a pulse generator are split up into two portions each
consisting of a multiplexer (MUX).
Multiplexer 1
The first multiplexer, MUX1, selects between two different sources and also allows to be completely unused by utilizing a logical ‘1’ or HIGH
level, being transparent to the following AND condition combining the two multiplexers:
Table 208: Spectrum API: pulse generator trigger MUX1 registers with their available settings
Register Value Direction Description
SPC_XIO_PULSEGEN0_MUX1_SRC 601005 read/write Selects the input source for MUX1 for pulse generator 0.
SPC_XIO_PULSEGEN1_MUX1_SRC 601105 read/write Selects the input source for MUX1 for pulse generator 1.
SPC_XIO_PULSEGEN2_MUX1_SRC 601205 read/write Selects the input source for MUX1 for pulse generator 2.
SPC_XIO_PULSEGEN3_MUX1_SRC 601305 read/write Selects the input source for MUX1 for pulse generator 3.
SPCM_PULSEGEN_MUX1_SRC_UNUSED 0 Inputs of MUX1 are not used in creating the trigger condition and instead a static logic HIGH is used for MUX1.
SPCM_PULSEGEN_MUX1_SRC_RUN 1 This input of MUX1 reflects the current run state of the card. If acquisition/output is running the signal is HIGH. If
card has stopped the signal is LOW.
The signal is identical to XIO output using SPCM_XMODE_RUNSTATE.
SPCM_PULSEGEN_MUX1_SRC_ARM 2 This input of MUX1 reflects the current ARM state of the card. If the card is armed and ready to receive a trigger
the signal is HIGH. If the card isn’t running or the card is still acquiring pretrigger data or the trigger has already
been detected. the signal is LOW.
The signal is identical to XIO output using SPCM_XMODE_ARMSTATE.
By having the two status lines ARM and RUN available as input, it is either possible to generate pulses depending only on the card’s RUN
or ARM state (e.g., currently running or currently not running enabling the inverter of MUX1 output) or to mask other trigger conditions from
MUX2 to only be passed upon the card’s acquisition/replay RUN or ARM state.
(c) Spectrum Instrumentation GmbH 187

Pulse Generator (Firmware Option) Setting up the Pulse Generator
Multiplexer 2
The second multiplexer can be transparent and hence unused or allows to select various sources for starting the pulse creation:
• Allowing a start command issued by the application software by issuing a force trigger command
• Any one of the other pulse generator unit outputs to create pulses or pulse trains with up to four repetition time scales
• The card’s acquisition or replay trigger output
• An external logic signal coming in from any of the multi-purpose XIO input capable lines
Table 209: Spectrum API: pulse generator trigger MUX2 registers with their available settings
Register Value Direction Description
SPC_XIO_PULSEGEN0_MUX2_SRC 601006 read/write Selects the input source for MUX2 for pulse generator 0.
SPC_XIO_PULSEGEN1_MUX2_SRC 601106 read/write Selects the input source for MUX2 for pulse generator 1.
SPC_XIO_PULSEGEN2_MUX2_SRC 601206 read/write Selects the input source for MUX2 for pulse generator 2.
SPC_XIO_PULSEGEN3_MUX2_SRC 601306 read/write Selects the input source for MUX2 for pulse generator 3.
SPCM_PULSEGEN_MUX2_SRC_UNUSED 0 No input of MUX2 is used in creating the trigger condition for the pulse generator. A static logic HIGH is
used, so that the MUX output is transparent for the following AND gate.
SPCM_PULSEGEN_MUX2_SRC_SOFTWARE 1 This input reflects the positive edge generated by issuing the SPCM_PULSEGEN_CMD_FORCE command.
SPCM_PULSEGEN_MUX2_SRC_CARDTRIGGER 2 This input of MUX2 reflects the trigger detection of the acquisition/replay. The trigger output goes HIGH as
soon as the card’s main trigger is recognized. After end of acquisition/replay it is LOW again. In Multiple
Recording/Gated Sampling/ABA mode it goes LOW after the acquisition of the current segment stops. In
FIFO single mode the trigger output is HIGH until FIFO mode is stopped.
The signal is identical to what a XIO output is providing when using SPCM_XMODE_TRIGOUT.
SPCM_PULSEGEN_MUX2_SRC_PULSEGEN0 3 Input to MUX2 is set to output of pulse generator 0/1/2 or 3.
This can be used to cascade pulse generators for creating up to four pulse repetition time scales.
SPCM_PULSEGEN_MUX2_SRC_PULSEGEN1 4 Each pulse generator can select to be triggered by any of the other pulse generator’s output.
SPCM_PULSEGEN_MUX2_SRC_PULSEGEN2 5 Selecting its own pulse generator’s output as a trigger (loopback) is not allowed and will lead to a driver
error.
SPCM_PULSEGEN_MUX2_SRC_PULSEGEN3 6
SPCM_PULSEGEN_MUX2_SRC_XIO0 7 Input to MUX2 is set to the input signal coming in from multi-purpose line of X0.
M2p: Since X0 is an output only, it therefore is not allowed to be used as an input.
SPCM_PULSEGEN_MUX2_SRC_XIO1 8 Input to MUX2 is set to the input signal coming in from multi-purpose line of X1.
SPCM_PULSEGEN_MUX2_SRC_XIO2 9 Input to MUX2 is set to the input signal coming in from multi-purpose line of X2.
SPCM_PULSEGEN_MUX2_SRC_XIO3 10 Input to MUX2 is set to the input signal coming in from multi-purpose line of X3.
M4i/M4x: Since X3 is not available, it therefore is not allowed to be used as an input.
The output of the following command register is connected to all pulse generator units in parallel in a synchronous fashion:
Table 210: Spectrum API: pulse generator command register for trigger forcing by software
Register Value Direction Description
SPC_XIO_PULSEGEN_COMMAND 601501 write only Executes a command for the pulse generator option.
SPCM_PULSEGEN_CMD_FORCE 1h Generate a single rising edge, that is common for all pulse generator engines. This allows to start/trigger the output
of all enabled pulse generators synchronously by issuing a software command.
This allows to start any number of pulse generators set to MUX2_SRC_SOFTWARE to be started at the same instant even from software, useful
when requiring pulses with a known and static phase relation.
Additional trigger configuration (changing the active edge or level)
Please note that the Trigger/Gate input to the “Pulse Generation” portion is always HIGH-active. Depending
on the selected pulse generator configuration it is triggering on the rising edge or the logic HIGH state. The
two programmable inverters at the multiplexer outputs can be used to trigger on the falling edge or a logical
LOW instead.
To access the three programmable inverters and to optionally change whether triggering on a rising edge (the trigger signal changing its
state from LOW to HIGH) or on the valid level (the trigger being logically HIGH), following registers can be used:
Table 211: Spectrum API: pulse generator additional configuration registers with the available settings
Register Value Direction Description
SPC_XIO_PULSEGEN0_CONFIG 601007 read/write Bitmask with additional configuration for pulse generator 0.
SPC_XIO_PULSEGEN1_CONFIG 601107 read/write Bitmask with additional configuration for pulse generator 1.
SPC_XIO_PULSEGEN2_CONFIG 601207 read/write Bitmask with additional configuration for pulse generator 2.
SPC_XIO_PULSEGEN3_CONFIG 601307 read/write Bitmask with additional configuration for pulse generator 3.
SPCM_PULSEGEN_CONFIG_MUX1_INVERT 1h When bit is set, the output of MUX1 is logically inverted.
SPCM_PULSEGEN_CONFIG_MUX2_INVERT 2h When bit is set, the output of MUX2 is logically inverted.
SPCM_PULSEGEN_CONFIG_INVERT 4h When bit is set, the output of the pulse generator is logically inverted.
SPCM_PULSEGEN_CONFIG_HIGH 8h As default the pulse generator’s trigger input is sensitive only to a rising edge. When using this configura-
tion, the input will not look for an active edge, but rather detect a HIGH level. This is similar to the distinc-
tion of the card’s main trigger modes, when choosing between SPC_TM_POS and SPC_TM_HIGH.
(c) Spectrum Instrumentation GmbH 188

Pulse Generator (Firmware Option) Setting up the Pulse Generator
Since the register is implemented as a bitmask, any combination of the above configuration flags is possible.
// enable the inverters on MUX1 and MUX2 outputs for pulse generator 2
int32 lPulseGenConfig = (SPCM_PULSEGEN_CONFIG_MUX1_INVERT | SPCM_PULSEGEN_CONFIG_MUX2_INVERT);
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN2_CONFIG, lPulseGenConfig);
Configuring Multi Purpose lines to output generated pulses
Each of the up to four on-board multi purpose I/O lines can be programmed to output the pulses generated by its corresponding pulse gen-
erator unit, making it available for any external devices.
Please check the available modes by reading the SPCM_X0_AVAILMODES, SPCM_X1_AVAILMODES, SPCM_X2_AVAILMODES and
SPCM_X3_AVAILMODES register first. The available modes may differ from card to card and may be enhanced with new driver/firmware
versions to come.
Table 212: Spectrum API: XIO lines and mode software registers with their reduced to the settings required for outputting pulses
Register Value Direction Description
SPCM_X0_AVAILMODES 600300 read Bitmask with all bits of the below mentioned modes showing the available modes for (X0)
SPCM_X1_AVAILMODES 600301 read Bitmask with all bits of the below mentioned modes showing the available modes for (X1)
SPCM_X2_AVAILMODES 600302 read Bitmask with all bits of the below mentioned modes showing the available modes for (X2)
SPCM_X3_AVAILMODES 600303 read Bitmask with all bits of the below mentioned modes showing the available modes for (X3)
SPCM_X0_MODE 600200 read/write Defines the mode for (X0). Only one mode selection is possible to be set at a time
SPCM_X1_MODE 600201 read/write Defines the mode for (X1). Only one mode selection is possible to be set at a time
SPCM_X2_MODE 600202 read/write Defines the mode for (X2). Only one mode selection is possible to be set at a time
SPCM_X3_MODE 600203 read/write Defines the mode for (X3). Only one mode selection is possible to be set at a time
SPCM_XMODE_DISABLE 00000000h No mode selected. Output is tristate (default setup)
... ... For all other modes please see chapter “Multi Purpose I/O Lines”.
SPCM_XMODE_PULSEGEN 00080000h A/D and D/A cards only (optional):
Connector reflects the output of the same index pulse generator (X1 can output pulses from pulse generator 1, X2 can
output pulses from pulse generator 2, ... etc.).
On M4i/M4x cards with three XIO lines (X0, X1, X2) and four pulse generators, pulses from pulse generator 3 can-
not be output, but can still be used in cascading configurations to trigger another pulse generator.
Please note that a change to the SPCM_X0_MODE, SPCM_X1_MODE, SPCM_X2_MODE or SPCM_X3_MODE will
only be updated with the next call to either the M2CMD_CARD_START or M2CMD_CARD_WRITESETUP register.
For further details please see the relating chapter on the M2CMD_CARD registers.
(c) Spectrum Instrumentation GmbH 189

Pulse Generator (Firmware Option) Programming Example
Programming Example
The following example shows in principle, the steps required for generating a single, repetitive pulse with one of the pulse generators and
how to output that pulse on the matching multi-purpose I/O line:
// First we set up the channel selection and the clock.
// For this example we enable only one channel to be able to use max sampling rate on all card types.
spcm_dwSetParam_i32 (hCard, SPC_CHENABLE, CHANNEL0);
// Read out the max. supported sampling rate ...
int64 llMaxSR = 0;
spcm_dwGetParam_i64 (hCard, SPC_PCISAMPLERATE, &llMaxSR);
// ... and use this as the card’s sampling rate
spcm_dwSetParam_i64 (hCard, SPC_SAMPLERATE, llMaxSR);
// Read out the clock, at which the pulse generator will run with the above set sampling rate.
int64 llPulseGenClock_Hz = 0;
spcm_dwGetParam_i64 (hCard, SPC_XIO_PULSEGEN_CLOCK, &llPulseGenClock_Hz);
// Configure X0 to output signal from corresponding pulse generator 0
spcm_dwSetParam_i32 (hCard, SPCM_X0_MODE, SPCM_XMODE_PULSEGEN);
// Setup pulse generator 0 (output on X0)
// to generate a continuous signal with 1 MHz and ~50% duty-cycle
int32 lLenFor1MHz = static_cast < int32 > (llPulseGenClock_Hz / MEGA(1));
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN0_MODE, SPCM_PULSEGEN_MODE_TRIGGERED);
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN0_LEN, lLenFor1MHz);
// An integer division by 2 will be truncated if lLenFor1MHz is an odd number,
// resulting in a slightly shorter HIGH than LOW time.
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN0_HIGH, lLenFor1MHz / 2);
// Set LOOPS to 0: repeat infinitely
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN0_LOOPS, 0);
// Configure pulse generator to be triggered/started by software force command
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN0_MUX1_SRC, SPCM_PULSEGEN_MUX1_SRC_UNUSED);
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN0_MUX2_SRC, SPCM_PULSEGEN_MUX2_SRC_SOFTWARE);
// Enable the selected pulse generator and hence arm its trigger detection
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN_ENABLE, SPCM_PULSEGEN_ENABLE0);
// Write the settings to the card:
// This will update the clock section to generate the programmed frequencies
// (SPC_SAMPLERATE) and also write the pulse generator settings to the card.
spcm_dwSetParam_i32 (hCard, SPC_M2CMD, M2CMD_CARD_WRITESETUP);
// Start all armed pulse generators (in this case just one) by a software command
spcm_dwSetParam_i32 (hCard, SPC_XIO_PULSEGEN_COMMAND, SPCM_PULSEGEN_CMD_FORCE);
// Wait until a key is pressed
printf ("\nPress a key to stop the pulse generator(s) ");
cGetch ();
// Stop all running pulse generators
spcm_dwSetParam_i32(hCard, SPC_XIO_PULSEGEN_ENABLE, 0);
spcm_dwSetParam_i32(hCard, SPC_M2CMD, M2CMD_CARD_WRITESETUP);
Spectrum provides a dedicated programming example for the pulse generator feature as part of the stand-
ard example package. This example is showing different and more complex configurations than shown
above, e.g., cascading of multiple pulse generators for more complex pulse generation time scales.
(c) Spectrum Instrumentation GmbH 190

Option Star-Hub (M3i and M4i only) Star-Hub introduction
