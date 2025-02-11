namespace csAcq4
{
    partial class FormInfo
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
            this.DataGridViewInfo = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // DataGridViewInfo
            // 
            this.DataGridViewInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridViewInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGridViewInfo.Location = new System.Drawing.Point(0, 0);
            this.DataGridViewInfo.Name = "DataGridViewInfo";
            this.DataGridViewInfo.Size = new System.Drawing.Size(284, 262);
            this.DataGridViewInfo.TabIndex = 0;
            // 
            // FormInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.DataGridViewInfo);
            this.Name = "FormInfo";
            this.Text = "FormInfo";
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewInfo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView DataGridViewInfo;
    }
}