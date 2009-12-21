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
        public TOFChannelSet TOFDemodulateBlock(Block b)
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
            // They should probably share a base class.
            BlockDemodulator bd = new BlockDemodulator();
            for (uint i = 0; i < numStates; i++)
            {
                for (uint j = 0; j < numStates; j++)
                {
                    stateSigns[i, j] = (bd.stateSign(j, i) == 1);
                }
            }

            TOFChannelSet tcs = new TOFChannelSet();
            tcs.Config = b.Config;
            for (int channel = 0; channel < numStates; channel++)
            {
                // generate the Channel
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
                // This "if" is to take care of the case of the "SIG" channel, for which there
                // is no off TOF.
                if (tc.Off.Length != 0) tc.Difference = tc.On - tc.Off;
                else tc.Difference = tc.On;

                // add the Channel to the ChannelSet
                List<string> usedSwitches = new List<string>();
                for (int i = 0; i < modNames.Count; i++)
                    if ((channel & (1 << i)) != 0) usedSwitches.Add(modNames[i]);
                string[] channelName = usedSwitches.ToArray();
                // the SIG channel has a special name
                if (channel == 0) channelName = new string[] {"SIG"};
                tcs.AddChannel(channelName, tc);
            }
            // add the special channels
            TOFChannel eCal = (TOFChannel)tcs.GetChannel(new string[] {"E", "DB"});
            TOFChannel bShift = (TOFChannel)tcs.GetChannel(new string[] { "B" });
            TOFChannel cal = (TOFChannel)tcs.GetChannel(new string[] { "DB" });
            TOFChannel correction = (eCal * bShift) / cal;
            tcs.AddChannel(new string[] { "CORR" }, correction);

            TOFChannel eb = (TOFChannel)tcs.GetChannel(new string[] {"E", "B"});
            TOFChannel correctedEDM = eb - correction;
            tcs.AddChannel(new string[] { "EDMCORR" }, correctedEDM);

            return tcs;
        }
    }
}
