
namespace AlFHardwareControl
{
    partial class MSquaredLaserView
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
            this.Conn_status = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.M2_Control_Group = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.updateLineData = new System.Windows.Forms.Button();
            this.lineName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lineFrequency = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.error = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.offset = new AlFHardwareControl.ParamSet();
            this.setpoint = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.RemoveLine = new System.Windows.Forms.Button();
            this.AddLine = new System.Windows.Forms.Button();
            this.LinesSelector = new System.Windows.Forms.ComboBox();
            this.lockPrecision = new AlFHardwareControl.ParamSet();
            this.lockTolerance = new AlFHardwareControl.ParamSet();
            this.lockCheckBox = new System.Windows.Forms.CheckBox();
            this.VelSet = new AlFHardwareControl.ParamSet();
            this.M2_Control_Group.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Conn_status
            // 
            this.Conn_status.BackColor = System.Drawing.Color.Salmon;
            this.Conn_status.Enabled = false;
            this.Conn_status.Location = new System.Drawing.Point(49, 19);
            this.Conn_status.Name = "Conn_status";
            this.Conn_status.Size = new System.Drawing.Size(99, 20);
            this.Conn_status.TabIndex = 2;
            this.Conn_status.Text = "DISCONNECTED";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Status";
            // 
            // M2_Control_Group
            // 
            this.M2_Control_Group.Controls.Add(this.groupBox1);
            this.M2_Control_Group.Controls.Add(this.error);
            this.M2_Control_Group.Controls.Add(this.label4);
            this.M2_Control_Group.Controls.Add(this.offset);
            this.M2_Control_Group.Controls.Add(this.setpoint);
            this.M2_Control_Group.Controls.Add(this.label3);
            this.M2_Control_Group.Controls.Add(this.label2);
            this.M2_Control_Group.Controls.Add(this.RemoveLine);
            this.M2_Control_Group.Controls.Add(this.AddLine);
            this.M2_Control_Group.Controls.Add(this.LinesSelector);
            this.M2_Control_Group.Controls.Add(this.lockPrecision);
            this.M2_Control_Group.Controls.Add(this.lockTolerance);
            this.M2_Control_Group.Controls.Add(this.lockCheckBox);
            this.M2_Control_Group.Controls.Add(this.VelSet);
            this.M2_Control_Group.Controls.Add(this.label1);
            this.M2_Control_Group.Controls.Add(this.Conn_status);
            this.M2_Control_Group.Location = new System.Drawing.Point(5, 4);
            this.M2_Control_Group.Name = "M2_Control_Group";
            this.M2_Control_Group.Size = new System.Drawing.Size(214, 404);
            this.M2_Control_Group.TabIndex = 4;
            this.M2_Control_Group.TabStop = false;
            this.M2_Control_Group.Text = "M2 Laser Control";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.updateLineData);
            this.groupBox1.Controls.Add(this.lineName);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.lineFrequency);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(5, 261);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(203, 101);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Line Parameters";
            // 
            // updateLineData
            // 
            this.updateLineData.Location = new System.Drawing.Point(9, 71);
            this.updateLineData.Name = "updateLineData";
            this.updateLineData.Size = new System.Drawing.Size(188, 23);
            this.updateLineData.TabIndex = 4;
            this.updateLineData.Text = "Update";
            this.updateLineData.UseVisualStyleBackColor = true;
            this.updateLineData.Click += new System.EventHandler(this.updateLineData_Click);
            // 
            // lineName
            // 
            this.lineName.Location = new System.Drawing.Point(98, 18);
            this.lineName.Name = "lineName";
            this.lineName.Size = new System.Drawing.Size(99, 20);
            this.lineName.TabIndex = 3;
            this.lineName.Text = "Default";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Name";
            // 
            // lineFrequency
            // 
            this.lineFrequency.Location = new System.Drawing.Point(98, 44);
            this.lineFrequency.Name = "lineFrequency";
            this.lineFrequency.Size = new System.Drawing.Size(99, 20);
            this.lineFrequency.TabIndex = 1;
            this.lineFrequency.Text = "327";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Frequency [THz]";
            // 
            // error
            // 
            this.error.Enabled = false;
            this.error.Location = new System.Drawing.Point(72, 72);
            this.error.Name = "error";
            this.error.Size = new System.Drawing.Size(133, 20);
            this.error.TabIndex = 17;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Error [MHz]";
            // 
            // offset
            // 
            this.offset.Label = "Offset";
            this.offset.Location = new System.Drawing.Point(5, 100);
            this.offset.Name = "offset";
            this.offset.Size = new System.Drawing.Size(203, 27);
            this.offset.TabIndex = 15;
            this.offset.OnSetClick += new System.EventHandler(this.offset_OnSetClick);
            // 
            // setpoint
            // 
            this.setpoint.Enabled = false;
            this.setpoint.Location = new System.Drawing.Point(72, 46);
            this.setpoint.Name = "setpoint";
            this.setpoint.Size = new System.Drawing.Size(133, 20);
            this.setpoint.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "SP [THz]";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 237);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Line";
            // 
            // RemoveLine
            // 
            this.RemoveLine.Location = new System.Drawing.Point(188, 232);
            this.RemoveLine.Name = "RemoveLine";
            this.RemoveLine.Size = new System.Drawing.Size(17, 23);
            this.RemoveLine.TabIndex = 11;
            this.RemoveLine.Text = "-";
            this.RemoveLine.UseVisualStyleBackColor = true;
            this.RemoveLine.Click += new System.EventHandler(this.RemoveLine_Click);
            // 
            // AddLine
            // 
            this.AddLine.Location = new System.Drawing.Point(167, 232);
            this.AddLine.Name = "AddLine";
            this.AddLine.Size = new System.Drawing.Size(17, 23);
            this.AddLine.TabIndex = 10;
            this.AddLine.Text = "+";
            this.AddLine.UseVisualStyleBackColor = true;
            this.AddLine.Click += new System.EventHandler(this.AddLine_Click);
            // 
            // LinesSelector
            // 
            this.LinesSelector.FormattingEnabled = true;
            this.LinesSelector.Location = new System.Drawing.Point(44, 234);
            this.LinesSelector.Name = "LinesSelector";
            this.LinesSelector.Size = new System.Drawing.Size(117, 21);
            this.LinesSelector.TabIndex = 9;
            this.LinesSelector.SelectedIndexChanged += new System.EventHandler(this.LinesSelector_SelectedIndexChanged);
            // 
            // lockPrecision
            // 
            this.lockPrecision.Label = "P [nm]";
            this.lockPrecision.Location = new System.Drawing.Point(5, 199);
            this.lockPrecision.Name = "lockPrecision";
            this.lockPrecision.Size = new System.Drawing.Size(203, 27);
            this.lockPrecision.TabIndex = 8;
            this.lockPrecision.OnSetClick += new System.EventHandler(this.lockPrecision_OnSetClick);
            // 
            // lockTolerance
            // 
            this.lockTolerance.Label = "Tol. [nm]";
            this.lockTolerance.Location = new System.Drawing.Point(5, 166);
            this.lockTolerance.Name = "lockTolerance";
            this.lockTolerance.Size = new System.Drawing.Size(203, 27);
            this.lockTolerance.TabIndex = 7;
            this.lockTolerance.OnSetClick += new System.EventHandler(this.lockTolerance_OnSetClick);
            // 
            // lockCheckBox
            // 
            this.lockCheckBox.AutoSize = true;
            this.lockCheckBox.Location = new System.Drawing.Point(158, 21);
            this.lockCheckBox.Name = "lockCheckBox";
            this.lockCheckBox.Size = new System.Drawing.Size(50, 17);
            this.lockCheckBox.TabIndex = 6;
            this.lockCheckBox.Text = "Lock";
            this.lockCheckBox.UseVisualStyleBackColor = true;
            this.lockCheckBox.CheckedChanged += new System.EventHandler(this.lockCheckBox_CheckedChanged);
            // 
            // VelSet
            // 
            this.VelSet.Label = "Velocity";
            this.VelSet.Location = new System.Drawing.Point(5, 133);
            this.VelSet.Name = "VelSet";
            this.VelSet.Size = new System.Drawing.Size(203, 27);
            this.VelSet.TabIndex = 5;
            this.VelSet.OnSetClick += new System.EventHandler(this.VelSet_OnSetClick);
            // 
            // MSquaredLaserView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.M2_Control_Group);
            this.Name = "MSquaredLaserView";
            this.Size = new System.Drawing.Size(222, 411);
            this.Load += new System.EventHandler(this.MSquaredLaserView_Load);
            this.M2_Control_Group.ResumeLayout(false);
            this.M2_Control_Group.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox Conn_status;
        private System.Windows.Forms.Label label1;
        private ParamSet lockPrecision;
        private ParamSet lockTolerance;
        private System.Windows.Forms.CheckBox lockCheckBox;
        private ParamSet VelSet;
        private System.Windows.Forms.TextBox setpoint;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button RemoveLine;
        private System.Windows.Forms.Button AddLine;
        private System.Windows.Forms.ComboBox LinesSelector;
        public System.Windows.Forms.GroupBox M2_Control_Group;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox error;
        private System.Windows.Forms.Label label4;
        private ParamSet offset;
        private System.Windows.Forms.Button updateLineData;
        private System.Windows.Forms.TextBox lineName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox lineFrequency;
        private System.Windows.Forms.Label label5;
    }
}
