using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.Pattern
{
    /// <summary>
    /// This class builds a sequence of waveforms to be written to a HSDIO card. The advantage of this is that these waveforms can be looped over in a script, which will generally require a lot less onboard memory than defining the pattern for each clock cycle
    /// </summary>
    public class HSDIOPatternBuilder : PatternBuilder32
    {
        // A list of waveforms to write to the hsdio card, indexed by a name
        private UInt32[] waveforms;
        // An array of loop times for each waveform
        private int[] loopTimes;
        
        public HSDIOPatternBuilder()
        {
            //Clear();
        }

        /** Generates a pattern by dividing the layout into a sequence of static values for each waveform **/
        public override void BuildPattern(int length)
        {
            //Used to check if the sequence starts at zero time.
            bool zeroStart = true;
            // Check there are events
            if (Layout.EventTimes.Count == 0)
                throw new PatternBuildException("No events to build patterns for.");
            // Check the length is long enough
            if (length < Layout.LastEventTime + 1)
                throw new PatternBuildException("Pattern will not fit in array of requested length.\n"
                    + "Pattern length is " + Layout.LastEventTime + ". Array length is " + length);
            ArrayList times = Layout.EventTimes;
            int numberOfEvents = times.Count;
            waveforms = new UInt32[numberOfEvents];
            //make the first waveform before the first event
            if ((int)times[0] != 0)
            {
                waveforms[0] = 0;
                zeroStart = false;
            }

            //loop over the events and create a waveform for each
            for (int j = 1; j < numberOfEvents;j++)
            {
                int startTime = (int)times[j - 1];
                int endTime = (int)times[j];
                EdgeSet es = Layout.GetEdgeSet(startTime);
                UInt32 nextInt;
                if (startTime != 0 )
                {
                    //middle of a pattern - get the last value of each channel
                    UInt32 previousInt = waveforms[j - 1];
                    nextInt = GenerateNextInt(previousInt, es, true, startTime);
                }
                else
                {
                    UInt32 previousInt = 0;
                    nextInt = GenerateNextInt(previousInt, es, false, startTime);
                }
                //add this waveform
                if (zeroStart)
                    waveforms[j] = nextInt;
                else
                    waveforms[j - 1] = nextInt;
                loopTimes[j - 1] = endTime - startTime;
            }

        }

    }
}
