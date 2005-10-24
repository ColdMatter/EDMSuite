using System;
using System.Collections;
using DAQ.Environment;
using DAQ.HAL;


namespace EDMLeakageMonitor
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class CurrentLeakageBackEnd
	{
		private ICurrentLeakageFrontEnd frontEnd;
		private bool breakAcquisition = false;	
		private ArrayList lmArray;
		public bool BreakAcquisition
		{
			set
			{ 
				breakAcquisition = value;
			}
			get
			{
				return breakAcquisition;
			}
		}



		public CurrentLeakageBackEnd(ICurrentLeakageFrontEnd frontEnd)
		{
			this.frontEnd = frontEnd;
			lmArray = new ArrayList();
			this.lmArray.Add( new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["currentLeakageMonitorNorth-C"]),5049, 5040) );
			this.lmArray.Add( new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["currentLeakageMonitorNorth-G"]),1, 5000) );
			this.lmArray.Add( new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["currentLeakageMonitorSouth-C"]),4976, 4990) );
			this.lmArray.Add( new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["currentLeakageMonitorSouth-G"]),1, 5000) );
		
		}

		public void start()
		{
			ArrayList latestData = new ArrayList();

			foreach( LeakageMonitor m in this.lmArray)
			{
				m.Initialize();
			}  

			while(!breakAcquisition)
			{
				foreach( LeakageMonitor m in this.lmArray)
				{
					latestData.Add(	m.GetCurrent() );
				}   
     
				frontEnd.PlotDataPoints(latestData);
				latestData.Clear();
			}

		
		}


	}
}
