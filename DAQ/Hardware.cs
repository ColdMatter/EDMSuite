using System;
using System.Collections;
using System.Collections.Generic;
using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
	/// <summary>
	/// This class represents the hardware that can be used for experiments. The idea of the HAL
	/// is this: in the hardware interface, members correspond to the union of the capabilites of
	/// the edm and decelerator experiments. The members are usually interface types. Particular
	/// instances that implement the hardware interface will supply concrete classes for each of
	/// generic hardware capabilities. This means as long as code is written to the hardware in this
	/// interface, it should be trivial to port to new setups. That's the idea, anyway.
	/// </summary>
	public class Hardware
	{

		private Hashtable boards = new Hashtable();
		public Hashtable Boards
		{
			get {return boards;}
		}

		private Hashtable digitalOutputChannels = new Hashtable();
		public Hashtable DigitalOutputChannels
		{
			get {return digitalOutputChannels;}
		}

        private Hashtable digitalInputChannels = new Hashtable();
        public Hashtable DigitalInputChannels
        {
            get {return digitalInputChannels;}
        }

		private Hashtable analogInputChannels = new Hashtable();
		public Hashtable AnalogInputChannels
		{
			get {return analogInputChannels;}
		}
		
		private Hashtable analogOutputChannels = new Hashtable();
		public Hashtable AnalogOutputChannels
		{
			get {return analogOutputChannels;}
		}

		private Hashtable counterChannels = new Hashtable();
		public Hashtable CounterChannels
		{
			get {return counterChannels;}
		}

        public string showDigitalName(String name)
        {
            string res = "";
            foreach (DigitalOutputChannel chn in digitalOutputChannels.Values)
            {
                if (chn.Name.Equals(name))
                {
                    if (chn.ShowAs.Equals("")) res = chn.Name;
                    else res = chn.ShowAs;
                    break;
                }
            }
            if (!res.Equals("")) return res;
            foreach (DigitalInputChannel chn in digitalInputChannels.Values)
            {
                if (chn.Name.Equals(name))
                {
                    if (chn.ShowAs.Equals("")) res = chn.Name;
                    else res = chn.ShowAs;
                    break;
                }
            }
            return res;
        }

        public string nameFromDigitalShowAs(String showAs)
        {
            string res = "";
            foreach (DigitalOutputChannel chn in digitalOutputChannels.Values)
            {
                if (chn.ShowAs.Equals(showAs))
                {
                    res = chn.Name;
                    break;
                }
            }
            if (!res.Equals("")) return res;
            foreach (DigitalInputChannel chn in digitalInputChannels.Values)
            {
                if (chn.ShowAs.Equals(showAs))
                {
                    res = chn.Name;
                    break;
                }
            }
            if (res.Equals("")) res = showAs; // default is set to showAs
            return res;
        }

        public string showAnalogName(String name)
        {
            string res = "";
            foreach (AnalogOutputChannel chn in analogOutputChannels.Values)
            {
                if (chn.Name.Equals(name))
                {
                    if (chn.ShowAs.Equals("")) res = chn.Name;
                    else res = chn.ShowAs;
                    break;
                }
            }
            if (!res.Equals("")) return res;
            foreach (AnalogInputChannel chn in analogInputChannels.Values)
            {
                if (chn.Name.Equals(name))
                {
                    if (chn.ShowAs.Equals("")) res = chn.Name;
                    else res = chn.ShowAs;
                    break;
                }
            }
            return res;
        }

        public string nameFromAnalogShowAs(String showAs) 
        {
            string res = "";
            foreach (AnalogOutputChannel chn in analogOutputChannels.Values)
            {
                if (chn.ShowAs.Equals(showAs))
                {
                    res = chn.Name;
                    break;
                }
            }
            if (!res.Equals("")) return res;
            foreach (AnalogInputChannel chn in analogInputChannels.Values)
            {
                if (chn.ShowAs.Equals(showAs))
                {
                    res = chn.Name;
                    break;
                }
            }
            if (res.Equals("")) res = showAs;
            return res;
        }

        public string showCounterName(String name) 
        {
            string res = "";
            foreach (CounterChannel chn in counterChannels.Values)
            {
                if (chn.Name.Equals(name))
                {
                    if (chn.ShowAs.Equals("")) res = chn.Name;
                    else res = chn.ShowAs;
                    break;
                }
            }
            return res;
        }

        public string nameFromCounterShowAs(String showAs)
        {
            string res = "";
            foreach (CounterChannel chn in counterChannels.Values)
            {
                if (chn.ShowAs.Equals(showAs))
                {
                    res = chn.Name;
                    break;
                }
            }
            return res;
        }
        
        protected YAGLaser yag;
		public YAGLaser YAG
		{
			get {return yag;}
		}
		
		private Hashtable instruments = new Hashtable();
		public Hashtable Instruments
		{
			get {return instruments;}
		}

        private Hashtable calibrations = new Hashtable();
        public Hashtable Calibrations
        {
            get { return calibrations; }
        }
        // You can dump anything you like in here that's specific to your experiment.
        // You can only add to it from within the Hardware subclass, but can access
        // from anywhere.
        protected Hashtable Info = new Hashtable();
        public object GetInfo(object key)
        {
            return Info[key];
        }
        //A method to check if a certain info item is contained in the hardware.
        public bool ContainsInfo(object value)
        {
            if (Info.ContainsValue(value))
                return true;
            else
                return false;
        }

		protected void AddAnalogInputChannel(
			String name,
			String physicalChannel,
			AITerminalConfiguration terminalConfig
			)
		{
			analogInputChannels.Add(name.Split('/')[0], new AnalogInputChannel(name, physicalChannel, terminalConfig));
		}

        protected void AddAnalogInputChannel(
            String name,
            String physicalChannel,
            AITerminalConfiguration terminalConfig,
            Double config
            )
        {
            analogInputChannels.Add(name.Split('/')[0], new AnalogInputChannel(name, physicalChannel, terminalConfig, config));
        }
        protected void AddAnalogInputChannel(
            String name,
            String physicalChannel,
            AITerminalConfiguration terminalConfig,
            Double inputRangeLow,
            Double inputRangeHigh
            )
        {
            analogInputChannels.Add(name, new AnalogInputChannel(name, physicalChannel, terminalConfig, inputRangeLow, inputRangeHigh));
        }


		protected void AddAnalogOutputChannel(String name, String physicalChannel)
		{
			analogOutputChannels.Add(name.Split('/')[0], new AnalogOutputChannel(name, physicalChannel));
		}

        protected void AddAnalogOutputChannel(String name, String physicalChannel,
                                                    double rangeLow, double rangeHigh)
        {
            analogOutputChannels.Add(name.Split('/')[0], new AnalogOutputChannel(name, physicalChannel, rangeLow, rangeHigh));
        }
        
        protected void AddDigitalOutputChannel(String name, string device, int port, int line)
		{
			digitalOutputChannels.Add(name.Split('/')[0], new DigitalOutputChannel(name, device, port, line));
		}

        protected void AddDigitalInputChannel(String name, string device, int port, int line)
        {
            digitalInputChannels.Add(name.Split('/')[0], new DigitalInputChannel(name, device, port, line));
        }

		protected void AddCounterChannel(String name, string physicalChannel)
		{
			counterChannels.Add(name.Split('/')[0], new CounterChannel(name, physicalChannel));
		}

        protected void AddCalibration(String channelName, Calibration calibration)
        {
            calibrations.Add(channelName, calibration);
        }
        public virtual void ConnectApplications()
        {
            // default is - do nothing
        }

	}

}
