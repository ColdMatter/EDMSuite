
namespace AlFHardwareControl
{
    partial class DictionaryView
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
            this.DictionaryData = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DictionaryData)).BeginInit();
            this.SuspendLayout();
            // 
            // DictionaryData
            // 
            this.DictionaryData.AllowUserToAddRows = false;
            this.DictionaryData.AllowUserToDeleteRows = false;
            this.DictionaryData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DictionaryData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.DictionaryData.Location = new System.Drawing.Point(12, 12);
            this.DictionaryData.Name = "DictionaryData";
            this.DictionaryData.Size = new System.Drawing.Size(337, 277);
            this.DictionaryData.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Value";
            this.dataGridViewTextBoxColumn1.HeaderText = "Value";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // DictionaryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 301);
            this.Controls.Add(this.DictionaryData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DictionaryView";
            this.Text = "DictionaryView";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DictionaryView_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.DictionaryData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DataGridView DictionaryData;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    }
}