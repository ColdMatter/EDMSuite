
namespace AlFHardwareControl
{
    partial class MacroEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MacroSelector = new System.Windows.Forms.ListBox();
            this.NewMacro = new System.Windows.Forms.Button();
            this.DeleteMacro = new System.Windows.Forms.Button();
            this.macroSettings = new System.Windows.Forms.GroupBox();
            this.TaskGroupBox = new System.Windows.Forms.GroupBox();
            this.ResourcePicker = new System.Windows.Forms.ComboBox();
            this.ConditionTypePicker = new System.Windows.Forms.ComboBox();
            this.Value = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RemoveTask = new System.Windows.Forms.Button();
            this.AddTask = new System.Windows.Forms.Button();
            this.RemovePrereq = new System.Windows.Forms.Button();
            this.AddPrereq = new System.Windows.Forms.Button();
            this.TaskNumber = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Prerequisites = new System.Windows.Forms.ListBox();
            this.DelayHH = new System.Windows.Forms.TextBox();
            this.DelayMM = new System.Windows.Forms.TextBox();
            this.DelaySS = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.isResourceEvent = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ResourceSchedTask = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Repeat = new System.Windows.Forms.CheckBox();
            this.MacroName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TaskList = new System.Windows.Forms.ListBox();
            this.Start = new System.Windows.Forms.CheckBox();
            this.macroSettings.SuspendLayout();
            this.TaskGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MacroSelector
            // 
            this.MacroSelector.FormattingEnabled = true;
            this.MacroSelector.Location = new System.Drawing.Point(12, 12);
            this.MacroSelector.Name = "MacroSelector";
            this.MacroSelector.Size = new System.Drawing.Size(203, 368);
            this.MacroSelector.TabIndex = 0;
            this.MacroSelector.DoubleClick += new System.EventHandler(this.MacroSelector_DoubleClick);
            // 
            // NewMacro
            // 
            this.NewMacro.Location = new System.Drawing.Point(12, 385);
            this.NewMacro.Name = "NewMacro";
            this.NewMacro.Size = new System.Drawing.Size(100, 23);
            this.NewMacro.TabIndex = 1;
            this.NewMacro.Text = "New";
            this.NewMacro.UseVisualStyleBackColor = true;
            this.NewMacro.Click += new System.EventHandler(this.NewMacro_Click);
            // 
            // DeleteMacro
            // 
            this.DeleteMacro.Location = new System.Drawing.Point(114, 385);
            this.DeleteMacro.Name = "DeleteMacro";
            this.DeleteMacro.Size = new System.Drawing.Size(100, 23);
            this.DeleteMacro.TabIndex = 2;
            this.DeleteMacro.Text = "Delete";
            this.DeleteMacro.UseVisualStyleBackColor = true;
            this.DeleteMacro.Click += new System.EventHandler(this.DeleteMacro_Click);
            // 
            // macroSettings
            // 
            this.macroSettings.Controls.Add(this.TaskGroupBox);
            this.macroSettings.Controls.Add(this.groupBox1);
            this.macroSettings.Controls.Add(this.TaskList);
            this.macroSettings.Controls.Add(this.AddTask);
            this.macroSettings.Controls.Add(this.RemoveTask);
            this.macroSettings.Enabled = false;
            this.macroSettings.Location = new System.Drawing.Point(222, 13);
            this.macroSettings.Name = "macroSettings";
            this.macroSettings.Size = new System.Drawing.Size(566, 395);
            this.macroSettings.TabIndex = 3;
            this.macroSettings.TabStop = false;
            this.macroSettings.Text = "Macro settings";
            // 
            // TaskGroupBox
            // 
            this.TaskGroupBox.Controls.Add(this.Start);
            this.TaskGroupBox.Controls.Add(this.ResourcePicker);
            this.TaskGroupBox.Controls.Add(this.ConditionTypePicker);
            this.TaskGroupBox.Controls.Add(this.Value);
            this.TaskGroupBox.Controls.Add(this.label6);
            this.TaskGroupBox.Controls.Add(this.RemovePrereq);
            this.TaskGroupBox.Controls.Add(this.AddPrereq);
            this.TaskGroupBox.Controls.Add(this.TaskNumber);
            this.TaskGroupBox.Controls.Add(this.label4);
            this.TaskGroupBox.Controls.Add(this.Prerequisites);
            this.TaskGroupBox.Controls.Add(this.DelayHH);
            this.TaskGroupBox.Controls.Add(this.DelayMM);
            this.TaskGroupBox.Controls.Add(this.DelaySS);
            this.TaskGroupBox.Controls.Add(this.label2);
            this.TaskGroupBox.Controls.Add(this.isResourceEvent);
            this.TaskGroupBox.Controls.Add(this.label3);
            this.TaskGroupBox.Controls.Add(this.ResourceSchedTask);
            this.TaskGroupBox.Enabled = false;
            this.TaskGroupBox.Location = new System.Drawing.Point(216, 99);
            this.TaskGroupBox.Name = "TaskGroupBox";
            this.TaskGroupBox.Size = new System.Drawing.Size(344, 288);
            this.TaskGroupBox.TabIndex = 6;
            this.TaskGroupBox.TabStop = false;
            this.TaskGroupBox.Text = "Task";
            // 
            // ResourcePicker
            // 
            this.ResourcePicker.FormattingEnabled = true;
            this.ResourcePicker.Location = new System.Drawing.Point(86, 167);
            this.ResourcePicker.Name = "ResourcePicker";
            this.ResourcePicker.Size = new System.Drawing.Size(146, 21);
            this.ResourcePicker.TabIndex = 19;
            this.ResourcePicker.SelectedIndexChanged += new System.EventHandler(this.ResourcePicker_SelectedIndexChanged);
            // 
            // ConditionTypePicker
            // 
            this.ConditionTypePicker.FormattingEnabled = true;
            this.ConditionTypePicker.Location = new System.Drawing.Point(238, 167);
            this.ConditionTypePicker.Name = "ConditionTypePicker";
            this.ConditionTypePicker.Size = new System.Drawing.Size(31, 21);
            this.ConditionTypePicker.TabIndex = 18;
            this.ConditionTypePicker.SelectedIndexChanged += new System.EventHandler(this.ConditionTypePicker_SelectedIndexChanged);
            // 
            // Value
            // 
            this.Value.Location = new System.Drawing.Point(275, 167);
            this.Value.Name = "Value";
            this.Value.Size = new System.Drawing.Size(63, 20);
            this.Value.TabIndex = 17;
            this.Value.Leave += new System.EventHandler(this.Value_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 170);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Condition";
            // 
            // RemoveTask
            // 
            this.RemoveTask.Location = new System.Drawing.Point(119, 364);
            this.RemoveTask.Name = "RemoveTask";
            this.RemoveTask.Size = new System.Drawing.Size(90, 23);
            this.RemoveTask.TabIndex = 15;
            this.RemoveTask.Text = "Remove";
            this.RemoveTask.UseVisualStyleBackColor = true;
            this.RemoveTask.Click += new System.EventHandler(this.RemoveTask_Click);
            // 
            // AddTask
            // 
            this.AddTask.Location = new System.Drawing.Point(6, 364);
            this.AddTask.Name = "AddTask";
            this.AddTask.Size = new System.Drawing.Size(90, 23);
            this.AddTask.TabIndex = 14;
            this.AddTask.Text = "Add Task";
            this.AddTask.UseVisualStyleBackColor = true;
            this.AddTask.Click += new System.EventHandler(this.AddTask_Click);
            // 
            // RemovePrereq
            // 
            this.RemovePrereq.Location = new System.Drawing.Point(86, 112);
            this.RemovePrereq.Name = "RemovePrereq";
            this.RemovePrereq.Size = new System.Drawing.Size(75, 23);
            this.RemovePrereq.TabIndex = 13;
            this.RemovePrereq.Text = "Remove";
            this.RemovePrereq.UseVisualStyleBackColor = true;
            this.RemovePrereq.Click += new System.EventHandler(this.RemovePrereq_Click);
            // 
            // AddPrereq
            // 
            this.AddPrereq.Location = new System.Drawing.Point(86, 83);
            this.AddPrereq.Name = "AddPrereq";
            this.AddPrereq.Size = new System.Drawing.Size(75, 23);
            this.AddPrereq.TabIndex = 4;
            this.AddPrereq.Text = "Add";
            this.AddPrereq.UseVisualStyleBackColor = true;
            this.AddPrereq.Click += new System.EventHandler(this.AddPrereq_Click);
            // 
            // TaskNumber
            // 
            this.TaskNumber.AutoSize = true;
            this.TaskNumber.Location = new System.Drawing.Point(45, 27);
            this.TaskNumber.Name = "TaskNumber";
            this.TaskNumber.Size = new System.Drawing.Size(13, 13);
            this.TaskNumber.TabIndex = 12;
            this.TaskNumber.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 83);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Prerequisites";
            // 
            // Prerequisites
            // 
            this.Prerequisites.FormattingEnabled = true;
            this.Prerequisites.Location = new System.Drawing.Point(167, 83);
            this.Prerequisites.Name = "Prerequisites";
            this.Prerequisites.Size = new System.Drawing.Size(171, 56);
            this.Prerequisites.TabIndex = 10;
            // 
            // DelayHH
            // 
            this.DelayHH.Location = new System.Drawing.Point(167, 56);
            this.DelayHH.Name = "DelayHH";
            this.DelayHH.Size = new System.Drawing.Size(53, 20);
            this.DelayHH.TabIndex = 9;
            this.DelayHH.Text = "00";
            this.DelayHH.Leave += new System.EventHandler(this.DelayHH_Leave);
            // 
            // DelayMM
            // 
            this.DelayMM.Location = new System.Drawing.Point(226, 56);
            this.DelayMM.Name = "DelayMM";
            this.DelayMM.Size = new System.Drawing.Size(53, 20);
            this.DelayMM.TabIndex = 8;
            this.DelayMM.Text = "00";
            this.DelayMM.Leave += new System.EventHandler(this.DelayMM_Leave);
            // 
            // DelaySS
            // 
            this.DelaySS.Location = new System.Drawing.Point(285, 56);
            this.DelaySS.Name = "DelaySS";
            this.DelaySS.Size = new System.Drawing.Size(53, 20);
            this.DelaySS.TabIndex = 7;
            this.DelaySS.Text = "00";
            this.DelaySS.Leave += new System.EventHandler(this.DelaySS_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Delay";
            // 
            // isResourceEvent
            // 
            this.isResourceEvent.AutoSize = true;
            this.isResourceEvent.Location = new System.Drawing.Point(6, 141);
            this.isResourceEvent.Name = "isResourceEvent";
            this.isResourceEvent.Size = new System.Drawing.Size(78, 17);
            this.isResourceEvent.TabIndex = 3;
            this.isResourceEvent.Text = "Conditional";
            this.isResourceEvent.UseVisualStyleBackColor = true;
            this.isResourceEvent.CheckedChanged += new System.EventHandler(this.isResourceEvent_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Task";
            // 
            // ResourceSchedTask
            // 
            this.ResourceSchedTask.FormattingEnabled = true;
            this.ResourceSchedTask.Location = new System.Drawing.Point(86, 24);
            this.ResourceSchedTask.Name = "ResourceSchedTask";
            this.ResourceSchedTask.Size = new System.Drawing.Size(252, 21);
            this.ResourceSchedTask.TabIndex = 2;
            this.ResourceSchedTask.SelectedIndexChanged += new System.EventHandler(this.ResourceSchedTask_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Repeat);
            this.groupBox1.Controls.Add(this.MacroName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(216, 20);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(344, 72);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            // 
            // Repeat
            // 
            this.Repeat.AutoSize = true;
            this.Repeat.Location = new System.Drawing.Point(10, 49);
            this.Repeat.Name = "Repeat";
            this.Repeat.Size = new System.Drawing.Size(61, 17);
            this.Repeat.TabIndex = 2;
            this.Repeat.Text = "Repeat";
            this.Repeat.UseVisualStyleBackColor = true;
            this.Repeat.CheckedChanged += new System.EventHandler(this.Repeat_CheckedChanged);
            // 
            // MacroName
            // 
            this.MacroName.Location = new System.Drawing.Point(49, 20);
            this.MacroName.Name = "MacroName";
            this.MacroName.Size = new System.Drawing.Size(289, 20);
            this.MacroName.TabIndex = 1;
            this.MacroName.TextChanged += new System.EventHandler(this.MacroName_TextChanged);
            this.MacroName.Leave += new System.EventHandler(this.MacroName_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // TaskList
            // 
            this.TaskList.FormattingEnabled = true;
            this.TaskList.Location = new System.Drawing.Point(6, 19);
            this.TaskList.Name = "TaskList";
            this.TaskList.Size = new System.Drawing.Size(203, 342);
            this.TaskList.TabIndex = 4;
            this.TaskList.DoubleClick += new System.EventHandler(this.TaskList_DoubleClick);
            // 
            // Start
            // 
            this.Start.AutoSize = true;
            this.Start.Enabled = false;
            this.Start.Location = new System.Drawing.Point(6, 265);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(48, 17);
            this.Start.TabIndex = 20;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            // 
            // MacroEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 420);
            this.Controls.Add(this.macroSettings);
            this.Controls.Add(this.DeleteMacro);
            this.Controls.Add(this.NewMacro);
            this.Controls.Add(this.MacroSelector);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MacroEditor";
            this.Text = "Macros";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MacroEditor_FormClosing);
            this.Load += new System.EventHandler(this.MacroEditor_Load);
            this.macroSettings.ResumeLayout(false);
            this.TaskGroupBox.ResumeLayout(false);
            this.TaskGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox MacroSelector;
        private System.Windows.Forms.Button NewMacro;
        private System.Windows.Forms.Button DeleteMacro;
        private System.Windows.Forms.GroupBox macroSettings;
        private System.Windows.Forms.ListBox TaskList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox MacroName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox Repeat;
        private System.Windows.Forms.GroupBox TaskGroupBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ResourceSchedTask;
        private System.Windows.Forms.CheckBox isResourceEvent;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox Prerequisites;
        private System.Windows.Forms.TextBox DelayHH;
        private System.Windows.Forms.TextBox DelayMM;
        private System.Windows.Forms.TextBox DelaySS;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button RemoveTask;
        private System.Windows.Forms.Button AddTask;
        private System.Windows.Forms.Button RemovePrereq;
        private System.Windows.Forms.Button AddPrereq;
        private System.Windows.Forms.ComboBox ResourcePicker;
        private System.Windows.Forms.ComboBox ConditionTypePicker;
        private System.Windows.Forms.TextBox Value;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.Label TaskNumber;
        private System.Windows.Forms.CheckBox Start;
    }
}