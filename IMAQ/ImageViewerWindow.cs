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
using NationalInstruments.Vision.WindowsForms;
using NationalInstruments;

namespace IMAQ
{
    public partial class ImageViewerWindow : Form
    {
        public CameraController IM;

        public ImageViewerWindow()
        {
            InitializeComponent();
            imageViewer.ImageMouseDown += displayPointClicked;
            imageViewer.ShowToolbar = true;
            //imageViewer.RoiChanged += newRoi;
            //imageViewer.Conto
        }


        #region ThreadSafe wrappers

        //An irritating number of threadsafe delegates for the viewer controlWindow.

        private void imageViewer_RoiChanged(object sender, ContoursChangedEventArgs e)
        {
            if (e.Action == ContoursChangedAction.Add && imageViewer.ActiveTool == ViewerTools.Point)
            {
                if (IM.pointROI.Count == 0)
                {
                    e.NewItems[0].CopyTo(IM.pointROI);
                }
                else
                {
                    IM.pointROI.RemoveAt(0);
                    e.NewItems[0].CopyTo(IM.pointROI);
                }
            }
            if (e.Action == ContoursChangedAction.Add && imageViewer.ActiveTool == ViewerTools.Rectangle)
            {
                if (IM.rectangleROI.Count == 0)
                {
                    e.NewItems[0].CopyTo(IM.rectangleROI);
                }
                else
                {
                    IM.rectangleROI.RemoveAt(0);
                    e.NewItems[0].CopyTo(IM.rectangleROI);
                }
            }
            if (e.Action == ContoursChangedAction.Add)
            {
                IM.copyContoursToViewerROI();
            }
        }

        private void displayPointClicked(object sender, ImageMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && IM.IsCameraFree())
            {
                
                if (!IM.roiSet)
                {
                    IM.SetROI();
                    WriteToConsole("Setting new Region of Interest");
                }
                else
                {
                    IM.ClearROI(2452, 2054);
                    WriteToConsole("Clearing Region of Interest");
                }

            }
            PointContour point = e.Point;
            IM.pointOfInterest = e.Point;

        }



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

        private VisionImage disImage = new VisionImage();
        public void AttachImagesToViewer(List<VisionImage> images, int frame)
        {

            disImage = images[frame];
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

        //   private void hScrollBar_Change(int newScrollValue)
        //{
        //    AttachImagesToViewer(IM.imageList, newScrollValue);

        //    hScrollBar.Maximum = IM.imageList.Count-1;

        //}



        //private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        //{

        //    if(IM.imageList.Count == 0)

        //    {
        //        hScrollBar.Update();
        //        hScrollBar.Maximum = 1;


        //    }


        //    else
        //    {

        //    hScrollBar_Change(e.NewValue);
        //    }



        //}




    }
}