using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAQ.HAL
{
    public abstract class Instrument
    {

        public abstract void Connect();

        public abstract void Disconnect();

        protected abstract void Write(String command);

        protected abstract string Read();
    }
}
