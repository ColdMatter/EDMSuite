using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AlFHardwareControl;
using LatticeHardwareControl;
using System.IO.Ports;



namespace LatticeHardwareControl
{
    public partial class Form1 : Form
    {
        public Program controller;
        public Form1()
        {
            InitializeComponent();
            controller = new Program();
        }

        #region Controls needed for commands in form

        public void SetTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }
        public delegate void SetTextDelegate(TextBox box, string text);

        public void EnableControl(Control control, bool enabled)
        {
            control.Invoke(new EnableControlDelegate(EnableControlHelper), new object[] { control, enabled });
        }
        private delegate void EnableControlDelegate(Control control, bool enabled);
        private void EnableControlHelper(Control control, bool enabled)
        {
            control.Enabled = enabled;
        }

        public void SetTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        public void AddPointToChart(Chart chart, string series, DateTime xpoint, double ypoint)
        {
            chart.Invoke(new AddPointToChartDelegate(AddPointToChartHelper), new object[] { chart, series, xpoint, ypoint });
        }
        private delegate void AddPointToChartDelegate(Chart chart, string series, DateTime xpoint, double ypoint);
        private void AddPointToChartHelper(Chart chart, string series, DateTime xpoint, double ypoint)
        {
            // All charts are for temperature or pressure - these should not be zero or negative.
            // This also has the benefit of allowing logarithmic Y scales to be used on plots, without an exception being thrown.
            chart.Series[series].Points.AddXY(xpoint, ypoint);
        }
        #endregion

        public double flowRateHe = 2.0;
        public double flowRateSF6 = 0.01;
        public bool flowStateHe = false;
        public bool flowStateSF6 = false;

        private void buttonStartPMonitor_Click(object sender, EventArgs e)
        {
            buttonStartPMonitor.Enabled = false;
            buttonStopPMonitor.Enabled = true;
            try {
                controller.StartPTMonitorPoll();
                 }
            catch
            {
                buttonStopPMonitor.PerformClick();
                //SetTextBox(textBoxSourcePressure, "Error");
                //SetTextBox(textBoxDownstreamPressure, "Error");            
            }
            
        }

        private void buttonStopPMonitor_Click(object sender, EventArgs e)
        {
            buttonStopPMonitor.Enabled = false;
            buttonStartPMonitor.Enabled = true;
            controller.StopPTMonitorPoll();
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxSourcePressure_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_Flow_controller_connect_Click(object sender, EventArgs e)
        {
            controller.ConnectFlowControl();
        }

        private void button_Get_Serial_Ports_click(object sender, EventArgs e)
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            do
            {
                index += 1;
                richTextBox_output.Text += ArrayComPortsNames[index] + "\n";
            }
            while (!((ArrayComPortsNames[index] == ComPortName) ||
                                (index == ArrayComPortsNames.GetUpperBound(0))));
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void Flow_Controllers_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void textbox_P_source_scroll_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_On_flow_He_Click(object sender, EventArgs e)
        {
            
            if (!flowStateHe)
            {
                flowStateHe = true;
                controller.AlicatFlowSet("a", flowRateHe.ToString());//the original code
                textBoxHeFlow.BackColor = Color.PaleGreen;
            }
            
            //SetTextBox(textBoxHeFlow, "2.0");
        }

        private void textBoxDownstreamPressure_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_stop_flow_He_Click(object sender, EventArgs e)
        {
            flowStateHe = false;
            controller.AlicatFlowSet("a", "0");
            textBoxHeFlow.BackColor = Color.Pink;
            //SetTextBox(textBoxHeFlow, "0.0");
        }

        private void button_On_flow_SF6_Click(object sender, EventArgs e)
        {
            if (!flowStateSF6)
            {
                flowStateSF6 = true;
                controller.AlicatFlowSet("b", flowRateSF6.ToString());
                textBoxSF6Flow.BackColor = Color.PaleGreen;
            }
            //SetTextBox(textBoxSF6Flow, "0.01");
        }

        private void textBoxHeFlow_TextChanged(object sender, EventArgs e)
        {
            buttonSetHe.BackColor = SystemColors.ControlLightLight;
        }

        private void textBoxSF6Flow_TextChanged(object sender, EventArgs e)
        {
            buttonSetSF6.BackColor = SystemColors.ControlLightLight;
        }

        private void button_stop_flow_SF6_Click(object sender, EventArgs e)
        {
            flowStateSF6 = false;
            controller.AlicatFlowSet("b", "0");
            textBoxSF6Flow.BackColor = Color.Pink;
            //SetTextBox(textBoxSF6Flow, "0.0");
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button_Set_All_Click(object sender, EventArgs e)
        {
            button_On_flow_SF6.PerformClick();
            button_On_flow_He.PerformClick();
        }

        private void button_Clear_All_Click(object sender, EventArgs e)
        {
            button_Off_flow_SF6.PerformClick();
            button_Off_flow_He.PerformClick();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonSetHe_Click(object sender, EventArgs e)
        {
            try
            {
                flowRateHe = Double.Parse(textBoxHeFlow.Text);
                if (flowStateHe)
                {
                    controller.AlicatFlowSet("a", flowRateHe.ToString());
                }
                buttonSetHe.BackColor = SystemColors.GradientActiveCaption;

            }
            catch (Exception)
            {
                textBoxHeFlow.Text = flowRateHe.ToString();
            }
        }

        private void buttonSetSF6_Click(object sender, EventArgs e)
        {
            try
            {
                flowRateSF6 = Double.Parse(textBoxSF6Flow.Text);
                if (flowStateSF6)
                {
                    controller.AlicatFlowSet("b", flowRateSF6.ToString());
                }
                buttonSetSF6.BackColor = SystemColors.GradientActiveCaption;
            }
            catch (Exception)
            {
                textBoxSF6Flow.Text = flowRateSF6.ToString();
            }
        }

        private void textbox_P_dump_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox2_Enter_1(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cryoStatus_TextChanged(object sender, EventArgs e)
        {

        }

        private void AnapicoSetFreqCH1Button_Click(object sender, EventArgs e)
        {
            double freq = Double.Parse(AnapicoSetFreqCH1TextBox.Text);
            controller.SetAnapicoCWFrequencyCH1(freq);
            controller.UpdateAnapicoCWCH1();
            //napicoSetFreqCH1Button.BackColor = SystemColors.GradientActiveCaption;
        }

        private void AnapicoSetFreqCH2Button_Click(object sender, EventArgs e)
        {
            double freq = Double.Parse(AnapicoSetFreqCH2TextBox.Text);
            controller.SetAnapicoCWFrequencyCH2(freq);
            controller.UpdateAnapicoCWCH2();
            //AnapicoSetFreqCH2Button.BackColor = SystemColors.GradientActiveCaption;
        }

        private void anapicoEnableButton_Click(object sender, EventArgs e)
        {
            controller.EnableAnapico(true);
            anaPicoLED.Value=true;
        }

        private void anapicoDisableButton_Click(object sender, EventArgs e)
        {
            controller.EnableAnapico(false);
            anaPicoLED.Value = false;
        }

        private void AnapicoSetFMDevCH1Button_Click(object sender, EventArgs e)
        {
            double dev = Double.Parse(AnapicoSetFMDevCH1TextBox.Text);
            controller.SetAnapicoFMDeviationCH1(dev);
            controller.UpdateAnapicoFMDeviationCH1();
            //napicoSetFreqCH1Button.BackColor = SystemColors.GradientActiveCaption;
        }

        private void AnapicoSetFMDevCH2Button_Click(object sender, EventArgs e)
        {
            double dev = Double.Parse(AnapicoSetFMDevCH2TextBox.Text);
            controller.SetAnapicoFMDeviationCH2(dev);
            controller.UpdateAnapicoFMDeviationCH2();
            //napicoSetFreqCH1Button.BackColor = SystemColors.GradientActiveCaption;
        }

        private void CH1FMEnableButton_Click(object sender, EventArgs e)
        {
            controller.EnableFMCH1(true);
        }

        private void CH1FMDisableButton_Click(object sender, EventArgs e)
        {
            controller.EnableFMCH1(false);
        }

        private void CH2FMEnableButton_Click(object sender, EventArgs e)
        {
            controller.EnableFMCH2(true);
        }

        private void CH2FMDisableButton_Click(object sender, EventArgs e)
        {
            controller.EnableFMCH2(false);
        }

        private void label37_Click(object sender, EventArgs e)
        {

        }

        private void AnapicoSetPowerCH2Textbox_TextChanged(object sender, EventArgs e)
        {

        }

        private void AnapicoSetPowerCH1Textbox_TextChanged(object sender, EventArgs e)
        {

        }

        private void AnapicoSetPowerCH1Button_Click(object sender, EventArgs e)
        {
            double power = Double.Parse(AnapicoSetPowerCH1Textbox.Text);
            controller.SetAnapicoPowerCH1(power);
            controller.UpdateAnapicoPowerCH1();
        }

        private void AnapicoSetPowerCH2Button_Click(object sender, EventArgs e)
        {
            double power = Double.Parse(AnapicoSetPowerCH2Textbox.Text);
            controller.SetAnapicoPowerCH2(power);
            controller.UpdateAnapicoPowerCH2();
        }

        private void label38_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }


        private void PulseMode_enable_button_Click(object sender, EventArgs e)
        {
            controller.EnablePulseMode(true);
        }

        private void PulseMode_disable_button_Click(object sender, EventArgs e)
        {
            controller.EnablePulseMode(false);
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void label39_Click(object sender, EventArgs e)
        {

        }

        private void label40_Click(object sender, EventArgs e)
        {

        }

        private void windfreak1SetCH1Frequency(object sender, EventArgs e)
        {
            long freq = long.Parse(WindfreakCH1FrequencyTextbox.Text);
            controller.SetWindfreak1FrequencyCH1(freq);
            controller.UpdateWindfreakFrequencyCH1();
        }

        private void windfreak1SetCH2Frequency(object sender, EventArgs e)
        {
            long freq = long.Parse(WindfreakCH2FrequencyTextbox.Text);
            controller.SetWindfreak1FrequencyCH2(freq);
            controller.UpdateWindfreakFrequencyCH2();
        }

        private void windfreak1SetCH1Power(object sender, EventArgs e)
        {
            double power = Double.Parse(WindfreakCH1PowerTextbox.Text);
            controller.SetWindfreak1PowerCH1(power);
            controller.UpdateWindfreakPowerCH1();
        }

        private void windfreak1SetCH2Power(object sender, EventArgs e)
        {
            double power = Double.Parse(WindfreakCH2PowerTextbox.Text);
            controller.SetWindfreak1PowerCH2(power);
            controller.UpdateWindfreakPowerCH2();
        }


        private void EnableWindfreak1CH1_button(object sender, EventArgs e)
        {
            controller.EnableWindfreak1CH1(true);
            windFreak1CH1LED.Value = true;
        }

        private void DisableWindfreak1CH1_button(object sender, EventArgs e)
        {
            controller.EnableWindfreak1CH1(false);
            windFreak1CH1LED.Value = false;
        }

        private void DisableWindfreak1CH2_button(object sender, EventArgs e)
        {
            controller.EnableWindfreak1CH2(false);
            windFreak1CH2LED.Value = false;
        }

        private void EnableWindfreak1CH2_button(object sender, EventArgs e)
        {
            controller.EnableWindfreak1CH2(true);
            windFreak1CH2LED.Value = true;
        }

        private void label47_Click(object sender, EventArgs e)
        {

        }

        private void Windfreak2EnableCH1Button_Click(object sender, EventArgs e)
        {
            controller.EnableWindfreak2CH1(true);
            windFreak2CH1LED.Value = true;
        }

        private void Windfreak2CH1SetFrequencyButton_Click(object sender, EventArgs e)
        {
            long freq = long.Parse(Windfreak2CH1FrequencyTextbox.Text);
            controller.SetWindfreak2FrequencyCH1(freq);
            controller.UpdateWindfreak2FrequencyCH1();
        }

        private void Windfreak2DisableCH1Button_Click(object sender, EventArgs e)
        {
            controller.EnableWindfreak2CH1(false);
            windFreak2CH1LED.Value = false;
        }

        private void Windfreak2EnableCH2Button_Click(object sender, EventArgs e)
        {
            controller.EnableWindfreak2CH2(true);
            windFreak2CH2LED.Value = true;
        }

        private void Windfreak2DisableCH2Button_Click(object sender, EventArgs e)
        {
            controller.EnableWindfreak2CH2(false);
            windFreak2CH2LED.Value = false;
        }

        private void Windfreak2CH2SetFrequencyButton_Click(object sender, EventArgs e)
        {
            long freq = long.Parse(Windfreak2CH2FrequencyTextbox.Text);
            controller.SetWindfreak2FrequencyCH2(freq);
            controller.UpdateWindfreak2FrequencyCH2();
        }

        private void Windfreak2CH1SetPowerButton_Click(object sender, EventArgs e)
        {
            double power = Double.Parse(Windfreak2CH1PowerTextbox.Text);
            controller.SetWindfreak2PowerCH1(power);
            controller.UpdateWindfreak2PowerCH1();
        }

        private void Windfreak2CH2SetPowerButton_Click(object sender, EventArgs e)
        {
            double power = Double.Parse(Windfreak2CH2PowerTextbox.Text);
            controller.SetWindfreak2PowerCH2(power);
            controller.UpdateWindfreak2PowerCH2();
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.URL = "D:\\EDMSuite\\LatticeEDMtest\\franz_lang_yodelling_auf_und_auf_voll_lebenslust.mp4";
        }

        private void led1_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void Windfreak2CH1FrequencyTextbox_TextChanged(object sender, EventArgs e)
        {

        }

        private void AnapicoSetFreqCH1TextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void AnapicoSetFMDevCH1TextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
