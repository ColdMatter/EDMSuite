
// ChildFrm.cpp : implementation of the CChildFrame class
//

#include "stdafx.h"
#include "ExCap4.h"

#include "ChildFrm.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CChildFrame
static UINT indicators[] =
{
	//ID_SEPARATOR,           // status line indicator
	ID_INDICATOR_CURRENTFRAME,	
	ID_INDICATOR_FRAMES,	
	ID_INDICATOR_ZOOM	
};

IMPLEMENT_DYNCREATE(CChildFrame, CMDIChildWnd)

BEGIN_MESSAGE_MAP(CChildFrame, CMDIChildWnd)
	ON_WM_CREATE()
END_MESSAGE_MAP()

// CChildFrame construction/destruction

CChildFrame::CChildFrame()
{
}

CChildFrame::~CChildFrame()
{
}


BOOL CChildFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: Modify the Window class or styles here by modifying the CREATESTRUCT cs
	if( !CMDIChildWnd::PreCreateWindow(cs) )
		return FALSE;

	return TRUE;
}

// CChildFrame diagnostics

#ifdef _DEBUG
void CChildFrame::AssertValid() const
{
	CMDIChildWnd::AssertValid();
}

void CChildFrame::Dump(CDumpContext& dc) const
{
	CMDIChildWnd::Dump(dc);
}
#endif //_DEBUG

// CChildFrame message handlers


int CChildFrame::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CMDIChildWnd::OnCreate(lpCreateStruct) == -1)
		return -1;

	if (!m_wndToolBar.CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_BOTTOM
		| CBRS_GRIPPER | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) ||
		!m_wndToolBar.LoadToolBar(IDR_TOOLBAR_PLAY))
	{
		TRACE0("Failed to create toolbar\n");
		return -1;      // fail to create
	}
	if (!m_wndStatusBar.CreateEx(this,SBARS_SIZEGRIP ) ||
		!m_wndStatusBar.SetIndicators(indicators,
		  sizeof(indicators)/sizeof(UINT)))
	{
		TRACE0("Failed to create status bar\n");
		return -1;      // fail to create
	}

	m_wndStatusBar.SetPaneInfo(0, ID_INDICATOR_CURRENTFRAME, SBPS_STRETCH, 0);
	m_wndStatusBar.SetPaneInfo(1, ID_INDICATOR_FRAMES, SBPS_NORMAL, 200);
	m_wndStatusBar.SetPaneInfo(2, ID_INDICATOR_ZOOM, SBPS_NORMAL, 200);

	return 0;
}

void CChildFrame::UpdateZoomStatus(double fzoom)
{
	int izoom = static_cast<int>(fzoom);		
	CString str;
	if(izoom > 0)
		str.Format(_T("Zoom: %d%%"), izoom);
	else str = "Zoom:";

	m_wndStatusBar.SetPaneText(2,str);
}
