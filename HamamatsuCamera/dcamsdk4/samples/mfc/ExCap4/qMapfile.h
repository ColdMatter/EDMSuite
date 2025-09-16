// qMapfile.h
//

class qMapfile
{
public:
	~qMapfile();
	qMapfile();

public:
	BOOL	open( LPCTSTR path, long flag );

	enum {
		flag_readonly			= 0x0001,
		flag_writeonly			= 0x0002,
		flag_readwrite			= 0x0003,

		mask_readwrite		= 0x0003,

		flag_shareread			= 0x0010,
		flag_sharewrite			= 0x0020,
		flag_sharedelete		= 0x0040,

		mask_share			= 0x00F0,

		flag_open_existing		= 0x0000,
		flag_open_always		= 0x0100,
		flag_create_new			= 0x0200,
		flag_create_always		= 0x0300,
		flag_open_truncate		= 0x0500,

		mask_createopen		= 0x0F00,
	};

	void*	lock( size_t& len, long offset_L = 0, long offset_H = 0 );

protected:
	struct var_qMapfile*	pvar_qMapfile;
};
