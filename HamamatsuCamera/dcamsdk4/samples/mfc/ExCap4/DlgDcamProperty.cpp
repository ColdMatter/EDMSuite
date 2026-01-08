// DlgDcamProperty.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include <math.h>	// ceil() & fmod()

#include "DlgDcamProperty.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////

inline BOOL value_to_text( HDCAM hdcam, int32 iProp, double value, char* text, int32 textsize )
{
	DCAMERR err;

	DCAMPROP_VALUETEXT	pvt;
	memset( &pvt, 0, sizeof( pvt ) );
	pvt.cbSize	= sizeof( pvt );
	pvt.iProp	= iProp;
	pvt.value	= value;
	pvt.text	= text;
	pvt.textbytes = textsize;

	err = dcamprop_getvaluetext( hdcam, &pvt );
	if( !failed(err) )
		return TRUE;

	DCAMPROP_ATTR	pa;
	memset( &pa, 0, sizeof( pa ) );
	pa.cbSize	= sizeof( pa );
	pa.iProp	= iProp;

	err = dcamprop_getattr( hdcam, &pa );
	VERIFY( !failed(err) );

	if( ( pa.attribute & DCAMPROP_TYPE_MASK ) == DCAMPROP_TYPE_REAL )
	{
		sprintf_s( text, textsize, "%g", value );
	}
	else
	if( ( pa.attribute & DCAMPROP_TYPE_MASK ) == DCAMPROP_TYPE_LONG )
	{
		sprintf_s( text, textsize, "%d", (long)value );
	}
	else
	{
		ASSERT( ( pa.attribute & DCAMPROP_TYPE_MASK ) == DCAMPROP_TYPE_MODE );
		sprintf_s( text, textsize, "(invalid value; %g)", value );
	}

	return TRUE;
}


#ifdef HPKINTERNALUSE //{{HPKINTERNALUSE


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

#define	TAB		_T( "\x09" )
#define	CRLF	_T( "\x0d\x0a" )

#define	IDM_COPYPROPERTIES			0x0020

BOOL dcamwin_copyproperties( HDCAM hdcam, HWND hWnd )
{
	if( hdcam == NULL )
		return FALSE;

	int32	iProp = 0;
	int32	option = 0;

	CString	str;

	DCAMERR err;
	err = dcamprop_getnextid( hdcam, &iProp, option );
	if( !failed(err) )
	{
		do
		{
			CString	strItem;
			CString	str;

			char	title[ 64 ];
			err = dcamprop_getname( hdcam, iProp, title, sizeof( title ) );
			VERIFY( !failed(err) );
			str = title;
			strItem.Append( str );

/*
			{
				DCAMPROP_ATTR	attr;
				memset( &attr, 0, sizeof( attr ) );
				attr.cbSize	= sizeof( attr );
				attr.iProp	= iProp;
		
				err = dcamprop_getattr( hdcam, &attr );
				VERIFY( !failed(err) );
				m_arrayAttr.Add( attr.attribute );
			}
*/
			char	text[ 64 ];
			double	value;

			err = dcamprop_getvalue( hdcam, iProp, &value );
			if( failed(err) )
			{
				strcpy_s( text, sizeof( text ), "(invalid)" );
			}
			else
			{
				value_to_text( hdcam, iProp, value, text, sizeof( text ) );
			}

			strItem.Append( TAB );
			str = text;
			strItem.Append( str );

			strItem.Append( CRLF );

			str.Append( strItem );

			err = dcamprop_getnextid( hdcam, &iProp, option );
		} while( !failed(err) );
	}

	return set_clipboarddata_text( hWnd, str );
}

#undef	TAB
#undef	CRLF

#endif //}}HPKINTERNALUSE

/////////////////////////////////////////////////////////////////////////////
// CDlgDcamProperty dialog

enum {
	INDEX_PROPERTY_NAME,
	INDEX_PROPERTY_UPDATE,
	INDEX_PROPERTY_VALUE
};

CDlgDcamProperty::CDlgDcamProperty( CWnd* pParent /*=NULL*/)
	: CDialog(CDlgDcamProperty::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDlgDcamProperty)
	m_bShowAllProperties = FALSE;
	m_bUseListboxAlways = FALSE;
	m_bUpdatePeriodically = FALSE;
	//}}AFX_DATA_INIT

	m_bCreateDialog		= FALSE;
	m_hdcam				= NULL;
	m_bChangingEditbox	= FALSE;

	memset( &m_editprop, 0, sizeof( m_editprop ) );
	m_editprop.indexOnListview	= -1;

	m_fRatioSlider		= 1;
	m_fStepSlider		= 0;
	m_dcamstatus		= DCAMCAP_STATUS_ERROR;
	m_bAutomaticUpdatePropertyValues	= TRUE;

	m_channelmode		= 0;
	m_iChannel			= 0;
	m_idpropoffset		= 0;
	m_idproparraybase	= 0;
	
	m_rcClient.SetRectEmpty();
}

// ----

HDCAM CDlgDcamProperty::set_hdcam( HDCAM hdcam )
{
	HDCAM	old = m_hdcam;

	m_hdcam = hdcam;

	if( IsWindow( GetSafeHwnd() ) )
	{
		update_viewchannel_control();
		update_listview_title( 0, TRUE );
		update_listview_value();
		reset_listview_updated_value();
	}

	return old;
}

BOOL CDlgDcamProperty::toggle_visible()
{
	if( ! IsWindow( GetSafeHwnd() ) )
	{
		if( ! Create() )
		{
			ASSERT( 0 );
			return FALSE;
		}
	}
	else
	if( IsWindowVisible() )
	{
		ShowWindow( SW_HIDE );
	}
	else
	{
		update_listview_updated_value();
		SetWindowPos( &CWnd::wndTop, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW );
	}

	return TRUE;
}

BOOL CDlgDcamProperty::Create( CWnd* pParentWnd, CCreateContext* pContext ) 
{
	// TODO: Add your specialized code here and/or call the base class
	
	if( ! CDialog::Create(IDD, pParentWnd) )
		return FALSE;

	m_bCreateDialog = TRUE;
	return TRUE;
}

// ----------------

void CDlgDcamProperty::update_viewchannel_control()
{
	DCAMERR err;

	long	nChannel;

	double	v;
	err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_NUMBEROF_CHANNEL, &v );
	if( failed(err) || v <= 1 )
		nChannel = 1;
	else
		nChannel = (long)v;

	if( nChannel == 1 )
	{
		// single view and channel

		m_cbChannel.ShowWindow( SW_HIDE );
		for(int i = 0; i <= 3; i++ )
		{
			m_btnChannel[ i ].ShowWindow( SW_HIDE );
		}

		m_channelmode = 0;
	}
	else
	if( nChannel <= 3 )
	{
		LPCTSTR	type;
		type = _T( "CH" );

		m_cbChannel.ShowWindow( SW_HIDE );
		for(int i = 0; i <= 3; i++ )
		{
			m_btnChannel[ i ].ShowWindow( ( i <= v ) ? SW_SHOW : SW_HIDE );

			CString	str;
			str.Format( i == 0 ? _T( "All %s" ) : _T( "%s %d" ), type, i );
			m_btnChannel[ i ].SetWindowText( str );
		}

		m_channelmode = nChannel;
	}
	else
	{
		// over 3 channels

		for(int i = 0; i <= 3; i++ )
		{
			m_btnChannel[ i ].ShowWindow( SW_HIDE );
		}

		m_cbChannel.ResetContent();

		CString	str;


		m_cbChannel.AddString( _T( "CH All" ) );
		m_cbChannel.SetItemData( 0, 0 );	// idpropoffset is 0.

		for(int j = 1; j <= nChannel; j++ )
		{
			str.Format( _T( "CH %d" ), j );
			int i = m_cbChannel.AddString( str );

			long	idpropoffset = j * DCAM_IDPROP__CHANNEL;
			m_cbChannel.SetItemData( i, idpropoffset );
		}
	

		m_cbChannel.SetCurSel( 0 );
		m_cbChannel.ShowWindow( SW_SHOW );

		m_channelmode = 1;
	}
}

void CDlgDcamProperty::update_listview_title( long iPropArrayBase, BOOL bInit )
{
	m_listview.DeleteAllItems();

	if( m_hdcam != NULL )
	{
		m_listview.SetRedraw( FALSE );
		m_arrayIDPROP.RemoveAll();
		m_arrayAttr.RemoveAll();

		DCAMERR err = DCAMERR_SUCCESS;

		int32	iProp = iPropArrayBase;
		int32	option = ( iProp == 0 ? 0 : DCAMPROP_OPTION_ARRAYELEMENT );

		if( iProp == 0 )
			err = dcamprop_getnextid( m_hdcam, &iProp ); 

		if( !failed(err) )
		{
			long	iItem = 0;
			do
			{
				char	text[ 64 ];
				err = dcamprop_getname( m_hdcam, iProp, text, sizeof( text ) );
				VERIFY( !failed(err) );
				
				DCAMPROP_ATTR	attr;
				memset( &attr, 0, sizeof( attr ) );
				attr.cbSize	= sizeof( attr );
				attr.iProp	= iProp;
				
				err = dcamprop_getattr( m_hdcam, &attr );
				VERIFY( !failed(err) );		
	
				if (attr.attribute & DCAMPROP_ATTR_HASVIEW)
				{
					for (int i=0; i<attr.nMaxView; i++)
					{
						CString	str(text);
						CString sView;
						sView.Format(_T(" VIEW%d"), i+1);
						str += sView;
						m_listview.InsertItem( iItem, str );
						m_listview.SetItemData( iItem, iItem );
						m_arrayIDPROP.Add( (iProp + (DCAM_IDPROP__VIEW * (i+1))) );
						m_arrayAttr.Add( attr.attribute );//use same attribute as arraybase...
						iItem++;
					}
				}
				else
				{
					CString	str(text);
					m_listview.InsertItem( iItem, str );
					m_listview.SetItemData( iItem, iItem );
					m_arrayIDPROP.Add( iProp );
					m_arrayAttr.Add( attr.attribute );
					iItem++;
				}

				err = dcamprop_getnextid( m_hdcam, &iProp, option ); 
			} while( !failed(err) );
		}

		m_listview.SetRedraw( TRUE );
		m_listview.Invalidate();
	}

	m_idproparraybase = iPropArrayBase;
}

void CDlgDcamProperty::update_listview_value()
{
	ASSERT( m_arrayIDPROP.GetSize() == m_arrayAttr.GetSize() );

	DWORD	bRedraw = FALSE;

	DCAMERR err;

	{
		long	dwNew;
		err = dcamcap_status( m_hdcam, &dwNew);
		if( failed(err) )
			dwNew = DCAMCAP_STATUS_ERROR;

		if( m_dcamstatus != dwNew )
		{
			m_dcamstatus = dwNew;
			bRedraw = TRUE;
		}
	}

	{
		m_listview.SetRedraw( FALSE );

		long	iItem;
		for( iItem = 0; iItem < m_listview.GetItemCount(); iItem++ )
		{
			long	i = (long)m_listview.GetItemData( iItem );
			ASSERT( 0 <= i && i < m_arrayIDPROP.GetSize() );

			long	iProp = (long)m_arrayIDPROP.GetAt( i );
			iProp += m_idpropoffset;

			char	text[ 64 ];
			double	value;

			err = dcamprop_getvalue( m_hdcam, iProp, &value );
			if( failed(err) )
			{
				strcpy_s( text, sizeof( text ), "(invalid)" );
			}
			else
			{
				value_to_text( m_hdcam, iProp, value, text, sizeof( text ) );
			}

			CString	str = m_listview.GetItemText( iItem, INDEX_PROPERTY_VALUE );
			if( str != text )
			{
				str = text;
				m_listview.SetItemText( iItem, INDEX_PROPERTY_VALUE, str );
				bRedraw = TRUE;
			}
		}

		m_listview.SetRedraw( TRUE );
		if( bRedraw )
			m_listview.Invalidate();
	}
}

// Enumerate DCAM_IDPROP which is updated value or attribute and Make count up to visualize.

void CDlgDcamProperty::update_listview_updated_value()
{
	if( m_hdcam != NULL )
	{
		m_listview.SetRedraw( FALSE );

		DCAMERR err;

		int32	iProp = 0;

		err = dcamprop_getnextid( m_hdcam, &iProp, DCAMPROP_OPTION_UPDATED );
		if( !failed(err) )
		{
			do
			{
				// find index on the listview.
				long	iItem;
				for( iItem = 0; iItem < m_listview.GetItemCount(); iItem++ )
				{
					long	i = (long)m_listview.GetItemData( iItem );
					if( (long)(m_arrayIDPROP[ i ] & DCAM_IDPROP__MASK_BODY) == iProp )
					{
						CString	str = m_listview.GetItemText( iItem, INDEX_PROPERTY_UPDATE );
						str.Format( _T( "%d" ), atoi( str ) + 1 );
						m_listview.SetItemText( iItem, INDEX_PROPERTY_UPDATE, str );

						char	text[ 64 ];
						double	value;

						err = dcamprop_getvalue( m_hdcam, iProp, &value );
						if( failed(err) )
						{
							strcpy_s( text, sizeof( text ), "(invalid)" );
						}
						else
						{
							value_to_text( m_hdcam, iProp, value, text, sizeof( text ) );
						}

						str = m_listview.GetItemText( iItem, INDEX_PROPERTY_VALUE );
						if( str == text )
						{
							// text sometimes not changed because DCAMPROP_OPTION_UPDATED capture even if the only support range is changed.
						}
						else
						{
							str = text;
							m_listview.SetItemText( iItem, INDEX_PROPERTY_VALUE, str );
						}

						{
							DCAMPROP_ATTR	attr;
							memset( &attr, 0, sizeof( attr ) );
							attr.cbSize	= sizeof( attr );
							attr.iProp	= iProp;
					
							err = dcamprop_getattr( m_hdcam, &attr );
							VERIFY( !failed(err) );
							m_arrayAttr.SetAt( iItem, attr.attribute );
						}
					}
				}

				err = dcamprop_getnextid( m_hdcam, &iProp, DCAMPROP_OPTION_UPDATED );
			} while( !failed(err) );
		}

		m_listview.SetRedraw( TRUE );
		m_listview.Invalidate();
	}
}

void CDlgDcamProperty::reset_listview_updated_value()
{
	if( m_hdcam != NULL )
	{
		m_listview.SetRedraw( FALSE );

		// reset INDEX_PROPERTY_UPDATE column of listview

		long	iItem;
		for( iItem = 0; iItem < m_listview.GetItemCount(); iItem++ )
		{
			m_listview.SetItemText( iItem, INDEX_PROPERTY_UPDATE, _T( "" ) );
		}

		// All UPDATED flag should be reset because update_listview_updated_value() is called before this routine.

		DCAMERR err;

		int32	iProp = 0;
		err = dcamprop_getnextid( m_hdcam, &iProp, DCAMPROP_OPTION_UPDATED );
		ASSERT( failed(err) );

		m_listview.SetRedraw( TRUE );
		m_listview.Invalidate();
	}
}

// ----------------

void CDlgDcamProperty::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDlgDcamProperty)
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_LISTVIEW, m_listview);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_CBSELECTVIEW, m_cbChannel);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_LBVALUES, m_lbValues);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_LBATTR, m_lbAttr);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_TXTMAX, m_txtMax);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_TXTMIN, m_txtMin);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_WNDMIN, m_wndMin);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_WNDMAX, m_wndMax);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_EBVALUE, m_ebValue);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_SLIDERVALUE, m_sliderValue);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_SPINVALUE, m_spinValue);
	DDX_Check(pDX, IDC_DLGDCAMPROPERTY_BTNUSELISTBOX, m_bUseListboxAlways);
	DDX_Check(pDX, IDC_DLGDCAMPROPERTY_BTNUPDATEPERIODICALLY, m_bUpdatePeriodically);
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_BTNALL, m_btnChannel[0]);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_BTN1, m_btnChannel[1]);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_BTN2, m_btnChannel[2]);
	DDX_Control(pDX, IDC_DLGDCAMPROPERTY_BTN3, m_btnChannel[3]);
}


BEGIN_MESSAGE_MAP(CDlgDcamProperty, CDialog)
	//{{AFX_MSG_MAP(CDlgDcamProperty)
	ON_WM_SIZE()
	ON_WM_DESTROY()
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_DLGDCAMPROPERTY_LISTVIEW, OnItemchangedDlgdcampropertyListview)
	ON_NOTIFY(NM_CLICK, IDC_DLGDCAMPROPERTY_LISTVIEW, OnClickDlgdcampropertyListview)
	ON_NOTIFY(NM_DBLCLK, IDC_DLGDCAMPROPERTY_LISTVIEW, OnDblclkDlgdcampropertyListview)
	ON_NOTIFY(NM_CUSTOMDRAW, IDC_DLGDCAMPROPERTY_LISTVIEW, OnCustomdrawDlgdcampropertyListview)
	ON_WM_NCHITTEST()
	ON_WM_VSCROLL()
	ON_CBN_SELCHANGE(IDC_DLGDCAMPROPERTY_CBSELECTVIEW, OnSelchangeDlgdcampropertyCbselectchl)
	ON_LBN_SELCHANGE(IDC_DLGDCAMPROPERTY_LBVALUES, OnSelchangeDlgdcampropertyLbvalues)
	ON_EN_CHANGE(IDC_DLGDCAMPROPERTY_EBVALUE, OnChangeDlgdcampropertyEbvalue)
	ON_NOTIFY(UDN_DELTAPOS, IDC_DLGDCAMPROPERTY_SPINVALUE, OnDeltaposDlgdcampropertySpin)
	ON_WM_TIMER()
	ON_COMMAND_RANGE(IDC_DLGDCAMPROPERTY_BTNALL, IDC_DLGDCAMPROPERTY_BTN3, OnDlgdcampropertyBtn)
	ON_BN_CLICKED(IDC_DLGDCAMPROPERTY_BTNUSELISTBOX, OnDlgdcampropertyBtnuselistbox)
	ON_BN_CLICKED(IDC_DLGDCAMPROPERTY_BTNUPDATEPERIODICALLY, OnDlgdcampropertyBtnupdateperiodically)
	ON_BN_CLICKED(IDC_DLGDCAMPROPERTY_BTNUPDATEVALUES, OnDlgdcampropertyBtnupdatevalues)
	ON_BN_CLICKED(IDC_DLGDCAMPROPERTY_BTNWHOLEIDPROP, OnDlgdcampropertyBtnwholeidprop)
	ON_BN_CLICKED(IDC_DLGDCAMPROPERTY_BTNARRAYELEMENT, OnDlgdcampropertyBtnarrayelement)
	//}}AFX_MSG_MAP
#ifdef HPKINTERNALUSE //{{HPKINTERNALUSE
	ON_WM_SYSCOMMAND()
#endif //}}HPKINTERNALUSE
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CDlgDcamProperty 

/////////////////////////////////////////////////////////////////////////////
// helper functions

inline CString get_string_as_long( double v )
{
	CString	str;
	str.Format( _T( "%d" ), (long)v );

	return str;
}

inline CString get_string_as_real( double v )
{
	CString	str;
	str.Format( _T( "%g" ), v );

	return str;
}

void fill_propertyvaluetext_into_listbox( CListBox& lb, HDCAM hdcam, long iProp, double selectedValue = 0 )
{
	DCAMERR err;

	DCAMPROP_ATTR	pa;
	memset( &pa, 0, sizeof( pa ) );
	pa.cbSize	= sizeof( pa );
	pa.iProp	= iProp;

	err = dcamprop_getattr( hdcam, &pa );
	VERIFY( !failed(err) );
//	if( ( pa.attribute & DCAMPROP_ATTR_WRITABLE ) == 0 )
//		return;

	lb.SetRedraw( FALSE );
	lb.ResetContent();

	long	iSel = -1;
	double	v = pa.valuemin;
	for( ;; )
	{
		char	buf[ 256 ];

		VERIFY( value_to_text( hdcam, iProp, v, buf, sizeof( buf ) ) );

		CString	str;
		str = buf;
		long	index = lb.AddString( str );

		lb.SetItemData( index, (long)v );
		if( selectedValue == v )
			iSel = index;

		err = dcamprop_queryvalue( hdcam, iProp, &v, DCAMPROP_OPTION_NEXT );
		if( failed(err) )
			break;
	}

	lb.SetCurSel( iSel );
	lb.SetRedraw( TRUE );
	lb.Invalidate();
}


void CDlgDcamProperty::edit_property_of( long index )
{
	m_bChangingEditbox = TRUE;

	m_editprop.indexOnListview	= index;
	m_editprop.attribute	= 0;
	m_editprop.attribute2	= 0;

	long	iPropBase = 0;
	if( m_hdcam != NULL && index >= 0 )
	{
		DCAMERR err;

		ASSERT( index < m_listview.GetItemCount() );

		long	i = (long)m_listview.GetItemData( m_editprop.indexOnListview );
		ASSERT( 0 <= i && i < m_arrayIDPROP.GetSize() );

		iPropBase = m_arrayIDPROP.GetAt( i );
		long	iProp = iPropBase + m_idpropoffset;

		DCAMPROP_ATTR	pa;
		memset( &pa, 0, sizeof( pa ) );
		pa.cbSize	= sizeof( pa );
		pa.iProp	= iPropBase;

		CString	strValue;

		err = dcamprop_getattr( m_hdcam, &pa );
		if( failed(err) )
		{
			ASSERT( m_idpropoffset != 0 );
			m_editprop.attribute	= 0;
			m_editprop.attribute2	= 0;
		}
		else
		{
			m_editprop.attribute	= pa.attribute;
			m_editprop.attribute2	= pa.attribute2;
			m_editprop.nMaxView		= pa.nMaxView;
			m_editprop.nMaxChannel	= pa.nMaxChannel;

			if( m_editprop.nMaxView <= 1 && m_editprop.nMaxChannel <= 1 )
			{
				m_idpropoffset = 0;	// all
				m_iChannel = 0;
			}

			double	value;
			err = dcamprop_getvalue( m_hdcam, iProp, &value );
			VERIFY( !failed(err) );

			if( m_bUseListboxAlways )
			{
				fill_propertyvaluetext_into_listbox( m_lbValues, m_hdcam, iProp, value );
			}
			else
			{
				switch( m_editprop.attribute & DCAMPROP_TYPE_MASK )
				{
				case DCAMPROP_TYPE_NONE:	break;
				case DCAMPROP_TYPE_MODE:
					fill_propertyvaluetext_into_listbox( m_lbValues, m_hdcam, iProp, value );
					break;

				case DCAMPROP_TYPE_LONG:
					m_txtMin.SetWindowText( get_string_as_long( pa.valuemin ) );
					m_txtMax.SetWindowText( get_string_as_long( pa.valuemax ) );

					m_fRatioSlider = 1;
					m_fStepSlider = pa.valuestep;
					m_sliderValue.SetRange( -(long)pa.valuemax, -(long)pa.valuemin );
					m_sliderValue.SetPos( -(long)value );
					m_spinValue.SetRange32( (long)pa.valuemin, (long)pa.valuemax );
					strValue = get_string_as_long( value );
				//	m_spinValue.
					break;
				case DCAMPROP_TYPE_REAL:
					m_txtMin.SetWindowText( get_string_as_real( pa.valuemin ) );
					m_txtMax.SetWindowText( get_string_as_real( pa.valuemax ) );
					m_fStepSlider = 0;
					if( pa.valuestep > 0 )
					{
						if( ( pa.valuemax - pa.valuemin ) / pa.valuestep >= 65536 )
							m_fRatioSlider = ceil( ( pa.valuemax - pa.valuemin ) / pa.valuestep / 65536 ) * pa.valuestep;
						else
							m_fRatioSlider = pa.valuestep;
					}
					else
					{
						if( ( pa.valuemax - pa.valuemin ) >= 65536 )
							m_fRatioSlider = ceil( ( pa.valuemax - pa.valuemin ) / 65536 );
						else
							m_fRatioSlider = 1;
					}

					m_sliderValue.SetRange( -(long)( pa.valuemax / m_fRatioSlider ), -(long)( pa.valuemin / m_fRatioSlider ) );
					m_sliderValue.SetPos( -(long)( value / m_fRatioSlider ) );
					m_spinValue.SetRange32( (long)( pa.valuemin / m_fRatioSlider ), (long)( pa.valuemax / m_fRatioSlider ) );
					strValue = get_string_as_real( value );
					break;
				}
			}
		}

		if( ! strValue.IsEmpty() && GetFocus() != &m_ebValue )
		{
			m_ebValue.SetWindowText( strValue );
			m_listview.SetItemText( m_editprop.indexOnListview, INDEX_PROPERTY_VALUE, strValue );
		}
	}

	if( m_editprop.idprop != iPropBase )
	{
		m_editprop.idprop = iPropBase;

		update_controls();
	}

	m_bChangingEditbox = FALSE;
}

static void addstring( CListBox& lb, BOOL bAdd, LPCTSTR str )
{
	lb.AddString( bAdd ? str : _T( "" ) );
}

void CDlgDcamProperty::recalc_layout()
{
	GetClientRect( &m_rcClient );

	CRect	rc;
	m_listview.GetWindowRect( &rc );
	ScreenToClient( &rc );

	rc.bottom = m_rcClient.bottom - m_szSpaceListview.cy;
	m_listview.SetWindowPos( NULL, rc.left, rc.top, rc.Width(), rc.Height(), SWP_NOZORDER );

	m_lbValues.GetWindowRect( &rc );
	ScreenToClient( &rc );

	rc.bottom = m_rcClient.bottom - m_szSpaceListview.cy;
	m_lbValues.SetWindowPos( NULL, rc.left, rc.top, rc.Width(), rc.Height(), SWP_NOZORDER );
}

void CDlgDcamProperty::update_controls()
{
	BOOL	bShowValues		= FALSE;
	BOOL	bShowModes		= FALSE;
	BOOL	bEnableArrayElement = FALSE;
	BOOL	bEnableValues	= FALSE;
	BOOL	bEnableModes	= FALSE;

	m_lbAttr.SetRedraw( FALSE );
	m_lbAttr.ResetContent();

	if( m_editprop.attribute != 0 )
	{
		// update attribute listbox
		CString	str;
		switch( m_editprop.attribute & DCAMPROP_TYPE_MASK )
		{
		default:					ASSERT( 0 );
		case DCAMPROP_TYPE_NONE:	str = "(type_none)";		break;
		case DCAMPROP_TYPE_MODE:	str = "TYPE_MODE";		break;
		case DCAMPROP_TYPE_LONG:	str = "TYPE_LONG";		break;
		case DCAMPROP_TYPE_REAL:	str = "TYPE_REAL";		break;
		}

		m_lbAttr.AddString( str );

		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_WRITABLE,		_T( "WRITABLE" ) );
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_READABLE,		_T( "READABLE" ) );
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_EFFECTIVE,	_T( "EFFECTIVE" ) );
		addstring(m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_STEPPING_INCONSISTENT, _T("STEPPING_INCONSISTENT"));
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_DATASTREAM,	_T( "DATASTREAM" ) );
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_ACCESSREADY,	_T( "ACCESSREADY" ) );
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_ACCESSBUSY,	_T( "ACCESSBUSY" ) );
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_HASVIEW,		_T( "HASVIEW" ) );
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_HASCHANNEL,	_T( "HASCHANNEL" ) );
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_VOLATILE,		_T( "VOLATILE" ) );
		addstring( m_lbAttr, m_editprop.attribute & DCAMPROP_ATTR_ACTION,		_T( "ACTION" ) );

		addstring( m_lbAttr, m_editprop.attribute2 & DCAMPROP_ATTR2_ARRAYBASE,	_T( "ARRAYBASE" ) );

		if( m_channelmode == 1 )
		{
		}
		else
		if( m_channelmode > 1 )
		{
			long	iMaxViewChMode;
			long	iFactorIdprop;

			if( m_editprop.nMaxView <= 1 && m_editprop.nMaxChannel <= 1 )
			{
				iMaxViewChMode	= 0;
				iFactorIdprop	= 0;
			}
			else
			if( m_editprop.nMaxView <= 1 )
			{
				ASSERT( m_editprop.nMaxChannel > 1 );
				ASSERT( m_editprop.attribute & DCAMPROP_ATTR_HASCHANNEL );

				iMaxViewChMode	= m_editprop.nMaxChannel;
				iFactorIdprop	= DCAM_IDPROP__CHANNEL;
			}
			else
			{
				ASSERT( m_editprop.nMaxView > 1 );
				ASSERT( m_editprop.nMaxChannel <= 1 );
				ASSERT( m_editprop.attribute & DCAMPROP_ATTR_HASVIEW );

				iMaxViewChMode	= m_editprop.nMaxView;
				iFactorIdprop	= DCAM_IDPROP__VIEW;
			}

			// reset index for channel or view if it is out of range.
			if( m_iChannel > m_channelmode )
				m_iChannel = 0;

			// update buttons
			long	i;
			for( i = 0; i <= m_channelmode; i++ )
			{
				m_btnChannel[ i ].EnableWindow( i <= iMaxViewChMode );
			}

			m_idpropoffset = m_iChannel * iFactorIdprop;
			CheckRadioButton( IDC_DLGDCAMPROPERTY_BTNALL, IDC_DLGDCAMPROPERTY_BTN3, IDC_DLGDCAMPROPERTY_BTNALL + m_iChannel );
		}
		else
		{
			ASSERT( m_channelmode == 0 );

			m_idpropoffset = 0;	// offset for DCAM_IDPROP_*, means basic access
			m_iChannel = 0;
		}

		// update related control to TYPE
		if( m_bUseListboxAlways )
		{
			bShowModes = TRUE;
			if( m_editprop.attribute & DCAMPROP_ATTR_WRITABLE )
			{
				if( m_dcamstatus == DCAMCAP_STATUS_UNSTABLE
				 ||	m_dcamstatus == DCAMCAP_STATUS_STABLE 
				 || m_dcamstatus == DCAMCAP_STATUS_READY && ( m_editprop.attribute & DCAMPROP_ATTR_ACCESSREADY )
				 || m_dcamstatus == DCAMCAP_STATUS_BUSY  && ( m_editprop.attribute & DCAMPROP_ATTR_ACCESSBUSY ) )
				{
					bEnableModes = TRUE;
				}
			}
		}
		else
		{
			switch( m_editprop.attribute & DCAMPROP_TYPE_MASK )
			{
			case DCAMPROP_TYPE_NONE:	break;
			case DCAMPROP_TYPE_MODE:	bShowModes = TRUE;
										if( m_editprop.attribute & DCAMPROP_ATTR_WRITABLE )
										{
											if( m_dcamstatus == DCAMCAP_STATUS_UNSTABLE
											 ||	m_dcamstatus == DCAMCAP_STATUS_STABLE 
											 || m_dcamstatus == DCAMCAP_STATUS_READY && ( m_editprop.attribute & DCAMPROP_ATTR_ACCESSREADY )
											 || m_dcamstatus == DCAMCAP_STATUS_BUSY  && ( m_editprop.attribute & DCAMPROP_ATTR_ACCESSBUSY ) )
											{
												bEnableModes = TRUE;
											}
										}
										break;

			case DCAMPROP_TYPE_LONG:
			case DCAMPROP_TYPE_REAL:	bShowValues = TRUE;
										if( m_editprop.attribute & DCAMPROP_ATTR_WRITABLE )
										{
											if( m_dcamstatus == DCAMCAP_STATUS_UNSTABLE
											 ||	m_dcamstatus == DCAMCAP_STATUS_STABLE 
											 || m_dcamstatus == DCAMCAP_STATUS_READY && ( m_editprop.attribute & DCAMPROP_ATTR_ACCESSREADY )
											 || m_dcamstatus == DCAMCAP_STATUS_BUSY  && ( m_editprop.attribute & DCAMPROP_ATTR_ACCESSBUSY ) )
											{
												bEnableValues = TRUE;
											}
										}
										break;
			}

			if( m_idproparraybase != 0 )
			{
			}
			else
			if( m_editprop.attribute2 & DCAMPROP_ATTR2_ARRAYBASE )
			{
				bEnableArrayElement = TRUE;
			}
		}
	}
	m_lbAttr.SetRedraw( TRUE );
	m_lbAttr.Invalidate( FALSE );

	m_txtMin.ShowWindow( bShowValues );			m_txtMin.EnableWindow( bEnableValues );
	m_txtMax.ShowWindow( bShowValues );			m_txtMax.EnableWindow( bEnableValues );
	m_wndMin.ShowWindow( bShowValues );			m_wndMin.EnableWindow( bEnableValues );
	m_wndMax.ShowWindow( bShowValues );			m_wndMax.EnableWindow( bEnableValues );

	m_sliderValue.ShowWindow( bShowValues );	m_sliderValue.EnableWindow( bEnableValues );
	m_ebValue.ShowWindow( bShowValues );		m_ebValue.EnableWindow( bEnableValues );
	m_spinValue.ShowWindow( bShowValues );		m_spinValue.EnableWindow( bEnableValues );

	m_lbValues.ShowWindow( bShowModes );		m_lbValues.EnableWindow( bEnableModes );

	GetDlgItem( IDC_DLGDCAMPROPERTY_BTNARRAYELEMENT )->EnableWindow( bEnableArrayElement );

	CheckRadioButton( IDC_DLGDCAMPROPERTY_BTNWHOLEIDPROP, IDC_DLGDCAMPROPERTY_BTNARRAYELEMENT
		, m_idproparraybase == 0 ? IDC_DLGDCAMPROPERTY_BTNWHOLEIDPROP : IDC_DLGDCAMPROPERTY_BTNARRAYELEMENT );
}

/////////////////////////////////////////////////////////////////////////////
// CDlgDcamProperty message handlers

#define IDT_UPDATEPROPERTY	1

BOOL CDlgDcamProperty::OnInitDialog() 
{
	CDialog::OnInitDialog();

#ifdef HPKINTERNALUSE //{{HPKINTERNALUSE
	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_COPYPROPERTIES & 0xFFF0) == IDM_COPYPROPERTIES);
	ASSERT(IDM_COPYPROPERTIES < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString str = _T( "Copy &Properties" );
		if (!str.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_COPYPROPERTIES, str);
		}
	}
#endif //}}HPKINTERNALUSE

	// TODO: Add extra initialization here

	GetClientRect( &m_rcClient );
	{
		CRect	rc;
		m_listview.GetWindowRect( &rc );
		ScreenToClient( &rc );
		m_szSpaceListview = CSize( rc.left - m_rcClient.left, rc.top - m_rcClient.top );
	}

	m_listview.InsertColumn( INDEX_PROPERTY_NAME,   _T( "name" ),	LVCFMT_LEFT, 225 );
	m_listview.InsertColumn( INDEX_PROPERTY_UPDATE, _T( "#" ),		LVCFMT_LEFT,  30 );
	m_listview.InsertColumn( INDEX_PROPERTY_VALUE,  _T( "value" ),	LVCFMT_LEFT, 225 );

	m_listview.SetExtendedStyle( LVS_EX_FULLROWSELECT | LVS_EX_GRIDLINES );

	m_editprop.indexOnListview	= -1;
	m_editprop.idprop		= 0;

	if( m_hdcam != NULL )
	{
		update_viewchannel_control();
		update_listview_title( 0, TRUE );
		update_listview_value();
		reset_listview_updated_value();
	}
	update_controls();

	if( m_bUpdatePeriodically )
		SetTimer( IDT_UPDATEPROPERTY, 500, NULL );	// to update property status

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CDlgDcamProperty::OnDestroy() 
{
	CDialog::OnDestroy();
	
	// TODO: Add your message handler code here

	m_bCreateDialog = FALSE;
}

void CDlgDcamProperty::OnSize(UINT nType, int cx, int cy) 
{
	CDialog::OnSize(nType, cx, cy);
	
	// TODO: Add your message handler code here

	if( IsWindow( m_listview.GetSafeHwnd() ) )
	{
		recalc_layout();
	}
}

LRESULT CDlgDcamProperty::OnNcHitTest(CPoint point) 
{
	LRESULT	ht = CDialog::OnNcHitTest(point);

	switch( ht )
	{
	case HTTOPLEFT:
	case HTTOPRIGHT:
		ht = HTTOP;
		break;
	case HTBOTTOMLEFT:
	case HTBOTTOMRIGHT:
		ht = HTBOTTOM;
		break;
	case HTLEFT:
	case HTRIGHT:
		ht = HTCAPTION;
		break;
	}

	return ht;
}

// ----

inline long interpret_scrollmessage( CSliderCtrl& slider, UINT nSBCode, double ratio = 0 )
{
	int		nMin, nMax;
	slider.GetRange( nMin, nMax );
	// flip vertical direction
	{
		long	_nmin = -nMax;
		long	_nmax = -nMin;

		nMin = _nmin;
		nMax = _nmax;
	}

	long	v = -slider.GetPos();
	long	page;
	if( ratio <= 0 )
		page = (nMax - nMin ) / 8;
	else
	if( ratio < 1 )
		page = (long)( (nMax - nMin ) * ratio );
	else
		page = (long)ratio;

	switch( nSBCode )
	{
	case SB_BOTTOM:		v = nMin;	break;	// Scroll to far left.
	case SB_TOP:		v = nMax;	break;	// Scroll to far right.
	case SB_LINEDOWN:	v--;		break;	// Scroll left.
	case SB_LINEUP:		v++;		break;	// Scroll right.
	case SB_PAGEDOWN:	v -= page;	break;	// Scroll one page left.
	case SB_PAGEUP:	v += page;	break;	// Scroll one page right.
	case SB_THUMBPOSITION:	// Scroll to absolute position. The current position is specified by the nPos parameter.
	case SB_THUMBTRACK:	// Drag scroll box to specified position. The current position is specified by the nPos parameter. 
	case SB_ENDSCROLL:	// End scroll.
		break;
	}

	if( v < nMin )
		v = nMin;
	else
	if( v > nMax )
		v = nMax;

	return v;
}

void CDlgDcamProperty::OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar) 
{
	// TODO: Add your message handler code here and/or call default

	if( pScrollBar == (CScrollBar*)&m_sliderValue )
	{
		DCAMERR err;

		BOOL	bValid = FALSE;

		long	pos;
		double	oldValue, newValue;

		err = dcamprop_getvalue( m_hdcam, m_editprop.idprop, &oldValue );
		if( !failed(err) )
		{
			pos = interpret_scrollmessage( m_sliderValue, nSBCode );
			newValue = pos * m_fRatioSlider;
			if( m_fStepSlider != 0 )
			{
				newValue -= fmod( newValue, m_fStepSlider );
			}

			ASSERT( m_hdcam != NULL );
			if( oldValue == newValue )
			{
				bValid = TRUE;
			}
			else
			{
				err = dcamprop_queryvalue( m_hdcam, m_editprop.idprop, &newValue );

				if( !failed(err) )
				{
					bValid = TRUE;
				}
				else
				{
					if( oldValue < newValue )
					{
						err = dcamprop_queryvalue( m_hdcam, m_editprop.idprop, &newValue, DCAMPROP_OPTION_NEXT );
						if( !failed(err) )
							bValid = TRUE;
					}
					else
					{
						ASSERT( newValue < oldValue );
						err = dcamprop_queryvalue( m_hdcam, m_editprop.idprop, &newValue, DCAMPROP_OPTION_PRIOR );
						if( !failed(err) )
							bValid = TRUE;
					}
				}
			}

			if( bValid )
			{
				err = dcamprop_setgetvalue( m_hdcam, m_editprop.idprop, &newValue );
				bValid = ( !failed(err) );
			}
		}

		char	text[ 64 ];
		if( ! bValid )
		{
			value_to_text( m_hdcam, m_editprop.idprop, oldValue, text, sizeof( text ) );
			m_sliderValue.SetPos( -pos );
			//strcpy_s( text, sizeof( text ), "(invalid)" );
		}
		else
		{
			value_to_text( m_hdcam, m_editprop.idprop, newValue, text, sizeof( text ) );
			m_sliderValue.SetPos( -pos );
		}

		CString	str;
		str = text;
		m_ebValue.SetWindowText( str );
		m_listview.SetItemText( m_editprop.indexOnListview, INDEX_PROPERTY_VALUE, str );
	}

	CDialog::OnVScroll(nSBCode, nPos, pScrollBar);
}

/////////////////////////////////////////////////////////////////////////////
// CDlgDcamProperty custom draw function

void CDlgDcamProperty::OnCustomdrawDlgdcampropertyListview(NMHDR* pNMHDR, LRESULT* pResult)
{
	// TODO: Add your control notification handler code here

	NMLVCUSTOMDRAW* lplvcd = (NMLVCUSTOMDRAW*)pNMHDR;

	/*
		CDDS_PREPAINT is at the beginning of the paint cycle. You 
		implement custom draw by returning the proper value. In this 
		case, we're requesting item-specific notifications.
	*/
	if( lplvcd->nmcd.dwDrawStage == CDDS_PREPAINT )
	{
		// Request prepaint notifications for each item.
		*pResult = CDRF_NOTIFYITEMDRAW;
	}
	else
	if( lplvcd->nmcd.dwDrawStage == CDDS_ITEMPREPAINT)
	{
		/*
			Because we returned CDRF_NOTIFYITEMDRAW in response to
			CDDS_PREPAINT, CDDS_ITEMPREPAINT is sent when the control is
			about to paint an item.
		*/

		/*
			To change the font, select the desired font into the 
			provided HDC. We're changing the font for every third item
			in the control, starting with item zero.
		*/

		/*
			To change the text and background colors in a list view 
			control, set the clrText and clrTextBk members of the 
			NMLVCUSTOMDRAW structure to the desired color.

			This differs from most other controls that support 
			CustomDraw. To change the text and background colors for 
			the others, call SetTextColor and SetBkColor on the provided HDC.
		*/

		BOOL	bGrayed;
		long	attr = m_arrayAttr.GetAt( lplvcd->nmcd.dwItemSpec );

		if( ( attr & DCAMPROP_ATTR_WRITABLE ) == 0 )
		{
			bGrayed = TRUE;
		}
		else
		{
			if( m_dcamstatus == DCAMCAP_STATUS_UNSTABLE
			 ||	m_dcamstatus == DCAMCAP_STATUS_STABLE 
			 || m_dcamstatus == DCAMCAP_STATUS_READY && ( attr & DCAMPROP_ATTR_ACCESSREADY )
			 || m_dcamstatus == DCAMCAP_STATUS_BUSY  && ( attr & DCAMPROP_ATTR_ACCESSBUSY ) )
			{
				bGrayed = FALSE;
			}
			else
			{
				bGrayed = TRUE;
			}
		}

		lplvcd->clrText = GetSysColor( bGrayed ? COLOR_GRAYTEXT : COLOR_WINDOWTEXT );

		/*
			We changed the font, so we're returning CDRF_NEWFONT. This
			tells the control to recalculate the extent of the text.
		*/
		*pResult = CDRF_NEWFONT;
	}
	else
		*pResult = 0;
}

/////////////////////////////////////////////////////////////////////////////
// CDlgDcamProperty command handlers

void CDlgDcamProperty::OnOK() 
{
	// TODO: Add extra validation here
/*
	if( m_bCreateDialog )
		DestroyWindow();
	else
		CDialog::OnOK();
*/
}

void CDlgDcamProperty::OnCancel() 
{
	// TODO: Add extra cleanup here
	
	if( m_bCreateDialog )
		DestroyWindow();
	else
		CDialog::OnCancel();
}

#ifdef HPKINTERNALUSE //{{HPKINTERNALUSE
// ----------------------------------------------------------------
void CDlgDcamProperty::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_COPYPROPERTIES )
	{
		if( dcamwin_copyproperties( m_hdcam, GetSafeHwnd() ) )
			AfxMessageBox( _T( "Properties are copied" ) );
		else
			AfxMessageBox( _T( "Failed to copy Properties" ), MB_ICONERROR );
	}
	else
	{
		__super::OnSysCommand(nID, lParam);
	}
}
// ----------------------------------------------------------------
#endif //}}HPKINTERNALUSE


void CDlgDcamProperty::OnItemchangedDlgdcampropertyListview(NMHDR* pNMHDR, LRESULT* pResult) 
{
	NM_LISTVIEW* pNMListView = (NM_LISTVIEW*)pNMHDR;
	// TODO: Add your control notification handler code here
	
	long	i = m_listview.GetNextItem( -1, LVNI_SELECTED );
	if( i >= 0 && m_editprop.indexOnListview != i )
	{
		edit_property_of( i );
	}

	*pResult = 0;
}

void CDlgDcamProperty::OnClickDlgdcampropertyListview(NMHDR* pNMHDR, LRESULT* pResult) 
{
	// TODO: Add your control notification handler code here

	LVHITTESTINFO	lvht;
	memset( &lvht, 0, sizeof( lvht ) );

	GetCursorPos( &lvht.pt );
	m_listview.ScreenToClient( &lvht.pt );

	m_listview.SubItemHitTest( &lvht );
//	if( lvht.iItem >= 0 )
	edit_property_of( lvht.iItem );

	*pResult = 0;
}

void CDlgDcamProperty::OnDblclkDlgdcampropertyListview(NMHDR* pNMHDR, LRESULT* pResult) 
{
	// TODO: Add your control notification handler code here
	
	*pResult = 0;
}

/*
void CDlgDcamProperty::OnDlgdcampropertyBtnshowallproperties() 
{
	// TODO: Add your control notification handler code here

	DWORD	dwExStyle = m_listview.GetExtendedStyle();
	if( m_bShowAllProperties )
	{
		dwExStyle |= LVS_EX_CHECKBOXES;
	}
	else
	{
		dwExStyle &=~LVS_EX_CHECKBOXES;
	}
	m_listview.SetExtendedStyle( dwExStyle );
}
*/

void CDlgDcamProperty::OnDlgdcampropertyBtn( UINT nID )
{
	// TODO: Add your control notification handler code here

	m_iChannel = nID - IDC_DLGDCAMPROPERTY_BTNALL;

	update_controls();

	update_all();
}

void CDlgDcamProperty::OnSelchangeDlgdcampropertyCbselectchl() 
{
	// TODO: Add your control notification handler code here

	m_iChannel = m_cbChannel.GetCurSel();
	m_idpropoffset = (long)m_cbChannel.GetItemData( m_iChannel );

	update_listview_value();
	if( m_editprop.indexOnListview >= 0 )
		edit_property_of( m_editprop.indexOnListview );
}

void CDlgDcamProperty::OnSelchangeDlgdcampropertyLbvalues() 
{
	// TODO: Add your control notification handler code here

	long	i = m_lbValues.GetCurSel();
	ASSERT( i != LB_ERR );
	long	v = (long)m_lbValues.GetItemData( i );

	dcamprop_setvalue( m_hdcam, m_editprop.idprop + m_idpropoffset, v );

	edit_property_of( m_editprop.indexOnListview );

	char	buf[ 256 ];
	value_to_text( m_hdcam, m_editprop.idprop, v, buf, sizeof( buf) );
	CString	str;
	str = buf;
	m_listview.SetItemText( m_editprop.indexOnListview, INDEX_PROPERTY_VALUE, str );

	update_listview_updated_value();
}

void CDlgDcamProperty::OnChangeDlgdcampropertyEbvalue() 
{
	if( ! m_bChangingEditbox )
	{
		CString	str;
		m_ebValue.GetWindowText( str );

		double	v = atof( str );

		DCAMERR err;
		err = dcamprop_queryvalue( m_hdcam, m_editprop.idprop + m_idpropoffset, &v );
		if( !failed(err) )
		{
			err = dcamprop_setgetvalue( m_hdcam, m_editprop.idprop + m_idpropoffset, &v );
			if( !failed(err) )
			{
				char	buf[ 256 ];
				value_to_text( m_hdcam, m_editprop.idprop, v, buf, sizeof( buf) );
				CString	strbuf;
				strbuf = buf;
				m_listview.SetItemText( m_editprop.indexOnListview, INDEX_PROPERTY_VALUE, strbuf );
			}

			edit_property_of( m_editprop.indexOnListview );

			update_listview_updated_value();
		}
	}
}

void CDlgDcamProperty::OnDeltaposDlgdcampropertySpin(NMHDR* pNMHDR, LRESULT* pResult) 
{
	NM_UPDOWN* pNMUpDown = (NM_UPDOWN*)pNMHDR;

	BOOL	bValid = FALSE;

	DCAMERR err;

	long	pos;
	double	oldValue, newValue;

	err = dcamprop_getvalue( m_hdcam, m_editprop.idprop, &oldValue );
	if( !failed(err) )
	{
		pos = pNMUpDown->iDelta;
		newValue = pos * m_fRatioSlider + oldValue;
		if( m_fStepSlider != 0 )
		{
			if (pos > 0)
				newValue += fmod( newValue, m_fStepSlider );
			else
				newValue -= fmod( newValue, m_fStepSlider );
		}

		ASSERT( m_hdcam != NULL );
		if( oldValue == newValue )
		{
			bValid = TRUE;
		}
		else
		{
			if ((m_editprop.attribute & DCAMPROP_ATTR_STEPPING_INCONSISTENT) && (oldValue > newValue))
			{
				newValue = oldValue;
				err = dcamprop_queryvalue(m_hdcam, m_editprop.idprop, &newValue, DCAMPROP_OPTION_PRIOR);
			}
			else
				err = dcamprop_queryvalue( m_hdcam, m_editprop.idprop, &newValue );
			if( !failed(err) )
			{
				bValid = TRUE;
			}
			else
			{
				if( oldValue < newValue )
				{
					err = dcamprop_queryvalue( m_hdcam, m_editprop.idprop, &newValue, DCAMPROP_OPTION_NEXT );
					if( !failed(err) )
						bValid = TRUE;
				}
				else if (newValue < oldValue)
				{
					err = dcamprop_queryvalue( m_hdcam, m_editprop.idprop, &newValue, DCAMPROP_OPTION_PRIOR );
					if( !failed(err) )
						bValid = TRUE;
				}
			}
		}

		if( bValid )
		{
			err = dcamprop_setgetvalue( m_hdcam, m_editprop.idprop, &newValue );
			bValid = ( !failed(err) );
		}
	}

	char	text[ 64 ];
	if( ! bValid )
	{
		value_to_text( m_hdcam, m_editprop.idprop, oldValue, text, sizeof( text ) );
		m_spinValue.SetPos( pos );
	}
	else
	{
		value_to_text( m_hdcam, m_editprop.idprop, newValue, text, sizeof( text ) );
		m_spinValue.SetPos( pos );
	}

	CString	str;
	str = text;
	m_ebValue.SetWindowText( str );
	m_listview.SetItemText( m_editprop.indexOnListview, INDEX_PROPERTY_VALUE, str );
	*pResult = 0;

}

void CDlgDcamProperty::OnDlgdcampropertyBtnuselistbox() 
{
	// TODO: Add your control notification handler code here

	m_bUseListboxAlways = ! m_bUseListboxAlways;

	update_controls();
	edit_property_of( m_editprop.indexOnListview );
}

void CDlgDcamProperty::OnDlgdcampropertyBtnupdateperiodically() 
{
	m_bUpdatePeriodically = ! m_bUpdatePeriodically;

	if( m_bUpdatePeriodically )
		SetTimer( IDT_UPDATEPROPERTY, 500, NULL );
	else
		KillTimer( IDT_UPDATEPROPERTY );
}

void CDlgDcamProperty::OnDlgdcampropertyBtnupdatevalues() 
{
	// TODO: Add your control notification handler code here	
	update_all();
}


void CDlgDcamProperty::OnDlgdcampropertyBtnwholeidprop() 
{
	// TODO: Add your control notification handler code here
	
	if( m_idproparraybase != 0 )
	{
		// find index on the listview.
		long	idprop = m_idproparraybase;

		update_listview_title( 0 );
		update_listview_value();
		reset_listview_updated_value();

		long	iItem;
		for( iItem = m_listview.GetItemCount(); iItem-- >= 0; )
		{
			long	i = (long)m_listview.GetItemData( iItem );
			if( (long)m_arrayIDPROP[ i ] == idprop )
				break;
		}

		edit_property_of( iItem );
		m_listview.SetItemState( iItem, LVIS_SELECTED | LVIS_FOCUSED, LVIS_SELECTED | LVIS_FOCUSED );
	}
}

void CDlgDcamProperty::OnDlgdcampropertyBtnarrayelement() 
{
	// TODO: Add your control notification handler code here

	ASSERT( m_idproparraybase == 0 );

	update_listview_title( m_editprop.idprop );
	update_listview_value();
	reset_listview_updated_value();

	long	iItem = 0;
	edit_property_of( iItem );
	m_listview.SetItemState( iItem, LVIS_SELECTED | LVIS_FOCUSED, LVIS_SELECTED | LVIS_FOCUSED );
}

void CDlgDcamProperty::OnTimer(UINT_PTR nIDEvent) 
{
	// TODO: Add your message handler code here and/or call default

	if( m_bAutomaticUpdatePropertyValues && m_arrayIDPROP.GetSize() > 0 )
	{
		update_listview_value();
	}

	CDialog::OnTimer(nIDEvent);
}

void CDlgDcamProperty::update_all()
{
	if( m_bAutomaticUpdatePropertyValues && m_arrayIDPROP.GetSize() > 0 )
	{
		if( m_bUpdatePeriodically )
			KillTimer( IDT_UPDATEPROPERTY );

		update_listview_updated_value();
		update_listview_value();
		update_controls();

		if( m_bUpdatePeriodically )
			SetTimer( IDT_UPDATEPROPERTY, 500, NULL );
	}
}