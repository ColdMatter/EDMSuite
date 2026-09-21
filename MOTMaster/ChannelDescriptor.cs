using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOTMaster
{
    /// <summary>Describes one digital channel from any set.</summary>
    public class DigitalChannelDescriptor
    {
        public string SetName { get; }
        public int BitIndex { get; }
        public uint[] Data { get; }
        public string CustomName { get; }

        // ++ Primary label: custom name if available, else "SETNAME/CH 00"
        public string DisplayLabel => string.IsNullOrWhiteSpace(CustomName)
            ? $"{SetName}/CH {BitIndex:D2}"
            : CustomName;

        // ++ Secondary label: always "SETNAME/CH 00", but only rendered
        //    in the graph when a custom name is present
        public string SecondaryLabel => $"{SetName}/CH {BitIndex:D2}";

        public DigitalChannelDescriptor(string setName, int bitIndex,
                                        uint[] data, string customName)
        {
            SetName = setName;
            BitIndex = bitIndex;
            Data = data;
            CustomName = customName;
        }
    }

    /// <summary>Describes one analog channel from any set.</summary>
    public class AnalogChannelDescriptor
    {
        public string SetName { get; }
        public int RowIndex { get; }
        public double[,] Data { get; }
        public string CustomName { get; }
        public double Min { get; }
        public double Max { get; }

        // ++ Primary label: custom name if available, else "SETNAME/CH 00"
        public string DisplayLabel => string.IsNullOrWhiteSpace(CustomName)
            ? $"{SetName}/CH {RowIndex:D2}"
            : CustomName;

        // ++ Secondary label: always "SETNAME/CH 00", only rendered in
        //    the graph when a custom name is present
        public string SecondaryLabel => $"{SetName}/CH {RowIndex:D2}";

        public AnalogChannelDescriptor(string setName, int rowIndex,
                                       double[,] data, string customName,
                                       double min, double max)
        {
            SetName = setName;
            RowIndex = rowIndex;
            Data = data;
            CustomName = customName;
            Min = min;
            Max = max;
        }
    }
}
