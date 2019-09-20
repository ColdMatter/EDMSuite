using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SirCachealot
{
    public partial class MainWindow : Form
    {
        internal Controller controller;

        public MainWindow()
        {
            InitializeComponent();
        }

        // choosing File->Exit is set to close the form.
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        // If the form closes, either from File->Exit, or by clicking the close
        // box, then the controller is alerted, so that it can shut down.
        private void formCloseHandler(object sender, FormClosingEventArgs e)
        {
            controller.Exit();
        }
        // this alerts the controller that the form is loaded and ready.
        private void formLoadHandler(object sender, EventArgs e)
        {
            controller.UIInitialise();
        }


        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.CreateDB();
        }

        internal void SetMemcachedStatsText(string p)
        {
            statsTextBox.Text = p;
        }

        private void selectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SelectDB();
        }

        public void AppendToLog(string txt)
        {
            logTextBox.BeginInvoke(new AppendTextDelegate(logTextBox.AppendText),
				new object[] {txt + Environment.NewLine});
        }
        
        public void AppendToErrorLog(string txt)
        {
            errorLogTextBox.BeginInvoke(new AppendTextDelegate(errorLogTextBox.AppendText),
                new object[] { txt + Environment.NewLine });
        }

        public void SetStatsText(string txt)
        {
            logTextBox.BeginInvoke(new AppendTextDelegate(SetStatsTextInternal),
                new object[] { txt + Environment.NewLine });
        }

        private void SetStatsTextInternal(string txt)
        {
            statsTextBox.Text = txt;
        }

 		private delegate void AppendTextDelegate(String text);

        private void test1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.Test1();
        }

        private void test2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.Test2();
        }
    }
}