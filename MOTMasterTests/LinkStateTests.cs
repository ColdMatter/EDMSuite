using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using DAQ.HAL;
using MOTMaster;
using static MOTMasterTests.TestRunner;

namespace MOTMasterTests
{
    /// <summary>Scriptable stand-in for the remote secondary.</summary>
    public class FakeSecondary : ILinkSecondary
    {
        public string Token = "tok";
        public LinkReadyInfo Ready = new LinkReadyInfo
        {
            ScriptHash = "S",
            ParameterHash = "P",
            Provenance = "git abc | MOTMaster.exe 1",
            DigitalClockFrequency = 100000,
            AnalogClockFrequency = 100000,
            ClampedEvents = new string[0]
        };
        public Exception ClaimException, PrepareException, StatusException, EndException, ReleaseException;
        public Queue<LinkShotStatus> Statuses = new Queue<LinkShotStatus>();
        public LinkShotStatus LastStatus = new LinkShotStatus { ArmedShots = 0, Running = true };
        public LinkShotStatus AfterEnd = new LinkShotStatus { ArmedShots = 0, Running = false };
        public int ReleaseCalls, EndCalls;
        public LinkRunRequest LastRequest;
        bool ended;

        public string ClaimLink(string primaryName)
        {
            if (ClaimException != null) throw ClaimException;
            return Token;
        }
        public void ReleaseLink(string token)
        {
            ReleaseCalls++;
            if (ReleaseException != null) throw ReleaseException;
        }
        public LinkReadyInfo PrepareLinkedRun(string token, LinkRunRequest request)
        {
            if (PrepareException != null) throw PrepareException;
            LastRequest = request;
            return Ready;
        }
        public LinkShotStatus GetLinkShotStatus(string token)
        {
            if (StatusException != null) throw StatusException;
            if (ended) return AfterEnd;
            if (Statuses.Count > 0) LastStatus = Statuses.Dequeue();
            return LastStatus;
        }
        public void EndLinkedRun(string token)
        {
            EndCalls++;
            if (EndException != null) throw EndException;
            ended = true;
        }
    }

    public static class LinkStateTests
    {
        static MOTMasterLinkConfig Config(int armMs = 5000, int endMs = 5000)
        {
            return new MOTMasterLinkConfig
            {
                SecondaryUrl = "tcp://10.0.0.2:1187/controller.rem",
                SecondaryHardwareType = "DAQ.HAL.RbTweezerHardware",
                TriggerChannel = "trig",
                AnalogStartChannels = new[] { "ana" },
                ArmTimeoutMs = armMs,
                EndTimeoutMs = endMs
            };
        }

        static LinkRunRequest Request()
        {
            return new LinkRunRequest
            {
                LinkRunId = "id1",
                PrimaryName = "PH",
                ScriptFileName = "s.cs",
                ScriptText = "text",
                Parameters = new Dictionary<string, object> { { "SecondaryTriggerDelay", 7 } },
                AnalogOrigin = 1234
            };
        }

        static LinkedPrimary Claimed(FakeSecondary f, List<string> log, int armMs = 5000, int endMs = 5000)
        {
            var p = new LinkedPrimary(f, Config(armMs, endMs), s => log.Add(s));
            p.Claim("PH");
            return p;
        }

        static LinkReadyInfo Begin(LinkedPrimary p, string sh = "S", string ph = "P", int dc = 100000, int ac = 100000, string prov = "git abc | MOTMaster.exe 2")
        {
            return p.Begin(Request(), sh, ph, dc, ac, prov);
        }

        public static void TestFingerprintOrderIndependentAndTypeSensitive()
        {
            var ab = new Dictionary<string, object> { { "a", 1 }, { "b", 2.0 } };
            var ba = new Dictionary<string, object> { { "b", 2.0 }, { "a", 1 } };
            var along = new Dictionary<string, object> { { "a", 1L }, { "b", 2.0 } };
            var close = new Dictionary<string, object> { { "a", 1 }, { "b", 2.0000000001 } };
            Assert.Equal(LinkFingerprint.OfParameters(ab), LinkFingerprint.OfParameters(ba), "order");
            Assert.True(LinkFingerprint.OfParameters(ab) != LinkFingerprint.OfParameters(along), "type sensitive");
            Assert.True(LinkFingerprint.OfParameters(ab) != LinkFingerprint.OfParameters(close), "precision sensitive");
            var withNull = new Dictionary<string, object> { { "a", null } };
            Assert.Equal(64, LinkFingerprint.OfParameters(withNull).Length, "null ok");
        }

        public static void TestFingerprintText()
        {
            Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", LinkFingerprint.OfText("abc"));
        }

        public static void TestProvenanceSameSource()
        {
            Assert.True(BuildProvenance.SameSource("git x | MOTMaster.exe 1", "git x | MOTMaster.exe 2"), "same");
            Assert.True(!BuildProvenance.SameSource("git x", "git y"), "different");
            Assert.True(BuildProvenance.Describe().StartsWith("git "), "describe starts with git");
            Assert.True(BuildProvenance.Describe().Contains(" | MOTMaster.exe "), "describe has exe");
        }

        public static void TestSecondaryClaimRules()
        {
            var s = new LinkedSecondaryState();
            int changed = 0;
            s.Changed += () => changed++;
            string token = s.Claim("P", false);
            Assert.True(!string.IsNullOrEmpty(token), "token");
            Assert.True(s.IsClaimed, "claimed");
            Assert.Equal("P", s.ClaimedBy);
            Assert.True(changed > 0, "Changed raised");
            StringAssert(Assert.Throws<LinkedRunException>(() => s.Claim("Q", false)).Message, "linked to P");
            Assert.Equal("This MOTMaster is linked to P; Go is refused until the link is released.",
                Assert.Throws<LinkedRunException>(() => s.ThrowIfClaimed("Go")).Message);
            StringAssert(Assert.Throws<LinkedRunException>(() => s.Validate("wrong")).Message, "re-link");
            s.Validate(token);
            s.Release("wrong");
            Assert.True(s.IsClaimed, "wrong-token release is a no-op");
            s.Release(token);
            Assert.True(!s.IsClaimed, "released");
            s.ThrowIfClaimed("Go");

            var s2 = new LinkedSecondaryState();
            Assert.Throws<LinkedRunException>(() => s2.Claim("P", true));
            Assert.True(!s2.IsClaimed, "refused claim leaves no claim");
            s2.Claim("P", false);
            s2.Break();
            Assert.True(!s2.IsClaimed, "Break clears");
            StringAssert(Assert.Throws<LinkedRunException>(() => s2.Validate("x")).Message, "not linked");
        }

        public static void TestSecondaryShotStatus()
        {
            var s = new LinkedSecondaryState();
            string t = s.Claim("P", false);
            s.RunStarted();
            s.MarkArmed();
            s.MarkArmed();
            var st = s.Status(t);
            Assert.Equal(2, st.ArmedShots);
            Assert.True(st.Running, "running");
            Assert.True(st.Error == null, "no error");
            s.Fail("bad");
            s.RunEnded();
            st = s.Status(t);
            Assert.Equal("bad", st.Error);
            Assert.True(!st.Running, "ended");
            s.RunStarted();
            st = s.Status(t);
            Assert.Equal(0, st.ArmedShots);
            Assert.True(st.Error == null, "error cleared by RunStarted");
            Assert.Throws<LinkedRunException>(() => s.Status("wrong"));
        }

        public static void TestPrimaryBeginMismatches()
        {
            var f = new FakeSecondary();
            var log = new List<string>();
            var p = Claimed(f, log);
            StringAssert(Assert.Throws<LinkedRunException>(() => Begin(p, sh: "other")).Message, "script differs");
            StringAssert(Assert.Throws<LinkedRunException>(() => Begin(p, ph: "other")).Message, "parameters differ");
            StringAssert(Assert.Throws<LinkedRunException>(() => Begin(p, dc: 50000)).Message, "clock frequencies differ");
            StringAssert(Assert.Throws<LinkedRunException>(() => Begin(p, ac: 50000)).Message, "clock frequencies differ");
            Begin(p);
        }

        public static void TestPrimaryBeginWarnsOnProvenance()
        {
            var f = new FakeSecondary();
            var log = new List<string>();
            var p = Claimed(f, log);
            Begin(p, prov: "git different | MOTMaster.exe 2");
            Assert.True(log.Exists(l => l.Contains("different source revisions")), "warned");
            Assert.True(log.Exists(l => l.Contains("git different")) && log.Exists(l => l.Contains("git abc")), "both provenances logged");

            var log2 = new List<string>();
            var p2 = Claimed(new FakeSecondary(), log2);
            Begin(p2, prov: "git abc | MOTMaster.exe 99");
            Assert.True(!log2.Exists(l => l.Contains("different source revisions")), "no warning when same");
        }

        public static void TestPrimaryBeginLogsClampedAndFillsSaveInfo()
        {
            var f = new FakeSecondary();
            f.Ready.ClampedEvents = new[] { "tB at 0 moved to 0", "tC at -3 moved to 0" };
            var log = new List<string>();
            var p = Claimed(f, log);
            Begin(p);
            Assert.True(log.Exists(l => l.Contains("tB at 0 moved to 0")), "clamp 1 logged");
            Assert.True(log.Exists(l => l.Contains("tC at -3 moved to 0")), "clamp 2 logged");
            var si = p.SaveInfo;
            Assert.Equal("id1", (string)si["LinkRunId"]);
            Assert.Equal("primary", (string)si["Role"]);
            Assert.Equal("tcp://10.0.0.2:1187/controller.rem", (string)si["Partner"]);
            Assert.Equal("S", (string)si["ScriptHash"]);
            Assert.Equal("P", (string)si["ParameterHash"]);
            Assert.Equal("git abc | MOTMaster.exe 2", (string)si["PrimaryProvenance"]);
            Assert.Equal("git abc | MOTMaster.exe 1", (string)si["SecondaryProvenance"]);
            Assert.Equal(1234, (int)si["AnalogOrigin"]);
            Assert.Equal(7, (int)si["SecondaryTriggerDelay"]);
            Assert.Equal("tB at 0 moved to 0; tC at -3 moved to 0", (string)si["ClampedEvents"]);
        }

        public static void TestPrimaryWaitArmed()
        {
            var f = new FakeSecondary();
            var p = Claimed(f, new List<string>());
            f.Statuses.Enqueue(new LinkShotStatus { ArmedShots = 0, Running = true });
            f.Statuses.Enqueue(new LinkShotStatus { ArmedShots = 0, Running = true });
            f.Statuses.Enqueue(new LinkShotStatus { ArmedShots = 1, Running = true });
            Assert.True(p.WaitUntilArmed(1, () => true), "armed");
        }

        public static void TestPrimaryWaitErrorAndTimeout()
        {
            var f = new FakeSecondary();
            var p = Claimed(f, new List<string>());
            f.LastStatus = new LinkShotStatus { ArmedShots = 0, Running = true, Error = "boom" };
            StringAssert(Assert.Throws<LinkedRunException>(() => p.WaitUntilArmed(1, () => true)).Message, "boom");

            var f2 = new FakeSecondary();
            var p2 = Claimed(f2, new List<string>(), armMs: 50);
            StringAssert(Assert.Throws<LinkedRunException>(() => p2.WaitUntilArmed(1, () => true)).Message, "did not arm");

            var f3 = new FakeSecondary();
            var p3 = Claimed(f3, new List<string>());
            f3.LastStatus = new LinkShotStatus { ArmedShots = 0, Running = false };
            StringAssert(Assert.Throws<LinkedRunException>(() => p3.WaitUntilArmed(1, () => true)).Message, "secondary stopped");
        }

        public static void TestPrimaryWaitStopsOnUserStop()
        {
            var f = new FakeSecondary();
            var p = Claimed(f, new List<string>());
            var sw = Stopwatch.StartNew();
            bool result = p.WaitUntilArmed(1, () => sw.ElapsedMilliseconds < 20);
            Assert.True(!result, "returns false");
            Assert.True(sw.ElapsedMilliseconds < 100, "returned promptly, took " + sw.ElapsedMilliseconds);
        }

        public static void TestPrimaryUnreachable()
        {
            var f = new FakeSecondary { ClaimException = new SocketException() };
            var p = new LinkedPrimary(f, Config(), s => { });
            var e = Assert.Throws<LinkedRunException>(() => p.Claim("PH"));
            StringAssert(e.Message, "tcp://10.0.0.2:1187/controller.rem");
        }

        public static void TestPrimaryClaimLost()
        {
            var f = new FakeSecondary { PrepareException = new LinkedRunException("This MOTMaster is not linked to you; re-link.") };
            var p = Claimed(f, new List<string>());
            StringAssert(Assert.Throws<LinkedRunException>(() => Begin(p)).Message, "re-link");

            // Over remoting the type may be lost; the message text must still be recognised.
            var f2 = new FakeSecondary { PrepareException = new InvalidOperationException("not linked: re-link required") };
            var p2 = Claimed(f2, new List<string>());
            StringAssert(Assert.Throws<LinkedRunException>(() => Begin(p2)).Message, "re-link");

            var f3 = new FakeSecondary { PrepareException = new InvalidOperationException("compile failed") };
            var p3 = Claimed(f3, new List<string>());
            StringAssert(Assert.Throws<LinkedRunException>(() => Begin(p3)).Message, "compile failed", "tcp://10.0.0.2:1187/controller.rem");
        }

        public static void TestPrimaryEnd()
        {
            var f = new FakeSecondary();
            var p = Claimed(f, new List<string>());
            p.End();
            Assert.Equal(1, f.EndCalls);

            var f2 = new FakeSecondary();
            f2.AfterEnd = new LinkShotStatus { Running = false, Error = "late failure" };
            var p2 = Claimed(f2, new List<string>());
            StringAssert(Assert.Throws<LinkedRunException>(() => p2.End()).Message, "late failure");

            var f3 = new FakeSecondary();
            f3.AfterEnd = new LinkShotStatus { Running = true };
            var p3 = Claimed(f3, new List<string>(), endMs: 50);
            StringAssert(Assert.Throws<LinkedRunException>(() => p3.End()).Message, "did not stop");
        }

        public static void TestPrimaryAbortNeverThrows()
        {
            var f = new FakeSecondary { EndException = new SocketException() };
            var log = new List<string>();
            var p = Claimed(f, log);
            p.Abort();
            Assert.Equal(1, f.EndCalls);
            Assert.True(log.Count > 0, "End failure logged");

            // EndLinkedRun succeeds but polling the status fails: Abort must still not throw.
            var f2 = new FakeSecondary { StatusException = new SocketException() };
            var log2 = new List<string>();
            var p2 = Claimed(f2, log2);
            p2.Abort();
            Assert.Equal(1, f2.EndCalls);
            Assert.True(log2.Count > 0, "status failure logged");
        }

        public static void TestPrimaryReleaseSwallowsErrors()
        {
            var f = new FakeSecondary { ReleaseException = new SocketException() };
            var log = new List<string>();
            var p = Claimed(f, log);
            p.Release();
            Assert.Equal(1, f.ReleaseCalls);
            Assert.True(log.Count > 0, "release failure logged");
            p.Release();
            Assert.Equal(1, f.ReleaseCalls, "token cleared, second Release does not call the secondary");
        }
    }
}
