using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Data;

namespace AlFHardwareControl
{
    public partial class MOTMasterData : UserControl
    {

        private delegate bool Comparison<T>(T a);
        private delegate Comparison<T> ComparisonGenerator<T>(string comparisonValue);

        private static Dictionary<string, ComparisonGenerator<double>> comparisons = new Dictionary<string, ComparisonGenerator<double>>{
            { "Minimum <", (string comp) => {
                double compVal = Convert.ToDouble(comp);
                return (double a)=> { return a < compVal; };
            }},
            { "Maximum >", (string comp) => {
                double compVal = Convert.ToDouble(comp);
                return (double a)=> { return a > compVal; };
            }},
        };

        private string name;

        public MOTMasterData()
        {
            InitializeComponent();
            RejectCondPicker.Items.AddRange(comparisons.Keys.ToArray<string>());
            RejectCondPicker.SelectedIndex = 0;
            sourceEnabled = sourceEnable.Checked;
            name = "";
        }

        public MOTMasterData(string _name)
        {
            InitializeComponent();
            RejectCondPicker.Items.AddRange(comparisons.Keys.ToArray<string>());
            RejectCondPicker.SelectedIndex = 0;
            sourceEnabled = sourceEnable.Checked;
            name = _name;
        }

        public MOTMasterData(string _name, string yaxis_caption)
        {
            InitializeComponent();
            RejectCondPicker.Items.AddRange(comparisons.Keys.ToArray<string>());
            RejectCondPicker.SelectedIndex = 0;
            sourceEnabled = sourceEnable.Checked;
            name = _name;
            dataGraph.YAxes[0].Caption = yaxis_caption;
        }

        private string normSourceName;
        private int normSourceID;
        private double[] xData;

        public void initNormalisation(List<string> options)
        {
            foreach (string norm in options)
            {
                NormSource.Items.Add(norm);
            }
            NormSource.SelectedIndex = 0;
        }

        public MOTMasterStuff mmstuff;

        public void UpdateXData(double[] xdata)
        {
            xData = xdata;
            ReDraw();
        }

        public void UpdateScan()
        {
            if (xData == null) return;
            if (!this.SourceEnabled) return;
            this.Invoke((Action)(() =>
            {
                for (int i = 0; i < mmstuff.SwitchStates; ++i)
                {
                    dataGraph.Plots[2 * i + 1].ClearData();
                }
            }));
            if (mmstuff.ScanData.Count == 0) return;
            lock (mmstuff)
            {
                Dictionary<double,List<List<SerializableDictionary<string, List<double[]>>>>> ScanData = mmstuff.ScanData;

                List<SerializableDictionary<string,List<double[]>>> flatData = ScanData.Values.ToList().SelectMany(i => i).SelectMany(i=>i).ToList();
                List<double[]>[] normedData = Enumerable.Range(0, flatData.Count).Select(
                    i => NormaliseData(flatData[i],false)).ToArray();

                List<double[]> data = Enumerable.Range(0, normedData[0].Count).Select(
                                    k => Enumerable.Range(0, normedData[0][0].Length).Select(
                                    i => Enumerable.Range(0, normedData.Length).Select(
                                    j => normedData[j][k][i]).Average()).ToArray()).ToList();

                this.Invoke((Action)(() =>
                {
                    for (int i = 0; i < data.Count; ++i)
                    {
                        dataGraph.Plots[2 * i + 1].PlotXY(xData, data[i]);
                    }
                }));
            }
        }

        public void ReDraw()
        {
            if (xData == null || mmstuff.AIData.Count == 0) return;
            List<double[]> data = NormaliseData();
            mmstuff.Integrals = IntegrateData(true);
            this.Invoke((Action)(() =>
            {
                while (mmstuff.SwitchStates * 2 < dataGraph.Plots.Count)
                {
                    dataGraph.Plots.RemoveAt(dataGraph.Plots.Count - 1);
                }
                while (mmstuff.SwitchStates * 2 > dataGraph.Plots.Count)
                {
                    NationalInstruments.UI.ScatterPlot points = new NationalInstruments.UI.ScatterPlot(dataGraph.XAxes[0], dataGraph.YAxes[0]);
                    NationalInstruments.UI.ScatterPlot scan = new NationalInstruments.UI.ScatterPlot(dataGraph.XAxes[0], dataGraph.YAxes[0]);
                    dataGraph.Plots.Add(points);
                    dataGraph.Plots.Add(scan);
                    points.PointColor = points.LineColor;
                    points.LineStyle = NationalInstruments.UI.LineStyle.Dot;
                    scan.LineColor = points.LineColor;
                    points.Visible = show_last_data.Checked;
                    scan.Visible = show_average.Checked;
                }
                for (int i = 0; i < data.Count; ++i)
                {
                    dataGraph.Plots[2 * i].ClearData();
                    if (i >= data.Count) continue; 
                    dataGraph.Plots[2 * i].PlotXY(xData, data[i]);
                }
                dataGraph.Update();
            }));
        }

        public List<double[]> NormaliseData()
        {
            if (!sourceEnabled) return new List<double[]> { };
            if (!Normalise.Checked) return mmstuff.AIData[name];
            List<double[]> normedData = new List<double[]> { };
            List<double[]> normaliser = mmstuff.AIData[normSourceName];
            lock (mmstuff)
            {
                for (int j = 0; j < mmstuff.AIData[name].Count; ++j)
                {
                    normedData.Add(new double[xData.Length]);
                    for (int i = 0; i < xData.Length; ++i)
                    {
                        if (normaliser[j][i] != 0)
                            normedData[j][i] = mmstuff.AIData[name][j][i] / normaliser[j][i];
                        else
                            normedData[j][i] = 0;
                    }
                }
            }
            return normedData;

        }

        public List<double> IntegrateData(bool gate)
        {
            List<double[]> normedData = new List<double[]> { };
            List<double[]> normaliser;
            lock (mmstuff)
            {
                if (!sourceEnabled) return new List<double>{ };
                normaliser = mmstuff.AIData[normSourceName];
                for (int j = 0; j < mmstuff.AIData[name].Count; ++j)
                {
                    normedData.Add(new double[xData.Length]);
                    for (int i = 0; i < xData.Length; ++i)
                    {
                        if ((Normalise.Checked && normaliser[j][i] != 0) && (!gate || (dataPlotXhigh.XPosition >= xData[i] && dataPlotXlow.XPosition <= xData[i])))
                            normedData[j][i] = mmstuff.AIData[name][j][i] / normaliser[j][i];
                        else if (!Normalise.Checked && (!gate || (dataPlotXhigh.XPosition >= xData[i] && dataPlotXlow.XPosition <= xData[i])))
                            normedData[j][i] = mmstuff.AIData[name][j][i];
                        else
                            normedData[j][i] = 0;
                    }
                }
            }
            List<double> result = Enumerable.Range(0, normedData.Count).Select(
                    i => Enumerable.Range(0, normedData[0].Length).Select(
                    j => normedData[i][j]).Sum()).ToList();
            return result;
        }

        public List<double[]> NormaliseData(Dictionary<string, List<double[]>> rawData, bool gate)
        {
            if (!sourceEnabled) return new List<double[]> { };
            List<double[]> normedData = new List<double[]> { };


            for (int j = 0; j < mmstuff.AIData[name].Count; ++j)
            {
                normedData.Add(new double[xData.Length]);
                for (int i = 0; i < xData.Length; ++i)
                {
                    if (!gate || (dataPlotXhigh.XPosition >= xData[i] && dataPlotXlow.XPosition <= xData[i]))
                        normedData[j][i] = rawData[name][j][i];
                    else
                        normedData[j][i] = 0;
                }
            } 

            if (!Normalise.Checked) return normedData;
            List<double[]> normaliser = rawData[normSourceName];
            for (int j = 0; j < mmstuff.AIData[name].Count; ++j)
            {
                normedData.Add(new double[xData.Length]);
                for (int i = 0; i < xData.Length; ++i)
                {
                    if (normaliser[j][i] != 0)
                        normedData[j][i] = normedData[j][i] / normaliser[j][i];
                    else
                        normedData[j][i] = 0;
                }
            }
            return normedData;

        }

        private Comparison<double> comparer = null;

        public bool reject_shot()
        {
            if (comparer != null)
            {
                lock (mmstuff)
                foreach (double[] rawYData in mmstuff.AIData[name])
                foreach (double y in rawYData)
                    if (comparer(y)) return true;
            }


            return false;
        }

        private bool sourceEnabled;
        public bool SourceEnabled
        {
            get
            {
                return sourceEnabled;
            }
        }

        private void fixX_CheckedChanged(object sender, EventArgs e)
        {
            dataGraph.XAxes[0].Mode = fixX.Checked ? NationalInstruments.UI.AxisMode.Fixed : NationalInstruments.UI.AxisMode.AutoScaleLoose;
        }

        private void fixY_CheckedChanged(object sender, EventArgs e)
        {
            dataGraph.YAxes[0].Mode = fixY.Checked ? NationalInstruments.UI.AxisMode.Fixed : NationalInstruments.UI.AxisMode.AutoScaleLoose;
        }

        private void Normalise_CheckedChanged(object sender, EventArgs e)
        {
            if (!mmstuff.mmdata[normSourceID].SourceEnabled && Normalise.Checked)
            {
                Normalise.Checked = false;
            }
            ReDraw();
            UpdateScan();
            mmstuff.ReDrawScanResults();
        }

        private void NormSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            normSourceName = NormSource.Text;
            normSourceID = NormSource.SelectedIndex;
            ReDraw();
            UpdateScan();
            mmstuff.ReDrawScanResults();
        }

        private void RejectEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (!RejectEnable.Checked)
            {
                comparer = null;
                this.RejectVal.Enabled = true;
                this.RejectCondPicker.Enabled = true;
                return;
            }

            try
            {
                comparer = comparisons[RejectCondPicker.Text](RejectVal.Text);
            }
            catch (Exception exc) when (exc is System.FormatException || exc is InvalidCastException)
            {
                comparer = null;
                RejectEnable.Checked = false;
                return;
            }

            this.RejectVal.Enabled = false;
            this.RejectCondPicker.Enabled = false;

        }

        public void UpdateScanStatus(bool status)
        {
            this.Invoke((Action) (()=>{
                this.RejectEnable.Enabled = !status;
            }));
        }

        private void sourceEnable_CheckedChanged(object sender, EventArgs e)
        {
            sourceEnabled = sourceEnable.Checked;
        }

        private void show_average_CheckedChanged(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                for (int i = 0; i < dataGraph.Plots.Count; ++i)
                {
                    if (i % 2 == 0) continue;
                    dataGraph.Plots[i].Visible = show_average.Checked;
                }
            }));
        }

        private void show_last_data_CheckedChanged(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                for (int i = 0; i < dataGraph.Plots.Count; ++i)
                {
                    if (i % 2 == 1) continue;
                    dataGraph.Plots[i].Visible = show_last_data.Checked;
                }
            }));
        }

        private double xlow = 0;
        private double xhigh = 10;
        private void dataPlotLimitsAfterMove(object sender, NationalInstruments.UI.AfterMoveXYCursorEventArgs e)
        {
            if (mmstuff == null) return;
            if (dataPlotXlow.XPosition == xlow && dataPlotXhigh.XPosition == xhigh)
                return;

            xlow = dataPlotXlow.XPosition;
            xhigh = dataPlotXhigh.XPosition;

            UpdateScan();
            mmstuff.ReDrawScanResults();
            mmstuff.Integrals = IntegrateData(true);
        }
    }
}
