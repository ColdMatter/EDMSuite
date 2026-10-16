using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MOTMaster
{
    public class PatternViewer : Form
    {
        // ── tuneable constants ────────────────────────────────────────────
        private const int VISIBLE_CHANNELS = 5;
        private const int LABEL_WIDTH = 160;  // wider for "SETNAME/CH 00"
        private const int CHANNEL_HEIGHT = 70;
        private const int WAVEFORM_PAD_V = 14;
        private const int CHECKLIST_WIDTH = 180;  // wider for same reason
        private const int AXIS_HEIGHT = 36;
        private const double TIME_MULTIPLIER = 0.01;
        private const string TIME_UNIT = "ms";

        // ── flattened channel lists ───────────────────────────────────────
        private readonly List<DigitalChannelDescriptor> _digitalChannels;
        private readonly List<AnalogChannelDescriptor> _analogChannels;

        // ── visibility arrays (indexed into the flat lists) ───────────────
        private readonly bool[] _digitalVisible;
        private readonly bool[] _analogVisible;

        // ── controls ─────────────────────────────────────────────────────
        private Panel _digitalDrawPanel;
        private VScrollBar _digitalVScroll;
        private CheckedListBox _digitalChannelList;
        private Button _btnDigitalSelectAll;

        private Panel _analogDrawPanel;
        private VScrollBar _analogVScroll;
        private CheckedListBox _analogChannelList;
        private Button _btnAnalogSelectAll;

        // ── x-axis view windows (in time units, i.e. samples × TIME_MULTIPLIER) ──
        private double _digitalViewStart;
        private double _digitalViewEnd;
        private double _analogViewStart;
        private double _analogViewEnd;

        // ── zoom bar controls (needed to update programmatically on reset) ────────
        private NumericUpDown _nudDigitalStart;
        private NumericUpDown _nudDigitalEnd;
        private NumericUpDown _nudAnalogStart;
        private NumericUpDown _nudAnalogEnd;

        // ─────────────────────────────────────────────────────────────────
        /// <param name="digitalSets">
        ///   Each entry is (setName, data) where data is a uint[] of samples.
        /// </param>
        /// <param name="analogSets">
        ///   Each entry is (setName, data) where data is double[channel, time].
        /// </param>
        /// <param name="digitalNames">
        ///   Optional. Each entry is (setName, names) where names maps
        ///   bit-index → custom channel name for that set.
        /// </param>
        /// <param name="analogNames">
        ///   Optional. Each entry is (setName, names) where names maps
        ///   row-index → custom channel name for that set.
        /// </param>
        public PatternViewer(
            List<(string setName, uint[] data)> digitalSets,
            List<(string setName, double[,] data)> analogSets,
            List<(string setName, SortedList<int, string> names)> digitalNames = null,
            List<(string setName, SortedList<int, string> names)> analogNames = null)
        {
            if (digitalSets == null) throw new ArgumentNullException(nameof(digitalSets));
            if (analogSets == null) throw new ArgumentNullException(nameof(analogSets));

            _digitalChannels = FlattenDigital(digitalSets, digitalNames);
            _analogChannels = FlattenAnalog(analogSets, analogNames);

            _digitalVisible = new bool[_digitalChannels.Count];
            _analogVisible = new bool[_analogChannels.Count];
            for (int i = 0; i < _digitalChannels.Count; i++) _digitalVisible[i] = true;
            for (int i = 0; i < _analogChannels.Count; i++) _analogVisible[i] = true;

            BuildUI();

            // Initialise view windows to full range
            double digitalTotal = MaxDigitalSamples() * TIME_MULTIPLIER;
            double analogTotal = MaxAnalogSamples() * TIME_MULTIPLIER;

            _digitalViewStart = 0;
            _digitalViewEnd = digitalTotal;
            _analogViewStart = 0;
            _analogViewEnd = analogTotal;

            // ++ Initialise the NUD values to match the full range
            SetZoomBar(_nudDigitalStart, _nudDigitalEnd, 0, digitalTotal);
            SetZoomBar(_nudAnalogStart, _nudAnalogEnd, 0, analogTotal);
        }

        // ════════════════════════════════════════════════════════════════
        //  Flattening helpers
        // ════════════════════════════════════════════════════════════════

        private static List<DigitalChannelDescriptor> FlattenDigital(
            List<(string setName, uint[] data)> sets,
            List<(string setName, SortedList<int, string> names)> namesList)
        {
            var result = new List<DigitalChannelDescriptor>();

            foreach (var (setName, data) in sets)
            {
                // Find the matching names entry for this set (if any)
                SortedList<int, string> names = null;
                if (namesList != null)
                {
                    foreach (var (n, nl) in namesList)
                        if (n == setName) { names = nl; break; }
                }

                // Each uint[] has 32 bit channels
                for (int bit = 0; bit < 32; bit++)
                {
                    string customName = null;
                    names?.TryGetValue(bit, out customName);
                    result.Add(new DigitalChannelDescriptor(setName, bit, data, customName));
                }
            }

            return result;
        }

        private static List<AnalogChannelDescriptor> FlattenAnalog(
            List<(string setName, double[,] data)> sets,
            List<(string setName, SortedList<int, string> names)> namesList)
        {
            var result = new List<AnalogChannelDescriptor>();

            foreach (var (setName, data) in sets)
            {
                SortedList<int, string> names = null;
                if (namesList != null)
                {
                    foreach (var (n, nl) in namesList)
                        if (n == setName) { names = nl; break; }
                }

                int channelCount = data.GetLength(0);
                int sampleCount = data.GetLength(1);

                for (int row = 0; row < channelCount; row++)
                {
                    // Pre-compute min/max for this channel
                    double mn = double.MaxValue, mx = double.MinValue;
                    for (int t = 0; t < sampleCount; t++)
                    {
                        double v = data[row, t];
                        if (v < mn) mn = v;
                        if (v > mx) mx = v;
                    }
                    if (Math.Abs(mx - mn) < double.Epsilon) mx = mn + 1.0;

                    string customName = null;
                    names?.TryGetValue(row, out customName);

                    result.Add(new AnalogChannelDescriptor(
                        setName, row, data, customName, mn, mx));
                }
            }

            return result;
        }

        // ════════════════════════════════════════════════════════════════
        //  UI Construction
        // ════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            Text = "Channel Viewer";
            Size = new Size(1100, VISIBLE_CHANNELS * CHANNEL_HEIGHT * 2 + 120);
            MinimumSize = new Size(700, 400);
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            StartPosition = FormStartPosition.CenterParent;

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(60, 60, 60),
                SplitterWidth = 5
            };

            BuildDigitalPane(split.Panel1);
            BuildAnalogPane(split.Panel2);
            Controls.Add(split);
        }

        // ────────────────────────────────────────────────────────────────
        //  Pane builders
        // ────────────────────────────────────────────────────────────────
        private void BuildDigitalPane(SplitterPanel parent)
        {
            var header = MakeSectionHeader("⬛  Digital Channels");

            _digitalVScroll = MakeScrollBar(_digitalChannels.Count);
            _digitalVScroll.ValueChanged += (s, e) => _digitalDrawPanel.Invalidate();

            _digitalDrawPanel = MakeDrawPanel();
            _digitalDrawPanel.Paint += OnDigitalPanelPaint;

            var sidebar = BuildSidebar(
                _digitalChannels.Count,
                i => _digitalChannels[i].DisplayLabel,
                out _digitalChannelList,
                out _btnDigitalSelectAll);

            _digitalChannelList.ItemCheck += OnDigitalChannelToggled;
            _btnDigitalSelectAll.Click += OnDigitalSelectDeselectAll;

            // ++ zoom bar
            var zoomBar = BuildZoomBar(
                out _nudDigitalStart,
                out _nudDigitalEnd,
                onChanged: () =>
                {
                    _digitalViewStart = (double)_nudDigitalStart.Value;
                    _digitalViewEnd = (double)_nudDigitalEnd.Value;
                    _digitalDrawPanel.Invalidate();
                },
                onReset: () =>
                {
                    double total = MaxDigitalSamples() * TIME_MULTIPLIER;
                    SetZoomBar(_nudDigitalStart, _nudDigitalEnd, 0, total);
                    _digitalViewStart = 0;
                    _digitalViewEnd = total;
                    _digitalDrawPanel.Invalidate();
                });

            var content = new Panel { Dock = DockStyle.Fill };
            content.Controls.Add(_digitalDrawPanel);
            content.Controls.Add(_digitalVScroll);
            content.Controls.Add(zoomBar);         // ++ added

            parent.Controls.Add(content);
            parent.Controls.Add(sidebar);
            parent.Controls.Add(header);

            UpdateScrollRange(_digitalVScroll, _digitalChannels.Count);
        }

        private void BuildAnalogPane(SplitterPanel parent)
        {
            var header = MakeSectionHeader("〰  Analog Channels");

            _analogVScroll = MakeScrollBar(_analogChannels.Count);
            _analogVScroll.ValueChanged += (s, e) => _analogDrawPanel.Invalidate();

            _analogDrawPanel = MakeDrawPanel();
            _analogDrawPanel.Paint += OnAnalogPanelPaint;

            var sidebar = BuildSidebar(
                _analogChannels.Count,
                i => _analogChannels[i].DisplayLabel,
                out _analogChannelList,
                out _btnAnalogSelectAll);

            _analogChannelList.ItemCheck += OnAnalogChannelToggled;
            _btnAnalogSelectAll.Click += OnAnalogSelectDeselectAll;

            // ++ zoom bar
            var zoomBar = BuildZoomBar(
                out _nudAnalogStart,
                out _nudAnalogEnd,
                onChanged: () =>
                {
                    _analogViewStart = (double)_nudAnalogStart.Value;
                    _analogViewEnd = (double)_nudAnalogEnd.Value;
                    _analogDrawPanel.Invalidate();
                },
                onReset: () =>
                {
                    double total = MaxAnalogSamples() * TIME_MULTIPLIER;
                    SetZoomBar(_nudAnalogStart, _nudAnalogEnd, 0, total);
                    _analogViewStart = 0;
                    _analogViewEnd = total;
                    _analogDrawPanel.Invalidate();
                });

            var content = new Panel { Dock = DockStyle.Fill };
            content.Controls.Add(_analogDrawPanel);
            content.Controls.Add(_analogVScroll);
            content.Controls.Add(zoomBar);         // ++ added

            parent.Controls.Add(content);
            parent.Controls.Add(sidebar);
            parent.Controls.Add(header);

            UpdateScrollRange(_analogVScroll, _analogChannels.Count);
        }

        // ════════════════════════════════════════════════════════════════
        //  Shared UI factories
        // ════════════════════════════════════════════════════════════════
        private Panel MakeDrawPanel()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 20) };
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                ?.SetValue(p, true);
            p.Resize += (s, e) => ((Panel)s).Invalidate();
            return p;
        }

        private static VScrollBar MakeScrollBar(int channelCount)
        {
            return new VScrollBar
            {
                Dock = DockStyle.Right,
                Minimum = 0,
                Maximum = Math.Max(0, channelCount - 1),
                SmallChange = 1,
                LargeChange = VISIBLE_CHANNELS,
                Value = 0
            };
        }

        private static Label MakeSectionHeader(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 26,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            BackColor = Color.FromArgb(45, 45, 60),
            ForeColor = Color.FromArgb(200, 210, 255),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };

        private static Panel BuildSidebar(
            int channelCount,
            Func<int, string> labelFunc,
            out CheckedListBox listBox,
            out Button selectAllBtn)
        {
            var headerLabel = new Label
            {
                Text = "Channels",
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            selectAllBtn = new Button
            {
                Text = "Deselect All",
                Dock = DockStyle.Bottom,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            selectAllBtn.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);

            listBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                CheckOnClick = true,
                Font = new Font("Consolas", 8.5f)
            };
            for (int i = 0; i < channelCount; i++)
                listBox.Items.Add(labelFunc(i), true);

            var sidebar = new Panel
            {
                Width = CHECKLIST_WIDTH,
                Dock = DockStyle.Left,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            sidebar.Controls.Add(listBox);
            sidebar.Controls.Add(selectAllBtn);
            sidebar.Controls.Add(headerLabel);
            return sidebar;
        }

        /// <summary>
        /// Builds a slim toolbar with Start/End NumericUpDowns and a Reset button.
        /// The toolbar docks to the bottom of its parent panel, above the time axis.
        /// </summary>
        private Panel BuildZoomBar(
    out NumericUpDown nudStart,
    out NumericUpDown nudEnd,
    Action onChanged,
    Action onReset)
        {
            var bar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(28, 28, 38),
                Padding = new Padding(4, 0, 4, 0)
            };

            var lblStart = new Label
            {
                Text = "Start:",
                ForeColor = Color.FromArgb(170, 180, 210),
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Location = new Point(6, 6)
            };

            nudStart = new NumericUpDown
            {
                Minimum = 0,
                Maximum = decimal.MaxValue,
                DecimalPlaces = 4,
                Increment = (decimal)TIME_MULTIPLIER,
                Width = 100,
                BackColor = Color.FromArgb(40, 40, 55),
                ForeColor = Color.White,
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Right,
                Anchor = AnchorStyles.Left,
                Location = new Point(46, 4)
            };

            var lblEnd = new Label
            {
                Text = "End:",
                ForeColor = Color.FromArgb(170, 180, 210),
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Location = new Point(156, 6)
            };

            nudEnd = new NumericUpDown
            {
                Minimum = 0,
                Maximum = decimal.MaxValue,
                DecimalPlaces = 4,
                Increment = (decimal)TIME_MULTIPLIER,
                Width = 100,
                BackColor = Color.FromArgb(40, 40, 55),
                ForeColor = Color.White,
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Right,
                Anchor = AnchorStyles.Left,
                Location = new Point(192, 4)
            };

            var btnReset = new Button
            {
                Text = "⟳ Reset",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 55, 75),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand,
                Width = 72,
                Height = 22,
                Anchor = AnchorStyles.Left,
                Location = new Point(302, 3)
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 120);

            // ++ Copy out params into locals so lambdas can capture them legally
            NumericUpDown localStart = nudStart;
            NumericUpDown localEnd = nudEnd;

            localStart.ValueChanged += (s, e) =>
            {
                if (localStart.Value >= localEnd.Value)
                    localStart.Value = Math.Max(0, localEnd.Value - (decimal)TIME_MULTIPLIER);
                onChanged();
            };

            localEnd.ValueChanged += (s, e) =>
            {
                if (localEnd.Value <= localStart.Value)
                    localEnd.Value = localStart.Value + (decimal)TIME_MULTIPLIER;
                onChanged();
            };

            btnReset.Click += (s, e) => onReset();

            bar.Controls.AddRange(new Control[]
                { lblStart, localStart, lblEnd, localEnd, btnReset });

            return bar;
        }

        /// <summary>Sets both NUDs without triggering recursive ValueChanged calls.</summary>
        private static void SetZoomBar(NumericUpDown nudStart, NumericUpDown nudEnd,
                                double start, double end)
        {
            nudStart.Maximum = decimal.MaxValue;
            nudEnd.Maximum = decimal.MaxValue;

            decimal decStart = (decimal)Math.Max(0, start);
            decimal decEnd = (decimal)end;                          // ++ convert end to decimal first
            decimal minEndVal = decStart + (decimal)TIME_MULTIPLIER;  // ++ both operands now decimal

            nudStart.Value = decStart;
            nudEnd.Value = Math.Max(minEndVal, decEnd);              // ++ Math.Max(decimal, decimal)
        }

        // ════════════════════════════════════════════════════════════════
        //  Scroll helpers
        // ════════════════════════════════════════════════════════════════
        private static void UpdateScrollRange(VScrollBar sb, int totalChannels)
        {
            sb.Maximum = Math.Max(0, totalChannels - 1);
            sb.LargeChange = VISIBLE_CHANNELS;
            sb.SmallChange = 1;
            ClampScroll(sb);
        }

        private static void ClampScroll(VScrollBar sb)
        {
            int maxVal = Math.Max(0, sb.Maximum - sb.LargeChange + 1);
            if (sb.Value > maxVal) sb.Value = maxVal;
        }

        // ════════════════════════════════════════════════════════════════
        //  Toggle handlers
        // ════════════════════════════════════════════════════════════════
        private void OnDigitalChannelToggled(object sender, ItemCheckEventArgs e)
        {
            _digitalVisible[e.Index] = (e.NewValue == CheckState.Checked);
            UpdateScrollRange(_digitalVScroll, _digitalChannels.Count);
            _digitalDrawPanel.Invalidate();
            UpdateSelectAllButton(_btnDigitalSelectAll, _digitalVisible,
                                  _digitalChannels.Count, e.Index, e.NewValue);
        }

        private void OnAnalogChannelToggled(object sender, ItemCheckEventArgs e)
        {
            _analogVisible[e.Index] = (e.NewValue == CheckState.Checked);
            UpdateScrollRange(_analogVScroll, _analogChannels.Count);
            _analogDrawPanel.Invalidate();
            UpdateSelectAllButton(_btnAnalogSelectAll, _analogVisible,
                                  _analogChannels.Count, e.Index, e.NewValue);
        }

        private void OnDigitalSelectDeselectAll(object sender, EventArgs e)
        {
            BulkToggle(_digitalChannelList, _digitalVisible,
                       _digitalChannels.Count, OnDigitalChannelToggled);
            UpdateScrollRange(_digitalVScroll, _digitalChannels.Count);
            UpdateSelectAllButton(_btnDigitalSelectAll,
                                  _digitalVisible, _digitalChannels.Count);
            _digitalDrawPanel.Invalidate();
        }

        private void OnAnalogSelectDeselectAll(object sender, EventArgs e)
        {
            BulkToggle(_analogChannelList, _analogVisible,
                       _analogChannels.Count, OnAnalogChannelToggled);
            UpdateScrollRange(_analogVScroll, _analogChannels.Count);
            UpdateSelectAllButton(_btnAnalogSelectAll,
                                  _analogVisible, _analogChannels.Count);
            _analogDrawPanel.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════
        //  Shared toggle helpers
        // ════════════════════════════════════════════════════════════════
        private static void BulkToggle(CheckedListBox list, bool[] visible,
                                       int count, ItemCheckEventHandler handler)
        {
            bool target = !AreAllChecked(visible, count);
            list.ItemCheck -= handler;
            for (int i = 0; i < count; i++)
            {
                visible[i] = target;
                list.SetItemChecked(i, target);
            }
            list.ItemCheck += handler;
        }

        private static bool AreAllChecked(bool[] visible, int count)
        {
            for (int i = 0; i < count; i++) if (!visible[i]) return false;
            return true;
        }

        private static void UpdateSelectAllButton(Button btn, bool[] visible, int count)
            => btn.Text = AreAllChecked(visible, count) ? "Deselect All" : "Select All";

        private static void UpdateSelectAllButton(Button btn, bool[] visible,
                                                  int count, int changed, CheckState state)
        {
            int n = 0;
            for (int i = 0; i < count; i++)
            {
                bool will = (i == changed) ? (state == CheckState.Checked) : visible[i];
                if (will) n++;
            }
            btn.Text = (n == count) ? "Deselect All" : "Select All";
        }

        // ════════════════════════════════════════════════════════════════
        //  Digital paint
        // ════════════════════════════════════════════════════════════════
        private void OnDigitalPanelPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            int panelW = _digitalDrawPanel.ClientSize.Width;
            int panelH = _digitalDrawPanel.ClientSize.Height;
            g.Clear(Color.FromArgb(20, 20, 20));

            int maxSamples = MaxDigitalSamples();
            int[] visible = GetVisibleIndices(_digitalVisible, _digitalChannels.Count);
            int availableH = panelH - AXIS_HEIGHT;
            int rowH = Math.Max(1, availableH / VISIBLE_CHANNELS);
            int plotW = panelW - LABEL_WIDTH - 4;

            if (visible.Length > 0)
            {
                int scrollOffset = Math.Min(_digitalVScroll.Value,
                                   Math.Max(0, visible.Length - VISIBLE_CHANNELS));

                for (int slot = 0; slot < VISIBLE_CHANNELS; slot++)
                {
                    int idx = scrollOffset + slot;
                    if (idx >= visible.Length) break;
                    DrawDigitalRow(g, _digitalChannels[visible[idx]], slot, slot * rowH,
                                   plotW, panelW, rowH, maxSamples,
                                   _digitalViewStart, _digitalViewEnd); // ++ view window
                }

                using (var pen = new Pen(Color.FromArgb(70, 70, 70)))
                    g.DrawLine(pen, LABEL_WIDTH, 0, LABEL_WIDTH, panelH - AXIS_HEIGHT);
            }

            DrawTimeAxis(g, panelW, panelH, plotW,
                         _digitalViewStart, _digitalViewEnd); // ++ view window
        }

        // ════════════════════════════════════════════════════════════════
        //  Analog paint
        // ════════════════════════════════════════════════════════════════
        private void OnAnalogPanelPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int panelW = _analogDrawPanel.ClientSize.Width;
            int panelH = _analogDrawPanel.ClientSize.Height;
            g.Clear(Color.FromArgb(20, 20, 20));

            int maxSamples = MaxAnalogSamples();
            int[] visible = GetVisibleIndices(_analogVisible, _analogChannels.Count);
            int availableH = panelH - AXIS_HEIGHT;
            int rowH = Math.Max(1, availableH / VISIBLE_CHANNELS);
            int plotW = panelW - LABEL_WIDTH - 4;

            if (visible.Length > 0)
            {
                int scrollOffset = Math.Min(_analogVScroll.Value,
                                   Math.Max(0, visible.Length - VISIBLE_CHANNELS));

                for (int slot = 0; slot < VISIBLE_CHANNELS; slot++)
                {
                    int idx = scrollOffset + slot;
                    if (idx >= visible.Length) break;
                    DrawAnalogRow(g, _analogChannels[visible[idx]], slot, slot * rowH,
                                  plotW, panelW, rowH, maxSamples,
                                  _analogViewStart, _analogViewEnd); // ++ view window
                }

                using (var pen = new Pen(Color.FromArgb(70, 70, 70)))
                    g.DrawLine(pen, LABEL_WIDTH, 0, LABEL_WIDTH, panelH - AXIS_HEIGHT);
            }

            DrawTimeAxis(g, panelW, panelH, plotW,
                         _analogViewStart, _analogViewEnd); // ++ view window
        }

        // ════════════════════════════════════════════════════════════════
        //  Row drawing — Digital
        // ════════════════════════════════════════════════════════════════
        private void DrawDigitalRow(Graphics g, DigitalChannelDescriptor ch,
                            int slot, int rowY, int plotW, int panelW,
                            int rowH, int maxSamples,
                            double viewStart, double viewEnd) // ++ view window
        {
            using (var br = new SolidBrush(slot % 2 == 0
                       ? Color.FromArgb(22, 22, 22) : Color.FromArgb(28, 28, 28)))
                g.FillRectangle(br, 0, rowY, panelW, rowH);

            DrawDescriptorLabel(g, ch.DisplayLabel,
                string.IsNullOrWhiteSpace(ch.CustomName) ? null : ch.SecondaryLabel,
                rowY, rowH);

            using (var p = new Pen(Color.FromArgb(45, 45, 45)))
                g.DrawLine(p, 0, rowY + rowH - 1, panelW, rowY + rowH - 1);

            if (ch.Data.Length == 0) return;

            int waveTop = rowY + WAVEFORM_PAD_V;
            int waveBottom = rowY + rowH - WAVEFORM_PAD_V;
            int yHigh = waveTop;
            int yLow = waveBottom;

            // ++ Convert view window (time units) → sample indices
            double totalTime = maxSamples * TIME_MULTIPLIER;
            int iStart = (int)Math.Floor(viewStart / TIME_MULTIPLIER);
            int iEnd = (int)Math.Ceiling(viewEnd / TIME_MULTIPLIER);
            iStart = Math.Max(0, Math.Min(iStart, ch.Data.Length - 1));
            iEnd = Math.Max(0, Math.Min(iEnd, ch.Data.Length - 1));

            int visibleSamples = iEnd - iStart;
            if (visibleSamples <= 0) return;
            float xScale = (float)plotW / visibleSamples;

            using (var wavePen = new Pen(GetChannelColor(_digitalChannels.IndexOf(ch)), 1.5f))
            {
                int prevY = GetBit(ch.Data[iStart], ch.BitIndex) ? yHigh : yLow;
                float prevX = LABEL_WIDTH;

                for (int t = iStart + 1; t <= iEnd; t++)
                {
                    int curY = GetBit(ch.Data[t], ch.BitIndex) ? yHigh : yLow;
                    float curX = LABEL_WIDTH + (t - iStart) * xScale;
                    g.DrawLine(wavePen, prevX, prevY, curX, prevY);
                    if (curY != prevY) g.DrawLine(wavePen, curX, prevY, curX, curY);
                    prevX = curX;
                    prevY = curY;
                }
                g.DrawLine(wavePen, prevX, prevY, LABEL_WIDTH + plotW, prevY);
            }

            int midY = (waveTop + waveBottom) / 2;
            using (var mp = new Pen(Color.FromArgb(50, 50, 50), 1f))
                g.DrawLine(mp, LABEL_WIDTH, midY, LABEL_WIDTH + plotW, midY);
        }

        // ════════════════════════════════════════════════════════════════
        //  Row drawing — Analog
        // ════════════════════════════════════════════════════════════════
        private void DrawAnalogRow(Graphics g, AnalogChannelDescriptor ch,
                                   int slot, int rowY, int plotW, int panelW,
                                   int rowH, int maxSamples,
                                   double viewStart, double viewEnd) // ++ view window
        {
            using (var br = new SolidBrush(slot % 2 == 0
                       ? Color.FromArgb(22, 22, 22) : Color.FromArgb(28, 28, 28)))
                g.FillRectangle(br, 0, rowY, panelW, rowH);

            DrawDescriptorLabel(g, ch.DisplayLabel,
                string.IsNullOrWhiteSpace(ch.CustomName) ? null : ch.SecondaryLabel,
                rowY, rowH);

            using (var p = new Pen(Color.FromArgb(45, 45, 45)))
                g.DrawLine(p, 0, rowY + rowH - 1, panelW, rowY + rowH - 1);

            int samples = ch.Data.GetLength(1);
            if (samples == 0) return;

            int waveTop = rowY + WAVEFORM_PAD_V;
            int waveBottom = rowY + rowH - WAVEFORM_PAD_V;
            int waveH = waveBottom - waveTop;
            double range = ch.Max - ch.Min;

            DrawAnalogScaleLabels(g, ch.Min, ch.Max, rowY, waveTop, waveBottom);

            if (ch.Min < 0 && ch.Max > 0)
            {
                float zeroY = waveBottom - (float)((0.0 - ch.Min) / range * waveH);
                using (var zp = new Pen(Color.FromArgb(60, 60, 60), 1f)
                { DashStyle = DashStyle.Dash })
                    g.DrawLine(zp, LABEL_WIDTH, zeroY, LABEL_WIDTH + plotW, zeroY);
            }

            // ++ Convert view window → sample indices
            int iStart = (int)Math.Floor(viewStart / TIME_MULTIPLIER);
            int iEnd = (int)Math.Ceiling(viewEnd / TIME_MULTIPLIER);
            iStart = Math.Max(0, Math.Min(iStart, samples - 1));
            iEnd = Math.Max(0, Math.Min(iEnd, samples - 1));

            int visibleSamples = iEnd - iStart;
            if (visibleSamples <= 0) return;
            float xScale = (float)plotW / visibleSamples;

            using (var wavePen = new Pen(GetChannelColor(_analogChannels.IndexOf(ch)), 1.5f))
            {
                float prevX = LABEL_WIDTH;
                float prevY = SampleToY(ch.Data[ch.RowIndex, iStart],
                                        ch.Min, range, waveTop, waveH);

                for (int t = iStart + 1; t <= iEnd; t++)
                {
                    float curX = LABEL_WIDTH + (t - iStart) * xScale;
                    float curY = SampleToY(ch.Data[ch.RowIndex, t],
                                           ch.Min, range, waveTop, waveH);
                    g.DrawLine(wavePen, prevX, prevY, curX, curY);
                    prevX = curX;
                    prevY = curY;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Label drawing
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Draws a two-line label: displayLabel (top, bright) + indexLabel (bottom, dim).
        /// Falls back to a single centred label when there is no custom name.
        /// </summary>
        private static void DrawDescriptorLabel(Graphics g,
                                        string displayLabel,
                                        string secondaryLabel,
                                        int rowY, int rowH)
        {
            var fmt = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };

            // Two-line layout when a secondary label (set/index) exists
            if (!string.IsNullOrEmpty(secondaryLabel))
            {
                float halfH = rowH / 2f;
                var topRect = new RectangleF(2, rowY, LABEL_WIDTH - 8, halfH);
                var botRect = new RectangleF(2, rowY + halfH, LABEL_WIDTH - 8, halfH);

                using (var topBrush = new SolidBrush(Color.White))
                using (var botBrush = new SolidBrush(Color.FromArgb(120, 160, 200)))
                using (var topFont = new Font("Consolas", 8.5f, FontStyle.Bold))
                using (var botFont = new Font("Consolas", 7.5f, FontStyle.Regular))
                {
                    g.DrawString(displayLabel, topFont, topBrush, topRect, fmt); // e.g. CLK
                    g.DrawString(secondaryLabel, botFont, botBrush, botRect, fmt); // e.g. PG/CH 00
                }
            }
            else
            {
                // No custom name — single centred line showing "SETNAME/CH 00"
                var rect = new RectangleF(2, rowY, LABEL_WIDTH - 8, rowH);
                using (var font = new Font("Consolas", 8.5f, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(180, 220, 255)))
                    g.DrawString(displayLabel, font, brush, rect, fmt);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Time axis
        // ════════════════════════════════════════════════════════════════
        private static void DrawTimeAxis(Graphics g, int panelW, int panelH,
                                 int plotW, double viewStart, double viewEnd) // ++ view window
        {
            double viewRange = viewEnd - viewStart;
            if (viewRange <= 0) return;

            int axisTop = panelH - AXIS_HEIGHT;
            float plotX0 = LABEL_WIDTH;
            float plotX1 = LABEL_WIDTH + plotW;

            using (var bgBrush = new SolidBrush(Color.FromArgb(15, 15, 15)))
                g.FillRectangle(bgBrush, 0, axisTop, panelW, AXIS_HEIGHT);

            using (var axisPen = new Pen(Color.FromArgb(100, 100, 120), 1f))
                g.DrawLine(axisPen, plotX0, axisTop, plotX1, axisTop);

            using (var unitFont = new Font("Segoe UI", 8f, FontStyle.Italic))
            using (var unitBrush = new SolidBrush(Color.FromArgb(140, 150, 180)))
            {
                var unitRect = new RectangleF(0, axisTop, LABEL_WIDTH - 4, AXIS_HEIGHT);
                var unitFmt = new StringFormat
                {
                    Alignment = StringAlignment.Far,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(TIME_UNIT, unitFont, unitBrush, unitRect, unitFmt);
            }

            const int MIN_TICK_PX = 80;
            int maxTicks = Math.Max(1, plotW / MIN_TICK_PX);
            double niceInterval = NiceInterval(viewRange / maxTicks); // ++ based on viewRange

            const float LABEL_HALF_W = 40f;
            const float MIN_LABEL_W = 20f;

            using (var tickPen = new Pen(Color.FromArgb(100, 100, 120), 1f))
            using (var gridPen = new Pen(Color.FromArgb(35, 35, 50), 1f))
            using (var labelFont = new Font("Consolas", 7.5f))
            using (var lblBrush = new SolidBrush(Color.FromArgb(170, 180, 210)))
            {
                // ++ First tick at or after viewStart
                double firstTick = Math.Ceiling(viewStart / niceInterval) * niceInterval;

                for (double t = firstTick; t <= viewEnd + niceInterval * 0.5; t += niceInterval)
                {
                    if (t < viewStart || t > viewEnd + niceInterval * 0.5) continue;

                    // ++ Map time value within the view window to pixel x
                    float x = plotX0 + (float)((t - viewStart) / viewRange) * plotW;
                    if (x < plotX0 || x > plotX1 + 1f) continue;

                    g.DrawLine(gridPen, x, 0, x, axisTop);
                    g.DrawLine(tickPen, x, axisTop, x, axisTop + 5);

                    float rawLeft = x - LABEL_HALF_W;
                    float rawRight = x + LABEL_HALF_W;
                    float clampedLeft = Math.Max(rawLeft, plotX0);
                    float clampedRight = Math.Min(rawRight, plotX1);
                    float clampedW = clampedRight - clampedLeft;
                    if (clampedW < MIN_LABEL_W) continue;

                    var lblFmt = new StringFormat { Alignment = StringAlignment.Center };
                    if (rawLeft < plotX0) lblFmt.Alignment = StringAlignment.Near;
                    else if (rawRight > plotX1) lblFmt.Alignment = StringAlignment.Far;

                    g.DrawString(t.ToString("G4"), labelFont, lblBrush,
                        new RectangleF(clampedLeft, axisTop + 6, clampedW, 20), lblFmt);
                }
            }
        }

        private static double NiceInterval(double raw)
        {
            if (raw <= 0) return 1;
            double exp = Math.Floor(Math.Log10(raw));
            double mag = Math.Pow(10, exp);
            double frac = raw / mag;
            double niceFrac = frac <= 1 ? 1 : frac <= 2 ? 2 : frac <= 5 ? 5 : 10;
            return niceFrac * mag;
        }

        // ════════════════════════════════════════════════════════════════
        //  Pure helpers
        // ════════════════════════════════════════════════════════════════
        private static int[] GetVisibleIndices(bool[] visible, int count)
        {
            var list = new List<int>(count);
            for (int i = 0; i < count; i++) if (visible[i]) list.Add(i);
            return list.ToArray();
        }

        private static bool GetBit(uint word, int ch) => ((word >> ch) & 1u) == 1u;

        private static float SampleToY(double value, double min, double range,
                                       int waveTop, int waveH)
        {
            float norm = (float)((value - min) / range);
            return waveTop + waveH * (1f - norm);
        }

        private static void DrawAnalogScaleLabels(Graphics g, double min, double max,
                                                  int rowY, int waveTop, int waveBottom)
        {
            using (var font = new Font("Consolas", 7f))
            using (var brush = new SolidBrush(Color.FromArgb(110, 130, 150)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Far };
                g.DrawString(max.ToString("G4"), font, brush,
                    new RectangleF(LABEL_WIDTH + 2, waveTop, 60, 14), fmt);
                g.DrawString(min.ToString("G4"), font, brush,
                    new RectangleF(LABEL_WIDTH + 2, waveBottom - 14, 60, 14), fmt);
            }
        }

        private static Color GetChannelColor(int globalIndex)
        {
            Color[] palette =
            {
                Color.FromArgb(100, 220, 255),
                Color.FromArgb(120, 255, 120),
                Color.FromArgb(255, 200,  80),
                Color.FromArgb(255, 100, 100),
                Color.FromArgb(200, 150, 255),
                Color.FromArgb(255, 180, 100),
                Color.FromArgb(100, 255, 200),
                Color.FromArgb(255, 255, 120),
            };
            return palette[globalIndex % palette.Length];
        }

        private int MaxDigitalSamples()
        {
            int max = 0;
            foreach (var ch in _digitalChannels)
                if (ch.Data.Length > max) max = ch.Data.Length;
            return max;
        }

        private int MaxAnalogSamples()
        {
            int max = 0;
            foreach (var ch in _analogChannels)
            {
                int s = ch.Data.GetLength(1);
                if (s > max) max = s;
            }
            return max;
        }
    }
}