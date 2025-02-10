
// ExCapDoc.h : interface of the CExCapDoc class
//


#pragma once


class CExCapDoc : public CDocument
{
protected: // create from serialization only
	CExCapDoc();
	DECLARE_DYNCREATE(CExCapDoc)

// Attributes
public:

protected:
	class image*	m_image;

	// related to DCAM
			int32			m_supportevents;
			HDCAM			m_hdcam;
			HDCAMREC		m_hrec;
			HDCAMWAIT		m_hwait;
			DCAM_PIXELTYPE	m_nPixeltype;
			long			m_nFramecount;
			long			m_nFramecountDcimg;
			CString			m_strFolderDcimg;
			CString			m_strFilenameDcimg;
			
			UINT	m_idCapturingSequence;
			BOOL	m_bBufferReady;
			long	m_nCapturedFramecount;
			long	m_nCapturedOffset;
			BOOL	m_bUseAttachBuffer;

#ifdef _EXCAP_SUPPORTS_VIEWS_
			UINT	m_idShowMultiView;
#endif // _EXCAP_SUPPORTS_VIEWS_ !
	BOOL	m_disable_setpathname;

// Operations
public:
	// for DCAM
	HDCAM		get_hdcam() const			{ return m_hdcam; }
	HDCAMWAIT	get_hwait() const			{ return m_hwait; }
	int32		get_suportevents() const	{ return m_supportevents; }
	BOOL	start_capturing( long param );
	long	suspend_capturing();
	void	resume_capturing( long param );

	// for image
	image*	get_image() const		{ return m_image; }
#ifdef _EXCAP_SUPPORTS_VIEWS_
	BOOL	is_show_allview();
#endif // _EXCAP_SUPPORTS_VIEWS_ !
	BOOL	is_bitmap_updated();
	BOOL	get_bitmapinfoheader( BITMAPINFOHEADER& bmih );
	long	numberof_capturedframes() const;
	BOOL	copy_dibits( BYTE* bottomleft, const BITMAPINFOHEADER& bmih, long iFrame, long hOffset, long vOffset, RGBQUAD* rgb = NULL, const BYTE* lut = NULL );

	// for update
	void	update_pixeltype();

	// for notification
	enum {
		image_updated = 1,
		start_capture,
		stop_capture,
		zoom_factorchange,
	};

// Overrides
public:
	virtual BOOL OnNewDocument();
	virtual void Serialize(CArchive& ar);
#ifdef SHARED_HANDLERS
	virtual void InitializeSearchContent();
	virtual void OnDrawThumbnail(CDC& dc, LPRECT lprcBounds);
#endif // SHARED_HANDLERS
	virtual void OnCloseDocument();
	virtual BOOL OnOpenDocument(LPCTSTR lpszPathName);
	virtual BOOL OnSaveDocument(LPCTSTR lpszPathName);
	virtual void SetPathName(LPCTSTR lpszPathName, BOOL bAddToMRU = TRUE);

// Implementation
public:
	virtual ~CExCapDoc();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	afx_msg void OnCaptureSequence();
	afx_msg void OnCaptureSnap();
	afx_msg void OnCaptureRecord();
	afx_msg void OnCaptureIdle();
	afx_msg void OnUpdateCapture(CCmdUI* pCmdUI);
	afx_msg void OnUpdateCaptureIdle(CCmdUI* pCmdUI);
	afx_msg void OnCaptureFiretrigger();
	afx_msg void OnUpdateCaptureFiretrigger(CCmdUI* pCmdUI);
	afx_msg void OnCaptureDataframes();
	afx_msg void OnUpdateCaptureDataframes(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFileSaveAs(CCmdUI* pCmdUI);
	afx_msg void OnFileSaveAs();
#ifdef _EXCAP_SUPPORTS_VIEWS_
	afx_msg void OnMultiViewShowImage(UINT nID);
	afx_msg void OnUpdateMultiViewShowImage(CCmdUI* pCmdUI);
#endif // _EXCAP_SUPPORTS_VIEWS_ !
	DECLARE_MESSAGE_MAP()

#ifdef SHARED_HANDLERS
	// Helper function that sets search content for a Search Handler
	void SetSearchContent(const CString& value);
#endif // SHARED_HANDLERS
};
