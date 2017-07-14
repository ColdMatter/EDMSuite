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
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MOTMaster2
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class HardwareOptions : Window
    {
        public HardwareOptions()
        {
            InitializeComponent();
            string fileJson = JsonConvert.SerializeObject(DAQ.Environment.Environs.FileSystem);
            hardwareJson = JsonConvert.SerializeObject(DAQ.Environment.Environs.Hardware, Formatting.Indented);
            LoadJsonToTreeView(hardwareTreeView, hardwareJson);
            LoadJsonToTreeView(filesystemTreeView, fileJson);
        }

        string hardwareJson = "";
        void LoadJsonToTreeView(TreeView treeView, string json)
        {
            var token = JToken.Parse(json);

            var children = new List<JToken>();
            if (token != null)
            {
                children.Add(token);
            }

            treeView.ItemsSource = null;
            treeView.Items.Clear();
            treeView.ItemsSource = children;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (btnModify.Content.Equals("Verify"))
                if (!CheckHardwareJson()) return;

            Close();
        }

        private void tabCtrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((tabCtrl.SelectedIndex == 0) || (tabCtrl.SelectedIndex == 2)) btnModify.Visibility = System.Windows.Visibility.Visible;
            else btnModify.Visibility = System.Windows.Visibility.Hidden;
        }

        private bool CheckHardwareJson()
        {
            rtbModify.SelectAll();
            try
            {//json consistency
                JContainer.Parse(rtbModify.Selection.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " (try again)");
                return false;
            }
            return true;
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            if (btnModify.Content.Equals("Modify"))
            {
                btnModify.Content = "Verify";
                tiHardware.Visibility = System.Windows.Visibility.Hidden;
                tiFileSystem.Visibility = System.Windows.Visibility.Hidden;
                tiModify.Visibility = System.Windows.Visibility.Visible;

                tabCtrl.SelectedIndex = 2;
                rtbModify.Document.Blocks.Clear();
                rtbModify.AppendText(hardwareJson);
            }
            else
            {
                if (!CheckHardwareJson()) return;
                btnModify.Content = "Modify";
                tabCtrl.SelectedIndex = 0;
                tiModify.Visibility = System.Windows.Visibility.Hidden;
                tiHardware.Visibility = System.Windows.Visibility.Visible;
                tiFileSystem.Visibility = System.Windows.Visibility.Visible;
            }
        }

    }
    public sealed class MethodToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var methodName = parameter as string;
            if (value == null || methodName == null)
                return null;
            var methodInfo = value.GetType().GetMethod(methodName, new Type[0]);
            if (methodInfo == null)
                return null;
            return methodInfo.Invoke(value, new object[0]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }
    }
}