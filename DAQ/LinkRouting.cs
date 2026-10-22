using System;
using System.Collections.Generic;
using System.Linq;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.Pattern
{
    public enum LinkSide { Primary, Secondary }

    public class LinkRoutingException : ApplicationException
    {
        public LinkRoutingException(string message) : base(message) { }
    }

    /// <summary>
    /// Routing context for a linked MOTMaster run. The Controller sets one around GetSequence() so that
    /// the pattern builders emit only the channels owned by this machine and apply this machine's time
    /// mapping. When no context is set the builders behave exactly as they always have.
    /// </summary>
    public sealed class LinkRouting
    {
        [ThreadStatic]
        private static LinkRouting current;

        public static LinkRouting Current
        {
            get { return current; }
        }

        public static IDisposable Enter(LinkRouting routing)
        {
            if (current != null)
                throw new InvalidOperationException("A link routing context is already active on this thread");
            current = routing;
            return new Scope(routing);
        }

        private sealed class Scope : IDisposable
        {
            private readonly LinkRouting routing;
            public Scope(LinkRouting routing) { this.routing = routing; }
            public void Dispose()
            {
                if (current == routing) current = null;
            }
        }

        public static DigitalOutputChannel LookupDigital(string name)
        {
            Hardware hardware = current != null ? current.Local : Environs.Hardware;
            return (DigitalOutputChannel)hardware.DigitalOutputChannels[name];
        }

        public static AnalogOutputChannel LookupAnalog(string name)
        {
            Hardware hardware = current != null ? current.Local : Environs.Hardware;
            return (AnalogOutputChannel)hardware.AnalogOutputChannels[name];
        }

        /// <summary>Throws if a routing context is active; for builder features that don't support linked runs.</summary>
        public static void ThrowIfActive(string what)
        {
            if (current != null)
                throw new LinkRoutingException(what + " is not supported in a linked run");
        }

        public static LinkRouting ForPrimary(Hardware local, Hardware other, string triggerChannel, IEnumerable<string> analogStartChannels)
        {
            return new LinkRouting(LinkSide.Primary, local, other, triggerChannel,
                analogStartChannels == null ? new string[0] : analogStartChannels.ToArray(), 0, 0);
        }

        public static LinkRouting ForSecondary(Hardware local, Hardware other, int triggerDelay, int analogOrigin)
        {
            return new LinkRouting(LinkSide.Secondary, local, other, null, new string[0], triggerDelay, analogOrigin);
        }

        private readonly Hardware other;
        private readonly string triggerChannel;
        private readonly HashSet<string> analogStartChannels;
        private readonly int triggerDelay;
        private readonly int analogOrigin;
        private readonly Dictionary<string, int> earliestRise = new Dictionary<string, int>();
        private readonly List<string> clampedEvents = new List<string>();
        // Per channel and kind: whether an event has landed on tick 0, and whether any of them was clamped there.
        private readonly Dictionary<string, TickZero> tickZero = new Dictionary<string, TickZero>();

        private class TickZero
        {
            public bool Seen;
            public bool AnyClamped;
        }

        private LinkRouting(LinkSide side, Hardware local, Hardware other, string triggerChannel,
            string[] analogStartChannels, int triggerDelay, int analogOrigin)
        {
            Side = side;
            Local = local;
            this.other = other;
            this.triggerChannel = triggerChannel;
            this.analogStartChannels = new HashSet<string>(analogStartChannels);
            this.triggerDelay = triggerDelay;
            this.analogOrigin = analogOrigin;
        }

        public LinkSide Side { get; private set; }
        public Hardware Local { get; private set; }

        public bool UsedOtherChannels { get; private set; }
        public bool UsedOtherAnalog { get; private set; }
        public IList<string> ClampedEvents { get { return clampedEvents; } }

        // ---- Ownership ----

        public bool IsLocalDigital(string name)
        {
            if (Side == LinkSide.Primary && triggerChannel != null && name == triggerChannel)
                throw new LinkRoutingException("Digital channel '" + name + "' is reserved for the secondary trigger in a linked run; scripts must not drive it");
            bool isLocal = Local.DigitalOutputChannels.ContainsKey(name);
            bool isOther = other.DigitalOutputChannels.ContainsKey(name);
            if (isLocal && isOther) throw BothError(name, "Digital");
            if (!isLocal && !isOther) throw NeitherError(name, "Digital");
            if (isOther) UsedOtherChannels = true;
            return isLocal;
        }

        public bool IsLocalAnalog(string name)
        {
            bool isLocal = Local.AnalogOutputChannels.ContainsKey(name);
            bool isOther = other.AnalogOutputChannels.ContainsKey(name);
            if (isLocal && isOther) throw BothError(name, "Analog");
            if (!isLocal && !isOther) throw NeitherError(name, "Analog");
            if (isOther)
            {
                UsedOtherChannels = true;
                UsedOtherAnalog = true;
            }
            return isLocal;
        }

        private Exception BothError(string name, string kind)
        {
            return new LinkRoutingException(kind + " channel '" + name + "' is defined on both "
                + Local.GetType().Name + " and " + other.GetType().Name + "; a linked run needs each channel name on exactly one machine");
        }

        private Exception NeitherError(string name, string kind)
        {
            return new LinkRoutingException(kind + " channel '" + name + "' is not a channel on "
                + Local.GetType().Name + " or " + other.GetType().Name);
        }

        // ---- Time mapping ----

        public int ShiftDigital(int time)
        {
            return Side == LinkSide.Primary ? time : time - triggerDelay;
        }

        public int ShiftAnalog(int time)
        {
            return Side == LinkSide.Primary ? time : time + analogOrigin - triggerDelay;
        }

        public int AnalogLengthExtension
        {
            get { return Side == LinkSide.Primary ? 0 : analogOrigin - triggerDelay; }
        }

        public int MapDigitalPoint(string name, int time)
        {
            return MapPoint(name, "digital", time, ShiftDigital(time));
        }

        public int MapAnalogPoint(string name, int time)
        {
            return MapPoint(name, "analog", time, ShiftAnalog(time));
        }

        public int MapDigitalSpan(string name, int start)
        {
            return MapSpan(name, "digital", start, ShiftDigital(start));
        }

        public int MapAnalogSpan(string name, int start)
        {
            return MapSpan(name, "analog", start, ShiftAnalog(start));
        }

        private int MapPoint(string name, string kind, int time, int shifted)
        {
            if (Side == LinkSide.Primary) return time;
            bool clamped = shifted < 0;
            int mapped = clamped ? 0 : shifted;
            if (clamped) clampedEvents.Add(name + " " + kind + " t=" + time + " -> 0 (was " + shifted + ")");
            if (mapped == 0)
            {
                string key = kind + ":" + name;
                TickZero zero;
                if (!tickZero.TryGetValue(key, out zero))
                {
                    zero = new TickZero();
                    tickZero[key] = zero;
                }
                if (zero.Seen && (zero.AnyClamped || clamped))
                    throw new LinkRoutingException("Two " + kind + " events on '" + name + "' land on secondary tick 0 and at least one was clamped to the trigger, "
                        + "so their order would be lost. Move the events apart or change SecondaryTriggerDelay");
                zero.Seen = true;
                zero.AnyClamped |= clamped;
            }
            return mapped;
        }

        private int MapSpan(string name, string kind, int start, int shifted)
        {
            if (Side == LinkSide.Primary) return start;
            if (shifted < 0)
                throw new LinkRoutingException("A " + kind + " operation on '" + name + "' starts before the secondary is triggered (t=" + start
                    + " maps to secondary tick " + shifted + "); it would be silently shifted or truncated, so it is refused");
            return shifted;
        }

        // ---- Analog start ----

        public void NoteDigitalRise(string name, int time)
        {
            if (!analogStartChannels.Contains(name)) return;
            int earliest;
            if (!earliestRise.TryGetValue(name, out earliest) || time < earliest)
                earliestRise[name] = time;
        }

        public int? AnalogOrigin()
        {
            var ticks = earliestRise.Values.Distinct().ToList();
            if (ticks.Count == 0) return null;
            if (ticks.Count == 1) return ticks[0];
            throw new LinkRoutingException("The analog start is ambiguous: the configured analog-start channels rose at different ticks ("
                + string.Join(", ", earliestRise.Select(kv => kv.Key + "@" + kv.Value)) + ")");
        }
    }
}
