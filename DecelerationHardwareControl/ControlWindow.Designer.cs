namespace DecelerationHardwareControl
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.LaserTab = new System.Windows.Forms.TabPage();
            this.LaserLockCheckBox = new System.Windows.Forms.CheckBox();
            this.EFieldTab = new System.Windows.Forms.TabPage();
            this.DeceleratorTab = new System.Windows.Forms.TabPage();
            this.tabControl.SuspendLayout();
            this.LaserTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.LaserTab);
            this.tabControl.Controls.Add(this.EFieldTab);
            this.tabControl.Controls.Add(this.DeceleratorTab);
            this.tabControl.Location = new System.Drawing.Point(1, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(355, 251);
            this.tabControl.TabIndex = 0;
            // 
            // LaserTab
            // 
            this.LaserTab.Controls.Add(this.LaserLockCheckBox);
            this.LaserTab.Location = new System.Drawing.Point(4, 22);
            this.LaserTab.Name = "LaserTab";
            this.LaserTab.Padding = new System.Windows.Forms.Padding(3);
            this.LaserTab.Size = new System.Drawing.Size(347, 225);
            this.LaserTab.TabIndex = 0;
            this.LaserTab.Text = "Laser";
            this.LaserTab.UseVisualStyleBackColor = true;
            // 
            // LaserLockCheckBox
            // 
            this.LaserLockCheckBox.AutoSize = true;
            this.LaserLockCheckBox.Enabled = false;
            this.LaserLockCheckBox.Location = new System.Drawing.Point(17, 21);
            this.LaserLockCheckBox.Name = "LaserLockCheckBox";
            this.LaserLockCheckBox.Size = new System.Drawing.Size(62, 17);
            this.LaserLockCheckBox.TabIndex = 0;
            this.LaserLockCheckBox.Text = "Locked";
            this.LaserLockCheckBox.UseVisualStyleBackColor = true;
            // 
            // EFieldTab
            // 
            this.EFieldTab.Location = new System.Drawing.Point(4, 22);
            this.EFieldTab.Name = "EFieldTab";
            this.EFieldTab.Padding = new System.Windows.Forms.Padding(3);
            this.EFieldTab.Size = new System.Drawing.Size(347, 225);
            this.EFieldTab.TabIndex = 1;
            this.EFieldTab.Text = "Electric Fields";
            this.EFieldTab.UseVisualStyleBackColor = true;
            // 
            // DeceleratorTab
            // 
            this.DeceleratorTab.Location = new System.Drawing.Point(4, 22);
            this.DeceleratorTab.Name = "DeceleratorTab";
            this.DeceleratorTab.Padding = new System.Windows.Forms.Padding(3);
            this.DeceleratorTab.Size = new System.Drawing.Size(347, 225);
            this.DeceleratorTab.TabIndex = 2;
            this.DeceleratorTab.Text = "Decelerator";
            this.DeceleratorTab.UseVisualStyleBackColor = true;
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 266);
            this.Controls.Add(this.tabControl);
            this.MaximizeBox = false;
            this.Name = "ControlWindow";
            this.Text = "Decelerator Control";
            this.tabControl.ResumeLayout(false);
            this.LaserTab.ResumeLayout(false);
            this.LaserTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage LaserTab;
        private System.Windows.Forms.TabPage EFieldTab;
        private System.Windows.Forms.TabPage DeceleratorTab;
        public System.Windows.Forms.CheckBox LaserLockCheckBox;
    }
}

