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
                    throw new LinkedRunException("The secondary MOTMaster at " + config.SecondaryUrl
                        + " no longer holds this link (" + e.Message + "); re-link and run again.", e);
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
                    throw new LinkedRunException("The secondary MOTMaster did not arm shot " + shot + " within "
                        + config.ArmTimeoutMs + " ms (it did not arm).");
                Thread.Sleep(PollMs);
            }
        }

        /// <summary>Tells the secondary to finish and waits for it to stop. Throws on a secondary error or timeout.</summary>
        public void End()
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

        /// <summary>Like End, but never throws; failures are logged.</summary>
        public void Abort()
        {
            try { End(); }
            catch (Exception e) { log("Aborting the linked run on the secondary failed: " + e.Message); }
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
