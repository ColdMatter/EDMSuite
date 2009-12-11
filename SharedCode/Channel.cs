using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis.EDM
{
    /// <summary>
    /// This class represents an analysis channel. A channel is simply something which has an
    /// on and an off value, and these values can be of any type. Examples could be a channel
    /// of TOF values, or simply a channel of numbers.
    /// </summary>
    [Serializable]
    public class Channel<T>
    {
        public T On;
        public T Off;
    }
}
