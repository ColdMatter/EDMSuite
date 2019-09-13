namespace RfArbitraryWaveformGenerator
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.configurationGroupBox = new System.Windows.Forms.GroupBox();
            this.powerLevelLabel = new System.Windows.Forms.Label();
            this.powerLevelNumeric = new System.Windows.Forms.NumericUpDown();
            this.trigger2SourceLabel = new System.Windows.Forms.Label();
            this.trigger2SourceComboBox = new System.Windows.Forms.ComboBox();
            this.frequencyLabel = new System.Windows.Forms.Label();
            this.frequencyNumeric = new System.Windows.Forms.NumericUpDown();
            this.trigger1TypeComboBox = new System.Windows.Forms.ComboBox();
            this.trigger1SourceLabel = new System.Windows.Forms.Label();
            this.trigger1TypeLabel = new System.Windows.Forms.Label();
            this.trigger1SourceComboBox = new System.Windows.Forms.ComboBox();
            this.trigger2TypeComboBox = new System.Windows.Forms.ComboBox();
            this.trigger2TypeLabel = new System.Windows.Forms.Label();
            this.pulseMakerGroupBox = new System.Windows.Forms.GroupBox();
            this.a3Label = new System.Windows.Forms.Label();
            this.a3TextBox = new System.Windows.Forms.TextBox();
            this.pulseLabel = new System.Windows.Forms.Label();
            this.pulseNameTextBox = new System.Windows.Forms.TextBox();
            this.savePulseButton = new System.Windows.Forms.Button();
            this.pmTextBox = new System.Windows.Forms.TextBox();
            this.fmTextBox = new System.Windows.Forms.TextBox();
            this.a2Label = new System.Windows.Forms.Label();
            this.a2TextBox = new System.Windows.Forms.TextBox();
            this.a1Label = new System.Windows.Forms.Label();
            this.a1TextBox = new System.Windows.Forms.TextBox();
            this.a0Label = new System.Windows.Forms.Label();
            this.a0TextBox = new System.Windows.Forms.TextBox();
            this.pmLabel = new System.Windows.Forms.Label();
            this.fmLabel = new System.Windows.Forms.Label();
            this.amLabel = new System.Windows.Forms.Label();
            this.signalBandwidthLabel = new System.Windows.Forms.Label();
            this.signalBandwidthNumeric = new System.Windows.Forms.NumericUpDown();
            this.pulseDurationLabel = new System.Windows.Forms.Label();
            this.pulseDurationNumeric = new System.Windows.Forms.NumericUpDown();
            this.scriptGroupBox = new System.Windows.Forms.GroupBox();
            this.scriptLabel = new System.Windows.Forms.Label();
            this.scriptTextBox = new System.Windows.Forms.TextBox();
            this.scriptFileTextBox = new System.Windows.Forms.TextBox();
            this.scriptFileButton = new System.Windows.Forms.Button();
            this.scriptFileLabel = new System.Windows.Forms.Label();
            this.pulseChoiceGroupBox = new System.Windows.Forms.GroupBox();
            this.selectPulsesButton = new System.Windows.Forms.Button();
            this.rf2StartTimeTextBox = new System.Windows.Forms.TextBox();
            this.rf2StartTimeLabel = new System.Windows.Forms.Label();
            this.rf1StartTimeTextBox = new System.Windows.Forms.TextBox();
            this.rf1StartTimeLabel = new System.Windows.Forms.Label();
            this.rf2TotalSamplesLabel = new System.Windows.Forms.Label();
            this.rf2TotalSamplesTextBox = new System.Windows.Forms.TextBox();
            this.rf2PulseComboBox = new System.Windows.Forms.ComboBox();
            this.rf2PulseLabel = new System.Windows.Forms.Label();
            this.rf2PulseDurationTextBox = new System.Windows.Forms.TextBox();
            this.rf2PulseDurationLabel = new System.Windows.Forms.Label();
            this.rf1TotalSamplesLabel = new System.Windows.Forms.Label();
            this.rf1TotalSamplesTextBox = new System.Windows.Forms.TextBox();
            this.rf1PulseComboBox = new System.Windows.Forms.ComboBox();
            this.rf1PulseLabel = new System.Windows.Forms.Label();
            this.rf1PulseDurationTextBox = new System.Windows.Forms.TextBox();
            this.rf1PulseDurationLabel = new System.Windows.Forms.Label();
            this.statusGroupBox = new System.Windows.Forms.GroupBox();
            this.actualSampleRateLabel = new System.Windows.Forms.Label();
            this.actualSampleRateTextBox = new System.Windows.Forms.TextBox();
            this.errorMessagesGroupBox = new System.Windows.Forms.GroupBox();
            this.errorMessagesTextBox = new System.Windows.Forms.TextBox();
            this.resourceNameLabel = new System.Windows.Forms.Label();
            this.resourceNameTextBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.rfsgStatusTimer = new System.Windows.Forms.Timer(this.components);
            this.pulseDirectoryLabel = new System.Windows.Forms.Label();
            this.pulseDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.pulseDirectoryButton = new System.Windows.Forms.Button();
            this.loadPulsesButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.configurationGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.powerLevelNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frequencyNumeric)).BeginInit();
            this.pulseMakerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.signalBandwidthNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pulseDurationNumeric)).BeginInit();
            this.scriptGroupBox.SuspendLayout();
            this.pulseChoiceGroupBox.SuspendLayout();
            this.statusGroupBox.SuspendLayout();
            this.errorMessagesGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.configurationGroupBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pulseMakerGroupBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.scriptGroupBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.pulseChoiceGroupBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.statusGroupBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.errorMessagesGroupBox, 1, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 41);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 28.30957F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.86558F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46.02851F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1017, 491);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // configurationGroupBox
            // 
            this.configurationGroupBox.Controls.Add(this.powerLevelLabel);
            this.configurationGroupBox.Controls.Add(this.powerLevelNumeric);
            this.configurationGroupBox.Controls.Add(this.trigger2SourceLabel);
            this.configurationGroupBox.Controls.Add(this.trigger2SourceComboBox);
            this.configurationGroupBox.Controls.Add(this.frequencyLabel);
            this.configurationGroupBox.Controls.Add(this.frequencyNumeric);
            this.configurationGroupBox.Controls.Add(this.trigger1TypeComboBox);
            this.configurationGroupBox.Controls.Add(this.trigger1SourceLabel);
            this.configurationGroupBox.Controls.Add(this.trigger1TypeLabel);
            this.configurationGroupBox.Controls.Add(this.trigger1SourceComboBox);
            this.configurationGroupBox.Controls.Add(this.trigger2TypeComboBox);
            this.configurationGroupBox.Controls.Add(this.trigger2TypeLabel);
            this.configurationGroupBox.Location = new System.Drawing.Point(3, 3);
            this.configurationGroupBox.Name = "configurationGroupBox";
            this.configurationGroupBox.Size = new System.Drawing.Size(502, 132);
            this.configurationGroupBox.TabIndex = 0;
            this.configurationGroupBox.TabStop = false;
            this.configurationGroupBox.Text = "Configuration";
            // 
            // powerLevelLabel
            // 
            this.powerLevelLabel.AutoSize = true;
            this.powerLevelLabel.Location = new System.Drawing.Point(6, 61);
            this.powerLevelLabel.Name = "powerLevelLabel";
            this.powerLevelLabel.Size = new System.Drawing.Size(92, 13);
            this.powerLevelLabel.TabIndex = 3;
            this.powerLevelLabel.Text = "Power level (dBm)";
            // 
            // powerLevelNumeric
            // 
            this.powerLevelNumeric.Location = new System.Drawing.Point(9, 78);
            this.powerLevelNumeric.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.powerLevelNumeric.Name = "powerLevelNumeric";
            this.powerLevelNumeric.Size = new System.Drawing.Size(120, 20);
            this.powerLevelNumeric.TabIndex = 2;
            this.powerLevelNumeric.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // trigger2SourceLabel
            // 
            this.trigger2SourceLabel.AutoSize = true;
            this.trigger2SourceLabel.Location = new System.Drawing.Point(287, 61);
            this.trigger2SourceLabel.Name = "trigger2SourceLabel";
            this.trigger2SourceLabel.Size = new System.Drawing.Size(84, 13);
            this.trigger2SourceLabel.TabIndex = 19;
            this.trigger2SourceLabel.Text = "Trigger 2 source";
            // 
            // trigger2SourceComboBox
            // 
            this.trigger2SourceComboBox.FormattingEnabled = true;
            this.trigger2SourceComboBox.Location = new System.Drawing.Point(290, 77);
            this.trigger2SourceComboBox.Name = "trigger2SourceComboBox";
            this.trigger2SourceComboBox.Size = new System.Drawing.Size(120, 21);
            this.trigger2SourceComboBox.TabIndex = 18;
            // 
            // frequencyLabel
            // 
            this.frequencyLabel.AutoSize = true;
            this.frequencyLabel.Location = new System.Drawing.Point(6, 18);
            this.frequencyLabel.Name = "frequencyLabel";
            this.frequencyLabel.Size = new System.Drawing.Size(110, 13);
            this.frequencyLabel.TabIndex = 1;
            this.frequencyLabel.Text = "Center frequency (Hz)";
            // 
            // frequencyNumeric
            // 
            this.frequencyNumeric.Location = new System.Drawing.Point(9, 35);
            this.frequencyNumeric.Maximum = new decimal(new int[] {
            -1594967296,
            0,
            0,
            0});
            this.frequencyNumeric.Name = "frequencyNumeric";
            this.frequencyNumeric.Size = new System.Drawing.Size(120, 20);
            this.frequencyNumeric.TabIndex = 0;
            this.frequencyNumeric.Value = new decimal(new int[] {
            170000000,
            0,
            0,
            0});
            // 
            // trigger1TypeComboBox
            // 
            this.trigger1TypeComboBox.FormattingEnabled = true;
            this.trigger1TypeComboBox.Location = new System.Drawing.Point(150, 38);
            this.trigger1TypeComboBox.Name = "trigger1TypeComboBox";
            this.trigger1TypeComboBox.Size = new System.Drawing.Size(120, 21);
            this.trigger1TypeComboBox.TabIndex = 12;
            // 
            // trigger1SourceLabel
            // 
            this.trigger1SourceLabel.AutoSize = true;
            this.trigger1SourceLabel.Location = new System.Drawing.Point(287, 22);
            this.trigger1SourceLabel.Name = "trigger1SourceLabel";
            this.trigger1SourceLabel.Size = new System.Drawing.Size(84, 13);
            this.trigger1SourceLabel.TabIndex = 17;
            this.trigger1SourceLabel.Text = "Trigger 1 source";
            // 
            // trigger1TypeLabel
            // 
            this.trigger1TypeLabel.AutoSize = true;
            this.trigger1TypeLabel.Location = new System.Drawing.Point(147, 22);
            this.trigger1TypeLabel.Name = "trigger1TypeLabel";
            this.trigger1TypeLabel.Size = new System.Drawing.Size(72, 13);
            this.trigger1TypeLabel.TabIndex = 13;
            this.trigger1TypeLabel.Text = "Trigger 1 type";
            // 
            // trigger1SourceComboBox
            // 
            this.trigger1SourceComboBox.FormattingEnabled = true;
            this.trigger1SourceComboBox.Location = new System.Drawing.Point(290, 38);
            this.trigger1SourceComboBox.Name = "trigger1SourceComboBox";
            this.trigger1SourceComboBox.Size = new System.Drawing.Size(120, 21);
            this.trigger1SourceComboBox.TabIndex = 16;
            // 
            // trigger2TypeComboBox
            // 
            this.trigger2TypeComboBox.FormattingEnabled = true;
            this.trigger2TypeComboBox.Location = new System.Drawing.Point(150, 77);
            this.trigger2TypeComboBox.Name = "trigger2TypeComboBox";
            this.trigger2TypeComboBox.Size = new System.Drawing.Size(120, 21);
            this.trigger2TypeComboBox.TabIndex = 14;
            // 
            // trigger2TypeLabel
            // 
            this.trigger2TypeLabel.AutoSize = true;
            this.trigger2TypeLabel.Location = new System.Drawing.Point(147, 61);
            this.trigger2TypeLabel.Name = "trigger2TypeLabel";
            this.trigger2TypeLabel.Size = new System.Drawing.Size(72, 13);
            this.trigger2TypeLabel.TabIndex = 15;
            this.trigger2TypeLabel.Text = "Trigger 2 type";
            // 
            // pulseMakerGroupBox
            // 
            this.pulseMakerGroupBox.Controls.Add(this.a3Label);
            this.pulseMakerGroupBox.Controls.Add(this.a3TextBox);
            this.pulseMakerGroupBox.Controls.Add(this.pulseLabel);
            this.pulseMakerGroupBox.Controls.Add(this.pulseNameTextBox);
            this.pulseMakerGroupBox.Controls.Add(this.savePulseButton);
            this.pulseMakerGroupBox.Controls.Add(this.pmTextBox);
            this.pulseMakerGroupBox.Controls.Add(this.fmTextBox);
            this.pulseMakerGroupBox.Controls.Add(this.a2Label);
            this.pulseMakerGroupBox.Controls.Add(this.a2TextBox);
            this.pulseMakerGroupBox.Controls.Add(this.a1Label);
            this.pulseMakerGroupBox.Controls.Add(this.a1TextBox);
            this.pulseMakerGroupBox.Controls.Add(this.a0Label);
            this.pulseMakerGroupBox.Controls.Add(this.a0TextBox);
            this.pulseMakerGroupBox.Controls.Add(this.pmLabel);
            this.pulseMakerGroupBox.Controls.Add(this.fmLabel);
            this.pulseMakerGroupBox.Controls.Add(this.amLabel);
            this.pulseMakerGroupBox.Controls.Add(this.signalBandwidthLabel);
            this.pulseMakerGroupBox.Controls.Add(this.signalBandwidthNumeric);
            this.pulseMakerGroupBox.Controls.Add(this.pulseDurationLabel);
            this.pulseMakerGroupBox.Controls.Add(this.pulseDurationNumeric);
            this.pulseMakerGroupBox.Location = new System.Drawing.Point(3, 141);
            this.pulseMakerGroupBox.Name = "pulseMakerGroupBox";
            this.pulseMakerGroupBox.Size = new System.Drawing.Size(502, 120);
            this.pulseMakerGroupBox.TabIndex = 1;
            this.pulseMakerGroupBox.TabStop = false;
            this.pulseMakerGroupBox.Text = "Pulse maker";
            // 
            // a3Label
            // 
            this.a3Label.AutoSize = true;
            this.a3Label.Location = new System.Drawing.Point(17, 97);
            this.a3Label.Name = "a3Label";
            this.a3Label.Size = new System.Drawing.Size(19, 13);
            this.a3Label.TabIndex = 30;
            this.a3Label.Text = "a3";
            // 
            // a3TextBox
            // 
            this.a3TextBox.Location = new System.Drawing.Point(42, 94);
            this.a3TextBox.Name = "a3TextBox";
            this.a3TextBox.Size = new System.Drawing.Size(45, 20);
            this.a3TextBox.TabIndex = 29;
            this.a3TextBox.Text = "0";
            // 
            // pulseLabel
            // 
            this.pulseLabel.AutoSize = true;
            this.pulseLabel.Location = new System.Drawing.Point(385, 21);
            this.pulseLabel.Name = "pulseLabel";
            this.pulseLabel.Size = new System.Drawing.Size(62, 13);
            this.pulseLabel.TabIndex = 28;
            this.pulseLabel.Text = "Pulse name";
            // 
            // pulseNameTextBox
            // 
            this.pulseNameTextBox.Location = new System.Drawing.Point(388, 37);
            this.pulseNameTextBox.Name = "pulseNameTextBox";
            this.pulseNameTextBox.Size = new System.Drawing.Size(75, 20);
            this.pulseNameTextBox.TabIndex = 27;
            this.pulseNameTextBox.Text = "pulse12302";
            // 
            // savePulseButton
            // 
            this.savePulseButton.Location = new System.Drawing.Point(388, 74);
            this.savePulseButton.Name = "savePulseButton";
            this.savePulseButton.Size = new System.Drawing.Size(75, 23);
            this.savePulseButton.TabIndex = 26;
            this.savePulseButton.Text = "&Save pulse";
            this.savePulseButton.UseVisualStyleBackColor = true;
            this.savePulseButton.Click += new System.EventHandler(this.savePulseButton_Click);
            // 
            // pmTextBox
            // 
            this.pmTextBox.Location = new System.Drawing.Point(135, 77);
            this.pmTextBox.Name = "pmTextBox";
            this.pmTextBox.Size = new System.Drawing.Size(45, 20);
            this.pmTextBox.TabIndex = 18;
            this.pmTextBox.Text = "0";
            // 
            // fmTextBox
            // 
            this.fmTextBox.Location = new System.Drawing.Point(135, 38);
            this.fmTextBox.Name = "fmTextBox";
            this.fmTextBox.Size = new System.Drawing.Size(45, 20);
            this.fmTextBox.TabIndex = 17;
            this.fmTextBox.Text = "0";
            // 
            // a2Label
            // 
            this.a2Label.AutoSize = true;
            this.a2Label.Location = new System.Drawing.Point(17, 77);
            this.a2Label.Name = "a2Label";
            this.a2Label.Size = new System.Drawing.Size(19, 13);
            this.a2Label.TabIndex = 16;
            this.a2Label.Text = "a2";
            // 
            // a2TextBox
            // 
            this.a2TextBox.Location = new System.Drawing.Point(42, 74);
            this.a2TextBox.Name = "a2TextBox";
            this.a2TextBox.Size = new System.Drawing.Size(45, 20);
            this.a2TextBox.TabIndex = 15;
            this.a2TextBox.Text = "0";
            // 
            // a1Label
            // 
            this.a1Label.AutoSize = true;
            this.a1Label.Location = new System.Drawing.Point(17, 60);
            this.a1Label.Name = "a1Label";
            this.a1Label.Size = new System.Drawing.Size(19, 13);
            this.a1Label.TabIndex = 14;
            this.a1Label.Text = "a1";
            // 
            // a1TextBox
            // 
            this.a1TextBox.Location = new System.Drawing.Point(42, 57);
            this.a1TextBox.Name = "a1TextBox";
            this.a1TextBox.Size = new System.Drawing.Size(45, 20);
            this.a1TextBox.TabIndex = 13;
            this.a1TextBox.Text = "0";
            // 
            // a0Label
            // 
            this.a0Label.AutoSize = true;
            this.a0Label.Location = new System.Drawing.Point(17, 42);
            this.a0Label.Name = "a0Label";
            this.a0Label.Size = new System.Drawing.Size(19, 13);
            this.a0Label.TabIndex = 12;
            this.a0Label.Text = "a0";
            // 
            // a0TextBox
            // 
            this.a0TextBox.Location = new System.Drawing.Point(42, 39);
            this.a0TextBox.Name = "a0TextBox";
            this.a0TextBox.Size = new System.Drawing.Size(45, 20);
            this.a0TextBox.TabIndex = 11;
            this.a0TextBox.Text = "1.0";
            // 
            // pmLabel
            // 
            this.pmLabel.AutoSize = true;
            this.pmLabel.Location = new System.Drawing.Point(118, 60);
            this.pmLabel.Name = "pmLabel";
            this.pmLabel.Size = new System.Drawing.Size(84, 13);
            this.pmLabel.TabIndex = 10;
            this.pmLabel.Text = "Phase step (rad)";
            // 
            // fmLabel
            // 
            this.fmLabel.AutoSize = true;
            this.fmLabel.Location = new System.Drawing.Point(111, 22);
            this.fmLabel.Name = "fmLabel";
            this.fmLabel.Size = new System.Drawing.Size(102, 13);
            this.fmLabel.TabIndex = 9;
            this.fmLabel.Text = "Frequency step (Hz)";
            // 
            // amLabel
            // 
            this.amLabel.AutoSize = true;
            this.amLabel.Location = new System.Drawing.Point(8, 22);
            this.amLabel.Name = "amLabel";
            this.amLabel.Size = new System.Drawing.Size(93, 13);
            this.amLabel.TabIndex = 8;
            this.amLabel.Text = "Amplitude shaping";
            // 
            // signalBandwidthLabel
            // 
            this.signalBandwidthLabel.AutoSize = true;
            this.signalBandwidthLabel.Location = new System.Drawing.Point(223, 61);
            this.signalBandwidthLabel.Name = "signalBandwidthLabel";
            this.signalBandwidthLabel.Size = new System.Drawing.Size(110, 13);
            this.signalBandwidthLabel.TabIndex = 7;
            this.signalBandwidthLabel.Text = "Signal bandwidth (Hz)";
            // 
            // signalBandwidthNumeric
            // 
            this.signalBandwidthNumeric.Location = new System.Drawing.Point(226, 78);
            this.signalBandwidthNumeric.Maximum = new decimal(new int[] {
            80000000,
            0,
            0,
            0});
            this.signalBandwidthNumeric.Name = "signalBandwidthNumeric";
            this.signalBandwidthNumeric.Size = new System.Drawing.Size(120, 20);
            this.signalBandwidthNumeric.TabIndex = 6;
            // 
            // pulseDurationLabel
            // 
            this.pulseDurationLabel.AutoSize = true;
            this.pulseDurationLabel.Location = new System.Drawing.Point(223, 21);
            this.pulseDurationLabel.Name = "pulseDurationLabel";
            this.pulseDurationLabel.Size = new System.Drawing.Size(94, 13);
            this.pulseDurationLabel.TabIndex = 5;
            this.pulseDurationLabel.Text = "Pulse duration (us)";
            // 
            // pulseDurationNumeric
            // 
            this.pulseDurationNumeric.Location = new System.Drawing.Point(226, 38);
            this.pulseDurationNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.pulseDurationNumeric.Name = "pulseDurationNumeric";
            this.pulseDurationNumeric.Size = new System.Drawing.Size(120, 20);
            this.pulseDurationNumeric.TabIndex = 4;
            this.pulseDurationNumeric.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // scriptGroupBox
            // 
            this.scriptGroupBox.Controls.Add(this.scriptLabel);
            this.scriptGroupBox.Controls.Add(this.scriptTextBox);
            this.scriptGroupBox.Controls.Add(this.scriptFileTextBox);
            this.scriptGroupBox.Controls.Add(this.scriptFileButton);
            this.scriptGroupBox.Controls.Add(this.scriptFileLabel);
            this.scriptGroupBox.Location = new System.Drawing.Point(3, 267);
            this.scriptGroupBox.Name = "scriptGroupBox";
            this.scriptGroupBox.Size = new System.Drawing.Size(502, 221);
            this.scriptGroupBox.TabIndex = 2;
            this.scriptGroupBox.TabStop = false;
            this.scriptGroupBox.Text = "Script";
            // 
            // scriptLabel
            // 
            this.scriptLabel.AutoSize = true;
            this.scriptLabel.Location = new System.Drawing.Point(6, 49);
            this.scriptLabel.Name = "scriptLabel";
            this.scriptLabel.Size = new System.Drawing.Size(34, 13);
            this.scriptLabel.TabIndex = 8;
            this.scriptLabel.Text = "Script";
            // 
            // scriptTextBox
            // 
            this.scriptTextBox.Location = new System.Drawing.Point(62, 49);
            this.scriptTextBox.Multiline = true;
            this.scriptTextBox.Name = "scriptTextBox";
            this.scriptTextBox.ReadOnly = true;
            this.scriptTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.scriptTextBox.Size = new System.Drawing.Size(434, 179);
            this.scriptTextBox.TabIndex = 7;
            // 
            // scriptFileTextBox
            // 
            this.scriptFileTextBox.Location = new System.Drawing.Point(62, 22);
            this.scriptFileTextBox.Name = "scriptFileTextBox";
            this.scriptFileTextBox.Size = new System.Drawing.Size(246, 20);
            this.scriptFileTextBox.TabIndex = 5;
            this.scriptFileTextBox.Text = "..\\..\\wfm0.txt";
            // 
            // scriptFileButton
            // 
            this.scriptFileButton.Location = new System.Drawing.Point(314, 20);
            this.scriptFileButton.Name = "scriptFileButton";
            this.scriptFileButton.Size = new System.Drawing.Size(32, 23);
            this.scriptFileButton.TabIndex = 6;
            this.scriptFileButton.Text = "...";
            this.scriptFileButton.UseVisualStyleBackColor = true;
            this.scriptFileButton.Click += new System.EventHandler(this.scriptFileButton_Click);
            // 
            // scriptFileLabel
            // 
            this.scriptFileLabel.AutoSize = true;
            this.scriptFileLabel.Location = new System.Drawing.Point(6, 25);
            this.scriptFileLabel.Name = "scriptFileLabel";
            this.scriptFileLabel.Size = new System.Drawing.Size(50, 13);
            this.scriptFileLabel.TabIndex = 0;
            this.scriptFileLabel.Text = "Script file";
            // 
            // pulseChoiceGroupBox
            // 
            this.pulseChoiceGroupBox.Controls.Add(this.selectPulsesButton);
            this.pulseChoiceGroupBox.Controls.Add(this.rf2StartTimeTextBox);
            this.pulseChoiceGroupBox.Controls.Add(this.rf2StartTimeLabel);
            this.pulseChoiceGroupBox.Controls.Add(this.rf1StartTimeTextBox);
            this.pulseChoiceGroupBox.Controls.Add(this.rf1StartTimeLabel);
            this.pulseChoiceGroupBox.Controls.Add(this.rf2TotalSamplesLabel);
            this.pulseChoiceGroupBox.Controls.Add(this.rf2TotalSamplesTextBox);
            this.pulseChoiceGroupBox.Controls.Add(this.rf2PulseComboBox);
            this.pulseChoiceGroupBox.Controls.Add(this.rf2PulseLabel);
            this.pulseChoiceGroupBox.Controls.Add(this.rf2PulseDurationTextBox);
            this.pulseChoiceGroupBox.Controls.Add(this.rf2PulseDurationLabel);
            this.pulseChoiceGroupBox.Controls.Add(this.rf1TotalSamplesLabel);
            this.pulseChoiceGroupBox.Controls.Add(this.rf1TotalSamplesTextBox);
            this.pulseChoiceGroupBox.Controls.Add(this.rf1PulseComboBox);
            this.pulseChoiceGroupBox.Controls.Add(this.rf1PulseLabel);
            this.pulseChoiceGroupBox.Controls.Add(this.rf1PulseDurationTextBox);
            this.pulseChoiceGroupBox.Controls.Add(this.rf1PulseDurationLabel);
            this.pulseChoiceGroupBox.Location = new System.Drawing.Point(511, 3);
            this.pulseChoiceGroupBox.Name = "pulseChoiceGroupBox";
            this.pulseChoiceGroupBox.Size = new System.Drawing.Size(503, 132);
            this.pulseChoiceGroupBox.TabIndex = 3;
            this.pulseChoiceGroupBox.TabStop = false;
            this.pulseChoiceGroupBox.Text = "Pulse chooser";
            // 
            // selectPulsesButton
            // 
            this.selectPulsesButton.Location = new System.Drawing.Point(167, 104);
            this.selectPulsesButton.Name = "selectPulsesButton";
            this.selectPulsesButton.Size = new System.Drawing.Size(126, 23);
            this.selectPulsesButton.TabIndex = 26;
            this.selectPulsesButton.Text = "&Select pulses";
            this.selectPulsesButton.UseVisualStyleBackColor = true;
            this.selectPulsesButton.Click += new System.EventHandler(this.selectPulsesButton_Click);
            // 
            // rf2StartTimeTextBox
            // 
            this.rf2StartTimeTextBox.Location = new System.Drawing.Point(130, 79);
            this.rf2StartTimeTextBox.Name = "rf2StartTimeTextBox";
            this.rf2StartTimeTextBox.Size = new System.Drawing.Size(99, 20);
            this.rf2StartTimeTextBox.TabIndex = 32;
            this.rf2StartTimeTextBox.Text = "1725";
            // 
            // rf2StartTimeLabel
            // 
            this.rf2StartTimeLabel.AutoSize = true;
            this.rf2StartTimeLabel.Location = new System.Drawing.Point(127, 62);
            this.rf2StartTimeLabel.Name = "rf2StartTimeLabel";
            this.rf2StartTimeLabel.Size = new System.Drawing.Size(84, 13);
            this.rf2StartTimeLabel.TabIndex = 31;
            this.rf2StartTimeLabel.Text = "rf2 start time (us)";
            // 
            // rf1StartTimeTextBox
            // 
            this.rf1StartTimeTextBox.Location = new System.Drawing.Point(130, 34);
            this.rf1StartTimeTextBox.Name = "rf1StartTimeTextBox";
            this.rf1StartTimeTextBox.Size = new System.Drawing.Size(99, 20);
            this.rf1StartTimeTextBox.TabIndex = 30;
            this.rf1StartTimeTextBox.Text = "800";
            // 
            // rf1StartTimeLabel
            // 
            this.rf1StartTimeLabel.AutoSize = true;
            this.rf1StartTimeLabel.Location = new System.Drawing.Point(127, 17);
            this.rf1StartTimeLabel.Name = "rf1StartTimeLabel";
            this.rf1StartTimeLabel.Size = new System.Drawing.Size(84, 13);
            this.rf1StartTimeLabel.TabIndex = 29;
            this.rf1StartTimeLabel.Text = "rf1 start time (us)";
            // 
            // rf2TotalSamplesLabel
            // 
            this.rf2TotalSamplesLabel.AutoSize = true;
            this.rf2TotalSamplesLabel.Location = new System.Drawing.Point(372, 61);
            this.rf2TotalSamplesLabel.Name = "rf2TotalSamplesLabel";
            this.rf2TotalSamplesLabel.Size = new System.Drawing.Size(82, 13);
            this.rf2TotalSamplesLabel.TabIndex = 27;
            this.rf2TotalSamplesLabel.Text = "rf2 # of samples";
            // 
            // rf2TotalSamplesTextBox
            // 
            this.rf2TotalSamplesTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.rf2TotalSamplesTextBox.Enabled = false;
            this.rf2TotalSamplesTextBox.Location = new System.Drawing.Point(375, 78);
            this.rf2TotalSamplesTextBox.Name = "rf2TotalSamplesTextBox";
            this.rf2TotalSamplesTextBox.Size = new System.Drawing.Size(120, 20);
            this.rf2TotalSamplesTextBox.TabIndex = 26;
            this.rf2TotalSamplesTextBox.Text = "0";
            // 
            // rf2PulseComboBox
            // 
            this.rf2PulseComboBox.FormattingEnabled = true;
            this.rf2PulseComboBox.Location = new System.Drawing.Point(15, 78);
            this.rf2PulseComboBox.Name = "rf2PulseComboBox";
            this.rf2PulseComboBox.Size = new System.Drawing.Size(104, 21);
            this.rf2PulseComboBox.TabIndex = 22;
            this.rf2PulseComboBox.SelectedIndexChanged += new System.EventHandler(this.rf2PulseComboBox_SelectedIndexChanged);
            // 
            // rf2PulseLabel
            // 
            this.rf2PulseLabel.AutoSize = true;
            this.rf2PulseLabel.Location = new System.Drawing.Point(12, 62);
            this.rf2PulseLabel.Name = "rf2PulseLabel";
            this.rf2PulseLabel.Size = new System.Drawing.Size(47, 13);
            this.rf2PulseLabel.TabIndex = 23;
            this.rf2PulseLabel.Text = "rf2 pulse";
            // 
            // rf2PulseDurationTextBox
            // 
            this.rf2PulseDurationTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.rf2PulseDurationTextBox.Enabled = false;
            this.rf2PulseDurationTextBox.Location = new System.Drawing.Point(242, 79);
            this.rf2PulseDurationTextBox.Name = "rf2PulseDurationTextBox";
            this.rf2PulseDurationTextBox.Size = new System.Drawing.Size(120, 20);
            this.rf2PulseDurationTextBox.TabIndex = 24;
            this.rf2PulseDurationTextBox.Text = "0";
            // 
            // rf2PulseDurationLabel
            // 
            this.rf2PulseDurationLabel.AutoSize = true;
            this.rf2PulseDurationLabel.Location = new System.Drawing.Point(239, 62);
            this.rf2PulseDurationLabel.Name = "rf2PulseDurationLabel";
            this.rf2PulseDurationLabel.Size = new System.Drawing.Size(108, 13);
            this.rf2PulseDurationLabel.TabIndex = 25;
            this.rf2PulseDurationLabel.Text = "rf2 pulse duration (us)";
            // 
            // rf1TotalSamplesLabel
            // 
            this.rf1TotalSamplesLabel.AutoSize = true;
            this.rf1TotalSamplesLabel.Location = new System.Drawing.Point(372, 17);
            this.rf1TotalSamplesLabel.Name = "rf1TotalSamplesLabel";
            this.rf1TotalSamplesLabel.Size = new System.Drawing.Size(82, 13);
            this.rf1TotalSamplesLabel.TabIndex = 21;
            this.rf1TotalSamplesLabel.Text = "rf1 # of samples";
            // 
            // rf1TotalSamplesTextBox
            // 
            this.rf1TotalSamplesTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.rf1TotalSamplesTextBox.Enabled = false;
            this.rf1TotalSamplesTextBox.Location = new System.Drawing.Point(375, 34);
            this.rf1TotalSamplesTextBox.Name = "rf1TotalSamplesTextBox";
            this.rf1TotalSamplesTextBox.Size = new System.Drawing.Size(120, 20);
            this.rf1TotalSamplesTextBox.TabIndex = 20;
            this.rf1TotalSamplesTextBox.Text = "0";
            // 
            // rf1PulseComboBox
            // 
            this.rf1PulseComboBox.FormattingEnabled = true;
            this.rf1PulseComboBox.Location = new System.Drawing.Point(15, 34);
            this.rf1PulseComboBox.Name = "rf1PulseComboBox";
            this.rf1PulseComboBox.Size = new System.Drawing.Size(104, 21);
            this.rf1PulseComboBox.TabIndex = 12;
            this.rf1PulseComboBox.SelectedIndexChanged += new System.EventHandler(this.rf1PulseComboBox_SelectedIndexChanged);
            // 
            // rf1PulseLabel
            // 
            this.rf1PulseLabel.AutoSize = true;
            this.rf1PulseLabel.Location = new System.Drawing.Point(12, 18);
            this.rf1PulseLabel.Name = "rf1PulseLabel";
            this.rf1PulseLabel.Size = new System.Drawing.Size(47, 13);
            this.rf1PulseLabel.TabIndex = 13;
            this.rf1PulseLabel.Text = "rf1 pulse";
            // 
            // rf1PulseDurationTextBox
            // 
            this.rf1PulseDurationTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.rf1PulseDurationTextBox.Enabled = false;
            this.rf1PulseDurationTextBox.Location = new System.Drawing.Point(242, 35);
            this.rf1PulseDurationTextBox.Name = "rf1PulseDurationTextBox";
            this.rf1PulseDurationTextBox.Size = new System.Drawing.Size(120, 20);
            this.rf1PulseDurationTextBox.TabIndex = 14;
            this.rf1PulseDurationTextBox.Text = "0";
            // 
            // rf1PulseDurationLabel
            // 
            this.rf1PulseDurationLabel.AutoSize = true;
            this.rf1PulseDurationLabel.Location = new System.Drawing.Point(239, 18);
            this.rf1PulseDurationLabel.Name = "rf1PulseDurationLabel";
            this.rf1PulseDurationLabel.Size = new System.Drawing.Size(108, 13);
            this.rf1PulseDurationLabel.TabIndex = 15;
            this.rf1PulseDurationLabel.Text = "rf1 pulse duration (us)";
            // 
            // statusGroupBox
            // 
            this.statusGroupBox.Controls.Add(this.actualSampleRateLabel);
            this.statusGroupBox.Controls.Add(this.actualSampleRateTextBox);
            this.statusGroupBox.Location = new System.Drawing.Point(511, 141);
            this.statusGroupBox.Name = "statusGroupBox";
            this.statusGroupBox.Size = new System.Drawing.Size(503, 107);
            this.statusGroupBox.TabIndex = 4;
            this.statusGroupBox.TabStop = false;
            this.statusGroupBox.Text = "Status";
            // 
            // actualSampleRateLabel
            // 
            this.actualSampleRateLabel.AutoSize = true;
            this.actualSampleRateLabel.Location = new System.Drawing.Point(173, 24);
            this.actualSampleRateLabel.Name = "actualSampleRateLabel";
            this.actualSampleRateLabel.Size = new System.Drawing.Size(120, 13);
            this.actualSampleRateLabel.TabIndex = 17;
            this.actualSampleRateLabel.Text = "Actual sample rate (S/s)";
            // 
            // actualSampleRateTextBox
            // 
            this.actualSampleRateTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.actualSampleRateTextBox.Enabled = false;
            this.actualSampleRateTextBox.Location = new System.Drawing.Point(176, 41);
            this.actualSampleRateTextBox.Name = "actualSampleRateTextBox";
            this.actualSampleRateTextBox.Size = new System.Drawing.Size(120, 20);
            this.actualSampleRateTextBox.TabIndex = 16;
            this.actualSampleRateTextBox.Text = "0";
            // 
            // errorMessagesGroupBox
            // 
            this.errorMessagesGroupBox.Controls.Add(this.errorMessagesTextBox);
            this.errorMessagesGroupBox.Location = new System.Drawing.Point(511, 267);
            this.errorMessagesGroupBox.Name = "errorMessagesGroupBox";
            this.errorMessagesGroupBox.Size = new System.Drawing.Size(503, 221);
            this.errorMessagesGroupBox.TabIndex = 5;
            this.errorMessagesGroupBox.TabStop = false;
            this.errorMessagesGroupBox.Text = "Warning/Error messages";
            // 
            // errorMessagesTextBox
            // 
            this.errorMessagesTextBox.Location = new System.Drawing.Point(15, 19);
            this.errorMessagesTextBox.Multiline = true;
            this.errorMessagesTextBox.Name = "errorMessagesTextBox";
            this.errorMessagesTextBox.ReadOnly = true;
            this.errorMessagesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.errorMessagesTextBox.Size = new System.Drawing.Size(478, 209);
            this.errorMessagesTextBox.TabIndex = 9;
            // 
            // resourceNameLabel
            // 
            this.resourceNameLabel.AutoSize = true;
            this.resourceNameLabel.Location = new System.Drawing.Point(10, 15);
            this.resourceNameLabel.Name = "resourceNameLabel";
            this.resourceNameLabel.Size = new System.Drawing.Size(82, 13);
            this.resourceNameLabel.TabIndex = 1;
            this.resourceNameLabel.Text = "Resource name";
            // 
            // resourceNameTextBox
            // 
            this.resourceNameTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.resourceNameTextBox.Enabled = false;
            this.resourceNameTextBox.Location = new System.Drawing.Point(98, 12);
            this.resourceNameTextBox.Name = "resourceNameTextBox";
            this.resourceNameTextBox.Size = new System.Drawing.Size(120, 20);
            this.resourceNameTextBox.TabIndex = 22;
            this.resourceNameTextBox.Text = "PXI1Slot4";
            // 
            // startButton
            // 
            this.startButton.Enabled = false;
            this.startButton.Location = new System.Drawing.Point(847, 9);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 23;
            this.startButton.Text = "St&art";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(928, 9);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 24;
            this.stopButton.Text = "St&op";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // rfsgStatusTimer
            // 
            this.rfsgStatusTimer.Tick += new System.EventHandler(this.rfsgStatusTimer_Tick);
            // 
            // pulseDirectoryLabel
            // 
            this.pulseDirectoryLabel.AutoSize = true;
            this.pulseDirectoryLabel.Location = new System.Drawing.Point(233, 15);
            this.pulseDirectoryLabel.Name = "pulseDirectoryLabel";
            this.pulseDirectoryLabel.Size = new System.Drawing.Size(76, 13);
            this.pulseDirectoryLabel.TabIndex = 22;
            this.pulseDirectoryLabel.Text = "Pulse directory";
            // 
            // pulseDirectoryTextBox
            // 
            this.pulseDirectoryTextBox.Location = new System.Drawing.Point(315, 12);
            this.pulseDirectoryTextBox.Name = "pulseDirectoryTextBox";
            this.pulseDirectoryTextBox.Size = new System.Drawing.Size(246, 20);
            this.pulseDirectoryTextBox.TabIndex = 9;
            this.pulseDirectoryTextBox.Text = ".\\";
            // 
            // pulseDirectoryButton
            // 
            this.pulseDirectoryButton.Location = new System.Drawing.Point(567, 10);
            this.pulseDirectoryButton.Name = "pulseDirectoryButton";
            this.pulseDirectoryButton.Size = new System.Drawing.Size(32, 23);
            this.pulseDirectoryButton.TabIndex = 10;
            this.pulseDirectoryButton.Text = "...";
            this.pulseDirectoryButton.UseVisualStyleBackColor = true;
            this.pulseDirectoryButton.Click += new System.EventHandler(this.pulseDirectoryButton_Click);
            // 
            // loadPulsesButton
            // 
            this.loadPulsesButton.Location = new System.Drawing.Point(766, 9);
            this.loadPulsesButton.Name = "loadPulsesButton";
            this.loadPulsesButton.Size = new System.Drawing.Size(75, 23);
            this.loadPulsesButton.TabIndex = 25;
            this.loadPulsesButton.Text = "&Load pulses";
            this.loadPulsesButton.UseVisualStyleBackColor = true;
            this.loadPulsesButton.Click += new System.EventHandler(this.loadPulsesButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 544);
            this.Controls.Add(this.loadPulsesButton);
            this.Controls.Add(this.pulseDirectoryTextBox);
            this.Controls.Add(this.pulseDirectoryButton);
            this.Controls.Add(this.pulseDirectoryLabel);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.resourceNameTextBox);
            this.Controls.Add(this.resourceNameLabel);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainForm";
            this.Text = "Rf Arbitrary Waveform Generator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.configurationGroupBox.ResumeLayout(false);
            this.configurationGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.powerLevelNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frequencyNumeric)).EndInit();
            this.pulseMakerGroupBox.ResumeLayout(false);
            this.pulseMakerGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.signalBandwidthNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pulseDurationNumeric)).EndInit();
            this.scriptGroupBox.ResumeLayout(false);
            this.scriptGroupBox.PerformLayout();
            this.pulseChoiceGroupBox.ResumeLayout(false);
            this.pulseChoiceGroupBox.PerformLayout();
            this.statusGroupBox.ResumeLayout(false);
            this.statusGroupBox.PerformLayout();
            this.errorMessagesGroupBox.ResumeLayout(false);
            this.errorMessagesGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox configurationGroupBox;
        private System.Windows.Forms.NumericUpDown frequencyNumeric;
        private System.Windows.Forms.Label signalBandwidthLabel;
        private System.Windows.Forms.NumericUpDown signalBandwidthNumeric;
        private System.Windows.Forms.Label pulseDurationLabel;
        private System.Windows.Forms.NumericUpDown pulseDurationNumeric;
        private System.Windows.Forms.Label powerLevelLabel;
        private System.Windows.Forms.NumericUpDown powerLevelNumeric;
        private System.Windows.Forms.Label frequencyLabel;
        private System.Windows.Forms.GroupBox pulseMakerGroupBox;
        private System.Windows.Forms.Label trigger2SourceLabel;
        private System.Windows.Forms.ComboBox trigger2SourceComboBox;
        private System.Windows.Forms.Label trigger1SourceLabel;
        private System.Windows.Forms.ComboBox trigger1SourceComboBox;
        private System.Windows.Forms.Label trigger2TypeLabel;
        private System.Windows.Forms.ComboBox trigger2TypeComboBox;
        private System.Windows.Forms.Label trigger1TypeLabel;
        private System.Windows.Forms.ComboBox trigger1TypeComboBox;
        private System.Windows.Forms.GroupBox scriptGroupBox;
        private System.Windows.Forms.TextBox scriptFileTextBox;
        private System.Windows.Forms.Button scriptFileButton;
        private System.Windows.Forms.Label scriptFileLabel;
        private System.Windows.Forms.Label scriptLabel;
        private System.Windows.Forms.TextBox scriptTextBox;
        private System.Windows.Forms.GroupBox pulseChoiceGroupBox;
        private System.Windows.Forms.Label rf1PulseLabel;
        private System.Windows.Forms.ComboBox rf1PulseComboBox;
        private System.Windows.Forms.Label rf1TotalSamplesLabel;
        private System.Windows.Forms.TextBox rf1TotalSamplesTextBox;
        private System.Windows.Forms.Label actualSampleRateLabel;
        private System.Windows.Forms.TextBox actualSampleRateTextBox;
        private System.Windows.Forms.Label rf1PulseDurationLabel;
        private System.Windows.Forms.TextBox rf1PulseDurationTextBox;
        private System.Windows.Forms.GroupBox statusGroupBox;
        private System.Windows.Forms.GroupBox errorMessagesGroupBox;
        private System.Windows.Forms.TextBox errorMessagesTextBox;
        private System.Windows.Forms.Label resourceNameLabel;
        private System.Windows.Forms.TextBox resourceNameTextBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Timer rfsgStatusTimer;
        private System.Windows.Forms.Label pulseDirectoryLabel;
        private System.Windows.Forms.TextBox pulseDirectoryTextBox;
        private System.Windows.Forms.Button pulseDirectoryButton;
        private System.Windows.Forms.Button loadPulsesButton;
        private System.Windows.Forms.Button savePulseButton;
        private System.Windows.Forms.TextBox pmTextBox;
        private System.Windows.Forms.TextBox fmTextBox;
        private System.Windows.Forms.Label a2Label;
        private System.Windows.Forms.TextBox a2TextBox;
        private System.Windows.Forms.Label a1Label;
        private System.Windows.Forms.TextBox a1TextBox;
        private System.Windows.Forms.Label a0Label;
        private System.Windows.Forms.TextBox a0TextBox;
        private System.Windows.Forms.Label pmLabel;
        private System.Windows.Forms.Label fmLabel;
        private System.Windows.Forms.Label amLabel;
        private System.Windows.Forms.Label pulseLabel;
        private System.Windows.Forms.TextBox pulseNameTextBox;
        private System.Windows.Forms.Label rf2TotalSamplesLabel;
        private System.Windows.Forms.TextBox rf2TotalSamplesTextBox;
        private System.Windows.Forms.ComboBox rf2PulseComboBox;
        private System.Windows.Forms.Label rf2PulseLabel;
        private System.Windows.Forms.TextBox rf2PulseDurationTextBox;
        private System.Windows.Forms.Label rf2PulseDurationLabel;
        private System.Windows.Forms.TextBox rf2StartTimeTextBox;
        private System.Windows.Forms.Label rf2StartTimeLabel;
        private System.Windows.Forms.TextBox rf1StartTimeTextBox;
        private System.Windows.Forms.Label rf1StartTimeLabel;
        private System.Windows.Forms.Button selectPulsesButton;
        private System.Windows.Forms.Label a3Label;
        private System.Windows.Forms.TextBox a3TextBox;
    }
}