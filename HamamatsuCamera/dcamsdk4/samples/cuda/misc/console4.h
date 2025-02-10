// console4.h

// ----------------------------------------------------------------

#ifdef _WIN32

// Windows

#include	<windows.h>

#elif defined( LINUX )

// Linux

#include	<string.h>
#include	<stdint.h>
#include	<stdlib.h>
#include	<pthread.h>
#include	<ctype.h>

#elif defined( MACOSX ) || __ppc64__ || __i386__ || __x86_64__

// Mac

// No carbon

#endif


// common headers

#include	<stdio.h>

// DCAM-API headers

#ifndef _NO_DCAMAPI

#ifndef DCAMAPI_VER
#define	DCAMAPI_VER		4000
#endif

#ifndef DCAMAPI_VERMIN
#define	DCAMAPI_VERMIN	4000
#endif

#if	defined( LINUX )
#include			"dcamapi4.h"
#include			"dcamprop.h"
#else
#include			"../../../inc/dcamapi4.h"
#include			"../../../inc/dcamprop.h"
#endif

#if defined( _WIN64 )
#pragma comment(lib,"../../../lib/win64/dcamapi.lib")
#elif defined(_WIN32)
#pragma comment(lib,"../../../lib/win32/dcamapi.lib")
#endif

#endif // _NO_DCAMAPI

// ----------------------------------------------------------------

// define common macro

#ifndef ASSERT
#define	ASSERT(c)
#endif

// absorb different function

#ifdef _WIN32

#define	strcmpi	_strcmpi

#if defined(UNICODE) || defined(_UNICODE)
#define	_T(str)	L##str
#else
#define	_T(str)	str
#endif

#elif defined( MACOSX ) || __ppc64__ || __i386__ || __x86_64__ || defined( LINUX )

#define	strcmpi	strcasecmp
#define	_T(str)	str

#endif

// absorb Visual Studio 2005 and later

#if defined(_WIN32) && _MSC_VER >= 1400

#define	_secure_buf(buf)		buf,sizeof( buf )
#define	_secure_ptr(ptr,size)	ptr,size
#define	_secure_bufuseptr(base,now)	now, sizeof(base)-(now-base)

#else

#define	memcpy_s				memcpy
#define	sprintf_s				sprintf
#define	strcat_s				strcat
#define	strcpy_s				strcpy
#define	_stricmp(str1, str2)			strncasecmp( str1, str2, strlen(str2) )
#define	_strcmpi						strcasecmp
#define	gets_s			gets
#define	_secure_buf(buf)		buf
#define	_secure_ptr(ptr,size)	ptr
#define	_secure_bufuseptr(base,now)	now
#define	WORD			uint16_t
#define	DWORD			uint32_t
#define	LONGLONG		int64_t
#define	BYTE			uint8_t
#define	MAX_PATH	256
#define	BOOL		int
#define	TRUE		1
#define	FALSE		0

#ifndef fopen_s
inline int fopen_s( FILE** fpp, const char* filename, const char* filemode )
{
	*fpp = fopen( filename, filemode );
	if( fpp == NULL )
		return 1;
	else
		return 0;
}
#endif
#ifndef Sleep
inline void Sleep( DWORD dwMillseconds )
{
	struct timespec t;
	t.tv_sec = dwMillseconds / 1000;
	t.tv_nsec = ( dwMillseconds % 1000 ) * 1000000;
	nanosleep( &t, NULL );
}
#endif

#endif

// ----------------------------------------------------------------
