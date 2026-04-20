using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MOTMaster
{
    public partial class ControllerWindow : Form
    {
        public Controller controller = new Controller();

        public ControllerWindow()
        {
            InitializeComponent();
        }

        public void WriteToScriptPath(string str)
        {
            setTextBox(PatternPathTextBox, str);
        }

        public void WriteToSaveBatchTextBox(int number)
        {
            setTextBox(saveBatchTextBox, Convert.ToString(number));
        }
        public void SetSaveCheckBox(bool value)
        {
            setCheckBox(saveExperimentCheckBox, value);
        }
        #region wrappers

        private void setTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextDelegate(setTextHelper), new object[] { box, text });
        }
        private delegate void SetTextDelegate(TextBox box, string text);
        private void setTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }
        private string readComboBox(ComboBox box)
        {
            return (string)box.Invoke(new ReadComboDelegate(readComboHelper), new object[] { box });
        }
        private delegate string ReadComboDelegate(ComboBox box);
        private string readComboHelper(ComboBox box)
        {
            return box.Text;
        }
        private void setCheckBox(CheckBox box, bool value)
        {
            box.Invoke(new SetCheckDelegate(setCheckHelper), new object[] { box, value });
        }
        private delegate void SetCheckDelegate(CheckBox box, bool value);
        private void setCheckHelper(CheckBox box, bool value)
        {
            box.Checked = value;
        }

        #endregion

        public void FillScriptComboBox(string[] s)
        {
            scriptListComboBox.Items.Clear();
            scriptListComboBox.Items.AddRange(s);
            scriptListComboBox.Text = s[0];
        }

        private void lookupScriptsButton_Click(object sender, EventArgs e)
        {
            controller.ScriptLookupAndDisplay();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            controller.Run();
        }

        private string getScriptPath()
        {
            return readComboBox(scriptListComboBox);
        }

        private void ControllerWindow_Load(object sender, EventArgs e)
        {
            controller.SetScriptPath(getScriptPath());
        }

        private void saveExperimentCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (saveExperimentCheckBox.Checked == true)
            {
                saveBatchTextBox.Enabled = true;
                controller.SaveToggle(true);
            }
            if (saveExperimentCheckBox.Checked == false)
            {
                saveBatchTextBox.Enabled = false;
                controller.SaveToggle(false);
            }
        }

        public int GetSaveBatchNumber()
        {
            return int.Parse(saveBatchTextBox.Text);
        }

        public int GetIterations()
        {
            return int.Parse(iterationsBox.Text);
        }
        public void SetIterations(int number)
        {
            setTextBox(iterationsBox, Convert.ToString(number));
        }

        public bool RunUntilStoppedState
        {
            get { return runUntilStopCheckBox.Checked; }
            set { runUntilStopCheckBox.Checked = value;  }
           
        }

        private void selectScriptButton_Click(object sender, EventArgs e)
        {
            controller.SetScriptPath(getScriptPath());
        }

        private void newPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SetScriptPath(getScriptPath());
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ReplicateScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
             controller.RunReplica();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            controller.status = Controller.RunningState.stopped;
        }

        private void runUntilStopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (runUntilStopCheckBox.Checked) iterationsBox.Enabled = false;
            else iterationsBox.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (triggeredCheckBox.Checked) controller.triggered = true;
            else controller.triggered = false;
        }

        private void preview_button_Click(object sender, EventArgs e)
        {

        }
    }
}
