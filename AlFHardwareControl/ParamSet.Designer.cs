
namespace AlFHardwareControl
{
    partial class ParamSet
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
            this.Param_Set = new System.Windows.Forms.Button();
            this.Param_Target = new System.Windows.Forms.TextBox();
            this.Param_Name = new System.Windows.Forms.Label();
            this.Param_Value = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Param_Set
            // 
            this.Param_Set.Location = new System.Drawing.Point(155, 1);
            this.Param_Set.Name = "Param_Set";
            this.Param_Set.Size = new System.Drawing.Size(45, 23);
            this.Param_Set.TabIndex = 10;
            this.Param_Set.Text = "Set";
            this.Param_Set.UseVisualStyleBackColor = true;
            this.Param_Set.Click += new System.EventHandler(this.Param_Set_Click);
            // 
            // Param_Target
            // 
            this.Param_Target.Location = new System.Drawing.Point(111, 3);
            this.Param_Target.Name = "Param_Target";
            this.Param_Target.Size = new System.Drawing.Size(39, 20);
            this.Param_Target.TabIndex = 9;
            // 
            // Param_Name
            // 
            this.Param_Name.AutoSize = true;
            this.Param_Name.Location = new System.Drawing.Point(5, 6);
            this.Param_Name.Name = "Param_Name";
            this.Param_Name.Size = new System.Drawing.Size(31, 13);
            this.Param_Name.TabIndex = 7;
            this.Param_Name.Text = "VMO";
            // 
            // Param_Value
            // 
            this.Param_Value.Enabled = false;
            this.Param_Value.Location = new System.Drawing.Point(55, 3);
            this.Param_Value.Name = "Param_Value";
            this.Param_Value.Size = new System.Drawing.Size(50, 20);
            this.Param_Value.TabIndex = 11;
            // 
            // ParamSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Param_Value);
            this.Controls.Add(this.Param_Set);
            this.Controls.Add(this.Param_Target);
            this.Controls.Add(this.Param_Name);
            this.Name = "ParamSet";
            this.Size = new System.Drawing.Size(203, 27);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Param_Set;
        private System.Windows.Forms.TextBox Param_Target;
        private System.Windows.Forms.TextBox Param_Value;
        public System.Windows.Forms.Label Param_Name;
    }
}
