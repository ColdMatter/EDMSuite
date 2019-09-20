using System;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;

namespace DAQ.Pattern
{
	/// <summary>
	/// A layout represents a pattern as a set of edges. This class is used
	/// internally by the pattern builder.
	/// </summary>
    [DataContract]
    [KnownType(typeof(EdgeSet))]
    [KnownType(typeof(SortedList))]
	public class Layout
	{
        [DataMember]
        public SortedList EventList = new SortedList();
		private int channels = 0;
	
		public Layout( int channels ) 
		{
			this.channels = channels;
		}
	
		public void AddEdge( int channel, int time, bool sense ) 
		{
			EdgeSet edgeSet = (EdgeSet)EventList[time];
			// is there already an edgeSet at this time ?
			if (edgeSet == null)
			{
				// If not add a new one
				edgeSet = new EdgeSet(channels);
				edgeSet.AddEdge(channel, sense);
				EventList[time] = edgeSet;
			} 
			else 
			{
				// else add the edge to the existing one
				edgeSet.AddEdge(channel, sense);
			}
		}

		public ArrayList EventTimes
		{
			get { return new ArrayList(EventList.Keys); }
		}

		public int LastEventTime
		{
			get { return (int)EventTimes[EventTimes.Count - 1]; }
		}
	
		public EdgeSet GetEdgeSet( int time ) 
		{
			return (EdgeSet)EventList[time];
		}

		public override String ToString() 
		{
			StringBuilder sb = new StringBuilder();
		
			// work out how many tabs are needed after the time field
			int maxTime = LastEventTime;
			int maxTimeLength = maxTime.ToString().Length;
			int maxTabs = maxTimeLength/4 + 1;
		
			// Header
			sb.Append("t");
			for (int i = 0 ; i < maxTabs ; i++ ) sb.Append("\t");
			for (int i = 0 ; i < channels ; i++ ) sb.Append( i + "\t" );
			sb.Append("\n");
		
			// Events
			foreach (DictionaryEntry ev in EventList)
			{
				int time = (int)ev.Key;
				EdgeSet es = (EdgeSet)ev.Value;
				// display this event
				sb.Append( time );
				// pad with tabs
				int timeLength = time.ToString().Length;
				int numTabs = maxTabs - (timeLength/4);
				for (int i = 0 ; i < numTabs ; i++ ) sb.Append("\t");
				for (int i = 0 ; i < channels ; i++ ) 
				{
					EdgeSense sense = es.GetEdge(i);
					if (sense == EdgeSense.NC) sb.Append("-\t");
					if (sense == EdgeSense.UP) sb.Append("U\t");
					if (sense == EdgeSense.DOWN) sb.Append("D\t");		
				}
				sb.Append("\n");		
			}
			return sb.ToString();
		}

	}
}
