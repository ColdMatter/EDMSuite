using System;

namespace EDMConfig
{
	/// <summary>
	/// A switching waveform dictates when a channel has its state switched.
	/// They are compactly represented by a binary waveform code.
	/// </summary>
	[Serializable]
	public class Waveform
	{
		private string name;
		private int codeLength;
		private int bitsLength;
		private bool[] code;
		private bool[] bits;
        public bool Inverted = false;

		/// <summary>
		/// Creates a waveform which can be represented by a code of codeLength bits.
		/// </summary>
		/// <param name="name">The name of the waveform.</param>
		/// <param name="codeLength">The number of bits in this waveform's code.</param>
		public Waveform( string name, int codeLength )
		{
			this.name = name;
			this.codeLength = codeLength;
			code = new bool[codeLength];
		}

		public Waveform()
		{}


		/// <summary>
		/// The name of the waveform.
		/// </summary>
		public string Name
		{
			get { return name; }
            set { name = value; }
		}

		/// <summary>
		/// The length of this waveform.
		/// </summary>
		public int Length
		{
			get { return bitsLength; }
		}

		/// <summary>
		/// The number of bits in the code specifying this waveform.
		/// </summary>
		public int CodeLength
		{
			get { return codeLength; }
		}
		
		/// <summary>
		/// The code that specifies this waveform.
		/// </summary>
		public bool[] Code
		{
			set 
			{
				code = value;
				codeLength = code.Length;
				GenerateBits();
			}
			get { return code; }
		}

		/// <summary>
		/// The waveform.
		/// </summary>
		public bool[] Bits
		{
			get 
			{ 
				// this takes care of Waveforms that have been serialized (the bits are not serialised)
				if (bits == null) GenerateBits();
				return bits;
			}
		}

		private void GenerateBits()
		{
			bitsLength = (int)Math.Pow(2,codeLength);
			bits = new bool[bitsLength];

			// Clear the sequence
			for ( int i = 0 ; i < bitsLength ; i++ ) bits[i] = true;

			// Step through each bit of the sequence, staring at the msb
			for ( int i = 0 ; i < codeLength ; i++ )
			{
				// does this bit contribute
				if ( code[i] == true )
				{
					//calculate the increment (which is the wavelength of the basis
					// function over 2)
					int inc = (int)Math.Pow( 2 , ( codeLength - (1 + i) ) );

					// Now, step through the sequence in blocks of inc
					for ( int k = 0 ; k < bitsLength ; k += (2 * inc) )
					{
						// and flip the bits in these blocks
						for ( int j = 0 ; j < inc ; j++ )
						{
							bits[ k + j + inc ] = !bits[ k + j + inc ];
						}
					}
				}
			}

            if (Inverted) for (int i = 0; i < bitsLength; i++) bits[i] = !bits[i];
		}

    }
}
