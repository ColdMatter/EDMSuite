using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Hand the molecules over from the MOT coils to the transfer (transport) coils: over
    // HandoverDur from TransferHandoverStartTime (ticks after the Q-switch), ramp the MOT
    // coils to 0 and the transfer coils to 10 V.
    public class HandoverToTransferCoils : MOTMasterScriptSnippet
    {
        public HandoverToTransferCoils(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public HandoverToTransferCoils(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }
        public HandoverToTransferCoils(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddChannel("transferCoils");
            p.AddLinearRamp("MOTCoilsCurrent", (int)parameters["TransferHandoverStartTime"], (int)parameters["HandoverDur"], 0.0);
            p.AddLinearRamp("transferCoils", (int)parameters["TransferHandoverStartTime"], (int)parameters["HandoverDur"], 10.0);
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
        }
    }
}
