namespace MoleculeMOTHardwareControl.Controls
{
    partial class TrackView
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
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxIterations = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.textBoxTweezer = new System.Windows.Forms.TextBox();
            this.RBTrigger = new System.Windows.Forms.RadioButton();
            this.label14 = new System.Windows.Forms.Label();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.buttonRunTCL = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxLog = new System.Windows.Forms.CheckBox();
            this.buttonHome = new System.Windows.Forms.Button();
            this.buttonKill = new System.Windows.Forms.Button();
            this.buttonInitialize = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.TextBox_Group = new System.Windows.Forms.TextBox();
            this.textBox_IPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.textBox_IPPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.BTN_update = new System.Windows.Forms.Button();
            this.textBoxVelocityInput = new System.Windows.Forms.TextBox();
            this.textBoxAccelerationInput = new System.Windows.Forms.TextBox();
            this.textBoxVelocity = new System.Windows.Forms.TextBox();
            this.textBoxAcceleration = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxPosition = new System.Windows.Forms.TextBox();
            this.TxBoundTweezer = new System.Windows.Forms.TextBox();
            this.TxBoundSource = new System.Windows.Forms.TextBox();
            this.TxMaxVel = new System.Windows.Forms.TextBox();
            this.TxMaxAccel = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.RBManual = new System.Windows.Forms.RadioButton();
            this.textBoxTarget = new System.Windows.Forms.TextBox();
            this.buttonMoveTo = new System.Windows.Forms.Button();
            this.label_MessageCommunication = new System.Windows.Forms.Label();
            this.label_GroupStatusDescription = new System.Windows.Forms.Label();
            this.label_ErrorMessage = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40.73529F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 59.26471F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 74.44052F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(680, 514);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.groupBox4.Controls.Add(this.label13);
            this.groupBox4.Controls.Add(this.textBoxIterations);
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.textBoxTweezer);
            this.groupBox4.Controls.Add(this.RBTrigger);
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.textBoxSource);
            this.groupBox4.Controls.Add(this.buttonRunTCL);
            this.groupBox4.Location = new System.Drawing.Point(300, 384);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(355, 110);
            this.groupBox4.TabIndex = 6;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Triggered positioning";
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(247, 22);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(50, 13);
            this.label13.TabIndex = 38;
            this.label13.Text = "Iterations";
            this.label13.Click += new System.EventHandler(this.label13_Click);
            // 
            // textBoxIterations
            // 
            this.textBoxIterations.Location = new System.Drawing.Point(303, 19);
            this.textBoxIterations.Name = "textBoxIterations";
            this.textBoxIterations.Size = new System.Drawing.Size(31, 20);
            this.textBoxIterations.TabIndex = 37;
            this.textBoxIterations.Text = "0";
            this.textBoxIterations.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label15
            // 
            this.label15.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(239, 47);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(72, 13);
            this.label15.TabIndex = 36;
            this.label15.Text = "Pos. Tweezer";
            // 
            // textBoxTweezer
            // 
            this.textBoxTweezer.Location = new System.Drawing.Point(233, 65);
            this.textBoxTweezer.Name = "textBoxTweezer";
            this.textBoxTweezer.Size = new System.Drawing.Size(100, 20);
            this.textBoxTweezer.TabIndex = 35;
            this.textBoxTweezer.Text = "0";
            this.textBoxTweezer.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // RBTrigger
            // 
            this.RBTrigger.AutoSize = true;
            this.RBTrigger.Location = new System.Drawing.Point(30, 26);
            this.RBTrigger.Name = "RBTrigger";
            this.RBTrigger.Size = new System.Drawing.Size(14, 13);
            this.RBTrigger.TabIndex = 34;
            this.RBTrigger.TabStop = true;
            this.RBTrigger.UseVisualStyleBackColor = true;
            this.RBTrigger.CheckedChanged += new System.EventHandler(this.RBTrigger_CheckedChanged);
            this.RBTrigger.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RBTrigger_MouseDown);
            // 
            // label14
            // 
            this.label14.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(143, 47);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 13);
            this.label14.TabIndex = 33;
            this.label14.Text = "Pos. Source";
            // 
            // textBoxSource
            // 
            this.textBoxSource.Location = new System.Drawing.Point(125, 65);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(100, 20);
            this.textBoxSource.TabIndex = 29;
            this.textBoxSource.Text = "0";
            this.textBoxSource.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // buttonRunTCL
            // 
            this.buttonRunTCL.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonRunTCL.Location = new System.Drawing.Point(11, 55);
            this.buttonRunTCL.Name = "buttonRunTCL";
            this.buttonRunTCL.Size = new System.Drawing.Size(97, 38);
            this.buttonRunTCL.TabIndex = 28;
            this.buttonRunTCL.Text = "Run stage script";
            this.buttonRunTCL.UseVisualStyleBackColor = true;
            this.buttonRunTCL.Click += new System.EventHandler(this.buttonRunTCL_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.Controls.Add(this.checkBoxLog);
            this.groupBox1.Controls.Add(this.buttonHome);
            this.groupBox1.Controls.Add(this.buttonKill);
            this.groupBox1.Controls.Add(this.buttonInitialize);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TextBox_Group);
            this.groupBox1.Controls.Add(this.textBox_IPAddress);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonConnect);
            this.groupBox1.Controls.Add(this.buttonDisconnect);
            this.groupBox1.Controls.Add(this.textBox_IPPort);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(8, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 353);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Track connection and initialization";
            // 
            // checkBoxLog
            // 
            this.checkBoxLog.AutoSize = true;
            this.checkBoxLog.Location = new System.Drawing.Point(138, 300);
            this.checkBoxLog.Name = "checkBoxLog";
            this.checkBoxLog.Size = new System.Drawing.Size(72, 17);
            this.checkBoxLog.TabIndex = 24;
            this.checkBoxLog.Text = "Save Log";
            this.checkBoxLog.UseVisualStyleBackColor = true;
            // 
            // buttonHome
            // 
            this.buttonHome.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonHome.Location = new System.Drawing.Point(17, 290);
            this.buttonHome.Name = "buttonHome";
            this.buttonHome.Size = new System.Drawing.Size(97, 38);
            this.buttonHome.TabIndex = 23;
            this.buttonHome.Text = "Home";
            this.buttonHome.UseVisualStyleBackColor = true;
            this.buttonHome.Click += new System.EventHandler(this.buttonHome_Click);
            // 
            // buttonKill
            // 
            this.buttonKill.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonKill.Location = new System.Drawing.Point(126, 233);
            this.buttonKill.Name = "buttonKill";
            this.buttonKill.Size = new System.Drawing.Size(97, 38);
            this.buttonKill.TabIndex = 22;
            this.buttonKill.Text = "Kill";
            this.buttonKill.UseVisualStyleBackColor = true;
            this.buttonKill.Click += new System.EventHandler(this.buttonKill_Click);
            // 
            // buttonInitialize
            // 
            this.buttonInitialize.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonInitialize.Location = new System.Drawing.Point(17, 233);
            this.buttonInitialize.Name = "buttonInitialize";
            this.buttonInitialize.Size = new System.Drawing.Size(97, 38);
            this.buttonInitialize.TabIndex = 21;
            this.buttonInitialize.Text = "Initialize";
            this.buttonInitialize.UseVisualStyleBackColor = true;
            this.buttonInitialize.Click += new System.EventHandler(this.buttonInitialize_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 196);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Positioner name";
            // 
            // TextBox_Group
            // 
            this.TextBox_Group.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TextBox_Group.Location = new System.Drawing.Point(123, 193);
            this.TextBox_Group.Name = "TextBox_Group";
            this.TextBox_Group.Size = new System.Drawing.Size(100, 20);
            this.TextBox_Group.TabIndex = 19;
            this.TextBox_Group.Text = "Group1.Pos";
            // 
            // textBox_IPAddress
            // 
            this.textBox_IPAddress.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textBox_IPAddress.Location = new System.Drawing.Point(91, 42);
            this.textBox_IPAddress.Name = "textBox_IPAddress";
            this.textBox_IPAddress.Size = new System.Drawing.Size(132, 20);
            this.textBox_IPAddress.TabIndex = 15;
            this.textBox_IPAddress.Text = "192.168.0.254";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "IP address";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonConnect.Location = new System.Drawing.Point(17, 127);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(97, 38);
            this.buttonConnect.TabIndex = 13;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.ConnectButton);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonDisconnect.Location = new System.Drawing.Point(126, 127);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(97, 38);
            this.buttonDisconnect.TabIndex = 14;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // textBox_IPPort
            // 
            this.textBox_IPPort.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textBox_IPPort.Location = new System.Drawing.Point(91, 82);
            this.textBox_IPPort.Name = "textBox_IPPort";
            this.textBox_IPPort.Size = new System.Drawing.Size(132, 20);
            this.textBox_IPPort.TabIndex = 16;
            this.textBox_IPPort.Text = "5001";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "IP port";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.BTN_update);
            this.groupBox2.Controls.Add(this.textBoxVelocityInput);
            this.groupBox2.Controls.Add(this.textBoxAccelerationInput);
            this.groupBox2.Controls.Add(this.textBoxVelocity);
            this.groupBox2.Controls.Add(this.textBoxAcceleration);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.textBoxPosition);
            this.groupBox2.Controls.Add(this.TxBoundTweezer);
            this.groupBox2.Controls.Add(this.TxBoundSource);
            this.groupBox2.Controls.Add(this.TxMaxVel);
            this.groupBox2.Controls.Add(this.TxMaxAccel);
            this.groupBox2.Location = new System.Drawing.Point(302, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(352, 356);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Stage parameters";
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(144, 232);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 13);
            this.label12.TabIndex = 32;
            this.label12.Text = "Vel. [mm/s]";
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(29, 232);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(78, 13);
            this.label11.TabIndex = 31;
            this.label11.Text = "Accel. [mm/s2]";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(249, 182);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(62, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Current Vel.";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(137, 182);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(74, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "Current Accel.";
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(36, 182);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 28;
            this.label8.Text = "Current Pos.";
            // 
            // BTN_update
            // 
            this.BTN_update.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.BTN_update.Location = new System.Drawing.Point(232, 242);
            this.BTN_update.Name = "BTN_update";
            this.BTN_update.Size = new System.Drawing.Size(97, 38);
            this.BTN_update.TabIndex = 27;
            this.BTN_update.Text = "Update";
            this.BTN_update.UseVisualStyleBackColor = true;
            this.BTN_update.Click += new System.EventHandler(this.BTN_update_Click);
            // 
            // textBoxVelocityInput
            // 
            this.textBoxVelocityInput.Location = new System.Drawing.Point(124, 249);
            this.textBoxVelocityInput.Name = "textBoxVelocityInput";
            this.textBoxVelocityInput.Size = new System.Drawing.Size(100, 20);
            this.textBoxVelocityInput.TabIndex = 26;
            this.textBoxVelocityInput.Text = "0";
            this.textBoxVelocityInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxAccelerationInput
            // 
            this.textBoxAccelerationInput.Location = new System.Drawing.Point(18, 249);
            this.textBoxAccelerationInput.Name = "textBoxAccelerationInput";
            this.textBoxAccelerationInput.Size = new System.Drawing.Size(100, 20);
            this.textBoxAccelerationInput.TabIndex = 25;
            this.textBoxAccelerationInput.Text = "0";
            this.textBoxAccelerationInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxVelocity
            // 
            this.textBoxVelocity.Location = new System.Drawing.Point(230, 201);
            this.textBoxVelocity.Name = "textBoxVelocity";
            this.textBoxVelocity.ReadOnly = true;
            this.textBoxVelocity.Size = new System.Drawing.Size(100, 20);
            this.textBoxVelocity.TabIndex = 24;
            this.textBoxVelocity.Text = "0";
            this.textBoxVelocity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxAcceleration
            // 
            this.textBoxAcceleration.Location = new System.Drawing.Point(124, 201);
            this.textBoxAcceleration.Name = "textBoxAcceleration";
            this.textBoxAcceleration.ReadOnly = true;
            this.textBoxAcceleration.Size = new System.Drawing.Size(100, 20);
            this.textBoxAcceleration.TabIndex = 23;
            this.textBoxAcceleration.Text = "0";
            this.textBoxAcceleration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(186, 89);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(98, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Max. Pos. Tweezer";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(74, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Max. Pos. Source";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(210, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Max. Vel.";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(86, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Max. Accel";
            // 
            // textBoxPosition
            // 
            this.textBoxPosition.Location = new System.Drawing.Point(18, 201);
            this.textBoxPosition.Name = "textBoxPosition";
            this.textBoxPosition.ReadOnly = true;
            this.textBoxPosition.Size = new System.Drawing.Size(100, 20);
            this.textBoxPosition.TabIndex = 4;
            this.textBoxPosition.Text = "0";
            this.textBoxPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TxBoundTweezer
            // 
            this.TxBoundTweezer.Location = new System.Drawing.Point(185, 105);
            this.TxBoundTweezer.Name = "TxBoundTweezer";
            this.TxBoundTweezer.ReadOnly = true;
            this.TxBoundTweezer.Size = new System.Drawing.Size(100, 20);
            this.TxBoundTweezer.TabIndex = 3;
            this.TxBoundTweezer.Text = "0";
            this.TxBoundTweezer.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TxBoundSource
            // 
            this.TxBoundSource.Location = new System.Drawing.Point(68, 105);
            this.TxBoundSource.Name = "TxBoundSource";
            this.TxBoundSource.ReadOnly = true;
            this.TxBoundSource.Size = new System.Drawing.Size(100, 20);
            this.TxBoundSource.TabIndex = 2;
            this.TxBoundSource.Text = "0";
            this.TxBoundSource.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TxMaxVel
            // 
            this.TxMaxVel.Location = new System.Drawing.Point(185, 50);
            this.TxMaxVel.Name = "TxMaxVel";
            this.TxMaxVel.ReadOnly = true;
            this.TxMaxVel.Size = new System.Drawing.Size(100, 20);
            this.TxMaxVel.TabIndex = 1;
            this.TxMaxVel.Text = "0";
            this.TxMaxVel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TxMaxAccel
            // 
            this.TxMaxAccel.Location = new System.Drawing.Point(68, 50);
            this.TxMaxAccel.Name = "TxMaxAccel";
            this.TxMaxAccel.ReadOnly = true;
            this.TxMaxAccel.Size = new System.Drawing.Size(100, 20);
            this.TxMaxAccel.TabIndex = 0;
            this.TxMaxAccel.Text = "0";
            this.TxMaxAccel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.groupBox3.Controls.Add(this.RBManual);
            this.groupBox3.Controls.Add(this.textBoxTarget);
            this.groupBox3.Controls.Add(this.buttonMoveTo);
            this.groupBox3.Location = new System.Drawing.Point(9, 384);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(258, 110);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Manual positioning";
            // 
            // RBManual
            // 
            this.RBManual.AutoSize = true;
            this.RBManual.Checked = true;
            this.RBManual.Location = new System.Drawing.Point(30, 26);
            this.RBManual.Name = "RBManual";
            this.RBManual.Size = new System.Drawing.Size(14, 13);
            this.RBManual.TabIndex = 34;
            this.RBManual.TabStop = true;
            this.RBManual.UseVisualStyleBackColor = true;
            this.RBManual.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RBManual_MouseDown);
            // 
            // textBoxTarget
            // 
            this.textBoxTarget.Location = new System.Drawing.Point(137, 65);
            this.textBoxTarget.Name = "textBoxTarget";
            this.textBoxTarget.Size = new System.Drawing.Size(100, 20);
            this.textBoxTarget.TabIndex = 29;
            this.textBoxTarget.Text = "0";
            this.textBoxTarget.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // buttonMoveTo
            // 
            this.buttonMoveTo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonMoveTo.Location = new System.Drawing.Point(16, 56);
            this.buttonMoveTo.Name = "buttonMoveTo";
            this.buttonMoveTo.Size = new System.Drawing.Size(97, 38);
            this.buttonMoveTo.TabIndex = 28;
            this.buttonMoveTo.Text = "Move to";
            this.buttonMoveTo.UseVisualStyleBackColor = true;
            this.buttonMoveTo.Click += new System.EventHandler(this.buttonMoveTo_Click);
            // 
            // label_MessageCommunication
            // 
            this.label_MessageCommunication.AutoSize = true;
            this.label_MessageCommunication.Location = new System.Drawing.Point(41, 542);
            this.label_MessageCommunication.Name = "label_MessageCommunication";
            this.label_MessageCommunication.Size = new System.Drawing.Size(13, 13);
            this.label_MessageCommunication.TabIndex = 3;
            this.label_MessageCommunication.Text = "0";
            // 
            // label_GroupStatusDescription
            // 
            this.label_GroupStatusDescription.AutoSize = true;
            this.label_GroupStatusDescription.Location = new System.Drawing.Point(41, 581);
            this.label_GroupStatusDescription.Name = "label_GroupStatusDescription";
            this.label_GroupStatusDescription.Size = new System.Drawing.Size(37, 13);
            this.label_GroupStatusDescription.TabIndex = 4;
            this.label_GroupStatusDescription.Text = "Status";
            // 
            // label_ErrorMessage
            // 
            this.label_ErrorMessage.AutoSize = true;
            this.label_ErrorMessage.Location = new System.Drawing.Point(41, 713);
            this.label_ErrorMessage.Name = "label_ErrorMessage";
            this.label_ErrorMessage.Size = new System.Drawing.Size(0, 13);
            this.label_ErrorMessage.TabIndex = 5;
            // 
            // TrackView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label_ErrorMessage);
            this.Controls.Add(this.label_GroupStatusDescription);
            this.Controls.Add(this.label_MessageCommunication);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TrackView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_IPAddress;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.TextBox textBox_IPPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxLog;
        private System.Windows.Forms.Button buttonHome;
        private System.Windows.Forms.Button buttonKill;
        private System.Windows.Forms.Button buttonInitialize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TextBox_Group;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxPosition;
        private System.Windows.Forms.TextBox TxBoundTweezer;
        private System.Windows.Forms.TextBox TxBoundSource;
        private System.Windows.Forms.TextBox TxMaxVel;
        private System.Windows.Forms.TextBox TxMaxAccel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button BTN_update;
        private System.Windows.Forms.TextBox textBoxVelocityInput;
        private System.Windows.Forms.TextBox textBoxAccelerationInput;
        private System.Windows.Forms.TextBox textBoxVelocity;
        private System.Windows.Forms.TextBox textBoxAcceleration;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton RBManual;
        private System.Windows.Forms.TextBox textBoxTarget;
        private System.Windows.Forms.Button buttonMoveTo;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox textBoxTweezer;
        private System.Windows.Forms.RadioButton RBTrigger;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Button buttonRunTCL;
        private System.Windows.Forms.Label label_MessageCommunication;
        private System.Windows.Forms.Label label_GroupStatusDescription;
        private System.Windows.Forms.Label label_ErrorMessage;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxIterations;
    }
}
