using System;

using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public class DAQMxChannel
	{
		protected String name;
		protected String physicalChannel;
        protected bool blocked;

		public String Name
		{
			get { return name; }
		}

		public String PhysicalChannel
		{
			get { return physicalChannel; }
		}

        /* An application may mark a channel as blocked.
         * The blocking is not enforced anywhere. Applications should self-regulate */ 
        public bool Blocked
        {
            get { return blocked; }
            set { blocked = value; }
        }
	}
}
