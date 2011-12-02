using System;

using DAQ.Environment;

namespace DAQ.HAL
{
    public class Parker404XR : MechanicalTranslationStage
    {
        public Parker404XR(String visaAddress)
            : base(visaAddress)
        { }

        public override void Setup()
        {
            throw new NotImplementedException();
        }

        public override void Restart()
        {
            throw new NotImplementedException();
        }

        public override void ArmMove()
        {
            throw new NotImplementedException();
        }

        public override void Move()
        {
            throw new NotImplementedException();
        }
    }
}
