// Reproduces exactly what MOTMaster.Controller does with a script up to the point of
// touching hardware: compile it (Controller.compileFromFile), load and instantiate it
// (Controller.loadScriptFromDLL), call GetSequence(), then build the digital/analog/
// static patterns (Controller.buildPattern). This is the same boundary
// Controller.ViewPattern() (the "View Pattern" button) already stops at - nothing here
// calls initializeHardware/run/releaseHardware, constructs a DAQMxPatternGenerator, or
// opens the DDS card. Safe to run with no NI hardware present and no permission needed.
//
// Shared by ScriptTests.cs (xUnit, for VS Code's Test Explorer / `dotnet test`) and by
// .claude/skills/motmaster-controller/ScriptSmokeTest.cs (a standalone console version
// of the same check, for use outside a full dotnet/VS Code setup). If you fix or extend
// the logic here, check whether that other copy needs the same change.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using MOTMaster;

namespace MOTMaster.ScriptTests
{
    public static class ScriptCompileCheck
    {
        private static bool _resolverInstalled;
        private static readonly object ResolverLock = new object();

        /// <summary>
        /// Checks one script. Returns null on success, or a human-readable failure
        /// description (compile errors, or the exception thrown while constructing the
        /// script / building its pattern).
        /// </summary>
        /// <remarks>
        /// Deliberately just a thin wrapper: it must not itself reference any MOTMaster/DAQ
        /// type. The JIT resolves every type touched anywhere in a method's IL before that
        /// method's *own* first statement runs - so if the AssemblyResolve hookup and the
        /// MOTMasterScript-touching logic were in the same method, the JIT would already be
        /// trying (and failing) to load MOTMaster.exe before the hookup executed. Splitting
        /// into this clean outer method (no MOTMaster/DAQ types anywhere in its signature or
        /// body) and <see cref="CheckImpl"/> (JIT'd lazily, only once actually called, by
        /// which point the resolver is registered) sidesteps that.
        /// </remarks>
        public static string Check(string scriptPath, string[] referencedAssemblies)
        {
            EnsureAssemblyResolver(referencedAssemblies);
            return CheckImpl(scriptPath, referencedAssemblies);
        }

        private static string CheckImpl(string scriptPath, string[] referencedAssemblies)
        {
            // ---- Stage 1: compile, exactly like Controller.compileFromFile ----
            var options = new CompilerParameters();
            foreach (string asm in referencedAssemblies)
                options.ReferencedAssemblies.Add(asm);
            options.GenerateExecutable = false;
            options.GenerateInMemory = true;

            var provider = new CSharpCodeProvider();
            CompilerResults results;
            try
            {
                results = provider.CompileAssemblyFromFile(options, scriptPath);
            }
            catch (Exception ex)
            {
                return "compiler threw: " + ex.Message;
            }

            if (results.Errors.HasErrors)
            {
                var lines = new List<string>();
                foreach (CompilerError err in results.Errors)
                    if (!err.IsWarning)
                        lines.Add(string.Format("line {0}: {1} {2}", err.Line, err.ErrorNumber, err.ErrorText));
                return "did not compile:\n" + string.Join("\n", lines);
            }

            // ---- Stage 2: load + instantiate, exactly like Controller.loadScriptFromDLL ----
            object loadedInstance = null;
            try
            {
                Assembly assembly = results.CompiledAssembly;
                foreach (Type type in assembly.GetTypes())
                    if (type.IsClass)
                        loadedInstance = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                return "threw while constructing the script (runs in the constructor - check Parameters[...] assignments and LoadGlobalParameters()):\n"
                     + Describe(ex);
            }

            var script = loadedInstance as MOTMasterScript;
            if (script == null)
            {
                return "compiled, but no class in the file derives from MOTMasterScript "
                     + "(Controller would silently fail to cast it and MOTMaster would show \"Unable to load pattern\").";
            }

            // ---- Stage 3: build the sequence, exactly like Controller.Go/ViewPattern up to buildPattern ----
            try
            {
                MOTMasterSequence sequence = script.GetSequence();

                object patternLengthObj;
                if (!script.Parameters.TryGetValue("PatternLength", out patternLengthObj))
                    return "Parameters has no \"PatternLength\" entry - every script needs one (usually from globalParameters.txt or a script-specific override).";

                int patternLength;
                try
                {
                    patternLength = (int)patternLengthObj;
                }
                catch (InvalidCastException)
                {
                    return string.Format(
                        "Parameters[\"PatternLength\"] is a {0}, not an int - MOTMaster casts it with (int) and would throw.",
                        patternLengthObj.GetType().FullName);
                }

                sequence.DigitalPattern.BuildPattern(patternLength);
                sequence.AnalogPattern.BuildPattern();
                sequence.AnalogStatic.BuildPattern();
            }
            catch (Exception ex)
            {
                string hint = "";
                if (ex is NullReferenceException && ex.StackTrace != null && ex.StackTrace.Contains(".GetBoard(")
                    && (ex.StackTrace.Contains("PatternBuilder32.GetBoard") || ex.StackTrace.Contains("AnalogPatternBuilder.GetBoard")))
                {
                    // DigitalOutputChannels/AnalogOutputChannels is a Hashtable: a lookup by
                    // an unregistered channel name returns null instead of throwing, so the
                    // very next line (channel.Device) NullReferenceExceptions with no mention
                    // of the channel name at all. Confirmed cause every time this shape has
                    // come up - see architecture.md's "MOTMasterEXEPath" note for the same
                    // Hashtable-null-on-miss pattern elsewhere in this codebase.
                    hint = "\n\nThis shape (NullReferenceException inside GetBoard) almost always means the script "
                         + "passed a channel name that isn't registered in this experiment's Hardware class (a typo, or a "
                         + "channel that was renamed/removed there) - Environs.Hardware.*OutputChannels[name] silently "
                         + "returns null instead of throwing on a miss. Grep the script's AddEdge/Pulse/DownPulse/"
                         + "AddAnalogValue/etc. channel-name arguments against the Hardware class's AddDigitalOutputChannel/"
                         + "AddAnalogOutputChannel calls to find the bad one.";
                }

                return "compiled and constructed, but threw while building its pattern "
                     + "(GetSequence()/GetDigitalPattern()/GetAnalogPattern() - check channel names against "
                     + "the experiment's Hardware class, and PatternLength against the events in the script):\n"
                     + Describe(ex) + hint;
            }

            return null;
        }

        private static string Describe(Exception ex)
        {
            string s = ex.GetType().FullName + ": " + ex.Message + "\n" + ex.StackTrace;
            if (ex.InnerException != null)
                s += "\n  --> " + Describe(ex.InnerException);
            return s;
        }

        /// <summary>
        /// GenerateInMemory'd script assemblies are loaded with no path context, so the
        /// CLR's default probing (this process's own directory + the GAC) can't find
        /// MOTMaster.exe/DAQ.dll/etc. even though they were resolvable at compile time.
        /// Resolve by simple name against the same paths every script is compiled against.
        /// </summary>
        private static void EnsureAssemblyResolver(string[] referencedAssemblies)
        {
            lock (ResolverLock)
            {
                if (_resolverInstalled) return;
                AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
                {
                    string simpleName = new AssemblyName(resolveArgs.Name).Name;
                    string match = referencedAssemblies.FirstOrDefault(p =>
                        string.Equals(Path.GetFileNameWithoutExtension(p), simpleName, StringComparison.OrdinalIgnoreCase));
                    return match != null ? Assembly.LoadFrom(match) : null;
                };
                _resolverInstalled = true;
            }
        }
    }
}
