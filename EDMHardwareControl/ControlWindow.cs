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
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage5;
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
        public TextBox textBox1;
        private Button button1;
        private Label label67;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem saveParametersToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        public RadioButton radioButton1;
        public RadioButton radioButton2;
        public RadioButton radioButton3;
        private ToolStripMenuItem loadParametersToolStripMenuItem;
        public CheckBox scramblerCheckBox;
        public Button setScramblerVoltageButton;
        private Label label74;
        public TextBox scramblerVoltageTextBox;
        private GroupBox groupBox20;
        public TextBox miniFlux1TextBox;
        private Button updateMiniFluxgatesButton;
        private Label label75;
        public TextBox miniFlux2TextBox;
        public TextBox miniFlux3TextBox;
        private Label label77;
        private Label label76;
        private Label label78;
        public TextBox eOvershootHoldTextBox;
        private Label label79;
        public TextBox eOvershootFactorTextBox;
        private TabPage tabPage6;
        private GroupBox groupBox21;
        public CheckBox eManualStateCheckBox;
        private GroupBox groupBox22;
        public CheckBox rfManualStateCheckBox;
        public CheckBox checkBox1;
        private GroupBox groupBox23;
        public CheckBox bManualStateCheckBox;
        private TabPage tabPage7;
        private TextBox alertTextBox;
        private Button clearAlertButton;
        private GroupBox groupBox24;
        private Button updatePiMonitorButton;
        private Label label82;
        public TextBox piMonitor1TextBox;
        public TextBox piMonitor2TextBox;
        public TextBox southV2FSlopeTextBox;
        public TextBox northV2FSlopeTextBox;
        private Label label85;
        private Label label84;
        private TabPage tabPage8;
        public RadioButton radioButton4;
        public RadioButton radioButton5;
        public RadioButton radioButton6;
        private TabPage tabPage9;
        private Label label97;
        public Switch switchScanTTLSwitch;
        private GroupBox groupBox32;
        public TextBox probePolMesAngle;
        private Button updateProbePolMesAngle;
        private Button zeroProbePol;
        private Label label101;
        private Label label102;
        private Button setProbePolAngle;
        public TextBox probePolSetAngle;
        private Label label104;
        private Label label103;
        private Label label107;
        private Label label106;
        private Label label105;
        private GroupBox groupBox33;
        public Switch probePolModeSelectSwitch;
        public TrackBar probePolVoltTrackBar;
        public Button probePolVoltStopButton;
        private GroupBox groupBox34;
        private Label label108;
        private Label label109;
        public TextBox pumpPolMesAngle;
        private Button updatePumpPolMesAngle;
        private Button zeroPumpPol;
        private Label label110;
        private GroupBox groupBox35;
        public Button pumpPolVoltStopButton;
        public TrackBar pumpPolVoltTrackBar;
        private Label label111;
        private Label label112;
        public TextBox pumpPolSetAngle;
        private Label label113;
        private Label label114;
        private Button setPumpPolAngle;
        public Switch pumpPolModeSelectSwitch;
        private Button automaticBiasCalcButton;
        private Label label123;
        public TextBox probeBacklashTextBox;
        private Label label124;
        public TextBox pumpBacklashTextBox;
        private TabPage tabPage10;
        private GroupBox groupBox37;
        public WaveformGraph I2ErrorSigGraph;
        public WaveformPlot I2ErrorSigPlot;
        private XAxis xAxis3;
        private YAxis yAxis3;
        public TextBox I2ErrorPollPeriodTextBox;
        public Button updateI2ErrorSigButton;
        public TextBox I2ErrorSigTextBox;
        public Button stopI2ErrorSigPollButton;
        private Label label80;
        public Button startI2ErrorSigPollButton;
        private GroupBox groupBox19;
        public TrackBar I2BiasVoltageTrackBar;
        private Button UpdateI2BiasVoltage;
        public TextBox I2BiasVoltageTextBox;
        private Button setDCFMtoGuess;
        private Button Copyrf2f;
        private Button Copyrf1f;
        public TextBox rf2fCentreGuessTextBox;
        public TextBox rf1fCentreGuessTextBox;
        private Button setAttunatorsToGuesses;
        private Button Copyrf2a;
        private Button Copyrf1a;
        public TextBox rf2aCentreGuessTextBox;
        public TextBox rf1aCentreGuessTextBox;
        private Label label81;
        private Label label125;
        private Label label126;
        private Label label127;
        private Label label129;
        public TextBox currentMonitorSampleLengthTextBox;
        private Label label128;
        private Label label130;
        public TextBox northIMonitorErrorTextBox;
        public TextBox southIMonitorErrorTextBox;
        private Label label131;
        private Button clearIMonitorButton;
        public TextBox piFlipMonTextBox;
        private Button UpdatePiFlipMonButton;
        private Label label132;
        private Label label133;
        public CheckBox logCurrentDataCheckBox;
        private TabPage tabPage11;
        private GroupBox groupBox25;
        public TrackBar pumpAOMTrackBar;
        private Panel panel7;
        public RadioButton pumpAOMStepZeroButton;
        public RadioButton pumpAOMStepPlusButton;
        public RadioButton pumpAOMStepMinusButton;
        public TextBox pumpAOMStepTextBox;
        private Label label99;
        public TextBox pumpAOMVoltageTextBox;
        private Button updatePumpAOMButton;
        private Label label100;
        public TextBox pumpAOMFreqStepTextBox;
        private Label label88;
        public TextBox pumpAOMFreqPlusTextBox;
        public TextBox pumpAOMFreqCentreTextBox;
        private Label label95;
        public TextBox pumpAOMFreqMinusTextBox;
        private Label label96;
        private Button pumpAOMFreqUpdateButton;
        private Label label98;
        private GroupBox groupBox11;
        private Button updateLaserPhotodiodesButton;
        public TextBox pumpMonitorTextBox;
        public TextBox pump2MonitorTextBox;
        public TextBox probeMonitorTextBox;
        private Label label29;
        private Label label30;
        private Label label31;
        private GroupBox groupBox10;
        public CheckBox argonShutterCheckBox;
        private Label label32;
        private GroupBox groupBox18;
        public TextBox probeAOMFreqStepTextBox;
        private Label label73;
        public TextBox probeAOMFreqMinusTextBox;
        public TextBox probeAOMFreqCentreTextBox;
        private Label label71;
        public TextBox probeAOMFreqPlusTextBox;
        private Label label72;
        private Button probeAOMFreqUpdateButton;
        private Label label69;
        public TrackBar probeAOMtrackBar;
        private Panel panel5;
        public RadioButton probeAOMStepZeroButton;
        public RadioButton probeAOMStepPlusButton;
        public RadioButton probeAOMMinusButton;
        public TextBox probeAOMStepTextBox;
        private Label label70;
        public TextBox probeAOMVTextBox;
        private Button UpdateProbeAOMButton;
        private Label label68;
        //uWave Control
        private TabPage tabPage12;
        private GroupBox groupBox161MHzVCO;
        public TrackBar VCO161AmpTrackBar;
        public TextBox VCO161FreqTextBox;
        private Label label137;
        public TextBox VCO161AmpVoltageTextBox;
        private Button VCO161UpdateButton;
        private Label label138;
        private GroupBox groupBox40;
        public TrackBar VCO155AmpTrackBar;
        public TextBox VCO155FreqVoltageTextBox;
        private Label label135;
        public TextBox VCO155AmpVoltageTextBox;
        private Button VCO155UpdateButton;
        private Label label136;
        private GroupBox groupBox39;
        public TrackBar VCO30AmpTrackBar;
        public TextBox VCO30FreqVoltageTextBox;
        private Label label83;
        public TextBox VCO30AmpVoltageTextBox;
        private Button VCO30UpdateButton;
        private Label label134;
        public TrackBar VCO161FreqTrackBar;
        private Label label140;
        private Label label139;
        public TrackBar VCO30FreqTrackBar;
        private Label label142;
        private Label label141;
        private Label label144;
        private Label label143;
        public TrackBar VCO155FreqTrackBar;
        private Button VCO161AmpStepMinusButton;
        public TextBox VCO161AmpStepTextBox;
        private Label label145;
        private Button VCO161AmpStepPlusButton;
        private Button VCO161FreqStepPlusButton;
        public TextBox VCO161FreqStepTextBox;
        private Label label146;
        private Button VCO161FreqStepMinusButton;
        private Button VCO30FreqStepMinusButton;
        private Button VCO30FreqStepPlusButton;
        public TextBox VCO30FreqStepTextBox;
        private Label label148;
        private Button VCO30AmpStepMinusButton;
        private Button VCO30AmpStepPlusButton;
        public TextBox VCO30AmpStepTextBox;
        private Label label147;
        private Button VCO155FreqStepMinusButton;
        private Button VCO155FreqStepPlusButton;
        private Button VCO155AmpStepMinusButton;
        private Button VCO155AmpStepPlusButton;
        public TextBox VCO155FreqStepTextBox;
        private Label label150;
        public TextBox VCO155AmpStepTextBox;
        private Label label149;
        //uWave control
        private TabPage tabPage13;
        private Button uWaveDCFMMinusButton;
        public TextBox uWaveDCFMStepTextBox;
        private Label label152;
        private Button uWaveDCFMPlusButton;
        public TrackBar pumpMixerVoltageTrackBar;
        private Label label154;
        public TrackBar uWaveDCFMTrackBar;
        public TextBox pumpMixerVoltageTextBox;
        private Label label155;
        public TextBox uWaveDCFMTextBox;
        private Button uWaveUpdateButton;
        private Label label156;
        private GroupBox groupBox42;
        public CheckBox pumpRFCheckBox;
        private GroupBox groupBox43;
        public CheckBox mwEnableCheckBox;
        public CheckBox rfSensorCheckBox;
        public CheckBox synthSensorConnectCheckBox;
        public CheckBox secondProbeHornCheckBox;
        public CheckBox firstProbeHornCheckBox;
        public CheckBox pumpHornCheckBox;
        public TrackBar topProbeMixerVoltageTrackBar;
        public TextBox topProbeMixerVoltageTextBox;
        public TrackBar bottomProbeMixerVoltageTrackBar;
        public TextBox bottomProbeMixerVoltageTextBox;
        private Button mixerVoltateUpdateButton;
        public CheckBox anapicoEnabledCheckBox;
        private GroupBox anapicofreqcontrol;
        private Button anapicoCwFreqUpdate;
        public TextBox anapicoCwFreqBox;
        private Label cwfreqlabel;
        public CheckBox listSweepEnabledCheckBox;
        private Label label86;
        public TextBox anapicof0FreqTextBox;
        private Label label90;
        private Label label89;
        private Label label87;
        private Label label92;
        private GroupBox groupBox41;
        private Label label93;
        private Label label118;
        public Led topProbeMWf1Indicator;
        private Label label119;
        public Led topProbeMWf0Indicator;
        private Label label116;
        public Led bottomProbeMWf1Indicator;
        private Label label117;
        public Led bottomProbeMWf0Indicator;
        private Label label115;
        public Led pumpMWf1Indicator;
        private Label label94;
        public Led pumpMWf0Indicator;
        private Button getCurrentListButton;
        public TextBox displayCurrentListBox;
        public TextBox anapicof1FreqTextBox;
        private Label label91;
        public CheckBox microwaveStateCheckBox;
        public TextBox topProbeMWDwellOffTextBox;
        public TextBox topProbeMWDwellOnTextBox;
        public TextBox bottomProbeMWDwellOffTextBox;
        public TextBox bottomProbeMWDwellOnTextBox;
        public TextBox pumpMWDwellOffTextBox;
        public TextBox pumpMWDwellOnTextBox;
        private Label label120;
        private GroupBox pressureMonitorGroupBox;
        public CheckBox logPressureDataCheckBox;
        private Button clearPressureMonitorButton;
        private Label label153;
        public TextBox pressureMonitorSampleLengthTextBox;
        private Label label157;
        public Button stopPressureMonitorPollButton;
        private Legend legend2;
        private LegendItem pressurePlotLegend;
        public WaveformPlot pressurePlot;
        private XAxis xAxis2;
        private YAxis yAxis2;
        private Label label161;
        public TextBox pressureMonitorPollPeriodTextBox;
        public Button startPressureMonitorPollButton;
        public WaveformGraph pressureGraph;
        public TextBox pressureMonitorTextBox;
        private Button updatePressureMonitorButton;
        private Label label165;
        private Label label121;
        public TextBox pressureMonitorLogPeriodTextBox;

 


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
            this.label78 = new System.Windows.Forms.Label();
            this.eOvershootHoldTextBox = new System.Windows.Forms.TextBox();
            this.label79 = new System.Windows.Forms.Label();
            this.eOvershootFactorTextBox = new System.Windows.Forms.TextBox();
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox21 = new System.Windows.Forms.GroupBox();
            this.eManualStateCheckBox = new System.Windows.Forms.CheckBox();
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
            this.logCurrentDataCheckBox = new System.Windows.Forms.CheckBox();
            this.clearIMonitorButton = new System.Windows.Forms.Button();
            this.southIMonitorErrorTextBox = new System.Windows.Forms.TextBox();
            this.label131 = new System.Windows.Forms.Label();
            this.label130 = new System.Windows.Forms.Label();
            this.northIMonitorErrorTextBox = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.zeroIMonitorButton = new System.Windows.Forms.Button();
            this.label129 = new System.Windows.Forms.Label();
            this.currentMonitorSampleLengthTextBox = new System.Windows.Forms.TextBox();
            this.label128 = new System.Windows.Forms.Label();
            this.southOffsetIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.northOffsetIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.label85 = new System.Windows.Forms.Label();
            this.label84 = new System.Windows.Forms.Label();
            this.southV2FSlopeTextBox = new System.Windows.Forms.TextBox();
            this.northV2FSlopeTextBox = new System.Windows.Forms.TextBox();
            this.leakageMonitorSlopeTextBox = new System.Windows.Forms.TextBox();
            this.label64 = new System.Windows.Forms.Label();
            this.stopIMonitorPollButton = new System.Windows.Forms.Button();
            this.legend1 = new NationalInstruments.UI.WindowsForms.Legend();
            this.NorthLegendItem = new NationalInstruments.UI.LegendItem();
            this.northLeakagePlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.SouthLegendItem = new NationalInstruments.UI.LegendItem();
            this.southLeakagePlot = new NationalInstruments.UI.WaveformPlot();
            this.label63 = new System.Windows.Forms.Label();
            this.iMonitorPollPeriod = new System.Windows.Forms.TextBox();
            this.startIMonitorPollButton = new System.Windows.Forms.Button();
            this.leakageGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.IMonitorMeasurementLengthTextBox = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.southIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.northIMonitorTextBox = new System.Windows.Forms.TextBox();
            this.updateIMonitorButton = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox24 = new System.Windows.Forms.GroupBox();
            this.piFlipMonTextBox = new System.Windows.Forms.TextBox();
            this.UpdatePiFlipMonButton = new System.Windows.Forms.Button();
            this.piMonitor2TextBox = new System.Windows.Forms.TextBox();
            this.updatePiMonitorButton = new System.Windows.Forms.Button();
            this.label82 = new System.Windows.Forms.Label();
            this.piMonitor1TextBox = new System.Windows.Forms.TextBox();
            this.label132 = new System.Windows.Forms.Label();
            this.label133 = new System.Windows.Forms.Label();
            this.groupBox22 = new System.Windows.Forms.GroupBox();
            this.rfManualStateCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.setDCFMtoGuess = new System.Windows.Forms.Button();
            this.Copyrf2f = new System.Windows.Forms.Button();
            this.Copyrf1f = new System.Windows.Forms.Button();
            this.rf2fCentreGuessTextBox = new System.Windows.Forms.TextBox();
            this.rf1fCentreGuessTextBox = new System.Windows.Forms.TextBox();
            this.setAttunatorsToGuesses = new System.Windows.Forms.Button();
            this.Copyrf2a = new System.Windows.Forms.Button();
            this.Copyrf1a = new System.Windows.Forms.Button();
            this.rf2aCentreGuessTextBox = new System.Windows.Forms.TextBox();
            this.rf1aCentreGuessTextBox = new System.Windows.Forms.TextBox();
            this.rf2StepPowerMon = new System.Windows.Forms.TextBox();
            this.rf2StepFreqMon = new System.Windows.Forms.TextBox();
            this.rf1StepPowerMon = new System.Windows.Forms.TextBox();
            this.rfPowerUpdateButton = new System.Windows.Forms.Button();
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
            this.label52 = new System.Windows.Forms.Label();
            this.rfFrequencyUpdateButton = new System.Windows.Forms.Button();
            this.label51 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.label81 = new System.Windows.Forms.Label();
            this.label125 = new System.Windows.Forms.Label();
            this.label126 = new System.Windows.Forms.Label();
            this.label127 = new System.Windows.Forms.Label();
            this.groupBox16 = new System.Windows.Forms.GroupBox();
            this.synthSensorConnectCheckBox = new System.Windows.Forms.CheckBox();
            this.scramblerCheckBox = new System.Windows.Forms.CheckBox();
            this.attenuatorSelectCheck = new System.Windows.Forms.CheckBox();
            this.phaseFlip2CheckBox = new System.Windows.Forms.CheckBox();
            this.phaseFlip1CheckBox = new System.Windows.Forms.CheckBox();
            this.fmSelectCheck = new System.Windows.Forms.CheckBox();
            this.rfSwitchEnableCheck = new System.Windows.Forms.CheckBox();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.setScramblerVoltageButton = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.rf2FMZeroRB = new System.Windows.Forms.RadioButton();
            this.rf2FMPlusRB = new System.Windows.Forms.RadioButton();
            this.rf2FMMinusRB = new System.Windows.Forms.RadioButton();
            this.label74 = new System.Windows.Forms.Label();
            this.scramblerVoltageTextBox = new System.Windows.Forms.TextBox();
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
            this.groupBox23 = new System.Windows.Forms.GroupBox();
            this.bManualStateCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox20 = new System.Windows.Forms.GroupBox();
            this.miniFlux2TextBox = new System.Windows.Forms.TextBox();
            this.miniFlux3TextBox = new System.Windows.Forms.TextBox();
            this.label77 = new System.Windows.Forms.Label();
            this.label76 = new System.Windows.Forms.Label();
            this.miniFlux1TextBox = new System.Windows.Forms.TextBox();
            this.updateMiniFluxgatesButton = new System.Windows.Forms.Button();
            this.label75 = new System.Windows.Forms.Label();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.automaticBiasCalcButton = new System.Windows.Forms.Button();
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
            this.tabPage11 = new System.Windows.Forms.TabPage();
            this.groupBox25 = new System.Windows.Forms.GroupBox();
            this.pumpAOMTrackBar = new System.Windows.Forms.TrackBar();
            this.panel7 = new System.Windows.Forms.Panel();
            this.pumpAOMStepZeroButton = new System.Windows.Forms.RadioButton();
            this.pumpAOMStepPlusButton = new System.Windows.Forms.RadioButton();
            this.pumpAOMStepMinusButton = new System.Windows.Forms.RadioButton();
            this.pumpAOMStepTextBox = new System.Windows.Forms.TextBox();
            this.label99 = new System.Windows.Forms.Label();
            this.pumpAOMVoltageTextBox = new System.Windows.Forms.TextBox();
            this.updatePumpAOMButton = new System.Windows.Forms.Button();
            this.label100 = new System.Windows.Forms.Label();
            this.pumpAOMFreqStepTextBox = new System.Windows.Forms.TextBox();
            this.label88 = new System.Windows.Forms.Label();
            this.pumpAOMFreqPlusTextBox = new System.Windows.Forms.TextBox();
            this.pumpAOMFreqCentreTextBox = new System.Windows.Forms.TextBox();
            this.label95 = new System.Windows.Forms.Label();
            this.pumpAOMFreqMinusTextBox = new System.Windows.Forms.TextBox();
            this.label96 = new System.Windows.Forms.Label();
            this.pumpAOMFreqUpdateButton = new System.Windows.Forms.Button();
            this.label98 = new System.Windows.Forms.Label();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.updateLaserPhotodiodesButton = new System.Windows.Forms.Button();
            this.pumpMonitorTextBox = new System.Windows.Forms.TextBox();
            this.pump2MonitorTextBox = new System.Windows.Forms.TextBox();
            this.probeMonitorTextBox = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.argonShutterCheckBox = new System.Windows.Forms.CheckBox();
            this.label32 = new System.Windows.Forms.Label();
            this.groupBox18 = new System.Windows.Forms.GroupBox();
            this.probeAOMFreqStepTextBox = new System.Windows.Forms.TextBox();
            this.label73 = new System.Windows.Forms.Label();
            this.probeAOMFreqMinusTextBox = new System.Windows.Forms.TextBox();
            this.probeAOMFreqCentreTextBox = new System.Windows.Forms.TextBox();
            this.label71 = new System.Windows.Forms.Label();
            this.probeAOMFreqPlusTextBox = new System.Windows.Forms.TextBox();
            this.label72 = new System.Windows.Forms.Label();
            this.probeAOMFreqUpdateButton = new System.Windows.Forms.Button();
            this.label69 = new System.Windows.Forms.Label();
            this.probeAOMtrackBar = new System.Windows.Forms.TrackBar();
            this.panel5 = new System.Windows.Forms.Panel();
            this.probeAOMStepZeroButton = new System.Windows.Forms.RadioButton();
            this.probeAOMStepPlusButton = new System.Windows.Forms.RadioButton();
            this.probeAOMMinusButton = new System.Windows.Forms.RadioButton();
            this.probeAOMStepTextBox = new System.Windows.Forms.TextBox();
            this.label70 = new System.Windows.Forms.Label();
            this.probeAOMVTextBox = new System.Windows.Forms.TextBox();
            this.UpdateProbeAOMButton = new System.Windows.Forms.Button();
            this.label68 = new System.Windows.Forms.Label();
            this.tabPage8 = new System.Windows.Forms.TabPage();
            this.tabPage13 = new System.Windows.Forms.TabPage();
            this.label93 = new System.Windows.Forms.Label();
            this.anapicofreqcontrol = new System.Windows.Forms.GroupBox();
            this.label120 = new System.Windows.Forms.Label();
            this.label118 = new System.Windows.Forms.Label();
            this.topProbeMWf1Indicator = new NationalInstruments.UI.WindowsForms.Led();
            this.label119 = new System.Windows.Forms.Label();
            this.topProbeMWf0Indicator = new NationalInstruments.UI.WindowsForms.Led();
            this.label116 = new System.Windows.Forms.Label();
            this.bottomProbeMWf1Indicator = new NationalInstruments.UI.WindowsForms.Led();
            this.label117 = new System.Windows.Forms.Label();
            this.bottomProbeMWf0Indicator = new NationalInstruments.UI.WindowsForms.Led();
            this.label115 = new System.Windows.Forms.Label();
            this.pumpMWf1Indicator = new NationalInstruments.UI.WindowsForms.Led();
            this.label94 = new System.Windows.Forms.Label();
            this.pumpMWf0Indicator = new NationalInstruments.UI.WindowsForms.Led();
            this.getCurrentListButton = new System.Windows.Forms.Button();
            this.displayCurrentListBox = new System.Windows.Forms.TextBox();
            this.anapicof1FreqTextBox = new System.Windows.Forms.TextBox();
            this.label91 = new System.Windows.Forms.Label();
            this.microwaveStateCheckBox = new System.Windows.Forms.CheckBox();
            this.topProbeMWDwellOffTextBox = new System.Windows.Forms.TextBox();
            this.topProbeMWDwellOnTextBox = new System.Windows.Forms.TextBox();
            this.bottomProbeMWDwellOffTextBox = new System.Windows.Forms.TextBox();
            this.bottomProbeMWDwellOnTextBox = new System.Windows.Forms.TextBox();
            this.pumpMWDwellOffTextBox = new System.Windows.Forms.TextBox();
            this.pumpMWDwellOnTextBox = new System.Windows.Forms.TextBox();
            this.label92 = new System.Windows.Forms.Label();
            this.anapicof0FreqTextBox = new System.Windows.Forms.TextBox();
            this.label90 = new System.Windows.Forms.Label();
            this.label89 = new System.Windows.Forms.Label();
            this.label87 = new System.Windows.Forms.Label();
            this.label86 = new System.Windows.Forms.Label();
            this.listSweepEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.anapicoCwFreqUpdate = new System.Windows.Forms.Button();
            this.anapicoEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.anapicoCwFreqBox = new System.Windows.Forms.TextBox();
            this.cwfreqlabel = new System.Windows.Forms.Label();
            this.groupBox43 = new System.Windows.Forms.GroupBox();
            this.bottomProbeMixerVoltageTextBox = new System.Windows.Forms.TextBox();
            this.mixerVoltateUpdateButton = new System.Windows.Forms.Button();
            this.topProbeMixerVoltageTrackBar = new System.Windows.Forms.TrackBar();
            this.topProbeMixerVoltageTextBox = new System.Windows.Forms.TextBox();
            this.bottomProbeMixerVoltageTrackBar = new System.Windows.Forms.TrackBar();
            this.secondProbeHornCheckBox = new System.Windows.Forms.CheckBox();
            this.firstProbeHornCheckBox = new System.Windows.Forms.CheckBox();
            this.pumpMixerVoltageTrackBar = new System.Windows.Forms.TrackBar();
            this.pumpHornCheckBox = new System.Windows.Forms.CheckBox();
            this.mwEnableCheckBox = new System.Windows.Forms.CheckBox();
            this.label155 = new System.Windows.Forms.Label();
            this.pumpMixerVoltageTextBox = new System.Windows.Forms.TextBox();
            this.groupBox41 = new System.Windows.Forms.GroupBox();
            this.uWaveDCFMMinusButton = new System.Windows.Forms.Button();
            this.uWaveDCFMStepTextBox = new System.Windows.Forms.TextBox();
            this.label152 = new System.Windows.Forms.Label();
            this.uWaveDCFMPlusButton = new System.Windows.Forms.Button();
            this.label154 = new System.Windows.Forms.Label();
            this.uWaveDCFMTrackBar = new System.Windows.Forms.TrackBar();
            this.uWaveUpdateButton = new System.Windows.Forms.Button();
            this.uWaveDCFMTextBox = new System.Windows.Forms.TextBox();
            this.label156 = new System.Windows.Forms.Label();
            this.tabPage12 = new System.Windows.Forms.TabPage();
            this.groupBox42 = new System.Windows.Forms.GroupBox();
            this.rfSensorCheckBox = new System.Windows.Forms.CheckBox();
            this.pumpRFCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox40 = new System.Windows.Forms.GroupBox();
            this.VCO155FreqStepMinusButton = new System.Windows.Forms.Button();
            this.VCO155FreqStepPlusButton = new System.Windows.Forms.Button();
            this.VCO155AmpStepMinusButton = new System.Windows.Forms.Button();
            this.VCO155AmpStepPlusButton = new System.Windows.Forms.Button();
            this.VCO155FreqStepTextBox = new System.Windows.Forms.TextBox();
            this.label150 = new System.Windows.Forms.Label();
            this.VCO155AmpStepTextBox = new System.Windows.Forms.TextBox();
            this.label149 = new System.Windows.Forms.Label();
            this.label144 = new System.Windows.Forms.Label();
            this.label143 = new System.Windows.Forms.Label();
            this.VCO155FreqTrackBar = new System.Windows.Forms.TrackBar();
            this.VCO155AmpTrackBar = new System.Windows.Forms.TrackBar();
            this.VCO155FreqVoltageTextBox = new System.Windows.Forms.TextBox();
            this.label135 = new System.Windows.Forms.Label();
            this.VCO155AmpVoltageTextBox = new System.Windows.Forms.TextBox();
            this.VCO155UpdateButton = new System.Windows.Forms.Button();
            this.label136 = new System.Windows.Forms.Label();
            this.groupBox39 = new System.Windows.Forms.GroupBox();
            this.VCO30FreqStepMinusButton = new System.Windows.Forms.Button();
            this.VCO30FreqStepPlusButton = new System.Windows.Forms.Button();
            this.VCO30FreqStepTextBox = new System.Windows.Forms.TextBox();
            this.label148 = new System.Windows.Forms.Label();
            this.VCO30AmpStepMinusButton = new System.Windows.Forms.Button();
            this.VCO30AmpStepPlusButton = new System.Windows.Forms.Button();
            this.VCO30AmpStepTextBox = new System.Windows.Forms.TextBox();
            this.label147 = new System.Windows.Forms.Label();
            this.VCO30FreqTrackBar = new System.Windows.Forms.TrackBar();
            this.label142 = new System.Windows.Forms.Label();
            this.label141 = new System.Windows.Forms.Label();
            this.VCO30AmpTrackBar = new System.Windows.Forms.TrackBar();
            this.VCO30FreqVoltageTextBox = new System.Windows.Forms.TextBox();
            this.label83 = new System.Windows.Forms.Label();
            this.VCO30AmpVoltageTextBox = new System.Windows.Forms.TextBox();
            this.VCO30UpdateButton = new System.Windows.Forms.Button();
            this.label134 = new System.Windows.Forms.Label();
            this.groupBox161MHzVCO = new System.Windows.Forms.GroupBox();
            this.VCO161FreqStepMinusButton = new System.Windows.Forms.Button();
            this.VCO161FreqStepPlusButton = new System.Windows.Forms.Button();
            this.VCO161FreqStepTextBox = new System.Windows.Forms.TextBox();
            this.label146 = new System.Windows.Forms.Label();
            this.VCO161AmpStepMinusButton = new System.Windows.Forms.Button();
            this.VCO161AmpStepTextBox = new System.Windows.Forms.TextBox();
            this.label145 = new System.Windows.Forms.Label();
            this.VCO161AmpStepPlusButton = new System.Windows.Forms.Button();
            this.VCO161FreqTrackBar = new System.Windows.Forms.TrackBar();
            this.label140 = new System.Windows.Forms.Label();
            this.label139 = new System.Windows.Forms.Label();
            this.VCO161AmpTrackBar = new System.Windows.Forms.TrackBar();
            this.VCO161FreqTextBox = new System.Windows.Forms.TextBox();
            this.label137 = new System.Windows.Forms.Label();
            this.VCO161AmpVoltageTextBox = new System.Windows.Forms.TextBox();
            this.VCO161UpdateButton = new System.Windows.Forms.Button();
            this.label138 = new System.Windows.Forms.Label();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.groupBox34 = new System.Windows.Forms.GroupBox();
            this.label108 = new System.Windows.Forms.Label();
            this.label109 = new System.Windows.Forms.Label();
            this.pumpPolMesAngle = new System.Windows.Forms.TextBox();
            this.updatePumpPolMesAngle = new System.Windows.Forms.Button();
            this.zeroPumpPol = new System.Windows.Forms.Button();
            this.label110 = new System.Windows.Forms.Label();
            this.groupBox35 = new System.Windows.Forms.GroupBox();
            this.label124 = new System.Windows.Forms.Label();
            this.pumpBacklashTextBox = new System.Windows.Forms.TextBox();
            this.pumpPolVoltStopButton = new System.Windows.Forms.Button();
            this.pumpPolVoltTrackBar = new System.Windows.Forms.TrackBar();
            this.label111 = new System.Windows.Forms.Label();
            this.label112 = new System.Windows.Forms.Label();
            this.pumpPolSetAngle = new System.Windows.Forms.TextBox();
            this.label113 = new System.Windows.Forms.Label();
            this.label114 = new System.Windows.Forms.Label();
            this.setPumpPolAngle = new System.Windows.Forms.Button();
            this.pumpPolModeSelectSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.groupBox32 = new System.Windows.Forms.GroupBox();
            this.label106 = new System.Windows.Forms.Label();
            this.label105 = new System.Windows.Forms.Label();
            this.probePolMesAngle = new System.Windows.Forms.TextBox();
            this.updateProbePolMesAngle = new System.Windows.Forms.Button();
            this.zeroProbePol = new System.Windows.Forms.Button();
            this.label101 = new System.Windows.Forms.Label();
            this.groupBox33 = new System.Windows.Forms.GroupBox();
            this.label123 = new System.Windows.Forms.Label();
            this.probeBacklashTextBox = new System.Windows.Forms.TextBox();
            this.probePolVoltStopButton = new System.Windows.Forms.Button();
            this.probePolVoltTrackBar = new System.Windows.Forms.TrackBar();
            this.label107 = new System.Windows.Forms.Label();
            this.label102 = new System.Windows.Forms.Label();
            this.probePolSetAngle = new System.Windows.Forms.TextBox();
            this.label103 = new System.Windows.Forms.Label();
            this.label104 = new System.Windows.Forms.Label();
            this.setProbePolAngle = new System.Windows.Forms.Button();
            this.probePolModeSelectSwitch = new NationalInstruments.UI.WindowsForms.Switch();
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
            this.tabPage9 = new System.Windows.Forms.TabPage();
            this.switchScanTTLSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.label97 = new System.Windows.Forms.Label();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.clearAlertButton = new System.Windows.Forms.Button();
            this.alertTextBox = new System.Windows.Forms.TextBox();
            this.tabPage10 = new System.Windows.Forms.TabPage();
            this.groupBox19 = new System.Windows.Forms.GroupBox();
            this.UpdateI2BiasVoltage = new System.Windows.Forms.Button();
            this.I2BiasVoltageTextBox = new System.Windows.Forms.TextBox();
            this.I2BiasVoltageTrackBar = new System.Windows.Forms.TrackBar();
            this.groupBox37 = new System.Windows.Forms.GroupBox();
            this.I2ErrorPollPeriodTextBox = new System.Windows.Forms.TextBox();
            this.updateI2ErrorSigButton = new System.Windows.Forms.Button();
            this.I2ErrorSigTextBox = new System.Windows.Forms.TextBox();
            this.I2ErrorSigGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.I2ErrorSigPlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.stopI2ErrorSigPollButton = new System.Windows.Forms.Button();
            this.label80 = new System.Windows.Forms.Label();
            this.startI2ErrorSigPollButton = new System.Windows.Forms.Button();
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
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.pressureMonitorGroupBox = new System.Windows.Forms.GroupBox();
            this.logPressureDataCheckBox = new System.Windows.Forms.CheckBox();
            this.clearPressureMonitorButton = new System.Windows.Forms.Button();
            this.label153 = new System.Windows.Forms.Label();
            this.pressureMonitorSampleLengthTextBox = new System.Windows.Forms.TextBox();
            this.label157 = new System.Windows.Forms.Label();
            this.stopPressureMonitorPollButton = new System.Windows.Forms.Button();
            this.legend2 = new NationalInstruments.UI.WindowsForms.Legend();
            this.pressurePlotLegend = new NationalInstruments.UI.LegendItem();
            this.pressurePlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.label161 = new System.Windows.Forms.Label();
            this.pressureMonitorPollPeriodTextBox = new System.Windows.Forms.TextBox();
            this.startPressureMonitorPollButton = new System.Windows.Forms.Button();
            this.pressureGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.pressureMonitorTextBox = new System.Windows.Forms.TextBox();
            this.updatePressureMonitorButton = new System.Windows.Forms.Button();
            this.label165 = new System.Windows.Forms.Label();
            this.label121 = new System.Windows.Forms.Label();
            this.pressureMonitorLogPeriodTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.switchingLED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox21.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.legend1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leakageGraph)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupBox24.SuspendLayout();
            this.groupBox22.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox16.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox23.SuspendLayout();
            this.groupBox20.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.tabPage11.SuspendLayout();
            this.groupBox25.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pumpAOMTrackBar)).BeginInit();
            this.panel7.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox18.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.probeAOMtrackBar)).BeginInit();
            this.panel5.SuspendLayout();
            this.tabPage13.SuspendLayout();
            this.anapicofreqcontrol.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topProbeMWf1Indicator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.topProbeMWf0Indicator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomProbeMWf1Indicator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomProbeMWf0Indicator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pumpMWf1Indicator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pumpMWf0Indicator)).BeginInit();
            this.groupBox43.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topProbeMixerVoltageTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomProbeMixerVoltageTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pumpMixerVoltageTrackBar)).BeginInit();
            this.groupBox41.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uWaveDCFMTrackBar)).BeginInit();
            this.tabPage12.SuspendLayout();
            this.groupBox42.SuspendLayout();
            this.groupBox40.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VCO155FreqTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VCO155AmpTrackBar)).BeginInit();
            this.groupBox39.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VCO30FreqTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VCO30AmpTrackBar)).BeginInit();
            this.groupBox161MHzVCO.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VCO161FreqTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VCO161AmpTrackBar)).BeginInit();
            this.tabPage6.SuspendLayout();
            this.groupBox34.SuspendLayout();
            this.groupBox35.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pumpPolVoltTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pumpPolModeSelectSwitch)).BeginInit();
            this.groupBox32.SuspendLayout();
            this.groupBox33.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.probePolVoltTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.probePolModeSelectSwitch)).BeginInit();
            this.tabPage5.SuspendLayout();
            this.groupBox17.SuspendLayout();
            this.groupBox15.SuspendLayout();
            this.tabPage9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.switchScanTTLSwitch)).BeginInit();
            this.tabPage7.SuspendLayout();
            this.tabPage10.SuspendLayout();
            this.groupBox19.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.I2BiasVoltageTrackBar)).BeginInit();
            this.groupBox37.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.I2ErrorSigGraph)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.pressureMonitorGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.legend2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pressureGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label78);
            this.groupBox2.Controls.Add(this.eOvershootHoldTextBox);
            this.groupBox2.Controls.Add(this.label79);
            this.groupBox2.Controls.Add(this.eOvershootFactorTextBox);
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
            // label78
            // 
            this.label78.Location = new System.Drawing.Point(25, 215);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(115, 23);
            this.label78.TabIndex = 52;
            this.label78.Text = "Settle time (s)";
            // 
            // eOvershootHoldTextBox
            // 
            this.eOvershootHoldTextBox.Location = new System.Drawing.Point(145, 170);
            this.eOvershootHoldTextBox.Name = "eOvershootHoldTextBox";
            this.eOvershootHoldTextBox.Size = new System.Drawing.Size(64, 20);
            this.eOvershootHoldTextBox.TabIndex = 50;
            this.eOvershootHoldTextBox.Text = "1";
            // 
            // label79
            // 
            this.label79.Location = new System.Drawing.Point(25, 194);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(115, 23);
            this.label79.TabIndex = 51;
            this.label79.Text = "Overshoot factor";
            // 
            // eOvershootFactorTextBox
            // 
            this.eOvershootFactorTextBox.Location = new System.Drawing.Point(145, 191);
            this.eOvershootFactorTextBox.Name = "eOvershootFactorTextBox";
            this.eOvershootFactorTextBox.Size = new System.Drawing.Size(64, 20);
            this.eOvershootFactorTextBox.TabIndex = 49;
            this.eOvershootFactorTextBox.Text = "2";
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
            this.label62.Location = new System.Drawing.Point(25, 131);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(115, 23);
            this.label62.TabIndex = 47;
            this.label62.Text = "Switch time (s)";
            // 
            // eSwitchTimeTextBox
            // 
            this.eSwitchTimeTextBox.Location = new System.Drawing.Point(145, 128);
            this.eSwitchTimeTextBox.Name = "eSwitchTimeTextBox";
            this.eSwitchTimeTextBox.Size = new System.Drawing.Size(64, 20);
            this.eSwitchTimeTextBox.TabIndex = 3;
            this.eSwitchTimeTextBox.Text = "1";
            // 
            // label61
            // 
            this.label61.Location = new System.Drawing.Point(25, 110);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(115, 23);
            this.label61.TabIndex = 45;
            this.label61.Text = "Bleed time (s)";
            // 
            // eBleedTimeTextBox
            // 
            this.eBleedTimeTextBox.Location = new System.Drawing.Point(145, 107);
            this.eBleedTimeTextBox.Name = "eBleedTimeTextBox";
            this.eBleedTimeTextBox.Size = new System.Drawing.Size(64, 20);
            this.eBleedTimeTextBox.TabIndex = 2;
            this.eBleedTimeTextBox.Text = "0.01";
            // 
            // label60
            // 
            this.label60.Location = new System.Drawing.Point(25, 173);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(115, 23);
            this.label60.TabIndex = 45;
            this.label60.Text = "Overshoot hold (s)";
            // 
            // label57
            // 
            this.label57.Location = new System.Drawing.Point(25, 89);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(115, 23);
            this.label57.TabIndex = 41;
            this.label57.Text = "Ramp down delay (s)";
            // 
            // eRampUpDelayTextBox
            // 
            this.eRampUpDelayTextBox.Location = new System.Drawing.Point(145, 212);
            this.eRampUpDelayTextBox.Name = "eRampUpDelayTextBox";
            this.eRampUpDelayTextBox.Size = new System.Drawing.Size(64, 20);
            this.eRampUpDelayTextBox.TabIndex = 5;
            this.eRampUpDelayTextBox.Text = "1";
            // 
            // label58
            // 
            this.label58.Location = new System.Drawing.Point(25, 68);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(115, 23);
            this.label58.TabIndex = 40;
            this.label58.Text = "Ramp down time (s)";
            // 
            // eRampDownDelayTextBox
            // 
            this.eRampDownDelayTextBox.Location = new System.Drawing.Point(145, 86);
            this.eRampDownDelayTextBox.Name = "eRampDownDelayTextBox";
            this.eRampDownDelayTextBox.Size = new System.Drawing.Size(64, 20);
            this.eRampDownDelayTextBox.TabIndex = 1;
            this.eRampDownDelayTextBox.Text = "3";
            // 
            // label59
            // 
            this.label59.Location = new System.Drawing.Point(25, 152);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(115, 23);
            this.label59.TabIndex = 43;
            this.label59.Text = "Ramp up time (s)";
            // 
            // eRampDownTimeTextBox
            // 
            this.eRampDownTimeTextBox.Location = new System.Drawing.Point(145, 65);
            this.eRampDownTimeTextBox.Name = "eRampDownTimeTextBox";
            this.eRampDownTimeTextBox.Size = new System.Drawing.Size(64, 20);
            this.eRampDownTimeTextBox.TabIndex = 0;
            this.eRampDownTimeTextBox.Text = "2";
            // 
            // eRampUpTimeTextBox
            // 
            this.eRampUpTimeTextBox.Location = new System.Drawing.Point(145, 149);
            this.eRampUpTimeTextBox.Name = "eRampUpTimeTextBox";
            this.eRampUpTimeTextBox.Size = new System.Drawing.Size(64, 20);
            this.eRampUpTimeTextBox.TabIndex = 4;
            this.eRampUpTimeTextBox.Text = "2";
            // 
            // fieldsOffButton
            // 
            this.fieldsOffButton.Enabled = false;
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
            this.label1.Location = new System.Drawing.Point(6, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 23);
            this.label1.TabIndex = 23;
            this.label1.Text = "Green synth DC FM (kHz)";
            // 
            // greenDCFMBox
            // 
            this.greenDCFMBox.Location = new System.Drawing.Point(168, 85);
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
            this.label7.Location = new System.Drawing.Point(6, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(122, 23);
            this.label7.TabIndex = 13;
            this.label7.Text = "Green synth amplitude";
            // 
            // greenOnAmpBox
            // 
            this.greenOnAmpBox.Location = new System.Drawing.Point(168, 53);
            this.greenOnAmpBox.Name = "greenOnAmpBox";
            this.greenOnAmpBox.Size = new System.Drawing.Size(64, 20);
            this.greenOnAmpBox.TabIndex = 1;
            this.greenOnAmpBox.Text = "-6";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(6, 24);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(122, 23);
            this.label8.TabIndex = 11;
            this.label8.Text = "Green synth frequency";
            // 
            // greenOnFreqBox
            // 
            this.greenOnFreqBox.Location = new System.Drawing.Point(168, 21);
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
            this.groupBox5.Size = new System.Drawing.Size(376, 56);
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
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage11);
            this.tabControl.Controls.Add(this.tabPage8);
            this.tabControl.Controls.Add(this.tabPage13);
            this.tabControl.Controls.Add(this.tabPage12);
            this.tabControl.Controls.Add(this.tabPage6);
            this.tabControl.Controls.Add(this.tabPage5);
            this.tabControl.Controls.Add(this.tabPage9);
            this.tabControl.Controls.Add(this.tabPage7);
            this.tabControl.Controls.Add(this.tabPage10);
            this.tabControl.Location = new System.Drawing.Point(12, 27);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(705, 601);
            this.tabControl.TabIndex = 25;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Transparent;
            this.tabPage1.Controls.Add(this.groupBox21);
            this.tabPage1.Controls.Add(this.groupBox13);
            this.tabPage1.Controls.Add(this.groupBox6);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.groupBox7);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(697, 575);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "E-field";
            // 
            // groupBox21
            // 
            this.groupBox21.Controls.Add(this.eManualStateCheckBox);
            this.groupBox21.Location = new System.Drawing.Point(17, 231);
            this.groupBox21.Name = "groupBox21";
            this.groupBox21.Size = new System.Drawing.Size(184, 61);
            this.groupBox21.TabIndex = 27;
            this.groupBox21.TabStop = false;
            this.groupBox21.Text = "Manual state";
            // 
            // eManualStateCheckBox
            // 
            this.eManualStateCheckBox.Location = new System.Drawing.Point(6, 23);
            this.eManualStateCheckBox.Name = "eManualStateCheckBox";
            this.eManualStateCheckBox.Size = new System.Drawing.Size(167, 24);
            this.eManualStateCheckBox.TabIndex = 53;
            this.eManualStateCheckBox.Text = "State (Checked is 0=>N+)";
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
            this.groupBox7.Controls.Add(this.logCurrentDataCheckBox);
            this.groupBox7.Controls.Add(this.clearIMonitorButton);
            this.groupBox7.Controls.Add(this.southIMonitorErrorTextBox);
            this.groupBox7.Controls.Add(this.label131);
            this.groupBox7.Controls.Add(this.label130);
            this.groupBox7.Controls.Add(this.northIMonitorErrorTextBox);
            this.groupBox7.Controls.Add(this.label35);
            this.groupBox7.Controls.Add(this.zeroIMonitorButton);
            this.groupBox7.Controls.Add(this.label129);
            this.groupBox7.Controls.Add(this.currentMonitorSampleLengthTextBox);
            this.groupBox7.Controls.Add(this.label128);
            this.groupBox7.Controls.Add(this.southOffsetIMonitorTextBox);
            this.groupBox7.Controls.Add(this.northOffsetIMonitorTextBox);
            this.groupBox7.Controls.Add(this.label85);
            this.groupBox7.Controls.Add(this.label84);
            this.groupBox7.Controls.Add(this.southV2FSlopeTextBox);
            this.groupBox7.Controls.Add(this.northV2FSlopeTextBox);
            this.groupBox7.Controls.Add(this.leakageMonitorSlopeTextBox);
            this.groupBox7.Controls.Add(this.label64);
            this.groupBox7.Controls.Add(this.stopIMonitorPollButton);
            this.groupBox7.Controls.Add(this.legend1);
            this.groupBox7.Controls.Add(this.label63);
            this.groupBox7.Controls.Add(this.iMonitorPollPeriod);
            this.groupBox7.Controls.Add(this.startIMonitorPollButton);
            this.groupBox7.Controls.Add(this.leakageGraph);
            this.groupBox7.Controls.Add(this.IMonitorMeasurementLengthTextBox);
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.label16);
            this.groupBox7.Controls.Add(this.southIMonitorTextBox);
            this.groupBox7.Controls.Add(this.northIMonitorTextBox);
            this.groupBox7.Controls.Add(this.updateIMonitorButton);
            this.groupBox7.Controls.Add(this.label18);
            this.groupBox7.Controls.Add(this.label19);
            this.groupBox7.Location = new System.Drawing.Point(17, 298);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(660, 274);
            this.groupBox7.TabIndex = 44;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Current monitors";
            // 
            // logCurrentDataCheckBox
            // 
            this.logCurrentDataCheckBox.AutoSize = true;
            this.logCurrentDataCheckBox.Location = new System.Drawing.Point(580, 86);
            this.logCurrentDataCheckBox.Name = "logCurrentDataCheckBox";
            this.logCurrentDataCheckBox.Size = new System.Drawing.Size(68, 17);
            this.logCurrentDataCheckBox.TabIndex = 75;
            this.logCurrentDataCheckBox.Text = "Log data";
            this.logCurrentDataCheckBox.UseVisualStyleBackColor = true;
            this.logCurrentDataCheckBox.CheckedChanged += new System.EventHandler(this.logdataCheckBox_CheckedChanged);
            // 
            // clearIMonitorButton
            // 
            this.clearIMonitorButton.Location = new System.Drawing.Point(118, 76);
            this.clearIMonitorButton.Name = "clearIMonitorButton";
            this.clearIMonitorButton.Size = new System.Drawing.Size(39, 23);
            this.clearIMonitorButton.TabIndex = 74;
            this.clearIMonitorButton.Text = "Clear";
            this.clearIMonitorButton.UseVisualStyleBackColor = true;
            this.clearIMonitorButton.Click += new System.EventHandler(this.clearIMonitorButton_Click);
            // 
            // southIMonitorErrorTextBox
            // 
            this.southIMonitorErrorTextBox.BackColor = System.Drawing.Color.Black;
            this.southIMonitorErrorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.southIMonitorErrorTextBox.Location = new System.Drawing.Point(93, 47);
            this.southIMonitorErrorTextBox.Name = "southIMonitorErrorTextBox";
            this.southIMonitorErrorTextBox.ReadOnly = true;
            this.southIMonitorErrorTextBox.Size = new System.Drawing.Size(38, 20);
            this.southIMonitorErrorTextBox.TabIndex = 73;
            this.southIMonitorErrorTextBox.Text = "0";
            // 
            // label131
            // 
            this.label131.Location = new System.Drawing.Point(80, 50);
            this.label131.Name = "label131";
            this.label131.Size = new System.Drawing.Size(12, 23);
            this.label131.TabIndex = 72;
            this.label131.Text = "±";
            // 
            // label130
            // 
            this.label130.Location = new System.Drawing.Point(80, 24);
            this.label130.Name = "label130";
            this.label130.Size = new System.Drawing.Size(12, 23);
            this.label130.TabIndex = 71;
            this.label130.Text = "±";
            // 
            // northIMonitorErrorTextBox
            // 
            this.northIMonitorErrorTextBox.BackColor = System.Drawing.Color.Black;
            this.northIMonitorErrorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.northIMonitorErrorTextBox.Location = new System.Drawing.Point(93, 21);
            this.northIMonitorErrorTextBox.Name = "northIMonitorErrorTextBox";
            this.northIMonitorErrorTextBox.ReadOnly = true;
            this.northIMonitorErrorTextBox.Size = new System.Drawing.Size(38, 20);
            this.northIMonitorErrorTextBox.TabIndex = 70;
            this.northIMonitorErrorTextBox.Text = "0";
            // 
            // label35
            // 
            this.label35.Location = new System.Drawing.Point(163, 73);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(80, 31);
            this.label35.TabIndex = 51;
            this.label35.Text = "Measurement Length (S)";
            // 
            // zeroIMonitorButton
            // 
            this.zeroIMonitorButton.Location = new System.Drawing.Point(68, 76);
            this.zeroIMonitorButton.Name = "zeroIMonitorButton";
            this.zeroIMonitorButton.Size = new System.Drawing.Size(44, 23);
            this.zeroIMonitorButton.TabIndex = 46;
            this.zeroIMonitorButton.Text = "Zero";
            this.zeroIMonitorButton.UseVisualStyleBackColor = true;
            this.zeroIMonitorButton.Click += new System.EventHandler(this.calibrateIMonitorButton_Click);
            // 
            // label129
            // 
            this.label129.Location = new System.Drawing.Point(134, 54);
            this.label129.Name = "label129";
            this.label129.Size = new System.Drawing.Size(47, 23);
            this.label129.TabIndex = 69;
            this.label129.Text = "samples";
            // 
            // currentMonitorSampleLengthTextBox
            // 
            this.currentMonitorSampleLengthTextBox.Location = new System.Drawing.Point(137, 31);
            this.currentMonitorSampleLengthTextBox.Name = "currentMonitorSampleLengthTextBox";
            this.currentMonitorSampleLengthTextBox.Size = new System.Drawing.Size(36, 20);
            this.currentMonitorSampleLengthTextBox.TabIndex = 67;
            this.currentMonitorSampleLengthTextBox.Text = "20";
            // 
            // label128
            // 
            this.label128.Location = new System.Drawing.Point(134, 16);
            this.label128.Name = "label128";
            this.label128.Size = new System.Drawing.Size(47, 23);
            this.label128.TabIndex = 68;
            this.label128.Text = "Average";
            // 
            // southOffsetIMonitorTextBox
            // 
            this.southOffsetIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.southOffsetIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.southOffsetIMonitorTextBox.Location = new System.Drawing.Point(244, 47);
            this.southOffsetIMonitorTextBox.Name = "southOffsetIMonitorTextBox";
            this.southOffsetIMonitorTextBox.ReadOnly = true;
            this.southOffsetIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.southOffsetIMonitorTextBox.TabIndex = 47;
            this.southOffsetIMonitorTextBox.Text = "0";
            // 
            // northOffsetIMonitorTextBox
            // 
            this.northOffsetIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.northOffsetIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.northOffsetIMonitorTextBox.Location = new System.Drawing.Point(244, 21);
            this.northOffsetIMonitorTextBox.Name = "northOffsetIMonitorTextBox";
            this.northOffsetIMonitorTextBox.ReadOnly = true;
            this.northOffsetIMonitorTextBox.Size = new System.Drawing.Size(64, 20);
            this.northOffsetIMonitorTextBox.TabIndex = 49;
            this.northOffsetIMonitorTextBox.Text = "0";
            // 
            // label85
            // 
            this.label85.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label85.Location = new System.Drawing.Point(317, 41);
            this.label85.Name = "label85";
            this.label85.Size = new System.Drawing.Size(82, 28);
            this.label85.TabIndex = 66;
            this.label85.Text = "South monitor (V/kHz)";
            // 
            // label84
            // 
            this.label84.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label84.Location = new System.Drawing.Point(317, 13);
            this.label84.Name = "label84";
            this.label84.Size = new System.Drawing.Size(82, 28);
            this.label84.TabIndex = 65;
            this.label84.Text = "North monitor (V/kHz)";
            // 
            // southV2FSlopeTextBox
            // 
            this.southV2FSlopeTextBox.Location = new System.Drawing.Point(405, 38);
            this.southV2FSlopeTextBox.Name = "southV2FSlopeTextBox";
            this.southV2FSlopeTextBox.Size = new System.Drawing.Size(65, 20);
            this.southV2FSlopeTextBox.TabIndex = 64;
            this.southV2FSlopeTextBox.Text = "0.0255023";
            // 
            // northV2FSlopeTextBox
            // 
            this.northV2FSlopeTextBox.Location = new System.Drawing.Point(405, 10);
            this.northV2FSlopeTextBox.Name = "northV2FSlopeTextBox";
            this.northV2FSlopeTextBox.Size = new System.Drawing.Size(65, 20);
            this.northV2FSlopeTextBox.TabIndex = 63;
            this.northV2FSlopeTextBox.Text = "0.025425";
            // 
            // leakageMonitorSlopeTextBox
            // 
            this.leakageMonitorSlopeTextBox.Location = new System.Drawing.Point(405, 84);
            this.leakageMonitorSlopeTextBox.Name = "leakageMonitorSlopeTextBox";
            this.leakageMonitorSlopeTextBox.Size = new System.Drawing.Size(65, 20);
            this.leakageMonitorSlopeTextBox.TabIndex = 2;
            this.leakageMonitorSlopeTextBox.Text = "0.200";
            // 
            // label64
            // 
            this.label64.Location = new System.Drawing.Point(317, 68);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(149, 43);
            this.label64.TabIndex = 58;
            this.label64.Text = "Frequency to Current (kHz/I)\r\n 0.2 kHz/nA hi\r\n~2 kHz/uA lo";
            // 
            // stopIMonitorPollButton
            // 
            this.stopIMonitorPollButton.Enabled = false;
            this.stopIMonitorPollButton.Location = new System.Drawing.Point(577, 50);
            this.stopIMonitorPollButton.Name = "stopIMonitorPollButton";
            this.stopIMonitorPollButton.Size = new System.Drawing.Size(75, 23);
            this.stopIMonitorPollButton.TabIndex = 55;
            this.stopIMonitorPollButton.Text = "Stop poll";
            this.stopIMonitorPollButton.UseVisualStyleBackColor = true;
            this.stopIMonitorPollButton.Click += new System.EventHandler(this.stopIMonitorPollButton_Click);
            // 
            // legend1
            // 
            this.legend1.Items.AddRange(new NationalInstruments.UI.LegendItem[] {
            this.NorthLegendItem,
            this.SouthLegendItem});
            this.legend1.ItemSize = new System.Drawing.Size(12, 12);
            this.legend1.Location = new System.Drawing.Point(472, 86);
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
            this.northLeakagePlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.northLeakagePlot.LineWidth = 2F;
            this.northLeakagePlot.XAxis = this.xAxis1;
            this.northLeakagePlot.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis1.Range = new NationalInstruments.UI.Range(0D, 500D);
            // 
            // yAxis1
            // 
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis1.OriginLineVisible = true;
            this.yAxis1.Range = new NationalInstruments.UI.Range(-20D, 20D);
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
            this.southLeakagePlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.southLeakagePlot.LineWidth = 2F;
            this.southLeakagePlot.XAxis = this.xAxis1;
            this.southLeakagePlot.YAxis = this.yAxis1;
            // 
            // label63
            // 
            this.label63.Location = new System.Drawing.Point(519, 16);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(65, 23);
            this.label63.TabIndex = 56;
            this.label63.Text = "Poll period (ms)";
            // 
            // iMonitorPollPeriod
            // 
            this.iMonitorPollPeriod.Location = new System.Drawing.Point(590, 13);
            this.iMonitorPollPeriod.Name = "iMonitorPollPeriod";
            this.iMonitorPollPeriod.Size = new System.Drawing.Size(64, 20);
            this.iMonitorPollPeriod.TabIndex = 0;
            this.iMonitorPollPeriod.Text = "100";
            // 
            // startIMonitorPollButton
            // 
            this.startIMonitorPollButton.Location = new System.Drawing.Point(491, 50);
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
            this.leakageGraph.Location = new System.Drawing.Point(9, 114);
            this.leakageGraph.Name = "leakageGraph";
            this.leakageGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.northLeakagePlot,
            this.southLeakagePlot});
            this.leakageGraph.Size = new System.Drawing.Size(645, 153);
            this.leakageGraph.TabIndex = 45;
            this.leakageGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.leakageGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // IMonitorMeasurementLengthTextBox
            // 
            this.IMonitorMeasurementLengthTextBox.Location = new System.Drawing.Point(244, 76);
            this.IMonitorMeasurementLengthTextBox.Name = "IMonitorMeasurementLengthTextBox";
            this.IMonitorMeasurementLengthTextBox.Size = new System.Drawing.Size(64, 20);
            this.IMonitorMeasurementLengthTextBox.TabIndex = 1;
            this.IMonitorMeasurementLengthTextBox.Text = "200";
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(179, 24);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(72, 23);
            this.label17.TabIndex = 50;
            this.label17.Text = "N offset (Hz)";
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(181, 50);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(74, 23);
            this.label16.TabIndex = 48;
            this.label16.Text = "S offset (Hz)";
            // 
            // southIMonitorTextBox
            // 
            this.southIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.southIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.southIMonitorTextBox.Location = new System.Drawing.Point(42, 47);
            this.southIMonitorTextBox.Name = "southIMonitorTextBox";
            this.southIMonitorTextBox.ReadOnly = true;
            this.southIMonitorTextBox.Size = new System.Drawing.Size(39, 20);
            this.southIMonitorTextBox.TabIndex = 45;
            this.southIMonitorTextBox.Text = "0";
            // 
            // northIMonitorTextBox
            // 
            this.northIMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.northIMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.northIMonitorTextBox.Location = new System.Drawing.Point(42, 21);
            this.northIMonitorTextBox.Name = "northIMonitorTextBox";
            this.northIMonitorTextBox.ReadOnly = true;
            this.northIMonitorTextBox.Size = new System.Drawing.Size(39, 20);
            this.northIMonitorTextBox.TabIndex = 42;
            this.northIMonitorTextBox.Text = "0";
            // 
            // updateIMonitorButton
            // 
            this.updateIMonitorButton.Location = new System.Drawing.Point(9, 76);
            this.updateIMonitorButton.Name = "updateIMonitorButton";
            this.updateIMonitorButton.Size = new System.Drawing.Size(53, 23);
            this.updateIMonitorButton.TabIndex = 40;
            this.updateIMonitorButton.Text = "Update";
            this.updateIMonitorButton.Click += new System.EventHandler(this.updateIMonitorButton_Click);
            // 
            // label18
            // 
            this.label18.Location = new System.Drawing.Point(6, 50);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(40, 23);
            this.label18.TabIndex = 37;
            this.label18.Text = "S (nA)";
            // 
            // label19
            // 
            this.label19.Location = new System.Drawing.Point(6, 24);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(40, 23);
            this.label19.TabIndex = 36;
            this.label19.Text = "N (nA)";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.Transparent;
            this.tabPage2.Controls.Add(this.groupBox24);
            this.tabPage2.Controls.Add(this.groupBox22);
            this.tabPage2.Controls.Add(this.groupBox4);
            this.tabPage2.Controls.Add(this.groupBox16);
            this.tabPage2.Controls.Add(this.groupBox14);
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(697, 575);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Synth";
            // 
            // groupBox24
            // 
            this.groupBox24.Controls.Add(this.piFlipMonTextBox);
            this.groupBox24.Controls.Add(this.UpdatePiFlipMonButton);
            this.groupBox24.Controls.Add(this.piMonitor2TextBox);
            this.groupBox24.Controls.Add(this.updatePiMonitorButton);
            this.groupBox24.Controls.Add(this.label82);
            this.groupBox24.Controls.Add(this.piMonitor1TextBox);
            this.groupBox24.Controls.Add(this.label132);
            this.groupBox24.Controls.Add(this.label133);
            this.groupBox24.Location = new System.Drawing.Point(8, 532);
            this.groupBox24.Name = "groupBox24";
            this.groupBox24.Size = new System.Drawing.Size(675, 39);
            this.groupBox24.TabIndex = 65;
            this.groupBox24.TabStop = false;
            this.groupBox24.Text = "pi monitor";
            // 
            // piFlipMonTextBox
            // 
            this.piFlipMonTextBox.BackColor = System.Drawing.Color.Black;
            this.piFlipMonTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.piFlipMonTextBox.Location = new System.Drawing.Point(127, 16);
            this.piFlipMonTextBox.Name = "piFlipMonTextBox";
            this.piFlipMonTextBox.ReadOnly = true;
            this.piFlipMonTextBox.Size = new System.Drawing.Size(64, 20);
            this.piFlipMonTextBox.TabIndex = 67;
            this.piFlipMonTextBox.Text = "0";
            // 
            // UpdatePiFlipMonButton
            // 
            this.UpdatePiFlipMonButton.Location = new System.Drawing.Point(212, 14);
            this.UpdatePiFlipMonButton.Name = "UpdatePiFlipMonButton";
            this.UpdatePiFlipMonButton.Size = new System.Drawing.Size(75, 23);
            this.UpdatePiFlipMonButton.TabIndex = 66;
            this.UpdatePiFlipMonButton.Text = "Update";
            this.UpdatePiFlipMonButton.Click += new System.EventHandler(this.UpdatePiFlipMonButton_Click);
            // 
            // piMonitor2TextBox
            // 
            this.piMonitor2TextBox.BackColor = System.Drawing.Color.Black;
            this.piMonitor2TextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.piMonitor2TextBox.Location = new System.Drawing.Point(441, 17);
            this.piMonitor2TextBox.Name = "piMonitor2TextBox";
            this.piMonitor2TextBox.ReadOnly = true;
            this.piMonitor2TextBox.Size = new System.Drawing.Size(64, 20);
            this.piMonitor2TextBox.TabIndex = 65;
            this.piMonitor2TextBox.Text = "0";
            // 
            // updatePiMonitorButton
            // 
            this.updatePiMonitorButton.Location = new System.Drawing.Point(528, 15);
            this.updatePiMonitorButton.Name = "updatePiMonitorButton";
            this.updatePiMonitorButton.Size = new System.Drawing.Size(75, 23);
            this.updatePiMonitorButton.TabIndex = 63;
            this.updatePiMonitorButton.Text = "Check";
            this.updatePiMonitorButton.Click += new System.EventHandler(this.updatePiMonitorButton_Click);
            // 
            // label82
            // 
            this.label82.Location = new System.Drawing.Point(16, 19);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(108, 23);
            this.label82.TabIndex = 64;
            this.label82.Text = "Monitor voltage (V)";
            // 
            // piMonitor1TextBox
            // 
            this.piMonitor1TextBox.BackColor = System.Drawing.Color.Black;
            this.piMonitor1TextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.piMonitor1TextBox.Location = new System.Drawing.Point(345, 17);
            this.piMonitor1TextBox.Name = "piMonitor1TextBox";
            this.piMonitor1TextBox.ReadOnly = true;
            this.piMonitor1TextBox.Size = new System.Drawing.Size(64, 20);
            this.piMonitor1TextBox.TabIndex = 62;
            this.piMonitor1TextBox.Text = "0";
            // 
            // label132
            // 
            this.label132.Location = new System.Drawing.Point(331, 20);
            this.label132.Name = "label132";
            this.label132.Size = new System.Drawing.Size(80, 23);
            this.label132.TabIndex = 72;
            this.label132.Text = "0";
            // 
            // label133
            // 
            this.label133.Location = new System.Drawing.Point(418, 20);
            this.label133.Name = "label133";
            this.label133.Size = new System.Drawing.Size(80, 23);
            this.label133.TabIndex = 73;
            this.label133.Text = "180";
            // 
            // groupBox22
            // 
            this.groupBox22.Controls.Add(this.rfManualStateCheckBox);
            this.groupBox22.Location = new System.Drawing.Point(320, 263);
            this.groupBox22.Name = "groupBox22";
            this.groupBox22.Size = new System.Drawing.Size(363, 61);
            this.groupBox22.TabIndex = 33;
            this.groupBox22.TabStop = false;
            this.groupBox22.Text = "Manual state";
            // 
            // rfManualStateCheckBox
            // 
            this.rfManualStateCheckBox.Location = new System.Drawing.Point(29, 19);
            this.rfManualStateCheckBox.Name = "rfManualStateCheckBox";
            this.rfManualStateCheckBox.Size = new System.Drawing.Size(257, 24);
            this.rfManualStateCheckBox.TabIndex = 53;
            this.rfManualStateCheckBox.Text = "State (Checked is rf in bottom)";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.setDCFMtoGuess);
            this.groupBox4.Controls.Add(this.Copyrf2f);
            this.groupBox4.Controls.Add(this.Copyrf1f);
            this.groupBox4.Controls.Add(this.rf2fCentreGuessTextBox);
            this.groupBox4.Controls.Add(this.rf1fCentreGuessTextBox);
            this.groupBox4.Controls.Add(this.setAttunatorsToGuesses);
            this.groupBox4.Controls.Add(this.Copyrf2a);
            this.groupBox4.Controls.Add(this.Copyrf1a);
            this.groupBox4.Controls.Add(this.rf2aCentreGuessTextBox);
            this.groupBox4.Controls.Add(this.rf1aCentreGuessTextBox);
            this.groupBox4.Controls.Add(this.rf2StepPowerMon);
            this.groupBox4.Controls.Add(this.rf2StepFreqMon);
            this.groupBox4.Controls.Add(this.rf1StepPowerMon);
            this.groupBox4.Controls.Add(this.rfPowerUpdateButton);
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
            this.groupBox4.Controls.Add(this.label52);
            this.groupBox4.Controls.Add(this.rfFrequencyUpdateButton);
            this.groupBox4.Controls.Add(this.label51);
            this.groupBox4.Controls.Add(this.label46);
            this.groupBox4.Controls.Add(this.label50);
            this.groupBox4.Controls.Add(this.label43);
            this.groupBox4.Controls.Add(this.label49);
            this.groupBox4.Controls.Add(this.label45);
            this.groupBox4.Controls.Add(this.label44);
            this.groupBox4.Controls.Add(this.label81);
            this.groupBox4.Controls.Add(this.label125);
            this.groupBox4.Controls.Add(this.label126);
            this.groupBox4.Controls.Add(this.label127);
            this.groupBox4.Location = new System.Drawing.Point(8, 328);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(675, 209);
            this.groupBox4.TabIndex = 27;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "rf measurement";
            // 
            // setDCFMtoGuess
            // 
            this.setDCFMtoGuess.Location = new System.Drawing.Point(62, 175);
            this.setDCFMtoGuess.Name = "setDCFMtoGuess";
            this.setDCFMtoGuess.Size = new System.Drawing.Size(225, 23);
            this.setDCFMtoGuess.TabIndex = 67;
            this.setDCFMtoGuess.Text = "Set DCFM Voltages to Guess";
            this.setDCFMtoGuess.Click += new System.EventHandler(this.setDCFMtoGuess_Click);
            // 
            // Copyrf2f
            // 
            this.Copyrf2f.Location = new System.Drawing.Point(256, 148);
            this.Copyrf2f.Name = "Copyrf2f";
            this.Copyrf2f.Size = new System.Drawing.Size(75, 23);
            this.Copyrf2f.TabIndex = 66;
            this.Copyrf2f.Text = "Copy";
            this.Copyrf2f.Click += new System.EventHandler(this.Copyrf2f_Click);
            // 
            // Copyrf1f
            // 
            this.Copyrf1f.Location = new System.Drawing.Point(95, 147);
            this.Copyrf1f.Name = "Copyrf1f";
            this.Copyrf1f.Size = new System.Drawing.Size(75, 23);
            this.Copyrf1f.TabIndex = 65;
            this.Copyrf1f.Text = "Copy";
            this.Copyrf1f.Click += new System.EventHandler(this.Copyrf1f_Click);
            // 
            // rf2fCentreGuessTextBox
            // 
            this.rf2fCentreGuessTextBox.Location = new System.Drawing.Point(185, 148);
            this.rf2fCentreGuessTextBox.Name = "rf2fCentreGuessTextBox";
            this.rf2fCentreGuessTextBox.Size = new System.Drawing.Size(64, 20);
            this.rf2fCentreGuessTextBox.TabIndex = 64;
            this.rf2fCentreGuessTextBox.Text = "0";
            // 
            // rf1fCentreGuessTextBox
            // 
            this.rf1fCentreGuessTextBox.Location = new System.Drawing.Point(24, 147);
            this.rf1fCentreGuessTextBox.Name = "rf1fCentreGuessTextBox";
            this.rf1fCentreGuessTextBox.Size = new System.Drawing.Size(64, 20);
            this.rf1fCentreGuessTextBox.TabIndex = 63;
            this.rf1fCentreGuessTextBox.Text = "0";
            // 
            // setAttunatorsToGuesses
            // 
            this.setAttunatorsToGuesses.Location = new System.Drawing.Point(398, 177);
            this.setAttunatorsToGuesses.Name = "setAttunatorsToGuesses";
            this.setAttunatorsToGuesses.Size = new System.Drawing.Size(225, 23);
            this.setAttunatorsToGuesses.TabIndex = 62;
            this.setAttunatorsToGuesses.Text = "Set Attenuator Voltages to Guess";
            this.setAttunatorsToGuesses.Click += new System.EventHandler(this.setAttunatorsToGuesses_Click);
            // 
            // Copyrf2a
            // 
            this.Copyrf2a.Location = new System.Drawing.Point(592, 148);
            this.Copyrf2a.Name = "Copyrf2a";
            this.Copyrf2a.Size = new System.Drawing.Size(75, 23);
            this.Copyrf2a.TabIndex = 61;
            this.Copyrf2a.Text = "Copy";
            this.Copyrf2a.Click += new System.EventHandler(this.Copyrf2a_Click);
            // 
            // Copyrf1a
            // 
            this.Copyrf1a.Location = new System.Drawing.Point(431, 147);
            this.Copyrf1a.Name = "Copyrf1a";
            this.Copyrf1a.Size = new System.Drawing.Size(75, 23);
            this.Copyrf1a.TabIndex = 60;
            this.Copyrf1a.Text = "Copy";
            this.Copyrf1a.Click += new System.EventHandler(this.Copyrf1a_Click);
            // 
            // rf2aCentreGuessTextBox
            // 
            this.rf2aCentreGuessTextBox.Location = new System.Drawing.Point(521, 148);
            this.rf2aCentreGuessTextBox.Name = "rf2aCentreGuessTextBox";
            this.rf2aCentreGuessTextBox.Size = new System.Drawing.Size(64, 20);
            this.rf2aCentreGuessTextBox.TabIndex = 59;
            this.rf2aCentreGuessTextBox.Text = "0";
            // 
            // rf1aCentreGuessTextBox
            // 
            this.rf1aCentreGuessTextBox.Location = new System.Drawing.Point(360, 147);
            this.rf1aCentreGuessTextBox.Name = "rf1aCentreGuessTextBox";
            this.rf1aCentreGuessTextBox.Size = new System.Drawing.Size(64, 20);
            this.rf1aCentreGuessTextBox.TabIndex = 34;
            this.rf1aCentreGuessTextBox.Text = "0";
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
            // rfPowerUpdateButton
            // 
            this.rfPowerUpdateButton.Location = new System.Drawing.Point(469, 107);
            this.rfPowerUpdateButton.Name = "rfPowerUpdateButton";
            this.rfPowerUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.rfPowerUpdateButton.TabIndex = 50;
            this.rfPowerUpdateButton.Text = "Update";
            this.rfPowerUpdateButton.Click += new System.EventHandler(this.rfPowerUpdateButton_Click);
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
            this.rfFrequencyUpdateButton.Location = new System.Drawing.Point(140, 107);
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
            // label81
            // 
            this.label81.Location = new System.Drawing.Point(358, 133);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(80, 23);
            this.label81.TabIndex = 68;
            this.label81.Text = "Centre";
            // 
            // label125
            // 
            this.label125.Location = new System.Drawing.Point(519, 134);
            this.label125.Name = "label125";
            this.label125.Size = new System.Drawing.Size(80, 23);
            this.label125.TabIndex = 69;
            this.label125.Text = "Centre";
            // 
            // label126
            // 
            this.label126.Location = new System.Drawing.Point(23, 133);
            this.label126.Name = "label126";
            this.label126.Size = new System.Drawing.Size(80, 23);
            this.label126.TabIndex = 70;
            this.label126.Text = "Centre";
            // 
            // label127
            // 
            this.label127.Location = new System.Drawing.Point(184, 134);
            this.label127.Name = "label127";
            this.label127.Size = new System.Drawing.Size(80, 23);
            this.label127.TabIndex = 71;
            this.label127.Text = "Centre";
            // 
            // groupBox16
            // 
            this.groupBox16.Controls.Add(this.synthSensorConnectCheckBox);
            this.groupBox16.Controls.Add(this.scramblerCheckBox);
            this.groupBox16.Controls.Add(this.attenuatorSelectCheck);
            this.groupBox16.Controls.Add(this.phaseFlip2CheckBox);
            this.groupBox16.Controls.Add(this.phaseFlip1CheckBox);
            this.groupBox16.Controls.Add(this.fmSelectCheck);
            this.groupBox16.Controls.Add(this.rfSwitchEnableCheck);
            this.groupBox16.Location = new System.Drawing.Point(8, 184);
            this.groupBox16.Name = "groupBox16";
            this.groupBox16.Size = new System.Drawing.Size(296, 140);
            this.groupBox16.TabIndex = 26;
            this.groupBox16.TabStop = false;
            this.groupBox16.Text = "TTL controls";
            // 
            // synthSensorConnectCheckBox
            // 
            this.synthSensorConnectCheckBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.synthSensorConnectCheckBox.Checked = true;
            this.synthSensorConnectCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.synthSensorConnectCheckBox.Location = new System.Drawing.Point(190, 50);
            this.synthSensorConnectCheckBox.Name = "synthSensorConnectCheckBox";
            this.synthSensorConnectCheckBox.Size = new System.Drawing.Size(97, 54);
            this.synthSensorConnectCheckBox.TabIndex = 32;
            this.synthSensorConnectCheckBox.Text = "Connect sensors to synth rf output";
            this.synthSensorConnectCheckBox.CheckedChanged += new System.EventHandler(this.synthSensorConnectCheckBox_CheckedChanged);
            // 
            // scramblerCheckBox
            // 
            this.scramblerCheckBox.Location = new System.Drawing.Point(190, 22);
            this.scramblerCheckBox.Name = "scramblerCheckBox";
            this.scramblerCheckBox.Size = new System.Drawing.Size(122, 24);
            this.scramblerCheckBox.TabIndex = 31;
            this.scramblerCheckBox.Text = "scrambler TTL";
            this.scramblerCheckBox.CheckedChanged += new System.EventHandler(this.scramblerCheckBox_CheckedChanged);
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
            this.phaseFlip2CheckBox.Location = new System.Drawing.Point(187, 112);
            this.phaseFlip2CheckBox.Name = "phaseFlip2CheckBox";
            this.phaseFlip2CheckBox.Size = new System.Drawing.Size(152, 24);
            this.phaseFlip2CheckBox.TabIndex = 29;
            this.phaseFlip2CheckBox.Text = "pi flip enable";
            this.phaseFlip2CheckBox.CheckedChanged += new System.EventHandler(this.phaseFlip2CheckBox_CheckedChanged);
            // 
            // phaseFlip1CheckBox
            // 
            this.phaseFlip1CheckBox.Location = new System.Drawing.Point(24, 112);
            this.phaseFlip1CheckBox.Name = "phaseFlip1CheckBox";
            this.phaseFlip1CheckBox.Size = new System.Drawing.Size(152, 24);
            this.phaseFlip1CheckBox.TabIndex = 28;
            this.phaseFlip1CheckBox.Text = "Pi flip";
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
            this.groupBox14.Controls.Add(this.label74);
            this.groupBox14.Controls.Add(this.scramblerVoltageTextBox);
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
            // setScramblerVoltageButton
            // 
            this.setScramblerVoltageButton.Location = new System.Drawing.Point(125, 218);
            this.setScramblerVoltageButton.Name = "setScramblerVoltageButton";
            this.setScramblerVoltageButton.Size = new System.Drawing.Size(131, 23);
            this.setScramblerVoltageButton.TabIndex = 33;
            this.setScramblerVoltageButton.Text = "Set scrambler voltage";
            this.setScramblerVoltageButton.Click += new System.EventHandler(this.setScramblerVoltageButton_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.rf2FMZeroRB);
            this.panel4.Controls.Add(this.rf2FMPlusRB);
            this.panel4.Controls.Add(this.rf2FMMinusRB);
            this.panel4.Location = new System.Drawing.Point(249, 134);
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
            // label74
            // 
            this.label74.Location = new System.Drawing.Point(6, 195);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(114, 23);
            this.label74.TabIndex = 32;
            this.label74.Text = "Scrambler voltage (V)";
            // 
            // scramblerVoltageTextBox
            // 
            this.scramblerVoltageTextBox.Location = new System.Drawing.Point(128, 192);
            this.scramblerVoltageTextBox.Name = "scramblerVoltageTextBox";
            this.scramblerVoltageTextBox.Size = new System.Drawing.Size(34, 20);
            this.scramblerVoltageTextBox.TabIndex = 31;
            this.scramblerVoltageTextBox.Text = "0";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.rf1FMZeroRB);
            this.panel3.Controls.Add(this.rf1FMPlusRB);
            this.panel3.Controls.Add(this.rf1FMMinusRB);
            this.panel3.Location = new System.Drawing.Point(249, 106);
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
            this.rf2FMIncTextBox.Location = new System.Drawing.Point(198, 136);
            this.rf2FMIncTextBox.Name = "rf2FMIncTextBox";
            this.rf2FMIncTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf2FMIncTextBox.TabIndex = 7;
            this.rf2FMIncTextBox.Text = "0";
            // 
            // label24
            // 
            this.label24.Location = new System.Drawing.Point(168, 136);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(24, 23);
            this.label24.TabIndex = 30;
            this.label24.Text = "+-";
            // 
            // rf1FMIncTextBox
            // 
            this.rf1FMIncTextBox.Location = new System.Drawing.Point(198, 110);
            this.rf1FMIncTextBox.Name = "rf1FMIncTextBox";
            this.rf1FMIncTextBox.Size = new System.Drawing.Size(34, 20);
            this.rf1FMIncTextBox.TabIndex = 5;
            this.rf1FMIncTextBox.Text = "0";
            // 
            // label28
            // 
            this.label28.Location = new System.Drawing.Point(168, 110);
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
            this.setFMVoltagesButton.Location = new System.Drawing.Point(125, 166);
            this.setFMVoltagesButton.Name = "setFMVoltagesButton";
            this.setFMVoltagesButton.Size = new System.Drawing.Size(131, 23);
            this.setFMVoltagesButton.TabIndex = 23;
            this.setFMVoltagesButton.Text = "Set fm voltages";
            this.setFMVoltagesButton.Click += new System.EventHandler(this.setFMVoltagesButton_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(59, 140);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 23);
            this.label2.TabIndex = 22;
            this.label2.Text = "rf2 fm (V)";
            // 
            // rf2FMVoltage
            // 
            this.rf2FMVoltage.Location = new System.Drawing.Point(128, 136);
            this.rf2FMVoltage.Name = "rf2FMVoltage";
            this.rf2FMVoltage.Size = new System.Drawing.Size(34, 20);
            this.rf2FMVoltage.TabIndex = 6;
            this.rf2FMVoltage.Text = "0";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(59, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 23);
            this.label3.TabIndex = 20;
            this.label3.Text = "rf1 fm (V)";
            // 
            // rf1FMVoltage
            // 
            this.rf1FMVoltage.Location = new System.Drawing.Point(128, 110);
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
            this.tabPage3.BackColor = System.Drawing.Color.Transparent;
            this.tabPage3.Controls.Add(this.groupBox23);
            this.tabPage3.Controls.Add(this.groupBox20);
            this.tabPage3.Controls.Add(this.groupBox9);
            this.tabPage3.Controls.Add(this.groupBox12);
            this.tabPage3.Controls.Add(this.groupBox8);
            this.tabPage3.Controls.Add(this.groupBox5);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(697, 575);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "B-field";
            // 
            // groupBox23
            // 
            this.groupBox23.Controls.Add(this.bManualStateCheckBox);
            this.groupBox23.Location = new System.Drawing.Point(390, 16);
            this.groupBox23.Name = "groupBox23";
            this.groupBox23.Size = new System.Drawing.Size(233, 61);
            this.groupBox23.TabIndex = 49;
            this.groupBox23.TabStop = false;
            this.groupBox23.Text = "Manual state";
            // 
            // bManualStateCheckBox
            // 
            this.bManualStateCheckBox.Location = new System.Drawing.Point(6, 23);
            this.bManualStateCheckBox.Name = "bManualStateCheckBox";
            this.bManualStateCheckBox.Size = new System.Drawing.Size(221, 24);
            this.bManualStateCheckBox.TabIndex = 53;
            this.bManualStateCheckBox.Text = "State (Checked is Red=>+Iz)";
            // 
            // groupBox20
            // 
            this.groupBox20.Controls.Add(this.miniFlux2TextBox);
            this.groupBox20.Controls.Add(this.miniFlux3TextBox);
            this.groupBox20.Controls.Add(this.label77);
            this.groupBox20.Controls.Add(this.label76);
            this.groupBox20.Controls.Add(this.miniFlux1TextBox);
            this.groupBox20.Controls.Add(this.updateMiniFluxgatesButton);
            this.groupBox20.Controls.Add(this.label75);
            this.groupBox20.Location = new System.Drawing.Point(390, 78);
            this.groupBox20.Name = "groupBox20";
            this.groupBox20.Size = new System.Drawing.Size(233, 192);
            this.groupBox20.TabIndex = 48;
            this.groupBox20.TabStop = false;
            this.groupBox20.Text = "Mini-Fluxgate Monitor";
            // 
            // miniFlux2TextBox
            // 
            this.miniFlux2TextBox.BackColor = System.Drawing.Color.Black;
            this.miniFlux2TextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.miniFlux2TextBox.Location = new System.Drawing.Point(140, 50);
            this.miniFlux2TextBox.Name = "miniFlux2TextBox";
            this.miniFlux2TextBox.ReadOnly = true;
            this.miniFlux2TextBox.Size = new System.Drawing.Size(64, 20);
            this.miniFlux2TextBox.TabIndex = 49;
            this.miniFlux2TextBox.Text = "0";
            // 
            // miniFlux3TextBox
            // 
            this.miniFlux3TextBox.BackColor = System.Drawing.Color.Black;
            this.miniFlux3TextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.miniFlux3TextBox.Location = new System.Drawing.Point(140, 79);
            this.miniFlux3TextBox.Name = "miniFlux3TextBox";
            this.miniFlux3TextBox.ReadOnly = true;
            this.miniFlux3TextBox.Size = new System.Drawing.Size(64, 20);
            this.miniFlux3TextBox.TabIndex = 48;
            this.miniFlux3TextBox.Text = "0";
            // 
            // label77
            // 
            this.label77.Location = new System.Drawing.Point(16, 82);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(101, 23);
            this.label77.TabIndex = 47;
            this.label77.Text = "Computer Rack (V)";
            // 
            // label76
            // 
            this.label76.Location = new System.Drawing.Point(16, 53);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(80, 23);
            this.label76.TabIndex = 46;
            this.label76.Text = "Optic Table (V)";
            // 
            // miniFlux1TextBox
            // 
            this.miniFlux1TextBox.BackColor = System.Drawing.Color.Black;
            this.miniFlux1TextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.miniFlux1TextBox.Location = new System.Drawing.Point(140, 21);
            this.miniFlux1TextBox.Name = "miniFlux1TextBox";
            this.miniFlux1TextBox.ReadOnly = true;
            this.miniFlux1TextBox.Size = new System.Drawing.Size(64, 20);
            this.miniFlux1TextBox.TabIndex = 45;
            this.miniFlux1TextBox.Text = "0";
            // 
            // updateMiniFluxgatesButton
            // 
            this.updateMiniFluxgatesButton.Location = new System.Drawing.Point(16, 120);
            this.updateMiniFluxgatesButton.Name = "updateMiniFluxgatesButton";
            this.updateMiniFluxgatesButton.Size = new System.Drawing.Size(75, 23);
            this.updateMiniFluxgatesButton.TabIndex = 40;
            this.updateMiniFluxgatesButton.Text = "Update";
            this.updateMiniFluxgatesButton.Click += new System.EventHandler(this.updateMiniFluxgatesButton_Click);
            // 
            // label75
            // 
            this.label75.Location = new System.Drawing.Point(16, 24);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(80, 23);
            this.label75.TabIndex = 36;
            this.label75.Text = "Supplies (V)";
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.automaticBiasCalcButton);
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
            // automaticBiasCalcButton
            // 
            this.automaticBiasCalcButton.Location = new System.Drawing.Point(96, 56);
            this.automaticBiasCalcButton.Name = "automaticBiasCalcButton";
            this.automaticBiasCalcButton.Size = new System.Drawing.Size(163, 23);
            this.automaticBiasCalcButton.TabIndex = 46;
            this.automaticBiasCalcButton.Text = "Set to measured Bias";
            this.automaticBiasCalcButton.Click += new System.EventHandler(this.automaticBiasCalcButton_Click);
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
            this.scanningBFSButton.Location = new System.Drawing.Point(184, 56);
            this.scanningBFSButton.Name = "scanningBFSButton";
            this.scanningBFSButton.Size = new System.Drawing.Size(75, 23);
            this.scanningBFSButton.TabIndex = 44;
            this.scanningBFSButton.Text = "Max";
            this.scanningBFSButton.Click += new System.EventHandler(this.scanningBFSButton_Click);
            // 
            // scanningBZeroButton
            // 
            this.scanningBZeroButton.Location = new System.Drawing.Point(85, 56);
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
            // tabPage11
            // 
            this.tabPage11.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage11.Controls.Add(this.groupBox25);
            this.tabPage11.Controls.Add(this.groupBox11);
            this.tabPage11.Controls.Add(this.groupBox10);
            this.tabPage11.Controls.Add(this.groupBox18);
            this.tabPage11.Location = new System.Drawing.Point(4, 22);
            this.tabPage11.Name = "tabPage11";
            this.tabPage11.Size = new System.Drawing.Size(697, 575);
            this.tabPage11.TabIndex = 10;
            this.tabPage11.Text = "Q(0)+P12";
            // 
            // groupBox25
            // 
            this.groupBox25.Controls.Add(this.pumpAOMTrackBar);
            this.groupBox25.Controls.Add(this.panel7);
            this.groupBox25.Controls.Add(this.pumpAOMStepTextBox);
            this.groupBox25.Controls.Add(this.label99);
            this.groupBox25.Controls.Add(this.pumpAOMVoltageTextBox);
            this.groupBox25.Controls.Add(this.updatePumpAOMButton);
            this.groupBox25.Controls.Add(this.label100);
            this.groupBox25.Controls.Add(this.pumpAOMFreqStepTextBox);
            this.groupBox25.Controls.Add(this.label88);
            this.groupBox25.Controls.Add(this.pumpAOMFreqPlusTextBox);
            this.groupBox25.Controls.Add(this.pumpAOMFreqCentreTextBox);
            this.groupBox25.Controls.Add(this.label95);
            this.groupBox25.Controls.Add(this.pumpAOMFreqMinusTextBox);
            this.groupBox25.Controls.Add(this.label96);
            this.groupBox25.Controls.Add(this.pumpAOMFreqUpdateButton);
            this.groupBox25.Controls.Add(this.label98);
            this.groupBox25.Location = new System.Drawing.Point(6, 369);
            this.groupBox25.Name = "groupBox25";
            this.groupBox25.Size = new System.Drawing.Size(684, 203);
            this.groupBox25.TabIndex = 71;
            this.groupBox25.TabStop = false;
            this.groupBox25.Text = "Pump AOM";
            // 
            // pumpAOMTrackBar
            // 
            this.pumpAOMTrackBar.Location = new System.Drawing.Point(9, 128);
            this.pumpAOMTrackBar.Maximum = 1000;
            this.pumpAOMTrackBar.Name = "pumpAOMTrackBar";
            this.pumpAOMTrackBar.Size = new System.Drawing.Size(284, 45);
            this.pumpAOMTrackBar.TabIndex = 73;
            this.pumpAOMTrackBar.Scroll += new System.EventHandler(this.pumpAOMTrackBar_Scroll);
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.pumpAOMStepZeroButton);
            this.panel7.Controls.Add(this.pumpAOMStepPlusButton);
            this.panel7.Controls.Add(this.pumpAOMStepMinusButton);
            this.panel7.Location = new System.Drawing.Point(177, 19);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(116, 32);
            this.panel7.TabIndex = 72;
            // 
            // pumpAOMStepZeroButton
            // 
            this.pumpAOMStepZeroButton.AutoSize = true;
            this.pumpAOMStepZeroButton.Checked = true;
            this.pumpAOMStepZeroButton.Location = new System.Drawing.Point(77, 7);
            this.pumpAOMStepZeroButton.Name = "pumpAOMStepZeroButton";
            this.pumpAOMStepZeroButton.Size = new System.Drawing.Size(31, 17);
            this.pumpAOMStepZeroButton.TabIndex = 32;
            this.pumpAOMStepZeroButton.TabStop = true;
            this.pumpAOMStepZeroButton.Text = "0";
            this.pumpAOMStepZeroButton.UseVisualStyleBackColor = true;
            // 
            // pumpAOMStepPlusButton
            // 
            this.pumpAOMStepPlusButton.AutoSize = true;
            this.pumpAOMStepPlusButton.Location = new System.Drawing.Point(3, 6);
            this.pumpAOMStepPlusButton.Name = "pumpAOMStepPlusButton";
            this.pumpAOMStepPlusButton.Size = new System.Drawing.Size(31, 17);
            this.pumpAOMStepPlusButton.TabIndex = 32;
            this.pumpAOMStepPlusButton.Text = "+";
            this.pumpAOMStepPlusButton.UseVisualStyleBackColor = true;
            // 
            // pumpAOMStepMinusButton
            // 
            this.pumpAOMStepMinusButton.AutoSize = true;
            this.pumpAOMStepMinusButton.Location = new System.Drawing.Point(42, 7);
            this.pumpAOMStepMinusButton.Name = "pumpAOMStepMinusButton";
            this.pumpAOMStepMinusButton.Size = new System.Drawing.Size(28, 17);
            this.pumpAOMStepMinusButton.TabIndex = 32;
            this.pumpAOMStepMinusButton.Text = "-";
            this.pumpAOMStepMinusButton.UseVisualStyleBackColor = true;
            // 
            // pumpAOMStepTextBox
            // 
            this.pumpAOMStepTextBox.Location = new System.Drawing.Point(96, 45);
            this.pumpAOMStepTextBox.Name = "pumpAOMStepTextBox";
            this.pumpAOMStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.pumpAOMStepTextBox.TabIndex = 71;
            this.pumpAOMStepTextBox.Text = "0";
            // 
            // label99
            // 
            this.label99.Location = new System.Drawing.Point(13, 48);
            this.label99.Name = "label99";
            this.label99.Size = new System.Drawing.Size(80, 23);
            this.label99.TabIndex = 70;
            this.label99.Text = "Step (V)";
            // 
            // pumpAOMVoltageTextBox
            // 
            this.pumpAOMVoltageTextBox.Location = new System.Drawing.Point(96, 22);
            this.pumpAOMVoltageTextBox.Name = "pumpAOMVoltageTextBox";
            this.pumpAOMVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.pumpAOMVoltageTextBox.TabIndex = 69;
            this.pumpAOMVoltageTextBox.Text = "0";
            // 
            // updatePumpAOMButton
            // 
            this.updatePumpAOMButton.Location = new System.Drawing.Point(131, 79);
            this.updatePumpAOMButton.Name = "updatePumpAOMButton";
            this.updatePumpAOMButton.Size = new System.Drawing.Size(75, 23);
            this.updatePumpAOMButton.TabIndex = 68;
            this.updatePumpAOMButton.Text = "Update";
            // 
            // label100
            // 
            this.label100.Location = new System.Drawing.Point(13, 25);
            this.label100.Name = "label100";
            this.label100.Size = new System.Drawing.Size(80, 23);
            this.label100.TabIndex = 67;
            this.label100.Text = "Voltage (V)";
            // 
            // pumpAOMFreqStepTextBox
            // 
            this.pumpAOMFreqStepTextBox.BackColor = System.Drawing.Color.Black;
            this.pumpAOMFreqStepTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.pumpAOMFreqStepTextBox.Location = new System.Drawing.Point(549, 105);
            this.pumpAOMFreqStepTextBox.Name = "pumpAOMFreqStepTextBox";
            this.pumpAOMFreqStepTextBox.ReadOnly = true;
            this.pumpAOMFreqStepTextBox.Size = new System.Drawing.Size(126, 20);
            this.pumpAOMFreqStepTextBox.TabIndex = 65;
            this.pumpAOMFreqStepTextBox.Text = "0";
            // 
            // label88
            // 
            this.label88.Location = new System.Drawing.Point(420, 108);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(111, 23);
            this.label88.TabIndex = 63;
            this.label88.Text = "Step (Hz)";
            // 
            // pumpAOMFreqPlusTextBox
            // 
            this.pumpAOMFreqPlusTextBox.BackColor = System.Drawing.Color.Black;
            this.pumpAOMFreqPlusTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.pumpAOMFreqPlusTextBox.Location = new System.Drawing.Point(549, 51);
            this.pumpAOMFreqPlusTextBox.Name = "pumpAOMFreqPlusTextBox";
            this.pumpAOMFreqPlusTextBox.ReadOnly = true;
            this.pumpAOMFreqPlusTextBox.Size = new System.Drawing.Size(126, 20);
            this.pumpAOMFreqPlusTextBox.TabIndex = 66;
            this.pumpAOMFreqPlusTextBox.Text = "0";
            // 
            // pumpAOMFreqCentreTextBox
            // 
            this.pumpAOMFreqCentreTextBox.BackColor = System.Drawing.Color.Black;
            this.pumpAOMFreqCentreTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.pumpAOMFreqCentreTextBox.Location = new System.Drawing.Point(549, 79);
            this.pumpAOMFreqCentreTextBox.Name = "pumpAOMFreqCentreTextBox";
            this.pumpAOMFreqCentreTextBox.ReadOnly = true;
            this.pumpAOMFreqCentreTextBox.Size = new System.Drawing.Size(126, 20);
            this.pumpAOMFreqCentreTextBox.TabIndex = 62;
            this.pumpAOMFreqCentreTextBox.Text = "0";
            // 
            // label95
            // 
            this.label95.Location = new System.Drawing.Point(420, 54);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(111, 23);
            this.label95.TabIndex = 64;
            this.label95.Text = "AOM freq high (Hz)";
            // 
            // pumpAOMFreqMinusTextBox
            // 
            this.pumpAOMFreqMinusTextBox.BackColor = System.Drawing.Color.Black;
            this.pumpAOMFreqMinusTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.pumpAOMFreqMinusTextBox.Location = new System.Drawing.Point(549, 25);
            this.pumpAOMFreqMinusTextBox.Name = "pumpAOMFreqMinusTextBox";
            this.pumpAOMFreqMinusTextBox.ReadOnly = true;
            this.pumpAOMFreqMinusTextBox.Size = new System.Drawing.Size(126, 20);
            this.pumpAOMFreqMinusTextBox.TabIndex = 61;
            this.pumpAOMFreqMinusTextBox.Text = "0";
            // 
            // label96
            // 
            this.label96.Location = new System.Drawing.Point(420, 82);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(111, 23);
            this.label96.TabIndex = 58;
            this.label96.Text = "Centre (Hz)";
            // 
            // pumpAOMFreqUpdateButton
            // 
            this.pumpAOMFreqUpdateButton.Location = new System.Drawing.Point(600, 131);
            this.pumpAOMFreqUpdateButton.Name = "pumpAOMFreqUpdateButton";
            this.pumpAOMFreqUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.pumpAOMFreqUpdateButton.TabIndex = 60;
            this.pumpAOMFreqUpdateButton.Text = "Update";
            this.pumpAOMFreqUpdateButton.Click += new System.EventHandler(this.pumpAOMFreqUpdateButton_Click_1);
            // 
            // label98
            // 
            this.label98.Location = new System.Drawing.Point(420, 28);
            this.label98.Name = "label98";
            this.label98.Size = new System.Drawing.Size(111, 23);
            this.label98.TabIndex = 59;
            this.label98.Text = "AOM freq low (Hz)";
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
            this.groupBox11.Location = new System.Drawing.Point(242, 16);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(448, 149);
            this.groupBox11.TabIndex = 70;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Photodiodes";
            // 
            // updateLaserPhotodiodesButton
            // 
            this.updateLaserPhotodiodesButton.Location = new System.Drawing.Point(220, 27);
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
            this.groupBox10.Controls.Add(this.argonShutterCheckBox);
            this.groupBox10.Controls.Add(this.label32);
            this.groupBox10.Location = new System.Drawing.Point(12, 16);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(224, 149);
            this.groupBox10.TabIndex = 69;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Shutters";
            // 
            // argonShutterCheckBox
            // 
            this.argonShutterCheckBox.Location = new System.Drawing.Point(10, 35);
            this.argonShutterCheckBox.Name = "argonShutterCheckBox";
            this.argonShutterCheckBox.Size = new System.Drawing.Size(72, 24);
            this.argonShutterCheckBox.TabIndex = 25;
            this.argonShutterCheckBox.Text = "Ar+";
            this.argonShutterCheckBox.CheckedChanged += new System.EventHandler(this.argonShutterCheckBox_CheckedChanged);
            // 
            // label32
            // 
            this.label32.Location = new System.Drawing.Point(10, 16);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(105, 43);
            this.label32.TabIndex = 24;
            this.label32.Text = "Checked is blocked.";
            // 
            // groupBox18
            // 
            this.groupBox18.Controls.Add(this.probeAOMFreqStepTextBox);
            this.groupBox18.Controls.Add(this.label73);
            this.groupBox18.Controls.Add(this.probeAOMFreqMinusTextBox);
            this.groupBox18.Controls.Add(this.probeAOMFreqCentreTextBox);
            this.groupBox18.Controls.Add(this.label71);
            this.groupBox18.Controls.Add(this.probeAOMFreqPlusTextBox);
            this.groupBox18.Controls.Add(this.label72);
            this.groupBox18.Controls.Add(this.probeAOMFreqUpdateButton);
            this.groupBox18.Controls.Add(this.label69);
            this.groupBox18.Controls.Add(this.probeAOMtrackBar);
            this.groupBox18.Controls.Add(this.panel5);
            this.groupBox18.Controls.Add(this.probeAOMStepTextBox);
            this.groupBox18.Controls.Add(this.label70);
            this.groupBox18.Controls.Add(this.probeAOMVTextBox);
            this.groupBox18.Controls.Add(this.UpdateProbeAOMButton);
            this.groupBox18.Controls.Add(this.label68);
            this.groupBox18.Location = new System.Drawing.Point(6, 171);
            this.groupBox18.Name = "groupBox18";
            this.groupBox18.Size = new System.Drawing.Size(684, 192);
            this.groupBox18.TabIndex = 68;
            this.groupBox18.TabStop = false;
            this.groupBox18.Text = "Probe AOM";
            // 
            // probeAOMFreqStepTextBox
            // 
            this.probeAOMFreqStepTextBox.BackColor = System.Drawing.Color.Black;
            this.probeAOMFreqStepTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.probeAOMFreqStepTextBox.Location = new System.Drawing.Point(549, 97);
            this.probeAOMFreqStepTextBox.Name = "probeAOMFreqStepTextBox";
            this.probeAOMFreqStepTextBox.ReadOnly = true;
            this.probeAOMFreqStepTextBox.Size = new System.Drawing.Size(126, 20);
            this.probeAOMFreqStepTextBox.TabIndex = 64;
            this.probeAOMFreqStepTextBox.Text = "0";
            // 
            // label73
            // 
            this.label73.Location = new System.Drawing.Point(420, 100);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(111, 23);
            this.label73.TabIndex = 62;
            this.label73.Text = "Step (Hz)";
            // 
            // probeAOMFreqMinusTextBox
            // 
            this.probeAOMFreqMinusTextBox.BackColor = System.Drawing.Color.Black;
            this.probeAOMFreqMinusTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.probeAOMFreqMinusTextBox.Location = new System.Drawing.Point(549, 43);
            this.probeAOMFreqMinusTextBox.Name = "probeAOMFreqMinusTextBox";
            this.probeAOMFreqMinusTextBox.ReadOnly = true;
            this.probeAOMFreqMinusTextBox.Size = new System.Drawing.Size(126, 20);
            this.probeAOMFreqMinusTextBox.TabIndex = 65;
            this.probeAOMFreqMinusTextBox.Text = "0";
            // 
            // probeAOMFreqCentreTextBox
            // 
            this.probeAOMFreqCentreTextBox.BackColor = System.Drawing.Color.Black;
            this.probeAOMFreqCentreTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.probeAOMFreqCentreTextBox.Location = new System.Drawing.Point(549, 71);
            this.probeAOMFreqCentreTextBox.Name = "probeAOMFreqCentreTextBox";
            this.probeAOMFreqCentreTextBox.ReadOnly = true;
            this.probeAOMFreqCentreTextBox.Size = new System.Drawing.Size(126, 20);
            this.probeAOMFreqCentreTextBox.TabIndex = 61;
            this.probeAOMFreqCentreTextBox.Text = "0";
            // 
            // label71
            // 
            this.label71.Location = new System.Drawing.Point(420, 46);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(111, 23);
            this.label71.TabIndex = 63;
            this.label71.Text = "AOM freq high (Hz)";
            // 
            // probeAOMFreqPlusTextBox
            // 
            this.probeAOMFreqPlusTextBox.BackColor = System.Drawing.Color.Black;
            this.probeAOMFreqPlusTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.probeAOMFreqPlusTextBox.Location = new System.Drawing.Point(549, 17);
            this.probeAOMFreqPlusTextBox.Name = "probeAOMFreqPlusTextBox";
            this.probeAOMFreqPlusTextBox.ReadOnly = true;
            this.probeAOMFreqPlusTextBox.Size = new System.Drawing.Size(126, 20);
            this.probeAOMFreqPlusTextBox.TabIndex = 60;
            this.probeAOMFreqPlusTextBox.Text = "0";
            // 
            // label72
            // 
            this.label72.Location = new System.Drawing.Point(420, 74);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(111, 23);
            this.label72.TabIndex = 57;
            this.label72.Text = "Centre (Hz)";
            // 
            // probeAOMFreqUpdateButton
            // 
            this.probeAOMFreqUpdateButton.Location = new System.Drawing.Point(600, 123);
            this.probeAOMFreqUpdateButton.Name = "probeAOMFreqUpdateButton";
            this.probeAOMFreqUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.probeAOMFreqUpdateButton.TabIndex = 59;
            this.probeAOMFreqUpdateButton.Text = "Update";
            this.probeAOMFreqUpdateButton.Click += new System.EventHandler(this.probeAOMFreqUpdateButton_Click);
            // 
            // label69
            // 
            this.label69.Location = new System.Drawing.Point(420, 20);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(111, 23);
            this.label69.TabIndex = 58;
            this.label69.Text = "AOM freq low (Hz)";
            // 
            // probeAOMtrackBar
            // 
            this.probeAOMtrackBar.Location = new System.Drawing.Point(6, 94);
            this.probeAOMtrackBar.Maximum = 1000;
            this.probeAOMtrackBar.Name = "probeAOMtrackBar";
            this.probeAOMtrackBar.Size = new System.Drawing.Size(287, 45);
            this.probeAOMtrackBar.TabIndex = 49;
            this.probeAOMtrackBar.Scroll += new System.EventHandler(this.probeAOMtrackBar_Scroll);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.probeAOMStepZeroButton);
            this.panel5.Controls.Add(this.probeAOMStepPlusButton);
            this.panel5.Controls.Add(this.probeAOMMinusButton);
            this.panel5.Location = new System.Drawing.Point(182, 20);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(111, 32);
            this.panel5.TabIndex = 48;
            // 
            // probeAOMStepZeroButton
            // 
            this.probeAOMStepZeroButton.AutoSize = true;
            this.probeAOMStepZeroButton.Checked = true;
            this.probeAOMStepZeroButton.Location = new System.Drawing.Point(77, 7);
            this.probeAOMStepZeroButton.Name = "probeAOMStepZeroButton";
            this.probeAOMStepZeroButton.Size = new System.Drawing.Size(31, 17);
            this.probeAOMStepZeroButton.TabIndex = 32;
            this.probeAOMStepZeroButton.TabStop = true;
            this.probeAOMStepZeroButton.Text = "0";
            this.probeAOMStepZeroButton.UseVisualStyleBackColor = true;
            // 
            // probeAOMStepPlusButton
            // 
            this.probeAOMStepPlusButton.AutoSize = true;
            this.probeAOMStepPlusButton.Location = new System.Drawing.Point(3, 6);
            this.probeAOMStepPlusButton.Name = "probeAOMStepPlusButton";
            this.probeAOMStepPlusButton.Size = new System.Drawing.Size(31, 17);
            this.probeAOMStepPlusButton.TabIndex = 32;
            this.probeAOMStepPlusButton.Text = "+";
            this.probeAOMStepPlusButton.UseVisualStyleBackColor = true;
            // 
            // probeAOMMinusButton
            // 
            this.probeAOMMinusButton.AutoSize = true;
            this.probeAOMMinusButton.Location = new System.Drawing.Point(42, 7);
            this.probeAOMMinusButton.Name = "probeAOMMinusButton";
            this.probeAOMMinusButton.Size = new System.Drawing.Size(28, 17);
            this.probeAOMMinusButton.TabIndex = 32;
            this.probeAOMMinusButton.Text = "-";
            this.probeAOMMinusButton.UseVisualStyleBackColor = true;
            // 
            // probeAOMStepTextBox
            // 
            this.probeAOMStepTextBox.Location = new System.Drawing.Point(96, 48);
            this.probeAOMStepTextBox.Name = "probeAOMStepTextBox";
            this.probeAOMStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.probeAOMStepTextBox.TabIndex = 47;
            this.probeAOMStepTextBox.Text = "0";
            // 
            // label70
            // 
            this.label70.Location = new System.Drawing.Point(16, 48);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(80, 23);
            this.label70.TabIndex = 46;
            this.label70.Text = "Step (V)";
            // 
            // probeAOMVTextBox
            // 
            this.probeAOMVTextBox.Location = new System.Drawing.Point(96, 24);
            this.probeAOMVTextBox.Name = "probeAOMVTextBox";
            this.probeAOMVTextBox.Size = new System.Drawing.Size(64, 20);
            this.probeAOMVTextBox.TabIndex = 45;
            this.probeAOMVTextBox.Text = "0";
            // 
            // UpdateProbeAOMButton
            // 
            this.UpdateProbeAOMButton.Location = new System.Drawing.Point(202, 58);
            this.UpdateProbeAOMButton.Name = "UpdateProbeAOMButton";
            this.UpdateProbeAOMButton.Size = new System.Drawing.Size(75, 23);
            this.UpdateProbeAOMButton.TabIndex = 40;
            this.UpdateProbeAOMButton.Text = "Update";
            this.UpdateProbeAOMButton.Click += new System.EventHandler(this.UpdateProbeAOMButton_Click);
            // 
            // label68
            // 
            this.label68.Location = new System.Drawing.Point(16, 24);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(80, 23);
            this.label68.TabIndex = 36;
            this.label68.Text = "Voltage (V)";
            // 
            // tabPage8
            // 
            this.tabPage8.BackColor = System.Drawing.Color.Transparent;
            this.tabPage8.Location = new System.Drawing.Point(4, 22);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage8.Size = new System.Drawing.Size(697, 575);
            this.tabPage8.TabIndex = 7;
            this.tabPage8.Text = "P(1) Lasers";
            // 
            // tabPage13
            // 
            this.tabPage13.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage13.Controls.Add(this.label93);
            this.tabPage13.Controls.Add(this.anapicofreqcontrol);
            this.tabPage13.Controls.Add(this.groupBox43);
            this.tabPage13.Controls.Add(this.groupBox41);
            this.tabPage13.Location = new System.Drawing.Point(4, 22);
            this.tabPage13.Name = "tabPage13";
            this.tabPage13.Size = new System.Drawing.Size(697, 575);
            this.tabPage13.TabIndex = 10;
            this.tabPage13.Text = "Microwaves";
            // 
            // label93
            // 
            this.label93.Location = new System.Drawing.Point(281, 144);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(64, 17);
            this.label93.TabIndex = 69;
            this.label93.Text = "Dwell off (s)";
            // 
            // anapicofreqcontrol
            // 
            this.anapicofreqcontrol.Controls.Add(this.label120);
            this.anapicofreqcontrol.Controls.Add(this.label118);
            this.anapicofreqcontrol.Controls.Add(this.topProbeMWf1Indicator);
            this.anapicofreqcontrol.Controls.Add(this.label119);
            this.anapicofreqcontrol.Controls.Add(this.topProbeMWf0Indicator);
            this.anapicofreqcontrol.Controls.Add(this.label116);
            this.anapicofreqcontrol.Controls.Add(this.bottomProbeMWf1Indicator);
            this.anapicofreqcontrol.Controls.Add(this.label117);
            this.anapicofreqcontrol.Controls.Add(this.bottomProbeMWf0Indicator);
            this.anapicofreqcontrol.Controls.Add(this.label115);
            this.anapicofreqcontrol.Controls.Add(this.pumpMWf1Indicator);
            this.anapicofreqcontrol.Controls.Add(this.label94);
            this.anapicofreqcontrol.Controls.Add(this.pumpMWf0Indicator);
            this.anapicofreqcontrol.Controls.Add(this.getCurrentListButton);
            this.anapicofreqcontrol.Controls.Add(this.displayCurrentListBox);
            this.anapicofreqcontrol.Controls.Add(this.anapicof1FreqTextBox);
            this.anapicofreqcontrol.Controls.Add(this.label91);
            this.anapicofreqcontrol.Controls.Add(this.microwaveStateCheckBox);
            this.anapicofreqcontrol.Controls.Add(this.topProbeMWDwellOffTextBox);
            this.anapicofreqcontrol.Controls.Add(this.topProbeMWDwellOnTextBox);
            this.anapicofreqcontrol.Controls.Add(this.bottomProbeMWDwellOffTextBox);
            this.anapicofreqcontrol.Controls.Add(this.bottomProbeMWDwellOnTextBox);
            this.anapicofreqcontrol.Controls.Add(this.pumpMWDwellOffTextBox);
            this.anapicofreqcontrol.Controls.Add(this.pumpMWDwellOnTextBox);
            this.anapicofreqcontrol.Controls.Add(this.label92);
            this.anapicofreqcontrol.Controls.Add(this.anapicof0FreqTextBox);
            this.anapicofreqcontrol.Controls.Add(this.label90);
            this.anapicofreqcontrol.Controls.Add(this.label89);
            this.anapicofreqcontrol.Controls.Add(this.label87);
            this.anapicofreqcontrol.Controls.Add(this.label86);
            this.anapicofreqcontrol.Controls.Add(this.listSweepEnabledCheckBox);
            this.anapicofreqcontrol.Controls.Add(this.anapicoCwFreqUpdate);
            this.anapicofreqcontrol.Controls.Add(this.anapicoEnabledCheckBox);
            this.anapicofreqcontrol.Controls.Add(this.anapicoCwFreqBox);
            this.anapicofreqcontrol.Controls.Add(this.cwfreqlabel);
            this.anapicofreqcontrol.Location = new System.Drawing.Point(13, 12);
            this.anapicofreqcontrol.Name = "anapicofreqcontrol";
            this.anapicofreqcontrol.Size = new System.Drawing.Size(355, 323);
            this.anapicofreqcontrol.TabIndex = 72;
            this.anapicofreqcontrol.TabStop = false;
            this.anapicofreqcontrol.Text = "Anapico Frequency Control";
            // 
            // label120
            // 
            this.label120.Location = new System.Drawing.Point(31, 110);
            this.label120.Name = "label120";
            this.label120.Size = new System.Drawing.Size(320, 23);
            this.label120.TabIndex = 73;
            this.label120.Text = " (1 is checked, correponding to f0 in bottom probe, f1 in top probe)";
            // 
            // label118
            // 
            this.label118.Location = new System.Drawing.Point(186, 205);
            this.label118.Name = "label118";
            this.label118.Size = new System.Drawing.Size(22, 18);
            this.label118.TabIndex = 90;
            this.label118.Text = "f1";
            // 
            // topProbeMWf1Indicator
            // 
            this.topProbeMWf1Indicator.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.topProbeMWf1Indicator.Location = new System.Drawing.Point(170, 202);
            this.topProbeMWf1Indicator.Name = "topProbeMWf1Indicator";
            this.topProbeMWf1Indicator.Size = new System.Drawing.Size(21, 22);
            this.topProbeMWf1Indicator.TabIndex = 89;
            this.topProbeMWf1Indicator.Value = true;
            // 
            // label119
            // 
            this.label119.Location = new System.Drawing.Point(153, 205);
            this.label119.Name = "label119";
            this.label119.Size = new System.Drawing.Size(22, 18);
            this.label119.TabIndex = 88;
            this.label119.Text = "f0";
            // 
            // topProbeMWf0Indicator
            // 
            this.topProbeMWf0Indicator.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.topProbeMWf0Indicator.Location = new System.Drawing.Point(137, 202);
            this.topProbeMWf0Indicator.Name = "topProbeMWf0Indicator";
            this.topProbeMWf0Indicator.Size = new System.Drawing.Size(21, 22);
            this.topProbeMWf0Indicator.TabIndex = 87;
            // 
            // label116
            // 
            this.label116.Location = new System.Drawing.Point(186, 181);
            this.label116.Name = "label116";
            this.label116.Size = new System.Drawing.Size(22, 18);
            this.label116.TabIndex = 86;
            this.label116.Text = "f1";
            // 
            // bottomProbeMWf1Indicator
            // 
            this.bottomProbeMWf1Indicator.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.bottomProbeMWf1Indicator.Location = new System.Drawing.Point(170, 178);
            this.bottomProbeMWf1Indicator.Name = "bottomProbeMWf1Indicator";
            this.bottomProbeMWf1Indicator.Size = new System.Drawing.Size(21, 22);
            this.bottomProbeMWf1Indicator.TabIndex = 85;
            // 
            // label117
            // 
            this.label117.Location = new System.Drawing.Point(153, 181);
            this.label117.Name = "label117";
            this.label117.Size = new System.Drawing.Size(22, 18);
            this.label117.TabIndex = 84;
            this.label117.Text = "f0";
            // 
            // bottomProbeMWf0Indicator
            // 
            this.bottomProbeMWf0Indicator.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.bottomProbeMWf0Indicator.Location = new System.Drawing.Point(137, 178);
            this.bottomProbeMWf0Indicator.Name = "bottomProbeMWf0Indicator";
            this.bottomProbeMWf0Indicator.Size = new System.Drawing.Size(21, 22);
            this.bottomProbeMWf0Indicator.TabIndex = 83;
            this.bottomProbeMWf0Indicator.Value = true;
            // 
            // label115
            // 
            this.label115.Location = new System.Drawing.Point(186, 155);
            this.label115.Name = "label115";
            this.label115.Size = new System.Drawing.Size(22, 18);
            this.label115.TabIndex = 82;
            this.label115.Text = "f1";
            // 
            // pumpMWf1Indicator
            // 
            this.pumpMWf1Indicator.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.pumpMWf1Indicator.Location = new System.Drawing.Point(170, 152);
            this.pumpMWf1Indicator.Name = "pumpMWf1Indicator";
            this.pumpMWf1Indicator.Size = new System.Drawing.Size(21, 22);
            this.pumpMWf1Indicator.TabIndex = 81;
            this.pumpMWf1Indicator.Value = true;
            // 
            // label94
            // 
            this.label94.Location = new System.Drawing.Point(153, 155);
            this.label94.Name = "label94";
            this.label94.Size = new System.Drawing.Size(22, 18);
            this.label94.TabIndex = 80;
            this.label94.Text = "f0";
            // 
            // pumpMWf0Indicator
            // 
            this.pumpMWf0Indicator.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.pumpMWf0Indicator.Location = new System.Drawing.Point(137, 152);
            this.pumpMWf0Indicator.Name = "pumpMWf0Indicator";
            this.pumpMWf0Indicator.Size = new System.Drawing.Size(21, 22);
            this.pumpMWf0Indicator.TabIndex = 79;
            // 
            // getCurrentListButton
            // 
            this.getCurrentListButton.Location = new System.Drawing.Point(21, 265);
            this.getCurrentListButton.Name = "getCurrentListButton";
            this.getCurrentListButton.Size = new System.Drawing.Size(75, 43);
            this.getCurrentListButton.TabIndex = 78;
            this.getCurrentListButton.Text = "Get current list";
            this.getCurrentListButton.Click += new System.EventHandler(this.getCurrentListButton_Click);
            // 
            // displayCurrentListBox
            // 
            this.displayCurrentListBox.BackColor = System.Drawing.Color.Black;
            this.displayCurrentListBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.displayCurrentListBox.Location = new System.Drawing.Point(106, 255);
            this.displayCurrentListBox.Multiline = true;
            this.displayCurrentListBox.Name = "displayCurrentListBox";
            this.displayCurrentListBox.ReadOnly = true;
            this.displayCurrentListBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.displayCurrentListBox.Size = new System.Drawing.Size(235, 62);
            this.displayCurrentListBox.TabIndex = 77;
            this.displayCurrentListBox.Text = "0";
            // 
            // anapicof1FreqTextBox
            // 
            this.anapicof1FreqTextBox.Location = new System.Drawing.Point(180, 229);
            this.anapicof1FreqTextBox.Name = "anapicof1FreqTextBox";
            this.anapicof1FreqTextBox.Size = new System.Drawing.Size(75, 20);
            this.anapicof1FreqTextBox.TabIndex = 75;
            this.anapicof1FreqTextBox.Text = "14458087000";
            // 
            // label91
            // 
            this.label91.Location = new System.Drawing.Point(143, 232);
            this.label91.Name = "label91";
            this.label91.Size = new System.Drawing.Size(48, 20);
            this.label91.TabIndex = 76;
            this.label91.Text = "Set f1";
            // 
            // microwaveStateCheckBox
            // 
            this.microwaveStateCheckBox.Checked = true;
            this.microwaveStateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.microwaveStateCheckBox.Location = new System.Drawing.Point(19, 89);
            this.microwaveStateCheckBox.Name = "microwaveStateCheckBox";
            this.microwaveStateCheckBox.Size = new System.Drawing.Size(293, 24);
            this.microwaveStateCheckBox.TabIndex = 74;
            this.microwaveStateCheckBox.Text = "Switch state ";
            this.microwaveStateCheckBox.CheckedChanged += new System.EventHandler(this.microwaveStateCheckBox_CheckedChanged);
            // 
            // topProbeMWDwellOffTextBox
            // 
            this.topProbeMWDwellOffTextBox.Location = new System.Drawing.Point(271, 200);
            this.topProbeMWDwellOffTextBox.Name = "topProbeMWDwellOffTextBox";
            this.topProbeMWDwellOffTextBox.Size = new System.Drawing.Size(61, 20);
            this.topProbeMWDwellOffTextBox.TabIndex = 73;
            this.topProbeMWDwellOffTextBox.Text = "1E-5";
            // 
            // topProbeMWDwellOnTextBox
            // 
            this.topProbeMWDwellOnTextBox.Location = new System.Drawing.Point(209, 201);
            this.topProbeMWDwellOnTextBox.Name = "topProbeMWDwellOnTextBox";
            this.topProbeMWDwellOnTextBox.Size = new System.Drawing.Size(61, 20);
            this.topProbeMWDwellOnTextBox.TabIndex = 72;
            this.topProbeMWDwellOnTextBox.Text = "1E-3";
            // 
            // bottomProbeMWDwellOffTextBox
            // 
            this.bottomProbeMWDwellOffTextBox.Location = new System.Drawing.Point(271, 177);
            this.bottomProbeMWDwellOffTextBox.Name = "bottomProbeMWDwellOffTextBox";
            this.bottomProbeMWDwellOffTextBox.Size = new System.Drawing.Size(61, 20);
            this.bottomProbeMWDwellOffTextBox.TabIndex = 71;
            this.bottomProbeMWDwellOffTextBox.Text = "1E-5";
            // 
            // bottomProbeMWDwellOnTextBox
            // 
            this.bottomProbeMWDwellOnTextBox.Location = new System.Drawing.Point(209, 178);
            this.bottomProbeMWDwellOnTextBox.Name = "bottomProbeMWDwellOnTextBox";
            this.bottomProbeMWDwellOnTextBox.Size = new System.Drawing.Size(61, 20);
            this.bottomProbeMWDwellOnTextBox.TabIndex = 70;
            this.bottomProbeMWDwellOnTextBox.Text = "1.52E-3";
            // 
            // pumpMWDwellOffTextBox
            // 
            this.pumpMWDwellOffTextBox.Location = new System.Drawing.Point(271, 152);
            this.pumpMWDwellOffTextBox.Name = "pumpMWDwellOffTextBox";
            this.pumpMWDwellOffTextBox.Size = new System.Drawing.Size(61, 20);
            this.pumpMWDwellOffTextBox.TabIndex = 69;
            this.pumpMWDwellOffTextBox.Text = "1E-5";
            // 
            // pumpMWDwellOnTextBox
            // 
            this.pumpMWDwellOnTextBox.Location = new System.Drawing.Point(209, 153);
            this.pumpMWDwellOnTextBox.Name = "pumpMWDwellOnTextBox";
            this.pumpMWDwellOnTextBox.Size = new System.Drawing.Size(61, 20);
            this.pumpMWDwellOnTextBox.TabIndex = 59;
            this.pumpMWDwellOnTextBox.Text = "1E-3";
            // 
            // label92
            // 
            this.label92.Location = new System.Drawing.Point(206, 132);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(64, 17);
            this.label92.TabIndex = 57;
            this.label92.Text = "Dwell on (s)";
            this.label92.Click += new System.EventHandler(this.label92_Click);
            // 
            // anapicof0FreqTextBox
            // 
            this.anapicof0FreqTextBox.Location = new System.Drawing.Point(55, 229);
            this.anapicof0FreqTextBox.Name = "anapicof0FreqTextBox";
            this.anapicof0FreqTextBox.Size = new System.Drawing.Size(75, 20);
            this.anapicof0FreqTextBox.TabIndex = 59;
            this.anapicof0FreqTextBox.Text = "14467242000";
            this.anapicof0FreqTextBox.TextChanged += new System.EventHandler(this.anapicof0FreqTextBox_TextChanged);
            // 
            // label90
            // 
            this.label90.Location = new System.Drawing.Point(18, 232);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(48, 20);
            this.label90.TabIndex = 59;
            this.label90.Text = "Set f0";
            this.label90.Click += new System.EventHandler(this.label90_Click);
            // 
            // label89
            // 
            this.label89.Location = new System.Drawing.Point(17, 205);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(133, 18);
            this.label89.TabIndex = 63;
            this.label89.Text = "Top probe microwaves";
            // 
            // label87
            // 
            this.label87.Location = new System.Drawing.Point(17, 180);
            this.label87.Name = "label87";
            this.label87.Size = new System.Drawing.Size(133, 18);
            this.label87.TabIndex = 62;
            this.label87.Text = "Bottom probe microwaves";
            // 
            // label86
            // 
            this.label86.Location = new System.Drawing.Point(17, 156);
            this.label86.Name = "label86";
            this.label86.Size = new System.Drawing.Size(101, 18);
            this.label86.TabIndex = 59;
            this.label86.Text = "Pump microwaves";
            this.label86.Click += new System.EventHandler(this.label86_Click);
            // 
            // listSweepEnabledCheckBox
            // 
            this.listSweepEnabledCheckBox.Location = new System.Drawing.Point(19, 63);
            this.listSweepEnabledCheckBox.Name = "listSweepEnabledCheckBox";
            this.listSweepEnabledCheckBox.Size = new System.Drawing.Size(130, 24);
            this.listSweepEnabledCheckBox.TabIndex = 60;
            this.listSweepEnabledCheckBox.Text = "List Sweep enabled";
            this.listSweepEnabledCheckBox.CheckedChanged += new System.EventHandler(this.listSweepEnabledCheckBox_CheckedChanged);
            // 
            // anapicoCwFreqUpdate
            // 
            this.anapicoCwFreqUpdate.Location = new System.Drawing.Point(205, 40);
            this.anapicoCwFreqUpdate.Name = "anapicoCwFreqUpdate";
            this.anapicoCwFreqUpdate.Size = new System.Drawing.Size(75, 23);
            this.anapicoCwFreqUpdate.TabIndex = 59;
            this.anapicoCwFreqUpdate.Text = "Update";
            this.anapicoCwFreqUpdate.Click += new System.EventHandler(this.anapicoCwFreqUpdate_Click);
            // 
            // anapicoEnabledCheckBox
            // 
            this.anapicoEnabledCheckBox.Location = new System.Drawing.Point(19, 18);
            this.anapicoEnabledCheckBox.Name = "anapicoEnabledCheckBox";
            this.anapicoEnabledCheckBox.Size = new System.Drawing.Size(104, 24);
            this.anapicoEnabledCheckBox.TabIndex = 57;
            this.anapicoEnabledCheckBox.Text = "Anapico on";
            this.anapicoEnabledCheckBox.CheckedChanged += new System.EventHandler(this.anapicoEnabledCheckBox_CheckedChanged);
            // 
            // anapicoCwFreqBox
            // 
            this.anapicoCwFreqBox.Location = new System.Drawing.Point(121, 41);
            this.anapicoCwFreqBox.Name = "anapicoCwFreqBox";
            this.anapicoCwFreqBox.Size = new System.Drawing.Size(78, 20);
            this.anapicoCwFreqBox.TabIndex = 55;
            this.anapicoCwFreqBox.Text = "14452087000";
            this.anapicoCwFreqBox.TextChanged += new System.EventHandler(this.anapicoCwFreqBox_TextChanged);
            // 
            // cwfreqlabel
            // 
            this.cwfreqlabel.Location = new System.Drawing.Point(16, 43);
            this.cwfreqlabel.Name = "cwfreqlabel";
            this.cwfreqlabel.Size = new System.Drawing.Size(107, 23);
            this.cwfreqlabel.TabIndex = 54;
            this.cwfreqlabel.Text = "CW Frequency (Hz)";
            // 
            // groupBox43
            // 
            this.groupBox43.Controls.Add(this.bottomProbeMixerVoltageTextBox);
            this.groupBox43.Controls.Add(this.mixerVoltateUpdateButton);
            this.groupBox43.Controls.Add(this.topProbeMixerVoltageTrackBar);
            this.groupBox43.Controls.Add(this.topProbeMixerVoltageTextBox);
            this.groupBox43.Controls.Add(this.bottomProbeMixerVoltageTrackBar);
            this.groupBox43.Controls.Add(this.secondProbeHornCheckBox);
            this.groupBox43.Controls.Add(this.firstProbeHornCheckBox);
            this.groupBox43.Controls.Add(this.pumpMixerVoltageTrackBar);
            this.groupBox43.Controls.Add(this.pumpHornCheckBox);
            this.groupBox43.Controls.Add(this.mwEnableCheckBox);
            this.groupBox43.Controls.Add(this.label155);
            this.groupBox43.Controls.Add(this.pumpMixerVoltageTextBox);
            this.groupBox43.Location = new System.Drawing.Point(13, 341);
            this.groupBox43.Name = "groupBox43";
            this.groupBox43.Size = new System.Drawing.Size(661, 224);
            this.groupBox43.TabIndex = 71;
            this.groupBox43.TabStop = false;
            this.groupBox43.Text = "Microwave Switches";
            // 
            // bottomProbeMixerVoltageTextBox
            // 
            this.bottomProbeMixerVoltageTextBox.Location = new System.Drawing.Point(294, 103);
            this.bottomProbeMixerVoltageTextBox.Name = "bottomProbeMixerVoltageTextBox";
            this.bottomProbeMixerVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.bottomProbeMixerVoltageTextBox.TabIndex = 58;
            this.bottomProbeMixerVoltageTextBox.Text = "0";
            // 
            // mixerVoltateUpdateButton
            // 
            this.mixerVoltateUpdateButton.Location = new System.Drawing.Point(294, 196);
            this.mixerVoltateUpdateButton.Name = "mixerVoltateUpdateButton";
            this.mixerVoltateUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.mixerVoltateUpdateButton.TabIndex = 57;
            this.mixerVoltateUpdateButton.Text = "Update";
            this.mixerVoltateUpdateButton.Click += new System.EventHandler(this.mixerVoltateUpdateButton_Click);
            // 
            // topProbeMixerVoltageTrackBar
            // 
            this.topProbeMixerVoltageTrackBar.Location = new System.Drawing.Point(364, 145);
            this.topProbeMixerVoltageTrackBar.Maximum = 500;
            this.topProbeMixerVoltageTrackBar.Name = "topProbeMixerVoltageTrackBar";
            this.topProbeMixerVoltageTrackBar.Size = new System.Drawing.Size(287, 45);
            this.topProbeMixerVoltageTrackBar.TabIndex = 56;
            this.topProbeMixerVoltageTrackBar.Scroll += new System.EventHandler(this.topProbeMixerVoltageTrackBar_Scroll);
            // 
            // topProbeMixerVoltageTextBox
            // 
            this.topProbeMixerVoltageTextBox.Location = new System.Drawing.Point(294, 147);
            this.topProbeMixerVoltageTextBox.Name = "topProbeMixerVoltageTextBox";
            this.topProbeMixerVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.topProbeMixerVoltageTextBox.TabIndex = 55;
            this.topProbeMixerVoltageTextBox.Text = "0";
            // 
            // bottomProbeMixerVoltageTrackBar
            // 
            this.bottomProbeMixerVoltageTrackBar.Location = new System.Drawing.Point(364, 100);
            this.bottomProbeMixerVoltageTrackBar.Maximum = 500;
            this.bottomProbeMixerVoltageTrackBar.Name = "bottomProbeMixerVoltageTrackBar";
            this.bottomProbeMixerVoltageTrackBar.Size = new System.Drawing.Size(287, 45);
            this.bottomProbeMixerVoltageTrackBar.TabIndex = 54;
            this.bottomProbeMixerVoltageTrackBar.Scroll += new System.EventHandler(this.bottomProbeMixerVoltageTrackBar_Scroll);
            // 
            // secondProbeHornCheckBox
            // 
            this.secondProbeHornCheckBox.Location = new System.Drawing.Point(102, 144);
            this.secondProbeHornCheckBox.Name = "secondProbeHornCheckBox";
            this.secondProbeHornCheckBox.Size = new System.Drawing.Size(191, 24);
            this.secondProbeHornCheckBox.TabIndex = 26;
            this.secondProbeHornCheckBox.Text = "Microwaves to top probe horns";
            this.secondProbeHornCheckBox.CheckedChanged += new System.EventHandler(this.secondProbeHornCheckBox_CheckedChanged);
            // 
            // firstProbeHornCheckBox
            // 
            this.firstProbeHornCheckBox.Location = new System.Drawing.Point(102, 99);
            this.firstProbeHornCheckBox.Name = "firstProbeHornCheckBox";
            this.firstProbeHornCheckBox.Size = new System.Drawing.Size(191, 24);
            this.firstProbeHornCheckBox.TabIndex = 25;
            this.firstProbeHornCheckBox.Text = "Microwaves to bottom probe horns";
            this.firstProbeHornCheckBox.CheckedChanged += new System.EventHandler(this.firstProbeHornCheckBox_CheckedChanged);
            // 
            // pumpMixerVoltageTrackBar
            // 
            this.pumpMixerVoltageTrackBar.Location = new System.Drawing.Point(364, 50);
            this.pumpMixerVoltageTrackBar.Maximum = 500;
            this.pumpMixerVoltageTrackBar.Name = "pumpMixerVoltageTrackBar";
            this.pumpMixerVoltageTrackBar.Size = new System.Drawing.Size(287, 45);
            this.pumpMixerVoltageTrackBar.TabIndex = 52;
            this.pumpMixerVoltageTrackBar.Scroll += new System.EventHandler(this.mixerVoltageTrackBar_Scroll);
            // 
            // pumpHornCheckBox
            // 
            this.pumpHornCheckBox.Location = new System.Drawing.Point(102, 56);
            this.pumpHornCheckBox.Name = "pumpHornCheckBox";
            this.pumpHornCheckBox.Size = new System.Drawing.Size(160, 24);
            this.pumpHornCheckBox.TabIndex = 24;
            this.pumpHornCheckBox.Text = "Microwaves to pump Horn";
            this.pumpHornCheckBox.CheckedChanged += new System.EventHandler(this.pumpHornCheckBox_CheckedChanged);
            // 
            // mwEnableCheckBox
            // 
            this.mwEnableCheckBox.Location = new System.Drawing.Point(102, 19);
            this.mwEnableCheckBox.Name = "mwEnableCheckBox";
            this.mwEnableCheckBox.Size = new System.Drawing.Size(160, 24);
            this.mwEnableCheckBox.TabIndex = 23;
            this.mwEnableCheckBox.Text = "Enable microwaves ";
            this.mwEnableCheckBox.CheckedChanged += new System.EventHandler(this.mwEnableCheckBox_CheckedChanged);
            // 
            // label155
            // 
            this.label155.Location = new System.Drawing.Point(291, 24);
            this.label155.Name = "label155";
            this.label155.Size = new System.Drawing.Size(101, 23);
            this.label155.TabIndex = 46;
            this.label155.Text = "Mixer Voltage (V)";
            // 
            // pumpMixerVoltageTextBox
            // 
            this.pumpMixerVoltageTextBox.Location = new System.Drawing.Point(294, 58);
            this.pumpMixerVoltageTextBox.Name = "pumpMixerVoltageTextBox";
            this.pumpMixerVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.pumpMixerVoltageTextBox.TabIndex = 47;
            this.pumpMixerVoltageTextBox.Text = "0";
            // 
            // groupBox41
            // 
            this.groupBox41.Controls.Add(this.uWaveDCFMMinusButton);
            this.groupBox41.Controls.Add(this.uWaveDCFMStepTextBox);
            this.groupBox41.Controls.Add(this.label152);
            this.groupBox41.Controls.Add(this.uWaveDCFMPlusButton);
            this.groupBox41.Controls.Add(this.label154);
            this.groupBox41.Controls.Add(this.uWaveDCFMTrackBar);
            this.groupBox41.Controls.Add(this.uWaveUpdateButton);
            this.groupBox41.Controls.Add(this.uWaveDCFMTextBox);
            this.groupBox41.Controls.Add(this.label156);
            this.groupBox41.Location = new System.Drawing.Point(374, 12);
            this.groupBox41.Name = "groupBox41";
            this.groupBox41.Size = new System.Drawing.Size(311, 175);
            this.groupBox41.TabIndex = 70;
            this.groupBox41.TabStop = false;
            this.groupBox41.Text = "DCFM";
            // 
            // uWaveDCFMMinusButton
            // 
            this.uWaveDCFMMinusButton.Location = new System.Drawing.Point(225, 19);
            this.uWaveDCFMMinusButton.Name = "uWaveDCFMMinusButton";
            this.uWaveDCFMMinusButton.Size = new System.Drawing.Size(37, 23);
            this.uWaveDCFMMinusButton.TabIndex = 56;
            this.uWaveDCFMMinusButton.Text = "-";
            this.uWaveDCFMMinusButton.UseVisualStyleBackColor = true;
            this.uWaveDCFMMinusButton.Click += new System.EventHandler(this.uWaveDCFMMinusButton_Click);
            // 
            // uWaveDCFMStepTextBox
            // 
            this.uWaveDCFMStepTextBox.Location = new System.Drawing.Point(112, 48);
            this.uWaveDCFMStepTextBox.Name = "uWaveDCFMStepTextBox";
            this.uWaveDCFMStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.uWaveDCFMStepTextBox.TabIndex = 55;
            this.uWaveDCFMStepTextBox.Text = "0.1";
            this.uWaveDCFMStepTextBox.TextChanged += new System.EventHandler(this.uWaveDCFMStepTextBox_TextChanged);
            // 
            // label152
            // 
            this.label152.Location = new System.Drawing.Point(16, 51);
            this.label152.Name = "label152";
            this.label152.Size = new System.Drawing.Size(90, 23);
            this.label152.TabIndex = 54;
            this.label152.Text = "Step DCFM (V)";
            // 
            // uWaveDCFMPlusButton
            // 
            this.uWaveDCFMPlusButton.Location = new System.Drawing.Point(182, 19);
            this.uWaveDCFMPlusButton.Name = "uWaveDCFMPlusButton";
            this.uWaveDCFMPlusButton.Size = new System.Drawing.Size(37, 23);
            this.uWaveDCFMPlusButton.TabIndex = 53;
            this.uWaveDCFMPlusButton.Text = "+";
            this.uWaveDCFMPlusButton.UseVisualStyleBackColor = true;
            this.uWaveDCFMPlusButton.Click += new System.EventHandler(this.uWaveDCFMPlusButton_Click);
            // 
            // label154
            // 
            this.label154.Location = new System.Drawing.Point(16, 78);
            this.label154.Name = "label154";
            this.label154.Size = new System.Drawing.Size(90, 23);
            this.label154.TabIndex = 50;
            this.label154.Text = "DCFM Voltage";
            // 
            // uWaveDCFMTrackBar
            // 
            this.uWaveDCFMTrackBar.Location = new System.Drawing.Point(6, 99);
            this.uWaveDCFMTrackBar.Maximum = 250;
            this.uWaveDCFMTrackBar.Minimum = -250;
            this.uWaveDCFMTrackBar.Name = "uWaveDCFMTrackBar";
            this.uWaveDCFMTrackBar.Size = new System.Drawing.Size(287, 45);
            this.uWaveDCFMTrackBar.TabIndex = 49;
            this.uWaveDCFMTrackBar.Scroll += new System.EventHandler(this.uWaveDCFMTrackBar_Scroll);
            // 
            // uWaveUpdateButton
            // 
            this.uWaveUpdateButton.Location = new System.Drawing.Point(19, 146);
            this.uWaveUpdateButton.Name = "uWaveUpdateButton";
            this.uWaveUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.uWaveUpdateButton.TabIndex = 40;
            this.uWaveUpdateButton.Text = "Update";
            this.uWaveUpdateButton.Click += new System.EventHandler(this.uWaveUpdateButton_Click);
            // 
            // uWaveDCFMTextBox
            // 
            this.uWaveDCFMTextBox.Location = new System.Drawing.Point(112, 21);
            this.uWaveDCFMTextBox.Name = "uWaveDCFMTextBox";
            this.uWaveDCFMTextBox.Size = new System.Drawing.Size(64, 20);
            this.uWaveDCFMTextBox.TabIndex = 45;
            this.uWaveDCFMTextBox.Text = "0";
            this.uWaveDCFMTextBox.TextChanged += new System.EventHandler(this.uWaveDCFMTextBox_TextChanged);
            // 
            // label156
            // 
            this.label156.Location = new System.Drawing.Point(16, 24);
            this.label156.Name = "label156";
            this.label156.Size = new System.Drawing.Size(103, 23);
            this.label156.TabIndex = 36;
            this.label156.Text = "DCFM Voltage (V)";
            // 
            // tabPage12
            // 
            this.tabPage12.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage12.Controls.Add(this.groupBox42);
            this.tabPage12.Controls.Add(this.groupBox40);
            this.tabPage12.Controls.Add(this.groupBox39);
            this.tabPage12.Controls.Add(this.groupBox161MHzVCO);
            this.tabPage12.Location = new System.Drawing.Point(4, 22);
            this.tabPage12.Name = "tabPage12";
            this.tabPage12.Size = new System.Drawing.Size(697, 575);
            this.tabPage12.TabIndex = 10;
            this.tabPage12.Text = "RF VCOs";
            // 
            // groupBox42
            // 
            this.groupBox42.Controls.Add(this.rfSensorCheckBox);
            this.groupBox42.Controls.Add(this.pumpRFCheckBox);
            this.groupBox42.Location = new System.Drawing.Point(17, 8);
            this.groupBox42.Name = "groupBox42";
            this.groupBox42.Size = new System.Drawing.Size(316, 113);
            this.groupBox42.TabIndex = 70;
            this.groupBox42.TabStop = false;
            this.groupBox42.Text = "Rf Switches";
            // 
            // rfSensorCheckBox
            // 
            this.rfSensorCheckBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.rfSensorCheckBox.Location = new System.Drawing.Point(11, 49);
            this.rfSensorCheckBox.Name = "rfSensorCheckBox";
            this.rfSensorCheckBox.Size = new System.Drawing.Size(208, 24);
            this.rfSensorCheckBox.TabIndex = 24;
            this.rfSensorCheckBox.Text = "Connect sensors to pump rf output";
            this.rfSensorCheckBox.CheckedChanged += new System.EventHandler(this.rfSensorCheckBox_CheckedChanged);
            // 
            // pumpRFCheckBox
            // 
            this.pumpRFCheckBox.Location = new System.Drawing.Point(11, 19);
            this.pumpRFCheckBox.Name = "pumpRFCheckBox";
            this.pumpRFCheckBox.Size = new System.Drawing.Size(208, 24);
            this.pumpRFCheckBox.TabIndex = 23;
            this.pumpRFCheckBox.Text = "Enable pump rf (check on)";
            this.pumpRFCheckBox.CheckedChanged += new System.EventHandler(this.pumpRFCheckBox_CheckedChanged);
            // 
            // groupBox40
            // 
            this.groupBox40.Controls.Add(this.VCO155FreqStepMinusButton);
            this.groupBox40.Controls.Add(this.VCO155FreqStepPlusButton);
            this.groupBox40.Controls.Add(this.VCO155AmpStepMinusButton);
            this.groupBox40.Controls.Add(this.VCO155AmpStepPlusButton);
            this.groupBox40.Controls.Add(this.VCO155FreqStepTextBox);
            this.groupBox40.Controls.Add(this.label150);
            this.groupBox40.Controls.Add(this.VCO155AmpStepTextBox);
            this.groupBox40.Controls.Add(this.label149);
            this.groupBox40.Controls.Add(this.label144);
            this.groupBox40.Controls.Add(this.label143);
            this.groupBox40.Controls.Add(this.VCO155FreqTrackBar);
            this.groupBox40.Controls.Add(this.VCO155AmpTrackBar);
            this.groupBox40.Controls.Add(this.VCO155FreqVoltageTextBox);
            this.groupBox40.Controls.Add(this.label135);
            this.groupBox40.Controls.Add(this.VCO155AmpVoltageTextBox);
            this.groupBox40.Controls.Add(this.VCO155UpdateButton);
            this.groupBox40.Controls.Add(this.label136);
            this.groupBox40.Location = new System.Drawing.Point(17, 424);
            this.groupBox40.Name = "groupBox40";
            this.groupBox40.Size = new System.Drawing.Size(661, 141);
            this.groupBox40.TabIndex = 71;
            this.groupBox40.TabStop = false;
            this.groupBox40.Text = "155 MHz VCO";
            // 
            // VCO155FreqStepMinusButton
            // 
            this.VCO155FreqStepMinusButton.Location = new System.Drawing.Point(551, 20);
            this.VCO155FreqStepMinusButton.Name = "VCO155FreqStepMinusButton";
            this.VCO155FreqStepMinusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO155FreqStepMinusButton.TabIndex = 71;
            this.VCO155FreqStepMinusButton.Text = "-";
            this.VCO155FreqStepMinusButton.UseVisualStyleBackColor = true;
            this.VCO155FreqStepMinusButton.Click += new System.EventHandler(this.VCO155FreqStepMinusButton_Click);
            // 
            // VCO155FreqStepPlusButton
            // 
            this.VCO155FreqStepPlusButton.Location = new System.Drawing.Point(508, 20);
            this.VCO155FreqStepPlusButton.Name = "VCO155FreqStepPlusButton";
            this.VCO155FreqStepPlusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO155FreqStepPlusButton.TabIndex = 70;
            this.VCO155FreqStepPlusButton.Text = "+";
            this.VCO155FreqStepPlusButton.UseVisualStyleBackColor = true;
            this.VCO155FreqStepPlusButton.Click += new System.EventHandler(this.VCO155FreqStepPlusButton_Click);
            // 
            // VCO155AmpStepMinusButton
            // 
            this.VCO155AmpStepMinusButton.Location = new System.Drawing.Point(225, 20);
            this.VCO155AmpStepMinusButton.Name = "VCO155AmpStepMinusButton";
            this.VCO155AmpStepMinusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO155AmpStepMinusButton.TabIndex = 67;
            this.VCO155AmpStepMinusButton.Text = "-";
            this.VCO155AmpStepMinusButton.UseVisualStyleBackColor = true;
            this.VCO155AmpStepMinusButton.Click += new System.EventHandler(this.VCO155AmpStepMinusButton_Click);
            // 
            // VCO155AmpStepPlusButton
            // 
            this.VCO155AmpStepPlusButton.Location = new System.Drawing.Point(182, 19);
            this.VCO155AmpStepPlusButton.Name = "VCO155AmpStepPlusButton";
            this.VCO155AmpStepPlusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO155AmpStepPlusButton.TabIndex = 67;
            this.VCO155AmpStepPlusButton.Text = "+";
            this.VCO155AmpStepPlusButton.UseVisualStyleBackColor = true;
            this.VCO155AmpStepPlusButton.Click += new System.EventHandler(this.VCO155AmpStepPlusButton_Click);
            // 
            // VCO155FreqStepTextBox
            // 
            this.VCO155FreqStepTextBox.Location = new System.Drawing.Point(438, 50);
            this.VCO155FreqStepTextBox.Name = "VCO155FreqStepTextBox";
            this.VCO155FreqStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO155FreqStepTextBox.TabIndex = 69;
            this.VCO155FreqStepTextBox.Text = "0.1";
            // 
            // label150
            // 
            this.label150.Location = new System.Drawing.Point(342, 53);
            this.label150.Name = "label150";
            this.label150.Size = new System.Drawing.Size(90, 23);
            this.label150.TabIndex = 68;
            this.label150.Text = "Step Voltage (V)";
            // 
            // VCO155AmpStepTextBox
            // 
            this.VCO155AmpStepTextBox.Location = new System.Drawing.Point(112, 50);
            this.VCO155AmpStepTextBox.Name = "VCO155AmpStepTextBox";
            this.VCO155AmpStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO155AmpStepTextBox.TabIndex = 67;
            this.VCO155AmpStepTextBox.Text = "0.1";
            // 
            // label149
            // 
            this.label149.Location = new System.Drawing.Point(16, 53);
            this.label149.Name = "label149";
            this.label149.Size = new System.Drawing.Size(90, 23);
            this.label149.TabIndex = 67;
            this.label149.Text = "Step Voltage (V)";
            // 
            // label144
            // 
            this.label144.Location = new System.Drawing.Point(342, 85);
            this.label144.Name = "label144";
            this.label144.Size = new System.Drawing.Size(90, 23);
            this.label144.TabIndex = 56;
            this.label144.Text = "VCO Frequency";
            // 
            // label143
            // 
            this.label143.Location = new System.Drawing.Point(16, 85);
            this.label143.Name = "label143";
            this.label143.Size = new System.Drawing.Size(90, 23);
            this.label143.TabIndex = 56;
            this.label143.Text = "VCO Amplitude";
            // 
            // VCO155FreqTrackBar
            // 
            this.VCO155FreqTrackBar.Location = new System.Drawing.Point(345, 108);
            this.VCO155FreqTrackBar.Maximum = 1000;
            this.VCO155FreqTrackBar.Name = "VCO155FreqTrackBar";
            this.VCO155FreqTrackBar.Size = new System.Drawing.Size(287, 45);
            this.VCO155FreqTrackBar.TabIndex = 50;
            this.VCO155FreqTrackBar.Scroll += new System.EventHandler(this.VCO155FreqTrackBar_Scroll);
            // 
            // VCO155AmpTrackBar
            // 
            this.VCO155AmpTrackBar.Location = new System.Drawing.Point(6, 108);
            this.VCO155AmpTrackBar.Maximum = 1000;
            this.VCO155AmpTrackBar.Name = "VCO155AmpTrackBar";
            this.VCO155AmpTrackBar.Size = new System.Drawing.Size(287, 45);
            this.VCO155AmpTrackBar.TabIndex = 49;
            this.VCO155AmpTrackBar.Scroll += new System.EventHandler(this.VCO155AmpTrackBar_Scroll);
            // 
            // VCO155FreqVoltageTextBox
            // 
            this.VCO155FreqVoltageTextBox.Location = new System.Drawing.Point(438, 21);
            this.VCO155FreqVoltageTextBox.Name = "VCO155FreqVoltageTextBox";
            this.VCO155FreqVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO155FreqVoltageTextBox.TabIndex = 47;
            this.VCO155FreqVoltageTextBox.Text = "0";
            // 
            // label135
            // 
            this.label135.Location = new System.Drawing.Point(342, 23);
            this.label135.Name = "label135";
            this.label135.Size = new System.Drawing.Size(99, 23);
            this.label135.TabIndex = 46;
            this.label135.Text = "Freq Voltage (V)";
            // 
            // VCO155AmpVoltageTextBox
            // 
            this.VCO155AmpVoltageTextBox.Location = new System.Drawing.Point(112, 22);
            this.VCO155AmpVoltageTextBox.Name = "VCO155AmpVoltageTextBox";
            this.VCO155AmpVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO155AmpVoltageTextBox.TabIndex = 45;
            this.VCO155AmpVoltageTextBox.Text = "0";
            // 
            // VCO155UpdateButton
            // 
            this.VCO155UpdateButton.Location = new System.Drawing.Point(182, 50);
            this.VCO155UpdateButton.Name = "VCO155UpdateButton";
            this.VCO155UpdateButton.Size = new System.Drawing.Size(80, 23);
            this.VCO155UpdateButton.TabIndex = 40;
            this.VCO155UpdateButton.Text = "Update";
            this.VCO155UpdateButton.Click += new System.EventHandler(this.VCO155UpdateButton_Click);
            // 
            // label136
            // 
            this.label136.Location = new System.Drawing.Point(16, 24);
            this.label136.Name = "label136";
            this.label136.Size = new System.Drawing.Size(98, 23);
            this.label136.TabIndex = 36;
            this.label136.Text = "Amp Voltage (V)";
            // 
            // groupBox39
            // 
            this.groupBox39.Controls.Add(this.VCO30FreqStepMinusButton);
            this.groupBox39.Controls.Add(this.VCO30FreqStepPlusButton);
            this.groupBox39.Controls.Add(this.VCO30FreqStepTextBox);
            this.groupBox39.Controls.Add(this.label148);
            this.groupBox39.Controls.Add(this.VCO30AmpStepMinusButton);
            this.groupBox39.Controls.Add(this.VCO30AmpStepPlusButton);
            this.groupBox39.Controls.Add(this.VCO30AmpStepTextBox);
            this.groupBox39.Controls.Add(this.label147);
            this.groupBox39.Controls.Add(this.VCO30FreqTrackBar);
            this.groupBox39.Controls.Add(this.label142);
            this.groupBox39.Controls.Add(this.label141);
            this.groupBox39.Controls.Add(this.VCO30AmpTrackBar);
            this.groupBox39.Controls.Add(this.VCO30FreqVoltageTextBox);
            this.groupBox39.Controls.Add(this.label83);
            this.groupBox39.Controls.Add(this.VCO30AmpVoltageTextBox);
            this.groupBox39.Controls.Add(this.VCO30UpdateButton);
            this.groupBox39.Controls.Add(this.label134);
            this.groupBox39.Location = new System.Drawing.Point(17, 272);
            this.groupBox39.Name = "groupBox39";
            this.groupBox39.Size = new System.Drawing.Size(661, 149);
            this.groupBox39.TabIndex = 70;
            this.groupBox39.TabStop = false;
            this.groupBox39.Text = "30 MHz VCO";
            // 
            // VCO30FreqStepMinusButton
            // 
            this.VCO30FreqStepMinusButton.Location = new System.Drawing.Point(551, 22);
            this.VCO30FreqStepMinusButton.Name = "VCO30FreqStepMinusButton";
            this.VCO30FreqStepMinusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO30FreqStepMinusButton.TabIndex = 66;
            this.VCO30FreqStepMinusButton.Text = "-";
            this.VCO30FreqStepMinusButton.UseVisualStyleBackColor = true;
            this.VCO30FreqStepMinusButton.Click += new System.EventHandler(this.VCO30FreqStepMinusButton_Click);
            // 
            // VCO30FreqStepPlusButton
            // 
            this.VCO30FreqStepPlusButton.Location = new System.Drawing.Point(508, 22);
            this.VCO30FreqStepPlusButton.Name = "VCO30FreqStepPlusButton";
            this.VCO30FreqStepPlusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO30FreqStepPlusButton.TabIndex = 65;
            this.VCO30FreqStepPlusButton.Text = "+";
            this.VCO30FreqStepPlusButton.UseVisualStyleBackColor = true;
            this.VCO30FreqStepPlusButton.Click += new System.EventHandler(this.VCO30FreqStepPlusButton_Click);
            // 
            // VCO30FreqStepTextBox
            // 
            this.VCO30FreqStepTextBox.Location = new System.Drawing.Point(438, 50);
            this.VCO30FreqStepTextBox.Name = "VCO30FreqStepTextBox";
            this.VCO30FreqStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO30FreqStepTextBox.TabIndex = 64;
            this.VCO30FreqStepTextBox.Text = "0.1";
            // 
            // label148
            // 
            this.label148.Location = new System.Drawing.Point(342, 53);
            this.label148.Name = "label148";
            this.label148.Size = new System.Drawing.Size(90, 23);
            this.label148.TabIndex = 63;
            this.label148.Text = "Step Voltage (V)";
            // 
            // VCO30AmpStepMinusButton
            // 
            this.VCO30AmpStepMinusButton.Location = new System.Drawing.Point(225, 22);
            this.VCO30AmpStepMinusButton.Name = "VCO30AmpStepMinusButton";
            this.VCO30AmpStepMinusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO30AmpStepMinusButton.TabIndex = 61;
            this.VCO30AmpStepMinusButton.Text = "-";
            this.VCO30AmpStepMinusButton.UseVisualStyleBackColor = true;
            this.VCO30AmpStepMinusButton.Click += new System.EventHandler(this.VCO30AmpStepMinusButton_Click);
            // 
            // VCO30AmpStepPlusButton
            // 
            this.VCO30AmpStepPlusButton.Location = new System.Drawing.Point(182, 22);
            this.VCO30AmpStepPlusButton.Name = "VCO30AmpStepPlusButton";
            this.VCO30AmpStepPlusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO30AmpStepPlusButton.TabIndex = 62;
            this.VCO30AmpStepPlusButton.Text = "+";
            this.VCO30AmpStepPlusButton.UseVisualStyleBackColor = true;
            this.VCO30AmpStepPlusButton.Click += new System.EventHandler(this.VCO30AmpStepPlusButton_Click);
            // 
            // VCO30AmpStepTextBox
            // 
            this.VCO30AmpStepTextBox.Location = new System.Drawing.Point(112, 50);
            this.VCO30AmpStepTextBox.Name = "VCO30AmpStepTextBox";
            this.VCO30AmpStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO30AmpStepTextBox.TabIndex = 61;
            this.VCO30AmpStepTextBox.Text = "0.1";
            // 
            // label147
            // 
            this.label147.Location = new System.Drawing.Point(16, 53);
            this.label147.Name = "label147";
            this.label147.Size = new System.Drawing.Size(90, 23);
            this.label147.TabIndex = 61;
            this.label147.Text = "Step Voltage (V)";
            // 
            // VCO30FreqTrackBar
            // 
            this.VCO30FreqTrackBar.Location = new System.Drawing.Point(345, 99);
            this.VCO30FreqTrackBar.Maximum = 1000;
            this.VCO30FreqTrackBar.Name = "VCO30FreqTrackBar";
            this.VCO30FreqTrackBar.Size = new System.Drawing.Size(287, 45);
            this.VCO30FreqTrackBar.TabIndex = 55;
            this.VCO30FreqTrackBar.Scroll += new System.EventHandler(this.VCO30FreqTrackBar_Scroll);
            // 
            // label142
            // 
            this.label142.Location = new System.Drawing.Point(342, 79);
            this.label142.Name = "label142";
            this.label142.Size = new System.Drawing.Size(90, 23);
            this.label142.TabIndex = 54;
            this.label142.Text = "VCO Frequency";
            // 
            // label141
            // 
            this.label141.Location = new System.Drawing.Point(16, 79);
            this.label141.Name = "label141";
            this.label141.Size = new System.Drawing.Size(90, 23);
            this.label141.TabIndex = 53;
            this.label141.Text = "VCO Amplitude";
            // 
            // VCO30AmpTrackBar
            // 
            this.VCO30AmpTrackBar.Location = new System.Drawing.Point(6, 100);
            this.VCO30AmpTrackBar.Maximum = 1000;
            this.VCO30AmpTrackBar.Name = "VCO30AmpTrackBar";
            this.VCO30AmpTrackBar.Size = new System.Drawing.Size(287, 45);
            this.VCO30AmpTrackBar.TabIndex = 49;
            this.VCO30AmpTrackBar.Scroll += new System.EventHandler(this.VCO30AmpTrackBar_Scroll);
            // 
            // VCO30FreqVoltageTextBox
            // 
            this.VCO30FreqVoltageTextBox.Location = new System.Drawing.Point(438, 24);
            this.VCO30FreqVoltageTextBox.Name = "VCO30FreqVoltageTextBox";
            this.VCO30FreqVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO30FreqVoltageTextBox.TabIndex = 47;
            this.VCO30FreqVoltageTextBox.Text = "0";
            // 
            // label83
            // 
            this.label83.Location = new System.Drawing.Point(342, 24);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(99, 23);
            this.label83.TabIndex = 46;
            this.label83.Text = "Freq Voltage (V)";
            // 
            // VCO30AmpVoltageTextBox
            // 
            this.VCO30AmpVoltageTextBox.Location = new System.Drawing.Point(112, 24);
            this.VCO30AmpVoltageTextBox.Name = "VCO30AmpVoltageTextBox";
            this.VCO30AmpVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO30AmpVoltageTextBox.TabIndex = 45;
            this.VCO30AmpVoltageTextBox.Text = "0";
            // 
            // VCO30UpdateButton
            // 
            this.VCO30UpdateButton.Location = new System.Drawing.Point(182, 50);
            this.VCO30UpdateButton.Name = "VCO30UpdateButton";
            this.VCO30UpdateButton.Size = new System.Drawing.Size(80, 23);
            this.VCO30UpdateButton.TabIndex = 40;
            this.VCO30UpdateButton.Text = "Update";
            this.VCO30UpdateButton.Click += new System.EventHandler(this.VCO30UpdateButton_Click);
            // 
            // label134
            // 
            this.label134.Location = new System.Drawing.Point(16, 24);
            this.label134.Name = "label134";
            this.label134.Size = new System.Drawing.Size(98, 23);
            this.label134.TabIndex = 36;
            this.label134.Text = "Amp Voltage (V)";
            // 
            // groupBox161MHzVCO
            // 
            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqStepMinusButton);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqStepPlusButton);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqStepTextBox);
            this.groupBox161MHzVCO.Controls.Add(this.label146);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpStepMinusButton);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpStepTextBox);
            this.groupBox161MHzVCO.Controls.Add(this.label145);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpStepPlusButton);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqTrackBar);
            this.groupBox161MHzVCO.Controls.Add(this.label140);
            this.groupBox161MHzVCO.Controls.Add(this.label139);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpTrackBar);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqTextBox);
            this.groupBox161MHzVCO.Controls.Add(this.label137);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpVoltageTextBox);
            this.groupBox161MHzVCO.Controls.Add(this.VCO161UpdateButton);
            this.groupBox161MHzVCO.Controls.Add(this.label138);
            this.groupBox161MHzVCO.Location = new System.Drawing.Point(17, 127);
            this.groupBox161MHzVCO.Name = "groupBox161MHzVCO";
            this.groupBox161MHzVCO.Size = new System.Drawing.Size(661, 140);
            this.groupBox161MHzVCO.TabIndex = 69;
            this.groupBox161MHzVCO.TabStop = false;
            this.groupBox161MHzVCO.Text = "161 MHz VCO";
            this.groupBox161MHzVCO.Enter += new System.EventHandler(this.groupBox39_Enter);
            // 
            // VCO161FreqStepMinusButton
            // 
            this.VCO161FreqStepMinusButton.Location = new System.Drawing.Point(551, 19);
            this.VCO161FreqStepMinusButton.Name = "VCO161FreqStepMinusButton";
            this.VCO161FreqStepMinusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO161FreqStepMinusButton.TabIndex = 60;
            this.VCO161FreqStepMinusButton.Text = "-";
            this.VCO161FreqStepMinusButton.UseVisualStyleBackColor = true;
            this.VCO161FreqStepMinusButton.Click += new System.EventHandler(this.VCO161FreqStepMinusButton_Click);
            // 
            // VCO161FreqStepPlusButton
            // 
            this.VCO161FreqStepPlusButton.Location = new System.Drawing.Point(508, 19);
            this.VCO161FreqStepPlusButton.Name = "VCO161FreqStepPlusButton";
            this.VCO161FreqStepPlusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO161FreqStepPlusButton.TabIndex = 59;
            this.VCO161FreqStepPlusButton.Text = "+";
            this.VCO161FreqStepPlusButton.UseVisualStyleBackColor = true;
            this.VCO161FreqStepPlusButton.Click += new System.EventHandler(this.VCO161FreqStepPlusButton_Click);
            // 
            // VCO161FreqStepTextBox
            // 
            this.VCO161FreqStepTextBox.Location = new System.Drawing.Point(438, 48);
            this.VCO161FreqStepTextBox.Name = "VCO161FreqStepTextBox";
            this.VCO161FreqStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO161FreqStepTextBox.TabIndex = 58;
            this.VCO161FreqStepTextBox.Text = "0.1";
            // 
            // label146
            // 
            this.label146.Location = new System.Drawing.Point(342, 51);
            this.label146.Name = "label146";
            this.label146.Size = new System.Drawing.Size(90, 23);
            this.label146.TabIndex = 57;
            this.label146.Text = "Step Voltage (V)";
            // 
            // VCO161AmpStepMinusButton
            // 
            this.VCO161AmpStepMinusButton.Location = new System.Drawing.Point(225, 19);
            this.VCO161AmpStepMinusButton.Name = "VCO161AmpStepMinusButton";
            this.VCO161AmpStepMinusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO161AmpStepMinusButton.TabIndex = 56;
            this.VCO161AmpStepMinusButton.Text = "-";
            this.VCO161AmpStepMinusButton.UseVisualStyleBackColor = true;
            this.VCO161AmpStepMinusButton.Click += new System.EventHandler(this.VCO161StepMinusButton_Click);
            // 
            // VCO161AmpStepTextBox
            // 
            this.VCO161AmpStepTextBox.Location = new System.Drawing.Point(112, 48);
            this.VCO161AmpStepTextBox.Name = "VCO161AmpStepTextBox";
            this.VCO161AmpStepTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO161AmpStepTextBox.TabIndex = 55;
            this.VCO161AmpStepTextBox.Text = "0.1";
            // 
            // label145
            // 
            this.label145.Location = new System.Drawing.Point(16, 51);
            this.label145.Name = "label145";
            this.label145.Size = new System.Drawing.Size(90, 23);
            this.label145.TabIndex = 54;
            this.label145.Text = "Step Voltage (V)";
            // 
            // VCO161AmpStepPlusButton
            // 
            this.VCO161AmpStepPlusButton.Location = new System.Drawing.Point(182, 19);
            this.VCO161AmpStepPlusButton.Name = "VCO161AmpStepPlusButton";
            this.VCO161AmpStepPlusButton.Size = new System.Drawing.Size(37, 23);
            this.VCO161AmpStepPlusButton.TabIndex = 53;
            this.VCO161AmpStepPlusButton.Text = "+";
            this.VCO161AmpStepPlusButton.UseVisualStyleBackColor = true;
            this.VCO161AmpStepPlusButton.Click += new System.EventHandler(this.VCO161StepPlusButton_Click);
            // 
            // VCO161FreqTrackBar
            // 
            this.VCO161FreqTrackBar.Location = new System.Drawing.Point(345, 91);
            this.VCO161FreqTrackBar.Maximum = 1000;
            this.VCO161FreqTrackBar.Name = "VCO161FreqTrackBar";
            this.VCO161FreqTrackBar.Size = new System.Drawing.Size(287, 45);
            this.VCO161FreqTrackBar.TabIndex = 52;
            this.VCO161FreqTrackBar.Scroll += new System.EventHandler(this.VCO161FreqTrackBar_Scroll);
            // 
            // label140
            // 
            this.label140.Location = new System.Drawing.Point(342, 78);
            this.label140.Name = "label140";
            this.label140.Size = new System.Drawing.Size(90, 23);
            this.label140.TabIndex = 51;
            this.label140.Text = "VCO Frequency";
            // 
            // label139
            // 
            this.label139.Location = new System.Drawing.Point(16, 78);
            this.label139.Name = "label139";
            this.label139.Size = new System.Drawing.Size(90, 23);
            this.label139.TabIndex = 50;
            this.label139.Text = "VCO Amplitude";
            // 
            // VCO161AmpTrackBar
            // 
            this.VCO161AmpTrackBar.Location = new System.Drawing.Point(6, 92);
            this.VCO161AmpTrackBar.Maximum = 1000;
            this.VCO161AmpTrackBar.Name = "VCO161AmpTrackBar";
            this.VCO161AmpTrackBar.Size = new System.Drawing.Size(287, 45);
            this.VCO161AmpTrackBar.TabIndex = 49;
            this.VCO161AmpTrackBar.Scroll += new System.EventHandler(this.VCO161AmpTrackBar_Scroll);
            // 
            // VCO161FreqTextBox
            // 
            this.VCO161FreqTextBox.Location = new System.Drawing.Point(438, 21);
            this.VCO161FreqTextBox.Name = "VCO161FreqTextBox";
            this.VCO161FreqTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO161FreqTextBox.TabIndex = 47;
            this.VCO161FreqTextBox.Text = "0";
            // 
            // label137
            // 
            this.label137.Location = new System.Drawing.Point(342, 24);
            this.label137.Name = "label137";
            this.label137.Size = new System.Drawing.Size(90, 23);
            this.label137.TabIndex = 46;
            this.label137.Text = "Freq Voltage (V)";
            // 
            // VCO161AmpVoltageTextBox
            // 
            this.VCO161AmpVoltageTextBox.Location = new System.Drawing.Point(112, 21);
            this.VCO161AmpVoltageTextBox.Name = "VCO161AmpVoltageTextBox";
            this.VCO161AmpVoltageTextBox.Size = new System.Drawing.Size(64, 20);
            this.VCO161AmpVoltageTextBox.TabIndex = 45;
            this.VCO161AmpVoltageTextBox.Text = "0";
            // 
            // VCO161UpdateButton
            // 
            this.VCO161UpdateButton.Location = new System.Drawing.Point(182, 45);
            this.VCO161UpdateButton.Name = "VCO161UpdateButton";
            this.VCO161UpdateButton.Size = new System.Drawing.Size(80, 23);
            this.VCO161UpdateButton.TabIndex = 40;
            this.VCO161UpdateButton.Text = "Update";
            this.VCO161UpdateButton.Click += new System.EventHandler(this.VCO161UpdateButton_Click);
            // 
            // label138
            // 
            this.label138.Location = new System.Drawing.Point(16, 24);
            this.label138.Name = "label138";
            this.label138.Size = new System.Drawing.Size(90, 23);
            this.label138.TabIndex = 36;
            this.label138.Text = "Amp Voltage (V)";
            // 
            // tabPage6
            // 
            this.tabPage6.BackColor = System.Drawing.Color.Transparent;
            this.tabPage6.Controls.Add(this.groupBox34);
            this.tabPage6.Controls.Add(this.groupBox32);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(697, 575);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Polarizer";
            // 
            // groupBox34
            // 
            this.groupBox34.Controls.Add(this.label108);
            this.groupBox34.Controls.Add(this.label109);
            this.groupBox34.Controls.Add(this.pumpPolMesAngle);
            this.groupBox34.Controls.Add(this.updatePumpPolMesAngle);
            this.groupBox34.Controls.Add(this.zeroPumpPol);
            this.groupBox34.Controls.Add(this.label110);
            this.groupBox34.Controls.Add(this.groupBox35);
            this.groupBox34.Location = new System.Drawing.Point(349, 6);
            this.groupBox34.Name = "groupBox34";
            this.groupBox34.Size = new System.Drawing.Size(345, 229);
            this.groupBox34.TabIndex = 13;
            this.groupBox34.TabStop = false;
            this.groupBox34.Text = "Pump Polariser";
            // 
            // label108
            // 
            this.label108.AutoSize = true;
            this.label108.Location = new System.Drawing.Point(271, 30);
            this.label108.Name = "label108";
            this.label108.Size = new System.Drawing.Size(0, 13);
            this.label108.TabIndex = 48;
            // 
            // label109
            // 
            this.label109.AutoSize = true;
            this.label109.Location = new System.Drawing.Point(15, 35);
            this.label109.Name = "label109";
            this.label109.Size = new System.Drawing.Size(74, 13);
            this.label109.TabIndex = 47;
            this.label109.Text = "Position Mode";
            // 
            // pumpPolMesAngle
            // 
            this.pumpPolMesAngle.BackColor = System.Drawing.Color.Black;
            this.pumpPolMesAngle.ForeColor = System.Drawing.Color.Chartreuse;
            this.pumpPolMesAngle.Location = new System.Drawing.Point(111, 180);
            this.pumpPolMesAngle.Name = "pumpPolMesAngle";
            this.pumpPolMesAngle.ReadOnly = true;
            this.pumpPolMesAngle.Size = new System.Drawing.Size(82, 20);
            this.pumpPolMesAngle.TabIndex = 43;
            this.pumpPolMesAngle.Text = "0";
            // 
            // updatePumpPolMesAngle
            // 
            this.updatePumpPolMesAngle.Location = new System.Drawing.Point(199, 178);
            this.updatePumpPolMesAngle.Name = "updatePumpPolMesAngle";
            this.updatePumpPolMesAngle.Size = new System.Drawing.Size(75, 23);
            this.updatePumpPolMesAngle.TabIndex = 6;
            this.updatePumpPolMesAngle.Text = "Update";
            this.updatePumpPolMesAngle.UseVisualStyleBackColor = true;
            this.updatePumpPolMesAngle.Click += new System.EventHandler(this.updatePumpPolMesAngle_Click);
            // 
            // zeroPumpPol
            // 
            this.zeroPumpPol.Location = new System.Drawing.Point(280, 177);
            this.zeroPumpPol.Name = "zeroPumpPol";
            this.zeroPumpPol.Size = new System.Drawing.Size(44, 23);
            this.zeroPumpPol.TabIndex = 2;
            this.zeroPumpPol.Text = "Zero";
            this.zeroPumpPol.UseVisualStyleBackColor = true;
            this.zeroPumpPol.Click += new System.EventHandler(this.zeroPumpPol_Click);
            // 
            // label110
            // 
            this.label110.AutoSize = true;
            this.label110.Location = new System.Drawing.Point(12, 183);
            this.label110.Name = "label110";
            this.label110.Size = new System.Drawing.Size(84, 13);
            this.label110.TabIndex = 7;
            this.label110.Text = "Measured Angle";
            // 
            // groupBox35
            // 
            this.groupBox35.Controls.Add(this.label124);
            this.groupBox35.Controls.Add(this.pumpBacklashTextBox);
            this.groupBox35.Controls.Add(this.pumpPolVoltStopButton);
            this.groupBox35.Controls.Add(this.pumpPolVoltTrackBar);
            this.groupBox35.Controls.Add(this.label111);
            this.groupBox35.Controls.Add(this.label112);
            this.groupBox35.Controls.Add(this.pumpPolSetAngle);
            this.groupBox35.Controls.Add(this.label113);
            this.groupBox35.Controls.Add(this.label114);
            this.groupBox35.Controls.Add(this.setPumpPolAngle);
            this.groupBox35.Controls.Add(this.pumpPolModeSelectSwitch);
            this.groupBox35.Location = new System.Drawing.Point(6, 11);
            this.groupBox35.Name = "groupBox35";
            this.groupBox35.Size = new System.Drawing.Size(332, 153);
            this.groupBox35.TabIndex = 50;
            this.groupBox35.TabStop = false;
            // 
            // label124
            // 
            this.label124.AutoSize = true;
            this.label124.Location = new System.Drawing.Point(118, 55);
            this.label124.Name = "label124";
            this.label124.Size = new System.Drawing.Size(114, 13);
            this.label124.TabIndex = 54;
            this.label124.Text = "-ve overshoot ( 0 = off)";
            // 
            // pumpBacklashTextBox
            // 
            this.pumpBacklashTextBox.Location = new System.Drawing.Point(244, 52);
            this.pumpBacklashTextBox.Name = "pumpBacklashTextBox";
            this.pumpBacklashTextBox.Size = new System.Drawing.Size(75, 20);
            this.pumpBacklashTextBox.TabIndex = 53;
            this.pumpBacklashTextBox.Text = "0";
            // 
            // pumpPolVoltStopButton
            // 
            this.pumpPolVoltStopButton.Enabled = false;
            this.pumpPolVoltStopButton.Location = new System.Drawing.Point(243, 106);
            this.pumpPolVoltStopButton.Name = "pumpPolVoltStopButton";
            this.pumpPolVoltStopButton.Size = new System.Drawing.Size(75, 23);
            this.pumpPolVoltStopButton.TabIndex = 51;
            this.pumpPolVoltStopButton.Text = "Stop";
            this.pumpPolVoltStopButton.UseVisualStyleBackColor = true;
            this.pumpPolVoltStopButton.Click += new System.EventHandler(this.pumpPolVoltStopButton_Click);
            // 
            // pumpPolVoltTrackBar
            // 
            this.pumpPolVoltTrackBar.Enabled = false;
            this.pumpPolVoltTrackBar.Location = new System.Drawing.Point(88, 102);
            this.pumpPolVoltTrackBar.Maximum = 100;
            this.pumpPolVoltTrackBar.Minimum = -100;
            this.pumpPolVoltTrackBar.Name = "pumpPolVoltTrackBar";
            this.pumpPolVoltTrackBar.Size = new System.Drawing.Size(149, 45);
            this.pumpPolVoltTrackBar.TabIndex = 51;
            this.pumpPolVoltTrackBar.Scroll += new System.EventHandler(this.pumpPolVoltTrackBar_Scroll);
            // 
            // label111
            // 
            this.label111.AutoSize = true;
            this.label111.Location = new System.Drawing.Point(9, 126);
            this.label111.Name = "label111";
            this.label111.Size = new System.Drawing.Size(73, 13);
            this.label111.TabIndex = 49;
            this.label111.Text = "Voltage Mode";
            // 
            // label112
            // 
            this.label112.AutoSize = true;
            this.label112.Location = new System.Drawing.Point(102, 24);
            this.label112.Name = "label112";
            this.label112.Size = new System.Drawing.Size(53, 13);
            this.label112.TabIndex = 8;
            this.label112.Text = "Set Angle";
            // 
            // pumpPolSetAngle
            // 
            this.pumpPolSetAngle.Location = new System.Drawing.Point(161, 19);
            this.pumpPolSetAngle.Name = "pumpPolSetAngle";
            this.pumpPolSetAngle.Size = new System.Drawing.Size(66, 20);
            this.pumpPolSetAngle.TabIndex = 13;
            this.pumpPolSetAngle.Text = "0";
            // 
            // label113
            // 
            this.label113.AutoSize = true;
            this.label113.Location = new System.Drawing.Point(172, 78);
            this.label113.Name = "label113";
            this.label113.Size = new System.Drawing.Size(55, 13);
            this.label113.TabIndex = 44;
            this.label113.Text = "Clockwise";
            // 
            // label114
            // 
            this.label114.AutoSize = true;
            this.label114.Location = new System.Drawing.Point(85, 78);
            this.label114.Name = "label114";
            this.label114.Size = new System.Drawing.Size(75, 13);
            this.label114.TabIndex = 45;
            this.label114.Text = "Anti-clockwise";
            // 
            // setPumpPolAngle
            // 
            this.setPumpPolAngle.Location = new System.Drawing.Point(243, 17);
            this.setPumpPolAngle.Name = "setPumpPolAngle";
            this.setPumpPolAngle.Size = new System.Drawing.Size(75, 23);
            this.setPumpPolAngle.TabIndex = 5;
            this.setPumpPolAngle.Text = "Set";
            this.setPumpPolAngle.UseVisualStyleBackColor = true;
            this.setPumpPolAngle.Click += new System.EventHandler(this.setPumpPolAngle_Click);
            // 
            // pumpPolModeSelectSwitch
            // 
            this.pumpPolModeSelectSwitch.Location = new System.Drawing.Point(12, 33);
            this.pumpPolModeSelectSwitch.Name = "pumpPolModeSelectSwitch";
            this.pumpPolModeSelectSwitch.Size = new System.Drawing.Size(64, 96);
            this.pumpPolModeSelectSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.pumpPolModeSelectSwitch.TabIndex = 51;
            this.pumpPolModeSelectSwitch.Value = true;
            this.pumpPolModeSelectSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.pumpPolModeSelectSwitch_StateChanged);
            // 
            // groupBox32
            // 
            this.groupBox32.Controls.Add(this.label106);
            this.groupBox32.Controls.Add(this.label105);
            this.groupBox32.Controls.Add(this.probePolMesAngle);
            this.groupBox32.Controls.Add(this.updateProbePolMesAngle);
            this.groupBox32.Controls.Add(this.zeroProbePol);
            this.groupBox32.Controls.Add(this.label101);
            this.groupBox32.Controls.Add(this.groupBox33);
            this.groupBox32.Location = new System.Drawing.Point(3, 6);
            this.groupBox32.Name = "groupBox32";
            this.groupBox32.Size = new System.Drawing.Size(345, 229);
            this.groupBox32.TabIndex = 12;
            this.groupBox32.TabStop = false;
            this.groupBox32.Text = "Probe Polariser";
            // 
            // label106
            // 
            this.label106.AutoSize = true;
            this.label106.Location = new System.Drawing.Point(271, 30);
            this.label106.Name = "label106";
            this.label106.Size = new System.Drawing.Size(0, 13);
            this.label106.TabIndex = 48;
            // 
            // label105
            // 
            this.label105.AutoSize = true;
            this.label105.Location = new System.Drawing.Point(15, 35);
            this.label105.Name = "label105";
            this.label105.Size = new System.Drawing.Size(74, 13);
            this.label105.TabIndex = 47;
            this.label105.Text = "Position Mode";
            // 
            // probePolMesAngle
            // 
            this.probePolMesAngle.BackColor = System.Drawing.Color.Black;
            this.probePolMesAngle.ForeColor = System.Drawing.Color.Chartreuse;
            this.probePolMesAngle.Location = new System.Drawing.Point(111, 180);
            this.probePolMesAngle.Name = "probePolMesAngle";
            this.probePolMesAngle.ReadOnly = true;
            this.probePolMesAngle.Size = new System.Drawing.Size(82, 20);
            this.probePolMesAngle.TabIndex = 43;
            this.probePolMesAngle.Text = "0";
            // 
            // updateProbePolMesAngle
            // 
            this.updateProbePolMesAngle.Location = new System.Drawing.Point(199, 178);
            this.updateProbePolMesAngle.Name = "updateProbePolMesAngle";
            this.updateProbePolMesAngle.Size = new System.Drawing.Size(75, 23);
            this.updateProbePolMesAngle.TabIndex = 6;
            this.updateProbePolMesAngle.Text = "Update";
            this.updateProbePolMesAngle.UseVisualStyleBackColor = true;
            this.updateProbePolMesAngle.Click += new System.EventHandler(this.updateProbePolMesAngle_Click);
            // 
            // zeroProbePol
            // 
            this.zeroProbePol.Location = new System.Drawing.Point(280, 177);
            this.zeroProbePol.Name = "zeroProbePol";
            this.zeroProbePol.Size = new System.Drawing.Size(44, 23);
            this.zeroProbePol.TabIndex = 2;
            this.zeroProbePol.Text = "Zero";
            this.zeroProbePol.UseVisualStyleBackColor = true;
            this.zeroProbePol.Click += new System.EventHandler(this.zeroProbePol_Click);
            // 
            // label101
            // 
            this.label101.AutoSize = true;
            this.label101.Location = new System.Drawing.Point(12, 183);
            this.label101.Name = "label101";
            this.label101.Size = new System.Drawing.Size(84, 13);
            this.label101.TabIndex = 7;
            this.label101.Text = "Measured Angle";
            // 
            // groupBox33
            // 
            this.groupBox33.Controls.Add(this.label123);
            this.groupBox33.Controls.Add(this.probeBacklashTextBox);
            this.groupBox33.Controls.Add(this.probePolVoltStopButton);
            this.groupBox33.Controls.Add(this.probePolVoltTrackBar);
            this.groupBox33.Controls.Add(this.label107);
            this.groupBox33.Controls.Add(this.label102);
            this.groupBox33.Controls.Add(this.probePolSetAngle);
            this.groupBox33.Controls.Add(this.label103);
            this.groupBox33.Controls.Add(this.label104);
            this.groupBox33.Controls.Add(this.setProbePolAngle);
            this.groupBox33.Controls.Add(this.probePolModeSelectSwitch);
            this.groupBox33.Location = new System.Drawing.Point(6, 11);
            this.groupBox33.Name = "groupBox33";
            this.groupBox33.Size = new System.Drawing.Size(332, 153);
            this.groupBox33.TabIndex = 50;
            this.groupBox33.TabStop = false;
            // 
            // label123
            // 
            this.label123.AutoSize = true;
            this.label123.Location = new System.Drawing.Point(117, 55);
            this.label123.Name = "label123";
            this.label123.Size = new System.Drawing.Size(114, 13);
            this.label123.TabIndex = 52;
            this.label123.Text = "-ve overshoot ( 0 = off)";
            // 
            // probeBacklashTextBox
            // 
            this.probeBacklashTextBox.Location = new System.Drawing.Point(243, 52);
            this.probeBacklashTextBox.Name = "probeBacklashTextBox";
            this.probeBacklashTextBox.Size = new System.Drawing.Size(75, 20);
            this.probeBacklashTextBox.TabIndex = 14;
            this.probeBacklashTextBox.Text = "0";
            // 
            // probePolVoltStopButton
            // 
            this.probePolVoltStopButton.Enabled = false;
            this.probePolVoltStopButton.Location = new System.Drawing.Point(243, 106);
            this.probePolVoltStopButton.Name = "probePolVoltStopButton";
            this.probePolVoltStopButton.Size = new System.Drawing.Size(75, 23);
            this.probePolVoltStopButton.TabIndex = 51;
            this.probePolVoltStopButton.Text = "Stop";
            this.probePolVoltStopButton.UseVisualStyleBackColor = true;
            this.probePolVoltStopButton.Click += new System.EventHandler(this.probePolVoltStopButton_Click);
            // 
            // probePolVoltTrackBar
            // 
            this.probePolVoltTrackBar.Enabled = false;
            this.probePolVoltTrackBar.Location = new System.Drawing.Point(88, 102);
            this.probePolVoltTrackBar.Maximum = 100;
            this.probePolVoltTrackBar.Minimum = -100;
            this.probePolVoltTrackBar.Name = "probePolVoltTrackBar";
            this.probePolVoltTrackBar.Size = new System.Drawing.Size(149, 45);
            this.probePolVoltTrackBar.TabIndex = 51;
            this.probePolVoltTrackBar.Scroll += new System.EventHandler(this.probePolVoltTrackBar_Scroll);
            // 
            // label107
            // 
            this.label107.AutoSize = true;
            this.label107.Location = new System.Drawing.Point(9, 126);
            this.label107.Name = "label107";
            this.label107.Size = new System.Drawing.Size(73, 13);
            this.label107.TabIndex = 49;
            this.label107.Text = "Voltage Mode";
            // 
            // label102
            // 
            this.label102.AutoSize = true;
            this.label102.Location = new System.Drawing.Point(102, 24);
            this.label102.Name = "label102";
            this.label102.Size = new System.Drawing.Size(53, 13);
            this.label102.TabIndex = 8;
            this.label102.Text = "Set Angle";
            // 
            // probePolSetAngle
            // 
            this.probePolSetAngle.Location = new System.Drawing.Point(161, 19);
            this.probePolSetAngle.Name = "probePolSetAngle";
            this.probePolSetAngle.Size = new System.Drawing.Size(66, 20);
            this.probePolSetAngle.TabIndex = 13;
            this.probePolSetAngle.Text = "0";
            // 
            // label103
            // 
            this.label103.AutoSize = true;
            this.label103.Location = new System.Drawing.Point(172, 78);
            this.label103.Name = "label103";
            this.label103.Size = new System.Drawing.Size(55, 13);
            this.label103.TabIndex = 44;
            this.label103.Text = "Clockwise";
            // 
            // label104
            // 
            this.label104.AutoSize = true;
            this.label104.Location = new System.Drawing.Point(85, 78);
            this.label104.Name = "label104";
            this.label104.Size = new System.Drawing.Size(75, 13);
            this.label104.TabIndex = 45;
            this.label104.Text = "Anti-clockwise";
            // 
            // setProbePolAngle
            // 
            this.setProbePolAngle.Location = new System.Drawing.Point(243, 17);
            this.setProbePolAngle.Name = "setProbePolAngle";
            this.setProbePolAngle.Size = new System.Drawing.Size(75, 23);
            this.setProbePolAngle.TabIndex = 5;
            this.setProbePolAngle.Text = "Set";
            this.setProbePolAngle.UseVisualStyleBackColor = true;
            this.setProbePolAngle.Click += new System.EventHandler(this.setProbePolAngle_Click);
            // 
            // probePolModeSelectSwitch
            // 
            this.probePolModeSelectSwitch.Location = new System.Drawing.Point(12, 33);
            this.probePolModeSelectSwitch.Name = "probePolModeSelectSwitch";
            this.probePolModeSelectSwitch.Size = new System.Drawing.Size(64, 96);
            this.probePolModeSelectSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.probePolModeSelectSwitch.TabIndex = 51;
            this.probePolModeSelectSwitch.Value = true;
            this.probePolModeSelectSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.probePolModeSelectSwitch_StateChanged_1);
            // 
            // tabPage5
            // 
            this.tabPage5.BackColor = System.Drawing.Color.Transparent;
            this.tabPage5.Controls.Add(this.groupBox17);
            this.tabPage5.Controls.Add(this.groupBox15);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(697, 575);
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
            this.yagFlashlampVTextBox.TextChanged += new System.EventHandler(this.yagFlashlampVTextBox_TextChanged);
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
            // tabPage9
            // 
            this.tabPage9.BackColor = System.Drawing.Color.Transparent;
            this.tabPage9.Controls.Add(this.pressureMonitorGroupBox);
            this.tabPage9.Controls.Add(this.switchScanTTLSwitch);
            this.tabPage9.Controls.Add(this.label97);
            this.tabPage9.Location = new System.Drawing.Point(4, 22);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage9.Size = new System.Drawing.Size(697, 575);
            this.tabPage9.TabIndex = 8;
            this.tabPage9.Text = "Misc";
            // 
            // switchScanTTLSwitch
            // 
            this.switchScanTTLSwitch.Location = new System.Drawing.Point(6, 6);
            this.switchScanTTLSwitch.Name = "switchScanTTLSwitch";
            this.switchScanTTLSwitch.Size = new System.Drawing.Size(64, 96);
            this.switchScanTTLSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.switchScanTTLSwitch.TabIndex = 2;
            this.switchScanTTLSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.switch1_StateChanged);
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.Location = new System.Drawing.Point(76, 53);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(90, 13);
            this.label97.TabIndex = 1;
            this.label97.Text = "Switch Scan TTL";
            // 
            // tabPage7
            // 
            this.tabPage7.BackColor = System.Drawing.Color.Transparent;
            this.tabPage7.Controls.Add(this.clearAlertButton);
            this.tabPage7.Controls.Add(this.alertTextBox);
            this.tabPage7.ImageKey = "(none)";
            this.tabPage7.Location = new System.Drawing.Point(4, 22);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(697, 575);
            this.tabPage7.TabIndex = 6;
            this.tabPage7.Text = "Alerts";
            // 
            // clearAlertButton
            // 
            this.clearAlertButton.Location = new System.Drawing.Point(18, 540);
            this.clearAlertButton.Name = "clearAlertButton";
            this.clearAlertButton.Size = new System.Drawing.Size(140, 23);
            this.clearAlertButton.TabIndex = 1;
            this.clearAlertButton.Text = "Clear alert status";
            this.clearAlertButton.UseVisualStyleBackColor = true;
            this.clearAlertButton.Click += new System.EventHandler(this.clearAlertButton_Click);
            // 
            // alertTextBox
            // 
            this.alertTextBox.Location = new System.Drawing.Point(18, 22);
            this.alertTextBox.Multiline = true;
            this.alertTextBox.Name = "alertTextBox";
            this.alertTextBox.Size = new System.Drawing.Size(654, 512);
            this.alertTextBox.TabIndex = 0;
            // 
            // tabPage10
            // 
            this.tabPage10.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage10.Controls.Add(this.groupBox19);
            this.tabPage10.Controls.Add(this.groupBox37);
            this.tabPage10.Location = new System.Drawing.Point(4, 22);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Size = new System.Drawing.Size(697, 575);
            this.tabPage10.TabIndex = 9;
            this.tabPage10.Text = "I2 lock code";
            // 
            // groupBox19
            // 
            this.groupBox19.Controls.Add(this.UpdateI2BiasVoltage);
            this.groupBox19.Controls.Add(this.I2BiasVoltageTextBox);
            this.groupBox19.Controls.Add(this.I2BiasVoltageTrackBar);
            this.groupBox19.Location = new System.Drawing.Point(118, 234);
            this.groupBox19.Name = "groupBox19";
            this.groupBox19.Size = new System.Drawing.Size(576, 74);
            this.groupBox19.TabIndex = 68;
            this.groupBox19.TabStop = false;
            this.groupBox19.Text = "Lock Bias";
            // 
            // UpdateI2BiasVoltage
            // 
            this.UpdateI2BiasVoltage.Location = new System.Drawing.Point(488, 45);
            this.UpdateI2BiasVoltage.Name = "UpdateI2BiasVoltage";
            this.UpdateI2BiasVoltage.Size = new System.Drawing.Size(75, 23);
            this.UpdateI2BiasVoltage.TabIndex = 53;
            this.UpdateI2BiasVoltage.Text = "Update";
            this.UpdateI2BiasVoltage.Click += new System.EventHandler(this.UpdateI2BiasVoltage_Click);
            // 
            // I2BiasVoltageTextBox
            // 
            this.I2BiasVoltageTextBox.Location = new System.Drawing.Point(488, 18);
            this.I2BiasVoltageTextBox.Name = "I2BiasVoltageTextBox";
            this.I2BiasVoltageTextBox.Size = new System.Drawing.Size(75, 20);
            this.I2BiasVoltageTextBox.TabIndex = 52;
            this.I2BiasVoltageTextBox.Text = "0";
            // 
            // I2BiasVoltageTrackBar
            // 
            this.I2BiasVoltageTrackBar.Location = new System.Drawing.Point(30, 18);
            this.I2BiasVoltageTrackBar.Maximum = 5000;
            this.I2BiasVoltageTrackBar.Minimum = -5000;
            this.I2BiasVoltageTrackBar.Name = "I2BiasVoltageTrackBar";
            this.I2BiasVoltageTrackBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.I2BiasVoltageTrackBar.Size = new System.Drawing.Size(441, 45);
            this.I2BiasVoltageTrackBar.TabIndex = 51;
            this.I2BiasVoltageTrackBar.Scroll += new System.EventHandler(this.I2BiasVoltageTrackBar_Scroll);
            // 
            // groupBox37
            // 
            this.groupBox37.Controls.Add(this.I2ErrorPollPeriodTextBox);
            this.groupBox37.Controls.Add(this.updateI2ErrorSigButton);
            this.groupBox37.Controls.Add(this.I2ErrorSigTextBox);
            this.groupBox37.Controls.Add(this.I2ErrorSigGraph);
            this.groupBox37.Controls.Add(this.stopI2ErrorSigPollButton);
            this.groupBox37.Controls.Add(this.label80);
            this.groupBox37.Controls.Add(this.startI2ErrorSigPollButton);
            this.groupBox37.Location = new System.Drawing.Point(3, 3);
            this.groupBox37.Name = "groupBox37";
            this.groupBox37.Size = new System.Drawing.Size(691, 224);
            this.groupBox37.TabIndex = 48;
            this.groupBox37.TabStop = false;
            this.groupBox37.Text = "Error Signal";
            // 
            // I2ErrorPollPeriodTextBox
            // 
            this.I2ErrorPollPeriodTextBox.Location = new System.Drawing.Point(441, 193);
            this.I2ErrorPollPeriodTextBox.Name = "I2ErrorPollPeriodTextBox";
            this.I2ErrorPollPeriodTextBox.Size = new System.Drawing.Size(64, 20);
            this.I2ErrorPollPeriodTextBox.TabIndex = 64;
            this.I2ErrorPollPeriodTextBox.Text = "50";
            // 
            // updateI2ErrorSigButton
            // 
            this.updateI2ErrorSigButton.Location = new System.Drawing.Point(166, 191);
            this.updateI2ErrorSigButton.Name = "updateI2ErrorSigButton";
            this.updateI2ErrorSigButton.Size = new System.Drawing.Size(72, 23);
            this.updateI2ErrorSigButton.TabIndex = 68;
            this.updateI2ErrorSigButton.Text = "Update";
            this.updateI2ErrorSigButton.UseVisualStyleBackColor = true;
            this.updateI2ErrorSigButton.Click += new System.EventHandler(this.updateI2ErrorSigButton_Click);
            // 
            // I2ErrorSigTextBox
            // 
            this.I2ErrorSigTextBox.BackColor = System.Drawing.Color.Black;
            this.I2ErrorSigTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.I2ErrorSigTextBox.Location = new System.Drawing.Point(23, 193);
            this.I2ErrorSigTextBox.Name = "I2ErrorSigTextBox";
            this.I2ErrorSigTextBox.ReadOnly = true;
            this.I2ErrorSigTextBox.Size = new System.Drawing.Size(137, 20);
            this.I2ErrorSigTextBox.TabIndex = 67;
            this.I2ErrorSigTextBox.Text = "0";
            // 
            // I2ErrorSigGraph
            // 
            this.I2ErrorSigGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.I2ErrorSigGraph.Location = new System.Drawing.Point(7, 19);
            this.I2ErrorSigGraph.Name = "I2ErrorSigGraph";
            this.I2ErrorSigGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.I2ErrorSigPlot});
            this.I2ErrorSigGraph.Size = new System.Drawing.Size(678, 166);
            this.I2ErrorSigGraph.TabIndex = 49;
            this.I2ErrorSigGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.I2ErrorSigGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            // 
            // I2ErrorSigPlot
            // 
            this.I2ErrorSigPlot.AntiAliased = true;
            this.I2ErrorSigPlot.HistoryCapacity = 10000;
            this.I2ErrorSigPlot.LineWidth = 2F;
            this.I2ErrorSigPlot.XAxis = this.xAxis3;
            this.I2ErrorSigPlot.YAxis = this.yAxis3;
            // 
            // xAxis3
            // 
            this.xAxis3.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis3.Range = new NationalInstruments.UI.Range(0D, 500D);
            // 
            // yAxis3
            // 
            this.yAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis3.OriginLineVisible = true;
            this.yAxis3.Range = new NationalInstruments.UI.Range(-0.4D, 0.4D);
            // 
            // stopI2ErrorSigPollButton
            // 
            this.stopI2ErrorSigPollButton.Enabled = false;
            this.stopI2ErrorSigPollButton.Location = new System.Drawing.Point(592, 191);
            this.stopI2ErrorSigPollButton.Name = "stopI2ErrorSigPollButton";
            this.stopI2ErrorSigPollButton.Size = new System.Drawing.Size(75, 23);
            this.stopI2ErrorSigPollButton.TabIndex = 66;
            this.stopI2ErrorSigPollButton.Text = "Stop poll";
            this.stopI2ErrorSigPollButton.UseVisualStyleBackColor = true;
            this.stopI2ErrorSigPollButton.Click += new System.EventHandler(this.stopI2ErrorSigPollButton_Click);
            // 
            // label80
            // 
            this.label80.Location = new System.Drawing.Point(354, 196);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(101, 23);
            this.label80.TabIndex = 63;
            this.label80.Text = "Poll period (ms)";
            // 
            // startI2ErrorSigPollButton
            // 
            this.startI2ErrorSigPollButton.Location = new System.Drawing.Point(511, 191);
            this.startI2ErrorSigPollButton.Name = "startI2ErrorSigPollButton";
            this.startI2ErrorSigPollButton.Size = new System.Drawing.Size(75, 23);
            this.startI2ErrorSigPollButton.TabIndex = 65;
            this.startI2ErrorSigPollButton.Text = "Start poll";
            this.startI2ErrorSigPollButton.UseVisualStyleBackColor = true;
            this.startI2ErrorSigPollButton.Click += new System.EventHandler(this.startI2ErrorSigPollButton_Click);
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
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(724, 24);
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
            // checkBox1
            // 
            this.checkBox1.Location = new System.Drawing.Point(6, 23);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(167, 24);
            this.checkBox1.TabIndex = 53;
            this.checkBox1.Text = "State (Checked is 0=>N+)";
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Checked = true;
            this.radioButton4.Location = new System.Drawing.Point(77, 7);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(31, 17);
            this.radioButton4.TabIndex = 32;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "0";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(3, 6);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(31, 17);
            this.radioButton5.TabIndex = 32;
            this.radioButton5.Text = "+";
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(42, 7);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(28, 17);
            this.radioButton6.TabIndex = 32;
            this.radioButton6.Text = "-";
            this.radioButton6.UseVisualStyleBackColor = true;
            // 
            // pressureMonitorGroupBox
            // 
            this.pressureMonitorGroupBox.Controls.Add(this.label121);
            this.pressureMonitorGroupBox.Controls.Add(this.pressureMonitorLogPeriodTextBox);
            this.pressureMonitorGroupBox.Controls.Add(this.logPressureDataCheckBox);
            this.pressureMonitorGroupBox.Controls.Add(this.clearPressureMonitorButton);
            this.pressureMonitorGroupBox.Controls.Add(this.label153);
            this.pressureMonitorGroupBox.Controls.Add(this.pressureMonitorSampleLengthTextBox);
            this.pressureMonitorGroupBox.Controls.Add(this.label157);
            this.pressureMonitorGroupBox.Controls.Add(this.stopPressureMonitorPollButton);
            this.pressureMonitorGroupBox.Controls.Add(this.legend2);
            this.pressureMonitorGroupBox.Controls.Add(this.label161);
            this.pressureMonitorGroupBox.Controls.Add(this.pressureMonitorPollPeriodTextBox);
            this.pressureMonitorGroupBox.Controls.Add(this.startPressureMonitorPollButton);
            this.pressureMonitorGroupBox.Controls.Add(this.pressureGraph);
            this.pressureMonitorGroupBox.Controls.Add(this.pressureMonitorTextBox);
            this.pressureMonitorGroupBox.Controls.Add(this.updatePressureMonitorButton);
            this.pressureMonitorGroupBox.Controls.Add(this.label165);
            this.pressureMonitorGroupBox.Location = new System.Drawing.Point(18, 150);
            this.pressureMonitorGroupBox.Name = "pressureMonitorGroupBox";
            this.pressureMonitorGroupBox.Size = new System.Drawing.Size(660, 274);
            this.pressureMonitorGroupBox.TabIndex = 45;
            this.pressureMonitorGroupBox.TabStop = false;
            this.pressureMonitorGroupBox.Text = "Pressure monitor";
            // 
            // logPressureDataCheckBox
            // 
            this.logPressureDataCheckBox.AutoSize = true;
            this.logPressureDataCheckBox.Location = new System.Drawing.Point(580, 86);
            this.logPressureDataCheckBox.Name = "logPressureDataCheckBox";
            this.logPressureDataCheckBox.Size = new System.Drawing.Size(68, 17);
            this.logPressureDataCheckBox.TabIndex = 75;
            this.logPressureDataCheckBox.Text = "Log data";
            this.logPressureDataCheckBox.UseVisualStyleBackColor = true;
            this.logPressureDataCheckBox.CheckedChanged += new System.EventHandler(this.logPressureDataCheckBox_CheckedChanged);
            // 
            // clearPressureMonitorButton
            // 
            this.clearPressureMonitorButton.Location = new System.Drawing.Point(68, 66);
            this.clearPressureMonitorButton.Name = "clearPressureMonitorButton";
            this.clearPressureMonitorButton.Size = new System.Drawing.Size(39, 23);
            this.clearPressureMonitorButton.TabIndex = 74;
            this.clearPressureMonitorButton.Text = "Clear";
            this.clearPressureMonitorButton.UseVisualStyleBackColor = true;
            this.clearPressureMonitorButton.Click += new System.EventHandler(this.clearPressureMonitorButton_Click);
            // 
            // label153
            // 
            this.label153.Location = new System.Drawing.Point(211, 22);
            this.label153.Name = "label153";
            this.label153.Size = new System.Drawing.Size(47, 23);
            this.label153.TabIndex = 69;
            this.label153.Text = "samples";
            // 
            // pressureMonitorSampleLengthTextBox
            // 
            this.pressureMonitorSampleLengthTextBox.Location = new System.Drawing.Point(170, 19);
            this.pressureMonitorSampleLengthTextBox.Name = "pressureMonitorSampleLengthTextBox";
            this.pressureMonitorSampleLengthTextBox.Size = new System.Drawing.Size(36, 20);
            this.pressureMonitorSampleLengthTextBox.TabIndex = 67;
            this.pressureMonitorSampleLengthTextBox.Text = "20";
            // 
            // label157
            // 
            this.label157.Location = new System.Drawing.Point(122, 22);
            this.label157.Name = "label157";
            this.label157.Size = new System.Drawing.Size(47, 23);
            this.label157.TabIndex = 68;
            this.label157.Text = "Average";
            // 
            // stopPressureMonitorPollButton
            // 
            this.stopPressureMonitorPollButton.Enabled = false;
            this.stopPressureMonitorPollButton.Location = new System.Drawing.Point(211, 76);
            this.stopPressureMonitorPollButton.Name = "stopPressureMonitorPollButton";
            this.stopPressureMonitorPollButton.Size = new System.Drawing.Size(75, 23);
            this.stopPressureMonitorPollButton.TabIndex = 55;
            this.stopPressureMonitorPollButton.Text = "Stop poll";
            this.stopPressureMonitorPollButton.UseVisualStyleBackColor = true;
            this.stopPressureMonitorPollButton.Click += new System.EventHandler(this.stopPressureMonitorPollButton_Click);
            // 
            // legend2
            // 
            this.legend2.Items.AddRange(new NationalInstruments.UI.LegendItem[] {
            this.pressurePlotLegend});
            this.legend2.ItemSize = new System.Drawing.Size(12, 12);
            this.legend2.Location = new System.Drawing.Point(440, 83);
            this.legend2.Name = "legend2";
            this.legend2.Size = new System.Drawing.Size(144, 22);
            this.legend2.TabIndex = 59;
            // 
            // pressurePlotLegend
            // 
            this.pressurePlotLegend.Source = this.pressurePlot;
            this.pressurePlotLegend.Text = "Middle Penning gauge";
            // 
            // pressurePlot
            // 
            this.pressurePlot.AntiAliased = true;
            this.pressurePlot.HistoryCapacity = 10000;
            this.pressurePlot.LineColor = System.Drawing.Color.Crimson;
            this.pressurePlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.pressurePlot.LineWidth = 2F;
            this.pressurePlot.XAxis = this.xAxis2;
            this.pressurePlot.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.Mode = NationalInstruments.UI.AxisMode.StripChart;
            this.xAxis2.Range = new NationalInstruments.UI.Range(0D, 500D);
            // 
            // yAxis2
            // 
            this.yAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis2.OriginLineVisible = true;
            this.yAxis2.Range = new NationalInstruments.UI.Range(5E-07D, 1E-05D);
            // 
            // label161
            // 
            this.label161.Location = new System.Drawing.Point(123, 50);
            this.label161.Name = "label161";
            this.label161.Size = new System.Drawing.Size(81, 23);
            this.label161.TabIndex = 56;
            this.label161.Text = "Poll period (ms)";
            // 
            // pressureMonitorPollPeriodTextBox
            // 
            this.pressureMonitorPollPeriodTextBox.Location = new System.Drawing.Point(210, 47);
            this.pressureMonitorPollPeriodTextBox.Name = "pressureMonitorPollPeriodTextBox";
            this.pressureMonitorPollPeriodTextBox.Size = new System.Drawing.Size(64, 20);
            this.pressureMonitorPollPeriodTextBox.TabIndex = 0;
            this.pressureMonitorPollPeriodTextBox.Text = "100";
            // 
            // startPressureMonitorPollButton
            // 
            this.startPressureMonitorPollButton.Location = new System.Drawing.Point(125, 76);
            this.startPressureMonitorPollButton.Name = "startPressureMonitorPollButton";
            this.startPressureMonitorPollButton.Size = new System.Drawing.Size(75, 23);
            this.startPressureMonitorPollButton.TabIndex = 53;
            this.startPressureMonitorPollButton.Text = "Start poll";
            this.startPressureMonitorPollButton.UseVisualStyleBackColor = true;
            this.startPressureMonitorPollButton.Click += new System.EventHandler(this.startPressureMonitorPollButton_Click);
            // 
            // pressureGraph
            // 
            this.pressureGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.pressureGraph.Location = new System.Drawing.Point(9, 114);
            this.pressureGraph.Name = "pressureGraph";
            this.pressureGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.pressurePlot});
            this.pressureGraph.Size = new System.Drawing.Size(645, 153);
            this.pressureGraph.TabIndex = 45;
            this.pressureGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.pressureGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // pressureMonitorTextBox
            // 
            this.pressureMonitorTextBox.BackColor = System.Drawing.Color.Black;
            this.pressureMonitorTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.pressureMonitorTextBox.Location = new System.Drawing.Point(10, 39);
            this.pressureMonitorTextBox.Name = "pressureMonitorTextBox";
            this.pressureMonitorTextBox.ReadOnly = true;
            this.pressureMonitorTextBox.Size = new System.Drawing.Size(89, 20);
            this.pressureMonitorTextBox.TabIndex = 42;
            this.pressureMonitorTextBox.Text = "0";
            // 
            // updatePressureMonitorButton
            // 
            this.updatePressureMonitorButton.Location = new System.Drawing.Point(9, 66);
            this.updatePressureMonitorButton.Name = "updatePressureMonitorButton";
            this.updatePressureMonitorButton.Size = new System.Drawing.Size(53, 23);
            this.updatePressureMonitorButton.TabIndex = 40;
            this.updatePressureMonitorButton.Text = "Update";
            this.updatePressureMonitorButton.Click += new System.EventHandler(this.updatePressureMonitorButton_Click);
            // 
            // label165
            // 
            this.label165.Location = new System.Drawing.Point(10, 21);
            this.label165.Name = "label165";
            this.label165.Size = new System.Drawing.Size(89, 23);
            this.label165.TabIndex = 36;
            this.label165.Text = "Pressure (mbar)";
            // 
            // label121
            // 
            this.label121.Location = new System.Drawing.Point(440, 21);
            this.label121.Name = "label121";
            this.label121.Size = new System.Drawing.Size(129, 23);
            this.label121.TabIndex = 77;
            this.label121.Text = "Log + display period (min)";
            // 
            // pressureMonitorLogPeriodTextBox
            // 
            this.pressureMonitorLogPeriodTextBox.Location = new System.Drawing.Point(575, 18);
            this.pressureMonitorLogPeriodTextBox.Name = "pressureMonitorLogPeriodTextBox";
            this.pressureMonitorLogPeriodTextBox.Size = new System.Drawing.Size(64, 20);
            this.pressureMonitorLogPeriodTextBox.TabIndex = 76;
            this.pressureMonitorLogPeriodTextBox.Text = "15";
            // 
            // ControlWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(724, 626);
            this.Controls.Add(this.tabControl);
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
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox21.ResumeLayout(false);
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.legend1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leakageGraph)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.groupBox24.ResumeLayout(false);
            this.groupBox24.PerformLayout();
            this.groupBox22.ResumeLayout(false);
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
            this.groupBox23.ResumeLayout(false);
            this.groupBox20.ResumeLayout(false);
            this.groupBox20.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.tabPage11.ResumeLayout(false);
            this.groupBox25.ResumeLayout(false);
            this.groupBox25.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pumpAOMTrackBar)).EndInit();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox18.ResumeLayout(false);
            this.groupBox18.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.probeAOMtrackBar)).EndInit();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tabPage13.ResumeLayout(false);
            this.anapicofreqcontrol.ResumeLayout(false);
            this.anapicofreqcontrol.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topProbeMWf1Indicator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.topProbeMWf0Indicator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomProbeMWf1Indicator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomProbeMWf0Indicator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pumpMWf1Indicator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pumpMWf0Indicator)).EndInit();
            this.groupBox43.ResumeLayout(false);
            this.groupBox43.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topProbeMixerVoltageTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomProbeMixerVoltageTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pumpMixerVoltageTrackBar)).EndInit();
            this.groupBox41.ResumeLayout(false);
            this.groupBox41.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uWaveDCFMTrackBar)).EndInit();
            this.tabPage12.ResumeLayout(false);
            this.groupBox42.ResumeLayout(false);
            this.groupBox40.ResumeLayout(false);
            this.groupBox40.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VCO155FreqTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VCO155AmpTrackBar)).EndInit();
            this.groupBox39.ResumeLayout(false);
            this.groupBox39.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VCO30FreqTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VCO30AmpTrackBar)).EndInit();
            this.groupBox161MHzVCO.ResumeLayout(false);
            this.groupBox161MHzVCO.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VCO161FreqTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VCO161AmpTrackBar)).EndInit();
            this.tabPage6.ResumeLayout(false);
            this.groupBox34.ResumeLayout(false);
            this.groupBox34.PerformLayout();
            this.groupBox35.ResumeLayout(false);
            this.groupBox35.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pumpPolVoltTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pumpPolModeSelectSwitch)).EndInit();
            this.groupBox32.ResumeLayout(false);
            this.groupBox32.PerformLayout();
            this.groupBox33.ResumeLayout(false);
            this.groupBox33.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.probePolVoltTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.probePolModeSelectSwitch)).EndInit();
            this.tabPage5.ResumeLayout(false);
            this.groupBox17.ResumeLayout(false);
            this.groupBox17.PerformLayout();
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            this.tabPage9.ResumeLayout(false);
            this.tabPage9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.switchScanTTLSwitch)).EndInit();
            this.tabPage7.ResumeLayout(false);
            this.tabPage7.PerformLayout();
            this.tabPage10.ResumeLayout(false);
            this.groupBox19.ResumeLayout(false);
            this.groupBox19.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.I2BiasVoltageTrackBar)).EndInit();
            this.groupBox37.ResumeLayout(false);
            this.groupBox37.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.I2ErrorSigGraph)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.pressureMonitorGroupBox.ResumeLayout(false);
            this.pressureMonitorGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.legend2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pressureGraph)).EndInit();
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
			controller.UpdateVMonitorUI();
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


        private void argonShutterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetArgonShutter(argonShutterCheckBox.Checked);
        }

		private void updateLaserPhotodiodesButton_Click(object sender, EventArgs e)
		{
			controller.UpdateLaserPhotodiodesUI();
		}
       
        private void updateMiniFluxgatesButton_Click(object sender, EventArgs e)
        {
           controller.UpdateMiniFluxgatesUI();
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

        private void updatePumpAOMButton_Click(object sender, EventArgs e)
        {
            controller.UpdatePumpAOM();
        }

        private void pumpAOMFreqUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdatePumpAOMFreqMonitor();
        }

        private void pumpAOMTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdatePumpAOM((Double)pumpAOMTrackBar.Value / 100.0);
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

        private void clearAlertButton_Click(object sender, EventArgs e)
        {
            BackColor = DefaultBackColor;
        }

        private void updatePiMonitorButton_Click(object sender, EventArgs e)
        {
            controller.CheckPiMonitor();
        }

        private void updateDiodeCurrentMonButton_Click(object sender, EventArgs e)
        {
            controller.UpdateDiodeCurrentMonitor();
        }


        private void setProbePolAngle_Click(object sender, EventArgs e)
        {
            controller.SetProbePolAngle();
        }

        private void updateProbePolMesAngle_Click(object sender, EventArgs e)
        {
            controller.UpdateProbePolAngleMonitor();
        }

        private void probePolModeSelectSwitch_StateChanged_1(object sender, ActionEventArgs e)
        {
            controller.UpdateProbePolMode();
        }

        private void probePolVoltTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.SetProbePolVoltage();
        }

        private void probePolVoltStopButton_Click(object sender, EventArgs e)
        {
            controller.SetProbePolVoltageZero();
        }

        private void zeroProbePol_Click(object sender, EventArgs e)
        {
            controller.SetProbePolAngleZero();
        }

        private void setPumpPolAngle_Click(object sender, EventArgs e)
        {
            controller.SetPumpPolAngle();
        }

        private void pumpPolVoltStopButton_Click(object sender, EventArgs e)
        {
            controller.SetPumpPolVoltageZero();
        }

        private void pumpPolVoltTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.SetPumpPolVoltage();
        }

        private void updatePumpPolMesAngle_Click(object sender, EventArgs e)
        {
            controller.UpdatePumpPolAngleMonitor();
        }

        private void zeroPumpPol_Click(object sender, EventArgs e)
        {
            controller.SetPumpPolAngleZero();
        }

        private void pumpPolModeSelectSwitch_StateChanged(object sender, ActionEventArgs e)
        {
            controller.UpdatePumpPolMode();
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

        public void AddAlert(string alertText)
        {
            Invoke(new AddAlertDelegate(AddAlertHelper), new object[] { alertText });
        }
        private delegate void AddAlertDelegate(string alertText);
        private void AddAlertHelper(string alertText)
        {
            BackColor = System.Drawing.Color.Red;
            WindowState = FormWindowState.Minimized;
            WindowState = FormWindowState.Normal;
            BringToFront();
            alertTextBox.AppendText(DateTime.Now.ToString() + " " + alertText + "\n");
        }

        #endregion

        private void WindowClosing(object sender, FormClosingEventArgs e)
        {
            controller.WindowClosing();
        }


        private void switch1_StateChanged(object sender, ActionEventArgs e)
        {
            controller.SetSwitchTTL(switchScanTTLSwitch.Value);
        }



        private void automaticBiasCalcButton_Click(object sender, EventArgs e)
        {
            controller.AutomaticBiasCalculation();
        }

      

       

        

        

        private void updateI2ErrorSigButton_Click(object sender, EventArgs e)
        {
            controller.UpdateI2ErrorSigMonitor();
        }

        private void startI2ErrorSigPollButton_Click(object sender, EventArgs e)
        {
            controller.StartI2ErrorSigPoll();
        }

        private void stopI2ErrorSigPollButton_Click(object sender, EventArgs e)
        {
            controller.StopI2ErrorSigPoll();
        }

        private void I2BiasVoltageTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.SetI2Bias((Double)I2BiasVoltageTrackBar.Value / 1000.0);
        }

        private void probeAOMtrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdateProbeAOMV((Double)probeAOMtrackBar.Value / 100.0);
        }

        private void I2AOMFreqUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateProbeAOMFreqMonitor();
        }

        private void UpdateProbeAOMButton_Click(object sender, EventArgs e)
        {
            controller.UpdateProbeAOMV();
        }

       

        private void UpdateI2BiasVoltage_Click(object sender, EventArgs e)
        {
            double voltage = double.Parse(I2BiasVoltageTextBox.Text);
            controller.SetI2Bias(voltage);
            I2BiasVoltageTrackBar.Value = (int)(1000.0 * voltage);
        }

        private void Copyrf1a_Click(object sender, EventArgs e)
        {
            rf1aCentreGuessTextBox.Text = rf1CentrePowerMon.Text;
        }

        private void Copyrf2a_Click(object sender, EventArgs e)
        {
            rf2aCentreGuessTextBox.Text = rf2CentrePowerMon.Text;
        }

        private void Copyrf1f_Click(object sender, EventArgs e)
        {
            rf1fCentreGuessTextBox.Text = rf1CentreFreqMon.Text;
        }

        private void Copyrf2f_Click(object sender, EventArgs e)
        {
            rf2fCentreGuessTextBox.Text = rf2CentreFreqMon.Text;
        }

        private void setAttunatorsToGuesses_Click(object sender, EventArgs e)
        {
            controller.AutomaticRFxACalculation();
        }

        private void setDCFMtoGuess_Click(object sender, EventArgs e)
        {
            controller.AutomaticRFxFCalculation();
        }

        private void clearIMonitorButton_Click(object sender, EventArgs e)
        {
            controller.ClearIMonitorAv();
        }

        private void UpdatePiFlipMonButton_Click(object sender, EventArgs e)
        {
            controller.UpdatePiMonitorUI();
        }

        private void logdataCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (logCurrentDataCheckBox.Checked)
            {
                controller.StartLoggingCurrent();
            }
            else
            {
                controller.StopLoggingCurrent();
            }
        }

        private void probeAOMFreqUpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateProbeAOMFreqMonitor();
        }

        private void pumpAOMFreqUpdateButton_Click_1(object sender, EventArgs e)
        {
            controller.UpdatePumpAOMFreqMonitor();
        }

        private void groupBox39_Enter(object sender, EventArgs e)
        {

        }

        private void VCO161AmpTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdateVCO161AmpVoltage((Double)VCO161AmpTrackBar.Value / 100.0);
        }

        private void VCO161FreqTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdateVCO161FreqVoltage((Double)VCO161FreqTrackBar.Value / 100.0);
        }

        private void VCO30AmpTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdateVCO30AmpVoltage((Double)VCO30AmpTrackBar.Value / 100.0);
        }

        private void VCO30FreqTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdateVCO30FreqVoltage((Double)VCO30FreqTrackBar.Value / 100.0);
        }

        private void VCO155AmpTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdateVCO155AmpVoltage((Double)VCO155AmpTrackBar.Value / 100.0);
        }

        private void VCO155FreqTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdateVCO155FreqVoltage((Double)VCO155FreqTrackBar.Value / 100.0);
        }

        private void VCO161UpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateVCO161AmpV();
            controller.UpdateVCO161FreqV();
        }

        private void VCO30UpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateVCO30AmpV();
            controller.UpdateVCO30FreqV();
        }

        private void VCO155UpdateButton_Click(object sender, EventArgs e)
        {
            controller.UpdateVCO155AmpV();
            controller.UpdateVCO155FreqV();
        }

        private void VCO161StepPlusButton_Click(object sender, EventArgs e)
        {
            controller.IncreaseVCOVoltage();
            controller.TweakVCO161AmpV();
        }

        private void VCO161StepMinusButton_Click(object sender, EventArgs e)
        {
            controller.DecreaseVCOVoltage();
            controller.TweakVCO161AmpV();
        }

        private void VCO161FreqStepPlusButton_Click(object sender, EventArgs e)
        {
            controller.IncreaseVCOVoltage();
            controller.TweakVCO161FreqV();
        }

        private void VCO161FreqStepMinusButton_Click(object sender, EventArgs e)
        {
            controller.DecreaseVCOVoltage();
            controller.TweakVCO161FreqV();
        }

       private void VCO30AmpStepPlusButton_Click(object sender, EventArgs e)
        {
            controller.IncreaseVCOVoltage();
            controller.TweakVCO30AmpV();
        }

       private void VCO30AmpStepMinusButton_Click(object sender, EventArgs e)
       {
           controller.DecreaseVCOVoltage();
           controller.TweakVCO30AmpV();
       }

       private void VCO30FreqStepPlusButton_Click(object sender, EventArgs e)
       {
           controller.IncreaseVCOVoltage();
           controller.TweakVCO30FreqV();
       }

       private void VCO30FreqStepMinusButton_Click(object sender, EventArgs e)
       {
           controller.DecreaseVCOVoltage();
           controller.TweakVCO30FreqV();
       }

       private void VCO155AmpStepPlusButton_Click(object sender, EventArgs e)
       {
           controller.IncreaseVCOVoltage();
           controller.TweakVCO155AmpV();
       }

       private void VCO155AmpStepMinusButton_Click(object sender, EventArgs e)
       {
           controller.DecreaseVCOVoltage();
           controller.TweakVCO155AmpV();
       }

       private void VCO155FreqStepPlusButton_Click(object sender, EventArgs e)
       {
           controller.IncreaseVCOVoltage();
           controller.TweakVCO155FreqV();
       }

       private void VCO155FreqStepMinusButton_Click(object sender, EventArgs e)
       {
           controller.DecreaseVCOVoltage();
           controller.TweakVCO155FreqV();
       }

         private void uWaveUpdateButton_Click(object sender, EventArgs e)
       {
           controller.UpdateuWaveDCFMV();
       }

         private void uWaveDCFMTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdateuWaveDCFMVoltage((Double)uWaveDCFMTrackBar.Value / 100.0);
        }

         private void mixerVoltageTrackBar_Scroll(object sender, EventArgs e)
        {
            controller.UpdatePumpMicrowaveMixerV((Double)pumpMixerVoltageTrackBar.Value / 100.0);
        }

         private void bottomProbeMixerVoltageTrackBar_Scroll(object sender, EventArgs e)
         {
             controller.UpdateBottomProbeMicrowaveMixerV((Double)bottomProbeMixerVoltageTrackBar.Value / 100.0);
         }

         private void topProbeMixerVoltageTrackBar_Scroll(object sender, EventArgs e)
         {
             controller.UpdateTopProbeMicrowaveMixerV((Double)topProbeMixerVoltageTrackBar.Value / 100.0);
         }

           private void uWaveDCFMPlusButton_Click(object sender, EventArgs e)
       {
           controller.TweakuWaveDCFMVoltage();
       }

       private void uWaveDCFMMinusButton_Click(object sender, EventArgs e)
       {
           controller.TweakuWaveDCFMVoltage();
       }


       private void pumpRFCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.EnablePumpRFSwitch(pumpRFCheckBox.Checked);	
       }

       private void mwEnableCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.EnableMicrowaves(mwEnableCheckBox.Checked);
       }

       private void rfSensorCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           synthSensorConnectCheckBox.Checked = !rfSensorCheckBox.Checked;
       }

       private void synthSensorConnectCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.ConnectRFToSensorSwitch(synthSensorConnectCheckBox.Checked);
           rfSensorCheckBox.Checked = !synthSensorConnectCheckBox.Checked;
       }

       private void pumpHornCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.SendMicrowavesToPump(pumpHornCheckBox.Checked);
       }

       private void firstProbeHornCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.SendMicrowavesToBottomProbe(firstProbeHornCheckBox.Checked);
       }

       private void secondProbeHornCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.SendMicrowavesToTopProbe(secondProbeHornCheckBox.Checked);
       }

       private void mixerVoltateUpdateButton_Click(object sender, EventArgs e)
       {
           controller.UpdateuWaveMixerV();
       }

       private void uWaveDCFMTextBox_TextChanged(object sender, EventArgs e)
       {

       }

       private void uWaveDCFMStepTextBox_TextChanged(object sender, EventArgs e)
       {

       }

       private void anapicoEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.EnableAnapico(anapicoEnabledCheckBox.Checked);
       }

       private void anapicoCwFreqBox_TextChanged(object sender, EventArgs e)
       {

       }

       private void anapicoCwFreqUpdate_Click(object sender, EventArgs e)
       {
           controller.UpdateAnapicoCW();
       }

       private void yagFlashlampVTextBox_TextChanged(object sender, EventArgs e)
       {

       }

       private void textBox2_TextChanged(object sender, EventArgs e)
       {

       }

       private void listSweepEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.EnableAnapicoListSweep(listSweepEnabledCheckBox.Checked);
       }

       private void label86_Click(object sender, EventArgs e)
       {

       }

       private void label90_Click(object sender, EventArgs e)
       {

       }

       private void label92_Click(object sender, EventArgs e)
       {

       }

       private void microwaveStateCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           controller.UpdateAnapicoRAMList(microwaveStateCheckBox.Checked);
       }

       private void anapicof0FreqTextBox_TextChanged(object sender, EventArgs e)
       {

       }

       private void getCurrentListButton_Click(object sender, EventArgs e)
       {
           controller.GetAnapicoCurrentList();
       }

       private void updatePressureMonitorButton_Click(object sender, EventArgs e)
       {
           controller.UpdatePressureMonitor();
       }

       private void clearPressureMonitorButton_Click(object sender, EventArgs e)
       {
           controller.ClearPressureMonitorAv();
       }

       private void startPressureMonitorPollButton_Click(object sender, EventArgs e)
       {
           controller.StartPressureMonitorPoll();
       }

       private void stopPressureMonitorPollButton_Click(object sender, EventArgs e)
       {
           controller.StopPressureMonitorPoll();
       }

       private void logPressureDataCheckBox_CheckedChanged(object sender, EventArgs e)
       {
           if (logPressureDataCheckBox.Checked)
           {
               controller.StartLoggingPressure();
           }
           else
           {
               controller.StopLoggingPressure();
           }
       }



        
        


    }
}
       