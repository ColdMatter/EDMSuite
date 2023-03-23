using System.Collections.Generic;

namespace TransferCavityLock2023
{
    public class ScanParameters
    {
        public int Steps;
        public double AnalogSampleRate;
        public bool TriggerOnRisingEdge;
        public Dictionary<string, int> Channels;
    }
}
