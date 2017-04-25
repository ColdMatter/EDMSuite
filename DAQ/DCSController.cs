using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace DAQ.HAL
{
    /// <summary>
    /// A class to communicate messages to control the MSquared Digital Control System (DCS). This module implements a phase-locked loop to control the phase difference between two lasers. It also synthesises a sequence of pulses used in the Navigator experiment.
    /// </summary>
    public class DCSController : ICEBLOCCommunicator
    {
        string ipaddress;
        int port;
        TcpClient client;

        public void Configure(string ipaddress, int port)
        {
            this.ipaddress = ipaddress;
            this.port = port;
        }

        public void Enable()
        {
            try
            {
                this.client.Connect(this.ipaddress, this.port);
            }
            catch (SocketException e)
            {
                throw new Exception("Could Not Connect to the DCS Module. Check ip address and port number.");
            }
        }

        public void Disable()
        {
                this.client.Close();
        }

        //TODO Add a method to save settings if needed
        public void Save()
        {
            throw new NotImplementedException();
        }

        public DCSController(string ipaddress, int port)
        {
            Configure(ipaddress, port);
            this.client = new TcpClient();
        }

        public ICEBLOCMessage SendMessage(ICEBLOCMessage message)
        {
            string json = JsonConvert.SerializeObject(message);
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(json);
            Byte[] reply = new Byte[data.Length];

            Enable();
            NetworkStream stream = this.client.GetStream();

            stream.Write(data, 0, data.Length);
            Int32 bytes = stream.Read(reply, 0, data.Length);
            
            string jsonreply = System.Text.Encoding.ASCII.GetString(reply, 0, bytes);
            
            stream.Close();
            Disable();
            
            return JsonConvert.DeserializeObject<ICEBLOCMessage>(jsonreply);    
        }

        public void StartLink()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["ip_address"] = "192.168.1.1";
            string transmission_id = "[999]";
            string op = "start_link";
            ICEBLOCMessage message = new ICEBLOCMessage(op, transmission_id, parameters);

            ICEBLOCMessage reply = SendMessage(message);

            if (!reply.MessageStatus())
                throw new Exception("Incorrect Start Link Message");
        }
    }
}
