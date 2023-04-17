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
            this.lockButton = new System.Windows.Forms.Button();
            this.SetPoint = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.displayWL = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lockLED = new NationalInstruments.UI.WindowsForms.Led();
            this.lockChannelNum = new System.Windows.Forms.Label();
            this.errorScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.errorPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.scaleDown = new System.Windows.Forms.Button();
            this.scaleUp = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.controlPanel = new System.Windows.Forms.GroupBox();
            this.setAsReading = new System.Windows.Forms.Button();
            this.offset = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.offsetSet = new System.Windows.Forms.Button();
            this.resetGraph = new System.Windows.Forms.Button();
            this.groupBoxErrorPlot = new System.Windows.Forms.GroupBox();
            this.groupBoxLaserInfo = new System.Windows.Forms.GroupBox();
            this.labelOutOfRange = new System.Windows.Forms.Label();
            this.TestLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.lockLED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorScatterGraph)).BeginInit();
            this.controlPanel.SuspendLayout();
            this.groupBoxErrorPlot.SuspendLayout();
            this.groupBoxLaserInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // stepDown
            // 
            this.stepDown.Location = new System.Drawing.Point(457, 68);
            this.stepDown.Name = "stepDown";
            this.stepDown.Size = new System.Drawing.Size(30, 24);
            this.stepDown.TabIndex = 65;
            this.stepDown.Text = "-";
            this.stepDown.UseVisualStyleBackColor = true;
            this.stepDown.Click += new System.EventHandler(this.stepDownBtn_Click);
            // 
            // stepUp
            // 
            this.stepUp.Location = new System.Drawing.Point(493, 68);
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
            this.label11.Location = new System.Drawing.Point(263, 73);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(127, 13);
            this.label11.TabIndex = 63;
            this.label11.Text = "Set point setp size (MHz):";
            // 
            // stepSize
            // 
            this.stepSize.Location = new System.Drawing.Point(392, 70);
            this.stepSize.Name = "stepSize";
            this.stepSize.Size = new System.Drawing.Size(59, 20);
            this.stepSize.TabIndex = 62;
            this.stepSize.Text = "10";
            // 
            // displayFreq
            // 
            this.displayFreq.AutoSize = true;
            this.displayFreq.Location = new System.Drawing.Point(110, 64);
            this.displayFreq.Name = "displayFreq";
            this.displayFreq.Size = new System.Drawing.Size(32, 13);
            this.displayFreq.TabIndex = 61;
            this.displayFreq.Text = "xxxxx";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(26, 89);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 13);
            this.label10.TabIndex = 60;
            this.label10.Text = "Wavelength";
            // 
            // resetBtn
            // 
            this.resetBtn.Location = new System.Drawing.Point(254, 156);
            this.resetBtn.Name = "resetBtn";
            this.resetBtn.Size = new System.Drawing.Size(95, 23);
            this.resetBtn.TabIndex = 59;
            this.resetBtn.Text = "Reset Output";
            this.resetBtn.UseVisualStyleBackColor = true;
            this.resetBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // IGainSet
            // 
            this.IGainSet.Location = new System.Drawing.Point(412, 136);
            this.IGainSet.Name = "IGainSet";
            this.IGainSet.Size = new System.Drawing.Size(75, 23);
            this.IGainSet.TabIndex = 58;
            this.IGainSet.Text = "Set";
            this.IGainSet.UseVisualStyleBackColor = true;
            this.IGainSet.Click += new System.EventHandler(this.IGainSet_Click);
            // 
            // IGain
            // 
            this.IGain.Location = new System.Drawing.Point(320, 136);
            this.IGain.Name = "IGain";
            this.IGain.Size = new System.Drawing.Size(59, 20);
            this.IGain.TabIndex = 57;
            this.IGain.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(266, 141);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 56;
            this.label9.Text = "I Gain:";
            // 
            // frequencyError
            // 
            this.frequencyError.AutoSize = true;
            this.frequencyError.Location = new System.Drawing.Point(148, 115);
            this.frequencyError.Name = "frequencyError";
            this.frequencyError.Size = new System.Drawing.Size(13, 13);
            this.frequencyError.TabIndex = 54;
            this.frequencyError.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 115);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(116, 13);
            this.label8.TabIndex = 53;
            this.label8.Text = "Frequency Error (MHz):";
            // 
            // VOut
            // 
            this.VOut.AutoSize = true;
            this.VOut.Location = new System.Drawing.Point(110, 147);
            this.VOut.Name = "VOut";
            this.VOut.Size = new System.Drawing.Size(32, 13);
            this.VOut.TabIndex = 48;
            this.VOut.Text = "xxxxx";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 47;
            this.label5.Text = "Voltage Out (V):";
            // 
            // PGainSet
            // 
            this.PGainSet.Location = new System.Drawing.Point(412, 107);
            this.PGainSet.Name = "PGainSet";
            this.PGainSet.Size = new System.Drawing.Size(75, 23);
            this.PGainSet.TabIndex = 46;
            this.PGainSet.Text = "Set";
            this.PGainSet.UseVisualStyleBackColor = true;
            this.PGainSet.Click += new System.EventHandler(this.PGainSet_Click);
            // 
            // PGain
            // 
            this.PGain.Location = new System.Drawing.Point(320, 107);
            this.PGain.Name = "PGain";
            this.PGain.Size = new System.Drawing.Size(59, 20);
            this.PGain.TabIndex = 45;
            this.PGain.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(266, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 44;
            this.label2.Text = "P Gain:";
            // 
            // lockMsg
            // 
            this.lockMsg.AutoSize = true;
            this.lockMsg.Location = new System.Drawing.Point(348, 24);
            this.lockMsg.Name = "lockMsg";
            this.lockMsg.Size = new System.Drawing.Size(48, 13);
            this.lockMsg.TabIndex = 43;
            this.lockMsg.Text = "Lock Off";
            // 
            // lockButton
            // 
            this.lockButton.Location = new System.Drawing.Point(508, 28);
            this.lockButton.Name = "lockButton";
            this.lockButton.Size = new System.Drawing.Size(55, 23);
            this.lockButton.TabIndex = 41;
            this.lockButton.Text = "Lock";
            this.lockButton.UseVisualStyleBackColor = true;
            this.lockButton.Click += new System.EventHandler(this.lockButton_Click);
            // 
            // SetPoint
            // 
            this.SetPoint.Location = new System.Drawing.Point(376, 30);
            this.SetPoint.Name = "SetPoint";
            this.SetPoint.Size = new System.Drawing.Size(94, 20);
            this.SetPoint.TabIndex = 40;
            this.SetPoint.Text = "390";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(263, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 13);
            this.label4.TabIndex = 39;
            this.label4.Text = "Set Frequency (THz):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 38;
            this.label3.Text = "Frequency:";
            // 
            // displayWL
            // 
            this.displayWL.AutoSize = true;
            this.displayWL.Location = new System.Drawing.Point(110, 89);
            this.displayWL.Name = "displayWL";
            this.displayWL.Size = new System.Drawing.Size(32, 13);
            this.displayWL.TabIndex = 37;
            this.displayWL.Text = "xxxxx";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 35;
            this.label1.Text = "Lock Channel Number";
            // 
            // lockLED
            // 
            this.lockLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.lockLED.Location = new System.Drawing.Point(319, 18);
            this.lockLED.Name = "lockLED";
            this.lockLED.OffColor = System.Drawing.Color.Crimson;
            this.lockLED.Size = new System.Drawing.Size(23, 23);
            this.lockLED.TabIndex = 67;
            // 
            // lockChannelNum
            // 
            this.lockChannelNum.AutoSize = true;
            this.lockChannelNum.Location = new System.Drawing.Point(145, 29);
            this.lockChannelNum.Name = "lockChannelNum";
            this.lockChannelNum.Size = new System.Drawing.Size(32, 13);
            this.lockChannelNum.TabIndex = 68;
            this.lockChannelNum.Text = "xxxxx";
            // 
            // errorScatterGraph
            // 
            this.errorScatterGraph.Location = new System.Drawing.Point(12, 19);
            this.errorScatterGraph.Name = "errorScatterGraph";
            this.errorScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.errorPlot});
            this.errorScatterGraph.Size = new System.Drawing.Size(613, 249);
            this.errorScatterGraph.TabIndex = 69;
            this.errorScatterGraph.UseColorGenerator = true;
            this.errorScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.errorScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            this.errorScatterGraph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.errorScatterGraph_PlotDataChanged);
            // 
            // errorPlot
            // 
            this.errorPlot.HistoryCapacity = 2000;
            this.errorPlot.XAxis = this.xAxis1;
            this.errorPlot.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.AutoSpacing = false;
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis1.Range = new NationalInstruments.UI.Range(0D, 1000000D);
            // 
            // scaleDown
            // 
            this.scaleDown.Location = new System.Drawing.Point(254, 123);
            this.scaleDown.Name = "scaleDown";
            this.scaleDown.Size = new System.Drawing.Size(30, 24);
            this.scaleDown.TabIndex = 71;
            this.scaleDown.Text = "-";
            this.scaleDown.UseVisualStyleBackColor = true;
            this.scaleDown.Click += new System.EventHandler(this.scaleDown_click);
            // 
            // scaleUp
            // 
            this.scaleUp.Location = new System.Drawing.Point(290, 123);
            this.scaleUp.Name = "scaleUp";
            this.scaleUp.Size = new System.Drawing.Size(30, 24);
            this.scaleUp.TabIndex = 70;
            this.scaleUp.Text = "+\r\n";
            this.scaleUp.UseVisualStyleBackColor = true;
            this.scaleUp.Click += new System.EventHandler(this.scaleUp_click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(254, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 72;
            this.label6.Text = "X Axis Scale";
            // 
            // controlPanel
            // 
            this.controlPanel.Controls.Add(this.TestLabel);
            this.controlPanel.Controls.Add(this.setAsReading);
            this.controlPanel.Controls.Add(this.offset);
            this.controlPanel.Controls.Add(this.label7);
            this.controlPanel.Controls.Add(this.offsetSet);
            this.controlPanel.Controls.Add(this.resetGraph);
            this.controlPanel.Controls.Add(this.label6);
            this.controlPanel.Controls.Add(this.scaleDown);
            this.controlPanel.Controls.Add(this.resetBtn);
            this.controlPanel.Controls.Add(this.lockLED);
            this.controlPanel.Controls.Add(this.scaleUp);
            this.controlPanel.Controls.Add(this.lockMsg);
            this.controlPanel.Location = new System.Drawing.Point(251, 9);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(407, 193);
            this.controlPanel.TabIndex = 73;
            this.controlPanel.TabStop = false;
            this.controlPanel.Text = "Control Panel";
            this.controlPanel.Enter += new System.EventHandler(this.controlPanel_Enter);
            // 
            // setAsReading
            // 
            this.setAsReading.Location = new System.Drawing.Point(304, 60);
            this.setAsReading.Name = "setAsReading";
            this.setAsReading.Size = new System.Drawing.Size(85, 23);
            this.setAsReading.TabIndex = 75;
            this.setAsReading.Text = "Set as reading";
            this.setAsReading.UseVisualStyleBackColor = true;
            this.setAsReading.Click += new System.EventHandler(this.setAsReading_Click);
            // 
            // offset
            // 
            this.offset.Location = new System.Drawing.Point(69, 156);
            this.offset.Name = "offset";
            this.offset.Size = new System.Drawing.Size(59, 20);
            this.offset.TabIndex = 76;
            this.offset.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 161);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 13);
            this.label7.TabIndex = 75;
            this.label7.Text = "Offset (V):";
            // 
            // offsetSet
            // 
            this.offsetSet.Location = new System.Drawing.Point(161, 156);
            this.offsetSet.Name = "offsetSet";
            this.offsetSet.Size = new System.Drawing.Size(75, 23);
            this.offsetSet.TabIndex = 75;
            this.offsetSet.Text = "Set";
            this.offsetSet.UseVisualStyleBackColor = true;
            this.offsetSet.Click += new System.EventHandler(this.offsetSet_Click);
            // 
            // resetGraph
            // 
            this.resetGraph.Location = new System.Drawing.Point(339, 98);
            this.resetGraph.Name = "resetGraph";
            this.resetGraph.Size = new System.Drawing.Size(50, 47);
            this.resetGraph.TabIndex = 75;
            this.resetGraph.Text = "Reset Graph";
            this.resetGraph.UseVisualStyleBackColor = true;
            this.resetGraph.Click += new System.EventHandler(this.resetGraph_Click);
            // 
            // groupBoxErrorPlot
            // 
            this.groupBoxErrorPlot.Controls.Add(this.errorScatterGraph);
            this.groupBoxErrorPlot.Location = new System.Drawing.Point(27, 208);
            this.groupBoxErrorPlot.Name = "groupBoxErrorPlot";
            this.groupBoxErrorPlot.Size = new System.Drawing.Size(631, 279);
            this.groupBoxErrorPlot.TabIndex = 74;
            this.groupBoxErrorPlot.TabStop = false;
            this.groupBoxErrorPlot.Text = "Frequency Error (MHz)";
            // 
            // groupBoxLaserInfo
            // 
            this.groupBoxLaserInfo.Controls.Add(this.labelOutOfRange);
            this.groupBoxLaserInfo.Controls.Add(this.label1);
            this.groupBoxLaserInfo.Controls.Add(this.lockChannelNum);
            this.groupBoxLaserInfo.Controls.Add(this.displayFreq);
            this.groupBoxLaserInfo.Controls.Add(this.displayWL);
            this.groupBoxLaserInfo.Controls.Add(this.label10);
            this.groupBoxLaserInfo.Controls.Add(this.VOut);
            this.groupBoxLaserInfo.Controls.Add(this.frequencyError);
            this.groupBoxLaserInfo.Controls.Add(this.label8);
            this.groupBoxLaserInfo.Controls.Add(this.label5);
            this.groupBoxLaserInfo.Controls.Add(this.label3);
            this.groupBoxLaserInfo.Location = new System.Drawing.Point(27, 9);
            this.groupBoxLaserInfo.Name = "groupBoxLaserInfo";
            this.groupBoxLaserInfo.Size = new System.Drawing.Size(218, 193);
            this.groupBoxLaserInfo.TabIndex = 74;
            this.groupBoxLaserInfo.TabStop = false;
            this.groupBoxLaserInfo.Text = "Laser Status";
            this.groupBoxLaserInfo.Enter += new System.EventHandler(this.groupBoxLaserInfo_Enter);
            // 
            // labelOutOfRange
            // 
            this.labelOutOfRange.AutoSize = true;
            this.labelOutOfRange.ForeColor = System.Drawing.Color.Red;
            this.labelOutOfRange.Location = new System.Drawing.Point(110, 166);
            this.labelOutOfRange.Name = "labelOutOfRange";
            this.labelOutOfRange.Size = new System.Drawing.Size(74, 13);
            this.labelOutOfRange.TabIndex = 75;
            this.labelOutOfRange.Text = "Out of Range!";
            // 
            // TestLabel
            // 
            this.TestLabel.AutoSize = true;
            this.TestLabel.Location = new System.Drawing.Point(353, 163);
            this.TestLabel.Name = "TestLabel";
            this.TestLabel.Size = new System.Drawing.Size(33, 13);
            this.TestLabel.TabIndex = 77;
            this.TestLabel.Text = "None";
            this.TestLabel.Click += new System.EventHandler(this.label12_Click);
            // 
            // LockControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.stepDown);
            this.Controls.Add(this.stepUp);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.stepSize);
            this.Controls.Add(this.IGainSet);
            this.Controls.Add(this.IGain);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.PGainSet);
            this.Controls.Add(this.PGain);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lockButton);
            this.Controls.Add(this.SetPoint);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.groupBoxErrorPlot);
            this.Controls.Add(this.groupBoxLaserInfo);
            this.Name = "LockControlPanel";
            this.Size = new System.Drawing.Size(679, 522);
            this.Load += new System.EventHandler(this.LockControlPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.lockLED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorScatterGraph)).EndInit();
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            this.groupBoxErrorPlot.ResumeLayout(false);
            this.groupBoxLaserInfo.ResumeLayout(false);
            this.groupBoxLaserInfo.PerformLayout();
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
        private Button lockButton;
        private TextBox SetPoint;
        private Label label4;
        private Label label3;
        private Label displayWL;
        private Label label1;
        private NationalInstruments.UI.WindowsForms.Led lockLED;
        private Label lockChannelNum;
        private NationalInstruments.UI.WindowsForms.ScatterGraph errorScatterGraph;
        private NationalInstruments.UI.ScatterPlot errorPlot;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private Button scaleDown;
        private Button scaleUp;
        private Label label6;
        private GroupBox controlPanel;
        private GroupBox groupBoxErrorPlot;
        private GroupBox groupBoxLaserInfo;
        private Button resetGraph;
        private TextBox offset;
        private Label label7;
        private Button offsetSet;
        private Button setAsReading;
        private Label labelOutOfRange;
        private Label TestLabel;
    }
}
