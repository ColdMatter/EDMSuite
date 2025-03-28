﻿using System;
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
        private System.Windows.Forms.Timer frameCounterTimer;

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
            PushIdle.Enabled = isAcquiring;
            //PushBufRelease.Enabled = isAcquired; 
            PushClose.Enabled = isInitialized || isAcquired;
            //PushUninit.Enabled = isInitialized; rhys remove 14/02

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

            // Initialize and start the frame counter timer
            frameCounterTimer = new System.Windows.Forms.Timer();
            frameCounterTimer.Interval = 1; // Update every 500ms
            frameCounterTimer.Tick += UpdateFrameCounter;
            frameCounterTimer.Start();
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

            comboBoxCameraSelection.Items.Add("CCD1");
            comboBoxCameraSelection.Items.Add("CCD2");
            comboBoxCameraSelection.SelectedIndex = 1; // Default to CCD2

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
            controller.CameraInit();
        }

        private void comboTriggerSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool v = controller.ApplySelectedTriggerSource();
        }

        private void ComboBoxCameraSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            controller.SelectedCamera = comboBoxCameraSelection.SelectedIndex;
            Console.WriteLine($"Selected Camera: {(controller.SelectedCamera == 0 ? "CCD1" : "CCD2")}");
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
        private void ContinuousSnapAndSave_Click(object sender, EventArgs e)
        {
            controller.ContinuousSnapAndSave();
        }

        // Button Click Event to Set Save Directory
        private void SetSaveDirectoryButton_Click(object sender, EventArgs e)
        {
            controller.SetSaveDirectory();
        }

        private void QueryNumSnapsButton_Click(object sender, EventArgs e)
        {
            controller.QueryNumSnaps();
        }

        private void UpdateNumSnapsButton_Click(object sender, EventArgs e)
        {
            controller.UpdateNumSnaps();
        }

        private void PushSnap_Click(object sender, EventArgs e)
        {
            controller.Snap();
        }

        private void PushLive_Click(object sender, EventArgs e)
        {
        //    if (mydcam == null)
        //    {
        //        MyShowStatus("Internal Error: mydcam is null");
        //        MyFormStatus_Initialized();     // FormStatus should be Initialized.
        //        return;                         // internal error
        //    }

        //    string text = "";

        //    if (IsMyFormStatus_Initialized())
        //    {
        //        // if FormStatus is Opened, DCAM buffer is not allocated.
        //        // So call dcambuf_alloc() to prepare capturing.

        //        text = string.Format("dcambuf_alloc({0})", m_nFrameCount);

        //        // allocate frame buffer
        //        if (!mydcam.buf_alloc(m_nFrameCount))
        //        {
        //            // allocation was failed
        //            MyShowStatusNG("Failed to allocate live images.", mydcam.m_lasterr);
        //            MyFormStatus_Initialized(); // Reset form status to initialized
        //            return;                     // Fail: dcambuf_alloc()
        //        }

        //        // Success: dcambuf_alloc()

        //        update_lut(true);  // Success: update LUT
        //    }

        //    // start acquisition
        //    m_cap_stopping = false;
        //    mydcam.m_capmode = DCAMCAP_START.SEQUENCE;    // continuous capturing.  continuously acqusition will be done
        //    if (!mydcam.cap_start())
        //    {
        //        // acquisition was failed. In this sample, frame buffer is also released.
        //        MyShowStatusNG("dcamcap_start()", mydcam.m_lasterr);

        //        mydcam.buf_release();           // release unnecessary buffer in DCAM
        //        MyFormStatus_Initialized();          // change dialog FormStatus to Opened
        //        return;                         // Fail: dcamcap_start()
        //    }

        //    // Success: dcamcap_start()
        //    // acquisition has started

        //    if (text.Length > 0)
        //    {
        //        text += " && ";
        //    }

        //    MyShowStatusOK("Live capturing...");

        //    MyDcamProp prop = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);
        //    double v = 0;
        //    prop.getvalue(ref v);

        //    if (v == DCAMPROP.TRIGGERSOURCE.SOFTWARE)
        //    {
        //        MyFormStatus_AcquiringSoftwareTrigger(); // change dialog FormStatus to AcquiringSoftwareTrigger
        //    }
        //    else
        //    {
        //        MyFormStatus_Acquiring();           // change dialog FormStatus to Acquiring
        //    }
        //    MyThreadCapture_Start();            // start monitoring thread

            controller.Live();
        }


        // Stop Acquisition button, which stops image acquisition and return the application to a state where the camera is not actively capturing or processing frames.
        private void PushIdle_Click(object sender, EventArgs e)
        {
            controller.StopAcquisition();
        }

        private void InitializeCsvFile_Click(object sender, EventArgs e)
        {
            controller.InitializeCsvFile();
        }
        private void BurstTriggerRearm_Click(object sender, EventArgs e)
        {
            controller.BurstTriggerRearm();
        }

        private void PushBufRelease_Click(object sender, EventArgs e)
        {
            controller.BufRelease();
        }

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
    }
}
