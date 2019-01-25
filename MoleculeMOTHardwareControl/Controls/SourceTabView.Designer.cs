namespace MoleculeMOTHardwareControl.Controls
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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.sf6Temperature = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.currentTemperature = new System.Windows.Forms.TextBox();
            this.cryoGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.cryoSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.cryoLED = new NationalInstruments.UI.WindowsForms.Led();
            this.heaterGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.heaterSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.heaterLED = new NationalInstruments.UI.WindowsForms.Led();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cycleLimit = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.holdButton = new System.Windows.Forms.Button();
            this.cycleButton = new System.Windows.Forms.Button();
            this.readButton = new System.Windows.Forms.Button();
            this.tempGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            this.tableLayoutPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tempGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.23529F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.76471F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tempGraph, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.97531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.02469F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(680, 810);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox3, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.cryoGroup, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.heaterGroup, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.readButton, 0, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(500, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(177, 658);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.sf6Temperature);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 592);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(171, 63);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "SF6 Temperature (°C)";
            // 
            // sf6Temperature
            // 
            this.sf6Temperature.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sf6Temperature.Location = new System.Drawing.Point(3, 16);
            this.sf6Temperature.Name = "sf6Temperature";
            this.sf6Temperature.ReadOnly = true;
            this.sf6Temperature.Size = new System.Drawing.Size(165, 20);
            this.sf6Temperature.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.currentTemperature);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 527);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(171, 59);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Source Temperature (°C)";
            // 
            // currentTemperature
            // 
            this.currentTemperature.Dock = System.Windows.Forms.DockStyle.Fill;
            this.currentTemperature.Location = new System.Drawing.Point(3, 16);
            this.currentTemperature.Name = "currentTemperature";
            this.currentTemperature.ReadOnly = true;
            this.currentTemperature.Size = new System.Drawing.Size(165, 20);
            this.currentTemperature.TabIndex = 0;
            // 
            // cryoGroup
            // 
            this.cryoGroup.Controls.Add(this.tableLayoutPanel4);
            this.cryoGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cryoGroup.Location = new System.Drawing.Point(3, 134);
            this.cryoGroup.Name = "cryoGroup";
            this.cryoGroup.Size = new System.Drawing.Size(171, 125);
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
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(165, 106);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // cryoSwitch
            // 
            this.cryoSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoSwitch.Enabled = false;
            this.cryoSwitch.Location = new System.Drawing.Point(53, 56);
            this.cryoSwitch.Name = "cryoSwitch";
            this.cryoSwitch.Size = new System.Drawing.Size(59, 47);
            this.cryoSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.cryoSwitch.TabIndex = 0;
            this.cryoSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.toggleCryo);
            // 
            // cryoLED
            // 
            this.cryoLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.cryoLED.Location = new System.Drawing.Point(58, 4);
            this.cryoLED.Name = "cryoLED";
            this.cryoLED.Size = new System.Drawing.Size(49, 45);
            this.cryoLED.TabIndex = 1;
            // 
            // heaterGroup
            // 
            this.heaterGroup.Controls.Add(this.tableLayoutPanel3);
            this.heaterGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heaterGroup.Location = new System.Drawing.Point(3, 3);
            this.heaterGroup.Name = "heaterGroup";
            this.heaterGroup.Size = new System.Drawing.Size(171, 125);
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
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(165, 106);
            this.tableLayoutPanel3.TabIndex = 0;
            this.tableLayoutPanel3.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel3_Paint);
            // 
            // heaterSwitch
            // 
            this.heaterSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterSwitch.Enabled = false;
            this.heaterSwitch.Location = new System.Drawing.Point(53, 56);
            this.heaterSwitch.Name = "heaterSwitch";
            this.heaterSwitch.Size = new System.Drawing.Size(59, 47);
            this.heaterSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.heaterSwitch.TabIndex = 0;
            this.heaterSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.toggleHeater);
            // 
            // heaterLED
            // 
            this.heaterLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.heaterLED.Location = new System.Drawing.Point(58, 5);
            this.heaterLED.Name = "heaterLED";
            this.heaterLED.Size = new System.Drawing.Size(49, 42);
            this.heaterLED.TabIndex = 1;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel7, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 265);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38.60759F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 61.39241F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(171, 125);
            this.tableLayoutPanel5.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cycleLimit);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(165, 42);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cycle Max Temp (°C)";
            // 
            // cycleLimit
            // 
            this.cycleLimit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cycleLimit.Location = new System.Drawing.Point(24, 12);
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
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.holdButton, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.cycleButton, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 51);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(165, 71);
            this.tableLayoutPanel7.TabIndex = 4;
            // 
            // holdButton
            // 
            this.holdButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.holdButton.Enabled = false;
            this.holdButton.Location = new System.Drawing.Point(93, 13);
            this.holdButton.Name = "holdButton";
            this.holdButton.Size = new System.Drawing.Size(61, 44);
            this.holdButton.TabIndex = 3;
            this.holdButton.Text = "Hold Source";
            this.holdButton.UseVisualStyleBackColor = true;
            this.holdButton.Click += new System.EventHandler(this.toggleHolding);
            // 
            // cycleButton
            // 
            this.cycleButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cycleButton.Enabled = false;
            this.cycleButton.Location = new System.Drawing.Point(10, 13);
            this.cycleButton.Name = "cycleButton";
            this.cycleButton.Size = new System.Drawing.Size(61, 44);
            this.cycleButton.TabIndex = 2;
            this.cycleButton.Text = "Cycle Source";
            this.cycleButton.UseVisualStyleBackColor = true;
            this.cycleButton.Click += new System.EventHandler(this.toggleCycling);
            // 
            // readButton
            // 
            this.readButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.readButton.Location = new System.Drawing.Point(51, 435);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(75, 46);
            this.readButton.TabIndex = 3;
            this.readButton.Text = "Start Reading";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.toggleReading);
            // 
            // tempGraph
            // 
            this.tempGraph.Location = new System.Drawing.Point(3, 3);
            this.tempGraph.Name = "tempGraph";
            this.tempGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.tempGraph.Size = new System.Drawing.Size(491, 658);
            this.tempGraph.TabIndex = 0;
            this.tempGraph.UseColorGenerator = true;
            this.tempGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.tempGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Caption = "Time";
            // 
            // yAxis1
            // 
            this.yAxis1.Caption = "Temperature (°C) ";
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.AutoScaleVisibleLoose;
            // 
            // SourceTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SourceTabView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
            this.tableLayoutPanel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tempGraph)).EndInit();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button cycleButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown cycleLimit;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox currentTemperature;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Button holdButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox sf6Temperature;

    }
}
