Mode Gated Replay
The Gated Replay mode allows the data generation controlled by an exter-
nal or an internal gate signal. Data will only be replayed if the programmed
gate condition is true.
This chapter will explain all the necessary software register to set up the
card for Gated Replay properly.
The section on the allowed trigger modes deals with detailed description on Image 71: Gated Replay timing diagram in relation to gate signal
the different trigger events and the resulting gates.
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
Generation Modes
Standard Mode
Data will be replayed as long as the gate signal fulfills the programmed gate condition. At the end of the gate interval the replay will be
stopped and the card will pause until another gates signal appears. If loops (SPC_LOOPS) is set to 1 the card stops immediately as soon as
the total amount of data (SPC_MEMSIZE) has been replayed. In that case the last gate segment is ended by the expiring memory size counter
and not by the gate end signal. If loops is set to zero the Gated Replay mode will run in a continuous loop until explicitly stopped by user. If
the replay reaches the end of the programmed memory it will start again at the beginning with no gap in between.
The table below shows the register for enabling Gated Sampling. For detailed information on how to setup and start the standard acquisition
mode please refer to the according chapter earlier in this manual.
Table 121: Spectrum API: card mode register and settings for Gated Replay standard mode
Register Value Direction Description
SPC_CARDMODE 9500 read/write Defines the used operating mode
SPC_REP_STD_GATE 400h Enables Gated Sampling for standard acquisition.
The total number of samples to be replayed from the on-board memory in standard mode is defined by the SPC_MEMSIZE register.
Table 122: Spectrum API: memsize and loops register and register settings for Gated Replay mode
Register Value Direction Description
SPC_MEMSIZE 10000 read/write Defines the total number of samples to be replayed.
SPC_LOOPS 10020 read/write Defines the number of gates to be replayed
0 Replay will be infinite until the user stops it. When replay reaches the end of programmed memory it will start from the
beginning with no gap.
1 The complete memory is replayed once. The last gate segment is cut off when end of memory is reached.
Examples of Standard Standard Gated Replay with the use of SPC_LOOPS parameter
To keep the diagram easy to read there’s no delay shown in here and there’s also only a very small number of samples shown. Any further
restrictions are described later in this chapter.
Image 72: timing diagram of Gated Replay mode depending on different loops settings
FIFO Mode
The Gated Replay in FIFO mode is similar to the Gated Replay in standard mode. The replay can either run until the user stops it by software
(infinite replay, loops = 0) or until a programmed number of gates has been played (loops = 1). The data is written continuously by the driver
(c) Spectrum Instrumentation GmbH 140

Mode Gated Replay Limits of segment size, memory size
and can be either online calculated or loaded from hard disk. The table below shows the dedicated register for enabling Gated Sampling in
FIFO mode. For detailed information how to setup and start the card in FIFO mode please refer to the according chapter earlier in this manual.
Table 123: Spectrum API: card mode register and Gated Replay FIFO mode settings
| Register     |                   |     | Value |     | Direction                           | Description                     |     |     |     |
| ------------ | ----------------- | --- | ----- | --- | ----------------------------------- | ------------------------------- | --- | --- | --- |
| SPC_CARDMODE |                   |     | 9500  |     | read/write                          | Defines the used operating mode |     |     |     |
|              | SPC_REP_FIFO_GATE |     | 2000h |     | Enables Gated Replay with FIFO mode |                                 |     |     |     |
The number of gates to be replayed must be set separately with the register shown in the following table:
Table 124: Spectrum API: Gated Replay FIFO mode loops register settings
| Register  |               |     | Value |     | Direction                                                             | Description                                |     |     |     |
| --------- | ------------- | --- | ----- | --- | --------------------------------------------------------------------- | ------------------------------------------ | --- | --- | --- |
| SPC_LOOPS |               |     | 10020 |     | read/write                                                            | Defines the number of gates to be replayed |     |     |     |
|           | 0             |     |       |     | Replay will be infinite until the user stops it or an underrun occurs |                                            |     |     |     |
|           | 1 … [4Gi - 1] |     |       |     | Defines the total gates to be replayed.                               |                                            |     |     |     |
Examples of Fifo Gated Replay with the use of SPC_LOOPS parameter
To keep the diagram easy to read there’s no delay shown in here and there’s also only a very small number of samples shown. Any further
restrictions are described later in this chapter.
Image 73: timing diagram of Gated Replay FIFO mode depending on different loops settings

Limits of segment size, memory size
The maximum memory size parameter is only limited by the number of activated channels and by the amount of installed memory. Please
keep in mind that each sample needs 2 bytes of memory to be stored.
Due to the internal organization of the card memory there is a certain stepsize when setting these values that has to be taken into account.
The following table gives you an overview of all limits concerning memory size, segment size and loops. The table shows all values in relation
to the installed memory size in samples. If more memory is installed the maximum memory size figures will increase according to the complete
installed memory:
Table 125: Spectrum API: limits of segment size, memory size and loops registers depending on selected mode
| Activated | Used            |     | Memory size |      | Segment size    |          |          | Loops     |      |
| --------- | --------------- | --- | ----------- | ---- | --------------- | -------- | -------- | --------- | ---- |
| Channels  | Mode            |     | SPC_MEMSIZE |      | SPC_SEGMENTSIZE |          |          | SPC_LOOPS |      |
|           |                 | Min | Max         | Step | Min             | Max      | Step Min | Max       | Step |
| 1 channel | Standard Single | 32  | Mem         | 32   |                 | not used | 0 ()    | 4Gi - 1   | 1    |
|           | Single Restart  | 32  | Mem         | 32   |                 | not used | 0 ()    | 4Gi - 1   | 1    |
|           | Standard Multi  | 32  | Mem         | 32   | 16              | Mem/2 16 | 0 ()    | 1         | 1    |
0 ()
|            | Standard Gate   | 32  | Mem      | 32  |     | not used    |       | 1       | 1   |
| ---------- | --------------- | --- | -------- | --- | --- | ----------- | ----- | ------- | --- |
|            | FIFO Single     |     | not used |     | 16  | 8Gi - 16 16 | 0 () | 4Gi - 1 | 1   |
|            | FIFO Multi      |     | not used |     | 16  | Mem/2 16    | 0 () | 4Gi - 1 | 1   |
|            | FIFO Gate       |     | not used |     |     | not used    | 0 () | 4Gi - 1 | 1   |
| 2 channels | Standard Single | 32  | Mem/2    | 32  |     | not used    | 0 () | 4Gi - 1 | 1   |
0 ()
|     | Single Restart | 32  | Mem/2    | 32  |     | not used    |       | 4Gi - 1 | 1   |
| --- | -------------- | --- | -------- | --- | --- | ----------- | ----- | ------- | --- |
|     | Standard Multi | 32  | Mem/2    | 32  | 16  | Mem/4 16    | 0 () | 1       | 1   |
|     | Standard Gate  | 32  | Mem/2    | 32  |     | not used    | 0 () | 1       | 1   |
|     | FIFO Single    |     | not used |     | 16  | 8Gi - 16 16 | 0 () | 4Gi - 1 | 1   |
|     | FIFO Multi     |     | not used |     | 16  | Mem/4 16    | 0 () | 4Gi - 1 | 1   |
0 ()
|            | FIFO Gate       |     | not used |     |     | not used |       | 4Gi - 1 | 1   |
| ---------- | --------------- | --- | -------- | --- | --- | -------- | ----- | ------- | --- |
| 4 channels | Standard Single | 32  | Mem/4    | 32  |     | not used | 0 () | 4Gi - 1 | 1   |
|            | Single Restart  | 32  | Mem/4    | 32  |     | not used | 0 () | 4Gi - 1 | 1   |
|            | Standard Multi  | 32  | Mem/4    | 32  | 16  | Mem/8 16 | 0 () | 1       | 1   |
|            | Standard Gate   | 32  | Mem/4    | 32  |     | not used | 0 () | 1       | 1   |
0 ()
|     | FIFO Single |     | not used |     | 16  | 8Gi - 16 16 |       | 4Gi - 1 | 1   |
| --- | ----------- | --- | -------- | --- | --- | ----------- | ----- | ------- | --- |
|     | FIFO Multi  |     | not used |     | 16  | Mem/8 16    | 0 () | 4Gi - 1 | 1   |
|     | FIFO Gate   |     | not used |     |     | not used    | 0 () | 4Gi - 1 | 1   |
All figures listed here are given in samples. An entry of [8Ki - 16] means [8 KibiSamples - 16] = [8192 - 16] = 8176 samples.
(c) Spectrum Instrumentation GmbH 141

Mode Gated Replay Trigger
The given memory and memory / divider figures depend on the installed on-board memory as listed below:
Installed Memory
2 GiSample
Mem 2 GiSample
Mem / 2 1 GiSample
Mem / 4 512 MiSample
Mem / 8 256 MiSample
Please keep in mind that this table shows all values at once. Only the absolute maximum and minimum values are shown. There might be
additional limitations. Which of these values is programmed depends on the used mode. Please read the detailed documentation of the mode.
Trigger
Detailed description of the external analog trigger modes
For all external analog trigger modes shown below, either the OR mask or the AND must contain the external trigger to activate the external
input as trigger source:.
Table 126: Spectrum API: trigger mask registers and available register settings
Register Value Direction Description
SPC_TRIG_ORMASK 40410 read/write Defines the events included within the trigger OR mask of the card.
SPC_TRIG_ANDMASK 40430 read/write Defines the events included within the trigger AND mask of the card.
SPC_TMASK_EXT0 2h Enables the main external (analog) trigger 0 for the mask.
SPC_TMASK_EXT1 4h Enables the secondary external (analog) trigger 0 for the mask.
The following pages explain the available modes in detail. All modes that only require one single trigger level are available for both external
trigger inputs. All modes that require two trigger levels are only available for the main external trigger input (Ext0).
Trigger on positive edge
The trigger input is continuously sampled with the selected
sample rate. If the programmed trigger level is crossed by
the trigger signal from lower values to higher values (rising
edge) then the gate starts.
When the signal crosses the programmed trigger level from
higher values to lower values (falling edge) then the gate
will stop.
As this mode is purely edge-triggered, the high level at the
cards start time does not trigger the board.
Table 127: Spectrum API: trigger register settings for trigger on positive edge
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_POS 1h
SPC_TRIG_EXT1_MODE 40511 read/write SPC_TM_POS 1h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the desired trigger level in mV mV
Trigger on negative edge
The trigger input is continuously sampled with the selected
sample rate. If the programmed trigger level is crossed by
the trigger signal from higher values to lower values (falling
edge) then the gate starts.
When the signal crosses the programmed trigger from low-
er values to higher values (rising edge) then the gate will
stop.
As this mode is purely edge-triggered, the low level at the
cards start time does not trigger the board.
(c) Spectrum Instrumentation GmbH 142

Mode Gated Replay Trigger
Table 128: Spectrum API: trigger register settings for trigger on negative edge
| Register           | Value Direction  | set to     | Value |
| ------------------ | ---------------- | ---------- | ----- |
| SPC_TRIG_EXT0_MODE | 40510 read/write | SPC_TM_NEG | 2h    |
| SPC_TRIG_EXT1_MODE | 40511 read/write | SPC_TM_NEG | 2h    |
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the desired trigger level in mV mV
(c) Spectrum Instrumentation GmbH 143

Mode Gated Replay Trigger
Re-arm trigger on positive edge
The trigger input is continuously sampled with the selected
sample rate. If the programmed re-arm level is crossed from
lower to higher values, the trigger engine is armed and
waiting for trigger. If the programmed trigger level is
crossed by the trigger signal from lower values to higher
values (rising edge) then the gate starts will be detected and
the trigger engine will be disarmed. A new trigger event is
only detected if the trigger engine is armed again.
If the programmed trigger level is crossed by the external
signal from higher values to lower values (falling edge) the
gate stops.
The re-arm trigger modes can be used to prevent the board from triggering on wrong edges in noisy signals.
Table 129: Spectrum API: trigger register settings for re-arm trigger on positive edge
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_POS | SPC_TM_REARM 01000001h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the desired trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Defines the re-arm level in mV mV
Re-arm trigger on negative edge
The trigger input is continuously sampled with the selected
sample rate. If the programmed re-arm level is crossed from
higher to lower values, the trigger engine is armed and
waiting for trigger. If the programmed trigger level is
crossed by the trigger signal from higher values to lower
values (falling edge) then the gate starts and the trigger en-
gine will be disarmed. A new trigger event is only detected,
if the trigger engine is armed again.
If the programmed trigger level is crossed by the external
signal from lower values to higher values (rising edge) the
gate stops.
The re-arm trigger modes can be used to prevent the board from triggering on wrong edges in noisy signals.
Table 130: Spectrum API: trigger register settings for re-arm trigger on negative edge
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_NEG | SPC_TM_REARM 01000002h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Defines the re-arm level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the desired trigger level in mV mV
Window trigger for entering signals
The trigger input is continuously sampled with the selected
sample rate. The upper and the lower level define a win-
dow.
When the signal enters the window from the outside to the
inside, the gate will start. When the signal leaves the win-
dow from the inside to the outside, the gate will stop.
As this mode is purely edge-triggered, the signal outside the
window at the cards start time does not trigger the board.
Table 131: Spectrum API: trigger register settings for window trigger on entering signals
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_WINENTER 00000020h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the lower trigger level in mV mV
(c) Spectrum Instrumentation GmbH 144

Mode Gated Replay Trigger
Window trigger for leaving signals
The trigger input is continuously sampled with the selected
sample rate. The upper and the lower level define a win-
dow. Every time the signal leaves the window from the in-
side, a trigger event will be detected.
When the signal leaves the window from the inside to the
outside, the gate will start. When the signal enters the win-
dow from the outside to the inside, the gate will stop.
As this mode is purely edge-triggered, the signal within the
window at the cards start time does not trigger the board.
Table 132: Spectrum API: trigger register settings for window trigger on leaving signals
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_WINLEAVE 00000040h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the lower trigger level in mV mV
High level trigger
The external input is continuously sampled with the selected
sample rate. If the signal is equal or higher than the pro-
grammed trigger level the gate starts.
When the signal is lower than the programmed trigger level
the gate will stop.
As this mode is level-triggered, the high level at the cards start
time does trigger the board.
Table 133: Spectrum API: trigger register settings for high-level trigger
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_HIGH 00000008h
SPC_TRIG_EXT1_MODE 40511 read/write SPC_TM_HIGH 00000008h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
Low level trigger
The external input is continuously sampled with the selected
sample rate. If the signal is equal or lower than the pro-
grammed trigger level the gate starts.
When the signal is higher than the programmed trigger level
the gate will stop.
As this mode is level-triggered, the high level at the cards start
time does trigger the board.
Table 134: Spectrum API: trigger register settings for low-level trigger
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_LOW 00000010h
SPC_TRIG_EXT1_MODE 40511 read/write SPC_TM_LOW 00000010h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
(c) Spectrum Instrumentation GmbH 145

Mode Gated Replay Trigger
In window trigger
The external input is continuously sampled with the selected
sample rate. The upper and the lower level define a window.
When the signal enters the window from the outside to the in-
side, the gate will start.
When the signal leaves the window from the inside to the out-
side, the gate will stop.
As this mode is level-triggered, the signal inside the window
at the cards start time does trigger the board.
Table 135: Spectrum API: trigger register settings for in-window trigger
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_INWIN 00000080h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the lower trigger level in mV mV
Outside window trigger
The external input is continuously sampled with the selected
sample rate. The upper and the lower level define a window.
When the signal leaves the window from the inside to the out-
side, the gate will start.
When the signal enters the window from the outside to the in-
side, the gate will stop.
As this mode is level-triggered, the signal outside the window
at the cards start time does trigger the board.
Table 136: Spectrum API: trigger register settings for outside-window trigger
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_OUTSIDEWIN 00000100h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the lower trigger level in mV mV
Programming examples
The following examples shows how to set up the card for Gated Replay in standard mode for Gated Replay in FIFO mode.
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_STD_GATE); // Enables Standard Gated Replay
spcm_dwSetParam_i64 (hDrv, SPC_MEMSIZE, 8192); // Set the total memsize for replay to 8192 samples
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_MODE, SPC_TM_POS); // Set triggermode to ext. TTL rising edge
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_EXT0); // and enable it within the trigger OR-mask
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REP_FIFO_GATE); // Enables FIFO Gated Replay
pcm_dwSetParam_i64 (hDrv, SPC_LOOP, 1024); // 1024 gates will be replayed
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_MODE, SPC_TM_NEG);// Set triggermode to ext. TTL falling edge
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_EXT0);// and enable it within the trigger OR-mask
Programming the behavior in pauses and after replay
Usually the used outputs of the analog generation boards are set to zero level after replay. This is in most cases adequate. In some cases it
can be necessary to hold the last sample, to output the maximum positive level or maximum negative level after replay. The stoplevel will stay
(c) Spectrum Instrumentation GmbH 146

Mode Gated Replay Trigger
on the defined level until the next output will be made. With the following registers you can define the behavior after replay, when using the
cards in AWG mode:
Table 137: Spectrum API: stop level register and register settings
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
Table 138: Spectrum API: custom stop level registers
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
(c) Spectrum Instrumentation GmbH 147

Sequence Mode Theory of operation
