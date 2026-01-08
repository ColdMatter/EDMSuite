// qThread.h
//

class qThread
{
protected:
			~qThread();
			qThread();

public:
			BOOL	create();
			BOOL	destroy();

	virtual	BOOL	on_create()		{	return TRUE;	}	// FALSE means termination.
	virtual	BOOL	on_begin_run()	{	return TRUE;	}	// FALSE means termination.
	virtual	void	run() = 0;
	virtual	void	on_end_run()	{}	// on_end_run() is called even on_begin_run() returns FALSE.
	virtual	void	on_destroy()	{}	// on_destroy() is always called even on_create() returns FALSE.

protected:
	static DWORD WINAPI proc_entry( LPVOID pParam );

protected:
	BOOL		m_bTerminate;

	HANDLE		m_hThread;
	DWORD		m_dwThreadId;
	DWORD		m_dwTimeout;

	// trigger event
	HANDLE		m_hEventAbort;

	// status event
	HANDLE		m_hEventAlive;
	HANDLE		m_hEventDead;
};

