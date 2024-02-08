using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.HAL;
using DAQ.Environment;

namespace AlFHardwareControl
{
    public partial class Shutter_Control : UserControl
    {

        public Shutter_Control()
        {
            InitializeComponent();

            this.ShutterPanel.AutoScroll = false;
            this.ShutterPanel.HorizontalScroll.Enabled = false;
            this.ShutterPanel.HorizontalScroll.Visible = false;
            this.ShutterPanel.HorizontalScroll.Maximum = 0;
            this.ShutterPanel.AutoScroll = true;

            int i = 0;
            foreach (Tuple<string,string,bool> shutter in (List<Tuple<string, string, bool>>)Environs.Hardware.GetInfo("Shutters"))
            {
                ShutterSet s = new ShutterSet(shutter.Item1, shutter.Item2, shutter.Item3);
                this.ShutterPanel.Controls.Add(s);
                s.Location = new Point(0, i);
                i += 26;
            }
        }
    }
}
