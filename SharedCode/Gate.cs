using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;

namespace Analysis.EDM
{
    [Serializable]
    public class Gate
    {
        public int GateLow;
        public int GateHigh;
        public bool Integrate;

        public Gate(int gateLow, int gateHigh, bool integrate)
        {
            GateLow = gateLow;
            GateHigh = gateHigh;
            Integrate = integrate;
        }

        public static Gate WideGate()
        {
            return new Gate(0, 10000, true);
        }
    }
}
