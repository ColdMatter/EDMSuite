using System;
using System.Collections;
using System.Xml.Serialization;
using System.Text;

namespace DecelerationConfig
{
	/// <summary>
	/// Holds a switching sequence
	/// </summary>
	[Serializable]
	public class TimingSequence
	{
		public TimingSequence()
		{
		}

		private ArrayList sequence = new ArrayList();
		private String version;
		private String name;

		public String Version
		{
			get { return version; }
			set { version = value; }
		}

		public String Name
		{
			get { return name; }
			set { name = value; }
		}

		[XmlArray]
		[XmlArrayItem(Type = typeof(Edge))]
			public ArrayList Sequence
		{
			get { return sequence; }
		}

		public void Add(String channel, int time, bool sense)
		{
			Edge edge = new Edge();
			edge.Channel = channel;
			edge.Time = time;
			edge.Sense = sense;

			sequence.Add(edge);
		}

		public override String ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Version = " + version + "\n");
			sb.Append("Name = " + name + "\n");
			foreach (Edge edge in sequence) sb.Append(edge.ToString() + "\n");

			return sb.ToString();
		}
			
			


		public class Edge
		{
			private String channel;
			private int time;
			private bool sense;

			public Edge()
			{
			}

			public String Channel
			{
				get { return channel; }
				set { channel = value; }
			}

			public int Time
			{
				get { return time; }
				set { time = value; }
			}

			public bool Sense
			{
				get { return sense; }
				set { sense = value; }
			}

			public override String ToString()
			{
				return "Channel = " + channel + ", time = " + time + ", sense = " + sense;
			}

		}


	}
}
