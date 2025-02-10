
// ExCapView.h : interface of the CExCapView class
//

#pragma once

struct var_wndBitmap
{
	BITMAPINFOHEADER	bmih;
    RGBQUAD             bmiColors[256];
	BYTE*	dibits;		
	long	dibitsize;
	double	m_hBinning;
	double	m_vBinning;

public:
	~var_wndBitmap()
	{
		//dbits is allocated as array
		if( dibits != NULL )
			delete [] dibits;
	}
	var_wndBitmap()
	{
		memset( &bmih, 0, sizeof( bmih ) );
		bmih.biSize = sizeof( bmih );
		memset( &bmiColors, 0, sizeof( bmiColors ) );
		dibits = NULL;
		dibitsize = 0;
		m_hBinning = m_vBinning = 1;
	}

	var_wndBitmap(const var_wndBitmap &rhsVar)
	{
		memset( &bmih, 0, sizeof( BITMAPINFOHEADER ) );
		bmih.biSize = sizeof( BITMAPINFOHEADER );
		memcpy(&bmih, &rhsVar.bmih,  sizeof( BITMAPINFOHEADER ) );

		memset( &bmiColors, 0, sizeof( RGBQUAD ) );
		memcpy(&bmiColors, &rhsVar.bmiColors,  sizeof( RGBQUAD ) );

		//dibits = rhsVar.dibits;		
		dibitsize = rhsVar.dibitsize;

		dibits = new BYTE[dibitsize];
		memcpy(dibits,rhsVar.dibits,dibitsize);

		m_hBinning = rhsVar.m_hBinning;
		m_vBinning = rhsVar.m_vBinning;
	}

	void GetBmpInfoHeader(BITMAPINFOHEADER &bmpifo)
	{
		memset( &bmpifo, 0, sizeof( BITMAPINFOHEADER ) );
		bmpifo.biSize = sizeof( BITMAPINFOHEADER );
		memcpy(&bmpifo, &bmih, sizeof( BITMAPINFOHEADER ) );
	}

	void GetBmpInfo(BITMAPINFO &bmpifo)
	{			
		bmpifo.bmiHeader.biSize = sizeof( BITMAPINFOHEADER );
		memcpy(&bmpifo.bmiHeader, &bmih, sizeof( BITMAPINFOHEADER ) );			
	}
};


class CExCapView : public CScrollView
{

public:
	CExCapDoc* GetDocument() const;	

// Overrides
	virtual ~CExCapView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
	virtual void OnInitialUpdate();
	virtual void OnPrepareDC(CDC* pDC, CPrintInfo* pInfo = 0);
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);

protected:	
	var_wndBitmap*	m_wndBitmap;
	class luttable*	m_luttable;
	
	struct {
		long	total;
		long	current;
		long	last_draw;
	} m_frame;

	CDC m_memDC ;

	CExCapView();
	DECLARE_DYNCREATE(CExCapView)
	virtual void OnActivateView(BOOL bActivate, CView* pActivateView, CView* pDeactiveView);
	virtual void OnUpdate(CView* pSender, LPARAM lHint, CObject* pHint);
	virtual void BltBufferedImage(CDC *memDC, CDC* viewDC);
	virtual void OnDraw(CDC* pDC) ;  // overridden to draw this view
	virtual BOOL OnScrollBy(CSize sizeScroll, BOOL bDoScroll = TRUE);
	
	virtual void __FillOutsideRect(CDC* pDC,CBrush* pBrush) ;

	BOOL	allocbits( );
	void	update_bitmap();
	void	update_luttable();	
	void	SetZoom(double nZoom) ;
	void	ResetZoom();
	
// Generated message map functions

	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnUpdateCurrentframe(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFrames(CCmdUI* pCmdUI);
	afx_msg void OnCommandFrames( UINT id );
	afx_msg BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	DECLARE_MESSAGE_MAP()

// Attributes
private: 
	double m_oldzoom;
	double m_zoom;
	BOOL	m_drawing;
	
	void	ResetView();
	void	UpdateScroll();
	void	UpdateStatusBar();
	BOOL	EraseBkgnd(CDC* pDC);

};

#ifndef _DEBUG  // debug version in ExCapView.cpp
inline CExCapDoc* CExCapView::GetDocument() const
   { return reinterpret_cast<CExCapDoc*>(m_pDocument); }
#endif

