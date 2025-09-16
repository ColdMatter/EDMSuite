// qThread.cpp
//

#include "stdafx.h"
#include "qThread.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////

const long	DEFAULT_TIMEOUT = 300;

qThread::~qThread()
{
}

qThread::qThread()
{
	m_dwThreadId = 0;

	m_hEventAbort	= CreateEvent( NULL, TRUE,  FALSE, NULL );
	m_hEventAlive	= CreateEvent( NULL, TRUE,  FALSE, NULL );
	m_hEventDead	= CreateEvent( NULL, TRUE,  TRUE,  NULL );

	m_bTerminate	= FALSE;
	m_dwTimeout		= DEFAULT_TIMEOUT;
}

BOOL qThread::create()
{
	m_bTerminate = FALSE;
	ResetEvent( m_hEventAbort );
	ResetEvent( m_hEventDead );

	m_hThread = CreateThread( NULL, 0, proc_entry, this, NULL, &m_dwThreadId );
	if( m_hThread == NULL )
		return FALSE;

	HANDLE	hEvent[ 2 ];
	hEvent[ 0 ] = m_hEventAlive;
	hEvent[ 1 ] = m_hEventDead;

	DWORD	dw = WaitForMultipleObjects( 2, hEvent, FALSE, m_dwTimeout );
	return ( dw == WAIT_OBJECT_0 );
}

BOOL qThread::destroy()
{
	m_bTerminate = TRUE;
	SetEvent( m_hEventAbort );

	VERIFY( WaitForSingleObject( m_hEventDead, m_dwTimeout * 2 ) == WAIT_OBJECT_0 );

	return TRUE;
}


DWORD WINAPI qThread::proc_entry( LPVOID pparam )
{
	ASSERT( pparam != NULL );

	qThread*	p = (qThread*)pparam;

	if( p->on_create() )
	{
		ResetEvent( p->m_hEventDead );
		SetEvent( p->m_hEventAlive );

		if( p->on_begin_run() )
			p->run();
		p->on_end_run();
	}
	ResetEvent( p->m_hEventAlive );

	p->on_destroy();

	SetEvent( p->m_hEventDead );

	return 0;
}

