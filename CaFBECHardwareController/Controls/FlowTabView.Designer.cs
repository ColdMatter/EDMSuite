using NationalInstruments.UI;

namespace CaFBECHardwareController.Controls
{
    partial class FlowTabView
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
            this.scatterPlot2 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.readButton = new System.Windows.Forms.Button();
            this.chkAO1Enable = new System.Windows.Forms.CheckBox();
            this.chkAO0Enable = new System.Windows.Forms.CheckBox();
            this.lblAO1 = new System.Windows.Forms.Label();
            this.lblAO0 = new System.Windows.Forms.Label();
            this.numAO1 = new System.Windows.Forms.NumericUpDown();
            this.numAO0 = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblheflow = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblsf6flow = new System.Windows.Forms.Label();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAO1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAO0)).BeginInit();
            this.SuspendLayout();
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.XAxis = this.xAxis2;
            this.scatterPlot2.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.Caption = "Time (ms)";
            // 
            // yAxis2
            // 
            this.yAxis2.Caption = "Voltage (V)";
            this.yAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis2.Range = new NationalInstruments.UI.Range(0D, 1D);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.readButton);
            this.groupBox4.Controls.Add(this.chkAO1Enable);
            this.groupBox4.Controls.Add(this.chkAO0Enable);
            this.groupBox4.Controls.Add(this.lblAO1);
            this.groupBox4.Controls.Add(this.lblAO0);
            this.groupBox4.Controls.Add(this.numAO1);
            this.groupBox4.Controls.Add(this.numAO0);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.lblheflow);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.lblsf6flow);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(15, 26);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(441, 180);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Flow Controllers";
            // 
            // readButton
            // 
            this.readButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.readButton.Location = new System.Drawing.Point(154, 140);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(99, 34);
            this.readButton.TabIndex = 3;
            this.readButton.Text = "Start Reading";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.toggleReading);
            // 
            // chkAO1Enable
            // 
            this.chkAO1Enable.AutoSize = true;
            this.chkAO1Enable.Location = new System.Drawing.Point(248, 112);
            this.chkAO1Enable.Name = "chkAO1Enable";
            this.chkAO1Enable.Size = new System.Drawing.Size(67, 17);
            this.chkAO1Enable.TabIndex = 13;
            this.chkAO1Enable.Text = "Flow ON";
            this.chkAO1Enable.UseVisualStyleBackColor = true;
            this.chkAO1Enable.CheckedChanged += new System.EventHandler(this.chkAO1Enable_CheckedChanged);
            // 
            // chkAO0Enable
            // 
            this.chkAO0Enable.AutoSize = true;
            this.chkAO0Enable.Location = new System.Drawing.Point(10, 111);
            this.chkAO0Enable.Name = "chkAO0Enable";
            this.chkAO0Enable.Size = new System.Drawing.Size(67, 17);
            this.chkAO0Enable.TabIndex = 12;
            this.chkAO0Enable.Text = "Flow ON";
            this.chkAO0Enable.UseVisualStyleBackColor = true;
            this.chkAO0Enable.CheckedChanged += new System.EventHandler(this.chkAO0Enable_CheckedChanged);
            // 
            // lblAO1
            // 
            this.lblAO1.AutoSize = true;
            this.lblAO1.Location = new System.Drawing.Point(335, 87);
            this.lblAO1.Name = "lblAO1";
            this.lblAO1.Size = new System.Drawing.Size(44, 13);
            this.lblAO1.TabIndex = 11;
            this.lblAO1.Text = "(0.00 V)";
            // 
            // lblAO0
            // 
            this.lblAO0.AutoSize = true;
            this.lblAO0.Location = new System.Drawing.Point(94, 88);
            this.lblAO0.Name = "lblAO0";
            this.lblAO0.Size = new System.Drawing.Size(44, 13);
            this.lblAO0.TabIndex = 10;
            this.lblAO0.Text = "(0.00 V)";
            // 
            // numAO1
            // 
            this.numAO1.DecimalPlaces = 3;
            this.numAO1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numAO1.Location = new System.Drawing.Point(248, 84);
            this.numAO1.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numAO1.Name = "numAO1";
            this.numAO1.Size = new System.Drawing.Size(77, 20);
            this.numAO1.TabIndex = 9;
            this.numAO1.ValueChanged += new System.EventHandler(this.numAO1_ValueChanged);
            // 
            // numAO0
            // 
            this.numAO0.DecimalPlaces = 3;
            this.numAO0.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numAO0.Location = new System.Drawing.Point(10, 84);
            this.numAO0.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numAO0.Name = "numAO0";
            this.numAO0.Size = new System.Drawing.Size(77, 20);
            this.numAO0.TabIndex = 8;
            this.numAO0.ValueChanged += new System.EventHandler(this.numAO0_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(245, 67);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(177, 13);
            this.label10.TabIndex = 7;
            this.label10.Text = "He flow set point in sccm (min 0.15):";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 67);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(182, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "SF6 flow set point in sccm (min 0.01):";
            // 
            // lblheflow
            // 
            this.lblheflow.AutoSize = true;
            this.lblheflow.Location = new System.Drawing.Point(300, 27);
            this.lblheflow.Name = "lblheflow";
            this.lblheflow.Size = new System.Drawing.Size(62, 13);
            this.lblheflow.TabIndex = 5;
            this.lblheflow.Text = "0.000 sccm";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(245, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "He Flow:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "SF6 Flow:";
            // 
            // lblsf6flow
            // 
            this.lblsf6flow.AutoSize = true;
            this.lblsf6flow.Location = new System.Drawing.Point(67, 27);
            this.lblsf6flow.Name = "lblsf6flow";
            this.lblsf6flow.Size = new System.Drawing.Size(62, 13);
            this.lblsf6flow.TabIndex = 4;
            this.lblsf6flow.Text = "0.000 sccm";
            // 
            // FlowTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox4);
            this.Name = "FlowTabView";
            this.Size = new System.Drawing.Size(702, 810);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAO1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAO0)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private ScatterPlot scatterPlot2;
        private XAxis xAxis2;
        private YAxis yAxis2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.CheckBox chkAO1Enable;
        private System.Windows.Forms.CheckBox chkAO0Enable;
        private System.Windows.Forms.Label lblAO1;
        private System.Windows.Forms.Label lblAO0;
        private System.Windows.Forms.NumericUpDown numAO1;
        private System.Windows.Forms.NumericUpDown numAO0;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblheflow;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblsf6flow;
    }
}
