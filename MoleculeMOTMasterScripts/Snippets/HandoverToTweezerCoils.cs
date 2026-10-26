using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Hand the molecules over from the transfer coils to the tweezer-chamber coils: over
    // HandoverDur from TweezerHandoverStartTime (ticks after the Q-switch), ramp the transfer
    // coils to TransferCoilsOffValue and the tweezer coils to tMagTrapField.
    public class HandoverToTweezerCoils : MOTMasterScriptSnippet
    {
        public HandoverToTweezerCoils(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public HandoverToTweezerCoils(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }
        public HandoverToTweezerCoils(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("transferCoils");
            p.AddChannel("TweezerCoils");
            p.AddLinearRamp("transferCoils", (int)parameters["TweezerHandoverStartTime"], (int)parameters["HandoverDur"], (double)parameters["TransferCoilsOffValue"]);
            p.AddLinearRamp("TweezerCoils", (int)parameters["TweezerHandoverStartTime"], (int)parameters["HandoverDur"], (double)parameters["tMagTrapField"]);
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
        }
    }
}
