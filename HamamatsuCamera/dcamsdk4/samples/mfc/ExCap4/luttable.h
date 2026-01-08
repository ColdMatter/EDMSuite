// luttable.h
//

class luttable
{
public:
			~luttable();
			luttable( long bitperchannel );

public:
			const BYTE*	gettable();

			void	set_bitmask( long bitmask );
			long	get_bitmask() const;

			void	set_inputrange( long max, long min );
			void	get_inputrange( long& max, long& min ) const;

			BOOL	is_updated() const;

protected:
	BYTE*	m_lut;
	long	m_lutsize;

	BOOL	m_bUpdated;
	BOOL	m_bValid;
	long	m_bitmask;

	long	m_inmin;
	long	m_inmax;
};
