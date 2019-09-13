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

        private GenericView()
        {
            // This is just required for the designer to work
            // The constructor that takes controllerInstance argument below should be the one used
            InitializeComponent();
        }

        public GenericView(GenericController controllerInstance)
        {
            controller = controllerInstance;
            InitializeComponent();
        }

    }
}
