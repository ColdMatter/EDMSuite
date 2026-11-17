using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MOTMaster
{
    /// <summary>
    /// Which source revision this MOTMaster is running. Used to warn (never block) when the primary and
    /// secondary differ, and recorded in both machines' run files.
    /// </summary>
    public static class BuildProvenance
    {
        static string cached;
        static readonly object gate = new object();

        /// <summary>"git sha[ +local changes] | MOTMaster.exe utc | DAQ.dll utc". Computed once; never throws.</summary>
        public static string Describe()
        {
            lock (gate)
            {
                if (cached == null)
                {
                    try { cached = Compute(); }
                    catch (Exception e) { cached = "git unavailable | provenance error: " + e.Message; }
                }
                return cached;
            }
        }

        /// <summary>True when the text before the first " | " matches.</summary>
        public static bool SameSource(string a, string b)
        {
            return SourcePart(a) == SourcePart(b);
        }

        static string SourcePart(string s)
        {
            if (s == null) return "";
            int i = s.IndexOf(" | ", StringComparison.Ordinal);
            return i < 0 ? s : s.Substring(0, i);
        }

        static string Compute()
        {
            string exePath = typeof(BuildProvenance).Assembly.Location;
            string exeDir = Path.GetDirectoryName(exePath);
            string dllPath = typeof(DAQ.HAL.MOTMasterLinkConfig).Assembly.Location;
            return GitPart(exeDir) + " | MOTMaster.exe " + Stamp(exePath) + " | DAQ.dll " + Stamp(dllPath);
        }

        static string Stamp(string path)
        {
            try { return File.GetLastWriteTimeUtc(path).ToString("yyyy-MM-ddTHH:mm:ssZ"); }
            catch { return "unknown"; }
        }

        static string GitPart(string startDir)
        {
            try
            {
                string root = FindRepoRoot(startDir);
                if (root == null) return "git unavailable";
                string sha = RunGit("-C \"" + root + "\" rev-parse HEAD");
                if (sha == null) return "git unavailable";
                sha = sha.Trim();
                if (sha.Length == 0) return "git unavailable";
                string status = RunGit("-C \"" + root + "\" status --porcelain --untracked-files=no");
                bool dirty = status != null && status.Trim().Length > 0;
                return "git " + sha + (dirty ? " +local changes" : "");
            }
            catch
            {
                return "git unavailable";
            }
        }

        static string FindRepoRoot(string dir)
        {
            while (!string.IsNullOrEmpty(dir))
            {
                string dotGit = Path.Combine(dir, ".git");
                if (Directory.Exists(dotGit) || File.Exists(dotGit)) return dir;
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        /// <summary>Runs git with a 5 s timeout. Returns stdout, or null on any failure.</summary>
        static string RunGit(string arguments)
        {
            var psi = new ProcessStartInfo("git", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process p = Process.Start(psi))
            {
                // Drain both pipes asynchronously so a full buffer can never block git.
                var outTask = p.StandardOutput.ReadToEndAsync();
                var errTask = p.StandardError.ReadToEndAsync();
                var budget = Stopwatch.StartNew();
                if (!p.WaitForExit(5000))
                {
                    try { p.Kill(); } catch { }
                    return null;
                }
                if (p.ExitCode != 0) return null;
                // A grandchild holding the pipe open would keep stdout from reaching EOF; bound the read
                // by what is left of the 5 s budget.
                int remaining = Math.Max(0, 5000 - (int)budget.ElapsedMilliseconds);
                if (!outTask.Wait(remaining)) return null;
                return outTask.Result;
            }
        }
    }
}
