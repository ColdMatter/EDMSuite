#if DDS
using System;
using System.Collections.Generic;
using MOTMaster;
using SpectrumDDS;

namespace MOTMasterTests
{
    public static class DDSWaitForTriggerTests
    {
        private static DDSCapabilities Capabilities()
        {
            return new DDSCapabilities
            {
                QueueCommandMaximum = 4098,
                FrequencyMinimum = 0.0,
                FrequencyMaximum = 625e6,
                FrequencySlopeMinimum = -1.9e14,
                FrequencySlopeMaximum = 1.9e14,
                AmplitudeSlopeMinimum = -3e5,
                AmplitudeSlopeMaximum = 3e5,
            };
        }

        private static readonly double[] Clamps = { 1.0, 1.0, 1.0, 1.0 };

        private static DDSPatternBuilder ThreeEvents()
        {
            var b = new DDSPatternBuilder();
            b.AddEvent("start", 0, new double[4], new double[4]);
            b.AddEvent("hold", 100, new double[4], new double[4]);
            b.AddEvent("resume", 200, new double[4], new double[4]);
            return b;
        }

        public static void TestBuilderMarksOnlyTheNamedEvent()
        {
            var b = ThreeEvents();
            b.WaitForTrigger("resume");

            DDSPattern p = DDSPattern.FromLegacyDictionary(b.Pattern);

            Assert.Equal(false, p[0].WaitForTrigger, "start");
            Assert.Equal(false, p[1].WaitForTrigger, "hold");
            Assert.Equal(true, p[2].WaitForTrigger, "resume");
        }

        public static void TestBuilderRejectsUnknownEvent()
        {
            var b = ThreeEvents();
            Assert.Throws<ArgumentException>(() => b.WaitForTrigger("nope"));
        }

        public static void TestLegacyRoundTripKeepsFiveRowsUnlessWaiting()
        {
            var b = ThreeEvents();
            b.WaitForTrigger("resume");

            Dictionary<string, List<List<double>>> back =
                DDSPattern.FromLegacyDictionary(b.Pattern).ToLegacyDictionary();

            Assert.Equal(5, back["hold"].Count, "an ordinary event keeps the old five rows");
            Assert.Equal(6, back["resume"].Count, "a waiting event carries the sixth row");
            Assert.Equal(true, DDSPattern.FromLegacyDictionary(back)[2].WaitForTrigger, "round trip");
        }

        public static void TestMalformedWaitRowIsRejected()
        {
            var b = ThreeEvents();
            b.Pattern["resume"].Add(new List<double> { 1.0, 1.0 });
            Assert.Throws<ArgumentException>(() => DDSPattern.FromLegacyDictionary(b.Pattern));
        }

        public static void TestWaitingEventDropsItsTimerFromTheCommandCount()
        {
            const int perBlock = 4 * DDSPattern.ChannelCount + 2;

            var b = ThreeEvents();
            Assert.Equal(3 * perBlock + 2, DDSPattern.FromLegacyDictionary(b.Pattern).CommandCount(), "all timed");

            b.WaitForTrigger("resume");
            Assert.Equal(3 * perBlock + 1, DDSPattern.FromLegacyDictionary(b.Pattern).CommandCount(), "one waits");
        }

        public static void TestTimerLimitsDoNotApplyToTheGapBeforeAWait()
        {
            const int beyondTimerMaximum = 6000000; // 60 s in ticks
            var b = new DDSPatternBuilder();
            b.AddEvent("start", 0, new double[4], new double[4]);
            b.AddEvent("resume", beyondTimerMaximum, new double[4], new double[4]);

            Assert.True(DDSPattern.FromLegacyDictionary(b.Pattern).Validate(Capabilities(), Clamps).Count > 0,
                "a 60 s timer gap is rejected");

            b.WaitForTrigger("resume");
            List<string> problems = DDSPattern.FromLegacyDictionary(b.Pattern).Validate(Capabilities(), Clamps);
            Assert.Equal(0, problems.Count, string.Join("; ", problems));
        }

        public static void TestWaitingEventMustStillComeAfterThePreviousOne()
        {
            var b = new DDSPatternBuilder();
            b.AddEvent("start", 100, new double[4], new double[4]);
            b.AddEvent("resume", 100, new double[4], new double[4]);
            b.WaitForTrigger("resume");

            Assert.True(DDSPattern.FromLegacyDictionary(b.Pattern).Validate(Capabilities(), Clamps).Count > 0,
                "two events at the same time are still ambiguous");
        }
    }
}
#endif
