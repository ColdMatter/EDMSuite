/**
 @file access_image.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to access image.
 @details	This program accesses the captured image. The function used to access image is changed by the directive "USE_COPYFRAME".
 @details	This program outputs raw data if the directive "OUTPUT_IMAGE" is enable.
 @remarks	dcambuf_lockframe
 @remarks	dcambuf_copyframe
 */

#include "../misc/console4.h"
#include "../misc/common.h"

/**
 @def	USE_COPYFRAME
 *
 *0:	dcambuf_lockframe is used to access image.\n
 *		This function gets the pointer of image, so it is necessary to copy the target ROI from this pointer.
 *
 *1:	dcambuf_copyframe is used to access image.\n
 *		This function sets the pointer of buffer to get image. DCAM copies the target ROI to this buffer.
 */
#define USE_COPYFRAME	0

/**
 @def	OUTPUT_IMAGE
 *
 *0:	Not output image.
 *
 *1:	Output the accessed image with sequential name.
 */
#define OUTPUT_IMAGE	0

/**
 @brief	Copy image to the specified buffer by the specified area.
 @param	hdcam		DCAM handle
 @param iFrame		frame index
 @param buf		    buffer to copy image
 @param rowbytes	rowbytes of buf
 @param ox			horizontal offset
 @param oy			vertical offset
 @param cx			horizontal size
 @param cy			vertical size
 @return	result of copy image
 */
BOOL copy_targetarea( HDCAM hdcam, int32 iFrame, void* buf, int32 rowbytes, int32 ox, int32 oy, int32 cx, int32 cy )
{
	DCAMERR err;

	// prepare frame param
	DCAMBUF_FRAME bufframe;
	memset( &bufframe, 0, sizeof(bufframe) );
	bufframe.size	= sizeof(bufframe);
	bufframe.iFrame	= iFrame;

#if USE_COPYFRAME
	// set user buffer information and copied ROI
	bufframe.buf		= buf;
	bufframe.rowbytes	= rowbytes;
	bufframe.left		= ox;
	bufframe.top		= oy;
	bufframe.width		= cx;
	bufframe.height		= cy;
	
	// access image
	err = dcambuf_copyframe( hdcam, &bufframe );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcambuf_copyframe()" );
		return FALSE;
	}
#else
	// access image
	err = dcambuf_lockframe( hdcam, &bufframe );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcambuf_lockframe()" );
		return FALSE;
	}

	if( bufframe.type != DCAM_PIXELTYPE_MONO16 )
	{
		printf( "not implement pixel type\n" );
		return FALSE;
	}

	// copy target ROI
	int32 copyrowbytes = cx * 2;
	char* pSrc = (char*)bufframe.buf + oy * bufframe.rowbytes + ox * 2;
	char* pDst = (char*)buf;

	int y;
	for( y = 0; y < cy; y++ )
	{
		memcpy_s( pDst, rowbytes, pSrc, copyrowbytes );

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
void get_image_information( HDCAM hdcam, int32& pixeltype, int32& width, int32& rowbytes, int32& height )
{
	DCAMERR err;

	double v;

	// image pixel type(DCAM_PIXELTYPE_MONO16, MONO8, ... )
	err = dcamprop_getvalue( hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, &v );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamprop_getvalue()", "IDPROP:IMAGE_PIXELTYPE" );
		return;
	}
	else
		pixeltype = (int32) v;

	// image width
	err = dcamprop_getvalue( hdcam, DCAM_IDPROP_IMAGE_WIDTH, &v );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamprop_getvalue()", "IDPROP:IMAGE_WIDTH" );
		return;
	}
	else
		width = (int32)v;

	// image row bytes
	err = dcamprop_getvalue( hdcam, DCAM_IDPROP_IMAGE_ROWBYTES, &v );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamprop_getvalue()", "IDPROP:IMAGE_ROWBYTES" );
		return;
	}
	else
		rowbytes = (int32)v;

	// image height
	err = dcamprop_getvalue( hdcam, DCAM_IDPROP_IMAGE_HEIGHT, &v );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamprop_getvalue()", "IDPROP:IMAGE_HEIGHT" );
		return;
	}
	else
		height = (int32)v;
}

/**
 @brief	Sample used to process image after capturing.
 @details	This function copies the target area that is 10% of full area on the center.
 @param	hdcam	DCAM handle
 @sa	get_image_information, copy_targetarea
 */
void sample_access_image( HDCAM hdcam )
{
	DCAMERR err;

	// transferinfo param
	DCAMCAP_TRANSFERINFO captransferinfo;
	memset( &captransferinfo, 0, sizeof(captransferinfo) );
	captransferinfo.size	= sizeof(captransferinfo);

	// get number of captured image
	err = dcamcap_transferinfo( hdcam, &captransferinfo );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamcap_transferinfo()" );
		return;
	}

	if( captransferinfo.nFrameCount < 1 )
	{
		printf( "not capture image\n" );
		return;
	}

	// get image information
	int32 pixeltype = 0, width = 0, rowbytes = 0, height = 0;
	get_image_information( hdcam, pixeltype, width, rowbytes, height );

	if( pixeltype != DCAM_PIXELTYPE_MONO16 )
	{
		printf( "not implement\n" );
		return;
	}

	int32 cx = width / 10;
	int32 cy = height / 10;
	if( cx < 10 )	cx = 10;
	if( cy < 10 )	cy = 10;
	
	if( cx > width || cy > height )
	{
		printf( "frame is too small\n" );
		return;
	}

	int32 ox = (width - cx) / 2;
	int32 oy = (height - cy) / 2;

	char* buf = new char[ cx * 2 * cy ];
	memset( buf, 0, cx * 2 * cy );

	int iFrame;
	for( iFrame = 0; iFrame < captransferinfo.nFrameCount; iFrame++ )
	{
		// copy image
		copy_targetarea( hdcam, iFrame, buf, cx * 2, ox, oy, cx, cy );

		{
			// process image
#if OUTPUT_IMAGE
			char filename[MAX_PATH];
			sprintf_s( filename, sizeof(filename), "output%03d.raw", iFrame );
			output_data( filename, buf, cx * 2 * cy );
#endif
		}
	}

	delete[] buf;
}

int main( int argc, char* const argv[] )
{
	printf( "PROGRAM START\n" );

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
		dcamcon_show_dcamdev_info( hdcam );

		// open wait handle
		DCAMWAIT_OPEN	waitopen;
		memset( &waitopen, 0, sizeof(waitopen) );
		waitopen.size	= sizeof(waitopen);
		waitopen.hdcam	= hdcam;

		err = dcamwait_open( &waitopen );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcamwait_open()" );
			ret = 1;
		}
		else
		{
			HDCAMWAIT hwait = waitopen.hwait;

			// allocate buffer
			int32 number_of_buffer = 10;
			err = dcambuf_alloc( hdcam, number_of_buffer );
			if( failed(err) )
			{
				dcamcon_show_dcamerr( hdcam, err, "dcambuf_alloc()" );
				ret = 1;
			}
			else
			{
				// start capture
				err = dcamcap_start( hdcam, DCAMCAP_START_SEQUENCE );
				if( failed(err) )
				{
					dcamcon_show_dcamerr( hdcam, err, "dcamcap_start()" );
					ret = 1;
				}
				else
				{
					printf( "\nStart Capture\n" );

					// set wait param
					DCAMWAIT_START waitstart;
					memset( &waitstart, 0, sizeof(waitstart) );
					waitstart.size		= sizeof(waitstart);
					waitstart.eventmask	= DCAMWAIT_CAPEVENT_FRAMEREADY;
					waitstart.timeout	= 1000;

					// wait image
					err = dcamwait_start( hwait, &waitstart );
					if( failed(err) )
					{
						dcamcon_show_dcamerr( hdcam, err, "dcamwait_start()" );
						ret = 1;
					}

					// stop capture
					dcamcap_stop( hdcam );
					printf( "Stop Capture\n" );

					// access image
					printf( "Access Image\n" );
					sample_access_image( hdcam );
				}

				// release buffer
				dcambuf_release( hdcam );
			}

			// close wait handle
			dcamwait_close( hwait );
		}

		// close DCAM handle
		dcamdev_close( hdcam );
	}

	// finalize DCAM-API
	dcamapi_uninit();

	printf( "PROGRAM END\n" );
	return ret;	// 0:Success, Other:Failure
}
