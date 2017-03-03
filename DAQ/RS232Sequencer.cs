using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DAQmx;
using NationalInstruments.VisaNS;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.Pattern
{
    /// <summary>
    /// This is a class to build a sequence of RS232 commands that is then output by the pattern generator. Since these cannot be hardware timed the output of these commands is timed using a software clock which starts at the same time as the hardware timed DAQmx tasks.
    /// </summary>
    public class RS232Sequencer
    {
        [NonSerialized]
        private bool timeOrdered = true;
        private Dictionary<string,int> latestTimes;
        private List<RS232Command> pattern;

        /// <summary>
        /// This is used to encapsulate the rs232 commands as a time-ordered list
        /// </summary>
        private struct RS232Command
        {
            private string channel;
            private string command;
            private int time;
            
            public RS232Command(string channel, string command, int time)
            {
                this.channel = channel;
                this.command = command;
                this.time = time;
            }
        }
        public RS232Sequencer()
        {
            Clear();
        }
        public void AddCommand(string channel, string command, int time)
        { 
           if (timeOrdered)
           {
               if (!latestTimes.ContainsKey(channel) || time > latestTimes[channel]) latestTimes[channel] = time;
               else throw new TimeOrderException();
           }
           pattern.Add(new RS232Command(channel, command, time));
        }

        public void Clear()
        {
            pattern = new List<RS232Command>();
            latestTimes = new Dictionary<string, int>();
        }
    }
}
