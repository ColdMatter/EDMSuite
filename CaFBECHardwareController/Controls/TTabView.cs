using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Data;

namespace CaFBECHardwareController.Controls
{
    public partial class TTabView : CaFBECHardwareController.Controls.GenericView
    {
        protected TTabController castController;

        public TTabView(TTabController controllerInstance) : base(controllerInstance)
        {
            InitializeComponent();
            castController = (TTabController)controller;

            this.DataGraph.Series.Add(this.label1.Text).ChartType = SeriesChartType.Line;
            this.DataGraph.Series.Add(this.label2.Text).ChartType = SeriesChartType.Line;
            this.DataGraph.Series.Add(this.label3.Text).ChartType = SeriesChartType.Line;
            this.DataGraph.Series.Add(this.label4.Text).ChartType = SeriesChartType.Line;
            //this.DataGraph.Series.Add(this.label5.Text).ChartType = SeriesChartType.Line;
            this.DataGraph.Series.Add(this.label6.Text).ChartType = SeriesChartType.Line;
            this.DataGraph.Series.Add(this.label7.Text).ChartType = SeriesChartType.Line;
            this.DataGraph.Series.Add(this.label8.Text).ChartType = SeriesChartType.Line;

            foreach (Series series in this.DataGraph.Series)
            {
                series.XValueType = ChartValueType.DateTime;
            }

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

        private void TPTabView_Load(object sender, EventArgs e)
        {
            //castController.WindowLoaded();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void CryoON_Click(object sender, EventArgs e)
        {
            castController.ToggleCryo();
        }

        private void CycleButton_Click(object sender, EventArgs e)
        {
            castController.ToggleCycleSource();

        }

        private void HeaterON_Click(object sender, EventArgs e)
        {
            castController.ToggleHeater();
        }

        public void UpdateRenderedObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            obj.Invoke(new UpdateObjectDelegate<T>(UpdateObject), new object[] { obj, updateFunc });
        }

        private delegate void UpdateObjectDelegate<T>(T obj, Action<T> updateFunc) where T : Control;

        private void UpdateObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            updateFunc(obj);
        }

        private void StartReading_Click(object sender, EventArgs e)
        {
            castController.ToggleReading();
        }

        private void SaveTempData_Click(object sender, EventArgs e)
        {
            //String csvContent = "Time";
            StringBuilder csvContentBuilder = new StringBuilder("Time", 10 * 4 * this.DataGraph.Series[0].Points.Count); // Try later

            foreach (Series series in this.DataGraph.Series)
            {
                //csvContent += ",";
                //csvContent += series.Name;
                csvContentBuilder.AppendFormat(",{0}", series.Name);
            }

            for (int i = 0; i < this.DataGraph.Series[0].Points.Count; ++i)
            {
                //csvContent += "\r\n";
                //csvContent += this.DataGraph.Series[0].Points[i].XValue;
                csvContentBuilder.AppendFormat("\r\n{0}", DateTime.FromOADate(this.DataGraph.Series[0].Points[i].XValue));

                foreach (Series series in this.DataGraph.Series)
                {
                    if (series.Points.Count <= i) continue;
                    //csvContent += ",";
                    //csvContent += series.Points[i].YValues[0];
                    csvContentBuilder.AppendFormat(",{0}", series.Points[i].YValues[0]);
                }
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(this.TempDataSaveLoc.Text);

            file.WriteLine(csvContentBuilder.ToString());

            file.Close();
        }

        private void DataClear_Click(object sender, EventArgs e)
        {
            foreach (Series series in this.DataGraph.Series)
            {
                series.Points.Clear();
            }
        }
    }
}
