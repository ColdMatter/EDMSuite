using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LatticeEDMController
{
    public partial class LatticeEDMController : Form
    {
        public LatticeEDMController()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LatticeController.SetSetpointHelium();
        }

        public void SetTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }
        public delegate void SetTextDelegate(TextBox box, string text);


        public void SetTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }
    }
}
