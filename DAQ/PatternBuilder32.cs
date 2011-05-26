using System;
using System.Collections;
using System.Text;

using DAQ.HAL;
using DAQ.Environment;

namespace DAQ.Pattern
{
	/// <summary>
	/// A class for building patterns that can be output by a NI pattern generator.
	/// To use this class, subclass it, and add your own structure. This class provides
	/// the primitives edge and pulse. Everything else is up to you.
	/// </summary>
	public class PatternBuilder32 : IPatternSource
	{
		private bool timeOrdered = true;
		private Layout layout;
		private UInt32[] pattern;
		private Int16[] patternInt16;
		private byte[] bytePattern;
		private int[] latestTimes;

		// Build a table of bit -> int conversions
		private UInt32[] bitValues = new UInt32[32];

		public PatternBuilder32() 
		{
			for (int i = 0 ; i < 32 ; i++) 
			{
				UInt32 tmp = 1;
				bitValues[i] = tmp << i;
			}
			Clear();
		}

		/** Add an edge to a pattern. All pattern trees must have addEdges as their terminals
		 * (either directly or through <code>pulse</code> which is just two <code>addEdge</code>s
		 */
		public void AddEdge( int channel, int time, bool sense )
		{
			if (timeOrdered) 
			{
				// check the time ordering
				if ( time > latestTimes[channel] ) latestTimes[channel] = time;
				else throw new TimeOrderException();
			}
			// add the edge
			layout.AddEdge(channel, time, sense);
		}
        public void AddEdge(string channel, int time, bool sense)
        {
            AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[channel]).BitNumber
                , time, sense);
        }
		/** Convenience method to add two edges. */
		public int Pulse(int startTime, int delay, int duration, int channel )
		{
			AddEdge(channel, startTime + delay, true );
			AddEdge(channel, startTime + delay + duration, false );
		
			return delay + duration;
		}
        public int Pulse(int startTime, int delay, int duration, string channel)
        {
            return Pulse(startTime,delay,duration,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[channel]).BitNumber);
        }
		/** Adds a downward going pulse **/
		public int DownPulse(int startTime, int delay, int duration, int channel )
		{
			AddEdge(channel, startTime, true);
			AddEdge(channel, startTime + delay, false );
			AddEdge(channel, startTime + delay + duration, true );
		
			return delay + duration;
		}
        public int DownPulse(int startTime, int delay, int duration, string channel)
        {
            return DownPulse(startTime, delay, duration,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[channel]).BitNumber);
        }
		/** Convenience method to determine the channel number from a NI port/line spec. */
		public static int ChannelFromNIPort(int port, int line) 
		{
			return line + (8 * port);
		}		

		/** Clear the pattern. */	
		public void Clear() 
		{
			layout = new Layout(32);
			latestTimes = new int[32];
			for (int i = 0 ; i < 32 ; i++) latestTimes[i] = -1;
			pattern = null;
		}
	
		/** Get the layout for this pattern. */
		public Layout Layout 
		{
			get { return layout; }
		}

		// Methods for controlling the level of error checking
		/** Used to enable or disable time order checks when pattern building. If time
		 * order checking is enabled, adding an edge to a channel at a time earlier than
		 * the latest edge on that channel will throw a <code>TimeOrderException</code>
		 */
		public bool EnforceTimeOrdering
		{
			set { timeOrdered = value; }
		}
	
		// Methods for building the pattern from its layout, and getting the
		// resulting pattern
	
		/** Return the minimum length array that this pattern can fit into. */
		public int GetMinimumLength() 
		{
			return layout.LastEventTime + 1;
		}

		/** Generate the pattern. */
		public void BuildPattern( int length ) 
		{
			// Are there any events ?
			if (layout.EventTimes.Count == 0)
				throw new PatternBuildException("No events to build pattern for.");
		
			// Is that big enough ?
			if ( length < layout.LastEventTime + 1 )
				throw new PatternBuildException("Pattern will not fit in array of requested length.\n"
                    + "Pattern length is " + layout.LastEventTime + ". Array length is " + length);
		
			// make the pattern array
			pattern = new UInt32[length];
		
			// Get the event times and fill in the gaps
			ArrayList times = layout.EventTimes;
			int numberOfEvents = times.Count;
			// first the time before the first event, if there is any
			for (int i = 0 ; i < (int)times[0] ; i++ ) 
			{
				pattern[i] = 0;
			}
			int endTime = 0;
			for (int j = 1 ; j < numberOfEvents ; j++ ) 
			{
				int startTime = (int)times[j-1];
				endTime = (int)times[j];
				EdgeSet es = layout.GetEdgeSet(startTime);
				UInt32 nextInt;
				if (startTime != 0) 
				{
					// middle of a pattern
					UInt32 previousInt = pattern[startTime - 1];
					nextInt = GenerateNextInt( previousInt, es, true, startTime );
				} 
				else 
				{
					// start of a pattern, special case
					UInt32 previousInt = 0;
					nextInt = GenerateNextInt( previousInt, es, false, startTime );
				}
				// fill in the pattern
				for ( int i = startTime ; i < endTime ; i++ ) 
				{
					pattern[i] = nextInt;
				}
			}
			// pad up to the end
			UInt32 padInt;
			if (endTime == 0) padInt = 0;
			else padInt = GenerateNextInt( pattern[endTime-1], layout.GetEdgeSet(endTime), true, endTime );
			for (int i = endTime ; i < length ; i++ ) 
			{
				pattern[i] = padInt;
			}

			GenerateInt16Pattern();
			GenerateBytePattern();
		
		}
	
		private UInt32 GenerateNextInt( UInt32 previousInt, EdgeSet es, bool throwError, int time)
		{
			// build a bit mask for the upwards edges
			UInt32 upMask = 0;
			for (int i = 0 ; i < 32 ; i++) if (es.GetEdge(i) == EdgeSense.UP) upMask = upMask | bitValues[i];
			// and the downwards edges
			UInt32 downMask = 0;
			for (int i = 0 ; i < 32 ; i++) if (es.GetEdge(i) == EdgeSense.DOWN) downMask = downMask | bitValues[i];
	  	
			// error checking
			if (throwError) 
			{
				if ( (upMask & previousInt) != 0 )
					throw new PatternBuildException("Edge conflict on upward edge at time " + time);
				if ( (downMask & ~previousInt) != 0 )
					throw new PatternBuildException("Edge conflict on downward edge at time " + time);
			}
			UInt32 returnInt = previousInt | upMask;
			returnInt = ~(~returnInt | downMask);
			return returnInt;
		}
	
		/** Get the pattern - you must call <code>generatePattern()</code> first.
		 */
		public UInt32[] Pattern 
		{
			get { return pattern; }
		}

		public Int16[] PatternAsInt16s
		{
			get 
			{
				return patternInt16;
			}
		}

		public byte[] PatternAsBytes
		{
			get 
			{
				return bytePattern;
			}
		}

		// gets the low word of the pattern so that you can run the pattern generator in half-width mode
		public Int16[] LowHalfPatternAsInt16
		{
			get
			{
				Int16[] patternInt16 = new Int16[pattern.Length];
				for (int i = 0 ; i < pattern.Length ; i++)
				{
					Int16 lowWord = (Int16)(pattern[i] & 0x0000ffff);
					patternInt16[i] = lowWord;
				}
				return patternInt16;
			}
		}

		// gets the high word of the pattern so that you can run the pattern generator in half-width mode
		public Int16[] HighHalfPatternAsInt16
		{
			get
			{
				Int16[] patternInt16 = new Int16[pattern.Length];
				for (int i = 0 ; i < pattern.Length ; i++)
				{
					Int16 highWord = (Int16)((pattern[i] & 0xffff0000) >> 16);
					patternInt16[i] = highWord;
				}
				return patternInt16;
			}
		}

		public byte[] LowHalfPatternAsByte
		{
			get
			{
				byte[] bytePattern = new byte[2 * pattern.Length];
				for (int i = 0; i < pattern.Length; i++)
				{
					byte one = (byte) (pattern[i] & 0x000000ff);
					byte two = (byte) ((pattern[i] & 0x0000ff00) >> 8);
					bytePattern[2*i] = one;
					bytePattern[2*i + 1] = two;
				}
				return bytePattern;
			}
		}

		public byte[] HighHalfPatternAsByte
		{
			get
			{
				byte[] bytePattern = new byte[2 * pattern.Length];
				for (int i = 0; i < pattern.Length; i++)
				{
					byte three = (byte) ((pattern[i] & 0x00ff0000) >> 16);
					byte four = (byte) ((pattern[i] & 0xff000000) >> 24);
					bytePattern[2*i] = three;
					bytePattern[2*i + 1] = four;
				}
				return bytePattern;
			}
		}


		private void GenerateInt16Pattern()
		{
			// TODO: I learn that there's a BitConverter class that can do this a bit more nicely
			patternInt16 = new Int16[pattern.Length * 2];
			for (int i = 0 ; i < pattern.Length ; i++)
			{
				Int16 lowWord = (Int16)(pattern[i] & 0x0000ffff);
				Int16 highWord = (Int16)((pattern[i] & 0xffff0000) >> 16);
				// the order of these bytes is not obvious !!! By swapping these,
				// the word endianism can be changed
				patternInt16[2*i] = lowWord;
				patternInt16[2*i + 1] = highWord;
			}
		}

		private void GenerateBytePattern()
		{
			bytePattern = new byte[pattern.Length * 4];
			for (int i = 0; i < pattern.Length; i++)
			{
				byte one = (byte) (pattern[i] & 0x000000ff);
				byte two = (byte) ((pattern[i] & 0x0000ff00) >> 8);
				byte three = (byte) ((pattern[i] & 0x00ff0000) >> 16);
				byte four = (byte) ((pattern[i] & 0xff000000) >> 24);
				bytePattern[4*i] = one;
				bytePattern[4*i + 1] = two;
				bytePattern[4*i + 2] = three;
				bytePattern[4*i + 3] = four;
			}
		}

		// Methods for displaying the pattern and layout
	
		/** Display the binary representation of the pattern. */
		public String ArrayToString() 
		{
			StringBuilder sb = new StringBuilder(pattern.Length * 33);
			for (int i = 0 ; i < pattern.Length ; i++) 
			{
				for (int j = 0 ; j < 32 ; j++) 
				{
					bool bit = ((pattern[i] & bitValues[j]) != 0);
					if (bit) sb.Append("1");
					else sb.Append("0");
				}
				sb.Append("\n");
			}
			return sb.ToString();
		}

		/** Display the pattern's layout as a list of edge events. */
		public String LayoutToString()
		{
			return layout.ToString();
		}


	}

	public class TimeOrderException : ApplicationException {}
	public class PatternBuildException : ApplicationException 
	{
		public PatternBuildException(String message) : base(message) {}
	}
}
