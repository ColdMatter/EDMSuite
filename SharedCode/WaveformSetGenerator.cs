using System;
using System.Collections.Generic;
using System.Text;

namespace EDMConfig
{
    public class WaveformSetGenerator
    {
        static Random r = new Random();
        const int kFastBits = 4;
        const int kFastBitThreshold = 2;
        const int kSlowBitThreshold = 4;

        // This function takes a set of static waveforms (like E and LF1) and a list of names
        // for other waveforms. It generates a set of waveforms randomly and tests them against
        // a set of criteria. If they pass they are returned. If not, a new set are generated and
        // the process repeated. There must be at least one static waveform.
        public static Dictionary<string, Waveform> GenerateWaveforms(Waveform[] staticWaveforms, string[] waveformNames)
        {
            // assume that the codeLength is given by the first static waveform's length
            int codeLength = staticWaveforms[0].CodeLength;
            // generate a set
            bool gotGoodSet = false;
            Dictionary<string, Waveform> waves = new Dictionary<string, Waveform>();
            while (!gotGoodSet)
            {
                waves.Clear();
                foreach (Waveform w in staticWaveforms) waves.Add(w.Name, w);
                foreach (string name in waveformNames)
                {
                    Waveform w = GenerateRandomWaveform(codeLength);
                    w.Name = name;
                    waves.Add(w.Name, w);
                }

                //* test the set *
                //Console.Out.WriteLine("Testing waveform set");
                // first, test the E.B waveform
                Waveform eWave = waves["E"];
                Waveform bWave = waves["B"];
                // generate the E.B code
                bool[] ebCode = new bool[codeLength];
                for (int i = 0; i < codeLength; i++)
                    ebCode[i] = eWave.Code[i] ^ bWave.Code[i];
                // count the number of fast bits set (as defined by kFastBits)
                int totalFastBits = 0;
                for (int i = codeLength - kFastBits; i < codeLength; i++) totalFastBits += ebCode[i] ? 1 : 0;
                // count the number of slow bits set
                int totalSlowBits = 0;
                for (int i = 0; i < codeLength - kFastBits; i++) totalSlowBits += ebCode[i] ? 1 : 0;
                bool passedEBTest = (totalFastBits >= kFastBitThreshold) && (totalSlowBits >= kSlowBitThreshold);
                //Console.Out.WriteLine("Passed E.B test: " + passedEBTest);

                // now check that none of the codes are identical
                bool[][] codes = new bool[waves.Count][];
                // hash the codes to integers (easier to test for equality)
                List<int> codeNums = new List<int>();
                foreach (KeyValuePair<string, Waveform> w in waves)
                    codeNums.Add(waveformCodeToInteger(w.Value.Code));
                // check the code hash list for duplicates (by adding the hashes to a hash table, confusingly!)
                bool passedUniqueTest = true;
                Dictionary<int, int> tmpDic = new Dictionary<int, int>();
                foreach (int num in codeNums)
                {
                    if (tmpDic.ContainsKey(num))
                    {
                        passedUniqueTest = false;
                        break;
                    }
                    else
                    {
                        tmpDic.Add(num, num);
                    }
                }
                //Console.Out.WriteLine("Passed uniqueness test: " + passedUniqueTest);

                // check the codes for linear independence, modulo 2
                // (what we sometimes erroneously call orthogonality).
                // The problem with linearly dependent codes is that they
                // don't sample all of the machine states in a block. As
                // far as I can tell it's easier to test for this property
                // than to test the linear indepence mod 2 of the codes.
                // ** work out the switch state for each point **
                int blockLength = staticWaveforms[0].Length;
                List<bool[]> wfBits = new List<bool[]>();
                foreach (KeyValuePair<string,Waveform> wf in waves) wfBits.Add(wf.Value.Bits);
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
                // ** now check that all switch states are represented **
                bool passedIndependenceTest = true;
                for (uint i = 0; i < (uint)Math.Pow(2, waves.Count); i++)
                    if (!switchStates.Contains(i)) passedIndependenceTest = false;
                //Console.Out.WriteLine("Passed independence test: " + passedIndependenceTest);

                // is the set is ok ?
                gotGoodSet = passedEBTest && passedUniqueTest && passedIndependenceTest;
            }
            return waves;
        }

        public static Waveform GenerateRandomWaveform(int codeLength)
        {
            Waveform w = new Waveform();
            bool[] code = new bool[codeLength];
            for (int i = 0; i < codeLength; i++) code[i] = RandomBool();
            w.Code = code;
            w.Inverted = RandomBool();
            return w;
        }

        public static bool RandomBool()
        {
            return (r.Next(2) == 1);
        }

        private static int waveformCodeToInteger(bool[] code)
        {
            int num = 0;
            for (int i = 0; i < code.Length; i++) if (code[i]) num += (int)Math.Pow(2, i);
            return num;
        }

    }


}
