// DlgDcamOpen.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"

#include "DlgDcamOpen.h"
#include "DlgDcamAbout.h"
#include "showdcamerr.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// ----------------------------------------------------------------

static CDlgDcamOpen	aDlgDcamOpen;

UINT CDlgDcamOpen::dcam_init_and_open( HDCAM& hdcam, long* option )
{
	return aDlgDcamOpen.init_and_open( hdcam, option );
}
// ----------------------------------------------------------------
// CDlgDcamOpen dialog

CDlgDcamOpen::~CDlgDcamOpen()
{
	if( m_bDcamInitialized )
	{
		dcamapi_uninit();
	}
}

CDlgDcamOpen::CDlgDcamOpen(CWnd* pParent /*=NULL*/)
	: CDialog(CDlgDcamOpen::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDlgDcamOpen)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT

	m_bCreated = FALSE;
	m_bDcamInitialized = FALSE;
	m_result = 0;
	m_nDevice = 0;
	m_strStatusForDcamInit = _T( "" );
}

BOOL CDlgDcamOpen::Create( CWnd* pParentWnd ) 
{
	if( ! CDialog::Create(IDD, pParentWnd) )
		return FALSE;

	m_bCreated = TRUE;
	return TRUE;
}

void CDlgDcamOpen::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDlgDcamOpen)
	DDX_Control(pDX, IDC_DLGDCAMOPEN_BTNDETAIL, m_btnDetail);
	DDX_Control(pDX, IDOK, m_btnOK);
	DDX_Control(pDX, IDCANCEL, m_btnCancel);
	DDX_Control(pDX, IDRETRY, m_btnRetry);
	DDX_Control(pDX, IDC_DLGDCAMOPEN_TXTSTATUS, m_txtStatus);
	DDX_Control(pDX, IDC_DLGDCAMOPEN_TXTCAMERANAME, m_txtCameraName);
	DDX_Control(pDX, IDC_DLGDCAMOPEN_CBCAMERANAMES, m_cbCameraNames);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CDlgDcamOpen, CDialog)
	//{{AFX_MSG_MAP(CDlgDcamOpen)
	ON_BN_CLICKED(IDRETRY, OnRetry)
	ON_BN_CLICKED(IDC_DLGDCAMOPEN_BTNDETAIL, OnDlgdcamopenBtndetail)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

UINT CDlgDcamOpen::init_and_open( HDCAM& hdcam, long* option )
{
	UINT	result = IDCANCEL;
	BOOL	bNowInitialized = FALSE;

	if( option == NULL || *option == '\0' )
		m_strStatusForDcamInit.Empty();
	else
	{
		CString strOption;
		switch (*option)
		{
		case DCAMAPI_INITOPTION_APIVER__4_0:
			strOption = _T("DCAMAPI_INITOPTION_APIVER__4_0");
			break;
		case DCAMAPI_INITOPTION_APIVER__LATEST:
			strOption = _T("DCAMAPI_INITOPTION_APIVER__LATEST");
			break;
		case DCAMAPI_INITOPTION_MULTIVIEW__DISABLE:
			strOption = _T("DCAMAPI_INITOPTION_MULTIVIEW__DISABLE");
			break;
		default:
			strOption.Format(_T("%d"), *option);
			break;
		}
		m_strStatusForDcamInit.Format( _T( "with \"%s\" option" ), static_cast<LPCTSTR>(strOption) );
	}

	m_result = 0;
	VERIFY( Create() );

	DCAMAPI_INIT paraminit;
	memset( &paraminit, 0, sizeof(paraminit) );
	paraminit.size = sizeof(paraminit);
	paraminit.initoption = option;
	paraminit.initoptionbytes = sizeof(option);

	for( ;; )
	{
		long	index;

		if( ! m_bDcamInitialized )
		{
			DCAMERR err;
			err = dcamapi_init(&paraminit);
			if( failed(err) || paraminit.iDeviceCount == 0 )
			{
				// fail dcamapi_init()

				dcamapi_uninit();
				result = on_fail_dcaminit();
				index = -1;	// without device;
			}
			else
			{
				m_nDevice = paraminit.iDeviceCount;
				m_bDcamInitialized = TRUE;
				bNowInitialized = TRUE;

				m_saModelInfo.RemoveAll();

				long	i;
				for (i = 0; i < m_nDevice; i++)
				{
					CString	str;
					{
						char buf[256] = { '\0' };
						DCAMDEV_STRING param;
						param.size = sizeof(param);
						param.text = buf;
						param.textbytes = 256;
						param.iString = DCAM_IDSTR_MODEL;
						err = dcamdev_getstring((HDCAM)(INT_PTR)i, &param);
						VERIFY(!failed(err));
						CString	strModel(buf);

						param.iString = DCAM_IDSTR_CAMERAID;
						dcamdev_getstring((HDCAM)(INT_PTR)i, &param);
						CString	strCameraId(buf);

						param.iString = DCAM_IDSTR_BUS;
						err = dcamdev_getstring((HDCAM)(INT_PTR)i, &param);
						VERIFY(!failed(err));
						CString	strBus(buf);

						str = strModel
							+ _T(" (")
							+ strCameraId
							+ _T(") on ")
							+ strBus
							;
					}
					m_saModelInfo.Add(str);
				}
			}
		}

		if( m_bDcamInitialized )
		{
			// show camera names
			setup_cameralist();

			if( m_nDevice == 1 )
			{
				index = 0;
				result = IDOK;

				UpdateWindow();
			}
			else
			{
				index = 0;
				result = choose_cameraname( index );
			}
		}

		if( result == IDCANCEL )
		{
			// close dialog

			if( bNowInitialized )
			{
				dcamapi_uninit();
				m_bDcamInitialized = FALSE;
			}

			break;
		}

		if( result == IDRETRY )
		{
			// retry initialization

			clear_textbox();

			if( bNowInitialized )
			{
				dcamapi_uninit();
				m_bDcamInitialized = FALSE;
			}

			continue;
		}

		if( index >= 0 )
		{
			ASSERT( result == IDOK );
			DCAMDEV_OPEN paramopen;
			memset( &paramopen, 0, sizeof(paramopen) );
			paramopen.size = sizeof(paramopen);
			paramopen.index = index;

			DCAMERR err = dcamdev_open(&paramopen);
			if (failed(err))
			{
				show_dcamerrorbox((HDCAM)(INT_PTR)paramopen.index, err, "dcamdev_open()");

				result = IDCANCEL;
				hdcam = NULL;

				if (m_nDevice == 1)
					break;

				continue;
			}
			hdcam = paramopen.hdcam;
		}

		break;
	}

	DestroyWindow();

	return result;
}

// ----------------------------------------------------------------
// CDlgDcamOpen message handlers

void CDlgDcamOpen::clear_textbox()
{
	m_txtStatus.SetWindowText( m_strStatusForDcamInit );

	m_txtCameraName.ShowWindow( SW_HIDE );
	m_cbCameraNames.ShowWindow( SW_HIDE );
	m_cbCameraNames.ResetContent();

	m_btnDetail.ShowWindow( SW_HIDE );
	m_btnOK.ShowWindow( SW_HIDE );
	m_btnCancel.ShowWindow( SW_HIDE );
	m_btnRetry.ShowWindow( SW_HIDE );

	m_result = 0;
}

UINT CDlgDcamOpen::on_fail_dcaminit()
{
	CString	str;
	str.LoadString( IDS_DLGDCAMOPEN_FAILDCAMINIT );
	m_txtStatus.SetWindowText( str );

	m_btnDetail.ShowWindow( SW_HIDE );
	m_btnOK.ShowWindow( SW_HIDE );
	m_btnCancel.ShowWindow( SW_SHOW );
	m_btnRetry.ShowWindow( SW_SHOW );

	MSG	msg;
	while( GetMessage( &msg, GetSafeHwnd(), 0, 0 ) )
	{
		if( ! IsDialogMessage( &msg ) )
		{
			TranslateMessage( &msg );
			DispatchMessage( &msg );
		}

		if( m_result != 0 )
			return m_result;
	}

	return IDCANCEL;
}

void CDlgDcamOpen::setup_cameralist()
{
	CString	strStatus;

	m_cbCameraNames.ResetContent();

	if( m_nDevice == 1 )
	{
		strStatus.LoadString( IDS_DLGDCAMOPEN_FINDADEVICE );

		CString	model = m_saModelInfo.GetAt( 0 );
		m_txtCameraName.SetWindowText( model );

		m_txtCameraName.ShowWindow( SW_SHOW );
		m_cbCameraNames.ShowWindow( SW_HIDE );
	}
	else if( m_nDevice > 1 )
	{
		strStatus.LoadString( IDS_DLGDCAMOPEN_FINDDEVICESANDCHOOSE );

		long	i;
		for( i = 0; i < m_nDevice; i++ )
		{
			CString	model = m_saModelInfo.GetAt( i );
			CString	str;
			str.Format( _T( "%d: %s" ), i, static_cast<LPCTSTR>(model) );
			m_cbCameraNames.AddString( str );
		}

		m_txtCameraName.ShowWindow( SW_HIDE );
		m_cbCameraNames.ShowWindow( SW_SHOW );
	}
	else
	{
		strStatus.LoadString( IDS_DLGDCAMOPEN_FINDNODEVICE );
	}

	m_txtStatus.SetWindowText( strStatus );
}

UINT CDlgDcamOpen::choose_cameraname( long& index )
{
	m_result = 0;
	m_cbCameraNames.SetCurSel( index );

	m_btnDetail.ShowWindow( SW_SHOW );
	m_btnOK.ShowWindow( SW_SHOW );
	m_btnCancel.ShowWindow( SW_SHOW );

	MSG	msg;
	while( GetMessage( &msg, NULL, 0, 0 ) )
	{
		if( ! IsDialogMessage( &msg ) )
		{
			TranslateMessage( &msg );
			DispatchMessage( &msg );
		}

		if( m_result != 0 )
		{
			index = m_cbCameraNames.GetCurSel();
			return m_result;
		}
	}

	return IDCANCEL;
}

// ================================================================
// CDlgDcamOpen message handlers


BOOL CDlgDcamOpen::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	CenterWindow( GetParentFrame() );

	m_txtStatus.SetWindowText( m_strStatusForDcamInit );
	m_txtCameraName.SetWindowText( _T( "" ) );

	m_txtCameraName.ShowWindow( SW_HIDE );
	m_cbCameraNames.ShowWindow( SW_HIDE );

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CDlgDcamOpen::OnOK()		{	m_result = IDOK;		}
void CDlgDcamOpen::OnCancel()	{	m_result = IDCANCEL;	}
void CDlgDcamOpen::OnRetry()	{	m_result = IDRETRY;		}

void CDlgDcamOpen::OnDlgdcamopenBtndetail() 
{
	// TODO: Add your control notification handler code here

	CDlgDcamAbout	dlg( m_cbCameraNames.GetCurSel() );

	dlg.DoModal();
}
