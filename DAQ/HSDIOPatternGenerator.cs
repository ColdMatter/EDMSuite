using System;
using System.Threading;

using NationalInstruments.ModularInstruments.Interop;

using DAQ.Environment;

namespace DAQ.HAL
{
    /// <summary>
    /// A class to control the PatternList generator using a HSDIO card. This is designed to operate similarly to the DAQMxPatternGenerator
    /// </summary>
    public class HSDIOPatternGenerator : DAQMxPatternGenerator
    {
        //The HSDIO cards do not have Task objects. Instead the card is initialised for generation or acquisition and the waveform is written to the card
        private niHSDIO hsTask;
        private string device;
        private double clockFrequency;
        private int length;
        private int[] loopTimes;

        public HSDIOPatternGenerator(String device):base(device)
        {
            this.device = device;
        }
        
        public void Configure(double clockFrequency, bool loop, int length, bool internalClock, bool triggered)
        {
            this.clockFrequency = clockFrequency;
            this.length = length;

            /**** Configure the output lines ****/
            hsTask = niHSDIO.InitGenerationSession(device, true, true,"");
            //configure the card for dynamic generation
            hsTask.AssignDynamicChannels("0-31");

            /**** Configure the clock ****/
            String clockSource = "";
            if (!internalClock) clockSource = (string)Environment.Environs.Hardware.GetInfo("HSClockLine");
            else clockSource = "";

            //TODO implement a method to export the sample clock at a lower frequency for other channels
            hsTask.ConfigureSampleClock(clockSource, clockFrequency);
            if(!triggered)
               
            
            /**** Configure regeneration ****/
            if (loop)
            {
                hsTask.ConfigureGenerationRepeat(niHSDIOConstants.Continuous, 1);
            }
            /**** Configure triggering ****/
            if (triggered)
            {
                hsTask.ConfigureDigitalEdgeStartTrigger((string)Environs.Hardware.GetInfo("HSTrigger"), niHSDIOConstants.RisingEdge);
            }
            else
            {
                //This card will trigger the others and uses the trigger line defined in the hardware class
                hsTask.ExportSignal(niHSDIOConstants.StartTrigger, "", (string)Environs.Hardware.GetInfo("HSTrigger"));
            }
            /*** Write configuration to board ****/
            hsTask.CommitDynamic();
 	        throw new NotImplementedException();
        }
        /// <summary>
        /// Accessor method for loopTimes
        /// </summary>
       public int[] LoopTimes
        {
            get { return loopTimes; }
            set { loopTimes = value; }
        } 
       
        public void OutputPattern(uint[] pattern)
        {
            //Check lengths are equal
            if (pattern.Length != loopTimes.Length)
                throw new Exception("Pattern length not equal to number of loop times");
            //loop over pattern to write the waveforms to the card and build a script string
            string script = "script myScript \n";
            uint[] data = new uint[0];
            for (int i = 0; i < loopTimes.Length;i++ )
            {
                data[0] = pattern[i];
                hsTask.WriteNamedWaveformU32("waveform_" + i, 1, data);
                script += "\t repeat " + loopTimes[i] + "\n \t generate waveform_" + i + "\n";
            }
            //Writes the script to the card
            hsTask.WriteScript(script);
            //This assumes the hsdio card triggers the other cards, which is most likely the case
            hsTask.Initiate();
            //To avoid timing issues associated with the different pattern generators, I'll add the sleeop one pattern method here
            SleepOnePattern();
        }

        private void SleepOnePattern()
		{
			int sleepTime = (int)(((double)length * 1000) / clockFrequency);
			Thread.Sleep(sleepTime);
		}
        public void StopPattern()
        {
            hsTask.Dispose();
        }
    }
}
