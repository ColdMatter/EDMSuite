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
using System.Windows.Shapes;
using MOTMaster2.SequenceData;
using System.Dynamic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;


namespace MOTMaster2
{
    class SequenceStepViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SequenceStep> SequenceSteps { get; set; }

        private SequenceStep _selectedStep;
        public SequenceStep SelectedSequenceStep
        {
            get { return _selectedStep; }
            set
            {
                _selectedStep = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedSequenceStep"));
            }
        }

        private KeyValuePair<string, AnalogChannelSelector> _selectedAnalogChannel;
        public KeyValuePair<string, AnalogChannelSelector> SelectedAnalogChannel
        {
            get { return _selectedAnalogChannel; }
            set
            {
                _selectedAnalogChannel = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedAnalogChannel"));
            }
        }

        private bool _rs232Enabled;
        public bool RS232Enabled
        {
            get { return _rs232Enabled; }
            set
            {
                _rs232Enabled = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("EnabledRS232"));
            }
        }

        public SequenceStepViewModel()
        {

            if (Controller.sequenceData == null)
            {
                SequenceSteps = new ObservableCollection<SequenceStep>();
                SequenceSteps.Add(new SequenceStep());

            }
            else
            {
                SequenceSteps = new ObservableCollection<SequenceStep>(Controller.sequenceData.Steps);
            }

            this.PropertyChanged += SequenceStep.SequenceStep_PropertyChanged;
        }


        private RelayCommand newSequenceStep;
        public RelayCommand NewSequenceStep
        {
            get
            {
                if (newSequenceStep == null)
                {
                    newSequenceStep = new RelayCommand(param => this.AddStep());
                }
                return newSequenceStep;
            }
        }

        public void AddStep()
        {
            SequenceSteps.Add(new SequenceStep());
        }

        private RelayCommand deleteSequenceStep;
        public RelayCommand DeleteSequenceStep
        {
            get
            {
                if (deleteSequenceStep == null)
                {
                    deleteSequenceStep = new RelayCommand(param => this.DeleteStep());
                }
                return deleteSequenceStep;
            }
        }
        public void DeleteStep()
        {
            SequenceSteps.Remove(_selectedStep);
        }

        private RelayCommand duplicateSequenceStep;
        public RelayCommand DuplicateSequenceStep
        {
            get
            {
                if (duplicateSequenceStep == null)
                {
                    duplicateSequenceStep = new RelayCommand(param => this.DuplicateStep());
                }
                return duplicateSequenceStep;
            }
        }
        public void DuplicateStep()
        {
            int index = SequenceSteps.IndexOf(_selectedStep);
            SequenceSteps.Insert(index, _selectedStep);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Updates the selected property (analog channel, rs232 commands) with the given arguments
        /// </summary>
        /// <param name="arguments"></param>
        internal void UpdateChannelValues(object arguments)
        {
            if (arguments.GetType() == typeof(List<AnalogArgItem>))
            {
                List<AnalogArgItem> items = arguments as List<AnalogArgItem>;
                _selectedStep.SetAnalogDataItem(_selectedAnalogChannel.Key, _selectedAnalogChannel.Value, items);

            }
            else if (arguments.GetType() == typeof(List<SerialItem>))
            {
                List<SerialItem> items = arguments as List<SerialItem>;
                _selectedStep.SetSerialCommands(items);
            }
            else throw new Exception("Incorrect argument passed to UpdateChannelValues");
        }
    }

}
