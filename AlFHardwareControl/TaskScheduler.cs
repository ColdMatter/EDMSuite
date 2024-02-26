using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using DAQ.Environment;
using System.Xml.Serialization;

namespace AlFHardwareControl
{
    public partial class TaskScheduler : UserControl
    {

        public Dictionary<string, Func<bool, object>> Tasks = new Dictionary<string, Func<bool, object>>();
        public Dictionary<string, Func<string>> Resources = new Dictionary<string, Func<string>>();

        public delegate bool ComparisonFunc(string a, string b);
        public Dictionary<string, ComparisonFunc> Comparisons = new Dictionary<string, ComparisonFunc>();

        public Dictionary<string, Func<List<string>, Func<bool, object>>> taskCommands = new Dictionary<string, Func<List<string>, Func<bool, object>>>();
        public Dictionary<string, Func<List<string>, Func<string>>> resourceCommands = new Dictionary<string, Func<List<string>, Func<string>>>();

        public MacroConfigurationCollection macroCollection;

        public Thread UpdateThread;

        public static TaskScheduler tSched;

        public TaskScheduler()
        {
            TaskScheduler.tSched = this;
            InitializeComponent();
            this.TimeSchedDate.Value = DateTime.Now;

            this.AddTask("", (bool discard) =>
            {
                return null;
            });

            this.AddTask("Print Time", (bool discard) =>
            {
                this.UpdateEventLog("The time is: " + DateTime.Now.ToString());
                return null;
            });

            this.AddComparison("<", (string a, string b) =>
            {
                return Convert.ToDouble(a) < Convert.ToDouble(b);
            });

            this.AddComparison(">", (string a, string b) =>
            {
                return Convert.ToDouble(a) > Convert.ToDouble(b);
            });
            
            this.AddComparison("is", (string a, string b) =>
            {
                return a == b;
            });


            try
            {
                System.IO.FileStream fs = System.IO.File.Open((string)Environs.Hardware.GetInfo("MacroConfig"), System.IO.FileMode.Open);
                XmlSerializer s = new XmlSerializer(typeof(MacroConfigurationCollection));
                macroCollection = (MacroConfigurationCollection)s.Deserialize(fs);
                fs.Close();
            }
            catch (System.IO.FileNotFoundException)
            {
                macroCollection = new MacroConfigurationCollection();
            }

            macroCollection.UndoSerialization();
            this.macrosDropbox.Items.Clear();
            foreach (MacroConfiguration macro in macroCollection)
                this.macrosDropbox.Items.Add(macro.Name);
            if (this.macrosDropbox.Items.Count > 0)
                this.macrosDropbox.SelectedIndex = 0;

            UpdateThread = new Thread(new ThreadStart(UpdateTasks));
            UpdateThread.Start();
        }

        public void AddResource(string name, Func<string> resource)
        {
            Resources.Add(name, resource);
            this.ResourcePicker.Items.Add(name);
            this.ResourcePicker.SelectedIndex = 0;
        }

        public void AddComparison(string name, ComparisonFunc resource)
        {
            Comparisons.Add(name, resource);
            this.ConditionTypePicker.Items.Add(name);
            this.ConditionTypePicker.SelectedIndex = 0;
        }

        public Func<string> GetResource(string name)
        {
            return Resources[name];
        }

        public ComparisonFunc GetComparison(string name)
        {
            return Comparisons[name];
        }

        public void AddTask(string name, Func<bool, object> action)
        {
            Tasks.Add(name, action);
            this.SchedulerTasksPicker1.Items.Add(name);
            this.ResourceSchedTask.Items.Add(name);
            this.SchedulerTasksPicker1.SelectedIndex = 0;
            this.ResourceSchedTask.SelectedIndex = 0;
        }

        public void UpdateScheduledLayout()
        {
            UpdateRenderedObject(this.ScheduledEventsPanel, (Panel pan) =>
            {
                int offset = pan.VerticalScroll.Value;
                int i = 0;
                foreach(Control con in pan.Controls)
                {
                    con.Location = new System.Drawing.Point(0, (i++) * 112 - offset);
                }
            });
        }

        private void UpdateTasks()
        {
            for(; ; )
            {
                foreach(ITaskSchedulerEvent uc in ScheduledEventsPanel.Controls.OfType<ITaskSchedulerEvent>())
                {
                    uc.UpdateEvent();
                }

                /*
                foreach (ResourceEvent uc in ScheduledEventsPanel.Controls.OfType<ResourceEvent>())
                {
                    uc.UpdateEvent();
                }

                foreach (MacroEvent uc in ScheduledEventsPanel.Controls.OfType<MacroEvent>())
                {
                    uc.UpdateEvent();
                }*/

                System.Threading.Thread.Sleep(1000);
            }
        }

        public void UpdateMacros()
        {
            this.Invoke((Action)(() =>
            {
                this.macrosDropbox.Items.Clear();
                foreach (MacroConfiguration macro in macroCollection)
                    this.macrosDropbox.Items.Add(macro.Name);
                if (this.macrosDropbox.Items.Count > 0)
                    this.macrosDropbox.SelectedIndex = 0;
            }));
        }

        public void UpdateEventLog(string update)
        {
            UpdateRenderedObject(this.EventLog, (RichTextBox box) =>
            {
                box.AppendText("[");
                box.AppendText(DateTime.Now.ToString());
                box.AppendText("] ");
                box.AppendText(update);
                box.AppendText("\n");
                box.ScrollToCaret();
            });
        }

        public void AddEvent<T>(T uc) where T : UserControl, ITaskSchedulerEvent
        {
            UpdateRenderedObject(this.ScheduledEventsPanel, (Panel box) => { box.Controls.Add(uc); box.Controls.SetChildIndex(uc, 0); });
            UpdateScheduledLayout();
        }

        public void RemoveEvent<T>(T uc) where T : UserControl, ITaskSchedulerEvent
        {
            UpdateRenderedObject(this.ScheduledEventsPanel, (Panel box) => { box.Controls.Remove(uc); });
            UpdateScheduledLayout();
        }


        #region Command Parsing

        private struct Token
        {
            string value;
            bool regex;
            bool dictionary;

        }
        /// <summary>
        /// Parses command strings. These have the form "!command param1 param2"
        /// Space is considered a delimiter between the tokens. Quotes can be used
        /// to have longer tokens. [] can be used to refer to a variable in a dictionary. Using quotes here are possible
        /// but not necessary. A resource can be prefaced with # to indicate that it should match all of the
        /// keys in the data dictionary and it should be expanded. Example command would be:
        /// !run #[Turn off.*]
        /// For a dictionary having keys "Turn off A", "Turn off B" and "Do C" this will expand to:
        /// !run "Turn off A" "Turn off B"
        /// </summary>
        /// <returns>
        /// A tokenised list of the command
        /// </returns>
        private List<Token> ParseCommand(string command)
        {
            List<Token> parsedCommand = new List<Token>();

            bool escaped = false;
            bool regex = false;
            bool dictionary = false;

            foreach (char c in command)
            {

            }



            return parsedCommand;
        }

        #endregion

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

        private void DataGrapher_Load(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void SubmitTimedSchedule_Click(object sender, EventArgs e)
        {
            if (this.TimeSchedDate.Value <= DateTime.Now)
            {
                UpdateEventLog("Scheduled Event can't be in the past!");
                return;
            }
            UpdateEventLog("Scheduling \"" + this.SchedulerTasksPicker1.Text + "\" for " + this.TimeSchedDate.Value.ToString());
            AddEvent(new TimedEvent(this, this.TimeSchedDate.Value, this.SchedulerTasksPicker1.Text, this.DiscardTimedSchedOnInterlockFail.Checked));
            this.TimeSchedDate.Value = DateTime.Now;
        }

        private void ScheduleResourceEvent_Click(object sender, EventArgs e)
        {
            if (Comparisons[this.ConditionTypePicker.Text](Resources[this.ResourcePicker.Text](), this.Value.Text))
            {
                UpdateEventLog("Condition can't already be true!");
                return;
            }
            UpdateEventLog("Scheduling \"" + this.ResourceSchedTask.Text + "\" for when " + this.ResourcePicker.Text + " " + this.ConditionTypePicker.Text + " " + this.Value.Text);
            AddEvent(new ResourceEvent(this, this.ResourcePicker.Text, this.ConditionTypePicker.Text, this.Value.Text, this.ResourceSchedTask.Text, this.ResourceEventDiscard.Checked));

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void editMacro_Click(object sender, EventArgs e)
        {
            UpdateRenderedObject(macroGroupBox, (Control c) => { c.Enabled = false; });
            MacroEditor mEditor = new MacroEditor(macroCollection,this);
            mEditor.ShowDialog();
        }

        private void runMacro_Click(object sender, EventArgs e)
        {
            UpdateEventLog("Scheduling \"" + this.macrosDropbox.SelectedItem + "\".");
            AddEvent(new MacroEvent(this, this.macrosDropbox.SelectedIndex));
        }

        public void Exit()
        {
            UpdateThread.Abort();
        }
    }
}
