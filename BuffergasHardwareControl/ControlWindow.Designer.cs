namespace BuffergasHardwareControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.FlowVoltageBox = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.flowmeterTextBox1 = new System.Windows.Forms.TextBox();
            this.flowmeterbutton1 = new System.Windows.Forms.Button();
            this.snapshotButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.streamButton = new System.Windows.Forms.Button();
            this.stopStreamButton = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.openImageViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImagedataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleRichTextBox = new System.Windows.Forms.RichTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.led1 = new NationalInstruments.UI.WindowsForms.Led();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.disposeButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.FlowVoltageBox)).BeginInit();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(88, 121);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Flow Controller";
            // 
            // FlowVoltageBox
            // 
            this.FlowVoltageBox.Location = new System.Drawing.Point(69, 147);
            this.FlowVoltageBox.Name = "FlowVoltageBox";
            this.FlowVoltageBox.Size = new System.Drawing.Size(120, 20);
            this.FlowVoltageBox.TabIndex = 2;
            this.FlowVoltageBox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.FlowVoltageBox.ValueChanged += new System.EventHandler(this.FlowVoltageBox_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(88, 170);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Flow Meter";
            // 
            // flowmeterTextBox1
            // 
            this.flowmeterTextBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.flowmeterTextBox1.Location = new System.Drawing.Point(69, 186);
            this.flowmeterTextBox1.Name = "flowmeterTextBox1";
            this.flowmeterTextBox1.ReadOnly = true;
            this.flowmeterTextBox1.Size = new System.Drawing.Size(100, 20);
            this.flowmeterTextBox1.TabIndex = 6;
            // 
            // flowmeterbutton1
            // 
            this.flowmeterbutton1.Location = new System.Drawing.Point(171, 185);
            this.flowmeterbutton1.Name = "flowmeterbutton1";
            this.flowmeterbutton1.Size = new System.Drawing.Size(18, 21);
            this.flowmeterbutton1.TabIndex = 7;
            this.flowmeterbutton1.Text = "F";
            this.flowmeterbutton1.UseVisualStyleBackColor = true;
            this.flowmeterbutton1.Click += new System.EventHandler(this.FlowmeterButton_Click);
            // 
            // snapshotButton
            // 
            this.snapshotButton.Location = new System.Drawing.Point(297, 120);
            this.snapshotButton.Name = "snapshotButton";
            this.snapshotButton.Size = new System.Drawing.Size(49, 31);
            this.snapshotButton.TabIndex = 8;
            this.snapshotButton.Text = "Snap";
            this.snapshotButton.UseVisualStyleBackColor = true;
            this.snapshotButton.Click += new System.EventHandler(this.snapshotButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.Control;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(96, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 24);
            this.label3.TabIndex = 9;
            this.label3.Text = "Flow";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.Control;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(374, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 24);
            this.label4.TabIndex = 10;
            this.label4.Text = "Camera";
            // 
            // streamButton
            // 
            this.streamButton.Location = new System.Drawing.Point(378, 120);
            this.streamButton.Name = "streamButton";
            this.streamButton.Size = new System.Drawing.Size(58, 31);
            this.streamButton.TabIndex = 14;
            this.streamButton.Text = "Stream";
            this.streamButton.UseVisualStyleBackColor = true;
            this.streamButton.Click += new System.EventHandler(this.streamButton_Click);
            // 
            // stopStreamButton
            // 
            this.stopStreamButton.Location = new System.Drawing.Point(465, 121);
            this.stopStreamButton.Name = "stopStreamButton";
            this.stopStreamButton.Size = new System.Drawing.Size(43, 30);
            this.stopStreamButton.TabIndex = 15;
            this.stopStreamButton.Text = "Stop";
            this.stopStreamButton.UseVisualStyleBackColor = true;
            this.stopStreamButton.Click += new System.EventHandler(this.stopStreamButton_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(575, 25);
            this.toolStrip1.TabIndex = 16;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openImageViewerToolStripMenuItem,
            this.saveImageToolStripMenuItem,
            this.saveImagesToolStripMenuItem,
            this.saveImagedataToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 22);
            this.toolStripDropDownButton1.Text = "toolStripDropDownButton1";
            // 
            // openImageViewerToolStripMenuItem
            // 
            this.openImageViewerToolStripMenuItem.Name = "openImageViewerToolStripMenuItem";
            this.openImageViewerToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.openImageViewerToolStripMenuItem.Text = "Start camera and open image viewer";
            this.openImageViewerToolStripMenuItem.Click += new System.EventHandler(this.openImageViewerToolStripMenuItem_Click);
            // 
            // saveImageToolStripMenuItem
            // 
            this.saveImageToolStripMenuItem.Name = "saveImageToolStripMenuItem";
            this.saveImageToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.saveImageToolStripMenuItem.Text = "Save image";
            this.saveImageToolStripMenuItem.Click += new System.EventHandler(this.saveImageToolStripMenuItem_Click);
            // 
            // saveImagesToolStripMenuItem
            // 
            this.saveImagesToolStripMenuItem.Name = "saveImagesToolStripMenuItem";
            this.saveImagesToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.saveImagesToolStripMenuItem.Text = "Save images";
            this.saveImagesToolStripMenuItem.Click += new System.EventHandler(this.saveImagesToolStripMenu_Click);
            // 
            // saveImagedataToolStripMenuItem
            // 
            this.saveImagedataToolStripMenuItem.Name = "saveImagedataToolStripMenuItem";
            this.saveImagedataToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.saveImagedataToolStripMenuItem.Text = "Save imagedata";
            this.saveImagedataToolStripMenuItem.Click += new System.EventHandler(this.saveImageDataToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // consoleRichTextBox
            // 
            this.consoleRichTextBox.BackColor = System.Drawing.Color.Black;
            this.consoleRichTextBox.ForeColor = System.Drawing.Color.Lime;
            this.consoleRichTextBox.Location = new System.Drawing.Point(0, 282);
            this.consoleRichTextBox.Name = "consoleRichTextBox";
            this.consoleRichTextBox.Size = new System.Drawing.Size(575, 79);
            this.consoleRichTextBox.TabIndex = 17;
            this.consoleRichTextBox.Text = "";
            this.consoleRichTextBox.TextChanged += new System.EventHandler(this.consoleRichTextBox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(471, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Trigger";
            // 
            // led1
            // 
            this.led1.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led1.Location = new System.Drawing.Point(479, 28);
            this.led1.Name = "led1";
            this.led1.Size = new System.Drawing.Size(29, 29);
            this.led1.TabIndex = 19;
            // 
            // commandTextBox
            // 
            this.commandTextBox.BackColor = System.Drawing.Color.Black;
            this.commandTextBox.ForeColor = System.Drawing.Color.Lime;
            this.commandTextBox.Location = new System.Drawing.Point(0, 372);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(575, 20);
            this.commandTextBox.TabIndex = 23;
            this.commandTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.commandTextBox_KeyDown);
            // 
            // disposeButton
            // 
            this.disposeButton.Location = new System.Drawing.Point(297, 186);
            this.disposeButton.Name = "disposeButton";
            this.disposeButton.Size = new System.Drawing.Size(69, 30);
            this.disposeButton.TabIndex = 24;
            this.disposeButton.Text = "Dispose";
            this.disposeButton.UseVisualStyleBackColor = true;
            this.disposeButton.Click += new System.EventHandler(this.disposeButton_Click);
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 394);
            this.Controls.Add(this.disposeButton);
            this.Controls.Add(this.commandTextBox);
            this.Controls.Add(this.led1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.consoleRichTextBox);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.stopStreamButton);
            this.Controls.Add(this.streamButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.snapshotButton);
            this.Controls.Add(this.flowmeterbutton1);
            this.Controls.Add(this.flowmeterTextBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FlowVoltageBox);
            this.Controls.Add(this.label1);
            this.Name = "ControlWindow";
            this.Text = "ControlWindow";
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.FlowVoltageBox)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown FlowVoltageBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox flowmeterTextBox1;
        private System.Windows.Forms.Button flowmeterbutton1;
        private System.Windows.Forms.Button snapshotButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button streamButton;
        private System.Windows.Forms.Button stopStreamButton;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem saveImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openImageViewerToolStripMenuItem;
        private System.Windows.Forms.RichTextBox consoleRichTextBox;
        private System.Windows.Forms.ToolStripMenuItem saveImagedataToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private NationalInstruments.UI.WindowsForms.Led led1;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.ToolStripMenuItem saveImagesToolStripMenuItem;
        private System.Windows.Forms.Button disposeButton;
    }
}

