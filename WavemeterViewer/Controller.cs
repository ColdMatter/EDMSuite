using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.Environment;
using System.Net;
using System.Net.Sockets;

namespace WavemeterViewer
{
    class Controller
    {

        private int hostTCPChannel;
        private string hostComputerName;
        private ViewerForm ui;
        private WavemeterLockServer.Controller wavemeterContrller;
        private string name;
        private string thisComputerName;

        public Controller(string hostName, int channelNumber)
        {
            hostComputerName = hostName;
            hostTCPChannel = channelNumber;
            initializeTCPChannel();
        }

        public void start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ui = new ViewerForm(wavemeterContrller, hostComputerName);
            Application.Run(ui);
        }

        public void initializeTCPChannel()
        {
            
            foreach (var addr in Dns.GetHostEntry(hostComputerName).AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    name = addr.ToString();
                }
            }

            try
            {
                wavemeterContrller = (WavemeterLockServer.Controller)(Activator.GetObject(typeof(WavemeterLockServer.Controller), "tcp://" + name + ":" + hostTCPChannel.ToString() + "/controller.rem"));
            }

            catch (Exception e)
            {
                connectionError(e);
            }
            //Register in remote server
            thisComputerName = Environment.MachineName.ToString();

            try
            {
                wavemeterContrller.registerWavemeterViewer(thisComputerName);
            }
            catch (Exception e)
            {
                connectionError(e);
            }
        }

        public void connectionError(Exception e)
        {
            MessageBox.Show($"Connection failed: {e.Message}");
            if (Application.MessageLoop)
            {
                // WinForms app
                Application.Exit();
            }
            else
            {
                // Console app
                Environment.Exit(1);
            }
        }
    }
}
