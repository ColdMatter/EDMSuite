#pragma once


// CDlgExcapRecord dialog

class CDlgExcapRecord : public CDialog
{
	DECLARE_DYNAMIC(CDlgExcapRecord)

public:
	CDlgExcapRecord(CWnd* pParent = NULL);   // standard constructor
	virtual ~CDlgExcapRecord();

// Dialog Data
	enum { IDD = IDD_EXCAPRECORD };
	long	m_nMaxFrames;
	CString m_strFolder;
	CString m_strName;

// Overrides
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	
// Generated message map functions
protected:	
	afx_msg void OnExcaprecordBtnbrowse();
	DECLARE_MESSAGE_MAP()
	virtual void OnOK();
};
