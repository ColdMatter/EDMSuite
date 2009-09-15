namespace TransferCavityLock
{
    partial class MainForm
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
            this.voltageRampControl = new System.Windows.Forms.GroupBox();
            this.rampLED = new NationalInstruments.UI.WindowsForms.Led();
            this.rampStopButton = new System.Windows.Forms.Button();
            this.vRampExtButton = new System.Windows.Forms.RadioButton();
            this.vRampIntButton = new System.Windows.Forms.RadioButton();
            this.rampChannelMenu = new System.Windows.Forms.ComboBox();
            this.rampStartButton = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.voltageRampControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();
            this.SuspendLayout();
            // 
            // voltageRampControl
            // 
            this.voltageRampControl.Controls.Add(this.rampLED);
            this.voltageRampControl.Controls.Add(this.rampStopButton);
            this.voltageRampControl.Controls.Add(this.vRampExtButton);
            this.voltageRampControl.Controls.Add(this.vRampIntButton);
            this.voltageRampControl.Controls.Add(this.rampChannelMenu);
            this.voltageRampControl.Controls.Add(this.rampStartButton);
            this.voltageRampControl.Location = new System.Drawing.Point(517, 12);
            this.voltageRampControl.Name = "voltageRampControl";
            this.voltageRampControl.Size = new System.Drawing.Size(234, 247);
            this.voltageRampControl.TabIndex = 2;
            this.voltageRampControl.TabStop = false;
            this.voltageRampControl.Text = "Voltage Ramp";
            this.voltageRampControl.Enter += new System.EventHandler(this.voltageRampControl_Enter);
            // 
            // rampLED
            // 
            this.rampLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.rampLED.Location = new System.Drawing.Point(197, 212);
            this.rampLED.Name = "rampLED";
            this.rampLED.OffColor = System.Drawing.Color.Red;
            this.rampLED.Size = new System.Drawing.Size(31, 29);
            this.rampLED.TabIndex = 7;
            // 
            // rampStopButton
            // 
            this.rampStopButton.Location = new System.Drawing.Point(6, 46);
            this.rampStopButton.Name = "rampStopButton";
            this.rampStopButton.Size = new System.Drawing.Size(111, 23);
            this.rampStopButton.TabIndex = 6;
            this.rampStopButton.Text = "Stop ramping";
            this.rampStopButton.UseVisualStyleBackColor = true;
            this.rampStopButton.Click += new System.EventHandler(this.rampStopButton_Click);
            // 
            // vRampExtButton
            // 
            this.vRampExtButton.AutoSize = true;
            this.vRampExtButton.Location = new System.Drawing.Point(136, 49);
            this.vRampExtButton.Name = "vRampExtButton";
            this.vRampExtButton.Size = new System.Drawing.Size(75, 17);
            this.vRampExtButton.TabIndex = 5;
            this.vRampExtButton.TabStop = true;
            this.vRampExtButton.Text = "Ext. trigger";
            this.vRampExtButton.UseVisualStyleBackColor = true;
            this.vRampExtButton.CheckedChanged += new System.EventHandler(this.vRampExtButton_CheckedChanged);
            // 
            // vRampIntButton
            // 
            this.vRampIntButton.AutoSize = true;
            this.vRampIntButton.Location = new System.Drawing.Point(136, 19);
            this.vRampIntButton.Name = "vRampIntButton";
            this.vRampIntButton.Size = new System.Drawing.Size(93, 17);
            this.vRampIntButton.TabIndex = 4;
            this.vRampIntButton.TabStop = true;
            this.vRampIntButton.Text = "60Hz (internal)";
            this.vRampIntButton.UseVisualStyleBackColor = true;
            this.vRampIntButton.CheckedChanged += new System.EventHandler(this.vRampIntButton_CheckedChanged);
            // 
            // rampChannelMenu
            // 
            this.rampChannelMenu.FormattingEnabled = true;
            this.rampChannelMenu.Items.AddRange(new object[] {
            "laser",
            "cavity"});
            this.rampChannelMenu.Location = new System.Drawing.Point(6, 79);
            this.rampChannelMenu.MaxDropDownItems = 2;
            this.rampChannelMenu.Name = "rampChannelMenu";
            this.rampChannelMenu.Size = new System.Drawing.Size(121, 21);
            this.rampChannelMenu.TabIndex = 3;
            this.rampChannelMenu.Text = "Select Channel";
            this.rampChannelMenu.SelectedIndexChanged += new System.EventHandler(this.rampChannelMenu_SelectedIndexChanged);
            // 
            // rampStartButton
            // 
            this.rampStartButton.Location = new System.Drawing.Point(6, 19);
            this.rampStartButton.Name = "rampStartButton";
            this.rampStartButton.Size = new System.Drawing.Size(111, 23);
            this.rampStartButton.TabIndex = 2;
            this.rampStartButton.Text = "Start ramping";
            this.rampStartButton.UseVisualStyleBackColor = true;
            this.rampStartButton.Click += new System.EventHandler(this.rampStartButton_Click);
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(12, 239);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(499, 20);
            this.textBox.TabIndex = 3;
            this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 271);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.voltageRampControl);
            this.Name = "MainForm";
            this.Text = "Transfer Cavity Lock";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.voltageRampControl.ResumeLayout(false);
            this.voltageRampControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox voltageRampControl;
        private System.Windows.Forms.Button rampStartButton;
        private System.Windows.Forms.ComboBox rampChannelMenu;
        private System.Windows.Forms.RadioButton vRampExtButton;
        private System.Windows.Forms.RadioButton vRampIntButton;
        private System.Windows.Forms.Button rampStopButton;
        private NationalInstruments.UI.WindowsForms.Led rampLED;
        private System.Windows.Forms.TextBox textBox;
    }
}

