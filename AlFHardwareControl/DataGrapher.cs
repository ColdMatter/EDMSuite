using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text;
using System.Collections.Generic;

using DAQ;

namespace AlFHardwareControl
{
    public partial class DataGrapher : UserControl
    {

        private Action<DataGrapher> updateData;
        private string name;

        public DataGrapher(string plotName, string yaxisTitle, Action<DataGrapher> _updateData)
        {
            InitializeComponent();
            name = plotName;
            this.updateData = _updateData;
            this.DataGraph.Titles.First().Text = plotName;
            this.DataGraph.ChartAreas.First().AxisY.Title = yaxisTitle;
        }

        public string unit = "";

        public Label[] dataLabels;
        public Label[] dataLabelsValue;

        public void SetupDataDisplay()
        {

            dataLabels = new Label[DataGraph.Series.Count];
            dataLabelsValue = new Label[DataGraph.Series.Count];

            DataTable.RowCount = DataGraph.Series.Count;

            for (int i = 0; i < DataGraph.Series.Count; ++i)
            {
                dataLabels[i] = new Label();
                dataLabelsValue[i] = new Label();
                dataLabels[i].Text = DataGraph.Series[i].Name;
                dataLabelsValue[i].Text = "No Data";
                dataLabels[i].Anchor = AnchorStyles.Left | AnchorStyles.Top;
                dataLabelsValue[i].Anchor = AnchorStyles.Left | AnchorStyles.Top;

                DataTable.Controls.Add(dataLabels[i], 0, i);
                DataTable.Controls.Add(dataLabelsValue[i], 1, i);
            }

        }

        private Dictionary<string, double> LastData = new Dictionary<string, double> { };
        public double this[string key]
        {
            get
            {
                lock(LastData){
                    return LastData[key];
                }
            }
        }
        public void UpdatePlot()
        {
            if (!this.takeData.Checked)
            {
                this.Invoke((Action)(()=>{
                    for (int i = 0; i < DataGraph.Series.Count; ++i)
                    {
                        dataLabelsValue[i].Text = "No Data";
                    }
                }));
                return;
            }
            updateData(this);
            if (!this.takeData.Checked) return;
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

            lock (LastData)
            {
                LastData.Clear();
            }
            this.Invoke((Action)(() => {
                lock (LastData)
                {
                    for (int i = 0; i < DataGraph.Series.Count; ++i)
                    {
                        dataLabelsValue[i].Text = DataGraph.Series[i].Points.Last().YValues[0].ToString("g6") + " " + unit;
                        LastData.Add(dataLabels[i].Text, DataGraph.Series[i].Points.Last().YValues[0]);
                    }
                }
            }));

#if INFLUX_DB
            UpdateInfluxDB();
#endif
        }

#if INFLUX_DB
        private void UpdateInfluxDB()
        {
            InfluxDBDataLogger data = InfluxDBDataLogger.Measurement("DataGrapher").Tag("name", name);
            lock (LastData)
            {
                foreach (string key in LastData.Keys)
                {
                    data = data.Field(key, LastData[key]);
                }
                data = data.TimestampMS(DateTime.UtcNow);

            }

            data.Write("https://ccmmonitoring.ph.ic.ac.uk:8086", Environment.GetEnvironmentVariable("INFLUX_BUCKET"), "CentreForColdMatter");

        }
#endif

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
