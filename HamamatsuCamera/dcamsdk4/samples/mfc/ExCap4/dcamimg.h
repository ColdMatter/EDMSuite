// dcamimg.h

#define	DCAMIMG_SIGNATURE	"DCAMIMG"

#ifndef OFFSETOF
#define	OFFSET_OF(member,type)		(long)(&((type*)0)->member)
#endif

struct DCAMIMG_HDRVIEW
{
// 18h
	short	length_hdrview;	// include this value. usually 0x28.
	char	colortype;		// 1=B/W, 3=RGB
	char	bitsperchannel;	// 8=8bit, 16=16bit per channel. 9-15 means bit per pixel.
	long	totalframe;		// 0=line, >1=frame

// 20h
	long	imageoffset_L;	// from top(00h)
	long	imageoffset_H;	// from top(00h)
	long	imagestep_L;
	long	imagestep_H;

// 30h
	long	width;
	long	height;
	long	framebytes;
	long	rowbytes;
};

struct DCAMIMG_HDRBLOCK
{
// 10h
	long	length_hdrblock;	// include this value. usually 0x30.
	short	numberof_view;		// usually 1
	short	reserved2;

// 18h
	// header view begin
	DCAMIMG_HDRVIEW	view[ 1 ];
};

struct DCAMIMG_HDRFILE
{
// 00h
	char	signature[ 8 ];			// DCAMIMG_SIGNATURE
	char	major_fileversion;		// 1
	char	reserved1[ 3 ];			// must be 0
	long	offset_1st_hdrblock;	// from top. usually 0x10

// 10h
	// header block will begin
//	DCAMIMG_HDRBLOCK	block[ 1 ];	// the last block must have 4 bytes 0 data
};

// ----

class DCAMIMG
{
public:
	~DCAMIMG();
	DCAMIMG();
	
public:
	BOOL	set_numberof_block( long nBlock );	// option
	BOOL	set_numberof_view( long nView );	// option
	BOOL	set_imagesize( long imagesize );	// option

public:
	BOOL	saveas( LPCTSTR path );
	BOOL	saveclose();

	BOOL	set_current_block( long iBlock );	// option
	BOOL	set_current_view( long iView );		// option

	enum {
		color_bw = 1,
		color_rgb = 4,
		color_bgr = 5,
		color_bw12l = 6,		// packed mono 12bit little endian
		color_bw12b = 7,		// packed mono 12bit big endian
	};
	BOOL	set_colortype( long c );			// 1=B/W, 4=RGB, 5=BGR (old 3=RGB)
	BOOL	set_bitsperchannel( long n );		// 8=8bit, 16=16bit per channel. 9-15 means bit per pixel.
	BOOL	set_numberof_line( long n );		// line sensor output.
	BOOL	set_numberof_frame( long n );		// area sensor output.

	BOOL	set_width( long width, long rowbytes = 0 );
	BOOL	set_height( long height );
	BOOL	set_framestep( long framestep );

	BOOL	saveimage( const void* buf, long bufbytes = 0 );

protected:
	struct DCAMIMG_HDRVIEW*	get_dcamimg_hdrview();

	long	calc_headersize() const;
	long	calc_viewoffset() const;

protected:
	char*	m_buffer;
	HANDLE	m_hFile;

	long	m_numberof_block;
	long	m_numberof_view;
	long	m_imagesize;

	long	m_iBlock;
	long	m_iView;

	long	m_colortype;
	long	m_bitsperchannel;
	long	m_bSensorMode_Area;
	long	m_nFrame;

	long	m_width;
	long	m_rowbytes;
	long	m_height;
	long	m_framestep;

};

/*
public:
	DCAMIMG_HDRVIEW*	get_dcamimg_hdrview( long iBlock = 0, long iView = 0 );

	BOOL	get_header( void*& hdr, long& hdrsize );
	long	calc_filesize();
};

// long initialize_view( DCAMIMG_HDRBLOCK* hdrblock, long iView, long ofs, long width, long height, long totalframe, long bitsperchannel, long colortype = 1, long framestep = 0 );
*/
