namespace ZeemanSisyphusHardwareControl.Controls
{
    partial class SourceTabView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tempGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cryoGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.cryoSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.cryoLED = new NationalInstruments.UI.WindowsForms.Led();
            this.heaterGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.heaterSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.heaterLED = new NationalInstruments.UI.WindowsForms.Led();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.holdButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cycleLimit = new System.Windows.Forms.NumericUpDown();
            this.cycleButton = new System.Windows.Forms.Button();
            this.readButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.currentPressure = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rbThermLT = new System.Windows.Forms.RadioButton();
            this.numRref = new System.Windows.Forms.NumericUpDown();
            this.rbThermRT = new System.Windows.Forms.RadioButton();
            this.currentTemperature = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tempGraph)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.cryoGroup.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cryoSwitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cryoLED)).BeginInit();
            this.heaterGroup.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.heaterSwitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heaterLED)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cycleLimit)).BeginInit();
            this.tableLayoutPanel6.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRref)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.64706F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.35294F));
            this.tableLayoutPanel1.Controls.Add(this.tempGraph, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.97531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.02469F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(680, 810);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // tempGraph
            // 
            this.tempGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tempGraph.Location = new System.Drawing.Point(3, 3);
            this.tempGraph.Name = "tempGraph";
            this.tempGraph.Size = new System.Drawing.Size(521, 658);
            this.tempGraph.TabIndex = 0;
            this.tempGraph.UseColorGenerator = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.cryoGroup, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.heaterGroup, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.readButton, 0, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(530, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.96227F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.03773F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(147, 658);
            this.tableLayoutPanel2.TabIndex = 1;
            this.tableLayoutPanel2.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel2_Paint);
            // 
            // cryoGroup
            // 
            this.cryoGroup.Controls.Add(this.tableLayoutPanel4);
            this.cryoGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cryoGroup.Location = new System.Drawing.Point(3, 162);
            this.cryoGroup.Name = "cryoGroup";
            this.cryoGroup.Size = new System.Drawing.Size(141, 153);
            this.cryoGroup.TabIndex = 1;
            this.cryoGroup.TabStop = false;
            this.cryoGroup.Text = "Cryo-cooler";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.cryoSwitch, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.cryoLED, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(135, 134);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // cryoSwitch
            // 
            this.cryoSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoSwitch.Enabled = false;
            this.cryoSwitch.Location = new System.Drawing.Point(38, 56);
            this.cryoSwitch.Name = "cryoSwitch";
            this.cryoSwitch.Size = new System.Drawing.Size(59, 75);
            this.cryoSwitch.TabIndex = 0;
            this.cryoSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.toggleCryo);
            // 
            // cryoLED
            // 
            this.cryoLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoLED.Location = new System.Drawing.Point(43, 3);
            this.cryoLED.Name = "cryoLED";
            this.cryoLED.Size = new System.Drawing.Size(49, 47);
            this.cryoLED.TabIndex = 1;
            // 
            // heaterGroup
            // 
            this.heaterGroup.Controls.Add(this.tableLayoutPanel3);
            this.heaterGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heaterGroup.Location = new System.Drawing.Point(3, 3);
            this.heaterGroup.Name = "heaterGroup";
            this.heaterGroup.Size = new System.Drawing.Size(141, 153);
            this.heaterGroup.TabIndex = 0;
            this.heaterGroup.TabStop = false;
            this.heaterGroup.Text = "Heater";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.heaterSwitch, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.heaterLED, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(135, 134);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // heaterSwitch
            // 
            this.heaterSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterSwitch.Enabled = false;
            this.heaterSwitch.Location = new System.Drawing.Point(38, 56);
            this.heaterSwitch.Name = "heaterSwitch";
            this.heaterSwitch.Size = new System.Drawing.Size(59, 75);
            this.heaterSwitch.TabIndex = 0;
            this.heaterSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.toggleHeater);
            // 
            // heaterLED
            // 
            this.heaterLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterLED.Location = new System.Drawing.Point(43, 3);
            this.heaterLED.Name = "heaterLED";
            this.heaterLED.Size = new System.Drawing.Size(49, 47);
            this.heaterLED.TabIndex = 1;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.holdButton, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.cycleButton, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 321);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 57.03125F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.96875F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 61F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(141, 210);
            this.tableLayoutPanel5.TabIndex = 2;
            // 
            // holdButton
            // 
            this.holdButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.holdButton.Enabled = false;
            this.holdButton.Location = new System.Drawing.Point(33, 157);
            this.holdButton.Name = "holdButton";
            this.holdButton.Size = new System.Drawing.Size(75, 44);
            this.holdButton.TabIndex = 4;
            this.holdButton.Text = "Hold Source";
            this.holdButton.UseVisualStyleBackColor = true;
            this.holdButton.Click += new System.EventHandler(this.toggleHolding);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cycleLimit);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(135, 78);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cycle Max Temp (°C)";
            // 
            // cycleLimit
            // 
            this.cycleLimit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cycleLimit.Location = new System.Drawing.Point(6, 21);
            this.cycleLimit.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.cycleLimit.Name = "cycleLimit";
            this.cycleLimit.Size = new System.Drawing.Size(120, 20);
            this.cycleLimit.TabIndex = 3;
            this.cycleLimit.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.cycleLimit.ValueChanged += new System.EventHandler(this.cycleLimit_ValueChanged);
            // 
            // cycleButton
            // 
            this.cycleButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cycleButton.Enabled = false;
            this.cycleButton.Location = new System.Drawing.Point(33, 94);
            this.cycleButton.Name = "cycleButton";
            this.cycleButton.Size = new System.Drawing.Size(75, 44);
            this.cycleButton.TabIndex = 2;
            this.cycleButton.Text = "Cycle Source";
            this.cycleButton.UseVisualStyleBackColor = true;
            this.cycleButton.Click += new System.EventHandler(this.toggleCycling);
            // 
            // readButton
            // 
            this.readButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.readButton.Location = new System.Drawing.Point(36, 562);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(75, 46);
            this.readButton.TabIndex = 3;
            this.readButton.Text = "Start Reading";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.toggleReading);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.groupBox3, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 667);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(521, 140);
            this.tableLayoutPanel6.TabIndex = 2;
            this.tableLayoutPanel6.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel6_Paint);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.currentPressure);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(263, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(255, 134);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Source Pressure (mbar)";
            // 
            // currentPressure
            // 
            this.currentPressure.Location = new System.Drawing.Point(84, 56);
            this.currentPressure.Name = "currentPressure";
            this.currentPressure.ReadOnly = true;
            this.currentPressure.Size = new System.Drawing.Size(100, 20);
            this.currentPressure.TabIndex = 0;
            this.currentPressure.TextChanged += new System.EventHandler(this.currentPressure_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.rbThermLT);
            this.groupBox2.Controls.Add(this.numRref);
            this.groupBox2.Controls.Add(this.rbThermRT);
            this.groupBox2.Controls.Add(this.currentTemperature);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(254, 134);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Current Temperature (°C)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Thermistor in use:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(173, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Rref (kΩ):";
            // 
            // rbThermLT
            // 
            this.rbThermLT.AutoSize = true;
            this.rbThermLT.Enabled = false;
            this.rbThermLT.Location = new System.Drawing.Point(95, 111);
            this.rbThermLT.Name = "rbThermLT";
            this.rbThermLT.Size = new System.Drawing.Size(75, 17);
            this.rbThermLT.TabIndex = 3;
            this.rbThermLT.Text = "Low Temp";
            this.rbThermLT.UseVisualStyleBackColor = true;
            // 
            // numRref
            // 
            this.numRref.DecimalPlaces = 1;
            this.numRref.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numRref.Location = new System.Drawing.Point(176, 110);
            this.numRref.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numRref.Name = "numRref";
            this.numRref.Size = new System.Drawing.Size(72, 20);
            this.numRref.TabIndex = 3;
            this.numRref.Value = new decimal(new int[] {
            465,
            0,
            0,
            65536});
            this.numRref.ValueChanged += new System.EventHandler(this.numRref_ValueChanged);
            // 
            // rbThermRT
            // 
            this.rbThermRT.AutoSize = true;
            this.rbThermRT.Enabled = false;
            this.rbThermRT.Location = new System.Drawing.Point(6, 111);
            this.rbThermRT.Name = "rbThermRT";
            this.rbThermRT.Size = new System.Drawing.Size(83, 17);
            this.rbThermRT.TabIndex = 2;
            this.rbThermRT.Text = "Room Temp";
            this.rbThermRT.UseVisualStyleBackColor = true;
            // 
            // currentTemperature
            // 
            this.currentTemperature.Location = new System.Drawing.Point(75, 56);
            this.currentTemperature.Name = "currentTemperature";
            this.currentTemperature.ReadOnly = true;
            this.currentTemperature.Size = new System.Drawing.Size(100, 20);
            this.currentTemperature.TabIndex = 0;
            // 
            // SourceTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SourceTabView";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tempGraph)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.cryoGroup.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cryoSwitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cryoLED)).EndInit();
            this.heaterGroup.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.heaterSwitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heaterLED)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cycleLimit)).EndInit();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRref)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private NationalInstruments.UI.WindowsForms.ScatterGraph tempGraph;
        private NationalInstruments.UI.ScatterPlot scatterPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox cryoGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private NationalInstruments.UI.WindowsForms.Switch cryoSwitch;
        private NationalInstruments.UI.WindowsForms.Led cryoLED;
        private System.Windows.Forms.GroupBox heaterGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private NationalInstruments.UI.WindowsForms.Switch heaterSwitch;
        private NationalInstruments.UI.WindowsForms.Led heaterLED;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox currentTemperature;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox currentPressure;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rbThermLT;
        private System.Windows.Forms.RadioButton rbThermRT;
        private System.Windows.Forms.NumericUpDown numRref;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown cycleLimit;
        private System.Windows.Forms.Button cycleButton;
        private System.Windows.Forms.Button holdButton;

    }
}
