using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicrocavityHardwareControl
{
    public partial class ControlWindow : Form
    {
        public Controller controller;
        
        public ControlWindow()
        {
            InitializeComponent();
        }

        private void uCavityReflectionECDL_TextChanged(object sender, EventArgs e)
        {

        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateMonitoring();
        }

        private void uCavityReflectionECDLLabel_Click(object sender, EventArgs e)
        {

        }

        private void uCavityReflectionTiSapphLabel_Click(object sender, EventArgs e)
        {

        }

        private void uCavityReflectionTiSapph_TextChanged(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
