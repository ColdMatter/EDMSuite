using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace EDMHardwareControl
{
	/// <summary>
	/// Front panel for the edm hardware controller. Everything is just stuffed in there. No particularly
	/// clever structure. This class just hands everything straight off to the controller. It has a few
	/// thread safe wrappers so that remote calls can safely manipulate the front panel.
	/// </summary>
	public class ControlWindow : System.Windows.Forms.Form
	{
		#region Setup

		private System.Windows.Forms.GroupBox groupBox2;
		public System.Windows.Forms.CheckBox eOnCheck;
		public System.Windows.Forms.CheckBox ePolarityCheck;
		public System.Windows.Forms.CheckBox eBleedCheck;
		public System.Windows.Forms.Button switchEButton;
        private System.ComponentModel.Container components = null;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckBox greenOnCheck;
		private System.Windows.Forms.Label label7;
		public System.Windows.Forms.TextBox greenOnAmpBox;
		private System.Windows.Forms.Label label8;
        public System.Windows.Forms.TextBox greenOnFreqBox;
		public System.Windows.Forms.TextBox greenDCFMBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button updateFieldButton;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label5;
		public System.Windows.Forms.TextBox cPlusOffTextBox;
		public System.Windows.Forms.TextBox cMinusOffTextBox;
		public System.Windows.Forms.TextBox cMinusTextBox;
		public System.Windows.Forms.TextBox cPlusTextBox;
		private System.Windows.Forms.GroupBox groupBox5;
		public System.Windows.Forms.CheckBox bFlipCheck;
		public System.Windows.Forms.CheckBox calFlipCheck;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
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
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
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
        public System.Windows.Forms.TextBox southIMonitorTextBox;
        public System.Windows.Forms.TextBox northIMonitorTextBox;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.GroupBox groupBox10;
		private System.Windows.Forms.GroupBox groupBox11;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.Button updateLaserPhotodiodesButton;
		private System.Windows.Forms.Label label32;
		public System.Windows.Forms.TextBox pumpMonitorTextBox;
		public System.Windows.Forms.TextBox pump2MonitorTextBox;
		public System.Windows.Forms.TextBox probeMonitorTextBox;
		public System.Windows.Forms.CheckBox pump2ShutterCheck;
		public System.Windows.Forms.CheckBox pumpShutterCheck;
		private System.Windows.Forms.Label label33;
		public System.Windows.Forms.TextBox yagFlashlampVTextBox;
		private System.Windows.Forms.Button updateFlashlampVButton;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.Button checkYagInterlockButton;
		public System.Windows.Forms.Button yagQDisableButton;
		public System.Windows.Forms.Button yagQEnableButton;
		public System.Windows.Forms.Button stopYagFlashlampsButton;
		public System.Windows.Forms.Button startYAGFlashlampsButton;
		public System.Windows.Forms.TextBox interlockStatusTextBox;
		private System.Windows.Forms.GroupBox groupBox12;
		private System.Windows.Forms.Label label41;
		private System.Windows.Forms.Button scanningBFSButton;
		private System.Windows.Forms.Button scanningBZeroButton;
		private System.Windows.Forms.Button scanningBUpdateButton;
		public System.Windows.Forms.TextBox scanningBVoltageBox;
		private GroupBox groupBox13;
		public CheckBox eFieldAsymmetryCheckBox;
		private Label label37;
		private Label label38;
		public TextBox zeroPlusOneMinusBoostTextBox;
		public TextBox zeroPlusBoostTextBox;
		private GroupBox groupBox14;
		public Button setAttenuatorsButton;
		private Label label36;
		public TextBox rf2AttenuatorVoltageTextBox;
		private Label label39;
		public TextBox rf1AttenuatorVoltageTextBox;
		private GroupBox groupBox16;
		public CheckBox phaseFlip2CheckBox;
		public CheckBox phaseFlip1CheckBox;
        public CheckBox fmSelectCheck;
		public CheckBox rfSwitchEnableCheck;
        private Label label16;
        public TextBox southOffsetIMonitorTextBox;
        private Button calibrateIMonitorButton;
        private Label label35;
        private Label label17;
        public TextBox northOffsetIMonitorTextBox;
        public TextBox IMonitorMeasurementLengthTextBox;
        private Button setIMonitorMeasurementLengthButton;
        public Button setFMVoltagesButton;
        private Label label2;
        public TextBox rf2FMVoltage;
        private Label label3;
        public TextBox rf1FMVoltage;
        public CheckBox attenuatorSelectCheck;
        public Button fieldsOffButton;
        public TextBox rf2FMIncTextBox;
        private Label label24;
        public TextBox rf1FMIncTextBox;
        private Label label28;
        public TextBox rf2AttIncTextBox;
        private Label label6;
        public TextBox rf1AttIncTextBox;
        private Label label4;
        public RadioButton rf1AttZeroRB;
        public RadioButton rf1AttMinusRB;
        public RadioButton rf1AttPlusRB;
        private GroupBox groupBox4;
        private Panel panel1;
        private Panel panel4;
        public RadioButton rf2FMZeroRB;
        public RadioButton rf2FMPlusRB;
        public RadioButton rf2FMMinusRB;
        private Panel panel3;
        public RadioButton rf1FMZeroRB;
        public RadioButton rf1FMPlusRB;
        public RadioButton rf1FMMinusRB;
        private Panel panel2;
        public RadioButton rf2AttZeroRB;
        public RadioButton rf2AttPlusRB;
        public RadioButton rf2AttMinusRB;
        public TextBox rf1StepFreqMon;
        public TextBox rf1CentreFreqMon;
        private Label label40;
        private Label label42;
        public TextBox rf1MinusFreqMon;
        public TextBox rf1PlusFreqMon;
        private Button rfFrequencyUpdateButton;
        private Label label43;
        private Label label44;
        public TextBox rf2StepFreqMon;
        public TextBox rf2CentreFreqMon;
        private Label label48;
        private Label label47;
        public TextBox rf2MinusFreqMon;
        public TextBox rf2PlusFreqMon;
        private Label label46;
        private Label label45;
        public TextBox rf2StepPowerMon;
        public TextBox rf1StepPowerMon;
        public TextBox rf2CentrePowerMon;
        public TextBox rf1CentrePowerMon;
        private Label label56;
        private Label label55;
        private Label label54;
        private Label label53;
        public TextBox rf2MinusPowerMon;
        public TextBox rf1MinusPowerMon;
        public TextBox rf2PlusPowerMon;
        public TextBox rf1PlusPowerMon;
        private Button rfPowerUpdateButton;
        private Label label52;
        private Label label51;
        private Label label50;
        private Label label49;


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

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlWindow));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.fieldsOffButton = new System.Windows.Forms.Button();
            this.switchEButton = new System.Windows.Forms.Button();
            this.eBleedCheck = new System.Windows.Forms.CheckBox();
            this.ePolarityCheck = new System.Windows.Forms.CheckBox();
            this.eOnCheck = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.greenDCFMBox = new System.Windows.Forms.TextBox();
            this.greenOnCheck = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.greenOnAmpBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.greenOnFreqBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.updateFieldButton = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cPlusOffTextBox = new System.Windows.Forms.TextBox();
            this.cMinusOffTextBox = new System.Windows.Forms.TextBox();
            this.cMinusTextBox = new System.Windows.Forms.TextBox();
            this.cPlusTextBox = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.calFlipCheck = new System.Windows.Forms.CheckBox();
            this.bFlipCheck = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.eFieldAsymmetryCheckBox = new System.Windows.Forms.CheckBox();
            this.label37 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.zeroPlusOneMinusBoostTextBox = new System.Windows.Forms.TextBox();
            this.zeroPlusBoostTextBox = new System.Windows.Forms.TextBox();
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
            this.setIMonitorMeasurementLengthButton = new System.Windows.Forms.Button();
            this.IMonitorMeasurementLengthTextBox = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.northOffsetIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.southOffsetIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.calibrateIMonitorButton = new System.Windows.Forms.Button();
            this.southIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.northIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.updateIMonitorButton = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox16 = new System.Windows.Forms.GroupBox();
            this.attenuatorSelectCheck = new System.Windows.Forms.CheckBox();
            this.phaseFlip2CheckBox = new System.Windows.Forms.CheckBox();
            this.phaseFlip1CheckBox = new System.Windows.Forms.CheckBox();
            this.fmSelectCheck = new System.Windows.Forms.CheckBox();
            this.rfSwitchEnableCheck = new System.Windows.Forms.CheckBox();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.rf2FMZeroRB = new System.Windows.Forms.RadioButton();
            this.rf2FMPlusRB = new System.Windows.Forms.RadioButton();
            this.rf2FMMinusRB = new System.Windows.Forms.RadioButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.rf1FMZeroRB = new System.Windows.Forms.RadioButton();
            this.rf1FMPlusRB = new System.Windows.Forms.RadioButton();
            this.rf1FMMinusRB = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rf2AttZeroRB = new System.Windows.Forms.RadioButton();
            this.rf2AttPlusRB = new System.Windows.Forms.RadioButton();
            this.rf2AttMinusRB = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rf1AttZeroRB = new System.Windows.Forms.RadioButton();
            this.rf1AttPlusRB = new System.Windows.Forms.RadioButton();
            this.rf1AttMinusRB = new System.Windows.Forms.RadioButton();
            this.rf2FMIncTextBox = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.rf1FMIncTextBox = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.rf2AttIncTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.rf1AttIncTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.setFMVoltagesButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.rf2FMVoltage = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.rf1FMVoltage = new System.Windows.Forms.TextBox();
            this.setAttenuatorsButton = new System.Windows.Forms.Button();
            this.label36 = new System.Windows.Forms.Label();
            this.rf2AttenuatorVoltageTextBox = new System.Windows.Forms.TextBox();
            this.label39 = new System.Windows.Forms.Label();
            this.rf1AttenuatorVoltageTextBox = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.scanningBVoltageBox = new System.Windows.Forms.TextBox();
            this.scanningBFSButton = new System.Windows.Forms.Button();
            this.scanningBZeroButton = new System.Windows.Forms.Button();
            this.scanningBUpdateButton = new System.Windows.Forms.Button();
            this.label41 = new System.Windows.Forms.Label();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.bCurrentCalStepTextBox = new System.Windows.Forms.TextBox();
            this.bCurrentFlipStepTextBox = new System.Windows.Forms.TextBox();
            this.bCurrentBiasTextBox = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.bCurrent01TextBox = new System.Windows.Forms.TextBox();
            this.bCurrent11TextBox = new System.Windows.Forms.TextBox();
            this.bCurrent10TextBox = new System.Windows.Forms.TextBox();
            this.bCurrent00TextBox = new System.Windows.Forms.TextBox();
            this.updateBCurrentMonitorButton = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.updateLaserPhotodiodesButton = new System.Windows.Forms.Button();
            this.pumpMonitorTextBox = new System.Windows.Forms.TextBox();
            this.pump2MonitorTextBox = new System.Windows.Forms.TextBox();
            this.probeMonitorTextBox = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.label32 = new System.Windows.Forms.Label();
            this.pump2ShutterCheck = new System.Windows.Forms.CheckBox();
            this.pumpShutterCheck = new System.Windows.Forms.CheckBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.checkYagInterlockButton = new System.Windows.Forms.Button();
            this.interlockStatusTextBox = new System.Windows.Forms.TextBox();
            this.label34 = new System.Windows.Forms.Label();
            this.yagQDisableButton = new System.Windows.Forms.Button();
            this.yagQEnableButton = new System.Windows.Forms.Button();
            this.stopYagFlashlampsButton = new System.Windows.Forms.Button();
            this.startYAGFlashlampsButton = new System.Windows.Forms.Button();
            this.updateFlashlampVButton = new System.Windows.Forms.Button();
            this.label33 = new System.Windows.Forms.Label();
            this.yagFlashlampVTextBox = new System.Windows.Forms.TextBox();
            this.rf1MinusFreqMon = new System.Windows.Forms.TextBox();
            this.rf1PlusFreqMon = new System.Windows.Forms.TextBox();
            this.rfFrequencyUpdateButton = new System.Windows.Forms.Button();
            this.label43 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.rf1StepFreqMon = new System.Windows.Forms.TextBox();
            this.rf1CentreFreqMon = new System.Windows.Forms.TextBox();
            this.label40 = new System.Windows.Forms.Label();
            this.label42 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.rf2PlusFreqMon = new System.Windows.Forms.TextBox();
            this.rf2MinusFreqMon = new System.Windows.Forms.TextBox();
            this.label47 = new System.Windows.Forms.Label();
            this.label48 = new System.Windows.Forms.Label();
            this.rf2CentreFreqMon = new System.Windows.Forms.TextBox();
            this.rf2StepFreqMon = new System.Windows.Forms.TextBox();
            this.label49 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label51 = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.rfPowerUpdateButton = new System.Windows.Forms.Button();
            this.rf1PlusPowerMon = new System.Windows.Forms.TextBox();
            this.rf2PlusPowerMon = new System.Windows.Forms.TextBox();
            this.rf1MinusPowerMon = new System.Windows.Forms.TextBox();
            this.rf2MinusPowerMon = new System.Windows.Forms.TextBox();
            this.label53 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.rf1CentrePowerMon = new System.Windows.Forms.TextBox();
            this.rf2CentrePowerMon = new System.Windows.Forms.TextBox();
            this.rf1StepPowerMon = new System.Windows.Forms.TextBox();
            this.rf2StepPowerMon = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox16.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.fieldsOffButton);
            this.groupBox2.Controls.Add(this.switchEButton);
            this.groupBox2.Controls.Add(this.eBleedCheck);
            this.groupBox2.Controls.Add(this.ePolarityCheck);
            this.groupBox2.Controls.Add(this.eOnCheck);
            this.groupBox2.Location = new System.Drawing.Point(216, 230);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(280, 142);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Switch";
            // 
            // fieldsOffButton
            // 
            this.fieldsOffButton.Location = new System.Drawing.Point(19, 103);
            this.fieldsOffButton.Name = "fieldsOffButton";
            this.fieldsOffButton.Size = new System.Drawing.Size(96, 23);
            this.fieldsOffButton.TabIndex = 23;
            this.fieldsOffButton.Text = "Turn Off Fields";
            this.fieldsOffButton.Click += new System.EventHandler(this.fieldsOffButton_Click);
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
            this.ePolarityCheck.Location = new System.Drawing.Point(136, 32);
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
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.greenDCFMBox);
            this.groupBox3.Controls.Add(this.greenOnCheck);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.greenOnAmpBox);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.greenOnFreqBox);
            this.groupBox3.Location = new System.Drawing.Point(8, 16);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(296, 160);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Direct synth control";
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
            this.greenDCFMBox.Text = "0";
            // 
            // greenOnCheck
            // 
            this.greenOnCheck.Location = new System.Drawing.Point(24, 114);
            this.greenOnCheck.Name = "greenOnCheck";
            this.greenOnCheck.Size = new System.Drawing.Size(104, 24);
            this.greenOnCheck.TabIndex = 18;
            this.greenOnCheck.Text = "Green on";
            this.greenOnCheck.CheckedChanged += new System.EventHandler(this.greenOnCheck_CheckedChanged);
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
            this.greenOnAmpBox.Text = "-6";
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
            this.greenOnFreqBox.Text = "170.800";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.updateFieldButton);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cPlusOffTextBox);
            this.groupBox1.Controls.Add(this.cMinusOffTextBox);
            this.groupBox1.Controls.Add(this.cMinusTextBox);
            this.groupBox1.Controls.Add(this.cPlusTextBox);
            this.groupBox1.Location = new System.Drawing.Point(17, 16);
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
            this.updateFieldButton.Size = new System.Drawing.Size(75, 23);
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
            this.label10.Text = "C minus off (V)";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(16, 96);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(80, 23);
            this.label11.TabIndex = 38;
            this.label11.Text = "C plus off (V)";
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
            // cPlusOffTextBox
            // 
            this.cPlusOffTextBox.Location = new System.Drawing.Point(104, 96);
            this.cPlusOffTextBox.Name = "cPlusOffTextBox";
            this.cPlusOffTextBox.Size = new System.Drawing.Size(64, 20);
            this.cPlusOffTextBox.TabIndex = 35;
            this.cPlusOffTextBox.Text = "0";
            // 
            // cMinusOffTextBox
            // 
            this.cMinusOffTextBox.Location = new System.Drawing.Point(104, 128);
            this.cMinusOffTextBox.Name = "cMinusOffTextBox";
            this.cMinusOffTextBox.Size = new System.Drawing.Size(64, 20);
            this.cMinusOffTextBox.TabIndex = 34;
            this.cMinusOffTextBox.Text = "0";
            // 
            // cMinusTextBox
            // 
            this.cMinusTextBox.Location = new System.Drawing.Point(104, 56);
            this.cMinusTextBox.Name = "cMinusTextBox";
            this.cMinusTextBox.Size = new System.Drawing.Size(64, 20);
            this.cMinusTextBox.TabIndex = 33;
            this.cMinusTextBox.Text = "0";
            // 
            // cPlusTextBox
            // 
            this.cPlusTextBox.Location = new System.Drawing.Point(104, 24);
            this.cPlusTextBox.Name = "cPlusTextBox";
            this.cPlusTextBox.Size = new System.Drawing.Size(64, 20);
            this.cPlusTextBox.TabIndex = 32;
            this.cPlusTextBox.Text = "0";
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
            this.bFlipCheck.Size = new System.Drawing.Size(61, 24);
            this.bFlipCheck.TabIndex = 0;
            this.bFlipCheck.Text = "DB";
            this.bFlipCheck.CheckedChanged += new System.EventHandler(this.bFlipCheck_CheckedChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Location = new System.Drawing.Point(12, 25);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(705, 546);
            this.tabControl1.TabIndex = 25;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox13);
            this.tabPage1.Controls.Add(this.groupBox6);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.groupBox7);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(697, 520);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "E-field";
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.eFieldAsymmetryCheckBox);
            this.groupBox13.Controls.Add(this.label37);
            this.groupBox13.Controls.Add(this.label38);
            this.groupBox13.Controls.Add(this.zeroPlusOneMinusBoostTextBox);
            this.groupBox13.Controls.Add(this.zeroPlusBoostTextBox);
            this.groupBox13.Location = new System.Drawing.Point(17, 230);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(184, 112);
            this.groupBox13.TabIndex = 41;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "Supply asymmetry";
            // 
            // eFieldAsymmetryCheckBox
            // 
            this.eFieldAsymmetryCheckBox.Location = new System.Drawing.Point(19, 19);
            this.eFieldAsymmetryCheckBox.Name = "eFieldAsymmetryCheckBox";
            this.eFieldAsymmetryCheckBox.Size = new System.Drawing.Size(72, 24);
            this.eFieldAsymmetryCheckBox.TabIndex = 38;
            this.eFieldAsymmetryCheckBox.Text = "Enable";
            // 
            // label37
            // 
            this.label37.Location = new System.Drawing.Point(16, 83);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(75, 23);
            this.label37.TabIndex = 37;
            this.label37.Text = "0+1- boost (V)";
            // 
            // label38
            // 
            this.label38.Location = new System.Drawing.Point(16, 51);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(80, 23);
            this.label38.TabIndex = 36;
            this.label38.Text = "0+ boost (V)";
            // 
            // zeroPlusOneMinusBoostTextBox
            // 
            this.zeroPlusOneMinusBoostTextBox.Location = new System.Drawing.Point(101, 79);
            this.zeroPlusOneMinusBoostTextBox.Name = "zeroPlusOneMinusBoostTextBox";
            this.zeroPlusOneMinusBoostTextBox.Size = new System.Drawing.Size(64, 20);
            this.zeroPlusOneMinusBoostTextBox.TabIndex = 33;
            this.zeroPlusOneMinusBoostTextBox.Text = "0";
            // 
            // zeroPlusBoostTextBox
            // 
            this.zeroPlusBoostTextBox.Location = new System.Drawing.Point(102, 51);
            this.zeroPlusBoostTextBox.Name = "zeroPlusBoostTextBox";
            this.zeroPlusBoostTextBox.Size = new System.Drawing.Size(64, 20);
            this.zeroPlusBoostTextBox.TabIndex = 32;
            this.zeroPlusBoostTextBox.Text = "0";
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
            this.updateVMonitorButton.Size = new System.Drawing.Size(75, 23);
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
            this.groupBox7.Controls.Add(this.setIMonitorMeasurementLengthButton);
            this.groupBox7.Controls.Add(this.IMonitorMeasurementLengthTextBox);
            this.groupBox7.Controls.Add(this.label35);
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.northOffsetIMonitorTextBox);
            this.groupBox7.Controls.Add(this.label16);
            this.groupBox7.Controls.Add(this.southOffsetIMonitorTextBox);
            this.groupBox7.Controls.Add(this.calibrateIMonitorButton);
            this.groupBox7.Controls.Add(this.southIMonitorTextBox);
            this.groupBox7.Controls.Add(this.northIMonitorTextBox);
            this.groupBox7.Controls.Add(this.updateIMonitorButton);
            this.groupBox7.Controls.Add(this.label18);
            this.groupBox7.Controls.Add(this.label19);
            this.groupBox7.Location = new System.Drawing.Point(502, 16);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(184, 326);
            this.groupBox7.TabIndex = 44;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Current monitors";
            // 
            // setIMonitorMeasurementLengthButton
            // 
            this.setIMonitorMeasurementLengthButton.Location = new System.Drawing.Point(56, 290);
            this.setIMonitorMeasurementLengthButton.Name = "setIMonitorMeasurementLengthButton";
            this.setIMonitorMeasurementLengthButton.Size = new System.Drawing.Size(75, 23);
            this.setIMonitorMeasurementLengthButton.TabIndex = 53;
            this.setIMonitorMeasurementLengthButton.Text = "Set";
            this.setIMonitorMeasurementLengthButton.UseVisualStyleBackColor = true;
            this.setIMonitorMeasurementLengthButton.Click += new System.EventHandler(this.setIMonitorMEasurementLengthButton_Click);
            // 
            // IMonitorMeasurementLengthTextBox
            // 
            this.IMonitorMeasurementLengthTextBox.Location = new System.Drawing.Point(104, 256);
            this.IMonitorMeasurementLengthTextBox.Name = "IMonitorMeasurementLengthTextBox";
            this.IMonitorMeasurementLengthTextBox.Size = new System.Drawing.Size(64, 20);
            this.IMonitorMeasurementLengthTextBox.TabIndex = 52;
            this.IMonitorMeasurementLengthTextBox.Text = "200";
            // 
            // label35
            // 
            this.label35.Location = new System.Drawing.Point(16, 256);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(80, 31);
            this.label35.TabIndex = 51;
            this.label35.Text = "Measurement Length (S)";
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(16, 162);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(80, 23);
            this.label17.TabIndex = 50;
            this.label17.Text = "North offset";
            // 
            // northOffsetIMonitorTextBox
            // 
            this.northOffsetIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.northOffsetIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.northOffsetIMonitorTextBox.Location = new System.Drawing.Point(104, 162);
            this.northOffsetIMonitorTextBox.Name = "northOffsetIMonitorTextBox";
            this.northOffsetIMonitorTextBox.ReadOnly = true;
            this.northOffsetIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.northOffsetIMonitorTextBox.TabIndex = 49;
            this.northOffsetIMonitorTextBox.Text = "0";
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(16, 188);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(80, 23);
            this.label16.TabIndex = 48;
            this.label16.Text = "South offset";
            // 
            // southOffsetIMonitorTextBox
            // 
            this.southOffsetIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.southOffsetIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.southOffsetIMonitorTextBox.Location = new System.Drawing.Point(104, 188);
            this.southOffsetIMonitorTextBox.Name = "southOffsetIMonitorTextBox";
            this.southOffsetIMonitorTextBox.ReadOnly = true;
            this.southOffsetIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.southOffsetIMonitorTextBox.TabIndex = 47;
            this.southOffsetIMonitorTextBox.Text = "0";
            // 
            // calibrateIMonitorButton
            // 
            this.calibrateIMonitorButton.Location = new System.Drawing.Point(56, 214);
            this.calibrateIMonitorButton.Name = "calibrateIMonitorButton";
            this.calibrateIMonitorButton.Size = new System.Drawing.Size(75, 23);
            this.calibrateIMonitorButton.TabIndex = 46;
            this.calibrateIMonitorButton.Text = "Calibrate";
            this.calibrateIMonitorButton.UseVisualStyleBackColor = true;
            this.calibrateIMonitorButton.Click += new System.EventHandler(this.calibrateIMonitorButton_Click);
            // 
            // southIMonitorTextBox
            // 
            this.southIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.southIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.southIMonitorTextBox.Location = new System.Drawing.Point(104, 56);
            this.southIMonitorTextBox.Name = "southIMonitorTextBox";
            this.southIMonitorTextBox.ReadOnly = true;
            this.southIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.southIMonitorTextBox.TabIndex = 45;
            this.southIMonitorTextBox.Text = "0";
            // 
            // northIMonitorTextBox
            // 
            this.northIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.northIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.northIMonitorTextBox.Location = new System.Drawing.Point(104, 24);
            this.northIMonitorTextBox.Name = "northIMonitorTextBox";
            this.northIMonitorTextBox.ReadOnly = true;
            this.northIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.northIMonitorTextBox.TabIndex = 42;
            this.northIMonitorTextBox.Text = "0";
            // 
            // updateIMonitorButton
            // 
            this.updateIMonitorButton.Location = new System.Drawing.Point(56, 82);
            this.updateIMonitorButton.Name = "updateIMonitorButton";
            this.updateIMonitorButton.Size = new System.Drawing.Size(75, 23);
            this.updateIMonitorButton.TabIndex = 40;
            this.updateIMonitorButton.Text = "Update";
            this.updateIMonitorButton.Click += new System.EventHandler(this.updateIMonitorButton_Click);
            // 
            // label18
            // 
            this.label18.Location = new System.Drawing.Point(16, 56);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(80, 23);
            this.label18.TabIndex = 37;
            this.label18.Text = "South C (nA)";
            // 
            // label19
            // 
            this.label19.Location = new System.Drawing.Point(16, 24);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(80, 23);
            this.label19.TabIndex = 36;
            this.label19.Text = "North C (nA)";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Controls.Add(this.groupBox16);
            this.tabPage2.Controls.Add(this.groupBox14);
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(697, 520);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Synths";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rf2StepPowerMon);
            this.groupBox4.Controls.Add(this.rf2StepFreqMon);
            this.groupBox4.Controls.Add(this.rf1StepPowerMon);
            this.groupBox4.Controls.Add(this.rf1StepFreqMon);
            this.groupBox4.Controls.Add(this.rf2CentrePowerMon);
            this.groupBox4.Controls.Add(this.rf2CentreFreqMon);
            this.groupBox4.Controls.Add(this.rf1CentrePowerMon);
            this.groupBox4.Controls.Add(this.rf1CentreFreqMon);
            this.groupBox4.Controls.Add(this.label56);
            this.groupBox4.Controls.Add(this.label48);
            this.groupBox4.Controls.Add(this.label55);
            this.groupBox4.Controls.Add(this.label40);
            this.groupBox4.Controls.Add(this.label54);
            this.groupBox4.Controls.Add(this.label47);
            this.groupBox4.Controls.Add(this.label53);
            this.groupBox4.Controls.Add(this.label42);
            this.groupBox4.Controls.Add(this.rf2MinusPowerMon);
            this.groupBox4.Controls.Add(this.rf2MinusFreqMon);
            this.groupBox4.Controls.Add(this.rf1MinusPowerMon);
            this.groupBox4.Controls.Add(this.rf1MinusFreqMon);
            this.groupBox4.Controls.Add(this.rf2PlusPowerMon);
            this.groupBox4.Controls.Add(this.rf2PlusFreqMon);
            this.groupBox4.Controls.Add(this.rf1PlusPowerMon);
            this.groupBox4.Controls.Add(this.rf1PlusFreqMon);
            this.groupBox4.Controls.Add(this.rfPowerUpdateButton);
            this.groupBox4.Controls.Add(this.label52);
            this.groupBox4.Controls.Add(this.rfFrequencyUpdateButton);
            this.groupBox4.Controls.Add(this.label51);
            this.groupBox4.Controls.Add(this.label46);
            this.groupBox4.Controls.Add(this.label50);
            this.groupBox4.Controls.Add(this.label43);
            this.groupBox4.Controls.Add(this.label49);
            this.groupBox4.Controls.Add(this.label45);
            this.groupBox4.Controls.Add(this.label44);
            this.groupBox4.Location = new System.Drawing.Point(8, 373);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(675, 138);
            this.groupBox4.TabIndex = 27;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "rf measurement";
            // 
            // groupBox16
            // 
            this.groupBox16.Controls.Add(this.attenuatorSelectCheck);
            this.groupBox16.Controls.Add(this.phaseFlip2CheckBox);
            this.groupBox16.Controls.Add(this.phaseFlip1CheckBox);
            this.groupBox16.Controls.Add(this.fmSelectCheck);
            this.groupBox16.Controls.Add(this.rfSwitchEnableCheck);
            this.groupBox16.Location = new System.Drawing.Point(8, 184);
            this.groupBox16.Name = "groupBox16";
            this.groupBox16.Size = new System.Drawing.Size(296, 183);
            this.groupBox16.TabIndex = 26;
            this.groupBox16.TabStop = false;
            this.groupBox16.Text = "TTL controls";
            // 
            // attenuatorSelectCheck
            // 
            this.attenuatorSelectCheck.Location = new System.Drawing.Point(24, 82);
            this.attenuatorSelectCheck.Name = "attenuatorSelectCheck";
            this.attenuatorSelectCheck.Size = new System.Drawing.Size(208, 24);
            this.attenuatorSelectCheck.TabIndex = 30;
            this.attenuatorSelectCheck.Text = "Attenuator select (check rf1)";
            this.attenuatorSelectCheck.CheckedChanged += new System.EventHandler(this.attenuatorSelectCheck_CheckedChanged);
            // 
            // phaseFlip2CheckBox
            // 
            this.phaseFlip2CheckBox.Location = new System.Drawing.Point(24, 142);
            this.phaseFlip2CheckBox.Name = "phaseFlip2CheckBox";
            this.phaseFlip2CheckBox.Size = new System.Drawing.Size(152, 24);
            this.phaseFlip2CheckBox.TabIndex = 29;
            this.phaseFlip2CheckBox.Text = "phase flip TTL 2";
            this.phaseFlip2CheckBox.CheckedChanged += new System.EventHandler(this.phaseFlip2CheckBox_CheckedChanged);
            // 
            // phaseFlip1CheckBox
            // 
            this.phaseFlip1CheckBox.Location = new System.Drawing.Point(24, 112);
            this.phaseFlip1CheckBox.Name = "phaseFlip1CheckBox";
            this.phaseFlip1CheckBox.Size = new System.Drawing.Size(152, 24);
            this.phaseFlip1CheckBox.TabIndex = 28;
            this.phaseFlip1CheckBox.Text = "phase flip TTL 1";
            this.phaseFlip1CheckBox.CheckedChanged += new System.EventHandler(this.phaseFlip1CheckBox_CheckedChanged);
            // 
            // fmSelectCheck
            // 
            this.fmSelectCheck.Location = new System.Drawing.Point(24, 52);
            this.fmSelectCheck.Name = "fmSelectCheck";
            this.fmSelectCheck.Size = new System.Drawing.Size(208, 24);
            this.fmSelectCheck.TabIndex = 27;
            this.fmSelectCheck.Text = "DC FM select (check rf1)";
            this.fmSelectCheck.CheckedChanged += new System.EventHandler(this.greenFMSelectCheck_CheckedChanged);
            // 
            // rfSwitchEnableCheck
            // 
            this.rfSwitchEnableCheck.Location = new System.Drawing.Point(24, 22);
            this.rfSwitchEnableCheck.Name = "rfSwitchEnableCheck";
            this.rfSwitchEnableCheck.Size = new System.Drawing.Size(208, 24);
            this.rfSwitchEnableCheck.TabIndex = 22;
            this.rfSwitchEnableCheck.Text = "Enable rf1 switch (check on)";
            this.rfSwitchEnableCheck.CheckedChanged += new System.EventHandler(this.rfSwitchEnableCheck_CheckedChanged);
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.panel4);
            this.groupBox14.Controls.Add(this.panel3);
            this.groupBox14.Controls.Add(this.panel2);
            this.groupBox14.Controls.Add(this.panel1);
            this.groupBox14.Controls.Add(this.rf2FMIncTextBox);
            this.groupBox14.Controls.Add(this.label24);
            this.groupBox14.Controls.Add(this.rf1FMIncTextBox);
            this.groupBox14.Controls.Add(this.label28);
            this.groupBox14.Controls.Add(this.rf2AttIncTextBox);
            this.groupBox14.Controls.Add(this.label6);
            this.groupBox14.Controls.Add(this.rf1AttIncTextBox);
            this.groupBox14.Controls.Add(this.label4);
            this.groupBox14.Controls.Add(this.setFMVoltagesButton);
            this.groupBox14.Controls.Add(this.label2);
            this.groupBox14.Controls.Add(this.rf2FMVoltage);
            this.groupBox14.Controls.Add(this.label3);
            this.groupBox14.Controls.Add(this.rf1FMVoltage);
            this.groupBox14.Controls.Add(this.setAttenuatorsButton);
            this.groupBox14.Controls.Add(this.label36);
            this.groupBox14.Controls.Add(this.rf2AttenuatorVoltageTextBox);
            this.groupBox14.Controls.Add(this.label39);
            this.groupBox14.Controls.Add(this.rf1AttenuatorVoltageTextBox);
            this.groupBox14.Location = new System.Drawing.Point(320, 16);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(363, 244);
            this.groupBox14.TabIndex = 24;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "fast rf control";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.rf2FMZeroRB);
            this.panel4.Controls.Add(this.rf2FMPlusRB);
            this.panel4.Controls.Add(this.rf2FMMinusRB);
            this.panel4.Location = new System.Drawing.Point(249, 157);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(108, 32);
            this.panel4.TabIndex = 0;
            // 
            // rf2FMZeroRB
            // 
            this.rf2FMZeroRB.AutoSize = true;
            this.rf2FMZeroRB.Checked = true;
            this.rf2FMZeroRB.Location = new System.Drawing.Point(77, 7);
            this.rf2FMZeroRB.Name = "rf2FMZeroRB";
            this.rf2FMZeroRB.Size = new System.Drawing.Size(31, 17);
            this.rf2FMZeroRB.TabIndex = 32;
            this.rf2FMZeroRB.TabStop = true;
            this.rf2FMZeroRB.Text = "0";
            this.rf2FMZeroRB.UseVisualStyleBackColor = true;
            // 
            // rf2FMPlusRB
            // 
            this.rf2FMPlusRB.AutoSize = true;
            this.rf2FMPlusRB.Location = new System.Drawing.Point(3, 6);
            this.rf2FMPlusRB.Name = "rf2FMPlusRB";
            this.rf2FMPlusRB.Size = new System.Drawing.Size(31, 17);
            this.rf2FMPlusRB.TabIndex = 32;
            this.rf2FMPlusRB.Text = "+";
            this.rf2FMPlusRB.UseVisualStyleBackColor = true;
            // 
            // rf2FMMinusRB
            // 
            this.rf2FMMinusRB.AutoSize = true;
            this.rf2FMMinusRB.Location = new System.Drawing.Point(42, 7);
            this.rf2FMMinusRB.Name = "rf2FMMinusRB";
            this.rf2FMMinusRB.Size = new System.Drawing.Size(28, 17);
            this.rf2FMMinusRB.TabIndex = 32;
            this.rf2FMMinusRB.Text = "-";
            this.rf2FMMinusRB.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.rf1FMZeroRB);
            this.panel3.Controls.Add(this.rf1FMPlusRB);
            this.panel3.Controls.Add(this.rf1FMMinusRB);
            this.panel3.Location = new System.Drawing.Point(249, 129);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(108, 32);
            this.panel3.TabIndex = 0;
            // 
            // rf1FMZeroRB
            // 
            this.rf1FMZeroRB.AutoSize = true;
            this.rf1FMZeroRB.Checked = true;
            this.rf1FMZeroRB.Location = new System.Drawing.Point(77, 7);
            this.rf1FMZeroRB.Name = "rf1FMZeroRB";
            this.rf1FMZeroRB.Size = new System.Drawing.Size(31, 17);
            this.rf1FMZeroRB.TabIndex = 32;
            this.rf1FMZeroRB.TabStop = true;
            this.rf1FMZeroRB.Text = "0";
            this.rf1FMZeroRB.UseVisualStyleBackColor = true;
            // 
            // rf1FMPlusRB
            // 
            this.rf1FMPlusRB.AutoSize = true;
            this.rf1FMPlusRB.Location = new System.Drawing.Point(3, 6);
            this.rf1FMPlusRB.Name = "rf1FMPlusRB";
            this.rf1FMPlusRB.Size = new System.Drawing.Size(31, 17);
            this.rf1FMPlusRB.TabIndex = 32;
            this.rf1FMPlusRB.Text = "+";
            this.rf1FMPlusRB.UseVisualStyleBackColor = true;
            // 
            // rf1FMMinusRB
            // 
            this.rf1FMMinusRB.AutoSize = true;
            this.rf1FMMinusRB.Location = new System.Drawing.Point(42, 7);
            this.rf1FMMinusRB.Name = "rf1FMMinusRB";
            this.rf1FMMinusRB.Size = new System.Drawing.Size(28, 17);
            this.rf1FMMinusRB.TabIndex = 32;
            this.rf1FMMinusRB.Text = "-";
            this.rf1FMMinusRB.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rf2AttZeroRB);
            this.panel2.Controls.Add(this.rf2AttPlusRB);
            this.panel2.Controls.Add(this.rf2AttMinusRB);
            this.panel2.Location = new System.Drawing.Point(249, 47);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(108, 32);
            this.panel2.TabIndex = 0;
            // 
            // rf2AttZeroRB
            // 
            this.rf2AttZeroRB.AutoSize = true;
            this.rf2AttZeroRB.Checked = true;
            this.rf2AttZeroRB.Location = new System.Drawing.Point(77, 7);
            this.rf2AttZeroRB.Name = "rf2AttZeroRB";
            this.rf2AttZeroRB.Size = new System.Drawing.Size(31, 17);
            this.rf2AttZeroRB.TabIndex = 32;
            this.rf2AttZeroRB.TabStop = true;
            this.rf2AttZeroRB.Text = "0";
            this.rf2AttZeroRB.UseVisualStyleBackColor = true;
            // 
            // rf2AttPlusRB
            // 
            this.rf2AttPlusRB.AutoSize = true;
            this.rf2AttPlusRB.Location = new System.Drawing.Point(3, 6);
            this.rf2AttPlusRB.Name = "rf2AttPlusRB";
            this.rf2AttPlusRB.Size = new System.Drawing.Size(31, 17);
            this.rf2AttPlusRB.TabIndex = 32;
            this.rf2AttPlusRB.Text = "+";
            this.rf2AttPlusRB.UseVisualStyleBackColor = true;
            // 
            // rf2AttMinusRB
            // 
            this.rf2AttMinusRB.AutoSize = true;
            this.rf2AttMinusRB.Location = new System.Drawing.Point(42, 7);
            this.rf2AttMinusRB.Name = "rf2AttMinusRB";
            this.rf2AttMinusRB.Size = new System.Drawing.Size(28, 17);
            this.rf2AttMinusRB.TabIndex = 32;
            this.rf2AttMinusRB.Text = "-";
            this.rf2AttMinusRB.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rf1AttZeroRB);
            this.panel1.Controls.Add(this.rf1AttPlusRB);
            this.panel1.Controls.Add(this.rf1AttMinusRB);
            this.panel1.Location = new System.Drawing.Point(249, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(108, 32);
            this.panel1.TabIndex = 0;
            // 
            // rf1AttZeroRB
            // 
            this.rf1AttZeroRB.AutoSize = true;
            this.rf1AttZeroRB.Checked = true;
            this.rf1AttZeroRB.Location = new System.Drawing.Point(77, 7);
            this.rf1AttZeroRB.Name = "rf1AttZeroRB";
            this.rf1AttZeroRB.Size = new System.Drawing.Size(31, 17);
            this.rf1AttZeroRB.TabIndex = 32;
            this.rf1AttZeroRB.TabStop = true;
            this.rf1AttZeroRB.Text = "0";
            this.rf1AttZeroRB.UseVisualStyleBackColor = true;
            // 
            // rf1AttPlusRB
            // 
            this.rf1AttPlusRB.AutoSize = true;
            this.rf1AttPlusRB.Location = new System.Drawing.Point(3, 6);
            this.rf1AttPlusRB.Name = "rf1AttPlusRB";
            this.rf1AttPlusRB.Size = new System.Drawing.Size(31, 17);
            this.rf1AttPlusRB.TabIndex = 32;
            this.rf1AttPlusRB.Text = "+";
            this.rf1AttPlusRB.UseVisualStyleBackColor = true;
            // 
            // rf1AttMinusRB
            // 
            this.rf1AttMinusRB.AutoSize = true;
            this.rf1AttMinusRB.Location = new System.Drawing.Point(42, 7);
            this.rf1AttMinusRB.Name = "rf1AttMinusRB";
            this.rf1AttMinusRB.Size = new System.Drawing.Size(28, 17);
            this.rf1AttMinusRB.TabIndex = 32;
            this.rf1AttMinusRB.Text = "-";
            this.rf1AttMinusRB.UseVisualStyleBackColor = true;
            // 
            // rf2FMIncTextBox
            // 
            this.rf2FMIncTextBox.Location = new System.Drawing.Point(198, 159);
            this.rf2FMIncTextBox.Name = "rf2FMIncTextBox";
            this.rf2FMIncTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf2FMIncTextBox.TabIndex = 31;
            this.rf2FMIncTextBox.Text = "0";
            // 
            // label24
            // 
            this.label24.Location = new System.Drawing.Point(168, 159);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(24, 23);
            this.label24.TabIndex = 30;
            this.label24.Text = "+-";
            // 
            // rf1FMIncTextBox
            // 
            this.rf1FMIncTextBox.Location = new System.Drawing.Point(198, 133);
            this.rf1FMIncTextBox.Name = "rf1FMIncTextBox";
            this.rf1FMIncTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf1FMIncTextBox.TabIndex = 29;
            this.rf1FMIncTextBox.Text = "0";
            // 
            // label28
            // 
            this.label28.Location = new System.Drawing.Point(168, 133);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(24, 23);
            this.label28.TabIndex = 28;
            this.label28.Text = "+-";
            // 
            // rf2AttIncTextBox
            // 
            this.rf2AttIncTextBox.Location = new System.Drawing.Point(198, 50);
            this.rf2AttIncTextBox.Name = "rf2AttIncTextBox";
            this.rf2AttIncTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf2AttIncTextBox.TabIndex = 27;
            this.rf2AttIncTextBox.Text = "0";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(168, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 23);
            this.label6.TabIndex = 26;
            this.label6.Text = "+-";
            // 
            // rf1AttIncTextBox
            // 
            this.rf1AttIncTextBox.Location = new System.Drawing.Point(198, 24);
            this.rf1AttIncTextBox.Name = "rf1AttIncTextBox";
            this.rf1AttIncTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf1AttIncTextBox.TabIndex = 25;
            this.rf1AttIncTextBox.Text = "0";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(168, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(24, 23);
            this.label4.TabIndex = 24;
            this.label4.Text = "+-";
            // 
            // setFMVoltagesButton
            // 
            this.setFMVoltagesButton.Location = new System.Drawing.Point(125, 201);
            this.setFMVoltagesButton.Name = "setFMVoltagesButton";
            this.setFMVoltagesButton.Size = new System.Drawing.Size(131, 23);
            this.setFMVoltagesButton.TabIndex = 23;
            this.setFMVoltagesButton.Text = "Set fm voltages";
            this.setFMVoltagesButton.Click += new System.EventHandler(this.setFMVoltagesButton_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(59, 159);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 23);
            this.label2.TabIndex = 22;
            this.label2.Text = "rf2 fm (V)";
            // 
            // rf2FMVoltage
            // 
            this.rf2FMVoltage.Location = new System.Drawing.Point(128, 159);
            this.rf2FMVoltage.Name = "rf2FMVoltage";
            this.rf2FMVoltage.Size = new System.Drawing.Size(34, 20);
            this.rf2FMVoltage.TabIndex = 21;
            this.rf2FMVoltage.Text = "0";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(59, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 23);
            this.label3.TabIndex = 20;
            this.label3.Text = "rf1 fm (V)";
            // 
            // rf1FMVoltage
            // 
            this.rf1FMVoltage.Location = new System.Drawing.Point(128, 133);
            this.rf1FMVoltage.Name = "rf1FMVoltage";
            this.rf1FMVoltage.Size = new System.Drawing.Size(34, 20);
            this.rf1FMVoltage.TabIndex = 19;
            this.rf1FMVoltage.Text = "0";
            // 
            // setAttenuatorsButton
            // 
            this.setAttenuatorsButton.Location = new System.Drawing.Point(125, 83);
            this.setAttenuatorsButton.Name = "setAttenuatorsButton";
            this.setAttenuatorsButton.Size = new System.Drawing.Size(131, 23);
            this.setAttenuatorsButton.TabIndex = 18;
            this.setAttenuatorsButton.Text = "Set attenuator voltages";
            this.setAttenuatorsButton.Click += new System.EventHandler(this.setAttenuatorsButton_Click);
            // 
            // label36
            // 
            this.label36.Location = new System.Drawing.Point(24, 50);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(96, 23);
            this.label36.TabIndex = 15;
            this.label36.Text = "rf2 att. voltage (V)";
            // 
            // rf2AttenuatorVoltageTextBox
            // 
            this.rf2AttenuatorVoltageTextBox.Location = new System.Drawing.Point(128, 50);
            this.rf2AttenuatorVoltageTextBox.Name = "rf2AttenuatorVoltageTextBox";
            this.rf2AttenuatorVoltageTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf2AttenuatorVoltageTextBox.TabIndex = 14;
            this.rf2AttenuatorVoltageTextBox.Text = "5";
            // 
            // label39
            // 
            this.label39.Location = new System.Drawing.Point(24, 24);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(96, 23);
            this.label39.TabIndex = 13;
            this.label39.Text = "rf1 att. voltage (V)";
            // 
            // rf1AttenuatorVoltageTextBox
            // 
            this.rf1AttenuatorVoltageTextBox.Location = new System.Drawing.Point(128, 24);
            this.rf1AttenuatorVoltageTextBox.Name = "rf1AttenuatorVoltageTextBox";
            this.rf1AttenuatorVoltageTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf1AttenuatorVoltageTextBox.TabIndex = 12;
            this.rf1AttenuatorVoltageTextBox.Text = "5";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox12);
            this.tabPage3.Controls.Add(this.groupBox8);
            this.tabPage3.Controls.Add(this.groupBox5);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(697, 520);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "B-field";
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.scanningBVoltageBox);
            this.groupBox12.Controls.Add(this.scanningBFSButton);
            this.groupBox12.Controls.Add(this.scanningBZeroButton);
            this.groupBox12.Controls.Add(this.scanningBUpdateButton);
            this.groupBox12.Controls.Add(this.label41);
            this.groupBox12.Location = new System.Drawing.Point(304, 16);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(296, 96);
            this.groupBox12.TabIndex = 46;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Scanning B box";
            // 
            // scanningBVoltageBox
            // 
            this.scanningBVoltageBox.Location = new System.Drawing.Point(96, 24);
            this.scanningBVoltageBox.Name = "scanningBVoltageBox";
            this.scanningBVoltageBox.Size = new System.Drawing.Size(64, 20);
            this.scanningBVoltageBox.TabIndex = 45;
            this.scanningBVoltageBox.Text = "0";
            // 
            // scanningBFSButton
            // 
            this.scanningBFSButton.Location = new System.Drawing.Point(152, 56);
            this.scanningBFSButton.Name = "scanningBFSButton";
            this.scanningBFSButton.Size = new System.Drawing.Size(75, 23);
            this.scanningBFSButton.TabIndex = 44;
            this.scanningBFSButton.Text = "Max";
            this.scanningBFSButton.Click += new System.EventHandler(this.scanningBFSButton_Click);
            // 
            // scanningBZeroButton
            // 
            this.scanningBZeroButton.Location = new System.Drawing.Point(64, 56);
            this.scanningBZeroButton.Name = "scanningBZeroButton";
            this.scanningBZeroButton.Size = new System.Drawing.Size(75, 23);
            this.scanningBZeroButton.TabIndex = 43;
            this.scanningBZeroButton.Text = "Zero";
            this.scanningBZeroButton.Click += new System.EventHandler(this.scanningBZeroButton_Click);
            // 
            // scanningBUpdateButton
            // 
            this.scanningBUpdateButton.Location = new System.Drawing.Point(184, 24);
            this.scanningBUpdateButton.Name = "scanningBUpdateButton";
            this.scanningBUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.scanningBUpdateButton.TabIndex = 40;
            this.scanningBUpdateButton.Text = "Update";
            this.scanningBUpdateButton.Click += new System.EventHandler(this.scanningBUpdateButton_Click);
            // 
            // label41
            // 
            this.label41.Location = new System.Drawing.Point(16, 24);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(80, 23);
            this.label41.TabIndex = 36;
            this.label41.Text = "Voltage (V)";
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
            this.groupBox8.Location = new System.Drawing.Point(8, 120);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(376, 192);
            this.groupBox8.TabIndex = 45;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Current monitor";
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
            this.updateBCurrentMonitorButton.Size = new System.Drawing.Size(75, 23);
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
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.groupBox11);
            this.tabPage4.Controls.Add(this.groupBox10);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(697, 520);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Laser";
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.updateLaserPhotodiodesButton);
            this.groupBox11.Controls.Add(this.pumpMonitorTextBox);
            this.groupBox11.Controls.Add(this.pump2MonitorTextBox);
            this.groupBox11.Controls.Add(this.probeMonitorTextBox);
            this.groupBox11.Controls.Add(this.label29);
            this.groupBox11.Controls.Add(this.label30);
            this.groupBox11.Controls.Add(this.label31);
            this.groupBox11.Location = new System.Drawing.Point(8, 136);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(224, 176);
            this.groupBox11.TabIndex = 1;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Photodiodes";
            // 
            // updateLaserPhotodiodesButton
            // 
            this.updateLaserPhotodiodesButton.Location = new System.Drawing.Point(72, 136);
            this.updateLaserPhotodiodesButton.Name = "updateLaserPhotodiodesButton";
            this.updateLaserPhotodiodesButton.Size = new System.Drawing.Size(75, 23);
            this.updateLaserPhotodiodesButton.TabIndex = 52;
            this.updateLaserPhotodiodesButton.Text = "Update";
            this.updateLaserPhotodiodesButton.Click += new System.EventHandler(this.updateLaserPhotodiodesButton_Click);
            // 
            // pumpMonitorTextBox
            // 
            this.pumpMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.pumpMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.pumpMonitorTextBox.Location = new System.Drawing.Point(120, 64);
            this.pumpMonitorTextBox.Name = "pumpMonitorTextBox";
            this.pumpMonitorTextBox.ReadOnly = true;
            this.pumpMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.pumpMonitorTextBox.TabIndex = 51;
            this.pumpMonitorTextBox.Text = "0";
            // 
            // pump2MonitorTextBox
            // 
            this.pump2MonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.pump2MonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.pump2MonitorTextBox.Location = new System.Drawing.Point(120, 96);
            this.pump2MonitorTextBox.Name = "pump2MonitorTextBox";
            this.pump2MonitorTextBox.ReadOnly = true;
            this.pump2MonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.pump2MonitorTextBox.TabIndex = 50;
            this.pump2MonitorTextBox.Text = "0";
            // 
            // probeMonitorTextBox
            // 
            this.probeMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.probeMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.probeMonitorTextBox.Location = new System.Drawing.Point(120, 32);
            this.probeMonitorTextBox.Name = "probeMonitorTextBox";
            this.probeMonitorTextBox.ReadOnly = true;
            this.probeMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.probeMonitorTextBox.TabIndex = 49;
            this.probeMonitorTextBox.Text = "0";
            // 
            // label29
            // 
            this.label29.Location = new System.Drawing.Point(32, 96);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(80, 23);
            this.label29.TabIndex = 48;
            this.label29.Text = "Pump 2 (V)";
            // 
            // label30
            // 
            this.label30.Location = new System.Drawing.Point(32, 64);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(80, 23);
            this.label30.TabIndex = 47;
            this.label30.Text = "Pump (V)";
            // 
            // label31
            // 
            this.label31.Location = new System.Drawing.Point(32, 32);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(80, 23);
            this.label31.TabIndex = 46;
            this.label31.Text = "Probe (V)";
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.label32);
            this.groupBox10.Controls.Add(this.pump2ShutterCheck);
            this.groupBox10.Controls.Add(this.pumpShutterCheck);
            this.groupBox10.Location = new System.Drawing.Point(8, 16);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(224, 100);
            this.groupBox10.TabIndex = 0;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Shutters";
            // 
            // label32
            // 
            this.label32.Location = new System.Drawing.Point(104, 40);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(100, 32);
            this.label32.TabIndex = 24;
            this.label32.Text = "False is blocked. True is open.";
            // 
            // pump2ShutterCheck
            // 
            this.pump2ShutterCheck.Location = new System.Drawing.Point(24, 56);
            this.pump2ShutterCheck.Name = "pump2ShutterCheck";
            this.pump2ShutterCheck.Size = new System.Drawing.Size(72, 24);
            this.pump2ShutterCheck.TabIndex = 23;
            this.pump2ShutterCheck.Text = "Pump 2";
            this.pump2ShutterCheck.CheckedChanged += new System.EventHandler(this.pump2ShutterCheck_CheckedChanged);
            // 
            // pumpShutterCheck
            // 
            this.pumpShutterCheck.Location = new System.Drawing.Point(24, 24);
            this.pumpShutterCheck.Name = "pumpShutterCheck";
            this.pumpShutterCheck.Size = new System.Drawing.Size(72, 24);
            this.pumpShutterCheck.TabIndex = 22;
            this.pumpShutterCheck.Text = "Pump";
            this.pumpShutterCheck.CheckedChanged += new System.EventHandler(this.pumpShutterCheck_CheckedChanged);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.checkYagInterlockButton);
            this.tabPage5.Controls.Add(this.interlockStatusTextBox);
            this.tabPage5.Controls.Add(this.label34);
            this.tabPage5.Controls.Add(this.yagQDisableButton);
            this.tabPage5.Controls.Add(this.yagQEnableButton);
            this.tabPage5.Controls.Add(this.stopYagFlashlampsButton);
            this.tabPage5.Controls.Add(this.startYAGFlashlampsButton);
            this.tabPage5.Controls.Add(this.updateFlashlampVButton);
            this.tabPage5.Controls.Add(this.label33);
            this.tabPage5.Controls.Add(this.yagFlashlampVTextBox);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(697, 520);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "YAG";
            // 
            // checkYagInterlockButton
            // 
            this.checkYagInterlockButton.Location = new System.Drawing.Point(264, 64);
            this.checkYagInterlockButton.Name = "checkYagInterlockButton";
            this.checkYagInterlockButton.Size = new System.Drawing.Size(75, 23);
            this.checkYagInterlockButton.TabIndex = 45;
            this.checkYagInterlockButton.Text = "Check";
            this.checkYagInterlockButton.Click += new System.EventHandler(this.checkYagInterlockButton_Click);
            // 
            // interlockStatusTextBox
            // 
            this.interlockStatusTextBox.BackColor = System.Drawing.Color.Black;
            this.interlockStatusTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.interlockStatusTextBox.Location = new System.Drawing.Point(168, 64);
            this.interlockStatusTextBox.Name = "interlockStatusTextBox";
            this.interlockStatusTextBox.ReadOnly = true;
            this.interlockStatusTextBox.Size = new System.Drawing.Size(64, 20);
            this.interlockStatusTextBox.TabIndex = 44;
            this.interlockStatusTextBox.Text = "0";
            // 
            // label34
            // 
            this.label34.Location = new System.Drawing.Point(24, 64);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(104, 23);
            this.label34.TabIndex = 43;
            this.label34.Text = "Interlock failed";
            // 
            // yagQDisableButton
            // 
            this.yagQDisableButton.Enabled = false;
            this.yagQDisableButton.Location = new System.Drawing.Point(408, 104);
            this.yagQDisableButton.Name = "yagQDisableButton";
            this.yagQDisableButton.Size = new System.Drawing.Size(112, 23);
            this.yagQDisableButton.TabIndex = 18;
            this.yagQDisableButton.Text = "Q-switch Disable";
            this.yagQDisableButton.Click += new System.EventHandler(this.yagQDisableButton_Click);
            // 
            // yagQEnableButton
            // 
            this.yagQEnableButton.Location = new System.Drawing.Point(280, 104);
            this.yagQEnableButton.Name = "yagQEnableButton";
            this.yagQEnableButton.Size = new System.Drawing.Size(112, 23);
            this.yagQEnableButton.TabIndex = 17;
            this.yagQEnableButton.Text = "Q-switch Enable";
            this.yagQEnableButton.Click += new System.EventHandler(this.yagQEnableButton_Click);
            // 
            // stopYagFlashlampsButton
            // 
            this.stopYagFlashlampsButton.Enabled = false;
            this.stopYagFlashlampsButton.Location = new System.Drawing.Point(152, 104);
            this.stopYagFlashlampsButton.Name = "stopYagFlashlampsButton";
            this.stopYagFlashlampsButton.Size = new System.Drawing.Size(112, 23);
            this.stopYagFlashlampsButton.TabIndex = 16;
            this.stopYagFlashlampsButton.Text = "Stop Flashlamps";
            this.stopYagFlashlampsButton.Click += new System.EventHandler(this.stopYagFlashlampsButton_Click);
            // 
            // startYAGFlashlampsButton
            // 
            this.startYAGFlashlampsButton.Location = new System.Drawing.Point(24, 104);
            this.startYAGFlashlampsButton.Name = "startYAGFlashlampsButton";
            this.startYAGFlashlampsButton.Size = new System.Drawing.Size(112, 23);
            this.startYAGFlashlampsButton.TabIndex = 15;
            this.startYAGFlashlampsButton.Text = "Start Flashlamps";
            this.startYAGFlashlampsButton.Click += new System.EventHandler(this.startYAGFlashlampsButton_Click);
            // 
            // updateFlashlampVButton
            // 
            this.updateFlashlampVButton.Location = new System.Drawing.Point(264, 32);
            this.updateFlashlampVButton.Name = "updateFlashlampVButton";
            this.updateFlashlampVButton.Size = new System.Drawing.Size(75, 23);
            this.updateFlashlampVButton.TabIndex = 14;
            this.updateFlashlampVButton.Text = "Update V";
            this.updateFlashlampVButton.Click += new System.EventHandler(this.updateFlashlampVButton_Click);
            // 
            // label33
            // 
            this.label33.Location = new System.Drawing.Point(24, 32);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(144, 23);
            this.label33.TabIndex = 13;
            this.label33.Text = "Flashlamp voltage (V)";
            // 
            // yagFlashlampVTextBox
            // 
            this.yagFlashlampVTextBox.Location = new System.Drawing.Point(168, 32);
            this.yagFlashlampVTextBox.Name = "yagFlashlampVTextBox";
            this.yagFlashlampVTextBox.Size = new System.Drawing.Size(64, 20);
            this.yagFlashlampVTextBox.TabIndex = 12;
            this.yagFlashlampVTextBox.Text = "1220";
            // 
            // rf1MinusFreqMon
            // 
            this.rf1MinusFreqMon.BackColor = System.Drawing.Color.Black;
            this.rf1MinusFreqMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf1MinusFreqMon.Location = new System.Drawing.Point(98, 44);
            this.rf1MinusFreqMon.Name = "rf1MinusFreqMon";
            this.rf1MinusFreqMon.ReadOnly = true;
            this.rf1MinusFreqMon.Size = new System.Drawing.Size(64, 20);
            this.rf1MinusFreqMon.TabIndex = 54;
            this.rf1MinusFreqMon.Text = "0";
            // 
            // rf1PlusFreqMon
            // 
            this.rf1PlusFreqMon.BackColor = System.Drawing.Color.Black;
            this.rf1PlusFreqMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf1PlusFreqMon.Location = new System.Drawing.Point(27, 44);
            this.rf1PlusFreqMon.Name = "rf1PlusFreqMon";
            this.rf1PlusFreqMon.ReadOnly = true;
            this.rf1PlusFreqMon.Size = new System.Drawing.Size(64, 20);
            this.rf1PlusFreqMon.TabIndex = 51;
            this.rf1PlusFreqMon.Text = "0";
            // 
            // rfFrequencyUpdateButton
            // 
            this.rfFrequencyUpdateButton.Location = new System.Drawing.Point(139, 107);
            this.rfFrequencyUpdateButton.Name = "rfFrequencyUpdateButton";
            this.rfFrequencyUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.rfFrequencyUpdateButton.TabIndex = 50;
            this.rfFrequencyUpdateButton.Text = "Update";
            this.rfFrequencyUpdateButton.Click += new System.EventHandler(this.rfFrequencyUpdateButton_Click);
            // 
            // label43
            // 
            this.label43.Location = new System.Drawing.Point(95, 25);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(80, 23);
            this.label43.TabIndex = 47;
            this.label43.Text = "rf1 fr - (MHZ)";
            // 
            // label44
            // 
            this.label44.Location = new System.Drawing.Point(24, 25);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(80, 23);
            this.label44.TabIndex = 46;
            this.label44.Text = "rf1 fr + (MHZ)";
            // 
            // rf1StepFreqMon
            // 
            this.rf1StepFreqMon.BackColor = System.Drawing.Color.Black;
            this.rf1StepFreqMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf1StepFreqMon.Location = new System.Drawing.Point(98, 81);
            this.rf1StepFreqMon.Name = "rf1StepFreqMon";
            this.rf1StepFreqMon.ReadOnly = true;
            this.rf1StepFreqMon.Size = new System.Drawing.Size(64, 20);
            this.rf1StepFreqMon.TabIndex = 58;
            this.rf1StepFreqMon.Text = "0";
            // 
            // rf1CentreFreqMon
            // 
            this.rf1CentreFreqMon.BackColor = System.Drawing.Color.Black;
            this.rf1CentreFreqMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf1CentreFreqMon.Location = new System.Drawing.Point(27, 81);
            this.rf1CentreFreqMon.Name = "rf1CentreFreqMon";
            this.rf1CentreFreqMon.ReadOnly = true;
            this.rf1CentreFreqMon.Size = new System.Drawing.Size(64, 20);
            this.rf1CentreFreqMon.TabIndex = 57;
            this.rf1CentreFreqMon.Text = "0";
            // 
            // label40
            // 
            this.label40.Location = new System.Drawing.Point(95, 62);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(80, 23);
            this.label40.TabIndex = 56;
            this.label40.Text = "Step";
            // 
            // label42
            // 
            this.label42.Location = new System.Drawing.Point(24, 62);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(80, 23);
            this.label42.TabIndex = 55;
            this.label42.Text = "Centre";
            // 
            // label45
            // 
            this.label45.Location = new System.Drawing.Point(184, 25);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(80, 23);
            this.label45.TabIndex = 46;
            this.label45.Text = "rf2 fr + (MHZ)";
            // 
            // label46
            // 
            this.label46.Location = new System.Drawing.Point(255, 25);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(80, 23);
            this.label46.TabIndex = 47;
            this.label46.Text = "rf2 fr - (MHZ)";
            // 
            // rf2PlusFreqMon
            // 
            this.rf2PlusFreqMon.BackColor = System.Drawing.Color.Black;
            this.rf2PlusFreqMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf2PlusFreqMon.Location = new System.Drawing.Point(187, 44);
            this.rf2PlusFreqMon.Name = "rf2PlusFreqMon";
            this.rf2PlusFreqMon.ReadOnly = true;
            this.rf2PlusFreqMon.Size = new System.Drawing.Size(64, 20);
            this.rf2PlusFreqMon.TabIndex = 51;
            this.rf2PlusFreqMon.Text = "0";
            // 
            // rf2MinusFreqMon
            // 
            this.rf2MinusFreqMon.BackColor = System.Drawing.Color.Black;
            this.rf2MinusFreqMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf2MinusFreqMon.Location = new System.Drawing.Point(258, 44);
            this.rf2MinusFreqMon.Name = "rf2MinusFreqMon";
            this.rf2MinusFreqMon.ReadOnly = true;
            this.rf2MinusFreqMon.Size = new System.Drawing.Size(64, 20);
            this.rf2MinusFreqMon.TabIndex = 54;
            this.rf2MinusFreqMon.Text = "0";
            // 
            // label47
            // 
            this.label47.Location = new System.Drawing.Point(184, 62);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(80, 23);
            this.label47.TabIndex = 55;
            this.label47.Text = "Centre";
            // 
            // label48
            // 
            this.label48.Location = new System.Drawing.Point(255, 62);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(80, 23);
            this.label48.TabIndex = 56;
            this.label48.Text = "Step";
            // 
            // rf2CentreFreqMon
            // 
            this.rf2CentreFreqMon.BackColor = System.Drawing.Color.Black;
            this.rf2CentreFreqMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf2CentreFreqMon.Location = new System.Drawing.Point(187, 81);
            this.rf2CentreFreqMon.Name = "rf2CentreFreqMon";
            this.rf2CentreFreqMon.ReadOnly = true;
            this.rf2CentreFreqMon.Size = new System.Drawing.Size(64, 20);
            this.rf2CentreFreqMon.TabIndex = 57;
            this.rf2CentreFreqMon.Text = "0";
            // 
            // rf2StepFreqMon
            // 
            this.rf2StepFreqMon.BackColor = System.Drawing.Color.Black;
            this.rf2StepFreqMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf2StepFreqMon.Location = new System.Drawing.Point(258, 81);
            this.rf2StepFreqMon.Name = "rf2StepFreqMon";
            this.rf2StepFreqMon.ReadOnly = true;
            this.rf2StepFreqMon.Size = new System.Drawing.Size(64, 20);
            this.rf2StepFreqMon.TabIndex = 58;
            this.rf2StepFreqMon.Text = "0";
            // 
            // label49
            // 
            this.label49.Location = new System.Drawing.Point(357, 25);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(80, 23);
            this.label49.TabIndex = 46;
            this.label49.Text = "rf1ap + (dBm)";
            // 
            // label50
            // 
            this.label50.Location = new System.Drawing.Point(518, 25);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(80, 23);
            this.label50.TabIndex = 46;
            this.label50.Text = "rf2 ap + (dBm)";
            // 
            // label51
            // 
            this.label51.Location = new System.Drawing.Point(428, 25);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(80, 23);
            this.label51.TabIndex = 47;
            this.label51.Text = "rf1 ap - (dBm)";
            // 
            // label52
            // 
            this.label52.Location = new System.Drawing.Point(589, 25);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(80, 23);
            this.label52.TabIndex = 47;
            this.label52.Text = "rf2 ap - (dBm)";
            // 
            // rfPowerUpdateButton
            // 
            this.rfPowerUpdateButton.Location = new System.Drawing.Point(469, 107);
            this.rfPowerUpdateButton.Name = "rfPowerUpdateButton";
            this.rfPowerUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.rfPowerUpdateButton.TabIndex = 50;
            this.rfPowerUpdateButton.Text = "Update";
            this.rfPowerUpdateButton.Click += new System.EventHandler(this.rfPowerUpdateButton_Click);
            // 
            // rf1PlusPowerMon
            // 
            this.rf1PlusPowerMon.BackColor = System.Drawing.Color.Black;
            this.rf1PlusPowerMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf1PlusPowerMon.Location = new System.Drawing.Point(360, 44);
            this.rf1PlusPowerMon.Name = "rf1PlusPowerMon";
            this.rf1PlusPowerMon.ReadOnly = true;
            this.rf1PlusPowerMon.Size = new System.Drawing.Size(64, 20);
            this.rf1PlusPowerMon.TabIndex = 51;
            this.rf1PlusPowerMon.Text = "0";
            // 
            // rf2PlusPowerMon
            // 
            this.rf2PlusPowerMon.BackColor = System.Drawing.Color.Black;
            this.rf2PlusPowerMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf2PlusPowerMon.Location = new System.Drawing.Point(521, 44);
            this.rf2PlusPowerMon.Name = "rf2PlusPowerMon";
            this.rf2PlusPowerMon.ReadOnly = true;
            this.rf2PlusPowerMon.Size = new System.Drawing.Size(64, 20);
            this.rf2PlusPowerMon.TabIndex = 51;
            this.rf2PlusPowerMon.Text = "0";
            // 
            // rf1MinusPowerMon
            // 
            this.rf1MinusPowerMon.BackColor = System.Drawing.Color.Black;
            this.rf1MinusPowerMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf1MinusPowerMon.Location = new System.Drawing.Point(431, 44);
            this.rf1MinusPowerMon.Name = "rf1MinusPowerMon";
            this.rf1MinusPowerMon.ReadOnly = true;
            this.rf1MinusPowerMon.Size = new System.Drawing.Size(64, 20);
            this.rf1MinusPowerMon.TabIndex = 54;
            this.rf1MinusPowerMon.Text = "0";
            // 
            // rf2MinusPowerMon
            // 
            this.rf2MinusPowerMon.BackColor = System.Drawing.Color.Black;
            this.rf2MinusPowerMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf2MinusPowerMon.Location = new System.Drawing.Point(592, 44);
            this.rf2MinusPowerMon.Name = "rf2MinusPowerMon";
            this.rf2MinusPowerMon.ReadOnly = true;
            this.rf2MinusPowerMon.Size = new System.Drawing.Size(64, 20);
            this.rf2MinusPowerMon.TabIndex = 54;
            this.rf2MinusPowerMon.Text = "0";
            // 
            // label53
            // 
            this.label53.Location = new System.Drawing.Point(357, 62);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(80, 23);
            this.label53.TabIndex = 55;
            this.label53.Text = "Centre";
            // 
            // label54
            // 
            this.label54.Location = new System.Drawing.Point(518, 62);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(80, 23);
            this.label54.TabIndex = 55;
            this.label54.Text = "Centre";
            // 
            // label55
            // 
            this.label55.Location = new System.Drawing.Point(428, 62);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(80, 23);
            this.label55.TabIndex = 56;
            this.label55.Text = "Step";
            // 
            // label56
            // 
            this.label56.Location = new System.Drawing.Point(589, 62);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(80, 23);
            this.label56.TabIndex = 56;
            this.label56.Text = "Step";
            // 
            // rf1CentrePowerMon
            // 
            this.rf1CentrePowerMon.BackColor = System.Drawing.Color.Black;
            this.rf1CentrePowerMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf1CentrePowerMon.Location = new System.Drawing.Point(360, 81);
            this.rf1CentrePowerMon.Name = "rf1CentrePowerMon";
            this.rf1CentrePowerMon.ReadOnly = true;
            this.rf1CentrePowerMon.Size = new System.Drawing.Size(64, 20);
            this.rf1CentrePowerMon.TabIndex = 57;
            this.rf1CentrePowerMon.Text = "0";
            // 
            // rf2CentrePowerMon
            // 
            this.rf2CentrePowerMon.BackColor = System.Drawing.Color.Black;
            this.rf2CentrePowerMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf2CentrePowerMon.Location = new System.Drawing.Point(521, 81);
            this.rf2CentrePowerMon.Name = "rf2CentrePowerMon";
            this.rf2CentrePowerMon.ReadOnly = true;
            this.rf2CentrePowerMon.Size = new System.Drawing.Size(64, 20);
            this.rf2CentrePowerMon.TabIndex = 57;
            this.rf2CentrePowerMon.Text = "0";
            // 
            // rf1StepPowerMon
            // 
            this.rf1StepPowerMon.BackColor = System.Drawing.Color.Black;
            this.rf1StepPowerMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf1StepPowerMon.Location = new System.Drawing.Point(431, 81);
            this.rf1StepPowerMon.Name = "rf1StepPowerMon";
            this.rf1StepPowerMon.ReadOnly = true;
            this.rf1StepPowerMon.Size = new System.Drawing.Size(64, 20);
            this.rf1StepPowerMon.TabIndex = 58;
            this.rf1StepPowerMon.Text = "0";
            // 
            // rf2StepPowerMon
            // 
            this.rf2StepPowerMon.BackColor = System.Drawing.Color.Black;
            this.rf2StepPowerMon.ForeColor = System.Drawing.Color.Chartreuse;
            this.rf2StepPowerMon.Location = new System.Drawing.Point(592, 81);
            this.rf2StepPowerMon.Name = "rf2StepPowerMon";
            this.rf2StepPowerMon.ReadOnly = true;
            this.rf2StepPowerMon.Size = new System.Drawing.Size(64, 20);
            this.rf2StepPowerMon.TabIndex = 58;
            this.rf2StepPowerMon.Text = "0";
            // 
            // ControlWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(729, 583);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ControlWindow";
            this.Text = "EDM Hardware Control";
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox16.ResumeLayout(false);
            this.groupBox14.ResumeLayout(false);
            this.groupBox14.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		#region Click handlers

        private void fieldsOffButton_Click(object sender, System.EventArgs e)
        {
            controller.FieldsOff();
        }

		private void switchEButton_Click(object sender, System.EventArgs e)
		{
			controller.SwitchE();
		}

		private void greenOnCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.EnableGreenSynth(greenOnCheck.Checked);
		}

	    private void rfSwitchEnableCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.EnableRFSwitch(rfSwitchEnableCheck.Checked);	
		}

		private void eOnCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			/*controller.SetEFieldOnOff(eOnCheck.Checked);*/
            controller.UpdateVoltages();
		}

		private void ePolarityCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.SetEPolarity(ePolarityCheck.Checked);
		}

		private void eBleedCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.SetBleed(eBleedCheck.Checked);
		}

		private void greenFMSelectCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			controller.SelectGreenDCFM(fmSelectCheck.Checked);
		}

        private void attenuatorSelectCheck_CheckedChanged(object sender, EventArgs e)
        {
            controller.SelectAttenuator(attenuatorSelectCheck.Checked);
        }

		private void phaseFlip1CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			controller.SetPhaseFlip1(phaseFlip1CheckBox.Checked);
		}

		private void phaseFlip2CheckBox_CheckedChanged(object sender, EventArgs e)
		{
			controller.SetPhaseFlip2(phaseFlip2CheckBox.Checked);
		}
		
		private void updateFieldButton_Click(object sender, System.EventArgs e)
		{
			controller.UpdateVoltages();
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

		
		private void updateFlashlampVButton_Click(object sender, System.EventArgs e)
		{
			controller.UpdateYAGFlashlampVoltage();
		}

		private void checkYagInterlockButton_Click(object sender, System.EventArgs e)
		{
			controller.CheckYAGInterlock();
		}

		private void yagQDisableButton_Click(object sender, System.EventArgs e)
		{
			controller.DisableYAGQ();
		}

		private void yagQEnableButton_Click(object sender, System.EventArgs e)
		{
			controller.EnableYAGQ();
		}

		private void stopYagFlashlampsButton_Click(object sender, System.EventArgs e)
		{
			controller.StopYAGFlashlamps();
		}

		private void startYAGFlashlampsButton_Click(object sender, System.EventArgs e)
		{
			controller.StartYAGFlashlamps();
		}

		private void scanningBZeroButton_Click(object sender, System.EventArgs e)
		{
			controller.SetScanningBZero();
		}

		private void scanningBUpdateButton_Click(object sender, System.EventArgs e)
		{
			controller.SetScanningBVoltage();
		}

		private void scanningBFSButton_Click(object sender, System.EventArgs e)
		{
			controller.SetScanningBFS();
		}

		private void pumpShutterCheck_CheckedChanged(object sender, EventArgs e)
		{
			controller.SetPumpShutter(pumpShutterCheck.Checked);
		}

		private void pump2ShutterCheck_CheckedChanged(object sender, EventArgs e)
		{
			controller.SetPump2Shutter(pump2ShutterCheck.Checked);
		}

		private void updateLaserPhotodiodesButton_Click(object sender, EventArgs e)
		{
			controller.UpdateLaserPhotodiodes();
		}

        private void setAttenuatorsButton_Click(object sender, EventArgs e)
        {
            controller.SetAttenutatorVoltages();
        }

        private void setFMVoltagesButton_Click(object sender, EventArgs e)
        {
            controller.SetFMVoltages();
        }

        private void calibrateIMonitorButton_Click(object sender, EventArgs e)
        {
            controller.CalibrateIMonitors();
        }

        private void setIMonitorMEasurementLengthButton_Click(object sender, EventArgs e)
        {
            controller.setIMonitorMeasurementLength();
        }

        private void rfFrequencyUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateRFFrequencyMonitor();
        }

        private void rfPowerUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateRFPowerMonitor();
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

        public void SetRadioButton(RadioButton button, bool state)
        {
            button.Invoke(new SetRadioButtonDelegate(SetRadioButtonHelper), new object[] { button, state });
        }
        private delegate void SetRadioButtonDelegate(RadioButton button, bool state);
        private void SetRadioButtonHelper(RadioButton button, bool state)
        {
            button.Checked = state;
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

		private void ControlWindow_Load(object sender, EventArgs e)
		{
			controller.WindowLoaded();
		}

 
	}
}
