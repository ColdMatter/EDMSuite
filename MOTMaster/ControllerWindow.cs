using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Grey out everything that starts a run while one is in progress. Called from
        /// whichever thread the run is on, so it marshals to the UI thread. BeginInvoke,
        /// not Invoke, so a run thread can never block on a busy UI thread. Reads
        /// controller.IsRunning when it lands rather than taking a value, so posts from
        /// a finishing run and the next run can't arrive out of order and leave Run lit.
        /// </summary>
        public void RefreshRunControls()
        {
            if (!IsHandleCreated || IsDisposed) return;
            try
            {
                BeginInvoke(new Action(applyRunControls));
            }
            // The window is closing while a run winds down; nothing left to update.
            catch (InvalidOperationException) { }
        }

        // Runs on the UI thread.
        private void applyRunControls()
        {
            bool locked = controller.IsLockedByPrimary;

            bool enabled = !controller.IsRunning;
            enabled &= !locked;
            runButton.Enabled = enabled;
            ReplicateScriptToolStripMenuItem.Enabled = enabled;

            // These are not greyed by a run today; they are greyed only while another MOTMaster controls this one.
            bool notLocked = !locked;
            selectScriptButton.Enabled = notLocked;
            newPatternToolStripMenuItem.Enabled = notLocked;
            lookupScriptsButton.Enabled = notLocked;
            scriptListComboBox.Enabled = notLocked;
            triggeredCheckBox.Enabled = notLocked;

            linkBannerLabel.Visible = locked;
            breakLinkButton.Visible = locked;
            // Not during a linked run (TakeBackControl refuses then too).
            breakLinkButton.Enabled = !controller.IsRunning;
            if (locked) linkBannerLabel.Text = "Controlled by " + controller.LockedBy + " (linked run)";

            // The primary's Linked box: not while a change is in flight, and not mid-run (SetLinked refuses then).
            linkedCheckBox.Enabled = !linkedChangeInProgress && !controller.IsRunning;

            // SetLinked is also callable remotely (Python), so follow the controller's state; not while our own change is
            // in flight (linkedChangeFinished resyncs then).
            if (!linkedChangeInProgress && linkedCheckBox.Checked != controller.IsLinked)
            {
                syncingLinkedBox = true;
                try { linkedCheckBox.Checked = controller.IsLinked; }
                finally { syncingLinkedBox = false; }
            }
        }

        #region Linked mode

        private bool linkedChangeInProgress = false;
        // Set while the box is being synced from controller.IsLinked, so that does not re-trigger the handler.
        private bool syncingLinkedBox = false;

        private void initLinkedControls()
        {
            controller.LinkStateChanged += RefreshRunControls;

            if (controller.LinkConfig != null)
            {
                string host = controller.LinkConfig.SecondaryUrl;
                try { host = new Uri(controller.LinkConfig.SecondaryUrl).Host; }
                catch (UriFormatException) { }
                linkedCheckBox.Text = "Linked to " + host;
                linkedCheckBox.Visible = true;
            }
            else
            {
                linkedCheckBox.Visible = false;
            }
            applyRunControls();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            controller.LinkStateChanged -= RefreshRunControls;
            base.OnFormClosed(e);
        }

        private void linkedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (syncingLinkedBox) return;
            bool want = linkedCheckBox.Checked;
            linkedChangeInProgress = true;
            linkedCheckBox.Enabled = false;
            // SetLinked makes a blocking remoting call (a TCP timeout if the secondary is unreachable): keep it off the UI thread.
            Task.Run(() =>
            {
                string error = null;
                try { controller.SetLinked(want); }
                catch (Exception ex) { error = ex.Message; }
                if (IsDisposed || !IsHandleCreated) return;
                try
                {
                    BeginInvoke(new Action(() => linkedChangeFinished(error)));
                }
                catch (InvalidOperationException) { } // the window closed meanwhile
            });
        }

        private void linkedChangeFinished(string error)
        {
            linkedChangeInProgress = false;
            // The truth, not an assumption: whatever happened, show what the controller says.
            syncingLinkedBox = true;
            try { linkedCheckBox.Checked = controller.IsLinked; }
            finally { syncingLinkedBox = false; }
            applyRunControls();
            if (error != null) MessageBox.Show(this, error, "Linked mode");
        }

        private void breakLinkButton_Click(object sender, EventArgs e)
        {
            // The button is greyed during a linked run; this covers a run that started just as it was clicked.
            if (!controller.TakeBackControl())
                MessageBox.Show(this, "A linked run is in progress. Control can be taken back once it has finished.",
                    "Take back control");
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
            initLinkedControls();
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

        public bool TriggeredCheckBoxState
        {
            get { return triggeredCheckBox.Checked; }
            set { triggeredCheckBox.Checked = value; }
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

        private void viewPatternButton_Click(object sender, EventArgs e)
        {
            controller.ViewPattern();
        }

        private void editParameterFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParameterWindow parameterWindow = new ParameterWindow();
            parameterWindow.Show();
        }
    }
}
