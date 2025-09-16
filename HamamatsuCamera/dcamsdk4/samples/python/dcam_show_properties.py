"""Sample script for showing the list of properties with dcam.py.

This script recognizes the camera and shows the list of properties with dcam.py.
Displays the property IDs and values that the camera supported.

This sample source code just shows how to use DCAM-API.
The performance is not guranteed.
"""

__date__ = '2021-06-18'
__copyright__ = 'Copyright (C) 2021-2024 Hamamatsu Photonics K.K.'

from dcam import *
# control DCAM functions

def dcam_show_properties(iDevice=0):
    """Show the list of properties.
    
    Show the list of property IDs and values that the camera supported.

    Args:
        iDevice (int): Device index.
    
    Returns:
        Nothing.
    """
    if Dcamapi.init():
        dcam = Dcam(iDevice)
        if dcam.dev_open():
            idprop = dcam.prop_getnextid(0)
            while idprop is not False:
                output = '0x{:08X}: '.format(idprop)

                propname = dcam.prop_getname(idprop)
                if propname is not False:
                    output = output + propname

                print(output)
                idprop = dcam.prop_getnextid(idprop)

            dcam.dev_close()
        else:
            print('-NG: Dcam.dev_open() fails with error {}'.format(dcam.lasterr()))
    else:
        print('-NG: Dcamapi.init() fails with error {}'.format(Dcamapi.lasterr()))

    Dcamapi.uninit()


if __name__ == '__main__':
    dcam_show_properties()
