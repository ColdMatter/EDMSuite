
// ExCapApp.h : interface of the CExCapApp class
//
#pragma once

// CExCapApp:
// See ExCapApp.cpp for the implementation of this class
//

class CExCapDoc;
class luttable;

class CExCapApp : public CWinApp
{
	DECLARE_DYNAMIC(CExCapApp)
public:
	~CExCapApp();
	CExCapApp();

// Overrides
	// ClassWizard generated virtual function overrides
public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();

protected:
	void OnUpdateSetup(CCmdUI* pCmdUI, CDialog* dlg, BOOL bAvailable = TRUE );

// Implementation
	afx_msg void OnFileOpen();
	afx_msg void OnAppAbout();
	afx_msg void OnUpdateSetupProperties(CCmdUI* pCmdUI);
	afx_msg void OnSetupProperties();
	afx_msg void OnViewLut();
	afx_msg void OnUpdateViewLut(CCmdUI* pCmdUI);
	afx_msg void OnViewFramerate();
	afx_msg void OnUpdateViewFramerate(CCmdUI* pCmdUI);
	DECLARE_MESSAGE_MAP()

public:
	void	get_active_objects( HDCAM& hdcam, HDCAMWAIT& hwait, CExCapDoc*& doc, luttable*& lut ) const;
	void	set_active_objects( HDCAM hdcam,  HDCAMWAIT hwait, CExCapDoc* doc, luttable* lut );
	void	update_availables();
	void	on_close_document( CExCapDoc* doc );

	long	suspend_capturing();
	void	resume_capturing( long param );

	long	number_of_visible_controldialogs();

	long*	get_dcaminit_option();

protected:
	struct {
	HDCAM		hdcam;
	HDCAMWAIT   hwait;
	CExCapDoc*	docForHDCAM;
	luttable*	lut;
	char		cameraname[256];
	char		dcamapiver[64];
	} m_active;

	struct {
		long	dcaminit[2];
	} m_option;

	struct {
	class CDlgDcamProperty*		property;
	class CDlgExcapLUT*			lut;
	class CDlgExcapFramerate*	framerate;

	} m_dlg;

	struct {
	BOOL	property;
	BOOL	framerate;
	BOOL	status;

	BOOL	general;
	BOOL	subarray;
//	BOOL	features;
	} m_available;
#ifdef _EXCAP_SUPPORTS_VIEWS_
	CMenu	MenuMultiview;
#endif //_EXCAP_SUPPORTS_VIEWS_ !
};

inline CExCapApp* afxGetApp()
{
	return DYNAMIC_DOWNCAST( CExCapApp, AfxGetApp() );
}

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
