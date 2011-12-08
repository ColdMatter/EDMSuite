using System;

namespace DAQ.HAL
{
    public abstract class MechanicalTranslationStage : RS232Instrument
    {
        public MechanicalTranslationStage(String visaAddress)
            : base(visaAddress)
        {
        }

        public abstract void Setup();

        public abstract void Restart();

        public abstract void ArmMove();

        public abstract void Move();
    }
}
