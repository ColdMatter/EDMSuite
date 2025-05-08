using System.Windows.Forms;
using System.Drawing;

namespace NeanderthalDDSController

{
    partial class NeanderthalForm : Form
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button_add = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_eventName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_ch0_freq = new System.Windows.Forms.TextBox();
            this.textBox_ch1_freq = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_ch2_freq = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_ch3_freq = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_ch3_amp = new System.Windows.Forms.TextBox();
            this.textBox_ch2_amp = new System.Windows.Forms.TextBox();
            this.textBox_ch1_amp = new System.Windows.Forms.TextBox();
            this.textBox_ch0_amp = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_ch3_freq_slope = new System.Windows.Forms.TextBox();
            this.textBox_ch2_freq_slope = new System.Windows.Forms.TextBox();
            this.textBox_ch1_freq_slope = new System.Windows.Forms.TextBox();
            this.textBox_ch0_freq_slope = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_ch3_amp_slope = new System.Windows.Forms.TextBox();
            this.textBox_ch2_amp_slope = new System.Windows.Forms.TextBox();
            this.textBox_ch1_amp_slope = new System.Windows.Forms.TextBox();
            this.textBox_ch0_amp_slope = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_eventTime = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.patternGridView = new System.Windows.Forms.DataGridView();
            this.dataGridEventName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh0Freq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh1Freq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh2Freq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh3Freq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh0Amp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh1Amp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh2Amp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh3Amp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh0FreqSlpoe = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh1FreqSlope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh2FreqSlope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh3FreqSlope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh0AmpSlope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh1AmpSlope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh2AmpSlope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridCh3AmpSlope = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label11 = new System.Windows.Forms.Label();
            this.buttonStartPattern = new System.Windows.Forms.Button();
            this.buttonStopPattern = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.textBoxPatternLength = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSavePattern = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripLoadPattern = new System.Windows.Forms.ToolStripMenuItem();
            this.label13 = new System.Windows.Forms.Label();
            this.lablePatternName = new System.Windows.Forms.Label();
            this.labelRunIndicator = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.patternGridView)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_add
            // 
            this.button_add.Location = new System.Drawing.Point(556, 68);
            this.button_add.Name = "button_add";
            this.button_add.Size = new System.Drawing.Size(64, 20);
            this.button_add.TabIndex = 1;
            this.button_add.Text = "Add";
            this.button_add.UseVisualStyleBackColor = true;
            this.button_add.Click += new System.EventHandler(this.button_add_click);
            // 
            // button_delete
            // 
            this.button_delete.Location = new System.Drawing.Point(556, 94);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(64, 20);
            this.button_delete.TabIndex = 2;
            this.button_delete.Text = "Delete";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_Delete_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Event Name";
            // 
            // textBox_eventName
            // 
            this.textBox_eventName.Location = new System.Drawing.Point(10, 68);
            this.textBox_eventName.Name = "textBox_eventName";
            this.textBox_eventName.Size = new System.Drawing.Size(151, 20);
            this.textBox_eventName.TabIndex = 4;
            this.textBox_eventName.Text = "PatternStart";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(190, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Frequency (MHz)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(165, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Ch0";
            // 
            // textBox_ch0_freq
            // 
            this.textBox_ch0_freq.Location = new System.Drawing.Point(195, 68);
            this.textBox_ch0_freq.Name = "textBox_ch0_freq";
            this.textBox_ch0_freq.Size = new System.Drawing.Size(73, 20);
            this.textBox_ch0_freq.TabIndex = 7;
            this.textBox_ch0_freq.Text = "100";
            // 
            // textBox_ch1_freq
            // 
            this.textBox_ch1_freq.Location = new System.Drawing.Point(195, 94);
            this.textBox_ch1_freq.Name = "textBox_ch1_freq";
            this.textBox_ch1_freq.Size = new System.Drawing.Size(73, 20);
            this.textBox_ch1_freq.TabIndex = 9;
            this.textBox_ch1_freq.Text = "100";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(165, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Ch1";
            // 
            // textBox_ch2_freq
            // 
            this.textBox_ch2_freq.Location = new System.Drawing.Point(195, 119);
            this.textBox_ch2_freq.Name = "textBox_ch2_freq";
            this.textBox_ch2_freq.Size = new System.Drawing.Size(73, 20);
            this.textBox_ch2_freq.TabIndex = 11;
            this.textBox_ch2_freq.Text = "100";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(165, 121);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Ch2";
            // 
            // textBox_ch3_freq
            // 
            this.textBox_ch3_freq.Location = new System.Drawing.Point(195, 144);
            this.textBox_ch3_freq.Name = "textBox_ch3_freq";
            this.textBox_ch3_freq.Size = new System.Drawing.Size(73, 20);
            this.textBox_ch3_freq.TabIndex = 13;
            this.textBox_ch3_freq.Text = "100";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(165, 146);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(26, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Ch3";
            // 
            // textBox_ch3_amp
            // 
            this.textBox_ch3_amp.Location = new System.Drawing.Point(287, 144);
            this.textBox_ch3_amp.Name = "textBox_ch3_amp";
            this.textBox_ch3_amp.Size = new System.Drawing.Size(54, 20);
            this.textBox_ch3_amp.TabIndex = 18;
            this.textBox_ch3_amp.Text = "1";
            // 
            // textBox_ch2_amp
            // 
            this.textBox_ch2_amp.Location = new System.Drawing.Point(287, 119);
            this.textBox_ch2_amp.Name = "textBox_ch2_amp";
            this.textBox_ch2_amp.Size = new System.Drawing.Size(54, 20);
            this.textBox_ch2_amp.TabIndex = 17;
            this.textBox_ch2_amp.Text = "1";
            // 
            // textBox_ch1_amp
            // 
            this.textBox_ch1_amp.Location = new System.Drawing.Point(287, 94);
            this.textBox_ch1_amp.Name = "textBox_ch1_amp";
            this.textBox_ch1_amp.Size = new System.Drawing.Size(54, 20);
            this.textBox_ch1_amp.TabIndex = 16;
            this.textBox_ch1_amp.Text = "1";
            // 
            // textBox_ch0_amp
            // 
            this.textBox_ch0_amp.Location = new System.Drawing.Point(287, 68);
            this.textBox_ch0_amp.Name = "textBox_ch0_amp";
            this.textBox_ch0_amp.Size = new System.Drawing.Size(54, 20);
            this.textBox_ch0_amp.TabIndex = 15;
            this.textBox_ch0_amp.Text = "1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(287, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Amplitude";
            // 
            // textBox_ch3_freq_slope
            // 
            this.textBox_ch3_freq_slope.Location = new System.Drawing.Point(371, 144);
            this.textBox_ch3_freq_slope.Name = "textBox_ch3_freq_slope";
            this.textBox_ch3_freq_slope.Size = new System.Drawing.Size(73, 20);
            this.textBox_ch3_freq_slope.TabIndex = 23;
            this.textBox_ch3_freq_slope.Text = "0";
            // 
            // textBox_ch2_freq_slope
            // 
            this.textBox_ch2_freq_slope.Location = new System.Drawing.Point(371, 119);
            this.textBox_ch2_freq_slope.Name = "textBox_ch2_freq_slope";
            this.textBox_ch2_freq_slope.Size = new System.Drawing.Size(73, 20);
            this.textBox_ch2_freq_slope.TabIndex = 22;
            this.textBox_ch2_freq_slope.Text = "0";
            // 
            // textBox_ch1_freq_slope
            // 
            this.textBox_ch1_freq_slope.Location = new System.Drawing.Point(371, 94);
            this.textBox_ch1_freq_slope.Name = "textBox_ch1_freq_slope";
            this.textBox_ch1_freq_slope.Size = new System.Drawing.Size(73, 20);
            this.textBox_ch1_freq_slope.TabIndex = 21;
            this.textBox_ch1_freq_slope.Text = "0";
            // 
            // textBox_ch0_freq_slope
            // 
            this.textBox_ch0_freq_slope.Location = new System.Drawing.Point(371, 68);
            this.textBox_ch0_freq_slope.Name = "textBox_ch0_freq_slope";
            this.textBox_ch0_freq_slope.Size = new System.Drawing.Size(73, 20);
            this.textBox_ch0_freq_slope.TabIndex = 20;
            this.textBox_ch0_freq_slope.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(367, 53);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Frequency Slpoe";
            // 
            // textBox_ch3_amp_slope
            // 
            this.textBox_ch3_amp_slope.Location = new System.Drawing.Point(477, 144);
            this.textBox_ch3_amp_slope.Name = "textBox_ch3_amp_slope";
            this.textBox_ch3_amp_slope.Size = new System.Drawing.Size(54, 20);
            this.textBox_ch3_amp_slope.TabIndex = 28;
            this.textBox_ch3_amp_slope.Text = "0";
            // 
            // textBox_ch2_amp_slope
            // 
            this.textBox_ch2_amp_slope.Location = new System.Drawing.Point(477, 119);
            this.textBox_ch2_amp_slope.Name = "textBox_ch2_amp_slope";
            this.textBox_ch2_amp_slope.Size = new System.Drawing.Size(54, 20);
            this.textBox_ch2_amp_slope.TabIndex = 27;
            this.textBox_ch2_amp_slope.Text = "0";
            // 
            // textBox_ch1_amp_slope
            // 
            this.textBox_ch1_amp_slope.Location = new System.Drawing.Point(477, 94);
            this.textBox_ch1_amp_slope.Name = "textBox_ch1_amp_slope";
            this.textBox_ch1_amp_slope.Size = new System.Drawing.Size(54, 20);
            this.textBox_ch1_amp_slope.TabIndex = 26;
            this.textBox_ch1_amp_slope.Text = "0";
            // 
            // textBox_ch0_amp_slope
            // 
            this.textBox_ch0_amp_slope.Location = new System.Drawing.Point(477, 68);
            this.textBox_ch0_amp_slope.Name = "textBox_ch0_amp_slope";
            this.textBox_ch0_amp_slope.Size = new System.Drawing.Size(54, 20);
            this.textBox_ch0_amp_slope.TabIndex = 25;
            this.textBox_ch0_amp_slope.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(462, 53);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(83, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Amplitude Slpoe";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 96);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(83, 13);
            this.label10.TabIndex = 29;
            this.label10.Text = "Event Time (ms)";
            // 
            // textBox_eventTime
            // 
            this.textBox_eventTime.Location = new System.Drawing.Point(10, 112);
            this.textBox_eventTime.Name = "textBox_eventTime";
            this.textBox_eventTime.Size = new System.Drawing.Size(72, 20);
            this.textBox_eventTime.TabIndex = 30;
            this.textBox_eventTime.Text = "0";
            // 
            // timer1
            // 
            this.timer1.Interval = 200;
            // 
            // patternGridView
            // 
            this.patternGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.patternGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridEventName,
            this.dataGridTime,
            this.dataGridCh0Freq,
            this.dataGridCh1Freq,
            this.dataGridCh2Freq,
            this.dataGridCh3Freq,
            this.dataGridCh0Amp,
            this.dataGridCh1Amp,
            this.dataGridCh2Amp,
            this.dataGridCh3Amp,
            this.dataGridCh0FreqSlpoe,
            this.dataGridCh1FreqSlope,
            this.dataGridCh2FreqSlope,
            this.dataGridCh3FreqSlope,
            this.dataGridCh0AmpSlope,
            this.dataGridCh1AmpSlope,
            this.dataGridCh2AmpSlope,
            this.dataGridCh3AmpSlope});
            this.patternGridView.Location = new System.Drawing.Point(10, 169);
            this.patternGridView.Name = "patternGridView";
            this.patternGridView.Size = new System.Drawing.Size(1014, 218);
            this.patternGridView.TabIndex = 31;
            // 
            // dataGridEventName
            // 
            this.dataGridEventName.HeaderText = "Event Name";
            this.dataGridEventName.Name = "dataGridEventName";
            this.dataGridEventName.ReadOnly = true;
            this.dataGridEventName.Width = 120;
            // 
            // dataGridTime
            // 
            this.dataGridTime.HeaderText = "Time (ms)";
            this.dataGridTime.Name = "dataGridTime";
            this.dataGridTime.ReadOnly = true;
            this.dataGridTime.Width = 60;
            // 
            // dataGridCh0Freq
            // 
            this.dataGridCh0Freq.HeaderText = "Ch0 Freq";
            this.dataGridCh0Freq.Name = "dataGridCh0Freq";
            this.dataGridCh0Freq.ReadOnly = true;
            this.dataGridCh0Freq.Width = 60;
            // 
            // dataGridCh1Freq
            // 
            this.dataGridCh1Freq.HeaderText = "Ch1 Freq";
            this.dataGridCh1Freq.Name = "dataGridCh1Freq";
            this.dataGridCh1Freq.ReadOnly = true;
            this.dataGridCh1Freq.Width = 60;
            // 
            // dataGridCh2Freq
            // 
            this.dataGridCh2Freq.HeaderText = "Ch2 Freq";
            this.dataGridCh2Freq.Name = "dataGridCh2Freq";
            this.dataGridCh2Freq.ReadOnly = true;
            this.dataGridCh2Freq.Width = 60;
            // 
            // dataGridCh3Freq
            // 
            this.dataGridCh3Freq.HeaderText = "Ch3 Freq";
            this.dataGridCh3Freq.Name = "dataGridCh3Freq";
            this.dataGridCh3Freq.ReadOnly = true;
            this.dataGridCh3Freq.Width = 60;
            // 
            // dataGridCh0Amp
            // 
            this.dataGridCh0Amp.HeaderText = "Ch0 Amp";
            this.dataGridCh0Amp.Name = "dataGridCh0Amp";
            this.dataGridCh0Amp.ReadOnly = true;
            this.dataGridCh0Amp.Width = 60;
            // 
            // dataGridCh1Amp
            // 
            this.dataGridCh1Amp.HeaderText = "Ch1 Amp";
            this.dataGridCh1Amp.Name = "dataGridCh1Amp";
            this.dataGridCh1Amp.ReadOnly = true;
            this.dataGridCh1Amp.Width = 60;
            // 
            // dataGridCh2Amp
            // 
            this.dataGridCh2Amp.HeaderText = "Ch2 Amp";
            this.dataGridCh2Amp.Name = "dataGridCh2Amp";
            this.dataGridCh2Amp.ReadOnly = true;
            this.dataGridCh2Amp.Width = 60;
            // 
            // dataGridCh3Amp
            // 
            this.dataGridCh3Amp.HeaderText = "Ch3 Amp";
            this.dataGridCh3Amp.Name = "dataGridCh3Amp";
            this.dataGridCh3Amp.ReadOnly = true;
            this.dataGridCh3Amp.Width = 60;
            // 
            // dataGridCh0FreqSlpoe
            // 
            this.dataGridCh0FreqSlpoe.HeaderText = "Ch0 Freq Slope";
            this.dataGridCh0FreqSlpoe.Name = "dataGridCh0FreqSlpoe";
            this.dataGridCh0FreqSlpoe.ReadOnly = true;
            this.dataGridCh0FreqSlpoe.Width = 60;
            // 
            // dataGridCh1FreqSlope
            // 
            this.dataGridCh1FreqSlope.HeaderText = "Ch1 Freq Slope";
            this.dataGridCh1FreqSlope.Name = "dataGridCh1FreqSlope";
            this.dataGridCh1FreqSlope.ReadOnly = true;
            this.dataGridCh1FreqSlope.Width = 60;
            // 
            // dataGridCh2FreqSlope
            // 
            this.dataGridCh2FreqSlope.HeaderText = "Ch2 Freq Slope";
            this.dataGridCh2FreqSlope.Name = "dataGridCh2FreqSlope";
            this.dataGridCh2FreqSlope.ReadOnly = true;
            this.dataGridCh2FreqSlope.Width = 60;
            // 
            // dataGridCh3FreqSlope
            // 
            this.dataGridCh3FreqSlope.HeaderText = "Ch3 Freq Slope";
            this.dataGridCh3FreqSlope.Name = "dataGridCh3FreqSlope";
            this.dataGridCh3FreqSlope.ReadOnly = true;
            this.dataGridCh3FreqSlope.Width = 60;
            // 
            // dataGridCh0AmpSlope
            // 
            this.dataGridCh0AmpSlope.HeaderText = "Ch0 Amp Slope";
            this.dataGridCh0AmpSlope.Name = "dataGridCh0AmpSlope";
            this.dataGridCh0AmpSlope.ReadOnly = true;
            this.dataGridCh0AmpSlope.Width = 60;
            // 
            // dataGridCh1AmpSlope
            // 
            this.dataGridCh1AmpSlope.HeaderText = "Ch1 Amp Slope";
            this.dataGridCh1AmpSlope.Name = "dataGridCh1AmpSlope";
            this.dataGridCh1AmpSlope.ReadOnly = true;
            this.dataGridCh1AmpSlope.Width = 60;
            // 
            // dataGridCh2AmpSlope
            // 
            this.dataGridCh2AmpSlope.HeaderText = "Ch2 Amp Slope";
            this.dataGridCh2AmpSlope.Name = "dataGridCh2AmpSlope";
            this.dataGridCh2AmpSlope.ReadOnly = true;
            this.dataGridCh2AmpSlope.Width = 60;
            // 
            // dataGridCh3AmpSlope
            // 
            this.dataGridCh3AmpSlope.HeaderText = "Ch3 Amp Slope";
            this.dataGridCh3AmpSlope.Name = "dataGridCh3AmpSlope";
            this.dataGridCh3AmpSlope.ReadOnly = true;
            this.dataGridCh3AmpSlope.Width = 60;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(556, 144);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(307, 13);
            this.label11.TabIndex = 32;
            this.label11.Text = "All frequencies in MHz, amplitudes between 0 to 1, slope per ms";
            // 
            // buttonStartPattern
            // 
            this.buttonStartPattern.Location = new System.Drawing.Point(645, 68);
            this.buttonStartPattern.Name = "buttonStartPattern";
            this.buttonStartPattern.Size = new System.Drawing.Size(93, 20);
            this.buttonStartPattern.TabIndex = 33;
            this.buttonStartPattern.Text = "Start Pattern";
            this.buttonStartPattern.UseVisualStyleBackColor = true;
            this.buttonStartPattern.Click += new System.EventHandler(this.button_start_pattern_clicked);
            // 
            // buttonStopPattern
            // 
            this.buttonStopPattern.Location = new System.Drawing.Point(645, 93);
            this.buttonStopPattern.Name = "buttonStopPattern";
            this.buttonStopPattern.Size = new System.Drawing.Size(93, 20);
            this.buttonStopPattern.TabIndex = 34;
            this.buttonStopPattern.Text = "Stop Pattern";
            this.buttonStopPattern.UseVisualStyleBackColor = true;
            this.buttonStopPattern.Click += new System.EventHandler(this.button_stop_pattern_clicked);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(911, 25);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(99, 13);
            this.label12.TabIndex = 35;
            this.label12.Text = "Pattern Length (ms)";
            // 
            // textBoxPatternLength
            // 
            this.textBoxPatternLength.Location = new System.Drawing.Point(921, 46);
            this.textBoxPatternLength.Name = "textBoxPatternLength";
            this.textBoxPatternLength.Size = new System.Drawing.Size(72, 20);
            this.textBoxPatternLength.TabIndex = 36;
            this.textBoxPatternLength.Text = "300";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1048, 24);
            this.menuStrip1.TabIndex = 37;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            this.menuToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSavePattern,
            this.toolStripLoadPattern});
            this.menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            this.menuToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.menuToolStripMenuItem.Text = "Menu";
            // 
            // toolStripSavePattern
            // 
            this.toolStripSavePattern.Name = "toolStripSavePattern";
            this.toolStripSavePattern.Size = new System.Drawing.Size(180, 22);
            this.toolStripSavePattern.Text = "Save Pattern";
            this.toolStripSavePattern.Click += new System.EventHandler(this.save_pattern_clicked);
            // 
            // toolStripLoadPattern
            // 
            this.toolStripLoadPattern.Name = "toolStripLoadPattern";
            this.toolStripLoadPattern.Size = new System.Drawing.Size(180, 22);
            this.toolStripLoadPattern.Text = "Load Pattern";
            this.toolStripLoadPattern.Click += new System.EventHandler(this.load_pattern_clicked);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(11, 25);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(75, 13);
            this.label13.TabIndex = 38;
            this.label13.Text = "Pattern in use:";
            // 
            // lablePatternName
            // 
            this.lablePatternName.AutoSize = true;
            this.lablePatternName.Location = new System.Drawing.Point(87, 25);
            this.lablePatternName.Name = "lablePatternName";
            this.lablePatternName.Size = new System.Drawing.Size(33, 13);
            this.lablePatternName.TabIndex = 39;
            this.lablePatternName.Text = "None";
            // 
            // labelRunIndicator
            // 
            this.labelRunIndicator.AutoSize = true;
            this.labelRunIndicator.ForeColor = System.Drawing.Color.Red;
            this.labelRunIndicator.Location = new System.Drawing.Point(659, 121);
            this.labelRunIndicator.Name = "labelRunIndicator";
            this.labelRunIndicator.Size = new System.Drawing.Size(84, 13);
            this.labelRunIndicator.TabIndex = 40;
            this.labelRunIndicator.Text = "Pattern Stopped";
            this.labelRunIndicator.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // NeanderthalForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1048, 398);
            this.Controls.Add(this.labelRunIndicator);
            this.Controls.Add(this.lablePatternName);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.textBoxPatternLength);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.buttonStopPattern);
            this.Controls.Add(this.buttonStartPattern);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.patternGridView);
            this.Controls.Add(this.textBox_eventTime);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textBox_ch3_amp_slope);
            this.Controls.Add(this.textBox_ch2_amp_slope);
            this.Controls.Add(this.textBox_ch1_amp_slope);
            this.Controls.Add(this.textBox_ch0_amp_slope);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBox_ch3_freq_slope);
            this.Controls.Add(this.textBox_ch2_freq_slope);
            this.Controls.Add(this.textBox_ch1_freq_slope);
            this.Controls.Add(this.textBox_ch0_freq_slope);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox_ch3_amp);
            this.Controls.Add(this.textBox_ch2_amp);
            this.Controls.Add(this.textBox_ch1_amp);
            this.Controls.Add(this.textBox_ch0_amp);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_ch3_freq);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_ch2_freq);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_ch1_freq);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_ch0_freq);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_eventName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.button_add);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "NeanderthalForm";
            this.Text = "NeanderthalDDSController";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NeanderthalForm_FormClosing);
            this.Load += new System.EventHandler(this.NeanderthalForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.patternGridView)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private TextBox textBox_eventName;
        private Label label2;
        private Label label3;
        private TextBox textBox_ch0_freq;
        private TextBox textBox_ch1_freq;
        private Label label4;
        private TextBox textBox_ch2_freq;
        private Label label5;
        private TextBox textBox_ch3_freq;
        private Label label6;
        private TextBox textBox_ch3_amp;
        private TextBox textBox_ch2_amp;
        private TextBox textBox_ch1_amp;
        private TextBox textBox_ch0_amp;
        private Label label7;
        private TextBox textBox_ch3_freq_slope;
        private TextBox textBox_ch2_freq_slope;
        private TextBox textBox_ch1_freq_slope;
        private TextBox textBox_ch0_freq_slope;
        private Label label8;
        private TextBox textBox_ch3_amp_slope;
        private TextBox textBox_ch2_amp_slope;
        private TextBox textBox_ch1_amp_slope;
        private TextBox textBox_ch0_amp_slope;
        private Label label9;
        private Label label10;
        private TextBox textBox_eventTime;
        private System.Windows.Forms.Timer timer1;
        private DataGridView patternGridView;
        private Label label11;
        private Button buttonStartPattern;
        private Button buttonStopPattern;
        private Label label12;
        private TextBox textBoxPatternLength;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuToolStripMenuItem;
        private ToolStripMenuItem toolStripSavePattern;
        private ToolStripMenuItem toolStripLoadPattern;
        private Label label13;
        private Label lablePatternName;
        private DataGridViewTextBoxColumn dataGridEventName;
        private DataGridViewTextBoxColumn dataGridTime;
        private DataGridViewTextBoxColumn dataGridCh0Freq;
        private DataGridViewTextBoxColumn dataGridCh1Freq;
        private DataGridViewTextBoxColumn dataGridCh2Freq;
        private DataGridViewTextBoxColumn dataGridCh3Freq;
        private DataGridViewTextBoxColumn dataGridCh0Amp;
        private DataGridViewTextBoxColumn dataGridCh1Amp;
        private DataGridViewTextBoxColumn dataGridCh2Amp;
        private DataGridViewTextBoxColumn dataGridCh3Amp;
        private DataGridViewTextBoxColumn dataGridCh0FreqSlpoe;
        private DataGridViewTextBoxColumn dataGridCh1FreqSlope;
        private DataGridViewTextBoxColumn dataGridCh2FreqSlope;
        private DataGridViewTextBoxColumn dataGridCh3FreqSlope;
        private DataGridViewTextBoxColumn dataGridCh0AmpSlope;
        private DataGridViewTextBoxColumn dataGridCh1AmpSlope;
        private DataGridViewTextBoxColumn dataGridCh2AmpSlope;
        private DataGridViewTextBoxColumn dataGridCh3AmpSlope;
        private Label labelRunIndicator;
    }
}
