namespace UEDMHardwareControl
{
    partial class CooldownModeOptionsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CooldownModeOptionsDialog));
            this.groupBoxCoolingDownTheSource = new System.Windows.Forms.GroupBox();
            this.textBoxCryoStartingTemperatureMax = new System.Windows.Forms.TextBox();
            this.labelCryoStartingTemperatureMax = new System.Windows.Forms.Label();
            this.textBoxCryoStartingPressure = new System.Windows.Forms.TextBox();
            this.labelCryoStartingPressure = new System.Windows.Forms.Label();
            this.textBoxCoolDownPTPollPeriod = new System.Windows.Forms.TextBox();
            this.labelCoolDownPTPollPeriod = new System.Windows.Forms.Label();
            this.groupBoxRefreshModeTargetTemperatureReached = new System.Windows.Forms.GroupBox();
            this.textBoxSourceModeWaitPTPollPeriod = new System.Windows.Forms.TextBox();
            this.labelSourceModeWaitPTPollPeriod = new System.Windows.Forms.Label();
            this.groupBoxWarmup = new System.Windows.Forms.GroupBox();
            this.textBoxWarmupPTPollPeriod = new System.Windows.Forms.TextBox();
            this.labelWarmupPTPollPeriod = new System.Windows.Forms.Label();
            this.groupBoxGeneralConstants = new System.Windows.Forms.GroupBox();
            this.textBoxTurbomolecularPumpUpperPressureLimit = new System.Windows.Forms.TextBox();
            this.labelTurbomolecularPumpUpperPressureLimit = new System.Windows.Forms.Label();
            this.btReset = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btSaveSettings = new System.Windows.Forms.Button();
            this.groupBoxCoolingDownTheSource.SuspendLayout();
            this.groupBoxRefreshModeTargetTemperatureReached.SuspendLayout();
            this.groupBoxWarmup.SuspendLayout();
            this.groupBoxGeneralConstants.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxCoolingDownTheSource
            // 
            this.groupBoxCoolingDownTheSource.Controls.Add(this.textBoxCryoStartingTemperatureMax);
            this.groupBoxCoolingDownTheSource.Controls.Add(this.labelCryoStartingTemperatureMax);
            this.groupBoxCoolingDownTheSource.Controls.Add(this.textBoxCryoStartingPressure);
            this.groupBoxCoolingDownTheSource.Controls.Add(this.labelCryoStartingPressure);
            this.groupBoxCoolingDownTheSource.Controls.Add(this.textBoxCoolDownPTPollPeriod);
            this.groupBoxCoolingDownTheSource.Controls.Add(this.labelCoolDownPTPollPeriod);
            this.groupBoxCoolingDownTheSource.Location = new System.Drawing.Point(12, 234);
            this.groupBoxCoolingDownTheSource.Name = "groupBoxCoolingDownTheSource";
            this.groupBoxCoolingDownTheSource.Size = new System.Drawing.Size(477, 118);
            this.groupBoxCoolingDownTheSource.TabIndex = 12;
            this.groupBoxCoolingDownTheSource.TabStop = false;
            this.groupBoxCoolingDownTheSource.Text = "Cooling down the source";
            // 
            // textBoxCryoStartingTemperatureMax
            // 
            this.textBoxCryoStartingTemperatureMax.Location = new System.Drawing.Point(337, 80);
            this.textBoxCryoStartingTemperatureMax.Name = "textBoxCryoStartingTemperatureMax";
            this.textBoxCryoStartingTemperatureMax.Size = new System.Drawing.Size(119, 22);
            this.textBoxCryoStartingTemperatureMax.TabIndex = 7;
            this.textBoxCryoStartingTemperatureMax.TextChanged += new System.EventHandler(this.textBoxCryoStartingTemperatureMax_TextChanged);
            // 
            // labelCryoStartingTemperatureMax
            // 
            this.labelCryoStartingTemperatureMax.AutoSize = true;
            this.labelCryoStartingTemperatureMax.Location = new System.Drawing.Point(6, 83);
            this.labelCryoStartingTemperatureMax.Name = "labelCryoStartingTemperatureMax";
            this.labelCryoStartingTemperatureMax.Size = new System.Drawing.Size(322, 17);
            this.labelCryoStartingTemperatureMax.TabIndex = 6;
            this.labelCryoStartingTemperatureMax.Text = "Max temperature at which cryo should turn on (K):";
            // 
            // textBoxCryoStartingPressure
            // 
            this.textBoxCryoStartingPressure.Location = new System.Drawing.Point(337, 52);
            this.textBoxCryoStartingPressure.Name = "textBoxCryoStartingPressure";
            this.textBoxCryoStartingPressure.Size = new System.Drawing.Size(119, 22);
            this.textBoxCryoStartingPressure.TabIndex = 5;
            this.textBoxCryoStartingPressure.TextChanged += new System.EventHandler(this.textBoxCryoStartingPressure_TextChanged);
            // 
            // labelCryoStartingPressure
            // 
            this.labelCryoStartingPressure.AutoSize = true;
            this.labelCryoStartingPressure.Location = new System.Drawing.Point(6, 55);
            this.labelCryoStartingPressure.Name = "labelCryoStartingPressure";
            this.labelCryoStartingPressure.Size = new System.Drawing.Size(296, 17);
            this.labelCryoStartingPressure.TabIndex = 4;
            this.labelCryoStartingPressure.Text = "Pressure at which cryo should turn on (mbar):";
            // 
            // textBoxCoolDownPTPollPeriod
            // 
            this.textBoxCoolDownPTPollPeriod.Location = new System.Drawing.Point(337, 24);
            this.textBoxCoolDownPTPollPeriod.Name = "textBoxCoolDownPTPollPeriod";
            this.textBoxCoolDownPTPollPeriod.Size = new System.Drawing.Size(119, 22);
            this.textBoxCoolDownPTPollPeriod.TabIndex = 3;
            this.textBoxCoolDownPTPollPeriod.TextChanged += new System.EventHandler(this.textBoxCoolDownPTPollPeriod_TextChanged);
            // 
            // labelCoolDownPTPollPeriod
            // 
            this.labelCoolDownPTPollPeriod.AutoSize = true;
            this.labelCoolDownPTPollPeriod.Location = new System.Drawing.Point(6, 27);
            this.labelCoolDownPTPollPeriod.Name = "labelCoolDownPTPollPeriod";
            this.labelCoolDownPTPollPeriod.Size = new System.Drawing.Size(280, 17);
            this.labelCoolDownPTPollPeriod.TabIndex = 2;
            this.labelCoolDownPTPollPeriod.Text = "Pressure and temperature poll period (ms):";
            // 
            // groupBoxRefreshModeTargetTemperatureReached
            // 
            this.groupBoxRefreshModeTargetTemperatureReached.Controls.Add(this.textBoxSourceModeWaitPTPollPeriod);
            this.groupBoxRefreshModeTargetTemperatureReached.Controls.Add(this.labelSourceModeWaitPTPollPeriod);
            this.groupBoxRefreshModeTargetTemperatureReached.Location = new System.Drawing.Point(12, 159);
            this.groupBoxRefreshModeTargetTemperatureReached.Name = "groupBoxRefreshModeTargetTemperatureReached";
            this.groupBoxRefreshModeTargetTemperatureReached.Size = new System.Drawing.Size(477, 69);
            this.groupBoxRefreshModeTargetTemperatureReached.TabIndex = 11;
            this.groupBoxRefreshModeTargetTemperatureReached.TabStop = false;
            this.groupBoxRefreshModeTargetTemperatureReached.Text = "Once target temperature has been reached";
            // 
            // textBoxSourceModeWaitPTPollPeriod
            // 
            this.textBoxSourceModeWaitPTPollPeriod.Location = new System.Drawing.Point(337, 27);
            this.textBoxSourceModeWaitPTPollPeriod.Name = "textBoxSourceModeWaitPTPollPeriod";
            this.textBoxSourceModeWaitPTPollPeriod.Size = new System.Drawing.Size(119, 22);
            this.textBoxSourceModeWaitPTPollPeriod.TabIndex = 1;
            this.textBoxSourceModeWaitPTPollPeriod.TextChanged += new System.EventHandler(this.textBoxSourceModeWaitPTPollPeriod_TextChanged);
            // 
            // labelSourceModeWaitPTPollPeriod
            // 
            this.labelSourceModeWaitPTPollPeriod.AutoSize = true;
            this.labelSourceModeWaitPTPollPeriod.Location = new System.Drawing.Point(6, 30);
            this.labelSourceModeWaitPTPollPeriod.Name = "labelSourceModeWaitPTPollPeriod";
            this.labelSourceModeWaitPTPollPeriod.Size = new System.Drawing.Size(280, 17);
            this.labelSourceModeWaitPTPollPeriod.TabIndex = 0;
            this.labelSourceModeWaitPTPollPeriod.Text = "Pressure and temperature poll period (ms):";
            // 
            // groupBoxWarmup
            // 
            this.groupBoxWarmup.Controls.Add(this.textBoxWarmupPTPollPeriod);
            this.groupBoxWarmup.Controls.Add(this.labelWarmupPTPollPeriod);
            this.groupBoxWarmup.Location = new System.Drawing.Point(12, 85);
            this.groupBoxWarmup.Name = "groupBoxWarmup";
            this.groupBoxWarmup.Size = new System.Drawing.Size(477, 68);
            this.groupBoxWarmup.TabIndex = 10;
            this.groupBoxWarmup.TabStop = false;
            this.groupBoxWarmup.Text = "Warm up to target temperature";
            // 
            // textBoxWarmupPTPollPeriod
            // 
            this.textBoxWarmupPTPollPeriod.Location = new System.Drawing.Point(337, 30);
            this.textBoxWarmupPTPollPeriod.Name = "textBoxWarmupPTPollPeriod";
            this.textBoxWarmupPTPollPeriod.Size = new System.Drawing.Size(119, 22);
            this.textBoxWarmupPTPollPeriod.TabIndex = 1;
            this.textBoxWarmupPTPollPeriod.TextChanged += new System.EventHandler(this.textBoxWarmupPTPollPeriod_TextChanged);
            // 
            // labelWarmupPTPollPeriod
            // 
            this.labelWarmupPTPollPeriod.AutoSize = true;
            this.labelWarmupPTPollPeriod.Location = new System.Drawing.Point(6, 33);
            this.labelWarmupPTPollPeriod.Name = "labelWarmupPTPollPeriod";
            this.labelWarmupPTPollPeriod.Size = new System.Drawing.Size(280, 17);
            this.labelWarmupPTPollPeriod.TabIndex = 0;
            this.labelWarmupPTPollPeriod.Text = "Pressure and temperature poll period (ms):";
            // 
            // groupBoxGeneralConstants
            // 
            this.groupBoxGeneralConstants.Controls.Add(this.textBoxTurbomolecularPumpUpperPressureLimit);
            this.groupBoxGeneralConstants.Controls.Add(this.labelTurbomolecularPumpUpperPressureLimit);
            this.groupBoxGeneralConstants.Location = new System.Drawing.Point(12, 12);
            this.groupBoxGeneralConstants.Name = "groupBoxGeneralConstants";
            this.groupBoxGeneralConstants.Size = new System.Drawing.Size(477, 67);
            this.groupBoxGeneralConstants.TabIndex = 9;
            this.groupBoxGeneralConstants.TabStop = false;
            this.groupBoxGeneralConstants.Text = "General";
            // 
            // textBoxTurbomolecularPumpUpperPressureLimit
            // 
            this.textBoxTurbomolecularPumpUpperPressureLimit.Location = new System.Drawing.Point(337, 30);
            this.textBoxTurbomolecularPumpUpperPressureLimit.Name = "textBoxTurbomolecularPumpUpperPressureLimit";
            this.textBoxTurbomolecularPumpUpperPressureLimit.Size = new System.Drawing.Size(119, 22);
            this.textBoxTurbomolecularPumpUpperPressureLimit.TabIndex = 4;
            this.textBoxTurbomolecularPumpUpperPressureLimit.TextChanged += new System.EventHandler(this.textBoxTurbomolecularPumpUpperPressureLimit_TextChanged);
            // 
            // labelTurbomolecularPumpUpperPressureLimit
            // 
            this.labelTurbomolecularPumpUpperPressureLimit.AutoSize = true;
            this.labelTurbomolecularPumpUpperPressureLimit.Location = new System.Drawing.Point(6, 30);
            this.labelTurbomolecularPumpUpperPressureLimit.Name = "labelTurbomolecularPumpUpperPressureLimit";
            this.labelTurbomolecularPumpUpperPressureLimit.Size = new System.Drawing.Size(325, 17);
            this.labelTurbomolecularPumpUpperPressureLimit.TabIndex = 3;
            this.labelTurbomolecularPumpUpperPressureLimit.Text = "Turbomolecular pump upper pressure limit (mbar):";
            // 
            // btReset
            // 
            this.btReset.Location = new System.Drawing.Point(12, 358);
            this.btReset.Name = "btReset";
            this.btReset.Size = new System.Drawing.Size(92, 28);
            this.btReset.TabIndex = 15;
            this.btReset.Text = "Reset";
            this.btReset.UseVisualStyleBackColor = true;
            this.btReset.Click += new System.EventHandler(this.btReset_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(397, 358);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(92, 28);
            this.btCancel.TabIndex = 14;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btSaveSettings
            // 
            this.btSaveSettings.Location = new System.Drawing.Point(299, 358);
            this.btSaveSettings.Name = "btSaveSettings";
            this.btSaveSettings.Size = new System.Drawing.Size(92, 28);
            this.btSaveSettings.TabIndex = 13;
            this.btSaveSettings.Text = "Save";
            this.btSaveSettings.UseVisualStyleBackColor = true;
            this.btSaveSettings.Click += new System.EventHandler(this.btSaveSettings_Click);
            // 
            // CooldownModeOptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 395);
            this.Controls.Add(this.btReset);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btSaveSettings);
            this.Controls.Add(this.groupBoxCoolingDownTheSource);
            this.Controls.Add(this.groupBoxRefreshModeTargetTemperatureReached);
            this.Controls.Add(this.groupBoxWarmup);
            this.Controls.Add(this.groupBoxGeneralConstants);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CooldownModeOptionsDialog";
            this.Text = "Cool down mode options";
            this.groupBoxCoolingDownTheSource.ResumeLayout(false);
            this.groupBoxCoolingDownTheSource.PerformLayout();
            this.groupBoxRefreshModeTargetTemperatureReached.ResumeLayout(false);
            this.groupBoxRefreshModeTargetTemperatureReached.PerformLayout();
            this.groupBoxWarmup.ResumeLayout(false);
            this.groupBoxWarmup.PerformLayout();
            this.groupBoxGeneralConstants.ResumeLayout(false);
            this.groupBoxGeneralConstants.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxCoolingDownTheSource;
        private System.Windows.Forms.TextBox textBoxCryoStartingTemperatureMax;
        private System.Windows.Forms.Label labelCryoStartingTemperatureMax;
        private System.Windows.Forms.TextBox textBoxCryoStartingPressure;
        private System.Windows.Forms.Label labelCryoStartingPressure;
        private System.Windows.Forms.TextBox textBoxCoolDownPTPollPeriod;
        private System.Windows.Forms.Label labelCoolDownPTPollPeriod;
        private System.Windows.Forms.GroupBox groupBoxRefreshModeTargetTemperatureReached;
        private System.Windows.Forms.TextBox textBoxSourceModeWaitPTPollPeriod;
        private System.Windows.Forms.Label labelSourceModeWaitPTPollPeriod;
        private System.Windows.Forms.GroupBox groupBoxWarmup;
        private System.Windows.Forms.TextBox textBoxWarmupPTPollPeriod;
        private System.Windows.Forms.Label labelWarmupPTPollPeriod;
        private System.Windows.Forms.GroupBox groupBoxGeneralConstants;
        private System.Windows.Forms.TextBox textBoxTurbomolecularPumpUpperPressureLimit;
        private System.Windows.Forms.Label labelTurbomolecularPumpUpperPressureLimit;
        private System.Windows.Forms.Button btReset;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btSaveSettings;
    }
}