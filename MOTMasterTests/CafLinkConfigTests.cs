using System;
using System.Collections;
using DAQ.HAL;
using static MOTMasterTests.TestRunner;

namespace MOTMasterTests
{
    /// <summary>Consistency checks on the real PH-BONESAW (MoleculeMOTHardware) -> RbTweezer link configuration.</summary>
    public static class CafLinkConfigTests
    {
        public static void TestCafLinkConfigIsConsistent()
        {
            var caf = new MoleculeMOTHardware();
            var link = caf.GetInfo(MOTMasterLinkConfig.InfoKey) as MOTMasterLinkConfig;
            Assert.True(link != null, "MoleculeMOTHardware has a MOTMasterLinkConfig");

            // trigger channel: registered digital output, on the master pattern board
            string master = (string)caf.GetInfo("PatternGeneratorBoard");
            Assert.True(caf.DigitalOutputChannels.ContainsKey(link.TriggerChannel),
                "TriggerChannel " + link.TriggerChannel + " is a registered digital output channel");
            var trigger = (DigitalOutputChannel)caf.DigitalOutputChannels[link.TriggerChannel];
            Assert.Equal(master, trigger.Device, "TriggerChannel is on the master pattern board");

            Assert.True(link.AnalogStartChannels != null && link.AnalogStartChannels.Length > 0, "AnalogStartChannels is set");
            foreach (string name in link.AnalogStartChannels)
                Assert.True(caf.DigitalOutputChannels.ContainsKey(name), "AnalogStartChannel " + name + " is a registered digital output channel");

            Assert.Equal(typeof(RbTweezerHardware), Type.GetType(link.SecondaryHardwareType, true), "SecondaryHardwareType");

            // a name registered on both machines would be ambiguous in a linked script
            var rb = new RbTweezerHardware();
            AssertDisjoint(caf.DigitalOutputChannels, rb.DigitalOutputChannels, "digital");
            AssertDisjoint(caf.AnalogOutputChannels, rb.AnalogOutputChannels, "analog");
        }

        static void AssertDisjoint(Hashtable a, Hashtable b, string kind)
        {
            foreach (object name in a.Keys)
                Assert.True(!b.ContainsKey(name), kind + " output channel \"" + name + "\" exists in both MoleculeMOTHardware and RbTweezerHardware");
        }
    }
}
