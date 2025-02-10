// DlgExcapLUT.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "DlgExcapLUT.h"
#include "luttable.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CDlgExcapLUT dialog


CDlgExcapLUT::CDlgExcapLUT(CWnd* pParent /*=NULL*/)
	: CDialog(CDlgExcapLUT::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDlgExcapLUT)
	m_nInmin = 0;
	m_nInmax = 0;
	//}}AFX_DATA_INIT

	m_luttable			= NULL;
	m_bCreateDialog		= FALSE;
	m_bChangingEditbox	= FALSE;
}

// ----------------

luttable* CDlgExcapLUT::set_luttable( luttable* new_luttable )
{
	luttable*	old = m_luttable;
	m_luttable = new_luttable;

	if( IsWindow( GetSafeHwnd() ) && IsWindowVisible() )
	{
		update_controls();
	}

	return old;
}

BOOL CDlgExcapLUT::toggle_visible()
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
		SetWindowPos( &CWnd::wndTop, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW );
	}

	return TRUE;
}

void CDlgExcapLUT::update_controls()
{
	BOOL	bEnable = ( m_luttable != NULL );

	m_ebInmax.EnableWindow( bEnable );
	m_ebInmin.EnableWindow( bEnable );
	m_sliderInmax.EnableWindow( bEnable );
	m_sliderInmin.EnableWindow( bEnable );

	if( m_luttable != NULL )
	{
		long	bitmask = m_luttable->get_bitmask();

		m_sliderInmax.SetRange( 0, bitmask );
		m_sliderInmin.SetRange( 0, bitmask );

		long	nMax, nMin;

		m_luttable->get_inputrange( nMax, nMin );

		SetDlgItemInt( IDC_EXCAPLUT_EBINMAX, nMax );
		SetDlgItemInt( IDC_EXCAPLUT_EBINMIN, nMin );
		m_sliderInmax.SetPos( nMax );
		m_sliderInmin.SetPos( nMin );
	}
	else
	{
		m_ebInmax.SetWindowText( _T( "" ) );
		m_ebInmin.SetWindowText( _T( "" ) );
	}
}

/////////////////////////////////////////////////////////////////////////////

BOOL CDlgExcapLUT::Create( CWnd* pParentWnd ) 
{
	// TODO: Add your specialized code here and/or call the base class
	
	m_bCreateDialog = CDialog::Create(IDD, pParentWnd );
	return m_bCreateDialog;
}

void CDlgExcapLUT::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDlgExcapLUT)
	DDX_Control(pDX, IDC_EXCAPLUT_EBINMIN, m_ebInmin);
	DDX_Control(pDX, IDC_EXCAPLUT_EBINMAX, m_ebInmax);
	DDX_Control(pDX, IDC_EXCAPLUT_SLIDERINMIN, m_sliderInmin);
	DDX_Control(pDX, IDC_EXCAPLUT_SLIDERINMAX, m_sliderInmax);
	//}}AFX_DATA_MAP

	if( pDX->m_bSaveAndValidate )
	{
		m_nInmin = GetDlgItemInt( IDC_EXCAPLUT_EBINMIN );
		m_nInmax = GetDlgItemInt( IDC_EXCAPLUT_EBINMAX );
	}
	else
	if( m_bChangingEditbox == FALSE )
	{
		m_bChangingEditbox = TRUE;

		SetDlgItemInt( IDC_EXCAPLUT_EBINMIN, m_nInmin );
		SetDlgItemInt( IDC_EXCAPLUT_EBINMAX, m_nInmax );

		m_bChangingEditbox = FALSE;
	}
}


BEGIN_MESSAGE_MAP(CDlgExcapLUT, CDialog)
	//{{AFX_MSG_MAP(CDlgExcapLUT)
	ON_WM_DESTROY()
	ON_WM_HSCROLL()
	ON_EN_CHANGE(IDC_EXCAPLUT_EBINMAX, OnChangeExcaplutEditbox)
	ON_EN_CHANGE(IDC_EXCAPLUT_EBINMIN, OnChangeExcaplutEditbox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CDlgExcapLUT message handlers

BOOL CDlgExcapLUT::OnInitDialog() 
{
	CDialog::OnInitDialog();

	m_bChangingEditbox = FALSE;
	
	update_controls();

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CDlgExcapLUT::OnDestroy() 
{
	CDialog::OnDestroy();
	
	// TODO: Add your message handler code here
	
	m_bCreateDialog = FALSE;
}

void CDlgExcapLUT::OnOK() 
{
	// TODO: Add extra validation here
	
	if( ! m_bCreateDialog )
	{
		// If this dialog is called from CWnd::DoModal(), the main routine should set 
		CDialog::OnOK();
		return;
	}

	if( ! UpdateData() )
		return;

	DestroyWindow();
}

void CDlgExcapLUT::OnCancel() 
{
	// TODO: Add extra cleanup here
	
	if( m_bCreateDialog )
		DestroyWindow();
	else
		CDialog::OnCancel();
}


void CDlgExcapLUT::OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar) 
{
	// TODO: Add your message handler code here and/or call default

	m_nInmax = m_sliderInmax.GetPos();
	m_nInmin = m_sliderInmin.GetPos();

	UpdateData( FALSE );

	m_luttable->set_inputrange( m_nInmax, m_nInmin );

	CDialog::OnHScroll(nSBCode, nPos, pScrollBar);
}

void CDlgExcapLUT::OnChangeExcaplutEditbox() 
{
	// TODO: If this is a RICHEDIT control, the control will not
	// send this notification unless you override the CDialog::OnInitDialog()
	// function and call CRichEditCtrl().SetEventMask()
	// with the ENM_CHANGE flag ORed into the mask.
	
	// TODO: Add your control notification handler code here
	
	if( ! m_bChangingEditbox )
	{
		m_bChangingEditbox = TRUE;

		UpdateData();
		m_luttable->set_inputrange( m_nInmax, m_nInmin );

		m_sliderInmax.SetPos( m_nInmax );
		m_sliderInmin.SetPos( m_nInmin );

		m_bChangingEditbox = FALSE;
	}
}
