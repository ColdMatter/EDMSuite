namespace MOTMaster
{
    partial class ControllerWindow
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
            this.resultsTextBox = new System.Windows.Forms.TextBox();
            this.scriptListComboBox = new System.Windows.Forms.ComboBox();
            this.lookupScriptsButton = new System.Windows.Forms.Button();
            this.compileAndRunButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // resultsTextBox
            // 
            this.resultsTextBox.Location = new System.Drawing.Point(290, 41);
            this.resultsTextBox.Name = "resultsTextBox";
            this.resultsTextBox.ReadOnly = true;
            this.resultsTextBox.Size = new System.Drawing.Size(364, 20);
            this.resultsTextBox.TabIndex = 4;
            // 
            // scriptListComboBox
            // 
            this.scriptListComboBox.FormattingEnabled = true;
            this.scriptListComboBox.Location = new System.Drawing.Point(4, 12);
            this.scriptListComboBox.MaxDropDownItems = 32;
            this.scriptListComboBox.Name = "scriptListComboBox";
            this.scriptListComboBox.Size = new System.Drawing.Size(650, 21);
            this.scriptListComboBox.Sorted = true;
            this.scriptListComboBox.TabIndex = 6;
            // 
            // lookupScriptsButton
            // 
            this.lookupScriptsButton.Location = new System.Drawing.Point(4, 39);
            this.lookupScriptsButton.Name = "lookupScriptsButton";
            this.lookupScriptsButton.Size = new System.Drawing.Size(137, 23);
            this.lookupScriptsButton.TabIndex = 7;
            this.lookupScriptsButton.Text = "Lookup scripts";
            this.lookupScriptsButton.UseVisualStyleBackColor = true;
            this.lookupScriptsButton.Click += new System.EventHandler(this.lookupScriptsButton_Click);
            // 
            // compileAndRunButton
            // 
            this.compileAndRunButton.Location = new System.Drawing.Point(147, 39);
            this.compileAndRunButton.Name = "compileAndRunButton";
            this.compileAndRunButton.Size = new System.Drawing.Size(137, 23);
            this.compileAndRunButton.TabIndex = 8;
            this.compileAndRunButton.Text = "Compile and Run";
            this.compileAndRunButton.UseVisualStyleBackColor = true;
            this.compileAndRunButton.Click += new System.EventHandler(this.compileAndRunButton_Click);
            // 
            // ControllerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(656, 69);
            this.Controls.Add(this.compileAndRunButton);
            this.Controls.Add(this.lookupScriptsButton);
            this.Controls.Add(this.scriptListComboBox);
            this.Controls.Add(this.resultsTextBox);
            this.Name = "ControllerWindow";
            this.Text = "MOTMaster Main Window";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox resultsTextBox;
        private System.Windows.Forms.ComboBox scriptListComboBox;
        private System.Windows.Forms.Button lookupScriptsButton;
        private System.Windows.Forms.Button compileAndRunButton;
    }
}

