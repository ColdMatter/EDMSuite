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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.sf6Temperature = new System.Windows.Forms.TextBox();
            this.currentTemperature = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSourcePressure = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkLog = new System.Windows.Forms.CheckBox();
            this.txtSourceTemp2 = new System.Windows.Forms.TextBox();
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
            this.chkToF = new System.Windows.Forms.CheckBox();
            this.tempGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.chkSaveTrace = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
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
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 71.61765F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.38235F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tempGraph, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chkSaveTrace, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 77.16049F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.83951F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(680, 810);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.cryoGroup, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.heaterGroup, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.readButton, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.chkToF, 0, 5);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(490, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.206687F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 26.13982F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6.079027F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(187, 618);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.sf6Temperature);
            this.groupBox2.Controls.Add(this.currentTemperature);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtSourcePressure);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.chkLog);
            this.groupBox2.Controls.Add(this.txtSourceTemp2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 422);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(181, 154);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Source Parameters";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "sf6 Temp:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Source Temp:";
            // 
            // sf6Temperature
            // 
            this.sf6Temperature.Location = new System.Drawing.Point(78, 62);
            this.sf6Temperature.Name = "sf6Temperature";
            this.sf6Temperature.ReadOnly = true;
            this.sf6Temperature.Size = new System.Drawing.Size(100, 20);
            this.sf6Temperature.TabIndex = 7;
            // 
            // currentTemperature
            // 
            this.currentTemperature.Location = new System.Drawing.Point(78, 17);
            this.currentTemperature.Name = "currentTemperature";
            this.currentTemperature.ReadOnly = true;
            this.currentTemperature.Size = new System.Drawing.Size(100, 20);
            this.currentTemperature.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Source Temp:";
            // 
            // txtSourcePressure
            // 
            this.txtSourcePressure.Location = new System.Drawing.Point(57, 88);
            this.txtSourcePressure.Name = "txtSourcePressure";
            this.txtSourcePressure.ReadOnly = true;
            this.txtSourcePressure.Size = new System.Drawing.Size(121, 20);
            this.txtSourcePressure.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Pressure";
            // 
            // chkLog
            // 
            this.chkLog.AutoSize = true;
            this.chkLog.Location = new System.Drawing.Point(3, 116);
            this.chkLog.Name = "chkLog";
            this.chkLog.Size = new System.Drawing.Size(100, 17);
            this.chkLog.TabIndex = 2;
            this.chkLog.Text = "Log Parameters";
            this.chkLog.UseVisualStyleBackColor = true;
            // 
            // txtSourceTemp2
            // 
            this.txtSourceTemp2.Location = new System.Drawing.Point(78, 39);
            this.txtSourceTemp2.Name = "txtSourceTemp2";
            this.txtSourceTemp2.ReadOnly = true;
            this.txtSourceTemp2.Size = new System.Drawing.Size(100, 20);
            this.txtSourceTemp2.TabIndex = 1;
            // 
            // cryoGroup
            // 
            this.cryoGroup.Controls.Add(this.tableLayoutPanel4);
            this.cryoGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cryoGroup.Location = new System.Drawing.Point(3, 126);
            this.cryoGroup.Name = "cryoGroup";
            this.cryoGroup.Size = new System.Drawing.Size(181, 117);
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
            this.tableLayoutPanel4.Size = new System.Drawing.Size(175, 98);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // cryoSwitch
            // 
            this.cryoSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoSwitch.Enabled = false;
            this.cryoSwitch.Location = new System.Drawing.Point(58, 52);
            this.cryoSwitch.Name = "cryoSwitch";
            this.cryoSwitch.Size = new System.Drawing.Size(59, 43);
            this.cryoSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.cryoSwitch.TabIndex = 0;
            this.cryoSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.toggleCryo);
            // 
            // cryoLED
            // 
            this.cryoLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.cryoLED.Location = new System.Drawing.Point(63, 3);
            this.cryoLED.Name = "cryoLED";
            this.cryoLED.Size = new System.Drawing.Size(49, 43);
            this.cryoLED.TabIndex = 1;
            // 
            // heaterGroup
            // 
            this.heaterGroup.Controls.Add(this.tableLayoutPanel3);
            this.heaterGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heaterGroup.Location = new System.Drawing.Point(3, 3);
            this.heaterGroup.Name = "heaterGroup";
            this.heaterGroup.Size = new System.Drawing.Size(181, 117);
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
            this.tableLayoutPanel3.Size = new System.Drawing.Size(175, 98);
            this.tableLayoutPanel3.TabIndex = 0;
            this.tableLayoutPanel3.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel3_Paint);
            // 
            // heaterSwitch
            // 
            this.heaterSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterSwitch.Enabled = false;
            this.heaterSwitch.Location = new System.Drawing.Point(58, 52);
            this.heaterSwitch.Name = "heaterSwitch";
            this.heaterSwitch.Size = new System.Drawing.Size(59, 43);
            this.heaterSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.heaterSwitch.TabIndex = 0;
            this.heaterSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.toggleHeater);
            // 
            // heaterLED
            // 
            this.heaterLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.heaterLED.Location = new System.Drawing.Point(63, 3);
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
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 249);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38.60759F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 61.39241F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(181, 117);
            this.tableLayoutPanel5.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cycleLimit);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(175, 39);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cycle Max Temp (°C)";
            // 
            // cycleLimit
            // 
            this.cycleLimit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cycleLimit.Location = new System.Drawing.Point(29, 11);
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
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 48);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(175, 66);
            this.tableLayoutPanel7.TabIndex = 4;
            // 
            // holdButton
            // 
            this.holdButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.holdButton.Enabled = false;
            this.holdButton.Location = new System.Drawing.Point(100, 11);
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
            this.cycleButton.Location = new System.Drawing.Point(13, 11);
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
            this.readButton.Location = new System.Drawing.Point(44, 372);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(99, 44);
            this.readButton.TabIndex = 3;
            this.readButton.Text = "Start Reading";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.toggleReading);
            // 
            // chkToF
            // 
            this.chkToF.AutoSize = true;
            this.chkToF.Location = new System.Drawing.Point(3, 582);
            this.chkToF.Name = "chkToF";
            this.chkToF.Size = new System.Drawing.Size(131, 17);
            this.chkToF.TabIndex = 5;
            this.chkToF.Text = "Show ToF PMT signal";
            this.chkToF.UseVisualStyleBackColor = true;
            // 
            // tempGraph
            // 
            this.tempGraph.Location = new System.Drawing.Point(3, 3);
            this.tempGraph.Name = "tempGraph";
            this.tempGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.tempGraph.Size = new System.Drawing.Size(480, 240);
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
            // chkSaveTrace
            // 
            this.chkSaveTrace.AutoSize = true;
            this.chkSaveTrace.Location = new System.Drawing.Point(490, 627);
            this.chkSaveTrace.Name = "chkSaveTrace";
            this.chkSaveTrace.Size = new System.Drawing.Size(104, 17);
            this.chkSaveTrace.TabIndex = 2;
            this.chkSaveTrace.Text = "Save ToF Trace";
            this.chkSaveTrace.UseVisualStyleBackColor = true;
            // 
            // SourceTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SourceTabView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Button holdButton;
        private System.Windows.Forms.TextBox txtSourceTemp2;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.TextBox txtSourcePressure;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox sf6Temperature;
        private System.Windows.Forms.TextBox currentTemperature;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkToF;
        private System.Windows.Forms.CheckBox chkSaveTrace;

    }
}
