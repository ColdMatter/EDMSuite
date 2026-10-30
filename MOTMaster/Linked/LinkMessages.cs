using System;
using System.Collections.Generic;

namespace MOTMaster
{
    /// <summary>Failure of a linked run or of the link itself. Always surfaces as an exception, never a MessageBox.</summary>
    [Serializable]
    public class LinkedRunException : ApplicationException
    {
        public LinkedRunException(string message) : base(message) { }
        public LinkedRunException(string message, Exception inner) : base(message, inner) { }
        protected LinkedRunException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>The secondary refused the primary's link (it took back control, or the link was broken there).</summary>
    [Serializable]
    public class LinkLostException : LinkedRunException
    {
        public LinkLostException(string message, Exception inner) : base(message, inner) { }
        protected LinkLostException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>Everything the secondary needs to reproduce the primary's run. Remoting message: simple types only.</summary>
    [Serializable]
    public class LinkRunRequest
    {
        public string LinkRunId;
        public string PrimaryName;
        public string PrimaryHardwareType;
        public string ScriptFileName;
        public string ScriptText;
        public Dictionary<string, object> Parameters;
        public int AnalogOrigin;
        public bool SaveEnable;
        public int BatchNumber;
        /// <summary>The primary's BuildProvenance.Describe(), recorded in the secondary's _link.txt. Null from an older primary.</summary>
        public string PrimaryProvenance;
        /// <summary>
        /// The secondary ends the armed run (with an error) after this long without a call from the primary for
        /// it, so a primary that died mid-run cannot leave it armed. 0 or less (and any older primary): no bound.
        /// </summary>
        public int ContactTimeoutMs;
    }

    /// <summary>The secondary's answer to PrepareLinkedRun: what it compiled and what it will run.</summary>
    [Serializable]
    public class LinkReadyInfo
    {
        public string ScriptHash;
        public string ParameterHash;
        public string Provenance;
        public int DigitalClockFrequency;
        public int AnalogClockFrequency;
        public string[] ClampedEvents;
        /// <summary>32 or 64: the secondary's process bitness. 0 from an older secondary (not checked).</summary>
        public int ProcessBits;
        /// <summary>The secondary's running MOTMaster.exe, so a refusal can say which build it is.</summary>
        public string ExePath;
    }

    [Serializable]
    public class LinkShotStatus
    {
        public int ArmedShots;
        /// <summary>Shots whose triggered pattern actually played to completion (not abandoned by a stop).</summary>
        public int PlayedShots;
        public bool Running;
        public string Error;
    }

    /// <summary>The primary's view of the secondary. Implemented by the secondary's Controller (over remoting) and by test fakes.</summary>
    public interface ILinkSecondary
    {
        string ClaimLink(string primaryName);
        void ReleaseLink(string token);
        LinkReadyInfo PrepareLinkedRun(string token, LinkRunRequest request);
        LinkShotStatus GetLinkShotStatus(string token);
        void EndLinkedRun(string token);
    }
}
