using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace LaserLock
{
    public partial class MainForm : Form
    {

        private LaserController controller;
        private Thread controlThread;
        
        public MainForm()
        {
            InitializeComponent();
            controller = new LaserController(this);           
        }

        private void parkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controlThread = new Thread(new ThreadStart(controller.Park));
            controlThread.Name = "Laser Lock Control Thread";
            controlThread.Priority = ThreadPriority.Normal;
            controlThread.Start();       
        }

        private delegate void AppendToTextBoxDelegate(string text);
        public void AddToTextBox(String text)
        {
            textBox1.Invoke(new AppendToTextBoxDelegate(textBox1.AppendText), text);
        }
    }
}