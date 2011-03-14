using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MOTMaster
{
    public partial class ControllerWindow : Form
    {
        public Controller controller = new Controller();

        public ControllerWindow()
        {
            InitializeComponent();
        }

        private void StartHardwareControlButton_Click(object sender, EventArgs e)
        {
            controller.StartHardwareControl();
        }

        private void stopHardwareControlButton_Click(object sender, EventArgs e)
        {
            controller.StopHardwareControl();
        }


    }
}
