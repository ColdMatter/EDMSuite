using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DAQ.HAL
{
    /// <summary>
    /// A base class to communicate with an MSquared ICE-BLOC control module.
    /// </summary>
    public class ICEBLOCCommunicator
    {
        TcpClient client;
        string ipaddress;
        int port;

        public ICEBLOCCommunicator(string ipaddress, int port)
        {
            Configure(ipaddress, port);
            this.client = new TcpClient();
            Enable();
        }
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
                throw new Exception("Could Not Connect to the DCS Module. Check ip address and port number."+e.Message);
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

        public ICEBLOCMessage SendMessage(ICEBLOCMessage message)
        {
            string json = CreateJsonMessage(message);
            string jsonreply = string.Empty;
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(json);
            NetworkStream stream = this.client.GetStream();
            Byte[] reply = new Byte[1024];
            stream.Write(data, 0, data.Length);
            stream.Read(reply, 0, reply.Length);
            jsonreply = Encoding.ASCII.GetString(reply, 0, reply.Length);
          //  Disable();

            return CreateICEBLOCMessage(jsonreply);
        }

        public string CreateJsonMessage(ICEBLOCMessage message)
        {
            Dictionary<string, ICEBLOCMessage> messageDict = new Dictionary<string, ICEBLOCMessage>();
            messageDict["message"] = message;
            return JsonConvert.SerializeObject(messageDict,Formatting.None);
            
        }

        public ICEBLOCMessage CreateICEBLOCMessage(string json)
        {
           // Dictionary<string,object> rawMessage =  JsonConvert.DeserializeObject<ICEBLOCMessage>(json);
            Dictionary<string, ICEBLOCMessage> rawMessage = JsonConvert.DeserializeObject<Dictionary<string,ICEBLOCMessage>>(json);
            return rawMessage["message"];
        }
    }

    [Serializable,JsonObject]
    public class ICEBLOCMessage
    {
        public string op { get; set; }
        public int[] transmission_id { get; set; }
        public Dictionary<String, Object> parameters { get; set; }

        public ICEBLOCMessage(string op, int[] transmission_id,Dictionary<string,object> parameters)
        {
            this.op = op;
            this.transmission_id = transmission_id;
            this.parameters = parameters;
        }
        /// <summary>
        /// Checks a reply message to see if it was correctly received
        /// </summary>
        /// <returns></returns>
        public bool MessageStatus()
        {
            if (this.parameters.ContainsKey("status"))
             {
                if ((string)this.parameters["status"] == "ok")
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

    }
}
