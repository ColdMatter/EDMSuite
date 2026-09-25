using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace AlFHardwareControl
{
    public partial class ShutterSet : UserControl
    {

        private bool invert;
        private bool state;
        private DigitalSingleChannelWriter shutterWriter;
        private Color closedColor;
        private Color openColor;

        public ShutterSet()
        {
            InitializeComponent();
        }

        public ShutterSet(string _shutterName, string shutterChannel, bool _invert)
        {
            InitializeComponent();

            this.shutterName.Text = _shutterName;
            this.invert = _invert;
            state = false;

            if (!_invert)
            {
                this.Open.Click += openShutter;
                this.Close.Click += closeShutter;
                this.openColor = Color.PaleGreen;
                this.closedColor = Color.Salmon;
            }else
            {
                this.Open.Click += closeShutter;
                this.Close.Click += openShutter;
                this.closedColor = Color.PaleGreen;
                this.openColor = Color.Salmon;
            }
            try {
                Task t = new Task();
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[shutterChannel]).AddToTask(t);
                shutterWriter = new DigitalSingleChannelWriter(t.Stream);
            } catch (DaqException e)
            {
                this.Enabled = false;
            }

        }

        private void openShutter(object sender, EventArgs args)
        {
            try
            {
                shutterWriter.WriteSingleSampleSingleLine(true, true);
                this.shutterName.BackColor = openColor;
                state = true;
            }
            catch(DaqException e)
            {
                TaskScheduler.tSched.UpdateEventLog("Shutter write failed for shutter" + this.shutterName.Text + "!");
            }
        }

        private void closeShutter(object sender, EventArgs args)
        {
            try
            {
                shutterWriter.WriteSingleSampleSingleLine(true, false);
                this.shutterName.BackColor = closedColor;
                state = false;
            }
            catch (DaqException e)
            {
                TaskScheduler.tSched.UpdateEventLog("Shutter write failed for shutter" + this.shutterName.Text + "!");
            }
        }

    }
}
