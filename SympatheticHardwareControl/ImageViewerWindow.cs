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

namespace SympatheticHardwareControl.CameraControl
{
    public partial class ImageViewerWindow : Form
    {
        public Controller controller;

        public ImageViewerWindow()
        {
            InitializeComponent();
        }


        #region ThreadSafe wrappers

        //An irritating number of threadsafe delegates for the viewer controlWindow.
        public void AttachToViewer(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image)
        {
            viewer.Invoke(new AttachImageToViewerDelegate(AttachImageHelper), new object[] { viewer, image });
        }

        private delegate void AttachImageToViewerDelegate(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image);
        private void AttachImageHelper(NationalInstruments.Vision.WindowsForms.ImageViewer viewer, VisionImage image)
        {
            viewer.Attach(image);
        }

        #endregion

        #region Public methods
 
        public VisionImage Image
        {
            get
            {
                return imageViewer.Image;
            }
            set
            {
                AttachToViewer(imageViewer, value);
            }
        }

        #endregion
    }
}
