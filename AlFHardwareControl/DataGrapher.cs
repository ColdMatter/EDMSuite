using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text;

namespace AlFHardwareControl
{
    public partial class DataGrapher : UserControl
    {

        private Action<DataGrapher> updateData;

        public DataGrapher(string plotName, string yaxisTitle, Action<DataGrapher> _updateData)
        {
            InitializeComponent();
            this.updateData = _updateData;
            this.DataGraph.Titles.First().Text = plotName;
            this.DataGraph.ChartAreas.First().AxisY.Title = yaxisTitle;
        }

        public void UpdatePlot()
        {
            updateData(this);
            if (this.MaximumDataEnable.Checked)
            {
                this.UpdateRenderedObject<Chart>(DataGraph, (Chart c) => {
                    foreach (Series s in c.Series)
                    {
                        for (int i = 0; i < s.Points.Count - System.Convert.ToInt32(this.MaximumDatapointNumber.Text); ++i)
                        {
                            s.Points.RemoveAt(0);
                        }
                    }
                });
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

        public void UpdateRenderedObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            obj.Invoke(new UpdateObjectDelegate<T>(UpdateObject), new object[] { obj, updateFunc });
        }

        private delegate void UpdateObjectDelegate<T>(T obj, Action<T> updateFunc) where T : Control;

        private void UpdateObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            updateFunc(obj);
        }

        private void Settings_Enter(object sender, EventArgs e)
        {

        }

        private void logYAxis_CheckedChanged(object sender, EventArgs e)
        {

            this.DataGraph.ChartAreas[0].AxisY.IsLogarithmic = logYAxis.Checked;
            if (logYAxis.Checked) this.DataGraph.ChartAreas[0].AxisY.LabelStyle.Format = "0.0E0#";
            else this.DataGraph.ChartAreas[0].AxisY.LabelStyle.Format = "";
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
                csvContentBuilder.AppendFormat("\r\n{0}", this.DataGraph.Series[0].Points[i].XValue);

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


        private void MaximumDataEnable_CheckedChanged(object sender, EventArgs e)
        {
            this.MaximumDatapointNumber.Enabled = !this.MaximumDataEnable.Checked;
        }
    }
}
