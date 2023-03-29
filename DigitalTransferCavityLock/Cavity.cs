using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalTransferCavityLock
{
    public class Cavity : DTCLLockable
    {
        public Cavity(Func<double> _resource, string _feedback) : base(_resource, _feedback) { }

        public override void UpdateLock()
        {
            if (!locked)
                return;
            CurrentVoltage = CurrentVoltage - 0.1*VoltageError;
        }
    }
}
