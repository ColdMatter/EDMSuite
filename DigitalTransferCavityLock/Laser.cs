using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalTransferCavityLock
{
    public class Laser : DTCLLockable
    {
        protected Cavity cavity;
        public Laser(Func<double> _resource, string _feedback, Cavity reference) : base(_resource, _feedback)
        {
            cavity = reference;
        }

        public override double LockError
        {
            get
            {
                return resource() - LockLevel - cavity.LockLevel;
            }
        }

        public override void UpdateLock()
        {
            if (!locked)
                return;
            CurrentVoltage = CurrentVoltage + LockError * gain;
        }
    }
}
