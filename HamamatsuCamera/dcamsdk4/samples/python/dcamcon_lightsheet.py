"""Sample for lightsheet control with DCAM.

Sample script for setting properties related lightsheet to a camera.
"""

__date__ = '2023-12-06'
__copyright__ = 'Copyright (C) 2024 Hamamatsu Photonics K.K.'

import os
# for getting file name form __file__

import signal
# for handiling pressing Ctrl+C

import cv2
# pip install opencv-python
# Tested Version (Win/Lnx): 4.6.0.66/4.9.0.80
# License: Apache2 Software License (https://github.com/opencv/opencv/blob/master/LICENSE)
# for disply image

from screeninfo import get_monitors
# pip install screeninfo
# Tested Version (Win/Lnx): 0.8.1/0.8.1
# License: MIT License (https://github.com/rr-/screeninfo/blob/master/LICENSE.md)
# for getting monitor information

from dcamcon import *
# for control DCAM fucntion

# OpenCV window status.
# 0 = not created yet
# 1 = already created and open
# -1 = close manually by user 
cv_window_status = 0

signaled_sigint = False    # True means Ctrl+C was pressed

# if True, acquired images is saved as .RAW at finished live.
save_at_finish_live = False

def setup_properties(dcamcon: Dcamcon):
    """Set properties ralated lightsheet.

    Set to lightsheet mode and set properties related lightsheet for cpaturing.

    Args:
        dcamcon (Dcamcon): Dcamcon instance
    
    Returns:
        bool: result
    """
    # setup SENSORMODE to PROGRESSIVE to enable Lightsheet mode
    val = DCAMPROP.SENSORMODE.PROGRESSIVE
    val = dcamcon.setget_propertyvalue(DCAM_IDPROP.SENSORMODE, val)
    if val is False:
        print('Lightsheet is not supported.')
        return False
    
    if val != DCAMPROP.SENSORMODE.PROGRESSIVE:
        print('Lightsheet mode not capable with this device.')
        return False
    
    # not control PIXELTYPE and subarray. PIXELTYPE is MONO16. subarray is disable
    # subarray affects range of EXPOSURETIME, so implement here if necessary 


    if (dcamcon.prompt_propvalue(DCAM_IDPROP.READOUT_DIRECTION) is False or
        dcamcon.prompt_propvalue(DCAM_IDPROP.INTERNAL_LINEINTERVAL) is False or
        dcamcon.prompt_propvalue(DCAM_IDPROP.EXPOSURETIME) is False):
        return False
    
    # output trigger control.
    # if HSync output trigger is supported, set related properties
    val = DCAMPROP.OUTPUTTRIGGER_KIND.PROGRAMABLE
    val = dcamcon.setget_propertyvalue(DCAM_IDPROP.OUTPUTTRIGGER_KIND, val)
    if val == DCAMPROP.OUTPUTTRIGGER_KIND.PROGRAMABLE:
        val = DCAMPROP.OUTPUTTRIGGER_SOURCE.HSYNC
        val = dcamcon.setget_propertyvalue(DCAM_IDPROP.OUTPUTTRIGGER_SOURCE, val)
        if val == DCAMPROP.OUTPUTTRIGGER_SOURCE.HSYNC:
            if (dcamcon.prompt_propvalue(DCAM_IDPROP.OUTPUTTRIGGER_PREHSYNCCOUNT) is False or
                dcamcon.prompt_propvalue(DCAM_IDPROP.OUTPUTTRIGGER_POLARITY) is False or
                dcamcon.prompt_propvalue(DCAM_IDPROP.OUTPUTTRIGGER_PERIOD) is False or
                dcamcon.prompt_propvalue(DCAM_IDPROP.OUTPUTTRIGGER_DELAY) is False):
                return False
    
    return True

def show_framedata(camera_title, data):
    """Show image data.

    Open window of OpenCV with camera_title.
    Show numpy buffer as an image with OpenCV.

    Args:
        camera_title (string): for OpenCV window title
        data (Numpy ndarray): numpy buffer stored image
    """
    global signaled_sigint
    if signaled_sigint:
        return
    
    global cv_window_status
    if cv_window_status > 0:    # was the window created and open?
        cv_window_status = cv2.getWindowProperty(camera_title, 0)
        if cv_window_status == 0:   # if it is still open
            cv_window_status = 1    # mark it as still open again
    
    if cv_window_status >= 0:    # see if the window is not created yet or created and open
        maxval = np.amax(data)
        if data.dtype == np.uint16:
            if maxval > 0:
                imul = int(65535 / maxval)
                data = data * imul
        
        if cv_window_status == 0:
            # OpenCV window is not created yet
            cv2.namedWindow(camera_title, cv2.WINDOW_NORMAL | cv2.WINDOW_KEEPRATIO | cv2.WINDOW_GUI_NORMAL)

            # resize display window
            data_width = data.shape[1]
            data_height = data.shape[0]

            window_pos_left = 156
            window_pos_top = 48

            screeninfos = get_monitors()

            max_width = screeninfos[0].width - (window_pos_left * 2)
            max_height = screeninfos[0].height - (window_pos_top * 2)

            if data_width > max_width:
                scale_X100 = int(100 * max_width / data_width)
            else:
                scale_X100 = 100
            
            if data_height > max_height:
                scale_Y100 = int(100 * max_height / data_height)
            else:
                scale_Y100 = 100
            
            if scale_X100 < scale_Y100:
                scale_100 = scale_X100
            else:
                scale_100 = scale_Y100
            
            disp_width = int(data_width * scale_100 * 0.01)
            disp_height = int(data_height * scale_100 * 0.01)

            cv2.resizeWindow(camera_title, disp_width, disp_height)
            # end of resize

            cv2.moveWindow(camera_title, window_pos_left, window_pos_top)
            cv_window_status = 1
        
        cv2.imshow(camera_title, data)
        key = cv2.waitKey(1)
        if key == ord('q') or key == ord('Q'):  # if 'q' or 'Q' was pressed with the live window, close it
            cv_window_status = -1
        
        
def show_live_captured_images(dcamcon: Dcamcon):
    """Show live images.

    It captures images and shows live images.

    Args:
        dcamcon (Dcamcon): Dcamcon instance
    """
    # get property value used
    exposuretime = dcamcon.get_propertyvalue(DCAM_IDPROP.EXPOSURETIME)
    if exposuretime is False:
        # should be able to get the value
        return
    
    triggersource = dcamcon.get_propertyvalue(DCAM_IDPROP.TRIGGERSOURCE)
    if triggersource is False:
        # should be able to get the value
        return
    
    trigger_mode = dcamcon.get_propertyvalue(DCAM_IDPROP.TRIGGER_MODE)
    if trigger_mode is False:
        # shoulf be able to get the value
        return


    number_of_frames = 10
    # prepare buffer
    if not dcamcon.allocbuffer(number_of_frames):
        return
    
    # calculate timeout time
    timeout_millisec = 2
    
    frameinterval = dcamcon.get_propertyvalue(DCAM_IDPROP.INTERNAL_FRAMEINTERVAL, False)
    if frameinterval is not False:
        # set timeout waiting for a frame to arrive to exposure time + internal frame interval + 500 ms
        timeout_millisec = int((exposuretime + frameinterval) * 1000.0) + 500
    else:
        # set timeout waiting for a frame to arrive to exposure time + 1 second
        timeout_millisec = int(exposuretime * 1000.0) + 1000
    
    # let's use 2ms minimum timeout
    if timeout_millisec < 2:
        timeout_millisec = 2
    
    # start live
    if not dcamcon.startcapture():
        # dcamcon.allocbuffer() should have succeeded
        dcamcon.releasebuffer()
        return
    
    triggersource = dcamcon.get_propertyvalue(DCAM_IDPROP.TRIGGERSOURCE)

    firetrigger_cycle = 0
    framecount_till_firetrigger = 0
    if triggersource == DCAMPROP.TRIGGERSOURCE.SOFTWARE:
        if trigger_mode == DCAMPROP.TRIGGER_MODE.START:
            # Software Start requires only one firetrigger at beginning
            firetrigger_cycle = 0
        elif trigger_mode == DCAMPROP.TRIGGER_MODE.PIV:
            # PIV require firetrigger for 2 frames
            firetrigger_cycle = 2
        else:
            # standard software trigger requires one firetrigger for one frame
            firetrigger_cycle = 1
        
        # we'll fire a trigger to initiate capturing for this sample
        dcamcon.firetrigger()
        framecount_till_firetrigger = firetrigger_cycle
    
    timeout_happened = 0

    global cv_window_status
    global signaled_sigint
    while cv_window_status >= 0:
        if signaled_sigint:
            break

        res = dcamcon.wait_capevent_frameready(timeout_millisec)
        if res is not True:
            # frame does not come
            if res != DCAMERR.TIMEOUT:
                print('-NG: Dcam.wait_event() failed with error {}'.format(res))
                break

            # TIMEOUT error happens
            timeout_happened += 1
            if timeout_happened == 1:
                print('Waiting for a frame to arrive.', end='')
                if triggersource == DCAMPROP.TRIGGERSOURCE.EXTERNAL:
                    print(' Check your trigger source.', end='')
                else:
                    print(' Check your <timeout_millisec> calculation in the code.', end='')
                print(' Press Ctrl+C to abort.')
            else:
                print('.')
                if timeout_happened > 5:
                    timeout_happened = 0
            
            continue

        # wait_capevent_frameready() succeeded
        lastdata = dcamcon.get_lastframedata()
        if lastdata is not False:
            show_framedata(dcamcon.device_title, lastdata)
        
        if framecount_till_firetrigger > 0:
            framecount_till_firetrigger -= 1
            if framecount_till_firetrigger == 0:
                dcamcon.firetrigger()
                framecount_till_firetrigger = firetrigger_cycle
        
        timeout_happened = 0

    # End live
    cv2.destroyAllWindows()
    
    dcamcon.stopcapture()

    if save_at_finish_live:
        dcamcon.save_rawimages('LastGoodImage')

    dcamcon.releasebuffer()

def sigint_handler(signum, frame):
    """Detect pressing Ctrl+C.
    
    Signal handler function.
    This script is handling SIGINT only.
    signaled_sigint is set True.
    """
    global signaled_sigint
    signaled_sigint = True

#run handler (which does cleanup) if Ctrl+C is pressed in Python console.
signal.signal(signal.SIGINT, sigint_handler)

if __name__ == '__main__':
    ownname = os.path.basename(__file__)
    print('Start {}'.format(ownname))

    # initialize DCAM-API
    if dcamcon_init():
        # choose camera and get Dcamcon instance
        dcamcon = dcamcon_choose_and_open()
        if dcamcon is not None:
            res = True
            # set basic properties
            if (not signaled_sigint and
                res):
                res = setup_properties(dcamcon)
            
            # show live image
            if (not signaled_sigint and
                res):
                show_live_captured_images(dcamcon)
            
            # close dcam
            dcamcon.close()
    
    # cleanup dcamcon
    dcamcon_uninit()

    print('End {}'.format(ownname))