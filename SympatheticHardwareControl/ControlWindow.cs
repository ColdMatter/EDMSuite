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

        /*protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }*/

        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.shcTabs = new System.Windows.Forms.TabControl();
            this.tabOverview = new System.Windows.Forms.TabPage();
            this.updateAttributesButton = new System.Windows.Forms.Button();
            this.saveImageCheckBox = new System.Windows.Forms.CheckBox();
            this.manualControlLED = new NationalInstruments.UI.WindowsForms.Led();
            this.stopStreamButton = new System.Windows.Forms.Button();
            this.streamButton = new System.Windows.Forms.Button();
            this.motViewer = new NationalInstruments.Vision.WindowsForms.ImageViewer();
            this.snapshotButton = new System.Windows.Forms.Button();
            this.tabLasers = new System.Windows.Forms.TabPage();
            this.aom3ControlBox = new System.Windows.Forms.GroupBox();
            this.aom3UpdateButton = new System.Windows.Forms.Button();
            this.aom3Label3 = new System.Windows.Forms.Label();
            this.aom3Label1 = new System.Windows.Forms.Label();
            this.aom3CheckBox = new System.Windows.Forms.CheckBox();
            this.aom3rfFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.aom3rfAmplitudeTextBox = new System.Windows.Forms.TextBox();
            this.aom3LED = new NationalInstruments.UI.WindowsForms.Led();
            this.aom3Label2 = new System.Windows.Forms.Label();
            this.aom3Label0 = new System.Windows.Forms.Label();
            this.aom2ControlBox = new System.Windows.Forms.GroupBox();
            this.aom2UpdateButton = new System.Windows.Forms.Button();
            this.aom2Label3 = new System.Windows.Forms.Label();
            this.aom2Label1 = new System.Windows.Forms.Label();
            this.aom2CheckBox = new System.Windows.Forms.CheckBox();
            this.aom2rfFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.aom2rfAmplitudeTextBox = new System.Windows.Forms.TextBox();
            this.aom2LED = new NationalInstruments.UI.WindowsForms.Led();
            this.aom2Label2 = new System.Windows.Forms.Label();
            this.aom2Label0 = new System.Windows.Forms.Label();
            this.aom1ControlBox = new System.Windows.Forms.GroupBox();
            this.aom1UpdateButton = new System.Windows.Forms.Button();
            this.aom1Label3 = new System.Windows.Forms.Label();
            this.aom1Label1 = new System.Windows.Forms.Label();
            this.aom1CheckBox = new System.Windows.Forms.CheckBox();
            this.aom1rfFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.aom1rfAmplitudeTextBox = new System.Windows.Forms.TextBox();
            this.aom1LED = new NationalInstruments.UI.WindowsForms.Led();
            this.aom1Label2 = new System.Windows.Forms.Label();
            this.aom1Label0 = new System.Windows.Forms.Label();
            this.aom0ControlBox = new System.Windows.Forms.GroupBox();
            this.aom0UpdateButton = new System.Windows.Forms.Button();
            this.aom0Label3 = new System.Windows.Forms.Label();
            this.aom0Label1 = new System.Windows.Forms.Label();
            this.aom0CheckBox = new System.Windows.Forms.CheckBox();
            this.aom0rfFrequencyTextBox = new System.Windows.Forms.TextBox();
            this.aom0rfAmplitudeTextBox = new System.Windows.Forms.TextBox();
            this.aom0LED = new NationalInstruments.UI.WindowsForms.Led();
            this.aom0Label2 = new System.Windows.Forms.Label();
            this.aom0Label0 = new System.Windows.Forms.Label();
            this.tabCoils = new System.Windows.Forms.TabPage();
            this.coil1GroupBox = new System.Windows.Forms.GroupBox();
            this.coil1UpdateButton = new System.Windows.Forms.Button();
            this.coil1Label1 = new System.Windows.Forms.Label();
            this.coil1CurrentTextBox = new System.Windows.Forms.TextBox();
            this.coil1Label0 = new System.Windows.Forms.Label();
            this.coil0GroupBox = new System.Windows.Forms.GroupBox();
            this.coil0UpdateButton = new System.Windows.Forms.Button();
            this.coil0Label1 = new System.Windows.Forms.Label();
            this.coil0CurrentTextBox = new System.Windows.Forms.TextBox();
            this.coil0Label0 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.shcTabs.SuspendLayout();
            this.tabOverview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.manualControlLED)).BeginInit();
            this.tabLasers.SuspendLayout();
            this.aom3ControlBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aom3LED)).BeginInit();
            this.aom2ControlBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aom2LED)).BeginInit();
            this.aom1ControlBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aom1LED)).BeginInit();
            this.aom0ControlBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aom0LED)).BeginInit();
            this.tabCoils.SuspendLayout();
            this.coil1GroupBox.SuspendLayout();
            this.coil0GroupBox.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // shcTabs
            // 
            this.shcTabs.AllowDrop = true;
            this.shcTabs.Controls.Add(this.tabOverview);
            this.shcTabs.Controls.Add(this.tabLasers);
            this.shcTabs.Controls.Add(this.tabCoils);
            this.shcTabs.Location = new System.Drawing.Point(3, 27);
            this.shcTabs.Name = "shcTabs";
            this.shcTabs.SelectedIndex = 0;
            this.shcTabs.Size = new System.Drawing.Size(666, 568);
            this.shcTabs.TabIndex = 0;
            // 
            // tabOverview
            // 
            this.tabOverview.Controls.Add(this.updateAttributesButton);
            this.tabOverview.Controls.Add(this.saveImageCheckBox);
            this.tabOverview.Controls.Add(this.stopStreamButton);
            this.tabOverview.Controls.Add(this.streamButton);
            this.tabOverview.Controls.Add(this.motViewer);
            this.tabOverview.Controls.Add(this.snapshotButton);
            this.tabOverview.Location = new System.Drawing.Point(4, 22);
            this.tabOverview.Name = "tabOverview";
            this.tabOverview.Padding = new System.Windows.Forms.Padding(3);
            this.tabOverview.Size = new System.Drawing.Size(658, 542);
            this.tabOverview.TabIndex = 0;
            this.tabOverview.Text = "Overview";
            this.tabOverview.UseVisualStyleBackColor = true;
            // 
            // updateAttributesButton
            // 
            this.updateAttributesButton.Location = new System.Drawing.Point(395, 505);
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
            this.saveImageCheckBox.Location = new System.Drawing.Point(7, 509);
            this.saveImageCheckBox.Name = "saveImageCheckBox";
            this.saveImageCheckBox.Size = new System.Drawing.Size(99, 17);
            this.saveImageCheckBox.TabIndex = 19;
            this.saveImageCheckBox.Text = "Save Snapshot";
            this.saveImageCheckBox.UseVisualStyleBackColor = true;
            // 
            // manualControlLED
            // 
            this.manualControlLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.manualControlLED.Location = new System.Drawing.Point(675, 49);
            this.manualControlLED.Name = "manualControlLED";
            this.manualControlLED.OffColor = System.Drawing.Color.DarkRed;
            this.manualControlLED.OnColor = System.Drawing.Color.Red;
            this.manualControlLED.Size = new System.Drawing.Size(110, 110);
            this.manualControlLED.TabIndex = 14;
            // 
            // stopStreamButton
            // 
            this.stopStreamButton.Enabled = false;
            this.stopStreamButton.Location = new System.Drawing.Point(278, 505);
            this.stopStreamButton.Name = "stopStreamButton";
            this.stopStreamButton.Size = new System.Drawing.Size(75, 23);
            this.stopStreamButton.TabIndex = 18;
            this.stopStreamButton.Text = "Stop";
            this.stopStreamButton.UseVisualStyleBackColor = true;
            this.stopStreamButton.Click += new System.EventHandler(this.stopStreamButton_Click);
            // 
            // streamButton
            // 
            this.streamButton.Location = new System.Drawing.Point(197, 505);
            this.streamButton.Name = "streamButton";
            this.streamButton.Size = new System.Drawing.Size(75, 23);
            this.streamButton.TabIndex = 17;
            this.streamButton.Text = "Stream";
            this.streamButton.UseVisualStyleBackColor = true;
            this.streamButton.Click += new System.EventHandler(this.streamButton_Click);
            // 
            // motViewer
            // 
            this.motViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.motViewer.Location = new System.Drawing.Point(2, 0);
            this.motViewer.Name = "motViewer";
            this.motViewer.Size = new System.Drawing.Size(656, 494);
            this.motViewer.TabIndex = 16;
            // 
            // snapshotButton
            // 
            this.snapshotButton.Location = new System.Drawing.Point(116, 505);
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
            this.tabLasers.Size = new System.Drawing.Size(788, 542);
            this.tabLasers.TabIndex = 1;
            this.tabLasers.Text = "Laser Control";
            this.tabLasers.UseVisualStyleBackColor = true;
            // 
            // aom3ControlBox
            // 
            this.aom3ControlBox.Controls.Add(this.aom3UpdateButton);
            this.aom3ControlBox.Controls.Add(this.aom3Label3);
            this.aom3ControlBox.Controls.Add(this.aom3Label1);
            this.aom3ControlBox.Controls.Add(this.aom3CheckBox);
            this.aom3ControlBox.Controls.Add(this.aom3rfFrequencyTextBox);
            this.aom3ControlBox.Controls.Add(this.aom3rfAmplitudeTextBox);
            this.aom3ControlBox.Controls.Add(this.aom3LED);
            this.aom3ControlBox.Controls.Add(this.aom3Label2);
            this.aom3ControlBox.Controls.Add(this.aom3Label0);
            this.aom3ControlBox.Location = new System.Drawing.Point(3, 156);
            this.aom3ControlBox.Name = "aom3ControlBox";
            this.aom3ControlBox.Size = new System.Drawing.Size(606, 45);
            this.aom3ControlBox.TabIndex = 19;
            this.aom3ControlBox.TabStop = false;
            this.aom3ControlBox.Text = "AOM 3 (Absorption)";
            // 
            // aom3UpdateButton
            // 
            this.aom3UpdateButton.Enabled = false;
            this.aom3UpdateButton.Location = new System.Drawing.Point(542, 14);
            this.aom3UpdateButton.Name = "aom3UpdateButton";
            this.aom3UpdateButton.Size = new System.Drawing.Size(58, 23);
            this.aom3UpdateButton.TabIndex = 18;
            this.aom3UpdateButton.Text = "Update";
            this.aom3UpdateButton.UseVisualStyleBackColor = true;
            this.aom3UpdateButton.Click += new System.EventHandler(this.aom3UpdateButton_Click);
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
            this.aom3rfFrequencyTextBox.Text = "0";
            // 
            // aom3rfAmplitudeTextBox
            // 
            this.aom3rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom3rfAmplitudeTextBox.Name = "aom3rfAmplitudeTextBox";
            this.aom3rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom3rfAmplitudeTextBox.TabIndex = 8;
            this.aom3rfAmplitudeTextBox.Text = "0";
            // 
            // aom3LED
            // 
            this.aom3LED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.aom3LED.Location = new System.Drawing.Point(10, 16);
            this.aom3LED.Name = "aom3LED";
            this.aom3LED.Size = new System.Drawing.Size(22, 23);
            this.aom3LED.TabIndex = 3;
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
            this.aom2ControlBox.Controls.Add(this.aom2UpdateButton);
            this.aom2ControlBox.Controls.Add(this.aom2Label3);
            this.aom2ControlBox.Controls.Add(this.aom2Label1);
            this.aom2ControlBox.Controls.Add(this.aom2CheckBox);
            this.aom2ControlBox.Controls.Add(this.aom2rfFrequencyTextBox);
            this.aom2ControlBox.Controls.Add(this.aom2rfAmplitudeTextBox);
            this.aom2ControlBox.Controls.Add(this.aom2LED);
            this.aom2ControlBox.Controls.Add(this.aom2Label2);
            this.aom2ControlBox.Controls.Add(this.aom2Label0);
            this.aom2ControlBox.Location = new System.Drawing.Point(3, 105);
            this.aom2ControlBox.Name = "aom2ControlBox";
            this.aom2ControlBox.Size = new System.Drawing.Size(606, 45);
            this.aom2ControlBox.TabIndex = 20;
            this.aom2ControlBox.TabStop = false;
            this.aom2ControlBox.Text = "AOM 2 (Zeeman)";
            // 
            // aom2UpdateButton
            // 
            this.aom2UpdateButton.Enabled = false;
            this.aom2UpdateButton.Location = new System.Drawing.Point(542, 14);
            this.aom2UpdateButton.Name = "aom2UpdateButton";
            this.aom2UpdateButton.Size = new System.Drawing.Size(58, 23);
            this.aom2UpdateButton.TabIndex = 18;
            this.aom2UpdateButton.Text = "Update";
            this.aom2UpdateButton.UseVisualStyleBackColor = true;
            this.aom2UpdateButton.Click += new System.EventHandler(this.aom2UpdateButton_Click);
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
            // aom2LED
            // 
            this.aom2LED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.aom2LED.Location = new System.Drawing.Point(10, 16);
            this.aom2LED.Name = "aom2LED";
            this.aom2LED.Size = new System.Drawing.Size(22, 23);
            this.aom2LED.TabIndex = 3;
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
            this.aom1ControlBox.Controls.Add(this.aom1UpdateButton);
            this.aom1ControlBox.Controls.Add(this.aom1Label3);
            this.aom1ControlBox.Controls.Add(this.aom1Label1);
            this.aom1ControlBox.Controls.Add(this.aom1CheckBox);
            this.aom1ControlBox.Controls.Add(this.aom1rfFrequencyTextBox);
            this.aom1ControlBox.Controls.Add(this.aom1rfAmplitudeTextBox);
            this.aom1ControlBox.Controls.Add(this.aom1LED);
            this.aom1ControlBox.Controls.Add(this.aom1Label2);
            this.aom1ControlBox.Controls.Add(this.aom1Label0);
            this.aom1ControlBox.Location = new System.Drawing.Point(3, 54);
            this.aom1ControlBox.Name = "aom1ControlBox";
            this.aom1ControlBox.Size = new System.Drawing.Size(606, 45);
            this.aom1ControlBox.TabIndex = 19;
            this.aom1ControlBox.TabStop = false;
            this.aom1ControlBox.Text = "AOM 1 (MOT Repump)";
            // 
            // aom1UpdateButton
            // 
            this.aom1UpdateButton.Enabled = false;
            this.aom1UpdateButton.Location = new System.Drawing.Point(542, 14);
            this.aom1UpdateButton.Name = "aom1UpdateButton";
            this.aom1UpdateButton.Size = new System.Drawing.Size(58, 23);
            this.aom1UpdateButton.TabIndex = 18;
            this.aom1UpdateButton.Text = "Update";
            this.aom1UpdateButton.UseVisualStyleBackColor = true;
            this.aom1UpdateButton.Click += new System.EventHandler(this.aom1UpdateButton_Click);
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
            // aom1LED
            // 
            this.aom1LED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.aom1LED.Location = new System.Drawing.Point(10, 16);
            this.aom1LED.Name = "aom1LED";
            this.aom1LED.Size = new System.Drawing.Size(22, 23);
            this.aom1LED.TabIndex = 3;
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
            this.aom0ControlBox.Controls.Add(this.aom0UpdateButton);
            this.aom0ControlBox.Controls.Add(this.aom0Label3);
            this.aom0ControlBox.Controls.Add(this.aom0Label1);
            this.aom0ControlBox.Controls.Add(this.aom0CheckBox);
            this.aom0ControlBox.Controls.Add(this.aom0rfFrequencyTextBox);
            this.aom0ControlBox.Controls.Add(this.aom0rfAmplitudeTextBox);
            this.aom0ControlBox.Controls.Add(this.aom0LED);
            this.aom0ControlBox.Controls.Add(this.aom0Label2);
            this.aom0ControlBox.Controls.Add(this.aom0Label0);
            this.aom0ControlBox.Location = new System.Drawing.Point(3, 3);
            this.aom0ControlBox.Name = "aom0ControlBox";
            this.aom0ControlBox.Size = new System.Drawing.Size(606, 45);
            this.aom0ControlBox.TabIndex = 12;
            this.aom0ControlBox.TabStop = false;
            this.aom0ControlBox.Text = "AOM 0 (MOT AOM)";
            // 
            // aom0UpdateButton
            // 
            this.aom0UpdateButton.Enabled = false;
            this.aom0UpdateButton.Location = new System.Drawing.Point(542, 14);
            this.aom0UpdateButton.Name = "aom0UpdateButton";
            this.aom0UpdateButton.Size = new System.Drawing.Size(58, 23);
            this.aom0UpdateButton.TabIndex = 18;
            this.aom0UpdateButton.Text = "Update";
            this.aom0UpdateButton.UseVisualStyleBackColor = true;
            this.aom0UpdateButton.Click += new System.EventHandler(this.aom0UpdateButton_Click);
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
            // 
            // aom0rfAmplitudeTextBox
            // 
            this.aom0rfAmplitudeTextBox.Location = new System.Drawing.Point(421, 17);
            this.aom0rfAmplitudeTextBox.Name = "aom0rfAmplitudeTextBox";
            this.aom0rfAmplitudeTextBox.Size = new System.Drawing.Size(100, 20);
            this.aom0rfAmplitudeTextBox.TabIndex = 8;
            this.aom0rfAmplitudeTextBox.Text = "0";
            // 
            // aom0LED
            // 
            this.aom0LED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.aom0LED.Location = new System.Drawing.Point(10, 16);
            this.aom0LED.Name = "aom0LED";
            this.aom0LED.Size = new System.Drawing.Size(22, 23);
            this.aom0LED.TabIndex = 3;
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
            this.tabCoils.Size = new System.Drawing.Size(788, 542);
            this.tabCoils.TabIndex = 2;
            this.tabCoils.Text = "Magnetic Field Control";
            this.tabCoils.UseVisualStyleBackColor = true;
            // 
            // coil1GroupBox
            // 
            this.coil1GroupBox.Controls.Add(this.coil1UpdateButton);
            this.coil1GroupBox.Controls.Add(this.coil1Label1);
            this.coil1GroupBox.Controls.Add(this.coil1CurrentTextBox);
            this.coil1GroupBox.Controls.Add(this.coil1Label0);
            this.coil1GroupBox.Location = new System.Drawing.Point(307, 3);
            this.coil1GroupBox.Name = "coil1GroupBox";
            this.coil1GroupBox.Size = new System.Drawing.Size(304, 45);
            this.coil1GroupBox.TabIndex = 19;
            this.coil1GroupBox.TabStop = false;
            this.coil1GroupBox.Text = "lower MOT Coil";
            // 
            // coil1UpdateButton
            // 
            this.coil1UpdateButton.Enabled = false;
            this.coil1UpdateButton.Location = new System.Drawing.Point(220, 14);
            this.coil1UpdateButton.Name = "coil1UpdateButton";
            this.coil1UpdateButton.Size = new System.Drawing.Size(58, 23);
            this.coil1UpdateButton.TabIndex = 18;
            this.coil1UpdateButton.Text = "Update";
            this.coil1UpdateButton.UseVisualStyleBackColor = true;
            this.coil1UpdateButton.Click += new System.EventHandler(this.coil1UpdateButton_Click);
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
            this.coil0GroupBox.Controls.Add(this.coil0UpdateButton);
            this.coil0GroupBox.Controls.Add(this.coil0Label1);
            this.coil0GroupBox.Controls.Add(this.coil0CurrentTextBox);
            this.coil0GroupBox.Controls.Add(this.coil0Label0);
            this.coil0GroupBox.Location = new System.Drawing.Point(3, 3);
            this.coil0GroupBox.Name = "coil0GroupBox";
            this.coil0GroupBox.Size = new System.Drawing.Size(298, 45);
            this.coil0GroupBox.TabIndex = 13;
            this.coil0GroupBox.TabStop = false;
            this.coil0GroupBox.Text = "Upper MOT Coil";
            // 
            // coil0UpdateButton
            // 
            this.coil0UpdateButton.Enabled = false;
            this.coil0UpdateButton.Location = new System.Drawing.Point(220, 14);
            this.coil0UpdateButton.Name = "coil0UpdateButton";
            this.coil0UpdateButton.Size = new System.Drawing.Size(58, 23);
            this.coil0UpdateButton.TabIndex = 18;
            this.coil0UpdateButton.Text = "Update";
            this.coil0UpdateButton.UseVisualStyleBackColor = true;
            this.coil0UpdateButton.Click += new System.EventHandler(this.coil0UpdateButton_Click);
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
            // loadParametersToolStripMenuItem
            // 
            this.loadParametersToolStripMenuItem.Name = "loadParametersToolStripMenuItem";
            this.loadParametersToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.loadParametersToolStripMenuItem.Text = "Load Parameters";
            this.loadParametersToolStripMenuItem.Click += new System.EventHandler(this.loadParametersToolStripMenuItem_Click);
            // 
            // saveParametersToolStripMenuItem
            // 
            this.saveParametersToolStripMenuItem.Name = "saveParametersToolStripMenuItem";
            this.saveParametersToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.saveParametersToolStripMenuItem.Text = "Save Parameters";
            this.saveParametersToolStripMenuItem.Click += new System.EventHandler(this.saveParametersToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(159, 6);
            // 
            // loadImageToolStripMenuItem
            // 
            this.loadImageToolStripMenuItem.Name = "loadImageToolStripMenuItem";
            this.loadImageToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.loadImageToolStripMenuItem.Text = "Load Image";
            this.loadImageToolStripMenuItem.Click += new System.EventHandler(this.loadImageToolStripMenuItem_Click);
            // 
            // saveImageToolStripMenuItem
            // 
            this.saveImageToolStripMenuItem.Name = "saveImageToolStripMenuItem";
            this.saveImageToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.saveImageToolStripMenuItem.Text = "Save Image";
            this.saveImageToolStripMenuItem.Click += new System.EventHandler(this.saveImageToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(159, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
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
            this.onToolStripMenuItem.Name = "onToolStripMenuItem";
            this.onToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
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
            // ControlWindow
            // 
            this.ClientSize = new System.Drawing.Size(797, 596);
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
            this.tabOverview.ResumeLayout(false);
            this.tabOverview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.manualControlLED)).EndInit();
            this.tabLasers.ResumeLayout(false);
            this.aom3ControlBox.ResumeLayout(false);
            this.aom3ControlBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aom3LED)).EndInit();
            this.aom2ControlBox.ResumeLayout(false);
            this.aom2ControlBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aom2LED)).EndInit();
            this.aom1ControlBox.ResumeLayout(false);
            this.aom1ControlBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aom1LED)).EndInit();
            this.aom0ControlBox.ResumeLayout(false);
            this.aom0ControlBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aom0LED)).EndInit();
            this.tabCoils.ResumeLayout(false);
            this.coil1GroupBox.ResumeLayout(false);
            this.coil1GroupBox.PerformLayout();
            this.coil0GroupBox.ResumeLayout(false);
            this.coil0GroupBox.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Click handlers

        #endregion

        #region ThreadSafe wrappers

        public void SetCheckBox(CheckBox box, bool state)
        {
            box.Invoke(new SetCheckDelegate(SetCheckHelper), new object[] { box, state });
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
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
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

        //An irritating number of threadsafe delegates for the viewer window.
        public void AttachToViewer(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image)
        {
            viewer.Invoke(new AttachImageToViewerDelegate(AttachImageHelper), new object[] {viewer, image});
        }

        private delegate void AttachImageToViewerDelegate(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image);
        private void AttachImageHelper(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image)
        {
            viewer.Attach(image);
        }

        public void UpdateViewer(NationalInstruments.Vision.WindowsForms.ImageViewer viewer)
        {
            viewer.Invoke(new UpdateViewerDelegate(UpdateImageHelper), new object[] { viewer } );
        }

        private delegate void UpdateViewerDelegate(NationalInstruments.Vision.WindowsForms.ImageViewer viewer);
        private void UpdateImageHelper(NationalInstruments.Vision.WindowsForms.ImageViewer viewer)
        {
            viewer.Update();
        }
        /*
        public void DisposeViewer(NationalInstruments.Vision.WindowsForms.ImageViewer viewer)
        {
            viewer.Invoke(new DisposeViewerDelegate(DisposeImageHelper), new object[] { viewer });
        }

        private delegate void DisposeViewerDelegate(NationalInstruments.Vision.WindowsForms.ImageViewer viewer);
        private void DisposeImageHelper(NationalInstruments.Vision.WindowsForms.ImageViewer viewer)
        {
            viewer.Dispose();
        }
        */
        #endregion



        //Declare stuff here
        private TabControl shcTabs;
        private TabPage tabOverview;
        private TabPage tabLasers;
        private TabPage tabCoils;
        private GroupBox aom0ControlBox;
        public CheckBox aom0CheckBox;
        public TextBox aom0rfFrequencyTextBox;
        public TextBox aom0rfAmplitudeTextBox;
        public Led aom0LED;
        private Label aom0Label2;
        private Label aom0Label0;
        private Label aom0Label1;
        private Label aom0Label3;
        private Button aom0UpdateButton;
        public Led manualControlLED;
        private GroupBox aom3ControlBox;
        private Button aom3UpdateButton;
        private Label aom3Label3;
        private Label aom3Label1;
        public CheckBox aom3CheckBox;
        public TextBox aom3rfFrequencyTextBox;
        public TextBox aom3rfAmplitudeTextBox;
        public Led aom3LED;
        private Label aom3Label2;
        private Label aom3Label0;
        private GroupBox aom2ControlBox;
        private Button aom2UpdateButton;
        private Label aom2Label3;
        private Label aom2Label1;
        public CheckBox aom2CheckBox;
        public TextBox aom2rfFrequencyTextBox;
        public TextBox aom2rfAmplitudeTextBox;
        public Led aom2LED;
        private Label aom2Label2;
        private Label aom2Label0;
        private GroupBox aom1ControlBox;
        private Button aom1UpdateButton;
        private Label aom1Label3;
        private Label aom1Label1;
        public CheckBox aom1CheckBox;
        public TextBox aom1rfFrequencyTextBox;
        public TextBox aom1rfAmplitudeTextBox;
        public Led aom1LED;
        private Label aom1Label2;
        private Label aom1Label0;
        private GroupBox coil1GroupBox;
        private Button coil1UpdateButton;
        private Label coil1Label1;
        public TextBox coil1CurrentTextBox;
        private Label coil1Label0;
        private GroupBox coil0GroupBox;
        private Button coil0UpdateButton;
        private Label coil0Label1;
        public TextBox coil0CurrentTextBox;
        private Label coil0Label0;
        private Button button1;
        public CheckBox checkBox1;
        public TextBox textBox1;
        public TextBox textBox2;
        public NationalInstruments.Vision.WindowsForms.ImageViewer motViewer;
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

        private void aom0CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.Aom0Enabled = this.aom0CheckBox.Checked;
        }
        private void aom1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.Aom1Enabled = this.aom1CheckBox.Checked;
        }
        private void aom2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.Aom2Enabled = this.aom2CheckBox.Checked;
        }
        private void aom3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.Aom3Enabled = this.aom3CheckBox.Checked;
        }



        private void aom0UpdateButton_Click(object sender, EventArgs e)
        {
            if(controller.SHCUIControl == true)
            {
                controller.UpdateAOM0(controller.Aom0Enabled, controller.Aom0rfAmplitude, controller.Aom0rfFrequency);
            }
        }

        private void aom1UpdateButton_Click(object sender, EventArgs e)
        {
            if (controller.SHCUIControl == true)
            {
                controller.UpdateAOM1(controller.Aom1Enabled, controller.Aom1rfAmplitude, controller.Aom1rfFrequency);
            }
        }
        private void aom2UpdateButton_Click(object sender, EventArgs e)
        {
            if (controller.SHCUIControl == true)
            {
                controller.UpdateAOM2(controller.Aom2Enabled, controller.Aom2rfAmplitude, controller.Aom2rfFrequency);
            }
        }
        


        private void aom3UpdateButton_Click(object sender, EventArgs e)
        {
            if (controller.SHCUIControl == true)
            {
                controller.UpdateAOM3(controller.Aom3Enabled, controller.Aom3rfAmplitude, controller.Aom3rfFrequency);
            }
        }

        private void coil0UpdateButton_Click(object sender, EventArgs e)
        {
            if (controller.SHCUIControl == true)
            {
                controller.UpdateCoil0(controller.Coil0Current);
            }
        }
        private void coil1UpdateButton_Click(object sender, EventArgs e)
        {
            if (controller.SHCUIControl == true)
            {
                controller.UpdateCoil1(controller.Coil1Current);
            }
        }
        
        private void WindowClosing(object sender, FormClosingEventArgs e)
        {
            controller.WindowClosing();
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            controller.WindowLoaded();
        }
        private Button snapshotButton;

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

            controller.Streaming = true;
            controller.CameraStream();
        }

        private void stopStreamButton_Click(object sender, EventArgs e)
        {
            controller.Streaming = false;

            this.snapshotButton.Enabled = true;
            this.streamButton.Enabled = true;
            this.stopStreamButton.Enabled = false;
        }

        

        private void onToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StartManualControl();
            controller.SHCUIControl = true;
            this.loadParametersToolStripMenuItem.Enabled = false;
            this.aom0UpdateButton.Enabled = true;
            this.aom1UpdateButton.Enabled = true;
            this.aom2UpdateButton.Enabled = true;
            this.aom3UpdateButton.Enabled = true;
            this.coil0UpdateButton.Enabled = true;
            this.coil1UpdateButton.Enabled = true;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StopManualControl();
            controller.SHCUIControl = false;
            this.loadParametersToolStripMenuItem.Enabled = true;
            this.aom0UpdateButton.Enabled = false;
            this.aom1UpdateButton.Enabled = false;
            this.aom2UpdateButton.Enabled = false;
            this.aom3UpdateButton.Enabled = false;
            this.coil0UpdateButton.Enabled = false;
            this.coil1UpdateButton.Enabled = false;
        }

        private void loadParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controller.SHCUIControl == false)
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
            controller.SaveImageWithDialog(this.motViewer.Image);
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadImagesWithDialog();
        }

       

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private Button updateAttributesButton;

        private void updateAttributesButton_Click(object sender, EventArgs e)
        {
            controller.UpdateCameraAttributes();
        }


       
        


        



        
        
        
     
      


       


     

    }
}
