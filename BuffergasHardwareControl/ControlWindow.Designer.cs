namespace BuffergasHardwareControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.FlowVoltageBox = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.flowmeterTextBox1 = new System.Windows.Forms.TextBox();
            this.flowmeterbutton1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.FlowVoltageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(103, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Flow Controller";
            // 
            // FlowVoltageBox
            // 
            this.FlowVoltageBox.Location = new System.Drawing.Point(67, 58);
            this.FlowVoltageBox.Name = "FlowVoltageBox";
            this.FlowVoltageBox.Size = new System.Drawing.Size(120, 20);
            this.FlowVoltageBox.TabIndex = 2;
            this.FlowVoltageBox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.FlowVoltageBox.ValueChanged += new System.EventHandler(this.FlowVoltageBox_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(385, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Flow Meter";
            // 
            // flowmeterTextBox1
            // 
            this.flowmeterTextBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.flowmeterTextBox1.Location = new System.Drawing.Point(367, 58);
            this.flowmeterTextBox1.Name = "flowmeterTextBox1";
            this.flowmeterTextBox1.ReadOnly = true;
            this.flowmeterTextBox1.Size = new System.Drawing.Size(100, 20);
            this.flowmeterTextBox1.TabIndex = 6;
            // 
            // flowmeterbutton1
            // 
            this.flowmeterbutton1.Location = new System.Drawing.Point(481, 56);
            this.flowmeterbutton1.Name = "flowmeterbutton1";
            this.flowmeterbutton1.Size = new System.Drawing.Size(18, 21);
            this.flowmeterbutton1.TabIndex = 7;
            this.flowmeterbutton1.Text = "F";
            this.flowmeterbutton1.UseVisualStyleBackColor = true;
            this.flowmeterbutton1.Click += new System.EventHandler(this.FlowmeterButton_Click);
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 394);
            this.Controls.Add(this.flowmeterbutton1);
            this.Controls.Add(this.flowmeterTextBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FlowVoltageBox);
            this.Controls.Add(this.label1);
            this.Name = "ControlWindow";
            this.Text = "ControlWindow";
            ((System.ComponentModel.ISupportInitialize)(this.FlowVoltageBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown FlowVoltageBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox flowmeterTextBox1;
        private System.Windows.Forms.Button flowmeterbutton1;
    }
}

