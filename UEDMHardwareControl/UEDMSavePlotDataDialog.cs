using System.Windows.Forms;

namespace UEDMHardwareControl
{
    public partial class UEDMSavePlotDataDialog : Form
    {
        public UEDMSavePlotDataDialog()
        {
            InitializeComponent();
        }

        public UEDMSavePlotDataDialog(string title, string description)
        {
            InitializeComponent();

            this.Text = title;
            MessageDescription.Text = description;

            btSave.DialogResult = DialogResult.Yes;
            btDoNotSave.DialogResult = DialogResult.No;
            btCancel.DialogResult = DialogResult.Cancel;
        }
    }
}
