// portable.h
//

#if _MSC_VER < 1400
inline int __cdecl sprintf_s( char* buf, long bufsize, const char* fmt, ...)
{
	int	ret;

	va_list	arg;
	va_start(arg,fmt);
	ret = vsprintf(buf,fmt,arg);
	va_end(arg);

	return ret;
}

inline char* strcpy_s( char* dst, long dstsize, const char* src )
{
	long	len = strlen( src ) + 1;
	memcpy( dst, src, min( dstsize, len ) );

	return dst;
}

#endif

#if defined(UNICODE) || defined(_UNICODE)
#define	atoi	_wtoi
#define	atof	_wtof
#endif
