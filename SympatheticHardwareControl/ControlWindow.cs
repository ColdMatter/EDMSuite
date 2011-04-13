using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;


using NationalInstruments.UI.WindowsForms;
using NationalInstruments.UI;

using NationalInstruments.Vision;


namespace SympatheticHardwareControl
{
    /// <summary>
    /// Front panel for the edm hardware controller. Everything is just stuffed in there. No particularly
    /// clever structure. This class just hands everything straight off to the controller. It has a few
    /// thread safe wrappers so that remote calls can safely manipulate the front panel.
    /// </summary>
    public class ControlWindow : System.Windows.Forms.Form
    {
        #region Setup

        public Controller controller;

        public ControlWindow()
        {
            InitializeComponent();
        }

        private void WindowClosing(object sender, FormClosingEventArgs e)
        {
            controller.WindowClosing();
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            controller.WindowLoaded();
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
            this.updateAttributesButton = new System.Windows.Forms.Button();
            this.saveImageCheckBox = new System.Windows.Forms.CheckBox();
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
            this.manualControlLED = new NationalInstruments.UI.WindowsForms.Led();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLastParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.loadImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manualControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.onToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteControlLED = new NationalInstruments.UI.WindowsForms.Led();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.usingLastSavedValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usingValuesCurrentlyOnPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            ((System.ComponentModel.ISupportInitialize)(this.manualControlLED)).BeginInit();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.remoteControlLED)).BeginInit();
            this.SuspendLayout();
            // 
            // shcTabs
            // 
            this.shcTabs.AllowDrop = true;
            this.shcTabs.Controls.Add(this.tabCamera);
            this.shcTabs.Controls.Add(this.tabLasers);
            this.shcTabs.Controls.Add(this.tabCoils);
            this.shcTabs.Location = new System.Drawing.Point(3, 27);
            this.shcTabs.Name = "shcTabs";
            this.shcTabs.SelectedIndex = 0;
            this.shcTabs.Size = new System.Drawing.Size(666, 235);
            this.shcTabs.TabIndex = 0;
            // 
            // tabCamera
            // 
            this.tabCamera.Controls.Add(this.updateAttributesButton);
            this.tabCamera.Controls.Add(this.saveImageCheckBox);
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
            // updateAttributesButton
            // 
            this.updateAttributesButton.Location = new System.Drawing.Point(404, 14);
            this.updateAttributesButton.Name = "updateAttributesButton";
            this.updateAttributesButton.Size = new System.Drawing.Size(107, 23);
            this.updateAttributesButton.TabIndex = 20;
            this.updateAttributesButton.Text = "Update Attributes";
            this.updateAttributesButton.UseVisualStyleBackColor = true;
            this.updateAttributesButton.Click += new System.EventHandler(this.updateAttributesButton_Click);
            // 
            // saveImageCheckBox
            // 
            this.saveImageCheckBox.AutoSize = true;
            this.saveImageCheckBox.Location = new System.Drawing.Point(16, 18);
            this.saveImageCheckBox.Name = "saveImageCheckBox";
            this.saveImageCheckBox.Size = new System.Drawing.Size(99, 17);
            this.saveImageCheckBox.TabIndex = 19;
            this.saveImageCheckBox.Text = "Save Snapshot";
            this.saveImageCheckBox.UseVisualStyleBackColor = true;
            // 
            // stopStreamButton
            // 
            this.stopStreamButton.Enabled = false;
            this.stopStreamButton.Location = new System.Drawing.Point(287, 14);
            this.stopStreamButton.Name = "stopStreamButton";
            this.stopStreamButton.Size = new System.Drawing.Size(75, 23);
            this.stopStreamButton.TabIndex = 18;
            this.stopStreamButton.Text = "Stop";
            this.stopStreamButton.UseVisualStyleBackColor = true;
            this.stopStreamButton.Click += new System.EventHandler(this.stopStreamButton_Click);
            // 
            // streamButton
            // 
            this.streamButton.Location = new System.Drawing.Point(206, 14);
            this.streamButton.Name = "streamButton";
            this.streamButton.Size = new System.Drawing.Size(75, 23);
            this.streamButton.TabIndex = 17;
            this.streamButton.Text = "Stream";
            this.streamButton.UseVisualStyleBackColor = true;
            this.streamButton.Click += new System.EventHandler(this.streamButton_Click);
            // 
            // snapshotButton
            // 
            this.snapshotButton.Location = new System.Drawing.Point(125, 14);
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
            this.aom3CheckBox.CheckedChanged += new System.EventHandler(this.aom3CheckBox_CheckedChanged);
            // 
            // aom3rfFrequencyTextBox
            // 
            this.aom3rfFrequencyTextBox.Location = new System.Drawing.Point(155, 17);
            this.aom3rfFrequencyTextBox.Name = "aom3rfFrequencyTextBox";
            this.aom3rfFrequencyTextBox.Size = new System.Drawing.Size(103, 20);
            this.aom3rfFrequencyTextBox.TabIndex = 0;
            this.aom3rfFrequencyTextBox.Text = "0";
            this.aom3rfFrequencyTextBox.TextChanged += new System.EventHandler(this.aom3rfFrequencyTextBox_TextChanged);
            // 
            // aom3rfAmplitudeTextBox
            // 
            this.aom3rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom3rfAmplitudeTextBox.Name = "aom3rfAmplitudeTextBox";
            this.aom3rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom3rfAmplitudeTextBox.TabIndex = 8;
            this.aom3rfAmplitudeTextBox.Text = "0";
            this.aom3rfAmplitudeTextBox.TextChanged += new System.EventHandler(this.aom3rfAmplitudeTextBox_TextChanged);
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
            this.aom2CheckBox.CheckedChanged += new System.EventHandler(this.aom2CheckBox_CheckedChanged);
            // 
            // aom2rfFrequencyTextBox
            // 
            this.aom2rfFrequencyTextBox.Location = new System.Drawing.Point(155, 17);
            this.aom2rfFrequencyTextBox.Name = "aom2rfFrequencyTextBox";
            this.aom2rfFrequencyTextBox.Size = new System.Drawing.Size(103, 20);
            this.aom2rfFrequencyTextBox.TabIndex = 0;
            this.aom2rfFrequencyTextBox.Text = "0";
            this.aom2rfFrequencyTextBox.TextChanged += new System.EventHandler(this.aom2rfFrequencyTextBox_TextChanged);
            // 
            // aom2rfAmplitudeTextBox
            // 
            this.aom2rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom2rfAmplitudeTextBox.Name = "aom2rfAmplitudeTextBox";
            this.aom2rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom2rfAmplitudeTextBox.TabIndex = 8;
            this.aom2rfAmplitudeTextBox.Text = "0";
            this.aom2rfAmplitudeTextBox.TextChanged += new System.EventHandler(this.aom2rfAmplitudeTextBox_TextChanged);
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
            this.aom1CheckBox.CheckedChanged += new System.EventHandler(this.aom1CheckBox_CheckedChanged);
            // 
            // aom1rfFrequencyTextBox
            // 
            this.aom1rfFrequencyTextBox.Location = new System.Drawing.Point(155, 17);
            this.aom1rfFrequencyTextBox.Name = "aom1rfFrequencyTextBox";
            this.aom1rfFrequencyTextBox.Size = new System.Drawing.Size(103, 20);
            this.aom1rfFrequencyTextBox.TabIndex = 0;
            this.aom1rfFrequencyTextBox.Text = "0";
            this.aom1rfFrequencyTextBox.TextChanged += new System.EventHandler(this.aom1rfFrequencyTextBox_TextChanged);
            // 
            // aom1rfAmplitudeTextBox
            // 
            this.aom1rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom1rfAmplitudeTextBox.Name = "aom1rfAmplitudeTextBox";
            this.aom1rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom1rfAmplitudeTextBox.TabIndex = 8;
            this.aom1rfAmplitudeTextBox.Text = "0";
            this.aom1rfAmplitudeTextBox.TextChanged += new System.EventHandler(this.aom1rfAmplitudeTextBox_TextChanged);
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
            this.aom0CheckBox.CheckedChanged += new System.EventHandler(this.aom0CheckBox_CheckedChanged);
            // 
            // aom0rfFrequencyTextBox
            // 
            this.aom0rfFrequencyTextBox.Location = new System.Drawing.Point(155, 17);
            this.aom0rfFrequencyTextBox.Name = "aom0rfFrequencyTextBox";
            this.aom0rfFrequencyTextBox.Size = new System.Drawing.Size(103, 20);
            this.aom0rfFrequencyTextBox.TabIndex = 0;
            this.aom0rfFrequencyTextBox.Text = "0";
            this.aom0rfFrequencyTextBox.TextChanged += new System.EventHandler(this.aom0rfFrequencyTextBox_TextChanged);
            // 
            // aom0rfAmplitudeTextBox
            // 
            this.aom0rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom0rfAmplitudeTextBox.Name = "aom0rfAmplitudeTextBox";
            this.aom0rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom0rfAmplitudeTextBox.TabIndex = 8;
            this.aom0rfAmplitudeTextBox.Text = "0";
            this.aom0rfAmplitudeTextBox.TextChanged += new System.EventHandler(this.aom0rfAmplitudeTextBox_TextChanged);
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
            this.coil1GroupBox.Text = "lower MOT Coil";
            this.coil1GroupBox.Enter += new System.EventHandler(this.coil1GroupBox_Enter);
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
            this.coil0GroupBox.Text = "Upper MOT Coil";
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
            this.coil0CurrentTextBox.TextChanged += new System.EventHandler(this.coil0CurrentTextBox_TextChanged);
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
            // manualControlLED
            // 
            this.manualControlLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.manualControlLED.Location = new System.Drawing.Point(678, 69);
            this.manualControlLED.Name = "manualControlLED";
            this.manualControlLED.Size = new System.Drawing.Size(80, 80);
            this.manualControlLED.TabIndex = 14;
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
            this.manualControlToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(797, 24);
            this.menuStrip.TabIndex = 15;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadLastParametersToolStripMenuItem,
            this.loadParametersToolStripMenuItem,
            this.saveParametersToolStripMenuItem,
            this.toolStripSeparator1,
            this.loadImageToolStripMenuItem,
            this.saveImageToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadLastParametersToolStripMenuItem
            // 
            this.loadLastParametersToolStripMenuItem.Name = "loadLastParametersToolStripMenuItem";
            this.loadLastParametersToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.loadLastParametersToolStripMenuItem.Text = "Load last parameter set";
            this.loadLastParametersToolStripMenuItem.Click += new System.EventHandler(this.loadLastParametersToolStripMenuItem_Click);
            // 
            // loadParametersToolStripMenuItem
            // 
            this.loadParametersToolStripMenuItem.Name = "loadParametersToolStripMenuItem";
            this.loadParametersToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.loadParametersToolStripMenuItem.Text = "Load parameters";
            this.loadParametersToolStripMenuItem.Click += new System.EventHandler(this.loadParametersToolStripMenuItem_Click);
            // 
            // saveParametersToolStripMenuItem
            // 
            this.saveParametersToolStripMenuItem.Name = "saveParametersToolStripMenuItem";
            this.saveParametersToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.saveParametersToolStripMenuItem.Text = "Save parameters";
            this.saveParametersToolStripMenuItem.Click += new System.EventHandler(this.saveParametersToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(193, 6);
            // 
            // loadImageToolStripMenuItem
            // 
            this.loadImageToolStripMenuItem.Name = "loadImageToolStripMenuItem";
            this.loadImageToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.loadImageToolStripMenuItem.Text = "Load image";
            this.loadImageToolStripMenuItem.Click += new System.EventHandler(this.loadImageToolStripMenuItem_Click);
            // 
            // saveImageToolStripMenuItem
            // 
            this.saveImageToolStripMenuItem.Name = "saveImageToolStripMenuItem";
            this.saveImageToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.saveImageToolStripMenuItem.Text = "Save image";
            this.saveImageToolStripMenuItem.Click += new System.EventHandler(this.saveImageToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(193, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // manualControlToolStripMenuItem
            // 
            this.manualControlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.onToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.manualControlToolStripMenuItem.Name = "manualControlToolStripMenuItem";
            this.manualControlToolStripMenuItem.Size = new System.Drawing.Size(102, 20);
            this.manualControlToolStripMenuItem.Text = "Manual Control";
            // 
            // onToolStripMenuItem
            // 
            this.onToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.usingLastSavedValuesToolStripMenuItem,
            this.usingValuesCurrentlyOnPanelToolStripMenuItem});
            this.onToolStripMenuItem.Name = "onToolStripMenuItem";
            this.onToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.onToolStripMenuItem.Text = "Start";
            this.onToolStripMenuItem.Click += new System.EventHandler(this.onToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // remoteControlLED
            // 
            this.remoteControlLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.remoteControlLED.Location = new System.Drawing.Point(678, 182);
            this.remoteControlLED.Name = "remoteControlLED";
            this.remoteControlLED.OffColor = System.Drawing.Color.Maroon;
            this.remoteControlLED.OnColor = System.Drawing.Color.Red;
            this.remoteControlLED.Size = new System.Drawing.Size(80, 76);
            this.remoteControlLED.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(675, 166);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Remote Control";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(675, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Local Control";
            // 
            // usingLastSavedValuesToolStripMenuItem
            // 
            this.usingLastSavedValuesToolStripMenuItem.Name = "usingLastSavedValuesToolStripMenuItem";
            this.usingLastSavedValuesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.usingLastSavedValuesToolStripMenuItem.Text = "using last saved values";
            this.usingLastSavedValuesToolStripMenuItem.Click += new System.EventHandler(this.usingLastSavedValuesToolStripMenuItem_Click);
            // 
            // usingValuesCurrentlyOnPanelToolStripMenuItem
            // 
            this.usingValuesCurrentlyOnPanelToolStripMenuItem.Name = "usingValuesCurrentlyOnPanelToolStripMenuItem";
            this.usingValuesCurrentlyOnPanelToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.usingValuesCurrentlyOnPanelToolStripMenuItem.Text = "using values currently on panel";
            this.usingValuesCurrentlyOnPanelToolStripMenuItem.Click += new System.EventHandler(this.usingValuesCurrentlyOnPanelToolStripMenuItem_Click);
            // 
            // ControlWindow
            // 
            this.ClientSize = new System.Drawing.Size(797, 264);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.remoteControlLED);
            this.Controls.Add(this.shcTabs);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.manualControlLED);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "ControlWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sympathetic Hardware Control";
            this.Load += new System.EventHandler(this.WindowLoaded);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WindowClosing);
            this.shcTabs.ResumeLayout(false);
            this.tabCamera.ResumeLayout(false);
            this.tabCamera.PerformLayout();
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
            ((System.ComponentModel.ISupportInitialize)(this.manualControlLED)).EndInit();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.remoteControlLED)).EndInit();
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

        private void setTextBox(TextBox box, string text)
        {
            box.Invoke(new setTextDelegate(setTextHelper), new object[] { box, text });
        }
        private delegate void setTextDelegate(TextBox box, string text);
        private void setTextHelper(TextBox box, string text)
        {
            box.Text = text;
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
        public Led manualControlLED;
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
        public CheckBox saveImageCheckBox;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loadParametersToolStripMenuItem;
        private ToolStripMenuItem saveParametersToolStripMenuItem;
        private ToolStripMenuItem loadImageToolStripMenuItem;
        private ToolStripMenuItem saveImageToolStripMenuItem;
        private ToolStripMenuItem manualControlToolStripMenuItem;
        private ToolStripMenuItem onToolStripMenuItem;
        private ToolStripMenuItem stopToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        public Led remoteControlLED;
        private Label label1;
        private Label label2;
        private Button snapshotButton;
        private Button updateAttributesButton;
        private ToolStripMenuItem loadLastParametersToolStripMenuItem;
        private ToolStripMenuItem usingLastSavedValuesToolStripMenuItem;
        private ToolStripMenuItem usingValuesCurrentlyOnPanelToolStripMenuItem;

        #endregion

        #region Click Handlers

        private void loadLastParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadLastSavedParameterValues();
        }

        private void aom0CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom0EnabledState();
        }

        private void aom1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom1EnabledState();
        }
        private void aom2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom2EnabledState();
        }
        private void aom3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom3EnabledState();
        }


        //

        private void aom0rfFrequencyTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom0rfFrequency();
        }

        private void aom0rfAmplitudeTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom0rfAmplitude();
        }

        private void aom1rfFrequencyTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom1rfFrequency();
        }

        private void aom1rfAmplitudeTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom1rfAmplitude();
        }

        private void aom2rfFrequencyTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom2rfFrequency();
        }

        private void aom2rfAmplitudeTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom2rfAmplitude();
        }

        private void aom3rfFrequencyTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom3rfFrequency();
        }

        private void aom3rfAmplitudeTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUIAom3rfAmplitude();
        }

        private void coil0CurrentTextBox_TextChanged(object sender, EventArgs e)
        {
            controller.ReadAndApplyUICoil0Current();
        }

        private void coil1GroupBox_Enter(object sender, EventArgs e)
        {
            controller.ReadAndApplyUICoil1Current();
        }


        private void loadParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controller.HCState == Controller.SHCUIControlState.OFF)
            {
                controller.LoadParametersWithDialog();
            }
        }

        private void saveParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveParametersWithDialog();
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveImageWithDialog();
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadImagesWithDialog();
        }

       

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void updateAttributesButton_Click(object sender, EventArgs e)
        {
            controller.SetCameraAttributes();
        }

        private void onToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StartManualControl();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StopManualControl();
        }

        private void usingLastSavedValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StartManualControlUsingLastSavedValues();
        }

        private void usingValuesCurrentlyOnPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StartManualControl();
        }
        #endregion

        #region Public properties for controlling UI.
        //This gets/sets the values on the GUI panel

        public bool ReadAom0EnabledState()
        {
            return aom0CheckBox.Checked;
        }
        public void SetAom0EnabledState(bool value)
        {
            setCheckBox(aom0CheckBox, value);
        }

        public double ReadAom0rfAmplitude()
        {
            return Double.Parse(aom0rfAmplitudeTextBox.Text);
        }
        public void SetAom0rfAmplitude(double value)
        {
            setTextBox(aom0rfAmplitudeTextBox, Convert.ToString(value));
        }

        public double ReadAom0rfFrequency()
        {
            return Double.Parse(aom0rfFrequencyTextBox.Text); ;
        }
        public void SetAom0rfFrequency(double value)
        {
            setTextBox(aom0rfFrequencyTextBox, Convert.ToString(value));
        }

        ///

        public bool ReadAom1EnabledState()
        {
            return aom1CheckBox.Checked;
        }
        public void SetAom1EnabledState(bool value)
        {
            setCheckBox(aom1CheckBox, value);
        }

        public double ReadAom1rfAmplitude()
        {
            return Double.Parse(aom1rfAmplitudeTextBox.Text);
        }
        public void SetAom1rfAmplitude(double value)
        {
            setTextBox(aom1rfAmplitudeTextBox, Convert.ToString(value));
        }

        public double ReadAom1rfFrequency()
        {
            return Double.Parse(aom1rfFrequencyTextBox.Text);
        }
        public void SetAom1rfFrequency(double value)
        {
            setTextBox(aom1rfFrequencyTextBox, Convert.ToString(value));
        }

        ///

        public bool ReadAom2EnabledState()
        {
            return aom2CheckBox.Checked;
        }
        public void SetAom2EnabledState(bool value)
        {
            setCheckBox(aom2CheckBox, value);
        }

        public double ReadAom2rfAmplitude()
        {
            return Double.Parse(aom2rfAmplitudeTextBox.Text); ;
        }
        public void SetAom2rfAmplitude(double value)
        {
            setTextBox(aom2rfAmplitudeTextBox, Convert.ToString(value));
        }

        public double ReadAom2rfFrequency()
        {
            return Double.Parse(aom2rfFrequencyTextBox.Text);
        }
        public void SetAom2rfFrequency(double value)
        {
            setTextBox(aom2rfFrequencyTextBox, Convert.ToString(value));
        }

        ///

        public bool ReadAom3EnabledState()
        {
            return aom3CheckBox.Checked; ;
        }
        public void SetAom3EnabledState(bool value)
        {
            setCheckBox(aom3CheckBox, value);
        }

        public double ReadAom3rfAmplitude()
        {
            return Double.Parse(aom3rfAmplitudeTextBox.Text);
        }
        public void SetAom3rfAmplitude(double value)
        {
            setTextBox(aom3rfAmplitudeTextBox, Convert.ToString(value));
        }

        public double ReadAom3rfFrequency()
        {
            return Double.Parse(aom3rfFrequencyTextBox.Text); ;
        }
        public void SetAom3rfFrequency(double value)
        {
            setTextBox(aom3rfFrequencyTextBox, Convert.ToString(value));
        }

        public double ReadCoil0Current()
        {
            return Double.Parse(coil0CurrentTextBox.Text); ;
        }
        public void SetCoil0Current(double value)
        {
            setTextBox(coil0CurrentTextBox, Convert.ToString(value));
        }

        public double ReadCoil1Current()
        {
            return Double.Parse(coil0CurrentTextBox.Text);
        }
        public void SetCoil1Current(double value)
        {
            setTextBox(coil1CurrentTextBox, Convert.ToString(value));
        }

        #endregion

        #region Camera Control
        

        private void snapshotButton_Click(object sender, EventArgs e)
        {
            controller.CameraSnapshot();
            //controller.ManualCameraSnapshot();
        }

        private Button streamButton;
        private Button stopStreamButton;

        private void streamButton_Click(object sender, EventArgs e)
        {
            this.snapshotButton.Enabled = false;
            this.streamButton.Enabled = false;
            this.stopStreamButton.Enabled = true;

            controller.CameraStream();
        }

        private void stopStreamButton_Click(object sender, EventArgs e)
        {

            this.snapshotButton.Enabled = true;
            this.streamButton.Enabled = true;
            this.stopStreamButton.Enabled = false;
        }


        #endregion

        #region UI state
        
        public void UpdateUIState(Controller.SHCUIControlState state)
        {
            switch (state)
            {
                case Controller.SHCUIControlState.OFF:
                    loadParametersToolStripMenuItem.Enabled = true;
                    setLED(manualControlLED, false);
                    setLED(remoteControlLED, false);
                    break;

                case Controller.SHCUIControlState.LOCAL:
                    loadParametersToolStripMenuItem.Enabled = false;
                    setLED(manualControlLED, true);
                    break;

                case Controller.SHCUIControlState.REMOTE:
                    loadParametersToolStripMenuItem.Enabled = false;
                    setLED(remoteControlLED, true);
                    break;
            }
        }



        #endregion

        



      

        


    }
}
