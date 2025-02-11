/**
 @file propertylist.cpp
 @date 2023-12-06

 @copyright Copyright (C) 2017-2024 Hamamatsu Photonics K.K.. All rights reserved.

 @brief		Sample code to get the list of properties that the camera supports.
 @details	This program shows the list of properties that the camera supports. The shown information is added by the directives.
 @remark	dcamprop_getnextid
 @remark	dcamprop_getname
 @remark	dcamprop_getattr
 @remark	dcamprop_getvaluetext
 @remark	dcamprop_queryvalue
 */

#include "../misc/console4.h"
#include "../misc/common.h"

/**
 @def	SHOW_PROPERTY_ATTRIBUTE
 *
 *0:	Not show the attribute information.
 *
 *1:	Show the attribute information.
 */
#define SHOW_PROPERTY_ATTRIBUTE			0

/**
 @def	SHOW_PROPERTY_MODEVALUELIST
 *
 *0:	Not show the string list of value that is mode type.
 *
 *1:	Show the string list of value that is mode type.
 */
#define SHOW_PROPERTY_MODEVALUELIST		0

/**
 @def	SHOW_PROPERTY_ARRAYELEMENT
 *
 *0:	Show the base property only when the attribute has DCAMPROP_ATTR2_ARRAYBASE.
 *
 *1:	Show the valid element of array property when the property is array property.\n
 *		Get the property ID of element property from iProp_NumberOfElement and iPropStep_Element of attributes.
 *
 *2:	Show the all element of array property when the property is array property.\n
 *		Get the property ID of element property by dcamprop_getnextid(). Show all elemet properties that the camera supports.
 */
#define SHOW_PROPERTY_ARRAYELEMENT		0

/**
 @brief	Show attribute flag by one line sentence.
 @param	count	attribute index
 @param name	attribute string
 */
void printf_attr( int32& count, const char* name )
{
	if( count == 0 )
		printf( "%s", name );
	else
		printf( " | %s", name );

	count++;
}

/**
 @brief	Show attribute information.
 @param	propattr	shown DCAMPROP_ATTR structure
 @param	bElement	add indent if bElement is TRUE
 */
void dcamcon_show_propertyattr( DCAMPROP_ATTR propattr, BOOL bElement = FALSE )
{
	int32 count = 0;

	char indent[ 8 ];
	memset( indent, 0, sizeof(indent) );

	if( bElement )
		strcpy_s( indent, sizeof(indent), "\t\t" );

	printf( "%sATTR:\t", indent );

	// attribute
	if( propattr.attribute & DCAMPROP_ATTR_WRITABLE )				printf_attr( count, "WRITABLE" );
	if( propattr.attribute & DCAMPROP_ATTR_READABLE )				printf_attr( count, "READABLE" );
	if( propattr.attribute & DCAMPROP_ATTR_DATASTREAM )				printf_attr( count, "DATASTREAM" );
	if( propattr.attribute & DCAMPROP_ATTR_ACCESSREADY )			printf_attr( count, "ACCESSREADY" );
	if( propattr.attribute & DCAMPROP_ATTR_ACCESSBUSY )				printf_attr( count, "ACCESSBUSY" );
	if( propattr.attribute & DCAMPROP_ATTR_HASVIEW )				printf_attr( count, "HASVIEW" );
	if( propattr.attribute & DCAMPROP_ATTR_HASCHANNEL )				printf_attr( count, "HASCHANNEL" );
	if( propattr.attribute & DCAMPROP_ATTR_HASRATIO )				printf_attr( count, "HASRATIO" );
	if( propattr.attribute & DCAMPROP_ATTR_VOLATILE )				printf_attr( count, "VOLATILE" );
	if( propattr.attribute & DCAMPROP_ATTR_AUTOROUNDING )			printf_attr( count, "AUTOROUNDING" );
	if( propattr.attribute & DCAMPROP_ATTR_STEPPING_INCONSISTENT )	printf_attr( count, "STEPPING_INCONSISTENT" );

	// attribute2
	if( propattr.attribute2 & DCAMPROP_ATTR2_ARRAYBASE )			printf_attr( count, "ARRAYBASE" );
	if( propattr.attribute2 & DCAMPROP_ATTR2_ARRAYELEMENT )			printf_attr( count, "ARRAYELEMENT" );

	if( count == 0 )	printf( "none" );
	printf( "\n" );

	// mode
	switch( propattr.attribute & DCAMPROP_TYPE_MASK )
	{
	case DCAMPROP_TYPE_MODE:	printf( "%sTYPE:\tMODE\n", indent );	break;
	case DCAMPROP_TYPE_LONG:	printf( "%sTYPE:\tLONG\n", indent );	break;
	case DCAMPROP_TYPE_REAL:	printf( "%sTYPE:\tREAL\n", indent );	break;
	default:					printf( "%sTYPE:\tNONE\n", indent );	break;
	}

	// range
	if( propattr.attribute & DCAMPROP_ATTR_HASRANGE )
	{
		printf( "%smin:\t%f\n", indent, propattr.valuemin );
		printf( "%smax:\t%f\n", indent, propattr.valuemax );
	}
	// step
	if( propattr.attribute & DCAMPROP_ATTR_HASSTEP )
	{
		printf( "%sstep:\t%f\n", indent, propattr.valuestep );
	}
	// default
	if( propattr.attribute & DCAMPROP_ATTR_HASDEFAULT )
	{
		printf( "%sdefault:\t%f\n", indent, propattr.valuedefault );
	}
}

/**
 @brief	Show the string of mode values that the camera supports.
 @param hdcam		DCAM handle
 @param iProp		target property ID
 @param v			minimum value of target property
 @param	bElement	add indent if bElement is TRUE
 */
void dcamcon_show_supportmodevalues( HDCAM hdcam, int32 iProp, double v, BOOL bElement = FALSE )
{
	char indent[ 8 ];
	memset( indent, 0, sizeof(indent) );

	if( bElement )
		strcpy_s( indent, sizeof(indent), "\t\t" );

	printf( "%sSupport:\n", indent );

	DCAMERR err;

	int32 pv_index = 0;

	do
	{
		// get value text
		char	pv_text[ 64 ];

		DCAMPROP_VALUETEXT pvt;
		memset( &pvt, 0, sizeof(pvt) );
		pvt.cbSize		= sizeof(pvt);
		pvt.iProp		= iProp;
		pvt.value		= v;
		pvt.text		= pv_text;
		pvt.textbytes	= sizeof(pv_text);

		pv_index++;
		err = dcamprop_getvaluetext( hdcam, &pvt );
		if( !failed(err) )
		{
			printf( "%s\t%d:\t%s\n", indent, pv_index, pv_text );
		}

		// get next value
		err = dcamprop_queryvalue( hdcam, iProp, &v, DCAMPROP_OPTION_NEXT );
	} while( !failed(err) );
}

/**
 @brief	Show array element information.
 @param hdcam	DCAM handle
 @param attr	attribute parameter of base property
 */
#if SHOW_PROPERTY_ARRAYELEMENT
void dcamcon_show_arrayelement( HDCAM hdcam, DCAMPROP_ATTR basepropattr )
{
	if( !(basepropattr.attribute2 & DCAMPROP_ATTR2_ARRAYBASE) )
		return;

	printf( "Array Element:\n" );

	int32 iPropBase = basepropattr.iProp;

	int32 iProp = 0;

	// get number of array
#if SHOW_PROPERTY_ARRAYELEMENT == 2
	DCAMERR err;
	double v;
	iProp = basepropattr.iProp_NumberOfElement;
	err = dcamprop_getvalue( hdcam, iProp, &v );
	if( !failed(err) )
	{
		int32 nArray = (int32)v;
		printf( "\tNumber of valid element: %d\n", nArray );

		iProp = basepropattr.iProp;
		err = dcamprop_getnextid( hdcam, &iProp, DCAMPROP_OPTION_ARRAYELEMENT );
		if( !failed(err) )
		{
			char text[ 64 ];

			DCAMPROP_ATTR	subpropattr;
			memset( &subpropattr, 0, sizeof(subpropattr) );
			subpropattr.cbSize = sizeof(subpropattr);

			do
			{
				err = dcamprop_getname( hdcam, iProp, text, sizeof(text) );
				if( failed(err) )
				{
					dcamcon_show_dcamerr( hdcam, err, "dcamprop_getname()", "IDPROP:%0x08x", iProp );
					return;
				}

				printf( "\t0x%08x: %s\n", iProp, text );

				subpropattr.iProp = iProp;
				err = dcamprop_getattr( hdcam, &subpropattr );
				if( !failed(err) )
				{
					dcamcon_show_propertyattr( subpropattr, TRUE );

#if SHOW_PROPERTY_MODEVALUELIST
					// show mode value list of property
					if( (subpropattr.attribute & DCAMPROP_TYPE_MASK) == DCAMPROP_TYPE_MODE )
						dcamcon_show_supportmodevalues( hdcam, subpropattr.iProp, subpropattr.valuemin, TRUE );
#endif
				}

				err = dcamprop_getnextid( hdcam, &iProp, DCAMPROP_OPTION_ARRAYELEMENT );

			} while( !failed(err) );
		}
	}
#else // SHOW_PROPERTY_ARRAYELEMENT != 2
	DCAMERR err;
	double v;
	iProp = basepropattr.iProp_NumberOfElement;
	err = dcamprop_getvalue( hdcam, iProp, &v );
	if( !failed(err) )
	{
		int32 nArray = (int32)v;
		printf( "\tNumber of valid element: %d\n", nArray );

		DCAMPROP_ATTR	subpropattr;
		memset( &subpropattr, 0, sizeof(subpropattr) );
		subpropattr.cbSize = sizeof(subpropattr);

		int i;
		for( i = 1; i < nArray; i++ )
		{
			char	text[ 64 ];

			// get property name of array element
			int iSubProp = basepropattr.iProp + i * basepropattr.iPropStep_Element;
			err = dcamprop_getname( hdcam, iSubProp, text, sizeof(text) );
			if( failed(err) )
			{
				dcamcon_show_dcamerr( hdcam, err, "dcamprop_getname()", "IDPROP:%0x08x", iSubProp );
				return;
			}

			printf( "\t0x%08x: %s\n", iSubProp, text );

			subpropattr.iProp = iSubProp;
			err = dcamprop_getattr( hdcam, &subpropattr );
			if( !failed(err) )
			{
				dcamcon_show_propertyattr( subpropattr, TRUE );

#if SHOW_PROPERTY_MODEVALUELIST
				// show mode value list of property
				if( (subpropattr.attribute & DCAMPROP_TYPE_MASK) == DCAMPROP_TYPE_MODE )
					dcamcon_show_supportmodevalues( hdcam, subpropattr.iProp, subpropattr.valuemin, TRUE );
#endif
			}
		}
	}
#endif
}
#endif // SHOW_PROPERTY_ARRAYELEMENT

/**
 @brief	Show list of properties that the camera supports. Detail information is shown by the directives.
 @param hdcam	DCAM handle
 */
void dcamcon_show_property_list( HDCAM hdcam )
{
	printf( "\nShow Property List( ID: name" );
#if SHOW_PROPERTY_ATTRIBUTE
	printf( "\n\t-attribute" );
#endif
#if SHOW_PROPERTY_MODEVALUELIST
	printf( "\n\t-mode value list" );
#endif
#if SHOW_PROPERTY_ARRAYELEMENT
	printf( "\n\t-array element" );
#endif
	printf( " )\n" );

	int32	iProp = 0;	// property IDs

	DCAMERR err;
	err = dcamprop_getnextid( hdcam, &iProp, DCAMPROP_OPTION_SUPPORT );
	if( failed(err) )
	{
		dcamcon_show_dcamerr( hdcam, err, "dcamprop_getnextid()", "IDPROP:0x%08x, OPTION:SUPPORT", 0 );
		return;
	}

	do
	{
		// get property name
		char	text[ 64 ];
		err = dcamprop_getname( hdcam, iProp, text, sizeof(text) );
		if( failed(err) )
		{
			dcamcon_show_dcamerr( hdcam, err, "dcamprop_getname()", "IDPROP:0x%08x", iProp );
			return;
		}

		printf( "0x%08x: %s\n", iProp, text );

		// get property attribute
		DCAMPROP_ATTR	basepropattr;
		memset( &basepropattr, 0, sizeof(basepropattr) );
		basepropattr.cbSize	= sizeof(basepropattr);
		basepropattr.iProp	= iProp;

		err = dcamprop_getattr( hdcam, &basepropattr );
		if( !failed(err) )
		{
#if SHOW_PROPERTY_ATTRIBUTE
			// show property attribute
			dcamcon_show_propertyattr( basepropattr );
#endif

#if SHOW_PROPERTY_MODEVALUELIST
			// show mode value list of property
			if( (basepropattr.attribute & DCAMPROP_TYPE_MASK) == DCAMPROP_TYPE_MODE )
				dcamcon_show_supportmodevalues( hdcam, iProp, basepropattr.valuemin );
#endif

#if SHOW_PROPERTY_ARRAYELEMENT
			// show array element
			if( basepropattr.attribute2 & DCAMPROP_ATTR2_ARRAYBASE )
				dcamcon_show_arrayelement( hdcam, basepropattr );
#endif
		}

		// get next property id
		err = dcamprop_getnextid( hdcam, &iProp, DCAMPROP_OPTION_SUPPORT );
		if( failed(err) )
		{
			// no more supported property id
			return;
		}

	} while( iProp != 0 );
}

int main( int argc, char* const argv[] )
{
	printf( "PROGRAM START\n" );

	int	ret = 0;

	// initialize DCAM-API and open device
	HDCAM hdcam;
	hdcam = dcamcon_init_open();
	if ( hdcam == NULL )
	{
		// failed open DCAM handle
		ret = 1;
	}
	else
	{
		// show device information
		dcamcon_show_dcamdev_info(hdcam);

		// show all property list that the camera supports. 
		dcamcon_show_property_list(hdcam);

		// close device
		dcamdev_close(hdcam);
	}

	// finalize DCAM-API
	dcamapi_uninit();

	printf( "PROGRAM END\n" );
	return ret;	// 0:Success, Other:Failure
}