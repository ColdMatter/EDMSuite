Programming the Board
Overview
The following chapters show you in detail how to program the different aspects of the board. For every topic there’s a small example. For
the examples we focused on Visual C++. However as shown in the last chapter the differences in programming the board under different
programming languages are marginal. This manual describes the programming of the whole hardware family. Some of the topics are similar
for all board versions. But some differ a little bit from type to type. Please check the given tables for these topics and examine carefully which
settings are valid for your special kind of board.
Register tables
The programming of the boards is totally software register based. All software registers are described in the following form:
| The name of the software regis- | The decimal value of the software register.  |     |     |
| ------------------------------- | -------------------------------------------- | --- | --- |
Describes whether  Short description of the function-
| ter as found in the regs.h file.  | Also found in the regs.h file. This value must  |     |     |
| --------------------------------- | ----------------------------------------------- | --- | --- |
the register can be  ality of the register. A more de-
| These Mnemonics should be  | be used with all programs or compilers that  |     |     |
| -------------------------- | -------------------------------------------- | --- | --- |
read (r) and/or writ- tailed description is found
| used to increase readability. | cannot use the header file directly.  |     |     |
| ----------------------------- | ------------------------------------- | --- | --- |
ten (w). above or below the register ta-
bles.
Table 9: Spectrum API: Command register and basic commands
| Register         | Value Direction                                         | Description                    |     |
| ---------------- | ------------------------------------------------------- | ------------------------------ | --- |
| SPC_M2CMD        | 100 w                                                   | Command register of the board. |     |
| M2CMD_CARD_START | 4h Starts the board with the current register settings. |                                |     |
| M2CMD_CARD_STOP  | 40h Stops the board manually.                           |                                |     |
Any constants that can be used to  The decimal or hexadecimal value of the  Short description of
program the register directly are  constant, also found in the regs.h file. Hex- the use of this con-
| shown inserted beneath the register  | adecimal values are indicated with an „h“     |     | stant. |
| ------------------------------------ | --------------------------------------------- | --- | ------ |
| table.                               | at the end. This value must be used with all  |     |        |
programs or compilers that cannot use the
header file directly.
If no constants are given below the register table, the dedicated register is used as a switch. All such registers
are activated if written with a “1“ and deactivated if written with a “0“.
Programming examples
In this manual a lot of programming examples are used to give you an impression on how the actual mentioned registers can be set within
your own program. All of the examples are located in a separated colored box to indicate the example and to make it easier to differ it from
the describing text.
All of the examples mentioned throughout the manual are written in C/C++ and can be used with any C/C++ compiler for Windows or Linux.
(c) Spectrum Instrumentation GmbH 79

Programming the Board Initialization
Complete C/C++ Example
#include “../c_header/dlltyp.h”
#include “../c_header/regs.h”
#include “../c_header/spcm_drv.h”
#include <stdio.h>
int main()
{
drv_handle hDrv; // the handle of the device
int32 lCardType; // a place to store card information
hDrv = spcm_hOpen ("/dev/spcm0"); // Opens the board and gets a handle
if (!hDrv) // check whether we can access the card
return -1;
spcm_dwGetParam_i32 (hDrv, SPC_PCITYP, &lCardType); // simple command, read out of card type
printf (“Found card M2i/M3i/M4i/M4x/M2p/M5i.%04x in the system\n”, lCardType & TYP_VERSIONMASK);
spcm_vClose (hDrv);
return 0;
}
Initialization
Before using the card it is necessary to open the kernel device to access the hardware. It is only possible to use every device exclusively using
the handle that is obtained when opening the device. Opening the same device twice will only generate an error code. After ending the
driver use the device has to be closed again to allow later re-opening. Open and close of driver is done using the spcm_hOpen and spcm_v-
Close function as described in the “Driver Functions” chapter before.
Open/Close Example
drv_handle hDrv; // the handle of the device
hDrv = spcm_hOpen ("/dev/spcm0"); // Opens the board and gets a handle
if (!hDrv) // check whether we can access the card
{
printf “Open failed\n”);
return -1;
}
... do any work with the driver
spcm_vClose (hDrv);
return 0;
Initialization of Remote Products
The only step that is different when accessing remotely controlled cards or digitizerNETBOXes is the initialization of the driver. Instead of the
local handle one has to open the VISA string that is returned by the discovery function. Alternatively it is also possible to access the card
directly without discovery function if the IP address of the device is known.
drv_handle hDrv; // the handle of the device
hDrv = spcm_hOpen ("TCPIP::192.168.169.14::INSTR"); // Opens the remote board and gets a handle
if (!hDrv) // check whether we can access the card
{
printf “Open of remote card failed\n”);
return -1;
}
...
Multiple cards are opened by indexing the remote card number:
hDrv = spcm_hOpen ("TCPIP::192.168.169.14::INSTR"); // Opens the remote board #0
// or alternatively
hDrv = spcm_hOpen ("TCPIP::192.168.169.14::INST0::INSTR"); // Opens the remote board #0
// all other boards require an index:
hDrv = spcm_hOpen ("TCPIP::192.168.169.14::INST1::INSTR"); // Opens the remote board #1
hDrv = spcm_hOpen ("TCPIP::192.168.169.14::INST2::INSTR"); // Opens the remote board #2
(c) Spectrum Instrumentation GmbH 80

Programming the Board Error handling
Error handling
If one action caused an error in the driver this error and the register and value where it occurs will be saved.
The driver is then locked until the error is read out using the error function spcm_dwGetErrorInfo_i32. Any
calls to other functions will just return the error code ERR_LASTERR showing that there is an error to be read
out.
This error locking functionality will prevent the generation of unseen false commands and settings that may lead to totally unexpected behav-
ior. For sure there are only errors locked that result on false commands or settings. Any error code that is generated to report a condition to
the user won’t lock the driver. As example the error code ERR_TIMEOUT showing that the a timeout in a wait function has occurred won’t
lock the driver and the user can simply react to this error code without reading the complete error function.
As a benefit from this error locking it is not necessary to check the error return of each function call but just checking the error function once
at the end of all calls to see where an error occurred. The enhanced error function returns a complete error description that will lead to the
call that produces the error.
Example for error checking at end using the error text from the driver:
char szErrorText[ERRORTEXTLEN];
spcm_dwSetParam_i64 (hDrv, SPC_SAMPLERATE, 1000000); // correct command
spcm_dwSetParam_i64 (hDrv, SPC_MEMSIZE, -345); // faulty command
spcm_dwSetParam_i64 (hDrv, SPC_POSTTRIGGER, 1024); // correct command
if (spcm_dwGetErrorInfo_i32 (hDrv, NULL, NULL, szErrorText) != ERR_OK) // check for an error
{
printf (szErrorText); // print the error text
spcm_vClose (hDrv); // close the driver
exit (0); // and leave the program
}
This short program then would generate a printout as:
Error ocurred at register SPC_MEMSIZE with value -345: value not allowed
All error codes are described in detail in the appendix. Please refer to this error description and the descrip-
tion of the software register to examine the cause for the error message.
Any of the parameters of the spcm_dwGetErrorInfo_i32 function can be used to obtain detailed information on the error. If one is not interested
in parts of this information it is possible to just pass a NULL (zero) to this variable like shown in the example. If one is not interested in the
error text but wants to install its own error handler it may be interesting to just read out the error generating register and value.
Example for error checking with own (simple) error handler:
uint32 dwErrorReg;
int32 lErrorValue;
uint32 dwErrorCode;
spcm_dwSetParam_i64 (hDrv, SPC_SAMPLERATE, 1000000); // correct command
spcm_dwSetParam_i64 (hDrv, SPC_MEMSIZE, -345); // faulty command
spcm_dwSetParam_i64 (hDrv, SPC_POSTTRIGGER, 1024); // correct command
dwErrorCode = spcm_dwGetErrorInfo_i32 (hDrv, &dwErrorReg, &lErrorValue, NULL);
if (dwErrorCode) // check for an error
{
printf (“Errorcode: %d in register %d at value %d\n”, lErrorCode, dwErrorReg, lErrorValue);
spcm_vClose (hDrv); // close the driver
exit (0); // and leave the program
}
Printing custom strings to the driver debug log
The detailed driver debug logfile has proven to not only being a great help for support issues, but also for one’s own debugging during
application development. To additionally help the user in debugging, the driver allows to print output lines initiated by the application soft-
ware.
This feature requires the scm_dwSetParam_ptr to be implemented, which has been added with driver version V7.00
The following example shows how to print a custom message to the logfile:
const char* szText = "My custom log file entry";
spcm_dwSetParam_ptr (NULL, SPC_WRITE_TO_LOG, szText, strlen(szText)); // print line to debug log file
(c) Spectrum Instrumentation GmbH 81

Programming the Board Gathering information from the card

Gathering information from the card
When opening the card the driver library internally reads out a lot of information from the on-board eeprom. The driver also offers additional
information on hardware details. All of this information can be read out and used for programming and documentation. This chapter will
show all general information that is offered by the driver. There is also some more information on certain parts of the card, like clock machine
or trigger machine, that is described in detail in the documentation of that part of the card.
All information can be read out using one of the spcm_dwGetParam functions. Please stick to the “Driver Functions” chapter for more details
on this function.
Card type
The card type information returns the specific card type that is found under this device. When using multiple cards in one system it is highly
recommended to read out this register first to examine the ordering of cards. Please don’t rely on the card ordering as this is based on the
BIOS, the bus connections and the operating system.
Table 10: Spectrum API: Card Type Register
| Register   |     | Value | Direction Description                            |
| ---------- | --- | ----- | ------------------------------------------------ |
| SPC_PCITYP |     | 2000  | read Type of board as listed in the table below. |
The SPC_PCITYP register can be used to read the numeric card type as well as a full name of the card using the spcm_dwGetParam_ptr
function:
// read out the numeric card type as shown in the list below
spcm_dwGetParam_i32 (hDrv, SPC_PCITYP,  &lCardType);
// read out the official name of the card
char acCardType[20] = {};
spcm_dwGetParam_ptr (hCard, SPC_PCITYP, acCardType, sizeof (acCardType));
// printout both information:
printf ("Found: %s (decimal: %d)\n", acCardType, lCardType);
One of the following values is returned, when reading this register. Each card has its own card type constant defined in regs.h. Please note
that when reading the card information as a hex value, the lower word shows the digits of the card name while the upper word is a indication
for the used bus type.

66xx AWG cards:
Table 11: Spectrum API: list of card type codes for M4i.66xx series
Card type Card type Value  Value Card type Card type Value Value
as defined in  hexadecimal decimal as defined in  hexadecimal decimal
regs.h regs.h
M4i.6620-x8 TYP_M4I6620_X8 76620h 484896 M4i.6630-x8 TYP_M4I6630_X8 76630h 484912
M4i.6621-x8 TYP_M4I6621_X8 76621h 484897 M4i.6631-x8 TYP_M4I6631_X8 76631h 484913
| M4i.6622-x8 | TYP_M4I6622_X8 | 76622h | 484898 |
| ----------- | -------------- | ------ | ------ |
Table 12: Spectrum API: list of card type codes for M4x.66xx series
Card type Card type Value  Value Card type Card type Value Value
as defined in  hexadecimal decimal as defined in  hexadecimal decimal
regs.h regs.h
M4x.6620-x4 TYP_M4X6620_X4 86620h 550432 M4x.6630-x4 TYP_M4X6630_X4 86630h 550448
M4x.6621-x4 TYP_M4X6621_X4 86621h 550433 M4x.6631-x4 TYP_M4X6631_X4 86631h 550449
| M4x.6622-x4 | TYP_M4X6622_X4 | 86622h | 550434 |
| ----------- | -------------- | ------ | ------ |
96xx DDS cards:.
Table 13: Spectrum API: list of card type codes for M4i.96xx series
| Card type | Card type      | Value       | Value   |
| --------- | -------------- | ----------- | ------- |
|           | as defined in  | hexadecimal | decimal |
regs.h
| M4i.9620-x8 | TYP_M4I9620_X8 | 79620h | 497184 |
| ----------- | -------------- | ------ | ------ |
| M4i.9621-x8 | TYP_M4I9621_X8 | 79621h | 497185 |
| M4i.9622-x8 | TYP_M4I9622_X8 | 79622h | 497186 |
Table 14: Spectrum API: list of card type codes for M4x.96xx series
| Card type | Card type      | Value       | Value   |
| --------- | -------------- | ----------- | ------- |
|           | as defined in  | hexadecimal | decimal |
regs.h
| M4x.9621-x4 | TYP_M4X9621_X4 | 89621h | 562721 |
| ----------- | -------------- | ------ | ------ |
| M4x.9622-x4 | TYP_M4X9622_X4 | 89622h | 562722 |
(c) Spectrum Instrumentation GmbH 82

Programming the Board Gathering information from the card
Hardware and PCB version
Since all of the boards from Spectrum are modular boards, they consist of one base board and one piggy-back front-end module and even-
tually of an extension module like the star-hub. Each of these three kinds of hardware has its own version register. Normally you do not need
this information but if you have a support question, please provide the revision together with it.
Table 15: Spectrum API: hardware and PCB version register overview
Register Value Direction Description
SPC_PCIVERSION 2010 read Base card version: the upper 16 bit show the hardware version, the lower 16 bit show the firmware
version.
SPC_BASEPCBVERSION 2014 read Base card PCB version: the lower 16 bit are divided into two 8 bit values containing pre/post deci-
mal point version information. For example a lower 16 bit value of 0106h represents a PCB version
V1.6. The upper 16 bit are always zero.
SPC_PCIMODULEVERSION 2012 read Module version: the upper 16 bit show the hardware version, the lower 16 bit show the firmware ver-
sion.
SPC_MODULEPCBVERSION 2015 read Module PCB version: the lower 16 bit are divided into two 8 bit values containing pre/post decimal
point version information. For example a lower 16 bit value of 0106h represents a PCB version
V1.6. The upper 16 bit are always zero.
If your board has an additional piggy-back extension module mounted you can get the hardware version with the following register.
Table 16: Spectrum API: extension module hardware and PCB version register
Register Value Direction Description
SPC_PCIEXTVERSION 2011 read Extension module version: the upper 16 bit show the hardware version, the lower 16 bit show the
firmware version.
SPC_EXTPCBVERSION 2017 read Extension module PCB version: the lower 16 bit are divided into two 8 bit values containing pre/post
decimal point version information. For example a lower 16 bit value of 0106h represents a PCB ver-
sion V1.6. The upper 16 bit are always zero.
Reading currently used PXI slot No. (M4x only)
For the PXIe cards of the M4x.xxxx series it is possible to read out the current slot number, in which the card is installed within the chassis:
Table 17: Spectrum API: register for reading back the PXIe card slot number
Register Value Direction Description
SPC_PXIHWSLOTNO 2055 read Returns the currently used slot number of the chassis.
Firmware versions
All the cards from Spectrum typically contain multiple programmable devices such as FPGAs, CPLDs and the like. Each of these have their
own dedicated firmware version. This version information is readable for each device through the various version registers. Normally you do
not need this information but if you have a support question, please provide us with this information. Please note that number of devices and
hence the readable firmware information is card series dependent:
Table 18: Spectrum API: Register overview of firmware versions
Register Value Direction Description Available for
M2i M3i M4i M4x M2p M5i
SPCM_FW_CTRL 210000 read Main control FPGA version: the upper 16 bit show the firmware X X X X X X
type, the lower 16 bit show the firmware version. For the stand-
ard release firmware, the type has always a value of 1.
SPCM_FW_CTRL_GOLDEN 210001 read Main control FPGA golden version: the upper 16 bit show the — — X X X X
firmware type, the lower 16 bit show the firmware version. For
the golden (recovery) firmware, the type has always a value of
2.
SPCM_FW_CLOCK 210010 read Clock distribution version: the upper 16 bit show the firmware X — — — — —
type, the lower 16 bit show the firmware version. For the stand-
ard release firmware, the type has always a value of 1.
SPCM_FW_CONFIG 210020 read Configuration controller version: the upper 16 bit show the firm- X X — — — —
ware type, the lower 16 bit show the firmware version. For the
standard release firmware, the type has always a value of 1.
SPCM_FW_MODULEA 210030 read Front-end module A version: the upper 16 bit show the firmware X X X X X X
type, the lower 16 bit show the firmware version. For the stand-
ard release firmware, the type has always a value of 1.
SPCM_FW_MODULEB 210031 read Front-end module B version: the upper 16 bit show the firmware X — — — X —
type, the lower 16 bit show the firmware version. For the stand-
ard release firmware, the type has always a value of 1.
The version is zero if no second front-end module is installed on
the card.
SPCM_FW_MODEXTRA 210050 read Extension module (Star-Hub) version: the upper 16 bit show the X X X — X X
firmware type, the lower 16 bit show the firmware version. For
the standard release firmware, the type has always a value of 1.
The version is zero if no extension module is installed on the
card.
SPCM_FW_POWER 210060 read Power controller version: the upper 16 bit show the firmware — — X X X X
type, the lower 16 bit show the firmware version. For the stand-
ard release firmware, the type has always a value of 1.
(c) Spectrum Instrumentation GmbH 83

Programming the Board Gathering information from the card
Cards that do provide a golden recovery image for the main control FPGA, the currently booted firmware can additionally read out:
Table 19: Spectrum API: Register overview of reading current firmware
Register Value Direction Description
M2i M3i M4i M4x M2p M5i
SPCM_FW_CTRL_ACTIVE 210002 read Cards that do provide a golden (recovery) firmware additionally — — X X X X
have a register to read out the version information of the cur-
rently loaded firmware image, to determine if it is a standard or
golden image, and which kind of standard version is booted, in
case that different configurations are available.
The hexadecimal 32bit format is: TVVVCCUUh
T: the currently booted type (1: standard, 2: golden)
V: the version
C: configuration: for different kinds of firmware (0: default)
U: unused, in production versions always zero
SPC_NUM_FW_CONFIGS 210100 read Allows to read out the number of different firmware images avail- — — X X X X
able for the current card series.
Each available firmware will have a different configuration.
The following example shows how to read out the number of available firmware configurations for the card series and displays which con-
figuration is currently used.
spcm_dwGetParam_i32 (hDrv, SPCM_FW_CTRL_ACTIVE, &lActiveFW);
int32 lTyp = ((lActiveFW >> 28) & 0xf);
int32 lVer = ((lActiveFW >> 16) & 0xfff);
int32 lCfg = ((lActiveFW >> 8) & 0xff);
spcm_dwGetParam_i32 (hDrv, SPC_NUM_FW_CONFIGS, &lNumOfFW);
printf ("%d FW configs avialable. Active FW T:%d, V:%d, C:%d\n“, lNumOfFW, lTyp, lVer, lCfg);
Most cards only have one “Default” firmware configuration available. Hence the number returned when
reading SPC_NUM_FW_CONFIGS is 1 for most cards and the active configuration is typically 0.
If more than just the one default firmware configuration is available, switching images can be done with the help of the Spectrum Control
Center. Please see “Firmware switching” paragraph in the “Card Control Center” section for details.
Production date
This register informs you about the production date, which is returned as one 32 bit long word. The lower word is holding the information
about the year, while the upper word informs about the week of the year.
Table 20: Spectrum API: production date register
Register Value Direction Description
SPC_PCIDATE 2020 read Production date: week in bits 31 to 16, year in bits 15 to 0
The following example shows how to read out a date and how to interpret the value:
spcm_dwGetParam_i32 (hDrv, SPC_PCIDATE, &lProdDate);
printf ("Production: week &d of year &d\n“, (lProdDate >> 16) & 0xffff, lProdDate & 0xffff);
Last calibration date (A/D and D/A cards only)
This register informs you about the date of the last factory calibration. When receiving a new card this date is similar to the delivery date
when the production calibration is done. When returning the card to calibration this information is updated. This date is not updated when
the user does an on-board calibration. The date is returned as one 32 bit long word. The lower word is holding the information about the
year, while the upper word informs about the week of the year.
Table 21: Spectrum API: calibration date register
Register Value Direction Description
SPC_CALIBDATE 2025 read Last calibration date: week in bit 31 to 16, year in bit 15 to 0
Serial number
This register holds the information about the serial number of the board. This number is unique and should always be sent together with a
support question. Normally you use this information together with the register SPC_PCITYP to verify that multiple measurements are done with
the exact same board.
Table 22: Spectrum API: hardware serial number register
Register Value Direction Description
SPC_PCISERIALNO 2030 read Serial number of the board
(c) Spectrum Instrumentation GmbH 84

Programming the Board Gathering information from the card
Maximum possible sampling rate
This register gives you the maximum possible sampling rate the board can run. The information provided here does not consider any restric-
tions in the maximum speed caused by special channel settings. For detailed information about the correlation between the maximum sam-
pling rate and the number of activated channels please refer to the according chapter.
Table 23: Spectrum API: maximum sampling rate register
Register Value Direction Description
SPC_PCISAMPLERATE 2100 read Maximum sampling rate in Hz as a 64 bit integer value
Installed memory
This register returns the size of the installed on-board memory in bytes as a 64 bit integer value. If you want to know the amount of samples
you can store, you must regard the size of one sample of your card. All 7 bit and 8 bit A/D and D/A cards use only one byte per sample,
while all other A/D and D/A cards with 12, 14 and 16 bit resolution use two bytes to store one sample. All digital cards need one byte to
store 8 data bits.
Table 24: Spectrum API: installed memory registers
Register Value Direction Description
SPC_PCIMEMSIZE 2110 read _i32 Installed memory in bytes as a 32 bit integer value. Maximum return value will 1 GiByte. If more
memory is installed this function will return the error code ERR_EXCEEDINT32.
SPC_PCIMEMSIZE 2110 read _i64 Installed memory in bytes as a 64 bit integer value
The following example is written for a „two bytes“ per sample card (12, 14 or 16 bit board), on any 8 bit card memory in MiSamples is
similar to memory in MiBytes.
spcm_dwGetParam_i64 (hDrv, SPC_PCIMEMSIZE, &llInstMemsize);
printf ("Memory on card: %d MiBytes\n", (int32) (llInstMemsize /1024/1024));
printf (" : %d MiSamples\n", (int32) (llInstMemsize /1024/1024/2));
Installed features and options
The SPC_PCIFEATURES register informs you about the features, that are installed on the board. If you want to know about one option being
installed or not, you need to read out the 32 bit value and mask the interesting bit. In the table below you will find every feature that may be
installed on a M2i/M3i/M4i/M4x/M2p/M5i card. Please refer to the ordering information to see which of these features are available for
your card series.
Table 25: Spectrum API: Feature Register and available feature flags
Register Value Direction Description
SPC_PCIFEATURES 2120 read PCI feature register. Holds the installed features and options as a bitfield. The read value must be
masked out with one of the masks below to get information about one certain feature.
SPCM_FEAT_MULTI 1h Is set if the feature Multiple Recording / Multiple Replay is available.
SPCM_FEAT_GATE 2h Is set if the feature Gated Sampling / Gated Replay is available.
SPCM_FEAT_DIGITAL 4h Is set if the feature Digital Inputs / Digital Outputs is available.
SPCM_FEAT_TIMESTAMP 8h Is set if the feature Timestamp is available.
SPCM_FEAT_STARHUB6_EXTM 20h Is set on the card, that carries the star-hub extension or piggy-back module for synchronizing up to 6 cards (M2p).
SPCM_FEAT_STARHUB8_EXTM 20h Is set on the card, that carries the star-hub extension or piggy-back module for synchronizing up to 8 cards (M4i).
SPCM_FEAT_STARHUB4 20h Is set on the card, that carries the star-hub piggy-back module for synchronizing up to 4 cards (M3i).
SPCM_FEAT_STARHUB5 20h Is set on the card, that carries the star-hub piggy-back module for synchronizing up to 5 cards (M2i).
SPCM_FEAT_STARHUB16_EXTM 40h Is set on the card, that carries the star-hub piggy-back module for synchronizing up to 16 cards (M2p).
SPCM_FEAT_STARHUB8 40h Is set on the card, that carries the star-hub piggy-back module for synchronizing up to 8 cards (M3i and M5i).
SPCM_FEAT_STARHUB16 40h Is set on the card, that carries the star-hub piggy-back module for synchronizing up to 16 cards (M2i).
SPCM_FEAT_ABA 80h Is set if the feature ABA mode is available.
SPCM_FEAT_BASEXIO 100h Is set if the extra BaseXIO option is installed. The lines can be used for asynchronous digital I/O, extra trigger or
timestamp reference signal input.
SPCM_FEAT_AMPLIFIER_10V 200h Arbitrary Waveform Generators only: card has additional set of calibration values for amplifier card.
SPCM_FEAT_STARHUBSYSMASTER 400h Is set in the card that carries a System Star-Hub Master card to connect multiple systems (M2i).
SPCM_FEAT_DIFFMODE 800h M2i.30xx series only: card has option -diff installed for combining two SE channels to one differential channel.
SPCM_FEAT_SEQUENCE 1000h Only available for output cards or I/O cards: Replay sequence mode available.
SPCM_FEAT_AMPMODULE_10V 2000h Is set on the card that has a special amplifier module for mounted (M2i.60xx/61xx only).
SPCM_FEAT_STARHUBSYSSLAVE 4000h Is set in the card that carries a System Star-Hub Slave module to connect with System Star-Hub master systems (M2i).
SPCM_FEAT_NETBOX 8000h The card is physically mounted within a digitizerNETBOX, generatorNETBOX or hybridNETBOX.
SPCM_FEAT_REMOTESERVER 10000h Support for the Spectrum Remote Server option is installed on this card.
SPCM_FEAT_SCAPP 20000h Support for the SCAPP option allowing CUDA RDMA access to supported graphics cards for GPU calculations
(M5i, M4i and M2p)
SPCM_FEAT_DIG16_SMB 40000h M2p: Set if option M2p.xxxx-DigSMB is installed, adding16 additional digital I/Os via SMB connectors.
SPCM_FEAT_DIG16_FX2 80000h M2p: Set if option M2p.xxxx-DigFX2 is installed, adding16 additional digital I/Os via FX2 multipin connectors.
SPCM_FEAT_DIGITALBWFILTER 100000h A digital (boxcar) bandwidth filter is available that can be globally enabled/disabled for all channels.
SPCM_FEAT_CUSTOMMOD_MASK F0000000h The upper 4 bit of the feature register is used to mark special custom modifications. This is only used if the card has
been specially customized. Please refer to the extra documentation for the meaning of the custom modifications.
(M2i/M3i). For M5i, M4i, M4x and M2p cards see „Custom modifications“ chapter instead.
(c) Spectrum Instrumentation GmbH 85

Programming the Board Gathering information from the card
The following example demonstrates how to read out the information about one feature.
spcm_dwGetParam_i32 (hDrv, SPC_PCIFEATURES, &lFeatures);
if (lFeatures & SPCM_FEAT_DIGITAL)
printf("Option digital inputs/outputs is installed on your card");
The following example demonstrates how to read out the custom modification code.
spcm_dwGetParam_i32 (hDrv, SPC_PCIFEATURES, &lFeatures);
lCustomMod = (lFeatures >> 28) & 0xF;
if (lCustomMod != 0)
printf("Custom modification no. %d is installed.", lCustomMod);
Installed extended Options and Features
Some cards (such as M5i/M4i/M4x/M2p cards) can have advanced features and options installed. This can be read out with the following
register:
Table 26: Spectrum API: Extended feature register and available extended feature flags
Register Value Direction Description
SPC_PCIEXTFEATURES 2121 read PCI extended feature register. Holds the installed extended features and options as a bitfield. The
read value must be masked out with one of the masks below to get information about one certain fea-
ture.
SPCM_FEAT_EXTFW_SEGSTAT 1h Is set if the firmware option “Block Statistics” is installed on the board, which allows certain statistics to be on-board
calculated for data being recorded in segmented memory modes, such as Multiple Recording or ABA.
SPCM_FEAT_EXTFW_SEGAVERAGE 2h Is set if the firmware option “Block Average“ is installed on the board, which allows on-board hardware averaging of
data being recorded in segmented memory modes, such as Multiple Recording or ABA.
SPCM_FEAT_EXTFW_BOXCAR 4h Is set if the firmware mode “Boxcar Average“ is supported in the installed firmware version.
SPCM_FEAT_EXTFW_PULSEGEN 8h Is set if the firmware mode “Pulse Generator” is installed on the board, which allows generation of pulses for output
on the card’s multi-purpose I/O lines (XIO).
SPCM_FEAT_EXTFW_DDS20 10h If set, the firmware option “DDS20” (20 tone DDS) is installed. Prior to the introduction of the “DDS50”, this feature
was named “SPCM_FEAT_EXTFW_DDS”.
This register has been renamed, whilst the associated value remained unchanged.
SPCM_FEAT_EXTFW_DDS50 20h If set, the firmware option “DDS50” (50 tone DDS) is installed.
SPCM_FEAT_EXTFW_AWG 40h If set, the firmware option “AWG” (arbitrary waveform generator) is installed.
Miscellaneous Card Information
Some more detailed card information, that might be useful for the application to know, can be read out with the following registers:
Table 27: Spectrum API: register overview of miscellaneous cards information
Register Value Direction Description
SPC_MIINST_MODULES 1100 read Number of the installed front-end modules on the card.
SPC_MIINST_CHPERMODULE 1110 read Number of channels installed on one front-end module.
SPC_MIINST_BYTESPERSAMPLE 1120 read Number of bytes used in memory by one sample.
SPC_MIINST_BITSPERSAMPLE 1125 read Resolution of the samples in bits.
SPC_MIINST_MAXADCVALUE 1126 read Decimal code of the full scale value.
SPC_MIINST_MINEXTCLOCK 1145 read Minimum external clock that can be fed in for direct external clock (if available for card model).
SPC_MIINST_MAXEXTCLOCK 1146 read Maximum external clock that can be fed in for direct external clock (if available for card model).
SPC_MIINST_MINEXTREFCLOCK 1148 read Minimum external clock that can be fed in as a reference clock.
SPC_MIINST_MAXEXTREFCLOCK 1149 read Maximum external clock that can be fed in as a reference clock.
SPC_MIINST_ISDEMOCARD 1175 read Returns a value other than zero, if the card is a demo card.
Function type of the card
This register register returns the basic type of the card:
Table 28: Spectrum API: register card function type and possible types
Register Value Direction Description
SPC_FNCTYPE 2001 read Gives information about what type of card it is.
SPCM_TYPE_AI 1h Analog input card (analog acquisition; the M2i.4028 and M2i.4038 also return this value)
SPCM_TYPE_AO 2h Analog output card (arbitrary waveform generators)
SPCM_TYPE_DI 4h Digital input card (logic analyzer card)
SPCM_TYPE_DO 8h Digital output card (pattern generators)
SPCM_TYPE_DIO 10h Digital I/O (input/output) card, where the direction is software selectable.
(c) Spectrum Instrumentation GmbH 86

Programming the Board Gathering information from the card
Used type of driver
This register holds the information about the driver that is actually used to access the board. Although the driver interface doesn’t differ be-
tween Windows and Linux systems it may be of interest for a universal program to know on which platform it is working.
Table 29: Spectrum API: register driver type information and possible driver types
| Register |     | Value Direction | Description |
| -------- | --- | --------------- | ----------- |
SPC_GETDRVTYPE 1220 read Gives information about what type of driver is actually used
| DRVTYP_LINUX32 |     | 1 Linux 32bit driver is used |     |
| -------------- | --- | ---------------------------- | --- |
DRVTYP_WDM32 4 Windows WDM 32bit driver is used (XP/Vista/Windows 7/8/10/11).
DRVTYP_WDM64 5 Windows WDM 64bit driver is used by 64bit application (XP64/Vista/Windows 7/8/10/11).
DRVTYP_WOW64 6 Windows WDM 64bit driver is used by 32bit application (XP64/Vista/Windows 7/8/10/11).
| DRVTYP_LINUX64 |     | 7 Linux 64bit driver is used |     |
| -------------- | --- | ---------------------------- | --- |
Driver version
This register holds information about the currently installed driver library. As the drivers are permanently improved and maintained and new
features are added user programs that rely on a new feature are requested to check the driver version whether this feature is installed.
Table 30: Spectrum API: driver version read register
| Register |     | Value Direction | Description |
| -------- | --- | --------------- | ----------- |
SPC_GETDRVVERSION 1200 read Gives information about the driver library version
The resulting 32 bit value for the driver version consists of the three version number parts shown in the table below:
| Driver Major Version         | Driver Minor Version         | Driver Build                 |     |
| ---------------------------- | ---------------------------- | ---------------------------- | --- |
| 8 Bit wide: bit 24 to bit 31 | 8 Bit wide, bit 16 to bit 23 | 16 Bit wide, bit 0 to bit 15 |     |
Kernel Driver version
This register informs about the actually used kernel driver. Windows users can also get this information from the device manager. Please refer
to the „Driver Installation“ chapter. On Linux systems this information is also shown in the kernel message log at driver start time.
Table 31: Spectrum API: kernel driver version read register
| Register |     | Value Direction | Description |
| -------- | --- | --------------- | ----------- |
SPC_GETKERNELVERSION 1210 read Gives information about the kernel driver version.
The resulting 32 bit value for the driver version consists of the three version number parts shown in the table below:
| Driver Major Version         | Driver Minor Version         | Driver Build                 |     |
| ---------------------------- | ---------------------------- | ---------------------------- | --- |
| 8 Bit wide: bit 24 to bit 31 | 8 Bit wide, bit 16 to bit 23 | 16 Bit wide, bit 0 to bit 15 |     |
The following example demonstrates how to read out the kernel and library version and how to print them.
spcm_dwGetParam_i32 (hDrv, SPC_GETDRVVERSION,    &lLibVersion);
spcm_dwGetParam_i32 (hDrv, SPC_GETKERNELVERSION, &lKernelVersion);
printf("Kernel V %d.%d build %d\n”,lKernelVersion >> 24, (lKernelVersion >> 16) & 0xff, lKernelVersion & 0xffff);
printf("Library V %d.%d build %d\n”,lLibVersion >> 24, (lLibVersion >> 16) & 0xff, lLibVersion & 0xffff);
This small program will generate an output like this:
Kernel V 1.11 build 817
Library V 1.1 build 854

Custom modifications
Since all of the boards from Spectrum are modular boards, they consist of one base board and one piggy-back front-end module and even-
tually of an extension module like the Star-Hub. Each of these three kinds of hardware has its own version register. Normally you do not need
this information but if you have a support question, please provide the revision together with it.
Table 32: Spectrum API: custom modification register and different bitmasks to split the register in various hardware parts
| Register |     | Value Direction | Description |
| -------- | --- | --------------- | ----------- |
SPCM_CUSTOMMOD 3130 read Dedicated feature register used to mark special custom modifications of the base card and/or the
front-end module and/or the Star-Hub module. This is only used if the card has been specially
customized. Please refer to the extra documentation for the meaning of the custom modifications.
This register is supported for all M5i, M4i, M4x, M2p cards and all digitizerNETBOX,
generatorNETBOX or hybridNETBOX based upon these series of cards.
| SPCM_CUSTOMMOD_BASE_MASK    |     | 000000FFh Mask for the custom modification of the base card.           |     |
| --------------------------- | --- | ---------------------------------------------------------------------- | --- |
| SPCM_CUSTOMMOD_MODULE_MASK  |     | 0000FF00h Mask for the custom modification of the front-end module(s). |     |
| SPCM_CUSTOMMOD_STARHUB_MASK |     | 00FF0000h Mask out custom modification of the Star-Hub module.         |     |
(c) Spectrum Instrumentation GmbH 87

Programming the Board Reset
Reset
Every Spectrum card can be reset by software. Concerning the hardware, this reset is the same as the power-on reset when starting the host
computer. In addition to the power-on reset, the reset command also brings all internal driver settings to a defined default state. A software
reset is automatically performed, when the driver is first loaded after starting the host system.
Performing a board reset can be easily done by the related board command mentioned in the following table.
Table 33: Spectrum API: command register and reset command
Register Value Direction Description
SPC_M2CMD 100 w Command register of the board.
M2CMD_CARD_RESET 1h A software and hardware reset is done for the board. All settings are set to the default values. The data in the board’s
on-board memory will be no longer valid. Any output signals like trigger or clock output will be disabled.
(c) Spectrum Instrumentation GmbH 88

Analog Outputs Channel Selection
