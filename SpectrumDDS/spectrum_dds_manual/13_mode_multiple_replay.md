Mode Multiple Replay
The Multiple Replay mode allows the generation of data blocks with multi-
ple trigger events without restarting the hardware.
The on-board memory will be divided into several segments of the same
size. On each trigger event one segment of data will be replayed.
As this mode is totally controlled in hardware there is a very small re-arm
time from end of one segment until the trigger detection is enabled again.
You’ll find that re-arm time in the technical data section of this manual.
Image 67: Multiple Replay output and trigger timing diagram
The following table shows the register for defining the structure of the seg-
ments to be replayed with each trigger event.
Table 114: Spectrum API: segment size register for multiple replay mode
Register Value Direction Description
SPC_SEGMENTSIZE 10010 read/write Size of one Multiple Replay segment: the total number of samples to be replayed per channel after
detection of one trigger event.
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
Trigger Modes
When using Multiple Recording all of the card’s trigger modes can be used including the software trigger. For detailed information on the
available trigger modes, please take a look at the relating chapter earlier in this manual.
Programming examples
The following example shows how to set up the card for Multiple Replay in standard mode.
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_STD_MULTI); // Enables Standard Multiple Replay
spcm_dwSetParam_i64 (hDrv, SPC_SEGMENTSIZE, 1024); // Set the segment size to 1024 samples
spcm_dwSetParam_i64 (hDrv, SPC_MEMSIZE, 4096); // Set the total memsize for recording to 4096 samples
// so that actually four segments will be replayed
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_MODE, SPC_TM_POS); // Set trig mode to ext. TTL mode (rising edge)
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_EXT0); // and enable it within the trigger OR-mask
The following example shows how to set up the card for Multiple Replay in FIFO mode.
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_FIFO_MULTI); // Enables FIFO Multiple Replay
spcm_dwSetParam_i64 (hDrv, SPC_SEGMENTSIZE, 2048); // Set the segment size to 2048 samples
spcm_dwSetParam_i64 (hDrv, SPC_LOOPS 256); // 256 segments will be replayed
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_MODE, SPC_TM_NEG); // Set trig mode to ext. TTL mode (falling edge)
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_EXT0); // and enable it within the trigger OR-mask
(c) Spectrum Instrumentation GmbH 136

Mode Multiple Replay Replay modes
Replay modes
Standard Mode
With every detected trigger event one data block is replayed. The length of one multiple replay segment is set by the value of the segment
size register SPC_SEGMENTSIZE. The total amount of samples to be replayed is defined by the memsize register.
Memsize must be set to a a multiple of the segment size. The table below shows the register for enabling Multiple Recording. For detailed
information on how to setup and start the standard replay mode please refer to the according chapter earlier in this manual.
Table 115: Spectrum API: card mode register and multiple replay settings
Register Value Direction Description
SPC_CARDMODE 9500 read/write Defines the used operating mode
SPC_REP_STD_MULTI 200h Enables Multiple Replay for standard replay.
The total number of samples to be replayed from the on-board memory in standard mode is defined by the SPC_MEMSIZE register. When
using the SPC_LOOPS parameter one can further program whether all segments should be replayed once or continuously.
Table 116: Spectrum API: memory and loop registers with related multiple replay settings
Register Value Direction Description
SPC_MEMSIZE 10000 read/write Defines the total number of samples to be replayed.
SPC_LOOPS 10020 read/write When writing a 1 the complete memory is replayed once, when writing a zero the replay continues
from the beginning forever.
0 Replay will be infinite until the user stops it. When replay reaches the end of programmed memory it will start from the
beginning again.
1 The complete memory is replayed once.
Standard replay mode with the use of SPC_LOOPS
Image 68: timing diagram of multiple replay mode depending on loops settings
FIFO Mode
The Multiple Replay in FIFO mode is similar to the Multiple Replay in standard mode. In contrast to the standard mode it is not necessary to
program the number of samples to be replayed. The replay is running until the user stops it. The data is written block by block by the driver
as described under single FIFO mode example earlier in this manual. These blocks can be online calculated or loaded from hard disk. This
mode significantly reduces the amount of data to be transferred on the PCI bus as gaps with no significant output did not have to be trans-
ferred. This enables you to use faster sample rates then you would be able to in FIFO mode without Multiple Recording.
The table below shows the dedicated register for enabling Multiple Replay. For detailed information how to setup and start the board in FIFO
mode please refer to the according chapter earlier in this manual.
Image 69: Spectrum API: card mode register and multiple replay FIFO mode settings
Register Value Direction Description
SPC_CARDMODE 9500 read/write Defines the used operating mode
SPC_REP_FIFO_MULTI 1000h Enables Multiple Replay for FIFO mode.
The number of segments to be replayed must be set separately with the register shown in the following table:
Table 117: Spectrum API: loops register settings when using Multiple Replay FIFO mode
Register Value Direction Description
SPC_LOOPS 10020 read/write Defines the number of segments to be replayed
0 Replay will be infinite until the user stops it.
1 … [4Gi - 1] Defines the total segments to be replayed.
(c) Spectrum Instrumentation GmbH 137

Mode Multiple Replay Limits of segment size, memory size
Fifo replay mode with the use of SPC_LOOPS
Image 70: timing diagram of Multiple Replay FIFO mode with different loops settings

Limits of segment size, memory size
The maximum memory size parameter is only limited by the number of activated channels and by the amount of installed memory. Please
keep in mind that each sample needs 2 bytes of memory to be stored.
Due to the internal organization of the card memory there is a certain stepsize when setting these values that has to be taken into account.
The following table gives you an overview of all limits concerning memory size, segment size and loops. The table shows all values in relation
to the installed memory size in samples. If more memory is installed the maximum memory size figures will increase according to the complete
installed memory:
Table 118: Spectrum API: limits of segment size, memory size and loops registers depending on selected mode
| Activated  | Used            | Memory size |      | Segment size    |      | Loops         |      |
| ---------- | --------------- | ----------- | ---- | --------------- | ---- | ------------- | ---- |
| Channels   | Mode            | SPC_MEMSIZE |      | SPC_SEGMENTSIZE |      | SPC_LOOPS     |      |
|            |                 | Min Max     | Step | Min Max         | Step | Min Max       | Step |
| 1 channel  | Standard Single | 32 Mem      | 32   | not used        |      | 0 () 4Gi - 1 | 1    |
|            | Single Restart  | 32 Mem      | 32   | not used        |      | 0 () 4Gi - 1 | 1    |
|            | Standard Multi  | 32 Mem      | 32   | 16 Mem/2        | 16   | 0 () 1       | 1    |
|            | Standard Gate   | 32 Mem      | 32   | not used        |      | 0 () 1       | 1    |
|            | FIFO Single     | not used    |      | 16 8Gi - 16     | 16   | 0 () 4Gi - 1 | 1    |
|            | FIFO Multi      | not used    |      | 16 Mem/2        | 16   | 0 () 4Gi - 1 | 1    |
|            | FIFO Gate       | not used    |      | not used        |      | 0 () 4Gi - 1 | 1    |
| 2 channels | Standard Single | 32 Mem/2    | 32   | not used        |      | 0 () 4Gi - 1 | 1    |
|            | Single Restart  | 32 Mem/2    | 32   | not used        |      | 0 () 4Gi - 1 | 1    |
|            | Standard Multi  | 32 Mem/2    | 32   | 16 Mem/4        | 16   | 0 () 1       | 1    |
|            | Standard Gate   | 32 Mem/2    | 32   | not used        |      | 0 () 1       | 1    |
|            | FIFO Single     | not used    |      | 16 8Gi - 16     | 16   | 0 () 4Gi - 1 | 1    |
|            | FIFO Multi      | not used    |      | 16 Mem/4        | 16   | 0 () 4Gi - 1 | 1    |
|            | FIFO Gate       | not used    |      | not used        |      | 0 () 4Gi - 1 | 1    |
| 4 channels | Standard Single | 32 Mem/4    | 32   | not used        |      | 0 () 4Gi - 1 | 1    |
|            | Single Restart  | 32 Mem/4    | 32   | not used        |      | 0 () 4Gi - 1 | 1    |
|            | Standard Multi  | 32 Mem/4    | 32   | 16 Mem/8        | 16   | 0 () 1       | 1    |
|            | Standard Gate   | 32 Mem/4    | 32   | not used        |      | 0 () 1       | 1    |
|            | FIFO Single     | not used    |      | 16 8Gi - 16     | 16   | 0 () 4Gi - 1 | 1    |
|            | FIFO Multi      | not used    |      | 16 Mem/8        | 16   | 0 () 4Gi - 1 | 1    |
|            | FIFO Gate       | not used    |      | not used        |      | 0 () 4Gi - 1 | 1    |
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

Programming the behavior in pauses and after replay
Usually the used outputs of the analog generation boards are set to zero level after replay. This is in most cases adequate. In some cases it
can be necessary to hold the last sample, to output the maximum positive level or maximum negative level after replay. The stoplevel will stay
(c) Spectrum Instrumentation GmbH 138

Mode Multiple Replay Limits of segment size, memory size
on the defined level until the next output will be made. With the following registers you can define the behavior after replay, when using the
cards in AWG mode:
Table 119: Spectrum API: stop level register and register settings
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
Table 120: Spectrum API: custom stop level registers
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
(c) Spectrum Instrumentation GmbH 139

Mode Gated Replay Generation Modes
