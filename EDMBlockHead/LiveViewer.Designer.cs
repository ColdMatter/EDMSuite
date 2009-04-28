namespace EDMBlockHead
{
    partial class LiveViewer
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
            this.statusText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.clusterStatusText = new System.Windows.Forms.TextBox();
            this.resetRunningMeans = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // statusText
            // 
            this.statusText.BackColor = System.Drawing.Color.Black;
            this.statusText.ForeColor = System.Drawing.Color.Lime;
            this.statusText.Location = new System.Drawing.Point(12, 54);
            this.statusText.Multiline = true;
            this.statusText.Name = "statusText";
            this.statusText.ReadOnly = true;
            this.statusText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.statusText.Size = new System.Drawing.Size(431, 316);
            this.statusText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Block Analysis";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 394);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Cluster Information";
            // 
            // clusterStatusText
            // 
            this.clusterStatusText.BackColor = System.Drawing.Color.Black;
            this.clusterStatusText.ForeColor = System.Drawing.Color.Lime;
            this.clusterStatusText.Location = new System.Drawing.Point(12, 419);
            this.clusterStatusText.Multiline = true;
            this.clusterStatusText.Name = "clusterStatusText";
            this.clusterStatusText.ReadOnly = true;
            this.clusterStatusText.Size = new System.Drawing.Size(340, 43);
            this.clusterStatusText.TabIndex = 3;
            // 
            // resetRunningMeans
            // 
            this.resetRunningMeans.Location = new System.Drawing.Point(359, 419);
            this.resetRunningMeans.Name = "resetRunningMeans";
            this.resetRunningMeans.Size = new System.Drawing.Size(84, 24);
            this.resetRunningMeans.TabIndex = 4;
            this.resetRunningMeans.Text = "Reset means";
            this.resetRunningMeans.UseVisualStyleBackColor = true;
            this.resetRunningMeans.Click += new System.EventHandler(this.resetRunningMeans_Click);
            // 
            // LiveViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 528);
            this.Controls.Add(this.resetRunningMeans);
            this.Controls.Add(this.clusterStatusText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusText);
            this.MaximizeBox = false;
            this.Name = "LiveViewer";
            this.Text = "LiveViewer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox statusText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox clusterStatusText;
        private System.Windows.Forms.Button resetRunningMeans;

    }
}