namespace AlFHardwareControl
{
    partial class AlFControlWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlFControlWindow));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TemperatureLayout = new System.Windows.Forms.TableLayoutPanel();
            this.LabelD = new System.Windows.Forms.Label();
            this.TempD = new System.Windows.Forms.Label();
            this.TempC = new System.Windows.Forms.Label();
            this.TempB = new System.Windows.Forms.Label();
            this.TempA = new System.Windows.Forms.Label();
            this.LabelA = new System.Windows.Forms.Label();
            this.LabelB = new System.Windows.Forms.Label();
            this.LabelC = new System.Windows.Forms.Label();
            this.MainTabs = new System.Windows.Forms.TabControl();
            this.P1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.P3Name = new System.Windows.Forms.Label();
            this.P3 = new System.Windows.Forms.Label();
            this.P2Name = new System.Windows.Forms.Label();
            this.P2 = new System.Windows.Forms.Label();
            this.P1Name = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.CryoStatus = new System.Windows.Forms.Label();
            this.EngageCryo = new System.Windows.Forms.Button();
            this.DisengageCryo = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.Loop1Status = new System.Windows.Forms.Label();
            this.Loop1Engage = new System.Windows.Forms.Button();
            this.Loop1Disengage = new System.Windows.Forms.Button();
            this.Loop1Temp = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.Loop2Status = new System.Windows.Forms.Label();
            this.Loop2Engage = new System.Windows.Forms.Button();
            this.Loop2Disengage = new System.Windows.Forms.Button();
            this.Loop2Temp = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.Loop1Out = new System.Windows.Forms.ProgressBar();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.Loop2Out = new System.Windows.Forms.ProgressBar();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.SafetyInterlockStatus = new System.Windows.Forms.Label();
            this.SafetyInterlockEngage = new System.Windows.Forms.Button();
            this.SafetyInterlockDisengage = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.TemperatureLayout.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TemperatureLayout);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 105);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Diode Temperature";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // TemperatureLayout
            // 
            this.TemperatureLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TemperatureLayout.ColumnCount = 2;
            this.TemperatureLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.19786F));
            this.TemperatureLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.80214F));
            this.TemperatureLayout.Controls.Add(this.LabelD, 0, 3);
            this.TemperatureLayout.Controls.Add(this.TempD, 1, 3);
            this.TemperatureLayout.Controls.Add(this.TempC, 1, 2);
            this.TemperatureLayout.Controls.Add(this.TempB, 1, 1);
            this.TemperatureLayout.Controls.Add(this.TempA, 1, 0);
            this.TemperatureLayout.Controls.Add(this.LabelA, 0, 0);
            this.TemperatureLayout.Controls.Add(this.LabelB, 0, 1);
            this.TemperatureLayout.Controls.Add(this.LabelC, 0, 2);
            this.TemperatureLayout.Location = new System.Drawing.Point(7, 19);
            this.TemperatureLayout.Name = "TemperatureLayout";
            this.TemperatureLayout.RowCount = 4;
            this.TemperatureLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.TemperatureLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.TemperatureLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.TemperatureLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.TemperatureLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TemperatureLayout.Size = new System.Drawing.Size(187, 80);
            this.TemperatureLayout.TabIndex = 0;
            this.TemperatureLayout.Paint += new System.Windows.Forms.PaintEventHandler(this.TemperatureLayout_Paint);
            // 
            // LabelD
            // 
            this.LabelD.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LabelD.AutoSize = true;
            this.LabelD.Location = new System.Drawing.Point(53, 63);
            this.LabelD.Name = "LabelD";
            this.LabelD.Size = new System.Drawing.Size(35, 13);
            this.LabelD.TabIndex = 11;
            this.LabelD.Text = "label1";
            // 
            // TempD
            // 
            this.TempD.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.TempD.AutoSize = true;
            this.TempD.Location = new System.Drawing.Point(149, 63);
            this.TempD.Name = "TempD";
            this.TempD.Size = new System.Drawing.Size(35, 13);
            this.TempD.TabIndex = 7;
            this.TempD.Text = "label4";
            // 
            // TempC
            // 
            this.TempC.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.TempC.AutoSize = true;
            this.TempC.Location = new System.Drawing.Point(149, 43);
            this.TempC.Name = "TempC";
            this.TempC.Size = new System.Drawing.Size(35, 13);
            this.TempC.TabIndex = 6;
            this.TempC.Text = "label3";
            // 
            // TempB
            // 
            this.TempB.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.TempB.AutoSize = true;
            this.TempB.Location = new System.Drawing.Point(149, 23);
            this.TempB.Name = "TempB";
            this.TempB.Size = new System.Drawing.Size(35, 13);
            this.TempB.TabIndex = 5;
            this.TempB.Text = "label2";
            this.TempB.Click += new System.EventHandler(this.tempB_Click);
            // 
            // TempA
            // 
            this.TempA.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.TempA.AutoSize = true;
            this.TempA.Location = new System.Drawing.Point(149, 3);
            this.TempA.Name = "TempA";
            this.TempA.Size = new System.Drawing.Size(35, 13);
            this.TempA.TabIndex = 4;
            this.TempA.Text = "label1";
            this.TempA.Click += new System.EventHandler(this.TempA_Click);
            // 
            // LabelA
            // 
            this.LabelA.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LabelA.AutoSize = true;
            this.LabelA.Location = new System.Drawing.Point(53, 3);
            this.LabelA.Name = "LabelA";
            this.LabelA.Size = new System.Drawing.Size(35, 13);
            this.LabelA.TabIndex = 8;
            this.LabelA.Text = "label1";
            this.LabelA.Click += new System.EventHandler(this.label1_Click_2);
            // 
            // LabelB
            // 
            this.LabelB.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LabelB.AutoSize = true;
            this.LabelB.Location = new System.Drawing.Point(53, 23);
            this.LabelB.Name = "LabelB";
            this.LabelB.Size = new System.Drawing.Size(35, 13);
            this.LabelB.TabIndex = 9;
            this.LabelB.Text = "label1";
            // 
            // LabelC
            // 
            this.LabelC.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.LabelC.AutoSize = true;
            this.LabelC.Location = new System.Drawing.Point(53, 43);
            this.LabelC.Name = "LabelC";
            this.LabelC.Size = new System.Drawing.Size(35, 13);
            this.LabelC.TabIndex = 10;
            this.LabelC.Text = "label1";
            // 
            // MainTabs
            // 
            this.MainTabs.Location = new System.Drawing.Point(219, 13);
            this.MainTabs.Name = "MainTabs";
            this.MainTabs.SelectedIndex = 0;
            this.MainTabs.Size = new System.Drawing.Size(1188, 437);
            this.MainTabs.TabIndex = 1;
            // 
            // P1
            // 
            this.P1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.P1.AutoSize = true;
            this.P1.Location = new System.Drawing.Point(149, 4);
            this.P1.Name = "P1";
            this.P1.Size = new System.Drawing.Size(35, 13);
            this.P1.TabIndex = 4;
            this.P1.Text = "label1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel1);
            this.groupBox2.Location = new System.Drawing.Point(12, 123);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 92);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pressure";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.P3Name, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.P3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.P2Name, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.P2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.P1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.P1Name, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(187, 67);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // P3Name
            // 
            this.P3Name.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.P3Name.AutoSize = true;
            this.P3Name.Location = new System.Drawing.Point(42, 49);
            this.P3Name.Name = "P3Name";
            this.P3Name.Size = new System.Drawing.Size(48, 13);
            this.P3Name.TabIndex = 9;
            this.P3Name.Text = "P3Name";
            // 
            // P3
            // 
            this.P3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.P3.AutoSize = true;
            this.P3.Location = new System.Drawing.Point(164, 49);
            this.P3.Name = "P3";
            this.P3.Size = new System.Drawing.Size(20, 13);
            this.P3.TabIndex = 8;
            this.P3.Text = "P3";
            // 
            // P2Name
            // 
            this.P2Name.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.P2Name.AutoSize = true;
            this.P2Name.Location = new System.Drawing.Point(50, 26);
            this.P2Name.Name = "P2Name";
            this.P2Name.Size = new System.Drawing.Size(40, 13);
            this.P2Name.TabIndex = 7;
            this.P2Name.Text = "Loop 2";
            this.P2Name.Click += new System.EventHandler(this.label3_Click_1);
            // 
            // P2
            // 
            this.P2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.P2.AutoSize = true;
            this.P2.Location = new System.Drawing.Point(149, 26);
            this.P2.Name = "P2";
            this.P2.Size = new System.Drawing.Size(35, 13);
            this.P2.TabIndex = 5;
            this.P2.Text = "label2";
            // 
            // P1Name
            // 
            this.P1Name.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.P1Name.AutoSize = true;
            this.P1Name.Location = new System.Drawing.Point(50, 4);
            this.P1Name.Name = "P1Name";
            this.P1Name.Size = new System.Drawing.Size(40, 13);
            this.P1Name.TabIndex = 6;
            this.P1Name.Text = "Loop 1";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.CryoStatus, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.EngageCryo, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.DisengageCryo, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 37.2093F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62.7907F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(187, 43);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // CryoStatus
            // 
            this.CryoStatus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.CryoStatus.AutoSize = true;
            this.CryoStatus.Location = new System.Drawing.Point(161, 1);
            this.CryoStatus.Name = "CryoStatus";
            this.CryoStatus.Size = new System.Drawing.Size(23, 13);
            this.CryoStatus.TabIndex = 5;
            this.CryoStatus.Text = "ON";
            // 
            // EngageCryo
            // 
            this.EngageCryo.Enabled = false;
            this.EngageCryo.Location = new System.Drawing.Point(3, 18);
            this.EngageCryo.Name = "EngageCryo";
            this.EngageCryo.Size = new System.Drawing.Size(87, 22);
            this.EngageCryo.TabIndex = 0;
            this.EngageCryo.Text = "Engage";
            this.EngageCryo.UseVisualStyleBackColor = true;
            this.EngageCryo.Click += new System.EventHandler(this.EngageCryo_Click);
            // 
            // DisengageCryo
            // 
            this.DisengageCryo.Enabled = false;
            this.DisengageCryo.Location = new System.Drawing.Point(96, 18);
            this.DisengageCryo.Name = "DisengageCryo";
            this.DisengageCryo.Size = new System.Drawing.Size(88, 22);
            this.DisengageCryo.TabIndex = 1;
            this.DisengageCryo.Text = "Disengage";
            this.DisengageCryo.UseVisualStyleBackColor = true;
            this.DisengageCryo.Click += new System.EventHandler(this.DisengageCryo_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(53, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Status";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Controls.Add(this.Loop1Status, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.Loop1Engage, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.Loop1Disengage, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.Loop1Temp, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 37.2093F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62.7907F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(187, 43);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // Loop1Status
            // 
            this.Loop1Status.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Loop1Status.AutoSize = true;
            this.Loop1Status.Location = new System.Drawing.Point(165, 1);
            this.Loop1Status.Name = "Loop1Status";
            this.Loop1Status.Size = new System.Drawing.Size(19, 13);
            this.Loop1Status.TabIndex = 5;
            this.Loop1Status.Text = "??";
            // 
            // Loop1Engage
            // 
            this.Loop1Engage.Enabled = false;
            this.Loop1Engage.Location = new System.Drawing.Point(3, 18);
            this.Loop1Engage.Name = "Loop1Engage";
            this.Loop1Engage.Size = new System.Drawing.Size(87, 22);
            this.Loop1Engage.TabIndex = 0;
            this.Loop1Engage.Text = "Engage";
            this.Loop1Engage.UseVisualStyleBackColor = true;
            this.Loop1Engage.Click += new System.EventHandler(this.Loop1Engage_Click);
            // 
            // Loop1Disengage
            // 
            this.Loop1Disengage.Enabled = false;
            this.Loop1Disengage.Location = new System.Drawing.Point(96, 18);
            this.Loop1Disengage.Name = "Loop1Disengage";
            this.Loop1Disengage.Size = new System.Drawing.Size(88, 22);
            this.Loop1Disengage.TabIndex = 1;
            this.Loop1Disengage.Text = "Disengage";
            this.Loop1Disengage.UseVisualStyleBackColor = true;
            this.Loop1Disengage.Click += new System.EventHandler(this.Loop1Disengage_Click);
            // 
            // Loop1Temp
            // 
            this.Loop1Temp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Loop1Temp.AutoSize = true;
            this.Loop1Temp.Location = new System.Drawing.Point(53, 1);
            this.Loop1Temp.Name = "Loop1Temp";
            this.Loop1Temp.Size = new System.Drawing.Size(37, 13);
            this.Loop1Temp.TabIndex = 2;
            this.Loop1Temp.Text = "Status";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Controls.Add(this.Loop2Status, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.Loop2Engage, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.Loop2Disengage, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.Loop2Temp, 0, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 37.2093F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62.7907F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(187, 43);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // Loop2Status
            // 
            this.Loop2Status.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Loop2Status.AutoSize = true;
            this.Loop2Status.Location = new System.Drawing.Point(165, 1);
            this.Loop2Status.Name = "Loop2Status";
            this.Loop2Status.Size = new System.Drawing.Size(19, 13);
            this.Loop2Status.TabIndex = 5;
            this.Loop2Status.Text = "??";
            // 
            // Loop2Engage
            // 
            this.Loop2Engage.Enabled = false;
            this.Loop2Engage.Location = new System.Drawing.Point(3, 18);
            this.Loop2Engage.Name = "Loop2Engage";
            this.Loop2Engage.Size = new System.Drawing.Size(87, 22);
            this.Loop2Engage.TabIndex = 0;
            this.Loop2Engage.Text = "Engage";
            this.Loop2Engage.UseVisualStyleBackColor = true;
            this.Loop2Engage.Click += new System.EventHandler(this.Loop2Engage_Click);
            // 
            // Loop2Disengage
            // 
            this.Loop2Disengage.Enabled = false;
            this.Loop2Disengage.Location = new System.Drawing.Point(96, 18);
            this.Loop2Disengage.Name = "Loop2Disengage";
            this.Loop2Disengage.Size = new System.Drawing.Size(88, 22);
            this.Loop2Disengage.TabIndex = 1;
            this.Loop2Disengage.Text = "Disengage";
            this.Loop2Disengage.UseVisualStyleBackColor = true;
            this.Loop2Disengage.Click += new System.EventHandler(this.Loop2Disengage_Click);
            // 
            // Loop2Temp
            // 
            this.Loop2Temp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Loop2Temp.AutoSize = true;
            this.Loop2Temp.Location = new System.Drawing.Point(53, 1);
            this.Loop2Temp.Name = "Loop2Temp";
            this.Loop2Temp.Size = new System.Drawing.Size(37, 13);
            this.Loop2Temp.TabIndex = 2;
            this.Loop2Temp.Text = "Status";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 221);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(200, 88);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.tableLayoutPanel2);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(192, 62);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Cryo";
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.Loop1Out);
            this.tabPage1.Controls.Add(this.tableLayoutPanel3);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(192, 62);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Loop1";
            // 
            // Loop1Out
            // 
            this.Loop1Out.Location = new System.Drawing.Point(2, 49);
            this.Loop1Out.Name = "Loop1Out";
            this.Loop1Out.Size = new System.Drawing.Size(188, 10);
            this.Loop1Out.TabIndex = 7;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.Loop2Out);
            this.tabPage2.Controls.Add(this.tableLayoutPanel4);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 62);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Loop2";
            // 
            // Loop2Out
            // 
            this.Loop2Out.Location = new System.Drawing.Point(2, 49);
            this.Loop2Out.Name = "Loop2Out";
            this.Loop2Out.Size = new System.Drawing.Size(188, 10);
            this.Loop2Out.TabIndex = 1;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel5);
            this.groupBox3.Location = new System.Drawing.Point(12, 312);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 66);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Safety Interlocks";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.SafetyInterlockStatus, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.SafetyInterlockEngage, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.SafetyInterlockDisengage, 1, 1);
            this.tableLayoutPanel5.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel5.Location = new System.Drawing.Point(6, 17);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 37.2093F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62.7907F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(187, 43);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // SafetyInterlockStatus
            // 
            this.SafetyInterlockStatus.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.SafetyInterlockStatus.AutoSize = true;
            this.SafetyInterlockStatus.Location = new System.Drawing.Point(139, 1);
            this.SafetyInterlockStatus.Name = "SafetyInterlockStatus";
            this.SafetyInterlockStatus.Size = new System.Drawing.Size(45, 13);
            this.SafetyInterlockStatus.TabIndex = 5;
            this.SafetyInterlockStatus.Text = "ACTIVE";
            // 
            // SafetyInterlockEngage
            // 
            this.SafetyInterlockEngage.Enabled = false;
            this.SafetyInterlockEngage.Location = new System.Drawing.Point(3, 18);
            this.SafetyInterlockEngage.Name = "SafetyInterlockEngage";
            this.SafetyInterlockEngage.Size = new System.Drawing.Size(87, 22);
            this.SafetyInterlockEngage.TabIndex = 0;
            this.SafetyInterlockEngage.Text = "Engage";
            this.SafetyInterlockEngage.UseVisualStyleBackColor = true;
            this.SafetyInterlockEngage.Click += new System.EventHandler(this.SafetyInterlockEngage_Click);
            // 
            // SafetyInterlockDisengage
            // 
            this.SafetyInterlockDisengage.Location = new System.Drawing.Point(96, 18);
            this.SafetyInterlockDisengage.Name = "SafetyInterlockDisengage";
            this.SafetyInterlockDisengage.Size = new System.Drawing.Size(88, 22);
            this.SafetyInterlockDisengage.TabIndex = 1;
            this.SafetyInterlockDisengage.Text = "Disengage";
            this.SafetyInterlockDisengage.UseVisualStyleBackColor = true;
            this.SafetyInterlockDisengage.Click += new System.EventHandler(this.SafetyInterlockDisengage_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(53, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Status";
            // 
            // AlFControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1419, 462);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.MainTabs);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AlFControlWindow";
            this.Text = "AlF Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AlFControlWindow_FormClosing);
            this.Load += new System.EventHandler(this.AlFControlWindow_Load);
            this.groupBox1.ResumeLayout(false);
            this.TemperatureLayout.ResumeLayout(false);
            this.TemperatureLayout.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TabControl MainTabs;
        public System.Windows.Forms.TableLayoutPanel TemperatureLayout;
        public System.Windows.Forms.Label TempD;
        public System.Windows.Forms.Label TempC;
        public System.Windows.Forms.Label TempB;
        public System.Windows.Forms.Label TempA;
        public System.Windows.Forms.Label P1;
        public System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Label P2Name;
        public System.Windows.Forms.Label P2;
        public System.Windows.Forms.Label P1Name;
        public System.Windows.Forms.Label LabelA;
        public System.Windows.Forms.Label LabelD;
        public System.Windows.Forms.Label LabelB;
        public System.Windows.Forms.Label LabelC;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        public System.Windows.Forms.Label CryoStatus;
        public System.Windows.Forms.Button EngageCryo;
        public System.Windows.Forms.Button DisengageCryo;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        public System.Windows.Forms.Label Loop1Status;
        public System.Windows.Forms.Button Loop1Engage;
        public System.Windows.Forms.Button Loop1Disengage;
        public System.Windows.Forms.Label Loop1Temp;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        public System.Windows.Forms.Label Loop2Status;
        public System.Windows.Forms.Button Loop2Engage;
        public System.Windows.Forms.Button Loop2Disengage;
        public System.Windows.Forms.Label Loop2Temp;
        public System.Windows.Forms.Label P3Name;
        public System.Windows.Forms.Label P3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        public System.Windows.Forms.ProgressBar Loop2Out;
        public System.Windows.Forms.ProgressBar Loop1Out;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        public System.Windows.Forms.Label SafetyInterlockStatus;
        public System.Windows.Forms.Button SafetyInterlockEngage;
        public System.Windows.Forms.Button SafetyInterlockDisengage;
        public System.Windows.Forms.Label label3;
    }
}

