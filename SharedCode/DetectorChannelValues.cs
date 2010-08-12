using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis.EDM
{
    /// <summary>
    /// This class is a bit funny looking. It's because it's started as a simple, efficient
    /// way to store channels corresponding to switch combinations. But because of the need
    /// to deal with non-linearities in the analysis it also stores some values that are not
    /// directly associated to switches. These are the SpecialValues and are stored and accessed
    /// differently to the normal values. Cheezy, I know.
    /// </summary>
    [Serializable]
    public class DetectorChannelValues
    {
        public double[] Values;
        public double[] Errors;
        public Dictionary<string, double[]> SpecialValues = new Dictionary<string,double[]>();

        public Dictionary<string, uint> SwitchMasks = new Dictionary<string, uint>();
        public uint GetChannelIndex(string[] switches)
        {
            if (switches[0] == "SIG") return 0;
            else
            {
                uint index = 0;
                foreach (string s in switches) index += SwitchMasks[s];
                return index;
            }
        }

        public double GetValue(string[] switches)
        {
            return Values[GetChannelIndex(switches)];
        }

        public double GetError(string[] switches)
        {
            return Errors[GetChannelIndex(switches)];
        }

        public double GetSpecialValue(string name)
        {
            return SpecialValues[name][0];
        }

        public double GetSpecialError(string name)
        {
            return SpecialValues[name][1];
        }

    }
}
