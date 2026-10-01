Software
This chapter gives you an overview about the structure of the drivers and the software, where to find and how to use the examples. It shows
in detail, how the drivers are included using different programming languages and deals with the differences when calling the driver functions
from them.
This manual only shows the use of the standard driver API. For further information on programming drivers
for third-party software like LabVIEW, MATLAB, IVI or SCAPP an additional manual is required that is avail-
able on the USB stick or by download from our homepage.
Software Overview
Image 19: Spectrum Kernel Driver, API Library and Software structure
The Spectrum drivers offer you a common and fast API for using all of the board hardware features. This API is the same on all supported
operating systems. Based on this API one can write own programs using any programming language that can access the driver API. This
manual describes in detail the driver API, providing you with the necessary information to write your own programs.
The drivers for third-party products like LabVIEW or MATLAB, IVI or SCAPP are also based on this API. The special functionality of these
drivers is not subject of this document and is described with separate manuals available on the USB stick or on the website.
Card Control Center
A special Card Control Center is available on the USB stick and from the internet for
all Spectrum M2i/M3i/M4i/M4x/M2p/M5i cards and for all digitizerNETBOX,
generatorNETBOX or hybridNETBOX products. Windows users find the Control
Center installer on the USB stick under „Install\win\spcmcontrol_install.exe“.
Linux users find the versions for the different stdc++ libraries under /Install/linux/sp-
cm_control_center/ as RPM packages.
When using a digitizerNETBOX/generatorNETBOX/hybridNETBOX the Card Con-
trol Center installers for Windows and Linux are also directly available from the in-
tegrated webserver.
The Control Center under Windows and Linux is available as an executive program.
Under Windows it is also linked as a system control and can be accessed directly
from the Windows control panel. Under Linux it is also available from the KDE Sys- Image 20: Spectrum Control Center Installer
tem Settings, the Gnome or Unity Control Center. The different functions of the Spectrum Card Control Center are explained in detail in the
following passages.
To install the Spectrum Control Center you will need to be logged in with administrator rights for your oper-
ating system. On all Windows versions, starting with Windows Vista, installations with enabled UAC will ask
you to start the installer with administrative rights (run as administrator).
(c) Spectrum Instrumentation GmbH 51

Software Card Control Center
Discovery of Remote Cards, digitizerNETBOX/generatorNETBOX/hybridNETBOX products
The Discovery function helps you to find and identify the Spectrum LXI
instruments like digitizerNETBOX, generatorNETBOX or
hybridNETBOX available to your computer on the network. The Dis-
covery function will also locate Spectrum card products handled by
an installed Spectrum Remote Server somewhere on the network. The
function is not needed if you only have locally installed cards.
Please note that only remote products are found that are currently not
used by another program. Therefore in a bigger network the number
of Spectrum products found may vary depending on the current usage
of the products.
Execute the Discovery function by pressing the „Discovery“ button.
There is no progress window shown. After the discovery function has
been executed the remotely found Spectrum products are listed under
the node Remote as separate card level products. Inhere you find all
hardware information as shown in the next topic and also the needed
VISA resource string to access the remote card.
Please note that these information is also stored on your system and
allows Spectrum software like SBench 6 to access the cards directly
once found with the Discovery function.
After closing the control center and re-opening it the previously found
remote products are shown with the prefix cached, only showing the
card type and the serial number. This is the stored information that al-
lows other Spectrum products to access previously found cards. Using
the „Update cached cards“ button will try to re-open these cards and
Image 21: Spectrum Control Center showing detail card information
gather information of it. Afterwards the remote cards may disappear
if they’re in use from somewhere else or the complete information of
the remote products is shown again.
Enter IP Address of digitizerNETBOX/generatorNETBOX/hybridNETBOX manually
If for some reason an automatic discovery is not suitable, such as the case where
the remote device is located in a different subnet, it can also be manually ac-
cessed by its type and IP address.
Image 22: Spectrum Control Center - entering an IP address for a NETBOX
Wake On LAN of digitizerNETBOX/generatorNETBOX/hybridNETBOX
Cached digitizerNETBOX/generatorNETBOX/hybridNETBOX products that are currently in
standby mode can be woken up by using the „Wake remote device“ entry from the context
menu.
The Control Center will broadcast a standard Wake On LAN „Magic Packet“, that is sent to the
device’s MAC address.
It is also possible to use any other Wake On LAN software to wake e.g. a digitizerNETBOX by
sending such a „Magic Packet“ to the MAC address, which must be then entered manually.
It is also possible to wake a remote device from your own application software by using the
SPC_NETBOX_WAKEONLAN register. To wake a digitizerNETBOX, generatorNETBOX or
hybridNETBOX with the MAC address „00:03:2d:20:48:ec“, the following command can be
issued:
spcm_dwSetParam_i64 (NULL, SPC_NETBOX_WAKEONLAN, 0x00032d2048ec);
Image 23: Spectrum Control Center: wake on LAN
for a cached card
(c) Spectrum Instrumentation GmbH 52

Software Card Control Center
Netbox Monitor
The Netbox Monitor permanently monitors whether the digitizerNETBOX/generatorNETBOX/hybridNETBOX is still available through LAN.
This tool is helpful if e.g. the digitizerNETBOX is located somewhere in the company LAN or located remotely or directly mounted inside
another device. Starting the Netbox Monitor can be done in two different ways:
• Starting manually from the Spectrum Control Center using the context menu as shown above
• Starting from command line. The Netbox Monitor program is automatically installed together with the Spectrum Control Center and is
located in the selected install folder. Using the command line tool one can place a simple script into the autostart folder to have the Net-
box Monitor running automatically after system boot. The command line tool needs the IP address of the
digitizerNETBOX/generatorNETBOX/hybridNETBOX to monitor:
NetboxMonitor 192.168.169.22
The Netbox Monitor is shown as a small window with the type of digitizerNETBOX/generatorNETBOX in the title and the IP ad-
dress under which it is accessed in the window itself. The Netbox Monitor runs completely independent of any other software and
can be used in parallel to any application software. The background of the IP address is used to display the current status of the
device. Pressing the Escape key or alt + F4 (Windows) terminates the Netbox Monitor permanently.
After starting the Netbox Monitor it is also displayed as a tray icon under Windows. The tray icon itself shows the
status of the digitizerNETBOX/generatorNETBOX/hybridNETBOX as a color. Please note that the tray icon may
be hidden as a Windows default and need to be set to visible using the Windows tray setup.
Left clicking on the tray icon will hide/show the small Netbox Monitor status window. Right clicking on the tray
icon as shown in the picture on the right will open up a context menu. In here one can again select to hide/show
the Netbox Monitor status window, one can directly open the web interface from here or quit the program (includ-
ing the tray icon) completely.
Image 24: Netbox Monitor ac-
The checkbox „Show Status Message“ controls whether the tray icon should emerge a status message on status tivation
change. If enabled (which is default) one is notified with a status message if for example the LAN connection to
the digitizerNETBOX/generatorNETBOX/hybridNETBOX is lost.
The status colors:
• Green: digitizerNETBOX/generatorNETBOX/hybridNETBOX available and accessible over LAN
• Cyan: digitizerNETBOX/generatorNETBOX/hybridNETBOX is used from my computer
• Yellow: digitizerNETBOX/generatorNETBOX/hybridNETBOX is used from a different computer
• Red: LAN connection failed, digitizerNETBOX/generatorNETBOX/hybridNETBOX is no longer accessible
Device identification
Pressing the Identification button helps to identify a certain device in either a remote
location, such as inside a 19“ rack where the back of the device with the type plate
is not easily accessible, or a local device installed in a certain slot. Pressing the button
starts flashing a visible LED on the device, until the dialog is closed, for:
• On a digitizerNETBOX/generatorNETBOX/hybridNETBOX: the LAN LED light on
the front plate of the device Image 25: Device Identification
• On local or remote M5i, M4i, M4x or M2p card: the indicator LED on the card’s
bracket
This feature is not available for M2i/M3i cards, either local or remote, other than inside a digitizerNETBOX or generatorNETBOX.
(c) Spectrum Instrumentation GmbH 53

Software Card Control Center
Hardware information
Through the Control Center you can easily get the main informa-
tion about all the installed Spectrum hardware. For each installed
card there is a separate tree of information available. The picture
shows the information for one installed card by example. This giv-
en information contains:
• Basic information as the type of card, the production date and
its serial number, as well as the installed memory, the hard-
ware revision of the base card, the number of available chan-
nels and installed acquisition modules.
• Information about the maximum sampling clock and the availa-
ble quartz clock sources.
• The installed features/options in a sub-tree. The shown card is
equipped for example with the option Multiple Recording,
Gated Sampling, Timestamp and ABA-mode.
• Detailed Information concerning the installed acquisition mod-
ules. In case of the shown analog acquisition card the informa-
tion consists of the module’s hardware revision, of the
converter resolution and the last calibration date as well as
detailed information on the available analog input ranges, off-
set compensation capabilities and additional features of the
inputs.
Image 26: Spectrum Control Center: detailed hardware information on installed card
Firmware information
Another sub-tree is informing about the cards firmware ver-
sion. As all Spectrum cards consist of several programma-
ble components, there is one firmware version per
component.
Nearly all of the components firmware can be updated by
software. The only exception is the configuration device,
which only can receive a factory update.
The procedure on how to update the firmware of your Spec-
trum card with the help of the card control center is de-
scribed in a dedicated section later on.
The procedure on how to update the firmware of your
digitizerNETBOX/generatorNETBOX/hybridNETBOX with
the help of the integrated Webserver is described in a ded-
icated chapter later on.
Image 27: Spectrum Control Center - showing firmware information of an installed card
(c) Spectrum Instrumentation GmbH 54

Software Card Control Center
Software License information
This sub-tree is informing about installed possible software li-
censes.
As a default all cards come with the demo professional li-
cense of SBench6, that is limited to 30 starts of the software
with all professional features unlocked.
The number of demo starts left can be seen here.
Image 28: Spectrum Control Center - showing software license information of an installed card
Driver information
The Spectrum card control center also offers a way to
gather information on the installed and used Spectrum
driver.
The information on the driver is available through a
dedicated tab, as the picture is showing in the example.
The provided information informs about the used type,
distinguishing between Windows or Linux driver and the
32 bit or 64 bit type.
It also gives direct information about the version of the
installed Spectrum kernel driver, separately for M2i/ M3i
cards and M4i/M4x/M2p/M5i cards and the version of
the library (which is the *.dll file under Windows).
The information given here can also be found under
Windows using the device manager form the
controlpanel. For details in driver details within the con-
trol panel please stick to the section on driver installation
in your hardware manual.
Image 29: Spectrum Control Center - showing driver information details
(c) Spectrum Instrumentation GmbH 55

Software Card Control Center
Installing and removing Demo cards
With the help of the card control center one can install
demo cards in the system. A demo card is simulated by the
Spectrum driver including data production for acquisition
cards. As the demo card is simulated on the lowest driver
level all software can be tested including SBench, own ap-
plications and drivers for third-party products like Lab-
VIEW. The driver supports up to 64 demo cards at the
same time. The simulated memory as well as the simulated
software options can be defined when adding a demo
card to the system.
Please keep in mind that these demo cards are only meant
to test software and to show certain abilities of the soft-
ware. They do not simulate the complete behavior of a
card, especially not any timing concerning trigger, record-
ing length or FIFO mode notification. The demo card will
calculate data every time directly after been called and
give it to the user application without any more delay. As
the calculation routine isn’t speed optimized, generating
demo data may take more time than acquiring real data
and transferring them to the host PC.
Installed demo cards are listed together with the real hard-
ware in the main information tree as described above. Ex-
isting demo cards can be deleted by clicking the related
button. The demo card details can be edited by using the
edit button. It is for example possible to virtually install ad-
ditional feature to one card or to change the type to test
with a different number of channels.
Image 30: Spectrum Control Center - adding a demo card to the sysstem
For installing demo cards on a system without
real hardware simply run the Control Center installer. If the installer is not detecting the necessary driver files
normally residing on a system with real hardware, it will simply install the Spcm_driver.
Feature upgrade
All optional features of the M2i/M3i/M4i/M4x/M2p/M5i cards that do not
require any hardware modifications can be installed on fielded cards. After
Spectrum has received the order, the customer will get a personalized up-
grade code. Just start the card control center, click on „install feature“ and en-
ter that given code. After a short moment the feature will be installed and
ready to use. No restart of the host system is required.
For details on the available options and prices please contact your local Spec- Image 31: Spectrum Control Center - feature update, code entry
trum distributor.
Software License upgrade
The software license for SBench 6 Professional is installed on the hardware. If
ordering a software license for a card that has already been delivered you will
get an upgrade code to install that software license. The upgrade code will only
match for that particular card with the serial number given in the license. To in-
stall the software license please click the „Install SW License“ button and type
in the code exactly as given in the license.
Image 32: Spectrum Control Center - software license installe
Performing card calibration
The Card Control Center also provides an easy way to access the
automatic card calibration routines of the Spectrum Cards, if that
feature is supported by the hardware.
On-board calibration is supported by all A/D converter cards
and some D/A converter cards. Depending on the used card
family this can affect offset calibration only or also might include
gain calibration. Please refer to the dedicated chapter in your
hardware manual for details. Image 33: Spectrum Control Center - running an on-board calibration
This function is not available for digital I/O cards and some D/A cards.
(c) Spectrum Instrumentation GmbH 56

Software Card Control Center
Performing memory test
The complete on-board memory of the Spectrum
M2i/M3i/M4i/M4x/M2p/M5i cards can be tested by the memory test includ-
ed with the card control center.
When starting the test, randomized data is generated and written to the on-
board memory. After a complete write cycle all the data is read back and com-
pared with the generated pattern.
Depending on the amount of installed on-board memory, and your computer’s
performance this operation might take a while.
Image 34: Spectrum Control Center - performing memory test
Transfer speed test
The control center allows to measure the bus transfer
speed of an installed Spectrum card. Therefore different
setup is run multiple times and the overall bus transfer
speed is measured. To get reliable results it is necessary
that you disable debug logging as shown below. It is
also highly recommended that no other software or time-
consuming background threads are running on that sys-
tem. The speed test program runs the following two tests:
• Repetitive Memory Transfers: single DMA data trans-
fers are repeated and measured. This test simulates
the measuring of pulse repetition frequency when
doing multiple single-shots. The test is done using dif-
ferent block sizes. One can estimate the transfer in
relation to the transferred data size on multiple single-
shots.
• FIFO mode streaming: this test measures the stream-
ing speed in FIFO mode. The test can only use the Image 35: Spectrum Control Center - running a transfer speed test of one card
same direction of transfer the card has been
designed for (card to PC = read for all DAQ (data acquisition) cards, PC to card = write for all generator cards and both directions for
I/O cards). The streaming speed is tested without using the front-end to measure the maximum bus speed that can be reached.
The Speed in FIFO mode depends on the selected notify size which is explained later in this manual in greater detail.
The results are given in MiB/s (Mebibyte = 1024 * 1024 bytes) and MB/s (Megabyte = 1000 * 1000 bytes). To estimate whether a desired
acquisition speed is possible to reach, one has to calculate the transfer speed in bytes. There are a few things that have to be put into the
calculation:
• 12, 14 and 16 bit analog cards need two bytes for each sample.
• 16 channel digital cards need 2 bytes per sample while 32 channel digital cards need 4 bytes and 64 channel digital cards need 8
bytes.
• The sum of analog channels must be used to calculate the total transfer rate.
• The figures in the Speed Test Utility are given as MiBytes, meaning 1024 * 1024 Bytes, 1 MiByte = 1048576 Bytes
As an example running a card with two 16 bit analog channels with 28 MSps produces a transfer rate of
[2 channels * 2 Bytes/Sample * 28000000] = 112000000 Bytes/second = 112 MB/s. When expressing this in Mebibytes, this would
equal to [112000000 / 1024 / 1024] = 106.8MiB/s. For convenience, the Control Center presents both values.
Unfortunately it is not possible to measure transfer speed on a system without having a Spectrum card installed.
(c) Spectrum Instrumentation GmbH 57

Software Card Control Center
Debug logging for support cases
For answering your support questions as fast as possible, the
setup of the card, driver and firmware version and other in-
formation is very helpful.
Therefore the card control center provides an easy way to
gather all that information automatically.
Different debug log levels are available through the graphi-
cal interface. By default the log level is set to „no logging“ for
maximum performance.
The customer can select different log levels and the path of
the generated ASCII text file. One can also decide to delete
the previous log file first before creating a new one automat-
ically or to append different logs to one single log file.
Image 36: Spectrum Control Center - activate debug logging for support cases
For maximum performance of your hardware, please make sure that the debug logging is set to „no log-
ging“ for normal operation. Please keep in mind that a detailed logging in append mode can quickly gener-
ate huge log files.
Device mapping
Within the „Device mapping“ tab of the Spectrum Control Center, one can ena-
ble the re-mapping of Spectrum devices, be it either local cards, remote instru-
ments such as a digitizerNETBOX, generatorNETBOX, hybridNETBOX or even
cards in a remote PC and accessed via the Spectrum remote server option.
In the left column the re-mapped device name is visible that is given to the device
in the right column with its original un-mapped device string.
In this example the two local cards „spcm0“ and „spcm1“ are re-mapped to „sp-
cm1“ and „spcm0“ respectively, so that their names are simply swapped.
The remote digitizerNETBOX device is mapped to spcm2.
The application software can then use the re-mapped name for simplicity instead
of the quite long VISA string.
Changing the order of devices within one group (either local cards or remote
devices) can simply be accomplished by dragging&dropping the cards to their
desired position in the same table.
Image 37: Spectrum Control Center - using device mapping
(c) Spectrum Instrumentation GmbH 58

Software Card Control Center
Firmware upgrade
One of the major features of the card control center is the ability to update
the card’s firmware by an easy-to-use software. The latest firmware revi-
sions can be found in the download section of our homepage under
http://www.spectrum-instrumentation.com.
A new firmware version is provided there as an installer, that copies the
latest firmware to your system. All files are located in a dedicated subfold-
er „FirmwareUpdate“ that will be created inside the Spectrum installation
folder. Under Windows this folder by default has been created in the
standard program installation directory.
Please do the following steps when wanting to update the firmware of
your M2i/M3i/M4i/M4x/M2p/M5i card:
• Download the latest software driver for your operating system pro-
vided on the Spectrum homepage.
• Install the new driver as described in the driver install section of your
hardware manual or install manual. All manuals can also be found on
the Spectrum homepage in the literature download section.
• Download and run the latest Spectrum Control Center installer.
• Download the installer for the new firmware version.
• Start the installer and follow the instructions given there.
• Start the card control center, select the „card“ tab, select the card from
the listbox and press the „firmware update“ button on the right side.
The dialog then will inform you about the currently installed firmware ver-
sion for the different devices on the card and the new versions that are
Image 38: Spectrum Control Center - doing a firmware update for one device
available. All devices that will be affected with the update are marked as
„update needed“. Simply start the update or cancel the operation now, as
a running update cannot be aborted.
Please keep in mind that you have to start the update for each card installed in your system separately. Select
one card after the other from the listbox and press the „firmware update“ button. The firmware installer on
the other hand only needs to be started once prior to the update.
Do not abort or shut down the computer while the firmware update is in progress. After a successful update
please shut down your PC completely (remove power). The re-powering is required to finally activate the
new firmware version of your Spectrum card.
Firmware switch
Most Spectrum cards come with just a single, default firmware configuration, that supports all of the device features. But some of the cards
require different firmware configurations (different firmware images) for certain operating modes or for a certain set of features.
For such cards the firmware configuration can be changed by using the “Firmware Switch” function of the Spectrum Control Center:
Image 39: Spectrum Control Center - selecting a firmware configuration for one card
(c) Spectrum Instrumentation GmbH 59

Software Accessing the hardware with SBench 6
The dialog shows a list of available firmware configurations for the current hardware and provides information:
• Info: which firmware is currently used
• FW Version: The firmware version of this particular firmware configuration
• FW CC: The number of the firmware configuration. Most cards have only one single default configuration 0.
• Description: A more detailed information of what each configuration is capable of
The firmware directory is by default set to the path to which the FW-Installer has installed the firmware files. This folder can be changed
however to a specific location, if required.
For switching to a different configuration, press the “Use” button. The firmware will be applied to the hardware, similar to a firmware update.
Afterwards
Please keep in mind that you have to switch the configuration for each card installed in your system sepa-
rately. Select one card after the other from the listbox and press the „firmware switch“ button. The firmware
installer on the other hand only needs to be started once prior to the update.
Certain firmware configurations might only be available, when a certain feature/license is installed. In case
that a seperate license is required, the switch dialog will still display all available configurations, but might
not allow to select certain configurations. This information will be shown in the “Description” column.
Do not abort or shut down the computer while the firmware switch is in progress. After a successful firmware
switch please shut down your PC completely (remove power). The re-powering is required to finally activate
the new firmware version of your Spectrum card.
Accessing the hardware with SBench 6
Image 40: SBench 6 overview of main functionality with demo data
After the installation of the cards and the drivers it can be useful to first test the card function with a ready to run software before starting with
programming. If accessing a digitizerNETBOX/generatorNETBOX a full SBench6 Professional license is installed on the system and can be
used without any limitations. For plug-in card level products a base version of SBench6 is delivered with the card on USB stick also including
a 30starts Professional demo version for plain card products. If you already have bought a card prior to the first SBench 6 release please
contact your local dealer to get a SBench6 Professional demo version. All digitizerNETBOX/generatorNETBOX products come with a pre-
installed full SBench6 Professional.
SBench 6 supports all current acquisition and generation cards and
digitizerNETBOX/generatorNETBOX products from Spectrum. Depending on the used product and the software setup, one can use SBench
as a digital storage oscilloscope, a spectrum analyzer, a signal generator, a pattern generator, a logic analyzer or simply as a data recording
front end. Different export and import formats allow the use of SBench6 together with a variety of other programs.
(c) Spectrum Instrumentation GmbH 60

Software Accessing the hardware with DDS Control (DDS firmware required)
On the USB stick you’ll find an install version of SBench6 in the directory „/Install/SBench6“.
The current version of SBench6 is available free of charge directly from the Spectrum website: www.spectrum-instrumentation.com. Please
go to the download section and get the latest version there.
SBench6 has been designed to run under Windows7, 8, 10 and Windows 11 as well as Linux using KDE, Gnome or Unity Desktop.
Accessing the hardware with DDS Control (DDS firmware required)
Image 41: SBench 6 overview of main functionality with demo data
If you have purchased an Arbitrary Waveform Generator (AWG) with Direct Digital Synthesis (DDS) firmware option, the DDS Control tool
enables you to use and test the DDS functionality of your card. DDS Control supports the following cards:
• M2p.65xx
• M4i.66xx and M4i.96xx as well as the PXIe versions M4x.66xx and M4x.96xx
• M5i.63xx.
Additionally, all Spectrum generatorNETBOX and hybridNETBOX products with installed DDS feature are supported.
DDS control can program these DDS controls statically with DDS dialogs, or it allows the user to program full sequences (or patterns) of com-
mands in an exact timed manner.
The current DDS control software is free to download from our website, and all the features are available without additional fees. Both Win-
dows and Linux are supported.
C/C++ Driver Interface
C/C++ is the main programming language for which the drivers have been designed for. Therefore the interface to C/C++ is the best match.
All the small examples of the manual showing different parts of the hardware programming are done with C. As the libraries offer a standard
interface it is easy to access the libraries also with other programming languages like Delphi, Basic, Python or Java . Please read the following
chapters for additional information on this.
(c) Spectrum Instrumentation GmbH 61

Software C/C++ Driver Interface
Header files
The basic task before using the driver is to include the header files that are delivered on USB stick together with the board. The header files
are found in the directory /Driver/c_header. Please don’t change them in any way because they are updated with each new driver version
to include the new registers and new functionality.
Table 5: list of C/C++ header files in driver
dlltyp.h Includes the platform specific definitions for data types and function declarations. All data types are based on these definitions. The use of this type definition
file allows the use of examples and programs on different platforms without changes to the program source. The header file supports Microsoft Visual C++, Bor-
land C++ Builder and GNU C/C++ directly. When using other compilers it might be necessary to make a copy of this file and change the data types accord-
ing to this compiler.
regs.h Defines all registers and commands which are used in the Spectrum driver for the different boards. The registers a board uses are described in the board spe-
cific part of the documentation. This header file is common for all cards. Therefore this file also contains a huge number of registers used on other card types
than the one described in this manual. Please stick to the manual to see which registers are valid for your type of card.
spcm_drv.h Defines the functions of the used SpcM driver. All definitions are taken from the file dlltyp.h. The functions themselves are described below.
spcerr.h Contains all error codes used with the Spectrum driver. All error codes that can be given back by any of the driver functions are also described here briefly. The
error codes and their meaning are described in detail in the appendix of this manual.
Example for including the header files:
// ----- driver includes -----
#include "dlltyp.h" // 1st include
#include "regs.h" // 2nd include
#include "spcerr.h" // 3rd include
#include "spcm_drv.h" // 4th include
Please always keep the order of including the four Spectrum header files. Otherwise some or all of the func-
tions do not work properly or compiling your program will be impossible!
General Information on Windows 64 bit drivers
After installation of the Spectrum 64 bit driver there are two general ways to access the hardware and to de-
velop applications. If you’re going to develop a real 64 bit application it is necessary to access the 64 bit
driver dll (spcm_win64.dll) as only this driver dll is supporting the full 64 bit address range.
But it is still possible to run 32 bit applications or to develop 32 bit applications even under Windows 64 bit.
Therefore the 32 bit driver dll (spcm_win32.dll) is also installed in the system. The Spectrum SBench5 software
is for example running under Windows 64 bit using this driver. The 32 bit dll of course only offers the 32 bit
address range and is therefore limited to access only 4 GByte of memory. Beneath both drivers the 64 bit ker-
nel driver is running.
Mixing of 64 bit application with 32 bit dll or vice versa is not possible.
Microsoft Visual C++ 6.0, 2005 and newer 32 Bit
Include Driver
The driver files can be directly included in Microsoft C++ by simply using the library file spcm_win32_msvcpp.lib that is delivered together
with the drivers. The library file can be found on the CD in the path /examples/c_cpp/c_header. Please include the library file in your Visual
C++ project as shown in the examples. All functions described below are now available in your program.
Examples
Examples can be found on CD in the path /examples/c_cpp. This directory includes a number of different examples that can be used with
any card of the same type (e.g. A/D acquisition cards, D/A acquisition cards). You may use these examples as a base for own programming
and modify them as you like. The example directories contain a running workspace file for Microsoft Visual C++ 6.0 (*.dsw) as well as project
files for Microsoft Visual Studio 2005 and newer (*.vcproj) that can be directly loaded or imported and compiled.
There are also some more board type independent examples in separate subdirectory. These examples show different aspects of the cards
like programming options or synchronization and can be combined with one of the board type specific examples.
As the examples are build for a card class there are some checking routines and differentiation between cards families. Differentiation aspects
can be number of channels, data width, maximum speed or other details. It is recommended to change the examples matching your card
type to obtain maximum performance. Please be informed that the examples are made for easy understanding and simple showing of one
aspect of programming. Most of the examples are not optimized for maximum throughput or repetition rates.
Microsoft Visual C++ 2005 and newer 64 Bit
Depending on your version of the Visual Studio suite it may be necessary to install some additional 64 bit components (SDK) on your system.
Please follow the instructions found on the MSDN for further information.
Include Driver
The driver files can be directly included in Microsoft C++ by simply using the library file spcm_win64_msvcpp.lib that is delivered together
with the drivers. The library file can be found on the CD in the path /examples/c_cpp/c_header. All functions described below are now
available in your program.
(c) Spectrum Instrumentation GmbH 62

Software Driver functions
Linux Gnu C/C++ 32/64 Bit
Include Driver
The interface of the linux drivers does not differ from the windows interface. Please include the “libspcm_linux.so” library in your makefile
using the below shown “ ” line, to have access to all driver functions. A makefile may look like this:
LIBS = -lspcm_linux
COMPILER = gcc
EXECUTABLE = test_prg
LIBS = -lspcm_linux
OBJECTS = test.o\
test2.o
all: $(EXECUTABLE)
$(EXECUTABLE): $(OBJECTS)
$(COMPILER) $(CFLAGS) -o $(EXECUTABLE) $(LIBS) $(OBJECTS)
%.o: %.cpp
$(COMPILER) $(CFLAGS) -o $*.o -c $*.cpp
Examples
The Gnu C/C++ examples share the source with the Visual C++ examples. Please see above chapter for a more detailed documentation of
the examples. Each example directory contains a makefile for the Gnu C/C++ examples.
C++ for .NET
Please see the next chapter for more details on the .NET inclusion.
Other Windows C/C++ compilers 32 Bit
Include Driver
To access the driver using a compiler such as e.g. MinGW or Borland, the driver functions must be loaded from the 32bit driver DLL. Most
compilers offer special tools to generate a matching library (e.g. Borland offers the implib tool that generates a matching library out of the
windows driver DLL). If such a tool is available it is recommended to use it. Otherwise the driver functions need to be loaded from the dll
using standard Windows functions. There is one example in the example directory /examples/c_cpp/dll_loading that shows the process.
Example of function loading:
hDLL = LoadLibrary ("spcm_win32.dll"); // Load the 32 bit version of the Spcm driver
pfn_spcm_hOpen = (SPCM_HOPEN*) GetProcAddress (hDLL, "_spcm_hOpen@4");
pfn_spcm_vClose = (SPCM_VCLOSE*) GetProcAddress (hDLL, "_spcm_vClose@4");
Other Windows C/C++ compilers 64 Bit
Include Driver
To access the driver using a compiler such as e.g. MinGW or Borland, the driver functions must be loaded from the 64bit the driver DLL.
Most compilers offer special tools to generate a matching library (e.g. Borland offers the implib tool that generates a matching library out of
the windows driver DLL). If such a tool is available it is recommended to use it. Otherwise the driver functions need to be loaded from the dll
using standard Windows functions. There is one example in the example directory /examples/c_cpp/dll_loading that shows the process for
32bit environments. The only line that needs to be modified is the one loading the DLL:
Example of function loading:
hDLL = LoadLibrary ("spcm_win64.dll"); // Modified: Load the 64 bit version of the Spcm driver here
pfn_spcm_hOpen = (SPCM_HOPEN*) GetProcAddress (hDLL, "spcm_hOpen");
pfn_spcm_vClose = (SPCM_VCLOSE*) GetProcAddress (hDLL, "spcm_vClose");
Driver functions
The driver contains seven main functions to access the hardware.
Own types used by our drivers
To simplify the use of the header files and our examples with different platforms and compilers and to avoid any implicit type conversions we
decided to use our own type declarations. This allows us to use platform independent and universal examples and driver interfaces. If you
do not stick to these declarations please be sure to use the same data type width. However it is strongly recommended that you use our defined
(c) Spectrum Instrumentation GmbH 63

Software Driver functions
type declarations to avoid any hard to find errors in your programs. If you’re using the driver in an environment that is not natively supported
by our examples and drivers, please be sure to use a type declaration that represents a similar data width
Table 6: C/C++ type declarations for drivers and examples
Declaration Type Declaration Type
int8 8 bit signed integer (range from -128 to +127) uint8 8 bit unsigned integer (range from 0 to 255)
int16 16 bit signed integer (range from -32768 to 32767) uint16 16 bit unsigned integer (range from 0 to 65535)
int32 32 bit signed integer (range from -2147483648 to 2147483647) uint32 32 bit unsigned integer (range from 0 to 4294967295)
int64 64 bit signed integer (full range) uint64 64 bit unsigned integer (full range)
drv_handle handle to driver, implementation depends on operating system platform
Notation of variables and functions
In our header files and examples we use a common and reliable form of notation for variables and functions. Each name also contains the
type as a prefix. This notation form makes it easy to see implicit type conversions and minimizes programming errors that result from using
incorrect types. Feel free to use this notation form for your programs as well-
Table 7: C/C++ type naming convention throughout drivers and examples
Declaration Notation Declaration Notation
int8 cName (character) uint8 byName (byte)
int16 nName uint16 wName (word)
int32 lName (long) uint32 dwName (double word)
int64 llName (long long) uint64 qwName (quad word)
float fName double dName
void vName bool bName
int8* pcName (character) uint8* pbyName (byte)
int16* pnName uint16* pwName (word)
int32* plName (long) uint32* pdwName (double word)
int64* pllName (long long) uint64* pqwName (quad word)
float* pfName double* pdName
void* pvName
drv_handle hName char* szName (string with zero termination)
Function spcm_hOpen
This function initializes and opens an installed card supporting the new SpcM driver interface, which at the time of printing, are all cards of
the M2i/M3i/M4i/M4x/M2p/M5i series and the related digitizerNETBOX/generatorNETBOX/hybridNETBOX devices. The function re-
turns a handle that has to be used for driver access. If the card can’t be found or the loading of the driver generated an error the function
returns a NULL. When calling this function all card specific installation parameters are read out from the hardware and stored within the
driver. It is only possible to open one device by one software as concurrent hardware access may be very critical to system stability. As a
result when trying to open the same device twice an error will be raised and the function returns NULL.
Function spcm_hOpen (const char* szDeviceName):
drv_handle _stdcall spcm_hOpen ( // tries to open the device and returns handle or error code
const char* szDeviceName); // name of the device to be opened
Under Linux the device name in the function call needs to be a valid device name. Please change the string according to the location of the
device if you don’t use the standard Linux device names. The driver is installed as default under /dev/spcm0, /dev/spcm1 and so on. The
kernel driver numbers the devices starting with 0.
Under Windows the only part of the device name that is used is the trailing number. The rest of the device name is ignored. Therefore to keep
the examples simple we use the Linux notation in all our examples. The trailing number gives the index of the device to open. The Windows
kernel driver numbers all devices that it finds on boot time starting with 0.
Example for local installed cards
drv_handle hDrv; // returns the handle to the opended driver or NULL in case of error
hDrv = spcm_hOpen ("/dev/spcm0"); // open the first card (spcm0) and get a handle to this card
if (!hDrv)
printf (“open of driver failed\n”);
Example for digitizerNETBOX/generatorNETBOX and remote installed cards
drv_handle hDrv; // returns the handle to the opended driver or NULL in case of error
hDrv = spcm_hOpen ("TCPIP::192.168.169.14::INST0::INSTR");
if (!hDrv)
printf (“open of driver failed\n”);
If the function returns a NULL it is possible to read out the error description of the failed open function by simply passing this NULL to the error
function. The error function is described in one of the next topics.
(c) Spectrum Instrumentation GmbH 64

Software Driver functions
Function spcm_vClose
This function closes the driver and releases all allocated resources. After closing the driver handle it is not possible to access this driver any
more. Be sure to close the driver if you don’t need it any more to allow other programs to get access to this device.
Function spcm_vClose:
void _stdcall spcm_vClose ( // closes the device
drv_handle hDevice); // handle to an already opened device
Example:
spcm_vClose (hDrv);
Function spcm_dwSetParam
All hardware settings are based on software registers that can be set by one of the functions spcm_dwSetParam. These functions set a register
to a defined value or execute a command. The board must first be initialized by the spcm_hOpen function. The parameter lRegister must have
a valid software register constant as defined in regs.h. The available software registers for the driver are listed in the board specific part of
the documentation below. The function returns a 32 bit error code if an error occurs. If no error occurs the function returns ERR_OK, what is
zero.
Function spcm_dwSetParam
uint32 _stdcall spcm_dwSetParam_i32 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be modified
int32 lValue); // the value to be set
uint32 _stdcall spcm_dwSetParam_i64m ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be modified
int32 lValueHigh, // upper 32 bit of the value. Containing the sign bit !
uint32 dwValueLow); // lower 32 bit of the value.
uint32 _stdcall spcm_dwSetParam_i64 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be modified
int64 llValue); // the value to be set
uint32 _stdcall spcm_dwSetParam_d64 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be modified
double dValue); // the value to be set
uint32 _stdcall spcm_dwSetParam_ptr ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be modified
void* pvValue, // pointer to the value to be set
unit64 qwLen); // length of the buffer behind the pvValue
The functions spcm_dwSetParam_d64 and spcm_dwSetParam_ptr have been added with driver release V 7.00
Example:
if (spcm_dwSetParam_i64 (hDrv, SPC_MEMSIZE, 16384) != ERR_OK)
printf (“Error when setting memory size\n”);
This example sets the memory size to 16 KiSamples (16384). If an error occurred the example will show a short error message
Function spcm_dwGetParam
All hardware settings are based on software registers that can be read by one of the functions spcm_dwGetParam. These functions read an
internal register or status information. The board must first be initialized by the spcm_hOpen function. The parameter lRegister must have a
valid software register constant as defined in the regs.h file. The available software registers for the driver are listed in the board specific part
of the documentation below. The function returns a 32 bit error code if an error occurs. If no error occurs the function returns ERR_OK, what
is zero.
(c) Spectrum Instrumentation GmbH 65

Software Driver functions
Function spcm_dwGetParam
uint32 _stdcall spcm_dwGetParam_i32 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be read out
int32* plValue); // pointer for the return value
uint32 _stdcall spcm_dwGetParam_i64m ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be read out
int32* plValueHigh, // pointer for the upper part of the return value
uint32* pdwValueLow); // pointer for the lower part of the return value
uint32 _stdcall spcm_dwGetParam_i64 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be read out
int64* pllValue); // pointer for the return value
uint32 _stdcall spcm_dwGetParam_d64 ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be modified
double* dValue); // pointer for the return value
uint32 _stdcall spcm_dwGetParam_ptr ( // Return value is an error code
drv_handle hDevice, // handle to an already opened device
int32 lRegister, // software register to be modified
void* pvValue, // pointer for the return value
unit64 qwLen); // length of the buffer behind the pvValue
The functions spcm_dwGetParam_d64 and spcm_dwGetParam_ptr have been added with driver release V 7.00
Example:
int32 lSerialNumber;
spcm_dwGetParam_i32 (hDrv, SPC_PCISERIALNO, &lSerialNumber);
printf (“Your card has serial number: %05d\n”, lSerialNumber);
The example reads out the serial number of the installed card and prints it. As the serial number is available under all circumstances there is
no error checking when calling this function.
Different call types of spcm_dwSetParam and spcm_dwGetParam: _i32, _i64, _i64m, d64
The four functions only differ in the type of the parameters that are used to call them. As some of the registers can exceed the 32 bit integer
range (like memory size or post trigger) it is recommended to use the _i64 function to access these registers. However as there are some
programs or compilers that don’t support 64 bit integer variables there are two functions that are limited to 32 bit integer variables. In case
that you do not access registers that exceed 32 bit integer please use the _i32 function. In case that you access a register which exceeds 64
bit value please use the _i64m calling convention. Inhere the 64 bit value is split into a low double word part and a high double word part.
Please be sure to fill both parts with valid information.
As some registers need to be read/written in double precision and can’t be read/written as integer values, two additional new functions for
accessing double values have been added with the suffix _d64.
If accessing 64 bit registers with 32 bit functions the behaviour differs depending on the real value that is currently located in the register.
Please have a look at this table to see the different reactions depending on the size of the register:
Table 8: Spectrum driver API functions overview and differentiation between 32 bit and 64 bit registers
Internal register read/write Function type Behavior
32 bit register read spcm_dwGetParam_i32 value is returned as 32 bit integer in plValue
32 bit register read spcm_dwGetParam_i64 value is returned as 64 bit integer in pllValue
32 bit register read spcm_dwGetParam_i64m value is returned as 64 bit integer, the lower part in plValueLow, the upper part in plValueHigh. The upper part can
be ignored as it’s only a sign extension
32 bit register read spcm_dwGetParam_d64 value is returned as 64 bit double in pdValue
32 bit register write spcm_dwSetParam_i32 32 bit value can be directly written
32 bit register write spcm_dwSetParam_i64 64 bit value can be directly written, please be sure not to exceed the valid register value range
32 bit register write spcm_dwSetParam_i64m 32 bit value is written as llValueLow, the value llValueHigh needs to contain the sign extension of this value. In case
of llValueLow being a value >= 0 llValueHigh can be 0, in case of llValueLow being a value < 0, llValueHigh has to
be -1.
32 bit register write spcm_dwSetParam_d64 32 bit value needs to converted to double. Please make sure no to exceed the valid register range
64 bit register read spcm_dwGetParam_i32 If the internal register has a value that is inside the 32 bit integer range (-2Gi up to (2Gi - 1)) the value is returned
normally. If the internal register exceeds this size an error code ERR_EXCEEDSINT32 is returned. As an example:
reading back the installed memory will work as long as this memory is < 2 GiByte. If the installed memory is >= 2
GiByte the function will return an error.
64 bit register read spcm_dwGetParam_i64 value is returned as 64 bit integer value in pllValue independent of the value of the internal register.
64 bit register read spcm_dwGetParam_i64m the internal value is split into a low and a high part. As long as the internal value is within the 32 bit range, the low
part plValueLow contains the 32 bit value and the upper part plValueHigh can be ignored. If the internal value
exceeds the 32 bit range it is absolutely necessary to take both value parts into account.
64 bit register read spcm_dwGetParam_d64 value is returned as 64 bit double in pdValue. Please note that double values are limited to 2^48. Any larger value
is not returned with full precision.
64 bit register write spcm_dwSetParam_i32 the value to be written is limited to 32 bit range. If a value higher than the 32 bit range should be written, one of
the other function types need to used.
(c) Spectrum Instrumentation GmbH 66

Software Driver functions
Table 8: Spectrum driver API functions overview and differentiation between 32 bit and 64 bit registers
Internal register read/write Function type Behavior
64 bit register write spcm_dwSetParam_i64 the value has to be split into two parts. Be sure to fill the upper part lValueHigh with the correct sign extension even
if you only write a 32 bit value as the driver every time interprets both parts of the function call.
64 bit register write spcm_dwSetParam_i64m the value can be written directly independent of the size.
64 bit register write spcm_dwSetParam_d64 the value need to be converted to double. Any value up to 2^48 can be written directly. Larger values need to be
written using the _i64 function
Function spcm_dwGetContBuf
This function reads out the internal continuous memory buffer in bytes, in case one has been allocated. If no buffer has been allocated the
function returns a size of zero and a NULL pointer. You may use this buffer for data transfers. As the buffer is continuously allocated in memory
the data transfer will speed up by up to 15% - 25%, depending on your specific kind of card. Please see further details in the appendix of
this manual.
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
These functions have been added in driver version 1.36. The functions are not available in older driver ver-
sions.
These functions also only have effect on locally installed cards and are neither useful nor usable with any
digitizerNETBOX or generatorNETBOX products, because no local kernel driver is involved in such a setup.
For remote devices these functions will return a NULL pointer for the buffer and 0 Bytes in length.
Function spcm_dwDefTransfer
The spcm_dwDefTransfer function defines a buffer for a following data transfer. This function only defines the buffer, there is no data transfer
performed when calling this function. Instead the data transfer is started with separate register commands that are documented in a later
chapter. At this position there is also a detailed description of the function parameters.
Please make sure that all parameters of this function match. It is especially necessary that the buffer address is a valid address pointing to
memory buffer that has at least the size that is defined in the function call. Please be informed that calling this function with non valid param-
eters may crash your system as these values are base for following DMA transfers.
The use of this function is described in greater detail in a later chapter.
Function spcm_dwDefTransfer
uint32 _stdcall spcm_dwDefTransfer_i64m(// Defines the transfer buffer by 2 x 32 bit unsigned integer
drv_handle hDevice, // handle to an already opened device
uint32 dwBufType, // type of the buffer to define as listed above under SPCM_BUF_XXXX
uint32 dwDirection, // the transfer direction as defined above
uint32 dwNotifySize, // no. of bytes after which an event is sent (0=end of transfer)
void* pvDataBuffer, // pointer to the data buffer
uint32 dwBrdOffsH, // high part of offset in board memory (zero when using FIFO mode)
uint32 dwBrdOffsL, // low part of offset in board memory (zero when using FIFO mode)
uint32 dwTransferLenH, // high part of transfer buffer length
uint32 dwTransferLenL); // low part of transfer buffer length
uint32 _stdcall spcm_dwDefTransfer_i64 (// Defines the transfer buffer by using 64 bit unsigned integer values
drv_handle hDevice, // handle to an already opened device
uint32 dwBufType, // type of the buffer to define as listed above under SPCM_BUF_XXXX
uint32 dwDirection, // the transfer direction as defined above
uint32 dwNotifySize, // no. of bytes after which an event is sent (0=end of transfer)
void* pvDataBuffer, // pointer to the data buffer
uint64 qwBrdOffs, // offset for transfer in board memory (zero when using FIFO mode)
uint64 qwTransferLen); // buffer length
This function is available in two different formats as the spcm_dwGetParam and spcm_dwSetParam functions are. The background is the
same. As long as you’re using a compiler that supports 64 bit integer values please use the _i64 function. Any other platform needs to use
the _i64m function and split offset and length in two 32 bit words.
Example:
int16* pnBuffer = (int16*) pvAllocMemPageAligned (16384);
if (spcm_dwDefTransfer_i64 (hDrv, SPCM_BUF_DATA, SPCM_DIR_CARDTOPC, 0, (void*) pnBuffer, 0, 16384) != ERR_OK)
printf (“DefTransfer failed\n”);
(c) Spectrum Instrumentation GmbH 67

Software
The example defines a data buffer of 8 KiSamples of 16 bit integer values = 16 KiByte (16384 byte) for a transfer from card to PC memory.
As notify size is set to 0 we only want to get an event when the transfer has finished.
Function spcm_dwInvalidateBuf
The invalidate buffer function is used to tell the driver that the buffer that has been set with spcm_dwDefTransfer call is no longer valid. It is
necessary to use the same buffer type as the driver handles different buffers at the same time. Call this function if you want to delete the buffer
memory after calling the spcm_dwDefTransfer function. If the buffer already has been transferred after calling spcm_dwDefTransfer it is not
necessary to call this function. When calling spcm_dwDefTransfer any previously defined buffer of this type is automatically invalidated.
Function spcm_dwInvalidateBuf
uint32 _stdcall spcm_dwInvalidateBuf ( // invalidate the transfer buffer
drv_handle hDevice, // handle to an already opened device
uint32 dwBufType); // type of the buffer to invalidate as
// listed above under SPCM_BUF_XXXX
Function spcm_dwGetErrorInfo
The function returns complete error information on the last error that has occurred. The error handling itself is explained in a later chapter in
greater detail. When calling this function please be sure to have a text buffer allocated that has at least ERRORTEXTLEN length. The error text
function returns a complete description of the error including the register/value combination that has raised the error and a short description
of the error details. In addition it is possible to get back the error generating register/value for own error handling. If not needed the buffers
for register/value can be left to NULL.
Note that the timeout event (ERR_TIMEOUT) is not counted as an error internally as it is not locking the driver
but as a valid event. Therefore the GetErrorInfo function won’t return the timeout event even if it had occurred
in between. You can only recognize the ERR_TIMEOUT as a direct return value of the wait function that was
called.
Function spcm_dwGetErrorInfo
// for reading errors that occur during hOpen(), leave the drv_handle parameter NULL
uint32 _stdcall spcm_dwGetErrorInfo_i32 (
drv_handle hDevice, // handle to an already opened device
uint32* pdwErrorReg, // address of the error register (can be NULL if not of interest)
int32* plErrorValue, // address of the error value (can be NULL if not of interest)
char pszErrorTextBuffer[ERRORTEXTLEN]); // text buffer for text error
uint32 _stdcall spcm_dwGetErrorInfo_i64 (
drv_handle hDevice, // handle to an already opened device
uint32* pdwErrorReg, // address of the error register (can be NULL if not of interest)
int64* pllErrorValue, // address of the error value (can be NULL if not of interest)
char pszErrorTextBuffer[ERRORTEXTLEN]); // text buffer for text error
uint32 _stdcall spcm_dwGetErrorInfo_d64 (
drv_handle hDevice, // handle to an already opened device
uint32* pdwErrorReg, // address of the error register (can be NULL if not of interest)
double* pdErrorValue, // address of the error value (can be NULL if not of interest)
char pszErrorTextBuffer[ERRORTEXTLEN]); // text buffer for text error
The functions spcm_dwGetErrorInfo_i64 and spcm_dwGetErrorInfo_d64 have been added with driver release V 7.00
Example:
char szErrorBuf[ERRORTEXTLEN];
if (spcm_dwSetParam_i64 (hDrv, SPC_MEMSIZE, -1))
{
spcm_dwGetErrorInfo_i64 (hDrv, NULL, NULL, szErrorBuf);
printf (“Set of memsize failed with error message: %s\n”, szErrorBuf);
}
Delphi (Pascal) Programming Interface
Driver interface
The driver interface is located in the sub-directory d_header and contains the following files. The files need to be included in the delphi project
and have to be put into the „uses“ section of the source files that will access the driver. Please do not edit any of these files as they’re regularly
updated if new functions or registers have been included.
(c) Spectrum Instrumentation GmbH 68

Software Delphi (Pascal) Programming Interface
file spcm_win32.pas
The file contains the interface to the driver library and defines some needed constants and variable types. All functions of the delphi library
are similar to the above explained standard driver functions:
// ----- device handling functions -----
function spcm_hOpen (strName: pchar): int32; stdcall; external 'spcm_win32.dll' name '_spcm_hOpen@4';
procedure spcm_vClose (hDevice: int32); stdcall; external 'spcm_win32.dll' name '_spcm_vClose@4';
function spcm_dwGetErrorInfo_i32 (hDevice: int32; var lErrorReg, lErrorValue: int32; strError: pchar): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwGetErrorInfo_i32@16'
function spcm_dwGetErrorInfo_i64 (hDevice: int32; var plErrorReg: int32; var pllErrorValue: int64; strError:
PAnsiChar): uint32; stdcall; external 'spcm_win32.dll' name '_spcm_dwGetErrorInfo_i64@16'
function spcm_dwGetErrorInfo_d64 (hDevice: int32; var plErrorReg: int32; var pdErrorValue: double; strError:
PAnsiChar): uint32; stdcall; external 'spcm_win32.dll' name '_spcm_dwGetErrorInfo_d64@16'
// ----- register access functions -----
function spcm_dwSetParam_i32 (hDevice, lRegister, lValue: int32): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwSetParam_i32@12';
function spcm_dwSetParam_i64 (hDevice, lRegister: int32; llValue: int64): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwSetParam_i64@16';
function spcm_dwSetParam_d64 (hDevice, lRegister: int32; dValue: double): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwSetParam_d64@16';
function spcm_dwGetParam_i32 (hDevice, lRegister: int32; var plValue: int32): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwGetParam_i32@12';
function spcm_dwGetParam_i64 (hDevice, lRegister: int32; var pllValue: int64): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwGetParam_i64@12';
function spcm_dwGetParam_d64 (hDevice, lRegister: int32; var pdValue: double): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwGetParam_d64@12';
// ----- data handling -----
function spcm_dwDefTransfer_i64 (hDevice, dwBufType, dwDirection, dwNotifySize: int32; pvDataBuffer: Pointer;
llBrdOffs, llTransferLen: int64): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwDefTransfer_i64@36';
function spcm_dwInvalidateBuf (hDevice, lBuffer: int32): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwInvalidateBuf@8';
The file also defines types used inside the driver and the examples. The types have similar names as used under C/C++ to keep the examples
more simple to understand and allow a better comparison.
(c) Spectrum Instrumentation GmbH 69

Software Delphi (Pascal) Programming Interface
file spcm_win64.pas
The file contains the interface to the driver library and defines some needed constants and variable types. All functions of the delphi library
are similar to the above explained standard driver functions:
// ----- device handling functions -----
function spcm_hOpen (strName: pchar): int32; stdcall; external 'spcm_win32.dll' name '_spcm_hOpen@4';
procedure spcm_vClose (hDevice: int32); stdcall; external 'spcm_win32.dll' name '_spcm_vClose@4';
function spcm_dwGetErrorInfo_i32 (hDevice: int32; var lErrorReg, lErrorValue: int32; strError: pchar): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwGetErrorInfo_i32@16'
function spcm_dwGetErrorInfo_i64 (hDevice: int32; var plErrorReg: int32; var pllErrorValue: int64; strError:
PAnsiChar): uint32; stdcall; external 'spcm_win32.dll' name '_spcm_dwGetErrorInfo_i64@16'
function spcm_dwGetErrorInfo_d64 (hDevice: int32; var plErrorReg: int32; var pdErrorValue: double; strError:
PAnsiChar): uint32; stdcall; external 'spcm_win32.dll' name '_spcm_dwGetErrorInfo_d64@16'
// ----- register access functions -----
function spcm_dwSetParam_i32 (hDevice, lRegister, lValue: int32): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwSetParam_i32@12';
function spcm_dwSetParam_i64 (hDevice, lRegister: int32; llValue: int64): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwSetParam_i64@16';
function spcm_dwSetParam_d64 (hDevice, lRegister: int32; dValue: double): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwSetParam_d64@16';
function spcm_dwGetParam_i32 (hDevice, lRegister: int32; var plValue: int32): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwGetParam_i32@12';
function spcm_dwGetParam_i64 (hDevice, lRegister: int32; var pllValue: int64): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwGetParam_i64@12';
function spcm_dwGetParam_d64 (hDevice, lRegister: int32; var pdValue: double): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwGetParam_d64@12';
// ----- data handling -----
function spcm_dwDefTransfer_i64 (hDevice, dwBufType, dwDirection, dwNotifySize: int32; pvDataBuffer: Pointer;
llBrdOffs, llTransferLen: int64): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwDefTransfer_i64@36';
function spcm_dwInvalidateBuf (hDevice, lBuffer: int32): uint32;
stdcall; external 'spcm_win32.dll' name '_spcm_dwInvalidateBuf@8';
file SpcRegs.pas
The SpcRegs.pas file defines all constants that are used for the driver. The constant names are the same names as used under the C/C++
examples. All constants names will be found throughout this hardware manual when certain aspects of the driver usage are explained. It is
recommended to only use these constant names for better visibility of the programs:
const SPC_M2CMD = 100; { write a command }
const M2CMD_CARD_RESET = $00000001; { hardware reset }
const M2CMD_CARD_WRITESETUP = $00000002; { write setup only }
const M2CMD_CARD_START = $00000004; { start of card (including writesetup) }
const M2CMD_CARD_ENABLETRIGGER = $00000008; { enable trigger engine }
...
file SpcErr.pas
The SpeErr.pas file contains all error codes that may be returned by the driver.
Including the driver files
To use the driver function and all the defined constants it is necessary to include the files into the project as
shown in the picture on the right. The project overview is taken from one of the examples delivered on the
USB stick. Besides including the driver files in the project it is also necessary to include them in the uses
section of the source files where functions or constants should be used:
uses
Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
StdCtrls, ExtCtrls,
SpcRegs, SpcErr, spcm_win32;
Examples Image 42: Structure of the Delphi ex-
maples
Examples for Delphi can be found on the USB stick in the directory /examples/delphi. The directory contains
the above mentioned delphi header files and a couple of universal examples, each of them working with a certain type of card. Please feel
free to use these examples as a base for your programs and to modify them in any kind.
(c) Spectrum Instrumentation GmbH 70

Software .NET programming languages
spcm_scope
The example implements a very simple scope program that makes single acquisitions on button pressing. A fixed setup is done inside the
example. The spcm_scope example can be used with any analog data acquisition card from Spectrum. It covers cards with 1 byte per sample
(8 bit resolution) as well as cards with 2 bytes per sample (12, 14 and 16 bit resolution)
The program shows the following steps:
• Initialization of a card and reading of card information like type, function and serial number
• Doing a simple card setup
• Performing the acquisition and waiting for the end interrupt
• Reading of data, re-scaling it and displaying waveform on screen
.NET programming languages
Library
For using the driver with a .NET based language Spectrum delivers a special library that encapsulates the driver in a .NET object. By adding
this object to the project it is possible to access all driver functions and constants from within your .NET environment.
There is one small console based example for each supported .NET language that shows how to include the driver and how to access the
cards. Please combine this example with the different standard examples to get the different card functionality.
Declaration
The driver access methods and also all the type, register and error declarations are combined in the object Spcm and are located in one of
the two DLLs either SpcmDrv32.NET.dll or SpcmDrv64.NET.dll delivered with the .NET examples.
For simplicity, either file is simply called „SpcmDrv.NET.dll“ in the following passages and the actual file
name must be replaced with either the 32bit or 64bit version according to your application.
Spectrum also delivers the source code of the DLLs as a C# project. These sources are located in the directory SpcmDrv.NET.
namespace Spcm
{
public class Drv
{
[DllImport("spcm_win32.dll")]public static extern IntPtr spcm_hOpen (string szDeviceName);
[DllImport("spcm_win32.dll")]public static extern void spcm_vClose (IntPtr hDevice);
...
public class CardType
{
public const int TYP_M2I2020 = unchecked ((int)0x00032020);
public const int TYP_M2I2021 = unchecked ((int)0x00032021);
public const int TYP_M2I2025 = unchecked ((int)0x00032025);
...
public class Regs
{
public const int SPC_M2CMD = unchecked ((int)100);
public const int M2CMD_CARD_RESET = unchecked ((int)0x00000001);
public const int M2CMD_CARD_WRITESETUP = unchecked ((int)0x00000002);
...
Using C#
The SpcmDrv.NET.dll needs to be included within the Solution Explorer in the References section. Please use right mouse and select
„AddReference“. After this all functions and constants of the driver object are available.
Please see the example in the directory CSharp as a start:
// ----- open card -----
hDevice = Drv.spcm_hOpen("/dev/spcm0");
if ((int)hDevice == 0)
{
Console.WriteLine("Error: Could not open card\n");
return 1;
}
// ----- get card type -----
dwErrorCode = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_PCITYP, out lCardType);
dwErrorCode = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_PCISERIALNR, out lSerialNumber);
(c) Spectrum Instrumentation GmbH 71

Software .NET programming languages
Example for digitizerNETBOX/generatorNETBOX and remotely installed cards:
// ----- open remote card -----
hDevice = Drv.spcm_hOpen("TCPIP::192.168.169.14::INST0::INSTR");
(c) Spectrum Instrumentation GmbH 72

Software .NET programming languages
Using Managed C++/CLI
The SpcmDrv.NET.dll needs to be included within the project options. Please select „Project“ - „Properties“ - „References“ and finally
„Add new Reference“. After this all functions and constants of the driver object are available.
Please see the example in the directory CppCLR as a start:
// ----- open card -----
hDevice = Drv::spcm_hOpen("/dev/spcm0");
if ((int)hDevice == 0)
{
Console::WriteLine("Error: Could not open card\n");
return 1;
}
// ----- get card type -----
dwErrorCode = Drv::spcm_dwGetParam_i32(hDevice, Regs::SPC_PCITYP, lCardType);
dwErrorCode = Drv::spcm_dwGetParam_i32(hDevice, Regs::SPC_PCISERIALNR, lSerialNumber);
Example for digitizerNETBOX/generatorNETBOX and remotely installed cards:
// ----- open remote card -----
hDevice = Drv::spcm_hOpen("TCPIP::192.168.169.14::INST0::INSTR");
Using VB.NET
The SpcmDrv.NET.dll needs to be included within the project options. Please select „Project“ - „Properties“ - „References“ and finally
„Add new Reference“. After this all functions and constants of the driver object are available.
Please see the example in the directory VB.NET as a start:
' ----- open card -----
hDevice = Drv.spcm_hOpen("/dev/spcm0")
If (hDevice = 0) Then
Console.WriteLine("Error: Could not open card\n")
Else
' ----- get card type -----
dwError = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_PCITYP, lCardType)
dwError = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_PCISERIALNR, lSerialNumber)
Example for digitizerNETBOX/generatorNETBOX and remotely installed cards:
' ----- open remote card -----
hDevice = Drv.spcm_hOpen("TCPIP::192.168.169.14::INST0::INSTR")
Using J#
The SpcmDrv.NET.dll needs to be included within the Solution Explorer in the References section. Please use right mouse and select „AddRef-
erence“. After this all functions and constants of the driver object are available.
Please see the example in the directory JSharp as a start:
// ----- open card -----
hDevice = Drv.spcm_hOpen("/dev/spcm0");
if (hDevice.ToInt32() == 0)
System.out.println("Error: Could not open card\n");
else
{
// ----- get card type -----
dwErrorCode = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_PCITYP, lCardType);
dwErrorCode = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_PCISERIALNR, lSerialNumber);
Example for digitizerNETBOX/generatorNETBOX and remotely installed cards:
' ----- open remote card -----
hDevice = Drv.spcm_hOpen("TCPIP::192.168.169.14::INST0::INSTR")
(c) Spectrum Instrumentation GmbH 73

Software Python Programming Interface and Examples
Python Programming Interface and Examples
The Spectrum Instrumentation API has extensive support for Python. There is an implementation for both low- and high-level Python program-
ming. The low-level API is closely related to C and optimized for speed. The high-level general-purpose object-oriented API is located on
Github and available through the pip repository and optimized for usability and learning how to use the cards.
High-level object oriented Python package spcm
Build on top of the low-level Python API, there is a general purpose high-level object oriented Python package. This spcm package is available
through the PIP repository and is easily installed using the straight-forward installation command:
pip install spcm
The package automatically handles correct opening and closing of cards and Netbox devices, including sets of synchronizes cards. It also
implements error handling through exceptions.
Moreover, the package handles allocation of memory and provides a direct interface to numpy and support for physical quantities and units
through pint. A multitude of card functionalities are implemented in easy-to-use classes and examples. The spcm package can be found here:
• Spectrum Instrumentation Python package on GitHub: https://github.com/SpectrumInstrumentation/spcm
• Spectrum Instrumentation Python examples on GitHub: https://github.com/SpectrumInstrumentation/spcm/tree/master/src/examples
• Spectrum Instrumentation Python repository on GitHub: https://pypi.org/project/spcm/
• Spectrum Instrumentation Python reference documentation on GitHub: https://spectruminstrumentation.github.io/spcm/spcm.html
The spcm is free-to-use and freely available to all Spectrum Instrumentation card users under the MIT-license. The package is only available
for Python 3 starting with version 3.9.
Low-level driver interface
The driver interface contains the following files. The files need to be included in the python project. Please do not edit any of these files as
they are regularly updated if new functions or registers have been included. To use pyspcm you need either python 2 (2.4, 2.6 or 2.7) or
python 3 (3.x) and ctypes, which is included in python 2.6 and newer and needs to be installed separately for Python 2.4.
file pyspcm.py
The file contains the interface to the driver library and defines some needed constants. All functions of the python library are similar to the
above explained standard driver functions and use ctypes as input and return parameters:
# ----- Windows -----
# Load DLL into memory.
# use windll because all driver access functions use _stdcall calling convention under windows
if (bIs64Bit == 1):
spcmDll = windll.LoadLibrary ("spcm_win64.dll")
else:
spcmDll = windll.LoadLibrary ("spcm_win32.dll")
# load spcm_hOpen
if (bIs64Bit):
spcm_hOpen = getattr(spcmDll, "spcm_hOpen")
else:
spcm_hOpen = getattr(spcmDll, "_spcm_hOpen@4")
spcm_hOpen.argtype = [c_char_p]
spcm_hOpen.restype = drv_handle
# load spcm_vClose
if (bIs64Bit):
spcm_vClose = getattr(spcmDll, "spcm_vClose")
else:
spcm_vClose = getattr(spcmDll, "_spcm_vClose@4")
spcm_vClose.argtype = [drv_handle]
spcm_vClose.restype = None
# load spcm_dwGetErrorInfo_i32
if (bIs64Bit):
spcm_dwGetErrorInfo_i32 = getattr(spcmDll, "spcm_dwGetErrorInfo_i32")
else:
spcm_dwGetErrorInfo_i32 = getattr(spcmDll, "_spcm_dwGetErrorInfo_i32@16")
spcm_dwGetErrorInfo_i32.argtype = [drv_handle, uptr32, ptr32, c_char_p]
spcm_dwGetErrorInfo_i32.restype = uint32
...
(c) Spectrum Instrumentation GmbH 74

Software Java Programming Interface and Examples
file regs.py
The regs.py file defines all constants that are used for the driver. The constant names are the same names compared to the C/C++ examples.
All constant names will be found throughout this hardware manual when certain aspects of the driver usage are explained. It is recommended
to only use these constant names for better readability of the programs:
SPC_M2CMD = 100l # write a command
M2CMD_CARD_RESET = 0x00000001l # hardware reset
M2CMD_CARD_WRITESETUP = 0x00000002l # write setup only
M2CMD_CARD_START = 0x00000004l # start of card (including writesetup)
M2CMD_CARD_ENABLETRIGGER = 0x00000008l # enable trigger engine
...
file spcerr.py
The spcerr.py file contains all error codes that may be returned by the driver.
Examples
Examples for Python can be found on the USB stick in the directory /examples/python. The directory contains the above mentioned header
files and some examples, each of them working with a certain type of card. Please feel free to use these examples as a base for your programs
and to modify them in any kind.
When allocating the buffer for DMA transfers, use the following function to get a mutable character buffer:
ctypes.create_string_buffer(init_or_size[, size])
Java Programming Interface and Examples
Driver interface
The driver interface contains the following Java files (classes). The files need to be included in your Java project. Please do not edit any of
these files as they are regularly updated if new functions or registers have been included. The driver interface uses the Java Native Access
(JNA) library.
This library is licensed under the LGPL (https://www.gnu.org/licenses/lgpl-3.0.en.html) and has also to be included to your Java project.
To download the latest jna.jar package and to get more information about the JNA project please check the projects GitHub page under:
https://github.com/java-native-access/jna
The following files can be found in the „SpcmDrv“ folder of your Java examples install path.
SpcmDrv32.java / SpcmDrv64.java
The files contain the interface to the driver library and defines some needed constants. All functions of the driver interface are similar to the
above explained standard driver functions. Use the SpcmDrv32.java for 32 bit and the SpcmDrv64.java for 64 bit projects:
...
public interface SpcmWin64 extends StdCallLibrary {
SpcmWin64 INSTANCE = (SpcmWin64)Native.loadLibrary (("spcm_win64"), SpcmWin64.class);
long spcm_hOpen (String sDeviceName);
void spcm_vClose (long hDevice);
int spcm_dwSetParam_i64 (long hDevice, int lRegister, long llValue);
int spcm_dwGetParam_i64 (long hDevice, int lRegister, LongByReference pllValue);
int spcm_dwSetParam_ptr (long hDevice, int lRegister, Pointer pValue, long llLen);
int spcm_dwGetParam_ptr (long hDevice, int lRegister, Pointer pValue, long llLen);
int spcm_dwSetParam_d64 (int hDevice, int lRegister, double dValue);
int spcm_dwGetParam_d64 (int hDevice, int lRegister, DoubleByReference pdValue);
int spcm_dwDefTransfer_i64 (long hDevice, int lBufType, int lDirection, int lNotifySize, Pointer pDataBuffer,
long llBrdOffs, long llTransferLen);
int spcm_dwInvalidateBuf (long hDevice, int lBufType);
-
int spcm_dwGetErrorInfo_i32 (long hDevice, IntByReference plErrorReg, IntByReference plErrorValue, Pointer sEr
rorTextBuffer);
int spcm_dwGetErrorInfo_i64 (long hDevice, IntByReference plErrorReg, LongByReference pllErrorValue, Pointer
sErrorTextBuffer);
int spcm_dwGetErrorInfo_d64 (long hDevice, IntByReference plErrorReg, DoubleByReference pdErrorValue, Pointer
sErrorTextBuffer);
}
...
(c) Spectrum Instrumentation GmbH 75

Software Julia Programming Interface and Examples
SpcmRegs.java
The SpcmRegs class defines all constants that are used for the driver. The constants names are the same names compared to the C/C++
examples. All constant names will be found throughout this hardware manual when certain aspects of the driver usage are explained. It is
recommended to only use these constant names for better readability of the programs:
...
public static final int SPC_M2CMD = 100;
public static final int M2CMD_CARD_RESET = 0x00000001;
public static final int M2CMD_CARD_WRITESETUP = 0x00000002;
public static final int M2CMD_CARD_START = 0x00000004;
public static final int M2CMD_CARD_ENABLETRIGGER = 0x00000008;
...
SpcmErrors.java
The SpcmErrors class contains all error codes that may be returned by the driver.
Examples
Examples for Java can be found on the USB stick in the directory /examples/java. The directory contains the above mentioned header files
and some examples, each of them working with a certain type of card. Please feel free to use these examples as a base for your programs
and to modify them in any kind.
Julia Programming Interface and Examples
Driver interface
The driver interface contains the following files. The files need to be included in the julia project. Please do not edit any of these files as they
are regularly updated if new functions or registers have been included.
file spcm_drv.jl
The file contains the interface to the driver library and defines some needed constants. All functions of the Julia library are similar to the above
explained standard driver functions.
hDevice::Int64 = spcm_hOpen(sDeviceName::String)
Cvoid spcm_vClose(hDevice::Int64)
dwErr::UInt32, lValue::Int32 = spcm_dwGetParam_i32(hDevice::Int64, lRegister::Int32)
dwErr::UInt32, llValue::Int64 = spcm_dwGetParam_i64(hDevice::Int64, lRegister::Int32)
dwErr::UInt32, dValue::Float64 = spcm_dwGetParam_d64(hDevice::Int64, lRegister::Int32)
dwErr::UInt32 = spcm_dwSetParam_i32(hDevice::Int64, lRegister::Int32 ,lValue::Int32)
dwErr::UInt32 = spcm_dwSetParam_i64(hDevice::Int64, lRegister::Int32, llValue::Int64)
dwErr::UInt32 = spcm_dwSetParam_d64(hDevice::Int64, lRegister::Int32, dValue::Float64)
dwErr::UInt32 = spcm_dwDefTransfer_i64(hDevice::Int64, lBufType::Int32, lDirection::Int32,
dwNotifySize::UInt32, pDataBuffer::Array{Int16,1},
qwBrdOffs::UInt64, qwTransferLen::UInt64)
dwErr::UInt32 = spcm_dwDefTransfer_i64(hDevice::Int64, lBufType::Int32, lDirection::Int32,
dwNotifySize::UInt32, pDataBuffer::Array{Int8,1},
qwBrdOffs::UInt64, qwTransferLen::UInt64)
dwErr::UInt32 = spcm_dwInvalidateBuf(hDevice::Int64, lBufType::Int32)
dwErr::UInt32, dwErrReg::UInt32, lErrVal::Int32, sErrText::String = spcm_dwGetErrorInfo_i32(hDevice::Int64)
dwErr::UInt32, dwErrReg::UInt32, llErrVal::Int64, sErrText::String = spcm_dwGetErrorInfo_i64(hDevice::Int64)
dwErr::UInt32, dwErrReg::UInt32, dErrVal::Float64, sErrText::String = spcm_dwGetErrorInfo_d64(hDevice::Int64)
file regs.jl
The regs.jl file defines all constants that are used for the driver. The constant names are the same names compared to the C/C++ examples.
All constant names will be found throughout this hardware manual when certain aspects of the driver usage are explained. It is recommended
to only use these constant names for better readability of the programs:
const SPC_M2CMD = Int32(100) # write a command
const M2CMD_CARD_RESET = Int32(1) # 0x00000001 # hardware reset
const M2CMD_CARD_WRITESETUP = Int32(2) # 0x00000002 # write setup only
const M2CMD_CARD_START = Int32(4) # 0x00000004 # start of card (including writesetup)
const M2CMD_CARD_ENABLETRIGGER = Int32(8) # 0x00000008 # enable trigger engine
# ...
(c) Spectrum Instrumentation GmbH 76

Software LabVIEW driver and examples
file spcerr.jl
The spcerr.jl file contains all error codes that may be returned by the driver.
Examples
Examples for Julia can be found on USB-Stick in the directory /examples/julia. The directory contains the above mentioned include files and
some examples, each of them working with a certain type of card. Please feel free to use these examples as a base for your programs and
to modify them in any kind.
LabVIEW driver and examples
A full set of drivers and examples is available
for LabVIEW for Windows. LabVIEW for Linux
is currently not supported. The LabVIEW driv-
ers have their own manual. The LabVIEW driv-
ers, examples and the manual are found on
the USB stick that has been included in the de-
livery. The latest version is also available on
our webpage www.spectrum-instrumenta-
tion.com
Please follow the description in the LabVIEW
manual for installation and usage of the Lab-
VIEW drivers for this card.
Image 43: LabVIEW driver oscilloscope example
MATLAB driver and examples
A full set of drivers and examples is available for Mathworks MATLAB for Windows (32 bit
and 64 bit versions) and also for MATLAB for Linux (64 bit version). There is no additional
toolbox needed to run the MATLAB examples and drivers.
The MATLAB drivers have their own manual. The MATLAB drivers, examples and the manual
are found on the USB stick that has been included in the delivery. The latest version is also
available on our webpage www.spectrum-instrumentation.com
Please follow the description in the MATLAB manual for installation and useage of the
MATLAB drivers for this card.
Image 44: Spectrum MATLAB driver structure
(c) Spectrum Instrumentation GmbH 77

Software SCAPP – CUDA GPU based data processing
SCAPP – CUDA GPU based data processing
Spectrum’s CUDA Access for Parallel Processing
Modern GPUs (Graphic Processing Units) are de-
signed to handle a large number of parallel opera-
tions. While a CPU offers only a few cores for
parallel calculations, a GPU can offer thousands of
cores. This computing capabilities can be used for
calculations using the Nvidia CUDA interface.
Since bus bandwidth and CPU power are often a
bottleneck in calculations, CUDA Remote Direct
Memory Access (RDMA) can be used to directly transfer data from/to a Spectrum Digitizer/Generator to/from a GPU card for processing,
thus avoiding the transfer of raw data to the host memory and benefiting from the computational power of the GPU.
For applications requiring high performance signal and data processing Spectrum of-
fers SCAPP (Spectrum’s CUDA Access for Parallel Processing).
The SCAPP SDK allows a direct link between Spectrum digitizers or generators and
CUDA based GPU cards. Once data is available to the GPU, users can harness the pro-
cessing power of the GPU’s massive number of processing cores and large, ultra-high-
speed GPU memory. SCAPP uses an RDMA (Linux only) process to send data at the dig-
itizers full PCIe transfer speed to the GPU card. The SDK includes a set of examples for
interaction between the digitizer or generator and the GPU card and another set of
CUDA parallel processing examples with easy building blocks for basic functions like
filtering, averaging, data de-multiplexing, data conversion or FFT. All the software is
based on C/C++ and can easily be implemented, expanded and modified with normal
programming skills.
Image 45: GPU usage with SCAPP SDK: data transfer options
Please follow the description in the SCAPP manual for installation and usage of the SCAPP drivers for this card.
Please note that the DDS commands for cards that support one of the DDS features cannot be directly
streamed from the GPU to the card, since the necessary re-calculation of the hardware’s DDS registers re-
quires calculation to be done under control of the host CPU.
(c) Spectrum Instrumentation GmbH 78

Programming the Board Overview
