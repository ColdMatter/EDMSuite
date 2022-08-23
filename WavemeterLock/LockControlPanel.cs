using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace WavemeterLock
{
    public partial class LockControlPanel : UserControl
    {

        public string name;
        public string analogChannel;
        private int channelNumber = 0;
        double setFrequency = 0;
        public Controller controller;
        public double scale = 10000;
        

        public LockControlPanel(string name, string AnalogChannel, int wavemeterChannel, Controller controller)
        {
            this.name = name;
            this.controller = controller;
            analogChannel = AnalogChannel;
            channelNumber = wavemeterChannel;
            InitializeComponent();
            controller.panelList.Add(name, this);
            lockChannelNum.Text = Convert.ToString(channelNumber);

        }


        private void LockControlPanel_Load(object sender, EventArgs e)
        {
            errorPlot.XAxis.Range = new NationalInstruments.UI.Range(0, scale);
            errorPlot.LineColor = controller.selectColor(controller.colorParameter);
            controller.colorParameter++;
            setFrequency = Math.Round(controller.getFrequency(channelNumber),6);
            SetPoint.Text = Convert.ToString(setFrequency);
        }

        public void updatePanel()
        {
            displayWL.Text = controller.displayWL(channelNumber);
            displayFreq.Text = controller.displayFreq(channelNumber);
            lockMsg.Text = controller.getLaserState(name);
            frequencyError.Text = Convert.ToString(1000000 * controller.gerFrequencyError(name));
            VOut.Text = Convert.ToString(controller.getOutputvoltage(name));

            if (!controller.returnLaserState(name))
            {
                lockButton.Text = "Lock";
                lockLED.Value = false;
                SetPoint.Enabled = true;
            }
            else
            {
                lockButton.Text = "Unlock";
                lockLED.Value = true;
                SetPoint.Enabled = false;
            }

        }

        #region Events

        private void lockButton_Click(object sender, EventArgs e)
        {
            setFrequency = Convert.ToDouble(SetPoint.Text);
            controller.setFrequency(name, setFrequency);

            if (!controller.returnLaserState(name))
            {
                if (SetPoint != null && !string.IsNullOrWhiteSpace(SetPoint.Text))
                {
                    lockMsg.Text = "Lock On";
                    lockLED.Value = true;
                    setFrequency = Convert.ToDouble(SetPoint.Text);
                    controller.EngageLock(name);
                    ClearErrorGraph();
                }
                else
                {
                    lockMsg.Text = "Null Input!";
                }
            }

            else
            {
                lockMsg.Text = "Lock Off";
                lockLED.Value = false;
                controller.DisengageLock(name);
            }


        }

        

        private void PGainSet_Click(object sender, EventArgs e)
        {
            controller.setPGain(name, Convert.ToDouble(PGain.Text));
        }

        private void IGainSet_Click(object sender, EventArgs e)
        {
            controller.setIGain(name, Convert.ToDouble(IGain.Text));
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            controller.resetOutput(name);
        }

        private void resetGraph_Click(object sender, EventArgs e)
        {
            UIHelper.ClearGraph(errorScatterGraph);
        }

        private void stepUpBtn_Click(object sender, EventArgs e)
        {
            setFrequency += Convert.ToDouble(stepSize.Text) / 1000000;
            controller.setFrequency(name, setFrequency);
            SetPoint.Text = Convert.ToString(setFrequency);
        }

        private void stepDownBtn_Click(object sender, EventArgs e)
        {
            setFrequency -= Convert.ToDouble(stepSize.Text) / 1000000;
            controller.setFrequency(name, setFrequency);
            SetPoint.Text = Convert.ToString(setFrequency);
        }

        private void scaleUp_click(object sender, EventArgs e)
        {
            scale *= 0.25;
            errorPlot.XAxis.Range = new NationalInstruments.UI.Range(0, scale);
        }

        private void scaleDown_click(object sender, EventArgs e)
        {
            scale *= 4;
            errorPlot.XAxis.Range = new NationalInstruments.UI.Range(0, scale);
            
        }

        #endregion


        #region Error signal Plot
        public void AppendToErrorGraph(double lockCount, double error)//In MHz
        {
            UIHelper.appendPointToScatterGraph(errorScatterGraph, errorPlot, lockCount, error);
        }

        public void ClearErrorGraph()
        {
            UIHelper.ClearGraph(errorScatterGraph);
        }
            #endregion
            private void errorScatterGraph_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {
        }
    }
}
