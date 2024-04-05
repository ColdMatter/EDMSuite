using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace DAQ
{
    public class M2LaserInterface
    {

        private static Dictionary<IPEndPoint, M2LaserInterface> interfaces = new Dictionary<IPEndPoint, M2LaserInterface> { };

        private TCPConnection conn;
        private int transmission_id;
        private bool active = false;
        public bool Active
        {
            get
            {
                return active;
            }
        }

        private IPEndPoint local_ip;
        private IPEndPoint remote_ip;

        private M2LaserInterface(IPEndPoint _local_ip, IPEndPoint _remote_ip)
        {
            local_ip = _local_ip;
            remote_ip = _remote_ip;
            conn = new TCPConnection(local_ip, remote_ip, readData);
            conn.subscribeToInterrupt(()=> { signalConnectionChange(false); });
            AttemptReconnect();
        }

        public void IssueCommand(string op, Dictionary<string, object> parameters, ResponseHandler callback, bool singleResponse)
        {
            if (!active)
            {
                AttemptReconnect();
                return;
            }
            int t_id = transmission_id++;
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "message", new Dictionary<string, object>
                {
                    { "op", op },
                    { "transmission_id", new List<object> {t_id} },
                    { "parameters", parameters }
                }
                }
            };
            if (singleResponse)
                response_handlers.Add(t_id, (Dictionary<string, object> d) =>
                    {
                        callback(d);
                        response_handlers.Remove(t_id);
                    });
            else
                response_handlers.Add(t_id, callback);
            string message = JsonParser.encodeJSON(data);
            conn.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
        }

        public void IssueCommand(string op, Dictionary<string, object> parameters)
        {
            if (!active)
            {
                AttemptReconnect();
                return;
            }
            int t_id = transmission_id++;
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "message", new Dictionary<string, object>
                {
                    { "op", op },
                    { "transmission_id", new List<object> {t_id} },
                    { "parameters", parameters }
                }
                }
            };

            string message = JsonParser.encodeJSON(data);
            conn.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
        }

        private void readData(List<byte> bytes)
        {
            string result = Encoding.ASCII.GetString(bytes.ToArray());
            Dictionary<string, object> data;
            try
            {
                 data = JsonParser.parseJSON(result);
            }
            catch (ArgumentException e)
            {
                return;
            }
            int t_id = (int)((List<object>)((Dictionary<string, object>)data["message"])["transmission_id"])[0];
            if (response_handlers.ContainsKey(t_id))
                response_handlers[t_id](data);
            else
                defaultResponseHandler(data);

        }

        private void AttemptReconnect()
        {

            transmission_id = 0;
            response_handlers.Clear();
            Dictionary<string, object> startLinkData = new Dictionary<string, object>
            {
                { "message", new Dictionary<string, object>
                {
                    { "transmission_id", new List<object> {transmission_id++} },
                    { "op", "start_link" },
                    { "parameters", new Dictionary<string, object>
                    {
                        { "ip_address", local_ip.Address.ToString() }
                    }
                    }
                }
                }
            };
            string startLink = JsonParser.encodeJSON(startLinkData);

            response_handlers.Add(0, (Dictionary<string, object> data) =>
            {
                Dictionary<string, object> message = (Dictionary<string, object>)data["message"];
                Dictionary<string, object> parameters = (Dictionary<string, object>)message["parameters"];
                string op = (string)message["op"];
                if (op == "start_link_reply" && (string)parameters["status"] == "ok")
                    signalConnectionChange(true);
                response_handlers.Remove(0);
            });
            conn.Write(Encoding.ASCII.GetBytes(startLink), 0, startLink.Length);
        }

        private void defaultResponseHandler(Dictionary<string,object> data)
        {

        }

        public delegate void ResponseHandler(Dictionary<string, object> data);
        private Dictionary<int, ResponseHandler> response_handlers = new Dictionary<int, ResponseHandler> { };

        public delegate void ConnectionChangeCallback(bool status);
        private event ConnectionChangeCallback connectionChange;

        private void signalConnectionChange(bool state)
        {
            active = state;
            connectionChange?.Invoke(state);
        }

        public void registerForConnectionChange(ConnectionChangeCallback callback)
        {
            connectionChange += callback;
            callback.Invoke(active);
        }

        public static M2LaserInterface getInterface(IPEndPoint local_ip, IPEndPoint remote_ip)
        {
            // Add checks for a listener existing in an other process
            if (!interfaces.ContainsKey(local_ip))
                interfaces.Add(local_ip, new M2LaserInterface(local_ip, remote_ip));
            return interfaces[local_ip];
        }

    }
}
