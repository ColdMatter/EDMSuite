// luttable.cpp
//

#include "stdafx.h"
#include "luttable.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////

luttable::~luttable()
{
	if( m_lut != NULL )
		delete m_lut;
}

/////////////////////////////////////////////////////////////////////////////

luttable::luttable( long bitperchannel )
{
	ASSERT( 8 <= bitperchannel && bitperchannel <= 16 );

	m_lutsize = 65536;
	m_lut = new BYTE[ m_lutsize ];

	m_bitmask = ( 1 << bitperchannel ) - 1;
	m_inmin = 0;
	m_inmax = m_bitmask;

	m_bValid = FALSE;
	m_bUpdated = FALSE;
}

BOOL luttable::is_updated() const
{
	return this != NULL && m_bUpdated;
}

const BYTE*	luttable::gettable()
{
	if( m_bValid == FALSE )
	{
		ASSERT( m_lut != NULL );
		m_bValid = TRUE;
		m_bUpdated = FALSE;

		if( m_inmin == m_inmax )
		{
			int	i;
			for( i = 0; i <= m_inmin; i++ )
				m_lut[ i ] = 0;
			for( ; i <= m_bitmask; i++ )
				m_lut[ i ] = 255;
		}
		else
		{
			int	imin = ( m_inmin < 0 ? 0 : m_bitmask < m_inmin ? m_bitmask : m_inmin );
			int	imax = ( m_inmax < 0 ? 0 : m_bitmask < m_inmax ? m_bitmask : m_inmax );

			int	i;
			for( i = imin; i <= imax; i++ )
			{
				m_lut[ i ] = (BYTE)( ( i - m_inmin ) * 256 / ( m_inmax - m_inmin + 1 ) );
			}

			for( i = imin; i-- > 0; )
				m_lut[ i ] = 0;

			for( i = imax; i++ < m_bitmask; )
				m_lut[ i ] = 255;
		}
	}

	return m_lut;
}

void luttable::set_bitmask( long bitmask )
{
	if( m_bitmask != bitmask )
	{
		m_bitmask = bitmask;
		m_bValid = FALSE;
	}
}

long  luttable::get_bitmask() const
{
	return m_bitmask;
}

void luttable::set_inputrange( long max, long min )
{
	if( m_inmax != max
	 || m_inmin != min )
	{
		m_inmax = max;
		m_inmin = min;
		m_bValid = FALSE;
		m_bUpdated = TRUE;
	}
}

void luttable::get_inputrange( long& max, long& min ) const
{
	max = m_inmax;
	min = m_inmin;
}
