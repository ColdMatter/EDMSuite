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
            Controller.sequenceData.Parameters.ForEach((item) => { if(!item.IsHidden) _sequenceParameters.Add((Parameter)item.Copy());});

            parameterGrid.ItemsSource = _sequenceParameters;
            parameterGrid.DataContext = this;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Controller.sequenceData.Parameters = _sequenceParameters.ToList();
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

    }
}
