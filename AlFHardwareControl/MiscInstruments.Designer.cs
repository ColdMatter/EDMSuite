
namespace AlFHardwareControl
{
    partial class MiscInstruments
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
            this.ShutterControl = new AlFHardwareControl.Shutter_Control();
            this.YAG_Control = new AlFHardwareControl.YAG_Control();
            this.SuspendLayout();
            // 
            // ShutterControl
            // 
            this.ShutterControl.Location = new System.Drawing.Point(228, 0);
            this.ShutterControl.Name = "ShutterControl";
            this.ShutterControl.Size = new System.Drawing.Size(222, 411);
            this.ShutterControl.TabIndex = 1;
            // 
            // YAG_Control
            // 
            this.YAG_Control.Location = new System.Drawing.Point(0, 0);
            this.YAG_Control.Name = "YAG_Control";
            this.YAG_Control.Size = new System.Drawing.Size(222, 411);
            this.YAG_Control.TabIndex = 0;
            // 
            // MiscInstruments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ShutterControl);
            this.Controls.Add(this.YAG_Control);
            this.Name = "MiscInstruments";
            this.Size = new System.Drawing.Size(1180, 411);
            this.ResumeLayout(false);

        }

        #endregion

        public YAG_Control YAG_Control;
        public Shutter_Control ShutterControl;
    }
}
