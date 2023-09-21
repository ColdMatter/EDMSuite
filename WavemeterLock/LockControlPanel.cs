using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WavemeterLock
{
    public partial class LockControlPanel : UserControl
    {

        public string name;
        public string analogChannel;
        private int channelNumber = 0;
        public Controller controller;
        public double scale = 10;
        public bool lockBlocked = false;

        public LockControlPanel(string name, string AnalogChannel, int wavemeterChannel, Controller controller)
        {
            this.name = name;
            this.controller = controller;
            analogChannel = AnalogChannel;
            channelNumber = wavemeterChannel;
            InitializeComponent();
            controller.panelList.Add(name, this);
            lockChannelNum.Text = Convert.ToString(channelNumber);
            PGain.Text = controller.lasers[name].PGain.ToString();
            IGain.Text = controller.lasers[name].IGain.ToString();

        }


        private void LockControlPanel_Load(object sender, EventArgs e)
        {
            errorPlot.XAxis.Range = new NationalInstruments.UI.Range(0, scale);
            errorPlot.LineColor = controller.selectColor(controller.colorParameter);
            controller.colorParameter++;
            //controller.lasers[name].setFrequency = Math.Round(controller.getFrequency(channelNumber),6);
            SetPoint.Text = Convert.ToString(controller.lasers[name].setFrequency);
            labelOutOfRange.Visible = false;
        }

        public void updatePanel()
        {
            displayWL.Text = controller.displayWL(channelNumber);
            displayFreq.Text = controller.displayFreq(channelNumber);
            lockMsg.Text = controller.getLaserState(name);
            frequencyError.Text = Convert.ToString(Math.Round(1000000 * controller.gerFrequencyError(name),6));
            VOut.Text = Convert.ToString(Math.Round(controller.getOutputvoltage(name),6));
            if (controller.lasers[name].lState == Laser.LaserState.LOCKED)
            {
                SetPoint.Text = Convert.ToString(controller.lasers[name].setFrequency);
                RMSValue.Text = controller.lasers[name].RMSNoise.ToString("#.###");
            }

            else
                RMSValue.Text = "N/A";

            if (controller.lasers[name].isOutOfRange)
            {
                labelOutOfRange.Visible = true;
            }
            else labelOutOfRange.Visible = false;

            if (!controller.returnLaserState(name))//Not locked
            {
                lockButton.Text = "Lock";
                lockLED.Value = false;
                SetPoint.Enabled = true;
                setAsReading.Enabled = true;
                offsetSet.Enabled = true;
            }
            else //Locked
            {
                lockButton.Text = "Unlock";
                lockLED.Value = true;
                SetPoint.Enabled = false;
                setAsReading.Enabled = false;
                offsetSet.Enabled = false;
            }
           

        }

        public void updateLockBlockStatus(bool status)
        {
            lockBlocked = status;
            LEDBlockIndicator.Value = status;
        }

        public void SetTextField(Control box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }

        private delegate void SetTextDelegate(Control box, string text);

        private void SetTextHelper(Control box, string text)
        {
            box.Text = text;
        }
        #region Events

        private void lockButton_Click(object sender, EventArgs e)
        {
            controller.lasers[name].setFrequency = Convert.ToDouble(SetPoint.Text);
            controller.setFrequency(name, controller.lasers[name].setFrequency);

            if (!controller.returnLaserState(name))
            {
                if (SetPoint != null && !string.IsNullOrWhiteSpace(SetPoint.Text))
                {
                    lockMsg.Text = "Lock On";
                    lockLED.Value = true;
                    controller.indicateRemoteConnection(channelNumber, true);
                    controller.lasers[name].setFrequency = Convert.ToDouble(SetPoint.Text);
                    controller.EngageLock(name);
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
                controller.indicateRemoteConnection(channelNumber, false);
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
        private void offsetSet_Click(object sender, EventArgs e)
        {
            double g = Convert.ToDouble(offset.Text);
            controller.lasers[name].offsetVoltage = g;
            controller.resetOutput(name);
        }

        private void setAsReading_Click(object sender, EventArgs e)
        {
            double currentFreq = Math.Round(controller.getFrequency(channelNumber),6);
            controller.lasers[name].setFrequency = currentFreq;
            SetPoint.Text = Convert.ToString(currentFreq);
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            controller.resetOutput(name);
        }

        private void resetGraph_Click(object sender, EventArgs e)
        {
            UIHelper.ClearGraph(errorScatterGraph);
            controller.timeList[name] = 0;
            controller.lasers[name].sumedNoise = 0.0;
            controller.lasers[name].loopCount = 0;
            controller.lasers[name].RMSNoise = 0.0;
        }

        private void stepUpBtn_Click(object sender, EventArgs e)
        {
            controller.lasers[name].setFrequency += Convert.ToDouble(stepSize.Text) / 1000000;
        }

        private void stepDownBtn_Click(object sender, EventArgs e)
        {
            controller.lasers[name].setFrequency -= Convert.ToDouble(stepSize.Text) / 1000000;
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

        private void controlPanel_Enter(object sender, EventArgs e)
        {

        }

        private void groupBoxLaserInfo_Enter(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }
    }
}
