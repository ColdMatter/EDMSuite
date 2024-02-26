
namespace AlFHardwareControl
{
    partial class Shutter_Control
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
            this.Shutter_Control_Group = new System.Windows.Forms.GroupBox();
            this.ShutterPanel = new System.Windows.Forms.Panel();
            this.Shutter_Control_Group.SuspendLayout();
            this.SuspendLayout();
            // 
            // Shutter_Control_Group
            // 
            this.Shutter_Control_Group.Controls.Add(this.ShutterPanel);
            this.Shutter_Control_Group.Location = new System.Drawing.Point(4, 4);
            this.Shutter_Control_Group.Name = "Shutter_Control_Group";
            this.Shutter_Control_Group.Size = new System.Drawing.Size(214, 404);
            this.Shutter_Control_Group.TabIndex = 0;
            this.Shutter_Control_Group.TabStop = false;
            this.Shutter_Control_Group.Text = "Shutter Control";
            // 
            // ShutterPanel
            // 
            this.ShutterPanel.Location = new System.Drawing.Point(7, 20);
            this.ShutterPanel.Name = "ShutterPanel";
            this.ShutterPanel.Size = new System.Drawing.Size(200, 378);
            this.ShutterPanel.TabIndex = 0;
            // 
            // Shutter_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Shutter_Control_Group);
            this.Name = "Shutter_Control";
            this.Size = new System.Drawing.Size(222, 411);
            this.Shutter_Control_Group.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox Shutter_Control_Group;
        private System.Windows.Forms.Panel ShutterPanel;
    }
}
