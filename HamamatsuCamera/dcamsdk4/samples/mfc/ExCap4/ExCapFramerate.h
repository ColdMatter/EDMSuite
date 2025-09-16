// ExcapFramerate.h : header file

class CExCapFramerate
{
protected:
	~CExCapFramerate();
public:
	CExCapFramerate( long stampcount = 16 );

public:
	void	release();	// terminate and release memory

	HDCAM set_hdcamwait( HDCAM hdcam,  HDCAMWAIT hwait);

	void	enter_critical();
	void	leave_critical();

	BOOL	get_latestperiod( double& lastperiod ) const;
	BOOL	get_averageperiod( double& period ) const;
	BOOL	get_minimumperiod( double& minperiod ) const;
	BOOL	get_maximumperiod( double& maxperiod ) const;

	void	get_framecount( long& total, long& lost ) const;
	void	get_eventcount( long& exposureend, long& frameready, long& stopped, long& unknown ) const;
	void	reset_framecount();
	void	reset_timestamp();

	void	set_callback( class CExCapCallback* pCallback );

	void	set_eventmask( int32 mask );

protected:
	void	start_waiting();
	void	abort_waiting( BOOL bTerminate );

static DWORD WINAPI entry_waiting( LPVOID param );
	void	on_waiting();

	void	mark_timestamp();
	LONGLONG	get_period( long end, long begin ) const;

protected:
	HDCAM		m_hdcam;
	HDCAMWAIT	m_hwait;
	int32		m_eventmask;

	BOOL	m_bTerminate;
	HANDLE	m_hThread;
	DWORD	m_idThread;

	HANDLE	m_hMutex;
	HANDLE	m_hStart;
	HANDLE	m_hPause;

	struct {
	LARGE_INTEGER	freq;
	LARGE_INTEGER*	stamp;
	long	count;	// count of stamp;
	long	iNext;
	long	iTotal;
	} m_timestamp;

	struct {
		long	total;
		long	lost;

		long	unknown;
		long	exposureend;
		long	frameready;
		long	stopped;
		long	invalidimage;	//{HPKINTERNALUSE}
	} m_framecount;
	CExCapCallback*	m_pCallback;
};
