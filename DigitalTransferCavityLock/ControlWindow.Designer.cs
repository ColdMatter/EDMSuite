
namespace DigitalTransferCavityLock
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LockRate = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.RefVoltageFeedback = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.refLockLocV = new System.Windows.Forms.TextBox();
            this.LockReference = new System.Windows.Forms.CheckBox();
            this.refLocV = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.refLocMS = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.SlaveVoltageFeedback = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.slaveGain = new System.Windows.Forms.TextBox();
            this.LockSlave = new System.Windows.Forms.CheckBox();
            this.slaveLockLocV = new System.Windows.Forms.TextBox();
            this.slaveLocV = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.slaveLocMS = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LockRate);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(273, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ramp Control";
            // 
            // LockRate
            // 
            this.LockRate.Enabled = false;
            this.LockRate.Location = new System.Drawing.Point(166, 15);
            this.LockRate.Name = "LockRate";
            this.LockRate.Size = new System.Drawing.Size(100, 20);
            this.LockRate.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Lock rate [Hz]";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.RefVoltageFeedback);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.refLockLocV);
            this.groupBox2.Controls.Add(this.LockReference);
            this.groupBox2.Controls.Add(this.refLocV);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.refLocMS);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(13, 119);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(273, 132);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Reference Control";
            // 
            // RefVoltageFeedback
            // 
            this.RefVoltageFeedback.Location = new System.Drawing.Point(167, 87);
            this.RefVoltageFeedback.Name = "RefVoltageFeedback";
            this.RefVoltageFeedback.Size = new System.Drawing.Size(100, 20);
            this.RefVoltageFeedback.TabIndex = 15;
            this.RefVoltageFeedback.Text = "0";
            this.RefVoltageFeedback.TextChanged += new System.EventHandler(this.RefVoltageFeedback_TextChanged);
            this.RefVoltageFeedback.Leave += new System.EventHandler(this.RefVoltageFeedback_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 64);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Lock Location [V]";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 88);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(110, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Voltage Feedback [V]";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // refLockLocV
            // 
            this.refLockLocV.Location = new System.Drawing.Point(167, 64);
            this.refLockLocV.Name = "refLockLocV";
            this.refLockLocV.Size = new System.Drawing.Size(100, 20);
            this.refLockLocV.TabIndex = 15;
            this.refLockLocV.Text = "0";
            // 
            // LockReference
            // 
            this.LockReference.AutoSize = true;
            this.LockReference.Location = new System.Drawing.Point(6, 109);
            this.LockReference.Name = "LockReference";
            this.LockReference.Size = new System.Drawing.Size(50, 17);
            this.LockReference.TabIndex = 12;
            this.LockReference.Text = "Lock";
            this.LockReference.UseVisualStyleBackColor = true;
            this.LockReference.CheckedChanged += new System.EventHandler(this.LockReference_CheckedChanged);
            // 
            // refLocV
            // 
            this.refLocV.Enabled = false;
            this.refLocV.Location = new System.Drawing.Point(167, 41);
            this.refLocV.Name = "refLocV";
            this.refLocV.Size = new System.Drawing.Size(100, 20);
            this.refLocV.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Peak location [V]";
            // 
            // refLocMS
            // 
            this.refLocMS.Enabled = false;
            this.refLocMS.Location = new System.Drawing.Point(167, 19);
            this.refLocMS.Name = "refLocMS";
            this.refLocMS.Size = new System.Drawing.Size(100, 20);
            this.refLocMS.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Peak location [ms]";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.SlaveVoltageFeedback);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.slaveGain);
            this.groupBox3.Controls.Add(this.LockSlave);
            this.groupBox3.Controls.Add(this.slaveLockLocV);
            this.groupBox3.Controls.Add(this.slaveLocV);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.slaveLocMS);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(12, 257);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(273, 155);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Slave Control";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 113);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(110, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "Voltage Feedback [V]";
            // 
            // SlaveVoltageFeedback
            // 
            this.SlaveVoltageFeedback.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SlaveVoltageFeedback.Location = new System.Drawing.Point(166, 110);
            this.SlaveVoltageFeedback.Name = "SlaveVoltageFeedback";
            this.SlaveVoltageFeedback.Size = new System.Drawing.Size(100, 20);
            this.SlaveVoltageFeedback.TabIndex = 15;
            this.SlaveVoltageFeedback.Text = "0";
            this.SlaveVoltageFeedback.TextChanged += new System.EventHandler(this.SlaveVoltageFeedback_TextChanged);
            this.SlaveVoltageFeedback.Leave += new System.EventHandler(this.SlaveVoltageFeedback_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 89);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Lock Location [V]";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Gain";
            // 
            // slaveGain
            // 
            this.slaveGain.Location = new System.Drawing.Point(166, 63);
            this.slaveGain.Name = "slaveGain";
            this.slaveGain.Size = new System.Drawing.Size(100, 20);
            this.slaveGain.TabIndex = 12;
            this.slaveGain.Text = "0";
            // 
            // LockSlave
            // 
            this.LockSlave.AutoSize = true;
            this.LockSlave.Enabled = false;
            this.LockSlave.Location = new System.Drawing.Point(6, 132);
            this.LockSlave.Name = "LockSlave";
            this.LockSlave.Size = new System.Drawing.Size(50, 17);
            this.LockSlave.TabIndex = 11;
            this.LockSlave.Text = "Lock";
            this.LockSlave.UseVisualStyleBackColor = true;
            this.LockSlave.CheckedChanged += new System.EventHandler(this.LockSlave_CheckedChanged);
            // 
            // slaveLockLocV
            // 
            this.slaveLockLocV.Location = new System.Drawing.Point(166, 86);
            this.slaveLockLocV.Name = "slaveLockLocV";
            this.slaveLockLocV.Size = new System.Drawing.Size(100, 20);
            this.slaveLockLocV.TabIndex = 10;
            this.slaveLockLocV.Text = "0";
            // 
            // slaveLocV
            // 
            this.slaveLocV.Enabled = false;
            this.slaveLocV.Location = new System.Drawing.Point(166, 41);
            this.slaveLocV.Name = "slaveLocV";
            this.slaveLocV.Size = new System.Drawing.Size(100, 20);
            this.slaveLocV.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Peak location [V]";
            // 
            // slaveLocMS
            // 
            this.slaveLocMS.Enabled = false;
            this.slaveLocMS.Location = new System.Drawing.Point(166, 19);
            this.slaveLocMS.Name = "slaveLocMS";
            this.slaveLocMS.Size = new System.Drawing.Size(100, 20);
            this.slaveLocMS.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Peak location [ms]";
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(298, 424);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ControlWindow";
            this.Text = "Digital Transfer Cavity Lock";
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.TextBox slaveLocMS;
        public System.Windows.Forms.CheckBox LockReference;
        public System.Windows.Forms.TextBox refLocV;
        public System.Windows.Forms.TextBox refLocMS;
        public System.Windows.Forms.CheckBox LockSlave;
        public System.Windows.Forms.TextBox slaveLockLocV;
        public System.Windows.Forms.TextBox slaveLocV;
        public System.Windows.Forms.TextBox slaveGain;
        public System.Windows.Forms.TextBox refLockLocV;
        public System.Windows.Forms.TextBox LockRate;
        private System.Windows.Forms.Label label9;
        public System.Windows.Forms.TextBox RefVoltageFeedback;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.TextBox SlaveVoltageFeedback;
    }
}

