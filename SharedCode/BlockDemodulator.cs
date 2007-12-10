using System;
using System.Collections.Generic;
using System.Text;
using NationalInstruments;
using NationalInstruments.Analysis.Dsp;

using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    public class BlockDemodulator
    {
        // you'll be in trouble if the number of points per block is not divisible by this number!
        private const int kFourierAverage = 16;

        public DemodulatedBlock DemodulateBlock(Block b, DemodulationConfig config)
        {
            // copy across the metadata
            DemodulatedBlock db = new DemodulatedBlock();
            db.TimeStamp = b.TimeStamp;
            db.Config = b.Config;

            // extract the gated detector data using the given config
            List<GatedDetectorData> detectorData = new List<GatedDetectorData>();
            foreach (DetectorExtractSpec gate in config.DetectorExtractSpecs) 
                detectorData.Add(GatedDetectorData.ExtractFromBlock(b, gate));

            // calculate the norm FFT
            db.NormFourier = DetectorFT.MakeFT(detectorData[1], kFourierAverage);

            // ** demodulate channels **
            // build the list of modulations
            List<string> modNames = new List<string>();
            List<Waveform> modWaveforms = new List<Waveform>();
            foreach (AnalogModulation mod in b.Config.AnalogModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }
            foreach (DigitalModulation mod in b.Config.DigitalModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }
            foreach (TimingModulation mod in b.Config.TimingModulations)
            {
                modNames.Add(mod.Name);
                modWaveforms.Add(mod.Waveform);
            }
            // work out the switch state for each point
            int blockLength = modWaveforms[0].Length;
            List<bool[]> wfBits = new List<bool[]>();
            foreach (Waveform wf in modWaveforms) wfBits.Add(wf.Bits);
            List<uint> switchStates = new List<uint>(blockLength);
            for (int i = 0; i < blockLength; i++)
            {
                uint switchState = 0;
                for (int j = 0; j < wfBits.Count; j++)
                {
                    if (wfBits[j][i]) switchState += (uint)Math.Pow(2,j);
                }
                switchStates.Add(switchState);
            }
            // debug - to be removed
            db.switchStates = switchStates;
            // the following needs to be done for each detector
            for (int detector = 0; detector < detectorData.Count; detector++)
            {
                // divide the data up into bins according to switch state
                int numStates = (int)Math.Pow(2, modWaveforms.Count);
                List<List<double>> statePoints = new List<List<double>>(numStates);
                for (int i = 0; i < numStates; i++) statePoints.Add(new List<double>(blockLength / numStates));
                for (int i = 0; i < blockLength; i++)
                {
                    statePoints[(int)switchStates[i]].Add(detectorData[detector].PointValues[i]);
                }
            }

            return db;
        }
    }
}
