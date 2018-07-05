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
using MOTMaster2.SequenceData;
using System.Collections.ObjectModel;
using UtilsNS;

namespace MOTMaster2
{
    /// <summary>
    /// Interaction logic for ParametersWindow.xaml
    /// </summary>
    public partial class ParametersWindow : Window
    {
        private ObservableCollection<Parameter> _sequenceParameters;
        private Parameter _selectedParameter;
        public ParametersWindow()
        {
            InitializeComponent();
            _sequenceParameters = new ObservableCollection<Parameter>();
            foreach (Parameter p in Controller.sequenceData.Parameters.Values)
            {
                //if (!p.IsHidden) _sequenceParameters.Add(p.Copy());
                _sequenceParameters.Add(p.Copy());
            }
            parameterGrid.ItemsSource = _sequenceParameters;
            parameterGrid.DataContext = this;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            //TODO Remove parameters from SequenceData which have been removed from _sequenceParameters
            foreach (Parameter p in _sequenceParameters)
            {
                Parameter p1 = p.Copy();
                if (Utils.isNumeric(p1.Value))
                    p1.Value = Convert.ToDouble(p.Value);
                Controller.sequenceData.Parameters[p.Name] = p1;
            }
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private RelayCommand newParameter;
        public RelayCommand NewParameter
        {
            get
            {
                if (newParameter == null)
                {
                    newParameter = new RelayCommand(param => this.AddParameter());
                }
                return newParameter;
            }
        }

        public void AddParameter()
        {
            _sequenceParameters.Add(new Parameter());
        }

        private RelayCommand deleteParameter;
        public RelayCommand DeleteParameter
        {
            get
            {
                if (deleteParameter == null)
                {
                    deleteParameter = new RelayCommand(param => this.RemoveParameter());
                }
                return deleteParameter;
            }
        }
        public void RemoveParameter()
        {
            if (parameterGrid.Items.IndexOf(parameterGrid.CurrentItem) < _sequenceParameters.Count)
            {
                _sequenceParameters.RemoveAt(parameterGrid.Items.IndexOf(parameterGrid.CurrentItem));
            }
        }

        private void frmParameters_PreviewKeyDown(object sender, KeyEventArgs e)
        {            
            if ((e.Key == Key.F4) || (e.Key == Key.Return))
            {
                btnOK.Focus();
                OK_Click(sender, null);
            }
            if (e.Key == Key.Escape)
            {
                Cancel_Click(sender, null);
            }      
        }

    }
}
