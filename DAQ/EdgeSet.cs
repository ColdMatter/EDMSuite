using System;
using System.Collections;
using System.Runtime.Serialization;

namespace DAQ.Pattern
{
	/// <summary>
	/// A set of edges. Used internally by the pattern builder and its layout.
	/// </summary>
    [DataContract]
    [KnownType(typeof(EdgeSense[]))]
	public class EdgeSet
	{
        [DataMember]
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

    [DataContract]
    public enum EdgeSense { [EnumMember]NC, [EnumMember] UP, [EnumMember]DOWN };
}
