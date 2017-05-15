using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicrocavityScanner.GUI
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        // the application controller
        public Controller controller;

        public MainForm(Controller controller)
        {
            this.controller = controller;
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {

        }

        private void label1_Click_2(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                controller.laserSettings.Add("FastLaser", FastAxisSelectCombo.Text);
                controller.scanSettings.Add("FastAxisStart", Convert.ToDouble(FastAxisStart.Text));
                controller.scanSettings.Add("FastAxisEnd", Convert.ToDouble(FastAxisEnd.Text));
                controller.scanSettings.Add("FastAxisRes", Convert.ToDouble(FastAxisRes.Text));
                controller.laserSettings.Add("SlowLaser", SlowAxisSelectCombo.Text);
                controller.scanSettings.Add("SlowAxisStart", Convert.ToDouble(SlowAxisStart.Text));
                controller.scanSettings.Add("SlowAxisEnd", Convert.ToDouble(SlowAxisEnd.Text));
                controller.scanSettings.Add("SlowAxisRes", Convert.ToDouble(SlowAxisRes.Text));
                controller.scanSettings.Add("Exposure", Convert.ToDouble(Exposure.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message, 
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void MainForm_ClosedEvent(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void FastAxisStart_TextChanged(object sender, EventArgs e)
        {
            try
            {
                controller.scanSettings.Add("FastAxisStart", Convert.ToDouble(FastAxisStart.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FastAxisEnd_TextChanged(object sender, EventArgs e)
        {
            try
            {
                controller.scanSettings.Add("FastAxisEnd", Convert.ToDouble(FastAxisEnd.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FastAxisRes_TextChanged(object sender, EventArgs e)
        {
            try
            {
                controller.scanSettings.Add("FastAxisRes", Convert.ToDouble(FastAxisRes.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SlowAxisStart_TextChanged(object sender, EventArgs e)
        {
            try
            {
                controller.scanSettings.Add("SlowAxisStart", Convert.ToDouble(SlowAxisStart.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SlowAxisEnd_TextChanged(object sender, EventArgs e)
        {
            try
            {
                controller.scanSettings.Add("SlowAxisEnd", Convert.ToDouble(SlowAxisEnd.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SlowAxisRes_TextChanged(object sender, EventArgs e)
        {
            try
            {
                controller.scanSettings.Add("SlowAxisRes", Convert.ToDouble(SlowAxisRes.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Exposure_TextChanged(object sender, EventArgs e)
        {
            try
            {
                controller.scanSettings.Add("Exposure", Convert.ToDouble(Exposure.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FastAxisSelectCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                controller.laserSettings.Add("FastLaser", FastAxisSelectCombo.Text);
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SlowAxisSelectCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                controller.laserSettings.Add("SlowLaser", SlowAxisSelectCombo.Text);
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
