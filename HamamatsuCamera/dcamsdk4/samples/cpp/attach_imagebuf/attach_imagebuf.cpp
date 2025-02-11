/**
 @file attach_imagebuf.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to attach buffers to receive image data.
 @details	This program attaches the buffers to receive image data and accesses the captured image.
 @remarks	dcambuf_attach
 @remarks	dcambuf_release
 */

#include "../misc/console4.h"
#include "../misc/common.h"
#include "../misc/qthread.h"

/**
 @brief	Calculate the average of center of image.
 @param buf			pointer of top of image
 @param rowbytes	row bytes of buf
 @param	type		DCAM pixel type
 @param width		image width
 @param height		image height
 @return	average of center of image
 */
double calc_average( const void* buf, int32 rowbytes, DCAM_PIXELTYPE type, int32 width, int32 height )
{
	if( type != DCAM_PIXELTYPE_MONO16 )
	{
		// not implement
		return -1;
	}

	int32	cx = width / 10;
	int32	cy = height / 10;
	if( cx < 10 )	cx = 10;
	if( cy < 10 )	cy = 10;
	if( cx > width || cy > height )
	{
		// frame is too small
		return -1;
	}

	int32	ox = (width - cx) / 2;
	int32	oy = (width - cy) / 2;

	const char*	src = (const char*)buf + rowbytes * oy;
	double total = 0;

	// calculate center sum
	int32 x, y;
	for( y=0; y < cy; y++ )
	{
		const unsigned short*	s = (const unsigned short*)src + ox;
		for( x = 0; x < cx; x++ )
		{
			total += *s++;
		}
	}

	return total / cx / cy;
}

/**
 @brief	Get the value of DCAM property.
 @param hdcam		DCAM handle
 @param iProp		target property ID
 @param	lValue		value of target property
 @param propname	target property name
 @return	result of getting the value of DCAM property
 */
bool my_dcamprop_getvalue( HDCAM hdcam, int32 iProp, int32& lValue, const char* propname )
{
	DCAMERR	err;
	double	value;
	err = dcamprop_getvalue( hdcam, iProp, &value );
	if( failed( err ) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamprop_getvalue()", "IDPROP:%s", propname );
		return false;
	}

	lValue = (int32)value;
	return true;
}

/**
 @brief	Start capturing images and calculate the average of the specified number of images.
 @param hdcam	DCAM handle
 @param hwait	DCAMWAIT handle
 @param buffer	pointer array of attached buffers to DCAM
 @sa	my_dcamprop_getvalue, calc_average
 */
void sample_wait_and_calc( HDCAM hdcam, HDCAMWAIT hwait, void** buffer )
{
	if( hdcam == NULL || hwait == NULL )
		return;

	int32 topoffset, rowbytes, pixeltype, width, height;
	// get image information
	if( ! my_dcamprop_getvalue( hdcam, DCAM_IDPROP_BUFFER_TOPOFFSETBYTES, topoffset, "BUFFER_TOPOFFSETBYTES" )
	 || ! my_dcamprop_getvalue( hdcam, DCAM_IDPROP_BUFFER_ROWBYTES,       rowbytes,  "BUFFER_ROWBYTES" )
	 || ! my_dcamprop_getvalue( hdcam, DCAM_IDPROP_BUFFER_PIXELTYPE,      pixeltype, "BUFFER_PIXELTYPE" )
	 || ! my_dcamprop_getvalue( hdcam, DCAM_IDPROP_IMAGE_WIDTH,           width,     "IMAGE_WIDTH" )
	 || ! my_dcamprop_getvalue( hdcam, DCAM_IDPROP_IMAGE_HEIGHT,          height,    "IMAGE_HEIGHT" ) )
	{
		// should not happen
		return;
	}

	// wait start param
	DCAMWAIT_START waitstart;
	memset( &waitstart, 0, sizeof(waitstart) );
	waitstart.size		= sizeof(waitstart);
	waitstart.eventmask	= DCAMWAIT_CAPEVENT_FRAMEREADY;
	waitstart.timeout	= 1000;

	// transferinfo param
	DCAMCAP_TRANSFERINFO	captransferinfo;
	memset( &captransferinfo, 0, sizeof(captransferinfo) );
	captransferinfo.size		= sizeof(captransferinfo);
	
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

		// calculate average of latest image
		char* buf = (char*)buffer[ captransferinfo.nNewestFrameIndex ];
		buf += topoffset;
		double v = calc_average( buf, rowbytes, (DCAM_PIXELTYPE)pixeltype, width, height );
		printf( "%g\n", v );
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
		HDCAM		m_hdcam;
		/*! DCAMWAIT handle */
		HDCAMWAIT	m_hwait;

		/*! attached buffer by dcambuf_attach() */
		void**	m_pBuffer;
};

/**
 @brief	Define initial values.
*/
my_thread::my_thread()
{
	m_hdcam	= NULL;
	m_hwait	= NULL;

	m_pBuffer = NULL;
}

/**
 @brief	Thread process to wait the captured image and calculate the average of image.
 @return	result of thread process
 */
int32 my_thread::main()
{
	sample_wait_and_calc( m_hdcam, m_hwait, m_pBuffer );

	return 0;
}

/**
 @brief	Start and stop the thread to capture the image and calculate the average of image.
 @param hdcam		DCAM handle
 @param hwait		DCAMWAIT handle
 @param pAttachBuf	pointer array of attached buffers by dcambuf_attach()
 @param bBufCount	number of attached buffers
 */
void sample_wait_abort_and_calc( HDCAM hdcam, HDCAMWAIT hwait, void** pAttachBuf, long nBufCount )
{
	if( hdcam == NULL || hwait == NULL || pAttachBuf == NULL || nBufCount < 1 )
		return;

	my_thread	thread;
	thread.m_hdcam	= hdcam;
	thread.m_hwait	= hwait;
	thread.m_pBuffer		= pAttachBuf;

	printf( "Hit Enter key to stop capturing\n" );

	// start thread to wait and calculate image
	thread.start();

	// wait user input
	getchar();

	// abort signal to dcamwait_start
	dcamwait_abort( hwait );

	thread.wait_terminate();
}

int main( int argc, char* const argv[] )
{
	printf( "PROGRAM START\n" );

	int	ret = 0;

	DCAMERR err;

	// initialize DCAM-API and open device
	HDCAM hdcam;
	hdcam = dcamcon_init_open();
	if( hdcam == NULL )
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
			int32	bufframebytes;
			if( ! my_dcamprop_getvalue( hdcam, DCAM_IDPROP_BUFFER_FRAMEBYTES, bufframebytes, "BUFFER_FRAMEBYTES" ) )
			{
				ret = 1;
			}
			else
			{
				int		number_of_buffer = 10;
				void**	pFrames = new void*[ number_of_buffer ];

				char* buf = new char[ bufframebytes * number_of_buffer ];
				memset( buf, 0, bufframebytes * number_of_buffer );

				int		i;
				for( i = 0; i < number_of_buffer; i++ )
				{
					pFrames[ i ] = buf + bufframebytes * i;
				}

				DCAMBUF_ATTACH bufattach;
				memset( &bufattach, 0, sizeof(bufattach) );
				bufattach.size	= sizeof(bufattach);
				bufattach.iKind	= DCAMBUF_ATTACHKIND_FRAME;
				bufattach.buffer= pFrames;
				bufattach.buffercount	= number_of_buffer;

				// attach user buffer
				err = dcambuf_attach( hdcam, &bufattach );
				if( failed(err) )
				{
					dcamcon_show_dcamerr( hdcam, err, "dcambuf_attach()" );
					ret = 1;
				}
				else
				{
					// start_capture
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
						sample_wait_abort_and_calc( hdcam, hwait, pFrames, number_of_buffer );

						// stop capture
						dcamcap_stop( hdcam );
						printf( "Stop Capture\n" );
					}

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
