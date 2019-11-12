using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SirCachealot
{
    public partial class LogInToMySQLDialog : Form
    {
        public LogInToMySQLDialog()
        {
            InitializeComponent();
        }

        public string GetUsername()
        {
            return usernameTextBox.Text;
        }

        public string GetPassword()
        {
            return passwordTextBox.Text;
        }
    }
}
