/**
 @file		burst_copy.cpp
 @date		2024-08-02

 @copyright	Copyright (c) 2024 Hamamatsu Photinics K.K. All rights reserved.

 @brief		Sample code for continuous copying to the user buffer.
 @details	This program is designed to go back and copy the frame count to the user buffer if there is a difference of more than 2 frames.
 @remarks	dcamwait_*
 @remarks	dcamcap_transferinfo
 @remarks	dcambuf_lockframe
 */

#include "../misc/console4.h"
#include "../misc/common.h"
#include "../misc/qthread.h"

bool my_dcamprop_getvalue(HDCAM hdcam, int32 idprop, int32& lValue, const char* propname)
{
	DCAMERR	err;
	double	value;
	err = dcamprop_getvalue(hdcam, idprop, &value);
	if (failed(err))
	{
		dcamcon_show_dcamerr(hdcam, err, "dcamprop_getvalue()", "IDPROP:%s", propname);
		return false;
	}

	lValue = (int32)value;
	return true;
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
	int32 v;

	// image pixel type(DCAM_PIXELTYPE_MONO16, MONO8, ... )
	if( !my_dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, v, "IMAGE_PIXELTYPE") )
		return;
	else
		pixeltype = (int32)v;

	// image width
	if( !my_dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_WIDTH, v, "IMAGE_WIDTH") )
		return;
	else
		width = (int32)v;

	// image row bytes
	if( !my_dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_ROWBYTES, v, "IMAGE_ROWBYTES") )
		return;
	else
		rowbytes = (int32)v;

	// image height
	if( !my_dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_HEIGHT, v, "IMAGE_HEIGHT") )
		return;
	else
		height = (int32)v;
}

/**
 @brief	Start and stop the thread to capture and copying the images.
 @param hdcam				DCAM handle
 @param hwait				DCAMWAIT handle
 @param buffer				pointer array of attached buffer to receive the image
 @param nUserBufCount		number of user allocated buffer count
 @param nPrimaryBufCount	number of primary buffer count
 */
void wait_frame_and_copy( HDCAM hdcam, HDCAMWAIT hwait, void** buffer, int32 nUserBufCount, int32 nPrimaryBufCount )
{
	if ( hdcam == NULL || hwait == NULL )
		return;

	// wait start param
	DCAMWAIT_START waitstart;
	memset( &waitstart, 0, sizeof(waitstart) );
	waitstart.size = sizeof(waitstart);
	waitstart.eventmask = DCAMWAIT_CAPEVENT_FRAMEREADY;
	waitstart.timeout = 1000;

	// transferinfo param
	DCAMCAP_TRANSFERINFO captransferinfo;
	memset( &captransferinfo, 0, sizeof(captransferinfo) );
	captransferinfo.size = sizeof(captransferinfo);

	// get image information
	int32 pixeltype = 0, width = 0, rowbytes = 0, height = 0;
	get_image_information( hdcam, pixeltype, width, rowbytes, height );

	int32 iNextcopy = 0;
	while( 1 )
	{
		DCAMERR err;

		// wait image
		err = dcamwait_start( hwait, &waitstart );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcamwait_start()" );
			if( err == DCAMERR_ABORT )
			{
				// receive abort signal
				break;
			}

			continue;
		}

		// get number of captured image
		err = dcamcap_transferinfo( hdcam, &captransferinfo );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcamcap_transferinfo()" );
			continue;
		}

		int32 nFrameCount = captransferinfo.nFrameCount;

		while( iNextcopy < nFrameCount )
		{
			// prepare frame param
			DCAMBUF_FRAME bufframe;
			memset( &bufframe, 0, sizeof(bufframe) );
			bufframe.size = sizeof(bufframe);
			bufframe.iFrame = iNextcopy % nPrimaryBufCount;

			// set user buffer information
			bufframe.buf = buffer[ iNextcopy % nUserBufCount ];
			bufframe.rowbytes = rowbytes;
			bufframe.left = 0;
			bufframe.top = 0;
			bufframe.width = width;
			bufframe.height = height;

			// access image
			err = dcambuf_copyframe( hdcam, &bufframe );
			if (failed(err))
			{
				dcamcon_show_dcamerr( hdcam, err, "dcambuf_lockframe()" );

				continue;
			}

			iNextcopy++;
		}
	}
}

/*! @class my_thread
    @brief Calculate average of captured image until dcamwait_start() return DCAMERR_ABORT.
 */
class my_thread : public qthread
{
public:

		/*! constructor */
		my_thread();

public:
		/*! pure virtual function of qthread. execute this on creating thread.
		    @return 0
		*/

		int32 main();

public:

		/*! DCAM handle */
		HDCAM m_hdcam;
		/*! DCAMWAIT handle */
		HDCAMWAIT m_hwait;

		/*! attached buffer by dcambuf_attach() */
		void** m_pBuffer;
		/*! number of user allocated buffer count */
		int32 m_nUserBufCount;
		/*!	number of primary buffer count */
		int32 m_nPrimaryBufCount;
};

/**
 @brief	Define initial values.
*/
my_thread::my_thread()
{
	m_hdcam = NULL;
	m_hwait = NULL;

	m_pBuffer = NULL;

	m_nUserBufCount = 0;
	m_nPrimaryBufCount = 0;
}

/**
 @brief		Thread process to wait the captured image and copying to the user buffer.
 @return	result of thread process
 */
int32 my_thread::main()
{
	wait_frame_and_copy( m_hdcam, m_hwait, m_pBuffer, m_nUserBufCount, m_nPrimaryBufCount );

	return 0;
}

/**
 @brief	Start and stop the thread to capture and copying the images.
 @param hdcam				DCAM handle
 @param hwait				DCAMWAIT handle
 @param pUserBuf			pointer array of attached buffer to receive the image
 @param nUserBufCount		number of user allocated buffer count
 @param nPrimaryBufCount	number of primary buffer count
 */
void sample_wait_frame_abort_and_copy( HDCAM hdcam, HDCAMWAIT hwait, void** pUserBuf, int32 nUserBufCount, int32 nPrimaryBufCount )
{
	if (hdcam == NULL || hwait == NULL || pUserBuf == NULL || nUserBufCount < 1 || nPrimaryBufCount < 1)
		return;

	my_thread	thread;
	thread.m_hdcam = hdcam;
	thread.m_hwait = hwait;
	thread.m_pBuffer = pUserBuf;
	thread.m_nUserBufCount = nUserBufCount;
	thread.m_nPrimaryBufCount = nPrimaryBufCount;

	printf( "Hit Enter key to stop capturing\n" );

	// start thread to wait and calculate image
	thread.start();

	// wait user input
	getchar();

	// abort signal to dcamwait_start
	dcamwait_abort(hwait);

	thread.wait_terminate();
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

			// allocate buffer
			int32	bufframebytes;
			if ( ! my_dcamprop_getvalue( hdcam, DCAM_IDPROP_IMAGE_FRAMEBYTES, bufframebytes, "IMAGE_FRAMEBYTES" ) )
			{
				ret = 1;
			}
			else
			{
				int		number_of_buffer = 100;
				void**	pFrames = new void*[ number_of_buffer ];

				char* buf = new char[ bufframebytes * number_of_buffer ];
				memset( buf, 0, bufframebytes * number_of_buffer );

				int		i;
				for( i = 0; i < number_of_buffer; i++ )
				{
					pFrames[ i ] = buf + bufframebytes * i;
				}

				// allocate buffer
				int	number_of_primarybuffer = 10;
				err = dcambuf_alloc( hdcam, number_of_primarybuffer );
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
						// wait image and calculate average
						// stop to capture on hit the key

						sample_wait_frame_abort_and_copy( hdcam, hwait, pFrames, number_of_buffer, number_of_primarybuffer );

						// stop capture
						dcamcap_stop( hdcam );
						printf( "Stop Capture\n" );
					}

					// release buffer
					dcambuf_release( hdcam );
				}

				// free buffer
				delete buf;
				delete pFrames;
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
