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

        public LaserController controller;
        private Thread controlThread;
        
        public MainForm()
        {
            InitializeComponent();       
        }

        public double PSliderValue
        {
            get { return pSlider.Value; }
        }

        public double ISliderValue
        {
            get { return iSlider.Value; }
        }

        public double DSliderValue
        {
            get { return dSlider.Value; }
        }

        private delegate void AppendToTextBoxDelegate(string text);
        public void AddToTextBox(String text)
        {
            textBox1.Invoke(new AppendToTextBoxDelegate(textBox1.AppendText), text);
        }

        private void SetVoltageEditorValue(double d)
        {
            outputValueNumericEditor.Value = d;
        }
        private delegate void SetNumericEditorDelegate(double d);
        public void SetOutputVoltageNumericEditorValue(double val)
        {
            outputValueNumericEditor.Invoke(new SetNumericEditorDelegate(SetVoltageEditorValue), val);
        }
        
        public double OutputVoltageNumericEditorValue
        {
            get
            {
                return outputValueNumericEditor.Value;
            }            
        }

        private void UpdateVoltage(object sender, AfterChangeNumericValueEventArgs e)
        {
            controller.LaserVoltage = outputValueNumericEditor.Value;
        }

        private void UpdateVoltage(object sender, EventArgs e)
        {
            controller.LaserVoltage = outputValueNumericEditor.Value;
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
            LockCheck.Checked = true;
        }

        private void unlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LockCheck.Checked = false;
        }

        private void lockCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (LockCheck.Checked) Lock();
            else Unlock();
        }

        private void Lock()
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

        private void Unlock()
        {
            if (controller.Status == LaserController.ControllerState.busy)
            {
                controller.Status = LaserController.ControllerState.stopping;
            }
        }

        private void pSlider_AfterChangeValue(object sender, AfterChangeNumericValueEventArgs e)
        {
            controller.SetProportionalGain(pSlider.Value);
        }

        private void outputValueNumericEditor_Click(object sender, EventArgs e)
        {
            outputValueNumericEditor.Value = controller.LaserVoltage;
        }

    }
}