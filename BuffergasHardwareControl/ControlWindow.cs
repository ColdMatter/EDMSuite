using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BuffergasHardwareControl
{
    public partial class ControlWindow : Form
    {

        private double flow_voltage;

        public Controller controller;


        public ControlWindow()
        {
            InitializeComponent();
        }
        


        private void FlowVoltageBox_ValueChanged(object sender, EventArgs e)
        {
            //the conversion factor from voltage to flow, currently set to 1
            flow_voltage = ((double)FlowVoltageBox.Value) * 1;
            controller.FlowControlVoltage = flow_voltage;
        }

       

        private void FlowmeterButton_Click(object sender, EventArgs e)
            {
                flowmeterTextBox1.Text = controller.FlowInputVoltage.ToString();
            }

     
    }
}
