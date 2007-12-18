using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SympatheticHardwareControl
{
    public partial class ControlWindow : Form
    {
        public Controller controller;
        
        public ControlWindow()
        {
            InitializeComponent();
        }

        private void pressureLabel1_Click(object sender, EventArgs e)
        {

        }

        private void ControlWindow_Load(object sender, EventArgs e)
        {
            controller.WindowLoaded();
        }

        public void outputAIdata(TextBox box, string text)
        {
        	box.Invoke(new SetTextDelegate(SetTextHelper), new object[] {box,text});
        }
        private delegate void SetTextDelegate(TextBox box, String text);
        public void SetTextHelper(TextBox box, String text)
        {
            box.Text = text;
        }
              
    }
}
  