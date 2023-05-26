using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DAQ.Environment;
using System.Xml.Serialization;

namespace AlFHardwareControl
{
    public partial class MacroEditor : Form
    {

        public MacroConfigurationCollection macroCollection;
        public int selectedMacro = -1;
        public int selectedTask = -1;

        public int selectedTaskId
        {
            get
            {
                if (selectedTask == -1) return -1;
                return nameToID[(string)TaskList.Items[selectedTask]];
            }
        }

        private Dictionary<string, int> nameToID = new Dictionary<string, int> { };
        TaskScheduler ts;


        public MacroEditor(MacroConfigurationCollection _mc, TaskScheduler taskScheduler)
        {
            InitializeComponent();
            macroCollection = _mc;
            ts = taskScheduler;

            foreach (string comp in taskScheduler.Comparisons.Keys)
                ConditionTypePicker.Items.Add(comp);

            foreach (string res in taskScheduler.Resources.Keys)
                ResourcePicker.Items.Add(res);

            foreach (string task in taskScheduler.Tasks.Keys)
                ResourceSchedTask.Items.Add(task);
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

        private void MacroEditor_Load(object sender, EventArgs e)
        {
            foreach (MacroConfiguration macro in macroCollection.macros)
                MacroSelector.Items.Add(macro.Name);
        }

        private void MacroEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            macroCollection.PrepSerialization();
            XmlSerializer s = new XmlSerializer(typeof(MacroConfigurationCollection));
            System.IO.FileStream fstream = System.IO.File.Open((string)Environs.Hardware.GetInfo("MacroConfig"), System.IO.FileMode.Create);
            s.Serialize(fstream, macroCollection);
            fstream.Close();
            
            
            ts.UpdateRenderedObject(ts.macroGroupBox, (Control c) => { c.Enabled = true; });
            ts.UpdateMacros();
        }

        private void DeleteMacro_Click(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                if (this.MacroSelector.SelectedIndex == -1) return;
                if (this.MacroSelector.SelectedIndex == selectedMacro)
                {
                    selectedMacro = -1;
                    selectedTask = -1;
                    macroSettings.Enabled = false;
                    TaskGroupBox.Enabled = false;
                }
                if (this.MacroSelector.SelectedIndex < selectedMacro)
                    selectedMacro -= 1;
                macroCollection.RemoveMacro(this.MacroSelector.SelectedIndex);
                this.MacroSelector.Items.RemoveAt(this.MacroSelector.SelectedIndex);
            }));
        }

        private void NewMacro_Click(object sender, EventArgs e)
        {
            this.Invoke((Action)(() => {
                macroCollection.AddMacro();
                MacroSelector.Items.Add(macroCollection.macros.Last().Name);
            }));
        }

        private void MacroSelector_DoubleClick(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                if (MacroSelector.SelectedIndex == -1) return;
                selectedMacro = MacroSelector.SelectedIndex;
                macroSettings.Enabled = true;
                UpdateMacro();
            }));
        }

        private void UpdateMacro()
        {
            MacroName.Text = macroCollection.macros[selectedMacro].Name;
            Repeat.Checked = macroCollection.macros[selectedMacro].repeat;
            TaskList.Items.Clear();
            nameToID.Clear();
            foreach (MacroConfiguration.TaskNode tn in macroCollection.macros[selectedMacro].tasks.Values)
            {
                TaskList.Items.Add(tn.Name);
                nameToID.Add(tn.Name, tn.id);
            }
            selectedTask = -1;
            TaskGroupBox.Enabled = false;

        }

        private void TaskList_DoubleClick(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                if (TaskList.SelectedIndex == -1) return;
                selectedTask = TaskList.SelectedIndex;
                TaskGroupBox.Enabled = true;
                UpdateTask();
            }));
        }

        private void UpdateTask()
        {
            int index = nameToID[(string)TaskList.Items[selectedTask]];
            TaskNumber.Text = macroCollection.macros[selectedMacro].tasks[index].id.ToString();
            ResourceSchedTask.Text = macroCollection.macros[selectedMacro].tasks[index].TaskName;
            DelayHH.Text = macroCollection.macros[selectedMacro].tasks[index].delayHH.ToString();
            DelayMM.Text = macroCollection.macros[selectedMacro].tasks[index].delayMM.ToString();
            DelaySS.Text = macroCollection.macros[selectedMacro].tasks[index].delaySS.ToString();
            Prerequisites.Items.Clear();
            foreach (int i in macroCollection.macros[selectedMacro].tasks[index].prerequisites)
                Prerequisites.Items.Add(macroCollection.macros[selectedMacro].tasks[i].Name);
            isResourceEvent.Checked = macroCollection.macros[selectedMacro].tasks[index].conditional;
            ResourcePicker.Text = macroCollection.macros[selectedMacro].tasks[index].conditionResource;
            ConditionTypePicker.Text = macroCollection.macros[selectedMacro].tasks[index].conditionComparison;
            Value.Text = macroCollection.macros[selectedMacro].tasks[index].conditionValue;
            Start.Checked = macroCollection.macros[selectedMacro].tasks[index].start;
        }

        private void MacroName_TextChanged(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                MacroSelector.Items[selectedMacro] = MacroName.Text;
                macroCollection.macros[selectedMacro].Name = MacroName.Text;
            }));
        }

        private void RemoveTask_Click(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                if (this.TaskList.SelectedIndex == -1) return;
                if (this.TaskList.SelectedIndex == selectedTask)
                {
                    selectedTask = -1;
                    TaskGroupBox.Enabled = false;
                }
                if (this.TaskList.SelectedIndex < selectedTask)
                    selectedTask -= 1;


                macroCollection.macros[selectedMacro].RemoveTask(this.TaskList.SelectedIndex);
                this.TaskList.Items.RemoveAt(this.TaskList.SelectedIndex);
                if(selectedTask != -1)
                    UpdateTask();
            }));
        }

        private void AddTask_Click(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                int new_id = macroCollection.macros[selectedMacro].AddTask();
                nameToID.Add(macroCollection.macros[selectedMacro].tasks[new_id].Name, new_id);
                TaskList.Items.Add(macroCollection.macros[selectedMacro].tasks[new_id].Name);
            }));
        }

        private void ResourcePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            macroCollection.macros[selectedMacro].tasks[selectedTaskId].conditionResource = ResourcePicker.Text;
        }

        private void ConditionTypePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            macroCollection.macros[selectedMacro].tasks[selectedTaskId].conditionComparison = ConditionTypePicker.Text;
        }

        private void Value_Leave(object sender, EventArgs e)
        {
            macroCollection.macros[selectedMacro].tasks[selectedTaskId].conditionValue = Value.Text;
        }

        private void AddPrereq_Click(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                if (TaskList.SelectedIndex == -1) return;
                if (TaskList.SelectedIndex == selectedTask) return;
                macroCollection.macros[selectedMacro].AddPrerequisite(selectedTaskId, nameToID[(string)TaskList.Items[TaskList.SelectedIndex]]);
                UpdateTask();
            }));
        }

        private void RemovePrereq_Click(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                if (Prerequisites.SelectedIndex == -1) return;
                macroCollection.macros[selectedMacro].RemovePrerequisite(selectedTaskId, nameToID[(string)TaskList.Items[Prerequisites.SelectedIndex]]);
                UpdateTask();
            }));
        }

        private void DelayHH_Leave(object sender, EventArgs e)
        {
            try
            {
                macroCollection.macros[selectedMacro].tasks[selectedTaskId].delayHH = Convert.ToInt32(DelayHH.Text);
            }
            catch (FormatException)
            {
                this.SetTextField(this.DelayHH, macroCollection.macros[selectedMacro].tasks[selectedTaskId].delayHH.ToString());
            }
        }

        private void DelayMM_Leave(object sender, EventArgs e)
        {
            try
            {
                macroCollection.macros[selectedMacro].tasks[selectedTaskId].delayMM = Convert.ToInt32(DelayMM.Text);
            }
            catch (FormatException)
            {
                this.SetTextField(this.DelayMM, macroCollection.macros[selectedMacro].tasks[selectedTaskId].delayMM.ToString());
            }
        }

        private void DelaySS_Leave(object sender, EventArgs e)
        {
            try
            {
                macroCollection.macros[selectedMacro].tasks[selectedTaskId].delaySS = Convert.ToInt32(DelaySS.Text);
            }
            catch (FormatException)
            {
                this.SetTextField(this.DelaySS, macroCollection.macros[selectedMacro].tasks[selectedTaskId].delaySS.ToString());
            }
        }

        private void ResourceSchedTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                macroCollection.macros[selectedMacro].tasks[selectedTaskId].TaskName = ResourceSchedTask.Text;
                TaskList.Items.Clear();
                nameToID.Clear();
                foreach (MacroConfiguration.TaskNode tn in macroCollection.macros[selectedMacro].tasks.Values)
                {
                    TaskList.Items.Add(tn.Name);
                    nameToID.Add(tn.Name, tn.id);
                }
                UpdateTask();
            }));
        }

        private void isResourceEvent_CheckedChanged(object sender, EventArgs e)
        {
            macroCollection.macros[selectedMacro].tasks[selectedTaskId].conditional = isResourceEvent.Checked;
        }

        private void MacroName_Leave(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                MacroSelector.Items[selectedMacro] = MacroName.Text;
            }));
        }

        private void Repeat_CheckedChanged(object sender, EventArgs e)
        {
            macroCollection.macros[selectedMacro].repeat = Repeat.Checked;
        }
    }
}
