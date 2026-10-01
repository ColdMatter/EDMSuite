Introduction
Preface
This manual provides detailed information on the hardware features of your Spectrum board. This information includes technical data,
specifications, block diagram and a connector description.
In addition, this guide takes you through the process of installing your board and also describes the installation of the delivered driver package
for each operating system.
Finally this manual provides you with the complete software information of the board and the related driver. The reader of this manual will
be able to integrate the board in any PC system with one of the supported bus and operating systems.
Please note that this manual provides no description for specific driver parts such as those for IVI, LabVIEW or MATLAB. These driver manuals
are available on USB-Stick or on the Spectrum website.
For any new information on the board as well as new available options or memory upgrades please contact our website
www.spectrum-instrumentation.com. You will also find the current driver package with the latest bug fixes and new features on our site.
Please read this manual carefully before you install any hardware or software. Spectrum is not responsible
for any hardware failures resulting from incorrect usage.
Overview
M4i cards for PCIE Express (PCIe)
The M4i generation is the fast streaming and high performance
platform from Spectrum. The ¾ length cards are available in dif-
ferent speed grades and resolutions with best performance.
The cards have been optimized for extremely fast data transfer
and allow to read data for online analysis or offline storage with
more than 3 GB/s using the PCI Express x8 Gen 2 bus interface.
Mechanically the card family needs x8 or x16 lane PCI Express connectors with any PCI Ex-
press generation. Electrically the card can handle smaller number of PCI Express lanes with
reduced transfer speed.
When using high sampling rates the 4 GiByte standard on-board memory (2 GiSamples for cards with 12/14/16 bit resolution) is sufficient
to acquire up to several seconds of high-speed data. The M4i cards are carefully designed and offer an optimized clock section, a wide
range of trigger possibilities, new and improved features, easy usability and programming as well as an outstanding software support.
The PCI Express bus was first introduced in 2004. In today’s standard PC there are usually two to six slots available for instrumentation boards.
Special industrial PCs offer up to a maximum of 16 slots. The PCI Express Gen2 standard theoretically delivers up to 8 GByte/s data transfer
rate per x16 slot. The Spectrum M4i boards are available as PCIExpressx8 (eight lane) Gen2, 3/4 length card.
Within this document the name M4i or M4i.xxxx is used as a synonym for the PCI Express version with the full name of
M4i.xxxx-x8 to enhance readability. The exact order information can be found in the related passage in this manual.
M4x cards for PXI Express (PXIe)
The M4x platform takes the features of the M4i series of PCIe cards to
an industrial bus standard. The 3U two slot cards are available in differ-
ent speed grades and resolutions with best performance.
Based on Spectrums proven M4i series of PCIe products the new M4x
PXIe modules deliver the same advanced features and signal quality.
It also allows the new modules to share a common software interface and offer the same FPGA
based averaging and statistics options.
Compared to PCIe, PXIe systems come with superior mechanical design, better connectors and a
defined air flow for cooling. This makes it the ideal platform for many industrial and mobile appli-
cations.
(c) Spectrum Instrumentation GmbH 14

Introduction General Information
The M4x cards have been optimized for extremely fast data transfer and allow to read data for online analysis or
offline storage with more than 1.5 GB/s using the PCI Express x4 Gen 2 bus interface.
When using high sampling rates the 4 GiByte standard on-board memory (2 GiSamples for cards with 12/14/16
bit resolution) is sufficient to acquire up to several seconds of high-speed data. The M4x cards are carefully designed
and offer an optimized clock section, a wide range of trigger possibilities, new and improved features, easy usability
and programming as well as an outstanding software support.
Within this document the name M4x or M4x.xxxx is used as a synonym for the PXI Express version with the full name of
M4x.xxxx-x4 to enhance readability. The exact order information can be found in the related passage in this manual.
General Information
M4i.66xx / M4x.66xx - Arbitrary Waveform Generators
The M4i.66xx and M4x.66xx Arbitrary Waveform Generator series offer a wide range of ultra fast 16bit D/A converter boards for PCI
Express (PCIe) and PXI Express (PXIe) bus. Due to their well though-out design, these boards are available in several versions and different
speed grades. That makes it possible for the user to find a perfectly matching solution.
These boards offer one, two or four channels with a maximum sampling rate of 625MS/s in addition to models with one or two channels
with maximum sampling rates up to 1.25GS/s. The installed memory of 2GiSample will be used for fast data replay. It can completely be
used by the currently active channels. Alternatively the memory can be turned into a FIFO buffer and data will be transferred online from the
PC memory or from hard disk.
Optionally these boards can be equipped with a DDS20 option, allowing the cards to create 20-tone sine-wave signals on the fly in hardware,
with minimum of data to be transferred to the board.
Several boards of the M4i.xxxx series may be connected together by the internal standard synchronization bus in combination with one of
the star-hub options to work with the same time base. That allows to build system with multiple D/A channels or systems with combined A/D
and D/A channels.
Application examples: Automatic test systems, IQ Signal Generation, Stimulus Response Measurements, Noise Gen-
eration, Prototype Design, Production Test
M4i.96xx / M4x.96xx - Direct Digital Synthesis Generators
The M4i.96xx and M4x.96xx products of Direct Digital Synthesis generators offer a wide range of fast, 50-tone signal generator boards with
16bit D/A converter technology for PCI Express (PCIe) and PXI Express (PXIe) bus.
Due to their design being based on proven Spectrum’s proven D/A technology, these boards are available in several versions and different
speed grades. That makes it possible for the user to find here as well a perfectly matching solution.
These boards offer one, two or four channels with a maximum DDS update rate of 625MS/s. The installed memory of 2GiSample can be
used as command buffer for up to 512MiCommands for storing millions of DDS changes.
Optionally these boards can be equipped with an AWG option allowing to replay fully arbitrary waveforms on all active channels synchro-
nously. Then the on-board memory can then be turned into a FIFObuffer and full arbitrary data will be transferred online from the PC memory
or from hard disk.
Several boards of the M4i.xxxx series may be connected together by the internal standard synchronization bus in combination with one of
the star-hub options to work with the same time base. That allows to build system with multiple D/A channels or systems with combined A/D
and D/A channels.
Application examples: Quantum Research, IQ Signal Generation, MIMO Phased Array, Stimulus Response Measure-
ments, ADC Performance Evaluation, Prototype Testing
(c) Spectrum Instrumentation GmbH 15

Introduction Introduction to the AWG and DDS principle
Introduction to the AWG and DDS principle
Both principles, AWG and DDS, rely on high-speed digital-to-analog converters (DAC) for generating analog output waveforms by feeding
the DAC with a stream of digital samples, which in turn will be converted into time varying analog output voltages.
In case of an AWG, the digital samples must be provided by the application software and hence the content of each and every sample is
under full software control. This allows the generation of virtually any shape of signal, hence the term “arbitrary”. But with this freedom also
comes the responsibility of providing all the samples fast enough, so that the DAC is provided with an un-interrupted data stream.
The DDS on the other hand uses a different method of generating these analog values. The hardware itself works as sort of a numerically
controlled oscillator (NCO). This oscillator can be configured to generate periodic sine wave signals with defined amplitude, frequency,
phase offset from a fixed-frequency reference clock. Additionally these NCO can apply hardware-controlled intrinsic dynamic linear slope
functions to produce extremely smooth changes to frequency and amplitude.
By controlling these relatively few parameters just some commands instead of providing the full data stream, saves a huge amount of data,
but comes at the “cost”, such that the generated signals can only be periodic in nature and sine waves.
Feature and firmware matrix of AWG and DDS products
Both product lines, the 66xx series of arbitrary waveform generators (AWG) and the 96xx series of direct digital synthesizers (DDS) are
based on the proven D/A hardware of the 66xx series AWGs. However the different series of cards come with different installed firmware
version and different feature licenses.
This allows to use the AWG products of the 66xx series to optionally also operate as a multi-tone DDS generator, or use the DDS products
of the 96xx series to optionally also generate true arbitrary waveforms.
Depending on the card series, different combination of features/modes are available. Some of these modes
operate from within the same firmware configuration, whilst other modes require the card to have a specific
firmware configuration active. Passages where this matters are marked with the symbol shown here.
The different combinations of generation modes and required active firmware configurations are shown in the following table:
Table 4: Feature and firmware matrix of 66xx and 96xx products
Generation mode Available with active firmware 66xx 96xx
True arbitrary waveform generation (AWG) Configuration 0 installed as default Option available
(M4i.96xx-AWG)
Allows usage of all card modes except SPC_REP_STD_DDS.
DDS for generating up to 20 tones on a single output channel Option available currently not available
(M4i.66xx-DDS20)
Allows usage of card mode SPC_REP_STD_DDS.
DDS for generating up to 50 tones on a single output channel Configuration 1 currently not available installed as default
Allows usage of card mode SPC_REP_STD_DDS.
66xx
These cards come with the default firmware configuration 0 and with the AWG license installed, so that all AWG modes can be used. Op-
tionally the feature “M4i.66xx-DDS20” can be installed. With both features installed, this allows usage of the 20-tone DDS or the AWG
modes.
Since AWG and DDS20 require the same firmware configuration, the selection between AWG and DDS replay modes can be made by just
programming the card with different replay modes.
96xx
These cards come with the default firmware configuration 1 and with the DDS50 license installed.This allows the card to use the DDS mode
and to generate up to 50 tones on a single channel.
Optionally the feature “M4i.96xx-AWG” can be installed. With both features installed, this allows usage of the 50-tone DDS or the AWG
modes.
Since AWG and DDS50 require the different firmware configurations, the selection between AWG and DDS replay modes requires the cur-
rently active firmware image to be switched. This is done with the help of the Spectrum Control Center. Please see “Firmware switching”
paragraph in the “Card Control Center” section for details.
With the proper firmware image active either the use of the AWG modes or the use of the DDS mode is possible.
Since changing the firmware configuration requires to remove and re-apply power to the card, switching between AWG and DDS modes
cannot be made on the fly, but rather requires a power cycle. That in most cases also requires a shut-down and hence cold-start of the host
PC or generatorNETBOX.
(c) Spectrum Instrumentation GmbH 16

Introduction Different models of the M4i.66xx and M4i.96xx series
Different models of the M4i.66xx and M4i.96xx series
The following overview shows the different available models of the M4i.66xx and M4i.96xx series. They differ in the number of available
channels. You can also see the model dependent location of the input connectors.
• M4i.6620-x8
• M4i.6630-x8
• M4i.9620-x8
• M4i.6621-x8
• M4i.6631-x8
• M4i.9621-x8
• M4i.6622-x8
• M4i.9622-x8
(c) Spectrum Instrumentation GmbH 17

Introduction Different models of the M4x.66xx and M4x.96xx series
Different models of the M4x.66xx and M4x.96xx series
The following overview shows the different available models of the M4x.66xx and M4x.96xx series. They differ in the number of available
channels. You can also see the model dependent location of the input connectors.
• M4x.6620-x4 (EOL)
• M4x.6630-x4
• M4x.6621-x4
• M4x.6631-x4
• M4x.9621-x4
• M4x.6622-x8
• M4x.9622-x4
(c) Spectrum Instrumentation GmbH 18

Introduction Additional options
Additional options
Star-Hub (M4i only)
The star hub module allows the synchroni-
zation of up to 8 M4i cards. It is possible
to synchronize only cards of the same fam-
ily with each other.
Two different versions of the star-hub mod-
ule allowing the synchronization of up to
8 cards are available. A version that is
mounted on top of the carrier card as a
piggy-back module (option SH8tm) ex-
tending the width of the card to two slots.
The second version (option SH8ex) is
mounted behind the card and extends the
M4i base card to a full-length PCI Express
card. Therefore it requires the availability
of a full-length slot in the system but does
not need the space of an additional slot.
The module acts as a star hub for clock
and trigger signals. Each board is con-
nected with a small cable of the same
Image 1: M4i card showing mounted star-hub and sync bus connector
length, even the master board. That mini-
mizes the clock skew between the different
cards. The picture shows the piggy-back module mounted on the base board schematically without any cables to achieve a better visibility.
The carrier card acts as the clock master and the same or any other card can be the trigger master. All trigger modes that are available on
the master card are also available if the synchronization star-hub is used.
The cable connection of the boards is automatically recognized and checked by the driver when initializing the star-hub module. So no care
must be taken on how to cable the cards. The star-hub module itself is handled as an additional device just like any other card and the pro-
gramming consists of only a few additional commands.
Digital I/O with Dig-SMA (M4i.44xx only)
The Digital I/O options „Dig-SMA“ adds
eight additional Multi-Purpose I/O lines to
the card.
All eight lines are provided via SMA min-
iature coaxial connectors, just like the
analog channels or clock and trigger in-
put.
All of these lines are mounted on the PCI
bracket and are hence accessible from the
outside of the PC.
These lines extend the already existing
Multi-Purpose I/O lines that come stand-
ard with the main card (X0 .. X2) and are
intended to expand the number of availa-
ble digital lines, that can be recorded syn-
chronously alonside the analog channels.
Although the capabilities of these addi-
tional lines are not identical to those on the
main card, they are consistently named
Image 2: M4i card showing mounted digital option
(X3 .. X10).
Either one of the two Star-Hub or the DigSMA option can be mounted on any board physically at any given time. It is not possible
to mount a Star-Hub and a digital option DigSMA onto the same board.
(c) Spectrum Instrumentation GmbH 19

Introduction Hardware information (M4i.66xx and M4x.66xx)
Hardware information (M4i.66xx and M4x.66xx)
Block Diagrams
M4i.66xx Block Diagram
Image 3: M4i.66xx PCI Express series hardware block diagram
M4x.66xx Block Diagram
Image 4: M4x.66xx PXI Express series hardware block diagram
(c) Spectrum Instrumentation GmbH 20

| Introduction |     |     | Hardware information (M4i.66xx and M4x.66xx) |     |
| ------------ | --- | --- | -------------------------------------------- | --- |
Technical Data
Definitions
Specifications (spec)
Figures are valid for products stored for at least 2 hours inside the specified operating temperature range, after a 30 minute warm-up, after running an on-board calibration and with
properly cooled products. Data published in this document are specifications (spec) only where specifically indicated. Figures marked with (spec) are calibrated to the specification during
production and during factory calibrations.
Typical (typ)
Figures marked with (typ) are a characteristic performance that most of the manufactured instruments will meet due to design. Typical figures are measured during production but cannot
be calibrated.
Measured (meas)
A figure measured during development to communicate expected performance. The figure is measured in lab environment with an environmental temperature between 20°C and 25°C
and an altitude of less than 100 m.
Nominal (nom)
The value of this figure is determined by design and is not measured nor calibrated.
Analog Outputs
| Resolution (nom)  |     | 16 bit            |                    |                             |
| ----------------- | --- | ----------------- | ------------------ | --------------------------- |
| D/A Interpolation |     | no interpolation  |                    |                             |
|                   |     | M4i.662x/M4x.662x | M4i.663x/M4x.663x  |                             |
|                   |     | DN2.662/DN6.662x  | DN2.663/DN6.663    |                             |
|                   |     | DN2.82x-04        |                    | DN2.82x-02                  |
|                   |     | M4i.96xx/M4x.96xx | Standard Bandwidth | With high bandwidth option  |
|                   |     | DN2.96x/DN6.96x   |                    | (-hbw) installed            |
Output amplitude into 50  termination (nom) software programmable ±80 mV up to ±2.5 V  ±80 mV up to ±2 V ±80 mV up to ±480 mV
Output amplitude into high impedance loads (nom) software programmable ±160 mV up to ±5 V  ±160 mV up to ±4 V ±160 mV up to ±960 mV
Output amplitude stepsize (50 termination) (nom) 1 mV 1 mV 1 mV
| Stepsize of output amplitude (high impedance) |     | 2 mV | 2 mV | 2 mV |
| --------------------------------------------- | --- | ---- | ---- | ---- |
10% to 90% rise/fall time of 480mV pulse (meas) 1.5 ns 1.1 ns 440 ps
10% to 90% rise/fall time of 2000 mV pulse (meas) 1.5 ns 1.1 ns n.a.
| Output offset (nom) | fixed | 0 V |     |     |
| ------------------- | ----- | --- | --- | --- |
Output Amplifier Path Selection automatically by driver Low Power path: ±80 mV to ±480 mV (into 50 
High Power path: ±420 mV to ±2.5 V/±2 V (into 50 
Output Amplifier Setting Hysteresis automatically by driver 420 mV to 480 mV (if output is using low power path it will switch to high power path at
480mV. If output is using high power path it will switch to low power path at 420mV)
Output amplifier path switching time (meas) 10 ms (output disabled while switching)
| Filters                              | software programmable | bypass with no filter or one fixed filter |     |     |
| ------------------------------------ | --------------------- | ----------------------------------------- | --- | --- |
| DAC Differential non linearity (nom) | DNL, DAC only         | ±0.5 LSB typical                          |     |     |
| DAC Integral non linearity (nom)     | INL, DAC only         | ±1.0 LSB typical                          |     |     |
| Output resistance (nom)              |                       | 50                                       |     |     |
| Output coupling                      |                       | DC                                        |     |     |
| Minimum output load (nom)            |                       | 0  (short circuit safe)                  |     |     |
Output accuracy (spec) Low power path ±0.5 mV ±0.1% of programmed output amplitude
|                                 | High power path               | ±1.0 mV ±0.2% of programmed output amplitude |     |     |
| ------------------------------- | ----------------------------- | -------------------------------------------- | --- | --- |
| Offset temperature drift (meas) | after warm-up and calibration | TBD                                          |     |     |
| Gain temperature drift (meas)   | after warm-up and calibration | TBD                                          |     |     |
Calibration External External calibration calibrates the on-board references. All calibration constants are stored in
non-volatile memory. Optional ISO/IEC17025-compliant certificate. Optional ISO/IEC17025-
compliant certificate. A yearly external calibration is recommended.

Trigger
Available trigger modes software programmable External, Software, Window, Re-Arm, Or/And, Delay, PXI (M4x only)
Trigger edge software programmable Rising edge, falling edge or both edges
Trigger delay software programmable 0 to (8 GiSamples - 32) = 8589934560 Samples in steps of 32 samples
| Trigger accuracy (all sources)       |     | 1 sample    |      |     |
| ------------------------------------ | --- | ----------- | ---- | --- |
| Minimum external trigger pulse width |     | 2 samples |      |     |
| External trigger                     |     | Ext0        | Ext1 |     |
External trigger impedance (typ) software programmable 50  /1 k 1 k
External trigger coupling software programmable AC or DC fixed DC
External trigger type Window comparator Single level comparator
External input level (nom) ±10 V (1 k±2.5 V (50  ±10 V
External trigger sensitivity/signal swing) (nom) 2.5% of full scale range 2.5% of full scale range = 0.5 V
External trigger level (nom) software programmable ±10 V in steps of 10 mV ±10 V in steps of 10 mV
| External trigger maximum voltage (nom) |      | ±30V          | ±30 V         |     |
| -------------------------------------- | ---- | ------------- | ------------- | --- |
| External trigger bandwidth DC (typ)    | 50  | DC to 200 MHz | n.a.          |     |
|                                        | 1 k | DC to 150 MHz | DC to 200 MHz |     |
External trigger bandwidth AC (typ) 50  20 kHz to 200 MHz n.a.
| Minimum external trigger pulse width |     | 2 samples | 2 samples |     |
| ------------------------------------ | --- | ----------- | ----------- | --- |
| Multi, Gate: re-arming time (nom)    |     | 40 samples  |             |     |
Trigger to Output Delay (meas) sample rate  625 MS/s 238.5 sample clocks + 16 ns (valid for all modes except SPCSEQ_ENDLOOPONTRIG)
|     | sample rate > 625 MS/s | 476.5 sample clocks + 16 ns (valid for all modes except SPCSEQ_ENDLOOPONTRIG) |     |     |
| --- | ---------------------- | ----------------------------------------------------------------------------- | --- | --- |
Memory depth software programmable 32 up to [installed memory / number of active channels] samples in steps of 32
Multiple Replay segment size software programmable 16 up to [installed memory / 2 / active channels] samples in steps of 16

(c) Spectrum Instrumentation GmbH 21

Introduction Hardware information (M4i.66xx and M4x.66xx)
Clock
Clock Modes software programmable internal PLL, external reference clock, Star-Hub sync (generatorNETBOX and M4i only), PXI Ref-
erence Clock (M4x only)
| Internal clock accuracy (spec)                 | after warm-up         | ±5 ppm                |
| ---------------------------------------------- | --------------------- | ----------------------- |
| External reference clock range (nom)           | software programmable |  10 MHz and  1.25 GHz |
| External reference clock input impedance (nom) |                       | 50  fixed              |
| External reference clock input coupling        |                       | AC coupling             |
| External reference clock input edge            |                       | Rising edge             |
External reference clock input type Single-ended, sine wave or square wave
External reference clock input swing (meas) square wave 0.3 V peak-peak up to 3.0 V peak-peak
External reference clock input swing (meas) sine wave 1.0 V peak-peak up to 3.0 V peak-peak
External reference clock input max DC voltage (nom) ±30 V (with max 3.0 V difference between low and high level)
| External reference clock input duty cycle (nom) |                     | 45% to 55%                               |
| ----------------------------------------------- | ------------------- | ---------------------------------------- |
| External reference clock output type (nom)      |                     | Single-ended, 3.3V LVPECL                |
| Star-Hub synchronization clock modes            | software selectable | Internal clock, external reference clock |
| Channel to channel skew on one card (typ)       |                     | <250ps                                   |
| Skew between star-hub synchronized cards (typ)  |                     | <130ps                                   |
Internal clock setup granularity (nom) 8 Hz (internal reference clock only, restrictions apply to external reference clock)
| Setable Clock speeds |     | 50 MHz to max sampling clock |
| -------------------- | --- | ---------------------------- |
Clock Setting Gaps (nom) 750 to 757 MHz, 1125 to 1145 MHz (no sampling clock possible in these gaps)
| Clock output (nom) | sampling clock 71.68 MHz | Clock output = sampling clock/4 |
| ------------------ | ------------------------- | ------------------------------- |
| Clock output (nom) | sampling clock >71.68 MHz | Clock output = sampling clock/8 |

Sequence Replay Mode
| Required firmware version |     | At least V1.14 |
| ------------------------- | --- | -------------- |
Number of sequence steps software programmable 1 up to 4096 (sequence steps can be overloaded at runtime)
Number of memory segments software programmable 2 up to 64Ki (segment data can be overloaded at runtime)
Minimum segment size software programmable 384 samples (1 active channel), 192 samples (2 active channels),
96 samples (4 active channels), in steps of 32 samples.
Maximum segment size software programmable 2 GiS / active channels / number of sequence segments (round up to the next power of two)
| Loop Count | software programmable | 1 to (1Mi - 1) loops |
| ---------- | --------------------- | -------------------- |
Sequence Step Commands software programmable Loop for #Loops, Next, Loop until Trigger, End Sequence, Sequence Restart on Trigger
Special Commands software programmable Data Overload at runtime, sequence steps overload at runtime,
readout current replayed sequence step
Limitations for synchronized products Software commands changing the sequence as well as „Loop until trigger“ are not synchronized
between cards. This also applies to multiple AWG modules in a generatorNETBOX.
Synchronized products can run static sequences as well as sequence restart on trigger.

Multi Purpose I/O lines (front-plate)
| Number of multi purpose lines |                       | three, named X0, X1, X2 |
| ----------------------------- | --------------------- | ----------------------- |
| Input: available signal types | software programmable | Asynchronous Digital-In |
| Input: impedance              |                       | 10 kto 3.3 V          |
| Input: maximum voltage level  |                       | -0.5 V to +4.0 V        |
| Input: signal levels          |                       | 3.3 V LVTTL             |
Output: available signal types software programmable Asynchronous Digital-Out, Synchronous Digital-Out, Trigger Output,
Run, Arm, Marker Output, System Clock
| Output: impedance     |     | 50         |
| --------------------- | --- | ----------- |
| Output: signal levels |     | 3.3 V LVTTL |
Output: type 3.3 V LVTTL, TTL compatible for high impedance loads
Output: drive strength Capable of driving 50 loads, maximum drive strength ±48 mA
| Output: update rate |     | sampling clock |
| ------------------- | --- | -------------- |

(c) Spectrum Instrumentation GmbH 22

| Introduction |     |     | Hardware information (M4i.66xx and M4x.66xx) |     |     |
| ------------ | --- | --- | -------------------------------------------- | --- | --- |
Option M4i.xxxx-DDS (20 tone DDS firmware)
| Number of available DDS cores per AWG card |     |     | 23  |     |     |
| ------------------------------------------ | --- | --- | --- | --- | --- |
DDS core routing options software programmable Routed cores can individually be activated for output
Ch0: 8, 12, 16 or 20 cores;
Ch1: 1 or 5 cores
Ch2: 1 or 5 cores
Ch3: 1 or 5 cores
DDS commands individual for each core Set Frequency,, Set Amplitude, Set Phase, Frequency Slope, Amplitude Slope
DDS commands for all cores Reset, Execute Now, Execute at Trigger/Timer
| DDS command transfer mode                |     |     | single or DMA      |     |     |
| ---------------------------------------- | --- | --- | ------------------ | --- | --- |
| DDS time resolution (nom)                |     |     | 1.25 GS/s (800 ps) |     |     |
| DDS single command time resolution (nom) |     |     | 6.4 ns             |     |     |
DDS timer resolution (nom) software programmable 83.2 ns up to 27.48 s with a resolution of 6.4 ns
DDS frequency range per core programmable 0 Hz up to 1.25 GHz with a resolution of 1.25GHz/(232) = 0.29 Hz.
Frequencies above 625 MHz (Nyquist-Shannon) are mirrored
DDS amplitude range per core programmable -1.0 up to +1.0 with a resolution of 2/(216) = 0.0000305
programmed in relation to output level: +1.0 = 100% output, -1.0 = 100% inverted output
DDS phase range per core programmable 0° to +360° with a resolution of 360°/(212) = 0.088° (other values are mapped to this range)
| DDS command buffer                                |     | single mode | 4Ki commands                                                                      |     |     |
| ------------------------------------------------- | --- | ----------- | --------------------------------------------------------------------------------- | --- | --- |
|                                                   |     | DMA mode    | 512Mi commands in on-board RAM. More commands can reside in DMA buffer in PC-RAM. |     |     |
| Min user software to analog output latency (meas) |     | single mode | 10 us                                                                             |     |     |
|                                                   |     | DMA mode    | 20 us                                                                             |     |     |
| Max continuous DDS command rate (meas)            |     | single mode | 400 kHz                                                                           |     |     |
|                                                   |     | DMA mode    | 10 MHz                                                                            |     |     |
| External trigger to DDS output change (meas)      |     |             | ca. 560 ns                                                                        |     |     |
| Maximum external re-trigger rate (meas)           |     |             | 72 ns (14 MHz)                                                                    |     |     |
Number of DDS options per generatorNETBOX Each generatorNETBOX DN2.66x and DN6.66x contains multiple AWGs with either two or
four channels. The user can individually decide how many of these internal AWGs should be
equipped with the DDS option. Each single internal AWG needs a separate license.

Option M4i.xxxx-PulseGen
| Number of internal pulse generators |     |     | 4   |     |     |
| ----------------------------------- | --- | --- | --- | --- | --- |
Number of pulse generator output lines 3 (Existing multi-purpose outputs X0 to X2)
Time resolution of pulse generator Pulse generator’s sampling rate is derived from instrument’s sampling rate and value can be read
out. Maximum possible pulse generator update rate is
22xx: 156.25 MS/s (6.4 ns)
23xx: 156.25 MS/s (6.4 ns)
44xx: 125.00 MS/s (8.0 ns)
66xx: 156.25 MS/s (6.4 ns)
96xx: 156.25 MS/s (6.4 ns)
Programmable output modes Single-shot, multiple repetitions on trigger, gated
Programmable trigger sources Software, Card Trigger, Other Pulse Generator, XIO lines.
| Programmable trigger gate       |     |     | None, ARM state, RUN state              |     |     |
| ------------------------------- | --- | --- | --------------------------------------- | --- | --- |
| Programmable length (frequency) |     |     | 2 to 4Gi samples in steps of 1 (32 bit) |     |     |
| Programmable width (duty cycle) |     |     | 1 to 4Gi samples in steps of 1 (32 bit) |     |     |
| Programmable delay              |     |     | 0 to 4Gi samples in steps of 1 (32 bit) |     |     |
Programmable loops 0 to 4Gi samples in steps of 1 (32 bit) with 0 = infinite loops
Output level of digital pulse generators Please see section of multi-purpose I/O lines.

Bandwidth and Slewrate
|     | Filter | Output  663 models |     | 662 and 962 models |     |
| --- | ------ | ------------------ | --- | ------------------ | --- |
Amplitude (M4i.663x-x8, M4x.663x-x4, DN2.663-xx, DN6.663- (M4i.662x-x8, M4x.662x-x4, DN2.662-xx, DN6.662-
|     |     | xx, DN2.82x-02) |     | xx, DN2.82x-04, M4i.96xx-x8, M4x.96xx-x4, DN2.96x- |     |
| --- | --- | --------------- | --- | -------------------------------------------------- | --- |
xx, DN6.96x-xx)
| Maximum Output Rate  |           |          | 1.25 GS/s |     | 625 MS/s  |
| -------------------- | --------- | -------- | --------- | --- | --------- |
| -3dB Bandwidth (typ) | no Filter | ±480 mV  | 400 MHz   |     | 200 MHz   |
| -3dB Bandwidth (typ) | no Filter | ±1000 mV | 320 MHz   |     | 200 MHz   |
| -3dB Bandwidth (typ) | no Filter | ±2000 mV | 320 MHz   |     | 200 MHz   |
| -3dB Bandwidth (typ) | Filter    | all      | 65 MHz    |     | 65 MHz    |
| Slewrate (typ)       | no Filter | ±480 mV  | 4.5 V/ns  |     | 2.25 V/ns |

(c) Spectrum Instrumentation GmbH 23

| Introduction |     |     |     | Hardware information (M4i.66xx and M4x.66xx) |
| ------------ | --- | --- | --- | -------------------------------------------- |
Dynamic Parameters
662 and 962 models
(M4i.662x-x8, M4x.662x-x4, DN2.662-xx, DN6.662-xx, DN2.82x-04, M4i.96xx-x8, M4x.96xx-x4, DN2.96x-
xx, DN6.96x-xx)
| Test - Samplerate |     | 625 MS/s | 625 MS/s | 625 MS/s |
| ----------------- | --- | -------- | -------- | -------- |
| Output Frequency  |     | 10 MHz   | 50 MHz   | 50 MHz   |
Output Level in 50  ±480 mV ±1000mV ±2500mV ±480 mV ±2500mV ±480 mV ±2500mV
| Used Filter |     | none | none | Filter enabled |
| ----------- | --- | ---- | ---- | -------------- |
NSD (typ) -150 dBm/Hz -149 dBm/Hz -149 dBm/Hz -150 dBm/Hz -149 dBm/Hz -150 dBm/Hz -149 dBm/Hz
SNR (typ) 70.7 dB 72.4 dB 63.1 dB 65.3 dB 64.4 dB 67.5 dB 69.4 dB
THD (typ) -73.3 dB -70.5 dB -49.7 dB -64.1 dB -39.1 dB -68.4 dB -50.4 dB
SINAD (typ) 69.0 dB 67.7 dB 49.5 dB 61.6 dB 39.1 dB 64.9 dB 50.3 dB
SFDR (typ), excl harm. 98 dB 98 dB 99 dB 86 dB 76 dB 88 dB 89 dB
| ENOB (SINAD) | 11.2 | 11.0 8.0 10.0  | 6.2 10.5  | 8.1  |
| ------------ | ---- | -------------- | --------- | ---- |
| ENOB (SNR)   | 11.5 | 11.7 10.2 10.5 | 10.4 10.9 | 11.2 |

663 models
(M4i.663x-x8, M4x.663x-x4, DN2.663-xx, DN6.663-xx, DN2.82x-02)
| Test - Samplerate |     | 1.25 GS/s | 1.25 GS/s | 1.25 GS/s |
| ----------------- | --- | --------- | --------- | --------- |
| Output Frequency  |     | 10 MHz    | 50 MHz    | 50 MHz    |
Output Level in 50  ±480 mV ±1000mV ±2000mV ±480 mV ±2000mV ±480 mV ±2000mV
| Used Filter |     | none | none | Filter enabled |
| ----------- | --- | ---- | ---- | -------------- |
NSD (typ) -150 dBm/Hz -149 dBm/Hz -149 dBm/Hz -150 dBm/Hz -149 dBm/Hz -150 dBm/Hz -149 dBm/Hz
SNR (typ) 70.5 dB 72.1 dB 71.4 dB 65.2 dB 65.0 dB 67.2 dB 68.2 dB
THD (typ) -74.5 dB -73.5 dB -59.1 dB -60.9 dB -43.9 dB -67.9 dB -63.1 dB
SINAD (typ) 69.3 dB 69.7 dB 59 dB 59.5 dB 43.9 dB 64.5 dB 61.9 dB
SFDR (typ), excl harm. 96 dB 97 dB 98 dB 85 dB 84 dB 87 dB 87 dB
| ENOB (SINAD)           | 11.2   | 11.2 9.5 9.6   | 6.9 10.4  | 10.0 |
| ---------------------- | ------ | -------------- | --------- | ---- |
| ENOB (SNR)             | 11.5   | 11.5 11.5 10.5 | 10.5 10.9 | 11.0 |
| Phase Noise and Jitter |        | Output         |           |      |
| Output Frequency       | 50 MHz | 100 MHz        |           |      |
Output Level in 50 
|                                  | ±250 mV     | ±250 mV     |     |     |
| -------------------------------- | ----------- | ----------- | --- | --- |
| Offset 10 Hz (meas)              | -78 dBc/Hz  | -71 dBc/Hz  |     |     |
| Offset 100 Hz (meas)             | -104 dBc/Hz | -98 dBc/Hz  |     |     |
| Offset 1 kHz (meas)              | -113 dBc/Hz | -107 dBc/Hz |     |     |
| Offset 10 kHz (meas)             | -114 dBc/Hz | -108 dBc/Hz |     |     |
| Offset 100 kHz (meas)            | -140 dBc/Hz | -134 dBc/Hz |     |     |
| Offset 1 MHz (meas)              | -154 dBc/Hz | -150 dBc/Hz |     |     |
| Offset 10 MHz (meas)             | -156 dBc/Hz | -156 dBc/Hz |     |     |
| Jitter (meas)- Integration range |  1821.8 fs  | 1811.4 fs   |     |     |
12 kHz - 20 MHz
THD and SFDR are measured at the given output level and 50 Ohm termination with a high resolution M3i.4860/M4i.4450-x8 data acquisition card and are calculated from the spec-
trum. Noise Spectral Density is measured with built-in calculation from an HP E4401B Spectrum Analyzer. All available D/A channels are activated for the tests. SNR and SFDR figures
may differ depending on the quality of the used PC. NSD = Noise Spectral Density, THD = Total Harmonic Distortion, SFDR = Spurious Free Dynamic Range. Phase Noise and Jitter is mea-
sured with a Holzworth HA7062C Phase Noise Analyzer.

SFDR and THD versus signal frequency
| (meas) |     |     | (meas) |     |
| ------ | --- | --- | ------ | --- |
• Measurements done with a spectrum analyzer bandwidth of 1.5 GHz
• Please note that the bandwidth of the high range output is limited to 320 MHz
• Please note that the output bandwidth limit also affects the THD as harmonics higher than the bandwidth are filtered

(c) Spectrum Instrumentation GmbH 24

Introduction Hardware information (M4i.66xx and M4x.66xx)
M4i Specific Technical Data
Connectors
Analog Inputs/Analog Outputs SMA female (one for each single-ended input) Cable-Type: Cab-3mA-xx-xx
| Trigger 0 Input |     |     | SMA female  | Cable-Type: Cab-3mA-xx-xx |
| --------------- | --- | --- | ----------- | ------------------------- |
| Clock Input     |     |     | SMA female  | Cable-Type: Cab-3mA-xx-xx |
| Trigger 1 Input |     |     | MMCX female | Cable-Type: Cab-1m-xx-xx  |
| Clock Output    |     |     | MMCX female | Cable-Type: Cab-1m-xx-xx  |
Multi Purpose I/O MMCX female (3 lines) Cable-Type: Cab-1m-xx-xx
Connection Cycles
All connectors have an expected lifetime as specified below. Please avoid to exceed the specified connection cycles or use connector savers.
| SMA connector        |     |     | 500 connection cycles |     |
| -------------------- | --- | --- | --------------------- | --- |
| MMCX connector       |     |     | 500 connection cycles |     |
| PCIe connector       |     |     | 50 connection cycles  |     |
| PCIe power connector |     |     | 30 connection cycles  |     |

Environmental and Physical Details
Dimension (Single Card) L x H x W: 241 mm (¾ PCIe length) x 107 mm x 20 mm (single slot width)
Dimension (Card with option SH8tm installed) 241 mm (¾ PCIe length) x 107 mm x 40 mm (double slot width, extends W by 1 slot right
of the main card’s bracket, on „component side“ of the PCIe card.)
Dimension (Card with option SH8ex installed) Extends L to 312 mm (full PCIe length) x 107 mm x 20 mm (single slot width)
Dimension (Card with option M4i.44xx-DigSMA  241 mm (¾ PCIe length) x 107 mm x 40 mm (double slot width, extends W by 1 slot left
installed) of the main card’s bracket, on „solder side“ of the PCIe card.)
| Weight (M4i.44xx series)               | maximum |     | 290 g  |     |
| -------------------------------------- | ------- | --- | ------ | --- |
| Weight (M4i.22xx, M4i.23xx, M4i.66xx,  | maximum |     | 420 g  |     |
M4i.77xx, M4i.96xx series)
| Weight (Option star-hub -sh8ex, -sh8tm) | including 8 sync cables |     | 130 g                    |     |
| --------------------------------------- | ----------------------- | --- | ------------------------ | --- |
| Weight (Option M4i.44xx-DigSMA)         |                         |     | 320 g                    |     |
| Warm up time (meas)                     |                         |     | 30 minutes               |     |
| Operating temperature (nom)             |                         |     | 0°C to 50°C              |     |
| Storage temperature (nom)               |                         |     | -10°C to 70°C            |     |
| Humidity (nom)                          |                         |     | 10% to 90%               |     |
| Dimension of packing                    | 1 or 2 cards            |     | 470 mm x 250 mm x 130 cm |     |
| Volume weight of packing                | 1 or 2 cards            |     | 4 kg                     |     |

PCI Express specific details
| PCIe slot type                     |     |     | x8 Generation 2 (Gen2) |     |
| ---------------------------------- | --- | --- | ---------------------- | --- |
| PCIe slot compatibility (physical) |     |     | x8/x16                 |     |
PCIe slot compatibility (electrical) x1, x2, x4, x8, x16 with PCIe Gen1, Gen2, Gen3, Gen4 or Gen5
| Sustained streaming mode (meas): | Card-to-System |     | > 3.4 GB/s  |     |
| -------------------------------- | -------------- | --- | ----------- | --- |
M4i.22xx, M4i.23xx, M4i.44xx, M4i.77xx (measured with a chipset supporting a TLP size of 256 bytes, using PCIe x8 Gen2)
| Sustained streaming mode (meas): | System-to-Card |     | > 2.8 GB/s  |     |
| -------------------------------- | -------------- | --- | ----------- | --- |
M4i.66xx, M4i.96xx (measured with a chipset supporting a TLP size of 256 bytes, using PCIe x8 Gen2)

Certification, Compliance, Warranty
| Conformity Declaration | EN 17050-1:2010 | General Requirements                                                      |     |     |
| ---------------------- | --------------- | ------------------------------------------------------------------------- | --- | --- |
| EU Directives          | 2014/30/EU      | EMC - Electromagnetic Compatibility                                       |     |     |
|                        | 2014/35/EU      | LVD - Electrical equipment designed for use within certain voltage limits |     |     |
2011/65/EU RoHS - Restriction of the use of certain hazardous substances in electrical and electronic equipment
|     | 2006/1907/EC | REACH - Registration, Evaluation, Authorisation and Restriction of Chemicals |     |     |
| --- | ------------ | ---------------------------------------------------------------------------- | --- | --- |
|     | 2012/19/EU   | WEEE - Waste from Electrical and Electronic Equipment                        |     |     |
Compliance Standards EN 61010-1: 2010 Safety regulations for electrical measuring, control, regulating and laboratory devices - Part 1: General requirement
|     | EN 61187:1994     | Electrical and electronic measuring equipment - Documentation    |     |     |
| --- | ----------------- | ---------------------------------------------------------------- | --- | --- |
|     | EN 61326-1:2021   | Electrical equipment for measurement, control and laboratory use |     |     |
|     | EN 61326-2-1:2021 | EMC requirements - Part 1: General requirements                  |     |     |
EMC requirements - Part 2-1: Particular requirements - Test configurations, operational conditions and performance cri-
teria for sensitive test and measurement equipment for EMC unprotected applications
EN IEC 63000:2018 Technical documentation for the assessment of electrical and electronic products with respect to the restriction of haz-
ardous substances
| Product warranty              | 5 years starting with the day of delivery |     |     |     |
| ----------------------------- | ----------------------------------------- | --- | --- | --- |
| Software and firmware updates | Life-time, free of charge                 |     |     |     |

(c) Spectrum Instrumentation GmbH 25

| Introduction |     |     |     | Hardware information (M4i.66xx and M4x.66xx) |     |
| ------------ | --- | --- | --- | -------------------------------------------- | --- |
Power Consumption
PCI EXPRESS
|     |     |     |     | 3.3V 12 V | Total |
| --- | --- | --- | --- | --------- | ----- |
M4i.6620/9620-x8 (meas) All channels activated, Sample rate: 625 MSps 0.2 A 2.5 A 31 W
M4i.6621/9621-x8 (meas) Output signal: 31.25 MHz sine wave, Output level: ±1 V into 50  load 0.2 A 2.7 A 33 W
| M4i.6622/9622-x8 (meas) |     |     |     | 0.2 A 3.0 A | 36 W |
| ----------------------- | --- | --- | --- | ----------- | ---- |
M4i.6620/9620-x8 (meas) All channels activated, Sample rate: 625 MSps 0.2 A 2.6 A 32 W
Output signal: 31.25 MHz sine wave, Output level: ±2.5 V into 50  load
| M4i.6621/9621-x8 (meas) |     |     |     | 0.2 A 2.9 A | 35 W |
| ----------------------- | --- | --- | --- | ----------- | ---- |
| M4i.6622/9622-x8 (meas) |     |     |     | 0.2 A 3.3 A | 40 W |
M4i.6630-x8 (meas) Typical values: All channels activated, Sample rate: 1.25 GSps 0.2 A 2.7 A 33 W
M4i.6631-x8 (meas) Output signal: 31.25 MHz sine wave, Output level: ±1 V into 50  load 0.2 A 3.0 A 36 W
M4i.6630-x8 (meas) Typical values: All channels activated, Sample rate: 1.25 GSps 0.2 A 2.9 A 35 W
Output signal: 31.25 MHz sine wave, Output level: ±2.0 V into 50  load
| M4i.6631-x8 (meas) |     |     |     | 0.2 A 3.3 A | 40 W |
| ------------------ | --- | --- | --- | ----------- | ---- |
MTBF
| MTBF (typ) |     |     | 400000 hours |     |     |
| ---------- | --- | --- | ------------ | --- | --- |

M4x Specific Technical Data
Connectors
Analog Inputs/Analog Outputs SMA female (one for each single-ended input) Cable-Type: Cab-3mA-xx-xx
| Trigger 0 Input |     |     | SMA female |     | Cable-Type: Cab-3mA-xx-xx |
| --------------- | --- | --- | ---------- | --- | ------------------------- |
| Clock Input     |     |     | SMA female |     | Cable-Type: Cab-3mA-xx-xx |
| Trigger 1 Input |     |     | SMA female |     | Cable-Type: Cab-3mA-xx-xx |
| Clock Output    |     |     | SMA female |     | Cable-Type: Cab-3mA-xx-xx |
Multi Purpose I/O MMCX female (3 lines) Cable-Type: Cab-1m-xx-xx
Connection Cycles
All connectors have an expected lifetime as specified below. Please avoid to exceed the specified connection cycles or use connector savers.
| SMA connector  |     |     | 500 connection cycles |     |     |
| -------------- | --- | --- | --------------------- | --- | --- |
| MMCX connector |     |     | 500 connection cycles |     |     |
| PXIe connector |     |     | 250 connection cycles |     |     |

Environmental and Physical Details
| Dimension (Single Card)            | (PCB only)   |     | 160 mm x 100 mm (Standard 3U) |     |     |
| ---------------------------------- | ------------ | --- | ----------------------------- | --- | --- |
| Width                              |              |     | 2 slots                       |     |     |
| Weight (M4x.44xx series)           | maximum      |     | 340 g                         |     |     |
| Weight (M4x.22xx, M4x.66xx series) | maximum      |     | 450 g                         |     |     |
| Warm up time (meas)                |              |     | 30 minutes                    |     |     |
| Operating temperature (nom)        |              |     | 0°C to 50°C                   |     |     |
| Storage temperature (nom)          |              |     | -10°C to 70°C                 |     |     |
| Humidity (nom)                     |              |     | 10% to 90%                    |     |     |
| Dimension of packing               | 1 or 2 cards |     | 470 mm x 250 mm x 130 cm      |     |     |
| Volume weight of packing           | 1 or 2 cards |     | 4 kg                          |     |     |

PXI Express specific details
| PXIe slot type                 |     |     | 4 Lanes, PCIe Gen2 (x4 Gen2) |     |     |
| ------------------------------ | --- | --- | ---------------------------- | --- | --- |
| PXIe hybrid slot compatibility |     |     | Fully compatible             |     |     |
Sustained streaming mode (meas) > 1.7 GB/s (measured with a chipset supporting a TLP size of 256 bytes, using PXIe x4 Gen2)
(Card-to-System: M4x.22xx, M4x.44xx)
Sustained streaming mode (meas) > 1.4 GB/s (measured with a chipset supporting a TLP size of 256 bytes, using PXIe x4 Gen2)
(System-to-Card: M4x.66xx, M4x.96xx)

Certification, Compliance, Warranty
| Conformity Declaration | EN 17050-1:2010 | General Requirements                                                      |     |     |     |
| ---------------------- | --------------- | ------------------------------------------------------------------------- | --- | --- | --- |
| EU Directives          | 2014/30/EU      | EMC - Electromagnetic Compatibility                                       |     |     |     |
|                        | 2014/35/EU      | LVD - Electrical equipment designed for use within certain voltage limits |     |     |     |
2011/65/EU RoHS - Restriction of the use of certain hazardous substances in electrical and electronic equipment
|     | 2006/1907/EC | REACH - Registration, Evaluation, Authorisation and Restriction of Chemicals |     |     |     |
| --- | ------------ | ---------------------------------------------------------------------------- | --- | --- | --- |
|     | 2012/19/EU   | WEEE - Waste from Electrical and Electronic Equipment                        |     |     |     |
Compliance Standards EN 61010-1: 2010 Safety regulations for electrical measuring, control, regulating and laboratory devices - Part 1: General requirement
|     | EN 61187:1994     | Electrical and electronic measuring equipment - Documentation    |     |     |     |
| --- | ----------------- | ---------------------------------------------------------------- | --- | --- | --- |
|     | EN 61326-1:2021   | Electrical equipment for measurement, control and laboratory use |     |     |     |
|     | EN 61326-2-1:2021 | EMC requirements - Part 1: General requirements                  |     |     |     |
EMC requirements - Part 2-1: Particular requirements - Test configurations, operational conditions and performance cri-
teria for sensitive test and measurement equipment for EMC unprotected applications
EN IEC 63000:2018 Technical documentation for the assessment of electrical and electronic products with respect to the restriction of haz-
ardous substances
| Product warranty              | 5 years starting with the day of delivery |     |     |     |     |
| ----------------------------- | ----------------------------------------- | --- | --- | --- | --- |
| Software and firmware updates | Life-time, free of charge                 |     |     |     |     |

(c) Spectrum Instrumentation GmbH 26

| Introduction | Hardware information (M4i.66xx and M4x.66xx) |     |
| ------------ | -------------------------------------------- | --- |
Power Consumption
PCI EXPRESS
|     | 3.3V 12 V | Total |
| --- | --------- | ----- |
M4x.6620-x4 (meas) Typical values: All channels activated, Sample rate: 625 MSps 0.25 A 2.5 A 31 W
M4x.6621/9621-x4 (meas) Output signal: 31.25 MHz sine wave, Output level: +/- 1 V into 50  load 0.25 A 2.7 A 33 W
| M4x.6622/9622-x4 (meas) | 0.25 A 3.0 A | 36 W |
| ----------------------- | ------------ | ---- |
M4x.6620/9620-x4 (meas) Typical values: All channels activated, Sample rate: 625 MSps 0.25 A 2.6 A 32 W
Output signal: 31.25 MHz sine wave, Output level: +/- 2.5 V into 50  load
| M4x.6621/9621-x4 (meas) | 0.25 A 2.9 A | 35 W |
| ----------------------- | ------------ | ---- |
| M4x.6622/9622-x4 (meas) | 0.25 A 3.3 A | 40 W |
M4x.6630-x4 (meas) Typical values: All channels activated, Sample rate: 1.25 GSps 0.25 A 2.7 A 33 W
M4x.6631-x4 (meas) Output signal: 31.25 MHz sine wave, Output level: +/- 1 V into 50  load 0.25 A 3.0 A 36 W
M4x.6630-x4 (meas) Typical values: All channels activated, Sample rate: 1.25 GSps 0.25 A 2.9 A 35 W
Output signal: 31.25 MHz sine wave, Output level: +/- 2.0 V into 50  load
| M4x.6631-x4 (meas) | 0.25 A 3.3 A | 40 W |
| ------------------ | ------------ | ---- |
MTBF
MTBF (typ) 400000 hours

(c) Spectrum Instrumentation GmbH 27

Introduction Hardware information (M4i.96xx and M4x.96xx)
Hardware information (M4i.96xx and M4x.96xx)
Block Diagrams
M4i.96xx Block Diagram
Image 5: M4i.96xx PCI Express series hardware block diagram
M4x.96xx Block Diagram
Image 6: M4x.96xx PXI Express series hardware block diagram
(c) Spectrum Instrumentation GmbH 28

| Introduction |     |     | Hardware information (M4i.96xx and M4x.96xx) |     |
| ------------ | --- | --- | -------------------------------------------- | --- |
Technical Data
Definitions
Specifications (spec)
Figures are valid for products stored for at least 2 hours inside the specified operating temperature range, after a 30 minute warm-up, after running an on-board calibration and with
properly cooled products. Data published in this document are specifications (spec) only where specifically indicated. Figures marked with (spec) are calibrated to the specification during
production and during factory calibrations.
Typical (typ)
Figures marked with (typ) are a characteristic performance that most of the manufactured instruments will meet due to design. Typical figures are measured during production but cannot
be calibrated.
Measured (meas)
A figure measured during development to communicate expected performance. The figure is measured in lab environment with an environmental temperature between 20°C and 25°C
and an altitude of less than 100 m.
Nominal (nom)
The value of this figure is determined by design and is not measured nor calibrated.
Analog Outputs
| Resolution (nom)  |     | 16 bit            |                   |            |
| ----------------- | --- | ----------------- | ----------------- | ---------- |
| D/A Interpolation |     | no interpolation  |                   |            |
|                   |     | M4i.662x/M4x.662x | M4i.663x/M4x.663x |            |
|                   |     | DN2.662/DN6.662x  | DN2.663/DN6.663   |            |
|                   |     | DN2.82x-04        |                   | DN2.82x-02 |
M4i.96xx/M4x.96xx
|     |     |                 | Standard Bandwidth | With high bandwidth option  |
| --- | --- | --------------- | ------------------ | --------------------------- |
|     |     | DN2.96x/DN6.96x |                    | (-hbw) installed            |
Output amplitude into 50  termination (nom) software programmable ±80 mV up to ±2.5 V  ±80 mV up to ±2 V ±80 mV up to ±480 mV
Output amplitude into high impedance loads (nom) software programmable ±160 mV up to ±5 V  ±160 mV up to ±4 V ±160 mV up to ±960 mV
Output amplitude stepsize (50 termination) (nom)
|                                               |     | 1 mV | 1 mV | 1 mV |
| --------------------------------------------- | --- | ---- | ---- | ---- |
| Stepsize of output amplitude (high impedance) |     | 2 mV | 2 mV | 2 mV |
10% to 90% rise/fall time of 480mV pulse (meas) 1.5 ns 1.1 ns 440 ps
10% to 90% rise/fall time of 2000 mV pulse (meas) 1.5 ns 1.1 ns n.a.
| Output offset (nom) | fixed | 0 V |     |     |
| ------------------- | ----- | --- | --- | --- |
Low Power path: ±80 mV to ±480 mV (into 50 
| Output Amplifier Path Selection | automatically by driver |     |     |     |
| ------------------------------- | ----------------------- | --- | --- | --- |
High Power path: ±420 mV to ±2.5 V/±2 V (into 50 
Output Amplifier Setting Hysteresis automatically by driver 420 mV to 480 mV (if output is using low power path it will switch to high power path at
480mV. If output is using high power path it will switch to low power path at 420mV)
Output amplifier path switching time (meas) 10 ms (output disabled while switching)
| Filters                              | software programmable | bypass with no filter or one fixed filter |     |     |
| ------------------------------------ | --------------------- | ----------------------------------------- | --- | --- |
| DAC Differential non linearity (nom) | DNL, DAC only         | ±0.5 LSB typical                          |     |     |
| DAC Integral non linearity (nom)     | INL, DAC only         | ±1.0 LSB typical                          |     |     |
50 
Output resistance (nom)
| Output coupling           |     | DC                       |     |     |
| ------------------------- | --- | ------------------------ | --- | --- |
| Minimum output load (nom) |     | 0  (short circuit safe) |     |     |
Output accuracy (spec) Low power path ±0.5 mV ±0.1% of programmed output amplitude
|                                 | High power path               | ±1.0 mV ±0.2% of programmed output amplitude |     |     |
| ------------------------------- | ----------------------------- | -------------------------------------------- | --- | --- |
| Offset temperature drift (meas) | after warm-up and calibration | TBD                                          |     |     |
| Gain temperature drift (meas)   | after warm-up and calibration | TBD                                          |     |     |
| Calibration                     | External                      |                                              |     |     |
External calibration calibrates the on-board references. All calibration constants are stored in
non-volatile memory. Optional ISO/IEC17025-compliant certificate. Optional ISO/IEC17025-
compliant certificate. A yearly external calibration is recommended.

Trigger
Available trigger modes software programmable External, Software, Window, Re-Arm, Or/And, Delay, PXI (M4x only)
Trigger edge software programmable Rising edge, falling edge or both edges
Trigger delay software programmable 0 to (8 GiSamples - 32) = 8589934560 Samples in steps of 32 samples
| Trigger accuracy (all sources)       |     | 1 sample    |      |     |
| ------------------------------------ | --- | ----------- | ---- | --- |
| Minimum external trigger pulse width |     | 2 samples |      |     |
| External trigger                     |     | Ext0        | Ext1 |     |
External trigger impedance (typ) software programmable 50  /1 k 1 k
External trigger coupling software programmable AC or DC fixed DC
External trigger type Window comparator Single level comparator
±10 V (1 k±2.5 V (50 
| External input level (nom) |     |     | ±10 V |     |
| -------------------------- | --- | --- | ----- | --- |
External trigger sensitivity/signal swing) (nom) 2.5% of full scale range 2.5% of full scale range = 0.5 V
External trigger level (nom) software programmable ±10 V in steps of 10 mV ±10 V in steps of 10 mV
| External trigger maximum voltage (nom) |      | ±30V          | ±30 V         |     |
| -------------------------------------- | ---- | ------------- | ------------- | --- |
| External trigger bandwidth DC (typ)    | 50  | DC to 200 MHz | n.a.          |     |
|                                        | 1 k | DC to 150 MHz | DC to 200 MHz |     |
50 
| External trigger bandwidth AC (typ)  |     | 20 kHz to 200 MHz | n.a.        |     |
| ------------------------------------ | --- | ----------------- | ----------- | --- |
| Minimum external trigger pulse width |     | 2 samples       | 2 samples |     |

(c) Spectrum Instrumentation GmbH 29

Introduction Hardware information (M4i.96xx and M4x.96xx)
Clock
Clock Modes software programmable internal PLL, external reference clock, Star-Hub sync (generatorNETBOX and M4i only), PXI Ref-
erence Clock (M4x only)
| Internal clock accuracy (spec)                 | after warm-up         | ±5 ppm                |
| ---------------------------------------------- | --------------------- | ----------------------- |
| External reference clock range (nom)           | software programmable |  10 MHz and  1.25 GHz |
| External reference clock input impedance (nom) |                       | 50  fixed              |
| External reference clock input coupling        |                       | AC coupling             |
| External reference clock input edge            |                       | Rising edge             |
External reference clock input type Single-ended, sine wave or square wave
External reference clock input swing (meas) square wave 0.3 V peak-peak up to 3.0 V peak-peak
External reference clock input swing (meas) sine wave 1.0 V peak-peak up to 3.0 V peak-peak
External reference clock input max DC voltage (nom) ±30 V (with max 3.0 V difference between low and high level)
| External reference clock input duty cycle (nom) |                     | 45% to 55%                               |
| ----------------------------------------------- | ------------------- | ---------------------------------------- |
| External reference clock output type (nom)      |                     | Single-ended, 3.3V LVPECL                |
| Star-Hub synchronization clock modes            | software selectable | Internal clock, external reference clock |
| Channel to channel skew on one card (typ)       |                     | <250ps                                   |
| Skew between star-hub synchronized cards (typ)  |                     | <130ps                                   |
| Clock output                                    |                     | Clock output = 625 MS/s /4 = 156.25 MHz  |

DDS mode (50-tone DDS firmware)
| Number of available DDS cores per DDS card |     | 50  |
| ------------------------------------------ | --- | --- |
DDS core routing options software programmable Routed cores can individually be activated for output
Ch0: 50 or 47 cores
Ch1: 0 or 1 core
Ch2: 0 or 1 core
Ch3: 0 or 1 core
DDS commands individual for each core Set Frequency,, Set Amplitude, Set Phase, Frequency Slope, Amplitude Slope
DDS commands for all cores Reset, Execute Now, Execute at Trigger/Timer
| DDS command transfer mode                |     | single or DMA     |
| ---------------------------------------- | --- | ----------------- |
| DDS time resolution (nom)                |     | 625 MS/s (1.6 ns) |
| DDS single command time resolution (nom) |     | 6.4 ns            |
DDS timer resolution (nom) software programmable 83.2 ns up to 27.48 s with a resolution of 6.4 ns
DDS frequency range per core programmable 0 Hz up to 625 MHz with a resolution of 0.29 Hz.
Frequencies above 312.5 MHz (Nyquist-Shannon) are mirrored
DDS amplitude range per core programmable -1.0 up to +1.0 with a resolution of 2/(212) = 0.0000305
programmed in relation to output level: +1.0 = 100% output, -1.0 = 100% inverted output
DDS phase range per core programmable 0° to +360° with a resolution of 360°/(212) = 0.088° (other values are mapped to this range)
| DDS command buffer                                | single mode | 4Ki commands                                                                      |
| ------------------------------------------------- | ----------- | --------------------------------------------------------------------------------- |
|                                                   | DMA mode    | 512Mi commands in on-board RAM. More commands can reside in DMA buffer in PC-RAM. |
| Min user software to analog output latency (meas) | single mode | 10 us                                                                             |
|                                                   | DMA mode    | 20 us                                                                             |
| Max continuous DDS command rate (meas)            | single mode | 400 kHz                                                                           |
|                                                   | DMA mode    | 10 MHz                                                                            |
| External trigger to DDS output change (meas)      |             | ca. 572 ns                                                                        |
| Maximum external re-trigger rate (meas)           |             | 72 ns (14 MHz)                                                                    |

Multi Purpose I/O lines (front-plate)
| Number of multi purpose lines |                       | three, named X0, X1, X2 |
| ----------------------------- | --------------------- | ----------------------- |
| Input: available signal types | software programmable | Asynchronous Digital-In |
| Input: impedance              |                       | 10 kto 3.3 V          |
| Input: maximum voltage level  |                       | -0.5 V to +4.0 V        |
| Input: signal levels          |                       | 3.3 V LVTTL             |
Output: available signal types software programmable Asynchronous Digital-Out, Synchronous Digital-Out, Trigger Output,
Run, Arm, Marker Output, System Clock
| Output: impedance     |     | 50         |
| --------------------- | --- | ----------- |
| Output: signal levels |     | 3.3 V LVTTL |
Output: type 3.3 V LVTTL, TTL compatible for high impedance loads
Capable of driving 50 loads, maximum drive strength ±48 mA
Output: drive strength
| Output: update rate |     | sampling clock |
| ------------------- | --- | -------------- |

(c) Spectrum Instrumentation GmbH 30

| Introduction |     |     |     | Hardware information (M4i.96xx and M4x.96xx) |     |
| ------------ | --- | --- | --- | -------------------------------------------- | --- |
Option M4i.xxxx-PulseGen
| Number of internal pulse generators |     |     | 4   |     |     |
| ----------------------------------- | --- | --- | --- | --- | --- |
Number of pulse generator output lines 3 (Existing multi-purpose outputs X0 to X2)
Time resolution of pulse generator Pulse generator’s sampling rate is derived from instrument’s sampling rate and value can be read
out. Maximum possible pulse generator update rate is
22xx: 156.25 MS/s (6.4 ns)
23xx: 156.25 MS/s (6.4 ns)
44xx: 125.00 MS/s (8.0 ns)
66xx: 156.25 MS/s (6.4 ns)
96xx: 156.25 MS/s (6.4 ns)
Programmable output modes Single-shot, multiple repetitions on trigger, gated
Programmable trigger sources Software, Card Trigger, Other Pulse Generator, XIO lines.
| Programmable trigger gate       |     |     | None, ARM state, RUN state              |     |     |
| ------------------------------- | --- | --- | --------------------------------------- | --- | --- |
| Programmable length (frequency) |     |     | 2 to 4Gi samples in steps of 1 (32 bit) |     |     |
| Programmable width (duty cycle) |     |     | 1 to 4Gi samples in steps of 1 (32 bit) |     |     |
| Programmable delay              |     |     | 0 to 4Gi samples in steps of 1 (32 bit) |     |     |
Programmable loops 0 to 4Gi samples in steps of 1 (32 bit) with 0 = infinite loops
Output level of digital pulse generators Please see section of multi-purpose I/O lines.

Bandwidth and Slewrate
|     | Filter | Output  663 models |     | 662 and 962 models |     |
| --- | ------ | ------------------ | --- | ------------------ | --- |
Amplitude
|     |     | (M4i.663x-x8, M4x.663x-x4, DN2.663-xx, DN6.663- |     | (M4i.662x-x8, M4x.662x-x4, DN2.662-xx, DN6.662-    |     |
| --- | --- | ----------------------------------------------- | --- | -------------------------------------------------- | --- |
|     |     | xx, DN2.82x-02)                                 |     | xx, DN2.82x-04, M4i.96xx-x8, M4x.96xx-x4, DN2.96x- |     |
xx, DN6.96x-xx)
| Maximum Output Rate  |           |          | 1.25 GS/s |     | 625 MS/s  |
| -------------------- | --------- | -------- | --------- | --- | --------- |
| -3dB Bandwidth (typ) | no Filter | ±480 mV  | 400 MHz   |     | 200 MHz   |
| -3dB Bandwidth (typ) | no Filter | ±1000 mV | 320 MHz   |     | 200 MHz   |
| -3dB Bandwidth (typ) | no Filter | ±2000 mV | 320 MHz   |     | 200 MHz   |
| -3dB Bandwidth (typ) | Filter    | all      | 65 MHz    |     | 65 MHz    |
| Slewrate (typ)       | no Filter | ±480 mV  | 4.5 V/ns  |     | 2.25 V/ns |

Dynamic Parameters
662 and 962 models
(M4i.662x-x8, M4x.662x-x4, DN2.662-xx, DN6.662-xx, DN2.82x-04, M4i.96xx-x8, M4x.96xx-x4, DN2.96x-
xx, DN6.96x-xx)
| Test - Samplerate |     | 625 MS/s | 625 MS/s | 625 MS/s |     |
| ----------------- | --- | -------- | -------- | -------- | --- |
| Output Frequency  |     | 10 MHz   | 50 MHz   | 50 MHz   |     |
Output Level in 50  ±480 mV ±1000mV ±2500mV ±480 mV ±2500mV ±480 mV ±2500mV
| Used Filter |     | none | none | Filter enabled |     |
| ----------- | --- | ---- | ---- | -------------- | --- |
NSD (typ) -150 dBm/Hz -149 dBm/Hz -149 dBm/Hz -150 dBm/Hz -149 dBm/Hz -150 dBm/Hz -149 dBm/Hz
SNR (typ) 70.7 dB 72.4 dB 63.1 dB 65.3 dB 64.4 dB 67.5 dB 69.4 dB
THD (typ) -73.3 dB -70.5 dB -49.7 dB -64.1 dB -39.1 dB -68.4 dB -50.4 dB
SINAD (typ) 69.0 dB 67.7 dB 49.5 dB 61.6 dB 39.1 dB 64.9 dB 50.3 dB
SFDR (typ), excl harm. 98 dB 98 dB 99 dB 86 dB 76 dB 88 dB 89 dB
| ENOB (SINAD) | 11.2 | 11.0 8.0  | 10.0 6.2 10.5  | 8.1  |     |
| ------------ | ---- | --------- | -------------- | ---- | --- |
| ENOB (SNR)   | 11.5 | 11.7 10.2 | 10.5 10.4 10.9 | 11.2 |     |

663 models
(M4i.663x-x8, M4x.663x-x4, DN2.663-xx, DN6.663-xx, DN2.82x-02)
| Test - Samplerate |     | 1.25 GS/s | 1.25 GS/s | 1.25 GS/s |     |
| ----------------- | --- | --------- | --------- | --------- | --- |
| Output Frequency  |     | 10 MHz    | 50 MHz    | 50 MHz    |     |
Output Level in 50  ±480 mV ±1000mV ±2000mV ±480 mV ±2000mV ±480 mV ±2000mV
| Used Filter |     | none | none | Filter enabled |     |
| ----------- | --- | ---- | ---- | -------------- | --- |
NSD (typ) -150 dBm/Hz -149 dBm/Hz -149 dBm/Hz -150 dBm/Hz -149 dBm/Hz -150 dBm/Hz -149 dBm/Hz
SNR (typ) 70.5 dB 72.1 dB 71.4 dB 65.2 dB 65.0 dB 67.2 dB 68.2 dB
THD (typ) -74.5 dB -73.5 dB -59.1 dB -60.9 dB -43.9 dB -67.9 dB -63.1 dB
SINAD (typ) 69.3 dB 69.7 dB 59 dB 59.5 dB 43.9 dB 64.5 dB 61.9 dB
SFDR (typ), excl harm. 96 dB 97 dB 98 dB 85 dB 84 dB 87 dB 87 dB
| ENOB (SINAD)           | 11.2   | 11.2 9.5  | 9.6 6.9 10.4   | 10.0 |     |
| ---------------------- | ------ | --------- | -------------- | ---- | --- |
| ENOB (SNR)             | 11.5   | 11.5 11.5 | 10.5 10.5 10.9 | 11.0 |     |
| Phase Noise and Jitter |        | Output    |                |      |     |
| Output Frequency       | 50 MHz | 100 MHz   |                |      |     |
Output Level in 50 
|                                  | ±250 mV     | ±250 mV     |     |     |     |
| -------------------------------- | ----------- | ----------- | --- | --- | --- |
| Offset 10 Hz (meas)              | -78 dBc/Hz  | -71 dBc/Hz  |     |     |     |
| Offset 100 Hz (meas)             | -104 dBc/Hz | -98 dBc/Hz  |     |     |     |
| Offset 1 kHz (meas)              | -113 dBc/Hz | -107 dBc/Hz |     |     |     |
| Offset 10 kHz (meas)             | -114 dBc/Hz | -108 dBc/Hz |     |     |     |
| Offset 100 kHz (meas)            | -140 dBc/Hz | -134 dBc/Hz |     |     |     |
| Offset 1 MHz (meas)              | -154 dBc/Hz | -150 dBc/Hz |     |     |     |
| Offset 10 MHz (meas)             | -156 dBc/Hz | -156 dBc/Hz |     |     |     |
| Jitter (meas)- Integration range |  1821.8 fs  | 1811.4 fs   |     |     |     |
12 kHz - 20 MHz
THD and SFDR are measured at the given output level and 50 Ohm termination with a high resolution M3i.4860/M4i.4450-x8 data acquisition card and are calculated from the spec-
trum. Noise Spectral Density is measured with built-in calculation from an HP E4401B Spectrum Analyzer. All available D/A channels are activated for the tests. SNR and SFDR figures
(c) Spectrum Instrumentation GmbH 31

Introduction Hardware information (M4i.96xx and M4x.96xx)
may differ depending on the quality of the used PC. NSD = Noise Spectral Density, THD = Total Harmonic Distortion, SFDR = Spurious Free Dynamic Range. Phase Noise and Jitter is mea-
sured with a Holzworth HA7062C Phase Noise Analyzer.

Option M4i.96xx-AWG
Number of AWG options per generatorNETBOX Each generatorNETBOX DN2.96x and DN6.96x contains multiple DDS units with either two or
four channels. The user can individually decide how many of these internal DDS units should be
equipped with the AWG option. Each single internal DDS unit needs a separate license.

AWG specific trigger specifications
| Multi, Gate: re-arming time (nom) |     | 40 samples |     |
| --------------------------------- | --- | ---------- | --- |
Trigger to Output Delay (meas) sample rate  625 MS/s 238.5 sample clocks + 16 ns (valid for all modes except SPCSEQ_ENDLOOPONTRIG)
|     | sample rate > 625 MS/s | 476.5 sample clocks + 16 ns (valid for all modes except SPCSEQ_ENDLOOPONTRIG) |     |
| --- | ---------------------- | ----------------------------------------------------------------------------- | --- |
Memory depth software programmable 32 up to [installed memory / number of active channels] samples in steps of 32
Multiple Replay segment size software programmable 16 up to [installed memory / 2 / active channels] samples in steps of 16
AWG specific clock specifications
Internal clock setup granularity (nom) 8 Hz (internal reference clock only, restrictions apply to external reference clock)
| Setable Clock speeds |     | 50 MHz to max sampling clock |     |
| -------------------- | --- | ---------------------------- | --- |
Clock Setting Gaps (nom) 750 to 757 MHz, 1125 to 1145 MHz (no sampling clock possible in these gaps)
| Clock output (nom) | sampling clock 71.68 MHz | Clock output = sampling clock/4 |     |
| ------------------ | ------------------------- | ------------------------------- | --- |
| Clock output (nom) | sampling clock >71.68 MHz | Clock output = sampling clock/8 |     |
Sequence Replay Mode
| Required firmware version |     | At least V1.14 |     |
| ------------------------- | --- | -------------- | --- |
Number of sequence steps software programmable 1 up to 4096 (sequence steps can be overloaded at runtime)
Number of memory segments software programmable 2 up to 64Ki (segment data can be overloaded at runtime)
Minimum segment size software programmable 384 samples (1 active channel), 192 samples (2 active channels),
96 samples (4 active channels), in steps of 32 samples.
Maximum segment size software programmable 2 GiS / active channels / number of sequence segments (round up to the next power of two)
| Loop Count | software programmable | 1 to (1Mi - 1) loops |     |
| ---------- | --------------------- | -------------------- | --- |
Sequence Step Commands software programmable Loop for #Loops, Next, Loop until Trigger, End Sequence, Sequence Restart on Trigger
Special Commands software programmable Data Overload at runtime, sequence steps overload at runtime,
readout current replayed sequence step
Limitations for synchronized products Software commands changing the sequence as well as „Loop until trigger“ are not synchronized
between cards. This also applies to multiple AWG modules in a generatorNETBOX.
Synchronized products can run static sequences as well as sequence restart on trigger.

M4i Specific Technical Data
Connectors
Analog Inputs/Analog Outputs SMA female (one for each single-ended input) Cable-Type: Cab-3mA-xx-xx
| Trigger 0 Input |     | SMA female  | Cable-Type: Cab-3mA-xx-xx |
| --------------- | --- | ----------- | ------------------------- |
| Clock Input     |     | SMA female  | Cable-Type: Cab-3mA-xx-xx |
| Trigger 1 Input |     | MMCX female | Cable-Type: Cab-1m-xx-xx  |
| Clock Output    |     | MMCX female | Cable-Type: Cab-1m-xx-xx  |
Multi Purpose I/O MMCX female (3 lines) Cable-Type: Cab-1m-xx-xx
Connection Cycles
All connectors have an expected lifetime as specified below. Please avoid to exceed the specified connection cycles or use connector savers.
| SMA connector        |     | 500 connection cycles |     |
| -------------------- | --- | --------------------- | --- |
| MMCX connector       |     | 500 connection cycles |     |
| PCIe connector       |     | 50 connection cycles  |     |
| PCIe power connector |     | 30 connection cycles  |     |

(c) Spectrum Instrumentation GmbH 32

| Introduction |     |     |     | Hardware information (M4i.96xx and M4x.96xx) |     |
| ------------ | --- | --- | --- | -------------------------------------------- | --- |
Environmental and Physical Details
Dimension (Single Card) L x H x W: 241 mm (¾ PCIe length) x 107 mm x 20 mm (single slot width)
Dimension (Card with option SH8tm installed) 241 mm (¾ PCIe length) x 107 mm x 40 mm (double slot width, extends W by 1 slot right
of the main card’s bracket, on „component side“ of the PCIe card.)
Dimension (Card with option SH8ex installed) Extends L to 312 mm (full PCIe length) x 107 mm x 20 mm (single slot width)
Dimension (Card with option M4i.44xx-DigSMA  241 mm (¾ PCIe length) x 107 mm x 40 mm (double slot width, extends W by 1 slot left
installed) of the main card’s bracket, on „solder side“ of the PCIe card.)
| Weight (M4i.44xx series)               | maximum |     | 290 g  |     |     |
| -------------------------------------- | ------- | --- | ------ | --- | --- |
| Weight (M4i.22xx, M4i.23xx, M4i.66xx,  | maximum |     | 420 g  |     |     |
M4i.77xx, M4i.96xx series)
| Weight (Option star-hub -sh8ex, -sh8tm) | including 8 sync cables |     | 130 g                    |     |     |
| --------------------------------------- | ----------------------- | --- | ------------------------ | --- | --- |
| Weight (Option M4i.44xx-DigSMA)         |                         |     | 320 g                    |     |     |
| Warm up time (meas)                     |                         |     | 30 minutes               |     |     |
| Operating temperature (nom)             |                         |     | 0°C to 50°C              |     |     |
| Storage temperature (nom)               |                         |     | -10°C to 70°C            |     |     |
| Humidity (nom)                          |                         |     | 10% to 90%               |     |     |
| Dimension of packing                    | 1 or 2 cards            |     | 470 mm x 250 mm x 130 cm |     |     |
| Volume weight of packing                | 1 or 2 cards            |     | 4 kg                     |     |     |

PCI Express specific details
| PCIe slot type                     |     |     | x8 Generation 2 (Gen2) |     |     |
| ---------------------------------- | --- | --- | ---------------------- | --- | --- |
| PCIe slot compatibility (physical) |     |     | x8/x16                 |     |     |
PCIe slot compatibility (electrical) x1, x2, x4, x8, x16 with PCIe Gen1, Gen2, Gen3, Gen4 or Gen5
| Sustained streaming mode (meas): | Card-to-System |     | > 3.4 GB/s  |     |     |
| -------------------------------- | -------------- | --- | ----------- | --- | --- |
M4i.22xx, M4i.23xx, M4i.44xx, M4i.77xx (measured with a chipset supporting a TLP size of 256 bytes, using PCIe x8 Gen2)
| Sustained streaming mode (meas): | System-to-Card |     | > 2.8 GB/s  |     |     |
| -------------------------------- | -------------- | --- | ----------- | --- | --- |
M4i.66xx, M4i.96xx (measured with a chipset supporting a TLP size of 256 bytes, using PCIe x8 Gen2)

Certification, Compliance, Warranty
| Conformity Declaration | EN 17050-1:2010 | General Requirements                                                      |     |     |     |
| ---------------------- | --------------- | ------------------------------------------------------------------------- | --- | --- | --- |
| EU Directives          | 2014/30/EU      | EMC - Electromagnetic Compatibility                                       |     |     |     |
|                        | 2014/35/EU      | LVD - Electrical equipment designed for use within certain voltage limits |     |     |     |
2011/65/EU RoHS - Restriction of the use of certain hazardous substances in electrical and electronic equipment
|     | 2006/1907/EC | REACH - Registration, Evaluation, Authorisation and Restriction of Chemicals |     |     |     |
| --- | ------------ | ---------------------------------------------------------------------------- | --- | --- | --- |
|     | 2012/19/EU   | WEEE - Waste from Electrical and Electronic Equipment                        |     |     |     |
Compliance Standards EN 61010-1: 2010 Safety regulations for electrical measuring, control, regulating and laboratory devices - Part 1: General requirement
|     | EN 61187:1994     | Electrical and electronic measuring equipment - Documentation    |     |     |     |
| --- | ----------------- | ---------------------------------------------------------------- | --- | --- | --- |
|     | EN 61326-1:2021   | Electrical equipment for measurement, control and laboratory use |     |     |     |
|     | EN 61326-2-1:2021 | EMC requirements - Part 1: General requirements                  |     |     |     |
EMC requirements - Part 2-1: Particular requirements - Test configurations, operational conditions and performance cri-
teria for sensitive test and measurement equipment for EMC unprotected applications
EN IEC 63000:2018 Technical documentation for the assessment of electrical and electronic products with respect to the restriction of haz-
ardous substances
| Product warranty              | 5 years starting with the day of delivery |     |     |     |     |
| ----------------------------- | ----------------------------------------- | --- | --- | --- | --- |
| Software and firmware updates | Life-time, free of charge                 |     |     |     |     |

Power Consumption
PCI EXPRESS
|     |     |     |     | 3.3V 12 V | Total |
| --- | --- | --- | --- | --------- | ----- |
M4i.6620/9620-x8 (meas) All channels activated, Sample rate: 625 MSps 0.2 A 2.5 A 31 W
Output signal: 31.25 MHz sine wave, Output level: ±1 V into 50  load
| M4i.6621/9621-x8 (meas) |     |     |     | 0.2 A 2.7 A | 33 W |
| ----------------------- | --- | --- | --- | ----------- | ---- |
| M4i.6622/9622-x8 (meas) |     |     |     | 0.2 A 3.0 A | 36 W |
M4i.6620/9620-x8 (meas) All channels activated, Sample rate: 625 MSps 0.2 A 2.6 A 32 W
M4i.6621/9621-x8 (meas) Output signal: 31.25 MHz sine wave, Output level: ±2.5 V into 50  load 0.2 A 2.9 A 35 W
| M4i.6622/9622-x8 (meas) |     |     |     | 0.2 A 3.3 A | 40 W |
| ----------------------- | --- | --- | --- | ----------- | ---- |
M4i.6630-x8 (meas) Typical values: All channels activated, Sample rate: 1.25 GSps 0.2 A 2.7 A 33 W
Output signal: 31.25 MHz sine wave, Output level: ±1 V into 50  load
| M4i.6631-x8 (meas) |     |     |     | 0.2 A 3.0 A | 36 W |
| ------------------ | --- | --- | --- | ----------- | ---- |
M4i.6630-x8 (meas) Typical values: All channels activated, Sample rate: 1.25 GSps 0.2 A 2.9 A 35 W
M4i.6631-x8 (meas) Output signal: 31.25 MHz sine wave, Output level: ±2.0 V into 50  load 0.2 A 3.3 A 40 W
MTBF
| MTBF (typ) |     |     | 400000 hours |     |     |
| ---------- | --- | --- | ------------ | --- | --- |

(c) Spectrum Instrumentation GmbH 33

| Introduction |     |     |     | Hardware information (M4i.96xx and M4x.96xx) |     |
| ------------ | --- | --- | --- | -------------------------------------------- | --- |
M4x Specific Technical Data
Connectors
Analog Inputs/Analog Outputs SMA female (one for each single-ended input) Cable-Type: Cab-3mA-xx-xx
| Trigger 0 Input |     |     | SMA female |     | Cable-Type: Cab-3mA-xx-xx |
| --------------- | --- | --- | ---------- | --- | ------------------------- |
| Clock Input     |     |     | SMA female |     | Cable-Type: Cab-3mA-xx-xx |
| Trigger 1 Input |     |     | SMA female |     | Cable-Type: Cab-3mA-xx-xx |
| Clock Output    |     |     | SMA female |     | Cable-Type: Cab-3mA-xx-xx |
Multi Purpose I/O MMCX female (3 lines) Cable-Type: Cab-1m-xx-xx
Connection Cycles
All connectors have an expected lifetime as specified below. Please avoid to exceed the specified connection cycles or use connector savers.
| SMA connector  |     |     | 500 connection cycles |     |     |
| -------------- | --- | --- | --------------------- | --- | --- |
| MMCX connector |     |     | 500 connection cycles |     |     |
| PXIe connector |     |     | 250 connection cycles |     |     |

Environmental and Physical Details
| Dimension (Single Card)            | (PCB only)   |     | 160 mm x 100 mm (Standard 3U) |     |     |
| ---------------------------------- | ------------ | --- | ----------------------------- | --- | --- |
| Width                              |              |     | 2 slots                       |     |     |
| Weight (M4x.44xx series)           | maximum      |     | 340 g                         |     |     |
| Weight (M4x.22xx, M4x.66xx series) | maximum      |     | 450 g                         |     |     |
| Warm up time (meas)                |              |     | 30 minutes                    |     |     |
| Operating temperature (nom)        |              |     | 0°C to 50°C                   |     |     |
| Storage temperature (nom)          |              |     | -10°C to 70°C                 |     |     |
| Humidity (nom)                     |              |     | 10% to 90%                    |     |     |
| Dimension of packing               | 1 or 2 cards |     | 470 mm x 250 mm x 130 cm      |     |     |
| Volume weight of packing           | 1 or 2 cards |     | 4 kg                          |     |     |

PXI Express specific details
| PXIe slot type                 |     |     | 4 Lanes, PCIe Gen2 (x4 Gen2) |     |     |
| ------------------------------ | --- | --- | ---------------------------- | --- | --- |
| PXIe hybrid slot compatibility |     |     | Fully compatible             |     |     |
Sustained streaming mode (meas) > 1.7 GB/s (measured with a chipset supporting a TLP size of 256 bytes, using PXIe x4 Gen2)
(Card-to-System: M4x.22xx, M4x.44xx)
Sustained streaming mode (meas) > 1.4 GB/s (measured with a chipset supporting a TLP size of 256 bytes, using PXIe x4 Gen2)
(System-to-Card: M4x.66xx, M4x.96xx)

Certification, Compliance, Warranty
| Conformity Declaration | EN 17050-1:2010 | General Requirements                                                      |     |     |     |
| ---------------------- | --------------- | ------------------------------------------------------------------------- | --- | --- | --- |
| EU Directives          | 2014/30/EU      | EMC - Electromagnetic Compatibility                                       |     |     |     |
|                        | 2014/35/EU      | LVD - Electrical equipment designed for use within certain voltage limits |     |     |     |
2011/65/EU RoHS - Restriction of the use of certain hazardous substances in electrical and electronic equipment
|     | 2006/1907/EC | REACH - Registration, Evaluation, Authorisation and Restriction of Chemicals |     |     |     |
| --- | ------------ | ---------------------------------------------------------------------------- | --- | --- | --- |
|     | 2012/19/EU   | WEEE - Waste from Electrical and Electronic Equipment                        |     |     |     |
Compliance Standards EN 61010-1: 2010 Safety regulations for electrical measuring, control, regulating and laboratory devices - Part 1: General requirement
|     | EN 61187:1994     | Electrical and electronic measuring equipment - Documentation    |     |     |     |
| --- | ----------------- | ---------------------------------------------------------------- | --- | --- | --- |
|     | EN 61326-1:2021   | Electrical equipment for measurement, control and laboratory use |     |     |     |
|     | EN 61326-2-1:2021 | EMC requirements - Part 1: General requirements                  |     |     |     |
EMC requirements - Part 2-1: Particular requirements - Test configurations, operational conditions and performance cri-
teria for sensitive test and measurement equipment for EMC unprotected applications
EN IEC 63000:2018 Technical documentation for the assessment of electrical and electronic products with respect to the restriction of haz-
ardous substances
| Product warranty              | 5 years starting with the day of delivery |     |     |     |     |
| ----------------------------- | ----------------------------------------- | --- | --- | --- | --- |
| Software and firmware updates | Life-time, free of charge                 |     |     |     |     |

Power Consumption
PCI EXPRESS
|     |     |     |     | 3.3V 12 V | Total |
| --- | --- | --- | --- | --------- | ----- |
M4x.6620-x4 (meas) Typical values: All channels activated, Sample rate: 625 MSps 0.25 A 2.5 A 31 W
M4x.6621/9621-x4 (meas) Output signal: 31.25 MHz sine wave, Output level: +/- 1 V into 50  load 0.25 A 2.7 A 33 W
| M4x.6622/9622-x4 (meas) |     |     |     | 0.25 A 3.0 A | 36 W |
| ----------------------- | --- | --- | --- | ------------ | ---- |
M4x.6620/9620-x4 (meas) Typical values: All channels activated, Sample rate: 625 MSps 0.25 A 2.6 A 32 W
Output signal: 31.25 MHz sine wave, Output level: +/- 2.5 V into 50  load
| M4x.6621/9621-x4 (meas) |     |     |     | 0.25 A 2.9 A | 35 W |
| ----------------------- | --- | --- | --- | ------------ | ---- |
| M4x.6622/9622-x4 (meas) |     |     |     | 0.25 A 3.3 A | 40 W |
M4x.6630-x4 (meas) Typical values: All channels activated, Sample rate: 1.25 GSps 0.25 A 2.7 A 33 W
Output signal: 31.25 MHz sine wave, Output level: +/- 1 V into 50  load
| M4x.6631-x4 (meas) |     |     |     | 0.25 A 3.0 A | 36 W |
| ------------------ | --- | --- | --- | ------------ | ---- |
M4x.6630-x4 (meas) Typical values: All channels activated, Sample rate: 1.25 GSps 0.25 A 2.9 A 35 W
Output signal: 31.25 MHz sine wave, Output level: +/- 2.0 V into 50  load
| M4x.6631-x4 (meas) |     |     |     | 0.25 A 3.3 A | 40 W |
| ------------------ | --- | --- | --- | ------------ | ---- |
MTBF
| MTBF (typ) |     |     | 400000 hours |     |     |
| ---------- | --- | --- | ------------ | --- | --- |

(c) Spectrum Instrumentation GmbH 34

| Introduction |     |     |     | Order Information (M4i.66xx and M4x.66xx) |
| ------------ | --- | --- | --- | ----------------------------------------- |
Order Information (M4i.66xx and M4x.66xx)
M4i Order Information
The card is delivered with 2 GiSample on-board memory and supports standard replay, FIFO replay (streaming), Multiple Replay, Gated
Replay, Continuous Replay (Loop), Single-Restart as well as Sequence. Operating system drivers for Windows/Linux 32 bit and 64 bit, ex-
amples for C/C++, LabVIEW (Windows), MATLAB (Windows and Linux), IVI, .NET, Delphi, Java, Python, Julia and a Base license of the
measurement software SBench 6 are included.
Adapter cables are not included. Please order separately!
PCI Express x8 Order no. Bandwidth Standard mem 1 channel 2 channels 4 channels
|     | M4i.6620-x8 | 200 MHz 2 GiSample | 625 MS/s            |          |
| --- | ----------- | ------------------ | ------------------- | -------- |
|     | M4i.6621-x8 | 200 MHz 2 GiSample | 625 MS/s 625 MS/s   |          |
|     | M4i.6622-x8 | 200 MHz 2 GiSample | 625 MS/s 625 MS/s   | 625 MS/s |
|     | M4i.6630-x8 | 400 MHz 2 GiSample | 1.25 GS/s           |          |
|     | M4i.6631-x8 | 400 MHz 2 GiSample | 1.25 GS/s 1.25 GS/s |          |
Options
|     | Order no. | Option |     |     |
| --- | --------- | ------ | --- | --- |
M4i.xxxx-SH8ex (1) Synchronization Star-Hub for up to 8 cards (extension), only one slot width, extension of the card to full PCI Express length
(312 mm). 8 synchronization cables included.
M4i.xxxx-SH8tm (1) Synchronization Star-Hub for up to 8 cards (top mount), two slots width, top mounted on card. 8 synchronization cables
included.
|     | M4i-upgrade | Upgrade for M4i.xxxx: Later installation of option Star-Hub |     |     |
| --- | ----------- | ----------------------------------------------------------- | --- | --- |

| Options | Order no. | Option |     |     |
| ------- | --------- | ------ | --- | --- |
M4i.663x-hbw High bandwidth option 600 MHz. Available for 663x products with 1.25 GS/s only. Output level lim-
ited to ±480 mV into 50 . Needs external reconstruction filter. One option needed per AWG card.

| Firmware Options | Order no. | Option |     |     |
| ---------------- | --------- | ------ | --- | --- |
M4i.66xx-DDS Firmware Option multi-carrier DDS mode: adds 23 programmable DDS cores to the AWG. Each core
can be programmed with single commands for frequency, amplitude, phase, frequency slope, ampli-
tude slope.
M4i.xxxx-PulseGen Firmware Option: adds 4 freely programmable digital pulse generators that use the XIO lines for out-
put (later installation by firmware -upgrade available)

| Standard Cables |     | Order no. |     |     |
| --------------- | --- | --------- | --- | --- |
for Connections Length to BNC male to BNC female to SMA male to SMA female to SMB female
Analog/Clock-In/Trig-In 80 cm Cab-3mA-9m-80 Cab-3mA-9f-80 Cab-3mA-3mA-80 Cab-3f-3mA-80
Analog/Clock-In/Trig-In 200 cm Cab-3mA-9m-200 Cab-3mA-9f-200 Cab-3mA-3mA-200 Cab-3f-3mA-200
|     | Probes (short) | 5 cm | Cab-3mA-9f-5 |     |
| --- | -------------- | ---- | ------------ | --- |
Clk-Out/Trig-Out/Extra 80 cm Cab-1m-9m-80 Cab-1m-9f-80 Cab-1m-3mA-80 Cab-1m-3fA-80 Cab-1m-3f-80
Clk-Out/Trig-Out/Extra 200 cm Cab-1m-9m-200 Cab-1m-9f200 Cab-1m-3mA-200 Cab-1m-3fA-200 Cab-1m-3f-200
Information The standard adapter cables are based on RG174 cables and have a nominal attenuation of 0.3dB/m at 100MHz and
0.5dB/m at 250MHz. For high speed signals we recommend the low loss cables series CHF

| Low Loss Cables | Order No.       | Option                                      |     |     |
| --------------- | --------------- | ------------------------------------------- | --- | --- |
|                 | CHF-3mA-3mA-200 | Low loss cables SMA male to SMA male 200 cm |     |     |
|                 | CHF-3mA-9m-200  | Low loss cables SMA male to BNC male 200 cm |     |     |
Information The low loss adapter cables are based on MF141 cables and have an attenuation of 0.3dB/m at 500MHz and
0.5dB/m at 1.5GHz. They are recommended for signal frequencies of 200MHz and above.

Services
Order no.
|                  | Recal           | Recalibration at Spectrum incl. calibration protocol                                         |     |     |
| ---------------- | --------------- | -------------------------------------------------------------------------------------------- | --- | --- |
|                  | Card-Prot-17025 | ISO 17025 Calibration Certificate for one card; add-on for new cards or for recalibration    |     |     |
| Software SBench6 | Order no.       |                                                                                              |     |     |
|                  | SBench6         | Base version included in delivery. Supports standard mode for one card.                      |     |     |
|                  | SBench6-Pro     | Professional version for one card: FIFO mode, export/import, calculation functions, ...      |     |     |
|                  | SBench6-Multi   | Option multiple cards: Needs SBench6-Pro. Handles multiple synchronized cards in one system. |     |     |
|                  | Volume Licenses | Please ask Spectrum for details.                                                             |     |     |

| Software Options | Order no.   |                                                                                      |     |     |
| ---------------- | ----------- | ------------------------------------------------------------------------------------ | --- | --- |
|                  | SPc-RServer | Remote Server Software Package - LAN remote access for M2i/M3i/M4i/M4x/M2p/M5i cards |     |     |
SPc-SCAPP Spectrum’s CUDA Access for Parallel Processing - SDK for direct data transfer between Spectrum card and CUDA GPU.
Includes RDMA activation and examples. Only working for Data Acquisition or AWG mode, not usable for DDS mode
  (1) : Just one of the options can be installed on a card at a time.
(2) : Third party product with warranty differing from our export conditions. No volume rebate possible.

(c) Spectrum Instrumentation GmbH 35

| Introduction |     |     |     |     | Order Information (M4i.66xx and M4x.66xx) |     |
| ------------ | --- | --- | --- | --- | ----------------------------------------- | --- |
M4x Order Information
The card is delivered with 4 GiByte on-board memory (512 MiCommands for DDS or 2 GiSample for AWG data). The multi-core DDS gen-
eration is the standard mode. Adding the AWG option, the cards supports standard replay, FIFO replay (streaming), Multiple Replay, Gated
Replay, Continuous Replay (Loop), Single-Restart as well as Sequence. Operating system drivers for Windows/Linux 32 bit and 64 bit, ex-
amples for C/C++, LabVIEW (Windows), MATLAB (Windows and Linux), IVI, .NET, Delphi, Java, Python, Julia and a Base license of the
measurement software SBench 6 (AWG mode only) are included.
Adapter cables are not included. Please order separately!
PXI Express x4 Order no. Bandwidth Standard mem 1 channel 2 channels 4 channels
|     | M4x.6621-x8 | 200 MHz | 2 GiSample | 625 MS/s  | 625 MS/s  |          |
| --- | ----------- | ------- | ---------- | --------- | --------- | -------- |
|     | M4x.6622-x8 | 200 MHz | 2 GiSample | 625 MS/s  | 625 MS/s  | 625 MS/s |
|     | M4x.6630-x8 | 400 MHz | 2 GiSample | 1.25 GS/s |           |          |
|     | M4x.6631-x8 | 400 MHz | 2 GiSample | 1.25 GS/s | 1.25 GS/s |          |

| Firmware Options | Order no. | Option |     |     |     |     |
| ---------------- | --------- | ------ | --- | --- | --- | --- |
M4i.66xx-DDS Firmware Option multi-carrier DDS mode: adds 23 programmable DDS cores to the AWG. Each core
can be programmed with single commands for frequency, amplitude, phase, frequency slope, ampli-
tude slope.
M4i.xxxx-PulseGen Firmware Option: adds 4 freely programmable digital pulse generators that use the XIO lines for out-
put (later installation by firmware -upgrade available)

| Standard Cables |     | Order no. |     |     |     |     |
| --------------- | --- | --------- | --- | --- | --- | --- |
for Connections Length to BNC male to BNC female to SMA male to SMA female to SMB female
Analog/Clock-In/Trig-In 80 cm Cab-3mA-9m-80 Cab-3mA-9f-80 Cab-3mA-3mA-80 Cab-3f-3mA-80
Analog/Clock-In/Trig-In 200 cm Cab-3mA-9m-200 Cab-3mA-9f-200 Cab-3mA-3mA-200 Cab-3f-3mA-200
|     | Probes (short) | 5 cm | Cab-3mA-9f-5 |     |     |     |
| --- | -------------- | ---- | ------------ | --- | --- | --- |
Clk-Out/Trig-Out/Extra 80 cm Cab-1m-9m-80 Cab-1m-9f-80 Cab-1m-3mA-80 Cab-1m-3fA-80 Cab-1m-3f-80
Clk-Out/Trig-Out/Extra 200 cm Cab-1m-9m-200 Cab-1m-9f200 Cab-1m-3mA-200 Cab-1m-3fA-200 Cab-1m-3f-200
Information The standard adapter cables are based on RG174 cables and have a nominal attenuation of 0.3dB/m at 100MHz and
0.5dB/m at 250MHz. For high speed signals we recommend the low loss cables series CHF

| Low Loss Cables | Order No.       | Option                                      |     |     |     |     |
| --------------- | --------------- | ------------------------------------------- | --- | --- | --- | --- |
|                 | CHF-3mA-3mA-200 | Low loss cables SMA male to SMA male 200 cm |     |     |     |     |
|                 | CHF-3mA-9m-200  | Low loss cables SMA male to BNC male 200 cm |     |     |     |     |
Information The low loss adapter cables are based on MF141 cables and have an attenuation of 0.3dB/m at 500MHz and
0.5dB/m at 1.5GHz. They are recommended for signal frequencies of 200MHz and above.

| Services         | Order no.       |                                                                                              |     |     |     |     |
| ---------------- | --------------- | -------------------------------------------------------------------------------------------- | --- | --- | --- | --- |
|                  | Recal           | Recalibration at Spectrum incl. calibration protocol                                         |     |     |     |     |
|                  | Card-Prot-17025 | ISO 17025 Calibration Certificate for one card; add-on for new cards or for recalibration    |     |     |     |     |
| Software SBench6 | Order no.       |                                                                                              |     |     |     |     |
|                  | SBench6         | Base version included in delivery. Supports standard mode for one card.                      |     |     |     |     |
|                  | SBench6-Pro     | Professional version for one card: FIFO mode, export/import, calculation functions, ...      |     |     |     |     |
|                  | SBench6-Multi   | Option multiple cards: Needs SBench6-Pro. Handles multiple synchronized cards in one system. |     |     |     |     |
|                  | Volume Licenses | Please ask Spectrum for details.                                                             |     |     |     |     |

| Software Options | Order no.   |                                                                                      |     |     |     |     |
| ---------------- | ----------- | ------------------------------------------------------------------------------------ | --- | --- | --- | --- |
|                  | SPc-RServer | Remote Server Software Package - LAN remote access for M2i/M3i/M4i/M4x/M2p/M5i cards |     |     |     |     |
SPc-SCAPP Spectrum’s CUDA Access for Parallel Processing - SDK for direct data transfer between Spectrum card and CUDA GPU.
Includes RDMA activation and examples. Only working for Data Acquisition or AWG mode, not usable for DDS mode
  (1) : Just one of the options can be installed on a card at a time.
(2) : Third party product with warranty differing from our export conditions. No volume rebate possible.

(c) Spectrum Instrumentation GmbH 36

| Introduction |     |     |     |     | Order Information (M4i.96xx and M4x.96xx) |
| ------------ | --- | --- | --- | --- | ----------------------------------------- |
Order Information (M4i.96xx and M4x.96xx)
M4i Order Information
The card is delivered with 4 GiByte on-board memory (512 MiCommands for DDS or 2 GiSample for AWG data). The multi-core DDS gen-
eration is the standard mode. Adding the AWG option, the cards supports standard replay, FIFO replay (streaming), Multiple Replay, Gated
Replay, Continuous Replay (Loop), Single-Restart as well as Sequence. Operating system drivers for Windows/Linux 32 bit and 64 bit, ex-
amples for C/C++, LabVIEW (Windows), MATLAB (Windows and Linux), IVI, .NET, Delphi, Java, Python, Julia and a Base license of the
measurement software SBench 6 (AWG mode only) are included.
Adapter cables are not included. Please order separately!
PCI Express x8 Order no. Bandwidth DDS memory Channels AWG memory
|         | M4i.9620-x8 | 200 MHz | 512 MiCommands | 1   | 2 GiSample |
| ------- | ----------- | ------- | -------------- | --- | ---------- |
|         | M4i.9621-x8 | 200 MHz | 512 MiCommands | 2   | 2 GiSample |
|         | M4i.9622-x8 | 200 MHz | 512 MiCommands | 4   | 2 GiSample |
| Options | Order no.   | Option  |                |     |            |
M4i.xxxx-SH8ex (1) Synchronization Star-Hub for up to 8 cards (extension), only one slot width, extension of the card to full PCI Express length
(312 mm). 8 synchronization cables included.
M4i.xxxx-SH8tm (1) Synchronization Star-Hub for up to 8 cards (top mount), two slots width, top mounted on card. 8 synchronization cables
included.
|     | M4i-upgrade | Upgrade for M4i.xxxx: Later installation of option Star-Hub |     |     |     |
| --- | ----------- | ----------------------------------------------------------- | --- | --- | --- |

| Firmware Options | Order no. | Option |     |     |     |
| ---------------- | --------- | ------ | --- | --- | --- |
M4i.96xx-AWG Firmware Option AWG mode. Full AWG (arbitrary waveform generator) functionality including different replay modes,
freely programmable clock and trigger modes.
M4i.xxxx-PulseGen Firmware Option: adds 4 freely programmable digital pulse generators that use the XIO lines for output (later installation by
firmware -upgrade available)

| Standard Cables |     | Order no. |     |     |     |
| --------------- | --- | --------- | --- | --- | --- |
for Connections Length to BNC male to BNC female to SMA male to SMA female to SMB female
Analog/Clock-In/Trig-In 80 cm Cab-3mA-9m-80 Cab-3mA-9f-80 Cab-3mA-3mA-80 Cab-3f-3mA-80
Analog/Clock-In/Trig-In 200 cm Cab-3mA-9m-200 Cab-3mA-9f-200 Cab-3mA-3mA-200 Cab-3f-3mA-200
|     | Probes (short) | 5 cm | Cab-3mA-9f-5 |     |     |
| --- | -------------- | ---- | ------------ | --- | --- |
Clk-Out/Trig-Out/Extra 80 cm Cab-1m-9m-80 Cab-1m-9f-80 Cab-1m-3mA-80 Cab-1m-3fA-80 Cab-1m-3f-80
Clk-Out/Trig-Out/Extra 200 cm Cab-1m-9m-200 Cab-1m-9f200 Cab-1m-3mA-200 Cab-1m-3fA-200 Cab-1m-3f-200
Information The standard adapter cables are based on RG174 cables and have a nominal attenuation of 0.3dB/m at 100MHz and
0.5dB/m at 250MHz. For high speed signals we recommend the low loss cables series CHF
| Low Loss Cables | Order No.       | Option                                      |     |     |     |
| --------------- | --------------- | ------------------------------------------- | --- | --- | --- |
|                 | CHF-3mA-3mA-200 | Low loss cables SMA male to SMA male 200 cm |     |     |     |
|                 | CHF-3mA-9m-200  | Low loss cables SMA male to BNC male 200 cm |     |     |     |
Information The low loss adapter cables are based on MF141 cables and have an attenuation of 0.3dB/m at 500MHz and
0.5dB/m at 1.5GHz. They are recommended for signal frequencies of 200MHz and above.
| Services | Order no.       |                                                                                           |     |     |     |
| -------- | --------------- | ----------------------------------------------------------------------------------------- | --- | --- | --- |
|          | Recal           | Recalibration at Spectrum incl. calibration protocol                                      |     |     |     |
|          | Card-Prot-17025 | ISO 17025 Calibration Certificate for one card; add-on for new cards or for recalibration |     |     |     |

| Software SBench6 | Order no.       |                                                                                              |     |     |     |
| ---------------- | --------------- | -------------------------------------------------------------------------------------------- | --- | --- | --- |
|                  | SBench6         | Base version included in delivery. Supports standard mode for one card.                      |     |     |     |
|                  | SBench6-Pro     | Professional version for one card: FIFO mode, export/import, calculation functions, ...      |     |     |     |
|                  | SBench6-Multi   | Option multiple cards: Needs SBench6-Pro. Handles multiple synchronized cards in one system. |     |     |     |
|                  | Volume Licenses | Please ask Spectrum for details.                                                             |     |     |     |

Software Options
Order no.
|     | SPc-RServer | Remote Server Software Package - LAN remote access for M2i/M3i/M4i/M4x/M2p/M5i cards |     |     |     |
| --- | ----------- | ------------------------------------------------------------------------------------ | --- | --- | --- |
SPc-SCAPP Spectrum’s CUDA Access for Parallel Processing - SDK for direct data transfer between Spectrum card and CUDA GPU.
Includes RDMA activation and examples. Only working for Data Acquisition or AWG mode, not usable for DDS mode
  (1) : Just one of the options can be installed on a card at a time.
(2) : Third party product with warranty differing from our export conditions. No volume rebate possible.

(c) Spectrum Instrumentation GmbH 37

| Introduction |     |     |     |     | Order Information (M4i.96xx and M4x.96xx) |
| ------------ | --- | --- | --- | --- | ----------------------------------------- |
M4x Order Information
The card is delivered with 4 GiByte on-board memory (512 MiCommands for DDS or 2 GiSample for AWG data). The multi-core DDS gen-
eration is the standard mode. Adding the AWG option, the cards supports standard replay, FIFO replay (streaming), Multiple Replay, Gated
Replay, Continuous Replay (Loop), Single-Restart as well as Sequence. Operating system drivers for Windows/Linux 32 bit and 64 bit, ex-
amples for C/C++, LabVIEW (Windows), MATLAB (Windows and Linux), IVI, .NET, Delphi, Java, Python, Julia and a Base license of the
measurement software SBench 6 (AWG mode only) are included.
Adapter cables are not included. Please order separately!
PXI Express x4 Order no. Bandwidth DDS memory Channels AWG memory
|     | M4x.9621-x4 | 200 MHz | 512 MiCommands | 2   | 2 GiSample |
| --- | ----------- | ------- | -------------- | --- | ---------- |
|     | M4x.9622-x4 | 200 MHz | 512 MiCommands | 4   | 2 GiSample |

Firmware Options
|     | Order no. | Option |     |     |     |
| --- | --------- | ------ | --- | --- | --- |
M4i.96xx-AWG Firmware Option AWG mode. Full AWG (arbitrary waveform generator) functionality including different replay modes,
freely programmable clock and trigger modes.
M4i.xxxx-PulseGen Firmware Option: adds 4 freely programmable digital pulse generators that use the XIO lines for output (later installation by
firmware -upgrade available)

Standard Cables
Order no.
for Connections Length to BNC male to BNC female to SMA male to SMA female to SMB female
Analog/Clock-In/Trig-In 80 cm Cab-3mA-9m-80 Cab-3mA-9f-80 Cab-3mA-3mA-80 Cab-3f-3mA-80
Analog/Clock-In/Trig-In 200 cm Cab-3mA-9m-200 Cab-3mA-9f-200 Cab-3mA-3mA-200 Cab-3f-3mA-200
|     | Probes (short) | 5 cm | Cab-3mA-9f-5 |     |     |
| --- | -------------- | ---- | ------------ | --- | --- |
Clk-Out/Trig-Out/Extra 80 cm Cab-1m-9m-80 Cab-1m-9f-80 Cab-1m-3mA-80 Cab-1m-3fA-80 Cab-1m-3f-80
Clk-Out/Trig-Out/Extra 200 cm Cab-1m-9m-200 Cab-1m-9f200 Cab-1m-3mA-200 Cab-1m-3fA-200 Cab-1m-3f-200
Information The standard adapter cables are based on RG174 cables and have a nominal attenuation of 0.3dB/m at 100MHz and
0.5dB/m at 250MHz. For high speed signals we recommend the low loss cables series CHF

| Low Loss Cables | Order No.       | Option                                      |     |     |     |
| --------------- | --------------- | ------------------------------------------- | --- | --- | --- |
|                 | CHF-3mA-3mA-200 | Low loss cables SMA male to SMA male 200 cm |     |     |     |
|                 | CHF-3mA-9m-200  | Low loss cables SMA male to BNC male 200 cm |     |     |     |
Information The low loss adapter cables are based on MF141 cables and have an attenuation of 0.3dB/m at 500MHz and
0.5dB/m at 1.5GHz. They are recommended for signal frequencies of 200MHz and above.

| Services         | Order no.       |                                                                                              |     |     |     |
| ---------------- | --------------- | -------------------------------------------------------------------------------------------- | --- | --- | --- |
|                  | Recal           | Recalibration at Spectrum incl. calibration protocol                                         |     |     |     |
|                  | Card-Prot-17025 | ISO 17025 Calibration Certificate for one card; add-on for new cards or for recalibration    |     |     |     |
| Software SBench6 | Order no.       |                                                                                              |     |     |     |
|                  | SBench6         | Base version included in delivery. Supports standard mode for one card.                      |     |     |     |
|                  | SBench6-Pro     | Professional version for one card: FIFO mode, export/import, calculation functions, ...      |     |     |     |
|                  | SBench6-Multi   | Option multiple cards: Needs SBench6-Pro. Handles multiple synchronized cards in one system. |     |     |     |
|                  | Volume Licenses | Please ask Spectrum for details.                                                             |     |     |     |

Software Options
Order no.
|     | SPc-RServer | Remote Server Software Package - LAN remote access for M2i/M3i/M4i/M4x/M2p/M5i cards |     |     |     |
| --- | ----------- | ------------------------------------------------------------------------------------ | --- | --- | --- |
SPc-SCAPP Spectrum’s CUDA Access for Parallel Processing - SDK for direct data transfer between Spectrum card and CUDA GPU.
Includes RDMA activation and examples. Only working for Data Acquisition or AWG mode, not usable for DDS mode
  (1) : Just one of the options can be installed on a card at a time.
(2) : Third party product with warranty differing from our export conditions. No volume rebate possible.

(c) Spectrum Instrumentation GmbH 38

Hardware Installation ESD Precautions
