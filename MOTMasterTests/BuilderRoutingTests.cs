using System;
using System.Linq;
using DAQ.Analog;
using DAQ.Pattern;
using static MOTMasterTests.TestRunner;

namespace MOTMasterTests
{
    public static class BuilderRoutingTests
    {
        static TestHardware Local()
        {
            return new TestHardware().Digital("pA", "/D1", 0, 0).Digital("trig", "/D1", 0, 1)
                .Digital("dds", "/D1", 0, 2).Analog("aA", "/A1/ao0");
        }

        static TestHardware Other()
        {
            return new TestHardware().Digital("tB", "/D9", 0, 0).Analog("tC", "/A9/ao0");
        }

        public static void TestPrimarySkipsOtherChannels()
        {
            var routing = LinkRouting.ForPrimary(Local(), Other(), "trig", new[] { "dds" });
            using (LinkRouting.Enter(routing))
            {
                var p = new PatternBuilder32();
                p.AddEdge("tB", 5, true);
                int ret = p.Pulse(0, 10, 5, "tB");
                int downRet = p.DownPulse(0, 10, 5, "tB");
                p.Pulse(0, 10, 5, "pA");
                var a = new AnalogPatternBuilder(100);
                a.AddChannel("tC");
                a.AddAnalogValue("tC", 3, 1.0);
                a.AddAnalogPulse("tC", 3, 4, 1.0, 0.0);
                a.AddLinearRamp("tC", 3, 4, 1.0);
                a.SwitchOffAtEndOfPattern("tC");
                Assert.Equal(15, ret, "Pulse return on other channel");
                Assert.Equal(15, downRet, "DownPulse return on other channel");
                Assert.Equal(1, p.Boards.Count, "digital board count");
                Assert.True(p.Boards.ContainsKey("/D1"), "only /D1");
                Assert.Equal(0, a.Boards.Count, "analog board count");
            }
        }

        public static void TestSecondaryShiftsDigitalAndAnalog()
        {
            var routing = LinkRouting.ForSecondary(Local(), Other(), 3, 40);
            using (LinkRouting.Enter(routing))
            {
                var p = new PatternBuilder32();
                p.Pulse(10, 0, 5, "pA");
                p.BuildPattern(50);
                uint[] pat = p.Boards["/D1"].Pattern;
                Assert.Equal(0u, pat[6] & 1u, "tick 6");
                for (int t = 7; t <= 11; t++) Assert.Equal(1u, pat[t] & 1u, "tick " + t);
                Assert.Equal(0u, pat[12] & 1u, "tick 12");

                var a = new AnalogPatternBuilder(20);
                Assert.Equal(57, a.PatternLength, "extended length");
                a.AddChannel("aA");
                a.AddAnalogValue("aA", 0, 2.0);
                a.AddLinearRamp("aA", 5, 4, 6.0);
                var d = a.Boards["/A1"].AnalogPatterns["aA"];
                Assert.True(d.ContainsKey(37) && d[37] == 2.0, "value at 37");
                for (int k = 42; k <= 45; k++) Assert.True(d.ContainsKey(k), "ramp key " + k);
                Assert.True(!d.ContainsKey(41) && !d.ContainsKey(46), "no extra ramp keys");
            }
        }

        public static void TestRiseNotedForAnalogOrigin()
        {
            var routing = LinkRouting.ForPrimary(Local(), Other(), "trig", new[] { "dds" });
            using (LinkRouting.Enter(routing))
            {
                var p = new PatternBuilder32();
                p.Pulse(4000, 0, 10, "dds");
                Assert.Equal<int?>(4000, routing.AnalogOrigin());
            }
        }

        public static void TestGetValueOnOtherSystemThrows()
        {
            var routing = LinkRouting.ForPrimary(Local(), Other(), "trig", new string[0]);
            using (LinkRouting.Enter(routing))
            {
                var a = new AnalogPatternBuilder(100);
                var e = Assert.Throws<LinkRoutingException>(() => a.GetValue("tC", 0));
                StringAssert(e.Message, "GetValue", "tC");
            }
        }

        public static void TestLegacyIntCallsRefusedWhenLinked()
        {
            var routing = LinkRouting.ForPrimary(Local(), Other(), "trig", new string[0]);
            using (LinkRouting.Enter(routing))
            {
                var p = new PatternBuilder32();
                Assert.Throws<LinkRoutingException>(() => p.AddEdge(0, 5, true));
                Assert.Throws<LinkRoutingException>(() => p.Pulse(0, 1, 2, 0));
                Assert.Throws<LinkRoutingException>(() => p.DownPulse(0, 1, 2, 0));
                Assert.Throws<LinkRoutingException>(() => p.Clear());
            }
        }

        public static void TestNoContextUnknownNameStillNullReference()
        {
            Assert.True(LinkRouting.Current == null, "no context");
            Assert.Throws<NullReferenceException>(() => new PatternBuilder32().AddEdge("definitelyNotAChannel", 1, true));
        }
    }
}
