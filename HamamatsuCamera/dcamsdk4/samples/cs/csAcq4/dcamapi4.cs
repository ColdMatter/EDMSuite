// dcamapi4.cs: Aug 5, 2024

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Hamamatsu.DCAM4
{
    // error code in DCAM-API
    public struct DCAMERR : IEquatable<DCAMERR>
    {
        private enum ERR : uint
        {
            BUSY                                = 0x80000101,   // API cannot process in busy state.
            NOTREADY                            = 0x80000103,   // API requires ready state.
            NOTSTABLE                           = 0x80000104,   // API requires stable or unstable state.
            UNSTABLE                            = 0x80000105,   // API does not support in unstable state.
            NOTBUSY                             = 0x80000107,   // API requires busy state.
            EXCLUDED                            = 0x80000110,   // some resource is exclusive and already used
            COOLINGTROUBLE                      = 0x80000302,   // something happens near cooler
            NOTRIGGER                           = 0x80000303,   // no trigger when necessary. Some camera supports this error.
            TEMPERATURE_TROUBLE                 = 0x80000304,   // camera warns its temperature
            TOOFREQUENTTRIGGER                  = 0x80000305,   // input too frequent trigger. Some camera supports this error.
            ABORT                               = 0x80000102,   // abort process
            TIMEOUT                             = 0x80000106,   // timeout
            LOSTFRAME                           = 0x80000301,   // frame data is lost
            MISSINGFRAME_TROUBLE                = 0x80000f06,   // frame is lost but reason is low lever driver's bug
            INVALIDIMAGE                        = 0x80000321,   // hpk format data is invalid data
            NORESOURCE                          = 0x80000201,   // not enough resource except memory
            NOMEMORY                            = 0x80000203,   // not enough memory
            NOMODULE                            = 0x80000204,   // no sub module
            NODRIVER                            = 0x80000205,   // no driver
            NOCAMERA                            = 0x80000206,   // no camera
            NOGRABBER                           = 0x80000207,   // no grabber
            NOCOMBINATION                       = 0x80000208,   // no combination on registry
            FAILOPEN                            = 0x80001001,   // DEPRECATED
            FRAMEGRABBER_NEEDS_FIRMWAREUPDATE   = 0x80001002,   // need to update frame grabber firmware to use the camera
            INVALIDMODULE                       = 0x80000211,   // dcam_init() found invalid module
            INVALIDCOMMPORT                     = 0x80000212,   // invalid serial port
            FAILOPENBUS                         = 0x81001001,   // the bus or driver are not available
            FAILOPENCAMERA                      = 0x82001001,   // camera report error during opening
            DEVICEPROBLEM                       = 0x82001002,   // initialization failed(for maico)
            INVALIDCAMERA                       = 0x80000806,   // invalid camera
            INVALIDHANDLE                       = 0x80000807,   // invalid camera handle
            INVALIDPARAM                        = 0x80000808,   // invalid parameter
            INVALIDVALUE                        = 0x80000821,   // invalid property value
            OUTOFRANGE                          = 0x80000822,   // value is out of range
            NOTWRITABLE                         = 0x80000823,   // the property is not writable
            NOTREADABLE                         = 0x80000824,   // the property is not readable
            INVALIDPROPERTYID                   = 0x80000825,   // the property id is invalid
            NEWAPIREQUIRED                      = 0x80000826,   // old API cannot present the value because only new API need to be used
            WRONGHANDSHAKE                      = 0x80000827,   // this error happens DCAM get error code from camera unexpectedly
            NOPROPERTY                          = 0x80000828,   // there is no altenative or influence id, or no more property id
            INVALIDCHANNEL                      = 0x80000829,   // the property id specifies channel but channel is invalid
            INVALIDVIEW                         = 0x8000082a,   // the property id specifies channel but channel is invalid
            INVALIDSUBARRAY                     = 0x8000082b,   // the combination of subarray values are invalid. e.g. DCAM_IDPROP_SUBARRAYHPOS + DCAM_IDPROP_SUBARRAYHSIZE is greater than the number of horizontal pixel of sensor.
            ACCESSDENY                          = 0x8000082c,   // the property cannot access during this DCAM STATUS
            NOVALUETEXT                         = 0x8000082d,   // the property does not have value text
            WRONGPROPERTYVALUE                  = 0x8000082e,   // at least one property value is wrong
            DISHARMONY                          = 0x80000830,   // the paired camera does not have same parameter
            FRAMEBUNDLESHOULDBEOFF              = 0x80000832,   // framebundle mode should be OFF under current property settings
            INVALIDFRAMEINDEX                   = 0x80000833,   // the frame index is invalid
            INVALIDSESSIONINDEX                 = 0x80000834,   // the session index is invalid
            NOCORRECTIONDATA                    = 0x80000838,   // not take the dark and shading correction data yet.
            CHANNELDEPENDENTVALUE               = 0x80000839,   // each channel has own property value so can't return overall property value.
            VIEWDEPENDENTVALUE                  = 0x8000083a,   // each view has own property value so can't return overall property value.
            NODEVICEBUFFER                      = 0x8000083b,   // the frame count is larger than device momory size on using device memory.
            REQUIREDSNAP                        = 0x8000083c,   // the capture mode is sequence on using device memory.
            LESSSYSTEMMEMORY                    = 0x8000083f,   // the sysmte memory size is too small. PC doesn't have enough memory or is limited memory by 32bit OS.
            INVALID_SELECTEDLINES               = 0x80000842,   // the combination of selected lines values are invalid. e.g. DCAM_IDPROP_SELECTEDLINES_VPOS + DCAM_IDPROP_SELECTEDLINES_VSIZE is greater than the number of vertical lines of sensor.
            NOTSUPPORT                          = 0x80000f03,   // camera does not support the function or property with current settings
            FAILREADCAMERA                      = 0x83001002,   // failed to read data from camera
            FAILWRITECAMERA                     = 0x83001003,   // failed to write data to the camera
            CONFLICTCOMMPORT                    = 0x83001004,   // conflict the com port name user set
            OPTICS_UNPLUGGED                    = 0x83001005,   // Optics part is unplugged so please check it.
            FAILCALIBRATION                     = 0x83001006,   // fail calibration
            MISMATCH_CONFIGURATION              = 0x83001011,   // mismatch between camera output(connection) and frame grabber specs
            INVALIDMEMBER_3                     = 0x84000103,   // 3th member variable is invalid value
            INVALIDMEMBER_5                     = 0x84000105,   // 5th member variable is invalid value
            INVALIDMEMBER_7                     = 0x84000107,   // 7th member variable is invalid value
            INVALIDMEMBER_8                     = 0x84000108,   // 7th member variable is invalid value
            INVALIDMEMBER_9                     = 0x84000109,   // 9th member variable is invalid value
            FAILEDOPENRECFILE                   = 0x84001001,   // DCAMREC failed to open the file
            INVALIDRECHANDLE                    = 0x84001002,   // DCAMREC is invalid handle
            FAILEDWRITEDATA                     = 0x84001003,   // DCAMREC failed to write the data
            FAILEDREADDATA                      = 0x84001004,   // DCAMREC failed to read the data
            NOWRECORDING                        = 0x84001005,   // DCAMREC is recording data now
            WRITEFULL                           = 0x84001006,   // DCAMREC writes full frame of the session
            ALREADYOCCUPIED                     = 0x84001007,   // DCAMREC handle is already occupied by other HDCAM
            TOOLARGEUSERDATASIZE                = 0x84001008,   // DCAMREC is set the large value to user data size
            INVALIDWAITHANDLE                   = 0x84002001,   // DCAMWAIT is invalid handle
            NEWRUNTIMEREQUIRED                  = 0x84002002,   // DCAM Module Version is older than the version that the camera requests
            VERSIONMISMATCH                     = 0x84002003,   // Camre returns the error on setting parameter to limit version
            RUNAS_FACTORYMODE                   = 0x84002004,   // Camera is running as a factory mode
            IMAGE_UNKNOWNSIGNATURE              = 0x84003001,   // sigunature of image header is unknown or corrupted
            IMAGE_NEWRUNTIMEREQUIRED            = 0x84003002,   // version of image header is newer than version that used DCAM supports
            IMAGE_ERRORSTATUSEXIST              = 0x84003003,   // image header stands error status
            IMAGE_HEADERCORRUPTED               = 0x84004004,   // image header value is strange
            IMAGE_BROKENCONTENT                 = 0x84004005,   // image content is corrupted
            UNKNOWNMSGID                        = 0x80000801,   // unknown message id
            UNKNOWNSTRID                        = 0x80000802,   // unknown string id
            UNKNOWNPARAMID                      = 0x80000803,   // unkown parameter id
            UNKNOWNBITSTYPE                     = 0x80000804,   // unknown bitmap bits type
            UNKNOWNDATATYPE                     = 0x80000805,   // unknown frame data type
            NONE                                = 0,            // no error, nothing to have done
            INSTALLATIONINPROGRESS              = 0x80000f00,   // installation progress
            UNREACH                             = 0x80000f01,   // internal error
            UNLOADED                            = 0x80000f04,   // calling after process terminated
            THRUADAPTER                         = 0x80000f05,   
            NOCONNECTION                        = 0x80000f07,   // HDCAM lost connection to camera
            NOTIMPLEMENT                        = 0x80000f02,   // not yet implementation
            DELAYEDFRAME                        = 0x80000f09,   // the frame waiting re-load from hardware buffer with SNAPSHOT(EX) of DEVICEBUFFER MODE
            FAILRELOADFRAME                     = 0x80000f0a,   // failed to re-load frame from hardware buffer with SNAPSHOT(EX) of DEVICEBUFFER MODE
            CANCELRELOADFRAME                   = 0x80000f0b,   // cancel to re-load frame from hardware buffer with SNAPSHOT(EX) of DEVICEBUFFER MODE
            DEVICEINITIALIZING                  = 0xb0000001,   
            APIINIT_INITOPTIONBYTES             = 0xa4010003,   // DCAMAPI_INIT::initoptionbytes is invalid
            APIINIT_INITOPTION                  = 0xa4010004,   // DCAMAPI_INIT::initoption is invalid
            INITOPTION_COLLISION_BASE           = 0xa401C000,   
            INITOPTION_COLLISION_MAX            = 0xa401FFFF,   
            MISSPROP_TRIGGERSOURCE              = 0xE0100110,   // the trigger mode is internal or syncreadout on using device memory.
            SUCCESS                             = 1,            // no error, general success code, app should check the value is positive

        }
        
        private readonly ERR val;

        public static readonly DCAMERR BUSY                             = new DCAMERR(ERR.BUSY);
        public static readonly DCAMERR NOTREADY                         = new DCAMERR(ERR.NOTREADY);
        public static readonly DCAMERR NOTSTABLE                        = new DCAMERR(ERR.NOTSTABLE);
        public static readonly DCAMERR UNSTABLE                         = new DCAMERR(ERR.UNSTABLE);
        public static readonly DCAMERR NOTBUSY                          = new DCAMERR(ERR.NOTBUSY);
        public static readonly DCAMERR EXCLUDED                         = new DCAMERR(ERR.EXCLUDED);
        public static readonly DCAMERR COOLINGTROUBLE                   = new DCAMERR(ERR.COOLINGTROUBLE);
        public static readonly DCAMERR NOTRIGGER                        = new DCAMERR(ERR.NOTRIGGER);
        public static readonly DCAMERR TEMPERATURE_TROUBLE              = new DCAMERR(ERR.TEMPERATURE_TROUBLE);
        public static readonly DCAMERR TOOFREQUENTTRIGGER               = new DCAMERR(ERR.TOOFREQUENTTRIGGER);
        public static readonly DCAMERR ABORT                            = new DCAMERR(ERR.ABORT);
        public static readonly DCAMERR TIMEOUT                          = new DCAMERR(ERR.TIMEOUT);
        public static readonly DCAMERR LOSTFRAME                        = new DCAMERR(ERR.LOSTFRAME);
        public static readonly DCAMERR MISSINGFRAME_TROUBLE             = new DCAMERR(ERR.MISSINGFRAME_TROUBLE);
        public static readonly DCAMERR INVALIDIMAGE                     = new DCAMERR(ERR.INVALIDIMAGE);
        public static readonly DCAMERR NORESOURCE                       = new DCAMERR(ERR.NORESOURCE);
        public static readonly DCAMERR NOMEMORY                         = new DCAMERR(ERR.NOMEMORY);
        public static readonly DCAMERR NOMODULE                         = new DCAMERR(ERR.NOMODULE);
        public static readonly DCAMERR NODRIVER                         = new DCAMERR(ERR.NODRIVER);
        public static readonly DCAMERR NOCAMERA                         = new DCAMERR(ERR.NOCAMERA);
        public static readonly DCAMERR NOGRABBER                        = new DCAMERR(ERR.NOGRABBER);
        public static readonly DCAMERR NOCOMBINATION                    = new DCAMERR(ERR.NOCOMBINATION);
        public static readonly DCAMERR FAILOPEN                         = new DCAMERR(ERR.FAILOPEN);
        public static readonly DCAMERR FRAMEGRABBER_NEEDS_FIRMWAREUPDATE= new DCAMERR(ERR.FRAMEGRABBER_NEEDS_FIRMWAREUPDATE);
        public static readonly DCAMERR INVALIDMODULE                    = new DCAMERR(ERR.INVALIDMODULE);
        public static readonly DCAMERR INVALIDCOMMPORT                  = new DCAMERR(ERR.INVALIDCOMMPORT);
        public static readonly DCAMERR FAILOPENBUS                      = new DCAMERR(ERR.FAILOPENBUS);
        public static readonly DCAMERR FAILOPENCAMERA                   = new DCAMERR(ERR.FAILOPENCAMERA);
        public static readonly DCAMERR DEVICEPROBLEM                    = new DCAMERR(ERR.DEVICEPROBLEM);
        public static readonly DCAMERR INVALIDCAMERA                    = new DCAMERR(ERR.INVALIDCAMERA);
        public static readonly DCAMERR INVALIDHANDLE                    = new DCAMERR(ERR.INVALIDHANDLE);
        public static readonly DCAMERR INVALIDPARAM                     = new DCAMERR(ERR.INVALIDPARAM);
        public static readonly DCAMERR INVALIDVALUE                     = new DCAMERR(ERR.INVALIDVALUE);
        public static readonly DCAMERR OUTOFRANGE                       = new DCAMERR(ERR.OUTOFRANGE);
        public static readonly DCAMERR NOTWRITABLE                      = new DCAMERR(ERR.NOTWRITABLE);
        public static readonly DCAMERR NOTREADABLE                      = new DCAMERR(ERR.NOTREADABLE);
        public static readonly DCAMERR INVALIDPROPERTYID                = new DCAMERR(ERR.INVALIDPROPERTYID);
        public static readonly DCAMERR NEWAPIREQUIRED                   = new DCAMERR(ERR.NEWAPIREQUIRED);
        public static readonly DCAMERR WRONGHANDSHAKE                   = new DCAMERR(ERR.WRONGHANDSHAKE);
        public static readonly DCAMERR NOPROPERTY                       = new DCAMERR(ERR.NOPROPERTY);
        public static readonly DCAMERR INVALIDCHANNEL                   = new DCAMERR(ERR.INVALIDCHANNEL);
        public static readonly DCAMERR INVALIDVIEW                      = new DCAMERR(ERR.INVALIDVIEW);
        public static readonly DCAMERR INVALIDSUBARRAY                  = new DCAMERR(ERR.INVALIDSUBARRAY);
        public static readonly DCAMERR ACCESSDENY                       = new DCAMERR(ERR.ACCESSDENY);
        public static readonly DCAMERR NOVALUETEXT                      = new DCAMERR(ERR.NOVALUETEXT);
        public static readonly DCAMERR WRONGPROPERTYVALUE               = new DCAMERR(ERR.WRONGPROPERTYVALUE);
        public static readonly DCAMERR DISHARMONY                       = new DCAMERR(ERR.DISHARMONY);
        public static readonly DCAMERR FRAMEBUNDLESHOULDBEOFF           = new DCAMERR(ERR.FRAMEBUNDLESHOULDBEOFF);
        public static readonly DCAMERR INVALIDFRAMEINDEX                = new DCAMERR(ERR.INVALIDFRAMEINDEX);
        public static readonly DCAMERR INVALIDSESSIONINDEX              = new DCAMERR(ERR.INVALIDSESSIONINDEX);
        public static readonly DCAMERR NOCORRECTIONDATA                 = new DCAMERR(ERR.NOCORRECTIONDATA);
        public static readonly DCAMERR CHANNELDEPENDENTVALUE            = new DCAMERR(ERR.CHANNELDEPENDENTVALUE);
        public static readonly DCAMERR VIEWDEPENDENTVALUE               = new DCAMERR(ERR.VIEWDEPENDENTVALUE);
        public static readonly DCAMERR NODEVICEBUFFER                   = new DCAMERR(ERR.NODEVICEBUFFER);
        public static readonly DCAMERR REQUIREDSNAP                     = new DCAMERR(ERR.REQUIREDSNAP);
        public static readonly DCAMERR LESSSYSTEMMEMORY                 = new DCAMERR(ERR.LESSSYSTEMMEMORY);
        public static readonly DCAMERR INVALID_SELECTEDLINES            = new DCAMERR(ERR.INVALID_SELECTEDLINES);
        public static readonly DCAMERR NOTSUPPORT                       = new DCAMERR(ERR.NOTSUPPORT);
        public static readonly DCAMERR FAILREADCAMERA                   = new DCAMERR(ERR.FAILREADCAMERA);
        public static readonly DCAMERR FAILWRITECAMERA                  = new DCAMERR(ERR.FAILWRITECAMERA);
        public static readonly DCAMERR CONFLICTCOMMPORT                 = new DCAMERR(ERR.CONFLICTCOMMPORT);
        public static readonly DCAMERR OPTICS_UNPLUGGED                 = new DCAMERR(ERR.OPTICS_UNPLUGGED);
        public static readonly DCAMERR FAILCALIBRATION                  = new DCAMERR(ERR.FAILCALIBRATION);
        public static readonly DCAMERR MISMATCH_CONFIGURATION           = new DCAMERR(ERR.MISMATCH_CONFIGURATION);
        public static readonly DCAMERR INVALIDMEMBER_3                  = new DCAMERR(ERR.INVALIDMEMBER_3);
        public static readonly DCAMERR INVALIDMEMBER_5                  = new DCAMERR(ERR.INVALIDMEMBER_5);
        public static readonly DCAMERR INVALIDMEMBER_7                  = new DCAMERR(ERR.INVALIDMEMBER_7);
        public static readonly DCAMERR INVALIDMEMBER_8                  = new DCAMERR(ERR.INVALIDMEMBER_8);
        public static readonly DCAMERR INVALIDMEMBER_9                  = new DCAMERR(ERR.INVALIDMEMBER_9);
        public static readonly DCAMERR FAILEDOPENRECFILE                = new DCAMERR(ERR.FAILEDOPENRECFILE);
        public static readonly DCAMERR INVALIDRECHANDLE                 = new DCAMERR(ERR.INVALIDRECHANDLE);
        public static readonly DCAMERR FAILEDWRITEDATA                  = new DCAMERR(ERR.FAILEDWRITEDATA);
        public static readonly DCAMERR FAILEDREADDATA                   = new DCAMERR(ERR.FAILEDREADDATA);
        public static readonly DCAMERR NOWRECORDING                     = new DCAMERR(ERR.NOWRECORDING);
        public static readonly DCAMERR WRITEFULL                        = new DCAMERR(ERR.WRITEFULL);
        public static readonly DCAMERR ALREADYOCCUPIED                  = new DCAMERR(ERR.ALREADYOCCUPIED);
        public static readonly DCAMERR TOOLARGEUSERDATASIZE             = new DCAMERR(ERR.TOOLARGEUSERDATASIZE);
        public static readonly DCAMERR INVALIDWAITHANDLE                = new DCAMERR(ERR.INVALIDWAITHANDLE);
        public static readonly DCAMERR NEWRUNTIMEREQUIRED               = new DCAMERR(ERR.NEWRUNTIMEREQUIRED);
        public static readonly DCAMERR VERSIONMISMATCH                  = new DCAMERR(ERR.VERSIONMISMATCH);
        public static readonly DCAMERR RUNAS_FACTORYMODE                = new DCAMERR(ERR.RUNAS_FACTORYMODE);
        public static readonly DCAMERR IMAGE_UNKNOWNSIGNATURE           = new DCAMERR(ERR.IMAGE_UNKNOWNSIGNATURE);
        public static readonly DCAMERR IMAGE_NEWRUNTIMEREQUIRED         = new DCAMERR(ERR.IMAGE_NEWRUNTIMEREQUIRED);
        public static readonly DCAMERR IMAGE_ERRORSTATUSEXIST           = new DCAMERR(ERR.IMAGE_ERRORSTATUSEXIST);
        public static readonly DCAMERR IMAGE_HEADERCORRUPTED            = new DCAMERR(ERR.IMAGE_HEADERCORRUPTED);
        public static readonly DCAMERR IMAGE_BROKENCONTENT              = new DCAMERR(ERR.IMAGE_BROKENCONTENT);
        public static readonly DCAMERR UNKNOWNMSGID                     = new DCAMERR(ERR.UNKNOWNMSGID);
        public static readonly DCAMERR UNKNOWNSTRID                     = new DCAMERR(ERR.UNKNOWNSTRID);
        public static readonly DCAMERR UNKNOWNPARAMID                   = new DCAMERR(ERR.UNKNOWNPARAMID);
        public static readonly DCAMERR UNKNOWNBITSTYPE                  = new DCAMERR(ERR.UNKNOWNBITSTYPE);
        public static readonly DCAMERR UNKNOWNDATATYPE                  = new DCAMERR(ERR.UNKNOWNDATATYPE);
        public static readonly DCAMERR NONE                             = new DCAMERR(ERR.NONE);
        public static readonly DCAMERR INSTALLATIONINPROGRESS           = new DCAMERR(ERR.INSTALLATIONINPROGRESS);
        public static readonly DCAMERR UNREACH                          = new DCAMERR(ERR.UNREACH);
        public static readonly DCAMERR UNLOADED                         = new DCAMERR(ERR.UNLOADED);
        public static readonly DCAMERR THRUADAPTER                      = new DCAMERR(ERR.THRUADAPTER);
        public static readonly DCAMERR NOCONNECTION                     = new DCAMERR(ERR.NOCONNECTION);
        public static readonly DCAMERR NOTIMPLEMENT                     = new DCAMERR(ERR.NOTIMPLEMENT);
        public static readonly DCAMERR DELAYEDFRAME                     = new DCAMERR(ERR.DELAYEDFRAME);
        public static readonly DCAMERR FAILRELOADFRAME                  = new DCAMERR(ERR.FAILRELOADFRAME);
        public static readonly DCAMERR CANCELRELOADFRAME                = new DCAMERR(ERR.CANCELRELOADFRAME);
        public static readonly DCAMERR DEVICEINITIALIZING               = new DCAMERR(ERR.DEVICEINITIALIZING);
        public static readonly DCAMERR APIINIT_INITOPTIONBYTES          = new DCAMERR(ERR.APIINIT_INITOPTIONBYTES);
        public static readonly DCAMERR APIINIT_INITOPTION               = new DCAMERR(ERR.APIINIT_INITOPTION);
        public static readonly DCAMERR INITOPTION_COLLISION_BASE        = new DCAMERR(ERR.INITOPTION_COLLISION_BASE);
        public static readonly DCAMERR INITOPTION_COLLISION_MAX         = new DCAMERR(ERR.INITOPTION_COLLISION_MAX);
        public static readonly DCAMERR MISSPROP_TRIGGERSOURCE           = new DCAMERR(ERR.MISSPROP_TRIGGERSOURCE);
        public static readonly DCAMERR SUCCESS                          = new DCAMERR(ERR.SUCCESS);

        private DCAMERR(ERR argv)
        {
            val = argv;
        }

        public string text()
        {
            switch (val)
            {
            case ERR.BUSY:                              return "BUSY";
            case ERR.NOTREADY:                          return "NOTREADY";
            case ERR.NOTSTABLE:                         return "NOTSTABLE";
            case ERR.UNSTABLE:                          return "UNSTABLE";
            case ERR.NOTBUSY:                           return "NOTBUSY";
            case ERR.EXCLUDED:                          return "EXCLUDED";
            case ERR.COOLINGTROUBLE:                    return "COOLINGTROUBLE";
            case ERR.NOTRIGGER:                         return "NOTRIGGER";
            case ERR.TEMPERATURE_TROUBLE:               return "TEMPERATURE_TROUBLE";
            case ERR.TOOFREQUENTTRIGGER:                return "TOOFREQUENTTRIGGER";
            case ERR.ABORT:                             return "ABORT";
            case ERR.TIMEOUT:                           return "TIMEOUT";
            case ERR.LOSTFRAME:                         return "LOSTFRAME";
            case ERR.MISSINGFRAME_TROUBLE:              return "MISSINGFRAME_TROUBLE";
            case ERR.INVALIDIMAGE:                      return "INVALIDIMAGE";
            case ERR.NORESOURCE:                        return "NORESOURCE";
            case ERR.NOMEMORY:                          return "NOMEMORY";
            case ERR.NOMODULE:                          return "NOMODULE";
            case ERR.NODRIVER:                          return "NODRIVER";
            case ERR.NOCAMERA:                          return "NOCAMERA";
            case ERR.NOGRABBER:                         return "NOGRABBER";
            case ERR.NOCOMBINATION:                     return "NOCOMBINATION";
            case ERR.FAILOPEN:                          return "FAILOPEN";
            case ERR.FRAMEGRABBER_NEEDS_FIRMWAREUPDATE: return "FRAMEGRABBER_NEEDS_FIRMWAREUPDATE";
            case ERR.INVALIDMODULE:                     return "INVALIDMODULE";
            case ERR.INVALIDCOMMPORT:                   return "INVALIDCOMMPORT";
            case ERR.FAILOPENBUS:                       return "FAILOPENBUS";
            case ERR.FAILOPENCAMERA:                    return "FAILOPENCAMERA";
            case ERR.DEVICEPROBLEM:                     return "DEVICEPROBLEM";
            case ERR.INVALIDCAMERA:                     return "INVALIDCAMERA";
            case ERR.INVALIDHANDLE:                     return "INVALIDHANDLE";
            case ERR.INVALIDPARAM:                      return "INVALIDPARAM";
            case ERR.INVALIDVALUE:                      return "INVALIDVALUE";
            case ERR.OUTOFRANGE:                        return "OUTOFRANGE";
            case ERR.NOTWRITABLE:                       return "NOTWRITABLE";
            case ERR.NOTREADABLE:                       return "NOTREADABLE";
            case ERR.INVALIDPROPERTYID:                 return "INVALIDPROPERTYID";
            case ERR.NEWAPIREQUIRED:                    return "NEWAPIREQUIRED";
            case ERR.WRONGHANDSHAKE:                    return "WRONGHANDSHAKE";
            case ERR.NOPROPERTY:                        return "NOPROPERTY";
            case ERR.INVALIDCHANNEL:                    return "INVALIDCHANNEL";
            case ERR.INVALIDVIEW:                       return "INVALIDVIEW";
            case ERR.INVALIDSUBARRAY:                   return "INVALIDSUBARRAY";
            case ERR.ACCESSDENY:                        return "ACCESSDENY";
            case ERR.NOVALUETEXT:                       return "NOVALUETEXT";
            case ERR.WRONGPROPERTYVALUE:                return "WRONGPROPERTYVALUE";
            case ERR.DISHARMONY:                        return "DISHARMONY";
            case ERR.FRAMEBUNDLESHOULDBEOFF:            return "FRAMEBUNDLESHOULDBEOFF";
            case ERR.INVALIDFRAMEINDEX:                 return "INVALIDFRAMEINDEX";
            case ERR.INVALIDSESSIONINDEX:               return "INVALIDSESSIONINDEX";
            case ERR.NOCORRECTIONDATA:                  return "NOCORRECTIONDATA";
            case ERR.CHANNELDEPENDENTVALUE:             return "CHANNELDEPENDENTVALUE";
            case ERR.VIEWDEPENDENTVALUE:                return "VIEWDEPENDENTVALUE";
            case ERR.NODEVICEBUFFER:                    return "NODEVICEBUFFER";
            case ERR.REQUIREDSNAP:                      return "REQUIREDSNAP";
            case ERR.LESSSYSTEMMEMORY:                  return "LESSSYSTEMMEMORY";
            case ERR.INVALID_SELECTEDLINES:             return "INVALID_SELECTEDLINES";
            case ERR.NOTSUPPORT:                        return "NOTSUPPORT";
            case ERR.FAILREADCAMERA:                    return "FAILREADCAMERA";
            case ERR.FAILWRITECAMERA:                   return "FAILWRITECAMERA";
            case ERR.CONFLICTCOMMPORT:                  return "CONFLICTCOMMPORT";
            case ERR.OPTICS_UNPLUGGED:                  return "OPTICS_UNPLUGGED";
            case ERR.FAILCALIBRATION:                   return "FAILCALIBRATION";
            case ERR.MISMATCH_CONFIGURATION:            return "MISMATCH_CONFIGURATION";
            case ERR.INVALIDMEMBER_3:                   return "INVALIDMEMBER_3";
            case ERR.INVALIDMEMBER_5:                   return "INVALIDMEMBER_5";
            case ERR.INVALIDMEMBER_7:                   return "INVALIDMEMBER_7";
            case ERR.INVALIDMEMBER_8:                   return "INVALIDMEMBER_8";
            case ERR.INVALIDMEMBER_9:                   return "INVALIDMEMBER_9";
            case ERR.FAILEDOPENRECFILE:                 return "FAILEDOPENRECFILE";
            case ERR.INVALIDRECHANDLE:                  return "INVALIDRECHANDLE";
            case ERR.FAILEDWRITEDATA:                   return "FAILEDWRITEDATA";
            case ERR.FAILEDREADDATA:                    return "FAILEDREADDATA";
            case ERR.NOWRECORDING:                      return "NOWRECORDING";
            case ERR.WRITEFULL:                         return "WRITEFULL";
            case ERR.ALREADYOCCUPIED:                   return "ALREADYOCCUPIED";
            case ERR.TOOLARGEUSERDATASIZE:              return "TOOLARGEUSERDATASIZE";
            case ERR.INVALIDWAITHANDLE:                 return "INVALIDWAITHANDLE";
            case ERR.NEWRUNTIMEREQUIRED:                return "NEWRUNTIMEREQUIRED";
            case ERR.VERSIONMISMATCH:                   return "VERSIONMISMATCH";
            case ERR.RUNAS_FACTORYMODE:                 return "RUNAS_FACTORYMODE";
            case ERR.IMAGE_UNKNOWNSIGNATURE:            return "IMAGE_UNKNOWNSIGNATURE";
            case ERR.IMAGE_NEWRUNTIMEREQUIRED:          return "IMAGE_NEWRUNTIMEREQUIRED";
            case ERR.IMAGE_ERRORSTATUSEXIST:            return "IMAGE_ERRORSTATUSEXIST";
            case ERR.IMAGE_HEADERCORRUPTED:             return "IMAGE_HEADERCORRUPTED";
            case ERR.IMAGE_BROKENCONTENT:               return "IMAGE_BROKENCONTENT";
            case ERR.UNKNOWNMSGID:                      return "UNKNOWNMSGID";
            case ERR.UNKNOWNSTRID:                      return "UNKNOWNSTRID";
            case ERR.UNKNOWNPARAMID:                    return "UNKNOWNPARAMID";
            case ERR.UNKNOWNBITSTYPE:                   return "UNKNOWNBITSTYPE";
            case ERR.UNKNOWNDATATYPE:                   return "UNKNOWNDATATYPE";
            case ERR.NONE:                              return "NONE";
            case ERR.INSTALLATIONINPROGRESS:            return "INSTALLATIONINPROGRESS";
            case ERR.UNREACH:                           return "UNREACH";
            case ERR.UNLOADED:                          return "UNLOADED";
            case ERR.THRUADAPTER:                       return "THRUADAPTER";
            case ERR.NOCONNECTION:                      return "NOCONNECTION";
            case ERR.NOTIMPLEMENT:                      return "NOTIMPLEMENT";
            case ERR.DELAYEDFRAME:                      return "DELAYEDFRAME";
            case ERR.FAILRELOADFRAME:                   return "FAILRELOADFRAME";
            case ERR.CANCELRELOADFRAME:                 return "CANCELRELOADFRAME";
            case ERR.DEVICEINITIALIZING:                return "DEVICEINITIALIZING";
            case ERR.APIINIT_INITOPTIONBYTES:           return "APIINIT_INITOPTIONBYTES";
            case ERR.APIINIT_INITOPTION:                return "APIINIT_INITOPTION";
            case ERR.INITOPTION_COLLISION_BASE:         return "INITOPTION_COLLISION_BASE";
            case ERR.INITOPTION_COLLISION_MAX:          return "INITOPTION_COLLISION_MAX";
            case ERR.MISSPROP_TRIGGERSOURCE:            return "MISSPROP_TRIGGERSOURCE";
            case ERR.SUCCESS:                           return "SUCCESS";

            }
            return val.ToString();
        }

        public static implicit operator int(DCAMERR self)
        {
            return (int)self.val;
        }
        public static bool operator ==(DCAMERR a, DCAMERR b)
        {
            return a.val == b.val;
        }
        public static bool operator !=(DCAMERR a, DCAMERR b)
        {
            return a.val != b.val;
        }
        public override int GetHashCode()
        {
            return val.GetHashCode();
        }
        public bool Equals(DCAMERR err)
        {
            return this == err;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMERR))
                return false;

            return Equals((DCAMERR)obj);
        }

        public DCAMERR(UInt32 argv)
        {
            val = (ERR)argv;
        }

        public bool failed()
        {
            return (uint)val >= 0x80000000;
        }
    }

    public struct DCAMIDSTR : IEquatable<DCAMIDSTR>
    {
        private uint id;

        public static readonly DCAMIDSTR BUS                = new DCAMIDSTR(0x04000101);
        public static readonly DCAMIDSTR CAMERAID           = new DCAMIDSTR(0x04000102);
        public static readonly DCAMIDSTR VENDOR             = new DCAMIDSTR(0x04000103);
        public static readonly DCAMIDSTR MODEL              = new DCAMIDSTR(0x04000104);
        public static readonly DCAMIDSTR CAMERAVERSION      = new DCAMIDSTR(0x04000105);
        public static readonly DCAMIDSTR DRIVERVERSION      = new DCAMIDSTR(0x04000106);
        public static readonly DCAMIDSTR MODULEVERSION      = new DCAMIDSTR(0x04000107);
        public static readonly DCAMIDSTR DCAMAPIVERSION     = new DCAMIDSTR(0x04000108);
        public static readonly DCAMIDSTR SUBUNIT_INFO1      = new DCAMIDSTR(0x04000110);
        public static readonly DCAMIDSTR SUBUNIT_INFO2      = new DCAMIDSTR(0x04000111);
        public static readonly DCAMIDSTR SUBUNIT_INFO3      = new DCAMIDSTR(0x04000112);
        public static readonly DCAMIDSTR SUBUNIT_INFO4      = new DCAMIDSTR(0x04000113);
        public static readonly DCAMIDSTR CAMERA_SERIESNAME  = new DCAMIDSTR(0x0400012c);


        public DCAMIDSTR(uint v)
        {
            id = v;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public bool Equals(DCAMIDSTR a)
        {
            return id == a.id;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMIDSTR))
                return false;

            return Equals((DCAMIDSTR)obj);
        }

        public static implicit operator uint(DCAMIDSTR self)
        {
            return self.id;
        }
        public static implicit operator int(DCAMIDSTR self)
        {
            return (int)self.id;
        }
    }


    public struct DCAM_PIXELTYPE : IEquatable<DCAM_PIXELTYPE>
    {
        private int pixeltype;

        public static readonly DCAM_PIXELTYPE MONO8     = new DCAM_PIXELTYPE(0x00000001);
        public static readonly DCAM_PIXELTYPE MONO16    = new DCAM_PIXELTYPE(0x00000002);
        public static readonly DCAM_PIXELTYPE MONO12    = new DCAM_PIXELTYPE(0x00000003);
        public static readonly DCAM_PIXELTYPE MONO12P   = new DCAM_PIXELTYPE(0x00000005);
        public static readonly DCAM_PIXELTYPE RGB24     = new DCAM_PIXELTYPE(0x00000021);
        public static readonly DCAM_PIXELTYPE RGB48     = new DCAM_PIXELTYPE(0x00000022);
        public static readonly DCAM_PIXELTYPE BGR24     = new DCAM_PIXELTYPE(0x00000029);
        public static readonly DCAM_PIXELTYPE BGR48     = new DCAM_PIXELTYPE(0x0000002a);
        public static readonly DCAM_PIXELTYPE NONE      = new DCAM_PIXELTYPE(0x00000000);


        public DCAM_PIXELTYPE(int v)
        {
            pixeltype = v;
        }

        public override int GetHashCode()
        {
            return pixeltype.GetHashCode();
        }

        public bool Equals(DCAM_PIXELTYPE a)
        {
            return pixeltype == a.pixeltype;
        }
        public override bool Equals(object obj)
        {
            if( !(obj is DCAM_PIXELTYPE))
                return false;

            return Equals((DCAM_PIXELTYPE)obj);
        }

        public static implicit operator uint(DCAM_PIXELTYPE self)
        {
            return (uint)self.pixeltype;
        }
        public static implicit operator int(DCAM_PIXELTYPE self)
        {
            return self.pixeltype;
        }
    }

    // Property ID for dcamprop functions
    public struct DCAMIDPROP : IEquatable<DCAMIDPROP>
    {
        private readonly uint idprop;

        public static readonly DCAMIDPROP TRIGGERSOURCE                     = new DCAMIDPROP(0x00100110);
        public static readonly DCAMIDPROP TRIGGERACTIVE                     = new DCAMIDPROP(0x00100120);
        public static readonly DCAMIDPROP TRIGGER_MODE                      = new DCAMIDPROP(0x00100210);
        public static readonly DCAMIDPROP TRIGGERPOLARITY                   = new DCAMIDPROP(0x00100220);
        public static readonly DCAMIDPROP TRIGGER_CONNECTOR                 = new DCAMIDPROP(0x00100230);
        public static readonly DCAMIDPROP TRIGGERTIMES                      = new DCAMIDPROP(0x00100240);
        public static readonly DCAMIDPROP TRIGGERDELAY                      = new DCAMIDPROP(0x00100260);
        public static readonly DCAMIDPROP INTERNALTRIGGER_HANDLING          = new DCAMIDPROP(0x00100270);
        public static readonly DCAMIDPROP TRIGGERMULTIFRAME_COUNT           = new DCAMIDPROP(0x00100280);
        public static readonly DCAMIDPROP SYNCREADOUT_SYSTEMBLANK           = new DCAMIDPROP(0x00100290);
        public static readonly DCAMIDPROP TRIGGERENABLE_ACTIVE              = new DCAMIDPROP(0x00100410);
        public static readonly DCAMIDPROP TRIGGERENABLE_POLARITY            = new DCAMIDPROP(0x00100420);
        public static readonly DCAMIDPROP TRIGGERENABLE_SOURCE              = new DCAMIDPROP(0x00100430);
        public static readonly DCAMIDPROP TRIGGERENABLE_BURSTTIMES          = new DCAMIDPROP(0x00100440);
        public static readonly DCAMIDPROP TRIGGERNUMBER_FORFIRSTIMAGE       = new DCAMIDPROP(0x00100810);
        public static readonly DCAMIDPROP TRIGGERNUMBER_FORNEXTIMAGE        = new DCAMIDPROP(0x00100820);
        public static readonly DCAMIDPROP NUMBEROF_OUTPUTTRIGGERCONNECTOR   = new DCAMIDPROP(0x001C0010);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_CHANNELSYNC         = new DCAMIDPROP(0x001C0030);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_PROGRAMABLESTART    = new DCAMIDPROP(0x001C0050);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_SOURCE              = new DCAMIDPROP(0x001C0110);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_POLARITY            = new DCAMIDPROP(0x001C0120);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_ACTIVE              = new DCAMIDPROP(0x001C0130);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_DELAY               = new DCAMIDPROP(0x001C0140);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_PERIOD              = new DCAMIDPROP(0x001C0150);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_KIND                = new DCAMIDPROP(0x001C0160);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_BASESENSOR          = new DCAMIDPROP(0x001C0170);
        public static readonly DCAMIDPROP OUTPUTTRIGGER_PREHSYNCCOUNT       = new DCAMIDPROP(0x001C0190);
        public static readonly DCAMIDPROP _OUTPUTTRIGGER                    = new DCAMIDPROP(0x00000100);
        public static readonly DCAMIDPROP MASTERPULSE_MODE                  = new DCAMIDPROP(0x001E0020);
        public static readonly DCAMIDPROP MASTERPULSE_TRIGGERSOURCE         = new DCAMIDPROP(0x001E0030);
        public static readonly DCAMIDPROP MASTERPULSE_INTERVAL              = new DCAMIDPROP(0x001E0040);
        public static readonly DCAMIDPROP MASTERPULSE_BURSTTIMES            = new DCAMIDPROP(0x001E0050);
        public static readonly DCAMIDPROP EXPOSURETIME                      = new DCAMIDPROP(0x001F0110);
        public static readonly DCAMIDPROP EXPOSURETIME_CONTROL              = new DCAMIDPROP(0x001F0130);
        public static readonly DCAMIDPROP TRIGGER_FIRSTEXPOSURE             = new DCAMIDPROP(0x001F0200);
        public static readonly DCAMIDPROP TRIGGER_GLOBALEXPOSURE            = new DCAMIDPROP(0x001F0300);
        public static readonly DCAMIDPROP FIRSTTRIGGER_BEHAVIOR             = new DCAMIDPROP(0x001F0310);
        public static readonly DCAMIDPROP MULTIFRAME_EXPOSURE               = new DCAMIDPROP(0x001F1000);
        public static readonly DCAMIDPROP _MULTIFRAME                       = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP LIGHTMODE                         = new DCAMIDPROP(0x00200110);
        public static readonly DCAMIDPROP SENSITIVITYMODE                   = new DCAMIDPROP(0x00200210);
        public static readonly DCAMIDPROP SENSITIVITY                       = new DCAMIDPROP(0x00200220);
        public static readonly DCAMIDPROP DIRECTEMGAIN_MODE                 = new DCAMIDPROP(0x00200250);
        public static readonly DCAMIDPROP EMGAINWARNING_STATUS              = new DCAMIDPROP(0x00200260);
        public static readonly DCAMIDPROP EMGAINWARNING_LEVEL               = new DCAMIDPROP(0x00200270);
        public static readonly DCAMIDPROP EMGAINWARNING_ALARM               = new DCAMIDPROP(0x00200280);
        public static readonly DCAMIDPROP EMGAINPROTECT_MODE                = new DCAMIDPROP(0x00200290);
        public static readonly DCAMIDPROP EMGAINPROTECT_AFTERFRAMES         = new DCAMIDPROP(0x002002A0);
        public static readonly DCAMIDPROP MEASURED_SENSITIVITY              = new DCAMIDPROP(0x002002B0);
        public static readonly DCAMIDPROP PHOTONIMAGINGMODE                 = new DCAMIDPROP(0x002002F0);
        public static readonly DCAMIDPROP SENSORTEMPERATURE                 = new DCAMIDPROP(0x00200310);
        public static readonly DCAMIDPROP SENSORCOOLER                      = new DCAMIDPROP(0x00200320);
        public static readonly DCAMIDPROP SENSORTEMPERATURETARGET           = new DCAMIDPROP(0x00200330);
        public static readonly DCAMIDPROP SENSORCOOLERSTATUS                = new DCAMIDPROP(0x00200340);
        public static readonly DCAMIDPROP SENSORCOOLERFAN                   = new DCAMIDPROP(0x00200350);
        public static readonly DCAMIDPROP SENSORTEMPERATURE_AVE             = new DCAMIDPROP(0x00200360);
        public static readonly DCAMIDPROP SENSORTEMPERATURE_MIN             = new DCAMIDPROP(0x00200370);
        public static readonly DCAMIDPROP SENSORTEMPERATURE_MAX             = new DCAMIDPROP(0x00200380);
        public static readonly DCAMIDPROP SENSORTEMPERATURE_STATUS          = new DCAMIDPROP(0x00200390);
        public static readonly DCAMIDPROP SENSORTEMPERATURE_PROTECT         = new DCAMIDPROP(0x00200400);
        public static readonly DCAMIDPROP MECHANICALSHUTTER                 = new DCAMIDPROP(0x00200410);
        public static readonly DCAMIDPROP CONTRASTGAIN                      = new DCAMIDPROP(0x00300120);
        public static readonly DCAMIDPROP CONTRASTOFFSET                    = new DCAMIDPROP(0x00300130);
        public static readonly DCAMIDPROP HIGHDYNAMICRANGE_MODE             = new DCAMIDPROP(0x00300150);
        public static readonly DCAMIDPROP DIRECTGAIN_MODE                   = new DCAMIDPROP(0x00300160);
        public static readonly DCAMIDPROP REALTIMEGAINCORRECT_MODE          = new DCAMIDPROP(0x00300170);
        public static readonly DCAMIDPROP REALTIMEGAINCORRECT_LEVEL         = new DCAMIDPROP(0x00300180);
        public static readonly DCAMIDPROP REALTIMEGAINCORRECT_INTERVAL      = new DCAMIDPROP(0x00300190);
        public static readonly DCAMIDPROP NUMBEROF_REALTIMEGAINCORRECTREGION= new DCAMIDPROP(0x003001A0);
        public static readonly DCAMIDPROP VIVIDCOLOR                        = new DCAMIDPROP(0x00300200);
        public static readonly DCAMIDPROP WHITEBALANCEMODE                  = new DCAMIDPROP(0x00300210);
        public static readonly DCAMIDPROP WHITEBALANCETEMPERATURE           = new DCAMIDPROP(0x00300220);
        public static readonly DCAMIDPROP WHITEBALANCEUSERPRESET            = new DCAMIDPROP(0x00300230);
        public static readonly DCAMIDPROP REALTIMEGAINCORRECTREGION_HPOS    = new DCAMIDPROP(0x00301000);
        public static readonly DCAMIDPROP REALTIMEGAINCORRECTREGION_HSIZE   = new DCAMIDPROP(0x00302000);
        public static readonly DCAMIDPROP _REALTIMEGAINCORRECTIONREGION     = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP INTERFRAMEALU_ENABLE              = new DCAMIDPROP(0x00380010);
        public static readonly DCAMIDPROP RECURSIVEFILTER                   = new DCAMIDPROP(0x00380110);
        public static readonly DCAMIDPROP RECURSIVEFILTERFRAMES             = new DCAMIDPROP(0x00380120);
        public static readonly DCAMIDPROP SPOTNOISEREDUCER                  = new DCAMIDPROP(0x00380130);
        public static readonly DCAMIDPROP SUBTRACT                          = new DCAMIDPROP(0x00380210);
        public static readonly DCAMIDPROP SUBTRACTIMAGEMEMORY               = new DCAMIDPROP(0x00380220);
        public static readonly DCAMIDPROP STORESUBTRACTIMAGETOMEMORY        = new DCAMIDPROP(0x00380230);
        public static readonly DCAMIDPROP SUBTRACTOFFSET                    = new DCAMIDPROP(0x00380240);
        public static readonly DCAMIDPROP DARKCALIB_STABLEMAXINTENSITY      = new DCAMIDPROP(0x00380250);
        public static readonly DCAMIDPROP SUBTRACT_DATASTATUS               = new DCAMIDPROP(0x003802F0);
        public static readonly DCAMIDPROP SHADINGCALIB_DATASTATUS           = new DCAMIDPROP(0x00380300);
        public static readonly DCAMIDPROP SHADINGCORRECTION                 = new DCAMIDPROP(0x00380310);
        public static readonly DCAMIDPROP SHADINGCALIBDATAMEMORY            = new DCAMIDPROP(0x00380320);
        public static readonly DCAMIDPROP STORESHADINGCALIBDATATOMEMORY     = new DCAMIDPROP(0x00380330);
        public static readonly DCAMIDPROP SHADINGCALIB_METHOD               = new DCAMIDPROP(0x00380340);
        public static readonly DCAMIDPROP SHADINGCALIB_TARGET               = new DCAMIDPROP(0x00380350);
        public static readonly DCAMIDPROP SHADINGCALIB_STABLEMININTENSITY   = new DCAMIDPROP(0x00380360);
        public static readonly DCAMIDPROP SHADINGCALIB_SAMPLES              = new DCAMIDPROP(0x00380370);
        public static readonly DCAMIDPROP SHADINGCALIB_STABLESAMPLES        = new DCAMIDPROP(0x00380380);
        public static readonly DCAMIDPROP SHADINGCALIB_STABLEMAXERRORPERCENT= new DCAMIDPROP(0x00380390);
        public static readonly DCAMIDPROP FRAMEAVERAGINGMODE                = new DCAMIDPROP(0x003803A0);
        public static readonly DCAMIDPROP FRAMEAVERAGINGFRAMES              = new DCAMIDPROP(0x003803B0);
        public static readonly DCAMIDPROP DARKCALIB_STABLESAMPLES           = new DCAMIDPROP(0x003803C0);
        public static readonly DCAMIDPROP DARKCALIB_SAMPLES                 = new DCAMIDPROP(0x003803D0);
        public static readonly DCAMIDPROP DARKCALIB_TARGET                  = new DCAMIDPROP(0x003803E0);
        public static readonly DCAMIDPROP CAPTUREMODE                       = new DCAMIDPROP(0x00380410);
        public static readonly DCAMIDPROP LINEAVERAGING                     = new DCAMIDPROP(0x00380450);
        public static readonly DCAMIDPROP IMAGEFILTER                       = new DCAMIDPROP(0x00380460);
        public static readonly DCAMIDPROP INTENSITYLUT_MODE                 = new DCAMIDPROP(0x00380510);
        public static readonly DCAMIDPROP INTENSITYLUT_PAGE                 = new DCAMIDPROP(0x00380520);
        public static readonly DCAMIDPROP INTENSITYLUT_WHITECLIP            = new DCAMIDPROP(0x00380530);
        public static readonly DCAMIDPROP INTENSITYLUT_BLACKCLIP            = new DCAMIDPROP(0x00380540);
        public static readonly DCAMIDPROP INTENSITY_GAMMA                   = new DCAMIDPROP(0x00380560);
        public static readonly DCAMIDPROP SENSORGAPCORRECT_MODE             = new DCAMIDPROP(0x00380620);
        public static readonly DCAMIDPROP ADVANCEDEDGEENHANCEMENT_MODE      = new DCAMIDPROP(0x00380630);
        public static readonly DCAMIDPROP ADVANCEDEDGEENHANCEMENT_LEVEL     = new DCAMIDPROP(0x00380640);
        public static readonly DCAMIDPROP SHADINGCALIB_TARGETMIN            = new DCAMIDPROP(0x00380680);
        public static readonly DCAMIDPROP TAPGAINCALIB_METHOD               = new DCAMIDPROP(0x00380F10);
        public static readonly DCAMIDPROP TAPCALIB_BASEDATAMEMORY           = new DCAMIDPROP(0x00380F20);
        public static readonly DCAMIDPROP STORETAPCALIBDATATOMEMORY         = new DCAMIDPROP(0x00380F30);
        public static readonly DCAMIDPROP TAPCALIBDATAMEMORY                = new DCAMIDPROP(0x00380F40);
        public static readonly DCAMIDPROP NUMBEROF_TAPCALIB                 = new DCAMIDPROP(0x00380FF0);
        public static readonly DCAMIDPROP TAPCALIB_GAIN                     = new DCAMIDPROP(0x00381000);
        public static readonly DCAMIDPROP TAPCALIB_OFFSET                   = new DCAMIDPROP(0x00382000);
        public static readonly DCAMIDPROP _TAPCALIB                         = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP READOUTSPEED                      = new DCAMIDPROP(0x00400110);
        public static readonly DCAMIDPROP READOUT_DIRECTION                 = new DCAMIDPROP(0x00400130);
        public static readonly DCAMIDPROP READOUT_UNIT                      = new DCAMIDPROP(0x00400140);
        public static readonly DCAMIDPROP SHUTTER_MODE                      = new DCAMIDPROP(0x00400150);
        public static readonly DCAMIDPROP SENSORMODE                        = new DCAMIDPROP(0x00400210);
        public static readonly DCAMIDPROP SENSORMODE_LINEBUNDLEHEIGHT       = new DCAMIDPROP(0x00400250);
        public static readonly DCAMIDPROP SENSORMODE_PANORAMICSTARTV        = new DCAMIDPROP(0x00400280);
        public static readonly DCAMIDPROP SENSORMODE_TDISTAGE               = new DCAMIDPROP(0x00400290);
        public static readonly DCAMIDPROP CCDMODE                           = new DCAMIDPROP(0x00400310);
        public static readonly DCAMIDPROP EMCCD_CALIBRATIONMODE             = new DCAMIDPROP(0x00400320);
        public static readonly DCAMIDPROP CMOSMODE                          = new DCAMIDPROP(0x00400350);
        public static readonly DCAMIDPROP MULTILINESENSOR_READOUTMODE       = new DCAMIDPROP(0x00400380);
        public static readonly DCAMIDPROP MULTILINESENSOR_TOP               = new DCAMIDPROP(0x00400390);
        public static readonly DCAMIDPROP MULTILINESENSOR_HEIGHT            = new DCAMIDPROP(0x004003A0);
        public static readonly DCAMIDPROP OUTPUT_INTENSITY                  = new DCAMIDPROP(0x00400410);
        public static readonly DCAMIDPROP OUTPUTDATA_OPERATION              = new DCAMIDPROP(0x00400440);
        public static readonly DCAMIDPROP TESTPATTERN_KIND                  = new DCAMIDPROP(0x00400510);
        public static readonly DCAMIDPROP TESTPATTERN_OPTION                = new DCAMIDPROP(0x00400520);
        public static readonly DCAMIDPROP EXTRACTION_MODE                   = new DCAMIDPROP(0x00400620);
        public static readonly DCAMIDPROP BURIEDDATA_MODE                   = new DCAMIDPROP(0x00400A00);
        public static readonly DCAMIDPROP BINNING                           = new DCAMIDPROP(0x00401110);
        public static readonly DCAMIDPROP BINNING_INDEPENDENT               = new DCAMIDPROP(0x00401120);
        public static readonly DCAMIDPROP BINNING_HORZ                      = new DCAMIDPROP(0x00401130);
        public static readonly DCAMIDPROP BINNING_VERT                      = new DCAMIDPROP(0x00401140);
        public static readonly DCAMIDPROP SUBARRAYHPOS                      = new DCAMIDPROP(0x00402110);
        public static readonly DCAMIDPROP SUBARRAYHSIZE                     = new DCAMIDPROP(0x00402120);
        public static readonly DCAMIDPROP SUBARRAYVPOS                      = new DCAMIDPROP(0x00402130);
        public static readonly DCAMIDPROP SUBARRAYVSIZE                     = new DCAMIDPROP(0x00402140);
        public static readonly DCAMIDPROP SUBARRAYMODE                      = new DCAMIDPROP(0x00402150);
        public static readonly DCAMIDPROP DIGITALBINNING_METHOD             = new DCAMIDPROP(0x00402160);
        public static readonly DCAMIDPROP DIGITALBINNING_HORZ               = new DCAMIDPROP(0x00402170);
        public static readonly DCAMIDPROP DIGITALBINNING_VERT               = new DCAMIDPROP(0x00402180);
        public static readonly DCAMIDPROP TIMING_READOUTTIME                = new DCAMIDPROP(0x00403010);
        public static readonly DCAMIDPROP TIMING_CYCLICTRIGGERPERIOD        = new DCAMIDPROP(0x00403020);
        public static readonly DCAMIDPROP TIMING_MINTRIGGERBLANKING         = new DCAMIDPROP(0x00403030);
        public static readonly DCAMIDPROP TIMING_MINTRIGGERINTERVAL         = new DCAMIDPROP(0x00403050);
        public static readonly DCAMIDPROP TIMING_EXPOSURE                   = new DCAMIDPROP(0x00403060);
        public static readonly DCAMIDPROP TIMING_INVALIDEXPOSUREPERIOD      = new DCAMIDPROP(0x00403070);
        public static readonly DCAMIDPROP TIMING_FRAMESKIPNUMBER            = new DCAMIDPROP(0x00403080);
        public static readonly DCAMIDPROP TIMING_GLOBALEXPOSUREDELAY        = new DCAMIDPROP(0x00403090);
        public static readonly DCAMIDPROP INTERNALFRAMERATE                 = new DCAMIDPROP(0x00403810);
        public static readonly DCAMIDPROP INTERNAL_FRAMEINTERVAL            = new DCAMIDPROP(0x00403820);
        public static readonly DCAMIDPROP INTERNALLINERATE                  = new DCAMIDPROP(0x00403830);
        public static readonly DCAMIDPROP INTERNALLINESPEED                 = new DCAMIDPROP(0x00403840);
        public static readonly DCAMIDPROP INTERNAL_LINEINTERVAL             = new DCAMIDPROP(0x00403850);
        public static readonly DCAMIDPROP INTERNALLINERATE_CONTROL          = new DCAMIDPROP(0x00403870);
        public static readonly DCAMIDPROP TIMESTAMP_PRODUCER                = new DCAMIDPROP(0x00410A10);
        public static readonly DCAMIDPROP FRAMESTAMP_PRODUCER               = new DCAMIDPROP(0x00410A20);
        public static readonly DCAMIDPROP TRANSFERINFO_FRAMECOUNT           = new DCAMIDPROP(0x00410B10);
        public static readonly DCAMIDPROP TRANSFERINFO_LOSTCOUNT            = new DCAMIDPROP(0x00410B11);
        public static readonly DCAMIDPROP COLORTYPE                         = new DCAMIDPROP(0x00420120);
        public static readonly DCAMIDPROP BITSPERCHANNEL                    = new DCAMIDPROP(0x00420130);
        public static readonly DCAMIDPROP NUMBEROF_CHANNEL                  = new DCAMIDPROP(0x00420180);
        public static readonly DCAMIDPROP ACTIVE_CHANNELINDEX               = new DCAMIDPROP(0x00420190);
        public static readonly DCAMIDPROP NUMBEROF_VIEW                     = new DCAMIDPROP(0x004201C0);
        public static readonly DCAMIDPROP ACTIVE_VIEWINDEX                  = new DCAMIDPROP(0x004201D0);
        public static readonly DCAMIDPROP IMAGE_WIDTH                       = new DCAMIDPROP(0x00420210);
        public static readonly DCAMIDPROP IMAGE_HEIGHT                      = new DCAMIDPROP(0x00420220);
        public static readonly DCAMIDPROP IMAGE_ROWBYTES                    = new DCAMIDPROP(0x00420230);
        public static readonly DCAMIDPROP IMAGE_FRAMEBYTES                  = new DCAMIDPROP(0x00420240);
        public static readonly DCAMIDPROP IMAGE_TOPOFFSETBYTES              = new DCAMIDPROP(0x00420250);
        public static readonly DCAMIDPROP IMAGE_PIXELTYPE                   = new DCAMIDPROP(0x00420270);
        public static readonly DCAMIDPROP IMAGE_CAMERASTAMP                 = new DCAMIDPROP(0x00420300);
        public static readonly DCAMIDPROP RECORDFIXEDBYTES_PERFILE          = new DCAMIDPROP(0x00420410);
        public static readonly DCAMIDPROP RECORDFIXEDBYTES_PERSESSION       = new DCAMIDPROP(0x00420420);
        public static readonly DCAMIDPROP RECORDFIXEDBYTES_PERFRAME         = new DCAMIDPROP(0x00420430);
        public static readonly DCAMIDPROP IMAGEDETECTOR_PIXELWIDTH          = new DCAMIDPROP(0x00420810);
        public static readonly DCAMIDPROP IMAGEDETECTOR_PIXELHEIGHT         = new DCAMIDPROP(0x00420820);
        public static readonly DCAMIDPROP IMAGEDETECTOR_PIXELNUMHORZ        = new DCAMIDPROP(0x00420830);
        public static readonly DCAMIDPROP IMAGEDETECTOR_PIXELNUMVERT        = new DCAMIDPROP(0x00420840);
        public static readonly DCAMIDPROP FRAMEBUNDLE_MODE                  = new DCAMIDPROP(0x00421010);
        public static readonly DCAMIDPROP FRAMEBUNDLE_NUMBER                = new DCAMIDPROP(0x00421020);
        public static readonly DCAMIDPROP FRAMEBUNDLE_ROWBYTES              = new DCAMIDPROP(0x00421030);
        public static readonly DCAMIDPROP FRAMEBUNDLE_FRAMESTEPBYTES        = new DCAMIDPROP(0x00421040);
        public static readonly DCAMIDPROP NUMBEROF_PARTIALAREA              = new DCAMIDPROP(0x00430010);
        public static readonly DCAMIDPROP PARTIALAREA_HPOS                  = new DCAMIDPROP(0x00431000);
        public static readonly DCAMIDPROP PARTIALAREA_HSIZE                 = new DCAMIDPROP(0x00432000);
        public static readonly DCAMIDPROP PARTIALAREA_VPOS                  = new DCAMIDPROP(0x00433000);
        public static readonly DCAMIDPROP PARTIALAREA_VSIZE                 = new DCAMIDPROP(0x00434000);
        public static readonly DCAMIDPROP _PARTIALAREA                      = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP NUMBEROF_MULTILINE                = new DCAMIDPROP(0x0044F010);
        public static readonly DCAMIDPROP MULTILINE_VPOS                    = new DCAMIDPROP(0x00450000);
        public static readonly DCAMIDPROP MULTILINE_VSIZE                   = new DCAMIDPROP(0x00460000);
        public static readonly DCAMIDPROP _MULTILINE                        = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP DEFECTCORRECT_MODE                = new DCAMIDPROP(0x00470010);
        public static readonly DCAMIDPROP NUMBEROF_DEFECTCORRECT            = new DCAMIDPROP(0x00470020);
        public static readonly DCAMIDPROP HOTPIXELCORRECT_LEVEL             = new DCAMIDPROP(0x00470030);
        public static readonly DCAMIDPROP DEFECTCORRECT_HPOS                = new DCAMIDPROP(0x00471000);
        public static readonly DCAMIDPROP DEFECTCORRECT_METHOD              = new DCAMIDPROP(0x00473000);
        public static readonly DCAMIDPROP _DEFECTCORRECT                    = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP DEVICEBUFFER_MODE                 = new DCAMIDPROP(0x00490000);
        public static readonly DCAMIDPROP DEVICEBUFFER_FRAMECOUNTMAX        = new DCAMIDPROP(0x00490020);
        public static readonly DCAMIDPROP CALIBREGION_MODE                  = new DCAMIDPROP(0x00402410);
        public static readonly DCAMIDPROP NUMBEROF_CALIBREGION              = new DCAMIDPROP(0x00402420);
        public static readonly DCAMIDPROP CALIBREGION_HPOS                  = new DCAMIDPROP(0x004B0000);
        public static readonly DCAMIDPROP CALIBREGION_HSIZE                 = new DCAMIDPROP(0x004B1000);
        public static readonly DCAMIDPROP _CALIBREGION                      = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP MASKREGION_MODE                   = new DCAMIDPROP(0x00402510);
        public static readonly DCAMIDPROP NUMBEROF_MASKREGION               = new DCAMIDPROP(0x00402520);
        public static readonly DCAMIDPROP MASKREGION_HPOS                   = new DCAMIDPROP(0x004C0000);
        public static readonly DCAMIDPROP MASKREGION_HSIZE                  = new DCAMIDPROP(0x004C1000);
        public static readonly DCAMIDPROP _MASKREGION                       = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP CAMERASTATUS_INTENSITY            = new DCAMIDPROP(0x004D1110);
        public static readonly DCAMIDPROP CAMERASTATUS_INPUTTRIGGER         = new DCAMIDPROP(0x004D1120);
        public static readonly DCAMIDPROP CAMERASTATUS_CALIBRATION          = new DCAMIDPROP(0x004D1130);
        public static readonly DCAMIDPROP NUMBEROF_IMAGEBLOCK               = new DCAMIDPROP(0x004E0000);
        public static readonly DCAMIDPROP IMAGEBLOCK_FIRSTBYTESOFFSET       = new DCAMIDPROP(0x004E1000);
        public static readonly DCAMIDPROP IMAGEBLOCK_ACTUALXPOS             = new DCAMIDPROP(0x004E2000);
        public static readonly DCAMIDPROP IMAGEBLOCK_ACTUALYPOS             = new DCAMIDPROP(0x004E3000);
        public static readonly DCAMIDPROP IMAGEBLOCK_ACTUALXSIZE            = new DCAMIDPROP(0x004E4000);
        public static readonly DCAMIDPROP IMAGEBLOCK_ACTUALYSIZE            = new DCAMIDPROP(0x004E5000);
        public static readonly DCAMIDPROP IMAGEBLOCK_OVERLAPLEFTSIZE        = new DCAMIDPROP(0x004E6000);
        public static readonly DCAMIDPROP BACKFOCUSPOS_TARGET               = new DCAMIDPROP(0x00804010);
        public static readonly DCAMIDPROP BACKFOCUSPOS_CURRENT              = new DCAMIDPROP(0x00804020);
        public static readonly DCAMIDPROP BACKFOCUSPOS_LOADFROMMEMORY       = new DCAMIDPROP(0x00804050);
        public static readonly DCAMIDPROP BACKFOCUSPOS_STORETOMEMORY        = new DCAMIDPROP(0x00804060);
        public static readonly DCAMIDPROP CONFOCAL_SCANMODE                 = new DCAMIDPROP(0x00910010);
        public static readonly DCAMIDPROP CONFOCAL_SCANLINES                = new DCAMIDPROP(0x00910020);
        public static readonly DCAMIDPROP CONFOCAL_ZOOM                     = new DCAMIDPROP(0x00910030);
        public static readonly DCAMIDPROP SUBUNIT_IMAGEWIDTH                = new DCAMIDPROP(0x009100e0);
        public static readonly DCAMIDPROP NUMBEROF_SUBUNIT                  = new DCAMIDPROP(0x009100f0);
        public static readonly DCAMIDPROP SUBUNIT_CONTROL                   = new DCAMIDPROP(0x00910100);
        public static readonly DCAMIDPROP SUBUNIT_LASERPOWER                = new DCAMIDPROP(0x00910200);
        public static readonly DCAMIDPROP SUBUNIT_PMTGAIN                   = new DCAMIDPROP(0x00910300);
        public static readonly DCAMIDPROP SUBUNIT_PINHOLESIZE               = new DCAMIDPROP(0x00910400);
        public static readonly DCAMIDPROP SUBUNIT_WAVELENGTH                = new DCAMIDPROP(0x00910500);
        public static readonly DCAMIDPROP SUBUNIT_TOPOFFSETBYTES            = new DCAMIDPROP(0x00910600);
        public static readonly DCAMIDPROP _SUBUNIT                          = new DCAMIDPROP(0x00000010);
        public static readonly DCAMIDPROP SYSTEM_ALIVE                      = new DCAMIDPROP(0x00FF0010);
        public static readonly DCAMIDPROP PRIMARYBUFFER_TOTALBYTES          = new DCAMIDPROP(0x00FF1030);
        public static readonly DCAMIDPROP PRIMARYBUFFER_TOTALBYTES_MB       = new DCAMIDPROP(0x00FF1040);
        public static readonly DCAMIDPROP CONVERSIONFACTOR_COEFF            = new DCAMIDPROP(0x00FFE010);
        public static readonly DCAMIDPROP CONVERSIONFACTOR_OFFSET           = new DCAMIDPROP(0x00FFE020);


        public static readonly DCAMIDPROP _CHANNEL                          = new DCAMIDPROP(0x00000001);
        public static readonly DCAMIDPROP _VIEW                             = new DCAMIDPROP(0x01000000);

        public static readonly DCAMIDPROP ZERO = new DCAMIDPROP(0);

        private DCAMIDPROP(int argv)
        {
            idprop = (uint)argv;
        }

        public static bool operator ==(DCAMIDPROP a, DCAMIDPROP b)
        {
            return a.idprop == b.idprop;
        }
        public static bool operator !=(DCAMIDPROP a, DCAMIDPROP b)
        {
            return a.idprop != b.idprop;
        }
        public override int GetHashCode()
        {
            return idprop.GetHashCode();
        }
        public bool Equals(DCAMIDPROP err)
        {
            return this == err;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMIDPROP))
                return false;

            return Equals((DCAMIDPROP)obj);
        }

        public DCAMIDPROP(UInt32 argv)
        {
            idprop = (uint)argv;
        }

        public Int32 getidprop()
        {
            return (Int32)idprop;
        }

        public static implicit operator int(DCAMIDPROP self)
        {
            return self.getidprop();
        }
    }

    public struct DCAMPROP : IEquatable<DCAMPROP>
    {
        private double value;

        public struct SENSORMODE        {
            public static readonly DCAMPROP AREA                    = new DCAMPROP(1);          // "AREA"
            public static readonly DCAMPROP LINE                    = new DCAMPROP(3);          // "LINE"
            public static readonly DCAMPROP TDI                     = new DCAMPROP(4);          // "TDI"
            public static readonly DCAMPROP PARTIALAREA             = new DCAMPROP(6);          // "PARTIAL AREA"
            public static readonly DCAMPROP TDI_EXTENDED            = new DCAMPROP(10);         // "TDI EXTENDED"
            public static readonly DCAMPROP PANORAMIC               = new DCAMPROP(11);         // "PANORAMIC"
            public static readonly DCAMPROP PROGRESSIVE             = new DCAMPROP(12);         // "PROGRESSIVE"
            public static readonly DCAMPROP SPLITVIEW               = new DCAMPROP(14);         // "SPLIT VIEW"
            public static readonly DCAMPROP DUALLIGHTSHEET          = new DCAMPROP(16);         // "DUAL LIGHTSHEET"
            public static readonly DCAMPROP PHOTONNUMBERRESOLVING   = new DCAMPROP(18);         // "PHOTON NUMBER RESOLVING"
            public static readonly DCAMPROP WHOLELINES              = new DCAMPROP(19);         // "WHOLE LINES"
        };
        public struct SHUTTER_MODE      {
            public static readonly DCAMPROP GLOBAL                  = new DCAMPROP(1);          // "GLOBAL"
            public static readonly DCAMPROP ROLLING                 = new DCAMPROP(2);          // "ROLLING"
        };
        public struct READOUTSPEED      {
            public static readonly DCAMPROP SLOWEST                 = new DCAMPROP(1);          // no text
            public static readonly DCAMPROP FASTEST                 = new DCAMPROP(0x7FFFFFFF); // no text,w/o
        };
        public struct READOUT_DIRECTION     {
            public static readonly DCAMPROP FORWARD                 = new DCAMPROP(1);          // "FORWARD"
            public static readonly DCAMPROP BACKWARD                = new DCAMPROP(2);          // "BACKWARD"
            public static readonly DCAMPROP BYTRIGGER               = new DCAMPROP(3);          // "BY TRIGGER"
            public static readonly DCAMPROP DIVERGE                 = new DCAMPROP(5);          // "DIVERGE"
            public static readonly DCAMPROP FORWARDBIDIRECTION      = new DCAMPROP(6);          // "FORWARD BIDIRECTION"
            public static readonly DCAMPROP REVERSEBIDIRECTION      = new DCAMPROP(7);          // "REVERSE BIDIRECTION"
        };
        public struct READOUT_UNIT      {
            public static readonly DCAMPROP FRAME                   = new DCAMPROP(2);          // "FRAME"
            public static readonly DCAMPROP BUNDLEDLINE             = new DCAMPROP(3);          // "BUNDLED LINE"
            public static readonly DCAMPROP BUNDLEDFRAME            = new DCAMPROP(4);          // "BUNDLED FRAME"
        };
        public struct CCDMODE       {
            public static readonly DCAMPROP NORMALCCD               = new DCAMPROP(1);          // "NORMAL CCD"
            public static readonly DCAMPROP EMCCD                   = new DCAMPROP(2);          // "EM CCD"
        };
        public struct CMOSMODE      {
            public static readonly DCAMPROP NORMAL                  = new DCAMPROP(1);          // "NORMAL"
            public static readonly DCAMPROP NONDESTRUCTIVE          = new DCAMPROP(2);          // "NON DESTRUCTIVE"
        };
        public struct MULTILINESENSOR_READOUTMODE       {
            public static readonly DCAMPROP SYNCACCUMULATE          = new DCAMPROP(1);          // "SYNC ACCUMULATE"
            public static readonly DCAMPROP SYNCAVERAGE             = new DCAMPROP(2);          // "SYNC AVERAGE"
        };
        public struct OUTPUT_INTENSITY      {
            public static readonly DCAMPROP NORMAL                  = new DCAMPROP(1);          // "NORMAL"
            public static readonly DCAMPROP TESTPATTERN             = new DCAMPROP(2);          // "TEST PATTERN"
        };
        public struct OUTPUTDATA_OPERATION      {
            public static readonly DCAMPROP RAW                     = new DCAMPROP(1);          
            public static readonly DCAMPROP ALIGNED                 = new DCAMPROP(2);          
        };
        public struct TESTPATTERN_KIND      {
            public static readonly DCAMPROP FLAT                    = new DCAMPROP(2);          // "FLAT"
            public static readonly DCAMPROP IFLAT                   = new DCAMPROP(3);          // "INVERT FLAT"
            public static readonly DCAMPROP HORZGRADATION           = new DCAMPROP(4);          // "HORZGRADATION"
            public static readonly DCAMPROP IHORZGRADATION          = new DCAMPROP(5);          // "INVERT HORZGRADATION"
            public static readonly DCAMPROP VERTGRADATION           = new DCAMPROP(6);          // "VERTGRADATION"
            public static readonly DCAMPROP IVERTGRADATION          = new DCAMPROP(7);          // "INVERT VERTGRADATION"
            public static readonly DCAMPROP LINE                    = new DCAMPROP(8);          // "LINE"
            public static readonly DCAMPROP ILINE                   = new DCAMPROP(9);          // "INVERT LINE"
            public static readonly DCAMPROP DIAGONAL                = new DCAMPROP(10);         // "DIAGONAL"
            public static readonly DCAMPROP IDIAGONAL               = new DCAMPROP(11);         // "INVERT DIAGONAL"
            public static readonly DCAMPROP FRAMECOUNT              = new DCAMPROP(12);         // "FRAMECOUNT"
        };
        public struct DIGITALBINNING_METHOD     {
            public static readonly DCAMPROP MINIMUM                 = new DCAMPROP(1);          // "MINIMUM"
            public static readonly DCAMPROP MAXIMUM                 = new DCAMPROP(2);          // "MAXIMUM"
            public static readonly DCAMPROP ODD                     = new DCAMPROP(3);          // "ODD"
            public static readonly DCAMPROP EVEN                    = new DCAMPROP(4);          // "EVEN"
            public static readonly DCAMPROP SUM                     = new DCAMPROP(5);          // "SUM"
            public static readonly DCAMPROP AVERAGE                 = new DCAMPROP(6);          // "AVERAGE"
        };
        public struct TRIGGERSOURCE     {
            public static readonly DCAMPROP INTERNAL                = new DCAMPROP(1);          // "INTERNAL"
            public static readonly DCAMPROP EXTERNAL                = new DCAMPROP(2);          // "EXTERNAL"
            public static readonly DCAMPROP SOFTWARE                = new DCAMPROP(3);          // "SOFTWARE"
            public static readonly DCAMPROP MASTERPULSE             = new DCAMPROP(4);          // "MASTER PULSE"
        };
        public struct TRIGGERACTIVE     {
            public static readonly DCAMPROP EDGE                    = new DCAMPROP(1);          // "EDGE"
            public static readonly DCAMPROP LEVEL                   = new DCAMPROP(2);          // "LEVEL"
            public static readonly DCAMPROP SYNCREADOUT             = new DCAMPROP(3);          // "SYNCREADOUT"
            public static readonly DCAMPROP POINT                   = new DCAMPROP(4);          // "POINT"
        };
        public struct BUS_SPEED     {
            public static readonly DCAMPROP SLOWEST                 = new DCAMPROP(1);          // no text
            public static readonly DCAMPROP FASTEST                 = new DCAMPROP(0x7FFFFFFF); // no text,w/o
        };
        public struct TRIGGER_MODE      {
            public static readonly DCAMPROP NORMAL                  = new DCAMPROP(1);          // "NORMAL"
            public static readonly DCAMPROP PIV                     = new DCAMPROP(3);          // "PIV"
            public static readonly DCAMPROP START                   = new DCAMPROP(6);          // "START"
        };
        public struct TRIGGERPOLARITY       {
            public static readonly DCAMPROP NEGATIVE                = new DCAMPROP(1);          // "NEGATIVE"
            public static readonly DCAMPROP POSITIVE                = new DCAMPROP(2);          // "POSITIVE"
        };
        public struct TRIGGER_CONNECTOR     {
            public static readonly DCAMPROP INTERFACE               = new DCAMPROP(1);          // "INTERFACE"
            public static readonly DCAMPROP BNC                     = new DCAMPROP(2);          // "BNC"
            public static readonly DCAMPROP MULTI                   = new DCAMPROP(3);          // "MULTI"
        };
        public struct INTERNALTRIGGER_HANDLING      {
            public static readonly DCAMPROP SHORTEREXPOSURETIME     = new DCAMPROP(1);          // "SHORTER EXPOSURE TIME"
            public static readonly DCAMPROP FASTERFRAMERATE         = new DCAMPROP(2);          // "FASTER FRAME RATE"
            public static readonly DCAMPROP ABANDONWRONGFRAME       = new DCAMPROP(3);          // "ABANDON WRONG FRAME"
            public static readonly DCAMPROP BURSTMODE               = new DCAMPROP(4);          // "BURST MODE"
            public static readonly DCAMPROP INDIVIDUALEXPOSURE      = new DCAMPROP(7);          // "INDIVIDUAL EXPOSURE TIME"
        };
        public struct SYNCREADOUT_SYSTEMBLANK       {
            public static readonly DCAMPROP STANDARD                = new DCAMPROP(1);          // "STANDARD"
            public static readonly DCAMPROP MINIMUM                 = new DCAMPROP(2);          // "MINIMUM"
        };
        public struct TRIGGERENABLE_ACTIVE      {
            public static readonly DCAMPROP DENY                    = new DCAMPROP(1);          // "DENY"
            public static readonly DCAMPROP ALWAYS                  = new DCAMPROP(2);          // "ALWAYS"
            public static readonly DCAMPROP LEVEL                   = new DCAMPROP(3);          // "LEVEL"
            public static readonly DCAMPROP START                   = new DCAMPROP(4);          // "START"
            public static readonly DCAMPROP BURST                   = new DCAMPROP(6);          // "BURST"
        };
        public struct TRIGGERENABLE_SOURCE      {
            public static readonly DCAMPROP MULTI                   = new DCAMPROP(7);          // "MULTI"
            public static readonly DCAMPROP SMA                     = new DCAMPROP(8);          // "SMA"
        };
        public struct TRIGGERENABLE_POLARITY        {
            public static readonly DCAMPROP NEGATIVE                = new DCAMPROP(1);          // "NEGATIVE"
            public static readonly DCAMPROP POSITIVE                = new DCAMPROP(2);          // "POSITIVE"
            public static readonly DCAMPROP INTERLOCK               = new DCAMPROP(3);          // "INTERLOCK"
        };
        public struct OUTPUTTRIGGER_CHANNELSYNC     {
            public static readonly DCAMPROP _1CHANNEL               = new DCAMPROP(1);          // "1 Channel"
            public static readonly DCAMPROP _2CHANNELS              = new DCAMPROP(2);          // "2 Channels"
            public static readonly DCAMPROP _3CHANNELS              = new DCAMPROP(3);          // "3 Channels"
        };
        public struct OUTPUTTRIGGER_PROGRAMABLESTART        {
            public static readonly DCAMPROP FIRSTEXPOSURE           = new DCAMPROP(1);          // "FIRST EXPOSURE"
            public static readonly DCAMPROP FIRSTREADOUT            = new DCAMPROP(2);          // "FIRST READOUT"
        };
        public struct OUTPUTTRIGGER_SOURCE      {
            public static readonly DCAMPROP EXPOSURE                = new DCAMPROP(1);          // "EXPOSURE"
            public static readonly DCAMPROP READOUTEND              = new DCAMPROP(2);          // "READOUT END"
            public static readonly DCAMPROP VSYNC                   = new DCAMPROP(3);          // "VSYNC"
            public static readonly DCAMPROP HSYNC                   = new DCAMPROP(4);          // "HSYNC"
            public static readonly DCAMPROP TRIGGER                 = new DCAMPROP(6);          // "TRIGGER"
        };
        public struct OUTPUTTRIGGER_POLARITY        {
            public static readonly DCAMPROP NEGATIVE                = new DCAMPROP(1);          // "NEGATIVE"
            public static readonly DCAMPROP POSITIVE                = new DCAMPROP(2);          // "POSITIVE"
        };
        public struct OUTPUTTRIGGER_ACTIVE      {
            public static readonly DCAMPROP EDGE                    = new DCAMPROP(1);          // "EDGE"
            public static readonly DCAMPROP LEVEL                   = new DCAMPROP(2);          // "LEVEL"
        };
        public struct OUTPUTTRIGGER_KIND        {
            public static readonly DCAMPROP LOW                     = new DCAMPROP(1);          // "LOW"
            public static readonly DCAMPROP GLOBALEXPOSURE          = new DCAMPROP(2);          // "EXPOSURE"
            public static readonly DCAMPROP PROGRAMABLE             = new DCAMPROP(3);          // "PROGRAMABLE"
            public static readonly DCAMPROP TRIGGERREADY            = new DCAMPROP(4);          // "TRIGGER READY"
            public static readonly DCAMPROP HIGH                    = new DCAMPROP(5);          // "HIGH"
            public static readonly DCAMPROP ANYROWEXPOSURE          = new DCAMPROP(6);          // "ANYROW EXPOSURE"
        };
        public struct OUTPUTTRIGGER_BASESENSOR      {
            public static readonly DCAMPROP VIEW1                   = new DCAMPROP(1);          // "VIEW 1"
            public static readonly DCAMPROP VIEW2                   = new DCAMPROP(2);          // "VIEW 2"
            public static readonly DCAMPROP ANYVIEW                 = new DCAMPROP(15);         // "ANY VIEW"
            public static readonly DCAMPROP ALLVIEWS                = new DCAMPROP(16);         // "ALL VIEWS"
        };
        public struct EXPOSURETIME_CONTROL      {
            public static readonly DCAMPROP OFF                     = new DCAMPROP(1);          // "OFF"
            public static readonly DCAMPROP NORMAL                  = new DCAMPROP(2);          // "NORMAL"
        };
        public struct TRIGGER_FIRSTEXPOSURE     {
            public static readonly DCAMPROP NEW                     = new DCAMPROP(1);          // "NEW"
            public static readonly DCAMPROP CURRENT                 = new DCAMPROP(2);          // "CURRENT"
        };
        public struct TRIGGER_GLOBALEXPOSURE        {
            public static readonly DCAMPROP NONE                    = new DCAMPROP(1);          // "NONE"
            public static readonly DCAMPROP ALWAYS                  = new DCAMPROP(2);          // "ALWAYS"
            public static readonly DCAMPROP DELAYED                 = new DCAMPROP(3);          // "DELAYED"
            public static readonly DCAMPROP EMULATE                 = new DCAMPROP(4);          // "EMULATE"
            public static readonly DCAMPROP GLOBALRESET             = new DCAMPROP(5);          // "GLOBAL RESET"
        };
        public struct FIRSTTRIGGER_BEHAVIOR     {
            public static readonly DCAMPROP STARTEXPOSURE           = new DCAMPROP(1);          // "START EXPOSURE"
            public static readonly DCAMPROP STARTREADOUT            = new DCAMPROP(2);          // "START READOUT"
        };
        public struct MASTERPULSE_MODE      {
            public static readonly DCAMPROP CONTINUOUS              = new DCAMPROP(1);          // "CONTINUOUS"
            public static readonly DCAMPROP START                   = new DCAMPROP(2);          // "START"
            public static readonly DCAMPROP BURST                   = new DCAMPROP(3);          // "BURST"
        };
        public struct MASTERPULSE_TRIGGERSOURCE     {
            public static readonly DCAMPROP EXTERNAL                = new DCAMPROP(1);          // "EXTERNAL"
            public static readonly DCAMPROP SOFTWARE                = new DCAMPROP(2);          // "SOFTWARE"
        };
        public struct MECHANICALSHUTTER     {
            public static readonly DCAMPROP AUTO                    = new DCAMPROP(1);          // "AUTO"
            public static readonly DCAMPROP CLOSE                   = new DCAMPROP(2);          // "CLOSE"
            public static readonly DCAMPROP OPEN                    = new DCAMPROP(3);          // "OPEN"
        };
        public struct LIGHTMODE     {
            public static readonly DCAMPROP LOWLIGHT                = new DCAMPROP(1);          // "LOW LIGHT"
            public static readonly DCAMPROP HIGHLIGHT               = new DCAMPROP(2);          // "HIGH LIGHT"
        };
        public struct SENSITIVITYMODE       {
            public static readonly DCAMPROP OFF                     = new DCAMPROP(1);          // "OFF"
            public static readonly DCAMPROP ON                      = new DCAMPROP(2);          // "ON"
            public static readonly DCAMPROP INTERLOCK               = new DCAMPROP(3);          // "INTERLOCK"
        };
        public struct EMGAINWARNING_STATUS      {
            public static readonly DCAMPROP NORMAL                  = new DCAMPROP(1);          // "NORMAL"
            public static readonly DCAMPROP WARNING                 = new DCAMPROP(2);          // "WARNING"
            public static readonly DCAMPROP PROTECTED               = new DCAMPROP(3);          // "PROTECTED"
        };
        public struct PHOTONIMAGINGMODE     {
            public static readonly DCAMPROP _0                      = new DCAMPROP(0);          // "0"
            public static readonly DCAMPROP _1                      = new DCAMPROP(1);          // "1"
            public static readonly DCAMPROP _2                      = new DCAMPROP(2);          // "2"
            public static readonly DCAMPROP _3                      = new DCAMPROP(3);          // "2"
        };
        public struct SENSORCOOLER      {
            public static readonly DCAMPROP OFF                     = new DCAMPROP(1);          // "OFF"
            public static readonly DCAMPROP ON                      = new DCAMPROP(2);          // "ON"
            public static readonly DCAMPROP MAX                     = new DCAMPROP(4);          // "MAX"
        };
        public struct SENSORTEMPERATURE_STATUS      {
            public static readonly DCAMPROP NORMAL                  = new DCAMPROP(0);          // "NORMAL"
            public static readonly DCAMPROP WARNING                 = new DCAMPROP(1);          // "WARNING"
            public static readonly DCAMPROP PROTECTION              = new DCAMPROP(2);          // "PROTECTION"
        };
        public struct SENSORCOOLERSTATUS        {
            public static readonly DCAMPROP ERROR4                  = new DCAMPROP(-4);         // "ERROR4"
            public static readonly DCAMPROP ERROR3                  = new DCAMPROP(-3);         // "ERROR3"
            public static readonly DCAMPROP ERROR2                  = new DCAMPROP(-2);         // "ERROR2"
            public static readonly DCAMPROP ERROR1                  = new DCAMPROP(-1);         // "ERROR1"
            public static readonly DCAMPROP NONE                    = new DCAMPROP(0);          // "NONE"
            public static readonly DCAMPROP OFF                     = new DCAMPROP(1);          // "OFF"
            public static readonly DCAMPROP READY                   = new DCAMPROP(2);          // "READY"
            public static readonly DCAMPROP BUSY                    = new DCAMPROP(3);          // "BUSY"
            public static readonly DCAMPROP ALWAYS                  = new DCAMPROP(4);          // "ALWAYS"
            public static readonly DCAMPROP WARNING                 = new DCAMPROP(5);          // "WARNING"
        };
        public struct REALTIMEGAINCORRECT_LEVEL     {
            public static readonly DCAMPROP _1                      = new DCAMPROP(1);          // "1"
            public static readonly DCAMPROP _2                      = new DCAMPROP(2);          // "2"
            public static readonly DCAMPROP _3                      = new DCAMPROP(3);          // "3"
            public static readonly DCAMPROP _4                      = new DCAMPROP(4);          // "4"
            public static readonly DCAMPROP _5                      = new DCAMPROP(5);          // "5"
        };
        public struct WHITEBALANCEMODE      {
            public static readonly DCAMPROP FLAT                    = new DCAMPROP(1);          // "FLAT"
            public static readonly DCAMPROP AUTO                    = new DCAMPROP(2);          // "AUTO"
            public static readonly DCAMPROP TEMPERATURE             = new DCAMPROP(3);          // "TEMPERATURE"
            public static readonly DCAMPROP USERPRESET              = new DCAMPROP(4);          // "USER PRESET"
        };
        public struct DARKCALIB_TARGET      {
            public static readonly DCAMPROP ALL                     = new DCAMPROP(1);          // "ALL"
            public static readonly DCAMPROP ANALOG                  = new DCAMPROP(2);          // "ANALOG"
        };
        public struct SHADINGCALIB_METHOD       {
            public static readonly DCAMPROP AVERAGE                 = new DCAMPROP(1);          // "AVERAGE"
            public static readonly DCAMPROP MAXIMUM                 = new DCAMPROP(2);          // "MAXIMUM"
            public static readonly DCAMPROP USETARGET               = new DCAMPROP(3);          // "USE TARGET"
        };
        public struct CAPTUREMODE       {
            public static readonly DCAMPROP NORMAL                  = new DCAMPROP(1);          // "NORMAL"
            public static readonly DCAMPROP DARKCALIB               = new DCAMPROP(2);          // "DARK CALIBRATION"
            public static readonly DCAMPROP SHADINGCALIB            = new DCAMPROP(3);          // "SHADING CALIBRATION"
            public static readonly DCAMPROP TAPGAINCALIB            = new DCAMPROP(4);          // "TAP GAIN CALIBRATION"
            public static readonly DCAMPROP BACKFOCUSCALIB          = new DCAMPROP(5);          // "BACK FOCUS CALIBRATION"
        };
        public struct IMAGEFILTER       {
            public static readonly DCAMPROP THROUGH                 = new DCAMPROP(0);          // "THROUGH"
            public static readonly DCAMPROP PATTERN_1               = new DCAMPROP(1);          // "PATTERN 1"
        };
        public struct INTERFRAMEALU_ENABLE      {
            public static readonly DCAMPROP OFF                     = new DCAMPROP(1);          // "OFF"
            public static readonly DCAMPROP TRIGGERSOURCE_ALL       = new DCAMPROP(2);          // "TRIGGER SOURCE ALL"
            public static readonly DCAMPROP TRIGGERSOURCE_INTERNAL  = new DCAMPROP(3);          // "TRIGGER SOURCE INTERNAL ONLY"
        };
        public struct SHADINGCALIB_DATASTATUS       {
            public static readonly DCAMPROP NONE                    = new DCAMPROP(1);          // "NONE"
            public static readonly DCAMPROP FORWARD                 = new DCAMPROP(2);          // "FORWARD"
            public static readonly DCAMPROP BACKWARD                = new DCAMPROP(3);          // "BACKWARD"
            public static readonly DCAMPROP BOTH                    = new DCAMPROP(4);          // "BOTH"
        };
        public struct TAPGAINCALIB_METHOD       {
            public static readonly DCAMPROP AVE                     = new DCAMPROP(1);          // "AVERAGE"
            public static readonly DCAMPROP MAX                     = new DCAMPROP(2);          // "MAXIMUM"
            public static readonly DCAMPROP MIN                     = new DCAMPROP(3);          // "MINIMUM"
        };
        public struct RECURSIVEFILTERFRAMES     {
            public static readonly DCAMPROP _2                      = new DCAMPROP(2);          // "2 FRAMES"
            public static readonly DCAMPROP _4                      = new DCAMPROP(4);          // "4 FRAMES"
            public static readonly DCAMPROP _8                      = new DCAMPROP(8);          // "8 FRAMES"
            public static readonly DCAMPROP _16                     = new DCAMPROP(16);         // "16 FRAMES"
            public static readonly DCAMPROP _32                     = new DCAMPROP(32);         // "32 FRAMES"
            public static readonly DCAMPROP _64                     = new DCAMPROP(64);         // "64 FRAMES"
        };
        public struct INTENSITYLUT_MODE     {
            public static readonly DCAMPROP THROUGH                 = new DCAMPROP(1);          // "THROUGH"
            public static readonly DCAMPROP PAGE                    = new DCAMPROP(2);          // "PAGE"
            public static readonly DCAMPROP CLIP                    = new DCAMPROP(3);          // "CLIP"
        };
        public struct BINNING       {
            public static readonly DCAMPROP _1                      = new DCAMPROP(1);          // "1X1"
            public static readonly DCAMPROP _2                      = new DCAMPROP(2);          // "2X2"
            public static readonly DCAMPROP _4                      = new DCAMPROP(4);          // "4X4"
            public static readonly DCAMPROP _8                      = new DCAMPROP(8);          // "8X8"
            public static readonly DCAMPROP _16                     = new DCAMPROP(16);         // "16X16"
            public static readonly DCAMPROP _1_2                    = new DCAMPROP(102);        // "1X2"
            public static readonly DCAMPROP _2_4                    = new DCAMPROP(204);        // "2X4"
        };
        public struct COLORTYPE     {
            public static readonly DCAMPROP BW                      = new DCAMPROP(0x00000001); // "BW"
            public static readonly DCAMPROP RGB                     = new DCAMPROP(0x00000002); // "RGB"
            public static readonly DCAMPROP BGR                     = new DCAMPROP(0x00000003); // "BGR"
        };
        public struct BITSPERCHANNEL        {
            public static readonly DCAMPROP _8                      = new DCAMPROP(8);          // "8BIT"
            public static readonly DCAMPROP _10                     = new DCAMPROP(10);         // "10BIT"
            public static readonly DCAMPROP _12                     = new DCAMPROP(12);         // "12BIT"
            public static readonly DCAMPROP _14                     = new DCAMPROP(14);         // "14BIT"
            public static readonly DCAMPROP _16                     = new DCAMPROP(16);         // "16BIT"
        };
        public struct DEFECTCORRECT_MODE        {
            public static readonly DCAMPROP OFF                     = new DCAMPROP(1);          // "OFF"
            public static readonly DCAMPROP ON                      = new DCAMPROP(2);          // "ON"
        };
        public struct DEFECTCORRECT_METHOD      {
            public static readonly DCAMPROP CEILING                 = new DCAMPROP(3);          // "CEILING"
            public static readonly DCAMPROP PREVIOUS                = new DCAMPROP(4);          // "PREVIOUS"
            public static readonly DCAMPROP NEXT                    = new DCAMPROP(5);          // "NEXT"
        };
        public struct HOTPIXELCORRECT_LEVEL     {
            public static readonly DCAMPROP STANDARD                = new DCAMPROP(1);          // "STANDARD"
            public static readonly DCAMPROP MINIMUM                 = new DCAMPROP(2);          // "MINIMUM"
            public static readonly DCAMPROP AGGRESSIVE              = new DCAMPROP(3);          // "AGGRESSIVE"
        };
        public struct DEVICEBUFFER_MODE     {
            public static readonly DCAMPROP THRU                    = new DCAMPROP(1);          // "THRU"
            public static readonly DCAMPROP SNAPSHOT                = new DCAMPROP(2);          // "SNAPSHOT"
            public static readonly DCAMPROP SNAPSHOTEX              = new DCAMPROP(6);          // "SNAPSHOTEX"
        };
        public struct INTERNALLINERATE_CONTROL      {
            public static readonly DCAMPROP SYNC_EXPOSURETIME       = new DCAMPROP(1);          // "SYNC EXPOSURETIME"
            public static readonly DCAMPROP PRIORITIZE_LINERATE     = new DCAMPROP(2);          // "PRIORITIZE LINERATE"
            public static readonly DCAMPROP PRIORITIZE_EXPOSURETIME = new DCAMPROP(3);          // "PRIORITIZE EXPOSURETIME"
        };
        public struct SYSTEM_ALIVE      {
            public static readonly DCAMPROP OFFLINE                 = new DCAMPROP(1);          // "OFFLINE"
            public static readonly DCAMPROP ONLINE                  = new DCAMPROP(2);          // "ONLINE"
            public static readonly DCAMPROP ERROR                   = new DCAMPROP(3);          // "ERROR"
        };
        public struct TIMESTAMP_MODE        {
            public static readonly DCAMPROP NONE                    = new DCAMPROP(1);          // "NONE"
            public static readonly DCAMPROP LINEBEFORELEFT          = new DCAMPROP(2);          // "LINE BEFORE LEFT"
            public static readonly DCAMPROP LINEOVERWRITELEFT       = new DCAMPROP(3);          // "LINE OVERWRITE LEFT"
            public static readonly DCAMPROP AREABEFORELEFT          = new DCAMPROP(4);          // "AREA BEFORE LEFT"
            public static readonly DCAMPROP AREAOVERWRITELEFT       = new DCAMPROP(5);          // "AREA OVERWRITE LEFT"
        };
        public struct TIMING_EXPOSURE       {
            public static readonly DCAMPROP AFTERREADOUT            = new DCAMPROP(1);          // "AFTER READOUT"
            public static readonly DCAMPROP OVERLAPREADOUT          = new DCAMPROP(2);          // "OVERLAP READOUT"
            public static readonly DCAMPROP ROLLING                 = new DCAMPROP(3);          // "ROLLING"
            public static readonly DCAMPROP ALWAYS                  = new DCAMPROP(4);          // "ALWAYS"
            public static readonly DCAMPROP TDI                     = new DCAMPROP(5);          // "TDI"
        };
        public struct TIMESTAMP_PRODUCER        {
            public static readonly DCAMPROP NONE                    = new DCAMPROP(1);          // "NONE"
            public static readonly DCAMPROP DCAMMODULE              = new DCAMPROP(2);          // "DCAM MODULE"
            public static readonly DCAMPROP KERNELDRIVER            = new DCAMPROP(3);          // "KERNEL DRIVER"
            public static readonly DCAMPROP CAPTUREDEVICE           = new DCAMPROP(4);          // "CAPTURE DEVICE"
            public static readonly DCAMPROP IMAGINGDEVICE           = new DCAMPROP(5);          // "IMAGING DEVICE"
        };
        public struct FRAMESTAMP_PRODUCER       {
            public static readonly DCAMPROP NONE                    = new DCAMPROP(1);          // "NONE"
            public static readonly DCAMPROP DCAMMODULE              = new DCAMPROP(2);          // "DCAM MODULE"
            public static readonly DCAMPROP KERNELDRIVER            = new DCAMPROP(3);          // "KERNEL DRIVER"
            public static readonly DCAMPROP CAPTUREDEVICE           = new DCAMPROP(4);          // "CAPTURE DEVICE"
            public static readonly DCAMPROP IMAGINGDEVICE           = new DCAMPROP(5);          // "IMAGING DEVICE"
        };
        public struct CAMERASTATUS_INTENSITY        {
            public static readonly DCAMPROP GOOD                    = new DCAMPROP(1);          // "GOOD"
            public static readonly DCAMPROP TOODARK                 = new DCAMPROP(2);          // "TOO DRAK"
            public static readonly DCAMPROP TOOBRIGHT               = new DCAMPROP(3);          // "TOO BRIGHT"
            public static readonly DCAMPROP UNCARE                  = new DCAMPROP(4);          // "UNCARE"
            public static readonly DCAMPROP EMGAIN_PROTECTION       = new DCAMPROP(5);          // "EMGAIN PROTECTION"
            public static readonly DCAMPROP INCONSISTENT_OPTICS     = new DCAMPROP(6);          // "INCONSISTENT OPTICS"
            public static readonly DCAMPROP NODATA                  = new DCAMPROP(7);          // "NO DATA"
        };
        public struct CAMERASTATUS_INPUTTRIGGER     {
            public static readonly DCAMPROP GOOD                    = new DCAMPROP(1);          // "GOOD"
            public static readonly DCAMPROP NONE                    = new DCAMPROP(2);          // "NONE"
            public static readonly DCAMPROP TOOFREQUENT             = new DCAMPROP(3);          // "TOO FREQUENT"
        };
        public struct CAMERASTATUS_CALIBRATION      {
            public static readonly DCAMPROP DONE                    = new DCAMPROP(1);          // "DONE"
            public static readonly DCAMPROP NOTYET                  = new DCAMPROP(2);          // "NOT YET"
            public static readonly DCAMPROP NOTRIGGER               = new DCAMPROP(3);          // "NO TRIGGER"
            public static readonly DCAMPROP TOOFREQUENTTRIGGER      = new DCAMPROP(4);          // "TOO FREQUENT TRIGGER"
            public static readonly DCAMPROP OUTOFADJUSTABLERANGE    = new DCAMPROP(5);          
            public static readonly DCAMPROP UNSUITABLETABLE         = new DCAMPROP(6);          // "UNSUITABLE TABLE"
            public static readonly DCAMPROP TOODARK                 = new DCAMPROP(7);          // "TOO DARK"
            public static readonly DCAMPROP TOOBRIGHT               = new DCAMPROP(8);          // "TOO BRIGHT"
            public static readonly DCAMPROP NOTDETECTOBJECT         = new DCAMPROP(9);          // "NOT DETECT OBJECT"
        };
        public struct CONFOCAL_SCANMODE     {
            public static readonly DCAMPROP SIMULTANEOUS            = new DCAMPROP(1);          // "SIMULTANEOUS"
            public static readonly DCAMPROP SEQUENTIAL              = new DCAMPROP(2);          // "SEQUENTIAL"
        };
        public struct SUBUNIT_CONTROL       {
            public static readonly DCAMPROP NOTINSTALLED            = new DCAMPROP(0);          // "NOT INSTALLED"
            public static readonly DCAMPROP OFF                     = new DCAMPROP(1);          // "OFF"
            public static readonly DCAMPROP ON                      = new DCAMPROP(2);          // "ON"
        };
        public struct SUBUNIT_PINHOLESIZE       {
            public static readonly DCAMPROP ERROR                   = new DCAMPROP(1);          // "ERROR"
            public static readonly DCAMPROP SMALL                   = new DCAMPROP(2);          // "SMALL"
            public static readonly DCAMPROP MEDIUM                  = new DCAMPROP(3);          // "MEDIUM"
            public static readonly DCAMPROP LARGE                   = new DCAMPROP(4);          // "LARGE"
        };

        public struct MODE
        {
            public static readonly DCAMPROP OFF = new DCAMPROP(1);
            public static readonly DCAMPROP ON = new DCAMPROP(2);
        };

        public DCAMPROP( Int32 v )      {       value = (double)v;      }
        public DCAMPROP( UInt32 v )     {       value = (double)v;      }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public bool Equals(DCAMPROP a)
        {
            return value == a.value;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMPROP))
                return false;

            return Equals((DCAMPROP)obj);
        }

        public static implicit operator double(DCAMPROP self)
        {
            return self.value;
        }
    }

    public struct DCAMPROPUNIT : IEquatable<DCAMPROPUNIT>
    {
        private int propunit;

        public static readonly DCAMPROPUNIT SECOND          = new DCAMPROPUNIT(1);  // sec
        public static readonly DCAMPROPUNIT CELSIUS         = new DCAMPROPUNIT(2);  // for sensor temperature
        public static readonly DCAMPROPUNIT KELVIN          = new DCAMPROPUNIT(3);  // for color temperature
        public static readonly DCAMPROPUNIT METERPERSECOND  = new DCAMPROPUNIT(4);  // for LINESPEED
        public static readonly DCAMPROPUNIT PERSECOND       = new DCAMPROPUNIT(5);  // for FRAMERATE and LINERATE
        public static readonly DCAMPROPUNIT DEGREE          = new DCAMPROPUNIT(6);  // for OUTPUT ROTATION
        public static readonly DCAMPROPUNIT MICROMETER      = new DCAMPROPUNIT(7);  // for length
        public static readonly DCAMPROPUNIT NONE            = new DCAMPROPUNIT(0);  // no unit


        public DCAMPROPUNIT(int v)
        {
            propunit = v;
        }

        public override int GetHashCode()
        {
            return propunit.GetHashCode();
        }

        public bool Equals(DCAMPROPUNIT a)
        {
            return propunit == a.propunit;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMPROPUNIT))
                return false;

            return Equals((DCAMPROPUNIT)obj);
        }

        public static implicit operator uint(DCAMPROPUNIT self)
        {
            return (uint)self.propunit;
        }
        public static implicit operator int(DCAMPROPUNIT self)
        {
            return self.propunit;
        }
    }

    public struct DCAMPROPOPTION : IEquatable<DCAMPROPOPTION>
    {
        private uint propoption;

        public static readonly DCAMPROPOPTION PRIOR         = new DCAMPROPOPTION(0xFF000000);   // prior value
        public static readonly DCAMPROPOPTION NEXT          = new DCAMPROPOPTION(0x01000000);   // next value or id
        public static readonly DCAMPROPOPTION SUPPORT       = new DCAMPROPOPTION(0x00000000);   // default option
        public static readonly DCAMPROPOPTION UPDATED       = new DCAMPROPOPTION(0x00000001);   // UPDATED and VOLATILE can be used at same time
        public static readonly DCAMPROPOPTION VOLATILE      = new DCAMPROPOPTION(0x00000002);   // UPDATED and VOLATILE can be used at same time
        public static readonly DCAMPROPOPTION ARRAYELEMENT  = new DCAMPROPOPTION(0x00000004);   // ARRAYELEMENT
        public static readonly DCAMPROPOPTION NONE          = new DCAMPROPOPTION(0x00000000);   // no option


        public DCAMPROPOPTION(uint v)
        {
            propoption = v;
        }

        public override int GetHashCode()
        {
            return propoption.GetHashCode();
        }

        public bool Equals(DCAMPROPOPTION a)
        {
            return propoption == a.propoption;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMPROPOPTION))
                return false;

            return Equals((DCAMPROPOPTION)obj);
        }

        public static implicit operator uint(DCAMPROPOPTION self)
        {
            return (uint)self.propoption;
        }
        public static implicit operator int(DCAMPROPOPTION self)
        {
            return (int)self.propoption;
        }
    }

    public struct DCAMPROPATTRIBUTE : IEquatable<DCAMPROPATTRIBUTE>
    {
        private uint attribute;

        public static readonly DCAMPROPATTRIBUTE HASRANGE               = new DCAMPROPATTRIBUTE(0x80000000);
        public static readonly DCAMPROPATTRIBUTE HASSTEP                = new DCAMPROPATTRIBUTE(0x40000000);
        public static readonly DCAMPROPATTRIBUTE HASDEFAULT             = new DCAMPROPATTRIBUTE(0x20000000);
        public static readonly DCAMPROPATTRIBUTE HASVALUETEXT           = new DCAMPROPATTRIBUTE(0x10000000);
        public static readonly DCAMPROPATTRIBUTE HASCHANNEL             = new DCAMPROPATTRIBUTE(0x08000000);// value can set the value for each channels
        public static readonly DCAMPROPATTRIBUTE AUTOROUNDING           = new DCAMPROPATTRIBUTE(0x00800000);
        public static readonly DCAMPROPATTRIBUTE STEPPING_INCONSISTENT  = new DCAMPROPATTRIBUTE(0x00400000);
        public static readonly DCAMPROPATTRIBUTE DATASTREAM             = new DCAMPROPATTRIBUTE(0x00200000);// value is releated to image attribute
        public static readonly DCAMPROPATTRIBUTE HASRATIO               = new DCAMPROPATTRIBUTE(0x00100000);// value has ratio control capability
        public static readonly DCAMPROPATTRIBUTE VOLATILE               = new DCAMPROPATTRIBUTE(0x00080000);// value may be changed by user or automatically
        public static readonly DCAMPROPATTRIBUTE WRITABLE               = new DCAMPROPATTRIBUTE(0x00020000);// value can be set when state is manual
        public static readonly DCAMPROPATTRIBUTE READABLE               = new DCAMPROPATTRIBUTE(0x00010000);// value is readable when state is manual
        public static readonly DCAMPROPATTRIBUTE HASVIEW                = new DCAMPROPATTRIBUTE(0x00008000);// value can set the value for each views
        public static readonly DCAMPROPATTRIBUTE ACCESSREADY            = new DCAMPROPATTRIBUTE(0x00002000);// This value can get or set at READY status
        public static readonly DCAMPROPATTRIBUTE ACCESSBUSY             = new DCAMPROPATTRIBUTE(0x00001000);// This value can get or set at BUSY status
        public static readonly DCAMPROPATTRIBUTE ACTION                 = new DCAMPROPATTRIBUTE(0x00000400);// writing value takes related effect
        public static readonly DCAMPROPATTRIBUTE EFFECTIVE              = new DCAMPROPATTRIBUTE(0x00000200);// value is effective


            // property value type
        public static readonly DCAMPROPATTRIBUTE TYPE_NONE  = new DCAMPROPATTRIBUTE(0x00000000);// undefined
        public static readonly DCAMPROPATTRIBUTE TYPE_MODE  = new DCAMPROPATTRIBUTE(0x00000001);// 01:  mode, 32bit integer in case of 32bit OS
        public static readonly DCAMPROPATTRIBUTE TYPE_LONG  = new DCAMPROPATTRIBUTE(0x00000002);// 02:  32bit integer in case of 32bit OS
        public static readonly DCAMPROPATTRIBUTE TYPE_REAL  = new DCAMPROPATTRIBUTE(0x00000003);// 03:  64bit float
        public static readonly DCAMPROPATTRIBUTE TYPE_MASK  = new DCAMPROPATTRIBUTE(0x0000000F);// mask for property value type


        public DCAMPROPATTRIBUTE(uint v)
        {
            attribute = v;
        }
        public DCAMPROPATTRIBUTE(int v)
        {
            attribute = (uint)v;
        }

        public override int GetHashCode()
        {
            return attribute.GetHashCode();
        }

        public bool Equals(DCAMPROPATTRIBUTE a)
        {
            return attribute == a.attribute;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMPROPATTRIBUTE))
                return false;

            return Equals((DCAMPROPATTRIBUTE)obj);
        }
        public bool is_type(DCAMPROPATTRIBUTE type)
        {
            if ((attribute & TYPE_MASK) == (int)type)
                return true;

            return false;
        }
        public bool has_attr(DCAMPROPATTRIBUTE attr)
        {
            if ((attribute & (int)attr) == 0)
                return false;

            return true;
        }

        public static implicit operator uint(DCAMPROPATTRIBUTE self)
        {
            return self.attribute;
        }
        public static implicit operator int(DCAMPROPATTRIBUTE self)
        {
            return (int)self.attribute;
        }
        public static DCAMPROPATTRIBUTE operator |(DCAMPROPATTRIBUTE obj, uint value)
        {
            return new DCAMPROPATTRIBUTE(obj.attribute | value);
        }
        public static DCAMPROPATTRIBUTE operator &(DCAMPROPATTRIBUTE obj, uint value)
        {
            return new DCAMPROPATTRIBUTE(obj.attribute & value);
        }
        public static DCAMPROPATTRIBUTE operator ^(DCAMPROPATTRIBUTE obj, uint value)
        {
            return new DCAMPROPATTRIBUTE(obj.attribute ^ value);
        }
    }

    public struct DCAMPROPATTRIBUTE2 : IEquatable<DCAMPROPATTRIBUTE2>
    {
        private uint attribute;

        public static readonly DCAMPROPATTRIBUTE2 ARRAYBASE             = new DCAMPROPATTRIBUTE2(0x08000000);   
        public static readonly DCAMPROPATTRIBUTE2 ARRAYELEMENT          = new DCAMPROPATTRIBUTE2(0x04000000);   
        public static readonly DCAMPROPATTRIBUTE2 REAL32                = new DCAMPROPATTRIBUTE2(0x02000000);   
        public static readonly DCAMPROPATTRIBUTE2 INITIALIZEIMPROPER    = new DCAMPROPATTRIBUTE2(0x00000001);   
        public static readonly DCAMPROPATTRIBUTE2 CHANNELSEPARATEDDATA  = new DCAMPROPATTRIBUTE2(0x00040000);   // Channel 0 value is total of each channels.


        public DCAMPROPATTRIBUTE2(uint v)
        {
            attribute = v;
        }
        public DCAMPROPATTRIBUTE2(int v)
        {
            attribute = (uint)v;
        }

        public override int GetHashCode()
        {
            return attribute.GetHashCode();
        }

        public bool Equals(DCAMPROPATTRIBUTE2 a)
        {
            return attribute == a.attribute;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMPROPATTRIBUTE2))
                return false;

            return Equals((DCAMPROPATTRIBUTE2)obj);
        }
        public bool has_attr(DCAMPROPATTRIBUTE2 attr)
        {
            if ((attribute & (int)attr) == 0)
                return false;

            return true;
        }

        public static implicit operator uint(DCAMPROPATTRIBUTE2 self)
        {
            return self.attribute;
        }
        public static implicit operator int(DCAMPROPATTRIBUTE2 self)
        {
            return (int)self.attribute;
        }
        public static DCAMPROPATTRIBUTE2 operator |(DCAMPROPATTRIBUTE2 obj, uint value)
        {
            return new DCAMPROPATTRIBUTE2(obj.attribute | value);
        }
        public static DCAMPROPATTRIBUTE2 operator &(DCAMPROPATTRIBUTE2 obj, uint value)
        {
            return new DCAMPROPATTRIBUTE2(obj.attribute & value);
        }
        public static DCAMPROPATTRIBUTE2 operator ^(DCAMPROPATTRIBUTE2 obj, uint value)
        {
            return new DCAMPROPATTRIBUTE2(obj.attribute ^ value);
        }
    }


    public struct DCAMCAP_STATUS : IEquatable<DCAMCAP_STATUS>
    {
        private int status;

        public static readonly DCAMCAP_STATUS ERROR     = new DCAMCAP_STATUS(0x0000);   
        public static readonly DCAMCAP_STATUS BUSY      = new DCAMCAP_STATUS(0x0001);   
        public static readonly DCAMCAP_STATUS READY     = new DCAMCAP_STATUS(0x0002);   
        public static readonly DCAMCAP_STATUS STABLE    = new DCAMCAP_STATUS(0x0003);   
        public static readonly DCAMCAP_STATUS UNSTABLE  = new DCAMCAP_STATUS(0x0004);   


        public DCAMCAP_STATUS(int v)
        {
            status = v;
        }

        public override int GetHashCode()
        {
            return status.GetHashCode();
        }

        public bool Equals(DCAMCAP_STATUS a)
        {
            return status == a.status;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMCAP_STATUS))
                return false;

            return Equals((DCAMCAP_STATUS)obj);
        }

        public static implicit operator uint(DCAMCAP_STATUS self)
        {
            return (uint)self.status;
        }
        public static implicit operator int(DCAMCAP_STATUS self)
        {
            return self.status;
        }
    }

    public struct DCAMWAIT : IEquatable<DCAMWAIT>
    {
        private int eventbit;

        public static readonly DCAMWAIT NONE = new DCAMWAIT(0);
        public struct CAPEVENT
        {
            public static readonly DCAMWAIT TRANSFERRED = new DCAMWAIT(0x0001);
            public static readonly DCAMWAIT FRAMEREADY  = new DCAMWAIT(0x0002);
            public static readonly DCAMWAIT CYCLEEND    = new DCAMWAIT(0x0004);
            public static readonly DCAMWAIT EXPOSUREEND = new DCAMWAIT(0x0008);
            public static readonly DCAMWAIT STOPPED     = new DCAMWAIT(0x0010);
            public static readonly DCAMWAIT RELOADFRAME = new DCAMWAIT(0x0020);

        };

        public struct RECEVENT
        {
            public static readonly DCAMWAIT STOPPED     = new DCAMWAIT(0x0100);
            public static readonly DCAMWAIT WARNING     = new DCAMWAIT(0x0200);
            public static readonly DCAMWAIT MISSED      = new DCAMWAIT(0x0400);
            public static readonly DCAMWAIT DISKFULL    = new DCAMWAIT(0x1000);
            public static readonly DCAMWAIT WRITEFAULT  = new DCAMWAIT(0x2000);
            public static readonly DCAMWAIT SKIPPED     = new DCAMWAIT(0x4000);
            public static readonly DCAMWAIT WRITEFRAME  = new DCAMWAIT(0x8000);

        };

        public DCAMWAIT(int v)
        {
            eventbit = v;
        }

        public override int GetHashCode()
        {
            return eventbit.GetHashCode();
        }

        public bool Equals(DCAMWAIT a)
        {
            return eventbit == a.eventbit;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMWAIT))
                return false;

            return Equals((DCAMWAIT)obj);
        }

        public static implicit operator uint(DCAMWAIT self)
        {
            return (uint)self.eventbit;
        }
        public static implicit operator int(DCAMWAIT self)
        {
            return (int)self.eventbit;
        }
        public static implicit operator bool(DCAMWAIT self)
        {
            return self.eventbit != 0;
        }
        public static DCAMWAIT operator |(DCAMWAIT obj, int value)
        {
            return new DCAMWAIT(obj.eventbit | value);
        }
        public static DCAMWAIT operator &(DCAMWAIT obj, int value)
        {
            return new DCAMWAIT(obj.eventbit & value);
        }
        public static DCAMWAIT operator ^(DCAMWAIT obj, int value)
        {
            return new DCAMWAIT(obj.eventbit ^ value);
        }
    }

    public struct DCAMCAP_START : IEquatable<DCAMCAP_START>
    {
        private Int32 capmode;

        public static readonly DCAMCAP_START SEQUENCE   = new DCAMCAP_START(-1);
        public static readonly DCAMCAP_START SNAP       = new DCAMCAP_START(0);


        public DCAMCAP_START(Int32 v)
        {
            capmode = v;
        }

        public override int GetHashCode()
        {
            return capmode.GetHashCode();
        }

        public bool Equals(DCAMCAP_START a)
        {
            return capmode == a.capmode;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DCAMCAP_START))
                return false;

            return Equals((DCAMCAP_START)obj);
        }

        public static implicit operator uint(DCAMCAP_START self)
        {
            return (uint)self.capmode;
        }
        public static implicit operator int(DCAMCAP_START self)
        {
            return (int)self.capmode;
        }
    }

    public struct DCAMAPI_INITOPTION : IEquatable<DCAMAPI_INITOPTION>
    {
        private uint initoption;

        public static readonly DCAMAPI_INITOPTION APIVER__LATEST     = new DCAMAPI_INITOPTION(0x00000001);
        public static readonly DCAMAPI_INITOPTION APIVER__4_0        = new DCAMAPI_INITOPTION(0x00000400);
        public static readonly DCAMAPI_INITOPTION MULTIVIEW__DISABLE = new DCAMAPI_INITOPTION(0x00010002);
        public static readonly DCAMAPI_INITOPTION ENDMARK            = new DCAMAPI_INITOPTION(0x00000000);


        public DCAMAPI_INITOPTION(uint v)
        {
            initoption = v;
        }

        public override int GetHashCode()
        {
            return initoption.GetHashCode();
        }

        public bool Equals(DCAMAPI_INITOPTION a)
        {
            return initoption == a.initoption;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is DCAMAPI_INITOPTION))
                return false;
            
            return Equals((DCAMAPI_INITOPTION)obj);
        }

        public static implicit operator uint(DCAMAPI_INITOPTION self)
        {
            return (uint)self.initoption;
        }

        public static implicit operator int(DCAMAPI_INITOPTION self)
        {
            return (int)self.initoption;
        }
    }

    public struct DCAMBUF_METADATAKIND : IEquatable<DCAMBUF_METADATAKIND>
    {
        private uint metadatakind;

        public static readonly DCAMBUF_METADATAKIND TIMESTAMPS  = new DCAMBUF_METADATAKIND(0x00010000);
        public static readonly DCAMBUF_METADATAKIND FRAMESTAMPS = new DCAMBUF_METADATAKIND(0x00020000);


        public DCAMBUF_METADATAKIND(uint v)
        {
            metadatakind = v;
        }

        public override int GetHashCode()
        {
            return metadatakind.GetHashCode();
        }

        public bool Equals(DCAMBUF_METADATAKIND a)
        {
            return metadatakind == a.metadatakind;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DCAMBUF_METADATAKIND))
                return false;

            return Equals((DCAMBUF_METADATAKIND)obj);
        }

        public static implicit operator uint(DCAMBUF_METADATAKIND self)
        {
            return (uint)self.metadatakind;
        }
        public static implicit operator int(DCAMBUF_METADATAKIND self)
        {
            return (int)self.metadatakind;
        }
    }

    public struct DCAMREC_METADATAKIND : IEquatable<DCAMREC_METADATAKIND>
    {
        private uint metadatakind;

        public static readonly DCAMREC_METADATAKIND USERDATATEXT= new DCAMREC_METADATAKIND(0x00000001);
        public static readonly DCAMREC_METADATAKIND USERDATABIN = new DCAMREC_METADATAKIND(0x00000002);
        public static readonly DCAMREC_METADATAKIND TIMESTAMPS  = new DCAMREC_METADATAKIND(0x00010000);
        public static readonly DCAMREC_METADATAKIND FRAMESTAMPS = new DCAMREC_METADATAKIND(0x00020000);


        public DCAMREC_METADATAKIND(uint v)
        {
            metadatakind = v;
        }

        public override int GetHashCode()
        {
            return metadatakind.GetHashCode();
        }

        public bool Equals(DCAMREC_METADATAKIND a)
        {
            return metadatakind == a.metadatakind;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is DCAMREC_METADATAKIND a))
                return false;
            
            return Equals((DCAMREC_METADATAKIND)obj);
        }

        public static implicit operator uint(DCAMREC_METADATAKIND self)
        {
            return (uint)self.metadatakind;
        }

        public static implicit operator int(DCAMREC_METADATAKIND self)
        {
            return (int)self.metadatakind;
        }
    }


    public struct DCAMDATA_KIND : IEquatable<DCAMDATA_KIND>
    {
        private uint kind;

        public static readonly DCAMDATA_KIND LUT    = new DCAMDATA_KIND(0x00000002);
        public static readonly DCAMDATA_KIND NONE   = new DCAMDATA_KIND(0x00000000);


        public DCAMDATA_KIND(uint v)
        {
            kind = v;
        }

        public override int GetHashCode()
        {
            return kind.GetHashCode();
        }

        public bool Equals(DCAMDATA_KIND a)
        {
            return kind == a.kind;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is DCAMDATA_KIND))
                return false;
            
            return Equals((DCAMDATA_KIND)obj);
        }

        public static implicit operator uint(DCAMDATA_KIND self)
        {
            return (uint)self.kind;
        }

        public static implicit operator int(DCAMDATA_KIND self)
        {
            return (int)self.kind;
        }
    }


    public struct DCAMDATA_LUTTYPE : IEquatable<DCAMDATA_LUTTYPE>
    {
        private uint luttype;

        public static readonly DCAMDATA_LUTTYPE SEGMENTED_LINEAR= new DCAMDATA_LUTTYPE(0x00000001);
        public static readonly DCAMDATA_LUTTYPE MONO16          = new DCAMDATA_LUTTYPE(0x00000002);
        public static readonly DCAMDATA_LUTTYPE ACCESSREADY     = new DCAMDATA_LUTTYPE(0x01000000);
        public static readonly DCAMDATA_LUTTYPE ACCESSBUSY      = new DCAMDATA_LUTTYPE(0x02000000);
        public static readonly DCAMDATA_LUTTYPE BODYMASK        = new DCAMDATA_LUTTYPE(0x00FFFFFF);
        public static readonly DCAMDATA_LUTTYPE ATTRIBUTEMASK   = new DCAMDATA_LUTTYPE(0xFF000000);
        public static readonly DCAMDATA_LUTTYPE NONE            = new DCAMDATA_LUTTYPE(0x00000000);


        public DCAMDATA_LUTTYPE(uint v)
        {
            luttype = v;
        }

        public override int GetHashCode()
        {
            return luttype.GetHashCode();
        }

        public bool Equals(DCAMDATA_LUTTYPE a)
        {
            return luttype == a.luttype;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is DCAMDATA_LUTTYPE))
                return false;
            
            return Equals((DCAMDATA_LUTTYPE)obj);
        }

        public static implicit operator uint(DCAMDATA_LUTTYPE self)
        {
            return (uint)self.luttype;
        }

        public static implicit operator int(DCAMDATA_LUTTYPE self)
        {
            return (int)self.luttype;
        }
    }


    public struct DCAMDEV_CAPDOMAIN : IEquatable<DCAMDEV_CAPDOMAIN>
    {
        private uint capdomain;

        public static readonly DCAMDEV_CAPDOMAIN DCAMDATA   = new DCAMDEV_CAPDOMAIN(0x00000001);
        public static readonly DCAMDEV_CAPDOMAIN FUNCTION   = new DCAMDEV_CAPDOMAIN(0x00000000);


        public DCAMDEV_CAPDOMAIN(uint v)
        {
            capdomain = v;
        }

        public override int GetHashCode()
        {
            return capdomain.GetHashCode();
        }

        public bool Equals(DCAMDEV_CAPDOMAIN a)
        {
            return capdomain == a.capdomain;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is DCAMDEV_CAPDOMAIN))
                return false;
            
            return Equals((DCAMDEV_CAPDOMAIN)obj);
        }

        public static implicit operator uint(DCAMDEV_CAPDOMAIN self)
        {
            return (uint)self.capdomain;
        }

        public static implicit operator int(DCAMDEV_CAPDOMAIN self)
        {
            return (int)self.capdomain;
        }
    }


    // ================================ structures ================================

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAM_TIMESTAMP
    {
        public UInt32 sec;                      // [out]
        public Int32 microsec;                  // [out]
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAM_METADATAHDR
    {
        public Int32 size;                      // [in] size of whole structure, not only this.
        public Int32 iKind;                     // [in] DCAM_METADATAKIND
        public Int32 option;                    // [in] value meaning depends on DCAM_METADATAKIND
        public Int32 iFrame;                    // [in] frame index

        public DCAM_METADATAHDR(int metadatakind)
        {
            size = Marshal.SizeOf(typeof(DCAM_METADATAHDR));
            iKind = metadatakind;
            option = 0;
            iFrame = 0;
            iFrame = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAM_METADATABLOCKHDR
    {
        public Int32 size;                      // [in] size of whole structure, not only this.
        public Int32 iKind;                     // [in] DCAM_METADATAKIND
        public Int32 option;                    // [in] value meaning depends on DCAMBUF_METADATAOPTION or DCAMREC_METADATAOPTION
        public Int32 iFrame;                    // [in] start frame index
        public Int32 in_count;                  // [in] max count of meta data
        public Int32 outcount;                  // [out] count of got meta data.

        public DCAM_METADATABLOCKHDR(int metadatakind)
        {
            size = Marshal.SizeOf(typeof(DCAM_METADATABLOCKHDR));
            iKind = metadatakind;
            option = 0;
            iFrame = 0;
            in_count = 0;
            outcount = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack =8)]
    public struct DCAM_TIMESTAMPBLOCK
    {
        public DCAM_METADATABLOCKHDR hdr;   // [in] size member should be size of this structure
                                            // [in] iKind should be DCAMBUF_METADATAKIND_TIMESTAMPS.
                                            // [in] option should be one of DCAMBUF_METADATAOPTION or DCAMREC_METADATAOPTION

        public IntPtr timestamps;            // [in] pointer for TIMESTAMP block. DCAM_TIMESTAMP[]

        public Int32 timestampsize;             // [in] sizeof(DCAM_TIMESTRAMP)
        public Int32 timestampvaildsize;        // [o] return the written data size of DCAM_TIMESTRAMP.
        public Int32 timestampkind;             // [o] return timestamp kind(Hardware, Driver, DCAM etc..)
        public Int32 reserved;

        public DCAM_TIMESTAMPBLOCK(int option)
        {
            hdr.size = Marshal.SizeOf(typeof(DCAM_TIMESTAMPBLOCK));
            hdr.iKind = DCAMBUF_METADATAKIND.TIMESTAMPS;
            hdr.option = option;
            hdr.iFrame = 0;
            hdr.in_count = 0;
            hdr.outcount = 0;

            timestamps = IntPtr.Zero;

            timestampsize = 0;
            timestampvaildsize = 0;
            timestampkind = 0;
            reserved = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DCAM_FRAMESTAMPBLOCK
    {
        public DCAM_METADATABLOCKHDR hdr;   // [in] size member should be size of this structure
                                            // [in] iKind should be DCAMBUF_METADATAKIND_FRAMESTAMPS.
                                            // [in] option should be one of DCAMBUF_METADATAOPTION or DCAMREC_METADATAOPTION

        public IntPtr framestamps;            // [in] pointer for framestamp block. Int32[]

        public Int32 reserved;

        public DCAM_FRAMESTAMPBLOCK(int option)
        {
            hdr.size = Marshal.SizeOf(typeof(DCAM_FRAMESTAMPBLOCK));
            hdr.iKind = DCAMBUF_METADATAKIND.FRAMESTAMPS;
            hdr.option = option;
            hdr.iFrame = 0;
            hdr.in_count = 0;
            hdr.outcount = 0;

            framestamps = IntPtr.Zero;
            reserved = 0;
        }
    }


    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMAPI_INIT
    {
        public Int32 size;                      // [in]
        public Int32 iDeviceCount;              // [out]
        public Int32 reserved;                  // reserved
        public Int32 initoptionbytes;           // [in] maximum bytes of initoption array.
        public IntPtr initoption;               // [in ptr] initialize options. Choose from DCAMAPI_INITOPTION
        public IntPtr guid;                     // [in ptr]

        public DCAMAPI_INIT(int dummy)
        {
            size = Marshal.SizeOf(typeof(DCAMAPI_INIT));
            iDeviceCount = 0;
            reserved = 0;
            initoption = IntPtr.Zero;
            initoptionbytes = 0;
            guid = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMDEV_OPEN
    {
        public Int32 size;
        public Int32 index;
        public IntPtr hdcam;

        public DCAMDEV_OPEN(int iCamera)
        {
            size = Marshal.SizeOf(typeof(DCAMDEV_OPEN));
            index = iCamera;
            hdcam = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMDEV_CAPABILITY
    {
        public Int32 size;                      // [in]
        public Int32 domain;                    // [in] DCAMDEV_CAPDOMAIN__*
        public Int32 capflag;                   // [out] DCAMDEV_CAPFLAG
        public Int32 kind;                      // [out] data kind of domain
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMDEV_CAPABILITY_LUT
    {
        public DCAMDEV_CAPABILITY hdr;      // [in] size:       size of this structure
                                            // [in] domain:     DCAMDEV_CAPDOMAIN__DCAMDATA
                                            // [out]capflag:    DCAMDATA_LUTTYPE__*
                                            // [in] kind:       DCAMDATA_KIND__LUT
        
        public Int32 linearpointmax;        // [out] max of linear lut point
    }


    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct DCAMDEV_STRING
    {
        public Int32 size;                      // [in]
        public Int32 iString;                   // [in]
        public IntPtr text;                     // [in,obuf]
        public Int32 textbytes;                 // [in] int32_t

        public DCAMDEV_STRING(Int32 istr)
        {
            size = Marshal.SizeOf(typeof(DCAMDEV_STRING));
            iString = istr;
            text = IntPtr.Zero;
            textbytes = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMDATA_HDR
    {
        public Int32 size;                      // [in] size of whole structure, not only this
        public Int32 iKind;                     // [in] DCAMDATA_KIND__*
        public Int32 option;                    // [in] DCAMDATA_OPTION__*
        public Int32 reserved2;                 // [in] 0 reserved
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMDATA_LUT
    {
        public DCAMDATA_HDR hdr;                // [in] size:   size of this structure
                                                // [in] iKind:  DCAMDATA_KIND__LUT
        
        public Int32 type;                      // [in] DCAMDATA_LUTTYPE
        public Int32 page;                      // [in] use to load or store
        public IntPtr data;                     // WORD array or DCAMDATA_LINEARLUT array
        public Int32 datasize;                  // size of data
        public Int32 reserved;                  // 0 reserved
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMDATA_LINEARLUT
    {
        public Int32 lutin;
        public Int32 lutout;
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMBUF_ATTACH
    {
        public Int32 size;                      // [in] size of this structure.
        public Int32 iKind;                     // [in] DCAMBUF_ATTACHKIND
        public IntPtr buffer;                   // [in,ptr]
        public Int32 buffercount;               // [in]
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMBUF_FRAME
    {
        public Int32 size;                      // [i] size of this structure.
        public Int32 iKind;                     // [i] reserved. set to 0.
        public Int32 option;                    // [i] reserved. set to 0.
        public Int32 iFrame;                    // [i] reserved. set to 0.
        public IntPtr buf;                      // [o] pointer for top-left image
        public Int32 rowbytes;                  // [o] byte size for next line.
        public DCAM_PIXELTYPE type;             // [o] return pixeltype of image. set to 0 before call.
        public Int32 width;                     // [o] horizontal pixel count
        public Int32 height;                    // [o] vertical line count
        public Int32 left;                      // [o] horizontal start pixel
        public Int32 top;                       // [o] vertical start line
        public DCAM_TIMESTAMP timestamp;        // [o] timestamp
        public Int32 framestamp;                // [o] framestamp
        public Int32 camerastamp;               // [o] camerastamp

        public DCAMBUF_FRAME(int indexFrame)
        {
            size = Marshal.SizeOf(typeof(DCAMBUF_FRAME));
            iKind = 0;
            option = 0;
            iFrame = indexFrame;
            buf = IntPtr.Zero;
            rowbytes = 0;
            type = DCAM_PIXELTYPE.NONE;
            width = 0;
            height = 0;
            left = 0;
            top = 0;
            timestamp.sec = 0;
            timestamp.microsec = 0;
            framestamp = 0;
            camerastamp = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMREC_FRAME
    {
        public Int32 size;                      // [i] size of this structure.
        public Int32 iKind;                     // [i] reserved. set to 0.
        public Int32 option;                    // [i] reserved. set to 0.
        public Int32 iFrame;                    // [i] reserved. set to 0.
        public IntPtr buf;                      // [o] pointer for top-left image
        public Int32 rowbytes;                  // [o] byte size for next line.
        public DCAM_PIXELTYPE type;             // [o] return pixeltype of image. set to 0 before call.
        public Int32 width;                     // [o] horizontal pixel count
        public Int32 height;                    // [o] vertical line count
        public Int32 left;                      // [o] horizontal start pixel
        public Int32 top;                       // [o] vertical start line
        public DCAM_TIMESTAMP timestamp;        // [o] timestamp
        public Int32 framestamp;                // [o] framestamp
        public Int32 camerastamp;               // [o] camerastamp

        public DCAMREC_FRAME(int indexFrame)
        {
            size = Marshal.SizeOf(typeof(DCAMREC_FRAME));
            iKind = 0;
            option = 0;
            iFrame = indexFrame;
            buf = IntPtr.Zero;
            rowbytes = 0;
            type = DCAM_PIXELTYPE.NONE;
            width = 0;
            height = 0;
            left = 0;
            top = 0;
            timestamp.sec = 0;
            timestamp.microsec = 0;
            framestamp = 0;
            camerastamp = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMCAP_TRANSFERINFO
    {
        public Int32 size;                      // [in] size of this structure.
        public Int32 reserved;                  // [in]
        public Int32 nNewestFrameIndex;         // [out]
        public Int32 nFrameCount;               // [out]

        public DCAMCAP_TRANSFERINFO(int dummy)
        {
            size = Marshal.SizeOf(typeof(DCAMCAP_TRANSFERINFO));
            reserved = 0;
            nNewestFrameIndex = 0;
            nFrameCount = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMWAIT_OPEN
    {
        public Int32 size;                      // [in] size of this structure.
        public Int32 supportevent;              // [out];
        public IntPtr hwait;                    // [out];
        public IntPtr hdcam;                    // [in];

        public DCAMWAIT_OPEN(int dummy)
        {
            size = Marshal.SizeOf(typeof(DCAMWAIT_OPEN));
            supportevent = 0;
            hwait = IntPtr.Zero;
            hdcam = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMWAIT_START
    {
        public Int32 size;                      // [in] size of this structure.
        public Int32 eventhappened;             // [out]
        public Int32 eventmask;                 // [in]
        public Int32 timeout;                   // [in]

        public DCAMWAIT_START(int _mask)
        {
            size = Marshal.SizeOf(typeof(DCAMWAIT_START));
            eventhappened = 0;
            eventmask = _mask;
            unchecked
            {
                timeout = (int)0x80000000;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMPROP_ATTR
    {
        // input parameters
        public Int32 cbSize;                    // size of this structure
        public Int32 iProp;                     // DCAMIDPROPERTY
        public Int32 option;                    // DCAMPROPOTION
        public Int32 iReserved1;                // must be 0

        // output parameters
        public Int32 attribute;                 // DCAMPROPATTRIBUTE
        public Int32 iGroup;                    // 0 reserved; DCAMIDGROUP
        public Int32 iUnit;                     // DCAMPROPUNIT
        public Int32 attribute2;                // DCAMPROPATTRIBUTE2

        public double valuemin;                 // minimum value
        public double valuemax;                 // maximum value
        public double valuestep;                // minimum stepping between a value and the next
        public double valuedefault;             // default value

        // available from DCAM-API 3.0
        public Int32 nMaxChannel;               // max channel if supports
        public Int32 iReserved3;                // reserved to 0
        public Int32 nMaxView;                  // max view if supports

        // available from DCAM-API 3.1
        public Int32 iProp_NumberOfElement;     // number of elements for array
        public Int32 iProp_ArrayBase;           // base id of array if element
        public Int32 iPropStep_Element;         // step for iProp to next element

        public DCAMPROP_ATTR(Int32 _iprop)
        {
            cbSize = Marshal.SizeOf(typeof(DCAMPROP_ATTR));
            iProp = _iprop;
            option = 0;
            iReserved1 = 0;
            attribute = 0;
            iGroup = 0;
            iUnit = 0;
            attribute2 = 0;
            valuemin = 0;
            valuemax = 0;
            valuestep = 0;
            valuedefault = 0;
            nMaxChannel = 0;
            iReserved3 = 0;
            nMaxView = 0;
            iProp_NumberOfElement = 0;
            iProp_ArrayBase = 0;
            iPropStep_Element = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    public struct DCAMPROP_VALUETEXT
    {
        public Int32 cbSize;                    // [in] size of this structure
        public Int32 iProp;                     // [in] DCAMIDPROP
        public double value;                    // [in] value of property
        public IntPtr text;                     // [in, obuf] text of the value 
        public Int32 textbytes;                 // [in] text buf size

        public DCAMPROP_VALUETEXT(Int32 _iprop)
        {
            cbSize = Marshal.SizeOf(typeof(DCAMPROP_VALUETEXT));
            iProp = _iprop;
            value = 0;
            text = IntPtr.Zero;
            textbytes = 0;
        }
        public DCAMPROP_VALUETEXT(Int32 _iprop, double _value)
        {
            cbSize = Marshal.SizeOf(typeof(DCAMPROP_VALUETEXT));
            iProp = _iprop;
            value = _value;
            text = IntPtr.Zero;
            textbytes = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential,Pack=8,CharSet=CharSet.Unicode)]
    public struct DCAMREC_OPEN
    {
        public Int32 size;                      // [in] size of this structure.
        public Int32 reserved;                  // [in]
        public IntPtr hrec;                     // [out]
        [MarshalAs(UnmanagedType.LPWStr)]
        public string path;                     // [in]
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ext;                      // [in]
        public Int32 maxframepersession;        // [in]
        public Int32 userdatasize;              // [in]
        public Int32 userdatasize_session;      // [in]
        public Int32 userdatasize_file;         // [in]
        public Int32 usertextsize;              // [in]
        public Int32 usertextsize_session;      // [in]
        public Int32 usertextsize_file;         // [in]
    }
    
    [StructLayout(LayoutKind.Sequential,Pack=8,CharSet=CharSet.Ansi)]
    public struct DCAMREC_OPENA
    {
        public Int32 size;                      // [in] size of this structure.
        public Int32 reserved;                  // [in]
        public IntPtr hrec;                     // [out]
        [MarshalAs(UnmanagedType.LPStr)]
        public string path;                     // [in]
        [MarshalAs(UnmanagedType.LPStr)]
        public string ext;                      // [in]
        public Int32 maxframepersession;        // [in]
        public Int32 userdatasize;              // [in]
        public Int32 userdatasize_session;      // [in]
        public Int32 userdatasize_file;         // [in]
        public Int32 usertextsize;              // [in]
        public Int32 usertextsize_session;      // [in]
        public Int32 usertextsize_file;         // [in]
    }

    [StructLayout(LayoutKind.Sequential,Pack=8)]
    public struct DCAMREC_STATUS
    {
        public Int32 size;
        public Int32 currentsession_index;
        public Int32 maxframecount_per_session;
        public Int32 currentframe_index;
        public Int32 missingframe_count;
        public Int32 flags;                     // DCAMREC_STATUSFLAG
        public Int32 totalframecount;
        public Int32 reserved;
    }

    // ================================ common function ================================

    // Boolean failed( DCAMERR err ) is defined as a member funciton of DCAMERR

    // ================ declaration class for DCAM-API ================
    /// <summary>
    /// static class for calling DCAM-API library
    /// </summary>
    static class dcamapidll
    {
        //initialization
        [DllImport("dcamapi")]
        public static extern DCAMERR dcamapi_init(ref DCAMAPI_INIT param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamapi_uninit();

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamdev_open(ref DCAMDEV_OPEN param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamdev_close(IntPtr h);

        [DllImport("dcamapi", CharSet = CharSet.Ansi)]
        public static extern DCAMERR dcamdev_getstring(IntPtr h, ref DCAMDEV_STRING param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamdev_showpanel(IntPtr h, Int32 iKind);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamdev_getcapability(IntPtr h, ref DCAMDEV_CAPABILITY param);

        [DllImport("dcamapi", EntryPoint = "dcamdev_getcapability")]
        public static extern DCAMERR dcamdev_getcapability_lut(IntPtr h, ref DCAMDEV_CAPABILITY_LUT param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamdev_setdata(IntPtr h, ref DCAMDATA_HDR hdr);

        [DllImport("dcamapi", EntryPoint = "dcamdev_setdata")]
        public static extern DCAMERR dcamdev_setdata_lut(IntPtr h, ref DCAMDATA_LUT lut);

        //buffer control
        [DllImport("dcamapi")]
        public static extern DCAMERR dcambuf_alloc(IntPtr h, Int32 framecount);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcambuf_release(IntPtr h, Int32 iKind);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcambuf_lockframe(IntPtr h, ref DCAMBUF_FRAME pFrame);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcambuf_attach(IntPtr h, ref DCAMBUF_ATTACH param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcambuf_copyframe(IntPtr h, ref DCAMBUF_FRAME pFrame);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcambuf_copymetadata(IntPtr h, ref  DCAM_METADATAHDR hdr);

        [DllImport("dcamapi", EntryPoint = "dcambuf_copymetadata")]
        public static extern DCAMERR dcambuf_copymetadata_timestampblock(IntPtr h, ref DCAM_TIMESTAMPBLOCK tsb);

        [DllImport("dcamapi", EntryPoint = "dcambuf_copymetadata")]
        public static extern DCAMERR dcambuf_copymetadata_framestampblock(IntPtr h, ref DCAM_FRAMESTAMPBLOCK fsb);

        // Capturing
        [DllImport("dcamapi")]
        public static extern DCAMERR dcamcap_start(IntPtr h, Int32 mode);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamcap_stop(IntPtr h);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamcap_status(IntPtr h, ref DCAMCAP_STATUS pStatus);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamcap_transferinfo(IntPtr h, ref DCAMCAP_TRANSFERINFO param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamcap_firetrigger(IntPtr h, Int32 iKind);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamcap_record(IntPtr h, IntPtr hdcamrec);

        //wait abort handle control
        [DllImport("dcamapi")]
        public static extern DCAMERR dcamwait_open(ref DCAMWAIT_OPEN param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamwait_close(IntPtr hWait);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamwait_start(IntPtr hWait, ref DCAMWAIT_START param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamwait_abort(IntPtr hWait);

        // Recording
        [DllImport("dcamapi", CharSet = CharSet.Ansi)]
        public static extern DCAMERR dcamrec_openA(ref DCAMREC_OPENA param);
        [DllImport("dcamapi", CharSet = CharSet.Unicode)]
        public static extern DCAMERR dcamrec_openW(ref DCAMREC_OPEN param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_status(IntPtr hdcamrec, ref DCAMREC_STATUS param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_close(IntPtr hdcamrec);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_lockframe(IntPtr hdcamrec, ref DCAMREC_FRAME pFrame);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_copyframe(IntPtr hdcamrec, ref DCAMREC_FRAME pFrame);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_writemetadata(IntPtr hdcamrec, ref DCAM_METADATAHDR hdr);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_lockmetadata(IntPtr hdcamrec, ref DCAM_METADATAHDR hdr);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_copymetadata(IntPtr hdcamrec, ref DCAM_METADATAHDR hdr);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_lockmetadatablock(IntPtr hdcamrec, ref DCAM_METADATABLOCKHDR hdr);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_copymetadatablock(IntPtr hdcamrec, ref DCAM_METADATABLOCKHDR hdr);

        [DllImport("dcamapi", EntryPoint = "dcamrec_copymetadatablock")]
        public static extern DCAMERR dcamrec_copymetadata_timestampblock(IntPtr h, ref DCAM_TIMESTAMPBLOCK tsb);

        [DllImport("dcamapi", EntryPoint = "dcamrec_copymetadatablock")]
        public static extern DCAMERR dcamrec_copymetadata_framestampblock(IntPtr h, ref DCAM_FRAMESTAMPBLOCK fsb);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_pause(IntPtr hdcamrec);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamrec_resume(IntPtr hdcamrec);

        // Property control
        [DllImport("dcamapi")]
        public static extern DCAMERR dcamprop_getattr(IntPtr h, ref DCAMPROP_ATTR param);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamprop_getvalue(IntPtr h, Int32 iProp, ref double pValue);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamprop_setvalue(IntPtr h, Int32 iProp, double fValue);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamprop_setgetvalue(IntPtr h, Int32 iProp, ref double pValue, Int32 option);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamprop_queryvalue(IntPtr h, Int32 iProp, ref double pValue, Int32 option);

        [DllImport("dcamapi")]
        public static extern DCAMERR dcamprop_getnextid(IntPtr h, ref DCAMIDPROP iProp, Int32 option);

        [DllImport("dcamapi", CharSet = CharSet.Ansi)]
        public static extern DCAMERR dcamprop_getname(IntPtr h, Int32 iProp, StringBuilder text, Int32 textbytes);

        [DllImport("dcamapi", CharSet = CharSet.Ansi)]
        public static extern DCAMERR dcamprop_getvaluetext(IntPtr h, ref DCAMPROP_VALUETEXT param);
    }

    // ================ declaration class dcamapi ================
    /// <summary>
    /// static class for managing DCAM-API
    /// </summary>
    static class dcamapi
    {
        // member function
        public static DCAMERR init( ref DCAMAPI_INIT param )
        {
            return dcamapidll.dcamapi_init(ref param);
        }
        public static DCAMERR uninit()
        {
            return dcamapidll.dcamapi_uninit();
        }
    }

    // ================ declaration class dcamdev  ================
    /// <summary>
    /// class for DCAM device
    /// </summary>
    class dcamdev
    {
        // open HDCAM handle
        public static DCAMERR open( ref DCAMDEV_OPEN param )
        {
            return dcamapidll.dcamdev_open(ref param);
        }

        // close HDCAM handle
        public static DCAMERR close(IntPtr hdcam)
        {
            return dcamapidll.dcamdev_close(hdcam);
        }

        // get string information
        public static DCAMERR getstring(IntPtr hdcam, DCAMIDSTR idstr, ref String str)
        {
            DCAMDEV_STRING param = new DCAMDEV_STRING(idstr);
            param.textbytes = 256;
            byte[] buf = new byte[param.textbytes];
            GCHandle handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
            param.text = handle.AddrOfPinnedObject();

            DCAMERR err;
            err = dcamapidll.dcamdev_getstring(hdcam, ref param);
            handle.Free();

            if (err.failed())
            {
                str = "";
            }
            else
            {
               int i;
                for( i = 0; i < buf.Count(); i++ )
                {
                    if (buf[i] == 0)
                        break;
                }
                str = Encoding.ASCII.GetString(buf).Substring(0,i);
            }

            return err;
        }

        // get capability of DCAM functions
        public static DCAMERR getcapability(IntPtr hdcam, ref DCAMDEV_CAPABILITY param)
        {
            return dcamapidll.dcamdev_getcapability(hdcam,ref param);
        }
    }

    // ================ declaration class dcambuf ================
    /// <summary>
    /// image  buffer control class
    /// </summary>
    class dcambuf
    {
        public static DCAMERR alloc(IntPtr hdcam,int framecount)
        {
            return dcamapidll.dcambuf_alloc(hdcam,framecount);
        }
        public static DCAMERR release(IntPtr hdcam, int iKind )
        {
            return dcamapidll.dcambuf_release(hdcam, iKind);
        }
        public static DCAMERR lockframe(IntPtr hdcam, ref DCAMBUF_FRAME aFrame)
        {
            return dcamapidll.dcambuf_lockframe(hdcam, ref aFrame);
        }
        public static DCAMERR copyframe(IntPtr hdcam, ref DCAMBUF_FRAME aFrame)
        {
            return dcamapidll.dcambuf_copyframe(hdcam, ref aFrame);
        }
        public static DCAMERR copymetadata_timestampblock(IntPtr hdcam, ref DCAM_TIMESTAMPBLOCK tsb)
        {
            return dcamapidll.dcambuf_copymetadata_timestampblock(hdcam, ref tsb);
        }
        public static DCAMERR copymetadata_framestampblock(IntPtr hdcam, ref DCAM_FRAMESTAMPBLOCK fsb)
        {
            return dcamapidll.dcambuf_copymetadata_framestampblock(hdcam, ref fsb);
        }
    }

    // ================ declaration class dcamcap ================
    /// <summary>
    /// capturing control class
    /// </summary>
    class dcamcap
    {
        public static DCAMERR start(IntPtr hdcam, DCAMCAP_START mode )
        {
            return dcamapidll.dcamcap_start(hdcam, (int)mode);
        }
        public static DCAMERR stop(IntPtr hdcam)
        {
            return dcamapidll.dcamcap_stop(hdcam);
        }
        public static DCAMERR status(IntPtr hdcam, ref DCAMCAP_STATUS param)
        {
            return dcamapidll.dcamcap_status(hdcam, ref param);
        }
        public static DCAMERR transferinfo(IntPtr hdcam, ref DCAMCAP_TRANSFERINFO param)
        {
            return dcamapidll.dcamcap_transferinfo(hdcam, ref param);
        }
        public static DCAMERR firetrigger(IntPtr hdcam, int iKind )
        {
            return dcamapidll.dcamcap_firetrigger(hdcam, iKind);
        }
        public static DCAMERR record(IntPtr hdcam, IntPtr hdcamrec)
        {
            return dcamapidll.dcamcap_record(hdcam, hdcamrec);
        }
    }

    // ================ declaration class dcamwait ================
    /// <summary>
    /// event waiting class
    /// </summary>
    class dcamwait
    {
        public static DCAMERR open(ref DCAMWAIT_OPEN param)
        {
            return dcamapidll.dcamwait_open(ref param);
        }
        public static DCAMERR close(IntPtr hwait)
        {
            return dcamapidll.dcamwait_close(hwait);
        }
        public static DCAMERR start(IntPtr hwait, ref DCAMWAIT_START param )
        {
            return dcamapidll.dcamwait_start(hwait,ref param);
        }
        public static DCAMERR abort(IntPtr hwait)
        {
            return dcamapidll.dcamwait_abort(hwait);
        }
    }

    // ================ declaration class dcamprop ================
    /// <summary>
    /// property control class
    /// </summary>
    class dcamprop
    {
        public static DCAMERR getattr(IntPtr hdcam, ref DCAMPROP_ATTR param)
        {
            return dcamapidll.dcamprop_getattr(hdcam,ref param);
        }
        public static DCAMERR getvalue(IntPtr hdcam, DCAMIDPROP iProp, ref double value)
        {
            return dcamapidll.dcamprop_getvalue(hdcam, iProp, ref value);
        }
        public static DCAMERR setvalue(IntPtr hdcam, DCAMIDPROP iProp, double value)
        {
            return dcamapidll.dcamprop_setvalue(hdcam, iProp, value);
        }
        public static DCAMERR setgetvalue(IntPtr hdcam, DCAMIDPROP iProp, ref double value, DCAMPROPOPTION _option)
        {
            return dcamapidll.dcamprop_setgetvalue(hdcam, iProp, ref value, _option);
        }
        public static DCAMERR queryvalue(IntPtr hdcam, DCAMIDPROP iProp, ref double value, DCAMPROPOPTION _option)
        {
            return dcamapidll.dcamprop_queryvalue(hdcam, iProp, ref value, _option);
        }
        public static DCAMERR getnextid(IntPtr hdcam, ref DCAMIDPROP iProp, int _option )
        {
            return dcamapidll.dcamprop_getnextid(hdcam, ref iProp, _option);
        }
        public static DCAMERR getname(IntPtr hdcam, DCAMIDPROP iProp, ref string ret)
        {
            Int32   textbytes = 256;
            StringBuilder   sb = new StringBuilder(textbytes); 

            DCAMERR err;
            err = dcamapidll.dcamprop_getname(hdcam, iProp, sb, textbytes);
            if( ! err.failed() )
                ret = sb.ToString();

            return err;
        }
        public static DCAMERR getvaluetext(IntPtr hdcam, DCAMIDPROP idprop, double value, ref string ret)
        {
            DCAMPROP_VALUETEXT param = new DCAMPROP_VALUETEXT(idprop, value);
            param.textbytes = 256;
            byte[] buf = new byte[param.textbytes];
            GCHandle handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
            param.text = handle.AddrOfPinnedObject();

            DCAMERR err;
            err = dcamapidll.dcamprop_getvaluetext(hdcam, ref param);
            handle.Free();

            if( err.failed()) 
            {
                ret = "";
            }
            else
            {
                int i;
                for (i = 0; i < buf.Count(); i++)
                {
                    if (buf[i] == 0)
                        break;
                }
                ret = Encoding.ASCII.GetString(buf).Substring(0, i);
            }

            return err;
        }
    }

    // ================ declaration class dcamrec ================
    /// <summary>
    /// recording functional class
    /// </summary>
    class dcamrec
    {
        public static DCAMERR open(ref DCAMREC_OPEN param ) {
            return dcamapidll.dcamrec_openW(ref param);
        }
        public static DCAMERR open(ref DCAMREC_OPENA param ) {
            return dcamapidll.dcamrec_openA(ref param);
        }

        public static DCAMERR status(IntPtr hdcamrec, ref DCAMREC_STATUS param ) {
            return dcamapidll.dcamrec_status(hdcamrec, ref param);
        }

        public static DCAMERR close(IntPtr hdcamrec) {
            return dcamapidll.dcamrec_close(hdcamrec);
        }

        public static DCAMERR lockframe(IntPtr hdcamrec, ref DCAMREC_FRAME aFrame ) {
            return dcamapidll.dcamrec_lockframe(hdcamrec, ref aFrame);
        }

        public static DCAMERR copyframe(IntPtr hdcamrec, ref DCAMREC_FRAME aFrame ) {
            return dcamapidll.dcamrec_copyframe(hdcamrec, ref aFrame);
        }

        public static DCAMERR writemetadata(IntPtr hdcamrec, ref DCAM_METADATAHDR hdr) {
            return dcamapidll.dcamrec_writemetadata(hdcamrec, ref hdr);
        }

        public static DCAMERR lockmetadata(IntPtr hdcamrec, ref DCAM_METADATAHDR hdr) {
            return dcamapidll.dcamrec_lockmetadata(hdcamrec, ref hdr);
        }

        public static DCAMERR copymetadata(IntPtr hdcamrec, ref DCAM_METADATAHDR hdr) {
            return dcamapidll.dcamrec_copymetadata(hdcamrec, ref hdr);
        }

        public static DCAMERR lockmetadatablock(IntPtr hdcamrec, ref DCAM_METADATABLOCKHDR hdr) {
            return dcamapidll.dcamrec_lockmetadatablock(hdcamrec, ref hdr);
        }

        public static DCAMERR copymetadatablock(IntPtr hdcamrec, ref DCAM_METADATABLOCKHDR hdr ) {
            return dcamapidll.dcamrec_copymetadatablock(hdcamrec, ref hdr);
        }
    }
}