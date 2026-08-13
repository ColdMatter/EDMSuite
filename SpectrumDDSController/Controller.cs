using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using SpectrumDDS;

namespace SpectrumDDSController
{
    /// <summary>
    /// The remoting face of the DDS controller: what MOTMaster is allowed to ask
    /// for, plus what the GUI needs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This process owns the single card handle and publishes this object on TCP
    /// 1818 as <c>controller.rem</c>. MOTMaster holds a proxy under
    /// <c>#if DDS</c>. Running standalone is deliberate: manual control has to work
    /// with MOTMaster closed. (Port map for the suite: 1172 hardware control, 1187
    /// MOTMaster, 1818 here.)
    /// </para>
    /// <para>
    /// The wire format stays the dictionary the ~130 MOTMaster scripts already
    /// produce, so no script has to change.
    /// </para>
    /// </remarks>
    public class Controller : MarshalByRefObject
    {
        private readonly SpectrumDDSDriver driver = new SpectrumDDSDriver();
        private readonly object cardLock = new object();

        private Thread shotThread;
        private volatile bool running;
        private Dictionary<string, List<List<double>>> pattern;

        /// <summary>End-of-run tallies, newest last. See <see cref="ReportRunTally"/>.</summary>
        private readonly Queue<string> runTallies = new Queue<string>();
        private readonly object tallyLock = new object();

        /// <summary>How many runs the Status tab remembers.</summary>
        public const int RunTalliesKept = 10;

        public SpectrumDDSDriver Driver { get { return driver; } }
        public object CardLock { get { return cardLock; } }

        /// <summary>A remoted object with no lease never gets collected under us.</summary>
        public override object InitializeLifetimeService() { return null; }

        // -- lifecycle -------------------------------------------------------------

        public void OpenCard()
        {
            lock (cardLock) driver.Open();
        }

        public void CloseCard()
        {
            StopRepetitivePattern();
            lock (cardLock) driver.Close();
        }

        public bool IsOpen { get { return driver.IsOpen; } }

        // -- the surface MOTMaster uses ---------------------------------------------

        /// <summary>
        /// Stop whatever is running and drop the loaded pattern, ready for a new one.
        /// </summary>
        public void PrepareForNewPattern()
        {
            StopRepetitivePattern();
            lock (cardLock)
            {
                // The shot the last run left armed is still on the card -- SilenceNow
                // discards it, which is what stops a new pattern being queued behind
                // an old one and played a shot late.
                if (driver.IsOpen) driver.SilenceNow();
            }
            pattern = null;
        }

        /// <summary>
        /// The pattern in the MOTMaster script format: event label to five lists of
        /// [time ms], [f MHz x4], [amplitude x4], [df MHz/ms x4], [da /ms x4].
        /// </summary>
        public Dictionary<string, List<List<double>>> patternList
        {
            get { return pattern; }
            set
            {
                pattern = value;
                DDSPattern compiled = DDSPattern.FromLegacyDictionary(value);
                lock (cardLock) driver.LoadPattern(compiled);
            }
        }

        /// <summary>The loaded pattern in the driver's own form, for the GUI.</summary>
        public DDSPattern LoadedPattern { get { return driver.ArmedPattern; } }

        /// <summary>
        /// Start arming the loaded pattern once per shot, so every external trigger
        /// on DDS_Analog_Trg runs it once.
        /// </summary>
        public void StartRepetitivePattern()
        {
            if (driver.ArmedPattern == null)
                throw new InvalidOperationException("no DDS pattern has been loaded");
            StopRepetitivePattern();

            running = true;
            shotThread = new Thread(RunShots) { IsBackground = true, Name = "DDS shots" };
            shotThread.Start();
        }

        /// <summary>
        /// Stop arming shots, leaving the outputs wherever the last event put them.
        /// </summary>
        /// <remarks>
        /// The card is deliberately left as it is: the four channels hold the last
        /// event's frequency and amplitude at the end of a run, which is what the
        /// experiment wants, and clearing the queue would mean stopping the card and
        /// dropping the RF. The shot the loop had already armed therefore stays
        /// queued -- harmless while nothing is pulsing DDS_Analog_Trg, and thrown
        /// away by <see cref="PrepareForNewPattern"/> before anything else is queued
        /// behind it.
        /// </remarks>
        public void StopRepetitivePattern()
        {
            running = false;
            Thread thread = shotThread;
            if (thread == null) return;

            // Only forget the thread once it has actually gone. Dropping the handle
            // on a thread that is still arming the card would let the next
            // StartRepetitivePattern put a second one alongside it.
            if (!thread.Join(5000))
                throw new TimeoutException(
                    "the DDS shot thread did not stop; the card may be mid-transfer");
            shotThread = null;
        }

        public bool IsRunning { get { return running; } }

        /// <summary>
        /// Block until the card is armed and waiting for its trigger, so MOTMaster
        /// does not fire a shot into the re-queue deadtime.
        /// </summary>
        /// <remarks>
        /// Deliberately outside <see cref="cardLock"/>. The thing this waits for is
        /// the shot thread's <see cref="SpectrumDDSDriver.ArmForNextShot"/>, which
        /// needs that lock -- so holding it here blocks the arm and then times out
        /// waiting for it, and MOTMaster fires into an unarmed card and takes a
        /// QUEUE_UNDERRUN. Worse, an arm that is slow for its own reasons (debug
        /// logging at level 3 stalling on the log file) blocks this call for as long
        /// as the arm lasts, whatever the timeout says. Polling the status register
        /// needs no lock beyond SpcmCard's own per-call one.
        /// </remarks>
        public bool WaitUntilArmed(double timeoutSeconds)
        {
            try
            {
                return driver.WaitUntilArmed(timeoutSeconds);
            }
            catch (InvalidOperationException ex)
            {
                DdsLog.Error("Controller.WaitUntilArmed", ex);
                // The card was closed from the GUI while we were polling -- which
                // cardLock used to make impossible. "Not armed" is the honest answer
                // and the caller already handles it; throwing across remoting into
                // MOTMaster's run loop would not be.
                return false;
            }
        }

        public int PatternsFired { get { return driver.PatternsFired; } }

        /// <summary>
        /// Triggers the card has counted. Reads one low after a reset and never
        /// catches up, so it is only meaningful as a difference -- MOTMaster prints
        /// it beside its own sent count to spot missed shots.
        /// </summary>
        public int GetCardTriggerCount()
        {
            lock (cardLock) return driver.TriggerCount;
        }

        public string GetStatusText()
        {
            lock (cardLock) return driver.IsOpen ? driver.StatusText() : "card not open";
        }

        // -- the end-of-run tally ------------------------------------------------------

        /// <summary>
        /// Record how a MOTMaster run went, for the Status tab to show.
        /// </summary>
        /// <param name="triggersSent">
        /// Triggers MOTMaster fired. The one number this side cannot know -- everything
        /// else is sampled here, at the same moment.
        /// </param>
        /// <remarks>
        /// <para>
        /// MOTMaster prints these numbers to its console as well, but that goes nowhere
        /// anyone sees unless it was started from Visual Studio, which it rarely is. So
        /// this window is where they actually get read, and it is the only readout of
        /// whether the DDS kept up with the sequence.
        /// </para>
        /// <para>
        /// Stored rather than pushed at the GUI: the window picks it up on its status
        /// timer, the same way it picks up a pattern MOTMaster pushed straight into
        /// <see cref="patternList"/>. That keeps the update on the UI thread without a
        /// remoting thread having to marshal onto it.
        /// </para>
        /// <para>
        /// The end-of-run status is the useful half. Every run starts with
        /// <see cref="PrepareForNewPattern"/>, which resets the DDS engine and so clears
        /// the latched QUEUE_UNDERRUN bit -- so finding it set here means an underrun
        /// happened during <em>this</em> run. A run that missed shots with an underrun
        /// was fired into during the re-queue deadtime; one that missed them without was
        /// simply too slow to arm, which points at the driver's debug log level.
        /// </para>
        /// </remarks>
        public void ReportRunTally(int triggersSent)
        {
            int fired = PatternsFired;
            int missed = triggersSent - fired;

            // The card-derived half is best-effort. Closing the card between the run
            // ending and this call would make GetCardTriggerCount throw, and losing the
            // whole tally -- including the sent and fired counts, which are still
            // perfectly good -- over that would defeat the point of recording it.
            string cardTriggers, status;
            try
            {
                cardTriggers = GetCardTriggerCount().ToString(CultureInfo.InvariantCulture);
                status = GetStatusText();
            }
            catch (Exception ex)
            {
                cardTriggers = "?";
                status = "could not be read: " + ex.Message;
            }

            string entry = string.Format(CultureInfo.InvariantCulture,
                "{0:HH:mm:ss}  sent {1,5}  fired {2,5}  missed {3,4}  card triggers {4,7}  |  {5}{6}",
                DateTime.Now, triggersSent, fired, missed, cardTriggers, status,
                missed > 0 ? "   <<< MISSED " + missed : "");

            // The in-memory queue below is capped and dies with the process, and a
            // run that missed shots is exactly the thing worth still having a record
            // of tomorrow. Same string, so the file and the Status tab agree.
            DdsLog.Write("Controller", entry);

            lock (tallyLock)
            {
                runTallies.Enqueue(entry);
                while (runTallies.Count > RunTalliesKept) runTallies.Dequeue();
            }
        }

        /// <summary>
        /// The remembered run tallies, newest first, one per line. Empty if no run has
        /// reported since this controller started.
        /// </summary>
        public string RunTallyText
        {
            get
            {
                string[] entries;
                lock (tallyLock) entries = runTallies.ToArray();
                Array.Reverse(entries);
                return string.Join(Environment.NewLine, entries);
            }
        }

        /// <summary>
        /// Fire one DDS trigger in software, standing in for the pulse on
        /// DDS_Analog_Trg.
        /// </summary>
        /// <remarks>
        /// For checking a pattern on a scope without the experiment running. It
        /// reaches the Spectrum card and nothing else -- it drives no NI board and
        /// starts no sequence.
        /// </remarks>
        public void ForceTrigger()
        {
            lock (cardLock) driver.ForceTrigger();
        }

        // -- the shot loop ------------------------------------------------------------

        private void RunShots()
        {
            double timeout = driver.ArmedPattern.Span + 5.0;

            // A shot left armed by an earlier run of this loop -- stop then run
            // again from the GUI -- is still on the card and is still a shot of the
            // pattern now loaded, since loading one goes through
            // PrepareForNewPattern and that clears the queue. Adopt it. Queueing a
            // second copy behind it would leave the output a whole shot behind the
            // sequence for as long as the loop ran.
            bool armed;
            lock (cardLock) armed = driver.ShotPending;

            while (running)
            {
                if (!armed)
                {
                    try
                    {
                        lock (cardLock) driver.ArmForNextShot();
                    }
                    catch (Exception ex)
                    {
                        // The card has gone away or been stopped from the GUI. Give
                        // up quietly rather than throwing on a background thread.
                        // Quietly on screen, that is -- the shot loop dying mid-run
                        // used to leave no trace at all.
                        DdsLog.Error("Controller.RunShots", ex);
                        running = false;
                        break;
                    }
                    armed = true;
                }

                // Wait for the trigger and the pattern to run. A timeout here just
                // means no trigger has arrived yet, which is the normal state while
                // the experiment is idle -- the shot stays armed on the card and we
                // go round to keep waiting for it. Re-arming instead would queue a
                // second copy of the pattern behind the first, since nothing on the
                // card replaces a queued shot, and the output would run a whole
                // pattern behind the sequence for every idle timeout that elapsed.
                // Polling `running` is what lets StopRepetitivePattern return
                // promptly instead of after the timeout.
                if (driver.WaitForShot(timeout, () => !running)) armed = false;
                if (!running) break;
            }
        }
    }
}
