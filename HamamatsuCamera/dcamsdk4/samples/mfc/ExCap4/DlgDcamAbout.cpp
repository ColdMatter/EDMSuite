// DlgDcamAbout.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"

#include "DlgDcamAbout.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

#define	TAB		_T( "\x09" )
#define	CRLF	_T( "\x0d\x0a" )

#ifndef tab
#define	tab		_T( '\x09' )
#endif

#ifndef cr
#define	cr		_T( '\x0d' )
#endif

#ifndef lf
#define	lf		_T( '\x0a' )
#endif

static BOOL set_clipboarddata_text( HWND hWnd, LPCTSTR text )
{
	if( ::OpenClipboard( hWnd ) )
	{
		if( EmptyClipboard() )
		{
			DWORD	len = ( lstrlen( text ) + 1 ) * sizeof( TCHAR );
			HANDLE	h = GlobalAlloc( GHND | GMEM_DDESHARE, len );
			if( h != NULL )
			{
				LPVOID	p = GlobalLock( h );
				if( p != NULL )
				{
					memcpy( p, text, len );

					GlobalUnlock( h );
#if defined(UNICODE) || defined(_UNICODE)
					SetClipboardData( CF_UNICODETEXT, h );
#else
					SetClipboardData( CF_TEXT, h );
#endif
					CloseClipboard();

					return TRUE;
				}

				GlobalFree( h );
			}
		}
		CloseClipboard();
	}

	return FALSE;
}

// ----------------------------------------------------------------
// CDlgDcamAbout dialog

static struct {
	UINT		id;
	LPCTSTR		title;
} strings[] = {
	DCAM_IDSTR_BUS,							_T("BUS"),
	DCAM_IDSTR_CAMERAID,					_T("Camera ID"),
	DCAM_IDSTR_VENDOR,						_T("Vendor"),
	DCAM_IDSTR_MODEL,						_T("Model"),
	DCAM_IDSTR_CAMERAVERSION,				_T("Camera Version"),
	DCAM_IDSTR_DRIVERVERSION,				_T("Driver Version"),
	DCAM_IDSTR_MODULEVERSION,				_T("Module Version"),
	DCAM_IDSTR_DCAMAPIVERSION,				_T("DCAMAPI Version"),
#ifdef HPKINTERNALUSE//{{HPKINTERNALUSE
	DCAM_IDSTR_BUS_DETAIL,					_T("BUS Detail"),
	DCAM_IDSTR_BUS_MANUFACTURER,			_T("BUS Manufacturer"),
	DCAM_IDSTR_FGDEVICE_PROPERTIES,			_T("FGDevice Properties"),
#endif //}}HPKINTERNALUSE

	DCAM_IDSTR_OPTICALBLOCK_MODEL,			_T("Optical Block Model"),
	DCAM_IDSTR_OPTICALBLOCK_ID,				_T("Optical Block ID"),
	DCAM_IDSTR_OPTICALBLOCK_DESCRIPTION,	_T("Optical Block Description"),
	DCAM_IDSTR_OPTICALBLOCK_CHANNEL_1,		_T("Optical Block Channel 1"),
	DCAM_IDSTR_OPTICALBLOCK_CHANNEL_2,		_T("Optical Block Channel 2"),

	0, NULL
};

CDlgDcamAbout::CDlgDcamAbout( HDCAM hdcam, CWnd* pParent )
	: CDialog(CDlgDcamAbout::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDlgDcamAbout)
	//}}AFX_DATA_INIT

	m_strStrings.Empty();

	if( hdcam != NULL )
	{
		DCAMERR err;

		long	i;
		for( i = 0; strings[i].id != 0; i++ )
		{
			char	buf[ 256 ] = {'\0'};
			DCAMDEV_STRING param;
			param.size = sizeof(param);
			param.text = buf;
			param.textbytes = 256;
			param.iString = strings[i].id;
#ifdef HPKINTERNALUSE//{{HPKINTERNALUSE
			if (DCAM_IDSTR_FGDEVICE_PROPERTIES == param.iString)
				param.textbytes = 2048;	// should be same as MAXSIZE_FGDEVICE_PROPERTIES of module_common.h.
#endif

			err = dcamdev_getstring( hdcam, &param );
			if( failed(err) || buf[0] == '\0' )
			{
				// this string id is not supported in this camera.
				continue;
			}

			CString	strTitle = strings[i].title;
			CString	strValue( buf );

			CString	str;
			str.Format( _T("%s") TAB _T("%s") CRLF, static_cast<LPCTSTR>(strTitle), static_cast<LPCTSTR>(strValue) );

			m_strStrings += str;
		}
	}

	m_bCopy = ! m_strStrings.IsEmpty();
}

CDlgDcamAbout::CDlgDcamAbout( long index, CWnd* pParent )
	: CDialog(CDlgDcamAbout::IDD, pParent)
{
	m_strStrings.Empty();

	if( index >= 0 )
	{
		DCAMERR err;

		long	i;
		for( i = 0; strings[i].id != 0; i++ )
		{
			char	buf[ 256 ]={'\0'};
			DCAMDEV_STRING param;
			param.size = sizeof(param);
			param.text = buf;
			param.textbytes = 256;
			param.iString = strings[i].id;

			err = dcamdev_getstring( (HDCAM)(INT_PTR)index, &param );
			if( failed(err) )
			{
				// this string id is not supported in this camera.
				continue;
			}

			CString	strTitle = strings[i].title;
			CString	strValue( buf );

			CString	str;
			str.Format( _T("%s") TAB _T("%s") CRLF, static_cast<LPCTSTR>(strTitle), static_cast<LPCTSTR>(strValue) );

			m_strStrings += str;
		}
	}

	m_bCopy = ! m_strStrings.IsEmpty();
}

void CDlgDcamAbout::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDlgDcamAbout)
	DDX_Control(pDX, IDC_DLGDCAMABOUT_LVSTRINGS, m_lvStrings);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CDlgDcamAbout, CDialog)
	//{{AFX_MSG_MAP(CDlgDcamAbout)
	ON_BN_CLICKED(IDC_DLGDCAMABOUT_BTNCOPY, OnCopy)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

// ----------------------------------------------------------------
// CDlgDcamAbout message handlers

BOOL CDlgDcamAbout::OnInitDialog() 
{
	CDialog::OnInitDialog();

	CenterWindow( GetParentFrame() );

	m_lvStrings.InsertColumn( 0, _T("Kind"), 0, 128 );
	m_lvStrings.InsertColumn( 1, _T("Text") );

	LPCTSTR	p;
	for( p = m_strStrings; *p; )
	{
		LPCTSTR	q;
		// find end of line
		for( q = p; *q; q++ )
		{
			if( *q == cr || *q == lf )
				break;
		}

		// find tab
		LPCTSTR	r;
		for( r = p; r < q; r++ )
		{
			if( *r == tab )
				break;
		}

		if( *q && r < q )
		{
			CString	strTitle( p, int(r-p) );

			r++;
			CString	strValue( r, int(q-r) );

			int	nItem = m_lvStrings.InsertItem( m_lvStrings.GetItemCount(), strTitle );
			m_lvStrings.SetItemText( nItem, 1, strValue );
		}
		else
			ASSERT( 0 );

		// prepare next line
		if( *q == cr )	q++;
		if( *q == lf )	q++;
		p = q;
	}

	if( m_lvStrings.GetItemCount() > 0 )
	{
		m_lvStrings.SetColumnWidth( 0, LVSCW_AUTOSIZE );
		m_lvStrings.SetColumnWidth( 1, LVSCW_AUTOSIZE );
	}

	GetDlgItem( IDC_DLGDCAMABOUT_BTNCOPY )->EnableWindow( m_bCopy );

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CDlgDcamAbout::OnCopy() 
{
	CString	str;

	GetDlgItemText( IDC_DLGDCAMABOUT_TXTVERSION, str );

	str += CRLF;
	str += m_strStrings;

	if( ! set_clipboarddata_text( GetSafeHwnd(), str ) )
		AfxMessageBox( _T( "Fail: The version infomation was not copied into clipboard." ), MB_OK | MB_ICONWARNING );
	else
		AfxMessageBox( _T( "The version infomation has been copied into clipboard successfully." ), MB_OK | MB_ICONINFORMATION );
}
