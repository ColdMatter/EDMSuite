
namespace AlFHardwareControl
{
    partial class ShutterSet
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.shutterName = new System.Windows.Forms.TextBox();
            this.Open = new System.Windows.Forms.Button();
            this.Close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // shutterName
            // 
            this.shutterName.Enabled = false;
            this.shutterName.Location = new System.Drawing.Point(3, 3);
            this.shutterName.Name = "shutterName";
            this.shutterName.Size = new System.Drawing.Size(104, 20);
            this.shutterName.TabIndex = 0;
            // 
            // Open
            // 
            this.Open.Location = new System.Drawing.Point(113, 1);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(39, 23);
            this.Open.TabIndex = 1;
            this.Open.Text = "+";
            this.Open.UseVisualStyleBackColor = true;
            // 
            // Close
            // 
            this.Close.Location = new System.Drawing.Point(158, 1);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(39, 23);
            this.Close.TabIndex = 2;
            this.Close.Text = "-";
            this.Close.UseVisualStyleBackColor = true;
            // 
            // ShutterSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Close);
            this.Controls.Add(this.Open);
            this.Controls.Add(this.shutterName);
            this.Name = "ShutterSet";
            this.Size = new System.Drawing.Size(200, 26);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox shutterName;
        private System.Windows.Forms.Button Open;
        private System.Windows.Forms.Button Close;
    }
}
