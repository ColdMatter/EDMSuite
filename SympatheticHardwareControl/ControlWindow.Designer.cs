namespace SympatheticHardwareControl
{
    partial class ControlWindow
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
            this.MainTabs = new System.Windows.Forms.TabControl();
            this.pressureTab = new System.Windows.Forms.TabPage();
            this.pressureUnit2 = new System.Windows.Forms.Label();
            this.pressureUnit1 = new System.Windows.Forms.Label();
            this.pressureLabel2 = new System.Windows.Forms.Label();
            this.pressureIndicator2 = new System.Windows.Forms.TextBox();
            this.pressureLabel1 = new System.Windows.Forms.Label();
            this.pressureIndicator1 = new System.Windows.Forms.TextBox();
            this.fieldTab = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.allBurstButton = new System.Windows.Forms.Button();
            this.allOnButton = new System.Windows.Forms.Button();
            this.allOffButton = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.vnButtonBurst = new System.Windows.Forms.RadioButton();
            this.vnButtonOn = new System.Windows.Forms.RadioButton();
            this.vnButtonOff = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.vpButtonBurst = new System.Windows.Forms.RadioButton();
            this.vpButtonOn = new System.Windows.Forms.RadioButton();
            this.vpButtonOff = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.hnButtonBurst = new System.Windows.Forms.RadioButton();
            this.hnButtonOn = new System.Windows.Forms.RadioButton();
            this.hnButtonOff = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.hpButtonBurst = new System.Windows.Forms.RadioButton();
            this.hpButtonOn = new System.Windows.Forms.RadioButton();
            this.hpButtonOff = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.MainTabs.SuspendLayout();
            this.pressureTab.SuspendLayout();
            this.fieldTab.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabs
            // 
            this.MainTabs.Controls.Add(this.pressureTab);
            this.MainTabs.Controls.Add(this.fieldTab);
            this.MainTabs.Location = new System.Drawing.Point(1, 2);
            this.MainTabs.Name = "MainTabs";
            this.MainTabs.SelectedIndex = 0;
            this.MainTabs.Size = new System.Drawing.Size(312, 325);
            this.MainTabs.TabIndex = 0;
            // 
            // pressureTab
            // 
            this.pressureTab.Controls.Add(this.pressureUnit2);
            this.pressureTab.Controls.Add(this.pressureUnit1);
            this.pressureTab.Controls.Add(this.pressureLabel2);
            this.pressureTab.Controls.Add(this.pressureIndicator2);
            this.pressureTab.Controls.Add(this.pressureLabel1);
            this.pressureTab.Controls.Add(this.pressureIndicator1);
            this.pressureTab.Location = new System.Drawing.Point(4, 22);
            this.pressureTab.Name = "pressureTab";
            this.pressureTab.Padding = new System.Windows.Forms.Padding(3);
            this.pressureTab.Size = new System.Drawing.Size(304, 286);
            this.pressureTab.TabIndex = 0;
            this.pressureTab.Text = "Pressures";
            this.pressureTab.UseVisualStyleBackColor = true;
            // 
            // pressureUnit2
            // 
            this.pressureUnit2.AutoSize = true;
            this.pressureUnit2.Location = new System.Drawing.Point(119, 105);
            this.pressureUnit2.Name = "pressureUnit2";
            this.pressureUnit2.Size = new System.Drawing.Size(30, 13);
            this.pressureUnit2.TabIndex = 5;
            this.pressureUnit2.Text = "mbar";
            // 
            // pressureUnit1
            // 
            this.pressureUnit1.AutoSize = true;
            this.pressureUnit1.Location = new System.Drawing.Point(119, 50);
            this.pressureUnit1.Name = "pressureUnit1";
            this.pressureUnit1.Size = new System.Drawing.Size(30, 13);
            this.pressureUnit1.TabIndex = 4;
            this.pressureUnit1.Text = "mbar";
            // 
            // pressureLabel2
            // 
            this.pressureLabel2.AutoSize = true;
            this.pressureLabel2.Location = new System.Drawing.Point(15, 81);
            this.pressureLabel2.Name = "pressureLabel2";
            this.pressureLabel2.Size = new System.Drawing.Size(99, 13);
            this.pressureLabel2.TabIndex = 3;
            this.pressureLabel2.Text = "Li Source, Gauge 2";
            // 
            // pressureIndicator2
            // 
            this.pressureIndicator2.BackColor = System.Drawing.Color.Black;
            this.pressureIndicator2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pressureIndicator2.ForeColor = System.Drawing.Color.YellowGreen;
            this.pressureIndicator2.Location = new System.Drawing.Point(18, 97);
            this.pressureIndicator2.Name = "pressureIndicator2";
            this.pressureIndicator2.Size = new System.Drawing.Size(100, 26);
            this.pressureIndicator2.TabIndex = 2;
            // 
            // pressureLabel1
            // 
            this.pressureLabel1.AutoSize = true;
            this.pressureLabel1.Location = new System.Drawing.Point(15, 26);
            this.pressureLabel1.Name = "pressureLabel1";
            this.pressureLabel1.Size = new System.Drawing.Size(99, 13);
            this.pressureLabel1.TabIndex = 1;
            this.pressureLabel1.Text = "Li Source, Gauge 1";
            this.pressureLabel1.Click += new System.EventHandler(this.pressureLabel1_Click);
            // 
            // pressureIndicator1
            // 
            this.pressureIndicator1.BackColor = System.Drawing.Color.Black;
            this.pressureIndicator1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pressureIndicator1.ForeColor = System.Drawing.Color.YellowGreen;
            this.pressureIndicator1.Location = new System.Drawing.Point(18, 42);
            this.pressureIndicator1.Name = "pressureIndicator1";
            this.pressureIndicator1.ReadOnly = true;
            this.pressureIndicator1.Size = new System.Drawing.Size(100, 26);
            this.pressureIndicator1.TabIndex = 0;
            // 
            // fieldTab
            // 
            this.fieldTab.Controls.Add(this.groupBox1);
            this.fieldTab.Location = new System.Drawing.Point(4, 22);
            this.fieldTab.Name = "fieldTab";
            this.fieldTab.Padding = new System.Windows.Forms.Padding(3);
            this.fieldTab.Size = new System.Drawing.Size(304, 299);
            this.fieldTab.TabIndex = 1;
            this.fieldTab.Text = "Electric Fields";
            this.fieldTab.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.allBurstButton);
            this.groupBox1.Controls.Add(this.allOnButton);
            this.groupBox1.Controls.Add(this.allOffButton);
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(7, 18);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(196, 275);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "HV Switches";
            // 
            // allBurstButton
            // 
            this.allBurstButton.Location = new System.Drawing.Point(120, 214);
            this.allBurstButton.Name = "allBurstButton";
            this.allBurstButton.Size = new System.Drawing.Size(55, 23);
            this.allBurstButton.TabIndex = 6;
            this.allBurstButton.Text = "All Burst";
            this.allBurstButton.UseVisualStyleBackColor = true;
            // 
            // allOnButton
            // 
            this.allOnButton.Location = new System.Drawing.Point(61, 214);
            this.allOnButton.Name = "allOnButton";
            this.allOnButton.Size = new System.Drawing.Size(55, 23);
            this.allOnButton.TabIndex = 5;
            this.allOnButton.Text = "All On";
            this.allOnButton.UseVisualStyleBackColor = true;
            // 
            // allOffButton
            // 
            this.allOffButton.Location = new System.Drawing.Point(2, 214);
            this.allOffButton.Name = "allOffButton";
            this.allOffButton.Size = new System.Drawing.Size(55, 23);
            this.allOffButton.TabIndex = 4;
            this.allOffButton.Text = "All Off";
            this.allOffButton.UseVisualStyleBackColor = true;
            this.allOffButton.Click += new System.EventHandler(this.allOffButton_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.vnButtonBurst);
            this.groupBox5.Controls.Add(this.vnButtonOn);
            this.groupBox5.Controls.Add(this.vnButtonOff);
            this.groupBox5.Location = new System.Drawing.Point(13, 164);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(150, 41);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Vertical Negative";
            // 
            // vnButtonBurst
            // 
            this.vnButtonBurst.AutoSize = true;
            this.vnButtonBurst.Location = new System.Drawing.Point(95, 19);
            this.vnButtonBurst.Name = "vnButtonBurst";
            this.vnButtonBurst.Size = new System.Drawing.Size(49, 17);
            this.vnButtonBurst.TabIndex = 2;
            this.vnButtonBurst.Text = "Burst";
            this.vnButtonBurst.UseVisualStyleBackColor = true;
            this.vnButtonBurst.Click += new System.EventHandler(this.vnButtonBurst_Clicked);
            // 
            // vnButtonOn
            // 
            this.vnButtonOn.AutoSize = true;
            this.vnButtonOn.Location = new System.Drawing.Point(51, 19);
            this.vnButtonOn.Name = "vnButtonOn";
            this.vnButtonOn.Size = new System.Drawing.Size(39, 17);
            this.vnButtonOn.TabIndex = 1;
            this.vnButtonOn.Text = "On";
            this.vnButtonOn.UseVisualStyleBackColor = true;
            this.vnButtonOn.Click += new System.EventHandler(this.vnButtonOn_Clicked);
            // 
            // vnButtonOff
            // 
            this.vnButtonOff.AutoSize = true;
            this.vnButtonOff.Checked = true;
            this.vnButtonOff.Location = new System.Drawing.Point(6, 19);
            this.vnButtonOff.Name = "vnButtonOff";
            this.vnButtonOff.Size = new System.Drawing.Size(39, 17);
            this.vnButtonOff.TabIndex = 0;
            this.vnButtonOff.TabStop = true;
            this.vnButtonOff.Text = "Off";
            this.vnButtonOff.UseVisualStyleBackColor = true;
            this.vnButtonOff.Click += new System.EventHandler(this.vnButtonOff_Clicked);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.vpButtonBurst);
            this.groupBox4.Controls.Add(this.vpButtonOn);
            this.groupBox4.Controls.Add(this.vpButtonOff);
            this.groupBox4.Location = new System.Drawing.Point(13, 117);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(150, 41);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Vertical Positive";
            // 
            // vpButtonBurst
            // 
            this.vpButtonBurst.AutoSize = true;
            this.vpButtonBurst.Location = new System.Drawing.Point(95, 19);
            this.vpButtonBurst.Name = "vpButtonBurst";
            this.vpButtonBurst.Size = new System.Drawing.Size(49, 17);
            this.vpButtonBurst.TabIndex = 2;
            this.vpButtonBurst.Text = "Burst";
            this.vpButtonBurst.UseVisualStyleBackColor = true;
            this.vpButtonBurst.Click += new System.EventHandler(this.vpButtonBurst_Clicked);
            // 
            // vpButtonOn
            // 
            this.vpButtonOn.AutoSize = true;
            this.vpButtonOn.Location = new System.Drawing.Point(51, 19);
            this.vpButtonOn.Name = "vpButtonOn";
            this.vpButtonOn.Size = new System.Drawing.Size(39, 17);
            this.vpButtonOn.TabIndex = 1;
            this.vpButtonOn.Text = "On";
            this.vpButtonOn.UseVisualStyleBackColor = true;
            this.vpButtonOn.Click += new System.EventHandler(this.vpButtonOn_Clicked);
            // 
            // vpButtonOff
            // 
            this.vpButtonOff.AutoSize = true;
            this.vpButtonOff.Checked = true;
            this.vpButtonOff.Location = new System.Drawing.Point(6, 19);
            this.vpButtonOff.Name = "vpButtonOff";
            this.vpButtonOff.Size = new System.Drawing.Size(39, 17);
            this.vpButtonOff.TabIndex = 0;
            this.vpButtonOff.TabStop = true;
            this.vpButtonOff.Text = "Off";
            this.vpButtonOff.UseVisualStyleBackColor = true;
            this.vpButtonOff.Click += new System.EventHandler(this.vpButtonOff_Clicked);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.hnButtonBurst);
            this.groupBox3.Controls.Add(this.hnButtonOn);
            this.groupBox3.Controls.Add(this.hnButtonOff);
            this.groupBox3.Location = new System.Drawing.Point(13, 70);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(150, 41);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Horizontal Negative";
            // 
            // hnButtonBurst
            // 
            this.hnButtonBurst.AutoSize = true;
            this.hnButtonBurst.Location = new System.Drawing.Point(95, 19);
            this.hnButtonBurst.Name = "hnButtonBurst";
            this.hnButtonBurst.Size = new System.Drawing.Size(49, 17);
            this.hnButtonBurst.TabIndex = 2;
            this.hnButtonBurst.Text = "Burst";
            this.hnButtonBurst.UseVisualStyleBackColor = true;
            this.hnButtonBurst.Click += new System.EventHandler(this.hnButtonBurst_Clicked);
            // 
            // hnButtonOn
            // 
            this.hnButtonOn.AutoSize = true;
            this.hnButtonOn.Location = new System.Drawing.Point(51, 19);
            this.hnButtonOn.Name = "hnButtonOn";
            this.hnButtonOn.Size = new System.Drawing.Size(39, 17);
            this.hnButtonOn.TabIndex = 1;
            this.hnButtonOn.Text = "On";
            this.hnButtonOn.UseVisualStyleBackColor = true;
            this.hnButtonOn.Click += new System.EventHandler(this.hnButtonOn_Clicked);
            // 
            // hnButtonOff
            // 
            this.hnButtonOff.AutoSize = true;
            this.hnButtonOff.Checked = true;
            this.hnButtonOff.Location = new System.Drawing.Point(6, 19);
            this.hnButtonOff.Name = "hnButtonOff";
            this.hnButtonOff.Size = new System.Drawing.Size(39, 17);
            this.hnButtonOff.TabIndex = 0;
            this.hnButtonOff.TabStop = true;
            this.hnButtonOff.Text = "Off";
            this.hnButtonOff.UseVisualStyleBackColor = true;
            this.hnButtonOff.Click += new System.EventHandler(this.hnButtonOff_Clicked);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.hpButtonBurst);
            this.groupBox2.Controls.Add(this.hpButtonOn);
            this.groupBox2.Controls.Add(this.hpButtonOff);
            this.groupBox2.Location = new System.Drawing.Point(13, 23);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(150, 41);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Horizontal Positive";
            // 
            // hpButtonBurst
            // 
            this.hpButtonBurst.AutoSize = true;
            this.hpButtonBurst.Location = new System.Drawing.Point(95, 19);
            this.hpButtonBurst.Name = "hpButtonBurst";
            this.hpButtonBurst.Size = new System.Drawing.Size(49, 17);
            this.hpButtonBurst.TabIndex = 2;
            this.hpButtonBurst.Text = "Burst";
            this.hpButtonBurst.UseVisualStyleBackColor = true;
            this.hpButtonBurst.Click += new System.EventHandler(this.hpButtonBurst_Clicked);
            // 
            // hpButtonOn
            // 
            this.hpButtonOn.AutoSize = true;
            this.hpButtonOn.Location = new System.Drawing.Point(51, 19);
            this.hpButtonOn.Name = "hpButtonOn";
            this.hpButtonOn.Size = new System.Drawing.Size(39, 17);
            this.hpButtonOn.TabIndex = 1;
            this.hpButtonOn.Text = "On";
            this.hpButtonOn.UseVisualStyleBackColor = true;
            this.hpButtonOn.Click += new System.EventHandler(this.hpButtonOn_Clicked);
            this.hpButtonOn.CheckedChanged += new System.EventHandler(this.hpButtonOn_CheckedChanged);
            // 
            // hpButtonOff
            // 
            this.hpButtonOff.AutoSize = true;
            this.hpButtonOff.Checked = true;
            this.hpButtonOff.Location = new System.Drawing.Point(6, 19);
            this.hpButtonOff.Name = "hpButtonOff";
            this.hpButtonOff.Size = new System.Drawing.Size(39, 17);
            this.hpButtonOff.TabIndex = 0;
            this.hpButtonOff.TabStop = true;
            this.hpButtonOff.Text = "Off";
            this.hpButtonOff.UseVisualStyleBackColor = true;
            this.hpButtonOff.Click += new System.EventHandler(this.hpButtonOff_Clicked);
            this.hpButtonOff.CheckedChanged += new System.EventHandler(this.hpButtonOff_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 243);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 26);
            this.label1.TabIndex = 5;
            this.label1.Text = "In DC operation, disconnect capacitors\r\nand turn current limiters to zero";
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 326);
            this.Controls.Add(this.MainTabs);
            this.Name = "ControlWindow";
            this.Text = "Sympathetic Hardware Controller";
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            this.MainTabs.ResumeLayout(false);
            this.pressureTab.ResumeLayout(false);
            this.pressureTab.PerformLayout();
            this.fieldTab.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabs;
        private System.Windows.Forms.TabPage pressureTab;
        private System.Windows.Forms.TabPage fieldTab;
        private System.Windows.Forms.Label pressureLabel1;
        private System.Windows.Forms.Label pressureLabel2;
        public System.Windows.Forms.TextBox pressureIndicator1;
        public System.Windows.Forms.TextBox pressureIndicator2;
        private System.Windows.Forms.Label pressureUnit2;
        private System.Windows.Forms.Label pressureUnit1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton hpButtonBurst;
        private System.Windows.Forms.RadioButton hpButtonOn;
        private System.Windows.Forms.RadioButton hpButtonOff;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton vnButtonBurst;
        private System.Windows.Forms.RadioButton vnButtonOn;
        private System.Windows.Forms.RadioButton vnButtonOff;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton vpButtonBurst;
        private System.Windows.Forms.RadioButton vpButtonOn;
        private System.Windows.Forms.RadioButton vpButtonOff;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton hnButtonBurst;
        private System.Windows.Forms.RadioButton hnButtonOn;
        private System.Windows.Forms.RadioButton hnButtonOff;
        private System.Windows.Forms.Button allBurstButton;
        private System.Windows.Forms.Button allOnButton;
        private System.Windows.Forms.Button allOffButton;
        private System.Windows.Forms.Label label1;
    }
}

