using NationalInstruments.DAQmx;

namespace TransferCavityLock
{
    public class ScanParameters
    {
        //Never exceed these values during the ramp!
        public const double UPPER_CC_VOLTAGE_LIMIT = 10.0; //volts CC: Cavity control
        public const double LOWER_CC_VOLTAGE_LIMIT = -10.0; //volts CC: Cavity control
      
        public double Low;
        public double High;
        public int Steps;
        public int SleepTime;
        private double stepSize;

        public double GetStepSize()
        {
            CalculateStepSize();
            return stepSize;
        }

        private void CalculateStepSize()
        {
            stepSize = (High - Low) / (Steps - 1);
        }

        public  double[] CalculateRampVoltages()
        {
            double[] voltages = new double[Steps];
            double stepSize = GetStepSize();
            for (int i = 0; i < Steps; i++)
            {
                if (Low + i * stepSize < LOWER_CC_VOLTAGE_LIMIT)
                {
                    voltages[i] = LOWER_CC_VOLTAGE_LIMIT;
                }
                else if (Low + i * stepSize > UPPER_CC_VOLTAGE_LIMIT)
                {
                    voltages[i] = UPPER_CC_VOLTAGE_LIMIT;
                }
                else { voltages[i] = Low + i * stepSize; }
            }
            return voltages;
        }
    }
}
