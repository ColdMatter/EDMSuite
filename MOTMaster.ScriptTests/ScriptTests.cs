// One test per top-level *.cs file in MoleculeMOTMasterScripts, shown individually in
// VS Code's Test Explorer (and by `dotnet test`). A green check means MOTMaster will
// compile, load, and build a runnable pattern from that script - the same thing
// Controller does before it ever touches a board. See ScriptCompileCheck.cs for exactly
// what "runnable" means here, and the motmaster-controller skill's SKILL.md for how this
// fits into checking a script edit before trying it in MOTMaster itself.
//
// cafmot/PH-BONESAW-specific (see the motmaster-controller skill's "Scope" section): the
// folder, configuration, and assembly paths below are this experiment's own. Another
// experiment wanting the same Test Explorer experience should copy this project and
// change ScriptFolder/Configuration.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace MOTMaster.ScriptTests
{
    public class ScriptTests
    {
        private const string Configuration = "CaF";
        private const string ScriptFolderName = "MoleculeMOTMasterScripts";

        private static readonly string RepoRoot = FindRepoRoot();
        private static readonly string ScriptFolder = Path.Combine(RepoRoot, ScriptFolderName);

        public static IEnumerable<object[]> Scripts()
        {
            if (!Directory.Exists(ScriptFolder))
                yield break;

            // Top-level only, matching Controller.ScriptLookupAndDisplay - subfolders
            // (OLD, OldDDS, preJuly26, ...) are never part of the real script list.
            foreach (string path in Directory.GetFiles(ScriptFolder, "*.cs", SearchOption.TopDirectoryOnly)
                                              .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
                yield return new object[] { Path.GetFileName(path) };
        }

        [Theory]
        [MemberData(nameof(Scripts))]
        public void CompilesAndBuildsPattern(string scriptFileName)
        {
            string scriptPath = Path.Combine(ScriptFolder, scriptFileName);
            string[] referencedAssemblies = BuildReferencedAssemblies();

            string failure = ScriptCompileCheck.Check(scriptPath, referencedAssemblies);

            Assert.True(failure == null, failure);
        }

        private static string[] BuildReferencedAssemblies()
        {
            string motMasterExe = Path.Combine(RepoRoot, "MOTMaster", "bin", Configuration, "MOTMaster.exe");
            string daqDll = Path.Combine(RepoRoot, "DAQ", "bin", Configuration, "DAQ.dll");
            string spectrumDds = Path.Combine(RepoRoot, "SpectrumDDS", "bin", Configuration, "SpectrumDDS.dll");

            var refs = new List<string> { motMasterExe, daqDll };
            if (File.Exists(spectrumDds)) refs.Add(spectrumDds);
            return refs.ToArray();
        }

        /// <summary>Walks up from the test assembly's own output folder to find EDMSuite.sln.</summary>
        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "EDMSuite.sln")))
                dir = dir.Parent;

            if (dir == null)
                throw new InvalidOperationException(
                    "Could not find EDMSuite.sln above " + AppContext.BaseDirectory + " - is this project still under the repo root?");

            return dir.FullName;
        }
    }
}
