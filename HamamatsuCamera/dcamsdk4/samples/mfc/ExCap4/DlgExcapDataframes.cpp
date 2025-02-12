// DlgExcapDataframes.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "DlgExcapDataframes.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CDlgExcapDataframes dialog


CDlgExcapDataframes::CDlgExcapDataframes(CWnd* pParent /*=NULL*/)
	: CDialog(CDlgExcapDataframes::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDlgExcapDataframes)
	m_bUserAttachBuffer = FALSE;
	m_nFrames = 0;
	//}}AFX_DATA_INIT

	m_hdcam = NULL;
	m_nPixeltype = DCAM_PIXELTYPE_NONE;
	m_szData.cx = m_szData.cy = 0;
}


void CDlgExcapDataframes::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDlgExcapDataframes)
	DDX_Control(pDX, IDC_EXCAPDATAFRAMES_CBDATATYPE, m_cbDatatype);
	DDX_Check(pDX, IDC_EXCAPDATAFRAMES_BUSEATTACHBUFFER, m_bUserAttachBuffer);
	DDX_Text(pDX, IDC_EXCAPDATAFRAMES_EBFRAMEPERCYCLE, m_nFrames);
	//}}AFX_DATA_MAP

	DDX_Text(pDX, IDC_EXCAPDATAFRAMES_TXTWIDTH, m_szData.cx );
	DDX_Text(pDX, IDC_EXCAPDATAFRAMES_TXTHEIGHT, m_szData.cy );
}


BEGIN_MESSAGE_MAP(CDlgExcapDataframes, CDialog)
	//{{AFX_MSG_MAP(CDlgExcapDataframes)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CDlgExcapDataframes message handlers

inline void addstring_datatype( CComboBox& cb, DWORD dwItem, LPCTSTR strItem )
{
	long	i = cb.AddString( strItem );
	cb.SetItemData( i, dwItem );
	
}

inline long finditemdata( CComboBox& cb, DWORD dwItemdata )
{
	long	n = cb.GetCount();
	long	i;
	for( i = 0; i < n; i++ )
	{
		if( cb.GetItemData( i ) == dwItemdata )
			return i;
	}

	return LB_ERR;
}

BOOL CDlgExcapDataframes::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	// TODO: Add extra initialization here

	// update combobox for DCAM_DATATYPE
	{
		DCAMERR err;

		double	v;
		err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, &v );

		switch ((int32)v)
		{
		case DCAM_PIXELTYPE_MONO8:
			addstring_datatype( m_cbDatatype,  DCAM_PIXELTYPE_MONO8,	_T( "DCAM_PIXELTYPE_MONO8" ) );
			break;
		case DCAM_PIXELTYPE_MONO16:
			addstring_datatype( m_cbDatatype,  DCAM_PIXELTYPE_MONO16,	_T( "DCAM_PIXELTYPE_MONO16") );
			break;
		case DCAM_PIXELTYPE_MONO12:
			addstring_datatype( m_cbDatatype,  DCAM_PIXELTYPE_MONO12,	_T( "DCAM_PIXELTYPE_MONO12") );
			break;
		case DCAM_PIXELTYPE_RGB24:
			addstring_datatype( m_cbDatatype,  DCAM_PIXELTYPE_RGB24,	_T( "DCAM_PIXELTYPE_RGB24" ) );
			break;
		case DCAM_PIXELTYPE_RGB48:
			addstring_datatype( m_cbDatatype,  DCAM_PIXELTYPE_RGB48,	_T( "DCAM_PIXELTYPE_RGB48" ) );
			break;
		case DCAM_PIXELTYPE_BGR24:
			addstring_datatype( m_cbDatatype,  DCAM_PIXELTYPE_BGR24,	_T( "DCAM_PIXELTYPE_BGR24" ) );
			break;
		case DCAM_PIXELTYPE_BGR48:
			addstring_datatype( m_cbDatatype,  DCAM_PIXELTYPE_BGR48,	_T( "DCAM_PIXELTYPE_BGR48" ) );
			break;
		}

		long	i = finditemdata( m_cbDatatype, m_nPixeltype );
		m_cbDatatype.SetCurSel( i );

		m_cbDatatype.EnableWindow( FALSE );
	}
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CDlgExcapDataframes::OnOK() 
{
	// TODO: Add extra validation here

	CDialog::OnOK();
}
