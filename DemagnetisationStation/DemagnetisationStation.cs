/******************************************************************************
*
* Example program:
*   GenMultVoltUpdates_SWTimed
*
* Category:
*   AO
*
* Description:
*   This example demonstrates how to output multiple voltage updates (samples)
*   to an analog output channel in a software timed loop.
*
* Instructions for running:
*   1.  Select the physical channel corresponding to where your signal is output
*       on the DAQ device.
*   2.  Enter the minimum and maximum voltage values.
*   3.  Set the loop rate.  Note that the resolution of the timer is
*       approximately 10 ms.
*
* Steps:
*   1.  Create a new task and an analog output voltage channel.
*   2.  Enable the timer.
*   3.  Inside the timer event handler, create a AnalogSingleChannelWrite and
*       call the WriteSingleSample method to write one sine wave value to the
*       channel at a time.
*   4.  When the user hits the stop button, disable the timer and stop the task.
*   5.  Dispose the Task object to clean-up any resources associated with the
*       task.
*   6.  Handle any DaqExceptions, if they occur.
*
* I/O Connections Overview:
*   Make sure your signal output terminal matches the text in the physical
*   channel text box. In this case the signal will output to the ao0 pin on your
*   DAQ Device.  For more information on the input and output terminals for your
*   device, open the NI-DAQmx Help, and refer to the NI-DAQmx Device Terminals
*   and Device Considerations books in the table of contents.
*
* Microsoft Windows Vista User Account Control
*   Running certain applications on Microsoft Windows Vista requires
*   administrator privileges, because the application name contains keywords
*   such as setup, update, or install. To avoid this problem, you must add an
*   additional manifest to the application that specifies the privileges
*   required to run the application. Some Measurement Studio NI-DAQmx examples
*   for Visual Studio include these keywords. Therefore, all examples for Visual
*   Studio are shipped with an additional manifest file that you must embed in
*   the example executable. The manifest file is named
*   [ExampleName].exe.manifest, where [ExampleName] is the NI-provided example
*   name. For information on how to embed the manifest file, refer to http://msdn2.microsoft.com/en-us/library/bb756929.aspx.
*   
*   Note: The manifest file is not provided with examples for Visual Studio .NET
*   2003.
*
******************************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Windows.Threading;
using NationalInstruments.DAQmx;
using System.IO;

namespace NationalInstruments.Examples.GenMultVoltUpdates_SWTimed
{
    /// <summary>
    /// Summary description for MainForm.
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button waveStartButton;
        private System.Windows.Forms.Label maximumValueLabel;
        private System.Windows.Forms.Label minimumValueLabel;
        private System.Windows.Forms.Label physicalChannelLabel;
        private System.Windows.Forms.GroupBox channelParametersGroupBox;
        private System.Windows.Forms.NumericUpDown outputFreq;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Button waveStopButton;
        private System.Windows.Forms.GroupBox timingParametersGroupBox;
        private System.Windows.Forms.Label freqLabel;
        private System.Windows.Forms.ComboBox physicalChannelComboBox;
        private System.Windows.Forms.NumericUpDown expTimeConstant;
        private System.Windows.Forms.Label expTimeConstantLabel;
        private System.Windows.Forms.Timer timer;
        private AnalogSingleChannelWriter writer;
        private Task waveOutTask;
        private Task readInTask;
        private AnalogMultiChannelReader analogInReader;
        private int counter = 0;
        private double amplitude = 0;
        private double offset = 0;
        private double max = 0;
        private double min = 0;
        private int t = 0;
        private double modTime = 0;
        private double output = 0;
        private RadioButton expDecay;
        private RadioButton noMod;
        private RadioButton linDecay;
        private Label linTimeConstantLabel;
        private NumericUpDown linTimeConstant;
        private GroupBox waveGenerationGroup;
        private GroupBox groupBox2;
        private Button aquireStopButton;
        private Button aquireStartButton;
        private GroupBox groupBox3;
        private ComboBox aiChannel1ComboBox;
        private NumericUpDown aiMinimumValueNumeric;
        private NumericUpDown aiMaximumValueNumeric;
        private Label maximumLabel;
        private Label minimumLabel;
        private Label label1;
        private GroupBox writeToFileGroupBox;
        private Label filePathWriteLabel;
        private TextBox filePathWriteTextBox;
        private string fileNameWrite;
        private GroupBox groupBox4;
        private NumericUpDown aiRateNumeric;
        private Label samplesLabel;
        private Label rateLabel;
        private NumericUpDown samplesPerChannelNumeric;
        private Button browseWriteButton;
        private SaveFileDialog saveFileDialog1;
        private SaveFileDialog writeToFileSaveFileDialog;
        private ToolTip fileToolTip;
        private MicroLibrary.MicroTimer microTimer;
        private StreamWriter fileStreamWriter;
        private DataColumn[] dataColumn = null;
        private DataTable dataTable = null;
        private ArrayList savedData;
        private AsyncCallback analogCallback;
        private NumericUpDown aoMinimumValueNumeric;
        private NumericUpDown aoMaximumValueNumeric;
        private ComboBox aiChannel3ComboBox;
        private Label label3;
        private ComboBox aiChannel2ComboBox;
        private Label label2;
        private RadioButton continuousRadioButton;
        private RadioButton finiteRadioButton;
        private double[,] data;

        public MainForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            dataTable = new DataTable();

            physicalChannelComboBox.Items.AddRange(DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AO, PhysicalChannelAccess.External));
            if (physicalChannelComboBox.Items.Count > 0)
                physicalChannelComboBox.SelectedIndex = 0;

            aiChannel1ComboBox.Items.AddRange(DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External));
            if (physicalChannelComboBox.Items.Count > 0)
                physicalChannelComboBox.SelectedIndex = 0;

            aiChannel2ComboBox.Items.AddRange(DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External));
            if (physicalChannelComboBox.Items.Count > 0)
                physicalChannelComboBox.SelectedIndex = 0;

            aiChannel3ComboBox.Items.AddRange(DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External));
            if (physicalChannelComboBox.Items.Count > 0)
                physicalChannelComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.waveStartButton = new System.Windows.Forms.Button();
            this.channelParametersGroupBox = new System.Windows.Forms.GroupBox();
            this.aoMaximumValueNumeric = new System.Windows.Forms.NumericUpDown();
            this.aoMinimumValueNumeric = new System.Windows.Forms.NumericUpDown();
            this.physicalChannelComboBox = new System.Windows.Forms.ComboBox();
            this.maximumValueLabel = new System.Windows.Forms.Label();
            this.minimumValueLabel = new System.Windows.Forms.Label();
            this.physicalChannelLabel = new System.Windows.Forms.Label();
            this.outputFreq = new System.Windows.Forms.NumericUpDown();
            this.timingParametersGroupBox = new System.Windows.Forms.GroupBox();
            this.freqLabel = new System.Windows.Forms.Label();
            this.waveStopButton = new System.Windows.Forms.Button();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linTimeConstantLabel = new System.Windows.Forms.Label();
            this.linTimeConstant = new System.Windows.Forms.NumericUpDown();
            this.noMod = new System.Windows.Forms.RadioButton();
            this.linDecay = new System.Windows.Forms.RadioButton();
            this.expDecay = new System.Windows.Forms.RadioButton();
            this.expTimeConstant = new System.Windows.Forms.NumericUpDown();
            this.expTimeConstantLabel = new System.Windows.Forms.Label();
            this.waveGenerationGroup = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.writeToFileGroupBox = new System.Windows.Forms.GroupBox();
            this.browseWriteButton = new System.Windows.Forms.Button();
            this.filePathWriteTextBox = new System.Windows.Forms.TextBox();
            this.filePathWriteLabel = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.aiRateNumeric = new System.Windows.Forms.NumericUpDown();
            this.samplesLabel = new System.Windows.Forms.Label();
            this.rateLabel = new System.Windows.Forms.Label();
            this.samplesPerChannelNumeric = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.aiChannel1ComboBox = new System.Windows.Forms.ComboBox();
            this.aiMinimumValueNumeric = new System.Windows.Forms.NumericUpDown();
            this.aiMaximumValueNumeric = new System.Windows.Forms.NumericUpDown();
            this.maximumLabel = new System.Windows.Forms.Label();
            this.minimumLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.aquireStopButton = new System.Windows.Forms.Button();
            this.aquireStartButton = new System.Windows.Forms.Button();
            this.writeToFileSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.fileToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.aiChannel2ComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.aiChannel3ComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.finiteRadioButton = new System.Windows.Forms.RadioButton();
            this.continuousRadioButton = new System.Windows.Forms.RadioButton();
            this.channelParametersGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aoMaximumValueNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aoMinimumValueNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputFreq)).BeginInit();
            this.timingParametersGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.linTimeConstant)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.expTimeConstant)).BeginInit();
            this.waveGenerationGroup.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.writeToFileGroupBox.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aiRateNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.samplesPerChannelNumeric)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aiMinimumValueNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aiMaximumValueNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // waveStartButton
            // 
            this.waveStartButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.waveStartButton.Location = new System.Drawing.Point(92, 325);
            this.waveStartButton.Name = "waveStartButton";
            this.waveStartButton.Size = new System.Drawing.Size(75, 23);
            this.waveStartButton.TabIndex = 0;
            this.waveStartButton.Text = "Start Output";
            this.waveStartButton.Click += new System.EventHandler(this.WaveStartButton_Click);
            // 
            // channelParametersGroupBox
            // 
            this.channelParametersGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.channelParametersGroupBox.Controls.Add(this.aoMaximumValueNumeric);
            this.channelParametersGroupBox.Controls.Add(this.aoMinimumValueNumeric);
            this.channelParametersGroupBox.Controls.Add(this.physicalChannelComboBox);
            this.channelParametersGroupBox.Controls.Add(this.maximumValueLabel);
            this.channelParametersGroupBox.Controls.Add(this.minimumValueLabel);
            this.channelParametersGroupBox.Controls.Add(this.physicalChannelLabel);
            this.channelParametersGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.channelParametersGroupBox.Location = new System.Drawing.Point(6, 19);
            this.channelParametersGroupBox.Name = "channelParametersGroupBox";
            this.channelParametersGroupBox.Size = new System.Drawing.Size(343, 128);
            this.channelParametersGroupBox.TabIndex = 2;
            this.channelParametersGroupBox.TabStop = false;
            this.channelParametersGroupBox.Text = "Channel Parameters";
            // 
            // aoMaximumValueNumeric
            // 
            this.aoMaximumValueNumeric.Location = new System.Drawing.Point(152, 94);
            this.aoMaximumValueNumeric.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.aoMaximumValueNumeric.Name = "aoMaximumValueNumeric";
            this.aoMaximumValueNumeric.Size = new System.Drawing.Size(183, 20);
            this.aoMaximumValueNumeric.TabIndex = 7;
            this.aoMaximumValueNumeric.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // aoMinimumValueNumeric
            // 
            this.aoMinimumValueNumeric.Location = new System.Drawing.Point(152, 60);
            this.aoMinimumValueNumeric.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.aoMinimumValueNumeric.Name = "aoMinimumValueNumeric";
            this.aoMinimumValueNumeric.Size = new System.Drawing.Size(183, 20);
            this.aoMinimumValueNumeric.TabIndex = 6;
            // 
            // physicalChannelComboBox
            // 
            this.physicalChannelComboBox.Location = new System.Drawing.Point(152, 24);
            this.physicalChannelComboBox.Name = "physicalChannelComboBox";
            this.physicalChannelComboBox.Size = new System.Drawing.Size(183, 21);
            this.physicalChannelComboBox.TabIndex = 1;
            this.physicalChannelComboBox.Text = "Dev1/ao0";
            // 
            // maximumValueLabel
            // 
            this.maximumValueLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.maximumValueLabel.Location = new System.Drawing.Point(16, 96);
            this.maximumValueLabel.Name = "maximumValueLabel";
            this.maximumValueLabel.Size = new System.Drawing.Size(112, 16);
            this.maximumValueLabel.TabIndex = 4;
            this.maximumValueLabel.Text = "Maximum Value (V):";
            // 
            // minimumValueLabel
            // 
            this.minimumValueLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.minimumValueLabel.Location = new System.Drawing.Point(16, 62);
            this.minimumValueLabel.Name = "minimumValueLabel";
            this.minimumValueLabel.Size = new System.Drawing.Size(104, 16);
            this.minimumValueLabel.TabIndex = 2;
            this.minimumValueLabel.Text = "Minimum Value (V):";
            // 
            // physicalChannelLabel
            // 
            this.physicalChannelLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.physicalChannelLabel.Location = new System.Drawing.Point(16, 26);
            this.physicalChannelLabel.Name = "physicalChannelLabel";
            this.physicalChannelLabel.Size = new System.Drawing.Size(96, 16);
            this.physicalChannelLabel.TabIndex = 0;
            this.physicalChannelLabel.Text = "Physical Channel:";
            // 
            // outputFreq
            // 
            this.outputFreq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFreq.Location = new System.Drawing.Point(152, 24);
            this.outputFreq.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.outputFreq.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.outputFreq.Name = "outputFreq";
            this.outputFreq.Size = new System.Drawing.Size(183, 20);
            this.outputFreq.TabIndex = 1;
            this.outputFreq.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.outputFreq.ValueChanged += new System.EventHandler(this.rateNumericUpDown_ValueChanged);
            // 
            // timingParametersGroupBox
            // 
            this.timingParametersGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.timingParametersGroupBox.Controls.Add(this.freqLabel);
            this.timingParametersGroupBox.Controls.Add(this.outputFreq);
            this.timingParametersGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.timingParametersGroupBox.Location = new System.Drawing.Point(6, 153);
            this.timingParametersGroupBox.Name = "timingParametersGroupBox";
            this.timingParametersGroupBox.Size = new System.Drawing.Size(343, 56);
            this.timingParametersGroupBox.TabIndex = 3;
            this.timingParametersGroupBox.TabStop = false;
            this.timingParametersGroupBox.Text = "Timing Parameters";
            // 
            // freqLabel
            // 
            this.freqLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.freqLabel.Location = new System.Drawing.Point(16, 24);
            this.freqLabel.Name = "freqLabel";
            this.freqLabel.Size = new System.Drawing.Size(136, 16);
            this.freqLabel.TabIndex = 0;
            this.freqLabel.Text = "Wave frequency (Hz):";
            // 
            // waveStopButton
            // 
            this.waveStopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.waveStopButton.Enabled = false;
            this.waveStopButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.waveStopButton.Location = new System.Drawing.Point(173, 325);
            this.waveStopButton.Name = "waveStopButton";
            this.waveStopButton.Size = new System.Drawing.Size(75, 23);
            this.waveStopButton.TabIndex = 1;
            this.waveStopButton.Text = "Stop Output";
            this.waveStopButton.Click += new System.EventHandler(this.WaveStopButton_Click);
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.groupBox1.Controls.Add(this.linTimeConstantLabel);
            this.groupBox1.Controls.Add(this.linTimeConstant);
            this.groupBox1.Controls.Add(this.noMod);
            this.groupBox1.Controls.Add(this.linDecay);
            this.groupBox1.Controls.Add(this.expDecay);
            this.groupBox1.Controls.Add(this.expTimeConstant);
            this.groupBox1.Controls.Add(this.expTimeConstantLabel);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(6, 215);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(343, 99);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Modulation Parameters";
            // 
            // linTimeConstantLabel
            // 
            this.linTimeConstantLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linTimeConstantLabel.Location = new System.Drawing.Point(147, 67);
            this.linTimeConstantLabel.Name = "linTimeConstantLabel";
            this.linTimeConstantLabel.Size = new System.Drawing.Size(95, 20);
            this.linTimeConstantLabel.TabIndex = 14;
            this.linTimeConstantLabel.Text = "Decay Time (s):";
            // 
            // linTimeConstant
            // 
            this.linTimeConstant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.linTimeConstant.Location = new System.Drawing.Point(248, 65);
            this.linTimeConstant.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.linTimeConstant.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.linTimeConstant.Name = "linTimeConstant";
            this.linTimeConstant.Size = new System.Drawing.Size(87, 20);
            this.linTimeConstant.TabIndex = 13;
            this.linTimeConstant.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // noMod
            // 
            this.noMod.AutoSize = true;
            this.noMod.Checked = true;
            this.noMod.Location = new System.Drawing.Point(19, 19);
            this.noMod.Name = "noMod";
            this.noMod.Size = new System.Drawing.Size(94, 17);
            this.noMod.TabIndex = 12;
            this.noMod.TabStop = true;
            this.noMod.Text = "No Modulation";
            this.noMod.UseVisualStyleBackColor = true;
            // 
            // linDecay
            // 
            this.linDecay.AutoSize = true;
            this.linDecay.Location = new System.Drawing.Point(19, 65);
            this.linDecay.Name = "linDecay";
            this.linDecay.Size = new System.Drawing.Size(88, 17);
            this.linDecay.TabIndex = 11;
            this.linDecay.Text = "Linear Decay";
            this.linDecay.UseVisualStyleBackColor = true;
            this.linDecay.CheckedChanged += new System.EventHandler(this.linDecay_CheckedChanged);
            // 
            // expDecay
            // 
            this.expDecay.AutoSize = true;
            this.expDecay.Location = new System.Drawing.Point(19, 42);
            this.expDecay.Name = "expDecay";
            this.expDecay.Size = new System.Drawing.Size(114, 17);
            this.expDecay.TabIndex = 10;
            this.expDecay.Text = "Exponential Decay";
            this.expDecay.UseVisualStyleBackColor = true;
            this.expDecay.CheckedChanged += new System.EventHandler(this.expDecay_CheckedChanged);
            // 
            // expTimeConstant
            // 
            this.expTimeConstant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.expTimeConstant.Location = new System.Drawing.Point(248, 42);
            this.expTimeConstant.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.expTimeConstant.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.expTimeConstant.Name = "expTimeConstant";
            this.expTimeConstant.Size = new System.Drawing.Size(87, 20);
            this.expTimeConstant.TabIndex = 7;
            this.expTimeConstant.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // expTimeConstantLabel
            // 
            this.expTimeConstantLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.expTimeConstantLabel.Location = new System.Drawing.Point(147, 44);
            this.expTimeConstantLabel.Name = "expTimeConstantLabel";
            this.expTimeConstantLabel.Size = new System.Drawing.Size(95, 20);
            this.expTimeConstantLabel.TabIndex = 8;
            this.expTimeConstantLabel.Text = "Time Constant (s):";
            // 
            // waveGenerationGroup
            // 
            this.waveGenerationGroup.Controls.Add(this.waveStopButton);
            this.waveGenerationGroup.Controls.Add(this.groupBox1);
            this.waveGenerationGroup.Controls.Add(this.waveStartButton);
            this.waveGenerationGroup.Controls.Add(this.channelParametersGroupBox);
            this.waveGenerationGroup.Controls.Add(this.timingParametersGroupBox);
            this.waveGenerationGroup.Location = new System.Drawing.Point(12, 5);
            this.waveGenerationGroup.Name = "waveGenerationGroup";
            this.waveGenerationGroup.Size = new System.Drawing.Size(357, 356);
            this.waveGenerationGroup.TabIndex = 5;
            this.waveGenerationGroup.TabStop = false;
            this.waveGenerationGroup.Text = "Wave Generation";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Controls.Add(this.writeToFileGroupBox);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.aquireStopButton);
            this.groupBox2.Controls.Add(this.aquireStartButton);
            this.groupBox2.Location = new System.Drawing.Point(396, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(467, 356);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data Aquisition";
            // 
            // writeToFileGroupBox
            // 
            this.writeToFileGroupBox.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.writeToFileGroupBox.Controls.Add(this.browseWriteButton);
            this.writeToFileGroupBox.Controls.Add(this.filePathWriteTextBox);
            this.writeToFileGroupBox.Controls.Add(this.filePathWriteLabel);
            this.writeToFileGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.writeToFileGroupBox.Location = new System.Drawing.Point(6, 251);
            this.writeToFileGroupBox.Name = "writeToFileGroupBox";
            this.writeToFileGroupBox.Size = new System.Drawing.Size(448, 63);
            this.writeToFileGroupBox.TabIndex = 13;
            this.writeToFileGroupBox.TabStop = false;
            this.writeToFileGroupBox.Text = "Write To File";
            // 
            // browseWriteButton
            // 
            this.browseWriteButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.browseWriteButton.Location = new System.Drawing.Point(413, 22);
            this.browseWriteButton.Name = "browseWriteButton";
            this.browseWriteButton.Size = new System.Drawing.Size(24, 23);
            this.browseWriteButton.TabIndex = 7;
            this.browseWriteButton.Text = "...";
            this.browseWriteButton.Click += new System.EventHandler(this.browseWriteButton_Click);
            // 
            // filePathWriteTextBox
            // 
            this.filePathWriteTextBox.Location = new System.Drawing.Point(67, 26);
            this.filePathWriteTextBox.Name = "filePathWriteTextBox";
            this.filePathWriteTextBox.ReadOnly = true;
            this.filePathWriteTextBox.Size = new System.Drawing.Size(340, 20);
            this.filePathWriteTextBox.TabIndex = 4;
            this.filePathWriteTextBox.Text = "Choose file location";
            // 
            // filePathWriteLabel
            // 
            this.filePathWriteLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.filePathWriteLabel.Location = new System.Drawing.Point(16, 29);
            this.filePathWriteLabel.Name = "filePathWriteLabel";
            this.filePathWriteLabel.Size = new System.Drawing.Size(72, 16);
            this.filePathWriteLabel.TabIndex = 3;
            this.filePathWriteLabel.Text = "File Path:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.continuousRadioButton);
            this.groupBox4.Controls.Add(this.finiteRadioButton);
            this.groupBox4.Controls.Add(this.aiRateNumeric);
            this.groupBox4.Controls.Add(this.samplesLabel);
            this.groupBox4.Controls.Add(this.rateLabel);
            this.groupBox4.Controls.Add(this.samplesPerChannelNumeric);
            this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox4.Location = new System.Drawing.Point(6, 153);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(448, 92);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Timing Parameters";
            // 
            // aiRateNumeric
            // 
            this.aiRateNumeric.DecimalPlaces = 2;
            this.aiRateNumeric.Location = new System.Drawing.Point(261, 56);
            this.aiRateNumeric.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.aiRateNumeric.Name = "aiRateNumeric";
            this.aiRateNumeric.Size = new System.Drawing.Size(176, 20);
            this.aiRateNumeric.TabIndex = 3;
            this.aiRateNumeric.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // samplesLabel
            // 
            this.samplesLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.samplesLabel.Location = new System.Drawing.Point(151, 26);
            this.samplesLabel.Name = "samplesLabel";
            this.samplesLabel.Size = new System.Drawing.Size(104, 16);
            this.samplesLabel.TabIndex = 0;
            this.samplesLabel.Text = "Samples/Channel:";
            this.samplesLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // rateLabel
            // 
            this.rateLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rateLabel.Location = new System.Drawing.Point(199, 58);
            this.rateLabel.Name = "rateLabel";
            this.rateLabel.Size = new System.Drawing.Size(56, 16);
            this.rateLabel.TabIndex = 2;
            this.rateLabel.Text = "Rate (Hz):";
            this.rateLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // samplesPerChannelNumeric
            // 
            this.samplesPerChannelNumeric.Location = new System.Drawing.Point(261, 24);
            this.samplesPerChannelNumeric.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.samplesPerChannelNumeric.Name = "samplesPerChannelNumeric";
            this.samplesPerChannelNumeric.Size = new System.Drawing.Size(176, 20);
            this.samplesPerChannelNumeric.TabIndex = 1;
            this.samplesPerChannelNumeric.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.aiChannel3ComboBox);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.aiChannel2ComboBox);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.aiChannel1ComboBox);
            this.groupBox3.Controls.Add(this.aiMinimumValueNumeric);
            this.groupBox3.Controls.Add(this.aiMaximumValueNumeric);
            this.groupBox3.Controls.Add(this.maximumLabel);
            this.groupBox3.Controls.Add(this.minimumLabel);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox3.Location = new System.Drawing.Point(6, 19);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(448, 128);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Channel Parameters";
            // 
            // aiChannel1ComboBox
            // 
            this.aiChannel1ComboBox.Location = new System.Drawing.Point(120, 23);
            this.aiChannel1ComboBox.Name = "aiChannel1ComboBox";
            this.aiChannel1ComboBox.Size = new System.Drawing.Size(96, 21);
            this.aiChannel1ComboBox.TabIndex = 1;
            this.aiChannel1ComboBox.Text = "Dev1/ai0";
            // 
            // aiMinimumValueNumeric
            // 
            this.aiMinimumValueNumeric.Location = new System.Drawing.Point(341, 41);
            this.aiMinimumValueNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.aiMinimumValueNumeric.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.aiMinimumValueNumeric.Name = "aiMinimumValueNumeric";
            this.aiMinimumValueNumeric.Size = new System.Drawing.Size(96, 20);
            this.aiMinimumValueNumeric.TabIndex = 3;
            // 
            // aiMaximumValueNumeric
            // 
            this.aiMaximumValueNumeric.Location = new System.Drawing.Point(341, 73);
            this.aiMaximumValueNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.aiMaximumValueNumeric.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.aiMaximumValueNumeric.Name = "aiMaximumValueNumeric";
            this.aiMaximumValueNumeric.Size = new System.Drawing.Size(96, 20);
            this.aiMaximumValueNumeric.TabIndex = 5;
            this.aiMaximumValueNumeric.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // maximumLabel
            // 
            this.maximumLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.maximumLabel.Location = new System.Drawing.Point(240, 75);
            this.maximumLabel.Name = "maximumLabel";
            this.maximumLabel.Size = new System.Drawing.Size(112, 16);
            this.maximumLabel.TabIndex = 4;
            this.maximumLabel.Text = "Maximum Value (V):";
            // 
            // minimumLabel
            // 
            this.minimumLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.minimumLabel.Location = new System.Drawing.Point(240, 43);
            this.minimumLabel.Name = "minimumLabel";
            this.minimumLabel.Size = new System.Drawing.Size(104, 15);
            this.minimumLabel.TabIndex = 2;
            this.minimumLabel.Text = "Minimum Value (V):";
            // 
            // label1
            // 
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(16, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Physical Channel 1:";
            // 
            // aquireStopButton
            // 
            this.aquireStopButton.Enabled = false;
            this.aquireStopButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.aquireStopButton.Location = new System.Drawing.Point(126, 323);
            this.aquireStopButton.Name = "aquireStopButton";
            this.aquireStopButton.Size = new System.Drawing.Size(87, 23);
            this.aquireStopButton.TabIndex = 2;
            this.aquireStopButton.Text = "Stop Aquisition";
            this.aquireStopButton.Click += new System.EventHandler(this.aquireStopButton_Click);
            // 
            // aquireStartButton
            // 
            this.aquireStartButton.Enabled = false;
            this.aquireStartButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.aquireStartButton.Location = new System.Drawing.Point(31, 323);
            this.aquireStartButton.Name = "aquireStartButton";
            this.aquireStartButton.Size = new System.Drawing.Size(87, 23);
            this.aquireStartButton.TabIndex = 1;
            this.aquireStartButton.Text = "Start Aquisition";
            this.aquireStartButton.Click += new System.EventHandler(this.aquireStartButton_Click);
            // 
            // writeToFileSaveFileDialog
            // 
            this.writeToFileSaveFileDialog.DefaultExt = "txt";
            this.writeToFileSaveFileDialog.FileName = "aquisitionData.txt";
            this.writeToFileSaveFileDialog.Filter = "Text Files|*.txt| All Files|*.*";
            this.writeToFileSaveFileDialog.Title = "Save Acquisition Data To File";
            // 
            // aiChannel2ComboBox
            // 
            this.aiChannel2ComboBox.Location = new System.Drawing.Point(120, 59);
            this.aiChannel2ComboBox.Name = "aiChannel2ComboBox";
            this.aiChannel2ComboBox.Size = new System.Drawing.Size(96, 21);
            this.aiChannel2ComboBox.TabIndex = 7;
            this.aiChannel2ComboBox.Text = "Dev1/ai1";
            // 
            // label2
            // 
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Location = new System.Drawing.Point(16, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Physical Channel 2:";
            // 
            // aiChannel3ComboBox
            // 
            this.aiChannel3ComboBox.Location = new System.Drawing.Point(120, 93);
            this.aiChannel3ComboBox.Name = "aiChannel3ComboBox";
            this.aiChannel3ComboBox.Size = new System.Drawing.Size(96, 21);
            this.aiChannel3ComboBox.TabIndex = 9;
            this.aiChannel3ComboBox.Text = "Dev1/ai2";
            // 
            // label3
            // 
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.Location = new System.Drawing.Point(16, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 16);
            this.label3.TabIndex = 8;
            this.label3.Text = "Physical Channel 3:";
            // 
            // finiteRadioButton
            // 
            this.finiteRadioButton.AutoSize = true;
            this.finiteRadioButton.Checked = true;
            this.finiteRadioButton.Location = new System.Drawing.Point(19, 22);
            this.finiteRadioButton.Name = "finiteRadioButton";
            this.finiteRadioButton.Size = new System.Drawing.Size(80, 17);
            this.finiteRadioButton.TabIndex = 13;
            this.finiteRadioButton.Text = "Finite Mode";
            this.finiteRadioButton.UseVisualStyleBackColor = true;
            // 
            // continuousRadioButton
            // 
            this.continuousRadioButton.AutoSize = true;
            this.continuousRadioButton.Location = new System.Drawing.Point(19, 56);
            this.continuousRadioButton.Name = "continuousRadioButton";
            this.continuousRadioButton.Size = new System.Drawing.Size(108, 17);
            this.continuousRadioButton.TabIndex = 14;
            this.continuousRadioButton.Text = "Continuous Mode";
            this.continuousRadioButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(870, 365);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.waveGenerationGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 10);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(2000, 589);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(256, 289);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DemagnetisationStation";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
            this.channelParametersGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.aoMaximumValueNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aoMinimumValueNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputFreq)).EndInit();
            this.timingParametersGroupBox.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.linTimeConstant)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.expTimeConstant)).EndInit();
            this.waveGenerationGroup.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.writeToFileGroupBox.ResumeLayout(false);
            this.writeToFileGroupBox.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aiRateNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.samplesPerChannelNumeric)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.aiMinimumValueNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aiMaximumValueNumeric)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() 
        {
            Application.EnableVisualStyles();
            Application.DoEvents();
            Application.Run(new MainForm());
        }

        #region Wave Generation
        private void WaveStartButton_Click(object sender, System.EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                waveOutTask = new Task();
                waveOutTask.AOChannels.CreateVoltageChannel(physicalChannelComboBox.Text, "aoChannel",
                    Convert.ToDouble(aoMinimumValueNumeric.Text), Convert.ToDouble(aoMaximumValueNumeric.Text),
                    AOVoltageUnits.Volts);
                writer = new AnalogSingleChannelWriter(waveOutTask.Stream);

                max = Convert.ToDouble(aoMaximumValueNumeric.Text);
                min = Convert.ToDouble(aoMinimumValueNumeric.Text);
                amplitude = max - min;
                offset = min;
                t = Convert.ToInt32(1000000.0 / Convert.ToDouble(outputFreq.Value)); // Output wave period (µs)
                counter = 0;

                microTimer = new MicroLibrary.MicroTimer();
                microTimer.MicroTimerElapsed += new MicroLibrary.MicroTimer.MicroTimerElapsedEventHandler(OnTimedEvent);
                microTimer.Interval = t / 2;             // Set timer interval to half the wave period
                // microTimer.IgnoreEventIfLateBy = 500; // Can choose to ignore event if late by Xµs (by default it will try to catch up)

                CalcTimeConstant();

                microTimer.Enabled = true;               // Start timer
                //timer.Enabled = true;                  // Old unreliable System.Timer - don't use!

                waveStartButton.Enabled = false;
                waveStopButton.Enabled = true;               
            }
            catch(DaqException ex)
            {
                MessageBox.Show(ex.Message);
                //myTask.Dispose();
                waveOutTask = null;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void WaveStopButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                //timer.Enabled = false;    // Obsolete
                microTimer.Enabled = false; // Stop timer
                waveOutTask.Dispose();
                waveOutTask = null;
                waveStartButton.Enabled = true;
                waveStopButton.Enabled = false;
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);                
            }       
        }

        private void CalcTimeConstant()
        {
            // TimeConstant is in (s). Counter is incremented every t/2 (µs)
            if (noMod.Checked)
            {
                modTime = 0;
            }
            else if (expDecay.Checked)
            {
                modTime = 1000000.0 * Convert.ToDouble(expTimeConstant.Value) * 2 / t;
            }
            else if (linDecay.Checked)
            {
                modTime = 1000000.0 * Convert.ToDouble(linTimeConstant.Value) * 2 / t;
            }
        }
        private void OnTimedEvent(object sender, MicroLibrary.MicroTimerEventArgs timerEventArgs)
        {
            if (waveOutTask == null) return;
            try
            {
                if (noMod.Checked)
                {
                    output = amplitude * (counter++ % 2) + offset;
                    writer.WriteSingleSample(true, output);
                }
                else if (expDecay.Checked)
                {
                    output = Math.Exp(- counter / modTime) * amplitude * (counter % 2) + offset;
                    //output = Math.Exp(-counter / modTime) * amplitude + offset; // for debug
                    writer.WriteSingleSample(true, output);
                    counter++;
                }
                else if (linDecay.Checked)
                {
                    output = (amplitude - amplitude * counter / modTime) * (counter % 2) + offset;
                    if (output < min) { output = min; }
                    writer.WriteSingleSample(true, output);
                    counter++;
                }
            }
            catch (DaqException ex)
            {
                timer.Enabled = false;
                MessageBox.Show(ex.Message);
                waveOutTask.Dispose();
                waveOutTask = null;
                waveStartButton.Enabled = true;
                waveStopButton.Enabled = false;
            }
        }

        private void timer_Tick(object sender, System.EventArgs e)
        {
            // This function is obsolete. System.Timer is not reliable enough. Use the MicroLibrary
            // or Multimedia timer instead.

            if (waveOutTask == null) return;
            try
            {
                output = amplitude * (counter++ % 2) + offset;
                writer.WriteSingleSample(true, output);               
            }
            catch (DaqException ex)
            {
                timer.Enabled = false;
                MessageBox.Show(ex.Message);
                waveOutTask.Dispose();
                waveOutTask = null;
                waveStartButton.Enabled = true;
                waveStopButton.Enabled = false;
            }
        }

        private void rateNumericUpDown_ValueChanged(object sender, System.EventArgs e)
        {
           timer.Interval = Convert.ToInt32(outputFreq.Value);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (waveOutTask != null)
                WaveStopButton_Click(null,null);
        }

        private void expDecay_CheckedChanged(object sender, EventArgs e)
        {
            CalcTimeConstant();
            counter = 0; // Reset counter when activate (and deactivate) expDecay
        }

        private void linDecay_CheckedChanged(object sender, EventArgs e)
        {
            CalcTimeConstant();
            counter = 0; // Reset counter when activate (and deactivate) lineDecay
        }
        #endregion

        #region Data Aquisition
        private void aquireStartButton_Click(object sender, EventArgs e)
        {
            aquireStartButton.Enabled = false;
            aquireStopButton.Enabled = true;

            bool opened = CreateDataFile();
            if (!opened) { return; }

            try
            {
                //Create a new task
                readInTask = new Task();
                //Create virtual channels
                readInTask.AIChannels.CreateVoltageChannel(aiChannel1ComboBox.Text, "",
                    AITerminalConfiguration.Rse, Convert.ToDouble(aiMinimumValueNumeric.Value),
                    Convert.ToDouble(aiMaximumValueNumeric.Value), AIVoltageUnits.Volts);
                readInTask.AIChannels.CreateVoltageChannel(aiChannel2ComboBox.Text, "",
                    AITerminalConfiguration.Rse, Convert.ToDouble(aiMinimumValueNumeric.Value),
                    Convert.ToDouble(aiMaximumValueNumeric.Value), AIVoltageUnits.Volts);
                readInTask.AIChannels.CreateVoltageChannel(aiChannel3ComboBox.Text, "",
                    AITerminalConfiguration.Rse, Convert.ToDouble(aiMinimumValueNumeric.Value),
                    Convert.ToDouble(aiMaximumValueNumeric.Value), AIVoltageUnits.Volts);
                if (finiteRadioButton.Checked)
                {
                    //Configure the timing parameters
                    readInTask.Timing.ConfigureSampleClock("", Convert.ToDouble(aiRateNumeric.Value),
                        SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, Convert.ToInt32(samplesPerChannelNumeric.Value));
                }
                else
                {
                    //Configure the timing parameters
                    readInTask.Timing.ConfigureSampleClock("", Convert.ToDouble(aiRateNumeric.Value),
                        SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, Convert.ToInt32(samplesPerChannelNumeric.Value));
                }

                //Verify the Task
                readInTask.Control(TaskAction.Verify);

                //Prepare the table and file for Data
                String[] channelNames = new String[readInTask.AIChannels.Count];
                int i = 0;
                foreach (AIChannel a in readInTask.AIChannels)
                {
                    channelNames[i++] = a.PhysicalName;
                }

                InitializeDataTable(channelNames, ref dataTable);

                // Add the channel names (and any other information) to the file
                int samples = Convert.ToInt32(samplesPerChannelNumeric.Value);
                PrepareFileForData();
                savedData = new ArrayList();
                for (i = 0; i < readInTask.AIChannels.Count; i++)
                {
                    savedData.Add(new ArrayList());
                }
                analogInReader = new AnalogMultiChannelReader(readInTask.Stream);

                if (!finiteRadioButton.Checked)
                {
                    analogCallback = new AsyncCallback(AnalogInCallback);

                    analogInReader.BeginReadMultiSample(samples, analogCallback, readInTask);
                }
                else
                {
                    data = analogInReader.ReadMultiSample(Convert.ToInt32(samplesPerChannelNumeric.Value));
                    LogData(data);
                    CloseFile();
                    readInTask.Dispose();
                    aquireStartButton.Enabled = true;
                    aquireStopButton.Enabled = false;
                }

            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void AnalogInCallback(IAsyncResult ar)
        {
            try
            {
                if (readInTask == ar.AsyncState)
                {
                    //Read the available data from the channels
                    data = analogInReader.EndReadMultiSample(ar);

                    LogData(data);

                    analogInReader.BeginReadMultiSample(Convert.ToInt32(samplesPerChannelNumeric.Value),
                        analogCallback, readInTask);
                }
            }
            catch (DaqException exception)
            {
                //Display Errors
                MessageBox.Show(exception.Message);
                readInTask.Dispose();
                aquireStopButton.Enabled = false;
                aquireStartButton.Enabled = true;
            }
        }
        private void LogData(double[,] data)
        {
            int channelCount = data.GetLength(0);
            int dataCount = data.GetLength(1);

            for (int i = 0; i < channelCount; i++)
            {
                ArrayList l = savedData[i] as ArrayList;

                for (int j = 0; j < dataCount; j++)
                {
                    l.Add(data[i, j]);
                }
            }
        }

        private void aquireStopButton_Click(object sender, EventArgs e)
        {
            CloseFile();
            readInTask.Dispose();
            readInTask = null;
            aquireStartButton.Enabled = true;
            aquireStopButton.Enabled = false;    
        }

        private void browseWriteButton_Click(object sender, EventArgs e)
        {
            writeToFileSaveFileDialog.DefaultExt = "*.txt";
            writeToFileSaveFileDialog.FileName = "acquisitionData.txt";
            writeToFileSaveFileDialog.Filter = "Text Files|*.txt|All Files|*.*";

            DialogResult result = writeToFileSaveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                fileNameWrite = writeToFileSaveFileDialog.FileName;
                filePathWriteTextBox.Text = fileNameWrite;
                fileToolTip.SetToolTip(filePathWriteTextBox, fileNameWrite);
            }
            aquireStartButton.Enabled = true;
        }

        //Create a text stream based on the user selections
        private bool CreateDataFile()
        {
            try
            {
                FileStream fs = new FileStream(fileNameWrite, FileMode.Create);
                fileStreamWriter = new StreamWriter(fs);
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show(ex.Message);
                readInTask.Dispose();
                return false;
            }

            return true;
        }

        public void InitializeDataTable(String[] channelNames, ref DataTable data)
        {
            int numChannels = channelNames.GetLength(0);
            data.Rows.Clear();
            data.Columns.Clear();
            dataColumn = new DataColumn[numChannels];
            int numOfRows = 10;

            for (int i = 0; i < numChannels; i++)
            {   
                dataColumn[i] = new DataColumn();
                dataColumn[i].DataType = typeof(double);
                dataColumn[i].ColumnName = channelNames[i];
            }

            data.Columns.AddRange(dataColumn); 

            for (int i = 0; i < numOfRows; i++)             
            {
                object[] rowArr = new object[numChannels];
                data.Rows.Add(rowArr);              
            }
        }
        private void PrepareFileForData()
        {
            //Prepare file for data (Write out the channel names
            int numChannels = readInTask.AIChannels.Count;

            for (int i = 0; i < numChannels; i++)
            {
                fileStreamWriter.Write(readInTask.AIChannels[i].PhysicalName);
                fileStreamWriter.Write("\t");
            }
            fileStreamWriter.WriteLine();
        }
        private void CloseFile()
        {
            int channelCount = savedData.Count;
            int dataCount = (savedData[0] as ArrayList).Count;

            try
            {
                fileStreamWriter.WriteLine("Samples: " + dataCount.ToString());

                for (int i = 0; i < dataCount; i++)
                {
                    for (int j = 0; j < channelCount; j++)
                    {
                        // Writes data to file
                        ArrayList l = savedData[j] as ArrayList;
                        double dataValue = (double)l[i];
                        fileStreamWriter.Write(dataValue.ToString("e6"));
                        fileStreamWriter.Write("\t"); //seperate the data for each channel
                    }
                    fileStreamWriter.WriteLine(); //new line of data (start next scan)
                }
                fileStreamWriter.Close();
            }
            catch(Exception e)
            {
                readInTask.Dispose();
                MessageBox.Show(e.TargetSite.ToString());
            }
        }
        #endregion
    }
        

}
