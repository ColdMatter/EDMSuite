
namespace LattHardwareController
{
    partial class Form1
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
            this.textBoxSourcePressure = new System.Windows.Forms.TextBox();
            this.buttonStartPMonitor = new System.Windows.Forms.Button();
            this.buttonStopPMonitor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source:";
            // 
            // textBoxSourcePressure
            // 
            this.textBoxSourcePressure.Location = new System.Drawing.Point(63, 17);
            this.textBoxSourcePressure.Name = "textBoxSourcePressure";
            this.textBoxSourcePressure.ReadOnly = true;
            this.textBoxSourcePressure.Size = new System.Drawing.Size(100, 20);
            this.textBoxSourcePressure.TabIndex = 1;
            // 
            // buttonStartPMonitor
            // 
            this.buttonStartPMonitor.Location = new System.Drawing.Point(21, 68);
            this.buttonStartPMonitor.Name = "buttonStartPMonitor";
            this.buttonStartPMonitor.Size = new System.Drawing.Size(75, 23);
            this.buttonStartPMonitor.TabIndex = 2;
            this.buttonStartPMonitor.Text = "Start";
            this.buttonStartPMonitor.UseVisualStyleBackColor = true;
            // 
            // buttonStopPMonitor
            // 
            this.buttonStopPMonitor.Location = new System.Drawing.Point(102, 68);
            this.buttonStopPMonitor.Name = "buttonStopPMonitor";
            this.buttonStopPMonitor.Size = new System.Drawing.Size(75, 23);
            this.buttonStopPMonitor.TabIndex = 3;
            this.buttonStopPMonitor.Text = "Stop";
            this.buttonStopPMonitor.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonStopPMonitor);
            this.Controls.Add(this.buttonStartPMonitor);
            this.Controls.Add(this.textBoxSourcePressure);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textBoxSourcePressure;
        private System.Windows.Forms.Button buttonStartPMonitor;
        private System.Windows.Forms.Button buttonStopPMonitor;
    }
}

