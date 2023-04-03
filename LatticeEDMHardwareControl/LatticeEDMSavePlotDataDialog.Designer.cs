namespace LatticeEDMHardwareControl
{
    partial class LatticeEDMSavePlotDataDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LatticeEDMSavePlotDataDialog));
            this.MessageDescription = new System.Windows.Forms.RichTextBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.btSave = new System.Windows.Forms.Button();
            this.btDoNotSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // MessageDescription
            // 
            this.MessageDescription.BackColor = System.Drawing.SystemColors.Menu;
            this.MessageDescription.Location = new System.Drawing.Point(12, 12);
            this.MessageDescription.Name = "MessageDescription";
            this.MessageDescription.ReadOnly = true;
            this.MessageDescription.Size = new System.Drawing.Size(486, 88);
            this.MessageDescription.TabIndex = 0;
            this.MessageDescription.Text = "";
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(423, 106);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 1;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(261, 106);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(75, 23);
            this.btSave.TabIndex = 2;
            this.btSave.Text = "Save";
            this.btSave.UseVisualStyleBackColor = true;
            // 
            // btDoNotSave
            // 
            this.btDoNotSave.Location = new System.Drawing.Point(342, 106);
            this.btDoNotSave.Name = "btDoNotSave";
            this.btDoNotSave.Size = new System.Drawing.Size(75, 23);
            this.btDoNotSave.TabIndex = 3;
            this.btDoNotSave.Text = "Don\'t save";
            this.btDoNotSave.UseVisualStyleBackColor = true;
            // 
            // UEDMSavePlotDataDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 136);
            this.Controls.Add(this.btDoNotSave);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.MessageDescription);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LatticeEDMSavePlotDataDialog";
            this.Text = "windowTitleText";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox MessageDescription;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btDoNotSave;
    }
}