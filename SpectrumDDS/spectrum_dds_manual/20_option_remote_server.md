Option Remote Server
Introduction
Using the Spectrum Remote Server (order code
-SPc-RServer) it is possible to access the
M2i/M3i/M4i/M4x/M2p/M5i card(s) installed in
one PC (server) from another PC (client) via local
area network (LAN), similar to using a digitizerNET-
BOX, generatorNETBOX or hybridNETBOX.
It is possible to use different operating systems on
both server and client. For example the Remote Serv-
er is running on a Linux system and the client is ac-
cessing them from a Windows system.
The Remote Server software requires, that the option
„-SPc-RServer“ is installed on at least one card in-
stalled within the server side PC. You can either
check this with the Control Center in the "Installed
Image 100: Overview of remote server option interaction in comparison to NETBOX devices
Card features" node or by reading out the feature
register, as described in the „Installed features and
options“ passage, earlier in this manual.
To run the Remote Server software, it is required to have least version 3.18 of the Spectrum SPCM driver in-
stalled. Additionally at least on one card in the server PC the feature flag SPCM_FEAT_REMOTESERVER must
be set.
Installing and starting the Remote Server
Windows
Windows users find the Control Center installer on the USB-
Stick under „Install\win\spcm_remote_install.exe“.
After the installation has finished there will be a new start
menu entry in the Folder "Spectrum GmbH" to start the Re-
mote Server. To start the Remote Server automatically after
login, just copy this shortcut to the Autostart directory.
Linux
Linux users find the versions of the installer for the different
StdC libraries under under /Install/linux/spcm_con-
trol_center/ as RPM packages.
To start the Remote Server type "spcm_remote_server" (with-
out quotation marks). To start the Remote Server automati-
cally after login, add the following line to the .bashrc or
.profile file (depending on the used Linux distribution) in the
user's home directory:
spcm_remote_server&
Detecting the digitizerNETBOX/generatorNETBOX/hybridNETBOX
Before accessing the digitizerNETBOX/generatorNETBOX/hybridNETBOX one has to determine the IP address of the device. Normally that
can be done using one of the two methods described below:
Discovery Function
The digitizerNETBOX/generatorNETBOX/hybridNETBOX responds to the VISA described Discovery function. The next chapter will show
how to install and use the Spectrum control center to execute the discovery function and to find the Spectrum hardware. As the discovery
function is a standard feature of all LXI devices there are other software packages that can find the device using the discovery function:
• Spectrum control center (limited to Spectrum remote products)
• free LXI System Discovery Tool from the LXI consortium (www.lxistandard.org)
• Measurement and Automation Explorer from National Instruments (NI MAX)
• Keysight Connection Expert from Keysight Technologies
(c) Spectrum Instrumentation GmbH 197

Option Remote Server Detecting the digitizerNETBOX/generatorNETBOX/hybridNETBOX
Additionally the discovery procedure can be started from your own application, as shown below:
#define TIMEOUT_DISCOVERY 5000 // timeout value in ms
const uint32 dwMaxNumRemoteCards = 50;
char* pszVisa[dwMaxNumRemoteCards] = { NULL };
char* pszIdn[dwMaxNumRemoteCards] = { NULL };
const uint32 dwMaxIdnStringLen = 256;
const uint32 dwMaxVisaStringLen = 50;
// allocate memory for string list
for (uint32 i = 0; i < dwMaxNumRemoteCards; i++)
{
pszVisa[i] = new char [dwMaxVisaStringLen];
pszIdn[i] = new char [dwMaxIdnStringLen];
memset (pszVisa[i], 0, dwMaxVisaStringLen);
memset (pszIdn[i], 0, dwMaxIdnStringLen);
}
// first make discovery - check if there are any LXI compatible remote devices
dwError = spcm_dwDiscovery ((char**)pszVisa, dwMaxNumRemoteCards, dwMaxVisaStringLen, TIMEOUT_DISCOVERY);
// second: check from which manufacturer the devices are
spcm_dwSendIDNRequest ((char**)pszIdn, dwMaxNumRemoteCards, dwMaxIdnStringLen);
// Use the VISA strings of these devices with Spectrum as manufacturer
// for accessing remote devices without previous knowledge of their IP address
Finding the digitizerNETBOX/generatorNETBOX/hybridNETBOX in the network
As the digitizerNETBOX/generatorNETBOX/hybridNETBOX is a standard network device, it has its own IP address and host name and can
therefore be found on the computer network by other devices. The standard host name consist of the model type and the serial number of the
device. The serial number is also found on the type plate on the back of the digitizerNETBOX/generatorNETBOX/hybridNETBOX chassis.
As default DHCP (IPv4) will be used and an IP address will be automatically set. In case no DHCP server is found, an IP will be obtained
using the AutoIP feature. This will lead to an IPv4 address of 169.254.x.y (with x and y being assigned to a free IP in the network) using a
subnet mask of 255.255.0.0.
The default IP setup can also be restored, by using the „LAN Reset“ button on the device.
If a fixed IP address should be used instead, the parameters need to be set according to the current LAN requirements.
Windows7, Windows8, Windows10
and Windows 11
Under Windows7, Windows8, Windows 10
and Windows11 the digitizerNETBOX, genera-
torNETBOX and hybridNETBOX devices are list-
ed under the „other devices“ tree with their given
host name.
A right click on the digitizerNETBOX or
generatorNETBOX device opens the properties
window where you find further information on the
device including the IP address.
From here it is possible to go the website of the
device where all necessary information are found
to access the device from software.
Image 101: Windows screenshot: finding a remote Spectrum device like digitizerNETBOX
(c) Spectrum Instrumentation GmbH 198

Option Remote Server Accessing remote cards
Troubleshooting
If the above methods do not work please try one of the following steps:
• Ask your network administrator for the IP address of the digitizerNETBOX/generatorNETBOX and access it directly over the IP address.
• Check your local firewall whether it allows access to the device and whether it allows to access the ports listed in the technical data sec-
tion.
• Check with your network administrator whether the subnet, the device and the ports that are listed in the technical data section are acces-
sible from your system due to company security settings.
Accessing remote cards
To detect remote card(s) from the client PC, start the Spectrum Control Center on the client and click "Netbox Discovery". All discovered cards
will be listed under the "Remote" node.
Using remote cards instead of using local ones is as easy as using a digitizerNETBOX and only requires a few lines of code to be changed
compared to using local cards.
Instead of opening two locally installed cards like this:
hDrv0 = spcm_hOpen ("/dev/spcm0"); // open local card spcm0
hDrv1 = spcm_hOpen ("/dev/spcm1"); // open local card spcm1
one would call spcm_hOpen() with a VISA string as a parameter instead:
hDrv0 = spcm_hOpen ("TCPIP::192.168.1.2::inst0::INSTR"); // open card spcm0 on a Remote Server PC
hDrv1 = spcm_hOpen ("TCPIP::192.168.1.2::inst1::INSTR"); // open card spcm1 on a Remote Server PC
to open cards on the Remote Server PC with the IP address 192.168.1.2. The driver will take care of all the network communication.
(c) Spectrum Instrumentation GmbH 199

Appendix Error Codes
