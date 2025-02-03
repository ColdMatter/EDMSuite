' dcamapi4.vb: Aug 5, 2024

Imports System.Runtime.InteropServices
Imports System.Text

Namespace Hamamatsu
    Public Class DCAM4

        ' error code in DCAM-API
        Public Enum DCAMERR
            BUSY                                = &H80000101    ' API cannot process in busy state.
            NOTREADY                            = &H80000103    ' API requires ready state.
            NOTSTABLE                           = &H80000104    ' API requires stable or unstable state.
            UNSTABLE                            = &H80000105    ' API does not support in unstable state.
            NOTBUSY                             = &H80000107    ' API requires busy state.
            EXCLUDED                            = &H80000110    ' some resource is exclusive and already used
            COOLINGTROUBLE                      = &H80000302    ' something happens near cooler
            NOTRIGGER                           = &H80000303    ' no trigger when necessary. Some camera supports this error.
            TEMPERATURE_TROUBLE                 = &H80000304    ' camera warns its temperature
            TOOFREQUENTTRIGGER                  = &H80000305    ' input too frequent trigger. Some camera supports this error.
            ABORT                               = &H80000102    ' abort process
            TIMEOUT                             = &H80000106    ' timeout
            LOSTFRAME                           = &H80000301    ' frame data is lost
            MISSINGFRAME_TROUBLE                = &H80000f06    ' frame is lost but reason is low lever driver's bug
            INVALIDIMAGE                        = &H80000321    ' hpk format data is invalid data
            NORESOURCE                          = &H80000201    ' not enough resource except memory
            NOMEMORY                            = &H80000203    ' not enough memory
            NOMODULE                            = &H80000204    ' no sub module
            NODRIVER                            = &H80000205    ' no driver
            NOCAMERA                            = &H80000206    ' no camera
            NOGRABBER                           = &H80000207    ' no grabber
            NOCOMBINATION                       = &H80000208    ' no combination on registry
            FAILOPEN                            = &H80001001    ' DEPRECATED
            FRAMEGRABBER_NEEDS_FIRMWAREUPDATE   = &H80001002    ' need to update frame grabber firmware to use the camera
            INVALIDMODULE                       = &H80000211    ' dcam_init() found invalid module
            INVALIDCOMMPORT                     = &H80000212    ' invalid serial port
            FAILOPENBUS                         = &H81001001    ' the bus or driver are not available
            FAILOPENCAMERA                      = &H82001001    ' camera report error during opening
            DEVICEPROBLEM                       = &H82001002    ' initialization failed(for maico)
            INVALIDCAMERA                       = &H80000806    ' invalid camera
            INVALIDHANDLE                       = &H80000807    ' invalid camera handle
            INVALIDPARAM                        = &H80000808    ' invalid parameter
            INVALIDVALUE                        = &H80000821    ' invalid property value
            OUTOFRANGE                          = &H80000822    ' value is out of range
            NOTWRITABLE                         = &H80000823    ' the property is not writable
            NOTREADABLE                         = &H80000824    ' the property is not readable
            INVALIDPROPERTYID                   = &H80000825    ' the property id is invalid
            NEWAPIREQUIRED                      = &H80000826    ' old API cannot present the value because only new API need to be used
            WRONGHANDSHAKE                      = &H80000827    ' this error happens DCAM get error code from camera unexpectedly
            NOPROPERTY                          = &H80000828    ' there is no altenative or influence id, or no more property id
            INVALIDCHANNEL                      = &H80000829    ' the property id specifies channel but channel is invalid
            INVALIDVIEW                         = &H8000082a    ' the property id specifies channel but channel is invalid
            INVALIDSUBARRAY                     = &H8000082b    ' the combination of subarray values are invalid. e.g. DCAM_IDPROP_SUBARRAYHPOS + DCAM_IDPROP_SUBARRAYHSIZE is greater than the number of horizontal pixel of sensor.
            ACCESSDENY                          = &H8000082c    ' the property cannot access during this DCAM STATUS
            NOVALUETEXT                         = &H8000082d    ' the property does not have value text
            WRONGPROPERTYVALUE                  = &H8000082e    ' at least one property value is wrong
            DISHARMONY                          = &H80000830    ' the paired camera does not have same parameter
            FRAMEBUNDLESHOULDBEOFF              = &H80000832    ' framebundle mode should be OFF under current property settings
            INVALIDFRAMEINDEX                   = &H80000833    ' the frame index is invalid
            INVALIDSESSIONINDEX                 = &H80000834    ' the session index is invalid
            NOCORRECTIONDATA                    = &H80000838    ' not take the dark and shading correction data yet.
            CHANNELDEPENDENTVALUE               = &H80000839    ' each channel has own property value so can't return overall property value.
            VIEWDEPENDENTVALUE                  = &H8000083a    ' each view has own property value so can't return overall property value.
            NODEVICEBUFFER                      = &H8000083b    ' the frame count is larger than device momory size on using device memory.
            REQUIREDSNAP                        = &H8000083c    ' the capture mode is sequence on using device memory.
            LESSSYSTEMMEMORY                    = &H8000083f    ' the sysmte memory size is too small. PC doesn't have enough memory or is limited memory by 32bit OS.
            INVALID_SELECTEDLINES               = &H80000842    ' the combination of selected lines values are invalid. e.g. DCAM_IDPROP_SELECTEDLINES_VPOS + DCAM_IDPROP_SELECTEDLINES_VSIZE is greater than the number of vertical lines of sensor.
            NOTSUPPORT                          = &H80000f03    ' camera does not support the function or property with current settings
            FAILREADCAMERA                      = &H83001002    ' failed to read data from camera
            FAILWRITECAMERA                     = &H83001003    ' failed to write data to the camera
            CONFLICTCOMMPORT                    = &H83001004    ' conflict the com port name user set
            OPTICS_UNPLUGGED                    = &H83001005    ' Optics part is unplugged so please check it.
            FAILCALIBRATION                     = &H83001006    ' fail calibration
            MISMATCH_CONFIGURATION              = &H83001011    ' mismatch between camera output(connection) and frame grabber specs
            INVALIDMEMBER_3                     = &H84000103    ' 3th member variable is invalid value
            INVALIDMEMBER_5                     = &H84000105    ' 5th member variable is invalid value
            INVALIDMEMBER_7                     = &H84000107    ' 7th member variable is invalid value
            INVALIDMEMBER_8                     = &H84000108    ' 7th member variable is invalid value
            INVALIDMEMBER_9                     = &H84000109    ' 9th member variable is invalid value
            FAILEDOPENRECFILE                   = &H84001001    ' DCAMREC failed to open the file
            INVALIDRECHANDLE                    = &H84001002    ' DCAMREC is invalid handle
            FAILEDWRITEDATA                     = &H84001003    ' DCAMREC failed to write the data
            FAILEDREADDATA                      = &H84001004    ' DCAMREC failed to read the data
            NOWRECORDING                        = &H84001005    ' DCAMREC is recording data now
            WRITEFULL                           = &H84001006    ' DCAMREC writes full frame of the session
            ALREADYOCCUPIED                     = &H84001007    ' DCAMREC handle is already occupied by other HDCAM
            TOOLARGEUSERDATASIZE                = &H84001008    ' DCAMREC is set the large value to user data size
            INVALIDWAITHANDLE                   = &H84002001    ' DCAMWAIT is invalid handle
            NEWRUNTIMEREQUIRED                  = &H84002002    ' DCAM Module Version is older than the version that the camera requests
            VERSIONMISMATCH                     = &H84002003    ' Camre returns the error on setting parameter to limit version
            RUNAS_FACTORYMODE                   = &H84002004    ' Camera is running as a factory mode
            IMAGE_UNKNOWNSIGNATURE              = &H84003001    ' sigunature of image header is unknown or corrupted
            IMAGE_NEWRUNTIMEREQUIRED            = &H84003002    ' version of image header is newer than version that used DCAM supports
            IMAGE_ERRORSTATUSEXIST              = &H84003003    ' image header stands error status
            IMAGE_HEADERCORRUPTED               = &H84004004    ' image header value is strange
            IMAGE_BROKENCONTENT                 = &H84004005    ' image content is corrupted
            UNKNOWNMSGID                        = &H80000801    ' unknown message id
            UNKNOWNSTRID                        = &H80000802    ' unknown string id
            UNKNOWNPARAMID                      = &H80000803    ' unkown parameter id
            UNKNOWNBITSTYPE                     = &H80000804    ' unknown bitmap bits type
            UNKNOWNDATATYPE                     = &H80000805    ' unknown frame data type
            INSTALLATIONINPROGRESS              = &H80000f00    ' installation progress
            UNREACH                             = &H80000f01    ' internal error
            UNLOADED                            = &H80000f04    ' calling after process terminated
            THRUADAPTER                         = &H80000f05    
            NOCONNECTION                        = &H80000f07    ' HDCAM lost connection to camera
            NOTIMPLEMENT                        = &H80000f02    ' not yet implementation
            DELAYEDFRAME                        = &H80000f09    ' the frame waiting re-load from hardware buffer with SNAPSHOT(EX) of DEVICEBUFFER MODE
            FAILRELOADFRAME                     = &H80000f0a    ' failed to re-load frame from hardware buffer with SNAPSHOT(EX) of DEVICEBUFFER MODE
            CANCELRELOADFRAME                   = &H80000f0b    ' cancel to re-load frame from hardware buffer with SNAPSHOT(EX) of DEVICEBUFFER MODE
            DEVICEINITIALIZING                  = &Hb0000001    
            APIINIT_INITOPTIONBYTES             = &Ha4010003    ' DCAMAPI_INIT::initoptionbytes is invalid
            APIINIT_INITOPTION                  = &Ha4010004    ' DCAMAPI_INIT::initoption is invalid
            INITOPTION_COLLISION_BASE           = &Ha401C000    
            INITOPTION_COLLISION_MAX            = &Ha401FFFF    
            MISSPROP_TRIGGERSOURCE              = &HE0100110    ' the trigger mode is internal or syncreadout on using device memory.

        End Enum

        Public Enum DCAM_IDSTR
            BUS                 = &H04000101    
            CAMERAID            = &H04000102    
            VENDOR              = &H04000103    
            MODEL               = &H04000104    
            CAMERAVERSION       = &H04000105    
            DRIVERVERSION       = &H04000106    
            MODULEVERSION       = &H04000107    
            DCAMAPIVERSION      = &H04000108    
            SUBUNIT_INFO1       = &H04000110    
            SUBUNIT_INFO2       = &H04000111    
            SUBUNIT_INFO3       = &H04000112    
            SUBUNIT_INFO4       = &H04000113    
            CAMERA_SERIESNAME   = &H0400012c    

        End Enum

        Public Enum DCAM_PIXELTYPE
            MONO8   = &H00000001    
            MONO16  = &H00000002    
            MONO12  = &H00000003    
            MONO12P = &H00000005    
            RGB24   = &H00000021    
            RGB48   = &H00000022    
            BGR24   = &H00000029    
            BGR48   = &H0000002a    
            NONE    = &H00000000    

        End Enum


        Public Enum DCAMDEV_CAPDOMAIN
            DCAMDATA    = &H00000001    
            FRAMEOPTION = &H00000002    
            _FUNCTION   = &H00000000    

        End Enum


        ' Property ID for dcamprop functions
        Public Enum DCAMIDPROP
            TRIGGERSOURCE                       = &H00100110    ' R/W, mode,    "TRIGGER SOURCE"
            TRIGGERACTIVE                       = &H00100120    ' R/W, mode,    "TRIGGER ACTIVE"
            TRIGGER_MODE                        = &H00100210    ' R/W, mode,    "TRIGGER MODE"
            TRIGGERPOLARITY                     = &H00100220    ' R/W, mode,    "TRIGGER POLARITY"
            TRIGGER_CONNECTOR                   = &H00100230    ' R/W, mode,    "TRIGGER CONNECTOR"
            TRIGGERTIMES                        = &H00100240    ' R/W, long,    "TRIGGER TIMES"
            TRIGGERDELAY                        = &H00100260    ' R/W, sec, "TRIGGER DELAY"
            INTERNALTRIGGER_HANDLING            = &H00100270    
            TRIGGERMULTIFRAME_COUNT             = &H00100280    
            SYNCREADOUT_SYSTEMBLANK             = &H00100290    ' R/W, mode,    "SYNC READOUT SYSTEM BLANK"
            TRIGGERENABLE_ACTIVE                = &H00100410    ' R/W, mode,    "TRIGGER ENABLE ACTIVE"
            TRIGGERENABLE_POLARITY              = &H00100420    
            TRIGGERENABLE_SOURCE                = &H00100430    ' R/W, mode,    "TRIGGER ENABLE SOURCE"
            TRIGGERENABLE_BURSTTIMES            = &H00100440    ' R/W, mode,    "TRIGGER ENABLE BURST TIMES"
            TRIGGERNUMBER_FORFIRSTIMAGE         = &H00100810    ' R/O, long,    "TRIGGER NUMBER FOR FIRST IMAGE"
            TRIGGERNUMBER_FORNEXTIMAGE          = &H00100820    ' R/O, long,    "TRIGGER NUMBER FOR NEXT IMAGE"
            NUMBEROF_OUTPUTTRIGGERCONNECTOR     = &H001C0010    
            OUTPUTTRIGGER_CHANNELSYNC           = &H001C0030    ' R/W, mode,    "OUTPUT TRIGGER CHANNEL SYNC"
            OUTPUTTRIGGER_PROGRAMABLESTART      = &H001C0050    ' R/W, mode,    "OUTPUT TRIGGER PROGRAMABLE START"
            OUTPUTTRIGGER_SOURCE                = &H001C0110    ' R/W, mode,    "OUTPUT TRIGGER SOURCE"
            OUTPUTTRIGGER_POLARITY              = &H001C0120    ' R/W, mode,    "OUTPUT TRIGGER POLARITY"
            OUTPUTTRIGGER_ACTIVE                = &H001C0130    ' R/W, mode,    "OUTPUT TRIGGER ACTIVE"
            OUTPUTTRIGGER_DELAY                 = &H001C0140    ' R/W, sec, "OUTPUT TRIGGER DELAY"
            OUTPUTTRIGGER_PERIOD                = &H001C0150    ' R/W, sec, "OUTPUT TRIGGER PERIOD"
            OUTPUTTRIGGER_KIND                  = &H001C0160    ' R/W, mode,    "OUTPUT TRIGGER KIND"
            OUTPUTTRIGGER_BASESENSOR            = &H001C0170    ' R/W, mode,    "OUTPUT TRIGGER BASE SENSOR"
            OUTPUTTRIGGER_PREHSYNCCOUNT         = &H001C0190    ' R/W, mode,    "OUTPUT TRIGGER PRE HSYNC COUNT"
            _OUTPUTTRIGGER                      = &H00000100    ' the offset of ID for Nth OUTPUT TRIGGER parameter
            MASTERPULSE_MODE                    = &H001E0020    ' R/W, mode,    "MASTER PULSE MODE"
            MASTERPULSE_TRIGGERSOURCE           = &H001E0030    ' R/W, mode,    "MASTER PULSE TRIGGER SOURCE"
            MASTERPULSE_INTERVAL                = &H001E0040    ' R/W, sec, "MASTER PULSE INTERVAL"
            MASTERPULSE_BURSTTIMES              = &H001E0050    ' R/W, long,    "MASTER PULSE BURST TIMES"
            EXPOSURETIME                        = &H001F0110    ' R/W, sec, "EXPOSURE TIME"
            EXPOSURETIME_CONTROL                = &H001F0130    ' R/W, mode,    "EXPOSURE TIME CONTROL"
            TRIGGER_FIRSTEXPOSURE               = &H001F0200    ' R/W, mode,    "TRIGGER FIRST EXPOSURE"
            TRIGGER_GLOBALEXPOSURE              = &H001F0300    ' R/W, mode,    "TRIGGER GLOBAL EXPOSURE"
            FIRSTTRIGGER_BEHAVIOR               = &H001F0310    ' R/W, mode,    "FIRST TRIGGER BEHAVIOR"
            MULTIFRAME_EXPOSURE                 = &H001F1000    ' R/W, sec, "MULTI FRAME EXPOSURE TIME"
            _MULTIFRAME                         = &H00000010    ' the offset of ID for Nth MULTIFRAME
            LIGHTMODE                           = &H00200110    ' R/W, mode,    "LIGHT MODE"
            SENSITIVITYMODE                     = &H00200210    ' R/W, mode,    "SENSITIVITY MODE"
            SENSITIVITY                         = &H00200220    ' R/W, long,    "SENSITIVITY"
            DIRECTEMGAIN_MODE                   = &H00200250    ' R/W, mode,    "DIRECT EM GAIN MODE"
            EMGAINWARNING_STATUS                = &H00200260    
            EMGAINWARNING_LEVEL                 = &H00200270    ' R/W, long,    "EM GAIN WARNING LEVEL"
            EMGAINWARNING_ALARM                 = &H00200280    ' R/W, mode,    "EM GAIN WARNING ALARM"
            EMGAINPROTECT_MODE                  = &H00200290    ' R/W, mode,    "EM GAIN PROTECT MODE"
            EMGAINPROTECT_AFTERFRAMES           = &H002002A0    ' R/W, long,    "EM GAIN PROTECT AFTER FRAMES"
            MEASURED_SENSITIVITY                = &H002002B0    ' R/O, real,    "MEASURED SENSITIVITY"
            PHOTONIMAGINGMODE                   = &H002002F0    ' R/W, mode,    "PHOTON IMAGING MODE"
            SENSORTEMPERATURE                   = &H00200310    ' R/O, celsius,"SENSOR TEMPERATURE"
            SENSORCOOLER                        = &H00200320    ' R/W, mode,    "SENSOR COOLER"
            SENSORTEMPERATURETARGET             = &H00200330    ' R/W, celsius,"SENSOR TEMPERATURE TARGET"
            SENSORCOOLERSTATUS                  = &H00200340    ' R/O, mode,    "SENSOR COOLER STATUS"
            SENSORCOOLERFAN                     = &H00200350    ' R/W, mode,    "SENSOR COOLER FAN"
            SENSORTEMPERATURE_AVE               = &H00200360    ' R/O, celsius,"SENSOR TEMPERATURE AVE"
            SENSORTEMPERATURE_MIN               = &H00200370    ' R/O, celsius,"SENSOR TEMPERATURE MIN"
            SENSORTEMPERATURE_MAX               = &H00200380    ' R/O, celsius,"SENSOR TEMPERATURE MAX"
            SENSORTEMPERATURE_STATUS            = &H00200390    ' R/O, mode,    "SENSOR TEMPERATURE STATUS"
            SENSORTEMPERATURE_PROTECT           = &H00200400    ' R/W, mode,    "SENSOR TEMPERATURE MODE"
            MECHANICALSHUTTER                   = &H00200410    ' R/W, mode,    "MECHANICAL SHUTTER"
            CONTRASTGAIN                        = &H00300120    ' R/W, long,    "CONTRAST GAIN"
            CONTRASTOFFSET                      = &H00300130    ' R/W, long,    "CONTRAST OFFSET"
            HIGHDYNAMICRANGE_MODE               = &H00300150    ' R/W, mode,    "HIGH DYNAMIC RANGE MODE"
            DIRECTGAIN_MODE                     = &H00300160    ' R/W, mode,    "DIRECT GAIN MODE"
            REALTIMEGAINCORRECT_MODE            = &H00300170    ' R/W,  mode,   "REALTIME GAIN CORRECT MODE"
            REALTIMEGAINCORRECT_LEVEL           = &H00300180    ' R/W,  mode,   "REALTIME GAIN CORRECT LEVEL"
            REALTIMEGAINCORRECT_INTERVAL        = &H00300190    ' R/W,  mode,   "REALTIME GAIN CORRECT INTERVAL"
            NUMBEROF_REALTIMEGAINCORRECTREGION  = &H003001A0    
            VIVIDCOLOR                          = &H00300200    ' R/W, mode,    "VIVID COLOR"
            WHITEBALANCEMODE                    = &H00300210    ' R/W, mode,    "WHITEBALANCE MODE"
            WHITEBALANCETEMPERATURE             = &H00300220    ' R/W, color-temp., "WHITEBALANCE TEMPERATURE"
            WHITEBALANCEUSERPRESET              = &H00300230    ' R/W, long,    "WHITEBALANCE USER PRESET"
            REALTIMEGAINCORRECTREGION_HPOS      = &H00301000    ' R/W,  long,   "REALTIME GAIN CORRECT REGION HPOS"
            REALTIMEGAINCORRECTREGION_HSIZE     = &H00302000    ' R/W,  long,   "REALTIME GAIN CORRECT REGION HSIZE"
            _REALTIMEGAINCORRECTIONREGION       = &H00000010    ' the offset of ID for Nth REALTIME GAIN CORRECT REGION parameter
            INTERFRAMEALU_ENABLE                = &H00380010    ' R/W, mode,    "INTERFRAME ALU ENABLE"
            RECURSIVEFILTER                     = &H00380110    ' R/W, mode,    "RECURSIVE FILTER"
            RECURSIVEFILTERFRAMES               = &H00380120    
            SPOTNOISEREDUCER                    = &H00380130    ' R/W, mode,    "SPOT NOISE REDUCER"
            SUBTRACT                            = &H00380210    ' R/W, mode,    "SUBTRACT"
            SUBTRACTIMAGEMEMORY                 = &H00380220    ' R/W, mode,    "SUBTRACT IMAGE MEMORY"
            STORESUBTRACTIMAGETOMEMORY          = &H00380230    ' W/O, mode,    "STORE SUBTRACT IMAGE TO MEMORY"
            SUBTRACTOFFSET                      = &H00380240    ' R/W, long "SUBTRACT OFFSET"
            DARKCALIB_STABLEMAXINTENSITY        = &H00380250    ' R/W, long,    "DARKCALIB STABLE MAX INTENSITY"
            SUBTRACT_DATASTATUS                 = &H003802F0    ' R/W   mode,   "SUBTRACT DATA STATUS"
            SHADINGCALIB_DATASTATUS             = &H00380300    ' R/W   mode,   "SHADING CALIB DATA STATUS"
            SHADINGCORRECTION                   = &H00380310    ' R/W, mode,    "SHADING CORRECTION"
            SHADINGCALIBDATAMEMORY              = &H00380320    ' R/W, mode,    "SHADING CALIB DATA MEMORY"
            STORESHADINGCALIBDATATOMEMORY       = &H00380330    ' W/O, mode,    "STORE SHADING DATA TO MEMORY"
            SHADINGCALIB_METHOD                 = &H00380340    ' R/W, mode,    "SHADING CALIB METHOD"
            SHADINGCALIB_TARGET                 = &H00380350    ' R/W, long,    "SHADING CALIB TARGET"
            SHADINGCALIB_STABLEMININTENSITY     = &H00380360    ' R/W, long,    "SHADING CALIB STABLE MIN INTENSITY"
            SHADINGCALIB_SAMPLES                = &H00380370    ' R/W, long,    "SHADING CALIB SAMPLES"
            SHADINGCALIB_STABLESAMPLES          = &H00380380    ' R/W, long,    "SHADING CALIB STABLE SAMPLES"
            SHADINGCALIB_STABLEMAXERRORPERCENT  = &H00380390    ' R/W, long,    "SHADING CALIB STABLE MAX ERROR PERCENT"
            FRAMEAVERAGINGMODE                  = &H003803A0    ' R/W, mode,    "FRAME AVERAGING MODE"
            FRAMEAVERAGINGFRAMES                = &H003803B0    
            DARKCALIB_STABLESAMPLES             = &H003803C0    ' R/W, long,    "DARKCALIB STABLE SAMPLES"
            DARKCALIB_SAMPLES                   = &H003803D0    ' R/W, long,    "DARKCALIB SAMPLES"
            DARKCALIB_TARGET                    = &H003803E0    ' R/W, long,    "DARKCALIB TARGET"
            CAPTUREMODE                         = &H00380410    ' R/W, mode,    "CAPTURE MODE"
            LINEAVERAGING                       = &H00380450    ' R/W, long,    "LINE AVERAGING"
            IMAGEFILTER                         = &H00380460    ' R/W, mode,    "IMAGE FILTER"
            INTENSITYLUT_MODE                   = &H00380510    ' R/W, mode,    "INTENSITY LUT MODE"
            INTENSITYLUT_PAGE                   = &H00380520    ' R/W, long,    "INTENSITY LUT PAGE"
            INTENSITYLUT_WHITECLIP              = &H00380530    ' R/W, long,    "INTENSITY LUT WHITE CLIP"
            INTENSITYLUT_BLACKCLIP              = &H00380540    ' R/W, long,    "INTENSITY LUT BLACK CLIP"
            INTENSITY_GAMMA                     = &H00380560    ' R/W, real,    "INTENSITY GAMMA"
            SENSORGAPCORRECT_MODE               = &H00380620    ' R/W, long,    "SENSOR GAP CORRECT MODE"
            ADVANCEDEDGEENHANCEMENT_MODE        = &H00380630    ' R/W, mode,    "ADVANCED EDGE ENHANCEMENT MODE"
            ADVANCEDEDGEENHANCEMENT_LEVEL       = &H00380640    ' R/W, long,    "ADVANCED EDGE ENHANCEMENT LEVEL"
            SHADINGCALIB_TARGETMIN              = &H00380680    ' R/W, long,    "SHADING CALIB TARGET MIN"
            TAPGAINCALIB_METHOD                 = &H00380F10    ' R/W, mode,    "TAP GAIN CALIB METHOD"
            TAPCALIB_BASEDATAMEMORY             = &H00380F20    
            STORETAPCALIBDATATOMEMORY           = &H00380F30    
            TAPCALIBDATAMEMORY                  = &H00380F40    ' W/O, mode,    "TAP CALIB DATA MEMORY"
            NUMBEROF_TAPCALIB                   = &H00380FF0    ' R/W, long,    "NUMBER OF TAP CALIB"
            TAPCALIB_GAIN                       = &H00381000    ' R/W, mode,    "TAP CALIB GAIN"
            TAPCALIB_OFFSET                     = &H00382000    ' R/W, mode,    "TAP CALIB OFFSET"
            _TAPCALIB                           = &H00000010    ' the offset of ID for Nth TAPCALIB
            READOUTSPEED                        = &H00400110    ' R/W, long,    "READOUT SPEED"
            READOUT_DIRECTION                   = &H00400130    ' R/W, mode,    "READOUT DIRECTION"
            READOUT_UNIT                        = &H00400140    ' R/O, mode,    "READOUT UNIT"
            SHUTTER_MODE                        = &H00400150    ' R/W, mode,    "SHUTTER MODE"
            SENSORMODE                          = &H00400210    ' R/W, mode,    "SENSOR MODE"
            SENSORMODE_LINEBUNDLEHEIGHT         = &H00400250    ' R/W, long,    "SENSOR MODE LINE BUNDLEHEIGHT"
            SENSORMODE_PANORAMICSTARTV          = &H00400280    ' R/W, long,    "SENSOR MODE PANORAMIC START V"
            SENSORMODE_TDISTAGE                 = &H00400290    ' R/W,  long,   "SENSOR MODE TDI STAGE"
            CCDMODE                             = &H00400310    ' R/W, mode,    "CCD MODE"
            EMCCD_CALIBRATIONMODE               = &H00400320    ' R/W, mode,    "EM CCD CALIBRATION MODE"
            CMOSMODE                            = &H00400350    ' R/W, mode,    "CMOS MODE"
            MULTILINESENSOR_READOUTMODE         = &H00400380    ' R/W, mode,    "MULTI LINE SENSOR READOUT MODE"
            MULTILINESENSOR_TOP                 = &H00400390    ' R/W, long,    "MULTI LINE SENSOR TOP"
            MULTILINESENSOR_HEIGHT              = &H004003A0    ' R/W, long,    "MULTI LINE SENSOR HEIGHT"
            OUTPUT_INTENSITY                    = &H00400410    ' R/W, mode,    "OUTPUT INTENSITY"
            OUTPUTDATA_OPERATION                = &H00400440    ' R/W, mode,    "OUTPUT DATA OPERATION"
            TESTPATTERN_KIND                    = &H00400510    ' R/W, mode,    "TEST PATTERN KIND"
            TESTPATTERN_OPTION                  = &H00400520    ' R/W, long,    "TEST PATTERN OPTION"
            EXTRACTION_MODE                     = &H00400620    
            BURIEDDATA_MODE                     = &H00400A00    ' R/W, mode,    "BURIED DATA MODE"
            BINNING                             = &H00401110    ' R/W, mode,    "BINNING"
            BINNING_INDEPENDENT                 = &H00401120    ' R/W, mode,    "BINNING INDEPENDENT"
            BINNING_HORZ                        = &H00401130    ' R/W, long,    "BINNING HORZ"
            BINNING_VERT                        = &H00401140    ' R/W, long,    "BINNING VERT"
            SUBARRAYHPOS                        = &H00402110    ' R/W, long,    "SUBARRAY HPOS"
            SUBARRAYHSIZE                       = &H00402120    ' R/W, long,    "SUBARRAY HSIZE"
            SUBARRAYVPOS                        = &H00402130    ' R/W, long,    "SUBARRAY VPOS"
            SUBARRAYVSIZE                       = &H00402140    ' R/W, long,    "SUBARRAY VSIZE"
            SUBARRAYMODE                        = &H00402150    ' R/W, mode,    "SUBARRAY MODE"
            DIGITALBINNING_METHOD               = &H00402160    ' R/W, mode,    "DIGITALBINNING METHOD"
            DIGITALBINNING_HORZ                 = &H00402170    ' R/W, long,    "DIGITALBINNING HORZ"
            DIGITALBINNING_VERT                 = &H00402180    ' R/W, long,    "DIGITALBINNING VERT"
            TIMING_READOUTTIME                  = &H00403010    ' R/O, sec, "TIMING READOUT TIME"
            TIMING_CYCLICTRIGGERPERIOD          = &H00403020    ' R/O, sec, "TIMING CYCLIC TRIGGER PERIOD"
            TIMING_MINTRIGGERBLANKING           = &H00403030    ' R/O, sec, "TIMING MINIMUM TRIGGER BLANKING"
            TIMING_MINTRIGGERINTERVAL           = &H00403050    ' R/O, sec, "TIMING MINIMUM TRIGGER INTERVAL"
            TIMING_EXPOSURE                     = &H00403060    ' R/O, mode,    "TIMING EXPOSURE"
            TIMING_INVALIDEXPOSUREPERIOD        = &H00403070    ' R/O, sec, "INVALID EXPOSURE PERIOD"
            TIMING_FRAMESKIPNUMBER              = &H00403080    ' R/W, long,    "TIMING FRAME SKIP NUMBER"
            TIMING_GLOBALEXPOSUREDELAY          = &H00403090    ' R/O, sec, "TIMING GLOBAL EXPOSURE DELAY"
            INTERNALFRAMERATE                   = &H00403810    ' R/W, 1/sec,   "INTERNAL FRAME RATE"
            INTERNAL_FRAMEINTERVAL              = &H00403820    ' R/W, sec, "INTERNAL FRAME INTERVAL"
            INTERNALLINERATE                    = &H00403830    ' R/W, 1/sec,   "INTERNAL LINE RATE"
            INTERNALLINESPEED                   = &H00403840    ' R/W, m/sec,   "INTERNAL LINE SPEEED"
            INTERNAL_LINEINTERVAL               = &H00403850    ' R/W, sec, "INTERNAL LINE INTERVAL"
            INTERNALLINERATE_CONTROL            = &H00403870    ' R/W, mode,    "INTERNAL LINE RATE CONTROL"
            TIMESTAMP_PRODUCER                  = &H00410A10    ' R/O, mode,    "TIME STAMP PRODUCER"
            FRAMESTAMP_PRODUCER                 = &H00410A20    ' R/O, mode,    "FRAME STAMP PRODUCER"
            TRANSFERINFO_FRAMECOUNT             = &H00410B10    ' R/O, long,    "TRANSFER INFO FRAME COUNT"
            TRANSFERINFO_LOSTCOUNT              = &H00410B11    ' R/O, long,    "TRANSFER INFO LOST COUNT"
            COLORTYPE                           = &H00420120    ' R/W, mode,    "COLORTYPE"
            BITSPERCHANNEL                      = &H00420130    ' R/W, long,    "BIT PER CHANNEL"
            NUMBEROF_CHANNEL                    = &H00420180    ' R/O, long,    "NUMBER OF CHANNEL"
            ACTIVE_CHANNELINDEX                 = &H00420190    ' R/W, mode,    "ACTIVE CHANNEL INDEX"
            NUMBEROF_VIEW                       = &H004201C0    ' R/O, long,    "NUMBER OF VIEW"
            ACTIVE_VIEWINDEX                    = &H004201D0    ' R/W, mode,    "ACTIVE VIEW INDEX"
            IMAGE_WIDTH                         = &H00420210    ' R/O, long,    "IMAGE WIDTH"
            IMAGE_HEIGHT                        = &H00420220    ' R/O, long,    "IMAGE HEIGHT"
            IMAGE_ROWBYTES                      = &H00420230    ' R/O, long,    "IMAGE ROWBYTES"
            IMAGE_FRAMEBYTES                    = &H00420240    ' R/O, long,    "IMAGE FRAMEBYTES"
            IMAGE_TOPOFFSETBYTES                = &H00420250    
            IMAGE_PIXELTYPE                     = &H00420270    ' R/W, DCAM_PIXELTYPE,  "IMAGE PIXEL TYPE"
            IMAGE_CAMERASTAMP                   = &H00420300    ' R/W, long,    "IMAGE CAMERA STAMP"
            RECORDFIXEDBYTES_PERFILE            = &H00420410    ' R/O,  long    "RECORD FIXED BYTES PER FILE"
            RECORDFIXEDBYTES_PERSESSION         = &H00420420    
            RECORDFIXEDBYTES_PERFRAME           = &H00420430    ' R/O,  long    "RECORD FIXED BYTES PER FRAME"
            IMAGEDETECTOR_PIXELWIDTH            = &H00420810    ' R/O, micro-meter, "IMAGE DETECTOR PIXEL WIDTH"
            IMAGEDETECTOR_PIXELHEIGHT           = &H00420820    ' R/O, micro-meter, "IMAGE DETECTOR PIXEL HEIGHT"
            IMAGEDETECTOR_PIXELNUMHORZ          = &H00420830    ' R/O, long,    "IMAGE DETECTOR PIXEL NUM HORZ"
            IMAGEDETECTOR_PIXELNUMVERT          = &H00420840    ' R/O, long,    "IMAGE DETECTOR PIXEL NUM VERT"
            FRAMEBUNDLE_MODE                    = &H00421010    ' R/W, mode,    "FRAMEBUNDLE MODE"
            FRAMEBUNDLE_NUMBER                  = &H00421020    ' R/W, long,    "FRAMEBUNDLE NUMBER"
            FRAMEBUNDLE_ROWBYTES                = &H00421030    ' R/O,  long,   "FRAMEBUNDLE ROWBYTES"
            FRAMEBUNDLE_FRAMESTEPBYTES          = &H00421040    ' R/O, long,    "FRAMEBUNDLE FRAME STEP BYTES"
            NUMBEROF_PARTIALAREA                = &H00430010    
            PARTIALAREA_HPOS                    = &H00431000    ' R/W, long,    "PARTIAL AREA HPOS"
            PARTIALAREA_HSIZE                   = &H00432000    ' R/W, long,    "PARTIAL AREA HSIZE"
            PARTIALAREA_VPOS                    = &H00433000    ' R/W, long,    "PARTIAL AREA VPOS"
            PARTIALAREA_VSIZE                   = &H00434000    ' R/W, long,    "PARTIAL AREA VSIZE"
            _PARTIALAREA                        = &H00000010    ' the offset of ID for Nth PARTIAL AREA
            NUMBEROF_MULTILINE                  = &H0044F010    ' R/W, long,    "NUMBER OF MULTI LINE"
            MULTILINE_VPOS                      = &H00450000    ' R/W, long,    "MULTI LINE VPOS"
            MULTILINE_VSIZE                     = &H00460000    ' R/W, long,    "MULTI LINE VSIZE"
            _MULTILINE                          = &H00000010    ' the offset of ID for Nth MULTI LINE
            DEFECTCORRECT_MODE                  = &H00470010    ' R/W, mode,    "DEFECT CORRECT MODE"
            NUMBEROF_DEFECTCORRECT              = &H00470020    ' R/W, long,    "NUMBER OF DEFECT CORRECT"
            HOTPIXELCORRECT_LEVEL               = &H00470030    ' R/W, mode,    "HOT PIXEL CORRECT LEVEL"
            DEFECTCORRECT_HPOS                  = &H00471000    ' R/W, long,    "DEFECT CORRECT HPOS"
            DEFECTCORRECT_METHOD                = &H00473000    ' R/W, mode,    "DEFECT CORRECT METHOD"
            _DEFECTCORRECT                      = &H00000010    ' the offset of ID for Nth DEFECT
            DEVICEBUFFER_MODE                   = &H00490000    ' R/W, mode,    "DEVICE BUFFER MODE"
            DEVICEBUFFER_FRAMECOUNTMAX          = &H00490020    ' R/O, long,    "DEVICE BUFFER FRAME COUNT MAX"
            CALIBREGION_MODE                    = &H00402410    ' R/W, mode,    "CALIBRATE REGION MODE"
            NUMBEROF_CALIBREGION                = &H00402420    
            CALIBREGION_HPOS                    = &H004B0000    ' R/W, long,    "CALIBRATE REGION HPOS"
            CALIBREGION_HSIZE                   = &H004B1000    ' R/W, long,    "CALIBRATE REGION HSIZE"
            _CALIBREGION                        = &H00000010    ' the offset of ID for Nth REGION
            MASKREGION_MODE                     = &H00402510    ' R/W, mode,    "MASK REGION MODE"
            NUMBEROF_MASKREGION                 = &H00402520    ' R/W, long,    "NUMBER OF MASK REGION"
            MASKREGION_HPOS                     = &H004C0000    ' R/W, long,    "MASK REGION HPOS"
            MASKREGION_HSIZE                    = &H004C1000    ' R/W, long,    "MASK REGION HSIZE"
            _MASKREGION                         = &H00000010    ' the offset of ID for Nth REGION
            CAMERASTATUS_INTENSITY              = &H004D1110    ' R/O, mode,    "CAMERASTATUS INTENSITY"
            CAMERASTATUS_INPUTTRIGGER           = &H004D1120    
            CAMERASTATUS_CALIBRATION            = &H004D1130    ' R/O, mode,    "CAMERASTATUS CALIBRATION"
            NUMBEROF_IMAGEBLOCK                 = &H004E0000    ' R/O, long,    "NUMBER OF IMAGE BLOCK"
            IMAGEBLOCK_FIRSTBYTESOFFSET         = &H004E1000    
            IMAGEBLOCK_ACTUALXPOS               = &H004E2000    
            IMAGEBLOCK_ACTUALYPOS               = &H004E3000    
            IMAGEBLOCK_ACTUALXSIZE              = &H004E4000    
            IMAGEBLOCK_ACTUALYSIZE              = &H004E5000    
            IMAGEBLOCK_OVERLAPLEFTSIZE          = &H004E6000    
            BACKFOCUSPOS_TARGET                 = &H00804010    ' R/W, micro-meter,"BACK FOCUS POSITION TARGET"
            BACKFOCUSPOS_CURRENT                = &H00804020    ' R/O, micro-meter,"BACK FOCUS POSITION CURRENT"
            BACKFOCUSPOS_LOADFROMMEMORY         = &H00804050    
            BACKFOCUSPOS_STORETOMEMORY          = &H00804060    ' W/O, long, "BACK FOCUS POSITION STORE TO MEMORY"
            CONFOCAL_SCANMODE                   = &H00910010    ' R/W, mode,    "CONFOCAL SCAN MODE"
            CONFOCAL_SCANLINES                  = &H00910020    ' R/W, long,    "CONFOCAL SCANLINES"
            CONFOCAL_ZOOM                       = &H00910030    ' R/W, long,    "CONFOCAL ZOOM"
            SUBUNIT_IMAGEWIDTH                  = &H009100e0    ' R/O, long,    "SUBUNIT IMAGE WIDTH
            NUMBEROF_SUBUNIT                    = &H009100f0    ' R/O, long,    "NUMBER OF SUBUNIT"
            SUBUNIT_CONTROL                     = &H00910100    ' R/W, mode,    "SUBUNIT CONTROL"
            SUBUNIT_LASERPOWER                  = &H00910200    ' R/W, long,    "SUBUNIT LASERPOWER"
            SUBUNIT_PMTGAIN                     = &H00910300    ' R/W, real,    "SUBUNIT PMTGAIN"
            SUBUNIT_PINHOLESIZE                 = &H00910400    ' R/O, long,    "SUBUNIT PINHOLE SIZE"
            SUBUNIT_WAVELENGTH                  = &H00910500    ' R/O, long,    "SUBUNIT WAVELENGTH"
            SUBUNIT_TOPOFFSETBYTES              = &H00910600    ' R/O, long,    "SUBUNIT TOP OFFSET BYTES"
            _SUBUNIT                            = &H00000010    ' the offset of ID for Nth Subunit parameter
            SYSTEM_ALIVE                        = &H00FF0010    ' R/O, mode,    "SYSTEM ALIVE"
            PRIMARYBUFFER_TOTALBYTES            = &H00FF1030    ' R/W, long,    "PRIMARY BUFFER TOTALBYTES"
            PRIMARYBUFFER_TOTALBYTES_MB         = &H00FF1040    ' R/W, long,    "PRIMARY BUFFER TOTALBYTES MB"
            CONVERSIONFACTOR_COEFF              = &H00FFE010    ' R/O, double,  "CONVERSION FACTOR COEFF"
            CONVERSIONFACTOR_OFFSET             = &H00FFE020    ' R/O, double,  "CONVERSION FACTOR OFFSET"

        End Enum
        Public Enum DCAMPROPVALUE
            SENSORMODE__AREA                                    = 1             ' "AREA"
            SENSORMODE__LINE                                    = 3             ' "LINE"
            SENSORMODE__TDI                                     = 4             ' "TDI"
            SENSORMODE__PARTIALAREA                             = 6             ' "PARTIAL AREA"
            SENSORMODE__TDI_EXTENDED                            = 10            ' "TDI EXTENDED"
            SENSORMODE__PANORAMIC                               = 11            ' "PANORAMIC"
            SENSORMODE__PROGRESSIVE                             = 12            ' "PROGRESSIVE"
            SENSORMODE__SPLITVIEW                               = 14            ' "SPLIT VIEW"
            SENSORMODE__DUALLIGHTSHEET                          = 16            ' "DUAL LIGHTSHEET"
            SENSORMODE__PHOTONNUMBERRESOLVING                   = 18            ' "PHOTON NUMBER RESOLVING"
            SENSORMODE__WHOLELINES                              = 19            ' "WHOLE LINES"
            SHUTTER_MODE__GLOBAL                                = 1             ' "GLOBAL"
            SHUTTER_MODE__ROLLING                               = 2             ' "ROLLING"
            READOUTSPEED__SLOWEST                               = 1             ' no text
            READOUTSPEED__FASTEST                               = &H7FFFFFFF    ' no text,w/o
            READOUT_DIRECTION__FORWARD                          = 1             ' "FORWARD"
            READOUT_DIRECTION__BACKWARD                         = 2             ' "BACKWARD"
            READOUT_DIRECTION__BYTRIGGER                        = 3             ' "BY TRIGGER"
            READOUT_DIRECTION__DIVERGE                          = 5             ' "DIVERGE"
            READOUT_DIRECTION__FORWARDBIDIRECTION               = 6             ' "FORWARD BIDIRECTION"
            READOUT_DIRECTION__REVERSEBIDIRECTION               = 7             ' "REVERSE BIDIRECTION"
            READOUT_UNIT__FRAME                                 = 2             ' "FRAME"
            READOUT_UNIT__BUNDLEDLINE                           = 3             ' "BUNDLED LINE"
            READOUT_UNIT__BUNDLEDFRAME                          = 4             ' "BUNDLED FRAME"
            CCDMODE__NORMALCCD                                  = 1             ' "NORMAL CCD"
            CCDMODE__EMCCD                                      = 2             ' "EM CCD"
            CMOSMODE__NORMAL                                    = 1             ' "NORMAL"
            CMOSMODE__NONDESTRUCTIVE                            = 2             ' "NON DESTRUCTIVE"
            MULTILINESENSOR_READOUTMODE__SYNCACCUMULATE         = 1             ' "SYNC ACCUMULATE"
            MULTILINESENSOR_READOUTMODE__SYNCAVERAGE            = 2             ' "SYNC AVERAGE"
            OUTPUT_INTENSITY__NORMAL                            = 1             ' "NORMAL"
            OUTPUT_INTENSITY__TESTPATTERN                       = 2             ' "TEST PATTERN"
            OUTPUTDATA_OPERATION__RAW                           = 1             
            OUTPUTDATA_OPERATION__ALIGNED                       = 2             
            TESTPATTERN_KIND__FLAT                              = 2             ' "FLAT"
            TESTPATTERN_KIND__IFLAT                             = 3             ' "INVERT FLAT"
            TESTPATTERN_KIND__HORZGRADATION                     = 4             ' "HORZGRADATION"
            TESTPATTERN_KIND__IHORZGRADATION                    = 5             ' "INVERT HORZGRADATION"
            TESTPATTERN_KIND__VERTGRADATION                     = 6             ' "VERTGRADATION"
            TESTPATTERN_KIND__IVERTGRADATION                    = 7             ' "INVERT VERTGRADATION"
            TESTPATTERN_KIND__LINE                              = 8             ' "LINE"
            TESTPATTERN_KIND__ILINE                             = 9             ' "INVERT LINE"
            TESTPATTERN_KIND__DIAGONAL                          = 10            ' "DIAGONAL"
            TESTPATTERN_KIND__IDIAGONAL                         = 11            ' "INVERT DIAGONAL"
            TESTPATTERN_KIND__FRAMECOUNT                        = 12            ' "FRAMECOUNT"
            DIGITALBINNING_METHOD__MINIMUM                      = 1             ' "MINIMUM"
            DIGITALBINNING_METHOD__MAXIMUM                      = 2             ' "MAXIMUM"
            DIGITALBINNING_METHOD__ODD                          = 3             ' "ODD"
            DIGITALBINNING_METHOD__EVEN                         = 4             ' "EVEN"
            DIGITALBINNING_METHOD__SUM                          = 5             ' "SUM"
            DIGITALBINNING_METHOD__AVERAGE                      = 6             ' "AVERAGE"
            TRIGGERSOURCE__INTERNAL                             = 1             ' "INTERNAL"
            TRIGGERSOURCE__EXTERNAL                             = 2             ' "EXTERNAL"
            TRIGGERSOURCE__SOFTWARE                             = 3             ' "SOFTWARE"
            TRIGGERSOURCE__MASTERPULSE                          = 4             ' "MASTER PULSE"
            TRIGGERACTIVE__EDGE                                 = 1             ' "EDGE"
            TRIGGERACTIVE__LEVEL                                = 2             ' "LEVEL"
            TRIGGERACTIVE__SYNCREADOUT                          = 3             ' "SYNCREADOUT"
            TRIGGERACTIVE__POINT                                = 4             ' "POINT"
            BUS_SPEED__SLOWEST                                  = 1             ' no text
            BUS_SPEED__FASTEST                                  = &H7FFFFFFF    ' no text,w/o
            TRIGGER_MODE__NORMAL                                = 1             ' "NORMAL"
            TRIGGER_MODE__PIV                                   = 3             ' "PIV"
            TRIGGER_MODE__START                                 = 6             ' "START"
            TRIGGERPOLARITY__NEGATIVE                           = 1             ' "NEGATIVE"
            TRIGGERPOLARITY__POSITIVE                           = 2             ' "POSITIVE"
            TRIGGER_CONNECTOR__INTERFACE                        = 1             ' "INTERFACE"
            TRIGGER_CONNECTOR__BNC                              = 2             ' "BNC"
            TRIGGER_CONNECTOR__MULTI                            = 3             ' "MULTI"
            INTERNALTRIGGER_HANDLING__SHORTEREXPOSURETIME       = 1             ' "SHORTER EXPOSURE TIME"
            INTERNALTRIGGER_HANDLING__FASTERFRAMERATE           = 2             ' "FASTER FRAME RATE"
            INTERNALTRIGGER_HANDLING__ABANDONWRONGFRAME         = 3             ' "ABANDON WRONG FRAME"
            INTERNALTRIGGER_HANDLING__BURSTMODE                 = 4             ' "BURST MODE"
            INTERNALTRIGGER_HANDLING__INDIVIDUALEXPOSURE        = 7             ' "INDIVIDUAL EXPOSURE TIME"
            SYNCREADOUT_SYSTEMBLANK__STANDARD                   = 1             ' "STANDARD"
            SYNCREADOUT_SYSTEMBLANK__MINIMUM                    = 2             ' "MINIMUM"
            TRIGGERENABLE_ACTIVE__DENY                          = 1             ' "DENY"
            TRIGGERENABLE_ACTIVE__ALWAYS                        = 2             ' "ALWAYS"
            TRIGGERENABLE_ACTIVE__LEVEL                         = 3             ' "LEVEL"
            TRIGGERENABLE_ACTIVE__START                         = 4             ' "START"
            TRIGGERENABLE_ACTIVE__BURST                         = 6             ' "BURST"
            TRIGGERENABLE_SOURCE__MULTI                         = 7             ' "MULTI"
            TRIGGERENABLE_SOURCE__SMA                           = 8             ' "SMA"
            TRIGGERENABLE_POLARITY__NEGATIVE                    = 1             ' "NEGATIVE"
            TRIGGERENABLE_POLARITY__POSITIVE                    = 2             ' "POSITIVE"
            TRIGGERENABLE_POLARITY__INTERLOCK                   = 3             ' "INTERLOCK"
            OUTPUTTRIGGER_CHANNELSYNC__1CHANNEL                 = 1             ' "1 Channel"
            OUTPUTTRIGGER_CHANNELSYNC__2CHANNELS                = 2             ' "2 Channels"
            OUTPUTTRIGGER_CHANNELSYNC__3CHANNELS                = 3             ' "3 Channels"
            OUTPUTTRIGGER_PROGRAMABLESTART__FIRSTEXPOSURE       = 1             ' "FIRST EXPOSURE"
            OUTPUTTRIGGER_PROGRAMABLESTART__FIRSTREADOUT        = 2             ' "FIRST READOUT"
            OUTPUTTRIGGER_SOURCE__EXPOSURE                      = 1             ' "EXPOSURE"
            OUTPUTTRIGGER_SOURCE__READOUTEND                    = 2             ' "READOUT END"
            OUTPUTTRIGGER_SOURCE__VSYNC                         = 3             ' "VSYNC"
            OUTPUTTRIGGER_SOURCE__HSYNC                         = 4             ' "HSYNC"
            OUTPUTTRIGGER_SOURCE__TRIGGER                       = 6             ' "TRIGGER"
            OUTPUTTRIGGER_POLARITY__NEGATIVE                    = 1             ' "NEGATIVE"
            OUTPUTTRIGGER_POLARITY__POSITIVE                    = 2             ' "POSITIVE"
            OUTPUTTRIGGER_ACTIVE__EDGE                          = 1             ' "EDGE"
            OUTPUTTRIGGER_ACTIVE__LEVEL                         = 2             ' "LEVEL"
            OUTPUTTRIGGER_KIND__LOW                             = 1             ' "LOW"
            OUTPUTTRIGGER_KIND__GLOBALEXPOSURE                  = 2             ' "EXPOSURE"
            OUTPUTTRIGGER_KIND__PROGRAMABLE                     = 3             ' "PROGRAMABLE"
            OUTPUTTRIGGER_KIND__TRIGGERREADY                    = 4             ' "TRIGGER READY"
            OUTPUTTRIGGER_KIND__HIGH                            = 5             ' "HIGH"
            OUTPUTTRIGGER_KIND__ANYROWEXPOSURE                  = 6             ' "ANYROW EXPOSURE"
            OUTPUTTRIGGER_BASESENSOR__VIEW1                     = 1             ' "VIEW 1"
            OUTPUTTRIGGER_BASESENSOR__VIEW2                     = 2             ' "VIEW 2"
            OUTPUTTRIGGER_BASESENSOR__ANYVIEW                   = 15            ' "ANY VIEW"
            OUTPUTTRIGGER_BASESENSOR__ALLVIEWS                  = 16            ' "ALL VIEWS"
            EXPOSURETIME_CONTROL__OFF                           = 1             ' "OFF"
            EXPOSURETIME_CONTROL__NORMAL                        = 2             ' "NORMAL"
            TRIGGER_FIRSTEXPOSURE__NEW                          = 1             ' "NEW"
            TRIGGER_FIRSTEXPOSURE__CURRENT                      = 2             ' "CURRENT"
            TRIGGER_GLOBALEXPOSURE__NONE                        = 1             ' "NONE"
            TRIGGER_GLOBALEXPOSURE__ALWAYS                      = 2             ' "ALWAYS"
            TRIGGER_GLOBALEXPOSURE__DELAYED                     = 3             ' "DELAYED"
            TRIGGER_GLOBALEXPOSURE__EMULATE                     = 4             ' "EMULATE"
            TRIGGER_GLOBALEXPOSURE__GLOBALRESET                 = 5             ' "GLOBAL RESET"
            FIRSTTRIGGER_BEHAVIOR__STARTEXPOSURE                = 1             ' "START EXPOSURE"
            FIRSTTRIGGER_BEHAVIOR__STARTREADOUT                 = 2             ' "START READOUT"
            MASTERPULSE_MODE__CONTINUOUS                        = 1             ' "CONTINUOUS"
            MASTERPULSE_MODE__START                             = 2             ' "START"
            MASTERPULSE_MODE__BURST                             = 3             ' "BURST"
            MASTERPULSE_TRIGGERSOURCE__EXTERNAL                 = 1             ' "EXTERNAL"
            MASTERPULSE_TRIGGERSOURCE__SOFTWARE                 = 2             ' "SOFTWARE"
            MECHANICALSHUTTER__AUTO                             = 1             ' "AUTO"
            MECHANICALSHUTTER__CLOSE                            = 2             ' "CLOSE"
            MECHANICALSHUTTER__OPEN                             = 3             ' "OPEN"
            LIGHTMODE__LOWLIGHT                                 = 1             ' "LOW LIGHT"
            LIGHTMODE__HIGHLIGHT                                = 2             ' "HIGH LIGHT"
            SENSITIVITYMODE__OFF                                = 1             ' "OFF"
            SENSITIVITYMODE__ON                                 = 2             ' "ON"
            SENSITIVITY2_MODE__INTERLOCK                        = 3             ' "INTERLOCK"
            EMGAINWARNING_STATUS__NORMAL                        = 1             ' "NORMAL"
            EMGAINWARNING_STATUS__WARNING                       = 2             ' "WARNING"
            EMGAINWARNING_STATUS__PROTECTED                     = 3             ' "PROTECTED"
            PHOTONIMAGINGMODE__0                                = 0             ' "0"
            PHOTONIMAGINGMODE__1                                = 1             ' "1"
            PHOTONIMAGINGMODE__2                                = 2             ' "2"
            PHOTONIMAGINGMODE__3                                = 3             ' "2"
            SENSORCOOLER__OFF                                   = 1             ' "OFF"
            SENSORCOOLER__ON                                    = 2             ' "ON"
            SENSORCOOLER__MAX                                   = 4             ' "MAX"
            SENSORTEMPERATURE_STATUS__NORMAL                    = 0             ' "NORMAL"
            SENSORTEMPERATURE_STATUS__WARNING                   = 1             ' "WARNING"
            SENSORTEMPERATURE_STATUS__PROTECTION                = 2             ' "PROTECTION"
            SENSORCOOLERSTATUS__ERROR4                          = -4            ' "ERROR4"
            SENSORCOOLERSTATUS__ERROR3                          = -3            ' "ERROR3"
            SENSORCOOLERSTATUS__ERROR2                          = -2            ' "ERROR2"
            SENSORCOOLERSTATUS__ERROR1                          = -1            ' "ERROR1"
            SENSORCOOLERSTATUS__NONE                            = 0             ' "NONE"
            SENSORCOOLERSTATUS__OFF                             = 1             ' "OFF"
            SENSORCOOLERSTATUS__READY                           = 2             ' "READY"
            SENSORCOOLERSTATUS__BUSY                            = 3             ' "BUSY"
            SENSORCOOLERSTATUS__ALWAYS                          = 4             ' "ALWAYS"
            SENSORCOOLERSTATUS__WARNING                         = 5             ' "WARNING"
            REALTIMEGAINCORRECT_LEVEL__1                        = 1             ' "1"
            REALTIMEGAINCORRECT_LEVEL__2                        = 2             ' "2"
            REALTIMEGAINCORRECT_LEVEL__3                        = 3             ' "3"
            REALTIMEGAINCORRECT_LEVEL__4                        = 4             ' "4"
            REALTIMEGAINCORRECT_LEVEL__5                        = 5             ' "5"
            WHITEBALANCEMODE__FLAT                              = 1             ' "FLAT"
            WHITEBALANCEMODE__AUTO                              = 2             ' "AUTO"
            WHITEBALANCEMODE__TEMPERATURE                       = 3             ' "TEMPERATURE"
            WHITEBALANCEMODE__USERPRESET                        = 4             ' "USER PRESET"
            DARKCALIB_TARGET__ALL                               = 1             ' "ALL"
            DARKCALIB_TARGET__ANALOG                            = 2             ' "ANALOG"
            SHADINGCALIB_METHOD__AVERAGE                        = 1             ' "AVERAGE"
            SHADINGCALIB_METHOD__MAXIMUM                        = 2             ' "MAXIMUM"
            SHADINGCALIB_METHOD__USETARGET                      = 3             ' "USE TARGET"
            CAPTUREMODE__NORMAL                                 = 1             ' "NORMAL"
            CAPTUREMODE__DARKCALIB                              = 2             ' "DARK CALIBRATION"
            CAPTUREMODE__SHADINGCALIB                           = 3             ' "SHADING CALIBRATION"
            CAPTUREMODE__TAPGAINCALIB                           = 4             ' "TAP GAIN CALIBRATION"
            CAPTUREMODE__BACKFOCUSCALIB                         = 5             ' "BACK FOCUS CALIBRATION"
            IMAGEFILTER__THROUGH                                = 0             ' "THROUGH"
            IMAGEFILTER__PATTERN_1                              = 1             ' "PATTERN 1"
            INTERFRAMEALU_ENABLE__OFF                           = 1             ' "OFF"
            INTERFRAMEALU_ENABLE__TRIGGERSOURCE_ALL             = 2             ' "TRIGGER SOURCE ALL"
            INTERFRAMEALU_ENABLE__TRIGGERSOURCE_INTERNAL        = 3             ' "TRIGGER SOURCE INTERNAL ONLY"
            CALIBDATASTATUS__NONE                               = 1             ' "NONE"
            CALIBDATASTATUS__FORWARD                            = 2             ' "FORWARD"
            CALIBDATASTATUS__BACKWARD                           = 3             ' "BACKWARD"
            CALIBDATASTATUS__BOTH                               = 4             ' "BOTH"
            TAPGAINCALIB_METHOD__AVE                            = 1             ' "AVERAGE"
            TAPGAINCALIB_METHOD__MAX                            = 2             ' "MAXIMUM"
            TAPGAINCALIB_METHOD__MIN                            = 3             ' "MINIMUM"
            RECURSIVEFILTERFRAMES__2                            = 2             ' "2 FRAMES"
            RECURSIVEFILTERFRAMES__4                            = 4             ' "4 FRAMES"
            RECURSIVEFILTERFRAMES__8                            = 8             ' "8 FRAMES"
            RECURSIVEFILTERFRAMES__16                           = 16            ' "16 FRAMES"
            RECURSIVEFILTERFRAMES__32                           = 32            ' "32 FRAMES"
            RECURSIVEFILTERFRAMES__64                           = 64            ' "64 FRAMES"
            INTENSITYLUT_MODE__THROUGH                          = 1             ' "THROUGH"
            INTENSITYLUT_MODE__PAGE                             = 2             ' "PAGE"
            INTENSITYLUT_MODE__CLIP                             = 3             ' "CLIP"
            BINNING__1                                          = 1             ' "1X1"
            BINNING__2                                          = 2             ' "2X2"
            BINNING__4                                          = 4             ' "4X4"
            BINNING__8                                          = 8             ' "8X8"
            BINNING__16                                         = 16            ' "16X16"
            BINNING__1_2                                        = 102           ' "1X2"
            BINNING__2_4                                        = 204           ' "2X4"
            COLORTYPE__BW                                       = &H00000001    ' "BW"
            COLORTYPE__RGB                                      = &H00000002    ' "RGB"
            COLORTYPE__BGR                                      = &H00000003    ' "BGR"
            BITSPERCHANNEL__8                                   = 8             ' "8BIT"
            BITSPERCHANNEL__10                                  = 10            ' "10BIT"
            BITSPERCHANNEL__12                                  = 12            ' "12BIT"
            BITSPERCHANNEL__14                                  = 14            ' "14BIT"
            BITSPERCHANNEL__16                                  = 16            ' "16BIT"
            DEFECTCORRECT_MODE__OFF                             = 1             ' "OFF"
            DEFECTCORRECT_MODE__ON                              = 2             ' "ON"
            DEFECTCORRECT_METHOD__CEILING                       = 3             ' "CEILING"
            DEFECTCORRECT_METHOD__PREVIOUS                      = 4             ' "PREVIOUS"
            DEFECTCORRECT_METHOD__NEXT                          = 5             ' "NEXT"
            HOTPIXELCORRECT_LEVEL__STANDARD                     = 1             ' "STANDARD"
            HOTPIXELCORRECT_LEVEL__MINIMUM                      = 2             ' "MINIMUM"
            HOTPIXELCORRECT_LEVEL__AGGRESSIVE                   = 3             ' "AGGRESSIVE"
            DEVICEBUFFER_MODE__THRU                             = 1             ' "THRU"
            DEVICEBUFFER_MODE__SNAPSHOT                         = 2             ' "SNAPSHOT"
            DEVICEBUFFER_MODE__SNAPSHOTEX                       = 6             ' "SNAPSHOTEX"
            INTERNALLINERATE_CONTROL__SYNC_EXPOSURETIME         = 1             ' "SYNC EXPOSURETIME"
            INTERNALLINERATE_CONTROL__PRIORITIZE_LINERATE       = 2             ' "PRIORITIZE LINERATE"
            INTERNALLINERATE_CONTROL__PRIORITIZE_EXPOSURETIME   = 3             ' "PRIORITIZE EXPOSURETIME"
            SYSTEM_ALIVE__OFFLINE                               = 1             ' "OFFLINE"
            SYSTEM_ALIVE__ONLINE                                = 2             ' "ONLINE"
            SYSTEM_ALIVE__ERROR                                 = 3             ' "ERROR"
            TIMESTAMP_MODE__NONE                                = 1             ' "NONE"
            TIMESTAMP_MODE__LINEBEFORELEFT                      = 2             ' "LINE BEFORE LEFT"
            TIMESTAMP_MODE__LINEOVERWRITELEFT                   = 3             ' "LINE OVERWRITE LEFT"
            TIMESTAMP_MODE__AREABEFORELEFT                      = 4             ' "AREA BEFORE LEFT"
            TIMESTAMP_MODE__AREAOVERWRITELEFT                   = 5             ' "AREA OVERWRITE LEFT"
            TIMING_EXPOSURE__AFTERREADOUT                       = 1             ' "AFTER READOUT"
            TIMING_EXPOSURE__OVERLAPREADOUT                     = 2             ' "OVERLAP READOUT"
            TIMING_EXPOSURE__ROLLING                            = 3             ' "ROLLING"
            TIMING_EXPOSURE__ALWAYS                             = 4             ' "ALWAYS"
            TIMING_EXPOSURE__TDI                                = 5             ' "TDI"
            TIMESTAMP_PRODUCER__NONE                            = 1             ' "NONE"
            TIMESTAMP_PRODUCER__DCAMMODULE                      = 2             ' "DCAM MODULE"
            TIMESTAMP_PRODUCER__KERNELDRIVER                    = 3             ' "KERNEL DRIVER"
            TIMESTAMP_PRODUCER__CAPTUREDEVICE                   = 4             ' "CAPTURE DEVICE"
            TIMESTAMP_PRODUCER__IMAGINGDEVICE                   = 5             ' "IMAGING DEVICE"
            FRAMESTAMP_PRODUCER__NONE                           = 1             ' "NONE"
            FRAMESTAMP_PRODUCER__DCAMMODULE                     = 2             ' "DCAM MODULE"
            FRAMESTAMP_PRODUCER__KERNELDRIVER                   = 3             ' "KERNEL DRIVER"
            FRAMESTAMP_PRODUCER__CAPTUREDEVICE                  = 4             ' "CAPTURE DEVICE"
            FRAMESTAMP_PRODUCER__IMAGINGDEVICE                  = 5             ' "IMAGING DEVICE"
            CAMERASTATUS_INTENSITY__GOOD                        = 1             ' "GOOD"
            CAMERASTATUS_INTENSITY__TOODARK                     = 2             ' "TOO DRAK"
            CAMERASTATUS_INTENSITY__TOOBRIGHT                   = 3             ' "TOO BRIGHT"
            CAMERASTATUS_INTENSITY__UNCARE                      = 4             ' "UNCARE"
            CAMERASTATUS_INTENSITY__EMGAIN_PROTECTION           = 5             ' "EMGAIN PROTECTION"
            CAMERASTATUS_INTENSITY__INCONSISTENT_OPTICS         = 6             ' "INCONSISTENT OPTICS"
            CAMERASTATUS_INTENSITY__NODATA                      = 7             ' "NO DATA"
            CAMERASTATUS_INPUTTRIGGER__GOOD                     = 1             ' "GOOD"
            CAMERASTATUS_INPUTTRIGGER__NONE                     = 2             ' "NONE"
            CAMERASTATUS_INPUTTRIGGER__TOOFREQUENT              = 3             ' "TOO FREQUENT"
            CAMERASTATUS_CALIBRATION__DONE                      = 1             ' "DONE"
            CAMERASTATUS_CALIBRATION__NOTYET                    = 2             ' "NOT YET"
            CAMERASTATUS_CALIBRATION__NOTRIGGER                 = 3             ' "NO TRIGGER"
            CAMERASTATUS_CALIBRATION__TOOFREQUENTTRIGGER        = 4             ' "TOO FREQUENT TRIGGER"
            CAMERASTATUS_CALIBRATION__OUTOFADJUSTABLERANGE      = 5             
            CAMERASTATUS_CALIBRATION__UNSUITABLETABLE           = 6             ' "UNSUITABLE TABLE"
            CAMERASTATUS_CALIBRATION__TOODARK                   = 7             ' "TOO DARK"
            CAMERASTATUS_CALIBRATION__TOOBRIGHT                 = 8             ' "TOO BRIGHT"
            CAMERASTATUS_CALIBRATION__NOTDETECTOBJECT           = 9             ' "NOT DETECT OBJECT"
            CONFOCAL_SCANMODE__SIMULTANEOUS                     = 1             ' "SIMULTANEOUS"
            CONFOCAL_SCANMODE__SEQUENTIAL                       = 2             ' "SEQUENTIAL"
            SUBUNIT_CONTROL__NOTINSTALLED                       = 0             ' "NOT INSTALLED"
            SUBUNIT_CONTROL__OFF                                = 1             ' "OFF"
            SUBUNIT_CONTROL__ON                                 = 2             ' "ON"
            SUBUNIT_PINHOLESIZE__ERROR                          = 1             ' "ERROR"
            SUBUNIT_PINHOLESIZE__SMALL                          = 2             ' "SMALL"
            SUBUNIT_PINHOLESIZE__MEDIUM                         = 3             ' "MEDIUM"
            SUBUNIT_PINHOLESIZE__LARGE                          = 4             ' "LARGE"

        End Enum
        Public Enum DCAMPROP_UNIT
            SECOND          = &H1   ' sec
            CELSIUS         = &H2   ' for sensor temperature
            KELVIN          = &H3   ' for color temperature
            METERPERSECOND  = &H4   ' for LINESPEED
            PERSECOND       = &H5   ' for FRAMERATE and LINERATE
            DEGREE          = &H6   ' for OUTPUT ROTATION
            MICROMETER      = &H7   ' for length
            NONE            = &H0   ' no unit

        End Enum
        ' option for some dcamprop functions
        Public Enum DCAMPROPOPTION As Integer
            PRIOR       = &HFF000000    ' prior value
            _NEXT       = &H01000000    ' next value or id
            SUPPORT     = &H00000000    ' default option
            UPDATED     = &H00000001    ' UPDATED and VOLATILE can be used at same time
            VOLATILE    = &H00000002    ' UPDATED and VOLATILE can be used at same time
            ARRAYELEMENT= &H00000004    ' ARRAYELEMENT
            NONE        = &H00000000    ' no option

        End Enum
        Public Enum DCAMPROPATTRIBUTE As Integer
            HASRANGE                = &H80000000    
            HASSTEP                 = &H40000000    
            HASDEFAULT              = &H20000000    
            HASVALUETEXT            = &H10000000    
            HASCHANNEL              = &H08000000    ' value can set the value for each channels
            AUTOROUNDING            = &H00800000    
            STEPPING_INCONSISTENT   = &H00400000    
            DATASTREAM              = &H00200000    ' value is releated to image attribute
            HASRATIO                = &H00100000    ' value has ratio control capability
            VOLATILE                = &H00080000    ' value may be changed by user or automatically
            WRITABLE                = &H00020000    ' value can be set when state is manual
            READABLE                = &H00010000    ' value is readable when state is manual
            HASVIEW                 = &H00008000    ' value can set the value for each views
            ACCESSREADY             = &H00002000    ' This value can get or set at READY status
            ACCESSBUSY              = &H00001000    ' This value can get or set at BUSY status
            ACTION                  = &H00000400    ' writing value takes related effect
            EFFECTIVE               = &H00000200    ' value is effective


            ' property value type
            TYPE_NONE   = &H00000000    ' undefined
            TYPE_MODE   = &H00000001    ' 01:   mode, 32bit integer in case of 32bit OS
            TYPE_LONG   = &H00000002    ' 02:   32bit integer in case of 32bit OS
            TYPE_REAL   = &H00000003    ' 03:   64bit float
            TYPE_MASK   = &H0000000F    ' mask for property value type


            ' application has to use double-float type variable even the property is not REAL.
        End Enum
        Public Enum DCAMPROPATTRIBUTE2 As Integer
            ARRAYBASE           = &H08000000    
            ARRAYELEMENT        = &H04000000    
            REAL32              = &H02000000    
            INITIALIZEIMPROPER  = &H00000001    
            CHANNELSEPARATEDDATA= &H00040000    ' Channel 0 value is total of each channels.

        End Enum


        Public Enum DCAMCAP_STATUS
            _ERROR  = &H0000    
            BUSY    = &H0001    
            READY   = &H0002    
            STABLE  = &H0003    
            UNSTABLE= &H0004    

        End Enum
        Public Enum DCAMWAIT_EVENT As Integer
            CAPEVENT_TRANSFERRED= &H0001    
            CAPEVENT_FRAMEREADY = &H0002    ' all modules support
            CAPEVENT_CYCLEEND   = &H0004    ' all modules support
            CAPEVENT_EXPOSUREEND= &H0008    
            CAPEVENT_STOPPED    = &H0010    
            CAPEVENT_RELOADFRAME= &H0020    
            RECEVENT_STOPPED    = &H0100    
            RECEVENT_WARNING    = &H0200    
            RECEVENT_MISSED     = &H0400    
            RECEVENT_DISKFULL   = &H1000    
            RECEVENT_WRITEFAULT = &H2000    
            RECEVENT_SKIPPED    = &H4000    
            RECEVENT_WRITEFRAME = &H8000    ' DCAMCAP_START_BUFRECORD only

        End Enum
        Public Enum DCAMCAP_START
            SEQUENCE= -1    
            SNAP    = 0     

        End Enum


        ' ================================ common structures ================================

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAM_TIMESTAMP
            Public sec As UInteger
            Public microsec As UInteger
        End Structure
        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAM_METADATAHDR
            Public size As Integer              ' [in] size of whole structure, not only this.
            Public iKind As Integer             ' [in] DCAM_METADATAKIND
            Public _option As Integer           ' [in] value meaning depends on DCAM_METADATAKIND. the word "option" is reserved in VB.NET
            Public iFrame As Integer            ' [in] frame index
        End Structure
        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAM_METADATABLOCKHDR
            Public size As Integer              ' [in] size of whole structure, not only this.
            Public iKind As Integer             ' [in] DCAM_METADATAKIND
            Public _option As Integer           ' [in] value meaning depends on DCAMBUF_METADATAOPTION or DCAMREC_METADATAOPTION. the word "option" is reserved in VB.NET
            Public iFrame As Integer            ' [in] start frame index
            Public in_count As Integer          ' [in] max count of meta data
            Public outcount As Integer          ' [out] count of got meta data
        End Structure

        ' ================================ structures ================================

        ' ---- structure for dcamapi.dll ----
        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMAPI_INIT
            Public size As Integer              ' [in]
            Public iDeviceCount As Integer      ' [out]
            Public reserved As Integer          ' reserved
            Public initoptionbytes As Integer   ' [in] maximum bytes of initoption array.
            Public initoption As IntPtr         ' [in ptr] initialize options. Choose from DCAMAPI_INITOPTION
            Public guid As IntPtr               ' [in ptr(DCAM_GUID*)]

            Public Sub New(ByVal r As Integer)
                size = Marshal.SizeOf(GetType(DCAMAPI_INIT))
                iDeviceCount = 0
                reserved = r
                initoptionbytes = 0
                initoption = 0
                guid = 0
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMDEV_OPEN
            Public size As Integer              ' [in] size of this structure
            Public index As Integer             ' [in] camera index
            Public hdcam As IntPtr              ' [out] HDCAM handle is set if success

            Public Sub New(ByVal indexCamera As Integer)
                size = Marshal.SizeOf(GetType(DCAMDEV_OPEN))
                index = indexCamera
                hdcam = 0
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMDEV_STRING
            Public size As Integer              ' [in] size of this structure
            Public iString As Integer           ' [in] string index
            Public text As IntPtr               ' [in,obuf] string information
            Public textbytes As Integer         ' [in] byte size of string buffer

            Public Sub New(ByVal indexString As Integer)
                size = Marshal.SizeOf(GetType(DCAMDEV_STRING))
                iString = indexString
                textbytes = 0
                text = IntPtr.Zero
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMDEV_CAPABILITY
            Public size As Integer              ' [in] size of this structure
            Public domain As Integer            ' [in] DCAMDEV_CAPDOMAIN__*
            Public capflag As Integer           ' [out] available flags in current condition.
            Public kind As Integer              ' [in] DCAMDATA_KIND_* when domain is DCAMDEV_CAPDOMAIN__DCAMDATA

            Public Sub New(ByVal _domain As DCAMDEV_CAPDOMAIN)
                size = Marshal.SizeOf(GetType(DCAMDEV_CAPABILITY))
                domain = _domain
                capflag = 0
                kind = 0
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMBUF_FRAME
            ' copyframe() and lockframe() use this structure. Some members have different direction.
            ' [i:o] means, the member is input at copyframe() and output at lockframe().
            ' [i:i] and [o:o] means always input and output at both function.
            ' "input" means application has to set the value before calling.
            ' "output" means function filles a value at returning.
            Public size As Integer              ' [in] size of this structure
            Public iKind As Integer             ' [in] reserved. set to 0.
            Public _option As Integer           ' [in] reserved. set to 0. the word "option" is reserved in VB.NET
            Public iFrame As Integer            ' [in] frame index
            Public buf As IntPtr                ' [i:o] pointer for top-left image
            Public rowbytes As Integer          ' [i:o] byte size for next line.
            Public type As Integer              ' [i/o] return pixel type of image. set to 0. DCAM_PIXELTYPE
            Public width As Integer             ' [i:o] horizontal pixel count
            Public height As Integer            ' [i:o] vertical line count
            Public left As Integer              ' [i:o] horizontal start pixel
            Public top As Integer               ' [i:o] vertical start line
            Public timestamp As DCAM_TIMESTAMP  ' [o:o] timestamp
            Public framestamp As Integer        ' [o:o] framestamp
            Public camerastamp As Integer       ' [o:o] camerastamp

            Public Sub New(ByVal indexFrame As Integer)
                size = Marshal.SizeOf(GetType(DCAMBUF_FRAME))
                iKind = 0
                _option = 0
                iFrame = indexFrame
                buf = 0
                rowbytes = 0
                type = 0
                width = 0
                left = 0
                top = 0
                timestamp.sec = 0
                timestamp.microsec = 0
                framestamp = 0
                camerastamp = 0
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMCAP_TRANSFERINFO
            Public size As Integer              ' [in] size of this structure
            Public iKind As Integer             ' [in] reserved 0
            Public nNewestFrameIndex As Integer ' [out]
            Public nFrameCount As Integer       ' [out]

            Public Sub New(ByVal r As Integer)
                size = Marshal.SizeOf(GetType(DCAMCAP_TRANSFERINFO))
                iKind = r
                nNewestFrameIndex = 0
                nFrameCount = 0
            End Sub

        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMWAIT_OPEN
            Public size As Integer              ' [in] size of this structure
            Public supportevent As Integer      ' [out] filled with supported event events
            Public hwait As IntPtr              ' [out] HDCAMWAIT event handle
            Public hdcam As IntPtr              ' [in] HDCAM handle

            Public Sub New(ByVal _hdcam As IntPtr)
                size = Marshal.SizeOf(GetType(DCAMWAIT_OPEN))
                supportevent = 0
                hwait = 0
                hdcam = _hdcam
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMWAIT_START
            Public size As Integer              ' [in] size of this structure
            Public eventhappened As Integer     ' [out] the bit of happening event will be set
            Public eventmask As Integer         ' [out] the bits for waiting events
            Public timeout As Integer           ' [in] timeout by milliseconds

            Public Sub New(ByVal _eventmask As Integer)
                size = Marshal.SizeOf(GetType(DCAMWAIT_START))
                eventhappened = 0
                eventmask = _eventmask
                timeout = &H80000000            ' wait forever
            End Sub

            Public Sub New(ByVal _eventmask As Integer, ByVal _timeout As Integer)
                size = Marshal.SizeOf(GetType(DCAMWAIT_START))
                eventhappened = 0
                eventmask = _eventmask
                timeout = _timeout              ' wait millisecond
            End Sub

        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMPROP_ATTR
            Public size As Integer              ' [in] size of this structure
            Public iProp As Integer             ' [in] DCAMIDPROPERTY
            Public _option As DCAMPROPOPTION    ' [in] DCAMPROPOPTION
            Public iReserved1 As Integer        ' [in] must be 0

            Public attribute As DCAMPROPATTRIBUTE    ' [out] DCAMPROPATTR
            Public iGroup As Integer            ' [out] 0 reserved;
            Public iUnit As Integer             ' [out] DCAMPROPUNIT
            Public attribute2 As DCAMPROPATTRIBUTE2  ' [out] DCAMPROPATTR2

            Public valuemin As Double           ' minimum value
            Public valuemax As Double           ' maximum value
            Public valuestep As Double          ' minimum stepping between a value and the next
            Public valuedefault As Double       ' default value

            Public nMaxChannel As Integer       ' [out] max channel if supports
            Public iReserved3 As Integer        ' [out] reserved to 0
            Public nMaxView As Integer          ' [out] max view if supports

            Public iProp_NumberOfElement As Integer ' [out] property id to get number of elements of this property if it is array
            Public iProp_ArrayBase As Integer   ' [out] base id of array if element
            Public iPropStep_Element As Integer ' [out] step for iProp to next element

            Public Sub New(ByVal idProp As Integer)
                size = Marshal.SizeOf(GetType(DCAMPROP_ATTR))
                iProp = idProp
                _option = 0
                iReserved1 = 0

                attribute = 0
                iGroup = 0
                iUnit = 0
                attribute2 = 0

                valuemin = 0
                valuemax = 0
                valuestep = 0
                valuedefault = 0

                nMaxChannel = 0
                iReserved3 = 0
                nMaxView = 0

                iProp_NumberOfElement = 0
                iProp_ArrayBase = 0
                iPropStep_Element = 0
            End Sub

        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMPROP_VALUETEXT
            Public size As Integer              ' [in] size of this structure
            Public iProp As Integer             ' [in] DCAMIDPROPERTY
            Public value As Double              ' [in] value of property
            Public text As IntPtr               ' [in,obuf] text of the value
            Public textbytes As Integer         ' [in] text buf size

            Public Sub New(ByVal idProp As Integer, ByVal _value As Double)
                size = Marshal.SizeOf(GetType(DCAMPROP_VALUETEXT))
                iProp = idProp
                value = _value
                text = IntPtr.Zero
                textbytes = 0
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMREC_OPEN
            Public size As Integer              ' [in] size of this structure
            Public reserved As Integer          ' [in] 0 reserved
            Public hrec As IntPtr               ' [out] HDCAMREC handle
            Public path As String               ' [in]
            Public ext As String                ' [in]
            Public maxframepersession As Integer    ' [in]
            Public userdatasize As Integer      ' [in]
            Public userdatasize_session As Integer  ' [in]
            Public userdatasize_file As Integer ' [in]
            Public usertextsize As Integer      ' [in]
            Public usertextsize_session As Integer  ' [in]
            Public usertextsize_file As Integer ' [in]

            Public Sub New(ByRef _path As String, ByRef _ext As String)
                size = Marshal.SizeOf(GetType(DCAMREC_OPEN))
                reserved = 0
                hrec = 0
                path = _path
                ext = _ext
                maxframepersession = 0
                userdatasize = 0
                userdatasize_session = 0
                userdatasize_file = 0
                usertextsize = 0
                usertextsize_session = 0
                usertextsize_file = 0
            End Sub
        End Structure

        <StructLayout(LayoutKind.Sequential, Pack:=8, CharSet:=CharSet.Ansi)> _
        Public Structure DCAMREC_STATUS
            Public size As Integer              ' [in] size of this structure
            Public reserved1 As Integer         ' [out] reserved
            Public reserved2 As Integer         ' [out] reserved
            Public currentframe_index As Integer
            Public missingframe_count As Integer
            Public flags As Integer    ' DCAMREC_STATUSFLAG
            Public totalframecount As Integer
            Public reserved As Integer

            Public Sub New(ByVal r As Integer)
                size = Marshal.SizeOf(GetType(DCAMREC_STATUS))
                reserved1 = 0
                reserved2 = 0
                currentframe_index = 0
                missingframe_count = 0
                flags = 0
                totalframecount = 0
                reserved = 0
            End Sub
        End Structure

        ' ================================ common function ================================

        Shared Function failed(ByVal err As DCAMERR) As Boolean
            If err < 0 Then
                Return True
            Else
                Return False
            End If
        End Function

        ' ================ declaration class for DCAM-API ================

        Private Class dcamapidll
            Declare Function dcamapi_init Lib "dcamapi.dll" (ByRef param As DCAMAPI_INIT) As Integer
            Declare Function dcamapi_uninit Lib "dcamapi.dll" () As Integer

            Declare Function dcamdev_open Lib "dcamapi.dll" (ByRef param As DCAMDEV_OPEN) As Integer
            Declare Function dcamdev_close Lib "dcamapi.dll" (ByVal hdcam As IntPtr) As Integer
            Declare Function dcamdev_getstring Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef param As DCAMDEV_STRING) As Integer
            Declare Function dcamdev_getcapability Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef param As DCAMDEV_CAPABILITY) As Integer

            Declare Function dcambuf_alloc Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal framecount As Integer) As Integer
            Declare Function dcambuf_release Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal iKind As Integer) As Integer
            Declare Function dcambuf_lockframe Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As Integer
            Declare Function dcambuf_copyframe Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As Integer
            Declare Function dcambuf_copymetadata Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef hdr As DCAM_METADATAHDR) As Integer

            Declare Function dcamcap_start Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal mode As Integer) As Integer
            Declare Function dcamcap_stop Lib "dcamapi.dll" (ByVal hdcam As IntPtr) As Integer
            Declare Function dcamcap_status Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef param As Integer) As Integer
            Declare Function dcamcap_transferinfo Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef param As DCAMCAP_TRANSFERINFO) As Integer
            Declare Function dcamcap_firetrigger Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal iKind As Integer) As Integer
            Declare Function dcamcap_record Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal hrec As IntPtr) As Integer

            Declare Function dcamwait_open Lib "dcamapi.dll" (ByRef param As DCAMWAIT_OPEN) As Integer
            Declare Function dcamwait_close Lib "dcamapi.dll" (ByVal hwait As IntPtr) As Integer
            Declare Function dcamwait_start Lib "dcamapi.dll" (ByVal hwait As IntPtr, ByRef param As DCAMWAIT_START) As Integer
            Declare Function dcamwait_abort Lib "dcamapi.dll" (ByVal hwait As IntPtr) As Integer

            Declare Function dcamrec_openA Lib "dcamapi.dll" (ByRef param As DCAMREC_OPEN) As Integer
            Declare Function dcamrec_status Lib "dcamapi.dll" (ByVal hrec As IntPtr, ByRef param As DCAMREC_STATUS) As Integer
            Declare Function dcamrec_close Lib "dcamapi.dll" (ByVal hrec As IntPtr) As Integer
            Declare Function dcamrec_lockframe Lib "dcamapi.dll" (ByVal hrec As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As Integer
            Declare Function dcamrec_copyframe Lib "dcamapi.dll" (ByVal hrec As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As Integer
            Declare Function dcamrec_writemetadata Lib "dcamapi.dll" (ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As Integer
            Declare Function dcamrec_lockmetadata Lib "dcamapi.dll" (ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As Integer
            Declare Function dcamrec_copymetadata Lib "dcamapi.dll" (ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As Integer
            Declare Function dcamrec_lockmetadatablock Lib "dcamapi.dll" (ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATABLOCKHDR) As Integer
            Declare Function dcamrec_copymetadatablock Lib "dcamapi.dll" (ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATABLOCKHDR) As Integer

            Declare Function dcamprop_getattr Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef param As DCAMPROP_ATTR) As Integer
            Declare Function dcamprop_getvalue Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal iProp As Integer, ByRef pValue As Double) As Integer
            Declare Function dcamprop_setvalue Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal iProp As Integer, ByVal fValue As Double) As Integer
            Declare Function dcamprop_setgetvalue Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal iProp As Integer, ByRef pValue As Double, ByVal _option As DCAMPROPOPTION) As Integer
            Declare Function dcamprop_queryvalue Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal iProp As Integer, ByRef pValue As Double, ByVal _option As DCAMPROPOPTION) As Integer
            Declare Function dcamprop_getnextid Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef pParam As Integer, ByVal _option As DCAMPROPOPTION) As Integer
            Declare Function dcamprop_getname Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByVal iProp As Integer, ByVal text As StringBuilder, ByVal textbytes As Integer) As Integer
            Declare Function dcamprop_getvaluetext Lib "dcamapi.dll" (ByVal hdcam As IntPtr, ByRef param As DCAMPROP_VALUETEXT) As Integer
        End Class

        ' ================================ dcamapi ================================

        Class dcamapi
            ' initialization
            Public Shared Function init(ByRef param As DCAMAPI_INIT) As DCAMERR
                Return dcamapidll.dcamapi_init(param)
            End Function

            ' uninitialization
            Public Shared Function uninit() As DCAMERR
                Return dcamapidll.dcamapi_uninit()
            End Function
        End Class

        ' ================================ dcamdev ================================

        Class dcamdev
            ' open HDCAM handle
            Public Shared Function open(ByRef param As DCAMDEV_OPEN) As DCAMERR
                Return dcamapidll.dcamdev_open(param)
            End Function

            ' close HDCAM handle
            Public Shared Function close(ByVal hdcam As IntPtr) As DCAMERR
                Return dcamapidll.dcamdev_close(hdcam)
            End Function

            ' get string information
            Public Shared Function getstring(ByVal hdcam As IntPtr, ByVal idstr As Integer, ByRef str As String) As DCAMERR
                Dim param As New DCAMDEV_STRING(idstr)

                param.textbytes = 256
                Dim buf As Byte() = New Byte(param.textbytes) {}
                Dim handle As GCHandle = GCHandle.Alloc(buf, GCHandleType.Pinned)
                param.text = handle.AddrOfPinnedObject()

                Dim err As DCAMERR
                err = dcamapidll.dcamdev_getstring(hdcam, param)
                handle.Free()

                If failed(err) Then
                    str = ""
                Else
                    Dim i As Integer
                    For i = 0 To buf.Count()
                        If buf(i) = 0 Then Exit For
                    Next
                    str = Encoding.ASCII.GetString(buf).Substring(0, i)
                End If
                Return err
            End Function

            ' get capability of DCAM functions
            Public Shared Function getcapability(ByVal hdcam As IntPtr, ByRef param As DCAMDEV_CAPABILITY) As DCAMERR
                Return dcamapidll.dcamdev_getcapability(hdcam, param)
            End Function
        End Class

        ' ================================ dcambuf ================================

        'buffer control
        Class dcambuf
            Public Shared Function alloc(ByVal hdcam As IntPtr, ByVal framecount As Integer) As DCAMERR
                Return dcamapidll.dcambuf_alloc(hdcam, framecount)
            End Function
            Public Shared Function release(ByVal hdcam As IntPtr, ByVal iKind As Integer) As DCAMERR
                Return dcamapidll.dcambuf_release(hdcam, iKind)
            End Function
            Public Shared Function lockframe(ByVal hdcam As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As DCAMERR
                Return dcamapidll.dcambuf_lockframe(hdcam, pFrame)
            End Function
            Public Shared Function copyframe(ByVal hdcam As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As DCAMERR
                Return dcamapidll.dcambuf_copyframe(hdcam, pFrame)
            End Function
            Public Shared Function copymetadata(ByVal hdcam As IntPtr, ByRef hdr As DCAM_METADATAHDR) As DCAMERR
                Return dcamapidll.dcambuf_copymetadata(hdcam, hdr)
            End Function
        End Class

        ' ================================ dcamcap ================================
        ' capturing

        Class dcamcap
            Public Shared Function start(ByVal hdcam As IntPtr, ByVal mode As Integer) As DCAMERR
                Return dcamapidll.dcamcap_start(hdcam, mode)
            End Function
            Public Shared Function idle(ByVal hdcam As IntPtr) As DCAMERR  ' "stop" is reserved in VB.NET
                Return dcamapidll.dcamcap_stop(hdcam)
            End Function
            Public Shared Function status(ByVal hdcam As IntPtr, ByRef param As Integer) As DCAMERR
                Return dcamapidll.dcamcap_status(hdcam, param)
            End Function
            Public Shared Function transferinfo(ByVal hdcam As IntPtr, ByRef param As DCAMCAP_TRANSFERINFO) As DCAMERR
                Return dcamapidll.dcamcap_transferinfo(hdcam, param)
            End Function
            Public Shared Function firetrigger(ByVal hdcam As IntPtr, ByVal iKind As Integer) As DCAMERR
                Return dcamapidll.dcamcap_firetrigger(hdcam, iKind)
            End Function
            Public Shared Function record(ByVal hdcam As IntPtr, ByVal hrec As IntPtr) As DCAMERR
                Return dcamapidll.dcamcap_record(hdcam, hrec)
            End Function
        End Class

        ' ================================ dcamwait ================================
        'wait and abort handle control

        Class dcamwait
            Public Shared Function open(ByRef param As DCAMWAIT_OPEN) As DCAMERR
                Return dcamapidll.dcamwait_open(param)
            End Function
            Public Shared Function close(ByVal hwait As IntPtr) As DCAMERR
                Return dcamapidll.dcamwait_close(hwait)
            End Function
            Public Shared Function start(ByVal hwait As IntPtr, ByRef param As DCAMWAIT_START) As DCAMERR
                Return dcamapidll.dcamwait_start(hwait, param)
            End Function
            Public Shared Function abort(ByVal hwait As IntPtr) As DCAMERR
                Return dcamapidll.dcamwait_abort(hwait)
            End Function
        End Class

        ' ================================ dcamprop ================================
        ' Property control

        Class dcamprop
            Public Shared Function getattr(ByVal hdcam As IntPtr, ByRef param As DCAMPROP_ATTR) As DCAMERR
                Return dcamapidll.dcamprop_getattr(hdcam, param)
            End Function
            Public Shared Function getvalue(ByVal hdcam As IntPtr, ByVal iProp As Integer, ByRef pValue As Double) As DCAMERR
                Return dcamapidll.dcamprop_getvalue(hdcam, iProp, pValue)
            End Function
            Public Shared Function setvalue(ByVal hdcam As IntPtr, ByVal iProp As Integer, ByVal fValue As Double) As DCAMERR
                Return dcamapidll.dcamprop_setvalue(hdcam, iProp, fValue)
            End Function
            Public Shared Function setgetvalue(ByVal hdcam As IntPtr, ByVal iProp As Integer, ByRef pValue As Double, ByVal _option As DCAMPROPOPTION) As DCAMERR
                Return dcamapidll.dcamprop_setgetvalue(hdcam, iProp, pValue, _option)
            End Function
            Public Shared Function queryvalue(ByVal hdcam As IntPtr, ByVal iProp As Integer, ByRef pValue As Double, ByVal _option As DCAMPROPOPTION) As DCAMERR
                Return dcamapidll.dcamprop_queryvalue(hdcam, iProp, pValue, _option)
            End Function
            Public Shared Function getnextid(ByVal hdcam As IntPtr, ByRef pParam As Integer, ByVal _option As DCAMPROPOPTION) As DCAMERR
                Return dcamapidll.dcamprop_getnextid(hdcam, pParam, _option)
            End Function
            Public Shared Function getname(ByVal hdcam As IntPtr, ByVal iProp As Integer, ByRef ret As String) As DCAMERR
                Dim textbytes As Integer = 256
                Dim sb As New StringBuilder(textbytes)

                Dim err As DCAMERR
                err = dcamapidll.dcamprop_getname(hdcam, iProp, sb, textbytes)
                If Not failed(err) Then
                    ret = sb.ToString
                End If
                Return err
            End Function
            Public Shared Function getvaluetext(ByVal hdcam As IntPtr, ByVal idprop As Integer, ByVal value As Double, ByRef ret As String) As DCAMERR
                Dim param As New DCAMPROP_VALUETEXT(idprop, value)
                param.textbytes = 256
                Dim buf As Byte() = New Byte(param.textbytes) {}
                Dim handle As GCHandle = GCHandle.Alloc(buf, GCHandleType.Pinned)
                param.text = handle.AddrOfPinnedObject()

                Dim err As DCAMERR
                err = dcamapidll.dcamprop_getvaluetext(hdcam, param)
                handle.Free()

                If failed(err) Then
                    ret = ""
                Else
                    Dim i As Integer
                    For i = 0 To buf.Count()
                        If buf(i) = 0 Then Exit For
                    Next
                    ret = Encoding.ASCII.GetString(buf).Substring(0, i)
                End If
                Return err
            End Function
        End Class

        ' ================================ dcamrec ================================
        ' Recording

        Class dcamrec
            Public Shared Function open(ByRef param As DCAMREC_OPEN) As DCAMERR
                Return dcamapidll.dcamrec_openA(param)
            End Function

            Public Shared Function status(ByVal hrec As IntPtr, ByRef param As DCAMREC_STATUS) As DCAMERR
                Return dcamapidll.dcamrec_status(hrec, param)
            End Function

            Public Shared Function close(ByVal hrec As IntPtr) As DCAMERR
                Return dcamapidll.dcamrec_close(hrec)
            End Function

            Public Shared Function lockframe(ByVal hrec As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As DCAMERR
                Return dcamapidll.dcamrec_lockframe(hrec, pFrame)
            End Function

            Public Shared Function copyframe(ByVal hrec As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As DCAMERR
                Return dcamapidll.dcamrec_copyframe(hrec, pFrame)
            End Function

            Public Shared Function writemetadata(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As DCAMERR
                Return dcamapidll.dcamrec_writemetadata(hrec, hdr)
            End Function

            Public Shared Function lockmetadata(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As DCAMERR
                Return dcamapidll.dcamrec_lockmetadata(hrec, hdr)
            End Function

            Public Shared Function copymetadata(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As DCAMERR
                Return dcamapidll.dcamrec_copymetadata(hrec, hdr)
            End Function

            Public Shared Function lockmetadatablock(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATABLOCKHDR) As DCAMERR
                Return dcamapidll.dcamrec_lockmetadatablock(hrec, hdr)
            End Function

            Public Shared Function copymetadatablock(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATABLOCKHDR) As DCAMERR
                Return dcamapidll.dcamrec_copymetadatablock(hrec, hdr)
            End Function
        End Class
    End Class
End Namespace

