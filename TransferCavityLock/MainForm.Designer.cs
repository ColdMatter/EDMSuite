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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.voltageRampControl = new System.Windows.Forms.GroupBox();
            this.rampButton = new System.Windows.Forms.Button();
            this.rampChannelMenu = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            this.voltageRampControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.controlToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(763, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // controlToolStripMenuItem
            // 
            this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.parkToolStripMenuItem,
            this.lockToolStripMenuItem});
            this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
            this.controlToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.controlToolStripMenuItem.Text = "Control";
            // 
            // parkToolStripMenuItem
            // 
            this.parkToolStripMenuItem.Name = "parkToolStripMenuItem";
            this.parkToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.parkToolStripMenuItem.Text = "Park";
            // 
            // lockToolStripMenuItem
            // 
            this.lockToolStripMenuItem.Name = "lockToolStripMenuItem";
            this.lockToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.lockToolStripMenuItem.Text = "Lock";
            // 
            // voltageRampControl
            // 
            this.voltageRampControl.Controls.Add(this.rampChannelMenu);
            this.voltageRampControl.Controls.Add(this.rampButton);
            this.voltageRampControl.Location = new System.Drawing.Point(547, 35);
            this.voltageRampControl.Name = "voltageRampControl";
            this.voltageRampControl.Size = new System.Drawing.Size(204, 224);
            this.voltageRampControl.TabIndex = 2;
            this.voltageRampControl.TabStop = false;
            this.voltageRampControl.Text = "Voltage Ramp Menu";
            // 
            // rampButton
            // 
            this.rampButton.Location = new System.Drawing.Point(48, 19);
            this.rampButton.Name = "rampButton";
            this.rampButton.Size = new System.Drawing.Size(111, 23);
            this.rampButton.TabIndex = 2;
            this.rampButton.Text = "ramp voltage";
            this.rampButton.UseVisualStyleBackColor = true;
            // 
            // rampChannelMenu
            // 
            this.rampChannelMenu.FormattingEnabled = true;
            this.rampChannelMenu.Items.AddRange(new object[] {
            "laser",
            "cavity"});
            this.rampChannelMenu.Location = new System.Drawing.Point(48, 48);
            this.rampChannelMenu.MaxDropDownItems = 2;
            this.rampChannelMenu.Name = "rampChannelMenu";
            this.rampChannelMenu.Size = new System.Drawing.Size(121, 21);
            this.rampChannelMenu.TabIndex = 3;
            this.rampChannelMenu.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 271);
            this.Controls.Add(this.voltageRampControl);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Transfer Cavity Lock";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.voltageRampControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem controlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lockToolStripMenuItem;
        private System.Windows.Forms.GroupBox voltageRampControl;
        private System.Windows.Forms.Button rampButton;
        private System.Windows.Forms.ComboBox rampChannelMenu;
    }
}

