using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WavemeterLock
{
    partial class LockControlPanel : UserControl
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
            this.stepDown = new System.Windows.Forms.Button();
            this.stepUp = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.stepSize = new System.Windows.Forms.TextBox();
            this.displayFreq = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.resetBtn = new System.Windows.Forms.Button();
            this.IGainSet = new System.Windows.Forms.Button();
            this.IGain = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.frequencyError = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.VOut = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.PGainSet = new System.Windows.Forms.Button();
            this.PGain = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lockMsg = new System.Windows.Forms.Label();
            this.LockChannelNumber = new System.Windows.Forms.TextBox();
            this.lockButton = new System.Windows.Forms.Button();
            this.SetPoint = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.displayWL = new System.Windows.Forms.Label();
            this.showButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.errorMsg = new System.Windows.Forms.Label();
            this.lockLED = new NationalInstruments.UI.WindowsForms.Led();
            ((System.ComponentModel.ISupportInitialize)(this.lockLED)).BeginInit();
            this.SuspendLayout();
            // 
            // stepDown
            // 
            this.stepDown.Location = new System.Drawing.Point(244, 149);
            this.stepDown.Name = "stepDown";
            this.stepDown.Size = new System.Drawing.Size(30, 24);
            this.stepDown.TabIndex = 65;
            this.stepDown.Text = "-";
            this.stepDown.UseVisualStyleBackColor = true;
            this.stepDown.Click += new System.EventHandler(this.stepDownBtn_Click);
            // 
            // stepUp
            // 
            this.stepUp.Location = new System.Drawing.Point(280, 149);
            this.stepUp.Name = "stepUp";
            this.stepUp.Size = new System.Drawing.Size(30, 24);
            this.stepUp.TabIndex = 64;
            this.stepUp.Text = "+\r\n";
            this.stepUp.UseVisualStyleBackColor = true;
            this.stepUp.Click += new System.EventHandler(this.stepUpBtn_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(46, 154);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(127, 13);
            this.label11.TabIndex = 63;
            this.label11.Text = "Set point setp size (MHz):";
            // 
            // stepSize
            // 
            this.stepSize.Location = new System.Drawing.Point(179, 151);
            this.stepSize.Name = "stepSize";
            this.stepSize.Size = new System.Drawing.Size(59, 20);
            this.stepSize.TabIndex = 62;
            this.stepSize.Text = "10";
            // 
            // displayFreq
            // 
            this.displayFreq.AutoSize = true;
            this.displayFreq.Location = new System.Drawing.Point(176, 69);
            this.displayFreq.Name = "displayFreq";
            this.displayFreq.Size = new System.Drawing.Size(32, 13);
            this.displayFreq.TabIndex = 61;
            this.displayFreq.Text = "xxxxx";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(310, 69);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 13);
            this.label10.TabIndex = 60;
            this.label10.Text = "Wavelength (nm)";
            // 
            // resetBtn
            // 
            this.resetBtn.Location = new System.Drawing.Point(332, 149);
            this.resetBtn.Name = "resetBtn";
            this.resetBtn.Size = new System.Drawing.Size(95, 23);
            this.resetBtn.TabIndex = 59;
            this.resetBtn.Text = "Reset Output";
            this.resetBtn.UseVisualStyleBackColor = true;
            this.resetBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // IGainSet
            // 
            this.IGainSet.Location = new System.Drawing.Point(199, 233);
            this.IGainSet.Name = "IGainSet";
            this.IGainSet.Size = new System.Drawing.Size(75, 23);
            this.IGainSet.TabIndex = 58;
            this.IGainSet.Text = "Set";
            this.IGainSet.UseVisualStyleBackColor = true;
            this.IGainSet.Click += new System.EventHandler(this.IGainSet_Click);
            // 
            // IGain
            // 
            this.IGain.Location = new System.Drawing.Point(107, 233);
            this.IGain.Name = "IGain";
            this.IGain.Size = new System.Drawing.Size(59, 20);
            this.IGain.TabIndex = 57;
            this.IGain.Text = "1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(53, 238);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 56;
            this.label9.Text = "I Gain:";
            // 
            // frequencyError
            // 
            this.frequencyError.AutoSize = true;
            this.frequencyError.Location = new System.Drawing.Point(414, 238);
            this.frequencyError.Name = "frequencyError";
            this.frequencyError.Size = new System.Drawing.Size(13, 13);
            this.frequencyError.TabIndex = 54;
            this.frequencyError.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(292, 238);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(116, 13);
            this.label8.TabIndex = 53;
            this.label8.Text = "Frequency Error (MHz):";
            // 
            // VOut
            // 
            this.VOut.AutoSize = true;
            this.VOut.Location = new System.Drawing.Point(393, 207);
            this.VOut.Name = "VOut";
            this.VOut.Size = new System.Drawing.Size(32, 13);
            this.VOut.TabIndex = 48;
            this.VOut.Text = "xxxxx";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(292, 207);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 47;
            this.label5.Text = "Voltage Out (V):";
            // 
            // PGainSet
            // 
            this.PGainSet.Location = new System.Drawing.Point(199, 204);
            this.PGainSet.Name = "PGainSet";
            this.PGainSet.Size = new System.Drawing.Size(75, 23);
            this.PGainSet.TabIndex = 46;
            this.PGainSet.Text = "Set";
            this.PGainSet.UseVisualStyleBackColor = true;
            this.PGainSet.Click += new System.EventHandler(this.PGainSet_Click);
            // 
            // PGain
            // 
            this.PGain.Location = new System.Drawing.Point(107, 204);
            this.PGain.Name = "PGain";
            this.PGain.Size = new System.Drawing.Size(59, 20);
            this.PGain.TabIndex = 45;
            this.PGain.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(53, 209);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 44;
            this.label2.Text = "P Gain:";
            // 
            // lockMsg
            // 
            this.lockMsg.AutoSize = true;
            this.lockMsg.Location = new System.Drawing.Point(397, 114);
            this.lockMsg.Name = "lockMsg";
            this.lockMsg.Size = new System.Drawing.Size(48, 13);
            this.lockMsg.TabIndex = 43;
            this.lockMsg.Text = "Lock Off";
            // 
            // LockChannelNumber
            // 
            this.LockChannelNumber.Location = new System.Drawing.Point(162, 21);
            this.LockChannelNumber.Name = "LockChannelNumber";
            this.LockChannelNumber.Size = new System.Drawing.Size(59, 20);
            this.LockChannelNumber.TabIndex = 42;
            // 
            // lockButton
            // 
            this.lockButton.Location = new System.Drawing.Point(295, 109);
            this.lockButton.Name = "lockButton";
            this.lockButton.Size = new System.Drawing.Size(55, 23);
            this.lockButton.TabIndex = 41;
            this.lockButton.Text = "Lock";
            this.lockButton.UseVisualStyleBackColor = true;
            this.lockButton.Click += new System.EventHandler(this.lockButton_Click);
            // 
            // SetPoint
            // 
            this.SetPoint.Location = new System.Drawing.Point(163, 111);
            this.SetPoint.Name = "SetPoint";
            this.SetPoint.Size = new System.Drawing.Size(94, 20);
            this.SetPoint.TabIndex = 40;
            this.SetPoint.Text = "390";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(44, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 39;
            this.label4.Text = "Set Frequency (THz):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(50, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 38;
            this.label3.Text = "Frequency (THz):";
            // 
            // displayWL
            // 
            this.displayWL.AutoSize = true;
            this.displayWL.Location = new System.Drawing.Point(418, 69);
            this.displayWL.Name = "displayWL";
            this.displayWL.Size = new System.Drawing.Size(32, 13);
            this.displayWL.TabIndex = 37;
            this.displayWL.Text = "xxxxx";
            // 
            // showButton
            // 
            this.showButton.Location = new System.Drawing.Point(254, 19);
            this.showButton.Name = "showButton";
            this.showButton.Size = new System.Drawing.Size(75, 23);
            this.showButton.TabIndex = 36;
            this.showButton.Text = "Show";
            this.showButton.UseVisualStyleBackColor = true;
            this.showButton.Click += new System.EventHandler(this.showButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 35;
            this.label1.Text = "Lock Channel Number";
            // 
            // errorMsg
            // 
            this.errorMsg.AutoSize = true;
            this.errorMsg.ForeColor = System.Drawing.Color.Red;
            this.errorMsg.Location = new System.Drawing.Point(350, 24);
            this.errorMsg.Name = "errorMsg";
            this.errorMsg.Size = new System.Drawing.Size(0, 13);
            this.errorMsg.TabIndex = 66;
            // 
            // lockLED
            // 
            this.lockLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.lockLED.Location = new System.Drawing.Point(373, 109);
            this.lockLED.Name = "lockLED";
            this.lockLED.OffColor = System.Drawing.Color.Crimson;
            this.lockLED.Size = new System.Drawing.Size(23, 23);
            this.lockLED.TabIndex = 67;
            // 
            // LockControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lockLED);
            this.Controls.Add(this.errorMsg);
            this.Controls.Add(this.stepDown);
            this.Controls.Add(this.stepUp);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.stepSize);
            this.Controls.Add(this.displayFreq);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.resetBtn);
            this.Controls.Add(this.IGainSet);
            this.Controls.Add(this.IGain);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.frequencyError);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.VOut);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.PGainSet);
            this.Controls.Add(this.PGain);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lockMsg);
            this.Controls.Add(this.LockChannelNumber);
            this.Controls.Add(this.lockButton);
            this.Controls.Add(this.SetPoint);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.displayWL);
            this.Controls.Add(this.showButton);
            this.Controls.Add(this.label1);
            this.Name = "LockControlPanel";
            this.Size = new System.Drawing.Size(524, 290);
            this.Load += new System.EventHandler(this.LockControlPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.lockLED)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Button stepDown;
        private Button stepUp;
        private Label label11;
        private TextBox stepSize;
        private Label displayFreq;
        private Label label10;
        private Button resetBtn;
        private Button IGainSet;
        private TextBox IGain;
        private Label label9;
        private Label frequencyError;
        private Label label8;
        private Label VOut;
        private Label label5;
        private Button PGainSet;
        private TextBox PGain;
        private Label label2;
        private Label lockMsg;
        private TextBox LockChannelNumber;
        private Button lockButton;
        private TextBox SetPoint;
        private Label label4;
        private Label label3;
        private Label displayWL;
        private Button showButton;
        private Label label1;
        private Label errorMsg;
        private NationalInstruments.UI.WindowsForms.Led lockLED;
    }
}
