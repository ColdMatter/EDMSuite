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
        protected double gain = 0;
        public Laser(Func<double> _resource, string _feedback, Cavity reference) : base(_resource, _feedback)
        {
            cavity = reference;
        }

        public override double VoltageError
        {
            get
            {
                return resource() - lockLevel - cavity.LockLevel;
            }
        }

        public void ArmLock(double LockLevel, double Gain)
        {
            ArmLock(LockLevel);
            gain = Gain;
        }

        public override void UpdateLock()
        {
            if (!locked)
                return;
            CurrentVoltage = CurrentVoltage + VoltageError * gain;
        }
    }
}
