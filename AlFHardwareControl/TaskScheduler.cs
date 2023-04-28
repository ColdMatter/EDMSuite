using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;

namespace AlFHardwareControl
{
    public partial class TaskScheduler : UserControl
    {

        public Dictionary<string, Action<bool>> Tasks = new Dictionary<string, Action<bool>>();
        public Dictionary<string, Func<string>> Resources = new Dictionary<string, Func<string>>();

        public delegate bool ComparisonFunc(string a, string b);
        public Dictionary<string, ComparisonFunc> Comparisons = new Dictionary<string, ComparisonFunc>();

        public TaskScheduler()
        {
            InitializeComponent();
            this.TimeSchedDate.Value = DateTime.Now;

            this.AddTask("Print Time", (bool discard) =>
            {
                this.UpdateEventLog("The time is: " + DateTime.Now.ToString());
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

            (new Thread(new ThreadStart(UpdateTasks))).Start();
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

        public void AddTask(string name, Action<bool> action)
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
                foreach(TimedEvent uc in ScheduledEventsPanel.Controls.OfType<TimedEvent>())
                {
                    uc.UpdateEvent();
                }

                foreach (ResourceEvent uc in ScheduledEventsPanel.Controls.OfType<ResourceEvent>())
                {
                    uc.UpdateEvent();
                }

                System.Threading.Thread.Sleep(1000);
            }
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

        public void AddEvent<T>(T uc) where T : UserControl
        {
            UpdateRenderedObject(this.ScheduledEventsPanel, (Panel box) => { box.Controls.Add(uc); box.Controls.SetChildIndex(uc, 0); });
            UpdateScheduledLayout();
        }

        public void RemoveEvent<T>(T uc) where T : UserControl
        {
            UpdateRenderedObject(this.ScheduledEventsPanel, (Panel box) => { box.Controls.Remove(uc); });
            UpdateScheduledLayout();
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
    }
}
