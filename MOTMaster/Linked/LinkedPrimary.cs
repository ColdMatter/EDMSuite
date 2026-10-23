using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DAQ.HAL;

namespace MOTMaster
{
    /// <summary>
    /// The primary's side of a linked run, against an ILinkSecondary. No hardware and no Controller
    /// dependency, so it is unit-testable with a fake secondary.
    /// </summary>
    public sealed class LinkedPrimary
    {
        const int PollMs = 5;

        readonly ILinkSecondary secondary;
        readonly MOTMasterLinkConfig config;
        readonly Action<string> log;
        string token;

        public Dictionary<string, object> SaveInfo { get; private set; }

        /// <summary>
        /// True once the secondary has refused this link (it took back control, or the link was broken there).
        /// The link is dead from then on; the Controller turns linked mode off.
        /// </summary>
        public bool Lost { get; private set; }

        public LinkedPrimary(ILinkSecondary secondary, MOTMasterLinkConfig config, Action<string> log)
        {
            this.secondary = secondary;
            this.config = config;
            this.log = log ?? (s => { });
            SaveInfo = new Dictionary<string, object>();
        }

        public void Claim(string primaryName)
        {
            try
            {
                token = secondary.ClaimLink(primaryName);
            }
            catch (Exception e)
            {
                throw new LinkedRunException("Could not link to the secondary MOTMaster at " + config.SecondaryUrl
                    + ": " + e.Message, e);
            }
        }

        /// <summary>Best effort: a secondary that is down or already released must not stop the primary closing.</summary>
        public void Release()
        {
            string t = token;
            token = null;
            if (t == null) return;
            try { secondary.ReleaseLink(t); }
            catch (Exception e) { log("Releasing the link to " + config.SecondaryUrl + " failed (ignored): " + e.Message); }
        }

        public LinkReadyInfo Begin(LinkRunRequest request, string scriptHash, string parameterHash,
            int digitalClock, int analogClock, string provenance)
        {
            if (token == null)
                throw new LinkedRunException("Not linked to the secondary MOTMaster at " + config.SecondaryUrl + "; re-link first.");

            LinkReadyInfo info;
            try
            {
                info = secondary.PrepareLinkedRun(token, request);
            }
            catch (Exception e)
            {
                if (TokenRejected(e))
                {
                    token = null;
                    Lost = true;
                    throw new LinkLostException("The secondary MOTMaster at " + config.SecondaryUrl
                        + " has taken back control (or the link was broken there), so linked mode is now off. "
                        + "Tick Linked again to re-link. (" + e.Message + ")", e);
                }
                throw new LinkedRunException("The secondary MOTMaster at " + config.SecondaryUrl
                    + " failed to prepare the run: " + e.Message, e);
            }
            if (info == null)
                throw new LinkedRunException("The secondary MOTMaster at " + config.SecondaryUrl + " returned no ready information.");

            if (info.ScriptHash != scriptHash)
                throw new LinkedRunException("The secondary's script differs from the primary's (script hash "
                    + info.ScriptHash + " vs " + scriptHash + "); the run was not started.");
            if (info.ParameterHash != parameterHash)
                throw new LinkedRunException("The secondary's parameters differ from the primary's (parameter hash "
                    + info.ParameterHash + " vs " + parameterHash + "); the run was not started.");
            if (info.DigitalClockFrequency != digitalClock || info.AnalogClockFrequency != analogClock)
                throw new LinkedRunException("The primary and secondary clock frequencies differ (primary digital/analog "
                    + digitalClock + "/" + analogClock + " Hz, secondary " + info.DigitalClockFrequency + "/"
                    + info.AnalogClockFrequency + " Hz); the run was not started.");
            // The secondary's analog mapping (t + A - delay) adds digital ticks to analog ticks.
            if (digitalClock != analogClock || info.DigitalClockFrequency != info.AnalogClockFrequency)
                throw new LinkedRunException("The digital and analog clocks differ (primary digital/analog "
                    + digitalClock + "/" + analogClock + " Hz, secondary " + info.DigitalClockFrequency + "/"
                    + info.AnalogClockFrequency + " Hz); a linked run needs them equal on both machines, so it was not started.");

            string[] clamped = info.ClampedEvents ?? new string[0];
            foreach (string line in clamped) log(line);

            if (!BuildProvenance.SameSource(provenance, info.Provenance))
            {
                log("WARNING: primary and secondary run different source revisions.");
                log("  primary:   " + provenance);
                log("  secondary: " + info.Provenance);
            }

            var save = new Dictionary<string, object>
            {
                { "LinkRunId", request.LinkRunId },
                { "Role", "primary" },
                { "Partner", config.SecondaryUrl },
                { "ScriptHash", scriptHash },
                { "ParameterHash", parameterHash },
                { "PrimaryProvenance", provenance },
                { "SecondaryProvenance", info.Provenance },
                { "AnalogOrigin", request.AnalogOrigin },
                { "ClampedEvents", string.Join("; ", clamped) }
            };
            object delay;
            if (request.Parameters != null && request.Parameters.TryGetValue("SecondaryTriggerDelay", out delay))
                save["SecondaryTriggerDelay"] = delay;
            SaveInfo = save;
            return info;
        }

        /// <summary>
        /// Waits for the secondary to report shot number <paramref name="shot"/> armed. Returns false as soon as
        /// keepWaiting goes false (user stop). Throws if the secondary errors, stops, or does not arm in time.
        /// </summary>
        public bool WaitUntilArmed(int shot, Func<bool> keepWaiting)
        {
            var timer = Stopwatch.StartNew();
            while (true)
            {
                if (!keepWaiting()) return false;
                LinkShotStatus s = Poll();
                if (s.Error != null)
                    throw new LinkedRunException("The secondary MOTMaster reported an error: " + s.Error);
                if (s.ArmedShots >= shot) return true;
                if (!s.Running)
                    throw new LinkedRunException("The secondary stopped before it armed shot " + shot + " (secondary stopped).");
                if (timer.ElapsedMilliseconds >= config.ArmTimeoutMs)
                {
                    // It re-arms only after the previous shot has played, so the real fault is the missing trigger.
                    if (shot >= 2 && s.PlayedShots < shot - 1)
                        throw NeverSawTrigger(s.PlayedShots + 1, "it is still waiting for that trigger, so shot " + shot + " never armed");
                    throw new LinkedRunException("The secondary MOTMaster did not arm shot " + shot + " within "
                        + config.ArmTimeoutMs + " ms (it did not arm).");
                }
                Thread.Sleep(PollMs);
            }
        }

        /// <summary>
        /// Ends the run on the secondary. First, when the primary fired shots, waits up to ArmTimeoutMs for the
        /// secondary to report that many played, and throws "never saw the trigger" (without ending the run there;
        /// the caller aborts) if it does not: a secondary that sat armed through a shot must not count as a good
        /// run. Then tells it to finish and waits for it to stop. Throws on a secondary error or timeout.
        /// </summary>
        /// <param name="shotsFired">Shots whose primary pattern ran to completion (a shot cut short by Stop is not counted).</param>
        public void End(int shotsFired)
        {
            if (shotsFired > 0) WaitUntilPlayed(shotsFired);
            EndRun();
        }

        void WaitUntilPlayed(int shotsFired)
        {
            var timer = Stopwatch.StartNew();
            while (true)
            {
                LinkShotStatus s = Poll();
                if (s.Error != null)
                    throw new LinkedRunException("The secondary MOTMaster reported an error: " + s.Error);
                if (s.PlayedShots >= shotsFired) return;
                if (!s.Running)
                    throw NeverSawTrigger(s.PlayedShots + 1, "the secondary stopped");
                if (timer.ElapsedMilliseconds >= config.ArmTimeoutMs)
                    throw NeverSawTrigger(s.PlayedShots + 1, "no played pattern within " + config.ArmTimeoutMs + " ms of the primary's shot "
                        + shotsFired + " ending");
                Thread.Sleep(PollMs);
            }
        }

        LinkedRunException NeverSawTrigger(int shot, string detail)
        {
            return new LinkedRunException("The secondary MOTMaster at " + config.SecondaryUrl + " never saw the trigger for shot " + shot
                + " (" + detail + "). Check the trigger cable and line (" + config.TriggerChannel + ").");
        }

        /// <summary>Tells the secondary to finish and waits for it to stop. Throws on a secondary error or timeout.</summary>
        void EndRun()
        {
            try
            {
                secondary.EndLinkedRun(RequireToken());
            }
            catch (LinkedRunException) { throw; }
            catch (Exception e)
            {
                throw new LinkedRunException("Could not end the linked run on the secondary MOTMaster at "
                    + config.SecondaryUrl + ": " + e.Message, e);
            }

            var timer = Stopwatch.StartNew();
            while (true)
            {
                LinkShotStatus s = Poll();
                if (s.Error != null)
                    throw new LinkedRunException("The secondary MOTMaster reported an error: " + s.Error);
                if (!s.Running) return;
                if (timer.ElapsedMilliseconds >= config.EndTimeoutMs)
                    throw new LinkedRunException("The secondary MOTMaster did not stop within " + config.EndTimeoutMs + " ms.");
                Thread.Sleep(PollMs);
            }
        }

        /// <summary>Like End, but without the played-shots check, and never throws; failures are logged.</summary>
        public void Abort()
        {
            // The secondary no longer holds this link, so it has no run of ours to end.
            if (Lost) return;
            try { EndRun(); }
            catch (Exception e) { log("Aborting the linked run on the secondary failed: " + e.Message); }
        }

        /// <summary>
        /// LinkRunRequest.ContactTimeoutMs for a run: one pattern (rounded up to whole ms) + the arm timeout + 5 s,
        /// saturating at int.MaxValue. The longest the primary goes without polling the secondary mid-run is about
        /// one pattern (runPattern) plus the DDS arm wait, well inside this.
        /// </summary>
        public static int ContactTimeoutFor(int patternLengthTicks, int digitalClockHz, int armTimeoutMs)
        {
            long patternMs = digitalClockHz > 0
                ? ((long)Math.Max(0, patternLengthTicks) * 1000 + digitalClockHz - 1) / digitalClockHz
                : 0;
            long total = patternMs + Math.Max(0, armTimeoutMs) + 5000L;
            return total > int.MaxValue ? int.MaxValue : (int)total;
        }

        string RequireToken()
        {
            if (token == null)
                throw new LinkedRunException("Not linked to the secondary MOTMaster at " + config.SecondaryUrl + "; re-link first.");
            return token;
        }

        LinkShotStatus Poll()
        {
            try
            {
                LinkShotStatus s = secondary.GetLinkShotStatus(RequireToken());
                if (s == null) throw new LinkedRunException("The secondary MOTMaster returned no status.");
                return s;
            }
            catch (LinkedRunException) { throw; }
            catch (Exception e)
            {
                throw new LinkedRunException("Lost contact with the secondary MOTMaster at " + config.SecondaryUrl
                    + ": " + e.Message, e);
            }
        }

        // Over remoting the exception type can arrive changed, so also recognise the message text
        // produced by LinkedSecondaryState.Validate.
        static bool TokenRejected(Exception e)
        {
            string m = e.Message ?? "";
            return m.IndexOf("not linked", StringComparison.OrdinalIgnoreCase) >= 0
                || m.IndexOf("re-link", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
