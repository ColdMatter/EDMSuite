// qMapfile.cpp
//

#include "stdafx.h"
#include	"qMapfile.h"


/////////////////////////////////////////////////////////////////////////////

struct var_qMapfile
{
	HANDLE	hFile;
	HANDLE	hMapping;

	DWORD	size_H;
	DWORD	size_L;

	DWORD	limitsize;

	char*	pMapping;

	SYSTEM_INFO	sysinfo;
};

/////////////////////////////////////////////////////////////////////////////


qMapfile::~qMapfile()
{
	if( pvar_qMapfile->pMapping != NULL )
		UnmapViewOfFile( pvar_qMapfile->pMapping );
	if( pvar_qMapfile->hMapping != NULL )
		CloseHandle( pvar_qMapfile->hMapping );
	if( pvar_qMapfile->hFile != INVALID_HANDLE_VALUE )
		CloseHandle( pvar_qMapfile->hFile );

	delete pvar_qMapfile;
}

qMapfile::qMapfile()
{
	pvar_qMapfile = new var_qMapfile;
	memset( pvar_qMapfile, 0, sizeof( *pvar_qMapfile ) ) ;
	pvar_qMapfile->hFile = INVALID_HANDLE_VALUE;

	pvar_qMapfile->limitsize = 0x10000000;	// 256MB

	memset( &pvar_qMapfile->sysinfo, 0, sizeof( pvar_qMapfile->sysinfo ) );
	GetSystemInfo( &pvar_qMapfile->sysinfo );
}

BOOL qMapfile::open( LPCTSTR path, long flag )
{
	DWORD	dwDesiredAccess, dwShareMode, dwCreationDisposition;

	switch( flag & mask_readwrite )
	{
	case flag_readonly:		dwDesiredAccess = GENERIC_READ;					break;
	case flag_writeonly:	dwDesiredAccess = GENERIC_WRITE;				break;
	case flag_readwrite:	dwDesiredAccess = GENERIC_READ|GENERIC_WRITE;	break;
	default:
		ASSERT( 0 );
		dwDesiredAccess = 0;
		break;
	}

	dwShareMode = 0;
	if( flag & flag_shareread   )	dwShareMode	|= FILE_SHARE_READ;
	if( flag & flag_sharewrite  )	dwShareMode	|= FILE_SHARE_WRITE;
	if( flag & flag_sharedelete )	dwShareMode	|= FILE_SHARE_DELETE;

	switch( flag & mask_createopen )
	{
	default:	ASSERT( 0 );
	case flag_open_existing:	dwCreationDisposition = OPEN_EXISTING;		break;
	case flag_open_always:		dwCreationDisposition = OPEN_ALWAYS;		break;
	case flag_open_truncate:	dwCreationDisposition = TRUNCATE_EXISTING;	break;
	case flag_create_new:		dwCreationDisposition = CREATE_NEW;			break;
	case flag_create_always:	dwCreationDisposition = CREATE_ALWAYS;		break;
	}

	pvar_qMapfile->hFile = CreateFile( path, dwDesiredAccess, dwShareMode, NULL, dwCreationDisposition, NULL, NULL );
	if( pvar_qMapfile->hFile != INVALID_HANDLE_VALUE )
	{
		pvar_qMapfile->size_L = GetFileSize( pvar_qMapfile->hFile, (DWORD*)&pvar_qMapfile->size_H );
		long	end;

		if( pvar_qMapfile->size_H > 0 )
		{
			end = pvar_qMapfile->limitsize;
		}
		else
		{
			if( pvar_qMapfile->size_L < pvar_qMapfile->limitsize )
				pvar_qMapfile->limitsize = pvar_qMapfile->size_L;

			if( pvar_qMapfile->size_L > pvar_qMapfile->limitsize )
				end = pvar_qMapfile->limitsize;
			else
				end = pvar_qMapfile->size_L;
		}

		pvar_qMapfile->hMapping = CreateFileMapping( pvar_qMapfile->hFile, NULL, PAGE_READONLY, 0, end, NULL );
		if( pvar_qMapfile->hMapping != NULL )
		{
			return TRUE;
		}

		CloseHandle( pvar_qMapfile->hFile );
		pvar_qMapfile->hFile = INVALID_HANDLE_VALUE;
	}

	return FALSE;
}

void* qMapfile::lock( size_t& len, long offsetL, long offsetH )
{
	if( pvar_qMapfile->hMapping == NULL )
		return NULL;

	if( pvar_qMapfile->pMapping != NULL )
		UnmapViewOfFile( pvar_qMapfile->pMapping );

	long	ofs = offsetL % pvar_qMapfile->sysinfo.dwAllocationGranularity;
	DWORD	start = offsetL - ofs;
	DWORD	end = offsetL + (DWORD)len;

	if( end - start > pvar_qMapfile->limitsize )
		end = pvar_qMapfile->limitsize + start;

	pvar_qMapfile->pMapping = (char*)MapViewOfFile( pvar_qMapfile->hMapping, FILE_MAP_READ, offsetH, start, end - start );

	if( pvar_qMapfile->pMapping != NULL )
	{
		len = end - start - ofs;
		return (char*)pvar_qMapfile->pMapping + ofs;
	}

	TRACE( "OSERROR: %d\n", GetLastError() );
	ASSERT( 0 );
	return NULL;
}
