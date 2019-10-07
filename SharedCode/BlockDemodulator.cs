using System;
using System.Collections.Generic;
using System.Text;

using Data;
using Data.EDM;
using EDMConfig;

namespace Analysis.EDM
{
    public abstract class BlockDemodulator
    {
        public List<Modulation> GetModulations(Block b)
        {
            List<Modulation> modulations = new List<Modulation>();

            foreach (AnalogModulation mod in b.Config.AnalogModulations)
            {
                modulations.Add(mod);
            }
            foreach (DigitalModulation mod in b.Config.DigitalModulations)
            {
                modulations.Add(mod);
            }
            foreach (TimingModulation mod in b.Config.TimingModulations)
            {
                modulations.Add(mod);
            }

            return modulations;
        }

        // -This returns, for each point in the block, its state for each of the switching channels-
        // -Each switch state is represented by a binary string converted to an unsigned int-
        public List<uint> GetSwitchStates(List<Modulation> modulations)
        {
            int blockLength = modulations[0].Waveform.Length;
            List<bool[]> wfBits = new List<bool[]>();
            foreach (Modulation mod in modulations) wfBits.Add(mod.Waveform.Bits);
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
            return switchStates;
        }
        public int[,] GetStateSigns(int numStates)
        {
            int[,] stateSigns = new int[numStates, numStates];
            for (uint i = 0; i < numStates; i++)
            {
                for (uint j = 0; j < numStates; j++)
                {
                    stateSigns[i, j] = stateSign(j, i);
                }
            }
            return stateSigns;
        }
        
        // Calculate, for a given analysis channel, whether a given state contributes positively or negatively
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
    }
}