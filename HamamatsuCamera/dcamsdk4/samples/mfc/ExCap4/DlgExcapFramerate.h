// DlgExcapFramerate.h : header file
//

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CDlgExcapFramerate dialog

class CDlgExcapFramerate : public CDialog
{
// Construction
public:
	CDlgExcapFramerate(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CDlgExcapFramerate)
	enum { IDD = IDD_EXCAPFRAMERATE };
	CButton	m_btnFiretriggerRepeatedly;
	CButton	m_btnExposureEnd;
	CButton	m_btnTransferred;
	CButton	m_btnFrameReady;
	CButton	m_btnStopped;
	CString	m_strAverageFps;
	CString	m_strAveragePeriod;
	CString	m_strFastestFps;
	CString	m_strFastestPeriod;
	CString	m_strLatestFps;
	CString	m_strLatestPeriod;
	CString	m_strSlowestFps;
	CString	m_strSlowestPeriod;
	int m_bUseEventExposureEnd;
	int m_bUseEventTransferred;
	int m_bUseEventFrameReady;
	int m_bUseEventStopped;
	//}}AFX_DATA

// Common DCAM Dialog Data in ExCap
protected:
	BOOL		m_bCreateDialog;
	HDCAM		m_hdcam;
	HDCAMWAIT   m_hwait;
	int32		m_supportevents;

// Private Dialog Data
protected:
	class CExCapFramerate*		m_framerate;
	class CExCapFiretrigger*	m_firetrigger;
	class CExCapCallback_DlgExcapFramerate*		m_callback;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDlgExcapFramerate)
	public:
	virtual BOOL Create( CWnd* pParentWnd = NULL );
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation

// Common implementation of DCAM Dialog in ExCap
public:
			BOOL	toggle_visible();
			HDCAM	set_hdcamwait( HDCAM hdcam, HDCAMWAIT hdcamwait, int32 supportevents );
			BOOL	is_visible()	{ return IsWindow( GetSafeHwnd() ) && IsWindowVisible(); }

protected:
	void	update_controls( BOOL b, double period, CString& strPeriod, CString& strFps );

	void	set_eventmask();

	// Generated message map functions
	//{{AFX_MSG(CDlgExcapFramerate)
	virtual BOOL OnInitDialog();
	afx_msg void OnDestroy();
	virtual void OnOK();
	virtual void OnCancel();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnExcapframerateBtnrepeatedlyfiretrigger();
	afx_msg void OnExcapframerateBtnresetframecount();
	afx_msg void OnExcapframerateBtnchangedeventmask( UINT id );
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
