using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WavemeterViewer
{
    public partial class ViewerForm : Form
    {
        public WavemeterLockServer.Controller controller;
        public string hostName;

        public ViewerForm(WavemeterLockServer.Controller _controller, string _hostName)
        {
            InitializeComponent();
            controller = _controller;
            hostName = _hostName;
            updateDisplayUnits();
        }

        private void ViewerForm_Load(object sender, EventArgs e)
        {
            labelHostName.Text = hostName;
        }

        private void Timer_tick(object sender, EventArgs e)
        {
            updatePannel();
        }

        private bool displayFrequency;

        private void updateDisplayUnits()
        {
            if (displayFreqRadioButton.Checked)
            {
                displayFrequency = true;
            }
            else
            {
                displayFrequency = false;
            }
        }

        public void updatePannel()
        {
            string[] s = new string[8];
            double[] w = new double[8];

            for (int n = 0; n < 8; n++)
            {
<<<<<<< HEAD
                s[n] = controller.displayFrequency(n + 1);
=======
                if (displayFrequency)
                {
                    s[n] = controller.displayFrequency(n + 1);
                }
                else
                {
                    s[n] = controller.displayWavelength(n + 1);
                }
                
>>>>>>> c42f6cbe2bff64af1407db9f913dee1644e9a804
            }

            //Shows the wavelength of each channel
            SetTextField(label1, s[0]);
            SetTextField(label2, s[1]);
            SetTextField(label3, s[2]);
            SetTextField(label4, s[3]);
            SetTextField(label5, s[4]);
            SetTextField(label6, s[5]);
            SetTextField(label7, s[6]);
            SetTextField(label8, s[7]);


            

           
        }

        public void SetTextField(Control box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }

        private delegate void SetTextDelegate(Control box, string text);

        private void SetTextHelper(Control box, string text)
        {
            box.Text = text;
        }

        private void LockForm_Closing(object sender, FormClosingEventArgs e)
        {
            
            controller.removeWavemeterViewer(Environment.MachineName);
            Application.Exit();
        }

        private void displayWavRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            updateDisplayUnits();
        }

        private void displayFreqRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            updateDisplayUnits();
        }
    }
}
