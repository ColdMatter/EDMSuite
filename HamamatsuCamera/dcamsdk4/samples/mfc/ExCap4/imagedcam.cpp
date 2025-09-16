// imagedcam.cpp
//

#include "stdafx.h"
#include "dcamex.h"
#include "image.h"

#include "bitmap.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

template <class T>
inline BOOL myalloc( T*& p, size_t count )
{
	p = (T*)malloc( sizeof( T ) * count );
	return p != NULL;
}

inline void myfree( void* p )
{
	free( p );
}

/////////////////////////////////////////////////////////////////////////////
// imagedcam

class imagedcam : public image
{
public:
	~imagedcam();
	imagedcam( HDCAM hdcam );

// override from class iamge
public:
			BOOL	release();	// TRUE means still object is exist.

			BOOL	allocateframes( long framecount, BOOL bUser );
			BOOL	freeframes();
			void**	getframes();
			void*	getframe( long index, int32& rowbytes );
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

#ifdef _EXCAP_SUPPORTS_VIEWS_
			void	set_shown_multiview( long index );
#endif // _EXCAP_SUPPORTS_VIEWS_ !

protected:
			void	check_bmpupdated();
			long	_copybits( BYTE* dsttopleft, long dstrowbytes, const void* src, long srcrowbytes, long src_ox, long src_oy, long srcwidth, long srcheight, const BYTE* lut );

protected:
	struct {
		HDCAM	hdcam;
		BOOL	bmpupdated;
		long	newestFrameIndex;
		long	totalFrameCount;

		char**	chunkbuffers;
		long	nChunkBuffer;

		void**	userframes;
		long	nUserFrame;
#ifdef _EXCAP_SUPPORTS_VIEWS_
		long	idShownMultiview;
#endif // _EXCAP_SUPPORTS_VIEWS_ !
	} var_imagedcam;
};

// ----

imagedcam::~imagedcam()
{
}

imagedcam::imagedcam( HDCAM hdcam )
{
	memset( &var_imagedcam, 0, sizeof( var_imagedcam ) );

	var_imagedcam.hdcam = hdcam;
	var_imagedcam.newestFrameIndex = -1;
}

BOOL imagedcam::release()
{
	freeframes();

	return image::release();
}

BOOL imagedcam::allocateframes( long framecount, BOOL bUser )
{
	freeframes();

	DCAMERR err = DCAMERR_SUCCESS;

	if( ! bUser )
	{
		err = dcambuf_alloc( var_imagedcam.hdcam, framecount );
		if( failed(err) )
			return FALSE;

		var_imagedcam.nUserFrame = framecount;
		return TRUE;
	}

	size_t	dataframebytes;
	{
		double val;
		err = dcamprop_getvalue(var_imagedcam.hdcam, DCAM_IDPROP_IMAGE_FRAMEBYTES, &val);
		if ( failed(err) )
			return FALSE;

		dataframebytes = static_cast<size_t>(val);
	}

	if( myalloc( var_imagedcam.userframes, framecount ) )
	{
		memset( var_imagedcam.userframes, 0, sizeof( *var_imagedcam.userframes ) * framecount );
#ifndef _WIN64
		const int size_chunkblock = 0x04000000;	// 64 MB.
		long	frame_per_chunkblock = (long)( size_chunkblock / dataframebytes );
#else
		long	frame_per_chunkblock = framecount;
#endif
		long	nChunkBuffer = framecount / frame_per_chunkblock;
		if( framecount % frame_per_chunkblock )
			nChunkBuffer++;
		if( myalloc( var_imagedcam.chunkbuffers, nChunkBuffer ) )
		{
			memset( var_imagedcam.chunkbuffers, 0, sizeof( *var_imagedcam.chunkbuffers ) * nChunkBuffer );

			int	i;
			for( i = 0; i < nChunkBuffer; i++ )
			{
				if( ! myalloc( var_imagedcam.chunkbuffers[ i ], dataframebytes * frame_per_chunkblock ) )
					break;
			}

			if( i == nChunkBuffer )
			{
				int	j, k;

				k = 0;
				for( i = 0; i < nChunkBuffer; i++ )
				{
					for( j = 0; j < frame_per_chunkblock; j++ )
					{
						var_imagedcam.userframes[ k++ ] = var_imagedcam.chunkbuffers[ i ] + dataframebytes * j;
						if( k >= framecount )
							break;
					}
				}

				var_imagedcam.nUserFrame = framecount;
				var_imagedcam.nChunkBuffer = nChunkBuffer;

				return TRUE;
			}
		}

		freeframes();
	}

	return FALSE;
}

BOOL imagedcam::freeframes()
{
	if( var_imagedcam.chunkbuffers != NULL )
	{
		int	i;
		for( i = 0; i < var_imagedcam.nChunkBuffer; i++ )
		{
			if( var_imagedcam.chunkbuffers[ i ] != NULL )
			{
				myfree( var_imagedcam.chunkbuffers[ i ] );
			}
		}
		myfree( var_imagedcam.chunkbuffers );
		var_imagedcam.chunkbuffers = NULL;
		var_imagedcam.nChunkBuffer = 0;
	}

	if( var_imagedcam.userframes != NULL )
	{
		myfree( var_imagedcam.userframes );
		var_imagedcam.userframes = NULL;
		var_imagedcam.nUserFrame = 0;
	}

	return TRUE;
}

void** imagedcam::getframes()
{
	return var_imagedcam.userframes;
}


void* imagedcam::getframe( long index, int32& rowbytes )
{
	if( index < 0 || var_imagedcam.nUserFrame <= index )
		return NULL;

	double cx;
	dcamprop_getvalue( var_imagedcam.hdcam, DCAM_IDPROP_IMAGE_WIDTH, &cx);
	
	long	bitperchannel = dcamex_getpropvalue_bitsperchannel( var_imagedcam.hdcam );
	long	colortype = dcamex_getpropvalue_colortype( var_imagedcam.hdcam );

	rowbytes = static_cast<int32>(cx) * ( colortype == colortype_bw ? 1 : 3 ) * int( ( bitperchannel + 7 ) / 8 );
	return var_imagedcam.userframes[ index ];
}

BOOL imagedcam::get_bitmapinfoheader( BITMAPINFOHEADER& bmih ) const
{
	if( var_imagedcam.hdcam == NULL )
		return FALSE;

	bmih.biWidth		= dcamex_getpropvalue_imagewidth( var_imagedcam.hdcam );
    bmih.biHeight		= dcamex_getpropvalue_imageheight( var_imagedcam.hdcam );
    bmih.biPlanes		= 1;
	bmih.biBitCount		= ( dcamex_getpropvalue_colortype( var_imagedcam.hdcam ) == DCAMPROP_COLORTYPE__BW ? 8 : 24 );
    bmih.biCompression	= BI_RGB;
    bmih.biSizeImage	= 0;
    bmih.biXPelsPerMeter= 3780;	// 96dpi
    bmih.biYPelsPerMeter= 3780;	// 96dpi
    bmih.biClrUsed		= ( bmih.biBitCount == 8 ? 256 : 0 );
    bmih.biClrImportant	= bmih.biClrUsed;

#ifdef _EXCAP_SUPPORTS_VIEWS_
	DCAMERR err;

	double v;
	err = dcamprop_getvalue( var_imagedcam.hdcam, DCAM_IDPROP_NUMBEROF_VIEW, &v );
	if( !failed(err) && v > 1 )
	{
		if( var_imagedcam.idShownMultiview == 0 )
			bmih.biHeight *= (long)v;
	}
#endif // _EXCAP_SUPPORTS_VIEWS_ !
	return TRUE;
}

long imagedcam::_copybits( BYTE* dsttopleft, long dstrowbytes, const void* srctopleft, long srcrowbytes, long srcox, long srcoy, long srcwidth, long srcheight, const BYTE* lut )
{
	long	bitperchannel = dcamex_getpropvalue_bitsperchannel( var_imagedcam.hdcam );
	long	colortype = dcamex_getpropvalue_colortype( var_imagedcam.hdcam );
	long	pixeltype = dcamex_getpropvalue_pixeltype( var_imagedcam.hdcam );

	if( colortype == DCAMPROP_COLORTYPE__RGB )
	{
		if( bitperchannel == 8 )
		{
			return copybits_rgb8( dsttopleft, dstrowbytes
					, (const BYTE*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
		}

		if( lut != NULL )
		{
			return copybits_rgb16( dsttopleft, dstrowbytes, lut
					, (const WORD*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
		}

		{
			ASSERT( 8 < bitperchannel && bitperchannel <= 16 );

			long	nShift = bitperchannel - 8;
			return copybits_rgb16( dsttopleft, dstrowbytes, nShift
					, (const WORD*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
		}
	}

	if( colortype == DCAMPROP_COLORTYPE__BGR )
	{
		if( bitperchannel == 8 )
		{
			return copybits_bgr8( dsttopleft, dstrowbytes
					, (const BYTE*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
		}

		if( lut != NULL )
		{
			return copybits_bgr16( dsttopleft, dstrowbytes, lut
					, (const WORD*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
		}

		{
			ASSERT( 8 < bitperchannel && bitperchannel <= 16 );

			long	nShift = bitperchannel - 8;
			return copybits_bgr16( dsttopleft, dstrowbytes, nShift
					, (const WORD*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
		}
	}

#ifdef HPKINTERNALUSE//{{HPKINTERNALUSE
	if( colortype == DCAMPROP_COLORTYPE__YUV422 )
	{
		ASSERT( bitperchannel == 8 );

		return copybits_yuv422( dsttopleft, dstrowbytes
					, (const BYTE*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
	}
#endif //}}HPKINTERNALUSE

	{
		ASSERT( colortype == DCAMPROP_COLORTYPE__BW );

		if( bitperchannel == 8 )
		{
			if (lut != NULL)
			{
				return copybits_bw8(dsttopleft, dstrowbytes, lut
					, (const BYTE*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
			}

			{
				return copybits_bw8(dsttopleft, dstrowbytes
					, (const BYTE*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight);
			}
		}

		if( pixeltype == DCAM_PIXELTYPE_MONO12 )
		{
			if( lut != NULL )
			{
				return copybits_bw12p( dsttopleft, dstrowbytes, lut
						, (const WORD*)srctopleft, srcrowbytes
						, srcox, srcoy, srcwidth, srcheight, mono12pl.lut1, mono12pl.lut2 );
			}

			{
				ASSERT( 8 < bitperchannel && bitperchannel <= 16 );

				long	nShift = bitperchannel - 8;
				return copybits_bw12p( dsttopleft, dstrowbytes, nShift
						, (const WORD*)srctopleft, srcrowbytes
						, srcox, srcoy, srcwidth, srcheight, mono12pl.lut1, mono12pl.lut2 );
			}
		}
		else
		if( pixeltype == DCAM_PIXELTYPE_MONO12P )
		{
			if( lut != NULL )
			{
				return copybits_bw12p( dsttopleft, dstrowbytes, lut
						, (const WORD*)srctopleft, srcrowbytes
						, srcox, srcoy, srcwidth, srcheight, mono12pb.lut1, mono12pb.lut2 );
			}

			{
				ASSERT( 8 < bitperchannel && bitperchannel <= 16 );

				long	nShift = bitperchannel - 8;
				return copybits_bw12p( dsttopleft, dstrowbytes, nShift
						, (const WORD*)srctopleft, srcrowbytes
						, srcox, srcoy, srcwidth, srcheight, mono12pb.lut1, mono12pb.lut2 );
			}
		}
		else
		{
			ASSERT( pixeltype == DCAM_PIXELTYPE_MONO16 );

			if( lut != NULL )
			{
				return copybits_bw16( dsttopleft, dstrowbytes, lut
						, (const WORD*)srctopleft, srcrowbytes
						, srcox, srcoy, srcwidth, srcheight );
			}

			{
				ASSERT( 8 < bitperchannel && bitperchannel <= 16 );

				long	nShift = bitperchannel - 8;
				return copybits_bw16( dsttopleft, dstrowbytes, nShift
						, (const WORD*)srctopleft, srcrowbytes
						, srcox, srcoy, srcwidth, srcheight );
			}
		}
	}
}

long imagedcam::copybits( BYTE* dsttopleft, long dstrowbytes, long iFrame, long dstwidth, long dstheight, long hOffset, long vOffset, const BYTE* lut )
{
	long	ret = 0;

	if( var_imagedcam.hdcam != NULL )
	{
		DCAMERR	err;

		DCAMCAP_TRANSFERINFO	transferinfo;
		memset( &transferinfo, 0, sizeof(transferinfo) );
		transferinfo.size	= sizeof(transferinfo);

		err = dcamcap_transferinfo( var_imagedcam.hdcam, &transferinfo );
		if( !failed(err)
		 && transferinfo.nFrameCount > 0 )
		{
#ifdef _EXCAP_SUPPORTS_FRAMEBUNDLE_
			double	value;
#endif // _EXCAP_SUPPORTS_FRAMEBUNDLE_ !
			double cx, cy;
			dcamprop_getvalue(var_imagedcam.hdcam, DCAM_IDPROP_IMAGE_WIDTH, &cx);
			dcamprop_getvalue(var_imagedcam.hdcam, DCAM_IDPROP_IMAGE_HEIGHT, &cy);

			long imgwidth	= static_cast<long>(cx);
			long imgheight	= static_cast<long>(cy);

			long srcoffset	= 0;
#ifdef _EXCAP_SUPPORTS_VIEWS_
			double v;
			err = dcamprop_getvalue( var_imagedcam.hdcam, DCAM_IDPROP_NUMBEROF_VIEW, &v ); 
			if( !failed(err) && v > 1 )
			{
				if( var_imagedcam.idShownMultiview == 0 )
					imgheight *= static_cast<long>(v);
				else
				{
					err = dcamprop_getvalue( var_imagedcam.hdcam, DCAM_IDPROP_IMAGE_TOPOFFSETBYTES + DCAM_IDPROP__VIEW * var_imagedcam.idShownMultiview, &v );
					if( !failed(err) )
						srcoffset = static_cast<long>(v);
				}
			}
#endif // _EXCAP_SUPPORTS_VIEWS_ !
			long	width = dstwidth;
			if( hOffset + width > imgwidth )
				width = imgwidth - hOffset;
			long	height= dstheight;
			if( vOffset + height > imgheight )
				height = imgheight - vOffset;

			if( var_imagedcam.userframes != NULL )
			{
				if( iFrame == -1 )
				{
					iFrame = transferinfo.nNewestFrameIndex;
				}
				else
				if( iFrame != -1 && var_imagedcam.nUserFrame < transferinfo.nFrameCount )
				{
					iFrame = ( iFrame + transferinfo.nFrameCount ) % var_imagedcam.nUserFrame;
				}

				// use user buffer.
				long	bitperchannel = dcamex_getpropvalue_bitsperchannel( var_imagedcam.hdcam );
				long	colortype = dcamex_getpropvalue_colortype( var_imagedcam.hdcam );
	
				void* buffer = var_imagedcam.userframes[ iFrame ];
				int32 rowbytes = imgwidth * ( colortype == colortype_bw ? 1 : 3 ) * int( ( bitperchannel + 7 ) / 8 );
				
				char* pSrc = (char*)buffer + srcoffset;
				ret = _copybits( dsttopleft, dstrowbytes, pSrc, rowbytes
							, hOffset, vOffset, width, height, lut );
			}
			else
			{
#ifdef _EXCAP_SUPPORTS_FRAMEBUNDLE_
				err = dcamprop_getvalue( var_imagedcam.hdcam, DCAM_IDPROP_FRAMEBUNDLE_MODE, &value );
				if( !failed(err) 
				 && value == DCAMPROP_MODE__ON )
				{
					err = dcamprop_getvalue( var_imagedcam.hdcam, DCAM_IDPROP_FRAMEBUNDLE_ROWBYTES, &value );
					VERIFY( !failed(err) );
					int32	framebundle_rowbytes = (long)value;

					DCAMBUF_FRAME	frame;
					memset( &frame, 0, sizeof(frame) );
					frame.size	= sizeof(frame);
					frame.iFrame= iFrame;		

					err = dcambuf_lockframe( var_imagedcam.hdcam, &frame );
					if( !failed(err) )
					{
						char* pSrc = (char*)frame.buf + srcoffset;
						ret = _copybits( dsttopleft, dstrowbytes, pSrc, framebundle_rowbytes
									, hOffset, vOffset, width, height, lut );

					}
				}
				else
#endif // _EXCAP_SUPPORTS_FRAMEBUNDLE_ !
				{
					DCAMBUF_FRAME	frame;
					memset( &frame, 0, sizeof(frame) );
					frame.size	= sizeof(frame);
					frame.iFrame= iFrame;		

					err = dcambuf_lockframe( var_imagedcam.hdcam, &frame );
					if( !failed(err) )
					{
						char* pSrc = (char*)frame.buf + srcoffset;
						ret = _copybits( dsttopleft, dstrowbytes, pSrc, frame.rowbytes
									, hOffset, vOffset, width, height, lut );
					}
				}
			}
		}
	}

	return ret;
}

long imagedcam::width() const
{
	if( var_imagedcam.hdcam == NULL )
		return 0;
	else
		return dcamex_getpropvalue_imagewidth( var_imagedcam.hdcam );
}

long imagedcam::height() const
{
	if( var_imagedcam.hdcam == NULL )
		return 0;
	else
	{
		long height = dcamex_getpropvalue_imageheight( var_imagedcam.hdcam );

#ifdef _EXCAP_SUPPORTS_VIEWS_
		DCAMERR err;

		double v;
		err = dcamprop_getvalue( var_imagedcam.hdcam, DCAM_IDPROP_NUMBEROF_VIEW, &v );
		if( !failed(err) && v > 1 )
		{
			if( var_imagedcam.idShownMultiview == 0 )
				height *= (long)v;
		}
#endif // _EXCAP_SUPPORTS_VIEWS_ !
		return height;
	}
}

long imagedcam::colortype() const
{
	switch( dcamex_getpropvalue_colortype( var_imagedcam.hdcam ) )
	{
	default:	ASSERT( 0 );
	case DCAMPROP_COLORTYPE__BW:	return image::colortype_bw;
#ifdef HPKINTERNALUSE//{{HPKINTERNALUSE
	case DCAMPROP_COLORTYPE__YUV422:
#endif //}}HPKINTERNALUSE
	case DCAMPROP_COLORTYPE__RGB:	return image::colortype_rgb;
	case DCAMPROP_COLORTYPE__BGR:	return image::colortype_bgr;
	}
}

long imagedcam::pixelperchannel() const
{
	return dcamex_getpropvalue_bitsperchannel( var_imagedcam.hdcam );
}

long imagedcam::numberof_frames() const
{
	return var_imagedcam.nUserFrame;
}

BOOL imagedcam::is_bitmap_updated()
{
	check_bmpupdated();
	return var_imagedcam.bmpupdated;
}

void imagedcam::clear_bitmap_updated()
{
	var_imagedcam.bmpupdated = FALSE;
}

#ifdef _EXCAP_SUPPORTS_VIEWS_
void imagedcam::set_shown_multiview( long index )
{
	var_imagedcam.idShownMultiview = index;
}
#endif // _EXCAP_SUPPORTS_VIEWS_ !
// ----------------

void imagedcam::check_bmpupdated()
{
	if( var_imagedcam.hdcam != NULL )
	{
		DCAMERR err;

		DCAMCAP_TRANSFERINFO	transferinfo;
		memset( &transferinfo, 0, sizeof(transferinfo) );
		transferinfo.size	= sizeof(transferinfo);

		err = dcamcap_transferinfo( var_imagedcam.hdcam, &transferinfo );
		if( !failed(err) )
		{
			if( var_imagedcam.newestFrameIndex	!= transferinfo.nNewestFrameIndex
			 || var_imagedcam.totalFrameCount	!= transferinfo.nFrameCount )
			{
				var_imagedcam.newestFrameIndex	= transferinfo.nNewestFrameIndex;
				var_imagedcam.totalFrameCount	= transferinfo.nFrameCount;
				var_imagedcam.bmpupdated		= TRUE;
			}
		}
	}
}

/////////////////////////////////////////////////////////////////////////////

#include "imagedcam.h"

image* new_imagedcam( HDCAM hdcam )
{
	return new imagedcam( hdcam );
}
