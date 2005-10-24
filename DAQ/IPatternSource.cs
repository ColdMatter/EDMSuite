using System;

namespace DAQ.Pattern
{
	/// <summary>
	/// This interface provide the bridge between the pattern builder and the pattern generator.
	/// The pattern generator knows how to output IPatternSources, the pattern builder is one.
	/// </summary>
	public interface IPatternSource
	{
		UInt32[] Pattern 
		{
			get;
		}

		Int16[] PatternAsInt16s
		{
			get;
		}

		byte[] PatternAsBytes
		{
			get;
		}

		Int16[] LowHalfPatternAsInt16
		{
			get;
		}

		Int16[] HighHalfPatternAsInt16
		{
			get;
		}

		byte[] LowHalfPatternAsByte
		{
			get;
		}

		byte[] HighHalfPatternAsByte
		{
			get;
		}
	}
}
