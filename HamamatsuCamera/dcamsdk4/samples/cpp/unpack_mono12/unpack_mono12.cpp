/**
 @file unpack_mono12.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to unpack mono12 data.
 @details	This program retrieves mono 12 packed data from DCAM and unpacks the data.
 @details	This program does not work with all cameras.
 @remarks	dcamprop_setvalue
 */

#include "../misc/console4.h"
#include "../misc/common.h"

/**
 @def	USE_PIXELTYPE_MONO12P
 *
 *0:	Set pixel type to mono12.
 *
 *1:	Set pixel type to mono12P.
 */
#define	USE_PIXELTYPE_MONO12P	1

/**
 @brief	Unpack mono12 images.
 @param	pSrcTop			pointer of captured packed 12bit data
 @param srcRowbytes		rowbytes of pSrcTop
 @param pDstTop		    buffer to store the image unpacked data of pSrcTop
 @param dstRowbytes		rowbytes of pDstTop
 @param width			image width
 @param height			image height
 */
void unpack_mono12_image( void* pSrcTop, int32 srcRowbytes, void* pDstTop, int32 dstRowbytes, int32 width, int32 height )
{
	WORD lut1[65536];
	WORD lut2[65536];

#if USE_PIXELTYPE_MONO12P
	// make lut to unpack MONO12P
	int i, j;
	for( i=0; i<65536; i++ )
	{
		WORD w = (WORD)i;
		BYTE* p = (BYTE*)&w;

		lut1[i] = p[0] + ((p[1] & 0x0F) << 8);
		lut2[i] = (p[1] << 4) + ((p[0] & 0xF0) >> 4);
	}
#else
	// make lut to unpack MONO12
	int i, j;
	for( i=0; i<65536; i++ )
	{
		WORD w = (WORD)i;
		BYTE* p = (BYTE*)&w;

		lut1[i] = (p[0] << 4) + (p[1] & 0x0F);
		lut2[i] = (p[1] << 4) + ((p[0] & 0xF0) >> 4);
	}
#endif

	// unpack MONO12 and copy
	char* src = (char*)pSrcTop;
	char* dst = (char*)pDstTop;

	for( i=0; i<height; i++ )
	{
		WORD* pDst = (WORD*)(dst + dstRowbytes * i);
		BYTE* pSrc = (BYTE*)(src + srcRowbytes * i);
		for( j=0; j<width/2; j++ )
		{
			*pDst++ = lut1[*(WORD*)pSrc++];
			*pDst++ = lut2[*(WORD*)pSrc++];
			pSrc++;
		}
	}
}

/**
 @brief	Sample used to unpack the captured image of mono12.
 @param	hdcam	DCAM handle
 @sa	unpack_mono12_image
 */
void sample_access_mono12_image( HDCAM hdcam )
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

	// prepare frame param
	DCAMBUF_FRAME	bufframe;
	memset( &bufframe, 0, sizeof(bufframe) );
	bufframe.size	= sizeof(bufframe);
	bufframe.iFrame	= captransferinfo.nNewestFrameIndex;

	// access image
	err = dcambuf_lockframe( hdcam, &bufframe );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcambuf_lockframe()" );
		return;
	}

#if USE_PIXELTYPE_MONO12P
	if( bufframe.type != DCAM_PIXELTYPE_MONO12P )
#else
	if( bufframe.type != DCAM_PIXELTYPE_MONO12 )
#endif
	{
		printf( "not MONO12 image.\n" );
		return;
	}

	int32 rowbytes		= bufframe.width * 2;
	int32 framebytes	= rowbytes * bufframe.height;
	char* pImage = new char[ framebytes ];
	memset( pImage, 0, framebytes );

	unpack_mono12_image( bufframe.buf, bufframe.rowbytes, pImage, rowbytes, bufframe.width, bufframe.height );

	delete pImage;
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

			// set mono12
#if	USE_PIXELTYPE_MONO12P
			err = dcamprop_setvalue( hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, DCAM_PIXELTYPE_MONO12P );
#else
			err = dcamprop_setvalue( hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, DCAM_PIXELTYPE_MONO12 );
#endif
			if( !failed(err) )
			{
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
						sample_access_mono12_image( hdcam );
					}

					// release buffer
					dcambuf_release( hdcam );
				}
			}
			else
			{
#if USE_PIXELTYPE_MONO12P
				dcamcon_show_dcamerr( hdcam, err, "dcamprop_setvalue()", "IDPROP:IMAGE_PIXELTYPE, VALUE:DCAM_PIXELTYPE_MONO12P" );
#else
				dcamcon_show_dcamerr( hdcam, err, "dcamprop_setvalue()", "IDPROP:IMAGE_PIXELTYPE, VALUE:DCAM_PIXELTYPE_MONO12" );
#endif
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
	return ret;
}