using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoleculeMOTHardwareControl.Controls
{
    public partial class GenericView : UserControl
    {
        public GenericController controller; 

        public GenericView(GenericController controllerInstance)
        {
            controller = controllerInstance;
            InitializeComponent();
        }

    }
}
