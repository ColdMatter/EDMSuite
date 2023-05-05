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
    public partial class ResourceEvent : UserControl
    {
        protected TaskScheduler scheduler;
        protected Func<bool,object> action;
        protected Func<string> resource;

        [Obsolete("Designer only", true)]
        public ResourceEvent() : base()
        {
            InitializeComponent();
            this.Resource.Text = "";
            this.Comparison.Text = "";
            this.Value.Text = "";
            this.TaskName.Text = "";
            this.DiscardTimedSchedOnInterlockFail.Checked = false;
        }

        public ResourceEvent(TaskScheduler _scheduler, string _resource, string comparison, string comparisonValue, string taskName, bool interlockFail)
        {
            InitializeComponent();
            scheduler = _scheduler;
            this.Resource.Text = _resource;
            this.Comparison.Text = comparison;
            this.Value.Text = comparisonValue;
            this.TaskName.Text = taskName;
            this.DiscardTimedSchedOnInterlockFail.Checked = interlockFail;
            action = scheduler.Tasks[taskName];

            try
            {
                resource = scheduler.GetResource(_resource);
            }
            catch (KeyNotFoundException e)
            {
                scheduler.UpdateEventLog("No resource named " + _resource + " exists! Discarding malformed event.");
                scheduler.RemoveEvent(this);
                scheduler.UpdateScheduledLayout();
            }
        }

        public virtual void UpdateEvent()
        {
            if (scheduler.GetComparison(this.Comparison.Text)(resource(), this.Value.Text))
            {
                scheduler.UpdateEventLog("Executing resource event \"" + this.TaskName.Text + "\" set to run when " + this.Resource.Text + " " + this.Comparison.Text + " " + this.Value.Text);
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
            scheduler.UpdateEventLog("Dismissed event \"" + this.TaskName.Text + "\" set to run when " + this.Resource.Text + " " + this.Comparison.Text + " " + this.Value.Text);
            scheduler.RemoveEvent(this);
            scheduler.UpdateScheduledLayout();
        }
    }
}
