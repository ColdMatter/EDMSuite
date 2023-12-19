using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlFHardwareControl
{
    public partial class MOTMasterData : UserControl
    {
        public MOTMasterData()
        {
            InitializeComponent();
        }

        public void UpdateData(double[] xdata, double[] newData)
        {
            dataGraph.Plots[0].ClearData();
            dataGraph.Plots[0].PlotXY(xdata, newData);
            this.Invoke((Action)(() => { dataGraph.Update(); }));
        }

        private void fixX_CheckedChanged(object sender, EventArgs e)
        {
            dataGraph.XAxes[0].Mode = fixX.Checked ? NationalInstruments.UI.AxisMode.Fixed : NationalInstruments.UI.AxisMode.AutoScaleLoose;
        }

        private void fixY_CheckedChanged(object sender, EventArgs e)
        {
            dataGraph.YAxes[0].Mode = fixY.Checked ? NationalInstruments.UI.AxisMode.Fixed : NationalInstruments.UI.AxisMode.AutoScaleLoose;
        }
    }
}
