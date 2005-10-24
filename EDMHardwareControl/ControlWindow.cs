using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace EDMHardwareControl
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class ControlWindow : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox2;
		public System.Windows.Forms.CheckBox eOnCheck;
		public System.Windows.Forms.CheckBox ePolarityCheck;
		public System.Windows.Forms.CheckBox eBleedCheck;
		public System.Windows.Forms.Button switchEButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.CheckBox greenOnCheck;
		private System.Windows.Forms.Label label6;
		public System.Windows.Forms.TextBox redOnFreqBox;
		private System.Windows.Forms.Label label7;
		public System.Windows.Forms.TextBox greenOnAmpBox;
		private System.Windows.Forms.Label label8;
		public System.Windows.Forms.TextBox greenOnFreqBox;
		public System.Windows.Forms.CheckBox redOnCheck;
		public System.Windows.Forms.Button setRamanButton;
		public System.Windows.Forms.TextBox ramanAmplitudeBox;
		public System.Windows.Forms.TextBox urFrequencyBox;
		public System.Windows.Forms.TextBox lrFrequencyBox;
		public System.Windows.Forms.CheckBox greenFMEnableCheck;
		public System.Windows.Forms.TextBox greenDCFMBox;
		public System.Windows.Forms.CheckBox redRFSwitchEnableCheck;
		public System.Windows.Forms.CheckBox greenRFSwitchEnableCheck;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button updateFieldButton;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label5;
		public System.Windows.Forms.TextBox gPlusTextBox;
		public System.Windows.Forms.TextBox gMinusTextBox;
		public System.Windows.Forms.TextBox cMinusTextBox;
		public System.Windows.Forms.TextBox cPlusTextBox;
		private System.Windows.Forms.GroupBox groupBox5;
		public System.Windows.Forms.CheckBox bFlipCheck;
		public System.Windows.Forms.CheckBox calFlipCheck;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		public System.Windows.Forms.TextBox consoleBox;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		public System.Windows.Forms.TextBox cMinusVMonitorTextBox;
		public System.Windows.Forms.TextBox gPlusVMonitorTextBox;
		public System.Windows.Forms.TextBox cPlusVMonitorTextBox;
		public System.Windows.Forms.TextBox gMinusVMonitorTextBox;
		private System.Windows.Forms.GroupBox groupBox7;
		public System.Windows.Forms.TextBox cPlusIMonitorTextBox;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
		public System.Windows.Forms.TextBox gPlusIMonitorTextBox;
		public System.Windows.Forms.TextBox gMinusIMonitorTextBox;
		public System.Windows.Forms.TextBox cMinusIMonitorTextBox;
		private System.Windows.Forms.Button updateVMonitorButton;
		private System.Windows.Forms.Button updateIMonitorButton;
		private System.Windows.Forms.GroupBox groupBox8;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.Label label27;
		public System.Windows.Forms.TextBox bCurrent01TextBox;
		public System.Windows.Forms.TextBox bCurrent11TextBox;
		public System.Windows.Forms.TextBox bCurrent10TextBox;
		public System.Windows.Forms.TextBox bCurrent00TextBox;
		public System.Windows.Forms.TextBox bCurrentCalStepTextBox;
		public System.Windows.Forms.TextBox bCurrentFlipStepTextBox;
		public System.Windows.Forms.TextBox bCurrentBiasTextBox;
		private System.Windows.Forms.Button updateBCurrentMonitorButton;


		public Controller controller;

		public ControlWindow()
		{
			InitializeComponent();
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ControlWindow));
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.switchEButton = new System.Windows.Forms.Button();
			this.eBleedCheck = new System.Windows.Forms.CheckBox();
			this.ePolarityCheck = new System.Windows.Forms.CheckBox();
			this.eOnCheck = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.setRamanButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.ramanAmplitudeBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.urFrequencyBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.lrFrequencyBox = new System.Windows.Forms.TextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.greenFMEnableCheck = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.greenDCFMBox = new System.Windows.Forms.TextBox();
			this.redRFSwitchEnableCheck = new System.Windows.Forms.CheckBox();
			this.greenOnCheck = new System.Windows.Forms.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.redOnFreqBox = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.greenOnAmpBox = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.greenOnFreqBox = new System.Windows.Forms.TextBox();
			this.greenRFSwitchEnableCheck = new System.Windows.Forms.CheckBox();
			this.redOnCheck = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.updateFieldButton = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.gPlusTextBox = new System.Windows.Forms.TextBox();
			this.gMinusTextBox = new System.Windows.Forms.TextBox();
			this.cMinusTextBox = new System.Windows.Forms.TextBox();
			this.cPlusTextBox = new System.Windows.Forms.TextBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.calFlipCheck = new System.Windows.Forms.CheckBox();
			this.bFlipCheck = new System.Windows.Forms.CheckBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.gMinusVMonitorTextBox = new System.Windows.Forms.TextBox();
			this.cPlusVMonitorTextBox = new System.Windows.Forms.TextBox();
			this.gPlusVMonitorTextBox = new System.Windows.Forms.TextBox();
			this.updateVMonitorButton = new System.Windows.Forms.Button();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.cMinusVMonitorTextBox = new System.Windows.Forms.TextBox();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.cMinusIMonitorTextBox = new System.Windows.Forms.TextBox();
			this.gMinusIMonitorTextBox = new System.Windows.Forms.TextBox();
			this.gPlusIMonitorTextBox = new System.Windows.Forms.TextBox();
			this.cPlusIMonitorTextBox = new System.Windows.Forms.TextBox();
			this.updateIMonitorButton = new System.Windows.Forms.Button();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.consoleBox = new System.Windows.Forms.TextBox();
			this.groupBox8 = new System.Windows.Forms.GroupBox();
			this.bCurrent01TextBox = new System.Windows.Forms.TextBox();
			this.bCurrent11TextBox = new System.Windows.Forms.TextBox();
			this.bCurrent10TextBox = new System.Windows.Forms.TextBox();
			this.bCurrent00TextBox = new System.Windows.Forms.TextBox();
			this.updateBCurrentMonitorButton = new System.Windows.Forms.Button();
			this.label20 = new System.Windows.Forms.Label();
			this.label21 = new System.Windows.Forms.Label();
			this.label22 = new System.Windows.Forms.Label();
			this.label23 = new System.Windows.Forms.Label();
			this.bCurrentCalStepTextBox = new System.Windows.Forms.TextBox();
			this.bCurrentFlipStepTextBox = new System.Windows.Forms.TextBox();
			this.bCurrentBiasTextBox = new System.Windows.Forms.TextBox();
			this.label25 = new System.Windows.Forms.Label();
			this.label26 = new System.Windows.Forms.Label();
			this.label27 = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.groupBox8.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.switchEButton);
			this.groupBox2.Controls.Add(this.eBleedCheck);
			this.groupBox2.Controls.Add(this.ePolarityCheck);
			this.groupBox2.Controls.Add(this.eOnCheck);
			this.groupBox2.Location = new System.Drawing.Point(16, 232);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(280, 112);
			this.groupBox2.TabIndex = 11;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Switch";
			// 
			// switchEButton
			// 
			this.switchEButton.Location = new System.Drawing.Point(136, 64);
			this.switchEButton.Name = "switchEButton";
			this.switchEButton.Size = new System.Drawing.Size(96, 23);
			this.switchEButton.TabIndex = 22;
			this.switchEButton.Text = "Switch E";
			this.switchEButton.Click += new System.EventHandler(this.switchEButton_Click);
			// 
			// eBleedCheck
			// 
			this.eBleedCheck.Location = new System.Drawing.Point(24, 64);
			this.eBleedCheck.Name = "eBleedCheck";
			this.eBleedCheck.Size = new System.Drawing.Size(72, 24);
			this.eBleedCheck.TabIndex = 21;
			this.eBleedCheck.Text = "Bleed on";
			this.eBleedCheck.CheckedChanged += new System.EventHandler(this.eBleedCheck_CheckedChanged);
			// 
			// ePolarityCheck
			// 
			this.ePolarityCheck.Location = new System.Drawing.Point(120, 32);
			this.ePolarityCheck.Name = "ePolarityCheck";
			this.ePolarityCheck.Size = new System.Drawing.Size(136, 24);
			this.ePolarityCheck.TabIndex = 20;
			this.ePolarityCheck.Text = "Polarity (1 is checked)";
			this.ePolarityCheck.CheckedChanged += new System.EventHandler(this.ePolarityCheck_CheckedChanged);
			// 
			// eOnCheck
			// 
			this.eOnCheck.Location = new System.Drawing.Point(24, 32);
			this.eOnCheck.Name = "eOnCheck";
			this.eOnCheck.Size = new System.Drawing.Size(72, 24);
			this.eOnCheck.TabIndex = 19;
			this.eOnCheck.Text = "Field on";
			this.eOnCheck.CheckedChanged += new System.EventHandler(this.eOnCheck_CheckedChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.setRamanButton);
			this.groupBox4.Controls.Add(this.label4);
			this.groupBox4.Controls.Add(this.ramanAmplitudeBox);
			this.groupBox4.Controls.Add(this.label3);
			this.groupBox4.Controls.Add(this.urFrequencyBox);
			this.groupBox4.Controls.Add(this.label2);
			this.groupBox4.Controls.Add(this.lrFrequencyBox);
			this.groupBox4.Location = new System.Drawing.Point(312, 16);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(296, 160);
			this.groupBox4.TabIndex = 22;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Dual rf (FM)";
			// 
			// setRamanButton
			// 
			this.setRamanButton.Location = new System.Drawing.Point(88, 128);
			this.setRamanButton.Name = "setRamanButton";
			this.setRamanButton.TabIndex = 18;
			this.setRamanButton.Text = "Set synth";
			this.setRamanButton.Click += new System.EventHandler(this.setRamanButton_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 88);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(96, 23);
			this.label4.TabIndex = 17;
			this.label4.Text = "Amplitude";
			// 
			// ramanAmplitudeBox
			// 
			this.ramanAmplitudeBox.Location = new System.Drawing.Point(136, 88);
			this.ramanAmplitudeBox.Name = "ramanAmplitudeBox";
			this.ramanAmplitudeBox.Size = new System.Drawing.Size(64, 20);
			this.ramanAmplitudeBox.TabIndex = 16;
			this.ramanAmplitudeBox.Text = "3.3";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(96, 23);
			this.label3.TabIndex = 15;
			this.label3.Text = "UR Frequency";
			// 
			// urFrequencyBox
			// 
			this.urFrequencyBox.Location = new System.Drawing.Point(136, 56);
			this.urFrequencyBox.Name = "urFrequencyBox";
			this.urFrequencyBox.Size = new System.Drawing.Size(64, 20);
			this.urFrequencyBox.TabIndex = 14;
			this.urFrequencyBox.Text = "170.815";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 23);
			this.label2.TabIndex = 13;
			this.label2.Text = "LR Frequency";
			// 
			// lrFrequencyBox
			// 
			this.lrFrequencyBox.Location = new System.Drawing.Point(136, 24);
			this.lrFrequencyBox.Name = "lrFrequencyBox";
			this.lrFrequencyBox.Size = new System.Drawing.Size(64, 20);
			this.lrFrequencyBox.TabIndex = 12;
			this.lrFrequencyBox.Text = "170.795";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.greenFMEnableCheck);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.greenDCFMBox);
			this.groupBox3.Controls.Add(this.redRFSwitchEnableCheck);
			this.groupBox3.Controls.Add(this.greenOnCheck);
			this.groupBox3.Controls.Add(this.label6);
			this.groupBox3.Controls.Add(this.redOnFreqBox);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Controls.Add(this.greenOnAmpBox);
			this.groupBox3.Controls.Add(this.label8);
			this.groupBox3.Controls.Add(this.greenOnFreqBox);
			this.groupBox3.Controls.Add(this.greenRFSwitchEnableCheck);
			this.groupBox3.Controls.Add(this.redOnCheck);
			this.groupBox3.Location = new System.Drawing.Point(8, 16);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(296, 280);
			this.groupBox3.TabIndex = 21;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Direct synth control";
			// 
			// greenFMEnableCheck
			// 
			this.greenFMEnableCheck.Location = new System.Drawing.Point(24, 248);
			this.greenFMEnableCheck.Name = "greenFMEnableCheck";
			this.greenFMEnableCheck.Size = new System.Drawing.Size(152, 24);
			this.greenFMEnableCheck.TabIndex = 24;
			this.greenFMEnableCheck.Text = "Green DC FM enabled";
			this.greenFMEnableCheck.CheckedChanged += new System.EventHandler(this.greenFMEnableCheck_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 88);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(144, 23);
			this.label1.TabIndex = 23;
			this.label1.Text = "Green synth DC FM (kHz)";
			// 
			// greenDCFMBox
			// 
			this.greenDCFMBox.Location = new System.Drawing.Point(168, 88);
			this.greenDCFMBox.Name = "greenDCFMBox";
			this.greenDCFMBox.Size = new System.Drawing.Size(64, 20);
			this.greenDCFMBox.TabIndex = 22;
			this.greenDCFMBox.Text = "20";
			// 
			// redRFSwitchEnableCheck
			// 
			this.redRFSwitchEnableCheck.Location = new System.Drawing.Point(24, 216);
			this.redRFSwitchEnableCheck.Name = "redRFSwitchEnableCheck";
			this.redRFSwitchEnableCheck.Size = new System.Drawing.Size(136, 24);
			this.redRFSwitchEnableCheck.TabIndex = 21;
			this.redRFSwitchEnableCheck.Text = "Enable rf2 switch";
			this.redRFSwitchEnableCheck.CheckedChanged += new System.EventHandler(this.redRFSwitchEnableCheck_CheckedChanged);
			// 
			// greenOnCheck
			// 
			this.greenOnCheck.Location = new System.Drawing.Point(24, 152);
			this.greenOnCheck.Name = "greenOnCheck";
			this.greenOnCheck.TabIndex = 18;
			this.greenOnCheck.Text = "Green on";
			this.greenOnCheck.CheckedChanged += new System.EventHandler(this.greenOnCheck_CheckedChanged);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(24, 120);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(144, 23);
			this.label6.TabIndex = 15;
			this.label6.Text = "Red synth frequency";
			// 
			// redOnFreqBox
			// 
			this.redOnFreqBox.Location = new System.Drawing.Point(168, 120);
			this.redOnFreqBox.Name = "redOnFreqBox";
			this.redOnFreqBox.Size = new System.Drawing.Size(64, 20);
			this.redOnFreqBox.TabIndex = 14;
			this.redOnFreqBox.Text = "42.5";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(24, 56);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(144, 23);
			this.label7.TabIndex = 13;
			this.label7.Text = "Green synth amplitude";
			// 
			// greenOnAmpBox
			// 
			this.greenOnAmpBox.Location = new System.Drawing.Point(168, 56);
			this.greenOnAmpBox.Name = "greenOnAmpBox";
			this.greenOnAmpBox.Size = new System.Drawing.Size(64, 20);
			this.greenOnAmpBox.TabIndex = 12;
			this.greenOnAmpBox.Text = "3.3";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(24, 24);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(144, 23);
			this.label8.TabIndex = 11;
			this.label8.Text = "Green synth frequency";
			// 
			// greenOnFreqBox
			// 
			this.greenOnFreqBox.Location = new System.Drawing.Point(168, 24);
			this.greenOnFreqBox.Name = "greenOnFreqBox";
			this.greenOnFreqBox.Size = new System.Drawing.Size(64, 20);
			this.greenOnFreqBox.TabIndex = 10;
			this.greenOnFreqBox.Text = "170.795";
			// 
			// greenRFSwitchEnableCheck
			// 
			this.greenRFSwitchEnableCheck.Location = new System.Drawing.Point(24, 184);
			this.greenRFSwitchEnableCheck.Name = "greenRFSwitchEnableCheck";
			this.greenRFSwitchEnableCheck.Size = new System.Drawing.Size(152, 24);
			this.greenRFSwitchEnableCheck.TabIndex = 20;
			this.greenRFSwitchEnableCheck.Text = "Enable rf1 switch";
			this.greenRFSwitchEnableCheck.CheckedChanged += new System.EventHandler(this.GreenRFSwitchEnableCheck_CheckedChanged);
			// 
			// redOnCheck
			// 
			this.redOnCheck.Location = new System.Drawing.Point(152, 152);
			this.redOnCheck.Name = "redOnCheck";
			this.redOnCheck.TabIndex = 19;
			this.redOnCheck.Text = "Red on";
			this.redOnCheck.CheckedChanged += new System.EventHandler(this.redOnCheck_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.updateFieldButton);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.gPlusTextBox);
			this.groupBox1.Controls.Add(this.gMinusTextBox);
			this.groupBox1.Controls.Add(this.cMinusTextBox);
			this.groupBox1.Controls.Add(this.cPlusTextBox);
			this.groupBox1.Location = new System.Drawing.Point(16, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(184, 208);
			this.groupBox1.TabIndex = 23;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Supplies";
			// 
			// updateFieldButton
			// 
			this.updateFieldButton.Location = new System.Drawing.Point(48, 168);
			this.updateFieldButton.Name = "updateFieldButton";
			this.updateFieldButton.TabIndex = 40;
			this.updateFieldButton.Text = "Update";
			this.updateFieldButton.Click += new System.EventHandler(this.updateFieldButton_Click);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(16, 128);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(80, 23);
			this.label10.TabIndex = 39;
			this.label10.Text = "G minus (V)";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(16, 96);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(80, 23);
			this.label11.TabIndex = 38;
			this.label11.Text = "G plus (V)";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 56);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(80, 23);
			this.label9.TabIndex = 37;
			this.label9.Text = "C minus (V)";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 24);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 23);
			this.label5.TabIndex = 36;
			this.label5.Text = "C plus (V)";
			// 
			// gPlusTextBox
			// 
			this.gPlusTextBox.Location = new System.Drawing.Point(104, 96);
			this.gPlusTextBox.Name = "gPlusTextBox";
			this.gPlusTextBox.Size = new System.Drawing.Size(64, 20);
			this.gPlusTextBox.TabIndex = 35;
			this.gPlusTextBox.Text = "1600";
			// 
			// gMinusTextBox
			// 
			this.gMinusTextBox.Location = new System.Drawing.Point(104, 128);
			this.gMinusTextBox.Name = "gMinusTextBox";
			this.gMinusTextBox.Size = new System.Drawing.Size(64, 20);
			this.gMinusTextBox.TabIndex = 34;
			this.gMinusTextBox.Text = "-1600";
			// 
			// cMinusTextBox
			// 
			this.cMinusTextBox.Location = new System.Drawing.Point(104, 56);
			this.cMinusTextBox.Name = "cMinusTextBox";
			this.cMinusTextBox.Size = new System.Drawing.Size(64, 20);
			this.cMinusTextBox.TabIndex = 33;
			this.cMinusTextBox.Text = "-8000";
			// 
			// cPlusTextBox
			// 
			this.cPlusTextBox.Location = new System.Drawing.Point(104, 24);
			this.cPlusTextBox.Name = "cPlusTextBox";
			this.cPlusTextBox.Size = new System.Drawing.Size(64, 20);
			this.cPlusTextBox.TabIndex = 32;
			this.cPlusTextBox.Text = "8000";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.calFlipCheck);
			this.groupBox5.Controls.Add(this.bFlipCheck);
			this.groupBox5.Location = new System.Drawing.Point(8, 16);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(280, 56);
			this.groupBox5.TabIndex = 24;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Manual B-flip";
			// 
			// calFlipCheck
			// 
			this.calFlipCheck.Location = new System.Drawing.Point(152, 24);
			this.calFlipCheck.Name = "calFlipCheck";
			this.calFlipCheck.Size = new System.Drawing.Size(40, 24);
			this.calFlipCheck.TabIndex = 1;
			this.calFlipCheck.Text = "dB";
			this.calFlipCheck.CheckedChanged += new System.EventHandler(this.calFlipCheck_CheckedChanged);
			// 
			// bFlipCheck
			// 
			this.bFlipCheck.Location = new System.Drawing.Point(16, 24);
			this.bFlipCheck.Name = "bFlipCheck";
			this.bFlipCheck.Size = new System.Drawing.Size(40, 24);
			this.bFlipCheck.TabIndex = 0;
			this.bFlipCheck.Text = "DB";
			this.bFlipCheck.CheckedChanged += new System.EventHandler(this.bFlipCheck_CheckedChanged);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Location = new System.Drawing.Point(8, 16);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(624, 384);
			this.tabControl1.TabIndex = 25;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox6);
			this.tabPage1.Controls.Add(this.groupBox2);
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Controls.Add(this.groupBox7);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(616, 358);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "E-field";
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.gMinusVMonitorTextBox);
			this.groupBox6.Controls.Add(this.cPlusVMonitorTextBox);
			this.groupBox6.Controls.Add(this.gPlusVMonitorTextBox);
			this.groupBox6.Controls.Add(this.updateVMonitorButton);
			this.groupBox6.Controls.Add(this.label12);
			this.groupBox6.Controls.Add(this.label13);
			this.groupBox6.Controls.Add(this.label14);
			this.groupBox6.Controls.Add(this.label15);
			this.groupBox6.Controls.Add(this.cMinusVMonitorTextBox);
			this.groupBox6.Location = new System.Drawing.Point(216, 16);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(184, 208);
			this.groupBox6.TabIndex = 24;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Voltage monitors";
			// 
			// gMinusVMonitorTextBox
			// 
			this.gMinusVMonitorTextBox.BackColor = System.Drawing.Color.Black;
			this.gMinusVMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.gMinusVMonitorTextBox.Location = new System.Drawing.Point(104, 128);
			this.gMinusVMonitorTextBox.Name = "gMinusVMonitorTextBox";
			this.gMinusVMonitorTextBox.ReadOnly = true;
			this.gMinusVMonitorTextBox.Size = new System.Drawing.Size(64, 20);
			this.gMinusVMonitorTextBox.TabIndex = 43;
			this.gMinusVMonitorTextBox.Text = "0";
			// 
			// cPlusVMonitorTextBox
			// 
			this.cPlusVMonitorTextBox.BackColor = System.Drawing.Color.Black;
			this.cPlusVMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.cPlusVMonitorTextBox.Location = new System.Drawing.Point(104, 24);
			this.cPlusVMonitorTextBox.Name = "cPlusVMonitorTextBox";
			this.cPlusVMonitorTextBox.ReadOnly = true;
			this.cPlusVMonitorTextBox.Size = new System.Drawing.Size(64, 20);
			this.cPlusVMonitorTextBox.TabIndex = 42;
			this.cPlusVMonitorTextBox.Text = "0";
			// 
			// gPlusVMonitorTextBox
			// 
			this.gPlusVMonitorTextBox.BackColor = System.Drawing.Color.Black;
			this.gPlusVMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.gPlusVMonitorTextBox.Location = new System.Drawing.Point(104, 96);
			this.gPlusVMonitorTextBox.Name = "gPlusVMonitorTextBox";
			this.gPlusVMonitorTextBox.ReadOnly = true;
			this.gPlusVMonitorTextBox.Size = new System.Drawing.Size(64, 20);
			this.gPlusVMonitorTextBox.TabIndex = 41;
			this.gPlusVMonitorTextBox.Text = "0";
			// 
			// updateVMonitorButton
			// 
			this.updateVMonitorButton.Location = new System.Drawing.Point(56, 168);
			this.updateVMonitorButton.Name = "updateVMonitorButton";
			this.updateVMonitorButton.TabIndex = 40;
			this.updateVMonitorButton.Text = "Update";
			this.updateVMonitorButton.Click += new System.EventHandler(this.updateVMonitorButton_Click);
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(16, 128);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(80, 23);
			this.label12.TabIndex = 39;
			this.label12.Text = "G minus (V)";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(16, 96);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(80, 23);
			this.label13.TabIndex = 38;
			this.label13.Text = "G plus (V)";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(16, 56);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(80, 23);
			this.label14.TabIndex = 37;
			this.label14.Text = "C minus (V)";
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(16, 24);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(80, 23);
			this.label15.TabIndex = 36;
			this.label15.Text = "C plus (V)";
			// 
			// cMinusVMonitorTextBox
			// 
			this.cMinusVMonitorTextBox.BackColor = System.Drawing.Color.Black;
			this.cMinusVMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.cMinusVMonitorTextBox.Location = new System.Drawing.Point(104, 56);
			this.cMinusVMonitorTextBox.Name = "cMinusVMonitorTextBox";
			this.cMinusVMonitorTextBox.ReadOnly = true;
			this.cMinusVMonitorTextBox.Size = new System.Drawing.Size(64, 20);
			this.cMinusVMonitorTextBox.TabIndex = 33;
			this.cMinusVMonitorTextBox.Text = "0";
			// 
			// groupBox7
			// 
			this.groupBox7.Controls.Add(this.cMinusIMonitorTextBox);
			this.groupBox7.Controls.Add(this.gMinusIMonitorTextBox);
			this.groupBox7.Controls.Add(this.gPlusIMonitorTextBox);
			this.groupBox7.Controls.Add(this.cPlusIMonitorTextBox);
			this.groupBox7.Controls.Add(this.updateIMonitorButton);
			this.groupBox7.Controls.Add(this.label16);
			this.groupBox7.Controls.Add(this.label17);
			this.groupBox7.Controls.Add(this.label18);
			this.groupBox7.Controls.Add(this.label19);
			this.groupBox7.Location = new System.Drawing.Point(416, 16);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(184, 208);
			this.groupBox7.TabIndex = 44;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Current monitors";
			// 
			// cMinusIMonitorTextBox
			// 
			this.cMinusIMonitorTextBox.BackColor = System.Drawing.Color.Black;
			this.cMinusIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.cMinusIMonitorTextBox.Location = new System.Drawing.Point(104, 56);
			this.cMinusIMonitorTextBox.Name = "cMinusIMonitorTextBox";
			this.cMinusIMonitorTextBox.ReadOnly = true;
			this.cMinusIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
			this.cMinusIMonitorTextBox.TabIndex = 45;
			this.cMinusIMonitorTextBox.Text = "0";
			// 
			// gMinusIMonitorTextBox
			// 
			this.gMinusIMonitorTextBox.BackColor = System.Drawing.Color.Black;
			this.gMinusIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.gMinusIMonitorTextBox.Location = new System.Drawing.Point(104, 128);
			this.gMinusIMonitorTextBox.Name = "gMinusIMonitorTextBox";
			this.gMinusIMonitorTextBox.ReadOnly = true;
			this.gMinusIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
			this.gMinusIMonitorTextBox.TabIndex = 44;
			this.gMinusIMonitorTextBox.Text = "0";
			// 
			// gPlusIMonitorTextBox
			// 
			this.gPlusIMonitorTextBox.BackColor = System.Drawing.Color.Black;
			this.gPlusIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.gPlusIMonitorTextBox.Location = new System.Drawing.Point(104, 94);
			this.gPlusIMonitorTextBox.Name = "gPlusIMonitorTextBox";
			this.gPlusIMonitorTextBox.ReadOnly = true;
			this.gPlusIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
			this.gPlusIMonitorTextBox.TabIndex = 43;
			this.gPlusIMonitorTextBox.Text = "0";
			// 
			// cPlusIMonitorTextBox
			// 
			this.cPlusIMonitorTextBox.BackColor = System.Drawing.Color.Black;
			this.cPlusIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.cPlusIMonitorTextBox.Location = new System.Drawing.Point(104, 24);
			this.cPlusIMonitorTextBox.Name = "cPlusIMonitorTextBox";
			this.cPlusIMonitorTextBox.ReadOnly = true;
			this.cPlusIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
			this.cPlusIMonitorTextBox.TabIndex = 42;
			this.cPlusIMonitorTextBox.Text = "0";
			// 
			// updateIMonitorButton
			// 
			this.updateIMonitorButton.Location = new System.Drawing.Point(56, 168);
			this.updateIMonitorButton.Name = "updateIMonitorButton";
			this.updateIMonitorButton.TabIndex = 40;
			this.updateIMonitorButton.Text = "Update";
			this.updateIMonitorButton.Click += new System.EventHandler(this.updateIMonitorButton_Click);
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(16, 128);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(80, 23);
			this.label16.TabIndex = 39;
			this.label16.Text = "G minus (nA)";
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(16, 96);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(80, 23);
			this.label17.TabIndex = 38;
			this.label17.Text = "G plus (nA)";
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(16, 56);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(80, 23);
			this.label18.TabIndex = 37;
			this.label18.Text = "C minus (nA)";
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(16, 24);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(80, 23);
			this.label19.TabIndex = 36;
			this.label19.Text = "C plus (nA)";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Controls.Add(this.groupBox4);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(616, 358);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Synths";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.groupBox8);
			this.tabPage3.Controls.Add(this.groupBox5);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(616, 358);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "B-field";
			// 
			// consoleBox
			// 
			this.consoleBox.BackColor = System.Drawing.Color.Black;
			this.consoleBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.consoleBox.Location = new System.Drawing.Point(8, 408);
			this.consoleBox.Multiline = true;
			this.consoleBox.Name = "consoleBox";
			this.consoleBox.ReadOnly = true;
			this.consoleBox.Size = new System.Drawing.Size(624, 136);
			this.consoleBox.TabIndex = 26;
			this.consoleBox.Text = "";
			// 
			// groupBox8
			// 
			this.groupBox8.Controls.Add(this.bCurrentCalStepTextBox);
			this.groupBox8.Controls.Add(this.bCurrentFlipStepTextBox);
			this.groupBox8.Controls.Add(this.bCurrentBiasTextBox);
			this.groupBox8.Controls.Add(this.label25);
			this.groupBox8.Controls.Add(this.label26);
			this.groupBox8.Controls.Add(this.label27);
			this.groupBox8.Controls.Add(this.bCurrent01TextBox);
			this.groupBox8.Controls.Add(this.bCurrent11TextBox);
			this.groupBox8.Controls.Add(this.bCurrent10TextBox);
			this.groupBox8.Controls.Add(this.bCurrent00TextBox);
			this.groupBox8.Controls.Add(this.updateBCurrentMonitorButton);
			this.groupBox8.Controls.Add(this.label20);
			this.groupBox8.Controls.Add(this.label21);
			this.groupBox8.Controls.Add(this.label22);
			this.groupBox8.Controls.Add(this.label23);
			this.groupBox8.Location = new System.Drawing.Point(8, 88);
			this.groupBox8.Name = "groupBox8";
			this.groupBox8.Size = new System.Drawing.Size(376, 192);
			this.groupBox8.TabIndex = 45;
			this.groupBox8.TabStop = false;
			this.groupBox8.Text = "Current monitor";
			// 
			// bCurrent01TextBox
			// 
			this.bCurrent01TextBox.BackColor = System.Drawing.Color.Black;
			this.bCurrent01TextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.bCurrent01TextBox.Location = new System.Drawing.Point(104, 56);
			this.bCurrent01TextBox.Name = "bCurrent01TextBox";
			this.bCurrent01TextBox.ReadOnly = true;
			this.bCurrent01TextBox.Size = new System.Drawing.Size(64, 20);
			this.bCurrent01TextBox.TabIndex = 45;
			this.bCurrent01TextBox.Text = "0";
			// 
			// bCurrent11TextBox
			// 
			this.bCurrent11TextBox.BackColor = System.Drawing.Color.Black;
			this.bCurrent11TextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.bCurrent11TextBox.Location = new System.Drawing.Point(104, 120);
			this.bCurrent11TextBox.Name = "bCurrent11TextBox";
			this.bCurrent11TextBox.ReadOnly = true;
			this.bCurrent11TextBox.Size = new System.Drawing.Size(64, 20);
			this.bCurrent11TextBox.TabIndex = 44;
			this.bCurrent11TextBox.Text = "0";
			// 
			// bCurrent10TextBox
			// 
			this.bCurrent10TextBox.BackColor = System.Drawing.Color.Black;
			this.bCurrent10TextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.bCurrent10TextBox.Location = new System.Drawing.Point(104, 88);
			this.bCurrent10TextBox.Name = "bCurrent10TextBox";
			this.bCurrent10TextBox.ReadOnly = true;
			this.bCurrent10TextBox.Size = new System.Drawing.Size(64, 20);
			this.bCurrent10TextBox.TabIndex = 43;
			this.bCurrent10TextBox.Text = "0";
			// 
			// bCurrent00TextBox
			// 
			this.bCurrent00TextBox.BackColor = System.Drawing.Color.Black;
			this.bCurrent00TextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.bCurrent00TextBox.Location = new System.Drawing.Point(104, 24);
			this.bCurrent00TextBox.Name = "bCurrent00TextBox";
			this.bCurrent00TextBox.ReadOnly = true;
			this.bCurrent00TextBox.Size = new System.Drawing.Size(64, 20);
			this.bCurrent00TextBox.TabIndex = 42;
			this.bCurrent00TextBox.Text = "0";
			// 
			// updateBCurrentMonitorButton
			// 
			this.updateBCurrentMonitorButton.Location = new System.Drawing.Point(152, 152);
			this.updateBCurrentMonitorButton.Name = "updateBCurrentMonitorButton";
			this.updateBCurrentMonitorButton.TabIndex = 40;
			this.updateBCurrentMonitorButton.Text = "Update";
			this.updateBCurrentMonitorButton.Click += new System.EventHandler(this.updateBCurrentMonitorButton_Click);
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(16, 120);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(80, 23);
			this.label20.TabIndex = 39;
			this.label20.Text = "DB1 dB1 (uA)";
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(16, 88);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(80, 23);
			this.label21.TabIndex = 38;
			this.label21.Text = "DB1 dB0 (uA)";
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(16, 56);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(80, 23);
			this.label22.TabIndex = 37;
			this.label22.Text = "DB0 dB1 (uA)";
			// 
			// label23
			// 
			this.label23.Location = new System.Drawing.Point(16, 24);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(80, 23);
			this.label23.TabIndex = 36;
			this.label23.Text = "DB0 dB0 (uA)";
			// 
			// bCurrentCalStepTextBox
			// 
			this.bCurrentCalStepTextBox.BackColor = System.Drawing.Color.Black;
			this.bCurrentCalStepTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.bCurrentCalStepTextBox.Location = new System.Drawing.Point(288, 56);
			this.bCurrentCalStepTextBox.Name = "bCurrentCalStepTextBox";
			this.bCurrentCalStepTextBox.ReadOnly = true;
			this.bCurrentCalStepTextBox.Size = new System.Drawing.Size(64, 20);
			this.bCurrentCalStepTextBox.TabIndex = 53;
			this.bCurrentCalStepTextBox.Text = "0";
			// 
			// bCurrentFlipStepTextBox
			// 
			this.bCurrentFlipStepTextBox.BackColor = System.Drawing.Color.Black;
			this.bCurrentFlipStepTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.bCurrentFlipStepTextBox.Location = new System.Drawing.Point(288, 88);
			this.bCurrentFlipStepTextBox.Name = "bCurrentFlipStepTextBox";
			this.bCurrentFlipStepTextBox.ReadOnly = true;
			this.bCurrentFlipStepTextBox.Size = new System.Drawing.Size(64, 20);
			this.bCurrentFlipStepTextBox.TabIndex = 51;
			this.bCurrentFlipStepTextBox.Text = "0";
			// 
			// bCurrentBiasTextBox
			// 
			this.bCurrentBiasTextBox.BackColor = System.Drawing.Color.Black;
			this.bCurrentBiasTextBox.ForeColor = System.Drawing.Color.Chartreuse;
			this.bCurrentBiasTextBox.Location = new System.Drawing.Point(288, 24);
			this.bCurrentBiasTextBox.Name = "bCurrentBiasTextBox";
			this.bCurrentBiasTextBox.ReadOnly = true;
			this.bCurrentBiasTextBox.Size = new System.Drawing.Size(64, 20);
			this.bCurrentBiasTextBox.TabIndex = 50;
			this.bCurrentBiasTextBox.Text = "0";
			// 
			// label25
			// 
			this.label25.Location = new System.Drawing.Point(200, 88);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(80, 23);
			this.label25.TabIndex = 48;
			this.label25.Text = "DB (uA)";
			// 
			// label26
			// 
			this.label26.Location = new System.Drawing.Point(200, 56);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(80, 23);
			this.label26.TabIndex = 47;
			this.label26.Text = "dB (uA)";
			// 
			// label27
			// 
			this.label27.Location = new System.Drawing.Point(200, 24);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(80, 23);
			this.label27.TabIndex = 46;
			this.label27.Text = "Bias (uA)";
			// 
			// ControlWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(648, 558);
			this.Controls.Add(this.consoleBox);
			this.Controls.Add(this.tabControl1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ControlWindow";
			this.Text = "EDM Hardware Control";
			this.groupBox2.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox7.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.groupBox8.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Click handlers

		private void switchEButton_Click(object sender, System.EventArgs e)
		{
			controller.SwitchE();
		}

		private void greenOnCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.EnableGreenSynth(greenOnCheck.Checked);
		}

	
		private void redOnCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.EnableRedSynth(redOnCheck.Checked);
		}

		private void GreenRFSwitchEnableCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.EnableGreenRFSwitch(greenRFSwitchEnableCheck.Checked);	
		}

		private void redRFSwitchEnableCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.EnableRedRFSwitch(redRFSwitchEnableCheck.Checked);			
		}

		private void eOnCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.SetEFieldOnOff(eOnCheck.Checked);
		}

		private void ePolarityCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.SetEPolarity(ePolarityCheck.Checked);
		}

		private void eBleedCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.SetBleed(eBleedCheck.Checked);
		}

		private void greenFMEnableCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.EnableGreenDCFM(greenFMEnableCheck.Checked);
		}

		private void updateFieldButton_Click(object sender, System.EventArgs e)
		{
			controller.UpdateVoltages();
		}

		// convenience method to set up synths for Raman
		private void setRamanButton_Click(object sender, System.EventArgs e)
		{
			controller.SetRaman();
		}

		private void bFlipCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.SetBFlip(bFlipCheck.Checked);
		}

		private void calFlipCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.SetCalFlip(calFlipCheck.Checked);
		
		}
		private void updateVMonitorButton_Click(object sender, System.EventArgs e)
		{
			controller.UpdateVMonitor();
		}

		private void updateIMonitorButton_Click(object sender, System.EventArgs e)
		{
			controller.UpdateIMonitor();
		}

		private void updateBCurrentMonitorButton_Click(object sender, System.EventArgs e)
		{
			controller.UpdateBCurrentMonitor();
		}
		#endregion

		#region ThreadSafe wrappers

		public void SetCheckBox(CheckBox box, bool state)
		{
			box.Invoke(new SetCheckDelegate(SetCheckHelper), new object[] {box, state} );
		}
		private delegate void SetCheckDelegate(CheckBox box, bool state);
		private void SetCheckHelper(CheckBox box, bool state)
		{
			box.Checked = state;
		}

		public void SetTextBox(TextBox box, string text)
		{
			box.Invoke(new SetTextDelegate(SetTextHelper), new object[] {box, text});
		}
		private delegate void SetTextDelegate(TextBox box, string text);
		private void SetTextHelper(TextBox box, string text)
		{
			box.Text = text;
		}
		#endregion










	}
}
