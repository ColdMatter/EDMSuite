using System;

namespace MOTMaster
{
    /// <summary>
    /// The secondary's claim and shot state, with no hardware in it. One lock guards everything; the
    /// Changed event is raised outside the lock after every state change, for the GUI.
    /// </summary>
    public sealed class LinkedSecondaryState
    {
        readonly object gate = new object();
        string token;
        string claimedBy;
        int armedShots;
        bool running;
        string error;

        public event Action Changed;

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
                        + "; it can only be claimed again after that link is released (or broken here with Break link).");
                if (runInProgress)
                    throw new LinkedRunException("This MOTMaster is running; it cannot be linked until the run has finished.");
                newToken = Guid.NewGuid().ToString();
                token = newToken;
                claimedBy = primaryName;
                armedShots = 0;
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

        /// <summary>Clears any claim regardless of token (the "Break link" button on the secondary).</summary>
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
                running = true;
                error = null;
            }
            RaiseChanged();
        }

        public void MarkArmed()
        {
            lock (gate) { armedShots++; }
            RaiseChanged();
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
                return new LinkShotStatus { ArmedShots = armedShots, Running = running, Error = error };
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
