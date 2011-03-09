using System;
using System.Collections;

namespace DAQ.HAL
{
	/// <summary>
	/// Represents the capabilities that something must have to be able to call itself
	/// a pattern generator.
	/// </summary>
	public interface PatternGenerator
	{

//		event DisruptivePatternChangeStartingEventHandler DisruptivePatternChangeStarting;
//		event DisruptivePatternChangeEndedEventHandler DisruptivePatternChangeEnded;
		
		void Configure(double clockFrequency, bool loop, bool fullWidth, bool lowGroup, 
                            int length, bool internalClock, string program);
		// It's important that this call blocks until the pattern is being output. Without this,
		// tweak and pg scans won't work correctly.
		void SetPattern(UInt32[] pattern);
        void SetPatternLow(UInt16[] pattern);
        void SetPatternHigh(UInt16[] pattern);
        void SetOutputMode(PatternOutputMode mode);
        void StartPattern(string program);
		void StopPattern(string program);
	}

    public enum PatternOutputMode { LOW, HIGH, ALL };

//	public delegate void DisruptivePatternChangeStartingEventHandler(object sender, EventArgs e);
//	public delegate void DisruptivePatternChangeEndedEventHandler(object sender, EventArgs e);

}
