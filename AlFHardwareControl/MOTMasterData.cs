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
            Dictionary<double,List<List<double[,]>>> ScanData = mmstuff.ScanData;

            int index = mmstuff.mmdata.FindIndex((MOTMasterData mmdata) => { return System.Object.ReferenceEquals(mmdata,this); });

            List<double[,]> flatData = ScanData.Values.ToList().SelectMany(i => i).SelectMany(i=>i).ToList();
            double[][] normedData = Enumerable.Range(0, flatData.Count).Select(
                i => NormaliseData(Enumerable.Range(0, flatData[i].GetLength(1)).Select(j => flatData[i][index, j]).ToArray(), Enumerable.Range(0, flatData[i].GetLength(1)).Select(j => flatData[i][normSourceID, j]).ToArray())).ToArray();

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

        public double[] NormaliseData(double[,] rawData)
        {
            int index = mmstuff.mmdata.FindIndex((MOTMasterData mmdata) => { return System.Object.ReferenceEquals(mmdata, this); });
            double[][] data = Enumerable.Range(0, rawData.GetLength(0)).Select(
                i => Enumerable.Range(0, rawData.GetLength(1)).Select(
                    j => rawData[i, j]).ToArray()).ToArray();
            if (!Normalise.Checked) return data[index];
            double[] normaliser = data[normSourceID];
            double[] normedData = new double[xData.Length];
            for (int i = 0; i < xData.Length; ++i)
            {
                if (normaliser[i] != 0)
                    normedData[i] = data[index][i] / normaliser[i];
                else
                    normedData[i] = 0;
            }
            return normedData;

        }

        public double[] NormaliseData(double[] rawData, double[] normaliser)
        {
            if (!Normalise.Checked) return rawData;
            double[] normedData = new double[xData.Length];
            for (int i = 0; i < xData.Length; ++i)
            {
                if (normaliser[i] != 0)
                    normedData[i] = rawData[i] / normaliser[i];
                else
                    normedData[i] = 0;
            }
            return normedData;

        }

        public bool reject_shot()
        {
            return false;
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
    }
}
