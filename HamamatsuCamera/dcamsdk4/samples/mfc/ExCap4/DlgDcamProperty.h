// DlgDcamProperty.h : header file
//

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CDlgDcamProperty dialog

class CDlgDcamProperty : public CDialog
{
// Construction
public:
	CDlgDcamProperty( CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CDlgDcamProperty)
	enum { IDD = IDD_DLGDCAMPROPERTY };
	CListCtrl	m_listview;
	CComboBox	m_cbChannel;
	CListBox	m_lbValues;
	CListBox	m_lbAttr;
	CStatic	m_txtMax;
	CStatic	m_txtMin;
	CStatic	m_wndMin;
	CStatic	m_wndMax;
	CEdit	m_ebValue;
	CSliderCtrl	m_sliderValue;
	CSpinButtonCtrl	m_spinValue;
	CButton	m_btnShowAllProperties;
	BOOL	m_bShowAllProperties;
	BOOL	m_bUseListboxAlways;
	BOOL	m_bUpdatePeriodically;
	//}}AFX_DATA
	CButton	m_btnChannel[ 4 ];

// Common DCAM Dialog Data in ExCap
protected:
	BOOL	m_bCreateDialog;
	HDCAM	m_hdcam;

// Private Dialog Data
protected:
	BOOL	m_bChangingEditbox;

	BOOL	m_bAutomaticUpdatePropertyValues;

	CRect	m_rcClient;
	CSize	m_szSpaceListview;			// space for m_listview control in this dialog layout.

	// information for current editing property.
	struct {
		long	indexOnListview;		// index for m_listview Control
		long	idprop;					// DCAMIDPROP
		long	attribute;				// DCAMPROPATTRIBUTE
		long	attribute2;				// DCAMPROPATTRIBUTE2
		long	nMaxChannel;			// count of CHANNEL 
		long	nMaxView;				// count of VIEW
	} m_editprop;

	double	m_fRatioSlider;				// for slider range
	double	m_fStepSlider;

	DWORD	m_dcamstatus;

	long	m_channelmode;			// 0: no channel
									// 1: over than 3 channels. Channel is selected by combobox.
									// 2, 3: channels are selected by push button style radio button.
	long	m_iChannel;				// offset for radio button from IDC_DLGDCAMPROPERTY_BTNALL or cursor index of combobox.
	long	m_idpropoffset;
	long	m_idproparraybase;

	CDWordArray	m_arrayIDPROP;	// all property IDs of current HDCAM
	CDWordArray	m_arrayAttr;	// each attribute of m_arrayIDPROP

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDlgDcamProperty)
	public:
	virtual BOOL Create( CWnd* pParentWnd = NULL, CCreateContext* pContext = NULL);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
// Common implementation in ExCap
public:
			HDCAM	set_hdcam( HDCAM hdcam );
			BOOL	toggle_visible();

// Private implementation
protected:
			void	update_viewchannel_control();
			void	update_listview_title( long iPropArrayBase, BOOL bInit=FALSE );
			void	update_listview_value();
			void	update_listview_updated_value();
			void	reset_listview_updated_value();
protected:
			void	edit_property_of( long index );
			void	update_controls();
			void	recalc_layout();

			void	update_all();

	// Generated message map functions
	//{{AFX_MSG(CDlgDcamProperty)
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	virtual void OnOK();
	virtual void OnCancel();
	afx_msg void OnDestroy();
	afx_msg void OnItemchangedDlgdcampropertyListview(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnClickDlgdcampropertyListview(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnDblclkDlgdcampropertyListview(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnCustomdrawDlgdcampropertyListview(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg LRESULT OnNcHitTest(CPoint point);
	afx_msg void OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void OnSelchangeDlgdcampropertyCbselectchl();
	afx_msg void OnSelchangeDlgdcampropertyLbvalues();
	afx_msg void OnChangeDlgdcampropertyEbvalue();
	afx_msg void OnDeltaposDlgdcampropertySpin(NMHDR* pNMHDR, LRESULT* pResult); 
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnDlgdcampropertyBtn( UINT nID );
	afx_msg void OnDlgdcampropertyBtnuselistbox();
	afx_msg void OnDlgdcampropertyBtnupdateperiodically();
	afx_msg void OnDlgdcampropertyBtnupdatevalues();
	afx_msg void OnDlgdcampropertyBtnwholeidprop();
	afx_msg void OnDlgdcampropertyBtnarrayelement();
	//}}AFX_MSG
#ifdef HPKINTERNALUSE //{{HPKINTERNALUSE
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
#endif //}}HPKINTERNALUSE	DECLARE_MESSAGE_MAP()
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
