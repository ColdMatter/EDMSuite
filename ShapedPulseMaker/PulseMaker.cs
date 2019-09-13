using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfArbitraryWaveformGenerator
{
    public static class PulseMaker
    {
        
        /// <summary>
        /// Frequency step should be integer multiple of 1/(pulse length).
        /// Amplitude range is from 0.9 to 1.1.
        /// Phase entered in radians.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="pulseLength"></param>
        /// <param name="sampleRate"></param>
        /// <param name="amplitude"></param>
        /// <param name="frequencyStepInHz"></param>
        /// <param name="phaseInRadians"></param>
        public static RfPulse MakeTopHat(string pulseName, double pulseLength, double sampleRate, double amplitude, double frequencyStepInHz, double phaseInRadians)
        {
            RfPulse rfPulse = new RfPulse(pulseName);

            int totalSamples = Convert.ToInt32(sampleRate * pulseLength * Math.Pow(10, -6));
            int totalCycles = Convert.ToInt32(frequencyStepInHz * pulseLength * Math.Pow(10, -6));
            double scaledAmplitude = amplitude * 0.9;
            double[] iData = new double[totalSamples];
            double[] qData = new double[totalSamples];

            qData = SinePattern(totalSamples, scaledAmplitude, phaseInRadians, totalCycles);
            iData = SinePattern(totalSamples, scaledAmplitude, phaseInRadians + (Math.PI / 2), totalCycles);

            rfPulse.PulseLength = pulseLength;
            rfPulse.SampleRate = sampleRate;
            rfPulse.TotalSamples = totalSamples;
            rfPulse.IData = iData;
            rfPulse.QData = qData;

            return rfPulse;
        }

        public static RfPulse MakePulse(string pulseName, double pulseLength, double sampleRate, double a0, double a1, double a2, double a3, double frequencyStepInHz, double phaseinRadians)
        {
            RfPulse rfPulse = new RfPulse(pulseName);

            int totalSamples = Convert.ToInt32(sampleRate * pulseLength * Math.Pow(10, -6));
            int totalCycles = Convert.ToInt32(frequencyStepInHz * pulseLength * Math.Pow(10, -6));
            double[] iData = new double[totalSamples];
            double[] qData = new double[totalSamples];

            qData = ThreeColourSinePattern(totalSamples, a0, a1, a2, a3, phaseinRadians, totalCycles);
            iData = ThreeColourSinePattern(totalSamples, a0, a1, a2, a3, phaseinRadians + (Math.PI / 2), totalCycles);

            rfPulse.PulseLength = pulseLength;
            rfPulse.SampleRate = sampleRate;
            rfPulse.TotalSamples = totalSamples;
            rfPulse.IData = iData;
            rfPulse.QData = qData;

            return rfPulse;
        }

        public static RfPulse MakeZeros(string pulseName, double pulseLength, double sampleRate)
        {
            RfPulse rfPulse = new RfPulse(pulseName);

            int totalSamples = Convert.ToInt32(sampleRate * pulseLength * Math.Pow(10,-6));
            double[] iData = new double[totalSamples];
            double[] qData = new double[totalSamples];

            rfPulse.PulseLength = pulseLength;
            rfPulse.SampleRate = sampleRate;
            rfPulse.TotalSamples = totalSamples;
            rfPulse.IData = iData;
            rfPulse.QData = qData;

            return rfPulse;
        }

        public static void SendPulseToFile(double[] iData, double[] qData, string fileName)
        {
            string pulseString = "";
            int arrayLength = iData.Length;

            for (int itr = 0; itr < arrayLength - 1; itr++)
            {
                pulseString += iData[itr] + "\t" + qData[itr] + Environment.NewLine;
            }

            File.WriteAllText(@fileName, pulseString);
        }

        private static double[] SinePattern(int numberOfSamples, double amplitude, double phaseRadians, double numberOfCycles)
        {
            double[] sineArray = new double[numberOfSamples];
            for (int i = 0; i < numberOfSamples; i++)
            {
                sineArray[i] = amplitude * Math.Sin(2 * Math.PI * i * numberOfCycles / numberOfSamples + phaseRadians);
            }
            return sineArray;
        }

        private static double[] ThreeColourSinePattern(int numberofSamples, double a0, double a1, double a2, double a3, double phaseRadians, double numberofCycles)
        {
            double[] sineArray = new double[numberofSamples];

            for (int i = 0; i < numberofSamples; i++)
            {
                sineArray[i] = (a0 + a1 * Math.Sin(Math.PI * i / numberofSamples) + a2 * Math.Sin(2 * Math.PI * i / numberofSamples) + a3 * Math.Sin(3 * Math.PI * i / numberofSamples)) 
                    * Math.Sin(2 * Math.PI * i * numberofCycles / numberofSamples + phaseRadians);
            }
            return sineArray;
        }

    }
}
