using System;
using System.Collections.Generic;
using System.Text;

namespace EDMConfig
{
    class WaveformSetGenerator
    {
        // This function takes a set of static waveforms (like E and LF1) and a list of names
        // for other waveforms. It generates a set of waveforms randomly and tests them against
        // a set of criteria. If they pass they are returned. If not, a new set are generated and
        // the process repeated. There must be at least one static waveform.
        static Waveform[] GenerateWaveforms(Waveform[] staticWaveforms, string[] waveformNames)
        {
            // assume that the codeLength is given by the first static waveform's length
            int codeLength = staticWaveforms[0].CodeLength;
            return new Waveform[4];
        }

    }


}
