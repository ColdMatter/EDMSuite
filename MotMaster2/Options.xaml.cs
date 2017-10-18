using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
using UtilsNS;

namespace MOTMaster2
{

    public class GeneralOptions
    {
        public enum SaveOption { save, ask, nosave }

        public SaveOption saveSequence;

        public enum DataLogOption { allData, average}

        public DataLogOption dataLog;

        public enum M2CommOption { on, off}
        public M2CommOption m2Comm;

        public void Save()
        {
            string fileJson = JsonConvert.SerializeObject(this);
            File.WriteAllText(Utils.configPath + "genOptions.cfg", fileJson);
        }

    }

    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class OptionWindow : Window
    {
        public OptionWindow()
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

        private void OKButton_Click(object sender, RoutedEventArgs e) // visual to internal 
        {
            if (rbSaveSeqYes.IsChecked.Value) Controller.genOptions.saveSequence = GeneralOptions.SaveOption.save;
            if (rbSaveSeqAsk.IsChecked.Value) Controller.genOptions.saveSequence = GeneralOptions.SaveOption.ask;
            if (rbSaveSeqNo.IsChecked.Value) Controller.genOptions.saveSequence = GeneralOptions.SaveOption.nosave;

            if (m2Off.IsChecked.Value) Controller.genOptions.m2Comm = GeneralOptions.M2CommOption.off;
            if (m2On.IsChecked.Value) Controller.genOptions.m2Comm = GeneralOptions.M2CommOption.on;
            Close();
        }

        private void tabCtrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        private void frmOptions_Loaded(object sender, RoutedEventArgs e) // internal to visual
        {
            rbSaveSeqYes.IsChecked = Controller.genOptions.saveSequence.Equals(GeneralOptions.SaveOption.save);
            rbSaveSeqAsk.IsChecked = Controller.genOptions.saveSequence.Equals(GeneralOptions.SaveOption.ask);
            rbSaveSeqNo.IsChecked = Controller.genOptions.saveSequence.Equals(GeneralOptions.SaveOption.nosave);

            m2On.IsChecked = Controller.genOptions.m2Comm.Equals(GeneralOptions.M2CommOption.on);
            m2Off.IsChecked = Controller.genOptions.m2Comm.Equals(GeneralOptions.M2CommOption.off);
       }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
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