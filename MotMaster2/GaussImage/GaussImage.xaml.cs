using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MOTMaster2
{
    /// <summary>
    /// Interaction logic for GaussImage.xaml
    /// </summary>
    public partial class GaussImage : UserControl
    {
        public GaussImage()
        {
            InitializeComponent();
        }

        public bool LoadImage(string imagePath)
        {
            FileStream imageStream = File.OpenRead(imagePath);
            PngBitmapDecoder pngDecoder = new PngBitmapDecoder(imageStream,
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            BitmapSource img = pngDecoder.Frames[0];

            int depth = 4;
            int stride = img.PixelWidth * depth;
            int size = img.PixelHeight * stride;
            byte[] pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);

            byte[,] array = new byte[img.PixelWidth, img.PixelWidth];
            //int index = y * stride + depth * x;

            return true;
        }
    }
}
