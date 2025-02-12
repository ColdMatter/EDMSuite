/**
 @file live_average.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to access image data and calculate the average of captured image.
 @details	This program accesses the captured image.
 @remarks	dcambuf_attach
 @remarks	dcambuf_release
*/

#include "../misc/console4.h"
#include "../misc/common.h"
#include "../misc/qthread.h"

/**
 @def	STOPCAPTURE_BY_ABORTSIGNAL
 *
 *0:		Stop on captured specified number.
 *
 *1:		Stop by user timing.
 */
#define STOPCAPTURE_BY_ABORTSIGNAL	0

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
	int32	oy = (height - cy) / 2;

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
 @brief	Start capturing the image and calculate the average of image.
 @param hdcam	DCAM handle
 @param hwait	DCAMWAIT handle
 @sa	calc_average
 */
void sample_wait_and_calc( HDCAM hdcam, HDCAMWAIT hwait )
{
	if( hdcam == NULL || hwait == NULL )
		return;

	// wait start param
	DCAMWAIT_START	waitstart;
	memset( &waitstart, 0, sizeof(waitstart) );
	waitstart.size		= sizeof(waitstart);
	waitstart.eventmask	= DCAMWAIT_CAPEVENT_FRAMEREADY;
	waitstart.timeout	= 1000;

	// prepare frame param
	DCAMBUF_FRAME	bufframe;
	memset( &bufframe, 0, sizeof(bufframe) );
	bufframe.size		= sizeof(bufframe);
	bufframe.iFrame		= -1;				// latest frame

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
		
		// access image
		err = dcambuf_lockframe( hdcam, &bufframe );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcambuf_lockframe()" );
			continue;
		}

		// a frame has come
		double v = calc_average( bufframe.buf, bufframe.rowbytes, bufframe.type, bufframe.width, bufframe.height );
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
		int32	main();

public:
		/*! DCAM handle */
		HDCAM		m_hdcam;
		/*! DCAMWAIT handle */
		HDCAMWAIT	m_hwait;
};

/**
 @brief	Define initial values.
*/
my_thread::my_thread()
{
	m_hdcam	= NULL;
	m_hwait	= NULL;
}

/**
 @brief	Thread process to wait the captured image and calculate the average of image.
 @return	result of thread process
 @sa	sample_wait_and_calc
 */
int32 my_thread::main()
{
	sample_wait_and_calc( m_hdcam, m_hwait );

	return 0;
}

/**
 @brief	Start and stop the thread to capture the image and calculate the average of image.
 @param hdcam	DCAM handle
 @param hwait	DCAMWAIT handle
 */
void sample_wait_abort_and_calc( HDCAM hdcam, HDCAMWAIT hwait )
{
	if( hdcam == NULL || hwait == NULL )
		return;

	my_thread	thread;
	thread.m_hdcam	= hdcam;
	thread.m_hwait	= hwait;

	printf( "Hit Enter key to stop capturing\n" );

	// start thread to wait and calculate image
	thread.start();

	// wait user input
	getchar();

	// abort signal to dcamwait_start
	dcamwait_abort( hwait );

	thread.wait_terminate();
}

/**
 @brief	Start capturing images and calculate the average of the specified number of images.
 @param hdcam	DCAM handle
 @param hwait	DCAMWAIT handle
 @param nFrame	number of images acquired
 @sa	calc_average
 */
void sample_wait_and_calc( HDCAM hdcam, HDCAMWAIT hwait, int32 nFrame )
{
	// wait start param
	DCAMWAIT_START	waitstart;
	memset( &waitstart, 0, sizeof(waitstart) );
	waitstart.size		= sizeof(waitstart);
	waitstart.eventmask	= DCAMWAIT_CAPEVENT_FRAMEREADY;
	waitstart.timeout	= 1000;

	// prepare frame param
	DCAMBUF_FRAME	bufframe;
	memset( &bufframe, 0, sizeof(bufframe) );
	bufframe.size		= sizeof(bufframe);
	bufframe.iFrame		= -1;				// latest frame

	int32 i;
	for( i = 0; i < nFrame; i++ )
	{
		DCAMERR err;

		// wait image
		err = dcamwait_start( hwait, &waitstart );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcamwait_start()" );
			continue;
		}

		// access image
		err = dcambuf_lockframe( hdcam, &bufframe );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcambuf_lockframe()" );
			continue;
		}

		// a frame has come
		double v = calc_average( bufframe.buf, bufframe.rowbytes, bufframe.type, bufframe.width, bufframe.height );
		printf( "%g\n", v );
	}
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

					// wait image and calculate average
#if STOPCAPTURE_BY_ABORTSIGNAL
					// stop to capture on hit the key
					sample_wait_abort_and_calc( hdcam, hwait );
#else
					// stop to capture on the specified number
					sample_wait_and_calc( hdcam, hwait, 100 );
#endif

					// stop capture
					dcamcap_stop( hdcam );
					printf( "Stop Capture\n" );
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
