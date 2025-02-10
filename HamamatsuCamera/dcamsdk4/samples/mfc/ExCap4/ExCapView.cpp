
// ExCapView.cpp : implementation of the CExCapView class
//

#include "stdafx.h"
// SHARED_HANDLERS can be defined in an ATL project implementing preview, thumbnail
// and search filter handlers and allows sharing of document code with that project.
#ifndef SHARED_HANDLERS
#include "ExCap4.h"
#endif

#include "ExCapApp.h"
#include "ExCapDoc.h"
#include "ExCapView.h"
#include "ChildFrm.h"
#include "bitmap.h"
#include "luttable.h"
#include "image.h"
#include <algorithm>
#ifdef _DEBUG
#define new DEBUG_NEW
#endif

enum {
	IDT_EXCAPVIEW_UPDATEIMAGE = 1,
};

const int ELAPSE_TIME = 16;

// CExCapView

IMPLEMENT_DYNCREATE(CExCapView, CScrollView)

BEGIN_MESSAGE_MAP(CExCapView, CScrollView)
	ON_WM_CREATE()
	ON_WM_ERASEBKGND()
	ON_WM_TIMER()
	ON_WM_MOUSEWHEEL()
	ON_UPDATE_COMMAND_UI(ID_INDICATOR_CURRENTFRAME, OnUpdateCurrentframe)
	ON_UPDATE_COMMAND_UI(ID_INDICATOR_FRAMES, OnUpdateFrames)
	ON_UPDATE_COMMAND_UI_RANGE(ID_FRAME_HEAD, ID_FRAME_TAIL, OnUpdateFrames)
	ON_COMMAND_RANGE(ID_FRAME_HEAD,ID_FRAME_TAIL,OnCommandFrames)
END_MESSAGE_MAP()

// CExCapView construction/destruction

CExCapView::CExCapView() 
{	
	m_wndBitmap = new var_wndBitmap;
	m_luttable = NULL;	// new luttable;	

	memset( &m_frame, 0, sizeof( m_frame ) );
	m_frame.current = m_frame.total - 1;
	m_frame.last_draw = -1;
	m_oldzoom = 100;
	m_zoom = 100;	
	m_drawing = false;
	m_memDC.m_hDC = 0;
}

CExCapView::~CExCapView()
{
	delete m_wndBitmap;

	if( m_luttable != NULL )
		delete m_luttable;
	DeleteDC(m_memDC);  // kill the DC
}

void CExCapView::OnPrepareDC(CDC* pDC, CPrintInfo* pInfo)
{	
	// This must be set or CScrollView::OnPrepareDC will give an assertion
	m_nMapMode = MM_TEXT;
	CScrollView::OnPrepareDC(pDC,pInfo);

	pDC->SetMapMode(MM_TEXT);          // force map mode to MM_TEXT
	pDC->SetViewportOrg(CPoint(0, 0)); // force viewport origin to zero
}

void CExCapView::OnInitialUpdate() 
{	
	SetZoom(m_zoom);
	UpdateScroll();	
	// create a in-memory device context,	
	if(!m_memDC.CreateCompatibleDC( NULL ))	
		MessageBox(_T("Failed to Create Memory DC"), _T("Error"),MB_OK);	
	
	CScrollView::OnInitialUpdate();	
}

BOOL CExCapView::PreCreateWindow(CREATESTRUCT& cs)
{
	return CScrollView::PreCreateWindow(cs);
}

// If you choose to center the bitmap
// this should erase the areas outside.
void CExCapView::__FillOutsideRect(CDC* pDC,CBrush* pBrush)
{	
	// fill rect outside the image
	CRect rect;
	GetClientRect(rect);
	ASSERT(rect.left == 0 && rect.top == 0);

	long	imgwidth	=  m_totalDev.cx; // image width in device units
	long	imgheight	=  m_totalDev.cy; // image height in device units
	long	dstwidth	=  min(rect.Width(),static_cast<long>(m_wndBitmap->bmih.biWidth * (m_zoom *0.01 )));
	long	dstheight	=  min(rect.Height(),static_cast<long>(abs(m_wndBitmap->bmih.biHeight) * (m_zoom *0.01 )));
	long	imgleft		=  max(0,(( rect.right + rect.left - dstwidth ) >> 1) );
	long	imgtop		=  max(0,(( rect.bottom + rect.top - dstheight) >> 1) );

	CSize pos(imgleft,imgtop);
	pDC->LPtoDP((LPPOINT)&pos);
	imgleft = pos.cx;
	imgtop = pos.cy;

	rect.left = imgleft + imgwidth ;
	if (!rect.IsRectEmpty())
		pDC->FillRect(rect, pBrush);    // vertical strip along the right side  m_totalDev

	rect.left = 0;
	rect.right = imgleft + imgwidth;
	rect.top = imgtop + imgheight;
	if (!rect.IsRectEmpty())
		pDC->FillRect(rect, pBrush);    // horizontal strip along the bottom
	
	
	rect.top = 0;
	rect.bottom = imgtop;
	if (!rect.IsRectEmpty())
		pDC->FillRect(rect, pBrush);	// horizontal strip along the top

	rect.top = imgtop;
	rect.bottom = imgtop + imgheight;
	rect.right = imgleft;
	if (!rect.IsRectEmpty())
		pDC->FillRect(rect, pBrush);	// vertical strip along the left side

}

BOOL CExCapView::EraseBkgnd(CDC* pDC)
{	
    CBrush br;
    br.CreateStockObject(WHITE_BRUSH);

	if(m_drawing)
		__FillOutsideRect( pDC,&br);
	else {	// keeping it gray
		CRect rc;
		pDC->GetClipBox(&rc);
		pDC->FillRect(&rc,&br);
	}
	return TRUE;
}

// CExCapView drawing
BOOL CExCapView::OnEraseBkgnd(CDC* pDC) 
{		
	
    return EraseBkgnd(pDC);		
}
//
#if 20180418
void CExCapView::OnDraw(CDC* pDC)
{
	if( m_wndBitmap->dibits != NULL && m_drawing )
	{	
		// bitmap must be compatible with view DC

		CRect rcClient(0, 0, 0, 0);
		GetClientRect(rcClient);
		CPoint ptScroll = GetScrollPosition();
		double dZoom = m_zoom *0.01;
		

		const int cx = rcClient.right;				// view client area width
		const int cy = rcClient.bottom;				// view client area height
		const int bx = m_wndBitmap->bmih.biWidth;   // source bitmap width
		const int by = m_wndBitmap->bmih.biHeight;  // source bitmap height
		const int vx = (int)(bx * dZoom);			// virtual document width
		const int vy = (int)(by * dZoom);			// virtual document height
		const int xPos = ptScroll.x;				// horizontal scroll position
		const int yPos = ptScroll.y;				// vertical scroll position

		// source and destination cordinates and sizes
		int xSrc, ySrc, nSrcWidth, nSrcHeight, xDst, yDst, nDstWidth, nDstHeight;   

		if(vx > cx)
		{
			xSrc = (int)(xPos / dZoom);
			nSrcWidth = bx - xSrc;
			xDst = 0;
			nDstWidth = vx - xPos;
		}
		else 
		{
			xSrc = 0;
			nSrcWidth = bx;
			xDst = cx / 2 - vx / 2;
			nDstWidth = vx;
		}

		if(vy > cy)
		{
			ySrc = (int)(yPos / dZoom);
			nSrcHeight = by - ySrc;
			yDst = 0;
			nDstHeight = vy - yPos;
		}
		else 
		{
			ySrc = 0;
			nSrcHeight = by;
			yDst = cy / 2 - vy / 2;
			nDstHeight = vy;
		}
		
		BYTE*	dibits = m_wndBitmap->dibits;
		if( ySrc > 0 )
		{
			int	rowbytes = getrowbytes( m_wndBitmap->bmih );
			if( rowbytes < 0 )
			{
				m_wndBitmap->bmih.biHeight -= ySrc;
				ySrc = 0;
			}
			else
			{
				dibits += rowbytes;
				ySrc = 0;
			}
		}

		pDC->SetStretchBltMode(HALFTONE);
		StretchDIBits( pDC->GetSafeHdc()
			, xDst, yDst, nDstWidth, nDstHeight
			, xSrc, ySrc, nSrcWidth, nSrcHeight
			, dibits, (const BITMAPINFO*)&m_wndBitmap->bmih
			, DIB_RGB_COLORS, SRCCOPY );

		m_wndBitmap->bmih.biHeight = by;
	}		 
}
#else
void CExCapView::OnDraw(CDC* pDC)
{
	
	if( m_wndBitmap->dibits != NULL && m_drawing )
	{	
		// bitmap must be compatible with view DC
		HBITMAP h_Bmp = CreateDIBitmap(pDC->m_hDC,&m_wndBitmap->bmih,CBM_INIT,m_wndBitmap->dibits,(BITMAPINFO*)(&m_wndBitmap->bmih) ,DIB_RGB_COLORS);
		HBITMAP hBmpold = (HBITMAP)m_memDC.SelectObject(h_Bmp);

		BltBufferedImage(pDC,&m_memDC);
		m_memDC.SelectObject(hBmpold);
		DeleteObject(h_Bmp);
	}		 
}
#endif

void CExCapView::BltBufferedImage(CDC *viewDC, CDC* memDC)
{
	ASSERT_VALID(viewDC);

	CRect rcClient(0, 0, 0, 0);
	GetClientRect(rcClient);
	CPoint ptScroll = GetScrollPosition();
	double dZoom = m_zoom *0.01;
	

	const int cx = rcClient.right;				// view client area width
	const int cy = rcClient.bottom;				// view client area height
	const int bx = m_wndBitmap->bmih.biWidth;   // source bitmap width
	const int by = m_wndBitmap->bmih.biHeight;  // source bitmap height
	const int vx = (int)(bx * dZoom);			// virtual document width
	const int vy = (int)(by * dZoom);			// virtual document height
	const int xPos = ptScroll.x;				// horizontal scroll position
	const int yPos = ptScroll.y;				// vertical scroll position

	// source and destination cordinates and sizes
	int xSrc, ySrc, nSrcWidth, nSrcHeight, xDst, yDst, nDstWidth, nDstHeight;   

	if(vx > cx)
	{
		xSrc = (int)(xPos / dZoom);
		nSrcWidth = bx - xSrc;
		xDst = 0;
		nDstWidth = vx - xPos;
	}
	else 
	{
		xSrc = 0;
		nSrcWidth = bx;
		xDst = cx / 2 - vx / 2;
		nDstWidth = vx;
	}

	if(vy > cy)
	{
		ySrc = (int)(yPos / dZoom);
		nSrcHeight = by - ySrc;
		yDst = 0;
		nDstHeight = vy - yPos;
	}
	else 
	{
		ySrc = 0;
		nSrcHeight = by;
		yDst = cy / 2 - vy / 2;
		nDstHeight = vy;
	}
		
	viewDC->SetStretchBltMode(HALFTONE);
	viewDC->StretchBlt(xDst, yDst, nDstWidth, nDstHeight, memDC, xSrc, ySrc, nSrcWidth, nSrcHeight, SRCCOPY); 
}
// CExCapView diagnostics

#ifdef _DEBUG
void CExCapView::AssertValid() const
{
	CScrollView::AssertValid();
}

void CExCapView::Dump(CDumpContext& dc) const
{
	CScrollView::Dump(dc);
}

CExCapDoc* CExCapView::GetDocument() const // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CExCapDoc)));
	return dynamic_cast<CExCapDoc*>(m_pDocument);
}
#endif //_DEBUG

// CExCapView message handlers

int CExCapView::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CScrollView::OnCreate(lpCreateStruct) == -1)
		return -1;	
	return 0;
}

void CExCapView::OnActivateView(BOOL bActivate, CView* pActivateView, CView* pDeactiveView) 
{
	if( bActivate )
	{
		CExCapDoc*	doc = GetDocument();
		CExCapApp*	app = afxGetApp();
		app->set_active_objects( doc->get_hdcam(), doc->get_hwait(),  doc, m_luttable );
		app->update_availables();
	}
	
	CScrollView::OnActivateView(bActivate, pActivateView, pDeactiveView);
}

void CExCapView::ResetView()
{
	GetDocument()->get_bitmapinfoheader(m_wndBitmap->bmih);		
	m_wndBitmap->dibitsize = 0;		
	ResetZoom();
	SetScrollPos( SB_HORZ,0);
	SetScrollPos( SB_VERT,0);
	Invalidate();
}

void CExCapView::ResetZoom()
{	
	m_oldzoom = 100;
	m_zoom = 100;
}

BOOL CExCapView::allocbits(   )
{
	long	newrowbytes = abs( getrowbytes( m_wndBitmap->bmih ) );

	long	newdibitsize = newrowbytes * abs( m_wndBitmap->bmih.biHeight );
	if( m_wndBitmap->dibitsize != newdibitsize )
	{
		m_wndBitmap->dibitsize = newdibitsize;
		if(m_wndBitmap->dibits){				
			delete [] m_wndBitmap->dibits;
			m_wndBitmap->dibits = NULL;
		}

		m_wndBitmap->dibits = new BYTE[newdibitsize];					
	}

	if( m_wndBitmap->dibits == NULL )
	{
		MessageBox(_T("Failed to Allocate image buffer"), _T("Error"),MB_OK);
		return FALSE;
	}

	return TRUE;
}

// CExCapView draw bitmap
void CExCapView::update_bitmap()
{	
	CExCapDoc* pDoc = GetDocument();
	m_drawing = false;
	
	if(m_memDC.m_hDC == NULL)
		return;
	if( pDoc && allocbits() )
	{			
		const BYTE*	lut = ( m_luttable == NULL ? NULL : m_luttable->gettable() );
		m_drawing = pDoc->copy_dibits( m_wndBitmap->dibits, m_wndBitmap->bmih, m_frame.current, 0, 0, m_wndBitmap->bmiColors, lut );					
	}	

	if(m_drawing )
	{
		m_frame.last_draw = m_frame.current;
		Invalidate();
	}	
}


void CExCapView::update_luttable()
{
	CExCapDoc* pDoc = GetDocument();
	image*	pImage = pDoc->get_image();

	luttable*	new_luttable = NULL;
	if( pImage != NULL )
	{
		long	bitperchannel = pImage->pixelperchannel();

		ASSERT( 8 <= bitperchannel && bitperchannel <= 16 );
		new_luttable = new luttable( bitperchannel );
	}

	if( m_luttable != NULL )
		delete m_luttable;

	m_luttable = new_luttable;

	{
		CExCapApp*	app = afxGetApp();

		HDCAM		hdcam;
		HDCAMWAIT   hwait;
		CExCapDoc*	doc;
		luttable*	lut;

		app->get_active_objects( hdcam, hwait, doc, lut );
		if( doc == GetDocument() && hdcam == doc->get_hdcam() )
			app->set_active_objects( hdcam, hwait, doc, m_luttable );
	}
}

void CExCapView::OnUpdate(CView* pSender, LPARAM lHint, CObject* pHint) 
{
	CRect rc;
	GetClientRect(&rc);
	switch( lHint )
	{
	case CExCapDoc::image_updated:
		memset( &m_frame, 0, sizeof( m_frame ) );
		m_frame.last_draw = -1;
		{
			CExCapDoc*	doc = GetDocument();
			if( doc != NULL )
			{
				doc->get_bitmapinfoheader(m_wndBitmap->bmih);
				image* img = doc->get_image();
				m_frame.total = img->numberof_frames();
				if(m_frame.total)
					SetTimer( IDT_EXCAPVIEW_UPDATEIMAGE, ELAPSE_TIME, NULL );
			}
		}		
		SetZoom( 100);
		update_luttable();
		break;

	case CExCapDoc::stop_capture:
		memset( &m_frame, 0, sizeof( m_frame ) );
		{
			CExCapDoc*	doc = GetDocument();
			if( doc != NULL )
				m_frame.total = doc->numberof_capturedframes();
			m_frame.current = m_frame.total - 1;
			m_frame.last_draw = -1;

			if( m_frame.total == 0
			 && m_wndBitmap->dibits != NULL )
			{
				KillTimer(IDT_EXCAPVIEW_UPDATEIMAGE);
				//erase image
				memset( m_wndBitmap->dibits, 0x80, m_wndBitmap->dibitsize );	// gray		
				m_drawing = false;
				Invalidate();
				UpdateWindow();				
				delete [] m_wndBitmap->dibits;
				m_wndBitmap->dibits = NULL;				
			}
		}		
		break;

	case CExCapDoc::start_capture:
		{
			memset( &m_frame, 0, sizeof( m_frame ) );
			m_frame.current = m_frame.total - 1;
			m_frame.last_draw = -1;

			ResetView();			
			update_luttable();				
			
			CExCapDoc*	doc = GetDocument();			
			
			double	dBinning = 1;
			DCAMERR err = dcamprop_getvalue( doc->get_hdcam(), DCAM_IDPROP_BINNING, &dBinning );
			VERIFY( !failed(err) );

			int32 nBin = (int32)dBinning;
			long	hbin, vbin;
			hbin = vbin = nBin;
			if( nBin > 100 )
			{
				hbin = nBin / 100;
				vbin = nBin % 100;
			}

			dBinning = 1;
			err = dcamprop_getvalue( doc->get_hdcam(), DCAM_IDPROP_BINNING_HORZ, &dBinning );
			if( !failed(err) )
				hbin = (long)dBinning;

			dBinning = 1;
			err = dcamprop_getvalue( doc->get_hdcam(), DCAM_IDPROP_BINNING_VERT, &dBinning );
			if( !failed(err) )
				vbin = (long)dBinning;

			m_wndBitmap->m_hBinning = hbin;
			m_wndBitmap->m_vBinning = vbin;

			double cx = 1;
			err = dcamprop_getvalue( doc->get_hdcam(), DCAM_IDPROP_IMAGE_WIDTH, &cx);
			VERIFY( !failed(err) );
			double cy = 1;
			err = dcamprop_getvalue( doc->get_hdcam(), DCAM_IDPROP_IMAGE_HEIGHT, &cy);
			VERIFY( !failed(err) );

			long width = static_cast<long>(cx);
			long height = static_cast<long>(cy);
			
			double zx = min((rc.Width()/cx), (rc.Height()/cy));
			m_oldzoom = zx*100;
			SetZoom(m_oldzoom );	
				
#ifdef _EXCAP_SUPPORTS_VIEWS_
			if( doc->is_show_allview() )
			{
				double f = 1;
				err = dcamprop_getvalue( doc->get_hdcam(), DCAM_IDPROP_NUMBEROF_VIEW, &f );
				if( !failed(err) && f > 1 )
					height *= static_cast<int32>(f);
			}
#endif // _EXCAP_SUPPORTS_VIEWS_ !
			SetTimer( IDT_EXCAPVIEW_UPDATEIMAGE, ELAPSE_TIME, NULL );
		}
		break;

	default:
		CScrollView::OnUpdate( pSender, lHint, pHint );
	}
	
}

void CExCapView::OnTimer(UINT_PTR nIDEvent) 
{
	CExCapDoc*	doc = GetDocument();

	switch( nIDEvent )
	{
	case IDT_EXCAPVIEW_UPDATEIMAGE:
		if( (m_frame.total > 0 && m_frame.last_draw != m_frame.current )
			|| (doc->is_bitmap_updated() || m_luttable->is_updated() ))
		{
			update_bitmap();
		}
		break;
	}

	CScrollView::OnTimer(nIDEvent);
}

BOOL CExCapView::OnScrollBy(CSize sizeScroll, BOOL bDoScroll) 
{
	int xOrig, x;
	int yOrig, y;

	// don't scroll if there is no valid scroll range (ie. no scroll bar)
	CScrollBar* pBar;
	DWORD dwStyle = GetStyle();
	pBar = GetScrollBarCtrl(SB_VERT);
	if ((pBar != NULL && !pBar->IsWindowEnabled()) ||
		(pBar == NULL && !(dwStyle & WS_VSCROLL)))
	{
		// vertical scroll bar not enabled
		sizeScroll.cy = 0;
	}
	pBar = GetScrollBarCtrl(SB_HORZ);
	if ((pBar != NULL && !pBar->IsWindowEnabled()) ||
		(pBar == NULL && !(dwStyle & WS_HSCROLL)))
	{
		// horizontal scroll bar not enabled
		sizeScroll.cx = 0;
	}

	// adjust current x position
	xOrig = x = GetScrollPos(SB_HORZ);
	int xMax = GetScrollLimit(SB_HORZ);
	x += sizeScroll.cx;
	if (x < 0)
		x = 0;
	else if (x > xMax)
		x = xMax;

	// adjust current y position
	yOrig = y = GetScrollPos(SB_VERT);
	int yMax = GetScrollLimit(SB_VERT);
	y += sizeScroll.cy;
	if (y < 0)
		y = 0;
	else if (y > yMax)
		y = yMax;

	// did anything change?
	if (x == xOrig && y == yOrig)
		return FALSE;

	if (bDoScroll)
	{
		// do scroll and update scroll positions		
      Invalidate(FALSE); // eliminate flickering while scrolling
		if (x != xOrig)
			SetScrollPos(SB_HORZ, x);
		if (y != yOrig)
			SetScrollPos(SB_VERT, y);
	}
	return TRUE;
}

void CExCapView::OnUpdateCurrentframe(CCmdUI* pCmdUI) 
{
	BOOL	bEnable	= FALSE;

	if( m_frame.total > 0 )
	{
		CString	str;
		str.Format( _T("Current Frame: %d"),m_frame.current );
		pCmdUI->SetText( str );

		bEnable	= TRUE;
	}

	pCmdUI->Enable( bEnable );
}

void CExCapView::OnUpdateFrames(CCmdUI* pCmdUI) 
{
	BOOL	bEnable	= FALSE;

	if( pCmdUI->m_nID == ID_INDICATOR_FRAMES )
	{
		if( m_frame.total > 0 )
		{
			CString	str;
			str.Format( _T("Total Frames:  %d"), m_frame.total );
			pCmdUI->SetText( str );

			bEnable	= TRUE;
		}
		
	}
	else
	{
		switch( pCmdUI->m_nID )
		{
		case ID_FRAME_HEAD:
		case ID_FRAME_PREV:
			if( m_frame.total > 0 && m_frame.current > 0 )
				bEnable = TRUE;
			break;

		case ID_FRAME_TAIL:
		case ID_FRAME_NEXT:
			if( m_frame.total > 0 && m_frame.current < m_frame.total-1 )
				bEnable = TRUE;
			break;

		default:
			ASSERT( 0 );
		}
	}

	pCmdUI->Enable( bEnable );
}

void CExCapView::OnCommandFrames( UINT id ) 
{
	ASSERT( m_frame.total > 0 );

	long	iFrame = m_frame.current;
	switch( id )
	{
	case ID_FRAME_HEAD:	iFrame = 0;	break;
	case ID_FRAME_PREV:	iFrame--;	break;
	case ID_FRAME_TAIL:	iFrame = m_frame.total-1;	break;
	case ID_FRAME_NEXT:	iFrame++;	break;
	}

	m_frame.current = iFrame;
}

BOOL CExCapView::OnMouseWheel(UINT fFlags, short zDelta, CPoint point)
{
	int zoom = zDelta /24;	
	m_oldzoom = m_zoom;
	double fZoomH = m_oldzoom;

	fZoomH += zoom;	

	if( fZoomH >500 )
		fZoomH =500;	

	if( fZoomH < 25 )
		fZoomH = 25;	
	
	SetZoom( fZoomH );
	
	return CScrollView::OnMouseWheel(fFlags, zDelta, point);
}

void CExCapView::SetZoom(double nZH) 
{	
	m_zoom = nZH;
	UpdateStatusBar();
	UpdateScroll();	
	Invalidate();
	UpdateWindow();// paint immediately
}

void CExCapView::UpdateScroll()
{
	long	dstwidth = static_cast<long>(m_wndBitmap->bmih.biWidth * (m_zoom *0.01 ));
	long	dstheight= static_cast<long>(abs(m_wndBitmap->bmih.biHeight) * (m_zoom *0.01 ));
	CSize sizeTotal(max(0,dstwidth ), max(0,dstheight)); 	
	SetScrollSizes(MM_TEXT,sizeTotal);
}

void CExCapView::UpdateStatusBar()
{
	CChildFrame* frame = reinterpret_cast<CChildFrame*>(GetParentFrame());
	if(frame)	
		frame->UpdateZoomStatus(m_zoom);	
	
}