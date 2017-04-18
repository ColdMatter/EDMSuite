using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster2;



namespace MOTMaster2.SnippetLibrary
{
    public class StatePreparation : SequenceStep
    {
 
        public StatePreparation()
        {
            Console.WriteLine("No Parameter");
        }
        public StatePreparation(HSDIOPatternBuilder hs, Dictionary<String, Object> parameters,int startTime)
        {
            this.DigitalStartTime = startTime;
            AddDigitalSnippet(hs, parameters);
        }

        public StatePreparation(AnalogPatternBuilder p, Dictionary<String, Object> parameters,int startTime)
        {
            this.AnalogStartTime = startTime;
            AddAnalogSnippet(p, parameters);
        }

        public StatePreparation(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {
            AddMuquansCommands(mu, parameters);
        }
        public override void AddDigitalSnippet(PatternBuilder32 hs, Dictionary<String, Object> parameters)
        {

        }

        public override void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
           
        }

        public void AddMuquansCommands(MuquansBuilder mu, Dictionary<String, Object> parameters)
        {

        }

        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency / 1000);
        }
    }
}
