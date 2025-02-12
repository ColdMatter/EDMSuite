// bitmap.cpp

#include "stdafx.h"
#include "bitmap.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////

long copybits_bw8( BYTE* dsttopleft, long dstrowbytes
				  , const BYTE* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = srctopleft + srcrowbytes * srcoy + srcox;
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const BYTE*	s = src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			*d++ = *s++;
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

long copybits_bw8( BYTE* dsttopleft, long dstrowbytes, const BYTE* lut
				  , const BYTE* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = srctopleft + srcrowbytes * srcoy + srcox;
	BYTE* dst = dsttopleft;

	int	x, y;
	for (y = srcheight; y-- > 0; )
	{
		const BYTE*	s = src;
		BYTE*	d = dst;

		for (x = srcwidth; x-- > 0; )
		{
			*d++ = lut[*s++];
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

long copybits_bw16( BYTE* dsttopleft, long dstrowbytes, long nShift
				  , const WORD* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = (const BYTE*)srctopleft + srcrowbytes * srcoy + srcox * sizeof( WORD );
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const WORD*	s = (const WORD*)src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			*d++ = *s++ >> nShift;
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

long copybits_bw16( BYTE* dsttopleft, long dstrowbytes, const BYTE* lut
				  , const WORD* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = (const BYTE*)srctopleft + srcrowbytes * srcoy + srcox * sizeof( WORD );
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const WORD*	s = (const WORD*)src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			*d++ = lut[ *s++ ];
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

inline void _copybits_line_mono12( BYTE* dst, const BYTE* src1, const BYTE* src2, const WORD* lut1, const WORD* lut2, long width, long nShift )
{
	while( width-- > 0)
	{
		*dst++ = (lut1[ *(WORD*)src1 ] >> nShift);
		src1 += 3;

		if( width-- <= 0 )
			break;

		*dst++ = (lut2[ *(WORD*)src2 ] >> nShift);
		src2 += 3;
	}
}

long copybits_bw12p( BYTE* dsttopleft, long dstrowbytes, long nShift
				  , const WORD* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight, WORD* unpacklut1, WORD* unpacklut2 )
{
	long	lines = 0;

	const BYTE* src = (const BYTE*)srctopleft;
	BYTE* dst = dsttopleft;

	const BYTE* src1	= NULL;
	const BYTE* src2	= NULL;
	long src1Offset	= 0;
	long src2Offset = 0;

	const WORD*	lut1 = NULL;
	const WORD* lut2 = NULL;

	long baseOffset = srcrowbytes * srcoy + (srcox / 2) * 3;
	if( srcox % 2 )
	{
		src1Offset = baseOffset + 1;
		src2Offset = baseOffset + 3;

		lut1 = unpacklut2;
		lut2 = unpacklut1;
	}
	else
	{
		src1Offset = baseOffset;
		src2Offset = baseOffset + 1;

		lut1 = unpacklut1;
		lut2 = unpacklut2;
	}

	src1 = src + src1Offset;
	src2 = src + src2Offset;

	int	y;
	for( y = srcheight; y-- > 0; )
	{
		const WORD*	s = (const WORD*)src;
		BYTE*	d = dst;

		_copybits_line_mono12( d, src1, src2, lut1, lut2, srcwidth, nShift );

		src1 += srcrowbytes;
		src2 += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

inline void _copybits_line_mono12( BYTE* dst, const BYTE* src1, const BYTE* src2, const WORD* lut1, const WORD* lut2, long width, const BYTE* lut )
{
	while( width-- > 0)
	{
		*dst++ = lut[ lut1[ *(WORD*)src1 ] ];
		src1 += 3;

		if( width-- <= 0 )
			break;

		*dst++ = lut[ lut2[ *(WORD*)src2 ] ];
		src2 += 3;
	}
}

long copybits_bw12p( BYTE* dsttopleft, long dstrowbytes, const BYTE* lut
				  , const WORD* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight, WORD* unpacklut1, WORD* unpacklut2  )
{
	long	lines = 0;

	const BYTE* src = (const BYTE*)srctopleft;
	BYTE* dst = dsttopleft;

	const BYTE* src1	= NULL;
	const BYTE* src2	= NULL;
	long src1Offset	= 0;
	long src2Offset = 0;

	const WORD*	lut1 = NULL;
	const WORD* lut2 = NULL;

	long baseOffset = srcrowbytes * srcoy + (srcox / 2) * 3;
	if( srcox % 2 )
	{
		src1Offset = baseOffset + 1;
		src2Offset = baseOffset + 3;

		lut1 = unpacklut2;
		lut2 = unpacklut1;
	}
	else
	{
		src1Offset = baseOffset;
		src2Offset = baseOffset + 1;

		lut1 = unpacklut1;
		lut2 = unpacklut2;
	}

	src1 = src + src1Offset;
	src2 = src + src2Offset;

	int	y;
	for( y = srcheight; y-- > 0; )
	{
		const WORD*	s = (const WORD*)src;
		BYTE*	d = dst;

		_copybits_line_mono12( d, src1, src2, lut1, lut2, srcwidth, lut );

		src1 += srcrowbytes;
		src2 += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

// ----------------

#ifdef HPKINTERNALUSE//{{HPKINTERNALUSE

BYTE byteclip( double v )
{
	if( v < 0 )
		return 0;
	if( v > 255 )
		return 255;

	return (BYTE)v;
}

long copybits_yuv422( BYTE* dsttopleft, long dstrowbytes
				  , const BYTE* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = srctopleft + srcrowbytes * srcoy + srcox * sizeof( WORD );
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const BYTE*	s = src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			int	Cb = *s++;		Cb -= 128;
			int	y0 = *s++;
			int	Cr = *s++;		Cr -= 128;
			int	y1 = *s++;

			BYTE	r, g, b;
			r = byteclip( y0 + ( 1.40200 * Cr ) );
			g = byteclip( y0 - ( 0.34414 * Cb ) - ( 0.71414 * Cr ) );
			b = byteclip( y0 + ( 1.77200 * Cb ) );
			*d++ = b;
			*d++ = g;
			*d++ = r;

			x--;

			r = byteclip( y1 + ( 1.40200 * Cr ) );
			g = byteclip( y1 - ( 0.34414 * Cb ) - ( 0.71414 * Cr ) );
			b = byteclip( y1 + ( 1.77200 * Cb ) );
			*d++ = b;
			*d++ = g;
			*d++ = r;
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

// ----------------

#endif //}}HPKINTERNALUSE

long copybits_bgr8( BYTE* dsttopleft, long dstrowbytes
				  , const BYTE* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = srctopleft + srcrowbytes * srcoy + srcox * 3;
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const BYTE*	s = src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			*d++ = *s++;
			*d++ = *s++;
			*d++ = *s++;
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}


long copybits_bgr16( BYTE* dsttopleft, long dstrowbytes, long nShift
				  , const WORD* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = (const BYTE*)srctopleft + srcrowbytes * srcoy + srcox * 3 * sizeof( WORD );
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const WORD*	s = (const WORD*)src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			*d++ = *s++ >> nShift;
			*d++ = *s++ >> nShift;
			*d++ = *s++ >> nShift;
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

long copybits_bgr16( BYTE* dsttopleft, long dstrowbytes, const BYTE* lut
				  , const WORD* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = (const BYTE*)srctopleft + srcrowbytes * srcoy + srcox * 3 * sizeof( WORD );
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const WORD*	s = (const WORD*)src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			*d++ = lut[ *s++ ];
			*d++ = lut[ *s++ ];
			*d++ = lut[ *s++ ];
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

// ----------------

long copybits_rgb8( BYTE* dsttopleft, long dstrowbytes
				  , const BYTE* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = srctopleft + srcrowbytes * srcoy + srcox * 3;
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const BYTE*	s = src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			register BYTE	r = *s++;
			register BYTE	g = *s++;
			register BYTE	b = *s++;
			*d++ = b;
			*d++ = g;
			*d++ = r;
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}


long copybits_rgb16( BYTE* dsttopleft, long dstrowbytes, long nShift
				  , const WORD* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = (const BYTE*)srctopleft + srcrowbytes * srcoy + srcox * 3 * sizeof( WORD );
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const WORD*	s = (const WORD*)src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			register BYTE	r = *s++ >> nShift;
			register BYTE	g = *s++ >> nShift;
			register BYTE	b = *s++ >> nShift;
			*d++ = b;
			*d++ = g;
			*d++ = r;
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

long copybits_rgb16( BYTE* dsttopleft, long dstrowbytes, const BYTE* lut
				  , const WORD* srctopleft, long srcrowbytes
				  , long srcox, long srcoy, long srcwidth, long srcheight )
{
	long	lines = 0;
	const BYTE*	src = (const BYTE*)srctopleft + srcrowbytes * srcoy + srcox * 3 * sizeof( WORD );
	BYTE* dst = dsttopleft;

	int	x, y;
	for( y = srcheight; y-- > 0; )
	{
		const WORD*	s = (const WORD*)src;
		BYTE*	d = dst;

		for( x = srcwidth; x-- > 0; )
		{
			register BYTE	r = lut[ *s++ ];
			register BYTE	g = lut[ *s++ ];
			register BYTE	b = lut[ *s++ ];
			*d++ = b;
			*d++ = g;
			*d++ = r;
		}

		src += srcrowbytes;
		dst += dstrowbytes;
		lines++;
	}

	return lines;
}

// ----------------
/*
long copybits( BYTE* dsttopleft, long dstrowbytes
		 , const void* srctopleft, long srcrowbytes
		 , long srcox, long srcoy, long srcwidth, long srcheight
		 , BOOL bRGB, long bitperchannel, long nShift, const BYTE* lut )
{
	long	lines;

	if( bRGB )
	{
		if( bitperchannel == 8 )
		{
			lines = copybits_rgb8( dsttopleft, dstrowbytes
				, (const BYTE*)srctopleft, srcrowbytes
				, srcox, srcoy, srcwidth, srcheight );
		}
		else
		{
			ASSERT( bitperchannel <= 16 );

			if( lut == NULL )
			{
				lines = copybits_rgb16( dsttopleft, dstrowbytes, nShift
					, (const WORD*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
			}
			else
			{
				lines = copybits_rgb16( dsttopleft, dstrowbytes, lut
					, (const WORD*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
			}
		}
	}
	else
	{
		if( bitperchannel == 8 )
		{
			lines = copybits_bw8( dsttopleft, dstrowbytes
				, (const BYTE*)srctopleft, srcrowbytes
				, srcox, srcoy, srcwidth, srcheight );
		}
		else
		{
			ASSERT( bitperchannel <= 16 );

			if( lut == NULL )
			{
				lines = copybits_bw16( dsttopleft, dstrowbytes, nShift
					, (const WORD*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
			}
			else
			{
				lines = copybits_bw16( dsttopleft, dstrowbytes, lut
					, (const WORD*)srctopleft, srcrowbytes
					, srcox, srcoy, srcwidth, srcheight );
			}
		}
	}

	return lines;
}
*/
