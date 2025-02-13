// ExCapFiretrigger.h
//

class CExCapFiretrigger
{
protected:
	~CExCapFiretrigger();
public:
	CExCapFiretrigger();

public:
	void	release();	// terminate and release memory

	void	firetrigger( HDCAM hdcam, HDCAMWAIT hwait );

static DWORD WINAPI entry_waiting( LPVOID param );
	void	on_waiting();

protected:
	HDCAM		m_hdcam;
	HDCAMWAIT	m_hwait;

	HANDLE	m_hFire;
	HANDLE	m_hExited;

	BOOL	m_bTerminate;
	HANDLE	m_hThread;
	DWORD	m_idThread;
};
