using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// This class implements an asynchronous connection to each computer on board the muquans laser to control various parameters such as the offset locks, edfa power and monitoring the laser. It also launches the executable used for real-time control of the DDS
    /// </summary>
    public class MuquansCommunicator
    {
        //Define the IP address for each computer
        private static string slaveIPAddress = "192.168.1.118";
        private static string edfaIPAddress = "192.168.1.75";
        private static string ddsSlaveIPAddress = "192.168.1.125";
        private static string ddsAOMIPAddress = "192.168.1.126";

        public ASynchronousClient slaveConn = new ASynchronousClient();
        public ASynchronousClient edfaConn = new ASynchronousClient();
        public ASynchronousClient ddsSlaveConn = new ASynchronousClient();
        public ASynchronousClient ddsAOMConn = new ASynchronousClient();

        public Process slaveDDS = new Process();
        public Process aomDDS = new Process();
      
        
        public void Start()
        {
            slaveConn.StartClient(slaveIPAddress);
            edfaConn.StartClient(edfaIPAddress);
            ddsSlaveConn.StartClient(ddsSlaveIPAddress);
            ddsAOMConn.StartClient(ddsAOMIPAddress);
            
        }
        public void ConfigureDDS(string id, int port)
        {
            ///<summary>
            ///Configures the starting parameters for a dds process
            ///id - The identifier for the DDS. This is either "slave" or "aom"
            ///port - The port number used to communicate to the DDS. The default values are 18 and 20
            /// </summary>
            
            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = "ukus_dds_"+id+"_conf.txt comm "+ port;
            info.FileName = "serial_to_dds_gw.exe";
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;
    
        }
        public void Close()
        {
            slaveConn.CloseClient();
            edfaConn.CloseClient();
            ddsSlaveConn.CloseClient();
            ddsAOMConn.CloseClient();
        }

       public void LockLaser(string laser)
        {
            string msg = "ukus autolock_" + laser;
            slaveConn.Send(msg);
            slaveConn.Receive();
        }
        public void ScanMasterLaser()
        {
            string msg = "ukus autolock_scan_only";
            slaveConn.Send(msg);
            slaveConn.Receive();
        }

        public void UnlockLaser(string laser)
        {
            string msg = "ukus unlock_" + laser;
            slaveConn.Send(msg);
        }
        public void StartEDFA(string edfa)
        {
            string msg = "driver_edfa_tool ";
        }
    }

    /// <summary>
    /// This is the class used to handle communications for each Muquans computer
    /// </summary>
    public class ASynchronousClient
    {
        // The port number for the remote device.
        private const int port = 23;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private static String response = String.Empty;

        private static Socket client;

        public void StartClient(string ipString)
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // Using the given ipaddress.
                IPHostEntry ipHostInfo = Dns.GetHostEntry(ipString);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.
                Socket client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void CloseClient()
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        public static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Receive()
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public string Response()
        {
            return response;
        }

    }
    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 16684;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}
