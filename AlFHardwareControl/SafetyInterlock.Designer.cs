
namespace AlFHardwareControl
{
    partial class SafetyInterlock
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
            this.BoundingBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BoundingBox
            // 
            this.BoundingBox.Enabled = false;
            this.BoundingBox.Text = "Safety Interlock";
            // 
            // DiscardTimedSchedOnInterlockFail
            // 
            this.DiscardTimedSchedOnInterlockFail.Location = new System.Drawing.Point(209, 86);
            // 
            // Dismiss
            // 
            this.Dismiss.Enabled = false;
            this.Dismiss.Location = new System.Drawing.Point(3, 82);
            // 
            // SafetyInterlock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "SafetyInterlock";
            this.BoundingBox.ResumeLayout(false);
            this.BoundingBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
