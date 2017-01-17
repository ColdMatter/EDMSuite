using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using DAQ.Environment;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// This class acts as an iterface to control Cicero from the Navigator hardware controller
    /// </summary>
    public class CiceroController
    {

        public Controller controller;
        public bool ciceroConnected;
        public List<int> listNums;
        public CiceroController()
        {
            listNums = new List<int>();
            for (int i=1;i<11;i++)
            {
                listNums.Add(i);
            }
        }

        public void ConnectToCicero()
        {
            try
            {
            ciceroConnected = true;
            //if (RemotingServices.IsTransparentProxy(sequenceData))
            //    ciceroConnected = false;
            List<string> names = controller.hardwareState.analogs.Keys.ToList();
           
             
             
            }
            catch (SocketException e)
            {
                ciceroConnected = false;
                Console.WriteLine("Couldn't connect to cicero: " + e.Message);
            }
        }

        public void SendVariablesToCicero()
        {
            if (ciceroConnected)
            {
             
            }
            else
            {
                Console.WriteLine("Cicero not connected. Couldn't remotely update.");
            }
        }

  

    }
}
