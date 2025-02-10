/**
 @file recording.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to record image.
 @details	This program records the captured image. How to stop the image is changed by the directive "STOPCAPTURE_BY_ABORTSIGNAL".
 @details	This program outputs metadata if the directive "USE_USERMETADATA" is enable.
 @remarks	dcamcap_record
 @remarks	dcamrec_status
 @remarks	dcamrec_writemetadata
 */

#include "../misc/console4.h"
#include "../misc/common.h"
#include "../misc/qthread.h"

 /**
  @def	USE_USERMETADATA
  *
  *0:	Not output meta data.
  *
  *1:	Output meta data.
  */
#define	USE_USERMETADATA			0

  /**
   @def	USE_USERMETADATA
   *
   *0:	Stop on recorded specified number.
   *
   *1:	Stop by user timing.
   */
#define STOPCAPTURE_BY_ABORTSIGNAL	0

#if USE_USERMETADATA
#define USERMETADATA_FILEBIN	256
#define USERMETADATA_FILETXT	256
#define USERMETADATA_SESSIONBIN	128
#define USERMETADATA_SESSIONTXT	128
#define USERMETADATA_FRAMEBIN	64
#define USERMETADATA_FRAMETXT	64
#else
#define USERMETADATA_FILEBIN	0
#define USERMETADATA_FILETXT	0
#define USERMETADATA_SESSIONBIN	0
#define USERMETADATA_SESSIONTXT	0
#define USERMETADATA_FRAMEBIN	0
#define USERMETADATA_FRAMETXT	0
#endif

/**
 @brief	Get the current recording status and show it.
 @param hdcam	DCAM handle
 @param hrec	DCAMREC handle
 */
void show_recording_status( HDCAM hdcam, HDCAMREC hrec )
{
	DCAMERR err;

	// get recording status
	DCAMREC_STATUS recstatus;
	memset( &recstatus, 0, sizeof(recstatus) );
	recstatus.size	= sizeof(recstatus);
	
	err = dcamrec_status( hrec, &recstatus );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamrec_status()" );
	}
	else
	{
		printf( "flags: 0x%08x, latest index: %06d, miss: %06d, total: %d\n", recstatus.flags, recstatus.currentframe_index, recstatus.missingframe_count, recstatus.totalframecount );
	}
}

/**
 @brief	Write binary metadata to recorded image.
 @param hdcam			DCAM handle
 @param hrec			DCAMREC handle
 @param userdatakind	metadata location
 @param data			buffer stored binary metadata
 @param datasize		buffer size
 @param iFrame			frame to write metadata. used only when set DCAMREC_METADATAOPTION__LOCATION_FRAME to userdatakind.
 */
void write_usermetadata_bin( HDCAM hdcam, HDCAMREC hrec, int32 userdatakind, void* data, int32 datasize, int32 iFrame = 0 )
{
	DCAMERR err;

	// write user meta binary data
	DCAM_USERDATABIN	userbin;
	memset( &userbin, 0, sizeof(userbin) );
	userbin.hdr.size		= sizeof(userbin);
	userbin.hdr.iKind		= DCAMREC_METADATAKIND_USERDATABIN;
	userbin.hdr.option		= userdatakind;						// DCAMREC_METADATAOPTION__LOCATION_*
	if( userdatakind == DCAMREC_METADATAOPTION__LOCATION_FRAME )
		userbin.hdr.iFrame	= iFrame;

	userbin.bin				= data;
	userbin.bin_len			= datasize;

	err = dcamrec_writemetadata( hrec, &userbin.hdr );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamrec_writemetadata()", "KIND:USERDATABIN, OPTION:0x%08x", userdatakind );
	}
}

/**
 @brief	Write text metadata to recorded image.
 @param hdcam			DCAM handle
 @param hrec			DCAMREC handle
 @param userdatakind	metadata location
 @param text			buffer stored text metadata
 @param textsize		buffer size
 @param iFrame			frame to write metadata. used only when set DCAMREC_METADATAOPTION__LOCATION_FRAME to userdatakind.
 */
void write_usermetadata_txt( HDCAM hdcam, HDCAMREC hrec, int32 userdatakind, char* text, int32 textsize, int32 iFrame = 0 )
{
	DCAMERR err;

	// write user meta text data
	DCAM_USERDATATEXT	usertxt;
	memset( &usertxt, 0, sizeof(usertxt) );
	usertxt.hdr.size		= sizeof(usertxt);
	usertxt.hdr.iKind		= DCAMREC_METADATAKIND_USERDATATEXT;
	usertxt.hdr.option		= userdatakind;						// DCAMREC_METADATAOPTION__LOCATION_*
	if( userdatakind == DCAMREC_METADATAOPTION__LOCATION_FRAME )
		usertxt.hdr.iFrame	= iFrame;

	usertxt.text			= text;
	usertxt.text_len		= textsize;

	err = dcamrec_writemetadata( hrec, &usertxt.hdr );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamrec_writemetadata()", "KIND:USERDATATEXT, OPTION:0x%08x", userdatakind );
	}
}

/**
 @brief	Write file metadata to recorded image.
 @param hdcam	DCAM handle
 @param hrec	DCAMREC handle
 @sa	write_usermetadata_bin, write_usermetadata_txt
 */
void write_file_metadata( HDCAM hdcam, HDCAMREC hrec )
{
	// it it possible to write until dcamrec_close()

	if( USERMETADATA_FILEBIN > 0 )
	{
		char* bin = new char[ USERMETADATA_FILEBIN ];
		memset( bin, 0, USERMETADATA_FILEBIN );
				
		// set user data to bin
		// ...

		write_usermetadata_bin( hdcam, hrec, DCAMREC_METADATAOPTION__LOCATION_FILE, bin, USERMETADATA_FILEBIN );

		delete bin;
	}
				
	if( USERMETADATA_FILETXT > 0 )
	{
		char* txt = new char[ USERMETADATA_FILETXT ];
		memset(txt, 0, USERMETADATA_FILETXT);

		// set user text to txt
		// ...

		write_usermetadata_txt( hdcam, hrec, DCAMREC_METADATAOPTION__LOCATION_FILE, txt, USERMETADATA_FILETXT );

		delete txt;
	}
}

/**
 @brief	Write session metadata to recorded image.
 @param hdcam	DCAM handle
 @param hrec	DCAMREC handle
 @sa	write_usermetadata_bin, write_usermetadata_txt
 */
void write_session_metadata( HDCAM hdcam, HDCAMREC hrec )
{
	// it it possible to write until next dcamcap_record() or dcamrec_close()

	if( USERMETADATA_SESSIONBIN > 0 )
	{
		char* bin = new char[ USERMETADATA_SESSIONBIN ];
		memset( bin, 0, USERMETADATA_SESSIONBIN );
				
		// set user data to bin
		// ...

		write_usermetadata_bin( hdcam, hrec, DCAMREC_METADATAOPTION__LOCATION_SESSION, bin, USERMETADATA_SESSIONBIN );

		delete bin;
	}
				
	if( USERMETADATA_SESSIONTXT > 0 )
	{
		char* txt = new char[ USERMETADATA_SESSIONTXT ];
		memset( txt, 0, USERMETADATA_SESSIONTXT );
				
		// set user text to txt
		// ...

		write_usermetadata_txt( hdcam, hrec, DCAMREC_METADATAOPTION__LOCATION_SESSION, txt, USERMETADATA_SESSIONTXT );
	}
}

/**
 @brief	Write frame metadata to recorded image.
 @param hdcam	DCAM handle
 @param hrec	DCAMREC handle
 @sa	write_usermetadata_bin, write_usermetadata_txt
 */
void write_frame_metadata( HDCAM hdcam, HDCAMREC hrec )
{
	// it it possible to write until next dcamcap_record() or dcamrec_close()

	DCAMERR err;

	// get recording status
	DCAMREC_STATUS recstatus;
	memset( &recstatus, 0, sizeof(recstatus) );
	recstatus.size	= sizeof(recstatus);
	
	err = dcamrec_status( hrec, &recstatus );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamrec_status()" );
		return;
	}

	if( recstatus.currentframe_index < 0 )
	{
		// not record image
		return;
	}

	int iFrame;
	if( USERMETADATA_FRAMEBIN > 0 )
	{
		char* bin = new char[ USERMETADATA_FRAMEBIN ];

		for( iFrame = 0; iFrame <= recstatus.currentframe_index; iFrame++ )
		{
			memset( bin, 0, USERMETADATA_FRAMEBIN );
				
			// set user data to bin
			// ...

			write_usermetadata_bin( hdcam, hrec, DCAMREC_METADATAOPTION__LOCATION_FRAME, bin, USERMETADATA_FRAMEBIN, iFrame );
		}
	}

	if( USERMETADATA_FRAMETXT > 0 )
	{
		char* txt = new char[ USERMETADATA_FRAMETXT ];

		for( iFrame = 0; iFrame <= recstatus.currentframe_index; iFrame++ )
		{
			memset( txt, 0, USERMETADATA_FRAMETXT );
				
			// set user text to txt
			// ...

			write_usermetadata_txt( hdcam, hrec, DCAMREC_METADATAOPTION__LOCATION_FRAME, txt, (int32)strlen(txt), iFrame );
		}
	}
}

/*! @class my_thread
    @brief Show DCAMREC status information until dcamwait_start() return DCAMERR_ABORT.
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
		/*! DCAMREC handle */
		HDCAMREC	m_hrec;
};

/**
 @brief	Define initial values.
*/
my_thread::my_thread()
{
	m_hdcam = NULL;
	m_hwait = NULL;
	m_hrec = NULL;
}

/**
 @brief	Thread process to start capturing images and show recording status.
 @return	result of thread process
 */
int32 my_thread::main()
{
	if( m_hdcam == NULL || m_hwait == NULL || m_hrec == NULL )
		return 0;

	DCAMERR err;

	// wait start param
	DCAMWAIT_START	waitstart;
	memset( &waitstart, 0, sizeof(waitstart) );
	waitstart.size		= sizeof(waitstart);
	waitstart.eventmask	= DCAMWAIT_CAPEVENT_FRAMEREADY;
	waitstart.timeout	= 1000;

	while( 1 )
	{
		err = dcamwait_start( m_hwait, &waitstart );
		if( failed(err) )
		{
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

		show_recording_status( m_hdcam, m_hrec );
	}

	show_recording_status( m_hdcam, m_hrec );

	return 0;
}

/**
 @brief	Sample used to record images and write metadata.
 @param hdcam	DCAM handle
 @param hwait	DCAMWAIT handle
 @sa	write_file_metadata, write_session_metadata, write_frame_metadata, show_recording_status
 */
void sample_recording( HDCAM hdcam, HDCAMWAIT hwait )
{
	DCAMERR err;

	double v;

	int32 record_max_framecount = 1000;

	// calculate max file size
	{
		int32 fixedfile, fixedsession, fixedframe;
		dcamprop_getvalue( hdcam, DCAM_IDPROP_RECORDFIXEDBYTES_PERFILE, &v );		// should be success
		fixedfile = (int32)v;

		dcamprop_getvalue( hdcam, DCAM_IDPROP_RECORDFIXEDBYTES_PERSESSION, &v );	// should be success
		fixedsession = (int32)v;

		dcamprop_getvalue( hdcam, DCAM_IDPROP_RECORDFIXEDBYTES_PERFRAME, &v );		// should be success
		fixedframe = (int32)v;

		LONGLONG max_filesize = (LONGLONG)(fixedfile + USERMETADATA_FILEBIN + USERMETADATA_FILETXT)								// file
							  + (LONGLONG)(fixedsession + USERMETADATA_SESSIONBIN + USERMETADATA_SESSIONTXT)					// session
							  + (LONGLONG)(fixedframe + USERMETADATA_FRAMEBIN + USERMETADATA_FRAMETXT) * record_max_framecount;	// frames
	}

	// create file
	DCAMREC_OPEN	recopen;
	memset( &recopen, 0, sizeof(recopen) );
	recopen.size	= sizeof(recopen);
	recopen.path	= _T("testrec");		// it should set new file name.
	recopen.ext		= _T("dcimg");
	recopen.maxframepersession	= record_max_framecount;
	recopen.userdatasize_file	= USERMETADATA_FILEBIN;
	recopen.usertextsize_file	= USERMETADATA_FILETXT;
	recopen.userdatasize_session= USERMETADATA_SESSIONBIN;
	recopen.usertextsize_session= USERMETADATA_SESSIONTXT;
	recopen.userdatasize		= USERMETADATA_FRAMEBIN;
	recopen.usertextsize		= USERMETADATA_FRAMETXT;

	err = dcamrec_open( &recopen );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamrec_open()" );
	}
	else
	{
		HDCAMREC hrec = recopen.hrec;

		// attach recording handle to DCAM handle
		err = dcamcap_record( hdcam, hrec );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcamcap_record()" );
		}
		else
		{
#if USE_USERMETADATA
			// file meta data
			write_file_metadata( hdcam, hrec );			// modify each data in this function

			// session meta data
			write_session_metadata( hdcam, hrec );		// modify each data in this function
#endif

			// start to capture and record images
			err = dcamcap_start( hdcam, DCAMCAP_START_SEQUENCE );
			if( failed(err) )
			{
				dcamcon_show_dcamerr( hdcam, err, "dcamcap_start()" );
			}
			else
			{
#if STOPCAPTURE_BY_ABORTSIGNAL
				my_thread	thread;
				thread.m_hdcam	= hdcam;
				thread.m_hwait	= hwait;
				thread.m_hrec	= hrec;

				printf( "Hit Enter key to stop capturing\n" );

				// start thread to check wait event and recording status
				thread.start();

				// wait user input
				getchar();

				// abort signal to dcamwait_start
				dcamwait_abort( hwait );

				thread.wait_terminate();
#else
				DCAMWAIT_START	waitstart;
				memset( &waitstart, 0, sizeof(waitstart) );
				waitstart.size		= sizeof(waitstart);
				waitstart.eventmask	= DCAMWAIT_RECEVENT_MISSED
									| DCAMWAIT_RECEVENT_STOPPED;
				waitstart.timeout	= 1000;

				BOOL bStop = FALSE;
				while( !bStop )
				{
					err = dcamwait_start( hwait, &waitstart );

					if( !failed(err) )
					{
						if( waitstart.eventhappened & DCAMWAIT_RECEVENT_STOPPED )
						{
							// record maximum frame
							bStop = TRUE;
						}
						else
						if( waitstart.eventhappened & DCAMWAIT_RECEVENT_MISSED )
						{
							// miss to record frame
							// ...
						}
					}
					
					show_recording_status( hdcam, hrec );
				}
#endif

				dcamcap_stop( hdcam );

#if USE_USERMETADATA
				// write user meta data
				write_frame_metadata( hdcam, hrec );	// modify each data in this function
#endif
			}
		}

		// close file
		dcamrec_close( hrec );
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
			int32 number_of_buffer = 100;
			err = dcambuf_alloc( hdcam, number_of_buffer );
			if( failed(err) )
			{
				dcamcon_show_dcamerr( hdcam, err, "dcambuf_alloc()" );
				ret = 1;
			}
			else
			{	
				// start recording
				printf( "\nStart Recording\n" );
				sample_recording( hdcam, hwait );
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
