using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DAQ.HAL;
using DAQ.Pattern;

namespace MOTMaster
{
    /// <summary>One side's sequence for a linked run, built without touching any hardware.</summary>
    public sealed class LinkedBuild
    {
        public MOTMasterScript Script;
        public MOTMasterSequence Sequence;
        public LinkRouting Routing;
        /// <summary>The primary tick of the analog start (0 if the script has no secondary analog events).</summary>
        public int AnalogOrigin;
        public int TriggerDelay;
        /// <summary>Length to pass to DigitalPattern.BuildPattern for this side.</summary>
        public int DigitalLength;
    }

    /// <summary>
    /// Runs a script's GetSequence() under the right <see cref="LinkRouting"/> context for either side of a
    /// linked run. Static and hardware-free so it can be tested: it only builds patterns in memory.
    /// </summary>
    public static class LinkedScriptBuilder
    {
        const string DelayParameter = "SecondaryTriggerDelay";

        public static LinkedBuild BuildPrimary(MOTMasterScript script, Hardware local, Hardware other, MOTMasterLinkConfig config)
        {
            LinkRouting routing = LinkRouting.ForPrimary(local, other, config.TriggerChannel, config.AnalogStartChannels);
            MOTMasterSequence sequence;
            using (LinkRouting.Enter(routing))
            {
                sequence = script.GetSequence();
            }

            if (!routing.UsedOtherChannels)
                throw new LinkedRunException("Linked mode is on, but the script uses no channels on " + other.GetType().Name
                    + ". Turn linked mode off for this script, or add " + other.GetType().Name + " channels to it.");

            int analogOrigin = 0;
            if (routing.UsedOtherAnalog)
            {
                int? origin = routing.AnalogOrigin();
                if (origin == null)
                    throw new LinkedRunException("The script has analog events on " + other.GetType().Name + " but there is no analog start: none of the configured analog-start channels ("
                        + string.Join(", ", config.AnalogStartChannels ?? new string[0]) + ") has a rising edge in the primary's digital pattern.");
                analogOrigin = origin.Value;
            }

            int triggerDelay = ReadDelay(script.Parameters);
            int digitalLength = ReadPatternLength(script.Parameters);

            // The trigger channel is reserved while the routing context is active, so it is added here, after leaving it.
            // Using the local hardware's channel directly (rather than the name lookup, which goes through Environs.Hardware)
            // adds exactly what PatternBuilder32.Pulse(0, 0, ticks, name) adds without a context.
            DigitalOutputChannel trigger = local.DigitalOutputChannels[config.TriggerChannel] as DigitalOutputChannel;
            if (trigger == null)
                throw new LinkedRunException("The link's trigger channel '" + config.TriggerChannel + "' is not a digital channel on " + local.GetType().Name);
            sequence.DigitalPattern.GetBoard(trigger).Pulse(0, 0, config.TriggerPulseTicks, trigger.BitNumber);

            return new LinkedBuild
            {
                Script = script,
                Sequence = sequence,
                Routing = routing,
                AnalogOrigin = analogOrigin,
                TriggerDelay = triggerDelay,
                DigitalLength = digitalLength
            };
        }

        public static LinkedBuild BuildSecondary(LinkRunRequest request, Hardware local, Hardware other, Func<string, MOTMasterScript> compileAndLoad)
        {
            string path = ScriptPathFor(request);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, request.ScriptText ?? "", new UTF8Encoding(true));

            MOTMasterScript script;
            try
            {
                // Scripts call LoadGlobalParameters() in their constructor; this makes it use the primary's values.
                MOTMasterScript.LinkedParameterOverride = request.Parameters;
                script = compileAndLoad(path);
            }
            finally
            {
                MOTMasterScript.LinkedParameterOverride = null;
            }
            if (script == null) throw new LinkedRunException("script did not compile or load");

            // Script-specific assignments made in the constructor must not beat the primary's values.
            script.Parameters = new Dictionary<string, object>(request.Parameters);
            int delay = ReadDelay(script.Parameters);
            int patternLength = ReadPatternLength(script.Parameters);

            LinkRouting routing = LinkRouting.ForSecondary(local, other, delay, request.AnalogOrigin);
            MOTMasterSequence sequence;
            using (LinkRouting.Enter(routing))
            {
                sequence = script.GetSequence();
            }

            string masterBoard = (string)local.GetInfo("PatternGeneratorBoard");
            if (masterBoard == null || !sequence.DigitalPattern.Boards.ContainsKey(masterBoard))
                throw new LinkedRunException("The script has no digital events on its master pattern board (" + masterBoard + ") for " + local.GetType().Name
                    + ", so this system would never get a clock for its patterns. Add a digital event on one of its channels on that board.");

            return new LinkedBuild
            {
                Script = script,
                Sequence = sequence,
                Routing = routing,
                AnalogOrigin = request.AnalogOrigin,
                TriggerDelay = delay,
                DigitalLength = patternLength - Math.Max(0, delay)
            };
        }

        public static void BuildPatterns(LinkedBuild b)
        {
            b.Sequence.DigitalPattern.BuildPattern(b.DigitalLength);
            b.Sequence.AnalogPattern.BuildPattern();
            b.Sequence.AnalogStatic.BuildPattern();
        }

        public static string ScriptPathFor(LinkRunRequest r)
        {
            // GetFileName guards against a path in a remote request escaping the link's temp folder.
            string id = Path.GetFileName(r.LinkRunId ?? "");
            string name = Path.GetFileName(r.ScriptFileName ?? "");
            if (id.Length == 0 || name.Length == 0)
                throw new LinkedRunException("The linked run request has no run id or script file name");
            return Path.Combine(Path.GetTempPath(), "MOTMasterLinked", id, name);
        }

        static int ReadDelay(IDictionary<string, object> parameters)
        {
            object value;
            if (parameters == null || !parameters.TryGetValue(DelayParameter, out value))
                throw new LinkedRunException("The script has no '" + DelayParameter + "' parameter. Add it to globalParameters.json (or set it in the script): "
                    + "it is the number of clock ticks between the primary's start and the secondary's trigger.");
            return ReadInt(DelayParameter, value);
        }

        static int ReadPatternLength(IDictionary<string, object> parameters)
        {
            object value;
            if (parameters == null || !parameters.TryGetValue("PatternLength", out value))
                throw new LinkedRunException("The script has no 'PatternLength' parameter");
            return ReadInt("PatternLength", value);
        }

        static int ReadInt(string name, object value)
        {
            if (!(value is int))
                throw new LinkedRunException("Parameter '" + name + "' must be an int but is " + (value == null ? "null" : value.GetType().Name));
            return (int)value;
        }
    }
}
