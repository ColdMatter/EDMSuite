namespace ZeemanSisyphusHardwareControl
{
    partial class ControlWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlWindow));
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.splitPanel = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.messageBoxCollapseExpandButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.messageNumberPanel = new System.Windows.Forms.Panel();
            this.messageNumber = new System.Windows.Forms.TextBox();
            this.messageBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel)).BeginInit();
            this.splitPanel.Panel1.SuspendLayout();
            this.splitPanel.Panel2.SuspendLayout();
            this.splitPanel.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.tableLayoutPanel9.SuspendLayout();
            this.messageNumberPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // tabControl
            // 
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(3, 3);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(689, 836);
            this.tabControl.TabIndex = 0;
            // 
            // splitPanel
            // 
            this.splitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitPanel.Location = new System.Drawing.Point(0, 0);
            this.splitPanel.Name = "splitPanel";
            this.splitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitPanel.Panel1
            // 
            this.splitPanel.Panel1.Controls.Add(this.tableLayoutPanel8);
            // 
            // splitPanel.Panel2
            // 
            this.splitPanel.Panel2.Controls.Add(this.messageBox);
            this.splitPanel.Panel2Collapsed = true;
            this.splitPanel.Size = new System.Drawing.Size(695, 882);
            this.splitPanel.SplitterDistance = 724;
            this.splitPanel.TabIndex = 1;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 1;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.tabControl, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.tableLayoutPanel9, 0, 1);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 2;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(695, 882);
            this.tableLayoutPanel8.TabIndex = 1;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel9.ColumnCount = 3;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 4.848485F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 95.15151F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel9.Controls.Add(this.messageBoxCollapseExpandButton, 2, 0);
            this.tableLayoutPanel9.Controls.Add(this.label8, 1, 0);
            this.tableLayoutPanel9.Controls.Add(this.messageNumberPanel, 0, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(3, 845);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 1;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(689, 34);
            this.tableLayoutPanel9.TabIndex = 1;
            // 
            // messageBoxCollapseExpandButton
            // 
            this.messageBoxCollapseExpandButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.messageBoxCollapseExpandButton.BackColor = System.Drawing.Color.Transparent;
            this.messageBoxCollapseExpandButton.Location = new System.Drawing.Point(657, 5);
            this.messageBoxCollapseExpandButton.Name = "messageBoxCollapseExpandButton";
            this.messageBoxCollapseExpandButton.Size = new System.Drawing.Size(23, 23);
            this.messageBoxCollapseExpandButton.TabIndex = 0;
            this.messageBoxCollapseExpandButton.Text = "+";
            this.messageBoxCollapseExpandButton.UseVisualStyleBackColor = false;
            this.messageBoxCollapseExpandButton.Click += new System.EventHandler(this.ToggleMessageBox);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(41, 10);
            this.label8.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Messages";
            // 
            // messageNumberPanel
            // 
            this.messageNumberPanel.BackColor = System.Drawing.Color.Black;
            this.messageNumberPanel.Controls.Add(this.messageNumber);
            this.messageNumberPanel.Location = new System.Drawing.Point(3, 3);
            this.messageNumberPanel.Name = "messageNumberPanel";
            this.messageNumberPanel.Size = new System.Drawing.Size(25, 28);
            this.messageNumberPanel.TabIndex = 3;
            // 
            // messageNumber
            // 
            this.messageNumber.BackColor = System.Drawing.SystemColors.InfoText;
            this.messageNumber.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.messageNumber.ForeColor = System.Drawing.Color.White;
            this.messageNumber.Location = new System.Drawing.Point(3, 7);
            this.messageNumber.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.messageNumber.Name = "messageNumber";
            this.messageNumber.Size = new System.Drawing.Size(20, 13);
            this.messageNumber.TabIndex = 3;
            this.messageNumber.Text = "0";
            this.messageNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // messageBox
            // 
            this.messageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageBox.Location = new System.Drawing.Point(0, 0);
            this.messageBox.Name = "messageBox";
            this.messageBox.Size = new System.Drawing.Size(150, 46);
            this.messageBox.TabIndex = 0;
            this.messageBox.Text = "";
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(695, 882);
            this.Controls.Add(this.splitPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ControlWindow";
            this.Text = "Zeeman Sisyphus Hardware Controller";
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            this.splitPanel.Panel1.ResumeLayout(false);
            this.splitPanel.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPanel)).EndInit();
            this.splitPanel.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel9.ResumeLayout(false);
            this.tableLayoutPanel9.PerformLayout();
            this.messageNumberPanel.ResumeLayout(false);
            this.messageNumberPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.SplitContainer splitPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private System.Windows.Forms.Button messageBoxCollapseExpandButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel messageNumberPanel;
        private System.Windows.Forms.TextBox messageNumber;
        private System.Windows.Forms.RichTextBox messageBox;

    }
}

