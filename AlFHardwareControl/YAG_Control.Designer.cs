
namespace AlFHardwareControl
{
    partial class YAG_Control
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
            this.YAG_Control_Group = new System.Windows.Forms.GroupBox();
            this.Freq = new AlFHardwareControl.ParamSet();
            this.Temp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.qswitch = new System.Windows.Forms.CheckBox();
            this.flashlamp = new System.Windows.Forms.CheckBox();
            this.Status = new System.Windows.Forms.TextBox();
            this.IF = new System.Windows.Forms.TextBox();
            this.shutter = new System.Windows.Forms.CheckBox();
            this.ext_flash = new System.Windows.Forms.CheckBox();
            this.ext_Q = new System.Windows.Forms.CheckBox();
            this.Ene = new AlFHardwareControl.ParamSet();
            this.Delay = new AlFHardwareControl.ParamSet();
            this.VIS = new AlFHardwareControl.ParamSet();
            this.VOS = new AlFHardwareControl.ParamSet();
            this.VMO = new AlFHardwareControl.ParamSet();
            this.Shutdown = new System.Windows.Forms.Button();
            this.Conn_status = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.YAG_Control_Group.SuspendLayout();
            this.SuspendLayout();
            // 
            // YAG_Control_Group
            // 
            this.YAG_Control_Group.Controls.Add(this.Freq);
            this.YAG_Control_Group.Controls.Add(this.Temp);
            this.YAG_Control_Group.Controls.Add(this.label2);
            this.YAG_Control_Group.Controls.Add(this.qswitch);
            this.YAG_Control_Group.Controls.Add(this.flashlamp);
            this.YAG_Control_Group.Controls.Add(this.Status);
            this.YAG_Control_Group.Controls.Add(this.IF);
            this.YAG_Control_Group.Controls.Add(this.shutter);
            this.YAG_Control_Group.Controls.Add(this.ext_flash);
            this.YAG_Control_Group.Controls.Add(this.ext_Q);
            this.YAG_Control_Group.Controls.Add(this.Ene);
            this.YAG_Control_Group.Controls.Add(this.Delay);
            this.YAG_Control_Group.Controls.Add(this.VIS);
            this.YAG_Control_Group.Controls.Add(this.VOS);
            this.YAG_Control_Group.Controls.Add(this.VMO);
            this.YAG_Control_Group.Controls.Add(this.Shutdown);
            this.YAG_Control_Group.Controls.Add(this.Conn_status);
            this.YAG_Control_Group.Controls.Add(this.label1);
            this.YAG_Control_Group.Location = new System.Drawing.Point(4, 4);
            this.YAG_Control_Group.Name = "YAG_Control_Group";
            this.YAG_Control_Group.Size = new System.Drawing.Size(214, 404);
            this.YAG_Control_Group.TabIndex = 0;
            this.YAG_Control_Group.TabStop = false;
            this.YAG_Control_Group.Text = "YAG Control";
            // 
            // Freq
            // 
            this.Freq.Label = "Freq";
            this.Freq.Location = new System.Drawing.Point(6, 242);
            this.Freq.Name = "Freq";
            this.Freq.Size = new System.Drawing.Size(203, 27);
            this.Freq.TabIndex = 6;
            this.Freq.OnSetClick += new System.EventHandler(this.Freq_OnSetClick);
            // 
            // Temp
            // 
            this.Temp.Enabled = false;
            this.Temp.Location = new System.Drawing.Point(162, 326);
            this.Temp.Name = "Temp";
            this.Temp.Size = new System.Drawing.Size(44, 20);
            this.Temp.TabIndex = 18;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(122, 329);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Temp";
            // 
            // qswitch
            // 
            this.qswitch.AutoSize = true;
            this.qswitch.Enabled = false;
            this.qswitch.Location = new System.Drawing.Point(133, 281);
            this.qswitch.Name = "qswitch";
            this.qswitch.Size = new System.Drawing.Size(69, 17);
            this.qswitch.TabIndex = 8;
            this.qswitch.Text = "Q-Switch";
            this.qswitch.UseVisualStyleBackColor = true;
            this.qswitch.CheckedChanged += new System.EventHandler(this.qswitch_CheckedChanged);
            // 
            // flashlamp
            // 
            this.flashlamp.AutoSize = true;
            this.flashlamp.Location = new System.Drawing.Point(133, 303);
            this.flashlamp.Name = "flashlamp";
            this.flashlamp.Size = new System.Drawing.Size(73, 17);
            this.flashlamp.TabIndex = 10;
            this.flashlamp.Text = "Flashlamp";
            this.flashlamp.UseVisualStyleBackColor = true;
            this.flashlamp.CheckedChanged += new System.EventHandler(this.flashlamp_CheckedChanged);
            // 
            // Status
            // 
            this.Status.Enabled = false;
            this.Status.Location = new System.Drawing.Point(10, 378);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(196, 20);
            this.Status.TabIndex = 14;
            // 
            // IF
            // 
            this.IF.Enabled = false;
            this.IF.Location = new System.Drawing.Point(10, 352);
            this.IF.Name = "IF";
            this.IF.Size = new System.Drawing.Size(196, 20);
            this.IF.TabIndex = 13;
            // 
            // shutter
            // 
            this.shutter.AutoSize = true;
            this.shutter.Location = new System.Drawing.Point(10, 328);
            this.shutter.Name = "shutter";
            this.shutter.Size = new System.Drawing.Size(60, 17);
            this.shutter.TabIndex = 11;
            this.shutter.Text = "Shutter";
            this.shutter.UseVisualStyleBackColor = true;
            this.shutter.CheckedChanged += new System.EventHandler(this.shutter_CheckedChanged);
            // 
            // ext_flash
            // 
            this.ext_flash.AutoSize = true;
            this.ext_flash.Location = new System.Drawing.Point(10, 304);
            this.ext_flash.Name = "ext_flash";
            this.ext_flash.Size = new System.Drawing.Size(114, 17);
            this.ext_flash.TabIndex = 9;
            this.ext_flash.Text = "External Flashlamp";
            this.ext_flash.UseVisualStyleBackColor = true;
            this.ext_flash.CheckedChanged += new System.EventHandler(this.ext_flash_CheckedChanged);
            // 
            // ext_Q
            // 
            this.ext_Q.AutoSize = true;
            this.ext_Q.Location = new System.Drawing.Point(10, 281);
            this.ext_Q.Name = "ext_Q";
            this.ext_Q.Size = new System.Drawing.Size(110, 17);
            this.ext_Q.TabIndex = 7;
            this.ext_Q.Text = "External Q-Switch";
            this.ext_Q.UseVisualStyleBackColor = true;
            this.ext_Q.CheckedChanged += new System.EventHandler(this.ext_Q_CheckedChanged);
            // 
            // Ene
            // 
            this.Ene.Label = "ENE";
            this.Ene.Location = new System.Drawing.Point(6, 209);
            this.Ene.Name = "Ene";
            this.Ene.Size = new System.Drawing.Size(203, 27);
            this.Ene.TabIndex = 5;
            this.Ene.OnSetClick += new System.EventHandler(this.Ene_OnSetClick);
            // 
            // Delay
            // 
            this.Delay.Label = "Delay";
            this.Delay.Location = new System.Drawing.Point(6, 176);
            this.Delay.Name = "Delay";
            this.Delay.Size = new System.Drawing.Size(203, 27);
            this.Delay.TabIndex = 4;
            this.Delay.OnSetClick += new System.EventHandler(this.Delay_OnSetClick);
            // 
            // VIS
            // 
            this.VIS.Label = "VIS";
            this.VIS.Location = new System.Drawing.Point(6, 142);
            this.VIS.Name = "VIS";
            this.VIS.Size = new System.Drawing.Size(203, 27);
            this.VIS.TabIndex = 3;
            this.VIS.OnSetClick += new System.EventHandler(this.VIS_OnSetClick);
            // 
            // VOS
            // 
            this.VOS.Label = "VOS";
            this.VOS.Location = new System.Drawing.Point(6, 108);
            this.VOS.Name = "VOS";
            this.VOS.Size = new System.Drawing.Size(203, 27);
            this.VOS.TabIndex = 2;
            this.VOS.OnSetClick += new System.EventHandler(this.VOS_OnSetClick);
            // 
            // VMO
            // 
            this.VMO.Label = "VMO";
            this.VMO.Location = new System.Drawing.Point(6, 75);
            this.VMO.Name = "VMO";
            this.VMO.Size = new System.Drawing.Size(203, 27);
            this.VMO.TabIndex = 1;
            this.VMO.OnSetClick += new System.EventHandler(this.VMO_OnSetClick);
            // 
            // Shutdown
            // 
            this.Shutdown.Location = new System.Drawing.Point(10, 46);
            this.Shutdown.Name = "Shutdown";
            this.Shutdown.Size = new System.Drawing.Size(196, 23);
            this.Shutdown.TabIndex = 0;
            this.Shutdown.Text = "Shutdown";
            this.Shutdown.UseVisualStyleBackColor = true;
            this.Shutdown.Click += new System.EventHandler(this.Shutdown_Click);
            // 
            // Conn_status
            // 
            this.Conn_status.BackColor = System.Drawing.Color.Salmon;
            this.Conn_status.Enabled = false;
            this.Conn_status.Location = new System.Drawing.Point(106, 20);
            this.Conn_status.Name = "Conn_status";
            this.Conn_status.Size = new System.Drawing.Size(100, 20);
            this.Conn_status.TabIndex = 1;
            this.Conn_status.Text = "DISCONNECTED";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Connection status";
            // 
            // YAG_Control
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.YAG_Control_Group);
            this.Name = "YAG_Control";
            this.Size = new System.Drawing.Size(222, 411);
            this.YAG_Control_Group.ResumeLayout(false);
            this.YAG_Control_Group.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox YAG_Control_Group;
        private System.Windows.Forms.TextBox Conn_status;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Shutdown;
        private ParamSet VMO;
        private System.Windows.Forms.CheckBox qswitch;
        private System.Windows.Forms.TextBox Status;
        private System.Windows.Forms.TextBox IF;
        private System.Windows.Forms.CheckBox shutter;
        private System.Windows.Forms.CheckBox ext_flash;
        private System.Windows.Forms.CheckBox ext_Q;
        private ParamSet Ene;
        private ParamSet Delay;
        private ParamSet VIS;
        private ParamSet VOS;
        private System.Windows.Forms.TextBox Temp;
        private System.Windows.Forms.Label label2;
        private ParamSet Freq;
        private System.Windows.Forms.CheckBox flashlamp;
    }
}
