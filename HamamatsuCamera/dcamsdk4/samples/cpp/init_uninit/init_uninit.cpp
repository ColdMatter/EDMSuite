/**
 @file init_uninit.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to init and uninit.
 @details	This program initializes and uninitializes DCAM.
 @remarks	dcamapi_init
 @remarks	dcamapi_uninit
 */

#include "../misc/console4.h"
#include "../misc/common.h"

/**
 @def	USE_INITOPTION
 *
 *0:		Not set DCAMAPI_INITOPTION at dcamapi_init.
 *
 *1:		Set DCAMAPI_INITOPTION at dcamapi_init.\n
 *			Initialize as DCAM latest version.
 */
#define USE_INITOPTION	0

/**
 @def	USE_INITGUID
 *
 *0:		Not set GUID parameter at dcamapi_init.
 *
 *1:		Set GUID parameter at dcamapi_init.
 */
#define USE_INITGUID	0

#if USE_INITGUID
#include "../misc/dcamapix.h"
#endif

int main( int argc, char* const argv[] )
{
	printf( "PROGRAM START\n" );

	int ret = 0;
	DCAMERR err;

	// initialize DCAM-API
	DCAMAPI_INIT	apiinit;
	memset( &apiinit, 0, sizeof(apiinit) );
	apiinit.size	= sizeof(apiinit);

#if USE_INITOPTION
	// set option of initialization
	int32 initoption[] = {
							DCAMAPI_INITOPTION_APIVER__LATEST,
							DCAMAPI_INITOPTION_ENDMARK			// it is necessary to set as the last value.
						 };

	apiinit.initoption		= initoption;
	apiinit.initoptionbytes	= sizeof(initoption);
#endif

#if USE_INITGUID
	// set GUID parameter
	DCAM_GUID	guid = DCAM_GUID_MYAPP;

	apiinit.guid	= &guid;
#endif
	
	err = dcamapi_init( &apiinit );
	if( failed(err) )
	{
		// failed open DCAM handle
		dcamcon_show_dcamerr( NULL, err, "dcamapi_init()" );
		ret = 1;
	}
	else
	{
		int32	nDevice = apiinit.iDeviceCount;
		printf( "dcamapi_init() found %d device(s).\n", nDevice );

		int32 iDevice;
		for( iDevice = 0; iDevice < nDevice; iDevice++ )
		{	
			// show device information
			dcamcon_show_dcamdev_info( (HDCAM)(intptr_t)iDevice );
		}
	}

	// finalize DCAM-API
	dcamapi_uninit();	// recommended call dcamapi_uninit() when dcamapi_init() is called even if it failed.

	printf( "PROGRAM END\n" );
	return ret;	// 0:Success, Other:Failure
}