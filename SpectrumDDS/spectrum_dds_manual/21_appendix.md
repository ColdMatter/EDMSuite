Appendix
Error Codes
The following error codes could occur when a driver function has been called. Please check carefully the allowed setup for the register and
change the settings to run the program.
Table 218: Spectrum API: driver error codes and error description
error name value value error description
(hex) (dec.)
ERR_OK 0h 0 Execution OK, no error.
ERR_INIT 1h 1 An error occurred when initializing the given card. Either the card has already been opened by another process or
an hardware error occurred.
ERR_TYP 3h 3 Initialization only: The type of board is unknown. This is a critical error. Please check whether the board is correctly
plugged in the slot and whether you have the latest driver version.
ERR_FNCNOTSUPPORTED 4h 4 This function is not supported by the hardware version.
ERR_BRDREMAP 5h 5 The board index re map table in the registry is wrong. Either delete this table or check it carefully for double values.
ERR_KERNELVERSION 6h 6 The version of the kernel driver is not matching the version of the DLL. Please do a complete re-installation of the hard-
ware driver. This error normally only occurs if someone copies the driver library and the kernel driver manually.
ERR_HWDRVVERSION 7h 7 The hardware needs a newer driver version to run properly. Please install the driver that was delivered together with
the card.
ERR_ADRRANGE 8h 8 One of the address ranges is disabled (fatal error), can only occur under Linux.
ERR_INVALIDHANDLE 9h 9 The used handle is not valid.
ERR_BOARDNOTFOUND Ah 10 A card with the given name has not been found.
ERR_BOARDINUSE Bh 11 A card with given name is already in use by another application.
ERR_EXPHW64BITADR Ch 12 Express hardware version not able to handle 64 bit addressing -> update needed.
ERR_FWVERSION Dh 13 Firmware versions of synchronized cards or for this driver do not match -> update needed.
ERR_SYNCPROTOCOL Eh 14 Synchronization protocol versions of synchronized cards do not match -> update needed.
ERR_KERNEL Fh 15 Some error occurred in the kernel driver. On Linux check output of dmesg for details.
ERR_LASTERR 10h 16 Old error waiting to be read. Please read the full error information before proceeding. The driver is locked until the
error information has been read.
ERR_BOARDINUSE 11h 17 Board is already used by another application. It is not possible to use one hardware from two different programs at
the same time.
ERR_ABORT 20h 32 Abort of wait function. This return value just tells that the function has been aborted from another thread. The driver
library is not locked if this error occurs.
ERR_BOARDLOCKED 30h 48 The card is already in access and therefore locked by another process. It is not possible to access one card through
multiple processes. Only one process can access a specific card at the time.
ERR_DEVICE_MAPPING 32h 50 The device is mapped to an invalid device. The device mapping can be accessed via the Control Center.
ERR_NETWORKSETUP 40h 64 The network setup of a digitizerNETBOX has failed.
ERR_NETWORKTRANSFER 41h 65 The network data transfer from/to a digitizerNETBOX has failed.
ERR_FWPOWERCYCLE 42h 66 Power cycle (PC off/on) is needed to update the card's firmware (a simple OS reboot is not sufficient !).
ERR_NETWORKTIMEOUT 43h 67 A network time out has occurred.
ERR_BUFFERSIZE 44h 68 The buffer size is not sufficient (too small).
ERR_RESTRICTEDACCESS 45h 69 The access to the card has been intentionally restricted.
ERR_INVALIDPARAM 46h 70 An invalid parameter has been used for a certain function.
ERR_TEMPERATURE 47h 71 The temperature of at least one of the card’s sensors measures a temperature, that is too high for the hardware.
ERR_REG 100h 256 The register is not valid for this type of board.
ERR_VALUE 101h 257 The value for this register is not in a valid range. The allowed values and ranges are listed in the board specific docu-
mentation.
ERR_FEATURE 102h 258 Feature (option) is not installed on this board. It’s not possible to access this feature if it’s not installed.
ERR_SEQUENCE 103h 259 Command sequence is not allowed. Please check the manual carefully to see which command sequences are possi-
ble.
ERR_READABORT 104h 260 Data read is not allowed after aborting the data acquisition.
ERR_NOACCESS 105h 261 Access to this register is denied. This register is not accessible for users.
ERR_TIMEOUT 107h 263 A time out occurred while waiting for an interrupt. This error does not lock the driver.
ERR_CALLTYPE 108h 264 The access to the register is only allowed with one 64 bit access but not with the multiplexed 32 bit (high and low
double word) version.
ERR_EXCEEDSINT32 109h 265 The return value is int32 but the software register exceeds the 32 bit integer range. Use double int32 or int64
accesses instead, to get correct return values.
ERR_NOWRITEALLOWED 10Ah 266 The register that should be written is a read-only register. No write accesses are allowed.
ERR_SETUP 10Bh 267 The programmed setup for the card is not valid. The error register will show you which setting generates the error mes-
sage. This error is returned if the card is started or the setup is written.
ERR_CLOCKNOTLOCKED 10Ch 268 Synchronization to external clock failed: no signal connected or signal not stable. Please check external clock or try to
use a different sampling clock to make the PLL locking easier.
ERR_MEMINIT 10Dh 269 On-board memory initialization error. Power cycle the PC and try another PCIe slot (if possible). In case that the error
persists, please contact Spectrum support for further assistance.
ERR_POWERSUPPLY 10Eh 270 On-board power supply error. Power cycle the PC and try another PCIe slot (if possible). In case that the error persists,
please contact Spectrum support for further assistance.
ERR_ADCCOMMUNICA- 10Fh 271 Communication with ADC failed. Power cycle the PC and try another PCIe slot (if possible). In case that the error per-
TION sists, please contact Spectrum support for further assistance.
(c) Spectrum Instrumentation GmbH 200

Appendix Error Codes
Table 218: Spectrum API: driver error codes and error description
error name value value error description
(hex) (dec.)
ERR_CHANNEL 110h 272 The channel number may not be accessed on the board: Either it is not a valid channel number or the channel is not
accessible due to the current setup (e.g. Only channel 0 is accessible in interlace mode).
ERR_NOTIFYSIZE 111h 273 The notify size of the last spcm_dwDefTransfer call is not valid. The notify size must be a multiple of the page size of
4096. For data transfer it may also be a fraction of 4Ki in the range of 16, 32, 64, 128, 256, 512, 1Ki or 2Ki. For
ABA and timestamp the notify size can be 2Ki as a minimum.
ERR_RUNNING 120h 288 The board is still running, this function is not available now or this register is not accessible now.
ERR_ADJUST 130h 304 Automatic card calibration has reported an error. Please check the card inputs.
ERR_PRETRIGGERLEN 140h 320 The calculated pretrigger size (resulting from the user defined posttrigger values) exceeds the allowed limit.
ERR_DIRMISMATCH 141h 321 The direction of card and memory transfer mismatch. In normal operation mode it is not possible to transfer data from
PC memory to card if the card is an acquisition card nor it is possible to transfer data from card to PC memory if the
card is a generation card.
ERR_POSTEXCDSEGMENT 142h 322 The posttrigger value exceeds the programmed segment size in multiple recording/ABA mode. A delay of the multiple
recording segments is only possible by using the delay trigger!
ERR_SEGMENTINMEM 143h 323 Memsize is not a multiple of segment size when using Multiple Recording/Replay or ABA mode. The programmed
segment size must match the programmed memory size.
ERR_MULTIPLEPW 144h 324 Multiple pulsewidth counters used but card only supports one at the time.
ERR_NOCHANNELPWOR 145h 325 The channel pulsewidth on this card can’t be used together with the OR conjunction. Please use the AND conjunction
of the channel trigger sources.
ERR_ANDORMASKOVRLAP 146h 326 Trigger AND mask and OR mask overlap in at least one channel. Each trigger source can only be used either in the
AND mask or in the OR mask, no source can be used for both.
ERR_ANDMASKEDGE 147h 327 One channel is activated for trigger detection in the AND mask but has been programmed to a trigger mode using an
edge trigger. The AND mask can only work with level trigger modes.
ERR_ORMASKLEVEL 148h 328 One channel is activated for trigger detection in the OR mask but has been programmed to a trigger mode using a
level trigger. The OR mask can only work together with edge trigger modes.
ERR_EDGEPERMOD 149h 329 This card is only capable to have one programmed trigger edge for each module that is installed. It is not possible to
mix different trigger edges on one module.
ERR_DOLEVELMINDIFF 14Ah 330 The minimum difference between low output level and high output level is not reached.
ERR_STARHUBENABLE 14Bh 331 The card holding the star-hub must be enabled when doing synchronization.
ERR_PATPWSMALLEDGE 14Ch 332 Combination of pattern with pulsewidth smaller and edge is not allowed.
ERR_XMODESETUP 14Dh 333 The chosen setup for (SPCM_X0_MODE .. SPCM_X19_MODE) is not valid. See hardware manual for details.
ERR_AVRG_LSA 14Eh 334 Setup for Average LSA Mode not valid. Check Threshold and Replacement values for chosen AVRGMODE.
ERR_PCICHECKSUM 203h 515 The check sum of the card information has failed. This could be a critical hardware failure. Restart the system and
check the connection of the card in the slot.
ERR_MEMALLOC 205h 517 Internal memory allocation failed. Please restart the system and be sure that there is enough free memory.
ERR_EEPROMLOAD 206h 518 Time out occurred while loading information from the on-board EEProm. This could be a critical hardware failure.
Please restart the system and check the PCI connector.
ERR_CARDNOSUPPORT 207h 519 The card that has been found in the system seems to be a valid Spectrum card of a type that is supported by the driver
but the driver did not find this special type internally. Please get the latest driver from
www.spectrum-instrumentation.com and install this one.
ERR_CONFIGACCESS 208h 520 Internal error occurred during config writes or reads. Please contact Spectrum support for further assistance.
ERR_FIFOHWOVERRUN 301h 769 FIFO acquisition:
Hardware buffer overrun in FIFO mode. The complete on-board memory has been filled with data and data wasn’t
transferred fast enough to PC memory.
FIFO replay:
Hardware buffer underrun in FIFO mode. The complete on-board memory has been replayed and data wasn’t trans-
ferred fast enough from PC memory.
If acquisition or replay throughput is lower than the theoretical bus throughput, check the application buffer setup.
ERR_FIFOFINISHED 302h 770 FIFO transfer has been finished, programmed data length has been transferred completely.
ERR_TIMESTAMP_SYNC 310h 784 Synchronization to timestamp reference clock failed. Please check the connection and the signal levels of the refer-
ence clock input.
ERR_STARHUB 320h 800 The auto routing function of the Star-Hub initialization has failed. Please check whether all cables are mounted cor-
rectly.
ERR_INTERNAL_ERROR FFFFh 65535 Internal hardware error detected. Please check for driver and firmware update of the card.
Spectrum Knowledge Base
You will also find additional help and information in our knowledge base available on our website:
https://spectrum-instrumentation.com/support/knowledgebase/index.php
(c) Spectrum Instrumentation GmbH 201

Temperature sensors
Temperature sensors
The M4i/M4x card series has integrated temperature sensors that allow to read out different internal temperatures. Theses functions are also
available for the internal M4i cards inside the digitizerNETBOX, generatorNETBOX or hybridNETBOX series. In here the temperature can be
read out for every internal card separately.
Temperature read-out registers
Up to three different temperature sensors can be read-out for each M4i and M4x card. Depending on the specific card type not all of these
temperature sensors are used. The temperature can be read in different temperature scales at any time:
Table 219: Spectrum API: temperature read-out registers of internal temperature sensors
| Register             |     | Value Direction | Description                              |     |
| -------------------- | --- | --------------- | ---------------------------------------- | --- |
| SPC_MON_TK_BASE_CTRL |     | 500022 read     | Base card temperature in Kelvin          |     |
| SPC_MON_TK_MODULE_0  |     | 500023 read     | Module temperature 0 in Kelvin           |     |
| SPC_MON_TK_MODULE_1  |     | 500024 read     | Module temperature 1 in Kelvin           |     |
| SPC_MON_TC_BASE_CTRL |     | 500025 read     | Base card temperature in degrees Celsius |     |
| SPC_MON_TC_MODULE_0  |     | 500026 read     | Module temperature 0 in degrees Celsius  |     |
| SPC_MON_TC_MODULE_1  |     | 500027 read     | Module temperature 1 in degrees Celsius  |     |
SPC_MON_TF_BASE_CTRL 500028 read Base card temperature in degrees Fahrenheit
SPC_MON_TF_MODULE_0 500029 read Module temperature 0 in degrees Fahrenheit
SPC_MON_TF_MODULE_1 500030 read Module temperature 1 in degrees Fahrenheit
Temperature hints
• Monitoring of the temperature figures is recommended for environments where the operating temperature can reach or even exceed the
specified operating temperature. Please see technical data section for specified operating temperatures.
• The temperature sensors can be used to optimize the system cooling.
66xx and 96xx temperatures and limits
The following description shows the meaning of each temperature figure on the 66xx or 96xx series cards and also gives maximum ratings
that should not be exceeded. All figures given in degrees Celsius:
Table 220: Spectrum API: temperature limits
Sensor Name Sensor Location Typical figure at 25°C Maximum temperature
environment temperature
| BASE_CTRL | Inside FPGA         | 50°C ±5°C |     | 80°C |
| --------- | ------------------- | --------- | --- | ---- |
| MODULE_0  | not used            | n.a.      |     | n.a. |
| MODULE_1  | Amplifier Front-End | 50°C ±5°C |     | 80°C |

(c) Spectrum Instrumentation GmbH 202

Details on M4i/M4x cards I/O lines
Details on M4i/M4x cards I/O lines
Multi-Purpose I/O Lines
The MMCX Multi Purpose I/O connec-
tors (X0, X1 and X2) of the M4i/M4x
cards from Spectrum are protected
against over-voltage conditions. For this
purpose clamping diodes of the types
CD1005 are used in conjunction with a
series resistor. All three I/O lines are in-
ternally clamped to signal ground and to
3.3V clamping voltage. This limits the
voltage at the buffer to 3.3V plus the di-
odes intrinsic forward-voltage of 0.6V to
0.7V.
The maximum forward current limit for
Image 102: electrical structure of multi-purpose I/O lines
the used CD1005 diodes is 100 mA,
which is effectively limited by the used
series resistor for logic levels up to 5.0V. To avoid floating levels with unconnected inputs, a pull up resistor of 10 kOhm to 3.3V is used on
each line.
Interfacing with clock input
The clock input of the M4i/M4x cards is AC-coupled, sin-
gle-ended PECL type. Due to the internal biasing and a
relatively high maximum input voltage swing, it can be di-
rectly connected to various logic standards, without the
need for external level converters.
Single-ended LVTTL sources
All LVTTL sources, be it 2.5V LVTTL or 3.3V LVTTL must be
terminated with a 50 Ohm series resistor to avoid reflec-
tions and limit the maximum swing for the M4i card.
Differential (LV)PECL sources
Differential drivers require equal load on both the true
and the inverting outputs. Therefore the inverting output
should be loaded as shown in the drawing. All PECL driv-
ers require a proper DC path to ground, therefore emitter
resistors Re must be used, whose value depends on the
supply voltage of the driving PECL buffer: Image 103: electrical structure of clock inputs and potential interfacing circuits
VCC - VEE 2.5 V 3.3 V 5.0 V
Re ~50 Ohm ~100 Ohm ~200 Ohm
Interfacing with clock output
The clock output of the M4i/M4x cards is AC-cou-
pled, single-ended PECL type. The output swing of
the M4i/M4x clock output is approximately
800mV .
PP
Internal biased single-ended receivers
Because of the AC coupling of the M4i/M4x clock
output, the signal must be properly re-biased for the
receiver. Receivers that provide an internal re-bias
only require the signal to be terminated to ground by
a 50Ohm resistor.
Differential (LV)PECL receivers
Differential receivers require proper re-biasing and
likely a small minimum difference between the true
and the inverting input to avoid ringing with open re-
ceiver inputs. Therefore a Thevenin-equivalent can
be used, with receiver-type dependent values
for R1, R2, R1’ and R2’.
Image 104: electrical structure of clock outputs and potential interfacing circuits
(c) Spectrum Instrumentation GmbH 203

Details on M4i/M4x cards I/O lines
(c) Spectrum Instrumentation GmbH 204

Details on M4i/M4x cards status LED
Details on M4i/M4x cards status LED
Every M4i card has a two-color status LED mounted within the multi-purpose I/O connector field on the card
bracket.
The same two-color LED is located on the bracket of the M4x cards as well.
This chapter explains the different color codes and offers some possible solutions in case of an error condition.
Table 221: card status LED color and blink coding
Condition LED color Status Solution
Off Off Card not powered Power on the PC.
Static: red Power supply error Restart the PC. In case that the error persists, please contact Spectrum support for further assis-
tance.
Fast blinking (aprox. 4 Hz): Power supply error Restart the PC. In case that the error persists, please contact Spectrum support for further assis-
red - green - red - green ... tance.
Error Blinking: red - off - red - off ... Over temperature error Power down the PC, let the card cool down and restart the system. Please make sure that you
have a proper cooling fan installed to supply the M4i card in the PCIe slot with a constant air
flow.
Strobed blinking: FPGA boot error This error code is available with Power Firmware V1.8 or newer.
long red - short off - short red - short Power down the PC and restart the system.
off ... In case that this error is occurring after a firmware update please contact Spectrum support for
assistance on how to boot the card’s golden recovery image.
Slow blinking (aprox. 1 Hz): PCI Express link training has 1) Power down the PC, un-plug and re-plug the card to verify that there is a proper contact
red - green - red - green ... not finished between the card and the slot.
2) Try another PCI Express slot, maybe the currently used one is not properly working.
In case that the above steps did not help, please contact Spectrum support for assistance.
Static: green Card is ready for operation A full speed PCIe link has been established (PCIe x8, Gen 2) and the card is ready for opera-
(at full PCIe speed) tion.
Slow blinking (approx. 1 Hz): Indicator mode on To ease the identification of a specific card in a multi-card system without un-installing the card
green - off - green - off ... (at full PCIe speed) it is possible to activate the card identification status by software. This mode changes the static
„Ready for Operation“ green into a blinking state.
Static: green & red (yellow) Card is ready for operation A reduced speed PCIe link has been established either with less than all of the possible 8 lanes
O.K. (at reduced PCIe speed) and/or the card is installed in a PCIe Gen 1 slot. The card is ready for operation, but the data
transfer throughput over the PCI Express bus is reduced.
For getting the highest PCIe performance please consult your PC’s or motherboard’s manual for
details on the PCI Express slots of your system.
Slow blinking (aprox. 1 Hz): Indicator mode on To ease the identification of a specific card in a multi-card system without un-installing the card
yellow - off - yellow - off ... (at reduced PCIe speed) it is possible to activate the card identification status by software. This mode changes the static
„Ready for Operation“ yellow into a blinking state.
Turning on card identification LED
To enable/disable the cards LED indicator mode or to read out the current setting, please use the following register:
Table 222: Spectrum API: card identification LED register
Register Value Direction Description
SPC_CARDIDENTIFICATION 201500 read/write Writing a ’1’ turns on the LED card indicator mode, writing a ’0’ turns off the LED indicator mode.
The default for the card identification register is the OFF state.
(c) Spectrum Instrumentation GmbH 205

Continuous memory for increased data transfer rate
Continuous memory for increased data transfer rate
The continuous memory buffer has been added to the driver version 1.36. The continuous buffer is not avail-
able in older driver versions. Please update to the latest driver if you wish to use this function.
Background
All modern operating systems use a very complex memory management strategy that strictly separates between physical memory, kernel mem-
ory and user memory. The memory management is based on memory pages (normally 4 KiByte = 4096 Bytes). All software only sees virtual
memory that is translated into physical memory addresses by a memory management unit based on the mentioned pages.
This will lead to the circumstance that although a user program allocated a larger memory block (as an example 1 MiByte) and it sees the
whole 1 MiByte as a virtually continuous memory area this memory is physically located as spread 4 KiByte pages all over the physical mem-
ory. No problem for the user program as the memory management unit will simply translate the virtual continuous addresses to the physically
spread pages totally transparent for the user program.
When using this virtual memory for a DMA transfer things become more complicated. The DMA engine of any hardware can only access
physical addresses. As a result the DMA engine has to access each 4 KiByte page separately. This is done through the Scatter-Gather list.
This list is simply a linked list of the physical page addresses which represent the user buffer. All translation and set-up of the Scatter-Gather
list is done inside the driver without being seen by the user. Although the Scatter-Gather DMA transfer is an advanced and powerful technol-
ogy it has one disadvantage: For each transferred memory page of data it is necessary to also load one Scatter-Gather entry (which is 16
bytes on 32 bit systems and 32 bytes on 64 bit systems). The little overhead to transfer (16/32 bytes in relation to 4096 bytes, being less
than one percent) isn’t critical but the fact that the continuous data transfer on the bus is broken up every 4096 bytes and some different
addresses have to be accessed slow things down.
The solution is very simple: everything works faster if the user buffer is not only virtually continuous but also physically continuous. Unfortu-
nately it is not possible to get a physically continuous buffer for a user program. Therefore the kernel driver has to do the job and the user
program simply has to read out the address and the length of this continuous buffer. This is done with the function spcm_dwGetContBuf as
already mentioned in the general driver description. The desired length of the continuous buffer has to be programmed to the kernel driver
for load time and is done different on the different operating systems. Please see the following chapters for more details.
Next we’ll see some measuring results of the data transfer rate with/without continuous buffer. You will find more results on different mother-
boards and systems in the application note number 6 „Bus Transfer Speed Details“. Also with newer M5i/M4i/M4x/M2p cards the gain in
speed is not as impressive, as it is for older cards, but can be useful in certain applications and settings. As this is also system dependent,
your improvements may vary. This can not only depending on the system hardware but also on the used operating system, as in some cases
Linux does seem to benefit more than Windows for newer cards.
(c) Spectrum Instrumentation GmbH 206

Continuous memory for increased data transfer rate
Bus Transfer Speed Details (M2i/M3i cards in an example system)
|                          | PCI 33 MHz slot   | PCI-X 66 MHz slot | PCI Express x1 slot |     |     |
| ------------------------ | ----------------- | ----------------- | ------------------- | --- | --- |
| Mode                     | read write        | read write        | read write          |     |     |
| User buffer              | 109 MB/s 107 MB/s | 195 MB/s 190 MB/s | 130 MB/s 138 MB/s   |     |     |
| Continuous kernel buffer | 125 MB/s 122 MB/s | 248 MB/s 238 MB/s | 160 MB/s 170 MB/s   |     |     |
| Speed advantage          | 15% 14%           | 27% 25%           | 24% 23%             |     |     |
Bus Transfer Standard Read/Write Transfer Speed Details (M4i.44xx card in an example system)
|      | Notifysize | Notifysize | Notifysize | Notifysize  | Notifysize  |
| ---- | ---------- | ---------- | ---------- | ----------- | ----------- |
|      | 16 KiByte  | 64 KiByte  | 512 KiByte | 2048 KiByte | 4096 KiByte |
| Mode | read write | read write | read write | read write  | read write  |
User buffer 243 MB/s 132 MB/s 793 MB/s 464 MB/s 2271 MB/s 1352 MB/s 2007 MB/s 1900 MB/s 2687 MB/s 2284 MB/s
Continuous kernel buffer 239 MB/s 133 MB/s 788 MB/s 457 MB/s 2270 MB/s 1470 MB/s 2555 MB/s 2121 MB/s 2989 MB/s 2549 MB/s
Speed advantage --1.6% +0.7% -0.6% -1.5% 0% +8.7% +27.3% +11.6% +11.2% +11.6%
Bus Transfer FIFO Read Transfer Speed Details (M4i.44xx card in an example system)
Notifysize Notifysize Notifysize Notifysize Notifysize Notifysize Notifysize Notifysize Notifysize
4 KiByte 8 KiByte 16 KiByte 32 KiByte 64 KiByte 256 KiByte 1024 KiByte 2048 KiByte 4096 KiByte
Mode FIFO read FIFO read FIFO read FIFO read FIFO read FIFO read FIFO read FIFO read FIFO read
User buffer 455 MB/s 858 MB/s 1794 MB/s 2005 MB/s 3335 MB/s 3386 MB/s 3369 MB/s 3331 MB/s 3335 MB/s
Continuous kernel buffer 540 MB/s 833 MB/s 1767 MB/s 1965 MB/s 3216 MB/s 3386 MB/s 3389 MB/s 3388 MB/s 3389 MB/s
Speed advantage +18.6% --2.9% --1.5% --2.0% --3.5% 0% +0.6% +1.7% +1.6%
Bus Transfer FIFO Read Transfer Speed Details (M2p.5942 card in an example system)
Notifysize Notifysize Notifysize Notifysize Notifysize Notifysize Notifysize Notifysize Notifysize
4 KiByte 8 KiByte 16 KiByte 32 KiByte 64 KiByte 256 KiByte 1024 KiByte 2048 KiByte 4096 KiByte
Mode FIFO read FIFO read FIFO read FIFO read FIFO read FIFO read FIFO read FIFO read FIFO read
User buffer 282 MB/s 462 MB/s 597 MB/s 800 MB/s 800 MB/s 799 MB/s 799 MB/s 799 MB/s 797 MB/s
Continuous kernel buffer 279 MB/s 590 MB/s 577 MB/s 800 MB/s 800 MB/s 800 MB/s 800 MB/s 800 MB/s 799 MB/s
Speed advantage -1.1% +27.7% --3.4% +0.0% +0.0% 0% +0.1% +0.1% +0.3%
Bus Transfer FIFO Read Transfer Speed Details (M5i.3337 card in an example system)
|                   | Notifysize Notifysize   | Notifysize  |     |     |     |
| ----------------- | ----------------------- | ----------- | --- | --- | --- |
|                   | 1024 KiByte 2048 KiByte | 4096 KiByte |     |     |     |
| Mode              | FIFO read FIFO read     | FIFO read   |     |     |     |
| User buffer       | 12.95 GB/s 13.26 GB/s   | 13.32 MB/s  |     |     |     |
| Continuous buffer | 13.02 GB/s 13.26 GB/s   | 13.39 GB/s  |     |     |     |
Windows 128MiB
Setup on Linux systems
On Linux systems the continuous buffer setting is done via the command line argument contmem_mb when loading the kernel driver module:
insmod spcm.ko contmem_mb=4
As memory allocation is organized completely different compared to Windows the amount of data that is available for a continuous DMA
buffer is unfortunately limited to a maximum of 8 MiByte. On most systems it will even be only 4 MiBytes.
To use a larger continuous buffer you can use the Continuous Memory Allocator (CMA). To allocate continuous memory this way you pass
„cma=xyz“ as kernel boot parameter, with xyz being the size of the continuous memory, e.g. „cma=128M“ for 128 MiByte.
Your kernel needs to have CMA support enabled to use this.
You can check this with „grep CONFIG_CMA /boot/config-$(uname -r)“.
To enable CMA in our spcm4 kernel driver module edit the Makefile for the kernel driver module and uncomment the line #EXTRA_CFLAGS
+= -DSPCM4_USE_CMA by removing the # in front. Then recompile the kernel module and load it as described above, like so as example:.
insmod spcm4.ko contmem_mb=128
Using a continuous buffer of this size will need root privileges for the using program on most systems!
(c) Spectrum Instrumentation GmbH 207

Continuous memory for increased data transfer rate
Setup on Windows systems
The continuous buffer settings is done with the
SpectrumControlCenter using a setup located on
the „Support“ page. Please fill in the desired continu-
ous buffer settings as MiByte. After setting up the val-
ue the system needs to be restarted as the allocation
of the buffer is done during system boot time.
If the system cannot allocate the amount of memory it
will divide the desired memory by two and try again.
This will continue until the system can allocate a con-
tinuous buffer. Please note that this try and error rou-
tine will need several seconds for each failed
allocation try during boot up procedure. During these
tries the system will look like being crashed. It is then
recommended to change the buffer settings to a
smaller value to avoid the long waiting time during
boot up.
Continuous buffer settings should not exceed 1/4 of
Image 105: setting up continuous memory buffer in Spectrum Control Center
system memory. During tests the maximum amount
that could be allocated was 384 MiByte of continu-
ous buffer on a system with 4 GiByte memory installed.
Usage of the buffer
The usage of the continuous memory is very simple. It is just necessary to read the start address of the continuous memory from the driver and
use this address instead of a self allocated user buffer for data transfer.
Function spcm_dwGetContBuf
This function reads out the internal continuous memory buffer (in bytes) if one has been allocated. If no buffer has been allocated the function
returns a size of zero and a NULL pointer.
uint32 _stdcall spcm_dwGetContBuf_i64 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
uint32 dwBufType, // type of the buffer to read as listed above under SPCM_BUF_XXXX
void** ppvDataBuffer, // address of available data buffer
uint64* pqwContBufLen); // length of available continuous buffer
uint32 _stdcall spcm_dwGetContBuf_i64m (// Return value is an error code
drv_handle hDevice, // handle to an already opened device
uint32 dwBufType, // type of the buffer to read as listed above under SPCM_BUF_XXXX
void** ppvDataBuffer, // address of available data buffer
uint32* pdwContBufLenH, // high part of length of available continuous buffer
uint32* pdwContBufLenL); // low part of length of available continuous buffer
Please note that it is not possible to free the continuous memory for the user application.
Example
The following example shows a simple standard single mode data acquisition setup (for a card with 12/14/16 bit per resolution one sample
equals 2 bytes) with the read out of data afterwards. To keep this example simple there is no error checking implemented.
int32 lMemsize = 16384; // recording length is set to 16 KiSamples
spcm_dwSetParam_i64 (hDrv, SPC_CHENABLE, CHANNEL0); // only one channel activated
spcm_dwSetParam_i32 (hDrv, SPC_CARDMODE, SPC_REC_STD_SINGLE); // set the standard single recording mode
spcm_dwSetParam_i64 (hDrv, SPC_MEMSIZE, lMemsize); // recording length in samples
spcm_dwSetParam_i64 (hDrv, SPC_POSTTRIGGER, 8192); // samples to acquire after trigger = 8Ki
// now we start the acquisition and wait for the interrupt that signalizes the end
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_CARD_START | M2CMD_CARD_ENABLETRIGGER | M2CMD_CARD_WAITREADY);
// we now try to use a continuous buffer for data transfer or allocate our own buffer in case there’s none
spcm_dwGetContBuf_i64 (hDrv, SPCM_BUF_DATA, &pvData, &qwContBufLen);
if (qwContBufLen < (2 * lMemsize))
pvData = pvAllocMemPageAligned (lMemsize * 2); // assuming 2 bytes per sample
// read out the data
spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_CARDTOPC , 0, pvData, 0, 2 * lMemsize);
spcm_dwSetParam_i32 (hDrv, SPC_M2CMD, M2CMD_DATA_STARTDMA | M2CMD_DATA_WAITDMA);
// ... Use the data here for analysis/calculation/storage
// delete our own buffer in case we have created one
if (qwContBufLen < (2 * lMemsize))
vFreeMemPageAligned (pvData, lMemsize * 2);
(c) Spectrum Instrumentation GmbH 208

Abbreviations
Table 223: Abbreviations used throughout the Spectrum documents
| Abbreviation | Long Name | Description |
| ------------ | --------- | ----------- |
| s            | Second    |             |
ms Milli Second 1/1000 second; 1 ms is the time between two samples when running at 1 kS/s
us (µs) Micro Second 1/1000000 second or 1/1000 milli second; 1 us is the time between two samples when running at 1 MS/s
ns Nano Second 1/1000000000 second or 1/1000 micro second; 1 ns is the time between two samples when running at 1
GS/s
ps Pico Second 1/1000000000000 second or 1/1000 nano second; 100 ps is the time between two samples when run-
ning at 10 GS/s
fs Femto Second 1/1000000000000000 second or 1/1000 pico second
S Sample One sample represents one data word that has been acquired on the same time position. Each sample con-
sist of either one byte (8 bit resolution) or two bytes (12, 14 and 16 bit resolution)
| B   | Byte  | The smallest storage unit              |
| --- | ----- | -------------------------------------- |
| Hz  | Hertz | 1 Hertz is one event/sample per second |
PCIe PCI Express The PCI Express bus is a point to point connection allowing full speed for every single slot. The Express bus is
freely scaling and is available with 1 lane (x1), 4 lanes (x4), 8 lanes (x8) and 16 lanes (x16)
PXI PCI eXtensions for Instrumentation Based on the CompactPCI 3U standard the PXI (PCI eXtensions for Instrumentation) enhancement was
defined especially for the measurement user. In this specification additional lines for measurement purposes
are defined.
PXIe PXI Express PXI Express or PXIe is a subset of the PXI standard that replaces PXI’s parallel data bus with a high speed
serial interface.
AWG Arbitrary Waveform Generator A method of producing arbitrary shaped waveforms by replaying a stream of digital samples and perform-
ing a digital-to-analog conversion.
DDS Direct Digital Synthesis A method of producing an analog waveform, typically a sine wave, by generating a time-varying signal in
digital form and then performing a digital-to-analog conversion.
PLL Phase Lock Loop A clock device which generates a new clock phase-locked to a given reference clock.
LED Light-Emitting Diode A semiconductor device that emits light and is often used as a status light or indicator.
API Application Programming Interface A type of software interface, offering a service to access/control specific hardware or other pieces of soft-
ware.
| CPU | Central Processing Unit | The central processor of a computer/PC system.  |
| --- | ----------------------- | ----------------------------------------------- |
GPU Graphics Processing Unit An co-processor specifically tailored for fast and efficient and massively parallel calculations of certain data
structures. Often, but not exclusively, located on a separate PCIe graphics card or co-processing card. s
CUDA Compute Unified Device Architecture A proprietary API for Nvidia GPUs to perform “general purpose” as in non-graphic related processing on
GPUs rather than the CPU.
DMA Direct Memory Access A method to transfer data directly between a device (card) and PC memory.
RDMA Remote Direct Memory Access A method to transfer data directly between two devices (cards).
| RMA  | Return Manufacturer Authorization           |     |
| ---- | ------------------------------------------- | --- |
| WEEE | Waste Electrical and Electronic Equipment)  |     |
Prefixes:
| k   | Kilo | 1000 (10^3) e.g. kHz = 1000 Hertz               |
| --- | ---- | ----------------------------------------------- |
| M   | Mega | 1000000 (10^6)                                  |
| G   | Giga | 1000000000 (10^9)                               |
| T   | Tera | 1000000000000 (10^12)                           |
| Ki  | Kibi | 1024 (2^10) e.g. 1 KiB = 1024 Byte              |
| Mi  | Mebi | 1024 x 1024 (2^20) e.g. 1 MiB = 1024 KiB        |
| Gi  | Gibi | 1024 x 1024 x 1024 (2^30) e.g. 1 GiB = 1024 MiB |
Ti Tebi 1024 x 1024 x 1024 x 1024 (2^40) e.g. 1 TiB = 1024 GiB
(c) Spectrum Instrumentation GmbH 209

