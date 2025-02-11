// CExCapCallback.h
//

class CExCapCallback
{
public:
	virtual	~CExCapCallback()	{}
protected:
			CExCapCallback()	{}

public:
	virtual	void	on_dcamwait( HDCAM hdcam, HDCAMWAIT hwait, DWORD dwEvent )	{}
	virtual	void	on_lostframe( HDCAM hdcam, HDCAMWAIT hwait )					{}

};
