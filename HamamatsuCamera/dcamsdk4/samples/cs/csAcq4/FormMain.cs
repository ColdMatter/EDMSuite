using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using System.Threading;
using System.Drawing.Imaging;

using Hamamatsu.DCAM4;
using Hamamatsu.subacq4;
using static System.Windows.Forms.AxHost;
using System.IO;

//using ScanMaster.Acquire.Plugin;

namespace csAcq4
{
    public partial class FormMain : Form
    {
        //rhys add
        public CCDController controller;
        private enum FormStatus
        {
            Startup,                // After startup or camapi_uninit()
            Initialized,            // After dcamapi_init() or dcamdev_close()
            Opened,                 // After dcamdev_open() or dcamcap_stop() without any image
            Acquiring,              // After dcamcap_start()
            AcquiringSoftwareTrigger,
            Acquired                // After dcamcap_stop() with image
        }

        private MyDcam mydcam;
        private MyDcamWait mydcamwait;
        private MyImage m_image;
        private MyLut m_lut;
        private readonly object BitmapLock;

        private struct MyLut
        {
            public int camerabpp;          // camera bit per pixel.  This sample code only support MONO.
            public int cameramax;

            public int inmax;
            public int inmin;
        };

        private int m_indexCamera = 0;          // index of DCAM device.  This is used at allocating mydcam instance.
        private int m_nFrameCount;              // frame count of allocation buffer for DCAM capturing
        private FormStatus m_formstatus;        // Indicate current Form status. For setting, Use MyFormStatus() functions
        private Thread m_threadCapture;         // System.Threading.  Assigned for monitoring updated frames during capturing
        private Bitmap m_bitmap;                // bitmap data for displaying in this Windows Form
        private bool m_cap_stopping = false;    // reset to false when starting capture and set to true if stopping capture

        //private FormProperties formProperties;  // Properties Form

        // ---------------- private class MyImage ----------------

        private class MyImage
        {
            public DCAMBUF_FRAME bufframe;
            public MyImage()
            {
                bufframe = new DCAMBUF_FRAME(0);
            }
            public int width { get { return bufframe.width; } }
            public int height { get { return bufframe.height; } }
            public DCAM_PIXELTYPE pixeltype { get { return bufframe.type; } }
            public bool isValid()
            {
                if (width <= 0 || height <= 0 || pixeltype == DCAM_PIXELTYPE.MONO16)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            public void clear()
            {
                bufframe.width = 0;
                bufframe.height = 0;
                bufframe.type = DCAM_PIXELTYPE.MONO16;
            }
            public void set_iFrame(int index)
            {
                bufframe.iFrame = index;
            }
        }

        // ---------------- Local functions ----------------

        // My Form Status helper
        private void MyFormStatus(FormStatus status)
        {
            Boolean isStartup = (status == FormStatus.Startup);
            Boolean isInitialized = (status == FormStatus.Initialized);
            Boolean isOpened = (status == FormStatus.Opened);
            Boolean isAcquiring = (status == FormStatus.Acquiring) || (status == FormStatus.AcquiringSoftwareTrigger);
            Boolean isAcquired = (status == FormStatus.Acquired);
            Boolean isAcquiringSoftwareTrigger = (status == FormStatus.AcquiringSoftwareTrigger);
            PushInit.Enabled = isStartup;
            // Shirley added the constraints below on 26/02 to improve the stability of the program
            comboTriggerSource.Enabled = isInitialized || isAcquired;
            QueryFrameCountButton.Enabled = isInitialized || isAcquired;
            UpdateFrameCountButton.Enabled = isInitialized || isAcquired;
            FrameCountTextBox.Enabled = isInitialized || isAcquired;
            QueryNumSnapsButton.Enabled = isInitialized || isAcquired;
            UpdateNumSnapsButton.Enabled = isInitialized || isAcquired;
            NumSnapsTextBox.Enabled = isInitialized || isAcquired;
            QuerySensitivityGainButton.Enabled = isInitialized || isAcquired;
            UpdateSensitivityGainButton.Enabled = isInitialized || isAcquired;
            QueryExposureTimeButton.Enabled = isInitialized || isAcquired;
            UpdateExposureTimeButton.Enabled = isInitialized || isAcquired;
            QuerySensorTemperatureButton.Enabled = isInitialized || isAcquired;
            ContinuousSnapAndSaveButton.Enabled = isInitialized || isAcquired;
            PushSnap.Enabled = isInitialized || isAcquired;
            PushLive.Enabled = isInitialized || isAcquired;
            //PushIdle.Enabled = isAcquiring;
            //PushBufRelease.Enabled = isAcquired; 
            PushClose.Enabled = isInitialized || isAcquired;
            //PushUninit.Enabled = isInitialized; rhys remove 14/02

            //PushProperties.Enabled = (isInitialized || isAcquired);

            if (isInitialized || isStartup)
            {
                // acquisition is not starting
                MyThreadCapture_Abort();
            }

            m_formstatus = status;
        }
        public void MyFormStatus_Startup() { MyFormStatus(FormStatus.Startup); }
        public void MyFormStatus_Initialized() { MyFormStatus(FormStatus.Initialized); }
        public void MyFormStatus_Opened() { MyFormStatus(FormStatus.Opened); }
        public void MyFormStatus_Acquiring() { MyFormStatus(FormStatus.Acquiring); }
        public void MyFormStatus_AcquiringSoftwareTrigger() { MyFormStatus(FormStatus.AcquiringSoftwareTrigger); }
        public void MyFormStatus_Acquired() { MyFormStatus(FormStatus.Acquired); }

        public Boolean IsMyFormStatus_Startup() { return (m_formstatus == FormStatus.Startup); }
        public Boolean IsMyFormStatus_Initialized() { return (m_formstatus == FormStatus.Initialized); }
        private Boolean IsMyFormStatus_Opened() { return (m_formstatus == FormStatus.Opened); }
        public Boolean IsMyFormStatus_Acquiring() { return (m_formstatus == FormStatus.Acquiring) || (m_formstatus == FormStatus.AcquiringSoftwareTrigger); }
        public Boolean IsMyFormStatus_Acquired() { return (m_formstatus == FormStatus.Acquired); }

        // Display status
        private void MyShowStatus(string text) { LabelStatus.Text = text; }
        private void MyShowStatusOK(string text) { MyShowStatus("OK: " + text); }
        private void MyShowStatusNG(string text, DCAMERR err)
        {
            if (err == DCAMERR.SUCCESS)
            {
                Console.WriteLine($"Debug: {text} completed successfully with code 0x{(int)err:X8}.");
                return; // No error; skip showing NG status
            }

            MyShowStatus(String.Format("NG: 0x{0:X8}:{1}", (int)err, text));
        }

        // update LUT condition
        //private void update_lut(bool bUpdatePicture)
        //{
        //    if (mydcam != null)
        //    {
        //        MyDcamProp prop = new MyDcamProp(mydcam, DCAMIDPROP.BITSPERCHANNEL);

        //        double v = 0;
        //        prop.getvalue(ref v);
        //        bool reset = false;

        //        // Check if bit depth has changed 
        //        if (m_lut.camerabpp > 0 && m_lut.camerabpp != (int)v)
        //        {
        //            reset = true;
        //        }
        //        m_lut.camerabpp = (int)v;
        //        m_lut.cameramax = (1 << m_lut.camerabpp) - 1;

        //        m_lut.inmax = HScrollLutMax.Value;
        //        m_lut.inmin = HScrollLutMin.Value;

        //        HScrollLutMax.Maximum = m_lut.cameramax;
        //        HScrollLutMin.Maximum = m_lut.cameramax;

        //        if (reset)
        //        {
        //            HScrollLutMax.Value = m_lut.cameramax;
        //            HScrollLutMin.Value = 0;
        //            bUpdatePicture = true;
        //        }

        //        if (bUpdatePicture)
        //            MyUpdatePicture();
        //    }
        //}

        // auto LUT condition
        private void auto_lut()
        {

            bool bUpdatePicture = false;
            int min = m_lut.cameramax;
            int max = 0;


            if (m_image.isValid())
            {
                int w = PicDisplay.Size.Width;
                int h = PicDisplay.Size.Height;
                //if (w > m_image.width) w = m_image.width;
                //if (h > m_image.height) h = m_image.height;

                if (m_image.bufframe.buf == IntPtr.Zero) // Add a check for invalid buffer memory
                {
                    Console.WriteLine("Error: bufframe.buf is NULL!");
                    return;
                }
                Int16[] s = new Int16[w];

                // Displaying center of the image
                Int32 y0 = (m_image.height - h) / 2;
                Int32 x0 = (m_image.width - w) / 2;

                // Ensure the region is within bounds
                if (y0 < 0) y0 = 0;
                if (x0 < 0) x0 = 0;
                if (y0 + h > m_image.height) h = m_image.height - y0;
                if (x0 + w > m_image.width) w = m_image.width - x0;

                Int32 y;
                for (y = 0; y < h; y++)
                {
                    Int32 offset;

                    offset = m_image.bufframe.rowbytes * (y + y0) + (x0 * 2);// In bytes, so multiply by bpp

                    // Check offset validity before copying
                    if (offset + (w * 2) > m_image.bufframe.rowbytes * m_image.height)
                    {
                        Console.WriteLine("Error: Offset out of bounds!");
                        break;
                    }

                    Marshal.Copy((IntPtr)(m_image.bufframe.buf.ToInt64() + offset), s, 0, w);

                    Int32 x;
                    for (x = 0; x < w; x++)
                    {
                        UInt16 u = (UInt16)s[x];
                        if (u > max)
                            max = u;
                        else if (u < min)
                            min = u;
                    }
                }
            }

            if (m_lut.inmax != max)
            {
                m_lut.inmax = max;
                HScrollLutMax.Value = m_lut.inmax;
                EditLutMax.Text = m_lut.inmax.ToString();
                bUpdatePicture = true;
            }

            if (m_lut.inmin != min)
            {
                m_lut.inmin = min;
                HScrollLutMin.Value = m_lut.inmin;
                EditLutMin.Text = m_lut.inmin.ToString();
                bUpdatePicture = true;
            }

            if (m_lut.inmax - m_lut.inmin < 50) // Ensure there's a valid contrast
            {
                Console.WriteLine("Auto LUT failed: Low contrast detected, adjusting.");
                m_lut.inmax += 10;  // Small boost
                m_lut.inmin -= 10;
            }

            if (bUpdatePicture)
                MyUpdatePicture();
        }

        // Updating myimage by DCAM frame
        private void MyUpdateImage(int iFrame)
        {

            // lock selected frame by iFrame
            m_image.set_iFrame(iFrame);
            if (!mydcam.buf_lockframe(ref m_image.bufframe))
            {
                Console.WriteLine($"Failed to lock frame: {mydcam.m_lasterr}");
                m_image.clear();
                return;
            }

            MyUpdatePicture();
            //Console.WriteLine($"Locked frame dimensions: {m_image.width}x{m_image.height}");
        }

        // Draw Bitmap in the PictureBox 
        private delegate void MyDelegate_UpdateDisplay();

        private void MyUpdateDisplay()
        {
            if (m_bitmap == null)
            {
                Console.WriteLine("Warning: Bitmap is null, skipping update.");
                return;
            }

            //// Debug statements to print m_image properties
            //Console.WriteLine($"m_image width: {m_image.width}");
            //Console.WriteLine($"m_image height: {m_image.height}");
            //Console.WriteLine($"m_image pixeltype: {m_image.pixeltype}");

            Image oldImg = PicDisplay.Image;

            try
            {
                // Ensure dimensions are valid
                if (PicDisplay.Width <= 0 || PicDisplay.Height <= 0)
                {
                    Console.WriteLine("Error: Invalid PictureBox dimensions.");
                    return;
                }

                // Create a new bitmap. Ensure we always use the fixed PictureBox dimensions
                Bitmap bmp = new Bitmap(PicDisplay.Width, PicDisplay.Height, PixelFormat.Format16bppGrayScale);
                using (var gr = Graphics.FromImage(bmp))
                {
                    lock (BitmapLock)
                    {
                        gr.DrawImage(m_bitmap, 0, 0, PicDisplay.Width, PicDisplay.Height);
                    }
                }

                PicDisplay.Image = bmp;
                PicDisplay.Refresh();

                oldImg?.Dispose();
                //Console.WriteLine("PictureBox updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating or updating Bitmap: {ex.Message}");
            }
        }

        // Updating Bitmap and Display
        private void MyUpdatePicture()
        {
            if (m_image.isValid())
            {
                m_lut.inmax = HScrollLutMax.Value;
                m_lut.inmin = HScrollLutMin.Value;

                Rectangle rc = new Rectangle(0, 0, m_image.width, m_image.height);
                lock (BitmapLock)
                {
                    // Always initialize bitmap if it's null or size changes
                    if (m_bitmap == null || m_bitmap.Width != m_image.width || m_bitmap.Height != m_image.height)
                    {
                        Console.WriteLine("Initializing new bitmap...");
                        m_bitmap?.Dispose(); // Dispose old bitmap if it exists
                        m_bitmap = new Bitmap(m_image.width, m_image.height, PixelFormat.Format16bppGrayScale);
                    }

                    // Copy image data into Bitmap
                    SUBACQERR err = subacq.copydib(ref m_bitmap, m_image.bufframe, ref rc, m_lut.inmax, m_lut.inmin, m_lut.camerabpp);
                    if (err != SUBACQERR.SUCCESS)
                    {
                        Console.WriteLine($"Error copying DIB: {err}, using fallback blank image.");

                        // Fallback: Assign a blank image to avoid null issues
                        using (Graphics g = Graphics.FromImage(m_bitmap))
                        {
                            g.Clear(Color.Black);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Warning: Image is not valid, skipping update.");
                return;
            }

            if (PicDisplay.InvokeRequired)
            {
                PicDisplay.Invoke(new MyDelegate_UpdateDisplay(MyUpdateDisplay));
                return;
            }

            MyUpdateDisplay();
        }


        // Capturing Thread helper functions
        private void MyThreadCapture_Start()
        {
            m_threadCapture = new Thread(new ThreadStart(OnThreadCapture));

            m_threadCapture.IsBackground = true;
            m_threadCapture.Start();
        }
        void MyThreadCapture_Abort()
        {
            if (m_threadCapture != null)
            {
                if (mydcamwait != null)
                    mydcamwait.abort();

                m_threadCapture.Abort();
            }
        }

        // Updating myimage by DCAM frame
        private delegate void MyDelegate_SnapCaptureFinished();
        private void MySnapCaptureFinished()
        {
            if (InvokeRequired)
            {
                // worker thread calls this function
                Invoke(new MyDelegate_SnapCaptureFinished(MySnapCaptureFinished), null);
                return;
            }

            MyShowStatusOK($"{m_nFrameCount} frames have been taken.");
            MyFormStatus_Acquired();            // change dialog FormStatus to Acquired
        }

        private void OnThreadCapture()
        {
            bool bContinue = true;

            using (mydcamwait = new MyDcamWait(ref mydcam))
            {
                while (bContinue)
                {
                    DCAMWAIT eventmask = DCAMWAIT.CAPEVENT.FRAMEREADY | DCAMWAIT.CAPEVENT.STOPPED;
                    DCAMWAIT eventhappened = DCAMWAIT.NONE;

                    if (mydcamwait.start(eventmask, ref eventhappened))
                    {
                        if (eventhappened & DCAMWAIT.CAPEVENT.FRAMEREADY)
                        {
                            int iNewestFrame = 0;
                            int iFrameCount = 0;

                            if (mydcam.cap_transferinfo(ref iNewestFrame, ref iFrameCount))
                            {
                                MyUpdateImage(iNewestFrame);
                            }
                        }

                        if (eventhappened & DCAMWAIT.CAPEVENT.STOPPED)
                        {
                            bContinue = false;
                            if (m_cap_stopping == false && mydcam.m_capmode == DCAMCAP_START.SNAP)
                            {
                                // in this condition, cap_stop() happens automatically, so update the main dialog
                                MySnapCaptureFinished();
                            }
                        }
                    }
                    else
                    {
                        if (mydcamwait.m_lasterr == DCAMERR.TIMEOUT)
                        {
                            // nothing to do
                        }
                        else
                        if (mydcamwait.m_lasterr == DCAMERR.ABORT)
                        {
                            bContinue = false;
                        }
                    }
                }
            }
        }

        // ---------------- constructor of FormMain ----------------

        public FormMain(CCDController _controller)
        {
            InitializeComponent();
            controller = _controller;//new CCDController(); // Initialize CCD controller
            m_image = new MyImage();
            m_lut = new MyLut();
            BitmapLock = new object();
        }

        // ---------------- Windows Form Command Handler ----------------
        private void FormMain_Load(object sender, EventArgs e)
        {
            // Initialize form status
            MyFormStatus_Startup();
            m_nFrameCount = 20;

            // Update window title
            if (IntPtr.Size == 4)
            {
                Text = "UEDM Hamamatsu EMCCD Camera (32 bit)";
            }
            else
            if (IntPtr.Size == 8)
            {
                Text = "UEDM Hamamatsu EMCCD Camera (64 bit)";
            }

            comboTriggerSource.Items.Add("Internal Trigger");
            comboTriggerSource.Items.Add("External Start Trigger");
            comboTriggerSource.Items.Add("External Edge Trigger");
            comboTriggerSource.SelectedIndex = 0; // Default to Internal Trigger

            comboBoxCameraSelection.Items.Add("CCDA");
            comboBoxCameraSelection.Items.Add("CCDB");
            comboBoxCameraSelection.SelectedIndex = 0; // Default to CCDA

            // Auto-detect camera serial number
            // controller.SelectCamera(); // Detect camera using serial number
            comboBoxCameraSelection.SelectedIndex = controller.SelectedCamera; //Sync the selected camera index

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyThreadCapture_Abort();                // abort capturing thread if exist

            if (mydcam != null)                     // close device if exist
            {
                mydcam.dev_close();
                mydcam = null;
            }

            if (!IsMyFormStatus_Startup())          // uninitialize if necessary
            {
                MyDcamApi.uninit();
            }
        }

        // ---------------- DCAM-API related command handler ----------------


        private void PushInit_Click(object sender, EventArgs e)
        {
            //rhys commented out start - transfer over to ccd controller
            //// dcamapi_init() may takes for a few seconds
            //Cursor.Current = Cursors.WaitCursor;

            //// Step 1: Initialize Camera API
            //if (!MyDcamApi.init())
            //{
            //    MyShowStatusNG("Failed to initialise camera API", MyDcamApi.m_lasterr);
            //    Cursor.Current = Cursors.Default;
            //    return;                         // Fail: dcamapi_init()
            //}

            //// Success: dcamapi_init()

            //MyShowStatusOK("Camera API initialized successfully.");
            //MyFormStatus_Initialized();

            //// Step 2: Open the camera
            //MyFormStatus_Startup();     // change dialog FormStatus to Startup
            //MyDcam amydcam = new MyDcam();
            //if (!amydcam.dev_open(m_indexCamera))
            //{
            //    MyShowStatusNG("Failed to connect to the camera", amydcam.m_lasterr);
            //    MyDcamApi.uninit(); // Clean up on failure
            //    //amydcam = null;
            //    Cursor.Current = Cursors.Default;
            //    return;                         // fail: dcamdev_open()
            //}

            //// success: dcamdev_open()

            //mydcam = amydcam;                   // store mydcam instance

            //// Pass mydcam instance to CCDController
            //if (controller != null)
            //{
            //    controller.SetMyDcam(mydcam);
            //}
            //else
            //{
            //    Console.WriteLine("Error: CCDController is null when setting mydcam.");
            //}

            //MyShowStatusOK("Camera connected successfully.");
            //MyFormStatus_Opened();

            ////// Camera property configuration

            ////if (!ConfigureCamera())
            ////{
            ////    MyShowStatusNG("Camera configuration failed", mydcam.m_lasterr);
            ////    mydcam.dev_close();
            ////    mydcam = null;
            ////    MyDcamApi.uninit();
            ////    Cursor.Current = Cursors.Default;
            ////    return;
            ////}

            ////ApplySelectedTriggerSource(); // Apply the selected trigger source 

            ////MyShowStatusOK("Initialisation and camera configuration complete.");
            ////MyFormStatus_Initialized();
            ////Cursor.Current = Cursors.Default;
            //rhys commented out end  - transfer over to ccd controller

            controller.CameraInit();
        }



        //// The ConfigureCamera function sets the camera properties to the desired values as we do on the Capture tab in the software   
        //private bool ConfigureCamera()
        //{
        //    try
        //    {

        //        //// Set the Mono channel:1 
        //        //MyDcamProp channelProp = new MyDcamProp(mydcam, DCAMIDPROP.NUMBEROF_CHANNEL);
        //        //double currentValue = 1.0;
        //        //if (!channelProp.getvalue(ref currentValue))
        //        //{
        //        //    Console.WriteLine("NUMBEROF_CHANNEL not supported. Skipping...");
        //        //}
        //        //else
        //        //{
        //        //    // Set property if supported
        //        //    if (!channelProp.setvalue(1.0))
        //        //    {
        //        //        MyShowStatusNG("Failed to set camera to Mono: 1 Channel", channelProp.m_lasterr);
        //        //        return false; // Fail: Unable to set property
        //        //    }
        //        //    Console.WriteLine($"NUMBEROF_CHANNEL set successfully. Current value: {currentValue}");
        //        //}


        //        // ensure the camera is the correct model
        //        //MyDcamProp cameraname = new MyDcamProp(mydcam, DCAMIDSTR.MODEL);
        //        //MyDcamProp serialnumber = new MyDcamProp(mydcam, DCAMIDSTR.CAMERA_SERIESNAME);
        //        //string cameraname = mydcam.getstring(DCAMIDSTR.MODEL, ref cameraname);
        //        //string serialnumber = mydcam.getstring(DCAMIDSTR.CAMERA_SERIESNAME, ref serialnumber);
        //        //if (cameraname != "c9100-23b" || serialnumber != "000620")
        //        //{
        //        //    myshowstatus("camera model or serial number does not match");
        //        //    mydcam.dev_close();
        //        //    mydcam = null;
        //        //    cursor.current = cursors.default;
        //        //    return;                         // fail: incorrect camera
        //        //}

        //        // * Camera Control (Gain and Exposure Time) * //
        //        // Set CCD mode to EMCCD
        //        MyDcamProp ccdModeProp = new MyDcamProp(mydcam, DCAMIDPROP.CCDMODE);
        //        if (!ccdModeProp.setvalue(DCAMPROP.CCDMODE.EMCCD))
        //        {
        //            MyShowStatusNG("Failed to set CCD mode", ccdModeProp.m_lasterr);
        //            return false;                         // Fail: setting CCD mode
        //        }

        //        // Set direct gain mode to ON
        //        MyDcamProp directGainModeProp = new MyDcamProp(mydcam, DCAMIDPROP.DIRECTEMGAIN_MODE);
        //        if (!directGainModeProp.setvalue(DCAMPROP.MODE.ON))
        //        {
        //            MyShowStatusNG("Failed to set direct gain mode", directGainModeProp.m_lasterr);
        //            return false;                         // Fail: setting direct gain mode
        //        }
        //        // Set sensitivity gain
        //        MyDcamProp sensitivitygainprop = new MyDcamProp(mydcam, DCAMIDPROP.SENSITIVITY);
        //        if (!sensitivitygainprop.setvalue(100)) // Adjust sensitivity gain (0 to 1200 range)
        //        {
        //            MyShowStatusNG("Failed to set sensitivity gain", sensitivitygainprop.m_lasterr);
        //            return false; // Exit on failure
        //        }

        //        // Set exposure time mode to be ON such that exposure time can be controlled 
        //        MyDcamProp exposuremodeprop = new MyDcamProp(mydcam, DCAMIDPROP.EXPOSURETIME_CONTROL);
        //        if (!exposuremodeprop.setvalue(DCAMPROP.EXPOSURETIME_CONTROL.NORMAL))
        //        {
        //            MyShowStatusNG("Failed to set exposure mode", exposuremodeprop.m_lasterr);
        //            return false; // Exit on failure
        //        }

        //        // Set exposure time to 40 ms (entire TOF)
        //        MyDcamProp exposuretimeprop = new MyDcamProp(mydcam, DCAMIDPROP.EXPOSURETIME);
        //        if (!exposuretimeprop.setvalue(0.04)) // Adjust exposure time in seconds
        //        {
        //            MyShowStatusNG("Failed to set exposure time", exposuretimeprop.m_lasterr);
        //            return false; // Exit on failure
        //        }

        //        // Set internal trigger handling to faster frame rate such that the exposure time can be overlapped with the readout time
        //        MyDcamProp internalTriggerHandlingProp = new MyDcamProp(mydcam, DCAMIDPROP.INTERNALTRIGGER_HANDLING);
        //        if (!internalTriggerHandlingProp.setvalue(DCAMPROP.INTERNALTRIGGER_HANDLING.FASTERFRAMERATE))
        //        {
        //            MyShowStatusNG("Failed to set internal trigger handling", internalTriggerHandlingProp.m_lasterr);
        //            return false;                         // Fail: setting internal trigger handling
        //        }

        //        // * Binning and Subarray * //
        //        // Set binning size to 4x4
        //        MyDcamProp binningProp = new MyDcamProp(mydcam, DCAMIDPROP.BINNING);
        //        if (!binningProp.setvalue(4.0))
        //        {
        //            MyShowStatusNG("Failed to set binning size", binningProp.m_lasterr);
        //            return false;                         // Fail: setting binning size
        //        }

        //        // Set subarray HSIZE to 128
        //        MyDcamProp subarrayHSizeProp = new MyDcamProp(mydcam, DCAMIDPROP.SUBARRAYHSIZE);
        //        if (!subarrayHSizeProp.setvalue(128.0))
        //        {
        //            MyShowStatusNG("Failed to set subarray HSIZE", subarrayHSizeProp.m_lasterr);
        //            return false;                         // Fail: setting subarray HSIZE
        //        }

        //        // Set subarray VSIZE to 128
        //        MyDcamProp subarrayVSizeProp = new MyDcamProp(mydcam, DCAMIDPROP.SUBARRAYVSIZE);
        //        if (!subarrayVSizeProp.setvalue(128.0))
        //        {
        //            MyShowStatusNG("Failed to set subarray VSIZE", subarrayVSizeProp.m_lasterr);
        //            return false;                         // Fail: setting subarray VSIZE
        //        }

        //        // * Trigger Modes and Speed * //
        //        // Set readout speed to fastest (3)
        //        MyDcamProp readoutspeedprop = new MyDcamProp(mydcam, DCAMIDPROP.READOUTSPEED);
        //        if (!readoutspeedprop.setvalue((double)DCAMPROP.READOUTSPEED.FASTEST)) // Use correct constant for fastest speed
        //        {
        //            MyShowStatusNG("Failed to set readout speed", readoutspeedprop.m_lasterr);
        //            return false; // Exit on failure
        //        }

        //        //// Set input trigger connector to BNC
        //        //MyDcamProp triggerConnectorProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGER_CONNECTOR);
        //        //if (!triggerConnectorProp.setvalue(DCAMPROP.TRIGGER_CONNECTOR.BNC))
        //        //{
        //        //    MyShowStatusNG("Failed to set trigger connector", triggerConnectorProp.m_lasterr);
        //        //    return false;                         // Fail: setting trigger connector
        //        //}

        //        //// Set trigger source based on user selection (External by default)
        //        //bool useExternalTrigger = true; // Change this dynamically based on user settings

        //        //MyDcamProp triggerSourceProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);

        //        //if (useExternalTrigger)
        //        //{
        //        //    if (triggerSourceProp.setvalue(DCAMPROP.TRIGGERSOURCE.EXTERNAL))
        //        //    {
        //        //        Console.WriteLine(" Trigger source successfully set to EXTERNAL.");
        //        //    }
        //        //    else
        //        //    {
        //        //        MyShowStatusNG(" Failed to set trigger source to EXTERNAL", triggerSourceProp.m_lasterr);
        //        //        return false;
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    if (triggerSourceProp.setvalue(DCAMPROP.TRIGGERSOURCE.INTERNAL))
        //        //    {
        //        //        Console.WriteLine(" Trigger source successfully set to INTERNAL.");
        //        //    }
        //        //    else
        //        //    {
        //        //        MyShowStatusNG(" Failed to set trigger source to INTERNAL", triggerSourceProp.m_lasterr);
        //        //        return false;
        //        //    }
        //        //}

        //        //ApplySelectedTriggerSource();

        //        //// Set trigger mode to be normal (allowing to have external edge trigger)
        //        //    MyDcamProp triggerModeProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGER_MODE);
        //        //    if (!triggerModeProp.setvalue(DCAMPROP.TRIGGER_MODE.NORMAL))
        //        //    {
        //        //        MyShowStatusNG("Failed to set trigger mode", triggerModeProp.m_lasterr);
        //        //        return false;                         // Fail: setting trigger mode
        //        //    }

        //        //    // Set up trigger active mode to edge ???
        //        //    MyDcamProp triggerActiveModeProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERACTIVE);
        //        //    if (!triggerActiveModeProp.setvalue(DCAMPROP.TRIGGERACTIVE.EDGE))
        //        //    {
        //        //        MyShowStatusNG("Failed to set trigger active mode", triggerActiveModeProp.m_lasterr);
        //        //        return false;                         // Fail: setting trigger active mode
        //        //    }

        //        //    // Set input trigger polarity to positive (rising edge)
        //        //    MyDcamProp triggerPolarityProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERPOLARITY);
        //        //    if (!triggerPolarityProp.setvalue(DCAMPROP.TRIGGERPOLARITY.POSITIVE))
        //        //    {
        //        //        MyShowStatusNG("Failed to set trigger polarity", triggerPolarityProp.m_lasterr);
        //        //        return false;                         // Fail: setting trigger polarity
        //        //    }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception during camera configuration: {ex.Message}");
        //        MyShowStatusNG("Exception during configuration", DCAMERR.TIMEOUT);
        //        return false;
        //    }
        //}

        //public Boolean ApplySelectedTriggerSource()
        //{
        //    if (mydcam == null)
        //    {
        //        MyShowStatus("Camera is not initialized, please click Init...");
        //        return false;
        //    }

        //    MyDcamProp triggerSourceProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);
        //    MyDcamProp triggerModeProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGER_MODE);
        //    MyDcamProp triggerActiveModeProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERACTIVE);
        //    MyDcamProp triggerPolarityProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERPOLARITY);

        //    bool success = false;

        //    switch (comboTriggerSource.SelectedIndex)
        //    {
        //        case 0: // Internal Trigger
        //            if (triggerSourceProp.setvalue(DCAMPROP.TRIGGERSOURCE.INTERNAL))
        //            {
        //                MyShowStatusOK("Trigger source set to Internal.");
        //                success = true;
        //            }
        //            else
        //            {
        //                MyShowStatusNG("Failed to set trigger source to Internal", triggerSourceProp.m_lasterr);
        //            }
        //            break;

        //        case 1: // External Start Trigger

        //            // Step 1: First, set it to External Edge Mode
        //            if (!triggerModeProp.setvalue(DCAMPROP.TRIGGER_MODE.NORMAL))
        //            {
        //                MyShowStatusNG("Failed to set trigger mode to NORMAL before External Start", triggerModeProp.m_lasterr);
        //                return false;
        //            }

        //            if (!triggerActiveModeProp.setvalue(DCAMPROP.TRIGGERACTIVE.EDGE))
        //            {
        //                MyShowStatusNG("Failed to set trigger active mode to EDGE", triggerActiveModeProp.m_lasterr);
        //                return false;
        //            }

        //            if (!triggerPolarityProp.setvalue(DCAMPROP.TRIGGERPOLARITY.POSITIVE))
        //            {
        //                MyShowStatusNG("Failed to set trigger polarity to POSITIVE", triggerPolarityProp.m_lasterr);
        //                return false;
        //            }

        //            if (!triggerSourceProp.setvalue(DCAMPROP.TRIGGERSOURCE.EXTERNAL))
        //            {
        //                MyShowStatusNG("Failed to set trigger source to EXTERNAL before External Start", triggerSourceProp.m_lasterr);
        //                return false;
        //            }

        //            // Step 2: Now switch to External Start
        //            if (!triggerActiveModeProp.setvalue(DCAMPROP.TRIGGERACTIVE.EDGE))
        //            {
        //                MyShowStatusNG("Failed to set trigger active mode to EDGE", triggerActiveModeProp.m_lasterr);
        //                return false;
        //            }

        //            if (!triggerPolarityProp.setvalue(DCAMPROP.TRIGGERPOLARITY.POSITIVE))
        //            {
        //                MyShowStatusNG("Failed to set trigger polarity to POSITIVE", triggerPolarityProp.m_lasterr);
        //                return false;
        //            }

        //            if (!triggerModeProp.setvalue(DCAMPROP.TRIGGER_MODE.START))
        //            {
        //                MyShowStatusNG("Failed to set trigger mode to START", triggerModeProp.m_lasterr);
        //                return false;
        //            }

        //            // Verify trigger source
        //            double triggerCheck = 0;
        //            triggerSourceProp.getvalue(ref triggerCheck);
        //            triggerModeProp.getvalue(ref triggerCheck);
        //            if (triggerCheck == (double)DCAMPROP.TRIGGERSOURCE.EXTERNAL || triggerCheck == (double)DCAMPROP.TRIGGER_MODE.START)
        //            {
        //                MyShowStatusOK("Trigger source set to External Start Trigger.");
        //                Console.WriteLine("Trigger source set to External Start Trigger.");
        //                success = true;
        //            }
        //            else
        //            {
        //                MyShowStatusNG("Trigger source verification failed: It did not switch to External Start.", triggerSourceProp.m_lasterr);
        //            }
        //            break;

        //        case 2: // External Edge Trigger
        //            if (!triggerModeProp.setvalue(DCAMPROP.TRIGGER_MODE.NORMAL))
        //            {
        //                MyShowStatusNG("Failed to set trigger mode to NORMAL", triggerModeProp.m_lasterr);
        //                return false;
        //            }

        //            if (!triggerActiveModeProp.setvalue(DCAMPROP.TRIGGERACTIVE.EDGE))
        //            {
        //                MyShowStatusNG("Failed to set trigger active mode to EDGE", triggerActiveModeProp.m_lasterr);
        //                return false;
        //            }

        //            if (!triggerPolarityProp.setvalue(DCAMPROP.TRIGGERPOLARITY.POSITIVE))
        //            {
        //                MyShowStatusNG("Failed to set trigger polarity to POSITIVE", triggerPolarityProp.m_lasterr);
        //                return false;
        //            }

        //            if (triggerSourceProp.setvalue(DCAMPROP.TRIGGERSOURCE.EXTERNAL))
        //            {
        //                MyShowStatusOK("Trigger source set to External Edge Trigger.");
        //                success = true;
        //            }

        //            else
        //            {
        //                MyShowStatusNG("Failed to set trigger source to External Edge", triggerSourceProp.m_lasterr);
        //            }
        //            break;

        //        default:
        //            MyShowStatusNG("Invalid trigger selection", DCAMERR.INVALIDVALUE);
        //            return false;
        //    }

        //    return success;
        //}

        private void comboTriggerSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool v = controller.ApplySelectedTriggerSource();
        }

        private void ComboBoxCameraSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCameraSelection.SelectedIndex >= 0) // Ensure a valid selection
            {
                controller.SelectedCamera = comboBoxCameraSelection.SelectedIndex;
                Console.WriteLine($"Selected Camera: {(controller.SelectedCamera == 0 ? "CCDA" : "CCDB")}");
            }
        }



        // Event handler for Sensor Temperature Query
        private void QuerySensorTemperatureButton_Click(object sender, EventArgs e)
        {
            controller.QuerySensorTemperature();
        }

        // Event handler for Sensitivity Gain Query
        private void QuerySensitivityGainButton_Click(object sender, EventArgs e)
        {
            controller.QueryCCDGain();
        }

        // Event handler for Sensitivity Gain Update
        private void UpdateSensitivityGainButton_Click(object sender, EventArgs e)
        {
            controller.UpdateCCDGain();
        }

        // Event handler for Exposure Time Query
        private void QueryExposureTimeButton_Click(object sender, EventArgs e)
        {
            controller.QueryExposureTime();
        }

        // Event handler for Exposure Time Update
        private void UpdateExposureTimeButton_Click(object sender, EventArgs e)
        {
            controller.UpdateExposureTime();
        }

        private void QueryFrameCountButton_Click(object sender, EventArgs e)
        {
            controller.QueryFrameCount();
        }


        private void UpdateFrameCountButton_Click(object sender, EventArgs e)
        {
            controller.UpdateFrameCount();
        }

        private void UpdateFrameCounter(object sender, EventArgs e)
        {
            controller.FrameCounter();
        }

        //private void ContinuousSnapAndSave_Click(object sender, EventArgs e)
        //{
        //    controller.ContinuousSnapAndSave();
        //}

        private void StartBurstAcquisition_Click(object sender, EventArgs e)
        {
            controller.StartBurstAcquisition();
        }

        private void StopBurstAcquisition_Click(object sender, EventArgs e)
        {
            controller.StopBurstAcquisition();
        }

        // Button Click Event to Set Save Directory
        private void SetSaveDirectoryButton_Click(object sender, EventArgs e)
        {
            controller.SetSaveDirectory();
        }
        //private void PushInfo_Click(object sender, EventArgs e)
        //{
        //    FormInfo formInfo = new FormInfo();

        //    formInfo.set_mydcam(ref mydcam);
        //    formInfo.Show();                    // show FormProperties dialog as modeless
        //}
        //private void PushProperties_Click(object sender, EventArgs e)
        //{
        //    if (formProperties == null)
        //    {
        //        formProperties = new FormProperties();
        //    }
        //    else if (formProperties.IsDisposed)
        //    {
        //        formProperties = new FormProperties();
        //    }

        //    formProperties.set_mydcam(ref mydcam);
        //    formProperties.Show();          // show FormProperties dialog as modeless
        //    formProperties.update_properties();
        //}

        private void PushSnap_Click(object sender, EventArgs e)
        {
            //if (mydcam == null)
            //{
            //    MyShowStatus("Internal Error: mydcam is null");
            //    MyFormStatus_Initialized();     // FormStatus should be Initialized.
            //    return;                         // internal error
            //}

            //string text = "";

            //if (IsMyFormStatus_Initialized())
            //{
            //    // if FormStatus is Opened, DCAM buffer is not allocated.
            //    // So call dcambuf_alloc() to prepare capturing.

            //    text = string.Format("dcambuf_alloc({0})", m_nFrameCount);

            //    // allocate m_nFrameCount frames to the buffer
            //    if (!mydcam.buf_alloc(m_nFrameCount))
            //    {
            //        // allocation was failed
            //        Console.WriteLine("Failed to allocate buffer");
            //        MyShowStatusNG("Failed to allocate buffer", mydcam.m_lasterr);
            //        MyFormStatus_Initialized(); // Reset form status to initialized
            //        return;                     // Fail: dcambuf_alloc()
            //    }

            //    // Success: dcambuf_alloc()

            //    update_lut(true);
            //}

            //// start acquisition
            //m_cap_stopping = false;
            //mydcam.m_capmode = DCAMCAP_START.SNAP;    // one time capturing.  acqusition will stop after capturing m_nFrameCount frame
            //if (!mydcam.cap_start())
            //{
            //    // acquisition was failed. In this sample, frame buffer is also released.
            //    MyShowStatusNG("Failed to start capturing", mydcam.m_lasterr);

            //    mydcam.buf_release();           // release unnecessary buffer in DCAM
            //    MyFormStatus_Initialized();          // change dialog FormStatus to Initialized
            //    return;                         // Fail: dcamcap_start()
            //}

            //// Success: dcamcap_start()
            //// acquisition has started

            ////if (text.Length > 0)
            ////{
            ////    text += " && ";
            ////}
            //MyShowStatusOK(text + "dcamcap_start()");

            //MyDcamProp prop = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);

            //double v = 0;
            //prop.getvalue(ref v);

            //if (v == DCAMPROP.TRIGGERSOURCE.SOFTWARE)
            //{
            //    MyFormStatus_AcquiringSoftwareTrigger(); // change dialog FormStatus to AcquiringSoftwareTrigger
            //}
            //else
            //{
            //    MyFormStatus_Acquiring();           // change dialog FormStatus to Acquiring
            //}

            //MyThreadCapture_Start();            // start monitoring thread


            //using (mydcamwait = new MyDcamWait(ref mydcam))
            //{
            //    while (true)
            //    {
            //        DCAMWAIT eventmask = DCAMWAIT.CAPEVENT.FRAMEREADY | DCAMWAIT.CAPEVENT.STOPPED;
            //        DCAMWAIT eventhappened = DCAMWAIT.NONE;

            //        if (mydcamwait.start(eventmask, ref eventhappened))
            //        {
            //            if (eventhappened & DCAMWAIT.CAPEVENT.FRAMEREADY)
            //            {
            //                //break;
            //            }

            //            if (eventhappened & DCAMWAIT.CAPEVENT.STOPPED)
            //            {
            //                break;
            //            }
            //        }
            //        else
            //        {
            //            if (mydcamwait.m_lasterr == DCAMERR.TIMEOUT)
            //            {
            //                Console.WriteLine("Failed wait.");
            //                break;
            //            }
            //            else
            //            if (mydcamwait.m_lasterr == DCAMERR.ABORT)
            //            {
            //                Console.WriteLine("Failed wait.");
            //                break;
            //            }
            //        }
            //    }
            //}

            //// Stop acquisition after the snap is completed.
            //if (mydcam.cap_stop())
            //{
            //    MyShowStatusOK($"{m_nFrameCount} frames has been successfully taken, now please click Save Snaps.");
            //    MyFormStatus_Acquired(); // Update status to Acquired to reflect successful capture.
            //}
            //else
            //{
            //    MyShowStatusNG("Failed to stop acquisition after snap", mydcam.m_lasterr);
            //}

            //// Update the image to display the snap  
            //MyUpdateImage(0);
            //MyUpdateDisplay(); // Refresh the display to show the snap

            controller.Snap();
        }


        private void PushLive_Click(object sender, EventArgs e)
        {
            //if (mydcam == null)
            //{
            //    MyShowStatus("Internal Error: mydcam is null");
            //    MyFormStatus_Initialized();     // FormStatus should be Initialized.
            //    return;                         // internal error
            //}

            //string text = "";

            //if (IsMyFormStatus_Initialized())
            //{
            //    // if FormStatus is Opened, DCAM buffer is not allocated.
            //    // So call dcambuf_alloc() to prepare capturing.

            //    text = string.Format("dcambuf_alloc({0})", m_nFrameCount);

            //    // allocate frame buffer
            //    if (!mydcam.buf_alloc(m_nFrameCount))
            //    {
            //        // allocation was failed
            //        MyShowStatusNG("Failed to allocate live images.", mydcam.m_lasterr);
            //        MyFormStatus_Initialized(); // Reset form status to initialized
            //        return;                     // Fail: dcambuf_alloc()
            //    }

            //    // Success: dcambuf_alloc()

            //    update_lut(true);  // Success: update LUT
            //}

            //// start acquisition
            //m_cap_stopping = false;
            //mydcam.m_capmode = DCAMCAP_START.SEQUENCE;    // continuous capturing.  continuously acqusition will be done
            //if (!mydcam.cap_start())
            //{
            //    // acquisition was failed. In this sample, frame buffer is also released.
            //    MyShowStatusNG("dcamcap_start()", mydcam.m_lasterr);

            //    mydcam.buf_release();           // release unnecessary buffer in DCAM
            //    MyFormStatus_Initialized();          // change dialog FormStatus to Opened
            //    return;                         // Fail: dcamcap_start()
            //}

            //// Success: dcamcap_start()
            //// acquisition has started

            //if (text.Length > 0)
            //{
            //    text += " && ";
            //}

            //MyShowStatusOK("Live capturing...");

            //MyDcamProp prop = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);
            //double v = 0;
            //prop.getvalue(ref v);

            //if (v == DCAMPROP.TRIGGERSOURCE.SOFTWARE)
            //{
            //    MyFormStatus_AcquiringSoftwareTrigger(); // change dialog FormStatus to AcquiringSoftwareTrigger
            //}
            //else
            //{
            //    MyFormStatus_Acquiring();           // change dialog FormStatus to Acquiring
            //}
            //MyThreadCapture_Start();            // start monitoring thread

            controller.Live();
        }


        // Stop Acquisition button, which stops image acquisition and return the application to a state where the camera is not actively capturing or processing frames.
        private void PushIdle_Click(object sender, EventArgs e)
        {
            //if (mydcam == null)
            //{
            //    MyShowStatus("Internal Error: mydcam is null");
            //    MyFormStatus_Initialized();     // FormStatus should be Initialized.
            //    return;                         // internal error
            //}

            //if (!IsMyFormStatus_Acquiring())
            //{
            //    MyShowStatus("Internal Error: Idle button is only available when FormStatus is Acquiring");
            //    return;                         // internal error
            //}

            //// stop acquisition
            //m_cap_stopping = true;
            //if (!mydcam.cap_stop())
            //{
            //    MyShowStatusNG("dcamcap_stop()", mydcam.m_lasterr);
            //    return;                         // Fail: dcamcap_stop()
            //}

            //// Success: dcamcap_stop()

            //MyShowStatusOK("Frames capturing is stopped.");
            //MyFormStatus_Acquired();            // change dialog FormStatus to Acquired
            //MyShowStatus("Acquisition stopped, ready for further operations");

            controller.StopAcquisition();
        }

        //private void InitializeCsvFile_Click(object sender, EventArgs e)
        //{
        //    controller.InitializeCsvFile();
        //}
        private void BurstTriggerRearm_Click(object sender, EventArgs e)
        {
            controller.BurstTriggerRearm();
        }

        private void PushBufRelease_Click(object sender, EventArgs e)
        {
            //if (mydcam == null)
            //{
            //    MyShowStatus("Internal Error: mydcam is null");
            //    MyFormStatus_Initialized();     // FormStatus should be Initialized.
            //    return;                         // internal error
            //}

            //if (!IsMyFormStatus_Acquired())
            //{
            //    MyShowStatus("Internal Error: BufRelease is only available when FormStatus is Acquired");
            //    return;                         // internal error
            //}

            //// Save images to disk before releasing buffer
            //string saveDirectory = "E:\\Imperial College London\\Team ultracold - PH - Documents\\Data\\2025\\CCD data";
            //Directory.CreateDirectory(saveDirectory); // Create the directory if it does not exist 

            //try
            //{
            //    for (int i = 0; i < m_nFrameCount; i++)
            //    {
            //        // Lock the frame to access its data
            //        m_image.set_iFrame(i);
            //        if (!mydcam.buf_lockframe(ref m_image.bufframe))
            //        {
            //            MyShowStatusNG($"Failed to lock frame {i}", mydcam.m_lasterr);
            //            continue; // Skip to the next frame if locking fails
            //        }

            //        // Convert the frame to a Bitmap
            //        Bitmap frame;
            //        lock (BitmapLock)
            //        {
            //            frame = new Bitmap(m_image.width, m_image.height, PixelFormat.Format24bppRgb);
            //            Rectangle rc = new Rectangle(0, 0, m_image.width, m_image.height);
            //            SUBACQERR err = subacq.copydib(ref frame, m_image.bufframe, ref rc, m_lut.inmax, m_lut.inmin, m_lut.camerabpp);

            //            if (err != SUBACQERR.SUCCESS)
            //            {
            //                MyShowStatusNG($"Failed to convert frame {i} to Bitmap", mydcam.m_lasterr);
            //                frame.Dispose();
            //                continue;
            //            }
            //        }

            //        // Generate a unique file name for the frame
            //        string tiffFilePath = Path.Combine(saveDirectory, $"Frame_{i + 1:D2}.tiff");

            //        // Check if the file already exists and generate a new name if necessary
            //        int counter = 1;
            //        while (File.Exists(tiffFilePath))
            //        {
            //            tiffFilePath = Path.Combine(saveDirectory, $"Frame_{i + 1:D2}_{counter}.tiff");
            //            counter++;
            //        }

            //        // Save the frame as an individual TIFF file
            //        frame.Save(tiffFilePath, ImageFormat.Tiff);
            //        frame.Dispose(); // Release the frame bitmap
            //    }
            //    Console.WriteLine($"{m_nFrameCount} frames are saving to the directory...");
            //    MyShowStatusOK($"Saved {m_nFrameCount} frames as individual TIFF files to {saveDirectory}");
            //}
            //catch (Exception ex)
            //{
            //    MyShowStatusNG($"Error saving TIFF files: {ex.Message}", DCAMERR.SUCCESS);
            //}
            //finally
            //{
            //    bool isError = false; // Flag to track if any errors occur during buffer release

            //    // Release the buffer only if no error occurred during the main execution
            //    if (!mydcam.buf_release())
            //    {
            //        MyShowStatusNG("dcambuf_release()", mydcam.m_lasterr);
            //        isError = true; // Fail: dcambuf_release()
            //    }

            //    // Success: dcambuf_release()
            //    MyShowStatusOK($"{m_nFrameCount} frames have been successfully saved to the directory.");
            //    Console.WriteLine($"{m_nFrameCount} frames have been successfully saved to the directory.");
            //    MyFormStatus_Initialized(); // Change dialog FormStatus to Opened
            //    m_image.clear();

            //    // If there was an error during buffer release, handle accordingly
            //    if (isError)
            //    {
            //        // If you want to show a specific message or handle other things due to failure
            //        MyShowStatus("Buffer release failed, cleanup needed.");
            //    }
            //}
            controller.BufRelease();
        }

        //private void PushBufRelease_Click(object sender, EventArgs e)
        //{
        //    if (mydcam == null)
        //    {
        //        MyShowStatus("Internal Error: mydcam is null");
        //        MyFormStatus_Initialized();     // FormStatus should be Initialized.
        //        return;                         // internal error
        //    }

        //    if (!IsMyFormStatus_Acquired())
        //    {
        //        MyShowStatus("Internal Error: BufRelease is only available when FormStatus is Acquired");
        //        return;                         // internal error
        //    }

        //    // release buffer
        //    if (!mydcam.buf_release())
        //    {
        //        MyShowStatusNG("dcambuf_release()", mydcam.m_lasterr);
        //        return;                         // Fail: dcambuf_release()
        //    }

        //    // Success: dcambuf_release()

        //    MyShowStatusOK("dcambuf_release()");
        //    MyFormStatus_Initialized();              // change dialog FormStatus to Opened

        //    m_image.clear();
        //}

        private void PushClose_Click(object sender, EventArgs e)
        {
            if (mydcam == null)
            {
                MyShowStatus("Internal Error: mydcam is null");
                MyFormStatus_Initialized();     // FormStatus should be Initialized.
                return;                         // internal error
            }

            MyThreadCapture_Abort();            // abort capturing thread if exist

            if (!mydcam.dev_close())
            {
                MyShowStatusNG("dcamdev_close()", mydcam.m_lasterr);
                return;                         // Fail: dcamdev_close()
            }

            // Success: dcamdev_close()

            mydcam = null;

            MyShowStatusOK("Camera is closed.");
            MyFormStatus_Initialized();         // change dialog FormStatus to Initialized
        }

        private void EditLutMax_TextChanged(object sender, EventArgs e)
        {
            try
            {
                m_lut.inmax = int.Parse(EditLutMax.Text);
                if (HScrollLutMax.Value != m_lut.inmax && 0 <= m_lut.inmax && m_lut.inmax <= m_lut.cameramax)
                {
                    HScrollLutMax.Value = m_lut.inmax;
                    MyUpdatePicture();
                }
            }
            catch
            {
                m_lut.inmax = 0;
            }
        }

        private void EditLutMin_TextChanged(object sender, EventArgs e)
        {
            try
            {
                m_lut.inmin = int.Parse(EditLutMin.Text);
                if (HScrollLutMin.Value != m_lut.inmin && 0 <= m_lut.inmin && m_lut.inmin <= m_lut.cameramax)
                {
                    HScrollLutMin.Value = m_lut.inmin;
                    MyUpdatePicture();
                }
            }
            catch
            {
                m_lut.inmin = 0;
            }
        }

        private void HScrollLutMax_ValueChanged(object sender, EventArgs e)
        {
            if (m_lut.inmax != HScrollLutMax.Value)
            {
                m_lut.inmax = HScrollLutMax.Value;
                EditLutMax.Text = m_lut.inmax.ToString();
                MyUpdatePicture();
            }
        }

        private void HScrollLutMin_ValueChanged(object sender, EventArgs e)
        {
            if (m_lut.inmin != HScrollLutMin.Value)
            {
                m_lut.inmin = HScrollLutMin.Value;
                EditLutMin.Text = m_lut.inmin.ToString();
                MyUpdatePicture();
            }
        }

        // Button for auto adjusting the image intensity
        private void PushAsterisk_Click(object sender, EventArgs e)
        {
            controller.AutoIntensity();
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void SaveDirectoryLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
