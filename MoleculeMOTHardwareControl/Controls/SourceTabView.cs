using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoleculeMOTHardwareControl.Controls
{
    public partial class SourceTabView : MoleculeMOTHardwareControl.Controls.GenericView
    {
        protected SourceTabController castController;

        public SourceTabView() : base()
        {
            castController = (SourceTabController)controller; // saves casting in every method
        }

        #region UI Update Handlers

        public void UpdateGraph(double time, double temp)
        {
            tempGraph.PlotXYAppend(time, temp);
        }

        public void UpdateReadButton(bool state)
        {
            readButton.Text = state ? "Start Reading" : "Stop Reading";
        }

        #endregion

        #region UI Query Handlers

        #endregion

        #region UI Event Handlers

        private void toggleReading(object sender, EventArgs e)
        {
            castController.ToggleReading();
        }

        #endregion
 
    }
}
