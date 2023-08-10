
namespace AlFHardwareControl
{
    partial class TaskScheduler
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ScheduledEventsBox = new System.Windows.Forms.GroupBox();
            this.ScheduledEventsPanel = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SchedulerTasksPicker1 = new System.Windows.Forms.ComboBox();
            this.DiscardTimedSchedOnInterlockFail = new System.Windows.Forms.CheckBox();
            this.SubmitTimedSchedule = new System.Windows.Forms.Button();
            this.TimeSchedDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.EventLog = new System.Windows.Forms.RichTextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ResourcePicker = new System.Windows.Forms.ComboBox();
            this.ConditionTypePicker = new System.Windows.Forms.ComboBox();
            this.Value = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ResourceSchedTask = new System.Windows.Forms.ComboBox();
            this.ResourceEventDiscard = new System.Windows.Forms.CheckBox();
            this.ScheduleResourceEvent = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.macroGroupBox = new System.Windows.Forms.GroupBox();
            this.editMacro = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.macrosDropbox = new System.Windows.Forms.ComboBox();
            this.runMacro = new System.Windows.Forms.Button();
            this.ScheduledEventsBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.macroGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ScheduledEventsBox
            // 
            this.ScheduledEventsBox.Controls.Add(this.ScheduledEventsPanel);
            this.ScheduledEventsBox.Location = new System.Drawing.Point(3, 3);
            this.ScheduledEventsBox.Name = "ScheduledEventsBox";
            this.ScheduledEventsBox.Size = new System.Drawing.Size(404, 405);
            this.ScheduledEventsBox.TabIndex = 1;
            this.ScheduledEventsBox.TabStop = false;
            this.ScheduledEventsBox.Text = "Sceduled Events";
            this.ScheduledEventsBox.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // ScheduledEventsPanel
            // 
            this.ScheduledEventsPanel.AutoScroll = true;
            this.ScheduledEventsPanel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ScheduledEventsPanel.Location = new System.Drawing.Point(6, 19);
            this.ScheduledEventsPanel.Name = "ScheduledEventsPanel";
            this.ScheduledEventsPanel.Size = new System.Drawing.Size(392, 380);
            this.ScheduledEventsPanel.TabIndex = 10;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.SchedulerTasksPicker1);
            this.groupBox2.Controls.Add(this.DiscardTimedSchedOnInterlockFail);
            this.groupBox2.Controls.Add(this.SubmitTimedSchedule);
            this.groupBox2.Controls.Add(this.TimeSchedDate);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(413, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(313, 111);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Add Timed Event";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Scheduled Task";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // SchedulerTasksPicker1
            // 
            this.SchedulerTasksPicker1.FormattingEnabled = true;
            this.SchedulerTasksPicker1.Location = new System.Drawing.Point(107, 42);
            this.SchedulerTasksPicker1.Name = "SchedulerTasksPicker1";
            this.SchedulerTasksPicker1.Size = new System.Drawing.Size(200, 21);
            this.SchedulerTasksPicker1.TabIndex = 0;
            // 
            // DiscardTimedSchedOnInterlockFail
            // 
            this.DiscardTimedSchedOnInterlockFail.AutoSize = true;
            this.DiscardTimedSchedOnInterlockFail.Checked = true;
            this.DiscardTimedSchedOnInterlockFail.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DiscardTimedSchedOnInterlockFail.Location = new System.Drawing.Point(6, 82);
            this.DiscardTimedSchedOnInterlockFail.Name = "DiscardTimedSchedOnInterlockFail";
            this.DiscardTimedSchedOnInterlockFail.Size = new System.Drawing.Size(157, 17);
            this.DiscardTimedSchedOnInterlockFail.TabIndex = 6;
            this.DiscardTimedSchedOnInterlockFail.Text = "Discard on Interlock Failiure";
            this.DiscardTimedSchedOnInterlockFail.UseVisualStyleBackColor = true;
            // 
            // SubmitTimedSchedule
            // 
            this.SubmitTimedSchedule.Location = new System.Drawing.Point(169, 78);
            this.SubmitTimedSchedule.Name = "SubmitTimedSchedule";
            this.SubmitTimedSchedule.Size = new System.Drawing.Size(138, 23);
            this.SubmitTimedSchedule.TabIndex = 5;
            this.SubmitTimedSchedule.Text = "Schedule";
            this.SubmitTimedSchedule.UseVisualStyleBackColor = true;
            this.SubmitTimedSchedule.Click += new System.EventHandler(this.SubmitTimedSchedule_Click);
            // 
            // TimeSchedDate
            // 
            this.TimeSchedDate.CustomFormat = "yyyy/MM/dd HH:mm:ss";
            this.TimeSchedDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.TimeSchedDate.Location = new System.Drawing.Point(107, 16);
            this.TimeSchedDate.Name = "TimeSchedDate";
            this.TimeSchedDate.Size = new System.Drawing.Size(200, 20);
            this.TimeSchedDate.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Scheduled Time";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.EventLog);
            this.groupBox1.Location = new System.Drawing.Point(733, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(444, 404);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Event Log";
            // 
            // EventLog
            // 
            this.EventLog.Location = new System.Drawing.Point(7, 18);
            this.EventLog.Name = "EventLog";
            this.EventLog.ReadOnly = true;
            this.EventLog.Size = new System.Drawing.Size(431, 380);
            this.EventLog.TabIndex = 0;
            this.EventLog.Text = "";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ResourcePicker);
            this.groupBox3.Controls.Add(this.ConditionTypePicker);
            this.groupBox3.Controls.Add(this.Value);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.ResourceSchedTask);
            this.groupBox3.Controls.Add(this.ResourceEventDiscard);
            this.groupBox3.Controls.Add(this.ScheduleResourceEvent);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(413, 120);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(313, 111);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Add Resource Event";
            // 
            // ResourcePicker
            // 
            this.ResourcePicker.FormattingEnabled = true;
            this.ResourcePicker.Location = new System.Drawing.Point(107, 16);
            this.ResourcePicker.Name = "ResourcePicker";
            this.ResourcePicker.Size = new System.Drawing.Size(102, 21);
            this.ResourcePicker.TabIndex = 9;
            // 
            // ConditionTypePicker
            // 
            this.ConditionTypePicker.FormattingEnabled = true;
            this.ConditionTypePicker.Location = new System.Drawing.Point(215, 16);
            this.ConditionTypePicker.Name = "ConditionTypePicker";
            this.ConditionTypePicker.Size = new System.Drawing.Size(31, 21);
            this.ConditionTypePicker.TabIndex = 8;
            // 
            // Value
            // 
            this.Value.Location = new System.Drawing.Point(252, 16);
            this.Value.Name = "Value";
            this.Value.Size = new System.Drawing.Size(55, 20);
            this.Value.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Scheduled Task";
            // 
            // ResourceSchedTask
            // 
            this.ResourceSchedTask.FormattingEnabled = true;
            this.ResourceSchedTask.Location = new System.Drawing.Point(107, 42);
            this.ResourceSchedTask.Name = "ResourceSchedTask";
            this.ResourceSchedTask.Size = new System.Drawing.Size(200, 21);
            this.ResourceSchedTask.TabIndex = 0;
            // 
            // ResourceEventDiscard
            // 
            this.ResourceEventDiscard.AutoSize = true;
            this.ResourceEventDiscard.Checked = true;
            this.ResourceEventDiscard.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ResourceEventDiscard.Location = new System.Drawing.Point(6, 82);
            this.ResourceEventDiscard.Name = "ResourceEventDiscard";
            this.ResourceEventDiscard.Size = new System.Drawing.Size(157, 17);
            this.ResourceEventDiscard.TabIndex = 6;
            this.ResourceEventDiscard.Text = "Discard on Interlock Failiure";
            this.ResourceEventDiscard.UseVisualStyleBackColor = true;
            // 
            // ScheduleResourceEvent
            // 
            this.ScheduleResourceEvent.Location = new System.Drawing.Point(169, 78);
            this.ScheduleResourceEvent.Name = "ScheduleResourceEvent";
            this.ScheduleResourceEvent.Size = new System.Drawing.Size(138, 23);
            this.ScheduleResourceEvent.TabIndex = 5;
            this.ScheduleResourceEvent.Text = "Schedule";
            this.ScheduleResourceEvent.UseVisualStyleBackColor = true;
            this.ScheduleResourceEvent.Click += new System.EventHandler(this.ScheduleResourceEvent_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Condition";
            // 
            // macroGroupBox
            // 
            this.macroGroupBox.Controls.Add(this.editMacro);
            this.macroGroupBox.Controls.Add(this.label5);
            this.macroGroupBox.Controls.Add(this.macrosDropbox);
            this.macroGroupBox.Controls.Add(this.runMacro);
            this.macroGroupBox.Location = new System.Drawing.Point(413, 237);
            this.macroGroupBox.Name = "macroGroupBox";
            this.macroGroupBox.Size = new System.Drawing.Size(313, 79);
            this.macroGroupBox.TabIndex = 10;
            this.macroGroupBox.TabStop = false;
            this.macroGroupBox.Text = "Run Macro";
            // 
            // editMacro
            // 
            this.editMacro.Location = new System.Drawing.Point(9, 46);
            this.editMacro.Name = "editMacro";
            this.editMacro.Size = new System.Drawing.Size(138, 23);
            this.editMacro.TabIndex = 6;
            this.editMacro.Text = "Edit Macros";
            this.editMacro.UseVisualStyleBackColor = true;
            this.editMacro.Click += new System.EventHandler(this.editMacro_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Macro name";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // macrosDropbox
            // 
            this.macrosDropbox.FormattingEnabled = true;
            this.macrosDropbox.Location = new System.Drawing.Point(107, 19);
            this.macrosDropbox.Name = "macrosDropbox";
            this.macrosDropbox.Size = new System.Drawing.Size(200, 21);
            this.macrosDropbox.TabIndex = 0;
            this.macrosDropbox.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
            // 
            // runMacro
            // 
            this.runMacro.Location = new System.Drawing.Point(169, 46);
            this.runMacro.Name = "runMacro";
            this.runMacro.Size = new System.Drawing.Size(138, 23);
            this.runMacro.TabIndex = 5;
            this.runMacro.Text = "Run";
            this.runMacro.UseVisualStyleBackColor = true;
            this.runMacro.Click += new System.EventHandler(this.runMacro_Click);
            // 
            // TaskScheduler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.macroGroupBox);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.ScheduledEventsBox);
            this.Name = "TaskScheduler";
            this.Size = new System.Drawing.Size(1180, 411);
            this.Load += new System.EventHandler(this.DataGrapher_Load);
            this.ScheduledEventsBox.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.macroGroupBox.ResumeLayout(false);
            this.macroGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.GroupBox ScheduledEventsBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DateTimePicker TimeSchedDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SubmitTimedSchedule;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox SchedulerTasksPicker1;
        private System.Windows.Forms.CheckBox DiscardTimedSchedOnInterlockFail;
        public System.Windows.Forms.Panel ScheduledEventsPanel;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.RichTextBox EventLog;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox ResourcePicker;
        private System.Windows.Forms.ComboBox ConditionTypePicker;
        private System.Windows.Forms.TextBox Value;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ResourceSchedTask;
        private System.Windows.Forms.CheckBox ResourceEventDiscard;
        private System.Windows.Forms.Button ScheduleResourceEvent;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox macrosDropbox;
        private System.Windows.Forms.Button runMacro;
        private System.Windows.Forms.Button editMacro;
        public System.Windows.Forms.GroupBox macroGroupBox;
    }
}
