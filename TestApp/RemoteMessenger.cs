using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace MOTMaster2
{
    /// <summary>
    /// Allows other programs to control MOTMaster by sending properly structured JSON messages
    /// </summary>
    public class RemoteMessenger
    {
        bool logComm = true;
        private string serverName; // incomming messages 
        private string clientName; // outgoing messages
        private TcpListener listener;
        private Int32 port = 12000;
        private IPAddress localAddr;

        public delegate bool RemoteHandler(string msg);
        public event RemoteHandler Remote;

        protected bool OnRemote(string msg)
        {
            if (Remote != null) return Remote(msg);
            else return false;
        }

        public RemoteMessenger(string serverNameIn = "127.0.0.1", string clientNameOut = "127.0.0.2")
        {
            serverName = serverNameIn;
            clientName = clientNameOut;
            localAddr = IPAddress.Parse(serverName);
            listener = new TcpListener(localAddr, port);
        }

        public async Task Run()
        {
            listener.Start();
            Console.WriteLine(string.Format("Listening on port {0}", port));
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Task t = Process(client);
                await t;
                client.Close();
            }
        }

        private async Task Process(TcpClient tcpClient)
        {
            string clientEndPoint =
            tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Received connection request from "
              + clientEndPoint);
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
                while (true)
                {
                    string request = await reader.ReadLineAsync();
                    if (request != null)
                    {
                        if (logComm) Console.WriteLine("Received service request: " + request + "\n");
                        bool response = Response(request);
                        //await writer.WriteLineAsync(response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        }

        private bool Response(string request)
        {
           return OnRemote(request);
        }

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private static async Task<string> SendRequest(string server, int port, string msg)
        {
            try
            {
                /* IPAddress ipAddress = null;
                 IPHostEntry ipHostInfo = Dns.GetHostEntry(server);
                 for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
                 {
                     if (ipHostInfo.AddressList[i].AddressFamily ==
                       AddressFamily.InterNetwork)
                     {
                         ipAddress = ipHostInfo.AddressList[i];
                         break;
                     }
                 }
                 if (ipAddress == null)
                     throw new Exception("No IPv4 address for server"); */
                TcpClient client = new TcpClient();
                await client.ConnectAsync(server, port); // Connect ipAddress
                NetworkStream networkStream = client.GetStream();
                StreamWriter writer = new StreamWriter(networkStream);
                StreamReader reader = new StreamReader(networkStream);
                writer.AutoFlush = true;
                await writer.WriteLineAsync(msg);
                //string response = await reader.ReadLineAsync();
                client.Close();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async void Send(string msg)
        {
            string sResponse = await SendRequest(clientName, port, msg);
            if (logComm) Console.WriteLine("Computed sent out: " + msg + "\n");
            if (!sResponse.Equals("OK")) Console.WriteLine("Error sending a message" + sResponse + "\n");
        }

        public void Close()
        {
            listener.Stop();
        }

    }
}
