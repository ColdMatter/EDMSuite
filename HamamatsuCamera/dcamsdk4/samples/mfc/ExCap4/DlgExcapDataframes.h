// DlgExcapDataframes.h : header file
//

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CDlgExcapDataframes dialog

class CDlgExcapDataframes : public CDialog
{
// Construction
public:
	CDlgExcapDataframes(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CDlgExcapDataframes)
	enum { IDD = IDD_EXCAPDATAFRAMES };
	CComboBox	m_cbDatatype;
	BOOL	m_bUserAttachBuffer;
	long	m_nFrames;
	//}}AFX_DATA

	HDCAM			m_hdcam;
	DCAM_PIXELTYPE	m_nPixeltype;
	SIZE			m_szData;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDlgExcapDataframes)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CDlgExcapDataframes)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
