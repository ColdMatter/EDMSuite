using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using NationalInstruments.UI.WindowsForms;
using NationalInstruments.UI;

using NationalInstruments.Vision;
using NationalInstruments.Vision.Acquisition.Imaqdx;
using NationalInstruments.Vision.Internal;
using NationalInstruments.Vision.WindowsForms.Internal;

namespace IMAQ
{
    public partial class ImageViewerWindow : Form
    {
        public CameraController IM;

        public ImageViewerWindow()
        {
            InitializeComponent();
        }


        #region ThreadSafe wrappers

        //An irritating number of threadsafe delegates for the viewer controlWindow.
        private void attachToViewer(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image)
        {
            viewer.Invoke(new AttachImageToViewerDelegate(AttachImageHelper), new object[] { viewer, image });
        }

        private delegate void AttachImageToViewerDelegate(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image);
        private void AttachImageHelper(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image)
        {
            viewer.Attach(image);
            
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

        #endregion

        #region Public methods

        public void AttachToViewer(VisionImage image)
        {
            attachToViewer(imageViewer, image);
        }

        private VisionImage disImage =new VisionImage();
        public void AttachImagesToViewer(List<VisionImage> images, int frame)
        {
           
             disImage=images[frame];
             attachToViewer(imageViewer, disImage);
            

        }

        /*
        public VisionImage Image
        {
            get
            {
                return imageViewer.Image;
            }
            set
            {
                attachToViewer(imageViewer, value);
            }
        }*/
        public void WriteToConsole(string text)
        {
            setRichTextBox(consoleRichTextBox, ">> " + text + "\n");

        }
        #endregion

        private void ImageViewerWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            IM.Dispose();
        }

           private void hScrollBar_Change(int newScrollValue)
        {
            AttachImagesToViewer(IM.imageList, newScrollValue);

            hScrollBar.Maximum = IM.imageList.Count-1;
            
        }



        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
           
            if(IM.imageList.Count == 0)

            {
                hScrollBar.Update();
                hScrollBar.Maximum = 1;
                

            }


            else
            {
           
            hScrollBar_Change(e.NewValue);
            }
           
           

        }

        private void imageViewer_RoiChanged(object sender, NationalInstruments.Vision.WindowsForms.ContoursChangedEventArgs e)
        {

        }

        private void consoleRichTextBox_TextChanged(object sender, EventArgs e)
        {

        }


      


        


    }
}
