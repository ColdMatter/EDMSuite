// dcamex.cpp
//

#include "stdafx.h"
#include "dcamex.h"

// ----------------------------------------------------------------

long dcamex_getpropvalue_sensormode( HDCAM hdcam )
{
	DCAMERR err;

	double val;
	err = dcamprop_getvalue(hdcam, DCAMPROP_SENSORMODE__AREA, &val);
	if( failed(err) )
		return 0;

	return (static_cast<long>(val));
}

long dcamex_getpropvalue_imagewidth( HDCAM hdcam )
{
	DCAMERR err;

	double val;
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_WIDTH, &val);
	if( failed(err) )
		return 0;

	return (static_cast<long>(val));
}

long dcamex_getpropvalue_imageheight( HDCAM hdcam )
{
	DCAMERR err;

	double val;
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_HEIGHT, &val);
	if( failed(err) )
		return 0;

	return (static_cast<long>(val));
}

long dcamex_getpropvalue_imageframebytes( HDCAM hdcam )
{
	DCAMERR err;

	double val;
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_IMAGE_FRAMEBYTES, &val);
	if( failed(err) )
		return 0;

	return (static_cast<long>(val));
}

long dcamex_getpropvalue_colortype( HDCAM hdcam )
{
	DCAMERR err;

	double val;
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_COLORTYPE, &val);
	if( failed(err) )
		return 0;

	return (static_cast<long>(val));
}

long dcamex_getpropvalue_bitsperchannel( HDCAM hdcam )
{
	DCAMERR err;

	double val;
	err = dcamprop_getvalue(hdcam, DCAM_IDPROP_BITSPERCHANNEL, &val);
	if( failed(err) )
		return 0;

	return (static_cast<long>(val));
}

long dcamex_getpropvalue_pixeltype( HDCAM hdcam )
{
	DCAMERR err;

	double val;
	err = dcamprop_getvalue( hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, &val );
	if( failed(err) )
		return err;

	return (static_cast<long>(val));
}