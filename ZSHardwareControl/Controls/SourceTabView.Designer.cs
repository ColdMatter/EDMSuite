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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.current40KTemperature = new System.Windows.Forms.TextBox();
            this.currentSF6Temperature = new System.Windows.Forms.TextBox();
            this.tempGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.recordButton = new System.Windows.Forms.Button();
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
            this.currentPressureNear = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rbThermLT = new System.Windows.Forms.RadioButton();
            this.numRref = new System.Windows.Forms.NumericUpDown();
            this.rbThermRT = new System.Windows.Forms.RadioButton();
            this.currentTemperature = new System.Windows.Forms.TextBox();
            this.currentPressureFar = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
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
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.5F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 0, 1);
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
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.current40KTemperature);
            this.groupBox4.Controls.Add(this.currentSF6Temperature);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(496, 667);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(181, 140);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Other Temperatures (°C)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "40K";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "SF6";
            // 
            // current40KTemperature
            // 
            this.current40KTemperature.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.current40KTemperature.Location = new System.Drawing.Point(49, 74);
            this.current40KTemperature.Name = "current40KTemperature";
            this.current40KTemperature.ReadOnly = true;
            this.current40KTemperature.Size = new System.Drawing.Size(100, 20);
            this.current40KTemperature.TabIndex = 1;
            // 
            // currentSF6Temperature
            // 
            this.currentSF6Temperature.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.currentSF6Temperature.Location = new System.Drawing.Point(49, 48);
            this.currentSF6Temperature.Name = "currentSF6Temperature";
            this.currentSF6Temperature.ReadOnly = true;
            this.currentSF6Temperature.Size = new System.Drawing.Size(100, 20);
            this.currentSF6Temperature.TabIndex = 0;
            this.currentSF6Temperature.TextChanged += new System.EventHandler(this.currentSF6Temperature_TextChanged);
            // 
            // tempGraph
            // 
            this.tempGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tempGraph.Location = new System.Drawing.Point(3, 3);
            this.tempGraph.Name = "tempGraph";
            this.tempGraph.Size = new System.Drawing.Size(487, 658);
            this.tempGraph.TabIndex = 0;
            this.tempGraph.UseColorGenerator = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.recordButton, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.cryoGroup, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.heaterGroup, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.readButton, 0, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(496, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38.39122F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.51737F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(181, 658);
            this.tableLayoutPanel2.TabIndex = 1;
            this.tableLayoutPanel2.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel2_Paint);
            // 
            // recordButton
            // 
            this.recordButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.recordButton.Enabled = false;
            this.recordButton.Location = new System.Drawing.Point(53, 596);
            this.recordButton.Name = "recordButton";
            this.recordButton.Size = new System.Drawing.Size(75, 46);
            this.recordButton.TabIndex = 4;
            this.recordButton.Text = "Start Recording";
            this.recordButton.UseVisualStyleBackColor = true;
            this.recordButton.Click += new System.EventHandler(this.toggleRecording);
            // 
            // cryoGroup
            // 
            this.cryoGroup.Controls.Add(this.tableLayoutPanel4);
            this.cryoGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cryoGroup.Location = new System.Drawing.Point(3, 148);
            this.cryoGroup.Name = "cryoGroup";
            this.cryoGroup.Size = new System.Drawing.Size(175, 139);
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
            this.tableLayoutPanel4.Size = new System.Drawing.Size(169, 120);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // cryoSwitch
            // 
            this.cryoSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoSwitch.Enabled = false;
            this.cryoSwitch.Location = new System.Drawing.Point(55, 51);
            this.cryoSwitch.Name = "cryoSwitch";
            this.cryoSwitch.Size = new System.Drawing.Size(59, 66);
            this.cryoSwitch.TabIndex = 0;
            this.cryoSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.toggleCryo);
            // 
            // cryoLED
            // 
            this.cryoLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoLED.Location = new System.Drawing.Point(60, 3);
            this.cryoLED.Name = "cryoLED";
            this.cryoLED.Size = new System.Drawing.Size(49, 42);
            this.cryoLED.TabIndex = 1;
            // 
            // heaterGroup
            // 
            this.heaterGroup.Controls.Add(this.tableLayoutPanel3);
            this.heaterGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heaterGroup.Location = new System.Drawing.Point(3, 3);
            this.heaterGroup.Name = "heaterGroup";
            this.heaterGroup.Size = new System.Drawing.Size(175, 139);
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
            this.tableLayoutPanel3.Size = new System.Drawing.Size(169, 120);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // heaterSwitch
            // 
            this.heaterSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterSwitch.Enabled = false;
            this.heaterSwitch.Location = new System.Drawing.Point(55, 51);
            this.heaterSwitch.Name = "heaterSwitch";
            this.heaterSwitch.Size = new System.Drawing.Size(59, 66);
            this.heaterSwitch.TabIndex = 0;
            this.heaterSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.toggleHeater);
            // 
            // heaterLED
            // 
            this.heaterLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterLED.Location = new System.Drawing.Point(60, 3);
            this.heaterLED.Name = "heaterLED";
            this.heaterLED.Size = new System.Drawing.Size(49, 42);
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
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 293);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 57.03125F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.96875F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 61F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(175, 218);
            this.tableLayoutPanel5.TabIndex = 2;
            // 
            // holdButton
            // 
            this.holdButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.holdButton.Enabled = false;
            this.holdButton.Location = new System.Drawing.Point(50, 165);
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
            this.groupBox1.Size = new System.Drawing.Size(169, 83);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cycle Max Temp (°C)";
            // 
            // cycleLimit
            // 
            this.cycleLimit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cycleLimit.Location = new System.Drawing.Point(23, 24);
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
            this.cycleButton.Location = new System.Drawing.Point(50, 100);
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
            this.readButton.Location = new System.Drawing.Point(53, 524);
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
            this.tableLayoutPanel6.Size = new System.Drawing.Size(487, 140);
            this.tableLayoutPanel6.TabIndex = 2;
            this.tableLayoutPanel6.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel6_Paint);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.currentPressureFar);
            this.groupBox3.Controls.Add(this.currentPressureNear);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(246, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(238, 134);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Source Pressure (mbar)";
            // 
            // currentPressureNear
            // 
            this.currentPressureNear.Location = new System.Drawing.Point(79, 45);
            this.currentPressureNear.Name = "currentPressureNear";
            this.currentPressureNear.ReadOnly = true;
            this.currentPressureNear.Size = new System.Drawing.Size(100, 20);
            this.currentPressureNear.TabIndex = 0;
            this.currentPressureNear.TextChanged += new System.EventHandler(this.currentPressure_TextChanged);
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
            this.groupBox2.Size = new System.Drawing.Size(237, 134);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Source Temp (RT: C, LT: K)";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
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
            // currentPressureFar
            // 
            this.currentPressureFar.Location = new System.Drawing.Point(79, 71);
            this.currentPressureFar.Name = "currentPressureFar";
            this.currentPressureFar.ReadOnly = true;
            this.currentPressureFar.Size = new System.Drawing.Size(100, 20);
            this.currentPressureFar.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(51, 74);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Far";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(43, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Near";
            // 
            // SourceTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SourceTabView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
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
        private System.Windows.Forms.TextBox currentPressureNear;
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
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox currentSF6Temperature;
        private System.Windows.Forms.Button recordButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox current40KTemperature;
        private System.Windows.Forms.TextBox currentPressureFar;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;

    }
}
