Trigger modes and appendant registers
General Description
The trigger modes of the Spectrum M4i/M4x series A/D and D/A cards are very extensive and give you the possibility to detect nearly any
trigger event you can think of.
You can choose between more than 10 external trigger modes and up to 20 internal trigger modes (on analog acquisition cards) including
software and channel trigger, depending on your type of board. Many of the channel trigger modes can be independently set for each input
channel (on A/D boards only) resulting in a even bigger variety of modes. This chapter is about to explain all of the different trigger modes
and setting up the card’s registers for the desired mode.
Trigger Engine Overview
Image 58: Trigger Engine Overview. Red marked parts not available on all card types
The trigger engine of the M4i/M4x card series allows to combine several different trigger sources with OR and AND combination, with a
trigger delay or even with an OR combination across several cards when using the Star-Hub option. The above drawing gives a complete
overview of the trigger engine and shows all possible features that are available.
On A/D cards each analog input channel has two trigger level comparators to detect edges as well as windowed triggers. All card types
have a total of two different additional external trigger sources. One main trigger source (Ext0, labelled Trg0 on front panel) which also has
two analog level comparators also allowing to use edge and windowed trigger detection and one secondary analog trigger (Ext1, labelled
Trg1 on front panel) with one analog level comparator. Additionally three multi purpose in/outputs that can be software programmed to
either inputs or outputs some extended status signals.
The Enable trigger allows the user to enable or disable all trigger sources (including channel trigger on A/D cards and external trigger) with
a single software command. The enable trigger command will not work on force trigger.
When the card is waiting for a trigger event, either a channel trigger or an external trigger the force trigger command allows to force a
trigger event with a single software command. The force trigger overrides the enable trigger command.
Before the trigger event is finally generated, it is wired through a programmable trigger delay. This trigger delay will also work when used
in a synchronized system thus allowing each card to individually delay its trigger recognition.
(c) Spectrum Instrumentation GmbH 117

Trigger modes and appendant registers Trigger masks
Trigger masks
Trigger OR mask
The purpose of this passage is to explain the trigger OR mask (see
left figure) and all the appendant software registers in detail.
The OR mask shown in the overview before as one object, is separat-
ed into two parts: a general OR mask for main external trigger (ex-
ternal analog window trigger), the secondary external trigger
(external analog comparator trigger, the various PXI triggers (availa-
ble on M4x PXIe cards only) and software trigger and a channel OR
mask.
Image 59: trigger engine overview with trigger OR mask shown
Every trigger source of the M4i/M4x series cards is wired to one of the above
mentioned OR masks. The user then can program which trigger source will be
recognized, and which one won’t.
This selection for the general mask is realized with theSPC_TRIG_ORMASK
register in combination with constants for every possible trigger source.
This selection for the channel mask (A/D cards only) is realized with
theSPC_TRIG_CH_ORMASK0 register in combination with constants for
every possible channel trigger source.
In either case the sources are coded as a bitfield, so that they can be combined
by one access to the driver with the help of a bitwise OR.
If no input is enabled, the output will be a logic “true”, to not block the follow-
ing static AND mask.
The table below shows the relating register for the general OR mask and the
possible constants that can be written to it. Image 60: trigger engine OR mask details
Table 78: Spectrum API: general trigger OR mask register and available settings
Register Value Direction Description
SPC_TRIG_AVAILORMASK 40400 read Bitmask, in which all bits of the below mentioned sources for the OR mask are set, if available.
SPC_TRIG_ORMASK 40410 read/write Defines the events included within the trigger OR mask of the card.
SPC_TMASK_NONE 0 No trigger source selected
SPC_TMASK_SOFTWARE 1h Enables the software trigger for the OR mask. The card will trigger immediately after start.
SPC_TMASK_EXT0 2h Enables the external (analog window) trigger 0 (labelled Trg0 on front panel) for the OR mask. The card will trigger
when the programmed condition for this input is valid.
SPC_TMASK_EXT1 4h Enables the external (analog comparator) trigger 1 (labelled Trg1 on front panel)for the OR mask. The card will trig-
ger when the programmed condition for this input is valid.
SPC_TMASK_PXI0 100000h Enables the PXI_TRIG0 for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI1 200000h Enables the PXI_TRIG1 for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI2 400000h Enables the PXI_TRIG2 for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI3 800000h Enables the PXI_TRIG3 for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI4 1000000h Enables the PXI_TRIG4 for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI5 2000000h Enables the PXI_TRIG5 for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI6 4000000h Enables the PXI_TRIG6 for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI7 8000000h Enables the PXI_TRIG7 for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXISTAR 10000000h Enables the PXISTAR line for the OR mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXIDSTARB 20000000h Enables the PXI_DSTARB for the OR mask. The card will trigger when the signal on this input is HIGH.
Please note that as default the SPC_TRIG_ORMASK is set to SPC_TMASK_SOFTWARE. When not using any trig-
ger mode requiring values in the SPC_TRIG_ORMASK register, this mask should explicitely cleared, as other-
wise the software trigger will override other modes.
The following example shows, how to setup the OR mask, for the two external trigger inputs, ORing them together. When using just a single
trigger, only this particular trigger must be used in the OR mask register, respectively. As an example a simple edge detection has been
(c) Spectrum Instrumentation GmbH 118

Trigger modes and appendant registers Trigger masks
chosen for Ext1 input and a window edge detection has been chosen for Ext0 input. The explanation and a detailed description of the different
trigger modes for the external trigger inputs will be shown in the dedicated passage within this chapter.
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_LEVEL0, 1800); // lower Window Trigger level set to 1.8 V
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_LEVEL1, 2000); // upper Window Trigger level set to 2.0 V
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_MODE, SPC_TM_WINENTER);// Setting up main window trigger for entering
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT1_LEVEL0, 2500); // Trigger level set to 2.5 V
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT1_MODE, SPC_TM_POS); // Setting up secondary trigger for rising edges
// Enable both external triggers within the OR mask, hence ORing them together
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_EXT1 | SPC_TMASK_EXT0);
The table below is showing the registers for the channel OR mask (A/D cards only) and the possible constants that can be written to it.
Table 79: Spectrum API: channel trigger OR mask registers and available settings
Register Value Direction Description
SPC_TRIG_CH_AVAILORMASK0 40450 read Bitmask, in which all bits of the below mentioned sources/channels (0…7) for the channel OR mask
are set, if available.
SPC_TRIG_CH_ORMASK0 40460 read/write Includes the analog channels (0…7) within the channel trigger OR mask of the card.
SPC_TMASK0_CH0 00000001h Enables channel0 for recognition within the channel OR mask.
SPC_TMASK0_CH1 00000002h Enables channel1 for recognition within the channel OR mask.
SPC_TMASK0_CH2 00000004h Enables channel2 for recognition within the channel OR mask.
SPC_TMASK0_CH3 00000008h Enables channel3 for recognition within the channel OR mask.
The following example shows, how to setup the OR mask for channel trigger. As an example a simple edge detection has been chosen. The
explanation and a detailed description of the different trigger modes for the channel trigger modes will be shown in the dedicated passage
within this chapter.
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_NONE); // disable default software trigger
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_CH_ORMASK0, SPC_TMASK_CH0); // Enable channel0 trigger within the OR mask
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_CH0_LEVEL0, 0); // Trigger level is zero crossing
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_CH0_MODE, SPC_TM_POS); // Setting up channel trigger for rising edges
Trigger AND mask
The purpose of this passage is to explain the trigger AND mask (see
left figure) and all the additional software registers in detail.
The AND mask shown in the overview before as one object, is sepa-
rated into two parts: a general AND mask for external trigger and
software trigger and a channel AND mask.
Image 61: trigger engine overview with trigger AND mask shown
Every trigger source of the M4i/M4x series cards except the software
trigger is wired to one of the above mentioned AND masks. The user then can
program which trigger source will be recognized, and which one won’t.
This selection for the general mask is realized with theSPC_TRIG_ANDMASK
register in combination with constants for every possible trigger source.
This selection for the channel mask (A/D cards only) is realized with
theSPC_TRIG_CH_ANDMASK0 register in combination with constants for
every possible channel trigger source.
In either case the sources are coded as a bit-field, so that they can be com-
bined by one access to the driver with the help of a bitwise OR.
If no input is enabled, the output will be a logic “true”, to not block the follow-
ing static AND mask.
Image 62: trigger engine AND mask details
(c) Spectrum Instrumentation GmbH 119

Trigger modes and appendant registers Combining multiple trigger sources by OR/AND masks
The table below shows the relating register for the general AND mask and the possible constants that can be written to it.
Table 80: Spectrum API: general trigger AND mask registers and available settings
Register Value Direction Description
SPC_TRIG_AVAILANDMASK 40420 read Bit mask, in which all bits of the below mentioned sources for the AND mask are set, if available.
SPC_TRIG_ANDMASK 40430 read/write Defines the events included within the trigger AND mask of the card.
SPC_TMASK_NONE 0 No trigger source selected
SPC_TMASK_EXT0 2h Enables the external (analog window) trigger 0 (labelled Trg0 on front panel) for the AND mask. The card will trigger
when the programmed condition for this input is valid.
SPC_TMASK_EXT1 4h Enables the external (analog comparator) trigger 1 (labelled Trg1 on front panel) for the AND mask. The card will
trigger when the programmed condition for this input is valid.
SPC_TMASK_PXI0 100000h Enables the PXI_TRIG0 for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI1 200000h Enables the PXI_TRIG1 for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI2 400000h Enables the PXI_TRIG2 for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI3 800000h Enables the PXI_TRIG3 for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI4 1000000h Enables the PXI_TRIG4 for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI5 2000000h Enables the PXI_TRIG5 for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI6 4000000h Enables the PXI_TRIG6 for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI7 8000000h Enables the PXI_TRIG7 for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXISTAR 10000000h Enables the PXISTAR line for the AND mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXIDSTARB 20000000h Enables the PXI_DSTARB for the AND mask. The card will trigger when the signal on this input is HIGH.
The following example shows, how to setup the AND mask, for an external trigger. As an example a simple high level detection has been
chosen. When multiple external triggers shall be combined by AND, both of the external sources must be included in the AND mask register,
similar to the OR mask example shown before. The explanation and a detailed description of the different trigger modes for the external
trigger inputs will be shown in the dedicated passage within this chapter.
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_NONE); // disable default software trigger
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ANDMASK, SPC_TMASK_EXT0); // Enable external trigger within the AND mask
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_LEVEL0, 2000); // Trigger level is 2.0 V (2000 mV)
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_EXT0_MODE, SPC_TM_HIGH); // Setting up external trigger for HIGH level
The table below is showing the constants for the channel AND mask (A/D cards only) and all the constants for the different channels.
Table 81: Spectrum API: channel trigger AND mask registers and available settings
Register Value Direction Description
SPC_TRIG_CH_AVAILANDMASK0 40470 read Bitmask, in which all bits of the below mentioned sources/channels (0…7) for the channel AND mask
are set, if available.
SPC_TRIG_CH_ANDMASK0 40480 read/write Includes the analog or digital channels (0…7) within the channel trigger AND mask of the card.
SPC_TMASK0_CH0 00000001h Enables channel0 for recognition within the channel OR mask.
SPC_TMASK0_CH1 00000002h Enables channel1 for recognition within the channel OR mask.
SPC_TMASK0_CH2 00000004h Enables channel2 for recognition within the channel OR mask.
SPC_TMASK0_CH3 00000008h Enables channel3 for recognition within the channel OR mask.
The following example shows, how to setup the AND mask for a channel trigger. As an example a simple level detection has been chosen.
The explanation and a detailed description of the different trigger modes for the channel trigger modes will be shown in the dedicated pas-
sage within this chapter.
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_NONE); // disable default software trigger
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_CH_ANDMASK0, SPC_TMASK_CH0);// Enable channel0 trigger within AND mask
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_CH0_LEVEL0, 0); // channel level to detect is zero level
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_CH0_MODE, SPC_TM_HIGH); // Setting up ch0 trigger for HIGH levels
Combining multiple trigger sources by OR/AND masks
When combining multiple trigger sources with different trigger modes, it is recommended to add all trigger sources with modes generating
• “edge-events” (TM_POS, TM_NEG, TM_BOTH, etc.) to add to the OR-mask
• sources with modes generating “level-events” (TM_HIGH, TM_LOW ,etc.) to add to the AND-mask
Example for setting up a trigger combination where Ch0 acts as a “Gate/Enable” signal for edges to be detected on EXT0:
spcm_dwSetParam_i32 (hCard, SPC_TRIG_EXT0_MODE, SPC_TM_POS); // setup EXT0 to use rising edge
spcm_dwSetParam_i32 (hCard, SPC_TRIG_CH0_MODE, SPC_TM_HIGH); // setup Ch0 to use high level
// put EXT0 into OR mask and Ch0 into AND mask
spcm_dwSetParam_i32 (hCard, SPC_TRIG_ORMASK, SPC_TMASK_EXT0); // also disables default software trigger
spcm_dwSetParam_i32 (hCard, SPC_TRIG_CH_ANDMASK0, SPC_TMASK0_CH0);
(c) Spectrum Instrumentation GmbH 120

Trigger modes and appendant registers Software trigger
Example for setting up a trigger combination where EXT0 acts as a “Gate/Enable” signal for edges to be detected on either Ch0 or Ch1 or
Ch2:
spcm_dwSetParam_i32 (hCard, SPC_TRIG_EXT0_MODE, SPC_TM_HIGH); // setup EXT0 to use high level
spcm_dwSetParam_i32 (hCard, SPC_TRIG_CH0_MODE, SPC_TM_POS); // setup Ch0 to use rising edge
spcm_dwSetParam_i32 (hCard, SPC_TRIG_CH1_MODE, SPC_TM_NEG); // setup Ch1 to use falling edge
spcm_dwSetParam_i32 (hCard, SPC_TRIG_CH2_MODE, SPC_TM_BOTH); // setup Ch2 to use both edges
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_NONE); // disable default software trigger
// put EXT0 into AND mask and Ch0, Ch1 and Ch2 into OR mask
spcm_dwSetParam_i32 (hCard, SPC_TRIG_CH_ORMASK0, SPC_TMASK0_CH0 | SPC_TMASK0_CH1 | SPC_TMASK0_CH2);
spcm_dwSetParam_i32 (hCard, SPC_TRIG_ANDMASK SPC_TMASK_EXT0);
Software trigger
The software trigger is the easiest way of triggering any Spectrum
board. The acquisition or replay of data will start immediately af-
ter the card is started and the trigger engine is armed. The result-
ing delay upon start includes the time the board needs for its
setup and the time for recording the pre-trigger area (for acquisi-
tion cards).
For enabling the software trigger one simply has to include the
software event within the trigger OR mask, as the following table is showing:
Table 82: Spectrum API: software register and register setting for software trigger
Register Value Direction Description
SPC_TRIG_ORMASK 40410 read/write Defines the events included within the trigger OR mask of the card.
SPC_TMASK_SOFTWARE 1h Sets the trigger mode to software, so that the recording/replay starts immediately.
Example for setting up the software trigger:
spcm_dwSetParam_i32 (hDrv, SPC_TRIG_ORMASK, SPC_TMASK_SOFTWARE); // Internal software trigger mode is used
Force- and Enable trigger
In addition to the software trigger (free run) it is also possible to force a trigger event by software while the board is waiting for a real physical
trigger event. The forcetrigger command will only have any effect, when the board is waiting for a trigger event. The command for forcing
a trigger event is shown in the table below.
Issuing the forcetrigger command will every time only generate one trigger event. If for example using Multiple Recording that will result in
only one segment being acquired by forcetrigger. After execution of the forcetrigger command the trigger engine will fall back to the trigger
mode that was originally programmed and will again wait for a trigger event.
Table 83: Spectrum API: command register and force trigger command
Register Value Direction Description
SPC_M2CMD 100 write Command register of the M2i/M3i/M4i/M4x/M2p/M5i series cards.
M2CMD_CARD_FORCETRIGGER 10h Forces a trigger event if the hardware is still waiting for a trigger event.
The example shows, how to use the forcetrigger command:
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_FORCETRIGGER); // Force trigger is used.
It is also possible to enable (arm) or disable (disarm) the card’s whole triggerengine by software. By default the trigger engine is disabled.
Table 84: Spectrum API: command register and trigger enable/disable command
Register Value Direction Description
SPC_M2CMD 100 write Command register of the M2i/M3i/M4i/M4x/M2p/M5i series cards.
M2CMD_CARD_ENABLETRIGGER 8h Enables the trigger engine. Any trigger event will now be recognized.
M2CMD_CARD_DISABLETRIGGER 20h Disables the trigger engine. No trigger events will be recognized, except force trigger.
The example shows, how to arm and disarm the card’s trigger engine properly:
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_ENABLETRIGGER); // Trigger engine is armed.
...
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_DISABLETRIGGER); // Trigger engine is disarmed.
(c) Spectrum Instrumentation GmbH 121

Trigger modes and appendant registers Trigger delay
Trigger delay
All of the Spectrum M4i/M4x series cards allow the user to program
an additional trigger delay. As shown in the trigger overview section,
this delay is the last element in the trigger chain. Therefore the user
does not have to care for the sources when programming the trigger
delay.
As shown in the overview the trigger delay is located after the star-
hub connection meaning that every M4i card being synchronized
can still have its own trigger delay programmed. The Star-Hub will
combine the original trigger events before the result is being delayed.
The delay is programmed in samples. The resulting time delay will
therefore be [Programmed Delay] / [Sampling Rate].
Image 63: trigger engine overview with marked trigger delay stage
The following table shows the related register and the possible values. A value of 0 disables the trigger delay.
Table 85: Spectrum API: trigger delay registers and available settings
Register Value Direction Description
SPC_TRIG_AVAILDELAY 40800 read Contains the maximum available delay as a decimal integer value.
SPC_TRIG_AVAILDELAY_STEP 40801 read Returns the step size of the trigger delay register in sample clocks.
SPC_TRIG_DELAY 40810 read/write Defines the delay for the detected trigger events.
0 No additional delay will be added. The resulting internal delay is mentioned in the technical data section.
16…[8Gi -8] in steps of 16 (12, 14, 16 bit cards) Defines the additional trigger delay in number of sample clocks. The trigger delay can be programmed up to (8GiS-
amples - 16) = 8589934576. Step size is 16 samples for 12, 14, 16 bit cards.
32…[8Gi -32] in steps of 32 (8 bit cards) Defines the additional trigger delay in number of sample clocks. The trigger delay can be programmed up to (8GiS-
amples - 32) = 8589934560. Step size is 32 samples for 8 bit cards.
The example shows, how to use the trigger delay command:
spcm_dwSetParam_i64 (hDrv, SPC_TRIG_DELAY, 1984); // A detected trigger event will be
// delayed for 1984 sample clocks.
Using the delay trigger does not affect the ratio between pre trigger and post trigger recorded number of samples, but only shifts
the trigger event itself. For changing these values, please take a look in the relating chapter about „Acquisition Modes“.
Trigger Counter
The number of acquired trigger events is counted in hardware and can be read out while the acquisition is running or after the acquisition
has finished. The trigger events are counted both in standard mode as well as in FIFO mode.
Table 86: Spectrum API: trigger counter register and register return values
Register Value Direction Description
SPC_TRIGGERCOUNTER 200905 read Returns the number of trigger events that has been acquired since the acquisition start. The internal
trigger counter has 48 bits. It is therefore necessary to read out the trigger counter value with 64 bit
access or 2 x 32 bit access if the number of trigger events exceed the 32 bit range.
The trigger counter feature needs at least driver version V2.17 and firmware version V20 (M2i series), V10
(M3i series), V6 (M4i/M4x series) or V1 (M2p and M5i series). Please update the driver and the card firmware
to these versions to use this feature. Trying to use this feature without the proper firmware version will issue
a driver error.
On M2i and M3i cards, using the trigger counter information allows to determine how many Multiple Recording segments have
been acquired and can perform a memory flush by issuing Force trigger commands to read out all data. This is helpful if the number
of trigger events is not known at the start of the acquisition. In that case one will do the following steps:
• Program the maximum number of segments that one expects or use the FIFO mode with unlimited segments
• Set a timeout to be sure that there are no more trigger events acquired. Alternatively one can manually proceed as soon as it is clear from
the application that all trigger events have been acquired
• Read out the number of acquired trigger segments
• Issue a number of Force Trigger commands to fill the complete memory (standard mode) or to transfer the last FIFO block that contains
valid data segments
• Use the trigger counter value to split the acquired data into valid data with a real trigger event and invalid data with a force trigger event.
(c) Spectrum Instrumentation GmbH 122

Trigger modes and appendant registers Main external window trigger (Ext0/Trg0)
Main external window trigger (Ext0/Trg0)
The M4i/M4x series has one main external trigger input consisting
of an input stage with programmable termination and programmable
AC/DC coupling and two comparators that can be programmed in
the range of +/- 10000 mV. Using two comparators offers a wide
range of different trigger modes that are support like edge, level, re-
arm and window trigger.
The main external analog trigger can be easily combined with chan-
nel trigger or with the secondary external trigger being programmed
as an additional external trigger input. The programming of the
masks is shown in the chapters above.
The external trigger Ext0 is labelled Trg0 on the front-panel
Image 64: trigger engine overview with marked main external trigger Ext0/Trg0
Trigger Mode
Please find the main external (analog) trigger input modes below. A detailed description of the modes follows in the next chapters..
Table 87: Spectrum API: external trigger Ext0 registers and register settings
Register Value Direction Description
SPC_TRIG_EXT0_AVAILMODES 40500 read Bitmask showing all available trigger modes for external 0 (Ext0) = main analog trigger input
SPC_TRIG_EXT0_MODE 40510 read/write Defines the external trigger mode for the external SMA connector trigger input. The trigger need to
be added to either OR or AND mask input to be activated.
SPC_TM_NONE 00000000h Channel is not used for trigger detection. This is as with the trigger masks another possibility for disabling channels.
SPC_TM_POS 00000001h Trigger detection for positive edges (crossing level 0 from below to above)
SPC_TM_NEG 00000002h Trigger detection for negative edges (crossing level 0 from above to below)
SPC_TM_POS | SPC_TM_REARM 01000001h Trigger detection for positive edges on level 0. Trigger is armed when crossing level 1 to avoid false trigger on noise
SPC_TM_NEG | SPC_TM_REARM 01000002h Trigger detection for negative edges on level 1. Trigger is armed when crossing level 0 to avoid false trigger on noise
SPC_TM_BOTH 00000004h Trigger detection for positive and negative edges (any crossing of level 0)
SPC_TM_HIGH 00000008h Trigger detection for HIGH levels (signal above level 0)
SPC_TM_LOW 00000010h Trigger detection for LOW levels (signal below level 0)
SPC_TM_WINENTER 00000020h Window trigger for entering area between level 0 and level 1
SPC_TM_WINLEAVE 00000040h Window trigger for leaving area between level 0 and level 1
SPC_TM_INWIN 00000080h Window trigger for signal inside window between level 0 and level 1
SPC_TM_OUTSIDEWIN 00000100h Window trigger for signal outside window between level 0 and level 1
For all external edge and level trigger modes, the OR mask must contain the corresponding input, as the following table shows:
Table 88: Spectrum API: external trigger Ext0 OR mask settings
Register Value Direction Description
SPC_TRIG_ORMASK 40410 read/write Defines the OR mask for the different trigger sources.
SPC_TMASK_EXT0 2h Enable main external trigger input for the OR mask
Trigger Input Termination
The external trigger input is a high impedance input with 1kOhm termination against GND. It is possible to program a 50 Ohm termination
by software to terminate fast trigger signals correctly. If you enable the termination, please make sure, that your trigger source is capable to
deliver the needed current. Please check carefully whether the source is able to fulfil the trigger input specification given in the technical data
section.
Table 89: Spectrum API: external trigger Ext0 input termination
Register Value Direction Description
SPC_TRIG_TERM 40110 read/write A „1“ sets the 50 Ohm termination for external trigger signals. A „0“ sets the high impedance termi-
nation
Please note that the signal levels will drop by 50% if using the 50 ohm termination and your source also has 50 ohm output impedance (both
terminators will then work as a 1:2 divider). In that case it will be necessary to reprogram the trigger levels to match the new signal levels.
In case of problems receiving a trigger please check the signal level of your source while connected to the terminated input.
(c) Spectrum Instrumentation GmbH 123

Trigger modes and appendant registers Secondary external level trigger (Ext1/Trg1)
Trigger Input Coupling
The external trigger input can be switched by software between AC and DC coupling. Please see the technical data section for details on the
AC bandwidth.
Table 90: Spectrum API: external trigger Ext0 input coupling
Register Value Direction Description
SPC_TRIG_EXT0_ACDC 40120 read/write COUPLING_DC enables DC coupling, COUPLING_AC enables AC coupling for the external trigger
input (AC coupling is the default).
Secondary external level trigger (Ext1/Trg1)
The M4i/M4x series has one secondary external trigger input con-
sisting of an input stage with fixed 10 kOhm termination and one
comparator that can be programmed in the range of +/- 10000 mV.
Using one comparators offers a wide range of different logic levels
for the available trigger modes that are support like edge, level.
The secondary external analog trigger can be easily combined with
channel trigger or with the main external trigger being programmed
as an additional external trigger input. The programming of the
masks is shown in the chapters above.
The secondary trigger input Ext1 is labelled Trg1 on the front-panel.
Image 65: trigger engine overview with external trigger Ext1 marked
Trigger Mode
Please find the main external (analog) trigger input modes below. A detailed description of the modes follows in the next chapters..
Table 91: Spectrum API: external trigger Ext1 registers and register settings
Register Value Direction Description
SPC_TRIG_EXT1_AVAILMODES 40501 read Bit mask showing all available trigger modes for Ext1(Trg1) = secondary analog trigger input
SPC_TRIG_EXT1_MODE 40511 read/write Defines the external trigger mode for the external MMCX connector trigger input. The trigger need to
be added to either OR or AND mask input to be activated.
SPC_TM_NONE 00000000h Channel is not used for trigger detection. This is as with the trigger masks another possibility for disabling channels.
SPC_TM_POS 00000001h Trigger detection for positive edges (crossing level 0 from below to above)
SPC_TM_NEG 00000002h Trigger detection for negative edges (crossing level 0 from above to below)
SPC_TM_BOTH 00000004h Trigger detection for positive and negative edges (any crossing of level 0)
SPC_TM_HIGH 00000008h Trigger detection for HIGH levels (signal above level 0)
SPC_TM_LOW 00000010h Trigger detection for LOW levels (signal below level 0)
For all external edge and level trigger modes, the OR mask must contain the corresponding input, as the following table shows:
Table 92: Spectrum API: external trigger Ext1 OR mask settings
Register Value Direction Description
SPC_TRIG_ORMASK 40410 read/write Defines the OR mask for the different trigger sources.
SPC_TMASK_EXT1 4h Enable secondary external trigger input for the OR mask
Trigger level
All of the external (analog) trigger modes listed above require at least one trigger level to be set (except SPC_TM_NONE of course). Some
like the window or the re-arm triggers require even two levels (upper and lower level) to be set. The meaning of the trigger levels is depending
on the selected mode and can be found in the detailed trigger mode description that follows.
Trigger levels for the external (analog) trigger to be programmed in mV:
Table 93: Spectrum API: external trigger available settings for trigger levels
Register Value Direction Description Range
SPC_TRIG_EXT_AVAIL0_MIN 42340 read returns the minimum trigger level for Ext0 to be programmed in mV
SPC_TRIG_EXT_AVAIL0_MAX 42341 read returns the maximum trigger level for Ext0 to be programmed in mV
SPC_TRIG_EXT_AVAIL0_STEP 42342 read returns the step size of trigger level for Ext0 to be programmed in mV
SPC_TRIG_EXT_AVAIL1_MIN 42345 read returns the minimum trigger level for Ext1 to be programmed in mV
SPC_TRIG_EXT_AVAIL1_MAX 42346 read returns the maximum trigger level for Ext1 to be programmed in mV
SPC_TRIG_EXT_AVAIL1_STEP 42347 read returns the step size of trigger level for Ext1 to be programmed in mV
SPC_TRIG_EXT0_LEVEL0 42320 read/write Trigger level 0 for external trigger Ext0 -10000 mV to +10000 mV
(c) Spectrum Instrumentation GmbH 124

Trigger modes and appendant registers Secondary external level trigger (Ext1/Trg1)
Table 93: Spectrum API: external trigger available settings for trigger levels
Register Value Direction Description Range
SPC_TRIG_EXT0_LEVEL1 42330 read/write Trigger level 1 for external trigger Ext0 -10000 mV to +10000 mV
SPC_TRIG_EXT1_LEVEL0 42321 read/write Trigger level 0 for external trigger Ext1 -10000 mV to +10000 mV
Detailed description of the external analog trigger modes
For all external analog trigger modes shown below, either the OR mask or the AND must contain the external trigger to activate the external
input as trigger source:.
Table 94: Spectrum API: external trigger OR mask and AND mask register and settings
Register Value Direction Description
SPC_TRIG_ORMASK 40410 read/write Defines the events included within the trigger OR mask of the card.
SPC_TRIG_ANDMASK 40430 read/write Defines the events included within the trigger AND mask of the card.
SPC_TMASK_EXT0 2h Enables the main external (analog) trigger 0 (labelled Trg0 on front panel) for the mask.
SPC_TMASK_EXT1 4h Enables the secondary external (analog) trigger 1 (labelled Trg1 on front panel) for the mask.
The following pages explain the available modes in detail. All modes that only require one single trigger level are available for both external
trigger inputs. All modes that require two trigger levels are only available for the main external trigger input Ext0 (Trg0).
Trigger on positive edge
The trigger input is continuously sampled with the selected
sample rate. If the programmed trigger level is crossed by
the trigger signal from lower values to higher values (rising
edge) then the trigger event will be detected.
This edge triggered external trigger mode correspond to
the trigger possibilities of usual oscilloscopes.
Table 95: Spectrum API: external register mode setup for trigger on positive edge
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_POS 1h
SPC_TRIG_EXT1_MODE 40511 read/write SPC_TM_POS 1h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the desired trigger level in mV mV
Trigger on negative edge
The trigger input is continuously sampled with the selected
sample rate. If the programmed trigger level is crossed by
the trigger signal from higher values to lower values (falling
edge) then the trigger event will be detected.
This edge triggered external trigger mode correspond to
the trigger possibilities of usual oscilloscopes.
Table 96: Spectrum API: external register mode setup for trigger on negative edge
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_NEG 2h
SPC_TRIG_EXT1_MODE 40511 read/write SPC_TM_NEG 2h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the desired trigger level in mV mV
(c) Spectrum Instrumentation GmbH 125

Trigger modes and appendant registers Secondary external level trigger (Ext1/Trg1)
Trigger on positive and negative edge
The trigger input is continuously sampled with the selected
sample rate. If the programmed trigger level is crossed by
the trigger signal (either rising or falling edge) the trigger
event will be detected.
This edge triggered external trigger mode correspond to
the trigger possibilities of usual oscilloscopes.
Table 97: Spectrum API: external trigger register mode setup for trigger on positive and negative edge
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_BOTH 4h
SPC_TRIG_EXT1_MODE 40511 read/write SPC_TM_BOTH 4h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the desired trigger level in mV mV
Re-arm trigger on positive edge
The trigger input is continuously sampled with the selected
sample rate. If the programmed re-arm level is crossed from
lower to higher values, the trigger engine is armed and
waiting for trigger. If the programmed trigger level is
crossed by the trigger signal from lower values to higher
values (rising edge) then the trigger event will be detected
and the trigger engine will be disarmed. A new trigger
event is only detected if the trigger engine is armed again.
The re-arm trigger modes can be used to prevent the board
from triggering on wrong edges in noisy signals.
Table 98: Spectrum API: external trigger register mode setup for trigger re-arm on positive edge
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
values (falling edge) then the trigger event will be detected
and the trigger engine will be disarmed. A new trigger
event is only detected, if the trigger engine is armed again.
The re-arm trigger modes can be used to prevent the board
from triggering on wrong edges in noisy signals.
Table 99: Spectrum API: external trigger register mode setup for trigger re-arm on negative edge
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_NEG | SPC_TM_REARM 01000002h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Defines the re-arm level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the desired trigger level in mV mV
(c) Spectrum Instrumentation GmbH 126

Trigger modes and appendant registers Secondary external level trigger (Ext1/Trg1)
Window trigger for entering signals
The trigger input is continuously sampled with the selected
sample rate. The upper and the lower level define a win-
dow. Every time the signal enters the window from the out-
side, a trigger event will be detected.
Table 100: Spectrum API: external trigger register mode setup for window trigger for entering signals
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_WINENTER 00000020h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the lower trigger level in mV mV
Window trigger for leaving signals
The trigger input is continuously sampled with the selected
sample rate. The upper and the lower level define a win-
dow. Every time the signal leaves the window from the in-
side, a trigger event will be detected.
Table 101: Spectrum API: external trigger register mode setup for window trigger for leaving signals
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_WINLEAVE 00000040h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the lower trigger level in mV mV
High level trigger
This trigger mode will generate an internal gate signal that
can be useful in conjunction with a second trigger mode to
gate that second trigger. If using this mode as a single trigger
source the card will detect a trigger event at the time when
entering the high level (acting like positive edge trigger) or if
the trigger signal is already above the programmed level at
the start it will immediately detect a trigger event.
The trigger input is continuously sampled with the selected
sample rate. The trigger event will be detected if the trigger
input is above the programmed trigger level.
Table 102: Spectrum API: external trigger register mode setup for high level trigger
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_HIGH 00000008h
SPC_TRIG_EXT1_MODE 40511 read/write SPC_TM_HIGH 00000008h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
(c) Spectrum Instrumentation GmbH 127

Trigger modes and appendant registers Secondary external level trigger (Ext1/Trg1)
Low level trigger
This trigger mode will generate an internal gate signal that
can be useful in conjunction with a second trigger mode to
gate that second trigger. If using this mode as a single trigger
source the card will detect a trigger event at the time when
entering the low level (acting like negative edge trigger) or if
the trigger signal is already above the programmed level at
the start it will immediately detect a trigger event.
The trigger input is continuously sampled with the selected
sample rate. The trigger event will be detected if the trigger
input is below the programmed trigger level.
Table 103: Spectrum API: external trigger register mode setup for low level trigger
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_LOW 00000010h
SPC_TRIG_EXT1_MODE 40511 read/write SPC_TM_LOW 00000010h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
In window trigger
This trigger mode will generate an internal gate signal that
can be useful in conjunction with a second trigger mode to
gate that second trigger. If using this mode as a single trigger
source the card will detect a trigger event at the time when
entering the window defined by the two trigger levels (acting
like window enter trigger) or if the trigger signal is already
inside the programmed window at the start it will immediately
detect a trigger event.
The trigger input is continuously sampled with the selected
sample rate. The trigger event will be detected if the trigger
input is inside the programmed trigger window.
Table 104: Spectrum API: external trigger register mode setup for in window trigger
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_INWIN 00000080h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the lower trigger level in mV mV
Outside window trigger
This trigger mode will generate an internal gate signal that
can be useful in conjunction with a second trigger mode to
gate that second trigger. If using this mode as a single trigger
source the card will detect a trigger event at the time when
leaving the window defined by the two trigger levels (acting
like leaving window trigger) or if the trigger signal is already
outside the programmed window at the start it will immedi-
ately detect a trigger event.
The trigger input is continuously sampled with the selected
sample rate. The trigger event will be detected if the trigger
input is outside the programmed trigger window.
Table 105: Spectrum API: external trigger register mode setup for outside window trigger
Register Value Direction set to Value
SPC_TRIG_EXT0_MODE 40510 read/write SPC_TM_OUTSIDEWIN 00000100h
SPC_TRIG_EXT0_LEVEL0 42320 read/write Set it to the upper trigger level in mV mV
SPC_TRIG_EXT0_LEVEL1 42330 read/write Set it to the lower trigger level in mV mV
(c) Spectrum Instrumentation GmbH 128

PXI Trigger (M4x PXIe cards only)
PXI Trigger (M4x PXIe cards only)
The M4x PXIe cards can use the various PXI trigger sources for trigger
detection and/or trigger and status distribution.
This includes the eight lines from the PXI trigger bus (PXI_TRIG[7] to
PXI_TRIG[0]), as well as the „older“ single-ended PXI Star-Trigger line
(PXI_STAR) and the „newer“ differential PXI_DSTARC (dedicated out-
put) and PXI_DSTARB (dedicated input) lines, that have been intro-
duced with the PXI Express standard.
All these lines can be included within the programmable masks on
the cards, either within the OR mask as well within the AND mask, to
form rather complex trigger conditions required to properly synchro-
nize multiple M4x.xxxx cards to a single trigger event.
The following passage shows, how to program these lines for either
input or output and how to properly use them for trigger synchroniza- Table 106: trigger overview with PXI trigger lines marked
tion between multiple M4x.xxxx cards inside a PXI or PXIe chassis.
To set up PXI trigger conditions, the mode registers have to be set up properly, to define the direction of one or multiple PXI lines, as well as
the trigger masks registers for including one or multiple PXI lines to generate a trigger event from.
PXI Trigger Mode Registers
Please find the list of the various modes the different PXI trigger lines can be used for below. The modes available for driving the PXI lines are
very similar to thoese of the Multi Purpose I/O lines described earlier in this manual.
Table 107: Spectrum API: PXI trigger register and available register settings
Register Value Direction Description
SPC_PXITRG0_AVAILMODES 47310 read Bitmask showing all available PXI trigger modes usable with PXI_TRIG0 line.
SPC_PXITRG1_AVAILMODES 47311 read Bitmask showing all available PXI trigger modes usable with PXI_TRIG1 line.
SPC_PXITRG2_AVAILMODES 47312 read Bitmask showing all available PXI trigger modes usable with PXI_TRIG2 line.
SPC_PXITRG3_AVAILMODES 47313 read Bitmask showing all available PXI trigger modes usable with PXI_TRIG3 line.
SPC_PXITRG4_AVAILMODES 47314 read Bitmask showing all available PXI trigger modes usable with PXI_TRIG4 line.
SPC_PXITRG5_AVAILMODES 47315 read Bitmask showing all available PXI trigger modes usable with PXI_TRIG5 line.
SPC_PXITRG6_AVAILMODES 47316 read Bitmask showing all available PXI trigger modes usable with PXI_TRIG6 line.
SPC_PXITRG7_AVAILMODES 47317 read Bitmask showing all available PXI trigger modes usable with PXI_TRIG7 line.
SPC_PXISTAR_AVAILMODES 47318 read Bitmask showing all available PXI trigger modes usable with PXI_STAR line.
SPC_PXIDSTARC_AVAILMODES 47310 read Bitmask showing all available PXI trigger modes usable with PXI_DSTARC line, to send information to
a Startrigger card, installed in the System Timing Slot. The corresponding returned signal (from the
System Timing Slot to the card) will be available on the DSTARB line, which then has to be properly
included into the trigger source masks, as described later.
SPC_PXITRG0_MODE 47300 read/write Defines the output mode for the PXI_TRIG0 line.
SPC_PXITRG1_MODE 47301 read/write Defines the output mode for the PXI_TRIG1 line.
SPC_PXITRG2_MODE 47302 read/write Defines the output mode for the PXI_TRIG2 line.
SPC_PXITRG3_MODE 47303 read/write Defines the output mode for the PXI_TRIG3 line.
SPC_PXITRG4_MODE 47304 read/write Defines the output mode for the PXI_TRIG4 line.
SPC_PXITRG5_MODE 47305 read/write Defines the output mode for the PXI_TRIG5 line.
SPC_PXITRG6_MODE 47306 read/write Defines the output mode for the PXI_TRIG6 line.
SPC_PXITRG7_MODE 47307 read/write Defines the output mode for the PXI_TRIG7 line.
SPC_PXISTAR_MODE 47308 read/write Defines the trigger mode for the PXI_STAR line, to send information to a Startrigger card, installed in
the System Timing Slot.
SPC_PXIDSTARC_MODE 47309 read/write Defines the trigger mode for the PXI_DSTARC line, to send information to a possible Startrigger card,
installed in the System Timing Slot. The corresponding returned signal (from the System Timing Slot to
the card) will be available on the DSTARB line, which then has to be properly included into the trig-
ger source masks, as described later.
SPCM_PXITRGMODE_DISABLE 00000000h The PXI line is neither used as input or output and is in high-impedance mode (tristate).
SPCM_PXITRGMODE_IN 00000001h The PXI line is used as an input and can now be included in the trigger masks, as described below.
SPCM_PXITRGMODE_ASYNCOUT 00000002h The PXI line is is programmed for asynchronous output. Use SPC_PXITRG_ASYNCIO to write data asynchro-
nously.
SPCM_PXITRGMODE_RUNSTATE 00000004h The PXI line outputs the current run state of the card. If acquisition/output is running the signal is HIGH. If card
has stopped the signal is LOW.
SPCM_PXITRGMODE_ARMSTATE 00000008h The PXI line outputs the current ARM state of the card. If the card is armed and ready to receive a trigger the sig-
nal is HIGH. If the card isn’t running or the card is either still acquiring pretrigger data or the trigger has already
been detected the signal is LOW.
SPCM_PXITRGMODE_TRIGOUT 00000010h The PXI line outputs a detected trigger and hence shows the trigger detection. The trigger output goes HIGH as
soon as the trigger is recognized. After end of acquisition it is LOW again. In Multiple Recording/Gated Sam-
pling/ABA mode it goes LOW after the acquisition of the current segment stops. In standard FIFO mode the trig-
ger output is HIGH until FIFO mode is stopped.
(c) Spectrum Instrumentation GmbH 129

PXI Trigger (M4x PXIe cards only)
SPCM_PXITRGMODE_REFCLKOUT 00000020h The PXI line is reflects the internal generated 10 MHz reference clock signal generated from the PXI_CLK100
clock signal in conjunction with the PXI_SYNC100 signal. Can be used to provide other equipment with an
additional clock signal via one of the trigger lines.
SPCM_PXITRGMODE_CONTOUTMARK 00000040h Generator Cards only: the PXI line outputs a HIGH pulse as continuous marker signal for continuous replay
mode. The marker signal length is ½ of the programmed memory size.
Depending on the used PXI/PXIe backplane, the PXI trigger bus (PXI_TRIG[7..0]) might be segmented and
needs to be setup separately, to allow routing of trigger signals between different segments an even within
one segment. Please consult your PXI chassis/backplane manual for details on the routing capabilities of
these lines and the relating software interface.
Be aware not to enable outputs on the same PXI trigger line on multiple cards, or to enable a segmented
backplane driver and a card’s output on the same line within the same segment. If two or more outputs are
working against each other the result is unpredictable and may even harm the hardware parts.
The PXI trigger lines cannot be used to phase-synchronize multiple PXIe cards. The PXIe trigger lines are asyn-
chronous to the used sampling rate and thus may result in a phase difference of more than one sample be-
tween multiple cards.
PXI Trigger Sources within the Trigger Masks
To include PXI lines, whose mode has been set to „SPCM_PXITRGMODE_IN“, as shown above, the desired masks must contain the corre-
sponding input, as the following table shows:
Table 108: Spectrum API: PXI trigger mask register and available register settings
Register Value Direction Description
SPC_TRIG_ORMASK 40410 read/write Defines the events included within the trigger OR mask of the card.
SPC_TRIG_ANDMASK 40430 read/write Defines the events included within the trigger AND mask of the card.
SPC_TMASK_PXI0 100000h Enables the PXI_TRIG0 for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI1 200000h Enables the PXI_TRIG1 for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI2 400000h Enables the PXI_TRIG2 for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI3 800000h Enables the PXI_TRIG3 for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI4 1000000h Enables the PXI_TRIG4 for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI5 2000000h Enables the PXI_TRIG5 for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI6 4000000h Enables the PXI_TRIG6 for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXI7 8000000h Enables the PXI_TRIG7 for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXISTAR 10000000h Enables the PXISTAR line for the mask. The card will trigger when the signal on this input is HIGH.
SPC_TMASK_PXIDSTARB 20000000h Enables the PXI_DSTARB for the mask. The card will trigger when the signal on this input is HIGH.
The use of multiple PXI_TRIG lines in either mask to combine status and trigger information is shown in the following example for clarification.
(c) Spectrum Instrumentation GmbH 130

PXI Trigger (M4x PXIe cards only)
PXI Trigger Setup Example
Example of connecting three M4x.xxxx cards, card 0 is triggering all three cards:
drv_handle hDrv[3];
for (i = 0; i < 3; i++)
{
sprintf (s, "/dev/spcm%d", i);
hDrv[i] = spcm_hOpen (s); // open all three cards
spcm_dwSetParam_i32 (hDrv[i], SPC_CLOCKMODE, SPC_CM_PXIREFCLOCK); // Use PXI reference clock on all cards
spcm_dwGetParam_i64 (hDrv[i], SPC_SAMPLERATE, 100000000); // Use 100 MS/s as sample clock
}
// Slave card1 trigger setup
spcm_dwSetParam_i32 (hDrv[1], SPC_PXITRG0_MODE, SPCM_PXITRGMODE_IN); // set PXI_TRIG0 as input
spcm_dwSetParam_i32 (hDrv[1], SPC_PXITRG1_MODE, SPCM_PXITRGMODE_ARMSTATE); // Output my ARM state on PXI_TRIG1
spcm_dwSetParam_i32 (hDrv[1], SPC_TRIG_ORMASK, SPC_TMASK_PXI0); // trigger source: PXI_TRIG0
// Slave card2 trigger setup
spcm_dwSetParam_i32 (hDrv[2], SPC_PXITRG0_MODE, SPCM_PXITRGMODE_IN); // set PXI_TRIG0 as input
spcm_dwSetParam_i32 (hDrv[2], SPC_PXITRG2_MODE, SPCM_PXITRGMODE_ARMSTATE); // Output my ARM state on PXI_TRIG2
spcm_dwSetParam_i32 (hDrv[2], SPC_TRIG_ORMASK, SPC_TMASK_PXI0); // trigger source: PXI_TRIG0
// Master card0: Acts as a trigger master distributing External trigger 0 trigger
spcm_dwSetParam_i32 (hDrv[0], SPC_TRIG_ORMASK, SPC_TMASK_EXT0);
// Setting Ext0 trigger for rising edges (for AD/DA cards trigger levels might need to be adjusted)
spcm_dwSetParam_i32 (hDrv[0], SPC_TRIG_EXT0_MODE, SPC_TM_POS);
spcm_dwSetParam_i32 (hDrv[0], SPC_PXITRG0_MODE, SPCM_PXITRGMODE_TRIGOUT); // Output trigger on PXI_TRIG0
spcm_dwSetParam_i32 (hDrv[0], SPC_PXITRG1_MODE, SPCM_PXITRGMODE_IN); // Set PXI_TRIG1 as input
spcm_dwSetParam_i32 (hDrv[0], SPC_PXITRG2_MODE, SPCM_PXITRGMODE_IN); // Set PXI_TRIG2 as input
// Synchronize Pre-Trigger area of all cards to prevent unintended trigger detection, while the
// other card(s) are not ready yet. Therefore include PXI_TRIG1 and PXI_TRIG2 inputs in the AND mask,
// so that these lines both must be HIGH, to enable trigger detection on card0 and hence
// trigger distribution to the other cards.
spcm_dwSetParam_i32 (hDrv[0], SPC_TRIG_ANDMASK, SPC_TMASK_PXI2 | SPC_TMASK_PXI1);
// transfer setup to all cards to allow PXI lines to be activated (leave a possible high-impedance mode)
for (i = 0; i < 3; i++)
spcm_dwSetParam_i32 (hCard[i], SPC_M2CMD, M2CMD_CARD_WRITESETUP);
// Start the cards now in any order, as any triggering is now prevented as long no all the cards are armed.
// Wait for all cards to finish recording and then get data from all cards and do processing.
The above example assumes, that a possible PXI trigger bus segmentation of the used backplane has been
properly set up, so that each card’s output can reach the other card’s inputs properly and that no two drivers
on one segment are driving against each other.
(c) Spectrum Instrumentation GmbH 131

Multi Purpose I/O Lines On-board I/O lines (X0, X1, X2)
