from __future__ import print_function
import numpy as np
import ctypes as cp
import time

DCAMCAP_EVENT_FRAMEREADY = int("0x0002", 0)
DCAMERR_ERROR = 0
DCAMERR_NOERROR = 1

DCAMWAIT_TIMEOUT_INFINITE = int("0x80000000", 0)

DCAM_CAPTUREMODE_SNAP = 0
DCAM_CAPTUREMODE_SEQUENCE = 1

DCAM_DEFAULT_ARG = 0

DCAM_IDPROP_EXPOSURETIME = int("0x001F0110", 0)
DCAM_IDSTR_MODEL = int("0x04000104", 0)

CAPTUREMODE_SNAP = 0
CAPTUREMODE_SEQUENCE = 1

DCAM_IDMSG_GETPARAM=int("0x0202",0)
DCAM_IDMSG_SETPARAM=int("0x0201",0)
DCAM_IDPARAM_SUBARRAY_INQ=int("0x800001A2",0)
DCAM_IDPARAM_SUBARRAY=int("0xC00001E2",0)


class DCAM_PARAM_PROPERTYATTR(cp.Structure):
    _fields_ = [("cbSize", cp.c_int32),
                ("iProp", cp.c_int32),
                ("option", cp.c_int32),
                ("iReserved1", cp.c_int32),
                ("attribute", cp.c_int32),
                ("iGroup", cp.c_int32),
                ("iUnit", cp.c_int32),
                ("attribute2", cp.c_int32),
                ("valuemin", cp.c_double),
                ("valuemax", cp.c_double),
                ("valuestep", cp.c_double),
                ("valuedefault", cp.c_double),
                ("nMaxChannel", cp.c_int32),
                ("iReserved3", cp.c_int32),
                ("nMaxView", cp.c_int32),
                ("iProp_NumberOfElement", cp.c_int32),
                ("iProp_ArrayBase", cp.c_int32),
                ("iPropStep_Element", cp.c_int32)]

class DCAM_PARAM_PROPERTYVALUETEXT(cp.Structure):
    _fields_ = [("cbSize", cp.c_int32),
                ("iProp", cp.c_int32),
                ("value", cp.c_double),
                ("text", cp.c_char_p),
                ("textbytes", cp.c_int32)]

class DCAM_HDR_PARAM(cp.Structure):
    _fields_ = [("cbSize", cp.c_ulong),
                ("id", cp.c_ulong),
                ("iFlag", cp.c_ulong),
                ("oFlag", cp.c_ulong)]


class DCAM_PARAM_SUBARRAY_INQ(cp.Structure):
    _fields_ = [("hdr", DCAM_HDR_PARAM),
                ("binning", cp.c_int32),
                ("hmax", cp.c_int32),
                ("vmax", cp.c_int32),
                ("hposunit", cp.c_int32),
                ("vposunit", cp.c_int32),
                ("hunit", cp.c_int32),
                ("vunit", cp.c_int32)]


class DCAM_PARAM_SUBARRAY(cp.Structure):
    _fields_ = [("hdr", DCAM_HDR_PARAM),
                ("hpos", cp.c_int32),
                ("vpos", cp.c_int32),
                ("hsize", cp.c_int32),
                ("vsize", cp.c_int32)]

class DCAMAPI():
    def __init__(self):
        self.camera_id =  0
        self.camera_handle = cp.c_void_p(0)
        self.dcam=cp.windll.dcamapi
        
    
    def errorHandler(self,err_code,state):
        string_buffer=cp.create_string_buffer(100)
        if (err_code == DCAMERR_ERROR):
            last_error=self.dcam.dcam_getlasterror(self.camera_handle,
                                                   string_buffer,
                                                   cp.c_int32(100))
            raise Exception('Error in '+\
                            str(state)+\
                            ' with last error '+\
                            str(last_error))
        return err_code
    
    def dcam_init(self):
        n_camera=cp.c_int32(0)
        self.errorHandler(
                self.dcam.dcam_init(None,
                                    cp.byref(n_camera),
                                    None),"dcam_init")
        self.n_camera=n_camera
    
    def dcam_open(self):
        self.errorHandler(
                self.dcam.dcam_open(cp.byref(self.camera_handle),
                                             cp.c_int32(self.camera_id),
                                             None),
                                             "dcam_open")
        
    
    def dcam_close(self):
        self.errorHandler(
                self.dcam.dcam_close(self.camera_handle),
                                    "dcam_close")
        
    
    def dcam_getmodelinfo(self):
        string_buffer = cp.create_string_buffer(100)
        self.errorHandler(
                self.dcam.dcam_getmodelinfo(cp.c_int32(self.camera_id),
                                            cp.c_int32(DCAM_IDSTR_MODEL),
                                            string_buffer,
                                            cp.c_int(100)),
                                            "dcam_getmodelinfo")
        return string_buffer.value

    def set_capture_mode(self,capture_mode):
        self.capture_mode=capture_mode
    
    def getcameraproperties(self):
        pass
    def dcam_getpropertyattr(self):
        pass
    def dcam_getpropertyvalue(self):
        pass
    def dcam_getpropertyvaluetext(self):
        pass
    def dcam_setgetpropertyvalue(self):    
        pass
    
        
    def dcam_getbinning(self):
        binning=cp.c_int32(0)
        self.errorHandler(
                self.dcam.dcam_getbinning(self.camera_handle,
                                          cp.byref(binning)),
                                          "dcam_getbinning")
        return binning.value
    
    def dcam_getexposuretime(self):
        exptime=cp.c_double(0)
        self.errorHandler(
                self.dcam.dcam_getexposuretime(self.camera_handle,
                                               cp.byref(exptime)),
                                               "dcam_getexposuretime")
        return exptime.value
    
    def dcam_gettriggermode(self):
        trigmode=cp.c_int32(0)
        self.errorHandler(
                self.dcam.dcam_gettriggermode(self.camera_handle,
                                               cp.byref(trigmode)),
                                               "dcam_gettriggermode")
        return trigmode.value
    
    def dcam_gettriggerpolarity(self):
        trigpol=cp.c_int32(0)
        self.errorHandler(
                self.dcam.dcam_gettriggerpolarity(self.camera_handle,
                                                  cp.byref(trigpol)),
                                                  "dcam_gettriggerpolarity")
        return trigpol.value
    
    
    def dcam_getdataframebytes(self):
        frame_bytes = cp.c_int32(0)
        self.errorHandler(
                self.dcam.dcam_getdataframebytes(self.camera_handle,
                                                 cp.byref(frame_bytes)),
                                                 "dcam_getframedatabytes")
        return frame_bytes.value
    
    def dcam_setbinning(self,binning):
        self.errorHandler(
                self.dcam.dcam_setbinning(self.camera_handle,
                                          cp.c_int32(binning)),
                                          "dcam_setbinning")

    def dcam_setexposuretime(self,exptime):  # TODO: set low exposure time for speed up
        self.errorHandler(
                self.dcam.dcam_setexposuretime(self.camera_handle,
                                               cp.c_double(exptime)),
                                               "dcam_setexposuretime")
    
    def dcam_settriggermode(self,trigmode):
        self.errorHandler(
                self.dcam.dcam_settriggermode(self.camera_handle,
                                              cp.c_int32(trigmode)),
                                              "dcam_settriggermode")
    
    def dcam_settriggerpolarity(self,trigpol):
        self.errorHandler(
                self.dcam.dcam_settriggerpolarity(self.camera_handle,
                                                  cp.c_int32(trigpol)),
                                                  "dcam_settriggerpolarity")
    
    def dcam_extended_subarray_inq(self,binning):
        sub_array_inq=DCAM_PARAM_SUBARRAY_INQ(DCAM_HDR_PARAM())
        sub_array_inq.hdr.cbSize=cp.sizeof(sub_array_inq)
        sub_array_inq.hdr.id=DCAM_IDPARAM_SUBARRAY_INQ
        sub_array_inq.binning=binning
        self.errorHandler(
                self.dcam.dcam_extended(self.camera_handle,
                                        DCAM_IDMSG_GETPARAM,
                                        cp.byref(sub_array_inq),
                                        cp.sizeof(DCAM_PARAM_SUBARRAY_INQ)),
                                        "dcam_extended_subarray_inq")
        return sub_array_inq

    def dcam_extended_subarray_getROI(self):
        sub_array=DCAM_PARAM_SUBARRAY(DCAM_HDR_PARAM())
        sub_array.hdr.cbSize=cp.sizeof(sub_array)
        sub_array.hdr.id=DCAM_IDPARAM_SUBARRAY

        self.errorHandler(
                self.dcam.dcam_extended(self.camera_handle,
                                        DCAM_IDMSG_GETPARAM,
                                        cp.byref(sub_array),
                                        cp.sizeof(DCAM_PARAM_SUBARRAY)),
                                        "dcam_extended_subarray_getROI")
        return sub_array
    
    def dcam_extended_subarray_setROI(self,x,y,len_x,len_y,binning):
        sub_array_inq=self.dcam_extended_subarray_inq(binning)
        sub_array=DCAM_PARAM_SUBARRAY(DCAM_HDR_PARAM())
        sub_array.hdr.cbSize=cp.sizeof(sub_array)
        sub_array.hdr.id=DCAM_IDPARAM_SUBARRAY
        
        sub_array.hpos=x-(x%sub_array_inq.hposunit)
        sub_array.vpos=y-(y%sub_array_inq.vposunit)
        sub_array.hsize=len_x-(len_x%sub_array_inq.hunit)
        sub_array.vsize=len_y-(len_y%sub_array_inq.hunit)
    
        self.errorHandler(
                self.dcam.dcam_extended(self.camera_handle,
                                        DCAM_IDMSG_SETPARAM,
                                        cp.byref(sub_array),
                                        cp.sizeof(DCAM_PARAM_SUBARRAY)),
                                        "dcam_extended_subarray_setROI")
    def dcam_precapure(self):
        self.errorHandler(
                self.dcam.dcam_precapture(self.camera_handle,
                                          cp.c_int(self.capture_mode)),
                                          "dcam_precapture")
                
    def dcam_allocframe(self):
        self.errorHandler(
                self.dcam.dcam_allocframe(self.camera_handle,
                                          cp.c_int32(200)), # TODO: try less for speed up
                                          "dcam_allocframe")

    def dcam_capture(self):
        self.errorHandler(
                self.dcam.dcam_capture(self.camera_handle),
                                       "dcam_capture")
    
    def dcam_wait(self):
        wait=cp.c_int(DCAMCAP_EVENT_FRAMEREADY)
        self.errorHandler(
                self.dcam.dcam_wait(self.camera_handle,
                                    cp.byref(wait),
                                    cp.c_int(DCAMWAIT_TIMEOUT_INFINITE),
                                    None),
                                    "dcam_wait")
                
    def dcam_gettransferinfo(self):
        buffer_indx = cp.c_int32(0)
        frame_count = cp.c_int32(0)
        self.errorHandler(
                self.dcam.dcam_gettransferinfo(self.camera_handle,
                                               cp.byref(buffer_indx),
                                               cp.byref(frame_count)),
                                               "dcam_gettransferinfo")
        return buffer_indx.value, frame_count.value
    
    def dcam_lockdata(self,frame):
        buffer_pointer = cp.c_void_p(0)
        row_bytes = cp.c_int32(0)
        self.errorHandler(
                self.dcam.dcam_lockdata(self.camera_handle,
                                        cp.byref(buffer_pointer),
                                        cp.byref(row_bytes),
                                        cp.c_int32(frame)),
                                        "dcam_lockdata")
        return buffer_pointer
    
    def dcam_unlockdata(self):
        self.errorHandler(
                self.dcam.dcam_unlockdata(self.camera_handle),
                                          "dcam_unlockdata")
        
    def dcam_idle(self):
        self.errorHandler(
                self.dcam.dcam_idle(self.camera_handle),
                                    "dcam_idle")
        
    def dcam_freeframe(self):
        self.errorHandler(
                self.dcam.dcam_freeframe(self.camera_handle),
                                         "dcam_freeframe")
    
    def getdata(self,frame):
        data_frame_bytes=self.dcam_getdataframebytes()
        array = np.empty((int(data_frame_bytes/2), 1), dtype=np.uint16)
        buffer_pointer=self.dcam_lockdata(frame)
        cp.memmove(array.ctypes.data, buffer_pointer, data_frame_bytes)
        self.dcam_unlockdata()
        return array
        
                
    def run(self):
        buffer_index = -1
        self.capture_mode=0
        
        n_buffers = int(2.0*self.dcam_getdataframebytes())
        
        self.dcam_precapure()
        self.dcam_allocframe()
        self.dcam_capture()
        self.dcam_wait()
        
        buffer_indx, frame_count=self.dcam_gettransferinfo()
        
        new_frames = []
        if buffer_indx < buffer_index:
            for i in range(buffer_index + 1, n_buffers):
                new_frames.append(i)
            for i in range(buffer_indx + 1):
                new_frames.append(i)
        else:
            for i in range(buffer_index, buffer_indx):
                new_frames.append(i+1)
        buffer_index = buffer_index
        
        frames = []
        for frame in new_frames:
            array=self.getdata(frame)
            frames.append(array)
        return frames


def test_running_time_binning_4():
    dcamapi=DCAMAPI()
    dcamapi.dcam_init()
    dcamapi.dcam_open()
    dcamapi.dcam_setbinning(4)
    cap_time=[]
    for _ in range(100):
        start=time.time()
        dcamapi.run()
        stop=time.time()
        cap_time.append(stop-start)
        dcamapi.dcam_idle()
        dcamapi.dcam_freeframe()
    dcamapi.dcam_close()
    print('\nBinning 4x4 and full frame')
    print('Excluding freeframe and close camera')
    print('mean capture time (100 iter)',np.mean(cap_time))
    print('std capure time (100 iter)',np.std(cap_time))


def test_running_time_binning_1():
    dcamapi=DCAMAPI()
    dcamapi.dcam_init()
    dcamapi.dcam_open()
    dcamapi.dcam_setbinning(1)
    cap_time=[]
    for _ in range(100):
        start=time.time()
        dcamapi.run()
        stop=time.time()
        cap_time.append(stop-start)
        dcamapi.dcam_idle()
        dcamapi.dcam_freeframe()
    dcamapi.dcam_close()
    print('\nBinning 1x1 and full frame')
    print('Excluding freeframe and close camera')
    print('mean capture time (100 iter)',np.mean(cap_time))
    print('std capture time (100 iter)',np.std(cap_time))

def test_running_time_subarray_binning_4():
    import matplotlib.pyplot as plt
    dcamapi=DCAMAPI()
    dcamapi.dcam_init()
    dcamapi.dcam_open()
    dcamapi.dcam_setbinning(4)
    cap_time=np.zeros((16,16,10),dtype=float)
    for b in range(10):
        l=0
        for i in range(0,124,8):
            k=0
            for j in range(0,124,8):
                dcamapi.dcam_extended_subarray_setROI(i,j,4,4,4)
                start=time.time()
                dcamapi.run()
                stop=time.time()
                cap_time[l,k,b]=1e3*(stop-start)
                dcamapi.dcam_idle()
                dcamapi.dcam_freeframe()
                k+=1
            l+=1
    dcamapi.dcam_close()
    print('\nBinning 4x4 and subarray 4x4')
    print('Excluding freeframe and close camera')
    print('mean capture time (10 iter)')
    plt.figure()
    plt.imshow(np.mean(cap_time,axis=2))
    plt.colorbar()
    plt.show()
    print('std capture time (10 iter)')
    plt.figure()
    plt.imshow(np.std(cap_time,axis=2))
    plt.colorbar()
    plt.show()

def test_display_captured_image():
    import matplotlib.pyplot as plt
    dcamapi=DCAMAPI()
    dcamapi.dcam_init()
    dcamapi.dcam_open()
    dcamapi.dcam_setbinning(4)
    frames=dcamapi.run() 
    dcamapi.dcam_close()
    plt.imshow(frames[0].reshape(128,128))
    
def test_display_captured_image_subarray():
    #import matplotlib.pyplot as plt
    dcamapi=DCAMAPI()
    dcamapi.dcam_init()
    dcamapi.dcam_open()
    dcamapi.dcam_setbinning(4)
    dcamapi.dcam_extended_subarray_setROI(0,0,4,4,4)
    dcamapi.run() 
    dcamapi.dcam_close()
    #plt.imshow(frames[0].reshape(128,128))

def test_running_time_binning_1_ext():
    import matplotlib.pyplot as plt
    dcamapi=DCAMAPI()
    print(dcamapi)
    dcamapi.dcam_init()
    dcamapi.dcam_open()
    dcamapi.dcam_setbinning(1)
    print(dcamapi.dcam_gettriggermode())
    dcamapi.dcam_settriggermode(2)
    print('waiting for trigger')
    pic=dcamapi.run()
    dcamapi.dcam_idle()
    dcamapi.dcam_freeframe()
    dcamapi.dcam_close()
    return pic
    
if __name__=='__main__':
    pic=test_running_time_binning_1_ext()
    #test_running_time_binning_4()
    #test_running_time_subarray_binning_4()
    #test_display_captured_image_subarray()

    

























