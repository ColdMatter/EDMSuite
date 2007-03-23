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
            this.lmArray.Add(new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["northCLeakage"]), 1, 0,.200));
            this.lmArray.Add(new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["southCLeakage"]), 1, 0, .2));
            
            //this.lmArray.Add(new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["northCLeakage"]), 0.00405203, 0.00405203 * -5263.0));
            //this.lmArray.Add(new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["southCLeakage"]), 0.00454727, 0.00454727 * -5133.0));
            this.lmArray.Add(new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["northGLeakage"]), 1, 0,.2));
            this.lmArray.Add(new LeakageMonitor(((CounterChannel)Environs.Hardware.CounterChannels["southGLeakage"]), 1, 0,.2));
		
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
