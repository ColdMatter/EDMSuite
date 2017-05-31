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
using MOTMaster2.Sequence;
using System.Dynamic;
using System.Collections.ObjectModel;
using System.ComponentModel;

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



    public SequenceStepViewModel()
    {
        SequenceSteps = new ObservableCollection<SequenceStep>();
        SequenceSteps.Add(new SequenceStep() { name = "Init", description = "Intialisation", duration = 1.0, enabled = true });
        SequenceSteps.Add(new SequenceStep() { name = "Second", description = "False", duration = 2.0, enabled = false });

    }

    public event PropertyChangedEventHandler PropertyChanged;
    }
}
