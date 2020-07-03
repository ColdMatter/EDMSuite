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
    public partial class CooldownModeOptionsDialog : Form
    {
        public CooldownModeOptionsDialog()
        {
            InitializeComponent();

            InitializeOptionsValues();

            btCancel.DialogResult = DialogResult.Cancel;
        }

        // Options
        internal string TurbomolecularPumpUpperPressureLimitInitialTextValue;
        internal string TurbomolecularPumpUpperPressureLimitTextValue;
        public double TurbomolecularPumpUpperPressureLimitDoubleValue;

        internal string WarmupPTPollPeriodInitialTextValue;
        internal string WarmupPTPollPeriodTextValue;
        public int WarmupPTPollPeriodIntValue;

        internal string SourceModeWaitPTPollPeriodInitialTextValue;
        internal string SourceModeWaitPTPollPeriodTextValue;
        public int SourceModeWaitPTPollPeriodIntValue;

        internal string CoolDownPTPollPeriodInitialTextValue;
        internal string CoolDownPTPollPeriodTextValue;
        public int CoolDownPTPollPeriodIntValue;

        internal string CryoStartingPressureInitialTextValue;
        internal string CryoStartingPressureTextValue;
        public double CryoStartingPressureDoubleValue;

        internal string CryoStartingTemperatureMaxInitialTextValue;
        internal string CryoStartingTemperatureMaxTextValue;
        public double CryoStartingTemperatureMaxDoubleValue;

        // Conditional parameters
        internal double CryoMaxTemperatureWhenTurnedOff;
        internal int PTPollPeriodMinimum;

        // Flags
        public bool ParseFailFlag = false;
        public bool CooldownConstantChangedFlag = false;
        internal bool CancelOptionsChangeFlag = false;

        internal void InitializeOptionsValues()
        {
            // General constants
            TurbomolecularPumpUpperPressureLimitInitialTextValue = UEDMController.SourceCoolDownConstants.TurbomolecularPumpUpperPressureLimit.ToString("E3");

            // Warmup
            WarmupPTPollPeriodInitialTextValue = UEDMController.SourceCoolDownConstants.WarmupPTPollPeriod.ToString();

            // Wait at target temperature
            SourceModeWaitPTPollPeriodInitialTextValue = UEDMController.SourceCoolDownConstants.SourceModeWaitPTPollPeriod.ToString();

            // Cool down source
            CoolDownPTPollPeriodInitialTextValue = UEDMController.SourceCoolDownConstants.CoolDownPTPollPeriod.ToString();
            CryoStartingPressureInitialTextValue = UEDMController.SourceCoolDownConstants.CryoStartingPressure.ToString("E3");
            CryoStartingTemperatureMaxInitialTextValue = UEDMController.SourceCoolDownConstants.CryoStartingTemperatureMax.ToString("E3");

            // Other parameters
            CryoMaxTemperatureWhenTurnedOff = UEDMController.SourceCoolDownConstants.CryoMaxTemperatureWhenTurnedOff;
            PTPollPeriodMinimum = UEDMController.SourceCoolDownConstants.PTPollPeriodMinimum;

            ResetTextBoxValues();
        }

        internal void ResetTextBoxValues()
        {
            // General constants
            textBoxTurbomolecularPumpUpperPressureLimit.Text = TurbomolecularPumpUpperPressureLimitInitialTextValue;

            // Warmup
            textBoxWarmupPTPollPeriod.Text = WarmupPTPollPeriodInitialTextValue;

            // Wait at target temperature
            textBoxSourceModeWaitPTPollPeriod.Text = SourceModeWaitPTPollPeriodInitialTextValue;

            // Cool down source
            textBoxCoolDownPTPollPeriod.Text = CoolDownPTPollPeriodInitialTextValue;
            textBoxCryoStartingPressure.Text = CryoStartingPressureInitialTextValue;
            textBoxCryoStartingTemperatureMax.Text = CryoStartingTemperatureMaxInitialTextValue;

            // Reset text changed flag
            CooldownConstantChangedFlag = false;
        }

        private void ProcessOptions()
        {
            // Reset flag
            CancelOptionsChangeFlag = false;

            // TurbomolecularPumpUpperPressureLimit
            TurbomolecularPumpUpperPressureLimitTextValue = textBoxTurbomolecularPumpUpperPressureLimit.Text;
            if (!Double.TryParse(TurbomolecularPumpUpperPressureLimitTextValue, out TurbomolecularPumpUpperPressureLimitDoubleValue))
            {
                ParseFailFlag = true;
            }
            // WarmupPTPollPeriod
            WarmupPTPollPeriodTextValue = textBoxWarmupPTPollPeriod.Text;
            if (!Int32.TryParse(WarmupPTPollPeriodTextValue, out WarmupPTPollPeriodIntValue))
            {
                ParseFailFlag = true;
            }
            else
            {
                if (WarmupPTPollPeriodIntValue < PTPollPeriodMinimum)
                {
                    var res = MessageBox.Show("WarmupPTPollPeriod (" + WarmupPTPollPeriodTextValue + " ms) is less than minimum poll period (" + PTPollPeriodMinimum + " ms).\n\nPlease change the poll period to a greater value.", "", MessageBoxButtons.OK);
                    if (res == DialogResult.OK)
                    {
                        CancelOptionsChangeFlag = true;
                    }
                }
            }
            // SourceModeWaitPTPollPeriod
            SourceModeWaitPTPollPeriodTextValue = textBoxSourceModeWaitPTPollPeriod.Text;
            if (!Int32.TryParse(SourceModeWaitPTPollPeriodTextValue, out SourceModeWaitPTPollPeriodIntValue))
            {
                ParseFailFlag = true;
            }
            else
            {
                if (SourceModeWaitPTPollPeriodIntValue < PTPollPeriodMinimum)
                {
                    var res = MessageBox.Show("SourceModeWaitPTPollPeriod (" + SourceModeWaitPTPollPeriodTextValue + " ms) is less than minimum poll period (" + PTPollPeriodMinimum + " ms).\n\nPlease change the poll period to a greater value.", "", MessageBoxButtons.OK);
                    if (res == DialogResult.OK)
                    {
                        CancelOptionsChangeFlag = true;
                    }
                }
            }
            // CoolDownPTPollPeriod
            CoolDownPTPollPeriodTextValue = textBoxCoolDownPTPollPeriod.Text;
            if (!Int32.TryParse(CoolDownPTPollPeriodTextValue, out CoolDownPTPollPeriodIntValue))
            {
                ParseFailFlag = true;
            }
            else
            {
                if (CoolDownPTPollPeriodIntValue < PTPollPeriodMinimum)
                {
                    var res = MessageBox.Show("CoolDownPTPollPeriod (" + CoolDownPTPollPeriodTextValue + " ms) is less than minimum poll period (" + PTPollPeriodMinimum + " ms).\n\nPlease change the poll period to a greater value.", "", MessageBoxButtons.OK);
                    if (res == DialogResult.OK)
                    {
                        CancelOptionsChangeFlag = true;
                    }
                }
            }
            // CryoStartingPressure
            CryoStartingPressureTextValue = textBoxCryoStartingPressure.Text;
            if (!Double.TryParse(CryoStartingPressureTextValue, out CryoStartingPressureDoubleValue))
            {
                ParseFailFlag = true;
            }
            // CryoStartingTemperatureMax
            CryoStartingTemperatureMaxTextValue = textBoxCryoStartingTemperatureMax.Text;
            if (!Double.TryParse(CryoStartingTemperatureMaxTextValue, out CryoStartingTemperatureMaxDoubleValue))
            {
                ParseFailFlag = true;
            }
            else
            {
                if (CryoMaxTemperatureWhenTurnedOff < CryoStartingTemperatureMaxDoubleValue)
                {
                    var res = MessageBox.Show("Cryo starting temperature (" + CryoStartingTemperatureMaxTextValue + " K) is higher than storage temperature limit.\n\n Are you sure that you want to continue? Press OK to continue. \nPress cancel to continue editting options.", "", MessageBoxButtons.OKCancel);
                    if (res == DialogResult.Cancel)
                    {
                        CancelOptionsChangeFlag = true;
                    }
                }
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
            else
            {
                if (!CancelOptionsChangeFlag)
                {
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void btSaveSettings_Click(object sender, EventArgs e)
        {
            ProcessOptions();
        }


        private void textBoxTurbomolecularPumpUpperPressureLimit_TextChanged(object sender, EventArgs e)
        {
            CooldownConstantChangedFlag = true;
        }

        private void textBoxWarmupPTPollPeriod_TextChanged(object sender, EventArgs e)
        {
            CooldownConstantChangedFlag = true;
        }

        private void textBoxSourceModeWaitPTPollPeriod_TextChanged(object sender, EventArgs e)
        {
            CooldownConstantChangedFlag = true;
        }

        private void textBoxCoolDownPTPollPeriod_TextChanged(object sender, EventArgs e)
        {
            CooldownConstantChangedFlag = true;
        }

        private void textBoxCryoStartingPressure_TextChanged(object sender, EventArgs e)
        {
            CooldownConstantChangedFlag = true;
        }

        private void textBoxCryoStartingTemperatureMax_TextChanged(object sender, EventArgs e)
        {
            CooldownConstantChangedFlag = true;
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            ResetTextBoxValues();
        }
    }
}
