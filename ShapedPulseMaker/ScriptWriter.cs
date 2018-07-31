using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfArbitraryWaveformGenerator
{
    public static class ScriptWriter
    {
        public static string WriteScript(Dictionary<object, RfPulse> pulseLookupTable, int[] rf1PulseSequence, int[] rf2PulseSequence)
        {
            string script;
            string rf1PulseName = "";
            string rf2PulseName = "";
            int sequenceLength = rf1PulseSequence.Length;

            script = "script OneScriptToRunThemAll" + Environment.NewLine + "\t" + "repeat forever" + Environment.NewLine;

            for (int i = 0; i < sequenceLength; i++)
            {
                rf1PulseName = pulseLookupTable[rf1PulseSequence[i]].Name;
                rf2PulseName = pulseLookupTable[rf2PulseSequence[i]].Name;

                script += "\t\t" + "clear scriptTrigger0 scriptTrigger1" + Environment.NewLine;
                script += "\t\t" + "repeat until scriptTrigger1" + Environment.NewLine;
                script += "\t\t\t" + "repeat until scriptTrigger0" + Environment.NewLine;
                script += "\t\t\t\t" + "generate waitZeros" + Environment.NewLine;
                script += "\t\t\t" + "end repeat" + Environment.NewLine;
                script += "\t\t\t" + "generate " + rf1PulseName + Environment.NewLine;
                script += "\t\t\t" + "generate timeBetweenPulsesZeros" + Environment.NewLine;
                script += "\t\t\t" + "generate " + rf2PulseName + Environment.NewLine;
                script += "\t\t" + "end repeat" + Environment.NewLine;
            }

            script += "\t" + "end repeat" + Environment.NewLine + "end script";

            return script;
        }

        public static string WritePatternScript(Dictionary<object, RfPulse> pulseLookupTable)
        {
            string script;
            string rf1PulseName = "";
            string rf2PulseName = "";

            script = "script ThisIsAScriptForPatternOnly" + Environment.NewLine + "\t" + "repeat forever" + Environment.NewLine;

            rf1PulseName = pulseLookupTable["rf1Pulse"].Name;
            rf2PulseName = pulseLookupTable["rf2Pulse"].Name;

            script += "\t\t" + "clear scriptTrigger0" + Environment.NewLine;
            script += "\t\t" + "repeat until scriptTrigger0" + Environment.NewLine;
            script += "\t\t\t" + "generate waitZeros" + Environment.NewLine;
            script += "\t\t" + "end repeat" + Environment.NewLine;
            script += "\t\t" + "generate " + rf1PulseName + Environment.NewLine;
            script += "\t\t" + "generate timeBetweenPulsesZeros" + Environment.NewLine;
            script += "\t\t" + "generate " + rf2PulseName + Environment.NewLine;
            

            script += "\t" + "end repeat" + Environment.NewLine + "end script";

            return script;
        }

        public static string WriteRepeatingScript(Dictionary<object, RfPulse> pulseLookupTable)
        {
            string script;
            string rf1PulseName = "";

            script = "script ThisIsForRf1PulseTestingPurposes" + Environment.NewLine + "\t" + "repeat forever" + Environment.NewLine;

            rf1PulseName = pulseLookupTable["rf1Pulse"].Name;

            script += "\t\t" + "generate " + rf1PulseName + Environment.NewLine;
            script += "\t\t" + "generate timeBetweenPulsesZeros" + Environment.NewLine;

            script += "\t" + "end repeat" + Environment.NewLine + "end script";

            return script;
        }
    }
}
