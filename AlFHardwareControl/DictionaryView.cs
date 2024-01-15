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
    public partial class DictionaryView : Form
    {
        public DictionaryView()
        {
            InitializeComponent();
        }

        private MOTMasterStuff mmStuff;

        public DictionaryView(MOTMasterStuff _mmstuff)
        {
            InitializeComponent();
            mmStuff = _mmstuff;
        }

        private void DictionaryView_FormClosing(object sender, FormClosingEventArgs e)
        {
            mmStuff.DictionaryViewClosing();
        }
    }
}
