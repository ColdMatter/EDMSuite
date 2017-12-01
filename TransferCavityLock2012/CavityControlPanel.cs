using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransferCavityLock2012
{
    public partial class CavityControlPanel : UserControl
    {
        public Controller controller;
        public string CavityName;
        public Dictionary<string, LockControlPanel> SlaveLaserPanels = new Dictionary<string, LockControlPanel>();

        public CavityControlPanel()
        {
            CavityName = "Test";
            InitializeComponent();
        }

        public CavityControlPanel(string name)
        {
            CavityName = name;
            InitializeComponent();
        }

        public void AddSlaveLaserPanel(SlaveLaser sl)
        {
            string title = sl.Name;
            TabPage newTab = new TabPage(title);
            LockControlPanel panel = new LockControlPanel(title, sl.LowerVoltageLimit, sl.UpperVoltageLimit, sl.Gain);
            panel.Controller = this.controller;
            panel.CavityPanel = this;
            slaveLasersTab.TabPages.Add(newTab);
            newTab.Controls.Add(panel);
            slaveLasersTab.Enabled = true;
            SlaveLaserPanels.Add(title, panel);
        }

        private void CavLockVoltageTrackBar_Scroll(object sender, EventArgs e)
        {
            double value = ((double)CavLockVoltageTrackBar.Value) / 100;
            SetSummedVoltageTextBox(value);
            // Check its a UI event!
            if (CavLockVoltageTrackBar.Focused)
            {
                controller.VoltageToMasterLaserChanged(CavityName, value);
            }
        }

        private void SummedVoltageTextBox_TextChanged(object sender, EventArgs e)
        {
            double value;
            if (Double.TryParse(SummedVoltageTextBox.Text, out value))
            {
                if (SummedVoltageTextBox.Focused)
                {
                    controller.VoltageToMasterLaserChanged(CavityName, value);
                }
            }
        }

        private void MasterGainTextBox_TextChanged(object sender, EventArgs e)
        {
            double value;
            if (Double.TryParse(MasterGainTextBox.Text, out value))
            {
                if (MasterGainTextBox.Focused)
                {
                    controller.MasterGainChanged(CavityName, value);
                }
            }
        }

        private void MasterSetPointTextBox_TextChanged(object sender, EventArgs e)
        {
            double value;
            if (Double.TryParse(MasterSetPointTextBox.Text, out value))
            {
                if (MasterSetPointTextBox.Focused)
                {
                    controller.MasterSetPointChanged(CavityName, value);
                }
            }
        }

        public void SetSummedVoltageTextBox(double value)
        {
            UIHelper.SetTextBox(SummedVoltageTextBox, Convert.ToString(value));
        }

        public void SetVoltageIntoCavityTextBox(double value)
        {
            UIHelper.SetTextBox(VoltageIntoCavityTextBox, Convert.ToString(value));
        }

        internal void UpdateMasterUIState(MasterLaser.LaserState state)
        {
            switch (state)
            {
                case SlaveLaser.LaserState.FREE:
                    MasterSetPointTextBox.Enabled = true;
                    MasterGainTextBox.Enabled = true;
                    SummedVoltageTextBox.Enabled = true;
                    CavLockVoltageTrackBar.Enabled = true;
                    break;

                case SlaveLaser.LaserState.LOCKING:
                    MasterSetPointTextBox.Enabled = false;
                    MasterGainTextBox.Enabled = false;
                    SummedVoltageTextBox.Enabled = false;
                    CavLockVoltageTrackBar.Enabled = false;
                    break;

                case SlaveLaser.LaserState.LOCKED:
                    break;
            }
        }

        internal void AppendToErrorGraph(string laserName, int lockCount, double error)
        {
            SlaveLaserPanels[laserName].AppendToErrorGraph(lockCount, error);
        }

        internal void UpdateSlaveUIState(string slaveName, SlaveLaser.LaserState state)
        {
            SlaveLaserPanels[slaveName].UpdateUIState(state);
        }

        private void masterLockEnableCheck_CheckedChanged(object sender, EventArgs e)
        {

            if (masterLockEnableCheck.CheckState == CheckState.Checked)
            {
                controller.EngageMasterLock(CavityName);
            }
            if (masterLockEnableCheck.CheckState == CheckState.Unchecked)
            {
                controller.DisengageMasterLock(CavityName);
            }

        }

        public void DisplayMasterData(double[] rampData, double[] masterData)
        {
            UIHelper.scatterGraphPlot(MasterLaserIntensityScatterGraph, MasterDataPlot, rampData, masterData);
        }

        public void DisplayMasterFitData(double[] rampData, double[] masterData)
        {
            UIHelper.scatterGraphPlot(MasterLaserIntensityScatterGraph, MasterFitPlot, rampData, masterData);
        }
    }
}
