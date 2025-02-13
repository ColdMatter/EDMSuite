// dcamimg.cpp
//

#include "stdafx.h"
#include "dcamimg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// ----


inline void adjust_hdrfile_size( long& bufsize )
{
	if( bufsize <= 256 )
		bufsize = 256;
	else
	if( bufsize % 16 )
		bufsize += 16 - bufsize % 16;
}

inline long calc_rowbytes( long width, long colortype, long bitsperchannel )
{
	long	rowbytes = width;
	if( bitsperchannel > 8 )
		rowbytes *= ( bitsperchannel + 7 ) / 8;
	if( colortype > 1 )
		rowbytes *= colortype;

	return rowbytes;
}

/////////////////////////////////////////////////////////////////////////////
// class DCAMIMG

DCAMIMG::~DCAMIMG()
{
	if( m_buffer != NULL )
		delete m_buffer;
}

DCAMIMG::DCAMIMG()
{
	m_buffer = NULL;
	m_hFile = INVALID_HANDLE_VALUE;

	m_numberof_block= 1;
	m_numberof_view	= 1;
	m_imagesize		= 0;

	m_iBlock	= 0;
	m_iView		= 0;

	m_colortype		= color_bw;
	m_bitsperchannel	= 8;
	m_bSensorMode_Area = TRUE;
	m_nFrame		= 1;

	m_width		= 0;
	m_rowbytes	= 0;
	m_height	= 0;
	m_framestep	= 0;
}

// ----

BOOL DCAMIMG::set_numberof_block( long nBlock )
{
	if( nBlock < 1 )
		return FALSE;

	m_numberof_block = nBlock;
	return TRUE;
}

BOOL DCAMIMG::set_numberof_view( long nView )
{
	if( nView < 1 )
		return FALSE;

	m_numberof_view = nView;
	return TRUE;
}

BOOL DCAMIMG::set_imagesize( long imagesize )
{
	if( imagesize < 0 )
		return FALSE;

	m_imagesize = imagesize;
	return TRUE;
}

// ----

BOOL DCAMIMG::saveas( LPCTSTR path )
{
	if( m_hFile != INVALID_HANDLE_VALUE )
		return FALSE;

	m_hFile = CreateFile( path, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, NULL );
	if( m_hFile == INVALID_HANDLE_VALUE )
		return FALSE;

	long	bufsize = calc_headersize();
	SetFilePointer( m_hFile, bufsize, NULL, FILE_BEGIN );

	m_buffer = new char[ bufsize ];
	ASSERT( m_buffer != NULL );
	memset( m_buffer, 0, bufsize );

	DCAMIMG_HDRFILE*	hdrfile = (DCAMIMG_HDRFILE*)m_buffer;
	strcpy_s( hdrfile->signature, sizeof( hdrfile->signature ), DCAMIMG_SIGNATURE );

	hdrfile->major_fileversion = 1;
	hdrfile->offset_1st_hdrblock = sizeof( *hdrfile );

	return TRUE;
}

// ----

BOOL DCAMIMG::set_current_block( long iBlock )
{
	if( iBlock < 0 || m_numberof_block <= iBlock )
		return FALSE;

	m_iBlock = iBlock;
	return TRUE;
}

BOOL DCAMIMG::set_current_view( long iView )
{
	if( iView < 0 || m_numberof_view <= iView )
		return FALSE;

	m_iView = iView;
	return TRUE;
}

BOOL DCAMIMG::set_colortype( long c )
{
	if( c != color_bw && c != color_rgb && c != color_bgr
	 && c != color_bw12l && c != color_bw12b )
		return FALSE;

	m_colortype = c;
	return TRUE;
}

BOOL DCAMIMG::set_bitsperchannel( long bitsperchannel )
{
	if( bitsperchannel < 8 || 16 < bitsperchannel )
		return FALSE;

	m_bitsperchannel = bitsperchannel;
	return TRUE;
}

BOOL DCAMIMG::set_numberof_line( long n )	// line sensor output.
{
	if( n <= 0 )
		return FALSE;

	m_bSensorMode_Area = FALSE;
	m_height = n;
	return TRUE;
}

BOOL DCAMIMG::set_numberof_frame( long n )	// area sensor output.
{
	if( n <= 0 )
		return FALSE;

	m_bSensorMode_Area = TRUE;
	m_nFrame = n;
	return TRUE;
}

BOOL DCAMIMG::set_width( long width, long rowbytes )
{
	if( width <= 0 )
		return FALSE;

	m_width = width;
	m_rowbytes = rowbytes;
	return TRUE;
}

BOOL DCAMIMG::set_height( long height )
{
	if( height <= 0 )
		return FALSE;

	m_height = height;
	return TRUE;
}

BOOL DCAMIMG::set_framestep( long framestep )
{
	if( framestep <= 0 )
		return FALSE;

	m_framestep = framestep;
	return TRUE;
}

// ----

BOOL DCAMIMG::saveimage( const void* buf, long bufbytes )
{
	DCAMIMG_HDRVIEW*	hdrview = (DCAMIMG_HDRVIEW*)(m_buffer + calc_viewoffset() );

	hdrview->length_hdrview = sizeof( *hdrview );
	hdrview->colortype		= (char)m_colortype;
	hdrview->bitsperchannel	= (char)m_bitsperchannel;

	if( m_bSensorMode_Area )
	{
		hdrview->totalframe	= m_nFrame;
	}
	else
	{
		hdrview->totalframe	= 0;
	}

	hdrview->width	= m_width;
	hdrview->height	= m_height;

	if( m_rowbytes == 0 )
		hdrview->rowbytes = calc_rowbytes( m_width, m_colortype, m_bitsperchannel );
	else
		hdrview->rowbytes = m_rowbytes;

	if( m_framestep == 0 )
		hdrview->imagestep_L = hdrview->rowbytes * m_height;
	else
		hdrview->imagestep_L = m_framestep;
	hdrview->imagestep_H = 0;

	if( bufbytes == 0 )
		bufbytes = m_framestep;

	hdrview->imageoffset_L = SetFilePointer( m_hFile, 0, &hdrview->imageoffset_H, FILE_CURRENT );

	DWORD	ret;
	VERIFY( WriteFile( m_hFile, buf, bufbytes, &ret, NULL ) && ret == (DWORD)bufbytes );

	return TRUE;
}

BOOL DCAMIMG::saveclose()
{
	if( m_hFile != INVALID_HANDLE_VALUE )
	{
		// finalize header
		ASSERT( m_numberof_view >= 1 );
		ASSERT( m_numberof_block >= 1 );

		DCAMIMG_HDRFILE*	hdrfile	= (DCAMIMG_HDRFILE*)m_buffer;
		DCAMIMG_HDRBLOCK*	hdrblock= (DCAMIMG_HDRBLOCK*)( m_buffer + hdrfile->offset_1st_hdrblock );
		long	blocksize = sizeof( DCAMIMG_HDRBLOCK ) + sizeof( DCAMIMG_HDRVIEW ) * ( m_numberof_view - 1 );

		// write header
		SetFilePointer( m_hFile, 0, NULL, FILE_BEGIN );
		
		DWORD	ret;
		DWORD	len = calc_headersize();

		long	iBlock, iView;
		for( iBlock = 0; iBlock < m_numberof_block; iBlock++ )
		{
			hdrblock->length_hdrblock=blocksize;
			hdrblock->numberof_view	= (short)m_numberof_view;
			for( iView = 0; iView < m_numberof_view; iView++ )
			{
				DCAMIMG_HDRVIEW*	hdrview = hdrblock->view + iView;
			}
			hdrblock = (DCAMIMG_HDRBLOCK*)( (char*)hdrblock + hdrblock->length_hdrblock );
		}

		VERIFY( WriteFile( m_hFile, m_buffer, len, &ret, NULL ) && len == ret );

		CloseHandle( m_hFile );
		m_hFile = INVALID_HANDLE_VALUE;
	}

	return TRUE;
}

// ----

long DCAMIMG::calc_viewoffset() const
{
	long	blocksize = sizeof( DCAMIMG_HDRBLOCK ) + sizeof( DCAMIMG_HDRVIEW ) * ( m_numberof_view - 1 );

	return blocksize * m_iBlock + ( sizeof( DCAMIMG_HDRBLOCK ) + sizeof( DCAMIMG_HDRVIEW ) * ( m_iView - 1 )) + sizeof( DCAMIMG_HDRFILE );
}

long DCAMIMG::calc_headersize() const
{
	long	blocksize = sizeof( DCAMIMG_HDRBLOCK ) + sizeof( DCAMIMG_HDRVIEW ) * ( m_numberof_view - 1 );
	long	hdrsize = blocksize * m_numberof_block + sizeof( DCAMIMG_HDRFILE );
	long	total = hdrsize + sizeof( long );	// last long should be 0 as end mark

	adjust_hdrfile_size( total );

	return total;

/*
	long	ofs = m_hdrfile->offset_1st_hdrblock;

	for( ;; )
	{
		long	len = *(long*)( m_buffer + ofs );
		if( len == 0 )
			break;

		ofs += len;
	}
	ofs += sizeof( long );
	adjust_hdrfile_size( ofs );

	return ofs;
*/
}


/*
				dcamimg_hdrfile	hdr;

				recordimageparam	rip;
				memset( &rip, 0, sizeof( rip ) );

				initialize_rip( m_hdcam, rip );
				
				rip.nFrameBytes = dataframebytes;
				rip.nTotalFrame	= 1;

				fileindex--;
				if( fileindex < 0 ) fileindex += m_nFramecount;

				long offset = initialize_recordimage( &file, rip );
				if( offset > 0 )
				{
					VERIFY( file.Seek( offset, CFile::begin ) == offset );
					file.Write( src, dataframebytes );
				}

				file.Close();
			}


long dcamimg_hdrfile::calc_filesize()
{
	long	total = 0;
	long	iBlock, iView;

	for( iBlock = 0;; iBlock++ )
	{
		DCAMIMG_HDRBLOCK*	hdrblock = get_dcamimg_hdrblock( iBlock );
		if( hdrblock == NULL )
			break;

		for( iView = 0; iView < hdrblock->m_numberof_view; iView++ )
		{
			if( hdrblock->view[ iView ].totalframe == 0 )
				total += hdrblock->view[ iView ].framebytes;
			else
				total += hdrblock->view[ iView ].framebytes * hdrblock->view[ iView ].totalframe;
		}
	}

	return get_dcamimg_hdrfile_size() + total;
}

DCAMIMG_HDRBLOCK*	get_dcamimg_hdrblock( long iBlock = 0 );

long initialize_view( DCAMIMG_HDRBLOCK* hdrblock, long iView, long ofs, long width, long height, long totalframe, long bitsperchannel, long colortype, long framestep )
{
	DCAMIMG_HDRVIEW*	hdrview = ( hdrblock->view + iView );

	//	ASSERT( 0 <= iView && iView < m_
	ASSERT( colortype == 1 || colortype == 3 );
	ASSERT( 8 <= bitsperchannel && bitsperchannel <= 16 );

	hdrview->colortype	= (char)colortype;
	hdrview->bitsperchannel	= (char)bitsperchannel;
	hdrview->totalframe	= totalframe;

	long	pixelbytes	= ( bitsperchannel / 8 );
	if( bitsperchannel % 8 )
		pixelbytes++;
	pixelbytes *= colortype;

	hdrview->width			= width;
	hdrview->height			= height;
	hdrview->rowbytes		= width * pixelbytes;
	hdrview->framebytes		= height * hdrview->rowbytes;

	if( framestep != 0 )
		hdrview->imagestep_L= framestep;
	else
		hdrview->imagestep_L= hdrview->framebytes;

	hdrview->imageoffset_L = ofs;
	return hdrview->framebytes;
}
*/
