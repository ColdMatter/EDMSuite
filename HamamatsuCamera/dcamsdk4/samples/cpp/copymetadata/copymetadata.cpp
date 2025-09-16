/**
 @file copymetadata.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to copy metadata.
 @details	This program captures images and copies metadata.
 @details	This program copies timestamps and framestamps as metadata.
 @remarks	dcambuf_copymetadata
 @remarks	dcamdev_getcapability
 */

#include "../misc/console4.h"
#include "../misc/common.h"

/**
 @brief	Copy timestamps.
 @param	hdcam			DCAM handle
 @param iStartFrame		start index of target frame
 @param pStamp		    primary buffer to copy timestamps
 @param nStampCount		number of timestamps to copy
 @return	result of copy timestamps
 */
BOOL copy_metadata_timestamps( HDCAM hdcam, int32 iStartFrame, DCAM_TIMESTAMP* pStamp, int32& nStampCount )
{
	DCAMERR err;

	// copy time stamp
	DCAM_TIMESTAMPBLOCK	tsb;
	memset( &tsb, 0, sizeof(tsb) );
	tsb.hdr.size	= sizeof(tsb);
	tsb.hdr.iKind	= DCAMBUF_METADATAKIND_TIMESTAMPS;
	tsb.hdr.in_count= nStampCount;
	tsb.hdr.iFrame	= iStartFrame;
		
	tsb.timestamps		= pStamp;
	tsb.timestampsize	= sizeof(DCAM_TIMESTAMP);

	err = dcambuf_copymetadata( hdcam, (DCAM_METADATAHDR*)&tsb );
	if( failed(err) )
	{
		nStampCount = 0;
		dcamcon_show_dcamerr( hdcam, err, "dcambuf_copymetadata()", "KIND:TIMESTAMPS" );
		return FALSE;
	}
	
	nStampCount = tsb.hdr.outcount;

	return TRUE;
}

/**
 @brief	Copy framestamps.
 @param	hdcam			DCAM handle
 @param iStartFrame		start index of target frame
 @param pStamp		    number to copy framestamps
 @param nStampCount		number of framestamps to copy
 @return	result of copy framestamps
 */
BOOL copy_metadata_framestamps( HDCAM hdcam, int32 iStartFrame, int32* pStamp, int32& nStampCount )
{
	DCAMERR err;

	// copy frame stamp
	DCAM_FRAMESTAMPBLOCK fsb;
	memset( &fsb, 0, sizeof(fsb) );
	fsb.hdr.size	= sizeof(fsb);
	fsb.hdr.iKind	= DCAMBUF_METADATAKIND_FRAMESTAMPS;
	fsb.hdr.in_count= nStampCount;
	fsb.hdr.iFrame	= iStartFrame;

	fsb.framestamps	= pStamp;
	
	err = dcambuf_copymetadata( hdcam, (DCAM_METADATAHDR*)&fsb );
	if( failed(err) )
	{
		nStampCount = 0;
		dcamcon_show_dcamerr( hdcam, err, "dcambuf_copymetadata()", "KIND:FRAMESTAMPS" );
		return FALSE;
	}

	nStampCount = fsb.hdr.outcount;

	return TRUE;
}

/**
 @brief	Copy and show frame metadata.
 @param	hdcam	DCAM handle
 @param nAlloc	number of allocated buffers
 @sa	copy_metadata_timestamps, copy_metadata_framestamps
 */
void show_framemetadata( HDCAM hdcam, int32 nAlloc )
{
	DCAMERR err;

	// check camera capability
	DCAMDEV_CAPABILITY	devcap;
	memset( &devcap, 0, sizeof(devcap) );
	devcap.size	= sizeof(devcap);

	err = dcamdev_getcapability( hdcam, &devcap );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamdev_getcapability()" );
		return;
	}

	BOOL	bTimestamp	= (devcap.capflag & DCAMDEV_CAPFLAG_TIMESTAMP) ?	TRUE : FALSE;
	BOOL	bFramestamp	= (devcap.capflag & DCAMDEV_CAPFLAG_FRAMESTAMP)	?	TRUE : FALSE;

	if( !bTimestamp && !bFramestamp )
	{
		printf( "camera does not support both timestamp and framestamp.\n" );
		return;
	}

	printf( "Support:" );
	if( bTimestamp )	printf( "\tTimestamp" );
	if( bFramestamp )	printf( "\tFramestamp" );
	printf( "\n" );

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
		printf( "no image.\n" );
		return;
	}

	int32 iStart	= (captransferinfo.nFrameCount <= nAlloc) ? 0 : captransferinfo.nFrameCount - nAlloc;
	int32 nFrame	= captransferinfo.nFrameCount - iStart;

	if( bTimestamp )
	{
		printf( "[TimeStamp]\n" );
		int32 nStampCount = nFrame;
		DCAM_TIMESTAMP* pTimeStamp = new DCAM_TIMESTAMP[ nStampCount ];

		if( copy_metadata_timestamps( hdcam, iStart, pTimeStamp, nStampCount ) )
		{
			int i;
			for( i = 0; i < nStampCount; i++ )
			{
				printf( "%d:\t%d.%06d\n", iStart + i, pTimeStamp[i].sec, pTimeStamp[i].microsec );
			}
		}

		delete pTimeStamp;
		printf( "\n" );
	}

	if( bFramestamp )
	{
		printf( "[FrameStamp]\n" );
		int32 nStampCount = nFrame;
		int32* pFrameStamp = new int32[ nStampCount ];

		if( copy_metadata_framestamps( hdcam, iStart, pFrameStamp, nStampCount ) )
		{
			int i;
			for( i = 0; i < nStampCount; i++ )
			{
				printf( "%d:\t%d\n", iStart + i, pFrameStamp[i] );
			}
		}

		delete pFrameStamp;
		printf( "\n" );
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
					Sleep( 1000 );

					// stop capture
					dcamcap_stop( hdcam );
					printf( "Stop Capture\n" );
				}

				// show frame meta data are time stamp and frame stamp.
				show_framemetadata( hdcam, number_of_buffer );

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