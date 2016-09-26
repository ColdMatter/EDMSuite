using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using IMAQ;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : Window
    {
        public ImageViewer()
        {
            InitializeComponent();
            ImageViewerWindow imageWindow = new ImageViewerWindow();
            imageWindow.TopLevel = false;
            imageWindow.FormBorderStyle = FormBorderStyle.None;
            wfHost.Child = imageWindow;
        }

    }
}
