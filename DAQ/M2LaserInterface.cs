using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Collections;
using System.Net.Sockets;
using System.Net.Http;
using System.IO;

namespace DAQ
{

    public class M2LaserInterface : MarshalByRefObject
    {

        public override Object InitializeLifetimeService()
        {
            return null;
        }

        private class M2LaserInterfaceServer : MarshalByRefObject
        {
            public M2LaserInterfaceServer()
            {

            }

            public override Object InitializeLifetimeService()
            {
                return null;
            }

            public M2LaserInterface getLaserInterface(IPEndPoint local_ip, IPEndPoint remote_ip)
            {
                return M2LaserInterface.getInterfaceInternal(local_ip, remote_ip);
            }
        }

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
            conn.subscribeToInterrupt(() => { signalConnectionChange(false); });
            if (AUTH == null && File.Exists(".\\M2Auth.txt"))
            {
                AUTH = File.ReadAllText(".\\M2Auth.txt");
                wlchange_format = JsonParser.parseJSON(File.ReadAllText(@".\M2WlChange.json"));
            }

            http_address = String.Format("http://{0}/", remote_ip.Address.ToString());
            httpclient = new HttpClient();
            httpclient.BaseAddress = new Uri(http_address);
            httpclient.DefaultRequestHeaders.Add("Authorization", "BASIC " + AUTH);

            AttemptReconnect();
        }

        #region TCP_JSON_functions

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

        public Task<Dictionary<string, object>> IssueCommandAsync(string op, Dictionary<string, object> parameters)
        {
            Dictionary<string, object> ret = null;
            AutoResetEvent callbackEvent = new AutoResetEvent(false);
            CancellationTokenSource cancel_source = new CancellationTokenSource();
            CancellationToken cancel_token = cancel_source.Token;
            ConnectionChangeCallback cancel_token_callback = (bool status) =>
            {
                if (status) return;
                cancel_source.Cancel();
            };

            registerForConnectionChange(cancel_token_callback);

            cancel_token.Register(() =>
            {
                deregisterForConnectionChange(cancel_token_callback);
            });

            cancel_token.Register(() =>
            {
                ret = new Dictionary<string, object>
                {
                    { "message", new Dictionary<string, object>
                    {
                        { "op", "error" }
                    }
                    }
                };
                callbackEvent.Set();
            });

            return new Task<Dictionary<string, object>>(() =>
            {
                IssueCommand(op, parameters, (Dictionary<string, object> data) =>
                {
                    ret = data;
                    callbackEvent.Set();
                }, true);
                if (!active) return ret;
                callbackEvent.WaitOne();
                return ret;
            });
        }

        public Dictionary<string, object> IssueCommandSync(string op, Dictionary<string, object> parameters)
        {
            Dictionary<string, object> ret = null;
            AutoResetEvent callbackEvent = new AutoResetEvent(false);
            CancellationTokenSource cancel_source = new CancellationTokenSource();
            CancellationToken cancel_token = cancel_source.Token;
            ConnectionChangeCallback cancel_token_callback = (bool status) =>
            {
                if (status) return;
                cancel_source.Cancel();
            };

            cancel_token.Register(() =>
            {
                deregisterForConnectionChange(cancel_token_callback);
            });

            cancel_token.Register(() =>
            {
                ret = new Dictionary<string, object>
                {
                    { "message", new Dictionary<string, object>
                    {
                        { "op", "error" }
                    }
                    }
                };
                callbackEvent.Set();
            });

            IssueCommand(op, parameters, (Dictionary<string, object> data) =>
            {
                ret = data;
                callbackEvent.Set();
            }, true);
            if (!active) return ret;
            callbackEvent.WaitOne();
            return ret;
        }

        private void readData(List<byte> bytes)
        {
            string result = Encoding.ASCII.GetString(bytes.ToArray());
            Dictionary<string, object> data;
            try
            {
                data = JsonParser.parseJSON(result);
            }
            catch (ArgumentException)
            {
                return;
            }
            int t_id = (int)((List<object>)((Dictionary<string, object>)data["message"])["transmission_id"])[0];
            string op = (string)((Dictionary<string, object>)data["message"])["op"];

            if (response_callbacks.ContainsKey(op))
                response_callbacks[op]?.Invoke(data);

            if (response_handlers.ContainsKey(t_id))
                response_handlers[t_id].Invoke(data);
            else
                defaultResponseHandler(data);

        }

        public void RegisterCallback(string op, ResponseHandler handler)
        {
            if (response_callbacks.ContainsKey(op))
                response_callbacks[op] += handler;
            else
                response_callbacks[op] = handler;
        }

        public void DeRegisterCallback(string op, ResponseHandler handler)
        {
            if (response_callbacks.ContainsKey(op))
                response_callbacks[op] -= handler;
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

        private void defaultResponseHandler(Dictionary<string, object> data)
        {

        }

        public delegate void ResponseHandler(Dictionary<string, object> data);
        private Dictionary<int, ResponseHandler> response_handlers = new Dictionary<int, ResponseHandler> { };
        private Dictionary<string, ResponseHandler> response_callbacks = new Dictionary<string, ResponseHandler> { };

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

        public void deregisterForConnectionChange(ConnectionChangeCallback callback)
        {
            connectionChange -= callback;
        }

        private static M2LaserInterfaceServer server;
        public static M2LaserInterface getInterface(IPEndPoint local_ip, IPEndPoint remote_ip)
        {
            if (server != null)
                return server.getLaserInterface(local_ip, remote_ip);

            try
            {

                // ------------------------------------------------------------
                BinaryServerFormatterSinkProvider serverProv =
                    new BinaryServerFormatterSinkProvider();
                serverProv.TypeFilterLevel = TypeFilterLevel.Full;
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;

                serverProv.TypeFilterLevel = TypeFilterLevel.Full;
                IDictionary propBag = new Hashtable();
                propBag["port"] = 1275;
                propBag["typeFilterLevel"] = TypeFilterLevel.Full;
                propBag["name"] = "M2LaserServerChannel";  // here enter unique channel name
                // -----------------------------------------
                TcpChannel tcpChannel = new TcpChannel(propBag, null, serverProv);
                ChannelServices.RegisterChannel(tcpChannel, false);

                server = new M2LaserInterfaceServer();
                RemotingServices.Marshal(server, "server.rem");
            }
            catch (SocketException)
            {
                server = (M2LaserInterfaceServer)Activator.GetObject(typeof(M2LaserInterfaceServer), "tcp://localhost:1275/server.rem");
            }
            return server.getLaserInterface(local_ip, remote_ip);
        }

        public static M2LaserInterface getRemoteInterface(string server, IPEndPoint local_ip, IPEndPoint remote_ip)
        {
            return ((M2LaserInterfaceServer)Activator.GetObject(typeof(M2LaserInterfaceServer), "tcp://" + server + ":1275/server.rem")).getLaserInterface(local_ip, remote_ip);
        }

        public static M2LaserInterface getRemoteInterface(string server, string local_ip, string remote_ip)
        {
            byte[] local_addr = local_ip.Split(':')[0].Split('.').AsEnumerable().Select(i => Convert.ToByte(i)).ToArray();
            int local_port = Convert.ToInt32(local_ip.Split(':')[1]);

            byte[] remote_addr = remote_ip.Split(':')[0].Split('.').AsEnumerable().Select(i => Convert.ToByte(i)).ToArray();
            int remote_port = Convert.ToInt32(remote_ip.Split(':')[1]);

            return getRemoteInterface(server, new IPEndPoint(new IPAddress(local_addr), local_port), new IPEndPoint(new IPAddress(remote_addr), remote_port));

        }

        private static M2LaserInterface getInterfaceInternal(IPEndPoint local_ip, IPEndPoint remote_ip)
        {

            if (!interfaces.ContainsKey(local_ip))
                interfaces.Add(local_ip, new M2LaserInterface(local_ip, remote_ip));
            return interfaces[local_ip];
        }

        public static M2LaserInterface getInterface(string local_ip, string remote_ip)
        {
            byte[] local_addr = local_ip.Split(':')[0].Split('.').AsEnumerable().Select(i => Convert.ToByte(i)).ToArray();
            int local_port = Convert.ToInt32(local_ip.Split(':')[1]);

            byte[] remote_addr = remote_ip.Split(':')[0].Split('.').AsEnumerable().Select(i => Convert.ToByte(i)).ToArray();
            int remote_port = Convert.ToInt32(remote_ip.Split(':')[1]);

            return getInterface(new IPEndPoint(new IPAddress(local_addr), local_port), new IPEndPoint(new IPAddress(remote_addr), remote_port));

        }

        #endregion

        #region Auxiliary_HTTP_commands

        private string AUTH; // B64 encoded user:pass string. For security this is from a local file
        private HttpClient httpclient;
        private string http_address;
        private Dictionary<string, object> wlchange_format;

        public void specifyHTTPCreds(string _AUTH, Dictionary<string, object> _wlchange_format)
        {
            AUTH = _AUTH;
            wlchange_format = _wlchange_format;
        }

        /*        public Dictionary<string, object> IssuePostRequest(string subdirectory, Dictionary<string, object> postData)
                {

                    StringContent content = new StringContent(JsonParser.encodeJSON(postData),Encoding.ASCII);
                    content.Headers.Add("Authorization", AUTH);
                    Task<HttpResponseMessage> response = httpclient.PostAsync(http_address + subdirectory, content);

                    return (new Task<Dictionary<string, object>>(() => { return JsonParser.parseJSON((await response).Content.ToString())});

                }*/

        public double get_wavelength()
        {
            StringContent content = new StringContent("{'}", Encoding.ASCII);
            Task<HttpResponseMessage> response = httpclient.PostAsync(http_address + "UE/control_page_update.txt", content);
            try
            {
                response.Wait();
            }
            catch (Exception)
            {
                return 0;
            }
            if (response.Status == TaskStatus.Faulted)
                return 0;
            Task<string> result = response.Result.Content.ReadAsStringAsync();
            try
            {
                result.Wait();
            }
            catch (Exception)
            {
                return 0;
            }

            if (response.Status == TaskStatus.Faulted)
                return 0;
            string res = result.Result.Split('\0')[0];
            try
            {
                return Convert.ToDouble(((Dictionary<string, object>)JsonParser.parseJSON(result.Result)["left_panel"])["wlm_wavelength"]);
            }
            catch (ArgumentException)
            {
                return 0;
            }
        }

        public void set_wavelength(double wl)
        {
            lock (wlchange_format)
            {
                if (wl < 0) return;
                wlchange_format["target_wavelength"] = wl;
                StringContent content = new StringContent(JsonParser.encodeJSON(wlchange_format), Encoding.ASCII);
                httpclient.PostAsync(http_address + "UE/control_page_update.txt", content);
            }
        }

        #endregion

    }
}
