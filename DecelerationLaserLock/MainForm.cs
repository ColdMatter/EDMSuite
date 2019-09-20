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


namespace DecelerationLaserLock
{
    /// <summary>
    /// Front panel of the laser controller
    /// </summary>
    public partial class MainForm : Form
    {

        public LaserController controller;
        private Thread controlThread;
        
        public MainForm()
        {
            InitializeComponent();
        }

        #region Public properties

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

        public double ControlVoltageNumericEditorValue
        {
            get
            {
                return controlVoltageNumericEditor.Value;
            }
        }

        public bool SlopeSwitchState
        {
            get { return slopeSwitch.Value; }
        }

        public bool SpeedSwitchState
        {
            get { return speedSwitch.Value; }
        }

        public int ScansPerPark
        {
            get { return (int)scanNumberBox.Value; }
        }

        #endregion

        #region Thread-safe wrappers

        private delegate void AppendToTextBoxDelegate(string text);
        public void AddToTextBox(String text)
        {
            textBox1.Invoke(new AppendToTextBoxDelegate(textBox1.AppendText), text);
        }

        private void SetEditorValue(NumericEdit control, double d)
        {
            control.Value = d;
        }
        private delegate void SetNumericEditorDelegate(NumericEdit control, double d);
        public void SetControlVoltageNumericEditorValue(double val)
        {
            controlVoltageNumericEditor.Invoke(new SetNumericEditorDelegate(SetEditorValue), new Object[] { controlVoltageNumericEditor, val });
        }

        public void SetSetPointNumericEditorValue(double val)
        {
            setpointNumericEdit.Invoke(new SetNumericEditorDelegate(SetEditorValue), new Object[] { setpointNumericEdit, val });
        }

        private delegate void PlotXYDelegate(double y);
        public void DeviationPlotXYAppend(double y)
        {
            deviationGraph.Invoke(new PlotXYDelegate(deviationGraph.Plots[0].PlotYAppend), y);
        }

        private void SetNumericEditEnabledState(NumericEdit control, bool state)
        {
            control.Enabled = state;
        }

        private delegate void EnableDelegate(NumericEdit numericEdit, bool enable);
        public void ControlVoltageEditorEnabledState(bool state)
        {
            controlVoltageNumericEditor.Invoke(new EnableDelegate(SetNumericEditEnabledState), new Object[] { controlVoltageNumericEditor, state });
        }
        public void SetPointEditorEnabledState(bool state)
        {
            setpointNumericEdit.Invoke(new EnableDelegate(SetNumericEditEnabledState), new Object[] { setpointNumericEdit, state });
        }

        private delegate void SetCheckDelegate(CheckBox box, bool state);
        private void SetCheckHelper(CheckBox box, bool state)
        {
            box.Checked = state;
        }
        public void SetLockCheckBox(bool state)
        {
            LockCheck.Invoke(new SetCheckDelegate(SetCheckHelper), new object[] { LockCheck, state });
        }

        #endregion

        #region Event handlers

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
        private void pSlider_AfterChangeValue(object sender, AfterChangeNumericValueEventArgs e)
        {
            controller.SetProportionalGain(pSlider.Value);
        }
        private void iSlider_AfterChangeValue(object sender, AfterChangeNumericValueEventArgs e)
        {
            controller.SetIntegralGain(iSlider.Value);
        }
        private void setpointNumericEdit_AfterChangeValue(object sender, AfterChangeNumericValueEventArgs e)
        {
            controller.SetPoint = setpointNumericEdit.Value;
        }

        private void controlVoltageNumericEditor_AfterChangeValue(object sender, AfterChangeNumericValueEventArgs e)
        {
            controller.LaserVoltage = controlVoltageNumericEditor.Value;
        }

        #endregion

        #region Private methods

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
        #endregion

    }
}