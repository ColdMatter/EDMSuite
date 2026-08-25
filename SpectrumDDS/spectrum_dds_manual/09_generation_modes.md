Generation modes
Your card is able to run in different modes. Depending on the selected mode there are different registers that each define an aspect of this
mode. The single modes are explained in this chapter. Any further modes that are only available if an option is installed on the card is doc-
umented in a later chapter.
Overview
This chapter gives you a general overview on the related registers for the different modes. The use of these registers throughout the different
modes is described in the following chapters.
Setup of the mode
The mode register is organized as a bitmap. Each mode corresponds to one bit of this bitmap. When defining the mode to use, please be
sure just to set one of the bits. All other settings will return an error code.
The main difference between all standard and all FIFO modes is that the standard modes are limited to on-board memory and therefore can
run with full sampling rate. The FIFO modes are designed to transfer data continuously over the bus to PC memory or to hard disk and can
therefore run much longer. The FIFO modes are limited by the maximum bus transfer speed the PC can use. The FIFO mode uses the complete
installed on-board memory as a FIFO buffer.
However as you’ll see throughout the detailed documentation of the modes the standard and the FIFO mode are similar in programming and
behavior and there are only a very few differences between them.
Table 48: Spectrum API: card mode and read out of available card mode software registers
Register Value Direction Description
SPC_CARDMODE 9500 read/write Defines the used operating mode, a read command will return the currently used mode.
SPC_AVAILCARDMODES 9501 read Returns a bitmap with all available modes on your card. The modes are listed below.
Replay modes
Mode Value Description
SPC_REP_STD_SINGLE 100h Data generation from on-board memory repeating the complete programmed memory either once, infinite or for a
defined number of times after one single trigger event.
SPC_REP_STD_MULTI 200h Data generation from on-board memory for multiple trigger events. Each generated segment has the same size. This
mode is described in greater detail in a special chapter about the Multiple Replay mode.
SPC_REP_STD_GATE 400h Data generation from on-board memory using an external gate signal. Data is only generated as long as the gate sig-
nal has a programmed level. The mode is described in greater detail in a special chapter about the Gated Replay
mode.
SPC_REP_STD_SINGLERESTART 8000h Data generation from on-board memory. The programmed memory is repeated once after each single trigger event.
SPC_REP_STD_SEQUENCE 40000h Data generation from on-board memory splitting the memory into several segments and replaying the data using a
special sequence memory. The mode is described in greater detail in a special chapter about the Sequence mode.
SPC_REP_FIFO_SINGLE 800h Continuous data generation after one single trigger event. The on-board memory is used completely as FIFO buffer.
SPC_REP_FIFO_MULTI 1000h Continuous data generation after multiple trigger events. The on-board memory is used completely as FIFO buffer.
SPC_REP_FIFO_GATE 2000h Continuous data generation using an external gate signal. The on-board memory is used completely as FIFO buffer.
SPC_REP_STD_DDS 4000000h DDS replay mode functionality.
(c) Spectrum Instrumentation GmbH 95

Generation modes Commands
Commands
The data acquisition/data replay is controlled by the command register. The command register controls the state of the card in general and
also the state of the different data transfers. Data transfers are explained in an extra chapter later on.
The commands are split up into two types of commands: execution commands that fulfill a job and wait commands that will wait for the
occurrence of an interrupt. Again the commands register is organized as a bitmap allowing you to set several commands together with one
call. As not all of the command combinations make sense (like the combination of reset and start at the same time) the driver will check the
given command and return an error code ERR_SEQUENCE if one of the given commands is not allowed in the current state.
Table 49: Spectrum API: card command register and different commands with descriptions
Register Value Direction Description
SPC_M2CMD 100 write only Executes a command for the card or data transfer.
Card execution commands
M2CMD_CARD_RESET 1h Performs a hard and software reset of the card as explained further above.
M2CMD_CARD_WRITESETUP 2h Writes the current setup to the card without starting the hardware. This command may be useful if changing some
internal settings like clock frequency and enabling outputs.
M2CMD_CARD_START 4h Starts the card with all selected settings. This command automatically writes all settings to the card if any of the set-
tings has been changed since the last one was written. After card has been started, only some of the settings might
be changed while the card is running, such as e.g. output level and offset for D/A replay cards.
M2CMD_CARD_ENABLETRIGGER 8h The trigger detection is enabled. This command can be either sent together with the start command to enable trigger
immediately or in a second call after some external hardware has been started.
M2CMD_CARD_FORCETRIGGER 10h This command forces a trigger even if none has been detected so far. Sending this command together with the start
command is similar to using the software trigger.
M2CMD_CARD_DISABLETRIGGER 20h The trigger detection is disabled. All further trigger events are ignored until the trigger detection is again enabled.
When starting the card the trigger detection is started disabled.
M2CMD_CARD_STOP 40h Stops the current run of the card. If the card is not running this command has no effect.
Card wait commands
These commands do not return until either the defined state has been reached which is signaled by an interrupt from the card or the timeout
counter has expired. If the state has been reached the command returns with an ERR_OK. If a timeout occurs the command returns with ER-
R_TIMEOUT. If the card has been stopped from a second thread with a stop or reset command, the wait function returns with ERR_ABORT.
M2CMD_CARD_WAITPREFULL 1000h Acquisition modes only: the command waits until the pretrigger area has once been filled with data. After pretrigger
area has been filled the internal trigger engine starts to look for trigger events if the trigger detection has been ena-
bled.
M2CMD_CARD_WAITTRIGGER 2000h Waits until the first trigger event has been detected by the card. If using a mode with multiple trigger events like Multi-
ple Recording or Gated Sampling there only the first trigger detection will generate an interrupt for this wait com-
mand.
M2CMD_CARD_WAITREADY 4000h Waits until the card has completed the current run. In an acquisition mode receiving this command means that all data
has been acquired. In a generation mode receiving this command means that the output has stopped.
Wait command timeout
If the state for which one of the wait commands is waiting isn’t reached any of the wait commands will either wait forever if no timeout is
defined or it will return automatically with an ERR_TIMEOUT if the specified timeout has expired.
Table 50: Spectrum API: timeout definition register
Register Value Direction Description
SPC_TIMEOUT 295130 read/write Defines the timeout for any following wait command in a millisecond resolution. Writing a zero to this
register disables the timeout.
As a default the timeout is disabled. After defining a timeout this is valid for all following wait commands until the timeout is disabled again
by writing a zero to this register.
A timeout occurring should not be considered as an error. It did not change anything on the board status. The board is still running and will
complete normally. You may use the timeout to abort the run after a certain time if no trigger has occurred. In that case a stop command is
necessary after receiving the timeout. It is also possible to use the timeout to update the user interface frequently and simply call the wait
function afterwards again.
(c) Spectrum Instrumentation GmbH 96

Generation modes Commands
Example for card control:
// card is started and trigger detection is enabled immediately
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER);
// we wait a maximum of 1 second for a trigger detection. In case of timeout we force the trigger
spcm_dwSetParam_i32 (hDrv, SPC_TIMEOUT, 1000);
if (spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_WAITTRIGGER) == ERR_TIMEOUT)
{
printf (“No trigger detected so far, we force a trigger now!\n”);
spcm_dwSetParam (hdrv, SPC_M2CMD, M2CMD_CARD_FORCETRIGGER);
}
// we disable the timeout and wait for the end of the run
spcm_dwSetParam_i32 (hDrv, SPC_TIMEOUT, 0);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_WAITREADY);
printf (“Card has stopped now!\n”);
In case that a firmware update has gone wrong and the card has booted its golden recovery firmware (either
automatically or by manual selection), all M2CMD_CARD_START commands will be returned with an ERR_-
GOLDENIMAGE error. The sole intention for the golden image is to provide a backup for safely updating the
standard firmware image.
Card Status
In addition to the wait for an interrupt mechanism or completely instead of it one may also read out the current card status by reading the
SPC_M2STATUS register. The status register is organized as a bitmap, so that multiple bits can be set, showing the status of the card and
also of the different data transfers.
Table 51: Spectrum API: card status register and possible status values with descriptions of the status
Register Value Direction Description
SPC_M2STATUS 110 read only Reads out the current status information
M2STAT_CARD_PRETRIGGER 1h Acquisition modes only: the first pretrigger area has been filled. In Multi/ABA/Gated acquisition this status is set only
for the first segment and will be cleared at the end of the acquisition.
M2STAT_CARD_TRIGGER 2h The first trigger has been detected.
M2STAT_CARD_READY 4h The card has finished its run and is ready.
M2STAT_CARD_SEGMENT_PRETRG 8h This flag will be set for each completed pretrigger area including the first one of a Single acquisition.
Additionally for a Multi/ABA/Gated acquisition of M4i/M4x/M2p only, this flag will be set when the pretrigger
area of a segment has been filled and will be cleared after the trigger for a segment has been detected.
Acquisition cards status overview
The following drawing gives you an overview of the card commands and card status information. After start of card with M2CMD_-
CARD_START the card is acquiring pretrigger data until one time complete pretrigger data has been acquired. Then the status bit M2STAT_-
CARD_PRETRIGGER is set. Either the trigger has been enabled together with the start command or the card now waits for trigger enable
command M2CMD_CARD_ENABLETRIGGER. After receiving this command the trigger engine is enabled and card checks for a trigger event.
As soon as the trigger event is received the status bit M2STAT_CARD_TRIGGER is set and the card acquires the programmed posttrigger
data. After all post trigger data has been acquired the status bit M2STAT_CARD_READY is set and data can be read out:
Image 49: Acquisition cards: graphical overview of acquisition status and card command interaction
Generation card status overview
This drawing gives an overview of the card commands and status information for a simple generation mode. After start of card with the
M2CMD_CARD_START the card is armed and waiting. Either the trigger has been enabled together with the start command or the card now
waits for trigger enable command M2CMD_CARD_ENABLETRIGGER. After receiving this command the trigger engine is enabled and card
checks for a trigger event. As soon as the trigger event is received the status bit M2STAT_CARD_TRIGGER is set and the card starts with the
(c) Spectrum Instrumentation GmbH 97

Generation modes Commands
data replay. After replay has been finished - depending on the programmed mode - the status bit M2STAT_CARD_READY is set and the card
stops.
Image 50: Generation cards: graphical overview of generation status and card command interaction
Data Transfer
Data transfer consists of two parts: the buffer definition and the commands/status information that controls the transfer itself. Data transfer
shares the command and status register with the card control commands and status information. In general the following details on the data
transfer are valid for any data transfer in any direction:
• The memory size register (SPC_MEMSIZE) must be programmed before starting the data transfer.
• When the hardware buffer is adjusted from its default (see „Output latency“ section later in this manual), this must be done before defin-
ing the transfer buffers in the next step via the spcm_dwDefTransfer function.
• Before starting a data transfer the buffer must be defined using the spcm_dwDefTransfer function.
• Each defined buffer is only used once. After transfer has ended the buffer is automatically invalidated.
• If a buffer has to be deleted although the data transfer is in progress or the buffer has at least been defined it is necessary to call the spc-
m_dwInvalidateBuf function.
Definition of the transfer buffer
Before any data transfer can start it is necessary to define the transfer buffer with all its details. The definition of the buffer is done with the
spcm_dwDefTransfer function as explained in an earlier chapter.
uint32 _stdcall spcm_dwDefTransfer_i64 (// Defines the transfer buffer by using 64 bit unsigned integer values
drv_handle hDevice, // handle to an already opened device
uint32 dwBufType, // type of the buffer to define as listed below under SPCM_BUF_XXXX
uint32 dwDirection, // the transfer direction as defined below
uint32 dwNotifySize, // number of bytes after which an event is sent (0=end of transfer)
void* pvDataBuffer, // pointer to the data buffer
uint64 qwBrdOffs, // offset for transfer in board memory
uint64 qwTransferLen); // buffer length
This function is used to define buffers for standard sample data transfer as well as for extra data transfer for additional ABA or timestamp
information. Therefore the dwBufType parameter can be one of the following:
SPCM_BUF_DATA 1000 Buffer is used for transfer of standard sample data
SPCM_BUF_ABA 2000 Buffer is used to read out slow ABA data. Details on this mode are described in the chapter about the ABA mode
option
SPCM_BUF_TIMESTAMP 3000 Buffer is used to read out timestamp information. Details on this mode are described in the chapter about the
timestamp option.
The dwDirection parameter defines the direction of the following data transfer:
SPCM_DIR_PCTOCARD 0 Transfer is done from PC memory to on-board memory of card
SPCM_DIR_CARDTOPC 1 Transfer is done from card on-board memory to PC memory.
SPCM_DIR_CARDTOGPU 2 RDMA transfer from card memory to GPU memory, SCAPP option needed, Linux only
SPCM_DIR_GPUTOCARD 3 RDMA transfer from GPU memory to card memory, SCAPP option needed, Linux only
The direction information used here must match the currently used mode. While an acquisition mode is used
there’s no transfer from PC to card allowed and vice versa. It is possible to use a special memory test mode
to come beyond this limit. Set the SPC_MEMTEST register as defined further below.
The dwNotifySize parameter defines the amount of bytes after which an interrupt should be generated. If leaving this parameter zero, the
transfer will run until all data is transferred and then generate an interrupt. Filling in notify size > zero will allow you to use the amount of
data that has been transferred so far. The notify size is used on FIFO mode to implement a buffer handshake with the driver or when trans-
ferring large amount of data where it may be of interest to start data processing while data transfer is still running. Please see the chapter on
handling positions further below for details.
(c) Spectrum Instrumentation GmbH 98

Generation modes Commands
M2i, M3i, M4i, M4x and M2p cards:
The Notify size sticks to the page size which is defined by the PC hardware and the operating system. There-
fore the notify size must be a multiple of 4 KiByte. For main data transfer it may also be a fraction of 4Ki in
the range of 16, 32, 64, 128, 256, 512, 1Ki or 2Ki. No other values are allowed. For ABA and timestamp the
notify size can be 2Ki as a minimum. If you need to work with ABA or timestamp data in smaller chunks please use
the polling mode as described later.
M5i:
The Notify size sticks to the page size which is defined by the PC hardware and the operating system. There-
fore the notify size must be a multiple of 4 KiByte. For main data transfer it may also be a fraction of 4Ki in
the range of 64, 128, 256, 512, 1Ki or 2Ki. No other values are allowed. For timestamp the notify size can
be 2Ki as a minimum. If you need to work with timestamp data in smaller chunks please use the polling mode as
described later.
The pvDataBuffer must point to an allocated data buffer for the transfer. Please be sure to have at least the amount of memory allocated that
you program to be transferred. If the transfer is going from card to PC this data is overwritten with the current content of the card on-board
memory.
The pvDataBuffer needs to be aligned to a page size (4096 bytes). Please use appropriate software com-
mands when allocating the data buffer. Using a non-aligned buffer may result in data corruption.
When not doing FIFO mode one can also use the qwBrdOffs parameter. This parameter defines the starting position for the data transfer as
byte value in relation to the beginning of the card memory. Using this parameter allows it to split up data transfer in smaller chunks if one
has acquired a very large on-board memory.
The qwTransferLen parameter defines the number of bytes that has to be transferred with this buffer. Please be sure that the allocated memory
has at least the size that is defined in this parameter. In standard mode this parameter cannot be larger than the amount of data defined with
memory size.
M5i cards only:
On M5i cards the qwTransferLen parameter needs to be an integer multiple of 64 bytes.
Memory test mode
In some cases it might be of interest to transfer data in the opposite direction. Therefore a special memory test mode is available which allows
random read and write access of the complete on-board memory. While memory test mode is activated no normal card commands are pro-
cessed:
Table 52: Spectrum API: memory test register
Register Value Direction Description
SPC_MEMTEST 200700 read/write Writing a 1 activates the memory test mode, no commands are then processed.
Writing a 0 deactivates the memory test mode again.
Invalidation of the transfer buffer
The command can be used to invalidate an already defined buffer if the buffer is about to be deleted by user. This function is automatically
called if a new buffer is defined or if the transfer of a buffer has completed
uint32 _stdcall spcm_dwInvalidateBuf ( // invalidate the transfer buffer
drv_handle hDevice, // handle to an already opened device
uint32 dwBufType); // type of the buffer to invalidate as listed above under SPCM_BUF_XXXX
The dwBufType parameter need to be the same parameter for which the buffer has been defined:
SPCM_BUF_DATA 1000 Buffer is used for transfer of standard sample data
SPCM_BUF_ABA 2000 Buffer is used to read out slow ABA data. Details on this mode are described in the chapter about the ABA mode
option. The ABA mode is only available on analog acquisition cards.
SPCM_BUF_TIMESTAMP 3000 Buffer is used to read out timestamp information. Details on this mode are described in the chapter about the times-
tamp option. The timestamp mode is only available on analog or digital acquisition cards.
Commands and Status information for data transfer buffers.
As explained above the data transfer is performed with the same command and status registers like the card control. It is possible to send
commands for card control and data transfer at the same time as shown in the examples further below.
Table 53: Spectrum API: Command register and commands for DMA transfers
Register Value Direction Description
SPC_M2CMD 100 write only Executes a command for the card or data transfer
M2CMD_DATA_STARTDMA 10000h Starts the DMA transfer for an already defined buffer. In acquisition mode it may be that the card hasn’t received a
trigger yet, in that case the transfer start is delayed until the card receives the trigger event
(c) Spectrum Instrumentation GmbH 99

Generation modes Standard Single Replay modes
M2CMD_DATA_WAITDMA 20000h Waits until the data transfer has ended or until at least the amount of bytes defined by notify size are available. This
wait function also takes the timeout parameter described above into account.
M2CMD_DATA_STOPDMA 40000h Stops a running DMA transfer. Data is invalid afterwards.
The data transfer can generate one of the following status information:
Table 54: Spectrum API: status register and status codes for DMA data transfer
Register Value Direction Description
SPC_M2STATUS 110 read only Reads out the current status information
M2STAT_DATA_BLOCKREADY 100h The next data block as defined in the notify size is available. It is at least the amount of data available but it also can
be more data.
M2STAT_DATA_END 200h The data transfer has completed.
M2STAT_DATA_OVERRUN 400h The data transfer had on overrun (acquisition) or underrun (replay) while doing FIFO transfer.
M2STAT_DATA_ERROR 800h An internal error occurred while doing data transfer.
Example of data transfer
void* pvData = pvAllocMemPageAligned (1024);
// transfer data from PC memory to card memory (on replay cards) ...
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD , 0, pvData, 0, 1024);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// ... or transfer data from card memory to PC memory (acquisition cards)
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_CARDTOPC , 0, pvData, 0, 1024);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// explicitely stop DMA tranfer prior to invalidating buffer
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STOPDMA);
spcm_dwInvalidateBuf (hDrv, SPCM_BUF_DATA);
vFreeMemPageAligned (pvData, 1024);
To keep the example simple it does no error checking. Please be sure to check for errors if using these command in real world programs!
Users should take care to explicitly send the M2CMD_DATA_STOPDMA command prior to invalidating the
buffer, to avoid crashes due to race conditions when using higher-latency data transportation layers, such
as to remote Ethernet devices.
Standard Single Replay modes
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
The standard single modes are the easiest and mostly used modes to generate analog or digital data with a Spectrum arbitrary waveform
generation or digital output card. In standard single replay mode the card is working totally independent from the PC, after the card setup is
done and the data has been transferred into the on-board memory. The advantage of the Spectrum boards is that regardless to the system
usage the card will refresh the outputs with equidistant time intervals.
The data for replay is stored in the on-board memory and is held there for being replayed after the trigger event. This mode allows sample
generation at very high refresh rates without the need to transfer the data from the memory of the host system to the card at high speed.
Card mode
The card mode has to be set to the correct mode:
Table 55: Spectrum API: card mode register and single mode settings
Register Value Direction Description
SPC_CARDMODE 9500 read/write Defines the used operating mode, a read command will return the currently used mode.
SPC_REP_STD_SINGLE 100h Data generation from on-board memory repeating the complete programmed memory either once, infinite or for a
defined number of times after one single trigger event.
SPC_REP_STD_SINGLERESTART 8000h Data generation from on-board memory replaying the complete programmed memory on every detected trigger
event. The number of replays can be programmed by loops.
Memory setup
You have to define, how many samples are to be replayed from the on-board memory and how many times the complete memory should be
replayed after the trigger event.
(c) Spectrum Instrumentation GmbH 100

Generation modes Standard Single Replay modes
Please note that the memory size must be programmed to the correct value PRIOR to making any data trans-
fer to the card memory. An incorrect memory size value at the time the data transfer is initiated will result in
corrupted data and a wrong output.
Table 56: Spectrum API: memory and loop settings
Register Value Direction Description
SPC_MEMSIZE 10000 read/write Sets the memory size in samples per channel. The memory size setting must be set before transferring
data to the card.
SPC_LOOPS 10020 read/write Number of times the memory is replayed. If set to zero the generation will run continuously until it is
stopped by the user.
The maximum memsize that can be used for generating data is of course limited by the installed amount of memory and by the number of
channels to be replayed. Please have a look at the topic "Limits of pre, post memsize, loops" later in this chapter.
SPC_REP_STD_SINGLE
This mode waits for one trigger events and after this it starts to replay the programmed memory either once, a pre-defined number of times
on infinitely until explicitly stopped by the user. The SPC_LOOPS register is used to define the number of possible repetitions. Setting this
register to 0 the generation will continue until explicitly stopped by the user. Any other value than 0 for SPC_LOOPS will result in the signal
being replayed SPC_LOOPS times until the card stops automatically. For replaying the memory content only once after a trigger the SP-
C_LOOPS values hence must be set to a value of 1.
Replay of a data pattern just once (SPC_LOOPS = 1):
Image 51: timing diagram of single replay mode with commands and status changes
Replay for a defined number of times (2 in the example shown with SPC_LOOPS = 2):
Image 52: timing diagram of single replay mode with two loops with commands and status changes
Replay continuously until the replay is stopped/aborted by the user (SPC_LOOPS = 0):
Image 53: timing diagram of continuous replay mode stopped by user with commands and status changes
(c) Spectrum Instrumentation GmbH 101

Generation modes FIFO Single replay mode
SPC_REP_STD_SINGLERESTART
This mode behaves like multiple shots of SPC_REP_STD_SINGLE but with a very small re-arming time in between. When using this mode the
memory content is replayed on every detected trigger event. The SPC_LOOPS parameter defines how long this replay should continue. A
value of zero defines the mode to run continuously until stopped by the user.
Image 54: timing diagram of single restart mode with commands and status changes
Between the different replayed pieces the output will go to the programmed stoplevel.
Overview of settings and resulting modes
This table gives a brief overview on the setup of loops and the resulting behaviour of the output
Table 57: Spectrum API: overview of mode settings in relation to loops settings and resulting behaviour
SPC_LOOPS = 0 SPC_LOOPS = 1 SPC_LOOPS = N
SPC_REP_STD_SINGLE Replay starts with the first trigger event and The programmed memory content is replayed Replay starts with the first trigger event and
then the programmed data is replayed in a once after detection of the trigger event. then the programmed data is replayed in a
continuous loop until stopped by the user. continuous loop until the programmed number
N of loops has been replayed. Afterward the
card stops.
SPC_REP_STD_SINGLERESTART The programmed memory is replayed once on n.a. (similar to SPC_REP_STD_SINGLE) The programmed memory is replayed once on
every trigger event. This continues until every trigger event. This continues until the
stopped by the user. memory is N-times replayed. Afterwards the
card stops.
Continuous marker output
If using the continuous output one can activate a marker output on the multi-purpose I/O connectors marking the beginning of each loop.
The marker output will generate a TTL pulse on one of the multi-purpose I/O lines. The pulse length is of ½ of programmed memory. The
marker output is enabled using the dedicated multi-purpose I/O line setup that is described later in this manual. Please see the chapter „Multi
Purpose I/O Lines“ to find more information.
Example
The following example shows a simple standard single mode data generation setup with the transfer of data before the card is started. To
keep this example simple there is no error checking implemented.
int32 lMemsize = 16384; // replay length is set to 16 KiSamples
spcm_dwSetParam_i32 (hDrv, SPC_CHENABLE, CHANNEL0); // only one channel activated
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_STD_SINGLE); // set the standard single replay mode
spcm_dwSetParam_i64 (hDrv, SPC_MEMSIZE, lMemsize); // replay length
spcm_dwSetParam_i64 (hDrv, SPC_LOOPS, 1); // replay memsize once
void* pvData = pvAllocMemPageAligned (2 * lMemsize); // create a data buffer, 2 bytes per sample
vCalculate_or_Load_Data (pvData); // pvData must now be filled with data
// transfer the data to the on-board memory
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD , 0, pvData, 0, 2 * lMemsize);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// now we start the generation and wait for the interrupt that signalizes the end
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER | M2CMD_CARD_WAITREADY);
FIFO Single replay mode
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
(c) Spectrum Instrumentation GmbH 102

Generation modes FIFO Single replay mode
The FIFO single mode does a continuous data replay using the on-board memory as a FIFO buffer and transferring data continuously from
PC memory. One can generate the data on-line or load data continuously from disk.
Card mode
The card mode has to be set to the correct mode SPC_REP_FIFO_SINGLE.
Table 58: Spectrum API: FIFO single replay mode register and settings
Register Value Direction Description
SPC_CARDMODE 9500 read/write Defines the used operating mode, a read command will return the currently used mode.
SPC_REP_FIFO_SINGLE 800h Continuous data replay from PC memory. Complete on-board memory is used as FIFO buffer.
Length of FIFO mode
In general FIFO mode can run forever until it is stopped by an explicit user command or one can program the total length of the transfer by
two counters Loop and Segment size
Table 59: Spectrum API: FIFO mode length settings registers
Register Value Direction Description
SPC_SEGMENTSIZE 10010 read/write Length of segments to replay.
SPC_LOOPS 10020 read/write Number of segments to replay in total. If set to zero the FIFO mode will run continuously until it is
stopped by the user.
The total amount of samples per channel that is replayed can be calculated by [SPC_LOOPS * SPC_SEGMENTSIZE]. Please stick to the below
mentioned limitations of these registers.
Difference to standard single mode
The standard modes and the FIFO modes do not differ very much from the programming point of view. In fact one can even use the FIFO
mode to get the same behaviour as the standard mode. The buffer handling that is shown in the next chapter is the same for both modes.
Length of replay.
In standard mode the replay (memory size) length is defined before the start and is limited to the installed on-board memory whilst in FIFO
mode the replay length can either be defined or it can run continuously until user stops it.
(c) Spectrum Instrumentation GmbH 103

Generation modes FIFO Single replay mode
Example (FIFO replay)
The following example shows a simple FIFO single mode data replay setup with the data calculation placed somewhere else. To keep this
example simple there is no error checking implemented. Please see in this example that data has to be calculated and transferred prior to
the start of the output. The card start and the DMA transfer start cannot be done simultaneously.
spcm_dwSetParam_i32 (hDrv, SPC_CHENABLE, CHANNEL0); // only one channel activated
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_FIFO_SINGLE); // set the FIFO single replay mode
// in FIFO mode we need to define the buffer before starting the transfer
int16* pnData = (int16*) pvAllocMemPageAligned (llBufsizeInSamples * 2); // assuming 2 bytes per sample
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD, 4096, // set noptifysize to 4KiByte
(void*) pnData, 0, 2 * llBufsizeInSamples);
// before start we once have to fill some data in for the start of the output
vCalcOrLoadData (&pnData[0], 2 * llBufsizeInSamples);
spcm_dwSetParam_i64 (hDrv, SPC_DATA_AVAIL_CARD_LEN, 2 * llBufsizeInSamples);
dwError = spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// now the first <notifysize> bytes have been transferred to card and we start the output
dwError = spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER);
// we replay data in a loop. As we defined a notify size of 4Ki we’ll generate the data in >=4Ki chuncks
llTotalBytes = 2 * llBufsizeInSamples;
while (!dwError)
{
// read out the available bytes that are free again
spcm_dwGetParam_i64 (hDrv, SPC_DATA_AVAIL_USER_LEN, &llAvailBytes);
spcm_dwGetParam_i64 (hDrv, SPC_DATA_AVAIL_USER_POS, &llUserPosInBytes);
// be sure not to make a rollover and limit the data to be processed
if ((llUserPosInBytes + llAvailBytes) > (2 * llBufsizeInSamples))
llAvailBytes = (2 * llBufsizeInSamples) - llUserPosInBytes;
llotalBytes += llAvailBytes;
// generate some new data
vCalcOrLoadData (&pnData[llUserPosInBytes / 2], llAvailBytes);
printf ("Currently Available: %lld, total: %lld\n", llAvailBytes, llTotalBytes);
// now we mark the number of bytes that we just generated for replay and wait for the next free buffer
spcm_dwSetParam_i64 (hDrv, SPC_DATA_AVAIL_CARD_LEN, llAvailBytes);
dwError = spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_WAITDMA);
}
(c) Spectrum Instrumentation GmbH 104

Generation modes Limits of segment size, memory size
Limits of segment size, memory size
The maximum memory size parameter is only limited by the number of activated channels and by the amount of installed memory. Please
keep in mind that each sample needs 2 bytes of memory to be stored.
Due to the internal organization of the card memory there is a certain stepsize when setting these values that has to be taken into account.
The following table gives you an overview of all limits concerning memory size, segment size and loops. The table shows all values in relation
to the installed memory size in samples. If more memory is installed the maximum memory size figures will increase according to the complete
installed memory:
Table 60: Spectrum API: limits of segment size, memory size and loops registers depending on selected mode
| Activated | Used            | Memory size |      | Segment size    |      | Loops         |      |
| --------- | --------------- | ----------- | ---- | --------------- | ---- | ------------- | ---- |
| Channels  | Mode            | SPC_MEMSIZE |      | SPC_SEGMENTSIZE |      | SPC_LOOPS     |      |
|           |                 | Min Max     | Step | Min Max         | Step | Min Max       | Step |
| 1 channel | Standard Single | 32 Mem      | 32   | not used        |      | 0 () 4Gi - 1 | 1    |
|           | Single Restart  | 32 Mem      | 32   | not used        |      | 0 () 4Gi - 1 | 1    |
|           | Standard Multi  | 32 Mem      | 32   | 16 Mem/2        | 16   | 0 () 1       | 1    |
0 ()
|            | Standard Gate   | 32 Mem   | 32  | not used    |     | 1             | 1   |
| ---------- | --------------- | -------- | --- | ----------- | --- | ------------- | --- |
|            | FIFO Single     | not used |     | 16 8Gi - 16 | 16  | 0 () 4Gi - 1 | 1   |
|            | FIFO Multi      | not used |     | 16 Mem/2    | 16  | 0 () 4Gi - 1 | 1   |
|            | FIFO Gate       | not used |     | not used    |     | 0 () 4Gi - 1 | 1   |
| 2 channels | Standard Single | 32 Mem/2 | 32  | not used    |     | 0 () 4Gi - 1 | 1   |
0 ()
|     | Single Restart | 32 Mem/2 | 32  | not used    |     | 4Gi - 1       | 1   |
| --- | -------------- | -------- | --- | ----------- | --- | ------------- | --- |
|     | Standard Multi | 32 Mem/2 | 32  | 16 Mem/4    | 16  | 0 () 1       | 1   |
|     | Standard Gate  | 32 Mem/2 | 32  | not used    |     | 0 () 1       | 1   |
|     | FIFO Single    | not used |     | 16 8Gi - 16 | 16  | 0 () 4Gi - 1 | 1   |
|     | FIFO Multi     | not used |     | 16 Mem/4    | 16  | 0 () 4Gi - 1 | 1   |
0 ()
|            | FIFO Gate       | not used |     | not used |     | 4Gi - 1       | 1   |
| ---------- | --------------- | -------- | --- | -------- | --- | ------------- | --- |
| 4 channels | Standard Single | 32 Mem/4 | 32  | not used |     | 0 () 4Gi - 1 | 1   |
|            | Single Restart  | 32 Mem/4 | 32  | not used |     | 0 () 4Gi - 1 | 1   |
|            | Standard Multi  | 32 Mem/4 | 32  | 16 Mem/8 | 16  | 0 () 1       | 1   |
|            | Standard Gate   | 32 Mem/4 | 32  | not used |     | 0 () 1       | 1   |
0 ()
|     | FIFO Single | not used |     | 16 8Gi - 16 | 16  | 4Gi - 1       | 1   |
| --- | ----------- | -------- | --- | ----------- | --- | ------------- | --- |
|     | FIFO Multi  | not used |     | 16 Mem/8    | 16  | 0 () 4Gi - 1 | 1   |
|     | FIFO Gate   | not used |     | not used    |     | 0 () 4Gi - 1 | 1   |
All figures listed here are given in samples. An entry of [8Ki - 16] means [8 KibiSamples - 16] = [8192 - 16] = 8176 samples.
The given memory and memory / divider figures depend on the installed on-board memory as listed below:
Installed Memory
2 GiSample
| Mem      |     | 2 GiSample   |     |     |     |     |     |
| -------- | --- | ------------ | --- | --- | --- | --- | --- |
| Mem / 2  |     | 1 GiSample   |     |     |     |     |     |
| Mem / 4  |     | 512 MiSample |     |     |     |     |     |
| Mem / 8  |     | 256 MiSample |     |     |     |     |     |
Please keep in mind that this table shows all values at once. Only the absolute maximum and minimum values are shown. There might be
additional limitations. Which of these values is programmed depends on the used mode. Please read the detailed documentation of the mode.

(c) Spectrum Instrumentation GmbH 105

Generation modes Buffer handling
Buffer handling
To handle the huge amount of data that can possibly be acquired with the M5i/M4i/M4x/M2p series cards, there is a very reliable two
step buffer strategy set up. The on-board memory of the card can be completely used as a real FIFO buffer. In addition a part of the PC
memory can be used as an additional software buffer. Transfer between hardware FIFO and software buffer is performed interrupt driven
and automatically by the driver to get best performance. The following drawing will give you an overview of the structure of the data transfer
handling:
Image 55: Overview of buffer handling for DMA transfers showing and the interaction with the DMA engine
Although an M4i is shown here, this applies to M5i, M4x and M2p cards as well. A data buffer handshake is implemented in the driver
which allows to run the card in different data transfer modes. The software transfer buffer is handled as one large buffer which is on the one
side controlled by the driver and filled automatically by busmaster DMA from/to the hardware FIFO buffer and on the other hand it is handled
by the user who set’s parts of this software buffer available for the driver for further transfer. The handshake is fulfilled with the following 3
software registers:
Table 61: Spectrum API: registers for DMA buffer handling
Register Value Direction Description
SPC_DATA_AVAIL_USER_LEN 200 read Returns the number of currently to the user available bytes inside a sample data transfer.
SPC_DATA_AVAIL_USER_POS 201 read Returns the position as byte index where the currently available data samples start.
SPC_DATA_AVAIL_CARD_LEN 202 write Writes the number of bytes that the card can now use for sample data transfer again
SPC_FILLSIZEPROMILLE 200910 read The register holds the current fill size of the on-board memory (FIFO buffer) in per mill (1/1000) of
the full on-board memory.
Internally the card handles two counters, a user counter and a card counter. Depending on the transfer direction the software registers have
slightly different meanings:
Table 62: Spectrum API: content of DMA buffer handling registers for different use cases
Transfer direction Register Direction Description
Write to card SPC_DATA_AVAIL_USER_LEN read This register contains the currently available number of bytes that are free to write new data to the
card. The user can now fill this amount of bytes with new data to be transferred.
SPC_DATA_AVAIL_CARD_LEN write After filling an amount of the buffer with new data to transfer to card, the user tells the driver with this
register that the amount of data is now ready to transfer.
Read from card SPC_DATA_AVAIL_USER_LEN read This register contains the currently available number of bytes that are filled with newly transferred
data. The user can now use this data for own purposes, copy it, write it to disk or start calculations
with this data.
SPC_DATA_AVAIL_CARD_LEN write After finishing the job with the new available data the user needs to tell the driver that this amount of
bytes is again free for new data to be transferred.
Any direction SPC_DATA_AVAIL_USER_POS read The register holds the current byte index position where the available bytes start. The register is just
intended to help you and to avoid own position calculation
Any direction SPC_FILLSIZEPROMILLE read The register holds the current fill size of the on-board memory (FIFO buffer) in promille (1/1000) of
the full on-board memory. Please note that the hardware reports the fill size only in 1/16 parts of the
full memory. The reported fill size is therefore only shown in 1000/16 = 63 promille steps.
Directly after start of transfer the SPC_DATA_AVAIL_USER_LEN is every time zero as no data is available for the user and the SPC_DATA-
_AVAIL_CARD_LEN is every time identical to the length of the defined buffer as the complete buffer is available for the card for transfer.
The counter that is holding the user buffer available bytes (SPC_DATA_AVAIL_USER_LEN) is related to the
notify size at the DefTransfer call. Even when less bytes already have been transferred you won’t get notice
of it in case the notify size is programmed to a higher value.
Remarks
• The transfer between hardware FIFO buffer and application buffer is done with scatter-gather DMA using a busmaster DMA controller
(c) Spectrum Instrumentation GmbH 106

Generation modes Buffer handling
located on the card. Even if the PC is busy with other jobs data is still transferred until the application data buffer is completely used.
• Even if application data buffer is completely used there’s still the hardware FIFO buffer that can hold data until the complete on-board
memory is used. Therefore a larger on-board memory will make the transfer more reliable against any PC dead times.
• As you see in the above picture data is directly transferred between application data buffer and on-board memory. Therefore it is abso-
lutely critical to delete the application data buffer without stopping any DMA transfers that are running actually. It is also absolutely criti-
cal to define the application data buffer with an unmatching length as DMA can than try to access memory outside the application data
area.
• As shown in the drawing above the DMA control will announce new data to the application by sending an event. Waiting for an event is
done internally inside the driver if the application calls one of the wait functions. Waiting for an event does not consume any CPU time
and is therefore highly desirable if other threads do a lot of calculation work. However it is not necessary to use the wait functions and
one can simply request the current status whenever the program has time to do so. When using this polling mode the announced availa-
ble bytes still stick to the defined notify size!
• If the on-board FIFO buffer has an overrun (card to PC) or an underrun (PC to card) data transfer is stopped. However in case of transfer
from card to PC there is still a lot of data in the on-board memory. Therefore the data transfer will continue until all data has been trans-
ferred although the status information already shows an overrun.
• For very small notify sizes, getting best bus transfer performance could be improved by using a „continuous buffer“. This mode is
explained in the appendix in greater detail.
M2i, M3i, M4i, M4x and M2p cards:
The Notify size sticks to the page size which is defined by the PC hardware and the operating system. There-
fore the notify size must be a multiple of 4 KiByte. For main data transfer it may also be a fraction of 4Ki in
the range of 16, 32, 64, 128, 256, 512, 1Ki or 2Ki. No other values are allowed. For ABA and timestamp the
notify size can be 2Ki as a minimum. If you need to work with ABA or timestamp data in smaller chunks please use
the polling mode as described later.
M5i:
The Notify size sticks to the page size which is defined by the PC hardware and the operating system. There-
fore the notify size must be a multiple of 4 KiByte. For main data transfer it may also be a fraction of 4Ki in
the range of 64, 128, 256, 512, 1Ki or 2Ki. No other values are allowed. For timestamp the notify size can
be 2Ki as a minimum. If you need to work with timestamp data in smaller chunks please use the polling mode as
described later.
The following graphs will show the current buffer positions in different states of the transfer. The drawings have been made for the transfer
from card to PC. However all the block handling is similar for the opposite direction, just the empty and the filled parts of the buffer are
inverted.
Step 1: Buffer definition
Directly after buffer definition the complete buffer is empty (card to PC) or
completely filled (PC to card). In our example we have a notify size which
is 1/4 of complete buffer memory to keep the example simple. In real
world use it is recommended to set the notify size to a smaller stepsize.
Step 2: Start and first data available
In between we have started the transfer and have waited for the first data
to be available for the user. When there is at least one block of notify size
in the memory we get an interrupt and can proceed with the data. Any
data that already was transferred is announced. The USER_POS is still
zero as we are right at the beginning of the complete transfer.
Step 3: set the first data available for card
Now the data can be processed. If transfer is going from card to PC that
may be storing to hard disk or calculation of any figures. If transfer is go-
ing from PC to card that means we have to fill the available buffer again
with data. After the amount of data that has been processed by the user
application we set it available for the card and for the next step.
Step 4: next data available
After reaching the next border of the notify size we get the next part of the
data buffer to be available. In our example at the time when reading the
USER_LEN even some more data is already available. The user position
will now be at the position of the previous set CARD_LEN.
Step 5: set data available again
Again after processing the data we set it free for the card use.
In our example we now make something else and don’t react to the inter-
rupt for a longer time. In the background the buffer is filled with more da-
ta.
(c) Spectrum Instrumentation GmbH 107

Generation modes Buffer handling
Step 6: roll over the end of buffer
Now nearly the complete buffer is filled. Please keep in mind that our cur-
rent user position is still at the end of the data part that we processed and
marked in step 4 and step 5. Therefore the data to process now is split in
two parts. Part 1 is at the end of the buffer while part 2 is starting with
address 0.
Step 7: set the rest of the buffer available
Feel free to process the complete data or just the part 1 until the end of
the buffer as we do in this example. If you decide to process complete
buffer please keep in mind the roll over at the end of the buffer.
This buffer handling can now continue endless as long as we manage to
set the data available for the card fast enough. The USER_POS and USER_LEN for step 8 would now look exactly as the buffer shown in step 2.
Buffer handling example for transfer from card to PC (Data acquisition)
int8* pcData = (int8*) pvAllocMemPageAligned (llBufferSizeInBytes);
// we now define the transfer buffer with the minimum notify size of one page = 4 KiByte
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_CARDTOPC , 4096, (void*) pcData, 0, llBufferSizeInBytes);
// we start the DMA transfer
dwError = spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA);
do
{
if (!dwError)
{
// we wait for the next data to be available. Afte this call we get at least 4Ki of data to proceed
dwError = spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_WAITDMA);
// if there was no error we can proceed and read out the available bytes that are free again
spcm_dwGetParam_i64 (hDrv, SPC_DATA_AVAIL_USER_LEN, &llAvailBytes);
spcm_dwGetParam_i64 (hDrv, SPC_DATA_AVAIL_USER_POS, &llBytePos);
printf (“We now have %lld new bytes available\n”, llAvailBytes);
printf (“The available data starts at position %lld\n”, llBytesPos);
// we take care not to go across the end of the buffer, handling the wrap-around
if ((llBytePos + llAvailBytes) >= llBufferSizeInBytes)
llAvailBytes = llBufferSizeInBytes - llBytePos;
// our do function gets a pointer to the start of the available data section and the length
vDoSomething (&pcData[llBytesPos], llAvailBytes);
// the buffer section is now immediately set available for the card
spcm_dwSetParam_i64 (hDrv, SPC_DATA_AVAIL_CARD_LEN, llAvailBytes);
}
}
while (!dwError); // we loop forever if no error occurs
(c) Spectrum Instrumentation GmbH 108

Generation modes Output latency
Buffer handling example for transfer from PC to card (Data generation)
int8* pcData = (int8*) pvAllocMemPageAligned (llBufferSizeInBytes);
// before starting transfer we first need to fill complete buffer memory with meaningful data
vDoGenerateData (&pcData[0], llBufferSizeInBytes);
// we now define the transfer buffer with the minimum notify size of one page = 4 KiByte
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD , 4096, (void*) pcData, 0, llBufferSizeInBytes);
// and transfer some data to the hardware buffer before the start of the card
spcm_dwSetParam_i32 (hDrv, SPC_DATA_AVAIL_CARD_LEN, llBufferSizeInBytes);
dwError = spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
do
{
if (!dwError)
{
// if there was no error we can proceed and read out the current amount of available data
spcm_dwGetParam_i64 (hDrv, SPC_DATA_AVAIL_USER_LEN, &llAvailBytes);
spcm_dwGetParam_i64 (hDrv, SPC_DATA_AVAIL_USER_POS, &llBytePos);
printf (“We now have %lld free bytes available\n”, llAvailBytes);
printf (“The available data starts at position %lld\n”, llBytesPos);
// we take care not to go across the end of the buffer, handling the wrap-around
if ((llBytePos + llAvailBytes) >= llBufferSizeInBytes)
llAvailBytes = llBufferSizeInBytes - llBytePos;
// our do function gets a pointer to the start of the available data section and the length
vDoGenerateData (&pcData[llBytesPos], llAvailBytes);
// now we mark the number of bytes that we just generated for replay
// and wait for the next free buffer
spcm_dwSetParam_i64 (hDrv, SPC_DATA_AVAIL_CARD_LEN, llAvailBytes);
dwError = spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_WAITDMA);
}
}
while (!dwError); // we loop forever if no error occurs
Please keep in mind that you are using a continuous buffer writing/reading that will start again at the zero
position if the buffer length is reached. However the DATA_AVAIL_USER_LEN register will give you the com-
plete amount of available bytes even if one part of the free area is at the end of the buffer and the second
half at the beginning of the buffer.
Output latency
The card is designed to have a most stable and reliable continuous
output in FIFO mode. Therefore as default the complete on-board
memory is used for buffering data. This however means that you have
quite a large latency when changing output data dynamically in reac-
tion of - for example - some external events.
Image 56: output latency involved components
To have a smaller output latency when using dynamically changing
data it is recommended that you use smaller buffers. The size of the software buffer is programmed as described above. The size of the
hardware buffer can be programmed using a special register:
Table 63: Spectrum API: output buffer size register and register settings
Register
SPC_DATA_OUTBUFSIZE 209 read/write Programms the used hardware buffer size for output direction. The default value is the complete
standard on-board memory (which is 4 GiByte). The output buffer size can be programmed in steps
of factortwo of the minimum size of 1Ki. Resulting in allowed settings of 1Ki, 2Ki, 4Ki, 8Ki, 16Ki, ...
up to the installed on-board memory size.
When the hardware buffer is adjusted, this must be followed by a M2CMD_CARD_WRITESETUP command and
done after defining the card mode but before defining the transfer buffers via the spcm_dwDefTransfer func-
tion, as shown in the example below.
(c) Spectrum Instrumentation GmbH 109

Generation modes Output latency
The size of the output FIFO is fixed to 192KiByte (Latency 3) and cannot be changed. If setting a hardware buffer to 64KiByte (Latency 2)
and using a software buffer of 64KiByte (Latency 1), the total size of buffered data is hence 320KiByte. Please see the following table for
some example output latency calculations (1 sample = 2 bytes), taking buffers and the clock rate into account:
Table 64: output latency depending on channel settings, buffer settings and output FIFO
Configuration Sampling rate Software Buffer Hardware Buffer Output FIFO Overall
Size Latency Size Latency Size (max) Latency Latency
(max)
1 x 16 Bit Channel 1.25 GS/s 8 MiByte 3.36 ms 4 GiByte 1717.99 ms 192 KiByte 0.08 ms 1721.4 ms
8 MiByte 3.36 ms 8 MiByte 3.36 ms 192 KiByte 0.08 ms 6.8 ms
1 MiByte 0.42 ms 1 MiByte 0.42 ms 192 KiByte 0.08 ms 0.9 ms
64 KiByte 0.026 ms 64 KiByte 0.026 ms 192 KiByte 0.08 ms 0.13 ms
1 x 16 Bit Channel 625 MS/s 8 MiByte 6.71 ms 8 MiByte 6.71 ms 192 KiByte 0.16 ms 13.6 ms
1 MiByte 0.84 ms 1 MiByte 0.84 ms 192 KiByte 0.16 ms 1.8 ms
64 KiByte 0.052 ms 64 KiByte 0.052 ms 192 KiByte 0.16 ms 0.26 ms
1 x 16 Bit Channel 100 MS/s 8 MiByte 41.94 ms 8 MiByte 41.94 ms 192 KiByte 0.98 ms 84.9 ms
1 MiByte 5.24 ms 1 MiByte 5.24 ms 192 KiByte 0.98 ms 11.5 ms
64 KiByte 0.33 ms 64 KiByte 0.33 ms 192 KiByte 0.98 ms 1.6 ms
4 x 16 Bit Channel 100 MS/s 8 MiByte 10.49 ms 8 MiByte 10.49 ms 192 KiByte 0.25 ms 21.2 ms
1 MiByte 1.31 ms 1 MiByte 1.31 ms 192 KiByte 0.25 ms 2.9 ms
64 KiByte 0.08 ms 64 KiByte 0.08 ms 192 KiByte 0.25 ms 0.4 ms
Please keep in mind that lowering the output buffer size also means that the risk of a buffer underrun gets
higher as less data is buffered on the hardware side. Therefore please be careful with selecting the correct
hardware buffer size and do not make it smaller than absolutely necessary.
The above mentioned latency calculations are only an example on how to calculate the time. They’re not tested in
real life to run continuously with that sampling speed.
A very simplified example showing the parameters mentioned above can be seen here:
void* pvBuffer = NULL;
int64 llHWBufSize = KILO_B(64); // equals 64 * 1024
int64 llSWBufSize = KILO_B(128); // must be an integer multiple of llNotifysize
uint32 dwNotifySize = KILO_B(8);
uint32 dwErr;
// define card mode first
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_FIFO_SINGLE);
// secondly define the hardware buffer and write it to the hardware
spcm_dwSetParam_i64 (hDrv, SPC_DATA_OUTBUFSIZE, llHWBufSize);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_WRITESETUP);
// and then allocate and setup the software fifo buffer
pvBuffer = pvAllocMemPageAligned ((uint32) llSWBufSize);
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_PCTOCARD, dwNotifySize, pvBuffer, 0, llSWBufSize);
// --> now fill the buffer with initial data (not shown here)
spcm_dwSetParam_i64 (hDrv, SPC_DATA_AVAIL_CARD_LEN, llSWBufSize);
// now that SW-buffer is filled, we start the data transfer (replay itself is not started yet)
// and wait for the data to be transferred.
spcm_dwSetParam_i32 (stCard.hDrv, SPC_TIMEOUT, 1000);
dwErr = spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
if (!dwErr)
{
// please see FIFO replay examples for further details regarding the complete data transfer ...
}
(c) Spectrum Instrumentation GmbH 110

Generation modes Data organization
Data organization
Data is organized in a multiplexed way in the transfer buffer. If using 2 channels data of first activated channel comes first, then data of
second channel.
Table 65: M4i and M4x cards data organization
| Activated Channels | Ch0 Ch1 | Ch2 Ch3 Samples ordering in buffer memory starting with data offset zero |     |     |
| ------------------ | ------- | ------------------------------------------------------------------------ | --- | --- |
1 channel X A0 A1 A2 A3 A4 A5 A6 A7 A8 A9 A10 A11 A12 A13 A14 A15 A16
1 channel X B0 B1 B2 B3 B4 B5 B6 B7 B8 B9 B10 B11 B12 B13 B14 B15 B16
1 channel X C0 C1 C2 C3 C4 C5 C6 C7 C8 C9 C10 C11 C12 C13 C14 C15 C16
1 channel X D0 D1 D2 D3 D4 D5 D6 D7 D8 D9 D10 D11 D12 D13 D14 D15 D16
2 channels X X A0 B0 A1 B1 A2 B2 A3 B3 A4 B4 A5 B5 A6 B6 A7 B7 A8
2 channels X X A0 C0 A1 C1 A2 C2 A3 C3 A4 C4 A5 C5 A6 C6 A7 C7 A8
2 channels X X A0 D0 A1 D1 A2 D2 A3 D3 A4 D4 A5 D5 A6 D6 A7 D7 A8
2 channels X X B0 C0 B1 C1 B2 C2 B3 C3 B4 C4 B5 C5 B6 C6 B7 C7 B8
2 channels X X B0 D0 B1 D1 B2 D2 B3 D3 B4 D4 B5 D5 B6 D6 B7 D7 B8
2 channels X X C0 D0 C1 D1 C2 D2 C3 D3 C4 D4 C5 D5 C6 D6 C7 D7 C8
4 channels X X X X A0 B0 C0 D0 A1 B1 C1 D1 A2 B2 C2 D2 A3 B3 C3 D3 A4
The samples are re-named for better readability. A0 is sample 0 of channel 0, B4 is sample 4 of channel 1, and so on.
Data in the transfer buffer is stored in little-endian format, so it can be easily processed by systems based
on x86, x64 or others.

Sample format
The 16 bit D/A samples are stored in twos complement as a 16 bit signed data word. 16 bit resolution means that data is ranging from -
32768…to…+32767. Data is stored in little-endian format, the upper 8 bit come first and the lower 8 bit second.
A channel’s samples can contain also information for the synchronous digital output channels, with up to three digital channels combined
with the analog sample within one data word. When extracting the digital channels form the data word, the analog data will automatically
be shifted upwards, to not loose any gain information. The analog data is still in the same twos complement format.
Table 66: Spectrum API: data format and DAC resolution depending on selected mode and digital output modes
Standard Mode Digital outputs enabled Digital outputs enabled Digital outputs enabled
No embedded digital Bit 1 embedded digital Bit 2 embedded digital Bits 3 embedded digital Bits
Data bit 16 bit DAC resolution 15 bit DAC resolution 14 bit DAC resolution 13 bit DAC resolution
D15 DAx Bit 15 (MSB) Digital „Bit15“ of channel x Digital „Bit15“ of channel x Digital „Bit15“ of channel x
D14 DAx Bit 14 DAx Bit 15 (MSB) Digital „Bit14“ of channel x Digital „Bit14“ of channel x
D13 DAx Bit 13 DAx Bit 14 DAx Bit 15 (MSB) Digital „Bit13“ of channel x
| D12 | DAx Bit 12 | DAx Bit 13 | DAx Bit 14 | DAx Bit 15 (MSB) |
| --- | ---------- | ---------- | ---------- | ---------------- |
| D11 | DAx Bit 11 | DAx Bit 12 | DAx Bit 13 | DAx Bit 14       |
| D10 | DAx Bit 10 | DAx Bit 11 | DAx Bit 12 | DAx Bit 13       |
| D9  | DAx Bit 9  | DAx Bit 10 | DAx Bit 11 | DAx Bit 12       |
| D8  | DAx Bit 8  | DAx Bit 9  | DAx Bit 10 | DAx Bit 11       |
| D7  | DAx Bit 7  | DAx Bit 8  | DAx Bit 9  | DAx Bit 10       |
| D6  | DAx Bit 6  | DAx Bit 7  | DAx Bit 8  | DAx Bit 9        |
| D5  | DAx Bit 5  | DAx Bit 6  | DAx Bit 7  | DAx Bit 8        |
| D4  | DAx Bit 4  | DAx Bit 5  | DAx Bit 6  | DAx Bit 7        |
| D3  | DAx Bit 3  | DAx Bit 4  | DAx Bit 5  | DAx Bit 6        |
| D2  | DAx Bit 2  | DAx Bit 3  | DAx Bit 4  | DAx Bit 5        |
| D1  | DAx Bit 1  | DAx Bit 2  | DAx Bit 3  | DAx Bit 4        |
D0 DAx Bit 0 (LSB) DAx Bit 1 (LSB) DAx Bit 2 (LSB) DAx Bit 3 (LSB)

Hardware data conversion
The data conversion modes allow the conversion of input data in hardware. This is especially usefull when replaying previously recorded
data of acquisition cards with either 15 bit, 14 bit or 12 bit resolution. The conversion takes place in hardware and therefore avoids a pos-
sible time consuming shift in the user application software.
Table 67: Spectrum API: hardware data conversion registers and available register settings
| Register |     | Value Direction | Description |     |
| -------- | --- | --------------- | ----------- | --- |
SPC_AVAILDATACONVERSION 201401 read Bitmask, in which all bits of the below mentioned data conversion modes are set, if available.
SPC_DATACONVERSION 201400 read/write Defines the used global hardware data conversion mode for all channels or reads out the currently
selected one.
(c) Spectrum Instrumentation GmbH 111

Generation modes Hardware data conversion
SPCM_DC_NONE 0h 16 bit input data is assumed and no hardware data conversion will be done.
SPCM_DC_12BIT_TO_16BIT 4h 12 bit input data is assumed and all samples of all currently active channels will be logically shifted upwards to use
the available 16 bit DAC resolution.
SPCM_DC_14BIT_TO_16BIT 8h 14 bit input data is assumed and all samples of all currently active channels will be logically shifted upwards to use
the available 16 bit DAC resolution.
SPCM_DC_15BIT_TO_16BIT 10h 15 bit input data is assumed and all samples of all currently active channels will be logically shifted upwards to use
the available 16 bit DAC resolution.
The hardware data conversion shifts the 16bit data words no matter what their content is or what channel they belong to. In case
that you would like to replay also some digital data from a previous recording included within the samples, the added width of the
digital data would have to be taken into account.
For example when replaying a recording from an M4i.4420 card with one digital bit included, you can either use no data conversion and
replay that digital bit through your generators X0, X1 or X2 line by selecting SPCM_DC_NONE for the data conversion and as such treating
that sample as 16bit. Additionally you select the digital output of one bit accordingly as described in the „Multi Purpose I/O Lines“ section
later in this manual, which will properly split the in this case 15 bit analog data and the 1 bit digital data.
Or in case, that you want to get rid of the recorded digital bits and output only the pure analog data, you would select a data conversion of
SPCM_DC_15BIT_TO_16BIT and hence treat this sample as 15 bit.
(c) Spectrum Instrumentation GmbH 112

Clock generation Overview
