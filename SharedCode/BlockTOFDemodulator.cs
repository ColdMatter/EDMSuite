using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    /// <summary>
    /// This class takes a block and demodulates the TOFs into each analysis channel.
    /// A lot of the code is cheezily copied and pasted from the BlockDemodulator.
    /// </summary>
    public class BlockTOFDemodulator
    {
        public ChannelSet<TOFWithError> TOFDemodulateBlock(Block b)
        {
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
            bool[,] stateSigns = new bool[numStates, numStates];
            // make a BlockDemodulator just to use its stateSign code
            BlockDemodulator bd = new BlockDemodulator();
            for (uint i = 0; i < numStates; i++)
            {
                for (uint j = 0; j < numStates; j++)
                {
                    stateSigns[i, j] = (bd.stateSign(j, i) == 1);
                }
            }

            ChannelSet<TOFWithError> tcs = new ChannelSet<TOFWithError>();
            TOFChannel[] tcValues = new TOFChannel[numStates];
            for (int i = 0; i < modNames.Count; i++) tcs.SwitchMasks.Add(modNames[i], (uint)(1 << i));
            for (int channel = 0; channel < numStates; channel++)
            {
                TOFChannel tc = new TOFChannel();
                TOF tOn = new TOF();
                TOF tOff = new TOF();
                for (int i = 0; i < blockLength; i++)
                {
                    if (stateSigns[channel, switchStates[i]]) tOn += ((TOF)((EDMPoint)(b.Points[i])).Shot.TOFs[0]);
                    else tOff += ((TOF)((EDMPoint)(b.Points[i])).Shot.TOFs[0]);
                }
                tOn /= (blockLength / 2);
                tOff /= (blockLength / 2);
                tc.On = new TOFWithError(tOn);
                tc.Off = new TOFWithError(tOff);
                tcValues[channel] = tc;
            }
            tcs.Channels = tcValues;

            return tcs;
        }
    }
}
