using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using NationalInstruments;
using NationalInstruments.CWIMAQControls;
using System.IO;


namespace CameraHardwareControl
{
	/// <summary>
	/// Snap using NI-IMAQ for IEEE 1394
	/// </summary>
	public class ControlWindow : System.Windows.Forms.Form
	{
        public Controller controller;

        private NationalInstruments.CWIMAQControls.AxCWIMAQViewer axCWIMAQViewer1;
		public System.Windows.Forms.TextBox InterfaceName;
		public System.Windows.Forms.Button StopButton;
		public System.Windows.Forms.Button SnapButton;
		public System.Windows.Forms.Button QuitButton;
		public System.Windows.Forms.Button GrabButton;
		private System.ComponentModel.IContainer components;
		public System.Windows.Forms.Label Label;
		public System.Windows.Forms.Timer Timer1;
		public NationalInstruments.CWIMAQControls.CWIMAQImageClass currentImage;
        public NationalInstruments.CWIMAQControls.CWIMAQImageClass myImage2;
     

        public Object[] imageArray;
        public Object[] imageData;
        public ArrayList imageSequences = new ArrayList();
        public int scanCount = 0;
    

		public uint sid;	
		public int bufferNumber;
		public int errorCode;
        private HScrollBar ImageList;
        private Button AquireSequenceButton;
        private Label label1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem saveImageSequenceToolStripMenuItem;
        private Label label2;
        private ToolStripMenuItem saveLatestImageSequenceToolStripMenuItem;
        private ToolStripMenuItem saveAllScanSequenceImagesToolStripMenuItem;
        private Button button2;
		public string errorMessage;



		public ControlWindow()
		{
				
			InitializeComponent();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlWindow));
            this.axCWIMAQViewer1 = new NationalInstruments.CWIMAQControls.AxCWIMAQViewer();
            this.InterfaceName = new System.Windows.Forms.TextBox();
            this.Label = new System.Windows.Forms.Label();
            this.SnapButton = new System.Windows.Forms.Button();
            this.QuitButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.GrabButton = new System.Windows.Forms.Button();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.ImageList = new System.Windows.Forms.HScrollBar();
            this.AquireSequenceButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImageSequenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.saveLatestImageSequenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllScanSequenceImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.axCWIMAQViewer1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // axCWIMAQViewer1
            // 
            this.axCWIMAQViewer1.Enabled = true;
            this.axCWIMAQViewer1.Location = new System.Drawing.Point(8, 26);
            this.axCWIMAQViewer1.Name = "axCWIMAQViewer1";
            this.axCWIMAQViewer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCWIMAQViewer1.OcxState")));
            this.axCWIMAQViewer1.Size = new System.Drawing.Size(689, 503);
            this.axCWIMAQViewer1.TabIndex = 0;
            // 
            // InterfaceName
            // 
            this.InterfaceName.AcceptsReturn = true;
            this.InterfaceName.BackColor = System.Drawing.SystemColors.Window;
            this.InterfaceName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.InterfaceName.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InterfaceName.ForeColor = System.Drawing.SystemColors.WindowText;
            this.InterfaceName.Location = new System.Drawing.Point(8, 552);
            this.InterfaceName.MaxLength = 0;
            this.InterfaceName.Name = "InterfaceName";
            this.InterfaceName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.InterfaceName.Size = new System.Drawing.Size(129, 20);
            this.InterfaceName.TabIndex = 3;
            this.InterfaceName.Text = "cam0";
            // 
            // Label
            // 
            this.Label.BackColor = System.Drawing.SystemColors.Control;
            this.Label.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label.Location = new System.Drawing.Point(8, 536);
            this.Label.Name = "Label";
            this.Label.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label.Size = new System.Drawing.Size(81, 17);
            this.Label.TabIndex = 5;
            this.Label.Text = "Camera Name";
            // 
            // SnapButton
            // 
            this.SnapButton.BackColor = System.Drawing.SystemColors.Control;
            this.SnapButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.SnapButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SnapButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SnapButton.Location = new System.Drawing.Point(143, 533);
            this.SnapButton.Name = "SnapButton";
            this.SnapButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SnapButton.Size = new System.Drawing.Size(45, 33);
            this.SnapButton.TabIndex = 6;
            this.SnapButton.Text = "Snap";
            this.SnapButton.UseVisualStyleBackColor = false;
            this.SnapButton.Click += new System.EventHandler(this.SnapButton_Click);
            // 
            // QuitButton
            // 
            this.QuitButton.BackColor = System.Drawing.SystemColors.Control;
            this.QuitButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.QuitButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.QuitButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.QuitButton.Location = new System.Drawing.Point(636, 536);
            this.QuitButton.Name = "QuitButton";
            this.QuitButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.QuitButton.Size = new System.Drawing.Size(50, 33);
            this.QuitButton.TabIndex = 7;
            this.QuitButton.Text = "Quit";
            this.QuitButton.UseVisualStyleBackColor = false;
            this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.BackColor = System.Drawing.SystemColors.Control;
            this.StopButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.StopButton.Enabled = false;
            this.StopButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StopButton.Location = new System.Drawing.Point(569, 536);
            this.StopButton.Name = "StopButton";
            this.StopButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StopButton.Size = new System.Drawing.Size(42, 33);
            this.StopButton.TabIndex = 9;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = false;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // GrabButton
            // 
            this.GrabButton.BackColor = System.Drawing.SystemColors.Control;
            this.GrabButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.GrabButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GrabButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.GrabButton.Location = new System.Drawing.Point(517, 536);
            this.GrabButton.Name = "GrabButton";
            this.GrabButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GrabButton.Size = new System.Drawing.Size(46, 33);
            this.GrabButton.TabIndex = 10;
            this.GrabButton.Text = "Grab";
            this.GrabButton.UseVisualStyleBackColor = false;
            this.GrabButton.Click += new System.EventHandler(this.GrabButton_Click);
            // 
            // Timer1
            // 
            this.Timer1.Interval = 5;
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // ImageList
            // 
            this.ImageList.LargeChange = 1;
            this.ImageList.Location = new System.Drawing.Point(347, 575);
            this.ImageList.Maximum = 5;
            this.ImageList.Name = "ImageList";
            this.ImageList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ImageList.Size = new System.Drawing.Size(80, 16);
            this.ImageList.TabIndex = 8;
            this.ImageList.TabStop = true;
            this.ImageList.Scroll += new System.Windows.Forms.ScrollEventHandler(this.imageList_Scroll);
            // 
            // AquireSequenceButton
            // 
            this.AquireSequenceButton.Location = new System.Drawing.Point(227, 533);
            this.AquireSequenceButton.Name = "AquireSequenceButton";
            this.AquireSequenceButton.Size = new System.Drawing.Size(101, 27);
            this.AquireSequenceButton.TabIndex = 11;
            this.AquireSequenceButton.Text = "Start Sequence";
            this.AquireSequenceButton.UseVisualStyleBackColor = true;
            this.AquireSequenceButton.Click += new System.EventHandler(this.AquireSequenceButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(430, 578);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Scroll Frames";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(707, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveImageSequenceToolStripMenuItem,
            this.saveLatestImageSequenceToolStripMenuItem,
            this.saveAllScanSequenceImagesToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveImageSequenceToolStripMenuItem
            // 
            this.saveImageSequenceToolStripMenuItem.Name = "saveImageSequenceToolStripMenuItem";
            this.saveImageSequenceToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.saveImageSequenceToolStripMenuItem.Text = "Save Snap Image";
            this.saveImageSequenceToolStripMenuItem.Click += new System.EventHandler(this.saveImageSequenceToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(334, 536);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 17;
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // saveLatestImageSequenceToolStripMenuItem
            // 
            this.saveLatestImageSequenceToolStripMenuItem.Name = "saveLatestImageSequenceToolStripMenuItem";
            this.saveLatestImageSequenceToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.saveLatestImageSequenceToolStripMenuItem.Text = "Save Latest Image Sequence";
            this.saveLatestImageSequenceToolStripMenuItem.Click += new System.EventHandler(this.saveLatestImageSequenceToolStripMenuItem_Click);
            // 
            // saveAllScanSequenceImagesToolStripMenuItem
            // 
            this.saveAllScanSequenceImagesToolStripMenuItem.Name = "saveAllScanSequenceImagesToolStripMenuItem";
            this.saveAllScanSequenceImagesToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.saveAllScanSequenceImagesToolStripMenuItem.Text = "Save All Scan Sequence Images";
            this.saveAllScanSequenceImagesToolStripMenuItem.Click += new System.EventHandler(this.saveAllScanSequenceImagesToolStripMenuItem_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(227, 566);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(101, 25);
            this.button2.TabIndex = 19;
            this.button2.Text = "Stop Sequence";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // ControlWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(707, 646);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.AquireSequenceButton);
            this.Controls.Add(this.ImageList);
            this.Controls.Add(this.GrabButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.QuitButton);
            this.Controls.Add(this.SnapButton);
            this.Controls.Add(this.Label);
            this.Controls.Add(this.InterfaceName);
            this.Controls.Add(this.axCWIMAQViewer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ControlWindow";
            this.Text = "Snap 1394";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axCWIMAQViewer1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        ///// <summary>
        ///// The main entry point for the application.
        ///// </summary>
        //[STAThread]
        //static void Main()
        //{

        //    Application.Run(new ControlWindow());
        //}


		private void DisplayError(int x, out string y)
		{
			CWIMAQ1394.ShowErrorCW(x, out y);
			MessageBox.Show(y);
		}


        public void aquireSequence(int numFrames)
        {

            label2.Text = "Aquiring Sequence";
            label2.Update();


            imageArray = new Object[numFrames];

            for (int i = 0; i <= numFrames-1; i++)
            {
               imageArray[i] = new NationalInstruments.CWIMAQControls.CWIMAQImage();
            }

            ImageList.Value = ImageList.Minimum;
            ImageList.Maximum = numFrames - 1;


            errorCode = CWIMAQ1394.CameraOpen2(InterfaceName.Text, CWIMAQ1394.CameraMode.IMG1394_CAMERA_MODE_CONTROLLER, out sid);
            if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
                DisplayError(errorCode, out errorMessage);

            errorCode = CWIMAQ1394.TriggerConfigure(sid, CWIMAQ1394.TriggerPolarity.IMG1394_TRIG_POLAR_ACTIVEL, 60000, CWIMAQ1394.TriggerMode.IMG1394_TRIG_MODE1, 1);
            if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
                DisplayError(errorCode, out errorMessage);

            errorCode = CWIMAQ1394.SetupSequenceCW(sid, ref imageArray, numFrames, 0);
            if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
                DisplayError(errorCode, out errorMessage);
         
        }

        internal void DumpAndDisplay()
        {
            label2.Text = "Sequence Complete";
            label2.Update();

            imageSequences.Add(imageArray);

            ImageList_Change(0);

            scanCount++;

            CWIMAQ1394.Close(sid);

        }

        public void CloseCamera()
        {
            //errorCode = CWIMAQ1394.StopAcquisition(sid);

            //if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
            //    DisplayError(errorCode, out errorMessage);

            //CWIMAQ1394.Close(sid);
        }

        private void indefiniteSynchrnousAquisition()
        {

            currentImage = new NationalInstruments.CWIMAQControls.CWIMAQImageClass();

            errorCode = CWIMAQ1394.CameraOpen2(InterfaceName.Text, CWIMAQ1394.CameraMode.IMG1394_CAMERA_MODE_CONTROLLER, out sid);
            if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
                DisplayError(errorCode, out errorMessage);

            errorCode = CWIMAQ1394.TriggerConfigure(sid, CWIMAQ1394.TriggerPolarity.IMG1394_TRIG_POLAR_ACTIVEL, 60000, CWIMAQ1394.TriggerMode.IMG1394_TRIG_MODE1, 1);
            if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
                DisplayError(errorCode, out errorMessage);

            errorCode = CWIMAQ1394.SnapCW(sid, currentImage);
            if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
                DisplayError(errorCode, out errorMessage);

            imageSequences.Add(currentImage);

        }

        

		private void SnapButton_Click(object sender, System.EventArgs e)
		{

            ImageList.Enabled = false;

			currentImage = new NationalInstruments.CWIMAQControls.CWIMAQImageClass();
            axCWIMAQViewer1.Attach(currentImage);

			errorCode = CWIMAQ1394.CameraOpen2(InterfaceName.Text, CWIMAQ1394.CameraMode.IMG1394_CAMERA_MODE_CONTROLLER, out sid);
			if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD) 
				DisplayError(errorCode, out errorMessage);


            errorCode = CWIMAQ1394.SnapCW(sid, currentImage);
            if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
                DisplayError(errorCode, out errorMessage);

             CWIMAQ1394.Close(sid);

		}
        private void ImageList_Change(int newScrollValue)
        {
            currentImage = (NationalInstruments.CWIMAQControls.CWIMAQImageClass)(imageArray[newScrollValue]);
            axCWIMAQViewer1.Attach(currentImage);
        }

        private void ImageList_Change(Object[] obj, int newScrollValue)
        {

            currentImage = (NationalInstruments.CWIMAQControls.CWIMAQImageClass)(obj[newScrollValue]);
            axCWIMAQViewer1.Attach(currentImage);
        }

         
		private void QuitButton_Click(object sender, System.EventArgs e)
		{
			if (Timer1.Enabled)
				StopButton_Click(StopButton, new System.EventArgs());

			Application.Exit();
		}

		private void GrabButton_Click(object sender, System.EventArgs e)
		{
			currentImage = new NationalInstruments.CWIMAQControls.CWIMAQImageClass();			
			axCWIMAQViewer1.Attach(currentImage);

			errorCode = CWIMAQ1394.CameraOpen2(InterfaceName.Text, CWIMAQ1394.CameraMode.IMG1394_CAMERA_MODE_CONTROLLER, out sid);
			if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD) 
				DisplayError(errorCode, out errorMessage);
						
			errorCode = CWIMAQ1394.SetupGrabCW(sid);
			if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD) 
				DisplayError(errorCode, out errorMessage);

       

			
			Timer1.Enabled = true;
        	GrabButton.Enabled = false;
			SnapButton.Enabled = false;
            StopButton.Enabled = true;
		}

        private void grabTriggeredImages(){

            currentImage = new NationalInstruments.CWIMAQControls.CWIMAQImageClass();			
			
			errorCode = CWIMAQ1394.CameraOpen2(InterfaceName.Text, CWIMAQ1394.CameraMode.IMG1394_CAMERA_MODE_CONTROLLER, out sid);
			if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD) 
				DisplayError(errorCode, out errorMessage);

            errorCode = CWIMAQ1394.TriggerConfigure(sid, CWIMAQ1394.TriggerPolarity.IMG1394_TRIG_POLAR_ACTIVEL, 60000, CWIMAQ1394.TriggerMode.IMG1394_TRIG_MODE1, 1);
            if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD)
                DisplayError(errorCode, out errorMessage);
						
			errorCode = CWIMAQ1394.SetupGrabCW(sid);
			if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD) 
				DisplayError(errorCode, out errorMessage);

            imageSequences.Add(currentImage);
			
			Timer1.Enabled = true;
        	GrabButton.Enabled = false;
			SnapButton.Enabled = false;
            StopButton.Enabled = true;
        }

        public void SaveData()
        {
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Title = "Save image data";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDialog1.FileName != "")
                    {
                        SaveData(saveFileDialog1.FileName);
                    }

                }
            }
        }


        public void SaveDataSequence()
        {
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Title = "Save image data";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDialog1.FileName != "")
                    {
                        SaveDataSequence(saveFileDialog1.FileName);
                    }

                }
            }
        }

        public string GetSaveDialogFilename()
        {
            string file = "";
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Title = "Save image data";
               
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDialog1.FileName != "")
                    {
                        file = saveFileDialog1.FileName;
                    }
                }
            }
            return file;
        }

        public void SaveScanSequenceImages()
        {
            string filename = GetSaveDialogFilename();

            for(int i = 0; i<imageSequences.Count; i++)
            {
                string scanIndexedFilename = filename+"_scan_" + i.ToString();
                SaveDataSequence(scanIndexedFilename);
            }
            
        }

        public void SaveSnapImage()
        {
            string filename = GetSaveDialogFilename();
            SaveData(filename);

        }


        public void SaveData(string filename)
        {
            string indexedFilename = filename+".dat";

            Byte[,] arrImage = (Byte[,])(currentImage.ImageToArray(0, 0, currentImage.Width, currentImage.Height));
            byte[,] arrayImage = (byte[,])(arrImage);
            byte[] flatImage = new byte[currentImage.Height * currentImage.Width];

            int byteIndex = 0;
            for (int i = 0; i < arrayImage.GetLength(1); i++)
            {
                for (int j = 0; j < arrayImage.GetLength(0); j++)
                {
                    flatImage[byteIndex] = arrayImage[j, i];
                    byteIndex++;
                }
            }
            File.WriteAllBytes(indexedFilename, flatImage);
        
        }

        
        public void SaveDataSequence(string filename)
        {
            for (int k = 0; k <= imageArray.Length - 1; k++)
            {
                string indexedFilename = filename + "_image_" + k.ToString() + ".dat";

                currentImage = (NationalInstruments.CWIMAQControls.CWIMAQImageClass)(imageArray[k]);
                Byte[,] arrImage = (Byte[,])(currentImage.ImageToArray(0, 0, currentImage.Width, currentImage.Height));
                byte[,] arrayImage = (byte[,])(arrImage);
                byte[] flatImage = new byte[currentImage.Height * currentImage.Width];

                int byteIndex = 0;
                for (int i = 0; i < arrayImage.GetLength(1); i++)
                {
                    for (int j = 0; j < arrayImage.GetLength(0); j++)
                    {
                        flatImage[byteIndex] = arrayImage[j, i];
                        byteIndex++;
                    }
                }
                File.WriteAllBytes(indexedFilename, flatImage);
            }
         
        }



		private void StopButton_Click(object sender, System.EventArgs e)
		{
			Timer1.Enabled = false;
			GrabButton.Enabled = true;
			SnapButton.Enabled = true;
			StopButton.Enabled = false;

			CWIMAQ1394.Close(sid);
		}

		private void Timer1_Tick(object sender, System.EventArgs e)
		{
			errorCode = CWIMAQ1394.Grab2CW(sid, 1, ref bufferNumber, currentImage);
			if (errorCode != CWIMAQ1394.ErrorCodes.IMG1394_ERR_GOOD) 
			{
				DisplayError(errorCode, out errorMessage);
				StopButton_Click(StopButton, new System.EventArgs());
			}
		}

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void imageList_Scroll(object sender, ScrollEventArgs e)
        {
            ImageList_Change(e.NewValue);
        }

        private void AquireSequenceButton_Click(object sender, EventArgs e)
        {
           
            ImageList.Enabled = true;
            aquireSequence(3);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            indefiniteSynchrnousAquisition();
        }



        private void saveImageSequenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
             SaveData();
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void sequenceChooser_SelectedIndexChanged(object sender, EventArgs e)
        {
        
        }

        public void clearSequences()
        {
            imageSequences.Clear();
            scanCount = 0;
        }

        private void saveLatestImageSequenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveDataSequence();
        }

        private void saveAllScanSequenceImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveScanSequenceImages();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DumpAndDisplay();
        }
    }
}
