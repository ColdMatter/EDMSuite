using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace LaserLock
{
    public partial class MainForm : Form
    {

        private LaserController controller;
        private Thread controlThread;
        
        public MainForm()
        {
            InitializeComponent();
            controller = new LaserController(this);           
        }

        private delegate void AppendToTextBoxDelegate(string text);
        public void AddToTextBox(String text)
        {
            textBox1.Invoke(new AppendToTextBoxDelegate(textBox1.AppendText), text);
        }

        public double OutputVoltageNumericEditorValue
        {
            get
            {
                return outputValueNumericEditor.Value;
            }
            set 
            {
                outputValueNumericEditor.Value = value;            
            }
            
        }

        private void UpdateVoltage(object sender, AfterChangeNumericValueEventArgs e)
        {
            controller.Voltage = outputValueNumericEditor.Value;
        }

        private void UpdateVoltage(object sender, EventArgs e)
        {
            controller.Voltage = outputValueNumericEditor.Value;
        }

        private void parkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controller.Status == LaserController.ControllerState.free)
            {
                controlThread = new Thread(new ThreadStart(controller.Park));
                controlThread.Name = "Laser Lock Control Thread";
                controlThread.Priority = ThreadPriority.Normal;
                controlThread.Start();
            }
            else 
            {
                // do nothing
            }
        }

        private void lockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controller.Status == LaserController.ControllerState.free)
            {
                controlThread = new Thread(new ThreadStart(controller.Lock));
                controlThread.Name = "Laser Lock Control Thread";
                controlThread.Priority = ThreadPriority.Normal;
                controlThread.Start();
            }
            else 
            { 
                // do nothing 
            }
        }

        private void unlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controller.Status == LaserController.ControllerState.busy)
            {
                controller.Status = LaserController.ControllerState.stopping;
            }
        }

    }
}