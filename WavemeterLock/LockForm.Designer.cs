
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
            this.label1 = new System.Windows.Forms.Label();
            this.showButton = new System.Windows.Forms.Button();
            this.displayWL = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SetPoint = new System.Windows.Forms.TextBox();
            this.lockButton = new System.Windows.Forms.Button();
            this.LockChannelNumber = new System.Windows.Forms.TextBox();
            this.lockMsg = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PGain = new System.Windows.Forms.TextBox();
            this.PGainSet = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.VOut = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.loopCount = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.laserState = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.frequencyError = new System.Windows.Forms.Label();
            this.channelNum = new System.Windows.Forms.Label();
            this.IGainSet = new System.Windows.Forms.Button();
            this.IGain = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.resetBtn = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.displayFreq = new System.Windows.Forms.Label();
            this.stepSize = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.stepUp = new System.Windows.Forms.Button();
            this.stepDown = new System.Windows.Forms.Button();
            this.TestLable = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Lock Channel Number";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // showButton
            // 
            this.showButton.Location = new System.Drawing.Point(263, 69);
            this.showButton.Name = "showButton";
            this.showButton.Size = new System.Drawing.Size(75, 23);
            this.showButton.TabIndex = 2;
            this.showButton.Text = "Show";
            this.showButton.UseVisualStyleBackColor = true;
            this.showButton.Click += new System.EventHandler(this.showButton_Click);
            // 
            // displayWL
            // 
            this.displayWL.AutoSize = true;
            this.displayWL.Location = new System.Drawing.Point(420, 133);
            this.displayWL.Name = "displayWL";
            this.displayWL.Size = new System.Drawing.Size(32, 13);
            this.displayWL.TabIndex = 3;
            this.displayWL.Text = "xxxxx";
            this.displayWL.Click += new System.EventHandler(this.displayWL_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(52, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Frequency (THz):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(52, 188);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Set Frequency (THz):";
            // 
            // SetPoint
            // 
            this.SetPoint.Location = new System.Drawing.Point(171, 185);
            this.SetPoint.Name = "SetPoint";
            this.SetPoint.Size = new System.Drawing.Size(59, 20);
            this.SetPoint.TabIndex = 7;
            this.SetPoint.Text = "390";
            // 
            // lockButton
            // 
            this.lockButton.Location = new System.Drawing.Point(263, 185);
            this.lockButton.Name = "lockButton";
            this.lockButton.Size = new System.Drawing.Size(75, 23);
            this.lockButton.TabIndex = 8;
            this.lockButton.Text = "Lock";
            this.lockButton.UseVisualStyleBackColor = true;
            this.lockButton.Click += new System.EventHandler(this.lockButton_Click);
            // 
            // LockChannelNumber
            // 
            this.LockChannelNumber.Location = new System.Drawing.Point(171, 71);
            this.LockChannelNumber.Name = "LockChannelNumber";
            this.LockChannelNumber.Size = new System.Drawing.Size(59, 20);
            this.LockChannelNumber.TabIndex = 9;
            // 
            // lockMsg
            // 
            this.lockMsg.AutoSize = true;
            this.lockMsg.Location = new System.Drawing.Point(368, 190);
            this.lockMsg.Name = "lockMsg";
            this.lockMsg.Size = new System.Drawing.Size(48, 13);
            this.lockMsg.TabIndex = 10;
            this.lockMsg.Text = "Lock Off";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 244);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "P Gain:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // PGain
            // 
            this.PGain.Location = new System.Drawing.Point(172, 239);
            this.PGain.Name = "PGain";
            this.PGain.Size = new System.Drawing.Size(59, 20);
            this.PGain.TabIndex = 12;
            this.PGain.Text = "1";
            // 
            // PGainSet
            // 
            this.PGainSet.Location = new System.Drawing.Point(264, 239);
            this.PGainSet.Name = "PGainSet";
            this.PGainSet.Size = new System.Drawing.Size(75, 23);
            this.PGainSet.TabIndex = 13;
            this.PGainSet.Text = "Set";
            this.PGainSet.UseVisualStyleBackColor = true;
            this.PGainSet.Click += new System.EventHandler(this.PGainSet_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(78, 277);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Voltage Out (V):";
            // 
            // VOut
            // 
            this.VOut.AutoSize = true;
            this.VOut.Location = new System.Drawing.Point(179, 277);
            this.VOut.Name = "VOut";
            this.VOut.Size = new System.Drawing.Size(32, 13);
            this.VOut.TabIndex = 15;
            this.VOut.Text = "xxxxx";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(529, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Loop Count:";
            // 
            // loopCount
            // 
            this.loopCount.AutoSize = true;
            this.loopCount.Location = new System.Drawing.Point(600, 90);
            this.loopCount.Name = "loopCount";
            this.loopCount.Size = new System.Drawing.Size(13, 13);
            this.loopCount.TabIndex = 18;
            this.loopCount.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(529, 116);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(64, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Laser State:";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // laserState
            // 
            this.laserState.AutoSize = true;
            this.laserState.Location = new System.Drawing.Point(600, 116);
            this.laserState.Name = "laserState";
            this.laserState.Size = new System.Drawing.Size(13, 13);
            this.laserState.TabIndex = 20;
            this.laserState.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(78, 308);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(116, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Frequency Error (MHz):";
            // 
            // frequencyError
            // 
            this.frequencyError.AutoSize = true;
            this.frequencyError.Location = new System.Drawing.Point(200, 308);
            this.frequencyError.Name = "frequencyError";
            this.frequencyError.Size = new System.Drawing.Size(13, 13);
            this.frequencyError.TabIndex = 22;
            this.frequencyError.Text = "0";
            // 
            // channelNum
            // 
            this.channelNum.AutoSize = true;
            this.channelNum.Location = new System.Drawing.Point(368, 74);
            this.channelNum.Name = "channelNum";
            this.channelNum.Size = new System.Drawing.Size(32, 13);
            this.channelNum.TabIndex = 23;
            this.channelNum.Text = "xxxxx";
            // 
            // IGainSet
            // 
            this.IGainSet.Location = new System.Drawing.Point(519, 241);
            this.IGainSet.Name = "IGainSet";
            this.IGainSet.Size = new System.Drawing.Size(75, 23);
            this.IGainSet.TabIndex = 26;
            this.IGainSet.Text = "Set";
            this.IGainSet.UseVisualStyleBackColor = true;
            this.IGainSet.Click += new System.EventHandler(this.IGainSet_Click);
            // 
            // IGain
            // 
            this.IGain.Location = new System.Drawing.Point(427, 241);
            this.IGain.Name = "IGain";
            this.IGain.Size = new System.Drawing.Size(59, 20);
            this.IGain.TabIndex = 25;
            this.IGain.Text = "1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(373, 246);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "I Gain:";
            // 
            // resetBtn
            // 
            this.resetBtn.Location = new System.Drawing.Point(322, 277);
            this.resetBtn.Name = "resetBtn";
            this.resetBtn.Size = new System.Drawing.Size(95, 23);
            this.resetBtn.TabIndex = 27;
            this.resetBtn.Text = "Reset Output";
            this.resetBtn.UseVisualStyleBackColor = true;
            this.resetBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(312, 133);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 13);
            this.label10.TabIndex = 28;
            this.label10.Text = "Wavelength (nm)";
            // 
            // displayFreq
            // 
            this.displayFreq.AutoSize = true;
            this.displayFreq.Location = new System.Drawing.Point(178, 133);
            this.displayFreq.Name = "displayFreq";
            this.displayFreq.Size = new System.Drawing.Size(32, 13);
            this.displayFreq.TabIndex = 29;
            this.displayFreq.Text = "xxxxx";
            // 
            // stepSize
            // 
            this.stepSize.Location = new System.Drawing.Point(570, 187);
            this.stepSize.Name = "stepSize";
            this.stepSize.Size = new System.Drawing.Size(59, 20);
            this.stepSize.TabIndex = 30;
            this.stepSize.Text = "10";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(437, 190);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(127, 13);
            this.label11.TabIndex = 31;
            this.label11.Text = "Set point setp size (MHz):";
            this.label11.Click += new System.EventHandler(this.label11_Click);
            // 
            // stepUp
            // 
            this.stepUp.Location = new System.Drawing.Point(647, 160);
            this.stepUp.Name = "stepUp";
            this.stepUp.Size = new System.Drawing.Size(75, 23);
            this.stepUp.TabIndex = 32;
            this.stepUp.Text = "Step Up";
            this.stepUp.UseVisualStyleBackColor = true;
            this.stepUp.Click += new System.EventHandler(this.stepUpBtn_Click);
            // 
            // stepDown
            // 
            this.stepDown.Location = new System.Drawing.Point(647, 210);
            this.stepDown.Name = "stepDown";
            this.stepDown.Size = new System.Drawing.Size(75, 23);
            this.stepDown.TabIndex = 33;
            this.stepDown.Text = "Step Down";
            this.stepDown.UseVisualStyleBackColor = true;
            this.stepDown.Click += new System.EventHandler(this.stepDownBtn_Click);
            // 
            // TestLable
            // 
            this.TestLable.AutoSize = true;
            this.TestLable.Location = new System.Drawing.Point(109, 386);
            this.TestLable.Name = "TestLable";
            this.TestLable.Size = new System.Drawing.Size(32, 13);
            this.TestLable.TabIndex = 34;
            this.TestLable.Text = "xxxxx";
            this.TestLable.Click += new System.EventHandler(this.TestLable_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TestLable);
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
            this.Controls.Add(this.channelNum);
            this.Controls.Add(this.frequencyError);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.laserState);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.loopCount);
            this.Controls.Add(this.label6);
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button showButton;
        private System.Windows.Forms.Label displayWL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox SetPoint;
        private System.Windows.Forms.Button lockButton;
        private System.Windows.Forms.TextBox LockChannelNumber;
        private System.Windows.Forms.Label lockMsg;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox PGain;
        private System.Windows.Forms.Button PGainSet;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label VOut;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label loopCount;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label laserState;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label frequencyError;
        private System.Windows.Forms.Label channelNum;
        private System.Windows.Forms.Button IGainSet;
        private System.Windows.Forms.TextBox IGain;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button resetBtn;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label displayFreq;
        private System.Windows.Forms.TextBox stepSize;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button stepUp;
        private System.Windows.Forms.Button stepDown;
        private System.Windows.Forms.Label TestLable;
    }
}

