using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlFHardwareControl
{
    public partial class ParamSet : UserControl
    {
        public event EventHandler OnSetClick;

        public ParamSet()
        {
            InitializeComponent();
        }

        public class ParamEventArgs : EventArgs
        {
            public string Param { get; set; }
        }

        public string CurrentValue
        {
            set { this.Invoke((Action)(() => { this.Param_Value.Text = value; })); }
        }
        public string Label
        {
            get { return Param_Name.Text; }
            set { Param_Name.Text = value; }
        }

        private void Param_Set_Click(object sender, EventArgs e)
        {
            EventHandler handler = this.OnSetClick;
            if (handler != null)
            {
                ParamEventArgs eargs = new ParamEventArgs();
                eargs.Param = this.Param_Target.Text;
                handler(sender,eargs);
            }
        }
    }
}
