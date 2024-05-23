using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using wlmData;

namespace WavemeterLock
{
    partial class LockForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LockForm));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lockTab = new System.Windows.Forms.TabControl();
            this.masterBttn = new System.Windows.Forms.Button();
            this.wmlLED = new NationalInstruments.UI.WindowsForms.Led();
            this.groupBoxLockRate = new System.Windows.Forms.GroupBox();
            this.updateRateTextBox = new System.Windows.Forms.TextBox();
            this.led1 = new NationalInstruments.UI.WindowsForms.Led();
            this.led2 = new NationalInstruments.UI.WindowsForms.Led();
            this.led3 = new NationalInstruments.UI.WindowsForms.Led();
            this.led4 = new NationalInstruments.UI.WindowsForms.Led();
            this.led5 = new NationalInstruments.UI.WindowsForms.Led();
            this.led6 = new NationalInstruments.UI.WindowsForms.Led();
            this.led7 = new NationalInstruments.UI.WindowsForms.Led();
            this.led8 = new NationalInstruments.UI.WindowsForms.Led();
            this.button_lock_all = new System.Windows.Forms.Button();
            this.saveSetPointsButton = new System.Windows.Forms.Button();
            this.loadSetPointsButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.wmlLED)).BeginInit();
            this.groupBoxLockRate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led8)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lockTab
            // 
            this.lockTab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lockTab.Location = new System.Drawing.Point(-3, 70);
            this.lockTab.Name = "lockTab";
            this.lockTab.SelectedIndex = 0;
            this.lockTab.Size = new System.Drawing.Size(723, 497);
            this.lockTab.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.lockTab.TabIndex = 2;
            this.lockTab.SelectedIndexChanged += new System.EventHandler(this.lockTab_SelectedIndexChanged);
            // 
            // masterBttn
            // 
            this.masterBttn.Location = new System.Drawing.Point(31, 21);
            this.masterBttn.Name = "masterBttn";
            this.masterBttn.Size = new System.Drawing.Size(75, 23);
            this.masterBttn.TabIndex = 3;
            this.masterBttn.Text = "masterBttn";
            this.masterBttn.UseVisualStyleBackColor = true;
            this.masterBttn.Click += new System.EventHandler(this.masterBttn_Click);
            // 
            // wmlLED
            // 
            this.wmlLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.wmlLED.Location = new System.Drawing.Point(122, 21);
            this.wmlLED.Name = "wmlLED";
            this.wmlLED.OffColor = System.Drawing.Color.Crimson;
            this.wmlLED.Size = new System.Drawing.Size(23, 23);
            this.wmlLED.TabIndex = 5;
            // 
            // groupBoxLockRate
            // 
            this.groupBoxLockRate.Controls.Add(this.updateRateTextBox);
            this.groupBoxLockRate.Location = new System.Drawing.Point(582, 12);
            this.groupBoxLockRate.Name = "groupBoxLockRate";
            this.groupBoxLockRate.Size = new System.Drawing.Size(138, 42);
            this.groupBoxLockRate.TabIndex = 6;
            this.groupBoxLockRate.TabStop = false;
            this.groupBoxLockRate.Text = "Lock Update Rate (Hz)";
            this.groupBoxLockRate.Enter += new System.EventHandler(this.groupBoxLockRate_Enter);
            // 
            // updateRateTextBox
            // 
            this.updateRateTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.updateRateTextBox.Location = new System.Drawing.Point(28, 19);
            this.updateRateTextBox.Name = "updateRateTextBox";
            this.updateRateTextBox.ReadOnly = true;
            this.updateRateTextBox.Size = new System.Drawing.Size(75, 13);
            this.updateRateTextBox.TabIndex = 0;
            this.updateRateTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // led1
            // 
            this.led1.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led1.Location = new System.Drawing.Point(237, 21);
            this.led1.Name = "led1";
            this.led1.OffColor = System.Drawing.Color.Crimson;
            this.led1.Size = new System.Drawing.Size(23, 23);
            this.led1.TabIndex = 7;
            this.led1.Visible = false;
            // 
            // led2
            // 
            this.led2.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led2.Location = new System.Drawing.Point(266, 21);
            this.led2.Name = "led2";
            this.led2.OffColor = System.Drawing.Color.Crimson;
            this.led2.Size = new System.Drawing.Size(23, 23);
            this.led2.TabIndex = 8;
            this.led2.Visible = false;
            // 
            // led3
            // 
            this.led3.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led3.Location = new System.Drawing.Point(295, 21);
            this.led3.Name = "led3";
            this.led3.OffColor = System.Drawing.Color.Crimson;
            this.led3.Size = new System.Drawing.Size(23, 23);
            this.led3.TabIndex = 9;
            this.led3.Visible = false;
            // 
            // led4
            // 
            this.led4.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led4.Location = new System.Drawing.Point(324, 21);
            this.led4.Name = "led4";
            this.led4.OffColor = System.Drawing.Color.Crimson;
            this.led4.Size = new System.Drawing.Size(23, 23);
            this.led4.TabIndex = 10;
            this.led4.Visible = false;
            // 
            // led5
            // 
            this.led5.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led5.Location = new System.Drawing.Point(353, 21);
            this.led5.Name = "led5";
            this.led5.OffColor = System.Drawing.Color.Crimson;
            this.led5.Size = new System.Drawing.Size(23, 23);
            this.led5.TabIndex = 11;
            this.led5.Visible = false;
            // 
            // led6
            // 
            this.led6.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led6.Location = new System.Drawing.Point(382, 21);
            this.led6.Name = "led6";
            this.led6.OffColor = System.Drawing.Color.Crimson;
            this.led6.Size = new System.Drawing.Size(23, 23);
            this.led6.TabIndex = 12;
            this.led6.Visible = false;
            // 
            // led7
            // 
            this.led7.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led7.Location = new System.Drawing.Point(411, 21);
            this.led7.Name = "led7";
            this.led7.OffColor = System.Drawing.Color.Crimson;
            this.led7.Size = new System.Drawing.Size(23, 23);
            this.led7.TabIndex = 13;
            this.led7.Visible = false;
            // 
            // led8
            // 
            this.led8.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led8.Location = new System.Drawing.Point(440, 21);
            this.led8.Name = "led8";
            this.led8.OffColor = System.Drawing.Color.Crimson;
            this.led8.Size = new System.Drawing.Size(23, 23);
            this.led8.TabIndex = 14;
            this.led8.Visible = false;
            // 
            // button_lock_all
            // 
            this.button_lock_all.Enabled = false;
            this.button_lock_all.Location = new System.Drawing.Point(156, 21);
            this.button_lock_all.Name = "button_lock_all";
            this.button_lock_all.Size = new System.Drawing.Size(75, 23);
            this.button_lock_all.TabIndex = 15;
            this.button_lock_all.Text = "Lock all";
            this.button_lock_all.UseVisualStyleBackColor = true;
            this.button_lock_all.Click += new System.EventHandler(this.lock_all);
            // 
            // saveSetPointsButton
            // 
            this.saveSetPointsButton.Location = new System.Drawing.Point(469, 8);
            this.saveSetPointsButton.Name = "saveSetPointsButton";
            this.saveSetPointsButton.Size = new System.Drawing.Size(95, 25);
            this.saveSetPointsButton.TabIndex = 16;
            this.saveSetPointsButton.Text = "Save SetPoints";
            this.saveSetPointsButton.UseVisualStyleBackColor = true;
            this.saveSetPointsButton.Click += new System.EventHandler(this.saveSetPointsButton_Click);
            // 
            // loadSetPointsButton
            // 
            this.loadSetPointsButton.Location = new System.Drawing.Point(469, 39);
            this.loadSetPointsButton.Name = "loadSetPointsButton";
            this.loadSetPointsButton.Size = new System.Drawing.Size(95, 25);
            this.loadSetPointsButton.TabIndex = 17;
            this.loadSetPointsButton.Text = "Load SetPoints";
            this.loadSetPointsButton.UseVisualStyleBackColor = true;
            this.loadSetPointsButton.Click += new System.EventHandler(this.loadSetPointsButton_Click);
            // 
            // LockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 568);
            this.Controls.Add(this.loadSetPointsButton);
            this.Controls.Add(this.saveSetPointsButton);
            this.Controls.Add(this.button_lock_all);
            this.Controls.Add(this.led8);
            this.Controls.Add(this.led7);
            this.Controls.Add(this.led6);
            this.Controls.Add(this.led5);
            this.Controls.Add(this.led4);
            this.Controls.Add(this.led3);
            this.Controls.Add(this.led2);
            this.Controls.Add(this.led1);
            this.Controls.Add(this.groupBoxLockRate);
            this.Controls.Add(this.wmlLED);
            this.Controls.Add(this.masterBttn);
            this.Controls.Add(this.lockTab);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LockForm";
            this.Text = "Wavemeter Lock";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LockForm_Closing);
            this.Load += new System.EventHandler(this.LockForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wmlLED)).EndInit();
            this.groupBoxLockRate.ResumeLayout(false);
            this.groupBoxLockRate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led8)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabControl lockTab;
        private System.Windows.Forms.Button masterBttn;
        private NationalInstruments.UI.WindowsForms.Led wmlLED;
        private System.Windows.Forms.GroupBox groupBoxLockRate;
        private System.Windows.Forms.TextBox updateRateTextBox;
        private NationalInstruments.UI.WindowsForms.Led led1;
        private NationalInstruments.UI.WindowsForms.Led led2;
        private NationalInstruments.UI.WindowsForms.Led led3;
        private NationalInstruments.UI.WindowsForms.Led led4;
        private NationalInstruments.UI.WindowsForms.Led led5;
        private NationalInstruments.UI.WindowsForms.Led led6;
        private NationalInstruments.UI.WindowsForms.Led led7;
        private NationalInstruments.UI.WindowsForms.Led led8;
        private Button button_lock_all;
        private Button saveSetPointsButton;
        private Button loadSetPointsButton;
    }
}

