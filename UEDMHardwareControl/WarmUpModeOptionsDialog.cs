using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UEDMHardwareControl
{
    public partial class WarmUpModeOptionsDialog : Form
    {
        public WarmUpModeOptionsDialog()
        {
            InitializeComponent();

            InitializeOptionsValues();

            btCancel.DialogResult = DialogResult.Cancel;
        }

        // Parameters
        internal string TurbomolecularPumpUpperPressureLimitInitialTextValue;
        internal string TurbomolecularPumpUpperPressureLimitTextValue;
        public double TurbomolecularPumpUpperPressureLimitDoubleValue;

        internal string GasEvaporationCycleTemperatureMaxInitialTextValue;
        internal string GasEvaporationCycleTemperatureMaxTextValue;
        public double GasEvaporationCycleTemperatureMaxDoubleValue;

        internal string DesorbingPTPollPeriodInitialTextValue;
        internal string DesorbingPTPollPeriodTextValue;
        public int DesorbingPTPollPeriodIntValue;

        internal string CryoStoppingPressureInitialTextValue;
        internal string CryoStoppingPressureTextValue;
        public double CryoStoppingPressureDoubleValue;

        internal string WarmupPTPollPeriodInitialTextValue;
        internal string WarmupPTPollPeriodTextValue;
        public int WarmupPTPollPeriodIntValue;

        internal string SourceModeWaitPTPollPeriodInitialTextValue;
        internal string SourceModeWaitPTPollPeriodTextValue;
        public int SourceModeWaitPTPollPeriodIntValue;

        // Flags
        public bool ParseFailFlag = false;
        public bool WarmupConstantChangedFlag = false;

        internal void InitializeOptionsValues()
        {
            // General constants
            TurbomolecularPumpUpperPressureLimitInitialTextValue = UEDMController.SourceWarmUpConstants.TurbomolecularPumpUpperPressureLimit.ToString("E3");

            // Desorbing process
            GasEvaporationCycleTemperatureMaxInitialTextValue = UEDMController.SourceWarmUpConstants.GasEvaporationCycleTemperatureMax.ToString("E3");
            DesorbingPTPollPeriodInitialTextValue = UEDMController.SourceWarmUpConstants.DesorbingPTPollPeriod.ToString();

            // Warmup
            CryoStoppingPressureInitialTextValue = UEDMController.SourceWarmUpConstants.CryoStoppingPressure.ToString("E3");
            WarmupPTPollPeriodInitialTextValue = UEDMController.SourceWarmUpConstants.WarmupPTPollPeriod.ToString();

            // Wait at target temperature
            SourceModeWaitPTPollPeriodInitialTextValue = UEDMController.SourceWarmUpConstants.SourceModeWaitPTPollPeriod.ToString();

            ResetTextBoxValues();
        }

        internal void ResetTextBoxValues()
        {
            // General constants
            textBoxTurbomolecularPumpUpperPressureLimit.Text = TurbomolecularPumpUpperPressureLimitInitialTextValue;

            // Desorbing process
            textBoxGasDesorbingTemperatureMax.Text = GasEvaporationCycleTemperatureMaxInitialTextValue;
            textBoxDesorbingPTPollPeriod.Text = DesorbingPTPollPeriodInitialTextValue;

            // Warmup
            textBoxCryoStoppingPressure.Text = CryoStoppingPressureInitialTextValue;
            textBoxWarmupPTPollPeriod.Text = WarmupPTPollPeriodInitialTextValue;

            // Wait at target temperature
            textBoxSourceModeWaitPTPollPeriod.Text = SourceModeWaitPTPollPeriodInitialTextValue;

            // Reset text changed flag
            WarmupConstantChangedFlag = false;
        }

        private void ProcessOptions()
        {
            // TurbomolecularPumpUpperPressureLimit
            TurbomolecularPumpUpperPressureLimitTextValue = textBoxTurbomolecularPumpUpperPressureLimit.Text;
            if (!Double.TryParse(TurbomolecularPumpUpperPressureLimitTextValue, out TurbomolecularPumpUpperPressureLimitDoubleValue))
            {
                ParseFailFlag = true;
            }
            // GasEvaporationCycleTemperatureMax
            GasEvaporationCycleTemperatureMaxTextValue = textBoxGasDesorbingTemperatureMax.Text;
            if (!Double.TryParse(GasEvaporationCycleTemperatureMaxTextValue, out GasEvaporationCycleTemperatureMaxDoubleValue))
            {
                ParseFailFlag = true;
            }
            // DesorbingPTPollPeriod
            DesorbingPTPollPeriodTextValue = textBoxDesorbingPTPollPeriod.Text;
            if (!Int32.TryParse(DesorbingPTPollPeriodTextValue, out DesorbingPTPollPeriodIntValue))
            {
                ParseFailFlag = true;
            }
            // CryoStoppingPressure
            CryoStoppingPressureTextValue = textBoxCryoStoppingPressure.Text;
            if (!Double.TryParse(CryoStoppingPressureTextValue, out CryoStoppingPressureDoubleValue))
            {
                ParseFailFlag = true;
            }
            // WarmupPTPollPeriod
            WarmupPTPollPeriodTextValue = textBoxWarmupPTPollPeriod.Text;
            if (!Int32.TryParse(WarmupPTPollPeriodTextValue, out WarmupPTPollPeriodIntValue))
            {
                ParseFailFlag = true;
            }
            // SourceModeWaitPTPollPeriod
            SourceModeWaitPTPollPeriodTextValue = textBoxSourceModeWaitPTPollPeriod.Text;
            if (!Int32.TryParse(SourceModeWaitPTPollPeriodTextValue, out SourceModeWaitPTPollPeriodIntValue))
            {
                ParseFailFlag = true;
            }

            if (ParseFailFlag)
            {
                var res = MessageBox.Show("Unable to parse string. Ensure that a number has been written, with no additional non-numeric characters.\n\nWould you like to try again?", "", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    ParseFailFlag = false;
                }
                else DialogResult = DialogResult.Cancel;
            }
            else DialogResult = DialogResult.OK;
        }

        private void btSaveSettings_Click(object sender, EventArgs e)
        {
            ProcessOptions();
        }


        private void textBoxTurbomolecularPumpUpperPressureLimit_TextChanged(object sender, EventArgs e)
        {
            WarmupConstantChangedFlag = true;
        }

        private void textBoxGasDesorbingTemperatureMax_TextChanged(object sender, EventArgs e)
        {
            WarmupConstantChangedFlag = true;
        }

        private void textBoxDesorbingPTPollPeriod_TextChanged(object sender, EventArgs e)
        {
            WarmupConstantChangedFlag = true;
        }

        private void textBoxCryoStoppingPressure_TextChanged(object sender, EventArgs e)
        {
            WarmupConstantChangedFlag = true;
        }

        private void textBoxWarmupPTPollPeriod_TextChanged(object sender, EventArgs e)
        {
            WarmupConstantChangedFlag = true;
        }

        private void textBoxSourceModeWaitPTPollPeriod_TextChanged(object sender, EventArgs e)
        {
            WarmupConstantChangedFlag = true;
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            ResetTextBoxValues();
        }
    }
}
