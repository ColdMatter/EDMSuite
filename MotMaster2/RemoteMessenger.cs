using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;


namespace MOTMaster2
{
    /// <summary>
    /// Allows other programs to control MOTMaster by sending properly structured JSON messages
    /// </summary>
    public class RemoteMessenger
    {
        private TcpListener listener;
        //TODO Remove dependence on window class to execute sequences
        private MainWindow window;
        private Int32 port;
        private IPAddress localAddr;

        public RemoteMessenger(MainWindow window)
        {
            this.window = window;
            localAddr = IPAddress.Parse("127.0.0.1");
            port = 12000;
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
                        Console.WriteLine("Received service request: " + request);
                        string response = Response(request);
                        Console.WriteLine("Computed response is: " + response + "\n");
                        await writer.WriteLineAsync(response);
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
        public void Close()
        {
    
            listener.Stop();
        }
        private string Response(string request)
        {
            window.Interpreter(request);
            return "";
        }
    }
}
