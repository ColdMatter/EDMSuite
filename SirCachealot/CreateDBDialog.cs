using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SirCachealot.Database
{
    public partial class CreateDBDialog : Form
    {
        public CreateDBDialog()
        {
            InitializeComponent();
        }

        public string GetName()
        {
            return textBox1.Text;
        }
    }
}