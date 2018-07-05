using System;
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
using System.Windows.Shapes;

namespace MOTMaster2
{
    public class MetaData
    {
        public string desc;
    }
    /// <summary>
    /// Interaction logic for MetaData.xaml
    /// </summary>
    public partial class MetaDataWindow : Window
    {
        public MetaDataWindow()
        {
            InitializeComponent();
            metaData = new MetaData();
        }
        public MetaData metaData;
        private void OKButton_Click(object sender, RoutedEventArgs e) // visual to internal 
        {
            metaData.desc = tbDesc.Text;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
