Software Driver Installation and Driver Update
Before using the board, a driver must be installed that matches the operating system. Later on the same principles for the initial installation
also apply, when updating an existing driver on the system to a newer version.
Since driver V3.33 (released on install-disk V3.48 in August 2017) the installation is done via an installer
executable rather than manually via the Windows Device Manager. The steps for manually installing a card
has since been moved to a separate application note „AN008 - Legacy Windows Driver Installation“.
This new installer is common on all currently supported Windows platforms (Windows7, Windows8, Windows10 and Windows 11) both
32bit and 64bit. The driver from the USB-Stick supports all cards of the M2i/M3i, M4i/M4x, M2p and M5i series, meaning that you can
use the same driver for all cards of these families. This driver installer is also available from the Spectrum homepage under
https://spectrum-instrumentation.com/support/downloads.php
Windows
Before initial installation
When you install a card for the very first time, Windows will dis-
cover the new hardware and might try to search the Microsoft
Website for available matching driver modules (where no match-
ing driver will be found).
Prior to running the Spectrum installer, the card will hence appear
in the Windows device manager as a generalized card, shown
here is the device manager of a Windows 10 as an example.
• M2i and M3i cards will be shown as „DPIO module“
• M5i, M4i, M4x and M2p cards will be shown as
„PCI Data Acquisition and Signal Processing Controller“
Image 13: Windows Device Manager showing a new Spectrum card
Running the driver Installer/Update
Simply run the installer supplied either on the USB-Stick
“\Driver\windows“ folder or download it from our homepage
and run it.
The installer can be run on a fresh system for the first install or also
later on, when updating an already existing driver on the system.
Image 14: Spectrum Driver Installer Welcome Screen
(c) Spectrum Instrumentation GmbH 44

Software Driver Installation and Driver Update Windows
Image 15: Spectrum Driver Installer - Progress
Image 16: Spectrum Driver Installer - finished
After installation
After running the Spectrum driver installer, the card will appear in
the Windows device manager with its name matching the card se-
ries.
The card is now ready to be used with the new or updated driver.
Image 17: Windows Device Manager showing properly installed Spectrum card
(c) Spectrum Instrumentation GmbH 45

Software Driver Installation and Driver Update Linux
Linux
Overview
The Spectrum M2i/M3i/M4i/M4x/M2p/M5i cards and digitizerNETBOX/generatorNETBOX or
hybridNETBOX products are delivered with Linux drivers for 64 bit systems. As each Linux distribution con-
tains different kernel versions and different system setup it is in nearly every case necessary, to have a di-
rectly matching kernel driver for card level products to run it on a specific system.
For digitizerNETBOX/generatorNETBOX or hybridNETBOX products the library is sufficient and no kernel
driver has to be installed.
Spectrum delivers pre-compiled kernel driver modules for a number of common distributions with the cards.
You may try to use one of these kernel modules for different distributions which have a similar kernel version.
Unfortunately this won’t work in most cases as most Linux system refuse to load a driver which is not exactly
matching. In this case it is possible to get the kernel driver sources from Spectrum. Please contact your local
sales representative to get more details on this procedure.
The Standard delivery contains the pre-compiled kernel driver modules for the most popular Linux distributions, like Suse, Debian, Fedora and
Ubuntu. The list with all pre-compiled and readily supported distributions and their respective kernel version can be found under:
https://spectrum-instrumentation.com/support/knowledgebase/software/Supported_Linux_Distributions.php or via the shown QR code.
The Linux drivers have been tested with all above mentioned distributions by Spectrum. Each of these distributions has been installed with the
default setup using no kernel updates. A lot more different distributions are used by customers with self compiled kernel driver modules.
Driver Installation with Installation Script
The driver is delivered as installable kernel modules together with libraries to access the kernel driver. The installation script will help you with
the installation of the kernel module and the library.
This installation is only needed if you are operating real locally installed cards. For software emulated demo
cards, remotely installed cards or for digitizerNETBOX/generatorNETBOX/hybridNETBOX products it is only
necessary to install the libraries without a kernel as explained further below.
Login as root
It is necessary to have the root rights for installing a driver.
Call the install.sh <install_path> script
This script will try to use the package management of the system to install the kernel module and user-space driver library packages:
• the kernel driver package is called „ “ (M2i, M3i) or „ “ (M4i, M4x, M2p, M5i)
spcm spcm4
• the driver library package is called „ “
libspcm_linux
Udev support
Once the driver is loaded it automatically generates the device nodes under . The cards are automatically named to ,
/dev /dev/spcm0
,...
/dev/spcm1
You may use all the standard naming and rules that are available with udev.
Start the driver
The kernel driver should be loaded automatically when the system boots. If you need to load the kernel driver manually use the „modprobe“
command (as root or using sudo):
For M2i and M3i cards:
modprobe spcm
For M5i, M4i, M4x and M2p cards:
modprobe spcm4
Get first driver info
After the driver has been loaded successfully some information about the installed boards can be found in the matching file as shown
/proc/
below. Some basic information from the on-board EEProm is listed for every card.
(c) Spectrum Instrumentation GmbH 46

Software Driver Installation and Driver Update Linux
For M2i and M3i cards:
cat /proc/spcm_cards
For M5i, M4i, M4x and M2p cards:
cat /proc/spcm4_cards
Stop the driver
You can unload the kernel driver using the „modprobe -r“ command (as root or using sudo):
For M2i and M3i cards:
modprobe -r spcm
For M5i, M4i, M4x and M2p cards:
modprobe -r spcm4
Standard Driver Update
A driver update is done with the same commands as shown above. Please make sure that the driver has been stopped before updating it.
To stop the driver you may use the proper “modprobe -r” command as shown above.
Compilation of kernel driver sources (optional and local cards only)
The driver sources are only available for existing customers upon special request. Please send an email to Support@spec.de to receive the
kernel driver sources. The driver sources are not part of the standard delivery. The driver source package contains only the sources of the
kernel module, not the sources of the library.
Please do the following steps for compilation and installation of the kernel driver module:
Login as root
It is necessary to have the root rights for installing a driver.
Call the compile script
The compile script depends on the type of card that you have installed:
• for M2i and M3i cards: make_spcm_linux_kerneldrv.sh
• for M5i, M4i, M4x and M2p cards: make_spcm4_linux_kerneldrv.sh
This script will examine the type of system you use and compile the kernel with the correct settings. The compilation of the kernel driver modules
requires the kernel sources of the running kernel. These are normally available as a package with a name like kernel-devel, kernel-dev, kernel-
source and need to match the running kernel.
The compiled driver module will be copied to the module directory of the kernel ( ),
/lib/modules/$(uname -r)/kernel/drivers/
and will be loaded automatically at the next boot. To load or unload the kernel driver module manually use the modprobe command as
explained above in “Start the driver” and “Stop the driver”.
Update of a self compiled kernel driver
If the kernel driver has changed, one simply has to perform the same steps as shown above and recompile the kernel driver module. However
the kernel driver module isn’t changed very often.
Normally an update only needs new libraries. To update the libraries only you can either download the full Linux driver
(spcm_linux_drv_v123b4567) and only use the libraries out of this or one downloads the library package which is much smaller and doesn’t
contain the pre-compiled kernel driver module (spcm_linux_lib_v123b4567).
The update is done with a dedicated script which only updates the library file. This script is present in both driver archives:
sh install_libonly.sh
(c) Spectrum Instrumentation GmbH 47

Software Driver Installation and Driver Update Linux
Installing the library only without a kernel (for remote devices)
The kernel driver module only contains the basic hardware functions that are necessary to access locally installed card level products. The
main part of the driver is located inside a dynamically loadable library that is delivered with the driver. This library is available in two different
versions:
• spcm_linux_32bit_stdc++6.so - supporting libstdc++.so.6 on 32 bit systems
• spcm_linux_64bit_stdc++6.so - supporting libstdc++.so.6 on 64 bit systems
The matching version is installed automatically in the “ or “ or “ directory
/usr/lib” /usr/lib64/” /usr/lib/x86_64-linux-gnu”
(depending on your Linux distribution) by the kernel driver install script for card level products. The library is renamed for easy access to
libspcm_linux.so.
For digitizerNETBOX/generatorNETBOX/hybridNETBOX products and also for evaluating or using only the software simulated demo cards
the library is installed with a separate install script:
sh install_libonly.sh
To access the driver library one must include the library in the compilation:
gcc -o test_prg -lspcm_linux test.cpp
To start programming the cards under Linux please use the standard C/C++ examples which are all running under Linux and Windows.
Installation from Spectrum Repository
The driver library, Spectrum Control Center and SBench6 can be easily installed and updated from our online repositories.Adding the repos-
itory to the system and installing software differs depending on the package format used by the Linux distribution.
DEB based distributions (like Debian, Ubuntu and derived distributions)
Execute the following commands to get the Spectrum repository key and convert it for local use:
wget http://spectrum-instrumentation.com/dl/repo-key.asc
gpg --dearmor -o repo-key.gpg repo-key.asc
cp repo-key.gpg /etc/apt/spectrum-instrumentation.gpg
To add the repository create a new file /etc/apt/sources.list.d/spectrum-instrumentation.list with this content. Please note that there is a man-
datory blank between URL and “./”:
deb [signed-by=/etc/apt/spectrum-instrumentation.gpg] http://spectrum-instrumentation.com/dl/ ./
Alternatively this line can be added to /etc/apt/sources.list
Then run
sudo apt update
to update the repository information.
To install the software (e.g. SBench6) run
sudo apt install sbench6
An overview of DEB based distributions can be found here: https://en.wikipedia.org/wiki/Category:Debian-based_distributions
RPM based distributions
On distributions using Zypper (such as openSUSE, SLES, ...) to add the repository run:
sudo zypper ar --repo http://spectrum-instrumentation.com/dl/spectrum_instrumentation.repo
The repository information will be updated automatically.
To install the software (e.g. SBench6) run
sudo zypper install SBench6
(c) Spectrum Instrumentation GmbH 48

Software Driver Installation and Driver Update Linux
On distributions using DNF (such as Fedora, CentOS Stream, RHEL, ...) to add the repository run
sudo dnf config-manager --add-repo http://spectrum-instrumentation.com/dl/spectrum_instrumentation.repo
The repository information will be updated automatically.
To install the software (e.g. SBench6) run
sudo dnf install SBench6
An overview of RPM based distributions can be found here: https://en.wikipedia.org/wiki/Category:RPM-based_Linux_distributions
Control Center
The Spectrum Control Center is also available for Linux and needs to be installed sep-
arately. The features of the Control Center are described in a later chapter in deeper
detail. The Control Center has been tested under all Linux distributions for which Spec-
trum delivers pre-compiled kernel modules. The following packages need to be in-
stalled to run the Control Center:
• X-Server
• expat
• freetype
• fontconfig
• libpng
• libspcm_linux (the Spectrum Linux driver library)
Installation
Use the supplied packages in either *.deb or *.rpm format found in the driver section
of the USB stick by double clicking the package file root rights from a X-Windows win-
dow.
The Control Center is installed under KDE, Gnome or Unity in the system/system tools
section. It may be located directly in this menu or under a „More Programs“ menu. The
final location depends on the used Linux distribution. The program itself is installed as
and may be started directly from here.
/usr/bin/spcmcontrol
Image 18: Device Manager showing a new Spectrum card
Manual Installation
To manually install the Control Center, first extract the files from the rpm matching your distribution:
rpm2cpio spcmcontrol-{Version}.rpm > ~/spcmcontrol-{Version}.cpio
cd ~/
cpio -id < spcmcontrol-{Version}.cpio
You get the directory structure and the files contained in the rpm package. Copy the binary spcmcontrol to . Copy the .desktop
/usr/bin
file to . Run ldconfig to update your systems library cache. Finally you can run spcmcontrol.
/usr/share/applications
Troubleshooting
If you get a message like the following after starting spcmcontrol:
spcm_control: error while loading shared libraries: libz.so.1: cannot open shared object file: No such file
or directory
(c) Spectrum Instrumentation GmbH 49

Software Driver Installation and Driver Update Linux
Run ldd spcm_control in the directory where spcm_control resides to see the dependencies of the program. The output may look like this:
libXext.so.6 => /usr/X11R6/lib/libXext.so.6 (0x4019e000)
libX11.so.6 => /usr/X11R6/lib/libX11.so.6 (0x401ad000)
libz.so.1 => not found
libdl.so.2 => /lib/libdl.so.2 (0x402ba000)
libpthread.so.0 => /lib/tls/libpthread.so.0 (0x402be000)
libstdc++.so.6 => /usr/lib/libstdc++.so.6 (0x402d0000)
As seen in the output, one of the libraries isn’t found inside the library cache of the system. Be sure that this library has been properly installed.
You may then run ldconfig. If this still doesn’t help please add the library path to and run ldconfig again.
/etc/ld.so.conf
If the libspcm_linux.so is quoted as missing please make sure that you have installed the card driver properly before. If any other library is
stated as missing please install the matching package of your distribution.
(c) Spectrum Instrumentation GmbH 50

Software Software Overview
