
// ExCapDoc.cpp : implementation of the CExCapDoc class
//

#include "stdafx.h"
// SHARED_HANDLERS can be defined in an ATL project implementing preview, thumbnail
// and search filter handlers and allows sharing of document code with that project.
#ifndef SHARED_HANDLERS
#include "ExCap4.h"
#endif

#include "ExCapApp.h"
#include "ExCapDoc.h"
#include "image.h"
#include "imagedcam.h"
#include "dcamimg.h"

#include "bitmap.h"
#include "showdcamerr.h"

#include "DlgDcamOpen.h"
#include "DlgExcapDataframes.h"
#include "DlgExcapRecord.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CExCapDoc

IMPLEMENT_DYNCREATE(CExCapDoc, CDocument)

BEGIN_MESSAGE_MAP(CExCapDoc, CDocument)
	ON_COMMAND(ID_CAPTURE_SEQUENCE, OnCaptureSequence)
	ON_COMMAND(ID_CAPTURE_SNAP, OnCaptureSnap)
	ON_COMMAND(ID_CAPTURE_RECORD, OnCaptureRecord)
	ON_COMMAND(ID_CAPTURE_IDLE, OnCaptureIdle)
	ON_UPDATE_COMMAND_UI(ID_CAPTURE_SEQUENCE, OnUpdateCapture)
	ON_UPDATE_COMMAND_UI(ID_CAPTURE_RECORD, OnUpdateCapture)
	ON_UPDATE_COMMAND_UI(ID_CAPTURE_IDLE, OnUpdateCaptureIdle)
	ON_COMMAND(ID_CAPTURE_FIRETRIGGER, OnCaptureFiretrigger)
	ON_UPDATE_COMMAND_UI(ID_CAPTURE_FIRETRIGGER, OnUpdateCaptureFiretrigger)
	ON_COMMAND(ID_CAPTURE_DATAFRAMES, OnCaptureDataframes)
	ON_UPDATE_COMMAND_UI(ID_CAPTURE_DATAFRAMES, OnUpdateCaptureDataframes)
	ON_COMMAND(ID_FILE_SAVE_AS, OnFileSaveAs)
	ON_UPDATE_COMMAND_UI(ID_FILE_SAVE_AS, OnUpdateFileSaveAs)
	ON_UPDATE_COMMAND_UI(ID_CAPTURE_SNAP, OnUpdateCapture)
#ifdef _EXCAP_SUPPORTS_VIEWS_
	ON_COMMAND_RANGE(ID_MULTIVIEWSHOWIMAGE_VIEW1,ID_MULTIVIEWSHOWIMAGE_ALLVIEW, OnMultiViewShowImage)
	ON_UPDATE_COMMAND_UI_RANGE(ID_MULTIVIEWSHOWIMAGE_VIEW1,ID_MULTIVIEWSHOWIMAGE_ALLVIEW, OnUpdateMultiViewShowImage)
#endif // _EXCAP_SUPPORTS_VIEWS_ !
END_MESSAGE_MAP()


// CExCapDoc construction/destruction

CExCapDoc::CExCapDoc()
{
	// TODO: add one-time construction code here

	m_image = NULL;

	m_hdcam				= NULL;
	m_hrec				= NULL;
	m_hwait				= NULL;
	m_supportevents		= 0;
	m_nPixeltype		= DCAM_PIXELTYPE_NONE;
	m_nFramecount		= 3;
	m_nFramecountDcimg  = 100;

	TCHAR temp[2048];
	if (SUCCEEDED(SHGetFolderPath(NULL, CSIDL_MYDOCUMENTS, NULL, 0, temp)))
		m_strFolderDcimg = temp;

	if (m_strFolderDcimg.Right(1) != "\\")
		m_strFolderDcimg += "\\";

	m_strFilenameDcimg = _T("recData");
	
	m_bUseAttachBuffer	= FALSE;
#ifdef _EXCAP_SUPPORTS_VIEWS_
	m_idShowMultiView	= ID_MULTIVIEWSHOWIMAGE_VIEW1;
#endif // _EXCAP_SUPPORTS_VIEWS_ !
	m_bBufferReady		= FALSE;

	m_idCapturingSequence = ID_CAPTURE_IDLE;
	m_nCapturedFramecount = 0;
	m_nCapturedOffset	= 0;

	m_disable_setpathname = FALSE;
}

CExCapDoc::~CExCapDoc()
{
	ASSERT( m_hdcam == NULL );
	ASSERT( m_image == NULL );
}

BOOL CExCapDoc::OnNewDocument()
{
	if (!CDocument::OnNewDocument())
		return FALSE;

	// TODO: add reinitialization code here
	// (SDI documents will reuse this document)

	ASSERT( m_hdcam == NULL );
	CExCapApp*	app = afxGetApp();
	
	// initialize DCAM-API and open camera
	if( CDlgDcamOpen::dcam_init_and_open( m_hdcam, app->get_dcaminit_option() ) != IDOK )
		return FALSE;

	ASSERT( m_hdcam != NULL );

	ASSERT( m_hwait == NULL );

	DCAMERR err;

	DCAMWAIT_OPEN	waitopen;
	memset( &waitopen, 0, sizeof(waitopen) );
	waitopen.size = sizeof(waitopen);
	waitopen.hdcam	= m_hdcam;

	err = dcamwait_open( &waitopen );
	if( !failed(err) )
	{
		m_hwait = waitopen.hwait;
		m_supportevents = waitopen.supportevent;
	}

	ASSERT( m_hwait != NULL );
	{

		CString	strModel;
		CString	strCameraId;
		CString	strBus;

		{
			char cbModel[ 128 ]={ '\0' };
			char cbCameraId[ 64 ]={ '\0' };
			char cbBus[ 64 ]={ '\0' };
			DCAMDEV_STRING param;
			param.size = sizeof(param);
			param.text = cbModel;
			param.textbytes = 128;
			param.iString = DCAM_IDSTR_MODEL;
			err = dcamdev_getstring( m_hdcam, &param);
			VERIFY( !failed(err) );

			param.text = cbCameraId;
			param.textbytes = 64;
			param.iString = DCAM_IDSTR_CAMERAID;
			err = dcamdev_getstring( m_hdcam, &param);
			VERIFY( !failed(err) );

			param.text = cbBus;
			param.textbytes = 64;
			param.iString = DCAM_IDSTR_BUS;
			err = dcamdev_getstring( m_hdcam, &param);
			VERIFY( !failed(err) );

			strModel	= cbModel;
			strCameraId	= cbCameraId;
			strBus		= cbBus;
		}

		CString	str;
		str = strModel + _T( " (" ) + strCameraId + _T( ") on " ) + strBus;

		SetTitle( str );

		double type;
		err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, &type );
		if ( !failed(err) )
			m_nPixeltype = static_cast<DCAM_PIXELTYPE>((long)type);

	}

	{
		image*	p = new_imagedcam( m_hdcam );
		ASSERT( p != NULL );

		if( m_image != NULL )
			m_image->release();

		m_image = p;
		UpdateAllViews( NULL, image_updated );
	}

	return TRUE;
}

void CExCapDoc::OnCloseDocument() 
{
	CExCapApp*	app = afxGetApp();

	DCAMERR err = DCAMERR_SUCCESS;

	if( m_hwait != NULL )
	{
		err = dcamwait_close( m_hwait );
		VERIFY( !failed(err) );
		m_hwait = NULL;

	}
	// Close Recording handle
	if( m_hrec != NULL )
	{
		err = dcamrec_close( m_hrec );
		VERIFY( !failed(err) );
		m_hrec = NULL;
	}

	// Close DCAM handle
	if( m_hdcam != NULL )
	{
		err = dcamdev_close( m_hdcam );
		VERIFY( !failed(err) );
		m_hdcam = NULL;
		m_nPixeltype = DCAM_PIXELTYPE_NONE;
	}


	if( m_image != NULL )
	{
		m_image->release();
		m_image = NULL;
	}

	app->on_close_document( this );

	CDocument::OnCloseDocument();
}

//-----------

BOOL CExCapDoc::start_capturing( long param )
{
	long	status;
	DCAMERR err = DCAMERR_NONE;
	// check status and stop capturing if capturing is already started.
	err = dcamcap_status( m_hdcam, &status );
	if( failed(err) )
	{
		show_dcamerrorbox( m_hdcam, err, "dcamcap_status()" );
		return FALSE;
	}

	{
		switch( status )
		{
		case DCAMCAP_STATUS_BUSY:
			err = dcamcap_stop( m_hdcam );
			if( failed(err) )
			{
				show_dcamerrorbox( m_hdcam, err, "dcamcap_stop()" );
				return FALSE;
			}
		case DCAMCAP_STATUS_READY:
			ASSERT( m_bBufferReady );
			err = dcambuf_release( m_hdcam );
			if( failed(err) )
			{
				show_dcamerrorbox( m_hdcam, err, "dcambuf_release()" );
				return FALSE;
			}
			if( m_bUseAttachBuffer )
			{
				m_image->freeframes();
			}
			

			m_bBufferReady = FALSE;

		case DCAMCAP_STATUS_STABLE:
		case DCAMCAP_STATUS_UNSTABLE:
			break;
		}
	}

	// status must be STABLE
	// emd - Not sure this is true....
	// err = dcamcap_status( m_hdcam, &status );
	//ASSERT( !failed(err)  && status == DCAMCAP_STATUS_STABLE );

#ifdef _EXCAP_SUPPORTS_VIEWS_
	long id = (m_idShowMultiView == ID_MULTIVIEWSHOWIMAGE_ALLVIEW ? 0 : m_idShowMultiView - ID_MULTIVIEWSHOWIMAGE_VIEW1 + 1);
	m_image->set_shown_multiview( id );
#endif // _EXCAP_SUPPORTS_VIEWS_ !
	// prepare buffer
	if( m_bUseAttachBuffer )
	{
		// buffer is allocated by user.

		ASSERT( m_image != NULL );
		if( ! m_image->allocateframes( m_nFramecount, TRUE ) )
		{
			AfxMessageBox( IDS_ERR_NOTENOUGHMEMORY );
			return FALSE;
		}
		DCAMBUF_ATTACH param;
		memset( &param, 0, sizeof(param) );
		param.size = sizeof(param);
		param.iKind = DCAMBUF_ATTACHKIND_FRAME;
		param.buffercount = m_nFramecount;
		param.buffer = m_image->getframes();
		err = dcambuf_attach( m_hdcam, &param);
		if( failed(err) )
		{
			show_dcamerrorbox( m_hdcam, err, "dcambuf_attach()" );
			return FALSE;
		}

		m_bBufferReady = TRUE;
	}
	else
	{
		// buffer is allocated by DCAM.

		ASSERT( m_image != NULL );
		m_image->freeframes();

		if( ! m_image->allocateframes( m_nFramecount, FALSE ) )
		{
			return FALSE;
		}

		m_bBufferReady = TRUE;
	}

	// status must be READY
#ifdef _DEBUG
	{
		err = dcamcap_status( m_hdcam, &status );
		ASSERT( failed(err) == FALSE && status == DCAMCAP_STATUS_READY );
	}
#endif
	// start capturing
	if (param == ID_CAPTURE_RECORD)
	{
		DCAMREC_OPEN	recopen;
		memset( &recopen, 0, sizeof(recopen) );
		recopen.size	= sizeof(recopen);

		CString sPathName;
		sPathName.Format(_T("%s%s.dcimg"), static_cast<LPCTSTR>(m_strFolderDcimg), static_cast<LPCTSTR>(m_strFilenameDcimg));
	
		recopen.path = sPathName.GetBuffer(0);
		sPathName.ReleaseBuffer();

		recopen.maxframepersession	= m_nFramecountDcimg;

		if (m_hrec == NULL) {
			err = dcamrec_open( &recopen );
			if (failed(err))
				show_dcamerrorbox(m_hdcam, err, "dcamrec_open()");
			else
				m_hrec = recopen.hrec;
		}

		if (!failed(err))
		{
			err = dcamcap_record(m_hdcam, m_hrec);
			if (failed(err))
				show_dcamerrorbox(m_hdcam, err, "dcamcap_record()");
		}
	}

	if (!failed(err))
	{
		//	err = dcamcap_start( m_hdcam, (param == ID_CAPTURE_SNAP) ?  DCAMCAP_START_SNAP : DCAMCAP_START_SEQUENCE ); 
		err = dcamcap_start(m_hdcam, (param == ID_CAPTURE_SEQUENCE) ? DCAMCAP_START_SEQUENCE : DCAMCAP_START_SNAP);
		if (failed(err))
			show_dcamerrorbox(m_hdcam, err, "dcamcap_start()");
	}

	if( failed(err) )
	{
		err = dcambuf_release( m_hdcam );
		if( failed(err) )
			show_dcamerrorbox( m_hdcam, err, "dcambuf_release()" );

		if( m_bUseAttachBuffer )
		{
			m_image->freeframes();
		}

		m_bBufferReady = FALSE;
		return FALSE;
	}

	err = dcamcap_status( m_hdcam, &status );
	ASSERT( !failed(err) && status == DCAMCAP_STATUS_BUSY );
	return TRUE;
}

long CExCapDoc::suspend_capturing()
{
	long	status;
	DCAMERR err;
	err = dcamcap_status( m_hdcam, &status );
	if( failed(err))
	{
		show_dcamerrorbox( m_hdcam, err, "dcamcap_status()" );
		return 0;
	}

	switch( status )
	{
	case DCAMCAP_STATUS_BUSY:
		err = dcamcap_stop( m_hdcam );
		if( failed(err) )
		{
			show_dcamerrorbox( m_hdcam, err, "dcamcap_stop()" );
			return 0;
		}
	case DCAMCAP_STATUS_READY:
		err = dcambuf_release( m_hdcam );
		if( failed(err) )
		{
			show_dcamerrorbox( m_hdcam, err, "dcambuf_release()" );
			return 0;
		}
		if( m_bUseAttachBuffer )
		{
			m_image->freeframes();
		}
	
		m_bBufferReady = FALSE;
	}

	return m_idCapturingSequence;
}

void CExCapDoc::resume_capturing( long param )
{
	if( param == ID_CAPTURE_SEQUENCE )
	{
		if( start_capturing( ID_CAPTURE_SEQUENCE ) )
			UpdateAllViews( NULL, start_capture );
	}
	else
	if( param == ID_CAPTURE_SNAP )
	{
		if( start_capturing( ID_CAPTURE_SNAP ) )
			UpdateAllViews( NULL, start_capture );
	}
	else
		ASSERT( param == ID_CAPTURE_IDLE );
}

//-----------

#ifdef _EXCAP_SUPPORTS_VIEWS_
BOOL CExCapDoc::is_show_allview()
{
	return (m_idShowMultiView == ID_MULTIVIEWSHOWIMAGE_ALLVIEW);
}
#endif // _EXCAP_SUPPORTS_VIEWS_ !

BOOL CExCapDoc::is_bitmap_updated()
{
	if( m_image == NULL
	 || ! m_image->is_bitmap_updated() )
		return FALSE;

	return TRUE;
}

BOOL CExCapDoc::get_bitmapinfoheader( BITMAPINFOHEADER& bmih )
{
	return m_image->get_bitmapinfoheader( bmih );
}

long CExCapDoc::numberof_capturedframes() const
{
	return m_nCapturedFramecount;
}

BOOL CExCapDoc::copy_dibits( BYTE* bits, const BITMAPINFOHEADER& bmih, long iFrame, long hOffset, long vOffset, RGBQUAD* rgb, const BYTE* lut )
{
	m_image->clear_bitmap_updated();

	long	rowbytes	= getrowbytes( bmih );
	BYTE*	dsttopleft;
	if( rowbytes < 0 )
	{
		ASSERT( bmih.biHeight > 0 );
		dsttopleft = bits - rowbytes * ( bmih.biHeight - 1 );
	}
	else
		dsttopleft = bits;

	if( iFrame != -1 && m_nCapturedFramecount > 0 )
		iFrame = ( iFrame + m_nCapturedOffset ) % m_nCapturedFramecount;

	if( m_image->copybits( dsttopleft, rowbytes, iFrame, bmih.biWidth, bmih.biHeight, hOffset, vOffset, lut ) )
	{
		BOOL bBW = (m_image->colortype() == image::colortype_bw
				 || m_image->colortype() == image::colortype_bw12l
				 || m_image->colortype() == image::colortype_bw12b );

		if( rgb != NULL && bBW )
		{
			DWORD	i;
			for( i = 0; i < bmih.biClrUsed; i++ )
			{
				rgb[ i ].rgbRed		= (BYTE)(i * 255 / ( bmih.biClrUsed - 1 ) );
				rgb[ i ].rgbGreen	= (BYTE)(i * 255 / ( bmih.biClrUsed - 1 ) );
				rgb[ i ].rgbBlue	= (BYTE)(i * 255 / ( bmih.biClrUsed - 1 ) );
				rgb[ i ].rgbReserved= 0;
			}
		}

		return TRUE;
	}

	return FALSE;
}


// CExCapDoc update

void CExCapDoc::update_pixeltype()
{
	if( m_hdcam != NULL )
	{
		DCAMERR err;

		double type;
		err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, &type );
		if( !failed(err) )
			m_nPixeltype = static_cast<DCAM_PIXELTYPE>((long)type);
	}
}


// CExCapDoc serialization

void CExCapDoc::Serialize(CArchive& ar)
{
	// Serialize is not used.
	ASSERT( 0 );

	if (ar.IsStoring())
	{
		// TODO: add storing code here
	}
	else
	{
		// TODO: add loading code here
	}
}

#ifdef SHARED_HANDLERS

// Support for thumbnails
void CExCapDoc::OnDrawThumbnail(CDC& dc, LPRECT lprcBounds)
{
	// Modify this code to draw the document's data
	dc.FillSolidRect(lprcBounds, RGB(255, 255, 255));

	CString strText = _T("TODO: implement thumbnail drawing here");
	LOGFONT lf;

	CFont* pDefaultGUIFont = CFont::FromHandle((HFONT) GetStockObject(DEFAULT_GUI_FONT));
	pDefaultGUIFont->GetLogFont(&lf);
	lf.lfHeight = 36;

	CFont fontDraw;
	fontDraw.CreateFontIndirect(&lf);

	CFont* pOldFont = dc.SelectObject(&fontDraw);
	dc.DrawText(strText, lprcBounds, DT_CENTER | DT_WORDBREAK);
	dc.SelectObject(pOldFont);
}

// Support for Search Handlers
void CExCapDoc::InitializeSearchContent()
{
	CString strSearchContent;
	// Set search contents from document's data. 
	// The content parts should be separated by ";"

	// For example:  strSearchContent = _T("point;rectangle;circle;ole object;");
	SetSearchContent(strSearchContent);
}

void CExCapDoc::SetSearchContent(const CString& value)
{
	if (value.IsEmpty())
	{
		RemoveChunk(PKEY_Search_Contents.fmtid, PKEY_Search_Contents.pid);
	}
	else
	{
		CMFCFilterChunkValueImpl *pChunk = NULL;
		ATLTRY(pChunk = new CMFCFilterChunkValueImpl);
		if (pChunk != NULL)
		{
			pChunk->SetTextValue(PKEY_Search_Contents, value, CHUNK_TEXT);
			SetChunkValue(pChunk);
		}
	}
}

#endif // SHARED_HANDLERS

// ----------------

BOOL CExCapDoc::OnOpenDocument(LPCTSTR lpszPathName) 
{
//	if (!CDocument::OnOpenDocument(lpszPathName))
//		return FALSE;

	// TODO: Add your specialized creation code here
	image*	p = image::load( lpszPathName );
	if( p != NULL )
	{
		if( m_image != NULL )
			m_image->release();

		m_disable_setpathname = FALSE;
		m_image = p;

		UpdateAllViews( NULL, image_updated );
		return TRUE;
	}

	return FALSE;
}

BOOL CExCapDoc::OnSaveDocument(LPCTSTR lpszPathName) 
{
	// TODO: Add your specialized code here and/or call the base class
	CString strPathName = lpszPathName;
	if( strPathName.IsEmpty() )
		return FALSE;

	// get filename extention
	LPCTSTR	lpszExtention = PathFindExtension( lpszPathName );
	CString	strExtention = ( lpszExtention == NULL || *lpszExtention == '\0' ? _T( ".img" ) : lpszExtention );

	// remove filename extention
	PathRemoveExtension( strPathName.LockBuffer() );
	strPathName.UnlockBuffer();

	// get count of images
	ASSERT( m_hdcam != NULL );

	DCAMERR err;

	DCAMCAP_TRANSFERINFO	transferinfo;
	memset( &transferinfo, 0, sizeof(transferinfo) );
	transferinfo.size	= sizeof(transferinfo);

	err = dcamcap_transferinfo( m_hdcam, &transferinfo );
	if( failed(err) )
	{
		ASSERT ( 0 );
		return FALSE;
	}

	// initialize frame index
	long fileindex;
	long framecount;
	if( transferinfo.nFrameCount < m_nFramecount )
	{
		framecount = transferinfo.nFrameCount;
		fileindex = 0;
	}
	else
	{
		framecount = m_nFramecount;
		if( transferinfo.nNewestFrameIndex == m_nFramecount - 1 )
			fileindex = 0;
		else
			fileindex = transferinfo.nNewestFrameIndex+1;
	}

	long	bytesperframe = dcamex_getpropvalue_imageframebytes( m_hdcam );
	ASSERT( bytesperframe > 0 );

	long rank = 0, q = framecount;

#ifdef _EXCAP_SUPPORTS_FRAMEBUNDLE_
	//
	// Check FRAMEBUNDLE MODE and get following 2 values if FRAMEBUNDLE MODE is ON.
	//

	BOOL	framebundle_mode = FALSE;
	long	framebundle_number = 1;
	long	framebundle_rowbytes = 0;
	long	framebundle_framestepbytes = 0;
	{
		double	value;
		err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_FRAMEBUNDLE_MODE, &value ); 
		if( !failed(err)
		 && value == DCAMPROP_MODE__ON )
		{
			framebundle_mode = TRUE;

			err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_FRAMEBUNDLE_ROWBYTES, &value );
			VERIFY( !failed(err) );
			framebundle_rowbytes = (long)value;

			err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_FRAMEBUNDLE_NUMBER, &value );
			VERIFY( !failed(err) );
			framebundle_number = (long)value;

			err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_FRAMEBUNDLE_FRAMESTEPBYTES, &value );
			VERIFY( !failed(err) );
			framebundle_framestepbytes = (long)value;

			// total frame count will be times of FRAMEBUNDLE_NUMBER.

			ASSERT( framebundle_number > 0 );
			q *= framebundle_number;
		}
	}

#endif // _EXCAP_SUPPORTS_FRAMEBUNDLE_ !

	while( q != 0 )
	{
		rank++;
		q /= 10;
	}
	if( rank == 0 )
	{
		// image is not captured
		ASSERT( 0 );
		return FALSE;
	}

	// save images

	for( int index=0; index<framecount; index++ )
	{
		void*	src = NULL;
		int32	rowbytes;

		if( m_bUseAttachBuffer )
		{
			// buffer is allocated by user.

			ASSERT( m_image != NULL );
			src = m_image->getframe( index, rowbytes );
		}
		else
		{
			DCAMBUF_FRAME	frame;
			memset( &frame, 0, sizeof(frame) );
			frame.size	= sizeof(frame);
			frame.iFrame= fileindex;		

			err = dcambuf_lockframe( m_hdcam, &frame );
			VERIFY( !failed(err) );

			src = frame.buf;
			rowbytes = frame.rowbytes;
		}

		if( src == NULL )
		{
			ASSERT( 0 );
		}
		else
		{
#ifdef _EXCAP_SUPPORTS_FRAMEBUNDLE_

			//
			// When FRAMEBUNDLE MODE is ON, several frames are bundled into one buffer.
			// This routine makes each frame into one file so it is nessesary to make another loop for file saving routine.
			//

			int	subindex;
			for( subindex=0; subindex<framebundle_number; subindex++ )
#endif // _EXCAP_SUPPORTS_FRAMEBUNDLE_ !
			{
				DCAMIMG	img;

				CString strFilename;
#ifdef _EXCAP_SUPPORTS_FRAMEBUNDLE_

				// suffix of file name is also times of FRAMEBUNDLE_NUMBER and added subindex.

				strFilename.Format( _T( "%s_%0*d%s" ), static_cast<LPCTSTR>(strPathName), rank, index * framebundle_number + subindex, static_cast<LPCTSTR>(strExtention) );

#else // _EXCAP_SUPPORTS_FRAMEBUNDLE_ !

				strFilename.Format( _T( "%s_%0*d%s" ), static_cast<LPCTSTR>(strPathName), rank, index, static_cast<LPCTSTR>(strExtention) );

#endif // _EXCAP_SUPPORTS_FRAMEBUNDLE_
				if( img.saveas( strFilename ) )
				{
					long pixeltype = dcamex_getpropvalue_pixeltype( m_hdcam );
					long colortype = dcamex_getpropvalue_colortype( m_hdcam );
					
					if( pixeltype == DCAM_PIXELTYPE_MONO12 )
					{
						ASSERT( colortype == DCAMPROP_COLORTYPE__BW );
						VERIFY( img.set_colortype( DCAMIMG::color_bw12l ) );
					}
					else
					if( pixeltype == DCAM_PIXELTYPE_MONO12P )
					{
						ASSERT( colortype == DCAMPROP_COLORTYPE__BW );
						VERIFY( img.set_colortype( DCAMIMG::color_bw12b ) );
					}
					else
					{
						switch( dcamex_getpropvalue_colortype( m_hdcam ) )
						{
						case DCAMPROP_COLORTYPE__RGB:	VERIFY( img.set_colortype( DCAMIMG::color_rgb ) );	break;
						case DCAMPROP_COLORTYPE__BGR:	VERIFY( img.set_colortype( DCAMIMG::color_bgr ) );	break;
						default:
							ASSERT( 0 );
						case DCAMPROP_COLORTYPE__BW:
							VERIFY( img.set_colortype( DCAMIMG::color_bw ) );
							break;
						}
					}

					VERIFY( img.set_bitsperchannel( dcamex_getpropvalue_bitsperchannel( m_hdcam ) ) );

					double cx;
					err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_IMAGE_WIDTH, &cx);
					VERIFY( !failed(err) );
					double cy;
					err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_IMAGE_HEIGHT, &cy);
					VERIFY( !failed(err) );

					long width = static_cast<long>(cx);
					long height = static_cast<long>(cy);

#ifdef _EXCAP_SUPPORTS_VIEWS_				
					double v;
					err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_NUMBEROF_VIEW, &v );
					if( !failed(err) && v > 1 )
						height *= static_cast<long>(v);
#endif //_EXCAP_SUPPORTS_VIEWS_	!		
#ifdef _EXCAP_SUPPORTS_FRAMEBUNDLE_
					if( framebundle_mode )
					{
						// the offset to next line is FRAMEBUNDLE_ROWBYTES.

						img.set_width( width, framebundle_rowbytes );
						img.set_height( height );

						VERIFY( img.saveimage( (const char*)src + framebundle_framestepbytes * subindex, framebundle_rowbytes * height ) );
					}
					else
#endif // _EXCAP_SUPPORTS_FRAMEBUNDLE_ !
					{
						if( dcamex_getpropvalue_sensormode( m_hdcam ) == DCAMPROP_SENSORMODE__AREA )
						{
							img.set_width( width, rowbytes );
							img.set_height( height );
						}
						else
						{
							img.set_numberof_line( height );
							img.set_width( width, rowbytes );
						}


						VERIFY( img.saveimage( src, bytesperframe ) );
					}

					img.saveclose();
				}
			}
		}

		// next frameindex
		if( ++fileindex >= m_nFramecount )
		{
			ASSERT( fileindex == m_nFramecount );
			fileindex = 0;
		}
	}
	
//	return CDocument::OnSaveDocument(lpszPathName);
	m_disable_setpathname = TRUE;
	return TRUE;
}

void CExCapDoc::SetPathName(LPCTSTR lpszPathName, BOOL bAddToMRU)
{
	if( ! m_disable_setpathname )
		CDocument::SetPathName( lpszPathName, bAddToMRU );
}

// CExCapDoc diagnostics

#ifdef _DEBUG
void CExCapDoc::AssertValid() const
{
	CDocument::AssertValid();
}

void CExCapDoc::Dump(CDumpContext& dc) const
{
	CDocument::Dump(dc);
}
#endif //_DEBUG


// CExCapDoc commands
void CExCapDoc::OnFileSaveAs()
{
	CString newName = _T("Capture");
	CDocTemplate* pTemplate = GetDocTemplate();
	ASSERT(pTemplate != NULL);
	// append the default suffix if there is one
	CString strExt;
	if (pTemplate->GetDocString(strExt, CDocTemplate::filterExt) && !strExt.IsEmpty())
	{
		ASSERT(strExt[0] == '.');
		int iStart = 0;
		newName += strExt.Tokenize(_T(";"), iStart);
	}

	if (!AfxGetApp()->DoPromptFileName(newName, AFX_IDS_SAVEFILE,
		  OFN_HIDEREADONLY | OFN_PATHMUSTEXIST, FALSE, pTemplate))
			return;       // don't even attempt to save

	if(!DoSave(newName))
		TRACE(traceAppMsg, 0, "Warning: File save-as failed.\n");
}

void CExCapDoc::OnUpdateFileSaveAs(CCmdUI* pCmdUI) 
{
	BOOL	bEnable	= FALSE;

	if( m_bBufferReady && m_idCapturingSequence == ID_CAPTURE_IDLE )
		bEnable = TRUE;

	pCmdUI->Enable( bEnable );
}

void CExCapDoc::OnCaptureDataframes() 
{
	// TODO: Add your command handler code here

	CDlgExcapDataframes	dlg;

	dlg.m_hdcam				= m_hdcam;

	DCAMERR err;

	double type;
	err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_IMAGE_PIXELTYPE, &type );
	if( !failed(err) )
	{
		m_nPixeltype			= static_cast<DCAM_PIXELTYPE>((long)type);
		dlg.m_nPixeltype		= m_nPixeltype;
	}
	double cx;
	err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_IMAGE_WIDTH, &cx);
	VERIFY( !failed(err) );
	double cy;
	err = dcamprop_getvalue( m_hdcam, DCAM_IDPROP_IMAGE_HEIGHT, &cy);
	VERIFY( !failed(err) );
	dlg.m_szData.cx = static_cast<long>(cx);
	dlg.m_szData.cy = static_cast<long>(cy);

	dlg.m_nFrames			= m_nFramecount;
	dlg.m_bUserAttachBuffer = m_bUseAttachBuffer;

	if( dlg.DoModal() == IDOK )
	{
		m_nPixeltype			= dlg.m_nPixeltype;
		m_nFramecount		= dlg.m_nFrames;
		m_bUseAttachBuffer	= dlg.m_bUserAttachBuffer;
	}
}

void CExCapDoc::OnUpdateCaptureDataframes(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	BOOL	bEnable	= FALSE;

	if( m_hdcam != NULL )
	{
		if( ! m_bBufferReady )
			bEnable = ( afxGetApp()->number_of_visible_controldialogs() == 0 );
	}

	pCmdUI->Enable( bEnable );
}

void CExCapDoc::OnUpdateCapture(CCmdUI* pCmdUI) 
{
	BOOL	bEnable	= FALSE;
	BOOL	bRadio	= FALSE;

	if( m_hdcam != NULL )
	{
		DCAMERR err = DCAMERR_SUCCESS;

		if( m_idCapturingSequence == ID_CAPTURE_IDLE )
		{
			bEnable = TRUE;
		}
		else
		{
			BOOL bBusy = FALSE;
			if( ( m_idCapturingSequence == ID_CAPTURE_RECORD ) && m_hrec)
			{
				DCAMREC_STATUS status;
				memset(&status, 0, sizeof(status));
				status.size = sizeof(DCAMREC_STATUS);
				dcamrec_status( m_hrec, &status );
				bBusy = ( status.flags == DCAMREC_STATUSFLAG_RECORDING );
			}
			else
			{
				long	status;
				err = dcamcap_status( m_hdcam, &status );
				if( failed(err) )
					status = DCAMCAP_STATUS_ERROR;
				bBusy = ( status == DCAMCAP_STATUS_BUSY );
			}

			if( bBusy == FALSE )
			{
				m_idCapturingSequence = ID_CAPTURE_IDLE;
				bEnable = TRUE;

				DCAMCAP_TRANSFERINFO	transferinfo;
				memset( &transferinfo, 0, sizeof(transferinfo) );
				transferinfo.size	= sizeof(transferinfo);

				err = dcamcap_transferinfo( m_hdcam, &transferinfo );
				if( failed(err) )
				{
					ASSERT( 0 );
					m_nCapturedFramecount = 0;
					m_nCapturedOffset = 0;
				}
				else
				if( transferinfo.nFrameCount < m_nFramecount )
				{
					m_nCapturedFramecount = transferinfo.nFrameCount;
					m_nCapturedOffset = 0;
				}
				else
				{
					m_nCapturedFramecount = m_nFramecount;
					m_nCapturedOffset = transferinfo.nNewestFrameIndex+1;
				}
				// Close Recording handle
				if( m_hrec != NULL )
				{
					err = dcamrec_close( m_hrec );
					VERIFY( !failed(err) );
					m_hrec = NULL;
				}
				UpdateAllViews( NULL, stop_capture );
			}
			else
			{
				bRadio = ( m_idCapturingSequence == pCmdUI->m_nID );
			}
		}
	}

	pCmdUI->Enable( bEnable );
	pCmdUI->SetRadio( bRadio );
}

void CExCapDoc::OnCaptureSequence() 
{
	ASSERT( m_hdcam != NULL );

	if( start_capturing( ID_CAPTURE_SEQUENCE ) )
	{
		m_idCapturingSequence = ID_CAPTURE_SEQUENCE;
		m_nCapturedFramecount = 0;
		m_nCapturedOffset	= 0;
		UpdateAllViews( NULL, start_capture );
	}
}

void CExCapDoc::OnCaptureSnap() 
{
	ASSERT( m_hdcam != NULL );

	if( start_capturing( ID_CAPTURE_SNAP ) )
	{
		m_idCapturingSequence = ID_CAPTURE_SNAP;
		m_nCapturedFramecount = 0;
		m_nCapturedOffset	= 0;
		UpdateAllViews( NULL, start_capture );
	}
}

void CExCapDoc::OnCaptureRecord() 
{
	ASSERT( m_hdcam != NULL );
	CDlgExcapRecord	dlg;
	dlg.m_strFolder = m_strFolderDcimg;
	dlg.m_strName = m_strFilenameDcimg;
	dlg.m_nMaxFrames = m_nFramecountDcimg;
	if (dlg.DoModal() == IDOK)
	{
		m_strFolderDcimg = dlg.m_strFolder;
		m_strFilenameDcimg = dlg.m_strName;
		m_nFramecountDcimg = dlg.m_nMaxFrames;
		if( start_capturing( ID_CAPTURE_RECORD ) )
		{
			m_idCapturingSequence = ID_CAPTURE_RECORD;
			m_nCapturedFramecount = 0;
			m_nCapturedOffset	= 0;
			UpdateAllViews( NULL, start_capture );
		}
	}
}

// ----

void CExCapDoc::OnUpdateCaptureIdle(CCmdUI* pCmdUI) 
{
	BOOL	bEnable	= FALSE;
	BOOL	bRadio	= FALSE;

	if( m_hdcam != NULL )
	{
		if( m_bBufferReady )
			bEnable = TRUE;
	}

	pCmdUI->Enable( bEnable );
	pCmdUI->SetRadio( bRadio );
}

void CExCapDoc::OnCaptureIdle() 
{
	ASSERT( m_hdcam != NULL );

	DCAMERR err = DCAMERR_SUCCESS;

	long	status;
	err = dcamcap_status( m_hdcam, &status );
	if( failed(err) )
		status = DCAMCAP_STATUS_ERROR;

	if( status == DCAMCAP_STATUS_BUSY )
	{
		err = dcamcap_stop( m_hdcam );
		if( failed(err) )
			show_dcamerrorbox( m_hdcam, err, "dcamcap_stop()" );

		DCAMCAP_TRANSFERINFO	transferinfo;
		memset( &transferinfo, 0, sizeof(transferinfo) );
		transferinfo.size	= sizeof(transferinfo);

		err = dcamcap_transferinfo( m_hdcam, &transferinfo );
		if( failed(err) )
		{
			ASSERT( 0 );
			m_nCapturedFramecount = 0;
			m_nCapturedOffset	= 0;
		}
		else
		if( transferinfo.nFrameCount < m_nFramecount )
		{
			m_nCapturedFramecount = transferinfo.nFrameCount;
			m_nCapturedOffset	= 0;
		}
		else
		{
			m_nCapturedFramecount = m_nFramecount;
			m_nCapturedOffset	= transferinfo.nNewestFrameIndex+1;
		}
	}
	else
	if( status == DCAMCAP_STATUS_READY )
	{
		ASSERT( m_bBufferReady );

		err = dcambuf_release( m_hdcam );
		if( failed(err) )
			show_dcamerrorbox( m_hdcam, err, "dcambuf_release()" );

		if( m_bUseAttachBuffer )
		{
			m_image->freeframes();
		}


		m_bBufferReady = FALSE;
		m_nCapturedFramecount = 0;
		m_nCapturedOffset	= 0;
	}
	else
		ASSERT( 0 );

	// Close Recording handle
	if( m_hrec != NULL )
	{
		err = dcamrec_close( m_hrec );
		VERIFY( !failed(err) );
		m_hrec = NULL;
	}

	m_idCapturingSequence = ID_CAPTURE_IDLE;
	UpdateAllViews( NULL, stop_capture );
}

// ----

void CExCapDoc::OnUpdateCaptureFiretrigger(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->Enable( m_hdcam != NULL );
}

void CExCapDoc::OnCaptureFiretrigger() 
{
	ASSERT( m_hdcam != NULL );
	DCAMERR err;
	err = dcamcap_firetrigger( m_hdcam );
	if( failed(err) )
		show_dcamerrorbox( m_hdcam, err, "dcamcap_firetrigger()" );
}


#ifdef _EXCAP_SUPPORTS_VIEWS_
void CExCapDoc::OnMultiViewShowImage(UINT nID)
{
	m_idShowMultiView = nID;
}

void CExCapDoc::OnUpdateMultiViewShowImage(CCmdUI* pCmdUI)
{
	pCmdUI->Enable( m_idCapturingSequence == ID_CAPTURE_IDLE );
	pCmdUI->SetCheck( pCmdUI->m_nID == m_idShowMultiView );
}
#endif // _EXCAP_SUPPORTS_VIEWS_ !
