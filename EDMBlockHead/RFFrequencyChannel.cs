using System;
using System.Collections.Generic;
using System.Text;

namespace EDMBlockHead.Acquire.Channels
{
	class RFFrequencyChannel : SwitchedChannel
	{
		public override bool State
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public override void AcquisitionStarting()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override void AcquisitionFinishing()
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
