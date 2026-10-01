using System;
using System.Collections.Generic;

namespace SpectrumDDS
{
    /// <summary>
    /// Turns a <see cref="DDSPattern"/> into the sequence of register writes the
    /// card's command queue expects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The timing model this encodes was measured on the card, not taken from the
    /// manual. Parameters in a command block become <em>live</em> at the trigger
    /// that ends the block, and the engine then waits for the next trigger using
    /// those now-live registers. So <b>block k must carry the trigger source and
    /// timer that govern the wait for event k+1</b>, not its own arrival time.
    /// A prologue timer of 0.6 s followed by blocks carrying 0.2 / 0.4 / 0.8 s
    /// produced intervals of 0.594 / 0.200 / 0.400 / 0.800 s; the alternative
    /// model predicts 0.2 / 0.4 / 0.8 and is ruled out by 400 ms.
    /// </para>
    /// <para>
    /// The scripts pulse DDS_Analog_Trg once per shot at t = 0, so the external
    /// trigger starts the pattern and the card's internal timer steps through the
    /// rest of it. The last block hands the trigger source back to the card input
    /// so the next shot starts on the next digital pulse.
    /// </para>
    /// </remarks>
    public static class DDSPatternCompiler
    {
        /// <summary>
        /// Queue <paramref name="pattern"/> onto the card. Does <b>not</b> flush --
        /// the caller issues WRITE_TO_CARD, so that a whole pattern reaches the card
        /// as one transfer.
        /// </summary>
        /// <remarks>
        /// The card must already be started and primed (see
        /// <see cref="SpectrumDDSDriver.Prime"/>). Priming matters for ordering as
        /// well as for the EXEC_NOW precondition: the card always holds one block in
        /// its shadow registers, so if the prologue block were still sitting there
        /// the external trigger at t = 0 would execute the prologue instead of
        /// event 0 and the whole pattern would slip by one step.
        /// </remarks>
        /// <param name="rearmOnCardTrigger">
        /// When true the final block sets the trigger source back to the card input,
        /// so the pattern re-arms for the next shot. Set false for a one-shot run
        /// that should simply stop at the last event.
        /// </param>
        public static void Queue(SpcmCard card, DDSPattern pattern,
                                 IList<int> coreForChannel, bool rearmOnCardTrigger = true)
        {
            if (card == null) throw new ArgumentNullException("card");
            if (pattern == null) throw new ArgumentNullException("pattern");
            if (pattern.Count == 0) throw new ArgumentException("the pattern is empty", "pattern");

            for (int k = 0; k < pattern.Count; k++)
            {
                DDSEvent e = pattern[k];

                for (int ch = 0; ch < DDSPattern.ChannelCount; ch++)
                {
                    int core = coreForChannel[ch];
                    DDSChannelState c = e.Channels[ch];
                    card.SetDouble(SpcmRegs.CoreFreq(core), c.Frequency);
                    card.SetDouble(SpcmRegs.CoreAmp(core), c.Amplitude);
                    card.SetDouble(SpcmRegs.CoreFreqSlope(core), c.FrequencySlope);
                    card.SetDouble(SpcmRegs.CoreAmpSlope(core), c.AmplitudeSlope);
                }

                card.SetInt(SpcmRegs.SPC_DDS_X_MANUAL_OUTPUT, e.Xio);

                if (k + 1 < pattern.Count)
                {
                    // This block governs the wait for event k+1.
                    double gap = pattern[k + 1].Time - e.Time;
                    card.SetInt(SpcmRegs.SPC_DDS_TRG_SRC, SpcmRegs.SPCM_DDS_TRG_SRC_TIMER);
                    card.SetDouble(SpcmRegs.SPC_DDS_TRG_TIMER, gap);
                }
                else
                {
                    card.SetInt(SpcmRegs.SPC_DDS_TRG_SRC, rearmOnCardTrigger
                        ? SpcmRegs.SPCM_DDS_TRG_SRC_CARD
                        : SpcmRegs.SPCM_DDS_TRG_SRC_NONE);
                }

                card.SetInt(SpcmRegs.SPC_DDS_CMD, SpcmRegs.SPCM_DDS_CMD_EXEC_AT_TRG);
            }
        }
    }
}
