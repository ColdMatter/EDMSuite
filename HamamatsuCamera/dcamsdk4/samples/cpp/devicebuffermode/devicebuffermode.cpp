/**
 @file devicebuffermode.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2020-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to use device buffer mode.
 @details	This program sets the parameters to use device buffer mode and accesses the captured image.
 @details	This program does not work with all cameras.
 @remarks	dcamprop_setvalue
 @remarks	dcambuf_lockframe
 @remarks	dcambuf_copyframe
 @remarks	dcamwait_start
 */

#include "../misc/console4.h"
#include "../misc/common.h"

 /**
  @def	USE_COPYFRAME
  *
  *0:	dcambuf_lockframe is used to access image.\n
  *		This function gets the pointer of images, so it is necessary to copy the target ROI from this poitner. It is possible to calculate the top pointer of each image bundled with the properties related framebundle.
  *
  *1:	dcambuf_copyframe is used to access image.\n
  *		This function sets the pointer of buffer to get the images. DCAM copies the target ROI of each image to this pointer.
  */
#define USE_COPYFRAME	0


/**
 @brief	Set device buffer mode.
 @param hdcam	DCAM handle
 @return	result of setting the parameter of device buffer mode
 */
BOOL set_devicebuffermode(HDCAM hdcam)
{
	DCAMERR err;

	err = dcamprop_setvalue(hdcam, DCAM_IDPROP_DEVICEBUFFER_MODE, DCAMPROP_DEVICEBUFFER_MODE__SNAPSHOT);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_setvalue()", "IDPROP:DEVICEBUFFER_MODE, VALUE:SNAPSHOT");
		return FALSE;
	}

	return TRUE;
}

/**
 @brief Get maximum frame count that device buffer can keep.
 @param hdcam		DCAM handle
 @param nMaxFrame	receive maximum frame count that device buffer can keep
 @return	result of getting maximum frame count that device buffer can keep
 */
BOOL get_maximum_framecount(HDCAM hdcam, int32& nMaxFrame)
{
	DCAMERR	err;

	double v;
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_DEVICEBUFFER_FRAMECOUNTMAX, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue()", "IDPROP:DEVICEBUFFER_FRAMECOUNTMAX");
		return FALSE;
	}

	nMaxFrame = (int32)v;

	return TRUE;
}

/**
 @brief	Copy image to the specified buffer by the specified area.
 @param	hdcam		DCAM handle
 @param iFrame		frame index
 @param buf		    buffer to copy image
 @param rowbytes	rowbytes of buf
 @param ho			horizontal offset
 @param vo			vertical offset
 @param hs			horizontal size
 @param vs			vertical size
 @param err			DCAM err
 @return	result of copy image
 */
BOOL copy_targetarea(HDCAM hdcam, int32 iFrame, void* buf, int32 rowbytes, int32 ho, int32 vo, int32 hs, int32 vs, DCAMERR& err)
{
	// prepare frame param
	DCAMBUF_FRAME bufframe;
	memset(&bufframe, 0, sizeof(bufframe));
	bufframe.size = sizeof(bufframe);
	bufframe.iFrame = iFrame;

#if USE_COPYFRAME
	// set user buffer information and copied ROI
	bufframe.buf = buf;
	bufframe.rowbytes = rowbytes;
	bufframe.left = ho;
	bufframe.top = vo;
	bufframe.width = hs;
	bufframe.height = vs;

	// access image
	err = dcambuf_copyframe(hdcam, &bufframe);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcambuf_copyframe()");
		return FALSE;
	}
#else
	// access image
	err = dcambuf_lockframe(hdcam, &bufframe);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcambuf_lockframe()");
		return FALSE;
	}

	if (bufframe.type != DCAM_PIXELTYPE_MONO16)
	{
		printf("not implement pixel type\n");
		return FALSE;
	}

	// copy target ROI
	int32 copyrowbytes = hs * 2;
	char* pSrc = (char*)bufframe.buf + vo * bufframe.rowbytes + ho * 2;
	char* pDst = (char*)buf;

	int y;
	for (y = 0; y < vs; y++)
	{
		memcpy_s(pDst, rowbytes, pSrc, copyrowbytes);

		pSrc += bufframe.rowbytes;
		pDst += rowbytes;
	}
#endif

	return TRUE;
}

/**
 @brief	Get image information from properties.
 @param	hdcam		DCAM handle
 @param pixeltype	DCAM_PIXELTYPE value
 @param width		image width
 @param rowbytes	image rowbytes
 @param height		image height
 */
void get_image_information(HDCAM hdcam, int32& pixeltype, int32& width, int32& rowbytes, int32& height)
{
	DCAMERR err;

	double v;

	// image pixel type(DCAM_PIXELTYPE_MONO16, MONO8, ... )
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue()", "IDPROP:IMAGE_PIXELTYPE");
		return;
	}
	else
		pixeltype = (int32)v;

	// image width
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_WIDTH, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue()", "IDPROP:IMAGE_WIDTH");
		return;
	}
	else
		width = (int32)v;

	// image row bytes
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_ROWBYTES, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue()", "IDPROP:IMAGE_ROWBYTES");
		return;
	}
	else
		rowbytes = (int32)v;

	// image height
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_HEIGHT, &v);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue()", "IDPROP:IMAGE_HEIGHT");
		return;
	}
	else
		height = (int32)v;
}

/**
 @brief Sample to capture and access image on using device buffer mode.
 @details	This function accesses the image on capturing and accesses the missing frame on real-time transfer after capturing.
 @param		hdcam	DCAM handle
 @param		hwait	DCAMWAIT handle
 @sa	get_imageinformation
 @return	result of DCAM function
 */
int sample_capture_and_access_image(HDCAM hdcam, HDCAMWAIT hwait, int32 nFrame)
{
	int ret = 0;

	DCAMERR err;

	// get image information

	int32 pixeltype = 0, width = 0, rowbytes = 0, height = 0;
	get_image_information(hdcam, pixeltype, width, rowbytes, height);
	
	if (pixeltype != DCAM_PIXELTYPE_MONO16)
	{
		printf("not implement\n");	// it is necessary to implement the calculation the buffer size of different pixeltype.
		return 1;
	}

	// set copy rectangle
	int32 xoffset = 0;
	int32 yoffset = 0;
	int32 xsize = width;
	int32 ysize = height;

	if (xoffset + xsize > width || yoffset + ysize > height)
	{
		printf("copy rectangle is larger than image size\n");
		return 1;
	}

	int32 copyrowbytes = xsize * 2;	// DCAM_PIXELTYPE_MONO16 is assumed.
	int32 framebytes = copyrowbytes * ysize;

	// allocate buffer to copy image
	char** pFrames = new char*[nFrame];
	for (int i = 0; i < nFrame; i++)
	{
		pFrames[i] = new char[framebytes];
		memset(pFrames[i], 0, framebytes);
	}

	int32 nMissingFrame = 0;
	// allocate buffer to manage the index of missing frame
	int32* missingindex = new int32[nFrame];
	for (int i = 0; i < nFrame; i++)
	{
		missingindex[i] = -1;
	}

	// start capture
	err = dcamcap_start(hdcam, DCAMCAP_START_SNAP);	// it needs to set SNAP mode on using device buffer mode. if not, dcamcap_start() returns DCAMERR_REQUIREDSNAP.
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamcap_start()", "mode:SNAP");
		ret = 1;
	}
	else
	{
		printf("\nStart Capture\n");

		// set wait param
		DCAMWAIT_START	waitstart;
		memset(&waitstart, 0, sizeof(waitstart));
		waitstart.size = sizeof(waitstart);
		
		waitstart.timeout = 2000;

		DCAMBUF_FRAME	bufframe;
		int32 nAccessFrameCount = 0;
		int32 nCopiedFrameCount = 0;
		int32* pReloadIndex = missingindex;

		while (1)
		{
			if (nAccessFrameCount < nFrame)
			{
				// real time access
				waitstart.eventmask = DCAMWAIT_CAPEVENT_FRAMEREADY | DCAMWAIT_CAPEVENT_STOPPED;

				err = dcamwait_start(hwait, &waitstart);
				if (failed(err))
				{
					if (err == DCAMERR_DELAYEDFRAME)	// failed real time transfer. this frame is read again from device after captured specified frames.
						continue;

					dcamcon_show_dcamerr(hdcam, err, "dcamwait_start()");
					ret = 1;
					break;
				}

				if (waitstart.eventhappened & DCAMWAIT_CAPEVENT_FRAMEREADY)
				{
					DCAMCAP_TRANSFERINFO	captransferinfo;
					memset(&captransferinfo, 0, sizeof(captransferinfo));
					captransferinfo.size = sizeof(captransferinfo);

					err = dcamcap_transferinfo(hdcam, &captransferinfo);
					if (failed(err))
					{
						dcamcon_show_dcamerr(hdcam, err, "dcamcap_transferinfo()");
						ret = 1;
						break;
					}
					else
					{
						while (nAccessFrameCount < captransferinfo.nFrameCount)
						{
							// copy image
							if (copy_targetarea(hdcam, nAccessFrameCount, pFrames[bufframe.iFrame], copyrowbytes, xoffset, yoffset, width, height, err))
							{
								nCopiedFrameCount++;
							}
							else
							{
								if (err != DCAMERR_DELAYEDFRAME)
								{
									// unexpected error
									ret = 1;
									break;
								}
								else
								{
									missingindex[nMissingFrame] = bufframe.iFrame;
									nMissingFrame++;
								}
							}

							nAccessFrameCount++;
						}
					}
				}
			}
			else
			{
				// reload image from device buffer
				waitstart.eventmask = DCAMWAIT_CAPEVENT_RELOADFRAME | DCAMWAIT_CAPEVENT_STOPPED;

				err = dcamwait_start(hwait, &waitstart);
				if (failed(err))
				{
					dcamcon_show_dcamerr(hdcam, err, "dcamwait_start()");
					ret = 1;
					break;
				}

				if (waitstart.eventhappened & DCAMWAIT_CAPEVENT_RELOADFRAME)
				{
					double v;
					err = dcamprop_getvalue(hdcam, DCAM_IDPROP_TRANSFERINFO_FRAMECOUNT, &v);
					if (failed(err))
					{
						dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue()", "IDPROP:TRANSFERINFO_FRAMECOUNT");
						ret = 1;
						break;
					}
					else
					{
						int32 nFrameCount = (int32)v;

						int32 nReloaded = nFrameCount - nCopiedFrameCount;
						for (int32 i = 0; i < nReloaded; i++)
						{
							int32 index = *pReloadIndex++;

							memset(&bufframe, 0, sizeof(bufframe));
							bufframe.size = sizeof(bufframe);
							bufframe.iFrame = index;
							err = dcambuf_lockframe(hdcam, &bufframe);
							if (failed(err))
							{
								dcamcon_show_dcamerr(hdcam, err, "dcambuf_lockframe()", "iFrame:%d", bufframe.iFrame);
								ret = 1;
								break;
							}
							else
							{
								char* pSrc = (char*)bufframe.buf;
								char* pDst = pFrames[bufframe.iFrame];
								for (int i = 0; i < bufframe.height; i++)
								{
									memcpy_s(pDst, rowbytes, pSrc, rowbytes);
									pSrc += bufframe.rowbytes;
									pDst += rowbytes;
								}

								nCopiedFrameCount++;
							}
						}
					}
				}
			}

			if (ret != 0 || waitstart.eventhappened & DCAMWAIT_CAPEVENT_STOPPED)
				break;
		}

		// stop capture
		dcamcap_stop(hdcam);
		printf("Stop Capture\n");
	}

	// delete buffer to copy image
	for (int i = 0; i < nFrame; i++)
	{
		delete pFrames[i];
	}

	delete pFrames;

	return ret;
}

int main(int argc, char* const argv[])
{
	printf("PROGRAM START\n");

	int	ret = 0;

	DCAMERR err;

	// initialize DCAM-API and open device
	HDCAM hdcam;
	hdcam = dcamcon_init_open();
	if (hdcam == NULL)
	{
		// failed open DCAM handle
		ret = 1;
	}
	else
	{
		// show device information
		dcamcon_show_dcamdev_info(hdcam);

		// open wait handle
		DCAMWAIT_OPEN	waitopen;
		memset(&waitopen, 0, sizeof(waitopen));
		waitopen.size = sizeof(waitopen);
		waitopen.hdcam = hdcam;

		err = dcamwait_open(&waitopen);
		if (failed(err))
		{
			dcamcon_show_dcamerr(hdcam, err, "dcamwait_open()");
			ret = 1;
		}
		else
		{
			HDCAMWAIT hwait = waitopen.hwait;

			// TODO: add your process to set sensor mode, subarray, binning, etc...

			// set device buffer mode
			if (set_devicebuffermode(hdcam))
			{
				int32 max_framecount;
				// get maximum frame count of device buffer
				if (get_maximum_framecount(hdcam, max_framecount))
				{
					int32 number_of_buffer = 1000;
					if (number_of_buffer <= max_framecount)
					{
						// allocate buffer
						err = dcambuf_alloc(hdcam, number_of_buffer);
						if (failed(err))
						{
							dcamcon_show_dcamerr(hdcam, err, "dcambuf_alloc()");
							ret = 1;
						}
						else
						{
							sample_capture_and_access_image(hdcam, hwait, number_of_buffer);

							// release buffer
							dcambuf_release(hdcam);
						}
					}
					else
					{
						printf("setting count is larger than device buffer\n");
						ret = 1;
					}
				}
			}

			// close wait handle
			dcamwait_close(hwait);
		}

		// close DCAM handle
		dcamdev_close(hdcam);
	}

	// finalize DCAM-API
	dcamapi_uninit();

	printf("PROGRAM END\n");
	return ret;	// 0:Success, Other:Failure
}