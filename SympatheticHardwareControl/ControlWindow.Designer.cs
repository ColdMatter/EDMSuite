namespace SympatheticHardwareControl
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
            this.MainTabs = new System.Windows.Forms.TabControl();
            this.pressureTab = new System.Windows.Forms.TabPage();
            this.pressureLabel1 = new System.Windows.Forms.Label();
            this.pressureIndicator1 = new System.Windows.Forms.TextBox();
            this.fieldTab = new System.Windows.Forms.TabPage();
            this.pressureIndicator2 = new System.Windows.Forms.TextBox();
            this.pressureLabel2 = new System.Windows.Forms.Label();
            this.MainTabs.SuspendLayout();
            this.pressureTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabs
            // 
            this.MainTabs.Controls.Add(this.pressureTab);
            this.MainTabs.Controls.Add(this.fieldTab);
            this.MainTabs.Location = new System.Drawing.Point(1, 2);
            this.MainTabs.Name = "MainTabs";
            this.MainTabs.SelectedIndex = 0;
            this.MainTabs.Size = new System.Drawing.Size(288, 264);
            this.MainTabs.TabIndex = 0;
            // 
            // pressureTab
            // 
            this.pressureTab.Controls.Add(this.pressureLabel2);
            this.pressureTab.Controls.Add(this.pressureIndicator2);
            this.pressureTab.Controls.Add(this.pressureLabel1);
            this.pressureTab.Controls.Add(this.pressureIndicator1);
            this.pressureTab.Location = new System.Drawing.Point(4, 22);
            this.pressureTab.Name = "pressureTab";
            this.pressureTab.Padding = new System.Windows.Forms.Padding(3);
            this.pressureTab.Size = new System.Drawing.Size(280, 238);
            this.pressureTab.TabIndex = 0;
            this.pressureTab.Text = "Pressures";
            this.pressureTab.UseVisualStyleBackColor = true;
            // 
            // pressureLabel1
            // 
            this.pressureLabel1.AutoSize = true;
            this.pressureLabel1.Location = new System.Drawing.Point(39, 30);
            this.pressureLabel1.Name = "pressureLabel1";
            this.pressureLabel1.Size = new System.Drawing.Size(52, 13);
            this.pressureLabel1.TabIndex = 1;
            this.pressureLabel1.Text = "P1 (mbar)";
            this.pressureLabel1.Click += new System.EventHandler(this.pressureLabel1_Click);
            // 
            // pressureIndicator1
            // 
            this.pressureIndicator1.BackColor = System.Drawing.Color.Black;
            this.pressureIndicator1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pressureIndicator1.ForeColor = System.Drawing.Color.YellowGreen;
            this.pressureIndicator1.Location = new System.Drawing.Point(97, 27);
            this.pressureIndicator1.Name = "pressureIndicator1";
            this.pressureIndicator1.ReadOnly = true;
            this.pressureIndicator1.Size = new System.Drawing.Size(100, 26);
            this.pressureIndicator1.TabIndex = 0;
            // 
            // fieldTab
            // 
            this.fieldTab.Location = new System.Drawing.Point(4, 22);
            this.fieldTab.Name = "fieldTab";
            this.fieldTab.Padding = new System.Windows.Forms.Padding(3);
            this.fieldTab.Size = new System.Drawing.Size(280, 238);
            this.fieldTab.TabIndex = 1;
            this.fieldTab.Text = "Fields";
            this.fieldTab.UseVisualStyleBackColor = true;
            // 
            // pressureIndicator2
            // 
            this.pressureIndicator2.BackColor = System.Drawing.Color.Black;
            this.pressureIndicator2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pressureIndicator2.ForeColor = System.Drawing.Color.YellowGreen;
            this.pressureIndicator2.Location = new System.Drawing.Point(97, 74);
            this.pressureIndicator2.Name = "pressureIndicator2";
            this.pressureIndicator2.Size = new System.Drawing.Size(100, 26);
            this.pressureIndicator2.TabIndex = 2;
            // 
            // pressureLabel2
            // 
            this.pressureLabel2.AutoSize = true;
            this.pressureLabel2.Location = new System.Drawing.Point(42, 74);
            this.pressureLabel2.Name = "pressureLabel2";
            this.pressureLabel2.Size = new System.Drawing.Size(52, 13);
            this.pressureLabel2.TabIndex = 3;
            this.pressureLabel2.Text = "P2 (mbar)";
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.MainTabs);
            this.Name = "ControlWindow";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            this.MainTabs.ResumeLayout(false);
            this.pressureTab.ResumeLayout(false);
            this.pressureTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabs;
        private System.Windows.Forms.TabPage pressureTab;
        private System.Windows.Forms.TabPage fieldTab;
        private System.Windows.Forms.Label pressureLabel1;
        private System.Windows.Forms.Label pressureLabel2;
        public System.Windows.Forms.TextBox pressureIndicator1;
        public System.Windows.Forms.TextBox pressureIndicator2;
    }
}

