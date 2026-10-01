using System;
using System.IO;
using Microsoft.Win32;

namespace SpectrumDDS
{
    /// <summary>
    /// Reads and writes the same registry key the Spectrum Control Center's
    /// Debugging tab edits, and rotates the driver's debug log at the calendar-day
    /// boundary.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Undocumented by Spectrum, but it is just a registry key:
    /// <c>HKCU\SOFTWARE\Spectrum GmbH\spcm-driver\Debug</c>, values
    /// <c>LogLevel</c> (DWORD), <c>LogPath</c> (a directory, string) and
    /// <c>LogAppend</c> (DWORD). There is no per-run filename control -- the
    /// driver always writes a fixed <c>spcmdrv_debug.txt</c> into
    /// <c>LogPath</c> -- so per-day files are produced here by renaming that
    /// fixed file aside once it is no longer today's, before the driver's next
    /// write recreates it. Proved safe live on this card: renaming the active
    /// file out from under a running connection does not disrupt it, and the
    /// driver creates a fresh one on its very next log write.
    /// </para>
    /// <para>
    /// <b>Registry changes are not picked up by an already-open connection.</b>
    /// Confirmed on the bench: changing <c>LogLevel</c> while a card was open
    /// and logging kept the old verbosity going for as long as it was watched,
    /// with no sign of it being re-read. A level or path change here only takes
    /// effect the next time the card is opened.
    /// </para>
    /// </remarks>
    public static class DebugLogSettings
    {
        private const string KeyPath = @"SOFTWARE\Spectrum GmbH\spcm-driver\Debug";
        private const string ActiveFileName = "spcmdrv_debug.txt";

        public static int LogLevel { get { return ReadInt("LogLevel", 0); } }
        public static string LogPath { get { return ReadString("LogPath", ""); } }

        /// <summary>
        /// Write new Log Level / Log Path values. Takes effect the next time the
        /// card is opened, not on the connection already running -- see remarks.
        /// </summary>
        public static void Apply(int logLevel, string logPath)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath))
            {
                if (key == null)
                    throw new InvalidOperationException("could not open or create " + KeyPath);
                key.SetValue("LogLevel", logLevel, RegistryValueKind.DWord);
                key.SetValue("LogPath", logPath ?? "", RegistryValueKind.String);
            }
        }

        /// <summary>
        /// If the active log file is left over from an earlier day, archive it as
        /// <c>spcmdrv_log_{that day:yyyy-MM-dd}_level{level}.txt</c> -- date before
        /// level so alphabetical order in a file browser is also date order -- so
        /// the driver starts a fresh <c>spcmdrv_debug.txt</c> on its next write.
        /// Driven off the file's
        /// own last-write date rather than remembered state, so it self-heals
        /// across a controller restart that missed a midnight boundary. Never
        /// throws -- a logging problem must never affect card operation.
        /// </summary>
        public static void RotateIfNewDay()
        {
            try
            {
                string path = LogPath;
                if (string.IsNullOrEmpty(path)) return;
                string activeFile = Path.Combine(path, ActiveFileName);
                if (!File.Exists(activeFile)) return;

                DateTime fileDay = File.GetLastWriteTime(activeFile).Date;
                if (fileDay >= DateTime.Today) return;

                // Date before level so alphabetical order in a file browser is also
                // date order.
                string archiveName = string.Format("spcmdrv_log_{0:yyyy-MM-dd}_level{1}.txt", fileDay, LogLevel);
                string archivePath = Path.Combine(path, archiveName);
                if (File.Exists(archivePath)) return; // don't clobber an existing archive

                File.Move(activeFile, archivePath);
                SpcmCard.WriteLogLine(string.Format(
                    "=== log rotated: previous day archived as {0} ===", archiveName));
            }
            catch
            {
                // Best-effort; a rotation failure must not affect the card.
            }
        }

        private static int ReadInt(string name, int fallback)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath))
                {
                    if (key == null) return fallback;
                    object value = key.GetValue(name);
                    return value == null ? fallback : Convert.ToInt32(value);
                }
            }
            catch
            {
                return fallback;
            }
        }

        private static string ReadString(string name, string fallback)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath))
                {
                    if (key == null) return fallback;
                    object value = key.GetValue(name);
                    return value == null ? fallback : value.ToString();
                }
            }
            catch
            {
                return fallback;
            }
        }
    }
}
