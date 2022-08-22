
namespace WavemeterLock
{
    partial class LockForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LockForm));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lockTab = new System.Windows.Forms.TabControl();
            this.masterBttn = new System.Windows.Forms.Button();
            this.wmlLED = new NationalInstruments.UI.WindowsForms.Led();
            ((System.ComponentModel.ISupportInitialize)(this.wmlLED)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lockTab
            // 
            this.lockTab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lockTab.Location = new System.Drawing.Point(-3, 112);
            this.lockTab.Name = "lockTab";
            this.lockTab.SelectedIndex = 0;
            this.lockTab.Size = new System.Drawing.Size(683, 365);
            this.lockTab.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.lockTab.TabIndex = 2;
            this.lockTab.SelectedIndexChanged += new System.EventHandler(this.lockTab_SelectedIndexChanged);
            // 
            // masterBttn
            // 
            this.masterBttn.Location = new System.Drawing.Point(37, 38);
            this.masterBttn.Name = "masterBttn";
            this.masterBttn.Size = new System.Drawing.Size(75, 23);
            this.masterBttn.TabIndex = 3;
            this.masterBttn.Text = "masterBttn";
            this.masterBttn.UseVisualStyleBackColor = true;
            this.masterBttn.Click += new System.EventHandler(this.masterBttn_Click);
            // 
            // wmlLED
            // 
            this.wmlLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.wmlLED.Location = new System.Drawing.Point(128, 38);
            this.wmlLED.Name = "wmlLED";
            this.wmlLED.OffColor = System.Drawing.Color.Crimson;
            this.wmlLED.Size = new System.Drawing.Size(23, 23);
            this.wmlLED.TabIndex = 5;
            // 
            // LockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 478);
            this.Controls.Add(this.wmlLED);
            this.Controls.Add(this.masterBttn);
            this.Controls.Add(this.lockTab);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LockForm";
            this.Text = "Wavemeter Lock";
            this.Load += new System.EventHandler(this.LockForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wmlLED)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabControl lockTab;
        private System.Windows.Forms.Button masterBttn;
        private NationalInstruments.UI.WindowsForms.Led wmlLED;
    }
}

