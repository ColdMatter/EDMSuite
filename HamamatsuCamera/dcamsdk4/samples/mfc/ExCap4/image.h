// image.h
//

class image
{
protected:
	virtual	~image();
			image();

public:
	static image*	load( LPCTSTR path );

public:
	virtual	BOOL	release();	// TRUE means still object is exist.

public:
	virtual	BOOL	allocateframes( long framecount, BOOL bUser );
	virtual	BOOL	freeframes();
	virtual	void**	getframes();
	virtual	void*	getframe( long index, int32& rowbytes );

	virtual BOOL    load_frame( long index ) {return FALSE;}

public:
	virtual BOOL	get_bitmapinfoheader( BITMAPINFOHEADER& bmih ) const = 0;
	virtual	long	copybits( BYTE* dsttopleft, long rowbytes, long iFrame, long srcwidth, long srcheight, long hOffset, long vOffset, const BYTE* lut = NULL ) = 0;

public:
	enum {
		colortype_bw		= 1,
		colortype_oldbgr	= 2,
		colortype_oldrgb	= 3,
		colortype_rgb		= 4,
		colortype_bgr		= 5,
		colortype_bw12l		= 6,		// packed mono 12bit little endian
		colortype_bw12b		= 7,		// packed mono 12bit big endian
	};

public:
	virtual	long	width() const = 0;
	virtual	long	height() const = 0;
	virtual	long	colortype() const = 0;
	virtual long	pixelperchannel() const = 0;

	virtual long	numberof_frames() const = 0;

	virtual	BOOL	is_bitmap_updated() = 0;
	virtual	void	clear_bitmap_updated() = 0;
#ifdef _EXCAP_SUPPORTS_VIEWS_
	virtual void	set_shown_multiview( long index ) {};
#endif // _EXCAP_SUPPORTS_VIEWS_ !

protected:
	struct
	{
		WORD	lut1[65536];
		WORD	lut2[65536];
	} mono12pl, mono12pb;
};
