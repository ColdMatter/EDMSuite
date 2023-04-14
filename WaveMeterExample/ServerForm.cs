using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using System.ComponentModel;
using wlmData; // a namespace with a "WLM" class covering the header of the wlmData.dll since headers and includes are not supported by C#



namespace WavemeterLockServer
{

    public partial class ServerForm : Form
    {

        public Controller controller;
        public ServerForm(Controller _controller)
        {
            InitializeComponent();
            controller = _controller;
            controller.measurementAcquired += () => { updatePannel(); };
            //controller.measurementAcquired += () => { controller.indicateNewMeasurement(); };
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            controller.startServer();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            controller.startMeasure();
        }

        public void addStringToListBox(string clientName)
        {
            listBox1.Items.Add(clientName);
        }

        public void deleteStringToListBox(string clientName)
        {
            listBox1.Items.Remove(clientName);
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

        public void UpdateRenderedObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            obj.Invoke(new UpdateObjectDelegate<T>(UpdateObject), new object[] { obj, updateFunc });
        }

        private delegate void UpdateObjectDelegate<T>(T obj, Action<T> updateFunc) where T : Control;

        private void UpdateObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            updateFunc(obj);
        }

        public void updatePannel()
        {
            string[] s = new string[8];
            double[] w = new double[8];

            for (int n = 0; n < 8; n++)
            {
                s[n] = controller.displayWavelength(n + 1);
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


            if (WLM.GetOperationState(0) == 2)
                controller.bMeas = true;
            else controller.bMeas = false;

            // check whether wavemeter is running and apply the text of the Open/Close button
            controller.bAvail = (WLM.Instantiate(WLM.cInstCheckForWLM, 0, 0, 0) > 0);
            if (controller.bAvail)
            {
                SetTextField(btnOpen, "Close Server");
                //btnOpen.Text = "Close Server";
                UpdateRenderedObject(btnStart, (Button but) => { but.Enabled = true; });
                //btnStart.Enabled = true;
            }
            else
            {
                SetTextField(btnOpen, "Open Server");
                //btnOpen.Text = "Open Server";
                UpdateRenderedObject(btnStart, (Button but) => { but.Enabled = false; });
                //btnStart.Enabled = false;
            }

            // check whether measurement is running and apply the text of the Start/Stop button
            controller.bMeas = (WLM.GetOperationState(0) != 0);
            if (controller.bMeas)
            {
                SetTextField(btnStart, "Stop Measurement");
                //btnStart.Text = "Stop Measurement";
            }
            else
            {
                SetTextField(btnStart, "Start Measurement");
                //btnStart.Text = "Start Measurement";
            }


            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //updatePannel();

            //Check if there's any remote connection
            if (controller.remoteConnection[0])
                led1.Value = true;
            else
                led1.Value = false;

            if (controller.remoteConnection[1])
                led2.Value = true;
            else
                led2.Value = false;

            if (controller.remoteConnection[2])
                led3.Value = true;
            else
                led3.Value = false;

            if (controller.remoteConnection[3])
                led4.Value = true;
            else
                led4.Value = false;

            if (controller.remoteConnection[4])
                led5.Value = true;
            else
                led5.Value = false;

            if (controller.remoteConnection[5])
                led6.Value = true;
            else
                led6.Value = false;

            if (controller.remoteConnection[6])
                led7.Value = true;
            else
                led7.Value = false;

            if (controller.remoteConnection[7])
                led8.Value = true;
            else
                led8.Value = false;

        }

        private void ServerForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (controller.measurementStatus.Count != 0)
            {

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

        private void test_Click(object sender, EventArgs e)
        {

        }

        private void led2_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e)
        {

        }

        private void led3_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
