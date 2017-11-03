using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

namespace PaddlePolStabiliser
{
    public partial class MainForm : Form
    {
        public Controller controller;
        public bool LockEngaged = false;

        public MainForm(Controller controller)
        {
            this.controller = controller;

            controller.GUIUpdate += new Controller.GUIUpdateHandler(UpdateFront);

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] deviceList = DaqSystem.Local.Devices;
            comboDetectorDevice.DataSource = deviceList;

            string[] portsList = SerialPort.GetPortNames();
            combController.DataSource = portsList;

            controller.progSettings.Add("DetectorDevice", comboDetectorDevice.Text);
            controller.progSettings.Add("DetectorChannel", combDetectorChannel.Text);
            controller.progSettings.Add("ControllerChannel", combController.Text);
        }

        private void comboDetectorDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboDetectorDevice.Text != "")
                {
                    controller.progSettings["DetectorDevice"] = comboDetectorDevice.Text;
                }


                string[] deviceChannels = DaqSystem.Local.LoadDevice(controller.
                    progSettings["DetectorDevice"]).AIPhysicalChannels;
                combDetectorChannel.DataSource = deviceChannels;

            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void combDetectorChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (combDetectorChannel.Text != "")
                {
                    controller.progSettings["DetectorChannel"] = combDetectorChannel.Text;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void combController_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (combController.Text != "")
                {
                    controller.progSettings["ControllerChannel"] = combController.Text;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void butRun_Click(object sender, EventArgs e)
        {
            controller.StartLoop();
        }

        private void UpdateStatusBar()
        {
            Controller.ControllerState state = Controller.GetController().LockState;
            switch (state)
            {
                case Controller.ControllerState.UNLOCKED:
                    toolStripStatusLabel1.Text = "Unlocked";
                    break;
                case Controller.ControllerState.LOCKING:
                    toolStripStatusLabel1.Text = "Locking...";
                    break;
                case Controller.ControllerState.LOCKED:
                    toolStripStatusLabel1.Text = "LOCKED";
                    break;
                default:
                    toolStripStatusLabel1.Text = "";
                    break;
            }
        }

        public void UpdateManualGraph()
        {
            double[] latestpoints = controller.StoredData.ToArray();
            double[] lockpoints = new double[100];
            double thelock = controller.LockLevel;
            for (int i = 0; i < 100; ++i)
            {
                lockpoints[i] = thelock;
            }
            PlotY(waveformGraph1, waveformPlotDetector, 1, 1, latestpoints);
            PlotY(waveformGraph1, waveformPlotLock, 1, 1,lockpoints);
        }

        //public void UpdateGraphs(Controller m, EventArgs e)
        //{
        //    waveformGraph1.PlotXAppend(
        //        Convert.ToDouble(controller.StoredData.Peek()));
        //}

        private void UpdateFront(Controller controller, EventArgs e)
        {
            UpdateStatusBarHelper(this);
            UpdateManualGraph();
        }

        public void DisAbleRun()
        {
            DisAbleRunHelper(this);
        }

        private void DisAbleRunButton()
        {
            Controller.ProgramState state = controller.ProgState;
            switch (state)
            {
                case Controller.ProgramState.RUNNING:
                    butRun.Enabled = false;
                    break;
                case Controller.ProgramState.STOPPED:
                    butRun.Enabled = true;
                    break;
                default:
                    butRun.Enabled = true;
                    break;
            }
        }

        public void EndOfTravel()
        {
            EndOFTravelHelper(this);
        }

        private void EndOfTravelError()
        {
            labStatus.Text = "Error: The actuator has reached\r " +
                "the end of its travel. \r" +
                "Lock Stopped.";
        }

        public void ClearEndOfTravel()
        {
            ClearEndOFTravelHelper(this);
        }

        private void ClearEndOfTravelError()
        {
            labStatus.Text = "";
        }

        // UI delegates and thread-safe helpers
        private delegate void ClearDataDelegate();
        private void ClearNIGraph(Graph graph)
        {
            graph.Invoke(new ClearDataDelegate(graph.ClearData));
        }
        private delegate void PlotYDelegate(double[] yData, double start, double inc);
        private void PlotY(Graph graph, WaveformPlot p, double start, double inc, double[] ydata)
        {
            graph.Invoke(new PlotYDelegate(p.PlotY), new Object[] { ydata, start, inc });
        }
        private delegate void PlotYAppendDelegate(double[,] zdata, bool invert);
        private void PlotYAppend(IntensityGraph graph, double[,] zdata)
        {
            graph.Invoke(new PlotYAppendDelegate(graph.PlotYAppend), new Object[] { zdata, false });
        }
        private delegate void UpdateStatusBarDelegate();
        private void UpdateStatusBarHelper(MainForm form)
        {
            form.Invoke(new UpdateStatusBarDelegate(form.UpdateStatusBar));
        }
        private delegate void DisAbleRunDelegate();
        private void DisAbleRunHelper(MainForm form)
        {
            form.Invoke(new DisAbleRunDelegate(form.DisAbleRunButton));
        }
        private delegate void EndOfTravelDelegate();
        private void EndOFTravelHelper(MainForm form)
        {
            form.Invoke(new EndOfTravelDelegate(form.EndOfTravelError));
        }
        private delegate void ClearEndOfTravelDelegate();
        private void ClearEndOFTravelHelper(MainForm form)
        {
            form.Invoke(new ClearEndOfTravelDelegate(form.ClearEndOfTravelError));
        }


        public void ChangeLabel()
        {
            labStatus.Text = "Updated";
        }

        private void checkLock_CheckedChanged(object sender, EventArgs e)
        {
            if (checkLock.Enabled == true)
            {
                LockEngaged = true;
            }
            else
            {
                LockEngaged = false;
            }

        
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            controller.ProgState = Controller.ProgramState.STOPPED;
        }

        private void slideLockPoint_AfterChangeValue(object sender, AfterChangeNumericValueEventArgs e)
        {
            controller.LockLevel = slideLockPoint.Value;
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textGain_TextChanged(object sender, EventArgs e)
        {
            double value;
            if (!(textGain.Text == "" || textGain.Text == "-"))
            {
                if (Double.TryParse(textGain.Text, out value))
                {
                    controller.Gain = value;
                }
                else
                {
                    MessageBox.Show("There has been an error in applying the gain settings.",
                        "Settings Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    controller.Gain = 0;
                }
            }
        }
    }
}
