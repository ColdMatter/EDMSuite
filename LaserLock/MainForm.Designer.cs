namespace LaserLock
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.parkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.controlVoltageNumericEditor = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.label1 = new System.Windows.Forms.Label();
            this.LockCheck = new System.Windows.Forms.CheckBox();
            this.gainGroupBox = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.speedSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dLabel = new System.Windows.Forms.Label();
            this.iLabel = new System.Windows.Forms.Label();
            this.pLabel = new System.Windows.Forms.Label();
            this.slopeSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.dSlider = new NationalInstruments.UI.WindowsForms.Slide();
            this.iSlider = new NationalInstruments.UI.WindowsForms.Slide();
            this.pSlider = new NationalInstruments.UI.WindowsForms.Slide();
            this.deviationGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlot1 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.label2 = new System.Windows.Forms.Label();
            this.setpointNumericEdit = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.scanNumberBox = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlVoltageNumericEditor)).BeginInit();
            this.gainGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.speedSwitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slopeSwitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deviationGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.setpointNumericEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scanNumberBox)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(653, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.parkToolStripMenuItem,
            this.lockToolStripMenuItem,
            this.unlockToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(54, 20);
            this.toolStripMenuItem1.Text = "Actions";
            // 
            // parkToolStripMenuItem
            // 
            this.parkToolStripMenuItem.Name = "parkToolStripMenuItem";
            this.parkToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.parkToolStripMenuItem.Text = "Park";
            this.parkToolStripMenuItem.Click += new System.EventHandler(this.parkToolStripMenuItem_Click);
            // 
            // lockToolStripMenuItem
            // 
            this.lockToolStripMenuItem.Name = "lockToolStripMenuItem";
            this.lockToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.lockToolStripMenuItem.Text = "Lock";
            this.lockToolStripMenuItem.Click += new System.EventHandler(this.lockToolStripMenuItem_Click);
            // 
            // unlockToolStripMenuItem
            // 
            this.unlockToolStripMenuItem.Name = "unlockToolStripMenuItem";
            this.unlockToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.unlockToolStripMenuItem.Text = "Unlock";
            this.unlockToolStripMenuItem.Click += new System.EventHandler(this.unlockToolStripMenuItem_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 227);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(640, 53);
            this.textBox1.TabIndex = 3;
            // 
            // controlVoltageNumericEditor
            // 
            this.controlVoltageNumericEditor.CoercionInterval = 0.005;
            this.controlVoltageNumericEditor.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(3);
            this.controlVoltageNumericEditor.Location = new System.Drawing.Point(91, 26);
            this.controlVoltageNumericEditor.Name = "controlVoltageNumericEditor";
            this.controlVoltageNumericEditor.OutOfRangeMode = NationalInstruments.UI.NumericOutOfRangeMode.CoerceToRange;
            this.controlVoltageNumericEditor.Range = new NationalInstruments.UI.Range(-5, 5);
            this.controlVoltageNumericEditor.Size = new System.Drawing.Size(61, 20);
            this.controlVoltageNumericEditor.TabIndex = 4;
            this.controlVoltageNumericEditor.AfterChangeValue += new NationalInstruments.UI.AfterChangeNumericValueEventHandler(this.controlVoltageNumericEditor_AfterChangeValue);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(158, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Control Voltage";
            // 
            // LockCheck
            // 
            this.LockCheck.AutoSize = true;
            this.LockCheck.Location = new System.Drawing.Point(12, 29);
            this.LockCheck.Name = "LockCheck";
            this.LockCheck.Size = new System.Drawing.Size(62, 17);
            this.LockCheck.TabIndex = 6;
            this.LockCheck.Text = "Locked";
            this.LockCheck.UseVisualStyleBackColor = true;
            this.LockCheck.CheckedChanged += new System.EventHandler(this.lockCheck_CheckedChanged);
            // 
            // gainGroupBox
            // 
            this.gainGroupBox.Controls.Add(this.label6);
            this.gainGroupBox.Controls.Add(this.label5);
            this.gainGroupBox.Controls.Add(this.speedSwitch);
            this.gainGroupBox.Controls.Add(this.label3);
            this.gainGroupBox.Controls.Add(this.label4);
            this.gainGroupBox.Controls.Add(this.dLabel);
            this.gainGroupBox.Controls.Add(this.iLabel);
            this.gainGroupBox.Controls.Add(this.pLabel);
            this.gainGroupBox.Controls.Add(this.slopeSwitch);
            this.gainGroupBox.Controls.Add(this.dSlider);
            this.gainGroupBox.Controls.Add(this.iSlider);
            this.gainGroupBox.Controls.Add(this.pSlider);
            this.gainGroupBox.Location = new System.Drawing.Point(473, 27);
            this.gainGroupBox.Name = "gainGroupBox";
            this.gainGroupBox.Size = new System.Drawing.Size(173, 193);
            this.gainGroupBox.TabIndex = 7;
            this.gainGroupBox.TabStop = false;
            this.gainGroupBox.Text = "Gain settings";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 94);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Slow";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 38);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Fast";
            // 
            // speedSwitch
            // 
            this.speedSwitch.Location = new System.Drawing.Point(8, 49);
            this.speedSwitch.Name = "speedSwitch";
            this.speedSwitch.OffColor = System.Drawing.SystemColors.InactiveCaption;
            this.speedSwitch.OnColor = System.Drawing.SystemColors.ActiveCaption;
            this.speedSwitch.Size = new System.Drawing.Size(26, 48);
            this.speedSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalSlide;
            this.speedSwitch.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Pos";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 173);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Neg";
            // 
            // dLabel
            // 
            this.dLabel.AutoSize = true;
            this.dLabel.Location = new System.Drawing.Point(146, 16);
            this.dLabel.Name = "dLabel";
            this.dLabel.Size = new System.Drawing.Size(15, 13);
            this.dLabel.TabIndex = 5;
            this.dLabel.Text = "D";
            // 
            // iLabel
            // 
            this.iLabel.AutoSize = true;
            this.iLabel.Location = new System.Drawing.Point(104, 16);
            this.iLabel.Name = "iLabel";
            this.iLabel.Size = new System.Drawing.Size(10, 13);
            this.iLabel.TabIndex = 4;
            this.iLabel.Text = "I";
            // 
            // pLabel
            // 
            this.pLabel.AutoSize = true;
            this.pLabel.Location = new System.Drawing.Point(57, 16);
            this.pLabel.Name = "pLabel";
            this.pLabel.Size = new System.Drawing.Size(14, 13);
            this.pLabel.TabIndex = 3;
            this.pLabel.Text = "P";
            // 
            // slopeSwitch
            // 
            this.slopeSwitch.Location = new System.Drawing.Point(9, 128);
            this.slopeSwitch.Name = "slopeSwitch";
            this.slopeSwitch.OffColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.slopeSwitch.OnColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.slopeSwitch.Size = new System.Drawing.Size(26, 48);
            this.slopeSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalSlide;
            this.slopeSwitch.TabIndex = 8;
            // 
            // dSlider
            // 
            this.dSlider.CoercionInterval = 0.1;
            this.dSlider.Location = new System.Drawing.Point(123, 19);
            this.dSlider.Name = "dSlider";
            this.dSlider.PointerColor = System.Drawing.SystemColors.HotTrack;
            this.dSlider.Size = new System.Drawing.Size(50, 175);
            this.dSlider.TabIndex = 2;
            // 
            // iSlider
            // 
            this.iSlider.CoercionInterval = 0.1;
            this.iSlider.Location = new System.Drawing.Point(78, 19);
            this.iSlider.Name = "iSlider";
            this.iSlider.PointerColor = System.Drawing.SystemColors.HotTrack;
            this.iSlider.Size = new System.Drawing.Size(50, 175);
            this.iSlider.TabIndex = 1;
            this.iSlider.AfterChangeValue += new NationalInstruments.UI.AfterChangeNumericValueEventHandler(this.iSlider_AfterChangeValue);
            // 
            // pSlider
            // 
            this.pSlider.CaptionBackColor = System.Drawing.SystemColors.Control;
            this.pSlider.CaptionForeColor = System.Drawing.SystemColors.ControlText;
            this.pSlider.CoercionInterval = 0.1;
            this.pSlider.Location = new System.Drawing.Point(33, 19);
            this.pSlider.Name = "pSlider";
            this.pSlider.OutOfRangeMode = NationalInstruments.UI.NumericOutOfRangeMode.CoerceToRange;
            this.pSlider.PointerColor = System.Drawing.SystemColors.HotTrack;
            this.pSlider.Size = new System.Drawing.Size(50, 175);
            this.pSlider.TabIndex = 0;
            this.pSlider.AfterChangeValue += new NationalInstruments.UI.AfterChangeNumericValueEventHandler(this.pSlider_AfterChangeValue);
            // 
            // deviationGraph
            // 
            this.deviationGraph.Location = new System.Drawing.Point(6, 52);
            this.deviationGraph.Name = "deviationGraph";
            this.deviationGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot1});
            this.deviationGraph.Size = new System.Drawing.Size(461, 168);
            this.deviationGraph.TabIndex = 8;
            this.deviationGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.deviationGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // waveformPlot1
            // 
            this.waveformPlot1.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.waveformPlot1.XAxis = this.xAxis1;
            this.waveformPlot1.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis1.Range = new NationalInstruments.UI.Range(0, 100);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(322, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Set Point";
            // 
            // setpointNumericEdit
            // 
            this.setpointNumericEdit.CoercionInterval = 0.01;
            this.setpointNumericEdit.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(3);
            this.setpointNumericEdit.Location = new System.Drawing.Point(257, 26);
            this.setpointNumericEdit.Name = "setpointNumericEdit";
            this.setpointNumericEdit.Range = new NationalInstruments.UI.Range(0, 10);
            this.setpointNumericEdit.Size = new System.Drawing.Size(59, 20);
            this.setpointNumericEdit.TabIndex = 11;
            this.setpointNumericEdit.AfterChangeValue += new NationalInstruments.UI.AfterChangeNumericValueEventHandler(this.setpointNumericEdit_AfterChangeValue);
            // 
            // scanNumberBox
            // 
            this.scanNumberBox.Location = new System.Drawing.Point(391, 26);
            this.scanNumberBox.Name = "scanNumberBox";
            this.scanNumberBox.Size = new System.Drawing.Size(38, 20);
            this.scanNumberBox.TabIndex = 12;
            this.scanNumberBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(430, 30);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Scans";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 292);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.scanNumberBox);
            this.Controls.Add(this.setpointNumericEdit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.deviationGraph);
            this.Controls.Add(this.gainGroupBox);
            this.Controls.Add(this.LockCheck);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.controlVoltageNumericEditor);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Laser Lock";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlVoltageNumericEditor)).EndInit();
            this.gainGroupBox.ResumeLayout(false);
            this.gainGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.speedSwitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slopeSwitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.deviationGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.setpointNumericEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scanNumberBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem parkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lockToolStripMenuItem;
        private System.Windows.Forms.TextBox textBox1;
        private NationalInstruments.UI.WindowsForms.NumericEdit controlVoltageNumericEditor;
        private System.Windows.Forms.ToolStripMenuItem unlockToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox LockCheck;
        private System.Windows.Forms.GroupBox gainGroupBox;
        private NationalInstruments.UI.WindowsForms.Slide pSlider;
        private NationalInstruments.UI.WindowsForms.Slide dSlider;
        private NationalInstruments.UI.WindowsForms.Slide iSlider;
        private System.Windows.Forms.Label pLabel;
        private System.Windows.Forms.Label dLabel;
        private System.Windows.Forms.Label iLabel;
        private NationalInstruments.UI.WindowsForms.Switch slopeSwitch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private NationalInstruments.UI.WindowsForms.WaveformGraph deviationGraph;
        private NationalInstruments.UI.WaveformPlot waveformPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.Label label2;
        private NationalInstruments.UI.WindowsForms.NumericEdit setpointNumericEdit;
        private NationalInstruments.UI.WindowsForms.Switch speedSwitch;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown scanNumberBox;
        private System.Windows.Forms.Label label7;
    }
}

