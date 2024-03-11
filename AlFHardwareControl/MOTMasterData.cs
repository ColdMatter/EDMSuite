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

        private string normSourceName;
        private int normSourceID;
        private double[] xData;
        private double[] rawYData;

        public void initNormalisation(List<string> options)
        {
            foreach (string norm in options)
            {
                NormSource.Items.Add(norm);
            }
            NormSource.SelectedIndex = 0;
        }

        public MOTMasterStuff mmstuff;

        public void UpdateData(double[] xdata, double[] newData)
        {
            xData = xdata;
            rawYData = newData;
            ReDraw();
        }

        public void UpdateScan()
        {
            if (xData == null) return;
            this.Invoke((Action)(() =>
            {
                dataGraph.Plots[1].ClearData();
            }));
            if (mmstuff.ScanData.Count == 0) return;
            Dictionary<double,List<List<SerializableDictionary<string, double[]>>>> ScanData = mmstuff.ScanData;

            List<SerializableDictionary<string,double[]>> flatData = ScanData.Values.ToList().SelectMany(i => i).SelectMany(i=>i).ToList();
            double[][] normedData = Enumerable.Range(0, flatData.Count).Select(
                i => NormaliseData(flatData[i])).ToArray();

            double[] data = Enumerable.Range(0, normedData[0].Length).Select(
                i => Enumerable.Range(0, normedData.Length).Select(
                    j => normedData[j][i]).Average()).ToArray();

            this.Invoke((Action)(() =>
            {;
                dataGraph.Plots[1].PlotXY(xData,data);
            }));
        }

        private void ReDraw()
        {
            if (xData == null) return;
            double[] data = NormaliseData();
            this.Invoke((Action)(() =>
            {
                dataGraph.Plots[0].ClearData();
                dataGraph.Plots[0].PlotXY(xData, data);
                dataGraph.Update();
            }));
        }

        public double[] NormaliseData()
        {
            if (!sourceEnabled) return new double[xData.Length];
            if (!Normalise.Checked) return rawYData;
            double[] normedData = new double[xData.Length];
            double[] normaliser = mmstuff.AIData[normSourceName];
            for (int i = 0; i < xData.Length; ++i)
            {
                if (normaliser[i] != 0)
                    normedData[i] = rawYData[i] / normaliser[i];
                else
                    normedData[i] = 0;
            }
            return normedData;

        }

        public double[] NormaliseData(Dictionary<string, double[]> rawData)
        {
            if (!sourceEnabled) return new double[xData.Length];
            if (!Normalise.Checked) return rawData[name];
            double[] normaliser = rawData[normSourceName];
            double[] normedData = new double[xData.Length];
            for (int i = 0; i < xData.Length; ++i)
            {
                if (normaliser[i] != 0)
                    normedData[i] = rawData[name][i] / normaliser[i];
                else
                    normedData[i] = 0;
            }
            return normedData;

        }

        private Comparison<double> comparer = null;

        public bool reject_shot()
        {
            if (comparer != null)
            {
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
    }
}
