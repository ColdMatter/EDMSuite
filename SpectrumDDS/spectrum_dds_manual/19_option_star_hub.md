Option Star-Hub (M3i and M4i only)
Star-Hub introduction
The purpose of the Star-Hub is to extend the number of channels available for acquisition or generation by interconnecting multiple cards and
running them simultaneously.
The Star-Hub option allows to synchronize several cards of the same M3i/M4i series that are mounted within one host system (PC):
• For the M3i series there are the two different versions available: a small version with 4 connectors (option SH4) for synchronizing up to
four cards and a big version with 8 connectors (option SH8) for synchronizing up to eight cards.
• For the M4i series there are the two different mechanical versions available, with 8 connectors for synchronizing up to eight cards.
The Star-Hub allows synchronizing cards of the same family only. It is not possible to synchronize cards of
different families!
Both versions are implemented as a piggy -back module that is mounted to one of the cards. For details on how to install several cards in-
cluding the one carrying the Star-Hub module, please refer to the section on hardware installation.
Either which of the two available Star-Hub options is used, there will be no phase delay between the sampling clocks of the synchronized
cards and either no delay between the trigger events. The card holding the Star-Hub is automatically also the clock master. Any one of the
synchronized cards can be part of the trigger generation.
Star-Hub trigger engine
The trigger bus between an M3i/M4i card and the Star-Hub option consists of several lines. Some of them send the trigger information from
the card’s trigger engine to the Star-Hub and some receives the resulting trigger from the Star-Hub. All trigger events from the different cards
connected are combined with OR on the Star-Hub.
While the returned trigger is identical for all synchronized cards, the sent out trigger of every single card depends on their trigger settings.
Star-Hub clock engine
The card holding the Star-Hub is the clock master for the complete system. If
you need to feed in an external clock to a synchronized system the clock has
to be connected to the master card. Slave cards cannot generate a Star-Hub
system clock. As shown in the drawing on the right the clock master can use
either the programmable quartz 1 or the external clock input to be broadcast
to all other cards.
All cards including the clock master itself receive the distributed clock with
equal phase information. This makes sure that there is no phase delay be-
tween the cards.
Table 213: star-hub clock overview diagram
Software Interface
The software interface is similar to the card software interface that is explained earlier in this manual. The same functions and some of the
registers are used with the Star-Hub. The Star-Hub is accessed using its own handle which has some extra commands for synchronization
setup. All card functions are programmed directly on card as before. There are only a few commands that need to be programmed directly
to the Star-Hub for synchronization.
The software interface as well as the hardware supports multiple Star-Hubs in one system. Each set of cards connected by a Star-Hub then
runs totally independent. It is also possible to mix cards that are connected with the Star-Hub with other cards that run independent in one
system.
Star-Hub Initialization
The interconnection between the Star-Hubs is probed at driver load time and does not need to be programmed separately. Instead the cards
can be accessed using a logical index. This card index is only based on the ordering of the cards in the system and is not influenced by the
current cabling. It is even possible to change the cable connections between two system starts without changing the logical card order that
is used for Star-Hub programming.
The Star-Hub initialization must be done AFTER initialization of all cards in the system. Otherwise the inter-
connection won’t be detected properly.
(c) Spectrum Instrumentation GmbH 191

Option Star-Hub (M3i and M4i only) Software Interface
The Star-Hubs are accessed using a special device name „sync“ followed by the index of the star-hub to access. The Star-Hub is handled
completely like a physical card allowing all functions based on the handle like the card itself.
Example with 4 cards and one Star-Hub (no error checking to keep example simple)
drv_handle hSync;
drv_handle hCard[4];
for (i = 0; i < 4; i++)
{
sprintf (s, "/dev/spcm%d", i);
hCard[i] = spcm_hOpen (s);
}
hSync = spcm_hOpen ("sync0");
...
spcm_vClose (hSync);
for (i = 0; i < 4; i++)
spcm_vClose (hCard[i]);
Example for a digitizerNETBOX or generatorNETBOX with two internal digitizer/generator modules, This example is also suitable for
accessing a remote server with two cards installed:
drv_handle hSync;
drv_handle hCard[2];
for (i = 0; i < 2; i++)
{
sprintf (s, "TCPIP::192.168.169.14::INST%d::INSTR", i);
hCard[i] = spcm_hOpen (s);
}
hSync = spcm_hOpen ("sync0");
...
spcm_vClose (hSync);
for (i = 0; i < 2; i++)
spcm_vClose (hCard[i]);
When opening the Star-Hub the cable interconnection is checked. The Star-Hub may return an error if it sees internal cabling problems or if
the connection between Star-Hub and the card that holds the Star-Hub is broken. It can’t identify broken connections between Star-Hub and
other cards as it doesn’t know that there has to be a connection.
The synchronization setup is done using bit masks where one bit stands for one recognized card. All cards that are connected with a Star-
Hub are internally numbered beginning with 0. The number of connected cards as well as the connections of the Star-Hub can be read out
after initialization. For each card that is connected to the Star-Hub one can read the index of that card:
Table 214: Spectrum API: star-hub related registers for reading detected connections
Register Value Direction Description
SPC_SYNC_READ_NUMCONNECTORS 48991 read Number of connectors that the Star-Hub offers at max. (available with driver V5.6 or newer)
SPC_SYNC_READ_SYNCCOUNT 48990 read Number of cards that are connected to this Star-Hub
SPC_SYNC_READ_CARDIDX0 49000 read Index of card that is connected to star-hub logical index 0 (mask 0x0001)
SPC_SYNC_READ_CARDIDX1 49001 read Index of card that is connected to Star-Hub logical index 1 (mask 0x0002)
... read ...
SPC_SYNC_READ_CARDIDX7 49007 read Index of card that is connected to star-hub logical index 7 (mask 0x0080)
SPC_SYNC_READ_CARDIDX8 49008 read M2i only: Index of card that is connected to Star-Hub logical index 8 (mask 0x0100)
... read ...
SPC_SYNC_READ_CARDIDX15 49015 read M2i only: Index of card that is connected to star-hub logical index 15 (mask 0x8000)
SPC_SYNC_READ_CABLECON0 read Returns the index of the cable connection that is used for the logical connection 0. The cable connec-
tions can be seen printed on the PCB of the star-hub. Use these cable connection information in case
that there are hardware failures with the Star-Hub cabeling.
... 49100 read ...
SPC_SYNC_READ_CABLECON15 49115 read Returns the index of the cable connection that is used for the logical connection 15.
In standard systems where all cards are connected to one star-hub reading the Star-Hub logical index will simply return the index of the card
again. This results in bit 0 of Star-Hub mask being 1 when doing the setup for card 0, bit 1 in Star-Hub mask being 1 when setting up card
(c) Spectrum Instrumentation GmbH 192

Option Star-Hub (M3i and M4i only) Software Interface
1 and so on. On such systems it is sufficient to read out the SPC_SYNC_READ_SYNCCOUNT register to check whether the Star-Hub has
found the expected number of cards to be connected.
spcm_dwGetParam_i32 (hSync, SPC_SYNC_READ_SYNCCOUNT, &lSyncCount);
for (i = 0; i < lSyncCount; i++)
{
spcm_dwGetParam_i32 (hSync, SPC_SYNC_READ_CARDIDX0 + i, &lCardIdx);
printf ("Star-Hub logical index %d is connected with card %d\n“, i, lCardIdx);
}
In case of 4 cards in one system and all are connected with the Star-Hub this program excerpt will return:
Star-Hub logical index 0 is connected with card 0
Star-Hub logical index 1 is connected with card 1
Star-Hub logical index 2 is connected with card 2
Star-Hub logical index 3 is connected with card 3
Let’s see a more complex example with two Star-Hubs and one independent card in one system. Star-Hub A connects card 2, card 4 and
card 5. Star-Hub B connects card 0 and card 3. Card 1 is running completely independent and is not synchronized at all:
card Star-Hub connection card handle Star-Hub handle card index in Star-Hub mask for this card in
Star-Hub
card 0 - /dev/spcm0 0 (of Star-Hub B) 0x0001
card 1 - /dev/spcm1 -
card 2 Star-Hub A /dev/spcm2 sync0 0 (of Star-Hub A) 0x0001
card 3 Star-Hub B /dev/spcm3 sync1 1 (of Star-Hub B) 0x0002
card 4 - /dev/spcm4 1 (of Star-Hub A) 0x0002
card 5 - /dev/spcm5 2 (of Star-Hub A) 0x0004
Now the program has to check both Star-Hubs:
for (j = 0; j < lStarhubCount; j++)
{
spcm_dwGetParam_i32 (hSync[j], SPC_SYNC_READ_SYNCCOUNT, &lSyncCount);
for (i = 0; i < lSyncCount; i++)
{
spcm_dwGetParam_i32 (hSync[j], SPC_SYNC_READ_CARDIDX0 + i, &lCardIdx);
printf ("Star-Hub %c logical index %d is connected with card %d\n“, (!j ? ’A’ : ’B’), i, lCardIdx);
}
printf ("\n");
}
In case of the above mentioned cabling this program excerpt will return:
Star-Hub A logical index 0 is connected with card 2
Star-Hub A logical index 1 is connected with card 4
Star-Hub A logical index 2 is connected with card 5
Star-Hub B logical index 0 is connected with card 0
Star-Hub B logical index 1 is connected with card 3
For the following examples we will assume that 4 cards in one system are all connected to one Star-Hub to keep things easier.
Setup of Synchronization
The synchronization setup only requires one additional register to enable the cards that are synchronized in the next run
Table 215: Spectrum API: synchronization enable mask register
Register Value Direction Description
SPC_SYNC_ENABLEMASK 49200 read/write Mask of all cards that are enabled for the synchronization
The enable mask is based on the logical index explained above. It is possible to just select a couple of cards for the synchronization. All other
cards then will run independently. Please be sure to always enable the card on which the Star-Hub is located as this one is a must for the
synchronization.
(c) Spectrum Instrumentation GmbH 193

Option Star-Hub (M3i and M4i only) Software Interface
In our example we synchronize all four cards. The Star-Hub is located on card #2 and is therefor the clock master
spcm_dwSetParam_i32 (hSync, SPC_SYNC_ENABLEMASK, 0x000F); // all 4 cards are masked
// set the clock master to 100 MS/s internal clock
spcm_dwSetParam_i32 (hCard[2], SPC_CLOCKMODE, SPC_CM_INTPLL);
spcm_dwSetParam_i32 (hCard[2], SPC_SAMPLERATE, MEGA(100));
// set all the slaves to run synchronously with 100 MS/s
spcm_dwSetParam_i32 (hCard[0], SPC_SAMPLERATE, MEGA(100));
spcm_dwSetParam_i32 (hCard[1], SPC_SAMPLERATE, MEGA(100));
spcm_dwSetParam_i32 (hCard[3], SPC_SAMPLERATE, MEGA(100));
Setup of Trigger
Setting up the trigger does not need any further steps of synchronization setup. Simply all trigger settings of all cards that have been enabled
for synchronization are connected together. All trigger sources and all trigger modes can be used on synchronization as well.
Having positive edge of external trigger on card 0 to be the trigger source for the complete system needs the following setup:
spcm_dwSetParam_i32 (hCard[0], SPC_TRIG_ORMASK, SPC_TMASK_EXT0);
spcm_dwSetParam_i32 (hCard[0], SPC_TRIG_EXT0_MODE, SPC_TM_POS);
spcm_dwSetParam_i32 (hCard[1], SPC_TRIG_ORMASK, SPC_TM_NONE);
spcm_dwSetParam_i32 (hCard[2], SPC_TRIG_ORMASK, SPC_TM_NONE);
spcm_dwSetParam_i32 (hCard[3], SPC_TRIG_ORMASK, SPC_TM_NONE);
Assuming that the 4 cards are analog data acquisition cards with 4 channels each we can simply setup a synchronous system with all channels
of all cards being trigger source. The following setup will show how to set up all trigger events of all channels to be OR connected. If any of
the channels will now have a signal above the programmed trigger level the complete system will do an acquisition:
for (i = 0; i < lSyncCount; i++)
{
int32 lAllChannels = (SPC_TMASK0_CH0 | SPC_TMASK0_CH1 | SPC_TMASK_CH2 | SPC_TMASK_CH3);
spcm_dwSetParam_i32 (hCard[i], SPC_TRIG_CH_ORMASK0, lAllChannels);
for (j = 0; j < 2; j++)
{
// set all channels to trigger on positive edge crossing trigger level 100
spcm_dwSetParam_i32 (hCard[i], SPC_TRIG_CH0_MODE + j, SPC_TM_POS);
spcm_dwSetParam_i32 (hCard[i], SPC_TRIG_CH0_LEVEL0 + j, 100);
}
}
Run the synchronized cards
Running of the cards is very simple. The Star-Hub acts as one big card containing all synchronized cards. All card commands have to be
omitted directly to the Star-Hub which will check the setup, do the necessary steps for synchronization and distribute the commands in the
correct order to all synchronized cards.
The same card commands that are normally send to a single cards are also valid to be send to the Star-Hub:
Table 216: Spectrum API: star-hub synchronization commands
Register Value Direction Description
SPC_M2CMD 100 write only Executes a command for the card or data transfer
M2CMD_CARD_RESET 1h Performs a hard and software reset of the card as explained further above
M2CMD_CARD_WRITESETUP 2h Writes the current setup to the card without starting the hardware. This command may be useful if changing some
internal settings like clock frequency and enabling outputs.
M2CMD_CARD_START 4h Starts the card with all selected settings. This command automatically writes all settings to the card if any of the set-
tings has been changed since the last one was written. After card has been started none of the settings can be
changed while the card is running.
M2CMD_CARD_ENABLETRIGGER 8h The trigger detection is enabled. This command can be either send together with the start command to enable trigger
immediately or in a second call after some external hardware has been started.
M2CMD_CARD_FORCETRIGGER 10h This command forces a trigger even if none has been detected so far. Sending this command together with the start
command is similar to using the software trigger.
M2CMD_CARD_DISABLETRIGGER 20h The trigger detection is disabled. All further trigger events are ignored until the trigger detection is again enabled.
When starting the card the trigger detection is started disabled.
M2CMD_CARD_STOP 40h Stops the current run of the card. If the card is not running this command has no effect.
All other commands and all other settings should be send directly to the card(s) that they refer to. While sending the wait commands to the
Star-Hub handle is allowed, these commands are not distributed to/collected from the synchronized cards but only refer synonymously to the
card carrying the Star-Hub.
(c) Spectrum Instrumentation GmbH 194

Option Star-Hub (M3i and M4i only) Software Interface
This example shows the complete setup and synchronization start for our four cards:
spcm_dwSetParam_i32 (hSync, SPC_SYNC_ENABLEMASK, 0x000F); // all 4 cards are masked
// to keep it easy we set all card to the same clock and disable trigger
for (i = 0; i < 4; i++)
{
spcm_dwSetParam_i32 (hCard[i], SPC_CLOCKMODE, SPC_CM_INTPLL);
spcm_dwSetParam_i32 (hCard[i], SPC_SAMPLERATE, MEGA(100));
spcm_dwSetParam_i32 (hCard[i], SPC_TRIG_ORMASK, SPC_TM_NONE);
}
// card 0 is trigger master and waits for external positive edge
spcm_dwSetParam_i32 (hCard[0], SPC_TRIG_ORMASK, SPC_TMASK_EXT0);
spcm_dwSetParam_i32 (hCard[0], SPC_TRIG_EXT0_MODE, SPC_TM_POS);
// start the cards and wait for them a maximum of 1 second to be ready
spcm_dwSetParam_i32 (hSync, SPC_TIMEOUT, 1000);
spcm_dwSetParam_i32 (hSync, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER);
if (spcm_dwSetParam_i32 (hSync, SPC_M2CMD, M2CMD_CARD_WAITREADY) == ERR_TIMEOUT)
printf ("Timeout occured - no trigger received within time\n")
Using one of the wait commands for the Star-Hub will return as soon as the card holding the Star-Hub has
reached this state. However when synchronizing cards with different memory sizes there may be other cards
that still haven’t reached this level.
SH-Direct: using the Star-Hub clock directly without synchronization
Starting with driver version 1.26 it is possible to use the clock from the Star-Hub just like an external clock and running one or more cards
totally independent of the synchronized card. The mode is by example useful if one has one or more output cards that run continuously in a
loop and are synchronized with Star-Hub and in addition to this one or more acquisition cards should make multiple acquisitions but using
the same clock.
For all M2i cards it is also possible to run the „slave“ cards with a divided clock. Therefore please program a desired divided sampling rate
in the SPC_SAMPLERATE register (example: running the Star-Hub card with 10 MS/s and the independent cards with 1 MS/s). The sampling
rate is automatically adjusted by the driver to the next matching value.
What is necessary?
• All cards need to be connected to the Star-Hub
• The card(s) that should run independently can not hold the Star-Hub
• The card(s) with the Star-Hub must be setup to synchronization even if it’s only one card
• The synchronized card(s) have to be started prior to the card(s) that run with the direct Star-Hub clock
Setup
At first all cards that should run synchronized with the Star-Hub are set-up exactly as explained before. The card(s) that should run inde-
pendently and use the Star-Hub clock need to use the following clock mode:
Table 217: Spectrum API: clock mode register and settings for SH Direct mode
Register Value Direction Description
SPC_CLOCKMODE 20200 read/write Defines the used clock mode
SPC_CM_SHDIRECT 128 Uses the clock from the Star-Hub as if this was an external clock
When using SH_Direct mode, the register call to SPC_CLOCKMODE enabling this mode must be written before
initiating a card start command to any of the connected cards. Also it is not allowed to be modified later in
the programming sequence to prevent the driver from calculating wrong sample rates.
(c) Spectrum Instrumentation GmbH 195

Option Star-Hub (M3i and M4i only) Software Interface
Example
In this example we have one generator card with the Star-Hub mounted running in a continuous loop and one acquisition card running inde-
pendently using the SH-Direct clock.
// setup of the generator card
spcm_dwSetParam_i32 (hCard[0], SPC_CARDMODE, SPC_REP_STD_SINGLE);
spcm_dwSetParam_i32 (hCard[0], SPC_LOOPS, 0); // infinite data replay
spcm_dwSetParam_i32 (hCard[0], SPC_CLOCKMODE, SPC_CM_INTPLL);
spcm_dwSetParam_i32 (hCard[0], SPC_SAMPLERATE, MEGA(1));
spcm_dwSetParam_i32 (hCard[0], SPC_TRIG_ORMASK, SPC_TM_SOFTWARE);
spcm_dwSetParam_i32 (hSync, SPC_SYNC_ENABLEMASK, 0x0001); // card 0 is the generator card
spcm_dwSetParam_i32 (hSync, SPC_SYNC_CLKMASK, 0x0001); // only for M2i/M3i cards: set ClkMask
// Setup of the acquisition card (waiting for external trigger)
spcm_dwSetParam_i32 (hCard[1], SPC_CARDMODE, SPC_REC_STD_SINGLE);
spcm_dwSetParam_i32 (hCard[1], SPC_CLOCKMODE, SPC_CM_SHDIRECT);
spcm_dwSetParam_i32 (hCard[1], SPC_SAMPLERATE, MEGA(1));
spcm_dwSetParam_i32 (hCard[1], SPC_TRIG_ORMASK, SPC_TMASK_EXT0);
spcm_dwSetParam_i32 (hCard[1], SPC_TRIG_EXT0_MODE, SPC_TM_POS);
// now start the generator card (sync!) first and then the acquisition card
spcm_dwSetParam_i32 (hSync, SPC_TIMEOUT, 1000);
spcm_dwSetParam_i32 (hSync, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER);
// start first acquisition
spcm_dwSetParam_i32 (hCard[1], SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER | M2CMD_CARD_WAITREADY);
// process data
// start next acquistion
spcm_dwSetParam_i32 (hCard[1], SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER | M2CMD_CARD_WAITREADY);
// process data
Error Handling
The Star-Hub error handling is similar to the card error handling and uses the function spcm_dwGetErrorInfo_i32. Please see the example in
the card error handling chapter to see how the error handling is done.
(c) Spectrum Instrumentation GmbH 196

Option Remote Server Introduction
