namespace MicrocavityHardwareControl
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
            this.uCavityReflectionECDL = new System.Windows.Forms.TextBox();
            this.updateButton = new System.Windows.Forms.Button();
            this.uCavityReflectionECDLLabel = new System.Windows.Forms.Label();
            this.uCavityReflectionTiSapphLabel = new System.Windows.Forms.Label();
            this.uCavityReflectionTiSapph = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uCavityReflectionECDL
            // 
            this.uCavityReflectionECDL.Location = new System.Drawing.Point(244, 52);
            this.uCavityReflectionECDL.Name = "uCavityReflectionECDL";
            this.uCavityReflectionECDL.Size = new System.Drawing.Size(76, 20);
            this.uCavityReflectionECDL.TabIndex = 0;
            this.uCavityReflectionECDL.TextChanged += new System.EventHandler(this.uCavityReflectionECDL_TextChanged);
            // 
            // updateButton
            // 
            this.updateButton.Location = new System.Drawing.Point(385, 55);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(59, 34);
            this.updateButton.TabIndex = 1;
            this.updateButton.Text = "Update";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // uCavityReflectionECDLLabel
            // 
            this.uCavityReflectionECDLLabel.AutoSize = true;
            this.uCavityReflectionECDLLabel.Location = new System.Drawing.Point(114, 55);
            this.uCavityReflectionECDLLabel.Name = "uCavityReflectionECDLLabel";
            this.uCavityReflectionECDLLabel.Size = new System.Drawing.Size(124, 13);
            this.uCavityReflectionECDLLabel.TabIndex = 2;
            this.uCavityReflectionECDLLabel.Text = "ECDL uCavity Reflection";
            this.uCavityReflectionECDLLabel.Click += new System.EventHandler(this.uCavityReflectionECDLLabel_Click);
            // 
            // uCavityReflectionTiSapphLabel
            // 
            this.uCavityReflectionTiSapphLabel.AutoSize = true;
            this.uCavityReflectionTiSapphLabel.Location = new System.Drawing.Point(102, 81);
            this.uCavityReflectionTiSapphLabel.Name = "uCavityReflectionTiSapphLabel";
            this.uCavityReflectionTiSapphLabel.Size = new System.Drawing.Size(136, 13);
            this.uCavityReflectionTiSapphLabel.TabIndex = 4;
            this.uCavityReflectionTiSapphLabel.Text = "TiSapph uCavity Reflection";
            this.uCavityReflectionTiSapphLabel.Click += new System.EventHandler(this.uCavityReflectionTiSapphLabel_Click);
            // 
            // uCavityReflectionTiSapph
            // 
            this.uCavityReflectionTiSapph.Location = new System.Drawing.Point(244, 78);
            this.uCavityReflectionTiSapph.Name = "uCavityReflectionTiSapph";
            this.uCavityReflectionTiSapph.Size = new System.Drawing.Size(76, 20);
            this.uCavityReflectionTiSapph.TabIndex = 3;
            this.uCavityReflectionTiSapph.TextChanged += new System.EventHandler(this.uCavityReflectionTiSapph_TextChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(471, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 262);
            this.Controls.Add(this.uCavityReflectionTiSapphLabel);
            this.Controls.Add(this.uCavityReflectionTiSapph);
            this.Controls.Add(this.uCavityReflectionECDLLabel);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.uCavityReflectionECDL);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ControlWindow";
            this.Text = "Microcavity Control";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Label uCavityReflectionECDLLabel;
        public System.Windows.Forms.TextBox uCavityReflectionECDL;
        private System.Windows.Forms.Label uCavityReflectionTiSapphLabel;
        public System.Windows.Forms.TextBox uCavityReflectionTiSapph;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}