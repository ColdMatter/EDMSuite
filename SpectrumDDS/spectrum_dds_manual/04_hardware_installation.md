Hardware Installation
ESD Precautions
All Spectrum boards contain electronic components that can be damaged by electrostatic discharge (ESD).
Before installing the board in your system or protective conductive packaging, discharge yourself by touching
a grounded bare metal surface or approved anti-static mat before picking up this ESD sensitive product.
Sources of noise
Noise sensitive analog devices, such as analog acquisition and generator boards should be placed physically as far away from any noise
producing source (like e.g. the power supply) as possible. It should especially be avoided to place the board in the slot directly adjacent to
another fast board like e.g. a graphics controller.
Cooling Precautions
The boards of the M4i.xxxx-x8 and M4x.xxxx-x4 series operate with components having very high power consumption at high speeds. For
this reason it is absolutely required to cool the boards sufficiently.
For all M4i cards it is absolutely mandatory to have installed cooling fans specifically providing a stream of
air across the board’s surface.
• Make absolutely sure, that the on-board fan on the M4i card is not blocked by PC internal cabling or any other means.
• Ensure that there is plenty of space around the PC chassis fan’s intake and exhaust vents, both inside and outside the chassis.
• If your chassis includes fan filters, make sure that these are regularly cleaned.
• Set the rotation speed for all chassis fans and especially those providing air for the PCIe/PXIe cards to highest setting in the BIOS/UEFI.
• Whenever possible leave the slot adjacent to the M4i/M4x card empty. This allows for best possible air flow over the card’s surface.
• If you do need to to use any adjacent slots, preferably install cards, that grant the most clearance between the devices, such as low-profile
adapters.
• If available install filler panels with ventilation holes for all unused PCI or PCI Express slots to allow for additional air flow for the M4i
cards and serve as an additional outtake.
Connector Handling Precautions
The connectors used on this product are designed for high signal quality and good shielding. Due to the limited space on the front-panel they
have to be as small as possible to fit the needed signal connections on the front panel. Therefore these connectors are vulnerable to mechan-
ical damages when used not properly. Especially SMB and MMCX connectors may be broken when not operated correctly.
Always dismount the connections by operating the connector itself and not the cable. Always move the cable
connector in a straight line from the board connector. Do not cant the connector when opening the connection.
A broken connector can only be replaced in factory and is not covered by warranty.
(c) Spectrum Instrumentation GmbH 39

Hardware Installation M4i PCIe Cards
M4i PCIe Cards
System Requirements
All Spectrum M4i.xxxx-x8 instrumentation cards are compliant to the PCI Express 2.0 standard and require in general one free 3/4 length
PCI Express slot. This can mechanically either be a x8 or x16 slot, electrically all lane widths are supported, be it x1, x4, x8 or x16. Please
consult your mainboard manual for details. Depending on the installed options additional free slots can be necessary.
Installing the M4i board in the system
Please be sure that the system is powered-down and all power cables are disconnected from the system before starting with the installation
process.
Installing a single board without any options
Before installing the board you first need to unscrew and remove the dedicated blind-bracket usually mounted to cover unused slots of your
PC. Please keep the screw in reach to fasten your Spectrum card afterwards. All Spectrum M4i cards mechanically require one PCI Express
x8 or x16 slot (electrically either x1, x4, x8 or x16). Now insert the board slowly into your computer. This is done best with one hand each
at both fronts of the board.
Please take especial care to not bend the card in any direction while inserting it into the system. Bending of
the card may damage the PCB totally and is not covered by the standard warranty.
Please be very careful when inserting the board in the slot, as most of the mainboards are mounted with
spacers and therefore might be damaged if they are exposed to high pressure.
After the insertion of the board fasten the screw of the bracket carefully, without overdoing.
Installing the M4i.xxxx-x8 PCI Express card in a PCIe x8 or x16 slot:
Image 7: Mounting M4i PCIe card into connector
(c) Spectrum Instrumentation GmbH 40

Hardware Installation M4i PCIe Cards
Additional notes on PCIe x16 slot retention
M4i-xxx-x8 cards starting with
hardware version V7 (which
includes the new PCB revision
V1.2) do have an additional
PCIe retention hook (hockey
stick) added to the PCB.
That allows the card to be ad-
ditionally locked when being
installed into a PCIe x16 slot.
Image 8: M4i card slot retention with perforation
When installing the card
in a x16 slot, make sure that the locking mechanism of the slots properly lock in place with the retention
hook.
In the case that there are any components on the mainboard in the way of the retention hook when installing
the card in an x8 slot, you can remove the hook by carefully breaking it off at its perforation line.
Providing additional power to M4i.xxxx-x8 cards
All PCI Express cards, with the exception of graphic adapters, are
per specification only allowed to consume a maximum power of
25W per card. While some of the M4i PCIe cards are specified
with a power consumption to meet these power limits, many do
consume more than 25W of total power.
This is why all M4i cards can be optionally supplied with the re-
quired voltages via a dedicated PCIe 6-pin power connector di-
rectly from the system power supply.
As part of its power-on routine, the card will automatically detect,
whether a cable is plugged or not and will give preference to the
cable-supplied voltages.
Although it would be considered good practice to always provide
the power via cable in case the card’s rated power consumption Image 9: M4i card additional power connection usage
is above the 25W limit, in typical system setups with one or at
maximum two cards installed, not doing so and using just the slot power usually works out perfectly fine. Having more M4i cards in a system
will definitely require a separate power cable per card.
Please only connect 6-pin PCIe power cables to the M4i cards power connector and make absolutely sure,
that its three lower row wires are marked yellow (hence providing 12V) and the three upper row wires (the
side of the connectors retention hook) are marked black providing a connection to system ground (GND), as
shown on the picture.
Installing a M4i.44xx board with digital inputs/outputs mounted on an extra bracket
Before installing the board you first need to unscrew and remove the dedicated blind-bracket usually mounted to cover unused slots of your
PC. Please keep the screw in reach to fasten your Spectrum card afterwards. All Spectrum M4i cards mechanically require one PCI Express
x8 or x16 slot (electrically either x1, x4, x8 or x16). Now insert the board with it’s attached extra bracket slowly into your computer. This is
done best with one hand each at both fronts of the board.
Please take special care to not bend the card in any direction while inserting it into the system. A bending of
the card may damage the PCB totally and is not covered by the standard warranty.
Please be very carefully when inserting the board in the PCI slot, as most of the mainboards are mounted
with spacers and therefore might be damaged they are exposed to high pressure.
(c) Spectrum Instrumentation GmbH 41

Hardware Installation M4i PCIe Cards
After the board’s insertion fasten the screws of both brackets carefully, without overdoing. The figure shows a board
with the option installed.
Image 10: M4i card with digital option mechanical installation and position of screws
Installing multiple boards synchronized by star-hub option
Precautions M4i cards with Star-Hub option (SH8ex)
Due to the length of the SMA connectors on the card’s bracket in combination with the full-length of a card having the option SH8ex installed,
it may be necessary with some computer case designs, to remove the black plastic retainer bracket from the end of the M4i card’s main PCB
to properly plug the card into the PCIe slot.
In case the retainer must be removed, an alternative to steadily holding the back of the PCB should then be implemented (if not already present
in the case design).
This is especially critical, when the probability exists that the computer may be subject to sudden movement, shock or during shipment!
When fitting the card, please take care not the damage the motherboard with the lower edge of the metal
front connector bracket.
Hooking up the boards
Before mounting several synchronized boards for a multi channel
system into the PC you can hook up the cards with their synchroni-
zation cables first. If there is enough space in your computer’s case
(e.g. a big tower case) you can also mount the boards first and
hook them up afterwards. Spectrum ships the card carrying the star-
hub option together with the needed amount of synchronization ca-
bles. All of them are matched to the same length, to achieve a zero
clock delay between the cards.
Only use the included flat ribbon cables.
All of the cards, including the one that carries the star-hub
piggy-back module, must be wired to the star-hub.
It does not matter which of the available connectors on the star-hub
module you use for which board. The software driver will detect the
Image 11: M4i cards with star-hub ex installed and connecting cables
types and order of the synchronized boards automatically.
All of the synchronization cables are secured against wrong plugging, but nonetheless you should
take care to have the pin 1 markers on the connector and on the cable on the same side, as the
figure on the right is showing.
Mounting the wired boards
Before installing the cards you first need to unscrew and remove the dedicated blind-brackets usu-
ally mounted to cover unused slots of your PC. Please keep the screws in reach to fasten your Spec-
trum cards afterwards.
Spectrum M4i cards with the option „M4i.xxxx-SH8tm“ installed require two slots with ¾ PCIe
length, whilst M4i cards with the option „M4i.xxxx-SH8ex“ installed require one single full length
PCIe slot with a track at the backside to guide the card by its retainer.
(c) Spectrum Instrumentation GmbH 42

Hardware Installation M4x PXIe Cards
Now insert the cards slowly into your computer. This is done best with one hand each at both fronts of the board.
While inserting the board take care not to tilt the retainer in the track. Please take especial care to not bend
the card in any direction while inserting it in the system. A bending of the card may damage the PCB totally
and is not covered by the standard warranty.
Please be very careful when inserting the cards in the slots, as most of the mainboards are mounted with
spacers and therefore might be damaged if they are exposed to high pressure.
Shipment of systems with Spectrum cards installed
When shipping complete systems with Spectrum cards installed make sure that the cards are properly secured and cannot bent while being
transported. When using freight forwarders, the transport and handling processes can be quite rough potentially subjecting the shopped PC
system to quite large shocks. If the installed spectrum cards are not well mounted, secured correctly at the front and - if applicable for your
model - back of the card, it's possible that they can bend when subjected to strong forces, such when a shipment container is dropped.
If damage occurs during a transport, we do not consider this to be covered by the warranty.
To avoid this we strongly recommend that when shipping these systems, customers either:
• Install the cards securely - with separate protection - so they cannot bend, or
• Remove the cards and ship them separately in their original shipping boxes (or similar packaging)
Please note that a sole fixing of the card at the front panel may not be sufficient to avoid damages in case of a mechanical shock!
M4x PXIe Cards
System Requirements
The Spectrum M4x PXIe 3U cards run in dedicated 3U PXIe slots as well as 3U PXI/PXIe hybrid slots. The M4x series of cards occupies two
slots width, so up to eight cards can be installed in a large chassis providing 16 PXIe slots for peripheral cards.
The M4x cards cannot be installed in either the CPU system slot nor in the dedicated system timing slot. Only
a peripheral slot marked with the „circle“ symbol is suited for the cards.
Installing the M4x board in the system
Installing a single board without any options
The locks on the bottom side of PXIe boards need to be unlocked and opened before installing the board into a free slot of the system. The-
refore you need to press the little button on the inside of the fastener and move it outwards (see figure). Now slowly insert the card into the
host system using the key ways until the lock snaps in with a „click“.
While inserting the board take care not to tilt it.
After the board’s insertion fasten the four screws carefully, without overdoing.
Image 12: Installation of PXIe cards with connector handling and mounting screws
(c) Spectrum Instrumentation GmbH 43

Software Driver Installation and Driver Update Windows
