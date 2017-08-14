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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MOTMaster2
{
    /// <summary>
    /// Interaction logic for AnalogInputChart.xaml
    /// </summary>
    public partial class AnalogInputChart : UserControl
    {
        public static ExperimentData ExpData {get;set;}
        public AnalogInputChart()
        {
            InitializeComponent();
            DataContext = this;
            if (Controller.expData == null) Controller.expData = new ExperimentData();
            ExpData = Controller.expData;
            //sampleRateTextBox.DataContext = ExpData.SampleRate;
            //nsampleTextBox.DataContext = ExpData.NSamples;

        }

    }
}
