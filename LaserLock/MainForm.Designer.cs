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
            this.cavityGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.cavityPlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.outputValueNumericEditor = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.label1 = new System.Windows.Forms.Label();
            this.LockCheck = new System.Windows.Forms.CheckBox();
            this.gainGroupBox = new System.Windows.Forms.GroupBox();
            this.pSlider = new NationalInstruments.UI.WindowsForms.Slide();
            this.iSlider = new NationalInstruments.UI.WindowsForms.Slide();
            this.dSlider = new NationalInstruments.UI.WindowsForms.Slide();
            this.pLabel = new System.Windows.Forms.Label();
            this.iLabel = new System.Windows.Forms.Label();
            this.dLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cavityGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputValueNumericEditor)).BeginInit();
            this.gainGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dSlider)).BeginInit();
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
            this.parkToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.parkToolStripMenuItem.Text = "Park";
            this.parkToolStripMenuItem.Click += new System.EventHandler(this.parkToolStripMenuItem_Click);
            // 
            // lockToolStripMenuItem
            // 
            this.lockToolStripMenuItem.Name = "lockToolStripMenuItem";
            this.lockToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.lockToolStripMenuItem.Text = "Lock";
            this.lockToolStripMenuItem.Click += new System.EventHandler(this.lockToolStripMenuItem_Click);
            // 
            // unlockToolStripMenuItem
            // 
            this.unlockToolStripMenuItem.Name = "unlockToolStripMenuItem";
            this.unlockToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.unlockToolStripMenuItem.Text = "Unlock";
            this.unlockToolStripMenuItem.Click += new System.EventHandler(this.unlockToolStripMenuItem_Click);
            // 
            // cavityGraph
            // 
            this.cavityGraph.Location = new System.Drawing.Point(6, 54);
            this.cavityGraph.Name = "cavityGraph";
            this.cavityGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.cavityPlot});
            this.cavityGraph.Size = new System.Drawing.Size(475, 166);
            this.cavityGraph.TabIndex = 2;
            this.cavityGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.cavityGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // cavityPlot
            // 
            this.cavityPlot.XAxis = this.xAxis1;
            this.cavityPlot.YAxis = this.yAxis1;
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
            // outputValueNumericEditor
            // 
            this.outputValueNumericEditor.CoercionInterval = 0.005;
            this.outputValueNumericEditor.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(3);
            this.outputValueNumericEditor.Location = new System.Drawing.Point(100, 29);
            this.outputValueNumericEditor.Name = "outputValueNumericEditor";
            this.outputValueNumericEditor.OutOfRangeMode = NationalInstruments.UI.NumericOutOfRangeMode.CoerceToRange;
            this.outputValueNumericEditor.Range = new NationalInstruments.UI.Range(-5, 5);
            this.outputValueNumericEditor.Size = new System.Drawing.Size(61, 20);
            this.outputValueNumericEditor.TabIndex = 4;
            this.outputValueNumericEditor.AfterChangeValue += new NationalInstruments.UI.AfterChangeNumericValueEventHandler(this.UpdateVoltage);
            this.outputValueNumericEditor.DownButtonClicked += new System.EventHandler(this.UpdateVoltage);
            this.outputValueNumericEditor.UpButtonClicked += new System.EventHandler(this.UpdateVoltage);
            this.outputValueNumericEditor.Click += new System.EventHandler(this.outputValueNumericEditor_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(167, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Control Voltage";
            // 
            // LockCheck
            // 
            this.LockCheck.AutoSize = true;
            this.LockCheck.Location = new System.Drawing.Point(12, 30);
            this.LockCheck.Name = "LockCheck";
            this.LockCheck.Size = new System.Drawing.Size(62, 17);
            this.LockCheck.TabIndex = 6;
            this.LockCheck.Text = "Locked";
            this.LockCheck.UseVisualStyleBackColor = true;
            this.LockCheck.CheckedChanged += new System.EventHandler(this.lockCheck_CheckedChanged);
            // 
            // gainGroupBox
            // 
            this.gainGroupBox.Controls.Add(this.dLabel);
            this.gainGroupBox.Controls.Add(this.iLabel);
            this.gainGroupBox.Controls.Add(this.pLabel);
            this.gainGroupBox.Controls.Add(this.dSlider);
            this.gainGroupBox.Controls.Add(this.iSlider);
            this.gainGroupBox.Controls.Add(this.pSlider);
            this.gainGroupBox.Location = new System.Drawing.Point(487, 27);
            this.gainGroupBox.Name = "gainGroupBox";
            this.gainGroupBox.Size = new System.Drawing.Size(159, 193);
            this.gainGroupBox.TabIndex = 7;
            this.gainGroupBox.TabStop = false;
            this.gainGroupBox.Text = "Gain settings";
            // 
            // pSlider
            // 
            this.pSlider.CaptionBackColor = System.Drawing.SystemColors.Control;
            this.pSlider.CaptionForeColor = System.Drawing.SystemColors.ControlText;
            this.pSlider.CoercionInterval = 0.1;
            this.pSlider.Location = new System.Drawing.Point(6, 19);
            this.pSlider.Name = "pSlider";
            this.pSlider.OutOfRangeMode = NationalInstruments.UI.NumericOutOfRangeMode.CoerceToRange;
            this.pSlider.PointerColor = System.Drawing.SystemColors.HotTrack;
            this.pSlider.Size = new System.Drawing.Size(50, 175);
            this.pSlider.TabIndex = 0;
            this.pSlider.AfterChangeValue += new NationalInstruments.UI.AfterChangeNumericValueEventHandler(this.pSlider_AfterChangeValue);
            // 
            // iSlider
            // 
            this.iSlider.CoercionInterval = 0.1;
            this.iSlider.Location = new System.Drawing.Point(56, 19);
            this.iSlider.Name = "iSlider";
            this.iSlider.PointerColor = System.Drawing.SystemColors.HotTrack;
            this.iSlider.Size = new System.Drawing.Size(50, 175);
            this.iSlider.TabIndex = 1;
            // 
            // dSlider
            // 
            this.dSlider.CoercionInterval = 0.1;
            this.dSlider.Location = new System.Drawing.Point(104, 19);
            this.dSlider.Name = "dSlider";
            this.dSlider.PointerColor = System.Drawing.SystemColors.HotTrack;
            this.dSlider.Size = new System.Drawing.Size(50, 175);
            this.dSlider.TabIndex = 2;
            // 
            // pLabel
            // 
            this.pLabel.AutoSize = true;
            this.pLabel.Location = new System.Drawing.Point(31, 16);
            this.pLabel.Name = "pLabel";
            this.pLabel.Size = new System.Drawing.Size(14, 13);
            this.pLabel.TabIndex = 3;
            this.pLabel.Text = "P";
            // 
            // iLabel
            // 
            this.iLabel.AutoSize = true;
            this.iLabel.Location = new System.Drawing.Point(83, 16);
            this.iLabel.Name = "iLabel";
            this.iLabel.Size = new System.Drawing.Size(10, 13);
            this.iLabel.TabIndex = 4;
            this.iLabel.Text = "I";
            // 
            // dLabel
            // 
            this.dLabel.AutoSize = true;
            this.dLabel.Location = new System.Drawing.Point(130, 16);
            this.dLabel.Name = "dLabel";
            this.dLabel.Size = new System.Drawing.Size(15, 13);
            this.dLabel.TabIndex = 5;
            this.dLabel.Text = "D";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 292);
            this.Controls.Add(this.gainGroupBox);
            this.Controls.Add(this.LockCheck);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.outputValueNumericEditor);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cavityGraph);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Laser Lock";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cavityGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputValueNumericEditor)).EndInit();
            this.gainGroupBox.ResumeLayout(false);
            this.gainGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem parkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lockToolStripMenuItem;
        private NationalInstruments.UI.WindowsForms.WaveformGraph cavityGraph;
        private NationalInstruments.UI.WaveformPlot cavityPlot;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.TextBox textBox1;
        private NationalInstruments.UI.WindowsForms.NumericEdit outputValueNumericEditor;
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
    }
}

