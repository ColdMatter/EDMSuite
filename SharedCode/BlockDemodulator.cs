using System;
using System.Collections.Generic;
using System.Text;
using NationalInstruments;
using NationalInstruments.Analysis.Math;
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
            // *** copy across the metadata ***
            DemodulatedBlock db = new DemodulatedBlock();
            db.TimeStamp = b.TimeStamp;
            db.Config = b.Config;

            // *** extract the gated detector data using the given config ***
            List<GatedDetectorData> detectorData = new List<GatedDetectorData>();
            foreach (DetectorExtractSpec gate in config.DetectorExtractSpecs)
                detectorData.Add(GatedDetectorData.ExtractFromBlock(b, gate));

            // calculate the norm FFT
            db.NormFourier = DetectorFT.MakeFT(detectorData[1], kFourierAverage);

            // *** demodulate channels ***
            // ** build the list of modulations **
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
            // ** work out the switch state for each point **
            int blockLength = modWaveforms[0].Length;
            List<bool[]> wfBits = new List<bool[]>();
            foreach (Waveform wf in modWaveforms) wfBits.Add(wf.Bits);
            List<uint> switchStates = new List<uint>(blockLength);
            for (int i = 0; i < blockLength; i++)
            {
                uint switchState = 0;
                for (int j = 0; j < wfBits.Count; j++)
                {
                    if (wfBits[j][i]) switchState += (uint)Math.Pow(2, j);
                }
                switchStates.Add(switchState);
            }
            // pre-calculate the state signs for each analysis channel
            // the first index selects the analysis channel, the second the switchState
            int numStates = (int)Math.Pow(2, modWaveforms.Count);
            int[,] stateSigns = new int[numStates, numStates];
            for (uint i = 0; i < numStates; i++)
            {
                for (uint j = 0; j < numStates; j++)
                {
                    stateSigns[i, j] = stateSign(j, i);
                }
            }
             
            // ** the following needs to be done for each detector **
            for (int detector = 0; detector < detectorData.Count; detector++)
            {
                DetectorChannelValues dcv = new DetectorChannelValues();
                for (int i = 0; i < modNames.Count; i++) dcv.SwitchMasks.Add(modNames[i], (uint)(1 << i));
                // * divide the data up into bins according to switch state *
                List<List<double>> statePoints = new List<List<double>>(numStates);
                for (int i = 0; i < numStates; i++) statePoints.Add(new List<double>(blockLength / numStates));
                for (int i = 0; i < blockLength; i++)
                {
                    statePoints[(int)switchStates[i]].Add(detectorData[detector].PointValues[i]);
                }
                // * work out the state means *
                List<double> stateMeans = new List<double>(numStates);
                foreach (List<double> state in statePoints) stateMeans.Add(Statistics.Mean(state.ToArray()));
                // * calculate the channel values *
                double[] channelValues = new double[numStates];
                for (int channel = 0; channel < numStates; channel++)
                {
                    double chanVal = 0;
                    for (int i = 0; i < numStates; i++) chanVal += stateSigns[channel, i] * stateMeans[i];
                    chanVal /= (double)numStates;
                    channelValues[channel] = chanVal;
                }
                dcv.Values = channelValues;
                // * calculate the channel errors *
                // build the covariance matrix
                // zero the means of the states
                List<List<double>> offsetStatePoints = new List<List<double>>(numStates);
                for (int i = 0; i < numStates; i++)
                {
                    offsetStatePoints.Add(new List<double>(blockLength / numStates));
                    foreach (double d in statePoints[i]) offsetStatePoints[i].Add(d - stateMeans[i]);
                }
                // calculate the covariance matrix elements
                double[,] covarianceMatrix = new double[numStates, numStates];
                // somewhat non-obvious order is to take advantage of symmetry of
                // covariance matrix
                for (int i = 0; i < numStates; i++) 
                    covarianceMatrix[i,i] = covariance(offsetStatePoints[i], offsetStatePoints[i]);
                for (int i = 0; i < numStates - 1; i++)
                {
                    for (int j = i + 1; j < numStates; j++)
                    {
                        double cov = covariance(offsetStatePoints[i], offsetStatePoints[j]);
                        covarianceMatrix[i,j] = cov;
                        covarianceMatrix[j,i] = cov;
                    }
                }
                // calculate diagonal elements of transformed covariance matrix.
                // The transformed matrix is sign.cov.sign^T where sign is the
                // matrix of signs for states calculated above and cov is the
                // covariance matrix, ^T is the transpose.
                // There's some normalisation to take of as well - see the last line
                // of the loop.
                // This could be optimised if need be, again using the symmetry properties
                // of the covariance matrix for a factor 2 or so speed up.
                double[] channelErrors = new double[numStates];
                for (int channel = 0; channel < numStates; channel++)
                {
                    double sum = 0;
                    for (int k = 0; k < numStates; k++)
                    {
                        for (int l = 0; l < numStates; l++)
                        {
                            sum += stateSigns[channel, l] * covarianceMatrix[l, k] * stateSigns[channel, k];
                        }
                    }
                    channelErrors[channel] = Math.Sqrt(sum / (blockLength * numStates));
                }
                dcv.Errors = channelErrors;
                db.ChannelValues.Add(dcv);
            }

            return db;
        }

        // calculate, for a given analysis channel, whether a given state contributes
        // positively or negatively
        public int stateSign(uint state, uint analysisChannel)
        {
            uint maskedState = state & analysisChannel;
            if (parity(maskedState) == parity(analysisChannel)) return 1;
            else return -1;
        }

        // now _this_ is code! Old skool!!!
        public uint parity(uint p)
        {
            p = p ^ (p >> 16);
            p = p ^ (p >> 8);
            p = p ^ (p >> 4);
            p = p ^ (p >> 2);
            p = p ^ (p >> 1);
            return p & 1; 
        }

        // this function assumes that the lists have zero mean!
        // the lists need to be the same size, but this is not checked.
        // This is an "unbiased" covariance - not the -1 in the denominator.
        public double covariance(List<double> offsetList1, List<double> offsetList2)
        {
            double cov = 0;
            for (int i = 0; i < offsetList1.Count; i++) cov += offsetList1[i] * offsetList2[i];
            return cov / (double)(offsetList1.Count - 1);
        }
    }
}
