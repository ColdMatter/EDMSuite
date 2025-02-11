// cuda_live_attach.cpp : Defines the entry point for the console application.
//

#include "../misc/console4.h"
#include "../misc/common.h"

extern "C"	double	calc_average_gpu( const void* buf, int32 rowbytes, int32 width, int32 height );
extern "C"	BOOL	allocBuffer( void** buf, int32 bufsize );
extern "C"	void	releaseBuffer( void* buf );

boolean my_dcamprop_getvalue( HDCAM hdcam, int32 idprop, int32& lValue, const char* propname )
{
	DCAMERR	err;
	double	value;
	err = dcamprop_getvalue( hdcam, idprop, &value );
	if( failed( err ) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamprop_getvalue()", "IDPROP:%s", propname );
		return false;
	}

	lValue = (int32)value;
	return true;
}

void sample_wait_and_calc( HDCAM hdcam, HDCAMWAIT hwait, void** buffer )
{
	int32	topoffset, rowbytes, pixeltype, width, height;
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

	// this sample only supports B/W 16
	if( (DCAM_PIXELTYPE)pixeltype != DCAM_PIXELTYPE_MONO16 )
	{
		return;
	}

	// wait start param
	DCAMWAIT_START	waitstart;
	memset( &waitstart, 0, sizeof(waitstart) );
	waitstart.size		= sizeof(waitstart);
	waitstart.eventmask	= DCAMWAIT_CAPEVENT_FRAMEREADY;
	waitstart.timeout	= 1000;
	
	// transferinfo param
	DCAMCAP_TRANSFERINFO	transinfo;
	memset( &transinfo, 0, sizeof(transinfo) );
	transinfo.size	= sizeof(transinfo);

	int		i, number_of_test = 100;
	for( i = 0; i < number_of_test; i++ )
	{
		DCAMERR	err;

		// wait image
		err = dcamwait_start( hwait, &waitstart );
		if( failed( err ) )
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
		err = dcamcap_transferinfo( hdcam, &transinfo );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcamcap_transferinfo()" );
			continue;
		}

		// calculate average of latest image
		char*	buf = (char*)buffer[ transinfo.nNewestFrameIndex ];
		buf += topoffset;
		double	v = calc_average_gpu( buf, rowbytes, width, height );
		printf( "%g\n", v );
	}
}	

int main(int argc, char* const argv[])
{
	printf( "PROGRAM START\n" );

	int	ret = 0;

	DCAMERR	err;

	// initialize DCAM-API and open device
	HDCAM	hdcam;
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
		waitopen.size = sizeof(waitopen);
		waitopen.hdcam	= hdcam;

		err = dcamwait_open( &waitopen );
		if( failed( err ) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcamwait_open()" );
			ret = 1;
		}
		else
		{
			HDCAMWAIT	hwait = waitopen.hwait;

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

				void* src;

				int		i;
				for( i = 0; i < number_of_buffer; i++ )
				{
					allocBuffer( &src, bufframebytes );
					pFrames[i] = src;
				}

				DCAMBUF_ATTACH	bufattach;
				memset( &bufattach, 0, sizeof(bufattach) );
				bufattach.size		= sizeof(bufattach);
				bufattach.iKind	= DCAMBUF_ATTACHKIND_FRAME;
				bufattach.buffer		= pFrames;
				bufattach.buffercount	= number_of_buffer;

				// attach user buffer
				err = dcambuf_attach( hdcam, &bufattach );
				if( failed( err ) )
				{
					dcamcon_show_dcamerr( hdcam, err, "dcambuf_attach()" );
					ret = 1;
				}
				else
				{
					// start_capture
					err = dcamcap_start( hdcam, DCAMCAP_START_SEQUENCE );
					if( failed( err ) )
					{
						dcamcon_show_dcamerr( hdcam, err, "dcamcap_start()" );
						ret = 1;
					}
					else
					{
						printf( "\nStart Capture\n" );

						// wait image and calculate average
						sample_wait_and_calc( hdcam, hwait, pFrames );

						// stop capture
						dcamcap_stop( hdcam );
						printf( "Stop Capture\n" );
					}

					dcambuf_release( hdcam );
				}

				// free buffer
				for( i = 0; i < number_of_buffer; i++ )
				{
					releaseBuffer( pFrames[i] );
				}
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
    return ret;
}
