using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SirCachealot
{
    public partial class MainWindow : Form
    {
        internal Controller controller;

        private readonly List<string> gatedDetectorList = new List<string>() 
        { 
            "asymmetry", 
            "topProbeNoBackground", 
            "bottomProbeScaled", 
            "topProbe",
            "bottomProbe",
            "magnetometer", 
            "gnd", 
            "battery",
            "rfCurrent",
            "reflectedrf1Amplitude",
            "reflectedrf2Amplitude",
            "bottomProbeNoBackground"
        };

        private readonly List<string> pointDetectorList = new List<string>()
        {
            "MiniFlux1",
            "MiniFlux2",
            "MiniFlux3",
            "PumpPD",
            "ProbePD",
            "NorthCurrent",
            "SouthCurrent",
            "PhaseLockFrequency",
            "PhaseLockError"
        };

        public MainWindow()
        {
            InitializeComponent();
            InitialiseGateListDataView();
        }

        private void InitialiseGateListDataView()
        {
            this.gateListDataView.AutoGenerateColumns = false;

            DataGridViewComboBoxColumn col1 = new DataGridViewComboBoxColumn();
            col1.DataPropertyName = "Detector";
            col1.HeaderText = "Detector";
            col1.DataSource = gatedDetectorList;
            col1.ValueType = typeof(String);

            this.gateListDataView.Columns.Add(col1);

            DataGridViewTextBoxColumn col2 = new DataGridViewTextBoxColumn();
            col2.DataPropertyName = "StartTime";
            col2.HeaderText = "Start time (us)";
            col2.ValueType = typeof(int);

            this.gateListDataView.Columns.Add(col2);

            DataGridViewTextBoxColumn col3 = new DataGridViewTextBoxColumn();
            col3.DataPropertyName = "EndTime";
            col3.HeaderText = "End time (us)";
            col3.ValueType = typeof(int);

            this.gateListDataView.Columns.Add(col3);

            DataGridViewComboBoxColumn col4 = new DataGridViewComboBoxColumn();
            col4.DataPropertyName = "Integrate";
            col4.HeaderText = "Integrate?";
            col4.DataSource = new List<bool>() { true, false };
            col4.ValueType = typeof(bool);

            this.gateListDataView.Columns.Add(col4);

            this.gateListDataView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        // choosing File->Exit is set to close the form.
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        // If the form closes, either from File->Exit, or by clicking the close
        // box, then the controller is alerted, so that it can shut down.
        private void formCloseHandler(object sender, FormClosingEventArgs e)
        {
            controller.Exit();
        }
        // this alerts the controller that the form is loaded and ready.
        private void formLoadHandler(object sender, EventArgs e)
        {
            controller.UIInitialise();
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.CreateDB();
        }

        internal void SetMemcachedStatsText(string p)
        {
            statsTextBox.Text = p;
        }

        private void selectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SelectDB();
        }

        public void AppendToLog(string txt)
        {
            logTextBox.BeginInvoke(new AppendTextDelegate(logTextBox.AppendText),
				new object[] {txt + Environment.NewLine});
        }

        public void AppendToErrorLog(string txt)
        {
            errorLogTextBox.BeginInvoke(new AppendTextDelegate(errorLogTextBox.AppendText),
                new object[] { txt + Environment.NewLine });
        }

        public void SetStatsText(string txt)
        {
            logTextBox.BeginInvoke(new AppendTextDelegate(SetStatsTextInternal),
                new object[] { txt + Environment.NewLine });
        }

        public void PopulateGateConfigList(List<string> gateConfigNames)
        {
            gateConfigSelectionComboBox.DataSource = gateConfigNames;
        }

        public List<string> GetGateConfigList()
        {
            List<string> gateConfigList = new List<string>();
            foreach(object item in gateConfigSelectionComboBox.Items)
            {
                gateConfigList.Add((string)item);
            }
            return gateConfigList;
        }
        
        public void AddGateListEntry(string detector, int startTime, int endTime, bool integrate)
        {
            if (gatedDetectorList.Contains(detector))
            {
                this.gateListDataView.Rows.Add(detector, startTime, endTime, integrate);
            }

            this.gateListDataView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        public void ClearGateList()
        {
            this.gateListDataView.Rows.Clear();
        }

        public string GetGateConfigName()
        {
            return gateConfigSelectionComboBox.SelectedItem.ToString();
        }

        public string GetGateConfigNameTextBox()
        {
            return currentGateConfigNameTextBox.Text;
        }

        public void SelectGateConfig(string name)
        {
            if (gateConfigSelectionComboBox.Items.Contains(name))
            {
                gateConfigSelectionComboBox.SelectedItem = name;
            }
        }

        public List<object[]> GetGateConfigFromDataView()
        {
            List<object[]> configData = new List<object[]>();
            
            foreach (DataGridViewRow row in gateListDataView.Rows)
            {
                if (!row.IsNewRow)
                {
                    object[] entry = new object[4];
                    int i = 0;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        entry[i] = cell.Value;
                        i++;
                    }
                    configData.Add(entry);
                }
                
            }

            return configData;
        }

        private void SetStatsTextInternal(string txt)
        {
            statsTextBox.Text = txt;
        }

 		private delegate void AppendTextDelegate(String text);

        private void test1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.Test1();
        }

        private void test2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.Test2();
        }

        private void loadGateSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadGateSet();
        }

        private void gateConfigSelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.UpdateGateListInUI(gateConfigSelectionComboBox.SelectedItem.ToString());
            currentGateConfigNameTextBox.Text = gateConfigSelectionComboBox.SelectedItem.ToString();
        }

        private void updateGatesButton_Click(object sender, EventArgs e)
        {
            controller.SaveCurrentGateConfig();
        }

        private void newGateConfigButton_Click(object sender, EventArgs e)
        {
            controller.NewGateConfig();
        }

        private void saveGateConfigSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveGateSet();
        }

        private void addBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.AddBlockFromMainWindow();
        }

        private void addGatedBlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.AddGatedBlockFromMainWindow();
        }
    }
}
