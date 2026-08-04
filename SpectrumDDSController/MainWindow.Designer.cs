namespace SpectrumDDSController
{
    partial class MainWindow
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabs = new System.Windows.Forms.TabControl();

            // -- status tab ------------------------------------------------------
            this.statusTab = new System.Windows.Forms.TabPage();
            this.identityBox = new System.Windows.Forms.TextBox();
            this.liveStatusBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.statusTimer = new System.Windows.Forms.Timer(this.components);

            // -- pattern tab -----------------------------------------------------
            this.patternTab = new System.Windows.Forms.TabPage();
            this.patternGrid = new System.Windows.Forms.DataGridView();
            this.frequencyGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.amplitudeGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.patternSplit = new System.Windows.Forms.SplitContainer();
            this.graphSplit = new System.Windows.Forms.SplitContainer();
            this.patternButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.loadButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.addEventButton = new System.Windows.Forms.Button();
            this.removeEventButton = new System.Windows.Forms.Button();
            this.applyPatternButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.testTriggerButton = new System.Windows.Forms.Button();
            this.patternInfoLabel = new System.Windows.Forms.Label();

            // -- manual tab -------------------------------------------------------
            this.manualTab = new System.Windows.Forms.TabPage();
            this.manualLayout = new System.Windows.Forms.TableLayoutPanel();
            this.clampLabel = new System.Windows.Forms.Label();
            this.outputLevelBox = new System.Windows.Forms.NumericUpDown();
            this.outputLevelLabel = new System.Windows.Forms.Label();
            this.silenceButton = new System.Windows.Forms.Button();

            this.tabs.SuspendLayout();
            this.statusTab.SuspendLayout();
            this.patternTab.SuspendLayout();
            this.manualTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.patternGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frequencyGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.amplitudeGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.patternSplit)).BeginInit();
            this.patternSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphSplit)).BeginInit();
            this.graphSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.outputLevelBox)).BeginInit();
            this.SuspendLayout();

            // tabs
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Controls.Add(this.statusTab);
            this.tabs.Controls.Add(this.patternTab);
            this.tabs.Controls.Add(this.manualTab);
            this.tabs.Name = "tabs";

            // -- status tab --------------------------------------------------------
            this.statusTab.Text = "Status";
            this.statusTab.Padding = new System.Windows.Forms.Padding(8);
            this.statusTab.UseVisualStyleBackColor = true;

            this.liveStatusBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.liveStatusBox.Multiline = true;
            this.liveStatusBox.ReadOnly = true;
            this.liveStatusBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.liveStatusBox.Font = new System.Drawing.Font("Consolas", 9F);
            this.liveStatusBox.Name = "liveStatusBox";

            this.identityBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.identityBox.Multiline = true;
            this.identityBox.ReadOnly = true;
            this.identityBox.Height = 150;
            this.identityBox.Font = new System.Drawing.Font("Consolas", 9F);
            this.identityBox.Name = "identityBox";

            this.connectButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.connectButton.Height = 32;
            this.connectButton.Text = "Open card";
            this.connectButton.Name = "connectButton";
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);

            this.statusTab.Controls.Add(this.liveStatusBox);
            this.statusTab.Controls.Add(this.identityBox);
            this.statusTab.Controls.Add(this.connectButton);

            this.statusTimer.Interval = 250;
            this.statusTimer.Tick += new System.EventHandler(this.statusTimer_Tick);

            // -- pattern tab --------------------------------------------------------
            this.patternTab.Text = "Pattern";
            this.patternTab.Padding = new System.Windows.Forms.Padding(4);
            this.patternTab.UseVisualStyleBackColor = true;

            this.patternSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patternSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.patternSplit.SplitterDistance = 220;
            this.patternSplit.Name = "patternSplit";

            this.patternGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patternGrid.AllowUserToAddRows = false;
            this.patternGrid.AllowUserToDeleteRows = false;
            this.patternGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.patternGrid.Name = "patternGrid";
            this.patternGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.patternGrid_CellEndEdit);

            this.graphSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.graphSplit.Name = "graphSplit";

            this.frequencyGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.frequencyGraph.Name = "frequencyGraph";
            this.amplitudeGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.amplitudeGraph.Name = "amplitudeGraph";

            this.graphSplit.Panel1.Controls.Add(this.frequencyGraph);
            this.graphSplit.Panel2.Controls.Add(this.amplitudeGraph);

            this.patternButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.patternButtons.Height = 34;
            this.patternButtons.Name = "patternButtons";

            this.loadButton.Text = "Load JSON...";
            this.loadButton.Width = 100;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            this.saveButton.Text = "Save JSON...";
            this.saveButton.Width = 100;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            this.addEventButton.Text = "Add event";
            this.addEventButton.Width = 90;
            this.addEventButton.Click += new System.EventHandler(this.addEventButton_Click);
            this.removeEventButton.Text = "Remove event";
            this.removeEventButton.Width = 105;
            this.removeEventButton.Click += new System.EventHandler(this.removeEventButton_Click);
            this.applyPatternButton.Text = "Load onto card";
            this.applyPatternButton.Width = 110;
            this.applyPatternButton.Click += new System.EventHandler(this.applyPatternButton_Click);
            this.runButton.Text = "Arm for triggers";
            this.runButton.Width = 110;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            this.stopButton.Text = "Stop";
            this.stopButton.Width = 60;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            this.testTriggerButton.Text = "Test trigger";
            this.testTriggerButton.Width = 90;
            this.testTriggerButton.Click += new System.EventHandler(this.testTriggerButton_Click);
            this.patternInfoLabel.AutoSize = true;
            this.patternInfoLabel.Padding = new System.Windows.Forms.Padding(8, 8, 0, 0);

            this.patternButtons.Controls.Add(this.loadButton);
            this.patternButtons.Controls.Add(this.saveButton);
            this.patternButtons.Controls.Add(this.addEventButton);
            this.patternButtons.Controls.Add(this.removeEventButton);
            this.patternButtons.Controls.Add(this.applyPatternButton);
            this.patternButtons.Controls.Add(this.runButton);
            this.patternButtons.Controls.Add(this.stopButton);
            this.patternButtons.Controls.Add(this.testTriggerButton);
            this.patternButtons.Controls.Add(this.patternInfoLabel);

            this.patternSplit.Panel1.Controls.Add(this.patternGrid);
            this.patternSplit.Panel1.Controls.Add(this.patternButtons);
            this.patternSplit.Panel2.Controls.Add(this.graphSplit);
            this.patternTab.Controls.Add(this.patternSplit);

            // -- manual tab ----------------------------------------------------------
            this.manualTab.Text = "Manual";
            this.manualTab.Padding = new System.Windows.Forms.Padding(8);
            this.manualTab.UseVisualStyleBackColor = true;

            this.manualLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualLayout.ColumnCount = 6;
            this.manualLayout.RowCount = 6;
            this.manualLayout.Name = "manualLayout";

            this.clampLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.clampLabel.Height = 44;
            this.clampLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.clampLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.clampLabel.ForeColor = System.Drawing.Color.DarkRed;
            this.clampLabel.Name = "clampLabel";

            this.outputLevelLabel.Text = "Output level (mV)";
            this.outputLevelLabel.AutoSize = true;
            this.outputLevelBox.Minimum = 80;
            this.outputLevelBox.Maximum = 2500;
            this.outputLevelBox.Increment = 50;
            this.outputLevelBox.Name = "outputLevelBox";
            this.outputLevelBox.ValueChanged += new System.EventHandler(this.outputLevelBox_ValueChanged);

            this.silenceButton.Text = "Silence all outputs";
            this.silenceButton.Width = 140;
            this.silenceButton.Height = 30;
            this.silenceButton.Click += new System.EventHandler(this.silenceButton_Click);

            this.manualTab.Controls.Add(this.manualLayout);
            this.manualTab.Controls.Add(this.clampLabel);

            // -- form -------------------------------------------------------------------
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.tabs);
            this.Name = "MainWindow";
            this.Text = "Spectrum DDS controller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);

            ((System.ComponentModel.ISupportInitialize)(this.outputLevelBox)).EndInit();
            this.graphSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.graphSplit)).EndInit();
            this.patternSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.patternSplit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.amplitudeGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frequencyGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.patternGrid)).EndInit();
            this.manualTab.ResumeLayout(false);
            this.patternTab.ResumeLayout(false);
            this.statusTab.ResumeLayout(false);
            this.statusTab.PerformLayout();
            this.tabs.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabs;

        private System.Windows.Forms.TabPage statusTab;
        private System.Windows.Forms.TextBox identityBox;
        private System.Windows.Forms.TextBox liveStatusBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Timer statusTimer;

        private System.Windows.Forms.TabPage patternTab;
        private System.Windows.Forms.SplitContainer patternSplit;
        private System.Windows.Forms.SplitContainer graphSplit;
        private System.Windows.Forms.DataGridView patternGrid;
        private NationalInstruments.UI.WindowsForms.ScatterGraph frequencyGraph;
        private NationalInstruments.UI.WindowsForms.ScatterGraph amplitudeGraph;
        private System.Windows.Forms.FlowLayoutPanel patternButtons;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button addEventButton;
        private System.Windows.Forms.Button removeEventButton;
        private System.Windows.Forms.Button applyPatternButton;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button testTriggerButton;
        private System.Windows.Forms.Label patternInfoLabel;

        private System.Windows.Forms.TabPage manualTab;
        private System.Windows.Forms.TableLayoutPanel manualLayout;
        private System.Windows.Forms.Label clampLabel;
        private System.Windows.Forms.NumericUpDown outputLevelBox;
        private System.Windows.Forms.Label outputLevelLabel;
        private System.Windows.Forms.Button silenceButton;
    }
}
