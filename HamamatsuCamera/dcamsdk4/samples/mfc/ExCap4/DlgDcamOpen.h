// DlgDcamOpen.h : header file
//

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CDlgDcamOpen dialog

class CDlgDcamOpen : public CDialog
{
// Construction
public:
	~CDlgDcamOpen();
	CDlgDcamOpen(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CDlgDcamOpen)
	enum { IDD = IDD_DLGDCAMOPEN };
	CButton	m_btnDetail;
	CButton	m_btnOK;
	CButton	m_btnCancel;
	CButton	m_btnRetry;
	CStatic	m_txtStatus;
	CStatic	m_txtCameraName;
	CComboBox	m_cbCameraNames;
	//}}AFX_DATA

protected:
	BOOL	m_bCreated;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDlgDcamOpen)
	public:
	virtual BOOL Create( CWnd* pParentWnd = NULL );
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	void	clear_textbox();
	UINT	on_fail_dcaminit();

	void	setup_cameralist();
	UINT	choose_cameraname( long& index );

public:
	static	UINT dcam_init_and_open( HDCAM& hdcam, long* option = NULL );
	UINT	init_and_open( HDCAM& hdcam, long* option = NULL );
// Generated message map functions
protected:
	//{{AFX_MSG(CDlgDcamOpen)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	virtual void OnCancel();
	afx_msg void OnIgnore();
	afx_msg void OnRetry();
	afx_msg void OnDlgdcamopenBtndetail();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// local variables
protected:
	BOOL	m_bCreate;
	BOOL	m_bDcamInitialized;
	UINT	m_result;
	int32	m_nDevice;
	CString	m_strStatusForDcamInit;
	CStringArray	m_saModelInfo;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
