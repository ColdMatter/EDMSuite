using System;
using System.Collections;

namespace DAQ.TransferCavityLock2012
{
    /// <summary>
    /// This is an interface for all the capabilities necessary for a transfer cavity lock.
    /// </summary>
    public interface TransferCavity2012Lockable
    {

        void ConfigureReadAI(int numberOfMeasurements, double sampleRate, bool triggerOnRisingEdge, bool autostart);
        double[,] ReadAI(int numberOfMeasurements);
        void DisposeAITask();
    }


}
