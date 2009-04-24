using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Analysis.EDM;

namespace EDMBlockHead
{
    public partial class LiveViewer : Form
    {
        private Controller controller;

        // This is a List of channels to plot for the demodulated blocks.
        // Each channel is specified by an array of switch name strings
        // (see DetectorChannelValues.cs for details).
        private List<string[]> channelList = new List<string[]>();

        public LiveViewer(Controller c)
        {
            InitializeComponent();
            controller = c;
            // for now, the viewer is configured here and needs to be recompiled to change
            // the channel list.
            channelList.Add(new string[] { "SIG" });
            channelList.Add(new string[] { "DB" });
            channelList.Add(new string[] { "B" });
            channelList.Add(new string[] { "RF1F" });
            channelList.Add(new string[] { "RF2F" });
            channelList.Add(new string[] { "RF1A" });
            channelList.Add(new string[] { "RF2A" });
            channelList.Add(new string[] { "E", "RF1F" });
            channelList.Add(new string[] { "E", "RF2F" });

            // add the graphs
            //foreach (string[] chan in channelList)
            //    graphPanel.Controls.Add(new NationalInstruments.
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        public void Clear()
        {
        }

        public void AddDBlock(DemodulatedBlock dblock)
        {
        }

        public void AppendStatusText(string newText)
        {
            SetTextBox(statusText, statusText.Text + newText);
        }

        public void UpdateClusterStatusText(string newText)
        {
            SetTextBox(clusterStatusText, newText);
        }

        public void SetTextBox(TextBox textBox, string text)
        {
            textBox.Invoke(new SetTextDelegate(SetTextHelper), new object[] { textBox, text });
        }

        private delegate void SetTextDelegate(TextBox textBox, string text);

        private void SetTextHelper(TextBox textBox, string text)
        {
            textBox.Text = text;
        }
     

        #region Click Handler

        private void resetRunningMeans_Click(object sender, EventArgs e)
        {
            controller.resetEdmErrRunningMeans();
        }

        #endregion



    }

}