namespace csAcq4
{
    partial class FormMain
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
                if (m_bitmap != null)
                    m_bitmap.Dispose();

                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.SensitivityGainLabel = new System.Windows.Forms.Label();
            this.ExposureTimeLabel = new System.Windows.Forms.Label();
            this.QuerySensitivityGainButton = new System.Windows.Forms.Button();
            this.UpdateSensitivityGainButton = new System.Windows.Forms.Button();
            this.QueryExposureTimeButton = new System.Windows.Forms.Button();
            this.UpdateExposureTimeButton = new System.Windows.Forms.Button();
            this.SensitivityGainTextBox = new System.Windows.Forms.TextBox();
            this.ExposureTimeTextBox = new System.Windows.Forms.TextBox();
            this.SensorTemperatureLabel = new System.Windows.Forms.Label();
            this.QuerySensorTemperatureButton = new System.Windows.Forms.Button();
            this.LabelLutMin = new System.Windows.Forms.Label();
            this.LabelLutMax = new System.Windows.Forms.Label();
            this.EditLutMin = new System.Windows.Forms.TextBox();
            this.EditLutMax = new System.Windows.Forms.TextBox();
            this.PushAsterisk = new System.Windows.Forms.Button();
            this.HScrollLutMin = new System.Windows.Forms.HScrollBar();
            this.HScrollLutMax = new System.Windows.Forms.HScrollBar();
            this.PicDisplay = new System.Windows.Forms.PictureBox();
            this.PushInit = new System.Windows.Forms.Button();
            this.PushSnap = new System.Windows.Forms.Button();
            this.PushLive = new System.Windows.Forms.Button();
            this.PushIdle = new System.Windows.Forms.Button();
            this.PushFireTrigger = new System.Windows.Forms.Button();
            this.PushBufRelease = new System.Windows.Forms.Button();
            this.PushClose = new System.Windows.Forms.Button();
            this.PushUninit = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.comboTriggerSource = new System.Windows.Forms.ComboBox();
            this.QueryFrameCountButton = new System.Windows.Forms.Button();
            this.UpdateFrameCountButton = new System.Windows.Forms.Button();
            this.FrameCountTextBox = new System.Windows.Forms.TextBox();
            this.FrameCountLabel = new System.Windows.Forms.Label();
            this.TriggerSourceLabel = new System.Windows.Forms.Label();
            this.SetSaveDirectoryButton = new System.Windows.Forms.Button();
            this.SaveDirectoryLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelStatus
            // 
            this.LabelStatus.BackColor = System.Drawing.SystemColors.Control;
            this.LabelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelStatus.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelStatus.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LabelStatus.Location = new System.Drawing.Point(14, 14);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelStatus.Size = new System.Drawing.Size(521, 17);
            this.LabelStatus.TabIndex = 0;
            // 
            // SensitivityGainLabel
            // 
            this.SensitivityGainLabel.AutoSize = true;
            this.SensitivityGainLabel.Location = new System.Drawing.Point(313, 130);
            this.SensitivityGainLabel.Name = "SensitivityGainLabel";
            this.SensitivityGainLabel.Size = new System.Drawing.Size(142, 13);
            this.SensitivityGainLabel.TabIndex = 19;
            this.SensitivityGainLabel.Text = "Current Sensitivity Gain: N/A";
            // 
            // ExposureTimeLabel
            // 
            this.ExposureTimeLabel.AutoSize = true;
            this.ExposureTimeLabel.Location = new System.Drawing.Point(313, 198);
            this.ExposureTimeLabel.Name = "ExposureTimeLabel";
            this.ExposureTimeLabel.Size = new System.Drawing.Size(140, 13);
            this.ExposureTimeLabel.TabIndex = 20;
            this.ExposureTimeLabel.Text = "Current Exposure Time: N/A";
            // 
            // QuerySensitivityGainButton
            // 
            this.QuerySensitivityGainButton.Location = new System.Drawing.Point(150, 125);
            this.QuerySensitivityGainButton.Name = "QuerySensitivityGainButton";
            this.QuerySensitivityGainButton.Size = new System.Drawing.Size(150, 23);
            this.QuerySensitivityGainButton.TabIndex = 21;
            this.QuerySensitivityGainButton.Text = "Query Sensitivity Gain";
            this.QuerySensitivityGainButton.UseVisualStyleBackColor = true;
            this.QuerySensitivityGainButton.Click += new System.EventHandler(this.QuerySensitivityGainButton_Click);
            // 
            // UpdateSensitivityGainButton
            // 
            this.UpdateSensitivityGainButton.Location = new System.Drawing.Point(150, 154);
            this.UpdateSensitivityGainButton.Name = "UpdateSensitivityGainButton";
            this.UpdateSensitivityGainButton.Size = new System.Drawing.Size(150, 23);
            this.UpdateSensitivityGainButton.TabIndex = 22;
            this.UpdateSensitivityGainButton.Text = "Update Sensitivity Gain";
            this.UpdateSensitivityGainButton.UseVisualStyleBackColor = true;
            this.UpdateSensitivityGainButton.Click += new System.EventHandler(this.UpdateSensitivityGainButton_Click);
            // 
            // QueryExposureTimeButton
            // 
            this.QueryExposureTimeButton.Location = new System.Drawing.Point(150, 193);
            this.QueryExposureTimeButton.Name = "QueryExposureTimeButton";
            this.QueryExposureTimeButton.Size = new System.Drawing.Size(150, 23);
            this.QueryExposureTimeButton.TabIndex = 23;
            this.QueryExposureTimeButton.Text = "Query Exposure Time";
            this.QueryExposureTimeButton.UseVisualStyleBackColor = true;
            this.QueryExposureTimeButton.Click += new System.EventHandler(this.QueryExposureTimeButton_Click);
            // 
            // UpdateExposureTimeButton
            // 
            this.UpdateExposureTimeButton.Location = new System.Drawing.Point(150, 222);
            this.UpdateExposureTimeButton.Name = "UpdateExposureTimeButton";
            this.UpdateExposureTimeButton.Size = new System.Drawing.Size(150, 23);
            this.UpdateExposureTimeButton.TabIndex = 24;
            this.UpdateExposureTimeButton.Text = "Update Exposure Time (s)";
            this.UpdateExposureTimeButton.UseVisualStyleBackColor = true;
            this.UpdateExposureTimeButton.Click += new System.EventHandler(this.UpdateExposureTimeButton_Click);
            // 
            // SensitivityGainTextBox
            // 
            this.SensitivityGainTextBox.Location = new System.Drawing.Point(316, 157);
            this.SensitivityGainTextBox.Name = "SensitivityGainTextBox";
            this.SensitivityGainTextBox.Size = new System.Drawing.Size(100, 20);
            this.SensitivityGainTextBox.TabIndex = 25;
            // 
            // ExposureTimeTextBox
            // 
            this.ExposureTimeTextBox.Location = new System.Drawing.Point(316, 224);
            this.ExposureTimeTextBox.Name = "ExposureTimeTextBox";
            this.ExposureTimeTextBox.Size = new System.Drawing.Size(100, 20);
            this.ExposureTimeTextBox.TabIndex = 26;
            // 
            // SensorTemperatureLabel
            // 
            this.SensorTemperatureLabel.AutoSize = true;
            this.SensorTemperatureLabel.Location = new System.Drawing.Point(313, 89);
            this.SensorTemperatureLabel.Name = "SensorTemperatureLabel";
            this.SensorTemperatureLabel.Size = new System.Drawing.Size(166, 13);
            this.SensorTemperatureLabel.TabIndex = 27;
            this.SensorTemperatureLabel.Text = "Current Sensor Temperature: N/A";
            // 
            // QuerySensorTemperatureButton
            // 
            this.QuerySensorTemperatureButton.Location = new System.Drawing.Point(150, 84);
            this.QuerySensorTemperatureButton.Name = "QuerySensorTemperatureButton";
            this.QuerySensorTemperatureButton.Size = new System.Drawing.Size(150, 23);
            this.QuerySensorTemperatureButton.TabIndex = 28;
            this.QuerySensorTemperatureButton.Text = "Query Sensor Temperature";
            this.QuerySensorTemperatureButton.UseVisualStyleBackColor = true;
            this.QuerySensorTemperatureButton.Click += new System.EventHandler(this.QuerySensorTemperatureButton_Click);
            // 
            // LabelLutMin
            // 
            this.LabelLutMin.BackColor = System.Drawing.SystemColors.Control;
            this.LabelLutMin.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelLutMin.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LabelLutMin.Location = new System.Drawing.Point(14, 60);
            this.LabelLutMin.Name = "LabelLutMin";
            this.LabelLutMin.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelLutMin.Size = new System.Drawing.Size(52, 17);
            this.LabelLutMin.TabIndex = 4;
            this.LabelLutMin.Text = "LUT Min";
            // 
            // LabelLutMax
            // 
            this.LabelLutMax.BackColor = System.Drawing.SystemColors.Control;
            this.LabelLutMax.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelLutMax.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LabelLutMax.Location = new System.Drawing.Point(14, 38);
            this.LabelLutMax.Name = "LabelLutMax";
            this.LabelLutMax.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelLutMax.Size = new System.Drawing.Size(52, 17);
            this.LabelLutMax.TabIndex = 1;
            this.LabelLutMax.Text = "LUT Max";
            // 
            // EditLutMin
            // 
            this.EditLutMin.AcceptsReturn = true;
            this.EditLutMin.BackColor = System.Drawing.SystemColors.Window;
            this.EditLutMin.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.EditLutMin.ForeColor = System.Drawing.SystemColors.WindowText;
            this.EditLutMin.Location = new System.Drawing.Point(67, 64);
            this.EditLutMin.MaxLength = 0;
            this.EditLutMin.Name = "EditLutMin";
            this.EditLutMin.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.EditLutMin.Size = new System.Drawing.Size(50, 20);
            this.EditLutMin.TabIndex = 5;
            this.EditLutMin.TextChanged += new System.EventHandler(this.EditLutMin_TextChanged);
            // 
            // EditLutMax
            // 
            this.EditLutMax.AcceptsReturn = true;
            this.EditLutMax.BackColor = System.Drawing.SystemColors.Window;
            this.EditLutMax.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.EditLutMax.ForeColor = System.Drawing.SystemColors.WindowText;
            this.EditLutMax.Location = new System.Drawing.Point(67, 38);
            this.EditLutMax.MaxLength = 0;
            this.EditLutMax.Name = "EditLutMax";
            this.EditLutMax.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.EditLutMax.Size = new System.Drawing.Size(50, 20);
            this.EditLutMax.TabIndex = 2;
            this.EditLutMax.TextChanged += new System.EventHandler(this.EditLutMax_TextChanged);
            // 
            // PushAsterisk
            // 
            this.PushAsterisk.BackColor = System.Drawing.SystemColors.Control;
            this.PushAsterisk.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushAsterisk.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushAsterisk.Location = new System.Drawing.Point(484, 40);
            this.PushAsterisk.Name = "PushAsterisk";
            this.PushAsterisk.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushAsterisk.Size = new System.Drawing.Size(77, 34);
            this.PushAsterisk.TabIndex = 7;
            this.PushAsterisk.Text = "Auto Intensity";
            this.toolTip1.SetToolTip(this.PushAsterisk, "Auto LUT");
            this.PushAsterisk.UseVisualStyleBackColor = false;
            this.PushAsterisk.Click += new System.EventHandler(this.PushAsterisk_Click);
            // 
            // HScrollLutMin
            // 
            this.HScrollLutMin.Cursor = System.Windows.Forms.Cursors.Default;
            this.HScrollLutMin.LargeChange = 1;
            this.HScrollLutMin.Location = new System.Drawing.Point(122, 60);
            this.HScrollLutMin.Maximum = 32767;
            this.HScrollLutMin.Name = "HScrollLutMin";
            this.HScrollLutMin.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.HScrollLutMin.Size = new System.Drawing.Size(354, 17);
            this.HScrollLutMin.TabIndex = 6;
            this.HScrollLutMin.TabStop = true;
            this.HScrollLutMin.ValueChanged += new System.EventHandler(this.HScrollLutMin_ValueChanged);
            // 
            // HScrollLutMax
            // 
            this.HScrollLutMax.Cursor = System.Windows.Forms.Cursors.Default;
            this.HScrollLutMax.LargeChange = 1;
            this.HScrollLutMax.Location = new System.Drawing.Point(122, 38);
            this.HScrollLutMax.Maximum = 32767;
            this.HScrollLutMax.Name = "HScrollLutMax";
            this.HScrollLutMax.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.HScrollLutMax.Size = new System.Drawing.Size(354, 17);
            this.HScrollLutMax.TabIndex = 3;
            this.HScrollLutMax.TabStop = true;
            this.HScrollLutMax.ValueChanged += new System.EventHandler(this.HScrollLutMax_ValueChanged);
            // 
            // PicDisplay
            // 
            this.PicDisplay.BackColor = System.Drawing.SystemColors.Control;
            this.PicDisplay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PicDisplay.Cursor = System.Windows.Forms.Cursors.Default;
            this.PicDisplay.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PicDisplay.Location = new System.Drawing.Point(143, 271);
            this.PicDisplay.Name = "PicDisplay";
            this.PicDisplay.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PicDisplay.Size = new System.Drawing.Size(392, 275);
            this.PicDisplay.TabIndex = 8;
            this.PicDisplay.TabStop = false;
            // 
            // PushInit
            // 
            this.PushInit.BackColor = System.Drawing.SystemColors.Control;
            this.PushInit.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushInit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushInit.Location = new System.Drawing.Point(14, 95);
            this.PushInit.Name = "PushInit";
            this.PushInit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushInit.Size = new System.Drawing.Size(113, 33);
            this.PushInit.TabIndex = 9;
            this.PushInit.Text = "Init";
            this.PushInit.UseVisualStyleBackColor = false;
            this.PushInit.Click += new System.EventHandler(this.PushInit_Click);
            // 
            // PushSnap
            // 
            this.PushSnap.BackColor = System.Drawing.SystemColors.Control;
            this.PushSnap.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushSnap.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushSnap.Location = new System.Drawing.Point(14, 144);
            this.PushSnap.Name = "PushSnap";
            this.PushSnap.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushSnap.Size = new System.Drawing.Size(113, 33);
            this.PushSnap.TabIndex = 12;
            this.PushSnap.Text = "Snap";
            this.PushSnap.UseVisualStyleBackColor = false;
            this.PushSnap.Click += new System.EventHandler(this.PushSnap_Click);
            // 
            // PushLive
            // 
            this.PushLive.BackColor = System.Drawing.SystemColors.Control;
            this.PushLive.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushLive.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushLive.Location = new System.Drawing.Point(14, 193);
            this.PushLive.Name = "PushLive";
            this.PushLive.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushLive.Size = new System.Drawing.Size(113, 33);
            this.PushLive.TabIndex = 13;
            this.PushLive.Text = "Live";
            this.PushLive.UseVisualStyleBackColor = false;
            this.PushLive.Click += new System.EventHandler(this.PushLive_Click);
            // 
            // PushIdle
            // 
            this.PushIdle.BackColor = System.Drawing.SystemColors.Control;
            this.PushIdle.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushIdle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushIdle.Location = new System.Drawing.Point(14, 389);
            this.PushIdle.Name = "PushIdle";
            this.PushIdle.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushIdle.Size = new System.Drawing.Size(113, 33);
            this.PushIdle.TabIndex = 14;
            this.PushIdle.Text = "Stop Acquisition";
            this.PushIdle.UseVisualStyleBackColor = false;
            this.PushIdle.Click += new System.EventHandler(this.PushIdle_Click);
            // 
            // PushFireTrigger
            // 
            this.PushFireTrigger.BackColor = System.Drawing.SystemColors.Control;
            this.PushFireTrigger.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushFireTrigger.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushFireTrigger.Location = new System.Drawing.Point(14, 243);
            this.PushFireTrigger.Name = "PushFireTrigger";
            this.PushFireTrigger.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushFireTrigger.Size = new System.Drawing.Size(113, 33);
            this.PushFireTrigger.TabIndex = 15;
            this.PushFireTrigger.Text = "Fire Trigger";
            this.PushFireTrigger.UseVisualStyleBackColor = false;
            this.PushFireTrigger.Click += new System.EventHandler(this.PushFireTrigger_Click);
            // 
            // PushBufRelease
            // 
            this.PushBufRelease.BackColor = System.Drawing.SystemColors.Control;
            this.PushBufRelease.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushBufRelease.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushBufRelease.Location = new System.Drawing.Point(14, 428);
            this.PushBufRelease.Name = "PushBufRelease";
            this.PushBufRelease.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushBufRelease.Size = new System.Drawing.Size(113, 33);
            this.PushBufRelease.TabIndex = 16;
            this.PushBufRelease.Text = "Save Snaps";
            this.PushBufRelease.UseVisualStyleBackColor = false;
            this.PushBufRelease.Click += new System.EventHandler(this.PushBufRelease_Click);
            // 
            // PushClose
            // 
            this.PushClose.BackColor = System.Drawing.SystemColors.Control;
            this.PushClose.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushClose.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushClose.Location = new System.Drawing.Point(14, 486);
            this.PushClose.Name = "PushClose";
            this.PushClose.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushClose.Size = new System.Drawing.Size(113, 33);
            this.PushClose.TabIndex = 17;
            this.PushClose.Text = "Close";
            this.PushClose.UseVisualStyleBackColor = false;
            this.PushClose.Click += new System.EventHandler(this.PushClose_Click);
            // 
            // PushUninit
            // 
            this.PushUninit.BackColor = System.Drawing.SystemColors.Control;
            this.PushUninit.Cursor = System.Windows.Forms.Cursors.Default;
            this.PushUninit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.PushUninit.Location = new System.Drawing.Point(14, 525);
            this.PushUninit.Name = "PushUninit";
            this.PushUninit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.PushUninit.Size = new System.Drawing.Size(113, 33);
            this.PushUninit.TabIndex = 18;
            this.PushUninit.Text = "Uninit";
            this.PushUninit.UseVisualStyleBackColor = false;
            // 
            // comboTriggerSource
            // 
            this.comboTriggerSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTriggerSource.FormattingEnabled = true;
            this.comboTriggerSource.Location = new System.Drawing.Point(678, 190);
            this.comboTriggerSource.Name = "comboTriggerSource";
            this.comboTriggerSource.Size = new System.Drawing.Size(150, 21);
            this.comboTriggerSource.TabIndex = 29;
            this.comboTriggerSource.SelectedIndexChanged += new System.EventHandler(this.comboTriggerSource_SelectedIndexChanged);
            // 
            // QueryFrameCountButton
            // 
            this.QueryFrameCountButton.Location = new System.Drawing.Point(505, 128);
            this.QueryFrameCountButton.Name = "QueryFrameCountButton";
            this.QueryFrameCountButton.Size = new System.Drawing.Size(150, 23);
            this.QueryFrameCountButton.TabIndex = 29;
            this.QueryFrameCountButton.Text = "Query Frame Count";
            this.QueryFrameCountButton.UseVisualStyleBackColor = true;
            this.QueryFrameCountButton.Click += new System.EventHandler(this.QueryFrameCountButton_Click);
            // 
            // UpdateFrameCountButton
            // 
            this.UpdateFrameCountButton.Location = new System.Drawing.Point(505, 157);
            this.UpdateFrameCountButton.Name = "UpdateFrameCountButton";
            this.UpdateFrameCountButton.Size = new System.Drawing.Size(150, 23);
            this.UpdateFrameCountButton.TabIndex = 30;
            this.UpdateFrameCountButton.Text = "Update Frame Count";
            this.UpdateFrameCountButton.UseVisualStyleBackColor = true;
            this.UpdateFrameCountButton.Click += new System.EventHandler(this.UpdateFrameCountButton_Click);
            // 
            // FrameCountTextBox
            // 
            this.FrameCountTextBox.Location = new System.Drawing.Point(678, 156);
            this.FrameCountTextBox.Name = "FrameCountTextBox";
            this.FrameCountTextBox.Size = new System.Drawing.Size(100, 20);
            this.FrameCountTextBox.TabIndex = 31;
            this.FrameCountTextBox.Text = "4";
            // 
            // FrameCountLabel
            // 
            this.FrameCountLabel.AutoSize = true;
            this.FrameCountLabel.Location = new System.Drawing.Point(675, 133);
            this.FrameCountLabel.Name = "FrameCountLabel";
            this.FrameCountLabel.Size = new System.Drawing.Size(130, 13);
            this.FrameCountLabel.TabIndex = 32;
            this.FrameCountLabel.Text = "Current Frame Count: N/A";
            // 
            // TriggerSourceLabel
            // 
            this.TriggerSourceLabel.AutoSize = true;
            this.TriggerSourceLabel.Location = new System.Drawing.Point(538, 193);
            this.TriggerSourceLabel.Name = "TriggerSourceLabel";
            this.TriggerSourceLabel.Size = new System.Drawing.Size(117, 13);
            this.TriggerSourceLabel.TabIndex = 33;
            this.TriggerSourceLabel.Text = "Current Trigger Source:";
            // 
            // SetSaveDirectoryButton
            // 
            this.SetSaveDirectoryButton.Location = new System.Drawing.Point(598, 12);
            this.SetSaveDirectoryButton.Name = "SetSaveDirectoryButton";
            this.SetSaveDirectoryButton.Size = new System.Drawing.Size(180, 30);
            this.SetSaveDirectoryButton.TabIndex = 34;
            this.SetSaveDirectoryButton.Text = "Set Save Directory";
            this.SetSaveDirectoryButton.UseVisualStyleBackColor = true;
            this.SetSaveDirectoryButton.Click += new System.EventHandler(this.SetSaveDirectoryButton_Click);
            // 
            // SaveDirectoryLabel
            // 
            this.SaveDirectoryLabel.AutoSize = true;
            this.SaveDirectoryLabel.Location = new System.Drawing.Point(595, 51);
            this.SaveDirectoryLabel.MaximumSize = new System.Drawing.Size(400, 0);
            this.SaveDirectoryLabel.Name = "SaveDirectoryLabel";
            this.SaveDirectoryLabel.Size = new System.Drawing.Size(312, 39);
            this.SaveDirectoryLabel.TabIndex = 35;
            this.SaveDirectoryLabel.Text = "Current Save Directory: \nE:\\Imperial College London\\OneDrive - Imperial College L" +
    "ondon\\\nDocuments - Team ultracold - PH\\Data\\2025\\CCD data";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(911, 573);
            this.Controls.Add(this.LabelStatus);
            this.Controls.Add(this.LabelLutMin);
            this.Controls.Add(this.LabelLutMax);
            this.Controls.Add(this.EditLutMin);
            this.Controls.Add(this.EditLutMax);
            this.Controls.Add(this.PushAsterisk);
            this.Controls.Add(this.HScrollLutMin);
            this.Controls.Add(this.HScrollLutMax);
            this.Controls.Add(this.PicDisplay);
            this.Controls.Add(this.PushInit);
            this.Controls.Add(this.PushSnap);
            this.Controls.Add(this.PushLive);
            this.Controls.Add(this.PushFireTrigger);
            this.Controls.Add(this.PushIdle);
            this.Controls.Add(this.PushBufRelease);
            this.Controls.Add(this.PushClose);
            this.Controls.Add(this.PushUninit);
            this.Controls.Add(this.SensitivityGainLabel);
            this.Controls.Add(this.ExposureTimeLabel);
            this.Controls.Add(this.QuerySensitivityGainButton);
            this.Controls.Add(this.UpdateSensitivityGainButton);
            this.Controls.Add(this.QueryExposureTimeButton);
            this.Controls.Add(this.UpdateExposureTimeButton);
            this.Controls.Add(this.SensitivityGainTextBox);
            this.Controls.Add(this.ExposureTimeTextBox);
            this.Controls.Add(this.QuerySensorTemperatureButton);
            this.Controls.Add(this.SensorTemperatureLabel);
            this.Controls.Add(this.comboTriggerSource);
            this.Controls.Add(this.QueryFrameCountButton);
            this.Controls.Add(this.UpdateFrameCountButton);
            this.Controls.Add(this.FrameCountTextBox);
            this.Controls.Add(this.FrameCountLabel);
            this.Controls.Add(this.TriggerSourceLabel);
            this.Controls.Add(this.SaveDirectoryLabel);
            this.Controls.Add(this.SetSaveDirectoryButton);
            this.Name = "FormMain";
            this.Text = "Hamamatsu EMCCD Camera Automation";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label LabelStatus;
        public System.Windows.Forms.Label LabelLutMin;
        public System.Windows.Forms.Label LabelLutMax;
        public System.Windows.Forms.TextBox EditLutMin;
        public System.Windows.Forms.TextBox EditLutMax;
        public System.Windows.Forms.Button PushAsterisk;
        public System.Windows.Forms.HScrollBar HScrollLutMin;
        public System.Windows.Forms.HScrollBar HScrollLutMax;
        public System.Windows.Forms.PictureBox PicDisplay;
        public System.Windows.Forms.Button PushInit;
        //public System.Windows.Forms.Button PushOpen;
        //public System.Windows.Forms.Button PushInfo;
        //public System.Windows.Forms.Button PushProperties;
        public System.Windows.Forms.Button PushSnap;
        public System.Windows.Forms.Button PushLive;
        public System.Windows.Forms.Button PushFireTrigger;
        public System.Windows.Forms.Button PushIdle;
        public System.Windows.Forms.Button PushBufRelease;
        public System.Windows.Forms.Button PushClose;
        public System.Windows.Forms.Button PushUninit;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.Label SensitivityGainLabel;
        public System.Windows.Forms.Label ExposureTimeLabel;
        public System.Windows.Forms.Button QuerySensitivityGainButton;
        public System.Windows.Forms.Button UpdateSensitivityGainButton;
        public System.Windows.Forms.Button QueryExposureTimeButton;
        public System.Windows.Forms.Button UpdateExposureTimeButton;
        public System.Windows.Forms.TextBox SensitivityGainTextBox;
        public System.Windows.Forms.TextBox ExposureTimeTextBox;
        public System.Windows.Forms.Button QuerySensorTemperatureButton;
        public System.Windows.Forms.Label SensorTemperatureLabel;
        public System.Windows.Forms.ComboBox comboTriggerSource;
        public System.Windows.Forms.Button QueryFrameCountButton;
        public System.Windows.Forms.Button UpdateFrameCountButton;
        public System.Windows.Forms.TextBox FrameCountTextBox;
        public System.Windows.Forms.Label FrameCountLabel;
        public System.Windows.Forms.Label TriggerSourceLabel;
        public System.Windows.Forms.Button SetSaveDirectoryButton;
        public System.Windows.Forms.Label SaveDirectoryLabel;
    }
}