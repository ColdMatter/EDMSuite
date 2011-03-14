namespace MOTMaster
{
    partial class ControllerWindow
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
            this.startHardwareControlButton = new System.Windows.Forms.Button();
            this.stopHardwareControlButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // startHardwareControlButton
            // 
            this.startHardwareControlButton.Location = new System.Drawing.Point(2, 393);
            this.startHardwareControlButton.Name = "startHardwareControlButton";
            this.startHardwareControlButton.Size = new System.Drawing.Size(137, 23);
            this.startHardwareControlButton.TabIndex = 0;
            this.startHardwareControlButton.Text = "Start Hardware Control";
            this.startHardwareControlButton.UseVisualStyleBackColor = true;
            this.startHardwareControlButton.Click += new System.EventHandler(this.StartHardwareControlButton_Click);
            // 
            // stopHardwareControlButton
            // 
            this.stopHardwareControlButton.Location = new System.Drawing.Point(145, 393);
            this.stopHardwareControlButton.Name = "stopHardwareControlButton";
            this.stopHardwareControlButton.Size = new System.Drawing.Size(126, 23);
            this.stopHardwareControlButton.TabIndex = 1;
            this.stopHardwareControlButton.Text = "Stop Hardware Control";
            this.stopHardwareControlButton.UseVisualStyleBackColor = true;
            this.stopHardwareControlButton.Click += new System.EventHandler(this.stopHardwareControlButton_Click);
            // 
            // ControllerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 419);
            this.Controls.Add(this.stopHardwareControlButton);
            this.Controls.Add(this.startHardwareControlButton);
            this.Name = "ControllerWindow";
            this.Text = "MOTMaster Main Window";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startHardwareControlButton;
        private System.Windows.Forms.Button stopHardwareControlButton;
    }
}

