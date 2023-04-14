
namespace DigitalTransferCavityLock
{
    partial class CavityControl
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Gain = new System.Windows.Forms.TextBox();
            this.EnableData = new System.Windows.Forms.CheckBox();
            this.RefVoltageFeedback = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.refLockLocV = new System.Windows.Forms.TextBox();
            this.LockReference = new System.Windows.Forms.CheckBox();
            this.refLocV = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.refLocMS = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SlaveLasersTabs = new System.Windows.Forms.TabControl();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.Gain);
            this.groupBox2.Controls.Add(this.EnableData);
            this.groupBox2.Controls.Add(this.RefVoltageFeedback);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.refLockLocV);
            this.groupBox2.Controls.Add(this.LockReference);
            this.groupBox2.Controls.Add(this.refLocV);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.refLocMS);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(275, 158);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Reference Control";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Gain";
            // 
            // Gain
            // 
            this.Gain.Location = new System.Drawing.Point(169, 61);
            this.Gain.Name = "Gain";
            this.Gain.Size = new System.Drawing.Size(100, 20);
            this.Gain.TabIndex = 0;
            this.Gain.Text = "0";
            this.Gain.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.Gain.Leave += new System.EventHandler(this.Gain_Leave);
            // 
            // EnableData
            // 
            this.EnableData.AutoSize = true;
            this.EnableData.Location = new System.Drawing.Point(169, 135);
            this.EnableData.Name = "EnableData";
            this.EnableData.Size = new System.Drawing.Size(86, 17);
            this.EnableData.TabIndex = 4;
            this.EnableData.Text = "Enable Input";
            this.EnableData.UseVisualStyleBackColor = true;
            this.EnableData.CheckedChanged += new System.EventHandler(this.EnableData_CheckedChanged);
            // 
            // RefVoltageFeedback
            // 
            this.RefVoltageFeedback.Location = new System.Drawing.Point(169, 105);
            this.RefVoltageFeedback.Name = "RefVoltageFeedback";
            this.RefVoltageFeedback.Size = new System.Drawing.Size(100, 20);
            this.RefVoltageFeedback.TabIndex = 2;
            this.RefVoltageFeedback.Text = "0";
            this.RefVoltageFeedback.Leave += new System.EventHandler(this.RefVoltageFeedback_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 86);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(97, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Lock Location [ms]";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 108);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(110, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Voltage Feedback [V]";
            // 
            // refLockLocV
            // 
            this.refLockLocV.Location = new System.Drawing.Point(169, 83);
            this.refLockLocV.Name = "refLockLocV";
            this.refLockLocV.Size = new System.Drawing.Size(100, 20);
            this.refLockLocV.TabIndex = 1;
            this.refLockLocV.Text = "0";
            this.refLockLocV.Leave += new System.EventHandler(this.refLockLocV_Leave);
            // 
            // LockReference
            // 
            this.LockReference.AutoSize = true;
            this.LockReference.Enabled = false;
            this.LockReference.Location = new System.Drawing.Point(10, 135);
            this.LockReference.Name = "LockReference";
            this.LockReference.Size = new System.Drawing.Size(50, 17);
            this.LockReference.TabIndex = 3;
            this.LockReference.Text = "Lock";
            this.LockReference.UseVisualStyleBackColor = true;
            this.LockReference.CheckedChanged += new System.EventHandler(this.LockReference_CheckedChanged);
            // 
            // refLocV
            // 
            this.refLocV.Enabled = false;
            this.refLocV.Location = new System.Drawing.Point(169, 39);
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
            this.refLocMS.Location = new System.Drawing.Point(169, 17);
            this.refLocMS.Name = "refLocMS";
            this.refLocMS.Size = new System.Drawing.Size(100, 20);
            this.refLocMS.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Peak location [ms]";
            // 
            // SlaveLasersTabs
            // 
            this.SlaveLasersTabs.Location = new System.Drawing.Point(3, 167);
            this.SlaveLasersTabs.Name = "SlaveLasersTabs";
            this.SlaveLasersTabs.SelectedIndex = 0;
            this.SlaveLasersTabs.Size = new System.Drawing.Size(275, 386);
            this.SlaveLasersTabs.TabIndex = 5;
            // 
            // CavityControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SlaveLasersTabs);
            this.Controls.Add(this.groupBox2);
            this.Name = "CavityControl";
            this.Size = new System.Drawing.Size(280, 558);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.CheckBox EnableData;
        public System.Windows.Forms.TextBox RefVoltageFeedback;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        public System.Windows.Forms.TextBox refLockLocV;
        public System.Windows.Forms.CheckBox LockReference;
        public System.Windows.Forms.TextBox refLocV;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox refLocMS;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl SlaveLasersTabs;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox Gain;
    }
}
