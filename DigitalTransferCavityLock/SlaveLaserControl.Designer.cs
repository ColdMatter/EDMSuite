
namespace DigitalTransferCavityLock
{
    partial class SlaveLaserControl
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.InputEnable = new System.Windows.Forms.CheckBox();
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
            this.ErrorGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.RMSError = new System.Windows.Forms.TextBox();
            this.ResetRMS = new System.Windows.Forms.Button();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorGraph)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.InputEnable);
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
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(260, 155);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Slave Control";
            // 
            // InputEnable
            // 
            this.InputEnable.AutoSize = true;
            this.InputEnable.Location = new System.Drawing.Point(154, 132);
            this.InputEnable.Name = "InputEnable";
            this.InputEnable.Size = new System.Drawing.Size(86, 17);
            this.InputEnable.TabIndex = 4;
            this.InputEnable.Text = "Enable Input";
            this.InputEnable.UseVisualStyleBackColor = true;
            this.InputEnable.CheckedChanged += new System.EventHandler(this.InputEnable_CheckedChanged);
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
            this.SlaveVoltageFeedback.Location = new System.Drawing.Point(154, 110);
            this.SlaveVoltageFeedback.Name = "SlaveVoltageFeedback";
            this.SlaveVoltageFeedback.Size = new System.Drawing.Size(100, 20);
            this.SlaveVoltageFeedback.TabIndex = 2;
            this.SlaveVoltageFeedback.Text = "0";
            this.SlaveVoltageFeedback.Leave += new System.EventHandler(this.SlaveVoltageFeedback_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 89);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Lock Location [ms]";
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
            this.slaveGain.Location = new System.Drawing.Point(154, 63);
            this.slaveGain.Name = "slaveGain";
            this.slaveGain.Size = new System.Drawing.Size(100, 20);
            this.slaveGain.TabIndex = 0;
            this.slaveGain.Text = "0";
            this.slaveGain.Leave += new System.EventHandler(this.slaveGain_Leave);
            // 
            // LockSlave
            // 
            this.LockSlave.AutoSize = true;
            this.LockSlave.Enabled = false;
            this.LockSlave.Location = new System.Drawing.Point(11, 132);
            this.LockSlave.Name = "LockSlave";
            this.LockSlave.Size = new System.Drawing.Size(50, 17);
            this.LockSlave.TabIndex = 3;
            this.LockSlave.Text = "Lock";
            this.LockSlave.UseVisualStyleBackColor = true;
            this.LockSlave.CheckedChanged += new System.EventHandler(this.LockSlave_CheckedChanged);
            // 
            // slaveLockLocV
            // 
            this.slaveLockLocV.Location = new System.Drawing.Point(154, 86);
            this.slaveLockLocV.Name = "slaveLockLocV";
            this.slaveLockLocV.Size = new System.Drawing.Size(100, 20);
            this.slaveLockLocV.TabIndex = 1;
            this.slaveLockLocV.Text = "0";
            this.slaveLockLocV.Leave += new System.EventHandler(this.slaveLockLocV_Leave);
            // 
            // slaveLocV
            // 
            this.slaveLocV.Enabled = false;
            this.slaveLocV.Location = new System.Drawing.Point(154, 41);
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
            this.slaveLocMS.Location = new System.Drawing.Point(154, 19);
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
            // ErrorGraph
            // 
            this.ErrorGraph.Location = new System.Drawing.Point(6, 19);
            this.ErrorGraph.Name = "ErrorGraph";
            this.ErrorGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.ErrorGraph.Size = new System.Drawing.Size(247, 121);
            this.ErrorGraph.TabIndex = 4;
            this.ErrorGraph.UseColorGenerator = true;
            this.ErrorGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.ErrorGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ResetRMS);
            this.groupBox1.Controls.Add(this.RMSError);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.ErrorGraph);
            this.groupBox1.Location = new System.Drawing.Point(4, 165);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 192);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Error";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "RMS Error [MHz]";
            // 
            // RMSError
            // 
            this.RMSError.Enabled = false;
            this.RMSError.Location = new System.Drawing.Point(152, 147);
            this.RMSError.Name = "RMSError";
            this.RMSError.Size = new System.Drawing.Size(100, 20);
            this.RMSError.TabIndex = 5;
            // 
            // ResetRMS
            // 
            this.ResetRMS.Location = new System.Drawing.Point(6, 164);
            this.ResetRMS.Name = "ResetRMS";
            this.ResetRMS.Size = new System.Drawing.Size(87, 23);
            this.ResetRMS.TabIndex = 5;
            this.ResetRMS.Text = "Reset Error";
            this.ResetRMS.UseVisualStyleBackColor = true;
            this.ResetRMS.Click += new System.EventHandler(this.ResetRMS_Click);
            // 
            // SlaveLaserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Name = "SlaveLaserControl";
            this.Size = new System.Drawing.Size(267, 360);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorGraph)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.CheckBox InputEnable;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.TextBox SlaveVoltageFeedback;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox slaveGain;
        public System.Windows.Forms.CheckBox LockSlave;
        public System.Windows.Forms.TextBox slaveLocV;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox slaveLocMS;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox slaveLockLocV;
        private NationalInstruments.UI.WindowsForms.ScatterGraph ErrorGraph;
        private NationalInstruments.UI.ScatterPlot scatterPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ResetRMS;
        private System.Windows.Forms.TextBox RMSError;
        private System.Windows.Forms.Label label1;
    }
}
