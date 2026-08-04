using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NationalInstruments.UI;
using SpectrumDDS;

namespace SpectrumDDSController
{
    /// <summary>
    /// Status, pattern and manual control for the Spectrum DDS card.
    /// </summary>
    /// <remarks>
    /// The window is a view over <see cref="Controller"/>, which owns the card and
    /// is also what MOTMaster talks to. It works with MOTMaster closed, which is the
    /// point of running as a separate process.
    /// </remarks>
    public partial class MainWindow : Form
    {
        private const int ChannelCount = DDSPattern.ChannelCount;

        private readonly Controller controller;
        private DDSPattern pattern = new DDSPattern();
        private bool suppressGridEvents;
        private bool gridRebuildPending;

        /// <summary>
        /// The last pattern seen in <see cref="Controller.LoadedPattern"/>. Anything
        /// else turning up there was loaded over remoting rather than from here.
        /// </summary>
        private DDSPattern lastSeenLoadedPattern;

        /// <summary>Whether what the grid and graphs show arrived from MOTMaster.</summary>
        private bool patternFromMOTMaster;

        // Manual tab controls, built in code because there are four identical rows.
        private readonly NumericUpDown[] manualFrequency = new NumericUpDown[ChannelCount];
        private readonly NumericUpDown[] manualAmplitude = new NumericUpDown[ChannelCount];
        private readonly NumericUpDown[] manualClamp = new NumericUpDown[ChannelCount];
        private readonly CheckBox[] manualEnable = new CheckBox[ChannelCount];

        /// <summary>Clamps, last manual values and output level, kept across restarts.</summary>
        private readonly ControllerSettings settings = ControllerSettings.Load();

        private static readonly Color[] ChannelColours =
        {
            Color.FromArgb(0, 114, 189),    // DDS1 -> Ch0
            Color.FromArgb(217, 83, 25),    // DDS2 -> Ch1
            Color.FromArgb(119, 172, 48),   // DDS3 -> Ch2
            Color.FromArgb(126, 47, 142),   // DDS4 -> Ch3
        };

        public MainWindow(Controller controller, string patternFile = null)
        {
            this.controller = controller;

            // Before anything reads them off the driver. This touches no register,
            // so it is safe with the card still closed.
            for (int ch = 0; ch < ChannelCount; ch++)
                controller.Driver.SetMaximumAmplitude(ch, settings.MaximumAmplitudes[ch]);

            InitializeComponent();
            BuildPatternGridColumns();
            BuildGraphs();
            BuildManualTab();

            if (!string.IsNullOrEmpty(patternFile))
            {
                try
                {
                    pattern = DDSPatternFile.Load(patternFile);
                    tabs.SelectedTab = patternTab;
                }
                catch (Exception ex)
                {
                    Complain("Could not load " + patternFile, ex);
                }
            }
            RefreshGrid();
            RefreshGraphs();

            statusTimer.Start();
            UpdateStatusTab();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // SplitterDistance only sticks once the container has its real size, so
            // it cannot be set in the designer code. Give the grid and the graphs
            // roughly half each.
            patternSplit.SplitterDistance = patternSplit.Height / 2;

            // Open the card as soon as the window is up. MOTMaster cannot arm a
            // pattern against a closed card, and opening one enables no output stage
            // and emits nothing, so there is nothing here to catch out whoever
            // starts the program. Doing it in OnLoad rather than the constructor
            // means a failure has a window to complain over, and is not fatal: the
            // Open card button on the Status tab still works.
            if (!controller.IsOpen) TryOpenCard(true);
            UpdateStatusTab();
        }

        // -- status tab -------------------------------------------------------------

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (controller.IsOpen)
            {
                try
                {
                    controller.CloseCard();
                }
                catch (Exception ex)
                {
                    Complain("Could not close the DDS card", ex);
                }
            }
            else
            {
                TryOpenCard(true);
            }
            UpdateStatusTab();
        }

        /// <summary>
        /// Open the card and bring the controls that mirror driver state into line
        /// with it.
        /// </summary>
        /// <param name="complainOnFailure">
        /// Whether to put a message box up. The startup open passes true as well --
        /// a card that will not open is worth knowing about straight away.
        /// </param>
        private bool TryOpenCard(bool complainOnFailure)
        {
            try
            {
                controller.OpenCard();

                // Open() puts its own default level on the card, so push the
                // remembered one over it rather than reading the card's back.
                controller.Driver.SetOutputLevel((int)outputLevelBox.Value);

                // Outputs on by default. Open() leaves every core silenced, so the
                // stages come up carrying nothing: this emits no RF. The remembered
                // frequencies and amplitudes sit in the spinners and go no further
                // until somebody presses Apply now.
                for (int ch = 0; ch < ChannelCount; ch++) manualEnable[ch].Checked = true;

                UpdateClampLabel();
                return true;
            }
            catch (Exception ex)
            {
                if (complainOnFailure) Complain("Could not open the DDS card", ex);
                return false;
            }
        }

        private void statusTimer_Tick(object sender, EventArgs e)
        {
            AdoptExternallyLoadedPattern();
            UpdateStatusTab();
        }

        /// <summary>
        /// Show a pattern that arrived from MOTMaster rather than from this window.
        /// </summary>
        /// <remarks>
        /// <para>
        /// MOTMaster pushes patterns straight into <see cref="Controller.patternList"/>
        /// over remoting, which reaches the driver without passing through the window
        /// at all. Nothing else here redraws the pattern tab, so without this the grid
        /// and the graphs go on showing whatever was last edited or opened locally --
        /// an empty pattern, on a fresh start -- while the card runs something else
        /// entirely.
        /// </para>
        /// <para>
        /// Polling the status timer rather than raising an event from the setter keeps
        /// this on the UI thread; a remoting thread would have to marshal back here
        /// anyway. The view gets a clone, so editing the grid can never reach into the
        /// pattern the driver is arming.
        /// </para>
        /// </remarks>
        private void AdoptExternallyLoadedPattern()
        {
            DDSPattern loaded;
            try
            {
                loaded = controller.LoadedPattern;
            }
            catch (Exception)
            {
                return;     // the card went away; the status tab will say so
            }

            // A null means MOTMaster is between patterns, which is no reason to
            // throw away what is on screen.
            if (loaded == null || ReferenceEquals(loaded, lastSeenLoadedPattern)) return;

            // Never pull the grid out from under someone who is typing in it. The
            // next tick will pick the pattern up.
            if (patternGrid.IsCurrentCellInEditMode) return;

            lastSeenLoadedPattern = loaded;
            pattern = loaded.Clone();
            patternFromMOTMaster = true;
            RefreshGrid();
            RefreshGraphs();
        }

        private void UpdateStatusTab()
        {
            connectButton.Text = controller.IsOpen ? "Close card" : "Open card";

            if (!controller.IsOpen)
            {
                identityBox.Text = "The card is not open.";
                liveStatusBox.Text = "";
                return;
            }

            try
            {
                SpectrumDDSDriver driver = controller.Driver;
                identityBox.Text = driver.Identity + Environment.NewLine +
                                   Environment.NewLine + driver.Capabilities;

                DDSPattern loaded = controller.LoadedPattern;
                string text =
                    "DDS status        " + driver.StatusText() + Environment.NewLine +
                    "queued commands   " + driver.QueuedCommandCount + " of " +
                        driver.Capabilities.QueueCommandMaximum + Environment.NewLine +
                    "card trigger count " + driver.TriggerCount + Environment.NewLine +
                    "patterns fired    " + driver.PatternsFired + Environment.NewLine +
                    "run state         " + (controller.IsRunning ? "armed for triggers" : "stopped") +
                        Environment.NewLine +
                    "loaded pattern    " + (loaded == null ? "none"
                        : string.Format(CultureInfo.InvariantCulture,
                            "{0} events over {1:0.###} ms, {2} commands",
                            loaded.Count, loaded.Span * 1e3, loaded.CommandCount())) +
                        Environment.NewLine + Environment.NewLine +
                    "outputs           " + string.Join(", ",
                        Enumerable.Range(0, ChannelCount)
                                  .Select(i => "DDS" + (i + 1) + " " +
                                      (driver.OutputsEnabled[i] ? "on" : "off")).ToArray()) +
                        Environment.NewLine +
                    "output level      " + driver.OutputLevelMillivolts + " mV" + Environment.NewLine +
                    "XIO markers       " + (driver.XioMarkersRouted
                        ? "driving X0/X1/X2"
                        : "NOT AVAILABLE on this card - the XIO column drives nothing") +
                        Environment.NewLine +
                    "amplitude clamps  " + string.Join(", ",
                        Enumerable.Range(0, ChannelCount)
                                  .Select(i => "DDS" + (i + 1) + " " +
                                      driver.MaximumAmplitudes[i].ToString("0.###",
                                          CultureInfo.InvariantCulture)).ToArray());

                // Only touch the box when something changed, or the caret and any
                // selection jump every 250 ms.
                if (liveStatusBox.Text != text) liveStatusBox.Text = text;
            }
            catch (Exception ex)
            {
                liveStatusBox.Text = "status read failed: " + ex.Message;
            }
        }

        // -- pattern tab -------------------------------------------------------------

        private void BuildPatternGridColumns()
        {
            patternGrid.AutoGenerateColumns = false;
            patternGrid.Columns.Add(TextColumn("name", "Event"));
            patternGrid.Columns.Add(TextColumn("time", "t / ms"));
            for (int ch = 0; ch < ChannelCount; ch++)
            {
                string label = "DDS" + (ch + 1);
                patternGrid.Columns.Add(TextColumn("f" + ch, label + " f / MHz"));
                patternGrid.Columns.Add(TextColumn("a" + ch, label + " amp"));
                patternGrid.Columns.Add(TextColumn("df" + ch, label + " df / MHz per ms"));
                patternGrid.Columns.Add(TextColumn("da" + ch, label + " da / per ms"));
            }
            patternGrid.Columns.Add(TextColumn("xio", "XIO"));
        }

        private static DataGridViewTextBoxColumn TextColumn(string name, string header)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = 80,
            };
        }

        private void BuildGraphs()
        {
            ConfigureGraph(frequencyGraph, "frequency / MHz");
            ConfigureGraph(amplitudeGraph, "amplitude");
        }

        private void ConfigureGraph(NationalInstruments.UI.WindowsForms.ScatterGraph graph,
                                    string yCaption)
        {
            graph.Plots.Clear();

            // A ScatterGraph built in code starts with no axes at all -- the
            // designer normally adds them -- so indexing XAxes[0] straight off
            // throws.
            graph.XAxes.Clear();
            graph.YAxes.Clear();
            XAxis xAxis = new XAxis { Caption = "time / ms" };
            YAxis yAxis = new YAxis { Caption = yCaption };
            graph.XAxes.Add(xAxis);
            graph.YAxes.Add(yAxis);
            graph.Caption = yCaption + " against time";

            for (int ch = 0; ch < ChannelCount; ch++)
            {
                ScatterPlot plot = new ScatterPlot
                {
                    LineColor = ChannelColours[ch],
                    LineWidth = 2,
                    PointStyle = PointStyle.SolidCircle,
                    PointColor = ChannelColours[ch],
                    XAxis = xAxis,
                    YAxis = yAxis,
                };
                graph.Plots.Add(plot);
            }
        }

        /// <summary>Redraw both graphs from the pattern currently being edited.</summary>
        private void RefreshGraphs()
        {
            for (int ch = 0; ch < ChannelCount; ch++)
            {
                if (pattern.Count == 0)
                {
                    frequencyGraph.Plots[ch].ClearData();
                    amplitudeGraph.Plots[ch].ClearData();
                    continue;
                }

                double[] times, frequencies, amplitudes;
                pattern.Timeline(ch, out times, out frequencies, out amplitudes);

                // Ramps come back as their start and end values, so a straight line
                // between the points is the frequency or amplitude the card really
                // produces, not a staircase.
                double[] ms = times.Select(t => t * 1e3).ToArray();
                frequencyGraph.Plots[ch].PlotXY(ms, frequencies.Select(f => f / 1e6).ToArray());
                amplitudeGraph.Plots[ch].PlotXY(ms, amplitudes);
            }
        }

        private void RefreshGrid()
        {
            suppressGridEvents = true;
            // Clearing the rows drops the current cell, which would throw the focus
            // back to the top left every time an edit is committed.
            int currentColumn = patternGrid.CurrentCell == null ? -1 : patternGrid.CurrentCell.ColumnIndex;
            int currentRow = patternGrid.CurrentCell == null ? -1 : patternGrid.CurrentCell.RowIndex;
            try
            {
                patternGrid.Rows.Clear();
                foreach (DDSEvent e in pattern.Events)
                {
                    List<object> cells = new List<object> { e.Name, Fmt(e.Time * 1e3) };
                    foreach (DDSChannelState c in e.Channels)
                    {
                        cells.Add(Fmt(c.Frequency / DDSPattern.HzPerMHz));
                        cells.Add(Fmt(c.Amplitude));
                        cells.Add(Fmt(c.FrequencySlope / DDSPattern.HzPerSecondPerMHzPerMs));
                        cells.Add(Fmt(c.AmplitudeSlope / DDSPattern.PerSecondPerPerMs));
                    }
                    cells.Add(e.Xio);
                    patternGrid.Rows.Add(cells.ToArray());
                }

                if (currentRow >= 0 && currentRow < patternGrid.Rows.Count &&
                    currentColumn >= 0 && currentColumn < patternGrid.Columns.Count)
                {
                    patternGrid.CurrentCell = patternGrid.Rows[currentRow].Cells[currentColumn];
                }
            }
            finally
            {
                suppressGridEvents = false;
            }
            UpdatePatternInfo();
        }

        private static string Fmt(double v)
        {
            return v.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private void UpdatePatternInfo()
        {
            patternInfoLabel.Text = pattern.Count == 0
                ? "no events"
                : string.Format(CultureInfo.InvariantCulture,
                    "{0} events, {1:0.###} ms, {2} commands{3}",
                    pattern.Count, pattern.Span * 1e3, pattern.CommandCount(),
                    patternFromMOTMaster ? " — loaded by MOTMaster" : "");
        }

        private void patternGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (suppressGridEvents) return;
            // Clicking another cell commits the edit from inside the grid's own click
            // handling, so touching the rows here re-enters SetCurrentCellAddressCore
            // and the grid throws. Post the rebuild back to the message queue instead
            // and do it once the grid has finished with the click.
            if (gridRebuildPending || !IsHandleCreated || IsDisposed) return;
            gridRebuildPending = true;
            BeginInvoke((MethodInvoker)RebuildFromGrid);
        }

        private void RebuildFromGrid()
        {
            gridRebuildPending = false;
            try
            {
                pattern = ReadPatternFromGrid();
                patternFromMOTMaster = false;
            }
            catch (FormatException)
            {
                MessageBox.Show(this, "That is not a number.", "Spectrum DDS",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RefreshGrid();
                return;
            }
            // Re-sorting on time can move the row that was just edited, so rebuild
            // the grid from the sorted pattern rather than leaving the two out of step.
            RefreshGrid();
            RefreshGraphs();
        }

        private DDSPattern ReadPatternFromGrid()
        {
            List<DDSEvent> events = new List<DDSEvent>();
            foreach (DataGridViewRow row in patternGrid.Rows)
            {
                if (row.IsNewRow) continue;
                int col = 0;
                string name = Convert.ToString(row.Cells[col++].Value);
                double time = ParseCell(row.Cells[col++]) / DDSPattern.MillisecondsPerSecond;

                DDSChannelState[] channels = new DDSChannelState[ChannelCount];
                for (int ch = 0; ch < ChannelCount; ch++)
                {
                    channels[ch] = new DDSChannelState(
                        ParseCell(row.Cells[col++]) * DDSPattern.HzPerMHz,
                        ParseCell(row.Cells[col++]),
                        ParseCell(row.Cells[col++]) * DDSPattern.HzPerSecondPerMHzPerMs,
                        ParseCell(row.Cells[col++]) * DDSPattern.PerSecondPerPerMs);
                }
                int xio = (int)ParseCell(row.Cells[col]);
                events.Add(new DDSEvent(name, time, channels, xio));
            }
            return new DDSPattern(events);
        }

        private static double ParseCell(DataGridViewCell cell)
        {
            string text = Convert.ToString(cell.Value);
            if (string.IsNullOrEmpty(text)) return 0.0;
            return double.Parse(text, CultureInfo.InvariantCulture);
        }

        private void addEventButton_Click(object sender, EventArgs e)
        {
            double time = pattern.Count == 0 ? 0.0 : pattern[pattern.Count - 1].Time + 0.010;
            DDSChannelState[] channels = pattern.Count == 0
                ? DDSEvent.NewChannels()
                : pattern[pattern.Count - 1].Channels.Select(c => c.Clone()).ToArray();
            pattern.Add(new DDSEvent("event" + (pattern.Count + 1), time, channels));
            patternFromMOTMaster = false;
            RefreshGrid();
            RefreshGraphs();
        }

        private void removeEventButton_Click(object sender, EventArgs e)
        {
            if (patternGrid.CurrentRow == null) return;
            int index = patternGrid.CurrentRow.Index;
            if (index < 0 || index >= pattern.Count) return;
            pattern.Remove(pattern[index]);
            patternFromMOTMaster = false;
            RefreshGrid();
            RefreshGraphs();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "DDS pattern (*.json)|*.json|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    pattern = DDSPatternFile.Load(dialog.FileName);
                    patternFromMOTMaster = false;
                }
                catch (Exception ex)
                {
                    Complain("Could not load that pattern", ex);
                    return;
                }
            }
            RefreshGrid();
            RefreshGraphs();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "DDS pattern (*.json)|*.json";
                dialog.DefaultExt = "json";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    DDSPatternFile.Save(pattern, dialog.FileName);
                }
                catch (Exception ex)
                {
                    Complain("Could not save that pattern", ex);
                }
            }
        }

        private void applyPatternButton_Click(object sender, EventArgs e)
        {
            if (!RequireOpenCard()) return;
            try
            {
                controller.PrepareForNewPattern();
                controller.patternList = pattern.ToLegacyDictionary();

                // What the driver now holds is this pattern round-tripped through
                // the legacy dictionary, which has no XIO column. Claim it as seen
                // so the poll does not adopt it back and quietly drop the XIO edits.
                lastSeenLoadedPattern = controller.LoadedPattern;
            }
            catch (Exception ex)
            {
                Complain("The card would not take that pattern", ex);
            }
            UpdateStatusTab();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            if (!RequireOpenCard()) return;
            try
            {
                controller.StartRepetitivePattern();
            }
            catch (Exception ex)
            {
                Complain("Could not arm the pattern", ex);
            }
            UpdateStatusTab();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            controller.StopRepetitivePattern();
            UpdateStatusTab();
        }

        private void testTriggerButton_Click(object sender, EventArgs e)
        {
            if (!RequireOpenCard()) return;
            try
            {
                // Runs the armed pattern once, for a scope check with the
                // experiment idle. Nothing but the DDS card sees this.
                controller.ForceTrigger();
            }
            catch (Exception ex)
            {
                Complain("Could not fire a test trigger", ex);
            }
            UpdateStatusTab();
        }

        // -- manual tab -----------------------------------------------------------------

        private void BuildManualTab()
        {
            manualLayout.Controls.Clear();
            manualLayout.ColumnStyles.Clear();
            manualLayout.RowStyles.Clear();
            manualLayout.ColumnCount = 6;
            manualLayout.RowCount = ChannelCount + 3;
            manualLayout.AutoSize = true;

            manualLayout.Controls.Add(HeaderLabel("channel"), 0, 0);
            manualLayout.Controls.Add(HeaderLabel("frequency / MHz"), 1, 0);
            manualLayout.Controls.Add(HeaderLabel("amplitude"), 2, 0);
            manualLayout.Controls.Add(HeaderLabel("clamp"), 3, 0);
            manualLayout.Controls.Add(HeaderLabel("apply"), 4, 0);
            manualLayout.Controls.Add(HeaderLabel("output"), 5, 0);

            for (int ch = 0; ch < ChannelCount; ch++)
            {
                int channel = ch;   // captured by the handlers below
                int row = ch + 1;

                Label name = new Label
                {
                    Text = string.Format("DDS{0}  (core {1} to Ch{2})",
                        ch + 1, SpectrumDDSDriver.CoreForChannel[ch], ch),
                    AutoSize = true,
                    ForeColor = ChannelColours[ch],
                    Font = new Font(Font, FontStyle.Bold),
                    Padding = new Padding(0, 6, 12, 0),
                };
                manualLayout.Controls.Add(name, 0, row);

                // Maximum before Value throughout: a NumericUpDown silently pulls a
                // Value down to whatever Maximum currently is, so setting a
                // remembered value first would lose it.
                manualFrequency[ch] = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 625,
                    DecimalPlaces = 4,
                    Increment = 0.1M,
                    Value = (decimal)settings.ManualFrequenciesMHz[ch],
                    Width = 110,
                };
                manualLayout.Controls.Add(manualFrequency[ch], 1, row);

                manualAmplitude[ch] = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = (decimal)settings.MaximumAmplitudes[ch],
                    DecimalPlaces = 4,
                    Increment = 0.005M,
                    Value = (decimal)settings.ManualAmplitudes[ch],
                    Width = 90,
                };
                manualLayout.Controls.Add(manualAmplitude[ch], 2, row);

                manualClamp[ch] = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 1.0M,
                    DecimalPlaces = 3,
                    Increment = 0.05M,
                    Value = (decimal)settings.MaximumAmplitudes[ch],
                    Width = 80,
                };
                // Attached after the value is in place, so building the tab does not
                // look like the experimenter moving a clamp.
                manualClamp[ch].ValueChanged += (s, e) => ManualClampChanged(channel);
                manualLayout.Controls.Add(manualClamp[ch], 3, row);

                Button apply = new Button { Text = "Apply now", Width = 90 };
                apply.Click += (s, e) => ApplyManualTone(channel);
                manualLayout.Controls.Add(apply, 4, row);

                manualEnable[ch] = new CheckBox { Text = "enabled", AutoSize = true, Padding = new Padding(0, 4, 0, 0) };
                manualEnable[ch].CheckedChanged += (s, e) => SetOutputEnabled(channel, manualEnable[channel].Checked);
                manualLayout.Controls.Add(manualEnable[ch], 5, row);
            }

            outputLevelBox.Value = settings.OutputLevelMillivolts;
            manualLayout.Controls.Add(outputLevelLabel, 0, ChannelCount + 1);
            manualLayout.Controls.Add(outputLevelBox, 1, ChannelCount + 1);
            manualLayout.Controls.Add(silenceButton, 4, ChannelCount + 1);

            UpdateClampLabel();
        }

        private static Label HeaderLabel(string text)
        {
            return new Label { Text = text, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        }

        private void ApplyManualTone(int channel)
        {
            if (!RequireOpenCard()) return;
            try
            {
                // EXEC_NOW executes against the live registers, so this throws away
                // any armed pattern; the controller stops the shot loop first.
                controller.StopRepetitivePattern();
                controller.Driver.SetToneNow(channel,
                    (double)manualFrequency[channel].Value * DDSPattern.HzPerMHz,
                    (double)manualAmplitude[channel].Value);
                SaveSettings();
            }
            catch (Exception ex)
            {
                Complain("Could not set that tone", ex);
            }
            UpdateStatusTab();
        }

        private void SetOutputEnabled(int channel, bool enabled)
        {
            if (!controller.IsOpen)
            {
                manualEnable[channel].Checked = false;
                return;
            }
            try
            {
                controller.Driver.SetOutputEnabled(channel, enabled);
            }
            catch (Exception ex)
            {
                Complain("Could not change the output stage", ex);
            }
            UpdateStatusTab();
        }

        private void ManualClampChanged(int channel)
        {
            decimal clamp = manualClamp[channel].Value;
            controller.Driver.SetMaximumAmplitude(channel, (double)clamp);
            settings.MaximumAmplitudes[channel] = (double)clamp;

            // The amplitude spinner must not let you dial past this channel's clamp,
            // or the driver simply throws when you press Apply.
            manualAmplitude[channel].Maximum = clamp;
            if (manualAmplitude[channel].Value > clamp) manualAmplitude[channel].Value = clamp;

            // Clamps are the one setting worth writing out straight away rather than
            // at closing time: a crash should not quietly restore a higher one.
            SaveSettings();
            UpdateClampLabel();
        }

        private void UpdateClampLabel()
        {
            double[] clamps = controller.Driver.MaximumAmplitudes;
            string perChannel = string.Join(", ",
                Enumerable.Range(0, ChannelCount)
                          .Select(ch => string.Format(CultureInfo.InvariantCulture,
                              "DDS{0} {1:0.###}", ch + 1, clamps[ch])).ToArray());

            // DDS2 needs 0.6 and DDS3 0.35 for the MOT scripts to load at all; what
            // the production values should be is a physics decision, so this says
            // which channels are currently below them rather than raising anything.
            double[] wanted = { 0.25, 0.6, 0.35, 0.25 };
            string tooLow = string.Join(", ",
                Enumerable.Range(0, ChannelCount)
                          .Where(ch => clamps[ch] < wanted[ch])
                          .Select(ch => "DDS" + (ch + 1)).ToArray());

            clampLabel.Text = "Amplitude clamps — " + perChannel +
                (tooLow.Length == 0
                    ? "  (the MOT scripts will load)"
                    : "  — too low for the MOT scripts on " + tooLow);
        }

        private void outputLevelBox_ValueChanged(object sender, EventArgs e)
        {
            settings.OutputLevelMillivolts = (int)outputLevelBox.Value;
            if (!controller.IsOpen) return;
            try
            {
                controller.Driver.SetOutputLevel((int)outputLevelBox.Value);
            }
            catch (Exception ex)
            {
                Complain("Could not change the output level", ex);
            }
        }

        // -- settings ----------------------------------------------------------------

        /// <summary>
        /// Copy what is on the Manual tab into the settings and write them out.
        /// </summary>
        /// <remarks>
        /// Silent on failure: a settings file that cannot be written is not worth a
        /// message box every time the window closes, and the Status tab is not the
        /// place for it either. The values are all recoverable by retyping them.
        /// </remarks>
        private void SaveSettings()
        {
            for (int ch = 0; ch < ChannelCount; ch++)
            {
                settings.MaximumAmplitudes[ch] = (double)manualClamp[ch].Value;
                settings.ManualFrequenciesMHz[ch] = (double)manualFrequency[ch].Value;
                settings.ManualAmplitudes[ch] = (double)manualAmplitude[ch].Value;
            }
            settings.OutputLevelMillivolts = (int)outputLevelBox.Value;
            settings.Save();
        }

        private void silenceButton_Click(object sender, EventArgs e)
        {
            if (!RequireOpenCard()) return;
            try
            {
                controller.StopRepetitivePattern();
                controller.Driver.SilenceNow();
                for (int ch = 0; ch < ChannelCount; ch++) manualAmplitude[ch].Value = 0;
            }
            catch (Exception ex)
            {
                Complain("Could not silence the outputs", ex);
            }
            UpdateStatusTab();
        }

        // -- shared ------------------------------------------------------------------------

        private bool RequireOpenCard()
        {
            if (controller.IsOpen) return true;
            MessageBox.Show(this, "Open the card first, on the Status tab.", "Spectrum DDS",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void Complain(string what, Exception ex)
        {
            MessageBox.Show(this, what + ":" + Environment.NewLine + Environment.NewLine + ex.Message,
                "Spectrum DDS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            statusTimer.Stop();
            SaveSettings();
            try
            {
                controller.CloseCard();
            }
            catch (Exception)
            {
                // Nothing useful to do while shutting down.
            }
        }
    }
}
