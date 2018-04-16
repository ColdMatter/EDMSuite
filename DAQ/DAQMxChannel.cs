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
        protected String showAs;
        public void nameIt(String extendedName)
        {
            string[] words = extendedName.Split('/');
            if (words.Length == 2)
            {
                name = words[0];
                showAs = words[1];
            }
            else
            {
                name = extendedName;
                showAs = "";
            }
        }
		protected String physicalChannel;

		public String Name
		{
			get { return name; }
		}

        public String ShowAs
        {
            get { return showAs; }
        }

		public String PhysicalChannel
		{
			get { return physicalChannel; }
		}
	}
}
