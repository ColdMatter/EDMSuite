namespace IMAQ
{
    partial class ImageViewerWindow
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
            this.imageViewer = new NationalInstruments.Vision.WindowsForms.ImageViewer();
            this.consoleRichTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // imageViewer
            // 
            this.imageViewer.ActiveTool = NationalInstruments.Vision.WindowsForms.ViewerTools.ZoomIn;
            this.imageViewer.AutoSize = true;
            this.imageViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imageViewer.Location = new System.Drawing.Point(-2, 0);
            this.imageViewer.Name = "imageViewer";
            this.imageViewer.ShowScrollbars = true;
            this.imageViewer.Size = new System.Drawing.Size(793, 433);
            this.imageViewer.TabIndex = 0;
            this.imageViewer.ZoomToFit = true;
            this.imageViewer.RoiChanged += new System.EventHandler<NationalInstruments.Vision.WindowsForms.ContoursChangedEventArgs>(this.imageViewer_RoiChanged);
            // 
            // consoleRichTextBox
            // 
            this.consoleRichTextBox.BackColor = System.Drawing.Color.Black;
            this.consoleRichTextBox.ForeColor = System.Drawing.Color.Lime;
            this.consoleRichTextBox.Location = new System.Drawing.Point(-2, 430);
            this.consoleRichTextBox.Name = "consoleRichTextBox";
            this.consoleRichTextBox.ReadOnly = true;
            this.consoleRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.consoleRichTextBox.Size = new System.Drawing.Size(793, 185);
            this.consoleRichTextBox.TabIndex = 24;
            this.consoleRichTextBox.Text = "";
            this.consoleRichTextBox.TextChanged += new System.EventHandler(this.consoleRichTextBox_TextChanged);
            // 
            // hScrollBar
            // 
            this.hScrollBar.Location = new System.Drawing.Point(0, 425);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(791, 17);
            this.hScrollBar.TabIndex = 25;
            this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar_Scroll);
            // 
            // ImageViewerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(790, 612);
            this.Controls.Add(this.consoleRichTextBox);
            this.Controls.Add(this.imageViewer);
            this.Name = "ImageViewerWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Image";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImageViewerWindow_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public NationalInstruments.Vision.WindowsForms.ImageViewer imageViewer;
        private System.Windows.Forms.RichTextBox consoleRichTextBox;
    }
}