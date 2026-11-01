using System;
using System.Diagnostics;

namespace MOTMaster
{
    /// <summary>
    /// The secondary's claim and shot state, with no hardware in it. One lock guards everything; the
    /// Changed event is raised outside the lock after every state change, for the GUI.
    /// </summary>
    public sealed class LinkedSecondaryState
    {
        readonly object gate = new object();
        readonly Func<long> nowMs;
        string token;
        string claimedBy;
        int armedShots;
        int playedShots;
        bool running;
        string error;
        // When the primary last made a token-validated call (nowMs() clock), for the contact timeout.
        long lastContactMs;

        public event Action Changed;

        public LinkedSecondaryState() : this(null) { }

        /// <param name="nowMs">A millisecond clock; null uses Stopwatch. Tests pass a fake one.</param>
        public LinkedSecondaryState(Func<long> nowMs)
        {
            if (nowMs == null)
            {
                Stopwatch clock = Stopwatch.StartNew();
                nowMs = () => clock.ElapsedMilliseconds;
            }
            this.nowMs = nowMs;
            lastContactMs = this.nowMs();
        }

        public bool IsClaimed
        {
            get { lock (gate) { return token != null; } }
        }

        public string ClaimedBy
        {
            get { lock (gate) { return claimedBy; } }
        }

        /// <summary>Takes the claim and returns its token. Refused if already claimed or if a local run is in progress.</summary>
        public string Claim(string primaryName, bool runInProgress)
        {
            string newToken;
            lock (gate)
            {
                if (token != null)
                    throw new LinkedRunException("This MOTMaster is already linked to " + claimedBy
                        + "; it can only be claimed again after that link is released (or taken back here with Take back control).");
                if (runInProgress)
                    throw new LinkedRunException("This MOTMaster is running; it cannot be linked until the run has finished.");
                newToken = Guid.NewGuid().ToString();
                token = newToken;
                claimedBy = primaryName;
                armedShots = 0;
                playedShots = 0;
                running = false;
                error = null;
            }
            RaiseChanged();
            return newToken;
        }

        /// <summary>Releases the claim. A wrong or stale token does nothing.</summary>
        public void Release(string t)
        {
            lock (gate)
            {
                if (token == null || t != token) return;
                ClearClaim();
            }
            RaiseChanged();
        }

        /// <summary>Clears any claim regardless of token (the "Take back control" button on the secondary).</summary>
        public void Break()
        {
            lock (gate)
            {
                ClearClaim();
            }
            RaiseChanged();
        }

        void ClearClaim()
        {
            token = null;
            claimedBy = null;
            armedShots = 0;
            playedShots = 0;
            running = false;
            error = null;
        }

        public void Validate(string t)
        {
            lock (gate)
            {
                if (token == null)
                    throw new LinkedRunException("This MOTMaster is not linked (no claim is held); re-link from the primary.");
                if (t != token)
                    throw new LinkedRunException("This MOTMaster is not linked to you (the claim belongs to " + claimedBy
                        + " or was broken); re-link from the primary.");
            }
        }

        /// <summary>Throws if linked, for the entry points that must refuse local control while claimed.</summary>
        public void ThrowIfClaimed(string action)
        {
            lock (gate)
            {
                if (token != null)
                    throw new LinkedRunException("This MOTMaster is linked to " + claimedBy + "; " + action + " is refused until the link is released.");
            }
        }

        public void RunStarted()
        {
            lock (gate)
            {
                armedShots = 0;
                playedShots = 0;
                running = true;
                error = null;
                lastContactMs = nowMs();
            }
            RaiseChanged();
        }

        public void MarkArmed()
        {
            lock (gate) { armedShots++; }
            RaiseChanged();
        }

        /// <summary>The armed pattern ran to completion, i.e. the trigger arrived (not abandoned by a stop).</summary>
        public void MarkPlayed()
        {
            lock (gate) { playedShots++; }
            RaiseChanged();
        }

        /// <summary>Records a token-validated call from the primary (Status does this itself).</summary>
        public void NoteContact()
        {
            lock (gate) { lastContactMs = nowMs(); }
        }

        public long MillisecondsSinceContact()
        {
            lock (gate) { return nowMs() - lastContactMs; }
        }

        /// <summary>True when the primary has not been in touch for longer than timeoutMs. timeoutMs &lt;= 0: never.</summary>
        public bool ContactLost(int timeoutMs)
        {
            return timeoutMs > 0 && MillisecondsSinceContact() > timeoutMs;
        }

        public void Fail(string message)
        {
            lock (gate) { error = message; }
            RaiseChanged();
        }

        public void RunEnded()
        {
            lock (gate) { running = false; }
            RaiseChanged();
        }

        public LinkShotStatus Status(string t)
        {
            Validate(t);
            lock (gate)
            {
                lastContactMs = nowMs();
                return new LinkShotStatus { ArmedShots = armedShots, PlayedShots = playedShots, Running = running, Error = error };
            }
        }

        void RaiseChanged()
        {
            Action handler = Changed;
            if (handler == null) return;
            try { handler(); }
            catch { /* a GUI handler must never break the state machine */ }
        }
    }
}
