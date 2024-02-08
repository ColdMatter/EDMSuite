
namespace AlFHardwareControl
{
    partial class MacroEvent
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
            this.BoundingBox = new System.Windows.Forms.GroupBox();
            this.MacroName = new System.Windows.Forms.TextBox();
            this.ActiveTasks = new System.Windows.Forms.ListBox();
            this.Dismiss = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Repeat = new System.Windows.Forms.CheckBox();
            this.BoundingBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BoundingBox
            // 
            this.BoundingBox.Controls.Add(this.Repeat);
            this.BoundingBox.Controls.Add(this.MacroName);
            this.BoundingBox.Controls.Add(this.ActiveTasks);
            this.BoundingBox.Controls.Add(this.Dismiss);
            this.BoundingBox.Controls.Add(this.label2);
            this.BoundingBox.Controls.Add(this.label1);
            this.BoundingBox.Location = new System.Drawing.Point(0, 0);
            this.BoundingBox.Name = "BoundingBox";
            this.BoundingBox.Size = new System.Drawing.Size(372, 111);
            this.BoundingBox.TabIndex = 8;
            this.BoundingBox.TabStop = false;
            this.BoundingBox.Text = "Macro";
            // 
            // MacroName
            // 
            this.MacroName.Enabled = false;
            this.MacroName.Location = new System.Drawing.Point(195, 16);
            this.MacroName.Name = "MacroName";
            this.MacroName.Size = new System.Drawing.Size(171, 20);
            this.MacroName.TabIndex = 10;
            // 
            // ActiveTasks
            // 
            this.ActiveTasks.Enabled = false;
            this.ActiveTasks.FormattingEnabled = true;
            this.ActiveTasks.Location = new System.Drawing.Point(195, 42);
            this.ActiveTasks.Name = "ActiveTasks";
            this.ActiveTasks.Size = new System.Drawing.Size(171, 56);
            this.ActiveTasks.TabIndex = 9;
            // 
            // Dismiss
            // 
            this.Dismiss.Location = new System.Drawing.Point(3, 82);
            this.Dismiss.Name = "Dismiss";
            this.Dismiss.Size = new System.Drawing.Size(138, 23);
            this.Dismiss.TabIndex = 8;
            this.Dismiss.Text = "Dismiss";
            this.Dismiss.UseVisualStyleBackColor = true;
            this.Dismiss.Click += new System.EventHandler(this.Dismiss_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Active Tasks";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Macro Name";
            // 
            // Repeat
            // 
            this.Repeat.AutoSize = true;
            this.Repeat.Enabled = false;
            this.Repeat.Location = new System.Drawing.Point(9, 59);
            this.Repeat.Name = "Repeat";
            this.Repeat.Size = new System.Drawing.Size(61, 17);
            this.Repeat.TabIndex = 11;
            this.Repeat.Text = "Repeat";
            this.Repeat.UseVisualStyleBackColor = true;
            // 
            // MacroEvent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BoundingBox);
            this.Name = "MacroEvent";
            this.Size = new System.Drawing.Size(372, 111);
            this.BoundingBox.ResumeLayout(false);
            this.BoundingBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox BoundingBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Dismiss;
        private System.Windows.Forms.TextBox MacroName;
        private System.Windows.Forms.ListBox ActiveTasks;
        private System.Windows.Forms.CheckBox Repeat;
    }
}
