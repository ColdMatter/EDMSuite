using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZeemanSisyphusHardwareControl.Controls;

namespace ZeemanSisyphusHardwareControl
{
    public partial class ControlWindow : Form
    {
        
        public Controller controller;

        public ControlWindow()
        {
            InitializeComponent();
        }

        public void AddTabPage(TabPage tabPage)
        {
            tabControl.Controls.Add(tabPage);
        }

        private void ToggleMessageBox(object sender, EventArgs e)
        {

        }

        private void ControlWindow_Load(object sender, EventArgs e)
        {

        }

    }
}