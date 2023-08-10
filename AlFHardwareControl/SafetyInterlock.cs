using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace AlFHardwareControl
{
    public partial class SafetyInterlock : ResourceEvent
    {
        Func<bool> FailCond;
        public SafetyInterlock(TaskScheduler _scheduler, string _resource, string comparison, string comparisonValue, string taskName, Func<bool> _failCond)
            : base(_scheduler, _resource, comparison, comparisonValue, taskName, false)
        {
            InitializeComponent();
            FailCond = _failCond;
        }

        public override void UpdateEvent()
        {
            if (!FailCond())
            {
                this.Invoke((Action) delegate { this.BackColor = SystemColors.ControlDark; });
                return;
            }
            else
            {
                this.Invoke((Action) delegate { this.BackColor = SystemColors.Control; });
            }
            if (scheduler.GetComparison(this.Comparison.Text)(resource(), this.Value.Text))
            {
                scheduler.UpdateEventLog("Safety Interlock \"" + this.TaskName.Text + "\" set to occur when " + this.Resource.Text + " " + this.Comparison.Text + " " + this.Value.Text + " triggered!");
                action(this.DiscardTimedSchedOnInterlockFail.Checked);
            }
        }

    }
}
