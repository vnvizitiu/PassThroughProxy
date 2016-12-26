﻿using System;
using System.Net.Sockets;
using Proxy.Core.Headers;
using Proxy.Core.Network;

namespace Proxy.Core.Sessions
{
    public class SessionContext : IDisposable
    {
        public SessionContext(TcpClient client)
        {
            Client = client;
            ClientStream = client.GetStream();
        }

        public HttpHeader Header { get; set; }

        public Address CurrentHostAddress { get; set; }

        public TcpClient Client { get; private set; }

        public NetworkStream ClientStream { get; private set; }

        public TcpClient Host { get; private set; }

        public NetworkStream HostStream { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void AddHost(TcpClient client)
        {
            Host = client;
            HostStream = client.GetStream();
        }

        public void RemoveHost()
        {
            using (HostStream)
            using (Host)
            {
            }

            HostStream = null;
            Host = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ClientStream != null)
                {
                    using (ClientStream)
                    {
                    }
                    ClientStream = null;
                }

                if (Client != null)
                {
                    using (Client)
                    {
                    }
                    Client = null;
                }

                if (HostStream != null)
                {
                    using (HostStream)
                    {
                    }
                    HostStream = null;
                }

                if (Host != null)
                {
                    using (Host)
                    {
                    }
                    Host = null;
                }
            }
        }
    }
}