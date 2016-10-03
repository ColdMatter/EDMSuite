using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using IMAQ;
using RFMOTHardwareControl;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : Window
    {
        public ImageAnalysisWindow imageWindow;
        public ImageViewer()
        {
            InitializeComponent();
            imageWindow = new ImageAnalysisWindow();
            imageWindow.TopLevel = false;
            imageWindow.FormBorderStyle = FormBorderStyle.None;
            wfHost.Child = imageWindow;
         
        }

    }
}
