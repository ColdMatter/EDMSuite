using System;
using System.Collections;

namespace DAQ.Pattern
{
	/// <summary>
	/// A set of edges. Used internally by the pattern builder and its layout.
	/// </summary>
	public class EdgeSet
	{
		private EdgeSense[] edges;
		
		public EdgeSet( int channels ) 
		{
			edges = new EdgeSense[channels];
			for ( int i = 0 ; i < channels ; i++ ) 
			{
				edges[i] = EdgeSense.NC;
			}
		}
		
		public void AddEdge( int channel, bool sense ) 
		{
			if (sense) edges[channel] = EdgeSense.UP;
			else edges[channel] = EdgeSense.DOWN;
		}
					
		public EdgeSense GetEdge(int channel) 
		{
			return edges[channel];
		}
	}

	public enum EdgeSense { NC, UP, DOWN };
}
