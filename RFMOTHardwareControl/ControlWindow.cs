using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Collections.Generic;


using NationalInstruments.UI.WindowsForms;
using NationalInstruments.UI;


namespace RFMOTHardwareControl
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
        private Dictionary<string, TextBox> DDSChannels = new Dictionary<string, TextBox>();
            

        public ControlWindow()
        {
            InitializeComponent();
            AOTextBoxes["repumpsetpt"] = coolingsetptTextBox;
            AOTextBoxes["coolsetpt"] = coolingsetptTextBox;
            AOTextBoxes["motfet"] = fetvoltagesetptTextBox;
            AOTextBoxes["motlightatn"] = motlightatnTextBox;
            AOTextBoxes["motlightatn2"] = motlightatn2TextBox;
            AOTextBoxes["zbias"] = zbiasTextBox;
            AOTextBoxes["biasA"] = biasAcurrentdriver_TextBox;
            AOTextBoxes["biasB"] = biasBcurrentDriver_TextBox;
            AOTextBoxes["coolingfeedfwd"] = currentmodTextBox;
            DOCheckBoxes["refaom"] = refaomCheckBox;
            DOCheckBoxes["coolaom"] = coolingaomCheckBox;
            DOCheckBoxes["coolaom2"] = coolingaom2CheckBox;
            DOCheckBoxes["ctrInputSelect"] = coolingFrqToggle;
            DOCheckBoxes["camtrig"] = camtrigCheckBox;
            DOCheckBoxes["repumpShutter"] = repumpShutterCB;
            DDSChannels["ddsCh1"] = ch1TextBox;
            DDSChannels["ddsCh2"] = ch2TextBox;
            DDSChannels["ddsCh3"] = ch3TextBox;
            DDSChannels["ddsCh4"] = ch4TextBox;
        }

        private void WindowClosing(object sender, FormClosingEventArgs e)
        {
            controller.closeHardwareMonitorWindow();
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
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hardwareMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openImageViewer = new System.Windows.Forms.ToolStripMenuItem();
            this.startImageAnalysisWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startVoltageLoggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteControlLED = new NationalInstruments.UI.WindowsForms.Led();
            this.label1 = new System.Windows.Forms.Label();
            this.updateHardwareButton = new System.Windows.Forms.Button();
            this.consoleRichTextBox = new System.Windows.Forms.RichTextBox();
            this.tabCoils = new System.Windows.Forms.TabPage();
            this.coil0GroupBox = new System.Windows.Forms.GroupBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.biasBcurrentDriver_TextBox = new System.Windows.Forms.TextBox();
            this.biasAcurrentdriver_TextBox = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.zbiasTextBox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.coil0Label1 = new System.Windows.Forms.Label();
            this.fetvoltagesetptTextBox = new System.Windows.Forms.TextBox();
            this.coil0Label0 = new System.Windows.Forms.Label();
            this.tabLasers = new System.Windows.Forms.TabPage();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.currentmodTextBox = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.repumpsetptTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.coolingsetptTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.aom0ControlBox = new System.Windows.Forms.GroupBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.motlightatn2TextBox = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.coolingaom2CheckBox = new System.Windows.Forms.CheckBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.refaomCheckBox = new System.Windows.Forms.CheckBox();
            this.motlightatnTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.coolingaomCheckBox = new System.Windows.Forms.CheckBox();
            this.tabCamera = new System.Windows.Forms.TabPage();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.camtrigCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.snapshotButton = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.stopStreamButton = new System.Windows.Forms.Button();
            this.streamButton = new System.Windows.Forms.Button();
            this.shcTabs = new System.Windows.Forms.TabControl();
            this.COM = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.writeSerialCommandButton = new System.Windows.Forms.Button();
            this.serialCommandTextBox = new System.Windows.Forms.TextBox();
            this.setFrequenciesButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ch4TextBox = new System.Windows.Forms.TextBox();
            this.ch3TextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ch2TextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ch1TextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.closeCOMPortButton = new System.Windows.Forms.Button();
            this.openCOMPortButton = new System.Windows.Forms.Button();
            this.COMPortsListComboBox = new System.Windows.Forms.ComboBox();
            this.lookupCOMPortsButton = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.frqCtrUpdate = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.laserFrqTB = new System.Windows.Forms.TextBox();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.acquireCB = new System.Windows.Forms.CheckBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.gateTimeTB = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.repumpFrqToggle = new System.Windows.Forms.CheckBox();
            this.coolingFrqToggle = new System.Windows.Forms.CheckBox();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.repumpShutterCB = new System.Windows.Forms.CheckBox();
            this.label27 = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.remoteControlLED)).BeginInit();
            this.tabCoils.SuspendLayout();
            this.coil0GroupBox.SuspendLayout();
            this.tabLasers.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.aom0ControlBox.SuspendLayout();
            this.tabCamera.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.shcTabs.SuspendLayout();
            this.COM.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.SuspendLayout();
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
            this.menuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadParametersToolStripMenuItem,
            this.saveParametersToolStripMenuItem,
            this.toolStripSeparator1,
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
            this.openImageViewer,
            this.startImageAnalysisWindowToolStripMenuItem,
            this.startVoltageLoggerToolStripMenuItem});
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.windowsToolStripMenuItem.Text = "Windows";
            // 
            // hardwareMonitorToolStripMenuItem
            // 
            this.hardwareMonitorToolStripMenuItem.Name = "hardwareMonitorToolStripMenuItem";
            this.hardwareMonitorToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this.hardwareMonitorToolStripMenuItem.Text = "Open new hardware monitor";
            this.hardwareMonitorToolStripMenuItem.Click += new System.EventHandler(this.hardwareMonitorToolStripMenuItem_Click);
            // 
            // openImageViewer
            // 
            this.openImageViewer.Name = "openImageViewer";
            this.openImageViewer.Size = new System.Drawing.Size(269, 22);
            this.openImageViewer.Text = "Start Camera and open Image Viewer";
            this.openImageViewer.Click += new System.EventHandler(this.openImageViewerToolStripMenuItem_Click);
            // 
            // startImageAnalysisWindowToolStripMenuItem
            // 
            this.startImageAnalysisWindowToolStripMenuItem.Name = "startImageAnalysisWindowToolStripMenuItem";
            this.startImageAnalysisWindowToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this.startImageAnalysisWindowToolStripMenuItem.Text = "Start Image Analysis Window";
            this.startImageAnalysisWindowToolStripMenuItem.Click += new System.EventHandler(this.startImageAnalysisWindowToolStripMenuItem_Click);
            // 
            // startVoltageLoggerToolStripMenuItem
            // 
            this.startVoltageLoggerToolStripMenuItem.Name = "startVoltageLoggerToolStripMenuItem";
            this.startVoltageLoggerToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this.startVoltageLoggerToolStripMenuItem.Text = "Start Voltage Logger";
            this.startVoltageLoggerToolStripMenuItem.Click += new System.EventHandler(this.startVoltageLoggerToolStripMenuItem_Click);
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
            // tabCoils
            // 
            this.tabCoils.Controls.Add(this.coil0GroupBox);
            this.tabCoils.Location = new System.Drawing.Point(4, 22);
            this.tabCoils.Name = "tabCoils";
            this.tabCoils.Size = new System.Drawing.Size(658, 209);
            this.tabCoils.TabIndex = 2;
            this.tabCoils.Text = "Magnetic Field Control";
            this.tabCoils.UseVisualStyleBackColor = true;
            // 
            // coil0GroupBox
            // 
            this.coil0GroupBox.Controls.Add(this.label21);
            this.coil0GroupBox.Controls.Add(this.label20);
            this.coil0GroupBox.Controls.Add(this.label19);
            this.coil0GroupBox.Controls.Add(this.label18);
            this.coil0GroupBox.Controls.Add(this.biasBcurrentDriver_TextBox);
            this.coil0GroupBox.Controls.Add(this.biasAcurrentdriver_TextBox);
            this.coil0GroupBox.Controls.Add(this.label16);
            this.coil0GroupBox.Controls.Add(this.zbiasTextBox);
            this.coil0GroupBox.Controls.Add(this.label14);
            this.coil0GroupBox.Controls.Add(this.coil0Label1);
            this.coil0GroupBox.Controls.Add(this.fetvoltagesetptTextBox);
            this.coil0GroupBox.Controls.Add(this.coil0Label0);
            this.coil0GroupBox.Location = new System.Drawing.Point(3, 3);
            this.coil0GroupBox.Name = "coil0GroupBox";
            this.coil0GroupBox.Size = new System.Drawing.Size(272, 122);
            this.coil0GroupBox.TabIndex = 13;
            this.coil0GroupBox.TabStop = false;
            this.coil0GroupBox.Text = "MOT coils";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(6, 94);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(105, 13);
            this.label21.TabIndex = 26;
            this.label21.Text = "Bias B Current Driver";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(6, 68);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(108, 13);
            this.label20.TabIndex = 25;
            this.label20.Text = "Bias A Current Driver ";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(249, 94);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(14, 13);
            this.label19.TabIndex = 24;
            this.label19.Text = "V";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(249, 68);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(14, 13);
            this.label18.TabIndex = 23;
            this.label18.Text = "V";
            // 
            // biasBcurrentDriver_TextBox
            // 
            this.biasBcurrentDriver_TextBox.Location = new System.Drawing.Point(148, 91);
            this.biasBcurrentDriver_TextBox.Name = "biasBcurrentDriver_TextBox";
            this.biasBcurrentDriver_TextBox.Size = new System.Drawing.Size(100, 20);
            this.biasBcurrentDriver_TextBox.TabIndex = 22;
            this.biasBcurrentDriver_TextBox.Text = "0";
            // 
            // biasAcurrentdriver_TextBox
            // 
            this.biasAcurrentdriver_TextBox.Location = new System.Drawing.Point(148, 65);
            this.biasAcurrentdriver_TextBox.Name = "biasAcurrentdriver_TextBox";
            this.biasAcurrentdriver_TextBox.Size = new System.Drawing.Size(100, 20);
            this.biasAcurrentdriver_TextBox.TabIndex = 21;
            this.biasAcurrentdriver_TextBox.Text = "0";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(249, 43);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(14, 13);
            this.label16.TabIndex = 20;
            this.label16.Text = "V";
            // 
            // zbiasTextBox
            // 
            this.zbiasTextBox.Location = new System.Drawing.Point(148, 39);
            this.zbiasTextBox.Name = "zbiasTextBox";
            this.zbiasTextBox.Size = new System.Drawing.Size(100, 20);
            this.zbiasTextBox.TabIndex = 19;
            this.zbiasTextBox.Text = "0";
            this.zbiasTextBox.TextChanged += new System.EventHandler(this.zbiasTextBox_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 42);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(99, 13);
            this.label14.TabIndex = 18;
            this.label14.Text = "Z Bias FET Voltage";
            // 
            // coil0Label1
            // 
            this.coil0Label1.AutoSize = true;
            this.coil0Label1.Location = new System.Drawing.Point(249, 21);
            this.coil0Label1.Name = "coil0Label1";
            this.coil0Label1.Size = new System.Drawing.Size(14, 13);
            this.coil0Label1.TabIndex = 17;
            this.coil0Label1.Text = "V";
            // 
            // fetvoltagesetptTextBox
            // 
            this.fetvoltagesetptTextBox.Location = new System.Drawing.Point(148, 17);
            this.fetvoltagesetptTextBox.Name = "fetvoltagesetptTextBox";
            this.fetvoltagesetptTextBox.Size = new System.Drawing.Size(100, 20);
            this.fetvoltagesetptTextBox.TabIndex = 8;
            this.fetvoltagesetptTextBox.Text = "0";
            this.fetvoltagesetptTextBox.TextChanged += new System.EventHandler(this.fetvoltageTextBox_TextChanged);
            // 
            // coil0Label0
            // 
            this.coil0Label0.AutoSize = true;
            this.coil0Label0.Location = new System.Drawing.Point(6, 20);
            this.coil0Label0.Name = "coil0Label0";
            this.coil0Label0.Size = new System.Drawing.Size(93, 13);
            this.coil0Label0.TabIndex = 7;
            this.coil0Label0.Text = "MOT FET Voltage";
            // 
            // tabLasers
            // 
            this.tabLasers.AutoScroll = true;
            this.tabLasers.Controls.Add(this.groupBox13);
            this.tabLasers.Controls.Add(this.groupBox12);
            this.tabLasers.Controls.Add(this.groupBox6);
            this.tabLasers.Controls.Add(this.aom0ControlBox);
            this.tabLasers.Location = new System.Drawing.Point(4, 22);
            this.tabLasers.Name = "tabLasers";
            this.tabLasers.Padding = new System.Windows.Forms.Padding(3);
            this.tabLasers.Size = new System.Drawing.Size(658, 209);
            this.tabLasers.TabIndex = 1;
            this.tabLasers.Text = "Laser Control";
            this.tabLasers.UseVisualStyleBackColor = true;
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.label22);
            this.groupBox12.Controls.Add(this.label23);
            this.groupBox12.Controls.Add(this.currentmodTextBox);
            this.groupBox12.Location = new System.Drawing.Point(6, 105);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(254, 64);
            this.groupBox12.TabIndex = 15;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Cooling Current Mod";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(231, 28);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(14, 13);
            this.label22.TabIndex = 21;
            this.label22.Text = "V";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(6, 28);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(67, 13);
            this.label23.TabIndex = 19;
            this.label23.Text = "Mod Voltage";
            // 
            // currentmodTextBox
            // 
            this.currentmodTextBox.Location = new System.Drawing.Point(139, 25);
            this.currentmodTextBox.Name = "currentmodTextBox";
            this.currentmodTextBox.Size = new System.Drawing.Size(86, 20);
            this.currentmodTextBox.TabIndex = 20;
            this.currentmodTextBox.Text = "0";
            this.currentmodTextBox.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label13);
            this.groupBox6.Controls.Add(this.repumpsetptTextBox);
            this.groupBox6.Controls.Add(this.label10);
            this.groupBox6.Controls.Add(this.label9);
            this.groupBox6.Controls.Add(this.coolingsetptTextBox);
            this.groupBox6.Controls.Add(this.label8);
            this.groupBox6.Location = new System.Drawing.Point(6, 13);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(254, 86);
            this.groupBox6.TabIndex = 14;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Offset Locks";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(231, 49);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(14, 13);
            this.label13.TabIndex = 18;
            this.label13.Text = "V";
            // 
            // repumpsetptTextBox
            // 
            this.repumpsetptTextBox.Location = new System.Drawing.Point(139, 47);
            this.repumpsetptTextBox.Name = "repumpsetptTextBox";
            this.repumpsetptTextBox.Size = new System.Drawing.Size(86, 20);
            this.repumpsetptTextBox.TabIndex = 17;
            this.repumpsetptTextBox.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 49);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(118, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "Repump Laser Setpoint";
            this.label10.Click += new System.EventHandler(this.label10_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(231, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(14, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "V";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // coolingsetptTextBox
            // 
            this.coolingsetptTextBox.Location = new System.Drawing.Point(139, 21);
            this.coolingsetptTextBox.Name = "coolingsetptTextBox";
            this.coolingsetptTextBox.Size = new System.Drawing.Size(86, 20);
            this.coolingsetptTextBox.TabIndex = 14;
            this.coolingsetptTextBox.Text = "0";
            this.coolingsetptTextBox.TextChanged += new System.EventHandler(this.coolingsetptTextBox_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 24);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(113, 13);
            this.label8.TabIndex = 13;
            this.label8.Text = "Cooling Laser Setpoint";
            // 
            // aom0ControlBox
            // 
            this.aom0ControlBox.Controls.Add(this.label25);
            this.aom0ControlBox.Controls.Add(this.label26);
            this.aom0ControlBox.Controls.Add(this.motlightatn2TextBox);
            this.aom0ControlBox.Controls.Add(this.label24);
            this.aom0ControlBox.Controls.Add(this.coolingaom2CheckBox);
            this.aom0ControlBox.Controls.Add(this.label17);
            this.aom0ControlBox.Controls.Add(this.label15);
            this.aom0ControlBox.Controls.Add(this.label6);
            this.aom0ControlBox.Controls.Add(this.refaomCheckBox);
            this.aom0ControlBox.Controls.Add(this.motlightatnTextBox);
            this.aom0ControlBox.Controls.Add(this.label7);
            this.aom0ControlBox.Controls.Add(this.coolingaomCheckBox);
            this.aom0ControlBox.Location = new System.Drawing.Point(316, 13);
            this.aom0ControlBox.Name = "aom0ControlBox";
            this.aom0ControlBox.Size = new System.Drawing.Size(298, 98);
            this.aom0ControlBox.TabIndex = 12;
            this.aom0ControlBox.TabStop = false;
            this.aom0ControlBox.Text = "AOMs";
            this.aom0ControlBox.Enter += new System.EventHandler(this.aom0ControlBox_Enter);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(125, 47);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(61, 13);
            this.label25.TabIndex = 23;
            this.label25.Text = "Attenuation";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(275, 47);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(14, 13);
            this.label26.TabIndex = 22;
            this.label26.Text = "V";
            // 
            // motlightatn2TextBox
            // 
            this.motlightatn2TextBox.Location = new System.Drawing.Point(192, 44);
            this.motlightatn2TextBox.Name = "motlightatn2TextBox";
            this.motlightatn2TextBox.Size = new System.Drawing.Size(77, 20);
            this.motlightatn2TextBox.TabIndex = 21;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(6, 47);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(66, 13);
            this.label24.TabIndex = 20;
            this.label24.Text = "MOT Light 2";
            // 
            // coolingaom2CheckBox
            // 
            this.coolingaom2CheckBox.AutoSize = true;
            this.coolingaom2CheckBox.Location = new System.Drawing.Point(98, 47);
            this.coolingaom2CheckBox.Name = "coolingaom2CheckBox";
            this.coolingaom2CheckBox.Size = new System.Drawing.Size(15, 14);
            this.coolingaom2CheckBox.TabIndex = 19;
            this.coolingaom2CheckBox.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(125, 24);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(61, 13);
            this.label17.TabIndex = 18;
            this.label17.Text = "Attenuation";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(275, 24);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(14, 13);
            this.label15.TabIndex = 17;
            this.label15.Text = "V";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Imaging Light";
            // 
            // refaomCheckBox
            // 
            this.refaomCheckBox.AutoSize = true;
            this.refaomCheckBox.Location = new System.Drawing.Point(98, 70);
            this.refaomCheckBox.Name = "refaomCheckBox";
            this.refaomCheckBox.Size = new System.Drawing.Size(15, 14);
            this.refaomCheckBox.TabIndex = 10;
            this.refaomCheckBox.UseVisualStyleBackColor = true;
            // 
            // motlightatnTextBox
            // 
            this.motlightatnTextBox.Location = new System.Drawing.Point(192, 21);
            this.motlightatnTextBox.Name = "motlightatnTextBox";
            this.motlightatnTextBox.Size = new System.Drawing.Size(77, 20);
            this.motlightatnTextBox.TabIndex = 15;
            this.motlightatnTextBox.TextChanged += new System.EventHandler(this.motlightatnTextBox_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "MOT Light 1";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // coolingaomCheckBox
            // 
            this.coolingaomCheckBox.AutoSize = true;
            this.coolingaomCheckBox.Location = new System.Drawing.Point(98, 24);
            this.coolingaomCheckBox.Name = "coolingaomCheckBox";
            this.coolingaomCheckBox.Size = new System.Drawing.Size(15, 14);
            this.coolingaomCheckBox.TabIndex = 12;
            this.coolingaomCheckBox.UseVisualStyleBackColor = true;
            this.coolingaomCheckBox.CheckedChanged += new System.EventHandler(this.coolingaomCheckBox_CheckedChanged);
            // 
            // tabCamera
            // 
            this.tabCamera.Controls.Add(this.groupBox11);
            this.tabCamera.Controls.Add(this.groupBox5);
            this.tabCamera.Controls.Add(this.groupBox4);
            this.tabCamera.Location = new System.Drawing.Point(4, 22);
            this.tabCamera.Name = "tabCamera";
            this.tabCamera.Padding = new System.Windows.Forms.Padding(3);
            this.tabCamera.Size = new System.Drawing.Size(658, 209);
            this.tabCamera.TabIndex = 0;
            this.tabCamera.Text = "Camera Control";
            this.tabCamera.UseVisualStyleBackColor = true;
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.camtrigCheckBox);
            this.groupBox11.Location = new System.Drawing.Point(358, 13);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(113, 51);
            this.groupBox11.TabIndex = 5;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Trigger Idle State";
            // 
            // camtrigCheckBox
            // 
            this.camtrigCheckBox.AutoSize = true;
            this.camtrigCheckBox.Location = new System.Drawing.Point(28, 23);
            this.camtrigCheckBox.Name = "camtrigCheckBox";
            this.camtrigCheckBox.Size = new System.Drawing.Size(48, 17);
            this.camtrigCheckBox.TabIndex = 0;
            this.camtrigCheckBox.Text = "High";
            this.camtrigCheckBox.UseVisualStyleBackColor = true;
            this.camtrigCheckBox.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged_1);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.saveButton);
            this.groupBox5.Controls.Add(this.snapshotButton);
            this.groupBox5.Location = new System.Drawing.Point(5, 13);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(169, 51);
            this.groupBox5.TabIndex = 4;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Snapshot";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(87, 19);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 3;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // snapshotButton
            // 
            this.snapshotButton.Location = new System.Drawing.Point(6, 19);
            this.snapshotButton.Name = "snapshotButton";
            this.snapshotButton.Size = new System.Drawing.Size(75, 23);
            this.snapshotButton.TabIndex = 2;
            this.snapshotButton.Text = "Acquire";
            this.snapshotButton.Click += new System.EventHandler(this.snapshotButton_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.stopStreamButton);
            this.groupBox4.Controls.Add(this.streamButton);
            this.groupBox4.Location = new System.Drawing.Point(180, 13);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(172, 51);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Stream";
            // 
            // stopStreamButton
            // 
            this.stopStreamButton.Location = new System.Drawing.Point(87, 19);
            this.stopStreamButton.Name = "stopStreamButton";
            this.stopStreamButton.Size = new System.Drawing.Size(75, 23);
            this.stopStreamButton.TabIndex = 0;
            this.stopStreamButton.Text = "Stop";
            this.stopStreamButton.Click += new System.EventHandler(this.stopStreamButton_Click);
            // 
            // streamButton
            // 
            this.streamButton.Location = new System.Drawing.Point(6, 19);
            this.streamButton.Name = "streamButton";
            this.streamButton.Size = new System.Drawing.Size(75, 23);
            this.streamButton.TabIndex = 1;
            this.streamButton.Text = "Start";
            this.streamButton.Click += new System.EventHandler(this.streamButton_Click);
            // 
            // shcTabs
            // 
            this.shcTabs.AllowDrop = true;
            this.shcTabs.Controls.Add(this.tabCamera);
            this.shcTabs.Controls.Add(this.tabLasers);
            this.shcTabs.Controls.Add(this.tabCoils);
            this.shcTabs.Controls.Add(this.COM);
            this.shcTabs.Controls.Add(this.tabPage1);
            this.shcTabs.Location = new System.Drawing.Point(3, 27);
            this.shcTabs.Name = "shcTabs";
            this.shcTabs.SelectedIndex = 0;
            this.shcTabs.Size = new System.Drawing.Size(666, 235);
            this.shcTabs.TabIndex = 0;
            // 
            // COM
            // 
            this.COM.Controls.Add(this.groupBox3);
            this.COM.Controls.Add(this.groupBox2);
            this.COM.Controls.Add(this.groupBox1);
            this.COM.Location = new System.Drawing.Point(4, 22);
            this.COM.Name = "COM";
            this.COM.Padding = new System.Windows.Forms.Padding(3);
            this.COM.Size = new System.Drawing.Size(658, 209);
            this.COM.TabIndex = 3;
            this.COM.Text = "COM";
            this.COM.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.writeSerialCommandButton);
            this.groupBox3.Controls.Add(this.serialCommandTextBox);
            this.groupBox3.Controls.Add(this.setFrequenciesButton);
            this.groupBox3.Location = new System.Drawing.Point(6, 136);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(570, 51);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Serial Commands";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // writeSerialCommandButton
            // 
            this.writeSerialCommandButton.Location = new System.Drawing.Point(428, 19);
            this.writeSerialCommandButton.Name = "writeSerialCommandButton";
            this.writeSerialCommandButton.Size = new System.Drawing.Size(131, 23);
            this.writeSerialCommandButton.TabIndex = 25;
            this.writeSerialCommandButton.Text = "Write Serial Command";
            this.writeSerialCommandButton.UseVisualStyleBackColor = true;
            this.writeSerialCommandButton.Click += new System.EventHandler(this.writeSerialCommandButton_Click);
            // 
            // serialCommandTextBox
            // 
            this.serialCommandTextBox.Location = new System.Drawing.Point(153, 21);
            this.serialCommandTextBox.Name = "serialCommandTextBox";
            this.serialCommandTextBox.Size = new System.Drawing.Size(269, 20);
            this.serialCommandTextBox.TabIndex = 24;
            // 
            // setFrequenciesButton
            // 
            this.setFrequenciesButton.Location = new System.Drawing.Point(21, 19);
            this.setFrequenciesButton.Name = "setFrequenciesButton";
            this.setFrequenciesButton.Size = new System.Drawing.Size(100, 23);
            this.setFrequenciesButton.TabIndex = 4;
            this.setFrequenciesButton.Text = "Set Frequencies";
            this.setFrequenciesButton.UseVisualStyleBackColor = true;
            this.setFrequenciesButton.Click += new System.EventHandler(this.setFrequenciesButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ch4TextBox);
            this.groupBox2.Controls.Add(this.ch3TextBox);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.ch2TextBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.ch1TextBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(6, 72);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(592, 58);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Channel Frequencies";
            // 
            // ch4TextBox
            // 
            this.ch4TextBox.Location = new System.Drawing.Point(447, 24);
            this.ch4TextBox.Name = "ch4TextBox";
            this.ch4TextBox.Size = new System.Drawing.Size(100, 20);
            this.ch4TextBox.TabIndex = 7;
            // 
            // ch3TextBox
            // 
            this.ch3TextBox.Location = new System.Drawing.Point(300, 24);
            this.ch3TextBox.Name = "ch3TextBox";
            this.ch3TextBox.Size = new System.Drawing.Size(100, 20);
            this.ch3TextBox.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(553, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Ch4";
            // 
            // ch2TextBox
            // 
            this.ch2TextBox.Location = new System.Drawing.Point(153, 24);
            this.ch2TextBox.Name = "ch2TextBox";
            this.ch2TextBox.Size = new System.Drawing.Size(100, 20);
            this.ch2TextBox.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(406, 27);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Ch3";
            // 
            // ch1TextBox
            // 
            this.ch1TextBox.Location = new System.Drawing.Point(6, 24);
            this.ch1TextBox.Name = "ch1TextBox";
            this.ch1TextBox.Size = new System.Drawing.Size(100, 20);
            this.ch1TextBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(259, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Ch2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(112, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Ch1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.closeCOMPortButton);
            this.groupBox1.Controls.Add(this.openCOMPortButton);
            this.groupBox1.Controls.Add(this.COMPortsListComboBox);
            this.groupBox1.Controls.Add(this.lookupCOMPortsButton);
            this.groupBox1.Location = new System.Drawing.Point(6, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(570, 53);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "COM Port Select";
            // 
            // closeCOMPortButton
            // 
            this.closeCOMPortButton.Location = new System.Drawing.Point(438, 18);
            this.closeCOMPortButton.Name = "closeCOMPortButton";
            this.closeCOMPortButton.Size = new System.Drawing.Size(123, 21);
            this.closeCOMPortButton.TabIndex = 3;
            this.closeCOMPortButton.Text = "Close Port";
            this.closeCOMPortButton.UseVisualStyleBackColor = true;
            this.closeCOMPortButton.Click += new System.EventHandler(this.closeCOMPortButton_Click);
            // 
            // openCOMPortButton
            // 
            this.openCOMPortButton.Location = new System.Drawing.Point(309, 18);
            this.openCOMPortButton.Name = "openCOMPortButton";
            this.openCOMPortButton.Size = new System.Drawing.Size(123, 21);
            this.openCOMPortButton.TabIndex = 2;
            this.openCOMPortButton.Text = "Open Port";
            this.openCOMPortButton.UseVisualStyleBackColor = true;
            this.openCOMPortButton.Click += new System.EventHandler(this.openCOMPortButton_Click);
            // 
            // COMPortsListComboBox
            // 
            this.COMPortsListComboBox.FormattingEnabled = true;
            this.COMPortsListComboBox.Location = new System.Drawing.Point(6, 19);
            this.COMPortsListComboBox.Name = "COMPortsListComboBox";
            this.COMPortsListComboBox.Size = new System.Drawing.Size(168, 21);
            this.COMPortsListComboBox.TabIndex = 0;
            // 
            // lookupCOMPortsButton
            // 
            this.lookupCOMPortsButton.Location = new System.Drawing.Point(180, 18);
            this.lookupCOMPortsButton.Name = "lookupCOMPortsButton";
            this.lookupCOMPortsButton.Size = new System.Drawing.Size(123, 21);
            this.lookupCOMPortsButton.TabIndex = 1;
            this.lookupCOMPortsButton.Text = "Refresh";
            this.lookupCOMPortsButton.UseVisualStyleBackColor = true;
            this.lookupCOMPortsButton.Click += new System.EventHandler(this.lookupCOMPortsButton_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox10);
            this.tabPage1.Controls.Add(this.label12);
            this.tabPage1.Controls.Add(this.laserFrqTB);
            this.tabPage1.Controls.Add(this.groupBox9);
            this.tabPage1.Controls.Add(this.groupBox8);
            this.tabPage1.Controls.Add(this.groupBox7);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(658, 209);
            this.tabPage1.TabIndex = 4;
            this.tabPage1.Text = "Laser Frequencies";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.frqCtrUpdate);
            this.groupBox10.Location = new System.Drawing.Point(420, 22);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(119, 64);
            this.groupBox10.TabIndex = 12;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Update Settings";
            // 
            // frqCtrUpdate
            // 
            this.frqCtrUpdate.Location = new System.Drawing.Point(8, 23);
            this.frqCtrUpdate.Name = "frqCtrUpdate";
            this.frqCtrUpdate.Size = new System.Drawing.Size(101, 24);
            this.frqCtrUpdate.TabIndex = 0;
            this.frqCtrUpdate.Text = "Update";
            this.frqCtrUpdate.UseVisualStyleBackColor = true;
            this.frqCtrUpdate.Click += new System.EventHandler(this.frqCtrUpdate_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(273, 111);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(102, 46);
            this.label12.TabIndex = 11;
            this.label12.Text = "MHz";
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // laserFrqTB
            // 
            this.laserFrqTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.laserFrqTB.Location = new System.Drawing.Point(30, 108);
            this.laserFrqTB.Name = "laserFrqTB";
            this.laserFrqTB.Size = new System.Drawing.Size(237, 53);
            this.laserFrqTB.TabIndex = 9;
            this.laserFrqTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.laserFrqTB.TextChanged += new System.EventHandler(this.laserFrqTB_TextChanged);
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.acquireCB);
            this.groupBox9.Location = new System.Drawing.Point(273, 22);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(141, 64);
            this.groupBox9.TabIndex = 7;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Turn On";
            // 
            // acquireCB
            // 
            this.acquireCB.AutoSize = true;
            this.acquireCB.Location = new System.Drawing.Point(6, 25);
            this.acquireCB.Name = "acquireCB";
            this.acquireCB.Size = new System.Drawing.Size(133, 17);
            this.acquireCB.TabIndex = 5;
            this.acquireCB.Text = "Continuous Acquisition";
            this.acquireCB.UseVisualStyleBackColor = true;
            this.acquireCB.CheckedChanged += new System.EventHandler(this.acquireCB_CheckedChanged);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.label11);
            this.groupBox8.Controls.Add(this.gateTimeTB);
            this.groupBox8.Location = new System.Drawing.Point(129, 22);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(138, 64);
            this.groupBox8.TabIndex = 6;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Gate Time Set";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(112, 26);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(20, 13);
            this.label11.TabIndex = 4;
            this.label11.Text = "(S)";
            // 
            // gateTimeTB
            // 
            this.gateTimeTB.Location = new System.Drawing.Point(11, 23);
            this.gateTimeTB.Name = "gateTimeTB";
            this.gateTimeTB.Size = new System.Drawing.Size(95, 20);
            this.gateTimeTB.TabIndex = 2;
            this.gateTimeTB.TextChanged += new System.EventHandler(this.gateTimeTB_TextChanged);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.repumpFrqToggle);
            this.groupBox7.Controls.Add(this.coolingFrqToggle);
            this.groupBox7.Location = new System.Drawing.Point(28, 22);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(95, 64);
            this.groupBox7.TabIndex = 1;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Laser Toggle";
            // 
            // repumpFrqToggle
            // 
            this.repumpFrqToggle.AutoSize = true;
            this.repumpFrqToggle.Location = new System.Drawing.Point(6, 42);
            this.repumpFrqToggle.Name = "repumpFrqToggle";
            this.repumpFrqToggle.Size = new System.Drawing.Size(66, 17);
            this.repumpFrqToggle.TabIndex = 1;
            this.repumpFrqToggle.Text = "Repump";
            this.repumpFrqToggle.UseVisualStyleBackColor = true;
            this.repumpFrqToggle.Click += new System.EventHandler(this.repumpFrqToggle_Click);
            // 
            // coolingFrqToggle
            // 
            this.coolingFrqToggle.AutoSize = true;
            this.coolingFrqToggle.Location = new System.Drawing.Point(6, 19);
            this.coolingFrqToggle.Name = "coolingFrqToggle";
            this.coolingFrqToggle.Size = new System.Drawing.Size(61, 17);
            this.coolingFrqToggle.TabIndex = 0;
            this.coolingFrqToggle.Text = "Cooling";
            this.coolingFrqToggle.UseVisualStyleBackColor = true;
            this.coolingFrqToggle.CheckedChanged += new System.EventHandler(this.coolingFrqToggle_CheckedChanged);
            this.coolingFrqToggle.Click += new System.EventHandler(this.coolingFrqToggle_Click);
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.label27);
            this.groupBox13.Controls.Add(this.repumpShutterCB);
            this.groupBox13.Location = new System.Drawing.Point(316, 117);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(297, 55);
            this.groupBox13.TabIndex = 16;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "Shutters";
            // 
            // repumpShutterCB
            // 
            this.repumpShutterCB.AutoSize = true;
            this.repumpShutterCB.Location = new System.Drawing.Point(98, 19);
            this.repumpShutterCB.Name = "repumpShutterCB";
            this.repumpShutterCB.Size = new System.Drawing.Size(15, 14);
            this.repumpShutterCB.TabIndex = 0;
            this.repumpShutterCB.UseVisualStyleBackColor = true;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(6, 20);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(84, 13);
            this.label27.TabIndex = 1;
            this.label27.Text = "Repump Shutter";
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
            this.Text = "RFMOT Hardware Controller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WindowClosing);
            this.Load += new System.EventHandler(this.WindowLoaded);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.remoteControlLED)).EndInit();
            this.tabCoils.ResumeLayout(false);
            this.coil0GroupBox.ResumeLayout(false);
            this.coil0GroupBox.PerformLayout();
            this.tabLasers.ResumeLayout(false);
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.aom0ControlBox.ResumeLayout(false);
            this.aom0ControlBox.PerformLayout();
            this.tabCamera.ResumeLayout(false);
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.shcTabs.ResumeLayout(false);
            this.COM.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
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
        private Button button1;
        public CheckBox checkBox1;
        public TextBox textBox1;
        public TextBox textBox2;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loadParametersToolStripMenuItem;
        private ToolStripMenuItem saveParametersToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private Led remoteControlLED;
        private Label label1;
        private Button updateHardwareButton;
        private ToolStripMenuItem windowsToolStripMenuItem;
        private ToolStripMenuItem hardwareMonitorToolStripMenuItem;
        private RichTextBox consoleRichTextBox;

        #endregion

        #region Click Handlers

        private void saveParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveParametersWithDialog();
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
        #endregion

        #region SerialCOMStuff
        public void FillCOMPortsComboBox(string[] s)
        {
            if (s.Length == 0){
                WriteToConsole("No open COM Ports found..");
            }
            COMPortsListComboBox.Items.Clear();
            COMPortsListComboBox.Items.AddRange(s);
            COMPortsListComboBox.Text = s[0];
        }

        private void lookupCOMPortsButton_Click(object sender, EventArgs e)
        {
            controller.COMPortsLookupAndDisplay();
        }

        private void openCOMPortButton_Click(object sender, EventArgs e)
        {
            string portName = COMPortsListComboBox.SelectedItem.ToString();
            WriteToConsole("Trying to open Novatech dds on " + portName);
            controller.startNovatech(portName);
        }
        private void closeCOMPortButton_Click(object sender, EventArgs e)
        {
            controller.endNovatech();
        }

        private void setFrequenciesButton_Click(object sender, EventArgs e)
        {
            Dictionary<string,double> frequenciesToSet = new Dictionary<string,double>();
            foreach(var key in DDSChannels.Keys){
                TextBox value = DDSChannels[key];               
                frequenciesToSet[key] = double.Parse(value.Text);
                }
            controller.passDDSNewFrequencies(frequenciesToSet);
        }
        private void writeSerialCommandButton_Click(object sender, EventArgs e)
        {
            controller.writeSerialCommand(this.serialCommandTextBox.Text);
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

        #region MCCounterInterface

        public float getGateTime()
        {
            return float.Parse(gateTimeTB.Text);
        }

        public bool getAcquisitionState()
        {
            return acquireCB.Checked;
        }

        public void setFreqDisplay(object sender, EventArgs args)
        {
            setTextBox(laserFrqTB,controller.getNewFreq());
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
        private void saveButton_Click(object sender, EventArgs e)
        {
            controller.SaveImageWithDialog();
        }

        #endregion

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void fetvoltageTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void aom0Label2_Click(object sender, EventArgs e)
        {

        }

        private void aom0rfAmplitudeTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void coil0CurrentTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        public TabPage tabCoils;
        private GroupBox coil0GroupBox;
        private Label coil0Label1;
        public TextBox fetvoltagesetptTextBox;
        private Label coil0Label0;
        public TabPage tabLasers;
        private GroupBox aom0ControlBox;
        public CheckBox refaomCheckBox;
        public TabPage tabCamera;
        private Button stopStreamButton;
        private Button streamButton;
        private Button snapshotButton;
        public TabControl shcTabs;

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private TabPage COM;
        private GroupBox groupBox1;
        private ComboBox COMPortsListComboBox;
        private Button lookupCOMPortsButton;
        private Button openCOMPortButton;
        private Button setFrequenciesButton;
        private GroupBox groupBox2;
        private TextBox ch4TextBox;
        private TextBox ch3TextBox;
        private Label label4;
        private TextBox ch2TextBox;
        private Label label5;
        private TextBox ch1TextBox;
        private Label label3;
        private Label label2;
        private Button closeCOMPortButton;
        private GroupBox groupBox3;

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private Button writeSerialCommandButton;
        private TextBox serialCommandTextBox;
        private GroupBox groupBox5;
        private GroupBox groupBox4;
        private ToolStripMenuItem openImageViewer;
        private Button saveButton;

        private void aom0ControlBox_Enter(object sender, EventArgs e)
        {

        }

        private GroupBox groupBox6;
        private Label label9;
        public TextBox coolingsetptTextBox;
        private Label label8;
        private Label label7;
        public CheckBox coolingaomCheckBox;
        private Label label6;

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private TabPage tabPage1;
        private GroupBox groupBox7;
        private CheckBox coolingFrqToggle;

        private void coolingFrqToggle_Click(object sender, EventArgs e)
        {
            if (repumpFrqToggle.Checked)
            {
                repumpFrqToggle.Checked = false;
            }

        }

        private TextBox laserFrqTB;
        private GroupBox groupBox9;
        private CheckBox acquireCB;
        private GroupBox groupBox8;
        private Label label11;
        private TextBox gateTimeTB;
        private CheckBox repumpFrqToggle;
        private Label label12;

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private GroupBox groupBox10;
        private Button frqCtrUpdate;

        private void gateTimeTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void acquireCB_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void frqCtrUpdate_Click(object sender, EventArgs e)
        {
            controller.updateFreqCtrSettings();
        }

        private void repumpFrqToggle_Click(object sender, EventArgs e)
        {
            if (coolingFrqToggle.Checked)
            {
                coolingFrqToggle.Checked = false;
            }

        }

        private void laserFrqTB_TextChanged(object sender, EventArgs e)
        {

        }

        private Label label10;

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private Label label13;
        public TextBox repumpsetptTextBox;

        private void coolingsetptTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void coolingFrqToggle_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void coolingaomCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }
        private TextBox motlightatnTextBox;

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private Label label17;
        private Label label15;

        private void motlightatnTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private GroupBox groupBox11;
        private CheckBox camtrigCheckBox;

        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private Label label16;
        public TextBox zbiasTextBox;
        private Label label14;

        private void zbiasTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private ToolStripMenuItem startImageAnalysisWindowToolStripMenuItem;

        private void startImageAnalysisWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.OpenNewImageAnalysisWindow();
        }

        private Label label21;
        private Label label20;
        private Label label19;
        private Label label18;
        public TextBox biasBcurrentDriver_TextBox;
        public TextBox biasAcurrentdriver_TextBox;
        private GroupBox groupBox12;
        private Label label22;
        private Label label23;
        public TextBox currentmodTextBox;

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private Label label24;
        public CheckBox coolingaom2CheckBox;
        private Label label25;
        private Label label26;
        private TextBox motlightatn2TextBox;
        private ToolStripMenuItem startVoltageLoggerToolStripMenuItem;

        private void startVoltageLoggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.openNewVoltageLoggerWindow();
        }

        private GroupBox groupBox13;
        private Label label27;
        private CheckBox repumpShutterCB;

        

      

        

    



















    }
}
