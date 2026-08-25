using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SpectrumDDS
{
    /// <summary>Bits of SPC_DDS_STATUS, as a flags enum.</summary>
    [Flags]
    public enum DDSStatus
    {
        Idle = 0,
        WaitingForTrigger = SpcmRegs.SPCM_DDS_STAT_WAITING_FOR_TRG,
        QueueUnderrun = SpcmRegs.SPCM_DDS_STAT_QUEUE_UNDERRUN,
        QueueOverrun = SpcmRegs.SPCM_DDS_STAT_QUEUE_OVERRUN,
    }

    /// <summary>
    /// The Spectrum M4i.9622 DDS card, driven the way the MoleculeMOT experiment
    /// needs: four independent RF outputs stepped through a pattern by one external
    /// trigger per shot.
    /// </summary>
    /// <remarks>
    /// One process owns the card handle -- SpectrumDDSController -- and MOTMaster
    /// talks to it over .NET remoting. This class is where all the hardware
    /// knowledge lives; nothing above it should touch a register directly.
    /// </remarks>
    public class SpectrumDDSDriver : IDisposable
    {
        // -- core to channel routing -------------------------------------------
        //
        // Forced by the hardware, and the single place this mapping is written
        // down. Channel 0 accepts only 47 or 50 cores; channels 1, 2 and 3 accept
        // exactly core 47, 48 and 49 respectively, or nothing. Any other mask is
        // silently *snapped* to a legal one rather than rejected -- writing
        // CORES_ON_CH0 = 1 returns success and reads back 0x7FFFFFFFFFFF -- so
        // every routing write is read back and verified.

        /// <summary>Cores 0 to 46, the only legal multi-core mask for channel 0.</summary>
        public const long Channel0Cores = (1L << 47) - 1;

        public static readonly long[] FourChannelRouting =
        {
            Channel0Cores, 1L << 47, 1L << 48, 1L << 49,
        };

        /// <summary>
        /// Script channel DDS1..DDS4 to DDS core index. DDS1 is core 0 on channel 0;
        /// cores 1 to 46 are summed onto channel 0 as well and must be held at zero
        /// amplitude or they leak into that output.
        /// </summary>
        public static readonly int[] CoreForChannel = { 0, 47, 48, 49 };

        /// <summary>
        /// Comparator level for Trg0 in mV. DDS_Analog_Trg is TTL; half way up a
        /// 3.3 V swing works for either 3.3 V or 5 V logic.
        /// </summary>
        public const int DefaultTriggerLevelMillivolts = 1500;

        public const int DefaultOutputLevelMillivolts = 500;

        /// <summary>
        /// Safety clamp on per-core amplitude a channel starts with, agreed with the
        /// experimenter.
        /// </summary>
        public const double DefaultMaximumAmplitude = 0.25;

        /// <summary>Multi-purpose output lines X0, X1 and X2.</summary>
        public const int XioLineCount = 3;

        private readonly SpcmCard card;
        private DDSPattern armedPattern;
        private int patternsFired;
        private int triggerCountAtArm;
        private bool primed;
        private bool started;

        public SpectrumDDSDriver() : this(new SpcmCard()) { }

        public SpectrumDDSDriver(SpcmCard card)
        {
            this.card = card;
            MaximumAmplitudes = new double[DDSPattern.ChannelCount];
            for (int ch = 0; ch < MaximumAmplitudes.Length; ch++)
                MaximumAmplitudes[ch] = DefaultMaximumAmplitude;
            OutputLevelMillivolts = DefaultOutputLevelMillivolts;
            TriggerLevelMillivolts = DefaultTriggerLevelMillivolts;
            OutputsEnabled = new bool[DDSPattern.ChannelCount];
            EnableXioMarkers = true;
        }

        public SpcmCard Card { get { return card; } }
        public bool IsOpen { get { return card.IsOpen; } }

        public DDSCapabilities Capabilities { get; private set; }
        public DDSCardIdentity Identity { get; private set; }

        /// <summary>
        /// Per-channel amplitude clamp, indexed DDS1 to DDS4. Patterns and manual
        /// tones above it are rejected.
        /// </summary>
        /// <remarks>
        /// One clamp per channel rather than one for the card, because the four AOMs
        /// are not alike: a single number has to be set to the most permissive of
        /// them before a real MOT pattern will load, which leaves the other three
        /// unprotected. Use <see cref="SetMaximumAmplitude"/> to change one.
        /// </remarks>
        public double[] MaximumAmplitudes { get; private set; }

        public int OutputLevelMillivolts { get; private set; }
        public int TriggerLevelMillivolts { get; set; }
        public bool[] OutputsEnabled { get; private set; }

        /// <summary>
        /// Ask for X0, X1 and X2 to be driven from each event's XIO mask. Read at
        /// Open(); changing it later has no effect until the card is reopened.
        /// </summary>
        public bool EnableXioMarkers { get; set; }

        /// <summary>
        /// Whether the card actually accepted routing the DDS markers onto the
        /// multi-purpose pins. False on this card -- see
        /// <see cref="ConfigureXioLines"/> -- in which case a pattern's XIO column
        /// is recorded and stored but drives nothing.
        /// </summary>
        public bool XioMarkersRouted { get; private set; }

        /// <summary>Shots completed since the pattern was loaded.</summary>
        public int PatternsFired { get { return patternsFired; } }

        /// <summary>The pattern currently loaded, or null.</summary>
        public DDSPattern ArmedPattern { get { return armedPattern; } }

        // -- lifecycle -----------------------------------------------------------

        /// <summary>
        /// Open the card, put it in DDS mode, set up routing and the trigger engine,
        /// and prime the command queue.
        /// </summary>
        public void Open()
        {
            // Re-running the setup on an already-started card would fail at the
            // first WRITESETUP with "card is still running, access not possible".
            if (IsOpen) return;

            DebugLogSettings.RotateIfNewDay();
            card.Open();
            SpcmCard.WriteLogLine(string.Format(
                "=== SpectrumDDSDriver.Open: device={0}, PID={1} ===",
                card.Device, Process.GetCurrentProcess().Id));

            Identity = DDSCardIdentity.Read(card);
            if (!Identity.HasDDS50)
                throw new SpcmException(0, string.Format(
                    "{0} does not report the DDS50 firmware option (SPC_PCIEXTFEATURES = 0x{1:X})",
                    Identity.Model, Identity.ExtendedFeatures));

            card.SetInt(SpcmRegs.SPC_CARDMODE, SpcmRegs.SPC_REP_STD_DDS);
            card.SetInt(SpcmRegs.SPC_CHENABLE, 0xF);
            card.SetInt(SpcmRegs.SPC_CLOCKMODE, SpcmRegs.SPC_CM_INTPLL);

            for (int ch = 0; ch < DDSPattern.ChannelCount; ch++)
            {
                card.SetInt(SpcmRegs.EnableOut(ch), 0);
                card.SetInt(SpcmRegs.Amp(ch), OutputLevelMillivolts);
                card.SetInt(SpcmRegs.Filter(ch), 0);
                OutputsEnabled[ch] = false;
            }

            ConfigureCardTrigger();
            ConfigureXioLines();
            card.SetInt(SpcmRegs.SPC_M2CMD, SpcmRegs.M2CMD_CARD_WRITESETUP);

            Capabilities = DDSCapabilities.Read(card);

            // Clear anything a previous session left behind. A still-live
            // TRG_SRC = TIMER drains each block as fast as it is queued, so the
            // card would never park armed. This also zeroes SPC_DDS_TRG_COUNT.
            ResetDDSEngine();
            ApplyRouting();

            card.SetInt(SpcmRegs.SPC_M2CMD,
                SpcmRegs.M2CMD_CARD_START | SpcmRegs.M2CMD_CARD_ENABLETRIGGER);
            started = true;

            Prime();
        }

        public void Close()
        {
            if (!card.IsOpen) return;
            SpcmCard.WriteLogLine("=== SpectrumDDSDriver.Close ===");
            try
            {
                SilenceAllCores();
                card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_EXEC_NOW);
                card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_WRITE_TO_CARD);
                for (int ch = 0; ch < DDSPattern.ChannelCount; ch++)
                    card.SetInt(SpcmRegs.EnableOut(ch), 0);
                card.SetInt(SpcmRegs.SPC_M2CMD, SpcmRegs.M2CMD_CARD_STOP);
            }
            catch (SpcmException)
            {
                // Closing down: a card that is already unhappy should not stop us
                // releasing the handle.
            }
            card.Close();
            primed = false;
            started = false;
        }

        public void Dispose()
        {
            Close();
            card.Dispose();
        }

        /// <summary>
        /// Set up the card's own trigger logic, which is what
        /// SPCM_DDS_TRG_SRC_CARD delegates to.
        /// </summary>
        /// <remarks>
        /// Chapter 11 of the manual says the OR mask defaults to
        /// SPC_TMASK_SOFTWARE. On this card it defaults to SPC_TMASK_NONE, so
        /// without this the DDS engine has no trigger source at all and simply
        /// never advances. SPC_TMASK_SOFTWARE would be worse than useless here: it
        /// fires the instant the card starts and burns straight through the queued
        /// blocks into QUEUE_UNDERRUN. Trg0 is where DDS_Analog_Trg lands.
        /// </remarks>
        public void ConfigureCardTrigger()
        {
            card.SetInt(SpcmRegs.SPC_TRIG_EXT0_MODE, SpcmRegs.SPC_TM_POS);
            card.SetInt(SpcmRegs.SPC_TRIG_EXT0_LEVEL0, TriggerLevelMillivolts);
            card.SetInt(SpcmRegs.SPC_TRIG_TERM, 0);
            card.SetInt(SpcmRegs.SPC_TRIG_ORMASK, SpcmRegs.SPC_TMASK_EXT0);
            card.SetInt(SpcmRegs.SPC_TRIG_ANDMASK, SpcmRegs.SPC_TMASK_NONE);
        }

        /// <summary>Route one core to each of the four outputs, verifying by read-back.</summary>
        public void ApplyRouting()
        {
            for (int ch = 0; ch < DDSPattern.ChannelCount; ch++)
                card.SetLong(SpcmRegs.CoresOnChannel(ch), FourChannelRouting[ch]);

            for (int ch = 0; ch < DDSPattern.ChannelCount; ch++)
            {
                long got = card.GetLong(SpcmRegs.CoresOnChannel(ch));
                if (got != FourChannelRouting[ch])
                    throw new SpcmException(0, string.Format(
                        "the driver snapped the channel {0} routing: asked for 0x{1:X}, got 0x{2:X}",
                        ch, FourChannelRouting[ch], got));
            }
        }

        public void ResetDDSEngine()
        {
            card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_RESET);
            primed = false;
        }

        /// <summary>
        /// Throw away everything still queued on the card, leaving it primed, idle
        /// and ready for a fresh pattern.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The command queue outlives everything else: EXEC_NOW acts on the live
        /// registers and leaves queued EXEC_AT_TRG blocks exactly where they are, and
        /// stopping the shot loop does not unqueue the shot it has already armed.
        /// Whatever is left gets played by the next triggers to arrive, so a run
        /// beginning with stale blocks on the card runs those first and only reaches
        /// its own pattern afterwards -- the DDS output lagging the sequence by
        /// however many shots had piled up.
        /// </para>
        /// <para>
        /// SPCM_DDS_CMD_RESET is the only thing that clears the queue, and it is
        /// issued here the way <see cref="Open"/> issues it -- on a stopped card,
        /// followed by routing and a fresh prime -- because that is the order known
        /// to work. It also zeroes SPC_DDS_TRG_COUNT, which costs nothing: every
        /// count this driver acts on is a difference from a baseline taken at arming
        /// time.
        /// </para>
        /// <para>
        /// <b>It takes the RF down with it.</b> The card has to be stopped for the
        /// reset, and the prime that follows forces a silenced block live, so the
        /// four outputs end up at zero. That is why this belongs only where the
        /// output is about to be redefined anyway -- loading a new pattern, or
        /// applying a manual tone -- and not at the end of a run, where the channels
        /// are meant to hold the last event's state.
        /// </para>
        /// </remarks>
        public void DiscardQueuedShots()
        {
            if (!started) return;

            card.SetInt(SpcmRegs.SPC_M2CMD, SpcmRegs.M2CMD_CARD_STOP);
            started = false;
            primed = false;

            ConfigureCardTrigger();
            card.SetInt(SpcmRegs.SPC_M2CMD, SpcmRegs.M2CMD_CARD_WRITESETUP);
            ResetDDSEngine();
            ApplyRouting();

            card.SetInt(SpcmRegs.SPC_M2CMD,
                SpcmRegs.M2CMD_CARD_START | SpcmRegs.M2CMD_CARD_ENABLETRIGGER);
            started = true;

            Prime();
        }

        /// <summary>
        /// Flush one EXEC_AT_TRG block and force it live, leaving the queue and the
        /// shadow registers empty.
        /// </summary>
        /// <remarks>
        /// Undocumented precondition: SPCM_DDS_CMD_EXEC_NOW is rejected with error
        /// 267, "the setup isn't valid", until at least one EXEC_AT_TRG block has
        /// been written to the card after M2CMD_CARD_START. So the manual's own
        /// "simple example for fixed frequency output" cannot work as the first
        /// thing you do, and the GUI's manual-control tab depends on this having
        /// run. Forcing is not needed to satisfy the precondition, but it is needed
        /// to clear the shadow registers -- see DDSPatternCompiler.
        /// </remarks>
        public void Prime()
        {
            SilenceAllCores();
            card.SetInt(SpcmRegs.SPC_DDS_TRG_SRC, SpcmRegs.SPCM_DDS_TRG_SRC_CARD);
            card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_EXEC_AT_TRG);
            card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_WRITE_TO_CARD);
            card.SetInt(SpcmRegs.SPC_M2CMD, SpcmRegs.M2CMD_CARD_FORCETRIGGER);
            primed = true;
        }

        /// <summary>
        /// Queue a zero amplitude and zero slopes for every core.
        /// </summary>
        /// <remarks>
        /// All 50 cores, not just the four in use: cores 1 to 46 are summed onto
        /// channel 0 along with core 0, so anything left over from a previous run
        /// leaks into that output.
        /// </remarks>
        public void SilenceAllCores()
        {
            int cores = Capabilities != null ? Capabilities.CoreCount
                                             : card.GetInt(SpcmRegs.SPC_DDS_NUM_CORES);
            for (int core = 0; core < cores; core++)
            {
                card.SetDouble(SpcmRegs.CoreAmp(core), 0.0);
                card.SetDouble(SpcmRegs.CoreAmpSlope(core), 0.0);
                card.SetDouble(SpcmRegs.CoreFreqSlope(core), 0.0);
            }
        }

        // -- output stage ---------------------------------------------------------

        /// <summary>Set one channel's amplitude clamp.</summary>
        public void SetMaximumAmplitude(int channel, double maximum)
        {
            if (channel < 0 || channel >= DDSPattern.ChannelCount)
                throw new ArgumentOutOfRangeException("channel");
            if (maximum < 0.0 || maximum > 1.0 || double.IsNaN(maximum))
                throw new ArgumentOutOfRangeException("maximum",
                    "an amplitude clamp has to be between 0 and 1");
            MaximumAmplitudes[channel] = maximum;
        }

        public void SetOutputEnabled(int channel, bool enabled)
        {
            card.SetInt(SpcmRegs.EnableOut(channel), enabled ? 1 : 0);
            WriteSetupIfStopped();
            OutputsEnabled[channel] = enabled;
        }

        public void SetOutputLevel(int millivolts)
        {
            for (int ch = 0; ch < DDSPattern.ChannelCount; ch++)
                card.SetInt(SpcmRegs.Amp(ch), millivolts);
            WriteSetupIfStopped();
            OutputLevelMillivolts = millivolts;
        }

        /// <summary>
        /// Push pending setup registers to the card, but only while it is stopped.
        /// </summary>
        /// <remarks>
        /// Chapter 9: "After card has been started, only some of the settings might
        /// be changed while the card is running, such as e.g. output level and
        /// offset for D/A replay cards." The output stage is one of those, but
        /// M2CMD_CARD_WRITESETUP itself is rejected once the card is running, with
        /// error 288 "card is still running, access not possible" -- so the register
        /// write alone is both sufficient and the only thing allowed.
        /// </remarks>
        private void WriteSetupIfStopped()
        {
            if (!started) card.SetInt(SpcmRegs.SPC_M2CMD, SpcmRegs.M2CMD_CARD_WRITESETUP);
        }

        /// <summary>
        /// Try to put the three multi-purpose lines into DDS manual-output mode, so
        /// that the per-event XIO mask a pattern carries reaches a pin.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Must happen before M2CMD_CARD_START: unlike the output stage, the X-line
        /// mode is not one of the registers the card accepts while running.
        /// </para>
        /// <para>
        /// <b>This card will not do it.</b> Chapter 17's recipe is
        /// <c>SPCM_X0_MODE = SPCM_XMODE_DDS</c> followed by
        /// <c>SPC_DDS_X0_MODE = SPCM_DDS_XMODE_MANUAL</c>. The second half is
        /// accepted, but the first is rejected with error 257 "value not allowed":
        /// <c>SPCM_XMODE_DDS</c> (0x4, which the driver also calls
        /// <c>SPCM_XMODE_DIGIN</c>) is absent from this card's
        /// <c>SPCM_X0_AVAILMODES</c> of 0xF70732B. The available output modes are
        /// ASYNCOUT, TRIGOUT, RUNSTATE, ARMSTATE, CONTOUTMARK, REFCLKOUT and
        /// SYSCLKOUT -- none of them per-event.
        /// </para>
        /// <para>
        /// So the routing is attempted and the outcome recorded in
        /// <see cref="XioMarkersRouted"/> rather than being allowed to fail the
        /// whole Open(). The DDS-side register is set either way: it is accepted,
        /// harmless, and correct if a future firmware makes the routing available.
        /// </para>
        /// </remarks>
        private void ConfigureXioLines()
        {
            XioMarkersRouted = false;
            if (!EnableXioMarkers) return;

            int routed = 0;
            for (int line = 0; line < XioLineCount; line++)
            {
                try
                {
                    card.SetInt(SpcmRegs.XMode(line), SpcmRegs.SPCM_XMODE_DDS);
                    routed++;
                }
                catch (SpcmException)
                {
                    // Not available on this card; leave the line as it was.
                }
                card.SetInt(SpcmRegs.DDSXMode(line), SpcmRegs.SPCM_DDS_XMODE_MANUAL);
            }
            XioMarkersRouted = routed == XioLineCount;
        }

        // -- manual control -------------------------------------------------------

        /// <summary>
        /// Apply a single tone to one channel immediately, outside any pattern.
        /// </summary>
        /// <remarks>
        /// This discards whatever pattern was armed -- EXEC_NOW executes against the
        /// live registers, so mixing it with a queued pattern gives neither.
        /// </remarks>
        public void SetToneNow(int channel, double frequencyHz, double amplitude)
        {
            RequirePrimed();
            ClampCheck(amplitude, channel);
            DiscardQueuedShots();

            int core = CoreForChannel[channel];
            card.SetDouble(SpcmRegs.CoreFreq(core), frequencyHz);
            card.SetDouble(SpcmRegs.CoreAmp(core), amplitude);
            card.SetDouble(SpcmRegs.CoreFreqSlope(core), 0.0);
            card.SetDouble(SpcmRegs.CoreAmpSlope(core), 0.0);
            card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_EXEC_NOW);
            card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_WRITE_TO_CARD);

            armedPattern = null;
        }

        /// <summary>
        /// Silence every core immediately, dropping any shot still armed.
        /// </summary>
        /// <remarks>
        /// The discard is not incidental: this is what a new pattern load goes
        /// through, and it is the only point in a run where the card's queue is
        /// emptied. See <see cref="DiscardQueuedShots"/>.
        /// </remarks>
        public void SilenceNow()
        {
            RequirePrimed();
            DiscardQueuedShots();
            SilenceAllCores();
            card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_EXEC_NOW);
            card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_WRITE_TO_CARD);
            armedPattern = null;
        }

        // -- patterns ---------------------------------------------------------------

        /// <summary>
        /// Validate a pattern and make it the one this driver will arm for each shot.
        /// </summary>
        public void LoadPattern(DDSPattern pattern)
        {
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (Capabilities == null)
                throw new InvalidOperationException(
                    "the card is not open, so the pattern cannot be checked against its limits");

            List<string> problems = pattern.Validate(Capabilities, MaximumAmplitudes);
            if (problems.Count > 0)
                throw new ArgumentException("the DDS pattern was rejected:" +
                    Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", problems.ToArray()));

            armedPattern = pattern;
            patternsFired = 0;
            SpcmCard.WriteLogLine(string.Format(
                "=== SpectrumDDSDriver.LoadPattern: {0} events ===", pattern.Count));
        }

        /// <summary>
        /// Queue the loaded pattern and flush it, so the card is waiting for the
        /// next external trigger.
        /// </summary>
        /// <remarks>
        /// Only call this when <see cref="ShotPending"/> is false. Nothing on the
        /// card stops a second pattern being queued behind the first, and the extra
        /// copy is not overwritten but played first, so every needless call puts the
        /// output one whole shot further behind the sequence.
        /// </remarks>
        public void ArmForNextShot()
        {
            if (armedPattern == null)
                throw new InvalidOperationException("no pattern has been loaded");
            RequirePrimed();

            DDSPatternCompiler.Queue(card, armedPattern, CoreForChannel);
            card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_WRITE_TO_CARD);

            // Baseline for WaitForShot, taken here rather than there because the
            // card is quiescent at this point. Sampling it after the trigger races
            // the counter's one-trigger lag and loses a count about as often as not.
            triggerCountAtArm = TriggerCount;
        }

        /// <summary>
        /// Wait until a flushed command block is parked waiting for its trigger.
        /// </summary>
        /// <remarks>
        /// Only meaningful straight after <see cref="ArmForNextShot"/>:
        /// SPCM_DDS_STAT_WAITING_FOR_TRG tracks the command queue, not the trigger
        /// engine, so with an empty queue it reads idle even though the trigger
        /// logic is armed and listening. It also glitches low for an instant at
        /// every trigger boundary, which is why it must never be used to decide
        /// that a shot has finished -- see <see cref="WaitForShot"/>.
        /// </remarks>
        public bool WaitUntilArmed(double timeoutSeconds)
        {
            Stopwatch clock = Stopwatch.StartNew();
            while (clock.Elapsed.TotalSeconds < timeoutSeconds)
            {
                if ((Status & DDSStatus.WaitingForTrigger) != 0) return true;
                // Sleep(1), not Sleep(0). An arm takes hundreds of milliseconds, so
                // millisecond resolution is ample, and a Sleep(0) spin here reads the
                // status register ~90000 times a second -- which with the driver's
                // debug log at level 3 is 90000 log writes a second, all of them
                // contending with the arm this is waiting for.
                System.Threading.Thread.Sleep(1);
            }
            return false;
        }

        /// <summary>
        /// Wait for the shot in progress to finish, counting it if it does.
        /// Call after <see cref="ArmForNextShot"/> and the trigger.
        /// </summary>
        /// <remarks>
        /// The only reliable end-of-shot test is SPC_DDS_TRG_COUNT advancing by the
        /// number of events, measured from the baseline taken at arming time:
        /// QUEUE_CMD_COUNT reaches zero one event early, because the card always
        /// holds one block in its shadow registers, and WAITING_FOR_TRG glitches low
        /// at every trigger boundary.
        /// </remarks>
        /// <param name="abort">
        /// Polled while waiting; return true to give up early. A run can sit here
        /// for the whole timeout waiting for a trigger that never comes, so without
        /// this a caller asking the shot loop to stop would block for seconds — long
        /// enough for a stop-then-start to leave two threads arming the same card.
        /// </param>
        public bool WaitForShot(double timeoutSeconds, Func<bool> abort = null)
        {
            if (armedPattern == null)
                throw new InvalidOperationException("no pattern has been loaded");

            int wanted = armedPattern.Count;
            Stopwatch clock = Stopwatch.StartNew();
            while (clock.Elapsed.TotalSeconds < timeoutSeconds)
            {
                if (TriggerCount - triggerCountAtArm >= wanted)
                {
                    patternsFired++;
                    return true;
                }
                if (abort != null && abort()) return false;
                // A shot lasts hundreds of milliseconds; polling harder than this
                // buys nothing and would spin a core flat for the whole run.
                System.Threading.Thread.Sleep(1);
            }
            return false;
        }

        /// <summary>Force a trigger in software, standing in for the digital pulse.</summary>
        /// <remarks>
        /// Works repeatedly in DDS mode, advancing the engine exactly one step per
        /// call, so a whole run can be exercised on the bench without touching the
        /// NI pattern generators. Bench use only -- in the experiment the trigger
        /// comes from DDS_Analog_Trg.
        /// </remarks>
        public void ForceTrigger()
        {
            card.SetInt(SpcmRegs.SPC_M2CMD, SpcmRegs.M2CMD_CARD_FORCETRIGGER);
        }

        // -- status -------------------------------------------------------------------

        public DDSStatus Status
        {
            get { return (DDSStatus)card.GetInt(SpcmRegs.SPC_DDS_STATUS); }
        }

        /// <summary>
        /// Triggers the card has processed. Reads one lower than the true count
        /// after a reset (the first trigger is not counted) and never catches up,
        /// so compare differences, never absolute values.
        /// </summary>
        public int TriggerCount
        {
            get { return card.GetInt(SpcmRegs.SPC_DDS_TRG_COUNT); }
        }

        /// <summary>
        /// Commands still queued on the card. Excludes the block held in the shadow
        /// registers, so it reaches zero one event before a pattern ends.
        /// </summary>
        public int QueuedCommandCount
        {
            get { return card.GetInt(SpcmRegs.SPC_DDS_QUEUE_CMD_COUNT); }
        }

        /// <summary>
        /// Whether an armed shot is still waiting to be run.
        /// </summary>
        /// <remarks>
        /// A pattern's last block ends up in the shadow registers, which
        /// QUEUE_CMD_COUNT excludes, so this goes false as the shot's final event
        /// goes live and stays false until the next pattern is queued. That is the
        /// distinction wanted here -- "is there still a shot owing" -- and not the
        /// same question as <see cref="WaitForShot"/>'s, which is "has the shot
        /// finished"; the count reaching zero one event early is exactly why that one
        /// has to go by SPC_DDS_TRG_COUNT instead.
        /// </remarks>
        public bool ShotPending
        {
            get { return QueuedCommandCount > 0; }
        }

        public string StatusText()
        {
            DDSStatus s = Status;
            if (s == DDSStatus.Idle) return "idle";
            List<string> flags = new List<string>();
            if ((s & DDSStatus.WaitingForTrigger) != 0) flags.Add("waiting for trigger");
            if ((s & DDSStatus.QueueUnderrun) != 0) flags.Add("QUEUE UNDERRUN");
            if ((s & DDSStatus.QueueOverrun) != 0) flags.Add("QUEUE OVERRUN");
            return string.Join(" | ", flags.ToArray());
        }

        // -- helpers -------------------------------------------------------------------

        private void RequirePrimed()
        {
            if (!primed)
                throw new InvalidOperationException(
                    "the card has not been primed; call Open() first");
        }

        private void ClampCheck(double amplitude, int channel)
        {
            if (Math.Abs(amplitude) > MaximumAmplitudes[channel])
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "DDS{0} amplitude {1:g4} exceeds its {2:g4} safety clamp",
                    channel + 1, amplitude, MaximumAmplitudes[channel]));
        }
    }
}
