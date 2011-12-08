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

		protected void AddAnalogInputChannel(
			String name,
			String physicalChannel,
			AITerminalConfiguration terminalConfig
			)
		{
			analogInputChannels.Add(name, new AnalogInputChannel(name, physicalChannel, terminalConfig));
		}

		protected void AddAnalogOutputChannel(String name, String physicalChannel)
		{
			analogOutputChannels.Add(name, new AnalogOutputChannel(name, physicalChannel));
		}

        protected void AddAnalogOutputChannel(String name, String physicalChannel,
                                                    double rangeLow, double rangeHigh)
        {
            analogOutputChannels.Add(name, new AnalogOutputChannel(name, physicalChannel, rangeLow, rangeHigh));
        }
        
        protected void AddDigitalOutputChannel(String name, string device, int port, int line)
		{
			digitalOutputChannels.Add(name, new DigitalOutputChannel(name, device, port, line));
		}

        protected void AddDigitalInputChannel(String name, string device, int port, int line)
        {
            digitalInputChannels.Add(name, new DigitalInputChannel(name, device, port, line));
        }

		protected void AddCounterChannel(String name, string physicalChannel)
		{
			counterChannels.Add(name, new CounterChannel(name, physicalChannel));
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
