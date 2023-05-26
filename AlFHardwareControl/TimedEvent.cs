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
    public partial class TimedEvent : UserControl, ITaskSchedulerEvent
    {
        private TaskScheduler scheduler;
        private Func<bool, object> action;
        public TimedEvent(TaskScheduler _scheduler, DateTime scheduledFor, string taskName, bool interlockFail)
        {
            InitializeComponent();
            scheduler = _scheduler;
            this.TimeSchedDate.Value = scheduledFor;
            this.TaskName.Text = taskName;
            this.DiscardTimedSchedOnInterlockFail.Checked = interlockFail;
            action = scheduler.Tasks[taskName];
        }

        public void UpdateEvent()
        {
            DateTime time = DateTime.Now;
            if (time > this.TimeSchedDate.Value)
            {
                scheduler.UpdateEventLog("Executing timed event " + this.TaskName.Text);
                action(this.DiscardTimedSchedOnInterlockFail.Checked);
                scheduler.RemoveEvent(this);
                scheduler.UpdateScheduledLayout();
            } 

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Dismiss_Click(object sender, EventArgs e)
        {
            scheduler.UpdateEventLog("Dismissed event \"" + this.TaskName.Text + "\" set to run at " + this.TimeSchedDate.Value);
            scheduler.RemoveEvent(this);
            scheduler.UpdateScheduledLayout();
        }
    }
}
