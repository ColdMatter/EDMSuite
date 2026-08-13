using System;
using System.IO;

namespace SharedCode
{
    /// <summary>
    /// A small, persistent, plain-text event log for the applications in this suite.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Both MOTMaster and SpectrumDDSController are WinForms apps with no console
    /// attached, so the <c>Console.WriteLine</c> calls scattered through them reach
    /// nobody, and an exception that is caught and shown in a MessageBox leaves no
    /// record at all once the box is dismissed. Diagnosing anything after the fact
    /// then means re-deriving it from the vendor's 50-100 MB/day driver log. This
    /// gives the errors and the notable events a home of their own, in a file small
    /// enough to read.
    /// </para>
    /// <para>
    /// <b>Nothing here ever throws.</b> Every method swallows everything: a full
    /// disk or an unavailable drive must never change the behaviour of the caller,
    /// which is usually already in a catch block dealing with a real problem.
    /// </para>
    /// <para>
    /// The filename carries the date, so a new calendar day starts a fresh file and
    /// there is no rotation logic to get wrong. Format is tab-separated so it is
    /// both readable and greppable.
    /// </para>
    /// <para>
    /// The DDS side has its own mirror of this class, <c>SpectrumDDS.DdsLog</c>,
    /// writing its own file into the same directory. Deliberately separate:
    /// SpectrumDDSController is a standalone app with no reference to SharedCode,
    /// and giving it one would drag DAQmx and Mongo in behind it for no reason.
    /// </para>
    /// </remarks>
    public static class AppLog
    {
        /// <summary>
        /// Where the log files go. Defaults to <c>Logs\</c> at the root of the
        /// EDMSuite tree; set it before first use to put them somewhere else.
        /// </summary>
        public static string Directory = DefaultDirectory();

        private const string FilePrefix = "motmaster";

        private static readonly object writeLock = new object();

        /// <summary>
        /// <c>Logs\</c> beside EDMSuite.sln, found by walking up from wherever the
        /// executable is -- the apps run out of <c>&lt;project&gt;\bin\&lt;config&gt;\</c>,
        /// so the root is two or three levels up depending on the project.
        /// </summary>
        /// <remarks>
        /// Deliberately not an absolute path: this assembly is used by every
        /// experiment on the suite, and a hard-coded drive letter only works on the
        /// machine it was written for. Falls back to a <c>Logs</c> folder beside the
        /// executable when the marker is not found, which is what a copied-out
        /// deployment would hit.
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

                    // The lock only covers threads in this process, and
                    // AppendAllText opens with FileShare.Read, so anything else
                    // holding the file -- a second copy of the app, a text editor
                    // mid-save -- would otherwise cost this line. Retry briefly.
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
