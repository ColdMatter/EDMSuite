// DlgExcapLUT.h : header file
//

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CDlgExcapLUT dialog

class CDlgExcapLUT : public CDialog
{
// Construction
public:
	CDlgExcapLUT(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CDlgExcapLUT)
	enum { IDD = IDD_EXCAPLUT };
	CEdit	m_ebInmin;
	CEdit	m_ebInmax;
	CSliderCtrl	m_sliderInmin;
	CSliderCtrl	m_sliderInmax;
	long	m_nInmin;
	long	m_nInmax;
	//}}AFX_DATA

// Common Dialog Data in ExCap
protected:
	BOOL		m_bCreateDialog;
	BOOL		m_bChangingEditbox;

// Private Dialog Data
protected:
	class luttable*	m_luttable;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDlgExcapLUT)
	public:
	virtual BOOL Create( CWnd* pParentWnd = NULL );
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation

// Common implementation of DCAM Dialog in ExCap
public:
			BOOL	toggle_visible();
			luttable*	set_luttable( luttable* lut );

protected:
			void	update_controls();

	// Generated message map functions
	//{{AFX_MSG(CDlgExcapLUT)
	virtual BOOL OnInitDialog();
	afx_msg void OnDestroy();
	virtual void OnOK();
	virtual void OnCancel();
	afx_msg void OnHScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void OnChangeExcaplutEditbox();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
