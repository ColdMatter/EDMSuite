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
            this.diodeSaturation = new NationalInstruments.UI.WindowsForms.Led();
            this.aomfrequency_label = new System.Windows.Forms.Label();
            this.AomVoltageBox = new System.Windows.Forms.NumericUpDown();
            this.LaserLockCheckBox = new System.Windows.Forms.CheckBox();
            this.EFieldTab = new System.Windows.Forms.TabPage();
            this.DeceleratorTab = new System.Windows.Forms.TabPage();
            this.tabControl.SuspendLayout();
            this.LaserTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.diodeSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AomVoltageBox)).BeginInit();
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
            this.LaserTab.Controls.Add(this.diodeSaturation);
            this.LaserTab.Controls.Add(this.aomfrequency_label);
            this.LaserTab.Controls.Add(this.AomVoltageBox);
            this.LaserTab.Controls.Add(this.LaserLockCheckBox);
            this.LaserTab.Location = new System.Drawing.Point(4, 22);
            this.LaserTab.Name = "LaserTab";
            this.LaserTab.Padding = new System.Windows.Forms.Padding(3);
            this.LaserTab.Size = new System.Drawing.Size(347, 225);
            this.LaserTab.TabIndex = 0;
            this.LaserTab.Text = "Laser";
            this.LaserTab.UseVisualStyleBackColor = true;
            // 
            // diodeSaturation
            // 
            this.diodeSaturation.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.diodeSaturation.Location = new System.Drawing.Point(277, 3);
            this.diodeSaturation.Name = "diodeSaturation";
            this.diodeSaturation.OnColor = System.Drawing.Color.Crimson;
            this.diodeSaturation.Size = new System.Drawing.Size(64, 64);
            this.diodeSaturation.TabIndex = 2;
            // 
            // aomfrequency_label
            // 
            this.aomfrequency_label.AutoSize = true;
            this.aomfrequency_label.Location = new System.Drawing.Point(85, 73);
            this.aomfrequency_label.Name = "aomfrequency_label";
            this.aomfrequency_label.Size = new System.Drawing.Size(115, 13);
            this.aomfrequency_label.TabIndex = 1;
            this.aomfrequency_label.Text = "AOM Frequency (MHz)";
            // 
            // AomVoltageBox
            // 
            this.AomVoltageBox.DecimalPlaces = 2;
            this.AomVoltageBox.Location = new System.Drawing.Point(17, 73);
            this.AomVoltageBox.Maximum = new decimal(new int[] {
            192,
            0,
            0,
            0});
            this.AomVoltageBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.AomVoltageBox.Name = "AomVoltageBox";
            this.AomVoltageBox.Size = new System.Drawing.Size(62, 20);
            this.AomVoltageBox.TabIndex = 1;
            this.AomVoltageBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.AomVoltageBox.ValueChanged += new System.EventHandler(this.AomVoltageBox_ValueChanged);
            // 
            // LaserLockCheckBox
            // 
            this.LaserLockCheckBox.AutoSize = true;
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
            ((System.ComponentModel.ISupportInitialize)(this.diodeSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AomVoltageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage LaserTab;
        private System.Windows.Forms.TabPage EFieldTab;
        private System.Windows.Forms.TabPage DeceleratorTab;
        public System.Windows.Forms.CheckBox LaserLockCheckBox;
        public System.Windows.Forms.NumericUpDown AomVoltageBox;
        private System.Windows.Forms.Label aomfrequency_label;
        public NationalInstruments.UI.WindowsForms.Led diodeSaturation;
    }
}

