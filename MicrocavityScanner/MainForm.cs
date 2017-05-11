using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicrocavityScanner.GUI
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        // the application controller
        public Controller controller;

        public MainForm(Controller controller)
        {
            this.controller = controller;
            InitializeComponent();
        }
    }
}
