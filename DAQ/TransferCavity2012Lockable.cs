using System;
using System.Collections;

using NationalInstruments;

namespace DAQ.TransferCavityLock2012
{
    public class TCLReadData
    {
        public double[,] AnalogData;
        public DigitalWaveform[] DigitalData;
        public bool ReadSuccesful;
    }

    /// <summary>
    /// This is an interface for all the capabilities necessary for a transfer cavity lock.
    /// </summary>
    public interface TransferCavity2012Lockable
    {
        void ConfigureHardware(int numberOfMeasurements, double sampleRate, bool triggerOnRisingEdge, bool autostart);
        TCLReadData Read(int numberOfMeasurements);
        void DisposeReadTask();
    }


}
