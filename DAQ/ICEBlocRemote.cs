using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Collections;

namespace DAQ.HAL
{
    /// <summary>
    /// ICEBlocRemote provides wrapper around Phase Lock ICE-BLOCS laser system functionality
    /// the websocket communication is supported WebSocketSharp
    /// 
    /// the library is written by Teodor Krastev and it can be used free of charge
    /// all copyrights belong to the author and Imperial College, London, UK
    /// </summary>
    public abstract class ICEBlocRemote
    {
        TcpClient socket;
        NetworkStream stream;
        bool logFlag = true;
        protected string my_ip_address = "192.168.1.23";
        protected byte[] my_byte_ip_address = { 192, 168, 1, 23 };
        protected string M2_ip_address { get; set; }
        protected int M2_ip_port { get; set; }
        string lastMessage = "";
        int transmission_id = 0;

        protected ICEBlocRemote()
        {
            M2_ip_address = "192.168.1.222";
            M2_ip_port = 23232;
        }

        protected ICEBlocRemote(string ip_address)
        {
            my_ip_address = ip_address;
            my_byte_ip_address = ip_address.Split('.').Cast<byte>().ToArray();
        }

        public bool Connected
        {
            get { return (socket == null) ? false : socket.Connected; }
            set
            {
                if (value)
                {
                    if (!Connected) Connect();
                }
                else
                {
                    if (Connected) Disconnect();
                }
            }
        }

        public void Send(string msg)
        {
            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);
                if (logFlag) Console.WriteLine(">> " + msg);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public string Receive()
        {
            try
            {
                // Receive the TcpServer.response.
                // Buffer to store the response bytes.
                Byte[] data = new Byte[256];

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                lastMessage = System.Text.Encoding.ASCII.GetString(data, 0, bytes).Trim();
                if (logFlag) Console.WriteLine("<< " + lastMessage);
                return lastMessage;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
                return "";
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                return "";
            }
        }

        public Dictionary<string, object> ReceiveCustomMessage(string command)
        {
            string msgReport = "";
            while (msgReport == "")
            {
                msgReport = Receive();
            }
            Dictionary<string, object> reportDict = ConvertCustomMessageToDictionary(command, msgReport);
            return reportDict;
        }

        public void Connect()
        {
            //Creates a TCPClient using a local end point.
            IPAddress ipAddress = new IPAddress(my_byte_ip_address);
            //IPAddress[] ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, 0);
            socket = new TcpClient(ipLocalEndPoint);

            //socket = new TcpClient();
            socket.Connect(M2_ip_address, M2_ip_port);
            if (socket.Connected)
                if (logFlag) Console.WriteLine("Connected to PL IceBloc");
            stream = socket.GetStream();
        }

        public void Disconnect()
        {
            stream.Close();
            socket.Close();
            if (logFlag) Console.WriteLine("close connection");
        }

        public bool StartLink()
        {
            Dictionary<string, object> prmsOut = new Dictionary<string, object>();
            prmsOut.Add("ip_address", my_ip_address);
            Dictionary<string, object> prmsIn = GenericCommand("start_link", prmsOut);
            return (prmsIn.Count > 0);
        }

        public bool PingTest()
        {
            Dictionary<string, object> prmsOut = new Dictionary<string, object>();
            prmsOut.Add("text_in", "ABCDEFabcdef");
            Dictionary<string, object> prmsIn = GenericCommand("ping", prmsOut);
            return (prmsIn.Count > 0);
        }

        protected Dictionary<string, object> GenericCommand(string command, Dictionary<string, object> prms, bool report)
        {
            if (!Connected) new Exception("no connection to M2 to be tested");
            transmission_id++;
            string msgOut = @"{""message"":{""transmission_id"":[" + transmission_id.ToString();

            int[] iArr = new int[1]; double[] dArr = new double[1];
            string msg;
            if (prms.Count == 0)
            {
                msg = msgOut + @"],""op"":""" + command + @"""}}";
            }
            else
            {
                Dictionary<string, object> prmsCopy = ConvertToNumericArrays(prms);
                string strPrms = JsonConvert.SerializeObject(prmsCopy);
                msg = msgOut + @"],""op"":""" + command + @""",""parameters"":" + strPrms + "}}";
            }
            // the point of everything is here
            Send(msg);
            string msgIn = Receive();
            string msgReport = "";

            Dictionary<string, object> replyDict = ConvertMessageToDictionary(command, msgIn);
            if (replyDict.ContainsKey("protocol_error"))
            {
                throw new Exception("Failed to send command: " + command);
            }
            if (report)
            {
                while (msgReport == "")
                {
                    msgReport = Receive();
                }
                Dictionary<string, object> reportDict = ConvertMessageToDictionary(command, msgReport);
                return reportDict;
            }
            return replyDict;

        }

        private Dictionary<string, object> ConvertMessageToDictionary(string command, string msgIn)
        {
            Dictionary<string, object> rslt = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgIn);
            JObject j0 = (JObject)rslt["message"];
            bool ok = j0.GetValue("op").ToObject<string>().Equals(command + "_reply");
            //int[] j = j0.GetValue("transmission_id").ToObject<int[]>();
            //ok = ok && j[0].Equals(transmission_id);
            //if (!ok)
            //{
            //    throw new Exception("Reply does not match message.");
            //}
            Dictionary<string, object> final = j0.GetValue("parameters").ToObject<Dictionary<string, object>>();
            Dictionary<string, object> finalCopy = new Dictionary<string, object>(final);
            foreach (string key in final.Keys)
            {
                if (final[key].GetType().Name == "JArray")
                {
                    JArray value = (JArray)final[key];
                    if (value.First.GetType() == typeof(Int32))
                    {
                        finalCopy[key] = (int)value.First;
                    }
                    else if (value.First.GetType() == typeof(Double))
                    {
                        finalCopy[key] = (double)value.First;
                    }
                    else
                    {
                        JToken v = value.First;
                        switch (v.Type)
                        {
                            case JTokenType.Integer:
                                finalCopy[key] = v.ToObject<int>();
                                break;

                            case JTokenType.Float:
                                finalCopy[key] = v.ToObject<float>();
                                break;

                            default:
                                finalCopy[key] = v.ToObject<int>();
                                break;
                        }
                    }

                }
                if (final[key].GetType().Name == "Int32[]")
                {
                    int[] ia = (int[])final[key];
                    finalCopy[key] = ia[0];
                }
                if (final[key].GetType().Name == "Double[]")
                {
                    double[] da = (double[])final[key];
                    finalCopy[key] = da[0];
                }
            }
            return finalCopy;
        }

        private Dictionary<string, object> ConvertCustomMessageToDictionary(string command, string msgIn)
        {
            Dictionary<string, object> rslt = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgIn);
            JObject j0 = (JObject)rslt["message"];
            bool ok = j0.GetValue("op").ToObject<string>().Equals(command);
            int[] j = j0.GetValue("transmission_id").ToObject<int[]>();
            ok = ok && j[0].Equals(transmission_id);
            /*if (!ok)
            {
                //rslt.Clear();
                return rslt;
            }
            */
            Dictionary<string, object> final = j0.GetValue("parameters").ToObject<Dictionary<string, object>>();
            Dictionary<string, object> finalCopy = new Dictionary<string, object>(final);
            foreach (string key in final.Keys)
            {
                if (final[key].GetType().Name == "JArray")
                {
                    JArray value = (JArray)final[key];
                    if (value.First.GetType() == typeof(Int32))
                    {
                        finalCopy[key] = (int)value.First;
                    }
                    else if (value.First.GetType() == typeof(Double))
                    {
                        finalCopy[key] = (double)value.First;
                    }
                    else
                    {
                        JToken v = value.First;
                        switch (v.Type)
                        {
                            case JTokenType.Integer:
                                finalCopy[key] = v.ToObject<int>();
                                break;

                            case JTokenType.Float:
                                finalCopy[key] = v.ToObject<float>();
                                break;

                            default:
                                finalCopy[key] = v.ToObject<int>();
                                break;
                        }
                    }

                }
                if (final[key].GetType().Name == "Int32[]")
                {
                    int[] ia = (int[])final[key];
                    finalCopy[key] = ia[0];
                }
                if (final[key].GetType().Name == "Double[]")
                {
                    double[] da = (double[])final[key];
                    finalCopy[key] = da[0];
                }
            }
            return finalCopy;
        }

        protected Dictionary<string, object> GenericCommand(string command, Dictionary<string, object> prms)
        {
            return GenericCommand(command, prms, false);
        }

        public void AdjustReport(ref Dictionary<string, object> report)
        {
            //This is a temporary fix to add the report value
            report["report"] = report["status"];
        }

        protected Dictionary<string, object> ConvertToNumericArrays(IDictionary prms)
        {
            Dictionary<string, object> prmsCopy = new Dictionary<string, object>();
            foreach (string key in prms.Keys)
            {
                if (prms[key].GetType().Name == "Int32")
                {
                    // iArr[0] = (int)prms[key];
                    prmsCopy[key] = new int[1] { (int)prms[key] };
                    continue;
                }

                if (prms[key].GetType().Name == "Double")
                {
                    // dArr[0] = (double)prms[key];
                    prmsCopy[key] = new double[1] { (double)prms[key] };
                    continue;
                }
                else if (prms[key] is IDictionary)
                {

                    prmsCopy[key] = ConvertToNumericArrays((IDictionary)prms[key]);
                    continue;
                }
                prmsCopy[key] = prms[key];

            }
            return prmsCopy;
        }

    }
}
