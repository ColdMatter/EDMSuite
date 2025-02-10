/**
 @file software_trigger.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to captures image when software trigger is inputted.
 @details	This program captures image when a software trigger is inputted.
 @details	This program accesses the captured image.
 @remarks	dcamcap_firetrigger
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
	m_hdcam = NULL;
	m_hwait	= NULL;
}

/**
 @brief	Thread process to wait the captured image and calculate the average of image.
 @return	result of thread process
 @sa	calc_average
 */
int32 my_thread::main()
{
	if( m_hdcam == NULL || m_hwait == NULL )
		return 0;

	DCAMERR err;

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
		// wait image
		err = dcamwait_start( m_hwait, &waitstart );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( m_hdcam, err, "dcamwait_start()" );
			if( err == DCAMERR_ABORT )
			{
				// receive abort signal
				break;
			}
			else
			{
				continue;
			}
		}

		// access image
		err = dcambuf_lockframe( m_hdcam, &bufframe );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( m_hdcam, err, "dcambuf_lockframe()" );
			continue;
		}

		// a frame has come
		double v = calc_average( bufframe.buf, bufframe.rowbytes, bufframe.type, bufframe.width, bufframe.height );
		printf( "%g\n", v );
	}

	return 0;
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

		// set software trigger mode
		err = dcamprop_setvalue( hdcam, DCAM_IDPROP_TRIGGERSOURCE, DCAMPROP_TRIGGERSOURCE__SOFTWARE );
		if( failed(err) )
			dcamcon_show_dcamerr( hdcam, err, "dcamprop_setvalue()", "IDPROP:TRIGGERSOURCE, VALUE:SOFTWARE" );
		else
		{
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

						// make thread to wait image and calculate average
						my_thread	thread;
						thread.m_hdcam	= hdcam;
						thread.m_hwait	= hwait;

						printf( "Hit Enter key to capture frame\n" );

						// start thread
						thread.start();

						char buf[ 256 ];
						while( fgets( buf, sizeof(buf), stdin ) != NULL )
						{
							if( _stricmp( buf, "exit" ) == 0 || _stricmp( buf, "exit\n" ) == 0
							 || _stricmp( buf, "quit" ) == 0 || _stricmp( buf, "quit\n" ) == 0 )
								break;

							dcamcap_firetrigger( hdcam );
						}

						// abort signal to dcamwait_start
						dcamwait_abort( hwait );

						thread.wait_terminate();

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
		}

		// close DCAM handle
		dcamdev_close( hdcam );
	}

	// finalize DCAM-API
	dcamapi_uninit();

	printf( "PROGRAM END\n" );
	return ret;	// 0:Success, Other:Failure
}