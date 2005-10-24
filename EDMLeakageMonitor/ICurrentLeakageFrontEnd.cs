using System;
using System.Collections;
using DAQ.Environment;
using DAQ.HAL;

namespace EDMLeakageMonitor
{
	/// <summary>
	/// Summary description for CurrentLeakageFrontEndInterface.
	/// </summary>
	public interface ICurrentLeakageFrontEnd
	{
		void HandleBackEndMessage(string message);
		void PlotDataPoints(ArrayList recentData);
	}
}
