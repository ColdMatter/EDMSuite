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
            this.parkLockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cavityGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.cavityPlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.outputValueNumericEditor = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.unlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cavityGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputValueNumericEditor)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(618, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.parkToolStripMenuItem,
            this.lockToolStripMenuItem,
            this.parkLockToolStripMenuItem,
            this.unlockToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(54, 20);
            this.toolStripMenuItem1.Text = "Actions";
            // 
            // parkToolStripMenuItem
            // 
            this.parkToolStripMenuItem.Name = "parkToolStripMenuItem";
            this.parkToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.parkToolStripMenuItem.Text = "Park";
            this.parkToolStripMenuItem.Click += new System.EventHandler(this.parkToolStripMenuItem_Click);
            // 
            // lockToolStripMenuItem
            // 
            this.lockToolStripMenuItem.Name = "lockToolStripMenuItem";
            this.lockToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.lockToolStripMenuItem.Text = "Lock";
            this.lockToolStripMenuItem.Click += new System.EventHandler(this.lockToolStripMenuItem_Click);
            // 
            // parkLockToolStripMenuItem
            // 
            this.parkLockToolStripMenuItem.Name = "parkLockToolStripMenuItem";
            this.parkLockToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.parkLockToolStripMenuItem.Text = "Park and Lock";
            // 
            // cavityGraph
            // 
            this.cavityGraph.Location = new System.Drawing.Point(12, 54);
            this.cavityGraph.Name = "cavityGraph";
            this.cavityGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.cavityPlot});
            this.cavityGraph.Size = new System.Drawing.Size(595, 168);
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
            this.textBox1.Location = new System.Drawing.Point(24, 240);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(542, 40);
            this.textBox1.TabIndex = 3;
            // 
            // outputValueNumericEditor
            // 
            this.outputValueNumericEditor.CoercionInterval = 0.005;
            this.outputValueNumericEditor.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(3);
            this.outputValueNumericEditor.Location = new System.Drawing.Point(12, 28);
            this.outputValueNumericEditor.Name = "outputValueNumericEditor";
            this.outputValueNumericEditor.OutOfRangeMode = NationalInstruments.UI.NumericOutOfRangeMode.CoerceToRange;
            this.outputValueNumericEditor.Range = new NationalInstruments.UI.Range(-5, 5);
            this.outputValueNumericEditor.Size = new System.Drawing.Size(61, 20);
            this.outputValueNumericEditor.TabIndex = 4;
            this.outputValueNumericEditor.AfterChangeValue += new NationalInstruments.UI.AfterChangeNumericValueEventHandler(this.UpdateVoltage);
            this.outputValueNumericEditor.DownButtonClicked += new System.EventHandler(this.UpdateVoltage);
            this.outputValueNumericEditor.UpButtonClicked += new System.EventHandler(this.UpdateVoltage);
            // 
            // unlockToolStripMenuItem
            // 
            this.unlockToolStripMenuItem.Name = "unlockToolStripMenuItem";
            this.unlockToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.unlockToolStripMenuItem.Text = "Unlock";
            this.unlockToolStripMenuItem.Click += new System.EventHandler(this.unlockToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 292);
            this.Controls.Add(this.outputValueNumericEditor);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cavityGraph);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Laser Lock";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cavityGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputValueNumericEditor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem parkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parkLockToolStripMenuItem;
        private NationalInstruments.UI.WindowsForms.WaveformGraph cavityGraph;
        private NationalInstruments.UI.WaveformPlot cavityPlot;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.TextBox textBox1;
        private NationalInstruments.UI.WindowsForms.NumericEdit outputValueNumericEditor;
        private System.Windows.Forms.ToolStripMenuItem unlockToolStripMenuItem;
    }
}

