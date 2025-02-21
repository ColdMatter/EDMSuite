using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Threading.Tasks;

using System.Drawing.Imaging;

using Hamamatsu.DCAM4;
using Hamamatsu.subacq4;
using static System.Windows.Forms.AxHost;
using System.IO;
using BitMiracle.LibTiff.Classic;
using csAcq4;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics.Eventing.Reader;
using System.Xml.Linq;
using System.Windows.Controls;

namespace csAcq4
{
    public class CCDController : MarshalByRefObject
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        private enum FormStatus
        {
            Startup,                // After startup or camapi_uninit()
            Initialized,            // After dcamapi_init() or dcamdev_close()
            Opened,                 // After dcamdev_open() or dcamcap_stop() without any image
            Acquiring,              // After dcamcap_start()
            AcquiringSoftwareTrigger,
            Acquired                // After dcamcap_stop() with image
        }

        public FormMain window;
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
        private int m_nFrameCount = 4;              // frame count of allocation buffer for DCAM capturing
        private FormStatus m_formstatus;        // Indicate current Form status. For setting, Use MyFormStatus() functions
        private Thread m_threadCapture;         // System.Threading.  Assigned for monitoring updated frames during capturing
        private Bitmap m_bitmap;                // bitmap data for displaying in this Windows Form
        private bool m_cap_stopping = false;    // reset to false when starting capture and set to true if stopping capture


        // ---------------- private class MyImage ----------------

        public class MyImage
        {
            public DCAMBUF_FRAME bufframe;
            public MyImage()
            {
                bufframe = new DCAMBUF_FRAME(0);
                bufframe.type = DCAM_PIXELTYPE.MONO16;
            }
            public int width { get { return bufframe.width; } }
            public int height { get { return bufframe.height; } }
            public DCAM_PIXELTYPE pixeltype { get { return bufframe.type; } }
            public bool isValid()
            {
                if (width <= 0 || height <= 0 || pixeltype == DCAM_PIXELTYPE.NONE)
                {
                    return false;
                }
                else
                {
                    return true;
                }

                //if (width <= 0 || height <= 0)
                //{

                //    return false;
                //}

                //if (!pixeltype.Equals(DCAM_PIXELTYPE.MONO16))
                //{
                //    Console.WriteLine($"Invalid Pixel Type - Expected: {(int)DCAM_PIXELTYPE.MONO16}, Found: {(int)pixeltype}");
                //    return false;
                //}

                //return true;
            }
        public void clear()
            {
                bufframe.width = 0;
                bufframe.height = 0;
                bufframe.type = DCAM_PIXELTYPE.NONE;
            }
            public void set_iFrame(int index)
            {
                bufframe.iFrame = index;
            }
        }
        
        // ------------------ Startup ------------------
        public void Start()
        {
            // Create FormMain and assign this CCDController instance to it
            window = new FormMain(this);
            window.controller = this;  // Set this instance of CCDController to FormMain

            System.Windows.Forms.Application.Run(window);
        }

        // Set mydcam from FormMain - Rhys commented this out when transferring all of CCD initialisation over to the CCD controller
        //public void SetMyDcam(MyDcam dcamInstance)
        //{
        //    if (dcamInstance != null)
        //    {
        //        mydcam = dcamInstance;
        //        Console.WriteLine("mydcam successfully initialized in CCDController.");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Error: Attempted to initialize mydcam with a null reference.");
        //    }
        //}



        //------------------ Local functions ------------------

        // Display status
        private void MyShowStatus(string text)
        {
            if (window != null && window.LabelStatus != null)
            {
                if (window.LabelStatus.InvokeRequired)
                {
                    window.LabelStatus.Invoke(new Action(() =>
                    {
                        window.LabelStatus.Text = text;
                    }));
                }
                else
                {
                    window.LabelStatus.Text = text;
                }
             //window.LabelStatus.Text = text;
            }
            else
            {
                // Handle the case where window or LabelStatus is null
                Console.WriteLine("window or LabelStatus is not initialized.");
            }
        }
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
        private void update_lut(bool bUpdatePicture)
        {
            if (mydcam != null)
            {
                MyDcamProp prop = new MyDcamProp(mydcam, DCAMIDPROP.BITSPERCHANNEL);

                double v = 0;
                prop.getvalue(ref v);
                Console.WriteLine($"BITSPERCHANNEL: {v}");
                bool reset = false;

                // Check if bit depth has changed 
                if (m_lut.camerabpp > 0 && m_lut.camerabpp != (int)v)
                {
                    reset = true;
                }
                m_lut.camerabpp = (int)v;
                m_lut.cameramax = (1 << m_lut.camerabpp) - 1;

                m_lut.inmax = window.HScrollLutMax.Value;
                m_lut.inmin = window.HScrollLutMin.Value;

                if (window.HScrollLutMax.InvokeRequired)
                {
                    window.HScrollLutMax.Invoke(new Action(() =>
                    {
                        window.HScrollLutMax.Maximum = m_lut.cameramax;
                    }
                    ));
                }
                else
                {
                    window.HScrollLutMax.Maximum = m_lut.cameramax;
                }

                if (window.HScrollLutMin.InvokeRequired)
                {
                    window.HScrollLutMin.Invoke(new Action(() =>
                    {
                        window.HScrollLutMin.Maximum = m_lut.cameramax;
                    }
                    ));
                }
                else
                {
                    window.HScrollLutMin.Maximum = m_lut.cameramax;
                }
             
                if (reset)
                {
                    window.HScrollLutMax.Value = m_lut.cameramax;
                    window.HScrollLutMin.Value = 0;
                    bUpdatePicture = true;
                }

                if (bUpdatePicture)
                    MyUpdatePicture();
            }
        }

        // auto LUT condition
        public void auto_lut()
        {

            bool bUpdatePicture = false;
            int min = m_lut.cameramax;
            int max = 0;


            if (m_image.isValid())
            {
                int w = window.PicDisplay.Size.Width;
                int h = window.PicDisplay.Size.Height;
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
                window.HScrollLutMax.Value = m_lut.inmax;
                window.EditLutMax.Text = m_lut.inmax.ToString();
                bUpdatePicture = true;
            }

            if (m_lut.inmin != min)
            {
                m_lut.inmin = min;
                window.HScrollLutMin.Value = m_lut.inmin;
                window.EditLutMin.Text = m_lut.inmin.ToString();
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

            //m_image.bufframe.type = DCAM_PIXELTYPE.MONO16;
            //Console.WriteLine($"Locked frame dimensions: {m_image.width}x{m_image.height}");
            //Console.WriteLine($"Pixel Type: {m_image.pixeltype.ToString()}, Raw Value: {(int)m_image.pixeltype}");

            MyUpdatePicture();

        }

        // Draw Bitmap in the PictureBox 
        private delegate void MyDelegate_UpdateDisplay();


        private void MyUpdateDisplay()
        {
            if (window.InvokeRequired)
            {
                window.Invoke(new MyDelegate_UpdateDisplay(MyUpdateDisplay));
                return;
            }

            if (m_bitmap == null)
            {
                Console.WriteLine("Warning: Bitmap is null, skipping update.");
                return;
            }

            //// Debug statements to print m_image properties
            //Console.WriteLine($"m_image width: {m_image.width}");
            //Console.WriteLine($"m_image height: {m_image.height}");
            //Console.WriteLine($"m_image pixeltype: {m_image.pixeltype}");

            System.Drawing.Image oldImg = window.PicDisplay.Image;

            try
            {
                // Ensure dimensions are valid
                if (window.PicDisplay.Width <= 0 || window.PicDisplay.Height <= 0)
                {
                    Console.WriteLine("Error: Invalid PictureBox dimensions.");
                    return;
                }

                // Create a new bitmap. Ensure we always use the fixed PictureBox dimensions
                Bitmap bmp = new Bitmap(window.PicDisplay.Width, window.PicDisplay.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (var gr = Graphics.FromImage(bmp))
                {
                    lock (BitmapLock)
                    {
                        gr.DrawImage(m_bitmap, 0, 0, window.PicDisplay.Width, window.PicDisplay.Height);
                    }
                }

                window.PicDisplay.Image = bmp;
                window.PicDisplay.Refresh();

                oldImg?.Dispose();
                //Console.WriteLine("PictureBox updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating or updating Bitmap: {ex.Message}");
            }
        }

        // Updating Bitmap and Display
        //private void MyUpdatePicture()
        //{

        //    if (m_image.isValid())
        //    {
        //        m_lut.inmax = window.HScrollLutMax.Value;
        //        m_lut.inmin = window.HScrollLutMin.Value;

        //        Rectangle rc = new Rectangle(0, 0, m_image.width, m_image.height);
        //        lock (BitmapLock)
        //        {
        //            // Always initialize bitmap if it's null or size changes
        //            if (m_bitmap == null || m_bitmap.Width != m_image.width || m_bitmap.Height != m_image.height)
        //            {
        //                Console.WriteLine("Initializing new bitmap...");
        //                m_bitmap?.Dispose(); // Dispose old bitmap if it exists
        //                m_bitmap = new Bitmap(m_image.width, m_image.height, PixelFormat.Format24bppRgb);
        //            }

        //            // Copy image data into Bitmap
        //            SUBACQERR err = subacq.copydib(ref m_bitmap, m_image.bufframe, ref rc, m_lut.inmax, m_lut.inmin, m_lut.camerabpp);
        //            if (err != SUBACQERR.SUCCESS)
        //            {
        //                Console.WriteLine($"Error copying DIB: {err}, using fallback blank image.");

        //                // Fallback: Assign a blank image to avoid null issues
        //                using (Graphics g = Graphics.FromImage(m_bitmap))
        //                {
        //                    g.Clear(Color.Black);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine("Warning: Image is not valid, skipping update.");
        //        return;
        //    }

        //    if (window.PicDisplay.InvokeRequired)
        //    {
        //        window.PicDisplay.Invoke(new MyDelegate_UpdateDisplay(MyUpdateDisplay));
        //        return;
        //    }

        //    MyUpdateDisplay();
        //}

        private void MyUpdatePicture()
        {
            if (!m_image.isValid())
            {
                Console.WriteLine(" Warning: Image is not valid, skipping update.");
                return;
            }

            m_lut.inmax = window.HScrollLutMax.Value;
            m_lut.inmin = window.HScrollLutMin.Value;

            Rectangle rc = new Rectangle(0, 0, m_image.width, m_image.height);

            lock (BitmapLock)
            {
                // Allocate a 24-bit RGB bitmap for display
                if (m_bitmap == null || m_bitmap.Width != m_image.width || m_bitmap.Height != m_image.height)
                {
                    Console.WriteLine($" Re-initializing bitmap for display: {m_image.width}x{m_image.height}");
                    m_bitmap?.Dispose();
                    m_bitmap = new Bitmap(m_image.width, m_image.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                }

                // Convert 16-bit grayscale to 8-bit grayscale
                byte[] gray8bitImage = Convert16BitTo8Bit(m_image.bufframe.buf, m_image.width, m_image.height, m_image.bufframe.rowbytes);

                // Convert 8-bit grayscale to RGB for display
                byte[] rgbImage = Convert8BitToRGB(gray8bitImage, m_image.width, m_image.height);

                // Lock bitmap & copy converted RGB data
                BitmapData bitmapData = m_bitmap.LockBits(rc, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Marshal.Copy(rgbImage, 0, bitmapData.Scan0, rgbImage.Length);
                m_bitmap.UnlockBits(bitmapData);

                //Copy image data into Bitmap
                SUBACQERR err = subacq.copydib(ref m_bitmap, m_image.bufframe, ref rc, m_lut.inmax, m_lut.inmin, m_lut.camerabpp);
                if (err != SUBACQERR.SUCCESS)
                {
                    Console.WriteLine($"Error copying DIB: {err}, using fallback blank image.");

                    // Fallback: Assign a blank image to avoid null issues
                    using (Graphics g = Graphics.FromImage(m_bitmap))
                    {
                        g.Clear(System.Drawing.Color.Black);
                    }
                }
            }
            // Display the image
            if (window.PicDisplay.InvokeRequired)
            {
                window.PicDisplay.Invoke(new MyDelegate_UpdateDisplay(MyUpdateDisplay));
                return;
            }

            MyUpdateDisplay();
        }

        private byte[] Convert16BitTo8Bit(IntPtr srcPtr, int width, int height, int rowbytes)
        {
            byte[] gray8bitImage = new byte[width * height];
            byte[] srcBytes = new byte[width * 2];  // Each pixel is 2 bytes (16-bit)

            for (int y = 0; y < height; y++)
            {
                int srcOffset = rowbytes * y;
                Marshal.Copy((IntPtr)(srcPtr.ToInt64() + srcOffset), srcBytes, 0, width * 2);

                for (int x = 0; x < width; x++)
                {
                    ushort pixel16 = BitConverter.ToUInt16(srcBytes, x * 2);  // Convert 2 bytes to 16-bit value
                    gray8bitImage[y * width + x] = (byte)(pixel16 >> 8);      // Extract top 8 bits for 8-bit grayscale
                }
            }

            return gray8bitImage;
        }


        private byte[] Convert8BitToRGB(byte[] gray8bitImage, int width, int height)
        {
            byte[] rgbImage = new byte[width * height * 3];

            for (int i = 0; i < gray8bitImage.Length; i++)
            {
                byte gray = gray8bitImage[i];

                rgbImage[i * 3] = gray;     // Red
                rgbImage[i * 3 + 1] = gray; // Green
                rgbImage[i * 3 + 2] = gray; // Blue
            }

            return rgbImage;
        }



        // Capturing Thread helper functions
        private void MyThreadCapture_Start()
        {
            m_threadCapture = new Thread(new ThreadStart(OnThreadCapture));

            m_threadCapture.IsBackground = true;
            m_threadCapture.Start();
        }
        private void MyThreadCapture_Abort()
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
            if (window.InvokeRequired)
            {
                // worker thread calls this function
                window.Invoke(new MyDelegate_SnapCaptureFinished(MySnapCaptureFinished), null);
                return;
            }

            MyShowStatusOK($"{m_nFrameCount} frames have been taken.");
            window.MyFormStatus_Acquired();            // change dialog FormStatus to Acquired
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



        public CCDController()
        {

            m_image = new MyImage();
            m_lut = new MyLut();
            BitmapLock = new object();

            m_image.bufframe.type = DCAM_PIXELTYPE.MONO16;
        }

        // <summary>
        // Controller functions for event handlers in FormMain are defined here
        // </summary>
        //----------------DCAM-API related command Handler----------------
        public void CameraInit()
        {
            //dcamapi_init() may takes for a few seconds

           Cursor.Current = Cursors.WaitCursor;

            // Step 1: Initialize Camera API
            if (!MyDcamApi.init())
                {
                    MyShowStatusNG("Failed to initialise camera API", MyDcamApi.m_lasterr);
                    Cursor.Current = Cursors.Default;
                    return;                         // Fail: dcamapi_init()
                }

            // Success: dcamapi_init()

            MyShowStatusOK("Camera API initialized successfully.");
            window.MyFormStatus_Initialized();

            // Step 2: Open the camera
            window.MyFormStatus_Startup();     // change dialog FormStatus to Startup
            MyDcam amydcam = new MyDcam();
            if (!amydcam.dev_open(m_indexCamera))
            {
                MyShowStatusNG("Failed to connect to the camera", amydcam.m_lasterr);
                MyDcamApi.uninit(); // Clean up on failure
                                    //amydcam = null;
                Cursor.Current = Cursors.Default;
                return;                         // fail: dcamdev_open()
            }

            // success: dcamdev_open()

            mydcam = amydcam;                   // store mydcam instance

            // Pass mydcam instance to CCDController - Rhys commented this out when moving all initialisation across
            //if (controller != null)
            //{
            //    controller.SetMyDcam(mydcam);
            //    Console.WriteLine("mydcam successfully passed to CCDController.");
            //}
            //else
            //{
            //    Console.WriteLine("Error: CCDController is null when setting mydcam.");
            //}

            MyShowStatusOK("Camera connected successfully.");
            window.MyFormStatus_Opened();

            // Camera property configuration
            if (!ConfigureCamera())
            {
                MyShowStatusNG("Camera configuration failed", mydcam.m_lasterr);
                mydcam.dev_close();
                mydcam = null;
                MyDcamApi.uninit();
                Cursor.Current = Cursors.Default;
                return;
            }

            ApplySelectedTriggerSource(); // Apply the selected trigger source 

            MyShowStatusOK("Initialisation and camera configuration complete.");
            window.MyFormStatus_Initialized();
            Cursor.Current = Cursors.Default;
        }

        // The ConfigureCamera function sets the camera properties to the desired values as we do on the Capture tab in the software   
        public bool ConfigureCamera()
        {
            try
            {

                //// Set the Mono channel:1 
                //MyDcamProp channelProp = new MyDcamProp(mydcam, DCAMIDPROP.NUMBEROF_CHANNEL);
                //double currentValue = 1.0;
                //if (!channelProp.getvalue(ref currentValue))
                //{
                //    Console.WriteLine("NUMBEROF_CHANNEL not supported. Skipping...");
                //}
                //else
                //{
                //    // Set property if supported
                //    if (!channelProp.setvalue(1.0))
                //    {
                //        MyShowStatusNG("Failed to set camera to Mono: 1 Channel", channelProp.m_lasterr);
                //        return false; // Fail: Unable to set property
                //    }
                //    Console.WriteLine($"NUMBEROF_CHANNEL set successfully. Current value: {currentValue}");
                //}


                // ensure the camera is the correct model
                //MyDcamProp cameraname = new MyDcamProp(mydcam, DCAMIDSTR.MODEL);
                //MyDcamProp serialnumber = new MyDcamProp(mydcam, DCAMIDSTR.CAMERA_SERIESNAME);
                //string cameraname = mydcam.getstring(DCAMIDSTR.MODEL, ref cameraname);
                //string serialnumber = mydcam.getstring(DCAMIDSTR.CAMERA_SERIESNAME, ref serialnumber);
                //if (cameraname != "c9100-23b" || serialnumber != "000620")
                //{
                //    myshowstatus("camera model or serial number does not match");
                //    mydcam.dev_close();
                //    mydcam = null;
                //    cursor.current = cursors.default;
                //    return;                         // fail: incorrect camera
                //}

                //MyDcamProp colortypeProp = new MyDcamProp(mydcam, DCAMIDPROP.COLORTYPE);
                //if (!colortypeProp.setvalue(DCAMPROP.COLORTYPE.BW))
                //{
                //    MyShowStatusNG("Failed to set CCD mode", colortypeProp.m_lasterr);
                //    return false;                         // Fail: setting colour type
                //}

                // * Camera Control (Gain and Exposure Time) * //
                // Set CCD mode to EMCCD
                MyDcamProp ccdModeProp = new MyDcamProp(mydcam, DCAMIDPROP.CCDMODE);
                if (!ccdModeProp.setvalue(DCAMPROP.CCDMODE.EMCCD))
                {
                    MyShowStatusNG("Failed to set CCD mode", ccdModeProp.m_lasterr);
                    return false;                         // Fail: setting CCD mode
                }

                // Set direct gain mode to ON
                MyDcamProp directGainModeProp = new MyDcamProp(mydcam, DCAMIDPROP.DIRECTEMGAIN_MODE);
                if (!directGainModeProp.setvalue(DCAMPROP.MODE.ON))
                {
                    MyShowStatusNG("Failed to set direct gain mode", directGainModeProp.m_lasterr);
                    return false;                         // Fail: setting direct gain mode
                }
                // Set sensitivity gain
                MyDcamProp sensitivitygainprop = new MyDcamProp(mydcam, DCAMIDPROP.SENSITIVITY);
                if (!sensitivitygainprop.setvalue(150)) // Adjust sensitivity gain (0 to 1200 range)
                {
                    MyShowStatusNG("Failed to set sensitivity gain", sensitivitygainprop.m_lasterr);
                    return false; // Exit on failure
                }

                // Set exposure time mode to be ON such that exposure time can be controlled 
                MyDcamProp exposuremodeprop = new MyDcamProp(mydcam, DCAMIDPROP.EXPOSURETIME_CONTROL);
                if (!exposuremodeprop.setvalue(DCAMPROP.EXPOSURETIME_CONTROL.NORMAL))
                {
                    MyShowStatusNG("Failed to set exposure mode", exposuremodeprop.m_lasterr);
                    return false; // Exit on failure
                }

                // Set exposure time to 40 ms (entire TOF)
                MyDcamProp exposuretimeprop = new MyDcamProp(mydcam, DCAMIDPROP.EXPOSURETIME);
                if (!exposuretimeprop.setvalue(0.04)) // Adjust exposure time in seconds
                {
                    MyShowStatusNG("Failed to set exposure time", exposuretimeprop.m_lasterr);
                    return false; // Exit on failure
                }

                // Set internal trigger handling to faster frame rate such that the exposure time can be overlapped with the readout time
                MyDcamProp internalTriggerHandlingProp = new MyDcamProp(mydcam, DCAMIDPROP.INTERNALTRIGGER_HANDLING);
                if (!internalTriggerHandlingProp.setvalue(DCAMPROP.INTERNALTRIGGER_HANDLING.FASTERFRAMERATE))
                {
                    MyShowStatusNG("Failed to set internal trigger handling", internalTriggerHandlingProp.m_lasterr);
                    return false;                         // Fail: setting internal trigger handling
                }

                // * Binning and Subarray * //
                // Set binning size to 4x4
                MyDcamProp binningProp = new MyDcamProp(mydcam, DCAMIDPROP.BINNING);
                if (!binningProp.setvalue(4.0))
                {
                    MyShowStatusNG("Failed to set binning size", binningProp.m_lasterr);
                    return false;                         // Fail: setting binning size
                }

                // Set subarray HSIZE to 128
                MyDcamProp subarrayHSizeProp = new MyDcamProp(mydcam, DCAMIDPROP.SUBARRAYHSIZE);
                if (!subarrayHSizeProp.setvalue(128.0))
                {
                    MyShowStatusNG("Failed to set subarray HSIZE", subarrayHSizeProp.m_lasterr);
                    return false;                         // Fail: setting subarray HSIZE
                }

                // Set subarray VSIZE to 128
                MyDcamProp subarrayVSizeProp = new MyDcamProp(mydcam, DCAMIDPROP.SUBARRAYVSIZE);
                if (!subarrayVSizeProp.setvalue(128.0))
                {
                    MyShowStatusNG("Failed to set subarray VSIZE", subarrayVSizeProp.m_lasterr);
                    return false;                         // Fail: setting subarray VSIZE
                }

                // * Trigger Modes and Speed * //
                // Set readout speed to fastest (3)
                MyDcamProp readoutspeedprop = new MyDcamProp(mydcam, DCAMIDPROP.READOUTSPEED);
                if (!readoutspeedprop.setvalue((double)DCAMPROP.READOUTSPEED.FASTEST)) // Use correct constant for fastest speed
                {
                    MyShowStatusNG("Failed to set readout speed", readoutspeedprop.m_lasterr);
                    return false; // Exit on failure
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during camera configuration: {ex.Message}");
                MyShowStatusNG("Exception during configuration", DCAMERR.TIMEOUT);
                return false;
            }
        }

        public Boolean ApplySelectedTriggerSource()
        {
            if (mydcam == null)
            {
                MyShowStatus("Camera is not initialized, please click Init...");
                return false;
            }

            // Ensure the call to `SelectedIndex` is done on the UI thread. This was added by rhys
            int selectedIndex = 2;
            if (window.comboTriggerSource.InvokeRequired)
            {
                // If not on the UI thread, use Invoke to marshal the call
                window.comboTriggerSource.Invoke(new Action(() =>
                {
                    selectedIndex = window.comboTriggerSource.SelectedIndex;
                }));
            }
            else
            {
                // If already on the UI thread, simply access the property
                selectedIndex = window.comboTriggerSource.SelectedIndex;
            }

            MyDcamProp triggerSourceProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);
            MyDcamProp triggerModeProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGER_MODE);
            MyDcamProp triggerActiveModeProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERACTIVE);
            MyDcamProp triggerPolarityProp = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERPOLARITY);

            bool success = false;

            switch (selectedIndex)
            {
                case 0: // Internal Trigger
                    if (triggerSourceProp.setvalue(DCAMPROP.TRIGGERSOURCE.INTERNAL))
                    {
                        MyShowStatusOK("Trigger source set to Internal.");
                        Console.WriteLine("Trigger source set to Internal Trigger.");
                        success = true;
                    }
                    else
                    {
                        MyShowStatusNG("Failed to set trigger source to Internal", triggerSourceProp.m_lasterr);
                    }
                    break;

                case 1: // External Start Trigger

                    // Step 1: First, set it to External Edge Mode
                    if (!triggerModeProp.setvalue(DCAMPROP.TRIGGER_MODE.NORMAL))
                    {
                        MyShowStatusNG("Failed to set trigger mode to NORMAL before External Start", triggerModeProp.m_lasterr);
                        return false;
                    }

                    if (!triggerActiveModeProp.setvalue(DCAMPROP.TRIGGERACTIVE.EDGE))
                    {
                        MyShowStatusNG("Failed to set trigger active mode to EDGE", triggerActiveModeProp.m_lasterr);
                        return false;
                    }

                    if (!triggerPolarityProp.setvalue(DCAMPROP.TRIGGERPOLARITY.POSITIVE))
                    {
                        MyShowStatusNG("Failed to set trigger polarity to POSITIVE", triggerPolarityProp.m_lasterr);
                        return false;
                    }

                    if (!triggerSourceProp.setvalue(DCAMPROP.TRIGGERSOURCE.EXTERNAL))
                    {
                        MyShowStatusNG("Failed to set trigger source to EXTERNAL before External Start", triggerSourceProp.m_lasterr);
                        return false;
                    }

                    // Step 2: Now switch to External Start
                    if (!triggerActiveModeProp.setvalue(DCAMPROP.TRIGGERACTIVE.EDGE))
                    {
                        MyShowStatusNG("Failed to set trigger active mode to EDGE", triggerActiveModeProp.m_lasterr);
                        return false;
                    }

                    if (!triggerPolarityProp.setvalue(DCAMPROP.TRIGGERPOLARITY.POSITIVE))
                    {
                        MyShowStatusNG("Failed to set trigger polarity to POSITIVE", triggerPolarityProp.m_lasterr);
                        return false;
                    }

                    if (!triggerModeProp.setvalue(DCAMPROP.TRIGGER_MODE.START))
                    {
                        MyShowStatusNG("Failed to set trigger mode to START", triggerModeProp.m_lasterr);
                        return false;
                    }

                    // Verify trigger source
                    double triggerCheck = 0;
                    triggerSourceProp.getvalue(ref triggerCheck);
                    triggerModeProp.getvalue(ref triggerCheck);
                    if (triggerCheck == (double)DCAMPROP.TRIGGERSOURCE.EXTERNAL || triggerCheck == (double)DCAMPROP.TRIGGER_MODE.START)
                    {
                        MyShowStatusOK("Trigger source set to External Start Trigger.");
                        Console.WriteLine("Trigger source set to External Start Trigger.");
                        success = true;
                    }
                    else
                    {
                        MyShowStatusNG("Trigger source verification failed: It did not switch to External Start.", triggerSourceProp.m_lasterr);
                    }
                    break;

                case 2: // External Edge Trigger
                    if (!triggerModeProp.setvalue(DCAMPROP.TRIGGER_MODE.NORMAL))
                    {
                        MyShowStatusNG("Failed to set trigger mode to NORMAL", triggerModeProp.m_lasterr);
                        return false;
                    }

                    if (!triggerActiveModeProp.setvalue(DCAMPROP.TRIGGERACTIVE.EDGE))
                    {
                        MyShowStatusNG("Failed to set trigger active mode to EDGE", triggerActiveModeProp.m_lasterr);
                        return false;
                    }

                    if (!triggerPolarityProp.setvalue(DCAMPROP.TRIGGERPOLARITY.POSITIVE))
                    {
                        MyShowStatusNG("Failed to set trigger polarity to POSITIVE", triggerPolarityProp.m_lasterr);
                        return false;
                    }

                    if (triggerSourceProp.setvalue(DCAMPROP.TRIGGERSOURCE.EXTERNAL))
                    {
                        MyShowStatusOK("Trigger source set to External Edge Trigger.");
                        Console.WriteLine("Trigger source set to External Edge Trigger.");
                        success = true;
                    }

                    else
                    {
                        MyShowStatusNG("Failed to set trigger source to External Edge", triggerSourceProp.m_lasterr);
                    }
                    break;

                default:
                    MyShowStatusNG("Invalid trigger selection", DCAMERR.INVALIDVALUE);
                    return false;
            }

            return success;
        }

        public void QuerySensorTemperature()
        {
            MyDcamProp sensortemperatureprop = new MyDcamProp(mydcam, DCAMIDPROP.SENSORTEMPERATURE);
            double currentTemperature = 0;
            double roundedTemperature = Math.Round(currentTemperature, 2);
            if (sensortemperatureprop.getvalue(ref roundedTemperature))
            {
                Console.WriteLine($"Current Sensor Temperature: {roundedTemperature:F2} degrees");
                window.SensorTemperatureLabel.Text = $"Current Sensor Temperature: {roundedTemperature:F2} degrees";
            }
            else
            {
                MyShowStatusNG("Failed to query sensor temperature", sensortemperatureprop.m_lasterr);
            }
        }

        public void QueryCCDGain()
        {
            if (mydcam == null)
            {
                Console.WriteLine("Error: mydcam is not initialized.");
                return;
            }

            MyDcamProp sensitivitygainprop = new MyDcamProp(mydcam, DCAMIDPROP.SENSITIVITY);
            double currentSensitivity = 0;
            if (sensitivitygainprop.getvalue(ref currentSensitivity))
            {
                Console.WriteLine($"Current Sensitivity Gain: {currentSensitivity}");

                //NOTE: The invoke functionality prevents a cross thread error when communicating via TCP. The invoke method allows
                //you to communicate with the UI thread (i.e. formMain) from the controller thread. This is needed because normally
                //the UI controls (i.e. labels) within a Windows Form are not thread safe and thus you cannot update them directly from a background thread
                //i.e. the controller thread handling the TCP communication.
                //The invoke method allows you to update the UI controls from the controller thread. Any code that interacts with the UI must be wrapped
                //within the Invoke method. If the function is not called via TCP and is invoked (locally e.g. from a button click
                // then the invoke method is not needed as the function that includes the UI owned controls (e.g. label) is executed by the UI thread itself.
                // This means InvokeRequired boolean will be false and the else statement will be executed.

                if (window.SensitivityGainLabel.InvokeRequired)
                {
                    // If called from a non-UI thread i.e. this thread, we invoke to update the label on the UI thread
                    window.SensitivityGainLabel.Invoke(new Action(() =>
                    {
                        window.SensitivityGainLabel.Text = $"Current Sensitivity Gain: {currentSensitivity}";
                    }));

                }
                else
                {
                    // If already on the UI thread, update the label directly
                    window.SensitivityGainLabel.Text = $"Current Sensitivity Gain: {currentSensitivity}";
                }

            }
            else
            {
                MyShowStatusNG("Failed to query sensitivity gain", sensitivitygainprop.m_lasterr);
            }
        }

        public void RemoteFunction()
        {
            Console.WriteLine("Remote Ping!");
        }

        public void UpdateCCDGain()
        {
            if (mydcam == null)
            {
                Console.WriteLine("Error: mydcam is not initialized.");
                return;
            }

            if (double.TryParse(window.SensitivityGainTextBox.Text, out double newGain))
            {
                MyDcamProp sensitivitygainprop = new MyDcamProp(mydcam, DCAMIDPROP.SENSITIVITY);

                // Query the current value
                double currentGain = 0;
                if (sensitivitygainprop.getvalue(ref currentGain))
                {
                    Console.WriteLine($"Current Sensitivity Gain: {currentGain}");
                }
                else
                {
                    MyShowStatusNG("Failed to query current sensitivity gain", sensitivitygainprop.m_lasterr);
                }

                // Attempt to update the property
                if (sensitivitygainprop.setvalue(newGain))
                {
                    Console.WriteLine($"Updated Sensitivity Gain to {newGain}");
                    //if (window.SensitivityGainLabel.InvokeRequired)
                    //{
                    //    window.SensitivityGainLabel.Invoke(new Action(() =>
                    //    {
                    //        window.SensitivityGainLabel.Text = $"Updated Sensitivity Gain: {newGain}";
                    //    }));
                    //}
                    //else
                    //{
                    //    window.SensitivityGainLabel.Text = $"Updated Sensitivity Gain: {newGain}";
                    //}
                    window.SensitivityGainLabel.Text = $"Updated Sensitivity Gain: {newGain}";
                }
                else
                {
                    Console.WriteLine($"Failed to update sensitivity gain. Error: {sensitivitygainprop.m_lasterr:X}");
                    MyShowStatusNG("Failed to update sensitivity gain", sensitivitygainprop.m_lasterr);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid number for Sensitivity Gain.");
            }
        }

        double currentExposureTime;
        public void QueryExposureTime()
        {
            MyDcamProp exposuretimeprop = new MyDcamProp(mydcam, DCAMIDPROP.EXPOSURETIME);

            if (exposuretimeprop.getvalue(ref currentExposureTime))
            {
                double roundedExposureTime = Math.Round(currentExposureTime, 5); // Round to 5 decimal places
                Console.WriteLine($"Current Exposure Time: {roundedExposureTime:F5}");
                
                if (window.ExposureTimeLabel.InvokeRequired)
                {
                    window.ExposureTimeLabel.Invoke(new Action(() =>
                    {
                        window.ExposureTimeLabel.Text = $"Current Exposure Time: {roundedExposureTime:F5} seconds";
                    }));
                
                }
                else
                {
                    window.ExposureTimeLabel.Text = $"Current Exposure Time: {roundedExposureTime:F5} seconds";
                }
            }
            else
            {
                MyShowStatusNG("Failed to query exposure time", exposuretimeprop.m_lasterr);
            }
        }

        public void UpdateExposureTime()
        {
            if (double.TryParse(window.ExposureTimeTextBox.Text, out double newExposureTime))
            {
                MyDcamProp exposuretimeprop = new MyDcamProp(mydcam, DCAMIDPROP.EXPOSURETIME);

                // Query the current value 
                double roundedExposureTime = 0;
                if (exposuretimeprop.getvalue(ref roundedExposureTime))
                {
                    Console.WriteLine($"Current Exposure Time: {roundedExposureTime:F2}");
                }
                else
                {
                    MyShowStatusNG("Failed to query current exposure time", exposuretimeprop.m_lasterr);
                }

                // Attempt to update the property
                if (exposuretimeprop.setvalue(newExposureTime))
                {
                    Console.WriteLine($"Updated Exposure Time to {newExposureTime:F2}");
                    
                    if (window.ExposureTimeLabel.InvokeRequired)
                    {
                        window.ExposureTimeLabel.Invoke(new Action(() =>
                            {
                                window.ExposureTimeLabel.Text = $"Updated Exposure Time: {newExposureTime:F2} seconds";
                            }));
                    }
                    else
                    {
                        window.ExposureTimeLabel.Text = $"Updated Exposure Time: {newExposureTime:F2} seconds";
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to update exposure time. Error: {exposuretimeprop.m_lasterr:X}");
                    MyShowStatusNG("Failed to update exposure time", exposuretimeprop.m_lasterr);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid number for Exposure Time.");
            }
        }

        public void QueryFrameCount()
        {
            if (window.FrameCountLabel.InvokeRequired)
            {
                window.FrameCountLabel.Invoke(new Action(() =>
                {
                    window.FrameCountLabel.Text = $"Current Frame Count: {m_nFrameCount}";
                }));
            }
            else
            {
                window.FrameCountLabel.Text = $"Current Frame Count: {m_nFrameCount}";
            }

            Console.WriteLine($"Queried Frame Count: {m_nFrameCount}");
        }

        public void UpdateFrameCount()
        {
            if (int.TryParse(window.FrameCountTextBox.Text, out int newFrameCount) && newFrameCount > 0)
            {
                m_nFrameCount = newFrameCount;
                Console.WriteLine($"Frame Count updated to {m_nFrameCount}");

                if (window.FrameCountLabel.InvokeRequired)
                {
                    window.FrameCountLabel.Invoke(new Action(() =>
                    {
                        window.FrameCountLabel.Text = $"Current Frame Count: {m_nFrameCount}";
                    }));
                }
                else
                {
                    window.FrameCountLabel.Text = $"Current Frame Count: {m_nFrameCount}";
                }
            }
            else
            {
                Console.WriteLine("Invalid input: Frame count must be a positive integer.");
            }
        }


        public void Snap()
        {
            if (mydcam == null)
            {
                MyShowStatus("Internal Error: mydcam is null");
                Console.WriteLine("Error: mydcam is null");
                //window.MyFormStatus_Initialized();     // FormStatus should be Initialized.
                return;                         // internal error
            }

            window.MyFormStatus_Initialized();     // FormStatus should be Initialized.

            string text = "";

            //m_nFrameCount = 4;

            if (window.IsMyFormStatus_Initialized())
            {
                // if FormStatus is Opened, DCAM buffer is not allocated.
                // So call dcambuf_alloc() to prepare capturing.

                text = string.Format("dcambuf_alloc({0})", m_nFrameCount);

                // allocate m_nFrameCount frames to the buffer
                if (!mydcam.buf_alloc(m_nFrameCount))
                {
                    // allocation was failed
                    Console.WriteLine("Failed to allocate buffer");
                    MyShowStatusNG("Failed to allocate buffer", mydcam.m_lasterr);
                    //window.MyFormStatus_Initialized(); // Reset form status to initialized
                    return;                     // Fail: dcambuf_alloc()
                }

                // Success: dcambuf_alloc()
                update_lut(true);
            }

            // start acquisition
            m_cap_stopping = false;
            mydcam.m_capmode = DCAMCAP_START.SNAP;    // one time capturing.  Acqusition will stop after capturing m_nFrameCount frame
            if (!mydcam.cap_start(this))
            {
                // acquisition was failed. In this sample, frame buffer is also released.
                MyShowStatusNG("Failed to start capturing", mydcam.m_lasterr);

                mydcam.buf_release();           // release unnecessary buffer in DCAM
                //window.MyFormStatus_Initialized();          // change dialog FormStatus to Initialized
                return;                         // Fail: dcamcap_start()
            }
            else
            {
                // Acquisition started successfully
                MyShowStatusOK("Capture started successfully.");

                // Log message to indicate successful start
                Console.WriteLine("Capture started successfully.");
            }

            // Success: dcamcap_start()
            // acquisition has started

            MyShowStatusOK(text + "dcamcap_start()");

            MyDcamProp prop = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);

            double v = 0;
            prop.getvalue(ref v);

            if (v == DCAMPROP.TRIGGERSOURCE.SOFTWARE)
            {
                //window.MyFormStatus_AcquiringSoftwareTrigger(); // change dialog FormStatus to AcquiringSoftwareTrigger
            }
            else
            {
                //window.MyFormStatus_Acquiring();           // change dialog FormStatus to Acquiring
            }

            MyThreadCapture_Start();            // start monitoring thread


            using (mydcamwait = new MyDcamWait(ref mydcam))
            {
                while (true)
                {
                    DCAMWAIT eventmask = DCAMWAIT.CAPEVENT.FRAMEREADY | DCAMWAIT.CAPEVENT.STOPPED;
                    DCAMWAIT eventhappened = DCAMWAIT.NONE;

                    if (mydcamwait.start(eventmask, ref eventhappened))
                    {
                        if (eventhappened & DCAMWAIT.CAPEVENT.FRAMEREADY)
                        {
                            //break;
                        }

                        if (eventhappened & DCAMWAIT.CAPEVENT.STOPPED)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (mydcamwait.m_lasterr == DCAMERR.TIMEOUT)
                        {
                            Console.WriteLine("Failed wait.");
                            break;
                        }
                        else
                        if (mydcamwait.m_lasterr == DCAMERR.ABORT)
                        {
                            Console.WriteLine("Failed wait.");
                            break;
                        }
                    }
                }
            }

            // Stop acquisition after the snap is completed.
            if (mydcam.cap_stop())
            {
                MyShowStatusOK($"{m_nFrameCount} frames has been successfully taken, now please click Save Snaps.");
                window.MyFormStatus_Acquired(); // Update status to Acquired to reflect successful capture.
            }
            else
            {
                MyShowStatusNG("Failed to stop acquisition after snap", mydcam.m_lasterr);
            }
            // Update the image to display the snap  
            MyUpdateImage(0);
            MyUpdateDisplay(); // Refresh the display to show the snap
            //auto_lut();
        }

        public void Live()
        {

            if (mydcam == null)
            {
                MyShowStatus("Internal Error: mydcam is null");
                Console.WriteLine("Error: mydcam is null");
                window.MyFormStatus_Initialized();     // FormStatus should be Initialized.
                return;                         // internal error
            }

            string text = "";

            window.MyFormStatus_Initialized();     // FormStatus should be Initialized.

            //m_nFrameCount = 20;

            if (window.IsMyFormStatus_Initialized())
            {
                // if FormStatus is Opened, DCAM buffer is not allocated.
                // So call dcambuf_alloc() to prepare capturing.

                text = string.Format("dcambuf_alloc({0})", m_nFrameCount);

                // allocate frame buffer
                if (!mydcam.buf_alloc(m_nFrameCount))
                {
                    // allocation was failed
                    MyShowStatusNG("Failed to allocate live images.", mydcam.m_lasterr);
                    window.MyFormStatus_Initialized(); // Reset form status to initialized
                    return;                     // Fail: dcambuf_alloc()
                }

                // Success: dcambuf_alloc()

                update_lut(true);  // Success: update LUT
            }

            // start acquisition
            m_cap_stopping = false;
            mydcam.m_capmode = DCAMCAP_START.SEQUENCE;    // continuous capturing.  continuously acqusition will be done
            if (!mydcam.cap_start(this))
            {
                // acquisition was failed. In this sample, frame buffer is also released.
                MyShowStatusNG("dcamcap_start()", mydcam.m_lasterr);

                //mydcam.buf_release();           // release unnecessary buffer in DCAM
                window.MyFormStatus_Initialized();          // change dialog FormStatus to Opened
                return;                         // Fail: dcamcap_start()
            }

            // Success: dcamcap_start()
            // acquisition has started

            if (text.Length > 0)
            {
                text += " && ";
            }

            MyShowStatusOK("Live capturing...");

            MyDcamProp prop = new MyDcamProp(mydcam, DCAMIDPROP.TRIGGERSOURCE);
            double v = 0;
            prop.getvalue(ref v);

            if (v == DCAMPROP.TRIGGERSOURCE.SOFTWARE)
            {
                window.MyFormStatus_AcquiringSoftwareTrigger(); // change dialog FormStatus to AcquiringSoftwareTrigger
            }
            else
            {
                window.MyFormStatus_Acquiring();           // change dialog FormStatus to Acquiring
            }
            MyThreadCapture_Start();            // start monitoring thread
        }

        // Stop Acquisition button, which stops image acquisition and return the application to a state where the camera is not actively capturing or processing frames.
        public void StopAcquisition()
        {
            if (mydcam == null)
            {
                MyShowStatus("Internal Error: mydcam is null");
                window.MyFormStatus_Initialized();     // FormStatus should be Initialized.
                return;                         // internal error
            }

            if (!window.IsMyFormStatus_Acquiring())
            {
                MyShowStatus("Internal Error: Idle button is only available when FormStatus is Acquiring");
                return;                         // internal error
            }

            // stop acquisition
            m_cap_stopping = true;
            if (!mydcam.cap_stop())
            {
                MyShowStatusNG("dcamcap_stop()", mydcam.m_lasterr);
                return;                         // Fail: dcamcap_stop()
            }

            // Success: dcamcap_stop()

            MyShowStatusOK("Frames capturing is stopped.");
            window.MyFormStatus_Acquired();            // change dialog FormStatus to Acquired
            MyShowStatus("Acquisition stopped, ready for further operations");
        }

        private void SaveFrameAs16BitTiff(Tiff tiff, byte[] rawData, int width, int height, int pageIndex)
        {
            tiff.SetField(TiffTag.IMAGEWIDTH, width);
            tiff.SetField(TiffTag.IMAGELENGTH, height);
            tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1); 
            tiff.SetField(TiffTag.BITSPERSAMPLE, 16);
            //tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
            tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
            tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK); // Black = 0
            tiff.SetField(TiffTag.ROWSPERSTRIP, height);
            tiff.SetField(TiffTag.COMPRESSION, Compression.NONE); // No compression for accuracy

            // Write image data row by row
            int stride = width * 2; // 16-bit per pixel
            for (int row = 0; row < height; row++)
            {
                tiff.WriteScanline(rawData, row * stride, row, 0);
            }

            tiff.WriteDirectory(); // Create a new directory for the next page 
        }


        private string GetNextFileName(string directory, string baseFileName, string extension)
        {
            int fileIndex = 1;
            string fileName;
            do
            {
                fileName = Path.Combine(directory, $"{baseFileName}({fileIndex:D5}){extension}");
                fileIndex++;
            } while (File.Exists(fileName));
            return fileName;
        }

        // Default save directory
        private string saveDirectory = "E:\\Imperial College London\\OneDrive - Imperial College London\\Documents - Team ultracold - PH\\Data\\2025\\CCD data";

        // Button Click Event to Set Save Directory
        public void SetSaveDirectory() 
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to save captured frames";
                folderDialog.SelectedPath = saveDirectory; // Set default directory

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    saveDirectory = folderDialog.SelectedPath; // Update save path
                    Console.WriteLine($"Save directory set to: {saveDirectory}");
                    window.SaveDirectoryLabel.Text = $"Save Directory: {saveDirectory}";
                }
            }
        }
        public void BufRelease()
        {
            if (mydcam == null)
            {
                MyShowStatus("Internal Error: mydcam is null");
                window.MyFormStatus_Initialized(); // FormStatus should be Initialized.
                return; // Internal error
            }

            if (!window.IsMyFormStatus_Acquired())
            {
                MyShowStatus("Internal Error: BufRelease is only available when FormStatus is Acquired");
                return; // Internal error
            }

            Directory.CreateDirectory(saveDirectory);

            string multiTiffPath = GetNextFileName(saveDirectory, "CCD2", ".tiff"); // Get the next available file name

            try
            {
                using (Tiff tiff = Tiff.Open(multiTiffPath, "w")) // Open a new multi-page TIFF
                {
                    for (int i = 0; i < m_nFrameCount; i++)
                    {
                        // Lock the frame to access its data
                        m_image.set_iFrame(i);
                        if (!mydcam.buf_lockframe(ref m_image.bufframe))
                        {
                            MyShowStatusNG($"Failed to lock frame {i}", mydcam.m_lasterr);
                            continue; // Skip if frame locking fails
                        }

                        // Extract raw image data (in 16-bit grayscale)
                        int width = m_image.width;
                        int height = m_image.height;
                        int stride = width * 2; // 16-bit = 2 bytes per pixel

                        byte[] rawData = new byte[height * stride]; // Buffer for pixel data
                        Marshal.Copy(m_image.bufframe.buf, rawData, 0, rawData.Length); // Copy raw data

                        // **Save as 16-bit grayscale TIFF using BitMiracle.LibTiff**
                        SaveFrameAs16BitTiff(tiff, rawData, width, height, i);
                    }
                }

                Console.WriteLine($"{m_nFrameCount} frames saved in MTIFF.");
                MyShowStatusOK($"Saved {m_nFrameCount} frames as a MTIFF at {multiTiffPath}");
            }
            catch (Exception ex)
            {
                MyShowStatusNG($"Error saving TIFF files: {ex.Message}", DCAMERR.SUCCESS);
            }
            finally
            {
                if (!mydcam.buf_release())
                {
                    MyShowStatusNG("dcambuf_release()", mydcam.m_lasterr);
                }
                else
                {
                    MyShowStatusOK($"{m_nFrameCount} frames have been successfully saved.");
                }

                window.MyFormStatus_Initialized(); // Reset FormStatus
                m_image.clear();
            }
        }


        //public void BufRelease()
        //{
        //    if (mydcam == null)
        //    {
        //        MyShowStatus("Internal Error: mydcam is null");
        //        window.MyFormStatus_Initialized();     // FormStatus should be Initialized.
        //        return;                         // internal error
        //    }

        //    if (!window.IsMyFormStatus_Acquired())
        //    {
        //        MyShowStatus("Internal Error: BufRelease is only available when FormStatus is Acquired");
        //        return;                         // internal error
        //    }

        //    // Save images to disk before releasing buffer
        //    string saveDirectory = "E:\\Imperial College London\\Team ultracold - PH - Documents\\Data\\2025\\CCD data";
        //    Directory.CreateDirectory(saveDirectory); // Create the directory if it does not exist 

        //    try
        //    {
        //        for (int i = 0; i < m_nFrameCount; i++)
        //        {
        //            // Lock the frame to access its data
        //            m_image.set_iFrame(i);
        //            if (!mydcam.buf_lockframe(ref m_image.bufframe))
        //            {
        //                MyShowStatusNG($"Failed to lock frame {i}", mydcam.m_lasterr);
        //                continue; // Skip to the next frame if locking fails
        //            }

        //            // Convert the frame to a Bitmap
        //            Bitmap frame;
        //            lock (BitmapLock)
        //            {
        //                frame = new Bitmap(m_image.width, m_image.height, PixelFormat.Format24bppRgb);
        //                Rectangle rc = new Rectangle(0, 0, m_image.width, m_image.height);
        //                SUBACQERR err = subacq.copydib(ref frame, m_image.bufframe, ref rc, m_lut.inmax, m_lut.inmin, m_lut.camerabpp);

        //                if (err != SUBACQERR.SUCCESS)
        //                {
        //                    MyShowStatusNG($"Failed to convert frame {i} to Bitmap", mydcam.m_lasterr);
        //                    frame.Dispose();
        //                    continue;
        //                }
        //            }

        //            // Generate a unique file name for the frame
        //            string tiffFilePath = Path.Combine(saveDirectory, $"Frame_{i + 1:D2}.tiff");

        //            // Check if the file already exists and generate a new name if necessary
        //            int counter = 1;
        //            while (File.Exists(tiffFilePath))
        //            {
        //                tiffFilePath = Path.Combine(saveDirectory, $"Frame_{i + 1:D2}_{counter}.tiff");
        //                counter++;
        //            }

        //            // Save the frame as an individual TIFF file
        //            frame.Save(tiffFilePath, ImageFormat.Tiff);
        //            frame.Dispose(); // Release the frame bitmap
        //        }
        //        Console.WriteLine($"{m_nFrameCount} frames are saving to the directory...");
        //        MyShowStatusOK($"Saved {m_nFrameCount} frames as individual TIFF files to {saveDirectory}");
        //    }
        //    catch (Exception ex)
        //    {
        //        MyShowStatusNG($"Error saving TIFF files: {ex.Message}", DCAMERR.SUCCESS);
        //    }
        //    finally
        //    {
        //        bool isError = false; // Flag to track if any errors occur during buffer release

        //        // Release the buffer only if no error occurred during the main execution
        //        if (!mydcam.buf_release())
        //        {
        //            MyShowStatusNG("dcambuf_release()", mydcam.m_lasterr);
        //            isError = true; // Fail: dcambuf_release()
        //        }

        //        // Success: dcambuf_release()
        //        MyShowStatusOK($"{m_nFrameCount} frames have been successfully saved to the directory.");
        //        Console.WriteLine($"{m_nFrameCount} frames have been successfully saved to the directory.");
        //        window.MyFormStatus_Initialized(); // Change dialog FormStatus to Opened
        //        m_image.clear();

        //        // If there was an error during buffer release, handle accordingly
        //        if (isError)
        //        {
        //            // If you want to show a specific message or handle other things due to failure
        //            MyShowStatus("Buffer release failed, cleanup needed.");
        //        }
        //    }
        //}

        public void AutoIntensity()
        {
            auto_lut();
        }


        #region Remote methods only

        public void RemoteSnap()
        {
            //Task.Run sets up the function to be run Asynchronously 
            Task.Run(()=>Snap());
        }

        public void RemoteBufRelease()
        {
            BufRelease();
        }

        public void RemoteStop()
        {
            StopAcquisition();
        }

        private Thread updateGainThread;
        public void RemoteUpdateGain()
        {
            UpdateCCDGain();
        }

        private Thread queryGainThread;
        public void RemoteQueryCCDGain()
        {

            queryGainThread = new Thread(new ThreadStart(QueryCCDGain));
            queryGainThread.Start();
            //QueryCCDGain();
        }
        #endregion
    }
}



