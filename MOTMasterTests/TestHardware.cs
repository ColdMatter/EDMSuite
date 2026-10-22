using DAQ.HAL;

namespace MOTMasterTests
{
    /// <summary>A Hardware with only the output channels a test registers.</summary>
    public class TestHardware : Hardware
    {
        public TestHardware Digital(string name, string device, int port, int line)
        {
            AddDigitalOutputChannel(name, device, port, line);
            return this;
        }

        public TestHardware Analog(string name, string physicalChannel)
        {
            AddAnalogOutputChannel(name, physicalChannel);
            return this;
        }
    }
}
