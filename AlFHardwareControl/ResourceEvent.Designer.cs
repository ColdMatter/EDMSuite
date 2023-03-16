
namespace AlFHardwareControl
{
    partial class ResourceEvent
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
            this.TaskName = new System.Windows.Forms.TextBox();
            this.Dismiss = new System.Windows.Forms.Button();
            this.DiscardTimedSchedOnInterlockFail = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Value = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Resource = new System.Windows.Forms.TextBox();
            this.Comparison = new System.Windows.Forms.TextBox();
            this.BoundingBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BoundingBox
            // 
            this.BoundingBox.Controls.Add(this.Comparison);
            this.BoundingBox.Controls.Add(this.Resource);
            this.BoundingBox.Controls.Add(this.Value);
            this.BoundingBox.Controls.Add(this.label4);
            this.BoundingBox.Controls.Add(this.TaskName);
            this.BoundingBox.Controls.Add(this.Dismiss);
            this.BoundingBox.Controls.Add(this.DiscardTimedSchedOnInterlockFail);
            this.BoundingBox.Controls.Add(this.label2);
            this.BoundingBox.Location = new System.Drawing.Point(0, 0);
            this.BoundingBox.Name = "BoundingBox";
            this.BoundingBox.Size = new System.Drawing.Size(372, 111);
            this.BoundingBox.TabIndex = 8;
            this.BoundingBox.TabStop = false;
            this.BoundingBox.Text = "Resource Event";
            // 
            // TaskName
            // 
            this.TaskName.Enabled = false;
            this.TaskName.Location = new System.Drawing.Point(166, 42);
            this.TaskName.Name = "TaskName";
            this.TaskName.Size = new System.Drawing.Size(200, 20);
            this.TaskName.TabIndex = 9;
            // 
            // Dismiss
            // 
            this.Dismiss.Location = new System.Drawing.Point(228, 82);
            this.Dismiss.Name = "Dismiss";
            this.Dismiss.Size = new System.Drawing.Size(138, 23);
            this.Dismiss.TabIndex = 8;
            this.Dismiss.Text = "Dismiss";
            this.Dismiss.UseVisualStyleBackColor = true;
            this.Dismiss.Click += new System.EventHandler(this.Dismiss_Click);
            // 
            // DiscardTimedSchedOnInterlockFail
            // 
            this.DiscardTimedSchedOnInterlockFail.AutoSize = true;
            this.DiscardTimedSchedOnInterlockFail.Enabled = false;
            this.DiscardTimedSchedOnInterlockFail.Location = new System.Drawing.Point(9, 86);
            this.DiscardTimedSchedOnInterlockFail.Name = "DiscardTimedSchedOnInterlockFail";
            this.DiscardTimedSchedOnInterlockFail.Size = new System.Drawing.Size(157, 17);
            this.DiscardTimedSchedOnInterlockFail.TabIndex = 7;
            this.DiscardTimedSchedOnInterlockFail.Text = "Discard on Interlock Failiure";
            this.DiscardTimedSchedOnInterlockFail.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Scheduled Task";
            // 
            // Value
            // 
            this.Value.Enabled = false;
            this.Value.Location = new System.Drawing.Point(311, 15);
            this.Value.Name = "Value";
            this.Value.Size = new System.Drawing.Size(55, 20);
            this.Value.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Condition";
            // 
            // Resource
            // 
            this.Resource.Enabled = false;
            this.Resource.Location = new System.Drawing.Point(166, 15);
            this.Resource.Name = "Resource";
            this.Resource.Size = new System.Drawing.Size(112, 20);
            this.Resource.TabIndex = 13;
            // 
            // Comparison
            // 
            this.Comparison.Enabled = false;
            this.Comparison.Location = new System.Drawing.Point(284, 15);
            this.Comparison.Name = "Comparison";
            this.Comparison.Size = new System.Drawing.Size(21, 20);
            this.Comparison.TabIndex = 14;
            // 
            // ResourceEvent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BoundingBox);
            this.Name = "ResourceEvent";
            this.Size = new System.Drawing.Size(372, 111);
            this.BoundingBox.ResumeLayout(false);
            this.BoundingBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.GroupBox BoundingBox;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.CheckBox DiscardTimedSchedOnInterlockFail;
        protected System.Windows.Forms.Button Dismiss;
        protected System.Windows.Forms.TextBox TaskName;
        protected System.Windows.Forms.TextBox Comparison;
        protected System.Windows.Forms.TextBox Resource;
        protected System.Windows.Forms.TextBox Value;
        protected System.Windows.Forms.Label label4;
    }
}
