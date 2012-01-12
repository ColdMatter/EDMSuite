using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Collections.Generic;


using NationalInstruments.UI.WindowsForms;
using NationalInstruments.UI;


namespace SympatheticHardwareControl
{
    /// <summary>
    /// Front panel for the sympathetic hardware controller. Everything is just stuffed in there. No particularly
    /// clever structure. This class just hands everything straight off to the controller. It has a few
    /// thread safe wrappers so that remote calls can safely manipulate the front panel.
    /// </summary>
    public class ControlWindow : System.Windows.Forms.Form
    {
        #region Setup

        public Controller controller;
        private Dictionary<string, TextBox> AOTextBoxes = new Dictionary<string, TextBox>();
        private Dictionary<string, CheckBox> DOCheckBoxes = new Dictionary<string, CheckBox>();
            

        public ControlWindow()
        {
            InitializeComponent();
            AOTextBoxes["aom0amplitude"] = aom0rfAmplitudeTextBox;
            AOTextBoxes["aom1amplitude"] = aom1rfAmplitudeTextBox;
            AOTextBoxes["aom2amplitude"] = aom2rfAmplitudeTextBox;
            AOTextBoxes["aom3amplitude"] = aom3rfAmplitudeTextBox;
            AOTextBoxes["aom0frequency"] = aom0rfFrequencyTextBox;
            AOTextBoxes["aom1frequency"] = aom1rfFrequencyTextBox;
            AOTextBoxes["aom2frequency"] = aom2rfFrequencyTextBox;
            AOTextBoxes["aom3frequency"] = aom3rfFrequencyTextBox;
            AOTextBoxes["coil0current"] = coil0CurrentTextBox;
            AOTextBoxes["coil1current"] = coil1CurrentTextBox;
            DOCheckBoxes["aom0enable"] = aom0CheckBox;
            DOCheckBoxes["aom1enable"] = aom1CheckBox;
            DOCheckBoxes["aom2enable"] = aom2CheckBox;
            DOCheckBoxes["aom3enable"] = aom3CheckBox;
        }

        private void WindowClosing(object sender, FormClosingEventArgs e)
        {
            controller.ControllerStopping();
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            controller.ControllerLoaded();
            
        }

        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.shcTabs = new System.Windows.Forms.TabControl();
            this.tabCamera = new System.Windows.Forms.TabPage();
            this.stopStreamButton = new System.Windows.Forms.Button();
            this.streamButton = new System.Windows.Forms.Button();
            this.snapshotButton = new System.Windows.Forms.Button();
            this.tabLasers = new System.Windows.Forms.TabPage();
            this.aom3ControlBox = new System.Windows.Forms.GroupBox();
            this.aom3Label3 = new System.Windows.Forms.Label();
            this.aom3Label1 = new System.Windows.Forms.Label();
            this.aom3CheckBox = new System.Windows.Forms.CheckBox();
            this.aom3rfFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.aom3rfAmplitudeTextBox = new System.Windows.Forms.TextBox();
            this.aom3Label2 = new System.Windows.Forms.Label();
            this.aom3Label0 = new System.Windows.Forms.Label();
            this.aom2ControlBox = new System.Windows.Forms.GroupBox();
            this.aom2Label3 = new System.Windows.Forms.Label();
            this.aom2Label1 = new System.Windows.Forms.Label();
            this.aom2CheckBox = new System.Windows.Forms.CheckBox();
            this.aom2rfFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.aom2rfAmplitudeTextBox = new System.Windows.Forms.TextBox();
            this.aom2Label2 = new System.Windows.Forms.Label();
            this.aom2Label0 = new System.Windows.Forms.Label();
            this.aom1ControlBox = new System.Windows.Forms.GroupBox();
            this.aom1Label3 = new System.Windows.Forms.Label();
            this.aom1Label1 = new System.Windows.Forms.Label();
            this.aom1CheckBox = new System.Windows.Forms.CheckBox();
            this.aom1rfFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.aom1rfAmplitudeTextBox = new System.Windows.Forms.TextBox();
            this.aom1Label2 = new System.Windows.Forms.Label();
            this.aom1Label0 = new System.Windows.Forms.Label();
            this.aom0ControlBox = new System.Windows.Forms.GroupBox();
            this.aom0Label3 = new System.Windows.Forms.Label();
            this.aom0Label1 = new System.Windows.Forms.Label();
            this.aom0CheckBox = new System.Windows.Forms.CheckBox();
            this.aom0rfFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.aom0rfAmplitudeTextBox = new System.Windows.Forms.TextBox();
            this.aom0Label2 = new System.Windows.Forms.Label();
            this.aom0Label0 = new System.Windows.Forms.Label();
            this.tabCoils = new System.Windows.Forms.TabPage();
            this.coil1GroupBox = new System.Windows.Forms.GroupBox();
            this.coil1Label1 = new System.Windows.Forms.Label();
            this.coil1CurrentTextBox = new System.Windows.Forms.TextBox();
            this.coil1Label0 = new System.Windows.Forms.Label();
            this.coil0GroupBox = new System.Windows.Forms.GroupBox();
            this.coil0Label1 = new System.Windows.Forms.Label();
            this.coil0CurrentTextBox = new System.Windows.Forms.TextBox();
            this.coil0Label0 = new System.Windows.Forms.Label();
            this.tabTranslationStage = new System.Windows.Forms.TabPage();
            this.initParamsBox = new System.Windows.Forms.GroupBox();
            this.TSVelTextBox = new System.Windows.Forms.TextBox();
            this.TSStepsTextBox = new System.Windows.Forms.TextBox();
            this.TSDecTextBox = new System.Windows.Forms.TextBox();
            this.TSAccTextBox = new System.Windows.Forms.TextBox();
            this.TSInitButton = new System.Windows.Forms.Button();
            this.TSClearButton = new System.Windows.Forms.Button();
            this.TSRestartButton = new System.Windows.Forms.Button();
            this.TSReturnButton = new System.Windows.Forms.Button();
            this.read = new System.Windows.Forms.Button();
            this.disposeButton = new System.Windows.Forms.Button();
            this.TSOffButton = new System.Windows.Forms.Button();
            this.TSGoButton = new System.Windows.Forms.Button();
            this.TSOnButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hardwareMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openImageViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteControlLED = new NationalInstruments.UI.WindowsForms.Led();
            this.label1 = new System.Windows.Forms.Label();
            this.updateHardwareButton = new System.Windows.Forms.Button();
            this.consoleRichTextBox = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.AutoTriggerCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.TSConnectButton = new System.Windows.Forms.Button();
            this.RS232GroupBox = new System.Windows.Forms.GroupBox();
            this.shcTabs.SuspendLayout();
            this.tabCamera.SuspendLayout();
            this.tabLasers.SuspendLayout();
            this.aom3ControlBox.SuspendLayout();
            this.aom2ControlBox.SuspendLayout();
            this.aom1ControlBox.SuspendLayout();
            this.aom0ControlBox.SuspendLayout();
            this.tabCoils.SuspendLayout();
            this.coil1GroupBox.SuspendLayout();
            this.coil0GroupBox.SuspendLayout();
            this.tabTranslationStage.SuspendLayout();
            this.initParamsBox.SuspendLayout();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.remoteControlLED)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.RS232GroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // shcTabs
            // 
            this.shcTabs.AllowDrop = true;
            this.shcTabs.Controls.Add(this.tabCamera);
            this.shcTabs.Controls.Add(this.tabLasers);
            this.shcTabs.Controls.Add(this.tabCoils);
            this.shcTabs.Controls.Add(this.tabTranslationStage);
            this.shcTabs.Location = new System.Drawing.Point(3, 27);
            this.shcTabs.Name = "shcTabs";
            this.shcTabs.SelectedIndex = 0;
            this.shcTabs.Size = new System.Drawing.Size(666, 235);
            this.shcTabs.TabIndex = 0;
            // 
            // tabCamera
            // 
            this.tabCamera.Controls.Add(this.stopStreamButton);
            this.tabCamera.Controls.Add(this.streamButton);
            this.tabCamera.Controls.Add(this.snapshotButton);
            this.tabCamera.Location = new System.Drawing.Point(4, 22);
            this.tabCamera.Name = "tabCamera";
            this.tabCamera.Padding = new System.Windows.Forms.Padding(3);
            this.tabCamera.Size = new System.Drawing.Size(658, 209);
            this.tabCamera.TabIndex = 0;
            this.tabCamera.Text = "Camera Control";
            this.tabCamera.UseVisualStyleBackColor = true;
            // 
            // stopStreamButton
            // 
            this.stopStreamButton.Location = new System.Drawing.Point(168, 6);
            this.stopStreamButton.Name = "stopStreamButton";
            this.stopStreamButton.Size = new System.Drawing.Size(75, 23);
            this.stopStreamButton.TabIndex = 18;
            this.stopStreamButton.Text = "Stop";
            this.stopStreamButton.UseVisualStyleBackColor = true;
            this.stopStreamButton.Click += new System.EventHandler(this.stopStreamButton_Click);
            // 
            // streamButton
            // 
            this.streamButton.Location = new System.Drawing.Point(87, 6);
            this.streamButton.Name = "streamButton";
            this.streamButton.Size = new System.Drawing.Size(75, 23);
            this.streamButton.TabIndex = 17;
            this.streamButton.Text = "Stream";
            this.streamButton.UseVisualStyleBackColor = true;
            this.streamButton.Click += new System.EventHandler(this.streamButton_Click);
            // 
            // snapshotButton
            // 
            this.snapshotButton.Location = new System.Drawing.Point(6, 6);
            this.snapshotButton.Name = "snapshotButton";
            this.snapshotButton.Size = new System.Drawing.Size(75, 23);
            this.snapshotButton.TabIndex = 15;
            this.snapshotButton.Text = "Snapshot";
            this.snapshotButton.UseVisualStyleBackColor = true;
            this.snapshotButton.Click += new System.EventHandler(this.snapshotButton_Click);
            // 
            // tabLasers
            // 
            this.tabLasers.AutoScroll = true;
            this.tabLasers.Controls.Add(this.aom3ControlBox);
            this.tabLasers.Controls.Add(this.aom2ControlBox);
            this.tabLasers.Controls.Add(this.aom1ControlBox);
            this.tabLasers.Controls.Add(this.aom0ControlBox);
            this.tabLasers.Location = new System.Drawing.Point(4, 22);
            this.tabLasers.Name = "tabLasers";
            this.tabLasers.Padding = new System.Windows.Forms.Padding(3);
            this.tabLasers.Size = new System.Drawing.Size(658, 209);
            this.tabLasers.TabIndex = 1;
            this.tabLasers.Text = "Laser Control";
            this.tabLasers.UseVisualStyleBackColor = true;
            // 
            // aom3ControlBox
            // 
            this.aom3ControlBox.Controls.Add(this.aom3Label3);
            this.aom3ControlBox.Controls.Add(this.aom3Label1);
            this.aom3ControlBox.Controls.Add(this.aom3CheckBox);
            this.aom3ControlBox.Controls.Add(this.aom3rfFrequencyTextBox);
            this.aom3ControlBox.Controls.Add(this.aom3rfAmplitudeTextBox);
            this.aom3ControlBox.Controls.Add(this.aom3Label2);
            this.aom3ControlBox.Controls.Add(this.aom3Label0);
            this.aom3ControlBox.Location = new System.Drawing.Point(3, 156);
            this.aom3ControlBox.Name = "aom3ControlBox";
            this.aom3ControlBox.Size = new System.Drawing.Size(549, 45);
            this.aom3ControlBox.TabIndex = 19;
            this.aom3ControlBox.TabStop = false;
            this.aom3ControlBox.Text = "AOM 3 (Absorption)";
            // 
            // aom3Label3
            // 
            this.aom3Label3.AutoSize = true;
            this.aom3Label3.Location = new System.Drawing.Point(522, 21);
            this.aom3Label3.Name = "aom3Label3";
            this.aom3Label3.Size = new System.Drawing.Size(14, 13);
            this.aom3Label3.TabIndex = 17;
            this.aom3Label3.Text = "V";
            // 
            // aom3Label1
            // 
            this.aom3Label1.AutoSize = true;
            this.aom3Label1.Location = new System.Drawing.Point(260, 21);
            this.aom3Label1.Name = "aom3Label1";
            this.aom3Label1.Size = new System.Drawing.Size(29, 13);
            this.aom3Label1.TabIndex = 16;
            this.aom3Label1.Text = "MHz";
            // 
            // aom3CheckBox
            // 
            this.aom3CheckBox.AutoSize = true;
            this.aom3CheckBox.Location = new System.Drawing.Point(38, 21);
            this.aom3CheckBox.Name = "aom3CheckBox";
            this.aom3CheckBox.Size = new System.Drawing.Size(15, 14);
            this.aom3CheckBox.TabIndex = 10;
            this.aom3CheckBox.UseVisualStyleBackColor = true;
            // 
            // aom3rfFrequencyTextBox
            // 
            this.aom3rfFrequencyTextBox.Location = new System.Drawing.Point(155, 17);
            this.aom3rfFrequencyTextBox.Name = "aom3rfFrequencyTextBox";
            this.aom3rfFrequencyTextBox.Size = new System.Drawing.Size(103, 20);
            this.aom3rfFrequencyTextBox.TabIndex = 0;
            this.aom3rfFrequencyTextBox.Text = "200";
            // 
            // aom3rfAmplitudeTextBox
            // 
            this.aom3rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom3rfAmplitudeTextBox.Name = "aom3rfAmplitudeTextBox";
            this.aom3rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom3rfAmplitudeTextBox.TabIndex = 8;
            this.aom3rfAmplitudeTextBox.Text = "0";
            // 
            // aom3Label2
            // 
            this.aom3Label2.AutoSize = true;
            this.aom3Label2.Location = new System.Drawing.Point(348, 20);
            this.aom3Label2.Name = "aom3Label2";
            this.aom3Label2.Size = new System.Drawing.Size(70, 13);
            this.aom3Label2.TabIndex = 7;
            this.aom3Label2.Text = "RF Amplitude";
            // 
            // aom3Label0
            // 
            this.aom3Label0.AutoSize = true;
            this.aom3Label0.Location = new System.Drawing.Point(78, 20);
            this.aom3Label0.Name = "aom3Label0";
            this.aom3Label0.Size = new System.Drawing.Size(74, 13);
            this.aom3Label0.TabIndex = 6;
            this.aom3Label0.Text = "RF Frequency";
            // 
            // aom2ControlBox
            // 
            this.aom2ControlBox.Controls.Add(this.aom2Label3);
            this.aom2ControlBox.Controls.Add(this.aom2Label1);
            this.aom2ControlBox.Controls.Add(this.aom2CheckBox);
            this.aom2ControlBox.Controls.Add(this.aom2rfFrequencyTextBox);
            this.aom2ControlBox.Controls.Add(this.aom2rfAmplitudeTextBox);
            this.aom2ControlBox.Controls.Add(this.aom2Label2);
            this.aom2ControlBox.Controls.Add(this.aom2Label0);
            this.aom2ControlBox.Location = new System.Drawing.Point(3, 105);
            this.aom2ControlBox.Name = "aom2ControlBox";
            this.aom2ControlBox.Size = new System.Drawing.Size(549, 45);
            this.aom2ControlBox.TabIndex = 20;
            this.aom2ControlBox.TabStop = false;
            this.aom2ControlBox.Text = "AOM 2 (Zeeman)";
            // 
            // aom2Label3
            // 
            this.aom2Label3.AutoSize = true;
            this.aom2Label3.Location = new System.Drawing.Point(522, 21);
            this.aom2Label3.Name = "aom2Label3";
            this.aom2Label3.Size = new System.Drawing.Size(14, 13);
            this.aom2Label3.TabIndex = 17;
            this.aom2Label3.Text = "V";
            // 
            // aom2Label1
            // 
            this.aom2Label1.AutoSize = true;
            this.aom2Label1.Location = new System.Drawing.Point(260, 21);
            this.aom2Label1.Name = "aom2Label1";
            this.aom2Label1.Size = new System.Drawing.Size(29, 13);
            this.aom2Label1.TabIndex = 16;
            this.aom2Label1.Text = "MHz";
            // 
            // aom2CheckBox
            // 
            this.aom2CheckBox.AutoSize = true;
            this.aom2CheckBox.Location = new System.Drawing.Point(38, 21);
            this.aom2CheckBox.Name = "aom2CheckBox";
            this.aom2CheckBox.Size = new System.Drawing.Size(15, 14);
            this.aom2CheckBox.TabIndex = 10;
            this.aom2CheckBox.UseVisualStyleBackColor = true;
            // 
            // aom2rfFrequencyTextBox
            // 
            this.aom2rfFrequencyTextBox.Location = new System.Drawing.Point(155, 17);
            this.aom2rfFrequencyTextBox.Name = "aom2rfFrequencyTextBox";
            this.aom2rfFrequencyTextBox.Size = new System.Drawing.Size(103, 20);
            this.aom2rfFrequencyTextBox.TabIndex = 0;
            this.aom2rfFrequencyTextBox.Text = "0";
            // 
            // aom2rfAmplitudeTextBox
            // 
            this.aom2rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom2rfAmplitudeTextBox.Name = "aom2rfAmplitudeTextBox";
            this.aom2rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom2rfAmplitudeTextBox.TabIndex = 8;
            this.aom2rfAmplitudeTextBox.Text = "0";
            // 
            // aom2Label2
            // 
            this.aom2Label2.AutoSize = true;
            this.aom2Label2.Location = new System.Drawing.Point(348, 20);
            this.aom2Label2.Name = "aom2Label2";
            this.aom2Label2.Size = new System.Drawing.Size(70, 13);
            this.aom2Label2.TabIndex = 7;
            this.aom2Label2.Text = "RF Amplitude";
            // 
            // aom2Label0
            // 
            this.aom2Label0.AutoSize = true;
            this.aom2Label0.Location = new System.Drawing.Point(78, 20);
            this.aom2Label0.Name = "aom2Label0";
            this.aom2Label0.Size = new System.Drawing.Size(74, 13);
            this.aom2Label0.TabIndex = 6;
            this.aom2Label0.Text = "RF Frequency";
            // 
            // aom1ControlBox
            // 
            this.aom1ControlBox.Controls.Add(this.aom1Label3);
            this.aom1ControlBox.Controls.Add(this.aom1Label1);
            this.aom1ControlBox.Controls.Add(this.aom1CheckBox);
            this.aom1ControlBox.Controls.Add(this.aom1rfFrequencyTextBox);
            this.aom1ControlBox.Controls.Add(this.aom1rfAmplitudeTextBox);
            this.aom1ControlBox.Controls.Add(this.aom1Label2);
            this.aom1ControlBox.Controls.Add(this.aom1Label0);
            this.aom1ControlBox.Location = new System.Drawing.Point(3, 54);
            this.aom1ControlBox.Name = "aom1ControlBox";
            this.aom1ControlBox.Size = new System.Drawing.Size(549, 45);
            this.aom1ControlBox.TabIndex = 19;
            this.aom1ControlBox.TabStop = false;
            this.aom1ControlBox.Text = "AOM 1 (MOT Repump)";
            // 
            // aom1Label3
            // 
            this.aom1Label3.AutoSize = true;
            this.aom1Label3.Location = new System.Drawing.Point(522, 21);
            this.aom1Label3.Name = "aom1Label3";
            this.aom1Label3.Size = new System.Drawing.Size(14, 13);
            this.aom1Label3.TabIndex = 17;
            this.aom1Label3.Text = "V";
            // 
            // aom1Label1
            // 
            this.aom1Label1.AutoSize = true;
            this.aom1Label1.Location = new System.Drawing.Point(260, 21);
            this.aom1Label1.Name = "aom1Label1";
            this.aom1Label1.Size = new System.Drawing.Size(29, 13);
            this.aom1Label1.TabIndex = 16;
            this.aom1Label1.Text = "MHz";
            // 
            // aom1CheckBox
            // 
            this.aom1CheckBox.AutoSize = true;
            this.aom1CheckBox.Location = new System.Drawing.Point(38, 21);
            this.aom1CheckBox.Name = "aom1CheckBox";
            this.aom1CheckBox.Size = new System.Drawing.Size(15, 14);
            this.aom1CheckBox.TabIndex = 10;
            this.aom1CheckBox.UseVisualStyleBackColor = true;
            // 
            // aom1rfFrequencyTextBox
            // 
            this.aom1rfFrequencyTextBox.Location = new System.Drawing.Point(155, 17);
            this.aom1rfFrequencyTextBox.Name = "aom1rfFrequencyTextBox";
            this.aom1rfFrequencyTextBox.Size = new System.Drawing.Size(103, 20);
            this.aom1rfFrequencyTextBox.TabIndex = 0;
            this.aom1rfFrequencyTextBox.Text = "0";
            // 
            // aom1rfAmplitudeTextBox
            // 
            this.aom1rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom1rfAmplitudeTextBox.Name = "aom1rfAmplitudeTextBox";
            this.aom1rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom1rfAmplitudeTextBox.TabIndex = 8;
            this.aom1rfAmplitudeTextBox.Text = "0";
            // 
            // aom1Label2
            // 
            this.aom1Label2.AutoSize = true;
            this.aom1Label2.Location = new System.Drawing.Point(348, 20);
            this.aom1Label2.Name = "aom1Label2";
            this.aom1Label2.Size = new System.Drawing.Size(70, 13);
            this.aom1Label2.TabIndex = 7;
            this.aom1Label2.Text = "RF Amplitude";
            // 
            // aom1Label0
            // 
            this.aom1Label0.AutoSize = true;
            this.aom1Label0.Location = new System.Drawing.Point(78, 20);
            this.aom1Label0.Name = "aom1Label0";
            this.aom1Label0.Size = new System.Drawing.Size(74, 13);
            this.aom1Label0.TabIndex = 6;
            this.aom1Label0.Text = "RF Frequency";
            // 
            // aom0ControlBox
            // 
            this.aom0ControlBox.Controls.Add(this.aom0Label3);
            this.aom0ControlBox.Controls.Add(this.aom0Label1);
            this.aom0ControlBox.Controls.Add(this.aom0CheckBox);
            this.aom0ControlBox.Controls.Add(this.aom0rfFrequencyTextBox);
            this.aom0ControlBox.Controls.Add(this.aom0rfAmplitudeTextBox);
            this.aom0ControlBox.Controls.Add(this.aom0Label2);
            this.aom0ControlBox.Controls.Add(this.aom0Label0);
            this.aom0ControlBox.Location = new System.Drawing.Point(3, 3);
            this.aom0ControlBox.Name = "aom0ControlBox";
            this.aom0ControlBox.Size = new System.Drawing.Size(549, 45);
            this.aom0ControlBox.TabIndex = 12;
            this.aom0ControlBox.TabStop = false;
            this.aom0ControlBox.Text = "AOM 0 (MOT AOM)";
            // 
            // aom0Label3
            // 
            this.aom0Label3.AutoSize = true;
            this.aom0Label3.Location = new System.Drawing.Point(522, 21);
            this.aom0Label3.Name = "aom0Label3";
            this.aom0Label3.Size = new System.Drawing.Size(14, 13);
            this.aom0Label3.TabIndex = 17;
            this.aom0Label3.Text = "V";
            // 
            // aom0Label1
            // 
            this.aom0Label1.AutoSize = true;
            this.aom0Label1.Location = new System.Drawing.Point(260, 21);
            this.aom0Label1.Name = "aom0Label1";
            this.aom0Label1.Size = new System.Drawing.Size(29, 13);
            this.aom0Label1.TabIndex = 16;
            this.aom0Label1.Text = "MHz";
            // 
            // aom0CheckBox
            // 
            this.aom0CheckBox.AutoSize = true;
            this.aom0CheckBox.Location = new System.Drawing.Point(38, 21);
            this.aom0CheckBox.Name = "aom0CheckBox";
            this.aom0CheckBox.Size = new System.Drawing.Size(15, 14);
            this.aom0CheckBox.TabIndex = 10;
            this.aom0CheckBox.UseVisualStyleBackColor = true;
            // 
            // aom0rfFrequencyTextBox
            // 
            this.aom0rfFrequencyTextBox.Location = new System.Drawing.Point(155, 17);
            this.aom0rfFrequencyTextBox.Name = "aom0rfFrequencyTextBox";
            this.aom0rfFrequencyTextBox.Size = new System.Drawing.Size(103, 20);
            this.aom0rfFrequencyTextBox.TabIndex = 0;
            this.aom0rfFrequencyTextBox.Text = "0";
            // 
            // aom0rfAmplitudeTextBox
            // 
            this.aom0rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom0rfAmplitudeTextBox.Name = "aom0rfAmplitudeTextBox";
            this.aom0rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom0rfAmplitudeTextBox.TabIndex = 8;
            this.aom0rfAmplitudeTextBox.Text = "0";
            // 
            // aom0Label2
            // 
            this.aom0Label2.AutoSize = true;
            this.aom0Label2.Location = new System.Drawing.Point(348, 20);
            this.aom0Label2.Name = "aom0Label2";
            this.aom0Label2.Size = new System.Drawing.Size(70, 13);
            this.aom0Label2.TabIndex = 7;
            this.aom0Label2.Text = "RF Amplitude";
            // 
            // aom0Label0
            // 
            this.aom0Label0.AutoSize = true;
            this.aom0Label0.Location = new System.Drawing.Point(78, 20);
            this.aom0Label0.Name = "aom0Label0";
            this.aom0Label0.Size = new System.Drawing.Size(74, 13);
            this.aom0Label0.TabIndex = 6;
            this.aom0Label0.Text = "RF Frequency";
            // 
            // tabCoils
            // 
            this.tabCoils.Controls.Add(this.coil1GroupBox);
            this.tabCoils.Controls.Add(this.coil0GroupBox);
            this.tabCoils.Location = new System.Drawing.Point(4, 22);
            this.tabCoils.Name = "tabCoils";
            this.tabCoils.Size = new System.Drawing.Size(658, 209);
            this.tabCoils.TabIndex = 2;
            this.tabCoils.Text = "Magnetic Field Control";
            this.tabCoils.UseVisualStyleBackColor = true;
            // 
            // coil1GroupBox
            // 
            this.coil1GroupBox.Controls.Add(this.coil1Label1);
            this.coil1GroupBox.Controls.Add(this.coil1CurrentTextBox);
            this.coil1GroupBox.Controls.Add(this.coil1Label0);
            this.coil1GroupBox.Location = new System.Drawing.Point(3, 54);
            this.coil1GroupBox.Name = "coil1GroupBox";
            this.coil1GroupBox.Size = new System.Drawing.Size(225, 45);
            this.coil1GroupBox.TabIndex = 19;
            this.coil1GroupBox.TabStop = false;
            this.coil1GroupBox.Text = "Bias field coils";
            // 
            // coil1Label1
            // 
            this.coil1Label1.AutoSize = true;
            this.coil1Label1.Location = new System.Drawing.Point(200, 21);
            this.coil1Label1.Name = "coil1Label1";
            this.coil1Label1.Size = new System.Drawing.Size(14, 13);
            this.coil1Label1.TabIndex = 17;
            this.coil1Label1.Text = "A";
            // 
            // coil1CurrentTextBox
            // 
            this.coil1CurrentTextBox.Location = new System.Drawing.Point(99, 17);
            this.coil1CurrentTextBox.Name = "coil1CurrentTextBox";
            this.coil1CurrentTextBox.Size = new System.Drawing.Size(100, 20);
            this.coil1CurrentTextBox.TabIndex = 8;
            this.coil1CurrentTextBox.Text = "0";
            // 
            // coil1Label0
            // 
            this.coil1Label0.AutoSize = true;
            this.coil1Label0.Location = new System.Drawing.Point(40, 20);
            this.coil1Label0.Name = "coil1Label0";
            this.coil1Label0.Size = new System.Drawing.Size(41, 13);
            this.coil1Label0.TabIndex = 7;
            this.coil1Label0.Text = "Current";
            // 
            // coil0GroupBox
            // 
            this.coil0GroupBox.Controls.Add(this.coil0Label1);
            this.coil0GroupBox.Controls.Add(this.coil0CurrentTextBox);
            this.coil0GroupBox.Controls.Add(this.coil0Label0);
            this.coil0GroupBox.Location = new System.Drawing.Point(3, 3);
            this.coil0GroupBox.Name = "coil0GroupBox";
            this.coil0GroupBox.Size = new System.Drawing.Size(225, 45);
            this.coil0GroupBox.TabIndex = 13;
            this.coil0GroupBox.TabStop = false;
            this.coil0GroupBox.Text = "MOT coils";
            // 
            // coil0Label1
            // 
            this.coil0Label1.AutoSize = true;
            this.coil0Label1.Location = new System.Drawing.Point(200, 21);
            this.coil0Label1.Name = "coil0Label1";
            this.coil0Label1.Size = new System.Drawing.Size(14, 13);
            this.coil0Label1.TabIndex = 17;
            this.coil0Label1.Text = "A";
            // 
            // coil0CurrentTextBox
            // 
            this.coil0CurrentTextBox.Location = new System.Drawing.Point(99, 17);
            this.coil0CurrentTextBox.Name = "coil0CurrentTextBox";
            this.coil0CurrentTextBox.Size = new System.Drawing.Size(100, 20);
            this.coil0CurrentTextBox.TabIndex = 8;
            this.coil0CurrentTextBox.Text = "0";
            // 
            // coil0Label0
            // 
            this.coil0Label0.AutoSize = true;
            this.coil0Label0.Location = new System.Drawing.Point(40, 20);
            this.coil0Label0.Name = "coil0Label0";
            this.coil0Label0.Size = new System.Drawing.Size(41, 13);
            this.coil0Label0.TabIndex = 7;
            this.coil0Label0.Text = "Current";
            // 
            // tabTranslationStage
            // 
            this.tabTranslationStage.Controls.Add(this.groupBox4);
            this.tabTranslationStage.Controls.Add(this.RS232GroupBox);
            this.tabTranslationStage.Controls.Add(this.groupBox3);
            this.tabTranslationStage.Controls.Add(this.groupBox2);
            this.tabTranslationStage.Controls.Add(this.groupBox1);
            this.tabTranslationStage.Controls.Add(this.initParamsBox);
            this.tabTranslationStage.Location = new System.Drawing.Point(4, 22);
            this.tabTranslationStage.Name = "tabTranslationStage";
            this.tabTranslationStage.Size = new System.Drawing.Size(658, 209);
            this.tabTranslationStage.TabIndex = 3;
            this.tabTranslationStage.Text = "Translation Stage Control";
            this.tabTranslationStage.UseVisualStyleBackColor = true;
            // 
            // initParamsBox
            // 
            this.initParamsBox.Controls.Add(this.TSInitButton);
            this.initParamsBox.Controls.Add(this.label9);
            this.initParamsBox.Controls.Add(this.label8);
            this.initParamsBox.Controls.Add(this.label7);
            this.initParamsBox.Controls.Add(this.label6);
            this.initParamsBox.Controls.Add(this.label5);
            this.initParamsBox.Controls.Add(this.label4);
            this.initParamsBox.Controls.Add(this.label3);
            this.initParamsBox.Controls.Add(this.label2);
            this.initParamsBox.Controls.Add(this.TSVelTextBox);
            this.initParamsBox.Controls.Add(this.TSStepsTextBox);
            this.initParamsBox.Controls.Add(this.TSDecTextBox);
            this.initParamsBox.Controls.Add(this.TSAccTextBox);
            this.initParamsBox.Location = new System.Drawing.Point(5, 63);
            this.initParamsBox.Name = "initParamsBox";
            this.initParamsBox.Size = new System.Drawing.Size(260, 143);
            this.initParamsBox.TabIndex = 9;
            this.initParamsBox.TabStop = false;
            this.initParamsBox.Text = "Initialize parameters";
            // 
            // TSVelTextBox
            // 
            this.TSVelTextBox.Location = new System.Drawing.Point(115, 92);
            this.TSVelTextBox.Name = "TSVelTextBox";
            this.TSVelTextBox.Size = new System.Drawing.Size(100, 20);
            this.TSVelTextBox.TabIndex = 4;
            this.TSVelTextBox.Text = "50";
            // 
            // TSStepsTextBox
            // 
            this.TSStepsTextBox.Location = new System.Drawing.Point(115, 67);
            this.TSStepsTextBox.Name = "TSStepsTextBox";
            this.TSStepsTextBox.Size = new System.Drawing.Size(100, 20);
            this.TSStepsTextBox.TabIndex = 3;
            this.TSStepsTextBox.Text = "10000";
            // 
            // TSDecTextBox
            // 
            this.TSDecTextBox.Location = new System.Drawing.Point(115, 41);
            this.TSDecTextBox.Name = "TSDecTextBox";
            this.TSDecTextBox.Size = new System.Drawing.Size(100, 20);
            this.TSDecTextBox.TabIndex = 2;
            this.TSDecTextBox.Text = "50";
            // 
            // TSAccTextBox
            // 
            this.TSAccTextBox.Location = new System.Drawing.Point(115, 15);
            this.TSAccTextBox.Name = "TSAccTextBox";
            this.TSAccTextBox.Size = new System.Drawing.Size(100, 20);
            this.TSAccTextBox.TabIndex = 1;
            this.TSAccTextBox.Text = "50";
            // 
            // TSInitButton
            // 
            this.TSInitButton.Location = new System.Drawing.Point(115, 114);
            this.TSInitButton.Name = "TSInitButton";
            this.TSInitButton.Size = new System.Drawing.Size(100, 23);
            this.TSInitButton.TabIndex = 0;
            this.TSInitButton.Text = "Initialize";
            this.TSInitButton.UseVisualStyleBackColor = true;
            this.TSInitButton.Click += new System.EventHandler(this.TSInitButton_Click);
            // 
            // TSClearButton
            // 
            this.TSClearButton.Location = new System.Drawing.Point(87, 17);
            this.TSClearButton.Name = "TSClearButton";
            this.TSClearButton.Size = new System.Drawing.Size(75, 23);
            this.TSClearButton.TabIndex = 8;
            this.TSClearButton.Text = "Clear";
            this.TSClearButton.UseVisualStyleBackColor = true;
            this.TSClearButton.Click += new System.EventHandler(this.TSClearButton_Click);
            // 
            // TSRestartButton
            // 
            this.TSRestartButton.Location = new System.Drawing.Point(6, 19);
            this.TSRestartButton.Name = "TSRestartButton";
            this.TSRestartButton.Size = new System.Drawing.Size(75, 23);
            this.TSRestartButton.TabIndex = 7;
            this.TSRestartButton.Text = "Restart";
            this.TSRestartButton.UseVisualStyleBackColor = true;
            this.TSRestartButton.Click += new System.EventHandler(this.TSRestartButton_Click);
            // 
            // TSReturnButton
            // 
            this.TSReturnButton.Location = new System.Drawing.Point(87, 19);
            this.TSReturnButton.Name = "TSReturnButton";
            this.TSReturnButton.Size = new System.Drawing.Size(75, 23);
            this.TSReturnButton.TabIndex = 6;
            this.TSReturnButton.Text = "Return";
            this.TSReturnButton.UseVisualStyleBackColor = true;
            this.TSReturnButton.Click += new System.EventHandler(this.TSReturnButton_Click);
            // 
            // read
            // 
            this.read.Location = new System.Drawing.Point(6, 17);
            this.read.Name = "read";
            this.read.Size = new System.Drawing.Size(75, 23);
            this.read.TabIndex = 5;
            this.read.Text = "Read";
            this.read.UseVisualStyleBackColor = true;
            this.read.Click += new System.EventHandler(this.read_Click);
            // 
            // disposeButton
            // 
            this.disposeButton.Location = new System.Drawing.Point(115, 22);
            this.disposeButton.Name = "disposeButton";
            this.disposeButton.Size = new System.Drawing.Size(100, 23);
            this.disposeButton.TabIndex = 4;
            this.disposeButton.Text = "Disconnect";
            this.disposeButton.UseVisualStyleBackColor = true;
            this.disposeButton.Click += new System.EventHandler(this.disposeButton_Click);
            // 
            // TSOffButton
            // 
            this.TSOffButton.Location = new System.Drawing.Point(87, 53);
            this.TSOffButton.Name = "TSOffButton";
            this.TSOffButton.Size = new System.Drawing.Size(75, 23);
            this.TSOffButton.TabIndex = 3;
            this.TSOffButton.Text = "Off";
            this.TSOffButton.UseVisualStyleBackColor = true;
            this.TSOffButton.Click += new System.EventHandler(this.TSOffButton_Click);
            // 
            // TSGoButton
            // 
            this.TSGoButton.Location = new System.Drawing.Point(6, 19);
            this.TSGoButton.Name = "TSGoButton";
            this.TSGoButton.Size = new System.Drawing.Size(75, 23);
            this.TSGoButton.TabIndex = 2;
            this.TSGoButton.Text = "Go";
            this.TSGoButton.UseVisualStyleBackColor = true;
            this.TSGoButton.Click += new System.EventHandler(this.TSGoButton_Click);
            // 
            // TSOnButton
            // 
            this.TSOnButton.Location = new System.Drawing.Point(87, 19);
            this.TSOnButton.Name = "TSOnButton";
            this.TSOnButton.Size = new System.Drawing.Size(75, 23);
            this.TSOnButton.TabIndex = 1;
            this.TSOnButton.Text = "On";
            this.TSOnButton.UseVisualStyleBackColor = true;
            this.TSOnButton.Click += new System.EventHandler(this.TSOnButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            // 
            // checkBox1
            // 
            this.checkBox1.Location = new System.Drawing.Point(0, 0);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(104, 24);
            this.checkBox1.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 0;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(0, 0);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 0;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.windowsToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(794, 24);
            this.menuStrip.TabIndex = 15;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadParametersToolStripMenuItem,
            this.saveParametersToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveImageToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadParametersToolStripMenuItem
            // 
            this.loadParametersToolStripMenuItem.Name = "loadParametersToolStripMenuItem";
            this.loadParametersToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.loadParametersToolStripMenuItem.Text = "Load parameters";
            this.loadParametersToolStripMenuItem.Click += new System.EventHandler(this.loadParametersToolStripMenuItem_Click);
            // 
            // saveParametersToolStripMenuItem
            // 
            this.saveParametersToolStripMenuItem.Name = "saveParametersToolStripMenuItem";
            this.saveParametersToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.saveParametersToolStripMenuItem.Text = "Save parameters on UI";
            this.saveParametersToolStripMenuItem.Click += new System.EventHandler(this.saveParametersToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(188, 6);
            // 
            // saveImageToolStripMenuItem
            // 
            this.saveImageToolStripMenuItem.Name = "saveImageToolStripMenuItem";
            this.saveImageToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.saveImageToolStripMenuItem.Text = "Save image";
            this.saveImageToolStripMenuItem.Click += new System.EventHandler(this.saveImageToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(188, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // windowsToolStripMenuItem
            // 
            this.windowsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hardwareMonitorToolStripMenuItem,
            this.openImageViewerToolStripMenuItem});
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.windowsToolStripMenuItem.Text = "Windows";
            // 
            // hardwareMonitorToolStripMenuItem
            // 
            this.hardwareMonitorToolStripMenuItem.Name = "hardwareMonitorToolStripMenuItem";
            this.hardwareMonitorToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.hardwareMonitorToolStripMenuItem.Text = "Open new hardware monitor";
            this.hardwareMonitorToolStripMenuItem.Click += new System.EventHandler(this.hardwareMonitorToolStripMenuItem_Click);
            // 
            // openImageViewerToolStripMenuItem
            // 
            this.openImageViewerToolStripMenuItem.Name = "openImageViewerToolStripMenuItem";
            this.openImageViewerToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
            this.openImageViewerToolStripMenuItem.Text = "Start camera and open image viewer";
            this.openImageViewerToolStripMenuItem.Click += new System.EventHandler(this.openImageViewerToolStripMenuItem_Click);
            // 
            // remoteControlLED
            // 
            this.remoteControlLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.remoteControlLED.Location = new System.Drawing.Point(761, 32);
            this.remoteControlLED.Name = "remoteControlLED";
            this.remoteControlLED.Size = new System.Drawing.Size(25, 24);
            this.remoteControlLED.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(675, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Remote Control";
            // 
            // updateHardwareButton
            // 
            this.updateHardwareButton.Location = new System.Drawing.Point(678, 62);
            this.updateHardwareButton.Name = "updateHardwareButton";
            this.updateHardwareButton.Size = new System.Drawing.Size(102, 23);
            this.updateHardwareButton.TabIndex = 21;
            this.updateHardwareButton.Text = "Update hardware";
            this.updateHardwareButton.UseVisualStyleBackColor = true;
            this.updateHardwareButton.Click += new System.EventHandler(this.updateHardwareButton_Click);
            // 
            // consoleRichTextBox
            // 
            this.consoleRichTextBox.BackColor = System.Drawing.Color.Black;
            this.consoleRichTextBox.ForeColor = System.Drawing.Color.Lime;
            this.consoleRichTextBox.Location = new System.Drawing.Point(3, 264);
            this.consoleRichTextBox.Name = "consoleRichTextBox";
            this.consoleRichTextBox.ReadOnly = true;
            this.consoleRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.consoleRichTextBox.Size = new System.Drawing.Size(791, 154);
            this.consoleRichTextBox.TabIndex = 23;
            this.consoleRichTextBox.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Acceleration";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Deceleration";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Distance to travel";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 98);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Velocity";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TSRestartButton);
            this.groupBox1.Controls.Add(this.TSOnButton);
            this.groupBox1.Controls.Add(this.TSOffButton);
            this.groupBox1.Location = new System.Drawing.Point(271, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(174, 87);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Enable";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TSGoButton);
            this.groupBox2.Controls.Add(this.TSReturnButton);
            this.groupBox2.Location = new System.Drawing.Point(271, 103);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(174, 54);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Motion";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.read);
            this.groupBox3.Controls.Add(this.TSClearButton);
            this.groupBox3.Location = new System.Drawing.Point(271, 160);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(174, 46);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Console";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(221, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "m/s2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(221, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "m/s2";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(221, 72);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(15, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "m";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(221, 96);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(25, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "m/s";
            // 
            // AutoTriggerCheckBox
            // 
            this.AutoTriggerCheckBox.AutoSize = true;
            this.AutoTriggerCheckBox.Checked = true;
            this.AutoTriggerCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoTriggerCheckBox.Location = new System.Drawing.Point(6, 22);
            this.AutoTriggerCheckBox.Name = "AutoTriggerCheckBox";
            this.AutoTriggerCheckBox.Size = new System.Drawing.Size(81, 17);
            this.AutoTriggerCheckBox.TabIndex = 12;
            this.AutoTriggerCheckBox.Text = "AutoTrigger";
            this.AutoTriggerCheckBox.UseVisualStyleBackColor = true;
            this.AutoTriggerCheckBox.CheckedChanged += new System.EventHandler(this.AutoTriggerCheckBox_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.AutoTriggerCheckBox);
            this.groupBox4.Location = new System.Drawing.Point(451, 10);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(200, 46);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Triggering";
            // 
            // TSConnectButton
            // 
            this.TSConnectButton.Location = new System.Drawing.Point(9, 22);
            this.TSConnectButton.Name = "TSConnectButton";
            this.TSConnectButton.Size = new System.Drawing.Size(100, 23);
            this.TSConnectButton.TabIndex = 16;
            this.TSConnectButton.Text = "Connect";
            this.TSConnectButton.UseVisualStyleBackColor = true;
            this.TSConnectButton.Click += new System.EventHandler(this.TSConnectButton_Click);
            // 
            // RS232GroupBox
            // 
            this.RS232GroupBox.Controls.Add(this.TSConnectButton);
            this.RS232GroupBox.Controls.Add(this.disposeButton);
            this.RS232GroupBox.Location = new System.Drawing.Point(5, 3);
            this.RS232GroupBox.Name = "RS232GroupBox";
            this.RS232GroupBox.Size = new System.Drawing.Size(257, 55);
            this.RS232GroupBox.TabIndex = 16;
            this.RS232GroupBox.TabStop = false;
            this.RS232GroupBox.Text = "RS232";
            // 
            // ControlWindow
            // 
            this.ClientSize = new System.Drawing.Size(794, 419);
            this.Controls.Add(this.consoleRichTextBox);
            this.Controls.Add(this.updateHardwareButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.remoteControlLED);
            this.Controls.Add(this.shcTabs);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "ControlWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sympathetic Hardware Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WindowClosing);
            this.Load += new System.EventHandler(this.WindowLoaded);
            this.shcTabs.ResumeLayout(false);
            this.tabCamera.ResumeLayout(false);
            this.tabLasers.ResumeLayout(false);
            this.aom3ControlBox.ResumeLayout(false);
            this.aom3ControlBox.PerformLayout();
            this.aom2ControlBox.ResumeLayout(false);
            this.aom2ControlBox.PerformLayout();
            this.aom1ControlBox.ResumeLayout(false);
            this.aom1ControlBox.PerformLayout();
            this.aom0ControlBox.ResumeLayout(false);
            this.aom0ControlBox.PerformLayout();
            this.tabCoils.ResumeLayout(false);
            this.coil1GroupBox.ResumeLayout(false);
            this.coil1GroupBox.PerformLayout();
            this.coil0GroupBox.ResumeLayout(false);
            this.coil0GroupBox.PerformLayout();
            this.tabTranslationStage.ResumeLayout(false);
            this.initParamsBox.ResumeLayout(false);
            this.initParamsBox.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.remoteControlLED)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.RS232GroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region ThreadSafe wrappers

        private void setCheckBox(CheckBox box, bool state)
        {
            box.Invoke(new setCheckDelegate(setCheckHelper), new object[] { box, state });
        }
        private delegate void setCheckDelegate(CheckBox box, bool state);
        private void setCheckHelper(CheckBox box, bool state)
        {
            box.Checked = state;
        }

        private void setTabEnable(TabControl box, bool state)
        {
            box.Invoke(new setTabEnableDelegate(setTabEnableHelper), new object[] { box, state });
        }
        private delegate void setTabEnableDelegate(TabControl box, bool state);
        private void setTabEnableHelper(TabControl box, bool state)
        {
            box.Enabled = state;
        }

        private void setTextBox(TextBox box, string text)
        {
            box.Invoke(new setTextDelegate(setTextHelper), new object[] { box, text });
        }
        private delegate void setTextDelegate(TextBox box, string text);
        private void setTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }
        
        private void setRichTextBox(RichTextBox box, string text)
        {
            box.Invoke(new setRichTextDelegate(setRichTextHelper), new object[] { box, text });
        }
        private delegate void setRichTextDelegate(RichTextBox box, string text);
        private void setRichTextHelper(RichTextBox box, string text)
        {
            box.AppendText(text);
            consoleRichTextBox.ScrollToCaret();
        }

        private void setLED(NationalInstruments.UI.WindowsForms.Led led, bool val)
        {
            led.Invoke(new SetLedDelegate(SetLedHelper), new object[] { led, val });
        }
        private delegate void SetLedDelegate(NationalInstruments.UI.WindowsForms.Led led, bool val);
        private void SetLedHelper(NationalInstruments.UI.WindowsForms.Led led, bool val)
        {
            led.Value = val;
        }

        #endregion

        #region Declarations
        //Declare stuff here
        public TabControl shcTabs;
        public TabPage tabCamera;
        public TabPage tabLasers;
        public TabPage tabCoils;
        private GroupBox aom0ControlBox;
        public CheckBox aom0CheckBox;
        public TextBox aom0rfFrequencyTextBox;
        public TextBox aom0rfAmplitudeTextBox;
        private Label aom0Label2;
        private Label aom0Label0;
        private Label aom0Label1;
        private Label aom0Label3;
        private GroupBox aom3ControlBox;
        private Label aom3Label3;
        private Label aom3Label1;
        public CheckBox aom3CheckBox;
        public TextBox aom3rfFrequencyTextBox;
        public TextBox aom3rfAmplitudeTextBox;
        private Label aom3Label2;
        private Label aom3Label0;
        private GroupBox aom2ControlBox;
        private Label aom2Label3;
        private Label aom2Label1;
        public CheckBox aom2CheckBox;
        public TextBox aom2rfFrequencyTextBox;
        public TextBox aom2rfAmplitudeTextBox;
        private Label aom2Label2;
        private Label aom2Label0;
        private GroupBox aom1ControlBox;
        private Label aom1Label3;
        private Label aom1Label1;
        public CheckBox aom1CheckBox;
        public TextBox aom1rfFrequencyTextBox;
        public TextBox aom1rfAmplitudeTextBox;
        private Label aom1Label2;
        private Label aom1Label0;
        private GroupBox coil1GroupBox;
        private Label coil1Label1;
        public TextBox coil1CurrentTextBox;
        private Label coil1Label0;
        private GroupBox coil0GroupBox;
        private Label coil0Label1;
        public TextBox coil0CurrentTextBox;
        private Label coil0Label0;
        private Button button1;
        public CheckBox checkBox1;
        public TextBox textBox1;
        public TextBox textBox2;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loadParametersToolStripMenuItem;
        private ToolStripMenuItem saveParametersToolStripMenuItem;
        private ToolStripMenuItem saveImageToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private Button snapshotButton;
        private Led remoteControlLED;
        private Label label1;
        private Button updateHardwareButton;
        private ToolStripMenuItem windowsToolStripMenuItem;
        private ToolStripMenuItem hardwareMonitorToolStripMenuItem;
        private RichTextBox consoleRichTextBox;
        private ToolStripMenuItem openImageViewerToolStripMenuItem;
        private Button streamButton;
        private Button stopStreamButton;
        private TabPage tabTranslationStage;
        private Button TSOnButton;
        private Button TSInitButton;
        private Button TSOffButton;
        private Button TSGoButton;
        private Button disposeButton;
        private Button read;
        private Button TSReturnButton;
        private Button TSClearButton;
        private Button TSRestartButton;
        private GroupBox initParamsBox;
        private TextBox TSVelTextBox;
        private TextBox TSStepsTextBox;
        private TextBox TSDecTextBox;
        private TextBox TSAccTextBox;
        private GroupBox groupBox3;
        private GroupBox groupBox2;
        private GroupBox groupBox1;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private GroupBox groupBox4;
        private CheckBox AutoTriggerCheckBox;
        private Button TSConnectButton;

        #endregion

        #region Click Handlers

        private void saveParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveParametersWithDialog();
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveImageWithDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void updateHardwareButton_Click(object sender, EventArgs e)
        {
            controller.UpdateHardware();
        }
        private void loadParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadParametersWithDialog();
        }
        private void TSInitButton_Click(object sender, EventArgs e)
        {
            controller.TSInitialize(double.Parse(TSAccTextBox.Text), double.Parse(TSDecTextBox.Text),
                double.Parse(TSStepsTextBox.Text), double.Parse(TSVelTextBox.Text));
        }

        private void TSOnButton_Click(object sender, EventArgs e)
        {

            controller.TSOn();
        }

        private void TSGoButton_Click(object sender, EventArgs e)
        {
            controller.TSGo();
        }

        private void TSOffButton_Click(object sender, EventArgs e)
        {
            controller.TSOff();
        }

        private void read_Click(object sender, EventArgs e)
        {
            controller.TSRead();
        }

        private void TSReturnButton_Click(object sender, EventArgs e)
        {
            controller.TSReturn();
        }



        private void TSRestartButton_Click(object sender, EventArgs e)
        {
            controller.TSRestart();
        }

        private void TSClearButton_Click(object sender, EventArgs e)
        {
            controller.TSClear();
        }
        private void disposeButton_Click(object sender, EventArgs e)
        {
            controller.TSDisconnect();
        }

        private void TSConnectButton_Click(object sender, EventArgs e)
        {
            controller.TSConnect();
        }
        private void AutoTriggerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoTriggerCheckBox.Checked)
            {
                controller.TSAutoTriggerEnable();
            }
            if (!AutoTriggerCheckBox.Checked)
            {
                controller.TSAutoTriggerDisable();
            }
        }
        #endregion

        #region Public properties for controlling UI.
        //This gets/sets the values on the GUI panel
        public void WriteToConsole(string text)
        {
            setRichTextBox(consoleRichTextBox, ">> " + text + "\n");
            
           
        }
        public double ReadAnalog(string channelName)
        {
            return double.Parse(AOTextBoxes[channelName].Text);
        }
        public void SetAnalog(string channelName, double value)
        {
            setTextBox(AOTextBoxes[channelName], Convert.ToString(value));
        }
        public bool ReadDigital(string channelName)
        {
            return DOCheckBoxes[channelName].Checked;
        }
        public void SetDigital(string channelName, bool value)
        {
            setCheckBox(DOCheckBoxes[channelName], value);
        }
        #endregion

        #region Camera Control

        private void snapshotButton_Click(object sender, EventArgs e)
        {
            controller.CameraSnapshot();         
        }
        private void streamButton_Click(object sender, EventArgs e)
        {
            controller.CameraStream();
        }
        private void stopStreamButton_Click(object sender, EventArgs e)
        {
            controller.StopCameraStream();
        }

        #endregion

        #region UI state
        
        public void UpdateUIState(Controller.SHCUIControlState state)
        {
            switch (state)
            {
                case Controller.SHCUIControlState.OFF:

                    setLED(remoteControlLED, false);
                    setTabEnable(shcTabs, true);

                    break;

                case Controller.SHCUIControlState.LOCAL:

                    setLED(remoteControlLED, false);
                    setTabEnable(shcTabs, true);
                    break;

                case Controller.SHCUIControlState.REMOTE:

                    setLED(remoteControlLED, true);
                    setTabEnable(shcTabs, false) ;

                    break;
            }
        }

       
        #endregion

        #region Other Windows


        private void hardwareMonitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.OpenNewHardwareMonitorWindow();
        }
        
        private void openImageViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StartCameraControl();
        }
        #endregion

        private GroupBox RS232GroupBox;


        

    



















    }
}
