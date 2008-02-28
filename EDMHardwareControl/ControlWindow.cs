using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using NationalInstruments.UI.WindowsForms;
using NationalInstruments.UI;


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
        private Button zeroIMonitorButton;
        private Label label35;
        private Label label17;
        public TextBox northOffsetIMonitorTextBox;
        public TextBox IMonitorMeasurementLengthTextBox;
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
        private Label label60;
        public TextBox eRampUpDelayTextBox;
        private Label label59;
        public TextBox eRampUpTimeTextBox;
        private Label label57;
        private Label label58;
        public TextBox eRampDownDelayTextBox;
        public TextBox eRampDownTimeTextBox;
        private Label label61;
        public TextBox eBleedTimeTextBox;
        private Label label62;
        public TextBox eSwitchTimeTextBox;
        public NationalInstruments.UI.WindowsForms.Led rampLED;
        public NationalInstruments.UI.WindowsForms.Led switchingLED;
        public NationalInstruments.UI.WindowsForms.WaveformGraph leakageGraph;
        public NationalInstruments.UI.WaveformPlot northLeakagePlot;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        public NationalInstruments.UI.WaveformPlot southLeakagePlot;
        private Label label63;
        public Button stopIMonitorPollButton;
        public TextBox iMonitorPollPeriod;
        public Button startIMonitorPollButton;
        private Label label64;
        public TextBox leakageMonitorSlopeTextBox;
        private Legend legend1;
        private LegendItem NorthLegendItem;
        private LegendItem SouthLegendItem;
        private GroupBox groupBox9;
        public TextBox steppingBBoxBiasTextBox;
        private Button SteppingBBoxBiasUpdateButton;
        private Label label65;
        private Button scanningBFSButton;
        private Button scanningBZeroButton;
        private GroupBox groupBox17;
        private GroupBox groupBox15;
        private Button TargetStepButton;
        private Label label66;
        public TextBox TargetNumStepsTextBox;
        private GroupBox groupBox18;
        public TextBox FLPZTVTextBox;
        private Button UpdateFLPZTVButton;
        private Label label68;
        public TextBox textBox1;
        private Button button1;
        private Label label67;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem saveParametersToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        public TextBox I2AOMFreqPlusTextBox;
        private Button I2AOMFreqUpdateButton;
        private Label label69;
        private GroupBox groupBox19;
        public TextBox FLPZTStepTextBox;
        private Label label70;
        private Panel panel5;
        public RadioButton FLPZTStepZeroButton;
        public RadioButton FLPZTStepPlusButton;
        public RadioButton FLPZTStepMinusButton;
        public RadioButton radioButton1;
        public RadioButton radioButton2;
        public RadioButton radioButton3;
        public TextBox I2AOMFreqStepTextBox;
        private Label label73;
        public TextBox I2AOMFreqMinusTextBox;
        public TextBox I2AOMFreqCentreTextBox;
        private Label label71;
        private Label label72;
        private ToolStripMenuItem loadParametersToolStripMenuItem;
        public CheckBox scramblerCheckBox;
        public Button setScramblerVoltageButton;
        private Label label74;
        public TextBox scramblerVoltageTextBox;


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
            this.switchingLED = new NationalInstruments.UI.WindowsForms.Led();
            this.rampLED = new NationalInstruments.UI.WindowsForms.Led();
            this.label62 = new System.Windows.Forms.Label();
            this.eSwitchTimeTextBox = new System.Windows.Forms.TextBox();
            this.label61 = new System.Windows.Forms.Label();
            this.eBleedTimeTextBox = new System.Windows.Forms.TextBox();
            this.label60 = new System.Windows.Forms.Label();
            this.label57 = new System.Windows.Forms.Label();
            this.eRampUpDelayTextBox = new System.Windows.Forms.TextBox();
            this.label58 = new System.Windows.Forms.Label();
            this.eRampDownDelayTextBox = new System.Windows.Forms.TextBox();
            this.label59 = new System.Windows.Forms.Label();
            this.eRampDownTimeTextBox = new System.Windows.Forms.TextBox();
            this.eRampUpTimeTextBox = new System.Windows.Forms.TextBox();
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
            this.legend1 = new NationalInstruments.UI.WindowsForms.Legend();
            this.NorthLegendItem = new NationalInstruments.UI.LegendItem();
            this.northLeakagePlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.SouthLegendItem = new NationalInstruments.UI.LegendItem();
            this.southLeakagePlot = new NationalInstruments.UI.WaveformPlot();
            this.label64 = new System.Windows.Forms.Label();
            this.leakageMonitorSlopeTextBox = new System.Windows.Forms.TextBox();
            this.label63 = new System.Windows.Forms.Label();
            this.stopIMonitorPollButton = new System.Windows.Forms.Button();
            this.iMonitorPollPeriod = new System.Windows.Forms.TextBox();
            this.startIMonitorPollButton = new System.Windows.Forms.Button();
            this.leakageGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.IMonitorMeasurementLengthTextBox = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.northOffsetIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.southOffsetIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.zeroIMonitorButton = new System.Windows.Forms.Button();
            this.southIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.northIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.updateIMonitorButton = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rf2StepPowerMon = new System.Windows.Forms.TextBox();
            this.rf2StepFreqMon = new System.Windows.Forms.TextBox();
            this.rf1StepPowerMon = new System.Windows.Forms.TextBox();
            this.rf1StepFreqMon = new System.Windows.Forms.TextBox();
            this.rf2CentrePowerMon = new System.Windows.Forms.TextBox();
            this.rf2CentreFreqMon = new System.Windows.Forms.TextBox();
            this.rf1CentrePowerMon = new System.Windows.Forms.TextBox();
            this.rf1CentreFreqMon = new System.Windows.Forms.TextBox();
            this.label56 = new System.Windows.Forms.Label();
            this.label48 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label53 = new System.Windows.Forms.Label();
            this.label42 = new System.Windows.Forms.Label();
            this.rf2MinusPowerMon = new System.Windows.Forms.TextBox();
            this.rf2MinusFreqMon = new System.Windows.Forms.TextBox();
            this.rf1MinusPowerMon = new System.Windows.Forms.TextBox();
            this.rf1MinusFreqMon = new System.Windows.Forms.TextBox();
            this.rf2PlusPowerMon = new System.Windows.Forms.TextBox();
            this.rf2PlusFreqMon = new System.Windows.Forms.TextBox();
            this.rf1PlusPowerMon = new System.Windows.Forms.TextBox();
            this.rf1PlusFreqMon = new System.Windows.Forms.TextBox();
            this.rfPowerUpdateButton = new System.Windows.Forms.Button();
            this.label52 = new System.Windows.Forms.Label();
            this.rfFrequencyUpdateButton = new System.Windows.Forms.Button();
            this.label51 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
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
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.steppingBBoxBiasTextBox = new System.Windows.Forms.TextBox();
            this.SteppingBBoxBiasUpdateButton = new System.Windows.Forms.Button();
            this.label65 = new System.Windows.Forms.Label();
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
            this.groupBox19 = new System.Windows.Forms.GroupBox();
            this.I2AOMFreqStepTextBox = new System.Windows.Forms.TextBox();
            this.label73 = new System.Windows.Forms.Label();
            this.I2AOMFreqMinusTextBox = new System.Windows.Forms.TextBox();
            this.I2AOMFreqCentreTextBox = new System.Windows.Forms.TextBox();
            this.label71 = new System.Windows.Forms.Label();
            this.I2AOMFreqPlusTextBox = new System.Windows.Forms.TextBox();
            this.label72 = new System.Windows.Forms.Label();
            this.I2AOMFreqUpdateButton = new System.Windows.Forms.Button();
            this.label69 = new System.Windows.Forms.Label();
            this.groupBox18 = new System.Windows.Forms.GroupBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.FLPZTStepZeroButton = new System.Windows.Forms.RadioButton();
            this.FLPZTStepPlusButton = new System.Windows.Forms.RadioButton();
            this.FLPZTStepMinusButton = new System.Windows.Forms.RadioButton();
            this.FLPZTStepTextBox = new System.Windows.Forms.TextBox();
            this.label70 = new System.Windows.Forms.Label();
            this.FLPZTVTextBox = new System.Windows.Forms.TextBox();
            this.UpdateFLPZTVButton = new System.Windows.Forms.Button();
            this.label68 = new System.Windows.Forms.Label();
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
            this.groupBox17 = new System.Windows.Forms.GroupBox();
            this.TargetStepButton = new System.Windows.Forms.Button();
            this.label66 = new System.Windows.Forms.Label();
            this.TargetNumStepsTextBox = new System.Windows.Forms.TextBox();
            this.groupBox15 = new System.Windows.Forms.GroupBox();
            this.label33 = new System.Windows.Forms.Label();
            this.checkYagInterlockButton = new System.Windows.Forms.Button();
            this.yagFlashlampVTextBox = new System.Windows.Forms.TextBox();
            this.interlockStatusTextBox = new System.Windows.Forms.TextBox();
            this.updateFlashlampVButton = new System.Windows.Forms.Button();
            this.label34 = new System.Windows.Forms.Label();
            this.startYAGFlashlampsButton = new System.Windows.Forms.Button();
            this.yagQDisableButton = new System.Windows.Forms.Button();
            this.stopYagFlashlampsButton = new System.Windows.Forms.Button();
            this.yagQEnableButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label67 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.scramblerCheckBox = new System.Windows.Forms.CheckBox();
            this.setScramblerVoltageButton = new System.Windows.Forms.Button();
            this.label74 = new System.Windows.Forms.Label();
            this.scramblerVoltageTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.switchingLED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.legend1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leakageGraph)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox16.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox19.SuspendLayout();
            this.groupBox18.SuspendLayout();
            this.panel5.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.groupBox17.SuspendLayout();
            this.groupBox15.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.switchingLED);
            this.groupBox2.Controls.Add(this.rampLED);
            this.groupBox2.Controls.Add(this.label62);
            this.groupBox2.Controls.Add(this.eSwitchTimeTextBox);
            this.groupBox2.Controls.Add(this.label61);
            this.groupBox2.Controls.Add(this.eBleedTimeTextBox);
            this.groupBox2.Controls.Add(this.label60);
            this.groupBox2.Controls.Add(this.label57);
            this.groupBox2.Controls.Add(this.eRampUpDelayTextBox);
            this.groupBox2.Controls.Add(this.label58);
            this.groupBox2.Controls.Add(this.eRampDownDelayTextBox);
            this.groupBox2.Controls.Add(this.label59);
            this.groupBox2.Controls.Add(this.eRampDownTimeTextBox);
            this.groupBox2.Controls.Add(this.eRampUpTimeTextBox);
            this.groupBox2.Controls.Add(this.fieldsOffButton);
            this.groupBox2.Controls.Add(this.switchEButton);
            this.groupBox2.Controls.Add(this.eBleedCheck);
            this.groupBox2.Controls.Add(this.ePolarityCheck);
            this.groupBox2.Controls.Add(this.eOnCheck);
            this.groupBox2.Location = new System.Drawing.Point(207, 16);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(280, 276);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Switch";
            // 
            // switchingLED
            // 
            this.switchingLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.switchingLED.Location = new System.Drawing.Point(253, 19);
            this.switchingLED.Name = "switchingLED";
            this.switchingLED.OffColor = System.Drawing.Color.Maroon;
            this.switchingLED.OnColor = System.Drawing.Color.Red;
            this.switchingLED.Size = new System.Drawing.Size(21, 22);
            this.switchingLED.TabIndex = 48;
            // 
            // rampLED
            // 
            this.rampLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.rampLED.Location = new System.Drawing.Point(253, 40);
            this.rampLED.Name = "rampLED";
            this.rampLED.Size = new System.Drawing.Size(21, 22);
            this.rampLED.TabIndex = 48;
            // 
            // label62
            // 
            this.label62.Location = new System.Drawing.Point(25, 156);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(115, 23);
            this.label62.TabIndex = 47;
            this.label62.Text = "Switch time (s)";
            // 
            // eSwitchTimeTextBox
            // 
            this.eSwitchTimeTextBox.Location = new System.Drawing.Point(145, 153);
            this.eSwitchTimeTextBox.Name = "eSwitchTimeTextBox";
            this.eSwitchTimeTextBox.Size = new System.Drawing.Size(64, 20);
            this.eSwitchTimeTextBox.TabIndex = 3;
            this.eSwitchTimeTextBox.Text = "1";
            // 
            // label61
            // 
            this.label61.Location = new System.Drawing.Point(25, 130);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(115, 23);
            this.label61.TabIndex = 45;
            this.label61.Text = "Bleed time (s)";
            // 
            // eBleedTimeTextBox
            // 
            this.eBleedTimeTextBox.Location = new System.Drawing.Point(145, 127);
            this.eBleedTimeTextBox.Name = "eBleedTimeTextBox";
            this.eBleedTimeTextBox.Size = new System.Drawing.Size(64, 20);
            this.eBleedTimeTextBox.TabIndex = 2;
            this.eBleedTimeTextBox.Text = "0.01";
            // 
            // label60
            // 
            this.label60.Location = new System.Drawing.Point(25, 208);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(115, 23);
            this.label60.TabIndex = 45;
            this.label60.Text = "Ramp up delay (s)";
            // 
            // label57
            // 
            this.label57.Location = new System.Drawing.Point(25, 105);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(115, 23);
            this.label57.TabIndex = 41;
            this.label57.Text = "Ramp down delay (s)";
            // 
            // eRampUpDelayTextBox
            // 
            this.eRampUpDelayTextBox.Location = new System.Drawing.Point(145, 205);
            this.eRampUpDelayTextBox.Name = "eRampUpDelayTextBox";
            this.eRampUpDelayTextBox.Size = new System.Drawing.Size(64, 20);
            this.eRampUpDelayTextBox.TabIndex = 5;
            this.eRampUpDelayTextBox.Text = "1";
            // 
            // label58
            // 
            this.label58.Location = new System.Drawing.Point(25, 79);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(115, 23);
            this.label58.TabIndex = 40;
            this.label58.Text = "Ramp down time (s)";
            // 
            // eRampDownDelayTextBox
            // 
            this.eRampDownDelayTextBox.Location = new System.Drawing.Point(145, 102);
            this.eRampDownDelayTextBox.Name = "eRampDownDelayTextBox";
            this.eRampDownDelayTextBox.Size = new System.Drawing.Size(64, 20);
            this.eRampDownDelayTextBox.TabIndex = 1;
            this.eRampDownDelayTextBox.Text = "3";
            // 
            // label59
            // 
            this.label59.Location = new System.Drawing.Point(25, 182);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(115, 23);
            this.label59.TabIndex = 43;
            this.label59.Text = "Ramp up time (s)";
            // 
            // eRampDownTimeTextBox
            // 
            this.eRampDownTimeTextBox.Location = new System.Drawing.Point(145, 76);
            this.eRampDownTimeTextBox.Name = "eRampDownTimeTextBox";
            this.eRampDownTimeTextBox.Size = new System.Drawing.Size(64, 20);
            this.eRampDownTimeTextBox.TabIndex = 0;
            this.eRampDownTimeTextBox.Text = "2";
            // 
            // eRampUpTimeTextBox
            // 
            this.eRampUpTimeTextBox.Location = new System.Drawing.Point(145, 179);
            this.eRampUpTimeTextBox.Name = "eRampUpTimeTextBox";
            this.eRampUpTimeTextBox.Size = new System.Drawing.Size(64, 20);
            this.eRampUpTimeTextBox.TabIndex = 4;
            this.eRampUpTimeTextBox.Text = "2";
            // 
            // fieldsOffButton
            // 
            this.fieldsOffButton.Location = new System.Drawing.Point(24, 241);
            this.fieldsOffButton.Name = "fieldsOffButton";
            this.fieldsOffButton.Size = new System.Drawing.Size(96, 23);
            this.fieldsOffButton.TabIndex = 23;
            this.fieldsOffButton.Text = "Zero E fields";
            this.fieldsOffButton.Click += new System.EventHandler(this.fieldsOffButton_Click);
            // 
            // switchEButton
            // 
            this.switchEButton.Location = new System.Drawing.Point(136, 241);
            this.switchEButton.Name = "switchEButton";
            this.switchEButton.Size = new System.Drawing.Size(96, 23);
            this.switchEButton.TabIndex = 22;
            this.switchEButton.Text = "Switch E";
            this.switchEButton.Click += new System.EventHandler(this.switchEButton_Click);
            // 
            // eBleedCheck
            // 
            this.eBleedCheck.Location = new System.Drawing.Point(96, 19);
            this.eBleedCheck.Name = "eBleedCheck";
            this.eBleedCheck.Size = new System.Drawing.Size(72, 24);
            this.eBleedCheck.TabIndex = 21;
            this.eBleedCheck.Text = "Bleed on";
            this.eBleedCheck.CheckedChanged += new System.EventHandler(this.eBleedCheck_CheckedChanged);
            // 
            // ePolarityCheck
            // 
            this.ePolarityCheck.Location = new System.Drawing.Point(24, 40);
            this.ePolarityCheck.Name = "ePolarityCheck";
            this.ePolarityCheck.Size = new System.Drawing.Size(136, 24);
            this.ePolarityCheck.TabIndex = 20;
            this.ePolarityCheck.Text = "Polarity (1 is checked)";
            this.ePolarityCheck.CheckedChanged += new System.EventHandler(this.ePolarityCheck_CheckedChanged);
            // 
            // eOnCheck
            // 
            this.eOnCheck.Location = new System.Drawing.Point(24, 19);
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
            this.greenDCFMBox.TabIndex = 2;
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
            this.greenOnAmpBox.TabIndex = 1;
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
            this.greenOnFreqBox.TabIndex = 0;
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
            this.label9.TabIndex = 1;
            this.label9.Text = "C minus (V)";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(16, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 23);
            this.label5.TabIndex = 0;
            this.label5.Text = "C plus (V)";
            // 
            // cPlusOffTextBox
            // 
            this.cPlusOffTextBox.Location = new System.Drawing.Point(104, 96);
            this.cPlusOffTextBox.Name = "cPlusOffTextBox";
            this.cPlusOffTextBox.Size = new System.Drawing.Size(64, 20);
            this.cPlusOffTextBox.TabIndex = 2;
            this.cPlusOffTextBox.Text = "0";
            // 
            // cMinusOffTextBox
            // 
            this.cMinusOffTextBox.Location = new System.Drawing.Point(104, 128);
            this.cMinusOffTextBox.Name = "cMinusOffTextBox";
            this.cMinusOffTextBox.Size = new System.Drawing.Size(64, 20);
            this.cMinusOffTextBox.TabIndex = 3;
            this.cMinusOffTextBox.Text = "0";
            // 
            // cMinusTextBox
            // 
            this.cMinusTextBox.Location = new System.Drawing.Point(104, 56);
            this.cMinusTextBox.Name = "cMinusTextBox";
            this.cMinusTextBox.Size = new System.Drawing.Size(64, 20);
            this.cMinusTextBox.TabIndex = 1;
            this.cMinusTextBox.Text = "0";
            // 
            // cPlusTextBox
            // 
            this.cPlusTextBox.Location = new System.Drawing.Point(104, 24);
            this.cPlusTextBox.Name = "cPlusTextBox";
            this.cPlusTextBox.Size = new System.Drawing.Size(64, 20);
            this.cPlusTextBox.TabIndex = 0;
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
            this.tabControl1.Size = new System.Drawing.Size(705, 607);
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
            this.tabPage1.Size = new System.Drawing.Size(697, 581);
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
            this.groupBox13.Location = new System.Drawing.Point(493, 179);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(184, 113);
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
            this.label37.Location = new System.Drawing.Point(6, 83);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(84, 23);
            this.label37.TabIndex = 37;
            this.label37.Text = "0+1- boost (V)";
            // 
            // label38
            // 
            this.label38.Location = new System.Drawing.Point(6, 51);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(89, 23);
            this.label38.TabIndex = 36;
            this.label38.Text = "0+ boost (V)";
            // 
            // zeroPlusOneMinusBoostTextBox
            // 
            this.zeroPlusOneMinusBoostTextBox.Location = new System.Drawing.Point(101, 79);
            this.zeroPlusOneMinusBoostTextBox.Name = "zeroPlusOneMinusBoostTextBox";
            this.zeroPlusOneMinusBoostTextBox.Size = new System.Drawing.Size(64, 20);
            this.zeroPlusOneMinusBoostTextBox.TabIndex = 1;
            this.zeroPlusOneMinusBoostTextBox.Text = "0";
            // 
            // zeroPlusBoostTextBox
            // 
            this.zeroPlusBoostTextBox.Location = new System.Drawing.Point(102, 51);
            this.zeroPlusBoostTextBox.Name = "zeroPlusBoostTextBox";
            this.zeroPlusBoostTextBox.Size = new System.Drawing.Size(64, 20);
            this.zeroPlusBoostTextBox.TabIndex = 0;
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
            this.groupBox6.Location = new System.Drawing.Point(493, 16);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(184, 153);
            this.groupBox6.TabIndex = 24;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Voltage monitors";
            // 
            // gMinusVMonitorTextBox
            // 
            this.gMinusVMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.gMinusVMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.gMinusVMonitorTextBox.Location = new System.Drawing.Point(104, 95);
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
            this.gPlusVMonitorTextBox.Location = new System.Drawing.Point(104, 71);
            this.gPlusVMonitorTextBox.Name = "gPlusVMonitorTextBox";
            this.gPlusVMonitorTextBox.ReadOnly = true;
            this.gPlusVMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.gPlusVMonitorTextBox.TabIndex = 41;
            this.gPlusVMonitorTextBox.Text = "0";
            // 
            // updateVMonitorButton
            // 
            this.updateVMonitorButton.Location = new System.Drawing.Point(56, 121);
            this.updateVMonitorButton.Name = "updateVMonitorButton";
            this.updateVMonitorButton.Size = new System.Drawing.Size(75, 23);
            this.updateVMonitorButton.TabIndex = 40;
            this.updateVMonitorButton.Text = "Update";
            this.updateVMonitorButton.Click += new System.EventHandler(this.updateVMonitorButton_Click);
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(16, 95);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(80, 23);
            this.label12.TabIndex = 39;
            this.label12.Text = "G minus (V)";
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(16, 71);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(80, 23);
            this.label13.TabIndex = 38;
            this.label13.Text = "G plus (V)";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(16, 48);
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
            this.cMinusVMonitorTextBox.Location = new System.Drawing.Point(104, 48);
            this.cMinusVMonitorTextBox.Name = "cMinusVMonitorTextBox";
            this.cMinusVMonitorTextBox.ReadOnly = true;
            this.cMinusVMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.cMinusVMonitorTextBox.TabIndex = 33;
            this.cMinusVMonitorTextBox.Text = "0";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.legend1);
            this.groupBox7.Controls.Add(this.label64);
            this.groupBox7.Controls.Add(this.leakageMonitorSlopeTextBox);
            this.groupBox7.Controls.Add(this.label63);
            this.groupBox7.Controls.Add(this.stopIMonitorPollButton);
            this.groupBox7.Controls.Add(this.iMonitorPollPeriod);
            this.groupBox7.Controls.Add(this.startIMonitorPollButton);
            this.groupBox7.Controls.Add(this.leakageGraph);
            this.groupBox7.Controls.Add(this.IMonitorMeasurementLengthTextBox);
            this.groupBox7.Controls.Add(this.label35);
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.northOffsetIMonitorTextBox);
            this.groupBox7.Controls.Add(this.label16);
            this.groupBox7.Controls.Add(this.southOffsetIMonitorTextBox);
            this.groupBox7.Controls.Add(this.zeroIMonitorButton);
            this.groupBox7.Controls.Add(this.southIMonitorTextBox);
            this.groupBox7.Controls.Add(this.northIMonitorTextBox);
            this.groupBox7.Controls.Add(this.updateIMonitorButton);
            this.groupBox7.Controls.Add(this.label18);
            this.groupBox7.Controls.Add(this.label19);
            this.groupBox7.Location = new System.Drawing.Point(17, 298);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(660, 270);
            this.groupBox7.TabIndex = 44;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Current monitors";
            // 
            // legend1
            // 
            this.legend1.Items.AddRange(new NationalInstruments.UI.LegendItem[] {
            this.NorthLegendItem,
            this.SouthLegendItem});
            this.legend1.ItemSize = new System.Drawing.Size(12, 12);
            this.legend1.Location = new System.Drawing.Point(452, 86);
            this.legend1.Name = "legend1";
            this.legend1.Size = new System.Drawing.Size(115, 22);
            this.legend1.TabIndex = 59;
            // 
            // NorthLegendItem
            // 
            this.NorthLegendItem.Source = this.northLeakagePlot;
            this.NorthLegendItem.Text = "North";
            // 
            // northLeakagePlot
            // 
            this.northLeakagePlot.AntiAliased = true;
            this.northLeakagePlot.HistoryCapacity = 10000;
            this.northLeakagePlot.LineColor = System.Drawing.Color.Crimson;
            this.northLeakagePlot.LineWidth = 2F;
            this.northLeakagePlot.XAxis = this.xAxis1;
            this.northLeakagePlot.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis1.Range = new NationalInstruments.UI.Range(0, 500);
            // 
            // yAxis1
            // 
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis1.OriginLineVisible = true;
            this.yAxis1.Range = new NationalInstruments.UI.Range(-500, 500);
            // 
            // SouthLegendItem
            // 
            this.SouthLegendItem.Source = this.southLeakagePlot;
            this.SouthLegendItem.Text = "South";
            // 
            // southLeakagePlot
            // 
            this.southLeakagePlot.HistoryCapacity = 10000;
            this.southLeakagePlot.LineColor = System.Drawing.Color.DodgerBlue;
            this.southLeakagePlot.LineWidth = 2F;
            this.southLeakagePlot.XAxis = this.xAxis1;
            this.southLeakagePlot.YAxis = this.yAxis1;
            // 
            // label64
            // 
            this.label64.Location = new System.Drawing.Point(362, 14);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(108, 59);
            this.label64.TabIndex = 58;
            this.label64.Text = "Monitor slope (Hz/I)\r\nShould be:\r\n~200Hz/nA hi\r\n~2000Hz/uA lo";
            // 
            // leakageMonitorSlopeTextBox
            // 
            this.leakageMonitorSlopeTextBox.Location = new System.Drawing.Point(365, 76);
            this.leakageMonitorSlopeTextBox.Name = "leakageMonitorSlopeTextBox";
            this.leakageMonitorSlopeTextBox.Size = new System.Drawing.Size(64, 20);
            this.leakageMonitorSlopeTextBox.TabIndex = 2;
            this.leakageMonitorSlopeTextBox.Text = "200";
            // 
            // label63
            // 
            this.label63.Location = new System.Drawing.Point(485, 27);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(99, 23);
            this.label63.TabIndex = 56;
            this.label63.Text = "Poll period (ms)";
            // 
            // stopIMonitorPollButton
            // 
            this.stopIMonitorPollButton.Enabled = false;
            this.stopIMonitorPollButton.Location = new System.Drawing.Point(579, 80);
            this.stopIMonitorPollButton.Name = "stopIMonitorPollButton";
            this.stopIMonitorPollButton.Size = new System.Drawing.Size(75, 23);
            this.stopIMonitorPollButton.TabIndex = 55;
            this.stopIMonitorPollButton.Text = "Stop poll";
            this.stopIMonitorPollButton.UseVisualStyleBackColor = true;
            this.stopIMonitorPollButton.Click += new System.EventHandler(this.stopIMonitorPollButton_Click);
            // 
            // iMonitorPollPeriod
            // 
            this.iMonitorPollPeriod.Location = new System.Drawing.Point(590, 25);
            this.iMonitorPollPeriod.Name = "iMonitorPollPeriod";
            this.iMonitorPollPeriod.Size = new System.Drawing.Size(64, 20);
            this.iMonitorPollPeriod.TabIndex = 0;
            this.iMonitorPollPeriod.Text = "100";
            // 
            // startIMonitorPollButton
            // 
            this.startIMonitorPollButton.Location = new System.Drawing.Point(579, 51);
            this.startIMonitorPollButton.Name = "startIMonitorPollButton";
            this.startIMonitorPollButton.Size = new System.Drawing.Size(75, 23);
            this.startIMonitorPollButton.TabIndex = 53;
            this.startIMonitorPollButton.Text = "Start poll";
            this.startIMonitorPollButton.UseVisualStyleBackColor = true;
            this.startIMonitorPollButton.Click += new System.EventHandler(this.startIMonitorPollButton_Click);
            // 
            // leakageGraph
            // 
            this.leakageGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY)
                        | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint)
                        | NationalInstruments.UI.GraphInteractionModes.PanX)
                        | NationalInstruments.UI.GraphInteractionModes.PanY)
                        | NationalInstruments.UI.GraphInteractionModes.DragCursor)
                        | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption)
                        | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.leakageGraph.Location = new System.Drawing.Point(17, 111);
            this.leakageGraph.Name = "leakageGraph";
            this.leakageGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.northLeakagePlot,
            this.southLeakagePlot});
            this.leakageGraph.Size = new System.Drawing.Size(637, 153);
            this.leakageGraph.TabIndex = 45;
            this.leakageGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.leakageGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // IMonitorMeasurementLengthTextBox
            // 
            this.IMonitorMeasurementLengthTextBox.Location = new System.Drawing.Point(280, 76);
            this.IMonitorMeasurementLengthTextBox.Name = "IMonitorMeasurementLengthTextBox";
            this.IMonitorMeasurementLengthTextBox.Size = new System.Drawing.Size(64, 20);
            this.IMonitorMeasurementLengthTextBox.TabIndex = 1;
            this.IMonitorMeasurementLengthTextBox.Text = "200";
            // 
            // label35
            // 
            this.label35.Location = new System.Drawing.Point(194, 73);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(80, 31);
            this.label35.TabIndex = 51;
            this.label35.Text = "Measurement Length (S)";
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(186, 24);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(92, 23);
            this.label17.TabIndex = 50;
            this.label17.Text = "North offset (Hz)";
            // 
            // northOffsetIMonitorTextBox
            // 
            this.northOffsetIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.northOffsetIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.northOffsetIMonitorTextBox.Location = new System.Drawing.Point(282, 24);
            this.northOffsetIMonitorTextBox.Name = "northOffsetIMonitorTextBox";
            this.northOffsetIMonitorTextBox.ReadOnly = true;
            this.northOffsetIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.northOffsetIMonitorTextBox.TabIndex = 49;
            this.northOffsetIMonitorTextBox.Text = "0";
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(184, 50);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(97, 23);
            this.label16.TabIndex = 48;
            this.label16.Text = "South offset (Hz)";
            // 
            // southOffsetIMonitorTextBox
            // 
            this.southOffsetIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.southOffsetIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.southOffsetIMonitorTextBox.Location = new System.Drawing.Point(282, 50);
            this.southOffsetIMonitorTextBox.Name = "southOffsetIMonitorTextBox";
            this.southOffsetIMonitorTextBox.ReadOnly = true;
            this.southOffsetIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.southOffsetIMonitorTextBox.TabIndex = 47;
            this.southOffsetIMonitorTextBox.Text = "0";
            // 
            // zeroIMonitorButton
            // 
            this.zeroIMonitorButton.Location = new System.Drawing.Point(98, 76);
            this.zeroIMonitorButton.Name = "zeroIMonitorButton";
            this.zeroIMonitorButton.Size = new System.Drawing.Size(75, 23);
            this.zeroIMonitorButton.TabIndex = 46;
            this.zeroIMonitorButton.Text = "Zero";
            this.zeroIMonitorButton.UseVisualStyleBackColor = true;
            this.zeroIMonitorButton.Click += new System.EventHandler(this.calibrateIMonitorButton_Click);
            // 
            // southIMonitorTextBox
            // 
            this.southIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.southIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.southIMonitorTextBox.Location = new System.Drawing.Point(104, 50);
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
            this.updateIMonitorButton.Location = new System.Drawing.Point(17, 76);
            this.updateIMonitorButton.Name = "updateIMonitorButton";
            this.updateIMonitorButton.Size = new System.Drawing.Size(75, 23);
            this.updateIMonitorButton.TabIndex = 40;
            this.updateIMonitorButton.Text = "Update";
            this.updateIMonitorButton.Click += new System.EventHandler(this.updateIMonitorButton_Click);
            // 
            // label18
            // 
            this.label18.Location = new System.Drawing.Point(16, 50);
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
            this.tabPage2.Controls.Add(this.label74);
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Controls.Add(this.scramblerVoltageTextBox);
            this.tabPage2.Controls.Add(this.groupBox16);
            this.tabPage2.Controls.Add(this.groupBox14);
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(697, 581);
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
            // label56
            // 
            this.label56.Location = new System.Drawing.Point(589, 64);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(80, 23);
            this.label56.TabIndex = 56;
            this.label56.Text = "Step";
            // 
            // label48
            // 
            this.label48.Location = new System.Drawing.Point(255, 64);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(80, 23);
            this.label48.TabIndex = 56;
            this.label48.Text = "Step";
            // 
            // label55
            // 
            this.label55.Location = new System.Drawing.Point(428, 64);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(80, 23);
            this.label55.TabIndex = 56;
            this.label55.Text = "Step";
            // 
            // label40
            // 
            this.label40.Location = new System.Drawing.Point(95, 64);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(80, 23);
            this.label40.TabIndex = 56;
            this.label40.Text = "Step";
            // 
            // label54
            // 
            this.label54.Location = new System.Drawing.Point(518, 64);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(80, 23);
            this.label54.TabIndex = 55;
            this.label54.Text = "Centre";
            // 
            // label47
            // 
            this.label47.Location = new System.Drawing.Point(184, 64);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(80, 23);
            this.label47.TabIndex = 55;
            this.label47.Text = "Centre";
            // 
            // label53
            // 
            this.label53.Location = new System.Drawing.Point(357, 64);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(80, 23);
            this.label53.TabIndex = 55;
            this.label53.Text = "Centre";
            // 
            // label42
            // 
            this.label42.Location = new System.Drawing.Point(24, 64);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(80, 23);
            this.label42.TabIndex = 55;
            this.label42.Text = "Centre";
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
            // rfPowerUpdateButton
            // 
            this.rfPowerUpdateButton.Location = new System.Drawing.Point(469, 107);
            this.rfPowerUpdateButton.Name = "rfPowerUpdateButton";
            this.rfPowerUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.rfPowerUpdateButton.TabIndex = 50;
            this.rfPowerUpdateButton.Text = "Update";
            this.rfPowerUpdateButton.Click += new System.EventHandler(this.rfPowerUpdateButton_Click);
            // 
            // label52
            // 
            this.label52.Location = new System.Drawing.Point(589, 25);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(80, 23);
            this.label52.TabIndex = 47;
            this.label52.Text = "rf2 ap - (dBm)";
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
            // label51
            // 
            this.label51.Location = new System.Drawing.Point(428, 25);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(80, 23);
            this.label51.TabIndex = 47;
            this.label51.Text = "rf1 ap - (dBm)";
            // 
            // label46
            // 
            this.label46.Location = new System.Drawing.Point(255, 25);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(80, 23);
            this.label46.TabIndex = 47;
            this.label46.Text = "rf2 fr - (Hz)";
            // 
            // label50
            // 
            this.label50.Location = new System.Drawing.Point(518, 25);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(80, 23);
            this.label50.TabIndex = 46;
            this.label50.Text = "rf2 ap + (dBm)";
            // 
            // label43
            // 
            this.label43.Location = new System.Drawing.Point(95, 25);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(80, 23);
            this.label43.TabIndex = 47;
            this.label43.Text = "rf1 fr - (Hz)";
            // 
            // label49
            // 
            this.label49.Location = new System.Drawing.Point(357, 25);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(80, 23);
            this.label49.TabIndex = 46;
            this.label49.Text = "rf1ap + (dBm)";
            // 
            // label45
            // 
            this.label45.Location = new System.Drawing.Point(184, 25);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(80, 23);
            this.label45.TabIndex = 46;
            this.label45.Text = "rf2 fr + (Hz)";
            // 
            // label44
            // 
            this.label44.Location = new System.Drawing.Point(24, 25);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(80, 23);
            this.label44.TabIndex = 46;
            this.label44.Text = "rf1 fr + (Hz)";
            // 
            // groupBox16
            // 
            this.groupBox16.Controls.Add(this.scramblerCheckBox);
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
            this.groupBox14.Controls.Add(this.setScramblerVoltageButton);
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
            this.groupBox14.Size = new System.Drawing.Size(363, 304);
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
            this.rf2FMIncTextBox.TabIndex = 7;
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
            this.rf1FMIncTextBox.TabIndex = 5;
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
            this.rf2AttIncTextBox.TabIndex = 3;
            this.rf2AttIncTextBox.Text = "0";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(168, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 23);
            this.label6.TabIndex = 2;
            this.label6.Text = "+-";
            // 
            // rf1AttIncTextBox
            // 
            this.rf1AttIncTextBox.Location = new System.Drawing.Point(198, 24);
            this.rf1AttIncTextBox.Name = "rf1AttIncTextBox";
            this.rf1AttIncTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf1AttIncTextBox.TabIndex = 1;
            this.rf1AttIncTextBox.Text = "0";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(168, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(24, 23);
            this.label4.TabIndex = 0;
            this.label4.Text = "+-";
            // 
            // setFMVoltagesButton
            // 
            this.setFMVoltagesButton.Location = new System.Drawing.Point(125, 189);
            this.setFMVoltagesButton.Name = "setFMVoltagesButton";
            this.setFMVoltagesButton.Size = new System.Drawing.Size(131, 23);
            this.setFMVoltagesButton.TabIndex = 23;
            this.setFMVoltagesButton.Text = "Set fm voltages";
            this.setFMVoltagesButton.Click += new System.EventHandler(this.setFMVoltagesButton_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(59, 163);
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
            this.rf2FMVoltage.TabIndex = 6;
            this.rf2FMVoltage.Text = "0";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(59, 137);
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
            this.rf1FMVoltage.TabIndex = 4;
            this.rf1FMVoltage.Text = "0";
            // 
            // setAttenuatorsButton
            // 
            this.setAttenuatorsButton.Location = new System.Drawing.Point(125, 79);
            this.setAttenuatorsButton.Name = "setAttenuatorsButton";
            this.setAttenuatorsButton.Size = new System.Drawing.Size(131, 23);
            this.setAttenuatorsButton.TabIndex = 18;
            this.setAttenuatorsButton.Text = "Set attenuator voltages";
            this.setAttenuatorsButton.Click += new System.EventHandler(this.setAttenuatorsButton_Click);
            // 
            // label36
            // 
            this.label36.Location = new System.Drawing.Point(24, 54);
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
            this.rf2AttenuatorVoltageTextBox.TabIndex = 2;
            this.rf2AttenuatorVoltageTextBox.Text = "5";
            // 
            // label39
            // 
            this.label39.Location = new System.Drawing.Point(24, 28);
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
            this.rf1AttenuatorVoltageTextBox.TabIndex = 0;
            this.rf1AttenuatorVoltageTextBox.Text = "5";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox9);
            this.tabPage3.Controls.Add(this.groupBox12);
            this.tabPage3.Controls.Add(this.groupBox8);
            this.tabPage3.Controls.Add(this.groupBox5);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(697, 581);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "B-field";
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.steppingBBoxBiasTextBox);
            this.groupBox9.Controls.Add(this.SteppingBBoxBiasUpdateButton);
            this.groupBox9.Controls.Add(this.label65);
            this.groupBox9.Location = new System.Drawing.Point(310, 276);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(296, 96);
            this.groupBox9.TabIndex = 47;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Stepping B box bias";
            // 
            // steppingBBoxBiasTextBox
            // 
            this.steppingBBoxBiasTextBox.Location = new System.Drawing.Point(96, 24);
            this.steppingBBoxBiasTextBox.Name = "steppingBBoxBiasTextBox";
            this.steppingBBoxBiasTextBox.Size = new System.Drawing.Size(64, 20);
            this.steppingBBoxBiasTextBox.TabIndex = 45;
            this.steppingBBoxBiasTextBox.Text = "0";
            // 
            // SteppingBBoxBiasUpdateButton
            // 
            this.SteppingBBoxBiasUpdateButton.Location = new System.Drawing.Point(184, 24);
            this.SteppingBBoxBiasUpdateButton.Name = "SteppingBBoxBiasUpdateButton";
            this.SteppingBBoxBiasUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.SteppingBBoxBiasUpdateButton.TabIndex = 40;
            this.SteppingBBoxBiasUpdateButton.Text = "Update";
            this.SteppingBBoxBiasUpdateButton.Click += new System.EventHandler(this.SteppingBBoxBiasUpdateButton_Click);
            // 
            // label65
            // 
            this.label65.Location = new System.Drawing.Point(16, 24);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(80, 23);
            this.label65.TabIndex = 36;
            this.label65.Text = "Voltage (V)";
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.scanningBVoltageBox);
            this.groupBox12.Controls.Add(this.scanningBFSButton);
            this.groupBox12.Controls.Add(this.scanningBZeroButton);
            this.groupBox12.Controls.Add(this.scanningBUpdateButton);
            this.groupBox12.Controls.Add(this.label41);
            this.groupBox12.Location = new System.Drawing.Point(8, 276);
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
            this.groupBox8.Location = new System.Drawing.Point(8, 78);
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
            this.tabPage4.Controls.Add(this.groupBox19);
            this.tabPage4.Controls.Add(this.groupBox18);
            this.tabPage4.Controls.Add(this.groupBox11);
            this.tabPage4.Controls.Add(this.groupBox10);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(697, 581);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Laser";
            // 
            // groupBox19
            // 
            this.groupBox19.Controls.Add(this.I2AOMFreqStepTextBox);
            this.groupBox19.Controls.Add(this.label73);
            this.groupBox19.Controls.Add(this.I2AOMFreqMinusTextBox);
            this.groupBox19.Controls.Add(this.I2AOMFreqCentreTextBox);
            this.groupBox19.Controls.Add(this.label71);
            this.groupBox19.Controls.Add(this.I2AOMFreqPlusTextBox);
            this.groupBox19.Controls.Add(this.label72);
            this.groupBox19.Controls.Add(this.I2AOMFreqUpdateButton);
            this.groupBox19.Controls.Add(this.label69);
            this.groupBox19.Location = new System.Drawing.Point(238, 136);
            this.groupBox19.Name = "groupBox19";
            this.groupBox19.Size = new System.Drawing.Size(277, 176);
            this.groupBox19.TabIndex = 55;
            this.groupBox19.TabStop = false;
            this.groupBox19.Text = "Iodine lock";
            // 
            // I2AOMFreqStepTextBox
            // 
            this.I2AOMFreqStepTextBox.BackColor = System.Drawing.Color.Black;
            this.I2AOMFreqStepTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.I2AOMFreqStepTextBox.Location = new System.Drawing.Point(145, 109);
            this.I2AOMFreqStepTextBox.Name = "I2AOMFreqStepTextBox";
            this.I2AOMFreqStepTextBox.ReadOnly = true;
            this.I2AOMFreqStepTextBox.Size = new System.Drawing.Size(126, 20);
            this.I2AOMFreqStepTextBox.TabIndex = 56;
            this.I2AOMFreqStepTextBox.Text = "0";
            // 
            // label73
            // 
            this.label73.Location = new System.Drawing.Point(16, 112);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(111, 23);
            this.label73.TabIndex = 55;
            this.label73.Text = "Step (Hz)";
            // 
            // I2AOMFreqMinusTextBox
            // 
            this.I2AOMFreqMinusTextBox.BackColor = System.Drawing.Color.Black;
            this.I2AOMFreqMinusTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.I2AOMFreqMinusTextBox.Location = new System.Drawing.Point(145, 55);
            this.I2AOMFreqMinusTextBox.Name = "I2AOMFreqMinusTextBox";
            this.I2AOMFreqMinusTextBox.ReadOnly = true;
            this.I2AOMFreqMinusTextBox.Size = new System.Drawing.Size(126, 20);
            this.I2AOMFreqMinusTextBox.TabIndex = 56;
            this.I2AOMFreqMinusTextBox.Text = "0";
            // 
            // I2AOMFreqCentreTextBox
            // 
            this.I2AOMFreqCentreTextBox.BackColor = System.Drawing.Color.Black;
            this.I2AOMFreqCentreTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.I2AOMFreqCentreTextBox.Location = new System.Drawing.Point(145, 83);
            this.I2AOMFreqCentreTextBox.Name = "I2AOMFreqCentreTextBox";
            this.I2AOMFreqCentreTextBox.ReadOnly = true;
            this.I2AOMFreqCentreTextBox.Size = new System.Drawing.Size(126, 20);
            this.I2AOMFreqCentreTextBox.TabIndex = 54;
            this.I2AOMFreqCentreTextBox.Text = "0";
            // 
            // label71
            // 
            this.label71.Location = new System.Drawing.Point(16, 58);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(111, 23);
            this.label71.TabIndex = 55;
            this.label71.Text = "AOM freq high (Hz)";
            // 
            // I2AOMFreqPlusTextBox
            // 
            this.I2AOMFreqPlusTextBox.BackColor = System.Drawing.Color.Black;
            this.I2AOMFreqPlusTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.I2AOMFreqPlusTextBox.Location = new System.Drawing.Point(145, 29);
            this.I2AOMFreqPlusTextBox.Name = "I2AOMFreqPlusTextBox";
            this.I2AOMFreqPlusTextBox.ReadOnly = true;
            this.I2AOMFreqPlusTextBox.Size = new System.Drawing.Size(126, 20);
            this.I2AOMFreqPlusTextBox.TabIndex = 54;
            this.I2AOMFreqPlusTextBox.Text = "0";
            // 
            // label72
            // 
            this.label72.Location = new System.Drawing.Point(16, 86);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(111, 23);
            this.label72.TabIndex = 52;
            this.label72.Text = "Centre (Hz)";
            // 
            // I2AOMFreqUpdateButton
            // 
            this.I2AOMFreqUpdateButton.Location = new System.Drawing.Point(101, 147);
            this.I2AOMFreqUpdateButton.Name = "I2AOMFreqUpdateButton";
            this.I2AOMFreqUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.I2AOMFreqUpdateButton.TabIndex = 53;
            this.I2AOMFreqUpdateButton.Text = "Update";
            this.I2AOMFreqUpdateButton.Click += new System.EventHandler(this.I2AOMFreqUpdateButton_Click);
            // 
            // label69
            // 
            this.label69.Location = new System.Drawing.Point(16, 32);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(111, 23);
            this.label69.TabIndex = 52;
            this.label69.Text = "AOM freq low (Hz)";
            // 
            // groupBox18
            // 
            this.groupBox18.Controls.Add(this.panel5);
            this.groupBox18.Controls.Add(this.FLPZTStepTextBox);
            this.groupBox18.Controls.Add(this.label70);
            this.groupBox18.Controls.Add(this.FLPZTVTextBox);
            this.groupBox18.Controls.Add(this.UpdateFLPZTVButton);
            this.groupBox18.Controls.Add(this.label68);
            this.groupBox18.Location = new System.Drawing.Point(238, 20);
            this.groupBox18.Name = "groupBox18";
            this.groupBox18.Size = new System.Drawing.Size(299, 96);
            this.groupBox18.TabIndex = 48;
            this.groupBox18.TabStop = false;
            this.groupBox18.Text = "Laser frequency";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.FLPZTStepZeroButton);
            this.panel5.Controls.Add(this.FLPZTStepPlusButton);
            this.panel5.Controls.Add(this.FLPZTStepMinusButton);
            this.panel5.Location = new System.Drawing.Point(182, 20);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(108, 32);
            this.panel5.TabIndex = 48;
            // 
            // FLPZTStepZeroButton
            // 
            this.FLPZTStepZeroButton.AutoSize = true;
            this.FLPZTStepZeroButton.Checked = true;
            this.FLPZTStepZeroButton.Location = new System.Drawing.Point(77, 7);
            this.FLPZTStepZeroButton.Name = "FLPZTStepZeroButton";
            this.FLPZTStepZeroButton.Size = new System.Drawing.Size(31, 17);
            this.FLPZTStepZeroButton.TabIndex = 32;
            this.FLPZTStepZeroButton.TabStop = true;
            this.FLPZTStepZeroButton.Text = "0";
            this.FLPZTStepZeroButton.UseVisualStyleBackColor = true;
            // 
            // FLPZTStepPlusButton
            // 
            this.FLPZTStepPlusButton.AutoSize = true;
            this.FLPZTStepPlusButton.Location = new System.Drawing.Point(3, 6);
            this.FLPZTStepPlusButton.Name = "FLPZTStepPlusButton";
            this.FLPZTStepPlusButton.Size = new System.Drawing.Size(31, 17);
            this.FLPZTStepPlusButton.TabIndex = 32;
            this.FLPZTStepPlusButton.Text = "+";
            this.FLPZTStepPlusButton.UseVisualStyleBackColor = true;
            // 
            // FLPZTStepMinusButton
            // 
            this.FLPZTStepMinusButton.AutoSize = true;
            this.FLPZTStepMinusButton.Location = new System.Drawing.Point(42, 7);
            this.FLPZTStepMinusButton.Name = "FLPZTStepMinusButton";
            this.FLPZTStepMinusButton.Size = new System.Drawing.Size(28, 17);
            this.FLPZTStepMinusButton.TabIndex = 32;
            this.FLPZTStepMinusButton.Text = "-";
            this.FLPZTStepMinusButton.UseVisualStyleBackColor = true;
            // 
            // FLPZTStepTextBox
            // 
            this.FLPZTStepTextBox.Location = new System.Drawing.Point(96, 48);
            this.FLPZTStepTextBox.Name = "FLPZTStepTextBox";
            this.FLPZTStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.FLPZTStepTextBox.TabIndex = 47;
            this.FLPZTStepTextBox.Text = "0";
            // 
            // label70
            // 
            this.label70.Location = new System.Drawing.Point(16, 48);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(80, 23);
            this.label70.TabIndex = 46;
            this.label70.Text = "Step (V)";
            // 
            // FLPZTVTextBox
            // 
            this.FLPZTVTextBox.Location = new System.Drawing.Point(96, 24);
            this.FLPZTVTextBox.Name = "FLPZTVTextBox";
            this.FLPZTVTextBox.Size = new System.Drawing.Size(64, 20);
            this.FLPZTVTextBox.TabIndex = 45;
            this.FLPZTVTextBox.Text = "0";
            // 
            // UpdateFLPZTVButton
            // 
            this.UpdateFLPZTVButton.Location = new System.Drawing.Point(215, 67);
            this.UpdateFLPZTVButton.Name = "UpdateFLPZTVButton";
            this.UpdateFLPZTVButton.Size = new System.Drawing.Size(75, 23);
            this.UpdateFLPZTVButton.TabIndex = 40;
            this.UpdateFLPZTVButton.Text = "Update";
            this.UpdateFLPZTVButton.Click += new System.EventHandler(this.UpdateFLPZTVButton_Click);
            // 
            // label68
            // 
            this.label68.Location = new System.Drawing.Point(16, 24);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(80, 23);
            this.label68.TabIndex = 36;
            this.label68.Text = "Voltage (V)";
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
            this.tabPage5.Controls.Add(this.groupBox17);
            this.tabPage5.Controls.Add(this.groupBox15);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(697, 581);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Source";
            // 
            // groupBox17
            // 
            this.groupBox17.Controls.Add(this.TargetStepButton);
            this.groupBox17.Controls.Add(this.label66);
            this.groupBox17.Controls.Add(this.TargetNumStepsTextBox);
            this.groupBox17.Location = new System.Drawing.Point(13, 165);
            this.groupBox17.Name = "groupBox17";
            this.groupBox17.Size = new System.Drawing.Size(351, 64);
            this.groupBox17.TabIndex = 47;
            this.groupBox17.TabStop = false;
            this.groupBox17.Text = "Target stepper";
            // 
            // TargetStepButton
            // 
            this.TargetStepButton.Location = new System.Drawing.Point(256, 20);
            this.TargetStepButton.Name = "TargetStepButton";
            this.TargetStepButton.Size = new System.Drawing.Size(75, 23);
            this.TargetStepButton.TabIndex = 2;
            this.TargetStepButton.Text = "Step!";
            this.TargetStepButton.UseVisualStyleBackColor = true;
            this.TargetStepButton.Click += new System.EventHandler(this.TargetStepButton_Click);
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Location = new System.Drawing.Point(19, 25);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(89, 13);
            this.label66.TabIndex = 1;
            this.label66.Text = "Number of pulses";
            // 
            // TargetNumStepsTextBox
            // 
            this.TargetNumStepsTextBox.Location = new System.Drawing.Point(158, 22);
            this.TargetNumStepsTextBox.Name = "TargetNumStepsTextBox";
            this.TargetNumStepsTextBox.Size = new System.Drawing.Size(66, 20);
            this.TargetNumStepsTextBox.TabIndex = 0;
            this.TargetNumStepsTextBox.Text = "10";
            // 
            // groupBox15
            // 
            this.groupBox15.Controls.Add(this.label33);
            this.groupBox15.Controls.Add(this.checkYagInterlockButton);
            this.groupBox15.Controls.Add(this.yagFlashlampVTextBox);
            this.groupBox15.Controls.Add(this.interlockStatusTextBox);
            this.groupBox15.Controls.Add(this.updateFlashlampVButton);
            this.groupBox15.Controls.Add(this.label34);
            this.groupBox15.Controls.Add(this.startYAGFlashlampsButton);
            this.groupBox15.Controls.Add(this.yagQDisableButton);
            this.groupBox15.Controls.Add(this.stopYagFlashlampsButton);
            this.groupBox15.Controls.Add(this.yagQEnableButton);
            this.groupBox15.Location = new System.Drawing.Point(13, 14);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Size = new System.Drawing.Size(528, 145);
            this.groupBox15.TabIndex = 46;
            this.groupBox15.TabStop = false;
            this.groupBox15.Text = "YAG";
            // 
            // label33
            // 
            this.label33.Location = new System.Drawing.Point(16, 31);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(144, 23);
            this.label33.TabIndex = 13;
            this.label33.Text = "Flashlamp voltage (V)";
            // 
            // checkYagInterlockButton
            // 
            this.checkYagInterlockButton.Location = new System.Drawing.Point(256, 63);
            this.checkYagInterlockButton.Name = "checkYagInterlockButton";
            this.checkYagInterlockButton.Size = new System.Drawing.Size(75, 23);
            this.checkYagInterlockButton.TabIndex = 45;
            this.checkYagInterlockButton.Text = "Check";
            this.checkYagInterlockButton.Click += new System.EventHandler(this.checkYagInterlockButton_Click);
            // 
            // yagFlashlampVTextBox
            // 
            this.yagFlashlampVTextBox.Location = new System.Drawing.Point(160, 31);
            this.yagFlashlampVTextBox.Name = "yagFlashlampVTextBox";
            this.yagFlashlampVTextBox.Size = new System.Drawing.Size(64, 20);
            this.yagFlashlampVTextBox.TabIndex = 12;
            this.yagFlashlampVTextBox.Text = "1220";
            // 
            // interlockStatusTextBox
            // 
            this.interlockStatusTextBox.BackColor = System.Drawing.Color.Black;
            this.interlockStatusTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.interlockStatusTextBox.Location = new System.Drawing.Point(160, 63);
            this.interlockStatusTextBox.Name = "interlockStatusTextBox";
            this.interlockStatusTextBox.ReadOnly = true;
            this.interlockStatusTextBox.Size = new System.Drawing.Size(64, 20);
            this.interlockStatusTextBox.TabIndex = 44;
            this.interlockStatusTextBox.Text = "0";
            // 
            // updateFlashlampVButton
            // 
            this.updateFlashlampVButton.Location = new System.Drawing.Point(256, 31);
            this.updateFlashlampVButton.Name = "updateFlashlampVButton";
            this.updateFlashlampVButton.Size = new System.Drawing.Size(75, 23);
            this.updateFlashlampVButton.TabIndex = 14;
            this.updateFlashlampVButton.Text = "Update V";
            this.updateFlashlampVButton.Click += new System.EventHandler(this.updateFlashlampVButton_Click);
            // 
            // label34
            // 
            this.label34.Location = new System.Drawing.Point(16, 63);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(104, 23);
            this.label34.TabIndex = 43;
            this.label34.Text = "Interlock failed";
            // 
            // startYAGFlashlampsButton
            // 
            this.startYAGFlashlampsButton.Location = new System.Drawing.Point(16, 103);
            this.startYAGFlashlampsButton.Name = "startYAGFlashlampsButton";
            this.startYAGFlashlampsButton.Size = new System.Drawing.Size(112, 23);
            this.startYAGFlashlampsButton.TabIndex = 15;
            this.startYAGFlashlampsButton.Text = "Start Flashlamps";
            this.startYAGFlashlampsButton.Click += new System.EventHandler(this.startYAGFlashlampsButton_Click);
            // 
            // yagQDisableButton
            // 
            this.yagQDisableButton.Enabled = false;
            this.yagQDisableButton.Location = new System.Drawing.Point(400, 103);
            this.yagQDisableButton.Name = "yagQDisableButton";
            this.yagQDisableButton.Size = new System.Drawing.Size(112, 23);
            this.yagQDisableButton.TabIndex = 18;
            this.yagQDisableButton.Text = "Q-switch Disable";
            this.yagQDisableButton.Click += new System.EventHandler(this.yagQDisableButton_Click);
            // 
            // stopYagFlashlampsButton
            // 
            this.stopYagFlashlampsButton.Enabled = false;
            this.stopYagFlashlampsButton.Location = new System.Drawing.Point(144, 103);
            this.stopYagFlashlampsButton.Name = "stopYagFlashlampsButton";
            this.stopYagFlashlampsButton.Size = new System.Drawing.Size(112, 23);
            this.stopYagFlashlampsButton.TabIndex = 16;
            this.stopYagFlashlampsButton.Text = "Stop Flashlamps";
            this.stopYagFlashlampsButton.Click += new System.EventHandler(this.stopYagFlashlampsButton_Click);
            // 
            // yagQEnableButton
            // 
            this.yagQEnableButton.Location = new System.Drawing.Point(272, 103);
            this.yagQEnableButton.Name = "yagQEnableButton";
            this.yagQEnableButton.Size = new System.Drawing.Size(112, 23);
            this.yagQEnableButton.TabIndex = 17;
            this.yagQEnableButton.Text = "Q-switch Enable";
            this.yagQEnableButton.Click += new System.EventHandler(this.yagQEnableButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(96, 24);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(64, 20);
            this.textBox1.TabIndex = 45;
            this.textBox1.Text = "0";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(184, 24);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 40;
            this.button1.Text = "Update";
            // 
            // label67
            // 
            this.label67.Location = new System.Drawing.Point(16, 24);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(80, 23);
            this.label67.TabIndex = 36;
            this.label67.Text = "Voltage (V)";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(726, 24);
            this.menuStrip1.TabIndex = 26;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadParametersToolStripMenuItem,
            this.saveParametersToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadParametersToolStripMenuItem
            // 
            this.loadParametersToolStripMenuItem.Name = "loadParametersToolStripMenuItem";
            this.loadParametersToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.loadParametersToolStripMenuItem.Text = "Load parameters ...";
            this.loadParametersToolStripMenuItem.Click += new System.EventHandler(this.loadParametersToolStripMenuItem_Click);
            // 
            // saveParametersToolStripMenuItem
            // 
            this.saveParametersToolStripMenuItem.Name = "saveParametersToolStripMenuItem";
            this.saveParametersToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.saveParametersToolStripMenuItem.Text = "Save parameters ...";
            this.saveParametersToolStripMenuItem.Click += new System.EventHandler(this.SaveParametersMenuClicked);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitMenuClicked);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(77, 7);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(31, 17);
            this.radioButton1.TabIndex = 32;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "0";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(3, 6);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(31, 17);
            this.radioButton2.TabIndex = 32;
            this.radioButton2.Text = "+";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(42, 7);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(28, 17);
            this.radioButton3.TabIndex = 32;
            this.radioButton3.Text = "-";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // scramblerCheckBox
            // 
            this.scramblerCheckBox.Location = new System.Drawing.Point(156, 142);
            this.scramblerCheckBox.Name = "scramblerCheckBox";
            this.scramblerCheckBox.Size = new System.Drawing.Size(122, 24);
            this.scramblerCheckBox.TabIndex = 31;
            this.scramblerCheckBox.Text = "scrambler TTL";
            this.scramblerCheckBox.CheckedChanged += new System.EventHandler(this.scramblerCheckBox_CheckedChanged);
            // 
            // setScramblerVoltageButton
            // 
            this.setScramblerVoltageButton.Location = new System.Drawing.Point(125, 259);
            this.setScramblerVoltageButton.Name = "setScramblerVoltageButton";
            this.setScramblerVoltageButton.Size = new System.Drawing.Size(131, 23);
            this.setScramblerVoltageButton.TabIndex = 33;
            this.setScramblerVoltageButton.Text = "Set scrambler voltage";
            this.setScramblerVoltageButton.Click += new System.EventHandler(this.setScramblerVoltageButton_Click);
            // 
            // label74
            // 
            this.label74.Location = new System.Drawing.Point(326, 253);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(114, 23);
            this.label74.TabIndex = 32;
            this.label74.Text = "scrambler voltage (V)";
            // 
            // scramblerVoltageTextBox
            // 
            this.scramblerVoltageTextBox.Location = new System.Drawing.Point(448, 249);
            this.scramblerVoltageTextBox.Name = "scramblerVoltageTextBox";
            this.scramblerVoltageTextBox.Size = new System.Drawing.Size(34, 20);
            this.scramblerVoltageTextBox.TabIndex = 31;
            this.scramblerVoltageTextBox.Text = "0";
            // 
            // ControlWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(726, 636);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ControlWindow";
            this.Text = "EDM Hardware Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WindowClosing);
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.switchingLED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.legend1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leakageGraph)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
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
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.groupBox19.ResumeLayout(false);
            this.groupBox19.PerformLayout();
            this.groupBox18.ResumeLayout(false);
            this.groupBox18.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.groupBox17.ResumeLayout(false);
            this.groupBox17.PerformLayout();
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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

        private void scramblerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetScramblerTTL(scramblerCheckBox.Checked);
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

        private void SteppingBBoxBiasUpdateButton_Click(object sender, EventArgs e)
        {
            controller.SetSteppingBBiasVoltage();
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

        private void setScramblerVoltageButton_Click(object sender, EventArgs e)
        {
            controller.SetScramblerVoltage();
        }
        
        private void calibrateIMonitorButton_Click(object sender, EventArgs e)
        {
            controller.CalibrateIMonitors();
        }

         private void rfFrequencyUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateRFFrequencyMonitor();
        }

        private void rfPowerUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateRFPowerMonitor();
        }


        private void ControlWindow_Load(object sender, EventArgs e)
        {
            controller.WindowLoaded();
        }

        private void startIMonitorPollButton_Click(object sender, EventArgs e)
        {
            controller.StartIMonitorPoll();
        }

        private void stopIMonitorPollButton_Click(object sender, EventArgs e)
        {
            controller.StopIMonitorPoll();
        }

        private void TargetStepButton_Click(object sender, EventArgs e)
        {
            controller.StepTarget();
        }

        private void UpdateFLPZTVButton_Click(object sender, EventArgs e)
        {
            controller.UpdateFLPZTV();
        }

        private void I2AOMFreqUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateI2AOMFreqMonitor();
        }

        private void loadParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadParametersWithDialog();
        }

        private void SaveParametersMenuClicked(object sender, EventArgs e)
        {
            controller.SaveParametersWithDialog();
        }

        private void ExitMenuClicked(object sender, EventArgs e)
        {
            Close();
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

        public void SetLED(NationalInstruments.UI.WindowsForms.Led led, bool val)
        {
            led.Invoke(new SetLedDelegate(SetLedHelper), new object[] { led, val });
        }
        private delegate void SetLedDelegate(NationalInstruments.UI.WindowsForms.Led led, bool val);
        private void SetLedHelper(NationalInstruments.UI.WindowsForms.Led led, bool val)
        {
            led.Value = val;
        }

        public void EnableControl(Control control, bool enabled)
        {
            control.Invoke(new EnableControlDelegate(EnableControlHelper), new object[] { control, enabled });
        }
        private delegate void EnableControlDelegate(Control control, bool enabled);
        private void EnableControlHelper(Control control, bool enabled)
        {
            control.Enabled = enabled;
        }

        private delegate void PlotYDelegate(double[] y);
        public void PlotYAppend(Graph graph, WaveformPlot plot, double[] y)
        {
            graph.Invoke(new PlotYDelegate(plot.PlotYAppend), new Object[] { y });
        }
 
        #endregion

        private void WindowClosing(object sender, FormClosingEventArgs e)
        {
            controller.WindowClosing();
        }

    }
}
