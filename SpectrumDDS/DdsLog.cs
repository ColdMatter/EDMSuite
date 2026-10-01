using System;
using System.IO;

namespace SpectrumDDS
{
    /// <summary>
    /// A small, persistent, plain-text event log for the DDS: the errors this side
    /// throws, and the end-of-run tally that says whether shots were missed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// SpectrumDDSController is a WinForms app with no console, so its
    /// <c>Console.WriteLine</c> calls reach nobody, and the run tally lives only in
    /// memory and is lost on restart. The vendor's <c>spcmdrv_debug.txt</c> is the
    /// only thing on disk and it runs to 50-100 MB/day at level 3. This is the
    /// small file to read first when something went wrong yesterday.
    /// </para>
    /// <para>
    /// <b>Nothing here ever throws.</b> Every method swallows everything: a
    /// logging problem must never affect card operation, and these calls sit in
    /// catch blocks that are already dealing with a real failure.
    /// </para>
    /// <para>
    /// Written into <c>Logs\</c> at the root of the EDMSuite tree, which is
    /// git-ignored. Not beside the vendor log: that lives wherever the driver's
    /// registry setting points, conventionally <c>E:\SpectrumLog</c>, and not
    /// every machine running this code has an E: drive. The filename carries the
    /// date, so a new calendar day starts a fresh file with no rotation logic.
    /// </para>
    /// <para>
    /// Mirrors <c>SharedCode.AppLog</c>, which serves MOTMaster's non-DDS sites.
    /// Kept separate rather than shared because SpectrumDDSController references
    /// nothing but this assembly, and referencing SharedCode would pull DAQmx and
    /// Mongo into a deliberately standalone app.
    /// </para>
    /// </remarks>
    public static class DdsLog
    {
        /// <summary>
        /// Where the log files go. Defaults to <c>Logs\</c> at the root of the
        /// EDMSuite tree; set it before first use to put them somewhere else.
        /// </summary>
        public static string Directory = DefaultDirectory();

        private const string FilePrefix = "controller_events";

        private static readonly object writeLock = new object();

        /// <summary>
        /// <c>Logs\</c> beside EDMSuite.sln, found by walking up from wherever the
        /// executable is -- the apps run out of <c>&lt;project&gt;\bin\&lt;config&gt;\</c>,
        /// so the root is two or three levels up depending on the project.
        /// </summary>
        /// <remarks>
        /// Deliberately not an absolute path: a hard-coded drive letter only works
        /// on the machine it was written for. Falls back to a <c>Logs</c> folder
        /// beside the executable when the marker is not found, which is what a
        /// copied-out deployment would hit.
        /// </remarks>
        private static string DefaultDirectory()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (dir != null)
                {
                    if (File.Exists(Path.Combine(dir.FullName, "EDMSuite.sln")))
                        return Path.Combine(dir.FullName, "Logs");
                    dir = dir.Parent;
                }
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            }
            catch
            {
                // A static initialiser that throws would take the whole type down.
                return "Logs";
            }
        }

        /// <summary>Record an exception, with its stack, against the site that caught it.</summary>
        /// <param name="source">Where it came from, e.g. the method or class name.</param>
        public static void Error(string source, Exception ex)
        {
            try
            {
                Write(source, ex == null ? "(null exception)" : ex.ToString());
            }
            catch
            {
                // Logging must never affect the caller.
            }
        }

        /// <summary>Record a line of plain text against the site that produced it.</summary>
        public static void Write(string source, string message)
        {
            try
            {
                string line = string.Format("{0:yyyy-MM-dd HH:mm:ss}\t{1}\t{2}{3}",
                    DateTime.Now, source, message, Environment.NewLine);
                string file = Path.Combine(Directory,
                    string.Format("{0}_{1:yyyy-MM-dd}.log", FilePrefix, DateTime.Now));

                lock (writeLock)
                {
                    System.IO.Directory.CreateDirectory(Directory);

                    // Two processes write this file -- SpectrumDDSController and,
                    // over remoting, MOTMaster -- and they both write at the end of
                    // a run, within moments of each other. The lock above only
                    // covers threads in one of them, and AppendAllText opens with
                    // FileShare.Read, so the loser of a collision would otherwise
                    // lose its line into the catch below. Retry briefly instead:
                    // a run tally is exactly the line worth not dropping.
                    for (int attempt = 0; ; attempt++)
                    {
                        try
                        {
                            File.AppendAllText(file, line);
                            return;
                        }
                        catch (IOException)
                        {
                            if (attempt >= 4) throw;
                            System.Threading.Thread.Sleep(20);
                        }
                    }
                }
            }
            catch
            {
                // Logging must never affect the caller.
            }
        }
    }
}
