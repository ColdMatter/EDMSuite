using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NationalInstruments.UI.WindowsForms;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

namespace RfArbitraryWaveformGenerator
{
    /// <summary>
    /// Front panel of the rf arbitrary waveform generator (NI PXI-5672)
    /// </summary>
    public partial class MainForm : Form
    {
        public Controller controller;

        #region Setup

        public MainForm()
        {
            InitializeComponent();
            LoadRfsgDeviceNames();
            ConfigureTrigger1TypeComboBox();
            ConfigureTrigger2TypeComboBox();
            ConfigureTrigger1SourceComboBox();
            ConfigureTrigger2SourceComboBox();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void LoadRfsgDeviceNames()
        {
            // Fix this later
        }

        #endregion

        #region UI Initial Value Config Section

        private void ConfigureTrigger2TypeComboBox()
        {
            trigger2TypeComboBox.Items.Add(RfsgScriptTriggerType.None);
            trigger2TypeComboBox.Items.Add(RfsgScriptTriggerType.DigitalEdge);
            trigger2TypeComboBox.Items.Add(RfsgScriptTriggerType.DigitalLevel);
            trigger2TypeComboBox.SelectedIndex = 0;
        }

        private void ConfigureTrigger1TypeComboBox()
        {
            trigger1TypeComboBox.Items.Add(RfsgScriptTriggerType.None);
            trigger1TypeComboBox.Items.Add(RfsgScriptTriggerType.DigitalEdge);
            trigger1TypeComboBox.Items.Add(RfsgScriptTriggerType.DigitalLevel);
            trigger1TypeComboBox.SelectedIndex = 0;
        }

        private void ConfigureTrigger2SourceComboBox()
        {
            var triggerSource2ValueList = new List<DictionaryEntry>();
            triggerSource2ValueList.Add(new DictionaryEntry("PFI0", RfsgDigitalEdgeScriptTriggerSource.Pfi0));
            triggerSource2ValueList.Add(new DictionaryEntry("PFI1", RfsgDigitalEdgeScriptTriggerSource.Pfi1));

            trigger2SourceComboBox.DataSource = triggerSource2ValueList;
            trigger2SourceComboBox.DisplayMember = "Key";
            trigger2SourceComboBox.ValueMember = "Value";
            trigger2SourceComboBox.SelectedIndex = 1;

        }

        private void ConfigureTrigger1SourceComboBox()
        {
            var triggerSource1ValueList = new List<DictionaryEntry>();
            triggerSource1ValueList.Add(new DictionaryEntry("PFI0", RfsgDigitalEdgeScriptTriggerSource.Pfi0));
            triggerSource1ValueList.Add(new DictionaryEntry("PFI1", RfsgDigitalEdgeScriptTriggerSource.Pfi1));

            trigger1SourceComboBox.DataSource = triggerSource1ValueList;
            trigger1SourceComboBox.DisplayMember = "Key";
            trigger1SourceComboBox.ValueMember = "Value";
            trigger1SourceComboBox.SelectedIndex = 0;
        }

        #endregion

        #region ThreadSafe wrappers

        public void SetNumeric(NumericUpDown numeric, decimal value)
        {
            numeric.Invoke(new SetNumericDelegate(SetNumericHelper), new object[] { numeric, value });
        }
        private delegate void SetNumericDelegate(NumericUpDown numeric, decimal value);
        private void SetNumericHelper(NumericUpDown numeric, decimal value)
        {
            numeric.Value = value;
        }

        public void SetCheckBox(CheckBox box, bool state)
        {
            box.Invoke(new SetCheckDelegate(SetCheckHelper), new object[] { box, state });
        }
        private delegate void SetCheckDelegate(CheckBox box, bool state);
        private void SetCheckHelper(CheckBox box, bool state)
        {
            box.Checked = state;
        }

        public void SetTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }
        private delegate void SetTextDelegate(TextBox box, string text);
        private void SetTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        public void AppendTextBox(TextBox box, string text)
        {
            box.Invoke(new AppendTextDelegate(AppendTextHelper), new object[] { box, text });
        }
        private delegate void AppendTextDelegate(TextBox box, string text);
        private void AppendTextHelper(TextBox box, string text)
        {
            box.AppendText(text);
        }

        public void SetComboBox(ComboBox box, object obj)
        {
            box.Invoke(new SetComboBoxDelegate(SetComboBoxHelper), new object[] { box, obj });
        }
        private delegate void SetComboBoxDelegate(ComboBox box, object obj);
        private void SetComboBoxHelper(ComboBox box, object obj)
        {
            box.SelectedItem = obj;
        }

        public void SetLED(Led led, bool val)
        {
            led.Invoke(new SetLedDelegate(SetLedHelper), new object[] { led, val });
        }
        private delegate void SetLedDelegate(Led led, bool val);
        private void SetLedHelper(Led led, bool val)
        {
            led.Value = val;
        }

        public void EnableControl(Control control, bool enabled)
        {
            control.Invoke(new EnableControlDelegate(EnableControlHelper), new object[] { control, enabled });
        }
        private delegate void EnableControlDelegate(Control control, bool enabled);
        private void EnableControlHelper(Control control, bool enabled)
        {
            control.Enabled = enabled;
        }

        #endregion

        #region Set/get parameter values

        public void SetFrequency(double value)
        {
            SetNumeric(frequencyNumeric, (decimal)value);
        }
        public double GetFrequency()
        {
            return (double)frequencyNumeric.Value;
        }

        public void SetPowerLevel(double value)
        {
            SetNumeric(powerLevelNumeric, (decimal)value);
        }
        public double GetPowerLevel()
        {
            return (double)powerLevelNumeric.Value;
        }

        public void SetSignalBandwidth(double value)
        {
            SetNumeric(signalBandwidthNumeric, (decimal)value);
        }
        public double GetSignalBandwidth()
        {
            return (double)signalBandwidthNumeric.Value;
        }

        public void SetPulseDuration(double value)
        {
            SetNumeric(pulseDurationNumeric, (decimal)value);
        }
        public double GetPulseDuration()
        {
            return (double)pulseDurationNumeric.Value;
        }

        public void SetTrigger1Type(RfsgScriptTriggerType triggerType)
        {
            SetComboBox(trigger1TypeComboBox, triggerType);
        }
        public RfsgScriptTriggerType GetTrigger1Type()
        {
            return (RfsgScriptTriggerType)trigger1TypeComboBox.SelectedItem;
        }

        public void SetTrigger2Type(RfsgScriptTriggerType triggerType)
        {
            SetComboBox(trigger2TypeComboBox, triggerType);
        }
        public RfsgScriptTriggerType GetTrigger2Type()
        {
            return (RfsgScriptTriggerType)trigger2TypeComboBox.SelectedItem;
        }

        public void SetTrigger1Source(string triggerSource)
        {
            SetComboBox(trigger1SourceComboBox, triggerSource);
        }
        public string GetTrigger1Source()
        {
            return (string)trigger1SourceComboBox.Text;
        }

        public void SetTrigger2Source(string triggerSource)
        {
            SetComboBox(trigger2SourceComboBox, triggerSource);
        }
        public string GetTrigger2Source()
        {
            return (string)trigger2SourceComboBox.Text;
        }

        public void Seta0(double a0)
        {
            SetTextBox(a0TextBox, a0.ToString());
        }
        public double Geta0()
        {
            return Convert.ToDouble(a0TextBox.Text);
        }

        public void Seta1(double a1)
        {
            SetTextBox(a1TextBox, a1.ToString());
        }
        public double Geta1()
        {
            return Convert.ToDouble(a1TextBox.Text);
        }

        public void Seta2(double a2)
        {
            SetTextBox(a2TextBox, a2.ToString());
        }
        public double Geta2()
        {
            return Convert.ToDouble(a2TextBox.Text);
        }

        public void Seta3(double a3)
        {
            SetTextBox(a3TextBox, a3.ToString());
        }
        public double Geta3()
        {
            return Convert.ToDouble(a3TextBox.Text);
        }

        public void SetFrequencyStep(double freqStep)
        {
            SetTextBox(fmTextBox, freqStep.ToString());
        }
        public double GetFrequencyStep()
        {
            return Convert.ToDouble(fmTextBox.Text);
        }

        public void SetPhaseStep(double phaseStep)
        {
            SetTextBox(pmTextBox, phaseStep.ToString());
        }
        public double GetPhaseStep()
        {
            return Convert.ToDouble(pmTextBox.Text);
        }

        public void SetPulseName(string pulseName)
        {
            SetTextBox(pulseNameTextBox, pulseName);
        }
        public string GetPulseName()
        {
            return pulseNameTextBox.Text;
        }

        public void SetScriptFile(string filePath)
        {
            SetTextBox(scriptFileTextBox, filePath);
        }
        public string GetScriptFile()
        {
            return scriptFileTextBox.Text;
        }

        public void DisplayScript(string script)
        {
            SetTextBox(scriptTextBox, script);
        }
        public string GetScript()
        {
            return scriptTextBox.Text;
        }

        public string GetResourceName()
        {
            return resourceNameTextBox.Text;
        }

        public string GetPulseDirectory()
        {
            return pulseDirectoryTextBox.Text;
        }

        public double GetRf1StartTime()
        {
            return Convert.ToDouble(rf1StartTimeTextBox.Text);
        }

        public double GetRf2StartTime()
        {
            return Convert.ToDouble(rf2StartTimeTextBox.Text);
        }

        public double GetRf1PulseDuration()
        {
            return Convert.ToDouble(rf1PulseDurationTextBox.Text);
        }

        public double GetRf2PulseDuration()
        {
            return Convert.ToDouble(rf2PulseDurationTextBox.Text);
        }

        public RfPulse GetRf1Pulse()
        {
            return (RfPulse)rf1PulseComboBox.SelectedValue;
        }

        public RfPulse GetRf2Pulse()
        {
            return (RfPulse)rf2PulseComboBox.SelectedValue;
        }


        #endregion

        #region Display data

        public void PopulatePulseList(Dictionary<object, RfPulse> pulseList)
        {
            rf1PulseComboBox.DataSource = new BindingSource(pulseList, null);
            rf1PulseComboBox.DisplayMember = "Key";
            rf1PulseComboBox.ValueMember = "Value";
            rf1PulseComboBox.SelectedIndex = 0;

            rf2PulseComboBox.DataSource = new BindingSource(pulseList, null);
            rf2PulseComboBox.DisplayMember = "Key";
            rf2PulseComboBox.ValueMember = "Value";
            rf2PulseComboBox.SelectedIndex = 0;
        }

        private void rf1PulseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RfPulse pulse = ((KeyValuePair<object, RfPulse>)rf1PulseComboBox.SelectedItem).Value;
            DisplayRf1PulseData(pulse);
        }

        public void DisplayRf1PulseData(RfPulse pulse)
        {
            SetTextBox(rf1PulseDurationTextBox, (pulse.PulseLength * Math.Pow(10, 6)).ToString());
            SetTextBox(rf1TotalSamplesTextBox, pulse.TotalSamples.ToString());
        }

        private void rf2PulseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RfPulse pulse = ((KeyValuePair<object, RfPulse>)rf2PulseComboBox.SelectedItem).Value;
            DisplayRf2PulseData(pulse);
        }

        public void DisplayRf2PulseData(RfPulse pulse)
        {
            SetTextBox(rf2PulseDurationTextBox, (pulse.PulseLength * Math.Pow(10, 6)).ToString());
            SetTextBox(rf2TotalSamplesTextBox, pulse.TotalSamples.ToString());
        }

        public void DisplayErrorMessage(string errorMessage)
        {
            AppendTextBox(errorMessagesTextBox, errorMessage + Environment.NewLine);
        }

        public void SetResourceName(string resourceName)
        {
            SetTextBox(resourceNameTextBox, resourceName);
        }


        #endregion

        #region Form events

        private void startButton_Click(object sender, EventArgs e)
        {
            controller.StartGeneration();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            controller.StopGeneration();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.StopGeneration();
        }

        private void scriptFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";
            openFileDialog.Title = "Select a waveform file...";
            openFileDialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(scriptFileTextBox.Text));
            openFileDialog.FileName = Path.GetFileName(scriptFileTextBox.Text);
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                scriptFileTextBox.Text = openFileDialog.FileName;
                string script = File.ReadAllText(scriptFileTextBox.Text);
                DisplayScript(script);
            }
            openFileDialog.Dispose();
        }
        private void pulseDirectoryButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the folder containing the pulses...";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                pulseDirectoryTextBox.Text = folderBrowserDialog.SelectedPath;
            }
            folderBrowserDialog.Dispose();
        }

        private void loadPulsesButton_Click(object sender, EventArgs e)
        {
            controller.LoadPulsesFromFile(pulseDirectoryTextBox.Text);
        }

        private void savePulseButton_Click(object sender, EventArgs e)
        {
            controller.SavePulseToFile();
        }

        private void selectPulsesButton_Click(object sender, EventArgs e)
        {
            controller.SelectPulses();
        }

        private void rfsgStatusTimer_Tick(object sender, EventArgs e)
        {
            controller.CheckGeneration();
        }

        #endregion

        #region Enable form controls

        public void EnableStartButton(bool enabled)
        {
            EnableControl(startButton, enabled);
        }
        
        public void EnableControls(bool enabled)
        {
            EnableControl(startButton, enabled);
            EnableControl(stopButton, !enabled);
            EnableControl(loadPulsesButton, enabled);
            EnableControl(frequencyNumeric, enabled);
            EnableControl(powerLevelNumeric, enabled);
            EnableControl(pulseDurationNumeric, enabled);
            EnableControl(signalBandwidthNumeric, enabled);
            EnableControl(trigger1TypeComboBox, enabled);
            EnableControl(trigger2TypeComboBox, enabled);
            EnableControl(trigger1SourceComboBox, enabled);
            EnableControl(trigger2SourceComboBox, enabled);
            EnableControl(scriptFileTextBox, enabled);
            EnableControl(scriptFileButton, enabled);
            EnableControl(pulseDirectoryTextBox, enabled);
            EnableControl(pulseDirectoryButton, enabled);
            EnableControl(rf1PulseComboBox, enabled);
            EnableControl(rf2PulseComboBox, enabled);
            EnableControl(rf1StartTimeTextBox, enabled);
            EnableControl(rf2StartTimeTextBox, enabled);
            EnableControl(selectPulsesButton, enabled);
            rfsgStatusTimer.Enabled = !enabled;

            Application.DoEvents();
        }

        #endregion



    }
}
