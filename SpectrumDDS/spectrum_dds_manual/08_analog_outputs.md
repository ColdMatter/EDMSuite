Analog Outputs
Channel Selection
One key setting that influences all other possible settings is the channel enable register. A unique feature of the Spectrum cards is the possibility
to program the number of channels you want to use. All on-board memory can then be used by these activated channels.
This description shows you the channel enable register for the complete card family. However, your specific board may have less channels
depending on the card type that you have purchased and therefore does not allow you to set the maximum number of channels shown here.
Table 34: Spectrum API: channel enable register and register settings
| Register |     | Value Direction | Description |     |     |
| -------- | --- | --------------- | ----------- | --- | --- |
SPC_CHENABLE 11000 read/write Sets the channel enable information for the next card run.
| CHANNEL0 |     | 1 Activates channel 0 |     |     |     |
| -------- | --- | --------------------- | --- | --- | --- |
| CHANNEL1 |     | 2 Activates channel 1 |     |     |     |
| CHANNEL2 |     | 4 Activates channel 2 |     |     |     |
| CHANNEL3 |     | 8 Activates channel 3 |     |     |     |
The channel enable register is set as a bitmap. That means that one bit of the value corresponds to one channel to be activated. To activate
more than one channel the values have to be combined by a bitwise OR.
Example showing how to activate 4 channels:
spcm_dwSetParam_i64 (hDrv, SPC_CHENABLE, CHANNEL0 | CHANNEL1 | CHANNEL2 | CHANNEL3);
The following table shows all allowed settings for the channel enable register when your card has a maximum of 1 channel.
Channels to activate
| Ch0 |     | Values to program |     | Value as hex | Value as decimal |
| --- | --- | ----------------- | --- | ------------ | ---------------- |
| X   |     | CHANNEL0          |     | 1h           | 1                |
The following table shows all allowed settings for the channel enable register when your card has a maximum of 2 channels.
Channels to activate
| Ch0 | Ch1 | Values to program   |     | Value as hex | Value as decimal |
| --- | --- | ------------------- | --- | ------------ | ---------------- |
| X   |     | CHANNEL0            |     | 1h           | 1                |
|     | X   | CHANNEL1            |     | 2h           | 2                |
| X   | X   | CHANNEL0 | CHANNEL1 |     | 3h           | 3                |
The following table shows all allowed settings for the channel enable register in case that you have a four channel card.
Channels to activate
Ch0 Ch1 Ch2 Ch3 Values to program Value as hex Value as decimal
| X   |     | CHANNEL0                                  |     | 1h  | 1   |
| --- | --- | ----------------------------------------- | --- | --- | --- |
| X   |     | CHANNEL1                                  |     | 2h  | 2   |
|     | X   | CHANNEL2                                  |     | 4h  | 4   |
|     | X   | CHANNEL3                                  |     | 8h  | 8   |
| X X |     | CHANNEL0 | CHANNEL1                       |     | 3h  | 3   |
| X   | X   | CHANNEL0 | CHANNEL2                       |     | 5h  | 5   |
| X   | X   | CHANNEL0 | CHANNEL3                       |     | 9h  | 9   |
| X   | X   | CHANNEL1 | CHANNEL2                       |     | 6h  | 6   |
| X   | X   | CHANNEL1 | CHANNEL3                       |     | Ah  | 10  |
|     | X X | CHANNEL2 | CHANNEL3                       |     | Ch  | 12  |
| X X | X X | CHANNEL0 | CHANNEL1 | CHANNEL2 | CHANNEL3 |     | Fh  | 15  |
Any channel activation mask that is not shown here is not valid. If programming an other channel activation,
the driver will return with an error code ERR_VALUE.
To help user programs it is also possible to read out the number of activated channels that correspond to the currently programmed bitmap.
Table 35: Spectrum API: channel count register
| Register |     | Value Direction | Description |     |     |
| -------- | --- | --------------- | ----------- | --- | --- |
SPC_CHCOUNT 11001 read Reads back the number of currently activated channels.
(c) Spectrum Instrumentation GmbH 89

Analog Outputs Setting up the outputs
Reading out the channel enable information can be done directly after setting it or later like this:
spcm_dwSetParam_i32 (hDrv, SPC_CHENABLE, CHANNEL0 | CHANNEL1);
spcm_dwGetParam_i32 (hDrv, SPC_CHENABLE, &lActivatedChannels);
spcm_dwGetParam_i32 (hDrv, SPC_CHCOUNT, &lChCount);
printf ("Activated channels bitmask is: 0x%08x\n", lActivatedChannels);
printf ("Number of activated channels with this bitmask: %d\n", lChCount);
Assuming that the two channels are available on your card the program will have the following output:
Activated channels bitmask is: 0x00000003
Number of activated channels with this bitmask: 2
Important note on channel selection
As some of the manuals passages are used in more than one hardware manual most of the registers and
channel settings throughout this handbook are described for the maximum number of possible channels that
are available on one card of the current series. There can be less channels on your actual type of board or
bus-system. Please refer to the technical data section to get the actual number of available channels.
Setting up the outputs
Output Enable
The output of each channel can be completely disabled by software command at any time. Disabling the output will cut off the amplifier from
the connector with the help of a Relay. Therefore the programmable stoplevel (see below) has no influence if disabling the output. Instead the
output is galvanically interrupted and has no defined level any more. If a defined output level is needed the AWG output must be terminated
externally.
Table 36: Spectrum API: output enable register and register settings
Register Value Direction Description
SPC_ENABLEOUT0 30091 read/write Enables (write 1) or Disables (write 0) the output of channel 0
SPC_ENABLEOUT1 30191 read/write Enables (write 1) or Disables (write 0) the output of channel 1
SPC_ENABLEOUT2 30291 read/write Enables (write 1) or Disables (write 0) the output of channel 2
SPC_ENABLEOUT3 30391 read/write Enables (write 1) or Disables (write 0) the output of channel 3
This arbitrary waveform generator board uses separate output am-
plifiers for each channel. This gives you the possibility to separately
set up the channel outputs to best suit your application.
The output amplifiers can easily be set by the corresponding ampli-
tude registers.
The table below shows the available registers to set up the output
amplitude for your type of board.
Image 46: Scaling the output swing using the output amplitude registers
Table 37: Spectrum API: output amplitude registers and register settings depending on board type
Register Value Direction Description Amplitude range Amplitude range
M4i.6620 M4i.6630
M4i.6621 M4i.6631
M4i.6622
SPC_AMP0 30010 read/write Defines the amplitude of channel0 into 50 Ohm load in mV. 80 up to 2500 (in mV) 80 up to 2000 (in mV)
SPC_AMP1 30110 read/write Defines the amplitude of channel1 into 50 Ohm load in mV. 80 up to 2500 (in mV) 80 up to 2000 (in mV)
SPC_AMP2 30210 read/write Defines the amplitude of channel2 into 50 Ohm load in mV. 80 up to 2500 (in mV) 80 up to 2000 (in mV)
SPC_AMP3 30310 read/write Defines the amplitude of channel3 into 50 Ohm load in mV. 80 up to 2500 (in mV) 80 up to 2000 (in mV)
(c) Spectrum Instrumentation GmbH 90

Analog Outputs Setting up the outputs
The output stage has a 50 Ohm series termination. If not terminating the output with 50 Ohm externally this
will result into an output level of double the programmed level. A programmed amplitude of 2000 mV (4000
mV peak-to-peak voltage) will result into an amplitude of 4000 mV (8000 mV peak-to-peak voltage) into
high-impedance load !
To get the range of allowed values of the SPC_AMPx registers, there are the following specific minimum and maximum value registers.
Table 38: Spectrum API: output amplitude minimum and maximum values registers.
Register Value Direction Description
SPC_READAOGAINMIN 9100 read Read out the minimal setting of the AMPx register.
SPC_READAOGAINMAX 9110 read Read out the maximal setting of the AMPx register.
Output Amplitude Setting and Hysteresis
The output amplitude can be changed at any time either while the output is stopped or even while the output is running. The output amplitude
is changed on-the-fly with immediate result in the output signal.
As the output amplifier consist of two different paths (low power and high power) with slightly different specifications, there is a break in the
continuous output amplitude change when switching from one output amplifier path to the other, as this is done with the help of a relay. When
switching from one path to the other the driver will automatically disable the output (zero volt level) for the „path switching time“ to avoid a
disturbed output signal. Please see the technical detail section for the specification of the two different output amplifier path settings.
To prevent the card from switching on and off when operating around the limit between the output amplifiers paths there’s a built-in hysteresis:
• If output amplifier is already in low power path the output path is switched at the upper border of the hysteresis (480 mV) allowing to use
the area between 80 mV and 480 mV with continuous and gap-free change of output amplifier amplitude.
• If output amplifier is already in high power path the output path is switched at the lower border of the hysteresis (420 mV) allowing to use
the area between 420 mV and 2500mV (M4i.662x, M4x.662x, M4i.962x, M4x.962x) or 2000 mV (M4i.663x, M4x.663x) with con-
tinuous and gap-free change of output amplifier amplitude.
Output Filters
Every output of your Spectrum D/A board is equipped with a bypass path and a
fixed filter that can be used for signal smoothing.
The filter is located in the signal chain between the output amplification section and
the DAC, as shown in the right figure. Depending on your type of board the filter
are of different filter types and have different cut off frequencies, as shown below.
You can choose between the different filters easily by setting the dedicated filter
Image 47: output stage showing amplifier and filters
registers. The registers and the possible values are shown in the table below.
Table 39: Spectrum API: output filter registers and register settings
Register
SPC_FILTER0 30080 read/write Sets the signal filter of channel0.
SPC_FILTER1 30180 read/write Sets the signal filter of channel1.
SPC_FILTER2 30280 read/write Sets the signal filter of channel2.
SPC_FILTER3 30380 read/write Sets the signal filter of channel3.
0 No filter is used on the corresponding channel.
1 Filter 1 is used on the corresponding channel. The type of filter depends on the type of board and is shown below.
Table 40: output filter specifications depending on card version
Filter Specifications M4i.6620-x8, M4x.6620-x4, M4i,9620-x8, M4x.9620-x4 M4i.6630-x8, M4x.6630-x4
M4i.6621-x8, M4x.6621-x4, M4i.9621-x8, M4x.9621-x4 M4i.6631-x8, M4x.6631-x4
M4i.6622-x8, M4x.6622-x4, M4i.9622-x8, M4x.9622-x4
filter 0 No filter will be used.
filter 1 -3 dB bandwidth 65 MHz 65 MHz
(c) Spectrum Instrumentation GmbH 91

Analog Outputs Setting up the outputs
Differential Output
The differential mode outputs the data on the even
channels and the inverted data on the odd channels
of one module, as the figure on the right is showing.
As a result you have differential signals, which are
more resistant against noise when being transmit-
ted via long cables. Because of the hardware gen-
eration, only one data sample in memory is needed
for one pair of differential outputs.
The dedicated registers to set up the differential
mode are shown below.
If your board has four installed channels you can
generate two pairs of differential signals, otherwise
one pair is possible.
Differential outputs are not available for all types of
boards. Please refer to the table below, which men-
tions the boards this mode is available on.
When switching to differential/double out mode the following settings need to match:
• The channel enable mask must contain the primary channel only. That means, for example, when switching to this mode on a four chan-
nel card with only channel 0 and channel 2 must be enables. Channel 1 and channels 3 must be disabled
• All output setup like output enable, offset, gain or filter must be programmed for each channel of the pair
• Each channel pair only receives one sample of data for output. The second channel will be automatically generated according to the
selected mode
Table 41: Spectrum API: differential output register and register settings
Register
SPC_DIFF0 30040 read/write Sets channel 0/1 to differential mode.
SPC_DIFF2 30240 read/write Sets channel 2/3 to differential mode.
Table 42: availability of differential output mode depending on AWG model
Mode M4i.6620 M4i.6621 M4i.6622 M4i.6630 M4i.6631
M4x.6620 M4x.6622 M4x.6622
M4i.9620 M4i.9623 M4i.9622
M4x.9620 M4x.9624 M4x.9622
Differential Output (AWG mode only) not available installed installed not available installed
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
Double Out Mode
The double out mode outputs the data on the even
channels and the same data on the odd channels
of one module, as the figure on the right is showing.
The dedicated registers to set up the differential
mode are shown below.
If your board has four installed channels you can
generate two pairs of identical signals, otherwise
only one pair is possible.
The double out mode is not available for all types
of boards. Please refer to the table below, which
mentions the boards this mode is available on.
When switching to differential/double out mode
the following settings need to match:
• The channel enable mask must contain the pri-
mary channel only. That means, for example,
when switching to this mode on a four channel
card with only channel 0 and channel 2 must be Image 48: schematics of double output mode
enables. Channel 1 and channels 3 must be dis-
(c) Spectrum Instrumentation GmbH 92

Analog Outputs Setting up the outputs
abled
• All output setup like output enable, offset, gain or filter must be programmed for each channel of the pair
• Each channel pair only receives one sample of data for output. The second channel will be automatically generated according to the
selected mode
Table 43: Spectrum API: double output mode registers
Register
SPC_DOUBLEOUT0 30041 read/write Sets channel 0/1 to double out mode.
SPC_DOUBLEOUT2 30241 read/write Sets channel 2/3 to double out mode.
Table 44: availability of double output mode depending on AWG model
Mode M4i.6620 M4i.6621 M4i.6622 M4i.6630 M4i.6631
M4x.6620 M4x.6622 M4x.6622
M4i.9620 M4i.9623 M4i.9622
M4x.9620 M4x.9624 M4x.9622
DoubleOut mode (AWG mode only) not available installed installed not available installed
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
Programming the behavior in pauses and after replay
Usually the used outputs of the analog generation boards are set to zero level after replay. This is in most cases adequate. In some cases it
can be necessary to hold the last sample, to output the maximum positive level or maximum negative level after replay. The stoplevel will stay
on the defined level until the next output will be made. With the following registers you can define the behavior after replay, when using the
cards in AWG mode:
Table 45: Spectrum API: stop level register and register settings
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
Table 46: Spectrum API: custom stop level registers
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
Using this mode/feature requires the SPCM_FEAT_EXTFW_AWG feature to be installed and an AWG capable
firmware image to be active. For details see “Feature and firmware matrix of AWG and DDS products” par-
agraph in the “Introduction” chapter.
(c) Spectrum Instrumentation GmbH 93

Analog Outputs Setting up the outputs
Read out of output features
The analog outputs of the different cards do have different features implemented, that can be read out to make the software more general. If
you only operate one single card type in your software it is not necessary to read out these features.
Please note that the following table shows all output feature settings that are available throughout all Spectrum generator cards. Some of these
features are not installed on your specific hardware.
Table 47: Spectrum API: reading out the available features of the analog outputs
Register
SPC_READAOFEATURES 3102 read Returns a bit map with the available features of the analog output path. The possible return values are
listed below.
SPCM_AO_SE 00000002h Output is single-ended. If available together with SPC_AO_DIFF: output type is software selectable.
SPCM_AO_DIFF 00000004h Output is differential. If available together with SPC_AO_SE: output type is software selectable.
SPCM_AO_PROGFILTER 00000008h Software selectable output filters are available.
SPCM_AO_PROGOFFSET 00000010h Output offset is software programmable.
SPCM_AO_PROGGAIN 00000020h Output gain is software programmable.
SPCM_AO_PROGSTOPLEVEL 00000040h The output level between segments and after replay of generated data is programmable.
SPCM_AO_DOUBLEOUT 00000080h Double out mode is available allowing to generate cheap copies of even channel data on odd channels outputs for
driving multiple loads.
SPCM_AO_ENABLEOUT 00000100h The output of each channel can be completely disabled by software command at any time.
SPCM_AO_TRUEDIFF 00000200h Card is equipped with true differential outputs on dedicated +/- connectors.
SPCM_AO_AUTOCALOFFS 00000400h Output offset can be auto calibrated on the card.
SPCM_AO_AUTOCALGAIN 00000800h Output gain can be auto calibrated on the card.
(c) Spectrum Instrumentation GmbH 94

Generation modes Overview
