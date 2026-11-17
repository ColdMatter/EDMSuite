using System.Runtime.Remoting;

namespace MOTMaster
{
    /// <summary>
    /// By default .NET Remoting replaces every exception it sends to a caller on another machine with "Server
    /// encountered an internal error", so a primary would never see why a linked call failed (or that the secondary
    /// took back control). A MOTMaster turns that off the first time it is asked to be a secondary; MOTMasters that
    /// are never linked keep the default.
    /// </summary>
    public static class LinkRemoting
    {
        static readonly object gate = new object();
        static bool done;

        public static void ShowErrorsToRemoteCallers()
        {
            lock (gate)
            {
                if (done) return;
                // The setting can be made only once per process; if something else already made it, leave it be.
                try { RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off; }
                catch (RemotingException) { }
                done = true;
            }
        }
    }
}
