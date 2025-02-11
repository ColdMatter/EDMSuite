// DlgDcamAbout.h : header file
//

#include "afxcmn.h"
#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CDlgDcamAbout dialog

class CDlgDcamAbout : public CDialog
{
// Construction
public:
	CDlgDcamAbout( HDCAM hdcam, CWnd* pParent = NULL);
	CDlgDcamAbout( long index, CWnd* pParent = NULL);

// Dialog Data
	//{{AFX_DATA(CDlgDcamAbout)
	enum { IDD = IDD_DLGDCAMABOUT };
	CListCtrl m_lvStrings;
	//}}AFX_DATA

	CString	m_strStrings;
	BOOL	m_bCopy;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDlgDcamAbout)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

// Generated message map functions
protected:
	//{{AFX_MSG(CDlgDcamAbout)
	virtual BOOL OnInitDialog();
	afx_msg void OnCopy();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
