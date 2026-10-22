using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DAQ.HAL;
using DAQ.Pattern;
using Microsoft.CSharp;
using MOTMaster;
using static MOTMasterTests.TestRunner;

namespace MOTMasterTests
{
    /// <summary>MoleculeMOTHardware plus a trigger channel on a free /PXI1Slot4 bit, so the test needs no real link config.</summary>
    public class CafWithTrigger : MoleculeMOTHardware
    {
        public const int TriggerBit = 7; // port 0, line 7 on /PXI1Slot4: unused by any channel in MoleculeMOTHardware

        public CafWithTrigger()
        {
            AddDigitalOutputChannel("linkTestTrigger", "/PXI1Slot4", 0, 7);
        }
    }

    public static class LinkedScriptBuilderTests
    {
        static readonly string[] AnalogStart = { "DDS_Analog_Trg", "aoPatternTrigger" };

        // The harness passes the repo root as args[0]; fall back to walking up from the current directory
        // and then from the test exe to the folder containing ".git".
        static string RepoRoot(string[] args)
        {
            if (args != null && args.Length > 0 && Directory.Exists(args[0])) return args[0];
            foreach (string start in new[] { Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.BaseDirectory })
            {
                for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
                    if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) return dir.FullName;
            }
            throw new Exception("Cannot find the repo root: pass it as the first argument");
        }

        static string ExamplePath(string[] args)
        {
            return Path.Combine(RepoRoot(args), "MOTMasterTests", "scripts", "LinkedExample.cs");
        }

        static MOTMasterLinkConfig Config()
        {
            return new MOTMasterLinkConfig
            {
                TriggerChannel = "linkTestTrigger",
                AnalogStartChannels = AnalogStart,
                TriggerPulseTicks = 10
            };
        }

        // ---- CodeDom loader, copied from .claude\skills\motmaster-controller\ScriptCompileCheck.cs ----

        static MOTMasterScript CompileAndLoad(string path)
        {
            var options = new CompilerParameters();
            string motMaster = typeof(MOTMasterScript).Assembly.Location;
            string daq = typeof(PatternBuilder32).Assembly.Location;
            options.ReferencedAssemblies.Add(motMaster);
            options.ReferencedAssemblies.Add(daq);
            options.GenerateExecutable = false;
            options.GenerateInMemory = true;

            string binDir = Path.GetDirectoryName(motMaster);
            string spectrumDds = Path.Combine(binDir, "SpectrumDDS.dll");
            if (File.Exists(spectrumDds)) options.ReferencedAssemblies.Add(spectrumDds);

            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromFile(options, path);
            if (results.Errors.HasErrors)
            {
                var lines = new List<string>();
                foreach (CompilerError err in results.Errors)
                    if (!err.IsWarning) lines.Add("line " + err.Line + ": " + err.ErrorNumber + " " + err.ErrorText);
                throw new LinkedRunException("did not compile:\n" + string.Join("\n", lines));
            }

            object instance = null;
            foreach (Type type in results.CompiledAssembly.GetTypes())
                if (type.IsClass) instance = Activator.CreateInstance(type);
            var script = instance as MOTMasterScript;
            if (script == null) throw new LinkedRunException("no class derives from MOTMasterScript");
            return script;
        }

        static LinkRunRequest RequestFrom(LinkedBuild primary, string path, int delay)
        {
            var parameters = new Dictionary<string, object>(primary.Script.Parameters);
            parameters["SecondaryTriggerDelay"] = delay;
            return new LinkRunRequest
            {
                LinkRunId = Guid.NewGuid().ToString("N"),
                PrimaryName = "test",
                PrimaryHardwareType = typeof(CafWithTrigger).FullName,
                ScriptFileName = "LinkedExample.cs",
                ScriptText = File.ReadAllText(path),
                Parameters = parameters,
                AnalogOrigin = primary.AnalogOrigin
            };
        }

        static LinkedBuild Primary(string path, CafWithTrigger caf, RbTweezerHardware rb)
        {
            return LinkedScriptBuilder.BuildPrimary(CompileAndLoad(path), caf, rb, Config());
        }

        static LinkedBuild Secondary(LinkRunRequest request, CafWithTrigger caf, RbTweezerHardware rb)
        {
            return LinkedScriptBuilder.BuildSecondary(request, rb, caf, CompileAndLoad);
        }

        static string RbDigitalBoard(RbTweezerHardware rb)
        {
            return (string)rb.GetInfo("PatternGeneratorBoard");
        }

        static string Device(Hardware h, string analogChannel)
        {
            return ((AnalogOutputChannel)h.AnalogOutputChannels[analogChannel]).Device;
        }

        static bool Bit(PatternBuilder32 p, string board, int tick, int bit)
        {
            return (p.Boards[board].Pattern[tick] & (1u << bit)) != 0;
        }

        // ---- Tests ----

        public static void TestPrimaryBuildsOnlyCafAndAddsTrigger(string[] args)
        {
            string path = ExamplePath(args);
            var caf = new CafWithTrigger();
            var rb = new RbTweezerHardware();
            LinkedBuild b = Primary(path, caf, rb);

            string cafBoard = (string)caf.GetInfo("PatternGeneratorBoard");
            Assert.True(!b.Sequence.DigitalPattern.Boards.ContainsKey(RbDigitalBoard(rb)), "no RbTweezer digital board");
            Assert.True(b.Sequence.DigitalPattern.Boards.ContainsKey(cafBoard), "CaF master board present");
            Assert.True(!b.Sequence.AnalogPattern.Boards.ContainsKey(Device(rb, "tMOTcc")), "no RbTweezer analog board");
            Assert.True(b.Sequence.AnalogPattern.Boards.ContainsKey(Device(caf, "MOTCoilsCurrent")), "CaF analog board present");
            Assert.Equal(4000, b.AnalogOrigin, "analog origin");
            Assert.Equal((int)b.Script.Parameters["TCLBlockStart"], b.AnalogOrigin, "analog origin == TCLBlockStart");
            Assert.Equal(2, b.TriggerDelay, "trigger delay");
            Assert.Equal(50000, b.DigitalLength, "digital length");

            LinkedScriptBuilder.BuildPatterns(b);
            for (int t = 0; t < 10; t++)
                Assert.True(Bit(b.Sequence.DigitalPattern, cafBoard, t, CafWithTrigger.TriggerBit), "trigger high at tick " + t);
            Assert.True(!Bit(b.Sequence.DigitalPattern, cafBoard, 10, CafWithTrigger.TriggerBit), "trigger low at tick 10");
            Assert.True(!Bit(b.Sequence.DigitalPattern, cafBoard, 100, CafWithTrigger.TriggerBit), "trigger low at tick 100");
            Assert.True(LinkRouting.Current == null, "routing context left");
        }

        public static void TestSecondaryBuildsOnlyRbAtMappedTimes(string[] args)
        {
            string path = ExamplePath(args);
            var caf = new CafWithTrigger();
            var rb = new RbTweezerHardware();
            LinkedBuild primary = Primary(path, caf, rb);
            LinkedBuild s = Secondary(RequestFrom(primary, path, 2), caf, rb);

            string rbBoard = RbDigitalBoard(rb);
            Assert.Equal(1, s.Sequence.DigitalPattern.Boards.Count, "secondary digital boards");
            Assert.True(s.Sequence.DigitalPattern.Boards.ContainsKey(rbBoard), "secondary only on " + rbBoard);
            Assert.Equal(1, s.Sequence.AnalogPattern.Boards.Count, "secondary analog boards");
            Assert.True(s.Sequence.AnalogPattern.Boards.ContainsKey(Device(rb, "tMOTcc")), "secondary analog on the Rb board");
            Assert.Equal(50000 - 2, s.DigitalLength, "secondary digital length");
            Assert.Equal(2, s.TriggerDelay, "secondary delay");
            Assert.Equal(4000, s.AnalogOrigin, "secondary analog origin");

            LinkedScriptBuilder.BuildPatterns(s);
            int bit = ((DigitalOutputChannel)rb.DigitalOutputChannels["tCoolSwitch"]).BitNumber;
            Assert.True(!Bit(s.Sequence.DigitalPattern, rbBoard, 997, bit), "tCoolSwitch low at 997");
            Assert.True(Bit(s.Sequence.DigitalPattern, rbBoard, 998, bit), "tCoolSwitch high at 998");
            Assert.True(Bit(s.Sequence.DigitalPattern, rbBoard, 1497, bit), "tCoolSwitch high at 1497");
            Assert.True(!Bit(s.Sequence.DigitalPattern, rbBoard, 1498, bit), "tCoolSwitch low at 1498");

            var d = s.Sequence.AnalogPattern.Boards[Device(rb, "tMOTcc")].AnalogPatterns["tMOTcc"];
            Assert.True(d.ContainsKey(100 + 4000 - 2), "tMOTcc event at 4098");
            Assert.Equal(2.5, d[4098], "tMOTcc value");
            Assert.Equal(1, d.Count, "only one tMOTcc event");
            Assert.True(LinkRouting.Current == null, "routing context left");
            Assert.True(MOTMasterScript.LinkedParameterOverride == null, "parameter override cleared");
        }

        public static void TestFingerprintsAgreeAcrossSides(string[] args)
        {
            string path = ExamplePath(args);
            var caf = new CafWithTrigger();
            var rb = new RbTweezerHardware();
            LinkedBuild primary = Primary(path, caf, rb);
            LinkRunRequest request = RequestFrom(primary, path, 2);
            LinkedBuild secondary = Secondary(request, caf, rb);

            Assert.Equal(LinkFingerprint.OfParameters(primary.Script.Parameters),
                LinkFingerprint.OfParameters(secondary.Script.Parameters), "parameter fingerprints");
            string written = File.ReadAllText(LinkedScriptBuilder.ScriptPathFor(request));
            Assert.Equal(LinkFingerprint.OfText(File.ReadAllText(path)), LinkFingerprint.OfText(written), "script text fingerprints");
        }

        public static void TestLinkedOnButNoSecondaryChannels(string[] args)
        {
            string path = Path.Combine(RepoRoot(args), "MoleculeMOTMasterScripts", "2.a DDSBlueMOT.cs");
            var caf = new CafWithTrigger();
            var rb = new RbTweezerHardware();
            var e = Assert.Throws<LinkedRunException>(() => Primary(path, caf, rb));
            StringAssert(e.Message, "uses no channels on", "RbTweezerHardware");
        }

        public static void TestDelayOverridePerRun(string[] args)
        {
            string path = ExamplePath(args);
            var caf = new CafWithTrigger();
            var rb = new RbTweezerHardware();
            LinkedBuild primary = Primary(path, caf, rb);
            string rbBoard = RbDigitalBoard(rb);
            int bit = ((DigitalOutputChannel)rb.DigitalOutputChannels["tCoolSwitch"]).BitNumber;

            foreach (int[] c in new[] { new[] { 2, 998 }, new[] { 7, 993 } })
            {
                LinkedBuild s = Secondary(RequestFrom(primary, path, c[0]), caf, rb);
                Assert.Equal(c[0], s.TriggerDelay, "delay " + c[0]);
                Assert.Equal(50000 - c[0], s.DigitalLength, "length for delay " + c[0]);
                LinkedScriptBuilder.BuildPatterns(s);
                Assert.True(!Bit(s.Sequence.DigitalPattern, rbBoard, c[1] - 1, bit), "low before edge, delay " + c[0]);
                Assert.True(Bit(s.Sequence.DigitalPattern, rbBoard, c[1], bit), "edge at " + c[1] + " for delay " + c[0]);
                var d = s.Sequence.AnalogPattern.Boards[Device(rb, "tMOTcc")].AnalogPatterns["tMOTcc"];
                Assert.True(d.ContainsKey(4100 - c[0]), "analog event at " + (4100 - c[0]) + " for delay " + c[0]);
            }
        }

        public static void TestMissingDelayParameter(string[] args)
        {
            string path = ExamplePath(args);
            var caf = new CafWithTrigger();
            var rb = new RbTweezerHardware();

            MOTMasterScript script = CompileAndLoad(path);
            script.Parameters.Remove("SecondaryTriggerDelay");
            var e = Assert.Throws<LinkedRunException>(() => LinkedScriptBuilder.BuildPrimary(script, caf, rb, Config()));
            StringAssert(e.Message, "SecondaryTriggerDelay", "globalParameters.json");

            LinkedBuild primary = Primary(path, caf, rb);
            LinkRunRequest request = RequestFrom(primary, path, 2);
            request.Parameters.Remove("SecondaryTriggerDelay");
            // the example script sets the delay itself, but BuildSecondary replaces Parameters with the request's
            var e2 = Assert.Throws<LinkedRunException>(() => Secondary(request, caf, rb));
            StringAssert(e2.Message, "SecondaryTriggerDelay", "globalParameters.json");
            Assert.True(MOTMasterScript.LinkedParameterOverride == null, "parameter override cleared after failure");
        }

        public static void TestSecondaryNullScriptAndCompileFailure(string[] args)
        {
            string path = ExamplePath(args);
            var caf = new CafWithTrigger();
            var rb = new RbTweezerHardware();
            LinkedBuild primary = Primary(path, caf, rb);
            LinkRunRequest request = RequestFrom(primary, path, 2);

            var e = Assert.Throws<LinkedRunException>(() =>
                LinkedScriptBuilder.BuildSecondary(request, rb, caf, p => null));
            StringAssert(e.Message, "script did not compile or load");
            Assert.True(MOTMasterScript.LinkedParameterOverride == null, "override cleared after null script");

            request.ScriptText = "this is not C#";
            Assert.Throws<LinkedRunException>(() => Secondary(request, caf, rb));
            Assert.True(MOTMasterScript.LinkedParameterOverride == null, "override cleared after compile failure");
        }

        public static void TestSecondaryWithNoDigitalOnMasterBoard(string[] args)
        {
            string path = ExamplePath(args);
            var caf = new CafWithTrigger();
            var rb = new RbTweezerHardware();
            LinkedBuild primary = Primary(path, caf, rb);
            LinkRunRequest request = RequestFrom(primary, path, 2);
            // A local hardware whose master board has no events: its tCoolSwitch lives on a different board.
            var other = new TestHardware().Digital("DDS_Analog_Trg", "/Dx", 0, 0).Digital("motLightSwitch", "/Dx", 0, 1)
                .Analog("MOTCoilsCurrent", "/Ax/ao0");
            var e = Assert.Throws<LinkedRunException>(() =>
                LinkedScriptBuilder.BuildSecondary(request, new TestHardwareWithMaster(), other, CompileAndLoad));
            StringAssert(e.Message, "no digital events on its master pattern board");
        }

        // The run folder is deleted recursively when a linked run ends, so a remote request must not be able
        // to point it at the link's temp root or anything above it.
        public static void TestScriptPathRejectsEscapingNames()
        {
            foreach (string id in new[] { "..", ".", "", null, "...", ". ", "a\\b", "a/b", "c:x" })
            {
                var r = new LinkRunRequest { LinkRunId = id, ScriptFileName = "s.cs" };
                Assert.Throws<LinkedRunException>(() => LinkedScriptBuilder.ScriptPathFor(r));
            }
            foreach (string name in new[] { "..", ".", "", null, "..\\s.cs", "sub/s.cs" })
            {
                var r = new LinkRunRequest { LinkRunId = "id1", ScriptFileName = name };
                Assert.Throws<LinkedRunException>(() => LinkedScriptBuilder.ScriptPathFor(r));
            }

            string ok = LinkedScriptBuilder.ScriptPathFor(new LinkRunRequest { LinkRunId = "id1", ScriptFileName = "s.cs" });
            Assert.Equal(Path.Combine(LinkedScriptBuilder.TempRoot, "id1", "s.cs"), ok, "plain names accepted");
        }

        // The primary compiles, sends, hashes and saves one read of the script, via its own temp copy.
        public static void TestPrimaryScriptCopy(string[] args)
        {
            string id = Guid.NewGuid().ToString("N");
            string copy = LinkedScriptBuilder.PrimaryScriptPathFor(id, "LinkedExample.cs");
            var request = new LinkRunRequest { LinkRunId = id, ScriptFileName = "LinkedExample.cs" };
            Assert.True(!string.Equals(copy, LinkedScriptBuilder.ScriptPathFor(request), StringComparison.OrdinalIgnoreCase),
                "primary copy is not where a secondary on the same machine would write");
            Assert.Equal("LinkedExample.cs", Path.GetFileName(copy));
            string folder = Path.GetDirectoryName(copy);
            Assert.True(LinkedScriptBuilder.IsRunFolder(folder), "primary copy folder is a deletable run folder");

            foreach (string bad in new[] { "..", "", null, "..\\s.cs", "sub/s.cs" })
                Assert.Throws<LinkedRunException>(() => LinkedScriptBuilder.PrimaryScriptPathFor(id, bad));
            foreach (string bad in new[] { "..", "", null, "a\\b" })
                Assert.Throws<LinkedRunException>(() => LinkedScriptBuilder.PrimaryScriptPathFor(bad, "s.cs"));

            string text = File.ReadAllText(ExamplePath(args)) + "\n// éµ non-ASCII survives\n";
            try
            {
                LinkedScriptBuilder.WriteScript(copy, text);
                Assert.Equal(text, File.ReadAllText(copy), "primary copy holds exactly the text read");
                // What the secondary writes from the same text reads back identically, so the hashes agree.
                request.ScriptText = text;
                string secondaryCopy = LinkedScriptBuilder.ScriptPathFor(request);
                LinkedScriptBuilder.WriteScript(secondaryCopy, request.ScriptText);
                Assert.Equal(LinkFingerprint.OfText(text), LinkFingerprint.OfText(File.ReadAllText(secondaryCopy)), "hashes agree");
                Assert.Equal(LinkFingerprint.OfText(File.ReadAllText(copy)), LinkFingerprint.OfText(File.ReadAllText(secondaryCopy)), "file hashes agree");
            }
            finally
            {
                if (LinkedScriptBuilder.IsRunFolder(folder) && Directory.Exists(folder)) Directory.Delete(folder, true);
                string secondaryFolder = Path.GetDirectoryName(LinkedScriptBuilder.ScriptPathFor(request));
                if (LinkedScriptBuilder.IsRunFolder(secondaryFolder) && Directory.Exists(secondaryFolder)) Directory.Delete(secondaryFolder, true);
            }
        }

        public static void TestOnlyRunFoldersAreDeletable()
        {
            string root = LinkedScriptBuilder.TempRoot;
            Assert.True(LinkedScriptBuilder.IsRunFolder(Path.Combine(root, "id1")), "direct child of the root");
            Assert.True(LinkedScriptBuilder.IsRunFolder(Path.Combine(root, "id1") + "\\"), "direct child with trailing separator");
            Assert.True(!LinkedScriptBuilder.IsRunFolder(root), "the root itself");
            Assert.True(!LinkedScriptBuilder.IsRunFolder(root + "\\"), "the root with trailing separator");
            Assert.True(!LinkedScriptBuilder.IsRunFolder(Path.Combine(root, "..")), "the temp folder above the root");
            Assert.True(!LinkedScriptBuilder.IsRunFolder(Path.Combine(root, ".")), "the root via '.'");
            Assert.True(!LinkedScriptBuilder.IsRunFolder(Path.Combine(root, "id1", "sub")), "a grandchild");
            Assert.True(!LinkedScriptBuilder.IsRunFolder(Path.GetTempPath()), "the temp folder");
            Assert.True(!LinkedScriptBuilder.IsRunFolder(""), "empty");
            Assert.True(!LinkedScriptBuilder.IsRunFolder(null), "null");
        }

        class TestHardwareWithMaster : TestHardware
        {
            public TestHardwareWithMaster()
            {
                Info.Add("PatternGeneratorBoard", "/MasterBoard");
                Digital("tCoolSwitch", "/OtherBoard", 0, 0).Analog("tMOTcc", "/Ay/ao0");
            }
        }
    }
}
