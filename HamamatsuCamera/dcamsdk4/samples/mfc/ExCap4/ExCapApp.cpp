
// ExCapApp.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "afxwinappex.h"
#include "afxdialogex.h"
#include "ExCap4.h"
#include "ExCapApp.h"
#include "MainFrm.h"

#include "ChildFrm.h"
#include "ExCapDoc.h"
#include "ExCapView.h"

#include "DlgDcamAbout.h"
#include "DlgDcamOpen.h"
#include "DlgDcamProperty.h"
#include "DlgExcapLUT.h"

#include "DlgExcapFramerate.h"

#include "luttable.h"
#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// inlines

inline BOOL is_window_visible( CWnd* pWnd )
{
	return IsWindow( pWnd->GetSafeHwnd() ) && pWnd->IsWindowVisible();
}

inline void destroy_dialog( CDialog* pDlg )
{
	if( IsWindow( pDlg->GetSafeHwnd() ) )
		pDlg->DestroyWindow();
}

// CExCapApp

IMPLEMENT_DYNAMIC(CExCapApp, CWinApp)

BEGIN_MESSAGE_MAP(CExCapApp, CWinApp)
	ON_COMMAND(ID_FILE_OPEN, &CExCapApp::OnFileOpen)
	ON_COMMAND(ID_APP_ABOUT, &CExCapApp::OnAppAbout)
	ON_UPDATE_COMMAND_UI(ID_SETUP_PROPERTIES, OnUpdateSetupProperties)
	ON_COMMAND(ID_SETUP_PROPERTIES, OnSetupProperties)
	ON_COMMAND(ID_VIEW_LUT, OnViewLut)
	ON_UPDATE_COMMAND_UI(ID_VIEW_LUT, OnUpdateViewLut)
	ON_COMMAND(ID_VIEW_FRAMERATE, OnViewFramerate)
	ON_UPDATE_COMMAND_UI(ID_VIEW_FRAMERATE, OnUpdateViewFramerate)
	// Standard file based document commands
	ON_COMMAND(ID_FILE_NEW, &CWinApp::OnFileNew)
END_MESSAGE_MAP()


// CExCapApp construction

CExCapApp::~CExCapApp()
{
	if( m_dlg.property	!= NULL )	delete m_dlg.property;
	if( m_dlg.lut		!= NULL )	delete m_dlg.lut;
	if( m_dlg.framerate	!= NULL )	delete m_dlg.framerate;
}

CExCapApp::CExCapApp()
{
	// TODO: replace application ID string below with unique ID string; recommended
	// format for string is CompanyName.ProductName.SubProduct.VersionInformation
#if _MSC_VER >= 1600
	SetAppID(_T("ExCap.AppID.NoVersion"));
#endif

	memset( &m_active, 0, sizeof( m_active ) );
	memset( &m_dlg, 0, sizeof( m_dlg ) );
	memset( &m_available, 0, sizeof( m_available ) );

	// initialize dialogs
	m_dlg.property	= new CDlgDcamProperty;
	m_dlg.lut		= new CDlgExcapLUT;
	m_dlg.framerate	= new CDlgExcapFramerate;
}


// The one and only CExCapApp object

CExCapApp theApp;


// CExCapApp initialization

BOOL CExCapApp::InitInstance()
{
	// InitCommonControlsEx() is required on Windows XP if an application
	// manifest specifies use of ComCtl32.dll version 6 or later to enable
	// visual styles.  Otherwise, any window creation will fail.
	INITCOMMONCONTROLSEX InitCtrls;
	InitCtrls.dwSize = sizeof(InitCtrls);
	// Set this to include all the common control classes you want to use
	// in your application.
	InitCtrls.dwICC = ICC_WIN95_CLASSES;
	InitCommonControlsEx(&InitCtrls);
		
	CWinApp::InitInstance();


	// Initialize OLE libraries
	if (!AfxOleInit())
	{
		AfxMessageBox(IDP_OLE_INIT_FAILED);
		return FALSE;
	}

	AfxEnableControlContainer();

#if _MFC_VER >= 0x1000 
	EnableTaskbarInteraction(FALSE);
#endif

	// AfxInitRichEdit2() is required to use RichEdit control	
	// AfxInitRichEdit2();

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	// of your final executable, you should remove from the following
	// the specific initialization routines you do not need.

	// Change the registry key under which our settings are stored.
	// TODO: You should modify this string to be something appropriate
	// such as the name of your company or organization.
	SetRegistryKey(_T("Hamamatsu"));
	LoadStdProfileSettings();  // Load standard INI file options (including MRU)

	{
		long data;
		data = GetProfileInt( _T("Settings"), _T("option.dcamapiinit"), DCAMAPI_INITOPTION_APIVER__LATEST );
		m_option.dcaminit[0] = data;
		m_option.dcaminit[1] = 0;
	}

	// Register the application's document templates.  Document templates
	//  serve as the connection between documents, frame windows and views.

	CMultiDocTemplate* pDocTemplate;
	pDocTemplate = new CMultiDocTemplate(
		IDR_EXCAPTYPE,
		RUNTIME_CLASS(CExCapDoc),
		RUNTIME_CLASS(CChildFrame), // custom MDI child frame
		RUNTIME_CLASS(CExCapView));
	if (!pDocTemplate)
		return FALSE;
	AddDocTemplate(pDocTemplate);
	
	// create main MDI Frame window
	CMainFrame* pMainFrame = new CMainFrame;
	if (!pMainFrame || !pMainFrame->LoadFrame(IDR_MAINFRAME))
	{
		delete pMainFrame;
		return FALSE;
	}
	m_pMainWnd = pMainFrame;
	// call DragAcceptFiles only if there's a suffix
	//  In an MDI app, this should occur immediately after setting m_pMainWnd

	// Parse command line for standard shell commands, DDE, file open
	CCommandLineInfo cmdInfo;
	ParseCommandLine(cmdInfo);
//	if( cmdInfo.m_nShellCommand == CCommandLineInfo::FileNew )
//		cmdInfo.m_nShellCommand = CCommandLineInfo::FileNothing;

	// Dispatch commands specified on the command line.  Will return FALSE if
	// app was launched with /RegServer, /Register, /Unregserver or /Unregister.
	if (!ProcessShellCommand(cmdInfo))
		return FALSE;

#ifdef _EXCAP_SUPPORTS_VIEWS_
	MenuMultiview.CreateMenu();
	MenuMultiview.AppendMenu( MF_STRING | MF_CHECKED,	ID_MULTIVIEWSHOWIMAGE_VIEW1,	_T("View1") );
	MenuMultiview.AppendMenu( MF_STRING | MF_UNCHECKED,	ID_MULTIVIEWSHOWIMAGE_VIEW2,	_T("View2") );
	MenuMultiview.AppendMenu( MF_STRING | MF_UNCHECKED,	ID_MULTIVIEWSHOWIMAGE_ALLVIEW,	_T("All View") );


	CMenu* pTopMenu = AfxGetMainWnd()->GetMenu();
	CMenu* pViewMenu = pTopMenu->GetSubMenu(1);
	pViewMenu->AppendMenu( MF_STRING | MF_POPUP, (UINT_PTR)MenuMultiview.GetSafeHmenu(), _T("MultiView Show Image") );
#endif // _EXCAP_SUPPORTS_VIEWS_ !
	// The main window has been initialized, so show and update it
	pMainFrame->ShowWindow(m_nCmdShow);
	pMainFrame->UpdateWindow();
	// Enable drag/drop open
	pMainFrame->DragAcceptFiles();

	return TRUE;
}

// CExCapApp message handlers

int CExCapApp::ExitInstance() 
{
	// Destroy dialogs
	destroy_dialog( m_dlg.property );
	destroy_dialog( m_dlg.lut );
	destroy_dialog( m_dlg.framerate );

	//TODO: handle additional resources you may have added
	AfxOleTerm(FALSE);

	return CWinApp::ExitInstance();
}

void CExCapApp::get_active_objects( HDCAM& hdcam, HDCAMWAIT& hwait, CExCapDoc*& doc, luttable*& lut ) const
{
	hdcam	= m_active.hdcam;
	hwait   = m_active.hwait;
	doc		= m_active.docForHDCAM;
	lut		= m_active.lut;
}

void CExCapApp::set_active_objects( HDCAM hdcam,  HDCAMWAIT hwait, CExCapDoc* doc, luttable* lut ) 
{
	if( m_active.hdcam != hdcam
	 || m_active.docForHDCAM != doc )
	{
		m_active.hdcam = hdcam;
		m_active.docForHDCAM = doc;
		m_active.hwait = hwait;

		if( hdcam == NULL )
		{
			memset( m_active.dcamapiver, 0, sizeof( m_active.dcamapiver ) );
			memset( m_active.cameraname, 0, sizeof( m_active.cameraname ) );
		}
		else
		{
			DCAMERR err;

			DCAMDEV_STRING param;
			param.size = sizeof(param);
			param.text = m_active.dcamapiver;
			param.textbytes = sizeof(m_active.dcamapiver);
			param.iString = DCAM_IDSTR_DCAMAPIVERSION;

			err = dcamdev_getstring( hdcam, &param );
			VERIFY( !failed(err) );

			param.text = m_active.cameraname;
			param.textbytes = sizeof(m_active.cameraname);
			param.iString = DCAM_IDSTR_MODEL;

			err = dcamdev_getstring( hdcam, &param );
			VERIFY( !failed(err) );
		}

		m_dlg.property ->set_hdcam( hdcam );
		m_dlg.framerate->set_hdcamwait( hdcam, hwait, (doc ? doc->get_suportevents() : 0) );

	}

	if( m_active.lut != lut )
	{
		m_active.lut = lut;
		m_dlg.lut->set_luttable( lut );
	}
}

void CExCapApp::update_availables() 
{
	m_available.property	= TRUE;
	m_available.framerate	= TRUE;
	m_available.status		= TRUE;

	m_available.general		= TRUE;

	m_available.subarray	= TRUE;
}	

void CExCapApp::on_close_document( CExCapDoc* doc )
{
	ASSERT( doc != NULL );
	if( m_active.docForHDCAM == doc )
	{
		set_active_objects( NULL, NULL, NULL, NULL );
		update_availables();
	}
}

long CExCapApp::suspend_capturing()
{
	if( m_active.docForHDCAM == NULL )
		return 0;

	return m_active.docForHDCAM->suspend_capturing();
}

void CExCapApp::resume_capturing( long param )
{
	if( m_active.docForHDCAM != NULL )
		m_active.docForHDCAM->resume_capturing( param );
}

long CExCapApp::number_of_visible_controldialogs()
{
	long	nShown = 0;

	if( is_window_visible( m_dlg.property ) )	nShown++;

	return nShown;
}

long* CExCapApp::get_dcaminit_option()
{
	return m_option.dcaminit;
}

// CExCapApp message handlers

void CExCapApp::OnAppAbout() 
{
	// TODO: Add your command handler code here
	
	CDlgDcamAbout	dlg( m_active.hdcam );

	dlg.DoModal();
}

void CExCapApp::OnFileOpen() 
{
	// setup the filters
	CString strFilter;
	CString tempFilter;

#define IDCNT	6
	UINT nIDs[] = {IDS_FILTER_EXCAP, IDS_EXT_EXCAP, IDS_FILTER_DCIMG, IDS_EXT_DCIMG, IDS_FILTER_ALL, IDS_EXT_ALL};
	for (int i=0; i<IDCNT; i++)
	{
		VERIFY(tempFilter.LoadString(nIDs[i]));
		strFilter += tempFilter;
		if ((i % 2) == 0)
			strFilter += (TCHAR)'\0';  // next string please
		else
			strFilter += (TCHAR)'\0\0';  // end filter
	}
	
	
	CFileDialog dlgFile(TRUE, NULL, NULL, OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT | OFN_FILEMUSTEXIST, strFilter, NULL, 0);

	CString title;
	ENSURE(title.LoadString(AFX_IDS_OPENFILE));

	dlgFile.m_ofn.lpstrFilter = strFilter;
	dlgFile.m_ofn.lpstrTitle = title;

	INT_PTR nResult = dlgFile.DoModal();


	if ( nResult != IDOK )
		return; // open cancelled

	CString newName = dlgFile.GetPathName();
	OpenDocumentFile(newName);
}

void CExCapApp::OnUpdateSetup(CCmdUI* pCmdUI, CDialog* dlg, BOOL bAvailable ) 
{
	BOOL	bEnable = FALSE;
	BOOL	bCheck	= FALSE;

	if( dlg != NULL && m_active.hdcam != NULL )
	{
		long	nShown = number_of_visible_controldialogs();

		if( is_window_visible( dlg ) )
		{
			ASSERT( nShown > 0 );
			nShown--;
			bCheck = TRUE;
		}

		if( nShown == 0 )
			bEnable = bAvailable;
	}

	pCmdUI->Enable( bEnable );
	pCmdUI->SetCheck( bCheck );
}

// ----

void CExCapApp::OnUpdateSetupProperties(CCmdUI* pCmdUI) 
{
	OnUpdateSetup( pCmdUI, m_dlg.property, m_available.property );
}

void CExCapApp::OnSetupProperties() 
{
	ASSERT( m_dlg.property != NULL );
	m_dlg.property->toggle_visible();
}

void CExCapApp::OnUpdateViewLut(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_active.lut != NULL );
}

void CExCapApp::OnViewLut() 
{
	ASSERT( m_dlg.lut != NULL );
	m_dlg.lut->toggle_visible();
}

void CExCapApp::OnUpdateViewFramerate(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_available.framerate );
	pCmdUI->SetCheck( m_dlg.framerate->is_visible() );
}

void CExCapApp::OnViewFramerate() 
{
	ASSERT( m_dlg.framerate != NULL );
	m_dlg.framerate->toggle_visible();
}
