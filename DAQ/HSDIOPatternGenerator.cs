using System;
using System.Threading;

using NationalInstruments.ModularInstruments.Interop;

using DAQ.Environment;

namespace DAQ.HAL
{
    /// <summary>
    /// A class to control the PatternList generator using a HSDIO card. This is designed to operate similarly to the DAQMxPatternGenerator
    /// </summary>
    public class HSDIOPatternGenerator : PatternGenerator
    {
        //The HSDIO cards do not have Task objects. Instead the card is initialised for generation or acquisition and the waveform is written to the card
        private niHSDIO hsdio;
        private String device;
        private double clockFrequency;
        private int length;
        
        public HSDIOPatternGenerator(String device)
        {
            this.device = device;
        }
        //TODO Implement the configure method in the style of the DAQMxPatternGenerator
        public void Configure(double clockFrequency, bool loop, bool fullWidth, bool lowGroup, int length, bool internalClock, bool triggered)
        {
            
 	        throw new NotImplementedException();
        }

        public void OutputPattern(uint[] pattern)
        {
            hsdio.WriteNamedWaveformU32("waveform",length,pattern);
            //To avoid timing issues associated with the different pattern generators, I'll add the sleeop one pattern method here
            SleepOnePattern();
 	        throw new NotImplementedException();
        }

        private void SleepOnePattern()
		{
			int sleepTime = (int)(((double)length * 1000) / clockFrequency);
			Thread.Sleep(sleepTime);
		}
        public void StopPattern()
        {
 	        throw new NotImplementedException();
        }
    }
}
