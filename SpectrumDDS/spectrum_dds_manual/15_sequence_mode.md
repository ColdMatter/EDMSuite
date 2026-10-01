Sequence Mode
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
Sequence mode
Sequence mode is a dedicated firmware feature that enables the creation of a programmable output sequence. In this mode, you can define
a sequence composed of multiple steps, and each step references one data segment of the on-board memory. To support this functionality,
two separate memory areas are provided: one for storing the sequence steps and another for storing the data segments.
The sequence can contain multiple steps (the exact number depends on the hardware model and is specified in the Technical Data section).
Each step includes settings that define how many times it should repeat, which step follows next, and the condition under which the transition
occurs. The data stored for the steps is organized in the on-board memory, which is divided into multiple segments of varying sizes. Switching
between these segments occurs seamlessly, without missing samples or introducing signal artifacts.
Sequence Restart mode (driver version > 7.9)
Sequence Restart mode combines the optimized memory structure of Sequence mode with the deterministic trigger behaviour of Single Restart
mode. A sequence consists of a number of steps, and each step contains a data segment along with meta data such as loop count, segment
length, and a pointer to the next step.
In this mode, only the start of a sequence is triggered. After receiving the start trigger, the card plays through all programmed steps in order.
Each step references a single memory segment, which may be looped a defined number of times according to the meta data stored in that
step.
The total size of the sequence—meaning the total number of samples played across all steps and loops—is automatically calculated at card
start. It cannot be altered during runtime. Because the sequence size is fixed, this mode does not support the “loop until trigger” function
available in standard Sequence mode. Although certain parameters (such as loop counts or segment sizes) may be changed during runtime,
the user must ensure that the overall number of samples in the full sequence remains unchanged, otherwise unexpected behaviour may arise.
The maximum size of this total number is 240=1.15x1012samples.
When the card finishes one sequence playback, the device outputs based on the setting in SPC_CHX_STOPLEVEL see “Programming the
behavior in pauses and after replay” on page151. If a new trigger is received after completion, the entire sequence restarts from the begin-
ning.
Theory of operation
Define segments in data memory
The complete installed on-board memory of the card is divided into a user de-
finable number of segments. Each segment space has the same length limiting
the maximum length of one data segment to [Installed Memory] / [Number of
Segments]. Each data segment can be filled by the user with patterns of differ-
ent lengths or can even be left completely empty if unused:
In our example we see the complete installed card memory is being split into
8 segments and 6 of these segments are actually filled with data sequences of
different length afterwards (indicated in red). Two of these segments are not
Image 74: Sequence Mode: Segment definition in card memory
needed for the assumed sequence and therefore left empty as an example. Due
to the fact that each sequence step can be associated with any of the data seg-
ments, it is also possible to use a data segment in multiple steps or to just once upload the data for multiple sequences, and just change the
order of the sequence.
Each data segment is filled with data for all active channels in a multiplexed way. Please check the chapter “data organization” for details
of how to organize the data inside each segment.
Define steps in sequence memory
The sequence memory defines a number of data loop steps that are executed
step by step either linear or interrupted by waiting for trigger event. The first
step that is entered after a card start is separately defined by software. When
being entered, each step first repeats the associated data segment the number
times defined by its loop parameter. Afterwards the sequencer will either au-
tomatically proceed either unconditionally or check for a trigger event as a
condition to change over to the next step, which is defined by the steps next
parameter. This next segment can be the same segment again performing an
Image 75: Sequence mode: steps and step looping
(c) Spectrum Instrumentation GmbH 148

Sequence Mode Theory of operation
endless loop or the beginning of the sequence to repeat the sequence until being stopped by the user. Additionally a step can also be defined
to be the last step in a sequence such that the card is stopped afterwards.
In our example 4 steps have been defined. Three of them (Step #1, Step #3, Step #4) perform an endless loop that will be repeated contin-
uously. The output of the card will then be 10 times data segment #2, 100 times data segment #4, 1 time data segment #7 and then starting
over with 10 times data segment #2 and so on...
In this first simple example the sequence consisting of the three steps is once defined prior to the card start and not changed during runtime,
therefore the shown Step #2 is not used here. There will be an extra passage later, that shows how the sequence memory can be updated
or modified even during runtime, whilst the replay is in progress.
Programming
Programming of the sequence mode is done using the known driver interface with the addition of a few new registers.
Gathering information
If the sequence mode is installed on the card, the different details and limits of the sequence programming can be read out:
Table 139: Spectrum API: sequence mode registers and register settings
Register
SPC_PCIFEATURES 2120 read only PCI feature register. Holds the installed features and options as a bit field. The return value must be
masked out with one of the masks below to get information about one certain feature.
SPCM_FEAT_SEQUENCE 1000h Sequence mode available (only available for arbitrary generator and digital I/O cards).
Register
SPC_SEQMODE_AVAILMAXSEGMENT 349900 read only Returns the maximum number of segments the memory can be divided into. Please note that only
dividers with a power of 2 are possible return values.
SPC_SEQMODE_AVAILMAXSTEPS 349901 read only Returns the maximum number of sequence steps that can be used on this card.
SPC_SEQMODE_AVAILMAXLOOP 349902 read only Returns the maximum number of loops that can be programmed for a step.
SPC_SEQMODE_AVAILFEATURES 349903 read only Returns the available features for each sequence step as shown below:
SPCSEQ_ENDLOOPONTRIG 40000000h The step runs endless until a trigger is received. If no trigger has been detected, the step will enter itself again, count-
ing down its own loops and check for a trigger again. For a minimum reaction time on an external trigger event it is
good practice to set the loop parameter to 1 in the step checking for the trigger.
SPCSEQ_END 80000000h This sequence step is the end of the sequence. The card is stopped at the end of this segment after the loop counter
has reached his end.
Setting up the registers
Define the card mode
To enable the sequencer the card mode needs to be set appropriately first:
Table 140: Spectrum API: card mode register with Sequence Mode setup
Register
SPC_CARDMODE 9500 read/write Defines the used operating mode.
SPC_REP_STD_SEQUENCE 40000h Data generation from on-board memory, by splitting the memory into several segments and replaying the data using
a programmable order coming from a special sequence memory.
SPC_REP_STD_SEQUENCERESTART 8000000h Similar to SPC_REP_STD_SEQUENCE, but with the main difference that the card waits after the last step of the
sequence for a trigger to replay the sequence again with a deterministic trigger to output latency. It is NOT allowed to
use the wait for trigger flag inside the sequence.
Prepare the data memory
Setting up the segmentation of the on-board data memory is done by using the following registers:
Table 141: Spectrum API: sequence mode registers for segment handling
Register
SPC_SEQMODE_MAXSEGMENTS 349910 read/write Programs the number of segments the on-board memory should be divided into. If changing the num-
ber of segments all information that has been stored before is lost and all sequence data and all
sequence setup has to be written again. Only a power of two is allowed with a minimum value of
two, but not all of the segments must be actually used in the sequence.
If reading this register the number of segments the memory is currently divided into is returned.
SPC_SEQMODE_WRITESEGMENT 349920 read/write Defines the current segment to be addressed by the user. Must be programmed prior to changing any
segment parameters.
SPC_SEQMODE_SEGMENTSIZE 349940 read/write Defines the number of valid/to be replayed samples for the current selected memory segment in sam-
ples per channel.
Each data segment is filled with data for all active channels in a multiplexed way. Please check the chapter “data organization” for details
of how to organize the data inside each segment.
Due to the internal organization of the card memory there is a certain minimum, maximum and stepsize when setting the segmentsize for the
sequence memory. The following table gives you an overview of all limits. The table shows all values in relation to the installed memory size
in samples. If more memory is installed the maximum memory size figures will increase according to the complete installed memory:
(c) Spectrum Instrumentation GmbH 149

Sequence Mode Theory of operation
For analog waveform generator (D/A) cards
For cards with 16 bit converter resolution
Activated Pattern size for register
Channels SPC_SEQMODE_SEGMENTSIZE
Min Max Step
1 channel 384 (Mem/1) / SPC_SEQMODE_MAXSEGMENTS) 32
2 channels 192 (Mem/2) / SPC_SEQMODE_MAXSEGMENTS) 32
4 channels 96 (Mem/4) / SPC_SEQMODE_MAXSEGMENTS) 32
Definition of the transfer buffer
The data transfer itself is done using the standard data transfer commands, with the exception that the buffer type and the direction is fixed
in combination with the sequence mode. The definition of the buffer is done with the spcm_dwDefTransfer function as explained in an earlier
chapter.
uint32 _stdcall spcm_dwDefTransfer_i64 (// Defines the transfer buffer by using 64 bit unsigned integer values
drv_handle hDevice, // handle to an already opened device
uint32 dwBufType, // fixed SPCM_BUF_DATA (segment memory is always in on-board memory)
uint32 dwDirection, // fixed SPCM_DIR_PCTOCARD (only available for replay cards)
uint32 dwNotifySize, // number of bytes after which an event is sent (0=end of transfer)
void* pvDataBuffer, // pointer to the data buffer
uint64 qwBrdOffs, // offset for transfer in relation to the currently selected segment
uint64 qwTransferLen); // buffer length for the currently selected segment
The programming examples further below will show the setup and also some examples of data transfer.
Set up the sequence memory
Sequence steps are programmed using a dedicated register for each step. Please note that the register has to be written with 64 bit of data
to cover all settings. It is possible to either use raw 64 bit access or multiplexed 64 bit access (2 times 32 bit data). The masks mentioned in
the table below are 32 bit masks only, so that they can be used for 64 bit and 32 bit accesses.
Table 142: Spectrum API: sequence mode step registers and register setup
Register Value Direction Description
SPC_SEQMODE_STEPMEM0 340000 read/write First address (sequence step 0) of the 64 bit organized sequence memory.
... ... ... ...
SPC_SEQMODE_STEPMEM0 + 4095 344095 read/write Writes the sequence step 4095, as an example. The maximum number of steps should be read
out by using the SPC_SEQMODE_AVAILMAXSTEPS register as described above.
Lower 32 bit:
SPCSEQ_SEGMENTMASK 0000FFFFh Associates the current sequence step with one of the memory segments.
SPCSEQ_NEXTSTEPMASK FFFF0000h Defines the next step in the sequence.
Upper 32 bit:
SPCSEQ_LOOPMASK 000FFFFFh Defines how often the memory segment associated with the current step will be repeated before the next step
condition will be evaluated.
SPCSEQ_ENDLOOPALWAYS 0h Unconditionally change to the next step, if defined loops for the current segment have been replayed.
SPCSEQ_ENDLOOPONTRIG 40000000h Feature flag that marks the step to conditionally change to the next step on a trigger condition. The occurrence
of a trigger event is repeatedly checked each time the defined loops for the current segment have been
replayed. A temporary valid trigger condition will be stored until evaluation at the end of the step. (Sequence
Mode only)
SPCSEQ_END 80000000h Feature flag that marks the current step to be the last in the sequence. The card is stopped at the end of this seg-
ment after the loop counter has reached his end.
The start step register allows to define which of the set up steps is used first after card start. Therefore is possible to upload multiple sequences
prior to the start and switch between these sequences by using a simple command, setting a different starting point:
Table 143: Spectrum API: sequence mode start register
Register Value Direction Description
SPC_SEQMODE_STARTSTEP 349930 read/write Defines which of all defined steps in the sequence memory will be used first directly after the
card start.
In Sequence Restart mode: this step also needs to be the “next step” in the last step of the
sequence and marks the step before which the card waits until a trigger is received.
In Sequence Restart Mode, the maximum number of samples in a sequence (sum of each (data segment
length * number of loops)) is limited to ≤ (244 / enabled channels).
Read out the currently replayed sequence step
In case one wants to change the sequence on the fly or one needs to know which part of the sequence is currently replayed. It is possible to
read out the number of the sequence step that is currently at the output connector of the card. This could be extremely useful if external equip-
(c) Spectrum Instrumentation GmbH 150

Sequence Mode Theory of operation
ment has to be changed after a dedicated sequence has been replayed or if the AWG is changing between different patterns in automatic
test environment.
Table 144: Spectrum API: sequence mode segment status register
Register Value Direction Description
SPC_SEQMODE_STATUS 349950 read Number of the sequence step that is currently replayed.
Due to the internal structure of the sequencer , the delay between a trigger event and the change in the se-
quence, when using the SPCSEQ_ENDLOOPONTRIG feature, is not a fixed value but rather varies with the
current fill-size of the Output FIFO. Please see „Output latency“ section in this manual for the size of the
OutputFIFO on your card and take half the value given there as the max. value when using the Sequence mode. In
Sequence mode only half the value is involved, since in this mode only the FIFO between the on-board memory and
the DAC is relevant and the FIFO between the PCIe receiver and the on-board memory not in use in Sequence mode.
To avoid this, please have a look at the Sequence Restart mode.
Programming the behavior in pauses and after replay
Usually the used outputs of the analog generation boards are set to zero level after replay. This is in most cases adequate. In some cases it
can be necessary to hold the last sample, to output the maximum positive level or maximum negative level after replay. The stoplevel will stay
on the defined level until the next output will be made. With the following registers you can define the behavior after replay, when using the
cards in AWG mode:
Table 145: Spectrum API: stop level register and register settings
Register Value Direction Description
SPC_CH0_STOPLEVEL 206020 read/write Defines the behavior after replay in AWG mode for channel 0
SPC_CH1_STOPLEVEL 206021 read/write Defines the behavior after replay in AWG mode for channel 1
SPC_CH2_STOPLEVEL 206022 read/write Defines the behavior after replay in AWG mode for channel 2
SPC_CH3_STOPLEVEL 206023 read/write Defines the behavior after replay in AWG mode for channel 3
SPCM_STOPLVL_ZERO 16 Defines the analog output to enter zero level (D/A converter is fed with digital zero value).
When synchronous digital bits are replayed, these will be set to LOW state during pause.
SPCM_STOPLVL_LOW 2 Defines the analog output to enter maximum negative level (D/A converter is fed with most negative level).
When synchronous digital bits are replayed, these will be set to LOW state during pause.
SPCM_STOPLVL_HIGH 4 Defines the analog output to enter maximum positive level (D/A converter is fed with most positive level).
When synchronous digital bits are replayed, these will be set to HIGH state during pause.
SPCM_STOPLVL_HOLDLAST 8 Holds the last replayed sample on the analog output. When synchronous digital bits are replayed, their last state will
also be hold.
SPCM_STOPLVL_CUSTOM 32 Allows to define a 16bit wide custom level per channel for the analog output to enter in pauses.The sample format is
exactly the same as during replay, as described in the „sample format“ section.
When synchronous digital bits are replayed along, the custom level must include these as well and therefore allows to
set a custom level for each multi-purpose line separately.
When using SPCM_STOPLVL_CUSTOM, the sample value for the pauses must be defined via the following registers:
Table 146: Spectrum API: custom stop level registers
Register Value Direction Description
SPC_CH0_CUSTOM_STOP 206050 read/write Defines the custom stop level for channel 0 when using SPCM_STOPLVL_CUSTOM.
SPC_CH1_CUSTOM_STOP 206051 read/write Defines the custom stop level for channel 1 when using SPCM_STOPLVL_CUSTOM.
SPC_CH2_CUSTOM_STOP 206052 read/write Defines the custom stop level for channel 2 when using SPCM_STOPLVL_CUSTOM.
SPC_CH3_CUSTOM_STOP 206053 read/write Defines the custom stop level for channel 3 when using SPCM_STOPLVL_CUSTOM.
All outputs that are not activated for replay, will keep the programmed stoplevel also while the replay is in progress.
Example showing how to set a custom stoplevel for channel 0:
// enable the use of custom stop level and use raw value 10487 as stop value
spcm_dwSetParam_i32 (stCard.hDrv, SPC_CH0_STOPLEVEL, SPCM_STOPLVL_CUSTOM);
spcm_dwSetParam_i32 (stCard.hDrv, SPC_CH0_CUSTOM_STOP, 10487);
Changing sequences or step parameters during runtime
In Sequence Restart Mode, the total number of samples within the sequence must remain constant during
runtime. You may modify steps, loop counts, and segment parameters while the card is operating, however
any modification that changes the overall number of samples could lead to unexpected behavior..
(c) Spectrum Instrumentation GmbH 151

Sequence Mode Synchronization
Due to the strict separation of the two memory areas it is also possible to change the sequence memory during runtime. If we look again on
the example sequence below, we can see that there is an unused step #2:
Image 76: sequence mode changing sequence on-the-fly
In our example 3 steps have been defined, prior to the card start, and these at first are not changed. Additionally Step#2 is set up to repeat
itself, but due to the defined start step it is normally not used. Due to the nature of the sequence memory (read-before-write) it is possible to
write to any step register in the sequence memory during runtime without corrupting the sequence memory. By addressing a certain step and
changing for example its next parameter, it is possible switch between two sequences by software. Because the user does not know what
sequence is currently replayed, one cannot leave the „current“ step but instead has to address one certain step and therefore defines an
exit/change state.
Assuming in the example above, that we change the next parameter of Step#4 from Next=1 to Next=2, the infinitely executed 3-step se-
quence that is used as default after card start will be left the next time that the replay finishes the last sample of the pattern associated with
Step#4 (which in this case is Segment#7), will then jump to step #2 and seamlessly continue replaying with the first sample off the associated
segment #3. As step #2 links back to itself it will generate data segment #3 in an endless loop until being either stopped by a software
command or another change in the sequence is applied.
Any of the three step parameters „Next“, „Segment“ and „Loop“ of any step in the sequence memory can be changed during runtime, without
corruption the sequence memory. However once a step is entered, it will first execute the current parameters such as replay the associated
pattern and repeating it the programmed number of times.
Changing data patterns during runtime
In addition to the possible runtime changes within the sequence memory as described above, it is also possible to change the parts of the
pattern memory.
However since the data memory’s nature is not „read-before-write“, the user must take care not to change
the content of the memory segments, which are used within the currently active sequence. In Sequence Re-
start only: in between sequences while waiting for a trigger it is not possible to change the data segments
of the first step(s), because the card will preload data into its FIFO buffer to allow for a deterministic trigger to output
time.
Changing the data pattern can be useful in applications, where the data for the next test needs to be updated based on results from the
currently running test. Remember to update the sequence step entries if the segment length has changed, so that the driver can automatically
re-calculate the internal start-addresses of the segments.
Synchronization
Please note that the sequence mode is NOT fully synchronized using the star-hub. This also relates to
generatorNETBOX products with an internal star-hub.
Using sequence mode together with star-hub, it is still possible to
• synchronize the clock
• synchronize the start of the cards
• synchronize the trigger times (Sequence Restart mode only)
However, with star-hub synchronization, it is NOT possible to
• synchronize any changes inside the step memory
• synchronize software commands that change the step memory order
• synchronize a trigger that ends a steps loop (Sequence mode only)
The above mentioned restrictions are also valid with any other setup of synchronization apart from star-hub, be it
via external clock or internal SH-direct clock.
(c) Spectrum Instrumentation GmbH 152

Sequence Mode Sequence programming example
Sequence programming example
The following example shows a very simple sequence as an example. Only two segments are used, the first is replayed 10 times and then
unconditionally left and replay switches over to the second segment. This segment is repeated until a trigger event is detected by the card.
After the trigger has been detected the sequence starts over again ... until the card is stopped.
// Setup of channel enable, output conditioning as well as trigger setup not shown for simplicity.
#define MAX_SEGMENTS 2 // only 2 segments used here for simplicity
int32 lBytesPerSample, lChCnt;
// Read out used bytes per sample
spcm_dwGetParam_i32 (hDrv, SPC_MIINST_BYTESPERSAMPLE, &lBytesPerSample);
spcm_dwGetParam_i32 (hDrv, SPC_CHCOUNT, &lBytesPerSample);
// Setting up the card mode
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_STD_SEQUENCE); // enable sequence mode
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_MAXSEGMENTS, 2); // Divide on-board mem in two parts
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_STARTSTEP, 0); // Step#0 is the first step after card start
// Setting up the data memory and transfer data
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_WRITESEGMENT, 0); // set current configuration switch to segment 0
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_SEGMENTSIZE, 1024); // define size of current segment 0
// it is assumed, that the Buffer memory has been allocated and is already filled with valid data
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD, 0, pData, 0, 1024 * lBytesPerSample * lChCnt);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// Setting up the data memory and transfer data
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_WRITESEGMENT, 1); // set current configuration switch to segment 1
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_SEGMENTSIZE, 512); // define size of current segment 1
// it is assumed, that the Buffer memory has been allocated and is already filled with valid data
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD, 0, pData, 0, 512 * lBytesPerSample * lChCnt);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// Setting up the sequence memory (Only two steps used here as an example)
int32 lStep = 0; // current step is Step#0
int64 llSegment = 0; // associated with data memory segment 0
int64 llLoop = 10; // Pattern will be repeated 10 times
int64 llNext = 1; // Next step is Step#1
int64 llCondition = SPCSEQ_ENDLOOPALWAYS; // Unconditionally leave current step
// combine all the parameters to one int64 bit value
int64 llValue = (llCondition << 32) | (llLoop << 32) | (llNext << 16) | (llSegment);
spcm_dwSetParam_i64 (hDrv, SPC_SEQMODE_STEPMEM0 + lStep, llValue);
lStep = 1; // current step is Step#1
llSegment = 1; // associated with data memory segment 1
llLoop = 1; // Pattern will be repeated once before condition is checked
llNext = 0; // Next step is Step#0
llCondition = SPCSEQ_ENDLOOPONTRIG; // Repeat current step until a trigger has occurred
llValue = (llCondition << 32) | (llLoop << 32) | (llNext << 16) | (llSegment);
spcm_dwSetParam_i64 (hDrv, SPC_SEQMODE_STEPMEM0 + lStep, llValue);
// Start the card
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER);
// ... wait here or do something else ...
// Stop the card
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_STOP);
(c) Spectrum Instrumentation GmbH 153

Sequence Mode Sequence Restart programming example
Sequence Restart programming example
The following example shows a very simple sequence as an example. Only two segments are used, the first is replayed 10 times and then
unconditionally left and replay switches over to the second segment. This segment is repeated 5 times and afterwards output the last sample
until a trigger has been detected, then the sequence starts over again ... until the card is stopped.
// Setup of channel enable, output conditioning as well as trigger setup not shown for simplicity.
#define MAX_SEGMENTS 2 // only 2 segments used here for simplicity
int32 lBytesPerSample, lChCnt;
// Read out used bytes per sample
spcm_dwGetParam_i32 (hDrv, SPC_MIINST_BYTESPERSAMPLE, &lBytesPerSample);
spcm_dwGetParam_i32 (hDrv, SPC_CHCOUNT, &lBytesPerSample);
// Setting up the card mode
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_STD_SEQUENCERESTART); // enable sequence restart mode
// after the last step the card keeps outputting the last sample, until a new trigger is detected.
spcm_dwSetParam_i32 (hDrv, SPC_CH0_STOPLEVEL, SPCM_STOPLVL_HOLDLAST);
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_MAXSEGMENTS, 2); // Divide on-board mem in two parts
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_STARTSTEP, 0); // Step#0 is the first step after card start
// Setting up the data memory and transfer data
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_WRITESEGMENT, 0); // set current configuration switch to segment 0
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_SEGMENTSIZE, 1024); // define size of current segment 0
// it is assumed, that the Buffer memory has been allocated and is already filled with valid data
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD, 0, pData, 0, 1024 * lBytesPerSample * lChCnt);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// Setting up the data memory and transfer data
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_WRITESEGMENT, 1); // set current configuration switch to segment 1
spcm_dwSetParam_i32 (hDrv, SPC_SEQMODE_SEGMENTSIZE, 512); // define size of current segment 1
// it is assumed, that the Buffer memory has been allocated and is already filled with valid data
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD, 0, pData, 0, 512 * lBytesPerSample * lChCnt);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// Setting up the sequence memory (Only two steps used here as an example)
int32 lStep = 0; // current step is Step#0
int64 llSegment = 0; // associated with data memory segment 0
int64 llLoop = 10; // segment will be repeated 10 times
int64 llNext = 1; // Next step is Step#1
// combine all the parameters to one int64 bit value
int64 llValue = (llLoop << 32) | (llNext << 16) | (llSegment);
spcm_dwSetParam_i64 (hDrv, SPC_SEQMODE_STEPMEM0 + lStep, llValue);
lStep = 1; // current step is Step#1
llSegment = 1; // associated with data memory segment 1
llLoop = 1; // segment will be played once
llNext = 0; // Next step is Step#0. The loop needs to point back to the start step,
// this is where the waiting for trigger happens.
llValue = (llLoop << 32) | (llNext << 16) | (llSegment);
spcm_dwSetParam_i64 (hDrv, SPC_SEQMODE_STEPMEM0 + lStep, llValue);
// Start the card
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER);
// ... wait here or do something else ...
// Stop the card
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_STOP);
(c) Spectrum Instrumentation GmbH 154

Mode DDS20 (20-tone DDS) General Information
