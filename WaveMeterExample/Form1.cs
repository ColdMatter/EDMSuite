using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using wlmData; // a namespace with a "WLM" class covering the header of the wlmData.dll since headers and includes are not supported by C#



namespace WavemeterLockServer
{

    public partial class Form1 : Form
    {

        public Controller controller;

        
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            controller.startServer();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            controller.startMeasure();
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            
            string[] s = new string[8];
            double[] w = new double[8];

            for (int n = 0; n < 8; n++)
            {
                s[n] = controller.displayWavelength(n+1);
            }
            
           label1.Text = s[0];
           label2.Text = s[1];
           label3.Text = s[2];
           label4.Text = s[3];
           label5.Text = s[4];
           label6.Text = s[5];
           label7.Text = s[6];
           label8.Text = s[7];
           label10.Text = "Switch Mode: " + WLM.GetOperationState(0).ToString();

            // check whether server is available and apply the text of the Open/Close button
            if (controller.bAvail)
            {
                btnOpen.Text = "Close Server";
                btnStart.Enabled = true;
            }
            else
            {
                btnOpen.Text = "Open Server";
                btnStart.Enabled = false;
            }

            // check whether measurement is running and apply the text of the Start/Stop button
            controller.bMeas = (WLM.GetOperationState(0) != 0);
            if (controller.bMeas)
            {
                btnStart.Text = "Stop Measurement";
            }
            else
            {
                btnStart.Text = "Start Measurement";
            }


        }

        


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void groupBox9_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
