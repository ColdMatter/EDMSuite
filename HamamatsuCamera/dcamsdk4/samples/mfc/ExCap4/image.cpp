// image.cpp
//

#include "stdafx.h"
#include "image.h"
#include "bitmap.h"

#include "qMapfile.h"

/////////////////////////////////////////////////////////////////////////////
// class image

image::~image()
{
}

image::image()
{
	int i;
	for( i=0; i<65536; i++ )
	{
		BYTE*p = (BYTE*)&i;

		mono12pl.lut1[i] = ((WORD)(p[1] & 0x0F)) | (((WORD)p[0]) << 4);
		mono12pl.lut2[i] = (((WORD)(p[0] & 0xF0)) >> 4) | ((WORD)p[1] << 4);

		mono12pb.lut1[i] = (WORD)p[0] + (((WORD)(p[1] & 0x0F)) << 8);
		mono12pb.lut2[i] = (((WORD)p[1]) << 4) + (((WORD)(p[0] & 0xF0)) >> 4);
	}
}

BOOL image::release()
{
	delete this;
	return FALSE;
}

BOOL image::allocateframes( long framecount, BOOL bUser )
{
	ASSERT( 0 );	// Derived class should implement.
	return FALSE;
}

BOOL image::freeframes()
{
	ASSERT( 0 );	// Derived class should implement.
	return FALSE;
}

void** image::getframes()
{
	ASSERT( 0 );	// Derived class should implement.
	return NULL;
}

void* image::getframe( long index, int32& rowbytes )
{
	ASSERT( 0 );	// Derived class should implement.
	return NULL;
}

/////////////////////////////////////////////////////////////////////////////
// class imagefile_dcamimg

class imagefile : public image
{
public:
			~imagefile();
			imagefile();

// override from class iamge
public:
			BOOL	get_bitmapinfoheader( BITMAPINFOHEADER& bmih ) const;
			long	copybits( BYTE* dsttopleft, long rowbytes, long iFrame, long srcwidth, long srcheight, long hOffset, long vOffset, const BYTE* lut = NULL );

			long	width() const;
			long	height() const;
			long	colortype() const;
			long	pixelperchannel() const;
	
			long	numberof_frames() const;

			BOOL	is_bitmap_updated();
			void	clear_bitmap_updated();

// operation
public:

// override for derived class 
protected:
	virtual	BOOL	on_load();

protected:

	struct {
		BOOL	bValid;

		long	width;
		long	height;
		long	colortype;	// 1=B/W, 4=RGB, 5=BGR (old 3=RGB)
		BOOL	bmpupdated;

		const BYTE*	topleft;
		long	rowbytes;
		long	bitperchannel;
		long	framecount;
	} var_imagefile;

};

// ////////////////

imagefile::~imagefile()
{
}

imagefile::imagefile()
{
	memset( &var_imagefile, 0, sizeof( var_imagefile ) );
}

BOOL imagefile::on_load()
{
	var_imagefile.bValid = TRUE;
	var_imagefile.bmpupdated = TRUE;

	return TRUE;
}

long imagefile::width() const
{
	if( ! var_imagefile.bValid )
		return 0;
	else
		return var_imagefile.width;
}

long imagefile::height() const
{
	if( ! var_imagefile.bValid )
		return 0;
	else
		return var_imagefile.height;
}

long imagefile::colortype() const
{
	if( ! var_imagefile.bValid )
		return 0;
	else
		return var_imagefile.colortype;
}

long imagefile::pixelperchannel() const
{
	if( ! var_imagefile.bValid )
		return 0;
	else
		return var_imagefile.bitperchannel;
}

long imagefile::numberof_frames() const
{
	if( ! var_imagefile.bValid )
		return 0;
	else
		return var_imagefile.framecount;
}

BOOL imagefile::is_bitmap_updated()
{
	if( ! var_imagefile.bValid )
		return 0;
	else
		return var_imagefile.bmpupdated;
}

void imagefile::clear_bitmap_updated()
{
	var_imagefile.bmpupdated = FALSE;
}


BOOL imagefile::get_bitmapinfoheader( BITMAPINFOHEADER& bmih ) const
{
	if( ! var_imagefile.bValid )
		return FALSE;

	bmih.biWidth		= var_imagefile.width;
    bmih.biHeight		= var_imagefile.height;
    bmih.biPlanes		= 1;
	ASSERT( var_imagefile.colortype == colortype_bw
		||	var_imagefile.colortype == colortype_oldbgr 
		||	var_imagefile.colortype == colortype_oldrgb
		||	var_imagefile.colortype == colortype_bgr 
		||	var_imagefile.colortype == colortype_rgb
		||	var_imagefile.colortype == colortype_bw12l
		||	var_imagefile.colortype == colortype_bw12b );

	if( var_imagefile.colortype == colortype_oldbgr 
	 ||	var_imagefile.colortype == colortype_oldrgb
	 ||	var_imagefile.colortype == colortype_bgr 
	 ||	var_imagefile.colortype == colortype_rgb )
	{
		bmih.biBitCount = 24;
	}
	else
	{
		ASSERT( var_imagefile.colortype == colortype_bw
			 || var_imagefile.colortype == colortype_bw12l
			 ||	var_imagefile.colortype == colortype_bw12b );

		bmih.biBitCount = 8;
	}
    bmih.biCompression	= BI_RGB;
    bmih.biSizeImage	= 0;
    bmih.biXPelsPerMeter= 3780;	// 96dpi
    bmih.biYPelsPerMeter= 3780;	// 96dpi
    bmih.biClrUsed		= ( bmih.biBitCount == 8 ? 256 : 0 );
    bmih.biClrImportant	= bmih.biClrUsed;

	return TRUE;
}

long imagefile::copybits( BYTE* dsttopleft, long dstrowbytes, long iFrame, long srcwidth, long srcheight, long ox, long oy, const BYTE* lut )
{
	if( iFrame < 0 )
		return FALSE;

	load_frame(iFrame);

	if( var_imagefile.colortype == colortype_oldrgb
	 || var_imagefile.colortype == colortype_rgb )
	{
		if( var_imagefile.bitperchannel == 8 )
		{
			return copybits_rgb8( dsttopleft, dstrowbytes
					, (const BYTE*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
		else
		if( lut != NULL )
		{
			return copybits_rgb16( dsttopleft, dstrowbytes, lut
					, (const WORD*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
		else
		{
			ASSERT( var_imagefile.bitperchannel > 8 );

			return copybits_rgb16( dsttopleft, dstrowbytes, var_imagefile.bitperchannel - 8
					, (const WORD*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
	}
	else
	if( var_imagefile.colortype == colortype_oldbgr
	 || var_imagefile.colortype == colortype_bgr )
	{
		if( var_imagefile.bitperchannel == 8 )
		{
			return copybits_bgr8( dsttopleft, dstrowbytes
					, (const BYTE*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
		else
		if( lut != NULL )
		{
			return copybits_bgr16( dsttopleft, dstrowbytes, lut
					, (const WORD*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
		else
		{
			ASSERT( var_imagefile.bitperchannel > 8 );

			return copybits_bgr16( dsttopleft, dstrowbytes, var_imagefile.bitperchannel - 8
					, (const WORD*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
	}
	else
	if( var_imagefile.colortype == colortype_bw12b
	 || var_imagefile.colortype == colortype_bw12l )
	{
		ASSERT( var_imagefile.bitperchannel == 12 );

		WORD*	mono12plut1 = NULL;
		WORD*	mono12plut2 = NULL;

		if( var_imagefile.colortype == colortype_bw12b )
		{
			mono12plut1 = mono12pb.lut1;
			mono12plut2 = mono12pb.lut2;
		}
		else
		{
			ASSERT( var_imagefile.colortype == colortype_bw12l );
			mono12plut1 = mono12pl.lut1;
			mono12plut2 = mono12pl.lut2;
		}

		if( lut != NULL )
		{
			return copybits_bw12p( dsttopleft, dstrowbytes, lut
						, (const WORD*)var_imagefile.topleft, var_imagefile.rowbytes
						, ox, oy, srcwidth, srcheight, mono12plut1, mono12plut2 );
		}
		else
		{
			return copybits_bw12p( dsttopleft, dstrowbytes, var_imagefile.bitperchannel - 8
					, (const WORD*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight, mono12plut1, mono12plut2 );
		}
	}
	else
	{
		if( var_imagefile.bitperchannel == 8 )
		{
			return copybits_bw8( dsttopleft, dstrowbytes
					, (const BYTE*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
		else
		if( lut != NULL )
		{
			return copybits_bw16( dsttopleft, dstrowbytes, lut
					, (const WORD*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
		else
		{
			ASSERT( var_imagefile.bitperchannel > 8 );

			return copybits_bw16( dsttopleft, dstrowbytes, var_imagefile.bitperchannel - 8
					, (const WORD*)var_imagefile.topleft, var_imagefile.rowbytes
					, ox, oy, srcwidth, srcheight );
		}
	}

}

/////////////////////////////////////////////////////////////////////////////
// class imagefile_dcamimg

#include "dcamimg.h"

class imagefile_dcamimg : public imagefile
{
public:
	~imagefile_dcamimg();
	imagefile_dcamimg();

// operation
public:
	BOOL	attach( qMapfile* mapfile );

// override imagefile
public:
	virtual	BOOL	on_load();


public:
	static BOOL	support( const char* hdr, size_t hdrsize );

protected:
	qMapfile*	m_qMapfile;

};

imagefile_dcamimg::~imagefile_dcamimg()
{
	if( m_qMapfile != NULL )
		delete m_qMapfile;
}

imagefile_dcamimg::imagefile_dcamimg()
{
	m_qMapfile = NULL;
}

BOOL imagefile_dcamimg::attach( qMapfile* mapfile )
{
	m_qMapfile = mapfile;

	if( on_load() )
		return TRUE;

	memset( &var_imagefile, 0, sizeof( var_imagefile ) );
	return FALSE;
}

BOOL imagefile_dcamimg::on_load()
{
	size_t	len = 0xFFFFFFFF;
	const char*	top = (const char*)m_qMapfile->lock( len );

	ASSERT( len >= 256 );

	const DCAMIMG_HDRFILE*	hdr   = (const DCAMIMG_HDRFILE*)top;
	const DCAMIMG_HDRBLOCK*	block = (const DCAMIMG_HDRBLOCK*)(hdr + 1 );

	var_imagefile.width		= block->view[ 0 ].width;
	var_imagefile.height		= block->view[ 0 ].height;
	var_imagefile.colortype	= block->view[ 0 ].colortype;
	var_imagefile.rowbytes		= block->view[ 0 ].rowbytes;
	var_imagefile.bitperchannel= block->view[ 0 ].bitsperchannel;
	var_imagefile.framecount = 1;

	ASSERT( block->view[ 0 ].imageoffset_H == 0 );
	var_imagefile.topleft	= (const BYTE*)top + block->view[ 0 ].imageoffset_L;

	return imagefile::on_load();
}

BOOL imagefile_dcamimg::support( const char* hdr, size_t hdrsize )
{
	return strcmp( hdr, DCAMIMG_SIGNATURE ) == 0 && hdrsize >= 256;
}

/////////////////////////////////////////////////////////////////////////////
// class image_dcamimg

image* image::load( LPCTSTR path )
{
	{

		qMapfile*	mapfile = new qMapfile;

		if( mapfile->open( path, qMapfile::flag_readonly ) )
		{
			size_t	len = 256;
			const char*	p = (const char*)mapfile->lock( len );
		
			if( p != NULL )
			{
				imagefile_dcamimg*	ret = NULL;

				if( imagefile_dcamimg::support( p, len ) )
					ret = new imagefile_dcamimg;

				if( ret != NULL )
				{
					if( ret->attach( mapfile ) )
						return ret;

					delete ret;
				}
			}
		}
		delete mapfile;
	}

	
	return NULL;
}

