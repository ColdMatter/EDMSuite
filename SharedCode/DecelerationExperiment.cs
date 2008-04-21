using System;
using System.Collections.Generic;
using System.Text;

namespace DecelerationConfig
{
    public class DecelerationExperiment
    {
        public enum SwitchStructure { H_Off_V_Off, V_Off_H_Off, H_V, V_H };

        private const double h = 6.62607E-34;
        private const double u = 1.66054E-27;
        
        private Decelerator decel;
        private Molecule molecule;
        private double[] starkMap;
        private int[] state;
        private SwitchStructure structure = SwitchStructure.H_Off_V_Off; //default

        public Molecule Molecule
        {
            get { return molecule; }
            set { molecule = value; }
        }

        public Decelerator Decelerator
        {
            get { return decel; }
            set { decel = value; }
        }

        public int[] QuantumState
        {
            get { return state; }
            set { state = value; }
        }

        public SwitchStructure Structure
        {
            get { return structure; }
            set { structure = value; }
        }

        public void MakeStarkMap()
        {
            double[] field = decel.Field;
            starkMap = new double[field.Length];
            
            for (int i = 0; i < starkMap.Length; i++)
            {
                starkMap[i] = h * (molecule.B) * 
                    RotorStark.StarkShift(state[0], state[1], field[i] / (molecule.B / molecule.Dipole));
            }
        }

        public double GetPotential(double z)
        {
            int low = (int)Math.Floor((z - decel.Map.StartPoint) / decel.Map.MapResolution);
            if (low == starkMap.Length - 1) return starkMap[low];
            else
            {
                double potLow = starkMap[low];
                double potHigh = starkMap[low + 1];

                double delta = ((z - decel.Map.StartPoint) / decel.Map.MapResolution) - low;
                return ((1 - delta) * potLow) + (delta * potHigh);  //linear interpolation
            }        
        }

        public double[] GetTimingSequence(double voltage, double onPos, double offPos, double initspeed, int numberOfStages, int resonanceOrder)
        {
            decel.Voltage = voltage;
            MakeStarkMap();

            if (structure == SwitchStructure.H_Off_V_Off || structure == SwitchStructure.V_Off_H_Off)
                return MakeTimingSequenceCaseOne(onPos, offPos, initspeed, numberOfStages, resonanceOrder);
            else if (structure == SwitchStructure.H_V || structure == SwitchStructure.V_H)
                return MakeTimingSequenceCaseTwo(onPos, offPos, initspeed, numberOfStages, resonanceOrder);
            else return null; //this should never happen
        }

        // this is the algorithm to use when the switching structure is state 1 - off - state 2 - off
        // cares about the offPos parameter but ignores the resonanceOrder parameter
        // read arXiv:0803.0967 for an explanation of this algorithm
        private double[] MakeTimingSequenceCaseOne(double onPos, double offPos, double initspeed, int numberOfStages, int resonanceOrder)
        {
            double[] ontimes = new double[numberOfStages];
            double[] offtimes = new double[numberOfStages];
            double[] speeds = new double[numberOfStages];
            double[] times = new double[2 * numberOfStages];
            double integral;
            int integralPoints;
            double integralInterval;

            integralPoints = (int)Math.Floor(2 * (offPos - onPos) / decel.Map.MapResolution); // choose step sizes that match the resolution of the field map
            integralInterval = (offPos - onPos) / integralPoints; // the dz in the integral.

            ontimes[0] = onPos / initspeed; // the first turn-on time

            for (int i = 0; i < numberOfStages; i++)
            {
                integral = 0;
                for (int k = 0; k < integralPoints; k++)
                {
                    if (i != 0) integral += 0.5 * integralInterval * ((1 / newSpeed(speeds[i - 1], onPos, onPos + (integralInterval * k))) +
                        (1 / newSpeed(speeds[i - 1], onPos, onPos + (integralInterval * (k + 1)))));
                    else integral += 0.5 * integralInterval * ((1 / newSpeed(initspeed, onPos, onPos + (integralInterval * k))) +
                        (1 / newSpeed(initspeed, onPos, onPos + (integralInterval * (k + 1)))));
                }
                offtimes[i] = ontimes[i] + integral;

                if (i == 0) speeds[i] = newSpeed(initspeed, onPos, offPos);
                else speeds[i] = newSpeed(speeds[i - 1], onPos, offPos);

                if (i < numberOfStages - 1) ontimes[i + 1] = offtimes[i] + (decel.LensSpacing - (offPos - onPos)) / speeds[i];
            }
            for (int i = 0; i < numberOfStages; i++) // put all the times into a single array
            {
                times[2 * i] = ontimes[i];
                times[(2 * i) + 1] = offtimes[i];
            }
            return times;
            
        }

        // this is the algorithm to use when the switching structure is state 1 - off - state 2 - off
        // cares about the offPos parameter but ignores the resonanceOrder parameter
        // read arXiv:0803.0967 for an explanation of this algorithm
        private double[] MakeTimingSequenceCaseTwo(double onPos, double offPos, double initspeed, int numberOfStages, int resonanceOrder)
        {
            double[] ontimes = new double[numberOfStages + 1];
            double[] speeds = new double[numberOfStages];
            double integral;
            int integralPoints;
            double integralInterval;            
            double tempSpeed;
            double integralTwo = 0;

            // offPos is just onPos plus half a period (the lens spacing)
            offPos = onPos + decel.LensSpacing;

            ontimes[0] = onPos / initspeed; // the first turn-on time

            for (int i = 0; i < numberOfStages; i++)
            {
                if (i != 0) tempSpeed = speeds[i - 1];
                else tempSpeed = initspeed;

                integral = 0;
                integralPoints = (int)Math.Floor(2 * (offPos - onPos) / decel.Map.MapResolution); // choose step sizes that match the resolution of the field map
                integralInterval = (offPos - onPos) / integralPoints; // the dz in the integral
                for (int k = 0; k < integralPoints; k++) //the integral between the on and off points
                {
                    integral += 0.5 * integralInterval * ((1 / newSpeed(tempSpeed, onPos, onPos + (integralInterval * k))) +
                          (1 / newSpeed(tempSpeed, onPos, onPos + (integralInterval * (k + 1)))));
                }
                if (resonanceOrder > 1) //only do the second integral if we have to - it's the integral over an entire period
                {
                    integralTwo = 0;
                    integralPoints = (int)Math.Floor(2 * (2 * decel.LensSpacing - onPos) / decel.Map.MapResolution); // choose step sizes that match the resolution of the field map
                    integralInterval = (2 * decel.LensSpacing - onPos) / integralPoints; // the dz in the integral
                    for (int k = 0; k < integralPoints; k++) //the integral from the onPos to the end of the map
                    {
                        integralTwo += 0.5 * integralInterval * ((1 / newSpeed(tempSpeed, onPos, onPos + (integralInterval * k))) +
                              (1 / newSpeed(tempSpeed, onPos, onPos + (integralInterval * (k + 1)))));
                    }
                    integralPoints = (int)Math.Floor(2 * onPos / decel.Map.MapResolution); // choose step sizes that match the resolution of the field map
                    integralInterval = onPos / integralPoints; // the dz in the integral
                    for (int k = 0; k < integralPoints; k++) //the integral from the start of the map to the onPos
                    {
                        integralTwo += 0.5 * integralInterval * ((1 / newSpeed(tempSpeed, onPos, (integralInterval * k))) +
                              (1 / newSpeed(tempSpeed, onPos, (integralInterval * (k + 1)))));
                    }
                }

                ontimes[i + 1] = ontimes[i] + integral + ((resonanceOrder - 1) / 2) * integralTwo;

                speeds[i] = newSpeed(tempSpeed, onPos, offPos);
            }
            return ontimes;
        }

        private double newSpeed(double oldSpeed, double z1, double z2)
        {
            double W1 = GetPotential(z1);
            double W2 = GetPotential(z2);
            double returnVal = Math.Sqrt((oldSpeed * oldSpeed) + (2 * (W1 - W2) / (molecule.Mass * u)));
            return returnVal;

        }


    }
}
