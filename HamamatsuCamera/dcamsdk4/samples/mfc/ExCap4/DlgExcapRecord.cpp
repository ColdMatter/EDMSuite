// DlgExcapRecord.cpp : implementation file
//

#include "stdafx.h"
#include "ExCap4.h"
#include "DlgExcapRecord.h"


// CDlgExcapRecord dialog

IMPLEMENT_DYNAMIC(CDlgExcapRecord, CDialog)

CDlgExcapRecord::CDlgExcapRecord(CWnd* pParent /*=NULL*/)
	: CDialog(CDlgExcapRecord::IDD, pParent)
{

}

CDlgExcapRecord::~CDlgExcapRecord()
{
}

void CDlgExcapRecord::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EXCAPRECORD_EBMAXFRAMES, m_nMaxFrames);
	DDX_Text(pDX, IDC_EXCAPRECORD_EBFOLDER, m_strFolder);
	DDX_Text(pDX, IDC_EXCAPRECORD_EBNAME, m_strName);
}


BEGIN_MESSAGE_MAP(CDlgExcapRecord, CDialog)
	ON_BN_CLICKED(IDC_EXCAPRECORD_BTNBROWSE, OnExcaprecordBtnbrowse)
END_MESSAGE_MAP()


// DlgExcapRecord message handlers
void CDlgExcapRecord::OnExcaprecordBtnbrowse()
{
	TCHAR szDir[MAX_PATH];

	LPMALLOC pMalloc;

	BROWSEINFO bi;
	memset(&bi, 0, sizeof(bi));

	bi.hwndOwner = GetSafeHwnd();
	bi.pszDisplayName = szDir; // Address of a buffer to receive the display name of the folder selected by the user
	bi.lpszTitle = _T("Select dcimg directory"); // Title of the dialog
	bi.ulFlags =  BIF_USENEWUI;
	bi.lParam = reinterpret_cast<LPARAM>(&m_strFolder);
	if (::SHGetMalloc(&pMalloc) == NOERROR)
	{
		LPITEMIDLIST lpItem = SHBrowseForFolder( &bi );

		if( lpItem != NULL ) {
			if (::SHGetPathFromIDList(lpItem, szDir))
			{ 
				m_strFolder = CString(szDir);
				if (m_strFolder.Right(1) != "\\")
					m_strFolder += "\\";
				UpdateData(FALSE);
			}
			pMalloc->Free(lpItem);
		}
		pMalloc->Release();
			
	}

}

void CDlgExcapRecord::OnOK()
{
	UpdateData(TRUE);

	CString sPathName;
	sPathName.Format(_T("%s%s.dcimg"), static_cast<LPCTSTR>(m_strFolder), static_cast<LPCTSTR>(m_strName));
	if (_taccess(sPathName,0) != -1)
	{
		CString strPrompt;
		strPrompt.Format(_T("%s already exists\nDo you want to replace it?"), static_cast<LPCTSTR>(sPathName));
		if (AfxMessageBox(strPrompt,  MB_YESNO|MB_ICONQUESTION) == IDYES) {
			DeleteFile(sPathName);
		}
		else 
			return;
	}

	CDialog::OnOK();
}
