from dcamapi import DCAMAPI


def receive_image(cache):
    dcamapi = DCAMAPI()
    dcamapi.dcam_init()
    dcamapi.dcam_open()
    dcamapi.dcam_setbinning(int(cache.binning))
    dcamapi.dcam_settriggermode(int(cache.triggermode))
    frame = dcamapi.run()
    dcamapi.dcam_idle()
    dcamapi.dcam_freeframe()
    dcamapi.dcam_close()
    return frame
