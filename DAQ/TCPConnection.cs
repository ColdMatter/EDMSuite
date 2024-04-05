﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace DAQ
{
    public class TCPConnection
    {
        private TcpClient client;
        private NetworkStream stream;
        private IPEndPoint remote_ip;

        public TCPConnection(IPEndPoint local_ip, IPEndPoint _remote_ip, TCPDataCallback callback)
        {
            remote_ip = _remote_ip;
            client = new TcpClient(local_ip);
            dataReceived += callback;
            Reconnect();
        }

        private void ReconnectAsync()
        {
            try
            {
                client.Connect(remote_ip);
                stream = client.GetStream();
                stream.BeginRead(fake_buffer, 0, 1, readData, null);
            }
            catch (SocketException e)
            {
                ConnectionInterrupted?.Invoke();
            }
            lock (this)
            {
                reconnecting = false;
            }
        }

        public void Reconnect()
        {
            lock (this)
            {
                if (reconnecting) return;
                reconnecting = true;
                (new Thread(new ThreadStart(ReconnectAsync))).Start();
            }
        }

        private byte[] fake_buffer = new byte[1];
        private List<byte> input_buffer = new List<byte> { };

        private void readData(IAsyncResult res)
        {
            try
            {
                stream.EndRead(res);
                input_buffer.Add(fake_buffer[0]);
                if (stream.DataAvailable)
                {
                    return;
                }
                dataReceived?.Invoke(input_buffer);
                input_buffer = new List<byte> { };
            }
            catch (System.IO.IOException)
            {

            }
            finally
            {
                if (client.Connected)
                    try
                    {
                        stream.BeginRead(fake_buffer, 0, 1, readData, null);
                    }
                    catch (System.IO.IOException)
                    {

                    }
        }
        }

        private bool reconnecting = false;

        public void Write(byte[] data, int offset, int size)
        {
            if (stream == null)
            {
                Reconnect();
                return;
            }
            try
            {
                stream.Write(data, offset, size);
            }
            catch (System.IO.IOException)
            {
                ConnectionInterrupted?.Invoke();
            }
        }

        public delegate void TCPDataCallback(List<byte> result);
        private event TCPDataCallback dataReceived;

        private event Action ConnectionInterrupted;
        public void subscribeToInterrupt(Action callback)
        {
            ConnectionInterrupted += callback;
            if (!client.Connected) callback();
        }

    }
}
