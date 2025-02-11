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
            components = new System.ComponentModel.Container();
            button_add = new Button();
            button_delete = new Button();
            label1 = new Label();
            textBox_eventName = new TextBox();
            label2 = new Label();
            label3 = new Label();
            textBox_ch0_freq = new TextBox();
            textBox_ch1_freq = new TextBox();
            label4 = new Label();
            textBox_ch2_freq = new TextBox();
            label5 = new Label();
            textBox_ch3_freq = new TextBox();
            label6 = new Label();
            textBox_ch3_amp = new TextBox();
            textBox_ch2_amp = new TextBox();
            textBox_ch1_amp = new TextBox();
            textBox_ch0_amp = new TextBox();
            label7 = new Label();
            textBox_ch3_freq_slope = new TextBox();
            textBox_ch2_freq_slope = new TextBox();
            textBox_ch1_freq_slope = new TextBox();
            textBox_ch0_freq_slope = new TextBox();
            label8 = new Label();
            textBox_ch3_amp_slope = new TextBox();
            textBox_ch2_amp_slope = new TextBox();
            textBox_ch1_amp_slope = new TextBox();
            textBox_ch0_amp_slope = new TextBox();
            label9 = new Label();
            label10 = new Label();
            textBox_eventTime = new TextBox();
            timer1 = new System.Windows.Forms.Timer(components);
            patternGridView = new DataGridView();
            dataGridEventName = new DataGridViewTextBoxColumn();
            dataGridTime = new DataGridViewTextBoxColumn();
            dataGridCh0Freq = new DataGridViewTextBoxColumn();
            dataGridCh1Freq = new DataGridViewTextBoxColumn();
            dataGridCh2Freq = new DataGridViewTextBoxColumn();
            dataGridCh3Freq = new DataGridViewTextBoxColumn();
            dataGridCh0Amp = new DataGridViewTextBoxColumn();
            dataGridCh1Amp = new DataGridViewTextBoxColumn();
            dataGridCh2Amp = new DataGridViewTextBoxColumn();
            dataGridCh3Amp = new DataGridViewTextBoxColumn();
            dataGridCh0FreqSlpoe = new DataGridViewTextBoxColumn();
            dataGridCh1FreqSlope = new DataGridViewTextBoxColumn();
            dataGridCh2FreqSlope = new DataGridViewTextBoxColumn();
            dataGridCh3FreqSlope = new DataGridViewTextBoxColumn();
            dataGridCh0AmpSlope = new DataGridViewTextBoxColumn();
            dataGridCh1AmpSlope = new DataGridViewTextBoxColumn();
            dataGridCh2AmpSlope = new DataGridViewTextBoxColumn();
            dataGridCh3AmpSlope = new DataGridViewTextBoxColumn();
            label11 = new Label();
            buttonStartPattern = new Button();
            buttonStopPattern = new Button();
            label12 = new Label();
            textBoxPatternLength = new TextBox();
            menuStrip1 = new MenuStrip();
            menuToolStripMenuItem = new ToolStripMenuItem();
            toolStripSavePattern = new ToolStripMenuItem();
            toolStripLoadPattern = new ToolStripMenuItem();
            label13 = new Label();
            lablePatternName = new Label();
            labelRunIndicator = new Label();
            ((System.ComponentModel.ISupportInitialize)patternGridView).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // button_add
            // 
            button_add.Location = new Point(649, 79);
            button_add.Name = "button_add";
            button_add.Size = new Size(75, 23);
            button_add.TabIndex = 1;
            button_add.Text = "Add";
            button_add.UseVisualStyleBackColor = true;
            button_add.Click += button_add_click;
            // 
            // button_delete
            // 
            button_delete.Location = new Point(649, 108);
            button_delete.Name = "button_delete";
            button_delete.Size = new Size(75, 23);
            button_delete.TabIndex = 2;
            button_delete.Text = "Delete";
            button_delete.UseVisualStyleBackColor = true;
            button_delete.Click += button_Delete_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 61);
            label1.Name = "label1";
            label1.Size = new Size(71, 15);
            label1.TabIndex = 3;
            label1.Text = "Event Name";
            // 
            // textBox_eventName
            // 
            textBox_eventName.Location = new Point(12, 79);
            textBox_eventName.Name = "textBox_eventName";
            textBox_eventName.Size = new Size(175, 23);
            textBox_eventName.TabIndex = 4;
            textBox_eventName.Text = "PatternStart";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(222, 61);
            label2.Name = "label2";
            label2.Size = new Size(98, 15);
            label2.TabIndex = 5;
            label2.Text = "Frequency (MHz)";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(193, 82);
            label3.Name = "label3";
            label3.Size = new Size(28, 15);
            label3.TabIndex = 6;
            label3.Text = "Ch0";
            // 
            // textBox_ch0_freq
            // 
            textBox_ch0_freq.Location = new Point(227, 79);
            textBox_ch0_freq.Name = "textBox_ch0_freq";
            textBox_ch0_freq.Size = new Size(85, 23);
            textBox_ch0_freq.TabIndex = 7;
            textBox_ch0_freq.Text = "100";
            // 
            // textBox_ch1_freq
            // 
            textBox_ch1_freq.Location = new Point(227, 108);
            textBox_ch1_freq.Name = "textBox_ch1_freq";
            textBox_ch1_freq.Size = new Size(85, 23);
            textBox_ch1_freq.TabIndex = 9;
            textBox_ch1_freq.Text = "100";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(193, 111);
            label4.Name = "label4";
            label4.Size = new Size(28, 15);
            label4.TabIndex = 8;
            label4.Text = "Ch1";
            // 
            // textBox_ch2_freq
            // 
            textBox_ch2_freq.Location = new Point(227, 137);
            textBox_ch2_freq.Name = "textBox_ch2_freq";
            textBox_ch2_freq.Size = new Size(85, 23);
            textBox_ch2_freq.TabIndex = 11;
            textBox_ch2_freq.Text = "100";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(193, 140);
            label5.Name = "label5";
            label5.Size = new Size(28, 15);
            label5.TabIndex = 10;
            label5.Text = "Ch2";
            // 
            // textBox_ch3_freq
            // 
            textBox_ch3_freq.Location = new Point(227, 166);
            textBox_ch3_freq.Name = "textBox_ch3_freq";
            textBox_ch3_freq.Size = new Size(85, 23);
            textBox_ch3_freq.TabIndex = 13;
            textBox_ch3_freq.Text = "100";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(193, 169);
            label6.Name = "label6";
            label6.Size = new Size(28, 15);
            label6.TabIndex = 12;
            label6.Text = "Ch3";
            // 
            // textBox_ch3_amp
            // 
            textBox_ch3_amp.Location = new Point(335, 166);
            textBox_ch3_amp.Name = "textBox_ch3_amp";
            textBox_ch3_amp.Size = new Size(62, 23);
            textBox_ch3_amp.TabIndex = 18;
            textBox_ch3_amp.Text = "1";
            textBox_ch3_amp.TextChanged += TextBox_Amp_TextChanged;
            // 
            // textBox_ch2_amp
            // 
            textBox_ch2_amp.Location = new Point(335, 137);
            textBox_ch2_amp.Name = "textBox_ch2_amp";
            textBox_ch2_amp.Size = new Size(62, 23);
            textBox_ch2_amp.TabIndex = 17;
            textBox_ch2_amp.Text = "1";
            textBox_ch2_amp.TextChanged += TextBox_Amp_TextChanged;
            // 
            // textBox_ch1_amp
            // 
            textBox_ch1_amp.Location = new Point(335, 108);
            textBox_ch1_amp.Name = "textBox_ch1_amp";
            textBox_ch1_amp.Size = new Size(62, 23);
            textBox_ch1_amp.TabIndex = 16;
            textBox_ch1_amp.Text = "1";
            textBox_ch1_amp.TextChanged += TextBox_Amp_TextChanged;
            // 
            // textBox_ch0_amp
            // 
            textBox_ch0_amp.Location = new Point(335, 79);
            textBox_ch0_amp.Name = "textBox_ch0_amp";
            textBox_ch0_amp.Size = new Size(62, 23);
            textBox_ch0_amp.TabIndex = 15;
            textBox_ch0_amp.Text = "1";
            textBox_ch0_amp.TextChanged += TextBox_Amp_TextChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(335, 61);
            label7.Name = "label7";
            label7.Size = new Size(63, 15);
            label7.TabIndex = 14;
            label7.Text = "Amplitude";
            // 
            // textBox_ch3_freq_slope
            // 
            textBox_ch3_freq_slope.Location = new Point(433, 166);
            textBox_ch3_freq_slope.Name = "textBox_ch3_freq_slope";
            textBox_ch3_freq_slope.Size = new Size(85, 23);
            textBox_ch3_freq_slope.TabIndex = 23;
            textBox_ch3_freq_slope.Text = "0";
            // 
            // textBox_ch2_freq_slope
            // 
            textBox_ch2_freq_slope.Location = new Point(433, 137);
            textBox_ch2_freq_slope.Name = "textBox_ch2_freq_slope";
            textBox_ch2_freq_slope.Size = new Size(85, 23);
            textBox_ch2_freq_slope.TabIndex = 22;
            textBox_ch2_freq_slope.Text = "0";
            // 
            // textBox_ch1_freq_slope
            // 
            textBox_ch1_freq_slope.Location = new Point(433, 108);
            textBox_ch1_freq_slope.Name = "textBox_ch1_freq_slope";
            textBox_ch1_freq_slope.Size = new Size(85, 23);
            textBox_ch1_freq_slope.TabIndex = 21;
            textBox_ch1_freq_slope.Text = "0";
            // 
            // textBox_ch0_freq_slope
            // 
            textBox_ch0_freq_slope.Location = new Point(433, 79);
            textBox_ch0_freq_slope.Name = "textBox_ch0_freq_slope";
            textBox_ch0_freq_slope.Size = new Size(85, 23);
            textBox_ch0_freq_slope.TabIndex = 20;
            textBox_ch0_freq_slope.Text = "0";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(428, 61);
            label8.Name = "label8";
            label8.Size = new Size(94, 15);
            label8.TabIndex = 19;
            label8.Text = "Frequency Slpoe";
            // 
            // textBox_ch3_amp_slope
            // 
            textBox_ch3_amp_slope.Location = new Point(557, 166);
            textBox_ch3_amp_slope.Name = "textBox_ch3_amp_slope";
            textBox_ch3_amp_slope.Size = new Size(62, 23);
            textBox_ch3_amp_slope.TabIndex = 28;
            textBox_ch3_amp_slope.Text = "0";
            // 
            // textBox_ch2_amp_slope
            // 
            textBox_ch2_amp_slope.Location = new Point(557, 137);
            textBox_ch2_amp_slope.Name = "textBox_ch2_amp_slope";
            textBox_ch2_amp_slope.Size = new Size(62, 23);
            textBox_ch2_amp_slope.TabIndex = 27;
            textBox_ch2_amp_slope.Text = "0";
            // 
            // textBox_ch1_amp_slope
            // 
            textBox_ch1_amp_slope.Location = new Point(557, 108);
            textBox_ch1_amp_slope.Name = "textBox_ch1_amp_slope";
            textBox_ch1_amp_slope.Size = new Size(62, 23);
            textBox_ch1_amp_slope.TabIndex = 26;
            textBox_ch1_amp_slope.Text = "0";
            // 
            // textBox_ch0_amp_slope
            // 
            textBox_ch0_amp_slope.Location = new Point(557, 79);
            textBox_ch0_amp_slope.Name = "textBox_ch0_amp_slope";
            textBox_ch0_amp_slope.Size = new Size(62, 23);
            textBox_ch0_amp_slope.TabIndex = 25;
            textBox_ch0_amp_slope.Text = "0";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(539, 61);
            label9.Name = "label9";
            label9.Size = new Size(95, 15);
            label9.TabIndex = 24;
            label9.Text = "Amplitude Slpoe";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(3, 111);
            label10.Name = "label10";
            label10.Size = new Size(92, 15);
            label10.TabIndex = 29;
            label10.Text = "Event Time (ms)";
            // 
            // textBox_eventTime
            // 
            textBox_eventTime.Location = new Point(12, 129);
            textBox_eventTime.Name = "textBox_eventTime";
            textBox_eventTime.Size = new Size(83, 23);
            textBox_eventTime.TabIndex = 30;
            textBox_eventTime.Text = "0";
            // 
            // timer1
            // 
            timer1.Interval = 200;
            timer1.Tick += timer_tick;
            // 
            // patternGridView
            // 
            patternGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            patternGridView.Columns.AddRange(new DataGridViewColumn[] { dataGridEventName, dataGridTime, dataGridCh0Freq, dataGridCh1Freq, dataGridCh2Freq, dataGridCh3Freq, dataGridCh0Amp, dataGridCh1Amp, dataGridCh2Amp, dataGridCh3Amp, dataGridCh0FreqSlpoe, dataGridCh1FreqSlope, dataGridCh2FreqSlope, dataGridCh3FreqSlope, dataGridCh0AmpSlope, dataGridCh1AmpSlope, dataGridCh2AmpSlope, dataGridCh3AmpSlope });
            patternGridView.Location = new Point(12, 195);
            patternGridView.Name = "patternGridView";
            patternGridView.Size = new Size(1183, 252);
            patternGridView.TabIndex = 31;
            // 
            // dataGridEventName
            // 
            dataGridEventName.HeaderText = "Event Name";
            dataGridEventName.Name = "dataGridEventName";
            dataGridEventName.ReadOnly = true;
            dataGridEventName.Width = 120;
            // 
            // dataGridTime
            // 
            dataGridTime.HeaderText = "Time (ms)";
            dataGridTime.Name = "dataGridTime";
            dataGridTime.ReadOnly = true;
            dataGridTime.Width = 60;
            // 
            // dataGridCh0Freq
            // 
            dataGridCh0Freq.HeaderText = "Ch0 Freq";
            dataGridCh0Freq.Name = "dataGridCh0Freq";
            dataGridCh0Freq.ReadOnly = true;
            dataGridCh0Freq.Width = 60;
            // 
            // dataGridCh1Freq
            // 
            dataGridCh1Freq.HeaderText = "Ch1 Freq";
            dataGridCh1Freq.Name = "dataGridCh1Freq";
            dataGridCh1Freq.ReadOnly = true;
            dataGridCh1Freq.Width = 60;
            // 
            // dataGridCh2Freq
            // 
            dataGridCh2Freq.HeaderText = "Ch2 Freq";
            dataGridCh2Freq.Name = "dataGridCh2Freq";
            dataGridCh2Freq.ReadOnly = true;
            dataGridCh2Freq.Width = 60;
            // 
            // dataGridCh3Freq
            // 
            dataGridCh3Freq.HeaderText = "Ch3 Freq";
            dataGridCh3Freq.Name = "dataGridCh3Freq";
            dataGridCh3Freq.ReadOnly = true;
            dataGridCh3Freq.Width = 60;
            // 
            // dataGridCh0Amp
            // 
            dataGridCh0Amp.HeaderText = "Ch0 Amp";
            dataGridCh0Amp.Name = "dataGridCh0Amp";
            dataGridCh0Amp.ReadOnly = true;
            dataGridCh0Amp.Width = 60;
            // 
            // dataGridCh1Amp
            // 
            dataGridCh1Amp.HeaderText = "Ch1 Amp";
            dataGridCh1Amp.Name = "dataGridCh1Amp";
            dataGridCh1Amp.ReadOnly = true;
            dataGridCh1Amp.Width = 60;
            // 
            // dataGridCh2Amp
            // 
            dataGridCh2Amp.HeaderText = "Ch2 Amp";
            dataGridCh2Amp.Name = "dataGridCh2Amp";
            dataGridCh2Amp.ReadOnly = true;
            dataGridCh2Amp.Width = 60;
            // 
            // dataGridCh3Amp
            // 
            dataGridCh3Amp.HeaderText = "Ch3 Amp";
            dataGridCh3Amp.Name = "dataGridCh3Amp";
            dataGridCh3Amp.ReadOnly = true;
            dataGridCh3Amp.Width = 60;
            // 
            // dataGridCh0FreqSlpoe
            // 
            dataGridCh0FreqSlpoe.HeaderText = "Ch0 Freq Slope";
            dataGridCh0FreqSlpoe.Name = "dataGridCh0FreqSlpoe";
            dataGridCh0FreqSlpoe.ReadOnly = true;
            dataGridCh0FreqSlpoe.Width = 60;
            // 
            // dataGridCh1FreqSlope
            // 
            dataGridCh1FreqSlope.HeaderText = "Ch1 Freq Slope";
            dataGridCh1FreqSlope.Name = "dataGridCh1FreqSlope";
            dataGridCh1FreqSlope.ReadOnly = true;
            dataGridCh1FreqSlope.Width = 60;
            // 
            // dataGridCh2FreqSlope
            // 
            dataGridCh2FreqSlope.HeaderText = "Ch2 Freq Slope";
            dataGridCh2FreqSlope.Name = "dataGridCh2FreqSlope";
            dataGridCh2FreqSlope.ReadOnly = true;
            dataGridCh2FreqSlope.Width = 60;
            // 
            // dataGridCh3FreqSlope
            // 
            dataGridCh3FreqSlope.HeaderText = "Ch3 Freq Slope";
            dataGridCh3FreqSlope.Name = "dataGridCh3FreqSlope";
            dataGridCh3FreqSlope.ReadOnly = true;
            dataGridCh3FreqSlope.Width = 60;
            // 
            // dataGridCh0AmpSlope
            // 
            dataGridCh0AmpSlope.HeaderText = "Ch0 Amp Slope";
            dataGridCh0AmpSlope.Name = "dataGridCh0AmpSlope";
            dataGridCh0AmpSlope.ReadOnly = true;
            dataGridCh0AmpSlope.Width = 60;
            // 
            // dataGridCh1AmpSlope
            // 
            dataGridCh1AmpSlope.HeaderText = "Ch1 Amp Slope";
            dataGridCh1AmpSlope.Name = "dataGridCh1AmpSlope";
            dataGridCh1AmpSlope.ReadOnly = true;
            dataGridCh1AmpSlope.Width = 60;
            // 
            // dataGridCh2AmpSlope
            // 
            dataGridCh2AmpSlope.HeaderText = "Ch2 Amp Slope";
            dataGridCh2AmpSlope.Name = "dataGridCh2AmpSlope";
            dataGridCh2AmpSlope.ReadOnly = true;
            dataGridCh2AmpSlope.Width = 60;
            // 
            // dataGridCh3AmpSlope
            // 
            dataGridCh3AmpSlope.HeaderText = "Ch3 Amp Slope";
            dataGridCh3AmpSlope.Name = "dataGridCh3AmpSlope";
            dataGridCh3AmpSlope.ReadOnly = true;
            dataGridCh3AmpSlope.Width = 60;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(649, 166);
            label11.Name = "label11";
            label11.Size = new Size(344, 15);
            label11.TabIndex = 32;
            label11.Text = "All frequencies in MHz, amplitudes between 0 to 1, slope per ms";
            label11.Click += label11_Click;
            // 
            // buttonStartPattern
            // 
            buttonStartPattern.Location = new Point(752, 78);
            buttonStartPattern.Name = "buttonStartPattern";
            buttonStartPattern.Size = new Size(109, 23);
            buttonStartPattern.TabIndex = 33;
            buttonStartPattern.Text = "Start Pattern";
            buttonStartPattern.UseVisualStyleBackColor = true;
            buttonStartPattern.Click += button_start_pattern_clicked;
            // 
            // buttonStopPattern
            // 
            buttonStopPattern.Location = new Point(752, 107);
            buttonStopPattern.Name = "buttonStopPattern";
            buttonStopPattern.Size = new Size(109, 23);
            buttonStopPattern.TabIndex = 34;
            buttonStopPattern.Text = "Stop Pattern";
            buttonStopPattern.UseVisualStyleBackColor = true;
            buttonStopPattern.Click += button_stop_pattern_clicked;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(1063, 29);
            label12.Name = "label12";
            label12.Size = new Size(112, 15);
            label12.TabIndex = 35;
            label12.Text = "Pattern Length (ms)";
            label12.Click += label12_Click;
            // 
            // textBoxPatternLength
            // 
            textBoxPatternLength.Location = new Point(1075, 53);
            textBoxPatternLength.Name = "textBoxPatternLength";
            textBoxPatternLength.Size = new Size(83, 23);
            textBoxPatternLength.TabIndex = 36;
            textBoxPatternLength.Text = "300";
            textBoxPatternLength.TextChanged += textBox_pattern_length_changed;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { menuToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1223, 24);
            menuStrip1.TabIndex = 37;
            menuStrip1.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            menuToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripSavePattern, toolStripLoadPattern });
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.Size = new Size(50, 20);
            menuToolStripMenuItem.Text = "Menu";
            // 
            // toolStripSavePattern
            // 
            toolStripSavePattern.Name = "toolStripSavePattern";
            toolStripSavePattern.Size = new Size(141, 22);
            toolStripSavePattern.Text = "Save Pattern";
            toolStripSavePattern.Click += save_pattern_clicked;
            // 
            // toolStripLoadPattern
            // 
            toolStripLoadPattern.Name = "toolStripLoadPattern";
            toolStripLoadPattern.Size = new Size(141, 22);
            toolStripLoadPattern.Text = "Load Pattern";
            toolStripLoadPattern.Click += load_pattern_clicked;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(13, 29);
            label13.Name = "label13";
            label13.Size = new Size(82, 15);
            label13.TabIndex = 38;
            label13.Text = "Pattern in use:";
            // 
            // lablePatternName
            // 
            lablePatternName.AutoSize = true;
            lablePatternName.Location = new Point(101, 29);
            lablePatternName.Name = "lablePatternName";
            lablePatternName.Size = new Size(36, 15);
            lablePatternName.TabIndex = 39;
            lablePatternName.Text = "None";
            // 
            // labelRunIndicator
            // 
            labelRunIndicator.AutoSize = true;
            labelRunIndicator.ForeColor = Color.Red;
            labelRunIndicator.Location = new Point(769, 140);
            labelRunIndicator.Name = "labelRunIndicator";
            labelRunIndicator.Size = new Size(92, 15);
            labelRunIndicator.TabIndex = 40;
            labelRunIndicator.Text = "Pattern Stopped";
            labelRunIndicator.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // NeanderthalForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1223, 459);
            Controls.Add(labelRunIndicator);
            Controls.Add(lablePatternName);
            Controls.Add(label13);
            Controls.Add(menuStrip1);
            Controls.Add(textBoxPatternLength);
            Controls.Add(label12);
            Controls.Add(buttonStopPattern);
            Controls.Add(buttonStartPattern);
            Controls.Add(label11);
            Controls.Add(patternGridView);
            Controls.Add(textBox_eventTime);
            Controls.Add(label10);
            Controls.Add(textBox_ch3_amp_slope);
            Controls.Add(textBox_ch2_amp_slope);
            Controls.Add(textBox_ch1_amp_slope);
            Controls.Add(textBox_ch0_amp_slope);
            Controls.Add(label9);
            Controls.Add(textBox_ch3_freq_slope);
            Controls.Add(textBox_ch2_freq_slope);
            Controls.Add(textBox_ch1_freq_slope);
            Controls.Add(textBox_ch0_freq_slope);
            Controls.Add(label8);
            Controls.Add(textBox_ch3_amp);
            Controls.Add(textBox_ch2_amp);
            Controls.Add(textBox_ch1_amp);
            Controls.Add(textBox_ch0_amp);
            Controls.Add(label7);
            Controls.Add(textBox_ch3_freq);
            Controls.Add(label6);
            Controls.Add(textBox_ch2_freq);
            Controls.Add(label5);
            Controls.Add(textBox_ch1_freq);
            Controls.Add(label4);
            Controls.Add(textBox_ch0_freq);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(textBox_eventName);
            Controls.Add(label1);
            Controls.Add(button_delete);
            Controls.Add(button_add);
            MainMenuStrip = menuStrip1;
            Name = "NeanderthalForm";
            Text = "NeanderthalDDSController";
            Load += Form_Load;
            ((System.ComponentModel.ISupportInitialize)patternGridView).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
