using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
    /// <summary>
    /// Lets several programs on the same PC (e.g. TransferCavityLock2012 and WavemeterLock) share the
    /// analog output channels of a DAQ card without crashing each other.
    ///
    /// Two separate mechanisms:
    ///  - Device write lock: a named mutex per DAQ device, held only for the duration of a single
    ///    Start/Write/Stop. Stops two processes reserving the card's AO resources at the same moment
    ///    (DAQmx error -50103 "resource is reserved").
    ///  - Channel claim: a per-physical-channel lock file, held open by whichever program is currently
    ///    locking that laser. The other program then refuses to lock or write to that channel. Windows
    ///    closes the file automatically if the owning program exits or crashes, so a claim is never stuck.
    ///
    /// If only one program ever uses a card, every lock is always free and behaviour is unchanged.
    /// </summary>
    public static class SharedAnalogOutput
    {
        private const int deviceLockTimeoutMs = 2000;
        private const int reservedRetries = 5;
        private const int reservedRetryDelayMs = 5;

        private static readonly object claimsLock = new object();
        private static readonly Dictionary<string, FileStream> heldClaims = new Dictionary<string, FileStream>();
        private static readonly Dictionary<string, Mutex> deviceMutexes = new Dictionary<string, Mutex>();

        private static string ownerDescription
        {
            get
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
                return p.ProcessName + " (pid " + p.Id + ")";
            }
        }

        #region Device write lock

        /// <summary>
        /// Writes a single on-demand sample, holding the device lock for the whole Start/Write/Stop so that
        /// another process can't reserve the card in between. Retries briefly if the card is reserved by
        /// something that doesn't use this lock (e.g. another program). Returns false if the write failed.
        /// </summary>
        public static bool WriteSingleSample(Task task, AnalogSingleChannelWriter writer, AnalogOutputChannel channel, double voltage)
        {
            Mutex mutex = getDeviceMutex(channel.Device);
            bool haveMutex = false;
            try
            {
                try
                {
                    haveMutex = mutex.WaitOne(deviceLockTimeoutMs);
                }
                catch (AbandonedMutexException)
                {
                    // Previous holder exited without releasing; we now own it.
                    haveMutex = true;
                }

                for (int attempt = 0; ; attempt++)
                {
                    try
                    {
                        task.Start();
                        try
                        {
                            writer.WriteSingleSample(true, voltage);
                        }
                        finally
                        {
                            task.Stop();
                        }
                        return true;
                    }
                    catch (DaqException e)
                    {
                        if (isResourceReserved(e) && attempt < reservedRetries)
                        {
                            Thread.Sleep(reservedRetryDelayMs);
                            continue;
                        }
                        Console.WriteLine("SharedAnalogOutput: failed to write " + voltage + " V to " + channel.Name
                            + " (" + channel.PhysicalChannel + "): " + e.Message);
                        return false;
                    }
                }
            }
            finally
            {
                if (haveMutex) mutex.ReleaseMutex();
            }
        }

        private static bool isResourceReserved(DaqException e)
        {
            // -50103: The specified resource is reserved. -50201: Task cannot start because resources are in use.
            return e.Error == -50103 || e.Error == -50201;
        }

        private static Mutex getDeviceMutex(string device)
        {
            lock (claimsLock)
            {
                Mutex m;
                if (!deviceMutexes.TryGetValue(device, out m))
                {
                    m = new Mutex(false, "Local\\EDMSuite_AO_" + sanitise(device));
                    deviceMutexes[device] = m;
                }
                return m;
            }
        }

        #endregion

        #region Channel claims

        /// <summary>
        /// Claims a physical channel for this process. Returns true if this process now holds the claim
        /// (including if it already did). Returns false, with the current owner, if another process holds it.
        /// </summary>
        public static bool TryClaim(AnalogOutputChannel channel, out string currentOwner)
        {
            currentOwner = null;
            string key = channel.PhysicalChannel;
            lock (claimsLock)
            {
                if (heldClaims.ContainsKey(key)) return true;
                FileStream fs;
                try
                {
                    fs = new FileStream(claimFilePath(key), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                }
                catch (IOException)
                {
                    currentOwner = readOwner(key);
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    currentOwner = readOwner(key);
                    return false;
                }
                byte[] owner = Encoding.UTF8.GetBytes(ownerDescription + " - " + channel.Name);
                fs.SetLength(0);
                fs.Write(owner, 0, owner.Length);
                fs.Flush();
                heldClaims[key] = fs;
                return true;
            }
        }

        /// <summary>Releases this process's claim on a channel. Does nothing if it doesn't hold one.</summary>
        public static void Release(AnalogOutputChannel channel)
        {
            string key = channel.PhysicalChannel;
            lock (claimsLock)
            {
                FileStream fs;
                if (heldClaims.TryGetValue(key, out fs))
                {
                    heldClaims.Remove(key);
                    fs.Dispose();
                }
            }
        }

        /// <summary>True if this process currently holds the claim on the channel.</summary>
        public static bool IsClaimedByThisProcess(AnalogOutputChannel channel)
        {
            lock (claimsLock)
            {
                return heldClaims.ContainsKey(channel.PhysicalChannel);
            }
        }

        /// <summary>True (with the owner) if a different process currently holds the claim on the channel.</summary>
        public static bool IsClaimedByOther(AnalogOutputChannel channel, out string currentOwner)
        {
            currentOwner = null;
            if (IsClaimedByThisProcess(channel)) return false;
            string owner;
            if (TryClaim(channel, out owner))
            {
                Release(channel);
                return false;
            }
            currentOwner = owner;
            return true;
        }

        private static string readOwner(string key)
        {
            try
            {
                using (FileStream fs = new FileStream(claimFilePath(key), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                {
                    string s = reader.ReadToEnd();
                    return string.IsNullOrWhiteSpace(s) ? "another program" : s;
                }
            }
            catch (Exception)
            {
                return "another program";
            }
        }

        private static string claimFilePath(string key)
        {
            string dir = Path.Combine(Path.GetTempPath(), "EDMSuite_AOClaims");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, sanitise(key) + ".lock");
        }

        #endregion

        private static string sanitise(string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s.Trim('/'))
            {
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }
            return sb.ToString();
        }
    }
}
