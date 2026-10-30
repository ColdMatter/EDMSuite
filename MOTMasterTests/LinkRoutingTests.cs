using System;
using System.Threading;
using DAQ.Pattern;
using static MOTMasterTests.TestRunner;

namespace MOTMasterTests
{
    public static class LinkRoutingTests
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

        static TestHardware Both()
        {
            return new TestHardware().Digital("pA", "/D9", 0, 5);
        }

        public static void TestOwnership()
        {
            var r = LinkRouting.ForPrimary(Local(), Other(), "trig", new[] { "dds" });
            Assert.True(r.IsLocalDigital("pA"), "pA local");
            Assert.True(!r.IsLocalDigital("tB"), "tB other");
            Assert.True(r.UsedOtherChannels && !r.UsedOtherAnalog, "flags after digital");
            Assert.True(!r.IsLocalAnalog("tC") && r.UsedOtherAnalog, "analog flag");
            StringAssert(Assert.Throws<LinkRoutingException>(() => r.IsLocalDigital("nope")).Message, "is not a channel on", "nope");
            StringAssert(Assert.Throws<LinkRoutingException>(() => r.IsLocalDigital("trig")).Message, "reserved for the secondary trigger");
            var rb = LinkRouting.ForPrimary(Local(), Both(), "trig", new string[0]);
            StringAssert(Assert.Throws<LinkRoutingException>(() => rb.IsLocalDigital("pA")).Message, "is defined on both", "pA");
        }

        public static void TestPrimaryMappingIsIdentity()
        {
            var r = LinkRouting.ForPrimary(Local(), Other(), "trig", new string[0]);
            Assert.Equal(-5, r.MapDigitalPoint("pA", -5));
            Assert.Equal(7, r.MapAnalogSpan("aA", 7));
            Assert.Equal(0, r.AnalogLengthExtension);
        }

        public static void TestSecondaryShift()
        {
            var r = LinkRouting.ForSecondary(Local(), Other(), 3, 4000);
            Assert.Equal(7, r.MapDigitalPoint("pA", 10));
            Assert.Equal(4007, r.MapAnalogPoint("aA", 10));
            Assert.Equal(3997, r.AnalogLengthExtension);
        }

        public static void TestSecondaryClampPoint()
        {
            var r = LinkRouting.ForSecondary(Local(), Other(), 3, 4000);
            Assert.Equal(0, r.MapDigitalPoint("pA", 1));
            Assert.Equal(1, r.ClampedEvents.Count);
            Assert.True(r.ClampedEvents[0].Contains("pA digital t=1 -> 0 (was -2)"), r.ClampedEvents[0]);
        }

        public static void TestSecondaryClampCollision()
        {
            var r = LinkRouting.ForSecondary(Local(), Other(), 3, 0);   // analog shifted = t - 3
            r.MapAnalogPoint("aA", 0);
            StringAssert(Assert.Throws<LinkRoutingException>(() => r.MapAnalogPoint("aA", 3)).Message, "clamped to the trigger");
        }

        public static void TestSecondaryClampCollidesWithSpanAtZero()
        {
            // analog: shifted = t - 3. A clamped value, then a pulse starting exactly on tick 0.
            var r = LinkRouting.ForSecondary(Local(), Other(), 3, 0);
            r.MapAnalogPoint("aA", 0);
            StringAssert(Assert.Throws<LinkRoutingException>(() => r.MapAnalogSpan("aA", 3)).Message, "clamped to the trigger");

            // The other order: the span first, then a value clamped onto it.
            var r2 = LinkRouting.ForSecondary(Local(), Other(), 3, 0);
            Assert.Equal(0, r2.MapAnalogSpan("aA", 3));
            StringAssert(Assert.Throws<LinkRoutingException>(() => r2.MapAnalogPoint("aA", 1)).Message, "clamped to the trigger");

            // Digital the same way.
            var r3 = LinkRouting.ForSecondary(Local(), Other(), 3, 0);
            Assert.Equal(0, r3.MapDigitalSpan("pA", 3));
            StringAssert(Assert.Throws<LinkRoutingException>(() => r3.MapDigitalPoint("pA", 0)).Message, "clamped to the trigger");

            // Unclamped events on tick 0 alongside a span there are fine, as are spans on other channels.
            var r4 = LinkRouting.ForSecondary(Local(), Other(), 3, 0);
            r4.MapAnalogSpan("aA", 3);
            r4.MapAnalogPoint("aA", 3);
            r4.MapAnalogSpan("aA", 3);
            r4.MapDigitalPoint("pA", 0);
            r4.MapAnalogSpan("tC", 3);
        }

        public static void TestSecondaryNoClampCollisionWhenUnclamped()
        {
            var r = LinkRouting.ForSecondary(Local(), Other(), 0, 0);
            r.MapAnalogPoint("aA", 0);
            r.MapAnalogPoint("aA", 0);
        }

        public static void TestSecondarySpanBeforeTrigger()
        {
            var r = LinkRouting.ForSecondary(Local(), Other(), 3, 4000);
            StringAssert(Assert.Throws<LinkRoutingException>(() => r.MapDigitalSpan("pA", 2)).Message,
                "starts before the secondary is triggered");
        }

        public static void TestAnalogOrigin()
        {
            var r = LinkRouting.ForPrimary(Local(), Other(), "trig", new[] { "dds", "pA" });
            Assert.Equal<int?>(null, r.AnalogOrigin());
            r.NoteDigitalRise("dds", 4000); r.NoteDigitalRise("dds", 9000); r.NoteDigitalRise("pA", 4000);
            Assert.Equal<int?>(4000, r.AnalogOrigin());
            r.NoteDigitalRise("pA", 3000);
            StringAssert(Assert.Throws<LinkRoutingException>(() => r.AnalogOrigin()).Message, "analog start is ambiguous");
        }

        public static void TestEnterIsThreadLocalAndNotNested()
        {
            var r = LinkRouting.ForPrimary(Local(), Other(), "trig", new string[0]);
            LinkRouting seenOnOtherThread = r;
            using (LinkRouting.Enter(r))
            {
                Assert.True(ReferenceEquals(r, LinkRouting.Current), "Current set");
                Assert.Throws<InvalidOperationException>(() => LinkRouting.Enter(r));
                var t = new Thread(() => { seenOnOtherThread = LinkRouting.Current; });
                t.Start();
                t.Join();
                Assert.True(seenOnOtherThread == null, "other thread sees null");
            }
            Assert.True(LinkRouting.Current == null, "cleared after Dispose");
        }
    }
}
