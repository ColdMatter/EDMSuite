Multi Purpose I/O Lines
On-board I/O lines (X0, X1, X2)
The M4i/M4x series cards and the based upon digitizerNETBOX,
generatorNETBOX and hybridNETBOX products have three multi
purpose I/O lines that can be used for a wide variety of functions to
help the interconnection with external equipment. The functionality of
these multi purpose I/O lines can be software programmed and each
of these lines can either be used for input or output.
The multi purpose I/O lines may be used as status outputs such as
trigger output or internal arm/run as well as for asynchronous I/O to
control external equipment as well as additional digital input lines
that are sampled synchronously with the analog data.
The multi purpose I/O lines are available on the front plate and la-
beled with X0 (line 0), X1 (line 1) and X2 (line 2). As default these
lines are switched off.
Image 66: trigger overview with multi-purpose lines marked
As default (power-on and after reset command) the I/O capable lines are switched off and hence are not
actively driven. Hence the on-board 10 kOhm pull-up resistors are pulling these lines to logic HIGH. If a logic
LOW is required, external lower-value (1 kOhm) pull-down resistors might be used.
Please be careful when programming these lines as an output whilst maybe still being connected with an
external signal source, as that may damage components either on the external equipment or on the card
itself.
Programming the behavior
Each multi purpose I/O line can be individually programmed. Please check the available modes by reading the SPCM_X0_AVAILMODES,
SPCM_X1_AVAILMODES and SPCM_X2_AVAILMODES register first. The available modes may differ from card to card and may be en-
hanced with new driver/firmware versions to come.
Table 109: Spectrum API: multi-purpose I/O lines registers and available register settings
Register Value Direction Description
SPCM_X0_AVAILMODES 47210 read Bitmask with all bits of the below mentioned modes showing the available modes for (X0)
SPCM_X1_AVAILMODES 47211 read Bitmask with all bits of the below mentioned modes showing the available modes for (X1)
SPCM_X2_AVAILMODES 47212 read Bitmask with all bits of the below mentioned modes showing the available modes for (X2)
SPCM_X0_MODE 47200 read/write Defines the mode for (X0). Only one mode selection is possible to be set at a time
SPCM_X1_MODE 47201 read/write Defines the mode for (X1). Only one mode selection is possible to be set at a time
SPCM_X2_MODE 47202 read/write Defines the mode for (X2). Only one mode selection is possible to be set at a time
SPCM_XMODE_DISABLE 00000000h No mode selected. Output is tristate (default setup)
SPCM_XMODE_ASYNCIN 00000001h Connector is programmed for asynchronous input. Use SPCM_XX_ASYNCIO to read data asynchronous as shown in
next chapter.
SPCM_XMODE_ASYNCOUT 00000002h Connector is programmed for asynchronous output. Use SPCM_XX_ASYNCIO to write data asynchronous as shown
in next chapter.
SPCM_XMODE_DIGIN 00000004h A/D cards only:
Connector is programmed for synchronous digital input. For each analog channel, one digital channel X0/X1/X2 is
integrated into the ADC data stream. Depending on the ADC resolution of your card the resulting merged samples
can have different formats. Please check the „Sample format“ chapter for more details. Please note that automatic
sign extension of analog data is ineffective as soon as one digital input line is activated and the software must prop-
erly mask out the digital bits.
SPCM_XMODE_DIGOUT 00000008h D/A cards only:
Connector is programmed for synchronous digital output. Digital channels can be „included“ within the analog sam-
ples and synchronously replayed along. Requires additional MODE bits to be set along with this flag, as explained
later on.
SPCM_XMODE_TRIGOUT 00000020h Connector is programmed as trigger output and shows the trigger detection. The trigger output goes HIGH as soon as
the trigger is recognized. After end of acquisition it is LOW again. In Multiple Recording/Gated Sampling/ABA
mode it goes LOW after the acquisition of the current segment stops. In FIFO single mode the trigger output is HIGH
until FIFO mode is stopped.
SPCM_XMODE_DIGIN2BIT 00000080h Connector is programmed for digital input. For each analog channel, two digital channels X0/X1/X2 are integrated
into the ADC data stream. Depending on the ADC resolution of your card the resulting merged samples can have dif-
ferent formats. Please check the data format chapter to see more details. Please note that automatic sign extension of
analog data is ineffective as soon as one digital input line is activated and the software must properly mask out the
digital bits.
SPCM_XMODE_RUNSTATE 00000100h Connector shows the current run state of the card. If acquisition/output is running the signal is HIGH. If card has
stopped the signal is LOW.
SPCM_XMODE_ARMSTATE 00000200h Connector shows the current ARM state of the card. If the card is armed and ready to receive a trigger the signal is
HIGH. If the card isn’t running or the card is still acquiring pretrigger data or the trigger has been detected the signal
is LOW.
SPCM_XMODE_REFCLKOUT 00001000h Connector reflects the internally generated PLL reference clock in the range of 10 to 62.5 MHz.
SPCM_XMODE_CONTOUTMARK 00002000h Generator Cards only: outputs a HIGH pulse as continuous marker signal for continuous replay mode. The marker sig-
nal length is ½ of the programmed memory size.
(c) Spectrum Instrumentation GmbH 132

Multi Purpose I/O Lines On-board I/O lines (X0, X1, X2)
SPCM_XMODE_SYSCLKOUT 00004000h Connector reflects the internally generated system clock in the range of 2.5 up to 156.25 MHz.
SPCM_XMODE_PULSEGEN 00080000h A/D and D/A cards only (optional):
Connector reflects the output of the same index pulse generator (X0 output from pulse generator 0, X1 from pulse gen-
erator 1 etc.). For details on the pulse generator option please consult the “Pulse Generator (Option)” chapter.
Please note that a change to the SPCM_X0_MODE, SPCM_X1_MODE or SPCM_X2_MODE will only be updated
with the next call to either the M2CMD_CARD_START or M2CMD_CARD_WRITESETUP register. For further de-
tails please see the relating chapter on the M2CMD_CARD registers.
Using asynchronous I/O
To use asynchronous I/O on the multi purpose I/O lines it is first necessary to switch these lines to the desired asynchronous mode by pro-
gramming the above explained mode registers. As a special feature asynchronous input can also be read if the mode is set to trigger input
or digital input.
Table 110: Spectrum API: asynchronous I/O register settings of the multi-purpose I/O registers
Register Value Direction Description
SPCM_XX_ASYNCIO 47220 read/write Connector X0 is linked to bit 0 of the register, connector X1 is linked to bit 1 while connector X2 is
linked to bit 2 of this register. Data is written/read immediately without any relation to the currently
used sampling rate or mode. If a line is programmed to output, reading this line asynchronously will
return the current output level.
Example of asynchronous write and read. We write a high pulse on output X1 and wait for a high level answer on input X0:
spcm_dwSetParam_i32 (hDrv, SPCM_X0_MODE, SPCM_XMODE_ASYNCIN); // X0 set to asynchronous input
spcm_dwSetParam_i32 (hDrv, SPCM_X1_MODE, SPCM_XMODE_ASYNCOUT); // X1 set to asynchronous output
spcm_dwSetParam_i32 (hDrv, SPCM_X2_MODE, SPCM_XMODE_TRIGOUT); // X2 set to trigger output
spcm_dwSetParam_i32 (hDrv, SPCM_XX_ASYNCIO, 0); // programming a high pulse on output
spcm_dwSetParam_i32 (hDrv, SPCM_XX_ASYNCIO, 2);
spcm_dwSetParam_i32 (hDrv, SPCM_XX_ASYNCIO, 0);
do {
spcm_dwGetParam_i32 (hDrv, SPCM_XX_ASYNCIO, &lAsyncIn); // read input in a loop
} while ((lAsyncIn & 1) == 0); // until X0 is going to high level
Special behavior of trigger output
As the driver of the M4i/M4x series is the same as the driver for the M2i/M3i series and some old software may rely on register structure of
the M2i/M3i card series, there is a special compatible trigger output register that will work according to the M2i/M3i series style. It is not
recommended to use this register unless you’re writing software for multiple card series:
Table 111: Spectrum API: additional trigger output register for compatibility with older hardware
Register Value Direction Description
SPC_TRIG_OUTPUT 40100 read/write M2i style trigger output programming. Write a „1“ to enable:
- X2 trigger output (SPCM_X2_MODE = SPCM_XMODE_TRIGOUT)
- X1 arm state (SPCM_X1_MODE = SPCM_XMODE_ARMSTATE).
- X0 run state (SPCM_X0_MODE = SPCM_XMODE_RUNSTATE).
Write a „0“ to disable all three outputs:
- SPCM_X0_MODE = SPCM_X1_MODE = SPCM_X2_MODE = SPCM_XMODE_DISABLE
The SPC_TRIG_OUTPUT register overrides the multi purpose I/O settings done by SPCM_X0_MODE, SPCM_X-
1_MODE and SPCM_X2_MODE and vice versa. Do not use both methods together from within one program.
(c) Spectrum Instrumentation GmbH 133

Multi Purpose I/O Lines On-board I/O lines (X0, X1, X2)
Using synchronous digital outputs
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
 This mode allows the user to replay up to three additional digital channels that are synchronous and phase stable along with the analog data.
To enable that mode for a particular Multi Purpose I/O line the the digital output mode must selected along with some additional information:
Table 112: Spectrum API: multi-purpose I/O registers and synchronous digital output settings
| Register | Value Direction | Description |     |
| -------- | --------------- | ----------- | --- |
SPCM_X0_AVAILMODES 47210 read Bitmask with all bits of the below mentioned modes showing the available modes for (X0)
SPCM_X1_AVAILMODES 47211 read Bitmask with all bits of the below mentioned modes showing the available modes for (X1)
SPCM_X2_AVAILMODES 47212 read Bitmask with all bits of the below mentioned modes showing the available modes for (X2)
SPCM_X0_MODE 47200 read/write Defines the mode for (X0). Only one mode selection is possible to be set at a time
SPCM_X1_MODE 47201 read/write Defines the mode for (X1). Only one mode selection is possible to be set at a time
SPCM_X2_MODE 47202 read/write Defines the mode for (X2). Only one mode selection is possible to be set at a time
| SPCM_XMODE_DIGOUT | 00000008h D/A cards only: |     |     |
| ----------------- | ------------------------- | --- | --- |
Connector is programmed for synchronous digital output. Digital channels can be „included“ within the analog sam-
ples and synchronously replayed along. Requires additional MODE bits to be set along with this flag, as explained
later on.
Additional constants that must be combined together with SPCM_XMODE_DIGOUT to select the analog channel or channels containing the
digital data information and also the bit of the combined data word to be used for digital output:
| SPCM_XMODE_DIGOUTSRC_CH0 | 01000000h Select channel 0 as source (channel 0 must be enabled for replay). |     |     |
| ------------------------ | ---------------------------------------------------------------------------- | --- | --- |
| SPCM_XMODE_DIGOUTSRC_CH1 | 02000000h Select channel 1 as source (channel 1 must be enabled for replay). |     |     |
| SPCM_XMODE_DIGOUTSRC_CH2 | 04000000h Select channel 2 as source (channel 2 must be enabled for replay). |     |     |
| SPCM_XMODE_DIGOUTSRC_CH3 | 08000000h Select channel 3 as source (channel 3 must be enabled for replay). |     |     |
SPCM_XMODE_DIGOUTSRC_BIT15 00100000h Use Bit15 of selected channel: channel’s resolution will be reduced to 15 bit.
SPCM_XMODE_DIGOUTSRC_BIT14 00200000h Use Bit14 of selected channel: channel’s resolution will be reduced to 14 bit, even if bit 15 is not used for digital
replay.
SPCM_XMODE_DIGOUTSRC_BIT13 00400000h Use Bit13 of selected channel: channel’s resolution will be reduced to 13 bit, even if bit 15 and/or bit 14 are not
used for digital replay.
A channel’s samples can contain also information for the synchronous digital output channels, with up to three digital channels combined
with the analog sample within one data word. When extracting the digital channels form the data word, the analog data will automatically
be shifted upwards, to not loose any gain information. The analog data is still in the same twos complement format.
Table 113: Spectrum API: data format and DAC resolution depending on selected mode and digital output modes
Standard Mode Digital outputs enabled Digital outputs enabled Digital outputs enabled
No embedded digital Bit 1 embedded digital Bit 2 embedded digital Bits 3 embedded digital Bits
Data bit 16 bit DAC resolution 15 bit DAC resolution 14 bit DAC resolution 13 bit DAC resolution
D15 DAx Bit 15 (MSB) Digital „Bit15“ of channel x Digital „Bit15“ of channel x Digital „Bit15“ of channel x
D14 DAx Bit 14 DAx Bit 15 (MSB) Digital „Bit14“ of channel x Digital „Bit14“ of channel x
D13 DAx Bit 13 DAx Bit 14 DAx Bit 15 (MSB) Digital „Bit13“ of channel x
| D12 DAx Bit 12 | DAx Bit 13 | DAx Bit 14 | DAx Bit 15 (MSB) |
| -------------- | ---------- | ---------- | ---------------- |
| D11 DAx Bit 11 | DAx Bit 12 | DAx Bit 13 | DAx Bit 14       |
| D10 DAx Bit 10 | DAx Bit 11 | DAx Bit 12 | DAx Bit 13       |
| D9 DAx Bit 9   | DAx Bit 10 | DAx Bit 11 | DAx Bit 12       |
| D8 DAx Bit 8   | DAx Bit 9  | DAx Bit 10 | DAx Bit 11       |
| D7 DAx Bit 7   | DAx Bit 8  | DAx Bit 9  | DAx Bit 10       |
| D6 DAx Bit 6   | DAx Bit 7  | DAx Bit 8  | DAx Bit 9        |
| D5 DAx Bit 5   | DAx Bit 6  | DAx Bit 7  | DAx Bit 8        |
| D4 DAx Bit 4   | DAx Bit 5  | DAx Bit 6  | DAx Bit 7        |
| D3 DAx Bit 3   | DAx Bit 4  | DAx Bit 5  | DAx Bit 6        |
| D2 DAx Bit 2   | DAx Bit 3  | DAx Bit 4  | DAx Bit 5        |
| D1 DAx Bit 1   | DAx Bit 2  | DAx Bit 3  | DAx Bit 4        |
D0 DAx Bit 0 (LSB) DAx Bit 1 (LSB) DAx Bit 2 (LSB) DAx Bit 3 (LSB)
 This very flexible routing allows the use of one up to three digital outputs, whose data in included in the samples of only one, two or three
different channels. This allows to only enable as much digital channels as needed, whilst keeping the resolution of the analog channels as
high as possible.
(c) Spectrum Instrumentation GmbH 134

Multi Purpose I/O Lines On-board I/O lines (X0, X1, X2)
The following example shows the generation of analog data on four channels with two channels sourcing all three digital outputs:
uint32 dwXMode;
// enable all four channels
spcm_dwSetParam_i32 (hDrv, SPC_CHENABLE, CHANNEL0 | CHANNEL1 | CHANNEL2 | CHANNEL3);
// X0 set to synchronous output Bit 15 of channel 0
dwXMode = (SPCM_XMODE_DIGOUT | SPCM_XMODE_DIGOUTSRC_CH0 | SPCM_XMODE_DIGOUTSRC_BIT15);
spcm_dwSetParam_i32 (hDrv, SPCM_X0_MODE, dwXMode);
// X1 set to synchronous output Bit 15 of channel 1
dwXMode = (SPCM_XMODE_DIGOUT | SPCM_XMODE_DIGOUTSRC_CH1 | SPCM_XMODE_DIGOUTSRC_BIT15);
spcm_dwSetParam_i32 (hDrv, SPCM_X1_MODE, dwXMode);
// X2 set to synchronous output Bit 14 of channel 1
dwXMode = (SPCM_XMODE_DIGOUT | SPCM_XMODE_DIGOUTSRC_CH1 | SPCM_XMODE_DIGOUTSRC_BIT14);
spcm_dwSetParam_i32 (hDrv, SPCM_X2_MODE, dwXMode);
The following example shows the generation of analog data on just one channel sourcing all three digital outputs:
uint32 dwXMode;
// enable only one channel
spcm_dwSetParam_i32 (hDrv, SPC_CHENABLE, CHANNEL0);
// X0 set to synchronous output Bit 15 of channel 0
dwXMode = (SPCM_XMODE_DIGOUT | SPCM_XMODE_DIGOUTSRC_CH0 | SPCM_XMODE_DIGOUTSRC_BIT15);
spcm_dwSetParam_i32 (hDrv, SPCM_X0_MODE, dwXMode);
// X1 set to synchronous output Bit 14 of channel 0
dwXMode = (SPCM_XMODE_DIGOUT | SPCM_XMODE_DIGOUTSRC_CH0 | SPCM_XMODE_DIGOUTSRC_BIT14);
spcm_dwSetParam_i32 (hDrv, SPCM_X1_MODE, dwXMode);
// X2 set to synchronous output Bit 13 of channel 0
dwXMode = (SPCM_XMODE_DIGOUT | SPCM_XMODE_DIGOUTSRC_CH0 | SPCM_XMODE_DIGOUTSRC_BIT13);
spcm_dwSetParam_i32 (hDrv, SPCM_X2_MODE, dwXMode);
The following example shows the generation of analog data on two channels sourcing the one synchronous digital output:
uint32 dwXMode;
// enable two channels
spcm_dwSetParam_i32 (hDrv, SPC_CHENABLE, CHANNEL0 | CHANNEL1);
// X0 set to synchronous output Bit 15 of channel 1
dwXMode = (SPCM_XMODE_DIGOUT | SPCM_XMODE_DIGOUTSRC_CH1 | SPCM_XMODE_DIGOUTSRC_BIT15);
spcm_dwSetParam_i32 (hDrv, SPCM_X0_MODE, dwXMode);
// X1 set to trigger output
dwXMode = (SPCM_XMODE_DIGOUT | SPCM_XMODE_TRIGOUT);
spcm_dwSetParam_i32 (hDrv, SPCM_X1_MODE, dwXMode);
(c) Spectrum Instrumentation GmbH 135

Mode Multiple Replay Trigger Modes
