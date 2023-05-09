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
    public partial class MacroEvent : UserControl, ITaskSchedulerEvent
    {
        private TaskScheduler scheduler;
        MacroConfiguration macro;

        private class SchedTask
        {
            public string name;
            public Func<bool, object> action;
            public bool isConditional;
            public ResourceCondition condition;
            public DateTime delayedStart;
            public MacroConfiguration.TaskNode task;

            public SchedTask() { }
        }

        Dictionary<int, bool> prereqs = new Dictionary<int, bool>();
        List<SchedTask> runningTasks = new List<SchedTask>(); 

        public MacroEvent(TaskScheduler _scheduler, int macroID)
        {
            InitializeComponent();
            scheduler = _scheduler;

            macro = _scheduler.macroCollection[macroID];
            MacroName.Text = macro.Name;
            Repeat.Checked = macro.repeat;

            foreach (MacroConfiguration.TaskNode task in macro.tasks.Values)
            {
                prereqs[task.id] = false;
                if (task.start) initEvent(task);
            }
            this.ActiveTasks.Items.Clear();
            foreach (SchedTask task in runningTasks)
                this.ActiveTasks.Items.Add(task.name);
        }

        public void resetStatus()
        {
            foreach (MacroConfiguration.TaskNode task in macro.tasks.Values)
            {
                prereqs[task.id] = false;
                if (task.start) initEvent(task);
            }
            UpdateRunningTasks();
        }

        public void isFinished()
        {
            foreach (int id in macro.tasks.Keys)
                if (!prereqs[id]) return;

            if (!macro.repeat)
            {
                scheduler.UpdateEventLog("Macro \"" + this.MacroName.Text + "\" finished.");
                scheduler.RemoveEvent(this);
                scheduler.UpdateScheduledLayout();
                return;
            }
            scheduler.UpdateEventLog("Macro \"" + this.MacroName.Text + "\" finished. Restarting");
            resetStatus();
        }

        public void initEvent(MacroConfiguration.TaskNode tn)
        {
            foreach (int prereq in tn.prerequisites)
                if (!prereqs[prereq]) return;
            SchedTask task = new SchedTask();
            task.action = scheduler.Tasks[tn.TaskName];
            task.name = tn.Name;
            task.isConditional = tn.conditional;
            task.condition = new ResourceCondition(tn.conditionResource, tn.conditionComparison, tn.conditionValue);
            task.delayedStart = DateTime.Now.AddHours(tn.delayHH)
                                            .AddMinutes(tn.delayMM)
                                            .AddSeconds(tn.delaySS);
            task.task = tn;
            runningTasks.Add(task);
        }

        public void UpdateEvent()
        {
            List<SchedTask> complete = new List<SchedTask>();
            foreach (SchedTask task in runningTasks)
            {
                if (task.delayedStart > DateTime.Now) continue;
                if (!task.isConditional || scheduler.Comparisons[task.condition.Comparison](scheduler.Resources[task.condition.Resource](), task.condition.Value))
                {
                    scheduler.UpdateEventLog("Executing sub event \"" + task.name + "\" in macro \"" + this.MacroName.Text + "\".");
                    object cond = task.action(true);
                    if (cond == null)
                    {
                        complete.Add(task);
                        continue;
                    }
                    task.condition = (ResourceCondition)cond;
                    task.isConditional = true;
                    task.name += "[!]";
                }
            }
            foreach (SchedTask task in complete)
            {
                prereqs[task.task.id] = true;
                foreach (int id in task.task.follows)
                {
                    initEvent(macro.tasks[id]);
                }
                runningTasks.Remove(task);
            }
            UpdateRunningTasks();
            isFinished();
        }

        public void UpdateRunningTasks()
        {
            this.Invoke((Action)(() =>
            {
                this.ActiveTasks.Items.Clear();
                foreach (SchedTask task in runningTasks)
                    this.ActiveTasks.Items.Add(task.name);
            }));
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Dismiss_Click(object sender, EventArgs e)
        {
            scheduler.UpdateEventLog("Dismissed macro \"" + this.MacroName.Text + "\".");
            scheduler.RemoveEvent(this);
            scheduler.UpdateScheduledLayout();
        }
    }
}
